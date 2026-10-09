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

        Public Property Io As ScriptIO = New LogTextIO()

        Public ReadOnly ConsoleBuiltin As New NativeObject("console")
        Public ReadOnly MathBuiltin As New NativeObject("Math")
        Public ReadOnly Json As New NativeObject("JSON")
        Public ReadOnly ObjectBuiltin As New NativeObject("Object")
        Public ReadOnly ArrayBuiltin As New NativeObject("Array")
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
                {"Object", ObjectBuiltin}, {"Array", ArrayBuiltin},
                {"undefined", Undef}, {"NaN", Double.NaN}, {"Infinity", Double.PositiveInfinity}
            }
            For Each name In {"parseInt", "parseFloat", "isNaN", "isFinite",
                              "String", "Number", "Boolean"}
                g(name) = New BuiltinMethod(GlobalObj, name)
            Next
            Return g
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
            Return JsStr(x)
        End Function

        Public Function JsTypeOf(x As Object) As String
            If TypeOf x Is Double Then Return "number"
            If TypeOf x Is String Then Return "string"
            If TypeOf x Is Boolean Then Return "boolean"
            If TypeOf x Is Func(Of Object(), Object) OrElse TypeOf x Is BuiltinMethod Then Return "function"
            If x Is Nothing OrElse x Is Undef Then Return "undefined"
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
               TypeOf a Is BuiltinMethod OrElse TypeOf b Is BuiltinMethod Then
                Return ReferenceEquals(a, b)
            End If
            Return False
        End Function

        Public Function JsEq(a As Object, b As Object) As Boolean
            ' same types → strict
            If (IsNumber(a) AndAlso IsNumber(b)) OrElse
               (TypeOf a Is String AndAlso TypeOf b Is String) OrElse
               (TypeOf a Is Boolean AndAlso TypeOf b Is Boolean) OrElse
               (IsJsArray(a) OrElse IsJsObject(a) OrElse TypeOf a Is Func(Of Object(), Object) OrElse TypeOf a Is BuiltinMethod) AndAlso
               (IsJsArray(b) OrElse IsJsObject(b) OrElse TypeOf b Is Func(Of Object(), Object) OrElse TypeOf b Is BuiltinMethod) Then
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
                If d.TryGetValue(name, v) Then Return v
                Return Undef
            End If
            If TypeOf obj Is NativeObject Then
                Dim n = DirectCast(obj, NativeObject)
                If n.Name = "Math" AndAlso _mathProps.TryGetValue(name, Nothing) Then Return _mathProps(name)
                If n.Name = "Math" Then Return New BuiltinMethod(obj, name)
                Return New BuiltinMethod(obj, name)
            End If
            If IsNumber(obj) AndAlso name = "toFixed" Then Return New BuiltinMethod(obj, name)
            Return Undef
        End Function

        Public Function JsIndex(obj As Object, index As Object) As Object
            If obj Is Nothing OrElse obj Is Undef Then
                Throw JsRuntimeException.TypeError("cannot index " & JsStr(obj))
            End If
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
            If IsJsObject(obj) Then
                DirectCast(obj, Dictionary(Of String, Object))(name) = value
            End If
            ' other targets: silently ignored (non-strict JS)
        End Sub

        Public Sub JsSetIndex(obj As Object, index As Object, value As Object)
            If obj Is Nothing OrElse obj Is Undef Then
                Throw JsRuntimeException.TypeError("cannot index " & JsStr(obj))
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

        Public Function JsInvoke(f As Object, args As Object()) As Object
            If TypeOf f Is Func(Of Object(), Object) Then
                Return DirectCast(f, Func(Of Object(), Object)).Invoke(args)
            End If
            If TypeOf f Is BuiltinMethod Then
                Dim m = DirectCast(f, BuiltinMethod)
                Return DispatchBuiltin(m.Target, m.Name, args)
            End If
            Throw JsRuntimeException.TypeError(JsStr(f) & " is not a function")
        End Function

        ' ================= construction helpers (used by generated code) =================

        Public Function JsArray(ParamArray elements As Object()) As Object
            Return New List(Of Object)(elements)
        End Function

        ''' <summary>Object literal from (key, value) tuples (VB tuple literals convert implicitly).</summary>
        Public Function JsObj(pairs As (Key As String, Value As Object)()) As Object
            Dim d As New Dictionary(Of String, Object)
            For Each p In pairs
                d(p.Key) = p.Value
            Next
            Return d
        End Function

        ''' <summary>Keys for for-in: array indices (as strings) or object keys.</summary>
        Public Function JsForInKeys(obj As Object) As Object()
            If IsJsArray(obj) Then
                Dim a = DirectCast(obj, List(Of Object))
                Dim keys(a.Count - 1) As Object
                For i = 0 To a.Count - 1
                    keys(i) = i.ToString(CultureInfo.InvariantCulture)
                Next
                Return keys
            End If
            If IsJsObject(obj) Then
                Return New List(Of Object)(DirectCast(obj, Dictionary(Of String, Object)).Keys.Cast(Of Object)()).ToArray()
            End If
            Return New Object() {}
        End Function

        ' ================= builtin dispatch =================

        Private ReadOnly ArrayMethodNames As New HashSet(Of String) From {
            "push", "pop", "shift", "unshift", "join", "concat", "slice", "indexOf",
            "includes", "reverse", "sort", "map", "filter", "reduce", "forEach"
        }

        Private ReadOnly StringMethodNames As New HashSet(Of String) From {
            "charAt", "charCodeAt", "substring", "slice", "indexOf", "toUpperCase",
            "toLowerCase", "trim", "split", "replace", "includes", "startsWith",
            "endsWith", "repeat", "padStart", "toString"
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
                    Case "global" : Return BuiltinGlobalFn(name, args)
                End Select
            End If
            If IsJsArray(target) Then Return BuiltinArrayMethod(DirectCast(target, List(Of Object)), name, args)
            If TypeOf target Is String Then Return BuiltinStringMethod(CStr(target), name, args)
            If IsNumber(target) AndAlso name = "toFixed" Then
                Dim digits = CInt(JsNum(Arg(args, 0)))
                Return CDbl(target).ToString("F" & digits.ToString(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture)
            End If
            Throw JsRuntimeException.TypeError($"{JsStr(target)}.{name} is not a function")
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
                Case "pow" : Return System.Math.Pow(JsNum(Arg(args, 0)), JsNum(Arg(args, 1)))
                Case "sign"
                    Dim d = JsNum(Arg(args, 0))
                    Return If(Double.IsNaN(d) OrElse d = 0.0, If(d = 0.0, 0.0, Double.NaN), System.Math.Sign(d))
                Case "min"
                    Dim r = Double.PositiveInfinity
                    For Each a In args
                        Dim v = JsNum(a)
                        If v < r Then r = v
                    Next
                    Return r
                Case "max"
                    Dim r = Double.NegativeInfinity
                    For Each a In args
                        Dim v = JsNum(a)
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
            End Select
            Throw JsRuntimeException.TypeError("System.Math." & name & " is not a function")
        End Function

        Private Function BuiltinJson(name As String, args As Object()) As Object
            Select Case name
                Case "stringify" : Return JsonStringify(Arg(args, 0))
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
        Private Function DisplayJson(x As Object) As String
            ' TrimEnd: the json writer terminates the document with a newline
            Return ToJsonElement(x, New HashSet(Of Object)).BuildJsonString(JsonOpts()).TrimEnd()
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
                    obj.Add(kv.Key, ToJsonElement(v, ancestors))
                Next
                Call ancestors.Remove(x)
                Return obj
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
        Private Function JsonStringify(x As Object) As Object
            If x Is Nothing Then
                Return "null"
            ElseIf x Is Undef OrElse TypeOf x Is Func(Of Object(), Object) OrElse TypeOf x Is BuiltinMethod Then
                Return Undef
            Else
                Return DisplayJson(x)
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
                        Return New List(Of Object)(DirectCast(o, Dictionary(Of String, Object)).Values)
                    End If
                    Return New List(Of Object)()
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
            End Select
            Throw JsRuntimeException.TypeError("array." & name & " is not a function")
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
                    Dim sep = JsStr(args(0))
                    If sep = "" Then
                        Dim chars As New List(Of Object)
                        For Each c In s
                            chars.Add(c.ToString())
                        Next
                        Return chars
                    End If
                    Return New List(Of Object)(s.Split({sep}, StringSplitOptions.None).Cast(Of Object)())
                Case "replace"
                    Return s.Replace(JsStr(Arg(args, 0)), JsStr(Arg(args, 1)))
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
                Case TypeCode.Empty, TypeCode.DBNull : Return "undefined"
                Case Else
                    Dim o = v.AsObject()
                    If TypeOf o Is Func(Of Object(), Object) OrElse TypeOf o Is BuiltinMethod Then Return "function"
                    Return "object"
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
