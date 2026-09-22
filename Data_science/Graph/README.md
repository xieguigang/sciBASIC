# 图论算法、网络模型与图搜索

## 引言

「图」是科学计算中最通用的数据结构之一：代谢网络、蛋白质互作网络、社交网络、分子结构、流程图……几乎所有领域都能抽象成顶点与边的集合。而一旦完成抽象，接下来要回答的往往是同一批问题：

- 两点之间最短怎么走？
- 这个网络会自然分裂成哪些「社区」？
- 谁是网络中最关键的节点？
- 给定一个查询向量，最近的 k 个邻居是谁？

`Microsoft.VisualBasic.Data.GraphTheory` 就是 `sciBASIC#` 对这批问题的统一回答：**一套通用的图数据模型 + 一个覆盖面很广的算法库**。它是网络可视化栈（布局 / 渲染）与数据挖掘模块共同的算法地基。

## 设计目标

- **模型与算法解耦**：所有算法都建立在泛型 `Graph` / `NetworkGraph(Of Node, Edge)` 之上，不绑定具体业务类型。
- **同一问题给出多种实现**：例如最短路同时提供面向对象的 `DijkstraRouter` 与优先队列优化的 `DijkstraFast`，可按图规模与调用习惯选择。
- **精确与近似并存**：k 近邻既有精确的 KD-tree，也有近似的 HNSW。

## 核心特性

- **最短路与路由**：面向对象 Dijkstra（`DijkstraRouter`）、优先队列 Dijkstra（`DijkstraFast`）、带双向搜索的**收缩层次**（contraction hierarchies）、以及网格图上的 A* 路由。
- **社区发现**：Louvain、Leiden（其精化阶段保证社区内部连通）与标签传播（LPA），并提供模块度评估辅助。
- **网络算法**：Dinic 最大流、Kruskal 最小生成树、Ullmann 子图同构、二分匹配、PageRank 与加权 PageRank、Morgan 指纹。
- **搜索结构与数据模型**：KD-tree（含近似 k-NN）、HNSW 小世界图、余弦距离与 Earth Mover's Distance、Huffman 树、Trie、PQ-tree，以及 2D/3D 空间网格图。

## 命名空间地图

| 命名空间 | 职责 |
|---|---|
| `Microsoft.VisualBasic.Data.GraphTheory`（根） | 图数据模型：`Graph`、`NetworkGraph(Of Node, Edge)`、科学图与稀疏图 |
| `...GraphTheory.Analysis` | 分析算法根：收缩层次、Dijkstra、Dinic 最大流、Morgan 指纹、PageRank |
| `...GraphTheory.Analysis.Community.*` | 社区发现：`FastUnfolding`、`Louvain`、`LPA` |
| `...GraphTheory.Analysis.PQDijkstra` | 优先队列 Dijkstra |
| `...GraphTheory.EMD` | Earth Mover's Distance |
| `...GraphTheory.KNearNeighbors`（+ `.HNSW`） | k 近邻搜索与 HNSW 近似索引 |
| `...GraphTheory.Model.GridGraph` | 2D/3D 空间网格图 |
| `...GraphTheory.Model.Tree.*` | 索引树：`KdTree`（+ `ApproximateNearNeighbor`）、`HuffmanTree` |
| `...GraphTheory.MinimumSpanningTree` | 最小生成树 |
| `...GraphTheory.Network` | 网络图模型（节点度、边元数据、树形 API） |

## 关键类型与 API

- `Microsoft.VisualBasic.Data.GraphTheory.Graph` —— 抽象 `G = (V, E)` 容器，持有顶点与边。
- `Microsoft.VisualBasic.Data.GraphTheory.Network.NetworkGraph(Of Node, Edge)` —— 具体泛型网络图，支持节点度与边元数据。
- `Microsoft.VisualBasic.Data.GraphTheory.Analysis.Dijkstra.DijkstraRouter` —— 单源 Dijkstra 路由，为每个可达顶点返回一条 `Route`。
- `Microsoft.VisualBasic.Data.GraphTheory.Analysis.Louvain.LouvainCommunity` —— 大型网络的快速展开社区发现（`SolveClusters`、`SolveClustersParallel`）。
- `Microsoft.VisualBasic.Data.GraphTheory.Analysis.Louvain.LeidenCommunity` —— Louvain 的 Leiden 精化，保证社区内部连通性。
- `Microsoft.VisualBasic.Data.GraphTheory.Analysis.LPA.LabelPropagation` —— 近线性时间的标签传播社区发现。
- `Microsoft.VisualBasic.Data.GraphTheory.MinimumSpanningTree.Kruskal` —— 基于并查集的 Kruskal 最小生成树。
- `Microsoft.VisualBasic.Data.GraphTheory.KNearNeighbors.HNSW.SmallWorld` —— 层次可导航小世界图，用于近似 k-NN 搜索。

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.Data.GraphTheory.Analysis.Louvain
Imports Microsoft.VisualBasic.Data.GraphTheory.Network

' g 是一个已有的 NetworkGraph(Of Node, Edge)
Dim louvain As LouvainCommunity = Builder.Load(g)
Dim community As String() = louvain.SolveClusters().GetCommunity()
Dim clusterCount As Integer = louvain.GetClusterCount()
```

## 与 sciBASIC# 生态的关系

- **向上被依赖**：`gr/network-visualization` 下的布局（`network_layout`）与渲染（`Visualizer`、`Datavisualization.Network`）都建立在本包提供的图模型与算法之上；
- **横向复用**：数据挖掘模块（`DataMining`）使用本包的聚类与搜索结构；
- **向下依赖**：`Microsoft.VisualBasic.Core` 与 `Math`。

## 性能与实现要点

- **两种 Dijkstra**：`DijkstraRouter` 以对象化 `Route` 结果优先；`DijkstraFast` 以优先队列与紧凑存储优先，用于大规模图。
- **收缩层次**：预先对图做收缩预处理，使后续点对点查询退化为双向 Dijkstra，适合反复查询同一张静态图。
- **近似优先**：HNSW 以可调参数在召回率与查询速度之间权衡，适用于高维向量的近邻检索。

## 包信息

- Assembly：`Microsoft.VisualBasic.Data.GraphTheory`
- TargetFramework：`net10.0`
- Tags：`scibasic;graph-theory;graph-algorithm;shortest-path;dijkstra;a-star;community-detection;louvain;leiden;max-flow;spanning-tree;hnsw;network-analysis`
- 许可：GPL-3.0-or-later
