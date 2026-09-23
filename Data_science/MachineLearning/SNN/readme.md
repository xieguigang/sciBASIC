# SNN脉冲神经网络实现原理详解
脉冲神经网络（Spiking Neural Network, SNN）是模拟生物神经元通过"离散脉冲"进行信息传递与计算的第三代神经网络模型，与传统ANN最本质的区别在于：它用**时序上的二值脉冲序列**（spike train）替代了连续值的激活输出，并通过膜电位的动态积分—泄漏—阈值触发—复位机制完成计算，从而引入了时间维度和事件驱动的稀疏性。下面将从神经元模型、脉冲编解码、学习算法到整体信息流逐层展开。
---
## 一、生物神经元基础与LIF脉冲生成机制
生物神经元通过细胞膜内外的离子交换维持一个膜电位（membrane potential），当来自前突触的输入电流不断注入、使膜电位积累到一定阈值时，神经元便会"点火"，沿轴突发出一个动作电位（action potential，即脉冲），随后膜电位被重置并进入短暂的不应期。SNN用数学模型抽象这一过程，其中最常用的是**漏位积分—触发模型（Leaky Integrate-and-Fire, LIF）**，它在生物合理性与计算效率之间取得了平衡。
LIF神经元的连续时间微分方程为：
$$\tau \frac{dU(t)}{dt} = -U(t) + I(t)$$
其中 τ 是膜时间常数，U(t) 是膜电位，I(t) 是输入突触电流。在仿真中通常将其离散化为递归形式：
$$U[t+1] = \beta U[t] + W X[t+1] - R[t]$$
$$S[t] = \Theta(U[t] - U_{thr})$$
其中 β 是膜电位衰减系数（decay rate），W X[t] 是加权输入脉冲，Θ(·) 是 Heaviside 阶跃函数，R[t] 是发射脉冲后的复位项。
其完整动态可以拆解为四个阶段：
- **去极化（Integrate）**：输入脉冲 WX[t] 持续注入，使膜电位 U 上升；
- **泄漏（Leak）**：膜电位按 β 衰减，体现离子通道的被动耗散特性，β 越小遗忘越快；
- **阈值触发（Fire）**：当 U[t] 超过阈值 U_thr 时，神经元在该时间步发射一个脉冲 S[t]=1，否则为 0；
- **复位（Reset）**：发射后膜电位被强制拉回 V_reset（通常为 0 或低于静息电位），避免连续发放，模拟不应期。
这种离散递归形式与RNN高度同构——可以把LIF神经元视为一个具有隐式递归连接（膜电位衰减）和显式递归连接（脉冲复位反馈）的循环单元，从而可以直接借用RNN的反向传播框架进行训练。除LIF外，还有更精细的 IF（Integrate-and-Fire）、Synaptic（二阶）、Alpha、以及生物学上更精确但计算昂贵的 Hodgkin-Huxley、Izhikevich 模型，可根据任务复杂度选择。
---
## 二、脉冲编码与解码
SNN处理的是离散脉冲，因此输入的连续值必须先"编码"为脉冲序列，输出层的脉冲序列也要"解码"为分类或回归结果。常见编码方式对比如下：
| 编码方式 | 原理 | 信息载体 | 特点 |
|---------|------|---------|------|
| **频率编码（Rate Coding）** | 输入强度越大，对应神经元在时间窗内发放的脉冲数越多 | 单位时间内的脉冲计数 | 鲁棒性强、实现简单，但需要较长仿真时间才能统计频率，计算开销大 |
| **时间编码 / 时延编码（Temporal / Latency Coding）** | 输入强度越大，神经元发放首个脉冲的时刻越早（TTFS, Time-To-First-Spike） | 脉冲出现的精确时刻 | 稀疏、低功耗、响应快，但对噪声敏感 |
| **相位编码（Phase Coding）** | 以背景振荡为参照，脉冲落在振荡周期的不同相位代表不同数值 | 脉冲相对于振荡的相位 | 适合周期性信号，但依赖同步机制 |
| **成簇编码（Burst Coding）** | 通过一组快速连发的脉冲串（burst）携带信息 | 脉冲簇的频率与结构 | 兼顾速度与可靠性，介于频率与时间编码之间 |
研究表明，频率编码通常需要更长的训练延迟（~80ms）才能达到最高精度，而 TTFS 时间编码在 ~20ms 内即可收敛，体现出稀疏时间编码的速度优势。
解码端则常见两种做法：一是**计数解码**（统计输出层每个神经元在仿真窗内的脉冲数，取最大者为预测类别）；二是**膜电位解码**（直接读取输出层膜电位，避免脉冲稀疏带来的梯度问题）。
---
## 三、学习与训练方法
SNN的训练是其落地的核心难点，因为脉冲的 Heaviside 阶跃函数处处导数为 0，无法直接套用反向传播。主流训练方法分为三类，对比如下：
| 方法 | 核心思想 | 优点 | 局限 |
|------|---------|------|------|
| **STDP（脉冲时序依赖可塑性）** | 基于Hebb规则：若前突触脉冲先于后突触到达，突触增强（LTP）；反之减弱（LTD）。权重更新量随时间差呈指数衰减：Δw ∝ exp(-Δt/τ) | 生物合理性强、无需标签、支持在线无监督学习、适合神经形态硬件 | 难以扩展到深层监督任务，收敛性与精度有限 |
| **替代梯度** | 前向传播仍用不可导的阶跃函数产生脉冲；反向传播时用一段光滑可导函数（如 fast sigmoid、ATan、Sigmoid 等）近似替换阶跃函数的导数 | 兼容PyTorch等框架的自动微分，支持端到端反向传播，精度高 | 需要按时间步展开计算图，显存随时间步数线性增长；存在"死亡神经元"问题 |
| **ANN-to-SNN转换** | 先用ReLU ANN训练，再将ReLU激活近似为脉冲发放率，通过权重归一化与阈值标定把ANN映射成SNN | 复用成熟ANN训练流程，ImageNet级别精度可达 | 仿真时间步需求大、推理延迟高，难以发挥SNN原生时间动态的优势 |
**STDP** 的权重更新规则典型形式为：
$$\Delta w = \begin{cases} A_+ e^{-\Delta t/\tau_+}, & \Delta t = t_{post} - t_{pre} > 0 \\ -A_- e^{\Delta t/\tau_-}, & \Delta t < 0 \end{cases}$$
它是一种局部、无监督、事件驱动的Hebb式学习规则，与神经形态芯片天然契合。
**替代梯度** 是当前训练深度SNN的主流路径。其核心在于前向时严格使用 Heaviside 生成脉冲：
$$S[t] = \Theta(U[t] - U_{thr})$$
反向时则用替代函数 σ'(U) 替代 Θ'(U)（后者几乎处处为0），例如 fast sigmoid 形式 $\sigma'(U) = \frac{1}{(\alpha|U|+1)^2}$，通过调节陡度参数 α 在逼近精度与梯度消失之间权衡。snnTorch 等框架将 LIF 神经元封装成 PyTorch 模块，用户可像写 RNN 一样定义 `snn.Leaky(beta=alpha, spike_grad=surrogate.fast_sigmoid())`，并在时间步上展开后调用标准的 `loss.backward()` 与 `optimizer.step()`。
**ANN-to-SNN转换** 的思路是：把训练好的 ReLU ANN 的激活值解释为对应SNN神经元在一段时间内的发放率，于是 ReLU(x) ≈ firing_rate / max_rate，通过权重归一化（weight normalization）和阈值标定（threshold balancing）使SNN的脉冲统计量逼近原ANN的连续激活，从而几乎无损地迁移精度。近年来还有"差分编码"（Differential Coding）等改进，通过只传递增量脉冲降低转换后SNN的脉冲数量与能耗。混合训练策略也很常见——先做 ANN-to-SNN 转换得到良好初始化，再用替代梯度微调。
---
## 四、信息流：从输入到输出的整体流程
将上述模块串联起来，SNN一次完整前向推理的信息流如下：
1. **输入编码**：原始连续值数据（如图像像素、语音波形）经频率编码或时间编码转化为 T 个时间步上的脉冲序列 X[1], …, X[T]；
2. **逐层逐时间步推理**：对每一层 n 的 LIF 神经元，在每个时间步 t 执行：膜电位更新 U[t,n] = β·H[t-1,n] + W·X[t,n]；脉冲发放 S[t,n] = Θ(U[t,n] - u_th)；复位 H[t,n] = (1-S[t,n])⊙(γ·U[t,n]) + V_reset·S[t,n]；其中 H 是跨时间步保持的隐状态；
3. **跨层传播**：当前层脉冲 S[t,n] 作为下一层的输入脉冲 X[t,n+1]，整个网络在时间维度上"展开"为一个深层RNN计算图；
4. **输出解码**：仿真 T 步后，对输出层脉冲序列做计数或读取膜电位，得到最终预测；
5. **训练**：若是替代梯度路径，损失函数（如交叉熵）作用于输出脉冲序列，沿时间反向传播更新权重；若是 STDP，则在每个脉冲事件发生时局部更新突触；若是转换路径，则训练阶段完全在ANN上进行。
值得一提的是，由于SNN的状态在时间步之间持续，每次推理开始前需要初始化（reset）膜电位，这与ANN"无状态前向"的习惯不同。
---
## 五、主流框架与硬件平台
软件仿真框架主要有：基于 PyTorch 的 **snnTorch**（API友好、替代梯度完备，适合入门与学术研究）、**SpikingJelly**（旷视开源，中文文档完善，支持多种编码与替代梯度）、**BindsNET**（灵活可定制）、以及国内的 **BrainCog**（中科院自动化所开源，集成IF/LIF/Hodgkin-Huxley/Izhikevich多种神经元模型，支持STDP、STP、替代梯度等多尺度学习规则）。
硬件方面，SNN的真正威力在**神经形态芯片**上才能完全释放，因为这类芯片采用事件驱动架构——只有当脉冲到来时才进行乘加运算，未发脉冲的神经元几乎不耗能。代表平台包括 Intel **Loihi 2**、IBM **TrueNorth**、曼彻斯特大学的 **SpiNNaker / SpiNNaker 2**，以及集成了多芯片的 Intel **Hala Point**。在通用 GPU 上，SNN 需要按时间步展开计算图，显存随 T 线性增长，这是当前规模化训练的主要瓶颈。
---
## 小结与当前挑战
SNN的核心价值在于三点：**生物合理性**（更接近真实神经计算机制）、**时间动态性**（天然处理时序与事件流数据）、**能效比**（在神经形态硬件上可实现比GPU低几个数量级的功耗）。它已在事件相机视觉、低功耗边缘推理、神经形态机器人等场景展现出独特优势。
但SNN走向大规模应用仍面临若干挑战：脉冲函数的不可导使得深度训练困难、时间步展开带来显存与延迟开销、缺乏成熟的"ImageNet时刻"级基准、以及编码方案与超参（β、阈值、时间常数）对性能高度敏感等。替代梯度方法的成熟和 ANN-to-SNN 转换精度的提升正在逐步弥合 SNN 与 ANN 之间的精度差距，而神经形态硬件的演进则为其能效优势提供了物理载体——这两条路径的交汇，正是当前SNN研究最活跃的前沿。

