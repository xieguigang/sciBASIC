Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices.VM.JavaScript.Runtime
Imports Microsoft.VisualBasic.Scripting.Runtime

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

    ''' <summary>Label of the statement currently being entered (see labeled break/continue).</summary>
    Private _pendingLabel As String

    Public Sub New(Optional io As ScriptIO = Nothing)
        Me.Io = If(io, New LogTextIO)
        JsRuntime.Io = Me.Io          ' builtin dispatch (console.log) reads the static sink
        ' the script global root is the outermost JS function scope (var hoisting target)
        _globals = New Environment(Nothing, isFunctionScope:=True)
        For Each kv In JsRuntime.GlobalIdentifiers()
            _globals.Define(kv.Key, kv.Value, False)
        Next
        ' host objects published before this engine was built (ClrInterop.Register / ClrHost)
        For Each name In JsRuntime.HostGlobalNames()
            Dim hostValue As Object = Nothing
            If ClrInterop.TryGetHostGlobal(name, hostValue) Then _globals.Define(name, hostValue, False)
        Next
    End Sub

    ''' <summary>Run a parsed program; output goes to <see cref="Io"/>.</summary>
    Public Sub Run(program As Program)
        HoistFunctions(program.Body, _globals)
        ExecuteBlock(program.Body, _globals)
        ' promise reactions queued while the body ran are microtasks — drain them
        ' now, mirroring a JS job queue that has no other pending work
        JsRuntime.DrainMicrotasks()
    End Sub

    ''' <summary>
    ''' Register a host-side value as a global identifier visible to scripts.
    ''' <para>
    ''' A <see cref="Func(Of Object(), Object)"/> delegate is bound as a script
    ''' function; any other CLR object (or a <see cref="Type"/>) is wrapped by the
    ''' reflection bridge, so the script can read/write its members and call its
    ''' methods (see <see cref="ClrInterop"/>).
    ''' </para>
    ''' </summary>
    ''' <param name="name">the global name that the script references.</param>
    ''' <param name="value">the host value/CLR instance to bind.</param>
    Public Sub DefineGlobal(name As String, value As Object)
        _globals.Define(name, ClrInterop.Wrap(value), False)
    End Sub

    ''' <summary>
    ''' Inject a batch of host CLR objects (see <see cref="ClrHost"/>): each is published
    ''' to the process-wide registry — which makes it resolvable from translated code as
    ''' well — and bound into this engine's globals.
    ''' </summary>
    Public Sub InstallHost(host As ClrHost)
        If host Is Nothing Then Return
        host.Install()
        For Each kv In host.Items
            _globals.Define(kv.Key, ClrInterop.Wrap(kv.Value), False)
        Next
    End Sub

    ' ---------------- control-flow signals ----------------

    Private NotInheritable Class BreakSignal
        Inherits Exception
        ''' <summary>Target label, or Nothing for the innermost loop/switch.</summary>
        Public ReadOnly Label As String
        Public Sub New(Optional label As String = Nothing)
            MyBase.New()
            Me.Label = label
        End Sub
    End Class

    Private NotInheritable Class ContinueSignal
        Inherits Exception
        ''' <summary>Target label, or Nothing for the innermost loop.</summary>
        Public ReadOnly Label As String
        Public Sub New(Optional label As String = Nothing)
            MyBase.New()
            Me.Label = label
        End Sub
    End Class

    Private NotInheritable Class ReturnSignal
        Inherits Exception
        Public ReadOnly Value As JsValue
        Public Sub New(value As JsValue)
            MyBase.New()
            Me.Value = value
        End Sub
    End Class

    ''' <summary>Take (and clear) the label announced by an enclosing labeled statement.</summary>
    Private Function TakePendingLabel() As String
        Dim l = _pendingLabel
        _pendingLabel = Nothing
        Return l
    End Function

    ''' <summary>True when a break/continue signal is aimed at this loop.</summary>
    Private Shared Function TargetsLoop(label As String, myLabel As String) As Boolean
        Return label Is Nothing OrElse label = myLabel
    End Function

    ' ---------------- statements ----------------

    Private Sub HoistFunctions(body As List(Of Statement), env As Environment)
        For Each s In body
            If TypeOf s Is FuncDeclStmt Then
                Dim f = DirectCast(s, FuncDeclStmt)
                env.Define(f.Name,
                           MakeFunction(New FuncExpr(f.Name, f.Parameters, f.Body,
                                                     defaults:=f.Defaults, restParam:=f.RestParam,
                                                     isAsync:=f.IsAsync, isGenerator:=f.IsGenerator), env),
                           False)
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
                    Dim value = If(d.Init IsNot Nothing, Eval(d.Init, env), JsValue.Undef)
                    BindTarget(d.Target, value, env, v.Kind)
                Next

            Case TypeOf s Is BlockStmt
                ExecuteBlock(DirectCast(s, BlockStmt).Body, env)

            Case TypeOf s Is FuncDeclStmt
                ' already hoisted; the statement itself does nothing

            Case TypeOf s Is ClassDeclStmt
                ' class declarations are block-scoped and NOT hoisted
                Dim spec = DirectCast(s, ClassDeclStmt).Spec
                env.Define(spec.Name, JsRuntime.JsUnbox(BuildClass(spec, env)), False)

            Case TypeOf s Is IfStmt
                Dim i = DirectCast(s, IfStmt)
                If JsRuntime.JsTruthy(Eval(i.Test, env)) Then
                    Execute(i.ThenBranch, env)
                ElseIf i.ElseBranch IsNot Nothing Then
                    Execute(i.ElseBranch, env)
                End If

            Case TypeOf s Is LabeledStmt
                Dim lb = DirectCast(s, LabeledStmt)
                ' announce the label: the loop statement that follows consumes it
                _pendingLabel = lb.Label
                Try
                    Execute(lb.Body, env)
                Catch ex As BreakSignal
                    ' a matching labeled break of a labeled (non-loop) statement
                    If ex.Label Is Nothing OrElse ex.Label <> lb.Label Then Throw
                Catch ex As ContinueSignal
                    ' labeled continue must target a loop; reaching here means unmatched
                    Throw
                Finally
                    _pendingLabel = Nothing
                End Try

            Case TypeOf s Is WhileStmt
                Dim w = DirectCast(s, WhileStmt)
                Dim myLabel = TakePendingLabel()
                While JsRuntime.JsTruthy(Eval(w.Test, env))
                    Try
                        Execute(w.Body, env)
                    Catch ex As BreakSignal
                        If TargetsLoop(ex.Label, myLabel) Then
                            Exit While
                        Else
                            Throw
                        End If
                    Catch ex As ContinueSignal
                        If Not TargetsLoop(ex.Label, myLabel) Then Throw
                    End Try
                End While

            Case TypeOf s Is DoWhileStmt
                Dim d = DirectCast(s, DoWhileStmt)
                Dim myLabel = TakePendingLabel()
                Do
                    Try
                        Execute(d.Body, env)
                    Catch ex As BreakSignal
                        If TargetsLoop(ex.Label, myLabel) Then
                            Exit Do
                        Else
                            Throw
                        End If
                    Catch ex As ContinueSignal
                        If Not TargetsLoop(ex.Label, myLabel) Then Throw
                    End Try
                Loop While JsRuntime.JsTruthy(Eval(d.Test, env))

            Case TypeOf s Is ForStmt
                Dim f = DirectCast(s, ForStmt)
                Dim myLabel = TakePendingLabel()
                Dim fenv As New Environment(env)
                If TypeOf f.Init Is VarStmt Then
                    Execute(DirectCast(f.Init, Statement), fenv)
                ElseIf TypeOf f.Init Is Expression Then
                    Eval(DirectCast(f.Init, Expression), fenv)
                End If
                ' let/const loop headers get a fresh binding per iteration (JS spec);
                ' var headers were already hoisted into the enclosing function scope
                Dim loopNames As New List(Of String)
                Dim loopConst As New List(Of Boolean)
                If TypeOf f.Init Is VarStmt Then
                    Dim lv = DirectCast(f.Init, VarStmt)
                    If lv.Kind <> "var" Then
                        For Each d In lv.Declarations
                            If TypeOf d.Target Is String Then
                                loopNames.Add(CStr(d.Target))
                            ElseIf TypeOf d.Target Is NameTarget Then
                                loopNames.Add(DirectCast(d.Target, NameTarget).Name)
                            End If
                            loopConst.Add(lv.Kind = "const")
                        Next
                    End If
                End If
                Try
                    While f.Test Is Nothing OrElse JsRuntime.JsTruthy(Eval(f.Test, fenv))
                        Dim bodyEnv As New Environment(env)
                        For i = 0 To loopNames.Count - 1
                            Dim lv As JsValue = JsValue.Undef
                            If fenv.TryGet(loopNames(i), lv) Then
                                bodyEnv.Define(loopNames(i), lv, loopConst(i))
                            End If
                        Next
                        Try
                            Execute(f.Body, bodyEnv)
                        Catch ex As ContinueSignal
                            If Not TargetsLoop(ex.Label, myLabel) Then Throw
                            ' fall through to update (JS semantics)
                        End Try
                        ' copy mutated let bindings back before running the update clause
                        For i = 0 To loopNames.Count - 1
                            Dim lv As JsValue = JsValue.Undef
                            If bodyEnv.TryGet(loopNames(i), lv) Then
                                fenv.Define(loopNames(i), lv, loopConst(i))
                            End If
                        Next
                        If f.Update IsNot Nothing Then Eval(f.Update, fenv)
                    End While
                Catch ex As BreakSignal
                    If Not TargetsLoop(ex.Label, myLabel) Then Throw
                    ' loop done
                End Try

            Case TypeOf s Is ForInStmt
                Dim fin = DirectCast(s, ForInStmt)
                Dim myLabel = TakePendingLabel()
                Dim obj = Eval(fin.ObjectExpr, env)
                For Each key In JsRuntime.JsForInKeys(JsRuntime.JsBox(obj))
                    Dim bodyEnv As New Environment(env)
                    bodyEnv.Define(fin.VarName, JsRuntime.JsUnbox(key), False)
                    Try
                        Execute(fin.Body, bodyEnv)
                    Catch ex As BreakSignal
                        If TargetsLoop(ex.Label, myLabel) Then
                            Exit For
                        Else
                            Throw
                        End If
                    Catch ex As ContinueSignal
                        If Not TargetsLoop(ex.Label, myLabel) Then Throw
                        ' next key
                    End Try
                Next

            Case TypeOf s Is ForOfStmt
                Dim fo = DirectCast(s, ForOfStmt)
                Dim myLabel = TakePendingLabel()
                Dim iter = JsRuntime.JsBox(Eval(fo.Iterable, env))
                For Each item In JsRuntime.JsIterableValues(iter)
                    Dim bodyEnv As New Environment(env)
                    BindTarget(fo.Target, JsRuntime.JsUnbox(item), bodyEnv, fo.Kind)
                    Try
                        Execute(fo.Body, bodyEnv)
                    Catch ex As BreakSignal
                        If TargetsLoop(ex.Label, myLabel) Then
                            Exit For
                        Else
                            Throw
                        End If
                    Catch ex As ContinueSignal
                        If Not TargetsLoop(ex.Label, myLabel) Then Throw
                        ' next item
                    End Try
                Next

            Case TypeOf s Is SwitchStmt
                ExecuteSwitch(DirectCast(s, SwitchStmt), env)

            Case TypeOf s Is ReturnStmt
                Dim r = DirectCast(s, ReturnStmt)
                Throw New ReturnSignal(If(r.Value IsNot Nothing, Eval(r.Value, env), JsValue.Undef))

            Case TypeOf s Is BreakStmt
                Throw New BreakSignal(DirectCast(s, BreakStmt).Label)

            Case TypeOf s Is ContinueStmt
                Throw New ContinueSignal(DirectCast(s, ContinueStmt).Label)

            Case TypeOf s Is ThrowStmt
                Dim t = DirectCast(s, ThrowStmt)
                Throw New JsRuntimeException(JsRuntime.JsBox(Eval(t.Value, env)))

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

    ''' <summary>
    ''' switch: strict-equality matching, one shared block scope, fall-through,
    ''' and 'break' (unlabeled) exiting the switch.
    ''' </summary>
    Private Sub ExecuteSwitch(sw As SwitchStmt, env As Environment)
        Dim disc = Eval(sw.Discriminant, env)
        Dim matched = -1

        ' case tests are evaluated in source order until one matches (indices are
        ' evaluated lazily — a match stops the scan, like the JS If-chain below)
        Dim defaultIndex = -1
        For i = 0 To sw.Cases.Count - 1
            If sw.Cases(i).Test Is Nothing Then
                defaultIndex = i
                Continue For
            End If
            If JsRuntime.JsStrictEq(disc, Eval(sw.Cases(i).Test, env)) Then
                matched = i
                Exit For
            End If
        Next
        If matched < 0 Then matched = defaultIndex
        If matched < 0 Then Return          ' no match and no default

        ' all arms share one block scope (JS switch semantics) so that
        ' fall-through can observe variables declared in earlier arms
        Dim swenv As New Environment(env)
        For Each c In sw.Cases
            HoistFunctions(c.Body, swenv)
        Next
        Try
            For i = matched To sw.Cases.Count - 1
                For Each st In sw.Cases(i).Body
                    Execute(st, swenv)
                Next
            Next
        Catch ex As BreakSignal
            If ex.Label IsNot Nothing Then Throw    ' labeled break targets an outer label
            ' plain break exits the switch
        End Try
    End Sub

    ' ---------------- destructuring / binding ----------------

    ''' <summary>
    ''' Bind a (possibly destructuring) target. <paramref name="kind"/> = "var" /
    ''' "let" / "const" declares; Nothing assigns to an existing reference.
    ''' </summary>
    Private Sub BindTarget(target As Object, value As JsValue, env As Environment, kind As String)
        Select Case True
            Case TypeOf target Is String
                BindName(CStr(target), value, env, kind)

            Case TypeOf target Is NameTarget
                BindName(DirectCast(target, NameTarget).Name, value, env, kind)

            Case TypeOf target Is MemberTarget
                Dim m = DirectCast(target, MemberTarget)
                Dim obj = JsRuntime.JsBox(Eval(m.Obj, env))
                If m.Computed Then
                    JsRuntime.JsSetIndex(obj,
                                         JsRuntime.JsBox(Eval(DirectCast(m.Name, Expression), env)),
                                         JsRuntime.JsBox(value))
                Else
                    JsRuntime.JsSet(obj, CStr(m.Name), JsRuntime.JsBox(value))
                End If

            Case TypeOf target Is ArrayTarget
                Dim at = DirectCast(target, ArrayTarget)
                Dim arr = JsRuntime.JsBox(value)
                For i = 0 To at.Elements.Count - 1
                    If at.Elements(i) Is Nothing Then Continue For    ' hole
                    BindTarget(at.Elements(i), JsRuntime.JsUnbox(JsRuntime.JsIndex(arr, CDbl(i))), env, kind)
                Next
                If at.Rest IsNot Nothing Then
                    BindTarget(at.Rest, JsRuntime.JsUnbox(JsRuntime.JsArrayRest(arr, at.Elements.Count)), env, kind)
                End If

            Case TypeOf target Is ObjectTarget
                Dim ot = DirectCast(target, ObjectTarget)
                Dim obj = JsRuntime.JsBox(value)
                Dim used As New List(Of String)
                For Each prop In ot.Properties
                    used.Add(prop.Key)
                    BindTarget(prop.Value, JsRuntime.JsUnbox(JsRuntime.JsGet(obj, prop.Key)), env, kind)
                Next
                If ot.Rest IsNot Nothing Then
                    BindTarget(ot.Rest,
                               JsRuntime.JsUnbox(JsRuntime.JsObjectRest(obj, used.ToArray())), env, kind)
                End If

            Case TypeOf target Is DefaultTarget
                Dim dt = DirectCast(target, DefaultTarget)
                Dim v = value
                If v.IsUndef Then v = Eval(dt.Default, env)     ' default when undefined
                BindTarget(dt.Target, v, env, kind)

            Case Else
                Throw New InvalidOperationException("unknown binding target")
        End Select
    End Sub

    Private Sub BindName(name As String, value As JsValue, env As Environment, kind As String)
        If kind Is Nothing Then
            env.Assign(name, value)
        ElseIf kind = "var" Then
            env.DefineVar(name, value)
        Else
            env.Define(name, value, kind = "const")
        End If
    End Sub

    ' ---------------- expressions ----------------

    Private Function MakeFunction(f As FuncExpr, env As Environment) As Func(Of Object(), Object)
        Dim closure = env
        If f.IsGenerator Then
            ' eager generator: the body runs to completion, collecting yield values
            Return Function(args As Object())
                       Return JsRuntime.JsRunGenerator(Function() InvokeBody(f, closure, args))
                   End Function
        End If
        If f.IsAsync Then
            ' async: the body runs synchronously, its completion settles the promise
            Return Function(args As Object())
                       Return JsRuntime.JsAsyncWrapper(Function() InvokeBody(f, closure, args))
                   End Function
        End If
        Return Function(args As Object())
                   Return InvokeBody(f, closure, args)
               End Function
    End Function

    ''' <summary>Bind parameters and run a function body (shared by sync and async paths).</summary>
    Private Function InvokeBody(f As FuncExpr, closure As Environment, args As Object()) As Object
        Dim fenv As New Environment(closure, isFunctionScope:=True)

        For i = 0 To f.Parameters.Length - 1
            Dim v As JsValue
            ' a missing or explicitly-undefined argument triggers the default
            If i >= args.Length OrElse args(i) Is JsRuntime.Undef Then
                If i < f.Defaults.Length AndAlso f.Defaults(i) IsNot Nothing Then
                    v = Eval(f.Defaults(i), fenv)
                Else
                    v = JsValue.Undef
                End If
            Else
                v = JsRuntime.JsUnbox(args(i))
            End If
            fenv.Define(f.Parameters(i), v, False)
        Next
        If f.RestParam IsNot Nothing Then
            Dim rest As New List(Of Object)
            For i = f.Parameters.Length To args.Length - 1
                rest.Add(args(i))
            Next
            fenv.Define(f.RestParam, JsValue.OfObject(rest), False)
        End If
        If Not f.IsArrow Then
            ' arrow functions inherit 'this' and 'arguments' lexically
            fenv.Define("this", JsRuntime.JsUnbox(JsRuntime.CurrentThis), False)
            fenv.Define("arguments", New List(Of Object)(args), False)
        End If

        HoistFunctions(f.Body, fenv)
        Try
            For Each st In f.Body
                Execute(st, fenv)
            Next
        Catch r As ReturnSignal
            Return JsRuntime.JsBox(r.Value)
        End Try
        Return JsRuntime.Undef
    End Function

    Private Function Eval(e As Expression, env As Environment) As JsValue
        Select Case True
            Case TypeOf e Is LiteralExpr
                Return DirectCast(e, LiteralExpr).Value

            Case TypeOf e Is IdentExpr
                Dim i = DirectCast(e, IdentExpr)
                Return env.LookupOrThrow(i.Name)

            Case TypeOf e Is ThisExpr
                ' lexical 'this' (defined in every non-arrow function scope); at the top
                ' level of a script it is the global object
                Dim tv As JsValue = Nothing
                If env.TryGet("this", tv) Then Return tv
                Return JsRuntime.JsUnbox(JsRuntime.CurrentThis)

            Case TypeOf e Is SuperExpr
                Dim sp = DirectCast(e, SuperExpr)
                If sp.Member Is Nothing Then Return JsValue.Undef     ' bare super is only a call
                Return JsRuntime.JsUnbox(JsRuntime.JsSuperGet(sp.Member))

            Case TypeOf e Is NewExpr
                Dim ne = DirectCast(e, NewExpr)
                Dim ctor = JsRuntime.JsBox(Eval(ne.Callee, env))
                Dim ctorArgs = EvalArguments(ne.Arguments, env)
                Return JsRuntime.JsUnbox(JsRuntime.JsNew(ctor, ctorArgs))

            Case TypeOf e Is ClassExpr
                Return JsValue.OfObject(BuildClass(DirectCast(e, ClassExpr).Spec, env))

            Case TypeOf e Is RegexExpr
                ' each evaluation yields a fresh RegExp object, like JS
                Dim rx = DirectCast(e, RegexExpr)
                Return JsValue.OfObject(New JsRegExp(rx.Source, rx.Flags))

            Case TypeOf e Is AwaitExpr
                ' await unwraps a promise, draining the microtask queue first
                Dim av = DirectCast(e, AwaitExpr)
                Return JsRuntime.JsUnbox(JsRuntime.JsAwait(JsRuntime.JsBox(Eval(av.Operand, env))))

            Case TypeOf e Is YieldExpr
                ' eager collection: the value is recorded and the expression yields it
                Dim yv = DirectCast(e, YieldExpr)
                Dim yvalue As Object = JsRuntime.Undef
                If yv.Operand IsNot Nothing Then yvalue = JsRuntime.JsBox(Eval(yv.Operand, env))
                Return JsRuntime.JsUnbox(JsRuntime.JsYieldValue(yvalue))

            Case TypeOf e Is UnaryExpr
                Dim u = DirectCast(e, UnaryExpr)
                If u.Op = "typeof" Then
                    ' special case: typeof undeclared → "undefined" without throwing
                    If TypeOf u.Operand Is IdentExpr Then
                        Dim nm = DirectCast(u.Operand, IdentExpr).Name
                        Dim v As JsValue = Nothing
                        If Not env.TryGet(nm, v) Then Return JsValue.Str("undefined")
                        Return JsValue.Str(JsRuntime.JsTypeOf(v))
                    End If
                    Return JsValue.Str(JsRuntime.JsTypeOf(Eval(u.Operand, env)))
                End If
                If u.Op = "delete" Then
                    If TypeOf u.Operand Is MemberExpr Then
                        Dim m = DirectCast(u.Operand, MemberExpr)
                        Dim obj = JsRuntime.JsBox(Eval(m.Obj, env))
                        Dim key As Object
                        If m.Computed Then
                            key = JsRuntime.JsBox(Eval(DirectCast(m.Name, Expression), env))
                        Else
                            key = CStr(m.Name)
                        End If
                        Return JsValue.Boolean_(JsRuntime.JsDelete(obj, key))
                    End If
                    ' deleting a non-reference: evaluate for side effects, always true
                    Eval(u.Operand, env)
                    Return JsValue.Boolean_(True)
                End If
                Dim operand = Eval(u.Operand, env)
                Select Case u.Op
                    Case "-" : Return JsRuntime.JsNeg(operand)
                    Case "+" : Return JsValue.Number(JsRuntime.JsNum(operand))
                    Case "!" : Return JsRuntime.JsNot(operand)
                    Case "~" : Return JsRuntime.JsBitNot(operand)
                    Case "void" : Return JsValue.Undef
                End Select
                Throw New InvalidOperationException("unknown unary operator " & u.Op)

            Case TypeOf e Is UpdateExpr
                Dim up = DirectCast(e, UpdateExpr)
                Dim oldV As JsValue = JsValue.Undef
                If TypeOf up.Target Is IdentExpr Then
                    Dim nm = DirectCast(up.Target, IdentExpr).Name
                    If Not env.TryGet(nm, oldV) Then
                        ' non-strict JS: x++ on an undeclared x assigns NaN to the implicit global
                        oldV = JsValue.Number(Double.NaN)
                    End If
                Else
                    oldV = Eval(up.Target, env)
                End If
                Dim n = JsRuntime.JsNum(oldV)
                Dim [new] = If(up.Op = "++", n + 1.0, n - 1.0)
                AssignTo(up.Target, JsValue.Number([new]), env)
                Return If(up.IsPrefix, JsValue.Number([new]), oldV)

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
                    Case "<" : Return JsValue.Boolean_(JsRuntime.JsLt(l, r))
                    Case "<=" : Return JsValue.Boolean_(JsRuntime.JsLe(l, r))
                    Case ">" : Return JsValue.Boolean_(JsRuntime.JsGt(l, r))
                    Case ">=" : Return JsValue.Boolean_(JsRuntime.JsGe(l, r))
                    Case "==" : Return JsValue.Boolean_(JsRuntime.JsEq(l, r))
                    Case "!=" : Return JsValue.Boolean_(JsRuntime.JsNe(l, r))
                    Case "===" : Return JsValue.Boolean_(JsRuntime.JsStrictEq(l, r))
                    Case "!==" : Return JsValue.Boolean_(JsRuntime.JsStrictNe(l, r))
                    Case "&" : Return JsRuntime.JsBitAnd(l, r)
                    Case "|" : Return JsRuntime.JsBitOr(l, r)
                    Case "^" : Return JsRuntime.JsBitXor(l, r)
                    Case "<<" : Return JsRuntime.JsShl(l, r)
                    Case ">>" : Return JsRuntime.JsShr(l, r)
                    Case ">>>" : Return JsRuntime.JsUshr(l, r)
                    Case "in" : Return JsValue.Boolean_(JsRuntime.JsIn(JsRuntime.JsBox(l), JsRuntime.JsBox(r)))
                    Case "instanceof" : Return JsValue.Boolean_(JsRuntime.JsInstanceOf(JsRuntime.JsBox(l), JsRuntime.JsBox(r)))
                End Select
                Throw New InvalidOperationException("unknown binary operator " & b.Op)

            Case TypeOf e Is LogicalExpr
                Dim l = DirectCast(e, LogicalExpr)
                Dim left = Eval(l.Left, env)
                Select Case l.Op
                    Case "&&"
                        If Not JsRuntime.JsTruthy(left) Then Return left
                        Return Eval(l.Right, env)
                    Case "||"
                        If JsRuntime.JsTruthy(left) Then Return left
                        Return Eval(l.Right, env)
                    Case Else ' ??
                        If Not JsRuntime.JsIsNullish(left) Then Return left
                        Return Eval(l.Right, env)
                End Select

            Case TypeOf e Is CondExpr
                Dim c = DirectCast(e, CondExpr)
                If JsRuntime.JsTruthy(Eval(c.Test, env)) Then Return Eval(c.Consequent, env)
                Return Eval(c.Alternate, env)

            Case TypeOf e Is SequenceExpr
                Dim sq = DirectCast(e, SequenceExpr)
                Dim v As JsValue = JsValue.Undef
                For Each x In sq.Expressions
                    v = Eval(x, env)
                Next
                Return v

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
                    Dim v As JsValue
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

            Case TypeOf e Is DestructureAssignExpr
                Dim d = DirectCast(e, DestructureAssignExpr)
                Dim v = Eval(d.Value, env)
                BindTarget(d.Target, v, env, Nothing)
                Return v

            Case TypeOf e Is CallExpr
                Return JsRuntime.JsUnbox(EvalCall(DirectCast(e, CallExpr), env))

            Case TypeOf e Is OptionalChainExpr
                ' chain root: a nullish optional link short-circuits to undefined
                Try
                    Return Eval(DirectCast(e, OptionalChainExpr).Chain, env)
                Catch ex As OptionalChainShortCircuit
                    Return JsValue.Undef
                End Try

            Case TypeOf e Is MemberExpr
                Dim m = DirectCast(e, MemberExpr)
                Dim obj = JsRuntime.JsBox(Eval(m.Obj, env))
                If m.Computed Then
                    Dim key = JsRuntime.JsBox(Eval(DirectCast(m.Name, Expression), env))
                    If m.IsOptional Then Return JsRuntime.JsUnbox(JsRuntime.JsChainIndex(obj, key))
                    Return JsRuntime.JsUnbox(JsRuntime.JsIndex(obj, key))
                End If
                If m.IsOptional Then Return JsRuntime.JsUnbox(JsRuntime.JsChainGet(obj, CStr(m.Name)))
                Return JsRuntime.JsUnbox(JsRuntime.JsGet(obj, CStr(m.Name)))

            Case TypeOf e Is FuncExpr
                Return JsValue.OfObject(MakeFunction(DirectCast(e, FuncExpr), env))

            Case TypeOf e Is TemplateExpr
                Dim t = DirectCast(e, TemplateExpr)
                Dim sb As New StringBuilder()
                For Each chunk In t.Chunks
                    If TypeOf chunk Is String Then
                        sb.Append(CStr(chunk))
                    Else
                        sb.Append(JsRuntime.JsStr(Eval(DirectCast(chunk, Expression), env)))
                    End If
                Next
                Return JsValue.Str(sb.ToString())

            Case TypeOf e Is ArrayExpr
                Return EvalArrayLiteral(DirectCast(e, ArrayExpr), env)

            Case TypeOf e Is ObjectExpr
                Return EvalObjectLiteral(DirectCast(e, ObjectExpr), env)
        End Select

        Throw New InvalidOperationException("unknown expression type " & e.GetType().Name)
    End Function

    ''' <summary>
    ''' Build a class value from its spec: methods and accessors go to the prototype,
    ''' static members onto the class, instance fields become per-construction
    ''' initialiser callables (so <c>this</c> binds to the new instance).
    ''' </summary>
    Private Function BuildClass(spec As ClassSpec, env As Environment) As Object
        Dim sup As Object = Nothing
        If spec.SuperClass IsNot Nothing Then sup = JsRuntime.JsBox(Eval(spec.SuperClass, env))

        Dim ctor As Object = Nothing
        If spec.Constructor IsNot Nothing Then ctor = MakeFunction(spec.Constructor, env)

        Dim parts As New List(Of (Kind As String, Name As String, Value As Object))
        For Each m In spec.Members
            Select Case m.Kind
                Case ClassMemberKind.Method
                    parts.Add((If(m.IsStatic, "s", "i"), m.Name,
                               MakeFunction(DirectCast(m.Value, FuncExpr), env)))
                Case ClassMemberKind.Getter
                    parts.Add((If(m.IsStatic, "s", "i"), m.Name,
                               New AccessorProperty(MakeFunction(DirectCast(m.Value, FuncExpr), env), Nothing)))
                Case ClassMemberKind.Setter
                    parts.Add((If(m.IsStatic, "s", "i"), m.Name,
                               New AccessorProperty(Nothing, MakeFunction(DirectCast(m.Value, FuncExpr), env))))
                Case ClassMemberKind.Field
                    If m.IsStatic Then
                        ' static field: evaluated once, at class definition time
                        Dim v As Object = JsRuntime.Undef
                        If m.Value IsNot Nothing Then
                            v = JsRuntime.JsBox(Eval(m.Value, New Environment(env, isFunctionScope:=True)))
                        End If
                        parts.Add(("s", m.Name, v))
                    Else
                        Dim init As Object = Nothing
                        If m.Value IsNot Nothing Then
                            ' wrap the initialiser as a zero-argument function so that
                            ' 'this' resolves to the instance being constructed
                            init = MakeFunction(New FuncExpr(Nothing, New String() {},
                                                             New List(Of Statement) From {New ReturnStmt(m.Value)}),
                                                env)
                        End If
                        parts.Add(("f", m.Name, init))
                    End If
            End Select
        Next

        Return JsRuntime.JsDefineClass(spec.Name, ctor, sup, parts.ToArray())
    End Function

    ''' <summary>
    ''' Evaluate a call: super(...), method calls (receiver bound to the owning
    ''' object for <c>this</c>) and plain calls.
    ''' </summary>
    Private Function EvalCall(c As CallExpr, env As Environment) As Object
        ' super(...) / super.m(...)
        Dim sup = TryCast(c.Callee, SuperExpr)
        If sup IsNot Nothing Then
            Dim supArgs = EvalArguments(c.Arguments, env)
            If sup.Member Is Nothing Then Return JsRuntime.JsSuperConstructor(supArgs)
            Return JsRuntime.JsSuperInvoke(sup.Member, supArgs)
        End If

        ' method call: this = the object the method was read from
        Dim m = TryCast(c.Callee, MemberExpr)
        If m IsNot Nothing Then
            Dim obj = JsRuntime.JsBox(Eval(m.Obj, env))
            Dim args1 = EvalArguments(c.Arguments, env)
            If m.Computed Then
                Dim key = JsRuntime.JsBox(Eval(DirectCast(m.Name, Expression), env))
                Return JsRuntime.JsInvokeIndex(obj, key, args1, m.IsOptional, c.IsOptional)
            End If
            Return JsRuntime.JsInvokeMethod(obj, CStr(m.Name), args1, m.IsOptional, c.IsOptional)
        End If

        Dim f = JsRuntime.JsBox(Eval(c.Callee, env))
        Dim args = EvalArguments(c.Arguments, env)
        If c.IsOptional Then
            Return JsRuntime.JsChainInvoke(f, args)
        End If
        Return JsRuntime.JsInvoke(f, args)
    End Function

    ''' <summary>Evaluate a call argument list, expanding spread elements when present.</summary>
    Private Function EvalArguments(args As List(Of Expression), env As Environment) As Object()
        Dim hasSpread = False
        For Each a In args
            If TypeOf a Is SpreadElement Then
                hasSpread = True
                Exit For
            End If
        Next

        If Not hasSpread Then
            Dim boxed(args.Count - 1) As Object
            For i = 0 To args.Count - 1
                boxed(i) = JsRuntime.JsBox(Eval(args(i), env))
            Next
            Return boxed
        End If

        Dim parts(args.Count - 1) As Object
        For i = 0 To args.Count - 1
            Dim a = args(i)
            If TypeOf a Is SpreadElement Then
                parts(i) = JsRuntime.JsSpread(JsRuntime.JsBox(Eval(DirectCast(a, SpreadElement).Argument, env)))
            Else
                parts(i) = JsRuntime.JsBox(Eval(a, env))
            End If
        Next
        Return JsRuntime.JsBuildArgs(parts)
    End Function

    Private Function EvalArrayLiteral(arr As ArrayExpr, env As Environment) As JsValue
        Dim hasSpread = False
        For Each el In arr.Elements
            If TypeOf el Is SpreadElement Then
                hasSpread = True
                Exit For
            End If
        Next

        If Not hasSpread Then
            Dim list As New List(Of Object)
            For Each el In arr.Elements
                If el Is Nothing Then
                    list.Add(JsRuntime.Undef)       ' hole reads as undefined
                Else
                    list.Add(JsRuntime.JsBox(Eval(el, env)))
                End If
            Next
            Return JsValue.OfObject(list)
        End If

        Dim parts(arr.Elements.Count - 1) As Object
        For i = 0 To arr.Elements.Count - 1
            Dim el = arr.Elements(i)
            If el Is Nothing Then
                parts(i) = JsRuntime.Undef
            ElseIf TypeOf el Is SpreadElement Then
                parts(i) = JsRuntime.JsSpread(JsRuntime.JsBox(Eval(DirectCast(el, SpreadElement).Argument, env)))
            Else
                parts(i) = JsRuntime.JsBox(Eval(el, env))
            End If
        Next
        Return JsRuntime.JsUnbox(JsRuntime.JsArrayFrom(parts))
    End Function

    Private Function EvalObjectLiteral(o As ObjectExpr, env As Environment) As JsValue
        Dim d As New Dictionary(Of String, Object)
        For Each p In o.Properties
            Select Case p.Kind
                Case ObjectPropertyKind.Spread
                    JsRuntime.JsSpreadInto(d, JsRuntime.JsBox(Eval(p.Value, env)))
                Case ObjectPropertyKind.Getter
                    ' merge with an existing setter for the same key
                    JsRuntime.JsAccessorDefine(d, p.Key, MakeFunction(DirectCast(p.Value, FuncExpr), env), Nothing)
                Case ObjectPropertyKind.Setter
                    JsRuntime.JsAccessorDefine(d, p.Key, Nothing, MakeFunction(DirectCast(p.Value, FuncExpr), env))
                Case ObjectPropertyKind.Shorthand
                    Dim v = If(p.Value Is Nothing, Eval(New IdentExpr(p.Key), env), Eval(p.Value, env))
                    d(p.Key) = JsRuntime.JsBox(v)
                Case Else ' Normal (incl. computed keys)
                    Dim key = If(p.KeyExpr IsNot Nothing,
                                 JsRuntime.JsStr(Eval(p.KeyExpr, env)),
                                 p.Key)
                    d(key) = JsRuntime.JsBox(Eval(p.Value, env))
            End Select
        Next
        Return JsValue.OfObject(d)
    End Function

    Private Sub AssignTo(target As Expression, value As JsValue, env As Environment)
        If TypeOf target Is IdentExpr Then
            env.Assign(DirectCast(target, IdentExpr).Name, value)
        ElseIf TypeOf target Is MemberExpr Then
            Dim m = DirectCast(target, MemberExpr)
            Dim obj = JsRuntime.JsBox(Eval(m.Obj, env))
            If m.Computed Then
                JsRuntime.JsSetIndex(obj, JsRuntime.JsBox(Eval(DirectCast(m.Name, Expression), env)), JsRuntime.JsBox(value))
            Else
                JsRuntime.JsSet(obj, CStr(m.Name), JsRuntime.JsBox(value))
            End If
        Else
            Throw New InvalidOperationException("invalid assignment target")
        End If
    End Sub

End Class
