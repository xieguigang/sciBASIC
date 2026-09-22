# 统计工具箱：分布、假设检验与更多

## 引言

本包是 `sciBASIC#` 的**统计中枢**：分布建模、假设检验、描述统计与几类高级方法（ICA、RANSAC、Shapley 值）都集中在这里，供数据挖掘、机器学习与绘图包共同调用。

## 核心能力

### 概率分布（`Distributions`）

- 常见概率分布模型与密度函数；
- **参数估计**：矩估计（`MethodOfMoments`）与 L-矩估计（`LinearMoments`）。

### 假设检验（`Hypothesis`）

- 常用参数检验（t 检验、F 检验、卡方检验等）；
- **Fisher 精确检验**（`FishersExact`）：小样本列联表；
- **Mantel 检验**（`Mantel`）：两个距离矩阵之间的相关性检验。

### 描述统计（`MomentFunctions`）

均值、方差、偏度、峰度等矩统计量。

### 高级方法

| 命名空间 | 方法 | 用途 |
|---|---|---|
| `ShapleyValue` | Shapley 值归因 | 公平分配特征贡献 |
| `ShapleyValue.TreeShap` | TreeSHAP | 树模型预测的精确归因 |
| `FastICA` | 独立成分分析 | 盲源分离（如去除生理噪声） |
| `RANSAC` | 随机采样一致性 | 含离群点时的稳健模型拟合 |

## 命名空间地图

| 命名空间 | 职责 |
|---|---|
| `Microsoft.VisualBasic.Math.Statistics`（根） | 统计入口与共享类型 |
| `....Statistics.Distributions`（+ `LinearMoments` / `MethodOfMoments`） | 分布与参数估计 |
| `....Statistics.HypothesisTesting`（+ `FishersExact` / `MantelTest`） | 假设检验 |
| `....Statistics.MomentFunctions` | 矩统计量 |
| `....Statistics.ShapleyValue`（+ `TreeShap`） | Shapley 值归因 |
| `....Statistics.RANSAC` | 稳健拟合 |

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.Math.Statistics

' 1. 描述统计
Dim moments = MomentFunctions.Compute(values)

' 2. 假设检验（如 t 检验 / Fisher 精确检验）
Dim t = HypothesisTesting.TTest(groupA, groupB)
Dim fisher = FishersExact.Test(table2x2)

' 3. 分布参数估计
Dim fit = Distributions.MethodOfMoments.Fit(values, DistributionKind.Normal)

' 4. Shapley 值归因（TreeSHAP 面向树模型）
Dim shap = ShapleyValue.TreeShap.Explain(treeModel, instance)
```

## 实现要点

- **为什么需要多种参数估计方法**：矩估计简单但受离群点影响大；L-矩（线性矩）基于有序统计量，对小样本与重尾分布更稳健——这正是水文学、极值分析等领域偏爱它的原因。
- **参数检验 vs 精确检验**：卡方检验依赖大样本近似；当期望频数过小时必须改用 Fisher 精确检验。
- **Shapley 值的代价**：原始定义需要枚举全部特征子集（指数级），因此实践中使用 TreeSHAP 这类针对树模型的多项式算法。
- **FastICA 与 PCA 的分工**：PCA 找的是**不相关**的成分（二阶统计量），FastICA 找的是**统计独立**的成分（高阶统计量）；后者更适合「多个独立信号线性混合」的场景。

## 包信息

- Assembly：`Microsoft.VisualBasic.Math.Statistics`
- TargetFramework：`net10.0`
- Tags：`scibasic;statistics;hypothesis-testing;distribution;moment-functions;fastica;ransac;shapley-value;treeshap`
- 许可：GPL-3.0-or-later
