# 三维 SPH 流体与 Boids 群集粒子引擎

## 引言

粒子系统有两类截然不同的用途：一类只追求「看起来像」（爆炸、火花、烟雾），另一类需要「算得对」（液体、发酵罐搅拌、血液流动）。前者要求极低的单片开销以承载海量粒子，后者要求每一步都求解真实的密度与压力。

本包把两者都实现在同一个程序集里，共享同一套空间索引与积分内核：

- **游戏粒子**：`ParticleSystem` + `Emitter`，对象池复用、只受重力与线性阻力影响；
- **三维 SPH 流体**：`FluidEngine3D` + `SphParams3D` / `SphState3D`，求解密度、压力与粘性；
- **Boids 群集**：二维 `Field` 与三维 `Field3D`，分离 / 对齐 / 聚合三条规则的流式积分内核。

## 设计目标

- **Structure-of-Arrays**：三维 SPH 的全部状态（位置、速度、预测位置、密度、压力）以 `Single` 数组分列存储，可**零转换**上传到 CUDA 设备缓冲；
- **计算后端可插拔**：`ISphCompute3D` 抽象一步求解，默认是并行的 CPU 实现 `SphCpuCompute3D`；渲染侧可注入 GPU 实现，物理语义不变；
- **零分配内核**：群集与积分内核按「数组 + 下标」工作，单帧不产生托管分配；
- **边界是一等公民**：盒体、圆柱容器与旋转叶轮都是可组合的边界对象，而不是硬编码在求解器里的 `If`。

## 核心特性

- **三维 SPH 求解器**：密度 / 近密度、压力 / 近压力、粘性与碰撞阻尼，声速与 CFL 子步自适应（`CflFactor`、`MaxSubSteps`），密度自动标定（`AutoCalibrateDensity`）；
- **容器与搅拌**：`BoxBoundary3D`（轴向盒）、`CylinderBoundary3D`（立式圆柱罐，含壁面摩擦）、`SphImpeller3D`（Rushton 圆盘涡轮 + 轴，作为运动边界驱动流体，`SetRpm` / `Advance` / `Drive`）；
- **邻域搜索**：`Grid3D` / `UniformGrid3D`（计数排序的均匀网格），查询只访问平滑半径覆盖的 27 个单元；
- **三维群集**：`Field3D`、`Boid3D`（零分配值视图）、`FlockGrid3D`、`FlockKernels3D`（规则力 → 积分 → 限速 → 撞墙反弹 / 环绕边界）；
- **二维粒子**：`ParticleSystem`（对象池）、`Emitter`（方向 + 张角 + 一次性爆发或持续生成）；
- **力导向标签布局**：`ForceLayoutData`、`ForceVectorUtils`、`QuadTree`、`LabelAdjust` 等，用于二维场景的自动标号避让。

## 命名空间地图

| 命名空间 | 职责 |
|---|---|
| `Microsoft.VisualBasic.Engine.Particles`（根） | 三维 SPH 引擎、粒子状态、边界与叶轮 |
| `....Boids` | 二维 / 三维群集模拟与积分内核 |
| `....Grid` | 二维 / 三维空间哈希与均匀网格 |
| `....Particles2D` | 池化粒子系统与发射器 |
| `....layout` | 力导向节点布局与标签避让 |

## 关键类型与 API

- `FluidEngine3D` —— 三维 SPH 门面：`SetParticleCount(n)`、`Reset()`、`RunStep(dt)`、`RunSimulationStep()`，以及 `Boundary`、`Impeller`、`Backend`、`TargetDensity`、`GravityDirection`、`ViscosityStrength`、`DeltaTime`、`StepCount`；
- `SphParams3D` / `SphState3D` —— 单步参数包与 SoA 粒子状态（`ISphCompute3D` 的输入输出）；
- `ISphCompute3D` / `SphCpuCompute3D` —— 可插拔求解后端与其默认并行 CPU 实现；
- `BoxBoundary3D` / `CylinderBoundary3D` / `SphImpeller3D` —— 边界与运动边界；
- `Particle3D` / `Particle` —— 单个流体粒子的数据视图；
- `Field3D` / `FlockKernels3D` / `FlockState3D` / `Boid3D` —— 三维群集；
- `Field` / `FlockKernels` / `GridDynamics` —— 二维群集；
- `ParticleSystem` / `Emitter` / `GameParticle` —— 轻量粒子；
- 布局：`ForceLayoutData`、`ForceVector`、`ProportionalDisplacement`、`StepDisplacement`、`QuadTree`、`LabelAdjust`。

## 快速上手

三维 SPH 流体（立式罐 + 搅拌桨）：

```vbnet
Imports Microsoft.VisualBasic.Engine.Particles

Dim fluid As New FluidEngine3D()

' 罐体：半径 120、高 200 的立式圆柱
fluid.Boundary = New CylinderBoundary3D With {
    .CenterX = 0, .CenterY = 0,
    .Radius = 120, .ZBottom = 0, .ZTop = 200,
    .WallFriction = 0.15F
}

' 搅拌桨：半径 60 的圆盘涡轮，120 rpm
fluid.Impeller = New SphImpeller3D With {
    .CenterZ = 60, .Radius = 60, .BladeHeight = 30
}
fluid.Impeller.SetRpm(120)

fluid.SetParticleCount(60000)
fluid.DeltaTime = 1 / 60.0F

Do
    fluid.RunStep(fluid.DeltaTime)     ' 内部按 CFL 自动切分子步
    Console.WriteLine($"t = {fluid.Time:F2}, 子步 = {fluid.LastSubSteps}")
Loop While fluid.StepCount < 600
```

三维群集：

```vbnet
Imports Microsoft.VisualBasic.Engine.Particles.Boids

Dim field As New Field3D(width:=300, height:=200, depth:=300, boidCount:=500)

Do
    field.Advance(bounceOffWalls:=True, wrapAroundEdges:=False)

    ' 逐个体读取位置 / 朝向（Boid3D 是零分配的值视图）
    For i As Integer = 0 To field.BoidCount - 1
        Dim boid As Boid3D = field(i)
        ' boid.Position / boid.Velocity ...
    Next
Loop
```

二维粒子：

```vbnet
Imports Microsoft.VisualBasic.Engine.Particles.Particles2D

Dim system As New ParticleSystem()
system.Emit(New Emitter With {.Spread = 0.6}, 200)   ' 一次性喷发 200 个
system.Step(1 / 60.0)
```

## 实现要点

- **为什么用 SoA**：GPU 后端需要把整列状态一次性上传，AoS（`Particle()` 数组）会带来结构体内存布局与对齐问题；SoA 让 `Single()` 直接映射成设备缓冲。
- **近压力项**：SPH 的「近压力」（near pressure）用于抵抗粒子在自由表面的团聚，`NearPressureMultiplier` / `NearPressureRatio` 控制它的强度。
- **叶轮是运动边界**：`SphImpeller3D` 不参与粒子积分，而是在每个子步按角速度驱动边界附近的粒子速度（含径向泵送与混合率），因此对时间步长不敏感。
- **群集内核零分配**：`Boid3D` 是对 `FlockState3D` 某一槽位的**值视图**，它不持有数据，读写直接落到宿主数组上。

## 包信息

- Assembly：`Microsoft.VisualBasic.Engine.Particles`
- TargetFramework：`net10.0`
- Tags：`scibasic;particles;sph;fluid-simulation;boids;flocking;gpu-ready;3d-simulation;force-layout`
- 许可：GPL-3.0-or-later
