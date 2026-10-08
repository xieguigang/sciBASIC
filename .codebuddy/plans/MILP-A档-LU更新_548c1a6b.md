---
name: MILP-A档-LU更新
overview: 为 MILP 的 BoundedSimplex 与既有 SimplexSolver 引入共享的 Forrest–Tomlin LU 更新因子（含周期性重构与稳定性闸门），取代当前「每次单纯形迭代完整重构 LU」的 O(m³) 热点，并以 T1–T13 + lpp 回归 + 新增 benchmark 验收。
todos:
  - id: lu-updater-core
    content: 用 [skill:lsp-code-analysis] 确认全部 LU 调用点后，新增共享 LuUpdater 组件（LP/IPMCrossover/LuUpdate.vb）
    status: completed
  - id: lu-boundedsimplex
    content: 改造 BoundedSimplex：Refresh 拆分、三处换基触发更新、SetBounds/LoadWarmStart 处理
    status: completed
    dependencies:
      - lu-updater-core
  - id: lu-gomorycut
    content: GomoryCut 改用 simplex.SolveBasisT，保持并行只读线程安全语义
    status: completed
    dependencies:
      - lu-boundedsimplex
  - id: lu-simplex-solver
    content: 用 [subagent:code-explorer] 定位换基点后，SimplexSolver 的 LoopPhase1/LoopPhase2 接入 LuUpdater
    status: completed
    dependencies:
      - lu-updater-core
  - id: lu-options-bench
    content: MilpOptions 新增开关与阈值（同步 Clone），新增 MilpLuBenchmark 与 ProgramMilp 子命令
    status: completed
    dependencies:
      - lu-boundedsimplex
  - id: lu-selftest
    content: 新增 MilpLuSelfTest：T14 LuUpdater 残差对拍（≤1e-9）、T15 端到端开关一致性
    status: completed
    dependencies:
      - lu-boundedsimplex
      - lu-gomorycut
  - id: lu-regress-doc
    content: 全量回归（selftest/demo/lpp/lpp-selftest）与 README 文档同步，输出加速比报告
    status: completed
    dependencies:
      - lu-simplex-solver
      - lu-selftest
      - lu-options-bench
---

## 产品概述

对 sciBASIC# 的 MILP 求解引擎（`Algebra/MILP`）与既有 `SimplexSolver`（`Algebra/LP/IPMCrossover/Simplex.vb`）实施 **A 档第 ① 期优化**：消除单纯形「每次迭代完整重构 LU」的 O(m³) 热点，改为产品形式（η 向量 / Sherman–Morrison 秩 1 修正）增量更新 + 周期性重构 + 数值稳定性闸门。

该项源自对「MILP 能否用 ILCuda 做 GPU 加速」的审查结论：GPU 化在此算法结构下为负收益，真正的瓶颈是算法层的重复分解，应先修掉。

## 核心功能

1. **共享 LU 更新组件**：在 `IPMCrossover` 下新增可复用的基矩阵增量分解器，`BoundedSimplex` 与 `SimplexSolver` 共用。
2. **MILP 节点 LP 加速**：`BoundedSimplex.Refresh()` 从「每迭代 O(m³) 全量重构」改为「换基时 O(m²) 增量更新」，翻界不触发更新。
3. **数值稳定性闸门**：内置 η 增长、主元分母、更新次数、残差抽查四类监测，任一超阈值立即回落完整重构，保证最坏情况不劣于现状。
4. **热启动开销削减**：消除 `LoadWarmStart` 中为探测奇异性而做却丢弃的一次 O(m³) 分解。
5. **可观测性**：新增求解统计（重构次数 / 更新次数 / 闸门触发次数）与 A/B 开关，支撑 benchmark 与验收。
6. **回归与文档**：T1–T13 对拍用例 + 30 件背包（304.3）+ `lpp` / `lpp-selftest` 全绿，并同步 `README.md`。

## 验收标准（用户已确认）

- T1–T13 全部对拍用例的**目标值 / 最优解 / 状态与现状一致，容差 1e-9 内**。
- 内置稳定性监测，更新因子误差超阈值时**自动回退完整重构**，最坏情况不劣于现状。
- 新增 `LuUpdater` 单元对拍：随机矩阵 + 随机换基序列下，`Solve` / `SolveT` 与「每次全量重构后 `LinAlg.LuSolve` / `LuSolveT`」的残差 ≤ 1e-9。
- benchmark 输出 m/n、迭代数、重构次数、更新次数、闸门触发次数、耗时，并给出开关前后加速比。

