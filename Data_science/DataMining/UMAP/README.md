# UMAP：一致流形近似与投影

## 引言

UMAP 的出发点是**拓扑学**，但落到实现上只有三步：

1. **建近邻图**：对每个点找 k 个最近邻；
2. **构造模糊单纯集**：把"两个点是邻居"从布尔值变成 **0–1 之间的隶属度**；
3. **优化低维布局**：在低维空间构造同样的模糊图，用交叉熵做为目标，随机梯度下降求解。

第二步的"模糊化"是 UMAP 的精髓：**不再有"是/不是邻居"的硬边界，而是"有多大概率是邻居"**。

## 核心能力与关键类型

| 命名空间 | 职责 |
|---|---|
| `Microsoft.VisualBasic.DataMining.UMAP`（根） | 主算法：模糊图构造与 SGD 优化 |
| `....UMAP.KNN` | 近邻搜索 |
| `....UMAP.KNN.KDTreeMethod` | 基于 KD 树的精确近邻搜索方法 |
| `....UMAP.Components.Tree` | 近邻下降过程中使用的树结构 |

近邻计算提供两条路径：**NN-descent**（近似、可扩展）与 **KD 树**（精确、维度较低时更快）。

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.DataMining.UMAP

' 1. 用 k 近邻与最小距离配置嵌入器
Dim reducer As New UMAP(nNeighbors:=15, minDist:=0.1)

' 2. 拟合并得到低维嵌入
Dim embedding = reducer.FitTransform(data)

' 3. 嵌入结果可交给可视化层（data_visualize）绘制
Console.WriteLine(embedding.Dimensions)
```

## 实现要点

- **模糊隶属度如何计算**：对每个点，先根据到第 k 近邻的距离确定一个局部尺度，再把距离转成隶属度；这一步的局部尺度自适应，使 UMAP 在密度差异大的数据上仍然稳健。
- **为什么比 t-SNE 快**：t-SNE 需要对所有点对计算相似度（即使 Barnes-Hut 近似也要构建全图），UMAP 只依赖 k 近邻图——**图为稀疏**，因此优化阶段的边数是 O(nk) 而非 O(n²)。
- **n_neighbors / min_dist 的调参直觉**：
  - `n_neighbors` 越大 → 越强调**全局**结构，簇会更紧密相连；
  - `min_dist` 越小 → 低维空间中允许点更靠近，簇会更**紧凑**。
- **与 t-SNE 的取舍**：需要"快速得到局部结构良好的图"选 UMAP；数据规模不大、且更在意局部邻域保真度时 t-SNE 仍是稳妥选择。

## 包信息

- Assembly：`Microsoft.VisualBasic.DataMining.UMAP`
- TargetFramework：`net10.0`
- Tags：`scibasic;umap;dimensionality-reduction;manifold-learning;nearest-neighbor;fuzzy-simplicial-set;visualization`
- 许可：GPL-3.0-or-later
