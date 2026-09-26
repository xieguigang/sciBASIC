#Region "Microsoft.VisualBasic::b1fb646fdf8d8ec49d3ba41d742a4727, cuda\ILCudaTensor\GPUTensor\CudaTensor.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 1532
    '    Code Lines: 883 (57.64%)
    ' Comment Lines: 329 (21.48%)
    '    - Xml Docs: 76.90%
    ' 
    '   Blank Lines: 320 (20.89%)
    '     File Size: 70.08 KB


    '     Class CudaTensor
    ' 
    '         Properties: Current, Engine, KernelFailures, LastError, MaxStageBlocks
    '                     MinGemmElements, MinGpuElements, MinSparseNnz, Name, PinnedDeviceBytes
    '                     Resident, SupportsDeviceResidency, UseFp32Gemm, UseResidentStore
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: Abs, AdaptiveCacheBytes, Add, AddScalar, ArgMax
    '                   ArgMin, Clip, Conv2D, Conv2DBackwardBias, Conv2DBackwardFilter
    '                   Conv2DBackwardInput, ConvOutSize, Cos, DescribeDevice, DescribeKernels
    '                   Device, DeviceF32, Divide, DivideScalar, ElementCount
    '                   Elu, EwBinary, EwUnary, Exp, Gelu
    '                   Heaviside, IsDevicePinned, IsLastAxis, LeakyRelu, Log
    '                   LogSoftmax, MaskedCrossEntropy, MaskedCrossEntropyFp32Core, MatMul, MatMulFp32
    '                   Max, MaxPool2D, MaxPool2DBackward, Mean, MeanAll
    '                   Min, Multiply, MultiplyScalar, Negate, OnGpu
    '                   PinDevice, Pow, Reciprocal, ReducedShape, ReduceGlobal
    '                   Register, Relu, RowReduce, RowSoftmax, Sigmoid
    '                   Sin, Softmax, SpMM, Sqrt, Square
    '                   Subtract, Sum, SumAll, Swish, SyncFromDevice
    '                   Tanh, Transpose, TryAccumulateFp32, TryAdamWStep, TryKernel
    '                   UnpinDevice, ZeroBias
    ' 
    '         Sub: Dispose, EnsureDeviceCount, LaunchRow, SyncIfPinned, Unregister
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' ---------------------------------------------------------------------------
' CudaTensor —— 基于 ILCuda 的 GPU 计算后端（主机 Double / 设备可单精度）
'
' 它继承 TensorFlow 提供的标量兜底实现 TensorComputeBase，
' 因此只需要重写“GPU 有对应内核”的算子；其余算子（比较、乘积归约、Apply 等）
' 自动继承 CPU 实现，保证任何后端切换都不会让某个算子失效。
'
' 精度策略：
'   * **矩阵乘默认走 FP32**（Kernels\blas.cu 的 gemmKernel）。理由见 UseFp32Gemm 的注释：
'     消费级显卡的 FP64 吞吐只有 FP32 的 1/64，而 GEMM 是语言模型里占绝对主导的浮点运算。
'     主机侧数据始终是 Double，只在显存里降精度计算，结果升精度回写。
'   * 其余算子保持 **double**，沿用既有内核：
'       - 逐元素运算 / 转置          -> IL2Cuda 生成内核（DoubleKernels.vb）
'       - 末轴 softmax / 归约 / arg  -> 手写内核（tensor.cu）
'       - 全局归约                   -> 复用末轴归约内核（整张量当作一行）
'
' 显存管理有两层，职责不同：
'   * DeviceCache（LRU）：算子输入的暂存区，以 (主机数组引用, 版本号) 为键，
'     版本变化即失效重传 —— 这对推理正好，但对训练是灾难（权重每步都变）。
'   * DeviceResidentStore：长期张量（权重 / 梯度 / 优化器状态）的落脚点，
'     钉住后不随版本号失效，训练时只上传一次。由 UseResidentStore 控制启用。
'
' 所有显存缓冲都由这两者显式管理，避免 ILCuda 无终结器导致的显存泄漏。
'
' 用法：
'     If CudaTensor.Register() Then
'         ' 之后所有 Tensor / Math / nn 的运算自动走 GPU
'     End If
' ---------------------------------------------------------------------------

Imports ILCudaRuntime = Microsoft.VisualBasic.Computing.ILCuda.Runtime
Imports ILCudaKernels = Microsoft.VisualBasic.Computing.ILCuda.Kernels
Imports tf = Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports tfCompute = Microsoft.VisualBasic.MachineLearning.TensorFlow.Compute

