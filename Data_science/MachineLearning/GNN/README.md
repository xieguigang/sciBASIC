# 图神经网络：GCN、图注意力与递归时序图层

## 引言

图神经网络（GNN）的核心机制只有一句话：**每个节点通过聚合邻居的信息来更新自己的表示**。

这一机制之所以强大，是因为它不像图嵌入那样"先固化表示再下游使用"，而是可以把图结构**嵌入到端到端模型内部**——特征、结构、任务一起优化。

本包提供实现这一机制的几类图层，以及时序图的支持。

## 支持的图层与模型

| 命名空间 / 类型 | 机制 | 适用 |
|---|---|---|
| **GCN 层** | 邻居特征的加权平均（谱图卷积的简化） | 同质图、节点分类 |
| **图注意力层（GAT）** | 用注意力权重决定邻居重要性 | 邻居重要性差异大的图 |
| **GRU / RNN 递归图层** | 沿边传递并保留状态 | 边上带序列 / 图上的消息传递迭代 |
| **TemporalGraph / TemporalModels** | 对一系列图快照建模 | 动态图（随时间演化的网络） |
| **GraphDataset** | 图输入 + 标签的封装 | 训练数据组织 |
| **图分类模型** | 整图级别预测 | 分子活性、代码分类 |

训练使用 SGD 与 Adam。

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.DeepLearning.GNN

' 1. 组织图数据集（图 + 标签）
Dim dataset As New GraphDataset(graphs, labels)

' 2. 堆叠 GCN / 注意力层
Dim model As New GraphClassifier()
Call model.AddLayer(New GCNLayer(units:=64))
Call model.AddLayer(New GraphAttentionLayer(heads:=4))
Call model.AddLayer(New ReadoutLayer(units:=classes))

' 3. 训练（SGD / Adam）
Call model.Train(dataset, optimizer:=Optimizer.Adam, epochs:=100)

' 4. 时序图：对一系列快照建模
Dim temporal As New TemporalModels(snapshots)
```

## 实现要点

- **GCN 与 GAT 的取舍**：GCN 的邻居权重由度归一化**预先确定**（`1/√(d_i·d_j)`），因此简单高效；GAT 让模型**学习**邻居权重，表达力更强但参数更多。邻居重要性差异明显时 GAT 优势显著。
- **为什么要时序图模型**：很多真实网络是动态的（社交关系、交易网络、蛋白质互作在不同条件下不同）。把每个时刻的图当独立样本会丢失演化信息；`TemporalGraph` 把快照序列作为输入，让模型学习"如何演化"。
- **过平滑问题**：GCN 层数过多会使所有节点表示趋同（因为反复平均邻居）。实践中通常只用 2–3 层，或引入残差连接——这也是实现中把层数作为显式配置项的原因。
- **与 `Bootstrapping` 的分工**：嵌入方法（node2vec / graph2vec）产出的向量是**静态**的，适合"把图变成特征后供任意模型使用"；GNN 层可参与**端到端训练**，两者互补。

## 包信息

- Assembly：`Microsoft.VisualBasic.DeepLearning.GNN`
- TargetFramework：`net10.0`
- Tags：`scibasic;graph-neural-network;gcn;graph-attention;message-passing;temporal-graph;graph-classification`
- 许可：GPL-3.0-or-later
