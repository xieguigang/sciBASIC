Imports System.Globalization
Imports System.Text
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Scripting.Runtime
' NOTE: Microsoft.VisualBasic.MIME.application.json(+"".Javascript) 命名空间
' 已由 LiteJs.vbproj 的项目级 <Import> 提供，此处不再重复导入
Imports Microsoft.VisualBasic.MIME.application.json
Imports Microsoft.VisualBasic.MIME.application.json.Javascript

Namespace Runtime

    Public Module JsRuntime

        ''' <summary>JS function value in both worlds (interpreter closures and generated lambdas).</summary>
        Public ReadOnly Undef As Object = New UndefinedMarker()

        ' per-thread sink: two script engines on different threads must not
        ' interleave their console output (ThreadStatic fields cannot have an
        ' initialiser, hence the lazy default in the getter)
        <ThreadStatic>
        Private _io As ScriptIO

        ''' <summary>console.log sink used by builtin dispatch and generated code.</summary>
        Public Property Io As ScriptIO
            Get
                If _io Is Nothing Then _io = New LogTextIO()
                Return _io
            End Get
            Set(value As ScriptIO)
                _io = value
            End Set
        End Property

        Public ReadOnly ConsoleBuiltin As New NativeObject("console")
        Public ReadOnly MathBuiltin As New NativeObject("Math")
        Public ReadOnly Json As New NativeObject("JSON")
        Public ReadOnly ObjectBuiltin As New NativeObject("Object")
        Public ReadOnly ArrayBuiltin As New NativeObject("Array")
        Public ReadOnly FunctionBuiltin As New NativeObject("Function")
        Public ReadOnly DateBuiltin As New NativeObject("Date")
        Public ReadOnly RegExpBuiltin As New NativeObject("RegExp")
        Public ReadOnly ErrorBuiltin As New NativeObject("Error")
        Public ReadOnly PromiseBuiltin As New NativeObject("Promise")
        Public ReadOnly GlobalObj As New NativeObject("global")

        ' math constants exposed as properties (System.Math qualified — the
        ' field above shadows the type name inside this module)
        Private ReadOnly _mathProps As New Dictionary(Of String, Object) From {
            {"PI", System.Math.PI}, {"E", System.Math.E},
            {"LN2", System.Math.Log(2.0)}, {"LN10", System.Math.Log(10.0)}
        }

        ''' <summary>Global identifiers installed into every fresh interpreter environment.</summary>
        Public Function GlobalIdentifiers() As IDictionary(Of String, Object)
            Dim g As New Dictionary(Of String, Object) From {
                {"console", ConsoleBuiltin}, {"Math", MathBuiltin}, {"JSON", Json},
                {"Object", ObjectBuiltin}, {"Array", ArrayBuiltin}, {"Function", FunctionBuiltin},
                {"Date", DateBuiltin}, {"RegExp", RegExpBuiltin}, {"Error", ErrorBuiltin},
                {"Promise", PromiseBuiltin},
                {"undefined", Undef}, {"NaN", Double.NaN}, {"Infinity", Double.PositiveInfinity}
            }
            For Each name In {"parseInt", "parseFloat", "isNaN", "isFinite",
                              "String", "Number", "Boolean"}
                g(name) = New BuiltinMethod(GlobalObj, name)
            Next
            ' the Error hierarchy: each name is its own constructor
            For Each name In {"TypeError", "RangeError", "SyntaxError", "ReferenceError"}
                g(name) = New BuiltinMethod(GlobalObj, name)
            Next
            Return g
        End Function

        ''' <summary>
        ''' Resolve an identifier that is not one of the builtins: a CLR object the host
        ''' published through <see cref="ClrInterop.Register"/> (or a <see cref="ClrHost"/>).
        ''' <para>
        ''' Generated code calls this for unknown globals, so translated scripts can use
        ''' injected host objects exactly like interpreted ones; an unregistered name
        ''' raises a normal JS ReferenceError at run time.
        ''' </para>
        ''' </summary>
        Public Function ResolveHostGlobal(name As String) As Object
            Dim value As Object = Nothing
            If ClrInterop.TryGetHostGlobal(name, value) Then Return value
            Throw New JsRuntimeException(New JsError("ReferenceError", name & " is not defined"))
        End Function

        ''' <summary>Names of the host globals currently registered (see <see cref="ClrInterop"/>).</summary>
        Public Function HostGlobalNames() As List(Of String)
            Return ClrInterop.HostGlobalNames()
        End Function

        ' ================= type predicates =================

        Public Function IsNumber(x As Object) As Boolean
            Return TypeOf x Is Double
        End Function

        Public Function IsJsArray(x As Object) As Boolean
            Return TypeOf x Is List(Of Object)
        End Function

        Public Function IsJsObject(x As Object) As Boolean
            Return TypeOf x Is Dictionary(Of String, Object)
        End Function

        ' ================= conversions =================

        Public Function JsTruthy(x As Object) As Boolean
            If x Is Nothing OrElse x Is Undef Then Return False
            If TypeOf x Is Boolean Then Return CBool(x)
            If TypeOf x Is Double Then
                Dim d = CDbl(x)
                Return Not (d = 0.0 OrElse Double.IsNaN(d))
            End If
            If TypeOf x Is String Then Return CStr(x).Length > 0
            Return True
        End Function

        Public Function JsBool(x As Object) As Boolean
            Return JsTruthy(x)
        End Function

        Public Function JsNum(x As Object) As Double
            If TypeOf x Is Double Then Return CDbl(x)
            If TypeOf x Is Boolean Then Return If(CBool(x), 1.0, 0.0)
            If x Is Nothing Then Return 0.0
            If x Is Undef Then Return Double.NaN
            If TypeOf x Is String Then Return ParseNumberString(CStr(x))
            If TypeOf x Is List(Of Object) Then
                Dim a = DirectCast(x, List(Of Object))
                If a.Count = 0 Then Return 0.0
                If a.Count = 1 Then Return JsNum(a(0))
                Return Double.NaN
            End If
            Return Double.NaN
        End Function

        Private Function ParseNumberString(s As String) As Double
            Dim t = s.Trim()
            If t = "" Then Return 0.0
            Dim v As Double
            If Double.TryParse(t, NumberStyles.Float, CultureInfo.InvariantCulture, v) Then Return v
            Return Double.NaN
        End Function

        Public Function JsStr(x As Object) As String
            If x Is Nothing Then Return "null"
            If x Is Undef Then Return "undefined"
            If TypeOf x Is Double Then Return NumToString(CDbl(x))
            If TypeOf x Is Boolean Then Return If(CBool(x), "true", "false")
            If TypeOf x Is String Then Return CStr(x)
            If TypeOf x Is List(Of Object) Then Return JoinArray(DirectCast(x, List(Of Object)), ",")
            If TypeOf x Is Dictionary(Of String, Object) Then Return "[object Object]"
            If TypeOf x Is BuiltinMethod Then Return "[function: " & DirectCast(x, BuiltinMethod).Name & "]"
            If TypeOf x Is NativeObject Then Return "[object " & DirectCast(x, NativeObject).Name & "]"
            If TypeOf x Is JsClass Then Return "class " & DirectCast(x, JsClass).Name & " { ... }"
            If TypeOf x Is JsError Then Return DirectCast(x, JsError).ToString()
            If TypeOf x Is JsGenerator Then Return "[object Generator]"
            If TypeOf x Is JsDate Then Return DirectCast(x, JsDate).ToString()
            If TypeOf x Is JsRegExp Then Return DirectCast(x, JsRegExp).ToString()
            ' ---- CLR reflection bridge ----
            If TypeOf x Is ClrObject Then Return "[object Object]"
            If TypeOf x Is ClrType Then Return "class " & DirectCast(x, ClrType).UnderlyingType.Name
            If TypeOf x Is ClrBoundMethod Then Return "[function: " & DirectCast(x, ClrBoundMethod).Name & "]"
            Return CStr(x)
        End Function

        Private Function NumToString(d As Double) As String
            If Double.IsNaN(d) Then Return "NaN"
            If Double.IsPositiveInfinity(d) Then Return "Infinity"
            If Double.IsNegativeInfinity(d) Then Return "-Infinity"
            If d = System.Math.Truncate(d) AndAlso System.Math.Abs(d) < 1.0E+15 Then
                Return CLng(d).ToString(CultureInfo.InvariantCulture)
            End If
            Return d.ToString("R", CultureInfo.InvariantCulture)
        End Function

        Private Function JoinArray(a As List(Of Object), sep As String) As String
            Dim parts(a.Count - 1) As String
            For i = 0 To a.Count - 1
                parts(i) = If(a(i) Is Nothing OrElse a(i) Is Undef, "", JsStr(a(i)))
            Next
            Return String.Join(sep, parts)
        End Function

        ''' <summary>console.log formatting: arrays/objects shown as JSON text.</summary>
        Public Function JsDisplay(x As Object) As String
            If TypeOf x Is String Then Return CStr(x)
            If TypeOf x Is List(Of Object) OrElse TypeOf x Is Dictionary(Of String, Object) Then
                ' JSON style display: {"a": 1, "b": [1, 2]}
                Try
                    Return DisplayJson(x)
                Catch ex As JsRuntimeException
                    ' circular reference in displayed graph
                    Return "[Circular]"
                End Try
            End If
            ' ---- CLR reflection bridge: show the wrapped CLR type ----
            If TypeOf x Is ClrObject Then
                Return "[object " & DirectCast(x, ClrObject).TargetType.ToString() & "]"
            End If
            Return JsStr(x)
        End Function

        Public Function JsTypeOf(x As Object) As String
            If TypeOf x Is Double Then Return "number"
            If TypeOf x Is String Then Return "string"
            If TypeOf x Is Boolean Then Return "boolean"
            If IsJsFunction(x) Then Return "function"       ' classes are functions too
            ' the builtin constructors are functions; Math / JSON / console are objects
            If TypeOf x Is NativeObject Then
                Select Case DirectCast(x, NativeObject).Name
                    Case "Array", "Object", "Function", "Date", "RegExp", "Error", "Promise"
                        Return "function"
                End Select
                Return "object"
            End If
            If x Is Undef Then Return "undefined"
            If x Is Nothing Then Return "object"     ' JS: typeof null === "object"
            Return "object"
        End Function

        ' ================= arithmetic =================

        Public Function JsAdd(a As Object, b As Object) As Object
            If TypeOf a Is String OrElse TypeOf b Is String Then
                Return JsStr(a) & JsStr(b)
            End If
            If TypeOf a Is Double AndAlso TypeOf b Is Double Then
                Return CDbl(a) + CDbl(b)
            End If
            If a Is Nothing AndAlso b Is Nothing Then Return 0.0
            If a Is Undef OrElse b Is Undef Then Return Double.NaN
            Return JsNum(a) + JsNum(b)
        End Function

        Public Function JsSub(a As Object, b As Object) As Object
            Return JsNum(a) - JsNum(b)
        End Function

        Public Function JsMul(a As Object, b As Object) As Object
            Return JsNum(a) * JsNum(b)
        End Function

        Public Function JsDiv(a As Object, b As Object) As Object
            Return JsNum(a) / JsNum(b)
        End Function

        Public Function JsMod(a As Object, b As Object) As Object
            Dim x = JsNum(a), y = JsNum(b)
            If Double.IsNaN(x) OrElse Double.IsNaN(y) OrElse y = 0.0 Then Return Double.NaN
            Return x - System.Math.Floor(x / y) * y      ' JS-style floored modulo
        End Function

        ''' <summary>Exponentiation (**). NaN base with fractional exponent → NaN, like JS.</summary>
        Public Function JsPow(a As Object, b As Object) As Object
            Dim x = JsNum(a), y = JsNum(b)
            If Double.IsNaN(x) Then Return Double.NaN
            Return System.Math.Pow(x, y)
        End Function

        Public Function JsNeg(a As Object) As Object
            Return -JsNum(a)
        End Function

        Public Function JsNot(a As Object) As Object
            Return Not JsTruthy(a)
        End Function

        ''' <summary>Short-circuit &amp;&amp; — evaluates b lazily only when a is truthy.</summary>
        Public Function JsAnd(a As Object, b As Func(Of Object)) As Object
            If JsTruthy(a) Then Return b()
            Return a
        End Function

        ''' <summary>Short-circuit || — evaluates b lazily only when a is falsy.</summary>
        Public Function JsOr(a As Object, b As Func(Of Object)) As Object
            If JsTruthy(a) Then Return a
            Return b()
        End Function

        ' ================= comparison =================

        Public Function JsStrictEq(a As Object, b As Object) As Boolean
            If IsNumber(a) AndAlso IsNumber(b) Then
                Dim x = CDbl(a), y = CDbl(b)
                If Double.IsNaN(x) OrElse Double.IsNaN(y) Then Return False
                Return x = y
            End If
            If TypeOf a Is String AndAlso TypeOf b Is String Then Return CStr(a) = CStr(b)
            If TypeOf a Is Boolean AndAlso TypeOf b Is Boolean Then Return CBool(a) = CBool(b)
            If a Is Nothing AndAlso b Is Nothing Then Return True
            If a Is Undef AndAlso b Is Undef Then Return True
            If IsJsArray(a) OrElse IsJsObject(a) OrElse IsJsArray(b) OrElse IsJsObject(b) OrElse
               TypeOf a Is Func(Of Object(), Object) OrElse TypeOf b Is Func(Of Object(), Object) OrElse
               TypeOf a Is BuiltinMethod OrElse TypeOf b Is BuiltinMethod OrElse
               ClrInterop.IsClrProxy(a) OrElse ClrInterop.IsClrProxy(b) OrElse
               TypeOf a Is ClrBoundMethod OrElse TypeOf b Is ClrBoundMethod Then
                ' bridged values are objects/functions: identity comparison, like JS
                Return ReferenceEquals(a, b)
            End If
            Return False
        End Function

        Public Function JsNe(a As Object, b As Object) As Boolean
            Return Not JsEq(a, b)
        End Function

        Public Function JsStrictNe(a As Object, b As Object) As Boolean
            Return Not JsStrictEq(a, b)
        End Function

        Public Function JsEq(a As Object, b As Object) As Boolean
            ' same types → strict
            If (IsNumber(a) AndAlso IsNumber(b)) OrElse
               (TypeOf a Is String AndAlso TypeOf b Is String) OrElse
               (TypeOf a Is Boolean AndAlso TypeOf b Is Boolean) OrElse
               (IsJsArray(a) OrElse IsJsObject(a) OrElse TypeOf a Is Func(Of Object(), Object) OrElse TypeOf a Is BuiltinMethod OrElse ClrInterop.IsClrProxy(a)) AndAlso
               (IsJsArray(b) OrElse IsJsObject(b) OrElse TypeOf b Is Func(Of Object(), Object) OrElse TypeOf b Is BuiltinMethod OrElse ClrInterop.IsClrProxy(b)) Then
                Return JsStrictEq(a, b)
            End If
            ' null / undefined family
            If (a Is Nothing OrElse a Is Undef) AndAlso (b Is Nothing OrElse b Is Undef) Then Return True
            If (a Is Nothing OrElse a Is Undef) OrElse (b Is Nothing OrElse b Is Undef) Then Return False
            ' number vs string → numeric
            If IsNumber(a) AndAlso TypeOf b Is String Then Return JsEq(a, JsNum(b))
            If TypeOf a Is String AndAlso IsNumber(b) Then Return JsEq(JsNum(a), b)
            ' boolean → number
            If TypeOf a Is Boolean Then Return JsEq(JsNum(a), b)
            If TypeOf b Is Boolean Then Return JsEq(a, JsNum(b))
            ' object vs primitive → stringify then compare as strings when primitive is a string
            If TypeOf b Is String Then Return CStr(JsStr(a)) = CStr(b)
            If TypeOf a Is String Then Return CStr(a) = CStr(JsStr(b))
            Return JsNum(a) = JsNum(b)
        End Function

        Public Function JsLt(a As Object, b As Object) As Boolean
            If TypeOf a Is String AndAlso TypeOf b Is String Then Return String.Compare(CStr(a), CStr(b), StringComparison.Ordinal) < 0
            Dim x = JsNum(a), y = JsNum(b)
            If Double.IsNaN(x) OrElse Double.IsNaN(y) Then Return False
            Return x < y
        End Function

        Public Function JsLe(a As Object, b As Object) As Boolean
            If TypeOf a Is String AndAlso TypeOf b Is String Then Return String.Compare(CStr(a), CStr(b), StringComparison.Ordinal) <= 0
            Dim x = JsNum(a), y = JsNum(b)
            If Double.IsNaN(x) OrElse Double.IsNaN(y) Then Return False
            Return x <= y
        End Function

        Public Function JsGt(a As Object, b As Object) As Boolean
            If TypeOf a Is String AndAlso TypeOf b Is String Then Return String.Compare(CStr(a), CStr(b), StringComparison.Ordinal) > 0
            Dim x = JsNum(a), y = JsNum(b)
            If Double.IsNaN(x) OrElse Double.IsNaN(y) Then Return False
            Return x > y
        End Function

        Public Function JsGe(a As Object, b As Object) As Boolean
            If TypeOf a Is String AndAlso TypeOf b Is String Then Return String.Compare(CStr(a), CStr(b), StringComparison.Ordinal) >= 0
            Dim x = JsNum(a), y = JsNum(b)
            If Double.IsNaN(x) OrElse Double.IsNaN(y) Then Return False
            Return x >= y
        End Function

        ' ================= member / index access =================

        Public Function JsGet(obj As Object, name As String) As Object
            If obj Is Nothing OrElse obj Is Undef Then
                Throw JsRuntimeException.TypeError("cannot read property '" & name & "' of " & JsStr(obj))
            End If
            ' ---- CLR reflection bridge: host-injected .NET objects (see ClrInterop) ----
            Dim clrObj = TryCast(obj, ClrObject)
            If clrObj IsNot Nothing Then Return ClrInterop.GetMember(clrObj, name)
            Dim clrType = TryCast(obj, ClrType)
            If clrType IsNot Nothing Then Return ClrInterop.GetStaticMember(clrType, name)

            If IsJsArray(obj) Then
                Dim a = DirectCast(obj, List(Of Object))
                If name = "length" Then Return CDbl(a.Count)
                If ArrayMethodNames.Contains(name) Then Return New BuiltinMethod(obj, name)
                Return Undef
            End If
            If TypeOf obj Is String Then
                If name = "length" Then Return CDbl(CStr(obj).Length)
                If StringMethodNames.Contains(name) Then Return New BuiltinMethod(obj, name)
                Return Undef
            End If
            If IsJsObject(obj) Then
                Dim d = DirectCast(obj, Dictionary(Of String, Object))
                Dim v As Object = Nothing
                If d.TryGetValue(name, v) Then
                    ' accessor property: read through its getter (receiver = the object)
                    Return ReadMember(v, obj, InstanceClass(obj))
                End If
                ' class instances resolve inherited members through the __proto__ chain
                Dim owner As JsClass = Nothing
                Dim found As Boolean = False
                Dim inherited = LookupInstanceMember(d, name, owner, found)
                If found Then Return ReadMember(inherited, obj, owner)
                Return Undef
            End If
            If TypeOf obj Is JsClass Then
                Dim c = DirectCast(obj, JsClass)
                If name = "name" Then Return c.Name
                If name = "prototype" Then Return c.Prototype
                Dim v As Object = Nothing
                Dim cur = c
                While cur IsNot Nothing
                    If cur.Statics.TryGetValue(name, v) Then Return ReadMember(v, obj, cur)
                    cur = cur.SuperClass
                End While
                Return Undef
            End If
            If TypeOf obj Is NativeObject Then
                Dim n = DirectCast(obj, NativeObject)
                If n.Name = "Math" AndAlso _mathProps.TryGetValue(name, Nothing) Then Return _mathProps(name)
                ' the global object has no builtin methods beyond the registered globals
                If n.Name = "global" Then Return Undef
                Return New BuiltinMethod(obj, name)
            End If
            If TypeOf obj Is JsRegExp Then
                Dim re = DirectCast(obj, JsRegExp)
                Select Case name
                    Case "source" : Return re.Source
                    Case "flags" : Return re.Flags
                    Case "global" : Return re.IsGlobal
                    Case "ignoreCase" : Return re.IgnoreCase
                    Case "sticky" : Return re.Sticky
                    Case "lastIndex" : Return CDbl(re.LastIndex)
                    Case "test", "exec", "toString", "compile"
                        Return New BuiltinMethod(obj, name)
                End Select
                Return Undef
            End If
            If TypeOf obj Is JsDate Then
                Select Case name
                    Case "getTime", "getFullYear", "getMonth", "getDate", "getDay", "getHours",
                         "getMinutes", "getSeconds", "getMilliseconds", "getTimezoneOffset",
                         "toISOString", "toString", "toDateString", "toTimeString",
                         "toJSON", "toLocaleDateString", "toLocaleTimeString", "valueOf"
                        Return New BuiltinMethod(obj, name)
                End Select
                Return Undef
            End If
            If TypeOf obj Is JsPromise Then
                Select Case name
                    Case "then", "catch", "finally"
                        Return New BuiltinMethod(obj, name)
                End Select
                Return Undef
            End If
            If TypeOf obj Is JsGenerator Then
                Select Case name
                    Case "next", "toString", "return"
                        Return New BuiltinMethod(obj, name)
                End Select
                Return Undef
            End If
            If TypeOf obj Is JsError Then
                Dim err = DirectCast(obj, JsError)
                Select Case name
                    Case "name" : Return err.Name
                    Case "message" : Return err.Message
                    Case "stack" : Return err.ToString()
                    ' the .NET exception a CLR bridge call raised (Nothing for engine errors)
                    Case "clrException" : Return err.Original
                    Case "toString" : Return New BuiltinMethod(obj, name)
                End Select
                Return Undef
            End If
            ' static helpers live on the builtin function objects (Number.isInteger, ...)
            If TypeOf obj Is BuiltinMethod Then
                Dim bm = DirectCast(obj, BuiltinMethod)
                If bm.Name = "Number" Then
                    ' numeric constants are values, not methods
                    Select Case name
                        Case "MAX_SAFE_INTEGER" : Return 9007199254740991.0
                        Case "MIN_SAFE_INTEGER" : Return -9007199254740991.0
                        Case "MAX_VALUE" : Return Double.MaxValue
                        Case "MIN_VALUE" : Return Double.Epsilon
                        Case "EPSILON" : Return 2.2204460492503131E-16
                        Case "POSITIVE_INFINITY" : Return Double.PositiveInfinity
                        Case "NEGATIVE_INFINITY" : Return Double.NegativeInfinity
                        Case "NaN" : Return Double.NaN
                    End Select
                End If
                Select Case bm.Name
                    Case "Number", "String", "Boolean"
                        Return New BuiltinMethod(obj, name)
                End Select
                Return Undef
            End If
            If IsNumber(obj) AndAlso (name = "toFixed" OrElse name = "toString" OrElse name = "toPrecision") Then
                Return New BuiltinMethod(obj, name)
            End If
            Return Undef
        End Function

        Public Function JsIndex(obj As Object, index As Object) As Object
            If obj Is Nothing OrElse obj Is Undef Then
                Throw JsRuntimeException.TypeError("cannot index " & JsStr(obj))
            End If
            ' ---- CLR reflection bridge: array elements and indexers ----
            Dim clrObj = TryCast(obj, ClrObject)
            If clrObj IsNot Nothing Then Return ClrInterop.GetIndex(clrObj, index)

            If IsJsArray(obj) Then
                Dim a = DirectCast(obj, List(Of Object))
                Dim i = CInt(System.Math.Floor(JsNum(index)))
                If Double.IsNaN(JsNum(index)) Then Return Undef
                If i < 0 OrElse i >= a.Count Then Return Undef
                Return a(i)
            End If
            If TypeOf obj Is String Then
                Dim s = CStr(obj)
                Dim i = CInt(System.Math.Floor(JsNum(index)))
                If i < 0 OrElse i >= s.Length Then Return Undef
                Return s.Substring(i, 1)
            End If
            If IsJsObject(obj) Then
                Dim d = DirectCast(obj, Dictionary(Of String, Object))
                Dim v As Object = Nothing
                If d.TryGetValue(JsStr(index), v) Then Return v
                Return Undef
            End If
            Return Undef
        End Function

        Public Sub JsSet(obj As Object, name As String, value As Object)
            If obj Is Nothing OrElse obj Is Undef Then
                Throw JsRuntimeException.TypeError("cannot set property '" & name & "' of " & JsStr(obj))
            End If
            ' ---- CLR reflection bridge: property / field writes ----
            Dim clrObj = TryCast(obj, ClrObject)
            If clrObj IsNot Nothing Then
                ClrInterop.SetMember(clrObj, name, value)
                Return
            End If
            Dim clrType = TryCast(obj, ClrType)
            If clrType IsNot Nothing Then
                ClrInterop.SetStaticMember(clrType, name, value)
                Return
            End If

            If IsJsObject(obj) Then
                Dim d = DirectCast(obj, Dictionary(Of String, Object))
                Dim existing As Object = Nothing
                If d.TryGetValue(name, existing) Then
                    If TryWriteAccessor(existing, value, obj, InstanceClass(obj)) Then Return
                End If
                ' a setter may also live on the prototype chain
                Dim owner As JsClass = Nothing
                Dim found As Boolean = False
                Dim inherited = LookupInstanceMember(d, name, owner, found)
                If found AndAlso TryWriteAccessor(inherited, value, obj, owner) Then Return
                d(name) = value
            ElseIf TypeOf obj Is JsClass Then
                Dim c = DirectCast(obj, JsClass)
                Dim existingStatic As Object = Nothing
                If c.Statics.TryGetValue(name, existingStatic) Then
                    If TryWriteAccessor(existingStatic, value, obj, c) Then Return
                End If
                c.Statics(name) = value
            End If
            ' other targets: silently ignored (non-strict JS)
        End Sub

        ''' <summary>Write through an accessor setter when the value is one; False otherwise.</summary>
        Private Function TryWriteAccessor(existing As Object, value As Object, receiver As Object, home As JsClass) As Boolean
            Dim acc = TryCast(existing, AccessorProperty)
            If acc Is Nothing Then Return False
            If acc.Setter IsNot Nothing Then JsInvokeCore(acc.Setter, New Object() {value}, receiver, home)
            Return True
        End Function

        Public Sub JsSetIndex(obj As Object, index As Object, value As Object)
            If obj Is Nothing OrElse obj Is Undef Then
                Throw JsRuntimeException.TypeError("cannot index " & JsStr(obj))
            End If
            ' ---- CLR reflection bridge: element / indexer writes ----
            Dim clrObj = TryCast(obj, ClrObject)
            If clrObj IsNot Nothing Then
                ClrInterop.SetIndex(clrObj, index, value)
                Return
            End If

            If IsJsArray(obj) Then
                Dim a = DirectCast(obj, List(Of Object))
                Dim i = CInt(System.Math.Floor(JsNum(index)))
                While a.Count <= i
                    a.Add(Undef)
                End While
                a(i) = value
                Return
            End If
            If IsJsObject(obj) Then
                DirectCast(obj, Dictionary(Of String, Object))(JsStr(index)) = value
            End If
        End Sub

        ' ================= invocation =================

        ' receiver / home-object context of the innermost in-flight call. A
        ' stack discipline (save/restore around every call) keeps nested calls
        ' correct; per-thread so independent engines never collide.
        <ThreadStatic>
        Private _thisValue As Object

        <ThreadStatic>
        Private _homeClass As JsClass

        ''' <summary>
        ''' Receiver of the innermost in-flight call — the value of <c>this</c>.
        ''' A plain call (non-strict JS) sees the global object.
        ''' </summary>
        Public ReadOnly Property CurrentThis As Object
            Get
                If _thisValue Is Nothing Then Return GlobalObj
                Return _thisValue
            End Get
        End Property

        ''' <summary>
        ''' The class that provided the innermost in-flight method — the anchor of
        ''' <c>super</c> lookups.
        ''' </summary>
        Public ReadOnly Property CurrentHomeClass As JsClass
            Get
                Return _homeClass
            End Get
        End Property

        Public Function JsInvoke(f As Object, args As Object()) As Object
            ' plain call: this = global object (non-strict JS)
            Return JsInvokeCore(f, args, GlobalObj, Nothing)
        End Function

        ''' <summary>Invoke with an explicit receiver (method call).</summary>
        Public Function JsInvoke(f As Object, args As Object(), receiver As Object) As Object
            Return JsInvokeCore(f, args, receiver, FindMethodHome(receiver, Nothing))
        End Function

        ''' <summary>
        ''' Method call helper: resolves the member on <paramref name="obj"/> and binds
        ''' <c>this</c> to it.
        ''' <paramref name="accessOptional"/> mirrors <c>a?.b()</c> — short-circuit when the
        ''' OBJECT is nullish; <paramref name="callOptional"/> mirrors <c>a.b?.()</c> —
        ''' short-circuit when the METHOD is nullish.
        ''' </summary>
        Public Function JsInvokeMethod(obj As Object, name As String, args As Object(),
                                       Optional accessOptional As Boolean = False,
                                       Optional callOptional As Boolean = False) As Object
            If accessOptional AndAlso (obj Is Nothing OrElse obj Is Undef) Then
                Throw New OptionalChainShortCircuit()
            End If
            Dim f = JsGet(obj, name)
            If callOptional AndAlso (f Is Nothing OrElse f Is Undef) Then
                Throw New OptionalChainShortCircuit()
            End If
            Return JsInvokeCore(f, args, obj, FindMethodHome(obj, name))
        End Function

        ''' <summary>Indexed method call: <c>obj[key](args)</c> with the receiver bound.</summary>
        Public Function JsInvokeIndex(obj As Object, index As Object, args As Object(),
                                      Optional accessOptional As Boolean = False,
                                      Optional callOptional As Boolean = False) As Object
            If accessOptional AndAlso (obj Is Nothing OrElse obj Is Undef) Then
                Throw New OptionalChainShortCircuit()
            End If
            Dim f = JsIndex(obj, index)
            If callOptional AndAlso (f Is Nothing OrElse f Is Undef) Then
                Throw New OptionalChainShortCircuit()
            End If
            Dim name As String = Nothing
            If IsJsObject(obj) OrElse TypeOf obj Is JsClass Then name = JsStr(index)
            Return JsInvokeCore(f, args, obj, FindMethodHome(obj, name))
        End Function

        Private Function JsInvokeCore(f As Object, args As Object(), receiver As Object, home As JsClass) As Object
            Dim savedThis = _thisValue
            Dim savedHome = _homeClass
            _thisValue = receiver
            _homeClass = home
            Try
                Return DispatchCall(f, args)
            Finally
                _thisValue = savedThis
                _homeClass = savedHome
            End Try
        End Function

        Private Function DispatchCall(f As Object, args As Object()) As Object
            If TypeOf f Is Func(Of Object(), Object) Then
                Return DirectCast(f, Func(Of Object(), Object)).Invoke(args)
            End If
            If TypeOf f Is BuiltinMethod Then
                Dim m = DirectCast(f, BuiltinMethod)
                Return DispatchBuiltin(m.Target, m.Name, args)
            End If
            If TypeOf f Is JsClass Then
                Throw JsRuntimeException.TypeError(
                    "Class constructor " & DirectCast(f, JsClass).Name & " cannot be invoked without 'new'")
            End If
            ' ---- CLR reflection bridge: bound CLR methods ----
            If TypeOf f Is ClrBoundMethod Then
                Return ClrInterop.InvokeBound(DirectCast(f, ClrBoundMethod), args)
            End If
            If TypeOf f Is ClrType Then
                Throw JsRuntimeException.TypeError(
                    "Class constructor " & DirectCast(f, ClrType).UnderlyingType.Name & " cannot be invoked without 'new'")
            End If
            Throw JsRuntimeException.TypeError(JsStr(f) & " is not a function")
        End Function

        ' ================= construction =================

        ''' <summary>
        ''' The <c>new</c> operator: construct a class instance or a builtin object,
        ''' or run a plain function with a fresh object as <c>this</c>.
        ''' </summary>
        Public Function JsNew(ctor As Object, args As Object()) As Object
            ' ---- CLR reflection bridge: new T(...) runs a public constructor ----
            Dim clrType = TryCast(ctor, ClrType)
            If clrType IsNot Nothing Then Return ClrInterop.Construct(clrType, args)

            Dim cls = TryCast(ctor, JsClass)
            If cls IsNot Nothing Then Return Instantiate(cls, args)

            If TypeOf ctor Is Func(Of Object(), Object) Then
                ' function used as a constructor: fresh object bound to 'this'
                Dim inst As New Dictionary(Of String, Object)
                Call JsInvokeCore(ctor, args, inst, Nothing)
                Return inst
            End If

            If TypeOf ctor Is BuiltinMethod Then
                Return ConstructBuiltin(DirectCast(ctor, BuiltinMethod).Name, args)
            End If

            ' container constructors (Array / Object / Function / ...)
            Dim nativeCtor = TryCast(ctor, NativeObject)
            If nativeCtor IsNot Nothing Then Return ConstructBuiltin(nativeCtor.Name, args)

            Throw JsRuntimeException.TypeError(JsStr(ctor) & " is not a constructor")
        End Function

        Private Function Instantiate(cls As JsClass, args As Object()) As Object
            Dim inst As New Dictionary(Of String, Object)
            inst("__proto__") = cls.Prototype
            inst("__class__") = cls

            ' field initialisers run base-class first, before the constructor body
            Dim chain As New List(Of JsClass)
            Dim c = cls
            While c IsNot Nothing
                chain.Insert(0, c)
                c = c.SuperClass
            End While
            For Each cc In chain
                For Each f In cc.Fields
                    If f.Value Is Nothing Then
                        inst(f.Key) = Undef
                    Else
                        inst(f.Key) = JsInvokeCore(f.Value, New Object() {}, inst, cc)
                    End If
                Next
            Next

            ' an absent constructor forwards to the superclass (JS derived-class default)
            Dim ctorToRun = cls.Constructor
            Dim ctorHome = cls
            If ctorToRun Is Nothing Then
                Dim base = cls.SuperClass
                While base IsNot Nothing AndAlso base.Constructor Is Nothing
                    base = base.SuperClass
                End While
                If base IsNot Nothing Then
                    ctorToRun = base.Constructor
                    ctorHome = base
                End If
            End If

            If ctorToRun IsNot Nothing Then
                Call JsInvokeCore(ctorToRun, args, inst, ctorHome)
            End If
            Return inst
        End Function

        ''' <summary>Builtin constructors reachable through <c>new</c>.</summary>
        Private Function ConstructBuiltin(name As String, args As Object()) As Object
            Select Case name
                Case "Array"
                    If args.Length = 1 AndAlso IsNumber(args(0)) Then
                        Dim n = CInt(CDbl(args(0)))
                        Dim list As New List(Of Object)
                        For i = 1 To n
                            list.Add(Undef)
                        Next
                        Return list
                    End If
                    Return New List(Of Object)(args)
                Case "Object" : Return New Dictionary(Of String, Object)
                Case "String" : Return JsStr(Arg(args, 0))
                Case "Number" : Return JsNum(Arg(args, 0))
                Case "Boolean" : Return JsTruthy(Arg(args, 0))
                Case "RegExp"
                    If args.Length = 0 Then Return New JsRegExp("")
                    Dim src = Arg(args, 0)
                    If TypeOf src Is JsRegExp Then
                        Return New JsRegExp(DirectCast(src, JsRegExp).Source,
                                            If(args.Length > 1, JsStr(args(1)), DirectCast(src, JsRegExp).Flags))
                    End If
                    Return New JsRegExp(JsStr(src), If(args.Length > 1, JsStr(args(1)), ""))
                Case "Date"
                    If args.Length = 0 Then Return JsDate.Now()
                    If IsNumber(args(0)) Then Return JsDate.FromEpochMs(CDbl(args(0)))
                    Dim parsed = JsDate.Parse(JsStr(args(0)))
                    Return If(parsed IsNot Nothing, parsed, JsDate.FromEpochMs(Double.NaN))
                Case "Error", "TypeError", "RangeError", "SyntaxError", "ReferenceError"
                    Return New JsError(name, If(args.Length > 0 AndAlso args(0) IsNot Undef, JsStr(args(0)), ""))
                Case "Promise" : Return ConstructPromise(args)
            End Select
            Throw JsRuntimeException.TypeError("'" & name & "' is not a constructor")
        End Function

        ''' <summary>
        ''' Build a class value. <paramref name="parts"/> triples are
        ''' <c>("i", name, value)</c> instance members, <c>("s", name, value)</c> statics and
        ''' <c>("f", name, initialiser)</c> instance fields. Shared by the interpreter and
        ''' generated code so both build identical classes.
        ''' </summary>
        Public Function JsDefineClass(name As String, ctor As Object, superClass As Object,
                                     ParamArray parts As (Kind As String, Name As String, Value As Object)()) As Object
            Dim sup As JsClass = Nothing
            If superClass IsNot Nothing AndAlso Not (superClass Is Undef) Then
                sup = TryCast(superClass, JsClass)
                If sup Is Nothing Then
                    Throw JsRuntimeException.TypeError("Class extends value is not a constructor")
                End If
            End If

            Dim cls As New JsClass(name, ctor, sup)
            ' the derived prototype links to the base prototype
            If sup IsNot Nothing Then cls.Prototype("__proto__") = sup.Prototype

            For Each p In parts
                Select Case p.Kind
                    Case "i"
                        Dim acc = TryCast(p.Value, AccessorProperty)
                        If acc Is Nothing Then
                            cls.Prototype(p.Name) = p.Value
                        Else
                            JsAccessorDefine(cls.Prototype, p.Name, acc.Getter, acc.Setter)
                        End If
                    Case "s"
                        Dim acc = TryCast(p.Value, AccessorProperty)
                        If acc Is Nothing Then
                            cls.Statics(p.Name) = p.Value
                        Else
                            JsAccessorDefine(cls.Statics, p.Name, acc.Getter, acc.Setter)
                        End If
                    Case "f"
                        cls.Fields.Add(New KeyValuePair(Of String, Object)(p.Name, p.Value))
                End Select
            Next
            Return cls
        End Function

        ' ================= super =================

        ''' <summary>super(...) — run the superclass constructor against the current this.</summary>
        Public Function JsSuperConstructor(args As Object()) As Object
            Dim home = _homeClass
            If home Is Nothing OrElse home.SuperClass Is Nothing Then
                Throw JsRuntimeException.TypeError("'super' is only valid in a derived class")
            End If
            If home.SuperClass.Constructor IsNot Nothing Then
                Call JsInvokeCore(home.SuperClass.Constructor, args, _thisValue, home.SuperClass)
            End If
            Return _thisValue
        End Function

        ''' <summary>super.member — read the member from the superclass.</summary>
        Public Function JsSuperGet(name As String) As Object
            Dim home = _homeClass
            If home Is Nothing OrElse home.SuperClass Is Nothing Then Return Undef
            Return LookupSuperMember(home.SuperClass, name)
        End Function

        ''' <summary>super.member(args) — invoke with the current receiver.</summary>
        Public Function JsSuperInvoke(name As String, args As Object()) As Object
            Dim home = _homeClass
            If home Is Nothing OrElse home.SuperClass Is Nothing Then
                Throw JsRuntimeException.TypeError("'super." & name & "' is only valid in a derived class")
            End If
            Dim f = LookupSuperMember(home.SuperClass, name)
            Return JsInvokeCore(f, args, _thisValue, home.SuperClass)
        End Function

        ''' <summary>Look a member up on a class's prototype chain, then its statics.</summary>
        Private Function LookupSuperMember(cls As JsClass, name As String) As Object
            Dim v As Object = Nothing
            Dim c = cls
            While c IsNot Nothing
                If c.Prototype.TryGetValue(name, v) Then Return v
                c = c.SuperClass
            End While
            c = cls
            While c IsNot Nothing
                If c.Statics.TryGetValue(name, v) Then Return v
                c = c.SuperClass
            End While
            Return Undef
        End Function

        ' ================= prototype plumbing =================

        ''' <summary>The class of an instance dictionary, or Nothing for a plain object.</summary>
        Public Function InstanceClass(obj As Object) As JsClass
            Dim d = TryCast(obj, Dictionary(Of String, Object))
            If d Is Nothing Then Return Nothing
            Dim c As Object = Nothing
            If d.TryGetValue("__class__", c) Then Return TryCast(c, JsClass)
            Return Nothing
        End Function

        ''' <summary>
        ''' The class that provides member <paramref name="name"/> for <paramref name="obj"/> —
        ''' the anchor used by <c>super</c> inside that method.
        ''' </summary>
        Private Function FindMethodHome(obj As Object, name As String) As JsClass
            Dim cls = TryCast(obj, JsClass)
            If cls IsNot Nothing Then
                If name Is Nothing Then Return cls
                Dim c = cls
                While c IsNot Nothing
                    If c.Statics.ContainsKey(name) Then Return c
                    c = c.SuperClass
                End While
                Return Nothing
            End If

            Dim inst = InstanceClass(obj)
            If inst Is Nothing OrElse name Is Nothing Then Return inst
            Dim cur = inst
            While cur IsNot Nothing
                If cur.Prototype.ContainsKey(name) Then Return cur
                cur = cur.SuperClass
            End While
            Return Nothing
        End Function

        ''' <summary>Read a value that may be an accessor property.</summary>
        Private Function ReadMember(v As Object, receiver As Object, home As JsClass) As Object
            Dim acc = TryCast(v, AccessorProperty)
            If acc Is Nothing Then Return v
            If acc.Getter Is Nothing Then Return Undef
            Return JsInvokeCore(acc.Getter, New Object() {}, receiver, home)
        End Function

        ''' <summary>
        ''' Walk the <c>__proto__</c> chain of an instance dictionary looking for a
        ''' property; returns the raw value (accessors are not yet invoked).
        ''' </summary>
        Private Function LookupInstanceMember(d As Dictionary(Of String, Object), name As String,
                                              ByRef owner As JsClass, ByRef found As Boolean) As Object
            found = False
            Dim cur As Object = Nothing
            owner = InstanceClass(d)
            If Not d.TryGetValue("__proto__", cur) Then Return Nothing
            Dim guard = 0
            While IsJsObject(cur) AndAlso guard < 1000
                Dim pd = DirectCast(cur, Dictionary(Of String, Object))
                Dim v As Object = Nothing
                If pd.TryGetValue(name, v) Then
                    found = True
                    Return v
                End If
                If Not pd.TryGetValue("__proto__", cur) Then Exit While
                guard += 1
            End While
            Return Nothing
        End Function

        ' ================= construction helpers (used by generated code) =================

        Public Function JsArray(ParamArray elements As Object()) As Object
            Return New List(Of Object)(elements)
        End Function

        ''' <summary>Object literal from (key, value) tuples (VB tuple literals convert implicitly).</summary>
        Public Function JsObj(pairs As (Key As String, Value As Object)()) As Object
            Dim d As New Dictionary(Of String, Object)
            For Each p In pairs
                Dim acc = TryCast(p.Value, AccessorProperty)
                If acc Is Nothing Then
                    d(p.Key) = p.Value
                Else
                    ' merge get/set pairs declared for the same key
                    JsAccessorDefine(d, p.Key, acc.Getter, acc.Setter)
                End If
            Next
            Return d
        End Function

        ''' <summary>
        ''' Define a getter/setter on an object dictionary, merging with an
        ''' existing accessor pair ({get x(){}, set x(v){}} share one property).
        ''' </summary>
        Public Sub JsAccessorDefine(target As Dictionary(Of String, Object), key As String,
                                    getter As Object, setter As Object)
            Dim existing As Object = Nothing
            Dim acc As AccessorProperty = Nothing
            If target.TryGetValue(key, existing) Then acc = TryCast(existing, AccessorProperty)
            If acc Is Nothing Then
                target(key) = New AccessorProperty(getter, setter)
            Else
                target(key) = New AccessorProperty(If(getter, acc.Getter), If(setter, acc.Setter))
            End If
        End Sub

        ''' <summary>Keys for for-in: array/string indices (as strings) or object keys.</summary>
        Public Function JsForInKeys(obj As Object) As Object()
            If IsJsArray(obj) Then
                Dim a = DirectCast(obj, List(Of Object))
                Dim keys(a.Count - 1) As Object
                For i = 0 To a.Count - 1
                    keys(i) = i.ToString(CultureInfo.InvariantCulture)
                Next
                Return keys
            End If
            If TypeOf obj Is String Then
                Dim s = CStr(obj)
                Dim keys(s.Length - 1) As Object
                For i = 0 To s.Length - 1
                    keys(i) = i.ToString(CultureInfo.InvariantCulture)
                Next
                Return keys
            End If
            If IsJsObject(obj) Then
                ' internal keys (__proto__ / __class__) are not enumerable
                Dim keys As New List(Of Object)
                For Each k In DirectCast(obj, Dictionary(Of String, Object)).Keys
                    If k.StartsWith("__", StringComparison.Ordinal) Then Continue For
                    keys.Add(k)
                Next
                Return keys.ToArray()
            End If
            ' ---- CLR reflection bridge: indices for collections, data-member names otherwise ----
            Dim clrObj = TryCast(obj, ClrObject)
            If clrObj IsNot Nothing Then Return ClrInterop.MemberKeys(clrObj)

            Return New Object() {}
        End Function

        ' ================= builtin dispatch =================

        Private ReadOnly ArrayMethodNames As New HashSet(Of String) From {
            "push", "pop", "shift", "unshift", "join", "concat", "slice", "indexOf",
            "includes", "reverse", "sort", "map", "filter", "reduce", "forEach",
            "find", "findIndex", "findLast", "findLastIndex", "some", "every",
            "flat", "flatMap", "fill", "splice", "lastIndexOf", "reduceRight", "at",
            "keys", "values", "entries", "toString"
        }

        Private ReadOnly StringMethodNames As New HashSet(Of String) From {
            "charAt", "charCodeAt", "substring", "slice", "indexOf", "toUpperCase",
            "toLowerCase", "trim", "split", "replace", "includes", "startsWith",
            "endsWith", "repeat", "padStart", "toString",
            "at", "lastIndexOf", "padEnd", "trimStart", "trimEnd", "replaceAll",
            "match", "matchAll", "search", "concat", "codePointAt", "valueOf"
        }

        Private Function Arg(args As Object(), i As Integer) As Object
            If i < args.Length Then Return args(i)
            Return Undef
        End Function

        Private Function DispatchBuiltin(target As Object, name As String, args As Object()) As Object
            If TypeOf target Is NativeObject Then
                Select Case DirectCast(target, NativeObject).Name
                    Case "console" : Return BuiltinConsole(name, args)
                    Case "Math" : Return BuiltinMath(name, args)
                    Case "JSON" : Return BuiltinJson(name, args)
                    Case "Object" : Return BuiltinObject(name, args)
                    Case "Array" : Return BuiltinArrayCtor(name, args)
                    Case "Error" : Return BuiltinErrorCtor(name, args)
                    Case "Date" : Return BuiltinDateStatic(name, args)
                    Case "RegExp" : Return New JsRegExp(JsStr(Arg(args, 0)), If(args.Length > 1, JsStr(args(1)), ""))
                    Case "Promise" : Return BuiltinPromiseStatic(name, args)
                    Case "global" : Return BuiltinGlobalFn(name, args)
                End Select
            End If
            ' static helpers on the Number / String / Boolean function objects
            If TypeOf target Is BuiltinMethod Then
                Select Case DirectCast(target, BuiltinMethod).Name
                    Case "Number" : Return BuiltinNumberStatic(name, args)
                    Case "String" : Return BuiltinStringStatic(name, args)
                End Select
            End If
            If IsJsArray(target) Then Return BuiltinArrayMethod(DirectCast(target, List(Of Object)), name, args)
            If TypeOf target Is String Then Return BuiltinStringMethod(CStr(target), name, args)
            If TypeOf target Is JsGenerator Then
                Dim g = DirectCast(target, JsGenerator)
                Select Case name
                    Case "next" : Return g.Next2()
                    Case "return"
                        ' eager generators cannot resume: report exhaustion
                        Return New Dictionary(Of String, Object) From {
                            {"value", Arg(args, 0)}, {"done", True}
                        }
                    Case "toString" : Return g.ToString()
                End Select
                Throw JsRuntimeException.TypeError("Generator." & name & " is not a function")
            End If
            If TypeOf target Is JsPromise Then
                Dim p = DirectCast(target, JsPromise)
                Select Case name
                    Case "then" : Return p.Then(Arg(args, 0), Arg(args, 1))
                    Case "catch" : Return p.Catch(Arg(args, 0))
                    Case "finally" : Return p.Finally(Arg(args, 0))
                End Select
                Throw JsRuntimeException.TypeError("Promise." & name & " is not a function")
            End If
            If TypeOf target Is JsRegExp Then Return BuiltinRegExpMethod(DirectCast(target, JsRegExp), name, args)
            If TypeOf target Is JsDate Then Return BuiltinDateMethod(DirectCast(target, JsDate), name, args)
            If TypeOf target Is JsError Then
                If name = "toString" Then Return JsStr(target)
            End If
            If IsNumber(target) Then
                Dim d = CDbl(target)
                Select Case name
                    Case "toFixed"
                        Dim digits = CInt(JsNum(Arg(args, 0)))
                        Return d.ToString("F" & digits.ToString(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture)
                    Case "toString"
                        Dim radix = CInt(JsNum(Arg(args, 0)))
                        If radix >= 2 AndAlso radix <= 36 Then Return RadixString(d, radix)
                        Return NumToString(d)
                    Case "toPrecision"
                        If args.Length = 0 OrElse Arg(args, 0) Is Undef Then Return NumToString(d)
                        Return d.ToString("G" & CInt(JsNum(args(0))).ToString(CultureInfo.InvariantCulture),
                                          CultureInfo.InvariantCulture)
                End Select
            End If
            Throw JsRuntimeException.TypeError($"{JsStr(target)}.{name} is not a function")
        End Function

        ''' <summary>Number.prototype.toString(radix) for 2..36.</summary>
        Private Function RadixString(d As Double, radix As Integer) As String
            If Double.IsNaN(d) Then Return "NaN"
            If Double.IsInfinity(d) Then Return If(d > 0, "Infinity", "-Infinity")
            If d = System.Math.Truncate(d) AndAlso System.Math.Abs(d) < 9.0E+15 Then
                Dim n = CLng(d)
                Dim negative = n < 0
                If negative Then n = -n
                Dim digits = "0123456789abcdefghijklmnopqrstuvwxyz"
                Dim sb As New StringBuilder()
                If n = 0 Then Return "0"
                While n > 0
                    sb.Insert(0, digits(CInt(n Mod radix)))
                    n = n \ radix
                End While
                Return If(negative, "-", "") & sb.ToString()
            End If
            Return NumToString(d)
        End Function

        Private Function BuiltinConsole(name As String, args As Object()) As Object
            Dim parts(args.Length - 1) As String
            For i = 0 To args.Length - 1
                parts(i) = JsDisplay(args(i))
            Next
            Dim prefix = If(name = "error", "[error] ", If(name = "warn", "[warn] ", ""))
            Io.WriteLine(prefix & String.Join(" ", parts))
            Return Undef
        End Function

        Private Function BuiltinMath(name As String, args As Object()) As Object
            Select Case name
                Case "abs" : Return System.Math.Abs(JsNum(Arg(args, 0)))
                Case "ceil" : Return System.Math.Ceiling(JsNum(Arg(args, 0)))
                Case "floor" : Return System.Math.Floor(JsNum(Arg(args, 0)))
                Case "round"
                    Dim d = JsNum(Arg(args, 0))
                    ' JS rounds .5 up (towards +infinity)
                    Return System.Math.Floor(d + 0.5)
                Case "trunc" : Return System.Math.Truncate(JsNum(Arg(args, 0)))
                Case "sqrt" : Return System.Math.Sqrt(JsNum(Arg(args, 0)))
                Case "cbrt"
                    Dim d = JsNum(Arg(args, 0))
                    Return If(d < 0, -System.Math.Pow(-d, 1.0 / 3.0), System.Math.Pow(d, 1.0 / 3.0))
                Case "pow" : Return System.Math.Pow(JsNum(Arg(args, 0)), JsNum(Arg(args, 1)))
                Case "exp" : Return System.Math.Exp(JsNum(Arg(args, 0)))
                Case "expm1" : Return System.Math.Exp(JsNum(Arg(args, 0))) - 1.0
                Case "log" : Return System.Math.Log(JsNum(Arg(args, 0)))
                Case "log1p" : Return System.Math.Log(1.0 + JsNum(Arg(args, 0)))
                Case "log2" : Return System.Math.Log(JsNum(Arg(args, 0)), 2.0)
                Case "log10" : Return System.Math.Log10(JsNum(Arg(args, 0)))
                Case "sin" : Return System.Math.Sin(JsNum(Arg(args, 0)))
                Case "cos" : Return System.Math.Cos(JsNum(Arg(args, 0)))
                Case "tan" : Return System.Math.Tan(JsNum(Arg(args, 0)))
                Case "asin" : Return System.Math.Asin(JsNum(Arg(args, 0)))
                Case "acos" : Return System.Math.Acos(JsNum(Arg(args, 0)))
                Case "atan" : Return System.Math.Atan(JsNum(Arg(args, 0)))
                Case "atan2" : Return System.Math.Atan2(JsNum(Arg(args, 0)), JsNum(Arg(args, 1)))
                Case "sinh" : Return System.Math.Sinh(JsNum(Arg(args, 0)))
                Case "cosh" : Return System.Math.Cosh(JsNum(Arg(args, 0)))
                Case "tanh" : Return System.Math.Tanh(JsNum(Arg(args, 0)))
                Case "sign"
                    Dim d = JsNum(Arg(args, 0))
                    Return If(Double.IsNaN(d) OrElse d = 0.0, If(d = 0.0, 0.0, Double.NaN), System.Math.Sign(d))
                Case "min"
                    Dim r = Double.PositiveInfinity
                    For Each a In args
                        Dim v = JsNum(a)
                        If Double.IsNaN(v) Then Return Double.NaN
                        If v < r Then r = v
                    Next
                    Return r
                Case "max"
                    Dim r = Double.NegativeInfinity
                    For Each a In args
                        Dim v = JsNum(a)
                        If Double.IsNaN(v) Then Return Double.NaN
                        If v > r Then r = v
                    Next
                    Return r
                Case "random" : Return New Random().NextDouble()
                Case "hypot"
                    Dim s = 0.0
                    For Each a In args
                        Dim v = JsNum(a)
                        s += v * v
                    Next
                    Return System.Math.Sqrt(s)
                Case "imul"
                    ' 32 bit integer multiplication of the ToInt32 operands
                    Dim x = ToInt32Bits(JsNum(Arg(args, 0))), y = ToInt32Bits(JsNum(Arg(args, 1)))
                    Return WrapInt32(x * y)
                Case "clz32"
                    Dim u = ToUint32Bits(JsNum(Arg(args, 0)))
                    If u = 0 Then Return 32.0
                    Dim n = 0
                    While (u And &H80000000L) = 0
                        u <<= 1
                        n += 1
                    End While
                    Return CDbl(n)
                Case "fround"
                    ' Math.fround: round to the nearest 32 bit float
                    Return CDbl(CSng(JsNum(Arg(args, 0))))
            End Select
            Throw JsRuntimeException.TypeError("Math." & name & " is not a function")
        End Function

        Private Function BuiltinJson(name As String, args As Object()) As Object
            Select Case name
                Case "stringify"
                    ' the optional third argument (space) switches to indented output
                    Dim space = args.Length > 2 AndAlso Not (args(2) Is Undef) AndAlso JsTruthy(args(2))
                    Return JsonStringify(Arg(args, 0), space)
                Case "parse" : Return JsParseJson(JsStr(Arg(args, 0)))
            End Select
            Throw JsRuntimeException.TypeError("JSON." & name & " is not a function")
        End Function

        ' ========================================================
        ' JSON interop (based on Microsoft.VisualBasic.MIME.application.json)
        ' ========================================================

        ''' <summary>JSONSerializer 输出选项：紧凑、非 unicode 转义、Date 输出 ISO 字符串</summary>
        Private Function JsonOpts() As JSONSerializerOptions
            Return New JSONSerializerOptions With {
                .indent = False,
                .unicodeEscape = False,
                .unixTimestamp = False
            }
        End Function

        ''' <summary>对象/数组的 JSON 风格文本（console 显示与 JSON.stringify 共用）</summary>
        Private Function DisplayJson(x As Object, Optional indent As Boolean = False) As String
            Dim opts = JsonOpts()
            opts.indent = indent
            ' TrimEnd: the json writer terminates the document with a newline
            Return ToJsonElement(x, New HashSet(Of Object)).BuildJsonString(opts).TrimEnd()
        End Function

        ''' <summary>
        ''' JS 值图 → <see cref="JsonElement"/> DOM（JSON.stringify 语义）。
        ''' </summary>
        Private Function ToJsonElement(x As Object, ancestors As HashSet(Of Object)) As JsonElement
            If x Is Nothing OrElse x Is Undef Then
                Return JsonValue.NULL
            ElseIf TypeOf x Is Double Then
                ' NaN / ±Infinity → null (JS JSON.stringify standard)
                Dim d = CDbl(x)
                If Double.IsNaN(d) OrElse Double.IsInfinity(d) Then
                    Return JsonValue.NULL
                End If
                Return New JsonValue(d)
            ElseIf TypeOf x Is Boolean Then
                Return New JsonValue(CBool(x))
            ElseIf TypeOf x Is String Then
                Return New JsonValue(CStr(x))
            ElseIf TypeOf x Is Single OrElse TypeOf x Is Integer OrElse TypeOf x Is Long Then
                ' rendered as unquoted json numbers by JSONWriter
                Return New JsonValue(x)
            ElseIf TypeOf x Is List(Of Object) Then
                If Not ancestors.Add(x) Then
                    Throw JsRuntimeException.TypeError("Converting circular structure to JSON")
                End If
                Dim arr As New JsonArray
                For Each el In DirectCast(x, List(Of Object))
                    arr.Add(ToJsonElement(el, ancestors))
                Next
                Call ancestors.Remove(x)
                Return arr
            ElseIf TypeOf x Is Dictionary(Of String, Object) Then
                If Not ancestors.Add(x) Then
                    Throw JsRuntimeException.TypeError("Converting circular structure to JSON")
                End If
                Dim obj As New JsonObject
                For Each kv In DirectCast(x, Dictionary(Of String, Object))
                    Dim v = kv.Value
                    ' JS standard: undefined / function members are omitted
                    If v Is Undef OrElse TypeOf v Is Func(Of Object(), Object) OrElse TypeOf v Is BuiltinMethod Then
                        Continue For
                    End If
                    ' accessors are not own data properties → JSON.stringify skips them
                    If TypeOf v Is AccessorProperty Then Continue For
                    obj.Add(kv.Key, ToJsonElement(v, ancestors))
                Next
                Call ancestors.Remove(x)
                Return obj
            ElseIf TypeOf x Is ClrObject Then
                ' ---- CLR reflection bridge: visible members (collections as arrays) ----
                Dim clr = DirectCast(x, ClrObject)
                If Not ancestors.Add(x) Then
                    Throw JsRuntimeException.TypeError("Converting circular structure to JSON")
                End If
                Dim el As JsonElement
                If ClrInterop.IsCollection(clr) Then
                    Dim items As New JsonArray
                    For Each item In ClrInterop.JsonItems(clr)
                        items.Add(ToJsonElement(item, ancestors))
                    Next
                    el = items
                Else
                    Dim members As New JsonObject
                    For Each kv In ClrInterop.JsonMembers(clr)
                        members.Add(kv.Key, ToJsonElement(kv.Value, ancestors))
                    Next
                    el = members
                End If
                Call ancestors.Remove(x)
                Return el
            Else
                ' functions and other opaque clr objects → null
                Return JsonValue.NULL
            End If
        End Function

        ''' <summary>
        ''' <see cref="JsonElement"/> DOM → JS 值图（JSON.parse 语义）。
        ''' </summary>
        Private Function FromJsonElement(el As JsonElement) As Object
            If el Is Nothing Then
                Return Nothing
            ElseIf TypeOf el Is JsonObject Then
                Dim d As New Dictionary(Of String, Object)
                For Each member In DirectCast(el, JsonObject)
                    d(member.Name) = FromJsonElement(member.Value)
                Next
                Return d
            ElseIf TypeOf el Is JsonArray Then
                Dim list As New List(Of Object)
                For Each item In DirectCast(el, JsonArray)
                    list.Add(FromJsonElement(item))
                Next
                Return list
            Else
                Dim v = DirectCast(el, JsonValue)

                If v.value Is Nothing Then
                    Return Nothing
                ElseIf TypeOf v.value Is Double OrElse TypeOf v.value Is Boolean OrElse
                       TypeOf v.value Is Integer OrElse TypeOf v.value Is Long OrElse
                       TypeOf v.value Is Single Then
                    ' native clr values (host-constructed graphs)
                    Return v.value
                End If

                ' parsed scalars arrive as raw literal text:
                ' quoted → js string, otherwise number/boolean/null literal
                Dim raw = CStr(v.value)

                If raw.StartsWith(""""c) Then
                    Return v.GetStripString(decodeMetachar:=True)
                End If

                Dim text = raw.Trim()

                If text = "null" Then
                    Return Nothing
                ElseIf text = "true" Then
                    Return True
                ElseIf text = "false" Then
                    Return False
                Else
                    Dim num As Double
                    If Double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, num) Then
                        Return num
                    Else
                        Return v.GetStripString(decodeMetachar:=True)
                    End If
                End If
            End If
        End Function

        ''' <summary>
        ''' JSON.parse：解析失败抛出可被脚本 try/catch 捕获的 SyntaxError。
        ''' </summary>
        Public Function JsParseJson(text As String) As Object
            If text Is Nothing OrElse text.Trim().Length = 0 Then
                Throw JsRuntimeException.SyntaxError("Unexpected end of JSON input")
            End If

            Dim trimmed = text.Trim()

            ' the strict-mode json tokenicer rejects top-level scalar documents
            ' (a buffered literal at the end of the stream) — handle the scalar
            ' cases directly here with strict JS json grammar
            Dim head As Char = trimmed(0)

            If head <> "{"c AndAlso head <> "["c Then
                Return ParseScalarLiteral(trimmed)
            End If

            Dim el As JsonElement

            Try
                el = JsonParser.Parse(text)
            Catch ex As Exception
                If TypeOf ex Is JsRuntimeException Then
                    Throw
                End If
                Throw JsRuntimeException.SyntaxError(ex.Message)
            End Try

            If el Is Nothing Then
                ' the parser returns Nothing on tokenizer errors / malformed input
                Throw JsRuntimeException.SyntaxError("Unexpected token in JSON")
            End If

            Return FromJsonElement(el)
        End Function

        ''' <summary>
        ''' 顶层标量字面量（null/true/false/string/number）的 JS 严格解析。
        ''' </summary>
        Private Function ParseScalarLiteral(text As String) As Object
            If text = "null" Then
                Return Nothing
            ElseIf text = "true" Then
                Return True
            ElseIf text = "false" Then
                Return False
            ElseIf text.StartsWith(""""c) Then
                ' quoted string: must be terminated by the closing quote
                If text.Length < 2 OrElse Not text.EndsWith(""""c) Then
                    Throw JsRuntimeException.SyntaxError("Unexpected end of JSON input")
                End If
                ' strip the outer quotes, then decode the escape sequences
                ' (same as the tokenicer + StripString pipeline for string tokens)
                Return JsonParser.StripString(text.Substring(1, text.Length - 2), decodeMetaChar:=True)
            Else
                ' strict JS json number: -?(0|[1-9]\d*)(\.\d+)?([eE][+-]?\d+)?
                Dim num As Double
                If Regex.IsMatch(text, "^-?(0|[1-9]\d*)(\.\d+)?([eE][+-]?\d+)?$") AndAlso
                   Double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, num) Then
                    Return num
                End If
                Throw JsRuntimeException.SyntaxError($"Unexpected token '{text}' in JSON")
            End If
        End Function

        ''' <summary>
        ''' JSON.stringify（JS 标准语义）：
        ''' 顶层 undefined/函数返回 undefined 值本身；对象中 undefined/函数成员省略；
        ''' 数组中转 null；NaN/±Infinity 转 null；循环引用抛 TypeError。
        ''' </summary>
        Private Function JsonStringify(x As Object, Optional indent As Boolean = False) As Object
            If x Is Nothing Then
                Return "null"
            ElseIf x Is Undef OrElse TypeOf x Is Func(Of Object(), Object) OrElse TypeOf x Is BuiltinMethod Then
                Return Undef
            Else
                Return DisplayJson(x, indent)
            End If
        End Function

        Private Function BuiltinObject(name As String, args As Object()) As Object
            Select Case name
                Case "keys"
                    Dim o = Arg(args, 0)
                    Dim keys = JsForInKeys(o)
                    Return New List(Of Object)(keys)
                Case "values"
                    Dim o = Arg(args, 0)
                    If IsJsObject(o) Then
                        Dim vals As New List(Of Object)
                        For Each k In JsForInKeys(o)
                            vals.Add(JsGet(o, JsStr(k)))
                        Next
                        Return vals
                    End If
                    Return New List(Of Object)()
                Case "entries"
                    Dim o = Arg(args, 0)
                    Dim entries As New List(Of Object)
                    For Each k In JsForInKeys(o)
                        entries.Add(New List(Of Object) From {k, JsGet(o, JsStr(k))})
                    Next
                    Return entries
                Case "fromEntries"
                    Dim src = Arg(args, 0)
                    Dim d As New Dictionary(Of String, Object)
                    If IsJsArray(src) Then
                        For Each pair In DirectCast(src, List(Of Object))
                            If IsJsArray(pair) Then
                                Dim p = DirectCast(pair, List(Of Object))
                                If p.Count >= 2 Then d(JsStr(p(0))) = p(1)
                            End If
                        Next
                    End If
                    Return d
                Case "assign"
                    Dim target = Arg(args, 0)
                    If IsJsObject(target) Then
                        Dim d = DirectCast(target, Dictionary(Of String, Object))
                        For i = 1 To args.Length - 1
                            Dim src = args(i)
                            If IsJsObject(src) Then
                                For Each kv In DirectCast(src, Dictionary(Of String, Object))
                                    If kv.Key.StartsWith("__", StringComparison.Ordinal) Then Continue For
                                    d(kv.Key) = ReadMember(kv.Value, src, Nothing)
                                Next
                            End If
                        Next
                    End If
                    Return target
                Case "getOwnPropertyNames"
                    Dim o = Arg(args, 0)
                    Dim names As New List(Of Object)
                    If IsJsObject(o) Then
                        For Each k In DirectCast(o, Dictionary(Of String, Object)).Keys
                            If k.StartsWith("__", StringComparison.Ordinal) Then Continue For
                            names.Add(k)
                        Next
                    ElseIf TypeOf o Is ClrObject Then
                        ' ---- CLR reflection bridge ----
                        names.AddRange(ClrInterop.MemberKeys(DirectCast(o, ClrObject)))
                    End If
                    Return names
                Case "freeze", "seal"
                    ' the object model is mutable; freeze/seal return the object unchanged
                    Return Arg(args, 0)
                Case "isFrozen", "isSealed"
                    Return False
                Case "create"
                    Dim d As New Dictionary(Of String, Object)
                    Dim proto = Arg(args, 0)
                    If proto IsNot Nothing AndAlso Not (proto Is Undef) Then
                        If IsJsObject(proto) Then d("__proto__") = proto
                    End If
                    Return d
            End Select
            Throw JsRuntimeException.TypeError("Object." & name & " is not a function")
        End Function

        Private Function BuiltinArrayCtor(name As String, args As Object()) As Object
            Select Case name
                Case "isArray" : Return IsJsArray(Arg(args, 0))
                Case "from"
                    Dim o = Arg(args, 0)
                    If IsJsArray(o) Then Return New List(Of Object)(DirectCast(o, List(Of Object)))
                    Return New List(Of Object)()
            End Select
            Throw JsRuntimeException.TypeError("Array." & name & " is not a function")
        End Function

        Private Function BuiltinGlobalFn(name As String, args As Object()) As Object
            Select Case name
                Case "String" : Return JsStr(Arg(args, 0))
                Case "Number" : Return JsNum(Arg(args, 0))
                Case "Boolean" : Return JsTruthy(Arg(args, 0))
                Case "isNaN" : Return Double.IsNaN(JsNum(Arg(args, 0)))
                Case "isFinite" : Return Not Double.IsNaN(JsNum(Arg(args, 0)))
                Case "parseInt" : Return ParseIntPrefix(JsStr(Arg(args, 0)))
                Case "parseFloat" : Return ParseFloatPrefix(JsStr(Arg(args, 0)))
                ' the Error hierarchy is callable without 'new', returning an error object
                Case "Error", "TypeError", "RangeError", "SyntaxError", "ReferenceError"
                    Return BuiltinErrorCtor(name, args)
            End Select
            Throw JsRuntimeException.TypeError(name & " is not a function")
        End Function

        Private Function ParseIntPrefix(s As String) As Object
            Dim t = s.Trim()
            Dim sign = 1.0
            If t.StartsWith("-"c) Then sign = -1.0 : t = t.Substring(1)
            If t.StartsWith("+"c) Then t = t.Substring(1)
            If t.StartsWith("0x", StringComparison.OrdinalIgnoreCase) Then
                t = t.Substring(2)
                Dim i = 0
                While i < t.Length AndAlso Uri.IsHexDigit(t(i))
                    i += 1
                End While
                If i = 0 Then Return Double.NaN
                Return sign * Convert.ToInt64(t.Substring(0, i), 16)
            End If
            Dim n = 0
            While n < t.Length AndAlso Char.IsDigit(t(n))
                n += 1
            End While
            If n = 0 Then Return Double.NaN
            Return sign * CDbl(t.Substring(0, n))
        End Function

        Private Function ParseFloatPrefix(s As String) As Object
            Dim t = s.Trim()
            Dim n = 0
            If n < t.Length AndAlso (t(n) = "+"c OrElse t(n) = "-"c) Then n += 1
            While n < t.Length AndAlso Char.IsDigit(t(n)) : n += 1 : End While
            If n < t.Length AndAlso t(n) = "."c Then
                n += 1
                While n < t.Length AndAlso Char.IsDigit(t(n)) : n += 1 : End While
            End If
            If n < t.Length AndAlso (t(n) = "e"c OrElse t(n) = "E"c) Then
                Dim save = n
                n += 1
                If n < t.Length AndAlso (t(n) = "+"c OrElse t(n) = "-"c) Then n += 1
                If n < t.Length AndAlso Char.IsDigit(t(n)) Then
                    While n < t.Length AndAlso Char.IsDigit(t(n)) : n += 1 : End While
                Else
                    n = save
                End If
            End If
            If n = 0 Then Return Double.NaN
            Dim v As Double
            If Double.TryParse(t.Substring(0, n), NumberStyles.Float, CultureInfo.InvariantCulture, v) Then Return v
            Return Double.NaN
        End Function

        Private Function BuiltinArrayMethod(a As List(Of Object), name As String, args As Object()) As Object
            Select Case name
                Case "push"
                    For Each v In args
                        a.Add(v)
                    Next
                    Return CDbl(a.Count)
                Case "pop"
                    If a.Count = 0 Then Return Undef
                    Dim v = a(a.Count - 1)
                    a.RemoveAt(a.Count - 1)
                    Return v
                Case "shift"
                    If a.Count = 0 Then Return Undef
                    Dim v = a(0)
                    a.RemoveAt(0)
                    Return v
                Case "unshift"
                    For i = args.Length - 1 To 0 Step -1
                        a.Insert(0, args(i))
                    Next
                    Return CDbl(a.Count)
                Case "join"
                    Dim sep = If(args.Length > 0, JsStr(args(0)), ",")
                    Return JoinArray(a, sep)
                Case "concat"
                    Dim r As New List(Of Object)(a)
                    For Each v In args
                        If IsJsArray(v) Then
                            r.AddRange(DirectCast(v, List(Of Object)))
                        Else
                            r.Add(v)
                        End If
                    Next
                    Return r
                Case "slice"
                    Dim len = a.Count
                    Dim start = NormIndex(JsNum(Arg(args, 0)), len)
                    Dim ending As Integer
                    If args.Length < 2 OrElse Arg(args, 1) Is Undef Then
                        ending = len
                    Else
                        ending = NormIndex(JsNum(args(1)), len)
                    End If
                    Return New List(Of Object)(a.GetRange(start, System.Math.Max(0, ending - start)))
                Case "indexOf"
                    Dim r = -1
                    For i = 0 To a.Count - 1
                        If JsStrictEq(a(i), Arg(args, 0)) Then r = i : Exit For
                    Next
                    Return CDbl(r)
                Case "includes"
                    For Each v In a
                        If JsStrictEq(v, Arg(args, 0)) Then Return True
                    Next
                    Return False
                Case "reverse"
                    a.Reverse()
                    Return a
                Case "sort"
                    If args.Length > 0 AndAlso TypeOf args(0) Is Func(Of Object(), Object) Then
                        Dim cmp = DirectCast(args(0), Func(Of Object(), Object))
                        Dim proxy As New List(Of Object)(a)
                        proxy.Sort(Function(x, y) CInt(System.Math.Floor(JsNum(JsInvoke(cmp, {x, y})))))
                        a.Clear()
                        a.AddRange(proxy)
                    Else
                        ' JS default sort: string comparison
                        Dim proxy As New List(Of Object)(a)
                        proxy.Sort(Function(x, y) String.Compare(JsStr(x), JsStr(y), StringComparison.Ordinal))
                        a.Clear()
                        a.AddRange(proxy)
                    End If
                    Return a
                Case "map"
                    Dim fn = DirectCast(Arg(args, 0), Func(Of Object(), Object))
                    Dim r As New List(Of Object)
                    For i = 0 To a.Count - 1
                        r.Add(JsInvoke(fn, {a(i), CDbl(i), a}))
                    Next
                    Return r
                Case "filter"
                    Dim fn = DirectCast(Arg(args, 0), Func(Of Object(), Object))
                    Dim r As New List(Of Object)
                    For i = 0 To a.Count - 1
                        If JsTruthy(JsInvoke(fn, {a(i), CDbl(i), a})) Then r.Add(a(i))
                    Next
                    Return r
                Case "reduce"
                    Dim fn = DirectCast(Arg(args, 0), Func(Of Object(), Object))
                    Dim acc As Object
                    Dim startIdx As Integer
                    If args.Length > 1 Then
                        acc = args(1)
                        startIdx = 0
                    ElseIf a.Count > 0 Then
                        acc = a(0)
                        startIdx = 1
                    Else
                        Throw JsRuntimeException.TypeError("reduce of empty array with no initial value")
                    End If
                    For i = startIdx To a.Count - 1
                        acc = JsInvoke(fn, {acc, a(i), CDbl(i), a})
                    Next
                    Return acc
                Case "forEach"
                    Dim fn = DirectCast(Arg(args, 0), Func(Of Object(), Object))
                    For i = 0 To a.Count - 1
                        JsInvoke(fn, {a(i), CDbl(i), a})
                    Next
                    Return Undef
                Case "find", "findIndex", "findLast", "findLastIndex"
                    Dim fn = DirectCast(Arg(args, 0), Func(Of Object(), Object))
                    Dim backwards = name = "findLast" OrElse name = "findLastIndex"
                    Dim from = If(backwards, a.Count - 1, 0)
                    Dim [to] = If(backwards, 0, a.Count - 1)
                    Dim delta = If(backwards, -1, 1)
                    Dim i = from
                    While If(backwards, i >= [to], i <= [to])
                        If JsTruthy(JsInvoke(fn, {a(i), CDbl(i), a})) Then
                            If name = "findIndex" OrElse name = "findLastIndex" Then Return CDbl(i)
                            Return a(i)
                        End If
                        i += delta
                    End While
                    If name = "findIndex" OrElse name = "findLastIndex" Then Return -1.0
                    Return Undef
                Case "some", "every"
                    Dim fn = DirectCast(Arg(args, 0), Func(Of Object(), Object))
                    For i = 0 To a.Count - 1
                        Dim hit = JsTruthy(JsInvoke(fn, {a(i), CDbl(i), a}))
                        If name = "some" AndAlso hit Then Return True
                        If name = "every" AndAlso Not hit Then Return False
                    Next
                    Return name = "every"
                Case "flat"
                    Dim depth = If(args.Length > 0 AndAlso Arg(args, 0) IsNot Undef, CInt(JsNum(args(0))), 1)
                    Return FlattenArray(a, depth)
                Case "flatMap"
                    Dim fn = DirectCast(Arg(args, 0), Func(Of Object(), Object))
                    Dim mapped As New List(Of Object)
                    For i = 0 To a.Count - 1
                        mapped.Add(JsInvoke(fn, {a(i), CDbl(i), a}))
                    Next
                    Return FlattenArray(mapped, 1)
                Case "fill"
                    Dim value = Arg(args, 0)
                    Dim start = NormIndex(JsNum(Arg(args, 1)), a.Count)
                    Dim ending = If(args.Length > 2 AndAlso Arg(args, 2) IsNot Undef,
                                    NormIndex(JsNum(args(2)), a.Count), a.Count)
                    For i = start To ending - 1
                        a(i) = value
                    Next
                    Return a
                Case "splice"
                    ' splice(start, deleteCount, ...items)
                    Dim start = NormIndex(JsNum(Arg(args, 0)), a.Count)
                    Dim deleteCount As Integer
                    If args.Length <= 1 Then
                        deleteCount = a.Count - start
                    ElseIf Arg(args, 1) Is Undef Then
                        deleteCount = 0
                    Else
                        deleteCount = System.Math.Max(0, System.Math.Min(CInt(JsNum(args(1))), a.Count - start))
                    End If
                    Dim removed As New List(Of Object)
                    For i = 1 To deleteCount
                        removed.Add(a(start))
                        a.RemoveAt(start)
                    Next
                    Dim inserts As New List(Of Object)
                    For i = 2 To args.Length - 1
                        inserts.Add(args(i))
                    Next
                    If inserts.Count > 0 Then a.InsertRange(start, inserts)
                    Return removed
                Case "lastIndexOf"
                    Dim from = If(args.Length > 1 AndAlso Arg(args, 1) IsNot Undef,
                                  System.Math.Min(CInt(JsNum(args(1))), a.Count - 1), a.Count - 1)
                    For i = from To 0 Step -1
                        If JsStrictEq(a(i), Arg(args, 0)) Then Return CDbl(i)
                    Next
                    Return -1.0
                Case "reduceRight"
                    Dim fn = DirectCast(Arg(args, 0), Func(Of Object(), Object))
                    Dim acc As Object
                    Dim startIdx As Integer
                    If args.Length > 1 Then
                        acc = args(1)
                        startIdx = a.Count - 1
                    ElseIf a.Count > 0 Then
                        acc = a(a.Count - 1)
                        startIdx = a.Count - 2
                    Else
                        Throw JsRuntimeException.TypeError("reduce of empty array with no initial value")
                    End If
                    For i = startIdx To 0 Step -1
                        acc = JsInvoke(fn, {acc, a(i), CDbl(i), a})
                    Next
                    Return acc
                Case "at"
                    Dim i = CInt(JsNum(Arg(args, 0)))
                    If i < 0 Then i += a.Count
                    If i < 0 OrElse i >= a.Count Then Return Undef
                    Return a(i)
                Case "keys"
                    Dim ks As New List(Of Object)
                    For i = 0 To a.Count - 1
                        ks.Add(CDbl(i))
                    Next
                    Return ks
                Case "values"
                    Return New List(Of Object)(a)
                Case "entries"
                    Dim es As New List(Of Object)
                    For i = 0 To a.Count - 1
                        es.Add(New List(Of Object) From {CDbl(i), a(i)})
                    Next
                    Return es
                Case "toString"
                    Return JoinArray(a, ",")
            End Select
            Throw JsRuntimeException.TypeError("array." & name & " is not a function")
        End Function

        ''' <summary>Array.prototype.flat — flatten nested arrays up to <paramref name="depth"/>.</summary>
        Private Function FlattenArray(a As List(Of Object), depth As Integer) As Object
            Dim result As New List(Of Object)
            For Each v In a
                If depth > 0 AndAlso IsJsArray(v) Then
                    Dim inner = DirectCast(FlattenArray(DirectCast(v, List(Of Object)), depth - 1), List(Of Object))
                    result.AddRange(inner)
                Else
                    result.Add(v)
                End If
            Next
            Return result
        End Function

        Private Function NormIndex(d As Double, len As Integer) As Integer
            Dim i As Integer
            If Double.IsNaN(d) Then d = 0.0
            If d < 0 Then
                i = len + CInt(System.Math.Floor(d))
                If i < 0 Then i = 0
            Else
                i = CInt(System.Math.Floor(d))
                If i > len Then i = len
            End If
            Return i
        End Function

        Private Function BuiltinStringMethod(s As String, name As String, args As Object()) As Object
            Select Case name
                Case "charAt"
                    Dim i = CInt(JsNum(Arg(args, 0)))
                    If i < 0 OrElse i >= s.Length Then Return ""
                    Return s.Substring(i, 1)
                Case "charCodeAt"
                    Dim i = CInt(JsNum(Arg(args, 0)))
                    If i < 0 OrElse i >= s.Length Then Return Double.NaN
                    Return CDbl(AscW(s(i)))
                Case "substring"
                    Dim a = CInt(System.Math.Max(0.0, System.Math.Min(CDbl(s.Length), JsNum(Arg(args, 0)))))
                    Dim b = If(args.Length < 2 OrElse Arg(args, 1) Is Undef, s.Length,
                               CInt(System.Math.Max(0.0, System.Math.Min(CDbl(s.Length), JsNum(args(1))))))
                    If a > b Then Dim t = a : a = b : b = t
                    Return s.Substring(a, b - a)
                Case "slice"
                    Dim start = NormIndex(JsNum(Arg(args, 0)), s.Length)
                    Dim ending As Integer
                    If args.Length < 2 OrElse Arg(args, 1) Is Undef Then
                        ending = s.Length
                    Else
                        ending = NormIndex(JsNum(args(1)), s.Length)
                    End If
                    If ending <= start Then Return ""
                    Return s.Substring(start, ending - start)
                Case "indexOf"
                    Return CDbl(s.IndexOf(JsStr(Arg(args, 0)), StringComparison.Ordinal))
                Case "toUpperCase" : Return s.ToUpperInvariant()
                Case "toLowerCase" : Return s.ToLowerInvariant()
                Case "trim" : Return s.Trim()
                Case "split"
                    If args.Length = 0 OrElse Arg(args, 0) Is Undef Then Return New List(Of Object) From {s}
                    Dim limit = If(args.Length > 1 AndAlso Arg(args, 1) IsNot Undef, CInt(JsNum(args(1))), -1)
                    Dim sepRe = TryCast(args(0), JsRegExp)
                    Dim parts As New List(Of Object)
                    If sepRe IsNot Nothing Then
                        For Each piece In sepRe.Engine.Split(s)
                            parts.Add(piece)
                        Next
                    Else
                        Dim sep = JsStr(args(0))
                        If sep = "" Then
                            For Each c In s
                                parts.Add(c.ToString())
                            Next
                        Else
                            For Each piece In s.Split({sep}, StringSplitOptions.None)
                                parts.Add(piece)
                            Next
                        End If
                    End If
                    If limit >= 0 AndAlso parts.Count > limit Then parts = parts.GetRange(0, limit)
                    Return parts
                Case "replace" : Return JsStringReplace(s, Arg(args, 0), Arg(args, 1), replaceAll:=False)
                Case "replaceAll" : Return JsStringReplace(s, Arg(args, 0), Arg(args, 1), replaceAll:=True)
                Case "match" : Return JsStringMatch(s, Arg(args, 0))
                Case "matchAll" : Return JsStringMatchAll(s, Arg(args, 0))
                Case "search"
                    Dim searchRe = TryCast(args(0), JsRegExp)
                    If searchRe IsNot Nothing Then Return CDbl(searchRe.Engine.Match(s).Index)
                    Return CDbl(s.IndexOf(JsStr(Arg(args, 0)), StringComparison.Ordinal))
                Case "at"
                    Dim ai = CInt(JsNum(Arg(args, 0)))
                    If ai < 0 Then ai += s.Length
                    If ai < 0 OrElse ai >= s.Length Then Return Undef
                    Return s.Substring(ai, 1)
                Case "lastIndexOf"
                    Return CDbl(s.LastIndexOf(JsStr(Arg(args, 0)), StringComparison.Ordinal))
                Case "padEnd"
                    Dim padLen = CInt(JsNum(Arg(args, 0)))
                    Dim padText = If(args.Length > 1 AndAlso Arg(args, 1) IsNot Undef, JsStr(args(1)), " ")
                    If s.Length >= padLen OrElse padText = "" Then Return s
                    Dim padded As New StringBuilder(s)
                    While padded.Length < padLen
                        padded.Append(padText)
                    End While
                    Return padded.ToString().Substring(0, padLen)
                Case "trimStart" : Return s.TrimStart()
                Case "trimEnd" : Return s.TrimEnd()
                Case "concat"
                    Dim joined As New StringBuilder(s)
                    For Each a In args
                        joined.Append(JsStr(a))
                    Next
                    Return joined.ToString()
                Case "codePointAt"
                    Dim ci = CInt(JsNum(Arg(args, 0)))
                    If ci < 0 OrElse ci >= s.Length Then Return Undef
                    Return CDbl(AscW(s(ci)))
                Case "valueOf" : Return s
                Case "includes" : Return s.Contains(JsStr(Arg(args, 0)))
                Case "startsWith" : Return s.StartsWith(JsStr(Arg(args, 0)), StringComparison.Ordinal)
                Case "endsWith" : Return s.EndsWith(JsStr(Arg(args, 0)), StringComparison.Ordinal)
                Case "repeat"
                    Dim n = CInt(JsNum(Arg(args, 0)))
                    Return String.Concat(Enumerable.Repeat(s, n))
                Case "padStart"
                    Dim n = CInt(JsNum(Arg(args, 0)))
                    Dim pad = If(args.Length > 1, JsStr(args(1)), " ")
                    Dim sb As New StringBuilder()
                    While sb.Length < n - s.Length
                        sb.Append(pad)
                    End While
                    Dim r = sb.ToString()
                    If r.Length > n - s.Length Then r = r.Substring(0, n - s.Length)
                    Return r & s
                Case "toString" : Return s
            End Select
            Throw JsRuntimeException.TypeError("string." & name & " is not a function")
        End Function

        ' ========================================================
        ' T5: generators (eager collection)
        ' ========================================================

        <ThreadStatic>
        Private _yieldStack As Stack(Of JsYieldCollector)

        ''' <summary>
        ''' Record a yielded value into the collector of the innermost running
        ''' generator; the yield expression evaluates to the same value.
        ''' </summary>
        Public Function JsYieldValue(value As Object) As Object
            If _yieldStack IsNot Nothing AndAlso _yieldStack.Count > 0 Then
                Return _yieldStack.Peek().AddValue(value)
            End If
            Throw JsRuntimeException.SyntaxError("'yield' is only valid inside a generator function")
        End Function

        ''' <summary>Finish a generator call: wrap the collected values as the iterator.</summary>
        Public Function JsGeneratorFrom(collector As JsYieldCollector) As Object
            Return New JsGenerator(collector.Values)
        End Function

        ''' <summary>Run a generator body eagerly and return its iterator.</summary>
        Public Function JsRunGenerator(body As Func(Of Object)) As Object
            Dim collector As New JsYieldCollector()
            If _yieldStack Is Nothing Then _yieldStack = New Stack(Of JsYieldCollector)
            _yieldStack.Push(collector)
            Try
                body()
            Finally
                _yieldStack.Pop()
            End Try
            Return New JsGenerator(collector.Values)
        End Function

        ' ========================================================
        ' T4: Promise, async/await and the synchronous microtask queue
        ' ========================================================

        <ThreadStatic>
        Private _microtasks As Queue(Of Action)

        ''' <summary>Queue a microtask (promise reactions run here).</summary>
        Public Sub QueueMicrotask(job As Action)
            If _microtasks Is Nothing Then _microtasks = New Queue(Of Action)
            _microtasks.Enqueue(job)
        End Sub

        ''' <summary>
        ''' Run pending microtasks until the queue is empty. Promise reactions are
        ''' queued when their promise settles, so draining makes <c>then</c> callbacks
        ''' observable — JS runs them as microtasks too, just after the current job.
        ''' </summary>
        Public Sub DrainMicrotasks()
            If _microtasks Is Nothing Then Return
            Dim guard = 0
            While _microtasks.Count > 0 AndAlso guard < 1000000
                Dim job = _microtasks.Dequeue()
                Try
                    job()
                Catch ex As Exception
                    ' a rejected microtask must not abort the remaining queue
                End Try
                guard += 1
            End While
        End Sub

        ''' <summary>
        ''' <c>await</c>: unwrap a promise. Non-promises pass through; a rejected
        ''' promise throws the reason, so it is catchable by try/catch.
        ''' </summary>
        Public Function JsAwait(value As Object) As Object
            Dim p = TryCast(value, JsPromise)
            If p Is Nothing Then Return value

            DrainMicrotasks()
            If p.State = JsPromise.PromiseState.Rejected Then
                Throw New JsRuntimeException(p.Result)
            End If
            Return p.Result
        End Function

        ''' <summary>new Promise(executor) — the executor runs immediately.</summary>
        Private Function ConstructPromise(args As Object()) As Object
            Dim p As New JsPromise()
            Dim executor = Arg(args, 0)
            If Not IsJsFunction(executor) Then
                Throw JsRuntimeException.TypeError("Promise resolver " & JsStr(executor) & " is not a function")
            End If

            Dim resolve As Func(Of Object(), Object) =
                Function(a As Object())
                    p.Fulfill(If(a.Length > 0, a(0), Undef))
                    Return Undef
                End Function
            Dim reject As Func(Of Object(), Object) =
                Function(a As Object())
                    p.Reject(If(a.Length > 0, a(0), Undef))
                    Return Undef
                End Function

            Try
                Call JsInvoke(executor, New Object() {CObj(resolve), CObj(reject)})
            Catch ex As JsRuntimeException
                p.Reject(ex.Payload)
            End Try
            Return p
        End Function

        ''' <summary>Promise.resolve / reject / all / allSettled / race / any.</summary>
        Private Function BuiltinPromiseStatic(name As String, args As Object()) As Object
            Select Case name
                Case "resolve"
                    Dim p As New JsPromise()
                    p.ResolveWith(Arg(args, 0))
                    Return p
                Case "reject"
                    Dim p As New JsPromise()
                    p.Reject(Arg(args, 0))
                    Return p
                Case "all", "allSettled", "race", "any"
                    Dim items = JsIterableValues(Arg(args, 0))
                    DrainMicrotasks()
                    Select Case name
                        Case "race"
                            For Each item In items
                                Dim rp = TryCast(item, JsPromise)
                                If rp Is Nothing Then Return JsPromiseOf(item, True)
                                If rp.State = JsPromise.PromiseState.Fulfilled Then Return JsPromiseOf(rp.Result, True)
                                If rp.State = JsPromise.PromiseState.Rejected Then Return JsPromiseOf(rp.Result, False)
                            Next
                            Return New JsPromise()
                        Case "any"
                            Dim rejections As New List(Of Object)
                            For Each item In items
                                Dim rp = TryCast(item, JsPromise)
                                If rp Is Nothing Then Return JsPromiseOf(item, True)
                                If rp.State = JsPromise.PromiseState.Fulfilled Then Return JsPromiseOf(rp.Result, True)
                                If rp.State = JsPromise.PromiseState.Rejected Then rejections.Add(rp.Result)
                            Next
                            Return JsPromiseOf(rejections, False)
                        Case "all"
                            Dim values As New List(Of Object)
                            For Each item In items
                                Dim rp = TryCast(item, JsPromise)
                                If rp Is Nothing Then
                                    values.Add(item)
                                ElseIf rp.State = JsPromise.PromiseState.Rejected Then
                                    Return JsPromiseOf(rp.Result, False)
                                Else
                                    values.Add(rp.Result)
                                End If
                            Next
                            Return JsPromiseOf(values, True)
                        Case Else ' allSettled
                            Dim settled As New List(Of Object)
                            For Each item In items
                                Dim rp = TryCast(item, JsPromise)
                                Dim entry As New Dictionary(Of String, Object)
                                If rp Is Nothing OrElse rp.State = JsPromise.PromiseState.Fulfilled Then
                                    entry("status") = "fulfilled"
                                    entry("value") = If(rp Is Nothing, item, rp.Result)
                                Else
                                    entry("status") = "rejected"
                                    entry("reason") = rp.Result
                                End If
                                settled.Add(entry)
                            Next
                            Return JsPromiseOf(settled, True)
                    End Select
            End Select
            Throw JsRuntimeException.TypeError("Promise." & name & " is not a function")
        End Function

        ''' <summary>A settled promise carrying <paramref name="value"/>.</summary>
        Public Function JsPromiseOf(value As Object, fulfilled As Boolean) As Object
            Dim p As New JsPromise()
            If fulfilled Then p.ResolveWith(value) Else p.Reject(value)
            Return p
        End Function

        ''' <summary>
        ''' Wrap an <c>async function</c> body: running it yields a promise which is
        ''' fulfilled with the return value (or rejected with a thrown value).
        ''' </summary>
        Public Function JsAsyncWrapper(run As Func(Of Object)) As Object
            Dim p As New JsPromise()
            Try
                p.ResolveWith(run())
            Catch ex As JsRuntimeException
                p.Reject(ex.Payload)
            Catch ex As Exception
                p.Reject(ex.Message)
            End Try
            Return p
        End Function

        ' ========================================================
        ' T3: RegExp / Date / Error interop and library completion
        ' ========================================================

        ''' <summary>
        ''' String.prototype.replace / replaceAll: string or RegExp pattern, string or
        ''' function replacement (<c>$&amp;</c> / <c>$1</c>..<c>$9</c> substitution).
        ''' </summary>
        Private Function JsStringReplace(s As String, pattern As Object, replacement As Object,
                                         replaceAll As Boolean) As String
            Dim fn = If(TypeOf replacement Is Func(Of Object(), Object),
                        DirectCast(replacement, Func(Of Object(), Object)), Nothing)
            Dim re = TryCast(pattern, JsRegExp)

            If re Is Nothing Then
                Dim needle = JsStr(pattern)
                If needle = "" Then Return s
                Dim sb As New StringBuilder()
                Dim pos = 0
                While True
                    Dim idx = s.IndexOf(needle, pos, StringComparison.Ordinal)
                    If idx < 0 Then Exit While
                    sb.Append(s.Substring(pos, idx - pos))
                    sb.Append(If(fn IsNot Nothing, JsStr(JsInvoke(fn, {needle, CDbl(idx), s})), JsStr(replacement)))
                    pos = idx + needle.Length
                    If Not replaceAll Then Exit While
                End While
                sb.Append(s.Substring(pos))
                Return sb.ToString()
            End If

            Dim evaluator As MatchEvaluator =
                Function(m As Match)
                    If fn IsNot Nothing Then
                        Dim callArgs As New List(Of Object) From {m.Value}
                        For i = 1 To m.Groups.Count - 1
                            Dim g = m.Groups(i)
                            callArgs.Add(If(g.Success, CObj(g.Value), Nothing))
                        Next
                        callArgs.Add(CDbl(m.Index))
                        callArgs.Add(s)
                        Return JsStr(JsInvoke(fn, callArgs.ToArray()))
                    End If
                    ' $-substitution is performed by the .NET engine
                    Return m.Result(JsStr(replacement))
                End Function

            Dim count = If(replaceAll OrElse re.IsGlobal, Integer.MaxValue, 1)
            Return re.Engine.Replace(s, evaluator, count)
        End Function

        ''' <summary>String.prototype.match — Nothing, a match array, or all matches for /g.</summary>
        Private Function JsStringMatch(s As String, pattern As Object) As Object
            Dim re = TryCast(pattern, JsRegExp)
            If re Is Nothing Then re = New JsRegExp(JsStr(pattern))
            If re.IsGlobal Then
                Dim all As New List(Of Object)
                For Each m As Match In re.Engine.Matches(s)
                    all.Add(m.Value)
                Next
                Return If(all.Count = 0, CObj(Nothing), all)
            End If
            Dim m0 = re.Engine.Match(s)
            If Not m0.Success Then Return Nothing
            Return MatchToArray(m0)
        End Function

        ''' <summary>String.prototype.matchAll — an array of match arrays (no iterator object).</summary>
        Private Function JsStringMatchAll(s As String, pattern As Object) As Object
            Dim re = TryCast(pattern, JsRegExp)
            If re Is Nothing Then re = New JsRegExp(JsStr(pattern), "g")
            Dim all As New List(Of Object)
            For Each m As Match In re.Engine.Matches(s)
                all.Add(MatchToArray(m))
            Next
            Return all
        End Function

        Private Function BuiltinRegExpMethod(re As JsRegExp, name As String, args As Object()) As Object
            Select Case name
                Case "test"
                    Dim m = re.MatchFromState(JsStr(Arg(args, 0)))
                    Return m IsNot Nothing AndAlso m.Success
                Case "exec"
                    Dim m = re.MatchFromState(JsStr(Arg(args, 0)))
                    If m Is Nothing OrElse Not m.Success Then Return Nothing
                    Return MatchToArray(m)
                Case "toString" : Return re.ToString()
            End Select
            Throw JsRuntimeException.TypeError("RegExp." & name & " is not a function")
        End Function

        ''' <summary>RegExp match → JS result array [match, ...groups].</summary>
        Private Function MatchToArray(m As Match) As Object
            Dim list As New List(Of Object) From {CObj(m.Value)}
            For i = 1 To m.Groups.Count - 1
                Dim g = m.Groups(i)
                list.Add(If(g.Success, CObj(g.Value), Nothing))
            Next
            Return list
        End Function

        Private Function BuiltinErrorCtor(name As String, args As Object()) As Object
            Return New JsError(name, If(args.Length > 0 AndAlso Not (args(0) Is Undef), JsStr(args(0)), ""))
        End Function

        Private Function BuiltinDateStatic(name As String, args As Object()) As Object
            Select Case name
                Case "now" : Return CDbl(JsDate.Now().EpochMs)
                Case "parse"
                    Dim parsed = JsDate.Parse(JsStr(Arg(args, 0)))
                    If parsed Is Nothing Then Return Double.NaN
                    Return CDbl(parsed.EpochMs)
                Case "UTC"
                    Dim d As New Date(CInt(JsNum(Arg(args, 0))), CInt(JsNum(Arg(args, 1))) + 1,
                                      If(args.Length > 2, CInt(JsNum(args(2))), 1),
                                      If(args.Length > 3, CInt(JsNum(args(3))), 0),
                                      If(args.Length > 4, CInt(JsNum(args(4))), 0),
                                      If(args.Length > 5, CInt(JsNum(args(5))), 0), DateTimeKind.Utc)
                    Return CDbl((d - New Date(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds)
            End Select
            Throw JsRuntimeException.TypeError("Date." & name & " is not a function")
        End Function

        Private Function BuiltinDateMethod(d As JsDate, name As String, args As Object()) As Object
            ' calendar accessors read the LOCAL time view, like JavaScript
            Dim v = d.Local
            Select Case name
                Case "getFullYear" : Return CDbl(v.Year)
                Case "getMonth" : Return CDbl(v.Month - 1)
                Case "getDate" : Return CDbl(v.Day)
                Case "getDay" : Return CDbl(CInt(v.DayOfWeek))
                Case "getHours" : Return CDbl(v.Hour)
                Case "getMinutes" : Return CDbl(v.Minute)
                Case "getSeconds" : Return CDbl(v.Second)
                Case "getMilliseconds" : Return CDbl(v.Millisecond)
                Case "getTime", "valueOf" : Return CDbl(d.EpochMs)
                Case "getTimezoneOffset" : Return CDbl(-TimeZoneInfo.Local.GetUtcOffset(v).TotalMinutes)
                Case "toISOString", "toJSON"
                    Return v.ToUniversalTime().ToString("yyyy-MM-dd\THH:mm:ss.fff\Z", CultureInfo.InvariantCulture)
                Case "toString" : Return d.ToString()
                Case "toDateString" : Return v.ToString("ddd MMM dd yyyy")
                Case "toTimeString" : Return v.ToString("HH:mm:ss")
                Case "toLocaleDateString" : Return v.ToString("d")
                Case "toLocaleTimeString" : Return v.ToString("T")
            End Select
            Throw JsRuntimeException.TypeError("Date." & name & " is not a function")
        End Function

        Private Function BuiltinNumberStatic(name As String, args As Object()) As Object
            Select Case name
                Case "isInteger"
                    Dim d = JsNum(Arg(args, 0))
                    Return Not Double.IsNaN(d) AndAlso Not Double.IsInfinity(d) AndAlso d = System.Math.Floor(d)
                Case "isSafeInteger"
                    Dim d = JsNum(Arg(args, 0))
                    Return d = System.Math.Floor(d) AndAlso System.Math.Abs(d) <= 9007199254740991.0
                Case "isFinite"
                    Dim d = JsNum(Arg(args, 0))
                    Return Not Double.IsNaN(d) AndAlso Not Double.IsInfinity(d)
                Case "isNaN"
                    Dim o = Arg(args, 0)
                    Return TypeOf o Is Double AndAlso Double.IsNaN(CDbl(o))
                Case "parseFloat" : Return ParseFloatPrefix(JsStr(Arg(args, 0)))
                Case "parseInt" : Return ParseIntPrefix(JsStr(Arg(args, 0)))
            End Select
            Throw JsRuntimeException.TypeError("Number." & name & " is not a function")
        End Function

        Private Function BuiltinStringStatic(name As String, args As Object()) As Object
            Select Case name
                Case "fromCharCode"
                    Dim sb As New StringBuilder()
                    For Each a In args
                        sb.Append(ChrW(CInt(JsNum(a))))
                    Next
                    Return sb.ToString()
                Case "fromCodePoint"
                    Dim sb As New StringBuilder()
                    For Each a In args
                        sb.Append(ChrW(CInt(JsNum(a))))
                    Next
                    Return sb.ToString()
            End Select
            Throw JsRuntimeException.TypeError("String." & name & " is not a function")
        End Function

        ' ========================================================
        ' T1 syntax support: nullish / bitwise / spread / destructuring /
        ' delete / void / in / instanceof / optional chaining / for-of
        ' ========================================================

        ''' <summary>JS nullish test: null or undefined (the ?? trigger condition).</summary>
        Public Function JsIsNullish(x As Object) As Boolean
            Return x Is Nothing OrElse x Is Undef
        End Function

        Public Function JsIsNullish(v As JsValue) As Boolean
            Return v.IsNull OrElse v.IsUndef
        End Function

        ''' <summary>Short-circuit ?? — evaluates b lazily only when a is nullish.</summary>
        Public Function JsNullish(a As Object, b As Func(Of Object)) As Object
            If a Is Nothing OrElse a Is Undef Then Return b()
            Return a
        End Function

        ' ---------------- bitwise (ECMAScript ToInt32 / ToUint32) ----------------

        ''' <summary>ECMAScript ToInt32: NaN/±Infinity → 0, otherwise modulo 2^32 as signed.</summary>
        Private Function ToInt32Bits(d As Double) As Long
            If Double.IsNaN(d) OrElse Double.IsInfinity(d) Then Return 0
            Dim m = d - System.Math.Floor(d / 4294967296.0) * 4294967296.0      ' [0, 2^32)
            If m >= 2147483648.0 Then Return CLng(m) - 4294967296L
            Return CLng(m)
        End Function

        ''' <summary>ECMAScript ToUint32: NaN/±Infinity → 0, otherwise modulo 2^32.</summary>
        Private Function ToUint32Bits(d As Double) As Long
            If Double.IsNaN(d) OrElse Double.IsInfinity(d) Then Return 0
            Return CLng(d - System.Math.Floor(d / 4294967296.0) * 4294967296.0)
        End Function

        ''' <summary>Wrap a 64-bit shift intermediate back into the signed 32-bit range.</summary>
        Private Function WrapInt32(v As Long) As Double
            If v >= 2147483648.0 Then Return v - 4294967296.0
            If v < -2147483648.0 Then Return v + 4294967296.0
            Return v
        End Function

        ''' <summary>Shift distance: ToUint32(b) And 31 (JS masks the count to 5 bits).</summary>
        Private Function ShiftAmount(b As Object) As Integer
            Return CInt(ToUint32Bits(JsNum(b)) And 31)
        End Function

        Public Function JsBitAnd(a As Object, b As Object) As Object
            Return CDbl(ToInt32Bits(JsNum(a)) And ToInt32Bits(JsNum(b)))
        End Function

        Public Function JsBitOr(a As Object, b As Object) As Object
            Return CDbl(ToInt32Bits(JsNum(a)) Or ToInt32Bits(JsNum(b)))
        End Function

        Public Function JsBitXor(a As Object, b As Object) As Object
            Return CDbl(ToInt32Bits(JsNum(a)) Xor ToInt32Bits(JsNum(b)))
        End Function

        Public Function JsBitNot(a As Object) As Object
            Return CDbl(Not ToInt32Bits(JsNum(a)))
        End Function

        ''' <summary>Signed left shift, the &lt;&lt; operator.</summary>
        Public Function JsShl(a As Object, b As Object) As Object
            Return WrapInt32(ToInt32Bits(JsNum(a)) << ShiftAmount(b))
        End Function

        ''' <summary>Signed right shift, the &gt;&gt; arithmetic operator.</summary>
        Public Function JsShr(a As Object, b As Object) As Object
            Return CDbl(ToInt32Bits(JsNum(a)) >> ShiftAmount(b))
        End Function

        ''' <summary>Unsigned right shift, the &gt;&gt;&gt; logical operator.</summary>
        Public Function JsUshr(a As Object, b As Object) As Object
            Return CDbl(ToUint32Bits(JsNum(a)) >> ShiftAmount(b))
        End Function

        ' JsValue overloads: interpreter hot path, zero boxing
        Public Function JsBitAnd(a As JsValue, b As JsValue) As JsValue
            Return JsValue.Number(CDbl(ToInt32Bits(JsNum(a)) And ToInt32Bits(JsNum(b))))
        End Function

        Public Function JsBitOr(a As JsValue, b As JsValue) As JsValue
            Return JsValue.Number(CDbl(ToInt32Bits(JsNum(a)) Or ToInt32Bits(JsNum(b))))
        End Function

        Public Function JsBitXor(a As JsValue, b As JsValue) As JsValue
            Return JsValue.Number(CDbl(ToInt32Bits(JsNum(a)) Xor ToInt32Bits(JsNum(b))))
        End Function

        Public Function JsBitNot(a As JsValue) As JsValue
            Return JsValue.Number(CDbl(Not ToInt32Bits(JsNum(a))))
        End Function

        Public Function JsShl(a As JsValue, b As JsValue) As JsValue
            Return JsValue.Number(WrapInt32(ToInt32Bits(JsNum(a)) << ShiftAmount(JsBox(b))))
        End Function

        Public Function JsShr(a As JsValue, b As JsValue) As JsValue
            Return JsValue.Number(CDbl(ToInt32Bits(JsNum(a)) >> ShiftAmount(JsBox(b))))
        End Function

        Public Function JsUshr(a As JsValue, b As JsValue) As JsValue
            Return JsValue.Number(CDbl(ToUint32Bits(JsNum(a)) >> ShiftAmount(JsBox(b))))
        End Function

        ' ---------------- optional chaining ----------------

        ''' <summary>Optional member access a?.b — a nullish base short-circuits the chain.</summary>
        Public Function JsChainGet(obj As Object, name As String) As Object
            If obj Is Nothing OrElse obj Is Undef Then Throw New OptionalChainShortCircuit()
            Return JsGet(obj, name)
        End Function

        ''' <summary>Optional index access a?.[i].</summary>
        Public Function JsChainIndex(obj As Object, index As Object) As Object
            If obj Is Nothing OrElse obj Is Undef Then Throw New OptionalChainShortCircuit()
            Return JsIndex(obj, index)
        End Function

        ''' <summary>Optional call a?.(args).</summary>
        Public Function JsChainInvoke(f As Object, args As Object()) As Object
            If f Is Nothing OrElse f Is Undef Then Throw New OptionalChainShortCircuit()
            Return JsInvoke(f, args)
        End Function

        ' ---------------- spread / rest ----------------

        ''' <summary>
        ''' Build a call argument array: <see cref="SpreadMarker"/> entries are
        ''' expanded (arrays → elements, strings → chars).
        ''' </summary>
        Public Function JsBuildArgs(ParamArray parts As Object()) As Object()
            Dim list As New List(Of Object)
            For Each p In parts
                Dim m = TryCast(p, SpreadMarker)
                If m Is Nothing Then
                    list.Add(p)
                Else
                    AppendSpreadValues(list, m.Value)
                End If
            Next
            Return list.ToArray()
        End Function

        ''' <summary>Array literal carrying spread elements.</summary>
        Public Function JsArrayFrom(ParamArray parts As Object()) As Object
            Dim list As New List(Of Object)
            For Each p In parts
                Dim m = TryCast(p, SpreadMarker)
                If m Is Nothing Then
                    list.Add(p)
                Else
                    AppendSpreadValues(list, m.Value)
                End If
            Next
            Return list
        End Function

        Private Sub AppendSpreadValues(list As List(Of Object), value As Object)
            If IsJsArray(value) Then
                list.AddRange(DirectCast(value, List(Of Object)))
            ElseIf TypeOf value Is String Then
                For Each c In CStr(value)
                    list.Add(c.ToString())
                Next
            ElseIf TypeOf value Is JsGenerator Then
                list.AddRange(DirectCast(value, JsGenerator).Remaining())
            ElseIf TypeOf value Is ClrObject Then
                ' ---- CLR reflection bridge: spread an enumerable CLR collection ----
                Dim values As List(Of Object) = Nothing
                If Not ClrInterop.TryIterate(DirectCast(value, ClrObject), values) Then
                    Throw JsRuntimeException.TypeError(JsStr(value) & " is not iterable")
                End If
                list.AddRange(values)
            Else
                Throw JsRuntimeException.TypeError(JsStr(value) & " is not iterable")
            End If
        End Sub

        ''' <summary>Wrap a value as a spread part for JsBuildArgs / JsArrayFrom / JsObjFrom.</summary>
        Public Function JsSpread(value As Object) As Object
            Return New SpreadMarker(value)
        End Function

        ''' <summary>
        ''' Object literal variant of <see cref="JsObj"/> that also accepts
        ''' <see cref="SpreadMarker"/> (object spread) parts.
        ''' </summary>
        Public Function JsObjFrom(ParamArray parts As Object()) As Object
            Dim d As New Dictionary(Of String, Object)
            For Each p In parts
                Dim m = TryCast(p, SpreadMarker)
                If m Is Nothing Then
                    Dim pair = DirectCast(p, (Key As String, Value As Object))
                    Dim acc = TryCast(pair.Value, AccessorProperty)
                    If acc Is Nothing Then
                        d(pair.Key) = pair.Value
                    Else
                        JsAccessorDefine(d, pair.Key, acc.Getter, acc.Setter)
                    End If
                Else
                    JsSpreadInto(d, m.Value)
                End If
            Next
            Return d
        End Function

        ''' <summary>Copy own enumerable properties of src into target (object spread).</summary>
        Public Sub JsSpreadInto(target As Dictionary(Of String, Object), src As Object)
            If IsJsObject(src) Then
                For Each kv In DirectCast(src, Dictionary(Of String, Object))
                    Dim acc = TryCast(kv.Value, AccessorProperty)
                    If acc IsNot Nothing Then
                        ' spread materialises getters as data properties
                        If acc.Getter IsNot Nothing Then
                            target(kv.Key) = JsInvoke(acc.Getter, New Object() {})
                        End If
                    Else
                        target(kv.Key) = kv.Value
                    End If
                Next
            ElseIf IsJsArray(src) Then
                Dim a = DirectCast(src, List(Of Object))
                For i = 0 To a.Count - 1
                    target(i.ToString(CultureInfo.InvariantCulture)) = a(i)
                Next
            ElseIf TypeOf src Is String Then
                Dim s = CStr(src)
                For i = 0 To s.Length - 1
                    target(i.ToString(CultureInfo.InvariantCulture)) = s(i).ToString()
                Next
            ElseIf TypeOf src Is ClrObject Then
                ' ---- CLR reflection bridge: {...host} copies members (indices for collections) ----
                ClrInterop.SpreadInto(target, DirectCast(src, ClrObject))
            ElseIf src Is Nothing OrElse src Is Undef Then
                ' {...null} / {...undefined} are no-ops in JS
            Else
                Throw JsRuntimeException.TypeError("cannot spread " & JsStr(src) & " into an object literal")
            End If
        End Sub

        ''' <summary>Array rest pattern [a, ...rest] — elements from index skip on.</summary>
        Public Function JsArrayRest(arr As Object, skip As Integer) As Object
            Dim list As New List(Of Object)
            Dim src = TryCast(arr, List(Of Object))
            If src IsNot Nothing Then
                For i = System.Math.Max(0, skip) To src.Count - 1
                    list.Add(src(i))
                Next
            End If
            Return list
        End Function

        ''' <summary>Object rest pattern {a, ...rest} — own keys minus the bound ones.</summary>
        Public Function JsObjectRest(obj As Object, ParamArray usedKeys As String()) As Object
            Dim d As New Dictionary(Of String, Object)
            If IsJsObject(obj) Then
                Dim used As New HashSet(Of String)(usedKeys)
                For Each kv In DirectCast(obj, Dictionary(Of String, Object))
                    If Not used.Contains(kv.Key) Then d(kv.Key) = kv.Value
                Next
            End If
            Return d
        End Function

        ''' <summary>Rest parameters (...args) — the tail of the call arguments.</summary>
        Public Function JsRestArgs(args As Object(), skip As Integer) As Object
            Dim list As New List(Of Object)
            For i = System.Math.Max(0, skip) To args.Length - 1
                list.Add(args(i))
            Next
            Return list
        End Function

        ''' <summary>Object-literal accessor value ({get x(){}} / {set x(v){}}).</summary>
        Public Function JsAccessor(getter As Object, setter As Object) As Object
            Return New AccessorProperty(getter, setter)
        End Function

        ' ---------------- delete / void / in / instanceof ----------------

        ''' <summary>delete obj.prop / delete arr[i] — non-strict JS result rules.</summary>
        Public Function JsDelete(obj As Object, key As Object) As Boolean
            If obj Is Nothing OrElse obj Is Undef Then Return True
            ' ---- CLR reflection bridge: bridged members cannot be removed ----
            If TypeOf obj Is ClrObject OrElse TypeOf obj Is ClrType Then
                Throw JsRuntimeException.TypeError(
                    "cannot delete CLR member '" & JsStr(key) & "' of " & JsStr(obj))
            End If
            If IsJsObject(obj) Then
                Return DirectCast(obj, Dictionary(Of String, Object)).Remove(JsStr(key))
            End If
            If IsJsArray(obj) Then
                Dim a = DirectCast(obj, List(Of Object))
                Dim d = JsNum(key)
                If Double.IsNaN(d) Then Return True
                Dim i = CInt(System.Math.Floor(d))
                If i < 0 OrElse i >= a.Count Then Return True
                a(i) = Undef          ' JS leaves a hole; reading it yields undefined
                Return True
            End If
            Return True
        End Function

        ''' <summary>Non-reference delete evaluates its operand for side effects and yields true.</summary>
        Public Function JsDeleteValue(value As Object) As Boolean
            Return True
        End Function

        ''' <summary>void expr — evaluate for side effects, yield undefined.</summary>
        Public Function JsVoid(value As Object) As Object
            Return Undef
        End Function

        ''' <summary>The 'in' operator: property/index presence test.</summary>
        Public Function JsIn(key As Object, container As Object) As Boolean
            If IsJsArray(container) Then
                Dim a = DirectCast(container, List(Of Object))
                Dim d = JsNum(key)
                If Double.IsNaN(d) Then Return False
                Dim i = CInt(System.Math.Floor(d))
                Return i >= 0 AndAlso i < a.Count
            End If
            If TypeOf container Is String Then
                Dim s = CStr(container)
                Dim d = JsNum(key)
                If Double.IsNaN(d) Then Return False
                Dim i = CInt(System.Math.Floor(d))
                Return i >= 0 AndAlso i < s.Length
            End If
            If IsJsObject(container) Then
                Return DirectCast(container, Dictionary(Of String, Object)).ContainsKey(JsStr(key))
            End If
            ' ---- CLR reflection bridge: member presence ----
            Dim clrObj = TryCast(container, ClrObject)
            If clrObj IsNot Nothing Then Return ClrInterop.HasMember(clrObj, JsStr(key))
            Dim clrType = TryCast(container, ClrType)
            If clrType IsNot Nothing Then Return ClrInterop.HasStaticMember(clrType, JsStr(key))

            If container Is Nothing OrElse container Is Undef Then
                Throw JsRuntimeException.TypeError(
                    "Cannot use 'in' operator to search for '" & JsStr(key) & "' in " & JsStr(container))
            End If
            Return False
        End Function

        ''' <summary>instanceof — builtin constructors plus the user-class __proto__ chain.</summary>
        Public Function JsInstanceOf(obj As Object, ctor As Object) As Boolean
            If ctor Is Nothing OrElse ctor Is Undef Then
                Throw JsRuntimeException.TypeError("Right-hand side of 'instanceof' is not callable")
            End If

            ' ---- CLR reflection bridge: x instanceof T tests the CLR type relation ----
            Dim clrType = TryCast(ctor, ClrType)
            If clrType IsNot Nothing Then Return ClrInterop.IsInstanceOf(obj, clrType)

            ' user classes: walk the instance's __proto__ chain for the class prototype
            Dim cls = TryCast(ctor, JsClass)
            If cls IsNot Nothing Then Return JsProtoChainContains(obj, cls.Prototype)

            ' builtin constructors are either bound builtins (String/Number/...) or
            ' container objects (Array/Object/Function)
            Dim ctorName As String = Nothing
            If TypeOf ctor Is BuiltinMethod Then ctorName = DirectCast(ctor, BuiltinMethod).Name
            If TypeOf ctor Is NativeObject Then ctorName = DirectCast(ctor, NativeObject).Name
            If ctorName IsNot Nothing Then
                Select Case ctorName
                    Case "Array" : Return IsJsArray(obj)
                    Case "Object" : Return IsJsObject(obj) OrElse IsJsArray(obj) OrElse TypeOf obj Is ClrObject
                    Case "Function" : Return IsJsFunction(obj)
                    Case "RegExp" : Return TypeOf obj Is JsRegExp
                    Case "Date" : Return TypeOf obj Is JsDate
                    Case "Error" : Return TypeOf obj Is JsError
                    Case "TypeError", "RangeError", "SyntaxError", "ReferenceError"
                        Return TypeOf obj Is JsError AndAlso DirectCast(obj, JsError).Name = ctorName
                End Select
                ' primitives have no wrapper objects here: String/Number/Boolean → false,
                ' matching JS ('abc' instanceof String === false)
                Return False
            End If

            If TypeOf ctor Is Func(Of Object(), Object) Then
                Return JsProtoChainContains(obj, JsGet(ctor, "prototype"))
            End If
            Throw JsRuntimeException.TypeError("Right-hand side of 'instanceof' is not callable")
        End Function

        ''' <summary>True for callable values (closures, builtin methods and classes).</summary>
        Public Function IsJsFunction(x As Object) As Boolean
            ' bridged CLR methods are callable, an exposed type is callable through 'new'
            Return TypeOf x Is Func(Of Object(), Object) OrElse TypeOf x Is BuiltinMethod OrElse TypeOf x Is JsClass OrElse
                   TypeOf x Is ClrBoundMethod OrElse TypeOf x Is ClrType
        End Function

        ''' <summary>Walk the __proto__ chain of obj looking for a prototype object.</summary>
        Public Function JsProtoChainContains(obj As Object, proto As Object) As Boolean
            If proto Is Nothing OrElse proto Is Undef Then Return False
            Dim cur = obj
            Dim guard = 0
            While IsJsObject(cur) AndAlso guard < 1000
                Dim d = DirectCast(cur, Dictionary(Of String, Object))
                Dim p As Object = Nothing
                If Not d.TryGetValue("__proto__", p) Then Return False
                If ReferenceEquals(p, proto) Then Return True
                cur = p
                guard += 1
            End While
            Return False
        End Function

        ''' <summary>
        ''' Values for for-of and spread: arrays (copy), strings (chars) and
        ''' generators (the remaining yielded values).
        ''' </summary>
        Public Function JsIterableValues(iter As Object) As List(Of Object)
            If IsJsArray(iter) Then
                Return New List(Of Object)(DirectCast(iter, List(Of Object)))
            End If
            If TypeOf iter Is String Then
                Dim list As New List(Of Object)
                For Each c In CStr(iter)
                    list.Add(c.ToString())
                Next
                Return list
            End If
            If TypeOf iter Is JsGenerator Then
                Return DirectCast(iter, JsGenerator).Remaining()
            End If
            ' ---- CLR reflection bridge: an enumerable CLR value iterates like an array ----
            If TypeOf iter Is ClrObject Then
                Dim values As List(Of Object) = Nothing
                If ClrInterop.TryIterate(DirectCast(iter, ClrObject), values) Then Return values
            End If
            Throw JsRuntimeException.TypeError(JsStr(iter) & " is not iterable")
        End Function

        ' ========================================================
        ' JsValue 热路径重载：解释器内部求值专用，全程零 box/unbox。
        ' Object 版本保留给宿主边界、内置函数派发与生成代码使用。
        ' ========================================================

        ''' <summary>JsValue → Object（宿主边界装箱：undefined → Undef 标记、null → Nothing）</summary>
        Public Function JsBox(v As JsValue) As Object
            If v.IsUndef Then
                Return Undef
            ElseIf v.IsNull Then
                Return Nothing
            Else
                Return v.AsObject()
            End If
        End Function

        ''' <summary>Object → JsValue（宿主边界拆箱：Undef 标记 → undefined、Nothing → null）</summary>
        Public Function JsUnbox(x As Object) As JsValue
            If x Is Nothing Then
                Return JsValue.Null
            ElseIf x Is Undef Then
                Return JsValue.Undef
            Else
                Return JsValue.OfObject(x)
            End If
        End Function

        Public Function JsTruthy(v As JsValue) As Boolean
            Return v.Truthy()
        End Function

        Public Function JsBool(v As JsValue) As Boolean
            Return v.Truthy()
        End Function

        Public Function JsNum(v As JsValue) As Double
            Select Case v.VarType
                Case TypeCode.Double, TypeCode.Single, TypeCode.Int32, TypeCode.Int64
                    Return v.AsDouble()
                Case TypeCode.Boolean : Return If(v.AsBoolean(), 1.0, 0.0)
                Case TypeCode.String : Return ParseNumberString(v.AsString())
                Case TypeCode.Empty : Return Double.NaN
                Case TypeCode.DBNull : Return 0.0
                Case Else : Return JsNum(v.AsObject())
            End Select
        End Function

        Public Function JsStr(v As JsValue) As String
            Select Case v.VarType
                Case TypeCode.Empty : Return "undefined"
                Case TypeCode.DBNull : Return "null"
                Case TypeCode.Double, TypeCode.Single, TypeCode.Int32, TypeCode.Int64
                    Return NumToString(v.AsDouble())
                Case TypeCode.Boolean : Return If(v.AsBoolean(), "true", "false")
                Case TypeCode.String : Return v.AsString()
                Case Else : Return JsStr(v.AsObject())
            End Select
        End Function

        Public Function JsTypeOf(v As JsValue) As String
            Select Case v.VarType
                Case TypeCode.Double, TypeCode.Single, TypeCode.Int32, TypeCode.Int64 : Return "number"
                Case TypeCode.String : Return "string"
                Case TypeCode.Boolean : Return "boolean"
                Case TypeCode.Empty : Return "undefined"
                Case TypeCode.DBNull : Return "object"      ' JS: typeof null === "object"
                Case Else
                    ' reuse the object overload (handles callables, classes and builtins)
                    Return JsTypeOf(v.AsObject())
            End Select
        End Function

        ' ---------- JsValue 算术 ----------

        Public Function JsAdd(a As JsValue, b As JsValue) As JsValue
            If a.IsString OrElse b.IsString Then
                Return JsValue.Str(JsStr(a) & JsStr(b))
            End If
            If a.IsNumber AndAlso b.IsNumber Then
                Return JsValue.Number(a.AsDouble() + b.AsDouble())
            End If
            Return JsValue.Number(JsNum(a) + JsNum(b))
        End Function

        Public Function JsSub(a As JsValue, b As JsValue) As JsValue
            If a.IsNumber AndAlso b.IsNumber Then
                Return JsValue.Number(a.AsDouble() - b.AsDouble())
            End If
            Return JsValue.Number(JsNum(a) - JsNum(b))
        End Function

        Public Function JsMul(a As JsValue, b As JsValue) As JsValue
            If a.IsNumber AndAlso b.IsNumber Then
                Return JsValue.Number(a.AsDouble() * b.AsDouble())
            End If
            Return JsValue.Number(JsNum(a) * JsNum(b))
        End Function

        Public Function JsDiv(a As JsValue, b As JsValue) As JsValue
            If a.IsNumber AndAlso b.IsNumber Then
                Return JsValue.Number(a.AsDouble() / b.AsDouble())
            End If
            Return JsValue.Number(JsNum(a) / JsNum(b))
        End Function

        Public Function JsMod(a As JsValue, b As JsValue) As JsValue
            Dim x = JsNum(a), y = JsNum(b)
            If Double.IsNaN(x) OrElse Double.IsNaN(y) OrElse y = 0.0 Then
                Return JsValue.Number(Double.NaN)
            End If
            Return JsValue.Number(x - System.Math.Floor(x / y) * y)      ' JS-style floored modulo
        End Function

        ''' <summary>Exponentiation (**). NaN base → NaN, like JS.</summary>
        Public Function JsPow(a As JsValue, b As JsValue) As JsValue
            Dim x = JsNum(a), y = JsNum(b)
            If Double.IsNaN(x) Then Return JsValue.Number(Double.NaN)
            Return JsValue.Number(System.Math.Pow(x, y))
        End Function

        Public Function JsNeg(a As JsValue) As JsValue
            If a.IsNumber Then
                Return JsValue.Number(-a.AsDouble())
            End If
            Return JsValue.Number(-JsNum(a))
        End Function

        Public Function JsNot(a As JsValue) As JsValue
            Return JsValue.Boolean_(Not a.Truthy())
        End Function

        ' ---------- JsValue 比较 ----------

        Public Function JsStrictEq(a As JsValue, b As JsValue) As Boolean
            If a.IsNumber AndAlso b.IsNumber Then
                Dim x = a.AsDouble(), y = b.AsDouble()
                If Double.IsNaN(x) OrElse Double.IsNaN(y) Then Return False
                Return x = y
            End If
            If a.IsString AndAlso b.IsString Then Return a.AsString() = b.AsString()
            If a.IsBoolean AndAlso b.IsBoolean Then Return a.AsBoolean() = b.AsBoolean()
            If a.IsNull AndAlso b.IsNull Then Return True
            If a.IsUndef AndAlso b.IsUndef Then Return True
            If (a.IsObject OrElse a.VarType = TypeCode.DateTime) AndAlso
               (b.IsObject OrElse b.VarType = TypeCode.DateTime) Then
                Return ReferenceEquals(a.AsObject(), b.AsObject())
            End If
            Return False
        End Function

        Public Function JsEq(a As JsValue, b As JsValue) As Boolean
            ' same types → strict
            If (a.IsNumber AndAlso b.IsNumber) OrElse
               (a.IsString AndAlso b.IsString) OrElse
               (a.IsBoolean AndAlso b.IsBoolean) OrElse
               (a.IsObject AndAlso b.IsObject) Then
                Return JsStrictEq(a, b)
            End If
            ' null / undefined family
            Dim aNullish = a.IsNull OrElse a.IsUndef
            Dim bNullish = b.IsNull OrElse b.IsUndef
            If aNullish AndAlso bNullish Then Return True
            If aNullish OrElse bNullish Then Return False
            ' number vs string → numeric
            If a.IsNumber AndAlso b.IsString Then Return JsNum(a) = JsNum(b)
            If a.IsString AndAlso b.IsNumber Then Return JsNum(a) = JsNum(b)
            ' boolean → number
            If a.IsBoolean Then Return JsNum(a) = JsNum(b)
            If b.IsBoolean Then Return JsNum(a) = JsNum(b)
            ' object vs primitive → stringify then compare as strings when primitive is a string
            If b.IsString Then Return JsStr(a) = b.AsString()
            If a.IsString Then Return a.AsString() = JsStr(b)
            Return JsNum(a) = JsNum(b)
        End Function

        Public Function JsNe(a As JsValue, b As JsValue) As Boolean
            Return Not JsEq(a, b)
        End Function

        Public Function JsStrictNe(a As JsValue, b As JsValue) As Boolean
            Return Not JsStrictEq(a, b)
        End Function

        Public Function JsLt(a As JsValue, b As JsValue) As Boolean
            If a.IsString AndAlso b.IsString Then
                Return String.Compare(a.AsString(), b.AsString(), StringComparison.Ordinal) < 0
            End If
            Dim x = JsNum(a), y = JsNum(b)
            If Double.IsNaN(x) OrElse Double.IsNaN(y) Then Return False
            Return x < y
        End Function

        Public Function JsLe(a As JsValue, b As JsValue) As Boolean
            If a.IsString AndAlso b.IsString Then
                Return String.Compare(a.AsString(), b.AsString(), StringComparison.Ordinal) <= 0
            End If
            Dim x = JsNum(a), y = JsNum(b)
            If Double.IsNaN(x) OrElse Double.IsNaN(y) Then Return False
            Return x <= y
        End Function

        Public Function JsGt(a As JsValue, b As JsValue) As Boolean
            If a.IsString AndAlso b.IsString Then
                Return String.Compare(a.AsString(), b.AsString(), StringComparison.Ordinal) > 0
            End If
            Dim x = JsNum(a), y = JsNum(b)
            If Double.IsNaN(x) OrElse Double.IsNaN(y) Then Return False
            Return x > y
        End Function

        Public Function JsGe(a As JsValue, b As JsValue) As Boolean
            If a.IsString AndAlso b.IsString Then
                Return String.Compare(a.AsString(), b.AsString(), StringComparison.Ordinal) >= 0
            End If
            Dim x = JsNum(a), y = JsNum(b)
            If Double.IsNaN(x) OrElse Double.IsNaN(y) Then Return False
            Return x >= y
        End Function

    End Module

End Namespace
