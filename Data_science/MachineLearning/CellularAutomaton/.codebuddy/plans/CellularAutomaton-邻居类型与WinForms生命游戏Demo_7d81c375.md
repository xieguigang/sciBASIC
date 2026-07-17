---
name: CellularAutomaton-邻居类型与WinForms生命游戏Demo
overview: 完善 CellularAutomaton 库，新增冯·诺依曼型、摩尔型、扩展摩尔型三种邻居类型支持（含边界模式与同步双缓冲更新），并新建一个 WinForms 项目，运行可切换邻居类型的康威生命游戏（Conway's Game of Life）Demo。
design:
  styleKeywords:
    - Dark Mode
    - Neumorphism
    - Glassmorphism
    - Tech Console
    - Micro-animation
  fontSystem:
    fontFamily: Segoe UI, Microsoft YaHei
    heading:
      size: 18px
      weight: 700
    subheading:
      size: 13px
      weight: 600
    body:
      size: 11px
      weight: 400
  colorSystem:
    primary:
      - "#00E5FF"
      - "#2979FF"
      - "#1DE9B6"
    background:
      - "#1E1E2E"
      - "#16161F"
    text:
      - "#E0E0E0"
      - "#B0BEC5"
    functional:
      - "#FF5252"
      - "#69F0AE"
      - "#FFD740"
todos:
  - id: add-neighborhood
    content: 新增 Neighborhood.vb：枚举与偏移生成器
    status: completed
  - id: refactor-cellentity
    content: 重构 Individual 与 CellEntity 支持邻居配置与双缓冲提交
    status: completed
    dependencies:
      - add-neighborhood
  - id: sim-refactor
    content: 改造 Simulator 构造函数、索引器与两阶段 Step
    status: completed
    dependencies:
      - refactor-cellentity
  - id: create-winforms
    content: 创建 GameOfLife.WinForms 项目与 Program 入口
    status: completed
  - id: impl-demo
    content: 实现 ConwayCell 与 MainForm 交互式生命游戏
    status: completed
    dependencies:
      - sim-refactor
      - create-winforms
  - id: build-verify
    content: 构建并运行验证三种邻居类型与游戏行为
    status: completed
    dependencies:
      - impl-demo
---

## 用户需求

完善现有 VB.NET 元胞自动机类库，使其支持三种邻居类型；并新建一个 WinForms 桌面程序，用于运行康威生命游戏（Conway's Game of Life）可视化的 demo。

## 产品概述

在现有 `Microsoft.VisualBasic.MachineLearning.CellularAutomaton` 类库基础上，将原本硬编码的冯·诺依曼（4 邻居）扩展为可配置的三种邻居拓扑；同时提供一个 Windows 桌面程序，用户可在图形界面中实时运行生命游戏，并能切换邻居类型、边界模式、网格尺寸与速度，直观验证三类邻居的行为差异。

## 核心功能

- 类库新增三种邻居类型：冯·诺依曼型（半径1，4 邻居）、摩尔型（半径1，8 邻居）、扩展摩尔型（半径2，24 邻居），并支持按类型构建邻居引用。
- 新增边界模式：有界（Bounded，越界视为无邻居）与环面（Toroidal，环绕缝合）。
- 采用双缓冲同步更新（Tick 计算下一代、Commit 提交），保证同一世代基于同一帧状态演化。
- WinForms Demo：网格绘制、开始/暂停/单步/随机/清空/重置控制、速度滑块、网格尺寸、邻居类型与边界模式下拉、鼠标点击/拖拽绘制细胞。
- Demo 以 B3/S23 规则运行生命游戏，可在三种邻居类型下切换演示。

## 技术栈选择

- 语言/框架：VB.NET（.NET 10 / net10.0），沿用现有类库技术栈。
- 类库改造：仅修改 `SimulatorModel` 内的 `CellEntity.vb`、`Simulator.vb`、`Individual.vb`，新增 `Neighborhood.vb`，不改动外部依赖 `GridCell(Of T)`、`Iterator.Kernel` 签名。
- 桌面程序：新建 WinForms 项目（`<UseWindowsForms>true</UseWindowsForms>`、`OutputType=WinExe`），通过 `ProjectReference` 引用 `CellularAutomaton.vbproj`，复用库内 `Simulator(Of T)` 与 `Individual` 直接驱动演示。
- 渲染：双缓冲 `Panel` + `Bitmap` 离屏绘制网格，避免闪烁；计时器驱动演化步进。

