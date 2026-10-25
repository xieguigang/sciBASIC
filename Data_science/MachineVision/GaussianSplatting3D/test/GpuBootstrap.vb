' ---------------------------------------------------------------------------
' CUDA 后端注册（仅在测试宿主中执行）
'
' 库本身不引用 ILCudaTensor / ILCuda —— 只要宿主在启动时把
' Tensor.computeKernel / TensorF.computeKernelF 换成 CUDA 实现，
' 库内所有经张量后端的批量运算就会自动落到 GPU 上。
' ---------------------------------------------------------------------------

Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports Microsoft.VisualBasic.MachineLearning.TensorFlow.Compute
Imports gpu = Microsoft.VisualBasic.Computing.ILCuda.GPUTensor
Imports ILCudaRuntime = Microsoft.VisualBasic.Computing.ILCuda.Runtime

Module GpuBootstrap

    Private _enabled As Boolean

    ''' <summary>是否成功切换到 GPU 后端。</summary>
    Public ReadOnly Property Enabled As Boolean
        Get
            Return _enabled
        End Get
    End Property

    ''' <summary>
    ''' 尝试把 fp64 / fp32 两条张量栈切换到 CUDA 后端。
    ''' </summary>
    ''' <param name="strict">True 时把显存调度阈值压到 0，强制小张量也走内核（仅诊断用）</param>
    Public Function TryEnableGpu(Optional strict As Boolean = False) As Boolean
        Try
            Dim options As New ILCudaRuntime.EngineOptions()

            If Not gpu.CudaTensor.Register(options) Then
                Console.WriteLine($"    [回退] CUDA 注册失败，继续使用 CPU 后端: {gpu.CudaTensor.LastError}")
                _enabled = False
                Return False
            End If

            If strict Then
                gpu.CudaTensor.MinGpuElements = 0
                gpu.CudaTensor.MinGemmElements = 0
                gpu.CudaTensorF.MinGpuElementsF = 0
                gpu.CudaTensorF.MinGemmElementsF = 0
            End If

            _enabled = True

            Console.WriteLine($"    [OK] fp64 后端 = {Tensor.computeKernel.Name}")
            Console.WriteLine($"    [OK] fp32 后端 = {TensorF.computeKernelF.Name}")

            If strict AndAlso gpu.CudaTensorF.Current IsNot Nothing Then
                Console.WriteLine("    " & gpu.CudaTensorF.Current.DescribeKernels().Replace(vbLf, vbLf & "    "))
            End If

            If gpu.CudaTensor.KernelFailures.Count > 0 Then
                For Each f In gpu.CudaTensor.KernelFailures
                    Console.WriteLine($"    [内核翻译失败] {f.Key} -> {f.Value}")
                Next
            End If

            Return True
        Catch ex As Exception
            Console.WriteLine($"    [回退] CUDA 初始化异常，继续使用 CPU 后端: {ex.Message}")
            _enabled = False
            Return False
        End Try
    End Function

    ''' <summary>打印后端与混合精度策略（避免"以为在 GPU 上其实没有"）。</summary>
    Public Sub PrintBackends(prefix As String)
        Console.WriteLine($"{prefix}fp64 张量后端 : {Tensor.computeKernel.Name}")
        Console.WriteLine($"{prefix}fp32 张量后端 : {TensorF.computeKernelF.Name}")
        Console.WriteLine($"{prefix}光栅化后端    : {SplatBackends.Default.Name}")
        Console.WriteLine($"{prefix}混合精度决策  : ElementwiseThreshold={MixedPrecisionPolicy.ElementwiseThreshold}, " &
                          $"GemmOpsThreshold={MixedPrecisionPolicy.GemmOpsThreshold}, Enabled={MixedPrecisionPolicy.Enabled}")
    End Sub

End Module
