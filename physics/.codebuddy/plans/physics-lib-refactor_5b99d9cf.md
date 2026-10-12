---
name: physics-lib-refactor
overview: 对 gr/physics 物理模拟库进行系统性重构：1) 补全 Math 模块中两个 NotImplemented 的待实现函数；2) 全量 PascalCase 规范化（含重命名遮蔽 System.Math 的 Math 模块、Force.void()、Gephi 遗留的 fcBi*/fcUni* 缩写、predYval 拼写错误、ExternalForces 重载歧义等）；3) 合并 ForceVectorUtils 的复制粘贴函数与 FluidEngine 的薄包装/死代码；4) 按职责拆分 LabelAdjust.vb、FluidEngine.vb 等大文件。仅重构物理库，不动 test/ 演示项目（重命名公开 API 后 test 编译报错由用户后续自行同步）。
todos:
  - id: refactor-math-module
    content: 重命名 Math 模块为 ForceMath，实现 Decomposition3D 与 Friction，成员 PascalCase 化并同步 Force/Reactions/MassPoint 调用
    status: completed
  - id: pascalcase-core-types
    content: 对 Force/Vector2/Vector2Math/MassPoint/World/GridDynamics/Debugger/Reactions 做 PascalCase 化并修复全部引用
    status: completed
    dependencies:
      - refactor-math-module
  - id: rename-boids
    content: 将 Boid 自定义成员规范化，并把 Field.vb 的 predYval 拼写修正为 PredYvel 及字段 PascalCase 化
    status: completed
  - id: split-fluid-engine
    content: 拆分 FluidEngine 内核到 FluidKernels.vb，拆分 ExternalForces 重载，内联薄包装并删除死代码，Particle 成员 PascalCase 化
    status: completed
  - id: consolidate-forcevector-utils
    content: 合并 ForceVectorUtils 的 8 个 fc* 函数为语义化方法并修正碰撞逻辑，PascalCase 化 layout 内 ForceVector/ForceVectorNodeLayoutData/ForceLayoutData 等类型及引用
    status: completed
  - id: split-label-adjust
    content: 将 LabelAdjust.vb 拆分为 Node/TextProperties/QuadTree 独立文件，内部字段 PascalCase 化并同步 LabelAdjustLayoutData 引用
    status: completed
  - id: verify-build
    content: 用 [subagent:code-explorer] 校验重命名级联，编译 physics-netcore5，标注 ParallelogramLaw 的 Sinh 风险
    status: completed
    dependencies:
      - refactor-math-module
      - pascalcase-core-types
      - rename-boids
      - split-fluid-engine
      - consolidate-forcevector-utils
      - split-label-adjust
---

## 用户需求

对 `gr/physics` 物理模拟库（physics-netcore5 项目）进行整体重构，覆盖四类目标：对行数过多的文件按职责拆分、将项目内自定义公开成员全面 PascalCase 化、合并复制粘贴式的冗余代码并删除死代码、补全尚未实现的函数。test/ 演示项目不在范围内，重命名公开 API 后其编译报错由用户自行同步。

## 产品概述

本次为纯后端 VB.NET 库的代码质量重构，不涉及任何 UI 或运行时行为的新增。目标是提升代码可读性、可维护性与架构清晰度，同时保持现有物理算法行为不变（除明确修正的拼写/逻辑错误外）。

## 核心特性

- 拆分 `LabelAdjust.vb`（424 行，含 Node/TextProperties/LabelAdjust/QuadNode/QuadTree 4 类）与 `FluidEngine.vb`（403 行，核函数与主循环分离）为按职责组织的独立文件
- 将本项目自定义的全部公开类型、属性、方法、枚举、字段统一为 PascalCase（继承自外部库如 `Vector2D.x/y`、`Layout2D.X/Y` 的不改）
- 合并 `ForceVectorUtils.vb` 中 8 个 `fc*` 复制粘贴函数与 `FluidEngine.vb` 中 5 个核函数薄包装，删除 `HandleCollisions` 内整段注释死代码
- 实现 `Math.Decomposition3D` 与 `Math.Friction` 两个 `NotImplementedException` 占位函数
- 修正 `Boids/Field.vb` 中 `predYval` 拼写错误（应为 `predYvel`）；将遮蔽 `System.Math` 的 `Math` 模块重命名为 `ForceMath`

## 技术栈

- 语言/运行时：VB.NET，目标框架 netcore5 / net10.0（由 `physics-netcore5.vbproj` 定义）
- 项目结构：根命名空间物理库，含 `Boids`、`layout`、`Particles` 三个子命名空间
- 不引入任何新框架、第三方库或构建工具；保持现有 GPL3 版权头与 `#Region` 代码统计注释块不变

## 实现方案

