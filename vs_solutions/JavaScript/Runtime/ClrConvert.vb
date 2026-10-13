Imports System.Globalization
Imports System.Reflection
Imports System.Threading.Tasks

Namespace Runtime

    ''' <summary>
    ''' Value conversion at the CLR bridge boundary.
    ''' <para>
    ''' CLR → JS: numbers (all numeric types), strings, chars, booleans, null,
    ''' enums (numeric value), <see cref="Guid"/> (string),
    ''' <see cref="Date"/>/<see cref="DateTimeOffset"/> (<see cref="JsDate"/>),
    ''' <see cref="Task"/>/<c>Task(Of T)</c> (<see cref="JsPromise"/>) and — for
    ''' everything else — a lazily wrapped <see cref="ClrObject"/> proxy, so nested
    ''' objects stay reachable with plain dot notation.
    ''' </para>
    ''' <para>
    ''' JS → CLR: driven by the target parameter type. Script null/undefined map to
    ''' <c>Nothing</c> for reference and nullable targets (and are rejected for value
    ''' types); arrays and lists convert element-wise.
    ''' </para>
    ''' <para>
    ''' Script functions are NOT converted to delegates (out of scope for the bridge):
    ''' a delegate parameter makes a candidate unusable and the resulting error names
    ''' the limitation instead of silently passing <c>Nothing</c>.
    ''' </para>
    ''' </summary>
    Friend Module ClrConvert

        ' ======================================================== CLR → JS

        ''' <summary>Convert a CLR value into the script value model.</summary>
        Friend Function ToJs(value As Object) As Object
            If value Is Nothing Then Return Nothing
            If value Is DBNull.Value Then Return Nothing
            If value Is JsRuntime.Undef Then Return JsRuntime.Undef
            If ClrInterop.IsClrProxy(value) Then Return value

            Dim t = value.GetType()
            If t Is GetType(Double) OrElse t Is GetType(String) OrElse t Is GetType(Boolean) Then
                Return value                      ' already a JS primitive
            End If
            If IsNumericType(t) Then Return CDbl(value)
            If t Is GetType(Char) Then Return value.ToString()
            If t.IsEnum Then Return CDbl(Convert.ToInt64(value))
            If t Is GetType(Date) Then Return New JsDate(CDate(value))
            If t Is GetType(Guid) Then Return value.ToString()
            If t Is GetType(DateTimeOffset) Then
                Return New JsDate(DirectCast(value, DateTimeOffset).UtcDateTime)
            End If
            If TypeOf value Is Task Then Return ToPromise(DirectCast(value, Task))

            ' values the engine already owns keep their identity (JS arrays/objects/functions)
            If JsRuntime.IsJsArray(value) OrElse JsRuntime.IsJsObject(value) OrElse JsRuntime.IsJsFunction(value) Then
                Return value
            End If
            Return ClrInterop.Wrap(value)
        End Function

        ' ======================================================== JS → CLR

        ''' <summary>
        ''' Convert a script value to <paramref name="target"/>, or throw a script
        ''' TypeError describing why the conversion is impossible.
        ''' </summary>
        Friend Function ToClr(value As Object, target As Type) As Object
            ' a proxy passes the underlying CLR instance through
            Dim proxy = TryCast(value, ClrObject)
            If proxy IsNot Nothing Then
                If target.IsInstanceOfType(proxy.Target) Then Return proxy.Target
                value = proxy.Target
            End If

            If target Is GetType(Object) Then
                If value Is JsRuntime.Undef Then Return Nothing
                Return value
            End If

            Dim underlying = Nullable.GetUnderlyingType(target)
            If underlying IsNot Nothing Then
                If value Is Nothing OrElse value Is JsRuntime.Undef Then Return Nothing
                Return ToClr(value, underlying)
            End If

            If IsDelegate(target) Then Throw DelegateParameterError(target)

            If value Is Nothing OrElse value Is JsRuntime.Undef Then
                If Not target.IsValueType Then Return Nothing
                Throw JsRuntimeException.TypeError(
                    "cannot convert " & JsRuntime.JsStr(value) & " to " & target.Name)
            End If

            If target Is GetType(String) Then Return JsRuntime.JsStr(value)
            If target Is GetType(Boolean) Then Return JsRuntime.JsTruthy(value)
            If target Is GetType(Char) Then
                Dim s = JsRuntime.JsStr(value)
                If s.Length = 0 Then Return ChrW(0)
                Return s(0)
            End If
            If target.IsEnum Then Return ToEnum(value, target)
            If target Is GetType(Guid) Then Return Guid.Parse(JsRuntime.JsStr(value))
            If target Is GetType(Date) Then Return ToDate(value)
            If target Is GetType(DateTimeOffset) Then Return New DateTimeOffset(ToDate(value))
            If IsNumericType(target) Then Return ToNumber(value, target)

            If target.IsArray Then
                If target.GetArrayRank() <> 1 Then
                    Throw JsRuntimeException.TypeError("cannot convert to a multi-dimensional array " & target.Name)
                End If
                Dim elem = target.GetElementType()
                Dim items = EnumerableElements(value)
                Dim arr = Array.CreateInstance(elem, items.Count)
                For i = 0 To items.Count - 1
                    arr.SetValue(ToClr(items(i), elem), i)
                Next
                Return arr
            End If

            If IsGenericList(target) Then
                Dim elem = target.GetGenericArguments()(0)
                Dim list = Activator.CreateInstance(target)
                Dim add = target.GetMethod("Add")
                For Each item In EnumerableElements(value)
                    add.Invoke(list, New Object() {ToClr(item, elem)})
                Next
                Return list
            End If

            Dim vt = value.GetType()
            If target.IsAssignableFrom(vt) Then Return value
            Throw JsRuntimeException.TypeError(
                "cannot convert " & JsRuntime.JsTypeOf(value) & " to " & target.Name)
        End Function

        Private Function ToEnum(value As Object, target As Type) As Object
            Try
                If TypeOf value Is String Then
                    Return [Enum].Parse(target, CStr(value), ignoreCase:=True)
                End If
                Dim underlying = [Enum].GetUnderlyingType(target)
                Dim n = ToNumber(value, underlying)
                Return [Enum].ToObject(target, n)
            Catch ex As Exception
                Throw JsRuntimeException.TypeError(
                    "cannot convert " & JsRuntime.JsStr(value) & " to enum " & target.Name)
            End Try
        End Function

        Private Function ToDate(value As Object) As Date
            Dim d = TryCast(value, JsDate)
            If d IsNot Nothing Then Return d.Value
            If TypeOf value Is Date Then Return CDate(value)
            If IsNumericType(value.GetType()) Then Return JsDate.FromEpochMs(CDbl(value)).Value
            Dim parsed As Date
            If Date.TryParse(JsRuntime.JsStr(value), CultureInfo.InvariantCulture,
                             DateTimeStyles.None, parsed) Then
                Return parsed
            End If
            Throw JsRuntimeException.TypeError(
                "cannot convert " & JsRuntime.JsStr(value) & " to a Date")
        End Function

        Private Function ToNumber(value As Object, target As Type) As Object
            Try
                Return Convert.ChangeType(JsRuntime.JsNum(value), target, CultureInfo.InvariantCulture)
            Catch ex As Exception
                Throw JsRuntimeException.TypeError(
                    "cannot convert " & JsRuntime.JsStr(value) & " to " & target.Name)
            End Try
        End Function

        ''' <summary>
        ''' Cost of converting <paramref name="value"/> to <paramref name="target"/>:
        ''' 0 exact, 1 widening/parsed, 2 stringified, 3 null-family, −1 impossible.
        ''' Used to pick the best overload for a call.
        ''' </summary>
        Friend Function ConvertScore(value As Object, target As Type) As Integer
            Dim proxy = TryCast(value, ClrObject)
            If proxy IsNot Nothing Then
                If target.IsInstanceOfType(proxy.Target) Then Return 0
                value = proxy.Target
            End If

            If target Is GetType(Object) Then Return 0

            Dim underlying = Nullable.GetUnderlyingType(target)
            If underlying IsNot Nothing Then
                If value Is Nothing OrElse value Is JsRuntime.Undef Then Return 3
                Return ConvertScore(value, underlying)
            End If
            If IsDelegate(target) Then Return -1

            If value Is Nothing OrElse value Is JsRuntime.Undef Then
                If Not target.IsValueType Then Return 3
                Return -1
            End If

            Dim vt = value.GetType()
            If vt Is target Then Return 0
            If target Is GetType(String) Then Return 2
            If target Is GetType(Boolean) Then
                If vt Is GetType(Boolean) Then Return 0
                Return If(IsNumericType(vt), 1, 2)
            End If
            If target Is GetType(Char) Then
                If vt Is GetType(String) Then Return 1
                Return -1
            End If
            If target.IsEnum Then
                If vt Is GetType(String) OrElse IsNumericType(vt) Then Return 1
                Return -1
            End If
            If target Is GetType(Guid) Then Return 1
            If target Is GetType(Date) OrElse target Is GetType(DateTimeOffset) Then
                If vt Is GetType(JsDate) Then Return 0
                If vt Is GetType(String) OrElse IsNumericType(vt) Then Return 1
                Return -1
            End If
            If IsNumericType(target) Then
                If IsNumericType(vt) OrElse vt Is GetType(Boolean) OrElse vt Is GetType(String) Then Return 1
                Return -1
            End If
            If target.IsArray OrElse IsGenericList(target) Then
                If TypeOf value Is List(Of Object) OrElse TypeOf value Is Array OrElse TypeOf value Is ClrObject Then
                    Return 1
                End If
                Return -1
            End If
            If target.IsAssignableFrom(vt) Then Return 0
            Return -1
        End Function

        ''' <summary>The CLR default for an unsupplied parameter of a non-optional parameter slot.</summary>
        Friend Function DefaultValue(target As Type) As Object
            If target.IsValueType Then Return Activator.CreateInstance(target)
            Return Nothing
        End Function

        ' ======================================================== error boundaries

        ''' <summary>
        ''' Normalise an exception escaping a reflection call into a script error.
        ''' <see cref="TargetInvocationException"/> is unwrapped, so the script sees the
        ''' exception the member actually raised; the original instance is kept for the
        ''' host (<see cref="ClrInterop.ToHostException"/>).
        ''' </summary>
        Friend Function NormalizeFailure(ex As Exception) As JsRuntimeException
            Dim inner = ex
            If TypeOf ex Is TargetInvocationException AndAlso ex.InnerException IsNot Nothing Then
                inner = ex.InnerException
            End If
            Dim already = TryCast(inner, JsRuntimeException)
            If already IsNot Nothing Then Return already
            Return New JsRuntimeException(ErrorPayload(inner))
        End Function

        ''' <summary>Script error object carrying a CLR exception (name = CLR type name).</summary>
        Friend Function ErrorPayload(ex As Exception) As JsError
            If ex Is Nothing Then Return New JsError("Error", "")
            Return New JsError(ex.GetType().Name, ex.Message, ex)
        End Function

        ''' <summary>Readable "no overload matches" failure, listing the candidates.</summary>
        Friend Function NoOverload(name As String, candidates As List(Of MethodBase), args As Object()) As JsRuntimeException
            Dim sb As New System.Text.StringBuilder()
            sb.Append("no overload of '").Append(name).Append("' matches ").
               Append(args.Length.ToString(CultureInfo.InvariantCulture)).Append(" argument(s)")
            For Each m In candidates
                sb.Append(vbLf).Append("  ").Append(Signature(m))
                If HasDelegateParameter(m) Then
                    sb.Append(" [delegate parameters are not supported by the CLR bridge]")
                End If
            Next
            Return JsRuntimeException.TypeError(sb.ToString())
        End Function

        Friend Function HasDelegateParameter(m As MethodBase) As Boolean
            For Each p In m.GetParameters()
                If IsDelegate(p.ParameterType) Then Return True
            Next
            Return False
        End Function

        Friend Function Signature(m As MethodBase) As String
            Dim ps = m.GetParameters()
            Dim parts(ps.Length - 1) As String
            For i = 0 To ps.Length - 1
                parts(i) = ps(i).ParameterType.Name
            Next
            Dim name = m.Name
            If TypeOf m Is ConstructorInfo Then name = "new " & m.DeclaringType.Name
            Return name & "(" & String.Join(", ", parts) & ")"
        End Function

        Private Function DelegateParameterError(target As Type) As JsRuntimeException
            Return JsRuntimeException.TypeError(
                "delegate parameters are not supported by the CLR bridge (parameter type " &
                target.Name & "); pass a .NET delegate from the host instead of a script function")
        End Function

        ' ======================================================== Task → Promise

        ''' <summary>
        ''' <c>Task</c> / <c>Task(Of T)</c> → <see cref="JsPromise"/>. The engine has no
        ''' event loop, so an already settled task settles the promise immediately; an
        ''' unfinished task is waited for up to <see cref="ClrInterop.TaskWaitTimeoutMs"/>
        ''' and otherwise reported as an error instead of hanging.
        ''' </summary>
        Friend Function ToPromise(task As Task) As Object
            Dim p As New JsPromise()
            Dim completed As Boolean
            Try
                completed = task.Wait(ClrInterop.TaskWaitTimeoutMs)
            Catch ex As AggregateException
                completed = True                     ' a faulted task counts as settled
            Catch ex As Exception
                completed = True
            End Try

            If Not completed Then
                Throw New JsRuntimeException(New JsError("Error",
                    "Task did not complete within " & ClrInterop.TaskWaitTimeoutMs.ToString(CultureInfo.InvariantCulture) &
                    "ms; this engine has no event loop, so only synchronously completable tasks are supported"))
            End If

            If task.IsCanceled Then
                p.Reject(New JsError("Error", "Task was canceled"))
            ElseIf task.IsFaulted Then
                p.Reject(ErrorPayload(UnwrapAggregate(task.Exception)))
            Else
                p.Fulfill(TaskResult(task))
            End If
            Return p
        End Function

        ''' <summary>The result of a completed <c>Task(Of T)</c>, or undefined for a plain Task.</summary>
        Private Function TaskResult(task As Task) As Object
            Dim t = task.GetType()
            While t IsNot Nothing
                If t.IsGenericType AndAlso t.GetGenericTypeDefinition().FullName = "System.Threading.Tasks.Task`1" Then
                    Dim p = t.GetProperty("Result")
                    If p IsNot Nothing Then Return ToJs(p.GetValue(task))
                    Exit While
                End If
                t = t.BaseType
            End While
            Return JsRuntime.Undef
        End Function

        Friend Function UnwrapAggregate(ex As AggregateException) As Exception
            If ex Is Nothing Then Return Nothing
            Dim flat = ex.Flatten()
            If flat.InnerExceptions.Count > 0 Then Return flat.InnerExceptions(0)
            Return ex
        End Function

        ' ======================================================== type helpers

        Friend Function IsDelegate(t As Type) As Boolean
            Return GetType(System.Delegate).IsAssignableFrom(t)
        End Function

        Friend Function IsNumericType(t As Type) As Boolean
            Select Case Type.GetTypeCode(t)
                Case TypeCode.Byte, TypeCode.SByte, TypeCode.Int16, TypeCode.UInt16,
                     TypeCode.Int32, TypeCode.UInt32, TypeCode.Int64, TypeCode.UInt64,
                     TypeCode.Single, TypeCode.Double, TypeCode.Decimal
                    Return True
            End Select
            Return False
        End Function

        Friend Function IsGenericList(t As Type) As Boolean
            Return t.IsGenericType AndAlso t.GetGenericTypeDefinition().FullName = "System.Collections.Generic.List`1"
        End Function

        ''' <summary>Element-wise view of a script array or an enumerable CLR value.</summary>
        Friend Function EnumerableElements(value As Object) As List(Of Object)
            Dim proxy = TryCast(value, ClrObject)
            If proxy IsNot Nothing Then value = proxy.Target

            Dim jsArray = TryCast(value, List(Of Object))
            If jsArray IsNot Nothing Then Return jsArray

            Dim result As New List(Of Object)
            If value IsNot Nothing Then
                Dim arr = TryCast(value, Array)
                If arr IsNot Nothing Then
                    For Each el In arr
                        result.Add(el)
                    Next
                    Return result
                End If
                Dim seq = TryCast(value, System.Collections.IEnumerable)
                If seq IsNot Nothing Then
                    For Each el In seq
                        result.Add(el)
                    Next
                End If
            End If
            Return result
        End Function

    End Module

End Namespace
