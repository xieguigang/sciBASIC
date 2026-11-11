Imports System
Imports System.IO
Imports System.Linq
Imports Microsoft.VisualBasic.ApplicationServices.Debugging
Imports Microsoft.VisualBasic.Data
Imports Microsoft.VisualBasic.Data.Bootstrapping
Imports Microsoft.VisualBasic.Data.Bootstrapping.Poisson
Imports std = System.Math

''' <summary>
''' 泊松回归的正确性审计：系数恢复、offset 标准化、统计推断、过度离散诊断、
''' 二维表入口与异常输入防护。
'''
''' 这是一个 console 类型的测试程序：所有断言都经由
''' <see cref="DebugAudit"/> 记录，结束时由 <see cref="DebugAudit.SaveReport(String)"/>
''' 生成 JSON 测试报告，<see cref="DebugAudit.FailureCount"/> 即失败数。
''' </summary>
''' <remarks>
''' 本模块刻意不定义 <c>Sub Main</c>：自检入口是同项目下的
''' <c>Program.vb</c>，它通过 <see cref="Run"/> 把审计结果合并进总的失败计数。
''' </remarks>
Public Module PoissonAudit

    Private Const ReportName As String = "poisson-test-report.json"
    Private Const TrueIntercept As Double = 4.0
    Private Const TrueB1 As Double = 0.5
    Private Const TrueB2 As Double = -0.3

    ''' <summary>
    ''' 运行全部审计用例并生成测试报告。
    ''' </summary>
    ''' <returns>失败断言的数量（0 表示全部通过）</returns>
    Public Function Run() As Integer
        Dim reportPath As String = Path.Combine(AppContext.BaseDirectory, ReportName)

        ' Check / Capture / Snapshot 带有 <Conditional("DEBUG")>，Release 下会被消除，
        ' 因此断言一律使用 Test / AreEqual / CheckThrows / Require / Mark / Watch。
        ' FailureCount / Records 为全局状态：先清空，保证报告只反映本次泊松审计。
        Call DebugAudit.Reset()
        DebugAudit.MaxEvents = 4096
        Call DebugAudit.Mark("poisson regression audit started")

        Try
            Call CheckCoefficients()
            Call CheckInference()
            Call CheckOffset()
            Call CheckOverDispersion()
            Call CheckTableEntry()
            Call CheckContract()
            Call CheckBoundary()
            Call CheckConvergence()
        Catch ex As Exception
            Call DebugAudit.Mark("audit.crash", ex.ToString())
            Call DebugAudit.Test(False, $"审计套件不应抛出未处理异常: {ex.Message}")
        End Try

        ' Snapshot 只在 Debug 配置生效，用于把关键诊断量附到报告之中；
        ' 正式断言不依赖它（Release 下该调用会被编译器消除）
        Call DebugAudit.Snapshot(
            DebugAudit.V(ReportName),
            DebugAudit.V(DebugAudit.FailureCount)
        )

        Dim dump As String = DebugAudit.Dump()

        Call DebugAudit.SaveReport(reportPath)
        Call Console.WriteLine($"   [audit] report -> {reportPath}")
        Call Console.WriteLine($"   [audit] dump length = {dump.Length}, {DebugAudit.FailureCount} failure(s)")

        Return DebugAudit.FailureCount
    End Function

