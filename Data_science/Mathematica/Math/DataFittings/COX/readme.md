# glmnet 中的 Cox 比例风险模型（`family = "cox"`）
**Cox 模型（`family = "cox"`）是 glmnet 中用于"生存时间 + 是否发生事件"这类右删失数据的正则化回归，它在经典 Cox 偏似然的负对数上施加弹性网惩罚，从而在高维协变量下同时完成变量筛选与风险建模，是临床预后研究和基因生存数据中最常用的 family 之一**。
## 一、模型背景与数学形式
### 1. Cox 比例风险模型
生存数据的标准形式为 $(y_1, x_1, \delta_1), \ldots, (y_n, x_n, \delta_n)$，其中 $y_i$ 为观测时间（$\delta_i = 1$ 表示这是失败/事件时间，$\delta_i = 0$ 表示删失时间），$x_i$ 是 $p$ 维协变量向量。Cox 模型假设风险函数具有半参数形式：
$$h_i(t) = h_0(t)\, e^{x_i^T \beta}$$
其中 $h_0(t)$ 是所有个体共享的基线风险（不设定参数形式），$\beta$ 是待估的 $p$ 维系数向量。由于删失的存在，估计 $\beta$ 不使用完全似然，而是**偏似然**（partial likelihood）——设 $t_1 < t_2 < \cdots < t_m$ 为各不相同的失败时间，$R_i$ 为在 $t_i$ 时刻仍处于风险中的个体集合，则：
$$L(\beta) = \prod_{i=1}^{m} \frac{e^{x_{j(i)}^T \beta}}{\sum_{j \in R_i} e^{x_j^T \beta}}$$
直观上，这要求"在每个事件时刻，真正发生事件者应比当时仍在风险集中的其他个体有更高的风险得分"。
**注意：Cox 模型没有截距项**。截距被吸收进基线风险 $h_0(t)$ 中，在偏似然里会相互抵消，因此 glmnet 返回的系数矩阵不含 a0。
### 2. glmnet 的正则化目标
glmnet 对偏似然的负对数施加弹性网惩罚（Simon et al., 2011）：
$$\min_{\beta} \; -\ell(\beta) + \lambda \left[ \frac{1-\alpha}{2} \|\beta\|_2^2 + \alpha \|\beta\|_1 \right]$$
- $\alpha = 1$：lasso（稀疏变量选择，生存分析最常用）；
- $\alpha = 0$：ridge（仅收缩，适合高度相关协变量，不产生稀疏解）；
- $0 < \alpha < 1$：弹性网，兼顾稀疏性与分组效应，在高维基因数据（强相关基因成簇）中常优于纯 lasso。
优化算法为循环坐标下降配合 warm starts，沿 $\lambda$ 路径逐点求解。
## 二、数据格式要求
`y` 必须是生存响应对象，推荐用 survival 包的 `Surv()` 构造，也可直接给两列命名矩阵：
```r
library(glmnet)
library(survival)
data(CoxExample)
x <- CoxExample$x      # n x p 协变量矩阵
y <- CoxExample$y      # n x 2 矩阵：第1列 "time"，第2列 "status"（1=事件，0=删失）
# 等价写法：y <- Surv(time, status)   # 推荐，可自然扩展到 start-stop 数据
```
若 `y` 是自行构造的两列矩阵，列名必须严格为 `"time"` 和 `"status"`。
## 三、拟合与关键参数
### 1. 基本拟合
```r
fit <- glmnet(x, y, family = "cox")
plot(fit)                                # 系数路径图
coef(fit, s = 0.05)                      # 指定 lambda 处的稀疏系数（无截距）
```
`alpha`、`weights`、`nlambda`、`standardize` 等参数的用法与其他 family 一致。
### 2. 平局处理：`cox.ties`（Breslow vs Efron）
当多个个体在完全相同的时刻发生事件（平局，ties）时，偏似然需做近似。glmnet 支持两种方法，由 `cox.ties` 参数选择：
| 方法 | 特点 |
|---|---|
| `"breslow"` | 计算简单，把平局事件当作独立处理；glmnet v5.0 及更早版本的默认 |
| `"efron"` | 对平局的处理更精细，偏差更小；`survival::coxph` 的默认，**glmnet v5.1 起将改为默认** |
在 v5.0 与 v5.1 过渡期间，未显式指定 `cox.ties` 的调用会发出过渡警告，显式传入即可消除。若关心与 `coxph` 结果的一致性（例如想用 glmnet 做变量筛选后再交给 coxph 精修），应使用 `cox.ties = "efron"`：
```r
glmnet_fit <- glmnet(x, y, family = "cox", lambda = 0, cox.ties = "efron")
coxph_fit   <- coxph(y ~ x)
plot(coef(glmnet_fit), coef(coxph_fit)); abline(0, 1)   # 两者的系数应落在对角线上
```
### 3. 变量级惩罚：`penalty.factor`
对每个系数可设置单独的惩罚乘子（默认全为 1）。设为 0 表示该变量**不惩罚、永远保留在模型中**（例如临床分期、年龄等必须纳入的核心协变量）；设为较大值则更强力地收缩甚至剔除。惩罚因子内部会重新缩放至和为 p，lambda 序列会随之调整。这在"临床变量强制保留 + 高维基因变量参与筛选"的混合建模场景中非常实用。
## 四、交叉验证：`cv.glmnet` 与 C 指数
Cox 家族的 `cv.glmnet` 在两方面与其他 family 不同：
**第一，`type.measure` 只支持两种取值**：
- `"deviance"`（默认）：部分似然偏差（负对数偏似然），越低越好；
- `"C"`：Harrell 一致性指数，相当于生存数据版本的 AUC，**越高越好**，只统计"可比较对"（若一方的删失时间早于另一方的事件时间，则该对不参与计算）。
```r
cvfit <- cv.glmnet(x, y, family = "cox", type.measure = "C")
plot(cvfit)
cvfit$lambda.min    # C 指数最大处的 lambda
cvfit$lambda.1se    # 最正则化且 CV 误差在最优值 1 个标准误之内的 lambda
```
实践中 `type.measure = "C"` 的曲线往往较平坦、波动大，导致 `lambda.min` 与 `lambda.1se` 差异悬殊；此时改用 `"deviance"` 通常能得到更稳定、更"阶梯式"的 CV 曲线，两种指标下选出的最优 lambda 也可能明显不同，需结合图形判断。
**第二，`grouped = TRUE`（默认）** 采用"减法"方式计算第 k 折的 CV 偏似然：用全数据的对数偏似然减去剔除第 k 折后 (K−1)/K 数据上的对数偏似然。这能更高效地利用风险集。`grouped = FALSE` 则仅在第 k 折内部计算偏似然，只在每折样本量足够大时才合理。
C 指数也可以在训练集上直接对每条 lambda 路径计算，用于观察判别能力随正则化强度的变化：
```r
pred <- predict(fit, newx = x)
apply(pred, 2, Cindex, y = y)   # 每个 lambda 对应的 C 指数
```
## 五、预测与生存曲线
### 1. 预测风险得分
`predict()` 返回的是线性预测子 $x^T\beta$（风险得分的对数），得分越高代表风险越大：
```r
predict(fit, newx = x[1:5, ], s = 0.05, type = "link")
```
### 2. 个体生存曲线
`coxnet` 对象支持 `survival::survfit()` 方法，绘制指定 lambda 处的预测生存曲线。必须同时传入拟合所用的 `x` 和 `y`（用于估计基线风险）；若不传 `newx`，得到的是"协变量取全体均值的伪个体"的曲线，通常没有意义，因此一般应传入 `newx` 得到个体化曲线：
```r
sf <- survival::survfit(fit, s = 0.05, x = x, y = y, newx = x[1:3, ])
plot(sf)
```
`survfit` 也对 `cv.glmnet` 对象可用，默认使用 `lambda.1se`，也可指定 `s = "lambda.min"`。注意 glmnet 目前**无法给出这些生存曲线的标准误**，因此不带置信带。
## 六、扩展功能（v4.1 及以后）
glmnet 新版在经典右删失 Cox 模型之外，还支持三类常用扩展：
- **（start, stop] 区间数据（start-stop data）**：响应用 `Surv(start, stop, status)` 构造，进而支持时变协变量、时变分层、左删失、多重时间尺度、同一受试者多次事件以及 case-cohort 设计等复杂生存数据结构。设 `lambda = 0` 时其结果与 `coxph` 一致，可作正确性检验。
- **分层 Cox 模型**：各层有各自的基线风险、共享同一套协变量系数。需用 `stratifySurv()` 函数给 `Surv` 对象添加分层属性（直接用 `attr(y, "strata") <- ...` 不够，glmnet 会报错，因为取子集时分层信息会丢失）：
  ```r
  y2 <- stratifySurv(y, strata = group)
  fit <- glmnet(x, y2, family = "cox")
  ```
