#Region "Microsoft.VisualBasic::c3d5e7f9a1b3485796c0d1e2f3a4b5c6, Data_science\MachineLearning\TensorFlow\Compute\ITensorComputeH.vb"

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

    '     Interface ITensorComputeH
    '
    '         Properties: Name, PinnedDeviceBytes, SupportsDeviceResidency
    '         Function: Abs, Add, AddScalar, AdvectTrilinear, ArgMax
    '                   ArgMin, ApplyMask, Clip, Conv2D, Conv2DBackwardBias
    '                   Conv2DBackwardFilter, Conv2DBackwardInput, Divide, DivideScalar
    '                   Elu, Exp, Gelu, HuberLoss, IsDevicePinned, JacobiStencil7
    '                   Laplacian7, L2Loss, LeakyRelu, LifStep, Log, LogSoftmax
    '                   MaskedCrossEntropy, MatMul, Max, MaxPool2D, MaxPool2DBackward
    '                   Maximum, Mean, MeanAll, Min, MinAll, Minimum, MseLoss
    '                   Multiply, MultiplyScalar, Negate, PinDevice, Reciprocal
    '                   Relu, Sigmoid, SigmoidCrossEntropyWithLogits, SpMM
    '                   Softmax, Sqrt, Square, Subtract, SubtractScalar, Sum
    '                   SumAll, Swish, SyncFromDevice, Tanh, Transpose, TryAdamWStep
    '                   UnpinDevice
    '         Sub: ApplyMask

    ' /********************************************************************************/

#End Region

' ---------------------------------------------------------------------------
' 半精度张量计算的"可插拔后端"契约
'
' 与双精度栈的 <see cref="ITensorCompute"/>、单精度栈的 <see cref="ITensorComputeF"/>
' 完全同构：
'   * 默认后端（<c>SIMDTensorH</c>）：基于 <see cref="Numerics.HalfSimd"/> 8 路内核的
'     SIMD CPU 实现（<c>Vector(Of Half)</c> 不受运行时支持，HalfSimd 走
'     MemoryMarshal.Cast(Of Half, Short) 通道）；
'   * 未来后端（<c>CudaTensorH</c>）：基于 ILCuda 的 fp16 GPU 实现。
'
' 契约集合以 <see cref="ITensorComputeF"/> 为蓝本裁剪：保留逐元素 / 标量 / 矩阵 /
' 归约 / softmax / 损失 / 训练 / 卷积池化 / CFD 原语的完整集合，去掉双精度栈独有的
' Sin / Cos / Pow / Prod / StdDev / Slice / Concat / TopK / Heaviside、
' 五个融合批算子（LtcRk4Batch / GraphLayerBatch / GraphFeatureBatch / FluxHeadBatch /
' SystemTauBatch）以及 PinDevice64 —— 它们在单精度契约中同样不存在，纳入只会制造
' 无调用方的死代码；后续需要时以接口扩展方式追加。
'
' 数值约定（fp16 的硬约束，实现方必须遵守）：
'   * Half 的表示范围仅 ±65520、正规数下界 6.1e-5，exp 在 x > 11.1 即溢出；
'   * Softmax / LogSoftmax / 损失 / 激活一律"中间量升到 Single/Double、出口一次舍入"；
'   * MatMul / SpMM 的累加用 Double；归约类返回值（SumAll 等）保持 Double；
'   * HalfSimd 的融合算子与标量 a op b 逐位一致，后端可做差分回归测试。
' ---------------------------------------------------------------------------