Namespace GPUTensor

    ''' <summary>
    ''' <see cref="tfCompute.ITensorCompute"/> 的 CUDA GPU 实现（全 double 精度）。
    ''' </summary>
    Public Class CudaTensor
        Inherits tfCompute.TensorComputeBase
        Implements IDisposable

        ''' <summary>默认的显存驻留缓存上限（1 GiB）</summary>
        Public Const DefaultCacheBytes As Long = 1024L * 1024L * 1024L

        ''' <summary>末轴行内核使用的每 block 线程数（必须与 tensor.cu 的 TENSOR_BLOCK 一致）</summary>
        Public Const RowBlockSize As Integer = 256

        ''' <summary>两段式全局归约阶段一的每 block 线程数</summary>
        Public Const StageBlockSize As Integer = 256

        ''' <summary>两段式全局归约阶段一的 block 数上限（配合 grid-stride 可处理任意长度）</summary>
        Public Shared Property MaxStageBlocks As Integer = 1024

        ''' <summary>低于该元素数时走 CPU 兜底，避免显存拷贝开销倒挂</summary>
        Public Shared Property MinGpuElements As Integer = 4096

        ''' <summary>矩阵乘走到 GPU 的最小规模（m * k * n）</summary>
        Public Shared Property MinGemmElements As Integer = 65536

        ''' <summary>
        ''' 稀疏 SpMM 走到 GPU 的最小非零元素数（nnz）。
        ''' 连接规模过小时显存拷贝/启动开销会倒挂，回退 CPU 反而更快。
        ''' </summary>
        Public Shared Property MinSparseNnz As Integer = 65536

        ''' <summary>最近一次 <see cref="Register"/> 失败的原因</summary>
        Public Shared Property LastError As String

        Private Shared _current As CudaTensor

        ''' <summary>当前已注册的 GPU 后端实例（未注册时为 Nothing）</summary>
        Public Shared ReadOnly Property Current As CudaTensor
            Get
                Return _current
            End Get
        End Property

        Private ReadOnly _engine As ILCudaRuntime.CudaEngine
        Private ReadOnly _cache As DeviceCache(Of Double)
        Private ReadOnly _fp32 As DeviceCache(Of Single)
        Private ReadOnly _csrCache As SparseCsrCache

        ''' <summary>
        ''' 是否让矩阵乘改走单精度（FP32）内核。
        ''' </summary>
        ''' <remarks>
        ''' 消费级显卡（例如 8GB 的 RTX 30 / 40 系）的 FP64 吞吐只有 FP32 的 <b>1/64</b>，
        ''' 而矩阵乘占语言模型全部浮点运算的 95% 以上，因此把 GEMM 放到 FP32 上
        ''' 能拿到接近一个数量级的实际加速（主机侧数据仍是 Double，只在显存里降精度）。
        ''' 设为 <c>False</c> 会退回 <c>tensorGemmDoubleKernel</c> 的全双精度路径，
        ''' 用于"FP32 与 FP64 数值对照"这类诊断场景。
        ''' </remarks>
        Public Property UseFp32Gemm As Boolean = True

        ''' <summary>
        ''' 是否让矩阵乘使用常驻显存缓冲（跳过 LRU 缓存的版本失效重传）。
        ''' </summary>
        ''' <remarks>
        ''' 详见 <see cref="DeviceResidentStore"/>。默认关闭，开启后
        ''' <see cref="DeviceF32"/> 会优先返回被钉住的常驻缓冲。
        ''' </remarks>
        Public Property UseResidentStore As Boolean = False

        ''' <summary>设备常驻缓冲注册表（权重 / 梯度 / 优化器状态的长期落脚点）。</summary>
        Private ReadOnly _resident As New DeviceResidentStore()

        ''' <summary>
        ''' 是否让 <see cref="PinDevice64"/> 生效，即是否允许把张量钉成<b>双精度</b>常驻缓冲。
        ''' </summary>
        ''' <remarks>
        ''' 默认 <c>False</c>：既有调用方（稠密训练）只使用单精度常驻通道，
        ''' 不因为本特性多占显存。脉冲网络需要"零往返 + 双精度保真"时应显式打开，
        ''' 见 <see cref="PinDevice64"/> 的说明。
        ''' </remarks>
        Public Property UseFp64Residency As Boolean = False

        ''' <summary>双精度设备常驻缓冲注册表（脉冲网络的递归状态）。</summary>
        Private ReadOnly _resident64 As New DeviceResidentStore64()

        ''' <summary>
        ''' 融合 LIF 单步的跨时间步工作区（键 = 膜电位张量的主机数组引用）：
        ''' 复用突触输入缓冲 <c>I</c>，避免每步 <c>cuMemAlloc</c> 一次 1.1 MB。
        ''' </summary>
        Private ReadOnly _lifWorkspaces As New Dictionary(Of Double(), LifWorkspace)()
        Private ReadOnly _lifSync As New Object()

        ''' <summary>
        ''' 融合 LIF 单步的工作区：一片跨步复用的突触输入缓冲。
        ''' </summary>
        ''' <remarks>
        ''' 稀疏内核是"按行散射 + atomicAdd"，输出必须预先清零；把缓冲留在本对象里
        ''' 而不是每步新建，可以省掉一次分配 / 释放（在十万神经元规模下即 1.1 MB）。
        ''' </remarks>
        Private NotInheritable Class LifWorkspace
            Implements IDisposable

            ''' <summary>突触输入 <c>I[batch, columns]</c>（内核用 atomicAdd 累加，启动前必须清零）</summary>
            Public ReadOnly Input As ILCudaRuntime.DeviceBuffer(Of Double)

            ''' <summary>缓冲的元素数（形状变化时需要重建）</summary>
            Public ReadOnly Elements As Integer

            Public Sub New(elements As Integer)
                Input = New ILCudaRuntime.DeviceBuffer(Of Double)(elements)
                Me.Elements = elements
            End Sub

            Public Sub Dispose() Implements IDisposable.Dispose
                Input.Dispose()
            End Sub
        End Class

        Public Sub New(engine As ILCudaRuntime.CudaEngine, Optional cacheBytes As Long = 0)
            If engine Is Nothing Then Throw New ArgumentNullException(NameOf(engine))

            _engine = engine

            ' <= 0 表示按设备可用显存自适应
            If cacheBytes <= 0 Then cacheBytes = AdaptiveCacheBytes()

            _cache = New DeviceCache(Of Double)(cacheBytes)
            _fp32 = New DeviceCache(Of Single)(cacheBytes)
            _csrCache = New SparseCsrCache(cacheBytes)
        End Sub

        ''' <summary>自适应缓存容量的上限（4 GiB）。</summary>
        Public Const MaxCacheBytes As Long = 4L * 1024L * 1024L * 1024L

        ''' <summary>自适应缓存容量的下限（256 MiB），保证小显存设备上也有一份可用空间。</summary>
        Public Const MinCacheBytes As Long = 256L * 1024L * 1024L

        ''' <summary>
        ''' 按设备可用显存推算合适的驻留缓存容量：取可用量的 1/4，上限 4 GiB。
        ''' </summary>
        ''' <remarks>
        ''' 这里有两个缓存（单精度与双精度），各取可用量的 1/4，合计约占可用显存的一半，
        ''' 余下的留给 CUDA 上下文本身与各式算子输出缓冲。
        ''' <para>
        ''' 取 1/4 而不是更大，是因为容量过小虽然会频繁淘汰，容量过大则会挤压
        ''' 算子输出缓冲的分配空间；语言模型里单个张量可达数百 MB，
        ''' 在 8 GB 卡上 1/4（约 1.7 GiB）已足够容纳整个工作集。
        ''' </para>
        ''' 查询失败（上下文尚未就绪）时退回 <see cref="DefaultCacheBytes"/>。
        ''' </remarks>
        Private Shared Function AdaptiveCacheBytes() As Long
            Dim info As ILCudaRuntime.MemoryInfo = Nothing

            If Not ILCudaRuntime.CudaMemory.TryQuery(info) Then Return DefaultCacheBytes

            Dim quarter As Long = CLng(info.FreeBytes \ 4UL)
            Dim capped As Long = System.Math.Min(quarter, MaxCacheBytes)

            Return System.Math.Max(capped, MinCacheBytes)
        End Function

        ''' <summary>
        ''' 设备常驻缓冲注册表。
        ''' </summary>
        ''' <remarks>
        ''' 常驻缓冲由本注册表<b>独立持有</b>，不参与 <see cref="DeviceCache(Of T)"/> 的 LRU 淘汰，
        ''' 因此权重可以在整个训练过程中一直留在显存里。
        ''' </remarks>
        Public ReadOnly Property Resident As DeviceResidentStore
            Get
                Return _resident
            End Get
        End Property

        Public Overrides ReadOnly Property Name As String = "CUDA"

        ''' <summary>底层 CUDA 引擎</summary>
        Public ReadOnly Property Engine As ILCudaRuntime.CudaEngine
            Get
                Return _engine
            End Get
        End Property

        ''' <summary>IL2Cuda 生成内核的翻译失败信息（正常情况下为空）</summary>
        Public Shared ReadOnly Property KernelFailures As IReadOnlyDictionary(Of String, String)
            Get
                Return DoubleKernelRegistry.Failures
            End Get
        End Property

#Region "注册与切换"

        ''' <summary>
        ''' 尝试初始化 CUDA 并把 <see cref="tf.Tensor.computeKernel"/> 切换为 GPU 后端。
        ''' 设备不可用 / NVRTC 与驱动版本不匹配时返回 False，并且**不改变**当前后端。
        ''' </summary>
        ''' <param name="options">引擎选项（设备号 / NVRTC 路径等），<c>Nothing</c> 表示使用默认值</param>
        ''' <param name="cacheBytes">
        ''' 单精度与双精度两个驻留缓存各自的容量上限（字节）。
        ''' <c>0</c>（默认）表示按设备可用显存自适应；传正数则显式指定。
        ''' </param>
        ''' <remarks>
        ''' 缓存容量不能太小：语言模型的张量本身就有数百 MB，容量不足会让算子之间
        ''' 频繁互相淘汰，既拖慢速度也更容易触发淘汰相关的边界问题。
        ''' 自适应策略取可用显存的一半、上限 4 GiB，在 8 GB 卡上约为 3.5 GiB。
        ''' </remarks>
        ''' <param name="useFp32Gemm">
        ''' 是否让矩阵乘走单精度内核。默认 <c>True</c>。
        ''' 在消费级显卡（FP64 只有 FP32 的 1/64）上这是主要的加速来源；
        ''' 设为 <c>False</c> 可用于观察纯双精度路径的数值与耗时。
        ''' </param>
        Public Shared Function Register(Optional options As ILCudaRuntime.EngineOptions = Nothing,
                                        Optional cacheBytes As Long = 0,
                                        Optional useFp32Gemm As Boolean = True) As Boolean
            ' 必须在 CudaEngine.TryCreate 之前把内核源码注入编译单元
            DoubleKernelRegistry.EnsureRegistered()
            ILCudaKernels.KernelSources.Register(GetType(CudaTensor).Assembly)

            If options Is Nothing Then options = New ILCudaRuntime.EngineOptions()

            Dim engine = ILCudaRuntime.CudaEngine.TryCreate(options)

            If engine Is Nothing Then
                LastError = options.ErrorMessage
                Return False
            End If

            Dim backend As New CudaTensor(engine, cacheBytes)

            backend.UseFp32Gemm = useFp32Gemm

            SyncLock tf.Tensor.SyncRoot
                tf.Tensor.computeKernel = backend
            End SyncLock

            _current = backend
            LastError = Nothing
            Return True
        End Function

        ''' <summary>把计算后端切回默认的 SIMD CPU 实现</summary>
        Public Shared Sub Unregister()
            SyncLock tf.Tensor.SyncRoot
                tf.Tensor.computeKernel = tfCompute.SIMDTensor.Default
            End SyncLock

            _current = Nothing
        End Sub

#End Region

#Region "内部工具"

        Private Function OnGpu(t As tf.Tensor) As Boolean
            Return t IsNot Nothing AndAlso t.Length >= MinGpuElements
        End Function

        ''' <summary>
        ''' CPU 兜底之前，把被钉住的张量的最新内容同步回主机。
        ''' </summary>
        ''' <remarks>
        ''' 这是"设备常驻"最容易踩的坑：被钉住的张量以<b>设备为主副本</b>，主机
        ''' <c>Data</c> 会陈旧；而所有 CPU 实现读的都是主机数组。于是只要有一个算子
        ''' 落了 CPU 兜底，就会用旧权重静默算出错误结果。
        ''' <para>
        ''' 真实踩到的例子：增量解码（KV Cache）时注意力的 GEMM 只有 <c>m = 1</c>，
        ''' 规模 <c>1 × 128 × 128 = 16384</c> 低于 <see cref="MinGemmElements"/>（65536），
        ''' 于是落到 CPU 兜底。全序列路径走 GPU 读常驻缓冲、解码路径走 CPU 读陈旧主机副本，
        ''' 两条路径结果不一致 —— 表现为"KV Cache 逐 token 一致性"莫名失败。
        ''' </para>
        ''' <para>
        ''' 因此凡是可能消费权重、又存在 CPU 兜底分支的算子，都必须在兜底前调用本方法。
        ''' <see cref="UseResidentStore"/> 关闭时本方法几乎是空操作，不构成性能负担。
        ''' </para>
        ''' </remarks>
        Private Sub SyncIfPinned(t As tf.Tensor)
            If t Is Nothing Then Return

            ' 双精度通道优先：它承载的是脉冲网络的递归状态，主机副本一定是最陈旧的
            If _resident64.IsPinned(t.Data) Then
                Call SyncFromDevice(t)

                Return
            End If

            If Not UseResidentStore Then Return
            If Not _resident.IsPinned(t.Data) Then Return

            Call SyncFromDevice(t)
        End Sub

        ''' <summary>取张量底层数据对应的显存缓冲（驻留缓存，零精度损失）</summary>
        Private Function Device(t As tf.Tensor) As ILCudaRuntime.DeviceBuffer(Of Double)
            Return _cache.GetBuffer(_engine, t.Data, t.Version, Function(d) d)
        End Function

        ''' <summary>
        ''' 取张量对应的<b>单精度</b>显存缓冲（用于 FP32 的矩阵乘内核）。
        ''' </summary>
        ''' <remarks>
        ''' 查找顺序：<b>常驻表 → LRU 缓存</b>。
        '''   * 常驻表命中的是"权重 / 梯度 / 优化器状态"这类长期张量。
        '''     它们一旦钉住就不再随版本号失效，因此训练时不会每步重传整个模型
        '''     （200M 参数单精度下即 764 MB/步）；
        '''   * 未钉住的张量（激活、临时结果）落到 LRU 缓存，行为与改造前一致。
        ''' </remarks>
        Private Function DeviceF32(t As tf.Tensor) As ILCudaRuntime.DeviceBuffer(Of Single)
            If UseResidentStore Then
                Dim pinned As ILCudaRuntime.DeviceBuffer(Of Single) = Nothing

                If _resident.TryGet(t.Data, pinned) Then Return pinned
            End If

            Return _fp32.GetBuffer(_engine, t.Data, t.Version, AddressOf DeviceResidentStore.ToSingle)
        End Function

        ''' <summary>
        ''' 取张量对应的<b>双精度</b>显存缓冲，查找顺序同样是「常驻表 → LRU 缓存」。
        ''' </summary>
        ''' <remarks>
        ''' 与 <see cref="Device"/> 的唯一差别是它会先查双精度常驻表（<see cref="PinDevice64"/>）。
        ''' 这一点是脉冲网络能"零往返"的关键：被钉住的膜电位 / 脉冲 / 计数累加器
        ''' 跳过版本校验，内核直接原地读写同一片显存，主机数组只在需要时同步一次。
        ''' <para>
        ''' 反之，若用 <see cref="Device"/> 去读一个被钉住的张量，就会从<b>陈旧的主机副本</b>
        ''' 重新上传（LRU 缓存只看数组引用 + 版本号），静默算错 —— 所以凡是消费
        ''' 递归状态的算子里都必须用本方法取缓冲。
        ''' </para>
        ''' </remarks>
        Private Function Device64(t As tf.Tensor) As ILCudaRuntime.DeviceBuffer(Of Double)
            If UseFp64Residency Then
                Dim pinned As ILCudaRuntime.DeviceBuffer(Of Double) = Nothing

                If _resident64.TryGet(t.Data, pinned) Then Return pinned
            End If

            Return _cache.GetBuffer(_engine, t.Data, t.Version, Function(d) d)
        End Function

        ''' <summary>逐元素二元（纯标量内核：两个输入数组）</summary>
        Private Function EwBinary(a As tf.Tensor, b As tf.Tensor, kernelName As String) As tf.Tensor
            If Not OnGpu(a) OrElse Not DoubleKernelRegistry.Available(kernelName) Then
                ' 即将落到 CPU 兜底：先保证主机副本是最新的（设备常驻张量的主机侧会陈旧）
                Call SyncIfPinned(a)
                Call SyncIfPinned(b)

                Return Nothing
            End If

            Dim da = Device(a)
            Dim db = Device(b)

            Using dOut As New ILCudaRuntime.DeviceBuffer(Of Double)(a.Length)
                _engine.GetKernel(kernelName).Launch(
                    ILCudaRuntime.LaunchPlanner.For1D(a.Length, 256), da, db, dOut, a.Length)

                Return Wrap(dOut.Read(), a.Shape)
            End Using
        End Function

        ''' <summary>逐元素一元（可带 0 / 1 / 2 个运行时标量参数）</summary>
        Private Function EwUnary(t As tf.Tensor, kernelName As String,
                                 ParamArray scalars As Double()) As tf.Tensor
            If Not OnGpu(t) OrElse Not DoubleKernelRegistry.Available(kernelName) Then
                ' 即将落到 CPU 兜底：先保证主机副本是最新的
                Call SyncIfPinned(t)

                Return Nothing
            End If

            Dim dx = Device(t)

            Using dOut As New ILCudaRuntime.DeviceBuffer(Of Double)(t.Length)
                Dim args As New List(Of Object)
                args.Add(dx)

                For Each s As Double In scalars
                    args.Add(s)
                Next

                args.Add(dOut)
                args.Add(t.Length)

                _engine.GetKernel(kernelName).Launch(
                    ILCudaRuntime.LaunchPlanner.For1D(t.Length, 256), args.ToArray())

                Return Wrap(dOut.Read(), t.Shape)
            End Using
        End Function

#End Region

#Region "末轴 / 全局归约工具（P3 手写内核）"

        ''' <summary>启动一个“每 block 一行”的末轴内核</summary>
        Private Sub LaunchRow(kernelName As String,
                              x As ILCudaRuntime.DeviceBuffer(Of Double),
                              out As ILCudaRuntime.DeviceBuffer(Of Double),
                              outerSize As Integer, axisSize As Integer)
            ' tensor.cu 使用静态共享内存，不需要动态共享内存
            Dim config As New ILCudaRuntime.LaunchConfig(outerSize, 1, RowBlockSize, 1, 0)

            _engine.GetKernel(kernelName).Launch(config, x, out, outerSize, axisSize)
        End Sub

        ''' <summary>
        ''' 两段式全局归约：阶段一由多个 block 并行产出部分结果，阶段二由 1 个 block 汇总。
        ''' 两段都在 GPU 上完成，不需要主机端同步。
        ''' </summary>
        Private Function ReduceGlobal(t As tf.Tensor, partialKernel As String, finalKernel As String) As Double
            Dim n = t.Length
            Dim blocks = System.Math.Min(ILCudaRuntime.LaunchPlanner.CeilDiv(n, StageBlockSize), MaxStageBlocks)
            If blocks <= 0 Then blocks = 1

            Dim dx = Device(t)

            Using partials As New ILCudaRuntime.DeviceBuffer(Of Double)(blocks),
                  out As New ILCudaRuntime.DeviceBuffer(Of Double)(1)

                ' 阶段一：每个 block 一个部分结果
                Dim stage As New ILCudaRuntime.LaunchConfig(blocks, 1, StageBlockSize, 1, 0)
                _engine.GetKernel(partialKernel).Launch(stage, dx, partials, n)

                ' 阶段二：汇总部分结果
                Dim finalCfg As New ILCudaRuntime.LaunchConfig(1, 1, StageBlockSize, 1, 0)
                _engine.GetKernel(finalKernel).Launch(finalCfg, partials, out, blocks)

                Return out.Read()(0)
            End Using
        End Function

        ''' <summary>归一化轴下标并判断是否为“末轴且连续”</summary>
        Private Shared Function IsLastAxis(t As tf.Tensor, ByRef axis As Integer) As Boolean
            If axis < 0 Then axis += t.Rank
            Return axis = t.Rank - 1 AndAlso t.Length > 0
        End Function

        ''' <summary>归约后的输出形状（去掉 axis 维；keepdims 时该维保留为 1）</summary>
        Private Shared Function ReducedShape(shape As Integer(), axis As Integer, keepdims As Boolean) As Integer()
            Dim [dims] As New List(Of Integer)

            For i As Integer = 0 To shape.Length - 1
                If i = axis Then
                    If keepdims Then [dims].Add(1)
                Else
                    [dims].Add(shape(i))
                End If
            Next

            Return [dims].ToArray()
        End Function

        ''' <summary>
        ''' 末轴行归约：把张量按最后一维切成 outerSize 行逐行归约。
        ''' 不满足“末轴”条件时返回 Nothing，由调用方回退 CPU。
        ''' </summary>
        Private Function RowReduce(t As tf.Tensor, axis As Integer, kernelName As String,
                                   keepdims As Boolean) As tf.Tensor
            If Not OnGpu(t) Then Return Nothing

            Dim ax = axis
            If Not IsLastAxis(t, ax) Then Return Nothing

            Dim axisSize = t.Shape(ax)
            Dim outer = t.Length \ axisSize
            Dim outShape = ReducedShape(t.Shape, ax, keepdims)

            Dim dx = Device(t)

            Using dOut As New ILCudaRuntime.DeviceBuffer(Of Double)(outer)
                LaunchRow(kernelName, dx, dOut, outer, axisSize)
                Return Wrap(dOut.Read(), outShape)
            End Using
        End Function

        ''' <summary>末轴逐行 softmax / log-softmax（输出形状与输入相同）</summary>
        Private Function RowSoftmax(t As tf.Tensor, axis As Integer, kernelName As String) As tf.Tensor
            If Not OnGpu(t) Then Return Nothing

            Dim ax = axis
            If Not IsLastAxis(t, ax) Then Return Nothing

            Dim axisSize = t.Shape(ax)
            Dim outer = t.Length \ axisSize

            Dim dx = Device(t)

            Using dOut As New ILCudaRuntime.DeviceBuffer(Of Double)(t.Length)
                LaunchRow(kernelName, dx, dOut, outer, axisSize)
                Return Wrap(dOut.Read(), t.Shape)
            End Using
        End Function

#End Region

#Region "逐元素 - 二元"

        Public Overrides Function Add(a As tf.Tensor, b As tf.Tensor) As tf.Tensor
            RequireSameShape(a, b, "相加")
            Dim r = EwBinary(a, b, DoubleKernelRegistry.EwAdd)
            If r IsNot Nothing Then Return r
            Return MyBase.Add(a, b)
        End Function

        Public Overrides Function Subtract(a As tf.Tensor, b As tf.Tensor) As tf.Tensor
            RequireSameShape(a, b, "相减")
            Dim r = EwBinary(a, b, DoubleKernelRegistry.EwSub)
            If r IsNot Nothing Then Return r
            Return MyBase.Subtract(a, b)
        End Function

        Public Overrides Function Multiply(a As tf.Tensor, b As tf.Tensor) As tf.Tensor
            RequireSameShape(a, b, "相乘")
            Dim r = EwBinary(a, b, DoubleKernelRegistry.EwMul)
            If r IsNot Nothing Then Return r
            Return MyBase.Multiply(a, b)
        End Function

        Public Overrides Function Divide(a As tf.Tensor, b As tf.Tensor) As tf.Tensor
            RequireSameShape(a, b, "相除")
            Dim r = EwBinary(a, b, DoubleKernelRegistry.EwDiv)
            If r IsNot Nothing Then Return r
            Return MyBase.Divide(a, b)
        End Function

#End Region

#Region "逐元素 - 一元"

        Public Overrides Function Exp(t As tf.Tensor) As tf.Tensor
            Dim r = EwUnary(t, DoubleKernelRegistry.EwExp)
            If r IsNot Nothing Then Return r
            Return MyBase.Exp(t)
        End Function

        Public Overrides Function Log(t As tf.Tensor) As tf.Tensor
            Dim r = EwUnary(t, DoubleKernelRegistry.EwLog)
            If r IsNot Nothing Then Return r
            Return MyBase.Log(t)
        End Function

        Public Overrides Function Sqrt(t As tf.Tensor) As tf.Tensor
            Dim r = EwUnary(t, DoubleKernelRegistry.EwSqrt)
            If r IsNot Nothing Then Return r
            Return MyBase.Sqrt(t)
        End Function

        Public Overrides Function Abs(t As tf.Tensor) As tf.Tensor
            Dim r = EwUnary(t, DoubleKernelRegistry.EwAbs)
            If r IsNot Nothing Then Return r
            Return MyBase.Abs(t)
        End Function

        Public Overrides Function Negate(t As tf.Tensor) As tf.Tensor
            Dim r = EwUnary(t, DoubleKernelRegistry.EwNeg)
            If r IsNot Nothing Then Return r
            Return MyBase.Negate(t)
        End Function

        Public Overrides Function Reciprocal(t As tf.Tensor) As tf.Tensor
            Dim r = EwUnary(t, DoubleKernelRegistry.EwRecip)
            If r IsNot Nothing Then Return r
            Return MyBase.Reciprocal(t)
        End Function

        Public Overrides Function Square(t As tf.Tensor) As tf.Tensor
            Dim r = EwUnary(t, DoubleKernelRegistry.EwSquare)
            If r IsNot Nothing Then Return r
            Return MyBase.Square(t)
        End Function

        Public Overrides Function Tanh(t As tf.Tensor) As tf.Tensor
            Dim r = EwUnary(t, DoubleKernelRegistry.EwTanh)
            If r IsNot Nothing Then Return r
            Return MyBase.Tanh(t)
        End Function

        Public Overrides Function Sigmoid(t As tf.Tensor) As tf.Tensor
            Dim r = EwUnary(t, DoubleKernelRegistry.EwSigmoid)
            If r IsNot Nothing Then Return r
            Return MyBase.Sigmoid(t)
        End Function

        Public Overrides Function Sin(t As tf.Tensor) As tf.Tensor
            Dim r = EwUnary(t, DoubleKernelRegistry.EwSin)
            If r IsNot Nothing Then Return r
            Return MyBase.Sin(t)
        End Function

        Public Overrides Function Cos(t As tf.Tensor) As tf.Tensor
            Dim r = EwUnary(t, DoubleKernelRegistry.EwCos)
            If r IsNot Nothing Then Return r
            Return MyBase.Cos(t)
        End Function

        Public Overrides Function Relu(t As tf.Tensor) As tf.Tensor
            Dim r = EwUnary(t, DoubleKernelRegistry.EwRelu)
            If r IsNot Nothing Then Return r
            Return MyBase.Relu(t)
        End Function

        ''' <summary>
        ''' 阶跃函数(Heaviside): 大于 0 的元素取 1, 否则取 0
        ''' </summary>
        Public Overrides Function Heaviside(t As tf.Tensor) As tf.Tensor
            Dim r = EwUnary(t, DoubleKernelRegistry.EwStep)
            If r IsNot Nothing Then Return r
            Return MyBase.Heaviside(t)
        End Function

        Public Overrides Function Gelu(t As tf.Tensor) As tf.Tensor
            Dim r = EwUnary(t, DoubleKernelRegistry.EwGelu)
            If r IsNot Nothing Then Return r
            Return MyBase.Gelu(t)
        End Function

        Public Overrides Function Swish(t As tf.Tensor) As tf.Tensor
            Dim r = EwUnary(t, DoubleKernelRegistry.EwSwish)
            If r IsNot Nothing Then Return r
            Return MyBase.Swish(t)
        End Function

        Public Overrides Function LeakyRelu(t As tf.Tensor, alpha As Double) As tf.Tensor
            Dim r = EwUnary(t, DoubleKernelRegistry.EwLeakyRelu, alpha)
            If r IsNot Nothing Then Return r
            Return MyBase.LeakyRelu(t, alpha)
        End Function

        Public Overrides Function Elu(t As tf.Tensor, alpha As Double) As tf.Tensor
            Dim r = EwUnary(t, DoubleKernelRegistry.EwElu, alpha)
            If r IsNot Nothing Then Return r
            Return MyBase.Elu(t, alpha)
        End Function

