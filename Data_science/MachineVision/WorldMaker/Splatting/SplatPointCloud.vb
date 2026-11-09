' ---------------------------------------------------------------------------
' 高斯模型 → 三维点云（并导出 PLY）
'
' 3DGS 的“点云”本质是各向异性椭球的中心，直接导出会得到两层东西：
'   * 有用的表面高斯：不透明度高、尺度接近局部点距；
'   * 无用的“绒毛”：低不透明度、或者尺度异常放大（跨越多个物体）的椭球。
' 因此这里做两道过滤（不透明度 + 相对中位尺度），再交给 Cloudy 做几何重建。
' 同时仍然导出一份<b>全参数</b> PLY（位置/颜色/尺度/旋转/不透明度），
' 便于用 MeshLab / 官方 3DGS 查看器复核训练结果。
' ---------------------------------------------------------------------------

Imports System.IO
Imports GS = Microsoft.VisualBasic.MachineVision.GaussianSplatting3D
Imports std = System.Math

Namespace Splatting

    ''' <summary>点云导出的统计信息。</summary>
    Public Class SurfaceCloudStats

        Public Property TotalGaussians As Integer
        Public Property KeptPoints As Integer
        Public Property MedianScale As Double
        Public Property MinOpacity As Double
        Public Property MaxScale As Double

        Public Property MinX As Double
        Public Property MinY As Double
        Public Property MinZ As Double
        Public Property MaxX As Double
        Public Property MaxY As Double
        Public Property MaxZ As Double

        Public Property SurfacePly As String
        Public Property GaussianPly As String

        Public ReadOnly Property Diagonal As Double
            Get
                Dim dx As Double = MaxX - MinX
                Dim dy As Double = MaxY - MinY
                Dim dz As Double = MaxZ - MinZ

                Return std.Sqrt(dx * dx + dy * dy + dz * dz)
            End Get
        End Property

    End Class

    ''' <summary>
    ''' 高斯 → 表面点云，并写出 PLY。
    ''' </summary>
    Public NotInheritable Class SplatPointCloud

        Private Sub New()
        End Sub

        Public Shared Function Export(model As GS.GaussianModel,
                                      options As Cli.PipelineOptions,
                                      paths As Pipeline.RunPaths,
                                      Optional log As Action(Of String) = Nothing) As SurfaceCloudStats
            Dim count As Integer = model.Count
            Dim positions As Double() = model.PositionsData
            Dim colors As Double() = model.ColorsData
            Dim scalesRaw As Double() = model.ScalesData

            ' 1) 中位尺度（用于识别“跨物体”的巨型高斯）
            Dim maxScales(count - 1) As Double

            For i As Integer = 0 To count - 1
                Dim o3 As Integer = i * 3

                maxScales(i) = std.Max(std.Exp(scalesRaw(o3)),
                                       std.Max(std.Exp(scalesRaw(o3 + 1)), std.Exp(scalesRaw(o3 + 2))))
            Next

            Dim sorted As Double() = DirectCast(maxScales.Clone(), Double())

            Array.Sort(sorted)

            Dim medianScale As Double = If(count = 0, 0.0, sorted(count \ 2))
            Dim scaleLimit As Double = medianScale * std.Max(1.0, options.MaxScaleFactor)

            ' 2) 过滤 + 收集
            Dim xs As New List(Of Double)(count)
            Dim ys As New List(Of Double)(count)
            Dim zs As New List(Of Double)(count)
            ' 注意：变量名不能叫 gs/rs/bs —— VB 不区分大小写，会与命名空间别名 GS 冲突
            Dim reds As New List(Of Double)(count)
            Dim greens As New List(Of Double)(count)
            Dim blues As New List(Of Double)(count)
            Dim opacities As New List(Of Double)(count)
            Dim scales As New List(Of Double)(count)

            Dim minX As Double = Double.MaxValue, minY As Double = Double.MaxValue, minZ As Double = Double.MaxValue
            Dim maxX As Double = Double.MinValue, maxY As Double = Double.MinValue, maxZ As Double = Double.MinValue

            For i As Integer = 0 To count - 1
                Dim opacity As Double = model.GetOpacity(i)

                If Double.IsNaN(opacity) OrElse opacity < options.MinOpacity Then Continue For
                If maxScales(i) > scaleLimit Then Continue For

                Dim o3 As Integer = i * 3
                Dim x As Double = positions(o3)
                Dim y As Double = positions(o3 + 1)
                Dim z As Double = positions(o3 + 2)

                If Double.IsNaN(x) OrElse Double.IsNaN(y) OrElse Double.IsNaN(z) Then Continue For

                Call xs.Add(x)
                Call ys.Add(y)
                Call zs.Add(z)
                Call reds.Add(colors(o3))
                Call greens.Add(colors(o3 + 1))
                Call blues.Add(colors(o3 + 2))
                Call opacities.Add(opacity)
                Call scales.Add(maxScales(i))

                If x < minX Then minX = x
                If y < minY Then minY = y
                If z < minZ Then minZ = z
                If x > maxX Then maxX = x
                If y > maxY Then maxY = y
                If z > maxZ Then maxZ = z
            Next

            Dim stats As New SurfaceCloudStats With {
                .TotalGaussians = count,
                .KeptPoints = xs.Count,
                .MedianScale = medianScale,
                .MinOpacity = options.MinOpacity,
                .MaxScale = scaleLimit,
                .MinX = minX, .MinY = minY, .MinZ = minZ,
                .MaxX = maxX, .MaxY = maxY, .MaxZ = maxZ
            }

            Call Report(log, $"表面点云过滤: {count:N0} → {stats.KeptPoints:N0} " &
                             $"(不透明度 ≥ {options.MinOpacity:F2}，尺度 ≤ {scaleLimit:G4} = {options.MaxScaleFactor:F1}× 中位 {medianScale:G4})")

            If stats.KeptPoints = 0 Then
                Throw New InvalidOperationException(
                    "过滤后没有任何高斯点。请降低 --min-opacity 或提高 --max-scale-factor 后重试。")
            End If

            ' 3) 全参数 PLY（官方查看器可读）
            Dim gaussianPly As String = paths.Ply("gaussians_full.ply")

            Try
                Call GS.PointCloudIO.SaveGaussianPLY(gaussianPly, model)

                stats.GaussianPly = gaussianPly

                Call Report(log, $"全参数高斯 PLY: {gaussianPly}（{New FileInfo(gaussianPly).Length / 1024.0 / 1024.0:F1} MB）")
            Catch ex As Exception
                Call Report(log, $"[warn] 全参数 PLY 写出失败: {ex.Message}")
            End Try

            ' 4) 表面点云 PLY（交给 Cloudy）
            Dim surfacePly As String = paths.Ply("splat_surface.ply")

            Call Io.CloudPly.SaveSurface(surfacePly, xs.ToArray(), ys.ToArray(), zs.ToArray(),
                                         reds.ToArray(), greens.ToArray(), blues.ToArray(),
                                         opacities.ToArray(), scales.ToArray())

            stats.SurfacePly = surfacePly

            Call Report(log, $"表面点云 PLY: {surfacePly}（{New FileInfo(surfacePly).Length / 1024.0 / 1024.0:F1} MB，" &
                             $"包围盒对角线 {stats.Diagonal:F3}）")

            Return stats
        End Function

        Private Shared Sub Report(log As Action(Of String), message As String)
            If log IsNot Nothing Then Call log(message)
        End Sub

    End Class

End Namespace
