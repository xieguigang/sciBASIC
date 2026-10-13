#Region "Microsoft.VisualBasic::ad5bc3d68881a85384731823791fb845, Data_science\MachineLearning\TensorFlow\Compute\ITensorComputeF.vb"

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

    '   Total Lines: 174
    '    Code Lines: 52 (29.89%)
    ' Comment Lines: 95 (54.60%)
    '    - Xml Docs: 82.11%
    ' 
    '   Blank Lines: 27 (15.52%)
    '     File Size: 8.49 KB


    '     Interface ITensorComputeF
    ' 
    '         Properties: Name
    ' 
    '         Function: Abs, Add, AddScalar, AdvectTrilinear, Clip
    '                   Divide, DivideScalar, Exp, JacobiStencil7, Laplacian7
    '                   Log, MatMul, MaxAll, Maximum, MeanAll
    '                   MinAll, Minimum, Multiply, MultiplyScalar, Negate
    '                   Reciprocal, Relu, Sigmoid, Sqrt, Square
    '                   Subtract, SumAll, Tanh
    ' 
    '         Sub: ApplyMask
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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
        ''' <summary>二维张量转置</summary>
        Function Transpose(t As TensorF) As TensorF

        ''' <summary>
        ''' 稀疏 × 稠密矩阵乘法：<c>dense[batch, Rows] · W[Rows, Columns] → [batch, Columns]</c>。
        ''' </summary>
        ''' <param name="csr">
        ''' CSR 稀疏连接矩阵（行 = 突触前，列 = 突触后）。GPU 后端可据此把 CSR 数组常驻显存，
        ''' 并按 <see cref="SparseCsr.Version"/> 判定失效。
        ''' </param>
        ''' <param name="dense">稠密张量 <c>[batch, csr.Rows]</c>（例如上一时刻的脉冲矩阵）</param>
        ''' <returns>稠密张量 <c>[batch, csr.Columns]</c></returns>
        ''' <remarks>
        ''' 与双精度栈的 <see cref="ITensorCompute.SpMM"/> 语义完全一致，是本契约第一个从
        ''' 双精度栈对齐过来的「非逐元素」算子：图的邻接传播（GNN）与突触电流积累（SNN）
        ''' 本质都是 SpMM，缺少它时上层只能退回稠密 GEMM。
        ''' </remarks>
        Function SpMM(csr As SparseCsr, dense As TensorF) As TensorF

        ''' <summary>
        ''' 融合的稀疏递归 LIF 单步：一次调用完成整步脉冲动力学，并<b>就地</b>更新状态张量。
        ''' </summary>
        ''' <param name="synapses">突触连接矩阵 <c>W[Units, Units]</c>（行 = 突触前，列 = 突触后）</param>
        ''' <param name="sPrev">上一时间步的输出脉冲 <c>S[t−1]</c>（只读）</param>
        ''' <param name="externalCurrent">外部注入电流 <c>I_ext[t]</c>（只读）；<c>Nothing</c> 表示无外部刺激</param>
        ''' <param name="h">膜电位 <c>H[t−1]</c>，返回时被就地改写为 <c>H[t]</c></param>
        ''' <param name="s">输出参数：本步脉冲 <c>S[t]</c>（就地写入 0/1）</param>
        ''' <param name="counts">输出参数：脉冲计数累加器（就地累加 <c>S[t]</c>），调用方须先清零</param>
        ''' <param name="beta">
        ''' 膜电位衰减系数 β ∈ (0,1)。刻意保持 <c>Double</c>：这是逐时间步累积的递归状态，
        ''' 单精度舍入会改变阈值判定，历史上 SNN 的 CSng(Beta) 截断正是仿真漂移的根因。
        ''' </param>
        ''' <param name="threshold">发放阈值 <c>U_thr</c>（同样保持 <c>Double</c>）</param>
        ''' <param name="subtractThreshold">
        ''' 复位模式：<c>True</c> 表示 <c>H[t] = U[t] − S[t]·U_thr</c>；<c>False</c> 表示 <c>H[t] = U[t]·(1 − S[t])</c>
        ''' </param>
        ''' <returns>
        ''' <c>True</c> 表示整步已由本后端完成（调用方不得再走逐算子路径）；
        ''' <c>False</c> 表示本后端不提供该能力，由调用方回退。
        ''' </returns>
        ''' <remarks>
        ''' 之所以做成「一个」算子而非让调用方串起 SpMM / Add / MultiplyScalar / Heaviside，
        ''' 是因为逐算子写法每步要分配 6 个 <c>[batch, Units]</c> 中间张量，
        ''' 在 GPU 后端下产生 6~12 次显存往返，往返开销会把内核的计算收益整个吃掉。
        ''' </remarks>
        Function LifStep(synapses As SparseCsr,
                         sPrev As TensorF,
                         externalCurrent As TensorF,
                         h As TensorF,
                         s As TensorF,
                         counts As TensorF,
                         beta As Double,
                         threshold As Double,
                         subtractThreshold As Boolean) As Boolean

#End Region

#Region "归约运算"

        ''' <summary>整体求和。累加使用双精度，避免大数组单精度累加的舍入误差。</summary>
        Function SumAll(t As TensorF) As Double
        ''' <summary>整体求均值</summary>
        Function MeanAll(t As TensorF) As Double
        ''' <summary>最小值</summary>
        Function MinAll(t As TensorF) As Single
        ''' <summary>最大值</summary>
        Function MaxAll(t As TensorF) As Single

        ''' <summary>沿指定轴（Nothing 表示整体）求和</summary>
        Function Sum(t As TensorF, axis As Integer?, keepdims As Boolean) As TensorF
        ''' <summary>沿指定轴（Nothing 表示整体）求均值</summary>
        Function Mean(t As TensorF, axis As Integer?, keepdims As Boolean) As TensorF
        ''' <summary>沿指定轴（Nothing 表示整体）取最大值</summary>
        Function Max(t As TensorF, axis As Integer?, keepdims As Boolean) As TensorF
        ''' <summary>沿指定轴（Nothing 表示整体）取最小值</summary>
        Function Min(t As TensorF, axis As Integer?, keepdims As Boolean) As TensorF
        ''' <summary>沿指定轴（Nothing 表示整体）取最大值的下标</summary>
        Function ArgMax(t As TensorF, axis As Integer?) As TensorF
        ''' <summary>沿指定轴（Nothing 表示整体）取最小值的下标</summary>
        Function ArgMin(t As TensorF, axis As Integer?) As TensorF

#End Region

#Region "神经网络"

        ' ------------------------------------------------------------------
        ' 这一组算子的取舍说明（混合精度改造的核心边界）：
        '
        ' 它们**可以**落在单精度路径上，但内部必须做数值稳定的重参数化
        ' （softmax 先减最大值、log 先 clamp），舍入误差只体现在 E−7 量级的
        ' 相对误差上，不会改变 argmax 结果；而若把「输出的概率」再喂回去做
        ' 损失/优化器更新，则调用方应当在出口处升精度。
        ' ------------------------------------------------------------------

        ''' <summary>沿指定轴做 softmax（内部先减去该轴最大值以避免 exp 溢出）</summary>
        Function Softmax(t As TensorF, axis As Integer) As TensorF
        ''' <summary>沿指定轴做 log-softmax（<c>x − logsumexp(x)</c>，同样做减最大值稳定化）</summary>
        Function LogSoftmax(t As TensorF, axis As Integer) As TensorF
        ''' <summary>逐元素的 sigmoid 交叉熵（logits 形式，内部按 <c>max(x,0) − x·z + log(1+e^−|x|)</c> 稳定计算）</summary>
        Function SigmoidCrossEntropyWithLogits(labels As TensorF, logits As TensorF) As TensorF
        ''' <summary>
        ''' 均方误差损失。注意返回的是**标量张量**（全部元素的平均值），
        ''' 与双精度栈 <see cref="ITensorCompute.MseLoss"/> 的语义一致，而非逐元素误差。
        ''' </summary>
        Function MseLoss(predictions As TensorF, targets As TensorF) As TensorF
        ''' <summary>整体 L2 损失的一半 <c>Σ x² / 2</c>，返回标量张量</summary>
        Function L2Loss(t As TensorF) As TensorF
        ''' <summary>
        ''' Huber 损失。<c>|d| &lt;= delta</c> 时取 <c>0.5·d²</c>，否则取 <c>delta·(|d| − 0.5·delta)</c>；
        ''' 同样返回**标量张量**（平均值），与双精度栈一致。
        ''' </summary>
        Function HuberLoss(predictions As TensorF, targets As TensorF, delta As Single) As TensorF

#End Region

#Region "训练算子"

        ' ------------------------------------------------------------------
        ' 语言模型/分类训练步里两处「逐元素主机循环」在大张量下会成为绝对瓶颈：
        '   * 掩码交叉熵：对 [rows, vocab] 做 softmax + NLL，大词表即数千万次 exp；
        '   * TryAdamWStep：对每个参数的 4 个数组做逐元素更新。
        '
        ' 注意返回值刻意保持 Double：损失是用「Double 累加器」对「Single 数据」求和得到的，
        ' 这样下文 Advice 的损失数值不会因为计算侧降精度而抖动。
        ' ------------------------------------------------------------------

        ''' <summary>
        ''' 带损失掩码的 softmax 交叉熵。
        ''' </summary>
        ''' <param name="logits">形状 <c>[rows, vocab]</c> 的二维 logits</param>
        ''' <param name="targets">长度 <c>&gt;= rows</c> 的目标下标</param>
        ''' <param name="mask">长度 <c>&gt;= rows</c> 的损失掩码；<c>Nothing</c> 表示全部计入</param>
        ''' <param name="dLogits">
        ''' 输出参数：形状同 <paramref name="logits"/>，内容 <c>(softmax − onehot) / count</c>
        ''' </param>
        ''' <returns>平均到每个有效位置上的负对数似然（Double 累加）；无有效位置时返回 0</returns>
        Function MaskedCrossEntropy(logits As TensorF,
                                    targets As Integer(),
                                    mask As Boolean(),
                                    ByRef dLogits As TensorF) As Double

        ''' <summary>
        ''' 试做一次 AdamW 参数更新。
        ''' </summary>
        ''' <param name="param">待更新的参数（可能被就地改写）</param>
        ''' <param name="gradient">梯度累加器；成功后由实现负责清零</param>
        ''' <param name="momentum">一阶矩估计</param>
        ''' <param name="velocity">二阶矩估计</param>
        ''' <param name="biasCorrection1"><c>1 − beta1^step</c>（由调用方算好）</param>
        ''' <param name="biasCorrection2"><c>1 − beta2^step</c>（由调用方算好）</param>
        ''' <param name="weightDecay">解耦权重衰减系数；0 表示退化为纯 Adam</param>
        ''' <returns><c>True</c> 表示本轮更新已由本后端完成；<c>False</c> 表示调用方回退</returns>
        ''' <remarks>
        ''' 超参数（学习率 / 衰减率 / eps / 偏差校正）刻意全部保持 <c>Double</c>：
        ''' 1e−8 量级的 eps 在 Single 下会被附近的 v 直接吸收，是 Adam 数值失稳的
        ''' 典型来源。浅层存储降精度即可，不要把这些常量也降下去。
        ''' </remarks>
        Function TryAdamWStep(param As TensorF, gradient As TensorF,
                              momentum As TensorF, velocity As TensorF,
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
        ''' <see cref="SyncFromDevice"/>。单精度栈只有 FP32 通道，因此不提供 PinDevice64。
        ''' </remarks>
        Function PinDevice(t As TensorF, label As String, zeroFill As Boolean) As Boolean
        ''' <summary>解除钉住并立即释放对应的显存。</summary>
        Function UnpinDevice(t As TensorF) As Boolean
        ''' <summary>该张量当前是否已被钉住。</summary>
        Function IsDevicePinned(t As TensorF) As Boolean
        ''' <summary>当前钉住的显存总字节数（供显存占用报告使用）。</summary>
        ReadOnly Property PinnedDeviceBytes As Long
        ''' <summary>
        ''' 把设备常驻缓冲的内容回写到主机数组。未被钉住或不支持时常驻时返回 <c>False</c>。
        ''' </summary>
        Function SyncFromDevice(t As TensorF) As Boolean

#End Region

#Region "卷积与池化"

        ' ------------------------------------------------------------------
        ' 布局约定（全部为行主序 / channel-last，与双精度栈完全一致）:
        '
        '   输入   x          : (N, H, W, C)
        '   卷积核 filters    : (KH, KW, C, OutC)
        '   偏置   bias       : (OutC)
        '   输出   y          : (N, OH, OW, OutC)
        '
        '   OH = (H + 2*padding - KH) / stride + 1
        '   OW = (W + 2*padding - KW) / stride + 1
        '
        ' 选择 channel-last 是为了与 CNN 既有的 DataBlock（索引 (y*SX + x)*Depth + c）
        ' 完全同构，迁移时不需要任何转置/重排。
        ' ------------------------------------------------------------------

        ''' <summary>二维卷积（严格来说是互相关，与主流深度学习框架一致）</summary>
        Function Conv2D(x As TensorF, filters As TensorF, bias As TensorF,
                        stride As Integer, padding As Integer) As TensorF
        ''' <summary>卷积反向：对输入的梯度，形状 (N, H, W, C)</summary>
        Function Conv2DBackwardInput(gradOutput As TensorF, filters As TensorF,
                                     inputShape As Integer(), stride As Integer, padding As Integer) As TensorF
        ''' <summary>卷积反向：对卷积核的梯度，形状 (KH, KW, C, OutC)</summary>
        Function Conv2DBackwardFilter(gradOutput As TensorF, x As TensorF,
                                      filterShape As Integer(), stride As Integer, padding As Integer) As TensorF
        ''' <summary>卷积反向：对偏置的梯度（沿 N / OH / OW 求和），形状 (OutC)</summary>
        Function Conv2DBackwardBias(gradOutput As TensorF) As TensorF

        ''' <summary>
        ''' 二维最大池化前向，同时输出每个输出位置对应的<b>输入扁平下标</b>供反向 scatter。
        ''' </summary>
        ''' <param name="argMax">形状 (N, OH, OW, C)，元素为获胜元素在输入张量中的扁平下标</param>
        Function MaxPool2D(x As TensorF, size As Integer, stride As Integer, padding As Integer,
                           ByRef argMax As TensorF) As TensorF
        ''' <summary>二维最大池化反向：按 argMax 把梯度回填到输入位置</summary>
        Function MaxPool2DBackward(gradOutput As TensorF, argMax As TensorF, inputShape As Integer()) As TensorF

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
