# 树状图与径向树绘制

## 引言

层次聚类的结果是一棵树，但**树怎么画**直接决定读者能看出什么：

- **垂直树状图**（dendrogram）：最经典，能清晰展示合并顺序与距离；
- **圆形 / 径向布局**：叶子多时更省空间，也更能突出整体轮廓；
- **横向滚动面板**：叶子标签很长时（如基因名）更易读。

本包提供这些布局，并把它们统一到同一套绘制模型上。

## 设计目标

- **多布局一套模型**：所有布局共享 `Layouts` 提供的几何模型与面板基类；
- **出版级输出**：带类别颜色图例、叶标签与标尺；
- **消费标准树模型**：直接使用层次聚类引擎产出的簇树。

## 核心能力

| 类型 | 职责 |
|---|---|
| `Dendrogram` | 垂直树状图 |
| `Circular` | 圆形布局 |
| `RadialDendrogram` | 径向（放射）树状图 |
| `Horizon` / `HorizonRightToLeft` | 横向面板（左→右 / 右→左） |
| `DendrogramPanel` / `DendrogramPanelV2` | 面板基类（绘图与交互的公共骨架） |
| `Layouts` | 共享的树几何布局模型 |

以上类型均位于根命名空间 `Microsoft.VisualBasic.DataMining.HierarchicalClustering`。

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.DataMining.HierarchicalClustering

' tree 来自层次聚类引擎
Dim plot As New Dendrogram(tree)

Call plot.SetClassColors(classLabels)
Call plot.SetLeafLabels(names)
Call plot.Save("./dendrogram.png")

' 需要径向布局时
Dim radial As New RadialDendrogram(tree)
Call radial.Save("./radial.png")
```

## 实现要点

- **垂直树状图的读法**：横轴是叶节点顺序，纵轴是**合并距离**——两个簇合并的高度越高，说明它们越不相似。因此「在哪个高度切断树」等价于「取多粗的簇粒度」。
- **为什么要居中叶子标签**：当叶子很多时标签会互相覆盖；径向与横向布局通过旋转 / 拉伸文本轴线减少重叠，这也是它们在大规模聚类结果上更常用的原因。
- **面板基类的价值**：`DendrogramPanel` / `DendrogramPanelV2` 把坐标变换、标签定位与图例绘制等公共逻辑固定下来，各布局只需实现「如何把树映射为几何」。

## 包信息

- Assembly：`Microsoft.VisualBasic.DataMining.HierarchicalClustering`
- TargetFramework：`net10.0`
- Tags：`scibasic;dendrogram;radial-tree;visualization;hierarchical-clustering;plot;tree-layout`
- 许可：GPL-3.0-or-later
