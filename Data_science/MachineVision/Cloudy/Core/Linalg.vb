Imports std = System.Math

Namespace Core

    ''' <summary>
    ''' 小规模稠密线性代数：3x3/4x4/6x6/NxN 的求解、特征分解与刚体变换闭式估计。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 点云算法里 99% 的线性代数都是<b>极小规模</b>的：
    ''' 法向估计是 3x3 对称阵的特征分解、ICP 点到面是 6x6 正规方程、
    ''' 变换估计是 4x4 四元数矩阵的最大特征向量、位姿图是几十阶的稀疏（这里按稠密处理）系统。
    ''' 把这些问题交给通用矩阵库（构造 <c>NumericMatrix</c>、走 LU/QR 分解）
    ''' 会产生大量小对象分配与虚调用，在“每点一次”的调用频率下是不可接受的。
    ''' </para>
    ''' <para>
    ''' 因此这里自包含实现：
    ''' </para>
    ''' <list type="bullet">
    ''' <item><description><b>3x3 对称阵闭式特征分解</b>（三角函数解析解，无迭代、无分配、可复现）。</description></item>
    ''' <item><description><b>带主元的高斯消元</b>与<b>Cholesky</b>（6x6 正规方程：ICP/GICP/NDT/位姿图）。</description></item>
    ''' <item><description><b>Kabsch/Horn 闭式刚体变换</b>（4x4 幂迭代求最大特征向量，避免对 3x3 交叉协方差做 SVD）。</description></item>
    ''' </list>
    ''' <para>
    ''' <b>精度取舍</b>：解析特征值在“两个特征值几乎相等”时对扰动敏感
    ''' （特征向量在简并子空间内可以任意旋转），这是对称阵的固有性质而非实现缺陷。
    ''' 法向估计在平面区域内正好落在这个退化情形，但只要特征向量仍落在切平面内，
    ''' 法向依旧正确——因此这里只保证“最小特征向量稳定”，不保证其余两个的朝向。
    ''' </para>
    ''' </remarks>
    Public NotInheritable Class Linalg

        Private Sub New()
        End Sub

