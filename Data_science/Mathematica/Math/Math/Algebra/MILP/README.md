# MILP — 混合整数线性规划求解器（分支定界 + 割平面 + 启发式）

在既有 LP 基础设施之上实现的**中等规模 MILP 求解内核**，零第三方依赖（仅 .NET BCL
与工程内既有代码）。按 `readme.txt` 的思路，把 MILP 拆成两层：

```
连续层：LP 松弛（有界变量修订单纯形，l ≤ x ≤ u 原生表达）
   │
   │  组合层
   ├─ 预处理 presolve   ：界传播收紧、固定变量消元、自由变量规范化、不可行判定
   ├─ 根节点割平面       ：Gomory 混合整数割（GMI），全局有效不等式
   ├─ 整数启发式         ：舍入（rounding）+ 潜水（diving）
   └─ 分支定界           ：best-bound / depth-first，节点 LP 热启动重优化
   ▼
原始空间最优解 / 目标值 / 最优界 / 相对间隙 / 求解统计
```

## 一、调用示例

```vb
Imports Microsoft.VisualBasic.Math.LinearAlgebra.LinearProgramming.MILP

' max 5x + 4y, 6x + 4y ≤ 24, x + 2y ≤ 6, x/y 为非负整数（整数最优 20，LP 松弛 21）
Dim model As New MilpModel With {.ObjectiveSense = "max"}

model.AddVariable("x", 5, MilpVarType.GeneralInteger)
model.AddVariable("y", 4, MilpVarType.GeneralInteger)
model.AddConstraint(New Dictionary(Of String, Double) From {{"x", 6.0}, {"y", 4.0}}, "<=", 24)
model.AddConstraint(New Dictionary(Of String, Double) From {{"x", 1.0}, {"y", 2.0}}, "<=", 6)

Dim sol As MilpSolution = MilpSolver.Solve(model, New MilpOptions With {.MaxSeconds = 30})

Console.WriteLine(sol.ToString())          ' 状态 / 目标值 / 最优界 / 间隙 / 解 / 统计
Console.WriteLine(sol.GetSolution("x"))    ' 按变量名取解
```

变量类型：`MilpVarType.Continuous`（连续）、`MilpVarType.GeneralInteger`（一般整数）、
`MilpVarType.Binary`（二进制，界自动规范化到 [0,1]）；变量上下界由
`AddVariable(name, c, type, lowerBound, upperBound)` 指定。

## 二、readme.txt 概念 → 代码映射

| readme.txt 概念 | 实现 | 代码位置 |
|---|---|---|
| LP 松弛（线性代数子程序） | `BoundedSimplex`：`min cᵀx, Ax=b, l ≤ x ≤ u`；Phase 1 符号人工基 + Phase 2 原始单纯形 + 对偶单纯形翻界重优化；线性代数全部走 `LinAlg` | `BoundedSimplex.vb` |
| 分支定界（递归分割变量域） | `BranchAndBound`：best-bound / depth-first 节点池、按界剪枝、`x_j ≤ ⌊v⌋ / x_j ≥ ⌈v⌉` 双子节点、incumbent 与全局界 | `BranchAndBound.vb` |
| 割平面（Gomory 有效不等式） | `GomoryCut`：由最优基 tableau 行 `ā = (B⁻ᵀe_i)ᵀA` 生成 GMI 割，回溯到工作变量空间后追加新行 + 新松弛列 | `GomoryCut.vb` |
| 启发式（树中快速找可行解） | `MilpHeuristics`：舍入 + 潜水（逐变量定界并热启动重解） | `MilpHeuristics.vb` |
| presolve | `MilpPresolve`：活动度界传播（min/max activity）、整数界取整、固定变量消元、空行/不可行判定 | `MilpPresolve.vb` |
| 变量域 / 上下界 / 自由变量 | `MilpLpForm`：工作列 = (原始变量, 符号, 平移)，`x_orig = shift + sign·x_work`；l=−∞ 走翻转，双侧无限走两列拆分 | `MilpPresolve.vb` |
| 目标 min / max | `Sigma = ±1`，`c_work = Sigma·c_orig·sign`；原始目标 = `ObjOffset + Sigma·(c_workᵀx_work)` | `MilpPresolve.vb` / `MilpSolution.vb` |
| 结果与报告 | `MilpSolution`（状态、解、目标值、最优界、相对间隙、节点/LP/割/启发式计数、耗时、日志） | `MilpSolution.vb` |

