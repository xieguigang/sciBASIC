---
name: Fix-HOLA-edge-rendering
overview: 修复网络边"连接不正确"的问题：根因在 Visualizer/Render/EdgeRendering.vb 的 bends 渲染逻辑（用 SlideWindows(2) 只连相邻拐点，丢失起点与终点节点），以及 HOLA/OrthogonalRouter.vb 生成的 bends 比例语义错误。经审查，边连接数据(U/V 引用)本身正确，问题在渲染层与 bends 生成格式。
todos:
  - id: fix-render-bends
    content: 修改 Visualizer/Render/EdgeRendering.vb 的 rendering 函数，将 bends 拼接为 起点A+全部bends+终点B 的连续折线，修正首尾节点丢失
    status: completed
  - id: fix-router-bends
    content: 修改 network_layout/HOLA/OrthogonalRouter.vb，使两个拐点均使用 WayPointVector.CreateVector(pu,pv,...) 相对整条边 U→V 生成
    status: completed
  - id: verify-build-demo
    content: 重新构建 network_layout 与 test 工程，运行 holaTest 生成 HOLA_layout.png 与 HOLA_result.txt，目视验证边正确连接节点且坐标对齐
    status: completed
    dependencies:
      - fix-render-bends
      - fix-router-bends
---

## 用户需求

审查 HOLA 算法生成结果中"网络边连接不正确"的问题，确认是渲染层问题还是边连接数据未正确生成。经代码审查，结论如下：

- 边连接数据（Edge.U/V 引用）本身正确，HOLA 只读取 graphEdges、写回节点坐标与 bends，从未修改 U/V 逻辑连接。
- 真正的缺陷在渲染层 `Visualizer/Render/EdgeRendering.vb` 的 `rendering` 函数：当 `bends.Length >= 2` 时，使用 `SlideWindows(2)` 仅绘制相邻 bend 对，完全丢弃了起点 A 与终点 B，导致边线"漂浮"在中间拐点之间、不连接到节点。
- HOLA 的 `OrthogonalRouter.vb` 生成的 bends 格式也有误：第二个拐点用 `CreateVector(midPoint, pv, ...)`（相对子段）而非相对整条边 U→V，导致比例语义不一致。

## 核心修复内容

1. 修正渲染层 `EdgeRendering.rendering`：将"起点 A + 全部 bends + 终点 B"串成完整路径点序列，相邻两两绘制连续折线，不再丢失首尾节点。
2. 修正 HOLA `OrthogonalRouter` 的 bends 生成：两个拐点统一使用 `WayPointVector.CreateVector(pu, pv, hx, hy)` 相对整条边 U→V 计算比例。

## 技术栈

- 语言：VB.NET（`Option Strict On`），与现有工程一致
- 受影响工程：`Visualizer`（工作区根 `Visualizer/`，独立工程，含 `Render/EdgeRendering.vb`）、`network_layout`（含 `HOLA/OrthogonalRouter.vb`）
- 复用 API：`WayPointVector.CreateVector(ps, pt, hx, hy)`、`WayPointVector.GetPoint(sx, sy, tx, ty)`（位于 `Datavisualization.Network/Graph/Model/Handle/WayPointVector.vb`，语义已审查明确，不修改）

## 实现方案

采用"先定位后最小修正"策略，仅改动审查范围内明确有缺陷的两处，不触及 U/V 数据结构与 bends 语义约定：

### 关键决策

1. **渲染层为主因**：`EdgeRendering.rendering` 在 `bends.Length >= 2` 分支用 `bends.SlideWindows(2)` 两两配对，只画 `bend[i]→bend[i+1]`，丢失端点 A/B。修正为构造完整路径点序列 `{A, bend0, bend1, ..., B}`，相邻两两绘制，保证连续性。
2. **bends 语义统一**：每个 `WayPointVector` 都必须相对整条边 (U→V) 用 `CreateVector(pu, pv, hx, hy)` 生成；渲染时统一用 `GetPoint(a, b)`（a/b 为节点当前坐标）还原。HOLA 第二个拐点原用 `CreateVector(midPoint, pv, ...)` 属子段语义错误，改为 `CreateVector(pu, pv, midX, pv.Y)`。
3. **保持兼容**：无 bends / `DrawEdgeBends=False` 时仍画 A→B 直线；`bends.Length=1` 时按完整路径 `A→bend→B` 绘制，行为更正确且不回归。

### 性能与可靠性

- 渲染改动仅涉及 O(bends) 的列表拼接与相邻绘制，无额外复杂度；`SafeQuery`/`IsNullOrEmpty` 防御保留。
- 修改 `Friend Class EdgeRendering` 属同工程内部渲染逻辑，不影响外部 API；`Visualizer` 工程重新编译即可。
- 不改动 `WayPointVector`、不改动 `Edge.U/V`、不改动 HOLA 其它阶段，blast radius 最小。

## 实现注意事项

- `EdgeRendering` 使用 `Imports Microsoft.VisualBasic.ComponentModel.Algorithm.base`（`SlideWindows` 来源），修正后该用法可移除，避免误用。
- `DrawEdgeDirection` 箭头仅应在最后一段绘制，修正后维持 `i = path.Count - 2` 时启用。
- `bends.Any(Function(bend) bend.isNaN)` 的 NaN 防御需保留。
- 验证需同时覆盖"有 bends"（HOLA demo）与"无 bends"（其它用例）场景，避免回归。

## 架构设计

```mermaid
graph TD
    A[Edge.data.bends: WayPointVector()] --> B[EdgeRendering.rendering]
    B -->|当前bug: SlideWindows 2| C[只画 bend[i]→bend[i+1], 丢失A/B]
    B -->|修正后| D[路径=A + bends + B, 相邻两两绘制]
    D --> E[LineSegmentRender.Render 连续折线]
    F[HOLA.OrthogonalRouter] -->|CreateVector pu,pv| A
```

## 目录结构

```
Visualizer/
└── Render/
    └── EdgeRendering.vb          # [MODIFY] rendering 函数：bends 拼接为 A+bends+B 连续折线，修正首尾丢失
network_layout/
└── HOLA/
    └── OrthogonalRouter.vb      # [MODIFY] 两个 bend 均用 CreateVector(pu,pv,...) 相对整条边
test/
└── OrthogonalLayoutTest.vb      # [REFERENCE] demo 已正确，无需改动；用于验证 PNG
```

## 关键代码结构

渲染层修正后的核心逻辑（伪代码，非新增类型）：

```
Dim path As New List(Of PointF) From {a}
For Each bend In bends
    path.Add(bend.GetPoint(a.X, a.Y, b.X, b.Y))
Next
path.Add(b)
For i As Integer = 0 To path.Count - 2
    draw.drawDir = If(i = path.Count - 2, config.DrawEdgeDirection, False)
    Yield draw.Render(g, {path(i), path(i + 1)})
Next
```