## 明确不在本次范围

- 不做 GPU / ILCuda 相关工作。
- 不做稀疏化（CSR/CSC + Markowitz 主元 + 稀疏 η），留待 ① 验收后再议。
- 不改节点级并行、割平面策略、启发式。
- 不改 `Crossover.vb`（其 `LuSolve` 均为一次性分解，非迭代热点）。

## 技术栈

- 语言/平台：VB.NET，`net10.0`，`Math.NET5.vbproj`（`AssemblyName=Microsoft.VisualBasic.Math.Core`，`Platforms=AnyCPU;x64;x86`）。
- 现有可复用设施：`IPMCrossover.LuFactorization`（行主序 `Double(,)` + `Piv` 置换向量）、`LinAlg.LuFactor` / `LuSolve` / `LuSolveT`、`MilpKernels.LuFactorRows`（SIMD jagged 版）、`MilpKernels.AxpyInPlace` / `AxpyRange` / `MatVecRows`。
- 不引入任何新依赖（保持「零第三方依赖」承诺）。

## 实现方案

### 核心策略：产品形式更新（Product-Form Update, η 向量）而非原地 U 更新

换基（离开位置 `p`、进入列 `a_q`）时基矩阵变化为秩 1：

```
B' = B + (a_q − B·e_p)·e_pᵀ = B·(I + w·e_pᵀ)，  w = B⁻¹a_q − e_p
B'⁻¹ = (I − w·e_pᵀ/(1 + w_p))·B⁻¹
```

于是求解只需在**基 LU 的一次三角回代**之上叠加 O(m) 修正：

- 正解 `B'x = b`：`z = LuSolve(base, b)`；`x = z − w·(z_p / (1 + w_p))`
- 转置解 `B'ᵀx = b`：`B'⁻ᵀb = B⁻ᵀ( b − w·(b_p / (1 + w_p)) )`

**为什么选 PFU 而不是 Forrest–Tomlin 原地 U 更新**：PFU 能 100% 复用现有 `LinAlg.LuSolve` / `LuSolveT`（两者均为纯函数、不改入参、可返回 `Nothing`），语义零改动，正确性极易对拍；代价是求解随 η 条数 k 线性增长 O(m² + k·m)，由周期性重构封顶。FT 原地 U 更新（O(m²) 更新 + 恒定 O(m²) 求解）性能上限更高但需处理 spike 与重排序，归入第 ② 期（稀疏 η + Markowitz）一并做。

### 复杂度

| 项 | 现状（每迭代） | 优化后（每迭代，摊还） |
| --- | --- | --- |
| 基分解 | O(m³) | O(m²)（一次 `LuSolve` 求 w）+ O(m³)/K（每 K 次重构摊还） |
| 三角求解 | O(m²) ×2 | O(m² + k·m) |
| matvec | O(m·n) ×2 | 不变（重构后将成为新瓶颈，见「后续项」） |


m=500 时 LU 工作量约降两个数量级。

### 关键设计决策与权衡

1. **`Refresh()` 拆成两段**：`EnsureFactorization()`（按需重构/更新）+ 每次必算的 `xB / y / d`。分解只依赖 `basis` 与 `A`，**翻界（bound flip）不换基 → 不触发更新**；只有 L809 / L955 / L675 三处 `basis(...) = ...` 才触发。
2. **防御式正确性兜底**：维护 `_factorBasis`（上次重构时的基快照），`EnsureFactorization` 里做 O(m) 差异比对——只要差异位置数与已应用 η 条数不符（或割平面追加导致行数变化），立即强制完整重构。这样即便漏掉某个换基点，结果也只是「退化成现状」而非「静默算错」。
3. **线程安全硬规则**：`LuUpdater.Solve` / `SolveT` **必须是纯读**（只读 base 与 η 列表）；只有 `ApplyUpdate` / `Refactor` 写入。这是 `GomoryCut.Generate` 对候选行并行调用 `LuSolveT` 的前提（原注释明确依赖「只读 fac，线程安全」）。
4. **`GomoryCut` 改用 updater**：`Factorization` 属性不足以表达 η 修正，故 `GomoryCut.vb:124/210` 改为调用 `simplex.SolveBasisT(e)`。`BoundedSimplex.Factorization` 保留为公开 API，但 getter 内若有待应用 η 则先完整重构，保证对外返回的 `LuFactorization` 恒为当前基的精确分解（向后兼容，且仅被旧调用路径触发）。
5. **A/B 开关**：`MilpOptions.EnableLuUpdate`（默认 `True`）。关闭时 `EnsureFactorization` 恒走完整 `LuFactorRows`，行为与现状一致 —— 这既是 benchmark 对照组，也是一键回退开关。

