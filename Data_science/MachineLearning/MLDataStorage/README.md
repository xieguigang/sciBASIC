# 机器学习数据集存储：DataPack、MNIST 与数据框导入

## 引言

机器学习的第一步不是模型，而是**把数据变成算法期望的形态**。这件事包含三个独立问题：

1. **格式转换**：数据框 / CSV → 归一化样本数据集；
2. **标准数据集读取**：直接读 MNIST 等基准数据；
3. **大样本存储**：样本量超过内存时如何存与随机访问。

本包为这三个问题各提供一种实现。

## 核心能力

| 能力 | 说明 |
|---|---|
| **数据框导入** | 把数据框转换为归一化的样本数据集（算法期望的形式） |
| **MNIST 读取** | 直接解析 IDX 图像与标签文件（无需任何转换步骤） |
| **DataPack / HDSPack 打包** | 把大量样本打包为可流式访问的单文件归档 |

## 为什么用 HDSPack 而不是「一堆文件」或「一个大文件」

| 方案 | 问题 |
|---|---|
| 一堆文件 | 拷贝易漏、传输易丢、小文件 IO 开销大 |
| 一个大文件（顺序） | 要取某个样本必须从头扫描 |
| **HDSPack 归档** | 单文件可携带，且支持**随机访问单个样本** |

这对「数据集放不进内存、需要按批读取」的训练场景是关键能力。

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.MachineLearning.DataStorage

' 1. 从数据框构建样本数据集
Dim dataset = SampleDataset.FromDataFrame(frame, normalize:=True)

' 2. 直接读取 MNIST
Dim (images, labels) = MnistReader.Read("./mnist/train-images-idx3-ubyte", "./mnist/train-labels-idx1-ubyte")

' 3. 打包为单文件归档（支持按样本随机访问）
Using pack As DataPack = DataPack.Create("./dataset.hds")
    For Each sample In dataset.Samples
        Call pack.AddSample(sample)
    Next
End Using

' 4. 训练时按批读取，无需全部载入内存
Using pack As DataPack = DataPack.Open("./dataset.hds")
    For Each batch In pack.ReadBatches(batchSize:=64)
        Call trainer.TrainBatch(batch)
    Next
End Using
```

## 实现要点

- **归一化为什么在这里做**：归一化参数（均值、标准差、最大最小值）必须**与数据集一起保存**，否则推理时无法复现同样的变换；把它放在数据层而非训练代码里，可以保证训练与推理一致。
- **MNIST IDX 格式**：MNIST 使用自定义的 IDX 二进制格式（含魔数、维度、像素 / 标签数据）；直接解析它避免了「先转到 CSV 再读」的无谓往返，也避免了精度与体积损失。
- **打包与训练内存的关系**：训练通常按小批量进行，因此数据「全量常驻内存」并非必需；单文件 + 随机访问的组合让内存占用与批大小挂钩，而非与数据集大小挂钩。

## 包信息

- Assembly：`Microsoft.VisualBasic.MachineLearning.DataStorage`
- TargetFramework：`net10.0`
- Tags：`scibasic;machine-learning;dataset;mnist;idx;data-storage;hdspack;archive`
- 许可：GPL-3.0-or-later
