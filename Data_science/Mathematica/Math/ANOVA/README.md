# ANOVA 与多变量分析工具箱

## 引言

组学数据分析的常见流程是**两个层次**的对比：

1. **单特征层面**：某个代谢物 / 基因在不同实验组之间是否有显著差异？
2. **整体层面**：所有样本在多维空间中如何分布？分组是否可分？

本包同时覆盖这两层：单特征的参数与非参数检验，以及多变量的投影与判别方法。

## 核心能力

### 单特征检验

| 方法 | 适用场景 | 说明 |
|---|---|---|
| **单因素 ANOVA F 检验** | 多组、近似正态、方差齐性 | 比较组均值，输出 F 统计量与 p 值 |
| **Kruskal-Wallis** | 多组、非正态或含离群值 | 基于秩的非参数替代方案 |
| **FDR 校正** | 多重检验 | 组学数据检验成千上万个特征，必须控制假发现率 |

### 多变量分析

| 方法 | 用途 |
|---|---|
| **PCA** | 无监督降维：看样本整体分布与离群 |
| **PLS** | 有监督投影：找出与响应变量最相关的成分 |
| **OPLS-DA** | 判别分析 + 正交信号校正：把「与分组相关」和「与分组无关」的变化分开 |

## 命名空间与类型

| 命名空间 | 内容 |
|---|---|
| `Microsoft.VisualBasic.Math.Statistics.Hypothesis.ANOVA`（根） | ANOVA / Kruskal-Wallis 检验与多变量方法 |
| `....ANOVA.stats` | 配套统计辅助 |

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.Math.Statistics.Hypothesis.ANOVA

' 1. 单因素方差分析
Dim aov = ANOVA.FTest(measurements, groupLabels)
Console.WriteLine($"F={aov.F}, p={aov.PValue}")

' 2. 非参数替代 + FDR 校正
Dim kw = KruskalWallis.Test(measurements, groupLabels)
Dim adjusted = KruskalWallis.FDR({kw.PValue, otherP1, otherP2})

' 3. 多变量投影
Dim scores = PCA.Scores(matrix, components:=2)
Dim da = OPLS_DA.Fit(matrix, groupLabels)
```

## 实现要点

- **为什么必须做 FDR 校正**：若对 10000 个特征各用 p<0.05，即使全部无差异，也期望有 500 个假阳性。FDR 校正（Benjamini-Hochberg）控制的是**错误发现比例**，比 Bonferroni 更契合组学的探索性场景。
- **ANOVA 的前置假设**：ANOVA 假设各组方差齐性且近似正态；实际数据常常不满足。因此提供 Kruskal-Wallis 作为默认的非参数替代。
- **OPLS-DA 与 PLS-DA 的差别**：OPLS-DA 额外把与分组**无关**的系统性变异（正交成分）单独提取出来，因此判别成分的可解释性更强——这也是代谢组学中它更受青睐的原因。

## 包信息

- Assembly：`Microsoft.VisualBasic.Math.Statistics.Hypothesis.ANOVA`
- TargetFramework：`net10.0`
- Tags：`scibasic;anova;kruskal-wallis;fdr-correction;multivariate-analysis;pca;pls;opls-da`
- 许可：GPL-3.0-or-later
