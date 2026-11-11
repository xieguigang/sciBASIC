Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports Microsoft.VisualBasic.ApplicationServices.Debugging
Imports Microsoft.VisualBasic.Data
Imports Microsoft.VisualBasic.Data.Bootstrapping
Imports Microsoft.VisualBasic.Data.Bootstrapping.MGaussian
Imports std = System.Math

''' <summary>
''' 多响应高斯模型（mgaussian）的正确性审计：组稀疏、系数恢复、alpha 混合、
''' standardize.response、lambda 路径、多 lambda 预测、K 折交叉验证、二维表入口与异常输入防护。
'''
''' 这是一个 console 类型的测试程序：所有断言都经由
''' <see cref="DebugAudit"/> 记录，结束时由 <see cref="DebugAudit.SaveReport(String)"/>
''' 生成 JSON 测试报告，<see cref="DebugAudit.FailureCount"/> 即失败数。
''' </summary>
''' <remarks>
''' 本模块刻意不定义 <c>Sub Main</c>：自检入口是同项目下的
''' <c>Program.vb</c>，它通过 <see cref="Run"/> 把审计结果合并进总的失败计数。
''' </remarks>
Public Module MGaussianAudit

    Private Const ReportName As String = "mgaussian-test-report.json"
    Private Const N As Integer = 120
    Private Const P As Integer = 8
    Private Const K As Integer = 3
    Private Const ActiveFeatures As Integer = 3

    ''' <summary>
    ''' 运行全部审计用例并生成测试报告。
    ''' </summary>
    ''' <returns>失败断言的数量（0 表示全部通过）</returns>
    Public Function Run() As Integer
        Dim reportPath As String = Path.Combine(AppContext.BaseDirectory, ReportName)

        ' Check / Snapshot / Capture 带 <Conditional("DEBUG")>，Release 下会被消除，
        ' 因此断言一律使用 Test / AreEqual / Require / CheckThrows / Mark / Watch。
        Call DebugAudit.Reset()
        DebugAudit.MaxEvents = 8192
        Call DebugAudit.Mark("mgaussian audit started")

        Try
            Call CheckSharedSparsity()
            Call CheckCoefficientRecovery()
            Call CheckAlpha()
            Call CheckStandardizeResponse()
            Call CheckPath()
            Call CheckPrediction()
            Call CheckResponseView()
            Call CheckCrossValidation()
            Call CheckTableEntry()
            Call CheckBoundary()
        Catch ex As Exception
            Call DebugAudit.Mark("audit.crash", ex.ToString())
            Call DebugAudit.Test(False, $"审计套件不应抛出未处理异常: {ex.Message}")
        End Try

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

    ''' <summary>确定性的近似独立特征列（固定种子的伪随机矩阵）</summary>
    Private Function MakeDesign() As Double()()
        Dim rows As Double()() = New Double(N - 1)() {}
        Dim rnd As New Random(2024)

        For i As Integer = 0 To N - 1
            rows(i) = New Double(P - 1) {}

            For j As Integer = 0 To P - 1
                rows(i)(j) = 2.0R * rnd.NextDouble() - 1.0R
            Next
        Next

        Return rows
    End Function

    ''' <summary>只有前 3 个变量对所有 K 个响应有效，其余变量全为 0</summary>
    Private Function MakeTrueCoefficients() As Double()()
        Dim coef As Double()() = New Double(P - 1)() {}

        For j As Integer = 0 To P - 1
            coef(j) = New Double(K - 1) {}
        Next

        coef(0)(0) = 2.0R : coef(0)(1) = -1.5R : coef(0)(2) = 0.8R
        coef(1)(0) = -1.2R : coef(1)(1) = 1.8R : coef(1)(2) = -0.6R
        coef(2)(0) = 0.9R : coef(2)(1) = 0.7R : coef(2)(2) = 1.4R

        Return coef
    End Function

    ''' <summary>由已知系数生成 N x K 响应矩阵，可加入固定种子的确定性噪声</summary>
    Private Function MakeResponse(rows As Double()(),
                                  coef As Double()(),
                                  intercept As Double(),
                                  noise As Double,
                                  Optional seed As Integer = 17) As Double()()

        Dim response As Double()() = New Double(N - 1)() {}
        Dim rnd As New Random(seed)

        For i As Integer = 0 To N - 1
            response(i) = New Double(K - 1) {}

            For c As Integer = 0 To K - 1
                Dim value As Double = intercept(c)

                For j As Integer = 0 To P - 1
                    value += rows(i)(j) * coef(j)(c)
                Next

                response(i)(c) = value + noise * (rnd.NextDouble() - 0.5R)
            Next
        Next

        Return response
    End Function

    Private Function TrueIntercepts() As Double()
        Return New Double() {1.0R, -0.5R, 2.0R}
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

#Region "shared sparsity"

    ''' <summary>组-lasso 的共享支持集：变量在全部响应上同进同出</summary>
    Private Sub CheckSharedSparsity()
        Dim rows As Double()() = MakeDesign()
        Dim response As Double()() = MakeResponse(rows, MakeTrueCoefficients(), TrueIntercepts(), 0.0R)
        Dim fit As MGaussianFit = MGaussianFitting.Fit(rows, response, numberOfLambdas:=30)

        Call DebugAudit.Test(fit.df(0) = 0,
                            $"lambda_max 处全部变量组都被剔除: df(0)={fit.df(0)}")
        Call DebugAudit.Test(fit.df(fit.df.Length - 1) >= ActiveFeatures,
                            "惩罚减弱后活动变量数不少于真实数量")

        ' 取「第一次有足够多变量进入模型」的那一步：此时惩罚仍然很强，
        ' 真实有效变量应当明显强于全部无关变量
        Dim chosen As Integer = -1

        For li As Integer = 0 To fit.lambdas.Length - 1
            If fit.df(li) >= ActiveFeatures Then
                chosen = li
                Exit For
            End If
        Next

        Call DebugAudit.Test(chosen > 0, $"路径上存在活动变量达到 {ActiveFeatures} 个的步骤: idx={chosen}")

        If chosen > 0 Then
            Dim minActive As Double = Double.PositiveInfinity
            Dim maxInactive As Double = 0.0R

            For j As Integer = 0 To P - 1
                Dim norm As Double = fit.groupNorms(chosen)(j)

                If j < ActiveFeatures Then
                    minActive = std.Min(minActive, norm)
                Else
                    maxInactive = std.Max(maxInactive, norm)
                End If
            Next

            Call DebugAudit.Test(minActive > maxInactive,
                                $"组-lasso 优先选中真实的 {ActiveFeatures} 个共享变量: min(active)={minActive:F4} > max(inactive)={maxInactive:E3}")

            For j As Integer = 0 To P - 1
                Dim norm As Double = fit.groupNorms(chosen)(j)
                Dim anyNonZero As Boolean = False
                Dim allNonZero As Boolean = True

                For c As Integer = 0 To K - 1
                    Dim b As Double = fit.coefficients(chosen)(j)(c)

                    If std.Abs(b) > 0.0000000001R Then
                        anyNonZero = True
                    Else
                        allNonZero = False
                    End If
                Next

                Call DebugAudit.Test((norm > 0.0000000001R) = anyNonZero,
                                    $"变量 {j} 的 2-norm 与整组系数是否全零一致（组结构而非逐系数稀疏）")

                If j < ActiveFeatures AndAlso norm > 0.1R Then
                    Call DebugAudit.Test(allNonZero,
                                        $"真实有效变量 {j} 对全部 {K} 个响应同时非零（同进同出）")
                End If
            Next

            Call DebugAudit.Mark("mgaussian.sparsity.dfChosen", fit.df(chosen))
        End If

        Call DebugAudit.Mark("mgaussian.sparsity.lambdaMax", fit.MaxLambda)
    End Sub

