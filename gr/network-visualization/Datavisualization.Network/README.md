# 网络图数据模型与拓扑分析库

## 引言

网络可视化的第一步不是「画」，而是「建模」：节点是什么？边携带什么数据？布局坐标存哪里？分析结果如何回写到图上？

本包是 `sciBASIC#` 网络可视化栈的**数据层**，它给出这些问题的统一答案，并在此之上提供拓扑分析能力。渲染（`Visualizer`）、布局（`network_layout`）与交互控件（`NetworkCanvas`）都建立在这套模型之上。

## 设计目标

- **模型与算法分层**：图模型（`Network` / `Graph`）、分析算法（`Analysis`）、布局坐标（`Layouts`）各自独立；
- **互操作友好**：提供 mxGraph 兼容的模型命名空间与 dynamics table 交换接口；
- **分析与渲染解耦**：分析算法只依赖图模型，因此可以脱离渲染独立使用。

## 核心特性

- **图模型**：`NetworkGraph` 节点与边模型、动态图数据表、图树结构（`Graph`、`GraphTree`）与抽象基类（`Graph.Abstract`）；
- **拓扑分析**：社区发现、图嵌入、强连通分量（Kosaraju）、PAGA 与网络统计（`Analysis`）；
- **分析模型**：邻接集与边集、有向顶点、图索引与遍历策略（`Analysis.Model`）；
- **相似度**：类连通性与图拓扑相似度度量（`Analysis.SimilarityImpl`）；
- **布局坐标与边捆绑**：`Layouts` 存放布局算出的节点坐标，`Graph.EdgeBundling` 提供边捆绑句柄；
- **交换接口**：`FileStream.Generic` 定义与文件流序列化层交换图数据的 dynamics table 接口；`com.mxgraph.model` 提供 mxGraph 兼容模型命名空间；`TreeAPI` 暴露图数据上的层次化树遍历。

## 命名空间地图

| 命名空间 | 职责 |
|---|---|
| `Microsoft.VisualBasic.Data.visualize.Network`（根） | 图模型与共享 API |
| `....Network.Graph`（+ `.Graph.Abstract` / `.Model.Handle` / `.Model.Vectors`） | 图 / 边 / 顶点、抽象基类、边捆绑与布局向量 |
| `....Network.Analysis`（+ `.Model` / `.SimilarityImpl`） | 社区、嵌入、连通性、统计与相似度 |
| `....Network.Layouts` | 节点布局坐标 |
| `....Network.TreeAPI` | 树形遍历 API |
| `com.mxgraph.model` | mxGraph 兼容模型命名空间 |

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.Data.visualize.Network
Imports Microsoft.VisualBasic.Data.visualize.Network.Analysis

' g 是一个已构建好的 NetworkGraph
Dim groups = New LouvainCommunity(g).SolveClusters()
Dim degree = g.GetDegreeCentrality()
```

## 实现要点

- **为什么单独抽出「布局坐标」**：布局是**计算**（可能耗时数秒甚至数分钟），而渲染是**绘制**（需要反复重绘）。把坐标作为独立数据后，布局只需算一次，之后可以任意次重绘、动画或换样式。
- **mxGraph 兼容层的作用**：mxGraph 是广泛使用的图编辑器模型，提供兼容命名空间后，外部图数据可以直接映射进来，无需为每种来源写适配器。
- **dynamics table 交换接口**：把图数据交换抽象为「动态表」而非固定类型，使同一套 I/O 代码可以处理节点表、边表与属性表。

## 包信息

- Assembly：`Microsoft.VisualBasic.Data.visualize.Network`
- TargetFramework：`net10.0`
- Tags：`scibasic;network-graph;graph-model;graph-analysis;centrality;community-detection;shortest-path`
- 许可：GPL-3.0-or-later
