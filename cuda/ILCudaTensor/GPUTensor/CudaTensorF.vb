' ---------------------------------------------------------------------------
' 单精度（float32）张量计算后端：ITensorComputeF 的 CUDA GPU 实现
'
' 用法：
'     If CudaTensorF.Register() Then
'         ' 之后所有 TensorF 的运算自动走 GPU
'     End If
'
' 与双精度后端 CudaTensor 的关系：
'   * 两者**共享同一个 CudaEngine**（第一次注册时创建），不额外占用 CUDA 上下文；
'   * 内核名前缀不同（il_tensor_ vs il_tensorf_ / tensor* vs tensorF32*），
'     两条栈可以在同一个 NVRTC 编译单元里共存而不撞名；
'   * 逐元素 / 标量算子同样由 IL2Cuda 自动生成（FloatKernels.vb），
'     手写内核只有"块内共享内存归约"与"三维 CFD stencil"这两类。
'
' 为什么需要单精度栈：消费级显卡的 FP64 吞吐只有 FP32 的 1/64（专业卡约 1/32），
' 物理仿真 / CFD 这类算得动但精度要求不高的负载只有走 float32 才有实际收益。
'
' 回退策略（与双精度后端一致）：张量太小 / 内核缺失 / NVRTC 编译失败时
' 一律静默回退 MyBase（TensorComputeFBase 的 CPU 参考实现），绝不抛异常。
' ---------------------------------------------------------------------------

Imports ILCudaRuntime = Microsoft.VisualBasic.Computing.ILCuda.Runtime
Imports ILCudaKernels = Microsoft.VisualBasic.Computing.ILCuda.Kernels
Imports tf = Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports tfCompute = Microsoft.VisualBasic.MachineLearning.TensorFlow.Compute
Imports std = System.Math

Namespace GPUTensor

    ''' <summary>
    ''' <see cref="tfCompute.ITensorComputeF"/> 的 CUDA GPU 实现（单精度 float32）。
    ''' </summary>
    Public Class CudaTensorF
        Inherits tfCompute.TensorComputeFBase
        Implements IDisposable

        ''' <summary>默认的显存驻留缓存上限（512 MiB）</summary>
        Public Const DefaultCacheBytes As Long = 512L * 1024L * 1024L

        ''' <summary>两段式全局归约阶段一的每 block 线程数（必须与 tensor_f32.cu 的 TENSOR_F32_REDUCE_BLOCK 一致）</summary>
        Public Const StageBlockSize As Integer = 256

        ''' <summary>逐元素 / stencil 内核的每 block 线程数</summary>
        Public Const ElementBlockSize As Integer = 256

        ''' <summary>两段式全局归约阶段一的 block 数上限（配合 grid-stride 可处理任意长度）</summary>
        Public Shared Property MaxStageBlocks As Integer = 1024

        ''' <summary>
        ''' 低于该元素数时走 CPU 兜底，避免显存拷贝开销倒挂。
        ''' </summary>
        ''' <remarks>
        ''' 单精度只有 4 字节，PCIe 传输成本是双精度的一半，因此这里可以比双精度栈的
        ''' <c>CudaTensor.MinGpuElements</c>（4096）取得更低。默认仍取 4096 保持一致，
        ''' 便于按设备调优时对比两条栈。
        ''' </remarks>
        Public Shared Property MinGpuElementsF As Integer = 4096

        ''' <summary>矩阵乘走到 GPU 的最小规模（m * k * n）</summary>
        Public Shared Property MinGemmElementsF As Integer = 65536

        ''' <summary>
        ''' CFD stencil 原语走到 GPU 的最小格点数。
        ''' </summary>
        ''' <remarks>
        ''' stencil 的算术强度很低（每格点只有 6 次乘加），
        ''' 小网格下上传 / 回传的往返开销会完全吃掉计算收益。
        ''' </remarks>
        Public Shared Property MinCfdCellsF As Integer = 8192

        ''' <summary>最近一次 <see cref="Register"/> 失败的原因</summary>
        Public Shared Property LastError As String

        Private Shared _current As CudaTensorF

        ''' <summary>当前已注册的单精度 GPU 后端实例（未注册时为 Nothing）</summary>
        Public Shared ReadOnly Property Current As CudaTensorF
            Get
                Return _current
            End Get
        End Property

        Private ReadOnly _engine As ILCudaRuntime.CudaEngine
        Private ReadOnly _f32 As DeviceCache(Of Single)
        Private ReadOnly _maskCache As DeviceCache(Of Byte)

        ''' <summary>
        ''' 钉住张量的设备常驻缓冲表。
        ''' </summary>
        ''' <remarks>
        ''' 这是混合精度能真正拿到收益的<b>前提</b>而不是锦上添花：
        ''' 未钉住时每个算子的输出都要做一次完整的 D2H 回读，
        ''' 单层 tensorF 的算子链会退化成"每步来回搬数据"，比原双精度 SIMD 更慢。
        ''' </remarks>
        Private ReadOnly _resident As New DeviceResidentStoreF()

        ''' <summary>
        ''' "全流体"掩膜的替身。
        ''' </summary>
        ''' <remarks>
        ''' CFD 原语里 <c>mask Is Nothing</c> 表示全流体。与其让内核去判空指针，
        ''' 不如主机侧补一份全 1 的掩膜上传（语义等价、内核更简单、也避开
        ''' 托管侧传 Nothing 给内核参数的行为依赖）。按长度缓存，避免每次调用重建。
        ''' </remarks>
        Private ReadOnly _fluidMasks As New Dictionary(Of Integer, Byte())()

        ''' <summary>"每格 6 个流体邻居"的替身（nFluid Is Nothing 时 CPU 侧取常数 6.0）</summary>
        Private ReadOnly _uniformNFluid As New Dictionary(Of Integer, Byte())()

        Public Sub New(engine As ILCudaRuntime.CudaEngine, Optional cacheBytes As Long = 0)
            If engine Is Nothing Then Throw New ArgumentNullException(NameOf(engine))

            _engine = engine

            If cacheBytes <= 0 Then cacheBytes = DefaultCacheBytes

            _f32 = New DeviceCache(Of Single)(cacheBytes)
            _maskCache = New DeviceCache(Of Byte)(std.Max(cacheBytes \ 8, 16L * 1024L * 1024L))
        End Sub

        Public Overrides ReadOnly Property Name As String = "CUDA-F32"