Namespace Compute

    ''' <summary>
    ''' 半精度张量计算后端契约。
    ''' </summary>
    ''' <remarks>
    ''' 形状校验、广播等"语义边界"仍然由 <see cref="TensorH"/> 层负责，
    ''' 后端只需要实现纯粹的数值计算。后端应当是**无状态**（或线程安全）的，
    ''' 因为 <see cref="TensorH.computeKernelH"/> 是一个全局共享变量。
    ''' </remarks>
    Public Interface ITensorComputeH

        ''' <summary>后端名称，用于诊断输出（例如 "SIMD-F16" / "CUDA-F16"）。</summary>
        ReadOnly Property Name As String

#Region "逐元素 - 二元"

        ''' <summary>逐元素相加</summary>
        Function Add(a As TensorH, b As TensorH) As TensorH
        ''' <summary>逐元素相减</summary>
        Function Subtract(a As TensorH, b As TensorH) As TensorH
        ''' <summary>逐元素相乘</summary>
        Function Multiply(a As TensorH, b As TensorH) As TensorH
        ''' <summary>逐元素相除</summary>
        Function Divide(a As TensorH, b As TensorH) As TensorH
        ''' <summary>逐元素取较大值（NaN 传播，与 System.Math.Max 一致）</summary>
        Function Maximum(a As TensorH, b As TensorH) As TensorH
        ''' <summary>逐元素取较小值（NaN 传播，与 System.Math.Min 一致）</summary>
        Function Minimum(a As TensorH, b As TensorH) As TensorH

#End Region

#Region "逐元素 - 一元"

        ''' <summary>逐元素指数</summary>
        Function Exp(t As TensorH) As TensorH
        ''' <summary>逐元素自然对数</summary>
        Function Log(t As TensorH) As TensorH
        ''' <summary>逐元素平方根</summary>
        Function Sqrt(t As TensorH) As TensorH
        ''' <summary>逐元素平方</summary>
        Function Square(t As TensorH) As TensorH
        ''' <summary>逐元素绝对值</summary>
        Function Abs(t As TensorH) As TensorH
        ''' <summary>逐元素取负</summary>
        Function Negate(t As TensorH) As TensorH
        ''' <summary>逐元素倒数</summary>
        Function Reciprocal(t As TensorH) As TensorH
        ''' <summary>逐元素双曲正切</summary>
        Function Tanh(t As TensorH) As TensorH
        ''' <summary>逐元素 Sigmoid</summary>
        Function Sigmoid(t As TensorH) As TensorH
        ''' <summary>逐元素 ReLU</summary>
        Function Relu(t As TensorH) As TensorH

        ''' <summary>
        ''' Leaky ReLU: max(alpha * x, x)。
        ''' </summary>
        ''' <param name="alpha">负半轴斜率（Double 传入，内部在 Single 域计算）</param>
        Function LeakyRelu(t As TensorH, alpha As Double) As TensorH

        ''' <summary>
        ''' ELU: x if x &gt; 0 else alpha * (exp(x) - 1)。
        ''' </summary>
        Function Elu(t As TensorH, alpha As Double) As TensorH

        ''' <summary>
        ''' GELU（tanh 近似）: 0.5 * x * (1 + tanh(sqrt(2/pi) * (x + 0.044715 * x^3)))。
        ''' </summary>
        Function Gelu(t As TensorH) As TensorH

        ''' <summary>Swish / SiLU: x * sigmoid(x)。</summary>
        Function Swish(t As TensorH) As TensorH

#End Region