#End Region

#Region "coefficients"

    ''' <summary>小 lambda 之下把系数与截距还原到原始量纲并恢复真值</summary>
    Private Sub CheckCoefficientRecovery()
        Dim rows As Double()() = MakeDesign()
        Dim coef As Double()() = MakeTrueCoefficients()
        Dim b0 As Double() = TrueIntercepts()
        Dim response As Double()() = MakeResponse(rows, coef, b0, 0.0R)
        Dim fit As MGaussianFit = MGaussianFitting.Fit(rows, response, numberOfLambdas:=30)
        Dim last As Integer = fit.lambdas.Length - 1
        Dim maxCoefDiff As Double = 0.0R
        Dim maxInterceptDiff As Double = 0.0R

        For j As Integer = 0 To P - 1
            For c As Integer = 0 To K - 1
                maxCoefDiff = std.Max(maxCoefDiff,
                                      std.Abs(fit.coefficients(last)(j)(c) - coef(j)(c)))
            Next
        Next
        For c As Integer = 0 To K - 1
            maxInterceptDiff = std.Max(maxInterceptDiff,
                                       std.Abs(fit.intercepts(last)(c) - b0(c)))
        Next

        Call DebugAudit.Test(maxCoefDiff < 0.02R,
                            $"系数还原到原始量纲并恢复真值: maxΔ={maxCoefDiff:E3}")
        Call DebugAudit.Test(maxInterceptDiff < 0.02R,
                            $"每个响应的独立截距恢复真值: maxΔ={maxInterceptDiff:E3}")
        Call DebugAudit.Test(fit.trainMse(last) < 0.001R,
                            $"无噪声数据末端训练 MSE 接近 0: {fit.trainMse(last):E3}")
        Call DebugAudit.Test(fit.NumberOfFeatures = P AndAlso fit.NumberOfResponses = K,
                            "结果对象记录了正确的 p 与 K")
        Call DebugAudit.Test(fit.ToString().Contains("MGaussian"), "ToString 输出模型摘要")

        Dim matrix As Double()() = fit.getCoefficients(last)

        Call DebugAudit.AreEqual(P + 1, matrix.Length, "getCoefficients 返回 (p+1) x K 矩阵")
        Call DebugAudit.AreEqual(K, matrix(0).Length, "系数矩阵列数等于响应数")
        Call DebugAudit.Test(MaxAbsDiff(matrix(0), b0) < 0.02R, "系数矩阵第 0 行是各响应截距")

        Call DebugAudit.Mark("mgaussian.recovery.maxCoefficientDiff", maxCoefDiff)
        Call DebugAudit.Mark("mgaussian.recovery.maxInterceptDiff", maxInterceptDiff)
    End Sub

#End Region

#Region "alpha"

    ''' <summary>alpha=0 无精确零；alpha=0.95 弹性网混合仍可收敛</summary>
    Private Sub CheckAlpha()
        Dim rows As Double()() = MakeDesign()
        Dim response As Double()() = MakeResponse(rows, MakeTrueCoefficients(), TrueIntercepts(), 0.05R)
        Dim ridgeLike As MGaussianFit = MGaussianFitting.Fit(rows, response, alpha:=0.0R, numberOfLambdas:=20)
        Dim elastic As MGaussianFit = MGaussianFitting.Fit(rows, response, alpha:=0.95R, numberOfLambdas:=20)
        Dim last As Integer = ridgeLike.lambdas.Length - 1

        Call DebugAudit.Test(ridgeLike.df(last) = P,
                            $"alpha=0 退化成岭型收缩，不产生精确零: df={ridgeLike.df(last)}")
        Call DebugAudit.Test(ridgeLike.groupNorms(last).All(Function(v) v > 0.0R),
                            "alpha=0 时每个变量组都非零")
        Call DebugAudit.Test(ridgeLike.MaxLambda > 0.0R AndAlso Double.IsFinite(ridgeLike.MaxLambda),
                            "alpha=0 仍有有限的正 lambda 起点")
        Call DebugAudit.Test(elastic.converged.All(Function(v) v),
                            "alpha=0.95 的弹性网混合全部收敛")
        Call DebugAudit.Test(elastic.df(0) = 0, "alpha=0.95 在 lambda_max 处仍剔除全部变量")

        Call DebugAudit.Mark("mgaussian.alpha.ridgeLikeLambdaMax", ridgeLike.MaxLambda)
    End Sub

