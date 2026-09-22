# 网络可视化图布局算法

## 引言

同一张图，不同的节点摆位会讲出完全不同的故事：

- 力导向布局让**社区结构**自然浮现；
- 正交布局适合展示**流程与依赖**；
- 径向布局突出**层次与中心性**；
- 边捆绑（edge bundling）用于缓解**密集图的视觉噪声**。

本包提供这些布局算法，并把它们统一到一个 `IPlanner` 接口之后——**换布局不用改渲染代码**。

## 设计目标

- **接口统一**：所有布局实现同一个规划器接口，输入图、输出节点坐标；
- **算法独立**：每个布局各自一个命名空间，可单独使用、单独测试；
- **可无头运行**：布局是纯算法，不依赖渲染，因此可以为静态图片预计算坐标。

## 核心特性

| 布局 | 命名空间 | 特点 |
|---|---|---|
| 力导向 | `ForceDirected` | 经典斥力 / 引力模型 |
| 弹簧嵌入 | `SpringForce`（+ `Interfaces`） | 基于弹簧能量最小化 |
| 约束布局（Cola） | `Cola`（+ `Models` / `Layout` / `Layout3D` / `PowerGraph` / `Geom` / `Models.Accessor`） | 支持约束的布局引擎，含 3D 与网格路由 |
| 正交布局 | `Orthogonal`（+ `optimization` / `util`） | 边沿水平 / 垂直通道走线，适合流程类图 |
| 径向布局 | `Radial` | 同心圆排布，突出层次 |
| 圆形布局 | `Circular` | 节点落在圆周上 |
| HOLA | `HOLA` | 高层有机布局 |
| 边捆绑 | `EdgeBundling.Mingle` | 把平行边收束成束，降低密集图噪声 |

## 使用方式

布局消费图模型并产出节点坐标（即 `Layouts` 命名空间中的向量类型），随后由渲染层（`Visualizer`、`NetworkCanvas`）绘制。

```vbnet
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts

' graph 来自 Datavisualization.Network
Dim planner As IPlanner = New SpringForcePlanner()
Dim layout = planner.Layout(graph)

' 之后交给渲染层绘制 layout
```

## 实现要点

- **为什么要有 `IPlanner`**：布局算法的「输入 / 输出」是天然统一的（图进、坐标出），把这一点固化后，算法之间可以互相替换，也便于对比不同布局的同一张图。
- **正交布局需要优化阶段**：正交布局先确定节点层级与通道，再通过优化阶段减少拐点与交叉；这就是 `Orthogonal.optimization` 存在的原因。
- **边缘捆绑的取舍**：捆绑显著降低视觉噪声，但会牺牲「单条边可追踪性」；因此它适合概览视图，而非精确查询视图。

## 包信息

- Assembly：`Microsoft.VisualBasic.Data.visualize.Network.Layouts`
- TargetFramework：`net10.0`
- Tags：`scibasic;graph-layout;force-directed;spring-embedder;cola;orthogonal-routing;hola;edge-bundling;radial`
- 许可：GPL-3.0-or-later
