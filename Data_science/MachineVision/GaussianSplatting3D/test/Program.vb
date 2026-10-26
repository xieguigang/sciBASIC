' ---------------------------------------------------------------------------
' 3D 高斯溅射测试宿主
'
' 命令：
'   train   训练（默认）
'   render  仅渲染
'   info    打印数据与后端信息
'   bench   分阶段性能基准
'   verify  解析梯度 / 前向 / 精度 / 后端回退 四组对拍
'   help    帮助
'
' 全局开关：
'   --gpu         尝试注册 CUDA 后端（fp64 + fp32 两条栈）
'   --gpu-strict  额外把显存调度阈值压到 0（诊断用）
'   --no-gpu      强制纯 CPU
' ---------------------------------------------------------------------------

Imports System.IO
Imports Microsoft.VisualBasic.MachineVision.GaussianSplatting3D

Module Program

    Private ReadOnly GpuFlags As String() = {"--gpu", "--gpu-strict", "--no-gpu"}

    Sub Main(args As String())
        Console.OutputEncoding = System.Text.Encoding.UTF8

        Dim gpuRequested = args.Any(Function(a) a = "--gpu" OrElse a = "--gpu-strict")
        Dim gpuStrict = args.Any(Function(a) a = "--gpu-strict")
        Dim gpuDisabled = args.Any(Function(a) a = "--no-gpu")

        Dim positionals = args.Where(Function(a) Not a.StartsWith("--") AndAlso Not GpuFlags.Contains(a)).ToArray()
        Dim command = If(positionals.Length > 0, positionals(0).ToLowerInvariant(), "train")

        If command = "help" OrElse command = "-h" Then
            Call ShowHelp()
            Return
        End If

        Console.WriteLine(New String("="c, 78))
        Console.WriteLine("  3D Gaussian Splatting — optimized pipeline (VB.NET)")
        Console.WriteLine(New String("="c, 78))

        If gpuRequested AndAlso Not gpuDisabled Then
            Call TryEnableGpu(gpuStrict)
        Else
            Console.WriteLine("    [CPU] 未请求 CUDA 后端（加 --gpu 可启用）")
        End If

        Call PrintBackends("    ")

        Select Case command
            Case "train"
                Call TrainCommand.Run(args)
            Case "render"
                Call TrainCommand.RunRenderOnly(args)
            Case "info"
                Call ShowInfo()
            Case "bench"
                Call BenchCommand.Run(args)
            Case "verify"
                Call VerifyCommand.Run(args)
            Case Else
                Console.WriteLine($"未知命令: {command}")
                Call ShowHelp()
        End Select
    End Sub

    Private Sub ShowInfo()
        Console.WriteLine()
        Console.WriteLine("测试数据目录: " & TestDataDir)

        If Directory.Exists(TestDataDir) Then
            Console.WriteLine("测试数据文件:")

            For Each f In Directory.GetFiles(TestDataDir)
                Dim fi As New FileInfo(f)
                Console.WriteLine($"  {Path.GetFileName(f),-28} {fi.Length,12:N0} bytes")
            Next
        Else
            Console.WriteLine("  (目录不存在，请先运行 test_data/generate_scene.py)")
        End If

        Console.WriteLine()
        Console.WriteLine("输出目录: " & OutputDir)
        Console.WriteLine()
        Call PrintBackends("  ")

        Console.WriteLine()
        Console.WriteLine($"可用的光栅化后端: {String.Join(", ", SplatBackends.Available().Select(Function(b) b.Name))}")
        Console.WriteLine($"默认渲染参数: tile={SplatRenderOptions.Default.TileSize}, " &
                          $"sigma={SplatRenderOptions.Default.RadiusSigma}, " &
                          $"exp={SplatRenderOptions.Default.ExpMode}, " &
                          $"dotnet={Environment.Version}, cores={Environment.ProcessorCount}")
    End Sub

    Private Sub ShowHelp()
        Console.WriteLine("用法: GaussianSplatting <command> [options]")
        Console.WriteLine()
        Console.WriteLine("命令:")
        Console.WriteLine("  train    训练 3D 高斯模型（默认）")
        Console.WriteLine("  render   仅渲染已训练的模型（当前从随机初始化开始）")
        Console.WriteLine("  info     显示测试数据、后端与渲染参数")
        Console.WriteLine("  bench    分阶段性能基准（渲染 / 梯度 / 优化器 / 差分基线）")
        Console.WriteLine("  verify   对拍：解析梯度 vs 中心差分、tile 前向 vs 参考实现、精度与后端回退")
        Console.WriteLine("  help     显示此帮助信息")
        Console.WriteLine()
        Console.WriteLine("全局开关:")
        Console.WriteLine("  --gpu          注册 CUDA 张量后端（一次调用同时装配 fp64 / fp32）")
        Console.WriteLine("  --gpu-strict   额外把显存调度阈值压到 0 并打印内核清单")
        Console.WriteLine("  --no-gpu       强制纯 CPU")
        Console.WriteLine()
        Console.WriteLine("train 选项: --iterations N  --gaussians N  --eval-every N  --views N")
        Console.WriteLine("            --legacy（中心差分）  --profile（分阶段计时）  --no-densify")
        Console.WriteLine("bench 选项: --gaussians N  --views N  --repeat N")
        Console.WriteLine("verify 选项: --gaussians N  --sample N")
    End Sub

End Module