#End Region

#Region "标量运算"

        Public Overrides Function AddScalar(t As tf.Tensor, scalar As Double) As tf.Tensor
            Dim r = EwUnary(t, DoubleKernelRegistry.EwAddScalar, scalar)
            If r IsNot Nothing Then Return r
            Return MyBase.AddScalar(t, scalar)
        End Function

        Public Overrides Function MultiplyScalar(t As tf.Tensor, scalar As Double) As tf.Tensor
            Dim r = EwUnary(t, DoubleKernelRegistry.EwScale, scalar)
            If r IsNot Nothing Then Return r
            Return MyBase.MultiplyScalar(t, scalar)
        End Function

        Public Overrides Function DivideScalar(t As tf.Tensor, scalar As Double) As tf.Tensor
            Dim r = EwUnary(t, DoubleKernelRegistry.EwDivScalar, scalar)
            If r IsNot Nothing Then Return r
            Return MyBase.DivideScalar(t, scalar)
        End Function

        Public Overrides Function Pow(t As tf.Tensor, exponent As Double) As tf.Tensor
            Dim r = EwUnary(t, DoubleKernelRegistry.EwPow, exponent)
            If r IsNot Nothing Then Return r
            Return MyBase.Pow(t, exponent)
        End Function

        Public Overrides Function Clip(t As tf.Tensor, low As Double, high As Double) As tf.Tensor
            Dim r = EwUnary(t, DoubleKernelRegistry.EwClip, low, high)
            If r IsNot Nothing Then Return r
            Return MyBase.Clip(t, low, high)
        End Function

#End Region

