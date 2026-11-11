# GLMNet — GNU R `glmnet` 5.1 的 VB.NET 复刻

本工程在 `GLMNet` 根命名空间下提供一套与 R 包 `glmnet` 5.1 同语义的
"惩罚广义线性模型正则化路径"实现，覆盖 `gaussian` / `binomial` /
`poisson` / `multinomial` / `cox` / `mgaussian` 六个 family，
并统一 `glmnet()` / `cv.glmnet()` / `predict()` / `coef()` / `deviance()` /
`assess()` 的对象契约。

目标框架 `net10.0`，不引入新的 NuGet 依赖；R 仅作为**离线对拍工具**，
不是运行时依赖。

## 与 R 5.1 的参数映射

| R 参数 | VB API | 说明 |
|---|---|---|
| `family` | `Fit(x, y, "gaussian", options)` | 也接受 `FamilyKind` 枚举 |
| `alpha` | `GlmnetOptions.Alpha` | 1 = lasso（默认），0 = ridge |
| `lambda` | `GlmnetOptions.Lambda` | 严格递减的正数序列；为空时自动生成 |
| `nlambda` | `GlmnetOptions.NLambda` | 默认 100 |
| `lambda.min.ratio` | `GlmnetOptions.LambdaMinRatio` | 默认按 `n < p` 取 0.01 / 否则 1e-4 |
| `weights` | `GlmnetOptions.Weights` | 内部归一化到 `sum(w) = n` |
| `offset` | `GlmnetOptions.Offset` | 直接加在 link scale 上的 `n x k` 矩阵 |
| `standardize` / `intercept` | 同名属性 | 加权总体方差口径（除以 n） |
| `penalty.factor` | `GlmnetOptions.PenaltyFactor` | 0 = 永不惩罚，内部缩放至和为 p |
| `lower.limits` / `upper.limits` | 同名属性 | 原始量纲，内部换算到标准化空间 |
| `exclude` | `GlmnetOptions.Exclude` | 0 基变量下标 |
| `dfmax` / `pmax` | 同名属性 | 路径早停上限 |
| `thresh` / `maxit` | 同名属性 | 默认 1e-7 / 1e5 |
| `type.gaussian` / `type.multinomial` / `type.logistic` | 同名属性 | 5.1 实测：`type.multinomial` 默认 `ungrouped` |
| `standardize.response` | 同名属性 | 仅 mgaussian |
| `cox.ties` | `GlmnetOptions.CoxTies` | **R 5.1 实测默认 `breslow`**（`formals(glmnet)$cox.ties = c("breslow","efron")`） |
| `relax` | `GlmnetOptions.Relax` | relaxed lasso 的 gamma 网格 |
| `nfolds` / `foldid` / `type.measure` / `grouped` / `keep` | `GlmnetOptions` 的 CV 属性 | `cv.glmnet` 使用 |

> 注意：既有 `ElasticNet` 程序集使用 sklearn 语义
> （`alpha` = 总强度、`l1_ratio` = 混合比例），与本库**相反**；
> 本库统一使用 glmnet 语义并通过内核重新实现，避免语义混用。

## 快速上手

```vbnet
Imports GLMNet
Imports Microsoft.VisualBasic.Data

Dim options As New GlmnetOptions With {
    .Alpha = 1.0,
    .NLambda = 100,
    .PenaltyFactor = Nothing
}

' 拟合完整的 lambda 路径（六个 family 都返回同一类型）
Dim fit As GlmnetFit = GlmnetSolver.Fit(x, y, "gaussian", options)
Console.WriteLine(fit.ToString())
Console.WriteLine($"lambda.max = {fit.lambda(0):G6}, nulldev = {fit.nulldev:G6}")

' 预测 / 系数：s 支持数值、"lambda.min"、"lambda.1se"
Dim p As PredictionBundle = GlmnetPredict.Predict(fit, newx, PredictionType.Response, s:=0.05)
Dim coef As Double()() = GlmnetPredict.Coef(fit, s:=0.05, exact:=True)

' 交叉验证
Dim cvOptions As New GlmnetOptions With {.NFolds = 10, .TypeMeasure = CVMeasure.Mse}
Dim cv As GlmnetCVFit = GlmnetSolver.CrossValidate(x, y, "gaussian", cvOptions)
Console.WriteLine($"lambda.min = {cv.lambdaMin:G6}, lambda.1se = {cv.lambda1se:G6}")

' 二维表入口（X 取全部特征列，y 从标签列读取）
Dim table As New NumericTable(x, Nothing, Nothing)
Call table.SetLabel("y", y.Select(Function(r) r(0)).ToArray())
Dim tableFit As GlmnetFit = table.Glmnet("gaussian", options, New String() {"y"})
Dim pathTable As NumericTable = fit.toNumericTable()
```

