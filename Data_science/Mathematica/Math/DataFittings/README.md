# 曲线拟合与回归建模库

## 引言

「用一条曲线描述数据」这件事，随模型复杂度不同需要完全不同的工具：

- 模型对参数**线性**时 → 线性最小二乘（闭式解）；
- 响应变量是**非负整数计数**时 → 泊松回归（对数链接 GLM）；
- 需要**稀疏解**时 → LASSO（L1 正则）；
- 需要**局部光滑**而非全局函数时 → LOESS / LOWESS；
- 模型对参数**非线性**时 → Levenberg-Marquardt 迭代。

本包把这些方法统一到一个包内，并共享同一套「数据 + 模型 → 参数 + 误差」的接口。

## 核心能力

| 命名空间 | 方法 | 适用场景 |
|---|---|---|
| （根） | 线性 / 多项式最小二乘、加权与**非负**最小二乘、多元线性回归 | 参数线性、有先验约束 |
| `...Data.Bootstrapping.LASSO` | L1 正则回归 | 特征多、需要特征选择 |
| `...Data.Bootstrapping.Ridge` | L2 正则回归（岭回归） | 自变量共线、需要稳定解 |
| `...Data.Bootstrapping.LevenbergMarquardt` | 阻尼最小二乘（LM） | 任意非线性模型函数 |
| `...Data.Bootstrapping.Multivariate` | 多元线性回归（MLR） | 多自变量线性拟合 |
| `...Data.Bootstrapping.Logistic` | 逻辑回归 | 二分类 / S 型曲线 |
| `...Data.Bootstrapping.Poisson` | 泊松回归（对数链接 GLM） | 非负整数计数、发生率比、暴露量 offset 与过度离散诊断 |

根命名空间还包含**贝叶斯**与**高斯-牛顿**求解器，以及 LOESS / LOWESS 局部回归。

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.Data.Bootstrapping
Imports Microsoft.VisualBasic.Data.Bootstrapping.LevenbergMarquardt

' 1. 线性 / 多项式拟合
Dim fit = Fitting.Polynomial(x, y, degree:=3)
Console.WriteLine($"R2={fit.RSquared}")

' 2. 非线性模型：自定义模型函数 + LM 求解
Dim model = Function(p, t) p(0) * Math.Exp(-p(1) * t)
Dim lm = LevenbergMarquardt.Solve(x, y, model, guess:={1.0, 1.0})

' 3. 稀疏回归（自动做特征选择）
Dim lasso = LASSO.Fit(design, response, lambda:=0.01)

' 4. 岭回归：L2 正则，自变量共线时仍能给出稳定解
'    不指定 lambda 时会在 lambda 网格上求解完整路径并用 GCV 挑出最优值
Dim ridge = inputs.RidgeRegression()               ' 或 RidgeRegression(lambda:=0.1)
Console.WriteLine($"best lambda={ridge.BestLambda}, R2={ridge.R2}")
Console.WriteLine(ridge.BestCoefficients.JoinBy(", "))
'    岭迹（正则化路径）可以直接导出成二维表
Dim trace = ridge.toNumericTable()

' 5. 非线性拟合（Levenberg-Marquardt）：三种入口
'    a) 不给出模型，拟合幂律模型 y = p0 * x1^p1 * x2^p2 ...
Dim power = inputs.NonLinearFit()
'    b) 给出模型委托
Dim mm = inputs.NonLinearFit(Function(p, x) p(0) * x(0) / (p(1) + x(0)), guess:={1.0, 1.0})
'    c) 给出公式字符串，自变量按列名绑定
Dim decay = inputs.NonLinearFit("a*exp(-b*x1)", {"a", "b"})
Console.WriteLine($"R2={decay.R2}, a={decay.Parameters("a")}, b={decay.Parameters("b")}")

' 6. 泊松回归：响应变量必须是非负整数计数，截距自动拟合
'    exposure 指定暴露量列（可省略），库内部取 log(t) 作为 offset
Dim pois = inputs.PoissonRegression(y:="count", exposure:="person_years")
Console.WriteLine($"IRR(x1)={pois.IncidenceRateRatios(1):F4}, D/df={pois.DispersionRatio:F4}")
Console.WriteLine($"pseudoR2={pois.R2:F4}, converged={pois.Converged}")
'    GetY 返回单位暴露量的事件率；Predict 才返回乘上暴露量后的期望计数
Dim eventRate = pois.GetY(1.0, 0.5)
Dim expectedCount = pois.Predict({1.0, 0.5}, exposure:=2.0)
```

## 实现要点

- **线性 vs 非线性是「对参数」而言**：`y = a·x² + b·x + c` 对参数是线性的（可用最小二乘闭式解），而 `y = a·e^(b·x)` 对 `b` 是非线性的（必须迭代）。这一区分决定了该用哪套求解器。
- **Levenberg-Marquardt 的直观理解**：它在「高斯-牛顿（快但可能发散）」与「梯度下降（稳但慢）」之间自适应切换——阻尼因子大时接近梯度下降，小时接近高斯-牛顿。
- **非负最小二乘的实用价值**：浓度、强度、丰度等物理量不能为负，加上非负约束可避免得到无物理意义的解。
- **LOESS 为什么不是「拟合一个函数」**：它对每个预测点只用邻域数据做局部加权回归，因此能刻画非常不规则的形状，代价是无法写出一个封闭的解析式。
- **岭回归解决的是共线性**：自变量高度相关时 `X'X` 接近奇异，最小二乘的系数会剧烈震荡甚至算不出来；加上 `n·λ·I` 之后正规方程恒为正定，代价是系数被向零收缩（有偏），用 GCV 折中偏差与方差来选 λ。
- **岭回归必须标准化**：L2 惩罚对每个系数一视同仁，量纲不同会让惩罚效果完全不同；截距则单独估计、不参与惩罚，否则模型会依赖于 y 的原点。
- **泊松回归用对数链接，而不是对 y 取对数**：`log(y) = a + b·x` 的最小二乘要求 y>0 且隐含对数正态误差，遇到 0 计数就失效；泊松回归直接对条件均值建模 `log(mu) = Xβ`，保留计数分布「均值=方差」的结构，并用 IRLS / Newton-Raphson 求极大似然解。
- **offset 不是普通自变量**：暴露量以 `log(t)` 作为系数固定为 1 的项进入线性预测子，模型拟合的是事件率 `mu/t`；调用方传原始暴露量 `t`（必须为正），库内部取对数。带 offset 时 `GetY` 返回单位暴露量的率，`Predict(x, exposure)` 与 `FittedValues` 才返回期望计数。
- **离散比是模型诊断，不是拟合优度**：`Deviance / df ≈ 1` 才符合等散性；远大于 1（经验上 >1.5）时标准误被低估，应转向负二项 / 准泊松，零值过多则考虑零膨胀模型。`IFitted.R2` 在这里是基于偏差的伪 R²（`1 − D/D_null`），不是普通决定系数。

## 包信息

- Assembly：`Microsoft.VisualBasic.Data.Bootstrapping`
- TargetFramework：`net10.0`
- Tags：`scibasic;curve-fitting;regression;linear-least-squares;lasso;loess;logistic-regression;levenberg-marquardt`
- 许可：GPL-3.0-or-later
