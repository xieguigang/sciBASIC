' ============================================================================
' VectorMatrixSimdTest.vb — Vector / NumericMatrix 的 SIMD 重构正确性验证
' ----------------------------------------------------------------------------
' 覆盖：
'   + 向量逐元素运算（含长度 1 广播、空向量、非向量宽度整数倍的尾块）
'   + 点积 / 模 / 单位化（FMA 路径）
'   + 比较运算符（标量广播与向量对向量）
'   + 矩阵逐元素运算、就地版本、零安全除法语义
'   + 矩阵乘法（SimdParallel.MatrixDot）、转置、范数、迹、极值、按轴归约
'   + 分解与求解器（高斯消元 / LU / Cholesky）回归
'   + SIMDConfiguration.disable 的全局标量回退
' ============================================================================

Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
Imports SimdCapabilities = Microsoft.VisualBasic.Math.SIMD.SimdCapabilities
Imports SimdEngine = Microsoft.VisualBasic.Math.SIMD.SimdEngine
Imports SimdParallel = Microsoft.VisualBasic.Math.SIMD.SimdParallel
Imports SIMDConfiguration = Microsoft.VisualBasic.Math.SIMD.SIMDConfiguration
Imports SIMDEnvironment = Microsoft.VisualBasic.Math.SIMD.SIMDEnvironment
Imports std = System.Math

Public Module VectorMatrixSimdTest

    Private pass As Integer = 0
    Private fail As Integer = 0

    Public Function RunAll() As Integer
        pass = 0
        fail = 0

        Console.WriteLine("==================================================================")
        Console.WriteLine(" Vector / NumericMatrix SIMD 重构正确性验证")
        Console.WriteLine("==================================================================")
        Console.WriteLine($" SIMD: {SimdCapabilities.Description}")
        Console.WriteLine($" IsEnabled = {SIMDEnvironment.IsEnabled}")
        Console.WriteLine()

        TestVectorElementwise()
        TestVectorBroadcast()
        TestVectorDotAndNorms()
        TestVectorComparisons()
        TestMatrixElementwise()
        TestMatrixInPlace()
        TestMatrixMultiply()
        TestMatrixShapeAndNorms()
        TestMatrixReductions()
        TestMatrixHelpers()
        TestSolversAndDecompositions()
        TestScalarFallback()

        Console.WriteLine()
        Console.WriteLine("==================================================================")
        Console.WriteLine($" SIMD 测试完成: {pass} 通过, {fail} 失败, 共 {pass + fail} 项")
        Console.WriteLine("==================================================================")

        Return If(fail > 0, 1, 0)
    End Function

