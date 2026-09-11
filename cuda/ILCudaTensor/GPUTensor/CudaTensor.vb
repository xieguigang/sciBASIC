' ---------------------------------------------------------------------------
' CudaTensor —— 基于 ILCuda 的 GPU 计算后端（全 double 精度）
'
' 它继承 TensorFlow 提供的标量兜底实现 TensorComputeBase，
' 因此只需要重写“GPU 有对应内核”的算子；其余算子（比较、乘积归约、Apply 等）
' 自动继承 CPU 实现，保证任何后端切换都不会让某个算子失效。
'
' GPU 侧的算子全部是 **double**，不再有 Double<->Single 的降精度桥接：
'   * 逐元素运算 / 转置          -> P2 IL2Cuda 生成内核（DoubleKernels.vb）
'   * 末轴 softmax / 归约 / arg  -> P3 手写内核（tensor.cu）
'   * 矩阵乘                     -> 手写分块 GEMM（gemm.cu）
'   * 全局归约                   -> 复用末轴归约内核（整张量当作一行）
'
' 所有显存缓冲都由 DeviceCache 显式管理，避免 ILCuda 无终结器导致的显存泄漏。
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

        ''' <summary>低于该元素数时走 CPU 兜底，避免显存拷贝开销倒挂</summary>
        Public Shared Property MinGpuElements As Integer = 4096

        ''' <summary>矩阵乘走到 GPU 的最小规模（m * k * n）</summary>
        Public Shared Property MinGemmElements As Integer = 65536

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

        Public Sub New(engine As ILCudaRuntime.CudaEngine, Optional cacheBytes As Long = DefaultCacheBytes)
            If engine Is Nothing Then Throw New ArgumentNullException(NameOf(engine))

            _engine = engine
            _cache = New DeviceCache(Of Double)(cacheBytes)
        End Sub

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
        Public Shared Function Register(Optional options As ILCudaRuntime.EngineOptions = Nothing) As Boolean
            ' 必须在 CudaEngine.TryCreate 之前把内核源码注入编译单元
            DoubleKernelRegistry.EnsureRegistered()
            ILCudaKernels.KernelSources.Register(GetType(CudaTensor).Assembly)

            If options Is Nothing Then options = New ILCudaRuntime.EngineOptions()

            Dim engine = ILCudaRuntime.CudaEngine.TryCreate(options)

            If engine Is Nothing Then
                LastError = options.ErrorMessage
                Return False
            End If

            Dim backend As New CudaTensor(engine)

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

        ''' <summary>取张量底层数据对应的显存缓冲（驻留缓存，零精度损失）</summary>
        Private Function Device(t As tf.Tensor) As ILCudaRuntime.DeviceBuffer(Of Double)
            Return _cache.GetBuffer(_engine, t.Data, t.Version, Function(d) d)
        End Function

        ''' <summary>逐元素二元（纯标量内核：两个输入数组）</summary>
        Private Function EwBinary(a As tf.Tensor, b As tf.Tensor, kernelName As String) As tf.Tensor
            If Not OnGpu(a) OrElse Not DoubleKernelRegistry.Available(kernelName) Then Return Nothing

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
            If Not OnGpu(t) OrElse Not DoubleKernelRegistry.Available(kernelName) Then Return Nothing

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

        ''' <summary>全局归约：把整张量当作一行复用末轴归约内核</summary>
        Private Function ReduceAll(t As tf.Tensor, kernelName As String) As Double
            Dim dx = Device(t)

            Using dOut As New ILCudaRuntime.DeviceBuffer(Of Double)(1)
                LaunchRow(kernelName, dx, dOut, 1, t.Length)
                Return dOut.Read()(0)
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
        ''' 双精度分块矩阵乘（gemm.cu）。
        ''' 输出 (m, n) 的第 (i, j) 个元素 = Σ A(i, p) * B(p, j)。
        ''' </summary>
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

            If m * k * n < MinGemmElements Then Return MyBase.MatMul(a, b)

            Dim da = Device(a)
            Dim db = Device(b)

            Using dC As New ILCudaRuntime.DeviceBuffer(Of Double)(m * n)
                _engine.GetKernel(TensorKernelNames.GemmDouble).Launch(
                    ILCudaRuntime.LaunchPlanner.For2D(m, n, 16, 16),
                    da, db, dC, m, n, k)

                Return Wrap(dC.Read(), New Integer() {m, n})
            End Using
        End Function

        ''' <summary>
        ''' 二维转置：由 IL2Cuda 生成的 Grid2D double 内核完成，
        ''' 输出 (C, R) 的第 (i, j) 个元素 = 输入 (R, C) 的第 (j, i) 个元素。
        ''' </summary>
        Public Overrides Function Transpose(t As tf.Tensor) As tf.Tensor
            If t.Rank <> 2 Then
                Throw New ArgumentException("只支持二维张量转置")
            End If

            If Not OnGpu(t) OrElse Not DoubleKernelRegistry.Available(DoubleKernelRegistry.Transpose) Then
                Return MyBase.Transpose(t)
            End If

            Dim rows = t.Shape(0)
            Dim cols = t.Shape(1)

            Dim dx = Device(t)

            ' 输出形状为 (cols, rows)
            Using dOut As New ILCudaRuntime.DeviceBuffer(Of Double)(rows * cols)
                _engine.GetKernel(DoubleKernelRegistry.Transpose).Launch(
                    ILCudaRuntime.LaunchPlanner.For2D(cols, rows, 16, 16),
                    dx, cols, dOut, cols, rows)

                Return Wrap(dOut.Read(), New Integer() {cols, rows})
            End Using
        End Function

#End Region

#Region "归约运算"

        Public Overrides Function SumAll(t As tf.Tensor) As Double
            If Not OnGpu(t) Then Return MyBase.SumAll(t)

            Return ReduceAll(t, TensorKernelNames.RowSum)
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
                Return tf.Tensor.Scalar(ReduceAll(t, TensorKernelNames.RowMax))
            End If

            Dim r = RowReduce(t, axis.Value, TensorKernelNames.RowMax, keepdims)
            If r IsNot Nothing Then Return r
            Return MyBase.Max(t, axis, keepdims)
        End Function

        Public Overrides Function Min(t As tf.Tensor, axis As Integer?, keepdims As Boolean) As tf.Tensor
            If Not axis.HasValue Then
                If Not OnGpu(t) Then Return MyBase.Min(t, axis, keepdims)
                Return tf.Tensor.Scalar(ReduceAll(t, TensorKernelNames.RowMin))
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

#Region "资源释放"

        Public Sub Dispose() Implements IDisposable.Dispose
            _cache.Dispose()
            _engine.Dispose()
        End Sub

#End Region

    End Class

End Namespace
