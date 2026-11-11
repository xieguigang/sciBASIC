Imports System
Imports System.IO
Imports System.Linq
Imports Microsoft.VisualBasic.ApplicationServices.Debugging
Imports Microsoft.VisualBasic.Data.Bootstrapping.Logistic
Imports Microsoft.VisualBasic.Math.SIMD
Imports std = System.Math

''' <summary>
''' 逻辑回归的正确性审计、标量 / SIMD / SIMD+Parallel 三路数值对拍与性能基准。
'''
''' 这是一个 console 类型的测试程序：所有断言与耗时都经由
''' <see cref="DebugAudit"/> 记录，结束时由 <see cref="DebugAudit.SaveReport(String)"/>
''' 生成 JSON 测试报告，<see cref="DebugAudit.FailureCount"/> 即失败数。
''' </summary>
''' <remarks>
''' 本模块刻意不定义 <c>Sub Main</c>：自检入口是同项目下的
''' <c>Program.vb</c>，它通过 <see cref="Run"/> 把审计结果合并进总的失败计数。
''' </remarks>
Public Module LogisticAudit

    Private Const ReportName As String = "logistic-test-report.json"

    ''' <summary>
    ''' 运行全部审计用例并生成测试报告。
    ''' </summary>
    ''' <returns>失败断言的数量（0 表示全部通过）</returns>
    Public Function Run() As Integer
        Dim reportPath As String = Path.Combine(AppContext.BaseDirectory, ReportName)
        Dim savedConfig As SIMDConfiguration = SIMDEnvironment.config
        Dim savedParallel As Boolean = SimdParallel.Enable

        ' 注意：Check / Capture / Snapshot 带有 <Conditional("DEBUG")>，
        ' 在 Release 下会被编译器整体消除，因此这里一律使用 Test / AreEqual /
        ' CheckThrows / Mark / Watch 这些在任何配置下都有效的方法。
        ' FailureCount / Records 是全局状态：Core 在程序启动阶段也会写入断言，
        ' 必须先清空，保证报告与返回值只反映本次逻辑回归审计。
        Call DebugAudit.Reset()
        DebugAudit.MaxEvents = 4096
        Call DebugAudit.Mark("logistic regression audit started")

        Try
            Call CheckContract()
            Call CheckBoundary()
            Call CheckStandardization()
            Call CheckGradient()
            Call CheckFitting()
            Call CheckThreeWayParity()
            Call CheckBenchmark()
        Catch ex As Exception
            Call DebugAudit.Mark("audit.crash", ex.ToString())
            Call DebugAudit.Test(False, $"审计套件不应抛出未处理异常: {ex.Message}")
        Finally
            SIMDEnvironment.config = savedConfig
            SimdParallel.Enable = savedParallel
        End Try

        Call DebugAudit.SaveReport(reportPath)
        Call Console.WriteLine($"   [audit] report -> {reportPath}")
        Call Console.WriteLine($"   [audit] {DebugAudit.FailureCount} failure(s)")

        Return DebugAudit.FailureCount
    End Function

#Region "test data"

    ''' <summary>构造一份确定性、近似线性可分的二分类数据</summary>
    Private Function MakeData(n As Integer, d As Integer, Optional seed As Integer = 7) As Instance()
        Dim rnd As New Random(seed)
        Dim data As Instance() = New Instance(n - 1) {}

        For i As Integer = 0 To n - 1
            Dim x As Double() = New Double(d - 1) {}
            Dim score As Double = 0.3R

            For j As Integer = 0 To d - 1
                x(j) = rnd.NextDouble() * 2.0R - 1.0R

                If j = 0 Then
                    score += 2.5R * x(j)
                ElseIf j = 1 Then
                    score -= 1.2R * x(j)
                Else
                    score += 0.15R * x(j)
                End If
            Next

            data(i) = New Instance(If(score > 0.0R, 1, 0), x)
        Next

        Return data
    End Function

    Private Function RowsOf(data As Instance()) As Double()()
        Return data.Select(Function(a) a.x).ToArray()
    End Function

    Private Function MaxDiff(a As Double(), b As Double()) As Double
        If a.Length <> b.Length Then
            Return Double.PositiveInfinity
        End If

        Dim diff As Double = 0.0R

        For i As Integer = 0 To a.Length - 1
            diff = std.Max(diff, std.Abs(a(i) - b(i)))
        Next

        Return diff
    End Function

    ''' <summary>用固定步长训练，保证三路对拍走完全相同的数值路径</summary>
    Private Function TrainFixed(data As Instance(), d As Integer, useParallel As Boolean) As Logistic
        Dim model As New Logistic(d, 0.35R) With {
            .ITERATIONS = 60,
            .AdaptiveRate = False,
            .Tolerance = 0.0R,
            .UseParallel = useParallel,
            .MinParallelWork = 0
        }

        Call model.Train(data)

        Return model
    End Function

#End Region

#Region "contract"

    ''' <summary>概率语义一致性、方向、R2 与 LogLoss 真实性</summary>
    Private Sub CheckContract()
        Dim data As Instance() = MakeData(240, 3)
        Dim model As New Logistic(3, 0.8R) With {
            .ITERATIONS = 300,
            .AdaptiveRate = False,
            .Tolerance = 0.0R,
            .UseParallel = False
        }
        Dim fit As LogisticFit = model.Train(data)
        Dim maxDisagreement As Double = 0.0R
        Dim accuracy As Integer = 0

        For i As Integer = 0 To data.Length - 1
            Dim p1 As Double = model.predict(data(i).x)
            Dim p2 As Double = fit.GetY(data(i).x)
            Dim p3 As Double = fit.ErrorTest(i).Yfit

            maxDisagreement = std.Max(maxDisagreement, std.Max(std.Abs(p1 - p2), std.Abs(p2 - p3)))

            If If(p2 >= 0.5R, 1, 0) = CInt(data(i).label) Then
                accuracy += 1
            End If
        Next

        Call DebugAudit.Test(maxDisagreement < 0.000000000001R,
                            $"predict / GetY / ErrorTest.Yfit 返回同一个正类概率, maxΔ={maxDisagreement:E3}")
        Call DebugAudit.Test(accuracy / CDbl(data.Length) > 0.9R,
                            $"线性可分数据准确率 = {accuracy}/{data.Length}")
        Call DebugAudit.Test(AllProbabilities(fit), "所有拟合值都落在 [0,1] 概率区间内")

        ' R2 必须由 ErrorTest 真实计算，而不是硬编码为 1
        Dim meanY As Double = fit.ErrorTest.Sum(Function(e) e.Y) / fit.ErrorTest.Length
        Dim sse As Double = 0.0R
        Dim sst As Double = 0.0R

        For Each e In fit.ErrorTest
            sse += (e.Y - e.Yfit) ^ 2
            sst += (e.Y - meanY) ^ 2
        Next

        Dim expected As Double = If(sst > 0.0R, 1.0R - sse / sst, If(sse <= 0.0R, 1.0R, 0.0R))

        Call DebugAudit.Test(std.Abs(fit.R2 - expected) < 0.000000000001R,
                            $"R2 由 ErrorTest 计算而非硬编码: R2={fit.R2:F6}, expected={expected:F6}")
        Call DebugAudit.Test(fit.LogLoss > 0.0R AndAlso Double.IsFinite(fit.LogLoss),
                            $"LogLoss 为有效的正有限值: {fit.LogLoss:F6}")
        Call DebugAudit.Test(fit.Polynomial.Factors.Length = 3,
                            "Polynomial.Factors 长度仍等于特征数（截距为独立标量）")
        Call DebugAudit.Test(Double.IsFinite(fit.Intercept), $"截距项有效: b={fit.Intercept:F6}")

        Call DebugAudit.Mark("logistic.contract.accuracy", accuracy / CDbl(data.Length))
        Call DebugAudit.Mark("logistic.contract.logloss", fit.LogLoss)
    End Sub

    Private Function AllProbabilities(fit As LogisticFit) As Boolean
        For Each e In fit.ErrorTest
            If Double.IsNaN(e.Yfit) OrElse e.Yfit < 0.0R OrElse e.Yfit > 1.0R Then
                Return False
            End If
        Next

        Return True
    End Function

