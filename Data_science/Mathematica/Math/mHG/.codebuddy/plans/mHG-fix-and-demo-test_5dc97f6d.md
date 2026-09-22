---
name: mHG-fix-and-demo-test
overview: 修复 mHG.vbproj 中半成品 R→VB.NET 移植的编译错误与索引/公式错误，并在 test\test.vbproj 中编写可运行的控制台 demo 测试（含 R 官方测试用例与独立交叉验证），使 demo 测试全部通过。
todos:
  - id: verify-api
    content: 用 [subagent:code-explorer] 核实 Vector/NumericMatrix 索引器、Sum、VBDebugger 在 mHG 命名空间下的可用写法
    status: completed
  - id: fix-ratios
    content: 修正 mHGtest.vb 的 d_ratio/v_ratio 公式，并新增 v_ratio 对 b 的向量重载
    status: completed
    dependencies:
      - verify-api
  - id: fix-statistic
    content: 重写 mHGstatisticcalc，修正 HG_row_ncalc.iter/recur 的 n/N 混用并统一 0-based 索引
    status: completed
    dependencies:
      - fix-ratios
  - id: fix-pvalue
    content: 重写 R_separation_linecalc、pi_rcalc、mHGpvalcalc，修正容量、越界与伪 R 切片写法
    status: completed
    dependencies:
      - fix-statistic
  - id: write-tests
    content: 新增 test/ReferenceImpl.vb 独立参考实现，并在 test/Program.vb 编写黄金用例与交叉验证 demo
    status: completed
    dependencies:
      - fix-pvalue
  - id: run-tests
    content: 用 [skill:lsp-code-analysis] 核对调用点后构建并运行 test.vbproj，修复问题直至断言全过、退出码 0
    status: completed
    dependencies:
      - write-tests
---

## 产品概述

当前打开的项目是 VB.NET 实现的「最小超几何检验（mHG / minimum Hypergeometric test，Eden 2007）」，`readme.md` 描述了其算法原理（超几何 p 值最小化 + 动态规划精确置换 p 值）与应用场景（基因集富集、motif 发现、单细胞 marker 筛选）。库项目 `mHG.vbproj` 的代码是从 CRAN `mHG` 包（Kobi Perl）的 R 实现移植而来，但移植未完成：夹杂大量伪 R 语法，并存在公式、索引、容量等多处错误；同时 `test/test.vbproj` 目前只有一个 `Hello World`。

## 核心功能

1. **修复库项目 `mHG.vbproj` 的错误，使其可编译且结果正确**

- 修正两个递推比值函数：d_ratio（对角方向）与 v_ratio（垂直方向）的公式，并按 R 语义支持对 b 的向量化调用。
- 修正统计量计算：区分「循环计数器 n/b」与「总数 N/B」，正确传入超几何参数，并按 0-based 索引访问 `lambdas`。
- 修正 HG 行递推（迭代版与递归版）：把误用的总球数改回目标行号，并把 R 的 1-based 下标整体映射到 0-based。
- 修正 p 值计算链路：R 分隔线求解、pi_r 概率矩阵递推、越界访问与矩阵容量。
- 消除伪 R 字符串切片（如 `"1:(b+1)"`、`$"({bi}+1):({B}+1)"`）与恒假条件（`W < W`、`B < B`）、误改只读总数的写法。
- 修正不存在的 `Warning(...)` 调用。

2. **在测试项目 `test/test.vbproj` 中编写可运行的 demo 测试**

- 提供轻量断言与结果汇总机制（失败时以非 0 退出码结束）。
- 覆盖「已知输入的黄金期望值」用例：小规模排序二元列表的 mHG 统计量、最优截断位置 n、b 与精确 p 值。
- 覆盖内部子函数的定点用例：d_ratio/v_ratio、R 分隔线边界、pi_r 矩阵布局。
- 提供独立交叉验证：自实现超几何分布计算 + 暴力枚举小规模全部排列得到精确置换 p 值，与库的最终结果对比。
- 提供一个真实感场景 demo（较大规模基因列表），直观展示 API 用法与输出。

3. **跑通测试**：构建 0 错误、全部断言通过、进程退出码为 0。

