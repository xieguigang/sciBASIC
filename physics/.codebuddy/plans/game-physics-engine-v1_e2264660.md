---
name: game-physics-engine-v1
overview: 重构现有 VB.NET 2D 物理引擎核心，并新增"刚体动力学 + 粒子系统 + 力场"三大模块，使引擎达到可嵌入游戏的程度。本轮目标（按用户选定）：以 2D 为主（架构预留 3D 扩展），采用固定步长游戏循环（PhysicsWorld.Step(dt)），半隐式欧拉积分，向量化力模型。不破坏现有 MassPoint/World 的编译（保留向后兼容，新引擎并存于同命名空间）。
todos:
  - id: core-rigidbody
    content: 扩展 Vector2Math 并实现 RigidBody、PhysicsMaterial 与 PhysicsWorld 半隐式欧拉积分
    status: completed
  - id: collision
    content: 实现碰撞系统：Broad/Narrow Phase、Manifold、ContactSolver 含 CCD
    status: completed
    dependencies:
      - core-rigidbody
  - id: joints
    content: 实现约束关节：IConstraint 与铰链/滑块/固定/弹簧关节
    status: completed
    dependencies:
      - core-rigidbody
  - id: forcefields
    content: 实现力场系统：重力场/风力场/磁力场
    status: completed
    dependencies:
      - core-rigidbody
  - id: particles
    content: 实现轻量粒子系统：对象池、Emitter 与重力/阻力
    status: completed
    dependencies:
      - core-rigidbody
  - id: demo-validate
    content: 新增演示窗体并验证 test/ 与旧代码均可编译通过
    status: completed
    dependencies:
      - collision
      - joints
      - forcefields
      - particles
---

## 用户需求

将现有 VB.NET 2D 物理引擎完善为可用于游戏的物理计算引擎。本轮经确认的范围与决策：

- 维度：以 2D 为主（架构预留 3D 扩展）
- 优先范围：**刚体动力学 + 粒子系统 + 力场**
- 架构策略：**重构核心**（引入时间步长 dt、半隐式欧拉积分、向量化力模型），但旧 `World`/`MassPoint`/`Force` 保留兼容（标注 Obsolete），避免破坏现有 `test/`
- 运行方式：**固定步长游戏循环** `Step(dt)`

## 产品概述

一套面向 2D 游戏、以刚体动力学为基石的物理引擎子系统。在现有质点/力场/SPH 雏形之上，新增刚体运动学与动力学、碰撞检测与响应、约束关节、物理材质、力场，以及轻量粒子系统，并通过固定步长循环驱动，可直接接入游戏主循环。

## 核心特性

- **刚体运动学/动力学**：线速度、角速度、加速度；基于半隐式欧拉法按 `dt` 更新位置与旋转；质量、逆质量、转动惯量、逆惯量、质心。
- **力与力场生成**：重力、浮力（阿基米德）、恒定推进力、空气阻力（线性/角阻尼）、弹簧力（胡克）、以及重力场/风力场/磁力场。
- **碰撞检测与响应**：Broad Phase（复用均匀网格/Sort and Sweep）、Narrow Phase（2D SAT 多边形、圆-多边形、圆-圆，生成法向/穿透深度/接触点）、冲量响应（恢复系数、库仑摩擦、Baumgarte 位置修正）、简化连续碰撞检测（CCD）。
- **约束与关节**：铰链（旋转）、滑块（平移）、固定、弹簧关节，统一接口在速度求解中施加冲量。
- **物理材质**：表面摩擦系数与恢复系数，碰撞对组合决定接触行为。
- **轻量粒子系统**：对象池、仅重力+阻力简化模型、Emitter 爆发生成，支持爆炸/火花/灰尘等特效，不与刚体耦合以保证大数据量性能。

## 技术栈

- 语言/框架：VB.NET，目标框架 net10.0（沿用 `physics-netcore5.vbproj`）
- 依赖（复用现有，不新增 NuGet）：`Microsoft.VisualBasic.Core`、`Math.NET5`、`graph-netcore5`、`System.Drawing.Primitives`
- 复用既有：`Vector2`（2D 向量）、`Vector2Math` 模块、`GridDynamics`（空间哈希网格，作 Broad Phase 基础）、`Particles/` 下 SPH（本轮不动，留作后续流体阶段）
- 命名空间：根 `Microsoft.VisualBasic.Imaging.Physics` 下新增子模块文件夹（`RigidBody/`、`Collision/`、`Joints/`、`ForceFields/`、`Particles2D/`）