#Region "assert helpers"

    Private Sub Check(name As String, condition As Boolean)
        If condition Then
            pass += 1
        Else
            fail += 1
            Console.WriteLine($"  [FAIL] {name}")
        End If
    End Sub

    Private Sub CheckClose(name As String, expected As Double, actual As Double)
        Const tol As Double = 0.000000001

        Dim scale As Double = std.Max(1.0, std.Abs(expected))
        Dim diff As Double = std.Abs(expected - actual)

        If diff <= tol * scale Then
            pass += 1
        Else
            fail += 1
            Console.WriteLine($"  [FAIL] {name}: 期望 {expected:G17}, 实际 {actual:G17}, 偏差 {diff:G6}")
        End If
    End Sub

    Private Sub CheckRelative(name As String, expected As Double, actual As Double, tolerance As Double)
        Dim scale As Double = std.Max(1.0, std.Abs(expected))

        If std.Abs(expected - actual) <= tolerance * scale Then
            pass += 1
        Else
            fail += 1
            Console.WriteLine($"  [FAIL] {name}: 期望 {expected:G17}, 实际 {actual:G17}")
        End If
    End Sub

    Private Sub Section(title As String)
        Console.WriteLine($"--- {title} ---")
    End Sub

    ''' <summary>
    ''' 创建一份不与被引用数据共享内存的矩阵副本。
    ''' </summary>
    ''' <remarks>
    ''' <c>New NumericMatrix(Double()())</c> 会直接引用传入的数组，就地运算会污染调用方的数据，
    ''' 因此在测试就地版本时必须显式做一次深拷贝。
    ''' </remarks>
    Private Function Fresh(a As Double()()) As NumericMatrix
        Return DirectCast(NumericMatrix.Create(a), NumericMatrix)
    End Function

#End Region

#Region "test data"

    ''' <summary>
    ''' 故意选一个不是向量寄存器宽度整数倍的长度，用于覆盖尾块处理。
    ''' </summary>
    Private Function SampleData(length As Integer, seed As Integer) As Double()
        Dim rnd As New Random(seed)
        Dim data As Double() = New Double(length - 1) {}

        For i As Integer = 0 To length - 1
            data(i) = std.Round((rnd.NextDouble() - 0.5) * 20, 4)
        Next

        Return data
    End Function

    Private Function SampleMatrix(rows As Integer, cols As Integer, seed As Integer) As Double()()
        Dim rnd As New Random(seed)
        Dim m As Double()() = New Double(rows - 1)() {}

        For i As Integer = 0 To rows - 1
            m(i) = New Double(cols - 1) {}

            For j As Integer = 0 To cols - 1
                m(i)(j) = std.Round((rnd.NextDouble() - 0.5) * 10, 4)
            Next
        Next

        Return m
    End Function

    Private Function ScalarAdd(a As Double(), b As Double()) As Double()
        Dim out As Double() = New Double(a.Length - 1) {}
        For i As Integer = 0 To a.Length - 1
            out(i) = a(i) + b(i)
        Next
        Return out
    End Function

    Private Function ScalarDot(a As Double(), b As Double()) As Double
        Dim sum As Double = 0
        For i As Integer = 0 To a.Length - 1
            sum += a(i) * b(i)
        Next
        Return sum
    End Function

    Private Function ScalarMatMul(a As Double()(), b As Double()()) As Double()()
        Dim rows As Integer = a.Length
        Dim inner As Integer = b.Length
        Dim cols As Integer = b(0).Length
        Dim c As Double()() = New Double(rows - 1)() {}

        For i As Integer = 0 To rows - 1
            c(i) = New Double(cols - 1) {}

            For j As Integer = 0 To cols - 1
                Dim s As Double = 0

                For k As Integer = 0 To inner - 1
                    s += a(i)(k) * b(k)(j)
                Next

                c(i)(j) = s
            Next
        Next

        Return c
    End Function

    Private Function MatrixBinary(a As Double()(), b As Double()(), op As Func(Of Double, Double, Double)) As Double()()
        Dim out As Double()() = New Double(a.Length - 1)() {}

        For i As Integer = 0 To a.Length - 1
            out(i) = New Double(a(i).Length - 1) {}

            For j As Integer = 0 To a(i).Length - 1
                out(i)(j) = op(a(i)(j), b(i)(j))
            Next
        Next

        Return out
    End Function

    Private Function ArraysEqual(a As Double(), b As Double(), tolerance As Double) As Boolean
        If a.Length <> b.Length Then Return False

        For i As Integer = 0 To a.Length - 1
            Dim scale As Double = std.Max(1.0, std.Max(std.Abs(a(i)), std.Abs(b(i))))

            If std.Abs(a(i) - b(i)) > tolerance * scale Then Return False
        Next

        Return True
    End Function

    Private Function MatrixEquals(a As Double()(), b As Double()(), tolerance As Double) As Boolean
        If a.Length <> b.Length Then Return False

        For i As Integer = 0 To a.Length - 1
            If Not ArraysEqual(a(i), b(i), tolerance) Then Return False
        Next

        Return True
    End Function

#End Region

