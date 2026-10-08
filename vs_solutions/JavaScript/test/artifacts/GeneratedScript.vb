' Auto-generated from JavaScript by LiteJs — do not edit.
' All dynamic semantics come from LiteJs.Runtime.JsRuntime (shared with the interpreter).
Option Strict On
Option Explicit On
Option Infer On

Imports System
Imports LiteJs.Runtime
Imports JsValue = System.Object
' function values are Func(Of Object(), Object); VB Imports aliases cannot target generic types

Public NotInheritable Class GeneratedScript

    Public Shared Sub Run(io As LiteJs.Runtime.ScriptIO)
        JsRuntime.Io = io

        Dim fib As Func(Of Object(), Object) = Function(args__1 As JsValue())
            Dim n As JsValue = If(args__1.Length > 0, args__1(0), JsRuntime.Undef)
            If JsTruthy(JsRuntime.JsLt(n, 2R)) Then
                Return n
            End If
            Return JsRuntime.JsAdd(JsRuntime.JsInvoke(fib, New JsValue() {JsRuntime.JsSub(n, 1R)}), JsRuntime.JsInvoke(fib, New JsValue() {JsRuntime.JsSub(n, 2R)}))
            Return JsRuntime.Undef

        End Function

        Dim makeCounter As Func(Of Object(), Object) = Function(args__2 As JsValue())
            Dim count As JsValue = 0R
            Return CType(Function(args__3 As JsValue())
            count = JsRuntime.JsAdd(count, 1R)
            Return count
            Return JsRuntime.Undef

            End Function, Func(Of Object(), Object))
            Return JsRuntime.Undef

        End Function

        Dim risky As Func(Of Object(), Object) = Function(args__4 As JsValue())
            Dim x As JsValue = If(args__4.Length > 0, args__4(0), JsRuntime.Undef)
            If JsTruthy(JsRuntime.JsLt(x, 0R)) Then
                Throw New LiteJs.Runtime.JsRuntimeException(JsRuntime.JsAdd("negative: ", x))
            End If
            Return JsRuntime.JsInvoke(JsRuntime.JsGet(JsRuntime.MathBuiltin, "sqrt"), New JsValue() {x})
            Return JsRuntime.Undef

        End Function
        JsRuntime.JsInvoke(JsRuntime.JsGet(JsRuntime.ConsoleBuiltin, "log"), New JsValue() {"fib(20) =", JsRuntime.JsInvoke(fib, New JsValue() {20R})})
        Dim c As JsValue = JsRuntime.JsInvoke(makeCounter, New JsValue() {})
        JsRuntime.JsInvoke(c, New JsValue() {})
        JsRuntime.JsInvoke(c, New JsValue() {})
        JsRuntime.JsInvoke(JsRuntime.JsGet(JsRuntime.ConsoleBuiltin, "log"), New JsValue() {"counter:", JsRuntime.JsInvoke(c, New JsValue() {})})
        Dim total As JsValue = 0R
        Dim i As JsValue = 1R
        Do
            If Not JsTruthy(JsRuntime.JsLe(i, 10R)) Then Exit Do
            If JsTruthy(JsRuntime.JsEq(JsRuntime.JsMod(i, 2R), 0R)) Then
                GoTo __upd1
            End If
            If JsTruthy(JsRuntime.JsGt(i, 7R)) Then
                Exit Do
            End If
            total = JsRuntime.JsAdd(total, JsRuntime.JsMul(i, i))
            __upd1:
            i = JsRuntime.JsAdd(i, 1.0R)
        Loop
        JsRuntime.JsInvoke(JsRuntime.JsGet(JsRuntime.ConsoleBuiltin, "log"), New JsValue() {"loop total =", total})
        Dim n_1 As JsValue = 10R
        While JsTruthy(JsRuntime.JsGt(n_1, 0R))
            n_1 = JsRuntime.JsSub(n_1, 3R)
        End While
        JsRuntime.JsInvoke(JsRuntime.JsGet(JsRuntime.ConsoleBuiltin, "log"), New JsValue() {"while floor(10/3):", JsRuntime.JsAdd(n_1, 3R)})
        Try
            JsRuntime.JsInvoke(JsRuntime.JsGet(JsRuntime.ConsoleBuiltin, "log"), New JsValue() {"sqrt(16) =", JsRuntime.JsInvoke(risky, New JsValue() {16R})})
            JsRuntime.JsInvoke(risky, New JsValue() {JsRuntime.JsNeg(1R)})
            JsRuntime.JsInvoke(JsRuntime.JsGet(JsRuntime.ConsoleBuiltin, "log"), New JsValue() {"unreachable"})
        Catch jsex5 As LiteJs.Runtime.JsRuntimeException
            Dim e As JsValue = jsex5.Payload
            JsRuntime.JsInvoke(JsRuntime.JsGet(JsRuntime.ConsoleBuiltin, "log"), New JsValue() {"caught:", e})
        Finally
            JsRuntime.JsInvoke(JsRuntime.JsGet(JsRuntime.ConsoleBuiltin, "log"), New JsValue() {"finally runs"})
        End Try
        Dim nums As JsValue = JsRuntime.JsArray(5R, 3R, 8R, 1R, 9R, 2R)
        Dim evens As JsValue = JsRuntime.JsInvoke(JsRuntime.JsGet(nums, "filter"), New JsValue() {CType(Function(args__6 As JsValue())
Dim v As JsValue = If(args__6.Length > 0, args__6(0), JsRuntime.Undef)
Return JsRuntime.JsEq(JsRuntime.JsMod(v, 2R), 0R)
Return JsRuntime.Undef

End Function, Func(Of Object(), Object))})
        Dim doubled As JsValue = JsRuntime.JsInvoke(JsRuntime.JsGet(nums, "map"), New JsValue() {CType(Function(args__7 As JsValue())
Dim v_1 As JsValue = If(args__7.Length > 0, args__7(0), JsRuntime.Undef)
Return JsRuntime.JsMul(v_1, 2R)
Return JsRuntime.Undef

End Function, Func(Of Object(), Object))})
        Dim sum As JsValue = JsRuntime.JsInvoke(JsRuntime.JsGet(nums, "reduce"), New JsValue() {CType(Function(args__8 As JsValue())
Dim a As JsValue = If(args__8.Length > 0, args__8(0), JsRuntime.Undef)
Dim b As JsValue = If(args__8.Length > 1, args__8(1), JsRuntime.Undef)
Return JsRuntime.JsAdd(a, b)
Return JsRuntime.Undef

End Function, Func(Of Object(), Object)), 0R})
        JsRuntime.JsInvoke(JsRuntime.JsGet(nums, "sort"), New JsValue() {})
        JsRuntime.JsInvoke(JsRuntime.JsGet(JsRuntime.ConsoleBuiltin, "log"), New JsValue() {"evens:", JsRuntime.JsInvoke(JsRuntime.JsGet(evens, "join"), New JsValue() {","}), "sum:", sum, "max:", JsRuntime.JsInvoke(JsRuntime.JsGet(JsRuntime.MathBuiltin, "max"), New JsValue() {10R, 20R, 30R})})
        JsRuntime.JsInvoke(JsRuntime.JsGet(JsRuntime.ConsoleBuiltin, "log"), New JsValue() {"doubled:", JsRuntime.JsInvoke(JsRuntime.JsGet(doubled, "join"), New JsValue() {" "})})
        Dim gene As JsValue = JsRuntime.JsObj(New (String, Object)() {("name", "TP53"), ("score", 0.87R), ("tags", JsRuntime.JsArray("tumor", "suppressor"))})
        JsRuntime.JsInvoke(JsRuntime.JsGet(JsRuntime.ConsoleBuiltin, "log"), New JsValue() {JsRuntime.JsGet(gene, "name"), JsRuntime.JsGet(gene, "score"), JsRuntime.JsIndex(JsRuntime.JsGet(gene, "tags"), 1R)})
        For Each k As JsValue In JsRuntime.JsForInKeys(gene)
            JsRuntime.JsInvoke(JsRuntime.JsGet(JsRuntime.ConsoleBuiltin, "log"), New JsValue() {JsRuntime.JsAdd(JsRuntime.JsAdd("gene[", k), "] ="), JsRuntime.JsIndex(gene, k)})
        Next
        Dim grade As JsValue = If(JsTruthy(JsRuntime.JsGt(JsRuntime.JsGet(gene, "score"), 0.8R)), "high", "low")
        JsRuntime.JsInvoke(JsRuntime.JsGet(JsRuntime.ConsoleBuiltin, "log"), New JsValue() {"grade:", JsRuntime.JsInvoke(JsRuntime.JsGet(grade, "toUpperCase"), New JsValue() {})})
        JsRuntime.JsInvoke(JsRuntime.JsGet(JsRuntime.ConsoleBuiltin, "log"), New JsValue() {JsRuntime.JsTypeOf(1R), JsRuntime.JsTypeOf("x"), JsRuntime.JsTypeOf(True), JsRuntime.JsTypeOf(JsRuntime.Undef), JsRuntime.JsTypeOf(fib), JsRuntime.JsTypeOf(CType(Nothing, JsValue))})
        Dim counter As JsValue = 5R
        counter = JsRuntime.JsAdd(counter, 1.0R)
        counter = JsRuntime.JsAdd(counter, 1.0R)
        JsRuntime.JsInvoke(JsRuntime.JsGet(JsRuntime.ConsoleBuiltin, "log"), New JsValue() {"counter === 7:", JsRuntime.JsStrictEq(counter, 7R), "== ""7"":", JsRuntime.JsEq(counter, "7")})
        JsRuntime.JsInvoke(JsRuntime.JsGet(JsRuntime.ConsoleBuiltin, "log"), New JsValue() {"pi =", JsRuntime.JsGet(JsRuntime.MathBuiltin, "PI"), "rounded =", JsRuntime.JsInvoke(JsRuntime.JsGet(JsRuntime.MathBuiltin, "round"), New JsValue() {2.718R})})
        JsRuntime.JsInvoke(JsRuntime.JsGet(JsRuntime.ConsoleBuiltin, "log"), New JsValue() {JsRuntime.JsInvoke(JsRuntime.JsGet("  padded ", "trim"), New JsValue() {}), JsRuntime.JsInvoke(JsRuntime.JsGet("abc", "toUpperCase"), New JsValue() {}), JsRuntime.JsGet(JsRuntime.JsInvoke(JsRuntime.JsGet("a,b,c", "split"), New JsValue() {","}), "length")})
    End Sub
End Class