#Region "注册与切换"

        ''' <summary>
        ''' 尝试初始化 CUDA 并把 <see cref="tf.TensorF.computeKernelF"/> 切换为 GPU 后端。
        ''' 设备不可用 / NVRTC 与驱动版本不匹配时返回 False，并且**不改变**当前后端。
        ''' </summary>
        ''' <param name="options">引擎选项（设备号 / NVRTC 路径等），<c>Nothing</c> 表示使用默认值</param>
        ''' <param name="cacheBytes">单精度驻留缓存的容量上限（字节）；0 表示用默认值</param>
        ''' <remarks>
        ''' 如果双精度后端 <see cref="CudaTensor"/> 已经注册，本方法会<b>复用它的引擎</b>，
        ''' 不另建 CUDA 上下文。
        ''' </remarks>
        Public Shared Function Register(Optional options As ILCudaRuntime.EngineOptions = Nothing,
                                        Optional cacheBytes As Long = 0) As Boolean
            ' 必须在 CudaEngine.TryCreate 之前把内核源码注入编译单元
            FloatKernelRegistry.EnsureRegistered()
            ILCudaKernels.KernelSources.Register(GetType(CudaTensorF).Assembly)

            Dim engine As ILCudaRuntime.CudaEngine = Nothing

            ' 优先复用双精度后端已经建好的引擎
            If CudaTensor.Current IsNot Nothing Then
                engine = CudaTensor.Current.Engine
            End If

            If engine Is Nothing Then
                If options Is Nothing Then options = New ILCudaRuntime.EngineOptions()

                engine = ILCudaRuntime.CudaEngine.TryCreate(options)

                If engine Is Nothing Then
                    LastError = options.ErrorMessage
                    Return False
                End If
            End If

            Dim backend As New CudaTensorF(engine, cacheBytes)

            SyncLock tf.TensorF.SyncRoot
                tf.TensorF.computeKernelF = backend
            End SyncLock

            _current = backend
            LastError = Nothing
            Return True
        End Function

        ''' <summary>把单精度计算后端切回默认的 SIMD CPU 实现</summary>
        Public Shared Sub Unregister()
            SyncLock tf.TensorF.SyncRoot
                tf.TensorF.computeKernelF = tfCompute.SIMDTensorF.Default
            End SyncLock

            _current = Nothing
        End Sub

        ''' <summary>
        ''' 由双精度后端的 <see cref="CudaTensor.Register"/> 在已经拿到引擎之后调用：
        ''' 直接装配本后端并切换 <see cref="tf.TensorF.computeKernelF"/>，<b>不</b>再走一遍
        ''' <see cref="Register"/>（那会重复注入内核源码、并可能另建 CUDA 上下文）。
        ''' </summary>
        ''' <remarks>
        ''' 内核源码的注入必须在 <c>CudaEngine.TryCreate</c> 之前完成，因此
        ''' <c>CudaTensor.Register</c> 负责调用 <see cref="FloatKernelRegistry.EnsureRegistered"/>，
        ''' 这里只做"装配 + 切换"。
        ''' 两条栈的程序集是同一个，<c>KernelSources.Register</c> 一次就会把
        ''' <c>tensor_f32.cu</c> 一并注入，无需重复调用。
        ''' </remarks>
        Friend Shared Sub Adopt(engine As ILCudaRuntime.CudaEngine, cacheBytes As Long)
            If engine Is Nothing Then Throw New ArgumentNullException(NameOf(engine))

            ' 注意：即使 FloatKernelRegistry 里有翻译失败的内核也照常装配 ——
            ' 失败的只是逐元素算子，它们会自动回退 CPU；手写内核不受影响。
            Dim backend As New CudaTensorF(engine, cacheBytes)

            SyncLock tf.TensorF.SyncRoot
                tf.TensorF.computeKernelF = backend
            End SyncLock

            _current = backend
        End Sub

#End Region

#Region "内部工具"

        ''' <summary>该张量是否大到值得上 GPU</summary>
        Private Shared Function OnGpu(t As tf.TensorF) As Boolean
            Return t IsNot Nothing AndAlso t.Length >= MinGpuElementsF
        End Function

        ''' <summary>
        ''' 取一个内核；若该符号不在当前 NVRTC 编译单元之中则返回 Nothing，
        ''' 由调用方回退到 CPU 参考实现。
        ''' </summary>
        Private Function TryKernel(kernelName As String) As ILCudaRuntime.CudaKernel
            Try
                Return _engine.GetKernel(kernelName)
            Catch
                Return Nothing
            End Try
        End Function

        ''' <summary>
        ''' 取张量底层数据对应的显存缓冲（单精度，零转换）。
        ''' </summary>
        ''' <remarks>
        ''' 被钉住的张量直接返回常驻缓冲，**跳过版本号驱动的上传**——
        ''' 这正是常驻的全部意义：设备此时是主副本，主机内容可能是陈旧的。
        ''' </remarks>
        Private Function Device(t As tf.TensorF) As ILCudaRuntime.DeviceBuffer(Of Single)
            Dim pinned As ILCudaRuntime.DeviceBuffer(Of Single) = Nothing

            If _resident.TryGet(t.Data, pinned) Then
                Return pinned
            End If

            Return _f32.GetBufferF(_engine, t.Data, t.Version, Function(s) s)
        End Function

        ''' <summary>
        ''' 在算子**要读主机数组**之前调用：把被钉住的张量同步回主机。
        ''' </summary>
        ''' <returns>该张量未被钉住时返回 <c>False</c>（主机内容本来就是新的）</returns>
        ''' <remarks>
        ''' 这是 CudaTensor.SyncIfPinned 在单精度栈的对等形式。
        ''' 漏掉这一步的症状是"结果偶尔落后一步"而不是崩溃 —— 极难排查，
        ''' 因此所有走 CPU 兜底分支的算子都必须在回退之前先过这里。
        ''' </remarks>
        Private Function SyncIfPinned(t As tf.TensorF) As Boolean
            If t Is Nothing Then Return False
            Return _resident.ReadBack(t.Data)
        End Function

        ''' <summary>在回退到 CPU 参考实现之前同步<paramref name="items"/>里所有被钉住的张量。</summary>
        Private Sub SyncAll(ParamArray items As tf.TensorF())
            If items Is Nothing Then Return

            For Each item In items
                Call SyncIfPinned(item)
            Next
        End Sub

        ''' <summary>取掩膜对应的显存缓冲；<paramref name="mask"/> 为 Nothing 时补一份"全流体"掩膜</summary>
        Private Function MaskBuf(mask As Byte(), n As Integer) As ILCudaRuntime.DeviceBuffer(Of Byte)
            Return _maskCache.GetBufferF(_engine, FluidMask(mask, n), 0L, Function(b) b)
        End Function

        ''' <summary>取流体邻居数对应的显存缓冲；<paramref name="nFluid"/> 为 Nothing 时补一份全 6</summary>
        Private Function NFluidBuf(nFluid As Byte(), n As Integer) As ILCudaRuntime.DeviceBuffer(Of Byte)
            Return _maskCache.GetBufferF(_engine, UniformNFluid(nFluid, n), 0L, Function(b) b)
        End Function

        Private Function FluidMask(mask As Byte(), n As Integer) As Byte()
            If mask IsNot Nothing Then
                If mask.Length < n Then
                    Throw New ArgumentException($"掩膜长度 {mask.Length} 小于场长度 {n}")
                End If

                Return mask
            End If

            SyncLock _fluidMasks
                Dim buf As Byte() = Nothing

                If Not _fluidMasks.TryGetValue(n, buf) Then
                    buf = New Byte(n - 1) {}
                    Array.Fill(buf, CByte(1))
                    _fluidMasks(n) = buf
                End If

                Return buf
            End SyncLock
        End Function

        Private Function UniformNFluid(nFluid As Byte(), n As Integer) As Byte()
            If nFluid IsNot Nothing Then
                If nFluid.Length < n Then
                    Throw New ArgumentException($"流体邻居数数组长度 {nFluid.Length} 小于场长度 {n}")
                End If

                Return nFluid
            End If

            SyncLock _uniformNFluid
                Dim buf As Byte() = Nothing

                If Not _uniformNFluid.TryGetValue(n, buf) Then
                    buf = New Byte(n - 1) {}
                    Array.Fill(buf, CByte(6))
                    _uniformNFluid(n) = buf
                End If

                Return buf
            End SyncLock
        End Function

        ''' <summary>逐元素二元（纯标量内核：两个输入数组）</summary>
        Private Function EwBinary(a As tf.TensorF, b As tf.TensorF, kernelName As String) As tf.TensorF
            If Not OnGpu(a) OrElse Not FloatKernelRegistry.Available(kernelName) Then
                Return Nothing
            End If

            RequireSameShape(a, b, "逐元素二元运算")

            Dim dx = Device(a)
            Dim dy = Device(b)

            Using dOut As New ILCudaRuntime.DeviceBuffer(Of Single)(a.Length)
                _engine.GetKernel(kernelName).Launch(
                    ILCudaRuntime.LaunchPlanner.For1D(a.Length, ElementBlockSize),
                    dx, dy, dOut, a.Length)

                Return Wrap(dOut.Read(), a.Shape)
            End Using
        End Function

        ''' <summary>逐元素一元（可带 1 / 2 个运行时标量参数）</summary>
        Private Function EwUnary(t As tf.TensorF, kernelName As String,
                                ParamArray scalars As Single()) As tf.TensorF
            If Not OnGpu(t) OrElse Not FloatKernelRegistry.Available(kernelName) Then
                Return Nothing
            End If

            Dim dx = Device(t)

            Using dOut As New ILCudaRuntime.DeviceBuffer(Of Single)(t.Length)
                Dim args As New List(Of Object)
                args.Add(dx)

                For Each s As Single In scalars
                    args.Add(s)
                Next

                args.Add(dOut)
                args.Add(t.Length)

                _engine.GetKernel(kernelName).Launch(
                    ILCudaRuntime.LaunchPlanner.For1D(t.Length, ElementBlockSize), args.ToArray())

                Return Wrap(dOut.Read(), t.Shape)
            End Using
        End Function

        ''' <summary>
        ''' 两段式全局归约（求和）：阶段一多 block 产出部分和，阶段二汇总。
        ''' 两段都在 GPU 上完成，不需要主机端同步。累加全程 double。
        ''' </summary>
        Private Function ReduceSum(t As tf.TensorF) As Double
            Dim n = t.Length
            Dim blocks = std.Min(ILCudaRuntime.LaunchPlanner.CeilDiv(n, StageBlockSize), MaxStageBlocks)
            If blocks <= 0 Then blocks = 1

            Dim dx = Device(t)

            Using partials As New ILCudaRuntime.DeviceBuffer(Of Double)(blocks),
                  out As New ILCudaRuntime.DeviceBuffer(Of Double)(1)

                Dim stage As New ILCudaRuntime.LaunchConfig(blocks, 1, StageBlockSize, 1, 0)
                _engine.GetKernel(TensorKernelNames.PartialSumF32).Launch(stage, dx, partials, n)

                Dim finalCfg As New ILCudaRuntime.LaunchConfig(1, 1, StageBlockSize, 1, 0)
                _engine.GetKernel(TensorKernelNames.FinalSumF32).Launch(finalCfg, partials, out, blocks)

                Return out.Read()(0)
            End Using
        End Function

        ''' <summary>两段式全局归约（极值）：与 <see cref="ReduceSum"/> 同构，但全程 float</summary>
        Private Function ReduceExtreme(t As tf.TensorF, partialKernel As String, finalKernel As String) As Single
            Dim n = t.Length
            Dim blocks = std.Min(ILCudaRuntime.LaunchPlanner.CeilDiv(n, StageBlockSize), MaxStageBlocks)
            If blocks <= 0 Then blocks = 1

            Dim dx = Device(t)

            Using partials As New ILCudaRuntime.DeviceBuffer(Of Single)(blocks),
                  out As New ILCudaRuntime.DeviceBuffer(Of Single)(1)

                Dim stage As New ILCudaRuntime.LaunchConfig(blocks, 1, StageBlockSize, 1, 0)
                _engine.GetKernel(partialKernel).Launch(stage, dx, partials, n)

                Dim finalCfg As New ILCudaRuntime.LaunchConfig(1, 1, StageBlockSize, 1, 0)
                _engine.GetKernel(finalKernel).Launch(finalCfg, partials, out, blocks)

                Return out.Read()(0)
            End Using
        End Function

        ''' <summary>三维场是否合法（Rank=3 且规模够大）；不合法时返回 False 由调用方回退 CPU</summary>
        Private Shared Function TryGrid(t As tf.TensorF, minCells As Integer,
                                        ByRef nx As Integer, ByRef ny As Integer, ByRef nz As Integer,
                                        ByRef plane As Integer) As Boolean
            nx = 0 : ny = 0 : nz = 0 : plane = 0

            If t Is Nothing OrElse t.Rank <> 3 Then Return False

            Dim shp = t.Shape

            nx = shp(0) : ny = shp(1) : nz = shp(2)

            If nx < 3 OrElse ny < 3 OrElse nz < 3 Then Return False
            If t.Length < minCells Then Return False

            plane = ny * nz
            Return True
        End Function

#End Region

#Region "逐元素 - 二元"

        Public Overrides Function Add(a As tf.TensorF, b As tf.TensorF) As tf.TensorF
            Dim r = EwBinary(a, b, FloatKernelRegistry.EwAdd)
            Return If(r, MyBase.Add(a, b))
        End Function

        Public Overrides Function Subtract(a As tf.TensorF, b As tf.TensorF) As tf.TensorF
            Dim r = EwBinary(a, b, FloatKernelRegistry.EwSub)
            Return If(r, MyBase.Subtract(a, b))
        End Function

        Public Overrides Function Multiply(a As tf.TensorF, b As tf.TensorF) As tf.TensorF
            Dim r = EwBinary(a, b, FloatKernelRegistry.EwMul)
            Return If(r, MyBase.Multiply(a, b))
        End Function

        Public Overrides Function Divide(a As tf.TensorF, b As tf.TensorF) As tf.TensorF
            Dim r = EwBinary(a, b, FloatKernelRegistry.EwDiv)
            Return If(r, MyBase.Divide(a, b))
        End Function

        Public Overrides Function Maximum(a As tf.TensorF, b As tf.TensorF) As tf.TensorF
            Dim r = EwBinary(a, b, FloatKernelRegistry.EwMax)
            Return If(r, MyBase.Maximum(a, b))
        End Function

        Public Overrides Function Minimum(a As tf.TensorF, b As tf.TensorF) As tf.TensorF
            Dim r = EwBinary(a, b, FloatKernelRegistry.EwMin)
            Return If(r, MyBase.Minimum(a, b))
        End Function

#End Region

#Region "逐元素 - 一元"

        Public Overrides Function Exp(t As tf.TensorF) As tf.TensorF
            Dim r = EwUnary(t, FloatKernelRegistry.EwExp)
            Return If(r, MyBase.Exp(t))
        End Function

        Public Overrides Function Log(t As tf.TensorF) As tf.TensorF
            Dim r = EwUnary(t, FloatKernelRegistry.EwLog)
            Return If(r, MyBase.Log(t))
        End Function

        Public Overrides Function Sqrt(t As tf.TensorF) As tf.TensorF
            Dim r = EwUnary(t, FloatKernelRegistry.EwSqrt)
            Return If(r, MyBase.Sqrt(t))
        End Function

        Public Overrides Function Square(t As tf.TensorF) As tf.TensorF
            Dim r = EwUnary(t, FloatKernelRegistry.EwSquare)
            Return If(r, MyBase.Square(t))
        End Function

        Public Overrides Function Abs(t As tf.TensorF) As tf.TensorF
            Dim r = EwUnary(t, FloatKernelRegistry.EwAbs)
            Return If(r, MyBase.Abs(t))
        End Function

        Public Overrides Function Negate(t As tf.TensorF) As tf.TensorF
            Dim r = EwUnary(t, FloatKernelRegistry.EwNeg)
            Return If(r, MyBase.Negate(t))
        End Function

        Public Overrides Function Reciprocal(t As tf.TensorF) As tf.TensorF
            Dim r = EwUnary(t, FloatKernelRegistry.EwRecip)
            Return If(r, MyBase.Reciprocal(t))
        End Function

        Public Overrides Function Tanh(t As tf.TensorF) As tf.TensorF
            Dim r = EwUnary(t, FloatKernelRegistry.EwTanh)
            Return If(r, MyBase.Tanh(t))
        End Function

        Public Overrides Function Sigmoid(t As tf.TensorF) As tf.TensorF
            Dim r = EwUnary(t, FloatKernelRegistry.EwSigmoid)
            Return If(r, MyBase.Sigmoid(t))
        End Function

        Public Overrides Function Relu(t As tf.TensorF) As tf.TensorF
            Dim r = EwUnary(t, FloatKernelRegistry.EwRelu)
            Return If(r, MyBase.Relu(t))
        End Function

#End Region

#Region "标量运算"

        Public Overrides Function AddScalar(t As tf.TensorF, scalar As Single) As tf.TensorF
            Dim r = EwUnary(t, FloatKernelRegistry.EwAddScalar, scalar)
            Return If(r, MyBase.AddScalar(t, scalar))
        End Function

        Public Overrides Function MultiplyScalar(t As tf.TensorF, scalar As Single) As tf.TensorF
            Dim r = EwUnary(t, FloatKernelRegistry.EwScale, scalar)
            Return If(r, MyBase.MultiplyScalar(t, scalar))
        End Function

        Public Overrides Function DivideScalar(t As tf.TensorF, scalar As Single) As tf.TensorF
            Dim r = EwUnary(t, FloatKernelRegistry.EwDivScalar, scalar)
            Return If(r, MyBase.DivideScalar(t, scalar))
        End Function

        Public Overrides Function Clip(t As tf.TensorF, low As Single, high As Single) As tf.TensorF
            Dim r = EwUnary(t, FloatKernelRegistry.EwClip, low, high)
            Return If(r, MyBase.Clip(t, low, high))
        End Function

#End Region

#Region "矩阵运算"

        ''' <summary>
        ''' 二维矩阵乘 <c>C[m, n] = A[m, k] · B[k, n]</c>。
        ''' 直接复用 ILCuda 自带的 <c>gemmKernel</c>（Kernels\blas.cu，本来就是单精度），
        ''' 因此单精度栈不需要自己的 GEMM 内核 —— 这也是它相对双精度栈最占便宜的地方
        ''' （双精度栈还得先降精度再算）。
        ''' </summary>
        Public Overrides Function MatMul(a As tf.TensorF, b As tf.TensorF) As tf.TensorF
            If a Is Nothing OrElse b Is Nothing Then Return MyBase.MatMul(a, b)
            If a.Rank <> 2 OrElse b.Rank <> 2 Then Return MyBase.MatMul(a, b)
            If a.Shape(1) <> b.Shape(0) Then Return MyBase.MatMul(a, b)

            Dim m = a.Shape(0), k = a.Shape(1), n = b.Shape(1)
            Dim elements As Long = CLng(m) * CLng(n)

            If elements > Integer.MaxValue Then Return MyBase.MatMul(a, b)
            If CLng(m) * CLng(k) * CLng(n) < CLng(MinGemmElementsF) Then Return MyBase.MatMul(a, b)

            Dim kernel = TryKernel(ILCudaRuntime.KernelNames.Gemm)
            If kernel Is Nothing Then Return MyBase.MatMul(a, b)

            Dim da = Device(a)
            Dim db = Device(b)

            Using dC As New ILCudaRuntime.DeviceBuffer(Of Single)(CInt(elements))
                kernel.Launch(
                    ILCudaRuntime.LaunchPlanner.For2D(m, n, 16, 16),
                    da, db, dC, m, n, k)

                Return Wrap(dC.Read(), New Integer() {m, n})
            End Using
        End Function

#End Region

#Region "归约运算"

        ''' <summary>
        ''' 整体求和。设备侧用<b>双精度</b>累加，与契约要求一致
        ''' （"累加使用双精度，避免大数组单精度累加的舍入误差"）。
        ''' </summary>
        Public Overrides Function SumAll(t As tf.TensorF) As Double
            If Not OnGpu(t) Then Return MyBase.SumAll(t)

            Dim kernel = TryKernel(TensorKernelNames.PartialSumF32)
            If kernel Is Nothing Then Return MyBase.SumAll(t)

            Return ReduceSum(t)
        End Function

        Public Overrides Function MeanAll(t As tf.TensorF) As Double
            If Not OnGpu(t) Then Return MyBase.MeanAll(t)

            Dim kernel = TryKernel(TensorKernelNames.PartialSumF32)
            If kernel Is Nothing Then Return MyBase.MeanAll(t)

            Return ReduceSum(t) / t.Length
        End Function

        Public Overrides Function MinAll(t As tf.TensorF) As Single
            If Not OnGpu(t) Then Return MyBase.MinAll(t)

            Dim kernel = TryKernel(TensorKernelNames.PartialMinF32)
            If kernel Is Nothing Then Return MyBase.MinAll(t)

            Return ReduceExtreme(t, TensorKernelNames.PartialMinF32, TensorKernelNames.FinalMinF32)
        End Function

        Public Overrides Function MaxAll(t As tf.TensorF) As Single
            If Not OnGpu(t) Then Return MyBase.MaxAll(t)

            Dim kernel = TryKernel(TensorKernelNames.PartialMaxF32)
            If kernel Is Nothing Then Return MyBase.MaxAll(t)

            Return ReduceExtreme(t, TensorKernelNames.PartialMaxF32, TensorKernelNames.FinalMaxF32)
        End Function

#End Region

#Region "CFD 扩展原语"

        ''' <summary>
        ''' 七点拉普拉斯 stencil：<c>out = Σ(流体邻居) − nFluid·中心</c>。
        ''' </summary>
        ''' <remarks>
        ''' 语义与 <c>TensorComputeFBase.Laplacian7</c> 逐点一致：
        ''' 只算内部（留一层 ghost 壳），边界与固体单元输出恒 0；
        ''' 邻居求和按 −i, +i, −j, +j, −k, +k 的顺序用 double 累加，末尾只舍入一次。
        ''' </remarks>
        Public Overrides Function Laplacian7(src As tf.TensorF, mask As Byte(),
                                             nFluid As Byte()) As tf.TensorF
            Dim nx As Integer, ny As Integer, nz As Integer, plane As Integer

            If Not TryGrid(src, MinCfdCellsF, nx, ny, nz, plane) Then
                Return MyBase.Laplacian7(src, mask, nFluid)
            End If

            Dim kernel = TryKernel(TensorKernelNames.Laplacian7F32)
            If kernel Is Nothing Then Return MyBase.Laplacian7(src, mask, nFluid)

            Dim n = src.Length
            Dim dx = Device(src)
            Dim dm = MaskBuf(mask, n)
            Dim dn = NFluidBuf(nFluid, n)

            Using dOut As New ILCudaRuntime.DeviceBuffer(Of Single)(n)
                kernel.Launch(
                    ILCudaRuntime.LaunchPlanner.For1D(n, ElementBlockSize),
                    dx, dm, dn, dOut, nx, ny, nz, plane, n)

                Return Wrap(dOut.Read(), src.Shape)
            End Using
        End Function

        ''' <summary>
        ''' 带掩膜的七点 Jacobi 迭代：<c>x_new = (alpha·Σ_fluid x_old + rhs) / denom</c>。
        ''' </summary>
        ''' <remarks>
        ''' 这是本后端<b>收益最显著</b>的算子：CPU 侧 <c>iterations</c> 次扫描全部在
        ''' 主机完成，这里 <c>iterations</c> 次迭代<b>全部留在设备端</b>（两块缓冲 ping-pong），
        ''' 主机往返次数恒为"上传 1 次 + 回传 1 次"，与迭代次数无关。
        ''' <para>
        ''' ping-pong 是正确性要求而不是优化：CPU 参考实现每轮开头都
        ''' <c>Array.Copy(cur, prev)</c>，即真 Jacobi；就地写会变成 Gauss-Seidel，结果不同。
        ''' </para>
        ''' <para>
        ''' 边界（非内部）单元<b>不被写</b>，因此带 <c>initial</c> 调用时边界会保留
        ''' <c>initial</c> 的值 —— 这与 <see cref="Laplacian7"/> 的"边界恒 0"不同，
        ''' 是刻意照抄 CPU 参考实现的行为。两块缓冲在开始前都被填成 <c>initial</c>（或全 0），
        ''' 这样无论迭代多少次、ping-pong 怎么换手，边界值都稳定不变。
        ''' </para>
        ''' </remarks>
        Public Overrides Function JacobiStencil7(rhs As tf.TensorF, mask As Byte(), nFluid As Byte(),
                                                 alpha As Single, beta As Single,
                                                 iterations As Integer,
                                                 Optional initial As tf.TensorF = Nothing) As tf.TensorF
            Dim nx As Integer, ny As Integer, nz As Integer, plane As Integer

            If Not TryGrid(rhs, MinCfdCellsF, nx, ny, nz, plane) Then
                Return MyBase.JacobiStencil7(rhs, mask, nFluid, alpha, beta, iterations, initial)
            End If

            Dim kernel = TryKernel(TensorKernelNames.Jacobi7F32)
            If kernel Is Nothing Then
                Return MyBase.JacobiStencil7(rhs, mask, nFluid, alpha, beta, iterations, initial)
            End If

            Dim n = rhs.Length
            Dim db = Device(rhs)
            Dim dm = MaskBuf(mask, n)
            Dim dn = NFluidBuf(nFluid, n)

            ' 初值：initial 的副本或全 0。两块缓冲填同样的内容，
            ' 因为边界单元永远不会被内核写入，必须靠初值撑住。
            Dim seed(n - 1) As Single
            If initial IsNot Nothing Then Array.Copy(initial.Data, seed, n)

            Dim bufA As ILCudaRuntime.DeviceBuffer(Of Single) = Nothing
            Dim bufB As ILCudaRuntime.DeviceBuffer(Of Single) = Nothing

            Try
                bufA = New ILCudaRuntime.DeviceBuffer(Of Single)(n)
                bufB = New ILCudaRuntime.DeviceBuffer(Of Single)(n)

                bufA.Write(seed)
                bufB.Write(seed)

                Dim launch = ILCudaRuntime.LaunchPlanner.For1D(n, ElementBlockSize)

                For iter = 1 To iterations
                    kernel.Launch(launch, bufA, bufB, db, dm, dn,
                                  alpha, beta, nx, ny, nz, plane, n)

                    Dim swap = bufA
                    bufA = bufB
                    bufB = swap
                Next

                ' 迭代 0 次时 bufA 就是初值本身，与 CPU 参考实现一致
                Return Wrap(bufA.Read(), rhs.Shape)
            Finally
                If bufA IsNot Nothing Then bufA.Dispose()
                If bufB IsNot Nothing Then bufB.Dispose()
            End Try
        End Function

        ''' <summary>
        ''' 半拉格朗日平流：沿速度场反向追踪 <c>dt</c>，在回溯位置对源场做三线性插值。
        ''' </summary>
        ''' <remarks>
        ''' 纯 gather（只读源场、只写自己的格点），每个格点完全独立，1:1 映射到线程。
        ''' 网格间距恒为 1（cell 单位）：CPU 参考实现里没有任何 h / spacing，
        ''' 因此 <c>dt·u</c> 的单位就是格数，内核里绝不能引入 1/h。
        ''' <para>
        ''' 位移 <c>i − dt·u</c> 必须先用 float32 算再拓宽到 double 做钳制，
        ''' 否则会与 CPU 参考实现差 1 ulp（VB 的 Single×Single 留在 Single 域）。
        ''' </para>
        ''' </remarks>
        Public Overrides Function AdvectTrilinear(src As tf.TensorF,
                                                  u As tf.TensorF, v As tf.TensorF, w As tf.TensorF,
                                                  dt As Single, mask As Byte()) As tf.TensorF
            Dim nx As Integer, ny As Integer, nz As Integer, plane As Integer

            If Not TryGrid(src, MinCfdCellsF, nx, ny, nz, plane) Then
                Return MyBase.AdvectTrilinear(src, u, v, w, dt, mask)
            End If

            ' 速度场必须同形：CPU 参考实现不校验而直接按同一 idx 取值，
            ' 上 GPU 后形状不符会变成越界读，因此这里显式拦下并回退 CPU。
            If u Is Nothing OrElse v Is Nothing OrElse w Is Nothing Then
                Return MyBase.AdvectTrilinear(src, u, v, w, dt, mask)
            End If
            If u.Length <> src.Length OrElse v.Length <> src.Length OrElse w.Length <> src.Length Then
                Return MyBase.AdvectTrilinear(src, u, v, w, dt, mask)
            End If

            Dim kernel = TryKernel(TensorKernelNames.AdvectTrilinearF32)
            If kernel Is Nothing Then Return MyBase.AdvectTrilinear(src, u, v, w, dt, mask)

            Dim n = src.Length
            Dim dx = Device(src)
            Dim du = Device(u)
            Dim dv = Device(v)
            Dim dw = Device(w)
            Dim dm = MaskBuf(mask, n)

            Using dOut As New ILCudaRuntime.DeviceBuffer(Of Single)(n)
                kernel.Launch(
                    ILCudaRuntime.LaunchPlanner.For1D(n, ElementBlockSize),
                    dx, du, dv, dw, dm, dt, dOut, nx, ny, nz, plane, n)

                Return Wrap(dOut.Read(), src.Shape)
            End Using
        End Function

        ''' <summary>
        ''' 按掩膜把所有给定场中的固体单元置零（无滑移壁）。
        ''' </summary>
        ''' <remarks>
        ''' 与另外三个原语不同，本算子是<b>就地</b>语义：CPU 参考实现直接改写调用方
        ''' 张量的底层数组。GPU 路径因此必须"上传 → 内核 → 回写主机数组"，
        ''' 回写之后调用 <see cref="tf.TensorF.MarkHostModified"/> 让设备缓存失效，
        ''' 否则下一次算子会命中这份已被改写的旧缓冲。
        ''' <para>
        ''' 逐场判断规模：小场直接走 CPU 路径（往返开销倒挂），大场才上 GPU。
        ''' </para>
        ''' </remarks>
        Public Overrides Sub ApplyMask(mask As Byte(), ParamArray fields As tf.TensorF())
            If mask Is Nothing OrElse fields Is Nothing Then Return

            Dim kernel = TryKernel(TensorKernelNames.ApplyMaskF32)
            If kernel Is Nothing Then
                MyBase.ApplyMask(mask, fields)
                Return
            End If

            For Each f In fields
                If f Is Nothing Then Continue For

                Dim n = f.Length
                If n = 0 Then Continue For

                If n < MinGpuElementsF Then
                    ' 规模太小：往返开销倒挂，逐场退回 CPU
                    MyBase.ApplyMask(mask, f)
                    Continue For
                End If

                Dim dm = _maskCache.GetBufferF(_engine, mask, 0L, Function(b) b)

                Using dField As New ILCudaRuntime.DeviceBuffer(Of Single)(n)
                    dField.Write(f.Data)

                    kernel.Launch(
                        ILCudaRuntime.LaunchPlanner.For1D(n, ElementBlockSize),
                        dm, dField, n)

                    Dim result = dField.Read()
                    Array.Copy(result, f.Data, n)
                End Using

                ' 主机数组已被就地改写，设备缓存必须失效
                f.MarkHostModified()
            Next
        End Sub

#End Region

#Region "诊断"

        ''' <summary>
        ''' 指定名称的内核当前是否可用。
        ''' </summary>
        ''' <remarks>
        ''' 内核不可用（NVRTC 编译失败 / 驱动不匹配）时相关算子会<b>静默回退 CPU</b>。
        ''' 调用方在"汇报是否真的用上了 GPU"之前应当先问一下这里，
        ''' 否则很容易把"算得慢"误报成"GPU 有效果"。
        ''' </remarks>
        Public Function IsKernelAvailable(kernelName As String) As Boolean
            Return TryKernel(kernelName) IsNot Nothing
        End Function

        ''' <summary>
        ''' 关键内核的可用性快照，用于诊断"单精度栈到底有没有走上 GPU"。
        ''' </summary>
        Public Function DescribeKernels() As String
            Dim lines As New List(Of String)

            lines.Add($"后端: {Name}")
            lines.Add($"引擎: {If(_engine Is Nothing, "<无>", "已就绪")}")
            lines.Add($"阈值: 逐元素 {MinGpuElementsF} / GEMM {MinGemmElementsF} / CFD {MinCfdCellsF}")

            ' 循环变量不能叫 name —— VB 标识符大小写不敏感，会撞 Name 属性
            For Each label In KernelCatalog
                lines.Add($"  {label.Key,-22} {If(IsKernelAvailable(label.Value), "可用", "缺失")}")
            Next

            Dim fails = FloatKernelRegistry.Failures
            If fails.Count > 0 Then
                lines.Add($"IL2Cuda 翻译失败 {fails.Count} 个:")

                For Each kv In fails
                    lines.Add($"  {kv.Key}: {kv.Value}")
                Next
            End If

            Return String.Join(Environment.NewLine, lines)
        End Function

        ''' <summary>诊断用：内核名 -> 显示标签</summary>
        Private Shared ReadOnly Property KernelCatalog As Dictionary(Of String, String)
            Get
                Return New Dictionary(Of String, String) From {
                    {"逐元素 相加", FloatKernelRegistry.EwAdd},
                    {"归约 求和(阶段一)", TensorKernelNames.PartialSumF32},
                    {"归约 求和(阶段二)", TensorKernelNames.FinalSumF32},
                    {"归约 最大值", TensorKernelNames.PartialMaxF32},
                    {"归约 最小值", TensorKernelNames.PartialMinF32},
                    {"矩阵乘(ILCuda)", ILCudaRuntime.KernelNames.Gemm},
                    {"CFD 拉普拉斯7", TensorKernelNames.Laplacian7F32},
                    {"CFD Jacobi7", TensorKernelNames.Jacobi7F32},
                    {"CFD 平流", TensorKernelNames.AdvectTrilinearF32},
                    {"CFD 掩膜", TensorKernelNames.ApplyMaskF32}
                }
            End Get
        End Property

#End Region

#Region "释放"

        ''' <summary>释放单精度栈占用的全部显存缓冲</summary>
        Public Sub Dispose() Implements IDisposable.Dispose
            _resident.Dispose()
            _f32.Dispose()
            _maskCache.Dispose()
        End Sub

#End Region

    End Class

End Namespace
