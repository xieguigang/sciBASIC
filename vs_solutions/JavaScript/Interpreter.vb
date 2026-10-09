Imports Microsoft.VisualBasic.ApplicationServices.VM.JavaScript.Runtime

''' <summary>
''' Tree-walking interpreter over the AST: environment chains, closures,
''' block scoping with function hoisting, and control-flow via internal
''' signal exceptions (break/continue/return — these are NOT
''' <see cref="JsRuntimeException"/>s, so JS try/catch cannot swallow them,
''' matching JS semantics).
''' </summary>
Public NotInheritable Class Interpreter

    Private ReadOnly _globals As Environment
    Public ReadOnly Io As ScriptIO

    Public Sub New(Optional io As ScriptIO = Nothing)
        Me.Io = If(io, New ScriptIO())
        JsRuntime.Io = Me.Io          ' builtin dispatch (console.log) reads the static sink
        _globals = New Environment(Nothing)
        For Each kv In JsRuntime.GlobalIdentifiers()
            _globals.Define(kv.Key, kv.Value, False)
        Next
    End Sub

    ''' <summary>Run a parsed program; output goes to <see cref="Io"/>.</summary>
    Public Sub Run(program As Program)
        HoistFunctions(program.Body, _globals)
        ExecuteBlock(program.Body, _globals)
    End Sub

    ''' <summary>
    ''' Register a host-side value (e.g. a <see cref="Func(Of Object(), Object)"/>
    ''' delegate wrapping a .NET method) as a global identifier visible to scripts.
    ''' </summary>
    ''' <param name="name">the global name that the script references.</param>
    ''' <param name="value">the host value/closure to bind.</param>
    Public Sub DefineGlobal(name As String, value As Object)
        _globals.Define(name, value, False)
    End Sub

    ' ---------------- control-flow signals ----------------

    Private NotInheritable Class BreakSignal
        Inherits Exception
    End Class

    Private NotInheritable Class ContinueSignal
        Inherits Exception
    End Class

    Private NotInheritable Class ReturnSignal
        Inherits Exception
        Public ReadOnly Value As Object
        Public Sub New(value As Object)
            MyBase.New()
            Me.Value = value
        End Sub
    End Class

    ' ---------------- environments ----------------

    ''' <summary>Lexical scope: name → (value, isConst); undeclared writes go to the global scope.</summary>
    Public NotInheritable Class Environment

        Private ReadOnly _parent As Environment
        Private ReadOnly _slots As New Dictionary(Of String, Slot)

        Public Sub New(parent As Environment)
            _parent = parent
        End Sub

        Private Class Slot
            Public Value As Object
            Public ReadOnly IsConst As Boolean
            Public Sub New(value As Object, isConst As Boolean)
                Me.Value = value
                Me.IsConst = isConst
            End Sub
        End Class

        Public Function TryGet(name As String, ByRef value As Object) As Boolean
            Dim e = Me
            While e IsNot Nothing
                Dim s As Slot = Nothing
                If e._slots.TryGetValue(name, s) Then
                    value = s.Value
                    Return True
                End If
                e = e._parent
            End While
            Return False
        End Function

        Public Sub Define(name As String, value As Object, isConst As Boolean)
            ' re-declaration in the same block: overwrite (JS var semantics)
            _slots(name) = New Slot(value, isConst)
        End Sub

        Public Sub Assign(name As String, value As Object)
            Dim e = Me
            While e IsNot Nothing
                Dim s As Slot = Nothing
                If e._slots.TryGetValue(name, s) Then
                    If s.IsConst Then
                        Throw JsRuntimeException.TypeError("Assignment to constant variable '" & name & "'")
                    End If
                    s.Value = value
                    Return
                End If
                e = e._parent
            End While
            ' non-strict JS: implicit global
            Dim g = Me
            While g._parent IsNot Nothing
                g = g._parent
            End While
            g._slots(name) = New Slot(value, False)
        End Sub

        Public Function LookupOrThrow(name As String) As Object
            Dim v As Object = Nothing
            If TryGet(name, v) Then Return v
            Throw JsRuntimeException.ReferenceError(name)
        End Function
    End Class

    ' ---------------- statements ----------------

    Private Sub HoistFunctions(body As List(Of Statement), env As Environment)
        For Each s In body
            If TypeOf s Is FuncDeclStmt Then
                Dim f = DirectCast(s, FuncDeclStmt)
                env.Define(f.Name, MakeFunction(New FuncExpr(f.Name, f.Parameters, f.Body), env), False)
            End If
        Next
    End Sub

    Private Sub ExecuteBlock(body As List(Of Statement), parent As Environment)
        Dim env As New Environment(parent)
        HoistFunctions(body, env)
        For Each s In body
            Execute(s, env)
        Next
    End Sub

    Private Sub Execute(s As Statement, env As Environment)
        Select Case True
            Case TypeOf s Is ExprStmt
                Eval(DirectCast(s, ExprStmt).Expr, env)
            Case TypeOf s Is VarStmt
                Dim v = DirectCast(s, VarStmt)
                For Each d In v.Declarations
                    Dim value = If(d.Init IsNot Nothing, Eval(d.Init, env), JsRuntime.Undef)
                    env.Define(d.Name, value, v.Kind = "const")
                Next
            Case TypeOf s Is BlockStmt
                ExecuteBlock(DirectCast(s, BlockStmt).Body, env)
            Case TypeOf s Is FuncDeclStmt
                ' already hoisted; the statement itself does nothing
            Case TypeOf s Is IfStmt
                Dim i = DirectCast(s, IfStmt)
                If JsRuntime.JsTruthy(Eval(i.Test, env)) Then
                    Execute(i.ThenBranch, env)
                ElseIf i.ElseBranch IsNot Nothing Then
                    Execute(i.ElseBranch, env)
                End If
            Case TypeOf s Is WhileStmt
                Dim w = DirectCast(s, WhileStmt)
                While JsRuntime.JsTruthy(Eval(w.Test, env))
                    Try
                        Execute(w.Body, env)
                    Catch ex As BreakSignal
                        Exit While
                    Catch ex As ContinueSignal
                        ' next iteration
                    End Try
                End While
            Case TypeOf s Is DoWhileStmt
                Dim d = DirectCast(s, DoWhileStmt)
                Do
                    Try
                        Execute(d.Body, env)
                    Catch ex As BreakSignal
                        Exit Do
                    Catch ex As ContinueSignal
                        ' check condition and continue
                    End Try
                Loop While JsRuntime.JsTruthy(Eval(d.Test, env))
            Case TypeOf s Is ForStmt
                Dim f = DirectCast(s, ForStmt)
                Dim fenv As New Environment(env)
                If TypeOf f.Init Is VarStmt Then
                    Execute(DirectCast(f.Init, Statement), fenv)
                ElseIf TypeOf f.Init Is Expression Then
                    Eval(DirectCast(f.Init, Expression), fenv)
                End If
                Try
                    While f.Test Is Nothing OrElse JsRuntime.JsTruthy(Eval(f.Test, fenv))
                        Try
                            Execute(f.Body, fenv)
                        Catch ex As ContinueSignal
                            ' fall through to update (JS semantics)
                        End Try
                        If f.Update IsNot Nothing Then Eval(f.Update, fenv)
                    End While
                Catch ex As BreakSignal
                    ' loop done
                End Try
            Case TypeOf s Is ForInStmt
                Dim fin = DirectCast(s, ForInStmt)
                Dim obj = Eval(fin.ObjectExpr, env)
                For Each key In JsRuntime.JsForInKeys(obj)
                    Dim bodyEnv As New Environment(env)
                    bodyEnv.Define(fin.VarName, key, False)
                    Try
                        Execute(fin.Body, bodyEnv)
                    Catch ex As BreakSignal
                        Exit For
                    Catch ex As ContinueSignal
                        ' next key
                    End Try
                Next
            Case TypeOf s Is ReturnStmt
                Dim r = DirectCast(s, ReturnStmt)
                Throw New ReturnSignal(If(r.Value IsNot Nothing, Eval(r.Value, env), JsRuntime.Undef))
            Case TypeOf s Is BreakStmt
                Throw New BreakSignal()
            Case TypeOf s Is ContinueStmt
                Throw New ContinueSignal()
            Case TypeOf s Is ThrowStmt
                Dim t = DirectCast(s, ThrowStmt)
                Throw New JsRuntimeException(Eval(t.Value, env))
            Case TypeOf s Is TryStmt
                Dim t = DirectCast(s, TryStmt)
                Try
                    Try
                        ExecuteBlock(t.TryBlock, env)
                    Catch jex As JsRuntimeException
                        If t.CatchBlock IsNot Nothing Then
                            Dim cenv As New Environment(env)
                            If t.CatchParam IsNot Nothing Then
                                cenv.Define(t.CatchParam, jex.Payload, False)
                            End If
                            ExecuteBlock(t.CatchBlock, cenv)
                        Else
                            Throw
                        End If
                    End Try
                Finally
                    If t.FinallyBlock IsNot Nothing Then ExecuteBlock(t.FinallyBlock, env)
                End Try
            Case TypeOf s Is EmptyStmt
                ' nothing
            Case Else
                Throw New InvalidOperationException("unknown statement type " & s.GetType().Name)
        End Select
    End Sub

    ' ---------------- expressions ----------------

    Private Function MakeFunction(f As FuncExpr, env As Environment) As Func(Of Object(), Object)
        Dim closure = env
        Return Function(args As Object())
                   Dim fenv As New Environment(closure)
                   For i = 0 To f.Parameters.Length - 1
                       fenv.Define(f.Parameters(i),
                                   If(i < args.Length, args(i), JsRuntime.Undef), False)
                   Next
                   fenv.Define("arguments", New List(Of Object)(args), False)
                   HoistFunctions(f.Body, fenv)
                   Try
                       For Each st In f.Body
                           Execute(st, fenv)
                       Next
                   Catch r As ReturnSignal
                       Return r.Value
                   End Try
                   Return JsRuntime.Undef
               End Function
    End Function

    Private Function Eval(e As Expression, env As Environment) As Object
        Select Case True
            Case TypeOf e Is LiteralExpr
                Return DirectCast(e, LiteralExpr).Value

            Case TypeOf e Is IdentExpr
                Dim i = DirectCast(e, IdentExpr)
                Return env.LookupOrThrow(i.Name)

            Case TypeOf e Is UnaryExpr
                Dim u = DirectCast(e, UnaryExpr)
                If u.Op = "typeof" Then
                    ' special case: typeof undeclared → "undefined" without throwing
                    If TypeOf u.Operand Is IdentExpr Then
                        Dim nm = DirectCast(u.Operand, IdentExpr).Name
                        Dim v As Object = Nothing
                        If Not env.TryGet(nm, v) Then Return "undefined"
                        Return JsRuntime.JsTypeOf(v)
                    End If
                    Return JsRuntime.JsTypeOf(Eval(u.Operand, env))
                End If
                Dim operand = Eval(u.Operand, env)
                Select Case u.Op
                    Case "-" : Return JsRuntime.JsNeg(operand)
                    Case "+" : Return JsRuntime.JsNum(operand)
                    Case "!" : Return JsRuntime.JsNot(operand)
                End Select
                Throw New InvalidOperationException("unknown unary operator " & u.Op)

            Case TypeOf e Is UpdateExpr
                Dim up = DirectCast(e, UpdateExpr)
                Dim oldV = Eval(up.Target, env)
                Dim n = JsRuntime.JsNum(oldV)
                Dim [new] = If(up.Op = "++", n + 1.0, n - 1.0)
                AssignTo(up.Target, [new], env)
                Return If(up.IsPrefix, [new], oldV)

            Case TypeOf e Is BinaryExpr
                Dim b = DirectCast(e, BinaryExpr)
                Dim l = Eval(b.Left, env)
                Dim r = Eval(b.Right, env)
                Select Case b.Op
                    Case "+" : Return JsRuntime.JsAdd(l, r)
                    Case "-" : Return JsRuntime.JsSub(l, r)
                    Case "*" : Return JsRuntime.JsMul(l, r)
                    Case "/" : Return JsRuntime.JsDiv(l, r)
                    Case "%" : Return JsRuntime.JsMod(l, r)
                    Case "**" : Return JsRuntime.JsPow(l, r)
                    Case "<" : Return JsRuntime.JsLt(l, r)
                    Case "<=" : Return JsRuntime.JsLe(l, r)
                    Case ">" : Return JsRuntime.JsGt(l, r)
                    Case ">=" : Return JsRuntime.JsGe(l, r)
                    Case "==" : Return JsRuntime.JsEq(l, r)
                    Case "!=" : Return Not JsRuntime.JsEq(l, r)
                    Case "===" : Return JsRuntime.JsStrictEq(l, r)
                    Case "!==" : Return Not JsRuntime.JsStrictEq(l, r)
                End Select
                Throw New InvalidOperationException("unknown binary operator " & b.Op)

            Case TypeOf e Is LogicalExpr
                Dim l = DirectCast(e, LogicalExpr)
                Dim left = Eval(l.Left, env)
                If l.Op = "&&" Then
                    If Not JsRuntime.JsTruthy(left) Then Return left
                    Return Eval(l.Right, env)
                Else
                    If JsRuntime.JsTruthy(left) Then Return left
                    Return Eval(l.Right, env)
                End If

            Case TypeOf e Is CondExpr
                Dim c = DirectCast(e, CondExpr)
                If JsRuntime.JsTruthy(Eval(c.Test, env)) Then Return Eval(c.Consequent, env)
                Return Eval(c.Alternate, env)

            Case TypeOf e Is AssignExpr
                Dim a = DirectCast(e, AssignExpr)
                If a.Op = "=" Then
                    Dim v = Eval(a.Value, env)
                    AssignTo(a.Target, v, env)
                    Return v
                Else
                    ' compound: target = target OP value
                    Dim cur = Eval(a.Target, env)
                    Dim rhs = Eval(a.Value, env)
                    Dim v As Object
                    Select Case a.Op
                        Case "+=" : v = JsRuntime.JsAdd(cur, rhs)
                        Case "-=" : v = JsRuntime.JsSub(cur, rhs)
                        Case "*=" : v = JsRuntime.JsMul(cur, rhs)
                        Case "/=" : v = JsRuntime.JsDiv(cur, rhs)
                        Case "%=" : v = JsRuntime.JsMod(cur, rhs)
                        Case Else : Throw New InvalidOperationException("unknown assignment operator " & a.Op)
                    End Select
                    AssignTo(a.Target, v, env)
                    Return v
                End If

            Case TypeOf e Is CallExpr
                Dim c = DirectCast(e, CallExpr)
                Dim f = Eval(c.Callee, env)
                Dim args(c.Arguments.Count - 1) As Object
                For i = 0 To c.Arguments.Count - 1
                    args(i) = Eval(c.Arguments(i), env)
                Next
                Return JsRuntime.JsInvoke(f, args)

            Case TypeOf e Is MemberExpr
                Dim m = DirectCast(e, MemberExpr)
                Dim obj = Eval(m.Obj, env)
                If m.Computed Then
                    Return JsRuntime.JsIndex(obj, Eval(DirectCast(m.Name, Expression), env))
                End If
                Return JsRuntime.JsGet(obj, CStr(m.Name))

            Case TypeOf e Is FuncExpr
                Return MakeFunction(DirectCast(e, FuncExpr), env)

            Case TypeOf e Is ArrayExpr
                Dim arr = DirectCast(e, ArrayExpr)
                Dim list As New List(Of Object)
                For Each el In arr.Elements
                    list.Add(Eval(el, env))
                Next
                Return list

            Case TypeOf e Is ObjectExpr
                Dim o = DirectCast(e, ObjectExpr)
                Dim d As New Dictionary(Of String, Object)
                For Each p In o.Properties
                    d(p.Key) = Eval(p.Value, env)
                Next
                Return d
        End Select

        Throw New InvalidOperationException("unknown expression type " & e.GetType().Name)
    End Function

    Private Sub AssignTo(target As Expression, value As Object, env As Environment)
        If TypeOf target Is IdentExpr Then
            env.Assign(DirectCast(target, IdentExpr).Name, value)
        ElseIf TypeOf target Is MemberExpr Then
            Dim m = DirectCast(target, MemberExpr)
            Dim obj = Eval(m.Obj, env)
            If m.Computed Then
                JsRuntime.JsSetIndex(obj, Eval(DirectCast(m.Name, Expression), env), value)
            Else
                JsRuntime.JsSet(obj, CStr(m.Name), value)
            End If
        Else
            Throw New InvalidOperationException("invalid assignment target")
        End If
    End Sub

End Class
