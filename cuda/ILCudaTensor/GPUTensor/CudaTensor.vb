' ---------------------------------------------------------------------------
' CudaTensor —— 基于 ILCuda 的 GPU 计算后端
'
' 它继承 TensorFlow 提供的标量兜底实现 TensorComputeBase，
' 因此只需要重写“ILCuda 有现成内核”的算子；其余算子（转置、按轴归约、
' softmax、比较等）自动继承 CPU 实现，保证任何后端切换都不会让某个算子失效。
'
' P1 精度策略：Double -> Single 上传计算 -> 回读升回 Double（float 快速通道）。
' 所有显存缓冲都由 DeviceCache 显式管理，避免 ILCuda 无终结器导致的显存泄漏。
'
' 用法：
'     If CudaTensor.Register() Then
'         ' 之后所有 Tensor / Math / nn 的运算自动走 GPU
'     End If
' ---------------------------------------------------------------------------

Imports ILCudaRuntime = Microsoft.VisualBasic.Computing.ILCuda.Runtime
Imports ILCudaMath = Microsoft.VisualBasic.Computing.ILCuda.Math
Imports tf = Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports tfCompute = Microsoft.VisualBasic.MachineLearning.TensorFlow.Compute

Namespace GPUTensor

    ''' <summary>
    ''' <see cref="tf.Compute.ITensorCompute"/> 的 CUDA GPU 实现。
    ''' </summary>
    Public Class CudaTensor
        Inherits tfCompute.TensorComputeBase
        Implements IDisposable

        ''' <summary>默认的显存驻留缓存上限（1 GiB）</summary>
        Public Const DefaultCacheBytes As Long = 1024L * 1024L * 1024L

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
        Private ReadOnly _cache As DeviceCache

        Public Sub New(engine As ILCudaRuntime.CudaEngine, Optional cacheBytes As Long = DefaultCacheBytes)
            If engine Is Nothing Then Throw New ArgumentNullException(NameOf(engine))

            _engine = engine
            _cache = New DeviceCache(cacheBytes)
        End Sub

        Public Overrides ReadOnly Property Name As String = "CUDA"

        ''' <summary>底层 CUDA 引擎</summary>
        Public ReadOnly Property Engine As ILCudaRuntime.CudaEngine
            Get
                Return _engine
            End Get
        End Property

#Region "注册与切换"

        ''' <summary>
        ''' 尝试初始化 CUDA 并把 <see cref="tf.Tensor.computeKernel"/> 切换为 GPU 后端。
        ''' 设备不可用 / NVRTC 与驱动版本不匹配时返回 False，并且**不改变**当前后端。
        ''' </summary>
        Public Shared Function Register(Optional options As ILCudaRuntime.EngineOptions = Nothing) As Boolean
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

        ''' <summary>取得张量底层数据对应的显存缓冲（带驻留缓存）</summary>
        Private Function Device(t As tf.Tensor) As ILCudaRuntime.DeviceBuffer(Of Single)
            Return _cache.GetBuffer(_engine, t.Data, t.Version)
        End Function

        Private Function BinaryGpu(a As tf.Tensor, b As tf.Tensor,
                                   op As Action(Of ILCudaRuntime.DeviceBuffer(Of Single),
                                                    ILCudaRuntime.DeviceBuffer(Of Single),
                                                    ILCudaRuntime.DeviceBuffer(Of Single))) As tf.Tensor
            Dim da = Device(a)
            Dim db = Device(b)

            Using dOut As New ILCudaRuntime.DeviceBuffer(Of Single)(a.Length)
                op(da, db, dOut)
                Return Wrap(FloatBridge.ToDouble(dOut.Read()), a.Shape)
            End Using
        End Function

        Private Function UnaryGpu(t As tf.Tensor,
                                  op As Action(Of ILCudaRuntime.DeviceBuffer(Of Single),
                                                   ILCudaRuntime.DeviceBuffer(Of Single))) As tf.Tensor
            Dim dx = Device(t)

            Using dOut As New ILCudaRuntime.DeviceBuffer(Of Single)(t.Length)
                op(dx, dOut)
                Return Wrap(FloatBridge.ToDouble(dOut.Read()), t.Shape)
            End Using
        End Function

        Private Function ReduceGpu(t As tf.Tensor, op As Func(Of ILCudaRuntime.CudaEngine,
                                                                  ILCudaRuntime.DeviceBuffer(Of Single),
                                                                  Single)) As Double
            Return CDbl(op(_engine, Device(t)))
        End Function

#End Region

