# DataPlot：GDI+ 图表引擎与 ggplot2 风格主题

## 引言

画图代码最容易失控的地方是**坐标计算**：坐标轴放哪里、刻度标签会不会重叠、图例该摆在何处、边距留多少——这些琐碎计算会让真正的绘图逻辑淹没在像素算术里。

DataPlot 的解法是把布局**自动化**，并借鉴 ggplot2 的**主题单元**概念：

- **主题（theme）** 由一组可组合的「主题单元」描述（坐标轴、背景、网格、图例…）；
- **布局** 自动完成像素级定位，无需手工指定坐标。

## 支持的图表类型

| 类别 | 图表 |
|---|---|
| **关系与趋势** | 散点图、折线图 |
| **分布** | 直方图、箱线图（box）、小提琴图（violin）、抖动图（jitter） |
| **比较** | 柱状图、气泡图 |
| **比例** | 饼图、玫瑰图（rose）、雷达图 |
| **矩阵与层次** | 热图、和弦图（chord）、桑基图（Sankey）、树图（treemap） |

## ggplot2 风格主题单元

主题不是一组固定配色，而是**可组合的单元**：

| 单元 | 控制内容 |
|---|---|
| `axis` | 轴线、刻度、标签字体与颜色 |
| `plot background` | 绘图区背景填充 |
| `grid` | 网格线（主 / 次网格） |
| `legend` | 图例位置、边框与文字 |

多个单元可以叠加成完整主题，因此「换风格」＝「换主题定义」，绘图代码不变。

`GgplotTheme` 命名空间存放主题定义。

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.Data.Plots
Imports Microsoft.VisualBasic.Data.Plots.Engine.GgplotTheme

' 1. 准备数据与主题
Dim theme = GgplotThemes.Default

' 2. 绘制散点图（布局自动完成）
Dim plot As New ScatterPlot(x:=x, y:=y, theme:=theme)

Call plot.SetTitle("Expression vs Time")
Call plot.SaveChanges("./scatter.png", width:=1200, height:=800)

' 3. 换成箱线图 / 热图等只是换绘图类型
Dim box As New BoxPlot(groups, theme:=theme)
Call box.Save("./box.png", width:=800, height:=600)
```

## 实现要点

- **自动布局的价值**：手工计算坐标会让"加一个图例"变成"重算所有偏移"。把布局抽象出来后，新增元素只影响布局引擎，不影响绘图代码。
- **主题单元 vs 全局配色**：只换配色无法改变"网格是否显示""图例是否有边框"；主题单元模型让这些结构性差异也能通过配置表达。
- **GDI+ 的定位**：本包输出位图（PNG / BMP），适合报告与演示；需要矢量输出（SVG / PostScript）时应使用 `Microsoft.VisualBasic.Imaging` 的驱动层。
- **与 `Plots` 包的分工**：DataPlot 是**独立引擎**（自带主题与布局），`Plots` 是更偏统计与扩展的图表库；两者可分别使用，也可由上层按需选择。

## 包信息

- Assembly：`Microsoft.VisualBasic.Data.Plots`
- TargetFramework：`net10.0`
- Tags：`scibasic;dataplot;chart;plotting;gdi-plus;ggplot-theme;data-visualization;boxplot;violin-plot;heatmap;sankey;treemap`
- 许可：GPL-3.0-or-later
