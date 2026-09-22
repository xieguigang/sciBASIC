# 核心数学库：线性代数、数值方法与更多

## 引言

`Math` 是 `sciBASIC#` 数学栈的基座：上层的数据分析、机器学习与绘图都建立在这里提供的数据结构（矩阵 / 分布 / 插值 / 采样）与求解器（线性系统 / 优化 / 线性规划）之上。

它覆盖的领域相当广，可以按「你在做什么」来定位：

| 你要做的事 | 去哪里找 |
|---|---|
| 解线性方程组 / 求 SVD | `LinearAlgebra`、`LinearAlgebra.Solvers` |
| 做线性规划 / 整数规划 | `LinearAlgebra.LinearProgramming`（含 `MILP`、`IPMCrossover`） |
| 求最优解（含边界约束） | `Numerics.Framework.Optimization.LBFGSB` |
| 描述随机变量 / 分箱统计 | `Distributions`、`Distributions.BinBox` |
| 对时间序列降采样 | `DownSampling`（LTTB / MinMax / Mixed / TimeGap） |
| 插值 / 求分位数 | `Interpolation`（样条）、`Quantile` |
| 解析并执行数学表达式 | `Scripting`（含 `MathExpression`、BasicR 桥） |
| 近似集合相似度 | `HashMaps.MinHash` |
| 模糊推理 | `Logical.FuzzyLogic` |
| 高精度十进制运算 | BigDecimal 算术支持 |

## 命名空间地图

| 命名空间 | 职责 |
|---|---|
| `Microsoft.VisualBasic.Math`（根） | 数值辅助与共享类型 |
| `....Math.Algebra`（+ `LP` / `LP.IPMCrossover` / `Matrix.NET` / `Matrix.NET.MDS` / `MILP` / `Solvers`） | 线性代数、线性规划、矩阵与求解器 |
| `....Math.Numerics`（+ `Framework` / `Optimization` / `Optimization.LBFGSB`） | 数值方法与优化框架 |
| `....Math.Distributions`（+ `BinBox` / `Summary`） | 概率分布与分箱 |
| `....Math.DownSampling`（+ `LargestTriangleBucket` / `MaxMin` / `Mixed` / `TimeGap`） | 时间序列降采样 |
| `....Math.Quantile` / `....Math.Spline` | 分位数与样条插值 |
| `....Math.Scripting`（+ `Expression` / `R`） | 数学表达式脚本 |
| `....Math.HashMaps` / `....Math.Logical.FuzzyLogic` | MinHash 与模糊逻辑 |

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.Numerics.Framework.Optimization.LBFGSB

' 1. 线性代数：求解 Ax = b
Dim x = Matrix.Solve(A, b)

' 2. 有界优化：L-BFGS-B
Dim result = LBFGSB.Minimize(objective, lower, upper, x0)

' 3. 降采样：把曲线压到 500 个点（保留形状特征）
Dim reduced = DownSampling.LargestTriangleBucket(source, points:=500)
```

## 实现要点

- **稠密与稀疏并存**：小规模稠密问题用稠密矩阵（缓存友好），大规模稀疏问题用稀疏表示（避免零元素存储与运算）。
- **L-BFGS-B 的适用面**：多数实际优化问题都有变量边界（浓度 ≥ 0、比例 ∈ [0,1]）；L-BFGS-B 在保持拟牛顿法收敛速度的同时处理边界，因此是默认选择。
- **降采样方法为何有多种**：LTTB 保留整体形状（适合展示趋势），MinMax 保留极值（适合展示波动），TimeGap 按时间间隔抽稀（适合不均匀采样）。选择取决于"你想让读者看到什么"。
- **表达式脚本的意义**：把公式写成文本而非编译进代码，意味着公式可以由用户在运行时提供——这是报表与可配置分析的基础。

## 包信息

- Assembly：`Microsoft.VisualBasic.Math`
- TargetFramework：`net10.0`
- Tags：`scibasic;mathematics;linear-algebra;sparse-matrix;svd;numerics;linear-programming;bigdecimal;spline;quantile;expression-scripting`
- 许可：GPL-3.0-or-later