## 实现方案

**策略**：采用"并存式重构"——保留旧 `World`/`MassPoint`/`Force`（标注 `<Obsolete>` 保留编译），新增游戏向模块，避免破坏 `test/` 现有窗体。核心以固定步长累加器驱动 `PhysicsWorld.Step(dt)`，内部按子步（substep）执行 力场施加→BroadPhase→NarrowPhase→ContactSolver/Joints→RigidBody 积分。

**关键决策**：

1. **半隐式欧拉（Symplectic Euler）**：`a = F/m; v += a·dt; x += v·dt`，旋转 `ω += (τ/I)·dt; θ += ω·dt`。相比现有"速度永不由加速度更新、无 dt"的原始积分，数值更稳定，适合实时游戏。
2. **向量化力累加器**：`RigidBody` 持有 `force`/`torque`（`Vector2`）累加器，提供 `ApplyForce(worldPoint, F)`、`ApplyImpulse(point, J)`、`ApplyTorque`。不再使用极坐标 `Force` 做实时计算（旧 `Force` 仅保留兼容）。
3. **碰撞流形与顺序冲量（Sequential Impulse）**：`Manifold` 承载法向、穿透深度、最多 2 个接触点；求解器迭代 10–20 次，叠加恢复系数 `e` 与库仑摩擦（切向冲量按 `μ` 钳制），用 Baumgarte 软约束 + slop 修正穿透，避免抖动。
4. **CCD**：通过扫掠 AABB 与子步/时间补偿处理高速物体隧道效应（2D 简化版）。
5. **关节统一接口**：`IConstraint.SolveVelocity(ctx)` 在速度迭代中施加冲量，派生铰链/滑块/固定/弹簧，弹簧用胡克+阻尼。
6. **粒子系统独立于 SPH**：新建轻量 `GameParticle`/`ParticleSystem`/`Emitter`（对象池），仅重力+线性阻力，可选对静态碰撞体做简单反弹；不与刚体耦合，以支持万级粒子。

**性能**：BroadPhase 用 `GridDynamics` 均匀网格消除 O(n²) 配对，复杂度约 O(n)；NarrowPhase 仅处理潜在配对；求解器为固定迭代次数；粒子池避免 GC 抖动。瓶颈为刚体数量极大时的配对与迭代——通过网格与子步数上限控制。

**避免技术债**：复用 `GridDynamics.EncodeGrid/SpatialLookup`、`Vector2` 运算符与 `Vector2Math`；不改动与游戏无关的 `layout/`；不在旧 API 上做破坏性修改，仅标注 Obsolete 待后续清理。

## 实现要点

- 旧 API（`World`/`MassPoint`/`Force`/`Reactions`）仅加 `<Obsolete>` 特性，逻辑与签名不变，确保 `test/` 仍可编译。
- `Vector2Math` 扩展：补充 `Cross`、`LengthSquared`、`Normalize`、`Rotate(angle)`、`Perpendicular`、`Distance` 等 2D 运算。
- 固定步长默认 `1/60`，`PhysicsWorld` 采用累加器 + 最大子步数限制，防止螺旋死亡（spiral of death）。
- 复用 `GridDynamics` 做 BroadPhase，不重复造网格；NarrowPhase 圆/多边形解析优先于通用 GJK（2D 下 SAT 更直接）。
- 接触求解中摩擦/恢复取两材质组合值（如 `sqrt(μa·μb)` 或最小值），保持确定性。
- 不引入新 NuGet 依赖，渲染由 `test/` 演示窗体通过现有 `IGraphics`/`Debugger` 完成。

## 架构设计

```mermaid
graph TD
    A[PhysicsWorld.Step(dt)] --> B[累加器 / 子步]
    B --> C[ForceField 施加力到刚体]
    B --> D[BroadPhase 均匀网格剔除]
    D --> E[NarrowPhase SAT/GJK → Manifold]
    E --> F[ContactSolver 顺序冲量 + 摩擦 + 位置修正]
    E --> G[Joints 速度约束求解]
    F --> H[RigidBody.Integrate 半隐式欧拉]
    G --> H
    H --> I[输出 / 渲染]
    J[ParticleSystem.Step] --> A
```