## 技术栈选择

- 语言与运行时：VB.NET，目标框架 `net10.0`（本机已安装 .NET SDK 10.0.401，可直接 `dotnet build` / `dotnet run`）。
- 被改/被验证项目：
- `mHG/mHG.vbproj`（Library，AssemblyName = `Microsoft.VisualBasic.Math.Statistics.Hypothesis.mHG`）。
- `mHG/test/test.vbproj`（Exe），已引用 `Core.vbproj`、`Math/Math.NET5.vbproj`、`Math.Statistics/stats-netcore5.vbproj`、`mHG.vbproj`，无需新增第三方依赖。
- 复用的既有库能力（已核实）：
- `Microsoft.VisualBasic.Math.LinearAlgebra.Vector`：`New Vector(m As Integer)` 得长度 m 全 0 向量；整数索引器 0-based；`Sum` 来自 LINQ（返回 `Double`）。
- `Microsoft.VisualBasic.Math.LinearAlgebra.Matrix.NumericMatrix`：`Create(nrow, ncol)`，`Item(i, j)` 0-based。
- `Microsoft.VisualBasic.Language`：`list(!k = v).AsNumeric`、`x.AsDefault.When(cond)`、`a Or b.AsDefault.When(cond)`（表达式直接返回 `Double`）。
- `VBDebugger.warning(msg)`（Core 项目根命名空间 `Microsoft.VisualBasic`，为当前命名空间祖先，可直接调用）。

## 实现方案

### 总体策略

以 CRAN `mHG` 包（Kobi Perl）的 **R 源码为唯一权威参照**，把 `mHGtest.vb` 与 `HG_row_n/HG_row_ncalc.vb` 从「半移植、夹杂伪 R 语法」修正为**忠实的 0-based 移植**；随后在 `test` 项目中，用 R 官方测试套件的**黄金期望值**加**独立实现交叉验证**（暴力枚举 + 独立超几何分布计算）来证明修复正确。

### 关键决策与理由

1. **统一 0-based 索引约定**（不改动 `Vector`/`NumericMatrix` 的 0-based 语义，仅对 R 下标机械 −1）：

| 对象 | R（1-based） | VB（0-based） |
| --- | --- | --- |
| lambdas | `lambdas[n]` | `lambdas(n - 1)` |
| HG_row | `HG_row[i] = P(X = i-1)` | `HG_row(i-1)`；`HG_row[1] <- 1` → `HG_row(0) = 1` |
| HG_row 求和 | `sum(HG_row[1:b])` | 对 `HG_row(0 .. b-1)` 求和 |
| HG_row 对角递推 | `HG_row[b+1] <- HG_row[b]*d_ratio` | `HG_row(b) = HG_row(b-1)*d_ratio` |
| R_separation_line | 长度 B+1，下标 `b+1` | 长度 B+1，下标 `b` |
| pi_r | `pi_r[w+2, b+2]` | `pi_r(w+1, b+1)`；`pi_r[2,2]` → `pi_r(1,1)`；`pi_r[W+2,B+2]` → `pi_r(W+1,B+1)`，矩阵 (W+2)×(B+2) |
| HG_row_ncalc 内部 | `HG_row_m[X]` | `HG_row_m(X - 1)` |


2. **保持公共 API 签名与数据类不变**（`mHGtest`、`mHGstatisticcalc`、`mHGpvalcalc`、`R_separation_linecalc`、`pi_rcalc`、`d_ratio`、`v_ratio`、`HG_row_ncalc.iter/recur`、`htest`、`mHGstatisticInfo`），降低对其它调用方的破坏半径。
3. **删除伪 R 字符串切片与 `seq` 索引**，统一改为显式 `For` 循环与整型下标：既消除运行时越界/解析失败，也避免 `Vector`（`List(Of Double)`）传给 `IEnumerable(Of Integer)` 索引器的类型不匹配风险。
4. **递推比值为 b 的向量函数**：`d_ratio` 改为纯标量（参数显式 `Double`），`v_ratio` 提供标量 + 向量两个重载；调用点按 R 的 `v_ratio(b + w, 0:(b-1), N, B)` 逐元素展开为循环，避免 Object 后期绑定。
5. **保留并修正递归分支**（`HG_row_ncalc.recur`，O(B·log B)），与迭代分支（O(B·(n−m))）并用；由测试证明两条路径与独立 dhyper 完全一致，保证性能优化分支不引入回归。

