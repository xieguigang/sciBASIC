---
name: complex-heatmap
overview: 在 Visualization/Visualization/ComplexHeatMap.vb 中实现带行列层次聚类树（含自写 Ward.D2）与行列颜色注释条、图例的复杂热图，并提供 NumericTable/CSV 入口与 test/HeatmapDemo.vb 演示（渲染 PNG 与参考图目视比对）。
todos:
  - id: project-ref
    content: 为 data_visualize-netcore5.vbproj 添加 hctree 工程引用并验证可构建
    status: completed
  - id: core-types
    content: 实现 HeatMapAnnotation、HeatMapScale、LinkageMethod 与 ComplexHeatMap 公共属性面
    status: completed
    dependencies:
      - project-ref
  - id: clustering
    content: 实现距离矩阵、行列 z-score 与 Ward.D2 凝聚，接入既有四种连接策略
    status: completed
    dependencies:
      - core-types
  - id: render
    content: 重写 PlotInternal：布局、双树、注释条、主体、标签、colorbar 与分类图例
    status: completed
    dependencies:
      - core-types
      - clustering
  - id: data-entries
    content: 实现 FromTable（NumericTable）与 FromCsv（数值列+文本注释）入口
    status: completed
    dependencies:
      - core-types
  - id: demo-verify
    content: 在 test/HeatmapDemo.vb 渲染 PNG，与参考图逐项目视核对并微调间距
    status: completed
    dependencies:
      - render
      - data-entries
---

## 用户需求

在 `G:\GCModeller\src\runtime\sciBASIC#\Data_science\Visualization\Visualization\ComplexHeatMap.vb` 中，基于现有绘图代码库实现一个「带层次聚类树 + 颜色标签注释」的复杂热图，版式参照 `F:\datapool\20260301\202609\analysised\MRM_flavone_validation\figures\Fig1b_heatmap_top30.png`（R pheatmap 输出：30 行 × 15 列、行 z-score 后截断到 ±2、蓝-白-红发散色阶、顶部列聚类树、左侧行聚类树、列注释条 Variety、右侧行标签、竖直 colorbar 与分类图例、底部竖排列标签）。

## 产品概述

为 sciBASIC 数据可视化层补齐「杂志级复合热图」能力：调用方只需给矩阵（或表格/CSV）+ 行名/列名 + 注释，即可一次输出可直接用于论文插图的复杂热图对象，并支持 PNG/SVG/PDF 导出。

## 核心功能

- **行列双聚类树**：欧氏距离 + 5 种连接策略（Single / Complete / Average / Weighted / **Ward.D2**，后者自行实现以对齐 R 的 `ward.D2`）；可自动聚类，也可从外部传入已有聚类树；可单独关闭行树或列树。
- **颜色标签注释**：列注释（热图上方横条，可多条堆叠，如 Variety）、行注释（热图左侧竖条）；同一套注释既支持离散分类（色块 + 分类图例），也支持连续数值（渐变条 + 迷你色标），并自动判定类型；注释名可显示在注释条端部（加粗，如 "Variety"）。
- **热图主体**：发散/顺序色阶，支持自定义颜色停靠点（`#2166AC`-白-`#B2182B`）；行/列 z-score 标准化；色阶上下限截断；NaN 单元格以浅灰显示；可选单元格边框与单元格内数值。
- **标签排版**：行标签右对齐置于热图右侧（亦可放左侧），逐行垂直居中；列标签竖直旋转 270°（文字自下往上读），按文字长度自适应底部留白。
- **图例**：竖直连续 colorbar（刻度数字，标题可选）+ 右排离散分类图例（加粗标题、色块、类别名），位置与参考图一致。
- **数据入口**：①直接给 `Double(,)` + 标签数组；②从 `NumericTable` 构建（矩阵取 `features`，其标签列可作为注释）；③从 CSV 构建（数值列进矩阵、文本列自动成为离散注释）。
- **可视效果**：树、注释条、热图、标签、图例在同一画布上严格对齐（聚类树叶子必须与热图行列一一对应），整体风格与参考图一致（白底、无单元格边框、紧凑留白）。

## Tech Stack