#Region "test data"

    ''' <summary>构造两个近似独立、量纲温和的特征列（20 x 7 的全部组合都会出现）</summary>
    Private Function MakeRows(n As Integer) As Double()()
        Dim rows As Double()() = New Double(n - 1)() {}

        For i As Integer = 0 To n - 1
            rows(i) = New Double() {(i Mod 20) / 20.0R, (i Mod 7) / 7.0R}
        Next

        Return rows
    End Function

    ''' <summary>真实模型 ``mu = exp(4 + 0.5*x1 - 0.3*x2)`` 的期望计数</summary>
    Private Function ExpectedRates(rows As Double()()) As Double()
        Return rows _
            .Select(Function(r) std.Exp(TrueIntercept + TrueB1 * r(0) + TrueB2 * r(1))) _
            .ToArray()
    End Function

    ''' <summary>确定性计数：对期望计数四舍五入，避免随机数造成的测试抖动</summary>
    Private Function AsCounts(rates As Double()) As Double()
        Return rates.Select(Function(m) std.Round(m)).ToArray()
    End Function

    ''' <summary>暴露量 1..4，循环出现</summary>
    Private Function MakeExposures(n As Integer) As Double()
        Return Enumerable.Range(0, n).Select(Function(i) CDbl(1 + (i Mod 4))).ToArray()
    End Function

    Private Function ScaledCounts(rates As Double(), exposure As Double()) As Double()
        Return rates.Select(Function(m, i) std.Round(m * exposure(i))).ToArray()
    End Function

    Private Function MaxDiff(a As Double(), b As Double()) As Double
        If a Is Nothing OrElse b Is Nothing OrElse a.Length <> b.Length Then
            Return Double.PositiveInfinity
        End If

        Dim diff As Double = 0.0R

        For i As Integer = 0 To a.Length - 1
            diff = std.Max(diff, std.Abs(a(i) - b(i)))
        Next

        Return diff
    End Function

    Private Function AllFinite(values As Double()) As Boolean
        Return values IsNot Nothing AndAlso values.All(Function(v) Double.IsFinite(v))
    End Function

#End Region