### 性能与可靠性

- 复杂度与原算法一致，无额外开销：统计量计算在迭代路径为 O(N·B)、递归路径为 O(B·log B)；p 值链路（R 分隔线 + pi_r）为 O(W·B)。
- p 值使用动态规划精确求解，不引入蒙特卡洛近似。
- 所有临时数组容量按 0-based 重新核定，消除越界；关键循环保留 R 原注释语义，便于与参考实现逐行对照。
- 测试侧暴力枚举仅用于小规模（N ≤ 12），不影响库性能。

## 实施要点（执行注意）

1. `New Vector(x)` 中若 `x` 为 `Double` 会命中**单元素构造**重载，是本项目已存在的隐性缺陷；所有长度必须显式传 `Integer`（如 `New Vector(CInt(B) + 1)`）。
2. `Vector`/`NumericMatrix` 索引均为 0-based；`Vector.Sum` 为 LINQ 全量求和，切片求和必须手写循环或使用 `"1:b"` 字符串索引器（其 `:` 为「起始:个数」语义）。
3. `Warning(...)` 在当前代码库中**不存在**，改为 `Call VBDebugger.warning(msg)`。
4. `Dim` 省略类型声明时依赖 `Option Infer`，数值变量一律显式声明类型（如 `Dim mHG# = 1`），防止 Integer 截断。
5. `mHG.vbproj` 的 `GeneratePackageOnBuild=True` 保持不变；不修改 `mHG.vbproj` 的引用关系与其它无关项目。
6. 测试运行失败时设置 `Environment.ExitCode = 1`，并在控制台输出每个用例的名称、期望值、实际值与汇总统计。
7. 不引入第三方测试框架（保持零依赖），使用轻量断言辅助函数。

## 架构设计

本次为**既有库 bug 修复 + 测试补齐**，不引入新架构模式，维持现有「单一模块 + 内部辅助类」结构：

```
调用链（自顶向下）
mHGtest(lambdas, n_max)
 ├─ mHGstatisticcalc(lambdas, n_max)            # 扫描截断点，维护 HG_row
 │   └─ HG_row_ncalc_func ─┬─ HG_row_ncalc.iter # 迭代递推 HG 行
 │                         └─ HG_row_ncalc.recur# 递归递推 HG 行
 │                             └─ d_ratio / v_ratio（标量或向量）
 └─ mHGpvalcalc(mHG, N, B, n_max)               # 精确置换 p 值
     ├─ R_separation_linecalc(p, N, B, n_max)   # 求 R 分隔线（使用 d_ratio / v_ratio）
     └─ pi_rcalc(N, B, R_separation_line)       # 受 R 线约束的路径概率矩阵
```

测试侧架构：

```
test/Program.vb      # 断言框架 + 黄金用例 + 端到端 demo + 退出码
test/ReferenceImpl.vb # 独立实现：log-gamma 版 dhyper、暴力枚举置换 p 值
```

## 目录结构

