# 2D / 3D 科学图形渲染引擎

## 引言

科研可视化有一个反复出现的矛盾：**同一份数据，往往要用不同格式输出**——论文要矢量（SVG/PostScript），报告中要位图（PNG），网页要 SVG，打印机要 PostScript。

如果每种输出都写一遍绘制代码，维护成本会迅速失控。本包的解法是**驱动式架构**：

```text
                 ┌─► 位图驱动 ─► PNG / BMP
绘图代码 ─► IGraphics ─┼─► SVG 驱动 ─► .svg
                 ├─► PostScript 驱动 ─► .ps
                 └─► CSS 驱动 ─► HTML + CSS
```

**绘图代码只写一次，输出格式由驱动决定。**

## 设计目标

- **一次绘制，多种输出**：`IGraphics` 抽象屏蔽各后端的差异；
- **2D 与 3D 统一**：热图、形状、文本与等轴测 3D 场景共用同一套绘制入口；
- **样式可数据驱动**：d3.js 风格的 scale 与配色体系，让颜色 / 尺寸由数据决定。

## 核心特性

### Drawing2D

- **形状**：线、矩形、圆、菱形、六边形、五角星、圆角矩形、三角形与箭头（`Drawing2D.Shapes`）；
- **文本**：HTML 文本渲染、自动换行、ASCII 艺术（`Drawing2D.Text.ASCIIArt`）与 **nudge 标签避让**（`Drawing2D.Text.Nudge`，把重叠标签推开）；
- **配色**：调色板、ColorBrewer、Viridis、Office 主题色、颜色缩放器与图例（`Drawing2D.Colors`）；
- **热图**：热图矩阵与像素渲染，含 HQx 像素放大（`Drawing2D.HeatMap`）；
- **2D 数学**：画布缩放、样条、凸包与凹包、Marching Squares 等值线、Delaunay / Voronoi 与折线简化（`Drawing2D.Math2D`）。

### Drawing3D

- **等轴测引擎**：相机、光照、画家算法、点云与曲面（`Drawing3D`）；
- **3D 数学**：矩阵、向量、投影、变换、Marching Cubes 与多面体（`Drawing3D.Math3D`）；
- **现成模型**：立方体、网格、路径与柱体 / 纽结 / 棱锥等形状（`Drawing3D.Models`）。

### 驱动与样式

- **驱动层**：`Driver` 定义 `IGraphics` 契约、工厂、设备描述与画布，并给出图形数据模型（位图、SVG、PostScript、WMF）与 JPEG / EXIF 元数据处理；
- **d3.js 风格**：scale 体系（线性 / 序数 / 常量）与图表模块 API（`d3js`）；
- **SVG 样式**：CSS 模型驱动渐变与滤镜，配合 SVG XML 元素模型（形状、容器与路径指令）。

## 命名空间地图

| 命名空间 | 职责 |
|---|---|
| `Microsoft.VisualBasic.Imaging`（根） | 扩展与纹理资源加载 |
| `....Imaging.Drawing2D`（+ `.Colors` / `.HeatMap` / `.Math2D` / `.Shapes` / `.Text`） | 2D 绘制能力 |
| `....Imaging.Drawing3D`（+ `.Math3D` / `.Models`） | 等轴测 3D 绘制能力 |
| `....Imaging.Driver` / `.Drivers` | `IGraphics` 契约与图形数据模型 |
| `....Imaging.SVG`（+ `.CSS` / `.XML` / `.PathHelper`） | SVG 输出与元素模型 |
| `....Imaging.PostScript`（+ `.PSElements`） | PostScript 输出与绘图元素 |
| `....Imaging.d3js`（+ `.scale` / `.labeler`） | d3.js 风格缩放与标签布局 |
| `....Imaging.Filters` | 图像滤镜 |

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drivers

' 通过 IGraphics 绘制，输出格式由驱动决定
Dim g As IGraphics = CreateGraphicsDriver(dev:=DeviceDescription.Png(800, 600))

Call g.DrawLine(Stroke.DodgerBlue, {New Point(0, 0), New Point(800, 600)})
Call g.Save("./output.png")
```

## 实现要点

- **驱动抽象的收益**：矢量输出（SVG / PostScript）与光栅输出（位图）的差异被完全隔离在驱动内，绘图代码不再出现「如果是 SVG 则……」这类分支。
- **标签避让为什么重要**：散点图与网络图的可读性瓶颈往往不是数据本身，而是**重叠的标签**；`Nudge` 用近似力导向的方法把标签推开，与 `physics/layout` 共享思路。
- **等轴测而非透视**：等轴测投影保持平行线平行，更适合技术插图与图表，因此本包的 3D 引擎选择等轴测而非透视投影。

## 包信息

- Assembly：`Microsoft.VisualBasic.Imaging`
- TargetFramework：`net10.0`
- Tags：`scibasic;graphics;imaging;svg;postscript;drawing;heatmap;color-palette;d3js;isometric-3d`
- 许可：GPL-3.0-or-later