- **稀疏矩阵输入**：`x` 可为 `Matrix` 包的稀疏矩阵，在超宽组学数据中显著节省内存并加速计算。
## 七、完整可运行示例
```r
library(glmnet)
library(survival)
set.seed(1)
# ---- 1. 模拟右删失生存数据 ----
N <- 200; p <- 30
x <- matrix(rnorm(N * p), N, p)
beta <- c(rep(0.5, 5), rep(0, p - 5))        # 仅前 5 个变量有真实效应
ty    <- rexp(N, exp(x %*% beta / 3))        # 生存时间
tcens <- rbinom(N, 1, 0.3)                   # 0 为事件, 1 为删失
y <- Surv(ty, 1 - tcens)                     # status: 1=事件, 0=删失
# ---- 2. 拟合 lasso Cox 模型 ----
fit <- glmnet(x, y, family = "cox", alpha = 1)
plot(fit, label = TRUE)
# ---- 3. 交叉验证（C 指数）----
cvfit <- cv.glmnet(x, y, family = "cox", type.measure = "C")
plot(cvfit)
cvfit$lambda.min; cvfit$lambda.1se
# ---- 4. 提取选定变量与系数 ----
coef.1se <- coef(cvfit, s = "lambda.1se")
selected <- rownames(coef.1se)[which(coef.1se != 0)]
selected
exp(coef.1se[selected])                      # 对应的风险比 HR = exp(beta)
# ---- 5. 训练集 C 指数 & 生存曲线 ----
pred <- predict(cvfit, newx = x, s = "lambda.1se")
Cindex(drop(pred), y = y)
sf <- survival::survfit(cvfit, x = x, y = y, newx = x[1:3, ])
plot(sf)
```
## 八、适用场景与常见坑
**适用场景**：右侧删失的生存数据（临床随访、癌症预后、疾病进展时间），尤其是协变量数 p 接近或远超样本量 n 的高维组学数据；需要从成千上万个候选变量中筛出稳健的预后基因/特征集合的场合。
**常见易错点**：
- **y 格式错误**：两列矩阵的列名必须是 `"time"` 和 `"status"`；`status` 的取值语义是 1 = 事件、0 = 删失，与 `Surv()` 一致，不要弄反；
- **忽略平局处理差异**：与 `coxph`（默认 Efron）对比系数时若发现系统性偏差，多半是 glmnet 用了 Breslow（v5.0 默认）；显式设 `cox.ties = "efron"` 即可对齐；
- **混淆 C 指数方向**：C 指数越大越好，而 deviance 越小越好，解读 CV 图时不要套用"最低点最优"的惯性；
- **无截距项**：`coxnet` 对象的 `a0` 为 NULL，提取系数时不要期待有截距；
- **生存曲线缺 newx**：不传 `newx` 得到的是"均值伪个体"曲线，几乎没有解释价值，务必针对具体样本绘图；
- **风险比解释**：报告变量效应时应使用 $\exp(\beta_j)$（HR），而非系数本身；系数被正则化收缩后 HR 是有偏的，glmnet 输出更适合用于预测和变量筛选，若需要无偏的效应估计和置信区间，常用做法是把 glmnet 筛出的变量再交给 `coxph` 重新拟合。
## 小结
glmnet 的 Cox 家族把弹性网正则化引入偏似然框架，实现了高维生存数据的自动变量选择与风险预测；配合 `cox.ties` 处理平局、`penalty.factor` 强制保留临床协变量、`cv.glmnet` 的 C 指数评估以及 `survfit` 的个体化生存曲线，构成了一个覆盖"筛选—调参—评估—可视化"完整链条的生存分析工具，是 `survival::coxph` 在高维场景下的自然延伸。
## 九、本库实现与调用示例（VB.NET）