#Region "coefficients"

    ''' <summary>已知参数生成的计数数据之上恢复截距与两个斜率</summary>
    Private Sub CheckCoefficients()
        Dim rows As Double()() = MakeRows(300)
        Dim counts As Double() = AsCounts(ExpectedRates(rows))
        Dim fit As PoissonFit = PoissonFitting.Fit(rows, counts)

        Call DebugAudit.Require(fit IsNot Nothing, "泊松拟合返回结果对象")
        Call DebugAudit.Test(fit.Converged, $"IRLS 在迭代上限内收敛: {fit.Iterations} 轮")
        Call DebugAudit.Test(fit.Iterations > 0 AndAlso fit.Iterations < 100,
                            $"迭代轮数合理: {fit.Iterations}")
        Call DebugAudit.Test(std.Abs(fit.Intercept - TrueIntercept) < 0.03R,
                            $"截距恢复到 {TrueIntercept}: b0={fit.Intercept:F6}")
        Call DebugAudit.Test(std.Abs(fit.Coefficients(0) - TrueB1) < 0.03R,
                            $"斜率 b1 恢复到 {TrueB1}: b1={fit.Coefficients(0):F6}")
        Call DebugAudit.Test(std.Abs(fit.Coefficients(1) - TrueB2) < 0.03R,
                            $"斜率 b2 恢复到 {TrueB2}: b2={fit.Coefficients(1):F6}")
        Call DebugAudit.AreEqual(2, fit.Coefficients.Length, "系数向量不含截距，长度等于特征列数")
        Call DebugAudit.AreEqual(3, fit.AllCoefficients.Length, "AllCoefficients 含截距")
        Call DebugAudit.Test(MaxDiff(fit.AllCoefficients,
                                     New Double() {fit.Intercept, fit.Coefficients(0), fit.Coefficients(1)}) < 0.000000000001R,
                            "AllCoefficients 的顺序为截距 -> 特征列")

        Dim rate As Double() = ExpectedRates(rows)
        Dim fitted As Double() = fit.FittedValues
        Dim maxRateDiff As Double = MaxDiff(rate, fitted)

        Call DebugAudit.Test(std.Abs(maxRateDiff) < 1.0R,
                            $"逐样本期望计数贴近真实率, maxΔ={maxRateDiff:F4}")
        Call DebugAudit.Test(fit.Deviance > 0.0R AndAlso fit.Deviance / fit.ResidualDf < 0.1R,
                            $"无噪声计数数据的离散比接近 0: D/df={fit.DispersionRatio:F6}")
        Call DebugAudit.Test(Not fit.IsOverDispersed, "等散性成立时不报过度离散")
        Call DebugAudit.Test(fit.PseudoR2 > 0.9R, $"基于偏差的伪 R² = {fit.PseudoR2:F6}")

        Call DebugAudit.Mark("poisson.coefficients.intercept", fit.Intercept)
        Call DebugAudit.Mark("poisson.coefficients.b1", fit.Coefficients(0))
        Call DebugAudit.Mark("poisson.coefficients.b2", fit.Coefficients(1))
        Call DebugAudit.Mark("poisson.coefficients.iterations", fit.Iterations)
    End Sub

#End Region

#Region "inference"

    ''' <summary>标准误、Wald Z、IRR 与 AIC / 对数似然的有效性</summary>
    Private Sub CheckInference()
        Dim rows As Double()() = MakeRows(240)
        Dim fit As PoissonFit = PoissonFitting.Fit(rows, AsCounts(ExpectedRates(rows)))

        Call DebugAudit.AreEqual(3, fit.StandardErrors.Length, "每个待估参数都有一个标准误")
        Call DebugAudit.AreEqual(3, fit.ZValues.Length, "每个待估参数都有一个 Wald Z 值")
        Call DebugAudit.AreEqual(3, fit.IncidenceRateRatios.Length, "每个待估参数都有一个 IRR")
        Call DebugAudit.Test(fit.StandardErrors.All(Function(s) Double.IsFinite(s) AndAlso s > 0.0R),
                            "标准误全部为有限正数")
        Call DebugAudit.Test(AllFinite(fit.ZValues), "Wald Z 值全部有限")
        Call DebugAudit.Test(std.Abs(fit.IncidenceRateRatios(1) - std.Exp(fit.AllCoefficients(1))) < 0.000000000001R,
                            "IRR 严格等于 exp(beta)")
        Call DebugAudit.Test(std.Abs(fit.IncidenceRateRatios(1) - std.Exp(TrueB1)) < 0.1R,
                            $"x1 的发生率比 IRR ≈ {std.Exp(TrueB1):F4}: {fit.IncidenceRateRatios(1):F4}")
        Call DebugAudit.Test(std.Abs(fit.IncidenceRateRatios(2) - std.Exp(TrueB2)) < 0.1R,
                            $"x2 的发生率比 IRR ≈ {std.Exp(TrueB2):F4}: {fit.IncidenceRateRatios(2):F4}")
        Call DebugAudit.Test(Double.IsFinite(fit.LogLikelihood) AndAlso Double.IsFinite(fit.AIC),
                            $"对数似然与 AIC 均为有限值: ll={fit.LogLikelihood:F4}, AIC={fit.AIC:F4}")
        Call DebugAudit.Test(std.Abs(fit.AIC - (6.0R - 2.0R * fit.LogLikelihood)) < 0.0000001R,
                            "AIC = 2*p - 2*logLik")
        Call DebugAudit.Test(Double.IsFinite(fit.PearsonChiSquare) AndAlso fit.PearsonChiSquare > 0.0R,
                            $"Pearson 卡方为有限正数: {fit.PearsonChiSquare:F4}")
        Call DebugAudit.Test(fit.NullDeviance >= fit.Deviance - 0.000001R,
                            $"零模型偏差不小于拟合模型偏差: {fit.NullDeviance:F4} >= {fit.Deviance:F4}")
        Call DebugAudit.Test(fit.ErrorTest.Length = rows.Length,
                            "每个样本都有一条逐点误差记录")
        Call DebugAudit.Test(MaxDiff(fit.ErrorTest.Select(Function(e) e.Yfit).ToArray(),
                                     fit.FittedValues) < 0.000000000001R,
                            "ErrorTest.Yfit 与 FittedValues 一致")

        Call DebugAudit.Mark("poisson.inference.maxStandardError", fit.StandardErrors.Max())
        Call DebugAudit.Mark("poisson.inference.irr.x1", fit.IncidenceRateRatios(1))
    End Sub

#End Region

