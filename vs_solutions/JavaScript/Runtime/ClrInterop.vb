Imports System.Globalization
Imports System.Reflection
Imports System.Runtime.CompilerServices

Namespace Runtime

    ''' <summary>
    ''' The CLR reflection bridge: lets a host hand a .NET object (or type) to a script,
    ''' which then treats it like a plain JS object.
    ''' <para>
    ''' Host side: <see cref="Wrap"/> / <see cref="Register"/> / <see cref="ClrHost"/>, or
    ''' simply <c>Interpreter.DefineGlobal</c>. Script side: <c>obj.Name</c>,
    ''' <c>obj.Name = v</c>, <c>obj.Method(...)</c>, <c>obj[i]</c>, <c>for..of</c>,
    ''' <c>new T(...)</c>, <c>x instanceof T</c>. All member resolution funnels through
    ''' this module so the interpreter and translated code share one behaviour
    ''' (the engine's single-semantics-source rule).
    ''' </para>
    ''' <para>
    ''' Members are looked up by exact name (see <see cref="JsNameAttribute"/> for
    ''' aliases); methods are picked by overload resolution over the actual arguments;
    ''' exceptions leaving a member call become script errors
    ''' (<see cref="ToHostException"/> recovers the original .NET exception).
    ''' </para>
    ''' </summary>
    Public Module ClrInterop

        ''' <summary>
        ''' When True every type is exposed in whitelist mode: only members marked with
        ''' <see cref="JsExposeAttribute"/> stay visible. Default False (all public
        ''' members, minus <see cref="JsIgnoreAttribute"/>).
        ''' </summary>
        Public Property ExplicitOnly As Boolean = False

        ''' <summary>
        ''' How long the bridge waits for a <see cref="System.Threading.Tasks.Task"/> to
        ''' settle before reporting an error (the engine has no event loop).
        ''' </summary>
        Public Property TaskWaitTimeoutMs As Integer = 5000

        Private ReadOnly _hostGlobals As New Dictionary(Of String, Object)
        Private ReadOnly _hostGate As New Object
        Private ReadOnly _typeGate As New Object
        Private ReadOnly _typeProxies As New Dictionary(Of Type, ClrType)
        ' wrapping is identity-stable for reference types: host.child === host.child
        Private ReadOnly _objectProxies As New ConditionalWeakTable(Of Object, ClrObject)

        ' ======================================================== wrapping

        ''' <summary>True for the values this bridge produces (<see cref="ClrObject"/> / <see cref="ClrType"/>).</summary>
        Public Function IsClrProxy(value As Object) As Boolean
            Return TypeOf value Is ClrObject OrElse TypeOf value Is ClrType
        End Function

        ''' <summary>
        ''' Wrap a host value for the script: script values (and proxies) pass through
        ''' unchanged, a <see cref="Type"/> becomes a <see cref="ClrType"/> and any other
        ''' CLR object becomes a <see cref="ClrObject"/> proxy.
        ''' </summary>
        Public Function Wrap(value As Object) As Object
            If value Is Nothing OrElse value Is JsRuntime.Undef Then Return value
            If TypeOf value Is ClrObject OrElse TypeOf value Is ClrType OrElse TypeOf value Is ClrBoundMethod Then
                Return value
            End If
            If TypeOf value Is Type Then Return WrapType(DirectCast(value, Type))
            If IsScriptValue(value) Then Return value

            If value.GetType().IsValueType Then
                ' a struct is boxed once here; writes do not reach the original storage
                Return New ClrObject(value)
            End If
            Return _objectProxies.GetValue(value, AddressOf CreateObjectProxy)
        End Function

        Private Function CreateObjectProxy(target As Object) As ClrObject
            Return New ClrObject(target)
        End Function

        ''' <summary>Expose a <see cref="Type"/> — static members and <c>new</c>.</summary>
        Public Function WrapType(t As Type) As ClrType
            SyncLock _typeGate
                Dim cached As ClrType = Nothing
                If _typeProxies.TryGetValue(t, cached) Then Return cached
                Dim created As New ClrType(t)
                _typeProxies(t) = created
                Return created
            End SyncLock
        End Function

        ''' <summary>Host-side unwrapping: proxies yield their CLR value, anything else passes through.</summary>
        Public Function Unwrap(value As Object) As Object
            Dim o = TryCast(value, ClrObject)
            If o IsNot Nothing Then Return o.Target
            Dim t = TryCast(value, ClrType)
            If t IsNot Nothing Then Return t.UnderlyingType
            Dim m = TryCast(value, ClrBoundMethod)
            If m IsNot Nothing Then Return Unwrap(m.Receiver)
            Return value
        End Function

        Private Function IsScriptValue(value As Object) As Boolean
            If TypeOf value Is Double OrElse TypeOf value Is String OrElse TypeOf value Is Boolean Then Return True
            If JsRuntime.IsJsArray(value) OrElse JsRuntime.IsJsObject(value) OrElse JsRuntime.IsJsFunction(value) Then Return True
            If TypeOf value Is JsError OrElse TypeOf value Is JsDate OrElse TypeOf value Is JsPromise Then Return True
            If TypeOf value Is JsRegExp OrElse TypeOf value Is JsGenerator OrElse TypeOf value Is NativeObject Then Return True
            If TypeOf value Is AccessorProperty OrElse TypeOf value Is SpreadMarker Then Return True
            Return False
        End Function

        ' ======================================================== host registry

        ''' <summary>
        ''' Publish a host object process-wide under a script name; fresh interpreters
        ''' seed their globals from this registry and translated code resolves unknown
        ''' identifiers against it.
        ''' </summary>
        Public Sub Register(name As String, value As Object)
            If String.IsNullOrEmpty(name) Then Throw New ArgumentException("host global name is empty", NameOf(name))
            SyncLock _hostGate
                _hostGlobals(name) = Wrap(value)
            End SyncLock
        End Sub

        Public Function TryGetHostGlobal(name As String, ByRef value As Object) As Boolean
            SyncLock _hostGate
                Return _hostGlobals.TryGetValue(name, value)
            End SyncLock
        End Function

        ''' <summary>Registered host global names.</summary>
        Public Function HostGlobalNames() As List(Of String)
            SyncLock _hostGate
                Return New List(Of String)(_hostGlobals.Keys)
            End SyncLock
        End Function

        Public Sub Unregister(name As String)
            SyncLock _hostGate
                _hostGlobals.Remove(name)
            End SyncLock
        End Sub

        ''' <summary>Forget every registered host global (test teardown / host shutdown).</summary>
        Public Sub Clear()
            SyncLock _hostGate
                _hostGlobals.Clear()
            End SyncLock
        End Sub

        ''' <summary>
        ''' Recover the exception the bridge saw: a script rethrow keeps the original
        ''' .NET exception instance, so host code can react to its type.
        ''' </summary>
        Public Function ToHostException(ex As Exception) As Exception
            Dim jex = TryCast(ex, JsRuntimeException)
            If jex IsNot Nothing Then
                Dim err = TryCast(jex.Payload, JsError)
                If err IsNot Nothing Then
                    If err.Original IsNot Nothing Then Return err.Original
                    Return New Exception(err.ToString())
                End If
            End If
            Return ex
        End Function

        ' ======================================================== member access

        ''' <summary>Read a member (property, field or bound method); undefined when absent.</summary>
        Public Function GetMember(obj As ClrObject, name As String) As Object
            Dim members = ClrMembers.InstanceSet(obj.TargetType)

            Dim p As PropertyInfo = Nothing
            If members.Properties.TryGetValue(name, p) Then
                If Not p.CanRead Then Throw JsRuntimeException.TypeError(ReadOnlyMessage(obj, name))
                Return Reflect(Function() ClrConvert.ToJs(p.GetValue(obj.Target)))
            End If

            Dim f As FieldInfo = Nothing
            If members.Fields.TryGetValue(name, f) Then
                Return Reflect(Function() ClrConvert.ToJs(f.GetValue(obj.Target)))
            End If

            If members.Methods.ContainsKey(name) Then Return obj.BoundMethod(name)

            ' JS-level conveniences, applied only when no CLR member has the name
            If name = "length" AndAlso members.IsEnumerable Then
                Dim n As Double = 0
                If ClrMembers.TryCount(obj.Target, n) Then Return n
            End If
            If name = "toString" Then
                Return CObj(New Func(Of Object(), Object)(Function(args As Object()) CallToString(obj.Target)))
            End If
            Return JsRuntime.Undef
        End Function

        ''' <summary>Write a member (property or field).</summary>
        Public Sub SetMember(obj As ClrObject, name As String, value As Object)
            Dim members = ClrMembers.InstanceSet(obj.TargetType)

            Dim p As PropertyInfo = Nothing
            If members.Properties.TryGetValue(name, p) Then
                If Not p.CanWrite Then Throw JsRuntimeException.TypeError(ReadOnlyMessage(obj, name))
                Reflect(Sub() p.SetValue(obj.Target, ClrConvert.ToClr(value, p.PropertyType)))
                Return
            End If

            Dim f As FieldInfo = Nothing
            If members.Fields.TryGetValue(name, f) Then
                If f.IsInitOnly OrElse f.IsLiteral Then Throw JsRuntimeException.TypeError(ReadOnlyMessage(obj, name))
                Reflect(Sub() f.SetValue(obj.Target, ClrConvert.ToClr(value, f.FieldType)))
                Return
            End If

            If members.Methods.ContainsKey(name) Then
                Throw JsRuntimeException.TypeError("cannot assign to CLR method '" & name & "' of " & TypeLabel(obj))
            End If
            Throw JsRuntimeException.TypeError(
                "cannot set property '" & name & "' — " & TypeLabel(obj) & " has no such CLR member")
        End Sub

        ''' <summary>Does the member exist (the <c>in</c> operator)?</summary>
        Public Function HasMember(obj As ClrObject, name As String) As Boolean
            Dim members = ClrMembers.InstanceSet(obj.TargetType)
            If members.HasMember(name) Then Return True
            If name = "length" AndAlso members.IsEnumerable Then Return True
            If name = "toString" Then Return True
            Return False
        End Function

        ''' <summary>
        ''' Keys for for-in / Object.keys: indices for a collection (like a JS array),
        ''' otherwise the data-member names in declaration order (methods are not own
        ''' properties, matching JS).
        ''' </summary>
        Public Function MemberKeys(obj As ClrObject) As Object()
            Dim members = ClrMembers.InstanceSet(obj.TargetType)
            Dim keys As New List(Of Object)

            If members.IsEnumerable Then
                Dim n As Double = 0
                If ClrMembers.TryCount(obj.Target, n) AndAlso n >= 0 AndAlso n <= Integer.MaxValue Then
                    For i = 0 To CInt(n) - 1
                        keys.Add(i.ToString(CultureInfo.InvariantCulture))
                    Next
                    Return keys.ToArray()
                End If
            End If

            For Each name In members.Names
                keys.Add(name)
            Next
            Return keys.ToArray()
        End Function

        ''' <summary>True when the wrapped value behaves as a JS array (IEnumerable).</summary>
        Public Function IsCollection(obj As ClrObject) As Boolean
            Return ClrMembers.InstanceSet(obj.TargetType).IsEnumerable
        End Function

        ''' <summary>Elements of a collection target, for for-of / spread / arguments.</summary>
        Public Function TryIterate(obj As ClrObject, ByRef values As List(Of Object)) As Boolean
            If Not IsCollection(obj) Then Return False
            values = ClrMembers.Iterate(obj.Target)
            Return True
        End Function

        ''' <summary>Element count of a collection target (the <c>length</c> convenience).</summary>
        Public Function TryGetLength(obj As ClrObject, ByRef length As Double) As Boolean
            If Not IsCollection(obj) Then Return False
            Return ClrMembers.TryCount(obj.Target, length)
        End Function

        ' ======================================================== index access

        ''' <summary>Index read: an array/IList element or a CLR indexer (<c>Item</c>).</summary>
        Public Function GetIndex(obj As ClrObject, index As Object) As Object
            Dim target = obj.Target

            Dim arr = TryCast(target, Array)
            If arr IsNot Nothing Then
                Dim i As Integer
                If Not TryIndex(index, i) OrElse i < 0 OrElse i >= arr.Length Then Return JsRuntime.Undef
                Return Reflect(Function() ClrConvert.ToJs(arr.GetValue(i)))
            End If

            Dim members = ClrMembers.InstanceSet(obj.TargetType)
            For Each p In members.Indexers
                Dim ps = p.GetIndexParameters()
                If ps.Length <> 1 OrElse Not p.CanRead Then Continue For
                If ClrConvert.ConvertScore(index, ps(0).ParameterType) < 0 Then Continue For
                Dim key = ClrConvert.ToClr(index, ps(0).ParameterType)
                Return Reflect(Function() ClrConvert.ToJs(p.GetValue(target, New Object() {key})))
            Next

            ' an IList reached through an interface rather than an indexer property
            Dim list = TryCast(target, System.Collections.IList)
            If list IsNot Nothing Then
                Dim i As Integer
                If Not TryIndex(index, i) OrElse i < 0 OrElse i >= list.Count Then Return JsRuntime.Undef
                Return Reflect(Function() ClrConvert.ToJs(list(i)))
            End If

            Return JsRuntime.Undef
        End Function

        ''' <summary>Index write: an IList element or a writable CLR indexer.</summary>
        Public Sub SetIndex(obj As ClrObject, index As Object, value As Object)
            Dim target = obj.Target

            Dim arr = TryCast(target, Array)
            If arr IsNot Nothing Then
                Dim i As Integer
                If Not TryIndex(index, i) OrElse i < 0 OrElse i >= arr.Length Then
                    Throw JsRuntimeException.TypeError("index " & JsRuntime.JsStr(index) & " is out of range for " & TypeLabel(obj))
                End If
                Reflect(Sub() arr.SetValue(ClrConvert.ToClr(value, arr.GetType().GetElementType()), i))
                Return
            End If

            Dim members = ClrMembers.InstanceSet(obj.TargetType)
            For Each p In members.Indexers
                Dim ps = p.GetIndexParameters()
                If ps.Length <> 1 OrElse Not p.CanWrite Then Continue For
                If ClrConvert.ConvertScore(index, ps(0).ParameterType) < 0 Then Continue For
                Dim key = ClrConvert.ToClr(index, ps(0).ParameterType)
                Dim v = ClrConvert.ToClr(value, p.PropertyType)
                Reflect(Sub() p.SetValue(target, v, New Object() {key}))
                Return
            Next

            Dim list = TryCast(target, System.Collections.IList)
            If list IsNot Nothing Then
                Dim i As Integer
                If Not TryIndex(index, i) OrElse i < 0 OrElse i >= list.Count Then
                    Throw JsRuntimeException.TypeError("index " & JsRuntime.JsStr(index) & " is out of range for " & TypeLabel(obj))
                End If
                Dim v = ClrConvert.ToClr(value, If(list(i) Is Nothing, GetType(Object), list(i).GetType()))
                Reflect(Sub() list(i) = v)
                Return
            End If

            Throw JsRuntimeException.TypeError(TypeLabel(obj) & " does not support indexed assignment")
        End Sub

        Private Function TryIndex(index As Object, ByRef i As Integer) As Boolean
            Dim d = JsRuntime.JsNum(index)
            If Double.IsNaN(d) OrElse Double.IsInfinity(d) Then Return False
            If d < Integer.MinValue OrElse d > Integer.MaxValue Then Return False
            i = CInt(System.Math.Floor(d))
            Return True
        End Function

        ' ======================================================== static members

        ''' <summary>Read a static member of an exposed type; undefined when absent.</summary>
        Public Function GetStaticMember(t As ClrType, name As String) As Object
            Dim members = ClrMembers.StaticSet(t.UnderlyingType)

            Dim p As PropertyInfo = Nothing
            If members.Properties.TryGetValue(name, p) Then
                If Not p.CanRead Then Throw JsRuntimeException.TypeError("static property '" & name & "' of " & t.UnderlyingType.Name & " is write-only")
                Return Reflect(Function() ClrConvert.ToJs(p.GetValue(Nothing)))
            End If

            Dim f As FieldInfo = Nothing
            If members.Fields.TryGetValue(name, f) Then
                Return Reflect(Function() ClrConvert.ToJs(f.GetValue(Nothing)))
            End If

            If members.Methods.ContainsKey(name) Then Return New ClrBoundMethod(t, name)

            If name = "name" Then Return t.UnderlyingType.Name
            If name = "toString" Then
                Return CObj(New Func(Of Object(), Object)(Function(args As Object()) t.UnderlyingType.ToString()))
            End If
            Return JsRuntime.Undef
        End Function

        ''' <summary>Write a static member of an exposed type.</summary>
        Public Sub SetStaticMember(t As ClrType, name As String, value As Object)
            Dim members = ClrMembers.StaticSet(t.UnderlyingType)

            Dim p As PropertyInfo = Nothing
            If members.Properties.TryGetValue(name, p) Then
                If Not p.CanWrite Then
                    Throw JsRuntimeException.TypeError("static property '" & name & "' of " & t.UnderlyingType.Name & " is read-only")
                End If
                Reflect(Sub() p.SetValue(Nothing, ClrConvert.ToClr(value, p.PropertyType)))
                Return
            End If

            Dim f As FieldInfo = Nothing
            If members.Fields.TryGetValue(name, f) Then
                If f.IsInitOnly OrElse f.IsLiteral Then
                    Throw JsRuntimeException.TypeError("static field '" & name & "' of " & t.UnderlyingType.Name & " is read-only")
                End If
                Reflect(Sub() f.SetValue(Nothing, ClrConvert.ToClr(value, f.FieldType)))
                Return
            End If

            Throw JsRuntimeException.TypeError(
                "cannot set static property '" & name & "' — " & t.UnderlyingType.Name & " has no such CLR member")
        End Sub

        ''' <summary>Does the type expose this static member (<c>'x' in T</c>)?</summary>
        Public Function HasStaticMember(t As ClrType, name As String) As Boolean
            Return ClrMembers.StaticSet(t.UnderlyingType).HasMember(name)
        End Function

        ' ======================================================== invocation / construction

        ''' <summary>Invoke a bound CLR method with overload resolution over the arguments.</summary>
        Public Function InvokeBound(method As ClrBoundMethod, args As Object()) As Object
            Dim name = method.Name

            Dim proxy = TryCast(method.Receiver, ClrObject)
            If proxy IsNot Nothing Then
                Dim candidates As List(Of MethodInfo) = Nothing
                If Not ClrMembers.InstanceSet(proxy.TargetType).Methods.TryGetValue(name, candidates) Then
                    Throw JsRuntimeException.TypeError(name & " is not a function")
                End If
                Return InvokeMember(proxy.Target, ClrMembers.ResolveOverload(candidates, args, name))
            End If

            Dim typeProxy = TryCast(method.Receiver, ClrType)
            If typeProxy IsNot Nothing Then
                Dim candidates As List(Of MethodInfo) = Nothing
                If Not ClrMembers.StaticSet(typeProxy.UnderlyingType).Methods.TryGetValue(name, candidates) Then
                    Throw JsRuntimeException.TypeError(name & " is not a function")
                End If
                Return InvokeMember(Nothing, ClrMembers.ResolveOverload(candidates, args, name))
            End If

            Throw JsRuntimeException.TypeError(name & " is not a function")
        End Function

        Private Function InvokeMember(instance As Object, resolved As ClrCall) As Object
            Try
                Return ClrConvert.ToJs(DirectCast(resolved.Info, MethodInfo).Invoke(instance, resolved.Args))
            Catch ex As Exception
                Throw ClrConvert.NormalizeFailure(ex)
            End Try
        End Function

        ''' <summary><c>new T(...)</c> — run a public constructor and wrap the instance.</summary>
        Public Function Construct(t As ClrType, args As Object()) As Object
            Dim ctors = ClrMembers.Constructors(t.UnderlyingType)
            If ctors.Count = 0 Then
                If t.UnderlyingType.IsValueType Then Return Wrap(Activator.CreateInstance(t.UnderlyingType))
                Throw JsRuntimeException.TypeError(
                    "Class constructor " & t.UnderlyingType.Name & " cannot be instantiated — no public constructor")
            End If

            Dim resolved = ClrMembers.ResolveConstructor(ctors, args, t.UnderlyingType.Name)
            Try
                Return Wrap(DirectCast(resolved.Info, ConstructorInfo).Invoke(resolved.Args))
            Catch ex As Exception
                Throw ClrConvert.NormalizeFailure(ex)
            End Try
        End Function

        ''' <summary><c>x instanceof T</c> — CLR type relation of a bridged value.</summary>
        Public Function IsInstanceOf(value As Object, t As ClrType) As Boolean
            Dim proxy = TryCast(value, ClrObject)
            If proxy IsNot Nothing Then Return t.UnderlyingType.IsInstanceOfType(proxy.Target)
            Dim typeProxy = TryCast(value, ClrType)
            If typeProxy IsNot Nothing Then Return t.UnderlyingType.IsAssignableFrom(typeProxy.UnderlyingType)
            Return False
        End Function

        ' ======================================================== JSON / spread views

        ''' <summary>
        ''' Data members of a proxy for JSON/spread views: readable properties and
        ''' fields with converted values. A failing getter is skipped (one bad member
        ''' must not break the whole enumeration).
        ''' </summary>
        Public Function JsonMembers(obj As ClrObject) As List(Of KeyValuePair(Of String, Object))
            Dim result As New List(Of KeyValuePair(Of String, Object))
            Dim members = ClrMembers.InstanceSet(obj.TargetType)
            If members.IsEnumerable Then Return result          ' collections serialise as arrays

            For Each name In members.Names
                Dim p As PropertyInfo = Nothing
                If members.Properties.TryGetValue(name, p) Then
                    If p.CanRead Then
                        Try
                            result.Add(New KeyValuePair(Of String, Object)(name, ClrConvert.ToJs(p.GetValue(obj.Target))))
                        Catch ex As Exception
                        End Try
                    End If
                    Continue For
                End If

                Dim f As FieldInfo = Nothing
                If members.Fields.TryGetValue(name, f) Then
                    Try
                        result.Add(New KeyValuePair(Of String, Object)(name, ClrConvert.ToJs(f.GetValue(obj.Target))))
                    Catch ex As Exception
                    End Try
                End If
            Next
            Return result
        End Function

        ''' <summary>Element view of a collection proxy for JSON ([] when not a collection).</summary>
        Public Function JsonItems(obj As ClrObject) As List(Of Object)
            If Not IsCollection(obj) Then Return New List(Of Object)
            Return ClrMembers.Iterate(obj.Target)
        End Function

        ''' <summary>Object spread source (<c>{...host}</c>).</summary>
        Public Sub SpreadInto(target As Dictionary(Of String, Object), obj As ClrObject)
            If IsCollection(obj) Then
                Dim values = ClrMembers.Iterate(obj.Target)
                For i = 0 To values.Count - 1
                    target(i.ToString(CultureInfo.InvariantCulture)) = values(i)
                Next
                Return
            End If
            For Each kv In JsonMembers(obj)
                target(kv.Key) = kv.Value
            Next
        End Sub

        ' ======================================================== helpers

        Private Function TypeLabel(obj As ClrObject) As String
            Return obj.TargetType.Name
        End Function

        Private Function ReadOnlyMessage(obj As ClrObject, name As String) As String
            Return "'" & name & "' of " & TypeLabel(obj) & " is read-only or write-only"
        End Function

        Private Function CallToString(target As Object) As Object
            Try
                Dim s = target.ToString()
                If s Is Nothing Then Return JsRuntime.Undef
                Return s
            Catch ex As Exception
                Throw ClrConvert.NormalizeFailure(ex)
            End Try
        End Function

        ''' <summary>Run a reflection operation, turning CLR failures into script errors.</summary>
        Private Function Reflect(Of T)(f As Func(Of T)) As T
            Try
                Return f()
            Catch ex As Exception
                Throw ClrConvert.NormalizeFailure(ex)
            End Try
        End Function

        Private Sub Reflect(action As Action)
            Try
                action()
            Catch ex As Exception
                Throw ClrConvert.NormalizeFailure(ex)
            End Try
        End Sub

    End Module

    ''' <summary>
    ''' A batch of host objects published under script names. The generated entry point
    ''' of a translated script takes one as its optional second argument and installs it
    ''' before running, so <c>/translate</c> output can use injected objects too.
    ''' </summary>
    Public NotInheritable Class ClrHost

        Private ReadOnly _items As New List(Of KeyValuePair(Of String, Object))

        ''' <summary>Added (name, value) pairs, in declaration order.</summary>
        Public ReadOnly Property Items As List(Of KeyValuePair(Of String, Object))
            Get
                Return _items
            End Get
        End Property

        ''' <summary>Add a host object (fluent).</summary>
        Public Function Add(name As String, value As Object) As ClrHost
            _items.Add(New KeyValuePair(Of String, Object)(name, value))
            Return Me
        End Function

        ''' <summary>Publish every added object to the process-wide host registry.</summary>
        Public Sub Install()
            For Each kv In _items
                ClrInterop.Register(kv.Key, kv.Value)
            Next
        End Sub

    End Class

End Namespace
