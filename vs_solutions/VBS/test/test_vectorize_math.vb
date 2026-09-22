' =============================================================
' demo: 逐元素数学函数
'
'   vbs ./test/test_vectorize_math.vb
'
' 运行时对下列函数提供了 SIMD 内核, 会被直接映射:
'
'   Abs / Sqrt / Exp / Log / Sign / Floor / Ceiling / Truncate / Reciprocal
'
' 其余函数(三角函数、Round、Log10)没有专用内核, 通过 VecMap 逐元素化:
'
'   Math.Sin(v)  =>  VecMap(Of Double, Double)(v, Function(__v As Double) Math.Sin(__v))
' =============================================================

Dim x = {1.0, 4.0, 9.0, 16.0}
Dim y = {0.0, 1.0, 2.0, 3.0}

Call Console.WriteLine("x        = " & String.Join(", ", x))
Call Console.WriteLine()

' ---- 有专用 SIMD 内核 ----
Call Console.WriteLine("Math.Sqrt(x)   = " & String.Join(", ", Math.Sqrt(x)))
Call Console.WriteLine("Math.Exp(y)    = " & String.Join(", ", Math.Exp(y)))
Call Console.WriteLine("Math.Log(x)    = " & String.Join(", ", Math.Log(x)))
Call Console.WriteLine("Math.Abs(-x)   = " & String.Join(", ", Math.Abs(-x)))
Call Console.WriteLine("Math.Floor(x/3)= " & String.Join(", ", Math.Floor(x / 3.0)))
Call Console.WriteLine("Math.Sign(x-4)= " & String.Join(", ", Math.Sign(x - 4.0)))
Call Console.WriteLine()

' ---- 通过 VecMap 逐元素化 ----
Call Console.WriteLine("Math.Sin(y)    = " & String.Join(", ", Math.Sin(y)))
Call Console.WriteLine("Math.Cos(y)    = " & String.Join(", ", Math.Cos(y)))
Call Console.WriteLine("Math.Round(y/3)= " & String.Join(", ", Math.Round(y / 3.0)))
Call Console.WriteLine()

' ---- 整数向量会被先提升为 Double ----
Dim n = {1, 2, 3, 4}
Call Console.WriteLine("n              = " & String.Join(", ", n) & "   (" & TypeName(n) & ")")
Call Console.WriteLine("Math.Sqrt(n)   = " & String.Join(", ", Math.Sqrt(n)) & "   (" & TypeName(Math.Sqrt(n)) & ")")
Call Console.WriteLine()

' ---- 与算术混合: Math.Sqrt(x) * 2 + 1 ----
Dim combo = Math.Sqrt(x) * 2 + 1
Call Console.WriteLine("Sqrt(x)*2+1    = " & String.Join(", ", combo))
