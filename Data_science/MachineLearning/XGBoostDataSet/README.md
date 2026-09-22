# XGBoost 训练集 CSV 数据加载器

## 引言

把 CSV 变成 XGBoost 能吃的训练数据，中间有三件容易被低估的事：

1. **发现类别列**：字符串列需要映射为类别索引，但必须先扫描一遍才知道有多少个水平；
2. **定位缺失值**：空单元格、`NA`、`NaN` 等都需要统一标记为缺失（XGBoost 要据此走默认分支）；
3. **区分训练 / 测试 / 验证集**：它们的类别水平必须一致，否则索引无法对齐。

本包用**两遍扫描**解决这些问题，并产出带标签的 `TrainData`。

## 两遍扫描的必要性

| 遍 | 目标 |
|---|---|
| **第一遍** | 扫描所有行：发现类别列的取值集合、定位缺失值、统计列数 |
| **第二遍** | 在已知类别索引的前提下，把数据物化为数值矩阵 |

如果只扫一遍，就无法在遇到新类别水平时回溯修正之前的索引——这正是两遍扫描不可省的原因。

## 核心能力

| 能力 | 说明 |
|---|---|
| **两遍 CSV 读取** | 分别为测试表与验证表提供读取器 |
| **类别列转换** | 把字符串列转换为类别索引 |
| **缺失值索引** | 记录每行 / 每列的缺失位置，供 booster 走默认分支 |
| **TrainData 构建** | 产出带标签的训练数据对象 |

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.MachineLearning.XGBoost.DataSet

' 1. 读取训练 / 测试 CSV（两遍扫描：先发现类别与缺失，再物化矩阵）
Dim train As TrainData = CsvLoader.LoadTraining("./train.csv", labelColumn:="y")
Dim test As TrainData = CsvLoader.LoadTesting("./test.csv", categories:=train.Categories)

' 2. 交给 TGBoost 训练
Dim config As New XGBoostConfig With {.LearningRate = 0.1, .MaxDepth = 8}
Dim booster = TGBoost.Train(config, train)

' 3. 评估
Dim predictions = booster.Predict(test)
```

## 实现要点

- **为什么测试集要传入训练集的类别定义**：若测试集独立发现类别，遇到训练集中未出现过的水平会产生无法对齐的索引；以训练集的类别定义为基准，未登录类别统一归入「未知」桶，才能保证模型输入维度一致。
- **缺失值不插补的理由**：XGBoost 在分裂时会为缺失值学一个默认方向——这往往比均值插补更准确，因为「缺失」本身可能就是有信息的（例如某项检查未做，本身就与结果相关）。
- **与 `XGBoostDataSet` 定位一致的分工**：本包只做**数据准备**，不涉及模型；这样同一份数据可以在不同超参实验间复用，避免重复解析 CSV。

## 包信息

- Assembly：`Microsoft.VisualBasic.MachineLearning.XGBoost.DataSet`
- TargetFramework：`net10.0`
- Tags：`scibasic;xgboost;dataset;csv;data-preprocessing;categorical-encoding;missing-values;train-data`
- 许可：GPL-3.0-or-later
