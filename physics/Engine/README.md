# 2D 刚体物理引擎（含粒子与流体求解器）

## 引言

可视化的可信度往往来自**物理合理性**：网络的弹性展开、标签的相互避让、粒子的流动——这些效果如果只是随手写的插值，看起来总是不够「自然」。

本包提供一个**固定步长的 2D 物理世界**，包含刚体、碰撞、关节、力场、粒子与流体，并且把同一套力学方法扩展到**标签布局避让**。

## 设计目标

- **确定性**：固定步长积分，同样输入得到同样输出，便于复现与回放；
- **分层清晰**：刚体、碰撞、关节、力场、粒子各自独立；
- **跨场景复用**：物理求解器不仅服务于模拟，也服务于布局与标注。

## 核心特性

- **刚体世界**：固定时间步推进的确定性模拟，`RigidBody` 命名空间建模刚体及其属性；
- **碰撞检测**：`Collision` 命名空间实现粗检测（broad phase）与精检测（narrow phase），提供形状、重叠测试与接触求解；
- **关节约束**：`Joints` 实现转动关节（revolute）、棱柱关节（prismatic）、弹簧关节（spring）与焊接关节（weld）；
- **力场**：`ForceFields` 提供重力等力场作用；
- **粒子与流体**：`Particles2D` 实现基于 SPH 的流体求解器，`Boids` 实现群集（boids）行为，`Particles` 提供通用粒子系统；
- **标签布局**：`layout` 命名空间用同一套力驱动方法做 2D 标签避让，把重叠标签推开。

## 命名空间地图

| 命名空间 | 职责 |
|---|---|
| `Microsoft.VisualBasic.Imaging.Physics`（根） | 物理世界与共享类型 |
| `....Physics.RigidBody` | 刚体模型 |
| `....Physics.Collision` | 粗 / 精碰撞检测与接触求解 |
| `....Physics.Joints` | 各类关节约束 |
| `....Physics.ForceFields` | 力场 |
| `....Physics.Particles2D` / `.Particles` / `.Boids` | SPH 流体、粒子系统与群集 |
| `....Physics.layout` | 力驱动标签布局 |

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.Imaging.Physics

Dim world As New PhysicsWorld(fixedStep:=1.0 / 60)

Call world.AddBody(New RigidBody(mass:=1.0, shape:=Shape.Circle(radius:=10)))
Call world.AddField(New GravityField(g:=9.8))
Call world.Step(steps:=600)

For Each body In world.Bodies
    Console.WriteLine($"{body.Position.X}, {body.Position.Y}")
Next
```

## 实现要点

- **为什么必须固定步长**：可变步长的积分会让同样的初始条件在不同负载下产生不同结果；固定步长保证**可复现**，也让接触求解更稳定。
- **粗 / 精两阶段碰撞**：粗阶段用包围体快速排除大量不相交对象，精阶段才对少量候选对做精确相交测试——这是物理引擎能实时运行的关键。
- **SPH 的直觉**：流体不用「网格」表示，而是用一组带质量的粒子；每个粒子的受力由邻域粒子的密度与压强决定，从而自然涌现出流动与不可压缩性。
- **标签避让与物理同源**：把"标签"视为互相排斥的物体是最自然的避让模型，这正是 `layout` 复用力学求解的原因。

## 包信息

- Assembly：`Microsoft.VisualBasic.Imaging.Physics`
- TargetFramework：`net10.0`
- Tags：`scibasic;physics-engine;rigid-body;collision-detection;particle-system;sph;boids;joints`
- 许可：GPL-3.0-or-later
