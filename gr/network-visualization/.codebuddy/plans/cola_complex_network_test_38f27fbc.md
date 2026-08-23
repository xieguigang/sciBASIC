---
name: cola_complex_network_test
overview: 构建一个 100+ 节点的多层混合拓扑（网格块+链式+星型+独立分量）网络，用修正后的 Cola 应力最小化布局进行压力测试，并修复 LabelRendering.vb 的字体/颜色转换 bug 以启用节点标签渲染，最终通过 NetworkVisualizer.DrawImage 输出 PNG 供检查。
todos:
  - id: fix-label-rendering
    content: 修复 LabelRendering.vb 的 color 类型转换崩溃，支持 displayId:=True
    status: completed
  - id: add-complex-test
    content: 在 ColaTest.vb 新增 100+ 节点多层混合拓扑生成与 Cola 布局调用
    status: completed
    dependencies:
      - fix-label-rendering
  - id: render-complex
    content: 调用 NetworkVisualizer.DrawImage 渲染并保存 Cola_complex_layout.png（含标签）
    status: completed
    dependencies:
      - add-complex-test
  - id: verify-build
    content: 构建工程并验证复杂网络布局与标签渲染无异常
    status: completed
    dependencies:
      - render-complex
---

## 用户需求

构建一个更复杂的网络图用于测试已修正的 Cola 应力最小化布局算法，并修复标签渲染模块使节点标签可显示。

## 产品概述

在现有 test\ColaTest.vb 基础上，新增一个 100+ 节点的多层混合拓扑压力测试用例，复用已修正的 Cola 核心求解器进行布局，并渲染为 PNG 图像供目视检查。同时修复 Visualizer 中 LabelRendering 的历史崩溃缺陷，使节点标签（displayId:=True）可正常输出。

## 核心功能

- 程序化生成 100+ 节点多层混合拓扑：网格块（如 8×8 节点）、长链式结构、多星型簇、若干独立连通分量，刻意制造大量边交叉。
- 使用固定种子的随机散点作为 Cola 布局初始坐标，凸显应力最小化去交叉、去重叠效果。
- 桥接 NetworkGraph 到 Cola Node/Link，调用已修正的 Cola Layout（avoidOverlaps(False)，因 Projection 桩未完成），执行应力最小化布局并将坐标写回。
- 修复 LabelRendering.vb，使节点标签渲染（displayId:=True）不再抛 InvalidCastException。
- 通过 NetworkVisualizer.DrawImage 将该复杂网络布局渲染为 PNG（如 ./test/Cola_complex_layout.png）。

## 技术栈

- 语言：VB.NET（沿用现有 network_layout / test 工程）
- 布局算法：network_layout\Cola（已修正的 WebCola 翻译，应力最小化）
- 渲染：Microsoft.VisualBasic.Imaging + Visualizer.NetworkVisualizer.DrawImage
- 测试：test\ColaTest.vb（模块入口，Main 子过程）

## 实现方案

### 总体策略

沿用 ColaTest.vb 已有的 NetworkGraph↔Cola 桥接与布局调用模式，将 12 节点环+弦用例替换为程序化生成的 100+ 节点多层混合拓扑。核心改动分两部分：(1) 复杂网络数据生成与 Cola 调用；(2) 修复 LabelRendering 以支持 displayId:=True。

### 关键决策与权衡

1. **拓扑规模与结构**：参考 holaComplexTest 的网格+链+星+独立分量组合，放大到 100+ 节点（例如 8×8=64 网格 + 24 节点长链 + 3 个星型簇各 1hub+7leaf=24 节点 + 1 个 8 节点独立环，合计约 120 节点）。固定 Random 种子保证可复现。
2. **avoidOverlaps(False) 维持**：Layout\Projection.vb 的 ProjectionGroup 类型仍未定义（历史未完成任务），启用会编译/运行失败，故保持 avoidOverlaps(False)，仅验证应力最小化主路径。
3. **LabelRendering 修复策略**：优先采用"安全降级"方案——在 renderLabel 中对 label.color 做类型判断，若非 SolidBrush 则用 Brush 的通用取色方式（如通过反射或预设默认色）构造 SolidBrush，避免 DirectCast 崩溃；同时保证 labelColorAsNodeColor=False（默认）路径下 color.IsEmpty=True 时安全使用 defaultLabelColor。这样既能启用 displayId:=True，又不破坏既有行为。
4. **性能考量**：Cola 应力最小化为 O(n^2) 量级（descent 求解 + linklengths 矩阵），120 节点约需数千次迭代；convergenceThreshold 设为 0.01，linkDistance 用 symmetricDiffLinkLengths 合理控制边长。渲染阶段 labelerIterations 保持默认 1500 即可，避免过大画布导致标签退火过慢。

### 实现注意事项

- 复用现有 `g.AddNode`/`g.AddEdge`、`inode`/`ColaNode`/`ColaLink` 桥接代码，不引入新抽象。
- 初始坐标散点范围应与画布（如 1400×1400）匹配，避免布局后越界。
- 修复 LabelRendering 时保持 `labelColorAsNodeColor` 默认 False，仅修正崩溃分支与 color 类型安全。
- 输出文件名与已有 Cola_layout.png 区分，避免覆盖。
- 不改动已验证的 Cola 核心求解器（layout.vb/descent.vb/linklengths.vb/handledisconnected.vb）。

## 架构设计

沿用现有单模块测试结构，不新增项目或架构层：

- test\ColaTest.vb：新增 `ComplexColaTest()` 子过程，Main 中调用（保留原有 12 节点用例可选）。
- Visualizer\Render\LabelRendering.vb：局部修复 renderLabel 的 color 类型转换。

数据流：程序化拓扑 → NetworkGraph → Cola Node/Link 桥接 → Layout.start() 应力最小化 → 坐标写回 NetworkGraph → NetworkVisualizer.DrawImage(含标签) → PNG。

## 目录结构

```
network-visualization/
├── test/
│   └── ColaTest.vb              # [MODIFY] 新增 ComplexColaTest() 子过程，生成 100+ 节点多层混合拓扑，桥接 Cola 并渲染为 ./test/Cola_complex_layout.png；Main 中调用该过程（保留原用例注释）。
└── Visualizer/
    └── Render/
        └── LabelRendering.vb    # [MODIFY] 修复 renderLabel 中 label.color 非 SolidBrush 时的 InvalidCastException，使 displayId:=True 可用；保持 labelColorAsNodeColor=False 默认路径安全。
```

## 关键代码结构（可选）

LabelRendering.renderLabel 的修复要点（伪代码）：

```
' 当前崩溃点（第135-136行）
br = .color
br = New SolidBrush(DirectCast(br, SolidBrush).Color.Darken(0.005))
' 建议改为安全取色
Dim baseColor As Color = If(TypeOf .color Is SolidBrush, DirectCast(.color, SolidBrush).Color, defaultLabelColor.Color)
br = New SolidBrush(baseColor.Darken(0.005))
```