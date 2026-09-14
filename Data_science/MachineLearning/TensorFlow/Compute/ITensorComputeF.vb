' ---------------------------------------------------------------------------
' 单精度张量计算的"可插拔后端"契约
'
' 与双精度栈的 <see cref="ITensorCompute"/> 完全同构：
'   * 默认后端（<c>SIMDTensorF</c>）：基于 System.Numerics.Vector(Of Single) 的 SIMD CPU 实现
'   * 未来后端（<c>CudaTensorF</c>）  ：基于 ILCuda 的 GPU 实现
'
' 除了常规的逐元素 / 标量 / 归约算子之外，本接口**额外定义了 CFD 求解器真正
' 依赖的三个原语**（双精度栈里没有，也是 CFD 无法从 Double 栈获得 GPU 加速的
' 根本原因）：
'   * Laplacian7     —— 七点拉普拉斯 stencil（按掩膜处理固体邻居）
'   * JacobiStencil7 —— 带掩膜与流体邻居数的 Jacobi 迭代求解（压力泊松 / 隐式扩散）
'   * AdvectTrilinear—— 半拉格朗日回溯的三线性 gather（scatter-free，可完全并行）
'
' 有了这三个原语，整个 StableFluids 时间步就能整体下放到 GPU，
' 而不需要在 CPU 与 GPU 之间来回搬运场数据。
' ---------------------------------------------------------------------------

Imports std = System.Math

Namespace Compute

    ''' <summary>
    ''' 单精度张量计算后端契约。
    ''' </summary>
    ''' <remarks>
    ''' 形状校验、广播等"语义边界"仍然由 <see cref="TensorF"/> 层负责，
    ''' 后端只需要实现纯粹的数值计算。后端应当是**无状态**（或线程安全）的，
    ''' 因为 <see cref="TensorF.computeKernelF"/> 是一个全局共享变量。
    ''' </remarks>
    Public Interface ITensorComputeF

        ''' <summary>后端名称，用于诊断输出（例如 "SIMD-F32" / "CUDA-F32"）。</summary>
        ReadOnly Property Name As String

#Region "逐元素 - 二元"

        ''' <summary>逐元素相加</summary>
        Function Add(a As TensorF, b As TensorF) As TensorF
        ''' <summary>逐元素相减</summary>
        Function Subtract(a As TensorF, b As TensorF) As TensorF
        ''' <summary>逐元素相乘</summary>
        Function Multiply(a As TensorF, b As TensorF) As TensorF
        ''' <summary>逐元素相除</summary>
        Function Divide(a As TensorF, b As TensorF) As TensorF
        ''' <summary>逐元素取较大值</summary>
        Function Maximum(a As TensorF, b As TensorF) As TensorF
        ''' <summary>逐元素取较小值</summary>
        Function Minimum(a As TensorF, b As TensorF) As TensorF

#End Region

#Region "逐元素 - 一元"

        ''' <summary>逐元素指数</summary>
        Function Exp(t As TensorF) As TensorF
        ''' <summary>逐元素自然对数</summary>
        Function Log(t As TensorF) As TensorF
        ''' <summary>逐元素平方根</summary>
        Function Sqrt(t As TensorF) As TensorF
        ''' <summary>逐元素平方</summary>
        Function Square(t As TensorF) As TensorF
        ''' <summary>逐元素绝对值</summary>
        Function Abs(t As TensorF) As TensorF
        ''' <summary>逐元素取负</summary>
        Function Negate(t As TensorF) As TensorF
        ''' <summary>逐元素倒数</summary>
        Function Reciprocal(t As TensorF) As TensorF
        ''' <summary>逐元素双曲正切</summary>
        Function Tanh(t As TensorF) As TensorF
        ''' <summary>逐元素 Sigmoid</summary>
        Function Sigmoid(t As TensorF) As TensorF
        ''' <summary>逐元素 ReLU</summary>
        Function Relu(t As TensorF) As TensorF

#End Region

#Region "标量运算"

        ''' <summary>逐元素加标量</summary>
        Function AddScalar(t As TensorF, scalar As Single) As TensorF
        ''' <summary>逐元素乘标量</summary>
        Function MultiplyScalar(t As TensorF, scalar As Single) As TensorF
        ''' <summary>逐元素除标量</summary>
        Function DivideScalar(t As TensorF, scalar As Single) As TensorF
        ''' <summary>把元素钳制到 [low, high] 区间</summary>
        Function Clip(t As TensorF, low As Single, high As Single) As TensorF

#End Region

#Region "矩阵运算"

        ''' <summary>二维矩阵乘法</summary>
        Function MatMul(a As TensorF, b As TensorF) As TensorF