#Region "标量运算"

        ''' <summary>逐元素加标量（Half 标量）</summary>
        Function AddScalar(t As TensorH, scalar As Half) As TensorH
        ''' <summary>逐元素加标量（Double 标量，先经 Single 广播）</summary>
        Function AddScalar(t As TensorH, scalar As Double) As TensorH
        ''' <summary>逐元素乘标量（Half 标量）</summary>
        Function MultiplyScalar(t As TensorH, scalar As Half) As TensorH
        ''' <summary>逐元素乘标量（Double 标量，先经 Single 广播）</summary>
        Function MultiplyScalar(t As TensorH, scalar As Double) As TensorH
        ''' <summary>逐元素除标量（Half 标量）</summary>
        Function DivideScalar(t As TensorH, scalar As Half) As TensorH
        ''' <summary>逐元素除标量（Double 标量，先经 Single 广播）</summary>
        Function DivideScalar(t As TensorH, scalar As Double) As TensorH
        ''' <summary>逐元素减标量（Half 标量）</summary>
        Function SubtractScalar(t As TensorH, scalar As Half) As TensorH
        ''' <summary>逐元素减标量（Double 标量，先经 Single 广播）</summary>
        Function SubtractScalar(t As TensorH, scalar As Double) As TensorH
        ''' <summary>把元素钳制到 [low, high] 区间</summary>
        Function Clip(t As TensorH, low As Half, high As Half) As TensorH

#End Region

#Region "矩阵运算"

        ''' <summary>二维矩阵乘法（Double 累加，出口一次舍入到 Half）</summary>
        Function MatMul(a As TensorH, b As TensorH) As TensorH
        ''' <summary>二维张量转置</summary>
        Function Transpose(t As TensorH) As TensorH

        ''' <summary>
        ''' 稀疏 × 稠密矩阵乘法：<c>dense[batch, Rows] · W[Rows, Columns] → [batch, Columns]</c>。
        ''' </summary>
        ''' <param name="csr">
        ''' CSR 稀疏连接矩阵（行 = 突触前，列 = 突触后）。注意 <see cref="SparseCsr.Values"/>
        ''' 是 <c>Double()</c>（两条既有栈共享同一稀疏结构），后端据此常驻显存并按
        ''' <see cref="SparseCsr.Version"/> 判定失效。
        ''' </param>
        ''' <param name="dense">稠密张量 <c>[batch, csr.Rows]</c>（例如上一时刻的脉冲矩阵）</param>
        ''' <returns>稠密张量 <c>[batch, csr.Columns]</c></returns>
        Function SpMM(csr As SparseCsr, dense As TensorH) As TensorH

        ''' <summary>
        ''' 融合的稀疏递归 LIF 单步：一次调用完成整步脉冲动力学，并<b>就地</b>更新状态张量。
        ''' </summary>
        ''' <remarks>
        ''' 语义与 <see cref="ITensorComputeF.LifStep"/> 完全一致，仅存储精度换成 Half。
        ''' β 与 threshold 刻意保持 <c>Double</c>：膜电位是逐时间步累积量，
        ''' 中间舍入会改变阈值判定。
        ''' </remarks>
        Function LifStep(synapses As SparseCsr,
                         sPrev As TensorH,
                         externalCurrent As TensorH,
                         h As TensorH,
                         s As TensorH,
                         counts As TensorH,
                         beta As Double,
                         threshold As Double,
                         subtractThreshold As Boolean) As Boolean

#End Region

#Region "归约运算"

        ''' <summary>整体求和。累加使用双精度，避免半精度累加的舍入误差。</summary>
        Function SumAll(t As TensorH) As Double
        ''' <summary>整体求均值</summary>
        Function MeanAll(t As TensorH) As Double
        ''' <summary>最小值（NaN 传播，与 <see cref="Numerics.HalfSimd.Min"/> 一致）</summary>
        Function MinAll(t As TensorH) As Half
        ''' <summary>最大值（NaN 传播，与 <see cref="Numerics.HalfSimd.Max"/> 一致）</summary>
        Function MaxAll(t As TensorH) As Half

        ''' <summary>沿指定轴（Nothing 表示整体）求和</summary>
        Function Sum(t As TensorH, axis As Integer?, keepdims As Boolean) As TensorH
        ''' <summary>沿指定轴（Nothing 表示整体）求均值</summary>
        Function Mean(t As TensorH, axis As Integer?, keepdims As Boolean) As TensorH
        ''' <summary>沿指定轴（Nothing 表示整体）取最大值</summary>
        Function Max(t As TensorH, axis As Integer?, keepdims As Boolean) As TensorH
        ''' <summary>沿指定轴（Nothing 表示整体）取最小值</summary>
        Function Min(t As TensorH, axis As Integer?, keepdims As Boolean) As TensorH
        ''' <summary>沿指定轴（Nothing 表示整体）取最大值的下标</summary>
        Function ArgMax(t As TensorH, axis As Integer?) As TensorH
        ''' <summary>沿指定轴（Nothing 表示整体）取最小值的下标</summary>
        Function ArgMin(t As TensorH, axis As Integer?) As TensorH

#End Region

#Region "神经网络"

        ' ------------------------------------------------------------------
        ' 这一组算子的取舍说明（与单精度栈相同的改造边界）：
        '
        ' 它们**可以**落在半精度路径上，但内部必须做数值稳定的重参数化
        ' （softmax 先减最大值、损失按稳定公式计算），且中间量一律升到
        ' Single/Double —— fp16 的 exp 在 x > 11.1 即溢出，不做重参数化
        ' 连前向都会出 NaN。舍入误差只体现在 E-3 量级的相对误差上，
        ' 不会改变 argmax 结果；而若把「输出的概率」再喂回去做
        ' 损失/优化器更新，则调用方应当在出口处升精度。
        ' ------------------------------------------------------------------

        ''' <summary>沿指定轴做 softmax（内部先减去该轴最大值以避免 exp 溢出）</summary>
        Function Softmax(t As TensorH, axis As Integer) As TensorH
        ''' <summary>沿指定轴做 log-softmax（<c>x − logsumexp(x)</c>，同样做减最大值稳定化）</summary>
        Function LogSoftmax(t As TensorH, axis As Integer) As TensorH
        ''' <summary>逐元素的 sigmoid 交叉熵（logits 形式，内部按 <c>max(x,0) − x·z + log(1+e^−|x|)</c> 稳定计算）</summary>
        Function SigmoidCrossEntropyWithLogits(labels As TensorH, logits As TensorH) As TensorH
        ''' <summary>
        ''' 均方误差损失。注意返回的是**标量张量**（全部元素的平均值），
        ''' 与两条既有栈的语义一致，而非逐元素误差。
        ''' </summary>
        Function MseLoss(predictions As TensorH, targets As TensorH) As TensorH
        ''' <summary>整体 L2 损失的一半 <c>Σ x² / 2</c>，返回标量张量</summary>
        Function L2Loss(t As TensorH) As TensorH
        ''' <summary>
        ''' Huber 损失。<c>|d| &lt;= delta</c> 时取 <c>0.5·d²</c>，否则取 <c>delta·(|d| − 0.5·delta)</c>；
        ''' 同样返回**标量张量**（平均值），与两条既有栈一致。
        ''' </summary>
        Function HuberLoss(predictions As TensorH, targets As TensorH, delta As Half) As TensorH

#End Region

#Region "训练算子"

        ''' <summary>
        ''' 带损失掩码的 softmax 交叉熵。
        ''' </summary>
        ''' <param name="logits">形状 <c>[rows, vocab]</c> 的二维 logits</param>
        ''' <param name="targets">长度 <c>&gt;= rows</c> 的目标下标</param>
        ''' <param name="mask">长度 <c>&gt;= rows</c> 的损失掩码；<c>Nothing</c> 表示全部计入</param>
        ''' <param name="dLogits">输出参数：形状同 <paramref name="logits"/>，内容 <c>(softmax − onehot) / count</c></param>
        ''' <returns>平均到每个有效位置上的负对数似然（Double 累加）；无有效位置时返回 0</returns>
        Function MaskedCrossEntropy(logits As TensorH,
                                    targets As Integer(),
                                    mask As Boolean(),
                                    ByRef dLogits As TensorH) As Double

        ''' <summary>
        ''' 试做一次 AdamW 参数更新。
        ''' </summary>
        ''' <remarks>
        ''' 超参数（学习率 / 衰减率 / eps / 偏差校正）刻意全部保持 <c>Double</c>：
        ''' 1e−8 量级的 eps 在 Half 下会被直接吸收。浅层存储降精度即可，
        ''' 不要把这些常量也降下去。
        ''' </remarks>
        Function TryAdamWStep(param As TensorH, gradient As TensorH,
                              momentum As TensorH, velocity As TensorH,
                              learningRate As Double, beta1 As Double, beta2 As Double, eps As Double,
                              biasCorrection1 As Double, biasCorrection2 As Double,
                              weightDecay As Double) As Boolean

        ''' <summary>是否支持把张量钉成设备常驻缓冲（避免训练时每步重新上传）。</summary>
        ReadOnly Property SupportsDeviceResidency As Boolean
        ''' <summary>
        ''' 把张量钉成设备常驻缓冲。
        ''' </summary>
        ''' <param name="t">要被钉住的张量（其底层数组作为键）</param>
        ''' <param name="label">诊断用标签，例如 <c>conv3.W</c></param>
        ''' <param name="zeroFill">是否用 0 初始化（梯度累加器与优化器状态用 <c>True</c>）</param>
        ''' <returns>钉住成功返回 <c>True</c>；不支持时返回 <c>False</c></returns>
        ''' <remarks>
        ''' 钉住后<b>设备成为主副本</b>，主机 <c>Data</c> 会逐渐陈旧。
        ''' 凡是需要读主机内容的场合（检查点落盘、主机侧统计）都必须先调用
        ''' <see cref="SyncFromDevice"/>。半精度栈只有 FP16 通道，因此不提供 PinDevice64。
        ''' </remarks>
        Function PinDevice(t As TensorH, label As String, zeroFill As Boolean) As Boolean
        ''' <summary>解除钉住并立即释放对应的显存。</summary>
        Function UnpinDevice(t As TensorH) As Boolean
        ''' <summary>该张量当前是否已被钉住。</summary>
        Function IsDevicePinned(t As TensorH) As Boolean
        ''' <summary>当前钉住的显存总字节数（供显存占用报告使用）。</summary>
        ReadOnly Property PinnedDeviceBytes As Long
        ''' <summary>
        ''' 把设备常驻缓冲的内容回写到主机数组。未被钉住或不支持常驻时返回 <c>False</c>。
        ''' </summary>
        Function SyncFromDevice(t As TensorH) As Boolean

