#Region "Microsoft.VisualBasic::1355c5e0c1224aed4b4173320c4db7eb, Data_science\Mathematica\Math\Math\Algebra\Matrix.NET\Extensions\MatrixOps.vb"

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

    '   Total Lines: 434
    '    Code Lines: 309 (71.20%)
    ' Comment Lines: 68 (15.67%)
    '    - Xml Docs: 72.06%
    ' 
    '   Blank Lines: 57 (13.13%)
    '     File Size: 16.86 KB


    '     Module MatrixOps
    ' 
    '         Function: Add, Determinant, Inverse, JacobiEigen, JaggedToRect
    '                   Multiply, MultiplyVec, RectToJagged, Scale, Subtract
    '                   Trace, Transpose
    ' 
    '         Sub: PrintMatrix
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports SimdMatrix = Microsoft.VisualBasic.Math.SIMD.SimdMatrix
Imports SIMDIntrinsics = Microsoft.VisualBasic.Math.SIMD.SIMDIntrinsics
Imports stdf = System.Math

Namespace LinearAlgebra.Matrix

    ''' <summary>
    ''' 矩阵运算模块 - 使用 VB.NET 基础数学函数实现线性代数运算
    ''' 包含：矩阵乘法、转置、求逆、行列式、特征值分解等
    ''' </summary>
    Public Module MatrixOps

        ''' <summary>矩阵乘法 C = A × B</summary>
        ''' <remarks>
        ''' 先把两个矩形数组压平成 jagged 的连续行（这是走向量化点积的必要前提），
        ''' 再交给「右矩阵预转置 + 行方向并行 + 行内 FMA 点积」的内核；
        ''' O(n³) 的计算量足以摊薄这次 O(n²) 的拷贝开销。
        ''' </remarks>
        Public Function Multiply(a As Double(,), b As Double(,)) As Double(,)
            Dim rowsA = a.GetLength(0)
            Dim colsA = a.GetLength(1)
            Dim colsB = b.GetLength(1)

            Dim ja As Double()() = New Double(rowsA - 1)() {}
            For i = 0 To rowsA - 1
                Dim row As Double() = New Double(colsA - 1) {}
                For k = 0 To colsA - 1
                    row(k) = a(i, k)
                Next
                ja(i) = row
            Next

            Dim jb As Double()() = New Double(colsA - 1)() {}
            For k = 0 To colsA - 1
                Dim row As Double() = New Double(colsB - 1) {}
                For j = 0 To colsB - 1
                    row(j) = b(k, j)
                Next
                jb(k) = row
            Next

            Dim jc As Double()() = SimdMatrix.Dot(ja, jb)
            Dim result(rowsA - 1, colsB - 1) As Double

            For i = 0 To rowsA - 1
                Dim row As Double() = jc(i)
                For j = 0 To colsB - 1
                    result(i, j) = row(j)
                Next
            Next

            Return result
        End Function

        ''' <summary>
        ''' 矩阵乘法 <c>C = A × B</c>，结果写进调用方提供的缓冲（不再产生任何分配）。
        ''' </summary>
        ''' <param name="a">左矩阵 A，形状 <c>[rows, K]</c></param>
        ''' <param name="b">右矩阵 B，形状 <c>[K, cols]</c></param>
        ''' <param name="dest">目标缓冲，形状 <c>[rows, cols]</c>；内部会先清零</param>
        ''' <remarks>
        ''' <para>
        ''' 训练热路径（ANFIS 每个行块都要算一次 <c>f = X·P'</c>）需要反复调用乘法，
        ''' 而 <see cref="Multiply(Double(,), Double(,))"/> 每次都要把 A、B 各压平成 jagged
        ''' （A 有 rows 个行数组），在 512 行 × 3 万基因的规模下这是海量小对象分配。
        ''' </para>
        ''' <para>
        ''' 这里用 <c>k</c> 在最外层的行主序三重循环：内层对 <c>dest</c> 的写是连续的，
        ''' 且 <c>a(i,k)</c> 与 <c>b(k,j)</c> 都能被寄存器/预取友好地访问。
        ''' 累加顺序与 SIMD 的 FMA 点积不同，末位可能相差 1 ULP —— 调用方（ANFIS 训练）
        ''' 已确认可以接受。
        ''' </para>
        ''' </remarks>
        Public Sub Multiply(a As Double(,), b As Double(,), dest As Double(,))
            If a Is Nothing OrElse b Is Nothing OrElse dest Is Nothing Then
                Throw New ArgumentNullException(NameOf(a))
            End If

            Dim rows As Integer = a.GetLength(0)
            Dim kCount As Integer = a.GetLength(1)
            Dim cols As Integer = b.GetLength(1)

            If b.GetLength(0) <> kCount Then
                Throw New ArgumentException($"the inner dimensions do not match: {rows}x{kCount} * {b.GetLength(0)}x{cols}", NameOf(b))
            End If
            If dest.GetLength(0) <> rows OrElse dest.GetLength(1) <> cols Then
                Throw New ArgumentException($"the destination must be {rows}x{cols}", NameOf(dest))
            End If

            Call Array.Clear(dest, 0, dest.Length)

            For i As Integer = 0 To rows - 1
                For k As Integer = 0 To kCount - 1
                    Dim aik As Double = a(i, k)

                    If aik = 0.0 Then
                        Continue For
                    End If

                    For j As Integer = 0 To cols - 1
                        dest(i, j) += aik * b(k, j)
                    Next
                Next
            Next
        End Sub

        ''' <summary>矩形数组压平为 jagged 连续行（SIMD 内核的必要前提）</summary>
        Private Function RectToJagged(a As Double(,)) As Double()()
            Dim rows = a.GetLength(0)
            Dim cols = a.GetLength(1)
            Dim jagged(rows - 1)() As Double

            For i = 0 To rows - 1
                Dim row As Double() = New Double(cols - 1) {}
                For j = 0 To cols - 1
                    row(j) = a(i, j)
                Next
                jagged(i) = row
            Next

            Return jagged
        End Function

        ''' <summary>jagged 连续行还原为矩形数组</summary>
        Private Function JaggedToRect(jagged As Double()(), rows As Integer, cols As Integer) As Double(,)
            Dim result(rows - 1, cols - 1) As Double

            For i = 0 To rows - 1
                Dim row As Double() = jagged(i)
                For j = 0 To cols - 1
                    result(i, j) = row(j)
                Next
            Next

            Return result
        End Function

        ''' <summary>矩阵转置</summary>
        ''' <remarks>SIMD 化：交给 32x32 分块的缓存友好转置内核</remarks>
        Public Function Transpose(a As Double(,)) As Double(,)
            Dim rows = a.GetLength(0)
            Dim cols = a.GetLength(1)
            Dim jt As Double()() = SimdMatrix.Transpose(RectToJagged(a))
            Return JaggedToRect(jt, cols, rows)
        End Function

        ''' <summary>矩阵加法</summary>
        ''' <remarks>SIMD 化：行压平后走逐元素向量加法内核</remarks>
        Public Function Add(a As Double(,), b As Double(,)) As Double(,)
            Dim rows = a.GetLength(0)
            Dim cols = a.GetLength(1)
            Dim jc As Double()() = SimdMatrix.Add(RectToJagged(a), RectToJagged(b))
            Return JaggedToRect(jc, rows, cols)
        End Function

        ''' <summary>矩阵减法</summary>
        ''' <remarks>SIMD 化：行压平后走逐元素向量减法内核</remarks>
        Public Function Subtract(a As Double(,), b As Double(,)) As Double(,)
            Dim rows = a.GetLength(0)
            Dim cols = a.GetLength(1)
            Dim jc As Double()() = SimdMatrix.Subtract(RectToJagged(a), RectToJagged(b))
            Return JaggedToRect(jc, rows, cols)
        End Function

        ''' <summary>标量乘矩阵</summary>
        ''' <remarks>SIMD 化：行压平后走向量数乘内核</remarks>
        Public Function Scale(a As Double(,), s As Double) As Double(,)
            Dim rows = a.GetLength(0)
            Dim cols = a.GetLength(1)
            Dim jc As Double()() = SimdMatrix.MultiplyScalar(RectToJagged(a), s)
            Return JaggedToRect(jc, rows, cols)
        End Function

        ''' <summary>矩阵向量乘法 y = A × x</summary>
        ''' <remarks>
        ''' 复用同一个行缓冲把矩形数组的每一行拷成连续内存，再走 FMA 点积内核，
        ''' 避免为每一行单独分配数组。
        ''' </remarks>
        Public Function MultiplyVec(a As Double(,), x As Double()) As Double()
            Dim rows = a.GetLength(0)
            Dim cols = a.GetLength(1)
            Dim result(rows - 1) As Double
            Dim row As Double() = New Double(cols - 1) {}

            For i = 0 To rows - 1
                For j = 0 To cols - 1
                    row(j) = a(i, j)
                Next

                result(i) = SIMDIntrinsics.DotFma(row, x)
            Next

            Return result
        End Function

        ''' <summary>
        ''' 矩阵求逆 - 使用 Gauss-Jordan 消元法带部分主元选取
        ''' </summary>
        ''' <param name="strict">
        ''' 兼容参数：现在无论该参数取值如何，数值奇异都会按照
        ''' <paramref name="throwSingularity"/> 的设定抛出异常或返回 Nothing。
        ''' 旧实现中非 strict 模式会把接近零的主元替换为固定值 1e-13 继续计算，
        ''' 这会静默产生完全错误的逆矩阵，已移除该降级行为。
        ''' </param>
        ''' <param name="throwSingularity">
        ''' this function will returns nothing if this parameter value set to false
        ''' </param>
        Public Function Inverse(a As Double(,), strict As Boolean, Optional throwSingularity As Boolean = True) As Double(,)
            Dim n = a.GetLength(0)

            If a.GetLength(1) <> n Then
                Throw New InvalidProgramException($"矩阵A必须为方阵才能求逆，当前A维度为：{n}x{a.GetLength(1)}")
            End If

            ' 构造增广矩阵 [a | I]
            Dim aug(n - 1, 2 * n - 1) As Double
            For i = 0 To n - 1
                For j = 0 To n - 1
                    aug(i, j) = a(i, j)
                Next
                aug(i, i + n) = 1.0
            Next

            ' 尺度相对的奇异判定阈值：主元绝对值相对矩阵整体尺度
            ' 低于该比例时视为数值奇异（旧实现使用与尺度无关的固定值 1e-13）
            Dim matScale = 0.0
            For i = 0 To n - 1
                For j = 0 To n - 1
                    matScale = stdf.Max(matScale, stdf.Abs(a(i, j)))
                Next
            Next

            If matScale = 0 Then
                If throwSingularity Then
                    Throw New Exception("矩阵奇异（零矩阵），无法求逆")
                Else
                    Return Nothing
                End If
            End If

            Dim singularTol = matScale * 0.00000000000001

            ' 前向消元（带部分主元选取）
            For col As Integer = 0 To n - 1
                ' 寻找主元
                Dim maxRow = col
                Dim maxVal = stdf.Abs(aug(col, col))
                For row = col + 1 To n - 1
                    If stdf.Abs(aug(row, col)) > maxVal Then
                        maxVal = stdf.Abs(aug(row, col))
                        maxRow = row
                    End If
                Next

                ' 交换行
                If maxRow <> col Then
                    For j = 0 To 2 * n - 1
                        Dim temp = aug(col, j)
                        aug(col, j) = aug(maxRow, j)
                        aug(maxRow, j) = temp
                    Next
                End If

                ' 准确度修正：部分主元已是当前列剩余部分的最大元素，
                ' 若它仍小于尺度相对阈值，则矩阵数值奇异，立即失败而非静默降级
                Dim pivot = aug(col, col)

                If maxVal <= singularTol Then
                    If throwSingularity Then
                        Throw New Exception("矩阵奇异，无法求逆")
                    Else
                        Return Nothing
                    End If
                End If

                For j = 0 To 2 * n - 1
                    aug(col, j) /= pivot
                Next

                ' 消去其他行
                For row = 0 To n - 1
                    If row <> col Then
                        Dim factor = aug(row, col)
                        For j = 0 To 2 * n - 1
                            aug(row, j) -= factor * aug(col, j)
                        Next
                    End If
                Next
            Next

            ' 提取逆矩阵
            Dim inv(n - 1, n - 1) As Double
            For i = 0 To n - 1
                For j = 0 To n - 1
                    inv(i, j) = aug(i, j + n)
                Next
            Next
            Return inv
        End Function

        ''' <summary>计算行列式（LU 分解法，带部分主元选取，数值稳定）</summary>
        ''' <remarks>
        ''' 准确度修正：奇异判定从固定阈值 1e-14 改为尺度相对阈值
        ''' （主元绝对值 &lt;= 矩阵最大元素绝对值 * 1e-14 时返回 0）。
        ''' 旧实现会因固定阈值与矩阵尺度无关而对大数矩阵误判奇异。
        ''' </remarks>
        Public Function Determinant(a As Double(,)) As Double
            Dim n = a.GetLength(0)
            If a.GetLength(1) <> n Then Throw New Exception("行列式要求方阵")
            If n = 1 Then Return a(0, 0)
            If n = 2 Then Return a(0, 0) * a(1, 1) - a(0, 1) * a(1, 0)

            ' 使用 LU 分解法计算行列式（更稳定）
            Dim lu(n - 1, n - 1) As Double
            For i = 0 To n - 1
                For j = 0 To n - 1
                    lu(i, j) = a(i, j)
                Next
            Next

            ' 矩阵整体尺度，用于相对奇异判定
            Dim matScale = 0.0
            For i = 0 To n - 1
                For j = 0 To n - 1
                    matScale = stdf.Max(matScale, stdf.Abs(a(i, j)))
                Next
            Next

            Dim singularTol = matScale * 0.00000000000001

            Dim det = 1.0
            For k = 0 To n - 1
                ' 部分主元
                Dim maxRow = k
                Dim maxVal = stdf.Abs(lu(k, k))
                For i = k + 1 To n - 1
                    If stdf.Abs(lu(i, k)) > maxVal Then
                        maxVal = stdf.Abs(lu(i, k))
                        maxRow = i
                    End If
                Next
                If maxRow <> k Then
                    For j = 0 To n - 1
                        Dim temp = lu(k, j)
                        lu(k, j) = lu(maxRow, j)
                        lu(maxRow, j) = temp
                    Next
                    det = -det
                End If

                If maxVal <= singularTol Then Return 0.0

                det *= lu(k, k)
                For i = k + 1 To n - 1
                    lu(i, k) /= lu(k, k)
                    For j = k + 1 To n - 1
                        lu(i, j) -= lu(i, k) * lu(k, j)
                    Next
                Next
            Next
            Return det
        End Function

        ''' <summary>
        ''' 对称矩阵特征值分解 - 使用 Jacobi 旋转法
        ''' 返回 (eigenvalues, eigenvectors)，eigenvectors 的列是对应的特征向量
        ''' </summary>
        Public Function JacobiEigen(a As Double(,)) As (eigenvalues As Double(), eigenvectors As Double(,))
            Dim n = a.GetLength(0)
            If a.GetLength(1) <> n Then Throw New Exception("特征值分解要求方阵")

            Dim mat(n - 1, n - 1) As Double
            Dim v(n - 1, n - 1) As Double
            For i = 0 To n - 1
                For j = 0 To n - 1
                    mat(i, j) = a(i, j)
                Next
                v(i, i) = 1.0
            Next

            Dim maxIter = 200
            ' 准确度修正：收敛容差从固定值 1e-12 改为相对矩阵尺度的比例值，
            ' 避免大数/小数矩阵因固定阈值而误判收敛或永不收敛
            Dim matScale = 0.0
            For i = 0 To n - 1
                For j = 0 To n - 1
                    matScale = stdf.Max(matScale, stdf.Abs(a(i, j)))
                Next
            Next
            Dim tol = stdf.Max(matScale, 1.0) * 0.000000000001
            Dim converged = False

            For iter = 1 To maxIter
                ' 找最大的非对角元素
                Dim maxOff = 0.0
                Dim p = 0, q = 0
                For i = 0 To n - 2
                    For j = i + 1 To n - 1
                        If stdf.Abs(mat(i, j)) > maxOff Then
                            maxOff = stdf.Abs(mat(i, j))
                            p = i
                            q = j
                        End If
                    Next
                Next

                If maxOff < tol Then
                    converged = True
                    Exit For
                End If

                ' 计算 Jacobi 旋转
                Dim app = mat(p, p)
                Dim aqq = mat(q, q)
                Dim apq = mat(p, q)

                Dim theta As Double
                If stdf.Abs(app - aqq) < 1.0E-30 Then
                    theta = stdf.PI / 4
                Else
                    theta = 0.5 * stdf.Atan2(2 * apq, app - aqq)
                End If

                Dim c = stdf.Cos(theta)
                Dim s = stdf.Sin(theta)

                ' 应用旋转
                For i = 0 To n - 1
                    Dim tempPi = mat(i, p)
                    Dim tempQi = mat(i, q)
                    mat(i, p) = c * tempPi + s * tempQi
                    mat(i, q) = -s * tempPi + c * tempQi
                Next
                For j = 0 To n - 1
                    Dim tempPj = mat(p, j)
                    Dim tempQj = mat(q, j)
                    mat(p, j) = c * tempPj + s * tempQj
                    mat(q, j) = -s * tempPj + c * tempQj
                Next
                For i = 0 To n - 1
                    Dim tempVi = v(i, p)
                    Dim tempVi2 = v(i, q)
                    v(i, p) = c * tempVi + s * tempVi2
                    v(i, q) = -s * tempVi + c * tempVi2
                Next
            Next

            ' 准确度修正：迭代耗尽仍未收敛时抛出异常（旧实现静默返回未收敛的结果）
            If Not converged Then
                Throw New Exception(
                    $"Jacobi eigenvalue decomposition does not converge after {maxIter} iterations.")
            End If

            Dim eigenvalues(n - 1) As Double
            For i = 0 To n - 1
                eigenvalues(i) = mat(i, i)
            Next

            Return (eigenvalues, v)
        End Function

        ''' <summary>矩阵的迹（对角元素之和）</summary>
        Public Function Trace(a As Double(,)) As Double
            Dim n = stdf.Min(a.GetLength(0), a.GetLength(1))
            Dim tr = 0.0
            For i = 0 To n - 1
                tr += a(i, i)
            Next
            Return tr
        End Function

        ''' <summary>打印矩阵（调试用）</summary>
        Public Sub PrintMatrix(a As Double(,), Optional name As String = "")
            If name <> "" Then Console.WriteLine($"{name}:")
            Dim rows = a.GetLength(0)
            Dim cols = a.GetLength(1)
            For i = 0 To rows - 1
                For j = 0 To cols - 1
                    Console.Write($"{a(i, j),10:F4} ")
                Next
                Console.WriteLine()
            Next
        End Sub

    End Module
End Namespace