## 三、关键设计决策

1. **界原生表达，避免下界平移**：`BoundedSimplex` 直接以 `l ≤ x ≤ u` 为工作形式。
   分支只改 `l/u`，`A/b/c` 不变 ⇒ 父节点基可直接热启动（对偶单纯形修复）。
   这规避了"下界平移 ↔ 分支改界"之间的映射风险（MILP 实现最大的正确性雷区）。
2. **基的人工列用负数索引**：`basis(k) ≥ 0` 是工作列，`basis(k) < 0` 是第 `−1−k` 个人工列。
   人工列与工作列索引空间分离，所以**追加工作列（割的松弛列）不会移动人工列下标**，
   热启动基可以安全跨越"加行 + 加列"。
3. **冗余行的人工列留在基中并固定为 0**：该行的工作列系数全为 0，永远不会阻塞主元，
   因此无需删行，避免了行号重排。
4. **Phase 1 使用符号人工列**：`artSign(i) = sign(b_i − A_i·v)`，人工列 = `artSign(i)·e_i`，
   `b < 0` 的行无需翻转（矩阵保持原样）；Phase 1 目标恒为 `Σ 人工变量`（工作列成本置 0）。
5. **割平面只在根节点生成（cut-and-branch）**：GMI 割是全局有效不等式，根节点收紧后
   整棵树受益；节点级割会显著增加每节点开销，留作扩展。
6. **预处理保持索引不变**：只收紧界、只把固定列消元（列置零/删列但记录固定值），
   不做行删减重排 ⇒ "工作解 → 原始解"的映射始终索引对齐。

## 四、数值约定与容差

| 项 | 取值 |
|---|---|
| LP 原始/对偶可行性容差 | `MilpOptions.FeasibilityTolerance`，默认 1e-7 |
| 整数判定 / 分支容差 | `MilpOptions.IntegerTolerance`，默认 1e-6 |
| 单纯形主元阈值 | 1e-9 |
| 相对间隙终止 | `MilpOptions.RelativeGap`，默认 1e-4 |
| 绝对间隙 | `MilpOptions.AbsoluteGap`，默认 1e-6 |
| 割系数尺度上限 | `MilpOptions.CutCoefficientLimit`，默认 1e9 |
| 割密度上限 | `MilpOptions.CutDensityLimit`，默认 0.9 |
| 行均衡 | 每个约束行按 `max|a_ij|` 归一到 O(1)（行缩放不改变可行域） |

终止条件：时间上限（`MaxSeconds`）、节点上限（`MaxNodes`）、开集规模上限（`MaxOpenNodes`，
内存保护）、相对间隙、搜索树遍历完成。对应状态：
`Optimal / Infeasible / Unbounded / TimeLimit / NodeLimit / GapLimit / Error`。

## 五、自检与演示

```bash
cd test
dotnet run -- selftest        # 内置自检（T1–T12）
dotnet run -- demo            # 8 个演示问题（背包 / 选址 / 指派 / 混合整数 / 不可行 / 无界 / 30 件背包）
dotnet run -- lpp             # 既有 IPM+Crossover 线性规划演示（回归入口）
dotnet run -- lpp-selftest    # 既有线性规划自检（回归入口）
```

自检覆盖（T1–T12）：已知最优对拍、与**暴力枚举**逐实例对拍（20 个随机纯整数实例）、
整数间隙问题（LP 21 → 整数 20）、不可行与无界证书、预处理开关一致性、
割平面实际生成（分数根松弛，4 条 GMI 割）与全幺模对照（根松弛天然整数、不产生割）、
全部模型的可行性 / 整数性 / 目标值对账，以及**分支规则 × 节点选择规则**四种组合
与一维动态规划精确解对拍（30 件 0/1 背包，精确最优 304.3）。