#End Region

#Region "卷积与池化"

        ' ------------------------------------------------------------------
        ' 布局约定（全部为行主序 / channel-last，与两条既有栈完全一致):
        '
        '   输入   x          : (N, H, W, C)
        '   卷积核 filters    : (KH, KW, C, OutC)
        '   偏置   bias       : (OutC)
        '   输出   y          : (N, OH, OW, OutC)
        '
        '   OH = (H + 2*padding - KH) / stride + 1
        '   OW = (W + 2*padding - KW) / stride + 1
        ' ------------------------------------------------------------------

        ''' <summary>二维卷积（严格来说是互相关，与主流深度学习框架一致）</summary>
        Function Conv2D(x As TensorH, filters As TensorH, bias As TensorH,
                        stride As Integer, padding As Integer) As TensorH
        ''' <summary>卷积反向：对输入的梯度，形状 (N, H, W, C)</summary>
        Function Conv2DBackwardInput(gradOutput As TensorH, filters As TensorH,
                                     inputShape As Integer(), stride As Integer, padding As Integer) As TensorH
        ''' <summary>卷积反向：对卷积核的梯度，形状 (KH, KW, C, OutC)</summary>
        Function Conv2DBackwardFilter(gradOutput As TensorH, x As TensorH,
                                      filterShape As Integer(), stride As Integer, padding As Integer) As TensorH
        ''' <summary>卷积反向：对偏置的梯度（沿 N / OH / OW 求和），形状 (OutC)</summary>
        Function Conv2DBackwardBias(gradOutput As TensorH) As TensorH

        ''' <summary>
        ''' 二维最大池化前向，同时输出每个输出位置对应的<b>输入扁平下标</b>供反向 scatter。
        ''' </summary>
        ''' <param name="argMax">形状 (N, OH, OW, C)，元素为获胜元素在输入张量中的扁平下标</param>
        Function MaxPool2D(x As TensorH, size As Integer, stride As Integer, padding As Integer,
                           ByRef argMax As TensorH) As TensorH
        ''' <summary>二维最大池化反向：按 argMax 把梯度回填到输入位置</summary>
        Function MaxPool2DBackward(gradOutput As TensorH, argMax As TensorH, inputShape As Integer()) As TensorH

#End Region

#Region "CFD 扩展原语"

        ''' <summary>
        ''' 七点拉普拉斯 stencil：out = Σ(流体邻居) - nFluid * 中心。
        ''' 固体（掩膜为 0）单元的输出恒为 0；固体邻居按 Neumann 零梯度不计入，
        ''' 因此除数取该格的流体邻居数 <paramref name="nFluid"/>。
        ''' </summary>
        ''' <param name="src">输入场</param>
        ''' <param name="mask">固体掩膜，0 = 固体 / 空腔，非 0 = 流体。传 Nothing 表示全流体</param>
        ''' <param name="nFluid">每格的流体邻居数（0..6），长度与场一致</param>
        ''' <returns>拉普拉斯结果</returns>
        Function Laplacian7(src As TensorH, mask As Byte(), nFluid As Byte()) As TensorH

        ''' <summary>
        ''' 带掩膜的七点 Jacobi 迭代。统一迭代式：
        ''' <c>x_new(idx) = (alpha · Σ_fluid x_old(邻居) + rhs(idx)) / denom</c>
        ''' 其中 <c>denom = beta</c>（当 <c>beta &gt; 0</c>），否则 <c>denom = nFluid(idx)</c>。
        ''' 固体单元恒为 0。
        ''' </summary>
        Function JacobiStencil7(rhs As TensorH, mask As Byte(), nFluid As Byte(),
                                alpha As Half, beta As Half,
                                iterations As Integer,
                                Optional initial As TensorH = Nothing) As TensorH

        ''' <summary>
        ''' 半拉格朗日平流：对每个格子沿速度场反向追踪 dt 时间，
        ''' 在回溯位置对源场做三线性插值采样。纯 gather，可完全并行。
        ''' </summary>
        Function AdvectTrilinear(src As TensorH,
                                 u As TensorH, v As TensorH, w As TensorH,
                                 dt As Half, mask As Byte()) As TensorH

        ''' <summary>
        ''' 按掩膜把所有给定场中的固体单元置零（无滑移壁：速度 / 压力 / 密度恒为 0）。
        ''' </summary>
        ''' <param name="mask">固体掩膜，0 = 固体</param>
        ''' <param name="fields">需要置零的场（数量不定）</param>
        Sub ApplyMask(mask As Byte(), ParamArray fields As TensorH())

#End Region

    End Interface

End Namespace