```
mHG/
├── mHGtest.vb                         # [MODIFY] 库核心。修正 d_ratio/v_ratio 公式并新增 v_ratio 向量重载；重写 mHGstatisticcalc（分离 n/b 计数器与总数 N/B，修正 lambdas 0-based 访问、HG_row 容量与整型长度）；重写 R_separation_linecalc（消除 W<W、B<B 恒假条件、误改总数、伪 R 字符串切片，按 0-based 重定容量与切片）；重写 pi_rcalc（矩阵 (W+2)x(B+2)，下标整体 −1，保留 pi_r(1,1)=1 特例）；修正 mHGpvalcalc（pi_r(W+1, B+1) 边界、参数显式转换）与 HG_row_ncalc_func（判断条件改用目标行号 ni）；Warning → VBDebugger.warning。
├── HG_row_n/
│   └── HG_row_ncalc.vb                # [MODIFY] 修正 iter/recur/内层递归中把「总数 N」误当「目标行号 n」的问题；所有 HG_row_m 下标按 0-based 机械 −1（`HG_row_m(b_n_start+i) = HG_row_m(b_n_start+i-1)*...`、`HG_row_m(b_n_start) = HG_row_m(b_n_start)*...`）；用整型循环替代 seq 索引；保持 Shared 签名与 Delegate 不变。
├── htest.vb                           # [保持] 不变
├── mHGstatisticInfo.vb                # [保持] 不变
├── readme.md                          # [保持] 不变
└── test/
    ├── test.vbproj                    # [MODIFY-按需] 仅在需要补充 Imports/配置时调整；引用关系保持
    ├── ReferenceImpl.vb               # [NEW] 独立参考实现：基于对数阶乘/对数伽马的超几何 PMF 与右尾概率（不依赖库内部函数）；对给定二元列表暴力枚举 C(N,B) 种排列，独立计算每次排列的 mHG 统计量并统计 statistic <= observed 的比例，得到精确置换 p 值。
    └── Program.vb                     # [MODIFY] Demo 测试入口。包含：轻量断言辅助（AreEqual(expected, actual, tolerance, name) 支持统计量/n/b/p 值比较）；黄金用例（N=2/3 的各种 0/1 列表在 n_max=N/1/2 下的 mHG、n、b、p）；mHGpvalcalc 预定场景（N=4/5/6 不同 B 与 n_max）；R_separation_linecalc 边界（n_max=0，p=0 → 全 0；p=1 → 全 1）；pi_rcalc 约定矩阵布局用例；d_ratio/v_ratio 与独立 dhyper 比值一致性；iter 与 recur 在 N=100,B=40 多种 (m,n,b_n) 下互相一致且等于独立期望行；端到端暴力枚举交叉验证（N≈12,B≈4）；一个 N=1000,B=100 的富集 demo 打印 statistic/n/b/p-value；结尾输出通过/失败汇总并设置退出码。
```

## 关键代码结构（核心公式与映射，供实现时对齐）

```
' 递推比值（R: d_ratio <- function(n,b,N,B) n*(B-(b-1))/(b*(N-(n-1)))）
Public Function d_ratio(ni#, bi#, N#, B#) As Double
    Return ni * (B - (bi - 1)) / (bi * (N - (ni - 1)))
End Function

' 递推比值（R: v_ratio <- function(n,b,N,B) (n*(N-n-B+b+1))/((n-b)*(N-n+1))）
Public Function v_ratio(ni#, bi#, N#, B#) As Double
    Return (ni * (N - ni - B + bi + 1)) / ((ni - bi) * (N - ni + 1))
End Function

' 向量化重载：供 R 中 v_ratio(b + w, 0:(b-1), N, B) 的调用点使用
Public Function v_ratio(ni#, bi As Vector, N#, B#) As Vector
```

## Agent Extensions

### SubAgent

- **code-explorer**
- Purpose: 在动手改写前做一次针对性核对：确认 `Vector` 的整型/字符串索引器重载在 0-based 下标下的实际行为、`Vector.Sum` 是否只有 LINQ 全量重载（决定求和写法）、`NumericMatrix.Create`/`Item(i,j)` 的边界、以及 `VBDebugger.warning` 在 `mHG.vbproj` 命名空间下是否需要额外 `Imports`。
- Expected outcome: 给出各 API 的确定签名与所在文件行号，以及「哪些写法在 `mHG.vbproj` 中可直接编译」的结论，避免改写过程中出现二次编译错误。

### Skill

- **lsp-code-analysis**
- Purpose: 在改写完成后确认 `mHGtest` 模块内各内部函数的调用点（`HG_row_ncalc_func` / `d_ratio` / `v_ratio` / `HG_row_ncalc.iter|recur`）与 `test` 项目的引用都没有遗漏，且公共签名未被破坏。
- Expected outcome: 一份调用点/引用清单，确保没有遗留的旧调用方式（如把 Vector 当索引、Object 参数绑定），公共 API 保持兼容。