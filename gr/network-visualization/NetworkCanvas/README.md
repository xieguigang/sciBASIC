# 交互式 WinForms 网络图浏览器控件

## 引言

静态图片能回答「这个网络长什么样」，但回答不了「点这个节点会展开什么」「布局动起来是什么样」。

本包提供的是一个 **Windows Forms 控件**：把布局好的网络图放到可交互画布上，实时动画展示力导向布局的收敛过程，并支持鼠标探查与录制导出。

## 设计目标

- **实时动画**：布局求解过程本身就是有价值的信息，控件直接把每一帧画出来；
- **2D 与 3D 共存**：同一控件可切换 2D 平面与 3D 视角；
- **可导出**：动画能录制为 AVI 或导出为 SVG，用于报告与演示。

## 核心特性

- **力导向布局动画**：实时展示布局收敛过程；
- **2D / 3D 渲染**：支持平面视图与 3D 视图（`Canvas3D`）；
- **交互能力**：鼠标命中测试（hit-testing）、节点悬浮提示（tooltip）、视口平移与缩放；
- **样式分层**：UI 元素样式（`Styling.UI`）与图渲染样式分离，互不干扰；
- **录制导出**：动画可导出为 AVI（经 `AVIMedia` 包）或 SVG。

## 命名空间与关键能力

全部类型位于根命名空间 `Microsoft.VisualBasic.Data.visualize.Network.Canvas`：

| 能力 | 说明 |
|---|---|
| 画布控件 | 承载并绘制网络图的 WinForms 控件 |
| 动画驱动 | 按帧推进布局并重绘 |
| 命中测试与提示 | 鼠标位置到节点 / 边的映射与信息展示 |
| 视口操作 | 平移与缩放 |
| `Canvas3D` | 3D 视角渲染 |
| `Styling.UI` | 控件 UI 元素样式 |

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.Data.visualize.Network.Canvas

' 在 WinForms 窗体上放置网络画布控件
Dim canvas As New NetworkCanvas()

Call canvas.LoadGraph(graph)
Call canvas.StartAnimation()

' 导出当前动画为视频
Call canvas.ExportAVI("./animation.avi")
```

## 实现要点

- **命中测试为何要独立实现**：网络图不是规则的控件布局，节点位置随时变化；命中测试必须在每帧之后基于当前坐标建立索引，否则交互会明显滞后。
- **动画与布局的协同**：布局算法是「逐步迭代」的，控件把每一步的中间坐标都画出来，用户因此能看到社区结构「涌现」的过程——这本身就是一种有效的可视化叙事。
- **样式分离的意义**：控件自身的 UI（边框、提示框、工具条）与图的渲染样式（节点颜色、边粗细）由不同的样式体系管理，避免两者互相覆盖。

## 包信息

- Assembly：`Microsoft.VisualBasic.Data.visualize.Network.Canvas`
- TargetFramework：`net10.0`
- Tags：`scibasic;network-canvas;winforms;interactive-viewer;data-visualization;animation`
- 许可：GPL-3.0-or-later