`linear-netcore5.vbproj` 中的 `Microsoft.VisualBasic.Data.Bootstrapping.Cox` 命名空间实现了上文描述的标准 Cox 家族能力：负对数偏似然 + 弹性网惩罚的循环坐标下降、Breslow / Efron 平局、变量级惩罚因子、自动 lambda 路径（warm start）、风险比 HR、Harrell C 指数、Breslow 基线累积风险与个体化生存曲线，以及 K 折交叉验证（deviance / C 指数与 `lambda.min` / `lambda.1se`）。

```vbnet
Imports System.Linq
Imports Microsoft.VisualBasic.Data
Imports Microsoft.VisualBasic.Data.Bootstrapping
Imports Microsoft.VisualBasic.Data.Bootstrapping.Cox

' 0) 把 x / y 装进 NumericTable：两个标签列分别是 time 与 status
Dim p As Integer = x(0).Length
Dim table As New NumericTable(x, Nothing, Nothing)
Call table.SetLabel("time", y.Select(Function(r) r(0)).ToArray())
Call table.SetLabel("status", y.Select(Function(r) r(1)).ToArray())

' 1) 直接以 N x 2 响应拟合：y(i) = {time, status}，status: 1=事件、0=删失
Dim fit As CoxFit = CoxFitting.Fit(
    x, y,
    numberOfLambdas:=100,
    alpha:=1.0,                      ' 1 = lasso；0 = ridge；0<alpha<1 = 弹性网
    ties:=CoxTies.Efron)             ' 与 survival::coxph 默认一致；Breslow 亦可

Console.WriteLine(fit.ToString())
Console.WriteLine($"events={fit.eventCount}, censored={fit.censoredCount}")

' penalty.factor：设为 0 的变量永不惩罚、始终保留（如临床分期）
Dim pf As Double() = Enumerable.Repeat(1.0, p).ToArray()
pf(0) = 0.0R
Dim forced As CoxFit = CoxFitting.Fit(x, y, penaltyFactor:=pf, numberOfLambdas:=50)

' 2) 系数与风险比：Cox 没有截距，coef 长度就是 p
Dim coef As Double() = fit.getCoefficients(50)     ' p 维
Dim hr As Double() = fit.getHazardRatios(50)       ' exp(beta)
Dim pathTable As NumericTable = fit.toNumericTable() ' 每步一行 + lambdas/df/deviance/cIndex

' 3) 风险得分预测：返回 x'beta，值越大风险越高
Dim risk As Double() = fit.Predict(newx)
Dim riskPath As Double()() = fit.PredictPath(newx, New Integer() {0, 50, 99})
'    CoxFit 实现 IFitted，GetY 即风险得分；写回时不要开残差列（无逐点残差）
Call table.SetPrediction(fit, name:="risk_score")

' 4) 基线风险与个体化生存曲线（Breslow 估计量，不带置信带）
Dim baseline As SurvivalCurve = fit.BaselineHazard()
Dim curves As SurvivalCurve() = fit.PredictSurvival(New Double()() {newx(0), newx(1)})
Console.WriteLine($"S(t|x)={curves(0).Survival(curves(0).Survival.Length - 1):F4}")

' 5) 统一二维表入口：标签列名缺省为 time / status，可覆盖
Dim model As CoxFit = table.CoxRegression(numberOfLambdas:=50)
Dim custom As CoxFit = table.CoxRegression(time:="followup", status:="event")

' 6) K 折交叉验证：deviance 越小越好，CIndex 越大越好
Dim cv As CoxCVFit = CoxCV.CrossValidate(
    x, y, nfolds:=10, numberOfLambdas:=100,
    measure:=CoxCVMeasure.CIndex, grouped:=True)
' 或：Dim cv = table.CoxCV(nfolds:=10, measure:=CoxCVMeasure.Deviance)

Console.WriteLine($"lambda.min={cv.lambdaMin:G4}, lambda.1se={cv.lambda1se:G4}")
Dim bestRisk As Double() = cv.Predict(newx)
Dim sparseRisk As Double() = cv.Predict(newx, use1se:=True)
Dim cvCurve As NumericTable = cv.toNumericTable()
```

