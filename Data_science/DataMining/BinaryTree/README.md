# AVL 二叉树聚类与 Newick 树输出

## 引言

层次聚类（hierarchical clustering）最朴素的做法是维护一个距离矩阵，反复合并最近的两簇——复杂度 O(n³) 甚至更高，在数据量稍大时就不可行。

本包采用的思路是：**用一个 AVL 树承载「近似相似」的插入过程**。

- 每个新样本按「模糊相似度比较」插入 AVL 树；
- 相似样本自然落到相邻子树；
- 最后**切割树**得到簇划分。

这样既获得了树形结构（可直接输出为 Newick / 树状图），又避免了完整距离矩阵的开销。

## 设计目标

- **树结构即结果**：聚类结果天然是树，可直接用于树状图与层次分析；
- **标准格式互通**：Newick 是系统发生树的通用文本格式，读写它意味着结果可以被其它工具消费；
- **配套表示学习**：附带扩散映射与 KNN 图构建，便于把结果用于降维与可视化。

## 核心特性

- **AVL 树聚类**：`BuildTree` 在模糊相似度比较下把样本插入 AVL 树，`Partitioning` 切割树得到簇；
- **Newick 读写**：`newickParser` 读写 Newick 树文本，使树状图可在工具间往返；
- **扩散映射**：`DiffusionMap` 提供扩散映射嵌入；
- **KNN 图**：`KNNGraph` 构建 k 近邻图；
- **亲和传播聚类**：`AffinityPropagation` 直接从相似度矩阵选出代表点（exemplar），无需预设簇数量。

## 命名空间与关键类型

| 命名空间 | 类型 |
|---|---|
| `Microsoft.VisualBasic.DataMining.BinaryTree`（根） | `BuildTree`、`Partitioning`、`DiffusionMap`、`KNNGraph`、`newickParser` |
| `....BinaryTree.AffinityPropagation` | 亲和传播聚类 |

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.DataMining.BinaryTree

' 1. 在模糊相似度比较下构建 AVL 树
Dim tree = BuildTree.Create(vectors)

' 2. 切割成簇
Dim clusters = Partitioning.Cut(tree, threshold:=0.85)

' 3. 输出为 Newick，可交给其它树工具
Dim newick As String = newickParser.ToString(tree)
Console.WriteLine(newick)
```

## 实现要点

- **为什么是 AVL 而不是普通二叉搜索树**：样本插入顺序往往与相似度无关，若树退化成链表，插入代价会从 O(log n) 劣化为 O(n)；AVL 的自平衡保证始终接近对数复杂度。
- **「模糊」比较的意义**：精确相等在连续特征上几乎不会发生，必须用相似度阈值判定「落入哪一侧」；阈值的选择直接决定簇的粒度。
- **切割而非合并**：先建树再切割，天然支持「用不同阈值得到不同粒度的簇」，无需为每个粒度重跑一次聚类。

## 包信息

- Assembly：`Microsoft.VisualBasic.DataMining.BinaryTree`
- TargetFramework：`net10.0`
- Tags：`scibasic;clustering;binary-tree;avl-tree;newick;diffusion-map;knn-graph;affinity-propagation`
- 许可：GPL-3.0-or-later
