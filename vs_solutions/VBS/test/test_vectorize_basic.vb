' =============================================================
' demo: 数值向量的自动向量化(基础)
'
'   vbs ./test/test_vectorize_basic.vb
'
' 声明为数值数组的变量(Integer()/Double()/Single()...)在参与算术运算时,
' 引擎会在代码重构阶段把标量写法展开为等价的逐元素 SIMD 调用:
'
'   Dim x = {1, 2, 3, 4, 5}
'   Dim sum = x + 5
'   =>  Dim sum = SimdAddScalar(Of Integer)(x, 5)
'
'   Dim z = (x * y + 6) / (x + y)
'   =>  Dim z = SimdDivide(
'                   SimdConvert(Of Integer, Double)((SimdAddScalar(Of Integer)(SimdMultiply(x, y), 6))),
'                   SimdConvert(Of Integer, Double)((SimdAdd(x, y))))
'
' 使用 --verbose 可以打印重构之后的完整代码与改写点数量:
'
'   vbs ./test/test_vectorize_basic.vb --verbose
' =============================================================

Dim x = {1, 2, 3, 4, 5}
Dim y = {2, 3, 4, 5, 1}

Call Console.WriteLine("x          = " & String.Join(", ", x))
Call Console.WriteLine("y          = " & String.Join(", ", y))
Call Console.WriteLine()

' ---- 简单表达式: 向量 + 标量 ----
Dim sum = x + 5
Call Console.WriteLine("x + 5      = " & String.Join(", ", sum) & "   (" & TypeName(sum) & ")")

' ---- 整棵表达式树一次性展开 ----
Dim z = (x * y + 6) / (x + y)
Call Console.WriteLine("(x*y+6)/(x+y) = " & String.Join(", ", z) & "   (" & TypeName(z) & ")")
Call Console.WriteLine()

' ---- 显式声明的浮点向量 ----
Dim w As Double() = {1.0, 2.0, 3.0}
Dim v = w * 2.5 + 1
Call Console.WriteLine("w * 2.5 + 1 = " & String.Join(", ", v) & "   (" & TypeName(v) & ")")

' ---- 改写产生的向量结果可以继续参与后续运算 ----
Dim twice = z * 2
Call Console.WriteLine("z * 2       = " & String.Join(", ", twice) & "   (" & TypeName(twice) & ")")

Call Console.WriteLine()
Call Console.WriteLine("expected  = 8/3, 12/5, 18/7, 26/9, 11/6")