#Region "矩阵运算"

        ''' <summary>
        ''' 分块矩阵乘：<c>C(m×n) = A(m×k) · B(k×n)</c>。
        ''' </summary>
        ''' <remarks>
        ''' 默认走<b>单精度</b>内核（Kernels\blas.cu 的 <c>gemmKernel</c>）：
        ''' 消费级显卡的 FP64 吞吐只有 FP32 的 1/64，而 GEMM 是语言模型里
        ''' 占绝对主导的浮点运算。主机侧数据始终是 <c>Double</c>，
        ''' 只在显存里降精度计算，结果再升精度回写主机。
        ''' <see cref="UseFp32Gemm"/> 设为 <c>False</c> 可退回纯双精度路径做数值对照。
        ''' </remarks>
        Public Overrides Function MatMul(a As tf.Tensor, b As tf.Tensor) As tf.Tensor
            If a.Rank <> 2 OrElse b.Rank <> 2 Then
                Throw New ArgumentException("矩阵乘法需要二维张量")
            End If
            If a.Shape(1) <> b.Shape(0) Then
                Throw New ArgumentException($"矩阵维度不匹配: {a.Shape(1)} != {b.Shape(0)}")
            End If

            Dim m = a.Shape(0)
            Dim k = a.Shape(1)
            Dim n = b.Shape(1)

            ' 规模必须用 Long 计算：语言模型的输出层是 [N, d_model] × [d_model, vocab]，
            ' 在 10 万级词表下 m*k*n 轻松超过 Int32 上限（约 21.5 亿），
            ' 用 Int32 会直接抛 OverflowException。写法与 SIMDTensor.MatMul 保持一致。
            Dim totalOps As Long = CLng(m) * k * n

            If totalOps < MinGemmElements Then
                ' CPU 兜底读的是主机数组，必须先把常驻权重同步回来。
                ' 增量解码（m = 1）恰好会落到这条分支上，漏掉这一步会让
                ' "KV Cache 有/无缓存两条路径"给出不同结果。
                Call SyncIfPinned(a)
                Call SyncIfPinned(b)

                Return MyBase.MatMul(a, b)
            End If

            Dim elements As Long = CLng(m) * n
            Call EnsureDeviceCount(elements, "MatMul 的输出")

            If UseFp32Gemm Then
                Dim fp32Kernel = TryKernel(ILCudaRuntime.KernelNames.Gemm)

                If fp32Kernel IsNot Nothing Then
                    Return MatMulFp32(a, b, m, k, n, CInt(elements), fp32Kernel)
                End If
            End If

            ' 内核缺失（NVRTC 编译失败 / 驱动不匹配）时回退 CPU，
            ' 与 Transpose / Conv2D / SpMM 的回退策略保持一致
            Dim kernel = TryKernel(TensorKernelNames.GemmDouble)
            If kernel Is Nothing Then
                Call SyncIfPinned(a)
                Call SyncIfPinned(b)

                Return MyBase.MatMul(a, b)
            End If

            Dim da = Device(a)
            Dim db = Device(b)

            Using dC As New ILCudaRuntime.DeviceBuffer(Of Double)(CInt(elements))
                kernel.Launch(
                    ILCudaRuntime.LaunchPlanner.For2D(m, n, 16, 16),
                    da, db, dC, m, n, k)

                Return Wrap(dC.Read(), New Integer() {m, n})
            End Using
        End Function

        ''' <summary>
        ''' 单精度矩阵乘路径：输入从主机（或常驻表）取 FP32 缓冲，
        ''' 结果回读后升精度为 <c>Double()</c>。
        ''' </summary>
        ''' <remarks>
        ''' 内核是 ILCuda 自带的 <c>gemmKernel</c>，其索引全程用 <c>size_t</c> 转换
        ''' （见 Kernels\blas.cu），因此 12.8 万词表这类大矩阵在内核内部不会有 Int32 溢出。
        ''' </remarks>
        Private Function MatMulFp32(a As tf.Tensor, b As tf.Tensor, m As Integer, k As Integer,
                                    n As Integer, elements As Integer,
                                    kernel As ILCudaRuntime.CudaKernel) As tf.Tensor
            Dim da = DeviceF32(a)
            Dim db = DeviceF32(b)

            Using dC As New ILCudaRuntime.DeviceBuffer(Of Single)(elements)
                kernel.Launch(
                    ILCudaRuntime.LaunchPlanner.For2D(m, n, 16, 16),
                    da, db, dC, m, n, k)

                Return Wrap(DeviceResidentStore.ToDouble(dC.Read()), New Integer() {m, n})
            End Using
        End Function

        ''' <summary>
        ''' 校验单次算子输出的元素数落在 <c>DeviceBuffer(Of T)</c> 的 <c>Int32</c> 计数契约内。
        ''' </summary>
        ''' <remarks>
        ''' <c>ILCuda\Runtime\DeviceBuffer.vb</c> 的构造函数形参与 <c>Count</c> 属性都是
        ''' <c>Integer</c>，因此单个算子能寻址的元素数上限是 <see cref="Integer.MaxValue"/>
        ''' （约 21.5 亿）。张量形状本身也是 <c>Integer()</c>，所以这个分支在纯 CPU 语义下
        ''' 不可达；显式写出来是为了让"越界"变成一条指出算子与规模的<b>可读诊断</b>，
        ''' 而不是让底层算术静默溢出后抛出难以定位的异常。
        ''' </remarks>
        Private Shared Sub EnsureDeviceCount(elements As Long, opName As String)
            If elements > Integer.MaxValue Then
                Throw New NotSupportedException(
                    $"{opName} 需要 {elements:N0} 个元素，超出显存缓冲的 Int32 计数上限 " &
                    $"{Integer.MaxValue:N0}；请减小 batch / 序列长度或词表规模")
            End If
        End Sub

        ''' <summary>
        ''' 稀疏（CSR）× 稠密矩阵乘：dense[batch, Rows] · W[Rows, Columns] → [batch, Columns]。
        ''' </summary>
        ''' <remarks>
        ''' CSR 三个数组按 <see cref="tfCompute.SparseCsr"/> 的引用与版本常驻显存；
        ''' 内核为 Kernels\spmm.cu 的 <c>tensorSpmmCsrKernel</c>（按 (batch,row) 行并行，
        ''' 输出用 atomicAdd(double) 累加，需 sm_60 及以上）。
        ''' 无连接、规模过小、或内核不可用（NVRTC 编译失败）时自动回退 CPU 实现。
        ''' </remarks>
        Public Overrides Function SpMM(csr As tfCompute.SparseCsr, dense As tf.Tensor) As tf.Tensor
            If csr Is Nothing Then Throw New ArgumentNullException(NameOf(csr))
            If dense Is Nothing Then Throw New ArgumentNullException(NameOf(dense))
            If dense.Rank <> 2 OrElse dense.Shape(1) <> csr.Rows Then
                Throw New ArgumentException(
                    $"SpMM 输入形状应为 [batch, {csr.Rows}]，实际 [{String.Join(",", dense.Shape)}]")
            End If

            ' 无连接 / 规模过小 → CPU 兜底（同时避免空显存缓冲分配）
            If csr.NonZeros <= 0 OrElse csr.NonZeros < MinSparseNnz Then
                Return MyBase.SpMM(csr, dense)
            End If

            Dim kernel = TryKernel(TensorKernelNames.SpmmCsr)
            If kernel Is Nothing Then Return MyBase.SpMM(csr, dense)

            Dim batch = dense.Shape(0)
            Dim rows = csr.Rows
            Dim columns = csr.Columns

            ' CSR 数组常驻显存；dense 复用通用张量显存缓存
            Dim csrBuf = _csrCache.GetBuffers(_engine, csr)
            Dim ddense = Device(dense)

            Dim elements As Long = CLng(batch) * columns
            Call EnsureDeviceCount(elements, "SpMM 的输出")

            Using dOut As New ILCudaRuntime.DeviceBuffer(Of Double)(CInt(elements))
                ' 内核用 atomicAdd 累加到输出，启动前必须清零
                dOut.Fill(0.0)

                kernel.Launch(ILCudaRuntime.LaunchPlanner.For1D(batch * rows, 256),
                              csrBuf.RowPtr, csrBuf.ColIdx, csrBuf.Values,
                              ddense, dOut, rows, columns, batch)

                Return Wrap(dOut.Read(), New Integer() {batch, columns})
            End Using
        End Function

        ''' <summary>
        ''' 融合的稀疏递归 LIF 单步（GPU）：先由 <c>tensorSpmmCsrKernel</c> 把递归输入
        ''' 散射进<b>跨步复用</b>的显存缓冲 <c>I</c>，再由 <c>tensorLifUpdate*Kernel</c>
        ''' 就地更新膜电位 / 脉冲 / 计数累加器。
        ''' </summary>
        ''' <remarks>
        ''' 本覆写相对基类（主机标量实现）的差别只有"数据放在哪里"：
        ''' <list type="bullet">
        '''   <item>递归状态以<b>设备为主副本</b>，跨时间步常驻显存，全程零主机往返；</item>
        '''   <item><c>I</c> 缓冲按膜电位张量的引用身份缓存，只分配一次。</item>
        ''' </list>
        ''' 因此本覆写<b>要求状态张量已通过</b> <see cref="PinDevice64"/>（双精度档）或
        ''' <see cref="PinDevice"/>（单精度档）<b>钉住</b>：内核是就地写入的，
        ''' 若状态只存在于 LRU 缓存里，调用方就再也无法把结果同步回主机数组
        ''' （<see cref="SyncFromDevice"/> 只认常驻表）。任一前提不满足时返回 <c>False</c>，
        ''' 由调用方回退到逐算子路径 —— 语义完全一致，只是慢一些。
        ''' <para>
        ''' 档位选择：<c>h</c> 与 <c>counts</c> 双双钉在双精度表 → 走全双精度内核
        ''' （与 CPU 逐位一致）；钉在单精度表 → 走混合内核（<c>H</c>/<c>counts</c> 单精度，
        ''' 但脉冲 <c>S</c> 仍写双精度，以便下一步直接复用双精度的稀疏内核，
        ''' 而 0/1 在两种精度下都是精确值）。
        ''' </para>
        ''' </remarks>
        Public Overrides Function LifStep(synapses As tfCompute.SparseCsr,
                                          sPrev As tf.Tensor,
                                          externalCurrent As tf.Tensor,
                                          h As tf.Tensor,
                                          s As tf.Tensor,
                                          counts As tf.Tensor,
                                          beta As Double,
                                          threshold As Double,
                                          subtractThreshold As Boolean) As Boolean

            If synapses Is Nothing Then Throw New ArgumentNullException(NameOf(synapses))
            If sPrev Is Nothing Then Throw New ArgumentNullException(NameOf(sPrev))
            If h Is Nothing Then Throw New ArgumentNullException(NameOf(h))
            If s Is Nothing Then Throw New ArgumentNullException(NameOf(s))
            If counts Is Nothing Then Throw New ArgumentNullException(NameOf(counts))

            If sPrev.Rank <> 2 OrElse sPrev.Shape(1) <> synapses.Rows Then
                Throw New ArgumentException(
                    $"LifStep 的 sPrev 形状应为 [batch, {synapses.Rows}]，实际 [{String.Join(",", sPrev.Shape)}]")
            End If
            If h.Shape(0) <> sPrev.Shape(0) OrElse h.Shape(1) <> synapses.Columns OrElse
               Not h.Shape.SequenceEqual(s.Shape) OrElse Not h.Shape.SequenceEqual(counts.Shape) Then
                Throw New ArgumentException(
                    $"LifStep 的 h/s/counts 形状应同为 [{sPrev.Shape(0)}, {synapses.Columns}]，" &
                    $"实际 h=[{String.Join(",", h.Shape)}] s=[{String.Join(",", s.Shape)}] counts=[{String.Join(",", counts.Shape)}]")
            End If
            If externalCurrent IsNot Nothing AndAlso Not externalCurrent.Shape.SequenceEqual(h.Shape) Then
                Throw New ArgumentException(
                    $"LifStep 的 externalCurrent 形状应为 [{String.Join(",", h.Shape)}]，" &
                    $"实际 [{String.Join(",", externalCurrent.Shape)}]")
            End If

            ' ---- 能力前提：状态必须常驻 + 两个内核都必须可用 ----
            Dim fp64State As Boolean = UseFp64Residency AndAlso
                                       _resident64.IsPinned(h.Data) AndAlso
                                       _resident64.IsPinned(counts.Data)

            ' 脉冲在每个时间步都会被改写，因此必须走双精度常驻表：
            ' 下一步的 tensorSpmmCsrKernel 的 dense 输入就是它（double）
            If Not _resident64.IsPinned(sPrev.Data) OrElse Not _resident64.IsPinned(s.Data) Then
                Return False
            End If

            If Not fp64State AndAlso Not (_resident.IsPinned(h.Data) AndAlso _resident.IsPinned(counts.Data)) Then
                Return False
            End If

            ' 规模过小时显存拷贝 / 启动开销会倒挂（同时避免空缓冲）
            If synapses.NonZeros <= 0 OrElse synapses.NonZeros < MinSparseNnz Then
                Return False
            End If

            Dim lifKernel = TryKernel(If(fp64State, TensorKernelNames.LifUpdateDouble, TensorKernelNames.LifUpdateFp32))
            If lifKernel Is Nothing Then Return False

            Dim spmmKernel = TryKernel(TensorKernelNames.SpmmCsr)
            If spmmKernel Is Nothing Then Return False

            Dim batch = sPrev.Shape(0)
            Dim rows = synapses.Rows
            Dim columns = synapses.Columns
            Dim elements As Long = CLng(batch) * columns

            Call EnsureDeviceCount(elements, "LifStep 的突触输入")
            Call EnsureDeviceCount(CLng(batch) * rows, "LifStep 的稀疏输入行数")

            Dim workspace = Me.GetLifWorkspace(h.Data, CInt(elements))
            Dim csrBuf = _csrCache.GetBuffers(_engine, synapses)

            ' 1) 稀疏递归输入：I = W · S_prev（atomicAdd 累加型内核，启动前必须清零）
            workspace.Input.Fill(0.0)

            spmmKernel.Launch(ILCudaRuntime.LaunchPlanner.For1D(batch * rows, 256),
                              csrBuf.RowPtr, csrBuf.ColIdx, csrBuf.Values,
                              Device64(sPrev), workspace.Input, rows, columns, batch)

            ' 2) 泄漏积分 → 阈值触发 → 复位 → 计数累加（就地）
            '    β 与复位阈值都按单精度舍入后传入，与 Tensor * CSng(x) 的语义一致
            Dim extBuffer As Object = Nothing

            If externalCurrent IsNot Nothing Then
                extBuffer = If(fp64State, CObj(Device64(externalCurrent)), CObj(DeviceF32(externalCurrent)))
            End If

            Dim n As Integer = CInt(elements)
            Dim subtractFlag As Integer = If(subtractThreshold, 1, 0)

            If fp64State Then
                lifKernel.Launch(ILCudaRuntime.LaunchPlanner.For1D(n, 256),
                                 workspace.Input, extBuffer,
                                 Device64(h), Device64(s), Device64(counts),
                                 n, CDbl(CSng(beta)), threshold, CDbl(CSng(threshold)), subtractFlag)
            Else
                lifKernel.Launch(ILCudaRuntime.LaunchPlanner.For1D(n, 256),
                                 workspace.Input, extBuffer,
                                 DeviceF32(h), Device64(s), DeviceF32(counts),
                                 n, CSng(beta), CSng(threshold), CSng(threshold), subtractFlag)
            End If

            Return True
        End Function

        ''' <summary>
        ''' 取（或按需创建）某个膜电位张量对应的融合 LIF 工作区。
        ''' </summary>
        ''' <remarks>
        ''' 键是主机 <c>Double()</c> 数组的引用身份：同一层在同一个仿真窗里每步复用的是
        ''' 同一个张量对象，因此整段仿真只会分配一次 <c>I</c> 缓冲；
        ''' 神经元数量或批大小变化（<c>ResetState</c> 之后）时按元素数重建。
        ''' </remarks>
        Private Function GetLifWorkspace(key As Double(), elements As Integer) As LifWorkspace
            If elements <= 0 Then Throw New ArgumentException("融合 LIF 工作区的元素数必须为正", NameOf(elements))

            SyncLock _lifSync
                Dim workspace As LifWorkspace = Nothing

                If _lifWorkspaces.TryGetValue(key, workspace) Then
                    If workspace.Elements = elements Then Return workspace

                    ' 形状变了：释放旧缓冲后重建
                    _lifWorkspaces.Remove(key)
                    workspace.Dispose()
                End If

                Dim created As New LifWorkspace(elements)

                _lifWorkspaces(key) = created

                Return created
            End SyncLock
        End Function

        ''' <summary>释放某个张量对应的融合 LIF 工作区（未占用时返回 <c>False</c>）。</summary>
        Private Function ReleaseLifWorkspace(key As Double()) As Boolean
            If key Is Nothing Then Return False

            SyncLock _lifSync
                Dim workspace As LifWorkspace = Nothing

                If Not _lifWorkspaces.TryGetValue(key, workspace) Then Return False

                _lifWorkspaces.Remove(key)
                workspace.Dispose()
            End SyncLock

            Return True
        End Function

        ''' <summary>当前缓存的融合 LIF 工作区个数（诊断用）。</summary>
        Public ReadOnly Property LifWorkspaceCount As Integer
            Get
                SyncLock _lifSync
                    Return _lifWorkspaces.Count
                End SyncLock
            End Get
        End Property

        ''' <summary>
        ''' 二维转置：由 IL2Cuda 生成的 Grid2D double 内核完成，
        ''' 输出 (C, R) 的第 (i, j) 个元素 = 输入 (R, C) 的第 (j, i) 个元素。
        ''' </summary>
        Public Overrides Function Transpose(t As tf.Tensor) As tf.Tensor
            If t.Rank <> 2 Then
                Throw New ArgumentException("只支持二维张量转置")
            End If

            If Not OnGpu(t) Then
                Call SyncIfPinned(t)

                Return MyBase.Transpose(t)
            End If

            Dim rows = t.Shape(0)
            Dim cols = t.Shape(1)

            Dim elements As Long = CLng(rows) * cols
            Call EnsureDeviceCount(elements, "Transpose 的输出")

            ' 优先走单精度。除了与 MatMul 的精度策略保持一致，这里还有一个
            ' <b>正确性</b>理由：设备常驻的权重以设备为主副本、主机副本是陈旧的，
            ' 而 KernelNames 里的双精度转置是从主机数组上传的 —— 经它转置会静默算错。
            ' 反向传播恰好需要"转置后的权重"（dA = dC · Bᵀ），因此必须走这条能直读常驻表的路径。
            If UseFp32Gemm Then
                Dim fp32Kernel = TryKernel(TensorKernelNames.TrainTranspose)

                If fp32Kernel IsNot Nothing Then
                    Dim dx32 = DeviceF32(t)

                    Using dOut32 As New ILCudaRuntime.DeviceBuffer(Of Single)(CInt(elements))
                        ' 手写内核的形参顺序是 (x, y, rows, cols)，rows / cols 描述的是<b>输入</b>形状；
                        ' 内核里 j = blockIdx.x·blockDim.x + threadIdx.x 索引输入的列、i = blockIdx.y… 索引行。
                        '
                        ' 因此这里必须用 For2D(rows, cols)：For2D 的<b>第一个参数映射到 gridY</b>、
                        ' 第二个映射到 gridX（见 LaunchPlanner.For2D 的实现）。
                        ' 早先误写为 For2D(cols, rows)，gridX 便按 rows 计算 ——
                        ' 非方阵（rows ≠ cols）时 i 只覆盖到 cols 行，输出里有一半元素根本没被写入，
                        ' 表现为"FP32 转置结果里混着未初始化的垃圾值"（方阵下恰好看不出问题，
                        ' 因为两种写法的 grid 覆盖范围一致）。
                        fp32Kernel.Launch(
                            ILCudaRuntime.LaunchPlanner.For2D(rows, cols, 16, 16),
                            dx32, dOut32, rows, cols)

                        Return Wrap(DeviceResidentStore.ToDouble(dOut32.Read()), New Integer() {cols, rows})
                    End Using
                End If
            End If

            If Not DoubleKernelRegistry.Available(DoubleKernelRegistry.Transpose) Then
                Return MyBase.Transpose(t)
            End If

            Dim dx = Device(t)

            ' 输出形状为 (cols, rows)
            Using dOut As New ILCudaRuntime.DeviceBuffer(Of Double)(CInt(elements))
                _engine.GetKernel(DoubleKernelRegistry.Transpose).Launch(
                    ILCudaRuntime.LaunchPlanner.For2D(cols, rows, 16, 16),
                    dx, cols, dOut, cols, rows)

                Return Wrap(dOut.Read(), New Integer() {cols, rows})
            End Using
        End Function