## 执行要点（防回归）

- **`SetBounds`（BoundedSimplex.vb:242-246）必须失效 LU 状态**：`ThreadLocal` worker 复用同一实例跨节点求解，界数组被替换且热启动基来自别处，`_lu` 与 `_factorBasis` 都要清空。
- **`LoadWarmStart`（L444-486）**：L483 的 `LuFactorRows` 结果当前被丢弃——改为直接用来**播种** `LuUpdater`，一举消除这次 O(m³) 浪费。
- **必须完整重构的场景**：`ApplyRootCuts`（`BranchAndBound.vb:355` 因 A 行列变化而 `New BoundedSimplex`）、冷启动（`basis(k) = -1-k` 全人工列）、热启动基载入、以及闸门触发。
- **新增 `MilpOptions` 项必须同步进 `Clone()`（MilpOptions.vb:196-221）**，否则并行/嵌套求解会丢配置。
- **`Simplex.vb` 接入点**：`LoopPhase2`（L298-341）的 L300-306 移出循环，首次装配 `Bm` 并 `LuFactor`，换基点 L329 `basis(leave) = enter` 后应用更新；`LoopPhase1`（L248-266）同构，一并处理。注意该处 `basis` 是 `List(Of Int32)` 且用 `keepRows` 取存活行。
- **不改 `Crossover.vb`**：其 `LuSolve`（L193/194/233/234/266）均为一次性分解，无迭代热点，改动收益为零而风险不为零。
- **日志/可观测**：统计计数（重构/更新/闸门）挂在 `BoundedSimplex` 上并经 `MilpSolution` 输出，不写高频日志，避免污染 `Verbose` 输出与并行热路径。

## 架构设计

```mermaid
flowchart TD
    subgraph 新增共享层
        LU["LuUpdater<br/>(LP/IPMCrossover/LuUpdate.vb)<br/>base LuFactorization + η 列表"]
        G1["闸门: |1+w_p| 下限 / ‖w‖∞ 上限<br/>/ η 条数上限 / 残差抽查"]
        LU --- G1
    end

    subgraph MILP
        BS["BoundedSimplex"]
        RP["RunPrimal L809 basis(leave)=enter"]
        RD["RunDual L955 basis(leave)=cd.j"]
        P1["Phase1 L675 basis(k)=bestJ"]
        RF["Refresh L524<br/>拆为 EnsureFactorization + 重算 xB/y/d"]
        GC["GomoryCut L210<br/>并行 LuSolveT"]
    end

    subgraph 既有LP
        SX["SimplexSolver<br/>LoopPhase1 L248 / LoopPhase2 L298"]
    end

    BS --> RF --> LU
    RP -->|ApplyUpdate O(m²)| LU
    RD -->|ApplyUpdate| LU
    P1 -->|ApplyUpdate| LU
    LU -->|Solve / SolveT 纯读| RF
    LU -->|SolveT 纯读，可并发| GC
    SX -->|ApplyUpdate / Solve| LU
    G1 -->|超阈值 → NeedsRefactor| RF
    RF -->|强制完整 LuFactorRows| LU
```

**数据流**：换基点 → `LuUpdater.ApplyUpdate(离开位置, 进入列)`（一次 `Solve` 求 w，O(m²)）→ 后续 `Refresh` 用 `Solve` / `SolveT` 走「基 LU + η 叠加」→ 闸门触发或每 K 次 → `Refactor` 完整重构并清空 η。

## 目录结构

