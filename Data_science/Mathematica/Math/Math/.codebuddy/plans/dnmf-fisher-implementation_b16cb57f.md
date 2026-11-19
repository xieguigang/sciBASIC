---
name: dnmf-fisher-implementation
overview: 在 Matrix.NET/Decomposition 中新增 Fisher 判别约束 DNMF 实现（对齐现有 NMF 的 API），并在 Math/test 添加基于 DebugAudit 的控制台自测与报告。
todos:
  - id: verify-matrix-and-test-contracts
    content: 使用 [subagent:code-explorer] 核对矩阵 API、NMF 签名与测试入口
    status: completed
  - id: implement-dnmf-class
    content: 使用 [skill:lsp-code-analysis] 校验类型后新增 DNMF.vb，实现 Fisher 乘性更新
    status: completed
    dependencies:
      - verify-matrix-and-test-contracts
  - id: add-dnmf-audit-and-dispatch
    content: 新增 DNMFAudit.vb 并在 ProgramMilp 增加 dnmf 分支
    status: completed
    dependencies:
      - implement-dnmf-class
  - id: build-and-run-dnmf-selftest
    content: 编译并运行 dnmf 自测，修正数值或边界问题
    status: completed
    dependencies:
      - add-dnmf-audit-and-dispatch
---

## 产品概述

在现有矩阵分解算法库中新增 DNMF 分解方法，面向带类别标签的非负数据，在保持重构能力的同时提升样本表示的判别性。

## 核心功能

- 输入非负矩阵与每个样本的类别标签，输出基矩阵、样本系数矩阵、目标值和每轮误差。
- 接口与现有 NMF 分解保持一致，支持秩、迭代次数、收敛阈值、数值稳定参数和进度显示。
- 通过 Fisher 判别约束同时压缩类内散度、放大类间散度，并保证更新过程中因子非负。
- 提供控制台自测：构造带标签的合成数据，验证维度、非负性、有限性、重构误差与判别效果，导出测试报告。

## Tech Stack Selection

- 复用现有项目：VB.NET、.NET 10、SDK 风格项目 `Math.NET5.vbproj`。
- 核心数据结构：`Microsoft.VisualBasic.Math.LinearAlgebra.Matrix.NumericMatrix`。
- 测试宿主：`Math/test/test.vbproj` 现有控制台入口 `ProgramMilp.Main(args)`。
- 报告机制：`Microsoft.VisualBasic.ApplicationServices.Debugging.DebugAudit`。

## Implementation Approach

在 `Algebra/Matrix.NET/Decomposition` 中新增 `DNMF.vb`，沿用 `NMF.vb` 的类结构、参数顺序和乘性更新写法，不改动现有 NMF 行为。判别项采用 Fisher 思路：在每轮基于当前样本系数 `W` 的类内、类间散度矩阵上构造非负分子/分母，从而得到保持非负性的乘性更新；散度经迹归一化以避免尺度退化。

关键决策：

- 判别约束施加在样本表示 `W(m×k)` 上，直接服务分类/聚类用途；`H(k×n)` 仍按标准 NMF 更新。
- 散度矩阵按元素拆分为正部与负部，使含负off-diagonal的 Fisher 散度也能安全进入乘性更新。
- 新增可选参数均放在现有 NMF 参数之后，保持源码级兼容；不修改 `NMF.vb`。
- 测试通过现有 `ProgramMilp` 的命令行分支接入，避免改变默认启动行为。

目标函数：`J = 1/2 ||A - WH||_F² + λ_w/2 tr(Wᵀ S_W W) - λ_b/2 tr(Wᵀ S_B W)`，其中 `S_W` 为类内散度，`S_B` 为类间散度，二者按迹归一化。

乘性更新（`S = S⁺ - S⁻`，`S⁻ = max(-S, 0)`）：

- `W ← W ⊙ (A Hᵀ + λ_w S_W⁻ W + λ_b S_B⁺ W + ε) ⊘ (W H Hᵀ + λ_w S_W⁺ W + λ_b S_B⁻ W + ε)`
- `H ← H ⊙ (Wᵀ A) ⊘ (Wᵀ W H + ε)`

性能：每轮主要成本为 `O(m·n·k + m·k²)`，空间 `O(m·k + k·n + k²)`。缓存 `Wᵀ`、`Hᵀ`，矩阵乘法走 `DotProduct`，逐元素运算走 `*` 与 `/`；仅在散度累计时使用索引访问。