#End Region

#Region "批量细胞管线融合算子（手写内核：Kernels\cellaccel.cu）"

        ''' <summary>融合内核允许的最大线程数（blockDim.x 的 CUDA 上限）。</summary>
        Private Const MaxCellKernelThreads As Integer = 1024

        ''' <summary>
        ''' 融合内核的图卷积层线程数：输出特征维度不超过 32，用 32 线程覆盖。
        ''' </summary>
        Private Const CellGraphLayerBlockSize As Integer = 32

        ''' <summary>
        ''' 确保张量已钉成<b>双精度</b>设备常驻缓冲——本组融合内核全部就地写入状态，
        ''' 只有常驻表里的缓冲才能被 <see cref="SyncFromDevice"/> 同步回主机数组。
        ''' </summary>
        ''' <returns>张量已在常驻表中时返回 True；无法钉住时返回 False（调用方回退 CPU）</returns>
        Private Function EnsureResident64(t As tf.Tensor, label As String, zeroFill As Boolean) As Boolean
            If t Is Nothing OrElse t.Length <= 0 Then Return False
            If IsDevicePinned(t) Then Return True
            If Not SupportsDeviceResidency Then Return False

            Return PinDevice64(t, label, zeroFill)
        End Function

        ''' <summary>
        ''' 液态时间常数网络（LTC / LNN）批量 RK4 积分（GPU）。
        ''' </summary>
        ''' <remarks>
        ''' 整批细胞的状态 <c>[B, m]</c> 常驻显存，<c>subSteps</c> 个子步在内核里一次跑完：
        ''' 一次内核启动服务全部细胞，且没有任何主机往返（对比：逐算子写法每步需要
        ''' 十几次 GEMM + 逐元素调用，每次都要上传/回读）。
        ''' <para>
        ''' 任一前提不满足（内核缺失 / 形状不支持 / 状态无法钉住 / 可选权重缺失）时
        ''' 直接走基类的标量实现，语义完全等价，只是慢一些。
        ''' </para>
        ''' </remarks>
        Public Overrides Function LtcRk4Batch(state As tf.Tensor, input As tf.Tensor,
                                              weightRecurrent As tf.Tensor, weightInput As tf.Tensor, bias As tf.Tensor,
                                              weightGate As tf.Tensor, weightGateInput As tf.Tensor, biasGate As tf.Tensor,
                                              tauEff As tf.Tensor, dt As Double, subSteps As Integer,
                                              activation As Integer) As Boolean

            Dim kernel = TryKernel(TensorKernelNames.CellLtcRk4)

            If kernel Is Nothing OrElse state Is Nothing OrElse input Is Nothing OrElse
               weightRecurrent Is Nothing OrElse weightInput Is Nothing OrElse tauEff Is Nothing Then
                Return MyBase.LtcRk4Batch(state, input, weightRecurrent, weightInput, bias,
                                          weightGate, weightGateInput, biasGate, tauEff, dt, subSteps, activation)
            End If
            If state.Rank <> 2 OrElse input.Rank <> 2 OrElse weightRecurrent.Rank <> 2 OrElse
               weightInput.Rank <> 2 OrElse tauEff.Length <> state.Shape(1) Then
                Return MyBase.LtcRk4Batch(state, input, weightRecurrent, weightInput, bias,
                                          weightGate, weightGateInput, biasGate, tauEff, dt, subSteps, activation)
            End If

            Dim batch As Integer = state.Shape(0)
            Dim units As Integer = state.Shape(1)
            Dim nIn As Integer = input.Shape(1)

            If batch <= 0 OrElse units <= 0 OrElse nIn <= 0 Then
                Return MyBase.LtcRk4Batch(state, input, weightRecurrent, weightInput, bias,
                                          weightGate, weightGateInput, biasGate, tauEff, dt, subSteps, activation)
            End If
            If units > MaxCellKernelThreads OrElse input.Shape(0) <> batch Then
                ' blockDim 用单元数铺满：超过上限时内核无法按"一线程一单元"映射
                Return MyBase.LtcRk4Batch(state, input, weightRecurrent, weightInput, bias,
                                          weightGate, weightGateInput, biasGate, tauEff, dt, subSteps, activation)
            End If
            If Not EnsureResident64(state, "cell.ltc.state", zeroFill:=False) Then
                Return MyBase.LtcRk4Batch(state, input, weightRecurrent, weightInput, bias,
                                          weightGate, weightGateInput, biasGate, tauEff, dt, subSteps, activation)
            End If

            Dim hasGate As Boolean = weightGate IsNot Nothing AndAlso weightGateInput IsNot Nothing
            Dim sharedBytes As Integer = 6 * units * 8

            kernel.Launch(batch, 1, units, 1, sharedBytes,
                          Device64(state), Device64(input),
                          Device64(weightRecurrent), Device64(weightInput),
                          If(bias Is Nothing, Nothing, Device64(bias)),
                          If(hasGate, Device64(weightGate), Nothing),
                          If(hasGate, Device64(weightGateInput), Nothing),
                          If(hasGate AndAlso biasGate IsNot Nothing, Device64(biasGate), Nothing),
                          Device64(tauEff),
                          units, nIn, dt, System.Math.Max(1, subSteps), If(hasGate, 1, 0), activation)

            ' 就地写入：主机副本此刻是陈旧的，读主机数组之前必须 SyncFromDevice
            Return True
        End Function

        ''' <summary>
        ''' 图卷积批量层（GPU）：自身分支 + CSR 邻居聚合 + 偏置 + 激活。
        ''' </summary>
        ''' <remarks>
        ''' 拓扑与系数对全部细胞共享，因此一份 CSR 常驻显存即可服务整个 batch；
        ''' 节点特征按 <c>[B·n, inF]</c> 行优先布局，邻居行号 = 批内基址 + 源节点号。
        ''' <c>edges</c> 为空（解码器等无邻居层）时内核退化为普通全连接层。
        ''' </remarks>
        Public Overrides Function GraphLayerBatch(x As tf.Tensor, wSelf As tf.Tensor, wRel As tf.Tensor,
                                                  selfW As tf.Tensor, bias As tf.Tensor, edges As tfCompute.SparseCsr,
                                                  out As tf.Tensor, activation As Integer) As Boolean

            Dim kernel = TryKernel(TensorKernelNames.CellGraphLayer)

            If kernel Is Nothing OrElse x Is Nothing OrElse wSelf Is Nothing OrElse out Is Nothing Then
                Return MyBase.GraphLayerBatch(x, wSelf, wRel, selfW, bias, edges, out, activation)
            End If
            If x.Rank <> 2 OrElse out.Rank <> 2 OrElse x.Shape(0) <> out.Shape(0) Then
                Return MyBase.GraphLayerBatch(x, wSelf, wRel, selfW, bias, edges, out, activation)
            End If

            Dim rows As Integer = x.Shape(0)
            Dim inF As Integer = x.Shape(1)
            Dim outF As Integer = out.Shape(1)
            Dim nodes As Integer = If(edges IsNot Nothing, edges.Rows,
                                      If(selfW IsNot Nothing, selfW.Length, rows))

            If rows <= 0 OrElse inF <= 0 OrElse outF <= 0 OrElse nodes <= 0 Then
                Return MyBase.GraphLayerBatch(x, wSelf, wRel, selfW, bias, edges, out, activation)
            End If
            If Not EnsureResident64(out, "cell.graph.out", zeroFill:=False) Then
                Return MyBase.GraphLayerBatch(x, wSelf, wRel, selfW, bias, edges, out, activation)
            End If

            Dim csr = If(edges Is Nothing, Nothing, _csrCache.GetBuffers(_engine, edges))
            Dim rowPtr As Object = If(csr Is Nothing, Nothing, csr.RowPtr)
            Dim colIdx As Object = If(csr Is Nothing, Nothing, csr.ColIdx)
            Dim coeff As Object = If(csr Is Nothing, Nothing, csr.Values)

            kernel.Launch(rows, 1, CellGraphLayerBlockSize, 1, 0,
                          Device64(x), Device64(wSelf),
                          If(wRel Is Nothing, Nothing, Device64(wRel)),
                          If(selfW Is Nothing, Nothing, Device64(selfW)),
                          If(bias Is Nothing, Nothing, Device64(bias)),
                          rowPtr, colIdx, coeff,
                          Device64(out),
                          nodes, inF, outF, activation)

            Return True
        End Function

        ''' <summary>
        ''' 节点特征拼装（GPU）：<c>[B·n, 2+2d] ← [x̄ ‖ p ‖ e_i ‖ z_pert]</c>。
        ''' </summary>
        ''' <remarks>
        ''' 纯数据搬移，一次内核完成；输入中的逐节点向量按行索引取，扰动向量按批索引取。
        ''' 该拼装如果没有内核支撑，在主机侧就是 <c>B·n·dims</c> 次带索引的写入
        ''' （实测占 GRN 前向可观的一部分），因此同样做成融合算子。
        ''' </remarks>
        Public Overrides Function GraphFeatureBatch(xNorm As tf.Tensor, flag As tf.Tensor, embed As tf.Tensor,
                                                    zPert As tf.Tensor, out As tf.Tensor,
                                                    n As Integer, d As Integer) As Boolean

            Dim kernel = TryKernel(TensorKernelNames.CellGraphFeature)

            If kernel Is Nothing OrElse xNorm Is Nothing OrElse flag Is Nothing OrElse
               embed Is Nothing OrElse zPert Is Nothing OrElse out Is Nothing Then
                Return MyBase.GraphFeatureBatch(xNorm, flag, embed, zPert, out, n, d)
            End If
            If n <= 0 OrElse d <= 0 OrElse embed.Length <> n * d Then
                Return MyBase.GraphFeatureBatch(xNorm, flag, embed, zPert, out, n, d)
            End If

            Dim rows As Integer = xNorm.Length
            Dim dims As Integer = 2 + 2 * d

            If rows <= 0 OrElse rows Mod n <> 0 OrElse out.Length <> rows * dims Then
                Return MyBase.GraphFeatureBatch(xNorm, flag, embed, zPert, out, n, d)
            End If
            If Not EnsureResident64(out, "cell.graph.features", zeroFill:=False) Then
                Return MyBase.GraphFeatureBatch(xNorm, flag, embed, zPert, out, n, d)
            End If

            Dim total As Integer = rows * dims
            Dim grid As Integer = (total + 255) \ 256

            kernel.Launch(grid, 1, 256, 1, 0,
                          Device64(xNorm), Device64(flag), Device64(embed), Device64(zPert),
                          Device64(out), rows, n, d)

            Return True
        End Function

        ''' <summary>
        ''' 通量读取头批量计算（GPU）：<c>v = e ⊙ gsat([h ‖ u]·Wv + bv)</c>。
        ''' </summary>
        ''' <remarks>
        ''' 输出必须落在常驻缓冲里（内核直接写显存），因此这里对 <paramref name="output"/>
        ''' 调用 <see cref="EnsureResident64"/>；调用方读主机数组之前需要
        ''' <see cref="SyncFromDevice"/>。
        ''' </remarks>
        Public Overrides Function FluxHeadBatch(h As tf.Tensor, u As tf.Tensor, wFlux As tf.Tensor,
                                                fluxBias As tf.Tensor, reversible As tf.Tensor,
                                                output As tf.Tensor) As Boolean

            Dim kernel = TryKernel(TensorKernelNames.CellFluxHead)

            If kernel Is Nothing OrElse h Is Nothing OrElse u Is Nothing OrElse wFlux Is Nothing OrElse output Is Nothing Then
                Return MyBase.FluxHeadBatch(h, u, wFlux, fluxBias, reversible, output)
            End If
            If h.Rank <> 2 OrElse u.Rank <> 2 OrElse output.Rank <> 2 OrElse wFlux.Rank <> 2 Then
                Return MyBase.FluxHeadBatch(h, u, wFlux, fluxBias, reversible, output)
            End If

            Dim batch As Integer = h.Shape(0)
            Dim m As Integer = h.Shape(1)
            Dim nIn As Integer = u.Shape(1)
            Dim r As Integer = output.Shape(1)

            If batch <= 0 OrElse m <= 0 OrElse nIn <= 0 OrElse r <= 0 Then
                Return MyBase.FluxHeadBatch(h, u, wFlux, fluxBias, reversible, output)
            End If
            If wFlux.Shape(0) <> m + nIn OrElse wFlux.Shape(1) <> r OrElse output.Shape(0) <> batch Then
                Return MyBase.FluxHeadBatch(h, u, wFlux, fluxBias, reversible, output)
            End If
            If Not EnsureResident64(output, "cell.flux", zeroFill:=False) Then
                Return MyBase.FluxHeadBatch(h, u, wFlux, fluxBias, reversible, output)
            End If

            kernel.Launch(batch, 1, 64, 1, 0,
                          Device64(h), Device64(u), Device64(wFlux),
                          If(fluxBias Is Nothing, Nothing, Device64(fluxBias)),
                          If(reversible Is Nothing, Nothing, Device64(reversible)),
                          Device64(output), m, nIn, r)

            Return True
        End Function

        ''' <summary>
        ''' 系统时间常数批量计算（GPU）：<c>τ^sys = 1/(1/τ_eff + σ(h·Wgate + u·WgateIn + bg))</c>。
        ''' </summary>
        Public Overrides Function SystemTauBatch(h As tf.Tensor, u As tf.Tensor, weightGate As tf.Tensor,
                                                 weightGateInput As tf.Tensor, biasGate As tf.Tensor,
                                                 tauEff As tf.Tensor, output As tf.Tensor) As Boolean

            Dim kernel = TryKernel(TensorKernelNames.CellSysTau)

            If kernel Is Nothing OrElse h Is Nothing OrElse u Is Nothing OrElse tauEff Is Nothing OrElse output Is Nothing Then
                Return MyBase.SystemTauBatch(h, u, weightGate, weightGateInput, biasGate, tauEff, output)
            End If
            If h.Rank <> 2 OrElse u.Rank <> 2 OrElse output.Rank <> 2 Then
                Return MyBase.SystemTauBatch(h, u, weightGate, weightGateInput, biasGate, tauEff, output)
            End If

            Dim batch As Integer = h.Shape(0)
            Dim m As Integer = h.Shape(1)
            Dim nIn As Integer = u.Shape(1)
            Dim hasGate As Boolean = weightGate IsNot Nothing AndAlso weightGateInput IsNot Nothing

            If batch <= 0 OrElse m <= 0 OrElse nIn <= 0 OrElse output.Length <> batch * m Then
                Return MyBase.SystemTauBatch(h, u, weightGate, weightGateInput, biasGate, tauEff, output)
            End If
            If Not EnsureResident64(output, "cell.systau", zeroFill:=False) Then
                Return MyBase.SystemTauBatch(h, u, weightGate, weightGateInput, biasGate, tauEff, output)
            End If

            kernel.Launch(batch, 1, 64, 1, 0,
                          Device64(h), Device64(u),
                          If(hasGate, Device64(weightGate), Nothing),
                          If(hasGate, Device64(weightGateInput), Nothing),
                          If(hasGate AndAlso biasGate IsNot Nothing, Device64(biasGate), Nothing),
                          Device64(tauEff), Device64(output),
                          m, nIn, If(hasGate, 1, 0))

            Return True
        End Function

