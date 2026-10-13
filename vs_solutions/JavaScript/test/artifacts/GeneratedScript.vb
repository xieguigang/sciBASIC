#Region "Microsoft.VisualBasic::dd053a3bc996281352821fb1d38efe9f, vs_solutions\JavaScript\test\artifacts\GeneratedScript.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 284
    '    Code Lines: 257 (90.49%)
    ' Comment Lines: 3 (1.06%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 24 (8.45%)
    '     File Size: 17.36 KB


    ' Class GeneratedScript
    ' 
    '     Sub: Run
    ' 
    ' /********************************************************************************/

#End Region

' Auto-generated from JavaScript by LiteJs — do not edit.
' All dynamic semantics come from the LiteJs runtime (shared with the interpreter).
Option Strict On
Option Explicit On
Option Infer On

Imports System
Imports Microsoft.VisualBasic.ApplicationServices.VM.JavaScript.Runtime
Imports LiteJsRuntime = Microsoft.VisualBasic.ApplicationServices.VM.JavaScript.Runtime
Imports JsValue = System.Object
' function values are Func(Of Object(), Object); VB Imports aliases cannot target generic types

Public NotInheritable Class GeneratedScript

    Public Shared Sub Run(io As LiteJsRuntime.ScriptIO, Optional host As LiteJsRuntime.ClrHost = Nothing)
        JsRuntime.Io = io
        If host IsNot Nothing Then host.Install()
        Dim total As JsValue = JsRuntime.Undef
        Dim leaked As JsValue = JsRuntime.Undef
        Dim counter As JsValue = JsRuntime.Undef
        Dim bits As JsValue = JsRuntime.Undef
        Dim fallback As JsValue = JsRuntime.Undef
        Dim name As JsValue = JsRuntime.Undef
        Dim xs As JsValue = JsRuntime.Undef
        Dim base As JsValue = JsRuntime.Undef
        Dim merged As JsValue = JsRuntime.Undef
        Dim a As JsValue = JsRuntime.Undef
        Dim rest As JsValue = JsRuntime.Undef
        Dim first As JsValue = JsRuntime.Undef
        Dim others As JsValue = JsRuntime.Undef
        Dim withAccessor As JsValue = JsRuntime.Undef
        Dim grade As JsValue = JsRuntime.Undef
        Dim walked As JsValue = JsRuntime.Undef
        Dim i As JsValue = JsRuntime.Undef
        Dim [date] As JsValue = JsRuntime.Undef
        Dim sq As JsValue = JsRuntime.Undef
        Dim maybe As JsValue = JsRuntime.Undef
        Dim clrSum As JsValue = JsRuntime.Undef

        Dim makeCounter As Func(Of Object(), Object) = Function(args__1 As JsValue())
            Dim this As JsValue = JsRuntime.CurrentThis
            Dim arguments As JsValue = New List(Of Object)(args__1)
            Dim n As JsValue = JsRuntime.Undef
            n = 0R
            Return CType(Function(args__2 As JsValue())
            Dim this_1 As JsValue = JsRuntime.CurrentThis
            Dim arguments_2 As JsValue = New List(Of Object)(args__2)
            n = JsRuntime.JsAdd(n, 1R)
            Return n
            Return JsRuntime.Undef

            End Function, Func(Of Object(), Object))
            Return JsRuntime.Undef

        End Function

        Dim describe As Func(Of Object(), Object) = Function(args__3 As JsValue())
            Dim label As JsValue = If(args__3.Length > 0, args__3(0), JsRuntime.Undef)
            Dim count As JsValue = If(args__3.Length > 1 AndAlso args__3(1) IsNot JsRuntime.Undef, args__3(1), 1R)
            Dim extra As JsValue = JsRuntime.JsRestArgs(args__3, 2)
            Dim this_1 As JsValue = JsRuntime.CurrentThis
            Dim arguments_2 As JsValue = New List(Of Object)(args__3)
            Return JsRuntime.JsAdd(JsRuntime.JsAdd(label, count), JsRuntime.JsGet(extra, "length"))
            Return JsRuntime.Undef

        End Function

        Dim naturals As Func(Of Object(), Object) = Function(args__4 As JsValue())
            Dim gen5 As New LiteJsRuntime.JsYieldCollector()
            Dim n_1 As JsValue = If(args__4.Length > 0, args__4(0), JsRuntime.Undef)
            Dim this_2 As JsValue = JsRuntime.CurrentThis
            Dim arguments_3 As JsValue = New List(Of Object)(args__4)
            Dim k As JsValue = JsRuntime.Undef
            k = 1R
            Do
                If Not JsTruthy(JsRuntime.JsLe(k, n_1)) Then Exit Do
                gen5.AddValue(k)
                __upd1:
                k = JsRuntime.JsAdd(k, 1.0R)
            Loop
            Return JsRuntime.JsGeneratorFrom(gen5)

        End Function

        Dim fetchValue As Func(Of Object(), Object) = Function(args__6 As JsValue())
            Dim promise7 As New LiteJsRuntime.JsPromise()
            Try
                Dim this_3 As JsValue = JsRuntime.CurrentThis
                Dim arguments_4 As JsValue = New List(Of Object)(args__6)
                promise7.Fulfill(42R)
                Return promise7
                promise7.Fulfill(JsRuntime.Undef)
                Return promise7
            Catch ax8 As LiteJsRuntime.JsRuntimeException
                promise7.Reject(ax8.Payload)
                Return promise7
            End Try

        End Function

        Dim main As Func(Of Object(), Object) = Function(args__9 As JsValue())
            Dim promise10 As New LiteJsRuntime.JsPromise()
            Try
                Dim this_4 As JsValue = JsRuntime.CurrentThis
                Dim arguments_5 As JsValue = New List(Of Object)(args__9)
                Dim value As JsValue = JsRuntime.Undef
                value = JsRuntime.JsAwait(JsRuntime.JsInvoke(fetchValue, New JsValue() {}))
                JsRuntime.JsInvokeMethod(JsRuntime.ConsoleBuiltin, "log", New JsValue() {"awaited:", value}, False, False)
                promise10.Fulfill(JsRuntime.Undef)
                Return promise10
            Catch ax11 As LiteJsRuntime.JsRuntimeException
                promise10.Reject(ax11.Payload)
                Return promise10
            End Try

        End Function
        total = 0R
        If JsTruthy(True) Then
            leaked = 7R
        End If
        counter = JsRuntime.JsInvoke(makeCounter, New JsValue() {})
        JsRuntime.JsInvokeMethod(JsRuntime.ConsoleBuiltin, "log", New JsValue() {"counter:", JsRuntime.JsInvoke(counter, New JsValue() {}), JsRuntime.JsInvoke(counter, New JsValue() {}), "leaked:", leaked}, False, False)
        bits = JsRuntime.JsBitOr(JsRuntime.JsBitAnd(5R, 3R), JsRuntime.JsShl(1R, 4R))
        fallback = JsRuntime.JsNullish(CType(Nothing, JsValue), Function() "default")
        JsRuntime.JsInvokeMethod(JsRuntime.ConsoleBuiltin, "log", New JsValue() {"bits:", bits, "fallback:", fallback, JsRuntime.JsStrictEq(JsRuntime.JsMod(7R, 3R), 1R)}, False, False)
        name = "world"
        JsRuntime.JsInvokeMethod(JsRuntime.ConsoleBuiltin, "log", New JsValue() {"hello " & JsRuntime.JsStr(name) & ", total=" & JsRuntime.JsStr(JsRuntime.JsAdd(total, 1R)) & ""}, False, False)
        xs = JsRuntime.JsArray(3R, 1R, 2R)
        JsRuntime.JsInvokeMethod(xs, "sort", New JsValue() {}, False, False)
        JsRuntime.JsInvokeMethod(JsRuntime.ConsoleBuiltin, "log", New JsValue() {"joined:", JsRuntime.JsInvokeMethod(xs, "join", New JsValue() {","}, False, False), "sum:", JsRuntime.JsInvokeMethod(xs, "reduce", New JsValue() {CType(Function(args__12 As JsValue())
Dim a_1 As JsValue = If(args__12.Length > 0, args__12(0), JsRuntime.Undef)
Dim b As JsValue = If(args__12.Length > 1, args__12(1), JsRuntime.Undef)
Return JsRuntime.JsAdd(a_1, b)
Return JsRuntime.Undef

End Function, Func(Of Object(), Object)), 0R}, False, False)}, False, False)
        JsRuntime.JsInvokeMethod(JsRuntime.ConsoleBuiltin, "log", New JsValue() {"found:", JsRuntime.JsInvokeMethod(xs, "find", New JsValue() {CType(Function(args__13 As JsValue())
Dim x As JsValue = If(args__13.Length > 0, args__13(0), JsRuntime.Undef)
Return JsRuntime.JsGt(x, 1R)
Return JsRuntime.Undef

End Function, Func(Of Object(), Object))}, False, False), "flat:", JsRuntime.JsGet(JsRuntime.JsInvokeMethod(JsRuntime.JsArray(JsRuntime.JsArray(1R, 2R), JsRuntime.JsArray(3R)), "flat", New JsValue() {}, False, False), "length")}, False, False)
        base = JsRuntime.JsObj(New (String, Object)() {("a", 1R), ("b", 2R)})
        merged = JsRuntime.JsObjFrom(JsRuntime.JsSpread(base), ("c", 3R), (JsRuntime.JsStr(name), 4R))
        Dim obj14 As JsValue = merged
        a = JsRuntime.JsGet(obj14, "a")
        rest = JsRuntime.JsObjectRest(obj14, "a")
        Dim arr15 As JsValue = xs
        first = JsRuntime.JsIndex(arr15, 0.0R)
        others = JsRuntime.JsArrayRest(arr15, 1)
        withAccessor = JsRuntime.JsObjFrom(("stored", 0R), ("value", JsRuntime.JsAccessor(CType(Function(args__16 As JsValue())
Dim this_5 As JsValue = JsRuntime.CurrentThis
Dim arguments_6 As JsValue = New List(Of Object)(args__16)
Return JsRuntime.JsGet(this_5, "stored")
Return JsRuntime.Undef

End Function, Func(Of Object(), Object)), Nothing)), ("value", JsRuntime.JsAccessor(Nothing, CType(Function(args__17 As JsValue())
Dim v As JsValue = If(args__17.Length > 0, args__17(0), JsRuntime.Undef)
Dim this_6 As JsValue = JsRuntime.CurrentThis
Dim arguments_7 As JsValue = New List(Of Object)(args__17)
JsRuntime.JsSet(this_6, "stored", v)
Return JsRuntime.Undef

End Function, Func(Of Object(), Object)))))
        JsRuntime.JsSet(withAccessor, "value", JsRuntime.JsAdd(a, first))
        JsRuntime.JsInvokeMethod(JsRuntime.ConsoleBuiltin, "log", New JsValue() {"a:", a, "rest keys:", JsRuntime.JsGet(JsRuntime.JsInvokeMethod(JsRuntime.ObjectBuiltin, "keys", New JsValue() {rest}, False, False), "length"), "others:", JsRuntime.JsGet(others, "length")}, False, False)
        JsRuntime.JsInvokeMethod(JsRuntime.ConsoleBuiltin, "log", New JsValue() {"accessor:", JsRuntime.JsGet(withAccessor, "value"), "computed key:", JsRuntime.JsIndex(merged, name)}, False, False)
        JsRuntime.JsInvokeMethod(JsRuntime.ConsoleBuiltin, "log", New JsValue() {"describe:", JsRuntime.JsInvoke(describe, New JsValue() {"n", CType(Nothing, JsValue), "x", "y"})}, False, False)
        JsRuntime.JsInvokeMethod(JsRuntime.ConsoleBuiltin, "log", New JsValue() {"spread call:", JsRuntime.JsInvoke(describe, JsRuntime.JsBuildArgs(JsRuntime.JsSpread(JsRuntime.JsArray("z", 2R))))}, False, False)
        grade = ""
        Dim sw18 As JsValue = JsRuntime.JsGet(xs, "length")
        If JsRuntime.JsStrictEq(sw18, 2R) Then GoTo __case2_0
        If JsRuntime.JsStrictEq(sw18, 3R) Then GoTo __case2_1
        GoTo __case2_2
        __case2_0:
            grade = JsRuntime.JsAdd(grade, "two")
            GoTo __case2_1
        __case2_1:
            grade = JsRuntime.JsAdd(grade, "three")
            GoTo __swend2
            GoTo __case2_2
        __case2_2:
            grade = JsRuntime.JsAdd(grade, "other")
        __swend2:
        walked = ""
        i = 0R
        Do
            If Not JsTruthy(JsRuntime.JsLt(i, 3R)) Then GoTo __lexit3
            For Each ch As JsValue In JsRuntime.JsIterableValues("ab")
                If JsTruthy(JsRuntime.JsStrictEq(ch, "b")) Then
                    GoTo __lcont3
                End If
                walked = JsRuntime.JsAdd(walked, JsRuntime.JsAdd(i, ch))
            Next
            __lcont3:
            __upd4:
            i = JsRuntime.JsAdd(i, 1.0R)
        Loop
        __lexit3:
        Try
            JsRuntime.JsInvokeMethod(JsRuntime.Json, "parse", New JsValue() {"{"}, False, False)
        Catch jsex19 As LiteJsRuntime.JsRuntimeException
            Dim e As JsValue = jsex19.Payload
            JsRuntime.JsInvokeMethod(JsRuntime.ConsoleBuiltin, "log", New JsValue() {"json error:", JsRuntime.JsGet(e, "name")}, False, False)
        End Try
        JsRuntime.JsInvokeMethod(JsRuntime.ConsoleBuiltin, "log", New JsValue() {"grade:", grade, "walked:", walked}, False, False)
        [date] = "2026-10-14"
        JsRuntime.JsInvokeMethod(JsRuntime.ConsoleBuiltin, "log", New JsValue() {"iso:", JsRuntime.JsInvokeMethod([date], "replace", New JsValue() {New LiteJsRuntime.JsRegExp("(\d+)-(\d+)-(\d+)", ""), "$3/$2/$1"}, False, False), JsRuntime.JsInvokeMethod(New LiteJsRuntime.JsRegExp("\d+", "g"), "test", New JsValue() {[date]}, False, False)}, False, False)
        Dim Shape As JsValue = Nothing
        Shape = JsRuntime.JsDefineClass("Shape", CType(Function(args__20 As JsValue())
Dim area As JsValue = If(args__20.Length > 0, args__20(0), JsRuntime.Undef)
Dim this_7 As JsValue = JsRuntime.CurrentThis
Dim arguments_8 As JsValue = New List(Of Object)(args__20)
JsRuntime.JsSet(this_7, "area", area)
JsRuntime.JsSet(Shape, "made", JsRuntime.JsAdd(JsRuntime.JsGet(Shape, "made"), 1.0R))
Return JsRuntime.Undef

End Function, Func(Of Object(), Object)), Nothing, New (String, String, Object)() {("s", "made", 0R), ("i", "describe", CType(Function(args__21 As JsValue())
Dim this_8 As JsValue = JsRuntime.CurrentThis
Dim arguments_9 As JsValue = New List(Of Object)(args__21)
Return JsRuntime.JsAdd(JsRuntime.JsAdd("shape(", JsRuntime.JsGet(this_8, "area")), ")")
Return JsRuntime.Undef

End Function, Func(Of Object(), Object)))})
        Dim Square As JsValue = Nothing
        Square = JsRuntime.JsDefineClass("Square", CType(Function(args__22 As JsValue())
Dim side As JsValue = If(args__22.Length > 0, args__22(0), JsRuntime.Undef)
Dim this_9 As JsValue = JsRuntime.CurrentThis
Dim arguments_10 As JsValue = New List(Of Object)(args__22)
JsRuntime.JsSuperConstructor(New JsValue() {JsRuntime.JsMul(side, side)})
JsRuntime.JsSet(this_9, "side", side)
Return JsRuntime.Undef

End Function, Func(Of Object(), Object)), Shape, New (String, String, Object)() {("i", "describe", CType(Function(args__23 As JsValue())
Dim this_10 As JsValue = JsRuntime.CurrentThis
Dim arguments_11 As JsValue = New List(Of Object)(args__23)
Return JsRuntime.JsAdd(JsRuntime.JsAdd(JsRuntime.JsAdd("square(", JsRuntime.JsGet(this_10, "side")), ") -> "), JsRuntime.JsSuperInvoke("describe", New JsValue() {}))
Return JsRuntime.Undef

End Function, Func(Of Object(), Object)))})
        sq = JsRuntime.JsNew(Square, New JsValue() {3R})
        JsRuntime.JsInvokeMethod(JsRuntime.ConsoleBuiltin, "log", New JsValue() {JsRuntime.JsInvokeMethod(sq, "describe", New JsValue() {}, False, False), JsRuntime.JsInstanceOf(sq, Square), JsRuntime.JsInstanceOf(sq, Shape), JsRuntime.JsGet(Shape, "made")}, False, False)
        maybe = JsRuntime.JsObj(New (String, Object)() {("nested", JsRuntime.JsObj(New (String, Object)() {("value", 5R)}))})
        JsRuntime.JsInvokeMethod(JsRuntime.ConsoleBuiltin, "log", New JsValue() {"chain:", CType(Function() As JsValue
Try
Return JsRuntime.JsChainGet(JsRuntime.JsChainGet(maybe, "nested"), "value")
Catch oc24 As LiteJsRuntime.OptionalChainShortCircuit
Return JsRuntime.Undef
End Try
End Function, Func(Of Object(), Object)).Invoke(New Object() {}), CType(Function() As JsValue
Try
Return JsRuntime.JsChainGet(CType(Nothing, JsValue), "missing")
Catch oc25 As LiteJsRuntime.OptionalChainShortCircuit
Return JsRuntime.Undef
End Try
End Function, Func(Of Object(), Object)).Invoke(New Object() {})}, False, False)
        JsRuntime.JsInvokeMethod(JsRuntime.ConsoleBuiltin, "log", New JsValue() {"generated:", JsRuntime.JsInvokeMethod(JsRuntime.JsArrayFrom(JsRuntime.JsSpread(JsRuntime.JsInvoke(naturals, New JsValue() {4R}))), "join", New JsValue() {"-"}, False, False)}, False, False)
        JsRuntime.JsInvokeMethod(JsRuntime.JsInvoke(main, New JsValue() {}), "then", New JsValue() {CType(Function(args__26 As JsValue())
Return JsRuntime.JsInvokeMethod(JsRuntime.ConsoleBuiltin, "log", New JsValue() {"done"}, False, False)
Return JsRuntime.Undef

End Function, Func(Of Object(), Object))}, False, False)
        JsRuntime.JsInvokeMethod(JsRuntime.JsInvokeMethod(JsRuntime.PromiseBuiltin, "all", New JsValue() {JsRuntime.JsArray(JsRuntime.JsInvokeMethod(JsRuntime.PromiseBuiltin, "resolve", New JsValue() {1R}, False, False), JsRuntime.JsInvokeMethod(JsRuntime.PromiseBuiltin, "resolve", New JsValue() {2R}, False, False))}, False, False), "then", New JsValue() {CType(Function(args__27 As JsValue())
Dim values As JsValue = If(args__27.Length > 0, args__27(0), JsRuntime.Undef)
Return JsRuntime.JsInvokeMethod(JsRuntime.ConsoleBuiltin, "log", New JsValue() {"all:", JsRuntime.JsInvokeMethod(values, "join", New JsValue() {"+"}, False, False)}, False, False)
Return JsRuntime.Undef

End Function, Func(Of Object(), Object))}, False, False)
        JsRuntime.JsInvokeMethod(JsRuntime.ConsoleBuiltin, "log", New JsValue() {"clr:", JsRuntime.JsGet(JsRuntime.ResolveHostGlobal("bridge"), "Title"), JsRuntime.JsInvokeMethod(JsRuntime.ResolveHostGlobal("bridge"), "Add", New JsValue() {1R, 2R}, False, False), JsRuntime.JsGet(JsRuntime.JsGet(JsRuntime.ResolveHostGlobal("bridge"), "Items"), "length")}, False, False)
        JsRuntime.JsSet(JsRuntime.ResolveHostGlobal("bridge"), "Title", "updated")
        clrSum = 0R
        For Each item As JsValue In JsRuntime.JsIterableValues(JsRuntime.JsGet(JsRuntime.ResolveHostGlobal("bridge"), "Items"))
            clrSum = JsRuntime.JsAdd(clrSum, item)
        Next
        JsRuntime.JsInvokeMethod(JsRuntime.ConsoleBuiltin, "log", New JsValue() {"clr sum:", clrSum, JsRuntime.JsGe(JsRuntime.JsInvokeMethod(JsRuntime.JsInvokeMethod(JsRuntime.ObjectBuiltin, "keys", New JsValue() {JsRuntime.ResolveHostGlobal("bridge")}, False, False), "indexOf", New JsValue() {"Title"}, False, False), 0R), JsRuntime.JsIndex(JsRuntime.ResolveHostGlobal("bridge"), 1R)}, False, False)
        Try
            JsRuntime.JsInvokeMethod(JsRuntime.ResolveHostGlobal("bridge"), "Explode", New JsValue() {}, False, False)
        Catch jsex28 As LiteJsRuntime.JsRuntimeException
            Dim e As JsValue = jsex28.Payload
            JsRuntime.JsInvokeMethod(JsRuntime.ConsoleBuiltin, "log", New JsValue() {"clr error:", JsRuntime.JsGet(e, "name")}, False, False)
        End Try
        JsRuntime.DrainMicrotasks()
    End Sub
End Class
