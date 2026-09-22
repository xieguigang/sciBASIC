# 核心数据挖掘工具箱：聚类、关联规则与模型评估

## 引言

数据挖掘的算法看似各自独立，实际共享同一套「输入输出约定」：**样本 × 特征 → 分组 / 规则 / 评分**。把它们放进同一个工具箱的价值在于：

- 不同算法可以用同一套数据预处理（离散化、编码、归一化）；
- 可以直接比较不同算法的结果（用同一套评估指标）；
- 新增算法只需实现统一的组件接口。

## 设计目标

- **预处理统一**：离散化、编码、归一化集中在 `ComponentModel`；
- **算法并列可比**：聚类 / 关联 / 分类算法共享接口，便于横向对比；
- **可插拔**：`DFL_Driver` 定义驱动接口，使算法可被统一管线调度。

## 核心能力

### 聚类

| 算法 | 命名空间 | 特点 |
|---|---|---|
| K-Means | `Clustering.KMeans` | 经典划分聚类 |
| 二分 K-Means | `Clustering.KMeans.Bisecting` | 递归二分，缓解初始中心敏感 |
| Lloyd's | `Clustering.Lloyds` | K-Means 的原始算法 |
| DBSCAN | `Clustering.DBSCAN` | 基于密度，自动识别噪声 |
| HDBSCAN | `Clustering.HDBSCAN.*` | 层次化密度聚类（含 `Distance` / `Hdbscanstar` / `Runner`） |
| 模糊 C 均值 | `Clustering.FuzzyCMeans` | 软分配，输出隶属度 |

### 关联规则

`AprioriRules` 命名空间实现 Apriori 算法，配合 `AprioriRules.Entities`（项集与规则模型）与 `AprioriRules.Impl`（算法内部实现）。

### 分类与其它

- 决策树：`DecisionTree` 与其数据模型 `DecisionTree.Data`；
- 核方法：`Kernel.Classifier` 与 `Kernel.BayesianBeliefNetwork`；
- 模型评估：`Evaluation` 命名空间提供 ROC / AUC 等评估能力。

### 数据准备

`ComponentModel` 命名空间提供：离散化（`Discretion`）、特征编码（`Encoder` 及其 `Variable` 定义）、实体模型（`EntityModels`）、归一化（`Normalizer`）与上述模型的序列化（`Serialization`）。

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.DataMining
Imports Microsoft.VisualBasic.DataMining.Clustering.DBSCAN

' 数据准备：归一化 + 编码
Dim prepared = ComponentModel.Normalizer.Normalize(vectors)

' 聚类
Dim labels = New DBSCAN(eps:=0.5, minPts:=5).Cluster(prepared)

' 评估
Dim report = Evaluation.Report(labels, groundTruth)
```

## 实现要点

- **为什么把预处理抽成组件**：离散化阈值、编码方式、归一化参数都会显著影响聚类结果；把它们独立建模后，可以整体替换与对比，而不是散落在各算法内部。
- **DBSCAN 与 K-Means 的互补性**：K-Means 假定簇为凸形且需预设簇数；DBSCAN 能发现任意形状的簇并识别噪声。工具箱同时提供二者，正是为了让使用者按数据形态选择。
- **评估指标的意义**：无监督结果难以「目测正确」，ROC / AUC 等指标让算法选择与超参调优有据可依。

## 包信息

- Assembly：`Microsoft.VisualBasic.DataMining`
- TargetFramework：`net10.0`
- Tags：`scibasic;data-mining;clustering;kmeans;dbscan;hdbscan;apriori;decision-tree;association-rules;model-evaluation`
- 许可：GPL-3.0-or-later
