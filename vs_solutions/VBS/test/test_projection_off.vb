' =============================================================
' demo: 关闭 SIMD 时 @ 投影仍然生效
'
'   vbs ./test/test_projection_off.vb --no-vectorize --verbose
'
' `@` 是**语法糖**(与 let、元组分解同级), 与 --no-vectorize / #no-vectorize 无关:
' 关闭之后投影照常展开, 只是不再把数组算术改写成 SIMD 调用,
' 于是数组上的聚合必须写 LINQ 形式(values.Sum() / values.Max())。
' =============================================================

Class Item
    Property value As Double
    Property label As String
End Class

Dim items As Item() = {
    New Item With {.value = 1.5, .label = "a"},
    New Item With {.value = 2.5, .label = "b"},
    New Item With {.value = 3.5, .label = "c"},
    New Item With {.value = 4.5, .label = "d"}
}

' ---- 单成员投影: 展开为 Select(...).ToArray() ----
Dim values = items@value
Call Console.WriteLine("values      = " & String.Join(", ", values) & "   (" & TypeName(values) & ")")

' ---- 关闭 SIMD 时聚合走 LINQ ----
Call Console.WriteLine("values.Sum  = " & values.Sum())
Call Console.WriteLine("values.Max  = " & values.Max())
Call Console.WriteLine("values.Count= " & values.Count())
Call Console.WriteLine()

' ---- 多属性投影 ----
Dim pairs = items@{value, label}
Call Console.WriteLine("pairs.Length= " & pairs.Length)
Call Console.WriteLine("pairs(0)    = " & pairs(0).label & " / " & pairs(0).value)
Call Console.WriteLine()

' ---- 字符串投影 ----
Dim labels = items@label
Call Console.WriteLine("labels      = " & String.Join(", ", labels))