## 实现方案

- 策略：以枚举 `NeighborhoodType`/`BoundaryMode` 参数化邻居与边界；`CellEntity.config(grid, type)` 依据偏移表构建邻居引用，`Simulator` 在 `[Step]` 中先全部 `Tick()` 再全部 `Commit()` 实现同步世代。
- 关键决策：

1. 邻居偏移集中由 `Neighborhoods.Offsets(type)` 生成（冯·诺依曼 4 个轴偏移；摩尔为切比雪夫半径1 共8 个；扩展摩尔为切比雪夫半径2 共24 个），与单元格逻辑解耦，新增拓扑只需扩充偏移表，符合开闭原则。
2. 为 `Individual` 接口新增 `Commit()`（库内无其他实现者，无破坏性），用双缓冲解决当前顺序更新污染同世代的问题，使生命游戏规则正确。
3. `getCell` 索引器修复既有 bug（`i >= size.Width` 应为 `size.Height`），并增加 `Toroidal` 环绕取模；`Bounded` 保持越界返回 Nothing。
4. Demo 直接复用库，UI 切换邻居/边界即重建 `Simulator` 实例，零额外规则耦合，最大化验证库功能。

- 性能与可靠性：演化主循环为 O(N)（N 为细胞数）；渲染仅重绘变化帧（Bitmap 双缓冲）；邻居引用在初始化阶段一次性构建，运行期无额外遍历。网格规模限制在合理范围（如 ≤ 300×300）以保证交互流畅。

## 实现注意

- 保持 `CellEntity`/`Simulator` 既有公有方法签名兼容（`Snapshot`、`RandomCells`、`TakeSnapshots` 等不动），仅增量新增重载/参数（带默认值，向后兼容）。
- `config` 方法改为 `config(grid As Simulator(Of T), type As NeighborhoodType)`；`adjacents` 长度随偏移数动态分配，避免固定 4 的假设。
- 索引器越界判定务必同时校验 `size.Height`/`size.Width`，修复原逻辑将行误判为列宽导致的越界访问。
- WinForms 项目设置 `<UseApplicationFramework>false</UseApplicationFramework>` 并以 `Sub Main`（`Program` 模块）启动，规避应用框架依赖。
- 事务化构建：先确保类库可独立编译，再构建 WinForms 项目，控制改动爆炸半径。

## 架构设计

现有架构：

- `Simulator(Of T)` 持有 `grid As CellEntity(Of T)()()`，负责初始化、步进调度、快照。
- `CellEntity(Of T)` 持有 `data As T` 与 `adjacents()` 邻居引用，驱动 `Tick`。
- `Individual` 为细胞行为契约。

改造后仍保持该分层，仅将“邻居拓扑”与“边界处理”上提为 `Simulator` 的可配置属性，由 `CellEntity.config` 在初始化时按拓扑构建引用。WinForms 作为消费端，仅依赖 `Simulator` 与 `Individual`，不参与规则计算。

```mermaid
graph TD
    A[WinForms Demo] -->|引用| B[CellularAutomaton 类库]
    B --> C[Simulator(Of T)]
    C --> D[CellEntity(Of T)]
    D --> E[Individual: Tick/Commit]
    C --> F[NeighborhoodType/BoundaryMode]
    F --> G[Neighborhoods.Offsets]
    A --> H[ConwayCell : Implements Individual]
```

## 目录结构

本实现修改现有类库 3 个文件并新增 1 个文件，同时新建 1 个 WinForms 项目（4 个文件）。

