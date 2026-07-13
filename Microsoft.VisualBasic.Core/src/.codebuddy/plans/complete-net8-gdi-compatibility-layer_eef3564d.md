---
name: complete-net8-gdi-compatibility-layer
overview: 完善 `Drawing\netcore8.0` 文件夹中的 GDI+ 兼容层数据对象。补全 Brushes/Pens 全部约140个颜色属性、填充空壳类（HatchBrush/PathGradientBrush/ColorBlend/CustomLineCap/AdjustableArrowCap）、新增 LinearGradientBrush/Blend/LinearGradientMode/SystemBrushes/SystemPens、新增 Matrix/Region/GraphicsState/Metafile 对象、新增 SmoothingMode/InterpolationMode 等绘图质量枚举、新增 ImageFormat/ColorPalette、补全 GraphicsPath 更多操作方法（AddBeziers/AddPie/AddClosedCurve/AddPath/GetBounds/Flatten/Widen/Warp/Transform 等）、补全 Font 便捷属性。所有修改仅存储绘图操作参数，不实现实际渲染调用。
todos:
  - id: complete-brushes
    content: 补全 Brush.vb 中 Brushes 类的全部约 140 个标准命名颜色静态属性（在现有 17 个基础上补全约 120+ 个缺失颜色）
    status: completed
  - id: fill-brush-shells
    content: 完善 Brush.vb 中的空壳类：HatchBrush 存储构造参数和属性、PathGradientBrush 补全属性、ColorBlend 填充构造函数、新增 Blend 类、LinearGradientMode 枚举、LinearGradientBrush 类、SystemBrushes 类（约 30 个系统颜色）
    status: completed
    dependencies:
      - complete-brushes
  - id: complete-pens
    content: 补全 Pen.vb 中 Pens 类的全部约 140 个标准命名颜色静态属性（在现有 8 个基础上补全约 130+ 个缺失颜色）
    status: completed
  - id: fill-pen-shells
    content: 完善 Pen.vb 中的空壳类：CustomLineCap 和 AdjustableArrowCap 补全属性并填充构造函数、新增 SystemPens 类（对应约 30 个系统颜色）
    status: completed
    dependencies:
      - complete-pens
  - id: complete-graphicspath
    content: 实现 GraphicsPath.vb 中 PathData/PathPoints getter、新增 FillMode 枚举与属性、新增 SmoothingMode/InterpolationMode/CompositingMode/PixelOffsetMode/QualityMode 枚举与 GraphicsState 类、补全 StartFigure/AddPie/AddClosedCurve/AddPath/AddBeziers/GetBounds/Flatten/Widen/Warp/Transform/Reverse/IsVisible/IsOutlineVisible 方法及对应操作子类
    status: completed
  - id: complete-font
    content: 补全 Font.vb 中 Font 类的 Bold/Italic/Underline/Strikeout 便捷属性和无参 GetHeight 方法
    status: completed
  - id: add-matrix
    content: 新建 Matrix.vb 文件：实现 Matrix 变换矩阵类，存储 Elements 数组，提供 RotateAt/Rotate/Scale/Shear/Translate/Multiply/Invert/Reset/TransformPoints/TransformVectors 方法存储操作参数，IsIdentity/IsInvertible 属性
    status: completed
  - id: add-region
    content: 新建 Region.vb 文件：实现 Region 裁剪区域类，存储 GraphicsPath 和操作列表，提供 MakeEmpty/MakeInfinite/Complement/Exclude/Intersect/Union/Xor/Translate/Transform/GetBounds/IsVisible 方法存储操作参数
    status: completed
    dependencies:
      - complete-graphicspath
  - id: complete-image
    content: 完善 Image.vb：新增 ImageFormat 类（Bmp/Gif/Jpeg/Png 等静态 Guid）、ColorPalette 类（Entries/Flags）、Metafile 类（继承 Image 并实现 MustOverride 成员）
    status: completed
---

## 用户需求