# 基于 TensorFlow Tensor 对象实现 SNN 的可行性方案
TensorFlow 的 `tf.Tensor` 对象完全具备实现 SNN 的能力，但需要自行解决两个核心问题：**跨时间步的状态管理**（膜电位必须跨步传递）与**脉冲发放的不可导性**（Heaviside 阶跃函数导数处处为 0）。由于主流 SNN 框架（snnTorch、SpikingJelly、Norse）均基于 PyTorch，TensorFlow 生态下通常需要手写核心组件。
## 可行性分析：Tensor 与 SNN 计算的映射关系
SNN 的核心计算可以完全用 Tensor 表达，关键在于理解 Tensor 的角色分配：
- **`tf.Tensor`**：承载每个时间步的输入脉冲 `X[t]`、膜电位增量、输出脉冲 `S[t]` 等瞬时量；
- **`tf.Variable`**：承载跨时间步持续存在的状态——膜电位 `H`、突触电流（若使用二阶 LIF）、以及网络权重 `W`；
- **`tf.GradientTape`**：记录前向计算图，配合替代梯度实现端到端反向传播；
- **`@tf.custom_gradient`**：装饰器允许为不可导的 Heaviside 函数显式指定反向传播时的替代导数；
- **`tf.while_loop` 或 Python for 循环**：在时间维度上展开计算图（Eager 模式下用 Python 循环即可，图模式下必须用 `tf.while_loop`）。
Norse、SpikingJelly 等框架之所以未覆盖 TensorFlow，并非 TF 能力不足，而是社区生态选择的结果——Norse 明确要求 PyTorch 1.9+，SpikingJelly 的核心 API 也是基于 `torch.Tensor` 设计的。Rockpool 虽然支持多后端（JAX、PyTorch），但同样不包含 TF 后端。因此 TF 下实现 SNN 的现实路径是"手写 LIF 层 + 自定义梯度 + Keras 集成"。
## 模块一：脉冲编码层
将连续输入 `x ∈ [0,1]` 转换为时间步上的脉冲序列，最常用的是频率编码（泊松近似）：
```python
import tensorflow as tf
def rate_encode(x, T, dt=1.0):
    """频率编码：输入强度越大，时间窗内发放脉冲数越多
    x:  [batch, features] 连续输入，范围 [0,1]
    T:  仿真时间步数
    返回: [T, batch, features] 的脉冲序列
    """
    # 生成均匀随机数，与输入强度比较得到伯努利脉冲
    # 概率 p = x * dt
    rand = tf.random.uniform(shape=(T,) + x.shape, minval=0.0, maxval=1.0)
    spikes = tf.cast(rand < x * dt, tf.float32)
    return spikes
def latency_encode(x, T, tau=5.0, threshold=0.01):
    """时延编码：输入越强，首个脉冲发放时刻越早（TTFS）"""
    # 计算每个特征的首脉冲时刻：t_first = -tau * ln(x)
    t_first = -tau * tf.math.log(tf.clip_by_value(x, threshold, 1.0))
    # 生成 one-hot 时间脉冲
    t_steps = tf.range(T, dtype=tf.float32)
    # [T, batch, features] 的脉冲指示
    spikes = tf.cast(
        tf.abs(t_steps[:, None, None] - t_first[None, ...]) < 0.5, tf.float32
    )
    return spikes
```
## 模块二：LIF 神经元层（含状态管理）
这是 SNN 的核心。LIF 神经元在每个时间步执行四步操作：**积分 → 泄漏 → 触发 → 复位**。膜电位 H 必须作为 `tf.Variable` 在时间步之间传递：
```python
class LIFLayer(tf.keras.layers.Layer):
    """LIF 神经元层：支持跨时间步的状态保持"""
    def __init__(self, units, beta=0.9, threshold=1.0, **kwargs):
        super().__init__(**kwargs)
        self.units = units
        self.beta = beta          # 膜电位衰减系数
        self.threshold = threshold
    def build(self, input_shape):
        # 权重：[input_dim, units]
        self.W = self.add_weight(
            name="kernel",
            shape=(input_shape[-1], self.units),
            initializer="glorot_uniform",
            trainable=True,
        )
        # 膜电位状态：跨时间步持续
        self.H = self.add_weight(
            name="membrane",
            shape=(1, 1, self.units),  # 将在 call 中广播到 batch
            initializer="zeros",
            trainable=False,  # 状态不参与梯度，仅存数值
        )
    def reset_state(self):
        """每个 batch 开始前重置膜电位（SNN 的关键操作）"""
        self.H.assign(tf.zeros_like(self.H))
    def call(self, x_t, training=True):
        # x_t: [batch, input_dim]，单时间步输入
        # 1. 积分：输入电流注入
        I_t = tf.matmul(x_t, self.W)
        # 2. 泄漏：膜电位按 beta 衰减
        U_t = self.beta * self.H + I_t
        # 3. 触发：超过阈值发放脉冲
        S_t = spike_fn(U_t - self.threshold)  # 自定义脉冲函数
        # 4. 复位：发放后膜电位回到 0（软复位可通过减去阈值实现）
        H_new = U_t * (1.0 - S_t)
        # 更新状态
        self.H.assign(H_new)
        return S_t, U_t
```
## 模块三：替代梯度的 TF 实现
这是 TF 实现 SNN 最关键的技巧。前向传播时使用严格的 Heaviside 阶跃函数产生脉冲，反向传播时用光滑函数（如 fast sigmoid、ATan）替代其导数。通过 `@tf.custom_gradient` 显式指定这一行为：
```python
@tf.custom_gradient
def spike_fn(x):
    """前向：Heaviside 阶跃；反向：fast sigmoid 替代导数
    这是 SNN 可训练的关键——将 0/1 硬脉冲"伪装"成可导操作
    """
    s = tf.cast(x > 0.0, tf.float32)  # 前向：二值脉冲
    def grad(dy):
        # 反向：替代梯度 fast sigmoid: σ'(U) = 1 / (α|U| + 1)²
        alpha = 2.0
        surrogate = 1.0 / tf.pow(alpha * tf.abs(x) + 1.0, 2.0)
        return dy * surrogate
    return s, grad
```
也可以使用 ATan 作为替代函数，其梯度形式为 $\sigma'(U) = \frac{1}{\pi}\cdot\frac{1}{1+(\pi \alpha U/2)^2}$，在多数任务上表现更稳定：
```python
@tf.custom_gradient
def spike_fn_atan(x):
    s = tf.cast(x > 0.0, tf.float32)
    alpha = 2.0
    def grad(dy):
        surrogate = alpha / (2.0 * tf.constant(3.1415926)) / \
                    (1.0 + (tf.constant(3.1415926) / 2.0 * alpha * x) ** 2)
        return dy * surrogate
    return s, grad
```
若只想使用"直通估计器"（Straight-Through Estimator）的简化版本，可以配合 `tf.stop_gradient`：
```python
def spike_fn_ste(x):
    """前向用硬阶跃，反向用恒等梯度（最简单的替代方案）"""
    s = tf.cast(x > 0.0, tf.float32)
    return s + tf.stop_gradient(1.0 - s)  # 反向传播时梯度恒为 1
```
## 模块四：时间维度的循环展开
将上述组件串联起来，需要按时间步循环调用 LIF 层。Eager 模式下使用 Python for 循环即可，调试友好；若需导出为 `tf.function` 图模式，则要使用 `tf.while_loop`：
```python
class SNNModel(tf.keras.Model):
    """简单两层 SNN，用于 MNIST 等分类任务"""
    def __init__(self, T=20, **kwargs):
        super().__init__(**kwargs)
        self.T = T
        self.lif1 = LIFLayer(units=128, beta=0.9)
        self.lif2 = LIFLayer(units=10,  beta=0.9)
    def call(self, x, training=True):
        # x: [batch, features]，连续值输入
        # 1. 编码为脉冲序列：[T, batch, features]
        spikes_in = rate_encode(x, self.T)
        # 2. 重置所有 LIF 层状态（重要！每个 batch 需初始化）
        self.lif1.reset_state()
        self.lif2.reset_state()
        # 3. 记录输出层脉冲计数，用于解码
        out_spike_count = tf.zeros((tf.shape(x)[0], 10))
        # 4. 时间步循环
        for t in tf.range(self.T):
            s1, _ = self.lif1(spikes_in[t], training=training)
            s2, _ = self.lif2(s1, training=training)
            out_spike_count += s2  # 累积输出脉冲
        return out_spike_count  # 计数解码：取 argmax 即为预测类别
# 训练循环：标准 TF 范式
model = SNNModel(T=20)
optimizer = tf.keras.optimizers.Adam(1e-3)
loss_fn = tf.keras.losses.SparseCategoricalCrossentropy(from_logits=True)
@tf.function
def train_step(x, y):
    with tf.GradientTape() as tape:
        logits = model(x, training=True)
        loss = loss_fn(y, logits)
    grads = tape.gradient(loss, model.trainable_variables)
    optimizer.apply_gradients(zip(grads, model.trainable_variables))
    return loss
```
## 图模式 vs Eager 模式的差异
TensorFlow 的静态图机制对 SNN 是一把双刃剑。Eager 模式下用 Python for 循环展开时间步，代码直观、易于调试，但性能不如图模式；使用 `@tf.function` 装饰 `train_step` 时，TF 会将循环追踪为计算图，此时循环内的状态更新（`self.H.assign`）会被正确序列化，但如果循环内依赖 Python 侧的条件判断或动态形状，则需要改用 `tf.while_loop` 并显式传递状态张量：
```python
def simulate_T_steps(inputs, W1, W2, T, beta, threshold):
    """图模式下的 tf.while_loop 实现（适用于 XLA 编译加速）"""
    batch = tf.shape(inputs)[1]
    H1 = tf.zeros((batch, 128)); H2 = tf.zeros((batch, 10))
    out = tf.zeros((batch, 10))
    def body(t, H1, H2, out):
        s1_raw = tf.matmul(inputs[t], W1)
        U1 = beta * H1 + s1_raw
        S1 = spike_fn(U1 - threshold)
        H1 = U1 * (1.0 - S1)
        U2 = beta * H2 + tf.matmul(S1, W2)
        S2 = spike_fn(U2 - threshold)
        H2 = U2 * (1.0 - S2)
        return t + 1, H1, H2, out + S2
    _, H1, H2, out = tf.while_loop(
        lambda t, *_: t < T, body,
        [tf.constant(0), H1, H2, out]
    )
    return out
```
## TF 生态下的额外注意事项
**状态重置**：SNN 与 RNN 类似但更严格——每个 batch（甚至每个样本）开始前必须重置膜电位，否则前一个样本的"记忆"会污染当前推理结果。这是初学者最容易踩的坑。
**显存占用**：时间步展开后，反向传播需要保留所有时间步的中间激活（膜电位 U[t]、脉冲 S[t]），显存随 T 线性增长。T=20 时约为普通 ANN 的 20 倍，需通过减小 batch 或使用梯度检查点缓解。
**ANN-to-SNN 转换的替代路径**：若不想手写替代梯度，可以先用 TF 训练一个 ReLU ANN，再通过权重归一化将其转换为 SNN，这在 TF 下实现门槛更低，但推理延迟较长。
**XLA 加速**：对图模式下使用 `tf.while_loop` 的实现，可以启用 `jit_compile=True` 让 XLA 编译器融合算子，显著提升时间步循环的执行效率。
总体而言，TF 下实现 SNN 的推荐路径是：**Eager 模式 + Python for 循环 + `@tf.custom_gradient` 替代梯度 + Keras Layer 封装**，这套组合在开发效率与性能之间取得较好平衡，代码结构也最接近 snnTorch 的使用习惯，便于后续迁移到 PyTorch 生态。

