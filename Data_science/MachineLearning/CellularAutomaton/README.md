# 元胞自动机网格模拟器（含 netCDF 快照导出）

## 引言

元胞自动机的规则极简：**每个格子的下一状态，只由它和邻居的当前状态决定**。但这条简单规则能涌现出惊人的复杂度——从生命游戏到反应扩散斑图，从森林火灾到传染病扩散。

本包提供一个**通用**的元胞自动机引擎：格子类型可泛型化，邻域与边界可配置，状态可导出。

## 设计目标

- **更新语义正确**：采用「tick-and-commit」——先计算所有格子的新状态，再统一提交。这避免了「就地更新」导致结果依赖遍历顺序的问题；
- **可泛型**：格子承载任意类型（布尔、整数、枚举、结构体）；
- **可观测**：逐格状态可导出快照用于分析或动画。

## 核心能力

| 能力 | 说明 |
|---|---|
| **2D 网格演化** | 逐代计算邻域并提交新状态 |
| **可配置邻域** | 冯·诺依曼（4 邻域）/ 摩尔（8 邻域）等 |
| **可配置边界** | 环绕（周期性）或固定边缘 |
| **泛型格子** | 格子内容类型由调用方决定 |
| **netCDF 快照** | 把逐格状态导出到 netCDF 供后续分析 / 动画 |

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.MachineLearning.CellularAutomaton

' 1. 定义格子状态类型与初始网格
Dim grid = Grid(Of CellState).Create(rows:=200, cols:=200, initial:=Rule.Seed())
grid.Neighbourhood = Neighbourhood.Moore
grid.Boundary = Boundary.WrapAround

' 2. 推进若干代（每代 tick -> commit）
Dim engine As New Automaton(Of CellState)(grid, rule:=AddressOf MyRule)

For i As Integer = 1 To 500
    Call engine.Step()
Next

' 3. 导出快照（可用于分析或生成动画）
Call engine.ExportSnapshots("./snapshots.nc")
```

## 实现要点

- **为什么必须「先算后提交」**：如果就地更新，格子 (i, j) 的新状态会影响 (i, j+1) 的计算——结果会随遍历顺序改变，从而失去"每一步都是同步更新"的数学含义。双缓冲是元胞自动机的标准做法。
- **边界条件的重要性**：周期边界（环绕）模拟"无限平面"，固定边界引入人工边界效应；选择不同会让演化结果显著不同（例如波的反射）。
- **netCDF 快照的价值**：元胞自动机的结果是一个**时间序列的场**——正是 netCDF 最擅长的数据结构；导出后可以直接用标准工具做统计与可视化。
- **规则的表达**：规则被抽象为「读取邻域 → 返回新状态」的函数，因此可以是任意复杂逻辑（查表、概率、甚至调用外部模型）。

## 包信息

- Assembly：`Microsoft.VisualBasic.MachineLearning.CellularAutomaton`
- TargetFramework：`net10.0`
- Tags：`scibasic;cellular-automaton;simulation;lattice-gas;reaction-diffusion;netcdf;grid-model`
- 许可：GPL-3.0-or-later
