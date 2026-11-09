Imports Microsoft.VisualBasic.MachineVision.Cloudy.Core
Imports std = System.Math

Namespace Registration

    ''' <summary>
    ''' NDT(Normal Distributions Transform)配准：把目标点云体素化为高斯分布，最大化似然。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' <b>核心思想</b>：把目标点云切成边长为 <c>r</c> 的体素，
    ''' 每个非空体素用均值 <c>μ</c> 与协方差 <c>Σ</c> 描述（局部法向分布）。
    ''' 源点 <c>p</c> 经过变换 <c>T</c> 之后落在体素里的“似然”是
    ''' <c>exp(−½·dᵀΣ⁻¹d)</c>（<c>d = T(p) − μ</c>）。配准即最大化总似然。
    ''' </para>
    ''' <para>
    ''' <b>与 ICP 的差异</b>：
    ''' </para>
    ''' <list type="bullet">
    ''' <item><description>目标函数是<b>光滑</b>的高斯混合，不依赖离散的“最近点”，因此对初值更鲁棒
    ''' （典型容忍度比 ICP 大 2~3 倍），适合车载激光雷达/大场景。</description></item>
    ''' <item><description>代价函数在体素边界处仍不连续（协方差随体素变化），
    ''' 分辨率 <c>r</c> 的选择直接影响收敛域；工程上常用“粗到细”的多分辨率策略。</description></item>
    ''' <item><description>比 ICP 慢（每次迭代要评估全部点 × 3x3 矩阵运算），
    ''' 但它对离群点不敏感（落在空体素的点直接被跳过）。</description></item>
    ''' </list>
    ''' <para>
    ''' <b>优化</b>：这里使用 Gauss-Newton 近似（忽略 <c>exp</c> 的二阶项），
    ''' 每轮解 6x6 的 <c>H·Δ = −g</c>，参数化为 <c>(ω, t)</c>（小角度旋转 + 平移）。
    ''' 这是 NDT 的常用工程实现，收敛性略弱于完整 Newton，但每轮省掉一半的矩阵运算，
    ''' 且数值上更稳定（Hessian 在近简并处可能非正定）。
    ''' </para>
    ''' </remarks>
    Public NotInheritable Class NdtRegistration

        Private Sub New()
        End Sub

        ''' <summary>NDT 参数。</summary>
        Public Class NdtOptions

            ''' <summary>体素边长；<c>≤ 0</c> 表示按目标点距自适应（10 倍点距）。</summary>
            Public Property Resolution As Double = 0.0

            ''' <summary>最大迭代次数（默认 30）。</summary>
            Public Property MaxIterations As Integer = 30

            ''' <summary>体素内最少点数（少于该值的体素被视为不可用）。</summary>
            Public Property MinPointsPerVoxel As Integer = 6

            ''' <summary>协方差正则化（防止退化体素导致矩阵奇异）。</summary>
            Public Property Regularization As Double = 1.0E-3

            ''' <summary>收敛阈值（旋转角 / 平移）。</summary>
            Public Property Epsilon As Double = 1.0E-6
        End Class

        ''' <summary>
        ''' 执行 NDT 配准。
        ''' </summary>
        Public Shared Function Align(source As PointCloudBuffer, target As PointCloudBuffer,
                                     Optional initial As RigidTransform = Nothing,
                                     Optional options As NdtOptions = Nothing,
                                     Optional optionsCloudy As CloudyOptions = Nothing) As RegistrationResult
            If source Is Nothing Then Throw New ArgumentNullException(NameOf(source))
            If target Is Nothing Then Throw New ArgumentNullException(NameOf(target))

            Dim opt As NdtOptions = If(options, New NdtOptions())
            Dim global_ As CloudyOptions = If(optionsCloudy, CloudyOptions.Default)

            If target.Count < 10 OrElse source.Count < 10 Then
                Throw New ArgumentException("both point clouds should contain at least 10 points for NDT!")
            End If

            Dim resolution As Double = If(opt.Resolution > 0.0, opt.Resolution,
                                          std.Max(target.EstimateAverageSpacing() * 10.0, 1.0E-6))

            Dim cells As Dictionary(Of Long, NdtCell) = BuildCells(target, resolution, opt)
            Dim current As RigidTransform = If(initial, RigidTransform.Identity)

            Dim converged As Boolean = False
            Dim rmse As Double = Double.MaxValue
            Dim fitness As Double = 0.0
            Dim iterations As Integer = 0
            Dim previousScore As Double = Double.MinValue

            For iteration As Integer = 1 To std.Max(1, opt.MaxIterations)
                iterations = iteration

                Dim h As Double()() = Zeros()
                Dim b As Double() = New Double(5) {}
                Dim score As Double = 0.0
                Dim matched As Integer = 0
                Dim weightedResidual As Double = 0.0

                For i As Integer = 0 To source.Count - 1
                    Dim p As Vector3d = current.TransformPoint(source.Item(i))
                    Dim key As Long = VoxelHash.PackKey(CInt(std.Floor(p.X / resolution)),
                                                        CInt(std.Floor(p.Y / resolution)),
                                                        CInt(std.Floor(p.Z / resolution)))
                    Dim cell As NdtCell = Nothing

                    If Not cells.TryGetValue(key, cell) Then Continue For

                    Dim d As New Vector3d(p.X - cell.Mean.X, p.Y - cell.Mean.Y, p.Z - cell.Mean.Z)
                    Dim md As Vector3d = cell.ApplyInverseTimes(d)

                    ' exp(-½·dᵀΣ⁻¹d)，指数截断避免下溢
                    Dim exponent As Double = -0.5 * Vector3d.Dot(d, md)

                    If exponent < -700.0 Then Continue For

                    Dim weight As Double = std.Exp(exponent)

                    score += weight
                    matched += 1
                    weightedResidual += Vector3d.Dot(d, md)

                    ' J = [ −[p]× , I ]
                    Dim jacobian As Double()() = {
                        New Double() {0, p.Z, -p.Y, 1, 0, 0},
                        New Double() {-p.Z, 0, p.X, 0, 1, 0},
                        New Double() {p.Y, -p.X, 0, 0, 0, 1}
                    }

                    ' g += weight · Jᵀ·Σ⁻¹·d ；H += weight · Jᵀ·Σ⁻¹·J
                    Dim mj As Double()() = Multiply3(cell.Inverse, jacobian)
                    Dim md3 As Double() = {md.X, md.Y, md.Z}
                    Dim jtmd As Double() = MultiplyTransposed(jacobian, md3)

                    For r As Integer = 0 To 5
                        b(r) += weight * jtmd(r)

                        For c As Integer = 0 To 5
                            Dim sum As Double = 0.0

                            For k As Integer = 0 To 2
                                sum += jacobian(k)(r) * mj(k)(c)
                            Next

                            h(r)(c) += weight * sum
                        Next
                    Next
                Next

                If matched < 10 Then Exit For

                fitness = CDbl(matched) / source.Count
                rmse = std.Sqrt(std.Max(0.0, weightedResidual) / matched)

                Dim delta As Double()

                Try
                    delta = Linalg.SolveSymmetric(h, Negate(b))
                Catch ex As InvalidOperationException
                    Exit For
                End Try

                Dim omega As New Vector3d(delta(0), delta(1), delta(2))
                Dim translation As New Vector3d(delta(3), delta(4), delta(5))
                Dim angle As Double = omega.Length
                Dim increment As RigidTransform

                If angle > 1.0E-15 Then
                    increment = RigidTransform.FromAxisAngle(omega / angle, angle, translation)
                Else
                    increment = RigidTransform.FromTranslation(translation)
                End If

                current = increment.Compose(current)

                Dim delScore As Double = std.Abs(score - previousScore)

                previousScore = score

                If angle < opt.Epsilon AndAlso translation.Length < opt.Epsilon Then
                    converged = True

                    Exit For
                End If
                If delScore < 1.0E-9 * std.Max(1.0, std.Abs(score)) AndAlso iteration > 3 Then
                    converged = True

                    Exit For
                End If
            Next

            Return New RegistrationResult(current, fitness, rmse, iterations, converged, "NDT")
        End Function

        ''' <summary>
        ''' 目标点云的体素高斯分布：均值、协方差与协方差逆。
        ''' </summary>
        Private NotInheritable Class NdtCell

            Public Property Mean As Vector3d
            Public Property Covariance As Double()()
            Public Property Inverse As Double()()

            Public Function ApplyInverseTimes(v As Vector3d) As Vector3d
                Return New Vector3d(
                    Inverse(0)(0) * v.X + Inverse(0)(1) * v.Y + Inverse(0)(2) * v.Z,
                    Inverse(1)(0) * v.X + Inverse(1)(1) * v.Y + Inverse(1)(2) * v.Z,
                    Inverse(2)(0) * v.X + Inverse(2)(1) * v.Y + Inverse(2)(2) * v.Z)
            End Function
        End Class

        Private Shared Function BuildCells(target As PointCloudBuffer, resolution As Double,
                                           opt As NdtOptions) As Dictionary(Of Long, NdtCell)
            Dim hash As New VoxelHash(target, resolution)
            Dim cells As New Dictionary(Of Long, NdtCell)()

            For Each entry As KeyValuePair(Of Long, List(Of Integer)) In hash.AllCells()
                Dim members As List(Of Integer) = entry.Value

                If members.Count < opt.MinPointsPerVoxel Then Continue For

                Dim n As Integer = members.Count
                Dim mx As Double = 0, my As Double = 0, mz As Double = 0

                For i As Integer = 0 To n - 1
                    Dim p As Integer = members(i)

                    mx += target.X(p)
                    my += target.Y(p)
                    mz += target.Z(p)
                Next

                mx /= n
                my /= n
                mz /= n

                Dim cxx As Double = 0, cxy As Double = 0, cxz As Double = 0
                Dim cyy As Double = 0, cyz As Double = 0, czz As Double = 0

                For i As Integer = 0 To n - 1
                    Dim p As Integer = members(i)
                    Dim dx As Double = target.X(p) - mx
                    Dim dy As Double = target.Y(p) - my
                    Dim dz As Double = target.Z(p) - mz

                    cxx += dx * dx
                    cxy += dx * dy
                    cxz += dx * dz
                    cyy += dy * dy
                    cyz += dy * dz
                    czz += dz * dz
                Next

                Dim inv As Double = 1.0 / n
                Dim eps As Double = opt.Regularization

                Dim covariance As Double()() = {
                    New Double() {cxx * inv + eps, cxy * inv, cxz * inv},
                    New Double() {cxy * inv, cyy * inv + eps, cyz * inv},
                    New Double() {cxz * inv, cyz * inv, czz * inv + eps}
                }

                Dim inverse As Double()()

                If Not Linalg.TryInverse3(covariance, inverse) Then Continue For

                cells(entry.Key) = New NdtCell With {
                    .Mean = New Vector3d(mx, my, mz),
                    .Covariance = covariance,
                    .Inverse = inverse
                }
            Next

            Return cells
        End Function

        Private Shared Function Multiply3(m As Double()(), j As Double()()) As Double()()
            Dim result As Double()() = New Double(2)() {}

            For i As Integer = 0 To 2
                Dim row As Double() = New Double(5) {}

                For c As Integer = 0 To 5
                    row(c) = m(i)(0) * j(0)(c) + m(i)(1) * j(1)(c) + m(i)(2) * j(2)(c)
                Next

                result(i) = row
            Next

            Return result
        End Function

        Private Shared Function MultiplyTransposed(j As Double()(), v As Double()) As Double()
            Dim out(5) As Double

            For r As Integer = 0 To 5
                out(r) = j(0)(r) * v(0) + j(1)(r) * v(1) + j(2)(r) * v(2)
            Next

            Return out
        End Function

        Private Shared Function Zeros() As Double()()
            Dim m As Double()() = New Double(5)() {}

            For i As Integer = 0 To 5
                m(i) = New Double(5) {}
            Next

            Return m
        End Function

        Private Shared Function Negate(v As Double()) As Double()
            Dim out(v.Length - 1) As Double

            For i As Integer = 0 To v.Length - 1
                out(i) = -v(i)
            Next

            Return out
        End Function

    End Class
End Namespace
