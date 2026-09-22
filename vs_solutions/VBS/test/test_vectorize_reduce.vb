' =============================================================
' demo: 聚合归约
'
'   vbs ./test/test_vectorize_reduce.vb
'
' 数值向量上的零实参聚合调用会被映射到运行时的 SIMD 归约:
'
'   x.Sum()     => VecSum(x)
'   x.Average() => VecMean(x)
'   x.Min()     => VecMin(x)
'   x.Max()     => VecMax(x)
'   x.Product() => VecProduct(x)
'   x.Count()   => VecCount(Of Integer)(x)
' =============================================================

Dim x = {1, 2, 3, 4, 5}

Call Console.WriteLine("x         = " & String.Join(", ", x))
Call Console.WriteLine()

Call Console.WriteLine("Sum       = " & x.Sum() & "   (" & TypeName(x.Sum()) & ")")
Call Console.WriteLine("Average   = " & x.Average() & "   (" & TypeName(x.Average()) & ")")
Call Console.WriteLine("Min       = " & x.Min() & "   (" & TypeName(x.Min()) & ")")
Call Console.WriteLine("Max       = " & x.Max() & "   (" & TypeName(x.Max()) & ")")
Call Console.WriteLine("Product   = " & x.Product() & "   (" & TypeName(x.Product()) & ")")
Call Console.WriteLine("Count     = " & x.Count() & "   (" & TypeName(x.Count()) & ")")
Call Console.WriteLine()

' ---- Double 向量 ----
Dim d As Double() = {1.5, 2.5, 3.5, 4.5}
Call Console.WriteLine("d         = " & String.Join(", ", d))
Call Console.WriteLine("d.Sum     = " & d.Sum())
Call Console.WriteLine("d.Mean    = " & d.Mean())
Call Console.WriteLine("d.Max     = " & d.Max())
Call Console.WriteLine("d.Product = " & d.Product())
Call Console.WriteLine()

' ---- 归约结果可以继续参与算术 ----
Dim normalized = x / x.Sum()
Call Console.WriteLine("x / x.Sum() = " & String.Join(", ", normalized))
Call Console.WriteLine("verify      = " & normalized.Sum())