完善 `Drawing\netcore8.0` 文件夹中的 GDI+ System.Drawing 兼容层数据对象，创建与 System.Drawing.Common 接口一致的纯数据存储对象。使之前依赖 GDI+ 作图的数据可视化项目，在 Linux 平台无法使用 GDI+ 的情况下，能够通过相同的函数调用签名来兼容 SkiaSharp 函数库。

## 产品概述

一个纯数据对象的兼容层，镜像 System.Drawing.Common NuGet 包中的 GDI+ 绘制对象接口。所有类仅存储绘图操作参数（属性和字段），不包含任何实际渲染逻辑（不调用 SkiaSharp/Windows API/Linux API）。

## 核心功能（完整版）

### Brushes / Pens 补全

- 补全 `Brushes` 类全部约 140 个标准命名颜色静态属性
- 补全 `Pens` 类全部约 140 个标准命名颜色静态属性
- 新增 `SystemBrushes` 类（ActiveBorder/ActiveCaption/Control/Desktop/Highlight/HighlightText 等约 30 个系统颜色静态画刷）
- 新增 `SystemPens` 类（对应 SystemBrushes 的系统颜色静态画笔）

### 画刷相关完善

- 完善 `HatchBrush`：存储 HatchStyle、ForegroundColor、BackgroundColor 构造参数
- 完善 `PathGradientBrush`：补全 CenterColor、SurroundColors、CenterPoint、FocusScales、Blend、Transform、Rectangle 属性
- 完善 `ColorBlend`：填充构造函数，正确初始化 Colors/Positions 数组
- 新增 `Blend` 类：Factors/Single()、Positions/Single() 属性
- 新增 `LinearGradientBrush` 类：LinearColors、Rectangle、Angle、WrapMode、Blend、InterpolationColors、Transform、GammaCorrection 属性
- 新增 `LinearGradientMode` 枚举：Horizontal/Vertical/ForwardDiagonal/BackwardDiagonal

### 画笔相关完善

- 完善 `CustomLineCap`：WidthScale、HeightScale、BaseInset、BaseCap 属性
- 完善 `AdjustableArrowCap`：Width、Height、MiddleInset、Filled 属性，填充构造函数

### GraphicsPath 完善

- 实现 `PathData`/`PathPoints` getter（遍历操作列表动态计算）
- 新增 `FillMode` 枚举（Alternate/Winding）及 GraphicsPath.FillMode 属性
- 新增 `SmoothingMode` 枚举（Default/HighSpeed/HighQuality/None/AntiAlias/Invalid）
- 新增 `InterpolationMode` 枚举（Default/Low/High/Bilinear/Bicubic/NearestNeighbor/HighQualityBilinear/HighQualityBicubic/Invalid）
- 新增 `CompositingMode` 枚举（SourceOver/SourceCopy）
- 新增 `PixelOffsetMode` 枚举（Default/HighSpeed/HighQuality/None/Half/Invalid）
- 新增 `QualityMode` 枚举（Default/Low/High/Invalid）
- 新增 `GraphicsState` 类（存储状态索引）
- 补全操作方法：StartFigure、AddPie、AddClosedCurve、AddPath、AddBeziers、GetBounds、Flatten、Widen、Warp、Transform、Reverse、IsVisible、IsOutlineVisible（及对应 op_ 操作子类）
- 新增 AddEllipse 的 RectangleF 重载

### Font 补全

- Font 新增 Bold、Italic、Underline、Strikeout 只读便捷属性
- Font 新增无参 GetHeight() 方法

### Image 扩展

- 新增 `ImageFormat` 类（Bmp/Gif/Jpeg/Png/Tiff/Wmf/Emf/Exif/Icon/MemoryBmp 静态 Guid 属性）
- 新增 `ColorPalette` 类（Entries/Color()、Flags/Integer 属性）
- 新增 `Metafile` 类（继承 Image，存储 Header/MetafileHeader 数据）

### 新增独立文件

