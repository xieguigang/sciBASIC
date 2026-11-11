Imports System
Imports System.IO
Imports System.Linq
Imports Microsoft.VisualBasic.ApplicationServices.Debugging
Imports Microsoft.VisualBasic.MachineLearning.Bootstrapping.Ensemble

''' <summary>
''' 集成学习框架的 console 审计程序。
''' </summary>
''' <remarks>
''' 用 <see cref="DebugAudit"/> 记录全部断言与耗时，结束时生成
''' <c>ensemble-test-report.json</c>，退出码等于失败断言数。
''' </remarks>
Module Program

    Private Const ReportName As String = "ensemble-test-report.json"

    Function Main() As Integer
        Dim reportPath As String = Path.Combine(AppContext.BaseDirectory, ReportName)

        Call DebugAudit.Reset()
        DebugAudit.MaxEvents = 4096
        Call DebugAudit.Mark("ensemble audit started")

        Try
            Call CheckAdaptors()
            Call CheckCrossValidation()
            Call CheckVoting()
            Call CheckBagging()
            Call CheckStacking()
            Call CheckBoundary()
        Catch ex As Exception
            Call DebugAudit.Mark("audit.crash", ex.ToString())
            Call DebugAudit.Test(False, $"审计套件不应抛出未处理异常: {ex.Message}")
        End Try

        Call DebugAudit.SaveReport(reportPath)
        Call Console.WriteLine($"   [audit] report -> {reportPath}")

        ' 只统计本审计程序自身的失败断言：被集成的算法库在内部也会写入
        ' 全局断言（例如 RanFog 调用 Shuffles 时传 Nothing），那不属于本框架的缺陷。
        Dim ownFailures As Integer = DebugAudit.Records _
            .Where(Function(r) Not r.Succeeded AndAlso
                               r.SourceFile IsNot Nothing AndAlso
                               r.SourceFile.EndsWith("Program.vb", StringComparison.OrdinalIgnoreCase)) _
            .Count()

        Call Console.WriteLine($"   [audit] {ownFailures} failure(s) in this audit")

        Return ownFailures
    End Function

#Region "test data"

    ''' <summary>二分类数据：两个 0..4 的离散特征，和大于 3 记为正类</summary>
    Private Function MakeBinary(n As Integer, seed As Integer) As (x As Double()(), y As Double())
        Dim rnd As New Random(seed)
        Dim x As Double()() = New Double(n - 1)() {}
        Dim y As Double() = New Double(n - 1) {}

        For i As Integer = 0 To n - 1
            Dim a As Integer = rnd.Next(0, 5)
            Dim b As Integer = rnd.Next(0, 5)

            x(i) = New Double() {a, b}
            y(i) = If(a + b > 3, 1.0R, 0.0R)
        Next

        Return (x, y)
    End Function

    ''' <summary>三分类数据：按两个离散特征分段，类别条件近似独立</summary>
    Private Function MakeMultiClass(n As Integer, seed As Integer) As (x As Double()(), y As Double())
        Dim rnd As New Random(seed)
        Dim x As Double()() = New Double(n - 1)() {}
        Dim y As Double() = New Double(n - 1) {}

        For i As Integer = 0 To n - 1
            Dim a As Integer = rnd.Next(0, 5)
            Dim b As Integer = rnd.Next(0, 5)

            x(i) = New Double() {a, b}
            y(i) = If(a <= 1, 0.0R, If(b <= 1, 1.0R, 2.0R))
        Next

        Return (x, y)
    End Function

    ''' <summary>回归数据：y = 2a - b + 小噪声</summary>
    Private Function MakeRegression(n As Integer, seed As Integer) As (x As Double()(), y As Double())
        Dim rnd As New Random(seed)
        Dim x As Double()() = New Double(n - 1)() {}
        Dim y As Double() = New Double(n - 1) {}

        For i As Integer = 0 To n - 1
            Dim a As Double = rnd.NextDouble() * 4.0R - 2.0R
            Dim b As Double = rnd.NextDouble() * 4.0R - 2.0R

            x(i) = New Double() {a, b}
            y(i) = 2.0R * a - 1.0R * b + (rnd.NextDouble() - 0.5R) * 0.2R
        Next

        Return (x, y)
    End Function

    Private Function Encode(y As Double(), ByRef classes As Double()) As Integer()
        Dim encoder As LabelEncoder = LabelEncoder.Fit(y)

        classes = encoder.Classes

        Return encoder.Encode(y)
    End Function

    Private Function EncodePredictions(model As IClassifier, x As Double()()) As Integer()
        Dim classes As Double() = model.Classes
        Dim result As Integer() = New Integer(x.Length - 1) {}

        For i As Integer = 0 To x.Length - 1
            result(i) = Array.IndexOf(classes, ProbabilityMath.PredictClass(model, x(i)))
        Next

        Return result
    End Function

