# glmnet 中的多响应高斯模型（`family = "mgaussian"`）
**多响应高斯模型（multi-response Gaussian，即 `family = "mgaussian"`）是 glmnet 中用于同时预测多个定量响应变量的正则化线性回归模型，其核心特点是对每个自变量的 K 维系数向量施加组-lasso（group-lasso）惩罚，使变量要么对所有响应都保留、要么对所有响应都被剔除，这一机制被称为"多任务学习"（multi-task learning）**。
## 一、模型定义与数学形式
设响应 $y$ 是一个 $N \times K$ 的矩阵（而非单响应情形下的向量），glmnet 求解如下优化问题：
$$\min_{(\beta_0, \beta) \in \mathbb{R}^{(p+1)\times K}} \frac{1}{2N} \sum_{i=1}^{N} \|y_i - \beta_0 - \beta^T x_i\|_F^2 + \lambda \left[ (1-\alpha)\|\beta\|_F^2/2 + \alpha \sum_{j=1}^{p} \|\beta_j\|_2 \right]$$
其中：
- $\beta$ 是 $p \times K$ 的系数矩阵，$\beta_j$ 表示其中第 $j$ 行，即第 $j$ 个自变量在所有 $K$ 个响应上的系数向量；
- $\|\cdot\|_F$ 为 Frobenius 范数，$\|\cdot\|_2$ 为 $\ell_2$ 范数；
- $\alpha$ 与单响应弹性网一致：$\alpha = 1$ 为组-lasso 惩罚（最常用默认），$\alpha = 0$ 退化为对整个系数矩阵的岭回归型二次惩罚，$0 < \alpha < 1$ 为两者混合。
关键差别在于惩罚项：单响应 lasso 对**每个系数**施加 $\ell_1$ 惩罚（$|\beta_{jk}|$），而 mgaussian 将其替换为对**每个变量的系数向量** $\beta_j$（一个 $K$ 维向量）施加 $\ell_2$ 范数惩罚 $\|\beta_j\|_2$。组-lasso 的行为类似 lasso，但作用在变量的整组系数上：这些系数要么全为零，要么全不为零，仅随 $\lambda$ 被整体收缩。
## 二、与单响应 Gaussian 模型及多次单独拟合的对比
| 维度 | `family="gaussian"`（单响应） | 多次单独拟合 Gaussian | `family="mgaussian"`（多响应） |
|---|---|---|---|
| 响应格式 | 长度为 $N$ 的向量 | 需循环 $K$ 次 | $N \times K$ 矩阵 |
| 惩罚形式 | 每个系数的 $\ell_1$（lasso） | 各响应独立选择变量 | 每变量系数向量的 $\ell_2$ 组-lasso |
| 变量选择粒度 | 单个系数 | 各响应可得到不同变量集 | 变量对全部响应同进同出 |
| 是否利用响应间相关性 | 不适用 | 完全忽略 | 是（联合收缩） |
| 系数输出 | 稀疏向量 | 稀疏向量列表 | 稀疏矩阵（每个 $\lambda$ 一个） |
这种"共享支持集"（shared sparsity）的设计，特别适合各响应共享同一批底层解释变量的场景，例如多通道信号去卷积、多性状基因关联分析等。
## 三、R 代码实操
### 1. 基本拟合
```r
library(glmnet)
data(MultiGaussianExample)
x <- MultiGaussianExample$x
y <- MultiGaussianExample$y   # y 为 N x K 矩阵
mfit <- glmnet(x, y, family = "mgaussian")
```
参数 `alpha`、`lambda`、`weights`、`standardize`、`intercept` 等用法与单响应 Gaussian 相同。
### 2. mgaussian 特有参数：`standardize.response`
该参数仅对 mgaussian 家族有效，默认 `FALSE`。设为 `TRUE` 时，响应矩阵的各列会在拟合前被标准化（均值 0、方差 1）。当各响应变量量纲差异悬殊时，开启此项可避免某些响应在损失函数中主导系数估计。
### 3. 提取系数
系数在每个 $\lambda$ 处为一个稀疏矩阵（dgCMatrix），行对应自变量（含截距），列对应响应：
```r
coef(mfit, s = 0.1)   # 返回稀疏矩阵，维度为 (p+1) x K
```
### 4. 可视化系数路径
绘图时需指定 `type.coef`，两种取值代表不同展示视角：
```r
# 方式一：每个变量画一条曲线，取值为该变量 K 维系数的 L2 范数（推荐总览）
plot(mfit, xvar = "lambda", label = TRUE, type.coef = "2norm")
# 方式二（默认）：为每个响应分别绘制一张系数路径图（多图）
plot(mfit, xvar = "lambda", label = TRUE, type.coef = "coef")
```
`type.coef = "2norm"` 将同一变量的 K 个系数压缩为其 $\ell_2$ 范数，便于观察组-lasso 的变量进出行为；`type.coef = "coef"` 则逐响应展示，信息更细但图形数量为 $K$ 张。
### 5. 预测
预测结果为一个三维数组：前两维构成各响应的预测矩阵，第三维对应不同的 $\lambda$ 值：
```r
predict(mfit, newx = x[1:5, ], s = c(0.1, 0.01))
# 返回 5 x K x 2 的数组
```
### 6. 交叉验证
`cv.glmnet` 用法与单响应一致，会基于整体 MSE（所有响应合并计算）选择 $\lambda$：
```r
cv.mfit <- cv.glmnet(x, y, family = "mgaussian")
plot(cv.mfit)
cv.mfit$lambda.min
cv.mfit$lambda.1se
```
评估指标方面，mgaussian 可用的 measure 为 `mse` 和 `mae`（区别于二项式的 deviance/class 等）。
## 四、典型适用场景与注意事项
**适用场景**：
- 多任务学习：各响应共享同一批潜在解释变量，如多性状遗传关联、多站点环境预测、脑成像多通道回归；
- 响应间存在相关性，希望通过联合估计借用信息强度；
- 需要在变量层面（而非系数层面）获得稀疏解，便于解释"哪些变量对所有任务都重要"。
**注意事项**：
- mgaussian 强制所有响应共享同一变量集。若各响应的重要变量差异很大，单独拟合各响应 Gaussian 模型或允许响应间不同支持集的模型可能更合适；
- 响应量纲差异大时需考虑 `standardize.response = TRUE`，否则系数收缩将不平衡；
- 数值稳健性上，若采用纯组-lasso（$\alpha = 1$）遇到极端相关问题，可取 $\alpha$ 略小于 1（如 0.95）以获得类似弹性网的稳定行为；
- 预测输出为三维数组而非矩阵，编写下游代码时需注意索引方式。
## 小结
mgaussian 通过组-lasso 惩罚将"多个相关响应的联合建模"和"变量选择"统一到一个框架中，相比逐响应单独拟合，它能利用响应间相关性、产生一致的变量集、并减少多重检验带来的不稳定性，是 glmnet 处理多任务回归问题的标准工具。
## 八、本库实现与调用示例（VB.NET）