---

# 稀疏自定义连接：加载真实突触连接组（FlyWire 风格）

前面各模块中，`SpikingNetwork.AddLayer()` 构建的是**全连接** LIF 层（稠密权重矩阵 `[in, units]`），适合手写的小规模网络。但要仿真真实大脑（例如 FlyWire 果蝇脑，十万级神经元、千万级突触），稠密矩阵在内存与算力上都完全不可行——真实连接组是**高度稀疏**的：每个神经元平均只与数百个神经元相连。

为此，本库新增了稀疏自定义连接能力：神经元之间的连接关系与强度完全由用户给定的**稀疏突触矩阵**决定，网络在同一层的神经元之间（含**循环连接**与**自反馈**）按生物突触结构逐时间步传播脉冲。

## 一、稀疏连接的数据模型（CSR）

新增 `SparseMatrix`（`SparseMatrix.vb`），采用 **CSR（Compressed Sparse Row）** 存储：

- 约定 `W[pre, post]`：**行 = 突触前神经元（pre）**，**列 = 突触后神经元（post）**；
- 内部存储 `rowPtr / colIdx / values` 三个数组，仅保存非零边；
- `FromTriplets(pre[], post[], weight[], rows, columns)` 由三元组建矩阵。它采用**两次稳定计数排序**（先按列、再按行）得到 `(row, col)` 字典序，再线性合并重复边——相同 `(pre, post)` 的边**按权重累加**。整体复杂度 `O(nnz + rows + columns)`，不使用哈希字典，避免千万级突触下巨大的内存开销；
- `SpMM(dense)` 计算稀疏 × 稠密：`X[batch, Rows] · W[Rows, Columns] → [batch, Columns]`，热路径直接操作底层 `Double()` 数组，并对 0/1 脉冲输入跳过零源以进一步加速。