#End Region

#Region "standardize.response"

    ''' <summary>各响应量纲悬殊时 standardize.response 改变网格但预测仍还原到原量纲</summary>
    Private Sub CheckStandardizeResponse()
        Dim rows As Double()() = MakeDesign()
        Dim coef As Double()() = MakeTrueCoefficients()
        Dim baseResponse As Double()() = MakeResponse(rows, coef, TrueIntercepts(), 0.0R)
        Dim response As Double()() = New Double(N - 1)() {}
        Dim scales As Double() = {100.0R, 1.0R, 0.01R}

        For i As Integer = 0 To N - 1
            response(i) = New Double(K - 1) {}

            For c As Integer = 0 To K - 1
                response(i)(c) = baseResponse(i)(c) * scales(c)
            Next
        Next

        Dim raw As MGaussianFit = MGaussianFitting.Fit(rows, response, numberOfLambdas:=20,
                                                       standardizeResponse:=False)
        Dim scaled As MGaussianFit = MGaussianFitting.Fit(rows, response, numberOfLambdas:=20,
                                                          standardizeResponse:=True)

        Call DebugAudit.Test(scaled.standardizeResponse, "结果对象记录了 standardize.response=True")
        Call DebugAudit.Test(scaled.ResponseScales(0) > 10.0R * scaled.ResponseScales(2),
                            $"响应尺度被正确估计: {scaled.ResponseScales(0):F2} vs {scaled.ResponseScales(2):F2}")
        Call DebugAudit.Test(std.Abs(scaled.MaxLambda / raw.MaxLambda - 1.0R) > 0.1R,
                            "标准化响应改变了 lambda 网格起点（量纲影响收缩）")

        Dim last As Integer = scaled.lambdas.Length - 1
        Dim predicted As Double()() = scaled.Predict(rows, last)
        Dim maxRelativeError As Double = 0.0R

        For i As Integer = 0 To N - 1
            For c As Integer = 0 To K - 1
                Dim observed As Double = response(i)(c)
                Dim err As Double = std.Abs(predicted(i)(c) - observed) / std.Max(1.0R, std.Abs(observed))

                maxRelativeError = std.Max(maxRelativeError, err)
            Next
        Next

        Call DebugAudit.Test(maxRelativeError < 0.05R,
                            $"standardize.response 后预测仍还原到原始量纲: max relative error={maxRelativeError:E3}")

        Call DebugAudit.Mark("mgaussian.standardizeResponse.maxRelativeError", maxRelativeError)
    End Sub

#End Region

#Region "path"

    ''' <summary>lambda 严格递减、df 单调不减、训练 MSE 单调不增、warm start 收敛</summary>
    Private Sub CheckPath()
        Dim rows As Double()() = MakeDesign()
        Dim response As Double()() = MakeResponse(rows, MakeTrueCoefficients(), TrueIntercepts(), 0.05R)
        Dim fit As MGaussianFit = MGaussianFitting.Fit(rows, response, numberOfLambdas:=30)
        Dim steps As Integer = fit.lambdas.Length
        Dim descending As Boolean = True
        Dim dfMonotone As Boolean = True
        Dim mseMonotone As Boolean = True

        For li As Integer = 1 To steps - 1
            If fit.lambdas(li) >= fit.lambdas(li - 1) Then
                descending = False
            End If
            If fit.df(li) < fit.df(li - 1) Then
                dfMonotone = False
            End If
            If fit.trainMse(li) > fit.trainMse(li - 1) + 0.000001R Then
                mseMonotone = False
            End If
        Next

        Call DebugAudit.Test(descending, "lambda 网格严格递减")
        Call DebugAudit.Test(std.Abs(fit.MaxLambda / fit.lambdas(0) - 1.0R) < 0.000000000001R,
                            "MaxLambda 等于路径起点")
        Call DebugAudit.Test(fit.MinLambda < fit.MaxLambda, "MinLambda 小于 MaxLambda")
        Call DebugAudit.Test(dfMonotone, "惩罚减弱时活动变量数单调不减")
        Call DebugAudit.Test(mseMonotone, "惩罚减弱时训练 MSE 单调不增")
        Call DebugAudit.Test(fit.converged.All(Function(v) v),
                            $"全部 {steps} 个 lambda 步骤都收敛")
        Call DebugAudit.Test(fit.iterations.All(Function(v) v > 0 AndAlso v < 1000),
                            "warm start 把内迭代轮数压在合理范围")
        Dim flatNorms As New List(Of Double)()

        For li As Integer = 0 To steps - 1
            For j As Integer = 0 To P - 1
                Call flatNorms.Add(fit.groupNorms(li)(j))
            Next
        Next

        Call DebugAudit.Test(AllFinite(fit.trainMse) AndAlso AllFinite(flatNorms.ToArray()),
                            "训练误差与 2-norm 路径全部为有限数")

        Dim pathTable As NumericTable = fit.toNumericTable()

        Call DebugAudit.AreEqual(steps, pathTable.nsamples, "路径导出表每步一行")
        Call DebugAudit.AreEqual(P, pathTable.nfeatures, "路径导出表每变量一列（2-norm 视角）")
        Call DebugAudit.Test(pathTable.HasLabel("lambdas") AndAlso pathTable.HasLabel("df") AndAlso
                            pathTable.HasLabel("mse") AndAlso pathTable.HasLabel("iterations") AndAlso
                            pathTable.HasLabel("converged"),
                            "路径导出表携带 lambdas/df/mse/iterations/converged 标签列")
        Call DebugAudit.Test(MaxAbsDiff(pathTable.GetLabel("lambdas"), fit.lambdas) < 0.000000000001R,
                            "导出表的 lambda 列与路径一致")
        Call DebugAudit.Test(std.Abs(pathTable.GetLabel("df")(steps - 1) - CDbl(fit.df(steps - 1))) < 0.0000001R,
                            "导出表的 df 列与路径一致")

        Call DebugAudit.Mark("mgaussian.path.steps", steps)
        Call DebugAudit.Mark("mgaussian.path.maxIterations", fit.iterations.Max())
    End Sub