#End Region

#Region "卷积与池化（手写内核：Kernels\conv.cu / Kernels\pool.cu）"

        ''' <summary>卷积/池化的输出边长: (input + 2*padding - kernel) / stride + 1</summary>
        Private Overloads Shared Function ConvOutSize(inputSize As Integer, kernelSize As Integer,
                                            stride As Integer, padding As Integer) As Integer
            Return (inputSize + 2 * padding - kernelSize) \ stride + 1
        End Function

        ''' <summary>
        ''' 形状的元素总数（以 <c>Long</c> 累乘后再收敛到 <c>Int32</c>）。
        ''' </summary>
        ''' <remarks>
        ''' 卷积 / 池化的输出元素数同样可能超过 Int32 —— 例如 <c>[batch, H, W, C]</c>
        ''' 在大输入下累乘就会溢出。用 Long 累乘可以让溢出被<b>显式检测</b>，
        ''' 而不是得到一个负数后传给 <c>DeviceBuffer</c> 抛出难以定位的异常。
        ''' </remarks>
        Private Shared Function ElementCount(shape As Integer()) As Integer
            Dim n As Long = 1

            For Each d As Integer In shape
                n *= d
            Next

            Call EnsureDeviceCount(n, $"形状 [{String.Join(",", shape)}] 的元素数")

            Return CInt(n)
        End Function

        ''' <summary>
        ''' 取一个手写内核；若该符号不在当前 NVRTC 编译单元之中则返回 Nothing，
        ''' 由调用方回退到 CPU 参考实现（与其它算子的回退策略一致）。
        ''' </summary>
        Private Function TryKernel(name As String) As ILCudaRuntime.CudaKernel
            Try
                Return _engine.GetKernel(name)
            Catch ex As Exception
                Return Nothing
            End Try
        End Function

        ''' <summary>偏置为 Nothing 时用零张量占位，使后续显存上传路径保持统一</summary>
        Private Shared Function ZeroBias(channels As Integer) As tf.Tensor
            Return tf.Tensor.Zeros(New Integer() {channels})
        End Function

        Public Overrides Function Conv2D(x As tf.Tensor, filters As tf.Tensor, bias As tf.Tensor,
                                         stride As Integer, padding As Integer) As tf.Tensor
            Dim kernel = TryKernel(TensorKernelNames.Conv2DForward)

            If Not OnGpu(x) OrElse kernel Is Nothing Then
                Return MyBase.Conv2D(x, filters, bias, stride, padding)
            End If

            Dim N = x.Shape(0), H = x.Shape(1), W = x.Shape(2), C = x.Shape(3)
            Dim KH = filters.Shape(0), KW = filters.Shape(1), OutC = filters.Shape(3)
            Dim OH = ConvOutSize(H, KH, stride, padding)
            Dim OW = ConvOutSize(W, KW, stride, padding)
            Dim total = N * OH * OW * OutC

            Dim dx = Device(x)
            Dim df = Device(filters)
            Dim db = Device(If(bias Is Nothing, ZeroBias(OutC), bias))

            Using dOut As New ILCudaRuntime.DeviceBuffer(Of Double)(total)
                kernel.Launch(ILCudaRuntime.LaunchPlanner.For1D(total, 256),
                              dx, df, db, dOut, N, H, W, C, KH, KW, OutC, OH, OW, stride, padding)

                Return Wrap(dOut.Read(), New Integer() {N, OH, OW, OutC})
            End Using
        End Function

        Public Overrides Function Conv2DBackwardInput(gradOutput As tf.Tensor, filters As tf.Tensor,
                                                      inputShape As Integer(), stride As Integer,
                                                      padding As Integer) As tf.Tensor
            Dim kernel = TryKernel(TensorKernelNames.Conv2DBackwardInput)

            If Not OnGpu(gradOutput) OrElse kernel Is Nothing Then
                Return MyBase.Conv2DBackwardInput(gradOutput, filters, inputShape, stride, padding)
            End If

            Dim N = inputShape(0), H = inputShape(1), W = inputShape(2), C = inputShape(3)
            Dim KH = filters.Shape(0), KW = filters.Shape(1), OutC = filters.Shape(3)
            Dim OH = gradOutput.Shape(1), OW = gradOutput.Shape(2)

            Dim dg = Device(gradOutput)
            Dim df = Device(filters)

            Using dOut As New ILCudaRuntime.DeviceBuffer(Of Double)(ElementCount(inputShape))
                ' 反向是散加(重叠窗口会命中同一个输入位置), 因此先清零再用 atomicAdd 累加
                dOut.Fill(0.0)

                kernel.Launch(ILCudaRuntime.LaunchPlanner.For1D(N * OH * OW * OutC, 256),
                              dg, df, dOut, N, H, W, C, KH, KW, OutC, OH, OW, stride, padding)

                Return Wrap(dOut.Read(), inputShape)
            End Using
        End Function

        Public Overrides Function Conv2DBackwardFilter(gradOutput As tf.Tensor, x As tf.Tensor,
                                                       filterShape As Integer(), stride As Integer,
                                                       padding As Integer) As tf.Tensor
            Dim kernel = TryKernel(TensorKernelNames.Conv2DBackwardFilter)

            If Not OnGpu(gradOutput) OrElse kernel Is Nothing Then
                Return MyBase.Conv2DBackwardFilter(gradOutput, x, filterShape, stride, padding)
            End If

            Dim N = x.Shape(0), H = x.Shape(1), W = x.Shape(2), C = x.Shape(3)
            Dim KH = filterShape(0), KW = filterShape(1), OutC = filterShape(3)
            Dim OH = gradOutput.Shape(1), OW = gradOutput.Shape(2)

            Dim dg = Device(gradOutput)
            Dim dx = Device(x)

            Using dOut As New ILCudaRuntime.DeviceBuffer(Of Double)(ElementCount(filterShape))
                dOut.Fill(0.0)

                kernel.Launch(ILCudaRuntime.LaunchPlanner.For1D(N * OH * OW * OutC, 256),
                              dg, dx, dOut, N, H, W, C, KH, KW, OutC, OH, OW, stride, padding)

                Return Wrap(dOut.Read(), filterShape)
            End Using
        End Function

        Public Overrides Function Conv2DBackwardBias(gradOutput As tf.Tensor) As tf.Tensor
            Dim kernel = TryKernel(TensorKernelNames.Conv2DBackwardBias)

            If Not OnGpu(gradOutput) OrElse kernel Is Nothing Then
                Return MyBase.Conv2DBackwardBias(gradOutput)
            End If

            Dim N = gradOutput.Shape(0), OH = gradOutput.Shape(1)
            Dim OW = gradOutput.Shape(2), OutC = gradOutput.Shape(3)

            Dim dg = Device(gradOutput)

            Using dOut As New ILCudaRuntime.DeviceBuffer(Of Double)(OutC)
                ' 一个线程负责一个通道, 独占写入, 无需清零也无需原子操作
                kernel.Launch(ILCudaRuntime.LaunchPlanner.For1D(OutC, 64), dg, dOut, N, OH, OW, OutC)

                Return Wrap(dOut.Read(), New Integer() {OutC})
            End Using
        End Function

        Public Overrides Function MaxPool2D(x As tf.Tensor, size As Integer, stride As Integer,
                                            padding As Integer, ByRef argMax As tf.Tensor) As tf.Tensor
            Dim kernel = TryKernel(TensorKernelNames.MaxPool2DForward)

            If Not OnGpu(x) OrElse kernel Is Nothing Then
                Return MyBase.MaxPool2D(x, size, stride, padding, argMax)
            End If

            Dim N = x.Shape(0), H = x.Shape(1), W = x.Shape(2), C = x.Shape(3)
            Dim OH = ConvOutSize(H, size, stride, padding)
            Dim OW = ConvOutSize(W, size, stride, padding)
            Dim total = N * OH * OW * C
            Dim shape As Integer() = {N, OH, OW, C}

            Dim dx = Device(x)

            Using dOut As New ILCudaRuntime.DeviceBuffer(Of Double)(total),
                  dIdx As New ILCudaRuntime.DeviceBuffer(Of Integer)(total)

                kernel.Launch(ILCudaRuntime.LaunchPlanner.For1D(total, 256),
                              dx, dOut, dIdx, N, H, W, C, size, OH, OW, stride, padding)

                ' argMax 以整数的扁平输入下标产出, 这里转成 double 承载 (与 Tensor 的载体类型一致)
                Dim hostIdx = dIdx.Read()
                Dim idxData(hostIdx.Length - 1) As Double

                For i As Integer = 0 To hostIdx.Length - 1
                    idxData(i) = CDbl(hostIdx(i))
                Next

                argMax = Wrap(idxData, shape)

                Return Wrap(dOut.Read(), shape)
            End Using
        End Function

        Public Overrides Function MaxPool2DBackward(gradOutput As tf.Tensor, argMax As tf.Tensor,
                                                    inputShape As Integer()) As tf.Tensor
            Dim kernel = TryKernel(TensorKernelNames.MaxPool2DBackward)

            If Not OnGpu(gradOutput) OrElse kernel Is Nothing Then
                Return MyBase.MaxPool2DBackward(gradOutput, argMax, inputShape)
            End If

            Dim srcIdx = argMax.Data
            Dim hostIdx(srcIdx.Length - 1) As Integer

            For i As Integer = 0 To srcIdx.Length - 1
                hostIdx(i) = CInt(srcIdx(i))
            Next

            Dim dg = Device(gradOutput)

            Using dIdx As New ILCudaRuntime.DeviceBuffer(Of Integer)(hostIdx.Length),
                  dOut As New ILCudaRuntime.DeviceBuffer(Of Double)(ElementCount(inputShape))

                Call dIdx.Write(hostIdx)
                dOut.Fill(0.0)

                kernel.Launch(ILCudaRuntime.LaunchPlanner.For1D(hostIdx.Length, 256),
                              dg, dIdx, dOut, CLng(hostIdx.Length))

                Return Wrap(dOut.Read(), inputShape)
            End Using
        End Function

