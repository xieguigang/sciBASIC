# 面向数值机器学习的特征向量编码器

## 引言

真实数据表几乎从不是「纯数值」的：有字符串列（物种名、类别）、有布尔列（是否对照组）、有需要分箱的连续列。「把数据表直接喂给模型训练」这个看似简单的需求，实际上要求把混合类型的列**全部展开成数值指示列**。

本包就是这道工序的实现：**注册规则 → 逐列展开 → 输出纯数值数据框**。

## 设计目标

- **逐列规则**：可以为任意具名字段注册编码器，把该列展开为一列或多列数值指标；
- **自动模式**：检查每列的数据类型并自动选择编码器，替换原列；
- **可插拔**：抽象基类 `FeatureEncoder` 让新增编码方式只需实现一个方法。

## 核心特性

- **逐列编码规则**：为任意具名字段注册 `FeatureEncoder`，把该列展开为一列或多列数值指标列；
- **字符串列**：每个不同水平生成一个指标列（`EnumEncoder`，即 one-hot / 指示编码）；
- **布尔列**：编码为数值标志列（`FlagEncoder`）；
- **数值列**：原样通过（`NumericEncoder`），或按等宽分箱离散为 N 个带标签的水平（`NumericBinsEncoder`，基于 `Discretizer`）并每箱生成一个指标列；
- **自动模式**：检查每列数据类型并选择合适编码器，用生成的数值块替换原列。

## 关键类型与 API

| 类型 | 职责 |
|---|---|
| `Encoder` | 编排器：`AddEncodingRule`、`Encoding(data As DataFrame)`、共享的 `AutoEncoding` 与 `Encode(feature As FeatureVector)` |
| `FeatureEncoder` | 抽象基类：定义 `Encode(feature As FeatureVector) As DataFrame` 与共享的 `IndexNames` 辅助 |
| `EnumEncoder` | 把字符串特征展开为「每个因子水平一列 0/1」 |
| `FlagEncoder` | 把布尔特征编码为数值标志列 |
| `NumericEncoder` | 数值特征（Single / Double / Short / Integer / Long）原样通过 |
| `NumericBinsEncoder` | 把数值特征分为 `nbins` 个水平，每箱生成一个指标列 |

以上类型均位于根命名空间 `Microsoft.VisualBasic.DataMining.FeatureFrame`。

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.Data.Framework
Imports Microsoft.VisualBasic.DataMining.FeatureFrame

' 显式指定逐字段规则
Dim enc As New Encoder()
Call enc.AddEncodingRule("species", New EnumEncoder())
Call enc.AddEncodingRule("isControl", New FlagEncoder())
Call enc.AddEncodingRule("weight", New NumericBinsEncoder(nbins:=5))

Dim numeric As DataFrame = enc.Encoding(frame)

' 或者交给自动模式
Dim auto As DataFrame = Encoder.AutoEncoding(frame)
```

## 实现要点

- **为什么要「展开」而不是「映射为整数」**：把类别映射为 0/1/2 会引入**虚假的顺序关系**（模型会认为 2 比 0「更大」）。指示列（one-hot）消除了这种误导，代价是列数增加——这正是 `IndexNames` 需要生成含义清晰列名的原因。
- **分箱的用途**：连续特征与目标之间常常是**非线性**关系（例如「过高过低都不好」）。等宽分箱 + 指示列可以让线性模型表达这种关系。
- **自动模式的边界**：自动选择基于数据类型，无法判断「这一列虽然数值化但其实是类别」（如 ICD 编码）；这种情况下应使用显式规则。

## 包信息

- Assembly：`Microsoft.VisualBasic.DataMining.FeatureFrame`
- TargetFramework：`net10.0`
- Tags：`scibasic;feature-engineering;encoder;one-hot-encoding;binning;dataframe;machine-learning`
- 许可：GPL-3.0-or-later
