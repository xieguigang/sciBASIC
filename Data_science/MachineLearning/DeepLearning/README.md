# 深度学习框架：CNN、RNN、Transformer 与 ANN

## 引言

主流深度学习框架（PyTorch / TensorFlow）都依赖庞大的原生运行时。本包走另一条路：在**纯托管的张量运行时**之上，用 VB.NET 实现现代网络结构。

它的适用场景很明确：

- 需要**零原生依赖**部署（如受限环境、单文件发布）；
- 需要在 .NET 进程内**嵌入小模型**推理与训练；
- 需要**可读、可调试**的网络实现，而非黑箱调用。

## 支持的模型

| 模型 | 命名空间 | 说明 |
|---|---|---|
| **卷积网络（CNN）** | `CNN`（+ `data` / `layers` / `losslayers` / `trainers`） | 可训练 CNN，含数据容器、层实现、损失层与专用训练器 |
| **CeNiN 网络读取** | `Convolutional` | 导入 CeNiN 定义的网络结构 |
| **循环网络（RNN）** | `RNN` | 字符级循环网络，单元可配置 |
| **Transformer** | `Transformer` | 注意力层与 Transformer 模型 |
| **前馈网络（ANN）** | `NeuralNetwork` | 通用人工神经网络 |

## 训练器与优化器

`CNN.trainers` 与 `RNN` 相关命名空间提供训练循环，支持三种优化器：

| 优化器 | 特点 | 适用 |
|---|---|---|
| **SGD** | 简单，需仔细调学习率 | 小模型 / 教学 |
| **AdaGrad** | 逐维自适应，累积梯度平方 | 稀疏特征 |
| **Adam** | 动量 + 自适应（一阶 / 二阶矩估计） | **默认推荐** |

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.MachineLearning
Imports Microsoft.VisualBasic.MachineLearning.CNN
Imports Microsoft.VisualBasic.MachineLearning.CNN.trainers

' 1. 构建卷积网络
Dim net As New ConvolutionalNetwork()
Call net.AddLayer(New ConvolutionLayer(filters:=32, kernelSize:=3))
Call net.AddLayer(New PoolingLayer(size:=2))
Call net.AddLayer(New DenseLayer(units:=10))
Call net.Compile(loss:=New SoftmaxCrossEntropy())

' 2. 用 Adam 训练
Dim trainer As New SGDTrainer(optimizer:=Optimizer.Adam, learningRate:=0.001)

For epoch As Integer = 1 To 20
    Call trainer.TrainEpoch(net, trainData)
Next

' 3. 字符级 RNN / Transformer 用法类似，只是层类型不同
```

## 实现要点

- **为什么把 CNN 拆成 data / layers / losslayers / trainers 四个命名空间**：数据组织、层的正向反向、损失函数与训练调度是四件独立的事；分开后可以替换其中任意一环（换损失、换优化器）而不影响其余部分。
- **Adam 为什么通常是默认选择**：它同时利用一阶矩（动量，平滑方向）与二阶矩（自适应步长），对学习率不敏感，在多数任务上无需精细调参即可收敛。
- **纯托管方案的边界**：没有 cuDNN 级别的算子优化，因此训练大模型不现实；但用于小规模模型、教学与嵌入式推理完全够用。需要 GPU 时可叠加 `ILCudaTensor` 提供的 CUDA 后端。
- **Transformer 的注意力层**：注意力机制的核心是「按相关性加权聚合」；实现中把 QKV 投影、缩放点积注意力与前馈层分开，便于单独调试。

## 包信息

- Assembly：`Microsoft.VisualBasic.MachineLearning`
- TargetFramework：`net10.0`
- Tags：`scibasic;deep-learning;cnn;rnn;transformer;attention;adam-optimizer;neural-network;cenin`
- 许可：GPL-3.0-or-later