```
Data_science/Mathematica/Math/Math/Algebra/LP/IPMCrossover/
├── LuUpdate.vb              # [NEW] 共享 LU 更新组件（LuUpdater）。职责：持有一次完整 LuFactorization
│                            #   与 η（秩 1 修正）序列；ApplyUpdate(leavePos, enteringCol) 追加修正；
│                            #   Solve / SolveT 与 LinAlg.LuSolve / LuSolveT 语义完全一致（纯函数、不改
│                            #   入参、可返回 Nothing、可并发读）；Refactor(matrix) 完整重构并清空 η；
│                            #   稳定性闸门（|1+w_p| ≥ 1e-12、‖w‖∞ ≤ 1e6、η 条数 ≤ 60）与统计计数。
│                            #   命名空间 LinearAlgebra.LinearProgramming.IPMCrossover，不新增依赖。
├── Simplex.vb               # [MODIFY] LoopPhase2(L298-341) 与 LoopPhase1(L248-266)：把 Bm 装配与
│                            #   LinAlg.LuFactor 移出迭代循环，改为基 LU + ApplyUpdate；换基点 L329
│                            #   应用更新；闸门触发或基结构变化时回落完整 LuFactor。
└── LinAlg.vb                # [NOT MODIFY] LuFactorization / LuFactor / LuSolve / LuSolveT 保持原样，
                             #   仅作为被复用对象（L77-90、L179、L218、L244）。

Data_science/Mathematica/Math/Math/Algebra/MILP/
├── BoundedSimplex.vb        # [MODIFY] 核心改造。新增 _lu / _factorBasis / _luDirty 字段；
│                            #   Refresh(L524) 拆为 EnsureFactorization + 重算 xB/y/d；
│                            #   三处换基（L809/L955/L675）后 ApplyUpdate；
│                            #   LoadWarmStart(L444) 用 L483 的分解播种 updater 并消除浪费；
│                            #   SetBounds(L242) 失效 LU 状态；SolveWithBasis(L573) / L651 / L891
│                            #   改走 updater；Factorization(L354) 保留并按需强制重构保证精确。
├── GomoryCut.vb             # [MODIFY] L124/L210 由 simplex.Factorization + LinAlg.LuSolveT 改为
│                            #   simplex.SolveBasisT(e)；保持并行候选行只读语义不变。
├── MilpOptions.vb           # [MODIFY] 新增 EnableLuUpdate / LuMaxUpdates / LuPivotTolerance /
│                            #   LuMaxEtaNorm，并全部同步进 Clone()(L196-221)。
├── MilpSolution.vb          # [MODIFY] 新增 LuRefactors / LuUpdates / LuGateRejects 统计字段与
│                            #   ToString 展示。
├── MilpSolver.vb            # [MODIFY] L99-100 附近把新选项传给内核（沿用既有 EnableParallel 写法）。
├── BranchAndBound.vb        # [MODIFY] L199/L355 完整重构场景保持不变；把 LU 统计汇总进 Finish(L783)。
└── README.md                # [MODIFY] L115「未做 LU 更新」改为已实现；L139-142 文件清单补 LuUpdate.vb
                             #   与新增测试文件；顺手订正 L141「T1–T11」为 T1–T13(+T14/T15)。

Data_science/Mathematica/Math/Math/test/milp/
├── MilpLuSelfTest.vb        # [NEW] T14：LuUpdater 对拍（随机方阵 + 随机换基序列，Solve/SolveT 与
│                            #   全量重构后 LinAlg 结果的残差 ≤1e-9）；T15：端到端 EnableLuUpdate
│                            #   开关前后目标值/解一致（容差 1e-9）。沿用 Check(cond,name,detail) 原语。
├── MilpLuBenchmark.vb       # [NEW] 照抄 SimdBenchmark.vb(L88/L170/L194/L227-228) 的 RunAll /
│                            #   BenchApi / BenchVersus 约定，Stopwatch 多轮 + sink 防优化；
│                            #   输出 m/n、迭代数、重构/更新/闸门次数、耗时与 A/B 加速比。
└── ProgramMilp.vb           # [MODIFY] L82 Select Case 增加 "lu-selftest" / "milp-bench" 子命令，
                             #   并同步更新 L58-71 用法说明块注释。
```

## 关键代码结构

