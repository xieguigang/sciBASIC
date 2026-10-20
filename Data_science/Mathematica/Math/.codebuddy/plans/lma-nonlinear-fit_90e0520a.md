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

把 `DataFittings/LMA.vb` 里仍抛 `NotImplementedException` 的 Levenberg-Marquardt 非线性最小二乘入口补全为真正可用的算法模块，并让它接入本包既有的「二维表 → 拟合模型 → 预测写回」链路。

## 核心功能

1. **三类拟合入口，共用同一套求解内核**

- 默认入口 `NonLinearFit(输入集, iterations)`：不传模型，拟合纯数据驱动的幂律（Cobb-Douglas / 异速生长）模型 `y = p0 · x1^p1 · x2^p2 …`，初值由对数域 OLS 给出。
- 委托入口：用户传入 `f(参数, 自变量)` 的 lambda 与初值（或参数名），按位置取自变量。
- 公式字符串入口：用户传入如 `a*exp(-b*x1)+c` 的表达式与待估参数名，自变量按 `FitInput.factors` 的**列名**自动绑定，内部编译成委托后与委托入口共用同一条求解路径。

2. **通用非线性模型桥接**：把任意模型包装成求解器要求的逐样本 `f / ∂f/∂p / ∂²f/∂p²`，导数用中心差分（步长随参数量纲自适应）。
3. **专用拟合结果对象**：返回包含参数名与参数值、R²、RMSE、SSE、残差、拟合值、实际迭代数、是否收敛、预测函数 `GetY(x)`、可读摘要与 JSON 的结果对象，替代原来的 `As Object`。
4. **二维表入口与预测写回**：在 `NumericTableRegressions` 上提供同名的表级拟合入口（内部复用 `AsFitInputs`），并把 LMA 结果接进 `NumericTablePrediction` 的预测/残差写回。
5. **求解器稳健性**：修掉阻尼因子为 0 时 Cholesky 恒失败导致的空转、模型返回 NaN/Inf 时的静默无进展，并对外暴露迭代数/收敛状态/最终误差。
6. **自检覆盖**：在 smoketest 里对三条入口都加上真实拟合断言（参数还原精度、R²、预测一致性）与错误处理断言。

## 技术栈

- 语言/框架：VB.NET，net10.0，工程 `DataFittings/linear-netcore5.vbproj`（RootNamespace `Microsoft.VisualBasic.Data.Bootstrapping`）
- 既有依赖（全部工程内已引用，不新增）：`Microsoft.VisualBasic.Math.LinearAlgebra.Matrix`、`...LinearAlgebra.Solvers.OLS`、`Microsoft.VisualBasic.Math.Scripting.MathExpression.ExpressionEngine`、`Microsoft.VisualBasic.ComponentModel.Collection.RectangularArray`、`Microsoft.VisualBasic.Serialization.JSON`
- 约束：`GenerateDocumentationFile=True`，Public 成员必须写 XML 注释；目标是 **0 警告 0 错误**；内部实现类一律 `Friend` 以规避文档警告

## 实现思路

采用「**模型桥 + 既有 LM 内核**」：不重写阻尼最小二乘，而是把用户模型包装成 `LevenbergMarquardt.LmScalarModel`，交给既有的 `LmSumSquaresError` + `LmSolver` 求解，LMA 只负责数据整形、模型表达、导数近似、初值构造与结果统计。

关键决策与取舍：