## 二、权重归一化

真实连接组的权重通常是**突触计数**（syn_count），量级随神经元扇入/扇出剧烈变化，直接使用会导致膜电位尺度过大或过小。`SparseNormalization` 提供四种处理方式：

| 方式 | 含义 | 适用场景 |
|------|------|---------|
| `None` | 保留原始突触计数 | 需要保留绝对强度、自行控制阈值时 |
| `FanIn` | 按列（突触后）归一化：每个突触后神经元的入边权重和为 1 | **默认值**，最常用的 SNN 归一化 |
| `FanOut` | 按行（突触前）归一化：每个突触前神经元的出边权重和为 1 | 关注发放守恒时 |
| `GlobalMax` | 按全局最大（绝对）权重缩放 | 保持相对比例的整体缩放 |

## 三、单层稀疏递归仿真

新增 `SparseLIFLayer`（`SparseLIFLayer.vb`），复用与稠密 `LIFLayer` 完全相同的 LIF 四阶段动态，只是把输入电流改成**递归形式**：

```
I_rec[t] = W · S[t−1]          递归输入：上一时刻脉冲经稀疏矩阵回灌（含循环连接与自反馈）
I[t]     = I_ext[t] + I_rec[t] 叠加外部注入电流
U[t]     = β·H[t−1] + I[t]     泄漏积分
S[t]     = Θ(U[t] − U_thr)     阈值触发（二值脉冲）
H[t]     = 复位(U[t], S[t])    发放后复位
```

