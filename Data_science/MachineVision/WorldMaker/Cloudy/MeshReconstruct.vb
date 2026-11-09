' ---------------------------------------------------------------------------
' 点云 → 三角网格（Poisson 主 / BPA 备选）+ 统一后处理
'
' 选型理由：
'   * Poisson：求解指示函数、输出封闭平滑曲面，适合“要一个能打印/仿真的实体”；
'     代价是需要一致朝外的法向，且会把地面/背景一起包进来（面积大、细节被平滑）。
'   * BPA：滚球法贴着原始采样点铺三角面，保留扫描细节，但可能留孔洞、非水密。
' 两条路线共用同一份预处理点云与同一套导出/指标代码，方便直接对比取舍。
' ---------------------------------------------------------------------------

Imports CC = Microsoft.VisualBasic.MachineVision.Cloudy.Core
Imports CP = Microsoft.VisualBasic.MachineVision.Cloudy.Preprocessing
Imports CR = Microsoft.VisualBasic.MachineVision.Cloudy.Reconstruction
Imports std = System.Math

Namespace Cloudy

    ''' <summary>一套重建结果。</summary>
    Public Class ReconMesh

        Public Property Algorithm As String
        Public Property Mesh As CR.MeshData
        Public Property Seconds As Double
        Public Property Resolution As Integer
        Public Property Note As String

    End Class

    ''' <summary>
    ''' Poisson / BPA 重建与网格后处理。
    ''' </summary>
    Public NotInheritable Class MeshReconstruct

        Private Sub New()
        End Sub

        ''' <summary>
        ''' 泊松重建。若网格体积为负（法向整体朝内），翻转法向重跑一次。
        ''' </summary>
        Public Shared Function Poisson(cloud As CC.PointCloudBuffer,
                                       resolution As Integer,
                                       Optional log As Action(Of String) = Nothing) As ReconMesh
            Dim watch = System.Diagnostics.Stopwatch.StartNew()
            Dim current As CC.PointCloudBuffer = cloud
            Dim result As New ReconMesh With {
                .Algorithm = "poisson",
                .Resolution = resolution
            }

            Dim options As New CR.PoissonReconstruction.PoissonOptions With {
                .Resolution = resolution,
                .Padding = 0.05,
                .MaxIterations = 300,
                .MemoryBudgetMB = 2048
            }

            Dim poissonResult As CR.PoissonReconstruction.PoissonResult = Nothing

            Try
                poissonResult = CR.PoissonReconstruction.Reconstruct(current, options)
            Catch ex As Exception
                ' 分辨率过高导致内存不足：降一档重试（生产环境里这比直接失败好）
                Dim fallback As Integer = std.Max(48, resolution \ 2)

                Call Report(log, $"[warn] 泊松重建在分辨率 {resolution} 失败（{ex.Message}），降级到 {fallback} 重试")

                options.Resolution = fallback
                result.Resolution = fallback
                poissonResult = CR.PoissonReconstruction.Reconstruct(current, options)
            End Try

            Dim volume As Double = poissonResult.Mesh.Volume()

            If volume < 0.0 Then
                Call Report(log, $"泊松网格体积为负（{volume:G6}）⇒ 法向整体朝内，翻转后重跑一次")

                Dim flipped As CC.PointCloudBuffer = current.Clone()

                Call CloudPrep.FlipNormals(flipped)

                options.Resolution = result.Resolution
                poissonResult = CR.PoissonReconstruction.Reconstruct(flipped, options)
            End If

            watch.Stop()

            result.Mesh = poissonResult.Mesh
            result.Seconds = watch.Elapsed.TotalSeconds
            result.Note = $"converged={poissonResult.Converged}, iterations={poissonResult.Iterations}"

            Call Report(log, $"泊松重建完成: {result.Mesh.VertexCount:N0} 顶点 / {result.Mesh.TriangleCount:N0} 三角面，" &
                             $"{result.Seconds:F1}s（{result.Note}）")

            Return result
        End Function

        ''' <summary>
        ''' BPA 滚球法重建（保留扫描细节）。
        ''' </summary>
        Public Shared Function Bpa(cloud As CC.PointCloudBuffer,
                                   maxPoints As Integer,
                                   Optional log As Action(Of String) = Nothing) As ReconMesh
            Dim watch = System.Diagnostics.Stopwatch.StartNew()
            Dim sampled As CC.PointCloudBuffer = cloud

            If maxPoints > 0 AndAlso cloud.Count > maxPoints Then
                sampled = CP.Sampling.UniformCount(cloud, maxPoints)

                Call Report(log, $"BPA 采样: {cloud.Count:N0} → {sampled.Count:N0}（上限 {maxPoints:N0}）")
            End If

            Dim spacing As Double = sampled.EstimateAverageSpacing()
            Dim radius As Double = std.Max(spacing * 2.5, 1.0E-6)

            Call Report(log, $"BPA 滚球半径: {radius:G4}（平均点距 {spacing:G4}）")

            Dim mesh As CR.MeshData = CR.BallPivoting.Reconstruct(sampled, radius, 24)

            watch.Stop()

            Dim result As New ReconMesh With {
                .Algorithm = "bpa",
                .Mesh = mesh,
                .Seconds = watch.Elapsed.TotalSeconds,
                .Note = $"radius={radius:G4}, points={sampled.Count:N0}"
            }

            Call Report(log, $"BPA 重建完成: {mesh.VertexCount:N0} 顶点 / {mesh.TriangleCount:N0} 三角面，{result.Seconds:F1}s")

            Return result
        End Function

        ''' <summary>
        ''' 统一后处理：补洞 → Taubin 平滑（不收缩）→ 按预算简化 → 重算法向。
        ''' </summary>
        Public Shared Function PostProcess(mesh As CR.MeshData,
                                           simplifyTarget As Integer,
                                           Optional log As Action(Of String) = Nothing) As CR.MeshData
            If mesh Is Nothing Then Return Nothing

            Dim current As CR.MeshData = mesh

            ' 1) 补洞：Poisson 通常已封闭，BPA 常见小孔；边界环过多时跳过（耗时）
            Dim loops As Integer = 0

            Try
                loops = current.BoundaryLoops().Count
            Catch ex As Exception
                loops = 0
            End Try

            If loops > 0 AndAlso loops <= 500 Then
                current = CR.MeshProcessing.FillHoles(current, 200)

                Call Report(log, $"补洞: {loops} 个边界环 → {current.TriangleCount:N0} 三角面")
            ElseIf loops > 500 Then
                Call Report(log, $"[warn] 边界环过多（{loops}），跳过补洞（BPA 网格孔洞过多时应改用泊松）")
            End If

            ' 2) Taubin 平滑：λ/μ 交替，抑制高频噪声又几乎不收缩（Laplacian 会明显缩水）
            If current.TriangleCount > 0 Then
                current = CR.MeshProcessing.TaubinSmooth(current, 2, 0.5, -0.53, True)
            End If

            ' 3) QEM 简化（按预算）
            If simplifyTarget > 0 AndAlso current.TriangleCount > simplifyTarget Then
                Dim before As Integer = current.TriangleCount

                current = CR.MeshProcessing.SimplifyQem(current, simplifyTarget)

                Call Report(log, $"QEM 简化: {before:N0} → {current.TriangleCount:N0} 三角面")
            End If

            ' 4) 重算法向（后续 STL 导出与着色都依赖它）
            current.RecomputeNormals()

            Return current
        End Function

        Private Shared Sub Report(log As Action(Of String), message As String)
            If log IsNot Nothing Then Call log(message)
        End Sub

    End Class

End Namespace
