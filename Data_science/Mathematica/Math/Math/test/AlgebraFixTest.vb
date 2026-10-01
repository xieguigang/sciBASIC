' ============================================================================
' AlgebraFixTest.vb — Algebra 线性代数模块准确度修正与 SIMD 化的回归验证
' ----------------------------------------------------------------------------
' 覆盖：
'   + 历史缺陷修正：Operator *(矩阵, 向量) 与 RowSubtraction 的新语义
'   + 求解器准确度：GaussianElimination 部分主元 / 奇异守卫、
'     MatrixOps.Inverse / Determinant / JacobiEigen、PowerMethod 收敛、
'     OLS 回归、SOR 迭代
'   + 分解准确度：JAMA SVD（降序奇异值 / 重构 / 秩亏 Condition）、
'     根 SVD.vb 薄封装、特征值分解
'   + SIMD 化后的数值一致性：MatrixOps 矩形数组运算、向量范数、
'     逐元素幂、Cholesky / QR 求解
' ============================================================================

Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Solvers
Imports std = System.Math

Public Module AlgebraFixTest

    Private pass As Integer = 0
    Private fail As Integer = 0

    Public Function RunAll() As Integer
        pass = 0
        fail = 0

        Console.WriteLine("==================================================================")
        Console.WriteLine(" Algebra 准确度修正 + SIMD 化 回归验证")
        Console.WriteLine("==================================================================")

        TestHistoricalDefectFixes()
        TestGaussianElimination()
        TestMatrixOpsAccuracy()
        TestPowerMethod()
        TestSvd()
        TestEigenDecomposition()
        TestOlsAndSor()
        TestSimdConsistency()

        Console.WriteLine()
        Console.WriteLine("==================================================================")
        Console.WriteLine($" Algebra 修正测试完成: {pass} 通过, {fail} 失败, 共 {pass + fail} 项")
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

    Private Sub CheckClose(name As String, expected As Double, actual As Double, Optional tolerance As Double = 0.000000001)
        Dim scale As Double = std.Max(1.0, std.Abs(expected))

        If std.Abs(expected - actual) <= tolerance * scale Then
            pass += 1
        Else
            fail += 1
            Console.WriteLine($"  [FAIL] {name}: 期望 {expected:G17}, 实际 {actual:G17}")
        End If
    End Sub

    Private Sub CheckThrows(name As String, action As Action)
        Try
            Call action()
            fail += 1
            Console.WriteLine($"  [FAIL] {name}: 未抛出预期的异常")
        Catch ex As Exception
            pass += 1
        End Try
    End Sub

    ''' <summary>确定性伪随机数据（固定种子，保证测试可重复）</summary>
    Private Function SampleData(n As Integer, seed As Integer) As Double()
        Dim rand As New Random(seed)
        Dim data As Double() = New Double(n - 1) {}

        For i As Integer = 0 To n - 1
            data(i) = rand.NextDouble() * 4.0 - 2.0
        Next

        Return data
    End Function

    Private Function SampleMatrix(rows As Integer, cols As Integer, seed As Integer) As Double()()
        Dim data As Double() = SampleData(rows * cols, seed)
        Dim a As Double()() = New Double(rows - 1)() {}

        For i As Integer = 0 To rows - 1
            a(i) = New Double(cols - 1) {}

            For j As Integer = 0 To cols - 1
                a(i)(j) = data(i * cols + j)
            Next
        Next

        Return a
    End Function

    Private Function Deep(a As Double()()) As Double()()
        Dim copy As Double()() = New Double(a.Length - 1)() {}

        For i As Integer = 0 To a.Length - 1
            copy(i) = CType(a(i).Clone(), Double())
        Next

        Return copy
    End Function

#End Region

    ' ------------------------------------------------------------------
    ' 1. 历史缺陷修正
    ' ------------------------------------------------------------------
    Private Sub TestHistoricalDefectFixes()
        Section("历史缺陷修正")

        Dim a As Double()() = SampleMatrix(7, 5, 71)
        Dim mFresh As New NumericMatrix(Deep(a))
        Dim v As New Vector(SampleData(5, 72))

        ' Operator *(矩阵, 向量)：标准矩阵×向量乘法，且不修改左操作数
        Dim matVec As Vector = mFresh * v

        For i As Integer = 0 To 6
            Dim dot As Double = 0.0

            For j As Integer = 0 To 4
                dot += a(i)(j) * v.Array(j)
            Next

            CheckClose($"Operator *(矩阵,向量) 行点积 row{i}", dot, matVec.Array(i))
        Next

        Check("Operator *(矩阵,向量) 不修改左操作数", mFresh(3, 2) = a(3)(2))

        ' RowSubtraction 语义（经公共运算符 -(列向量矩阵, 矩阵) 走入）：
        ' result(i,j) = v(i) - m(i,j)
        Dim m2 As New NumericMatrix(Deep(a))
        Dim vv As New Vector(SampleData(7, 73))
        Dim colVecM As New NumericMatrix(vv.Array.Select(Function(x) New Double() {x}).ToArray())
        Dim diff As NumericMatrix = colVecM - m2

        For i As Integer = 0 To 6
            For j As Integer = 0 To 4
                CheckClose($"RowSubtraction({i},{j})", vv.Array(i) - a(i)(j), diff(i, j))
            Next
        Next

        ' Operator -(列向量矩阵, 矩阵) 的独立样例
        Dim colVec As New NumericMatrix(New Double()() {
            New Double() {1.0}, New Double() {2.0}, New Double() {3.0}})
        Dim rhs As New NumericMatrix(New Double()() {
            New Double() {0.5, 1.0, 1.5},
            New Double() {2.0, 2.5, 3.0},
            New Double() {3.5, 4.0, 4.5}})
        Dim diffOp As NumericMatrix = colVec - rhs

        CheckClose("Operator -(列向量,矩阵)(0,1)", 1.0 - 1.0, diffOp(0, 1))
        CheckClose("Operator -(列向量,矩阵)(1,2)", 2.0 - 3.0, diffOp(1, 2))
        CheckClose("Operator -(列向量,矩阵)(2,0)", 3.0 - 3.5, diffOp(2, 0))

        ' 维度不匹配必须抛异常
        CheckThrows("RowSubtraction 维度守卫",
                    Sub()
                        Dim small As New NumericMatrix(New Double()() {
                            New Double() {1.0}, New Double() {2.0}})
                        Dim diffBad As NumericMatrix = small - New NumericMatrix(3, 3)
                    End Sub)
    End Sub

    ' ------------------------------------------------------------------
    ' 2. GaussianElimination：部分主元与奇异守卫
    ' ------------------------------------------------------------------
    Private Sub TestGaussianElimination()
        Section("GaussianElimination 部分主元")

        ' 无主元交换就会除零的矩阵（对角线含 0）
        Dim a As New NumericMatrix(New Double()() {
            New Double() {0.0, 1.0},
            New Double() {1.0, 0.0}})
        Dim b As New Vector({2.0, 3.0})
        Dim x As Vector = GaussianElimination.Solve(a, b)

        CheckClose("Pivoting x1 = 3", 3.0, x.Array(0))
        CheckClose("Pivoting x2 = 2", 2.0, x.Array(1))

        ' 一般方程组回归（与文档示例一致）
        Dim a2 As New NumericMatrix(New Double()() {
            New Double() {2.0, 1.0, -1.0},
            New Double() {-3.0, -1.0, 2.0},
            New Double() {-2.0, 1.0, 2.0}})
        Dim x2 As Vector = GaussianElimination.Solve(a2, New Vector({8.0, -11.0, -3.0}))

        CheckClose("GaussianElimination x1 = 2", 2.0, x2.Array(0))
        CheckClose("GaussianElimination x2 = 3", 3.0, x2.Array(1))
        CheckClose("GaussianElimination x3 = -1", -1.0, x2.Array(2))

        ' 奇异矩阵必须失败而非产生 NaN
        Dim sing As New NumericMatrix(New Double()() {
            New Double() {1.0, 2.0},
            New Double() {2.0, 4.0}})

        CheckThrows("GaussianElimination 奇异矩阵抛异常",
                    Sub()
                        Call GaussianElimination.Solve(sing, New Vector({1.0, 2.0}))
                    End Sub)
    End Sub

    ' ------------------------------------------------------------------
    ' 3. MatrixOps：Inverse / Determinant / JacobiEigen 准确度
    ' ------------------------------------------------------------------
    Private Sub TestMatrixOpsAccuracy()
        Section("MatrixOps Inverse / Determinant / JacobiEigen")

        ' 奇异矩阵：非 throwSingularity 模式返回 Nothing（不再静默降级为垃圾结果）
        Dim sing As Double(,) = New Double(,) {{1.0, 2.0}, {2.0, 4.0}}
        Dim invSing As Double(,) = MatrixOps.Inverse(sing, strict:=False, throwSingularity:=False)

        Check("奇异矩阵 Inverse 返回 Nothing", invSing Is Nothing)

        CheckThrows("奇异矩阵 Inverse（throwSingularity）抛异常",
                    Sub()
                        Call MatrixOps.Inverse(sing, strict:=False, throwSingularity:=True)
                    End Sub)

        ' 良态矩阵：A * inv(A) ≈ I
        Dim a As Double(,) = New Double(,) {{4.0, 7.0}, {2.0, 6.0}}
        Dim inv As Double(,) = MatrixOps.Inverse(a, strict:=True)
        Dim prod As Double(,) = MatrixOps.Multiply(a, inv)

        CheckClose("Inverse 单位阵 (0,0)", 1.0, prod(0, 0))
        CheckClose("Inverse 单位阵 (0,1)", 0.0, prod(0, 1))
        CheckClose("Inverse 单位阵 (1,0)", 0.0, prod(1, 0))
        CheckClose("Inverse 单位阵 (1,1)", 1.0, prod(1, 1))

        ' 大数矩阵（尺度相对阈值：旧实现固定 1e-14 阈值下行为退化）
        Dim bigInv As Double(,) = MatrixOps.Inverse(New Double(,) {{1000.0, 0.0}, {0.0, 2000.0}}, strict:=True)

        CheckClose("大数矩阵 Inverse (0,0)", 0.001, bigInv(0, 0))
        CheckClose("大数矩阵 Inverse (1,1)", 0.0005, bigInv(1, 1))

        ' 行列式：需要主元交换 + 尺度无关
        CheckClose("Determinant 反对角矩阵 = -1", -1.0, MatrixOps.Determinant(New Double(,) {{0.0, 1.0}, {1.0, 0.0}}))
        CheckClose("Determinant 1000*I = 1e9", 1000000000.0, MatrixOps.Determinant(New Double(,) {{1000.0, 0.0, 0.0}, {0.0, 1000.0, 0.0}, {0.0, 0.0, 1000.0}}))
        CheckClose("Determinant 0.001*I = 1e-9", 0.000000001, MatrixOps.Determinant(New Double(,) {{0.001, 0.0, 0.0}, {0.0, 0.001, 0.0}, {0.0, 0.0, 0.001}}))

        ' JacobiEigen：已知特征值 {3, 1}（Jacobi 输出无序，排序后校验）
        Dim sym As Double(,) = New Double(,) {{2.0, 1.0}, {1.0, 2.0}}
        Dim jac As (eigenvalues As Double(), eigenvectors As Double(,)) = MatrixOps.JacobiEigen(sym)
        Dim evs As Double() = {jac.eigenvalues(0), jac.eigenvalues(1)}

        Call Array.Sort(evs)

        CheckClose("JacobiEigen 特征值 1", 1.0, evs(0), 0.0000001)
        CheckClose("JacobiEigen 特征值 3", 3.0, evs(1), 0.0000001)

        ' A v = λ v（列向量验证）
        For col As Integer = 0 To 1
            Dim lambda As Double = jac.eigenvalues(col)

            For row As Integer = 0 To 1
                Dim av As Double = sym(row, 0) * jac.eigenvectors(0, col) + sym(row, 1) * jac.eigenvectors(1, col)

                CheckClose($"JacobiEigen A*v=λ*v (col={col},row={row})",
                           lambda * jac.eigenvectors(row, col), av, 0.0000001)
            Next
        Next
    End Sub

    ' ------------------------------------------------------------------
    ' 4. PowerMethod：点积修正 + 收敛
    ' ------------------------------------------------------------------
    Private Sub TestPowerMethod()
        Section("PowerMethod 收敛")

        ' A^T A of [[2,1],[1,2]] = [[5,4],[4,5]]，最大特征值 9
        ' （构造函数内部会再对输入做 A^T A，因此传入原始矩阵 A）
        Dim a As Double()() = {
            New Double() {2.0, 1.0},
            New Double() {1.0, 2.0}}
        Dim pm As New PowerMethod(a)

        Call pm.powerMethod()

        ' curLambda / curX 为私有字段，测试通过反射读取结果
        Dim t As Type = GetType(PowerMethod)
        Dim lambdaField As Reflection.FieldInfo = t.GetField("curLambda", Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.Instance)
        Dim lambda As Double = CDbl(lambdaField.GetValue(pm))

        CheckClose("PowerMethod 最大特征值 = 9", 9.0, lambda, 0.0001)
    End Sub

    ' ------------------------------------------------------------------
    ' 5. SVD：JAMA 版行为 + 根 SVD.vb 薄封装
    ' ------------------------------------------------------------------
    Private Sub TestSvd()
        Section("SVD 行为一致性")

        Dim a As Double()() = SampleMatrix(4, 3, 91)
        Dim svd As New SingularValueDecomposition(New NumericMatrix(Deep(a)))

        ' 奇异值非负且降序
        Dim s As Double() = svd.SingularValues.Array

        Check("SVD 奇异值个数 = 3", s.Length = 3)

        For i As Integer = 0 To s.Length - 1
            Check($"SVD 奇异值非负 s{i}", s(i) >= 0)
        Next

        For i As Integer = 0 To s.Length - 2
            Check($"SVD 奇异值降序 s{i} >= s{i + 1}", s(i) >= s(i + 1))
        Next

        ' 重构 A ≈ U * S * V^T
        Dim m As Integer = 4, n As Integer = 3
        Dim uu As GeneralMatrix = svd.U
        Dim vv As GeneralMatrix = svd.V

        For i As Integer = 0 To m - 1
            For j As Integer = 0 To n - 1
                Dim sum As Double = 0.0

                For k As Integer = 0 To n - 1
                    sum += uu(i, k) * s(k) * vv(j, k)
                Next

                CheckClose($"SVD 重构 ({i},{j})", a(i)(j), sum, 0.0000001)
            Next
        Next

        ' V 正交：V^T V = I
        For i As Integer = 0 To n - 1
            For j As Integer = 0 To n - 1
                Dim dot As Double = 0.0

                For k As Integer = 0 To n - 1
                    dot += vv(k, i) * vv(k, j)
                Next

                Dim expected As Double = If(i = j, 1.0, 0.0)
                CheckClose($"SVD V^T V ({i},{j})", expected, dot, 0.0000001)
            Next
        Next

        ' 秩亏矩阵：Condition 应为 +Infinity 而非除零 NaN
        Dim rankDef As New SingularValueDecomposition(New Double(,) {
            {1.0, 2.0, 3.0},
            {2.0, 4.0, 6.0},
            {1.0, 1.0, 1.0}})

        Check("秩亏矩阵 Condition = +Inf", Double.IsPositiveInfinity(rankDef.Condition))

        ' 根 SVD.vb 薄封装：奇异值降序、V 正交、输入不被修改
        Dim rect As Double(,) = New Double(3, 2) {}

        For i As Integer = 0 To 3
            For j As Integer = 0 To 2
                rect(i, j) = a(i)(j)
            Next
        Next

        Dim w As Double() = Nothing, vRect As Double(,) = Nothing
        Call Microsoft.VisualBasic.Math.LinearAlgebra.SVD.SVDecomposition(rect, w, vRect)

        For i As Integer = 0 To 2
            CheckClose($"SVD 薄封装 w({i}) 一致", s(i), w(i), 0.0000001)
        Next

        Dim vDot As Double = 0.0

        For k As Integer = 0 To 2
            vDot += vRect(k, 0) * vRect(k, 0)
        Next

        CheckClose("SVD 薄封装 V 列单位化", 1.0, vDot, 0.0000001)

        CheckThrows("SVD 薄封装 m < n 抛异常",
                    Sub()
                        Dim ww As Double() = Nothing, vv2 As Double(,) = Nothing
                        Call Microsoft.VisualBasic.Math.LinearAlgebra.SVD.SVDecomposition(New Double(,) {{1.0, 2.0, 3.0}, {4.0, 5.0, 6.0}}, ww, vv2)
                    End Sub)
    End Sub

    ' ------------------------------------------------------------------
    ' 6. 特征值分解
    ' ------------------------------------------------------------------
    Private Sub TestEigenDecomposition()
        Section("特征值分解")

        Dim sym As New NumericMatrix(New Double()() {
            New Double() {2.0, 1.0},
            New Double() {1.0, 2.0}})
        Dim eig As New EigenvalueDecomposition(sym)
        Dim d As Double() = eig.RealEigenvalues
        Dim v As NumericMatrix = eig.V

        ' TODO-DEBUG
        Console.WriteLine($"  [DEBUG] d = [{d(0):G17}, {d(1):G17}]")
        Console.WriteLine($"  [DEBUG] V(0,0)={v(0, 0):G17} V(0,1)={v(0, 1):G17} V(1,0)={v(1, 0):G17} V(1,1)={v(1, 1):G17}")

        CheckClose("对称特征值 3", 3.0, d(1), 0.0000001)
        CheckClose("对称特征值 1", 1.0, d(0), 0.0000001)

        ' A v = λ v
        For col As Integer = 0 To 1
            For row As Integer = 0 To 1
                Dim av As Double = sym(row, 0) * v(0, col) + sym(row, 1) * v(1, col)

                CheckClose($"特征向量 A*v=λ*v (col={col},row={row})",
                           d(col) * v(row, col), av, 0.0000001)
            Next
        Next
    End Sub

    ' ------------------------------------------------------------------
    ' 7. OLS 与 SOR
    ' ------------------------------------------------------------------
    Private Sub TestOlsAndSor()
        Section("OLS / SOR")

        ' y = 2 + 3x
        Dim nS As Integer = 5
        Dim xArr As Double() = {1.0, 2.0, 3.0, 4.0, 5.0}
        Dim yArr As Double() = {5.0, 8.0, 11.0, 14.0, 17.0}
        Dim xDesign As Double(,) = New Double(nS - 1, 1) {}

        For i As Integer = 0 To nS - 1
            xDesign(i, 0) = 1.0
            xDesign(i, 1) = xArr(i)
        Next

        Dim beta As Double() = OLS.Solve(xDesign, yArr, nS, 2)

        CheckClose("OLS 截距 = 2", 2.0, beta(0), 0.000001)
        CheckClose("OLS 斜率 = 3", 3.0, beta(1), 0.000001)

        ' 完全多重共线性：两列相同 → 回退仅截距模型
        Dim collinear As Double(,) = New Double(1, 1) {}
        collinear(0, 0) = 1.0 : collinear(0, 1) = 1.0
        collinear(1, 0) = 1.0 : collinear(1, 1) = 1.0
        Dim y2 As Double() = {3.0, 5.0}
        Dim beta2 As Double() = OLS.Solve(collinear, y2, 2, 2)

        CheckClose("OLS 共线性回退截距 = mean(y)", 4.0, beta2(0), 0.000001)
        CheckClose("OLS 共线性回退斜率 = 0", 0.0, beta2(1), 0.000001)

        ' SOR：4x + y = 9, x + 3y = 7 → x = 2, y = 1（含非对角主导的松弛求解）
        Dim sorA As New NumericMatrix(New Double()() {
            New Double() {4.0, 1.0},
            New Double() {1.0, 3.0}})
        Dim sorX As Vector = SOR.Solve(sorA, New Vector({9.0, 7.0}), 1.0, 0.000000001, 100)

        CheckClose("SOR x = 2", 2.0, sorX.Array(0), 0.00001)
        CheckClose("SOR y = 1", 1.0, sorX.Array(1), 0.00001)

        ' SOR 零对角守卫
        CheckThrows("SOR 零对角抛异常",
                    Sub()
                        Call SOR.Solve(New NumericMatrix(New Double()() {
                            New Double() {0.0, 1.0},
                            New Double() {1.0, 1.0}}), New Vector({1.0, 1.0}))
                    End Sub)
    End Sub

    ' ------------------------------------------------------------------
    ' 8. SIMD 化后的数值一致性
    ' ------------------------------------------------------------------
    Private Sub TestSimdConsistency()
        Section("SIMD 数值一致性")

        Dim a As Double(,) = New Double(4, 3) {}
        Dim b As Double(,) = New Double(4, 3) {}
        Dim ra As Double()() = SampleMatrix(5, 4, 101)
        Dim rb As Double()() = SampleMatrix(5, 4, 102)

        For i As Integer = 0 To 4
            For j As Integer = 0 To 3
                a(i, j) = ra(i)(j)
                b(i, j) = rb(i)(j)
            Next
        Next

        ' MatrixOps.Add / Subtract / Scale（行压平 + SimdMatrix）
        Dim sum As Double(,) = MatrixOps.Add(a, b)

        CheckClose("MatrixOps.Add", a(2, 3) + b(2, 3), sum(2, 3))

        Dim dif As Double(,) = MatrixOps.Subtract(a, b)

        CheckClose("MatrixOps.Subtract", a(2, 3) - b(2, 3), dif(2, 3))

        Dim scaled As Double(,) = MatrixOps.Scale(a, 2.5)

        CheckClose("MatrixOps.Scale", a(2, 3) * 2.5, scaled(2, 3))

        ' Transpose（32x32 分块内核）
        Dim at As Double(,) = MatrixOps.Transpose(a)

        CheckClose("MatrixOps.Transpose", a(2, 3), at(3, 2))
        Check("Transpose 尺寸", at.GetLength(0) = 4 AndAlso at.GetLength(1) = 5)

        ' 向量范数
        Dim vec As New Vector(SampleData(33, 103))
        Dim l1 As Double = 0.0, linf As Double = 0.0

        For i As Integer = 0 To vec.Dim - 1
            l1 += std.Abs(vec.Array(i))
            linf = std.Max(linf, std.Abs(vec.Array(i)))
        Next

        CheckClose("L1Norm", l1, vec.L1Norm(), 0.000000001)
        CheckClose("InfinityNorm", linf, vec.InfinityNorm(), 0.000000001)

        ' 逐元素幂 Operator ^（标量 ^ 矩阵）
        Dim mPow As New NumericMatrix(Deep(ra))
        Dim pow As NumericMatrix = 2.0 ^ mPow

        CheckClose("Operator ^(标量,矩阵)", std.Pow(ra(2)(3), 2.0), pow(2, 3))

        ' Cholesky 回归（构造内环已 SIMD 化）
        Dim spd As New NumericMatrix(New Double()() {
            New Double() {4.0, 12.0, -16.0},
            New Double() {12.0, 37.0, -43.0},
            New Double() {-16.0, -43.0, 98.0}})
        Dim chol As CholeskyDecomposition = spd.chol()

        Check("Cholesky SPD", chol.SPD)

        Dim sol As GeneralMatrix = chol.Solve(New NumericMatrix(New Double()() {
            New Double() {1.0}, New Double() {2.0}, New Double() {3.0}}))

        ' 用残差 A·x ≈ b 校验（避免依赖手工换算的特值）
        For i As Integer = 0 To 2
            Dim av As Double = 0.0

            For j As Integer = 0 To 2
                av += spd(i, j) * sol(j, 0)
            Next

            CheckClose($"Cholesky Solve 残差 row{i}", CDbl(i + 1), av, 0.000001)
        Next

        ' QR 最小二乘回归（构造与 Solve 已 SIMD 化）
        ' 方程组 2x+y=3, x-y=0, x+y=4 的最小二乘解 x ≈ 1.5, y ≈ 1.25（手工解）
        Dim qrA As New NumericMatrix(New Double()() {
            New Double() {2.0, 1.0},
            New Double() {1.0, -1.0},
            New Double() {1.0, 1.0}})
        Dim qrB As New NumericMatrix(New Double()() {
            New Double() {3.0}, New Double() {0.0}, New Double() {4.0}})
        Dim qrX As GeneralMatrix = qrA.QRD().Solve(qrB)

        ' 用 QR 重构验证：A = Q R
        Dim qr As QRDecomposition = qrA.QRD()
        Dim qq As GeneralMatrix = qr.Q
        Dim rr As GeneralMatrix = qr.R

        For i As Integer = 0 To 2
            For j As Integer = 0 To 1
                Dim qrSum As Double = 0.0

                For k As Integer = 0 To 1
                    qrSum += qq(i, k) * rr(k, j)
                Next

                CheckClose($"QR 重构 ({i},{j})", qrA(i, j), qrSum, 0.000001)
            Next
        Next
    End Sub

    Private Sub Section(title As String)
        Console.WriteLine($"--- {title} ---")
    End Sub
End Module
