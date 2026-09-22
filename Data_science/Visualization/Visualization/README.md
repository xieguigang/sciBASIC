# 网络、聚类与嵌入的可视化层

## 引言

分析包（数据挖掘 / 机器学习）产出的结果通常是这样几种形态：

- **图**：节点与边（相关网络、KNN 图）；
- **分组**：每个样本属于哪个簇；
- **树**：层次聚类的结果；
- **坐标**：降维后的 2D / 3D 嵌入（UMAP、SOM…）。

本包是**分析结果到图形之间的粘合层**：它认识这些结果类型，并知道该如何把它们变成可查看的图形。

## 核心能力

| 能力 | 说明 |
|---|---|
| **相关网络图** | 由相关矩阵生成网络图（相关性作为边权重） |
| **K-Means 聚类可视化** | 展示簇划分与簇中心 |
| **二叉树布局** | 把层次聚类结果排成可读的树形布局 |
| **嵌入渲染（2D / 3D）** | 渲染 UMAP、SOM 等降维结果的散点 / 立体图 |
| **表格渲染** | 以 CSV 形式渲染表格的子集与摘要（`TabularRender`） |

## 命名空间

| 命名空间 | 职责 |
|---|---|
| `Microsoft.VisualBasic.Data.visualize`（根） | 相关网络、K-Means 可视化与共享渲染辅助 |
| `....visualize.Tabular` | 表格渲染模型 |

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.Data.visualize

' 1. 由相关矩阵生成网络图
Dim net = CorrelationNetwork.Build(correlationMatrix, threshold:=0.7)
Call net.Save("./network.png", width:=1200, height:=1200)

' 2. 展示聚类结果
Dim clusters As New KMeansVisualizer(labels, centers)
Call clusters.Save("./clusters.png")

' 3. 渲染降维嵌入（2D / 3D）
Dim embedding As New EmbeddingPlot(umapResult, dimensions:=2)
Call embedding.Save("./umap.png", width:=900, height:=900)

' 4. 表格摘要渲染
Dim table As New TabularRender(frame)
Call table.Save("./preview.csv")
```

## 实现要点

- **相关网络为什么需要阈值**：相关矩阵是**稠密**的（任意两变量间都有相关系数）；若全部作为边画出来，图会变成一团乱麻。按阈值（或 top-k）筛选后才能显现结构，这也意味着**阈值即叙事**——不同阈值会讲出不同的故事。
- **嵌入渲染的两种维度**：2D 适合快速查看与论文配图；3D 适合探索遮挡关系与层次，但可读性对视角敏感。
- **树布局与簇划分的互补**：簇划分给出"分成几组"的结论，树布局展示"如何一步步合并"的过程；后者在解释聚类稳定性时尤为有用。
- **在技术栈中的位置**：本包位于「分析包 → 可视化」之间，向上调用 `DataPlot` / `Plots` 等绘图引擎，向下消费 `DataMining` / `MachineLearning` 的输出。

## 包信息

- Assembly：`Microsoft.VisualBasic.Data.visualize`
- TargetFramework：`net10.0`
- Tags：`scibasic;visualization;network-graph;correlation-network;embedding;clustering;kmeans;umap;tabular-rendering`
- 许可：GPL-3.0-or-later