采用"语义化重命名 + 职责拆分 + 冗余合并 + 占位补全"的组合策略。先处理底层无依赖的 `Math` 模块与核心类型，再向上游（Boids、Particles、layout）推进，最后统一编译校验。重命名使用全项目范围的符号替换，但必须区分"项目定义成员"与"外部继承成员"（`Vector2D.x/y`、`Layout2D.X/Y`、`System.*` 类型不动）。

关键决策：

1. **`Math` 模块 → `ForceMath`**：当前 `Public Module Math` 与 `System.Math` 同名，文件内被迫用 `std = System.Math` 别名补偿；重命名后消除遮蔽，显式 `Math.` 调用点（Force.vb、Reactions.vb、Math.vb 内部）改为 `ForceMath.`，扩展方法（`Decomposition2D/Decomposition3D`、`RepulsiveForce/AttractiveForce`）仍可被编译器跨命名空间发现，调用处无需改。
2. **拆分而非删类**：`LabelAdjust.vb` 与 `FluidEngine.vb` 保持每个类单文件，符合 SoC；被拆分出的 `QuadNode`/`QuadTree`、`FluidKernels` 仍置于原命名空间。
3. **冗余合并**：`ForceVectorUtils` 的 8 个 `fc*` 子过程样板一致（xDist/yDist/dist/LayoutData），合并为少量带参数（双向/单向、是否碰撞感知、是否竖向化）的语义方法；合并时补齐 `fcBiAttractor_noCollide` 缺失的碰撞分支，并修正 `fcBiRepulsor_noCollide` 中 `ElseIf dist <> 0` 应为 `ElseIf dist <= 0` 的笔误。`FluidEngine` 的 `DensityKernel/NearDensityKernel/...` 薄包装直接内联到 `FluidKernels` 对应核函数。
4. **占位补全**：`Decomposition3D` 在 `Force` 仅含平面角的前提下，返回 xy 平面分量扩展为 3D 向量（z=0）；`Friction` 文档描述为"与合力方向相反"，补全为接收合力并返回其反向力 `Friction(f As Force) As Force = -f`（库内无调用方，签名调整安全）。

## 实现注意事项

- **保留头信息**：所有 `.vb` 顶部 GPL3 许可头与自动生成的 `#Region "Microsoft.VisualBasic::..."` 统计块原样保留。
- **不触碰继承成员**：`Boid.x/y`（来自 `Vector2D`）、`Node/Particle/Boid` 实现的 `Layout2D.X/Y`、`Vector2` 的 `x/y` 字段均不重命名。
- **级联范围**：`ForceVector.x/y`、`ForceVectorNodeLayoutData.dx/dy/old_dx/old_dy/freeze`、`ForceLayoutData.energy0/[step]/progress`、`AbstractForce.calculateForce`、`Displacement.moveNode` 等 layout 内部成员 PascalCase 化后，必须同步 `ProportionalDisplacement`、`StepDisplacement`、`ForceVectorUtils`、`LabelAdjust` 等调用点。
- **test/ 不在范围**：重命名后整个 `.sln` 中 test 项目会编译失败属预期，不在本计划中处理。
- **风险标注（不自动修改）**：`Math.ParallelogramLaw` 第 128 行使用 `Sinh(sina)`（双曲正弦）疑似应为 `Asin(sina)`，属潜在正确性 bug，仅在计划末尾标注，交人工确认，不在自动重构中硬改。

## 架构设计

保持现有分层与命名空间结构不变（根 / Boids / layout / Particles），仅做文件级与符号级整理，不引入新的抽象层或设计模式。模块依赖方向维持原状：`ForceMath` 被 `Force`/`Reactions`/`MassPoint` 依赖；`GridDynamics`/`IContainer` 被 `Boids.Field` 与 `Particles.FluidEngine` 依赖；`layout` 内部 `ForceVectorUtils/ForceVector/ForceVectorNodeLayoutData` 互相依赖并由 `AbstractForce`/`LabelAdjust` 使用。

## 目录结构