稀疏输入可用 `SparseCSC.FromDense(x)` 构造后传给
`GlmnetSolver.Fit(sparse, y, family)`；列压缩结构会展开为内核使用的列主序数组。

## 架构

```
GlmnetSolver / GlmnetPredict        统一 API
  └─ FamilyRegistry → IGlmnetFamily
       ├─ SingleResponsePath        gaussian / binomial / poisson（新建 IRLS + 加权 CD）
       ├─ MultinomialFamily         softmax 组/非组惩罚（新建）
       ├─ CoxFamily                 复用 DataFittings.CoxFitting
       └─ MGaussianFamily           复用 DataFittings.MGaussianFitting
  └─ Internals/                    加权标准化、lambda 网格、惩罚因子、
                                  强规则、坐标下降、IRLS、收敛、稀疏 CSC
  └─ Evaluation/                   type.measure、assess/ROC/混淆矩阵、relaxed
```

`GlmnetFit` 输出 R 风格的 `a0 / beta / lambda / df / dev.ratio / nulldev /
nzero / jerr`；`GlmnetCVFit` 输出 `cvm / cvsd / cvup / cvlo / lambda.min /
lambda.1se / foldid`。

## R 5.1 对拍与测试报告

本地环境：

```
C:\Program Files\R\R-4.5.0\bin\Rscript.exe
R 4.5.0 + glmnet 5.1
```

1. 版本与默认参数核验：

```powershell
& 'C:\Program Files\R\R-4.5.0\bin\Rscript.exe' `
  'GLMNet/test/R/check_version.R'
```

2. 重新生成六个 family 的黄金数据（`test/data/`）：

```powershell
& 'C:\Program Files\R\R-4.5.0\bin\Rscript.exe' `
  'GLMNet/test/R/export_golden.R' `
  'GLMNet/test/data'
```

脚本导出每个 family 的 `*_x.csv`、`*_y.csv`、`*_fit.csv`（lambda/a0/df/dev.ratio/nulldev）、
`*_beta.csv`、`*_cv.csv` 和 `*_foldid.csv`。**显式导出 foldid** 是为了让
VB 与 R 的 CV 使用完全相同的分折，避免随机分折带来的对拍噪声。

3. 运行自检（按项目规则使用 `DebugAudit` 记录结果并生成报告）：

```powershell
dotnet run --project GLMNet/test/test.vbproj
```

报告写到 `test/bin/Debug/net10.0/glmnet-test-report.json`，包含
逐项数值快照、断言失败与边界用例结果。对拍容差：`lambda` 与 `df` 近似严格、
系数与 `dev.ratio` 用相对容差、`cvm/cvsd` 用相对容差，
`lambda.min` 比较在网格上的位置（允许若干步偏差）。

## 实测校准记录

- `lambda.max`（gaussian，QuickStartExample）= `1.63076208204452`，
  与公式 `max_j |sum_i w_i*x_ij*(y_i - ybar)| / (n*alpha)` 完全一致；
- `nulldev` = `852.479177529376`，即 `sum w*(y - ybar)^2`；
- R 会因 `fdev` 规则把 100 步网格提前截断（QuickStartExample 实际 67 步），
  本库同样实现 `fdev` / `devmax` / `dfmax` / `pmax` 早停；对拍时把 R 的
  实际 lambda 序列作为 `options.Lambda` 传入即可逐步对齐；
- `type.multinomial` 默认 `ungrouped`；`cox.ties` 默认 `breslow`；
- `glmnet.control()` 5.1 的 `eps = 1e-6`、`devmax = 0.999`。

## 已知限制

- `multinomial` 的 `grouped` 模式采用逐类别 IRLS 近似块更新，
  与 R 的 C++ 分组内核在系数绝对值上可能有差异（路径趋势一致）；
- `cox` / `mgaussian` 通过既有内核适配，`weights` 与 `offset` 的
  支持范围以既有内核为准；`start-stop` / 分层 Cox 响应尚未接入；
- 稀疏输入当前展开为稠密列数组，功能完整但不是 O(nnz) 的稀疏热循环；
- `relax` 目前支持三个单响应 family，cox / mgaussian / multinomial 会
  显式抛出 `NotSupportedException`；
- 自定义 `glm()` family 对象与 IRLS 步长折半通用化尚未实现；
- 自定义观测权重下，绝对 `deviance` 与 R 相差一个权重归一化常数，
  但 `dev.ratio`（分子分母同比缩放）保持一致。

## 许可证

按 Friedman 2010、Simon 2011/2013、Tibshirani 2012、Tay 2023 与
glmnet 5.1 官方文档独立实现，未移植 R 包的 Fortran/C++ 源码；
与兄弟工程保持一致（GPL-3.0-or-later）。
