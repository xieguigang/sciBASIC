' ---------------------------------------------------------------------------
' train / render 命令
' ---------------------------------------------------------------------------

Imports System.IO
Imports Microsoft.VisualBasic.MachineVision.GaussianSplatting3D

Module TrainCommand

    Public Sub Run(args As String())
        Dim cli As New CommandLine(args)
        Dim startTime = DateTime.Now

        Console.WriteLine()
        Console.WriteLine("[1/5] 加载测试数据 ...")

        If Not Directory.Exists(TestDataDir) Then
            Console.WriteLine($"错误: 测试数据目录不存在: {TestDataDir}")
            Return
        End If

        Dim Intrinsics As CameraIntrinsics, Cameras As List(Of Camera)
        With LoadCameras()
            Intrinsics = .Intrinsics
            Cameras = .Cameras
        End With

        Console.WriteLine($"  - 内参: fx={Intrinsics.Fx:F2}, fy={Intrinsics.Fy:F2}, " &
                          $"cx={Intrinsics.Cx:F2}, cy={Intrinsics.Cy:F2}, {Intrinsics.Width}×{Intrinsics.Height}")
        Console.WriteLine($"  - 视图数量: {Cameras.Count}")

        Dim targetImages = LoadImages(Cameras.Count)

        If targetImages.Count = 0 Then
            Console.WriteLine("错误: 未找到任何 PNG 图像")
            Return
        End If

        Console.WriteLine($"  - 加载图像数量: {targetImages.Count}")

        Call EnsureOutput()

        ' ---- 初始化 ----
        Console.WriteLine()
        Console.WriteLine("[2/5] 初始化高斯模型 ...")

        Dim initial = cli.GetInt("gaussians", 2000)
        Dim model = InitializeModel(Intrinsics, Cameras, targetImages, initial)

        Console.WriteLine($"  - 初始高斯数量: {model.Count}")

        ' ---- 训练 ----
        Dim options As New TrainingOptions With {
            .Iterations = cli.GetInt("iterations", 500),
            .EvaluateEvery = cli.GetInt("eval-every", 10),
            .ViewsPerStep = cli.GetInt("views", 0),
            .EnableProfiling = cli.Has("profile"),
            .GradientMode = If(cli.Has("legacy"), SplatGradientMode.FiniteDifference, SplatGradientMode.Analytic)
        }

        If cli.Has("no-densify") Then options.DensifyInterval = 0

        Console.WriteLine()
        Console.WriteLine("[3/5] 开始训练 ...")
        Console.WriteLine($"  - 迭代次数: {options.Iterations}")
        Console.WriteLine($"  - 学习率: {options.InitialLr} → {options.FinalLr}")
        Console.WriteLine($"  - 梯度模式: {options.GradientMode}")
        Console.WriteLine($"  - 每步视图: {If(options.ViewsPerStep <= 0, "全部", options.ViewsPerStep.ToString())}")
        Console.WriteLine()

        Dim trainer As New SplatTrainer(model, Cameras, targetImages, options)
        Dim logPath = Path.Combine(OutputDir, "training_log.csv")
        Dim logWriter As New StreamWriter(logPath)

        logWriter.WriteLine("iteration,num_gaussians,loss,psnr,lr,cloned,split,pruned,step_ms")
        logWriter.Flush()

        Dim onProgress As Action(Of TrainingProgress) =
            Sub(p)
                Console.WriteLine("  " & p.ToString())

                If Not Double.IsNaN(p.Loss) Then
                    logWriter.WriteLine($"{p.Iteration},{p.Gaussians},{p.Loss:F6},{p.Psnr:F4}," &
                                        $"{p.LearningRate:F6},{p.Cloned},{p.Split},{p.Pruned},{p.StepMs:F0}")
                    logWriter.Flush()
                End If
            End Sub

        Dim result = trainer.Run(onProgress)

        logWriter.Dispose()

        Console.WriteLine($"  训练日志已保存: {logPath}")

        ' ---- 保存产物 ----
        Console.WriteLine()
        Console.WriteLine("[4/5] 保存输出 ...")

        Dim plyPath = Path.Combine(OutputDir, "reconstruction.ply")
        Call PointCloudIO.SaveGaussianPLY(plyPath, model)
        Console.WriteLine($"  - 点云已保存: {plyPath}")

        Call SaveRenders(model, Cameras, "renders", "render")
        Console.WriteLine($"  - 渲染结果已保存: {Path.Combine(OutputDir, "renders")}")

        ' ---- 评估 ----
        Console.WriteLine()
        Console.WriteLine("[5/5] 最终评估 ...")

        Dim final = trainer.Evaluate()

        Console.WriteLine($"  - 最终高斯数量: {model.Count}")
        Console.WriteLine($"  - 最终 MSE: {final.Loss:F6}")
        Console.WriteLine($"  - 最终 PSNR: {final.Psnr:F2} dB")
        Console.WriteLine($"  - 总耗时: {(DateTime.Now - startTime).TotalSeconds:F1} 秒")

        If result.ProfileReport IsNot Nothing Then
            Console.WriteLine()
            Console.WriteLine("分阶段耗时:")
            Console.Write(result.ProfileReport)
        End If

        Console.WriteLine()
        Console.WriteLine("训练完成！")
        Console.WriteLine($"输出目录: {OutputDir}")
    End Sub

    Public Sub RunRenderOnly(args As String())
        Dim Intrinsics As CameraIntrinsics, Cameras As List(Of Camera)
        With LoadCameras()
            Intrinsics = .Intrinsics
            Cameras = .Cameras
        End With

        Dim images = LoadImages(Cameras.Count)
        Dim model = InitializeModel(Intrinsics, Cameras, images, 2000)

        Call EnsureOutput()
        Call SaveRenders(model, Cameras, "renders_random", "render")
        Console.WriteLine($"渲染完成: {Path.Combine(OutputDir, "renders_random")}")
    End Sub

End Module