#End Region

#Region "boundary"

    ''' <summary>异常输入的防护：空集、非法标签、不等长特征、NaN、未训练模型</summary>
    Private Sub CheckBoundary()
        Call DebugAudit.CheckThrows(Of ArgumentException)(
            Sub()
                Dim model As New Logistic(2, 0.1R)
                Call model.Train(Array.Empty(Of Instance)())
            End Sub,
            "空样本集应抛出 ArgumentException")

        Dim badLabel As Instance() = {
            New Instance(1, New Double() {1.0R, 2.0R}),
            New Instance(0, New Double() {2.0R, 1.0R}) With {.label = 0.5R}
        }

        Call DebugAudit.CheckThrows(Of ArgumentException)(
            Sub()
                Dim model As New Logistic(2, 0.1R)
                Call model.Train(badLabel)
            End Sub,
            "非 0/1 标签应被拒绝")

        Dim ragged As Instance() = {
            New Instance(1, New Double() {1.0R, 2.0R}),
            New Instance(0, New Double() {1.0R})
        }

        Call DebugAudit.CheckThrows(Of ArgumentException)(
            Sub()
                Dim model As New Logistic(2, 0.1R)
                Call model.Train(ragged)
            End Sub,
            "样本特征长度不一致应被拒绝")

        Dim notFinite As Instance() = {
            New Instance(1, New Double() {Double.NaN, 2.0R}),
            New Instance(0, New Double() {1.0R, 2.0R})
        }

        Call DebugAudit.CheckThrows(Of ArgumentException)(
            Sub()
                Dim model As New Logistic(2, 0.1R)
                Call model.Train(notFinite)
            End Sub,
            "NaN / Infinity 特征应被拒绝")

        Call DebugAudit.CheckThrows(Of InvalidOperationException)(
            Sub()
                Dim model As New Logistic()
                Call model.predict(New Double() {1.0R, 2.0R})
            End Sub,
            "未训练模型进行 predict 应给出明确异常而不是 NullReferenceException")
    End Sub

#End Region

#Region "standardization"

    ''' <summary>ZScore 返回新数组、保留均值/标准差、常量列与推理变换一致性</summary>
    Private Sub CheckStandardization()
        Dim data As Instance() = MakeData(50, 3, seed:=13)

        ' 让第 2 列成为常量列，标准差为 0
        For i As Integer = 0 To data.Length - 1
            data(i).x(2) = 1.0R
        Next

        Dim original As Double = data(0).x(0)
        Dim norm As Standardization = Nothing
        Dim z As Instance() = Instance.ZScore(data, norm)

        Call DebugAudit.Test(Not norm.IsEmpty, "ZScore 带出了标准化参数")
        Call DebugAudit.Test(norm.Means IsNot Nothing AndAlso norm.Means.Length = 3,
                            "标准化参数包含每一列的均值")
        Call DebugAudit.Test(Not Object.ReferenceEquals(z(0), data(0)), "ZScore 返回新的样本数组")
        Call DebugAudit.Test(std.Abs(data(0).x(0) - original) < 0.000000000000001R,
                            "ZScore 不修改输入数据")
        Call DebugAudit.Test(z(10).x(2) = 0.0R, "常量列标准差为 0 时标准化结果为 0")

        Dim applied As Double() = norm.Apply(data(7).x)

        Call DebugAudit.Test(std.Abs(applied(0) - z(7).x(0)) < 0.000000000001R,
                            "Standardization.Apply 与 ZScore 完全一致（训练/推理共用变换）")

        Dim empty As New Standardization()
        Dim raw As Double() = {1.0R, 2.0R, 3.0R}

        Call DebugAudit.Test(Object.ReferenceEquals(empty.Apply(raw), raw),
                            "空的标准化参数原样返回输入")
        Call DebugAudit.CheckThrows(Of ArgumentException)(
            Sub()
                Dim ignored As Standardization = Nothing
                Call Instance.ZScore(data, ignored, size:=4)
            End Sub,
            "ZScore 的 size 超过特征数时应被拒绝")
    End Sub