#Region "逐元素 - 二元"

        Public Overrides Function Add(a As tf.Tensor, b As tf.Tensor) As tf.Tensor
            RequireSameShape(a, b, "相加")
            If Not OnGpu(a) Then Return MyBase.Add(a, b)

            Return BinaryGpu(a, b, Sub(x, y, o) ILCudaMath.GpuElementwise.Add(_engine, x, y, o))
        End Function

        Public Overrides Function Subtract(a As tf.Tensor, b As tf.Tensor) As tf.Tensor
            RequireSameShape(a, b, "相减")
            If Not OnGpu(a) Then Return MyBase.Subtract(a, b)

            Return BinaryGpu(a, b, Sub(x, y, o) ILCudaMath.GpuElementwise.Subtract(_engine, x, y, o))
        End Function

        Public Overrides Function Multiply(a As tf.Tensor, b As tf.Tensor) As tf.Tensor
            RequireSameShape(a, b, "相乘")
            If Not OnGpu(a) Then Return MyBase.Multiply(a, b)

            Return BinaryGpu(a, b, Sub(x, y, o) ILCudaMath.GpuElementwise.Multiply(_engine, x, y, o))
        End Function

        Public Overrides Function Divide(a As tf.Tensor, b As tf.Tensor) As tf.Tensor
            RequireSameShape(a, b, "相除")
            If Not OnGpu(a) Then Return MyBase.Divide(a, b)

            Return BinaryGpu(a, b, Sub(x, y, o) ILCudaMath.GpuElementwise.Divide(_engine, x, y, o))
        End Function

#End Region

#Region "逐元素 - 一元"

        Public Overrides Function Exp(t As tf.Tensor) As tf.Tensor
            If Not OnGpu(t) Then Return MyBase.Exp(t)

            Return UnaryGpu(t, Sub(x, o) ILCudaMath.GpuElementwise.Exp(_engine, x, o))
        End Function

        Public Overrides Function Log(t As tf.Tensor) As tf.Tensor
            If Not OnGpu(t) Then Return MyBase.Log(t)

            Return UnaryGpu(t, Sub(x, o) ILCudaMath.GpuElementwise.Log(_engine, x, o))
        End Function

        Public Overrides Function Abs(t As tf.Tensor) As tf.Tensor
            If Not OnGpu(t) Then Return MyBase.Abs(t)

            Return UnaryGpu(t, Sub(x, o) ILCudaMath.GpuElementwise.Abs(_engine, x, o))
        End Function

        Public Overrides Function Relu(t As tf.Tensor) As tf.Tensor
            If Not OnGpu(t) Then Return MyBase.Relu(t)

            Return UnaryGpu(t, Sub(x, o) ILCudaMath.GpuElementwise.Relu(_engine, x, o))
        End Function

#End Region

#Region "标量运算"

        Public Overrides Function MultiplyScalar(t As tf.Tensor, scalar As Double) As tf.Tensor
            If Not OnGpu(t) Then Return MyBase.MultiplyScalar(t, scalar)

            Dim dx = Device(t)

            Using dOut As New ILCudaRuntime.DeviceBuffer(Of Single)(t.Length)
                ILCudaMath.GpuElementwise.Scale(_engine, dx, CSng(scalar), dOut)
                Return Wrap(FloatBridge.ToDouble(dOut.Read()), t.Shape)
            End Using
        End Function

        Public Overrides Function DivideScalar(t As tf.Tensor, scalar As Double) As tf.Tensor
            If Not OnGpu(t) Then Return MyBase.DivideScalar(t, scalar)

            Dim dx = Device(t)

            Using dOut As New ILCudaRuntime.DeviceBuffer(Of Single)(t.Length)
                ILCudaMath.GpuElementwise.Scale(_engine, dx, CSng(1.0 / scalar), dOut)
                Return Wrap(FloatBridge.ToDouble(dOut.Read()), t.Shape)
            End Using
        End Function

#End Region

#Region "矩阵运算"

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

            Using dC As New ILCudaRuntime.DeviceBuffer(Of Single)(m * n)
                ILCudaMath.GpuBlas.Gemm(_engine, da, db, dC, m, n, k)
                Return Wrap(FloatBridge.ToDouble(dC.Read()), New Integer() {m, n})
            End Using
        End Function

#End Region

#Region "归约运算"

        Public Overrides Function SumAll(t As tf.Tensor) As Double
            If Not OnGpu(t) Then Return MyBase.SumAll(t)

            Return ReduceGpu(t, Function(e, x) ILCudaMath.GpuReduce.Sum(e, x))
        End Function

        Public Overrides Function MeanAll(t As tf.Tensor) As Double
            If Not OnGpu(t) Then Return MyBase.MeanAll(t)

            Return SumAll(t) / t.Length
        End Function

        Public Overrides Function Max(t As tf.Tensor, axis As Integer?) As tf.Tensor
            If axis.HasValue OrElse Not OnGpu(t) Then Return MyBase.Max(t, axis)

            Return tf.Tensor.Scalar(ReduceGpu(t, Function(e, x) ILCudaMath.GpuReduce.Max(e, x)))
        End Function

        Public Overrides Function Min(t As tf.Tensor, axis As Integer?) As tf.Tensor
            If axis.HasValue OrElse Not OnGpu(t) Then Return MyBase.Min(t, axis)

            Return tf.Tensor.Scalar(ReduceGpu(t, Function(e, x) ILCudaMath.GpuReduce.Min(e, x)))
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
