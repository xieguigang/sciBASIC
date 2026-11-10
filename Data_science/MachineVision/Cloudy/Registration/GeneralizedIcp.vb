Imports Microsoft.VisualBasic.MachineVision.Cloudy.Core
Imports Microsoft.VisualBasic.MachineVision.Cloudy.Features
Imports std = System.Math

Namespace Registration

    ''' <summary>
    ''' GICP(Generalized-ICP)：用逐点协方差加权的点到面 ICP。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' <b>原始 GICP 的思想</b>：把每个点看作一个局部高斯分布
    ''' <c>N(μ, C)</c>，两个点云之间的“距离”用分布之间的 Mahalanobis 距离度量：
    ''' </para>
    ''' <code>
    ''' d² = (p − q)ᵀ · (C_q + R·C_p·Rᵀ)⁻¹ · (p − q)
    ''' </code>
    ''' <para>
    ''' 平面邻域的协方差是“薄饼”形（<c>λ1≈λ2≫λ3</c>），
    ''' 这个距离会自动变成“沿法向的点到面距离”，
    ''' 同时把切平面内的不确定性吸收进 <c>λ1, λ2</c>；
    ''' 线状/角点邻域则给出各向异性的斜交度量。
    ''' 因此 GICP 在“平面 + 棱线”混合的零件上比点到面更稳。
    ''' </para>
    ''' <para>
    ''' <b>本实现的简化（必须知悉）</b>：完整形式需要对每个对应点求 3x3 矩阵的逆，
    ''' 成本是点到面的 3~5 倍。这里采用<b>主方向近似</b>：
    ''' 只保留 <c>(C_q + R·C_p·Rᵀ)⁻¹</c> 在法向上的分量，
    ''' 即权重 <c>w = 1/(λ3_p + λ3_q + ε)</c>，配合点到面残差
    ''' <c>n_q·(p − q)</c>。这使得目标函数退化为“加权点到面”，
    ''' 保留了 GICP “平面点权重高、角点权重低”的核心性质，
    ''' 但不等价于完整 Mahalanobis 形式。需要严格复刻时请替换
    ''' <see cref="WeightAt"/> 为完整矩阵求逆。
    ''' </para>
    ''' </remarks>
    Public NotInheritable Class GeneralizedIcp

        Private Sub New()
        End Sub

        ''' <summary>GICP 参数。</summary>
        Public Class GicpOptions

            ''' <summary>协方差估计的邻域点数（默认 20）。</summary>
            Public Property CovarianceK As Integer = 20

            ''' <summary>最大迭代次数（默认 40）。</summary>
            Public Property MaxIterations As Integer = 40

            ''' <summary>对应点最大距离；<c>≤ 0</c> 表示自适应（5 倍点距）。</summary>
            Public Property MaxCorrespondenceDistance As Double = 0.0

            ''' <summary>协方差正则化项（防止奇异，默认 1e-6）。</summary>
            Public Property Regularization As Double = 1.0E-6

            ''' <summary>收敛阈值（旋转角与平移）。</summary>
            Public Property Epsilon As Double = 1.0E-8

            ''' <summary>参与优化的对应比例（部分重叠时用 &lt; 1）。</summary>
            Public Property TrimRatio As Double = 1.0
        End Class

        ''' <summary>
        ''' 执行 GICP 配准。
        ''' </summary>
        ''' <param name="source">源点云</param>
        ''' <param name="target">目标点云（缺法向时自动估计）</param>
        ''' <param name="initial">初始变换</param>
        ''' <param name="options">参数</param>
        ''' <param name="optionsCloudy">并行选项</param>
        Public Shared Function Align(source As PointCloudBuffer, target As PointCloudBuffer,
                                     Optional initial As RigidTransform = Nothing,
                                     Optional options As GicpOptions = Nothing,
                                     Optional optionsCloudy As CloudyOptions = Nothing) As RegistrationResult
            If source Is Nothing Then Throw New ArgumentNullException(NameOf(source))
            If target Is Nothing Then Throw New ArgumentNullException(NameOf(target))

            Dim opt As GicpOptions = If(options, New GicpOptions())
            Dim global_ As CloudyOptions = If(optionsCloudy, CloudyOptions.Default)

            Dim tar As PointCloudBuffer = target

            If Not tar.HasNormals Then tar = NormalEstimation.Estimate(target, 20, 0.0, global_)

            Dim srcWithNormals As PointCloudBuffer = source

            If Not srcWithNormals.HasNormals Then
                srcWithNormals = NormalEstimation.Estimate(source, 20, 0.0, global_)
            End If

            Dim targetVariance As Double() = EstimateMinVariance(tar, opt, global_)
            Dim sourceVariance As Double() = EstimateMinVariance(srcWithNormals, opt, global_)

            Dim maxDistance As Double = If(opt.MaxCorrespondenceDistance > 0.0,
                                           opt.MaxCorrespondenceDistance,
                                           std.Max(tar.EstimateAverageSpacing() * 5.0, 1.0E-6))

            Dim tree As New KdTree3D(tar)
            Dim current As RigidTransform = If(initial, RigidTransform.Identity)

            Dim converged As Boolean = False
            Dim rmse As Double = Double.MaxValue
            Dim fitness As Double = 0.0
            Dim iterations As Integer = 0

            For iteration As Integer = 1 To std.Max(1, opt.MaxIterations)
                iterations = iteration

                Dim h As Double()() = Zeros()
                Dim b As Double() = New Double(5) {}
                Dim count As Integer = 0
                Dim sumSquared As Double = 0.0

                For i As Integer = 0 To source.Count - 1
                    Dim p As Vector3d = current.TransformPoint(source.Item(i))
                    Dim nearest As (index As Integer, distanceSquared As Double) = tree.Nearest(p.X, p.Y, p.Z)

                    If nearest.index < 0 Then Continue For
                    If nearest.distanceSquared > maxDistance * maxDistance Then Continue For

                    Dim q As Vector3d = tar.Item(nearest.index)
                    Dim normal As Vector3d = tar.Normal(nearest.index)

                    If normal.IsZero(1.0E-9) Then Continue For

                    Dim weight As Double = WeightAt(sourceVariance(i), targetVariance(nearest.index), opt.Regularization)
                    Dim scale As Double = std.Sqrt(weight)

                    Dim residual As Double = Vector3d.Dot(normal, p - q) * scale
                    Dim cross As Vector3d = Vector3d.Cross(p, normal)
                    Dim row As Double() = {cross.X * scale, cross.Y * scale, cross.Z * scale,
                                           normal.X * scale, normal.Y * scale, normal.Z * scale}

                    For r As Integer = 0 To 5
                        For c As Integer = 0 To 5
                            h(r)(c) += row(r) * row(c)
                        Next

                        b(r) += row(r) * residual
                    Next

                    sumSquared += residual * residual
                    count += 1
                Next

                If count < 6 Then Exit For

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

                Dim newRmse As Double = std.Sqrt(sumSquared / count)

                fitness = CDbl(count) / source.Count

                Dim delRmse As Double = std.Abs(rmse - newRmse)

                rmse = newRmse

                If angle < opt.Epsilon AndAlso translation.Length < opt.Epsilon Then
                    converged = True

                    Exit For
                End If
                If delRmse < 1.0E-10 AndAlso iteration > 2 Then
                    converged = True

                    Exit For
                End If
            Next

            Return New RegistrationResult(current, fitness, rmse, iterations, converged, "GICP")
        End Function

        ''' <summary>
        ''' 逐点最小特征值（平面方向的方差）：它越小说明邻域越“平”，权重应当越大。
        ''' </summary>
        Private Shared Function EstimateMinVariance(cloud As PointCloudBuffer, opt As GicpOptions,
                                                    options As CloudyOptions) As Double()
            Dim n As Integer = cloud.Count
            Dim variance As Double() = New Double(n - 1) {}
            Dim k As Integer = std.Max(6, opt.CovarianceK)
            Dim tree As New KdTree3D(cloud)
            Dim query As New NeighborhoodQuery(tree, k + 1)

            Call options.ForRange(n,
                Sub(start, ends)
                    Dim buffer As Neighborhood = Nothing

                    For i As Integer = start To ends - 1
                        buffer = query.Knn(i, k)

                        Dim count As Integer = buffer.Count

                        If count < 3 Then
                            variance(i) = 1.0
                            Continue For
                        End If

                        Dim cx As Double = 0, cy As Double = 0, cz As Double = 0

                        For j As Integer = 0 To count - 1
                            Dim p As Integer = buffer.Indices(j)

                            cx += cloud.X(p)
                            cy += cloud.Y(p)
                            cz += cloud.Z(p)
                        Next

                        cx /= count
                        cy /= count
                        cz /= count

                        Dim cxx As Double = 0, cxy As Double = 0, cxz As Double = 0
                        Dim cyy As Double = 0, cyz As Double = 0, czz As Double = 0

                        For j As Integer = 0 To count - 1
                            Dim p As Integer = buffer.Indices(j)
                            Dim dx As Double = cloud.X(p) - cx
                            Dim dy As Double = cloud.Y(p) - cy
                            Dim dz As Double = cloud.Z(p) - cz

                            cxx += dx * dx
                            cxy += dx * dy
                            cxz += dx * dz
                            cyy += dy * dy
                            cyz += dy * dz
                            czz += dz * dz
                        Next

                        Dim inv As Double = 1.0 / count
                        Dim eigen As Linalg.Eigen3 = Linalg.EigenSymmetric3(
                            cxx * inv, cxy * inv, cxz * inv, cyy * inv, cyz * inv, czz * inv)

                        variance(i) = std.Max(eigen.L3, 1.0E-9)
                    Next
                End Sub, 256)

            Return variance
        End Function

        Private Shared Function WeightAt(sourceMin As Double, targetMin As Double, regularization As Double) As Double
            Dim denominator As Double = sourceMin + targetMin + regularization

            Return 1.0 / std.Max(denominator, 1.0E-12)
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