```
physics/
├── Math.vb                          # [MODIFY] 模块重命名为 ForceMath；实现 Decomposition3D/Friction；成员 PascalCase（X/Y/Z 常量、Decomposition2D 等）；修复内部 std/System.Math 引用
├── Force.vb                         # [MODIFY] strength→Strength、angle→Angle、source→Source；void()→Reset()；操作符与 ToString 同步
├── Vector2.vb                       # [MODIFY] down/up/left/right/zero/one/magnitude→Down/Up/Left/Right/Zero/One/Magnitude；random→Random（x/y 继承自 Vector2D 不改）
├── Vector2Math.vb                   # [MODIFY] Abs/Dot/saturate 保持 PascalCase（已是）；确认引用
├── MassPoint.vb                     # [MODIFY] 同步 ForceMath 与 PascalCase 引用（World/Point/Charge 等属性已是 PascalCase）
├── Reactions.vb                     # [MODIFY] Math.CoulombsLaw→ForceMath.CoulombsLaw；同步引用
├── World.vb                         # [MODIFY] objects/Reactions/ForceSystem/Outputs 受保护成员 PascalCase 化（Objects/Reactions/ForceSystem/Outputs）
├── GridDynamics.vb                  # [MODIFY] 方法名已是 PascalCase，仅同步内部引用（如有）
├── Debugger.vb                      # [MODIFY] 同步 ForceMath 扩展方法引用（F.Sum、Decomposition2D）
├── Boids/
│   ├── Boid.vb                      # [MODIFY] Xvel/Yvel 已 PascalCase；统一 Boid 自定义成员风格
│   └── Field.vb                     # [MODIFY] 修正 predYval→PredYvel 拼写错误；PhysicTask.Solve 内变量同步；自定义字段 PascalCase 化
├── Particles/
│   ├── Particle.vb                  # [MODIFY] position/velocity/index/predictedPosition/density→Position/Velocity/Index/PredictedPosition/Density
│   ├── FluidEngine.vb               # [MODIFY] 拆分核函数到 FluidKernels.vb；ExternalForces 函数/子过程重载拆为 ComputeExternalForce/ApplyExternalForces；内联薄包装、删除 HandleCollisions 死代码；字段 PascalCase 化
│   └── FluidKernels.vb              # [NEW] 从 FluidEngine 抽出的核函数（Poly6/Spiky Pow2/Pow3 及其导数、Density/NearDensity 计算、压力换算），含缩放因子与邻居搜索辅助
└── layout/
    ├── ForceVectorUtils.vb          # [MODIFY] 合并 8 个 fc* 函数为语义化方法（如 ApplyBidirectionalRepulsion/Attraction、ApplyUnidirectionalRepulsion/Attraction、ApplyVerticalizedRepulsion、ApplyFlatAttraction 及 *NoCollide 变体）；distance→Distance；补齐碰撞分支、修正 dist<=0 笔误
    ├── ForceVector.vb               # [MODIFY] x/y→X/Y；add/multiply/subtract/normalize/z/Energy/Norm PascalCase 化
    ├── ForceVectorNodeLayoutData.vb # [MODIFY] dx/dy/old_dx/old_dy/freeze→Dx/Dy/OldDx/OldDy/Freeze
    ├── ForceLayoutData.vb           # [MODIFY] energy0/[step]/progress→Energy0/Step/Progress
    ├── AbstractForce.vb             # [MODIFY] calculateForce→CalculateForce；ForceVectorUtils.distance→Distance
    ├── Displacement.vb              # [MODIFY] moveNode→MoveNode（接口）
    ├── ProportionalDisplacement.vb  # [MODIFY] 同步 ForceVector.X/Y、Displacement.MoveNode
    ├── StepDisplacement.vb          # [MODIFY] 同步 ForceVector.X/Y、Displacement.MoveNode、normalize→Normalize
    ├── LabelAdjust.vb               # [MODIFY] 仅保留 LabelAdjust 类主体（goAlgo/repulse/Solve/resetPropertiesValues）；内部字段 RadiusScale/Xmin/Xmax/Ymin/Ymax/Converged/MaxIterations/Canvas/Speed/AdjustBySize PascalCase 化；dx/dy/freeze 引用同步
    ├── Node.vb                      # [NEW] 从 LabelAdjust.vb 抽出的 layout.Node 类（实现 Layout2D）
    ├── TextProperties.vb            # [NEW] 从 LabelAdjust.vb 抽出的 TextProperties 类
    ├── QuadTree.vb                  # [NEW] 从 LabelAdjust.vb 抽出的 QuadNode 与 QuadTree 类
    └── LabelAdjustLayoutData.vb     # [MODIFY] 同步 ForceVectorNodeLayoutData.Freeze 引用
```

## 关键代码结构

```
' Math.vb（重命名 ForceMath 后）补全的两个占位函数
Public Function Decomposition3D(f As Force) As Vector
    Dim v = f.Strength, a = f.Angle
    ' Force 仅编码平面角，3D 向量在 xy 平面投影、z 分量为 0
    Return New Vector({v * std.Cos(a), v * std.Sin(a), 0})
End Function

Public Function Friction(f As Force) As Force
    ' 与合力方向相反的摩檫力
    Return -f
End Function

' FluidEngine.vb 拆分后的外部力接口（消除同名重载歧义）
Private Function ComputeExternalForce(pos As Vector2, velocity As Vector2) As Vector2
Private Sub ApplyExternalForces(id As Integer)
```

## Agent Extensions

### SubAgent

- **code-explorer**
- Purpose: 在全库 21 个 .vb 文件中检索所有被重命名符号（ForceMath、Strength/Angle、PredYvel、dx/dy/freeze、fc* 等）的剩余引用点，确保 PascalCase 级联无遗漏
- Expected outcome: 输出完整的重命名调用点清单，使 physics-netcore5 项目在仅物理库范围内编译通过（test 项目预期失败由用户处理）