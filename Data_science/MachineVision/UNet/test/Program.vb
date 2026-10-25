Imports System
Imports System.IO
Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports Microsoft.VisualBasic.MachineLearning.TensorFlow.Compute
Imports UNet
Imports ILCudaRuntime = Microsoft.VisualBasic.Computing.ILCuda.Runtime
Imports gpu = Microsoft.VisualBasic.Computing.ILCuda.GPUTensor

Module Program

    ''' <summary>
    ''' 尝试把 fp64 与 fp32 两条张量栈切换到 CUDA 后端。
    ''' </summary>
    ''' <remarks>
    ''' CudaTensor.Register 默认会一并 Adopt 单精度栈，因此一次调用即可；
    ''' 失败时不改变当前后端（仍为 SIMD），并返回 False 由调用方降级。
    ''' </remarks>
    Public Function TryEnableGpu() As Boolean
        Dim options As New ILCudaRuntime.EngineOptions()

        If gpu.CudaTensor.Register(options) Then
            Console.WriteLine($"    [OK] fp64 后端 = {Tensor.computeKernel.Name}, fp32 后端 = {TensorF.computeKernelF.Name}")

            If gpu.CudaTensorF.Current IsNot Nothing Then
                Console.WriteLine("    " & gpu.CudaTensorF.Current.DescribeKernels().Replace(vbLf, vbLf & "    "))
            End If

            If gpu.CudaTensor.KernelFailures.Count > 0 Then
                For Each f In gpu.CudaTensor.KernelFailures
                    Console.WriteLine($"    [内核翻译失败] {f.Key} -> {f.Value}")
                Next
            End If

            Return True
        Else
            Console.WriteLine($"    [回退] GPU 注册失败, 继续使用 CPU: {gpu.CudaTensor.LastError}")

            If options.Diagnostics IsNot Nothing Then
                For Each line In options.Diagnostics
                    Console.WriteLine($"      {line}")
                Next
            End If

            Return False
        End If
    End Function

    ''' <summary>打印后端与混合精度策略，避免"以为在 GPU 上其实没有"。</summary>
    Public Sub PrintEnvironment(cfg As RunConfig)
        Console.WriteLine($"    fp64 后端 = {Tensor.computeKernel.Name}")
        Console.WriteLine($"    fp32 后端 = {TensorF.computeKernelF.Name}")
        Console.WriteLine($"    混合精度  = {If(cfg.MixedPrecision, "启用", "关闭")} " &
                          $"(卷积阈值 ConvOpsThreshold={MixedPrecisionPolicy.ConvOpsThreshold})")

        PrecisionRouter.Enabled = cfg.MixedPrecision
        MixedPrecisionPolicy.Enabled = cfg.MixedPrecision
    End Sub

    Private Sub PrintUsage()
        Console.WriteLine("U-Net 混合精度图像分割 (DSB2018 细胞核分割数据集)")
        Console.WriteLine()
        Console.WriteLine("用法:")
        Console.WriteLine("  test.exe demo     [选项]   用 stage1_train 训练, 在 stage1_test 上评估并落盘可视化")
        Console.WriteLine("  test.exe selftest [选项]   跑断言自检(数值梯度/真实数据/混合精度/存档/GPU 对拍)")
        Console.WriteLine("  test.exe help               打印本帮助")
        Console.WriteLine()
        Console.WriteLine("选项:")
        Console.WriteLine("  --data <path>     数据集根目录  (默认 C:\Users\Administrator\Downloads\unet-dataset)")
        Console.WriteLine("  --size <n>        模型输入边长  (默认 64)")
        Console.WriteLine("  --levels <n>      下采样层数    (默认 3)")
        Console.WriteLine("  --filters <n>     第一层通道数  (默认 8)")
        Console.WriteLine("  --epochs <n>      训练轮数      (默认 6)")
        Console.WriteLine("  --limit <n>       训练样本上限  (默认 40, 0 = 全量 670)")
        Console.WriteLine("  --testlimit <n>   测试样本上限  (默认 12, 0 = 全部 65)")
        Console.WriteLine("  --lr <f>          学习率        (默认 0.005)")
        Console.WriteLine("  --batch <n>       小批量大小    (默认 4)")
        Console.WriteLine("  --out <dir>       落盘目录      (默认 .\unet-out)")
        Console.WriteLine("  --gpu             注册 CUDA 后端后再跑")
        Console.WriteLine("  --no-cache        不使用数据集解码缓存")
        Console.WriteLine("  --no-mixed        关闭混合精度(全程 fp64)")
    End Sub

    Function Main(args As String()) As Integer
        Dim command As String = "demo"
        Dim useGpu As Boolean = False
        Dim cfg As New RunConfig()
        Dim i As Integer = 0

        While i < args.Length
            Dim arg = args(i)

            Select Case arg.ToLower()
                Case "demo", "selftest"
                    command = arg.ToLower()
                Case "help", "-h", "--help", "-?"
                    Call PrintUsage()
                    Return 0
                Case "--gpu"
                    useGpu = True
                Case "--no-cache"
                    cfg.UseCache = False
                Case "--no-mixed"
                    cfg.MixedPrecision = False
                Case "--data"
                    i += 1 : cfg.DataRoot = args(i)
                Case "--size"
                    i += 1 : cfg.ImageSize = Integer.Parse(args(i))
                Case "--levels"
                    i += 1 : cfg.Levels = Integer.Parse(args(i))
                Case "--filters"
                    i += 1 : cfg.BaseFilters = Integer.Parse(args(i))
                Case "--epochs"
                    i += 1 : cfg.Epochs = Integer.Parse(args(i))
                Case "--limit"
                    i += 1 : cfg.Limit = Integer.Parse(args(i))
                Case "--testlimit"
                    i += 1 : cfg.TestLimit = Integer.Parse(args(i))
                Case "--lr"
                    i += 1 : cfg.LearningRate = Double.Parse(args(i))
                Case "--batch"
                    i += 1 : cfg.BatchSize = Integer.Parse(args(i))
                Case "--out"
                    i += 1 : cfg.OutputDir = args(i)
                Case Else
                    Console.WriteLine($"未知参数: {arg}")
                    Call PrintUsage()
                    Return 2
            End Select

            i += 1
        End While

        Try
            If command = "selftest" Then
                Return UNetSelfTest.Run(cfg, useGpu)
            Else
                Return UNetDemo.Run(cfg, useGpu)
            End If
        Catch ex As Exception
            Console.WriteLine($"[异常] {ex.GetType().Name}: {ex.Message}")
            Console.WriteLine(ex.StackTrace)
            Return 1
        End Try
    End Function

End Module