- 语言/框架：VB.NET，`net10.0`，工程 `Visualization/Visualization/data_visualize-netcore5.vbproj`（RootNamespace `Microsoft.VisualBasic.Data.visualize`，`GenerateDocumentationFile=True`，Debug/x64 下 `RemoveIntegerChecks=true`）。
- 绘图：DataPlot 的 `PlotEngine` / `HeatmapPlot` / `ColorScale` / `PlotTheme` + `IGraphics`（GDI 栅格驱动；demo 工程已 `ImageDriver.Register()`）。
- 聚类：`hctree.NET5.vbproj` 的 `Cluster`、`DefaultClusteringAlgorithm`、四种连接策略、`DistanceMethods.EuclideanDistance`；Ward.D2 在本模块内自实现。
- 数据：`Microsoft.VisualBasic.Data.NumericTable`（Core）、`Microsoft.VisualBasic.Data.Framework.DataFrame.read_csv`。
- 演示：`Visualization/Visualization/test`（`net10.0-windows`，`StartupObject=test.HeatmapDemo`，已引用 Drawing-net4.8 / HCTreePlot / hctree / DataPlot / data_visualize）。

## Implementation Approach

1. **单入口类**：`ComplexHeatMap : Inherits HeatmapPlot`，完全重写 `PlotInternal`，自行计算"列树高 → 列注释高 → 主体 → 列标签高"与"行树宽 → 行注释宽 → 行标签宽 → 右侧图例宽"，得到 body 矩形后统一派生 `cellW/cellH`；所有子图元共用同一套坐标换算，保证像素级对齐（基类的 100px 硬编码留白无法满足参考图版式）。
2. **顺序一致性是核心正确性要求**：叶子顺序必须与树完全一致，采用既有库唯一约定 `Cluster.OrderLeafs()` —— 子节点按 `Leafs` 升序递归展开；行树、列树、矩阵、注释条全部由同一份 `RowOrder`/`ColOrder` 索引驱动。
3. **Ward.D2（对齐 R `hclust(method="ward.D2")`）**：合并准则 `Δ = n_i·n_j/(n_i+n_j) · ||c_i−c_j||²`，合并高度 `= ||c_i−c_j||`（质心欧氏距离）；用 Lance-Williams 在平方距离矩阵上 O(n²)/轮 更新，总 O(n³) 时间、O(n²) 空间（n=300 约 2.7e7 次简单运算，可接受；n>500 建议改用已有 Average 通道）。
4. **数值一致性**：z-score 使用**样本标准差（n−1，与 R `scale()` 相同）**，常数行/列（sd=0）置 0；`MinValue/MaxValue = ±2` 即等价 R 的事先截断（取色端内部已 clamp）。
5. **注释模型单一化**：一个 `HeatMapAnnotation` 同时承载离散（`Values As String()` + 颜色映射）与连续（`NumericValues As Double()` + 渐变），默认按"唯一值 ≤ 12 → 离散，否则连续"自动判定，可显式覆盖；列注释多条则纵向堆叠，行注释多条则横向并排。
6. **图例自绘**：为避免"单元格用自定义色阶、图例却用枚举色阶"的不一致，colorbar 统一自绘（取色走同一 `Func(Of Double, Color)`，刻度用 `Geometry.NiceTicks`），离散图例为加粗标题 + 色块 + 文字。
7. **性能与规模**：主体绘制为 `rows × cols` 次 `FillRectangle`（不可避免的 O(R·C)），距离矩阵 O(n²)，聚类只在树为空时算一次并缓存；文字宽度用 `MeasureString` 预计算并缓存（每行/每列只测一次），避免逐帧重复测量。

## Implementation Notes（执行要点 / 防回归）

