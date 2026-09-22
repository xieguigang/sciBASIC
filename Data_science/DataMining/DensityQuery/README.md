# 空间密度查询与 SLIC 超像素分割

## 引言

两个看似无关的问题，其实共享同一种数学工具：

- **离群点检测**：一个点周围「邻居很少」，它就很可能是异常值；
- **图像区域分组**：一块区域内「像素很密」，它们就很可能属于同一个物体。

两者都需要快速回答：**这个位置附近的密度是多少？**

本包提供两类互补的实现：规则网格上的密度估计，与 KD 树索引上的邻居查询。

## 设计目标

- **两种索引并存**：网格适合均匀分布（查询 O(1)），KD 树适合稀疏 / 聚类分布（查询 O(log n)）；
- **距离可配置**：欧氏、曼哈顿等度量统一由 `Metric` 提供；
- **附带图像分割**：SLIC 超像素把密度思想扩展到图像区域。

## 核心类型与职责

全部类型位于根命名空间 `Microsoft.VisualBasic.DataMining.DensityQuery`：

| 类型 | 职责 |
|---|---|
| `Density2D` | 在规则 2D 网格上估计局部点密度 |
| `GridBox` | 网格容器（范围、分辨率与分箱统计） |
| `KDQuery` | 通过 KD 树做密度与邻居查询（适合散乱点） |
| `Metric` | 查询使用的距离度量 |
| `SLIC` | 简单线性迭代聚类：把图像分割为紧凑均匀的超像素 |

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.DataMining.DensityQuery

' 1. 把点集装入 KD 树查询器
Dim query As New KDQuery(points, Metric.Euclidean)

' 2. 查询某点半径内的邻居数量，据此判断离群
Dim density = query.CountWithin(center, radius:=0.5)

If density < 5 Then
    Console.WriteLine("possible outlier")
End If

' 3. 图像超像素分割
Dim labels = SLIC.Segment(image, superpixels:=200, compactness:=10)
```

## 实现要点

- **网格 vs KD 树的取舍**：网格密度查询是 O(1)（直接查桶），但分辨率固定，稀疏区域会浪费内存；KD 树自适应数据分布，但查询需要树遍历。数据越不均匀，KD 树越有优势。
- **超像素的意义**：把百万像素降到几百个区域后再做分割 / 识别，计算量下降数个数量级；SLIC 之所以流行，是因为它**只用一个参数（超像素数量）**就能得到紧凑、边界贴合的结果。
- **密度阈值的确定**：实现只提供「统计邻居数」的能力，阈值需要按数据规模设定——通常取邻居数的下分位数作为离群判据。

## 包信息

- Assembly：`Microsoft.VisualBasic.DataMining.DensityQuery`
- TargetFramework：`net10.0`
- Tags：`scibasic;density;superpixel;slic;kd-tree;outlier-detection;image-segmentation`
- 许可：GPL-3.0-or-later
