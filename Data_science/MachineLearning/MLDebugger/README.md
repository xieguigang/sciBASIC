# 机器学习调试器：误差曲线、ROC 与样本框导出

## 引言

训练神经网络时，最让人头疼的不是「效果不好」，而是**不知道为什么不

好**：

- 是还没收敛？
- 是学习率太大在震荡？
- 还是模型容量不足、已经过拟合？

回答这些问题需要**训练过程的可观测性**。本包的作用就是：不只报告最终损失，而是把训练的**动态过程**变得可检查、可绘图。

## 核心能力

| 能力 | 说明 |
|---|---|
| **误差曲线** | 从训练日志中提取逐轮适应度 / 神经元取值，画出误差曲线 |
| **ROC 验证曲线** | 计算分类模型的 ROC 用于评估与阈值选择 |
| **演示样本集构建** | 快速生成小规模样本用于实验 |
| **XML 数据集转表格** | 把 XML 格式数据集转成表格行，便于统一处理 |
| **样本框导出** | 导出训练过程中的样本框（batch）数据，便于逐批核对 |

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.MachineLearning.Debugger

' 1. 训练过程中把日志写入 netCDF
Call trainer.Train(net, data, log:="./training.nc")

' 2. 从日志提取误差曲线 / 适应度曲线
Dim curve = TrainingLog.ErrorCurve("./training.nc")
Call curve.SaveCsv("./error_curve.csv")

' 3. 提取神经元取值帧（观察是否出现饱和 / 死亡）
Dim frames = TrainingLog.NeuronFrames("./training.nc")

' 4. 分类模型验证
Dim roc = Validation.RocCurve(net, validationSet)
Call roc.Save("./val_roc.png", width:=800, height:=800)
```

## 实现要点

- **为什么要看「曲线」而不是「最终值」**：同样一个最终损失，可能来自「平稳收敛」也可能是「大幅震荡后的偶然取值」；只有曲线能区分这两种情况，而它们对应的解决方案完全不同。
- **神经元取值的诊断价值**：如果某层神经元的输出长期为 0（ReLU 死亡）或长期饱和（Sigmoid 两端），梯度会消失——这是「模型不收敛」的常见根因之一，且**只能通过观察中间层取值发现**。
- **ROC 用于验证而非训练**：训练集上的 ROC 会乐观偏置；验证集 ROC 才反映泛化能力，也用于确定分类阈值。
- **与 `xlsx` / 绘图包的衔接**：导出的 CSV 可直接交给电子表格或 `plots_extensions` 的 ROC 绘图，无需额外转换。

## 包信息

- Assembly：`Microsoft.VisualBasic.MachineLearning.Debugger`
- TargetFramework：`net10.0`
- Tags：`scibasic;machine-learning;debugger;roc;error-curve;fitness-curve;training-log;diagnostics`
- 许可：GPL-3.0-or-later