#Region "offset"

    ''' <summary>暴露量标准化：拟合的是事件率而不是原始计数</summary>
    Private Sub CheckOffset()
        Dim n As Integer = 240
        Dim rows As Double()() = MakeRows(n)
        Dim rates As Double() = ExpectedRates(rows)
        Dim t As Double() = MakeExposures(n)
        Dim fit As PoissonFit = PoissonFitting.Fit(rows, ScaledCounts(rates, t), exposure:=t)

        Call DebugAudit.Test(fit.Converged, $"带 offset 的 IRLS 收敛: {fit.Iterations} 轮")
        Call DebugAudit.Test(fit.HasOffset, "结果对象记录了 offset 的使用")
        Call DebugAudit.AreEqual(n, fit.Exposures.Length, "结果对象保存了逐样本暴露量")
        Call DebugAudit.Test(MaxDiff(t, fit.Exposures) < 0.000000000001R, "暴露量被原样保留")
        Call DebugAudit.Test(std.Abs(fit.Intercept - TrueIntercept) < 0.03R,
                            $"offset 模型恢复截距: b0={fit.Intercept:F6}")
        Call DebugAudit.Test(std.Abs(fit.Coefficients(0) - TrueB1) < 0.03R,
                            $"offset 模型恢复 b1: {fit.Coefficients(0):F6}")
        Call DebugAudit.Test(std.Abs(fit.Coefficients(1) - TrueB2) < 0.03R,
                            $"offset 模型恢复 b2: {fit.Coefficients(1):F6}")

        Dim predictedRate As Double = fit.GetY(rows(7))
        Dim expectedCount As Double = fit.Predict(rows(7), exposure:=t(7))

        Call DebugAudit.Test(std.Abs(predictedRate - rates(7)) < 1.0R,
                            $"GetY 返回单位暴露量之下的事件率: {predictedRate:F4}")
        Call DebugAudit.Test(std.Abs(expectedCount - predictedRate * t(7)) < 0.000000001R,
                            $"Predict 返回乘上暴露量之后的期望计数: {expectedCount:F4}")
        Call DebugAudit.Test(std.Abs(fit.FittedValues(7) - expectedCount) < 0.000000001R,
                            "FittedValues 与 Predict(x, exposure) 一致")

        Dim noOffset As PoissonFit = PoissonFitting.Fit(rows, ScaledCounts(rates, t))

        Call DebugAudit.Test(Not noOffset.HasOffset, "不传 exposure 时 HasOffset 为 False")
        Call DebugAudit.Test(std.Abs(noOffset.Intercept - fit.Intercept) > 0.5R,
                            "忽略 offset 会把暴露量效应错误地吸收到截距里")

        Call DebugAudit.CheckThrows(Of ArgumentException)(
            Sub()
                Call fit.Predict(rows(0), exposure:=-1.0R)
            End Sub,
            "非正暴露量应被拒绝")

        Call DebugAudit.Mark("poisson.offset.intercept", fit.Intercept)
    End Sub

#End Region

#Region "diagnostics"

    ''' <summary>零膨胀式的交替计数数据应给出远大于 1 的离散比</summary>
    Private Sub CheckOverDispersion()
        Dim n As Integer = 200
        Dim rows As Double()() = New Double(n - 1)() {}
        Dim counts As Double() = New Double(n - 1) {}

        For i As Integer = 0 To n - 1
            rows(i) = New Double() {i / CDbl(n)}
            counts(i) = If(i Mod 2 = 0, 0.0R, 20.0R)
        Next

        Dim fit As PoissonFit = PoissonFitting.Fit(rows, counts)

        Call DebugAudit.Test(fit.Converged, $"过度离散数据仍可收敛: {fit.Iterations} 轮")
        Call DebugAudit.Test(fit.DispersionRatio > 1.5R,
                            $"残差偏差/自由度远大于 1: {fit.DispersionRatio:F4}")
        Call DebugAudit.Test(fit.IsOverDispersed, "IsOverDispersed 给出过度离散提示")
        Call DebugAudit.Test(fit.PearsonDispersion > 1.5R,
                            $"Pearson 卡方/自由度同样提示过度离散: {fit.PearsonDispersion:F4}")
        Call DebugAudit.Test(fit.FittedValues.All(Function(m) Double.IsFinite(m) AndAlso m > 0.0R),
                            "含大量零计数时预测值仍恒为正的有限数")
        Call DebugAudit.Test(fit.R2 >= 0.0R AndAlso fit.R2 <= 1.0R,
                            $"伪 R² 被裁剪到 [0,1]: {fit.R2:F6}")

        Call DebugAudit.Mark("poisson.diagnostics.dispersion", fit.DispersionRatio)
        Call DebugAudit.Mark("poisson.diagnostics.pearson", fit.PearsonDispersion)
    End Sub

