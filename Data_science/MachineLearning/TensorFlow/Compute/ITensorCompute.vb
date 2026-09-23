#Region "Microsoft.VisualBasic::bf2804ff491fac69e4c4405f0db5553d, Data_science\MachineLearning\TensorFlow\Compute\ITensorCompute.vb"

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

    '   Total Lines: 379
    '    Code Lines: 99 (26.12%)
    ' Comment Lines: 229 (60.42%)
    '    - Xml Docs: 83.84%
    ' 
    '   Blank Lines: 51 (13.46%)
    '     File Size: 20.36 KB


    '     Interface ITensorCompute
    ' 
    '         Properties: Name, PinnedDeviceBytes, SupportsDeviceResidency
    ' 
    '         Function: Abs, Add, AddScalar, ArgMax, ArgMin
    '                   Clip, Concat, Conv2D, Conv2DBackwardBias, Conv2DBackwardFilter
    '                   Conv2DBackwardInput, Cos, Divide, DivideScalar, Elu
    '                   Exp, Gelu, Heaviside, HuberLoss, IsDevicePinned
    '                   L2Loss, L2Norm, LeakyRelu, LifStep, Log, LogSoftmax
    '                   MaskedCrossEntropy, MatMul, Max, Maximum, MaxPool2D
    '                   MaxPool2DBackward, Mean, MeanAll, Min, Minimum
    '                   MseLoss, Multiply, MultiplyScalar, Negate, PinDevice
    '                   PinDevice64, Pow, Prod, Reciprocal, Relu, Sigmoid
    '                   SigmoidCrossEntropyWithLogits, Sin, Slice, Softmax, SpMM
    '                   Sqrt, Square, StdDev, Subtract, Sum
    '                   SumAll, Swish, SyncFromDevice, Tanh, TopK
    '                   Transpose, TryAdamWStep, UnpinDevice
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' ---------------------------------------------------------------------------
' 张量计算的“可插拔后端”契约
'
' 这个接口把 Tensor / Math / nn 里所有**纯数值计算**的算子抽象出来，
' <see cref="Tensor.computeKernel"/> 持有它的一个实例作为当前生效的后端：
'
'   * 默认后端（<c>SIMDTensor</c>）：基于 System.Numerics.Vector 的 SIMD 加速 CPU 实现；
'   * 可选后端（<c>ILCuda.GPUTensor.CudaTensor</c>）：基于 ILCuda 的 GPU 实现。
'
' 下游深度学习代码只需要调用 <c>Tensor</c> / <c>Math</c> / <c>nn</c> 的公开 API，
' 不需要感知后端，即可通过一次 <c>Register()</c> 调用在 CPU / GPU 之间无损切换。
' ---------------------------------------------------------------------------

Namespace Compute

    ''' <summary>
    ''' 张量计算后端契约。
    ''' </summary>
    ''' <remarks>
    ''' 形状校验、广播等“语义边界”仍然由 <see cref="Tensor"/> 层负责，
    ''' 后端只需要实现纯粹的数值计算。后端应当是**无状态**（或线程安全）的，
    ''' 因为 <see cref="Tensor.computeKernel"/> 是一个全局共享变量。
    ''' </remarks>
    Public Interface ITensorCompute

        ''' <summary>
        ''' 后端名称，用于诊断输出（例如 "SIMD" / "CUDA"）。
        ''' </summary>
        ReadOnly Property Name As String

#Region "逐元素 - 二元"

        Function Add(a As Tensor, b As Tensor) As Tensor
        Function Subtract(a As Tensor, b As Tensor) As Tensor
        Function Multiply(a As Tensor, b As Tensor) As Tensor
        Function Divide(a As Tensor, b As Tensor) As Tensor
        Function Maximum(a As Tensor, b As Tensor) As Tensor
        Function Minimum(a As Tensor, b As Tensor) As Tensor

#End Region

#Region "逐元素 - 一元"

        Function Exp(t As Tensor) As Tensor
        Function Log(t As Tensor) As Tensor
        Function Sqrt(t As Tensor) As Tensor
        Function Square(t As Tensor) As Tensor
        Function Abs(t As Tensor) As Tensor
        Function Sin(t As Tensor) As Tensor
        Function Cos(t As Tensor) As Tensor
        Function Tanh(t As Tensor) As Tensor
        Function Sigmoid(t As Tensor) As Tensor
        Function Negate(t As Tensor) As Tensor
        Function Reciprocal(t As Tensor) As Tensor

        Function Relu(t As Tensor) As Tensor
        Function LeakyRelu(t As Tensor, alpha As Double) As Tensor
        Function Elu(t As Tensor, alpha As Double) As Tensor
        Function Gelu(t As Tensor) As Tensor
        Function Swish(t As Tensor) As Tensor

        ''' <summary>
        ''' 阶跃函数(Heaviside): 大于 0 的元素取 1, 否则取 0。
        ''' </summary>
        ''' <remarks>
        ''' 后端本身没有比较/掩码算子, 而 ReLU 系激活函数的反向传播需要的正是
        ''' `` (x &gt; 0) ? dy : 0 `` 这样的逐元素掩码; 有了本算子之后就可以写成
        ''' ``Heaviside(x)`` 与上游梯度逐元素相乘, 从而让整个反向过程同样可以下放到 GPU。
        ''' (命名为 Heaviside 而不是 Step, 是因为 ``Step`` 是 VB 的保留字)
        ''' </remarks>
        Function Heaviside(t As Tensor) As Tensor