`linear-netcore5.vbproj` 中的 `Microsoft.VisualBasic.Data.Bootstrapping.MGaussian` 命名空间实现了上文描述的标准 mgaussian 能力：组-lasso 块坐标下降、`alpha` 弹性网混合、自动 `lambda` 正则化路径（warm start）、`standardize.response`、单/多 lambda 预测、2-norm 系数路径导出，以及 K 折交叉验证（`mse` / `mae` 与 `lambda.min` / `lambda.1se`）。绘图属于可视化，不在库实现范围内，但路径可导出成二维表供绘图使用。

```vbnet
Imports Microsoft.VisualBasic.Data
Imports Microsoft.VisualBasic.Data.Bootstrapping
Imports Microsoft.VisualBasic.Data.Bootstrapping.MGaussian

' 1) 直接以 N x K 响应矩阵拟合：x(sample)(feature)，y(sample)(response)
Dim fit As MGaussianFit = MGaussianFitting.Fit(
    x, y,
    numberOfLambdas:=100,
    alpha:=1.0,                  ' 1 = 组-lasso；0 = 岭型收缩；0.95 = 弹性网混合
    standardizeResponse:=True)   ' 各响应量纲悬殊时建议开启

Console.WriteLine(fit.ToString())
Console.WriteLine($"K={fit.NumberOfResponses}, p={fit.NumberOfFeatures}")
Console.WriteLine($"lambda=[{fit.MaxLambda:G4}, {fit.MinLambda:G4}]")

' 2) 提取系数与 2-norm 路径（对应 R 的 coef() 与 plot(type.coef="2norm")）
Dim coefStep As Double()() = fit.getCoefficients(50)  ' (p+1) x K，第 0 行是各响应截距
Dim norms As Double() = fit.getGroupNorms(50)        ' p 维，每变量 K 个系数的 l2 范数
Dim pathTable As NumericTable = fit.toNumericTable() ' 每步一行，携带 lambdas/df/mse 等标签列

' 3) 预测：单 lambda 返回 N x K，多 lambda 返回 (step)(sample)(response) 三维数组
Dim yhat As Double()() = fit.Predict(newx)
Dim yhat3d As Double()()() = fit.PredictPath(newx, New Integer() {0, 50, 99})

' 4) 多响应模型不是 IFitted（单响应契约）；取逐响应视图即可复用既有写回链路
'    先把 x / y 装进 NumericTable：每一个标签列就是一个响应
Dim table As New NumericTable(x, Nothing, featureNames)
For c As Integer = 0 To y(0).Length - 1
    Call table.SetLabel($"y{c + 1}", y.Select(Function(row) row(c)).ToArray())
Next
Dim view As IFitted = fit.GetResponseModel(0)
Call table.SetPrediction(view, name:="y1_fit")

' 5) 统一二维表入口：缺省把全部标签列组成 N x K 响应矩阵，也可指定列子集
Dim model As MGaussianFit = table.MGaussianRegression(
    y:=New String() {"y1", "y2"}, numberOfLambdas:=50)

' 6) K 折交叉验证：全响应合并的 mse / mae，自动选 lambda.min 与 lambda.1se
Dim cv As MGaussianCVFit = MGaussianCV.CrossValidate(
    x, y, nfolds:=10, numberOfLambdas:=100, measure:=CVMeasure.Mse)
' 或直接从表入口调用：Dim cv = table.MGaussianCV(nfolds:=10)

Console.WriteLine($"lambda.min={cv.lambdaMin:G4}, lambda.1se={cv.lambda1se:G4}")
Dim best As Double()() = cv.Predict(newx)             ' 用 lambda.min 预测
Dim sparse As Double()() = cv.Predict(newx, use1se:=True) ' 用 lambda.1se 预测更稀疏的模型
Dim cvCurve As NumericTable = cv.toNumericTable()     ' CV 曲线（cvMean/cvSe/df/trainMse）
```

结果解读要点：

- `groupNorms(step)(j)` 是第 j 个变量在 K 个响应上的系数向量的 l2 范数；它为 0 时该变量的**全部** K 个系数同时为 0，这就是「变量同进同出」的共享支持集。
- `alpha=1` 得到精确组稀疏；`alpha=0` 退化为对整个系数矩阵的岭型收缩，不会产生精确零；极端相关特征可取 `alpha≈0.95` 提升数值稳定性。
- `standardize.response=True` 会先标准化各响应列，避免量纲大的响应主导损失；预测结果自动还原到原始量纲。
- `lambda` 网格从 KKT 条件给出的 `lambda_max` 按对数间隔递减，上一步系数作为下一步初值；`df` 是每步的非零变量组数，`trainMse` 是训练数据上的全响应均方误差。
- `lambda.min` 是折外误差最小处；`lambda.1se` 是满足 `cvMean <= cvMean(min) + cvSe(min)` 的最大 lambda，模型更稀疏、误差略高但更稳健。
- 多响应结果不实现 `IFitted`；`PredictPath` 的索引顺序是 `(lambda 步骤)(样本)(响应)`，对应 R 中 `N x K x L` 的三维数组，但第一维换成了 lambda 步骤，方便按步骤遍历。