- **`IGraphics` 没有 `DrawString(..., StringFormat)` 重载**（`HeatmapPlot` 里构造的 `StringFormat` 实际从未传入，属死代码）。因此右对齐、居中、旋转定位必须用 `MeasureString` 手算锚点；旋转文字只允许 `TranslateTransform → RotateTransform → DrawString(s, font, brush, x, y) → ResetTransform`。
- 禁止使用 `DrawString(s, font, brush, ByRef x, ByRef y, angle)`：GDI / SVG / PS 三个驱动对该重载的对齐语义不一致。
- 旋转 270° 的定位公式（GDI，`RotateTransform(270)` 后局部 +x 指向屏幕上方、局部 +y 指向右方）：`Translate(X − h/2, topGap + w); RotateTransform(270); DrawString(label, font, brush, 0, 0)`，其中 `w/h = MeasureString(label)`，`X = 列中心`。
- `Microsoft.VisualBasic.Imaging.Font` 的 `Size` 单位是**像素**（`SizeInPoints = Size × 0.75`），构造用 `New Font(name, sizePx, FontStyle)`；文件内需 `Imports Font = Microsoft.VisualBasic.Imaging.Font` 与 `Imports std = System.Math`（避免被 `Microsoft.VisualBasic.Math` 遮蔽）。
- `Cluster.LeafCount`（WriteOnly）与 `Cluster.removed` 是 `Friend`，跨程序集不可写：手工建树时不要设置，`Leafs` 会自动回退到 `CountLeafs` 递归并缓存；`Distance` 位于 `...HierarchicalClustering.Hierarchy` 子命名空间。
- VB 大小写不敏感：方法内局部变量不得与属性同名（`Matrix`、`MinValue`、`MaxValue`、`ColorMap`、`Scale`、`RowOrder`、`ColOrder` 全部是属性名）。若 `Scale` 与其它类型成员冲突则改名 `ScaleMode`。
- `GenerateDocumentationFile=True`：所有新增 Public 成员必须有 XML 注释；XML 注释内不能出现 `--`。
- 不改动 `HeatmapPlot.vb`、`ColorScale.vb`、`PlotTheme.vb`、hctree 与 HCTreePlot 源码；本次只新增实现、一条 ProjectReference、一个 demo 文件内容。
- 边界情形必须安全：n=1、全等值、含 NaN、树为 Nothing（不聚类）、只画行树/只画列树、注释值个数与行列数不匹配（明确抛 `ArgumentException` 并给出中文说明）、body 宽高被挤压到 ≤0 时提前返回。
- demo 验证时务必使用**新构建产物**（`test/bin/x64/Debug/net10.0-windows` 下已有旧 dll）。

## Architecture Design

```mermaid
graph LR
  A[Double(,) / NumericTable / CSV] --> B[标准化：z-score 行/列]
  B --> C[距离矩阵 + 连接策略]
  C --> D[行 Cluster / 列 Cluster]
  D --> E[OrderLeafs 顺序索引]
  E --> F[布局矩形计算]
  F --> G[列树 + 行树]
  F --> H[注释条 + 图例]
  F --> I[热图主体 + 行列标签]
```

组件划分（全部位于 `ComplexHeatMap.vb`，以同文件多类型方式组织）：

- `ComplexHeatMap`：公共 API 门面 + 布局 + 主体/标签绘制 + 入口工厂（`FromTable` / `FromCsv`）。
- `HeatMapAnnotation`：注释数据模型（离散/连续、颜色映射、自动类型判定、图例项枚举）。
- `HeatMapScale`（枚举）：None / Row / Column。
- `LinkageMethod`（枚举）：Single / Complete / Average / Weighted / Ward。
- `Friend Module HeatMapDendrogram`：`OrderedChildren`、`LeafSlots`、`MaxHeight`、`DrawColumnTree`、`DrawRowTree`（正交肘形，等距叶子）。
- `Friend Module WardClustering`：`Agglomerate(data, names) As Cluster`。
- `Friend Module HeatMapLegend`：连续 colorbar、离散图例、色块+文字行高计算。

## Directory Structure