#End Region

#Region "标量运算"

        Function AddScalar(t As Tensor, scalar As Double) As Tensor
        Function MultiplyScalar(t As Tensor, scalar As Double) As Tensor
        Function DivideScalar(t As Tensor, scalar As Double) As Tensor
        Function Pow(t As Tensor, exponent As Double) As Tensor
        Function Clip(t As Tensor, low As Double, high As Double) As Tensor

#End Region

#Region "矩阵运算"

        Function MatMul(a As Tensor, b As Tensor) As Tensor
        Function Transpose(t As Tensor) As Tensor

        ''' <summary>
        ''' 稀疏 × 稠密矩阵乘法：<c>dense[batch, Rows] · W[Rows, Columns] → [batch, Columns]</c>。
        ''' </summary>
        ''' <param name="csr">
        ''' CSR 稀疏连接矩阵（行 = 突触前，列 = 突触后）。SDK 后端可据此把 CSR 数组
        ''' 常驻显存并按 <see cref="SparseCsr.Version"/> 判定失效。
        ''' </param>
        ''' <param name="dense">稠密张量 <c>[batch, csr.Rows]</c>（例如上一时刻的脉冲矩阵）</param>
        ''' <returns>稠密张量 <c>[batch, csr.Columns]</c></returns>
        ''' <remarks>
        ''' 这是脉冲神经网络加载真实突触连接组（如 FlyWire，十万级神经元 / 千万级突触）
        ''' 的核心算子：稠密矩阵在此规模下不可行，必须走稀疏路径。
        ''' </remarks>
        Function SpMM(csr As SparseCsr, dense As Tensor) As Tensor

        ''' <summary>
        ''' 融合的稀疏递归 LIF 单步：一次调用完成整步脉冲动力学，并<b>就地</b>更新状态张量。
        ''' </summary>
        ''' <param name="synapses">突触连接矩阵 <c>W[Units, Units]</c>（行 = 突触前，列 = 突触后）</param>
        ''' <param name="sPrev">上一时间步的输出脉冲 <c>S[t−1]</c>（只读）</param>
        ''' <param name="externalCurrent">
        ''' 外部注入电流 <c>I_ext[t]</c>（只读）；<c>Nothing</c> 表示本步无外部刺激。
        ''' </param>
        ''' <param name="h">膜电位 <c>H[t−1]</c>，返回时被就地改写为 <c>H[t]</c></param>
        ''' <param name="s">输出参数：本步脉冲 <c>S[t]</c>（就地写入 0/1）</param>
        ''' <param name="counts">
        ''' 输出参数：脉冲计数累加器（就地累加 <c>S[t]</c>）。
        ''' <b>调用方必须在使用前清零</b>，且整个仿真窗内保持同一个张量即可获得
        ''' <c>Σ_t S[t]</c>——这消除了主机侧的 O(T·N) 累加循环。
        ''' </param>
        ''' <param name="beta">膜电位衰减系数 β ∈ (0,1)</param>
        ''' <param name="threshold">发放阈值 <c>U_thr</c></param>
        ''' <param name="subtractThreshold">
        ''' 复位模式：<c>True</c> 表示 <c>H[t] = U[t] − S[t]·U_thr</c>；
        ''' <c>False</c> 表示 <c>H[t] = U[t]·(1 − S[t])</c>（发放即清零）。
        ''' </param>
        ''' <returns>
        ''' <c>True</c> 表示整步已由本后端完成（调用方不得再走逐算子路径）；
        ''' <c>False</c> 表示本后端不提供该能力，由调用方回退。
        ''' </returns>
        ''' <remarks>
        ''' 语义与逐算子写法完全等价：
        ''' <code>
        '''   I[t] = W · S[t−1] + I_ext[t]
        '''   U[t] = β·H[t−1] + I[t]
        '''   S[t] = Θ(U[t] − U_thr)
        '''   H[t] = 复位(U[t], S[t])
        '''   counts += S[t]
        ''' </code>
        ''' 之所以把它做成<b>一个</b>算子而不是让调用方串起 SpMM / Add / MultiplyScalar / Heaviside，
        ''' 是因为逐算子写法在十万级神经元规模下会为每步分配 6 个 <c>[batch, Units]</c> 中间张量，
        ''' 并在 GPU 后端下产生 6~12 次显存往返（每次约 1.1 MB），
        ''' 往返开销会把内核的计算收益整个吃掉 —— 这是脉冲网络与稠密训练最本质的差异：
        ''' 它的状态张量只有 <c>[1, N]</c>，而依赖的是每步都要回灌的稀疏连接。
        ''' <para>
        ''' 就地更新意味着<b>设备为主副本</b>：后端若把状态留在显存里，主机数组会陈旧，
        ''' 调用方读主机内容之前必须调用 <see cref="SyncFromDevice"/>（或
        ''' <see cref="IsDevicePinned"/> 为 <c>False</c> 时按普通张量读取）。
        ''' </para>
        ''' </remarks>
        Function LifStep(synapses As SparseCsr,
                         sPrev As Tensor,
                         externalCurrent As Tensor,
                         h As Tensor,
                         s As Tensor,
                         counts As Tensor,
                         beta As Double,
                         threshold As Double,
                         subtractThreshold As Boolean) As Boolean

