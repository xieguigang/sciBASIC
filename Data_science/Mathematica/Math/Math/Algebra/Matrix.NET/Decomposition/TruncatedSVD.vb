#Region "Microsoft.VisualBasic::656e99176119584d00c8e1d79d5be9fb, Data_science\Mathematica\Math\Math\Algebra\Matrix.NET\Decomposition\TruncatedSVD.vb"

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

    '   Total Lines: 428
    '    Code Lines: 213 (49.77%)
    ' Comment Lines: 155 (36.21%)
    '    - Xml Docs: 76.77%
    ' 
    '   Blank Lines: 60 (14.02%)
    '     File Size: 18.25 KB


    '     Class TruncatedSVD
    ' 
    '         Properties: Components, ReducedMatrix, SingularValues, U, V
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ApplyMatrix, ApplyTranspose, CloneMatrix, QrQ, Reduce
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports stdf = System.Math
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports _rand = Microsoft.VisualBasic.Math.RandomExtensions

Namespace LinearAlgebra.Matrix

    ''' <summary>
    ''' 截断奇异值分解（Truncated Singular Value Decomposition）。
    ''' <para>
    ''' 采用随机化 SVD 算法（Halko, Martinsson, Tropp, 2009，即 sklearn
    ''' ``TruncatedSVD(solver="randomized")`` 所使用的算法），针对高维稀疏矩阵
    ''' 进行截断的奇异值分解：只计算并保留前 k 个最大奇异值所对应的分量，
    ''' 从而将一个 m×n 的稀疏矩阵降维为 m×k 的稠密（非稀疏）矩阵。
    ''' </para>
    ''' <para>
    ''' 与完整的 <see cref="SingularValueDecomposition"/>（LINPACK 稠密算法，
    ''' 需要将矩阵完全稠密化，O(m·n) 内存）不同，本算法在整个计算流程中
    ''' 仅依赖稀疏矩阵-向量乘法（A·x 与 Aᵀ·y，均为 O(nnz) 复杂度），
    ''' 不会将原始高维稀疏矩阵稠密化，时间复杂度为
    ''' O(nnz·ℓ·(2q+3) + (m+n)·ℓ² + ℓ³)，空间复杂度为 O(nnz + (m+n)·ℓ)，
    ''' 其中 ℓ = k + 过采样维度，q 为幂迭代次数。
    ''' </para>
    ''' <para>
    ''' A ≈ U·Σ·Vᵀ，其中 U 为 m×k 的列正交矩阵（左奇异向量），
    ''' Σ 为 k 个降序排列的奇异值，V 为 n×k 的列正交矩阵（右奇异向量）。
    ''' 降维结果通过 <see cref="ReducedMatrix"/>（= U·Σ = A·V）获得。
    ''' </para>
    ''' </summary>
    Public Class TruncatedSVD