#Region "3x3 helpers"

        ''' <summary>
        ''' 3x3 矩阵行列式。
        ''' </summary>
        Public Shared Function Determinant3(m As Double()()) As Double
            If m Is Nothing Then Throw New ArgumentNullException(NameOf(m))

            Return m(0)(0) * (m(1)(1) * m(2)(2) - m(1)(2) * m(2)(1)) -
                m(0)(1) * (m(1)(0) * m(2)(2) - m(1)(2) * m(2)(0)) +
                m(0)(2) * (m(1)(0) * m(2)(1) - m(1)(1) * m(2)(0))
        End Function

        ''' <summary>
        ''' 3x3 矩阵转置。
        ''' </summary>
        Public Shared Function Transpose3(m As Double()()) As Double()()
            Return {
                New Double() {m(0)(0), m(1)(0), m(2)(0)},
                New Double() {m(0)(1), m(1)(1), m(2)(1)},
                New Double() {m(0)(2), m(1)(2), m(2)(2)}
            }
        End Function

        ''' <summary>
        ''' 3x3 矩阵乘法 <c>A·B</c>。
        ''' </summary>
        Public Shared Function Multiply3(a As Double()(), b As Double()()) As Double()()
            Dim r As Double()() = {
                New Double() {0, 0, 0},
                New Double() {0, 0, 0},
                New Double() {0, 0, 0}
            }

            For i As Integer = 0 To 2
                For j As Integer = 0 To 2
                    r(i)(j) = a(i)(0) * b(0)(j) + a(i)(1) * b(1)(j) + a(i)(2) * b(2)(j)
                Next
            Next

            Return r
        End Function

        ''' <summary>
        ''' 3x3 求逆（伴随矩阵法）。奇异或近奇异时返回 <c>False</c>。
        ''' </summary>
        ''' <remarks>
        ''' NDT 需要对每个体素的协方差求逆、GICP 需要对每个点的协方差求逆；
        ''' 这些矩阵是 3x3 对称半正定阵，用伴随矩阵法（<c>O(1)</c> 次乘加、无循环）
        ''' 远快于任何通用分解。
        ''' </remarks>
        Public Shared Function TryInverse3(m As Double()(), ByRef inverse As Double()(),
                                           Optional epsilon As Double = 1.0E-12) As Boolean
            Dim det As Double = Determinant3(m)

            If std.Abs(det) <= epsilon Then
                inverse = Nothing

                Return False
            End If

            Dim inv As Double()() = {
                New Double() {m(1)(1) * m(2)(2) - m(1)(2) * m(2)(1),
                              m(0)(2) * m(2)(1) - m(0)(1) * m(2)(2),
                              m(0)(1) * m(1)(2) - m(0)(2) * m(1)(1)},
                New Double() {m(1)(2) * m(2)(0) - m(1)(0) * m(2)(2),
                              m(0)(0) * m(2)(2) - m(0)(2) * m(2)(0),
                              m(0)(2) * m(1)(0) - m(0)(0) * m(1)(2)},
                New Double() {m(1)(0) * m(2)(1) - m(1)(1) * m(2)(0),
                              m(0)(1) * m(2)(0) - m(0)(0) * m(2)(1),
                              m(0)(0) * m(1)(1) - m(0)(1) * m(1)(0)}
            }

            Dim invDet As Double = 1.0 / det

            For i As Integer = 0 To 2
                For j As Integer = 0 To 2
                    inv(i)(j) *= invDet
                Next
            Next

            inverse = inv

            Return True
        End Function

        ''' <summary>
        ''' 3x3 矩阵与向量相乘。
        ''' </summary>
        Public Shared Function Multiply3(m As Double()(), v As Vector3d) As Vector3d
            Return New Vector3d(m(0)(0) * v.X + m(0)(1) * v.Y + m(0)(2) * v.Z,
                                m(1)(0) * v.X + m(1)(1) * v.Y + m(1)(2) * v.Z,
                                m(2)(0) * v.X + m(2)(1) * v.Y + m(2)(2) * v.Z)
        End Function

        ''' <summary>
        ''' Gram-Schmidt 正交化：把因浮点误差而轻微偏离正交的旋转矩阵拉回 <c>SO(3)</c>。
        ''' </summary>
        ''' <remarks>
        ''' 配准迭代几十轮之后，<c>R·Rᵀ = I</c> 的偏离会累积到 1e-9 量级；
        ''' 单次影响可以忽略，但如果把变换继续用于下一次迭代（乘法复合），
        ''' 误差会按轮数线性增长并最终表现为点云整体的“缓慢剪切”。
        ''' 每轮收尾做一次正交化是代价极低（十几条指令）的保险。
        ''' </remarks>
        Public Shared Function Orthonormalize(m As Double()()) As Double()()
            Dim c0 As Vector3d = New Vector3d(m(0)(0), m(1)(0), m(2)(0)).Normalize()
            Dim c1 As Vector3d = New Vector3d(m(0)(1), m(1)(1), m(2)(1))

            c1 = (c1 - c0 * Vector3d.Dot(c0, c1)).Normalize()

            Dim c2 As Vector3d = Vector3d.Cross(c0, c1)

            Return {
                New Double() {c0.X, c1.X, c2.X},
                New Double() {c0.Y, c1.Y, c2.Y},
                New Double() {c0.Z, c1.Z, c2.Z}
            }
        End Function

#End Region

#Region "symmetric 3x3 eigen decomposition"

        ''' <summary>
        ''' 3x3 对称矩阵特征分解结果（特征值降序，特征向量与之一一对应且两两正交）。
        ''' </summary>
        Public Structure Eigen3

            ''' <summary>最大特征值（= 该方向上的方差）。</summary>
            Public ReadOnly L1 As Double
            ''' <summary>中间特征值。</summary>
            Public ReadOnly L2 As Double
            ''' <summary>最小特征值（平面区域接近 0，对应法向方向）。</summary>
            Public ReadOnly L3 As Double

            ''' <summary>与 <see cref="L1"/> 对应的特征向量。</summary>
            Public ReadOnly V1 As Vector3d
            ''' <summary>与 <see cref="L2"/> 对应的特征向量。</summary>
            Public ReadOnly V2 As Vector3d
            ''' <summary>与 <see cref="L3"/> 对应的特征向量（法向估计取这个）。</summary>
            Public ReadOnly V3 As Vector3d

            Public Sub New(l1 As Double, l2 As Double, l3 As Double,
                           v1 As Vector3d, v2 As Vector3d, v3 As Vector3d)
                Me.L1 = l1
                Me.L2 = l2
                Me.L3 = l3
                Me.V1 = v1
                Me.V2 = v2
                Me.V3 = v3
            End Sub

            ''' <summary>
            ''' 特征值之和（等于协方差矩阵的迹）。
            ''' </summary>
            Public ReadOnly Property Trace As Double
                Get
                    Return L1 + L2 + L3
                End Get
            End Property

            ''' <summary>
            ''' 表面变化度（曲率）：<c>L3 / (L1 + L2 + L3)</c>，取值 <c>[0, 1/3]</c>。
            ''' </summary>
            ''' <remarks>
            ''' 这是 PCL 的 <c>computeCurvature</c> 使用的定义：平面区域 <c>L3≈0</c> 因此曲率≈0，
            ''' 各向同性噪声（球状邻域）时三个特征值相等，曲率≈1/3。
            ''' </remarks>
            Public ReadOnly Property SurfaceVariation As Double
                Get
                    Dim sum As Double = L1 + L2 + L3

                    If sum <= 0.0 Then Return 0.0

                    Return L3 / sum
                End Get
            End Property

            ''' <summary>
            ''' 线性度：<c>(L1 − L2) / L1</c>（管状/线状结构接近 1）。
            ''' </summary>
            Public ReadOnly Property Linearity As Double
                Get
                    If L1 <= 0.0 Then Return 0.0

                    Return (L1 - L2) / L1
                End Get
            End Property

            ''' <summary>
            ''' 平面度：<c>(L2 − L3) / L1</c>（平面结构接近 1）。
            ''' </summary>
            Public ReadOnly Property Planarity As Double
                Get
                    If L1 <= 0.0 Then Return 0.0

                    Return (L2 - L3) / L1
                End Get
            End Property

            ''' <summary>
            ''' 散射度：<c>L3 / L1</c>（体状/噪声区域接近 1）。
            ''' </summary>
            Public ReadOnly Property Scattering As Double
                Get
                    If L1 <= 0.0 Then Return 0.0

                    Return L3 / L1
                End Get
            End Property

            Public Overrides Function ToString() As String
                Return $"λ=({L1:G4}, {L2:G4}, {L3:G4})"
            End Function
        End Structure

        ''' <summary>
        ''' 3x3 对称矩阵的闭式特征分解（特征值降序）。
        ''' </summary>
        ''' <param name="cxx">M(0,0)</param>
        ''' <param name="cxy">M(0,1)=M(1,0)</param>
        ''' <param name="cxz">M(0,2)=M(2,0)</param>
        ''' <param name="cyy">M(1,1)</param>
        ''' <param name="cyz">M(1,2)=M(2,1)</param>
        ''' <param name="czz">M(2,2)</param>
        ''' <remarks>
        ''' <para>
        ''' 使用 Smith 的解析解：把矩阵写成 <c>q·I + p·B</c>（<c>B</c> 无迹），
        ''' 特征值通过 <c>acos</c> 三分角公式一次求出，不需要任何迭代。
        ''' 相比 Jacobi 旋转（每轮 3 次 2x2 旋转、需要 6~10 轮），
        ''' 在“每点一次”的调用频率下差距是一个数量级。
        ''' </para>
        ''' <para>
        ''' 特征向量用“取两行叉积”的方式求解：对 <c>M − λI</c>（秩 ≤ 2），
        ''' 任意两行的叉积都落在零空间里；选择长度最大的那个组合以避开数值退化，
        ''' 之后再做一次 Gram-Schmidt 保证三者正交。
        ''' </para>
        ''' </remarks>
        Public Shared Function EigenSymmetric3(cxx As Double, cxy As Double, cxz As Double,
                                               cyy As Double, cyz As Double, czz As Double) As Eigen3
            Dim p1 As Double = cxy * cxy + cxz * cxz + cyz * cyz

            If p1 <= 1.0E-30 Then
                ' 对角矩阵：特征值就是对角元，特征向量就是坐标轴
                Return SortEigen(cxx, Vector3d.UnitX, cyy, Vector3d.UnitY, czz, Vector3d.UnitZ)
            End If

            Dim q As Double = (cxx + cyy + czz) / 3.0
            Dim p2 As Double = (cxx - q) * (cxx - q) + (cyy - q) * (cyy - q) + (czz - q) * (czz - q) + 2.0 * p1
            Dim p As Double = std.Sqrt(p2 / 6.0)

            If p <= 0.0 Then
                Return SortEigen(q, Vector3d.UnitX, q, Vector3d.UnitY, q, Vector3d.UnitZ)
            End If

            ' B = (M − q·I) / p，无迹矩阵的行列式决定旋转角
            Dim b00 As Double = (cxx - q) / p
            Dim b11 As Double = (cyy - q) / p
            Dim b22 As Double = (czz - q) / p
            Dim b01 As Double = cxy / p
            Dim b02 As Double = cxz / p
            Dim b12 As Double = cyz / p

            Dim detB As Double = b00 * (b11 * b22 - b12 * b12) -
                b01 * (b01 * b22 - b12 * b02) +
                b02 * (b01 * b12 - b11 * b02)

            Dim r As Double = detB * 0.5

            If r > 1.0 Then r = 1.0
            If r < -1.0 Then r = -1.0

            Dim phi As Double = std.Acos(r) / 3.0
            Dim l1 As Double = q + 2.0 * p * std.Cos(phi)
            Dim l3 As Double = q + 2.0 * p * std.Cos(phi + 2.0 * std.PI / 3.0)
            Dim l2 As Double = 3.0 * q - l1 - l3

            Dim v1 As Vector3d = EigenvectorFor(cxx, cxy, cxz, cyy, cyz, czz, l1)
            Dim v2 As Vector3d = EigenvectorFor(cxx, cxy, cxz, cyy, cyz, czz, l2)

            ' 第三个向量由前两个叉乘得到，比独立求解更稳且自动正交
            Dim v3 As Vector3d = Vector3d.Cross(v1, v2)

            If v3.IsZero(1.0E-9) Then
                v3 = EigenvectorFor(cxx, cxy, cxz, cyy, cyz, czz, l3)
            End If

            v3 = v3.Normalize()

            ' 保证右手系（不影响特征向量本身，但让后续叉乘语义一致）
            If Vector3d.Dot(Vector3d.Cross(v1, v2), v3) < 0.0 Then
                v3 = -v3
            End If

            Return SortEigen(l1, v1, l2, v2, l3, v3)
        End Function

        ''' <summary>
        ''' 求指定特征值对应的特征向量（<c>M − λI</c> 的零空间方向）。
        ''' </summary>
        Private Shared Function EigenvectorFor(cxx As Double, cxy As Double, cxz As Double,
                                               cyy As Double, cyz As Double, czz As Double,
                                               lambda As Double) As Vector3d
            Dim r0 As New Vector3d(cxx - lambda, cxy, cxz)
            Dim r1 As New Vector3d(cxy, cyy - lambda, cyz)
            Dim r2 As New Vector3d(cxz, cyz, czz - lambda)

            Dim c01 As Vector3d = Vector3d.Cross(r0, r1)
            Dim c02 As Vector3d = Vector3d.Cross(r0, r2)
            Dim c12 As Vector3d = Vector3d.Cross(r1, r2)

            Dim best As Vector3d = c01
            Dim bestLen As Double = c01.LengthSquared

            If c02.LengthSquared > bestLen Then
                best = c02
                bestLen = c02.LengthSquared
            End If
            If c12.LengthSquared > bestLen Then
                best = c12
                bestLen = c12.LengthSquared
            End If

            If bestLen <= 1.0E-24 Then
                ' 三重简并（球状邻域）：任何方向都是特征向量，取与最小对角元对应的轴
                Dim ax As Double = std.Abs(cxx - lambda)
                Dim ay As Double = std.Abs(cyy - lambda)
                Dim az As Double = std.Abs(czz - lambda)

                If ax <= ay AndAlso ax <= az Then Return Vector3d.UnitX
                If ay <= az Then Return Vector3d.UnitY

                Return Vector3d.UnitZ
            End If

            Return best.Normalize()
        End Function

        ''' <summary>
        ''' 把三组 (λ, v) 按特征值降序排列，并对第二/第三个向量做正交化。
        ''' </summary>
        Private Shared Function SortEigen(l1 As Double, v1 As Vector3d,
                                          l2 As Double, v2 As Vector3d,
                                          l3 As Double, v3 As Vector3d) As Eigen3
            ' 三元素选择排序（无分配、可读性最好）
            If l1 < l2 Then Swap(l1, l2, v1, v2)
            If l2 < l3 Then Swap(l2, l3, v2, v3)
            If l1 < l2 Then Swap(l1, l2, v1, v2)

            Dim e1 As Vector3d = v1.Normalize()

            If e1.IsZero(1.0E-12) Then e1 = Vector3d.UnitX

            Dim e2 As Vector3d = (v2 - e1 * Vector3d.Dot(e1, v2)).Normalize()

            If e2.IsZero(1.0E-9) Then
                ' v2 与 v1 共线（简并特征值）：任取一个正交方向
                e2 = If(std.Abs(e1.X) < 0.9, Vector3d.UnitX, Vector3d.UnitY)
                e2 = (e2 - e1 * Vector3d.Dot(e1, e2)).Normalize()
            End If

            Dim e3 As Vector3d = Vector3d.Cross(e1, e2).Normalize()

            Return New Eigen3(l1, l2, l3, e1, e2, e3)
        End Function

        Private Shared Sub Swap(ByRef l1 As Double, ByRef l2 As Double,
                                ByRef v1 As Vector3d, ByRef v2 As Vector3d)
            Dim lt As Double = l1
            Dim vt As Vector3d = v1

            l1 = l2
            v1 = v2
            l2 = lt
            v2 = vt
        End Sub

#End Region

#Region "linear solve"

        ''' <summary>
        ''' 带部分主元的高斯消元求解 <c>A·x = b</c>（<c>A</c> 为 NxN 行主序）。
        ''' </summary>
        ''' <remarks>
        ''' <para>
        ''' ICP 点到面、GICP、NDT、位姿图的每一步都要解一个小型正规方程；
        ''' 这些矩阵规模在 6~100 之间，稠密高斯消元的 <c>O(n³)</c> 完全够用
        ''' （6³ = 216 次乘加），而稀疏/迭代解法在这里的调度开销反而更大。
        ''' </para>
        ''' <para>
        ''' <b>主元</b>：不选主元的高斯消元在矩阵近奇异时会把误差放大到不可用
        ''' （例如所有对应点共面时，点到面 ICP 的 6x6 矩阵会有一个接近零的特征方向）；
        ''' 这里每列都选绝对值最大的行交换，并在主元小于 <paramref name="epsilon"/> 时
        ''' 抛出 <see cref="InvalidOperationException"/>，让调用方有机会降级处理
        ''' （例如减小迭代步长或退回点到点 ICP），而不是返回一个发散的解。
        ''' </para>
        ''' </remarks>
        Public Shared Function SolveN(a As Double()(), b As Double(), Optional epsilon As Double = 1.0E-14) As Double()
            If a Is Nothing Then Throw New ArgumentNullException(NameOf(a))
            If b Is Nothing Then Throw New ArgumentNullException(NameOf(b))

            Dim n As Integer = a.Length

            If b.Length <> n Then
                Throw New ArgumentException($"the right hand side length ({b.Length}) should equal the matrix dimension ({n})!", NameOf(b))
            End If
            If n = 0 Then Return Array.Empty(Of Double)()

            For i As Integer = 0 To n - 1
                If a(i) Is Nothing OrElse a(i).Length <> n Then
                    Throw New ArgumentException($"the matrix should be {n}x{n}!", NameOf(a))
                End If
            Next

            ' 复制一份，避免就地修改调用方的矩阵（调用方往往还要用它算协方差/信息矩阵）
            Dim m As Double()() = New Double(n - 1)() {}
            Dim rhs As Double() = New Double(n - 1) {}

            For i As Integer = 0 To n - 1
                Dim row As Double() = New Double(n - 1) {}

                Array.Copy(a(i), row, n)
                m(i) = row
                rhs(i) = b(i)
            Next

            For col As Integer = 0 To n - 1
                Dim pivot As Integer = col
                Dim best As Double = std.Abs(m(col)(col))

                For r As Integer = col + 1 To n - 1
                    Dim v As Double = std.Abs(m(r)(col))

                    If v > best Then
                        best = v
                        pivot = r
                    End If
                Next

                If best <= epsilon Then
                    Throw New InvalidOperationException($"the linear system is singular or ill-conditioned at column {col} (pivot={best:G3})!")
                End If

                If pivot <> col Then
                    Dim rowSwap As Double() = m(col)

                    m(col) = m(pivot)
                    m(pivot) = rowSwap

                    Dim t As Double = rhs(col)

                    rhs(col) = rhs(pivot)
                    rhs(pivot) = t
                End If

                Dim invPivot As Double = 1.0 / m(col)(col)
                Dim pivotRow As Double() = m(col)

                For r As Integer = col + 1 To n - 1
                    Dim factor As Double = m(r)(col) * invPivot

                    If factor = 0.0 Then Continue For

                    Dim row As Double() = m(r)

                    For c As Integer = col To n - 1
                        row(c) -= factor * pivotRow(c)
                    Next

                    rhs(r) -= factor * rhs(col)
                Next
            Next

            ' 回代
            Dim x As Double() = New Double(n - 1) {}

            For i As Integer = n - 1 To 0 Step -1
                Dim sum As Double = rhs(i)
                Dim row As Double() = m(i)

                For j As Integer = i + 1 To n - 1
                    sum -= row(j) * x(j)
                Next

                x(i) = sum / row(i)
            Next

            Return x
        End Function

        ''' <summary>
        ''' Cholesky 分解：<c>A = L·Lᵀ</c>。
        ''' </summary>
        ''' <returns>矩阵对称正定时返回 <c>True</c>，并把下三角 <c>L</c> 写入 <paramref name="lower"/></returns>
        ''' <remarks>
        ''' 正规方程 <c>Jᵀ·W·J</c> 在理论上总是对称半正定的，用 Cholesky 比高斯消元
        ''' 快一倍（<c>n³/6</c> 次乘加）且不需要选主元；
        ''' 但数值上可能因“弱可观测方向”而失去正定性，此时必须退回带主元的高斯消元，
        ''' 因此调用方要检查返回值。
        ''' </remarks>
        Public Shared Function TryCholesky(a As Double()(), ByRef lower As Double()(),
                                           Optional epsilon As Double = 1.0E-14) As Boolean
            Dim n As Integer = a.Length
            Dim l As Double()() = New Double(n - 1)() {}

            For i As Integer = 0 To n - 1
                l(i) = New Double(n - 1) {}
            Next

            For i As Integer = 0 To n - 1
                For j As Integer = 0 To i
                    Dim sum As Double = a(i)(j)

                    For k As Integer = 0 To j - 1
                        sum -= l(i)(k) * l(j)(k)
                    Next

                    If i = j Then
                        If sum <= epsilon Then
                            lower = Nothing

                            Return False
                        End If

                        l(i)(i) = std.Sqrt(sum)
                    Else
                        l(i)(j) = sum / l(j)(j)
                    End If
                Next
            Next

            lower = l

            Return True
        End Function

        ''' <summary>
        ''' 用 Cholesky 因子求解 <c>L·Lᵀ·x = b</c>。
        ''' </summary>
        Public Shared Function CholeskySolve(lower As Double()(), b As Double()) As Double()
            Dim n As Integer = lower.Length
            Dim y As Double() = New Double(n - 1) {}

            For i As Integer = 0 To n - 1
                Dim sum As Double = b(i)

                For k As Integer = 0 To i - 1
                    sum -= lower(i)(k) * y(k)
                Next

                y(i) = sum / lower(i)(i)
            Next

            Dim x As Double() = New Double(n - 1) {}

            For i As Integer = n - 1 To 0 Step -1
                Dim sum As Double = y(i)

                For k As Integer = i + 1 To n - 1
                    sum -= lower(k)(i) * x(k)
                Next

                x(i) = sum / lower(i)(i)
            Next

            Return x
        End Function

        ''' <summary>
        ''' 对称正定系统求解：优先 Cholesky，失败时退回带主元高斯消元。
        ''' </summary>
        Public Shared Function SolveSymmetric(a As Double()(), b As Double()) As Double()
            Dim lower As Double()()

            If TryCholesky(a, lower) Then
                Return CholeskySolve(lower, b)
            End If

            Return SolveN(a, b)
        End Function

