# LppSolver — 内点法 + Crossover 线性规划求解器

VB.NET (.NET 10) 类库型控制台项目，**零第三方依赖**（仅 .NET BCL）。按《内点法 +
Crossover 求解线性规划》readme 实现现代 barrier 求解器标准流水线：

```
原始-对偶内点法 (Mehrotra 预测-校正)
   │  中心路径收敛；每步: 正规方程 A·D²·Aᵀ·Δy = rhs（D²=diag(x_i/s_i)）
   │  静态正则化 + 迭代精化；预测/校正共享同一 Cholesky 因子
   ▼
近似最优内点解（最优面解析中心，非基解）
   │  Crossover：严格互补划分 → 贪心构基(秩检测) → 比率测试消超基本 → 对偶检查
   ▼
最优基可行解（顶点；影子价/reduced cost 供敏感性分析）
   → 经用户提供的 LPPSolution 类返回
```

## 一、构建与运行

```bash
dotnet run -c Release -- demo        # 三个示例问题（教科书例/混合约束/不可行）
dotnet run -c Release -- selftest    # 8+1 组自检（对应 Python 镜像验证）
```

## 二、readme.md 文档 → 代码映射表

### §一 问题设定与 KKT 条件

| 文档概念 | 实现 | 代码位置 |
|---|---|---|
| (P) min cᵀx, Ax=b, x≥0；(D) Aᵀy+s=c, s≥0 | `StandardForm.FromProblem`：max→min（σ=−1）、≤/≥ 加/减松弛、b<0 行翻转 | `Lpp/LppProblem.vb` |
| 原始/对偶可行 + 互补松弛 + 强对偶 | 收敛判据 `‖r_p‖/(1+‖b‖), ‖r_d‖/(1+‖c‖), μ/(1+|cᵀx|) ≤ 1e-8`；输出量满足 KKT（自检 T7 验证） | `InteriorPoint.Solve` |

### §二 内点法计算原理

| 文档概念 | 实现 | 代码位置 |
|---|---|---|
| §2.1 松弛互补 x·s = μ → F_μ 牛顿方程 | 正规方程消元推导（见下"公式推导"） | `InteriorPointSolver` |
| §2.2 正规方程 AD²Aᵀ, D²=diag(x_i/s_i) | `M(i,k) = Σ_j A(i,j)·d2(j)·A(k,j)` | `Solve` 主循环 |
| §2.2 静态正则化 + 迭代精化 | `Cholesky(M, reg)` + 残差回代校正一次；**步长停滞 → reg×1000 自适应重试 ≤4 档**（工程扩展，Klee-Minty 必需） | `LinAlg.SolveSpd`/`Cholesky` 重载 |
| §2.2 预测-校正共享同一因子 | 每档位一次 `Cholesky`，预测与校正各两段回代 | `NewtonDir` |

### §三 Mehrotra 预测-校正步骤（逐步对应）

| 步骤 | 实现 |
|---|---|
| 初始点 x⁰>0,y⁰,s⁰>0 | **Mehrotra (1992) 起始点启发式**：最小范数 x̂=Aᵀ(AAᵀ)⁻¹b、ŝ=c−Aᵀ(AAᵀ)⁻¹Ac + 偏移平衡 δ=1.5·min⁻、δ̂=0.5(xᵀs)/Σ |
| 预测步（仿射尺度 σ=0） | `NewtonDir(0, 0)`：rhs = −r_p − A·S⁻¹γ − A·D²·r_d（S⁻¹γ = −x；可行情形退化为 readme 方框公式 b − σμ·A·S⁻¹e ✓） |
| fraction-to-boundary αᵃ | `MaxStep`：min(1, min_{Δ<0} −v/Δ)；μᵃ = (x+αᵃΔxᵃ)ᵀ(s+αᵃΔsᵃ)/n |
| σ = (μᵃ/μ)³ | 校正步 `NewtonDir(σμe, ΔX_aff·ΔS_aff·e)` |
| 原始/对偶分离步长 γ=0.99 | `aP = min(1, 0.99·MaxStep(x,dx))`；`aD` 对偶侧独立 |

