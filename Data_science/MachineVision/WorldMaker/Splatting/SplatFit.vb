' ---------------------------------------------------------------------------
' 3D 高斯溅射训练阶段
'
' 与“从随机点初始化”的玩具流程相比，这里有两个面向真实数据的关键改动：
'   1. 初始高斯来自 COLMAP 稀疏点（本例 18 万个带 RGB 的三维点），
'      因此不需要几千次迭代去“生成形状”，几百次迭代就能把颜色与细节拟合上；
'   2. 训练有墙钟时限（--budget-minutes）。到点即通过 CancellationToken 停止，
'      保留当前模型继续导出——生产流水线不能因为训练段超时整体失败。
' ---------------------------------------------------------------------------

Imports System.IO
Imports CC = Microsoft.VisualBasic.MachineVision.Cloudy.Core
Imports GS = Microsoft.VisualBasic.MachineVision.GaussianSplatting3D
Imports std = System.Math

Namespace Splatting

    ''' <summary>训练阶段产物。</summary>
    Public Class SplatFitResult

        Public Property Model As GS.GaussianModel
        Public Property Training As GS.TrainingResult
        Public Property Seconds As Double
        Public Property RequestedIterations As Integer
        Public Property CompletedIterations As Integer
        Public Property Cancelled As Boolean
        Public Property FinalLoss As Double
        Public Property FinalPsnr As Double
        Public Property LogPath As String

    End Class

    ''' <summary>
    ''' 用 COLMAP 稀疏点初始化高斯并运行 <see cref="GS.SplatTrainer"/>。
    ''' </summary>
    Public NotInheritable Class SplatFit

        Private Sub New()
        End Sub

        Public Shared Function Run(scene As Colmap.ColmapScene,
                                   dataset As Photos.ImageDataset,
                                   options As Cli.PipelineOptions,
                                   paths As Pipeline.RunPaths,
                                   Optional log As Action(Of String) = Nothing) As SplatFitResult
            Dim result As New SplatFitResult With {
                .RequestedIterations = options.Iterations
            }

            ' ---- 1) 初始化 ----
            Dim model As GS.GaussianModel
            Dim spacing As Double = scene.EstimateSpacing()

            ' 稀疏点也是“表面采样”，必须用邻近距离中位数而不是体积法估间距
            If scene.PointCount >= 1000 Then
                Dim sparseCloud As CC.PointCloudBuffer =
                    Pipeline.PointSpacing.FromFlat(scene.Points, scene.PointCount)

                spacing = Pipeline.PointSpacing.MedianNeighborSpacing(sparseCloud, 8)
            End If

            Dim initScale As Single = CSng(std.Max(spacing * 1.5, 1.0E-6))

            Call Report(log, $"稀疏点: {scene.PointCount:N0} 个，平均点距 ≈ {spacing:G4}，初始高斯尺度 {initScale:G4}")

            If scene.PointCount >= 1000 Then
                model = GS.GaussianModel.FromPointCloud(scene.Points, scene.Colors, scene.PointCount, initScale)
            Else
                ' 兜底：稀疏点太少（例如 COLMAP 未跑出结果）时用包围盒随机点，
                ' 让流程仍可跑通，但会在报告里标注质量风险。
                Dim box As Double() = SceneBounds(scene)

                Call Report(log, "[warn] 稀疏点少于 1000，改用随机初始化（重建质量会明显下降）")

                Dim bounds(5) As Single

                bounds(0) = CSng(box(0)) : bounds(1) = CSng(box(1)) : bounds(2) = CSng(box(2))
                bounds(3) = CSng(box(3)) : bounds(4) = CSng(box(4)) : bounds(5) = CSng(box(5))

                model = GS.GaussianModel.FromRandom(
                    std.Max(20000, dataset.Count * 200), bounds, CSng(std.Max(spacing * 3, 1.0E-4)), options.Seed)
            End If

            Call Report(log, $"初始高斯数量: {model.Count:N0}")

            ' ---- 2) 训练配置 ----
            ' 注意：EvaluateEvery 必须为 0。每步评估会渲染“全部”视图
            ' （48 视图 × 0.8s ≈ 40s），代价远超一步优化本身（约 3s），
            ' 因此只在训练结束后做一次全量评估。
            Dim training As New GS.TrainingOptions With {
                .Iterations = options.Iterations,
                .ViewsPerStep = options.ViewsPerStep,
                .EvaluateEvery = 0,
                .DensifyInterval = options.DensifyInterval,
                .DensifyFrom = CInt(options.Iterations * 0.1),
                .DensifyUntil = CInt(options.Iterations * 0.8),
                .EnableProfiling = False,
                .GradientMode = GS.SplatGradientMode.Analytic
            }

            Call Report(log, $"训练配置: {training.Iterations} 迭代 × 每步 {training.ViewsPerStep} 视图，" &
                             $"致密化间隔 {training.DensifyInterval}，" &
                             $"时限 {If(options.BudgetMinutes <= 0, "不限", options.BudgetMinutes.ToString("F1") & " 分钟")}")

            ' ---- 3) 训练（带墙钟时限）----
            Dim csvPath As String = paths.Log("training_log.csv")
            Dim writer As New StreamWriter(csvPath, False, System.Text.Encoding.UTF8)
            Dim watch = System.Diagnostics.Stopwatch.StartNew()
            Dim cancelled As Boolean = False

            Call writer.WriteLine("iteration,num_gaussians,loss,psnr,lr,cloned,split,pruned,step_ms")
            Call writer.Flush()

            Using cts As New System.Threading.CancellationTokenSource()
                If options.BudgetMinutes > 0.0 Then
                    Call cts.CancelAfter(TimeSpan.FromMinutes(options.BudgetMinutes))
                End If

                Dim trainer As New GS.SplatTrainer(model, dataset.Cameras, dataset.Images, training)

                Dim onProgress As Action(Of GS.TrainingProgress) =
                    Sub(p)
                        If Not Double.IsNaN(p.Loss) Then
                            Call writer.WriteLine($"{p.Iteration},{p.Gaussians},{p.Loss:F6},{p.Psnr:F4}," &
                                                  $"{p.LearningRate:F6},{p.Cloned},{p.Split},{p.Pruned},{p.StepMs:F0}")
                            Call writer.Flush()
                        End If

                        If p.Iteration Mod 10 = 0 OrElse p.Iteration = training.Iterations - 1 Then
                            Dim elapsed = watch.Elapsed.TotalSeconds
                            Dim eta As String = "-"

                            If p.Iteration > 0 Then
                                Dim perIter = elapsed / (p.Iteration + 1)
                                Dim remain = perIter * (training.Iterations - p.Iteration - 1)

                                eta = $"{remain / 60.0:F1} min"
                            End If

                            Call Report(log, $"  {p} | elapsed {elapsed:F0}s | eta {eta}")
                        End If
                    End Sub

                Try
                    result.Training = trainer.Run(onProgress, cts.Token)
                Catch ex As OperationCanceledException
                    cancelled = True
                    Call Report(log, "[warn] 训练被取消（达到时间预算），使用当前模型继续后续阶段")
                End Try

                If cts.IsCancellationRequested Then cancelled = True

                ' 训练结束后的最终评估（全量视图 MSE/PSNR），比每步的抽样更能反映质量
                Try
                    Dim evaluated = trainer.Evaluate()

                    result.FinalLoss = evaluated.Loss
                    result.FinalPsnr = evaluated.Psnr
                Catch ex As Exception
                    Call Report(log, $"[warn] 最终评估失败（不影响后续阶段）: {ex.Message}")
                End Try
            End Using

            watch.Stop()
            Call writer.Dispose()

            result.Model = model
            result.Seconds = watch.Elapsed.TotalSeconds
            result.Cancelled = cancelled
            result.CompletedIterations = If(result.Training Is Nothing, 0, result.Training.Iterations)
            result.LogPath = csvPath

            Call Report(log, $"训练完成: {model.Count:N0} 个高斯，{result.Seconds:F1}s，" &
                             $"最终 PSNR {(If(Double.IsNaN(result.FinalPsnr), 0, result.FinalPsnr)):F2} dB")

            ' ---- 4) 关键视角渲染对照 ----
            Call RenderSamples(model, dataset, options, paths, log)

            Return result
        End Function

        ''' <summary>
        ''' 渲染若干训练视角并与原图并排保存，便于人工核对“点云是否真的贴合照片”。
        ''' </summary>
        Private Shared Sub RenderSamples(model As GS.GaussianModel,
                                         dataset As Photos.ImageDataset,
                                         options As Cli.PipelineOptions,
                                         paths As Pipeline.RunPaths,
                                         log As Action(Of String))
            Dim count As Integer = std.Min(std.Max(0, options.RenderCount), dataset.Count)

            If count <= 0 Then Return

            Call Report(log, $"渲染 {count} 个对照视角 ...")

            For k As Integer = 0 To count - 1
                Dim index As Integer = CInt(CDbl(k) * dataset.Count / count)

                index = std.Min(index, dataset.Count - 1)

                Try
                    Dim rendered = GS.SplatRenderer.Render(model, dataset.Cameras(index))

                    Call GS.ImageLoader.Save(rendered, paths.Render($"splat_{k:00}_view{index:000}.png"))
                    Call dataset.SavePreview(index, paths.Render($"photo_{k:00}_view{index:000}.png"))
                Catch ex As Exception
                    Call Report(log, $"[warn] 视角 {index} 渲染失败: {ex.Message}")
                End Try
            Next
        End Sub

        Private Shared Function SceneBounds(scene As Colmap.ColmapScene) As Double()
            ' 返回 [minX, minY, minZ, maxX, maxY, maxZ]
            Dim bounds(5) As Double

            For i As Integer = 0 To 2
                bounds(i) = Double.MaxValue
                bounds(i + 3) = Double.MinValue
            Next

            If scene.PointCount = 0 Then Return {-4, -2, -4, 4, 2, 4}

            For i As Integer = 0 To scene.PointCount - 1
                For c As Integer = 0 To 2
                    Dim v As Double = scene.Points(i * 3 + c)

                    If v < bounds(c) Then bounds(c) = v
                    If v > bounds(c + 3) Then bounds(c + 3) = v
                Next
            Next

            Return bounds
        End Function

        Private Shared Sub Report(log As Action(Of String), message As String)
            If log IsNot Nothing Then Call log(message)
        End Sub

    End Class

End Namespace
