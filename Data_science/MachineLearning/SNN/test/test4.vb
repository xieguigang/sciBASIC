' ============================================================================
' test4.vb — 稀疏连接组仿真的 CUDA 端到端对拍与规模化性能
'
' 验证目标：同一个稀疏 SNN，在 CPU（SIMD）与 CUDA 两种张量计算后端下跑完整前向
' 仿真，输出脉冲计数应当一致 —— 说明 SparseMatrix.SpMM 已透明地走可插拔后端。
'
' 性能策略（与设计一致）：只把 SpMM 放到 GPU，LIF 逐元素运算留在 CPU。
' 实现方式：临时把 CudaTensor.MinGpuElements 调到极大，使逐元素算子回退 CPU，
' 而稀疏 SpMM 由 MinSparseNnz 单独控制 —— 从而避免每步逐元素算子的 PCIe 往返。
'
' 说明：
'   * 编码使用 LatencyCoding（确定性），保证 CPU/GPU 的输入脉冲序列完全相同；
'   * 本机无 CUDA / NVRTC 时打印原因并跳过（自动回退 CPU），不影响其它测试。
' ============================================================================

Imports System.Diagnostics
Imports Microsoft.VisualBasic.DeepLearning.SpikingNeuralNetwork
Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports gpu = Microsoft.VisualBasic.Computing.ILCuda.GPUTensor
Imports ILCudaRuntime = Microsoft.VisualBasic.Computing.ILCuda.Runtime
Imports std = System.Math

