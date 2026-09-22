' =============================================================
' demo: @ 链式投影(嵌套对象)
'
'   vbs ./test/test_projection_chain.vb --verbose
'
' 一条语句里连续投影:
'
'   Dim xs = items@inner@x
'   => Dim xs = items.Select(Function(__vbs_o) __vbs_o.inner).ToArray()
'                 .Select(Function(__vbs_o) __vbs_o.x).ToArray()
'
' 分两行写也完全等价 —— 中间结果的元素类型会被继续登记。
' =============================================================

Class Inner
    Property x As Double
    Property tag As String
End Class

Class Outer
    Property inner As Inner
    Property name As String
End Class

Dim items As Outer() = {
    New Outer With {.inner = New Inner With {.x = 1.5, .tag = "a"}, .name = "one"},
    New Outer With {.inner = New Inner With {.x = 2.5, .tag = "b"}, .name = "two"},
    New Outer With {.inner = New Inner With {.x = 3.5, .tag = "c"}, .name = "three"},
    New Outer With {.inner = New Inner With {.x = 4.5, .tag = "d"}, .name = "four"}
}

' ---- 一行之内链式投影 ----
Dim xs = items@inner@x
Call Console.WriteLine("items@inner@x   = " & String.Join(", ", xs) & "   (" & TypeName(xs) & ")")

' ---- 拆成两行同样可以(中间投影结果被登记为 Inner()) ----
Dim inners = items@inner
Dim ys = inners@x
Call Console.WriteLine("inners@x        = " & String.Join(", ", ys) & "   (" & TypeName(ys) & ")")

' ---- 内层字符串成员 ----
Dim tags = items@inner@tag
Call Console.WriteLine("items@inner@tag = " & String.Join(", ", tags))

' ---- 外层字符串成员 ----
Dim names = items@name
Call Console.WriteLine("items@name      = " & String.Join(", ", names))

Call Console.WriteLine()

' ---- 链式投影结果照样参与 SIMD ----
Dim total = items@inner@x.Sum()
Call Console.WriteLine("SUM = " & total)

Dim scaled = items@inner@x * 2
Call Console.WriteLine("x * 2 = " & String.Join(", ", scaled))