```
Data_science/Visualization/Visualization/
├── data_visualize-netcore5.vbproj   # [MODIFY] 新增 ProjectReference 指向 ..\..\DataMining\hierarchical-clustering\hierarchical-clustering\hctree.NET5.vbproj（已确认无循环：hctree 只依赖 Core + Math + DataMining，后者只依赖 Core + Math）。不要引用 HCTreePlot。
├── ComplexHeatMap.vb                # [MODIFY] 全部实现。当前仅 9 行空壳（Inherits HeatmapPlot + 三参构造）。需实现：
│                                    #   构造函数：调用基类 (width,height,theme,driver)，置 ColorMap=Seismic、Scale=Row、
│                                    #   RowLinkage/ColLinkage=Ward、AutoClusterRows/Columns=True、RowLabelsOnRight=True、
│                                    #   ColLabelAngle=270、ShowCellBorder=False、RowDendrogramWidth=90、ColDendrogramHeight=90 等默认值。
│                                    #   属性面：PaletteColors As Color()、Scale、Row/ColLinkage、AutoClusterRows/Cols、Row/ColTree、
│                                    #   ShowRowDendrogram/ShowColDendrogram、DendrogramColor/LineWidth、RowDendrogramWidth、
│                                    #   ColDendrogramHeight、ColAnnotations/RowAnnotations As List(Of HeatMapAnnotation)、
│                                    #   ColAnnotationHeight、RowAnnotationWidth、ShowAnnotationName、RowLabelsOnRight、
│                                    #   ColLabelAngle、RowLabelFont/ColLabelFont、ShowCellBorder/CellBorderColor、NaColor、
│                                    #   ShowAnnotationsLegend、LegendTitleFont、RowOrder/ColOrder（只读，Plot 后就绪）。
│                                    #   PlotInternal：布局 → 背景/标题 → 两棵树 → 注释条 → 主体单元格 → 边框/标签 → 图例。
│                                    #   FromTable / FromCsv：见下。
└── test/
    └── HeatmapDemo.vb               # [MODIFY] 保持已有 `Sub New`（ImageDriver.Register()），填充 Sub Main：
                                     #   1) 30×15 合成数据（5 组 × 3 样本，含组特异性模式 + 噪声），行名用参考图 30 个化合物名、
                                     #      列名 CM104_1..3 / Jiazhuo_1..3 / CC20J_1..3 / 13S962_1..3 / 13S938_1..3；
                                     #   2) 复刻参考图参数：行 z-score、±2、PaletteColors={#2166AC,White,#B2182B}、Ward、
                                     #      列注释 Variety（5 类，红/橙/蓝/绿/灰）、270° 列标签、右侧行标签；画布约 1074×955，
                                     #      输出 complex-heatmap.png；
                                     #   3) 追加 FromTable(NumericTable) 与 FromCsv(临时 CSV) 两条入口各输出一张 PNG（校验入口正确性）。
```

## Key Code Structures

```
''' <summary>热图行列的颜色标签注释（离散分类或连续数值）。</summary>
Public Class HeatMapAnnotation
    Public Property Name As String                       ' 图例/注释条标题，如 "Variety"
    Public Property Values As String()                    ' 离散注释值（与行数或列数一致）
    Public Property NumericValues As Double()             ' 连续注释值（与 Values 二选一）
    Public Property Colors As Color()                     ' 离散：候选色板；连续：渐变停靠点
    Public Property ColorMap As Dictionary(Of String, Color) ' 离散：显式类别→颜色
    Public Property Kind As AnnotationKind                ' Auto / Discrete / Continuous
    Public Property ShowInLegend As Boolean
End Class

''' <summary>在给定数据上执行 Ward.D2 凝聚层次聚类，返回以质心欧氏距离为合并高度的聚类树。</summary>
Friend Function WardCluster(data As Double()(), names As String()) As Cluster
```

（其余接口以 XML 注释在实现时说明，不在此处给出不确定签名。）

## Agent Extensions

- **code-explorer**（SubAgent）
- Purpose: 在实现阶段快速核对 `IGraphics` 重载、`NumericTable`/`DataFrame` 具体成员、`Geometry.NiceTicks` 所在命名空间等易变细节，避免臆造 API。
- Expected outcome: 得到可直接编译调用的精确签名清单，减少编译-返工循环。

- **lsp-code-analysis**（Skill）
- Purpose: 定位 `ComplexHeatMap`、`HeatmapPlot.PlotInternal`、`Cluster.OrderLeafs` 的引用与实现位置，做改动影响面分析（确认无其它代码依赖 `ComplexHeatMap` 现有空壳签名）。
- Expected outcome: 明确"只新增不改旧"的改动边界，避免破坏既有调用方。