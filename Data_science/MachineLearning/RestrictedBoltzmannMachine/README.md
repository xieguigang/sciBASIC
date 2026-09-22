# 受限玻尔兹曼机与对比散度训练

## 引言

受限玻尔兹曼机（RBM）是一种**能量模型**：它给每种可见层-隐藏层联合配置赋予一个能量，概率与能量的关系是

$$P(v, h) \propto e^{-E(v,h)}$$

这类模型的训练目标是最大化观测数据的似然，但似然里包含一项**配分函数**（对全部配置求和），其规模是指数级的——这正是 RBM 的难点所在。

## 关键技术：对比散度（Contrastive Divergence）

既然精确梯度求不了，就用**采样近似**：

1. 从数据出发，做 k 步 Gibbs 采样（可见 → 隐藏 → 可见 → …）；
2. 用采样得到的「模型分布样本」近似期望；
3. 用两组期望之差更新权重。

实践中 **k=1** 就能得到可用的结果——这是 RBM 能实际训练起来的关键。

## 核心能力

| 命名空间 | 内容 |
|---|---|
| `nn.rbm` | 单层 RBM（Bernoulli 与 Gaussian 两种） |
| `nn.rbm.deep` | 堆叠为**深度信念网络**风格的深层结构 |
| `nn.rbm.learn` | 对比散度训练（含 Gibbs 采样步）与基于反向误差传播的训练 |
| `nn.rbm.factory` | 由配置构建网络 |
| `nn.rbm.save` | 训练权重序列化 |
| `math`（+ `functions` / `distance` / `doubledouble`） | 激活与传递函数、距离函数，以及**双精度-双精度（double-double）算术层** |
| `nlp`（+ `encode` / `ngram`） | 文本 → 词袋输入 |
| `utils` | 辅助方法 |

## 为什么需要 double-double 精度

RBM 的似然计算涉及大量 `log-sum-exp` 运算，且各配置的概率差异可达数十个数量级；普通 `Double` 在累加过程中会因**抵消误差**而失去有效位。`math.functions.doubledouble` 用两个 `Double` 表示一个更高精度的数（约 32 位有效十进制数字），在不引入任意精度库的前提下稳定住这些估计。

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.MachineLearning.RestrictedBoltzmannMachine

' 1. 构建单层 RBM（可见层 784，隐藏层 256）
Dim rbm As New BernoulliRBM(visibleUnits:=784, hiddenUnits:=256)

' 2. 对比散度训练
Call rbm.Train(data, epochs:=20, learningRate:=0.01, k:=1)

' 3. 堆叠为深度网络
Dim dbn As New DeepBeliefNetwork()
Call dbn.AddLayer(rbm)
Call dbn.AddLayer(New BernoulliRBM(visibleUnits:=256, hiddenUnits:=64))
Call dbn.TrainLayerWise(data)

' 4. 保存与加载
Call dbn.Save("./model.rbm")

' 5. 文本输入（可选）：先做词袋编码
Dim encoded = nlp.encode.EncodeBagOfWords(documents)
```

## 实现要点

- **RBM 的用途**：无监督特征提取（用隐藏层表示作为下游特征）、协同过滤、以及深度网络的**逐层预训练**（在标注数据稀缺时特别有价值）。
- **Gaussian RBM 的适用条件**：可见层表示连续值时（如归一化的像素强度、信号幅值）应使用 Gaussian 可见单元，否则二值假设会引入系统性偏差。
- **k 步的作用**：k 越大，梯度估计越接近真值但越慢；k=1 的偏差在实践中可接受，这也是"CD-1"成为默认的原因。
- **逐层训练的意义**：深层 RBM 逐层贪心训练，每层只依赖前一层输出；这让训练分解为若干个可独立完成的小问题。

## 包信息

- Assembly：`Microsoft.VisualBasic.MachineLearning.RestrictedBoltzmannMachine`
- TargetFramework：`net10.0`
- Tags：`scibasic;rbm;restricted-boltzmann-machine;contrastive-divergence;deep-belief-network;double-double-precision;unsupervised`
- 许可：GPL-3.0-or-later