#End Region

#Region "gradient"

    ''' <summary>交叉熵梯度的中心差分检验（eps = 1e-6）</summary>
    Private Sub CheckGradient()
        Dim data As Instance() = MakeData(60, 3, seed:=11)
        Dim rows As Double()() = RowsOf(data)
        Dim labels As Double() = data.Select(Function(a) a.label).ToArray()
        Dim w As Double() = {0.35R, -0.2R, 0.11R}
        Dim bias As Double = -0.05R
        Dim eps As Double = 0.000001R
        Dim grad As Double() = Nothing
        Dim gradBias As Double = 0.0R

        Call LogisticKernels.CostGradient(rows, labels, w, bias, grad, gradBias)

        Dim maxRel As Double = 0.0R

        For j As Integer = 0 To w.Length - 1
            Dim plus As Double() = CType(w.Clone(), Double())
            Dim minus As Double() = CType(w.Clone(), Double())

            plus(j) += eps
            minus(j) -= eps

            Dim numeric As Double = (LogisticKernels.Cost(rows, labels, plus, bias) -
                                     LogisticKernels.Cost(rows, labels, minus, bias)) / (2.0R * eps)
            Dim scale As Double = std.Max(1.0R, std.Max(std.Abs(numeric), std.Abs(grad(j))))
            Dim rel As Double = std.Abs(numeric - grad(j)) / scale

            maxRel = std.Max(maxRel, rel)
        Next

        Dim biasNumeric As Double = (LogisticKernels.Cost(rows, labels, w, bias + eps) -
                                     LogisticKernels.Cost(rows, labels, w, bias - eps)) / (2.0R * eps)

        maxRel = std.Max(maxRel, std.Abs(biasNumeric - gradBias) / std.Max(1.0R, std.Abs(biasNumeric)))

        Call DebugAudit.Test(maxRel < 0.000001R,
                            $"交叉熵梯度通过中心差分检验, max relative error={maxRel:E3}")
        Call DebugAudit.Mark("logistic.gradient.maxRelativeError", maxRel)
    End Sub

#End Region

#Region "fitting"

    ''' <summary>成本单调下降、收敛早停、自适应学习率</summary>
    Private Sub CheckFitting()
        Dim data As Instance() = MakeData(200, 3, seed:=17)

        Dim fixedModel As New Logistic(3, 0.8R) With {
            .ITERATIONS = 200,
            .AdaptiveRate = False,
            .Tolerance = 0.0R,
            .UseParallel = False
        }

        Call fixedModel.Train(data)

        Dim history As Double() = fixedModel.CostHistory
        Dim monotone As Boolean = True

        For k As Integer = 1 To history.Length - 1
            If history(k) > history(k - 1) + 0.000000000001R Then
                monotone = False
            End If
        Next

        Call DebugAudit.Test(history.Length = 200 AndAlso monotone,
                            $"固定步长下成本单调下降, {history.Length} 轮")
        Call DebugAudit.Test(history(history.Length - 1) < history(0),
                            $"训练确实降低了成本: {history(0):F6} -> {history(history.Length - 1):F6}")

        ' 完全线性可分数据没有有限最优解，成本改善按 1/t² 衰减，早停会来得很晚；
        ' 用带 10% 标签噪声的数据，成本有非零最优值，才能干净地检验早停逻辑。
        Dim noisy As Instance() = MakeData(200, 3, seed:=31)

        For i As Integer = 0 To 19
            noisy(i).label = 1.0R - noisy(i).label
        Next

        Dim early As New Logistic(3, 0.2R) With {
            .ITERATIONS = 5000,
            .AdaptiveRate = False,
            .Tolerance = 0.0000001R,
            .UseParallel = False
        }

        Call early.Train(noisy)

        Call DebugAudit.Mark("logistic.fitting.earlyIterations", early.CostHistory.Length)
        Call DebugAudit.Test(early.CostHistory.Length < 5000,
                            $"收敛早停生效: 实际迭代 {early.CostHistory.Length} 轮（上限 5000）")

        ' 默认 ALPHA = 1e-4 在关闭自适应时几乎不收敛；开启后应能自动放大到可用步长
        Dim adaptive As New Logistic(3) With {.ITERATIONS = 300}
        Dim adaptiveFit As LogisticFit = adaptive.Train(data)
        Dim accuracy As Double = 0.0R

        For i As Integer = 0 To data.Length - 1
            If If(adaptiveFit.GetY(data(i).x) >= 0.5R, 1, 0) = CInt(data(i).label) Then
                accuracy += 1.0R
            End If
        Next

        accuracy /= data.Length

        Call DebugAudit.Test(adaptive.EffectiveAlpha > adaptive.ALPHA,
                            $"自适应学习率确实放大了步长: {adaptive.ALPHA:E3} -> {adaptive.EffectiveAlpha:E3}")
        Call DebugAudit.Test(accuracy > 0.9R,
                            $"默认学习率下自适应训练仍可收敛, accuracy={accuracy:F4}")
        Call DebugAudit.Mark("logistic.fitting.effectiveAlpha", adaptive.EffectiveAlpha)
    End Sub

