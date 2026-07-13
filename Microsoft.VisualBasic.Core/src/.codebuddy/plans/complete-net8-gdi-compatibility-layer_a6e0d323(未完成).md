---
name: complete-net8-gdi-compatibility-layer
overview: 完善 `Drawing\netcore8.0` 文件夹中的 GDI+ 兼容层数据对象，包括：补全 Brushes/Pens 全部约140个颜色属性、填充空壳类（HatchBrush/PathGradientBrush/ColorBlend/CustomLineCap/AdjustableArrowCap）、新增 LinearGradientBrush/Blend/LinearGradientMode、实现 GraphicsPath.PathData/PathPoints、补全 Font 便捷属性、新增常见 Path 操作方法等。仅做数据参数存储，不实现实际绘图 API 调用。
todos:
  - id: complete-brushes
    content: 补全 Brush.vb 中 Brushes 类的所有约 140 个标准命名颜色静态属性
    status: pending
  - id: fill-brush-shells
    content: 完善 Brush.vb 中的空壳类：HatchBrush 存储构造参数和属性、PathGradientBrush 补全属性、ColorBlend 填充构造函数、新增 Blend 类、LinearGradientMode 枚举、LinearGradientBrush 类
    status: pending
    dependencies:
      - complete-brushes
  - id: complete-pens
    content: 补全 Pen.vb 中 Pens 类的所有约 140 个标准命名颜色静态属性
    status: pending
  - id: fill-pen-shells
    content: 完善 Pen.vb 中的空壳类：CustomLineCap 和 AdjustableArrowCap 补全属性
    status: pending
    dependencies:
      - complete-pens
  - id: complete-graphicspath
    content: 实现 GraphicsPath.vb 中 PathData/PathPoints getter、新增 FillMode 枚举与属性、补全 StartFigure/AddPie/AddClosedCurve/AddPath 方法及对应操作子类、新增 AddEllipse 的 RectangleF 重载
    status: pending
  - id: complete-font
    content: 补全 Font.vb 中 Font 类的 Bold/Italic/Underline/Strikeout 便捷属性和无参 GetHeight 方法
    status: pending
---

## 用户需求

完善 `Drawing\netcore8.0` 文件夹中的 GDI+ System.Drawing 兼容层数据对象，补全缺失的绘图参数存储对象，使之前依赖 GDI+ 作图的数据可视化项目能够通过相同的函数调用签名来兼容 SkiaSharp 函数库。

## 产品概述

一个纯数据对象的兼容层，镜像 System.Drawing.Common NuGet 包中的 GDI+ 绘制对象接口，仅存储绘图操作参数而不实现实际渲染逻辑。

## 核心功能

- 补全 Brushes 类所有约 140 个标准命名颜色静态属性
- 补全 Pens 类所有约 140 个标准命名颜色静态属性
- 完善 HatchBrush、PathGradientBrush、ColorBlend 的空壳类，使其正确存储构造参数
- 新增 Blend 类、LinearGradientBrush 类、LinearGradientMode 枚举
- 完善 CustomLineCap、AdjustableArrowCap 类，补全属性
- 实现 GraphicsPath 的 PathData/PathPoints getter，补全 FillMode 和缺失的路径操作方法
- Font 类补全 Bold/Italic/Underline/Strikeout 便捷属性和无参 GetHeight 方法

## 技术栈

- 语言：VB.NET
- 条件编译：`#If NET8_0_OR_GREATER Or NETSTANDARD2_0_OR_GREATER Then`
- 命名空间：`Microsoft.VisualBasic.Imaging`
- 基础类型引用：`Imports System.Drawing`（Color / PointF / Size / RectangleF 等值类型）
- 项目文件：Core.vbproj 需确认新文件的包含

## 实现方案

### 整体策略

遵循现有编码模式，每个类均为纯数据持有对象：构造函数保存参数到属性/字段，提供 Public Property 暴露数据，不包含任何实际绘图逻辑。Brushes/Pens 采用与现有完全一致的 `Public Shared ReadOnly Property Xxx As New SolidBrush(Color.Xxx)` / `New Pen(Color.Xxx)` 模式。

### 修改范围

四个现有文件需修改，不新增文件（所有新增类型放入对应现有文件以保持组织结构不变）：

**Brush.vb** — 补全 Brushes 属性、完善空壳类、新增 Blend/LinearGradientBrush/LinearGradientMode
**Pen.vb** — 补全 Pens 属性、完善 CustomLineCap/AdjustableArrowCap
**GraphicsPath.vb** — 实现 PathData/PathPoints、新增 FillMode 与缺失方法
**Font.vb** — 补全便捷属性

### 关键设计决策

- Brushes/Pens 颜色属性直接引用 `Color.XXX` 命名颜色（这些命名颜色来自 System.Drawing.Color 结构体或项目的 Color 扩展）
- HatchBrush 需保存三个参数：HatchStyle + 两个 Color
- PathGradientBrush 需保存路径、中心点、环绕颜色、混合模式等
- PathData.Types 使用 `Byte()` 数组存储点类型标志
- GraphicsPath.PathData/PathPoints 通过遍历 opSet 操作列表动态计算
- 所有新增类型严格遵循现有命名模式和注释风格

### 性能考量

- GraphicsPath.PathData 的惰性求值：可缓存计算结果避免重复遍历 opSet
- Brushes/Pens 静态属性使用延迟初始化 `New`，首次访问时创建实例（符合现有模式）