#End Region

#Region "prediction"

    ''' <summary>单 lambda / 多 lambda 预测的形状与数值一致性</summary>
    Private Sub CheckPrediction()
        Dim rows As Double()() = MakeDesign()
        Dim response As Double()() = MakeResponse(rows, MakeTrueCoefficients(), TrueIntercepts(), 0.0R)
        Dim fit As MGaussianFit = MGaussianFitting.Fit(rows, response, numberOfLambdas:=20)
        Dim last As Integer = fit.lambdas.Length - 1
        Dim predicted As Double()() = fit.Predict(rows)

        Call DebugAudit.AreEqual(N, predicted.Length, "Predict 返回 N 行")
        Call DebugAudit.AreEqual(K, predicted(0).Length, "Predict 返回 K 列")
        Call DebugAudit.Test(MaxAbsDiff(fit.PredictSingle(rows(3)), predicted(3)) < 0.000000000001R,
                            "PredictSingle 与 Predict 的对应行一致")

        Dim matrix As Double()() = fit.getCoefficients(last)
        Dim manual As Double = matrix(0)(1)

        For j As Integer = 0 To P - 1
            manual += rows(5)(j) * matrix(j + 1)(1)
        Next

        Call DebugAudit.Test(std.Abs(manual - predicted(5)(1)) < 0.000000000001R,
                            "预测值等于截距 + 系数矩阵点积")

        Dim path As Double()()() = fit.PredictPath(rows, New Integer() {0, last})

        Call DebugAudit.AreEqual(2, path.Length, "PredictPath 第一维是 lambda 步骤数")
        Call DebugAudit.AreEqual(N, path(0).Length, "PredictPath 第二维是样本数")
        Call DebugAudit.AreEqual(K, path(0)(0).Length, "PredictPath 第三维是响应数")
        Call DebugAudit.Test(MaxAbsDiff(path(0)(7), fit.Predict(rows, 0)(7)) < 0.000000000001R,
                            "PredictPath 的第 0 步与单步 Predict 一致")
        Call DebugAudit.Test(MaxAbsDiff(path(1)(7), predicted(7)) < 0.000000000001R,
                            "PredictPath 的最后一步与默认 Predict 一致")

        Dim allSteps As Double()()() = fit.PredictPath(rows, Nothing)

        Call DebugAudit.AreEqual(fit.lambdas.Length, allSteps.Length,
                            "lambdaIdx 为 Nothing 时输出全部步骤（三维预测数组）")

        Call DebugAudit.CheckThrows(Of ArgumentException)(
            Sub()
                Call fit.Predict(New Double()() {New Double() {1.0R, 2.0R}})
            End Sub,
            "预测特征数量不足应给出明确异常")
    End Sub

#End Region

#Region "response view"

    ''' <summary>逐响应 IFitted 视图与既有 SetPrediction 写回链路联动</summary>
    Private Sub CheckResponseView()
        Dim rows As Double()() = MakeDesign()
        Dim response As Double()() = MakeResponse(rows, MakeTrueCoefficients(), TrueIntercepts(), 0.0R)
        Dim fit As MGaussianFit = MGaussianFitting.Fit(rows, response, numberOfLambdas:=20)
        Dim view As IFitted = fit.GetResponseModel(1)
        Dim predicted As Double()() = fit.Predict(rows)

        Call DebugAudit.Test(view IsNot Nothing, "GetResponseModel 返回 IFitted 视图")
        Call DebugAudit.Test(std.Abs(view.GetY(rows(4)) - predicted(4)(1)) < 0.000000000001R,
                            "逐响应视图的 GetY 与多响应 Predict 的第 1 列一致")
        Call DebugAudit.AreEqual(P, view.Polynomial.Factors.Length, "视图的多项式含 p 个系数")
        Call DebugAudit.Test(view.R2 > 0.99R, $"视图的 R2 由逐点误差真实计算: {view.R2:F6}")
        Call DebugAudit.AreEqual(N, view.ErrorTest.Length, "视图为每个样本保留一条误差记录")

        Dim table As New NumericTable(rows,
                                      Enumerable.Range(1, N).Select(Function(i) $"s{i}").ToArray(),
                                      Enumerable.Range(1, P).Select(Function(j) $"x{j}").ToArray())

        Call table.SetPrediction(view, name:="mg_fit")

        Call DebugAudit.Test(table.HasLabel("mg_fit"), "SetPrediction 可写回逐响应视图的预测")
        Call DebugAudit.Test(std.Abs(table.GetLabel("mg_fit")(4) - predicted(4)(1)) < 0.000000000001R,
                            "写回列的数值与视图 GetY 一致")

        Call DebugAudit.CheckThrows(Of ArgumentOutOfRangeException)(
            Sub()
                Call fit.GetResponseModel(K)
            End Sub,
            "响应下标越界应给出明确异常")
        Call DebugAudit.CheckThrows(Of ArgumentOutOfRangeException)(
            Sub()
                Call fit.getCoefficients(fit.lambdas.Length)
            End Sub,
            "lambda 下标越界应给出明确异常")
    End Sub