#End Region

#Region "numeric table entry"

    ''' <summary>NumericTable 统一入口、特征列名、offset 列读取与预测写回</summary>
    Private Sub CheckTableEntry()
        Dim n As Integer = 240
        Dim rows As Double()() = MakeRows(n)
        Dim rates As Double() = ExpectedRates(rows)
        Dim t As Double() = MakeExposures(n)
        Dim table As New NumericTable(rows,
                                      Enumerable.Range(1, n).Select(Function(i) $"s{i}").ToArray(),
                                      New String() {"x1", "x2"})

        Call table.SetLabel("count", AsCounts(rates))
        Call table.SetLabel("exposure", t)

        Dim fit As PoissonFit = table.PoissonRegression(y:="count")

        Call DebugAudit.Test(fit.Converged, "表入口拟合收敛")
        Call DebugAudit.AreEqual(2, fit.Coefficients.Length, "表入口使用全部 2 个特征列")
        Call DebugAudit.AreEqual("(Intercept)", fit.FeatureNames(0), "第一个参数为自动补上的截距")
        Call DebugAudit.AreEqual("x1", fit.FeatureNames(1), "特征列名传入结果对象")
        Call DebugAudit.AreEqual("x2", fit.FeatureNames(2), "第二个特征列名传入结果对象")
        Call DebugAudit.Test(std.Abs(fit.Coefficients(0) - TrueB1) < 0.03R,
                            $"表入口恢复 b1: {fit.Coefficients(0):F6}")

        Dim predicted As NumericTable = table.SetPrediction(fit, name:="poisson_fit", withResidual:=True)
        Dim rawRows As Double()() = table.NumericRows

        Call DebugAudit.Test(predicted.HasLabel("poisson_fit"), "SetPrediction 写入预测列")
        Call DebugAudit.Test(predicted.HasLabel("residual"), "SetPrediction 写入残差列")
        Call DebugAudit.Test(MaxDiff(predicted.GetLabel("poisson_fit"),
                                     rawRows.Select(Function(r) fit.GetY(r)).ToArray()) < 0.000000001R,
                            "写回的预测值等于 IFitted.GetY")

        ' 暴露量从标签列读取；同时检验另一标签列作为响应变量
        Call table.SetLabel("scaled_count", ScaledCounts(rates, t))

        Dim offsetFit As PoissonFit = table.PoissonRegression(y:="scaled_count", exposure:="exposure")

        Call DebugAudit.Test(offsetFit.HasOffset, "表入口从标签列读取暴露量")
        Call DebugAudit.Test(std.Abs(offsetFit.Coefficients(0) - TrueB1) < 0.03R,
                            $"表入口 offset 模型恢复 b1: {offsetFit.Coefficients(0):F6}")

        ' 暴露量也可以放在特征列里。x1/x2 含 0，不能作为暴露量，
        ' 因此单独造一份带正数 ``offset_col`` 特征列的表
        Dim featureRows As Double()() = rows _
            .Select(Function(r, i) New Double() {r(0), r(1), t(i)}) _
            .ToArray()
        Dim featureTable As New NumericTable(featureRows,
                                             Enumerable.Range(1, n).Select(Function(i) $"f{i}").ToArray(),
                                             New String() {"x1", "x2", "offset_col"})

        Call featureTable.SetLabel("scaled_count", ScaledCounts(rates, t))

        Dim featureOffset As PoissonFit = featureTable.PoissonRegression(y:="scaled_count", exposure:="offset_col")

        Call DebugAudit.Test(featureOffset.HasOffset, "表入口也允许从特征列读取暴露量")
        Call DebugAudit.Test(featureOffset.Converged, "从特征列读取暴露量时拟合仍然收敛")

        Call DebugAudit.CheckThrows(Of ArgumentException)(
            Sub()
                Call table.PoissonRegression(y:="scaled_count", exposure:="no_such_column")
            End Sub,
            "不存在的暴露量列应给出明确异常")

        Call DebugAudit.Mark("poisson.table.nsamples", n)
    End Sub

#End Region

#Region "contract"

    ''' <summary>IFitted / Formula 契约：R2、Polynomial、GetY 与公式文本</summary>
    Private Sub CheckContract()
        Dim rows As Double()() = MakeRows(120)
        Dim fit As PoissonFit = PoissonFitting.Fit(rows, AsCounts(ExpectedRates(rows)))
        Dim fitted As IFitted = fit

        Call DebugAudit.Test(std.Abs(fitted.R2 - fit.PseudoR2) < 0.000000000001R,
                            "IFitted.R2 就是基于偏差的伪 R²，而不是写死的 1")
        Call DebugAudit.Test(fit.R2 >= 0.0R AndAlso fit.R2 <= 1.0R,
                            $"R2 落在 [0,1]: {fit.R2:F6}")
        Call DebugAudit.AreEqual(2, fitted.Polynomial.Factors.Length,
                            "Polynomial.Factors 与斜率系数一一对应")
        Call DebugAudit.Test(std.Abs(fitted.Polynomial.Evaluate(rows(3)) - fit.GetY(rows(3))) < 0.000000000001R,
                            "Polynomial.Evaluate 与 GetY 语义一致")
        Call DebugAudit.Test(fit.GetY(rows(3)) > 0.0R AndAlso Double.IsFinite(fit.GetY(rows(3))),
                            "对数链接保证预测值恒为正的有限数")
        Call DebugAudit.Test(fitted.Polynomial.ToString().StartsWith("exp("),
                            "公式文本体现对数链接 exp(...)")
        Call DebugAudit.Test(fit.ToString().Contains("Poisson"),
                            "ToString 输出模型摘要")

        Call DebugAudit.CheckThrows(Of ArgumentException)(
            Sub()
                Call fit.GetY(1.0R)
            End Sub,
            "自变量数量不足时应给出明确异常")
    End Sub