- `Matrix.vb`：变换矩阵类，存储 Elements/Single() 数组，提供 RotateAt/Rotate/Scale/Shear/Translate/Multiply/Invert/Reset/TransformPoints/TransformVectors 方法存储参数，IsIdentity/IsInvertible 属性
- `Region.vb`：裁剪区域类，存储 GraphicsPath 引用，提供 MakeEmpty/MakeInfinite/Complement/Exclude/Intersect/Union/Xor/Translate/Transform/GetBounds/IsEmpty/IsInfinite/IsVisible 方法存储操作参数

## 技术栈

- 语言：VB.NET
- 条件编译：`#If NET8_0_OR_GREATER Or NETSTANDARD2_0_OR_GREATER Then`
- 命名空间：`Microsoft.VisualBasic.Imaging`
- 基础类型引用：`Imports System.Drawing`（Color/PointF/Size/RectangleF 等值类型来自 System.Drawing.Common NuGet 包）
- 项目为 .NET SDK 风格，新增 .vb 文件放入目录后自动编译

## 实现方案

### 整体策略

严格遵循现有编码模式：每个类均为纯数据持有对象，构造函数保存参数到属性/字段，提供 Public Property 暴露数据，不包含任何实际绘图逻辑。

### 文件修改范围

**修改现有文件（5个）：**

| 文件 | 修改内容 |
| --- | --- |
| Brush.vb | 补全 Brushes（~120+ 属性）、完善 HatchBrush/PathGradientBrush/ColorBlend、新增 Blend/LinearGradientBrush/LinearGradientMode/SystemBrushes |
| Pen.vb | 补全 Pens（~130+ 属性）、完善 CustomLineCap/AdjustableArrowCap、新增 SystemPens |
| GraphicsPath.vb | 实现 PathData/PathPoints、新增 5 个 Drawing2D 枚举 + GraphicsState + FillMode、补全 13 个操作方法及操作子类 |
| Font.vb | 新增 4 个便捷属性 + 无参 GetHeight |
| Image.vb | 新增 ImageFormat/ColorPalette/Metafile 类 |


**新增文件（2个）：**

| 文件 | 内容 |
| --- | --- |
| Matrix.vb | Matrix 变换矩阵类 |
| Region.vb | Region 裁剪区域类 |


### 关键设计决策

1. **Brushes/Pens 模式**：沿用现有 `Public Shared ReadOnly Property Xxx As New SolidBrush(Color.Xxx)` / `New Pen(Color.Xxx)` 模式
2. **SystemBrushes/SystemPens**：引用 `SystemColors.Xxx`（来自 System.Drawing），采用与 Brushes/Pens 相同的静态属性模式
3. **操作子类模式**：GraphicsPath 的新方法严格遵循现有的 op_ 操作子类模式，每个操作子类存储对应参数
4. **枚举整数值对齐**：所有 Drawing2D 枚举（SmoothingMode/InterpolationMode 等）的整数值与 System.Drawing.Drawing2D 对齐
5. **Matrix**：使用 `Single()` 数组（6 元素）存储变换矩阵数据，通过方法名存储操作类型和参数
6. **Region**：以 GraphicsPath 为核心数据存储，通过操作列表记录所有区域变换
7. **PathData/PathPoints**：遍历 opSet 操作列表动态构建，可缓存结果
8. **ImageFormat**：使用 `Guid` 类型的静态只读属性，Guid 值与 System.Drawing.Imaging.ImageFormat 对齐
9. **Metafile**：继承 Image 抽象类，实现必需的 MustOverride 成员

### 性能考量

- GraphicsPath.PathData/PathPoints 首次计算后缓存结果，Reset/Add 操作时清除缓存
- Brushes/Pens/SystemBrushes/SystemPens 静态属性使用实例立即初始化（符合现有模式），首次访问即可用
- Matrix 的 Elements 数组存储 6 个 Single 值，内存开销极小
- Region 使用 List(Of region_op) 模式存储操作历史，与 GraphicsPath 的 opSet 模式一致