结果解读与实现约定：

- **没有截距**：`getCoefficients` 长度等于 `p`，`Polynomial` 为无截距的 `MultivariatePolynomial`；`IFitted.GetY` 返回风险得分 `x'beta`，不是生存时间，`ErrorTest` 为 `Nothing`。
- **status 语义**：`1 = 事件`、`0 = 删失`，与 `Surv()` 一致；二维表入口缺省读取名为 `time` 与 `status` 的标签列，可用 `time` / `status` 参数覆盖列名。
- **风险比**：报告变量效应时用 `exp(beta_j)` 即 `hazardRatios`，而非系数本身；正则化收缩后的 HR 有偏，适合预测与变量筛选，需要无偏估计和置信区间时应把筛出的变量交给 `coxph` 重新拟合。
- **平局处理**：`CoxTies.Efron`（缺省）偏差更小、与 `coxph` 一致；`CoxTies.Breslow` 计算更简单。无平局数据时两者结果相同。
- **CV 方向不要弄反**：`CoxCVMeasure.Deviance` 取折外部分似然偏差最小处；`CoxCVMeasure.CIndex` 取 Harrell C 指数最大处。`CoxCVFit.Maximize` 会指出当前方向。`grouped=True` 用全数据对数偏似然减去训练子集对数偏似然（readme 的「减法」方式），`False` 则只在留出折内部计算。
- **生存曲线**：`BaselineHazard()` 给出 `H0(t)` 与 `S0(t)=exp(-H0(t))`，`PredictSurvival(newx)` 给出 `S(t|x)=exp(-H0(t)*exp(x'beta))`；必须传 `newx` 才有个体化意义，且不提供标准误/置信带。
- **未覆盖的扩展**：(start, stop] 区间/时变协变量数据、分层 Cox、稀疏矩阵输入、逐样本权重与图形绘制均不在本库实现范围内；`toNumericTable()` 导出的路径表供外部绘图使用。