由于 `S[t−1]` 作为跨时间步状态，本层天然是一个脉冲递归网络，对连接矩阵中存在的循环连接与自反馈均正确建模——这正是真实连接组仿真的核心。

## 四、用法示例

```vb
Imports Microsoft.VisualBasic.DeepLearning.SpikingNeuralNetwork
Imports Microsoft.VisualBasic.MachineLearning.TensorFlow

' 1) 准备 FlyWire 风格三元组：pre / post 为神经元索引，weight 为突触计数
Dim pre() As Integer = {0, 1, 2, 5, 5, ...}
Dim post() As Integer = {3, 3, 7, 9, 9, ...}
Dim weight() As Double = {12.0, 4.0, 8.0, 3.0, 6.0, ...}

Dim N = 140000            ' 神经元总数
Dim T = 50                ' 仿真时间步

' 2) 构建网络并配置单层稀疏层（自动完成三元组→CSR + 扇入归一化）
Dim net As New SpikingNetwork(N, T, SpikeEncoding.RateCoding)
net.AddSparseLayer(pre, post, weight, N,
                   normalization:=SparseNormalization.FanIn,
                   beta:=0.9, threshold:=1.0)

' 3) 前向仿真：输入经频率编码后注入，返回各神经元在 T 步内的脉冲计数
Dim x = New Tensor(inputVec, 1, N)          ' 输入维度需等于 N（或提供 inputMap）
Dim counts = net.ForwardSpikes(x)           ' counts: [1, N]

' 4) 读取轨迹：每个时间步的输出脉冲 [batch, N]
Dim sHist = net.SparseLayer.SHistory
```

### 输入注入映射（inputMap）

当编码特征的维度小于神经元总数时，可用 `inputMap` 把第 `f` 个特征注入到指定神经元：

```vb
' 8 个输入特征 → 注入到神经元 4..11
Dim inputMap() As Integer = {4, 5, 6, 7, 8, 9, 10, 11}
Dim net As New SpikingNetwork(8, 30, SpikeEncoding.RateCoding)
net.AddSparseLayer(pre, post, weight, N, inputMap:=inputMap)
```

若省略 `inputMap`，则要求 `inputSize = N`（特征与神经元 1:1 对应）。

