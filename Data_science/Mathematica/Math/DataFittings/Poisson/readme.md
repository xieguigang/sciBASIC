泊松回归是广义线性模型（GLM）的一种特例，专门用于对非负整数计数型响应变量进行建模，通过对数链接函数将解释变量的线性组合与事件发生率的期望值联系起来。下面从原理、适用条件、模型对比、参数解释、常见问题到代码实现，做系统梳理。
## 一、模型原理与数学形式
泊松回归建立在泊松分布之上。泊松分布描述的是在固定时间或空间内，独立事件发生次数的概率，其概率质量函数为：
$$P(Y = y \mid \mu) = \frac{e^{-\mu}\,\mu^{y}}{y!}, \quad y = 0, 1, 2, \dots$$
其中 $\mu$ 既是分布的均值也是方差，这一性质称为**等散性**。
泊松回归的核心，是把期望计数 $\mu_i$ 与协变量 $x_{i1},\dots,x_{ip}$ 通过对数链接函数联系起来：
$$\log \mu_i = \beta_0 + \beta_1 x_{i1} + \dots + \beta_p x_{ip}$$
等价地：
$$\mu_i = \exp\!\big(\beta_0 + \beta_1 x_{i1} + \dots + \beta_p x_{ip}\big)$$
之所以使用对数链接，是因为 $\mu$ 必须为正，而对数变换把取值域从 $\mathbb{R}$ 映射到 $(0,+\infty)$，从而保证预测的期望计数恒为正值。这一模型也被称为**对数线性模型**（log-linear model），在处理列联表时常以此称呼。
当观测的暴露量（exposure）不同时——比如不同地区人口数、不同随访人年——需要引入 **offset 项**对计数进行标准化：
$$\log \mu_i = X_i\beta + \log(t_i)$$
其中 $t_i$ 为暴露量。此时模型实际拟合的是事件发生率而非原始计数。
## 二、适用条件
泊松回归在以下假设成立时表现最佳：
- **响应变量为非负整数计数**（0, 1, 2, …），如某时段内的事故数、某区域内的物种数、每天的客户投诉数等。
- **事件相互独立**：一次事件的发生不影响另一次发生的概率。
- **发生率相对恒定**：在相似条件下，事件平均发生率稳定。
- **均值≈方差（等散性）**：这是泊松分布的关键假设，也是实际中最易被违反的条件。
- **时间或空间框架明确**：计数必须有清晰定义的时段或区域。
- **无过度零膨胀**：若零的比例远超泊松分布预期，需考虑零膨胀模型。
## 三、与线性回归、逻辑回归的对比
作为 GLM 家族的一员，泊松回归与线性回归、逻辑回归同出一脉，但分别针对不同类型的响应变量：
| 维度 | 线性回归 | 逻辑回归 | 泊松回归 |
|---|---|---|---|
| 响应变量类型 | 连续（近似正态） | 二分类 0/1 或比例 | 非负整数计数 |
| 分布假设 | 正态分布 | 二项分布 | 泊松分布 |
| 链接函数 | 恒等 | Logit：$\log\frac{\mu}{1-\mu}$ | 对数：$\log\mu$ |
| 均值-方差关系 | 方差恒定 | $\mu(1-\mu)$ | 均值=方差 |
| 典型应用 | 身高、温度预测 | 疾病有无、是否违约 | 事故次数、索赔数、点击量 |
## 四、参数估计与系数解释
参数 $\beta$ 通常采用**极大似然估计（MLE）**。给定 $n$ 个独立观测，对数似然函数为：
$$\ell(\beta) = \sum_{i=1}^{n}\Big[ y_i\,(X_i\beta) - \exp(X_i\beta) - \log(y_i!) \Big]$$
该对数似然函数为凸函数，但无解析解，需通过 **Newton–Raphson** 或 **迭代加权最小二乘（IWLS）** 等数值方法迭代求解至收敛。
**系数解释**采用乘法效应视角。对第 $j$ 个解释变量，系数 $\beta_j$ 的指数化 $e^{\beta_j}$ 表示该变量每增加一个单位，期望计数乘以的倍数，称为**发生率比**。
例如，若某营销投入变量的系数 $\beta = 0.1$，则 $e^{0.1}\approx 1.105$，意味着该变量每增加 1 单位，期望计数提升约 10.5%。若 $\beta = 0.5$，则 $e^{0.5}\approx 1.65$，对应期望计数增加约 65%。
## 五、常见问题与扩展模型
### 1. 过度离散
等散性假设（方差=均值）在实际数据中常被违反，方差往往大于均值，称为**过度离散**。此时泊松回归的标准误被低估，导致 Z 统计量偏大、p 值偏小，容易错误拒绝原假设。
**诊断方法**：计算残差偏差与自由度之比，若该比值远大于 1（如 >1.5），提示过离散。
**应对方案**：
- **准泊松回归**：引入一个离散参数 $\phi$ 修正标准误，不改变系数估计，仅调整推断。
- **负二项回归**：在泊松分布基础上增加一个离散参数 $\alpha$，显式刻画方差 $Var(Y)=\mu+\alpha\mu^2$，能更好拟合过离散计数数据，被认为是过离散的首选模型。
### 2. 零膨胀
当数据中零的比例远超泊松模型预测时，称为**零膨胀**，常见于保险索赔、就医次数等场景。
**应对方案**：
- **零膨胀泊松模型（ZIP）**：混合一个伯努利过程（决定"是否发生"）和一个泊松过程（决定"发生多少次"），由 `pscl::zeroinfl()` 实现。
- **零膨胀负二项模型（ZINB）**：在过离散与零膨胀同时存在时使用。
- **Hurdle 模型**：另一种处理过多零的两部分模型，由 `pscl::hurdle()` 实现。
### 3. 欠离散
方差小于均值时称为欠离散，相对少见，可通过广义泊松回归或 COM-Poisson 模型处理。
### 4. 模型选择思路
简明的决策路径如下：
- 计数数据 → 先拟合泊松回归
- 检查残差偏差/自由度 → 若比值≈1，模型可用
- 比值远大于 1 → 改用负二项回归或准泊松回归
- 零值过多 → 改用 ZIP 或 ZINB
## 六、R 语言代码示例
R 中泊松回归通过 `glm()` 函数实现，将 `family` 参数设为 `poisson` 即可：
```r
# 基本泊松回归
model_pois <- glm(count ~ x1 + x2, family = poisson(link = "log"), data = mydata)
summary(model_pois)
# 带暴露量 offset（如人年 log(person_years)）
model_offset <- glm(events ~ age + gender,
                    offset = log(exposure),
                    family = poisson, data = mydata)
# 检查过离散
dispersion_ratio <- model_pois$deviance / model_pois$df.residual
print(dispersion_ratio)  # 远大于1提示过离散
# 准泊松回归（仅修正标准误）
model_quasi <- glm(count ~ x1 + x2, family = quasipoisson, data = mydata)
# 负二项回归（需 MASS 包）
library(MASS)
model_nb <- glm.nb(count ~ x1 + x2, data = mydata)
# 零膨胀泊松回归（需 pscl 包）
library(pscl)
model_zip <- zeroinfl(count ~ x1 | x2, data = mydata, dist = "poisson")
# 指数化系数得到发生率比 IRR
exp(coef(model_pois))
```
以 R 内置的 `warpbreaks` 数据集为例，该数据记录织布过程中纱线断裂次数与羊毛类型、张力水平的关系，是典型的计数数据案例，适合用泊松回归或负二项回归建模。
## 七、简要总结
泊松回归通过泊松分布建模计数型响应变量，借助对数链接函数将协变量与期望计数线性关联，并使用极大似然估计参数；它适用于均值近似等于方差的非负整数计数场景。实际应用中，等散性假设常被违反，需通过诊断比检验决定是否转向准泊松、负二项回归或零膨胀模型，以保证推断的可靠性。
## 八、本库实现与调用示例（VB.NET）

