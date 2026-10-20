---
name: lma-nonlinear-fit
overview: 实现 DataFittings/LMA.vb 的 Levenberg-Marquardt 非线性最小二乘拟合模块：模型桥接（字符串公式 + 委托两条路径，共用同一套 LmScalarModel 桥）、专用结果类、默认幂律模型入口，并同步补 NumericTable 表入口、smoketest 断言与 LmSolver 稳健性修复。
todos:
  - id: survey-refs
    content: 用 [subagent:code-explorer] 全仓确认 NonLinearFit 与 AsFitInputs 的调用面
    status: completed
  - id: solver-robust
    content: 加固 LmSolver：修复 λ=0 死锁、NaN/Inf 拒绝、开放迭代数与收敛状态
    status: completed
    dependencies:
      - survey-refs
  - id: lma-core
    content: 实现 LMA.vb 主体：模型桥、中心差分求导、三类入口与结果类
    status: completed
    dependencies:
      - solver-robust
  - id: table-entry
    content: 补 NumericTable 拟合入口、更新 AsFitInputs 注释并新增预测写回重载
    status: completed
    dependencies:
      - lma-core
  - id: smoketest-assert
    content: 在 smoketest 中补充三条入口的真实拟合断言与错误处理断言
    status: completed
    dependencies:
      - table-entry
  - id: build-verify
    content: 构建 DataFittings 与 smoketest，做到 0 警告 0 错误且自检全部通过
    status: completed
    dependencies:
      - smoketest-assert
---


## 产品概述

在曲线拟合与回归工具包 `DataFittings`（`linear-netcore5.vbproj`）中新增 **Ridge 回归（L2 正则化线性回归）**，与既有的 LASSO（L1 正则）、MLR（无正则）、NNLS（非负约束）并列，共用同一套「二维表 → 拟合模型 → 预测写回」链路。

## 核心功能

1. **带岭迹的正则化路径**：在 λ 网格上给出全部解，每行记录截距、各特征系数、λ、R²、GCV 与有效自由度；可按路径下标取任意一组系数，也可自动取出最优 λ 对应的单个模型。
2. **闭式解求解**：`(XᵀX + nλI)β = Xᵀy`，λ>0 时矩阵恒正定，因此在 OLS 会失效的**完全共线性**数据上仍可给出有限、稳定的系数。
3. **λ 的两种给法**：用户显式指定（只在该 λ 上求解一次）；未指定时按 **GCV 准则自动挑选**（闭式、无随机性，结果可复现）。
4. **标准化与无惩罚截距**：按列中心化 + 标准化后拟合，再把系数还原到原始量纲；截距单独由 ȳ 反推，不施加惩罚。
5. **二维表集成**：`RidgeRegression` 表入口直接消费 `NumericTable`；结果实现 `IFitted`，可直接接入既有的 `SetPrediction` / `PredictTable` 写回预测值与残差；岭迹路径可导出为 `NumericTable`（行=λ，列=截距+各特征系数）。
6. **自检与文档**：共线性稳定性、λ→0 与 MLR 的一致性、路径规模与 GCV 选 λ、预测写回等断言，并更新 README。



## 技术栈

- 语言/框架：VB.NET，net10.0，工程 `DataFittings\linear-netcore5.vbproj`（RootNamespace `Microsoft.VisualBasic.Data.Bootstrapping`）
- 既有依赖（工程内已引用，不新增）：`Microsoft.VisualBasic.Math.LinearAlgebra.Matrix`（`NumericMatrix`/`CholeskyDecomposition`/`MatrixOps`/`Vector`）、`Microsoft.VisualBasic.Math.LinearAlgebra`（`Formula`）、`Microsoft.VisualBasic.Data`（`NumericTable`）、`Microsoft.VisualBasic.ComponentModel.Collection`（`RectangularArray`）
- 约束：`GenerateDocumentationFile=True`，全部 Public 成员需 XML 注释；目标 Debug/Release 均 **0 警告 0 错误**；工程为 SDK-style，新 `.vb` 文件自动纳入编译，无需改 vbproj

