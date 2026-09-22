# 2D / 3D 图表绘制库（GDI+ 图形）

## 引言

一张合格的科研图表需要同时解决三件事：

1. **图形元素**：画布、轴线、刻度、图例、数据图元；
2. **图表类型**：柱、箱、等高线、3D 曲面……
3. **重用性**：同一份数据换风格、换尺寸、换类型时不应重写代码。

本包把这三层分开组织：`g` 命名空间提供图形层（画布、轴、图例与共享绘制原语），各图表族各自独立，`Plot3D` 负责三维。

## 图表类型一览

| 类别 | 命名空间 | 图表 |
|---|---|---|
| 图形层 | `...ChartPlots.g`（+ `g.Axis` / `g.Legends`） | 画布、坐标系、轴、图例与绘制原语 |
| 柱状与分布 | `...ChartPlots.BarPlot`（+ `Data` / `Histogram`） | 柱状图、直方图 |
| 箱线图 | `...ChartPlots.BoxPlot` | 箱线图 |
| 等高线 | `...ChartPlots.Contour`（+ `HeatMap`） | 等高线图、热图 |
| 比例 | `...ChartPlots.Fractions` | 堆叠比例图 |
| 散点 | `...ChartPlots.Plots` | 散点 / 折线 |
| 三维 | `...ChartPlots.Plot3D`（+ `Device` / `Model` / `Impl`） | 3D 曲面与立体图 |

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.Data.ChartPlots
Imports Microsoft.VisualBasic.Data.ChartPlots.BarPlot

' 1. 准备数据与绘图区（画布尺寸决定像素布局）
Dim g = New GraphicsRegion(width:=1000, height:=700)

' 2. 绘制柱状图并配置轴与图例
Dim bar As New BarChart(data, title:="Group comparison")
Call bar.SetAxisTitles(x:="group", y:="value")
Call bar.Plot(g)
Call g.Save("./bar.png")

' 3. 三维图表
Dim surface As New Plot3D.Surface(x, y, z)
Call surface.Save("./surface.png", width:=1200, height:=900)
```

## 实现要点

- **为什么把"图形层"独立出来**：轴、图例、网格的绘制逻辑对所有图表都是相同的；把它们下沉到 `g` 命名空间后，新增图表类型只需关心"数据图元怎么画"。
- **二维图表为何仍需"设备"概念**：`Plot3D.Device` 把投影（视角、缩放）与绘制解耦，换视角不需要重算数据。
- **配色与图例的联动**：图例必须与数据图元使用同一套配色映射，否则读者无法对应；实现中由同一配置对象驱动，避免不一致。
- **与 DataPlot 的分工**：DataPlot 侧重 ggplot2 风格的主题单元与自动布局；本包侧重图表类型的广度（含 3D 与等高线）。两者都输出 GDI+ 位图。

## 包信息

- Assembly：`Microsoft.VisualBasic.Data.ChartPlots`
- TargetFramework：`net10.0`
- Tags：`scibasic;chart;plotting;data-visualization;gdi-plus;graphics;3d-plot;contour;heatmap;barplot;boxplot`
- 许可：GPL-3.0-or-later