Module test4

    ''' <summary>
    ''' 随机连接组（每行 fanIn 条边 + 8 条自反馈），返回 (pre, post, weight) 三元组。
    ''' </summary>
    Private Function BuildConnectome(rng As Random, n As Integer, fanIn As Integer) As (pre As Integer(), post As Integer(), weight As Double())
        Dim total = n * fanIn + 8
        Dim pre(total - 1) As Integer
        Dim post(total - 1) As Integer
        Dim w(total - 1) As Double

        For r As Integer = 0 To n - 1
            For k As Integer = 0 To fanIn - 1
                Dim idx = r * fanIn + k
                pre(idx) = r
                post(idx) = rng.Next(n)
                w(idx) = 1.0 + rng.NextDouble() * 3.0     ' 模拟突触计数
            Next
        Next
        For j As Integer = 0 To 7
            Dim idx = n * fanIn + j
            pre(idx) = j
            post(idx) = j
            w(idx) = 2.0                                  ' 自反馈
        Next

        Return (pre, post, w)
    End Function

    ''' <summary>构造稀疏网络（确定性时延编码 + 扇入归一化）。</summary>
    Private Function BuildNetwork(rng As Random, n As Integer, T As Integer, fanIn As Integer) As SpikingNetwork
        Dim conn = BuildConnectome(rng, n, fanIn)

        Dim net As New SpikingNetwork(n, T, SpikeEncoding.LatencyCoding)
        net.AddSparseLayer(conn.pre, conn.post, conn.weight, n,
                           normalization:=SparseNormalization.FanIn,
                           beta:=0.9, threshold:=1.0)
        Return net
    End Function

    ''' <summary>前 sensory 个神经元被外部驱动（确定性输入）。</summary>
    Private Function BuildInput(rng As Random, n As Integer, batch As Integer, sensory As Integer) As Tensor
        Dim x(batch * n - 1) As Double
        For b As Integer = 0 To batch - 1
            For i As Integer = 0 To sensory - 1
                x(b * n + i) = 0.5 + 0.4 * rng.NextDouble()
            Next
        Next
        Return New Tensor(x, batch, n)
    End Function

    Private Function MaxDiff(a As Double(), b As Double()) As Double
        Dim m As Double = 0
        For i As Integer = 0 To a.Length - 1
            m = std.Max(m, std.Abs(a(i) - b(i)))
        Next
        Return m
    End Function

    Public Sub SparseCudaDemo()
        Console.WriteLine("================================================================")
        Console.WriteLine("  Part 4 · 稀疏连接组仿真 · CUDA 端到端对拍")
        Console.WriteLine("================================================================")
        Console.WriteLine()
        Console.WriteLine($"  初始张量计算后端: {Tensor.computeKernel.Name}")
        Console.WriteLine()

        ' ---- 小规模：正确性对拍 ----
        Const nSmall As Integer = 256
        Const fanInSmall As Integer = 8
        Const tSmall As Integer = 32
        Const batchSmall As Integer = 4

        Dim rngA As New Random(20240916)
        Dim netSmall = BuildNetwork(rngA, nSmall, tSmall, fanInSmall)
        Dim xSmall = BuildInput(rngA, nSmall, batchSmall, 32)

        ' ---- 较大规模：性能对比（更接近真实连接组的使用形态）----
        Const nScale As Integer = 20000
        Const fanInScale As Integer = 64
        Const tScale As Integer = 8

        Dim rngB As New Random(777)
        Dim netScale = BuildNetwork(rngB, nScale, tScale, fanInScale)
        Dim xScale = BuildInput(rngB, nScale, 1, 200)

        Console.WriteLine($"  小规模网络: N={nSmall}, nnz={netSmall.SparseLayer.Synapses.NonZeros}")
        Console.WriteLine($"  规模网络:   N={nScale}, nnz={netScale.SparseLayer.Synapses.NonZeros}, T={tScale}, batch=1")
        Console.WriteLine()

        ' ---- CPU 基准 ----
        Dim swCpu = Stopwatch.StartNew()
        Dim cpuSmall = netSmall.ForwardSpikes(xSmall)
        swCpu.Stop()
        Dim cpuSmallMs = swCpu.Elapsed.TotalMilliseconds

        swCpu.Restart()
        Dim cpuScale = netScale.ForwardSpikes(xScale)
        swCpu.Stop()
        Dim cpuScaleMs = swCpu.Elapsed.TotalMilliseconds

        Console.WriteLine($"  CPU  小规模前向: {cpuSmallMs,9:F3} ms")
        Console.WriteLine($"  CPU  规模前向:   {cpuScaleMs,9:F3} ms")

        ' ---- 注册 CUDA ----
        Dim opts As New ILCudaRuntime.EngineOptions()

        If Not gpu.CudaTensor.Register(opts) Then
            Console.WriteLine()
            Console.WriteLine($"  [跳过] CUDA 不可用: {gpu.CudaTensor.LastError}")

            For Each line In opts.Diagnostics
                Console.WriteLine($"      {line}")
            Next

            Console.WriteLine("  [OK] 稀疏仿真已自动回退 CPU 后端，结果不受影响。")
            Return
        End If

        Console.WriteLine($"  [OK] 已切换到后端: {Tensor.computeKernel.Name}")

        If gpu.CudaTensor.KernelFailures.Count > 0 Then
            For Each f In gpu.CudaTensor.KernelFailures
                Console.WriteLine($"      [内核翻译失败] {f.Key} -> {f.Value}")
            Next
        End If

        ' 只把 SpMM 放 GPU：逐元素算子阈值调到极大 → LIF 留在 CPU
        Dim savedMinGpu = gpu.CudaTensor.MinGpuElements
        Dim savedMinNnz = gpu.CudaTensor.MinSparseNnz
        gpu.CudaTensor.MinGpuElements = Integer.MaxValue
        gpu.CudaTensor.MinSparseNnz = 1

        Dim swGpu = Stopwatch.StartNew()
        Dim gpuSmall = netSmall.ForwardSpikes(xSmall)
        swGpu.Stop()
        Dim gpuSmallMs = swGpu.Elapsed.TotalMilliseconds

        ' 预热一次（CSR / 显存缓冲驻留），再计时
        Call netScale.ForwardSpikes(xScale)
        swGpu.Restart()
        Dim gpuScale = netScale.ForwardSpikes(xScale)
        swGpu.Stop()
        Dim gpuScaleMs = swGpu.Elapsed.TotalMilliseconds

        gpu.CudaTensor.MinGpuElements = savedMinGpu
        gpu.CudaTensor.MinSparseNnz = savedMinNnz

        Console.WriteLine($"  CUDA 小规模前向: {gpuSmallMs,9:F3} ms")
        Console.WriteLine($"  CUDA 规模前向:   {gpuScaleMs,9:F3} ms   (含首次上传的预热调用不参与计时)")
        Console.WriteLine()

        ' ---- 对拍 ----
        Dim errSmall = MaxDiff(cpuSmall.Data, gpuSmall.Data)
        Dim errScale = MaxDiff(cpuScale.Data, gpuScale.Data)

        If errSmall < 0.000000001 AndAlso errScale < 0.000000001 Then
            Console.WriteLine($"  [PASS] CPU / GPU 脉冲计数一致（maxdiff: 小规模={errSmall:E2}, 规模={errScale:E2}）")
        Else
            Console.WriteLine($"  [FAIL] CPU / GPU 脉冲计数不一致（小规模={errSmall:E2}, 规模={errScale:E2}）")
        End If

        Console.WriteLine()
        Console.WriteLine($"  性能：小规模 {cpuSmallMs / gpuSmallMs:F2}x，规模 {cpuScaleMs / gpuScaleMs:F2}x")
        Console.WriteLine("  说明：脉冲输入高度稀疏，CPU 的 SpMM 会跳过零源（xv = 0），在中低发放率下")
        Console.WriteLine("        反而很有竞争力；GPU 每次调用有固定开销（内核启动 + 显存往返，约 0.6~1 ms/步），")
        Console.WriteLine("        需要足够大的『每步非零计算量』（高发放率 × 大规模 nnz）才能摊薄。")
        Console.WriteLine("        裸内核基准见 ILCudaTensor 测试：nnz=1.28M 稠密输入时 SpMM 加速约 4.7x。")

        ' 恢复默认后端，避免影响后续演示
        gpu.CudaTensor.Unregister()
        Console.WriteLine($"  已恢复默认后端: {Tensor.computeKernel.Name}")
    End Sub

End Module