#End Region

#Region "boundary"

    ''' <summary>输入校验：空集、维度、非法计数、非法暴露量与非法求解参数</summary>
    Private Sub CheckBoundary()
        Dim rows As Double()() = MakeRows(60)
        Dim counts As Double() = AsCounts(ExpectedRates(rows))

        Call DebugAudit.CheckThrows(Of ArgumentException)(
            Sub()
                Call PoissonFitting.Fit(Array.Empty(Of Double())(), Array.Empty(Of Double)())
            End Sub,
            "空样本集应抛出 ArgumentException")

        Call DebugAudit.CheckThrows(Of ArgumentException)(
            Sub()
                Call PoissonFitting.Fit(Nothing, counts)
            End Sub,
            "空特征矩阵应抛出 ArgumentException")

        Dim ragged As Double()() = {New Double() {1.0R, 2.0R}, New Double() {1.0R}}

        Call DebugAudit.CheckThrows(Of ArgumentException)(
            Sub()
                Call PoissonFitting.Fit(ragged, New Double() {3.0R, 4.0R})
            End Sub,
            "特征行长度不一致应被拒绝")

        Call DebugAudit.CheckThrows(Of ArgumentException)(
            Sub()
                Call PoissonFitting.Fit(rows, New Double() {1.0R, 2.0R})
            End Sub,
            "特征矩阵行数与响应变量长度不匹配应被拒绝")

        Dim negative As Double() = CType(counts.Clone(), Double())
        negative(0) = -1.0R

        Call DebugAudit.CheckThrows(Of ArgumentException)(
            Sub()
                Call PoissonFitting.Fit(rows, negative)
            End Sub,
            "负计数应被拒绝")

        Dim fractional As Double() = CType(counts.Clone(), Double())
        fractional(0) = 3.5R

        Call DebugAudit.CheckThrows(Of ArgumentException)(
            Sub()
                Call PoissonFitting.Fit(rows, fractional)
            End Sub,
            "非整数计数应被拒绝")

        Dim notFinite As Double() = CType(counts.Clone(), Double())
        notFinite(0) = Double.NaN

        Call DebugAudit.CheckThrows(Of ArgumentException)(
            Sub()
                Call PoissonFitting.Fit(rows, notFinite)
            End Sub,
            "NaN 计数应被拒绝")

        Dim allZero As Double() = Enumerable.Repeat(0.0R, rows.Length).ToArray()

        Call DebugAudit.CheckThrows(Of ArgumentException)(
            Sub()
                Call PoissonFitting.Fit(rows, allZero)
            End Sub,
            "全零响应变量没有有限 MLE，应被拒绝")

        Dim badX As Double()() = MakeRows(60)
        badX(0)(0) = Double.PositiveInfinity

        Call DebugAudit.CheckThrows(Of ArgumentException)(
            Sub()
                Call PoissonFitting.Fit(badX, counts)
            End Sub,
            "非有限特征值应被拒绝")

        Dim badExposure As Double() = MakeExposures(rows.Length)
        badExposure(0) = 0.0R

        Call DebugAudit.CheckThrows(Of ArgumentException)(
            Sub()
                Call PoissonFitting.Fit(rows, counts, exposure:=badExposure)
            End Sub,
            "非正暴露量应被拒绝")

        Call DebugAudit.CheckThrows(Of ArgumentException)(
            Sub()
                Call PoissonFitting.Fit(rows, counts, exposure:=New Double() {1.0R, 2.0R})
            End Sub,
            "暴露量长度不匹配应被拒绝")

        Call DebugAudit.CheckThrows(Of ArgumentException)(
            Sub()
                Call PoissonFitting.Fit(rows, counts, maxIterations:=0)
            End Sub,
            "非正迭代上限应被拒绝")

        Call DebugAudit.CheckThrows(Of ArgumentException)(
            Sub()
                Call PoissonFitting.Fit(rows, counts, tolerance:=0.0R)
            End Sub,
            "非正收敛容差应被拒绝")

        Call DebugAudit.CheckThrows(Of ArgumentException)(
            Sub()
                Call PoissonFitting.Fit(rows, counts, featureNames:=New String() {"only_one"})
            End Sub,
            "特征名数量不匹配应被拒绝")

        Call DebugAudit.CheckThrows(Of ArgumentException)(
            Sub()
                Call PoissonFitting.Fit(New Double()() {New Double() {1.0R, 2.0R}}, New Double() {3.0R})
            End Sub,
            "样本数不足以估计全部参数时应被拒绝")
    End Sub

#End Region

#Region "convergence"

    ''' <summary>迭代上限耗尽时如实报告未收敛，而不是静默返回</summary>
    Private Sub CheckConvergence()
        Dim rows As Double()() = MakeRows(200)
        Dim counts As Double() = AsCounts(ExpectedRates(rows))
        Dim truncated As PoissonFit = PoissonFitting.Fit(rows, counts, maxIterations:=1)
        Dim full As PoissonFit = PoissonFitting.Fit(rows, counts, maxIterations:=100)

        Call DebugAudit.Test(Not truncated.Converged, "只迭代 1 轮时 Converged = False")
        Call DebugAudit.AreEqual(1, truncated.Iterations, "实际迭代轮数等于上限 1")
        Call DebugAudit.Test(full.Converged, "放开迭代上限后收敛")
        Call DebugAudit.Test(full.LogLikelihood > truncated.LogLikelihood,
                            $"继续迭代确实提高了对数似然: {truncated.LogLikelihood:F4} -> {full.LogLikelihood:F4}")

        Dim elapsed As Double = 0.0R

        Using scope = DebugAudit.Watch("poisson.fit.100iterations", logOnExit:=False)
            Call PoissonFitting.Fit(rows, counts, maxIterations:=100)
            elapsed = scope.ElapsedMilliseconds
        End Using

        Call DebugAudit.Mark("poisson.convergence.elapsedMs", elapsed)
    End Sub

#End Region

End Module
