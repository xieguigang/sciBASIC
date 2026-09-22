# 统计图形扩展：ROC、QQ、热图与 PCA 图

## 引言

通用图表库能画"数据"，但统计报告需要的往往是**特定诊断图形**——它们有固定的画法约定，读者一眼就能读出结论：

- **ROC 曲线**：模型好坏（AUC）；
- **QQ 图**：分布是否近似正态；
- **森林图**：多个效应的置信区间对比；
- **相关热图**：变量间的整体关系；
- **PCA 碎石图**：保留多少主成分合适。

本包把这些图形补齐，让统计报告不必手工拼装。

## 图表清单

| 图表 | 用途 |
|---|---|
| **ROC 曲线** | 分类模型性能与阈值选择 |
| **QQ 图** | 检验分布假设（正态性、重尾） |
| **森林图（forest）** | 多元效应的置信区间并排比较 |
| **Z-score 图** | 标准化偏离程度 |
| **相关热图 / 相关三角** | 变量间相关结构（三角版省一半空间） |
| **密度图** | 分布的连续估计 |
| **时间趋势图** | 指标随时间变化 |
| **回归图** | 拟合线与散点的联合展示 |
| **PCA 碎石图 / 得分图** | 主成分选择与样本投影 |

## 命名空间

| 命名空间 | 职责 |
|---|---|
| `Microsoft.VisualBasic.Data.ChartPlots.Statistics`（根） | 统计诊断图形集合 |
| `....Statistics.Heatmap` | 通用热图 |
| `....Statistics.PCA` | PCA 碎石图与得分图 |

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.Data.ChartPlots.Statistics

' 1. 模型评估
Dim roc As New ROCPlot(labels, scores)
Call roc.Save("./roc.png", width:=800, height:=800)

' 2. 分布诊断
Dim qq As New QQPlot(values, DistributionKind.Normal)
Call qq.Save("./qq.png")

' 3. 多元结构
Dim corrPlot As New CorrelationHeatmap(matrix, method:=Correlation.Pearson)
Call corrPlot.Save("./corr.png", width:=1000, height:=1000)

Dim scree As New PCAScreePlot(pcaResult)
Call scree.Save("./scree.png")
```

## 实现要点

- **ROC 的判读**：曲线越靠近左上角越好，AUC = 0.5 等价于随机猜测。曲线上的每一点对应一个判决阈值，因此 ROC 同时也是**阈值选择工具**。
- **QQ 图怎么看**：横轴是理论分位数，纵轴是样本分位数；点**落在对角线上**说明样本符合该分布。在尾部明显弯曲说明重尾，在中间呈 S 形说明偏态。
- **相关三角为什么实用**：相关矩阵是对称的，完整热图有一半是冗余信息；三角形式把冗余部分裁掉，同样的空间可以放大单元格或加注数值。
- **碎石图的判断准则**：横轴是主成分序号，纵轴是解释方差；寻找曲线从"陡降"转为"平缓"的**拐点**，即保留主成分数量的常用依据。

## 包信息

- Assembly：`Microsoft.VisualBasic.Data.ChartPlots.Statistics`
- TargetFramework：`net10.0`
- Tags：`scibasic;statistics;roc-curve;qq-plot;forest-plot;heatmap;correlation-matrix;pca-plot;regression-plot`
- 许可：GPL-3.0-or-later
