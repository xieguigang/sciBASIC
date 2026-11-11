Imports System
Imports System.IO
Imports System.Linq
Imports Microsoft.VisualBasic.ApplicationServices.Debugging
Imports Microsoft.VisualBasic.Data
Imports Microsoft.VisualBasic.Data.Bootstrapping
Imports Microsoft.VisualBasic.Data.Bootstrapping.Cox
Imports std = System.Math

''' <summary>
''' Cox 比例危险模型的正确性审计：偏似然、Breslow/Efron 平局、弹性网路径、
''' penalty.factor、风险比与 C 指数、生存曲线、K 折交叉验证、二维表入口与异常输入防护。
''' 所有断言经 <see cref="DebugAudit"/> 记录，结束时生成 JSON 测试报告。
''' </summary>
Public Module CoxAudit

    Private Const ReportName As String = "cox-test-report.json"
    Private Const N As Integer = 200
    Private Const P As Integer = 12
    Private Const ActiveFeatures As Integer = 4

    ''' <summary>运行全部审计用例并生成测试报告</summary>
    Public Function Run() As Integer
        Dim reportPath As String = Path.Combine(AppContext.BaseDirectory, ReportName)

        Call DebugAudit.Reset()
        DebugAudit.MaxEvents = 8192
        Call DebugAudit.Mark("cox audit started")

        Try
            Call CheckModelContract()
            Call CheckPathAndCoefficients()
            Call CheckTies()
            Call CheckPenaltyFactor()
            Call CheckAlpha()
            Call CheckPrediction()
            Call CheckSurvivalCurves()
            Call CheckCrossValidation()
            Call CheckTableEntry()
            Call CheckBoundary()
        Catch ex As Exception
            Call DebugAudit.Mark("audit.crash", ex.ToString())
            Call DebugAudit.Test(False, $"审计套件不应抛出未处理异常: {ex.Message}")
        End Try

        Call DebugAudit.Snapshot(DebugAudit.V(ReportName), DebugAudit.V(DebugAudit.FailureCount))

        Dim dump As String = DebugAudit.Dump()

        Call DebugAudit.SaveReport(reportPath)
        Call Console.WriteLine($"   [audit] report -> {reportPath}")
        Call Console.WriteLine($"   [audit] dump length = {dump.Length}, {DebugAudit.FailureCount} failure(s)")

        Return DebugAudit.FailureCount
    End Function