#End Region

#Region "parity && benchmark"

    ''' <summary>标量 / SIMD / SIMD+Parallel 三路权重与预测对拍，并记录三段耗时</summary>
    Private Sub CheckThreeWayParity()
        Dim data As Instance() = MakeData(400, 4, seed:=23)
        Dim rows As Double()() = RowsOf(data)
        Dim wScalar As Double() = Nothing
        Dim wSimd As Double() = Nothing
        Dim wParallel As Double() = Nothing
        Dim pScalar As Double() = Nothing
        Dim pSimd As Double() = Nothing
        Dim pParallel As Double() = Nothing
        Dim parallelIterations As Integer = 0

        Using scope = DebugAudit.Watch("logistic.scalar", logOnExit:=False)
            SIMDEnvironment.config = SIMDConfiguration.disable
            SimdParallel.Enable = False

            Dim model As Logistic = TrainFixed(data, 4, useParallel:=False)

            wScalar = model.Weights
            pScalar = rows.Select(Function(x) model.predict(x)).ToArray()
        End Using

        Using scope = DebugAudit.Watch("logistic.simd", logOnExit:=False)
            SIMDEnvironment.config = SIMDConfiguration.auto
            SimdParallel.Enable = False

            Dim model As Logistic = TrainFixed(data, 4, useParallel:=False)

            wSimd = model.Weights
            pSimd = rows.Select(Function(x) model.predict(x)).ToArray()
        End Using

        Using scope = DebugAudit.Watch("logistic.parallel", logOnExit:=False)
            SIMDEnvironment.config = SIMDConfiguration.auto
            SimdParallel.Enable = True

            Dim model As Logistic = TrainFixed(data, 4, useParallel:=True)

            wParallel = model.Weights
            pParallel = rows.Select(Function(x) model.predict(x)).ToArray()
            parallelIterations = model.ParallelIterations
        End Using

        Dim maxWeight As Double = std.Max(MaxDiff(wScalar, wSimd), MaxDiff(wScalar, wParallel))
        Dim maxPredict As Double = std.Max(MaxDiff(pScalar, pSimd), MaxDiff(pScalar, pParallel))

        Call DebugAudit.Test(parallelIterations > 0,
                            $"分块并行分支确实被触发: {parallelIterations} 轮")
        Call DebugAudit.Test(maxWeight < 0.00000001R,
                            $"三路权重数值一致, maxΔ={maxWeight:E3}")
        Call DebugAudit.Test(maxPredict < 0.000000000001R,
                            $"三路预测概率数值一致, maxΔ={maxPredict:E3}")
        Call DebugAudit.Mark("logistic.parity.maxWeightDiff", maxWeight)
        Call DebugAudit.Mark("logistic.parity.maxPredictionDiff", maxPredict)
    End Sub

    ''' <summary>用固定参数跑一轮基准训练</summary>
    Private Function TrainBenchmark(data As Instance(), useParallel As Boolean) As Logistic
        Dim model As New Logistic(24, 0.2R) With {
            .ITERATIONS = 15,
            .AdaptiveRate = False,
            .Tolerance = 0.0R,
            .UseParallel = useParallel,
            .MinParallelWork = 100000
        }

        Call model.Train(data)

        Return model
    End Function

    ''' <summary>
    ''' 中等规模数据（20000 样本 x 24 特征 x 15 轮）的标量 / SIMD /
    ''' SIMD+Parallel 耗时基准。耗时只记录不设硬阈值，因为加速比与硬件相关；
    ''' 但会严格校验三路权重一致，确保加速没有牺牲数值正确性。
    ''' </summary>
    Private Sub CheckBenchmark()
        Dim data As Instance() = MakeData(20000, 24, seed:=41)
        Dim wScalar As Double() = Nothing
        Dim wSimd As Double() = Nothing
        Dim wParallel As Double() = Nothing
        Dim tScalar As Double = 0.0R
        Dim tSimd As Double = 0.0R
        Dim tParallel As Double = 0.0R
        Dim parallelIterations As Integer = 0

        Using scope = DebugAudit.Watch("benchmark.scalar", logOnExit:=False)
            SIMDEnvironment.config = SIMDConfiguration.disable
            SimdParallel.Enable = False
            wScalar = TrainBenchmark(data, False).Weights
            tScalar = scope.ElapsedMilliseconds
        End Using

        Using scope = DebugAudit.Watch("benchmark.simd", logOnExit:=False)
            SIMDEnvironment.config = SIMDConfiguration.auto
            SimdParallel.Enable = False
            wSimd = TrainBenchmark(data, False).Weights
            tSimd = scope.ElapsedMilliseconds
        End Using

        Using scope = DebugAudit.Watch("benchmark.parallel", logOnExit:=False)
            SIMDEnvironment.config = SIMDConfiguration.auto
            SimdParallel.Enable = True

            Dim model As Logistic = TrainBenchmark(data, True)

            wParallel = model.Weights
            parallelIterations = model.ParallelIterations
            tParallel = scope.ElapsedMilliseconds
        End Using

        Dim diffSimd As Double = MaxDiff(wScalar, wSimd)
        Dim diffParallel As Double = MaxDiff(wScalar, wParallel)

        Call DebugAudit.Test(diffSimd < 0.00000001R,
                            $"基准数据下标量/SIMD 权重一致, maxΔ={diffSimd:E3}")
        Call DebugAudit.Test(diffParallel < 0.00000001R,
                            $"基准数据下标量/并行权重一致, maxΔ={diffParallel:E3}")
        Call DebugAudit.Test(parallelIterations > 0,
                            $"基准数据下并行路径确实被触发: {parallelIterations} 轮")

        Call DebugAudit.Mark("benchmark.scalar.ms", tScalar)
        Call DebugAudit.Mark("benchmark.simd.ms", tSimd)
        Call DebugAudit.Mark("benchmark.parallel.ms", tParallel)
        Call DebugAudit.Mark("benchmark.simd.speedup", tScalar / std.Max(tSimd, 0.0001R))
        Call DebugAudit.Mark("benchmark.parallel.speedup", tSimd / std.Max(tParallel, 0.0001R))
    End Sub

#End Region

End Module