#End Region

#Region "rigid transform estimation"

        ''' <summary>
        ''' 加权质心。
        ''' </summary>
        Public Shared Function WeightedCentroid(points As Vector3d(), weights As Double()) As Vector3d
            If points Is Nothing Then Throw New ArgumentNullException(NameOf(points))

            Dim n As Integer = points.Length

            If n = 0 Then Return Vector3d.Zero

            Dim sx As Double = 0, sy As Double = 0, sz As Double = 0, sw As Double = 0

            For i As Integer = 0 To n - 1
                Dim w As Double = If(weights Is Nothing, 1.0, weights(i))

                sx += points(i).X * w
                sy += points(i).Y * w
                sz += points(i).Z * w
                sw += w
            Next

            If std.Abs(sw) <= Double.Epsilon Then
                Return New Vector3d(sx / n, sy / n, sz / n)
            End If

            Return New Vector3d(sx / sw, sy / sw, sz / sw)
        End Function

        ''' <summary>
        ''' Kabsch/Horn 闭式刚体变换估计：求满足 <c>dst(i) ≈ R·src(i) + t</c> 的最优 <c>(R, t)</c>。
        ''' </summary>
        ''' <param name="src">源点集</param>
        ''' <param name="dst">目标点集（与源点一一对应）</param>
        ''' <param name="weights">可选的逐点权重（例如鲁棒核权重、内点权重），<c>Nothing</c> 表示等权</param>
        ''' <remarks>
        ''' <para>
        ''' 采用 Horn 的四元数解法：先算加权交叉协方差 <c>S = Σ w·(p−cp)(q−cq)ᵀ</c>，
        ''' 再构造 4x4 对称阵 <c>N(S)</c>，其最大特征值对应的特征向量就是最优旋转的四元数。
        ''' 与“对 <c>S</c> 做 SVD 再取 <c>V·Uᵀ</c>”相比，
        ''' 这里的 4x4 幂迭代只需几十次 4x4 乘向量（约 1 千次乘加），
        ''' 而且天然给出<b>真正的旋转</b>（不会出现行列式为 −1 的镜像解，无需额外修正）。
        ''' </para>
        ''' <para>
        ''' <b>退化</b>：三点共线或所有点重合时旋转不可观测，
        ''' 此时返回的旋转在对应自由度上是任意的；调用方应保证输入点集的几何分布
        ''' （这也是 RANSAC 采样要检查三点不共线的原因）。
        ''' </para>
        ''' </remarks>
        Public Shared Function Kabsch(src As Vector3d(), dst As Vector3d(),
                                      Optional weights As Double() = Nothing) As RigidTransform
            If src Is Nothing Then Throw New ArgumentNullException(NameOf(src))
            If dst Is Nothing Then Throw New ArgumentNullException(NameOf(dst))

            Dim n As Integer = src.Length

            If dst.Length <> n Then
                Throw New ArgumentException($"the size of the two point sets not agree: {n} vs {dst.Length}!", NameOf(dst))
            End If
            If n < 3 Then
                Throw New ArgumentException("at least 3 point pairs are required to estimate a rigid transform!", NameOf(src))
            End If
            If weights IsNot Nothing AndAlso weights.Length < n Then
                Throw New ArgumentException($"the weight array is too small: {weights.Length} < {n}!", NameOf(weights))
            End If

            Dim cs As Vector3d = WeightedCentroid(src, weights)
            Dim cd As Vector3d = WeightedCentroid(dst, weights)

            ' 交叉协方差 S = Σ w·(p−cs)(q−cd)ᵀ
            Dim sxx As Double = 0, sxy As Double = 0, sxz As Double = 0
            Dim syx As Double = 0, syy As Double = 0, syz As Double = 0
            Dim szx As Double = 0, szy As Double = 0, szz As Double = 0

            For i As Integer = 0 To n - 1
                Dim w As Double = If(weights Is Nothing, 1.0, weights(i))

                If w = 0.0 Then Continue For

                Dim px As Double = src(i).X - cs.X
                Dim py As Double = src(i).Y - cs.Y
                Dim pz As Double = src(i).Z - cs.Z
                Dim qx As Double = dst(i).X - cd.X
                Dim qy As Double = dst(i).Y - cd.Y
                Dim qz As Double = dst(i).Z - cd.Z

                sxx += w * px * qx
                sxy += w * px * qy
                sxz += w * px * qz
                syx += w * py * qx
                syy += w * py * qy
                syz += w * py * qz
                szx += w * pz * qx
                szy += w * pz * qy
                szz += w * pz * qz
            Next

            Dim n4 As Double()() = {
                New Double() {sxx + syy + szz, syz - szy, szx - sxz, sxy - syx},
                New Double() {syz - szy, sxx - syy - szz, sxy + syx, szx + sxz},
                New Double() {szx - sxz, sxy + syx, -sxx + syy - szz, syz + szy},
                New Double() {sxy - syx, szx + sxz, syz + szy, -sxx - syy + szz}
            }

            Dim q As Double() = DominantEigenvector4(n4)
            Dim quat As New Quaternion(q(0), q(1), q(2), q(3))
            Dim r As Double()() = quat.ToRotationMatrix()
            Dim t As Vector3d = cd - Multiply3(r, cs)

            Return New RigidTransform(r, t)
        End Function

        ''' <summary>
        ''' 4x4 对称矩阵的最大（代数）特征值对应的单位特征向量。
        ''' </summary>
        ''' <remarks>
        ''' <para>
        ''' 用移位幂迭代：先加上一个 Gershgorin 上界 <c>c = MAX_i Σ_j|A(i,j)|</c>，
        ''' 把最小特征值抬到正半轴，于是“绝对值最大”与“代数最大”一致。
        ''' 这是把幂迭代用于 Horn 四元数问题的标准做法。
        ''' </para>
        ''' <para>
        ''' 起始向量取 <c>(1, 1, 1, 1)/2</c>（与所有主方向都不正交，避免恰好落在
        ''' 某个不收敛的子空间里），迭代 128 次并每步归一化；
        ''' 对 4x4 而言这个次数的总代价不到一次 3x3 SVD 的十分之一，
        ''' 而且是<b>确定性</b>的（相同输入永远得到相同结果，便于回归测试对拍）。
        ''' </para>
        ''' </remarks>
        Public Shared Function DominantEigenvector4(m As Double()(), Optional iterations As Integer = 128) As Double()
            Dim n As Integer = m.Length

            If n <> 4 Then
                Throw New ArgumentException($"the matrix should be 4x4, but got {n}x{If(n = 0, 0, m(0).Length)}!", NameOf(m))
            End If

            Dim shift As Double = 0.0

            For i As Integer = 0 To 3
                Dim rowSum As Double = 0.0

                For j As Integer = 0 To 3
                    rowSum += std.Abs(m(i)(j))
                Next

                If rowSum > shift Then shift = rowSum
            Next

            Dim v As Double() = {0.5, 0.5, 0.5, 0.5}
            Dim next4 As Double() = New Double(3) {}

            For it As Integer = 0 To std.Max(1, iterations) - 1
                For i As Integer = 0 To 3
                    Dim sum As Double = 0.0

                    For j As Integer = 0 To 3
                        sum += m(i)(j) * v(j)
                    Next

                    next4(i) = sum + shift * v(i)
                Next

                Dim norm As Double = std.Sqrt(next4(0) ^ 2 + next4(1) ^ 2 + next4(2) ^ 2 + next4(3) ^ 2)

                If norm <= 0.0 Then Exit For

                Dim inv As Double = 1.0 / norm

                For i As Integer = 0 To 3
                    v(i) = next4(i) * inv
                Next
            Next

            Return v
        End Function

#End Region

    End Class
End Namespace