可靠性：校验 `A` 非负且有限、`labels.Length = m`、标签分簇数至少为 2、`1 ≤ k ≤ min(m,n)`、迭代与阈值参数合法；分母统一加 `ε`；收敛采用相对变化 `|cost - prev| ≤ tolerance·(1 + |prev|)`。

## Implementation Notes

- 复用 `NMF.vb` 的文件头、命名空间 `LinearAlgebra.Matrix`、XML 文档注释与 `TqdmWrapper.Range` 进度条模式；进度条为 `Nothing` 时不调用 `SetLabel`。
- `DotProduct` 返回 `GeneralMatrix`，与 `NumericMatrix` 混用逐元素运算时按现有 `NMF.vb` 的方式处理类型转换。
- 初始化沿用 `NumericMatrix.random(rowDimension:=m, columnDimension:=k)`；新增 `randomSeed` 仅用于让自测可复现。
- 不在算法库内写日志；只在自测模块使用 `DebugAudit.Test/ArealEqual/Mark/SaveReport`，避免日志噪音与敏感信息。
- SDK 风格项目自动包含新增 `.vb` 文件，无需修改 `Math.NET5.vbproj` 或 `test.vbproj`。
-  blast radius 控制：仅新增 `DNMF.vb`、`DNMFAudit.vb`，并在 `ProgramMilp.vb` 增加一个命令分支与用法说明。

## Architecture Design

- 矩阵核心层：`NumericMatrix` 提供乘法、逐元素运算、转置、随机初始化与索引。
- 分解算法层：`NMF` 作为参照实现；新增 `DNMF` 暴露同构属性与 `Shared Factorisation` 入口。
- 判别约束层：`BuildScatter`（类内/类间散度）、`SplitScatter`（正负部拆分）、`NormalizeScatter`（迹归一化）作为 `DNMF` 私有辅助过程。
- 验证层：`DNMFAudit.RunAll()` 生成合成数据、调用 `DNMF.Factorisation`、用 `DebugAudit` 断言并导出 JSON 报告；`ProgramMilp.Main` 以 `"dnmf"` 参数分发。

## Directory Structure

```
G:\GCModeller\src\runtime\sciBASIC#\Data_science\Mathematica\Math\Math\
├── Algebra/
│   └── Matrix.NET/
│       └── Decomposition/
│           └── DNMF.vb  # [NEW] DNMF 类：W/H/cost/errors/rank 属性；Shared Factorisation 入口；Fisher 散度构建、正负部拆分、归一化、W/H 乘性更新、参数校验与收敛判断。沿用 NMF.vb 风格。
└── test/
    ├── DNMFAudit.vb  # [NEW] 基于 DebugAudit 的控制台自测：三簇非负合成数据、固定种子、维度/非负/有限/重构误差/类间类内比/最近质心准确率断言，保存 dnmf-test-report.json 并返回失败数。
    └── milp/
        └── ProgramMilp.vb  # [MODIFY] 在 Main(args) 的 Select Case 中增加 Case "dnmf" 调用 DNMFAudit.RunAll()，并补充用法注释；默认行为保持不变。
```

## Key Code Structures

```
Public Class DNMF
    Public Property W As NumericMatrix
    Public Property H As NumericMatrix
    Public Property cost As Double
    Public Property errors As Double()
    Public Property rank As Integer

    Public Shared Function Factorisation(A As NumericMatrix,
                                         labels As Integer(),
                                         Optional k As Integer = 2,
                                         Optional max_iterations As Integer = 1000,
                                         Optional tolerance As Double = 0.001,
                                         Optional epsilon As Double = 0.0001,
                                         Optional lambdaWithin As Double = 0.1,
                                         Optional lambdaBetween As Double = 0.1,
                                         Optional randomSeed As Integer? = Nothing,
                                         Optional tqdm As Boolean = True) As DNMF
    End Function
End Class
```

## Agent Extensions

### SubAgent

- **code-explorer**
- Purpose: 核对 `NumericMatrix` 的 `DotProduct`、`Transpose`、`Item`、`random`、逐元素运算符，`NMF.vb` 调用链，以及 `ProgramMilp.Main` 的分支结构。
- Expected outcome: 输出已验证的 API 清单、变量形状约定和最小改动点，避免实现阶段使用不存在的成员。

### Skill

- **lsp-code-analysis**
- Purpose: 在实现 `DNMF.vb` 时按语义解析验证类型契约、重载解析和引用点，重点检查 `GeneralMatrix` 与 `NumericMatrix` 的转换。
- Expected outcome: 确认新增类的公开签名可编译、与现有调用方无冲突，并返回需要修正的类型或引用问题。