#End Region

#Region "归约运算"

        Public Overrides Function SumAll(t As tf.Tensor) As Double
            If Not OnGpu(t) Then Return MyBase.SumAll(t)

            Return ReduceGlobal(t, TensorKernelNames.PartialSum, TensorKernelNames.FinalSum)
        End Function

        Public Overrides Function MeanAll(t As tf.Tensor) As Double
            If Not OnGpu(t) Then Return MyBase.MeanAll(t)

            Return SumAll(t) / t.Length
        End Function

        Public Overrides Function Sum(t As tf.Tensor, axis As Integer?, keepdims As Boolean) As tf.Tensor
            If Not axis.HasValue Then Return MyBase.Sum(t, axis, keepdims)

            Dim r = RowReduce(t, axis.Value, TensorKernelNames.RowSum, keepdims)
            If r IsNot Nothing Then Return r
            Return MyBase.Sum(t, axis, keepdims)
        End Function

        Public Overrides Function Mean(t As tf.Tensor, axis As Integer?, keepdims As Boolean) As tf.Tensor
            If Not axis.HasValue Then Return MyBase.Mean(t, axis, keepdims)

            Dim r = RowReduce(t, axis.Value, TensorKernelNames.RowMean, keepdims)
            If r IsNot Nothing Then Return r
            Return MyBase.Mean(t, axis, keepdims)
        End Function

        Public Overrides Function Max(t As tf.Tensor, axis As Integer?, keepdims As Boolean) As tf.Tensor
            If Not axis.HasValue Then
                If Not OnGpu(t) Then Return MyBase.Max(t, axis, keepdims)
                Return tf.Tensor.Scalar(ReduceGlobal(t, TensorKernelNames.PartialMax, TensorKernelNames.FinalMax))
            End If

            Dim r = RowReduce(t, axis.Value, TensorKernelNames.RowMax, keepdims)
            If r IsNot Nothing Then Return r
            Return MyBase.Max(t, axis, keepdims)
        End Function

        Public Overrides Function Min(t As tf.Tensor, axis As Integer?, keepdims As Boolean) As tf.Tensor
            If Not axis.HasValue Then
                If Not OnGpu(t) Then Return MyBase.Min(t, axis, keepdims)
                Return tf.Tensor.Scalar(ReduceGlobal(t, TensorKernelNames.PartialMin, TensorKernelNames.FinalMin))
            End If

            Dim r = RowReduce(t, axis.Value, TensorKernelNames.RowMin, keepdims)
            If r IsNot Nothing Then Return r
            Return MyBase.Min(t, axis, keepdims)
        End Function

        Public Overrides Function ArgMax(t As tf.Tensor, axis As Integer?) As tf.Tensor
            If Not axis.HasValue Then Return MyBase.ArgMax(t, axis)

            Dim r = RowReduce(t, axis.Value, TensorKernelNames.RowArgMax, keepdims:=False)
            If r IsNot Nothing Then Return r
            Return MyBase.ArgMax(t, axis)
        End Function

        Public Overrides Function ArgMin(t As tf.Tensor, axis As Integer?) As tf.Tensor
            If Not axis.HasValue Then Return MyBase.ArgMin(t, axis)

            Dim r = RowReduce(t, axis.Value, TensorKernelNames.RowArgMin, keepdims:=False)
            If r IsNot Nothing Then Return r
            Return MyBase.ArgMin(t, axis)
        End Function

#End Region

#Region "神经网络"

        Public Overrides Function Softmax(t As tf.Tensor, axis As Integer) As tf.Tensor
            Dim r = RowSoftmax(t, axis, TensorKernelNames.RowSoftmax)
            If r IsNot Nothing Then Return r
            Return MyBase.Softmax(t, axis)
        End Function

        Public Overrides Function LogSoftmax(t As tf.Tensor, axis As Integer) As tf.Tensor
            Dim r = RowSoftmax(t, axis, TensorKernelNames.RowLogSoftmax)
            If r IsNot Nothing Then Return r
            Return MyBase.LogSoftmax(t, axis)
        End Function

#End Region

