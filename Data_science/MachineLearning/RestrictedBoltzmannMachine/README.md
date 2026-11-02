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
| `nn.rbm` | 单层 RBM（`RBM` / `BernoulliRBM`），带有可见层偏置 `a` 与隐藏层偏置 `b` |
| `nn.rbm.deep` | `DeepRBM`（多 RBM 堆叠）与**深度信念网络** `DeepBeliefNetwork` |
| `nn.rbm.learn` | 对比散度 CD-k 训练（同时更新权重与偏置）以及基于反向误差传播的训练 |
| `nn.rbm.factory` | 由配置构建网络 |
| `nn.rbm.save` | 历史上的序列化占位实现：请使用 `DeepBeliefNetwork.Save` / `DeepBeliefNetwork.Load` |
| `math`（+ `functions` / `distance` / `doubledouble`） | 稠密矩阵运算、激活与传递函数、距离函数，以及把两个 `Double` 组合为逐元素二元运算的函数集合 |
| `nlp`（+ `encode` / `ngram`） | 词编码接口与 n-gram 生成器 |
| `utils` | 辅助方法（`PrettyPrint`） |

## 关于 `doubledouble` 命名空间

`math.functions.doubledouble` 之中的函数是把两个 `Double` 作为输入的**逐元素二元运算**（加、减、乘、除），
它服务于矩阵的逐元素运算（`DenseMatrix.apply(m2, f)`），并不是扩展精度的数值类型。

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.MachineLearning.RestrictedBoltzmannMachine.math
Imports Microsoft.VisualBasic.MachineLearning.RestrictedBoltzmannMachine.nn.rbm
Imports Microsoft.VisualBasic.MachineLearning.RestrictedBoltzmannMachine.nn.rbm.deep

' 0. 训练数据：每一行是一个样本，元素取值为 0 / 1
Dim data = DenseMatrix.make(training)

' 1. 单层 RBM（可见层 784，隐藏层 256）与对比散度训练
Dim rbm As New BernoulliRBM(visibleUnits:=784, hiddenUnits:=256)
Dim error = rbm.Train(data, epochs:=20, learningRate:=0.01, k:=1)

' 2. 堆叠为深度信念网络，逐层贪心预训练
Dim dbn As New DeepBeliefNetwork()
Call dbn.AddLayer(rbm)                                  ' 也可以使用 AddLayer(784, 256) 添加随机初始化的层
Call dbn.AddLayer(visibleUnits:=256, hiddenUnits:=64)
Call dbn.TrainLayerWise(data, epochs:=20, learningRate:=0.01, k:=1)

' 3. 使用：均值场推断得到顶层特征，自顶向下重构或者采样生成
Dim features = dbn.Encode(data)
Dim restored = dbn.Reconstruct(data)
Dim samples = dbn.Generate(64, gibbsSteps:=100)

' 4. 保存与加载
Call dbn.Save("./model.rbm")
Dim model = DeepBeliefNetwork.Load("./model.rbm")
```

## 实现要点

- **RBM 的用途**：无监督特征提取（用隐藏层表示作为下游特征）、协同过滤、以及深度网络的**逐层预训练**（在标注数据稀缺时特别有价值）。
- **偏置项的作用**：能量函数为 `E(v,h) = -v·a - h·b - v·W·h`，缺少偏置的时候模型无法表达隐藏单元的开启先验，学到的特征会明显变弱；CD-k 会同时更新权重 `W` 与两个偏置 `a`、`b`。
- **k 步的作用**：k 越大，梯度估计越接近真值但越慢；k=1 的偏差在实践中可接受，这也是"CD-1"成为默认的原因。
- **逐层训练的意义**：深层 RBM 逐层贪心训练，每层只依赖前一层输出；这让训练分解为若干个可独立完成的小问题。
- **DBN 的推断与生成**：`Encode` 使用自底向上的均值场推断（返回概率而不是二值状态），`Decode`/`Reconstruct` 自顶向下传播；`Generate` 先在最顶层的 RBM 上做交替 Gibbs 采样，再把样本自顶向下传播到可见层。
- **输入要求**：可见层是伯努利单元，输入数据必须是取值为 `0/1` 的稠密矩阵；连续值需要先归一化并二值化（本库尚未实现 Gaussian 可见单元）。
- **训练监控**：`ReconstructionError` 与 `AverageFreeEnergy` 可以用于判断预训练是否收敛——训练良好的时候两者都应该下降。

## 包信息

- Assembly：`Microsoft.VisualBasic.MachineLearning.RestrictedBoltzmannMachine`
- TargetFramework：`net10.0`
- Tags：`scibasic;rbm;restricted-boltzmann-machine;contrastive-divergence;deep-belief-network;double-double-precision;unsupervised`
- 许可：GPL-3.0-or-later