演示中的 30 件 0/1 背包可见完整流水线效果：根松弛 306.48 → 15 条割 → 61 个节点
→ 整数最优 304.3（与 DP 精确解一致；相对间隙 9.4e-5 ≤ 1e-4 触发间隙终止）。


## 六、已知边界（如实声明）

| 项 | 现状 |
|---|---|
| 割平面 | 仅根节点（cut-and-branch）；节点级割 / Mir / 覆盖割未实现 |
| 分支规则 | most-fractional（默认）/ first-fractional / 伪成本（pseudo-cost，基于子节点界退化在线学习）；节点选择 best-bound（默认）/ depth-first |
| 单一相对间隙语义 | 相对间隙按 `|incumbent − bound| / max(1, |incumbent|)` 计算，与 Gurobi 的 `|primal − dual|/(1e-10+|primal|)` 略有差异 |
| 节点 LP | 每次主元重构 LU（与既有 `SimplexSolver` 一致）；未做 LU 更新与稀疏 Markowitz 主元 |
| 稀疏大规模 | 工作矩阵为稠密 `Double(,)`，面向中等规模（数百 ~ 数千变量）；基因组规模 FBA 仍应走既有稀疏 LP 路径 |
| 整数变量下界 | 要求有限下界（否则分支定界无法终止），校验阶段会明确报错 |
| 预处理 | 未做系数缩放消除、隐含整数（implied integer）、行/列冗余的完整 presolve |
| 敏感性报告 | `MilpSolution.Lp`（内嵌 `LPPSolution`）当前未填充；MILP 节点的影子价 / reduced cost 未对外报告 |
| 最优性语义 | `MilpStatus.Optimal` 表示"搜索树遍历完成"或"相对间隙 ≤ 容差"；若存在因 LP 数值失败被丢弃的子树（`DroppedNodes > 0`），状态降级为 `Error` 而不宣称最优 |
| 节点 LP 热启动 | 先热启动（对偶单纯形）；任何非最优结果都会**冷启动重解一次**兜底，避免把数值失败误判为不可行而静默丢子树 |

## 七、文件清单

```
Algebra/MILP/
├── readme.txt           需求与算法背景说明（保持不变）
├── README.md            本文档
├── MilpModel.vb         MilpVarType / MilpVariable / MilpModel（建模与校验）
├── MilpOptions.vb       求解选项与枚举（BranchRule / NodeRule）
├── MilpSolution.vb      MilpStatus / MilpSolution（结果与报告）
├── MilpPresolve.vb      预处理 + MilpLpForm（工作形式与映射）
├── BoundedSimplex.vb    有界变量修订单纯形（Phase1 / 原始 / 对偶 + 翻界）
├── GomoryCut.vb         GMI 割生成
├── MilpHeuristics.vb    舍入 + 潜水启发式
├── BranchAndBound.vb    分支定界搜索 + 根节点割平面
└── MilpSolver.vb        顶层入口（流水线编排）

test/milp/
├── ProgramMilp.vb       演示入口（demo / selftest）
└── MilpSelfTest.vb      内置自检 T1–T11
```

## 八、复用关系（未修改任何既有 LP 代码）

| 复用对象 | 用途 |
|---|---|
| `LinearProgramming.LppVariable`（`LowerBound` / `UpperBound`） | `MilpVariable` 的基类 |
| `LinearProgramming.LPPSolution` | 结果类（`MilpSolution` 保留可选内嵌字段） |
| `LinearProgramming.OptimizationType` / `LpSparseMatrix` | 目标方向语义 / 稀疏矩阵（预留） |
| `IPMCrossover.LppConstraint` | MILP 约束（字典系数 + 运算符 + 右端项） |
| `IPMCrossover.LinAlg` / `LuFactorization` | LU 分解、前后代、转置求解、2-范数与点积 |
| `IPMCrossover.LuSolveT` | tableau 行 `B⁻ᵀe_i`（割平面推导） |

`BoundedSimplex` 与既有的 `SimplexSolver`（x ≥ 0 标准形）、`InteriorPointSolver`
（有界变量内点法）、`CrossoverSolver` 形成互补：前者服务 MILP 节点的
"改界热启动"，后者服务一次性连续 LP 求解。