## 五、内存与性能

- 复杂度：`SpMM` 为 `O(nnz × batch)`/步，整段仿真 `O(T × nnz × batch)`；
- 内存：`O(nnz + 逐步中间张量)`。以 FlyWire 量级（nnz ≈ 千万）为例，CSR 仅需约 `nnz×(4+4+8) ≈ 160 MB`，而稠密矩阵将需要 `140000² × 8 ≈ 157 TB`——CSR 是唯一可行选择；
- 建议：仿真前固定 `batch`，并尽量使用 `SpikeEncoding.LatencyCoding`（每特征至多 1 个脉冲）以降低注入量。

## 六、限制与兼容性

- 稀疏连接层**仅支持前向仿真**（权重固定）。对稀疏网络调用 `TrainStep` / `ComputeGradients` 会抛出 `NotSupportedException`，明确提示"稀疏连接层当前仅支持前向仿真"，避免静默给出错误梯度；
- 稀疏层与全连接层**互斥**：已调用 `AddSparseLayer` 后不能再 `AddLayer`，反之亦然；
- 原有全连接多层网络的构建、BPTT 训练与推理行为**完全不变**（见 `test/test1.vb`、`self_test.vb` 仍通过）；
- 稀疏连接矩阵必须为**方阵**（pre/post 为同一神经元群）。

`test/test3.vb` 提供了完整可运行示例：包含 `SparseMatrix.SpMM` 与稠密 `MatMul` 的对拍自检、单层稀疏递归网络（含自反馈）的脉冲动力学仿真，以及 `inputMap` 注入演示。

---

# CUDA 加速：让稀疏连接组仿真跑在 GPU 上

前面的稀疏仿真默认在 CPU 上执行。当连接组规模达到 FlyWire 量级（十万级神经元、千万级突触）时，可以通过 GPU 加速。

## 一、设计：把稀疏乘法接入可插拔后端

`Tensor` 本就有一套**可插拔计算后端**机制：`Tensor.computeKernel`（契约 `ITensorCompute`）默认是 SIMD CPU 实现，`CudaTensor.Register()` 会一次性切换为 CUDA GPU 实现。

为此我们为后端契约补上了**稀疏算子**：

| 组件 | 位置 | 作用 |
|------|------|------|
| `SparseCsr` | `TensorFlow/Compute/SparseCsr.vb` | CSR 稀疏矩阵载体（行指针 / 列索引 / 权重 + 版本号），跨后端传输与显存缓存的依据 |
| `ITensorCompute.SpMM(csr, dense)` | `TensorFlow/Compute/ITensorCompute.vb` | 稀疏 × 稠密算子契约 |
| `TensorComputeBase.SpMM` | `TensorFlow/Compute/TensorComputeBase.vb` | 默认主机实现（SIMD / CUDA / 标量后端自动继承） |
| `CudaTensor.SpMM` + `spmm.cu` | `cuda/ILCudaTensor/` | CUDA 内核：按 `(batch, row)` 行并行，输出 `atomicAdd` 累加 |

`SparseMatrix.SpMM` 本身不含计算逻辑，只是**委托**给当前后端：

```vb
Return Tensor.computeKernel.SpMM(_csr, dense)
```

因此 CPU ↔ GPU 的切换对上层完全透明。

> 注意：**SNN 主库不依赖 CUDA**（依赖方向仍是 CUDA → TensorFlow）。只有需要 GPU 的调用方（驱动/测试工程）才引用 `ILCudaTensor`。

## 二、用法

```vb
Imports Microsoft.VisualBasic.Computing.ILCuda.GPUTensor
Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports Microsoft.VisualBasic.DeepLearning.SpikingNeuralNetwork

' 1) 构建稀疏网络（与 CPU 用法完全一致）
Dim net As New SpikingNetwork(N, T, SpikeEncoding.RateCoding)
net.AddSparseLayer(pre, post, weight, N,
                   normalization:=SparseNormalization.FanIn,
                   beta:=0.9, threshold:=1.0)

' 2) 尝试切换到 CUDA 后端；失败则保持 CPU（不会抛异常）
If CudaTensor.Register() Then
    Console.WriteLine($"后端 = {Tensor.computeKernel.Name}")   ' → CUDA
End If

' 3) 之后同样的调用就自动走 GPU 的稀疏内核
Dim counts = net.ForwardSpikes(x)

' 4) 用完可切回 CPU
CudaTensor.Unregister()
```

## 三、阈值与回退策略

| 属性 | 默认 | 含义 |
|------|------|------|
| `CudaTensor.MinSparseNnz` | 65536 | 稀疏矩阵非零数小于此值时 SpMM 回退 CPU（避免显存往返倒挂） |
| `CudaTensor.MinGpuElements` | 4096 | 逐元素算子走 GPU 的最小元素数 |

**只把 SpMM 放 GPU、LIF 留在 CPU**：把 `MinGpuElements` 调到极大（如 `Integer.MaxValue`），逐元素算子（泄漏积分、复位、注入）就会回退 CPU，只有稀疏 SpMM 走 GPU，从而避免每步逐元素算子的 PCIe 往返：

```vb
Dim saved = CudaTensor.MinGpuElements
CudaTensor.MinGpuElements = Integer.MaxValue   ' LIF 留在 CPU
CudaTensor.MinSparseNnz = 1                    ' 让 SpMM 走 GPU
' ... 仿真 ...
CudaTensor.MinGpuElements = saved
```

### 三之二、融合单步 + 设备常驻（推荐做法）

上面那个"拆开跑"的技巧解决的是**症状**：逐算子写法每步要分配 6 个 `[batch, Units]` 中间张量、并在 GPU 后端下产生 6~12 次显存往返（每次约 1.1 MB）。真正的解法是把整步压成**一次调用**：