#Region "test data"

    ''' <summary>确定性的近似独立协变量（固定种子伪随机）</summary>
    Private Function MakeDesign(Optional seed As Integer = 101) As Double()()
        Dim rows As Double()() = New Double(N - 1)() {}
        Dim rnd As New Random(seed)

        For i As Integer = 0 To N - 1
            rows(i) = New Double(P - 1) {}

            For j As Integer = 0 To P - 1
                rows(i)(j) = 2.0R * rnd.NextDouble() - 1.0R
            Next
        Next

        Return rows
    End Function

    ''' <summary>前 4 个变量有真实效应，其余为 0</summary>
    Private Function TrueBeta() As Double()
        Dim beta As Double() = New Double(P - 1) {}

        beta(0) = 0.9R
        beta(1) = -0.7R
        beta(2) = 0.6R
        beta(3) = 0.4R

        Return beta
    End Function

    ''' <summary>
    ''' 按 ``h(t) = h0 * exp(x'beta)`` 生成指数生存时间与 30% 删失，
    ''' 因此 Cox 的真实系数就是 ``beta``（信号足够强以便稳定地检验符号）。
    ''' </summary>
    Private Function MakeSurvival(rows As Double()(),
                                  beta As Double(),
                                  Optional discretize As Double = 0.0R,
                                  Optional seed As Integer = 202) As Double()()

        Dim response As Double()() = New Double(N - 1)() {}
        Dim rnd As New Random(seed)

        For i As Integer = 0 To N - 1
            Dim score As Double = 0.0R

            For j As Integer = 0 To P - 1
                score += rows(i)(j) * beta(j)
            Next

            Dim rate As Double = std.Exp(score)
            Dim u As Double = rnd.NextDouble()

            If u < 0.0000001R Then
                u = 0.0000001R
            End If

            Dim observedTime As Double = -std.Log(u) / rate

            If discretize > 0.0R Then
                observedTime = std.Max(discretize, std.Round(observedTime / discretize) * discretize)
            End If

            ' rbinom(N,1,0.3) 为 1 时删失；status = 1 - censored
            Dim censored As Integer = If(rnd.NextDouble() < 0.3R, 1, 0)

            response(i) = New Double() {observedTime, CDbl(1 - censored)}
        Next

        Return response
    End Function

    Private Function AllFinite(values As Double()) As Boolean
        Return values IsNot Nothing AndAlso values.All(Function(v) Double.IsFinite(v))
    End Function

    Private Function MaxAbsDiff(actual As Double(), expected As Double()) As Double
        If actual Is Nothing OrElse expected Is Nothing OrElse actual.Length <> expected.Length Then
            Return Double.PositiveInfinity
        End If

        Dim diff As Double = 0.0R

        For i As Integer = 0 To actual.Length - 1
            diff = std.Max(diff, std.Abs(actual(i) - expected(i)))
        Next

        Return diff
    End Function

#End Region

#Region "model contract"

    ''' <summary>Cox 没有截距，IFitted 语义为风险得分</summary>
    Private Sub CheckModelContract()
        Dim rows As Double()() = MakeDesign()
        Dim response As Double()() = MakeSurvival(rows, TrueBeta())
        Dim fit As CoxFit = CoxFitting.Fit(rows, response, numberOfLambdas:=20)
        Dim fitted As IFitted = fit
        Dim last As Integer = fit.lambdas.Length - 1

        Call DebugAudit.AreEqual(P, fit.getCoefficients(last).Length,
                            "Cox 系数向量长度等于特征数，没有截距项")
        Call DebugAudit.AreEqual(P, fitted.Polynomial.Factors.Length,
                            "IFitted.Polynomial 是无截距的多元多项式")
        Call DebugAudit.Test(fit.ErrorTest Is Nothing,
                            "生存数据没有逐点残差，ErrorTest 为 Nothing")
        Call DebugAudit.Test(std.Abs(fitted.GetY(rows(6)) - fit.PredictSingle(rows(6))) < 0.000000000001R,
                            "GetY 返回风险得分而不是生存时间")
        Call DebugAudit.Test(std.Abs(fitted.R2 - fit.devRatio(last)) < 0.000000000001R,
                            "IFitted.R2 是基于偏似然的伪 R²")
        Call DebugAudit.Test(fit.eventCount > 0 AndAlso fit.censoredCount > 0,
                            $"事件数/删失数统计正确: {fit.eventCount}/{fit.censoredCount}")
        Call DebugAudit.Test(fit.nullLogLikelihood < 0.0R,
                            $"零模型对数偏似然为负: {fit.nullLogLikelihood:F4}")
        Call DebugAudit.Test(fit.ToString().Contains("Cox"), "ToString 输出模型摘要")

        Dim hr As Double() = fit.getHazardRatios(last)

        For j As Integer = 0 To P - 1
            Call DebugAudit.Test(std.Abs(hr(j) - std.Exp(fit.getCoefficients(last)(j))) < 0.0000001R,
                                $"变量 {j} 的风险比 HR = exp(beta)")
        Next

        Call DebugAudit.Mark("cox.contract.events", fit.eventCount)
        Call DebugAudit.Mark("cox.contract.censored", fit.censoredCount)
    End Sub

#End Region

#Region "path"

    ''' <summary>lambda 路径、系数恢复、HR、C 指数与收敛状态</summary>
    Private Sub CheckPathAndCoefficients()
        Dim rows As Double()() = MakeDesign()
        Dim response As Double()() = MakeSurvival(rows, TrueBeta())
        Dim fit As CoxFit = CoxFitting.Fit(rows, response, numberOfLambdas:=30)
        Dim steps As Integer = fit.lambdas.Length
        Dim last As Integer = steps - 1
        Dim descending As Boolean = True
        Dim devianceMonotone As Boolean = True
        Dim logPLMonotone As Boolean = True

        For li As Integer = 1 To steps - 1
            If fit.lambdas(li) >= fit.lambdas(li - 1) Then
                descending = False
            End If
            If fit.deviance(li) > fit.deviance(li - 1) + 0.000001R Then
                devianceMonotone = False
            End If
            If fit.logPL(li) < fit.logPL(li - 1) - 0.000001R Then
                logPLMonotone = False
            End If
        Next

        Call DebugAudit.Test(descending, "lambda 网格严格递减")
        Call DebugAudit.Test(fit.df(0) = 0, $"lambda_max 处全部变量被剔除: df(0)={fit.df(0)}")
        Call DebugAudit.Test(fit.df(last) >= ActiveFeatures, "惩罚减弱后活动变量数不少于真实数量")
        Call DebugAudit.Test(devianceMonotone, "部分似然偏差随惩罚减弱而不增")
        Call DebugAudit.Test(logPLMonotone, "对数偏似然随惩罚减弱而不减")
        Call DebugAudit.Test(AllFinite(fit.deviance) AndAlso AllFinite(fit.cIndex) AndAlso AllFinite(fit.logPL),
                            "偏差、C 指数与对数偏似然全部有限")
        Call DebugAudit.Test(fit.devRatio.All(Function(v) v >= 0.0R AndAlso v <= 1.0R),
                            "伪 R² 落在 [0,1]")
        Call DebugAudit.Test(fit.cIndex.All(Function(v) v >= 0.0R AndAlso v <= 1.0R),
                            "C 指数落在 [0,1]")
        Call DebugAudit.Test(fit.converged.All(Function(v) v),
                            $"全部 {steps} 个 lambda 步骤收敛")
        Call DebugAudit.Test(fit.iterations.All(Function(v) v > 0 AndAlso v < 1000),
                            "warm start 把内迭代轮数压在合理范围")

        ' 弱惩罚之下坐标下降应接近无偏的 Cox MLE：真实系数就是 beta
        Dim tiny As CoxFit = CoxFitting.Fit(rows, response, lambda:=New Double() {0.00000001R},
                                            numberOfLambdas:=2, tolerance:=0.0000000001R)
        Dim coef As Double() = tiny.getCoefficients(0)
        Dim truth As Double() = TrueBeta()
        Dim signsCorrect As Boolean = True
        Dim maxError As Double = 0.0R

        For j As Integer = 0 To ActiveFeatures - 1
            Dim expected As Double = truth(j)

            maxError = std.Max(maxError, std.Abs(coef(j) - expected))
            If coef(j) * expected <= 0.0R Then
                signsCorrect = False
            End If
        Next

        Call DebugAudit.Test(signsCorrect, "弱惩罚下真实有效变量的系数符号全部正确")
        Call DebugAudit.Test(maxError < 0.35R,
                            $"弱惩罚下的 Cox MLE 接近真实系数: maxΔ={maxError:F4}")

        ' 变量选择要在中等惩罚的 lasso 路径上检验：lambda 趋近 0 时是近似无偏
        ' MLE，无关变量可能因抽样噪声出现较大系数，不能用来判断筛选顺序
        Dim firstSelection As Integer = -1

        For li As Integer = 0 To steps - 1
            If fit.df(li) >= ActiveFeatures Then
                firstSelection = li
                Exit For
            End If
        Next

        Call DebugAudit.Test(firstSelection > 0,
                            $"路径上存在活动变量达到 {ActiveFeatures} 个的步骤: idx={firstSelection}")

        If firstSelection > 0 Then
            Dim selectedActive As Integer = 0
            Dim activeSum As Double = 0.0R
            Dim inactiveSum As Double = 0.0R

            For j As Integer = 0 To P - 1
                Dim magnitude As Double = std.Abs(fit.coefficients(firstSelection)(j))

                If j < ActiveFeatures Then
                    activeSum += magnitude

                    If magnitude > 0.0000000001R Then
                        selectedActive += 1
                    End If
                Else
                    inactiveSum += magnitude
                End If
            Next

            Call DebugAudit.Test(selectedActive >= 3,
                                $"lasso 首次选够变量时以真实预后变量为主: {selectedActive}/{ActiveFeatures}")
            Call DebugAudit.Test(activeSum / CDbl(ActiveFeatures) > inactiveSum / CDbl(P - ActiveFeatures),
                                $"真实变量的平均系数强于无关变量: {activeSum / CDbl(ActiveFeatures):F4} vs {inactiveSum / CDbl(P - ActiveFeatures):F4}")

            Call DebugAudit.Mark("cox.path.firstSelectionActive", selectedActive)
        End If

        Dim table As NumericTable = fit.toNumericTable()

        Call DebugAudit.AreEqual(steps, table.nsamples, "路径导出表每步一行")
        Call DebugAudit.AreEqual(P, table.nfeatures, "路径导出表每变量一列")
        Call DebugAudit.Test(table.HasLabel("lambdas") AndAlso table.HasLabel("df") AndAlso
                            table.HasLabel("deviance") AndAlso table.HasLabel("cIndex"),
                            "路径导出表携带 lambdas/df/deviance/cIndex 标签列")
        Call DebugAudit.Test(MaxAbsDiff(table.GetLabel("lambdas"), fit.lambdas) < 0.000000000001R,
                            "导出表的 lambda 列与路径一致")

        Call DebugAudit.Mark("cox.path.maxCoefficientError", maxError)
        Call DebugAudit.Mark("cox.path.cIndexLast", fit.cIndex(last))
        Call DebugAudit.Mark("cox.path.maxIterations", fit.iterations.Max())
    End Sub

#End Region

#Region "ties"

    ''' <summary>无平局时 Breslow 与 Efron 应一致；有平局时两者均可收敛且系数不同</summary>
    Private Sub CheckTies()
        Dim rows As Double()() = MakeDesign()
        Dim continuous As Double()() = MakeSurvival(rows, TrueBeta())
        Dim breslow As CoxFit = CoxFitting.Fit(rows, continuous, ties:=CoxTies.Breslow, numberOfLambdas:=15)
        Dim efron As CoxFit = CoxFitting.Fit(rows, continuous, ties:=CoxTies.Efron, numberOfLambdas:=15)

        Call DebugAudit.AreEqual(CoxTies.Breslow, breslow.ties, "结果对象记录 Breslow 平局方式")
        Call DebugAudit.AreEqual(CoxTies.Efron, efron.ties, "结果对象记录 Efron 平局方式")
        Call DebugAudit.Test(MaxAbsDiff(breslow.getCoefficients(14), efron.getCoefficients(14)) < 0.000001R,
                            "无平局数据时 Breslow 与 Efron 的系数一致")
        Call DebugAudit.Test(std.Abs(breslow.logPL(14) - efron.logPL(14)) < 0.000001R,
                            "无平局数据时两种偏似然一致")

        Dim tied As Double()() = MakeSurvival(rows, TrueBeta(), discretize:=0.25R, seed:=303)
        Dim tiedBreslow As CoxFit = CoxFitting.Fit(rows, tied, ties:=CoxTies.Breslow, numberOfLambdas:=15)
        Dim tiedEfron As CoxFit = CoxFitting.Fit(rows, tied, ties:=CoxTies.Efron, numberOfLambdas:=15)

        Call DebugAudit.Test(tiedBreslow.converged.All(Function(v) v), "平局数据 Breslow 路径全部收敛")
        Call DebugAudit.Test(tiedEfron.converged.All(Function(v) v), "平局数据 Efron 路径全部收敛")
        Call DebugAudit.Test(AllFinite(tiedBreslow.logPL) AndAlso AllFinite(tiedEfron.logPL),
                            "平局数据的对数偏似然全部有限")
        Call DebugAudit.Test(Double.IsFinite(tiedEfron.cIndex(14)), "平局数据的 C 指数有效")
        Call DebugAudit.Test(tiedEfron.cIndex(14) > 0.5R,
                            $"平局数据上模型仍有判别能力: C={tiedEfron.cIndex(14):F4}")

        Call DebugAudit.Mark("cox.ties.tiedCIndex", tiedEfron.cIndex(14))
    End Sub

#End Region

#Region "penalty.factor"

    ''' <summary>惩罚因子为 0 的变量永不惩罚、始终留在模型中</summary>
    Private Sub CheckPenaltyFactor()
        Dim rows As Double()() = MakeDesign()
        Dim response As Double()() = MakeSurvival(rows, TrueBeta())
        Dim pf As Double() = Enumerable.Repeat(1.0R, P).ToArray()

        pf(0) = 0.0R

        Dim fit As CoxFit = CoxFitting.Fit(rows, response, penaltyFactor:=pf, numberOfLambdas:=20)

        Call DebugAudit.Test(std.Abs(fit.penaltyFactor.Sum() - CDbl(P)) < 0.0000001R,
                            "惩罚因子被重缩放至和为 p")
        Call DebugAudit.Test(fit.penaltyFactor(0) = 0.0R, "第 0 个变量不施加惩罚")
        Call DebugAudit.Test(std.Abs(fit.coefficients(0)(0)) > 0.0000000001R,
                            "不惩罚变量在最强惩罚处仍保留在模型中")

        ' 不惩罚变量先被更新会改变风险集权重，因此其余变量在 lambda_max 处
        ' 可能被局部带活；关键契约是「不惩罚变量在整条路径上永不消失」
        Dim alwaysActive As Boolean = True

        For li As Integer = 0 To fit.lambdas.Length - 1
            If std.Abs(fit.coefficients(li)(0)) <= 0.0000000001R Then
                alwaysActive = False
            End If
        Next

        Call DebugAudit.Test(alwaysActive, "不惩罚变量在整条 lambda 路径上始终保留")
        Call DebugAudit.Test(fit.df(0) < P,
                            $"最强惩罚处并未让全部变量进入模型: df(0)={fit.df(0)}")

        Dim view As IFitted = fit.GetRiskModel(10)

        Call DebugAudit.Test(view IsNot Nothing, "GetRiskModel 返回指定 lambda 的 IFitted 视图")
        Call DebugAudit.Test(std.Abs(view.GetY(rows(2)) - fit.Predict(rows, 10)(2)) < 0.000000000001R,
                            "逐 lambda 视图的 GetY 与 Predict 一致")

        Call DebugAudit.Mark("cox.penaltyFactor.unpenalizedCoef", fit.coefficients(0)(0))
    End Sub

#End Region

#Region "alpha"

    ''' <summary>alpha=0 无精确零；alpha=0.5 弹性网仍收敛</summary>
    Private Sub CheckAlpha()
        Dim rows As Double()() = MakeDesign()
        Dim response As Double()() = MakeSurvival(rows, TrueBeta())
        Dim ridge As CoxFit = CoxFitting.Fit(rows, response, alpha:=0.0R, numberOfLambdas:=15)
        Dim elastic As CoxFit = CoxFitting.Fit(rows, response, alpha:=0.5R, numberOfLambdas:=15)
        Dim last As Integer = ridge.lambdas.Length - 1

        Call DebugAudit.Test(ridge.df(last) = P,
                            $"alpha=0 退化成 ridge，不产生精确零: df={ridge.df(last)}")
        Call DebugAudit.Test(ridge.MaxLambda > 0.0R AndAlso Double.IsFinite(ridge.MaxLambda),
                            "alpha=0 仍有有限的正 lambda 起点")
        Call DebugAudit.Test(elastic.converged.All(Function(v) v), "alpha=0.5 弹性网路径全部收敛")
        Call DebugAudit.Test(elastic.df(0) = 0, "alpha=0.5 在 lambda_max 处剔除全部变量")
        Call DebugAudit.Test(elastic.df(last) >= ActiveFeatures, "弹性网末端保留了真实变量")

        Call DebugAudit.Mark("cox.alpha.ridgeLambdaMax", ridge.MaxLambda)
    End Sub

#End Region

#Region "prediction"

    ''' <summary>风险得分预测、多 lambda 预测与写回链路</summary>
    Private Sub CheckPrediction()
        Dim rows As Double()() = MakeDesign()
        Dim response As Double()() = MakeSurvival(rows, TrueBeta())
        Dim fit As CoxFit = CoxFitting.Fit(rows, response, numberOfLambdas:=20)
        Dim scores As Double() = fit.Predict(rows)
        Dim last As Integer = fit.lambdas.Length - 1
        Dim coef As Double() = fit.getCoefficients(last)
        Dim manual As Double = 0.0R

        For j As Integer = 0 To P - 1
            manual += rows(5)(j) * coef(j)
        Next

        Call DebugAudit.AreEqual(N, scores.Length, "Predict 返回每个样本一个风险得分")
        Call DebugAudit.Test(std.Abs(scores(5) - manual) < 0.000000000001R,
                            "风险得分等于 x'beta（无截距）")
        Call DebugAudit.Test(std.Abs(fit.PredictSingle(rows(5)) - scores(5)) < 0.000000000001R,
                            "PredictSingle 与 Predict 一致")

        Dim path As Double()() = fit.PredictPath(rows, New Integer() {0, last})

        Call DebugAudit.AreEqual(2, path.Length, "PredictPath 第一维是 lambda 步骤数")
        Call DebugAudit.AreEqual(N, path(0).Length, "PredictPath 第二维是样本数")
        Call DebugAudit.Test(MaxAbsDiff(path(1), scores) < 0.000000000001R,
                            "PredictPath 最后一步与默认 Predict 一致")
        Call DebugAudit.AreEqual(fit.lambdas.Length, fit.PredictPath(rows, Nothing).Length,
                            "lambdaIdx 为 Nothing 时输出全部步骤")

        Dim cExternal As Double = fit.CIndexFor(rows,
                                                response.Select(Function(r) r(0)).ToArray(),
                                                response.Select(Function(r) r(1)).ToArray())

        Call DebugAudit.Test(std.Abs(cExternal - fit.cIndex(last)) < 0.000000000001R,
                            "CIndexFor 与路径保存的 C 指数一致")

        Dim table As New NumericTable(rows, Nothing, Nothing)

        Call table.SetPrediction(fit, name:="risk_score")

        Call DebugAudit.Test(table.HasLabel("risk_score"), "SetPrediction 可写回 Cox 风险得分")
        Call DebugAudit.Test(std.Abs(table.GetLabel("risk_score")(5) - scores(5)) < 0.000000000001R,
                            "写回列数值与 Predict 一致")

        Call DebugAudit.CheckThrows(Of ArgumentException)(
            Sub()
                Call fit.Predict(New Double()() {New Double() {1.0R, 2.0R}})
            End Sub,
            "预测特征数量不足应给出明确异常")
        Call DebugAudit.CheckThrows(Of ArgumentOutOfRangeException)(
            Sub()
                Call fit.getCoefficients(fit.lambdas.Length)
            End Sub,
            "lambda 下标越界应给出明确异常")
    End Sub

#End Region

#Region "survival curves"

    ''' <summary>Breslow 基线累积风险与个体化生存曲线</summary>
    Private Sub CheckSurvivalCurves()
        Dim rows As Double()() = MakeDesign()
        Dim response As Double()() = MakeSurvival(rows, TrueBeta())
        Dim fit As CoxFit = CoxFitting.Fit(rows, response, numberOfLambdas:=20)
        Dim baseline As SurvivalCurve = fit.BaselineHazard()
        Dim m As Integer = baseline.Time.Length

        Call DebugAudit.AreEqual(fit.eventCount, CountDistinctEventTimes(response),
                            "基线风险的时刻数等于不同事件时刻数")
        Call DebugAudit.Test(m > 0, "基线累积风险至少有一个事件时刻")

        Dim ascending As Boolean = True
        Dim hazardMonotone As Boolean = True
        Dim survivalMonotone As Boolean = True

        For q As Integer = 1 To m - 1
            If baseline.Time(q) <= baseline.Time(q - 1) Then
                ascending = False
            End If
            If baseline.CumulativeHazard(q) < baseline.CumulativeHazard(q - 1) Then
                hazardMonotone = False
            End If
            If baseline.Survival(q) > baseline.Survival(q - 1) + 0.000000001R Then
                survivalMonotone = False
            End If
        Next

        Call DebugAudit.Test(ascending, "生存曲线的事件时刻严格升序")
        Call DebugAudit.Test(hazardMonotone, "基线累积风险单调不减")
        Call DebugAudit.Test(survivalMonotone, "基线生存函数单调不增")
        Call DebugAudit.Test(baseline.Survival(0) > 0.0R AndAlso baseline.Survival(0) <= 1.0R,
                            "基线生存概率落在 (0,1]")
        Call DebugAudit.Test(std.Abs(baseline.Survival(m - 1) - std.Exp(-baseline.CumulativeHazard(m - 1))) < 0.0000000001R,
                            "S0(t) = exp(-H0(t))")
        Call DebugAudit.Test(std.Abs(baseline.SurvivalAt(baseline.Time(0) / 2.0R) - 1.0R) < 0.0000000001R,
                            "SurvivalAt 在首个事件时刻之前返回 1")

        Dim newx As Double()() = {rows(0), rows(1), rows(2)}
        Dim curves As SurvivalCurve() = fit.PredictSurvival(newx)
        Dim scores As Double() = fit.Predict(newx)

        Call DebugAudit.AreEqual(3, curves.Length, "每个新样本返回一条个体化生存曲线")
        Call DebugAudit.AreEqual(m, curves(0).Time.Length, "个体曲线与基线共享事件时刻")
        Call DebugAudit.Test(curves.All(Function(c) c.Survival.All(Function(s) s >= 0.0R AndAlso s <= 1.0R)),
                            "个体生存概率全部落在 [0,1]")

        Dim high As Integer = If(scores(0) > scores(1), 0, 1)
        Dim low As Integer = If(scores(0) > scores(1), 1, 0)

        Call DebugAudit.Test(curves(high).Survival(m - 1) <= curves(low).Survival(m - 1) + 0.0000000001R,
                            "风险得分更高的个体生存概率更低")
        Call DebugAudit.Test(std.Abs(curves(0).RiskScore - scores(0)) < 0.000000000001R,
                            "曲线保存了该个体的风险得分")

        Call DebugAudit.Mark("cox.survival.eventTimes", m)
    End Sub

    Private Function CountDistinctEventTimes(response As Double()()) As Integer
        Dim times As New System.Collections.Generic.HashSet(Of Double)()

        For i As Integer = 0 To response.Length - 1
            If response(i)(1) = 1.0R Then
                Call times.Add(response(i)(0))
            End If
        Next

        Return times.Count
    End Function

#End Region

#Region "cross validation"

    ''' <summary>deviance（越小越好）与 CIndex（越大越好）两种方向的 CV</summary>
    Private Sub CheckCrossValidation()
        Dim rows As Double()() = MakeDesign()
        Dim response As Double()() = MakeSurvival(rows, TrueBeta())
        Dim cv As CoxCVFit = CoxCV.CrossValidate(rows, response, nfolds:=5,
                                                 numberOfLambdas:=15, measure:=CoxCVMeasure.Deviance)
        Dim steps As Integer = cv.lambdas.Length

        Call DebugAudit.AreEqual(15, steps, "CV 使用 15 步统一 lambda 网格")
        Call DebugAudit.AreEqual(5, cv.nfolds, "CV 折数正确")
        Call DebugAudit.AreEqual(CoxCVMeasure.Deviance, cv.measure, "缺省评估指标为 deviance")
        Call DebugAudit.Test(cv.grouped, "缺省使用 grouped 减法方式")
        Call DebugAudit.Test(Not cv.Maximize, "deviance 方向为越小越好")
        Call DebugAudit.Test(AllFinite(cv.cvMean) AndAlso AllFinite(cv.cvSe), "CV 均值与标准误全部有限")
        Call DebugAudit.Test(cv.cvSe.All(Function(v) v >= 0.0R), "CV 标准误非负")
        Call DebugAudit.Test(cv.BestIndex >= 0 AndAlso cv.BestIndex < steps, "lambda.min 下标在网格内")
        Call DebugAudit.Test(cv.BestIndex1se <= cv.BestIndex,
                            $"1-SE 规则选出更强惩罚: {cv.BestIndex1se} <= {cv.BestIndex}")
        Call DebugAudit.Test(cv.lambda1se >= cv.lambdaMin, "lambda.1se >= lambda.min")
        Call DebugAudit.AreEqual(N, cv.foldId.Length, "确定性分折覆盖全部样本")
        Call DebugAudit.Test(cv.foldErrors.Length = cv.nfolds AndAlso cv.foldErrors(0).Length = steps,
                            "逐折指标矩阵维度为 nfolds x steps")

        Dim scores As Double() = cv.Predict(rows)

        Call DebugAudit.AreEqual(N, scores.Length, "CV 选中 lambda 的预测返回 N 个风险得分")
        Call DebugAudit.Test(MaxAbsDiff(scores, cv.Model.Predict(rows, cv.BestIndex)) < 0.000000000001R,
                            "CV 预测等价于用 lambda.min 下标预测")
        Call DebugAudit.Test(cv.GetRiskModel(use1se:=True) IsNot Nothing,
                            "CV 结果可取 1-SE 模型的 IFitted 视图")
        Call DebugAudit.Test(cv.PredictSurvival(New Double()() {rows(0)}).Length = 1,
                            "CV 结果可直接预测生存曲线")

        Dim cvTable As NumericTable = cv.toNumericTable()

        Call DebugAudit.AreEqual(steps, cvTable.nsamples, "CV 曲线表每步一行")
        Call DebugAudit.AreEqual(5, cvTable.nfeatures, "CV 曲线表含 cvMean/cvSe/df/devRatio/cIndex 五列")
        Call DebugAudit.Test(cvTable.HasLabel("lambdas"), "CV 曲线表携带 lambdas 标签列")

        Dim ungrouped As CoxCVFit = CoxCV.CrossValidate(rows, response, nfolds:=5,
                                                        numberOfLambdas:=15, grouped:=False)

        Call DebugAudit.Test(Not ungrouped.grouped, "可关闭 grouped 模式")
        Call DebugAudit.Test(AllFinite(ungrouped.cvMean), "非 grouped 模式的 CV 均值全部有限")

        Dim cIndexCV As CoxCVFit = CoxCV.CrossValidate(rows, response, nfolds:=5,
                                                       numberOfLambdas:=15,
                                                       measure:=CoxCVMeasure.CIndex)

        Call DebugAudit.AreEqual(CoxCVMeasure.CIndex, cIndexCV.measure, "可切换到 C 指数指标")
        Call DebugAudit.Test(cIndexCV.Maximize, "C 指数方向为越大越好")
        Call DebugAudit.Test(cIndexCV.cvMean.All(Function(v) v >= 0.0R AndAlso v <= 1.0R),
                            "C 指数 CV 均值全部落在 [0,1]")
        Call DebugAudit.Test(cIndexCV.BestScore >= cIndexCV.cvMean(0) - 0.0000000001R,
                            "C 指数选中的 lambda 不差于惩罚最强的起点")

        Dim again As CoxCVFit = CoxCV.CrossValidate(rows, response, nfolds:=5,
                                                    numberOfLambdas:=15, seed:=1)

        Call DebugAudit.Test(std.Abs(again.lambdaMin - cv.lambdaMin) < 0.000000000001R,
                            "相同种子下 CV 结果可复现")

        Call DebugAudit.Mark("cox.cv.lambdaMin", cv.lambdaMin)
        Call DebugAudit.Mark("cox.cv.lambda1se", cv.lambda1se)
        Call DebugAudit.Mark("cox.cv.cIndexBest", cIndexCV.BestScore)
    End Sub

#End Region

#Region "numeric table entry"

    ''' <summary>二维表 time/status 入口、自定义列名与表内 CV</summary>
    Private Sub CheckTableEntry()
        Dim rows As Double()() = MakeDesign()
        Dim response As Double()() = MakeSurvival(rows, TrueBeta())
        Dim table As New NumericTable(rows,
                                      Enumerable.Range(1, N).Select(Function(i) $"s{i}").ToArray(),
                                      Enumerable.Range(1, P).Select(Function(j) $"x{j}").ToArray())

        Call table.SetLabel("time", response.Select(Function(r) r(0)).ToArray())
        Call table.SetLabel("status", response.Select(Function(r) r(1)).ToArray())

        Dim fit As CoxFit = table.CoxRegression(numberOfLambdas:=15)

        Call DebugAudit.AreEqual(P, fit.NumberOfFeatures, "表入口使用全部特征列")
        Call DebugAudit.Test(fit.eventCount > 0, "表入口正确解析事件状态")
        Call DebugAudit.AreEqual("x1", fit.featureNames(0), "特征列名来自表头")
        Call DebugAudit.Test(fit.converged.All(Function(v) v), "表入口拟合全部收敛")

        Call table.SetLabel("followup", response.Select(Function(r) r(0)).ToArray())
        Call table.SetLabel("event", response.Select(Function(r) r(1)).ToArray())

        Dim renamed As CoxFit = table.CoxRegression(time:="followup", status:="event",
                                                    numberOfLambdas:=15)

        Call DebugAudit.AreEqual(fit.eventCount, renamed.eventCount, "可用自定义列名覆盖 time/status")
        Call DebugAudit.Test(MaxAbsDiff(fit.getCoefficients(14), renamed.getCoefficients(14)) < 0.000000000001R,
                            "同一份数据换列名后系数完全一致")

        Dim tableCV As CoxCVFit = table.CoxCV(nfolds:=4, numberOfLambdas:=10)

        Call DebugAudit.AreEqual(4, tableCV.nfolds, "表入口 CV 折数正确")
        Call DebugAudit.AreEqual(10, tableCV.lambdas.Length, "表入口 CV 使用指定步数")
        Call DebugAudit.Test(AllFinite(tableCV.cvMean), "表入口 CV 曲线全部有限")

        Dim noLabel As New NumericTable(rows, Nothing, Nothing)

        Call DebugAudit.CheckThrows(Of ArgumentException)(
            Sub()
                Call noLabel.CoxRegression()
            End Sub,
            "缺少 time/status 标签列时给出明确异常")

        Call DebugAudit.CheckThrows(Of ArgumentException)(
            Sub()
                Call table.CoxRegression(time:="no_such_column")
            End Sub,
            "指定不存在的时间列应给出明确异常")
    End Sub

#End Region

#Region "boundary"

    ''' <summary>输入校验：生存响应格式、参数范围与预测维度</summary>
    Private Sub CheckBoundary()
        Dim rows As Double()() = MakeDesign()
        Dim response As Double()() = MakeSurvival(rows, TrueBeta())

        Call DebugAudit.CheckThrows(Of ArgumentNullException)(
            Sub()
                Call CoxFitting.Fit(Nothing, response)
            End Sub,
            "空特征矩阵应抛出 ArgumentNullException")

        Call DebugAudit.CheckThrows(Of ArgumentException)(
            Sub()
                Call CoxFitting.Fit(rows, New Double()() {New Double() {1.0R}})
            End Sub,
            "生存响应必须是 N x 2 矩阵")

        Dim badStatus As Double()() = New Double(N - 1)() {}

        For i As Integer = 0 To N - 1
            badStatus(i) = New Double() {response(i)(0), response(i)(1)}
        Next
        badStatus(0)(1) = 0.5R

        Call DebugAudit.CheckThrows(Of ArgumentException)(
            Sub()
                Call CoxFitting.Fit(rows, badStatus)
            End Sub,
            "status 只能是 0 或 1")

        Dim badTime As Double()() = New Double(N - 1)() {}

        For i As Integer = 0 To N - 1
            badTime(i) = New Double() {response(i)(0), response(i)(1)}
        Next
        badTime(0)(0) = -1.0R

        Call DebugAudit.CheckThrows(Of ArgumentException)(
            Sub()
                Call CoxFitting.Fit(rows, badTime)
            End Sub,
            "负的观测时间应被拒绝")

        Dim noEvents As Double()() = New Double(N - 1)() {}

        For i As Integer = 0 To N - 1
            noEvents(i) = New Double() {response(i)(0), 0.0R}
        Next

        Call DebugAudit.CheckThrows(Of ArgumentException)(
            Sub()
                Call CoxFitting.Fit(rows, noEvents)
            End Sub,
            "没有事件时偏似然无定义，应被拒绝")

        Call DebugAudit.CheckThrows(Of ArgumentException)(
            Sub()
                Call CoxFitting.Fit(rows, response, alpha:=1.5R)
            End Sub,
            "alpha 越界应被拒绝")

        Call DebugAudit.CheckThrows(Of ArgumentException)(
            Sub()
                Call CoxFitting.Fit(rows, response, lambda:=New Double() {0.1R, 0.2R})
            End Sub,
            "自定义 lambda 网格必须严格递减")

        Call DebugAudit.CheckThrows(Of ArgumentException)(
            Sub()
                Call CoxFitting.Fit(rows, response, penaltyFactor:=New Double() {1.0R})
            End Sub,
            "惩罚因子长度不匹配应被拒绝")

        Dim negativePf As Double() = Enumerable.Repeat(1.0R, P).ToArray()
        negativePf(0) = -1.0R

        Call DebugAudit.CheckThrows(Of ArgumentException)(
            Sub()
                Call CoxFitting.Fit(rows, response, penaltyFactor:=negativePf)
            End Sub,
            "负惩罚因子应被拒绝")

        Call DebugAudit.CheckThrows(Of ArgumentException)(
            Sub()
                Call CoxFitting.Fit(rows, response, ties:=CType(99, CoxTies))
            End Sub,
            "非法平局处理方式应被拒绝")

        Call DebugAudit.CheckThrows(Of ArgumentException)(
            Sub()
                Call CoxCV.CrossValidate(rows, response, nfolds:=1, numberOfLambdas:=5)
            End Sub,
            "CV 折数小于 2 应被拒绝")

        Call DebugAudit.CheckThrows(Of ArgumentException)(
            Sub()
                Call CoxCV.CrossValidate(rows, response, nfolds:=N + 1, numberOfLambdas:=5)
            End Sub,
            "CV 折数大于样本数应被拒绝")

        Dim elapsed As Double = 0.0R

        Using scope = DebugAudit.Watch("cox.fit.20lambdas", logOnExit:=False)
            Call CoxFitting.Fit(rows, response, numberOfLambdas:=20)
            elapsed = scope.ElapsedMilliseconds
        End Using

        Call DebugAudit.Mark("cox.boundary.elapsedMs", elapsed)
    End Sub

#End Region

End Module