```
CellularAutomaton/
├── SimulatorModel/
│   ├── Neighborhood.vb      # [NEW] 定义 NeighborhoodType 枚举（VonNeumann/Moore/ExtendedMoore）、
│   │                       #        BoundaryMode 枚举（Bounded/Toroidal），以及 Neighborhoods.Offsets(type)
│   │                       #        偏移生成器。各拓扑偏移集中维护，便于扩展。
│   ├── Individual.vb       # [MODIFY] 为 Individual 接口新增 Sub Commit()，支持双缓冲同步更新；
│   │                       #          保留既有 Tick；Delegate ToInteger 不变。
│   ├── CellEntity.vb       # [MODIFY] config(grid, type) 按 Neighborhoods.Offsets 动态构建 adjacents；
│   │                       #          新增 Sub Commit() 转发 data.Commit()；Tick() 仅计算下一代。
│   └── Simulator.vb        # [MODIFY] 构造函数增加 neighborType/boundary 参数（默认值保兼容）；
│   │                       #          修复 getCell 越界判定（补 size.Height）；getCell 支持 Toroidal 环绕；
│   │                       #          [Step] 改为两阶段：先全部 Tick() 再全部 Commit()。
└── GameOfLife.WinForms/    # [NEW] WinForms 桌面演示项目
    ├── GameOfLife.WinForms.vbproj  # [NEW] net10.0 / WinExe / UseWindowsForms=true；
    │                               #         ProjectReference 引用 ..\CellularAutomaton.vbproj；
    │                               #         UseApplicationFramework=false，StartupObject=Program。
    ├── Program.vb                # [NEW] Module Program，Sub Main：Application.Run(New MainForm)。
    ├── ConwayCell.vb             # [NEW] 实现 Individual 的 Conway 细胞：State/nextState 布尔字段；
    │                               #         Tick 统计活邻居数并按 B3/S23 规则计算 nextState；
    │                               #         Commit 提交 nextState→State；暴露 State 供渲染。
    └── MainForm.vb               # [NEW] 主窗体（代码内 InitializeComponent，无 Designer/Resx）。
                                    #         双缓冲 Panel 网格渲染；Timer 驱动 simulator.Run(False)；
                                    #         按钮（开始/暂停/单步/随机/清空/重置）、速度滑块、
                                    #         网格尺寸、邻居类型与边界模式下拉；鼠标绘制细胞。
```

## 关键代码结构

```
' 邻居拓扑与边界模式（Neighborhood.vb）
Public Enum NeighborhoodType
    VonNeumann     ' 半径1，4 邻居
    Moore          ' 半径1，8 邻居
    ExtendedMoore  ' 半径2，24 邻居
End Enum

Public Enum BoundaryMode
    Bounded        ' 越界视为无邻居
    Toroidal       ' 环面环绕
End Enum

Public Module Neighborhoods
    ' 依据类型返回相对偏移集合（不含中心 (0,0)）
    Public Function Offsets(type As NeighborhoodType) As Point()
End Module

' 更新后的细胞行为契约（Individual.vb）
Public Interface Individual
    Sub Tick(adjacents As IEnumerable(Of Individual))
    Sub Commit()   ' 提交 Tick 计算出的下一代状态
End Interface
```

## 设计风格

采用现代深色科技控制台风格（暗色 Neumorphism/Glassmorphism 取向），主窗口深蓝灰背景，控制面板使用半透明卡片与柔和阴影分层。网格以纯黑表示死亡、亮青色表示存活，演化过程配合细微的亮度渐变与状态切换微动效，营造“鲜活”的仿真氛围。整体布局自上而下：顶部标题与状态栏、中部大尺寸网格画布、底部控制工具条，结构清晰且操作直观。

## 页面/窗口区块规划

1. 顶部标题栏：应用名“Conway's Game of Life”与当前世代计数、存活数实时显示。
2. 工具栏区块：开始/暂停、单步、随机、清空、重置按钮，采用圆角半透明按钮，hover 高亮。
3. 参数面板区块：邻居类型下拉（冯·诺依曼/摩尔/扩展摩尔）、边界模式下拉（有界/环面）、网格尺寸数字框、速度滑块。
4. 主网格画布区块：双缓冲 Panel 绘制细胞网格，支持鼠标点击/拖拽绘制或擦除细胞，4px 圆角单元格、细网格线。
5. 底部状态栏：显示 FPS、当前规则 B3/S23 与提示信息。