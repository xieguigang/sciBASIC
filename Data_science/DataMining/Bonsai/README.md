# Bonsai 扩散树布局：高维数据的分支结构重建

## 引言

主流降维方法（PCA、t-SNE、UMAP）都在追求一件事：**保持点与点之间的距离**。但很多高维数据的本质结构不是「距离」，而是**分支**——细胞分化、物种演化、样本亚群的形成。

Bonsai 的目标是把这种分支结构**显式重建为一棵树**：

- 树的每个内部节点是一个「分叉事件」；
- 每条边的长度有明确含义（由似然优化得到）；
- 最终结果既可用于可视化，也可作为层次聚类的依据。

## 设计目标

- **显式分支模型**：输出是一棵树，而不是一堆坐标；
- **似然驱动**：分支长度与内部节点坐标通过 Felsenstein 树似然优化，而非启发式放置；
- **可可视化**：布局结果直接产出 2D 坐标。

## 核心类型与职责

全部类型位于根命名空间 `Microsoft.VisualBasic.DataMining.Bonsai`：

| 类型 | 职责 |
|---|---|
| `BonsaiTree` | 重建并持有扩散树 |
| `Likelihood` | 在观测上评估 Felsenstein 风格树似然 |
| `Optimizer` | 针对似然优化每条分支长度与内部节点坐标 |
| `PointSet` | 持有高维观测点集 |
| `Layout` | 产出用于可视化的 2D 坐标 |
| `BonsaiApi` / `BonsaiNode` | 树的对外 API 与节点 |

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.DataMining.Bonsai

' 1. 用高维观测点集构建树
Dim tree As BonsaiTree = BonsaiTree.Build(points)

' 2. 针对树似然优化分支长度与内部节点坐标
Call Optimizer.Optimize(tree, points)

' 3. 得到可视化布局
Dim layout = Layout.Compute(tree)

For Each node In tree.Nodes
    Console.WriteLine($"{node.ID}  {layout(node).X}, {layout(node).Y}")
Next
```

## 实现要点

- **为什么用似然而不是距离**：距离只能描述「两个点有多远」，无法描述「分支发生在哪里」。引入树的生成模型后，内部节点的位置与分支长度都成为可优化的参数。
- **Felsenstein 剪枝算法**：直接在树上评估似然的开销是节点数的指数级；Felsenstein 剪枝把它降到线性，这是「似然驱动」可行的前提。
- **与 UMAP / t-SNE 的互补**：降维方法适合「看整体形状」，Bonsai 适合「确认是否存在分叉以及分叉发生在何处」——两者常配合使用。

## 包信息

- Assembly：`Microsoft.VisualBasic.DataMining.Bonsai`
- TargetFramework：`net10.0`
- Tags：`scibasic;bonsai;dimensionality-reduction;diffusion-tree;felsenstein-likelihood;visualization`
- 许可：GPL-3.0-or-later