## 目录结构

```
physics/
├── Vector2Math.vb            # [MODIFY] 扩展 2D 工具：Cross/LengthSquared/Normalize/Rotate/Perpendicular/Distance
├── World.vb                 # [MODIFY] 标注 <Obsolete>，保留兼容不删除
├── MassPoint.vb             # [MODIFY] 标注 <Obsolete>，保留兼容
├── Force.vb                 # [MODIFY] 标注 <Obsolete>（极坐标力仅作兼容）
├── RigidBody/
│   ├── RigidBody.vb         # [NEW] 刚体：质量/逆质量、转动惯量/逆惯量、线/角速度、力与扭矩累加、ApplyForce/ApplyImpulse/ApplyTorque、Integrate(dt) 半隐式欧拉、AABB、睡眠
│   ├── PhysicsMaterial.vb   # [NEW] 物理材质：friction、restitution
│   └── PhysicsWorld.vb      # [NEW] 固定步长世界：刚体集合、Step(dt) 累加器+子步、全局重力、力场/碰撞/关节驱动、Integrate
├── Collision/
│   ├── Collider.vb          # [NEW] 碰撞体几何：Circle/Polygon/AABB，支持点、质量、惯性、最近点/支持轴
│   ├── BroadPhase.vb        # [NEW] 复用 GridDynamics 均匀网格 / Sort and Sweep 剔除
│   ├── NarrowPhase.vb       # [NEW] 2D SAT（多边形-多边形、多边形-圆、圆-圆），生成 Manifold
│   ├── Manifold.vb          # [NEW] 接触流形：法向、穿透深度、接触点（≤2）
│   └── ContactSolver.vb     # [NEW] 顺序冲量求解：恢复系数、库仑摩擦、Baumgarte 位置修正、简化 CCD
├── Joints/
│   ├── IConstraint.vb       # [NEW] 约束接口：SolveVelocity(ctx)，统一冲量求解
│   ├── RevoluteJoint.vb     # [NEW] 铰链/球窝：单轴旋转
│   ├── PrismaticJoint.vb    # [NEW] 滑块：单轴平移
│   ├── WeldJoint.vb         # [NEW] 固定：锁定全部自由度
│   └── SpringJoint.vb       # [NEW] 弹簧：胡克+阻尼弹性连接
├── ForceFields/
│   ├── ForceField.vb        # [NEW] 力场基类：区域/全局、Apply(bodies)
│   ├── GravityField.vb      # [NEW] 重力场
│   ├── WindField.vb         # [NEW] 风力场（方向+强度）
│   └── MagneticField.vb     # [NEW] 磁力场
├── Particles2D/
│   ├── GameParticle.vb      # [NEW] 轻量粒子：位置/速度/寿命/尺寸/颜色
│   ├── ParticleSystem.vb    # [NEW] 粒子系统：对象池、仅重力+线性阻力、Step
│   └── Emitter.vb           # [NEW] 发射器：爆发/持续生成
└── test/
    └── DemoRigidBody.vb     # [NEW] 演示窗体：落地反弹、堆叠、关节链、力场影响、粒子爆发；现有 Form1-5/Module1 保持编译
```

## 关键代码结构

```
' RigidBody 核心成员（示意接口级）
Public Class RigidBody
    Public Position As Vector2
    Public Rotation As Double          ' 2D 标量角（3D 可升级为四元数）
    Public Velocity As Vector2
    Public AngularVelocity As Double
    Public Mass As Double, InvMass As Double
    Public Inertia As Double, InvInertia As Double
    Public Material As PhysicsMaterial
    Public Collider As Collider
    Private force As Vector2, torque As Double
    Sub ApplyForce(worldPoint As Vector2, f As Vector2)
    Sub ApplyImpulse(worldPoint As Vector2, j As Vector2)
    Sub Integrate(dt As Double)        ' 半隐式欧拉：v+=a*dt; x+=v*dt
End Class

' 约束统一接口
Public Interface IConstraint
    Sub SolveVelocity(ctx As SolverContext)
    Sub SolvePosition(ctx As SolverContext)
End Interface

' 接触流形
Public Class Manifold
    Public Normal As Vector2           ' 由 A 指向 B
    Public Penetration As Double
    Public Contacts As Vector2()       ' 最多 2 个接触点
    Public Restitution As Double
    Public Friction As Double
End Class
```