### §四 Crossover（核心三阶段）

| 阶段 | 实现 | 代码位置 |
|---|---|---|
| 阶段0 严格互补划分 | P={x_j>κ 且 x_j≥s_j}（基候选）、Z={其余→非基下界 0}；κ=1e-8·max(1,‖x‖∞) | `CrossoverSolver.Run` |
| 阶段1 构造初始基 | 候选按 x 降序贪心 + `EchelonRank` 线性无关检测；不足 m 列用剩余列补（优先稀疏） | 同上 |
| 阶段2 主元循环消超基本 | 超基本 j 沿"压向下界"方向比率测试 δ_max = min_{α_i<0} x_B,i/(−α_i)：δ_max ≥ x_j → 直接到界（翻界）；否则阻挡基本出界(到0)、j 以 x_j−δ_max 入基——**全程保原始可行** | 同上 |
| 阶段3 单纯形收尾 | 对偶检查：非基 reduced cost < −tol → `SimplexSolver.LoopPhase2` 迭代至精确最优 | `Simplex.vb` |

### §五 与单纯形法比较（文档化设计选择）

| 维度 | 本实现 |
|---|---|
| 迭代复杂度 | IPM ~7-25 次迭代与规模无关（T1: 7 次）；单纯形最坏指数（Klee-Minty T4 IPM 轻松、单纯形经典指数例） |
| 每步代价 | 稠密 Cholesky O(m³)（**简化声明**：生产用稀疏 Cholesky+AMD；本实现适合中等规模稠密问题） |
| 退化鲁棒性 | IPM 迭代数不受退化影响；单纯形侧 Dantzig+停滞 20 轮切 Bland 防循环 |
| 证书 | 不可行（Phase 1 人造目标 > tol）/ 无界（比值检验无阻挡行）由单纯形给出权威证书（T5/T6） |

## 三、公式推导（readme §2.2 牛顿方程的完整消元，实现依据）

```
牛顿系统:  A·Δx = −r_p                        (1)
           Aᵀ·Δy + Δs = −r_d                  (2)
           S·Δx + X·Δs = γ_c ≡ −XSe + σμe + ΔX_aff·ΔS_aff·e   (3)

(2)→ Δs = −r_d − AᵀΔy；代入(3)：S·Δx = γ_c + X·r_d + X·AᵀΔy
  → Δx = S⁻¹γ_c + D²·r_d + D²·AᵀΔy    (S⁻¹X = D²；S⁻¹γ_c = −x + σμ/s + corr/s)
代入(1)：A·D²·Aᵀ·Δy = −r_p − A·S⁻¹γ_c − A·D²·r_d ≡ rhs   ★

验证(3)：S·Δx + X·Δs = X·AᵀΔy − Sx + σμe + corr + X·r_d − X·r_d − X·AᵀΔy
                     = −XSe + σμe + corr ✓
可行情形 r_p=r_d=0：rhs = Ax − σμ·A·S⁻¹e = b − σμ·A·S⁻¹e ✓（readme 方框公式）
```
★ 实现陷阱（Python 镜像抓到）：`rhs` 中 +Ax 已经含在 −A·S⁻¹γ_c 的 −A·(−x) 里，
不可再单独加 A·x，否则原始残差发散。

## 四、API 与输出

```vb
Dim problem As New LppProblem With {.ObjectiveSense = "max"}
problem.Variables.Add(New LppVariable("x1", 3.0))
problem.Constraints.Add(New LppConstraint(
    New Dictionary(Of String, Double) From {{"x1", 1}}, "<=", 4.0))
Dim sol As LPPSolution = LppSolver.Solve(problem)
```

`LPPSolution`（用户提供的类，原样集成）填充约定：