```vb
' 契约：ITensorCompute.LifStep（ITensorCompute.vb）
'   I = W·S_prev + I_ext → U = β·H + I → S = Θ(U − U_thr) → H = 复位(U,S) → counts += S
Function LifStep(synapses, sPrev, externalCurrent, h, s, counts, beta, threshold, subtractThreshold) As Boolean
```

`SparseLIFLayer` 已经自动使用它（`UseFusedStep = True` 默认开启），无需调用方干预：

| 属性 | 默认 | 含义 |
|------|------|------|
| `SparseLIFLayer.UseFusedStep` | `True` | 优先走融合算子；置 `False` 强制逐算子路径（唯一区别是能拿到 `UHistory`） |
| `SparseLIFLayer.KeepHistory` | `True` | 是否逐步回读并保存 `SHistory`；置 `False` 后只维护设备端计数累加器（`Counts`），整段仿真零逐步回读 |
| `SparseLIFLayer.ResidentPrecision` | `Double64` | 设备常驻状态的精度档位（`Double64` = 与 CPU 逐位一致；`Single32` = 最快档，仅膜电位降精度） |
| `SparseLIFLayer.Counts` | — | `Σ_t S[t]`，融合路径下由**设备内核**累加；读之前需 `SyncFromDevice()` |
| `SparseLIFLayer.LastStepPath` / `FallbackSteps` | — | 诊断：最近一步走的路径（`Fused` / `OpByOp`）与回退步数 |

要点：

- **融合路径与逐算子路径数值等价**（相同的循环次序与结合次序），`Double64` 档下已实测**逐位一致**（`maxdiff = 0`）；
- 后端不支持常驻（CPU）时，融合实现直接就地写主机数组，同样省掉 5 个中间张量；GPU 下则把 `H`/`S_prev`/`S`/`counts` 钉成常驻缓冲，每步**零主机往返**；
- 任一前提不满足（显存不足、内核不可用、`nnz < MinSparseNnz`）时自动退回逐算子路径，功能不受影响；
- 解算计数时优先用 `SpikeDecoders.SpikeCounts(sparseLayer)` / `net.ForwardSpikes(x)`，它们直接读取层内累加器，不再需要 O(T·N) 的主机求和。

内核不可用（如 `atomicAdd(double)` 需要 sm_60+，或 NVRTC 编译失败）时，`CudaTensor` 会检测不到内核并**自动回退 CPU**，不会中断仿真。

## 四、缓存失效契约（务必遵守）

GPU 后端以「**主机数组引用 + 版本号**」缓存显存副本。任何**绕过 `Tensor` 索引器**的就地写入都必须声明失效，否则设备端会继续复用旧副本（静默错误）：

| 场景 | 需要调用 |
|------|---------|
| 就地修改 `Tensor.Data` | `tensor.MarkHostModified()`（或全局 `Tensor.InvalidateAllDeviceCaches()`） |
| 就地修改 `SparseCsr.Values` / `SparseMatrix.Normalize` | `SparseCsr.MarkModified()`（`Normalize` 内部已自动调用） |

SNN 内部已按此契约处理：`Network.ForwardSparse` 的计数累加与 `ScatterInput` 的注入散射在写完后都会 `MarkHostModified()`。

## 五、性能与显存

- **显存占用**：CSR 三数组为 `nnz × (4 + 4 + 8)` 字节；FlyWire 量级（nnz ≈ 千万）约 160 MB。CSR 会**常驻显存**（按引用 + 版本缓存，LRU，容量上限 `DefaultCacheBytes` = 1 GiB），并**显式释放**（ILCuda 显存无终结器）。融合算子的状态常驻缓冲（`H`/`S_prev`/`S`/`counts`）为 `4 × batch × N × 8` 字节（`N = 14万` 时约 4.5 MB），另有一片跨步复用的 `I` 缓冲同量级。
- **每步传输**（融合 + 常驻路径）：只有外部电流需要上传（`batch × N × 8`，恒流场景可用 `DirectCurrentEncode(..., shareBuffer:=True)` 复用同一份缓冲）；若 `KeepHistory = True` 则每步回读一次脉冲（`batch × N × 8`），`KeepHistory = False` 时**整段仿真零逐步回读**，只在结束时同步一次计数。
- **实测（本机 RTX A4000 / sm_86 / WDDM）**：
  - **全脑规模**（139,255 神经元、合并后 nnz = 3,732,460、T = 30、恒流编码、约 2~5% 发放率）：
    **CPU 51 ms vs GPU 4 ms ≈ 12.8×**（融合 + 双精度常驻 + 零逐步回读），
    逐神经元脉冲计数与 CPU **逐位一致**（max|Δ| = 0）；
  - 保留逐步轨迹的档位（每步回读一次脉冲张量）约 **2.1×** —— 瓶颈是单次 1.1 MB 的 D2H 拷贝
    （WDDM 上约 0.4~0.5 ms/步），而不是内核计算；需要逐步轨迹时建议接受这一代价，
    或把统计放在设备端做（当前实现见 `SparseLIFLayer.KeepHistory`）；
  - 融合 LIF 单步内核基准（`ILCudaTensor/test`，`N = 20,000`、`nnz = 1.28M`）：CPU 77.7 ms vs CUDA 5.84 ms ≈ **13.3×**；
  - 稀疏 SpMM 裸内核（`nnz = 1.28M`、稠密输入）：约 **4.3×**；
  - 单步成本分解（全脑规模，ms/步）：CPU `SpMM` 0.86 / 融合单步 1.27；
    GPU `SpMM`（公共算子，含回读）0.97 / **融合单步 0.06** / +同步回读 0.48 / +全新外部电流 0.40 ——
    可见 GPU 侧每步只剩 2 次内核启动与 0~1 次小传输，主机侧的开销（散射、统计、GC）才是主要项。