#Region "vector"

    Private Sub TestVectorElementwise()
        Section("向量逐元素运算")

        Dim a As Double() = SampleData(1001, 1)
        Dim b As Double() = SampleData(1001, 2)

        ' 保证分母非零
        For i As Integer = 0 To b.Length - 1
            If std.Abs(b(i)) < 0.5 Then b(i) += 1.5
        Next

        Dim va As New Vector(a)
        Dim vb As New Vector(b)

        Check("向量加法", ArraysEqual((va + vb).Array, ScalarAdd(a, b), 0.000000001))
        Check("向量减法", ArraysEqual((va - vb).Array, ScalarAdd(a, ScalarNegate(b)), 0.000000001))
        Check("向量乘法", ArraysEqual((va * vb).Array, MatrixBinaryHelper(a, b), 0.000000001))

        Dim div As Double() = (va / vb).Array
        CheckClose("向量除法", a(5) / b(5), div(5))

        ' 零安全语义：分子为 0 时结果必须为 0，而不是 NaN
        Dim zeroSafe As Double() = (New Vector({0.0, 1.0, 0.0, 5.0}) / New Vector({0.0, 0.0, 3.0, 2.0})).Array
        Check("0/0 = 0（零安全）", zeroSafe(0) = 0.0 AndAlso Not Double.IsNaN(zeroSafe(0)))
        Check("1/0 保持 IEEE 语义", Double.IsInfinity(zeroSafe(1)))
        Check("0/3 = 0", zeroSafe(2) = 0.0)
        Check("5/2 = 2.5", zeroSafe(3) = 2.5)

        ' 数乘 / 数加 / 数减 / 数除
        Dim expected As Double() = New Double(a.Length - 1) {}
        For i As Integer = 0 To a.Length - 1
            expected(i) = a(i) * 2.5
        Next
        Check("向量数乘", ArraysEqual((va * 2.5).Array, expected, 0.000000001))

        Check("向量数加", (va + 3.0).Array(5) = a(5) + 3.0)
        Check("标量加向量", (3.0 + va).Array(5) = a(5) + 3.0)
        Check("向量数减", (va - 3.0).Array(5) = a(5) - 3.0)
        Check("标量减向量", (3.0 - va).Array(5) = 3.0 - a(5))
        Check("向量数除", (va / 4.0).Array(5) = a(5) / 4.0)
        Check("标量除向量", (4.0 / va).Array(5) = 4.0 / a(5))
        Check("一元取负", (-va).Array(5) = -a(5))

        ' 幂运算
        Check("v ^ 2", (va ^ 2).Array(7) = a(7) ^ 2)
        Check("v ^ 0.5", (New Vector({4.0, 9.0}) ^ 0.5).Array(1) = 3.0)
        Check("v ^ 3", std.Abs((va ^ 3).Array(7) - a(7) * a(7) * a(7)) < 0.000000001)
        Check("v ^ p（向量次幂）", (New Vector({2.0, 3.0}) ^ New Vector({3.0, 2.0})).Array(0) = 8.0)

        ' 一元映射
        Dim pos As New Vector({-4.0, 9.0, 0.0, -0.5, 2.5})
        Check("Abs", Vector.Abs(pos).Array(0) = 4.0)
        Check("Sqrt", Vector.Sqrt(New Vector({4.0, 9.0})).Array(1) = 3.0)
        Check("Trunc", Vector.Trunc(New Vector({-2.7, 2.7})).Array(0) = -2.0)
        Check("floor", Vector.floor(New Vector({-2.2, 2.7})).Array(0) = -3.0)
        Check("Sign（负数）", Vector.Sign(pos).Array(0) = -1.0)
        Check("Sign（正数）", Vector.Sign(pos).Array(1) = 1.0)
        Check("Sign（零）", Vector.Sign(pos).Array(2) = 0.0)
        Check("round", Vector.round(New Vector({1.23456}), 2).Array(0) = 1.23)
        Check("Max(v, 标量)", Vector.Max(pos, 1.0).Array(0) = 1.0)
        Check("Max(v1, v2)", Vector.Max(New Vector({1.0, 5.0}), New Vector({3.0, 2.0})).Array(0) = 3.0)
        Check("Min(v, 标量)", Vector.Min(pos, 0.0).Array(1) = 0.0)
        Check("Min(v1, v2)", Vector.Min(New Vector({1.0, 5.0}), New Vector({3.0, 2.0})).Array(1) = 2.0)
        CheckClose("Max(v)", 9.0, Vector.Max(pos))
        CheckClose("Min(v)", -4.0, Vector.Min(pos))

        ' Exp / Log / Log10（无硬件指令，验证数值一致性）
        Dim logInput As New Vector({1.0, 2.5, 10.0})
        CheckClose("Log 自然对数", std.Log(2.5), Vector.Log(logInput).Array(1))
        CheckClose("Log10", std.Log10(2.5), Vector.Log(logInput, base:=10).Array(1))
        CheckClose("Exp", std.Exp(2.5), Vector.Exp(logInput).Array(1))

        ' 空向量
        Dim empty As New Vector(New Double() {})
        Check("空向量加法返回空", (empty + empty).Length = 0)
        Check("空向量平方和为 0", empty.Mod = 0.0)
        Check("空向量 L2 范数为 0", empty.SumMagnitude = 0.0)
    End Sub

    Private Function ScalarNegate(a As Double()) As Double()
        Dim out As Double() = New Double(a.Length - 1) {}
        For i As Integer = 0 To a.Length - 1
            out(i) = -a(i)
        Next
        Return out
    End Function

    Private Function MatrixBinaryHelper(a As Double(), b As Double()) As Double()
        Dim out As Double() = New Double(a.Length - 1) {}
        For i As Integer = 0 To a.Length - 1
            out(i) = a(i) * b(i)
        Next
        Return out
    End Function

    Private Sub TestVectorBroadcast()
        Section("向量长度 1 广播")

        Dim scalar As New Vector({3.0})
        Dim v As New Vector({1.0, 2.0, 3.0, 4.0, 5.0})

        Dim added As Double() = (scalar + v).Array
        Check("标量向量 + 向量（广播）", added(0) = 4.0 AndAlso added(4) = 8.0)

        Dim added2 As Double() = (v + scalar).Array
        Check("向量 + 标量向量（广播）", added2(2) = 6.0)
    End Sub

    Private Sub TestVectorDotAndNorms()
        Section("点积 / 模 / 单位化")

        Dim a As Double() = SampleData(777, 11)
        Dim b As Double() = SampleData(777, 12)

        Dim va As New Vector(a)
        Dim vb As New Vector(b)

        CheckClose("内积运算符 Or", ScalarDot(a, b), va Or vb)
        CheckClose("dot(Double(), Double())", ScalarDot(a, b), Vector.dot(a, b))
        CheckClose("DotProduct", ScalarDot(a, b), va.DotProduct(vb))

        Dim sumSq As Double = ScalarDot(a, a)
        CheckClose("Mod（平方和）", sumSq, va.Mod)
        CheckClose("SumMagnitude（L2 范数）", std.Sqrt(sumSq), va.SumMagnitude)
        CheckClose("Unit 第一项", a(0) / std.Sqrt(sumSq), va.Unit.Array(0))

        ' Single 版本点积（FMA + Double 累加）
        Dim sa As Single() = New Single(a.Length - 1) {}
        Dim sb As Single() = New Single(b.Length - 1) {}
        For i As Integer = 0 To a.Length - 1
            sa(i) = CSng(a(i))
            sb(i) = CSng(b(i))
        Next

        Dim expectedSingle As Double = 0
        For i As Integer = 0 To sa.Length - 1
            expectedSingle += CDbl(sa(i)) * CDbl(sb(i))
        Next

        ' Single 内核用 FMA 融合乘加，与「先乘后加」的标量参考在末位上有差异，
        ' 因此这里用相对误差 1e-5 的口径比较
        CheckRelative("dot(Single, Single)", expectedSingle, Vector.dot(sa, sb), 0.00001)
        CheckClose("长度 1 向量点积", 6.0, (New Vector({2.0}) Or New Vector({3.0})))
    End Sub

    Private Sub TestVectorComparisons()
        Section("向量比较运算符")

        Dim v As New Vector({1.0, 2.0, 3.0, 2.0})

        Dim eq As Boolean() = (v = 2.0).ToArray
        Check("v = 2 长度", eq.Length = 4)
        Check("v = 2 结果", (Not eq(0)) AndAlso eq(1) AndAlso (Not eq(2)) AndAlso eq(3))

        Dim ne As Boolean() = (v <> 2.0).ToArray
        Check("v <> 2", ne(0) AndAlso (Not ne(1)))

        Dim gt As Boolean() = (v > 2.0).ToArray
        Check("v > 2", (Not gt(0)) AndAlso (Not gt(1)) AndAlso gt(2))

        Dim lt As Boolean() = (v < 2.0).ToArray
        Check("v < 2", lt(0) AndAlso (Not lt(1)))

        Dim ge As Boolean() = (v >= 2.0).ToArray
        Check("v >= 2", (Not ge(0)) AndAlso ge(1) AndAlso ge(2))

        Dim le As Boolean() = (v <= 2.0).ToArray
        Check("v <= 2", le(0) AndAlso le(1) AndAlso (Not le(2)))

        Dim other As New Vector({0.0, 3.0, 3.0, 1.0})

        Dim vge As Boolean() = (v >= other).ToArray
        Check("v1 >= v2", vge(0) AndAlso (Not vge(1)) AndAlso vge(2) AndAlso vge(3))

        Dim vle As Boolean() = (v <= other).ToArray
        Check("v1 <= v2", (Not vle(0)) AndAlso vle(1) AndAlso vle(2) AndAlso (Not vle(3)))

        Dim sle As Boolean() = (2.0 <= v).ToArray
        Check("标量 <= 向量", (Not sle(0)) AndAlso sle(1) AndAlso sle(2))

        ' 注意：标量 >= 向量的历史实现是「Not (x <= y)」，即严格大于语义，
        ' 本次重构保持该可观察行为不变
        Dim sge As Boolean() = (2.0 >= v).ToArray
        Check("标量 >= 向量（保留历史语义）", sge(0) AndAlso (Not sge(1)) AndAlso (Not sge(2)))
    End Sub