1. **字符串公式与委托两条路径共用同一个桥**：字符串版本在拟合前用 `ExpressionEngine.Parse` 解析一次得到 `Expression`，运行时逐样本 `SetSymbol` 更新符号后 `Evaluate(已解析表达式)`，其对外表现与手写委托完全一致。这样求导/求解/统计只有一份实现，避免双份逻辑走偏。
2. **导数用中心差分，步长自适应** `h_j = eps·(1+|p_j|)`（默认 eps = 1e-5）。解析求导需要引入符号微分（MathLambda 体系），代价远超收益；中心差分在 Double 精度下截断与舍入误差平衡，且对任意用户模型都成立。
3. **默认走高斯-牛顿近似海塞**（`approxHessianFlg = True`）。LM 的标准做法就是 `H ≈ JᵀJ`，省掉每样本 `O(p²)` 次模型求值的二阶差分；`LmSumSquaresError.hessian` 在该开关下本身就不访问 `hessian(i,·)`，但仍保留精确二阶差分实现以备精确海塞选项。
4. **自变量顺序**：委托路径以第一个样本的键序为准，并逐样本校验键集完全相同，不一致直接抛清晰异常（`Dictionary` 的枚举顺序不是契约保证，必须显式定序）；字符串路径按名绑定，天然不受顺序影响，但需处理非法标识符。
5. **不实现 `IFitted`**：该接口要求返回多项式 `Formula`，非线性模型无法表示，返回 Nothing 会给下游埋 NRE；改为在 `NumericTablePrediction` 新增接受 LMA 结果对象的重载，代价是多两个小函数、收益是类型安全。

## 实现要点（防踩坑细节）

- **文件头必须原样保留**：`LMA.vb` 顶部的 `#Region "Microsoft.VisualBasic::069b6312a79e771279d3290e6c090929, Data_science\..."` … `#End Region` 自动生成块不要动，正文写在 `#End Region` 之后。
- **变量命名**：VB 大小写不敏感且保留字多 —— 不要用 `matrix`、`x`（与形参撞）、`error`/`[Step]`/`Next`/`Call`/`Set`/`Index`/`Result` 作局部名；残差局部变量用 `[error]` 或改名 `resid`；`Module` 成员不能写 `Shared`；XML 注释里不能出现 `--`。
- **公式路径的标识符陷阱**：分词器要求标识符以字母开头（`"x1"` 合法，纯数字名会被当成字面量）。而 `FeatureNameList` 在无列名时回退为 `"1","2",…`，因此字符串入口必须做标识符合法性检查，非法名自动替换为 `x1..xm` 别名并在结果的 `FactorNames` 中体现；同时用 `CheckFunction` 与 `GetVariableSymbols` 校验不与内建函数（`log`/`exp`/`pow`…）重名、公式中不存在无法绑定的符号。
- **表达式引擎复用**：解析一次、复用同一个引擎实例逐样本改符号求值，禁止在热路径里每次 `Evaluate(String)`（那会重新分词 + 建树）。
- **性能**：每轮迭代的模型求值次数约为 `n·(2p+1)`（雅可比 + 残差），`p` 为参数个数、`n` 为样本数；空间复杂度 `O(n·m + p²)`。热路径是中心差分，避免在差分内部分配大对象，参数向量用预分配缓冲 + 改完再还原。字符串路径额外注意引擎字典写入是热操作，一次性把因子名与参数名写入同一引擎。
- **日志/输出**：对数域 OLS 初值计算会向 `Console` 打印奇异警告，调用期间临时替换 `Console.Out` 并在 `Try/Finally` 中恢复，避免污染调用方输出；其余路径静默，异常只由入口抛出清晰信息，不做内部捕获吞掉。

## 架构设计

```
用户模型（公式字符串 / 委托 / 默认幂律）
        │  统一编译成 NonLinearModel 委托
        ▼
LmFitModel（实现 LmScalarModel：逐样本 f / ∂f/∂p / ∂²f/∂p²）
        ▼
LmSumSquaresError（0.5Σr²、g=−Jᵀr、H≈JᵀJ）
        ▼
LmSolver（(H+λI)h=−g，增益比自适应调 λ）  ← 本次加固
        ▼
NonLinearFitResult（参数、R²、RMSE、SSE、残差、GetY、迭代数/收敛）
        ▼
NumericTable 入口 ← AsFitInputs ← NumericTable
        ▼
NumericTablePrediction 预测/残差写回
```

## 目录结构

