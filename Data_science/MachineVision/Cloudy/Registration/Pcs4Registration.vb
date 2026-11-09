Imports Microsoft.VisualBasic.MachineVision.Cloudy.Core
Imports Microsoft.VisualBasic.MachineVision.Cloudy.Preprocessing
Imports std = System.Math

Namespace Registration

    ''' <summary>
    ''' 4PCS(4-Points Congruent Sets)全局粗配准。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' <b>核心不变量</b>：给定<b>共面</b>的四点 <c>{a, b, c, d}</c>，
    ''' 两条对角线 <c>ac</c> 与 <c>bd</c> 交于点 <c>e</c>，
    ''' 则比值
    ''' </para>
    ''' <code>
    ''' r1 = ‖a − e‖ / ‖a − c‖      r2 = ‖b − e‖ / ‖b − d‖
    ''' </code>
    ''' <para>
    ''' 在刚体变换下<b>保持不变</b>。因此在目标点云中，
    ''' 只要找到“同样满足这两个比值、且四点距离一致”的四点集，
    ''' 就得到一个候选对应，可直接求刚体变换。
    ''' </para>
    ''' <para>
    ''' <b>与 FPFH+RANSAC 的差别</b>：4PCS 不依赖描述子，因此对
    ''' “纹理单一、重复结构多”的金属件更可靠（描述子会在重复结构上大量误匹配）；
    ''' 代价是搜索空间大，需要靠参数控制耗时。
    ''' </para>
    ''' <para>
    ''' <b>本实现的简化（必须知悉）</b>：
    ''' </para>
    ''' <list type="bullet">
    ''' <item><description>目标是“随机采样 + 一致性验证”，而不是原论文的
    ''' “按仿射不变量的哈希索引全部点对”检索；因此命中率依赖采样次数，
    ''' 通过 <see cref="Pcs4Options.BaseSets"/> 与 <see cref="Pcs4Options.PairTrials"/> 调节。</description></item>
    ''' <item><description>没有实现 Super4PCS 的“智能索引 + 近似不变量”，因此大规模点云上会更慢；
    ''' 工程上建议先降采样到 1e4 点量级再调用。</description></item>
    ''' <item><description>四点共面性通过“到平面距离 &lt; 容差”筛选，退化时自动换点重采样。</description></item>
    ''' </list>
    ''' </remarks>
    Public NotInheritable Class Pcs4Registration

        Private Sub New()
        End Sub

        ''' <summary>4PCS 参数。</summary>
        Public Class Pcs4Options

            ''' <summary>源点云的基础四点集采样次数（默认 80）。</summary>
            Public Property BaseSets As Integer = 80

            ''' <summary>每个基础四点集在目标中的点对尝试次数（默认 800）。</summary>
            Public Property PairTrials As Integer = 800

            ''' <summary>四点共面容差；<c>≤ 0</c> 表示自适应（工作点距的 2 倍）。</summary>
            Public Property CoplanarTolerance As Double = 0.0

            ''' <summary>四点集全等判定的距离容差；<c>≤ 0</c> 表示自适应（基准尺度的 5%）。</summary>
            Public Property CongruenceTolerance As Double = 0.0

            ''' <summary>内点距离阈值；<c>≤ 0</c> 表示自适应（3 倍点距）。</summary>
            Public Property InlierDistance As Double = 0.0

            ''' <summary>验证假设时使用的源点数量上限。</summary>
            Public Property MaxVerifyPoints As Integer = 1200

            ''' <summary>随机种子（<c>0</c> 表示使用默认种子）。</summary>
            Public Property Seed As Integer = 0
        End Class

        ''' <summary>
        ''' 执行 4PCS 全局配准。
        ''' </summary>
        Public Shared Function Align(source As PointCloudBuffer, target As PointCloudBuffer,
                                     Optional options As Pcs4Options = Nothing,
                                     Optional optionsCloudy As CloudyOptions = Nothing) As RegistrationResult
            If source Is Nothing Then Throw New ArgumentNullException(NameOf(source))
            If target Is Nothing Then Throw New ArgumentNullException(NameOf(target))

            Dim opt As Pcs4Options = If(options, New Pcs4Options())
            Dim global_ As CloudyOptions = If(optionsCloudy, CloudyOptions.Default)

            If source.Count < 20 OrElse target.Count < 20 Then
                Throw New ArgumentException("both point clouds should contain at least 20 points for 4PCS!")
            End If

            ' 工作点云：降采样到 1e4 量级，兼顾搜索空间与耗时
            Dim vs As Double = std.Max(source.EstimateAverageSpacing() * 3.0, 1.0E-6)
            Dim srcWork As PointCloudBuffer = VoxelGridFilter.Downsample(source, vs, global_)
            Dim tarWork As PointCloudBuffer = VoxelGridFilter.Downsample(target, vs, global_)

            If srcWork.Count < 20 OrElse tarWork.Count < 20 Then
                srcWork = source
                tarWork = target
            End If

            Dim spacing As Double = std.Max(tarWork.EstimateAverageSpacing(), 1.0E-9)
            Dim coplanarTolerance As Double = If(opt.CoplanarTolerance > 0.0, opt.CoplanarTolerance, spacing * 2.0)
            Dim inlierDistance As Double = If(opt.InlierDistance > 0.0, opt.InlierDistance, spacing * 3.0)

            Dim rnd As Random = If(opt.Seed = 0, global_.CreateRandom(4441), New Random(opt.Seed))

            Dim tarTree As New KdTree3D(tarWork)
            Dim verifyCount As Integer = std.Min(srcWork.Count, opt.MaxVerifyPoints)
            Dim stride As Double = CDbl(srcWork.Count) / verifyCount
            Dim verifyPoints(verifyCount - 1) As Vector3d

            For i As Integer = 0 To verifyCount - 1
                verifyPoints(i) = srcWork.Item(std.Min(srcWork.Count - 1, CInt(std.Floor(i * stride))))
            Next

            Dim bestTransform As RigidTransform = RigidTransform.Identity
            Dim bestInliers As Integer = 0
            Dim bestRmse As Double = Double.MaxValue

            For baseSet As Integer = 1 To std.Max(1, opt.BaseSets)
                Dim basePoints As Vector3d() = SampleCoplanarSet(srcWork, rnd, coplanarTolerance)

                If basePoints Is Nothing Then Continue For

                Dim baseline As Double = std.Max((basePoints(0) - basePoints(1)).Length,
                                                 std.Max((basePoints(1) - basePoints(2)).Length,
                                                         (basePoints(2) - basePoints(3)).Length))
                Dim congruence As Double = If(opt.CongruenceTolerance > 0.0,
                                              opt.CongruenceTolerance,
                                              std.Max(baseline * 0.05, spacing * 0.5))
                Dim r1 As Double, r2 As Double

                If Not TryRatios(basePoints(0), basePoints(2), basePoints(1), basePoints(3), r1, r2, coplanarTolerance) Then
                    Continue For
                End If

                Dim baseDistances As Double() = PairwiseDistances(basePoints)

                For trial As Integer = 1 To std.Max(1, opt.PairTrials)
                    Dim candidate As Vector3d() = TryFindCongruentSet(tarWork, rnd, r1, r2, basePoints, baseDistances,
                                                                      coplanarTolerance, congruence)

                    If candidate Is Nothing Then Continue For

                    Dim hypothesis As RigidTransform

                    Try
                        hypothesis = Linalg.Kabsch(basePoints, candidate)
                    Catch ex As ArgumentException
                        Continue For
                    End Try

                    Dim inliers As Integer = 0
                    Dim sumSquared As Double = 0.0

                    For k As Integer = 0 To verifyCount - 1
                        Dim p As Vector3d = hypothesis.TransformPoint(verifyPoints(k))
                        Dim nearest As (index As Integer, distanceSquared As Double) = tarTree.Nearest(p.X, p.Y, p.Z)

                        If nearest.index < 0 Then Continue For
                        If nearest.distanceSquared > inlierDistance * inlierDistance Then Continue For

                        inliers += 1
                        sumSquared += nearest.distanceSquared
                    Next

                    If inliers > bestInliers Then
                        bestInliers = inliers
                        bestRmse = If(inliers > 0, std.Sqrt(sumSquared / inliers), Double.MaxValue)
                        bestTransform = hypothesis
                    End If
                Next

                If CDbl(bestInliers) / verifyCount >= 0.85 Then Exit For
            Next

            Dim fitness As Double = CDbl(bestInliers) / std.Max(1, verifyCount)

            Return New RegistrationResult(bestTransform, fitness, bestRmse,
                                          opt.BaseSets * opt.PairTrials, bestInliers >= 6, "4PCS")
        End Function

        ''' <summary>
        ''' 采样一组近似共面的四点。
        ''' </summary>
        Private Shared Function SampleCoplanarSet(cloud As PointCloudBuffer, rnd As Random,
                                                  tolerance As Double) As Vector3d()
            Dim n As Integer = cloud.Count
            Dim attempts As Integer = 0

            While attempts < 64
                attempts += 1

                Dim ia As Integer = rnd.Next(n)
                Dim ib As Integer = rnd.Next(n)
                Dim ic As Integer = rnd.Next(n)

                If ia = ib OrElse ib = ic OrElse ia = ic Then Continue While

                Dim a As Vector3d = cloud.Item(ia)
                Dim b As Vector3d = cloud.Item(ib)
                Dim c As Vector3d = cloud.Item(ic)
                Dim normal As Vector3d = Vector3d.Cross(b - a, c - a)

                If normal.IsZero(1.0E-9) Then Continue While

                normal = normal.Normalize()

                Dim id As Integer = rnd.Next(n)

                If id = ia OrElse id = ib OrElse id = ic Then Continue While

                Dim d As Vector3d = cloud.Item(id)

                If std.Abs(Vector3d.Dot(normal, d - a)) > tolerance Then Continue While

                Return {a, b, c, d}
            End While

            Return Nothing
        End Function

        ''' <summary>
        ''' 计算共面四点对角线交点的两个仿射比值。
        ''' </summary>
        ''' <returns>两条对角线近似相交时返回 <c>True</c></returns>
        Private Shared Function TryRatios(a As Vector3d, c As Vector3d, b As Vector3d, d As Vector3d,
                                          ByRef r1 As Double, ByRef r2 As Double, tolerance As Double) As Boolean
            Dim u As Vector3d = c - a
            Dim v As Vector3d = d - b
            Dim w As Vector3d = a - b
            Dim uu As Double = Vector3d.Dot(u, u)
            Dim vv As Double = Vector3d.Dot(v, v)
            Dim uv As Double = Vector3d.Dot(u, v)
            Dim uw As Double = Vector3d.Dot(u, w)
            Dim vw As Double = Vector3d.Dot(v, w)

            Dim denominator As Double = uu * vv - uv * uv

            If std.Abs(denominator) <= 1.0E-12 Then Return False

            ' 两条直线的最近点参数：e_a = a + λu, e_b = b + μv
            Dim lambda As Double = (uw * vv - vw * uv) / denominator
            Dim mu As Double = (uw * uv - vw * uu) / denominator

            Dim ea As Vector3d = a + u * lambda
            Dim eb As Vector3d = b + v * mu

            If (ea - eb).Length > tolerance Then Return False

            r1 = lambda
            r2 = mu

            Return r1 > 0.0 AndAlso r1 < 1.0 AndAlso r2 > 0.0 AndAlso r2 < 1.0
        End Function

        ''' <summary>
        ''' 在目标点云中寻找与基础四点集全等的四点集。
        ''' </summary>
        Private Shared Function TryFindCongruentSet(cloud As PointCloudBuffer, rnd As Random,
                                                    r1 As Double, r2 As Double,
                                                    basePoints As Vector3d(), baseDistances As Double(),
                                                    coplanarTolerance As Double, congruence As Double) As Vector3d()
            Dim n As Integer = cloud.Count

            ' 两条对角线的候选：各自随机取点对，再比较交点是否重合
            Dim ia As Integer = rnd.Next(n)
            Dim ic As Integer = rnd.Next(n)
            Dim ib As Integer = rnd.Next(n)
            Dim id As Integer = rnd.Next(n)

            If ia = ic OrElse ib = id Then Return Nothing

            Dim a As Vector3d = cloud.Item(ia)
            Dim c As Vector3d = cloud.Item(ic)
            Dim b As Vector3d = cloud.Item(ib)
            Dim d As Vector3d = cloud.Item(id)

            Dim e1 As Vector3d = a + (c - a) * r1
            Dim e2 As Vector3d = b + (d - b) * r2

            If (e1 - e2).Length > coplanarTolerance Then Return Nothing

            Dim candidate As Vector3d() = {a, b, c, d}
            Dim distances As Double() = PairwiseDistances(candidate)

            For i As Integer = 0 To distances.Length - 1
                If std.Abs(distances(i) - baseDistances(i)) > congruence Then Return Nothing
            Next

            Return candidate
        End Function

        ''' <summary>
        ''' 四点之间的六个距离（固定顺序：ab, ac, ad, bc, bd, cd）。
        ''' </summary>
        Private Shared Function PairwiseDistances(p As Vector3d()) As Double()
            Return {
                (p(0) - p(1)).Length,
                (p(0) - p(2)).Length,
                (p(0) - p(3)).Length,
                (p(1) - p(2)).Length,
                (p(1) - p(3)).Length,
                (p(2) - p(3)).Length
            }
        End Function

    End Class
End Namespace
