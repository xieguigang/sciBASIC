# Kaplan-Meier 生存分析与两组比较

## 引言

生存分析处理的是**「到事件发生的时间」**这类数据，它有两个特点让普通统计方法失效：

1. **删失（censoring）**：很多样本在研究结束时还没发生事件（如未复发），它们的信息是「至少活了这么久」，不能简单丢弃；
2. **时间分布高度偏态**：几乎从不满足正态假设。

**Kaplan-Meier 估计**正是为此设计：它不再估计「平均生存时间」，而是估计**生存函数 S(t)**——「活过时间 t 的概率」，并自然地处理删失。

## 核心能力

| 能力 | 说明 |
|---|---|
| **两组生存曲线** | 分别为两个队列计算 Kaplan-Meier 曲线 |
| **时间事件合并** | 把两组的「时间 → 事件」观测合并到共同时间轴 |
| **组间显著性检验** | 判断两条曲线是否有显著差异（log-rank 风格） |
| **分组策略** | 把连续变量（如某基因表达量）二分为高 / 低组（`SplitStrategies`） |
| **批量并行运行** | 对成千上万个候选基因并行做「分组 + 检验」 |

## 命名空间

| 命名空间 | 职责 |
|---|---|
| `Microsoft.VisualBasic.Math.KaplanMeierEstimator`（根） | 估计器与曲线计算 |
| `....KaplanMeierEstimator.Models` | 曲线与事件模型 |
| `....KaplanMeierEstimator.SplitStrategies` | 连续变量二分策略 |

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.Math.KaplanMeierEstimator

' 1. 准备两组「时间 + 事件」观测
Dim high = cohort.SplitBy(expression, SplitStrategies.Median)
Dim low = cohort.SplitBy(expression, SplitStrategies.Median)

' 2. 计算两组生存曲线
Dim curveHigh = KaplanMeier.Estimate(high)
Dim curveLow = KaplanMeier.Estimate(low)

' 3. 组间显著性检验
Dim p = KaplanMeier.LogRankTest(curveHigh, curveLow)
Console.WriteLine($"p = {p}")

' 4. 批量筛选预后标志物（并行）
Dim hits = PrognosticScreener.Scan(expressionMatrix, survivalData)
```

## 实现要点

- **删失为什么不能直接丢弃**：删失样本提供了「在观察期内未发生事件」这一信息；丢弃它们会**系统性高估**风险，因为「活得久的样本」恰恰最容易被删失。
- **曲线为什么是阶梯状**：Kaplan-Meier 是**非参数**估计——只在事件发生的时间点更新生存概率，因此曲线呈阶梯形，而非平滑曲线。
- **分组阈值的选择**：`SplitStrategies`（中位数、最优切点等）直接决定显著性水平。使用「最优切点」会引入**多重比较偏差**，因此批量筛选时应当做相应的校正。
- **批量并行的意义**：一个表达矩阵常含数万基因，逐个检验在单线程下非常慢；并行化使全基因组预后筛选变得可行。

## 包信息

- Assembly：`Microsoft.VisualBasic.Math.KaplanMeierEstimator`
- TargetFramework：`net10.0`
- Tags：`scibasic;survival-analysis;kaplan-meier;log-rank;gene-expression;split-strategy;biostatistics`
- 许可：GPL-3.0-or-later
