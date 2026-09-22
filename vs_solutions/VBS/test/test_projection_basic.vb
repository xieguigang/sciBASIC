' =============================================================
' demo: @ 数组投影运算符
'
'   vbs ./test/test_projection_basic.vb --verbose
'
' `list@x` 会把数组之中每个元素的 x 属性投影成一个新的数组:
'
'   Dim x = list@x
'   => Dim x = list.Select(Function(__vbs_o) __vbs_o.x).ToArray()
'
' 投影出来的数组可以直接参与 SIMD 向量化:
'
'   Dim z = list@x + {2, 3, 4, 5, 6, 7, 8, 9}
'   => Dim z = VecAdd(list.Select(Function(__vbs_o) __vbs_o.x).ToArray(),
'                     VecConvert(Of Integer, Double)({2, 3, 4, 5, 6, 7, 8, 9}))
' =============================================================

Class CLRObjectType
    Property x As Double
    Property y As String
End Class

Dim list As CLRObjectType() = {
    New CLRObjectType With {.x = 1.0, .y = "one"},
    New CLRObjectType With {.x = 2.0, .y = "two"},
    New CLRObjectType With {.x = 3.0, .y = "three"},
    New CLRObjectType With {.x = 4.0, .y = "four"},
    New CLRObjectType With {.x = 5.0, .y = "five"},
    New CLRObjectType With {.x = 6.0, .y = "six"},
    New CLRObjectType With {.x = 7.0, .y = "seven"},
    New CLRObjectType With {.x = 8.0, .y = "eight"}
}

Call Console.WriteLine($"list.Count = {list.Length}")
Call Console.WriteLine()

' ---- 单成员投影 ----
Dim x = list@x
Call Console.WriteLine("list@x         = " & String.Join(", ", x) & "   (" & TypeName(x) & ")")

Dim y = list@y
Call Console.WriteLine("list@y         = " & String.Join(", ", y) & "   (" & TypeName(y) & ")")
Call Console.WriteLine()

' ---- 投影结果参与 SIMD 向量化 ----
Dim z = list@x + {2, 3, 4, 5, 6, 7, 8, 9}
Call Console.WriteLine("list@x + {2..9} = " & String.Join(", ", z) & "   (" & TypeName(z) & ")")

' ---- 投影结果也可以继续做标量运算 ----
Dim scaled = list@x * 10
Call Console.WriteLine("list@x * 10    = " & String.Join(", ", scaled))

Call Console.WriteLine()
Call Console.WriteLine("expected list@x + {2..9} = 3, 5, 7, 9, 11, 13, 15, 17")