#End Region

#Region "adaptors"

    ''' <summary>逐一验证六类算法族的适配器能训练并输出合法结果</summary>
    Private Sub CheckAdaptors()
        Dim binary = MakeBinary(60, 7)
        Dim multi = MakeMultiClass(90, 11)
        Dim regression = MakeRegression(60, 13)
        Dim classes As Double() = Nothing
        Dim yBinary As Integer() = Encode(binary.y, classes)
        Dim binaryClasses As Double() = CType(classes.Clone(), Double())
        Dim yMulti As Integer() = Encode(multi.y, classes)
        Dim multiClasses As Double() = CType(classes.Clone(), Double())

        ' 分类
        Call CheckClassifier("naive-bayes",
            New NaiveBayesClassifierLearner().Fit(multi.x, yMulti, multiClasses), multi.x(0), 3)
        Call CheckClassifier("logistic",
            New LogisticClassifierLearner().Fit(binary.x, yBinary, binaryClasses), binary.x(0), 2)
        Call CheckClassifier("svm",
            New SvmClassifierLearner().Fit(binary.x, yBinary, binaryClasses), binary.x(0), 2)
        Call CheckClassifier("xgboost",
            New XGBoostClassifierLearner With {.Rounds = 5, .MaxDepth = 3}.Fit(binary.x, yBinary, binaryClasses), binary.x(0), 2)

        ' 回归
        Call CheckRegressor("mlr", New MlrRegressorLearner().Fit(regression.x, regression.y), regression.x(0))
        Call CheckRegressor("ridge", New RidgeRegressorLearner().Fit(regression.x, regression.y), regression.x(0))
        Call CheckRegressor("lasso", New LassoRegressorLearner().Fit(regression.x, regression.y), regression.x(0))
        Call CheckRegressor("bayesian", New BayesianRegressorLearner().Fit(regression.x, regression.y), regression.x(0))
        Call CheckRegressor("random-forest",
            New RandomForestRegressorLearner With {.TreeCount = 6, .MaxBranch = 200}.Fit(regression.x, regression.y), regression.x(0))
        Call CheckRegressor("svr", New SvmRegressorLearner().Fit(regression.x, regression.y), regression.x(0))
        Call CheckRegressor("xgboost",
            New XGBoostRegressorLearner With {.Rounds = 5, .MaxDepth = 3}.Fit(regression.x, regression.y), regression.x(0))
    End Sub

    Private Sub CheckClassifier(name As String, model As IClassifier, sample As Double(), expectedClasses As Integer)
        Dim probability As Double() = model.PredictProbability(sample)
        Dim total As Double = probability.Sum()

        Call DebugAudit.Test(model.Classes.Length = expectedClasses,
                            $"{name}: 类别数 = {model.Classes.Length}, 期望 {expectedClasses}")
        Call DebugAudit.Test(probability.Length = expectedClasses,
                            $"{name}: 概率向量长度 = {probability.Length}")
        Call DebugAudit.Test(probability.All(Function(v) Not Double.IsNaN(v) AndAlso Not Double.IsInfinity(v) AndAlso v >= 0.0R),
                            $"{name}: 概率均为非负有限值")
        Call DebugAudit.Test(System.Math.Abs(total - 1.0R) < 0.000001R,
                            $"{name}: 概率已归一化, sum={total:F6}")
    End Sub

    Private Sub CheckRegressor(name As String, model As IRegressor, sample As Double())
        Dim value As Double = model.Predict(sample)

        Call DebugAudit.Test(Double.IsFinite(value), $"{name}: 回归输出有限, value={value:F6}")
    End Sub

#End Region

#Region "cross validation"

    Private Sub CheckCrossValidation()
        Dim binary = MakeBinary(97, 41)
        Dim classes As Double() = Nothing
        Dim yIndex As Integer() = Encode(binary.y, classes)
        Dim foldIndex As Integer()() = CrossValidation.StratifiedKFold(yIndex, 5, 42)

        Call DebugAudit.Test(foldIndex.Length = 5, "kfold: 折数 = 5")
        Call DebugAudit.Test(foldIndex.Sum(Function(part) part.Length) = yIndex.Length,
                            "kfold: 全部样本都被覆盖")

        Dim seen As Integer() = New Integer(yIndex.Length - 1) {}

        For Each part As Integer() In foldIndex
            For Each i As Integer In part
                seen(i) += 1
            Next
        Next

        Call DebugAudit.Test(seen.All(Function(c) c = 1), "kfold: 每个样本恰好出现在一个测试折")

        Dim ratio As Double = yIndex.Count(Function(v) v = 0) / CDbl(yIndex.Length)

        For Each part As Integer() In foldIndex
            Dim partRatio As Double = part.Count(Function(i) yIndex(i) = 0) / CDbl(part.Length)

            Call DebugAudit.Test(System.Math.Abs(partRatio - ratio) < 0.2R,
                                $"kfold: 分层比例接近全集, {partRatio:F3} vs {ratio:F3}")
        Next
    End Sub

#End Region

#Region "voting"

    Private Sub CheckVoting()
        Dim binary = MakeBinary(120, 17)
        Dim classes As Double() = Nothing
        Dim yIndex As Integer() = Encode(binary.y, classes)
        Dim learners As IClassifierLearner() = {
            New NaiveBayesClassifierLearner(),
            New LogisticClassifierLearner()
        }

        Dim soft As VotingClassifier = New VotingClassifier(learners, Nothing, VotingMode.Soft).Train(binary.x, binary.y)
        Dim hard As VotingClassifier = New VotingClassifier(learners, Nothing, VotingMode.Hard).Train(binary.x, binary.y)

        Call DebugAudit.Test(soft.BaseModelCount = 2, "voting: 基模型数量 = 2")
        Call DebugAudit.Test(soft.Classes.Length = 2, "voting: 类别数 = 2")
        Call DebugAudit.Test(System.Math.Abs(soft.Weights.Sum() - 1.0R) < 0.000000000001R,
                            $"voting: 权重已归一化, sum={soft.Weights.Sum():R}, count={soft.Weights.Length}")
        Call DebugAudit.Test(soft.BaseModels.Length = 2 AndAlso soft.BaseModels(0) IsNot Nothing, "voting: 基模型已训练")

        Dim softAccuracy As Double = EnsembleMetrics.Accuracy(EncodePredictions(soft, binary.x), yIndex)
        Dim hardAccuracy As Double = EnsembleMetrics.Accuracy(EncodePredictions(hard, binary.x), yIndex)

        Call DebugAudit.Test(softAccuracy > 0.7R, $"voting soft accuracy={softAccuracy:F4}")
        Call DebugAudit.Test(hardAccuracy > 0.7R, $"voting hard accuracy={hardAccuracy:F4}")

        Dim regression = MakeRegression(120, 19)
        Dim regressor As VotingRegressor = New VotingRegressor(
            New IRegressorLearner() {New MlrRegressorLearner(), New RidgeRegressorLearner()}
        ).Train(regression.x, regression.y)

        Dim rmse As Double = EnsembleMetrics.RMSE(regressor.PredictBatch(regression.x), regression.y)
        Dim ridgeOnly As IRegressor = New RidgeRegressorLearner().Fit(regression.x, regression.y)
        Dim baselinePredicted As Double() = regression.x.Select(Function(row) ridgeOnly.Predict(row)).ToArray()
        Dim baseline As Double = EnsembleMetrics.RMSE(baselinePredicted, regression.y)
        Dim mlrOnly As IRegressor = New MlrRegressorLearner().Fit(regression.x, regression.y)
        Dim mlrRmse As Double = EnsembleMetrics.RMSE(
            regression.x.Select(Function(row) mlrOnly.Predict(row)).ToArray(), regression.y)

        Call DebugAudit.Mark("voting.baseline.ridgeRmse", baseline)
        Call DebugAudit.Mark("voting.baseline.mlrRmse", mlrRmse)

        Call DebugAudit.Test(rmse < 1.0R, $"voting regressor RMSE={rmse:F4}")
        Call DebugAudit.Test(rmse <= baseline + 0.05R, $"voting 不劣于单模型 ridge: {rmse:F4} vs {baseline:F4}")

        Call DebugAudit.Mark("voting.soft.accuracy", softAccuracy)
        Call DebugAudit.Mark("voting.regressor.rmse", rmse)
    End Sub

#End Region

#Region "bagging"

    Private Sub CheckBagging()
        Dim binary = MakeBinary(120, 23)
        Dim classes As Double() = Nothing
        Dim yIndex As Integer() = Encode(binary.y, classes)

        Dim bag As BaggingClassifier = New BaggingClassifier(
            New NaiveBayesClassifierLearner(), modelCount:=7, ratio:=1.0R, seed:=42
        ).Train(binary.x, binary.y)

        Call DebugAudit.Test(bag.BaseModelCount = 7, "bagging: 基模型数量 = 7")

        Dim probability As Double() = bag.PredictProbability(binary.x(0))

        Call DebugAudit.Test(System.Math.Abs(probability.Sum() - 1.0R) < 0.000001R, "bagging: 概率已归一化")

        Dim accuracy As Double = EnsembleMetrics.Accuracy(EncodePredictions(bag, binary.x), yIndex)

        Call DebugAudit.Test(accuracy > 0.7R, $"bagging accuracy={accuracy:F4}")

        Dim again As BaggingClassifier = New BaggingClassifier(
            New NaiveBayesClassifierLearner(), modelCount:=7, ratio:=1.0R, seed:=42
        ).Train(binary.x, binary.y)

        Call DebugAudit.Test(System.Math.Abs(again.PredictProbability(binary.x(0))(1) - probability(1)) < 0.000000000001R,
                            "bagging: 相同种子下结果可复现")

        Dim regression = MakeRegression(100, 29)
        Dim meanBag As BaggingRegressor = New BaggingRegressor(
            New RidgeRegressorLearner(), modelCount:=5, ratio:=1.0R, seed:=7
        ) With {.Aggregation = BaggingAggregation.Mean}
        Call meanBag.Train(regression.x, regression.y)

        Dim medianBag As BaggingRegressor = New BaggingRegressor(
            New RidgeRegressorLearner(), modelCount:=5, ratio:=1.0R, seed:=7
        ) With {.Aggregation = BaggingAggregation.Median}
        Call medianBag.Train(regression.x, regression.y)

        Dim rmseMean As Double = EnsembleMetrics.RMSE(meanBag.PredictBatch(regression.x), regression.y)
        Dim rmseMedian As Double = EnsembleMetrics.RMSE(medianBag.PredictBatch(regression.x), regression.y)

        Call DebugAudit.Test(rmseMean < 1.0R, $"bagging mean RMSE={rmseMean:F4}")
        Call DebugAudit.Test(rmseMedian < 1.0R, $"bagging median RMSE={rmseMedian:F4}")

        Call DebugAudit.Mark("bagging.accuracy", accuracy)
        Call DebugAudit.Mark("bagging.regressor.meanRmse", rmseMean)
        Call DebugAudit.Mark("bagging.regressor.medianRmse", rmseMedian)
    End Sub

#End Region

#Region "stacking"

    Private Sub CheckStacking()
        Dim binary = MakeBinary(120, 31)
        Dim classes As Double() = Nothing
        Dim yIndex As Integer() = Encode(binary.y, classes)

        Dim stack As StackingClassifier = New StackingClassifier(
            New IClassifierLearner() {New NaiveBayesClassifierLearner(), New LogisticClassifierLearner()},
            New LogisticClassifierLearner(),
            folds:=5, seed:=42
        ).Train(binary.x, binary.y)

        Call DebugAudit.Test(stack.BaseModelCount = 2, "stacking: 基模型数量 = 2")
        Call DebugAudit.Test(stack.MetaModel IsNot Nothing, "stacking: 元学习器已训练")
        Call DebugAudit.Test(stack.FoldCount = 5, "stacking: 折数 = 5")
        Call DebugAudit.Test(Double.IsFinite(stack.OofAccuracy), $"stacking: OOF accuracy={stack.OofAccuracy:F4}")

        Dim accuracy As Double = EnsembleMetrics.Accuracy(EncodePredictions(stack, binary.x), yIndex)

        Call DebugAudit.Test(accuracy > 0.7R, $"stacking accuracy={accuracy:F4}")

        Dim probability As Double() = stack.PredictProbability(binary.x(0))

        Call DebugAudit.Test(System.Math.Abs(probability.Sum() - 1.0R) < 0.000001R, "stacking: 概率已归一化")

        Dim regression = MakeRegression(120, 37)
        Dim regStack As StackingRegressor = New StackingRegressor(
            New IRegressorLearner() {New MlrRegressorLearner(), New RidgeRegressorLearner()},
            New RidgeRegressorLearner(),
            folds:=5, seed:=42
        ).Train(regression.x, regression.y)

        Call DebugAudit.Test(Double.IsFinite(regStack.OofRMSE), $"stacking OOF RMSE={regStack.OofRMSE:F4}")

        Dim rmse As Double = EnsembleMetrics.RMSE(regStack.PredictBatch(regression.x), regression.y)

        Call DebugAudit.Test(rmse < 1.0R, $"stacking regressor RMSE={rmse:F4}")

        Call DebugAudit.Mark("stacking.accuracy", accuracy)
        Call DebugAudit.Mark("stacking.oofAccuracy", stack.OofAccuracy)
        Call DebugAudit.Mark("stacking.regressor.rmse", rmse)
        Call DebugAudit.Mark("stacking.regressor.oofRmse", regStack.OofRMSE)
    End Sub

#End Region

#Region "boundary"

    Private Sub CheckBoundary()
        Dim nb As New NaiveBayesClassifierLearner()

        Call DebugAudit.CheckThrows(Of ArgumentException)(
            Sub()
                Dim model As New VotingClassifier(
                    New IClassifierLearner() {nb, New LogisticClassifierLearner()},
                    New Double() {-1.0R, 1.0R})
            End Sub,
            "voting: 负权重应被拒绝")

        Call DebugAudit.CheckThrows(Of ArgumentException)(
            Sub()
                Dim model As New VotingClassifier(
                    New IClassifierLearner() {nb, New LogisticClassifierLearner()},
                    New Double() {0.0R, 0.0R})
            End Sub,
            "voting: 权重和为零应被拒绝")

        Call DebugAudit.CheckThrows(Of ArgumentException)(
            Sub()
                Dim encoder As LabelEncoder = LabelEncoder.Fit(New Double() {1.0R, 1.0R})
            End Sub,
            "label encoder: 单类别应被拒绝")

        Call DebugAudit.CheckThrows(Of ArgumentException)(
            Sub()
                Dim encoder As LabelEncoder = LabelEncoder.Fit(New Double() {Double.NaN, 1.0R})
            End Sub,
            "label encoder: NaN 标签应被拒绝")

        Call DebugAudit.CheckThrows(Of ArgumentException)(
            Sub()
                Dim model As New VotingClassifier(New IClassifierLearner() {nb})
                Call model.Train(
                    New Double()() {New Double() {1.0R, 2.0R}, New Double() {2.0R, 3.0R}},
                    New Double() {0.0R})
            End Sub,
            "voting: X 与 y 长度不一致应被拒绝")

        Call DebugAudit.CheckThrows(Of ArgumentException)(
            Sub()
                Dim model As New VotingClassifier(New IClassifierLearner() {nb})
                Call model.Train(Array.Empty(Of Double())(), Array.Empty(Of Double)())
            End Sub,
            "voting: 空数据应被拒绝")

        Call DebugAudit.CheckThrows(Of InvalidOperationException)(
            Sub()
                Dim model As New VotingClassifier(New IClassifierLearner() {nb})
                Call model.PredictProbability(New Double() {1.0R, 2.0R})
            End Sub,
            "voting: 未训练预测应抛出 InvalidOperationException")

        Call DebugAudit.CheckThrows(Of ArgumentException)(
            Sub()
                Dim model As New BaggingClassifier(nb, modelCount:=0)
            End Sub,
            "bagging: 模型数必须为正")

        Call DebugAudit.CheckThrows(Of ArgumentException)(
            Sub()
                Dim model As New StackingClassifier(New IClassifierLearner() {nb}, New LogisticClassifierLearner(), folds:=1)
            End Sub,
            "stacking: 折数至少为 2")

        Call DebugAudit.CheckThrows(Of ArgumentException)(
            Sub()
                Dim model As New VotingClassifier(New IClassifierLearner() {}, Nothing)
            End Sub,
            "voting: 空基学习器列表应被拒绝")
    End Sub

#End Region

End Module