- **一个重要的性能事实**：脉冲输入**高度稀疏**，CPU 的 SpMM 会**跳过零源**（`xv = 0`），因此在中低发放率下 CPU 相当有竞争力；GPU 的收益主要来自"消除每步的显存往返"，而不是内核算得比 CPU 快。经验做法：先用 `SparseLIFLayer.LastStepPath` 确认走的确实是融合路径，再按实际发放率实测后决定是否启用。
- **两处踩过的坑（已修）**：`DeviceBuffer(Of T).Fill` 过去是"建主机数组 + H2D 全量拷贝"，而清零出现在每个 atomicAdd 型内核的启动前（1.1 MB/次），已改为设备端 `memset`；`BrainNetwork.SetGain` 过去无条件重写权重，而重写会让设备端 CSR 缓存失效、把一次 60 MB 的上传塞进仿真计时窗口，现已改为幂等。

## 六、验证

| 测试 | 内容 |
|------|------|
| `cuda/ILCudaTensor/test/Program.vb` | 稀疏 SpMM 的 CPU vs CUDA 逐元素对拍、`MarkModified` 后显存缓存同步、**融合 LIF 单步的双精度 / 单精度档对拍与性能参考** |
| `SNN/test/test4.vb` | 同一稀疏网络的 CPU / GPU 端到端前向对拍（`LatencyCoding` 确定性编码保证输入一致），并在无 CUDA 时安全跳过 |

两者均未破坏既有断言与演示（全连接训练、`SparseDemo`、`RecurrentLayerSelfCheck` 等照常通过）。

---

# 附：项目代码结构、关键 API 与快速上手

> 以上正文解释了 SNN 的数学原理与算法细节；本节从**代码实现**的角度说明 `Microsoft.VisualBasic.DeepLearning.SpikingNeuralNetwork` 这个程序集的组织方式与使用入口。

## 项目结构与模块地图

程序集 `Microsoft.VisualBasic.DeepLearning.SpikingNeuralNetwork`（根命名空间同名）由以下类型构成：

| 分类 | 类型 | 职责 |
|---|---|---|
| 神经元层 | `LIFLayer` | 基础漏位积分—触发层（膜电位积分、阈值触发、复位与不应期） |
| | `RecurrentLIFLayer` | 带递归连接的 LIF 层，用于循环 / 时序拓扑 |
| | `SparseLIFLayer` | 稀疏连接 LIF 层，突触权重以 CSR（`SparseMatrix`）存储 |
| 编解码 | `Encoder` | 把连续值编码为脉冲序列（群体编码 / 速率编码 / 延迟编码） |
| | `Decoder` | 把脉冲序列还原为连续输出（速率读出等） |
| 学习规则 | `STDP` | 脉冲时序依赖可塑性，用于无监督的局部学习 |
| | `Surrogate` | 代理梯度（surrogate gradient），使离散脉冲发射可反向传播 |
| | `RegressionLosses` | 面向回归任务的损失函数 |
| 网络装配 | `Network` | 组装各层并驱动前向传播（含 `ForwardSparse`） |
| | `LinearReadout` | 最终线性读出层 |
| 基础设施 | `SparseMatrix` | CSR 稀疏矩阵容器（行指针 / 列索引 / 值） |
| | `TensorHelper` | 张量形状变换与共享工具方法 |

依赖关系：`Microsoft.VisualBasic.Core`（基础库）、`Math`（数值与统计）、`TensorFlow`（张量运行时）；稀疏路径在启用 GPU 时会走 `ILCudaTensor` 提供的 `spmm` 内核。

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.DeepLearning.SpikingNeuralNetwork

' 1. 搭建网络：编码器 → LIF 层 → 线性读出
Dim net As New Network()
' ...（按需添加 LIFLayer / SparseLIFLayer / LinearReadout 等层）

' 2. 把连续输入编码成脉冲序列
'    Encoder 支持速率编码、群体编码与延迟编码等多种方案
Dim spikes = Encoder.Rate(x, steps:=T)

' 3. 前向传播（稀疏连接时走 ForwardSparse）
Dim y = net.Forward(spikes)

' 4. 训练：代理梯度负责反向传播，STDP 负责局部无监督学习
```

## 显存与缓存的正确用法

本实现在「主机张量」与「设备张量」之间采用**显式失效**契约：

| 场景 | 需要调用的方法 |
|---|---|
| 就地修改 `Tensor.Data` | `tensor.MarkHostModified()`（或全局 `Tensor.InvalidateAllDeviceCaches()`） |
| 就地修改 `SparseCsr.Values` / `SparseMatrix.Normalize` | `SparseCsr.MarkModified()`（`Normalize` 内部已自动调用） |

`Network.ForwardSparse` 的计数累加与 `ScatterInput` 的注入散射在写完后都会自动调用 `MarkHostModified()`，因此正常使用无需手工干预。

## 何时启用 GPU

脉冲输入天然高度稀疏，CPU 侧 SpMM 会跳过零源，因此在中低发放率下 CPU 往往更有竞争力；GPU 每次调用存在固定开销（内核启动 + 显存往返，约 0.6 ~ 1 ms/步）。经验结论是：需要足够大的「每步非零计算量」（**高发放率 × 大规模 nnz**）才能摊薄开销。建议按实际发放率实测后再决定是否启用。

## 包信息

- Assembly：`Microsoft.VisualBasic.DeepLearning.SpikingNeuralNetwork`
- 目标框架：`net10.0`；平台：`AnyCPU;x64`
- 关键属性：`OptionStrict=Off`、`OptionExplicit=On`、`ImplicitUsings=enable`
- 许可：GPL-3.0-or-later