#Region "训练内核（Kernels\train.cu）"

        ''' <summary>
        ''' GPU 端融合掩码交叉熵：<c>softmax + 负对数似然 + d(logits) = (softmax − onehot) / count</c>。
        ''' </summary>
        ''' <param name="logits">形状 <c>[rows, vocab]</c> 的二维 logits（主机张量）</param>
        ''' <param name="targets">长度 <c>&gt;= rows</c> 的目标 token；越界或负数视为"不计入"</param>
        ''' <param name="mask">
        ''' 长度 <c>&gt;= rows</c> 的 0/1 掩码；<c>Nothing</c> 表示全部计入。
        ''' 之所以不用 <c>Boolean()</c>：内核按 <c>int</c> 读取，显式用 <c>Integer()</c>
        ''' 可以避免 .NET 布尔数组的跨语言尺寸歧义。
        ''' </param>
        ''' <param name="dLogits">输出：形状同 <paramref name="logits"/> 的梯度</param>
        ''' <returns>内核不可用时返回 <c>False</c>，由调用方回退主机实现</returns>
        ''' <remarks>
        ''' 这是训练步里最重的一处主机循环：12.8 万词表下 <c>[rows, vocab]</c> 是
        ''' 3300 万次 <c>exp</c>，搬到 GPU 后由"每行一个 block + 共享内存树形归约"完成。
        ''' 
        ''' 只在计数完成后调用 <see cref="MaskedCrossEntropyFp32Core"/>。
        ''' 内核不可用时回退 <see cref="tfCompute.TensorComputeBase.MaskedCrossEntropy"/>，
        ''' 保证"任何后端切换都不会让某个算子失效"。
        ''' </remarks>
        Public Overrides Function MaskedCrossEntropy(logits As tf.Tensor,
                                                     targets As Integer(),
                                                     mask As Boolean(),
                                                     ByRef dLogits As tf.Tensor) As Double
            If logits Is Nothing OrElse logits.Rank <> 2 Then
                Return MyBase.MaskedCrossEntropy(logits, targets, mask, dLogits)
            End If

            Dim rows = logits.Shape(0)
            Dim vocab = logits.Shape(1)

            ' 主机侧只做一次廉价的计数：GPU 需要 1/count 才能把损失与梯度归一化
            Dim count As Integer = 0
            Dim maskInt(rows - 1) As Integer

            For r As Integer = 0 To rows - 1
                Dim valid As Boolean = True

                If mask IsNot Nothing AndAlso r < mask.Length AndAlso Not mask(r) Then valid = False
                If targets Is Nothing OrElse r >= targets.Length Then valid = False
                If valid AndAlso (targets(r) < 0 OrElse targets(r) >= vocab) Then valid = False

                maskInt(r) = If(valid, 1, 0)

                If valid Then count += 1
            Next

            If count = 0 Then
                dLogits = New tf.Tensor(logits.Shape)
                Call dLogits.MarkHostModified()

                Return 0.0
            End If

            Dim rowLoss As Single() = Nothing
            Dim grad As tf.Tensor = Nothing

            If Not MaskedCrossEntropyFp32Core(logits, targets, maskInt, 1.0 / count, grad, rowLoss) Then
                Return MyBase.MaskedCrossEntropy(logits, targets, mask, dLogits)
            End If

            dLogits = grad

            Dim total As Double = 0.0

            For r As Integer = 0 To rows - 1
                total += rowLoss(r)
            Next

            Return total
        End Function

        ''' <summary>
        ''' 融合掩码交叉熵内核的原始驱动。
        ''' </summary>
        ''' <param name="maskInt">已经判定好的 0/1 掩码（长度 <c>&gt;= rows</c>）</param>
        ''' <param name="inv"><c>1 / count</c>，count 由调用方统计</param>
        Private Function MaskedCrossEntropyFp32Core(logits As tf.Tensor,
                                                    targets As Integer(),
                                                    maskInt As Integer(),
                                                    inv As Double,
                                                    ByRef dLogits As tf.Tensor,
                                                    ByRef rowLoss As Single()) As Boolean
            dLogits = Nothing
            rowLoss = Nothing

            If logits Is Nothing OrElse logits.Rank <> 2 Then Return False

            Dim kernel = TryKernel(TensorKernelNames.TrainMaskedCrossEntropy)
            If kernel Is Nothing Then Return False

            Dim rows = logits.Shape(0)
            Dim vocab = logits.Shape(1)
            Dim elements As Long = CLng(rows) * vocab

            Call EnsureDeviceCount(elements, "掩码交叉熵的梯度")

            ' 目标是长度必须与行数匹配的 int 数组；不足时补 -1（内核视为"不计入"）
            Dim targetBuf(rows - 1) As Integer

            For i As Integer = 0 To rows - 1
                targetBuf(i) = If(targets IsNot Nothing AndAlso i < targets.Length, targets(i), -1)
            Next

            ' 掩码已经由调用方判定成 0/1，这里只做长度对齐。
            ' 仍然上传一份实体数组而不是传 Nothing：Nothing 会被 WriteArgument
            ' 写成空指针，内核侧就要多一次判空，不如在主机侧补齐长度。
            Dim useMask As Integer = 1
            Dim maskBuf(rows - 1) As Integer

            For i As Integer = 0 To rows - 1
                Dim flag As Integer = 1

                If maskInt IsNot Nothing AndAlso i < maskInt.Length Then
                    flag = If(maskInt(i) = 0, 0, 1)
                End If

                maskBuf(i) = flag
            Next

            Dim dTarget As ILCudaRuntime.DeviceBuffer(Of Integer) = Nothing
            Dim dMask As ILCudaRuntime.DeviceBuffer(Of Integer) = Nothing
            Dim dGrad As ILCudaRuntime.DeviceBuffer(Of Single) = Nothing
            Dim dLoss As ILCudaRuntime.DeviceBuffer(Of Single) = Nothing

            Try
                dGrad = New ILCudaRuntime.DeviceBuffer(Of Single)(CInt(elements))
                ' 未计入损失的行必须保持 0，因此先清零
                dGrad.Fill(0.0F)

                dLoss = New ILCudaRuntime.DeviceBuffer(Of Single)(rows)
                dTarget = New ILCudaRuntime.DeviceBuffer(Of Integer)(rows)
                dTarget.Write(targetBuf)

                dMask = New ILCudaRuntime.DeviceBuffer(Of Integer)(rows)
                dMask.Write(maskBuf)

                ' 每个 block 负责一行，块内 256 线程做共享内存树形归约（与 tensor.cu 的约定一致）
                Dim config As New ILCudaRuntime.LaunchConfig(rows, 1, RowBlockSize, 1, 0)

                kernel.Launch(config, DeviceF32(logits), dTarget, dMask, dGrad, dLoss,
                              rows, vocab, useMask, CSng(inv))

                ' 反向阶段仍需要一份主机侧的 dLogits（BatchedMatMulBackward 在主机上组装），
                ' 因此这里必须回读；回读量是 rows×vocab 个单精度数
                Dim gradResult = New tf.Tensor(rows, vocab)
                Dim hostGrad = dGrad.Read()

                Call Array.Copy(DeviceResidentStore.ToDouble(hostGrad), gradResult.Data, gradResult.Data.Length)
                Call gradResult.MarkHostModified()

                dLogits = gradResult
                rowLoss = dLoss.Read()

                Return True
            Finally
                If dLoss IsNot Nothing Then dLoss.Dispose()
                If dGrad IsNot Nothing Then dGrad.Dispose()
                If dMask IsNot Nothing Then dMask.Dispose()
                If dTarget IsNot Nothing Then dTarget.Dispose()
            End Try
        End Function

        ''' <summary>
        ''' GPU 端 AdamW 单步更新：原地更新参数与一阶/二阶矩，并在更新后清零梯度累加器。
        ''' </summary>
        ''' <remarks>
        ''' 四个张量都必须已被 <see cref="Resident"/> 钉住 —— 否则
        ''' <c>MarkHostModified</c> 会让下一次 <see cref="DeviceF32"/> 从主机重新上传，
        ''' 把内核刚写好的更新结果冲掉。钉住后 <see cref="DeviceF32"/> 直接命中常驻表、
        ''' 跳过版本校验，更新结果才得以保留。
        ''' </remarks>
        Public Overrides Function TryAdamWStep(param As tf.Tensor, gradient As tf.Tensor,
                                              momentum As tf.Tensor, velocity As tf.Tensor,
                                              learningRate As Double, beta1 As Double, beta2 As Double,
                                              eps As Double, biasCorrection1 As Double,
                                              biasCorrection2 As Double,
                                              weightDecay As Double) As Boolean
            If Not UseResidentStore Then Return False
            If param Is Nothing OrElse gradient Is Nothing OrElse momentum Is Nothing OrElse velocity Is Nothing Then
                Return False
            End If

            Dim kernel = TryKernel(TensorKernelNames.TrainAdamW)
            If kernel Is Nothing Then Return False

            Dim n = param.Length

            If n <= 0 Then Return False
            If gradient.Length <> n OrElse momentum.Length <> n OrElse velocity.Length <> n Then Return False

            Call EnsureDeviceCount(n, "AdamW 的参数")

            ' 参数与两个矩必须常驻：内核就地改写它们，若没钉住就会被随后的重新上传冲掉。
            ' 梯度则<b>刻意不要求</b>常驻 —— 它由主机侧的反向传播逐层累加产生，
            ' 每步都带着新的版本号，走 LRU 缓存重新上传正好是正确行为。
            ' 内核写进梯度缓冲的"清零"由调用方在主机侧用 ZeroGrad 同步。
            If Not _resident.IsPinned(param.Data) Then Return False
            If Not _resident.IsPinned(momentum.Data) Then Return False
            If Not _resident.IsPinned(velocity.Data) Then Return False

            kernel.Launch(ILCudaRuntime.LaunchPlanner.For1D(n, 256),
                          DeviceF32(param), DeviceF32(gradient),
                          DeviceF32(momentum), DeviceF32(velocity),
                          n,
                          CSng(learningRate), CSng(beta1), CSng(beta2), CSng(eps),
                          CSng(biasCorrection1), CSng(biasCorrection2), CSng(weightDecay))

            Return True
        End Function

        ''' <summary>
        ''' 梯度累加：<c>accum += alpha * src</c>（GPU，单精度）。
        ''' </summary>
        ''' <remarks>
        ''' 独立内核而非复用 <c>ewAxpyKernel</c>：后者把输入与输出都标了
        ''' <c>__restrict__</c>，在同一段缓冲上做累加会违反 restrict 契约。
        ''' </remarks>
        Public Function TryAccumulateFp32(accum As tf.Tensor, src As tf.Tensor,
                                          Optional alpha As Double = 1.0) As Boolean
            Dim kernel = TryKernel(TensorKernelNames.TrainAccumulate)
            If kernel Is Nothing Then Return False

            Dim n = accum.Length

            If n <= 0 OrElse src.Length <> n Then Return False

            Call EnsureDeviceCount(n, "梯度累加")

            kernel.Launch(ILCudaRuntime.LaunchPlanner.For1D(n, 256),
                          DeviceF32(accum), DeviceF32(src), CSng(alpha), n)

            Return True
        End Function

        ''' <summary>
        ''' CUDA 后端支持设备常驻缓冲。
        ''' </summary>
        Public Overrides ReadOnly Property SupportsDeviceResidency As Boolean
            Get
                Return True
            End Get
        End Property

        ''' <summary>
        ''' 把张量钉成设备常驻缓冲。
        ''' </summary>
        ''' <remarks>
        ''' 只应在"该张量的主机副本不再被任何主机循环读取"时使用 ——
        ''' 钉住后设备成为主副本，主机侧的 <c>Data</c> 会变陈旧。
        ''' 典型安全对象是只被 GEMM 消费的权重矩阵（<c>MatMul</c> 会通过
        ''' <see cref="DeviceF32"/> 直接命中常驻表）。
        ''' 反例：RMSNorm 的 γ（<c>RmsNorm</c> 在主机循环里读 <c>Gamma.Data</c>）、
        ''' 词嵌入（<c>LLMModel.Embed</c> 在主机上查表）都不该被钉住。
        ''' </remarks>
        Public Overrides Function PinDevice(t As tf.Tensor, label As String, zeroFill As Boolean) As Boolean
            If t Is Nothing Then Return False
            If t.Length <= 0 Then Return False

            Call _resident.Pin(_engine, t.Data, label, zeroFill)

            ' 打开常驻表开关后，DeviceF32 才会优先查询它
            UseResidentStore = True

            Return True
        End Function

        ''' <summary>
        ''' 把张量钉成<b>双精度</b>设备常驻缓冲（脉冲网络的递归状态专用）。
        ''' </summary>
        ''' <remarks>
        ''' 与 <see cref="PinDevice"/> 的区别只在于缓冲元素类型：这里是
        ''' <c>DeviceBuffer(Of Double)</c>，不经过任何降精度转换。
        ''' 典型用法（<see cref="LifStep"/> 的前提条件）：
        ''' <code>
        '''   backend.UseFp64Residency = True
        '''   backend.PinDevice64(h,      "lif.H",      zeroFill:=False)
        '''   backend.PinDevice64(sPrev,  "lif.S_prev", zeroFill:=False)
        '''   backend.PinDevice64(s,      "lif.S",      zeroFill:=False)
        '''   backend.PinDevice64(counts, "lif.counts", zeroFill:=True)
        ''' </code>
        ''' <para>
        ''' 注意双精度常驻缓冲<b>不参与</b> LRU 容量核算（与单精度通道一样独立计数），
        ''' 因此仿真结束后应调用 <see cref="UnpinDevice"/> 及时释放。
        ''' 打开本通道后 <see cref="TryAdamWStep"/> 仍只认单精度通道，
        ''' 若同时需要 AdamW 请对参数张量另行调用 <see cref="PinDevice"/>。
        ''' </para>
        ''' </remarks>
        Public Overrides Function PinDevice64(t As tf.Tensor, label As String, zeroFill As Boolean) As Boolean
            If t Is Nothing Then Return False
            If t.Length <= 0 Then Return False

            Call _resident64.Pin(_engine, t.Data, label, zeroFill)

            ' 打开常驻表开关后，Device64 才会优先查询它
            UseFp64Residency = True

            Return True
        End Function

        ''' <summary>解除钉住并立即释放显存（单精度 / 双精度两个通道一并解除）。</summary>
        ''' <remarks>
        ''' 同时释放该张量对应的融合 LIF 工作区：否则一次 <c>ResetState</c> 之后
        ''' 旧的 <c>I</c> 缓冲会一直挂着（键是主机数组引用，GC 无法回收显存）。
        ''' </remarks>
        Public Overrides Function UnpinDevice(t As tf.Tensor) As Boolean
            If t Is Nothing Then Return False

            Dim fp32 = _resident.Unpin(t.Data)
            Dim fp64 = _resident64.Unpin(t.Data)
            Dim workspace = Me.ReleaseLifWorkspace(t.Data)

            Return fp32 OrElse fp64 OrElse workspace
        End Function

        ''' <summary>该张量当前是否已被钉住（任一精度通道命中即视为已钉住）。</summary>
        Public Overrides Function IsDevicePinned(t As tf.Tensor) As Boolean
            If t Is Nothing Then Return False

            Return _resident.IsPinned(t.Data) OrElse _resident64.IsPinned(t.Data)
        End Function

        ''' <summary>当前钉住的显存总字节数（单精度 + 双精度两个通道之和）。</summary>
        Public Overrides ReadOnly Property PinnedDeviceBytes As Long
            Get
                Return _resident.TotalBytes + _resident64.TotalBytes
            End Get
        End Property

        ''' <summary>双精度常驻缓冲占用的字节数（诊断用）。</summary>
        Public ReadOnly Property PinnedDeviceBytes64 As Long
            Get
                Return _resident64.TotalBytes
            End Get
        End Property

        ''' <summary>
        ''' 把设备常驻缓冲的内容回写到主机数组。
        ''' </summary>
        ''' <remarks>
        ''' 回写之后主机版本号会递增，但<b>不会</b>触发重新上传 ——
        ''' <see cref="DeviceF32"/> 优先命中常驻表并跳过版本校验，
        ''' 因此设备仍然是主副本，刚回写的内容不会被覆盖。
        ''' </remarks>
        Public Overrides Function SyncFromDevice(t As tf.Tensor) As Boolean
            If t Is Nothing Then Return False

            ' 双精度通道优先：同一张量理论上只会在一个通道里被钉住，
            ' 但先查它可以让"混合档位"（脉冲双精度 + 膜电位单精度）的同步行为保持确定
            Dim host = _resident64.Download(t.Data)

            If host Is Nothing Then
                host = _resident.Download(t.Data)
            End If

            If host Is Nothing Then Return False
            If host.Length <> t.Data.Length Then Return False

            Call Array.Copy(host, t.Data, host.Length)
            Call t.MarkHostModified()

            Return True
        End Function

        ''' <summary>
        ''' 指定名称的内核当前是否可用。
        ''' </summary>
        ''' <remarks>
        ''' 内核不可用（NVRTC 编译失败 / 驱动不匹配）时相关算子会静默回退 CPU，
        ''' 调用方在"汇报是否真的用上了 GPU"之前应当先问一下这里，
        ''' 否则很容易把"算得慢"误报成"GPU 有效果"。
        ''' </remarks>
        Public Function IsKernelAvailable(kernelName As String) As Boolean
            Return TryKernel(kernelName) IsNot Nothing
        End Function

        ''' <summary>融合 LIF 单步内核（双精度档）是否可用。</summary>
        Public ReadOnly Property IsFusedLifAvailable As Boolean
            Get
                Return TryKernel(TensorKernelNames.LifUpdateDouble) IsNot Nothing
            End Get
        End Property

        ''' <summary>稀疏 SpMM 内核是否可用。</summary>
        Public ReadOnly Property IsSparseSpmmAvailable As Boolean
            Get
                Return TryKernel(TensorKernelNames.SpmmCsr) IsNot Nothing
            End Get
        End Property

        ''' <summary>
        ''' 关键内核的可用性快照，用于诊断"训练步到底有没有走上 GPU"。
        ''' </summary>
        ''' <remarks>
        ''' 内核不可用（NVRTC 编译失败 / 驱动不匹配）时全部算子都会<b>静默回退 CPU</b>。
        ''' 如果没有这份诊断，性能不符预期时很难分清"算得慢"和"根本没上 GPU"。
        ''' </remarks>
        Public Function DescribeKernels() As String
            ' 注意：循环变量不能叫 name —— VB 标识符大小写不敏感，
            ' 会与基类的 Name 属性（ITensorCompute.Name）冲突
            Dim kernelNames = {
                ILCudaRuntime.KernelNames.Gemm,
                TensorKernelNames.TrainTranspose,
                TensorKernelNames.TrainMaskedCrossEntropy,
                TensorKernelNames.TrainAdamW,
                TensorKernelNames.TrainAccumulate,
                TensorKernelNames.SpmmCsr,
                TensorKernelNames.LifUpdateDouble,
                TensorKernelNames.LifUpdateFp32,
                TensorKernelNames.CellLtcRk4,
                TensorKernelNames.CellGraphLayer,
                TensorKernelNames.CellGraphFeature
            }

            Dim sb As New System.Text.StringBuilder()

            For Each kernelName In kernelNames
                Dim ok As Boolean = TryKernel(kernelName) IsNot Nothing

                Call sb.Append(kernelName).Append("=").Append(If(ok, "OK", "缺失")).Append("  ")
            Next

            Dim failures = DoubleKernelRegistry.Failures

            If failures.Count > 0 Then
                Call sb.Append("| IL2Cuda 失败: ")

                For Each pair In failures
                    Call sb.Append(pair.Key).Append("(").Append(pair.Value).Append(") ")
                Next
            End If

            Return sb.ToString().TrimEnd()
        End Function

        ''' <summary>
        ''' 设备与显存概览（名称 / 计算能力 / 显存总量与可用量），供报告使用。
        ''' </summary>
        Public Function DescribeDevice() As String
            Dim device = _engine.Device
            Dim info = _engine.GetMemoryInfo()

            Return $"{device.Name} (CC {device.ComputeCapability}, {device.MultiprocessorCount} SM)  " &
                   $"显存 {info.TotalMB:N0} MB 总量 / {info.FreeMB:N0} MB 可用 / " &
                   $"已用 {info.UsedMB:N0} MB ({info.UsedRatio:P1})"
        End Function

#End Region

#Region "资源释放"

        Public Sub Dispose() Implements IDisposable.Dispose
            ' 常驻缓冲由本类显式持有，必须先于引擎释放：
            ' ILCuda 的显存没有终结器，漏掉这一步会让 cuMemAlloc 出来的显存永不归还
            _resident.Dispose()
            _resident64.Dispose()

            ' 融合 LIF 的跨步工作区同样持有显存
            SyncLock _lifSync
                For Each workspace In _lifWorkspaces.Values
                    workspace.Dispose()
                Next

                _lifWorkspaces.Clear()
            End SyncLock

            _csrCache.Dispose()
            _fp32.Dispose()
            _cache.Dispose()
            _engine.Dispose()
        End Sub

#End Region

    End Class

End Namespace