| 成员 | 约定 |
|---|---|
| `GetSolution`/`solution` | 原始变量最优值 |
| `ObjectiveFunctionValue` | **原始方向**目标值（max 问题报 max 值） |
| `slack(i)` | b_i − A_i·x：=0 → binding；>0 → slack（≤ 非绑定）；<0 → surplus（≥ 非绑定）——与 `constraintSensitivityString` 打印分档一致 |
| `shadowPrice(i)` | ∂(原始目标)/∂b_i = σ·flipSign_i·y_i（max 的 ≤ 行教科书值 ≥0） |
| `reducedCost(j)` | c_j − Σ_i A_ij·shadowPrice_i（原始方向；min 最优时非基 ≥0） |
| `SolveTime/FeasibleSolutionTime` | 总耗时 ms / 首次可行（IPM 收敛）时刻 ms |
| `SolutionLog` | IPM 逐迭代 (r_p, r_d, μ, obj) + crossover 主元明细 + 兜底状态 |
| `SolverError` + `failureMessage` | 不可行/无界/数值失败的消息（消息文本含"不可行"/"无界"关键字） |

**集成说明**：若宿主工程（如 sciBASIC）已自带 `IsNullOrEmpty`/`StringEmpty`/`NamedValue`
扩展，删除 `CompatHelpers.vb` 避免扩展方法二义性。

## 五、验证体系

1. **Python 镜像对拍**（`_validation/validate_lpp.py`，24/24 通过）：与 VB 逐式对应，
   含 numpy 基准对拍（`lu_solve_T` vs `np.linalg.solve(Bᵀ,·)`）。
   开发期抓到的三个真实 bug：★ rhs 双计 Ax；`lu_solve_T` 三角回代方向写反；
   Klee-Minty 上 D² 截断致 M 奇异 → 方向爆炸 → 步长 0 冻结（自适应正则化修复）。
2. **内置 SelfTest**（`selftest`）：T1 教科书例（obj=36、x*=(2,6)、影子价 (0,1.5,1)）、
   T2 ≥/≤ 混合（surplus 符号）、T3 退化多解、T4 Klee-Minty、T5/T6 证书、
   T7 随机 25×65 KKT（原始可行+互补松弛+目标对账）、T8 冗余等式行删除、
   T9 LPPSolution 接口行为。

## 六、已知边界（如实声明）

| 项 | 现状 |
|---|---|
| 变量域 | x ≥ 0（标准形；自由变量/上下界需改造——文档化未做） |
| 稀疏性 | 全稠密实现；超大规模稀疏 LP 需稀疏 Cholesky + AMD 排序（readme §2.2 提及，未实现） |
| ΔG… 即数值容差 | IPM 1e-8、单纯形 1e-9、crossover 划分 κ=1e-8——高度退化问题可能需要调紧/调松 |
| 起始点 | Mehrotra 启发式对病态缩放问题可能多耗迭代（无 Ruiz 均衡——readme 未要求，留作扩展） |
| 不可行/无界 | 由单纯形 Phase 1/2 给出权威证书；IPM 残差分类仅作快速路径 |
| 兜底链 | IPM 任意失败 → 纯单纯形独立求解（正确性不依赖 IPM 成功） |

## 七、文件清单

```
lpp-ipm-crossover/
├── LppSolver.vbproj            net10.0（无 PackageReference）
├── Program.vb                  demo 入口
├── SelfTest.vb                 自检（T1-T9）
├── LPPSolution.vb              用户提供的结果类（原样 + Imports）
├── CompatHelpers.vb            IsNullOrEmpty/StringEmpty/NamedValue 最小实现
├── Lpp/
│   ├── LinAlg.vb               Cholesky(±reg)/迭代精化/LU/LuSolveT/阶梯秩
│   ├── LppProblem.vb           输入模型 + 标准形转换（σ/翻转/松弛映射）
│   ├── InteriorPoint.vb        Mehrotra IPM（自适应正则化、停滞检测）
│   ├── Simplex.vb              Phase1/2 修订单纯形（证书 + 收尾）
│   ├── Crossover.vb            划分→构基→主元循环→对偶检查
│   └── LppSolver.vb            流水线组装 → LPPSolution
└── _validation/validate_lpp.py Python 镜像（24 项测试）
```