#End Region

#Region "批量细胞管线融合算子（Cella）"

        ' ------------------------------------------------------------------
        ' 这一组算子存在的理由：类器官 / 多细胞仿真里，每个细胞每步都要跑
        ' 一次「液态代谢网络 RK4 积分」与「先验调控图消息传递」。
        '
        ' 逐细胞调用时（1×122、339×34）规模低于 GPU 后端的回退阈值，
        ' 全部被判为"小算子"而落到 CPU；即使强行上 GPU，逐算子写法也会为
        ' 每步产生几十次显存往返，往返开销把计算收益吃光。
        '
        ' 因此把「同一步的全部存活细胞」当作 batch 维（峰值数千），
        ' 每个子网络压成一到两次内核启动，状态在显存里原地更新，
        ' 主机往返次数从"每细胞几十次"降到"每批几次"。
        '
        ' 约定与 LifStep 一致：返回 True 表示整步（或整个算子）已由本后端完成，
        ' 调用方不得再走逐算子路径；返回 False 时调用方回退到标量实现，
        ' 语义完全等价，只是慢一些。
        ' ------------------------------------------------------------------

        ''' <summary>
        ''' 融合的液态时间常数网络（LTC / LNN）批量 RK4 积分：<b>就地</b>推进状态。
        ''' </summary>
        ''' <param name="state">状态张量 <c>[B, m]</c>；返回时被就地改写为推进 <c>dt</c> 之后的状态</param>
        ''' <param name="input">区间内恒定的驱动输入 <c>[B, nIn]</c></param>
        ''' <param name="weightRecurrent">递归权重 <c>[m, m]</c></param>
        ''' <param name="weightInput">输入权重 <c>[nIn, m]</c></param>
        ''' <param name="bias">偏置 <c>[m]</c>；<c>Nothing</c> 表示无偏置</param>
        ''' <param name="weightGate">门控递归权重 <c>[m, m]</c>；<c>Nothing</c> 表示无门控（CT_RNN 模式）</param>
        ''' <param name="weightGateInput">门控输入权重 <c>[nIn, m]</c></param>
        ''' <param name="biasGate">门控偏置 <c>[m]</c></param>
        ''' <param name="tauEff">有效时间常数 <c>[m]</c>（已应用边界约束）</param>
        ''' <param name="dt">本次推进的总时长</param>
        ''' <param name="subSteps">子步数：每子步执行一次 RK4 步，子步长 = dt / subSteps</param>
        ''' <param name="activation">激活函数编码：0 线性 / 1 tanh / 2 sigmoid / 3 relu</param>
        ''' <returns><c>True</c> 表示整步已由本后端完成；<c>False</c> 表示调用方回退</returns>
        ''' <remarks>
        ''' 语义与 CPU 参考实现完全等价：
        ''' <code>
        '''   A     = act(h·Wrec + u·Win + b)
        '''   f     = σ(h·Wgate + u·WgateIn + bg)      （无门控时 f ≡ 0）
        '''   dh/dt = (1/τ_eff + f) ⊙ (A − h)
        ''' </code>
        ''' 就地更新意味着<b>设备为主副本</b>：调用方读主机数组之前必须调用
        ''' <see cref="SyncFromDevice"/>（或该张量未被钉住时按普通张量读取）。
        ''' </remarks>
        Function LtcRk4Batch(state As Tensor, input As Tensor,
                             weightRecurrent As Tensor, weightInput As Tensor, bias As Tensor,
                             weightGate As Tensor, weightGateInput As Tensor, biasGate As Tensor,
                             tauEff As Tensor, dt As Double, subSteps As Integer,
                             activation As Integer) As Boolean

        ''' <summary>
        ''' 融合的图卷积批量层（GEARS 消息传递；解码器等无邻居层复用同一算子）。
        ''' </summary>
        ''' <param name="x">节点特征 <c>[B·n, inF]</c>（批内第 b 个细胞的第 i 个节点 = 行 b·n+i）</param>
        ''' <param name="wSelf">自身分支权重 <c>[inF, outF]</c></param>
        ''' <param name="wRel">邻居分支权重 <c>[inF, outF]</c>；<c>Nothing</c> 表示忽略邻居项</param>
        ''' <param name="selfW">逐节点自身缩放 <c>[n]</c>；<c>Nothing</c> 表示全部取 1</param>
        ''' <param name="bias">偏置 <c>[outF]</c>；<c>Nothing</c> 表示无偏置</param>
        ''' <param name="edges">
        ''' 入边邻接（CSR）：<c>RowPointers[i]..RowPointers[i+1]</c> 给出节点 i 的入边，
        ''' <c>ColumnIndices</c> = 邻居（源）节点号，<c>Values</c> = 该边的合成系数。
        ''' 拓扑与系数对全部细胞共享，因此同一份 CSR 可服务整个 batch。
        ''' <c>Nothing</c> 表示无邻居（退化为全连接层）。
        ''' </param>
        ''' <param name="output">输出张量 <c>[B·n, outF]</c>（由本算子写入，可为未初始化张量）</param>
        ''' <param name="activation">激活函数编码：0 线性 / 1 tanh / 2 sigmoid / 3 relu</param>
        ''' <returns><c>True</c> 表示整层已由本后端完成；<c>False</c> 表示调用方回退</returns>
        ''' <remarks>
        ''' 计算式（与 GEARSConvLayer.Forward 的标量实现逐项对应）：
        ''' <code>
        '''   out[row, j] = act( selfW[i]·Σ_k x[row, k]·wSelf[k, j] + bias[j]
        '''                      + Σ_e coeff[e]·Σ_k x[src_e, k]·wRel[k, j] )
        ''' </code>
        ''' </remarks>
        Function GraphLayerBatch(x As Tensor, wSelf As Tensor, wRel As Tensor,
                                 selfW As Tensor, bias As Tensor, edges As SparseCsr,
                                 output As Tensor, activation As Integer) As Boolean

        ''' <summary>
        ''' 融合的节点特征拼装：<c>[x̄ ‖ p ‖ e_i ‖ z_pert]</c>。
        ''' </summary>
        ''' <param name="xNorm">逐节点控制表达 <c>[B·n]</c>（行优先）</param>
        ''' <param name="flag">逐节点扰动标记 <c>[B·n]</c></param>
        ''' <param name="embed">基因身份嵌入表 <c>[n, d]</c></param>
        ''' <param name="zPert">逐细胞的全局扰动向量 <c>[B, d]</c></param>
        ''' <param name="output">输出特征 <c>[B·n, 2+2d]</c></param>
        ''' <param name="n">节点数（基因数）</param>
        ''' <param name="d">嵌入维度</param>
        ''' <returns><c>True</c> 表示已由本后端完成；<c>False</c> 表示调用方回退</returns>
        Function GraphFeatureBatch(xNorm As Tensor, flag As Tensor, embed As Tensor, zPert As Tensor,
                                  output As Tensor, n As Integer, d As Integer) As Boolean

        ''' <summary>
        ''' 融合的通量读取头批量计算：<c>v[b, j] = e[b, j] ⊙ gsat([h ‖ u][b] · Wv[:, j] + bv[j])</c>。
        ''' </summary>
        ''' <param name="h">隐藏状态 <c>[B, m]</c>（代谢物浓度）</param>
        ''' <param name="u">网络输入 <c>[B, nIn]</c>（前 r 项为逐反应酶水平，其后为边界浓度）</param>
        ''' <param name="wFlux">读取权重 <c>[m+nIn, r]</c></param>
        ''' <param name="fluxBias">读取偏置 <c>[r]</c>；<c>Nothing</c> 表示无偏置</param>
        ''' <param name="reversible">逐反应可逆标记 <c>[r]</c>（非 0 表示可逆）；<c>Nothing</c> 表示全部不可逆</param>
        ''' <param name="output">输出通量 <c>[B, r]</c></param>
        ''' <returns><c>True</c> 表示已由本后端完成；<c>False</c> 表示调用方回退</returns>
        ''' <remarks>
        ''' 不可逆反应取 <c>σ(·)</c>，可逆反应取 <c>2σ(·)−1</c>（允许负通量表示逆向流动）；
        ''' 预激活按 ±30 夹断以避免 <c>exp</c> 溢出（与 CPU 侧 Clamp 一致）。
        ''' </remarks>
        Function FluxHeadBatch(h As Tensor, u As Tensor, wFlux As Tensor, fluxBias As Tensor,
                               reversible As Tensor, output As Tensor) As Boolean

        ''' <summary>
        ''' 融合的系统时间常数批量计算：<c>τ^sys = 1 / (1/τ_eff + f)</c>，
        ''' <c>f = σ(h·Wgate + u·WgateIn + bg)</c>（无门控时 <c>f ≡ 0</c>，退化为 τ_eff）。
        ''' </summary>
        ''' <param name="h">隐藏状态 <c>[B, m]</c></param>
        ''' <param name="u">网络输入 <c>[B, nIn]</c></param>
        ''' <param name="weightGate">门控递归权重 <c>[m, m]</c>；<c>Nothing</c> 表示无门控</param>
        ''' <param name="weightGateInput">门控输入权重 <c>[nIn, m]</c></param>
        ''' <param name="biasGate">门控偏置 <c>[m]</c></param>
        ''' <param name="tauEff">有效时间常数 <c>[m]</c></param>
        ''' <param name="output">系统时间常数 <c>[B, m]</c></param>
        ''' <returns><c>True</c> 表示已由本后端完成；<c>False</c> 表示调用方回退</returns>
        Function SystemTauBatch(h As Tensor, u As Tensor, weightGate As Tensor, weightGateInput As Tensor,
                                biasGate As Tensor, tauEff As Tensor, output As Tensor) As Boolean

#End Region

#Region "形状变换与选择"

        ''' <summary>
        ''' 沿指定轴截取 <c>[start, start + length)</c> 区间，返回独立的张量副本。
        ''' </summary>
        ''' <param name="t">输入张量（秩 &gt;= 1）</param>
        ''' <param name="axis">切片轴，支持负数（-1 表示最后一维）</param>
        ''' <param name="start">起始下标（含）</param>
        ''' <param name="length">截取长度</param>
        ''' <returns>与 <paramref name="t"/> 同秩、仅 <paramref name="axis"/> 维变为 <paramref name="length"/> 的新张量</returns>
        ''' <remarks>
        ''' 这是纯数据搬移算子，服务于 "只读取缓存前缀" 的场景（典型例子：KV Cache 按当前
        ''' 序列长度取出 K/V）。它不参与梯度计算，反向由调用方用散射类算子完成。
        ''' </remarks>
        Function Slice(t As Tensor, axis As Integer, start As Integer, length As Integer) As Tensor

        ''' <summary>
        ''' 沿指定轴拼接若干张量（除 <paramref name="axis"/> 外其余维度必须一致）。
        ''' </summary>
        ''' <param name="parts">待拼接的张量序列（至少一个，且秩相同）</param>
        ''' <param name="axis">拼接轴，支持负数</param>
        ''' <returns>沿 <paramref name="axis"/> 维长度为各输入该维长度之和的新张量</returns>
        ''' <remarks>
        ''' 既有的 <c>Transformer.TensorOps.ConcatLastDim</c> 只支持最后一维；本算子支持任意轴，
        ''' 是 KV Cache 沿 "序列维" 追加、以及多段 prompt 片段拼接的基础。
        ''' </remarks>
        Function Concat(parts As Tensor(), axis As Integer) As Tensor

        ''' <summary>
        ''' 沿最后一维取最大的 <paramref name="k"/> 个元素（按值降序）。
        ''' </summary>
        ''' <param name="t">输入张量（秩 &gt;= 1）</param>
        ''' <param name="k">保留的元素个数，取值范围 <c>[1, t.Shape(t.Rank - 1)]</c></param>
        ''' <param name="indices">
        ''' 输出参数：与返回值同形，元素为被选中元素在原始最后一维中的下标。
        ''' 与 <see cref="ArgMax"/> 保持一致，下标同样以 <c>Double</c> 存储。
        ''' </param>
        ''' <returns>形状与 <paramref name="t"/> 相同、仅最后一维变为 <paramref name="k"/> 的张量</returns>
        ''' <remarks>
        ''' MoE 路由器（专家打分 Top-K）与采样器（Top-k / Top-p）的共同前置步骤。
        ''' 纯选择算子，同样不参与梯度计算。
        ''' </remarks>
        Function TopK(t As Tensor, k As Integer, ByRef indices As Tensor) As Tensor

#End Region

#Region "训练算子"

        ' ------------------------------------------------------------------
        ' 这一组算子存在的理由：语言模型训练步里有两处"逐元素主机循环"，
        ' 在大词表 / 大参数下会成为绝对瓶颈 ——
        '
        '   * 掩码交叉熵：对 [rows, vocab] 做 softmax + NLL，
        '     12.8 万词表下即 3300 万次 exp；
        '   * AdamW：对每个参数的 4 个数组（param / grad / m / v）做逐元素更新。
        '
        ' 把这两个算子放进后端契约后，GPU 后端可以在设备端用融合内核完成，
        ' CPU 后端保留原有实现，调用方（LLM 训练器）无需分支。
        ' ------------------------------------------------------------------

        ''' <summary>
        ''' 带损失掩码的 softmax 交叉熵（语言模型训练的核心损失）。
        ''' </summary>
        ''' <param name="logits">形状 <c>[rows, vocab]</c> 的二维 logits</param>
        ''' <param name="targets">长度 <c>&gt;= rows</c> 的目标 token 下标</param>
        ''' <param name="mask">
        ''' 长度 <c>&gt;= rows</c> 的损失掩码；<c>Nothing</c> 表示全部计入。
        ''' <c>mask(r) = False</c> 的位置既不计入损失，梯度也保持 0。
        ''' </param>
        ''' <param name="dLogits">
        ''' 输出参数：形状同 <paramref name="logits"/>，
        ''' 内容为 <c>(softmax − onehot) / count</c>（未计入的位置为 0）。
        ''' </param>
        ''' <returns>平均到每个有效位置上的负对数似然；没有任何有效位置时返回 0</returns>
        ''' <remarks>
        ''' 这是 SFT 能成立的关键：只对 assistant 自己产出的 token 计损失，
        ''' 用户消息与工具返回结果不计 —— 模型要学的是"该怎么回应"。
        ''' </remarks>
        Function MaskedCrossEntropy(logits As Tensor,
                                    targets As Integer(),
                                    mask As Boolean(),
                                    ByRef dLogits As Tensor) As Double

        ''' <summary>
        ''' 试做一次 AdamW 参数更新。
        ''' </summary>
        ''' <param name="param">待更新的参数（可能被就地改写）</param>
        ''' <param name="gradient">梯度累加器；成功后由实现负责清零</param>
        ''' <param name="momentum">一阶矩估计（与参数同形）</param>
        ''' <param name="velocity">二阶矩估计（与参数同形）</param>
        ''' <param name="learningRate">学习率</param>
        ''' <param name="beta1">一阶矩衰减率</param>
        ''' <param name="beta2">二阶矩衰减率</param>
        ''' <param name="eps">数值稳定项</param>
        ''' <param name="biasCorrection1"><c>1 − beta1^step</c>（由调用方算好）</param>
        ''' <param name="biasCorrection2"><c>1 − beta2^step</c>（由调用方算好）</param>
        ''' <param name="weightDecay">解耦权重衰减系数；0 表示退化为纯 Adam</param>
        ''' <returns>
        ''' <c>True</c> 表示本轮更新已由本后端完成（调用方不得再走主机循环）；
        ''' <c>False</c> 表示本后端不提供该能力，由调用方回退。
        ''' </returns>
        ''' <remarks>
        ''' 之所以设计成"试做 + 返回布尔"而不是直接实现：主机侧的 AdamW 实现包含
        ''' 参数张量的读写语义（<c>MarkHostModified</c> 等），把它整体搬进后端会
        ''' 让契约承担超出"算子"的职责。让后端只负责"能不能替你做"，边界更清晰。
        ''' </remarks>
        Function TryAdamWStep(param As Tensor, gradient As Tensor,
                              momentum As Tensor, velocity As Tensor,
                              learningRate As Double, beta1 As Double, beta2 As Double, eps As Double,
                              biasCorrection1 As Double, biasCorrection2 As Double,
                              weightDecay As Double) As Boolean

        ''' <summary>
        ''' 是否支持把张量钉成设备常驻缓冲。
        ''' </summary>
        ''' <remarks>
        ''' 默认实现返回 <c>False</c>（纯 CPU 后端自然不需要这个概念）。
        ''' 只有 <c>True</c> 时调用方才应该去调用 <see cref="PinDevice"/>。
        ''' </remarks>
        ReadOnly Property SupportsDeviceResidency As Boolean

        ''' <summary>
        ''' 把张量钉成设备常驻缓冲，避免训练时每步重新上传。
        ''' </summary>
        ''' <param name="t">要被钉住的张量（其底层数组作为键）</param>
        ''' <param name="label">诊断用标签，例如 <c>layer3.moe.expert5.Wg</c></param>
        ''' <param name="zeroFill">
        ''' 是否用 0 初始化（梯度累加器与优化器状态用 <c>True</c>）。
        ''' </param>
        ''' <returns>钉住成功返回 <c>True</c>；后端不支持时返回 <c>False</c></returns>
        Function PinDevice(t As Tensor, label As String, zeroFill As Boolean) As Boolean

        ''' <summary>
        ''' 把张量钉成<b>双精度</b>设备常驻缓冲。
        ''' </summary>
        ''' <remarks>
        ''' <see cref="PinDevice"/> 钉出的是单精度缓冲（消费级显卡的 FP64 吞吐只有 FP32 的
        ''' 1/64，训练场景下矩阵乘占 95% 以上的浮点运算，因此设备侧统一降精度）。
        ''' 但脉冲网络的膜电位是<b>逐时间步累积</b>的递归状态：单精度舍入会改变阈值判定，
        ''' 让个别神经元在仿真窗内多发或少发一次脉冲，从而破坏「CPU / GPU 对拍」这类
        ''' 需要逐比特一致性的验收场景。需要这种保真度的调用方应当改用本方法，
        ''' 让状态张量以双精度留在显存里，同时仍然享受「零往返」。
        ''' <para>
        ''' 默认实现返回 <c>False</c>（CPU 后端不需要这个概念）；
        ''' 后端不支持时调用方应退回 <see cref="PinDevice"/> 或普通逐算子路径。
        ''' </para>
        ''' </remarks>
        ''' <param name="t">要被钉住的张量（其底层数组作为键）</param>
        ''' <param name="label">诊断用标签，例如 <c>lif.H</c></param>
        ''' <param name="zeroFill">是否用 0 初始化（累加器用 <c>True</c>）</param>
        ''' <returns>钉住成功返回 <c>True</c>；后端不支持时返回 <c>False</c></returns>
        Function PinDevice64(t As Tensor, label As String, zeroFill As Boolean) As Boolean

        ''' <summary>解除钉住并立即释放对应的显存（双精度 / 单精度通道一并解除）。</summary>
        Function UnpinDevice(t As Tensor) As Boolean

        ''' <summary>该张量当前是否已被钉住。</summary>
        Function IsDevicePinned(t As Tensor) As Boolean

        ''' <summary>当前钉住的显存总字节数（供显存占用报告使用）。</summary>
        ReadOnly Property PinnedDeviceBytes As Long

        ''' <summary>
        ''' 把设备常驻缓冲的内容回写到主机数组。
        ''' </summary>
        ''' <returns>该张量未被钉住、或后端不支持常驻时返回 <c>False</c></returns>
        ''' <remarks>
        ''' 被钉住的张量以<b>设备为主副本</b>，主机 <c>Data</c> 会逐渐陈旧。
        ''' 凡是需要"读主机内容"的场合（检查点落盘、主机侧统计、
        ''' 以及那些在主机循环里直接读权重的模块）都必须先调用本方法同步。
        ''' </remarks>
        Function SyncFromDevice(t As Tensor) As Boolean

#End Region

#Region "卷积与池化"

        ' ------------------------------------------------------------------
        ' 布局约定（全部为行主序 / channel-last，与 CNN 的 DataBlock 一致）:
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
        ' 完全同构，从而迁移时不需要任何转置/重排。
        ' ------------------------------------------------------------------

        ''' <summary>
        ''' 二维卷积（严格来说是互相关，与主流深度学习框架一致）。
        ''' </summary>
        ''' <param name="x">输入，形状 (N, H, W, C)</param>
        ''' <param name="filters">卷积核，形状 (KH, KW, C, OutC)</param>
        ''' <param name="bias">偏置，形状 (OutC)；传 Nothing 表示不加偏置</param>
        ''' <param name="stride">步长</param>
        ''' <param name="padding">四周零填充宽度</param>
        ''' <returns>输出，形状 (N, OH, OW, OutC)</returns>
        Function Conv2D(x As Tensor, filters As Tensor, bias As Tensor,
                        stride As Integer, padding As Integer) As Tensor

        ''' <summary>
        ''' 卷积反向：计算对输入的梯度。
        ''' </summary>
        ''' <param name="gradOutput">上游梯度，形状 (N, OH, OW, OutC)</param>
        ''' <param name="filters">前向使用过的卷积核，形状 (KH, KW, C, OutC)</param>
        ''' <param name="inputShape">前向输入的形状 (N, H, W, C)，用于确定输出形状</param>
        ''' <returns>对输入的梯度，形状 (N, H, W, C)</returns>
        Function Conv2DBackwardInput(gradOutput As Tensor, filters As Tensor,
                                     inputShape As Integer(), stride As Integer, padding As Integer) As Tensor

        ''' <summary>
        ''' 卷积反向：计算对卷积核的梯度。
        ''' </summary>
        ''' <param name="gradOutput">上游梯度，形状 (N, OH, OW, OutC)</param>
        ''' <param name="x">前向使用过的输入，形状 (N, H, W, C)</param>
        ''' <param name="filterShape">卷积核形状 (KH, KW, C, OutC)，用于确定输出形状</param>
        ''' <returns>对卷积核的梯度，形状 (KH, KW, C, OutC)</returns>
        Function Conv2DBackwardFilter(gradOutput As Tensor, x As Tensor,
                                      filterShape As Integer(), stride As Integer, padding As Integer) As Tensor

        ''' <summary>
        ''' 卷积反向：计算对偏置的梯度（沿 N / OH / OW 三个维度求和）。
        ''' </summary>
        ''' <param name="gradOutput">上游梯度，形状 (N, OH, OW, OutC)</param>
        ''' <returns>对偏置的梯度，形状 (OutC)</returns>
        Function Conv2DBackwardBias(gradOutput As Tensor) As Tensor

        ''' <summary>
        ''' 二维最大池化前向，同时输出每个输出位置所对应的<b>输入扁平下标</b>（供反向 scatter）。
        ''' </summary>
        ''' <param name="x">输入，形状 (N, H, W, C)</param>
        ''' <param name="size">池化窗口边长</param>
        ''' <param name="stride">步长</param>
        ''' <param name="padding">四周零填充宽度</param>
        ''' <param name="argMax">
        ''' 输出参数：形状 (N, OH, OW, C)，元素为获胜元素在<b>输入张量</b>之中的扁平下标
        ''' （即 <c>n*H*W*C + h*W*C + w*C + c</c>）。反向传播依赖它做梯度回填，
        ''' 因为每个输出位置唯一的对应一个输入位置，所以反向不需要原子操作。
        ''' </param>
        ''' <returns>输出，形状 (N, OH, OW, C)</returns>
        Function MaxPool2D(x As Tensor, size As Integer, stride As Integer, padding As Integer,
                           ByRef argMax As Tensor) As Tensor

        ''' <summary>
        ''' 二维最大池化反向：按前向记录的 <paramref name="argMax"/> 把梯度回填到输入位置。
        ''' </summary>
        ''' <param name="gradOutput">上游梯度，形状 (N, OH, OW, C)</param>
        ''' <param name="argMax">前向返回的输入扁平下标，形状 (N, OH, OW, C)</param>
        ''' <param name="inputShape">前向输入的形状 (N, H, W, C)</param>
        ''' <returns>对输入的梯度，形状 (N, H, W, C)</returns>
        Function MaxPool2DBackward(gradOutput As Tensor, argMax As Tensor, inputShape As Integer()) As Tensor

#End Region

#Region "归约运算"

        ''' <summary>沿指定轴（Nothing 表示整体）求和</summary>
        Function Sum(t As Tensor, axis As Integer?, keepdims As Boolean) As Tensor
        ''' <summary>整体求和</summary>
        Function SumAll(t As Tensor) As Double
        ''' <summary>沿指定轴（Nothing 表示整体）求均值</summary>
        Function Mean(t As Tensor, axis As Integer?, keepdims As Boolean) As Tensor
        ''' <summary>整体求均值</summary>
        Function MeanAll(t As Tensor) As Double
        Function Max(t As Tensor, axis As Integer?, keepdims As Boolean) As Tensor
        Function Min(t As Tensor, axis As Integer?, keepdims As Boolean) As Tensor
        Function Prod(t As Tensor, axis As Integer?) As Tensor
        ''' <summary>总体标准差</summary>
        Function StdDev(t As Tensor) As Double
        ''' <summary>L2 范数（欧几里得范数）</summary>
        Function L2Norm(t As Tensor) As Double
        Function ArgMax(t As Tensor, axis As Integer?) As Tensor
        Function ArgMin(t As Tensor, axis As Integer?) As Tensor

#End Region

#Region "神经网络"

        Function Softmax(t As Tensor, axis As Integer) As Tensor
        Function LogSoftmax(t As Tensor, axis As Integer) As Tensor
        Function SigmoidCrossEntropyWithLogits(labels As Tensor, logits As Tensor) As Tensor
        Function MseLoss(predictions As Tensor, targets As Tensor) As Tensor
        Function L2Loss(t As Tensor) As Tensor
        Function HuberLoss(predictions As Tensor, targets As Tensor, delta As Double) As Tensor

#End Region

    End Interface

End Namespace
