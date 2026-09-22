# 图与知识图谱嵌入工具箱（node2vec / graph2vec / ComplEx）

## 引言

机器学习算法吃的是**向量**，但很多真实数据是**图**：社交网络、分子结构、知识图谱、代谢网络。于是产生了「图嵌入」这一任务：

> 把图结构映射到向量空间，使「结构相近」的节点 / 图在向量空间中也相近。

本包提供三类主流方法：

| 方法 | 嵌入对象 | 核心思想 |
|---|---|---|
| **node2vec** | 节点 | 有偏随机游走 + word2vec |
| **graph2vec** | 整张图 | 把整图表示为向量 |
| **ComplEx 系列** | 知识图谱实体 / 关系 | 复数空间中的双线性打分 |

## node2vec：为什么"有偏"随机游走很重要

普通随机游走生成的是「邻居序列」，但邻居有两种截然不同的角色：

- **同质性（homophily）**：结构等价的节点（同一社区）→ 需要**深度优先**的游走（往回走的概率低）；
- **结构等价（structural equivalence）**：角色等价的节点（都是"枢纽"）→ 需要**广度优先**的游走（倾向回到起点附近）。

node2vec 通过两个参数（`p` 返回概率、`q` 前进概率）**连续调节**这两种偏向，因此同一张图可以产出面向不同任务的嵌入。

## graph2vec

把整张图（而不是单个节点）表示为向量，适合**图分类**任务，例如：

- 判断一个分子是否具有某种活性；
- 判断一个程序的调用图是否属于某类恶意软件。

## ComplEx 系列

| 命名空间 | 变体 | 说明 |
|---|---|---|
| `GraphEmbedding.complex` | ComplEx | 复数空间双线性打分，能表达**非对称**关系 |
| `GraphEmbedding.complex_NNE` | + 神经网络编码器 | 用神经网络学习实体 / 关系表示 |
| `GraphEmbedding.complex_NNE_AER` | + 自编码器正则 | 用自编码器约束嵌入空间结构 |
| `GraphEmbedding.complex_R` | 关系类型扩展 | 支持带类型的关系 |
| `GraphEmbedding.struct` | 结构嵌入 | 基于图结构的嵌入 |

为什么用**复数**：在实数向量空间中，双线性模型难以同时表达「对称」与「反对称」关系；引入复数虚部后，`Re(<e_s, w_r, ē_o>)` 这一打分可以自然地表达**方向性**（A 是 B 的老师 ≠ B 是 A 的老师）。

## 命名空间地图

| 命名空间 | 职责 |
|---|---|
| `Microsoft.VisualBasic.MachineLearning.Bootstrapping`（根） | graph2vec 风格整图向量 |
| `....Bootstrapping.GraphEmbedding`（+ `.complex` / `.complex_NNE` / `.complex_NNE_AER` / `.complex_R` / `.struct` / `.util`） | 图与知识图谱嵌入 |
| `....Bootstrapping.node2vec` | node2vec 有偏随机游走 + word2vec |

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.MachineLearning.Bootstrapping
Imports Microsoft.VisualBasic.MachineLearning.Bootstrapping.Node2Vec

' 1. node2vec：有偏随机游走 + word2vec 训练
Dim n2v As New node2vec(embeddingDim:=128, walkLength:=80, walksPerNode:=10)
Dim nodeVectors = n2v.Fit(graph, p:=1.0, q:=0.5)

' 2. 整图向量（graph2vec 风格）
Dim graphVector = Graph2Vec.Embed(graph)

' 3. 知识图谱嵌入（ComplEx）
Dim kg = ComplEx.Train(triples, dim:=100, iterations:=1000)
Dim score = kg.Score("virus", "infects", "human")
```

## 实现要点

- **p / q 参数的直觉**：`q > 1` 偏向广度优先（结构等价），`q < 1` 偏向深度优先（同质性）；`p` 控制"立刻原路返回"的概率。
- **ComplEx 的非对称性**：把关系视为复数变换，使 `Score(s, r, o) ≠ Score(o, r, s)`，从而正确建模有向关系。
- **嵌入的实际用途**：得到节点向量后，链接预测、节点分类、聚类都可以直接用标准机器学习包处理——这正是「把图问题转化为向量问题」的价值。

## 包信息

- Assembly：`Microsoft.VisualBasic.MachineLearning.Bootstrapping`
- TargetFramework：`net10.0`
- Tags：`scibasic;graph-embedding;node2vec;graph2vec;knowledge-graph;complex;random-walk;representation-learning`
- 许可：GPL-3.0-or-later