## 实现思路

采用「**标准化 + Cholesky 闭式解 + 单次分解复用**」：不引入迭代优化，而是把 Ridge 写成可解析求解的正规方程，用一次 Cholesky 分解同时拿到系数与 GCV 所需的有效自由度。

关键决策与取舍：

1. **λ 尺度与样本量解耦**。在标准化数据上把目标函数写成 `min ||ỹ − X̃β||² + n·λ·||β||²`，令 `R = X̃ᵀX̃/n`（相关矩阵，对角线恒为 1），正规方程即 `(R + λI)β = X̃ᵀỹ/n`。这样 λ 与 R 的特征值同尺度（∈ [0,p]），默认值 [1e-4, 10] 对任何 n、p 都合理，无需按数据量调参。
2. **GCV 优于 K 折 CV**。GCV(λ) = RSS(λ) / (n·(1 − (df+1)/n)²) 是闭式的、确定性的，不需要随机折划分，自检结果可复现；同时 K 折 CV 会随折划分方式变化，不利于回归测试。
3. **有效自由度走 `df = p − λ·tr(B⁻¹)`**。帽子矩阵 `H = X̃(X̃ᵀX̃+nλI)⁻¹X̃ᵀ`，其迹与 `B = R + λI` 的逆迹有恒等关系。求逆用一次 `chol.Solve(Identity(p))` 完成（对单位阵求解即求逆，`Solve` 已核实支持多列右端项），比逐 λ 做 O(np²) 的前代替换更省，且随 λ→0 有 `df→p`、λ→∞ 有 `df→0` 的正确极限。
4. **Cholesky 而非求逆**。λ>0 时 `R + λI` 严格正定，即便原始数据完全共线性也不奇异；而 `MatrixOps.Inverse` 在奇异时会返回 Nothing，无法用于共线性数据。
5. **绝不使用 `*` 做矩阵乘法**。本库的 `NumericMatrix` 的 `*` 是**逐元素（Hadamard）乘法**（见下方陷阱 1），因此 XᵀX、Xᵀy、β = B⁻¹b 全部按定义自写循环或用 `MatrixOps.Multiply`。
6. **自含 RidgePolynomial**。`IFitted` 要求返回 `Polynomial As Formula`，而既有的 `MultivariatePolynomial` 求值式为 `Σ x(i)*Factors(i)`、**不含截距**；Ridge 自带截距，因此在 Ridge 命名空间内继承 `Formula` 提供 `Intercept + Σ x(i)*Factors(i)` 的求值，避免改动公共 Math 库、也避免下游拿到语义错误的模型。

## 实现要点（防踩坑细节）

- **⚠ 库陷阱 1（必须绕开）**：`NumericMatrix` 的 `*` 是逐元素乘法，维度不一致会抛 `ArgumentException("GeneralMatrix dimensions must agree.")`，维度恰好一致时静默给出逐元素积。Ridge 全部线性代数自写循环。
- **⚠ 库陷阱 2**：`CholeskyDecomposition` 的 SPD 判据含 `isspd = isspd And (A(k)(j) = A(j)(k))`，是**逐位精确对称比较**。构造 R 时必须只算上三角再镜像填充，保证 `A = R + λI` 逐位对称；λ 极小时若判为非 SPD，则按 ×10 递增 λ 重试，仍失败则跳过该 λ 并在路径中标记。
- **常数列守卫**：标准差 s_j = 0 的列（如 smoketest 的 `bias` 常量列）必须守卫为 s_j = 1，否则还原系数时会除零。
- **性能**：标准化 O(np)；R = X̃ᵀX̃/n 只算上三角 O(np²/2)，**整条路径只算一次**；每个 λ 一次 Cholesky O(p³/3) + 一次求逆式 Solve O(p³) + 一次系数 Solve O(p²) + RSS O(np)。整条 100 点路径的复杂度为 O(np²/2 + 100·(p³ + np))，p 较小时可忽略。
- **GCV 的 RSS 定义在中心化响应上**，自由度取 `df + 1`（+1 是截距），必须在 XML 注释中写明，避免与「含截距的 RSS」口径混淆。
- **数值输出**：`SSE`/`SST`/`R²` 建在原始量纲的拟合值上（还原后再算），保证与 `MLRFit` 的 `R2 = 1 − SSE/SST` 口径一致，便于自检对拍。
- **变量命名**：VB 大小写不敏感，避免 `beta`/`R2` 之类的局部名与属性同名；不使用 `error`、`Step`、`Next`、`Call`、`Set`、`Index`、`Result` 等保留字；XML 注释中不得出现 `--`；类型名避免 `Ridge`（与命名空间同名会造成解析歧义），求解器模块取名 `RidgeFitting`。
- **公开 API 全部写 XML 注释**，否则触发文档警告；内部辅助一律 `Friend`/`Private`。