#Region "Class variables"

        ''' <summary>
        ''' 内部存储的左奇异向量矩阵 U（m×k，列正交）。
        ''' </summary>
        Dim m_valueU As Double()()

        ''' <summary>
        ''' 内部存储的右奇异向量矩阵 V（n×k，列正交）。
        ''' </summary>
        Dim m_valueV As Double()()

        ''' <summary>
        ''' 内部存储的前 k 个奇异值（降序排列）。
        ''' </summary>
        Dim m_s As Double()

        ''' <summary>
        ''' 内部存储的降维结果矩阵（m×k，= U·Σ = A·V，稠密非稀疏）。
        ''' </summary>
        Dim m_reduced As Double()()

        ''' <summary>
        ''' 矩阵的行数 m。
        ''' </summary>
        Dim m As Integer

        ''' <summary>
        ''' 矩阵的列数 n。
        ''' </summary>
        Dim n As Integer

        ''' <summary>
        ''' 实际保留的分量个数 k。
        ''' </summary>
        Dim k As Integer

#End Region

#Region "Constructor"

        ''' <summary>
        ''' 对高维稀疏矩阵执行截断的奇异值分解。
        ''' </summary>
        ''' <param name="A">
        ''' 待分解的 m×n 稀疏矩阵，可以是任意的非对称矩阵。
        ''' </param>
        ''' <param name="k">
        ''' 保留的分量个数（目标降维维度），必须满足 1 ≤ k ≤ min(m, n)。
        ''' </param>
        ''' <param name="oversampling">
        ''' 过采样维度，默认为 10。随机化算法使用 ℓ = k + oversampling 个随机
        ''' 采样方向来提升捕捉范围子空间的精度，之后再截断回前 k 个分量。
        ''' </param>
        ''' <param name="powerIterations">
        ''' 幂迭代次数，默认为 1（0 表示不迭代）。当奇异值谱衰减较慢时，
        ''' 增加幂迭代次数可以显著提升精度，代价是每次迭代额外执行
        ''' 2ℓ 次稀疏矩阵-向量乘法。
        ''' </param>
        Public Sub New(A As SparseMatrix, k As Integer,
                       Optional oversampling As Integer = 10,
                       Optional powerIterations As Integer = 1)

            ' ---------------- 参数校验 ----------------
            If A Is Nothing Then
                Throw New ArgumentNullException(NameOf(A))
            End If
            If k < 1 Then
                Throw New ArgumentException($"参数 k({k}) 必须大于等于 1！", NameOf(k))
            End If
            If oversampling < 0 Then
                Throw New ArgumentException($"参数 oversampling({oversampling}) 不能为负数！", NameOf(oversampling))
            End If
            If powerIterations < 0 Then
                Throw New ArgumentException($"参数 powerIterations({powerIterations}) 不能为负数！", NameOf(powerIterations))
            End If

            m = A.RowDimension
            n = A.ColumnDimension

            ' 空矩阵边界处理：直接返回空结果
            If m = 0 OrElse n = 0 Then
                Me.k = 0
                m_s = New Double(-1) {}
                m_valueU = New Double(m - 1)() {}
                m_valueV = New Double(n - 1)() {}
                m_reduced = New Double(m - 1)() {}
                Return
            End If

            If k > stdf.Min(m, n) Then
                Throw New ArgumentException($"参数 k({k}) 不能超过矩阵的较小维度 min(m,n) = {stdf.Min(m, n)}！", NameOf(k))
            End If

            Me.k = k

            ' ---------------- 随机化 SVD 主流程 ----------------

            ' 采样维度 ℓ = min(k + oversampling, min(m, n))
            Dim ell As Integer = stdf.Min(k + oversampling, stdf.Min(m, n))

            ' 1. 生成 n×ℓ 高斯随机测试矩阵 Ω
            Dim Omega As Double()() = RectangularArray.Matrix(Of Double)(n, ell)
            For i As Integer = 0 To n - 1
                For j As Integer = 0 To ell - 1
                    Omega(i)(j) = _rand.NextGaussian()
                Next
            Next

            ' 2. 范围子空间采样：Y = A·Ω（ℓ 次稀疏矩阵-向量乘法 A·x）
            '    随后立即正交化得到 Q（m×ℓ 正交基）
            Dim Q As Double()() = QrQ(ApplyMatrix(A, Omega, ell))

            ' 3. 幂迭代（q 次）：Q ← orth(A·(Aᵀ·Q))
            '    每轮迭代包含 2ℓ 次稀疏矩阵-向量乘法，并在迭代之间
            '    重新做 QR 正交化以避免数值退化
            For p As Integer = 1 To powerIterations
                ' T = Aᵀ·Q（n×ℓ）
                Dim T As Double()() = ApplyTranspose(A, Q, ell)
                ' Y = A·T（m×ℓ），再正交化
                Q = QrQ(ApplyMatrix(A, T, ell))
            Next

            ' 4. W = Aᵀ·Q（n×ℓ），Y2 = A·W（m×ℓ）
            '    则 C = Qᵀ·Y2 = Qᵀ·A·Aᵀ·Q ≈ B·Bᵀ（B = Qᵀ·A）
            '    只需要构造 ℓ×ℓ 的小矩阵即可恢复奇异三元组，
            '    避免了 n×n 或 m×m 稠密矩阵的构造
            Dim W As Double()() = ApplyTranspose(A, Q, ell)
            Dim Y2 As Double()() = ApplyMatrix(A, W, ell)

            ' 5. 构造 ℓ×ℓ 小矩阵 C = Qᵀ·Y2，并做对称化处理，
            '    确保 <see cref="EigenvalueDecomposition"/> 走对称路径得到实特征对
            Dim Cmat As Double()() = RectangularArray.Matrix(Of Double)(ell, ell)
            For ia As Integer = 0 To ell - 1
                For ib As Integer = 0 To ell - 1
                    Dim s As Double = 0.0
                    For i As Integer = 0 To m - 1
                        s += Q(i)(ia) * Y2(i)(ib)
                    Next
                    Cmat(ia)(ib) = s
                Next
            Next

            ' 对称化：C ← (C + Cᵀ)/2，消除浮点舍入带来的非对称误差
            For ia As Integer = 0 To ell - 1
                For ib As Integer = ia + 1 To ell - 1
                    Dim avg As Double = (Cmat(ia)(ib) + Cmat(ib)(ia)) / 2.0
                    Cmat(ia)(ib) = avg
                    Cmat(ib)(ia) = avg
                Next
            Next

            ' 6. 对 ℓ×ℓ 对称矩阵做特征分解（复用 EigenvalueDecomposition）
            Dim eig As New EigenvalueDecomposition(New NumericMatrix(Cmat, ell, ell))
            Dim lambda As Double() = eig.RealEigenvalues
            ' 特征向量矩阵（ℓ×ℓ，第 j 列为对应 lambda(j) 的特征向量）
            Dim Va As Double()() = eig.V.ArrayPack()

            ' 7. 按特征值降序排列，截断取前 k 个：
            '    σ_i = √λ_i（半正定矩阵特征值理论上非负，取 max 防数值负值）
            Dim order As Integer() = Enumerable.Range(0, ell) _
                .OrderByDescending(Function(i) lambda(i)) _
                .Take(k) _
                .ToArray

            m_s = New Double(k - 1) {}
            For j As Integer = 0 To k - 1
                m_s(j) = stdf.Sqrt(stdf.Max(lambda(order(j)), 0.0))
            Next

            ' 8. 恢复左奇异向量：U_k = Q·[u_1..u_k]（m×k）
            m_valueU = RectangularArray.Matrix(Of Double)(m, k)
            For r As Integer = 0 To m - 1
                For c As Integer = 0 To k - 1
                    Dim s As Double = 0.0
                    For t As Integer = 0 To ell - 1
                        s += Q(r)(t) * Va(t)(order(c))
                    Next
                    m_valueU(r)(c) = s
                Next
            Next

            ' 9. 恢复右奇异向量：V_k = W·[u_1..u_k]（n×k）
            '    因为 W = Aᵀ·Q = Bᵀ，所以 W·u_i/σ_i 即为第 i 个右奇异向量；
            '    这里复用已经计算好的 W 做小规模稠密矩阵乘法（O(n·ℓ·k)），
            '    避免额外的 k 次稀疏矩阵-向量乘法
            m_valueV = RectangularArray.Matrix(Of Double)(n, k)
            For r As Integer = 0 To n - 1
                For c As Integer = 0 To k - 1
                    Dim s As Double = 0.0
                    For t As Integer = 0 To ell - 1
                        s += W(r)(t) * Va(t)(order(c))
                    Next
                    If m_s(c) > 0.0 Then
                        m_valueV(r)(c) = s / m_s(c)
                    Else
                        m_valueV(r)(c) = 0.0
                    End If
                Next
            Next

            ' 10. 降维结果：X_k = U_k·Σ_k = A·V_k（m×k 稠密矩阵）
            '     即将原始稀疏矩阵的每一行投影到前 k 个右奇异向量张成的子空间上
            m_reduced = RectangularArray.Matrix(Of Double)(m, k)
            For r As Integer = 0 To m - 1
                For c As Integer = 0 To k - 1
                    m_reduced(r)(c) = m_valueU(r)(c) * m_s(c)
                Next
            Next
        End Sub

#End Region

#Region "Public Properties"

        ''' <summary>
        ''' 左奇异向量矩阵 U（m×k，列正交）。A ≈ U·Σ·Vᵀ。
        ''' </summary>
        ''' <returns>返回内部数组的副本，修改返回值不会影响分解结果。</returns>
        Public ReadOnly Property U As Double()()
            Get
                Return CloneMatrix(m_valueU)
            End Get
        End Property

        ''' <summary>
        ''' 前 k 个奇异值，按从大到小降序排列。Σ = diag(SingularValues)。
        ''' </summary>
        ''' <returns>返回内部数组的副本。</returns>
        Public ReadOnly Property SingularValues As Double()
            Get
                Return m_s.Clone()
            End Get
        End Property

        ''' <summary>
        ''' 右奇异向量矩阵 V（n×k，列正交）。A ≈ U·Σ·Vᵀ。
        ''' </summary>
        ''' <returns>返回内部数组的副本，修改返回值不会影响分解结果。</returns>
        Public ReadOnly Property V As Double()()
            Get
                Return CloneMatrix(m_valueV)
            End Get
        End Property

        ''' <summary>
        ''' 分量矩阵（k×n），即 V 的转置。第 i 行是第 i 个右奇异向量的转置，
        ''' 可用于将任意的 n 维观测向量投影降维到 k 维：y = x·Componentsᵀ。
        ''' </summary>
        ''' <returns>返回新生成的 k×n 稠密矩阵。</returns>
        Public ReadOnly Property Components As Double()()
            Get
                If k = 0 Then
                    Return New Double(-1)() {}
                End If

                Dim comp As Double()() = RectangularArray.Matrix(Of Double)(k, n)
                For i As Integer = 0 To k - 1
                    For j As Integer = 0 To n - 1
                        comp(i)(j) = m_valueV(j)(i)
                    Next
                Next
                Return comp
            End Get
        End Property

        ''' <summary>
        ''' 降维结果矩阵（m×k，稠密非稀疏），等于 U·Σ = A·V。
        ''' <para>
        ''' 将原始高维稀疏矩阵 A 的每一行（n 维稀疏向量）投影到前 k 个
        ''' 最大奇异值对应的右奇异向量所张成的子空间中，得到 k 维稠密表示。
        ''' </para>
        ''' </summary>
        ''' <returns>返回内部数组的副本，修改返回值不会影响分解结果。</returns>
        Public ReadOnly Property ReducedMatrix As Double()()
            Get
                Return CloneMatrix(m_reduced)
            End Get
        End Property