#End Region

#Region "cross validation"

    ''' <summary>K 折 CV 的 mse/mae、lambda.min/1se 与可复现性</summary>
    Private Sub CheckCrossValidation()
        Dim rows As Double()() = MakeDesign()
        Dim response As Double()() = MakeResponse(rows, MakeTrueCoefficients(), TrueIntercepts(), 0.3R, seed:=29)
        Dim cv As MGaussianCVFit = MGaussianCV.CrossValidate(rows, response, nfolds:=5,
                                                             numberOfLambdas:=20)
        Dim steps As Integer = cv.lambdas.Length

        Call DebugAudit.AreEqual(20, steps, "CV 使用 20 步统一 lambda 网格")
        Call DebugAudit.AreEqual(5, cv.nfolds, "CV 折数正确")
        Call DebugAudit.AreEqual(CVMeasure.Mse, cv.measure, "缺省评估指标为 mse")
        Call DebugAudit.AreEqual(steps, cv.cvMean.Length, "每个 lambda 都有一个 CV 均值")
        Call DebugAudit.AreEqual(steps, cv.cvSe.Length, "每个 lambda 都有一个 CV 标准误")
        Call DebugAudit.Test(AllFinite(cv.cvMean) AndAlso AllFinite(cv.cvSe),
                            "CV 均值与标准误全部有限")
        Call DebugAudit.Test(cv.cvMean.All(Function(v) v >= 0.0R) AndAlso
                            cv.cvSe.All(Function(v) v >= 0.0R),
                            "CV 误差与标准误非负")
        Call DebugAudit.Test(cv.BestIndex >= 0 AndAlso cv.BestIndex < steps,
                            $"lambda.min 下标在网格内: {cv.BestIndex}")
        Call DebugAudit.Test(cv.BestIndex1se <= cv.BestIndex,
                            $"1-SE 规则选出更强惩罚: idx {cv.BestIndex1se} <= {cv.BestIndex}")
        Call DebugAudit.Test(cv.lambda1se >= cv.lambdaMin,
                            $"lambda.1se >= lambda.min: {cv.lambda1se:G4} vs {cv.lambdaMin:G4}")
        Call DebugAudit.Test(cv.cvMean(cv.BestIndex) <= cv.cvMean(0) + 0.0000000001R,
                            "选中的 lambda 不差于惩罚最强的起点")
        Call DebugAudit.AreEqual(N, cv.foldId.Length, "确定性分折覆盖全部样本")
        Call DebugAudit.Test(cv.foldId.All(Function(f) f >= 0 AndAlso f < cv.nfolds),
                            "分折编号落在有效范围内")
        Call DebugAudit.Test(cv.foldErrors.Length = cv.nfolds AndAlso
                            cv.foldErrors(0).Length = steps,
                            "逐折误差矩阵维度为 nfolds x steps")

        Dim predicted As Double()() = cv.Predict(rows)

        Call DebugAudit.AreEqual(N, predicted.Length, "CV 选中 lambda 的预测返回 N 行")
        Call DebugAudit.AreEqual(K, predicted(0).Length, "CV 选中 lambda 的预测返回 K 列")
        Call DebugAudit.Test(MaxAbsDiff(predicted(2), cv.Model.Predict(rows, cv.BestIndex)(2)) < 0.000000000001R,
                            "CV 预测等价于用 lambda.min 下标预测")
        Call DebugAudit.Test(cv.GetResponseModel(0) IsNot Nothing,
                            "CV 结果可取得逐响应 IFitted 视图")

        Dim cvTable As NumericTable = cv.toNumericTable()

        Call DebugAudit.AreEqual(steps, cvTable.nsamples, "CV 曲线表每步一行")
        Call DebugAudit.AreEqual(4, cvTable.nfeatures, "CV 曲线表含 cvMean/cvSe/df/trainMse 四列")
        Call DebugAudit.Test(cvTable.HasLabel("lambdas"), "CV 曲线表携带 lambdas 标签列")
        Call DebugAudit.Test(cv.ToString().Contains("MGaussianCV"), "CV 结果 ToString 输出摘要")

        Dim mae As MGaussianCVFit = MGaussianCV.CrossValidate(rows, response, nfolds:=5,
                                                              numberOfLambdas:=20,
                                                              measure:=CVMeasure.Mae)

        Call DebugAudit.AreEqual(CVMeasure.Mae, mae.measure, "可切换到 mae 指标")
        Call DebugAudit.Test(AllFinite(mae.cvMean), "mae 指标下的 CV 均值全部有限")
        Call DebugAudit.Test(mae.cvMean(mae.BestIndex) < cv.cvMean(cv.BestIndex) + 10.0R,
                            "两种指标的误差量级合理")

        Dim again As MGaussianCVFit = MGaussianCV.CrossValidate(rows, response, nfolds:=5,
                                                                numberOfLambdas:=20, seed:=1)

        Call DebugAudit.Test(std.Abs(again.lambdaMin - cv.lambdaMin) < 0.000000000001R,
                            "相同种子下 CV 结果可复现")

        Call DebugAudit.Mark("mgaussian.cv.lambdaMin", cv.lambdaMin)
        Call DebugAudit.Mark("mgaussian.cv.lambda1se", cv.lambda1se)
        Call DebugAudit.Mark("mgaussian.cv.cvMeanMin", cv.cvMean(cv.BestIndex))
    End Sub

#End Region

#Region "numeric table entry"

    ''' <summary>二维表统一入口：全部标签列 / 指定标签列子集 / 表内 CV</summary>
    Private Sub CheckTableEntry()
        Dim rows As Double()() = MakeDesign()
        Dim response As Double()() = MakeResponse(rows, MakeTrueCoefficients(), TrueIntercepts(), 0.0R)
        Dim table As New NumericTable(rows,
                                      Enumerable.Range(1, N).Select(Function(i) $"m{i}").ToArray(),
                                      Enumerable.Range(1, P).Select(Function(j) $"x{j}").ToArray())

        For c As Integer = 0 To K - 1
            Dim col As Integer = c
            Call table.SetLabel($"y{c + 1}",
                                Enumerable.Range(0, N).Select(Function(i) response(i)(col)).ToArray())
        Next

        Dim fit As MGaussianFit = table.MGaussianRegression(numberOfLambdas:=20)

        Call DebugAudit.AreEqual(K, fit.NumberOfResponses, "表入口缺省使用全部标签列")
        Call DebugAudit.AreEqual(P, fit.NumberOfFeatures, "表入口使用全部特征列")
        Call DebugAudit.AreEqual("y1", fit.responseNames(0), "响应列名来自标签列名")
        Call DebugAudit.AreEqual("x1", fit.featureNames(0), "特征列名来自表头")
        Call DebugAudit.Test(fit.converged.All(Function(v) v), "表入口拟合全部收敛")

        Dim subset As MGaussianFit = table.MGaussianRegression(y:=New String() {"y1", "y3"},
                                                               numberOfLambdas:=20)

        Call DebugAudit.AreEqual(2, subset.NumberOfResponses, "指定标签列子集时 K=2")
        Call DebugAudit.AreEqual("y3", subset.responseNames(1), "子集保持了指定列的顺序")

        Dim tableCV As MGaussianCVFit = table.MGaussianCV(nfolds:=4, numberOfLambdas:=15)

        Call DebugAudit.AreEqual(4, tableCV.nfolds, "表入口 CV 折数正确")
        Call DebugAudit.AreEqual(15, tableCV.lambdas.Length, "表入口 CV 使用指定步数")
        Call DebugAudit.Test(tableCV.cvMean.All(Function(v) Double.IsFinite(v)),
                            "表入口 CV 曲线全部有限")

        Dim noLabel As New NumericTable(rows, Nothing, Nothing)

        Call DebugAudit.CheckThrows(Of ArgumentException)(
            Sub()
                Call noLabel.MGaussianRegression()
            End Sub,
            "没有标签列时表入口给出明确异常")

        Call DebugAudit.CheckThrows(Of KeyNotFoundException)(
            Sub()
                Call table.MGaussianRegression(y:=New String() {"no_such_label"})
            End Sub,
            "指定不存在的标签列应给出明确异常")

        Call DebugAudit.Mark("mgaussian.table.nlabels", table.nlabels)
    End Sub

