# 弹性网络回归（Elastic Net）

## 引言

Lasso 能选出特征，但在特征高度相关时会「随手挑一个」丢进模型；Ridge 稳定，却永远不把系数压到零。**弹性网络**（Zou & Hastie, 2005）同时施加 L1 与 L2 惩罚，既保留稀疏性，又让相关特征**成组地**进出模型。

本包在 sciBASIC# 中实现弹性网络，目标函数与 sklearn / glmnet 的参数化一致：

```
1/(2n)·||y − Xb||² + alpha·( l1Ratio·||b||₁ + (1 − l1Ratio)/2·||b||₂² )
```

其中截距 `c` 不参与惩罚。`l1Ratio = 1` 退化为纯 Lasso，`l1Ratio = 0` 退化为纯 Ridge。

## 设计目标

- **与主流实现同源**：坐标下降 + 软阈值 + 残差增量更新，与 sklearn 的 `ElasticNet` / glmnet 的算法路径一致，参数含义可直接对照；
- **尺度无关**：中心化 + 标准化（总体标准差，`ddof = 0`），惩罚力度与样本量、特征量纲解耦；
- **结果可编程**：拟合结果、正则化路径与交叉验证结果都是可枚举、可导出为 `NumericTable` 的模型对象，而不是一段打印文本。

## 核心特性

- **坐标下降求解器**：逐特征更新 `b_j = soft(ρ_j, alpha·l1Ratio) / (s_j + alpha·(1 − l1Ratio))`，残差 `r` 增量维护，常数特征列自动跳过（其作用由截距承担）；
- **正则化路径**：alpha 由 `alpha_max`（使全部系数为零的临界值）按比例递减，**warm start** 复用上一步的解，因此路径比逐点独立拟合快得多；
- **交叉验证**：在 `l1Ratio × alpha` 的二维网格上做确定性 K 折交叉验证。每个 `l1Ratio` 拥有各自的 alpha 网格（因为 L1 强度改变时 alpha 上界也改变），折外均方误差最小者胜出，最后在全量数据上重新拟合；
- **无泄漏的标准化**：每一折只用自己训练集的均值与标准差做中心化/标准化；
- **表入口**：`NumericTable` 扩展方法可直接把带列名的数据表喂进模型，预测值可写回标签矩阵。

## 命名空间地图

| 命名空间 | 职责 |
|---|---|
| `Microsoft.VisualBasic.Data.Bootstrapping.ElasticNet` | 求解器、拟合结果、路径、交叉验证与表入口（本包全部类型） |

## 关键类型与 API

- `ElasticNetFitting` —— 求解器模块：
  - `Fit(x, y, alpha, l1Ratio, standardize, tolerance, maxIterations, featureNames)`；
  - `FitColumns(columns, y, ...)` —— 按列优先（column-major）组织的数据；
  - `RegularizationPath(x, y, l1Ratio, numberOfAlphas, alphaRatio, alphaMax, ...)`；
  - `RegularizationPathColumns(...)`、`CrossValidate(x, y, l1Ratios, numberOfAlphas, folds, ...)`。
- `ElasticNetFit` —— 单次拟合结果：`Alpha`、`L1Ratio`、`Intercept`、`Coefficients`、`FeatureNames`、`R2`、`SSE`、`SST`、`NonZeroCount`、`Iterations`、`Converged`，以及 `GetY(x)`、`Polynomial`、`toNumericTable()`；
- `ElasticNetPath` —— 一条正则化路径上的全部解：`Alphas`、`Intercepts`、`Coefficients`、`RSquared`、`NonZeroCounts`、`getCoefficients(idx)`、`toNumericTable()`；
- `ElasticNetCVResult` —— 交叉验证结果：`L1Ratios`、`Alphas`、`MSE`（锯齿数组，每个 `l1Ratio` 一套网格）、`Folds`，以及 `BestL1Ratio`、`BestAlpha`、`BestMSE`、`Fit`；
- `ElasticNetPolynomial` —— 带截距的多元线性形式 `y = c + b1·x1 + ...`，因为既有的 `MultivariatePolynomial` 不含截距；
- `NumericTableElasticNet` —— 表入口扩展：`ElasticNetRegression(table, alpha, l1Ratio, ...)`、`ElasticNetRegularizationPath(table, l1Ratio, ...)`、`ElasticNetCrossValidate(table, l1Ratios, ...)`（`y :=` 指定标签列）。

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.Data.Bootstrapping.ElasticNet

' 单点拟合
Dim fit As ElasticNetFit = ElasticNetFitting.Fit(x, y,
                                                alpha:=0.1,
                                                l1Ratio:=0.5,
                                                featureNames:={"x1", "x2", "x3"})

Console.WriteLine($"R² = {fit.R2:F4}, 非零系数 = {fit.NonZeroCount}")
Console.WriteLine(fit.Polynomial.ToString({"x1", "x2", "x3"}, "F4"))

' 正则化路径（alpha 由大到小，warm start）
Dim path As ElasticNetPath = ElasticNetFitting.RegularizationPath(x, y,
                                                                 l1Ratio:=0.7,
                                                                 numberOfAlphas:=100)

' 交叉验证选参：默认 l1Ratio 网格 {0.1, 0.5, 0.7, 0.9, 0.95, 0.99, 1}
Dim cv As ElasticNetCVResult = ElasticNetFitting.CrossValidate(x, y, folds:=5)

Console.WriteLine($"最优 l1Ratio = {cv.BestL1Ratio}, alpha = {cv.BestAlpha}")
```

直接对数据表建模：

```vbnet
Imports Microsoft.VisualBasic.Data.Bootstrapping.ElasticNet

Dim cv As ElasticNetCVResult = table.ElasticNetCrossValidate(y:="response", folds:=10)
Dim best As ElasticNetFit = cv.Fit

Dim path As NumericTable = ElasticNetFitting.
    RegularizationPath(table.Columns(...), table("response"), l1Ratio:=0.9).
    toNumericTable()
```

## 实现要点

- **alpha 上界**：`alpha_max = max_j |x_jᵀy| / (n · l1Ratio)`，在该点全部系数为零，这是 KKT 条件的直接结果，也是路径的起点；
- **成组效应**：完全相同的两列特征在弹性网络中会得到**相等**的系数（L2 项让解唯一）。这条性质很适合做单元测试，但需要把 `tolerance` 收紧到 `1e-12`——默认的 `1e-4` 会留下大约 `5e-4` 的坐标间不平衡；
- **与 Ridge 的换算**：本模块的标准化使用总体标准差（`ddof = n`），而 `DataFittings` 的 Ridge 使用样本标准差（`ddof = n − 1`），因此对拍时需要把惩罚系数换算为 `lambda·(n − 1)/n`；
- **确定性**：折划分采用交错分配 `foldId(i) = i Mod k`（等价于 sklearn 不洗牌的 `KFold`），不使用随机数，结果可逐位复现。

更完整的算法推导、数值细节与自检用例见同目录下的 `elastic.md`，可运行的自检工程见 `_smoketest/`。

## 包信息

- Assembly：`Microsoft.VisualBasic.Data.Bootstrapping.ElasticNet`
- TargetFramework：`net10.0`
- Tags：`scibasic;elastic-net;regularization;lasso;ridge;coordinate-descent;cross-validation;feature-selection`
- 许可：GPL-3.0-or-later
