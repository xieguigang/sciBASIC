# ODE 求解器扩展：结果导出与相关性分析

## 引言

仿真本身只是中间产物。真正的问题是：**积分完成后，如何把结果变成可分析、可作图、可复现的数据？**

常见需求有三条：

1. **导出**：每一步的变量取值 → 数据框 / CSV；
2. **回读**：把之前导出的结果重新载入，避免重复积分；
3. **分析**：计算变量之间的相关性（谁随谁变化）。

本包把这三条补齐，让「积分 → 分析 → 绘图」成为一条无胶水代码的流水线。

## 核心能力

| 能力 | 说明 |
|---|---|
| **导出为数据框** | 把积分输出（逐步的变量取值）转为数据框 |
| **导出为 CSV** | 落盘为标准表格，便于外部工具消费 |
| **重新载入** | 从文件恢复为数据框，跳过重复积分 |
| **相关性分析** | 计算被建模变量之间的 Pearson / Spearman 相关 |

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.Math.Calculus

' 1. 积分（结果来自 odes 包）
Dim result = ODESolver.RK4(model, tEnd:=10.0, steps:=1000)

' 2. 导出为数据框与 CSV
Dim df = result.ToDataFrame()
Call df.SaveCsv("./simulation.csv")

' 3. 重新载入（无需重跑）
Dim reloaded = DataFrame.read_csv("./simulation.csv")

' 4. 变量间相关性（Pearson / Spearman）
Dim corr = result.VariableCorrelation(correlation:=Correlation.Pearson)
```

## 实现要点

- **为什么要把结果落盘**：积分可能很慢（尤其刚性方程的精细步长），而后续分析（换参数、换指标、重画图）需要反复访问结果。落盘一次，后续任意次分析都无需重算。
- **Pearson 与 Spearman 的选择**：Pearson 衡量**线性**相关，对离群点敏感；Spearman 基于秩，衡量**单调**相关，更稳健。动力学变量之间常是非线性单调关系（如一次反应产物与时间），此时 Spearman 更合适。
- **与绘图包的衔接**：导出为数据框后可以直接交给 `Plots` / `plots_extensions` 绘制时间序列、相图与相关性热图，无需额外的数据转换层。

## 包信息

- Assembly：`Microsoft.VisualBasic.Math.Calculus`（`ODESolver.Extensions`）
- TargetFramework：`net10.0`
- Tags：`scibasic;ode-solver;data-frame;correlation;pearson;spearman;csv-export`
- 许可：GPL-3.0-or-later