#End Region

#Region "归约运算"

        ''' <summary>
        ''' 整体求和。累加使用双精度，避免大数组单精度累加的舍入误差。
        ''' </summary>
        Function SumAll(t As TensorF) As Double
        ''' <summary>整体求均值</summary>
        Function MeanAll(t As TensorF) As Double
        ''' <summary>最小值</summary>
        Function MinAll(t As TensorF) As Single
        ''' <summary>最大值</summary>
        Function MaxAll(t As TensorF) As Single

#End Region

#Region "CFD 扩展原语（GPU 后端必须实现）"

        ''' <summary>
        ''' 七点拉普拉斯 stencil：out = Σ(流体邻居) - nFluid * 中心。
        ''' 固体（掩膜为 0）单元的输出恒为 0；固体邻居按 Neumann 零梯度不计入，
        ''' 因此除数取该格的流体邻居数 <paramref name="nFluid"/>。
        ''' </summary>
        ''' <param name="src">输入场</param>
        ''' <param name="mask">固体掩膜，0 = 固体 / 空腔，非 0 = 流体。传 Nothing 表示全流体</param>
        ''' <param name="nFluid">每格的流体邻居数（0..6），长度与场一致</param>
        ''' <returns>拉普拉斯结果</returns>
        Function Laplacian7(src As TensorF, mask As Byte(), nFluid As Byte()) As TensorF

        ''' <summary>
        ''' 带掩膜的七点 Jacobi 迭代。统一迭代式：
        ''' <c>x_new(idx) = (alpha · Σ_fluid x_old(邻居) + rhs(idx)) / denom</c>
        ''' 其中 <c>denom = beta</c>（当 <c>beta &gt; 0</c>），否则 <c>denom = nFluid(idx)</c>。
        ''' 固体单元恒为 0。两种典型用法：
        '''   压力泊松 ∇²p = div        → alpha = 1, beta = 0（除数取 nFluid）, rhs = -div
        '''   隐式扩散 (1+6a)φ = φ_old + a·Σ → alpha = a, beta = 1 + 6a, rhs = φ_old
        ''' </summary>
        ''' <param name="rhs">右端项（见上：压力泊松传 -div，扩散传 φ_old）</param>
        ''' <param name="mask">固体掩膜，0 = 固体。传 Nothing 表示全流体</param>
        ''' <param name="nFluid">每格的流体邻居数</param>
        ''' <param name="alpha">邻居项系数</param>
        ''' <param name="beta">中心项除数；&lt;= 0 表示除数改用每格流体邻居数</param>
        ''' <param name="iterations">迭代次数</param>
        ''' <param name="initial">初始猜测；传 Nothing 则全零开始</param>
        ''' <returns>迭代后的解</returns>
        Function JacobiStencil7(rhs As TensorF, mask As Byte(), nFluid As Byte(),
                                alpha As Single, beta As Single,
                                iterations As Integer,
                                Optional initial As TensorF = Nothing) As TensorF

        ''' <summary>
        ''' 半拉格朗日平流：对每个格子沿速度场反向追踪 dt 时间，
        ''' 在回溯位置对源场做三线性插值采样。
        ''' 这是纯 gather（只读源场、只写目标格），无 scatter，因此可以完全并行。
        ''' </summary>
        ''' <param name="src">被平流的源场</param>
        ''' <param name="u">速度场 X 分量</param>
        ''' <param name="v">速度场 Y 分量</param>
        ''' <param name="w">速度场 Z 分量</param>
        ''' <param name="dt">时间步长</param>
        ''' <param name="mask">固体掩膜，0 = 固体（输出恒为 0）。传 Nothing 表示全流体</param>
        ''' <returns>平流后的新场</returns>
        Function AdvectTrilinear(src As TensorF,
                                 u As TensorF, v As TensorF, w As TensorF,
                                 dt As Single, mask As Byte()) As TensorF

        ''' <summary>
        ''' 按掩膜把所有给定场中的固体单元置零（无滑移壁：速度 / 压力 / 密度恒为 0）。
        ''' </summary>
        ''' <param name="mask">固体掩膜，0 = 固体</param>
        ''' <param name="fields">需要置零的场（数量不定）</param>
        Sub ApplyMask(mask As Byte(), ParamArray fields As TensorF())

#End Region

    End Interface

End Namespace
