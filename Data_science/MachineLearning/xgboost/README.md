# XGBoost 模型预测器与梯度提升树训练器

## 引言

XGBoost 是表格数据上最常用的强模型之一。本包提供**两个方向的完整能力**：

| 方向 | 能力 | 价值 |
|---|---|---|
| **推理** | 读取并评分 XGBoost 模型文件 | 在 .NET 进程内使用别处训练好的模型，**无需原生依赖** |
| **训练** | 训练三元梯度提升树 | 在 .NET 内完成建模范式，含类别特征与缺失值处理 |

## 支持的 booster

| Booster | 特点 |
|---|---|
| **GBTree** | 经典梯度提升树（默认） |
| **GBLinear** | 线性 booster（特征维度高、样本少时） |
| **Dart** | 引入 dropout 的树提升，抗过拟合 |

推理侧会解码模型的完整结构：**树结构、分裂条件与叶子值**——因此不依赖任何 XGBoost 原生库。

## 命名空间地图

| 命名空间 | 职责 |
|---|---|
| `Microsoft.VisualBasic.MachineLearning.XGBoost`（根） | 模型读取与预测入口 |
| `....XGBoost.gbm` | 梯度提升机核心 |
| `....XGBoost.learner` | learner 抽象 |
| `....XGBoost.tree` | 决策树基学习器 |
| `....XGBoost.train` | TGBoost 训练实现（三元树） |
| `....XGBoost.config` | 参数模型（学习率、子采样、正则化） |
| `....XGBoost.util`（+ `FVecArray`） | 特征向量与工具方法 |
| `....XGBoost.spark` | Spark 集成类型 |

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.MachineLearning.XGBoost

' 1. 读取别处训练好的模型并预测
Dim model = XGBoostModel.Load("./model.json")
Dim score = model.Predict(features)

' 2. 在 .NET 内训练（TGBoost，显式处理类别列与缺失值）
Dim config As New XGBoostConfig With {
    .LearningRate = 0.1,
    .MaxDepth = 8,
    .Subsample = 0.8,
    .Lambda = 1.0
}

Dim booster = TGBoost.Train(config, trainData)
Dim prediction = booster.Predict(testData)
```

## 实现要点

- **为什么要在 .NET 内实现推理**：生产环境部署原生 XGBoost 往往受限于平台二进制、版本匹配与许可合规；纯托管推理只需一个 DLL，且行为完全可预测。
- **Booster 的选择依据**：树模型（GBTree / Dart）适合非线性与特征交互；线性 booster 在高维稀疏特征（如文本 TF-IDF）上更合适，且更易解释。
- **Dart 的 dropout 机制**：随机丢弃一部分已建成的树参与预测，从而抑制后续树对前序树的过度依赖，提升泛化。
- **类别特征与缺失值**：XGBoost 的一大优势是**内置处理**这两类问题——类别列无需 one-hot（避免维度爆炸），缺失值有默认分支（无需插补）。
- **与 `XGBoostDataSet` 的分工**：本包负责「模型」，训练数据的准备（CSV 读取、类别发现、缺失值索引）由 `XGBoostDataSet` 包完成。

## 包信息

- Assembly：`Microsoft.VisualBasic.MachineLearning.XGBoost`
- TargetFramework：`net10.0`
- Tags：`scibasic;xgboost;gradient-boosting;gbtree;gblinear;dart;decision-tree;predictor;classifier`
- 许可：GPL-3.0-or-later