#End Region

#Region "boundary"

    ''' <summary>输入校验：维度、有限性、alpha、网格、权重与 CV 折数</summary>
    Private Sub CheckBoundary()
        Dim rows As Double()() = MakeDesign()
        Dim response As Double()() = MakeResponse(rows, MakeTrueCoefficients(), TrueIntercepts(), 0.0R)

        Call DebugAudit.CheckThrows(Of ArgumentNullException)(
            Sub()
                Call MGaussianFitting.Fit(Nothing, response)
            End Sub,
            "空特征矩阵应抛出 ArgumentNullException")

        Call DebugAudit.CheckThrows(Of ArgumentException)(
            Sub()
                Call MGaussianFitting.Fit(Array.Empty(Of Double())(), Array.Empty(Of Double())())
            End Sub,
            "空样本集应抛出 ArgumentException")

        Dim ragged As Double()() = CType(rows.Clone(), Double()())
        ragged(0) = New Double() {1.0R, 2.0R}

        Call DebugAudit.CheckThrows(Of ArgumentException)(
            Sub()
                Call MGaussianFitting.Fit(ragged, response)
            End Sub,
            "特征行长度不一致应被拒绝")

        Dim shortY As Double()() = New Double(N - 2)() {}

        For i As Integer = 0 To N - 2
            shortY(i) = response(i)
        Next

        Call DebugAudit.CheckThrows(Of ArgumentException)(
            Sub()
                Call MGaussianFitting.Fit(rows, shortY)
            End Sub,
            "响应矩阵行数不匹配应被拒绝")

        Dim badX As Double()() = MakeDesign()
        badX(0)(0) = Double.NaN

        Call DebugAudit.CheckThrows(Of ArgumentException)(
            Sub()
                Call MGaussianFitting.Fit(badX, response)
            End Sub,
            "非有限特征值应被拒绝")

        Call DebugAudit.CheckThrows(Of ArgumentException)(
            Sub()
                Call MGaussianFitting.Fit(rows, response, alpha:=1.5R)
            End Sub,
            "alpha 越界应被拒绝")

        Call DebugAudit.CheckThrows(Of ArgumentException)(
            Sub()
                Call MGaussianFitting.Fit(rows, response, numberOfLambdas:=1)
            End Sub,
            "自动网格步数小于 2 应被拒绝")

        Call DebugAudit.CheckThrows(Of ArgumentException)(
            Sub()
                Call MGaussianFitting.Fit(rows, response, maxIterations:=0)
            End Sub,
            "非正迭代上限应被拒绝")

        Call DebugAudit.CheckThrows(Of ArgumentException)(
            Sub()
                Call MGaussianFitting.Fit(rows, response, tolerance:=0.0R)
            End Sub,
            "非正收敛容差应被拒绝")

        Call DebugAudit.CheckThrows(Of ArgumentException)(
            Sub()
                Call MGaussianFitting.Fit(rows, response, lambda:=New Double() {0.1R, 0.2R})
            End Sub,
            "自定义 lambda 网格必须严格递减")

        Call DebugAudit.CheckThrows(Of ArgumentException)(
            Sub()
                Call MGaussianFitting.Fit(rows, response, lambda:=New Double() {0.1R, 0.0R})
            End Sub,
            "自定义 lambda 网格必须全为正")

        Call DebugAudit.CheckThrows(Of ArgumentException)(
            Sub()
                Call MGaussianFitting.Fit(rows, response, featureNames:=New String() {"only_one"})
            End Sub,
            "特征名数量不匹配应被拒绝")

        Call DebugAudit.CheckThrows(Of ArgumentException)(
            Sub()
                Call MGaussianFitting.Fit(rows, response, responseNames:=New String() {"only_one"})
            End Sub,
            "响应名数量不匹配应被拒绝")

        Call DebugAudit.CheckThrows(Of ArgumentException)(
            Sub()
                Call MGaussianFitting.Fit(rows, response, weights:=New Double() {1.0R, 2.0R})
            End Sub,
            "权重长度不匹配应被拒绝")

        Dim badWeight As Double() = Enumerable.Repeat(1.0R, N).ToArray()
        badWeight(0) = -1.0R

        Call DebugAudit.CheckThrows(Of ArgumentException)(
            Sub()
                Call MGaussianFitting.Fit(rows, response, weights:=badWeight)
            End Sub,
            "非正样本权重应被拒绝")

        Call DebugAudit.CheckThrows(Of ArgumentException)(
            Sub()
                Call MGaussianCV.CrossValidate(rows, response, nfolds:=1, numberOfLambdas:=10)
            End Sub,
            "CV 折数小于 2 应被拒绝")

        Call DebugAudit.CheckThrows(Of ArgumentException)(
            Sub()
                Call MGaussianCV.CrossValidate(rows, response, nfolds:=N + 1, numberOfLambdas:=10)
            End Sub,
            "CV 折数大于样本数应被拒绝")

        Call DebugAudit.CheckThrows(Of ArgumentException)(
            Sub()
                Call MGaussianFitting.Fit(New Double()() {New Double() {1.0R}}, New Double()() {New Double() {1.0R}})
            End Sub,
            "样本数小于 2 应被拒绝")

        Dim weighted As MGaussianFit = MGaussianFitting.Fit(rows, response, numberOfLambdas:=10,
                                                            weights:=Enumerable.Repeat(2.0R, N).ToArray())

        Call DebugAudit.Test(weighted.converged.All(Function(v) v),
                            "带样本权重的路径全部收敛")

        Dim elapsed As Double = 0.0R

        Using scope = DebugAudit.Watch("mgaussian.fit.20lambdas", logOnExit:=False)
            Call MGaussianFitting.Fit(rows, response, numberOfLambdas:=20)
            elapsed = scope.ElapsedMilliseconds
        End Using

        Call DebugAudit.Mark("mgaussian.boundary.elapsedMs", elapsed)
    End Sub

#End Region

End Module