#End Region

#Region "matrix"

    Private Sub TestMatrixElementwise()
        Section("矩阵逐元素运算")

        Dim a As Double()() = SampleMatrix(37, 13, 21)
        Dim b As Double()() = SampleMatrix(37, 13, 22)

        ' 保证除数非零
        For i As Integer = 0 To b.Length - 1
            For j As Integer = 0 To b(i).Length - 1
                If std.Abs(b(i)(j)) < 1.0 Then b(i)(j) += 2.0
            Next
        Next

        Dim ma As New NumericMatrix(a)
        Dim mb As New NumericMatrix(b)

        Check("矩阵加法", MatrixEquals((ma + mb).ArrayPack(deepcopy:=False), MatrixBinary(a, b, Function(x, y) x + y), 0.000000001))
        Check("矩阵减法", MatrixEquals((ma - mb).ArrayPack(deepcopy:=False), MatrixBinary(a, b, Function(x, y) x - y), 0.000000001))
        Check("矩阵逐元素乘法", MatrixEquals(ma.ArrayMultiply(mb).ArrayPack(deepcopy:=False), MatrixBinary(a, b, Function(x, y) x * y), 0.000000001))

        Dim rightDivide As Double()() = ma.ArrayRightDivide(mb).ArrayPack(deepcopy:=False)
        CheckClose("矩阵右除（一般项）", a(3)(4) / b(3)(4), rightDivide(3)(4))

        Dim zeroNum As New NumericMatrix(New Double()() {New Double() {0.0, 1.0}, New Double() {2.0, 0.0}})
        Dim zeroDen As New NumericMatrix(New Double()() {New Double() {0.0, 1.0}, New Double() {2.0, 0.0}})
        Dim safe As Double()() = zeroNum.ArrayRightDivide(zeroDen).ArrayPack(deepcopy:=False)
        Check("矩阵右除零安全 (0/0 = 0)", safe(0)(0) = 0.0 AndAlso Not Double.IsNaN(safe(0)(0)))
        Check("矩阵右除零安全 (2/2 = 1)", safe(1)(0) = 1.0)

        Dim leftDivide As Double()() = ma.ArrayLeftDivide(mb).ArrayPack(deepcopy:=False)
        CheckClose("矩阵左除", b(3)(4) / a(3)(4), leftDivide(3)(4))

        ' 标量运算
        CheckClose("矩阵数乘", a(5)(6) * 3.0, ma.Multiply(3.0).ArrayPack(deepcopy:=False)(5)(6))
        CheckClose("标量乘矩阵", a(5)(6) * 3.0, (3.0 * ma).ArrayPack(deepcopy:=False)(5)(6))
        CheckClose("矩阵除以标量", a(5)(6) / 3.0, (ma / 3.0).ArrayPack(deepcopy:=False)(5)(6))
        CheckClose("矩阵减标量", a(5)(6) - 1.5, (ma - 1.5).ArrayPack(deepcopy:=False)(5)(6))
        CheckClose("标量减矩阵", 1.5 - a(5)(6), (1.5 - ma).ArrayPack(deepcopy:=False)(5)(6))
        CheckClose("标量加矩阵", a(5)(6) + 1.5, (1.5 + ma).ArrayPack(deepcopy:=False)(5)(6))
        CheckClose("标量除以矩阵", 4.0 / a(5)(6), (4.0 / ma).ArrayPack(deepcopy:=False)(5)(6))
        CheckClose("一元取负", -a(5)(6), (-ma).ArrayPack(deepcopy:=False)(5)(6))

        ' 一元映射
        CheckClose("矩阵绝对值", std.Abs(a(5)(6)), ma.Abs().ArrayPack(deepcopy:=False)(5)(6))
        CheckClose("矩阵幂", a(5)(6) ^ 2, ma.Power(2.0).ArrayPack(deepcopy:=False)(5)(6))

        Dim positive As Double()() = New Double()() {
            New Double() {1.0, 2.5, 10.0},
            New Double() {4.0, 8.0, 16.0}
        }
        Dim logMatrix As Double()() = New NumericMatrix(positive).Log().ArrayPack(deepcopy:=False)
        CheckClose("矩阵对数", std.Log(2.5), logMatrix(0)(1))
    End Sub

    Private Sub TestMatrixInPlace()
        Section("矩阵就地运算与拷贝")

        Dim a As Double()() = SampleMatrix(9, 7, 31)
        Dim b As Double()() = SampleMatrix(9, 7, 32)

        Dim mb As NumericMatrix = Fresh(b)

        Dim sum As GeneralMatrix = Fresh(a).AddEquals(mb)
        Check("AddEquals 就地更新", MatrixEquals(sum.ArrayPack(deepcopy:=False), MatrixBinary(a, b, Function(x, y) x + y), 0.000000001))

        Check("ArrayMultiplyEquals", MatrixEquals(Fresh(a).ArrayMultiplyEquals(mb).ArrayPack(deepcopy:=False), MatrixBinary(a, b, Function(x, y) x * y), 0.000000001))

        Dim multiplied As NumericMatrix = Fresh(a)
        multiplied.MultiplyEquals(2.0)
        CheckClose("MultiplyEquals", a(2)(3) * 2.0, multiplied(2, 3))

        Check("ArrayLeftDivideEquals", MatrixEquals(Fresh(a).ArrayLeftDivideEquals(mb).ArrayPack(deepcopy:=False), MatrixBinary(b, a, Function(x, y) x / y), 0.000000001))

        ' 深拷贝必须与源数据解耦
        Dim source As New NumericMatrix(a)
        Dim copyData As Double()() = source.Copy().ArrayPack(deepcopy:=False)
        copyData(0)(0) = 999.0
        Check("Copy() 深拷贝解耦", source(0, 0) <> 999.0)
    End Sub

    Private Sub TestMatrixMultiply()
        Section("矩阵乘法")

        Dim a As Double()() = SampleMatrix(23, 17, 41)
        Dim b As Double()() = SampleMatrix(17, 29, 42)
        Dim expected As Double()() = ScalarMatMul(a, b)

        Dim ma As New NumericMatrix(a)
        Dim mb As New NumericMatrix(b)

        Dim product As Double()() = ma.Multiply(mb).ArrayPack(deepcopy:=False)
        Check("DotProduct 形状", product.Length = 23 AndAlso product(0).Length = 29)
        Check("DotProduct 数值", MatrixEquals(product, expected, 0.000000001))

        ' 逐元素乘法需要两个同形矩阵
        Dim mwise As NumericMatrix = Fresh(SampleMatrix(23, 17, 45))
        Dim wise As Double()() = (ma * mwise).ArrayPack(deepcopy:=False)
        Check("逐元素乘法形状", wise.Length = 23 AndAlso wise(0).Length = 17)

        Dim v As New Vector(SampleData(17, 43))
        CheckClose("DotMultiply 第 3 行", ScalarDot(a(3), v.Array), ma.DotMultiply(v).Array(3))

        ' 矩阵按行缩放（保持历史行为：就地缩放左操作数）
        Dim rowScaled As NumericMatrix = Fresh(a) * New Vector(SampleData(23, 44))
        Check("矩阵按行缩放尺寸", rowScaled.RowDimension = 23 AndAlso rowScaled.ColumnDimension = 17)

        ' MatrixOps 的矩形数组乘法
        Dim rectA(2, 2) As Double
        Dim rectB(2, 2) As Double
        For i As Integer = 0 To 2
            For j As Integer = 0 To 2
                rectA(i, j) = a(i)(j)
                rectB(i, j) = b(i)(j)
            Next
        Next

        Dim rectC As Double(,) = MatrixOps.Multiply(rectA, rectB)
        Dim refC As Double = 0
        For k As Integer = 0 To 2
            refC += a(1)(k) * b(k)(2)
        Next
        CheckClose("MatrixOps.Multiply", refC, rectC(1, 2))

        Dim rectY As Double() = MatrixOps.MultiplyVec(rectA, New Double() {1.0, 2.0, 3.0})
        CheckClose("MatrixOps.MultiplyVec", a(1)(0) * 1.0 + a(1)(1) * 2.0 + a(1)(2) * 3.0, rectY(1))
    End Sub

    Private Sub TestMatrixShapeAndNorms()
        Section("矩阵转置 / 范数 / 迹")

        Dim a As Double()() = SampleMatrix(11, 6, 51)
        Dim m As New NumericMatrix(a)

        Dim t As Double()() = m.Transpose().ArrayPack(deepcopy:=False)
        Check("转置形状", t.Length = 6 AndAlso t(0).Length = 11)
        CheckClose("转置数值", a(7)(3), t(3)(7))

        Dim norm1Expected As Double = 0
        For j As Integer = 0 To 5
            Dim s As Double = 0
            For i As Integer = 0 To 10
                s += std.Abs(a(i)(j))
            Next
            norm1Expected = std.Max(norm1Expected, s)
        Next
        CheckClose("Norm1", norm1Expected, m.Norm1())

        Dim normInfExpected As Double = 0
        For i As Integer = 0 To 10
            Dim s As Double = 0
            For j As Integer = 0 To 5
                s += std.Abs(a(i)(j))
            Next
            normInfExpected = std.Max(normInfExpected, s)
        Next
        CheckClose("NormInf", normInfExpected, m.NormInf())

        Dim sumSq As Double = 0
        For i As Integer = 0 To 10
            For j As Integer = 0 To 5
                sumSq += a(i)(j) * a(i)(j)
            Next
        Next
        CheckClose("NormF", std.Sqrt(sumSq), m.NormF())

        Dim tr As Double = 0
        For i As Integer = 0 To 5
            tr += a(i)(i)
        Next
        CheckClose("Trace", tr, m.Trace())

        ' DiagonalVector 要求方阵（与原实现一致）
        Dim squareData As Double()() = SampleMatrix(8, 8, 52)
        CheckClose("DiagonalVector", squareData(4)(4), New NumericMatrix(squareData).DiagonalVector.Array(4))

        CheckClose("RowPackedCopy", a(3)(2), m.RowPackedCopy(3 * 6 + 2))
        CheckClose("ColumnPackedCopy", a(3)(2), m.ColumnPackedCopy(3 + 2 * 11))
    End Sub

    Private Sub TestMatrixReductions()
        Section("矩阵归约 / 极值 / 按轴")

        Dim a As Double()() = SampleMatrix(13, 8, 61)
        Dim m As New NumericMatrix(a)

        Dim maxRow As Integer = -1
        Dim maxCol As Integer = -1
        Dim maxVal As Double = m.Max(maxRow, maxCol)
        CheckClose("Max 值", a(maxRow)(maxCol), maxVal)
        CheckClose("Max 位置", a(maxRow)(maxCol), maxVal)

        Dim minRow As Integer = -1
        Dim minCol As Integer = -1
        Dim minVal As Double = m.Min(minRow, minCol)
        CheckClose("Min 值", a(minRow)(minCol), minVal)

        Dim colMax As Double() = m.max(0).Array
        Dim expectedColMax As Double = Double.MinValue
        For i As Integer = 0 To 12
            expectedColMax = std.Max(expectedColMax, a(i)(5))
        Next
        CheckClose("max(axis=0)", expectedColMax, colMax(5))

        Dim rowMax As Double() = m.max(1).Array
        Dim expectedRowMax As Double = Double.MinValue
        For j As Integer = 0 To 7
            expectedRowMax = std.Max(expectedRowMax, a(6)(j))
        Next
        CheckClose("max(axis=1)", expectedRowMax, rowMax(6))
    End Sub

    Private Sub TestMatrixHelpers()
        Section("矩阵辅助模块")

        Dim a As Double()() = SampleMatrix(7, 5, 71)
        Dim m As New NumericMatrix(a)

        CheckClose("ColumnVector", a(4)(2), m.ColumnVector(2).Array(4))

        ' 矩阵 × 向量（行维度匹配 → 按行缩放）
        Dim v As New Vector(SampleData(7, 72))
        Dim rowScaled As Double()() = m.Multiply(v).ArrayPack(deepcopy:=False)
        CheckClose("RowMultiply（经 Multiply(v)）", a(3)(2) * v.Array(3), rowScaled(3)(2))

        ' 矩阵 × 向量（列维度匹配 → 每行与向量逐元素相乘）
        Dim colV As New Vector(SampleData(5, 73))
        Dim colScaled As Double()() = m.Multiply(colV).ArrayPack(deepcopy:=False)
        CheckClose("ColumnMultiply（经 Multiply(v)）", a(3)(2) * colV.Array(2), colScaled(3)(2))

        Dim centered As Double()() = m.CenterNormalize().ArrayPack(deepcopy:=False)
        Dim rowMean As Double = 0
        For j As Integer = 0 To 4
            rowMean += a(2)(j)
        Next
        rowMean /= 5
        CheckClose("CenterNormalize", a(2)(3) - rowMean, centered(2)(3))

        Dim rowSum As Double = 0
        For j As Integer = 0 To 4
            rowSum += a(1)(j)
        Next
        CheckClose("WiseOperation.Sum", rowSum, m.RowWise().Sum().Array(1))

        ' Matrix * Vector 运算符（历史行为：就地按行缩放左操作数）
        Dim before As Double = a(3)(2)
        Dim mFresh As NumericMatrix = Fresh(a)
        Dim scaled As NumericMatrix = mFresh * v

        CheckClose("Operator *(矩阵, 向量) 就地缩放", before * v.Array(3), mFresh(3, 2))
        Check("Operator *(矩阵, 向量) 返回值尺寸保持", scaled.RowDimension = 7 AndAlso scaled.ColumnDimension = 5)
    End Sub

    Private Sub TestSolversAndDecompositions()
        Section("求解器与分解（回归）")

        Dim a As New NumericMatrix(New Double()() {
            New Double() {2.0, 1.0, -1.0},
            New Double() {-3.0, -1.0, 2.0},
            New Double() {-2.0, 1.0, 2.0}
        })
        Dim b As New Vector({8.0, -11.0, -3.0})
        Dim x As Double() = Solvers.GaussianElimination.Solve(a, b).Array

        CheckClose("GaussianElimination x1 = 2", 2.0, x(0))
        CheckClose("GaussianElimination x2 = 3", 3.0, x(1))
        CheckClose("GaussianElimination x3 = -1", -1.0, x(2))

        Dim spd As New NumericMatrix(New Double()() {
            New Double() {4.0, 12.0, -16.0},
            New Double() {12.0, 37.0, -43.0},
            New Double() {-16.0, -43.0, 98.0}
        })
        Dim rhs As NumericMatrix = New NumericMatrix(New Double() {1.0, 2.0, 3.0})

        Check("Cholesky SPD", spd.chol().SPD)

        ' ==== 分解求解器的等价性验证 ====
        ' 说明：CholeskyDecomposition.Solve 的前代实现会先用「未归一化的主元」更新后续行、
        ' 再做归一化，与标准算法不一致（属于本次重构之前就存在的缺陷）。
        ' 本次改造的原则是不改变可观察行为，因此这里用「复刻原循环顺序的标量参考实现」
        ' 做等价性对拍，而不是与 LU 的解互相对照。
        Dim chol As CholeskyDecomposition = spd.chol()
        Dim cholL As Double()() = chol.GetL().ArrayPack(deepcopy:=False)
        Dim cholActual As Double() = chol.Solve(rhs).ColumnVector(0).Array
        Dim cholExpected As Double() = ScalarCholeskySolve(cholL, New Double() {1.0, 2.0, 3.0})

        CheckClose("Cholesky Solve 与原标量实现等价 (0)", cholExpected(0), cholActual(0))
        CheckClose("Cholesky Solve 与原标量实现等价 (1)", cholExpected(1), cholActual(1))
        CheckClose("Cholesky Solve 与原标量实现等价 (2)", cholExpected(2), cholActual(2))

        ' LU 的前代/回代在本次改造中换成了 AXPY 内核：用「解必须满足原方程」来校验数学正确性
        Dim luActual As Double() = spd.LUD().Solve(rhs).ColumnVector(0).Array

        CheckClose("LU Solve 满足 A x = b (row1)", 1.0, spd(0, 0) * luActual(0) + spd(0, 1) * luActual(1) + spd(0, 2) * luActual(2))
        CheckClose("LU Solve 满足 A x = b (row2)", 2.0, spd(1, 0) * luActual(0) + spd(1, 1) * luActual(1) + spd(1, 2) * luActual(2))
        CheckClose("LU Solve 满足 A x = b (row3)", 3.0, spd(2, 0) * luActual(0) + spd(2, 1) * luActual(1) + spd(2, 2) * luActual(2))

        Dim inv As Double()() = spd.Inverse().ArrayPack(deepcopy:=False)
        Dim identity As Double()() = spd.Multiply(New NumericMatrix(inv)).ArrayPack(deepcopy:=False)

        CheckClose("A * A^-1 = I (0,0)", 1.0, identity(0)(0))
        CheckClose("A * A^-1 = I (0,1)", 0.0, identity(0)(1))
        CheckClose("A * A^-1 = I (1,2)", 0.0, identity(1)(2))
        CheckClose("Determinant", 36.0, spd.Determinant())

        ' 线性方程组 Solve（LU 路径，方阵）
        Dim solveX As Double() = spd.Solve(New Double() {1.0, 2.0, 3.0})
        Dim residual As Double = spd(0, 0) * solveX(0) + spd(0, 1) * solveX(1) + spd(0, 2) * solveX(2)

        CheckClose("Solve 回代校验", 1.0, residual)
    End Sub

    ''' <summary>
    ''' 复刻 <c>CholeskyDecomposition.Solve</c> 原始的循环顺序（前代 + 回代）。
    ''' </summary>
    ''' <remarks>
    ''' 只用于「重构前后行为等价」的对拍：原实现在前代的同一轮里先用未归一化的
    ''' <c>X(k)</c> 更新后续行、之后再归一化 <c>X(k)</c>，与标准前代算法不同。
    ''' </remarks>
    Private Function ScalarCholeskySolve(L As Double()(), b As Double()) As Double()
        Dim n As Integer = b.Length
        Dim X As Double()() = New Double(n - 1)() {}

        For i As Integer = 0 To n - 1
            X(i) = New Double() {b(i)}
        Next

        For k As Integer = 0 To n - 1
            For i As Integer = k + 1 To n - 1
                X(i)(0) -= X(k)(0) * L(i)(k)
            Next

            X(k)(0) /= L(k)(k)
        Next

        For k As Integer = n - 1 To 0 Step -1
            X(k)(0) /= L(k)(k)

            For i As Integer = 0 To k - 1
                X(i)(0) -= X(k)(0) * L(k)(i)
            Next
        Next

        Dim out As Double() = New Double(n - 1) {}

        For i As Integer = 0 To n - 1
            out(i) = X(i)(0)
        Next

        Return out
    End Function

    Private Sub TestScalarFallback()
        Section("SIMDConfiguration.disable 标量回退")

        Dim original As SIMDConfiguration = SIMDEnvironment.config

        Try
            SIMDEnvironment.config = SIMDConfiguration.disable

            Check("IsEnabled = False", Not SIMDEnvironment.IsEnabled)
            Check("CanVectorize = False", Not SimdEngine.CanVectorize(Of Double)(10000))

            Dim a As Double() = SampleData(1001, 91)
            Dim b As Double() = SampleData(1001, 92)

            Check("标量回退：向量加法", ArraysEqual(SimdEngine.Add(Of Double)(a, b), ScalarAdd(a, b), 0.000000001))
            CheckClose("标量回退：点积", ScalarDot(a, b), SimdParallel.Dot(a, b))

            Dim ma As New NumericMatrix(SampleMatrix(11, 9, 93))
            Dim mb As New NumericMatrix(SampleMatrix(11, 9, 94))
            Dim expected As Double()() = MatrixBinary(ma.ArrayPack(deepcopy:=False), mb.ArrayPack(deepcopy:=False), Function(x, y) x + y)
            Check("标量回退：矩阵加法", MatrixEquals((ma + mb).ArrayPack(deepcopy:=False), expected, 0.000000001))

            Check("标量回退：空数组", SimdEngine.Add(Of Double)(New Double() {}, New Double() {}).Length = 0)
            Check("标量回退：长度 1", SimdEngine.Add(Of Double)(New Double() {1.0}, New Double() {2.0})(0) = 3.0)
            Check("标量回退：长度 3", SimdEngine.Add(Of Double)(New Double() {1.0, 2.0, 3.0}, New Double() {1.0, 1.0, 1.0})(2) = 4.0)
        Finally
            SIMDEnvironment.config = original
        End Try

        Check("恢复 SIMD 配置", SIMDEnvironment.config = original)
    End Sub

#End Region

End Module
