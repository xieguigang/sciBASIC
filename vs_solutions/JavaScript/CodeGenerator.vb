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

    Public Function Generate(program As Program, Optional className As String = "GeneratedScript") As String
        _temp = 0
        _label = 0
        _loopExit.Clear() : _loopCont.Clear()
        Dim w As New IndentWriter()
        Dim top = Scope.NewRoot()

        w.Line("' Auto-generated from JavaScript by LiteJs — do not edit.")
        w.Line("' All dynamic semantics come from LiteJs.Runtime.JsRuntime (shared with the interpreter).")
        w.Line("Option Strict On")
        w.Line("Option Explicit On")
        w.Line("Option Infer On")
        w.Line()
        w.Line("Imports System")
        w.Line("Imports LiteJs.Runtime")
        w.Line("Imports JsValue = System.Object")
        w.Line("' function values are Func(Of Object(), Object); VB Imports aliases cannot target generic types")
        w.Line()
        w.Line($"Public NotInheritable Class {className}")
        w.Indent()
        w.Line()
        w.Line("Public Shared Sub Run(io As LiteJs.Runtime.ScriptIO)")
        w.Indent()
        w.Line("JsRuntime.Io = io")
        GenBlock(program.Body, w, top)
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
                Dim vbName = scope.DefineLocal(d.Name)
                Dim init = If(d.Init IsNot Nothing, GenExpr(d.Init, scope), "JsRuntime.Undef")
                w.Line($"Dim {vbName} As JsValue = {init}")
            Next

        ElseIf TypeOf s Is ExprStmt Then
            GenExprAsStatement(DirectCast(s, ExprStmt).Expr, w, scope)

        ElseIf TypeOf s Is BlockStmt Then
            GenBlock(DirectCast(s, BlockStmt).Body, w, scope)

        ElseIf TypeOf s Is IfStmt Then
            GenIf(DirectCast(s, IfStmt), w, scope)

        ElseIf TypeOf s Is WhileStmt Then
            Dim n = DirectCast(s, WhileStmt)
            w.Line($"While JsTruthy({GenExpr(n.Test, scope)})")
            w.Indent()
            _loopExit.Push("Exit While") : _loopCont.Push("Continue While")
            GenBody(n.Body, w, scope)
            _loopCont.Pop() : _loopExit.Pop()
            w.UnIndent()
            w.Line("End While")

        ElseIf TypeOf s Is DoWhileStmt Then
            Dim n = DirectCast(s, DoWhileStmt)
            w.Line("Do")
            w.Indent()
            _loopExit.Push("Exit Do") : _loopCont.Push("Continue Do")
            GenBody(n.Body, w, scope)
            _loopCont.Pop() : _loopExit.Pop()
            w.UnIndent()
            w.Line($"Loop While JsTruthy({GenExpr(n.Test, scope)})")

        ElseIf TypeOf s Is ForStmt Then
            Dim n = DirectCast(s, ForStmt)
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
            w.Line($"If Not JsTruthy({GenExpr(n.Test, scope)}) Then Exit Do")
            _loopExit.Push("Exit Do") : _loopCont.Push($"GoTo {updLabel}")
            GenBody(n.Body, w, scope)
            _loopCont.Pop() : _loopExit.Pop()
            w.Line($"{updLabel}:")
            If n.Update IsNot Nothing Then GenExprAsStatement(n.Update, w, scope)
            w.UnIndent()
            w.Line("Loop")

        ElseIf TypeOf s Is ForInStmt Then
            Dim n = DirectCast(s, ForInStmt)
            Dim vbVar = scope.DefineLocal(n.VarName)
            w.Line($"For Each {vbVar} As JsValue In JsRuntime.JsForInKeys({GenExpr(n.ObjectExpr, scope)})")
            w.Indent()
            _loopExit.Push("Exit For") : _loopCont.Push("Continue For")
            GenBody(n.Body, w, scope)
            _loopCont.Pop() : _loopExit.Pop()
            w.UnIndent()
            w.Line("Next")

        ElseIf TypeOf s Is BreakStmt Then
            w.Line(If(_loopExit.Count > 0, _loopExit.Peek(), "Exit Do"))

        ElseIf TypeOf s Is ContinueStmt Then
            w.Line(If(_loopCont.Count > 0, _loopCont.Peek(), "Continue Do"))

        ElseIf TypeOf s Is ReturnStmt Then
            Dim r = DirectCast(s, ReturnStmt)
            w.Line($"Return {If(r.Value IsNot Nothing, GenExpr(r.Value, scope), "JsRuntime.Undef")}")

        ElseIf TypeOf s Is ThrowStmt Then
            w.Line($"Throw New LiteJs.Runtime.JsRuntimeException({GenExpr(DirectCast(s, ThrowStmt).Value, scope)})")

        ElseIf TypeOf s Is TryStmt Then
            GenTry(DirectCast(s, TryStmt), w, scope)

        ElseIf TypeOf s Is EmptyStmt Then
            ' nothing

        Else
            Throw New NotSupportedException($"unsupported statement {s.GetType().Name}")
        End If
    End Sub

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
            w.Line($"Catch {exName} As LiteJs.Runtime.JsRuntimeException")
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
        w.Paste(LambdaBody(f.Parameters, f.Body, scope, argsName))
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
            Dim x = GenExpr(u.Operand, scope)
            Select Case u.Op
                Case "-" : Return $"JsRuntime.JsNeg({x})"
                Case "+" : Return $"JsRuntime.JsNum({x})"
                Case "!" : Return $"JsRuntime.JsNot({x})"
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
            End Select
            Throw New NotSupportedException("binary " & b.Op)

        ElseIf TypeOf e Is LogicalExpr Then
            Dim l = DirectCast(e, LogicalExpr)
            Dim x = GenExpr(l.Left, scope)
            Dim yLazy = $"Function() {GenExpr(l.Right, scope)}"
            If l.Op = "&&" Then Return $"JsRuntime.JsAnd({x}, {yLazy})"
            Return $"JsRuntime.JsOr({x}, {yLazy})"

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
            Return $"JsRuntime.JsInvoke({GenExpr(c.Callee, scope)}, {GenArgs(c.Arguments, scope)})"

        ElseIf TypeOf e Is MemberExpr Then
            Dim m = DirectCast(e, MemberExpr)
            Dim objTxt = GenExpr(m.Obj, scope)
            If m.Computed Then
                Return $"JsRuntime.JsIndex({objTxt}, {GenExpr(DirectCast(m.Name, Expression), scope)})"
            End If
            Return $"JsRuntime.JsGet({objTxt}, {VbString(CStr(m.Name))})"

        ElseIf TypeOf e Is ArrayExpr Then
            Dim a = DirectCast(e, ArrayExpr)
            If a.Elements.Count = 0 Then Return "JsRuntime.JsArray()"
            Dim items = a.Elements.Select(Function(x) GenExpr(x, scope)).ToArray()
            Return "JsRuntime.JsArray(" & String.Join(", ", items) & ")"

        ElseIf TypeOf e Is ObjectExpr Then
            Dim o = DirectCast(e, ObjectExpr)
            If o.Properties.Count = 0 Then Return "JsRuntime.JsObj(New (String, Object)() {})"
            Dim pairs = o.Properties.
                Select(Function(p) $"({VbString(p.Key)}, {GenExpr(p.Value, scope)})").ToArray()
            Return "JsRuntime.JsObj(New (String, Object)() {" & String.Join(", ", pairs) & "})"

        ElseIf TypeOf e Is FuncExpr Then
            Dim f = DirectCast(e, FuncExpr)
            Dim argsName = ArgsParam()
            Return "CType(Function(" & argsName & " As JsValue())" & vbLf &
                   LambdaBody(f.Parameters, f.Body, scope, argsName) & vbLf &
                   "End Function, Func(Of Object(), Object))"
        End If

        Throw New NotSupportedException("unsupported expression " & e.GetType().Name)
    End Function

    Private Function GenArgs(args As List(Of Expression), scope As Scope) As String
        If args.Count = 0 Then Return "New JsValue() {}"
        Dim items = args.Select(Function(a) GenExpr(a, scope)).ToArray()
        Return "New JsValue() {" & String.Join(", ", items) & "}"
    End Function

    ' ======================================================= lambda bodies

    Private Function ArgsParam() As String
        _temp += 1
        Return $"args__{_temp}"
    End Function

    ''' <summary>Parameter binding + function-hoisted block + trailing Return Undef, as plain lines.</summary>
    Private Function LambdaBody(parameters As String(), body As List(Of Statement), parent As Scope, argsName As String) As String
        ' a function body is a loop boundary in JS: break/continue inside can
        ' only target the function's own loops, never the caller's
        Dim saveExit = _loopExit.ToArray()
        Dim saveCont = _loopCont.ToArray()
        _loopExit.Clear() : _loopCont.Clear()
        Dim w As New IndentWriter()
        Dim fscope = parent.Push()
        For i = 0 To parameters.Length - 1
            Dim vbParam = fscope.DefineLocal(parameters(i))
            w.Line($"Dim {vbParam} As JsValue = If({argsName}.Length > {i}, {argsName}({i}), JsRuntime.Undef)")
        Next
        GenBlockInScope(body, w, fscope)
        w.Line("Return JsRuntime.Undef")
        _loopExit.Clear() : _loopCont.Clear()
        For Each e In saveExit : _loopExit.Push(e) : Next
        For Each c In saveCont : _loopCont.Push(c) : Next
        Return w.ToString()
    End Function

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