```
Namespace LinearAlgebra.LinearProgramming.IPMCrossover

    ''' <summary>
    ''' 基矩阵的产品形式（η）更新器：一次完整 LU 分解 + 若干 Sherman–Morrison 秩 1 修正。
    ''' Solve / SolveT 为纯读操作，可并发；只有 ApplyUpdate / Refactor 会改变内部状态。
    ''' </summary>
    Public NotInheritable Class LuUpdater

        ' ---- 生命周期 ----
        Public Sub New(base As LuFactorization)
        ''' <summary>完整重构（jagged 行就地消费），清空 η 序列；奇异返回 False</summary>
        Public Function Refactor(rows As Double()()) As Boolean

        ' ---- 唯一的写入路径 ----
        ''' <summary>换基：离开位置 leavePos，进入列 enteringCol；O(m²)。稳定性闸门拒绝时返回 False</summary>
        Public Function ApplyUpdate(leavePos As Integer, enteringCol As Double()) As Boolean

        ' ---- 求解（纯读，与 LinAlg.LuSolve / LuSolveT 语义一致）----
        Public Function Solve(rhs As Double()) As Double()
        Public Function SolveT(rhs As Double()) As Double()

        ' ---- 稳定性闸门与统计 ----
        Public ReadOnly Property NeedsRefactor As Boolean
        Public Property MaxUpdates As Integer        ' η 条数上限，默认 60
        Public Property PivotTolerance As Double     ' |1+w_p| 下限，默认 1e-12
        Public Property MaxEtaNorm As Double         ' ‖w‖∞ 上限，默认 1e6
        Public ReadOnly Property RefactorCount As Integer
        Public ReadOnly Property UpdateCount As Integer
        Public ReadOnly Property GateRejectCount As Integer
        Public ReadOnly Property PendingUpdates As Integer
    End Class
End Namespace
```

`BoundedSimplex` 侧新增的最小接口（供 `GomoryCut` 使用）：

```
''' <summary>B⁻ᵀ·rhs（纯读，可并发；等价于原 LinAlg.LuSolveT(fac, rhs)）</summary>
Friend Function SolveBasisT(rhs As Double()) As Double()
```

## 风险与回退

| 风险 | 缓解 |
| --- | --- |
| 漏掉某个换基点导致静默算错 | `_factorBasis` O(m) 差异比对兜底，不符即强制完整重构（退化为现状而非出错）；T14 残差对拍覆盖 |
| PFU 数值漂移累积 | 四重闸门 + 周期性强制重构 + 残差抽查；`EnableLuUpdate=False` 一键回退到与现状完全相同的代码路径 |
| `GomoryCut` 并行读破坏线程安全 | `Solve`/`SolveT` 强制纯读；η 列表只在单线程的换基点追加 |
| 改动 `Simplex.vb` 影响既有 LP | 补跑 `lpp` 与 `lpp-selftest`；`Crossover.vb` 不在改动范围 |
| `MilpOptions` 新增项未同步 `Clone()` | 明确列为验收检查项 |


## 后续项（本次不做，验收后再议）

- **增量 `xB` / `rhs` / `d`**：LU 修好后 `MatVecRows` 的 O(m·n) 将成为新瓶颈。`RunPrimal` L794 已在增量维护 `xB`，`Refresh` 的全量重算与之冗余；翻界时 `rhs` 可 O(m) 增量（`rhs -= Acols(j)·Δv`）。此项需独立开关与独立验证。
- **第 ② 期稀疏化**：`A` 改 CSR/CSC（复用既有 `LpSparseMatrix` / `SparseTableauRow`）+ Markowitz 阈值主元 + LUSOL 风格稀疏 η。

## Agent Extensions

### Skill

- **lsp-code-analysis**
- 用途：对 `LuFactorization`、`LinAlg.LuSolve` / `LuSolveT`、`BoundedSimplex.BasisRows`、`BoundedSimplex.Factorization`、`MilpKernels.LuFactorRows` 做定义/引用/调用层级分析，确认全部调用点与改动影响面，避免漏改。
- 预期产出：完整的引用点清单与影响面评估，确保 `Refresh` 拆分后无遗漏调用。

### SubAgent

- **code-explorer**
- 用途：深挖 `Simplex.vb` 的 `LoopPhase1` / `LoopPhase2` 与 `Crossover.vb` 的全部基变更位置，以及 `test/milp` 与 `SimdBenchmark.vb` 的既有约定细节，确认接入点与测试/benchmark 写法可照抄。
- 预期产出：`Simplex.vb` 换基点精确行号清单 + benchmark/自测的既有模式确认。