' =============================================================
' demo: 投影结果参与 SIMD 向量化
'
'   vbs ./test/test_projection_simd.vb --verbose
'
' 投影得到的是普通一维数组, 因此后续所有向量化能力都自动适用:
' 向量算术、标量广播、按 VB 语义的类型提升、逐元素数学函数、聚合归约。
' =============================================================

Class Sample
    Property v As Double
    Property n As Integer
    Property f As Single
End Class

Dim data As Sample() = {
    New Sample With {.v = 1.0, .n = 10, .f = 1.5F},
    New Sample With {.v = 4.0, .n = 20, .f = 2.5F},
    New Sample With {.v = 9.0, .n = 30, .f = 3.5F},
    New Sample With {.v = 16.0, .n = 40, .f = 4.5F}
}

Call Console.WriteLine("v = " & String.Join(", ", data@v))
Call Console.WriteLine("n = " & String.Join(", ", data@n))
Call Console.WriteLine("f = " & String.Join(", ", data@f) & "   (" & TypeName(data@f) & ")")
Call Console.WriteLine()

' ---- 聚合归约: VecSum / VecMax / VecProduct / VecMin ----
Call Console.WriteLine("SUM(v)     = " & data@v.Sum())
Call Console.WriteLine("MAX(v)     = " & data@v.Max())
Call Console.WriteLine("MIN(v)     = " & data@v.Min())
Call Console.WriteLine("PRODUCT(n) = " & data@n.Product())
Call Console.WriteLine("COUNT(v)   = " & data@v.Count())
Call Console.WriteLine()

' ---- 向量算术(两侧都是投影) ----
Dim sum = data@v + data@v
Call Console.WriteLine("v + v      = " & String.Join(", ", sum))

Dim ratio = data@v / data@n
Call Console.WriteLine("v / n      = " & String.Join(", ", ratio) & "   (" & TypeName(ratio) & ")")
Call Console.WriteLine()

' ---- 投影与标量/字面量混算(含类型提升与 VecConvert) ----
Dim scaled = data@v * 2 + 1
Call Console.WriteLine("v * 2 + 1  = " & String.Join(", ", scaled))

Dim intPlus = data@n + 0.5
Call Console.WriteLine("n + 0.5    = " & String.Join(", ", intPlus) & "   (" & TypeName(intPlus) & ")")

Dim intMul = data@n * 3
Call Console.WriteLine("n * 3      = " & String.Join(", ", intMul) & "   (" & TypeName(intMul) & ")")

Dim intVec = data@n + {1, 2, 3, 4}
Call Console.WriteLine("n + {1..4} = " & String.Join(", ", intVec) & "   (" & TypeName(intVec) & ")")
Call Console.WriteLine()

' ---- 逐元素数学函数 ----
Dim roots = Math.Sqrt(data@v)
Call Console.WriteLine("SQRT(v)    = " & String.Join(", ", roots))

Dim sines = Math.Sin(data@n)
Call Console.WriteLine("SIN(n)     = " & String.Join(", ", sines))
Call Console.WriteLine()

' ---- 单精度成员保持 Single ----
Dim singles = data@f * 2.0F
Call Console.WriteLine("f * 2.0F   = " & String.Join(", ", singles) & "   (" & TypeName(singles) & ")")
