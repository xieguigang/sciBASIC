Imports Microsoft.VisualBasic.MachineVision.Cloudy.Core
Imports Microsoft.VisualBasic.MachineVision.Cloudy.Features
Imports Microsoft.VisualBasic.MachineVision.Cloudy.Preprocessing
Imports std = System.Math

Namespace Registration

    ''' <summary>
    ''' 基于 FPFH 特征对应 + RANSAC 的粗配准。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' <b>解决的问题</b>：两站点云之间可能存在数十度的旋转与数十厘米的平移，
    ''' ICP 在这种初值下必然收敛到错误的局部极小。粗配准要在没有初值的情况下
    ''' 给出一个“足够接近”的估计（通常要求旋转误差 &lt; 10°、平移误差 &lt; 特征尺度）。
    ''' </para>
    ''' <para>
    ''' <b>流程</b>：
    ''' </para>
    ''' <list type="number">
    ''' <item><description>降采样（控制后续成本）→ 关键点检测（ISS，按得分截取前 N 个）；</description></item>
    ''' <item><description>计算关键点的 FPFH 描述子 → 匹配（比值检验 + 双向一致）；</description></item>
    ''' <item><description>RANSAC：随机取 3 个对应求解变换 → 统计内点 → 保留最优；</description></item>
    ''' <item><description>用内点集做一次最小二乘重估（可选，由 <see cref="RansacOptions.RefineInliers"/> 控制）。</description></item>
    ''' </list>
    ''' <para>
    ''' <b>三角形预筛</b>：三点假设如果与目标的对应三点“三角形边长比例”不一致，
    ''' 无论变换如何都不可能对齐。先用边长比值做一次 O(1) 的预筛，
    ''' 可以丢掉绝大多数错误的采样组合，把有效迭代次数提高一个数量级。
    ''' </para>
    ''' <para>
    ''' <b>为什么用 3 点而不是 4 点</b>：3 点足以唯一确定刚体变换（不共线时），
    ''' 采样空间更小、命中率更高；4 点在 4PCS 里用于“全等四点集”的全局搜索，是另一条技术路线。
    ''' </para>
    ''' <para>
    ''' <b>确定性</b>：随机采样使用显式种子（<see cref="RansacOptions.Seed"/>），
    ''' 相同输入必然得到相同结果，便于回归测试与检测报告复现。
    ''' </para>
    ''' </remarks>
    Public NotInheritable Class RansacRegistration

        Private Sub New()
        End Sub

        ''' <summary>RANSAC 粗配准参数。</summary>
        Public Class RansacOptions

            ''' <summary>RANSAC 最大迭代次数（默认 2000）。</summary>
            Public Property MaxIterations As Integer = 2000

            ''' <summary>关键点数量上限（默认 1500；越多越稳但越慢）。</summary>
            Public Property MaxKeypoints As Integer = 1500

            ''' <summary>内点距离阈值；<c>≤ 0</c> 表示按点距自适应（3 倍）。</summary>
            Public Property InlierDistance As Double = 0.0

            ''' <summary>内点率达标即提前终止（默认 0.9）。</summary>
            Public Property EarlyStopFitness As Double = 0.9

            ''' <summary>Lowe 比值阈值（默认 0.9）。</summary>
            Public Property RatioThreshold As Double = 0.9

            ''' <summary>三角形边长比例的容差（默认 0.15，即 15%）。</summary>
            Public Property EdgeLengthTolerance As Double = 0.15

            ''' <summary>是否用内点集做最终的最小二乘重估（默认 <c>True</c>）。</summary>
            Public Property RefineInliers As Boolean = True

            ''' <summary>随机种子（<c>0</c> 表示使用默认种子）。</summary>
            Public Property Seed As Integer = 0

            ''' <summary>是否对输入做体素降采样以控制成本（默认 <c>True</c>）。</summary>
            Public Property Downsample As Boolean = True

            ''' <summary>降采样体素尺寸；<c>≤ 0</c> 表示按点距自适应（2 倍）。</summary>
            Public Property VoxelSize As Double = 0.0
        End Class

        ''' <summary>
        ''' 执行粗配准（源 → 目标）。
        ''' </summary>
        Public Shared Function Align(source As PointCloudBuffer, target As PointCloudBuffer,
                                     Optional options As RansacOptions = Nothing,
                                     Optional optionsCloudy As CloudyOptions = Nothing) As RegistrationResult
            If source Is Nothing Then Throw New ArgumentNullException(NameOf(source))
            If target Is Nothing Then Throw New ArgumentNullException(NameOf(target))

            Dim opt As RansacOptions = If(options, New RansacOptions())
            Dim global_ As CloudyOptions = If(optionsCloudy, CloudyOptions.Default)

            If source.Count < 10 OrElse target.Count < 10 Then
                Throw New ArgumentException("both point clouds should contain at least 10 points for RANSAC registration!")
            End If

            ' 1) 降采样 + 法向（FPFH 依赖法向）
            Dim src As PointCloudBuffer = source
            Dim tar As PointCloudBuffer = target

            If opt.Downsample Then
                Dim vs As Double = If(opt.VoxelSize > 0.0, opt.VoxelSize,
                                      std.Max(source.EstimateAverageSpacing() * 2.0, 1.0E-6))

                src = VoxelGridFilter.Downsample(src, vs, global_)
                tar = VoxelGridFilter.Downsample(tar, vs, global_)
            End If

            ' 2) 关键点
            Dim srcKeypoints As KeypointSet = DetectKeypoints(src, opt, global_)
            Dim tarKeypoints As KeypointSet = DetectKeypoints(tar, opt, global_)

            If srcKeypoints.Count < 6 OrElse tarKeypoints.Count < 6 Then
                Return New RegistrationResult(RigidTransform.Identity, 0.0, Double.MaxValue, 0, False, "RANSAC(FPFH): insufficient keypoints")
            End If

            ' 3) 描述子与匹配
            Dim srcFeatures As FeatureCloud = FpfhDescriptor.Compute(src, srcKeypoints.Indices, 0.0, 0.0, global_)
            Dim tarFeatures As FeatureCloud = FpfhDescriptor.Compute(tar, tarKeypoints.Indices, 0.0, 0.0, global_)

            Dim matches As FeatureMatching.Correspondence() =
                FeatureMatching.Match(srcFeatures, tarFeatures, opt.RatioThreshold, True, global_)

            If matches.Length < 4 Then
                Return New RegistrationResult(RigidTransform.Identity, 0.0, Double.MaxValue, 0, False, "RANSAC(FPFH): insufficient feature matches")
            End If

            Dim srcIdx(matches.Length - 1) As Integer
            Dim tarIdx(matches.Length - 1) As Integer

            For i As Integer = 0 To matches.Length - 1
                srcIdx(i) = srcFeatures.Keypoints.Indices(matches(i).SourceIndex)
                tarIdx(i) = tarFeatures.Keypoints.Indices(matches(i).TargetIndex)
            Next

            Dim inlierDistance As Double = If(opt.InlierDistance > 0.0, opt.InlierDistance,
                                              std.Max(tar.EstimateAverageSpacing() * 3.0, 1.0E-6))

            Dim rnd As Random = If(opt.Seed = 0, global_.CreateRandom(31), New Random(opt.Seed))

            Dim tree As New KdTree3D(tar)
            Dim bestTransform As RigidTransform = RigidTransform.Identity
            Dim bestInliers As Integer = 0
            Dim bestRmse As Double = Double.MaxValue

            ' 预取对应点的坐标，避免循环里反复调用 Item
            Dim srcPoints(matches.Length - 1) As Vector3d
            Dim tarPoints(matches.Length - 1) As Vector3d

            For i As Integer = 0 To matches.Length - 1
                srcPoints(i) = src.Item(srcIdx(i))
                tarPoints(i) = tar.Item(tarIdx(i))
            Next

            ' 验证用的源点（最多取一部分，控制单次假设的验证成本）
            Dim verifyCount As Integer = std.Min(src.Count, 1500)
            Dim verifyIndices(verifyCount - 1) As Integer
            Dim stride As Double = CDbl(src.Count) / verifyCount

            For i As Integer = 0 To verifyCount - 1
                verifyIndices(i) = std.Min(src.Count - 1, CInt(std.Floor(i * stride)))
            Next

            For iteration As Integer = 1 To std.Max(1, opt.MaxIterations)
                Dim a As Integer = rnd.Next(matches.Length)
                Dim b As Integer = rnd.Next(matches.Length)
                Dim c As Integer = rnd.Next(matches.Length)

                If a = b OrElse b = c OrElse a = c Then Continue For

                ' 三角形预筛：边长比例必须匹配
                Dim sab As Double = (srcPoints(a) - srcPoints(b)).Length
                Dim sbc As Double = (srcPoints(b) - srcPoints(c)).Length
                Dim sca As Double = (srcPoints(c) - srcPoints(a)).Length

                If sab < 1.0E-9 OrElse sbc < 1.0E-9 OrElse sca < 1.0E-9 Then Continue For

                Dim tab As Double = (tarPoints(a) - tarPoints(b)).Length
                Dim tbc As Double = (tarPoints(b) - tarPoints(c)).Length
                Dim tca As Double = (tarPoints(c) - tarPoints(a)).Length

                If Not LengthRatioOk(sab, tab, opt.EdgeLengthTolerance) Then Continue For
                If Not LengthRatioOk(sbc, tbc, opt.EdgeLengthTolerance) Then Continue For
                If Not LengthRatioOk(sca, tca, opt.EdgeLengthTolerance) Then Continue For

                Dim hypothesis As RigidTransform

                Try
                    hypothesis = Linalg.Kabsch({srcPoints(a), srcPoints(b), srcPoints(c)},
                                               {tarPoints(a), tarPoints(b), tarPoints(c)})
                Catch ex As ArgumentException
                    Continue For
                End Try

                Dim inliers As Integer = 0
                Dim sumSquared As Double = 0.0

                For k As Integer = 0 To verifyCount - 1
                    Dim p As Vector3d = hypothesis.TransformPoint(src.Item(verifyIndices(k)))
                    Dim nearest As (index As Integer, distanceSquared As Double) = tree.Nearest(p.X, p.Y, p.Z)

                    If nearest.index < 0 Then Continue For
                    If nearest.distanceSquared > inlierDistance * inlierDistance Then Continue For

                    inliers += 1
                    sumSquared += nearest.distanceSquared
                Next

                If inliers > bestInliers OrElse (inliers = bestInliers AndAlso sumSquared < bestRmse * bestInliers) Then
                    bestInliers = inliers
                    bestRmse = If(inliers > 0, std.Sqrt(sumSquared / inliers), Double.MaxValue)
                    bestTransform = hypothesis
                End If

                If CDbl(bestInliers) / verifyCount >= opt.EarlyStopFitness Then Exit For
            Next

            Dim fitness As Double = CDbl(bestInliers) / std.Max(1, verifyCount)

            ' 4) 用内点集重估
            If opt.RefineInliers AndAlso bestInliers >= 6 Then
                Dim inlierSrc As New List(Of Vector3d)()
                Dim inlierTar As New List(Of Vector3d)()

                For k As Integer = 0 To verifyCount - 1
                    Dim p As Vector3d = bestTransform.TransformPoint(src.Item(verifyIndices(k)))
                    Dim nearest As (index As Integer, distanceSquared As Double) = tree.Nearest(p.X, p.Y, p.Z)

                    If nearest.index < 0 Then Continue For
                    If nearest.distanceSquared > inlierDistance * inlierDistance Then Continue For

                    Call inlierSrc.Add(src.Item(verifyIndices(k)))
                    Call inlierTar.Add(tar.Item(nearest.index))
                Next

                If inlierSrc.Count >= 6 Then
                    bestTransform = TransformEstimation.EstimateRobust(inlierSrc.ToArray(), inlierTar.ToArray(), 2, 0.9).Transform
                End If
            End If

            Return New RegistrationResult(bestTransform, fitness, bestRmse, opt.MaxIterations, bestInliers >= 6, "RANSAC(FPFH)")
        End Function

        Private Shared Function LengthRatioOk(a As Double, b As Double, tolerance As Double) As Boolean
            Dim larger As Double = std.Max(a, b)
            Dim smaller As Double = std.Min(a, b)

            If larger <= 0.0 Then Return False

            Return (larger - smaller) / larger <= tolerance
        End Function

        ''' <summary>
        ''' 关键点检测：优先 ISS；点数不足时退回“均匀抽样”。
        ''' </summary>
        Private Shared Function DetectKeypoints(cloud As PointCloudBuffer, opt As RansacOptions,
                                                options As CloudyOptions) As KeypointSet
            Dim detected As KeypointSet = IssKeypoint.Detect(cloud, 0.0, 0.0, 0.975, 0.975, 6, options)

            If detected.Count >= 10 Then
                If detected.Count <= opt.MaxKeypoints Then Return detected

                Dim indices(opt.MaxKeypoints - 1) As Integer
                Dim scores(opt.MaxKeypoints - 1) As Double

                For i As Integer = 0 To opt.MaxKeypoints - 1
                    indices(i) = detected.Indices(i)
                    scores(i) = detected.Scores(i)
                Next

                Return New KeypointSet(indices, scores)
            End If

            ' ISS 在平面/低纹理数据上可能检不出足够关键点，此时用均匀抽样保底
            Dim count As Integer = std.Min(opt.MaxKeypoints, cloud.Count)
            Dim stride As Double = CDbl(cloud.Count) / std.Max(1, count)
            Dim fallbackIndices(count - 1) As Integer
            Dim fallbackScores(count - 1) As Double

            For i As Integer = 0 To count - 1
                fallbackIndices(i) = std.Min(cloud.Count - 1, CInt(std.Floor(i * stride)))
            Next

            Return New KeypointSet(fallbackIndices, fallbackScores)
        End Function

    End Class
End Namespace