#End Region

#Region "Public Methods"

        ''' <summary>
        ''' 一步式的便捷入口：对高维稀疏矩阵执行截断 SVD 并直接返回降维结果。
        ''' </summary>
        ''' <param name="A">待降维的 m×n 稀疏矩阵</param>
        ''' <param name="k">目标降维维度，1 ≤ k ≤ min(m, n)</param>
        ''' <param name="oversampling">过采样维度，默认 10</param>
        ''' <param name="powerIterations">幂迭代次数，默认 1</param>
        ''' <returns>m×k 的稠密（非稀疏）降维矩阵（= U·Σ = A·V）</returns>
        Public Shared Function Reduce(A As SparseMatrix, k As Integer,
                                      Optional oversampling As Integer = 10,
                                      Optional powerIterations As Integer = 1) As Double()()
            Return New TruncatedSVD(A, k, oversampling, powerIterations).ReducedMatrix
        End Function

#End Region

#Region "Private Helpers"

        ''' <summary>
        ''' 批量计算稀疏矩阵的右乘：M = A·X。复用 <see cref="SparseMatrix.Multiply"/>
        ''' 逐列完成 ℓ 次稀疏矩阵-向量乘法。
        ''' </summary>
        ''' <param name="A">m×n 稀疏矩阵</param>
        ''' <param name="X">n×ℓ 输入矩阵</param>
        ''' <param name="ell">X 的列数 ℓ</param>
        ''' <returns>m×ℓ 的结果矩阵</returns>
        Private Shared Function ApplyMatrix(A As SparseMatrix, X As Double()(), ell As Integer) As Double()()
            Dim m As Integer = A.RowDimension
            Dim n As Integer = A.ColumnDimension
            Dim result As Double()() = RectangularArray.Matrix(Of Double)(m, ell)
            Dim col As Double() = New Double(n - 1) {}

            For j As Integer = 0 To ell - 1
                ' 提取 X 的第 j 列
                For i As Integer = 0 To n - 1
                    col(i) = X(i)(j)
                Next

                ' 稀疏矩阵-向量乘法：y = A·x
                Dim y As Double() = A.Multiply(col)

                ' 写回结果的第 j 列
                For i As Integer = 0 To m - 1
                    result(i)(j) = y(i)
                Next
            Next

            Return result
        End Function

        ''' <summary>
        ''' 批量计算稀疏矩阵的转置乘：M = Aᵀ·X。复用
        ''' <see cref="SparseMatrix.MultiplyTranspose"/> 逐列完成 ℓ 次稀疏乘法。
        ''' </summary>
        ''' <param name="A">m×n 稀疏矩阵</param>
        ''' <param name="X">m×ℓ 输入矩阵</param>
        ''' <param name="ell">X 的列数 ℓ</param>
        ''' <returns>n×ℓ 的结果矩阵</returns>
        Private Shared Function ApplyTranspose(A As SparseMatrix, X As Double()(), ell As Integer) As Double()()
            Dim m As Integer = A.RowDimension
            Dim n As Integer = A.ColumnDimension
            Dim result As Double()() = RectangularArray.Matrix(Of Double)(n, ell)
            Dim col As Double() = New Double(m - 1) {}

            For j As Integer = 0 To ell - 1
                ' 提取 X 的第 j 列
                For i As Integer = 0 To m - 1
                    col(i) = X(i)(j)
                Next

                ' 稀疏转置乘法：y = Aᵀ·x
                Dim y As Double() = A.MultiplyTranspose(col)

                ' 写回结果的第 j 列
                For i As Integer = 0 To n - 1
                    result(i)(j) = y(i)
                Next
            Next

            Return result
        End Function

        ''' <summary>
        ''' 对 m×ℓ（m ≥ ℓ）矩阵 Y 做 economy 尺寸的 QR 分解并返回正交因子 Q，
        ''' 复用 <see cref="QRDecomposition"/>（Householder 反射实现）。
        ''' </summary>
        ''' <param name="Y">待正交化的 m×ℓ 矩阵</param>
        ''' <returns>m×ℓ 的列正交矩阵</returns>
        Private Shared Function QrQ(Y As Double()()) As Double()()
            Dim m As Integer = Y.Length
            Dim ell As Integer = Y(0).Length
            Dim qr As New QRDecomposition(New NumericMatrix(Y, m, ell))
            Return qr.Q.ArrayPack()
        End Function

        ''' <summary>
        ''' 生成锯齿二维数组的深拷贝。
        ''' </summary>
        Private Shared Function CloneMatrix(X As Double()()) As Double()()
            If X Is Nothing OrElse X.Length = 0 Then
                Return New Double(-1)() {}
            End If

            Dim copy As Double()() = New Double(X.Length - 1)() {}
            For i As Integer = 0 To X.Length - 1
                copy(i) = X(i).Clone()
            Next
            Return copy
        End Function

#End Region

    End Class
End Namespace