## 架构设计

```
NumericTable(特征矩阵 X + 标签列 y)
        │  RidgeRegression 表入口（复用既有 FeatureMatrix/ResponseVector/FeatureNameList）
        ▼
RidgeFitting.Fit(x, y, lambda?)         ← 求解器（标准化 → R → λ 路径 → GCV 选优）
        │
        ├── 标准化：列均值 m、列尺度 s（s=0 守卫为 1）、ȳ
        ├── 每 λ：(R+λI) 的 Cholesky → β̃ ; df = p − λ·tr(B⁻¹) ; RSS ; R² ; GCV
        └── 还原：coef_j = β̃_j/s_j ; intercept = ȳ − Σ β̃_j·m_j/s_j
        ▼
RidgeFit（lambdas / intercepts / coefficients / rsquared / gcv / df / rss
          + bestIndex + 原始量纲尺度 + GetY/PredictAt/getCoefficients/toNumericTable）
        │  Implements IFitted（R2 / Polynomial / ErrorTest / GetY）
        ▼
NumericTablePrediction.SetPrediction / PredictTable（既有，零改动即可工作）
NumericTable（岭迹路径表，风格对齐 LassoFit.toNumericTable）
```

## 目录结构

```
DataFittings/
├── Ridge/                              # [NEW] Ridge 回归子模块，命名空间 Ridge
│   ├── RidgeFit.vb                     # [NEW] 结果类。保存 λ 路径（lambdas / intercepts / coefficients(path×p) / rsquared / gcv / df / rss）、最优下标 bestIndex、原始量纲的 columnMeans/columnScales/yMean、featureNames；方法 getCoefficients(lambdaIdx)、BestLambda/BestCoefficients/BestIntercept、GetY(ParamArray x)（IFitted 契约，取最优 λ）、PredictAt(lambdaIdx, x)、toNumericTable()（岭迹表：行=λ，列=intercept+各特征，标签=lambdas/rsquared/gcv/Df）、ToString()。实现 IFitted：R2 取最优 λ 的 1−SSE/SST，Polynomial 返回 RidgePolynomial，ErrorTest 填充 TestPoint(Y, Yfit)，GetY 用原始量纲系数预测。全部 Public 成员写 XML 注释。
│   ├── RidgeFitting.vb                 # [NEW] 求解器 Module。Public Function Fit(x As Double()(), y As Double(), Optional lambda As Double = Nothing, Optional nlambda As Integer = 100, Optional lambdaMin As Double = 0.0001, Optional lambdaMax As Double = 10, Optional standardize As Boolean = True, Optional featureNames As String() = Nothing) As RidgeFit；内部私有助手：Standardize（含 s=0 守卫）、Correlation（只算上三角再镜像）、SolveLambda（Cholesky 闭式解 + df + RSS + λ 递增重试）、ArgMinGCV。
│   ├── RidgePolynomial.vb              # [NEW] Public Class RidgePolynomial : Inherits Formula，Evaluate = Intercept + Σ x(i)*Factors(i)，两个 ToString 重载分别输出序号名与真实列名。
│   └── NamespaceDoc.vb                 # [NEW] 命名空间文档（对齐 LASSO/NamespaceDoc.vb 风格）。
├── NumericTableRegressions.vb          # [MODIFY] 顶部新增 Imports RidgeNS = Microsoft.VisualBasic.Data.Bootstrapping.Ridge；在既有 #Region 内新增 <Extension> Public Function RidgeRegression(table As NumericTable, Optional lambda As Double = Nothing, Optional y As String = Nothing, Optional Silent As Boolean = False) As RidgeNS.RidgeFit，内部复用 FeatureMatrix/ResponseVector/FeatureNameList。
├── README.md                           # [MODIFY] 核心能力表新增 Ridge 一行；快速上手补 Ridge 示例。
└── _smoketest/Program.vb               # [MODIFY] 新增 Ridge 断言（见 todolist）。
```

