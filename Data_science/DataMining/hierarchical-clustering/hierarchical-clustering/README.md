# 层次聚类：BIRCH 与连接策略

## 引言

层次聚类的理想形式是「反复合并最近的两簇」，但直接实现有一个致命问题：**需要完整距离矩阵**。n 个样本的距离矩阵是 n²，一万个样本就是上亿个浮点数——内存先于算力崩溃。

本包给出两条应对路径：

1. **BIRCH / CF 树**：用「聚类特征摘要」压缩数据，单遍扫描即可完成聚类，内存与 n 无关（只与簇数相关）；
2. **多种连接策略**：single / complete / average / weighted linkage，配合距离图与可复用的层次树构建器。

## 设计目标

- **可处理流式数据**：BIRCH 单遍扫描，适合无法一次性载入的语料；
- **连接策略可选**：不同 linkage 产生非常不同的树形，必须可切换；
- **结果可复用**：输出的层次树可直接交给树状图绘制（`HCTreePlot`）。

## 核心能力

### BIRCH（`...HierarchicalClustering.BIRCH`）

CF 树（Clustering Feature Tree）用「聚类特征三元组」——点数、线性和、平方和——摘要描述一个子簇。插入新样本时只需更新这些统计量，从而在**单遍扫描**中完成聚类；后续再对 CF 树的叶节点做一次常规层次聚类即可。

### 层次树构建（`...HierarchicalClustering.Hierarchy`）

- **连接策略**：single（最近距离）、complete（最远距离）、average（平均距离）、weighted（加权平均）；
- **距离图**：预先计算并缓存簇间距离，避免重复计算；
- **层次树构建器**：产出可复用的树结构，供绘制或分析使用。

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.DataMining.HierarchicalClustering
Imports Microsoft.VisualBasic.DataMining.HierarchicalClustering.BIRCH
Imports Microsoft.VisualBasic.DataMining.HierarchicalClustering.Hierarchy

' 方案一：BIRCH 单遍聚类（适合大数据）
Dim cf As New JBIRCH(threshold:=0.5, branchingFactor:=50)

For Each sample In streamingData
    Call cf.Add(sample)
Next

' 方案二：经典凝聚层次聚类，指定连接策略
Dim tree = HierarchyBuilder.Build(vectors, Linkage.Average)

' 交给树状图绘制
Dim plot As New Dendrogram(tree)
Call plot.Save("./tree.png")
```

## 实现要点

- **连接策略如何改变结果**：
  - `single`：倾向于产生**长条形**簇（容易「链式」连接）；
  - `complete`：倾向于产生**紧凑**簇；
  - `average` / `weighted`：折中，实际使用最多。
- **BIRCH 为什么快**：它把「合并」推迟到 CF 树构建之后——树构建阶段只做统计量更新，复杂度接近线性；这使它成为少数能处理超大规模数据的层次聚类方法。
- **距离图的必要性**：凝聚聚类每轮都要找「最近的簇对」；若每次都重算距离，复杂度会退化为 O(n³)。缓存距离图是实用实现的关键优化。

## 包信息

- Assembly：`Microsoft.VisualBasic.DataMining.HierarchicalClustering`
- TargetFramework：`net10.0`
- Tags：`scibasic;hierarchical-clustering;birch;cf-tree;linkage;dendrogram;clustering`
- 许可：GPL-3.0-or-later
