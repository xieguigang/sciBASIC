Imports System.IO
Imports System.Runtime

Module Program

    ' ===== 训练配置 =====
    Private Const NUM_VIEWS As Integer = 12
    Private Const IMAGE_WIDTH As Integer = 320
    Private Const IMAGE_HEIGHT As Integer = 240
    Private Const INITIAL_GAUSSIANS As Integer = 2000
    Private Const NUM_ITERATIONS As Integer = 500
    Private Const INITIAL_LR As Double = 0.015
    Private Const FINAL_LR As Double = 0.001
    Private Const DENSIFY_INTERVAL As Integer = 100
    Private Const DENSIFY_FROM_ITER As Integer = 50
    Private Const DENSIFY_UNTIL_ITER As Integer = 400

    ' ===== 路径配置 =====
    Private ReadOnly TestDataDir As String = "/home/z/my-project/download/test_data"
    Private ReadOnly OutputDir As String = "/home/z/my-project/download/output"

    Sub Main(args As String())
        Console.WriteLine("="c, 70)
        Console.WriteLine("  3D Gaussian Splatting - VB.NET Implementation")
        Console.WriteLine("  基于 Tensor 类的多视图三维重建")
        Console.WriteLine("="c, 70)
        Console.WriteLine()

        ' 解析命令行参数
        Dim cmd = If(args.Length > 0, args(0).ToLower(), "train")
        Select Case cmd
            Case "train"
                RunTraining(args)
            Case "render"
                RunRenderOnly(args)
            Case "info"
                ShowInfo()
            Case "help", "--help", "-h"
                ShowHelp()
            Case Else
                Console.WriteLine($"未知命令: {cmd}")
                ShowHelp()
        End Select
    End Sub

    ''' <summary>
    ''' 训练流程
    ''' </summary>
    Private Sub RunTraining(args As String())
        Dim startTime = DateTime.Now
        Console.WriteLine("[1/5] 加载测试数据 ...")
        If Not Directory.Exists(TestDataDir) Then
            Console.WriteLine($"错误: 测试数据目录不存在: {TestDataDir}")
            Console.WriteLine("请先运行 Python 脚本生成测试数据: python scripts/generate_scene.py")
            Return
        End If

        ' 加载相机参数
        Dim Intrinsics As CameraIntrinsics, Cameras As List(Of Camera)
        With Camera.LoadFromJson(Path.Combine(TestDataDir, "cameras.json"))
            Intrinsics = .Intrinsics
            Cameras = .Cameras
        End With

        Console.WriteLine($"  - 内参: fx={intrinsics.Fx}, fy={intrinsics.Fy}, " &
                          $"cx={intrinsics.Cx}, cy={intrinsics.Cy}")
        Console.WriteLine($"  - 视图数量: {cameras.Count}")

        ' 加载目标图像
        Dim targetImages = ImageLoader.LoadAll(TestDataDir, cameras.Count)
        If targetImages.Count = 0 Then
            Console.WriteLine("错误: 未找到任何 PNG 图像")
            Return
        End If
        Console.WriteLine($"  - 加载图像数量: {targetImages.Count}")

        ' 创建输出目录
        If Not Directory.Exists(OutputDir) Then
            Directory.CreateDirectory(OutputDir)
        End If

        ' ---- Step 2: 初始化高斯模型 ----
        Console.WriteLine()
        Console.WriteLine("[2/5] 初始化高斯模型 ...")
        Dim model = InitializeModel(intrinsics, cameras, targetImages)
        Console.WriteLine($"  - 初始高斯数量: {model.Count}")

        ' ---- Step 3: 训练循环 ----
        Console.WriteLine()
        Console.WriteLine("[3/5] 开始训练 ...")
        Console.WriteLine($"  - 迭代次数: {NUM_ITERATIONS}")
        Console.WriteLine($"  - 初始学习率: {INITIAL_LR}")
        Console.WriteLine($"  - 最终学习率: {FINAL_LR}")

        Dim optimizer As New AdamOptimizer(INITIAL_LR)
        Dim scheduler As New LRScheduler(INITIAL_LR, FINAL_LR, NUM_ITERATIONS)
        Dim gradCalc As New GradientCalculator(epsilon:=0.01, batchSize:=0)
        Dim densityCtrl As New DensityController() With {
            .DensifyInterval = DENSIFY_INTERVAL
        }

        ' 训练日志
        Dim logPath = Path.Combine(OutputDir, "training_log.csv")
        Using logSw As New StreamWriter(logPath)
            logSw.WriteLine("iteration,num_gaussians,loss,psnr,lr,cloned,split,pruned")

            For iter = 0 To NUM_ITERATIONS - 1
                ' 更新学习率
                optimizer.LearningRate = scheduler.GetLr(iter)

                ' 计算梯度
                gradCalc.ComputeGradients(model, cameras, targetImages)

                ' 优化器更新
                optimizer.StepModel(model)

                ' 自适应密度控制
                Dim stats = (0, 0, 0)
                If iter >= DENSIFY_FROM_ITER AndAlso iter < DENSIFY_UNTIL_ITER Then
                    stats = densityCtrl.Densify(model, iter)
                End If

                ' 每 10 步评估一次
                If iter Mod 10 = 0 OrElse iter = NUM_ITERATIONS - 1 Then
                    Dim avgLoss = 0.0
                    Dim avgPsnr = 0.0
                    For v = 0 To cameras.Count - 1
                        Dim rendered = SplatRenderer.Render(model, cameras(v))
                        avgLoss += SplatRenderer.ComputeMSE(rendered, targetImages(v))
                        avgPsnr += SplatRenderer.ComputePSNR(rendered, targetImages(v))
                    Next
                    avgLoss /= cameras.Count
                    avgPsnr /= cameras.Count

                    Console.WriteLine($"  iter {iter,4:D} | gaussians {model.Count,6:D} | " &
                                      $"loss {avgLoss:F6} | psnr {avgPsnr:F2} dB | " &
                                      $"lr {optimizer.LearningRate:F5} | " &
                                      $"+{stats.Item1} ~{stats.Item2} -{stats.Item3}")
                    logSw.WriteLine($"{iter},{model.Count},{avgLoss:F6},{avgPsnr:F4},{optimizer.LearningRate:F6}," &
                                    $"{stats.Item1},{stats.Item2},{stats.Item3}")
                    logSw.Flush()
                End If
            Next
        End Using
        Console.WriteLine($"  训练日志已保存: {logPath}")

        ' ---- Step 4: 保存输出 ----
        Console.WriteLine()
        Console.WriteLine("[4/5] 保存输出 ...")
        Dim plyPath = Path.Combine(OutputDir, "reconstruction.ply")
        PointCloudIO.SaveGaussianPLY(plyPath, model)
        Console.WriteLine($"  - 点云已保存: {plyPath}")

        ' 渲染并保存每个视图的结果
        Dim renderDir = Path.Combine(OutputDir, "renders")
        If Not Directory.Exists(renderDir) Then
            Directory.CreateDirectory(renderDir)
        End If
        For v = 0 To cameras.Count - 1
            Dim rendered = SplatRenderer.Render(model, cameras(v))
            ImageLoader.Save(rendered, Path.Combine(renderDir, $"render_{v:00}.png"))
        Next
        Console.WriteLine($"  - 渲染结果已保存: {renderDir}")

        ' ---- Step 5: 评估 ----
        Console.WriteLine()
        Console.WriteLine("[5/5] 最终评估 ...")
        Dim finalLoss = 0.0, finalPsnr = 0.0
        For v = 0 To cameras.Count - 1
            Dim rendered = SplatRenderer.Render(model, cameras(v))
            finalLoss += SplatRenderer.ComputeMSE(rendered, targetImages(v))
            finalPsnr += SplatRenderer.ComputePSNR(rendered, targetImages(v))
        Next
        finalLoss /= cameras.Count
        finalPsnr /= cameras.Count
        Console.WriteLine($"  - 最终高斯数量: {model.Count}")
        Console.WriteLine($"  - 最终 MSE: {finalLoss:F6}")
        Console.WriteLine($"  - 最终 PSNR: {finalPsnr:F2} dB")
        Console.WriteLine($"  - 总耗时: {(DateTime.Now - startTime).TotalSeconds:F1} 秒")

        Console.WriteLine()
        Console.WriteLine("训练完成！")
        Console.WriteLine($"输出目录: {OutputDir}")
    End Sub

    ''' <summary>
    ''' 仅渲染模式：加载已训练的模型并渲染
    ''' </summary>
    Private Sub RunRenderOnly(args As String())
        Console.WriteLine("渲染模式（待实现：加载 PLY 并渲染）")
    End Sub

    ''' <summary>
    ''' 初始化高斯模型
    ''' 策略：从所有视图的像素反投影到 3D 空间，生成初始点云
    ''' </summary>
    Private Function InitializeModel(intrinsics As CameraIntrinsics, cameras As List(Of Camera), targetImages As List(Of Tensor)) As GaussianModel
        ' 方法 1: 从第一个视图的像素随机采样反投影
        ' 方法 2: 在场景中心区域随机生成
        ' 这里采用方法 2 + 方法 1 的混合策略

        Dim rng As New Random(42)
        Dim positions As New List(Of Double())()
        Dim colors As New List(Of Double())()

        ' 从第一个视图采样像素并反投影
        Dim cam0 = cameras(0)
        Dim img0 = targetImages(0)
        Dim W = intrinsics.Width
        Dim H = intrinsics.Height
        Dim samplesPerPixel = INITIAL_GAUSSIANS \ (W * H) + 1

        For y = 0 To H - 1 Step 5
            For x = 0 To W - 1 Step 5
                Dim r = img0(y, x, 0)
                Dim g = img0(y, x, 1)
                Dim b = img0(y, x, 2)
                ' 跳过背景（黑色）
                If r + g + b < 0.1 Then Continue For

                ' 反投影到 3D：假设深度在 [1, 5] 范围内随机
                Dim depth = 1.5 + rng.NextDouble() * 2.5
                Dim xc = (x - intrinsics.Cx) / intrinsics.Fx * depth
                Dim yc = (y - intrinsics.Cy) / intrinsics.Fy * depth
                Dim zc = depth

                ' 变换到世界坐标系：P_world = R^T * (P_cam - t)
                Dim matR = cam0.Extrinsics.R
                Dim t = cam0.Extrinsics.t
                Dim xt = xc - t(0)
                Dim yt = yc - t(1)
                Dim zt = zc - t(2)
                Dim wx = matR(0, 0) * xt + matR(1, 0) * yt + matR(2, 0) * zt
                Dim wy = matR(0, 1) * xt + matR(1, 1) * yt + matR(2, 1) * zt
                Dim wz = matR(0, 2) * xt + matR(1, 2) * yt + matR(2, 2) * zt

                positions.Add({wx, wy, wz})
                colors.Add({r, g, b})
            Next
        Next

        ' 如果采样点不够，在场景中心区域补充随机点
        While positions.Count < INITIAL_GAUSSIANS
            Dim x = (rng.NextDouble() - 0.5) * 4
            Dim y = (rng.NextDouble() - 0.5) * 2 + 0.5
            Dim z = (rng.NextDouble() - 0.5) * 4
            positions.Add({x, y, z})
            colors.Add({rng.NextDouble(), rng.NextDouble(), rng.NextDouble()})
        End While

        ' 限制数量
        If positions.Count > INITIAL_GAUSSIANS Then
            positions = positions.GetRange(0, INITIAL_GAUSSIANS)
            colors = colors.GetRange(0, INITIAL_GAUSSIANS)
        End If

        ' 转换为 Tensor 并使用 FromPointCloud 创建模型
        Dim posTensor = New Tensor(positions.Count, 3)
        Dim colTensor = New Tensor(positions.Count, 3)
        For i = 0 To positions.Count - 1
            posTensor(i, 0) = positions(i)(0)
            posTensor(i, 1) = positions(i)(1)
            posTensor(i, 2) = positions(i)(2)
            colTensor(i, 0) = colors(i)(0)
            colTensor(i, 1) = colors(i)(1)
            colTensor(i, 2) = colors(i)(2)
        Next

        Return GaussianModel.FromPointCloud(posTensor, colTensor, 0.05F)
    End Function

    Private Sub ShowInfo()
        Console.WriteLine("3D Gaussian Splatting - VB.NET Implementation")
        Console.WriteLine()
        Console.WriteLine("测试数据目录: " & TestDataDir)
        If Directory.Exists(TestDataDir) Then
            Console.WriteLine("测试数据文件:")
            For Each f In Directory.GetFiles(TestDataDir)
                Dim fi As New FileInfo(f)
                Console.WriteLine($"  {Path.GetFileName(f),-30} {fi.Length,10:N0} bytes")
            Next
        Else
            Console.WriteLine("  (目录不存在，请先运行 Python 脚本生成测试数据)")
        End If
        Console.WriteLine()
        Console.WriteLine("输出目录: " & OutputDir)
    End Sub

    Private Sub ShowHelp()
        Console.WriteLine("用法: GaussianSplatting [command]")
        Console.WriteLine()
        Console.WriteLine("命令:")
        Console.WriteLine("  train    训练 3D 高斯模型（默认）")
        Console.WriteLine("  render   仅渲染已训练的模型")
        Console.WriteLine("  info     显示测试数据和输出信息")
        Console.WriteLine("  help     显示此帮助信息")
        Console.WriteLine()
        Console.WriteLine("示例:")
        Console.WriteLine("  GaussianSplatting train")
        Console.WriteLine("  GaussianSplatting info")
    End Sub

End Module