## 关键代码结构

```vbnet
Namespace Ridge

    ''' <summary>
    ''' Ridge 回归的拟合结果：λ 正则化路径（岭迹）+ 最优 λ 对应的单个模型
    ''' </summary>
    Public Class RidgeFit : Implements IFitted

        ''' <summary>路径上的 λ 序列</summary>
        Public Property lambdas As Double()
        ''' <summary>每一个 λ 对应的截距（原始量纲）</summary>
        Public Property intercepts As Double()
        ''' <summary>每一个 λ 对应的各特征回归系数（原始量纲），行为 λ、列为特征</summary>
        Public Property coefficients As Double()()
        ''' <summary>每一个 λ 对应的决定系数</summary>
        Public Property rSquared As Double()
        ''' <summary>每一个 λ 对应的 GCV 准则值</summary>
        Public Property gcv As Double()
        ''' <summary>每一个 λ 对应的有效自由度</summary>
        Public Property df As Double()
        ''' <summary>GCV 最小的那一步的下标</summary>
        Public Property bestIndex As Integer

        Public ReadOnly Property BestLambda As Double
        Public ReadOnly Property BestIntercept As Double
        Public ReadOnly Property BestCoefficients As Double()

        Public Function getCoefficients(lambdaIdx As Integer) As Double()
        Public Function PredictAt(lambdaIdx As Integer, x As Double()) As Double
        Public Function toNumericTable(Optional featureNames As String() = Nothing) As NumericTable

        Public ReadOnly Property R2 As Double Implements IFitted.R2
        Public ReadOnly Property Polynomial As Formula Implements IFitted.Polynomial
        Public Property ErrorTest As IFitError() Implements IFitted.ErrorTest
        Public Function GetY(ParamArray x As Double()) As Double Implements IFitted.GetY
    End Class

    ''' <summary>
    ''' Ridge 求解器：标准化 + Cholesky 闭式解 + λ 路径 + GCV 选 λ
    ''' </summary>
    Public Module RidgeFitting
        Public Function Fit(x As Double()(),
                            y As Double(),
                            Optional lambda As Double = Nothing,
                            Optional nlambda As Integer = 100,
                            Optional lambdaMin As Double = 0.0001,
                            Optional lambdaMax As Double = 10,
                            Optional standardize As Boolean = True,
                            Optional featureNames As String() = Nothing) As RidgeFit
    End Module
End Namespace
```

## Agent Extensions

### SubAgent

- **code-explorer**
  - 用途：动工前快速确认 `Ridge` / `RidgeFit` / `RidgeFitting` / `RidgePolynomial` 这些新名字在 G:/pixelArtist 全仓是否与既有类型、命名空间或模块名冲突（含大小写不敏感的 VB 同名判定），并定位 `RidgeRegression` 表入口在 `NumericTableRegressions.vb` 中的最佳插入位置与 region 归属。
  - 预期产出：确认命名无冲突（当前已知全仓无 Ridge 回归实现，仅 LMA.vb 的 `bridge` 变量与 OLS.vb 偶含 ridge 子串），给出表入口插入点。


## Agent Extensions

### SubAgent

- **code-explorer**
  - 用途：确认 `Ridge` / `RidgeFit` / `RidgeFitting` / `RidgePolynomial` 在全仓是否有命名冲突，并定位表入口的插入位置。
  - 预期产出：命名冲突清单（应为空）+ `NumericTableRegressions.vb` 的具体插入点。
