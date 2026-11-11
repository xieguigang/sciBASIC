# 曲线拟合与回归建模库

## 引言

「用一条曲线描述数据」这件事，随模型复杂度不同需要完全不同的工具：

- 模型对参数**线性**时 → 线性最小二乘（闭式解）；
- 响应变量是**非负整数计数**时 → 泊松回归（对数链接 GLM）；
- 响应是**生存时间 + 事件状态**（右删失）时 → Cox 比例风险模型；
- 需要**同时预测多个定量响应**时 → 多响应高斯模型（组-lasso，共享变量集）；
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
| `...Data.Bootstrapping.MGaussian` | 多响应高斯回归（组-lasso / 多任务学习） | N×K 响应矩阵、共享变量集、λ 路径与 K 折交叉验证 |
| `...Data.Bootstrapping.Cox` | Cox 比例风险回归（生存分析） | 右删失数据、风险比、C 指数、基线生存曲线与 CV |

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

' 7. 多响应高斯回归：全部标签列组成 N x K 响应矩阵，组-lasso 让变量同进同出
Dim mg = inputs.MGaussianRegression(numberOfLambdas:=50, standardizeResponse:=True)
Console.WriteLine($"K={mg.NumberOfResponses}, df={mg.df(mg.df.Length - 1)}")
'    2-norm 系数路径（对应 plot(type.coef="2norm")）
Dim mgPath = mg.toNumericTable()
'    单 lambda 的 N x K 预测，以及多 lambda 的 (step)(sample)(response) 三维预测
Dim mgFit = mg.Predict(newx)
Dim mgFit3D = mg.PredictPath(newx, {0, 24})
'    K 折交叉验证自动选 lambda.min / lambda.1se
Dim mgCV = inputs.MGaussianCV(nfolds:=10, numberOfLambdas:=50)
Console.WriteLine($"lambda.min={mgCV.lambdaMin:G4}, lambda.1se={mgCV.lambda1se:G4}")
'    多响应模型不是 IFitted；取逐响应视图后可复用 SetPrediction 写回
Dim mgView = mg.GetResponseModel(0)

' 8. Cox 比例风险回归：标签列 time（观测时间）+ status（1=事件、0=删失）
Dim cox = inputs.CoxRegression(numberOfLambdas:=50, ties:=CoxTies.Efron)
Console.WriteLine($"events={cox.eventCount}, C={cox.cIndex(cox.cIndex.Length - 1):F4}")
'    Cox 没有截距；风险比 HR = exp(beta)，风险得分 = x'beta
Dim hr = cox.getHazardRatios(49)
Dim riskScore = cox.Predict(newx)
'    Breslow 基线累积风险与个体化生存曲线（不提供置信带）
Dim surv = cox.PredictSurvival(New Double()() {newx(0)})
'    K 折交叉验证：deviance 越小越好，CIndex 越大越好
Dim coxCV = inputs.CoxCV(nfolds:=10, measure:=CoxCVMeasure.CIndex)
Console.WriteLine($"lambda.min={coxCV.lambdaMin:G4}, lambda.1se={coxCV.lambda1se:G4}")
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
- **mgaussian 的稀疏粒度是「变量」而不是「系数」**：惩罚作用在每个变量的 K 维系数向量的 `l2` 范数上，因此变量对全部响应同进同出（shared sparsity）；`alpha=1` 是组-lasso，`alpha=0` 退化为岭型收缩（无精确零）。若各响应的重要变量差异很大，逐响应单独拟合反而更合适。
- **`standardize.response` 是 mgaussian 特有参数**：响应各列量纲悬殊时开启，避免大方差响应主导损失与系数收缩；拟合在标准化空间进行，预测自动还原到原始量纲。CV 的每一折只用自己的训练集统计量做标准化，避免信息泄漏。
- **多 lambda 预测是三维结构**：`Predict` 返回 `N x K`，`PredictPath` 返回 `(lambda 步骤)(样本)(响应)`；下游代码需注意索引顺序。多响应结果不实现单响应的 `IFitted`，可用 `GetResponseModel(k)` 取逐响应视图复用 `SetPrediction`。
- **CV 的 1-SE 规则更保守**：`lambda.min` 取折外 `mse/mae`（或 deviance）最小处，`lambda.1se` 取满足 `cvMean <= cvMean(min) + cvSe(min)` 的最大 lambda，牺牲一点误差换取更稀疏、更稳定的模型。
- **Cox 估计的是偏似然而不是完全似然**：右删失数据下基线风险 `h0(t)` 不设参数形式，截距被吸收进去，因此 `coef` 不含 a0；风险集按观测时间降序累加，`status` 的语义必须是 `1=事件、0=删失`。
- **平局与惩罚因子**：同一时刻多个事件时 `CoxTies.Efron`（缺省）比 `Breslow` 偏差更小，无平局时两者一致；`penaltyFactor(j)=0` 可强制保留核心临床变量，内部会重缩放至和为 p。
- **HR 与 C 指数方向**：变量效应用 `exp(beta)`（风险比）报告；Cox 的 CV 可用 `deviance`（越小越好）或 `CIndex`（Harrell C，越大越好），`CoxCVFit.Maximize` 指明方向，不要套用「最低点最优」的惯性。
- **生存曲线与 IFitted 语义**：`BaselineHazard()` / `PredictSurvival(newx)` 用 Breslow 估计量给出 `S(t|x)=exp(-H0(t)*exp(x'beta))`，不带置信带。`CoxFit` 虽实现 `IFitted`，但 `GetY` 是风险得分、`R2` 是伪 R²、`ErrorTest` 为 `Nothing`，写回预测时不要开启残差列。

## 包信息

- Assembly：`Microsoft.VisualBasic.Data.Bootstrapping`
- TargetFramework：`net10.0`
- Tags：`scibasic;curve-fitting;regression;linear-least-squares;lasso;loess;logistic-regression;levenberg-marquardt;poisson;mgaussian;group-lasso;cox;survival`
- 许可：GPL-3.0-or-later
