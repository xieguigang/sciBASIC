# 动态贝叶斯网络的结构与参数学习

## 引言

贝叶斯网络用**有向无环图**表达变量之间的依赖关系。当变量随时间演化时，这个图变成「动态贝叶斯网络（DBN）」：跨时间片的转移结构与片内的依赖结构需要分别学习。

学习 DBN 的经典两步法是：

1. **结构搜索**：在所有可能的父节点组合中，选出「评分最高」的图结构；
2. **参数拟合**：在给定结构下估计条件概率表。

本包实现了这条链路，并提供多种评分函数与最优分支搜索。

## 设计目标

- **评分函数可插拔**：LL、MDL 与随机基线评分并存，便于对比与实验；
- **结构与参数解耦**：先搜结构、再拟合参数；
- **支持多网络与交叉验证**：为模型选择提供依据。

## 核心能力

### 网络模型（`dbn` 命名空间）

| 类型 | 职责 |
|---|---|
| `BayesNet` | 静态贝叶斯网络 |
| `DynamicBayesNet` / `MultiNet` | 动态贝叶斯网络与多网络模型 |
| `Attribute` / `NominalAttribute` / `NumericAttribute` | 变量类型（离散 / 连续） |
| `Configuration` / `LocalConfiguration` / `MutableConfiguration` | 变量配置与局部配置 |
| `Observations` | 观测序列 |

### 评分与搜索

| 类型 | 职责 |
|---|---|
| `ScoringFunction` / `Scores` | 评分函数抽象与评分容器 |
| `LLScoringFunction` | 对数似然评分 |
| `MDLScoringFunction` | 最小描述长度评分（自带复杂度惩罚） |
| `RandomScoringFunction` | 随机基线，用于对照实验 |
| `OptimumBranching` | 在候选父节点上求最优分支 |
| `CrossValidation` | 交叉验证估计泛化能力 |

`utils` 命名空间提供配套辅助方法。

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.DataMining.DynamicBayesianNetwork.dbn

' 1. 准备观测序列
Dim obs As New Observations(data, attributes)

' 2. 用 MDL 评分搜索结构
Dim structure = OptimumBranching.Search(obs, New MDLScoringFunction())

' 3. 拟合参数
Dim net As DynamicBayesNet = DynamicBayesNet.Fit(structure, obs)

' 4. 交叉验证
Dim cv = CrossValidation.Run(net, obs, folds:=5)
```

## 实现要点

- **为什么需要复杂度惩罚**：边越多，似然必然越大（过拟合）；MDL 评分通过描述长度惩罚自动抵制无意义的边，这是结构学习能否work的关键。
- **最优分支算法的价值**：为每个变量独立选择父节点会引入环；`OptimumBranching` 在保证无环的前提下求全局最优解，避免了「选完再检测环」的反复。
- **动态网络的特殊性**：DBN 的转移结构在所有时间片上共享，因此观测序列越长，参数估计越可靠——这正是时间序列建模的优势所在。

## 包信息

- Assembly：`Microsoft.VisualBasic.DataMining.DynamicBayesianNetwork`
- TargetFramework：`net10.0`
- Tags：`scibasic;bayesian-network;dynamic-bayesian-network;structure-learning;scoring-function;em;data-mining`
- 许可：GPL-3.0-or-later
