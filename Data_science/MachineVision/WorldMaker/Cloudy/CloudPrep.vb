' ---------------------------------------------------------------------------
' Cloudy 预处理阶段：降采样 → 去噪 →（可选）地面移除 → 法向 → 一致定向
'
' 为什么法向定向单独占一段：
'   泊松重建求解的是“指示函数”的泊松方程，法向若有一半点朝内，
'   右侧向量场会互相抵消，结果是网格塌陷/翻面。3DGS 导出的点云没有朝向，
'   所以必须：MST 一致定向 → 用“法向是否背离场景质心”的比例做全局校验，
'   比例低于 50% 就整体翻转（这一步对 T&T 这种环绕拍摄的场景几乎总是必要）。
' ---------------------------------------------------------------------------

Imports CC = Microsoft.VisualBasic.MachineVision.Cloudy.Core
Imports CP = Microsoft.VisualBasic.MachineVision.Cloudy.Preprocessing
Imports CF = Microsoft.VisualBasic.MachineVision.Cloudy.Features
Imports CS = Microsoft.VisualBasic.MachineVision.Cloudy.Segmentation
Imports std = System.Math

Namespace Cloudy

    ''' <summary>预处理产物与统计。</summary>
    Public Class PreparedCloud

        Public Property Cloud As CC.PointCloudBuffer

        Public Property InputCount As Integer
        Public Property CroppedCount As Integer
        Public Property DownsampledCount As Integer
        Public Property DenoisedCount As Integer
        Public Property FinalCount As Integer

        Public Property Spacing As Double
        Public Property VoxelSize As Double
        Public Property RemovedGround As Integer
        Public Property NormalFlipped As Boolean
        Public Property NormalConsistency As Double
        Public Property NormalRmsError As Double

    End Class

    ''' <summary>
    ''' 3DGS 点云的 Cloudy 预处理。
    ''' </summary>
    Public NotInheritable Class CloudPrep

        Private Sub New()
        End Sub

        Public Shared Function Run(plyPath As String,
                                   options As Cli.PipelineOptions,
                                   paths As Pipeline.RunPaths,
                                   Optional log As Action(Of String) = Nothing) As PreparedCloud
            Dim result As New PreparedCloud()

            ' ---- 1) 读入 ----
            Dim watch = System.Diagnostics.Stopwatch.StartNew()
            Dim cloud As CC.PointCloudBuffer = Io.CloudPly.Load(plyPath)

            result.InputCount = cloud.Count

            Call Report(log, $"读入点云: {cloud.Count:N0} 点（{watch.Elapsed.TotalSeconds:F1}s）")

            ' ---- 1b) 稳健裁剪：SFM 稀疏点里总有少量“飞到天边”的离群点，
            ' 它们数量极少却会把包围盒撑大一个量级，导致泊松的体素边长大于物体尺寸，
            ' 重建结果直接糊成一团。这里按轴去掉两端 0.5% 的极端值。
            cloud = RobustCrop(cloud, 0.005, log)

            result.CroppedCount = cloud.Count

            ' ---- 2) 体素降采样 ----
            ' 表面点云必须用邻近距离中位数估计点距：体积法会把薄壳内部的空间算进来，
            ' 得到的“间距”偏大一个量级，体素降采样会把点云直接压没。
            Dim spacing As Double = Pipeline.PointSpacing.MedianNeighborSpacing(cloud, 8)

            result.Spacing = spacing

            Dim voxel As Double = std.Max(spacing * 1.5, 1.0E-6)
            Dim downsampled As CC.PointCloudBuffer = CP.VoxelGridFilter.Downsample(cloud, voxel)

            result.VoxelSize = voxel
            result.DownsampledCount = downsampled.Count

            Call Report(log, $"体素降采样: {cloud.Count:N0} → {downsampled.Count:N0}（体素 {voxel:G4}，平均点距 {spacing:G4}）")

            ' ---- 3) 统计去噪 ----
            Dim sor = CP.StatisticalOutlierRemoval.Apply(downsampled, 20, 1.0)

            result.DenoisedCount = sor.Cloud.Count

            Call Report(log, $"统计去噪(SOR): 剔除 {sor.RemovedCount:N0} 点（{sor.RemovedRatio:P1}），剩余 {sor.Cloud.Count:N0}")

            cloud = sor.Cloud

            ' ---- 4) 可选地面/背景移除 ----
            result.RemovedGround = 0

            If options.RemoveGround AndAlso cloud.Count > 1000 Then
                Dim ransac As New CS.ModelFitting.RansacOptions With {
                    .MaxIterations = 800,
                    .InlierDistance = std.Max(spacing * 2.0, 1.0E-5),
                    .MinInliers = 500
                }
                Dim detection As CS.PrimitiveDetection = CS.ModelFitting.Detect(cloud, CS.PrimitiveType.Plane, ransac)

                If detection IsNot Nothing AndAlso detection.InlierCount > 0 Then
                    Dim inliers As Integer() = detection.InlierIndices
                    Dim mask(cloud.Count - 1) As Boolean

                    For i As Integer = 0 To mask.Length - 1
                        mask(i) = True
                    Next

                    For Each index As Integer In inliers
                        If index >= 0 AndAlso index < mask.Length Then mask(index) = False
                    Next

                    Dim kept As CC.PointCloudBuffer = cloud.Where(mask)

                    result.RemovedGround = cloud.Count - kept.Count

                    Call Report(log, $"RANSAC 地面移除: 移除 {result.RemovedGround:N0} 点（占 {detection.Fitness:P1}），剩余 {kept.Count:N0}")

                    cloud = kept
                End If
            End If

            ' ---- 5) 法向估计 ----
            Dim estimated As CC.PointCloudBuffer = CF.NormalEstimation.Estimate(cloud, 24)

            result.NormalRmsError = 0.0

            ' ---- 6) 一致定向 + 全局校验 ----
            Dim flippedCount As Integer = CF.NormalOrientation.OrientConsistent(estimated, 12)

            Call Report(log, $"法向一致定向(MST): 翻转 {flippedCount:N0} / {estimated.Count:N0}")

            Dim centroid As CC.Vector3d = estimated.Centroid()
            Dim outward As Integer = 0

            For i As Integer = 0 To estimated.Count - 1
                Dim normal As CC.Vector3d = estimated.Normal(i)

                If normal.IsZero(1.0E-12) Then Continue For

                If CC.Vector3d.Dot(normal, estimated.Item(i) - centroid) > 0.0 Then outward += 1
            Next

            result.NormalConsistency = outward / CDbl(std.Max(1, estimated.Count))

            If result.NormalConsistency < 0.5 Then
                Call FlipNormals(estimated)

                result.NormalFlipped = True
                result.NormalConsistency = 1.0 - result.NormalConsistency

                Call Report(log, $"法向整体翻转（朝外比例原为 {1.0 - result.NormalConsistency:P1}）")
            End If

            Call Report(log, $"法向朝外比例: {result.NormalConsistency:P1}")

            result.Cloud = estimated
            result.FinalCount = estimated.Count

            Call Io.CloudPly.SaveCloud(paths.Ply("prepared_cloud.ply"), estimated)

            Call Report(log, $"预处理完成: {result.FinalCount:N0} 点 → {paths.Ply("prepared_cloud.ply")}")

            Return result
        End Function

        ''' <summary>
        ''' 按轴去掉两端极端离群点（分位数裁剪）。
        ''' </summary>
        ''' <param name="cloud">输入点云</param>
        ''' <param name="ratio">每个轴两端各丢弃的比例（0.005 = 各 0.5%）</param>
        Private Shared Function RobustCrop(cloud As CC.PointCloudBuffer,
                                           ratio As Double,
                                           log As Action(Of String)) As CC.PointCloudBuffer
            Dim count As Integer = cloud.Count

            If count < 1000 OrElse ratio <= 0.0 Then Return cloud

            Dim xs As Double() = DirectCast(cloud.X.Clone(), Double())
            Dim ys As Double() = DirectCast(cloud.Y.Clone(), Double())
            Dim zs As Double() = DirectCast(cloud.Z.Clone(), Double())

            Array.Sort(xs)
            Array.Sort(ys)
            Array.Sort(zs)

            Dim lo As Integer = CInt(std.Floor(count * ratio))
            Dim hi As Integer = count - 1 - lo

            If lo < 0 Then lo = 0
            If hi >= count Then hi = count - 1

            Dim xLo As Double = xs(lo), xHi As Double = xs(hi)
            Dim yLo As Double = ys(lo), yHi As Double = ys(hi)
            Dim zLo As Double = zs(lo), zHi As Double = zs(hi)

            Dim keep(count - 1) As Boolean
            Dim kept As Integer = 0

            For i As Integer = 0 To count - 1
                Dim x As Double = cloud.X(i)
                Dim y As Double = cloud.Y(i)
                Dim z As Double = cloud.Z(i)

                If x >= xLo AndAlso x <= xHi AndAlso y >= yLo AndAlso y <= yHi AndAlso z >= zLo AndAlso z <= zHi Then
                    keep(i) = True
                    kept += 1
                End If
            Next

            If kept < count \ 2 Then
                Call Report(log, $"[warn] 分位数裁剪会丢掉 {count - kept:N0} 点（>50%），跳过裁剪")

                Return cloud
            End If

            Call Report(log, $"稳健裁剪: {count:N0} → {kept:N0}（各轴去掉 {ratio:P1} 尾部离群，" &
                             $"包围盒 {xHi - xLo:F3} × {yHi - yLo:F3} × {zHi - zLo:F3}）")

            Return cloud.Where(keep)
        End Function

        ''' <summary>整体翻转法向（原地）。</summary>
        Public Shared Sub FlipNormals(cloud As CC.PointCloudBuffer)
            Dim count As Integer = cloud.Count
            Dim nx(count - 1) As Double
            Dim ny(count - 1) As Double
            Dim nz(count - 1) As Double

            For i As Integer = 0 To count - 1
                nx(i) = -cloud.Nx(i)
                ny(i) = -cloud.Ny(i)
                nz(i) = -cloud.Nz(i)
            Next

            cloud.SetNormals(nx, ny, nz)
        End Sub

        Private Shared Sub Report(log As Action(Of String), message As String)
            If log IsNot Nothing Then Call log(message)
        End Sub

    End Class

End Namespace
