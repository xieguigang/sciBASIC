Imports Microsoft.VisualBasic.MachineVision.Cloudy.Core
Imports Microsoft.VisualBasic.Math.SIMD
Imports std = System.Math

Namespace Features

    ''' <summary>
    ''' 全局形状描述子：ESF(640 维)与 VFH(768 维)。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' <b>用途</b>：局部描述子（FPFH/SHOT）解决“这个点附近长什么样”，
    ''' 全局描述子解决“整个物体是什么形状”。工业上用于
    ''' ① 物体检索/分类（来料识别）② 粗配准的初筛（先用全局描述子排除不可能的对象）
    ''' ③ 位姿假设验证（整体形状一致性）。
    ''' </para>
    ''' <para>
    ''' <b>ESF(Ensemble of Shape Functions)</b>：在物体表面随机采样点对，
    ''' 统计距离、角度、面积与比例等形状函数的分布。
    ''' 原始论文使用 10 个 64 bin 直方图（共 640 维）：4 个形状函数 × 2（全体 / 加权）+ 2 个比例项。
    ''' 本实现采用相同的 640 维布局：
    ''' </para>
    ''' <list type="number">
    ''' <item><description>D2 距离分布（归一化到包围盒对角线）</description></item>
    ''' <item><description>A3 法向夹角分布</description></item>
    ''' <item><description>D3 点到切平面距离分布</description></item>
    ''' <item><description>面积比（点对与质心构成三角形的面积 / 对角线²）</description></item>
    ''' <item><description>距离比（点对距离 / 两侧到质心距离之和）</description></item>
    ''' </list>
    ''' <para>
    ''' 每个形状函数贡献两个 64 bin 段：原始计数与“法向接近平行”加权计数
    ''' （权重 <c>1/(1+A3)</c>）。加权段是原始论文中用于区分
    ''' “表面内部配对”与“跨越空腔配对”的思想的简化实现，
    ''' 因此本实现与 PCL 的 <c>ESF</c> <b>不逐位等价</b>，但保留了其核心的
    ''' “多形状函数联合分布”性质，可直接用于检索与粗筛。
    ''' </para>
    ''' <para>
    ''' <b>VFH(Viewpoint Feature Histogram)</b>：128 bin 的视角方向直方图
    ''' （法向与视点方向的夹角，按距离加权）拼接 ESF 的 640 bin，共 768 维。
    ''' 它把“从哪个方向看”也编码进描述子，因此对不同视角的同一物体敏感，
    ''' 适合“固定工位的来料识别”。
    ''' </para>
    ''' </remarks>
    Public NotInheritable Class GlobalDescriptors

        ''' <summary>ESF 维度。</summary>
        Public Const EsfDimension As Integer = 640

        ''' <summary>VFH 的视点方向直方图维度。</summary>
        Public Const ViewpointBins As Integer = 128

        ''' <summary>VFH 总维度（128 + 640）。</summary>
        Public Const VfhDimension As Integer = ViewpointBins + EsfDimension

        Private Const SectionBins As Integer = 64
        Private Const Sections As Integer = 10

        Private Sub New()
        End Sub

        ''' <summary>
        ''' 计算 ESF 描述子（640 维）。
        ''' </summary>
        ''' <param name="cloud">输入点云（会自动补齐法向）</param>
        ''' <param name="samplePairs">采样点对数量；<c>≤ 0</c> 表示自动（min(50000, max(1000, 20n))）</param>
        ''' <param name="seed">随机种子（<c>0</c> 表示使用默认种子）</param>
        ''' <param name="options">并行选项</param>
        Public Shared Function Esf(cloud As PointCloudBuffer, Optional samplePairs As Integer = 0,
                                   Optional seed As Integer = 0,
                                   Optional options As CloudyOptions = Nothing) As Double()
            If cloud Is Nothing Then Throw New ArgumentNullException(NameOf(cloud))

            Dim hist As Double() = New Double(EsfDimension - 1) {}
            Dim n As Integer = cloud.Count

            If n < 2 Then Return hist

            Dim opt As CloudyOptions = If(options, CloudyOptions.Default)
            Dim source As PointCloudBuffer = cloud

            If Not source.HasNormals Then
                source = NormalEstimation.Estimate(cloud, 15, 0.0, opt)
            End If

            Dim pairs As Integer = If(samplePairs > 0, samplePairs, std.Min(50000, std.Max(1000, n * 20)))
            Dim box As Aabb3 = source.Bounds()
            Dim diag As Double = std.Max(box.Diagonal, 1.0E-12)
            Dim center As Vector3d = box.Center
            Dim rnd As Random = If(seed = 0, opt.CreateRandom(7919), New Random(seed))

            ' 预生成随机点对，便于后续并行（随机数发生器本身不是线程安全的）
            Dim a As Integer() = New Integer(pairs - 1) {}
            Dim b As Integer() = New Integer(pairs - 1) {}

            For i As Integer = 0 To pairs - 1
                a(i) = rnd.Next(n)
                b(i) = rnd.Next(n)
            Next

            ' 按“固定块”并行：每块写自己的局部直方图，最后合并。
            ' 这里刻意不使用按线程 ID 分配槽位的写法——线程池会复用线程，
            ' 多个块落到同一线程时槽位会互相覆盖，结果将不可复现。
            Dim blocks As Integer = std.Max(1, opt.Workers)
            Dim blockSize As Integer = (pairs + blocks - 1) \ blocks
            Dim partials As Double()() = New Double(blocks - 1)() {}

            If pairs < 8192 OrElse Not opt.EnableParallel Then
                Dim local As Double() = New Double(EsfDimension - 1) {}

                For i As Integer = 0 To pairs - 1
                    If a(i) <> b(i) Then Call AddPair(source, a(i), b(i), center, diag, local)
                Next

                Call SimdHistogram.Accumulate(local, hist)
            Else
                Dim parallelOptions As New System.Threading.Tasks.ParallelOptions With {
                    .MaxDegreeOfParallelism = opt.Workers
                }

                System.Threading.Tasks.Parallel.For(0, blocks, parallelOptions,
                    Sub(block)
                        Dim lo As Integer = block * blockSize
                        Dim hi As Integer = std.Min(pairs, lo + blockSize)
                        Dim local As Double() = New Double(EsfDimension - 1) {}

                        For i As Integer = lo To hi - 1
                            If a(i) <> b(i) Then Call AddPair(source, a(i), b(i), center, diag, local)
                        Next

                        partials(block) = local
                    End Sub)

                For Each part As Double() In partials
                    If part IsNot Nothing Then Call SimdHistogram.Accumulate(part, hist)
                Next
            End If

            ' 每个 64 bin 段独立归一化：不同形状函数的量纲不同，必须先各自转成分布
            For s As Integer = 0 To Sections - 1
                Dim sum As Double = 0.0

                For i As Integer = 0 To SectionBins - 1
                    sum += hist(s * SectionBins + i)
                Next

                If sum > 0.0 Then
                    Dim inv As Double = 1.0 / sum

                    For i As Integer = 0 To SectionBins - 1
                        hist(s * SectionBins + i) *= inv
                    Next
                End If
            Next

            Return hist
        End Function

        Private Shared Sub AddPair(cloud As PointCloudBuffer, p As Integer, q As Integer,
                                   center As Vector3d, diag As Double, hist As Double())
            Dim px As Double = cloud.X(p)
            Dim py As Double = cloud.Y(p)
            Dim pz As Double = cloud.Z(p)
            Dim qx As Double = cloud.X(q)
            Dim qy As Double = cloud.Y(q)
            Dim qz As Double = cloud.Z(q)

            Dim dx As Double = qx - px
            Dim dy As Double = qy - py
            Dim dz As Double = qz - pz
            Dim distance As Double = std.Sqrt(dx * dx + dy * dy + dz * dz)

            If distance <= 0.0 Then Return

            Dim normalP As Vector3d = cloud.Normal(p)
            Dim normalQ As Vector3d = cloud.Normal(q)
            Dim angle As Double = normalP.AngleTo(normalQ) / std.PI

            Dim d2 As Double = distance / diag
            Dim d3 As Double = std.Abs(Vector3d.Dot(normalP, New Vector3d(dx, dy, dz))) / diag

            Dim ap As New Vector3d(px - center.X, py - center.Y, pz - center.Z)
            Dim aq As New Vector3d(qx - center.X, qy - center.Y, qz - center.Z)
            Dim area As Double = Vector3d.Cross(ap, aq).Length * 0.5 / (diag * diag)
            Dim radialSum As Double = (ap.Length + aq.Length) / diag
            Dim ratio As Double = If(radialSum > 1.0E-12, d2 / radialSum, 0.0)

            ' 权重：法向越平行（表面内部配对）越大
            Dim weight As Double = 1.0 / (1.0 + angle)

            Call Vote(hist, 0, d2, 1.0)
            Call Vote(hist, 1, d2, weight)
            Call Vote(hist, 2, angle, 1.0)
            Call Vote(hist, 3, angle, weight)
            Call Vote(hist, 4, d3, 1.0)
            Call Vote(hist, 5, d3, weight)
            Call Vote(hist, 6, area, 1.0)
            Call Vote(hist, 7, area, weight)
            Call Vote(hist, 8, ratio, 1.0)
            Call Vote(hist, 9, ratio, weight)
        End Sub

        Private Shared Sub Vote(hist As Double(), section As Integer, value As Double, weight As Double)
            Dim bin As Integer = CInt(std.Floor(std.Max(0.0, std.Min(1.0, value)) * SectionBins))

            If bin >= SectionBins Then bin = SectionBins - 1

            hist(section * SectionBins + bin) += weight
        End Sub

        ''' <summary>
        ''' 计算 VFH 描述子（768 维）：视点方向直方图 + ESF。
        ''' </summary>
        ''' <param name="cloud">输入点云</param>
        ''' <param name="viewpoint">视点（扫描仪位置）</param>
        ''' <param name="samplePairs">ESF 采样点对数（<c>≤ 0</c> 自动）</param>
        ''' <param name="seed">随机种子</param>
        ''' <param name="options">并行选项</param>
        Public Shared Function Vfh(cloud As PointCloudBuffer, viewpoint As Vector3d,
                                   Optional samplePairs As Integer = 0, Optional seed As Integer = 0,
                                   Optional options As CloudyOptions = Nothing) As Double()
            If cloud Is Nothing Then Throw New ArgumentNullException(NameOf(cloud))

            Dim opt As CloudyOptions = If(options, CloudyOptions.Default)
            Dim source As PointCloudBuffer = cloud

            If Not source.HasNormals Then
                source = NormalEstimation.Estimate(cloud, 15, 0.0, opt)
            End If

            Dim hist As Double() = New Double(VfhDimension - 1) {}
            Dim n As Integer = source.Count

            If n = 0 Then Return hist

            Dim rmax As Double = 0.0

            For i As Integer = 0 To n - 1
                Dim d As Double = (source.Item(i) - viewpoint).Length

                If d > rmax Then rmax = d
            Next

            If rmax <= 0.0 Then rmax = 1.0

            ' 128 bin 视角方向直方图：法向与视线夹角 + 距离权重
            For i As Integer = 0 To n - 1
                Dim toViewer As Vector3d = viewpoint - source.Item(i)
                Dim d As Double = toViewer.Length

                If d <= 1.0E-12 Then Continue For

                Dim normal As Vector3d = source.Normal(i)

                If normal.IsZero(1.0E-12) Then Continue For

                Dim alpha As Double = normal.AngleTo(toViewer) / std.PI
                Dim bin As Integer = CInt(std.Floor(std.Max(0.0, std.Min(1.0, alpha)) * ViewpointBins))

                If bin >= ViewpointBins Then bin = ViewpointBins - 1

                Dim weight As Double = std.Max(0.0, 1.0 - d / rmax)

                hist(bin) += weight + 1.0E-6
            Next

            Dim sumViewpoint As Double = 0.0

            For i As Integer = 0 To ViewpointBins - 1
                sumViewpoint += hist(i)
            Next

            If sumViewpoint > 0.0 Then
                For i As Integer = 0 To ViewpointBins - 1
                    hist(i) /= sumViewpoint
                Next
            End If

            ' 注意：局部变量不能叫 esf —— VB 的“自身初始化表达式”也在该局部变量作用域内，
            ' 会把 Esf(...) 解析成对局部数组的索引。
            Dim esfDescriptor As Double() = Esf(source, samplePairs, seed, opt)

            Array.Copy(esfDescriptor, 0, hist, ViewpointBins, EsfDimension)

            Return hist
        End Function

        ''' <summary>
        ''' 计算 VFH（视点默认取包围盒外侧正前方）。
        ''' </summary>
        Public Shared Function Vfh(cloud As PointCloudBuffer, Optional samplePairs As Integer = 0,
                                   Optional seed As Integer = 0,
                                   Optional options As CloudyOptions = Nothing) As Double()
            If cloud Is Nothing Then Throw New ArgumentNullException(NameOf(cloud))

            Dim box As Aabb3 = cloud.Bounds()
            Dim viewpoint As New Vector3d(box.Center.X, box.Center.Y, box.Max.Z + box.Diagonal + 1.0)

            Return Vfh(cloud, viewpoint, samplePairs, seed, options)
        End Function

    End Class
End Namespace
