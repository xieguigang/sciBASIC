' ---------------------------------------------------------------------------
' 端到端编排：照片 → COLMAP → 3DGS → 点云 PLY → Cloudy 重建 → 三维模型
'
' 每个阶段都有独立的计时与统计，任何一段失败都能从报告里定位到；
' 中间产物全部落盘（点云/网格/渲染对照/训练日志），便于逐段复跑与人工复核。
' ---------------------------------------------------------------------------

Imports System.Diagnostics
Imports System.IO
Imports Microsoft.VisualBasic.ApplicationServices.Debugging
Imports WorldMaker.Cli
Imports std = System.Math

Namespace Pipeline

    ''' <summary>
    ''' 真实生产数据的照片 → 三维模型流水线。
    ''' </summary>
    Public NotInheritable Class WorldPipeline

        Private Sub New()
        End Sub

        Public Shared Function Run(options As PipelineOptions) As Integer
            Dim totalWatch As Stopwatch = Stopwatch.StartNew()
            Dim paths As RunPaths = RunPaths.Create(options.OutputRoot, options.RunTag)
            Dim report As New Reporting.RunReport()
            Dim title As String = $"WorldMaker 运行报告  (tag={options.RunTag}, preset={options.Preset})"

            Call Banner(options, paths)
            Call FillParameters(report, options)

            ' 复用模式：直接拿上一次导出的表面点云跑重建，跳过训练。
            ' 这样调泊松分辨率/BPA 半径/简化预算时不必重新训练（训练是整条链路里最贵的一段）。
            Dim surfacePly As String = Nothing
            ' 计时器必须在分支外声明：复用模式下阶段 1-4 整体被跳过，
            ' 但阶段 5 及以后仍要用它计时。
            Dim t0 As Stopwatch = Stopwatch.StartNew()

            Try
                If String.IsNullOrEmpty(options.ReusePly) Then
                ' ================================================================
                ' 阶段 1：COLMAP 稀疏模型
                ' ================================================================
                Call Stage($"[1/6] 读取 COLMAP 稀疏模型: {options.SparseDir}")

                t0 = Stopwatch.StartNew()

                Dim scene As Colmap.ColmapScene = Colmap.ColmapSparse.Read(options.SparseDir)

                t0.Stop()

                Call Log($"  相机: {scene.Cameras.Count}，注册视图: {scene.Images.Count}，稀疏点: {scene.PointCount:N0}")

                For Each camera As Colmap.ColmapCamera In scene.Cameras.Values
                    Call Log($"  {camera}")
                Next

                Dim spacing As Double = scene.EstimateSpacing()

                Call Log($"  场景平均点距 ≈ {spacing:G4}")

                Call report.Add("colmap", "cameras", scene.Cameras.Count)
                Call report.Add("colmap", "registered_views", scene.Images.Count)
                Call report.Add("colmap", "sparse_points", scene.PointCount)
                Call report.Add("colmap", "sparse_spacing", spacing)
                Call report.Add("colmap", "seconds", t0.Elapsed.TotalSeconds, "F2")

                ' ================================================================
                ' 阶段 2：训练数据集（JPEG 解码 + 降采样）
                ' ================================================================
                Call Stage("[2/6] 构建训练数据集（JPEG → Tensor）")

                t0.Restart()

                Dim dataset As Photos.ImageDataset =
                    Photos.ImageDatasetBuilder.Build(scene, options.ImagesDir, options, AddressOf Log)

                t0.Stop()

                Call report.Add("dataset", "views", dataset.Count)
                Call report.Add("dataset", "target_width", dataset.TargetWidth)
                Call report.Add("dataset", "target_height", dataset.TargetHeight)
                Call report.Add("dataset", "total_pixels", dataset.TotalPixels)
                Call report.Add("dataset", "intrinsics_fx", dataset.Intrinsics.Fx)
                Call report.Add("dataset", "intrinsics_fy", dataset.Intrinsics.Fy)
                Call report.Add("dataset", "seconds", t0.Elapsed.TotalSeconds, "F2")

                ' 抽样保存两张图，确认解码/降采样没有翻转或色偏
                Call dataset.SavePreview(0, paths.Render("dataset_sample_000.png"))

                If dataset.Count > 1 Then
                    Call dataset.SavePreview(dataset.Count \ 2, paths.Render("dataset_sample_mid.png"))
                End If

                ' ================================================================
                ' 阶段 3：3D 高斯溅射训练
                ' ================================================================
                Call Stage("[3/6] 3D 高斯溅射训练")

                t0.Restart()

                Dim fit As Splatting.SplatFitResult =
                    Splatting.SplatFit.Run(scene, dataset, options, paths, AddressOf Log)

                t0.Stop()

                Call report.Add("splatting", "gaussians", fit.Model.Count)
                Call report.Add("splatting", "requested_iterations", fit.RequestedIterations)
                Call report.Add("splatting", "completed_iterations", fit.CompletedIterations)
                Call report.Add("splatting", "cancelled_by_budget", fit.Cancelled)
                Call report.Add("splatting", "final_loss", fit.FinalLoss, "G6")
                Call report.Add("splatting", "final_psnr_db", fit.FinalPsnr, "F3")
                Call report.Add("splatting", "seconds", t0.Elapsed.TotalSeconds, "F2")
                Call report.Add("splatting", "log", fit.LogPath)

                Call DebugAudit.Mark($"3DGS trained: {fit.Model.Count} gaussians, psnr={fit.FinalPsnr:F2} dB")

                ' ================================================================
                ' 阶段 4：高斯 → 点云 PLY
                ' ================================================================
                Call Stage("[4/6] 导出三维点云 PLY")

                t0.Restart()

                Dim surface As Splatting.SurfaceCloudStats =
                    Splatting.SplatPointCloud.Export(fit.Model, options, paths, AddressOf Log)

                t0.Stop()

                Call report.Add("pointcloud", "gaussians_total", surface.TotalGaussians)
                Call report.Add("pointcloud", "surface_points", surface.KeptPoints)
                Call report.Add("pointcloud", "median_scale", surface.MedianScale)
                Call report.Add("pointcloud", "bbox_diagonal", surface.Diagonal)
                Call report.Add("pointcloud", "bbox_robust_diagonal", surface.RobustDiagonal)
                Call report.Add("pointcloud", "full_ply", surface.GaussianPly)
                Call report.Add("pointcloud", "surface_ply", surface.SurfacePly)
                Call report.Add("pointcloud", "seconds", t0.Elapsed.TotalSeconds, "F2")

                Call DebugAudit.Check(surface.KeptPoints > 0, "surface point cloud should not be empty")
                Call DebugAudit.Capture($"{surface.KeptPoints:N0}", "surface points")

                surfacePly = surface.SurfacePly
                Else
                    Call Stage("[1-4/6] 复用已有点云（跳过训练）: " & options.ReusePly)

                    If Not File.Exists(options.ReusePly) Then
                        Throw New FileNotFoundException($"--reuse-ply 指定的点云不存在: {options.ReusePly}")
                    End If

                    surfacePly = options.ReusePly

                    Call report.Add("pointcloud", "reused_ply", surfacePly)
                End If

                ' ================================================================
                ' 阶段 5：Cloudy 预处理（降采样 / 去噪 / 法向 / 定向）
                ' ================================================================
                Call Stage("[5/6] Cloudy 点云预处理")

                t0.Restart()

                Dim prepared As Cloudy.PreparedCloud =
                    Cloudy.CloudPrep.Run(surfacePly, options, paths, AddressOf Log)

                t0.Stop()

                Call report.Add("preprocess", "input_points", prepared.InputCount)
                Call report.Add("preprocess", "downsampled_points", prepared.DownsampledCount)
                Call report.Add("preprocess", "denoised_points", prepared.DenoisedCount)
                Call report.Add("preprocess", "final_points", prepared.FinalCount)
                Call report.Add("preprocess", "voxel_size", prepared.VoxelSize)
                Call report.Add("preprocess", "removed_ground", prepared.RemovedGround)
                Call report.Add("preprocess", "normal_flipped", prepared.NormalFlipped)
                Call report.Add("preprocess", "normal_outward_ratio", prepared.NormalConsistency, "P1")
                Call report.Add("preprocess", "seconds", t0.Elapsed.TotalSeconds, "F2")

                Call DebugAudit.Check(prepared.FinalCount > 100, "prepared cloud should keep at least 100 points")

                ' ================================================================
                ' 阶段 6：重建（Poisson 主 + BPA 备选）与模型导出
                ' ================================================================
                Call Stage("[6/6] 三维重建与模型导出")

                Dim poisson As Cloudy.ReconMesh = Cloudy.MeshReconstruct.Poisson(
                    prepared.Cloud, options.PoissonResolution, AddressOf Log)

                Dim poissonMesh = Cloudy.MeshReconstruct.PostProcess(poisson.Mesh, options.SimplifyTarget, AddressOf Log)

                Call Cloudy.MeshReconstruct.TransferVertexColors(poissonMesh, prepared.Cloud, AddressOf Log)

                Dim poissonStat As Cloudy.MeshStat = Cloudy.MeshMetrics.Measure(
                    "poisson", poissonMesh, prepared.Cloud, poisson.Seconds, poisson.Note, AddressOf Log)

                poissonStat.Algorithm = "poisson"

                Dim poissonStl As String = paths.Mesh("scene_poisson.stl")
                Dim poissonPly As String = paths.Mesh("scene_poisson.ply")
                Dim stlTriangles As Integer = Io.MeshWriter.SaveStl(poissonMesh, poissonStl,
                    $"WorldMaker poisson reconstruction (run {options.RunTag})")
                Call Io.MeshWriter.SavePly(poissonMesh, poissonPly, True)

                Call Log($"  泊松模型: {poissonStl}（{stlTriangles:N0} 三角面）")
                Call Log($"  泊松网格 PLY: {poissonPly}")

                Call report.Add("mesh.poisson", "vertices", poissonStat.Vertices)
                Call report.Add("mesh.poisson", "triangles", poissonStat.Triangles)
                Call report.Add("mesh.poisson", "closed", poissonStat.Closed)
                Call report.Add("mesh.poisson", "boundary_loops", poissonStat.BoundaryLoops)
                Call report.Add("mesh.poisson", "surface_area", poissonStat.SurfaceArea)
                Call report.Add("mesh.poisson", "volume", poissonStat.Volume)
                Call report.Add("mesh.poisson", "rms_to_cloud", poissonStat.RmsToCloud)
                Call report.Add("mesh.poisson", "max_to_cloud", poissonStat.MaxToCloud)
                Call report.Add("mesh.poisson", "resolution", poisson.Resolution)
                Call report.Add("mesh.poisson", "seconds", poissonStat.Seconds, "F1")
                Call report.Add("mesh.poisson", "stl", poissonStl)
                Call report.Add("mesh.poisson", "ply", poissonPly)

                Call DebugAudit.Check(poissonStat.Triangles > 0, "poisson mesh should contain triangles")

                Dim bpaStat As Cloudy.MeshStat = Nothing
                Dim bpaStl As String = ""
                Dim bpaPly As String = ""

                If options.EnableBpa Then
                    Dim bpa As Cloudy.ReconMesh = Cloudy.MeshReconstruct.Bpa(
                        prepared.Cloud, options.BpaMaxPoints, AddressOf Log)

                    If bpa.Mesh IsNot Nothing AndAlso bpa.Mesh.TriangleCount > 0 Then
                        Dim bpaMesh = Cloudy.MeshReconstruct.PostProcess(bpa.Mesh, options.SimplifyTarget, AddressOf Log)

                        Call Cloudy.MeshReconstruct.TransferVertexColors(bpaMesh, prepared.Cloud, AddressOf Log)

                        bpaStat = Cloudy.MeshMetrics.Measure(
                            "bpa", bpaMesh, prepared.Cloud, bpa.Seconds, bpa.Note, AddressOf Log)
                        bpaStat.Algorithm = "bpa"

                        bpaStl = paths.Mesh("scene_bpa.stl")
                        bpaPly = paths.Mesh("scene_bpa.ply")

                        Call Io.MeshWriter.SaveStl(bpaMesh, bpaStl, $"WorldMaker BPA reconstruction (run {options.RunTag})")
                        Call Io.MeshWriter.SavePly(bpaMesh, bpaPly, True)

                        Call Log($"  BPA 模型: {bpaStl}（{bpaMesh.TriangleCount:N0} 三角面）")
                        Call Log($"  BPA 网格 PLY: {bpaPly}")

                        Call report.Add("mesh.bpa", "vertices", bpaStat.Vertices)
                        Call report.Add("mesh.bpa", "triangles", bpaStat.Triangles)
                        Call report.Add("mesh.bpa", "closed", bpaStat.Closed)
                        Call report.Add("mesh.bpa", "boundary_loops", bpaStat.BoundaryLoops)
                        Call report.Add("mesh.bpa", "surface_area", bpaStat.SurfaceArea)
                        Call report.Add("mesh.bpa", "volume", bpaStat.Volume)
                        Call report.Add("mesh.bpa", "rms_to_cloud", bpaStat.RmsToCloud)
                        Call report.Add("mesh.bpa", "max_to_cloud", bpaStat.MaxToCloud)
                        Call report.Add("mesh.bpa", "note", bpa.Note)
                        Call report.Add("mesh.bpa", "stl", bpaStl)
                        Call report.Add("mesh.bpa", "ply", bpaPly)
                    Else
                        Call Log("[warn] BPA 未生成有效三角面，跳过 BPA 模型导出")
                    End If
                End If

                ' ---- 对比结论 ----
                If bpaStat IsNot Nothing Then
                    Call Log("")
                    Call Log("重建对比:")

                    If bpaStat.RmsToCloud > 0 AndAlso poissonStat.RmsToCloud > 0 Then
                        Call Log($"  点云贴合度 RMS  : poisson {poissonStat.RmsToCloud:G4} vs bpa {bpaStat.RmsToCloud:G4} " &
                                 $"（越小越贴合）")
                    End If

                    Call Log($"  水密性          : poisson {If(poissonStat.Closed, "封闭", "非封闭")} vs " &
                             $"bpa {If(bpaStat.Closed, "封闭", "非封闭")}")
                    Call Log($"  三角面数        : poisson {poissonStat.Triangles:N0} vs bpa {bpaStat.Triangles:N0}")

                    Call report.Add("compare", "poisson_rms", poissonStat.RmsToCloud)
                    Call report.Add("compare", "bpa_rms", bpaStat.RmsToCloud)
                    Call report.Add("compare", "poisson_closed", poissonStat.Closed)
                    Call report.Add("compare", "bpa_closed", bpaStat.Closed)
                End If

                ' ---- 产物清单 ----
                Call FillProducts(report, paths)

                totalWatch.Stop()

                Call report.Add("run", "total_seconds", totalWatch.Elapsed.TotalSeconds, "F1")
                Call report.Add("run", "finished_at", Date.Now.ToString("yyyy-MM-dd HH:mm:ss"))

                Call report.Save(paths.ReportDir, title)

                Call DebugAudit.Mark($"WorldMaker finished in {totalWatch.Elapsed.TotalSeconds:F1}s")

                Call Summary(report, paths, totalWatch.Elapsed)

                Return 0
            Catch ex As Exception
                totalWatch.Stop()

                Call report.Add("run", "failed", True)
                Call report.Add("run", "error", ex.Message)
                Call report.Add("run", "total_seconds", totalWatch.Elapsed.TotalSeconds, "F1")

                Try
                    Call report.Save(paths.ReportDir, title & "  [FAILED]")
                Catch inner As Exception
                    Console.WriteLine($"[warn] 报告写出失败: {inner.Message}")
                End Try

                Throw
            End Try
        End Function

        Private Shared Sub Banner(options As PipelineOptions, paths As RunPaths)
            Console.WriteLine(New String("="c, 78))
            Console.WriteLine("WorldMaker —— 照片 → 3D 高斯溅射点云 → 三维场景模型")
            Console.WriteLine(New String("="c, 78))
            Console.WriteLine(options.ToText())
            Console.WriteLine($"产物目录: {paths.Root}")
            Console.WriteLine()
            Call DebugAudit.Mark($"WorldMaker started: {options.Preset}")
        End Sub

        Private Shared Sub FillParameters(report As Reporting.RunReport, options As PipelineOptions)
            Call report.Add("parameters", "preset", options.Preset)
            Call report.Add("parameters", "images", options.ImagesDir)
            Call report.Add("parameters", "sparse", options.SparseDir)
            Call report.Add("parameters", "views", options.MaxViews)
            Call report.Add("parameters", "width", options.TargetWidth)
            Call report.Add("parameters", "iterations", options.Iterations)
            Call report.Add("parameters", "views_per_step", options.ViewsPerStep)
            Call report.Add("parameters", "budget_minutes", options.BudgetMinutes, "F1")
            Call report.Add("parameters", "poisson_resolution", options.PoissonResolution)
            Call report.Add("parameters", "remove_ground", options.RemoveGround)
            Call report.Add("parameters", "enable_bpa", options.EnableBpa)
            Call report.Add("parameters", "seed", options.Seed)
        End Sub

        Private Shared Sub FillProducts(report As Reporting.RunReport, paths As RunPaths)
            For Each dir As String In {paths.PlyDir, paths.MeshDir, paths.RenderDir, paths.LogDir, paths.ReportDir}
                If Not Directory.Exists(dir) Then Continue For

                For Each file As String In Directory.GetFiles(dir)
                    Dim info As New FileInfo(file)

                    Call report.Add("products", Path.GetFileName(file), $"{info.Length / 1024.0 / 1024.0:F2} MB")
                Next
            Next
        End Sub

        Private Shared Sub Summary(report As Reporting.RunReport, paths As RunPaths, elapsed As TimeSpan)
            Console.WriteLine()
            Console.WriteLine(New String("="c, 78))
            Console.WriteLine("全部完成")
            Console.WriteLine(New String("="c, 78))
            Console.WriteLine($"总耗时: {elapsed.TotalMinutes:F1} 分钟")
            Console.WriteLine($"产物目录: {paths.Root}")

            If Directory.Exists(paths.MeshDir) Then
                Console.WriteLine("三维模型:")

                For Each file As String In Directory.GetFiles(paths.MeshDir)
                    Console.WriteLine($"  {Path.GetFileName(file)}  ({New FileInfo(file).Length / 1024.0 / 1024.0:F2} MB)")
                Next
            End If

            Console.WriteLine($"报告: {Path.Combine(paths.ReportDir, "worldmaker_report.txt")}")
        End Sub

        Private Shared Sub Stage(title As String)
            Console.WriteLine()
            Console.WriteLine($"--- {title} ---")
            Call DebugAudit.Mark(title)
        End Sub

        Private Shared Sub Log(message As String)
            Console.WriteLine($"[{Date.Now:HH:mm:ss}] {message}")
        End Sub

    End Class

End Namespace
