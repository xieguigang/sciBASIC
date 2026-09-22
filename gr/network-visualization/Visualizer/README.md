# 带 CSS 样式映射的网络图渲染引擎

## 引言

网络图渲染的难点不在于「把点和线画出来」，而在于**如何把数据映射为视觉属性**：

- 节点大小应该反映度数？还是表达量？
- 颜色应该按社区？还是按连续值？
- 同一张图在论文里和汇报里需要完全不同的配色。

本包用一套 **CSS 风格的数据映射表达式语言**解决这个问题：**样式规则是数据，而不是代码**。

## 设计目标

- **样式数据驱动**：视觉属性由表达式从节点 / 边数据计算得出；
- **图形元素完整**：节点、边、箭头、标签与凸包分组；
- **输出多样**：位图与矢量（交给 `Imaging` 的驱动层）。

## 核心特性

- **渲染**：把布局好的 `NetworkGraph` 渲染为位图或矢量图，绘制节点、边、箭头、标签与**凸包分组**；
- **样式映射**：`Styling` 命名空间实现 CSS 风格的 map 表达式语言，把数据值映射到颜色、形状与尺寸：
  - `Styling` —— 样式规则与表达式；
  - `Styling.FillBrushes` —— 表达式产出的填充画刷类型；
  - `Styling.Numeric` —— 数值型样式表达式；
  - `Styling.CSS` —— CSS 规则模型；
- **渲染实现**：`Render` 命名空间提供渲染器与凸包分组辅助，`MingleRender` 支持边捆绑渲染。

## 命名空间地图

| 命名空间 | 职责 |
|---|---|
| `Microsoft.VisualBasic.Data.visualize.Network`（根） | 渲染入口与共享类型 |
| `....Network.Render` | 渲染器与凸包分组 |
| `....Network.MingleRender` | 边捆绑渲染 |
| `....Network.Styling`（+ `.Expression` / `.Numeric` / `.FillBrushes` / `.CSS`） | 样式规则与数据映射表达式 |

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.Data.visualize.Network
Imports Microsoft.VisualBasic.Data.visualize.Network.Styling

' 用 CSS 风格的映射表达式描述样式
Dim style As String = "node { fill: map(degree); size: map(degree); }"

Dim renderer As New NetworkRenderer(style)

' layout 来自 network_layout
Call renderer.Render(graph, layout, "./network.png")
```

## 实现要点

- **为什么把样式做成「表达式」而不是参数**：参数化的样式只能覆盖预定义的几种映射；表达式则允许调用方自己写规则，甚至按数据字段组合出新的视觉维度。
- **凸包分组的价值**：社区 / 聚类结果如果只靠颜色区分，在密集图中很难看清边界；用凸包把同一组的节点包起来，能显著提升可读性。
- **与布局分工明确**：本包只做「已知坐标 → 像素」的映射，节点位置由 `network_layout` 决定，二者通过布局向量类型解耦。

## 包信息

- Assembly：`Microsoft.VisualBasic.Data.visualize.Network.Visualizer`
- TargetFramework：`net10.0`
- Tags：`scibasic;network-rendering;data-visualization;style-mapping;convex-hull;css;graphics`
- 许可：GPL-3.0-or-later
