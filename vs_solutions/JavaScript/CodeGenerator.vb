Option Strict On
Option Explicit On

Imports System.Globalization
Imports System.Text

''' <summary>
''' Translates a JavaScript AST into compilable VB.NET source code.
''' <para>
''' Strategy: every JS function becomes a multi-line <c>Function</c> lambda
''' of type <c>JsFunc</c> (a real VB closure — JS closures and recursion map
''' 1:1); loop shapes map onto Do/While/For Each so <c>Continue</c> still runs
''' the for-update (like JS <c>continue</c>); every dynamic operation calls
''' <see cref="Runtime.JsRuntime"/> — the same semantics module the
''' interpreter uses. Shadowing is resolved by deterministic renaming.
''' </para>
''' </summary>
Public NotInheritable Class CodeGenerator

    Private _temp As Integer = 0
    Private _label As Integer = 0
    ' what 'break'/'continue' compile to in the innermost loop being generated
    Private ReadOnly _loopExit As New Stack(Of String)
    Private ReadOnly _loopCont As New Stack(Of String)
    ' GoTo targets of labeled statements currently in scope (labeled break/continue)
    Private ReadOnly _labeledTargets As New Dictionary(Of String, LabelInfo)
    ' promise variable of the innermost async function being generated ('return' fulfils it)
    Private ReadOnly _asyncPromise As New Stack(Of String)
    ' yield collector of the innermost generator being generated
    Private ReadOnly _yieldCollector As New Stack(Of String)

    Public Function Generate(program As Program, Optional className As String = "GeneratedScript") As String
        _temp = 0
        _label = 0
        _loopExit.Clear() : _loopCont.Clear()
        Dim w As New IndentWriter()
        Dim top = Scope.NewRoot()

        w.Line("' Auto-generated from JavaScript by LiteJs — do not edit.")
        w.Line("' All dynamic semantics come from the LiteJs runtime (shared with the interpreter).")
        w.Line("Option Strict On")
        w.Line("Option Explicit On")
        w.Line("Option Infer On")
        w.Line()
        w.Line("Imports System")
        ' the runtime namespace is imported twice: once plainly (bare JsRuntime calls)
        ' and once under an alias the generated type references use
        w.Line("Imports Microsoft.VisualBasic.ApplicationServices.VM.JavaScript.Runtime")
        w.Line("Imports LiteJsRuntime = Microsoft.VisualBasic.ApplicationServices.VM.JavaScript.Runtime")
        w.Line("Imports JsValue = System.Object")
        w.Line("' function values are Func(Of Object(), Object); VB Imports aliases cannot target generic types")
        w.Line()
        w.Line($"Public NotInheritable Class {className}")
        w.Indent()
        w.Line()
        w.Line("Public Shared Sub Run(io As LiteJsRuntime.ScriptIO)")
        w.Indent()
        w.Line("JsRuntime.Io = io")
        ' JS var hoisting: every 'var' of this function body is declared once at
        ' the top (VB Dim is block-scoped, JS var is function-scoped)
        EmitHoistedVars(program.Body, w, top)
        GenBlock(program.Body, w, top)
        ' promise reactions queued by the body are microtasks — run them now
        w.Line("JsRuntime.DrainMicrotasks()")
        w.UnIndent()
        w.Line("End Sub")
        w.UnIndent()
        w.Line("End Class")
        Return w.ToString()
    End Function

    ' ======================================================= statements

    Private Sub GenBlock(stmts As List(Of Statement), w As IndentWriter, parent As Scope)
        GenBlockInScope(stmts, w, parent.Push())
    End Sub

    ''' <summary>Emit statements; the caller decides whether to push a new JS block scope.</summary>
    Private Sub GenBlockInScope(stmts As List(Of Statement), w As IndentWriter, scope As Scope)
        ' function hoisting: FunctionDecls of the block are emitted first
        For Each s In stmts
            If TypeOf s Is FuncDeclStmt Then GenFunctionDecl(DirectCast(s, FuncDeclStmt), w, scope)
        Next
        For Each s In stmts
            If TypeOf s Is FuncDeclStmt Then Continue For
            GenStmt(s, w, scope)
        Next
    End Sub

    ''' <summary>A statement-shaped body (block or single statement).</summary>
    Private Sub GenBody(body As Statement, w As IndentWriter, scope As Scope)
        If TypeOf body Is BlockStmt Then
            GenBlock(DirectCast(body, BlockStmt).Body, w, scope)
        Else
            GenStmt(body, w, scope)
        End If
    End Sub

    Private Sub GenStmt(s As Statement, w As IndentWriter, scope As Scope)
        If TypeOf s Is VarStmt Then
            Dim v = DirectCast(s, VarStmt)
            For Each d In v.Declarations
                ' let / const declare here; var names were hoisted by EmitHoistedVars
                Dim init = If(d.Init IsNot Nothing, GenExpr(d.Init, scope), "JsRuntime.Undef")
                GenBindTarget(d.Target, init, w, scope, v.Kind, isDeclaration:=True)
            Next

        ElseIf TypeOf s Is ExprStmt Then
            GenExprAsStatement(DirectCast(s, ExprStmt).Expr, w, scope)

        ElseIf TypeOf s Is BlockStmt Then
            GenBlock(DirectCast(s, BlockStmt).Body, w, scope)

        ElseIf TypeOf s Is IfStmt Then
            GenIf(DirectCast(s, IfStmt), w, scope)

        ElseIf TypeOf s Is WhileStmt Then
            GenWhileLoop(DirectCast(s, WhileStmt), w, scope)

        ElseIf TypeOf s Is DoWhileStmt Then
            GenDoWhileLoop(DirectCast(s, DoWhileStmt), w, scope)

        ElseIf TypeOf s Is ForStmt Then
            GenForLoop(DirectCast(s, ForStmt), w, scope)

        ElseIf TypeOf s Is ForInStmt Then
            GenForInLoop(DirectCast(s, ForInStmt), w, scope)

        ElseIf TypeOf s Is ForOfStmt Then
            GenForOfLoop(DirectCast(s, ForOfStmt), w, scope)

        ElseIf TypeOf s Is SwitchStmt Then
            GenSwitch(DirectCast(s, SwitchStmt), w, scope)

        ElseIf TypeOf s Is ClassDeclStmt Then
            ' class declarations are block-scoped and not hoisted. The binding is
            ' declared first so that the member closures can capture it (a method
            ' referring to its own class resolves it at call time).
            Dim cls = DirectCast(s, ClassDeclStmt)
            Dim vbCls = scope.DefineLocal(cls.Spec.Name)
            w.Line($"Dim {vbCls} As JsValue = Nothing")
            w.Line($"{vbCls} = {GenClassValue(cls.Spec, scope)}")

        ElseIf TypeOf s Is LabeledStmt Then
            GenLabeled(DirectCast(s, LabeledStmt), w, scope)

        ElseIf TypeOf s Is BreakStmt Then
            Dim b = DirectCast(s, BreakStmt)
            If b.Label IsNot Nothing Then
                w.Line(LabeledJump(b.Label, isExit:=True))
            Else
                w.Line(If(_loopExit.Count > 0, _loopExit.Peek(), "Exit Do"))
            End If

        ElseIf TypeOf s Is ContinueStmt Then
            Dim c = DirectCast(s, ContinueStmt)
            If c.Label IsNot Nothing Then
                w.Line(LabeledJump(c.Label, isExit:=False))
            Else
                w.Line(If(_loopCont.Count > 0, _loopCont.Peek(), "Continue Do"))
            End If

        ElseIf TypeOf s Is ReturnStmt Then
            Dim r = DirectCast(s, ReturnStmt)
            Dim valueTxt = If(r.Value IsNot Nothing, GenExpr(r.Value, scope), "JsRuntime.Undef")
            If _asyncPromise.Count > 0 Then
                ' inside an async function 'return x' settles the promise
                Dim promise = _asyncPromise.Peek()
                w.Line($"{promise}.Fulfill({valueTxt})")
                w.Line($"Return {promise}")
            Else
                w.Line($"Return {valueTxt}")
            End If

        ElseIf TypeOf s Is ThrowStmt Then
            w.Line($"Throw New LiteJsRuntime.JsRuntimeException({GenExpr(DirectCast(s, ThrowStmt).Value, scope)})")

        ElseIf TypeOf s Is TryStmt Then
            GenTry(DirectCast(s, TryStmt), w, scope)

        ElseIf TypeOf s Is EmptyStmt Then
            ' nothing

        Else
            Throw New NotSupportedException($"unsupported statement {s.GetType().Name}")
        End If
    End Sub

    ' ======================================================= loops

    ''' <summary>while (test) body — <paramref name="lc"/>/<paramref name="lx"/> are the
    ''' GoTo targets used by a labeled break/continue (nested loops included).</summary>
    Private Sub GenWhileLoop(n As WhileStmt, w As IndentWriter, scope As Scope,
                             Optional lc As String = Nothing, Optional lx As String = Nothing)
        w.Line($"While JsTruthy({GenExpr(n.Test, scope)})")
        w.Indent()
        _loopExit.Push(If(lx IsNot Nothing, $"GoTo {lx}", "Exit While"))
        _loopCont.Push(If(lc IsNot Nothing, $"GoTo {lc}", "Continue While"))
        GenBody(n.Body, w, scope)
        _loopCont.Pop() : _loopExit.Pop()
        w.UnIndent()
        If lc IsNot Nothing Then w.Line($"{lc}:")
        w.Line("End While")
        If lx IsNot Nothing Then w.Line($"{lx}:")
    End Sub

    Private Sub GenDoWhileLoop(n As DoWhileStmt, w As IndentWriter, scope As Scope,
                               Optional lc As String = Nothing, Optional lx As String = Nothing)
        w.Line("Do")
        w.Indent()
        _loopExit.Push(If(lx IsNot Nothing, $"GoTo {lx}", "Exit Do"))
        _loopCont.Push(If(lc IsNot Nothing, $"GoTo {lc}", "Continue Do"))
        GenBody(n.Body, w, scope)
        _loopCont.Pop() : _loopExit.Pop()
        If lc IsNot Nothing Then w.Line($"{lc}:")
        w.UnIndent()
        w.Line($"Loop While JsTruthy({GenExpr(n.Test, scope)})")
        If lx IsNot Nothing Then w.Line($"{lx}:")
    End Sub

    Private Sub GenForLoop(n As ForStmt, w As IndentWriter, scope As Scope,
                           Optional lc As String = Nothing, Optional lx As String = Nothing)
        If TypeOf n.Init Is VarStmt Then
            GenStmt(DirectCast(n.Init, VarStmt), w, scope)
        ElseIf n.Init IsNot Nothing Then
            GenExprAsStatement(DirectCast(n.Init, Expression), w, scope)
        End If
        ' JS 'continue' inside a for-loop still runs the update clause,
        ' so the update is placed at a label the generated GoTo targets.
        _label += 1
        Dim updLabel = $"__upd{_label}"
        w.Line("Do")
        w.Indent()
        Dim condExit = If(lx IsNot Nothing, $"GoTo {lx}", "Exit Do")
        w.Line($"If Not JsTruthy({GenExpr(n.Test, scope)}) Then {condExit}")
        _loopExit.Push(If(lx IsNot Nothing, $"GoTo {lx}", "Exit Do"))
        _loopCont.Push($"GoTo {updLabel}")
        GenBody(n.Body, w, scope)
        _loopCont.Pop() : _loopExit.Pop()
        If lc IsNot Nothing AndAlso lc <> updLabel Then w.Line($"{lc}:")
        w.Line($"{updLabel}:")
        If n.Update IsNot Nothing Then GenExprAsStatement(n.Update, w, scope)
        w.UnIndent()
        w.Line("Loop")
        If lx IsNot Nothing Then w.Line($"{lx}:")
    End Sub

    Private Sub GenForInLoop(n As ForInStmt, w As IndentWriter, scope As Scope,
                             Optional lc As String = Nothing, Optional lx As String = Nothing)
        Dim vbVar = scope.DefineLocal(n.VarName)
        w.Line($"For Each {vbVar} As JsValue In JsRuntime.JsForInKeys({GenExpr(n.ObjectExpr, scope)})")
        w.Indent()
        _loopExit.Push(If(lx IsNot Nothing, $"GoTo {lx}", "Exit For"))
        _loopCont.Push(If(lc IsNot Nothing, $"GoTo {lc}", "Continue For"))
        GenBody(n.Body, w, scope)
        _loopCont.Pop() : _loopExit.Pop()
        If lc IsNot Nothing Then w.Line($"{lc}:")
        w.UnIndent()
        w.Line("Next")
        If lx IsNot Nothing Then w.Line($"{lx}:")
    End Sub

    Private Sub GenForOfLoop(n As ForOfStmt, w As IndentWriter, scope As Scope,
                             Optional lc As String = Nothing, Optional lx As String = Nothing)
        Dim iterExpr = $"JsRuntime.JsIterableValues({GenExpr(n.Iterable, scope)})"
        Dim targetExpr As String = Nothing

        If TypeOf n.Target Is String AndAlso n.Kind IsNot Nothing Then
            ' declaration with a simple name: the loop variable itself
            Dim vbVar = scope.DefineLocal(CStr(n.Target))
            w.Line($"For Each {vbVar} As JsValue In {iterExpr}")
        Else
            ' destructuring target, or assignment to an existing variable
            Dim t = UniqueName("item")
            w.Line($"For Each {t} As JsValue In {iterExpr}")
            targetExpr = t
        End If

        w.Indent()
        If targetExpr IsNot Nothing Then
            GenBindTarget(n.Target, targetExpr, w, scope, n.Kind, isDeclaration:=(n.Kind IsNot Nothing))
        End If
        _loopExit.Push(If(lx IsNot Nothing, $"GoTo {lx}", "Exit For"))
        _loopCont.Push(If(lc IsNot Nothing, $"GoTo {lc}", "Continue For"))
        GenBody(n.Body, w, scope)
        _loopCont.Pop() : _loopExit.Pop()
        If lc IsNot Nothing Then w.Line($"{lc}:")
        w.UnIndent()
        w.Line("Next")
        If lx IsNot Nothing Then w.Line($"{lx}:")
    End Sub

    ' ======================================================= switch

    ''' <summary>
    ''' switch → If + GoTo label chain (VB Select Case has no fall-through and uses
    ''' loose comparison, neither of which matches JS).
    ''' </summary>
    Private Sub GenSwitch(n As SwitchStmt, w As IndentWriter, scope As Scope)
        Dim discVar = UniqueName("sw")
        w.Line($"Dim {discVar} As JsValue = {GenExpr(n.Discriminant, scope)}")

        _label += 1
        Dim tagN = _label
        Dim exitLabel = $"__swend{tagN}"
        Dim defaultLabel As String = Nothing
        Dim labels(n.Cases.Count - 1) As String

        ' dispatch chain: evaluate case tests in order until one matches
        For i = 0 To n.Cases.Count - 1
            labels(i) = $"__case{tagN}_{i}"
            If n.Cases(i).Test Is Nothing Then
                defaultLabel = labels(i)
            Else
                w.Line($"If JsRuntime.JsStrictEq({discVar}, {GenExpr(n.Cases(i).Test, scope)}) Then GoTo {labels(i)}")
            End If
        Next
        w.Line($"GoTo {If(defaultLabel IsNot Nothing, defaultLabel, exitLabel)}")

        ' bodies: fall through to the next arm unless a break jumps to the end
        _loopExit.Push($"GoTo {exitLabel}")
        For i = 0 To n.Cases.Count - 1
            w.Line($"{labels(i)}:")
            w.Indent()
            For Each st In n.Cases(i).Body
                GenStmt(st, w, scope)
            Next
            If i < n.Cases.Count - 1 Then w.Line($"GoTo {labels(i + 1)}")     ' fall-through
            w.UnIndent()
        Next
        _loopExit.Pop()
        w.Line($"{exitLabel}:")
    End Sub

    ' ======================================================= labels

    Private Sub GenLabeled(lb As LabeledStmt, w As IndentWriter, scope As Scope)
        _label += 1
        Dim n = _label
        Dim exitL = $"__lexit{n}"
        Dim contL = $"__lcont{n}"
        Dim body = lb.Body
        Dim isLoop = TypeOf body Is WhileStmt OrElse TypeOf body Is DoWhileStmt OrElse
                     TypeOf body Is ForStmt OrElse TypeOf body Is ForInStmt OrElse TypeOf body Is ForOfStmt

        Dim hadPrev = _labeledTargets.ContainsKey(lb.Label)
        Dim prev As LabelInfo = Nothing
        If hadPrev Then prev = _labeledTargets(lb.Label)
        _labeledTargets(lb.Label) = New LabelInfo(exitL, If(isLoop, contL, Nothing))
        Try
            If TypeOf body Is WhileStmt Then
                GenWhileLoop(DirectCast(body, WhileStmt), w, scope, contL, exitL)
            ElseIf TypeOf body Is DoWhileStmt Then
                GenDoWhileLoop(DirectCast(body, DoWhileStmt), w, scope, contL, exitL)
            ElseIf TypeOf body Is ForStmt Then
                ' the for-loop's own update label doubles as the continue target
                GenForLoop(DirectCast(body, ForStmt), w, scope, contL, exitL)
            ElseIf TypeOf body Is ForInStmt Then
                GenForInLoop(DirectCast(body, ForInStmt), w, scope, contL, exitL)
            ElseIf TypeOf body Is ForOfStmt Then
                GenForOfLoop(DirectCast(body, ForOfStmt), w, scope, contL, exitL)
            Else
                GenStmt(body, w, scope)
                w.Line($"{exitL}:")
            End If
        Finally
            If hadPrev Then
                _labeledTargets(lb.Label) = prev
            Else
                _labeledTargets.Remove(lb.Label)
            End If
        End Try
    End Sub

    ''' <summary>Resolve a labeled break/continue into its GoTo target.</summary>
    Private Function LabeledJump(label As String, isExit As Boolean) As String
        Dim info As LabelInfo = Nothing
        If Not _labeledTargets.TryGetValue(label, info) Then
            Throw New NotSupportedException($"label '{label}' is not visible in this function")
        End If
        If isExit Then Return $"GoTo {info.ExitLabel}"
        If info.ContLabel Is Nothing Then
            Throw New NotSupportedException($"'{label}' is not a loop label")
        End If
        Return $"GoTo {info.ContLabel}"
    End Function

    ''' <summary>GoTo targets of a labeled statement (exit label, optional continue label).</summary>
    Private NotInheritable Class LabelInfo
        Public ReadOnly ExitLabel As String
        Public ReadOnly ContLabel As String
        Public Sub New(exitLabel As String, contLabel As String)
            Me.ExitLabel = exitLabel
            Me.ContLabel = contLabel
        End Sub
    End Class

    Private Sub GenIf(n As IfStmt, w As IndentWriter, scope As Scope)
        w.Line($"If JsTruthy({GenExpr(n.Test, scope)}) Then")
        w.Indent()
        GenBody(n.ThenBranch, w, scope)
        w.UnIndent()
        Dim node = n
        While node.ElseBranch IsNot Nothing
            If TypeOf node.ElseBranch Is IfStmt Then
                node = DirectCast(node.ElseBranch, IfStmt)
                w.Line($"ElseIf JsTruthy({GenExpr(node.Test, scope)}) Then")
                w.Indent()
                GenBody(node.ThenBranch, w, scope)
                w.UnIndent()
            Else
                w.Line("Else")
                w.Indent()
                GenBody(node.ElseBranch, w, scope)
                w.UnIndent()
                Exit While
            End If
        End While
        w.Line("End If")
    End Sub

    Private Sub GenTry(t As TryStmt, w As IndentWriter, scope As Scope)
        Dim exName = UniqueName("jsex")
        w.Line("Try")
        w.Indent()
        GenBlock(t.TryBlock, w, scope)
        w.UnIndent()
        If t.CatchBlock IsNot Nothing Then
            w.Line($"Catch {exName} As LiteJsRuntime.JsRuntimeException")
            w.Indent()
            If t.CatchParam IsNot Nothing Then
                Dim vbParam = scope.DefineLocal(t.CatchParam)
                w.Line($"Dim {vbParam} As JsValue = {exName}.Payload")
            End If
            GenBlock(t.CatchBlock, w, scope)
            w.UnIndent()
        End If
        If t.FinallyBlock IsNot Nothing Then
            w.Line("Finally")
            w.Indent()
            GenBlock(t.FinallyBlock, w, scope)
            w.UnIndent()
        End If
        w.Line("End Try")
    End Sub

    Private Sub GenFunctionDecl(f As FuncDeclStmt, w As IndentWriter, scope As Scope)
        Dim vbName = scope.DefineLocal(f.Name)
        Dim argsName = ArgsParam()
        w.WriteLine()
        w.Line($"Dim {vbName} As Func(Of Object(), Object) = Function({argsName} As JsValue())")
        w.Indent()
        w.Paste(LambdaBody(New FuncExpr(f.Name, f.Parameters, f.Body, defaults:=f.Defaults,
                                        restParam:=f.RestParam, isAsync:=f.IsAsync,
                                        isGenerator:=f.IsGenerator),
                           scope, argsName))
        w.UnIndent()
        w.Line("End Function")
    End Sub

    ' ======================================================= expression statements

    Private Sub GenExprAsStatement(e As Expression, w As IndentWriter, scope As Scope)
        If TypeOf e Is AssignExpr Then
            GenAssignStatement(DirectCast(e, AssignExpr), w, scope)
        ElseIf TypeOf e Is UpdateExpr Then
            w.Line(UpdateStatementCore(DirectCast(e, UpdateExpr), scope))
        Else
            w.Line(GenExpr(e, scope))
        End If
    End Sub

    Private Sub GenAssignStatement(a As AssignExpr, w As IndentWriter, scope As Scope)
        Dim rhs = If(a.Op = "=", GenExpr(a.Value, scope),
                     $"{CompoundFn(a.Op)}({GenExpr(a.Target, scope)}, {GenExpr(a.Value, scope)})")
        Dim target = a.Target
        If TypeOf target Is IdentExpr Then
            w.Line($"{scope.ResolveOrThrow(DirectCast(target, IdentExpr).Name)} = {rhs}")
        ElseIf TypeOf target Is MemberExpr Then
            Dim m = DirectCast(target, MemberExpr)
            Dim objTxt = GenExpr(m.Obj, scope)
            If m.Computed Then
                w.Line($"JsRuntime.JsSetIndex({objTxt}, {GenExpr(DirectCast(m.Name, Expression), scope)}, {rhs})")
            Else
                w.Line($"JsRuntime.JsSet({objTxt}, {VbString(CStr(m.Name))}, {rhs})")
            End If
        Else
            Throw New NotSupportedException("invalid assignment target")
        End If
    End Sub

    Private Function UpdateStatementCore(u As UpdateExpr, scope As Scope) As String
        Dim fn = If(u.Op = "++", "JsRuntime.JsAdd", "JsRuntime.JsSub")
        Dim target = u.Target
        If TypeOf target Is IdentExpr Then
            Dim vbName = scope.ResolveOrThrow(DirectCast(target, IdentExpr).Name)
            Return $"{vbName} = {fn}({vbName}, 1.0R)"
        Else
            Dim m = DirectCast(target, MemberExpr)
            Dim objTxt = GenExpr(m.Obj, scope)
            Dim keyTxt = If(m.Computed, GenExpr(DirectCast(m.Name, Expression), scope), VbString(CStr(m.Name)))
            Dim cur = If(m.Computed,
                         $"JsRuntime.JsIndex({objTxt}, {keyTxt})",
                         $"JsRuntime.JsGet({objTxt}, {keyTxt})")
            Dim put = If(m.Computed,
                         $"JsRuntime.JsSetIndex({objTxt}, {keyTxt}, {fn}({cur}, 1.0R))",
                         $"JsRuntime.JsSet({objTxt}, {keyTxt}, {fn}({cur}, 1.0R))")
            Return put
        End If
    End Function

    Private Shared Function CompoundFn(op As String) As String
        Select Case op
            Case "+=" : Return "JsRuntime.JsAdd"
            Case "-=" : Return "JsRuntime.JsSub"
            Case "*=" : Return "JsRuntime.JsMul"
            Case "/=" : Return "JsRuntime.JsDiv"
            Case "%=" : Return "JsRuntime.JsMod"
        End Select
        Throw New NotSupportedException("unsupported compound assignment " & op)
    End Function

    ' ======================================================= expressions

    Friend Function GenExpr(e As Expression, scope As Scope) As String
        If TypeOf e Is LiteralExpr Then
            Return LiteralText(DirectCast(e, LiteralExpr).Value.AsObject())

        ElseIf TypeOf e Is IdentExpr Then
            Return ResolveIdent(DirectCast(e, IdentExpr).Name, scope)

        ElseIf TypeOf e Is UnaryExpr Then
            Dim u = DirectCast(e, UnaryExpr)
            Select Case u.Op
                Case "delete"
                    ' delete needs the reference, not the value
                    If TypeOf u.Operand Is MemberExpr Then
                        Dim m = DirectCast(u.Operand, MemberExpr)
                        Dim objTxt = GenExpr(m.Obj, scope)
                        If m.Computed Then
                            Return $"JsRuntime.JsDelete({objTxt}, {GenExpr(DirectCast(m.Name, Expression), scope)})"
                        End If
                        Return $"JsRuntime.JsDelete({objTxt}, {VbString(CStr(m.Name))})"
                    End If
                    ' non-reference operand: evaluate for side effects (JS returns true)
                    Return $"JsRuntime.JsDeleteValue({GenExpr(u.Operand, scope)})"
                Case "void"
                    Return $"JsRuntime.JsVoid({GenExpr(u.Operand, scope)})"
            End Select
            Dim x = GenExpr(u.Operand, scope)
            Select Case u.Op
                Case "-" : Return $"JsRuntime.JsNeg({x})"
                Case "+" : Return $"JsRuntime.JsNum({x})"
                Case "!" : Return $"JsRuntime.JsNot({x})"
                Case "~" : Return $"JsRuntime.JsBitNot({x})"
                Case "typeof" : Return $"JsRuntime.JsTypeOf({x})"
            End Select
            Throw New NotSupportedException("unary " & u.Op)

        ElseIf TypeOf e Is UpdateExpr Then
            ' expression position: IIFE that preserves the JS old/new value
            Dim u = DirectCast(e, UpdateExpr)
            Dim t = UniqueName("t")
            Dim stmt = UpdateStatementCore(u, scope)
            Dim post = GenExpr(u.Target, scope)          ' post-update value
            Dim pre = GenExpr(u.Target, scope)           ' pre-update read
            Dim sb As New StringBuilder()
            sb.Append("CType(Function() As JsValue" & vbLf)
            If Not u.IsPrefix Then sb.Append($"Dim {t} As JsValue = {pre}" & vbLf)
            sb.Append(stmt & vbLf)
            sb.Append($"Return {If(u.IsPrefix, post, t)}" & vbLf)
            sb.Append("End Function, Func(Of Object(), Object)).Invoke(New Object() {})")
            Return sb.ToString()

        ElseIf TypeOf e Is BinaryExpr Then
            Dim b = DirectCast(e, BinaryExpr)
            Dim x = GenExpr(b.Left, scope), y = GenExpr(b.Right, scope)
            Select Case b.Op
                Case "+" : Return $"JsRuntime.JsAdd({x}, {y})"
                Case "-" : Return $"JsRuntime.JsSub({x}, {y})"
                Case "*" : Return $"JsRuntime.JsMul({x}, {y})"
                Case "/" : Return $"JsRuntime.JsDiv({x}, {y})"
                Case "%" : Return $"JsRuntime.JsMod({x}, {y})"
                Case "**" : Return $"JsRuntime.JsPow({x}, {y})"
                Case "==" : Return $"JsRuntime.JsEq({x}, {y})"
                Case "!=" : Return $"JsRuntime.JsNe({x}, {y})"
                Case "===" : Return $"JsRuntime.JsStrictEq({x}, {y})"
                Case "!==" : Return $"JsRuntime.JsStrictNe({x}, {y})"
                Case "<" : Return $"JsRuntime.JsLt({x}, {y})"
                Case "<=" : Return $"JsRuntime.JsLe({x}, {y})"
                Case ">" : Return $"JsRuntime.JsGt({x}, {y})"
                Case ">=" : Return $"JsRuntime.JsGe({x}, {y})"
                Case "&" : Return $"JsRuntime.JsBitAnd({x}, {y})"
                Case "|" : Return $"JsRuntime.JsBitOr({x}, {y})"
                Case "^" : Return $"JsRuntime.JsBitXor({x}, {y})"
                Case "<<" : Return $"JsRuntime.JsShl({x}, {y})"
                Case ">>" : Return $"JsRuntime.JsShr({x}, {y})"
                Case ">>>" : Return $"JsRuntime.JsUshr({x}, {y})"
                Case "in" : Return $"JsRuntime.JsIn({x}, {y})"
                Case "instanceof" : Return $"JsRuntime.JsInstanceOf({x}, {y})"
            End Select
            Throw New NotSupportedException("binary " & b.Op)

        ElseIf TypeOf e Is LogicalExpr Then
            Dim l = DirectCast(e, LogicalExpr)
            Dim x = GenExpr(l.Left, scope)
            Dim yLazy = $"Function() {GenExpr(l.Right, scope)}"
            If l.Op = "&&" Then Return $"JsRuntime.JsAnd({x}, {yLazy})"
            If l.Op = "||" Then Return $"JsRuntime.JsOr({x}, {yLazy})"
            Return $"JsRuntime.JsNullish({x}, {yLazy})"

        ElseIf TypeOf e Is CondExpr Then
            Dim c = DirectCast(e, CondExpr)
            Return $"If(JsTruthy({GenExpr(c.Test, scope)}), {GenExpr(c.Consequent, scope)}, {GenExpr(c.Alternate, scope)})"

        ElseIf TypeOf e Is AssignExpr Then
            Dim a = DirectCast(e, AssignExpr)
            Dim t = UniqueName("t")
            Dim rhs = If(a.Op = "=", GenExpr(a.Value, scope),
                         $"{CompoundFn(a.Op)}({GenExpr(a.Target, scope)}, {GenExpr(a.Value, scope)})")
            Dim sb As New StringBuilder()
            sb.Append("CType(Function() As JsValue" & vbLf)
            sb.Append($"Dim {t} As JsValue = {rhs}" & vbLf)
            Dim target = a.Target
            If TypeOf target Is IdentExpr Then
                sb.Append($"{scope.ResolveOrThrow(DirectCast(target, IdentExpr).Name)} = {t}" & vbLf)
            ElseIf TypeOf target Is MemberExpr Then
                Dim m = DirectCast(target, MemberExpr)
                Dim objTxt = GenExpr(m.Obj, scope)
                If m.Computed Then
                    sb.Append($"JsRuntime.JsSetIndex({objTxt}, {GenExpr(DirectCast(m.Name, Expression), scope)}, {t})" & vbLf)
                Else
                    sb.Append($"JsRuntime.JsSet({objTxt}, {VbString(CStr(m.Name))}, {t})" & vbLf)
                End If
            Else
                Throw New NotSupportedException("invalid assignment target")
            End If
            sb.Append($"Return {t}" & vbLf)
            sb.Append("End Function, Func(Of Object(), Object)).Invoke(New Object() {})")
            Return sb.ToString()

        ElseIf TypeOf e Is CallExpr Then
            Dim c = DirectCast(e, CallExpr)

            ' super(...) / super.m(...)
            Dim sup = TryCast(c.Callee, SuperExpr)
            If sup IsNot Nothing Then
                Dim supArgs = GenArgs(c.Arguments, scope)
                If sup.Member Is Nothing Then Return $"JsRuntime.JsSuperConstructor({supArgs})"
                Return $"JsRuntime.JsSuperInvoke({VbString(sup.Member)}, {supArgs})"
            End If

            ' method call: pass the receiver so that 'this' binds correctly
            Dim m = TryCast(c.Callee, MemberExpr)
            If m IsNot Nothing Then
                Dim objTxt = GenExpr(m.Obj, scope)
                Dim callArgs = GenArgs(c.Arguments, scope)
                If m.Computed Then
                    Dim keyTxt = GenExpr(DirectCast(m.Name, Expression), scope)
                    Return $"JsRuntime.JsInvokeIndex({objTxt}, {keyTxt}, {callArgs}, {VbBool(m.IsOptional)}, {VbBool(c.IsOptional)})"
                End If
                Return $"JsRuntime.JsInvokeMethod({objTxt}, {VbString(CStr(m.Name))}, {callArgs}, {VbBool(m.IsOptional)}, {VbBool(c.IsOptional)})"
            End If

            Dim callee = GenExpr(c.Callee, scope)
            Dim args = GenArgs(c.Arguments, scope)
            If c.IsOptional Then Return $"JsRuntime.JsChainInvoke({callee}, {args})"
            Return $"JsRuntime.JsInvoke({callee}, {args})"

        ElseIf TypeOf e Is ThisExpr Then
            ' lexical 'this': a local inside non-arrow functions, the global object otherwise
            Dim vbThis = scope.Resolve("this")
            If vbThis Is Nothing Then Return "JsRuntime.CurrentThis"
            Return vbThis

        ElseIf TypeOf e Is SuperExpr Then
            Dim sup = DirectCast(e, SuperExpr)
            If sup.Member Is Nothing Then
                Throw New NotSupportedException("bare super is only valid as a call")
            End If
            Return $"JsRuntime.JsSuperGet({VbString(sup.Member)})"

        ElseIf TypeOf e Is NewExpr Then
            Dim ne = DirectCast(e, NewExpr)
            Return $"JsRuntime.JsNew({GenExpr(ne.Callee, scope)}, {GenArgs(ne.Arguments, scope)})"

        ElseIf TypeOf e Is ClassExpr Then
            Return GenClassValue(DirectCast(e, ClassExpr).Spec, scope)

        ElseIf TypeOf e Is RegexExpr Then
            Dim rx = DirectCast(e, RegexExpr)
            Return $"New LiteJsRuntime.JsRegExp({VbString(rx.Source)}, {VbString(rx.Flags)})"

        ElseIf TypeOf e Is AwaitExpr Then
            Return $"JsRuntime.JsAwait({GenExpr(DirectCast(e, AwaitExpr).Operand, scope)})"

        ElseIf TypeOf e Is YieldExpr Then
            Dim yy = DirectCast(e, YieldExpr)
            Dim yvalue = If(yy.Operand IsNot Nothing, GenExpr(yy.Operand, scope), "JsRuntime.Undef")
            If _yieldCollector.Count = 0 Then
                Throw New NotSupportedException("'yield' is only valid inside a generator function")
            End If
            ' AddValue records the value and evaluates to it (yield is an expression)
            Return $"{_yieldCollector.Peek()}.AddValue({yvalue})"

        ElseIf TypeOf e Is OptionalChainExpr Then
            ' the chain root turns the short-circuit signal into undefined
            Dim chain = GenExpr(DirectCast(e, OptionalChainExpr).Chain, scope)
            Dim exName = UniqueName("oc")
            Dim sb As New StringBuilder()
            sb.Append("CType(Function() As JsValue" & vbLf)
            sb.Append("Try" & vbLf)
            sb.Append($"Return {chain}" & vbLf)
            sb.Append($"Catch {exName} As LiteJsRuntime.OptionalChainShortCircuit" & vbLf)
            sb.Append("Return JsRuntime.Undef" & vbLf)
            sb.Append("End Try" & vbLf)
            sb.Append("End Function, Func(Of Object(), Object)).Invoke(New Object() {})")
            Return sb.ToString()

        ElseIf TypeOf e Is MemberExpr Then
            Dim m = DirectCast(e, MemberExpr)
            Dim objTxt = GenExpr(m.Obj, scope)
            If m.Computed Then
                Dim keyTxt = GenExpr(DirectCast(m.Name, Expression), scope)
                If m.IsOptional Then Return $"JsRuntime.JsChainIndex({objTxt}, {keyTxt})"
                Return $"JsRuntime.JsIndex({objTxt}, {keyTxt})"
            End If
            If m.IsOptional Then Return $"JsRuntime.JsChainGet({objTxt}, {VbString(CStr(m.Name))})"
            Return $"JsRuntime.JsGet({objTxt}, {VbString(CStr(m.Name))})"

        ElseIf TypeOf e Is TemplateExpr Then
            Return GenTemplate(DirectCast(e, TemplateExpr), scope)

        ElseIf TypeOf e Is SequenceExpr Then
            Dim sq = DirectCast(e, SequenceExpr)
            Dim sb As New StringBuilder()
            sb.Append("CType(Function() As JsValue" & vbLf)
            For i = 0 To sq.Expressions.Count - 2
                sb.Append(GenExpr(sq.Expressions(i), scope) & vbLf)
            Next
            sb.Append($"Return {GenExpr(sq.Expressions(sq.Expressions.Count - 1), scope)}" & vbLf)
            sb.Append("End Function, Func(Of Object(), Object)).Invoke(New Object() {})")
            Return sb.ToString()

        ElseIf TypeOf e Is DestructureAssignExpr Then
            Dim d = DirectCast(e, DestructureAssignExpr)
            Dim v = UniqueName("dv")
            Dim bind As New IndentWriter()
            GenBindTarget(d.Target, v, bind, scope, Nothing, isDeclaration:=False)
            Dim sb As New StringBuilder()
            sb.Append("CType(Function() As JsValue" & vbLf)
            sb.Append($"Dim {v} As JsValue = {GenExpr(d.Value, scope)}" & vbLf)
            sb.Append(bind.ToString())
            sb.Append($"Return {v}" & vbLf)
            sb.Append("End Function, Func(Of Object(), Object)).Invoke(New Object() {})")
            Return sb.ToString()

        ElseIf TypeOf e Is ArrayExpr Then
            Return GenArrayLiteral(DirectCast(e, ArrayExpr), scope)

        ElseIf TypeOf e Is ObjectExpr Then
            Return GenObjectLiteral(DirectCast(e, ObjectExpr), scope)

        ElseIf TypeOf e Is FuncExpr Then
            Return GenLambda(DirectCast(e, FuncExpr), scope)

        ElseIf TypeOf e Is SpreadElement Then
            Throw New NotSupportedException("spread element outside an argument/array/object literal")
        End If

        Throw New NotSupportedException("unsupported expression " & e.GetType().Name)
    End Function

    ''' <summary>VB Booleans for the generated runtime-helper flags.</summary>
    Private Shared Function VbBool(b As Boolean) As String
        Return If(b, "True", "False")
    End Function

    ''' <summary>Multi-line lambda expression for a function value.</summary>
    Private Function GenLambda(f As FuncExpr, scope As Scope) As String
        Dim argsName = ArgsParam()
        Return "CType(Function(" & argsName & " As JsValue())" & vbLf &
               LambdaBody(f, scope, argsName) & vbLf &
               "End Function, Func(Of Object(), Object))"
    End Function

    ''' <summary>
    ''' A class definition as an expression:
    ''' <c>JsRuntime.JsDefineClass(name, ctor, super, ("i"/"s"/"f", name, value)...)</c>.
    ''' The interpreter builds classes through the same helper.
    ''' </summary>
    Private Function GenClassValue(spec As ClassSpec, scope As Scope) As String
        Dim ctorTxt = "Nothing"
        If spec.Constructor IsNot Nothing Then ctorTxt = GenLambda(spec.Constructor, scope)

        Dim superTxt = "Nothing"
        If spec.SuperClass IsNot Nothing Then superTxt = GenExpr(spec.SuperClass, scope)

        Dim parts As New List(Of String)
        For Each m In spec.Members
            Dim tag = VbString(If(m.IsStatic, "s", "i"))
            Select Case m.Kind
                Case ClassMemberKind.Method
                    parts.Add($"({tag}, {VbString(m.Name)}, {GenLambda(DirectCast(m.Value, FuncExpr), scope)})")
                Case ClassMemberKind.Getter
                    parts.Add($"({tag}, {VbString(m.Name)}, JsRuntime.JsAccessor({GenLambda(DirectCast(m.Value, FuncExpr), scope)}, Nothing))")
                Case ClassMemberKind.Setter
                    parts.Add($"({tag}, {VbString(m.Name)}, JsRuntime.JsAccessor(Nothing, {GenLambda(DirectCast(m.Value, FuncExpr), scope)}))")
                Case ClassMemberKind.Field
                    If m.IsStatic Then
                        ' static fields are initialised once, at definition time
                        Dim v = If(m.Value Is Nothing, "JsRuntime.Undef", GenExpr(m.Value, scope))
                        parts.Add($"(""s"", {VbString(m.Name)}, {v})")
                    Else
                        Dim init = "Nothing"
                        If m.Value IsNot Nothing Then
                            ' zero-argument wrapper so 'this' binds to the new instance
                            Dim wrapper As New FuncExpr(Nothing, New String() {},
                                                        New List(Of Statement) From {New ReturnStmt(m.Value)})
                            init = GenLambda(wrapper, scope)
                        End If
                        parts.Add($"(""f"", {VbString(m.Name)}, {init})")
                    End If
            End Select
        Next

        Dim partsTxt = $"New (String, String, Object)() {{{String.Join(", ", parts)}}}"
        Return $"JsRuntime.JsDefineClass({VbString(spec.Name)}, {ctorTxt}, {superTxt}, {partsTxt})"
    End Function

    ''' <summary>
    ''' Template literal → VB string concatenation with JsStr conversion.
    ''' Every chunk after the first is joined with a concatenation operator; a
    ''' literal chunk that follows an interpolation needs the connector as well.
    ''' </summary>
    Private Function GenTemplate(t As TemplateExpr, scope As Scope) As String
        Dim sb As New StringBuilder()
        For Each chunk In t.Chunks
            If TypeOf chunk Is String Then
                If sb.Length > 0 Then sb.Append(" & ")
                sb.Append(VbString(CStr(chunk)))
            Else
                sb.Append(" & ").Append("JsRuntime.JsStr(").
                   Append(GenExpr(DirectCast(chunk, Expression), scope)).
                   Append(")")
            End If
        Next
        Dim result = sb.ToString()
        If result.StartsWith(" & ", StringComparison.Ordinal) Then result = result.Substring(3)
        If result = "" Then Return """"""
        Return result
    End Function

    ''' <summary>Array literal with holes and spread elements.</summary>
    Private Function GenArrayLiteral(a As ArrayExpr, scope As Scope) As String
        Dim hasSpread = a.Elements.Any(Function(x) TypeOf x Is SpreadElement)
        If hasSpread Then
            Dim parts = a.Elements.Select(Function(x) GenArrayPart(x, scope)).ToArray()
            Return "JsRuntime.JsArrayFrom(" & String.Join(", ", parts) & ")"
        End If
        If a.Elements.Count = 0 Then Return "JsRuntime.JsArray()"
        Dim items = a.Elements.Select(Function(x) GenArrayPart(x, scope)).ToArray()
        Return "JsRuntime.JsArray(" & String.Join(", ", items) & ")"
    End Function

    Private Function GenArrayPart(x As Expression, scope As Scope) As String
        If x Is Nothing Then Return "JsRuntime.Undef"        ' hole reads as undefined
        If TypeOf x Is SpreadElement Then
            Return $"JsRuntime.JsSpread({GenExpr(DirectCast(x, SpreadElement).Argument, scope)})"
        End If
        Return GenExpr(x, scope)
    End Function

    ''' <summary>Object literal with spread / accessors / computed keys / shorthand.</summary>
    Private Function GenObjectLiteral(o As ObjectExpr, scope As Scope) As String
        Dim needsFrom = o.Properties.Any(Function(p) p.Kind = ObjectPropertyKind.Spread OrElse
                                                    p.Kind = ObjectPropertyKind.Getter OrElse
                                                    p.Kind = ObjectPropertyKind.Setter)
        If o.Properties.Count = 0 Then Return "JsRuntime.JsObj(New (String, Object)() {})"
        Dim parts = o.Properties.Select(Function(p) GenObjectPart(p, scope)).ToArray()
        If needsFrom Then
            Return "JsRuntime.JsObjFrom(" & String.Join(", ", parts) & ")"
        End If
        Return "JsRuntime.JsObj(New (String, Object)() {" & String.Join(", ", parts) & "})"
    End Function

    Private Function GenObjectPart(p As ObjectProperty, scope As Scope) As String
        Select Case p.Kind
            Case ObjectPropertyKind.Spread
                Return $"JsRuntime.JsSpread({GenExpr(p.Value, scope)})"
            Case ObjectPropertyKind.Getter
                Return $"({KeyText(p, scope)}, JsRuntime.JsAccessor({GenExpr(p.Value, scope)}, Nothing))"
            Case ObjectPropertyKind.Setter
                Return $"({KeyText(p, scope)}, JsRuntime.JsAccessor(Nothing, {GenExpr(p.Value, scope)}))"
            Case ObjectPropertyKind.Shorthand
                Dim v = If(p.Value Is Nothing, ResolveIdent(p.Key, scope), GenExpr(p.Value, scope))
                Return $"({VbString(p.Key)}, {v})"
            Case Else
                Return $"({KeyText(p, scope)}, {GenExpr(p.Value, scope)})"
        End Select
    End Function

    ''' <summary>Property key text: literal name or a computed JsStr(expr).</summary>
    Private Function KeyText(p As ObjectProperty, scope As Scope) As String
        If p.KeyExpr IsNot Nothing Then Return $"JsRuntime.JsStr({GenExpr(p.KeyExpr, scope)})"
        Return VbString(p.Key)
    End Function

    ''' <summary>Call argument list, expanding spread elements when present.</summary>
    Private Function GenArgs(args As List(Of Expression), scope As Scope) As String
        Dim hasSpread = args.Any(Function(a) TypeOf a Is SpreadElement)
        If hasSpread Then
            Dim parts = args.Select(Function(a) GenArrayPart(a, scope)).ToArray()
            Return "JsRuntime.JsBuildArgs(" & String.Join(", ", parts) & ")"
        End If
        If args.Count = 0 Then Return "New JsValue() {}"
        Dim items = args.Select(Function(a) GenExpr(a, scope)).ToArray()
        Return "New JsValue() {" & String.Join(", ", items) & "}"
    End Function

    ' ======================================================= binding targets

    ''' <summary>
    ''' Emit the statements binding a (possibly destructuring) target.
    ''' <paramref name="isDeclaration"/> selects declaration semantics
    ''' (let/const Dim, var hoisted assignment) versus plain assignment.
    ''' </summary>
    Private Sub GenBindTarget(target As Object, valueExpr As String, w As IndentWriter, scope As Scope,
                              kind As String, isDeclaration As Boolean)
        If TypeOf target Is String Then
            GenBindName(CStr(target), valueExpr, w, scope, kind, isDeclaration)
            Return
        End If

        Dim bt = DirectCast(target, BindingTarget)
        Select Case True
            Case TypeOf bt Is NameTarget
                GenBindName(DirectCast(bt, NameTarget).Name, valueExpr, w, scope, kind, isDeclaration)

            Case TypeOf bt Is MemberTarget
                Dim m = DirectCast(bt, MemberTarget)
                Dim objTxt = GenExpr(m.Obj, scope)
                If m.Computed Then
                    w.Line($"JsRuntime.JsSetIndex({objTxt}, {GenExpr(DirectCast(m.Name, Expression), scope)}, {valueExpr})")
                Else
                    w.Line($"JsRuntime.JsSet({objTxt}, {VbString(CStr(m.Name))}, {valueExpr})")
                End If

            Case TypeOf bt Is ArrayTarget
                Dim at = DirectCast(bt, ArrayTarget)
                Dim t = UniqueName("arr")
                w.Line($"Dim {t} As JsValue = {valueExpr}")
                For i = 0 To at.Elements.Count - 1
                    If at.Elements(i) Is Nothing Then Continue For      ' hole
                    GenBindTarget(at.Elements(i), $"JsRuntime.JsIndex({t}, {i}.0R)", w, scope, kind, isDeclaration)
                Next
                If at.Rest IsNot Nothing Then
                    GenBindTarget(at.Rest, $"JsRuntime.JsArrayRest({t}, {at.Elements.Count})", w, scope, kind, isDeclaration)
                End If

            Case TypeOf bt Is ObjectTarget
                Dim ot = DirectCast(bt, ObjectTarget)
                Dim t = UniqueName("obj")
                w.Line($"Dim {t} As JsValue = {valueExpr}")
                Dim keys As New List(Of String)
                For Each prop In ot.Properties
                    keys.Add(VbString(prop.Key))
                    GenBindTarget(prop.Value, $"JsRuntime.JsGet({t}, {VbString(prop.Key)})", w, scope, kind, isDeclaration)
                Next
                If ot.Rest IsNot Nothing Then
                    Dim restKeys = If(keys.Count = 0, "", ", " & String.Join(", ", keys))
                    GenBindTarget(ot.Rest, $"JsRuntime.JsObjectRest({t}{restKeys})", w, scope, kind, isDeclaration)
                End If

            Case TypeOf bt Is DefaultTarget
                Dim dt = DirectCast(bt, DefaultTarget)
                Dim t = UniqueName("dv")
                w.Line($"Dim {t} As JsValue = {valueExpr}")
                GenBindTarget(dt.Target, $"If({t} Is JsRuntime.Undef, {GenExpr(dt.Default, scope)}, {t})",
                              w, scope, kind, isDeclaration)

            Case Else
                Throw New NotSupportedException("unknown binding target")
        End Select
    End Sub

    Private Sub GenBindName(name As String, valueExpr As String, w As IndentWriter, scope As Scope,
                            kind As String, isDeclaration As Boolean)
        If Not isDeclaration Then
            w.Line($"{scope.ResolveOrThrow(name)} = {valueExpr}")
            Return
        End If
        If kind = "var" Then
            ' hoisted by EmitHoistedVars → plain assignment
            Dim vb = scope.Resolve(name)
            If vb Is Nothing Then
                vb = scope.DefineLocal(name)
                w.Line($"Dim {vb} As JsValue = JsRuntime.Undef")
            End If
            w.Line($"{vb} = {valueExpr}")
        Else
            Dim vb = scope.DefineLocal(name)
            w.Line($"Dim {vb} As JsValue = {valueExpr}")
        End If
    End Sub

    ' ======================================================= lambda bodies

    Private Function ArgsParam() As String
        _temp += 1
        Return $"args__{_temp}"
    End Function

    ''' <summary>Parameter binding + function-hoisted block + trailing Return Undef, as plain lines.</summary>
    Private Function LambdaBody(f As FuncExpr, parent As Scope, argsName As String) As String
        ' a function body is a loop boundary in JS: break/continue (even labeled)
        ' inside can only target the function's own loops, never the caller's
        Dim saveExit = _loopExit.ToArray()
        Dim saveCont = _loopCont.ToArray()
        Dim saveLabels = _labeledTargets.ToArray()
        Dim saveAsync = _asyncPromise.ToArray()
        Dim saveGen = _yieldCollector.ToArray()
        _loopExit.Clear() : _loopCont.Clear() : _labeledTargets.Clear() : _asyncPromise.Clear() : _yieldCollector.Clear()

        Dim w As New IndentWriter()
        Dim fscope = parent.Push()

        ' generators collect their yield values eagerly and hand back an iterator
        Dim generatorVar As String = Nothing
        If f.IsGenerator Then
            generatorVar = UniqueName("gen")
            w.Line($"Dim {generatorVar} As New LiteJsRuntime.JsYieldCollector()")
        End If

        ' async functions hand back a promise settled by their return value
        Dim promiseVar As String = Nothing
        Dim catchVar As String = Nothing
        If f.IsAsync Then
            promiseVar = UniqueName("promise")
            catchVar = UniqueName("ax")
            w.Line($"Dim {promiseVar} As New LiteJsRuntime.JsPromise()")
            w.Line("Try")
            w.Indent()
        End If
        For i = 0 To f.Parameters.Length - 1
            Dim vbParam = fscope.DefineLocal(f.Parameters(i))
            If i < f.Defaults.Length AndAlso f.Defaults(i) IsNot Nothing Then
                ' a missing / explicitly-undefined argument falls back to the default
                Dim dflt = GenExpr(f.Defaults(i), fscope)
                w.Line($"Dim {vbParam} As JsValue = If({argsName}.Length > {i} AndAlso {argsName}({i}) IsNot JsRuntime.Undef, {argsName}({i}), {dflt})")
            Else
                w.Line($"Dim {vbParam} As JsValue = If({argsName}.Length > {i}, {argsName}({i}), JsRuntime.Undef)")
            End If
        Next
        If f.RestParam IsNot Nothing Then
            Dim vbRest = fscope.DefineLocal(f.RestParam)
            w.Line($"Dim {vbRest} As JsValue = JsRuntime.JsRestArgs({argsName}, {f.Parameters.Length})")
        End If
        If Not f.IsArrow Then
            ' 'this' and 'arguments': arrow functions inherit both lexically
            Dim vbThis = fscope.DefineLocal("this")
            w.Line($"Dim {vbThis} As JsValue = JsRuntime.CurrentThis")
            Dim vbArgs = fscope.DefineLocal("arguments")
            w.Line($"Dim {vbArgs} As JsValue = New List(Of Object)({argsName})")
        End If
        ' JS var hoisting: declare every 'var' of this function once at the top
        EmitHoistedVars(f.Body, w, fscope)

        If generatorVar IsNot Nothing Then
            _yieldCollector.Push(generatorVar)
            Try
                GenBlockInScope(f.Body, w, fscope)
            Finally
                _yieldCollector.Pop()
            End Try
            w.Line($"Return JsRuntime.JsGeneratorFrom({generatorVar})")
        ElseIf promiseVar Is Nothing Then
            GenBlockInScope(f.Body, w, fscope)
            w.Line("Return JsRuntime.Undef")
        Else
            _asyncPromise.Push(promiseVar)
            Try
                GenBlockInScope(f.Body, w, fscope)
            Finally
                _asyncPromise.Pop()
            End Try
            ' falling off the end fulfils with undefined
            w.Line($"{promiseVar}.Fulfill(JsRuntime.Undef)")
            w.Line($"Return {promiseVar}")
            w.UnIndent()
            w.Line($"Catch {catchVar} As LiteJsRuntime.JsRuntimeException")
            w.Indent()
            w.Line($"{promiseVar}.Reject({catchVar}.Payload)")
            w.Line($"Return {promiseVar}")
            w.UnIndent()
            w.Line("End Try")
        End If

        _loopExit.Clear() : _loopCont.Clear() : _labeledTargets.Clear()
        _asyncPromise.Clear() : _yieldCollector.Clear()
        For Each e In saveExit : _loopExit.Push(e) : Next
        For Each c In saveCont : _loopCont.Push(c) : Next
        For Each kv In saveLabels : _labeledTargets(kv.Key) = kv.Value : Next
        For Each a In saveAsync : _asyncPromise.Push(a) : Next
        For Each g In saveGen : _yieldCollector.Push(g) : Next
        Return w.ToString()
    End Function

    ' ======================================================= var hoisting (codegen)

    ''' <summary>
    ''' JS <c>var</c> is function-scoped while VB <c>Dim</c> is block-scoped, so every
    ''' <c>var</c> of a function body is declared once at the top as <c>Undef</c> and
    ''' the declaration site only assigns (see <see cref="GenStmt"/>).
    ''' </summary>
    Private Sub EmitHoistedVars(body As List(Of Statement), w As IndentWriter, scope As Scope)
        Dim names As New List(Of String)
        CollectHoistedVars(body, names, New HashSet(Of String))
        For Each name In names
            ' a parameter (or the 'arguments' binding) already owns the name
            If scope.IsDeclaredHere(name) Then Continue For
            Dim vbName = scope.DefineLocal(name)
            w.Line($"Dim {vbName} As JsValue = JsRuntime.Undef")
        Next
    End Sub

    ''' <summary>
    ''' Collect <c>var</c> names of a statement list. Never descends into nested
    ''' function declarations (their vars belong to their own function scope).
    ''' </summary>
    Private Sub CollectHoistedVars(stmts As List(Of Statement), names As List(Of String), seen As HashSet(Of String))
        For Each s In stmts
            CollectHoistedVars(s, names, seen)
        Next
    End Sub

    ''' <summary>Collect <c>var</c> names reachable from one statement (recursive).</summary>
    Private Sub CollectHoistedVars(s As Statement, names As List(Of String), seen As HashSet(Of String))
        Select Case True
            Case TypeOf s Is VarStmt
                Dim v = DirectCast(s, VarStmt)
                If v.Kind = "var" Then
                    For Each d In v.Declarations
                        CollectTargetNames(d.Target, names, seen)
                    Next
                End If
            Case TypeOf s Is BlockStmt
                CollectHoistedVars(DirectCast(s, BlockStmt).Body, names, seen)
            Case TypeOf s Is IfStmt
                Dim i = DirectCast(s, IfStmt)
                CollectHoistedVars(i.ThenBranch, names, seen)
                If i.ElseBranch IsNot Nothing Then CollectHoistedVars(i.ElseBranch, names, seen)
            Case TypeOf s Is ForStmt
                Dim fr = DirectCast(s, ForStmt)
                If TypeOf fr.Init Is VarStmt Then CollectHoistedVars(DirectCast(fr.Init, Statement), names, seen)
                CollectHoistedVars(fr.Body, names, seen)
            Case TypeOf s Is ForInStmt
                CollectHoistedVars(DirectCast(s, ForInStmt).Body, names, seen)
            Case TypeOf s Is ForOfStmt
                Dim fo = DirectCast(s, ForOfStmt)
                If fo.Kind = "var" Then CollectTargetNames(fo.Target, names, seen)
                CollectHoistedVars(fo.Body, names, seen)
            Case TypeOf s Is WhileStmt
                CollectHoistedVars(DirectCast(s, WhileStmt).Body, names, seen)
            Case TypeOf s Is DoWhileStmt
                CollectHoistedVars(DirectCast(s, DoWhileStmt).Body, names, seen)
            Case TypeOf s Is SwitchStmt
                For Each c In DirectCast(s, SwitchStmt).Cases
                    CollectHoistedVars(c.Body, names, seen)
                Next
            Case TypeOf s Is LabeledStmt
                CollectHoistedVars(DirectCast(s, LabeledStmt).Body, names, seen)
            Case TypeOf s Is TryStmt
                Dim t = DirectCast(s, TryStmt)
                CollectHoistedVars(t.TryBlock, names, seen)
                If t.CatchBlock IsNot Nothing Then CollectHoistedVars(t.CatchBlock, names, seen)
                If t.FinallyBlock IsNot Nothing Then CollectHoistedVars(t.FinallyBlock, names, seen)
            Case TypeOf s Is FuncDeclStmt, TypeOf s Is ClassDeclStmt
                ' their own scope — never hoisted into the enclosing function
        End Select
    End Sub

    ''' <summary>Collect the plain names bound by a (possibly destructuring) target.</summary>
    Private Shared Sub CollectTargetNames(target As Object, names As List(Of String), seen As HashSet(Of String))
        If target Is Nothing Then Return
        If TypeOf target Is String Then
            If seen.Add(CStr(target)) Then names.Add(CStr(target))
            Return
        End If
        Select Case True
            Case TypeOf target Is NameTarget
                Dim n = DirectCast(target, NameTarget).Name
                If seen.Add(n) Then names.Add(n)
            Case TypeOf target Is ArrayTarget
                Dim at = DirectCast(target, ArrayTarget)
                For Each el In at.Elements
                    CollectTargetNames(el, names, seen)
                Next
                CollectTargetNames(at.Rest, names, seen)
            Case TypeOf target Is ObjectTarget
                Dim ot = DirectCast(target, ObjectTarget)
                For Each prop In ot.Properties
                    CollectTargetNames(prop.Value, names, seen)
                Next
                CollectTargetNames(ot.Rest, names, seen)
            Case TypeOf target Is DefaultTarget
                CollectTargetNames(DirectCast(target, DefaultTarget).Target, names, seen)
        End Select
    End Sub

    ' ======================================================= literals / identifiers

    Private Shared Function LiteralText(v As Object) As String
        If v Is Nothing Then Return "CType(Nothing, JsValue)"
        If TypeOf v Is Double Then
            Dim d = CDbl(v)
            If Double.IsNaN(d) Then Return "Double.NaN"
            If Double.IsPositiveInfinity(d) Then Return "Double.PositiveInfinity"
            If Double.IsNegativeInfinity(d) Then Return "Double.NegativeInfinity"
            Return d.ToString("R", CultureInfo.InvariantCulture) & "R"
        End If
        If TypeOf v Is Boolean Then Return If(CBool(v), "True", "False")
        If TypeOf v Is String Then Return VbString(CStr(v))
        If ReferenceEquals(v, Runtime.JsRuntime.Undef) Then Return "JsRuntime.Undef"
        Throw New NotSupportedException("unsupported literal")
    End Function

    ''' <summary>JS string → VB string literal; control characters via ChrW segments.</summary>
    Private Shared Function VbString(s As String) As String
        Dim parts As New List(Of String)
        Dim plain As New StringBuilder()
        For Each c In s
            If c = """"c Then
                plain.Append("""""")
            ElseIf AscW(c) >= 32 AndAlso AscW(c) < 127 Then
                plain.Append(c)
            Else
                If plain.Length > 0 Then
                    parts.Add("""" & plain.ToString() & """")
                    plain.Clear()
                End If
                parts.Add($"ChrW({AscW(c)})")
            End If
        Next
        If plain.Length > 0 Then parts.Add("""" & plain.ToString() & """")
        If parts.Count = 0 Then Return """"""
        If parts.Count = 1 AndAlso parts(0).StartsWith("ChrW(", StringComparison.Ordinal) Then
            Return "CStr(" & parts(0) & ")"     ' a lone control char must still be a String
        End If
        Return String.Join(" & ", parts)
    End Function

    Private Function ResolveIdent(name As String, scope As Scope) As String
        Dim local = scope.Resolve(name)
        If local IsNot Nothing Then Return local
        Select Case name
            Case "console" : Return "JsRuntime.ConsoleBuiltin"
            Case "Math" : Return "JsRuntime.MathBuiltin"
            Case "JSON" : Return "JsRuntime.Json"
            Case "Object" : Return "JsRuntime.ObjectBuiltin"
            Case "Array" : Return "JsRuntime.ArrayBuiltin"
            Case "Function" : Return "JsRuntime.FunctionBuiltin"
            Case "Date" : Return "JsRuntime.DateBuiltin"
            Case "RegExp" : Return "JsRuntime.RegExpBuiltin"
            Case "Error" : Return "JsRuntime.ErrorBuiltin"
            Case "Promise" : Return "JsRuntime.PromiseBuiltin"
            Case "TypeError", "RangeError", "SyntaxError", "ReferenceError"
                Return $"JsRuntime.JsGet(JsRuntime.GlobalObj, {VbString(name)})"
            Case "undefined" : Return "JsRuntime.Undef"
            Case "NaN" : Return "Double.NaN"
            Case "Infinity" : Return "Double.PositiveInfinity"
            Case "parseInt", "parseFloat", "isNaN", "isFinite", "String", "Number", "Boolean"
                Return $"JsRuntime.JsGet(JsRuntime.GlobalObj, {VbString(name)})"
        End Select
        Throw New NotSupportedException($"identifier '{name}' is not defined")
    End Function

    Private Function UniqueName(base As String) As String
        _temp += 1
        Return $"{base}{_temp}"
    End Function

    ' ======================================================= helpers

    ''' <summary>Line writer with indentation, used for output and lambda bodies.</summary>
    Friend NotInheritable Class IndentWriter

        Private ReadOnly _sb As New StringBuilder()
        Private _level As Integer

        Public Sub Indent()
            _level += 1
        End Sub

        Public Sub UnIndent()
            _level -= 1
        End Sub

        Public Sub Line(Optional text As String = "")
            _sb.AppendLine(If(text = "", "", New String(" "c, _level * 4) & text))
        End Sub

        Public Sub WriteLine()
            _sb.AppendLine()
        End Sub

        Public Sub Paste(text As String)
            For Each ln In text.Replace(vbCrLf, vbLf).Split(ChrW(10))
                If ln = "" Then
                    _sb.AppendLine()
                Else
                    _sb.AppendLine(New String(" "c, _level * 4) & ln.TrimEnd())
                End If
            Next
        End Sub

        Public Overrides Function ToString() As String
            Return _sb.ToString()
        End Function

    End Class

    ''' <summary>
    ''' Lexical-scope map JS name → unique VB identifier.
    ''' <para>
    ''' VB hoists every closure's locals into a shared display class per
    ''' method, so identifiers must be unique across the WHOLE generated
    ''' method — sibling lambdas included (BC30616). The root scope owns a
    ''' method-wide "used names" set; shadowing still works via renaming.
    ''' </para>
    ''' </summary>
    Friend NotInheritable Class Scope

        Private ReadOnly _parent As Scope
        Private ReadOnly _names As New Dictionary(Of String, String)
        Private ReadOnly _used As HashSet(Of String)   ' method-wide (root only)
        Private _counter As Integer

        Private Sub New(parent As Scope, used As HashSet(Of String))
            _parent = parent
            _used = used
        End Sub

        Shared Function NewRoot() As Scope
            Return New Scope(Nothing, New HashSet(Of String)())
        End Function

        Public Function Push() As Scope
            Return New Scope(Me, _used)
        End Function

        ''' <summary>Declare (or re-use) a local in THIS scope; returns the VB identifier.</summary>
        Public Function DefineLocal(name As String) As String
            Dim existing As String = Nothing
            If _names.TryGetValue(name, existing) Then Return existing   ' JS var re-declaration
            Dim vb = SafeIdent(name)
            ' rename when the JS name shadows an outer JS variable, or when the
            ' VB identifier is already taken anywhere in the generated method
            If Not IsFreeInChain(name) Then
                vb = UniqueVariant(name)
            ElseIf Not _used.Add(vb) Then
                vb = UniqueVariant(name)
                _used.Add(vb)
            End If
            _names(name) = vb
            Return vb
        End Function

        Private Function UniqueVariant(name As String) As String
            Dim vb As String
            Do
                _counter += 1
                vb = SafeIdent(name) & "_" & _counter
            Loop While _used.Contains(vb)
            Return vb
        End Function

        Private Function IsFreeInChain(name As String) As Boolean
            Dim s = Me
            While s IsNot Nothing
                If s._names.ContainsKey(name) Then Return False
                s = s._parent
            End While
            Return True
        End Function

        Private Function UniqueSuffix() As Integer
            Dim s = Me
            While s._parent IsNot Nothing
                s = s._parent
            End While
            s._counter += 1
            Return s._counter
        End Function

        Public Function Resolve(name As String) As String
            Dim s = Me
            While s IsNot Nothing
                Dim v As String = Nothing
                If s._names.TryGetValue(name, v) Then Return v
                s = s._parent
            End While
            Return Nothing
        End Function

        ''' <summary>True when <paramref name="name"/> is declared in THIS scope only.</summary>
        Public Function IsDeclaredHere(name As String) As Boolean
            Return _names.ContainsKey(name)
        End Function

        Public Function ResolveOrThrow(name As String) As String
            Dim v = Resolve(name)
            If v Is Nothing Then Throw New NotSupportedException($"variable '{name}' must be declared before use")
            Return v
        End Function

        Private Shared Function SafeIdent(name As String) As String
            Return If(VbKeywords.Contains(name), "[" & name & "]", name)
        End Function

        Private Shared ReadOnly VbKeywords As New HashSet(Of String) From {
            "addhandler", "addressof", "alias", "and", "andalso", "as", "boolean", "byref",
            "byte", "byval", "call", "case", "catch", "char", "class", "const", "continue",
            "date", "decimal", "declare", "default", "delegate", "dim", "do", "double",
            "each", "else", "elseif", "end", "enum", "erase", "error", "event", "exit",
            "false", "finally", "for", "friend", "function", "get", "gettype", "getxmlnamespace",
            "global", "goto", "handles", "if", "implements", "imports", "in", "inherits",
            "integer", "interface", "is", "isnot", "let", "lib", "like", "long", "loop",
            "me", "mod", "module", "mustinherit", "mustoverride", "mybase", "myclass",
            "namespace", "narrowing", "new", "next", "not", "nothing", "notinheritable",
            "notoverridable", "object", "of", "on", "operator", "option", "optional",
            "or", "orelse", "overloads", "overridable", "overrides", "paramarray",
            "partial", "private", "property", "protected", "public", "raiseevent",
            "readonly", "redim", "rem", "resume", "return", "select", "set", "shadows",
            "shared", "short", "single", "static", "step", "stop", "string", "structure",
            "sub", "synclock", "then", "throw", "to", "true", "try", "typeof", "until",
            "using", "when", "widening", "while", "with", "withevents", "writeonly", "xor",
            "aggregate", "distinct", "equals", "from", "group", "into", "join", "order",
            "skip", "take", "where", "andalso", "orelse"
        }

    End Class

End Class