`linear-netcore5.vbproj` 中的 `Microsoft.VisualBasic.Data.Bootstrapping.Poisson` 命名空间实现了上文的标准泊松回归与诊断能力：对数链接、IRLS / Newton-Raphson 极大似然估计、暴露量 offset、系数标准误与 Wald Z 值、发生率比 IRR、残差偏差与离散比。结果对象 `PoissonFit` 实现了 `IFitted`，可与 `NumericTable` 的统一入口及预测写回联动。

```vbnet
Imports Microsoft.VisualBasic.Data
Imports Microsoft.VisualBasic.Data.Bootstrapping
Imports Microsoft.VisualBasic.Data.Bootstrapping.Poisson

' 1) 直接以矩阵拟合：自动补截距，y 必须为非负整数计数
'    exposure 传入原始暴露量 t（必须全为正），库内部取 log(t) 作为 offset
Dim fit As PoissonFit = PoissonFitting.Fit(x, y, exposure:=personYears)

Console.WriteLine(fit.ToString())
Console.WriteLine($"IRR(x1) = {fit.IncidenceRateRatios(1):F4}")   ' exp(beta_1)
Console.WriteLine($"SE(x1)  = {fit.StandardErrors(1):F4}, Z = {fit.ZValues(1):F4}")
Console.WriteLine($"D/df    = {fit.DispersionRatio:F4}, over-dispersed = {fit.IsOverDispersed}")
Console.WriteLine($"pseudoR2 = {fit.PseudoR2:F4}, converged = {fit.Converged}")

' 2) 或从统一的二维表入口拟合：全部特征列作为 X，y 指定计数标签列
Dim model As PoissonFit = table.PoissonRegression(y:="count", exposure:="person_years")

Console.WriteLine(model.GetY(1.0, 0.5))                    ' 单位暴露量之下的事件率
Console.WriteLine(model.Predict({1.0, 0.5}, exposure:=2.0)) ' 乘上暴露量后的期望计数
```

结果解读要点：

- `IncidenceRateRatios(j) = exp(beta_j)`：第 j 个变量每增加 1 个单位，期望计数乘以的倍数；截距对应的是基准期望计数。
- `DispersionRatio = Deviance / ResidualDf`：接近 1 才符合等散性，远大于 1（经验上 >1.5）时 `IsOverDispersed` 为真，标准误会被低估、Z 值偏大。
- `R2` / `PseudoR2` 是基于偏差的伪 R² `1 - Deviance / NullDeviance`（裁剪到 `[0,1]`），不是普通最小二乘的决定系数。
- 带 offset 时 `GetY` 与 `SetPrediction` 写回的是**单位暴露量的事件率**，`FittedValues` / `Predict(x, exposure)` 才是含实际暴露量的期望计数。
- 本库只覆盖标准泊松回归及其诊断，**不实现**准泊松、负二项回归、ZIP / ZINB 与 Hurdle 模型；诊断提示过离散或零膨胀时应改用相应的扩展模型。