```
DataFittings/
├── LMA.vb                                  # [MODIFY] 主体。保留自动生成的文件头与 FitInput；新增 NonLinearModel 委托、Friend LmFitModel（LmScalarModel 桥，中心差分雅可比 + 精确二阶海塞）、Public NonLinearFitResult（结果类）、三个 NonLinearFit 入口（默认幂律 / 委托 / 字符串公式）、幂律模型的构造与对数域 OLS 初值、输入校验与数据整形。所有 Public 成员写 XML 注释。
├── Levenberg-Marquardt/
│   └── LmSolver.vb                        # [MODIFY] 稳健性加固：λ=0 死锁修复、NaN/Inf 拒绝、对外暴露 Iterations / Converged / ErrorValue；主循环语义与收敛判据保持不变。
├── NumericTableRegressions.vb             # [MODIFY] 更新 AsFitInputs 的 XML 注释（删除"尚未实现"表述）；新增 3 个表级 NonLinearFit 扩展入口，复用既有 ResponseVector / FeatureMatrix / FeatureNameList 助手。
├── NumericTablePrediction.vb              # [MODIFY] 新增接受 LMA 结果对象的 SetPrediction / PredictTable 重载（不实现 IFitted，避免多项式语义污染）。
├── README.md                              # [MODIFY] 快速上手小节补上 LMA 三条入口的用法示例。
└── _smoketest/
    └── Program.vb                         # [MODIFY] 在 nonlinear 段新增：幂律默认拟合（专用正值因子表）、字符串公式拟合（如 a*exp(-b*t)）、委托拟合（如米氏方程）、参数还原精度 / R² / 预测一致性 / 错误处理断言。
```

## 关键代码结构

```
''' <summary>
''' 任意非线性模型：由参数向量与自变量向量计算模型的输出值
''' </summary>
''' <param name="params">待优化的模型参数向量</param>
''' <param name="x">单个样本的自变量向量（顺序由 <see cref="NonLinearFitResult.FactorNames"/> 决定）</param>
Public Delegate Function NonLinearModel(params As Double(), x As Double()) As Double

''' <summary>
''' 驱动既有 LM 内核的桥：逐样本给出模型值与一阶/二阶导数
''' （契约来自 LevenbergMarquardt.LmScalarModel，必须逐项实现）
''' </summary>
Friend Class LmFitModel : Implements LmScalarModel
    ' 字段：样本自变量矩阵 xs(n×m)、观测值 ys(n)、模型委托、差分步长
    Public ReadOnly Property MeasuredData As Double() Implements LmScalarModel.MeasuredData
    Public Function eval(dataIdx As Integer, optParams As Double()) As Double Implements LmScalarModel.eval
    Public Function jacobian(dataIdx As Integer, optParams As Double()) As Double() Implements LmScalarModel.jacobian
    Public Function hessian(dataIdx As Integer, optParams As Double()) As Double()() Implements LmScalarModel.hessian
End Class

''' <summary>
''' 非线性最小二乘拟合结果
''' </summary>
Public Class NonLinearFitResult
    Public Property Model As NonLinearModel          ' 拟合好的模型，用于预测
    Public Property Formula As String                ' 字符串公式入口时非空的原始公式
    Public Property FactorNames As String()          ' 自变量名（含自动生成的别名）
    Public Property ParameterNames As String()       ' 参数名
    Public Property Parameters As Dictionary(Of String, Double)
    Public ReadOnly Property Values As Double()      ' 参数向量形式
    Public Property SSE As Double
    Public Property RMSE As Double
    Public Property RSquared As Double
    Public Property Residuals As Double()
    Public Property FittedValues As Double()
    Public Property Iterations As Integer
    Public Property Converged As Boolean
    Public Function GetY(ParamArray x As Double()) As Double
    Public Overrides Function ToString() As String
End Class
```

## Agent Extensions

### SubAgent

- **code-explorer**
- 用途：在动工前快速确认 `NonLinearFit` / `AsFitInputs` / `LmSolver` 在 G:/pixelArtist 全仓的引用面，避免改坏下游调用方（仓库内 VB 无 documentSymbol provider，找引用需回退全文检索）。
- 预期产出：给出全部调用点与受影响工程清单，确认本次签名变更是安全的（当前全仓仅 3 处引用，均在 DataFittings 内）。