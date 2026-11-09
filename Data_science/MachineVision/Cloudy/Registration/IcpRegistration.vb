Imports Microsoft.VisualBasic.MachineVision.Cloudy.Core
Imports Microsoft.VisualBasic.MachineVision.Cloudy.Features
Imports std = System.Math

Namespace Registration

    ''' <summary>
    ''' ICP(Iterative Closest Point)精配准：点到点与点到面两种目标函数。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' <b>共同骨架</b>（每一轮）：
    ''' </para>
    ''' <list type="number">
    ''' <item><description>用当前估计把源点云变换到目标坐标系；</description></item>
    ''' <item><description>为每个变换后的点找目标点云中的最近点（KD 树）；</description></item>
    ''' <item><description>丢弃距离超过阈值的对应（外点），可选按比例截尾；</description></item>
    ''' <item><description>求使对应距离平方和最小的刚体变换，更新估计；</description></item>
    ''' <item><description>当变换增量与残差都小于阈值时收敛。</description></item>
    ''' </list>
    ''' <para>
    ''' <b>点到点 vs 点到面</b>：
    ''' </para>
    ''' <list type="bullet">
    ''' <item><description>
    ''' 点到点：目标函数 <c>Σ‖R·p + t − q‖²</c>，有闭式解（<see cref="Linalg.Kabsch(Vector3d(), Vector3d(), Double())"/>）。
    ''' 收敛慢（线性），但不需要法向，对噪声与部分重叠最稳。
    ''' </description></item>
    ''' <item><description>
    ''' 点到面：目标函数 <c>Σ(n_q·(R·p + t − q))²</c>，需要目标法向。
    ''' 它允许点在切平面内滑动，因此在平面上收敛极快（典型的钣金/机加工件，
    ''' 迭代次数常从 30+ 降到 8~12），代价是对法向质量敏感。
    ''' </description></item>
    ''' </list>
    ''' <para>
    ''' <b>初值敏感</b>：ICP 只能收敛到最近的局部极小，因此工业流程必须先用
    ''' 粗配准（<see cref="RansacRegistration"/> / <see cref="Pcs4Registration"/>）
    ''' 把位姿差压到“特征尺度以内”。
    ''' </para>
    ''' <para>
    ''' <b>截尾（TrimRatio &lt; 1）</b>：当两站点云只有部分重叠时，
    ''' 完整 ICP 会被“没有对应”的区域拉偏；只保留残差最小的那部分对应，
    ''' 等价于优化“重叠区域”的对齐，这是多站扫描的标准做法。
    ''' </para>
    ''' </remarks>
    Public NotInheritable Class IcpRegistration

        Private Sub New()
        End Sub

        ''' <summary>
        ''' ICP 参数。
        ''' </summary>
        Public Class IcpOptions

            ''' <summary>最大迭代次数（默认 50）。</summary>
            Public Property MaxIterations As Integer = 50

            ''' <summary>
            ''' 对应点的最大距离；<c>≤ 0</c> 表示按目标点云的平均点距自适应（5 倍）。
            ''' </summary>
            Public Property MaxCorrespondenceDistance As Double = 0.0

            ''' <summary>变换增量收敛阈值（旋转角，弧度）。</summary>
            Public Property RotationEpsilon As Double = 1.0E-8

            ''' <summary>变换增量收敛阈值（平移，与点云同单位）。</summary>
            Public Property TranslationEpsilon As Double = 1.0E-8

            ''' <summary>残差收敛阈值（RMSE 的变化量）。</summary>
            Public Property RmseEpsilon As Double = 1.0E-10

            '''
            ''' 参与优化的对应比例 <c>(0, 1]</c>。小于 1 时只保留残差最小的部分，
            ''' 用于部分重叠的站点。
            '''
            Public Property TrimRatio As Double = 1.0

            ''' <summary>是否要求双向一致（源的最优目标同时也是目标的最优源）。</summary>
            Public Property ReciprocalCorrespondences As Boolean = False

            Public Function Clone() As IcpOptions
                Return New IcpOptions With {
                    .MaxIterations = MaxIterations,
                    .MaxCorrespondenceDistance = MaxCorrespondenceDistance,
                    .RotationEpsilon = RotationEpsilon,
                    .TranslationEpsilon = TranslationEpsilon,
                    .RmseEpsilon = RmseEpsilon,
                    .TrimRatio = TrimRatio,
                    .ReciprocalCorrespondences = ReciprocalCorrespondences
                }
            End Function
        End Class

        ''' <summary>
        ''' 点到点 ICP。
        ''' </summary>
        ''' <param name="source">源点云</param>
        ''' <param name="target">目标点云</param>
        ''' <param name="initial">初始变换（<c>Nothing</c> 表示单位变换）</param>
        ''' <param name="options">参数</param>
        ''' <param name="estimateNormalsForTarget">点到面时是否自动估计目标法向</param>
        Public Shared Function Align(source As PointCloudBuffer, target As PointCloudBuffer,
                                     Optional initial As RigidTransform = Nothing,
                                     Optional options As IcpOptions = Nothing,
                                     Optional optionsCloudy As CloudyOptions = Nothing) As RegistrationResult
            Return AlignPointToPoint(source, target, initial, options, optionsCloudy)
        End Function

        ''' <summary>
        ''' 点到点 ICP（闭式最小二乘更新）。
        ''' </summary>
        Public Shared Function AlignPointToPoint(source As PointCloudBuffer, target As PointCloudBuffer,
                                                 Optional initial As RigidTransform = Nothing,
                                                 Optional options As IcpOptions = Nothing,
                                                 Optional optionsCloudy As CloudyOptions = Nothing) As RegistrationResult
            If source Is Nothing Then Throw New ArgumentNullException(NameOf(source))
            If target Is Nothing Then Throw New ArgumentNullException(NameOf(target))
            If source.Count < 6 OrElse target.Count < 6 Then
                Throw New ArgumentException("both point clouds should contain at least 6 points for ICP!")
            End If

            Dim opt As IcpOptions = If(options, New IcpOptions())
            Dim optGlobal As CloudyOptions = If(optionsCloudy, CloudyOptions.Default)

            Dim maxDistance As Double = If(opt.MaxCorrespondenceDistance > 0.0,
                                           opt.MaxCorrespondenceDistance,
                                           std.Max(target.EstimateAverageSpacing() * 5.0, 1.0E-6))

            Dim tree As New KdTree3D(target)
            Dim current As RigidTransform = If(initial, RigidTransform.Identity)

            Dim buffer As New Neighborhood(1)
            Dim srcList As New List(Of Vector3d)(source.Count)
            Dim dstList As New List(Of Vector3d)(source.Count)
            Dim distList As New List(Of Double)(source.Count)

            Dim converged As Boolean = False
            Dim rmse As Double = Double.MaxValue
            Dim fitness As Double = 0.0
            Dim iterations As Integer = 0

            For iteration As Integer = 1 To std.Max(1, opt.MaxIterations)
                iterations = iteration

                Call srcList.Clear()
                Call dstList.Clear()
                Call distList.Clear()

                For i As Integer = 0 To source.Count - 1
                    Dim p As Vector3d = current.TransformPoint(source.Item(i))
                    Dim nearest As (index As Integer, distanceSquared As Double) =
                        tree.Nearest(p.X, p.Y, p.Z)

                    If nearest.index < 0 Then Continue For
                    If nearest.distanceSquared > maxDistance * maxDistance Then Continue For

                    Call srcList.Add(source.Item(i))
                    Call dstList.Add(target.Item(nearest.index))
                    Call distList.Add(std.Sqrt(nearest.distanceSquared))
                Next

                If srcList.Count < 6 Then Exit For

                Dim keep As Integer = srcList.Count

                If opt.TrimRatio > 0.0 AndAlso opt.TrimRatio < 1.0 Then
                    keep = std.Max(6, CInt(std.Floor(srcList.Count * opt.TrimRatio)))

                    If keep < srcList.Count Then
                        Dim distances As Double() = distList.ToArray()
                        Dim order As Integer() = New Integer(distances.Length - 1) {}

                        For i As Integer = 0 To order.Length - 1
                            order(i) = i
                        Next

                        Array.Sort(DirectCast(distances.Clone(), Double()), order)

                        Dim trimmedSrc(keep - 1) As Vector3d
                        Dim trimmedDst(keep - 1) As Vector3d

                        For k As Integer = 0 To keep - 1
                            trimmedSrc(k) = srcList(order(k))
                            trimmedDst(k) = dstList(order(k))
                        Next

                        srcList.Clear()
                        dstList.Clear()
                        Call srcList.AddRange(trimmedSrc)
                        Call dstList.AddRange(trimmedDst)
                    End If
                End If

                Dim updated As RigidTransform = Linalg.Kabsch(srcList.ToArray(), dstList.ToArray())
                Dim delta As (rotation As Double, translation As Double) = current.DistanceTo(updated)

                current = updated

                Dim sumSquared As Double = 0.0

                For i As Integer = 0 To srcList.Count - 1
                    Dim d As Double = (current.TransformPoint(srcList(i)) - dstList(i)).Length

                    sumSquared += d * d
                Next

                Dim newRmse As Double = std.Sqrt(sumSquared / srcList.Count)

                fitness = CDbl(srcList.Count) / source.Count

                Dim delRmse As Double = std.Abs(rmse - newRmse)

                rmse = newRmse

                If delta.rotation < opt.RotationEpsilon AndAlso delta.translation < opt.TranslationEpsilon Then
                    converged = True

                    Exit For
                End If
                If delRmse < opt.RmseEpsilon AndAlso iteration > 2 Then
                    converged = True

                    Exit For
                End If
            Next

            Return New RegistrationResult(current, fitness, rmse, iterations, converged, "ICP(point-to-point)")
        End Function

        ''' <summary>
        ''' 点到面 ICP（目标法向引导，收敛更快）。
        ''' </summary>
        ''' <param name="source">源点云</param>
        ''' <param name="target">目标点云（缺法向时会自动估计）</param>
        ''' <param name="initial">初始变换</param>
        ''' <param name="options">参数</param>
        ''' <param name="optionsCloudy">并行/计算选项</param>
        Public Shared Function AlignPointToPlane(source As PointCloudBuffer, target As PointCloudBuffer,
                                                 Optional initial As RigidTransform = Nothing,
                                                 Optional options As IcpOptions = Nothing,
                                                 Optional optionsCloudy As CloudyOptions = Nothing) As RegistrationResult
            If source Is Nothing Then Throw New ArgumentNullException(NameOf(source))
            If target Is Nothing Then Throw New ArgumentNullException(NameOf(target))

            Dim opt As IcpOptions = If(options, New IcpOptions())
            Dim optGlobal As CloudyOptions = If(optionsCloudy, CloudyOptions.Default)

            Dim tar As PointCloudBuffer = target

            If Not tar.HasNormals Then
                tar = NormalEstimation.Estimate(target, 20, 0.0, optGlobal)
            End If

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

                Dim h As Double()() = Zeros6x6()
                Dim b As Double() = New Double(5) {}
                Dim count As Integer = 0
                Dim sumSquared As Double = 0.0

                For i As Integer = 0 To source.Count - 1
                    Dim p As Vector3d = current.TransformPoint(source.Item(i))
                    Dim nearest As (index As Integer, distanceSquared As Double) =
                        tree.Nearest(p.X, p.Y, p.Z)

                    If nearest.index < 0 Then Continue For
                    If nearest.distanceSquared > maxDistance * maxDistance Then Continue For

                    Dim q As Vector3d = tar.Item(nearest.index)
                    Dim normal As Vector3d = tar.Normal(nearest.index)

                    If normal.IsZero(1.0E-9) Then Continue For

                    Dim residual As Double = Vector3d.Dot(normal, p - q)

                    ' J = [ (p × n)ᵀ , nᵀ ]  （旋转在前、平移在后）
                    Dim cross As Vector3d = Vector3d.Cross(p, normal)
                    Dim row As Double() = {cross.X, cross.Y, cross.Z, normal.X, normal.Y, normal.Z}

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
                    ' 系统奇异（对应点退化到一条线/一个平面）：停止迭代，返回当前估计
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
                Dim delRmse As Double = std.Abs(rmse - newRmse)

                rmse = newRmse
                fitness = CDbl(count) / source.Count

                If angle < opt.RotationEpsilon AndAlso translation.Length < opt.TranslationEpsilon Then
                    converged = True

                    Exit For
                End If
                If delRmse < opt.RmseEpsilon AndAlso iteration > 2 Then
                    converged = True

                    Exit For
                End If
            Next

            Return New RegistrationResult(current, fitness, rmse, iterations, converged, "ICP(point-to-plane)")
        End Function

        Private Shared Function Zeros6x6() As Double()()
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
