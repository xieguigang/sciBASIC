' ---------------------------------------------------------------------------
' 光栅化管线共享类型
'
' 数据布局：所有逐高斯量都是长度 N 的 SoA 数组（Double），
' 逐高斯合成阶段需要的一切都预先算好，像素内循环不再触碰模型与张量对象。
' ---------------------------------------------------------------------------

Imports Microsoft.VisualBasic.MachineLearning.TensorFlow

''' <summary>
''' 投影阶段的数值精度策略。
    ''' </summary>
    ''' <remarks>
    ''' 投影是 O(N) 的逐高斯稠密运算（每个高斯约 200 flops），比像素合成低 3 个数量级，
    ''' 实测在 N=2e5 时仍只占单帧耗时的 3% 左右；因此这里只保留双精度内核
    ''' （它同时也是一致性对拍基准）。真正交给 fp32 张量后端的是图像空间环节
    ''' （见 <see cref="SplatRenderOptions.UseTensorLossBackend"/>），那里的数据量是
    ''' H·W·3 级别的纯逐元素运算，最能摊平 GPU 的调度与传输开销。
    ''' </remarks>
    Public Enum SplatPrecision

        ''' <summary>自动（当前等价于 <see cref="Fp64"/>）。</summary>
        Auto = 0
        ''' <summary>双精度标量/向量内核（始终可用，作为对拍基准）。</summary>
        Fp64 = 1

    End Enum

    ''' <summary>指数函数实现方式。</summary>
    Public Enum ExpMode

        ''' <summary>精确：直接调用 <c>System.Math.Exp</c>。</summary>
        Exact = 0
        ''' <summary>快速：2^k 分解 + FMA 多项式，相对误差约 1e-9（双精度）。</summary>
        Fast = 1

    End Enum

    ''' <summary>
    ''' 光栅化 / 训练的可调参数。
    ''' </summary>
    Public Class SplatRenderOptions

        ''' <summary>默认参数（进程级共享；修改会全局生效）。</summary>
        Public Shared Property [Default] As SplatRenderOptions = New SplatRenderOptions()

        ''' <summary>tile 边长（像素）。16 是 3DGS 的经典取值，兼顾局部缓冲与负载均衡。</summary>
        Public Property TileSize As Integer = 16

        ''' <summary>参与合成的半径（标准差倍数）。</summary>
        Public Property RadiusSigma As Double = 3.0

        ''' <summary>不透明度低于该值的高斯直接剔除。</summary>
        Public Property MinOpacity As Double = 0.005

        ''' <summary>相机空间深度下限（小于该值视为在高斯相机后方）。</summary>
        Public Property MinDepth As Double = 0.1

        ''' <summary>加到 2D 协方差对角线上的抗锯齿项。</summary>
        Public Property Cov2DEpsilon As Double = 0.3 * 0.3

        ''' <summary>单像素 alpha 上限（避免透射率被一步打到 0）。</summary>
        Public Property AlphaMax As Double = 0.99

        ''' <summary>指数函数实现方式。</summary>
        Public Property ExpMode As ExpMode = ExpMode.Fast

        ''' <summary>投影精度策略。</summary>
        Public Property Precision As SplatPrecision = SplatPrecision.Auto

        ''' <summary>
        ''' 图像空间（损失 / 像素梯度）是否允许经张量后端的精度链计算。
        '''
        ''' 这部分数据量是 H·W·3（数十万元素），是整条管线里唯一"纯逐元素、
        ''' 数据量足够大、且只需 3~4 个算子"的环节 —— 宿主注册了 CUDA 后端时
        ''' 由 <c>Tensor.computeKernel</c> / <c>TensorF.computeKernelF</c> 调到显卡，
        ''' 未注册时等价于 SIMD CPU 内核，因此默认开启没有副作用。
        ''' </summary>
        Public Property UseTensorLossBackend As Boolean = True

        ''' <summary>
        ''' 走张量后端做图像空间损失所需的最小元素数（默认 200 万）。
        '''
        ''' 实测（RTX A4000 / CUDA-F32）：320×240 的图像（23 万元素）走 GPU 时
        ''' 单次 MSE 从 0.07 ms 退化到 1.1 ms —— 显存往返（1.8 MB fp64 + 0.9 MB fp32）
        ''' 完全压倒了内核时间。只有图像大到 200 万元素以上（约 1000×700×3）时
        ''' 传输才摊得平，因此默认阈值取得比较保守；把它调小可以用于诊断。
        ''' </summary>
        Public Property TensorLossMinElements As Integer = 2000000

        ''' <summary>并行度（0 表示使用 <see cref="Environment.ProcessorCount"/>）。</summary>
        Public Property MaxDegreeOfParallelism As Integer = 0

        ''' <summary>tile 数低于该值时不做并行（避免调度开销倒挂）。</summary>
        Public Property ParallelMinTiles As Integer = 8

        ''' <summary>反向传播线程本地梯度缓冲的总内存上限（MB）。超限时逐 tile 串行归约。</summary>
        Public Property ThreadGradientBudgetMB As Integer = 512

        Public Function Clone() As SplatRenderOptions
            Return New SplatRenderOptions With {
                .TileSize = TileSize,
                .RadiusSigma = RadiusSigma,
                .MinOpacity = MinOpacity,
                .MinDepth = MinDepth,
                .Cov2DEpsilon = Cov2DEpsilon,
                .AlphaMax = AlphaMax,
                .ExpMode = ExpMode,
                .Precision = Precision,
                .UseTensorLossBackend = UseTensorLossBackend,
                .TensorLossMinElements = TensorLossMinElements,
                .MaxDegreeOfParallelism = MaxDegreeOfParallelism,
                .ParallelMinTiles = ParallelMinTiles,
                .ThreadGradientBudgetMB = ThreadGradientBudgetMB
            }
        End Function

        Friend Function ParallelOptions() As System.Threading.Tasks.ParallelOptions
            Dim degree = If(MaxDegreeOfParallelism > 0, MaxDegreeOfParallelism, Environment.ProcessorCount)

            Return New System.Threading.Tasks.ParallelOptions With {
                .MaxDegreeOfParallelism = degree
            }
        End Function

    End Class

    ''' <summary>
    ''' 逐高斯的屏幕空间数据（SoA）。
    '''
    ''' 索引始终是 **模型中的高斯下标**（长度 N），可见性由 <see cref="Visible"/> 标记，
    ''' 紧凑的可见列表见 <see cref="SplatRasterRequest.Order"/>。
    ''' </summary>
    Public Class SplatScreenData

        Public Property Count As Integer

        Public Property Width As Integer
        Public Property Height As Integer

        ''' <summary>屏幕坐标（像素）</summary>
        Public ReadOnly MeanX As Double()
        Public ReadOnly MeanY As Double()
        ''' <summary>相机空间坐标</summary>
        Public ReadOnly CamX As Double()
        Public ReadOnly CamY As Double()
        Public ReadOnly CamZ As Double()

        ''' <summary>2D 协方差的逆（conic）：a = c00, b = c01, c = c11</summary>
        Public ReadOnly ConicA As Double()
        Public ReadOnly ConicB As Double()
        Public ReadOnly ConicC As Double()

        ''' <summary>相机空间 3D 协方差（6 个独立分量，供反向传播使用）</summary>
        Public ReadOnly Sigma00 As Double(), Sigma01 As Double(), Sigma02 As Double()
        Public ReadOnly Sigma11 As Double(), Sigma12 As Double(), Sigma22 As Double()

        ''' <summary>sigmoid 之后的不透明度</summary>
        Public ReadOnly Opacity As Double()
        ''' <summary>logit 空间的不透明度（用于 sigmoid 导数）</summary>
        Public ReadOnly RawOpacity As Double()

        Public ReadOnly ColorR As Double()
        Public ReadOnly ColorG As Double()
        Public ReadOnly ColorB As Double()

        ''' <summary>屏幕半径（像素）</summary>
        Public ReadOnly Radius As Double()

        Public ReadOnly Visible As Boolean()

        ''' <summary>已经与屏幕求交后的像素包围盒（含端点）</summary>
        Public ReadOnly RectX0 As Integer(), RectX1 As Integer()
        Public ReadOnly RectY0 As Integer(), RectY1 As Integer()

        ''' <summary>本帧可见高斯数量</summary>
        Public Property VisibleCount As Integer

        ' 相机常量（反向传播需要）
        Public Property Fx As Double
        Public Property Fy As Double
        Public Property Cx As Double
        Public Property Cy As Double

        ''' <summary>世界→相机的旋转矩阵（行优先 9 元素）</summary>
        Public Property Rcam As Double()

        Public Sub New(n As Integer)
            Me.Count = n
            MeanX = New Double(n - 1) {}
            MeanY = New Double(n - 1) {}
            CamX = New Double(n - 1) {}
            CamY = New Double(n - 1) {}
            CamZ = New Double(n - 1) {}
            ConicA = New Double(n - 1) {}
            ConicB = New Double(n - 1) {}
            ConicC = New Double(n - 1) {}
            Sigma00 = New Double(n - 1) {}
            Sigma01 = New Double(n - 1) {}
            Sigma02 = New Double(n - 1) {}
            Sigma11 = New Double(n - 1) {}
            Sigma12 = New Double(n - 1) {}
            Sigma22 = New Double(n - 1) {}
            Opacity = New Double(n - 1) {}
            RawOpacity = New Double(n - 1) {}
            ColorR = New Double(n - 1) {}
            ColorG = New Double(n - 1) {}
            ColorB = New Double(n - 1) {}
            Radius = New Double(n - 1) {}
            Visible = New Boolean(n - 1) {}
            RectX0 = New Integer(n - 1) {}
            RectX1 = New Integer(n - 1) {}
            RectY0 = New Integer(n - 1) {}
            RectY1 = New Integer(n - 1) {}
        End Sub

    End Class

    ''' <summary>
    ''' 一次光栅化请求：投影 / 剔除 / 排序 / 分箱的产物。
    ''' 后端只负责"按 tile 做像素合成"，不接触模型对象。
    ''' </summary>
    Public NotInheritable Class SplatRasterRequest

        Public Property Width As Integer
        Public Property Height As Integer
        Public Property TileSize As Integer
        Public Property TilesX As Integer
        Public Property TilesY As Integer

        Public Property Screen As SplatScreenData

        ''' <summary>本次光栅化使用的参数（后端消费）。</summary>
        Public Property Options As SplatRenderOptions

        ''' <summary>可见高斯的模型下标，按深度 near → far 排序</summary>
        Public Property Order As Integer()

        ''' <summary>每个 tile 在 <see cref="TileGaussians"/> 中的区间（长度 tileCount + 1）</summary>
        Public Property TileOffsets As Integer()

        ''' <summary>每个 tile 内的高斯模型下标，深度有序</summary>
        Public Property TileGaussians As Integer()

        Public ReadOnly Property TileCount As Integer
            Get
                Return TilesX * TilesY
            End Get
        End Property

        ''' <summary>本帧参与合成的高斯数量</summary>
        Public ReadOnly Property SplatCount As Integer
            Get
                Return If(Order Is Nothing, 0, Order.Length)
            End Get
        End Property

    End Class

    ''' <summary>
    ''' 解析梯度累加目标。所有数组长度为模型高斯数，行优先。
    ''' </summary>
    Public Class SplatGradients

        Public Property Count As Integer

        Public Property Positions As Double()
        Public Property Scales As Double()
        Public Property Rotations As Double()
        Public Property Colors As Double()
        Public Property Opacities As Double()

        ''' <summary>
        ''' 屏幕空间中间梯度 dL/d(conic)：每高斯 3 个分量 [a, b, c]。
        ''' 由光栅化后端累加，随后在 <see cref="GradientCalculator"/> 中沿投影链回传。
        ''' </summary>
        Public Property Conic As Double()

        ''' <summary>屏幕空间中间梯度 dL/d(mean)：每高斯 2 个分量 [x, y]。</summary>
        Public Property Means As Double()

        Public Sub New(n As Integer)
            Me.Count = n
            Positions = New Double(n * 3 - 1) {}
            Scales = New Double(n * 3 - 1) {}
            Rotations = New Double(n * 4 - 1) {}
            Colors = New Double(n * 3 - 1) {}
            Opacities = New Double(n - 1) {}
            Conic = New Double(n * 3 - 1) {}
            Means = New Double(n * 2 - 1) {}
        End Sub

        Public Sub Clear()
            Array.Clear(Positions, 0, Positions.Length)
            Array.Clear(Scales, 0, Scales.Length)
            Array.Clear(Rotations, 0, Rotations.Length)
            Array.Clear(Colors, 0, Colors.Length)
            Array.Clear(Opacities, 0, Opacities.Length)
            Array.Clear(Conic, 0, Conic.Length)
            Array.Clear(Means, 0, Means.Length)
        End Sub

    End Class
