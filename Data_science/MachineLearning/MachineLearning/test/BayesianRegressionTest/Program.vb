' Copyright (c) sciBASIC.NET Foundation
' GNU GENERAL PUBLIC LICENSE (GPL3) or later
'
' Console verification for the naive bayes classifier optimization and the
' bayesian regression extension. All results are recorded with DebugAudit and
' exported as a text/JSON report into <AppContext.BaseDirectory>/report/.

Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports Microsoft.VisualBasic.ApplicationServices.Debugging
Imports Microsoft.VisualBasic.Data
Imports Microsoft.VisualBasic.DataMining.Kernel.Classifier
Imports BayesRegression = Microsoft.VisualBasic.MachineLearning.BayesianRegression.BayesianRegression
Imports BayesMode = Microsoft.VisualBasic.MachineLearning.BayesianRegression.BayesFeatureMode
Imports Binning = Microsoft.VisualBasic.MachineLearning.BayesianRegression.TargetBinning
Imports GaussianModel = Microsoft.VisualBasic.MachineLearning.BayesianRegression.GaussianBayes

Namespace BayesianRegressionTest

    ''' <summary>
    ''' 朴素贝叶斯分类器优化与朴素贝叶斯回归的可运行验证入口。
    ''' </summary>
    ''' <remarks>
    ''' 传入 <c>--svm</c> 命令行参数时转发给原有的 SVM demo 入口，
    ''' 以便在调整 StartupObject 之后仍然可以运行原来的演示程序。
    ''' </remarks>
    Public Module Program

        Public Sub Main()
            Dim args As String() = Environment.GetCommandLineArgs()

            If args.Any(Function(a) String.Equals(a, "--svm", StringComparison.OrdinalIgnoreCase)) Then
                Call Global.test.SVMDemo.SVMDemoProgram.Main()
                Return
            End If

            Dim reportDirectory As String = Path.Combine(AppContext.BaseDirectory, "report")

            If Not Directory.Exists(reportDirectory) Then
                Call Directory.CreateDirectory(reportDirectory)
            End If

            Call DebugAudit.Reset()
            Call DebugAudit.Mark("bayesian classifier and regression verification started")

            Call TestDiscreteClassifier()
            Call TestGaussianClassifier()
            Call TestSmoothingAndHighDimensions()
            Call TestInputValidation()
            Call TestDiscreteRegression()
            Call TestContinuousRegression()
            Call TestUnifiedApiAndLabelWriteback()
            Call TestDegenerateTargets()

            Dim reportPath As String = Path.Combine(reportDirectory, "bayesian-regression-report.txt")
            Dim jsonPath As String = Path.Combine(reportDirectory, "bayesian-regression-report.json")

            Call DebugAudit.SaveReport(reportPath)
            Call File.WriteAllText(jsonPath, DebugAudit.ToJson())
            Call Console.WriteLine(DebugAudit.Dump())

            If DebugAudit.FailureCount > 0 Then
                Call Console.WriteLine($"{DebugAudit.FailureCount} assertion(s) FAILED, see {reportPath}")
                Environment.ExitCode = 1
            Else
                Call Console.WriteLine("all assertions passed.")
                Call Console.WriteLine($"report: {reportPath}")
            End If
        End Sub

        ''' <summary>两类、两维整数特征的确定性分类数据</summary>
        Private Function MakeClassTable() As NumericTable
            Dim x As New List(Of Double())()
            Dim y As New List(Of Integer)()

            For i As Integer = 0 To 29
                x.Add(New Double() {0.0, 0.0})
                y.Add(0)
                x.Add(New Double() {0.0, 1.0})
                y.Add(0)
                x.Add(New Double() {1.0, 0.0})
                y.Add(0)
                x.Add(New Double() {2.0, 2.0})
                y.Add(1)
                x.Add(New Double() {2.0, 3.0})
                y.Add(1)
                x.Add(New Double() {3.0, 2.0})
                y.Add(1)
            Next

            Dim table As New NumericTable(x, y)

            table.featureNames = New String() {"a", "b"}
            table.labelNames = New String() {"class"}

            Return table
        End Function

        ''' <summary>离散特征 + 连续目标：y = 10*f + 5，f 为 0..4 的类别编码</summary>
        Private Function MakeDiscreteRegressionTable() As NumericTable
            Dim x As New List(Of Double())()
            Dim y As New List(Of Double)()

            For f As Integer = 0 To 4
                For r As Integer = 0 To 19
                    x.Add(New Double() {CDbl(f)})
                    y.Add(10.0 * f + 5.0)
                Next
            Next

            Dim table As New NumericTable(x, y)

            table.featureNames = New String() {"f"}
            table.labelNames = New String() {"target"}

            Return table
        End Function

        ''' <summary>连续特征 + 连续目标：每个目标箱对应一组二维高斯簇</summary>
        Private Function MakeContinuousRegressionTable() As NumericTable
            Dim x As New List(Of Double())()
            Dim y As New List(Of Double)()
            Dim i As Integer = 0

            For f As Integer = 0 To 5
                For r As Integer = 0 To 39
                    x.Add(New Double() {
                        f + 0.05 * System.Math.Sin(i),
                        -f + 0.05 * System.Math.Cos(i)
                    })
                    y.Add(3.0 * f + 2.0)
                    i += 1
                Next
            Next

            Dim table As New NumericTable(x, y)

            table.featureNames = New String() {"x1", "x2"}
            table.labelNames = New String() {"target"}

            Return table
        End Function

        Private Function MeanSquaredError(actual As Double(), predicted As Double()) As Double
            Dim sum As Double = 0.0

            For i As Integer = 0 To actual.Length - 1
                Dim d As Double = actual(i) - predicted(i)
                sum += d * d
            Next

            Return sum / actual.Length
        End Function

        Private Function MeanAbsoluteError(actual As Double(), predicted As Double()) As Double
            Dim sum As Double = 0.0

            For i As Integer = 0 To actual.Length - 1
                sum += System.Math.Abs(actual(i) - predicted(i))
            Next

            Return sum / actual.Length
        End Function

        Private Function PosteriorIsNormalized(p As Double()) As Boolean
            If p Is Nothing OrElse p.Length = 0 Then
                Return False
            End If
            If p.Any(Function(v) Double.IsNaN(v) OrElse Double.IsInfinity(v) OrElse v < 0.0) Then
                Return False
            End If

            Return System.Math.Abs(p.Sum() - 1.0) < 0.000000001
        End Function

        ''' <summary>分类正确性、后验归一性、条件似然与未知类别</summary>
        Private Sub TestDiscreteClassifier()
            Call DebugAudit.Mark("discrete classifier test")

            Dim table As NumericTable = MakeClassTable()
            Dim classifier As Bayesian = Bayesian.Load(table, 0)

            Call DebugAudit.AreEqual(2, classifier.ClassCount, "the training set contains two classes")
            Call DebugAudit.AreEqual(2, classifier.FeatureWidth, "the training set contains two feature columns")
            Call DebugAudit.AreEqual(180, classifier.SampleSize, "all 180 training samples were loaded")

            Call DebugAudit.AreEqual(0, classifier.Classify(New Double() {0.0, 0.0}),
                                     "a (0,0) sample is assigned to class 0")
            Call DebugAudit.AreEqual(1, classifier.Classify(New Double() {3.0, 2.0}),
                                     "a (3,2) sample is assigned to class 1")

            Dim posterior As Double() = classifier.Posterior(New Double() {0.0, 0.0})

            Call DebugAudit.AreEqual(2, posterior.Length, "posterior has one entry per class")
            Call DebugAudit.Test(PosteriorIsNormalized(posterior), "posterior probabilities sum to one")
            Call DebugAudit.Test(posterior(0) > 0.5, "class 0 is the most probable class for (0,0)")

            Dim likelihood As Double = classifier.P(New Double() {0.0, 0.0}, 0)

            Call DebugAudit.Test(likelihood > 0.0 AndAlso likelihood <= 1.0,
                                 "P(X|Y) is a normalized conditional likelihood")
            Call DebugAudit.Test(classifier.LogP(New Double() {0.0, 0.0}, 0) < 0.0,
                                 "log P(X|Y) is available in the log domain")
            Call DebugAudit.Test(Double.IsNegativeInfinity(classifier.LogP(New Double() {0.0, 0.0}, 999)),
                                 "an unknown class label has a zero likelihood")

            Call DebugAudit.Mark("discrete classifier class count", classifier.ClassCount)
            Call DebugAudit.Mark("discrete classifier log prior", classifier.LogPriors(0))
        End Sub

        ''' <summary>高斯路径的分类能力与方差下限保护</summary>
        Private Sub TestGaussianClassifier()
            Call DebugAudit.Mark("gaussian classifier test")

            Dim x As New List(Of Double())()
            Dim y As New List(Of Integer)()

            For i As Integer = 0 To 39
                x.Add(New Double() {0.1 * System.Math.Sin(i), 0.1 * System.Math.Cos(i)})
                y.Add(0)
                x.Add(New Double() {5.0 + 0.1 * System.Math.Sin(i), 5.0 + 0.1 * System.Math.Cos(i)})
                y.Add(1)
            Next

            Dim gaussian As GaussianModel = GaussianModel.Load(x.ToArray(), y.ToArray())

            Call DebugAudit.AreEqual(0, gaussian.Classify(New Double() {0.0, 0.0}),
                                     "gaussian path classifies the (0,0) cluster as class 0")
            Call DebugAudit.AreEqual(1, gaussian.Classify(New Double() {5.0, 5.0}),
                                     "gaussian path classifies the (5,5) cluster as class 1")
            Call DebugAudit.Test(PosteriorIsNormalized(gaussian.Posterior(New Double() {5.0, 5.0})),
                                 "gaussian posterior probabilities sum to one")
            Call DebugAudit.Test(gaussian.Variances(0)(0) >= gaussian.VarianceFloor,
                                 "the gaussian variance is protected by the configured floor")

            ' 第一维在所有样本中恒定，方差为 0，必须被下限保护且后验仍然可用
            Dim constantX As Double()() = {
                New Double() {1.0, 0.0}, New Double() {1.0, 0.1},
                New Double() {1.0, 1.0}, New Double() {1.0, 1.1}
            }
            Dim constantY As Integer() = {0, 0, 1, 1}
            Dim zeroVariance As GaussianModel = GaussianModel.Load(constantX, constantY)

            Call DebugAudit.Test(
                System.Math.Abs(zeroVariance.Variances(0)(0) - zeroVariance.VarianceFloor) < 0.000000001,
                "a constant feature dimension falls back to the variance floor")
            Call DebugAudit.Test(PosteriorIsNormalized(zeroVariance.Posterior(New Double() {1.0, 0.5})),
                                 "a zero-variance feature does not break the posterior")
        End Sub

        ''' <summary>零频平滑与高维特征下的 log 域数值稳定性</summary>
        Private Sub TestSmoothingAndHighDimensions()
            Call DebugAudit.Mark("smoothing and numerical stability test")

            Dim classifier As Bayesian = Bayesian.Load(MakeClassTable(), 0)
            Dim unseen As Double() = classifier.Posterior(New Double() {99.0, 99.0})

            Call DebugAudit.Test(unseen.All(Function(v) v > 0.0),
                                 "additive smoothing keeps unseen feature values away from zero probability")
            Call DebugAudit.Test(PosteriorIsNormalized(unseen),
                                 "the posterior of an unseen feature vector is still normalized")

            Dim width As Integer = 2000
            Dim x As New List(Of Double())()
            Dim y As New List(Of Integer)()

            For c As Integer = 0 To 1
                For r As Integer = 0 To 4
                    Dim row As Double() = New Double(width - 1) {}

                    For j As Integer = 0 To width - 1
                        row(j) = CDbl(c)
                    Next

                    x.Add(row)
                    y.Add(c)
                Next
            Next

            Dim highDim As Bayesian = Bayesian.Load(x.ToArray(), y.ToArray())
            Dim query As Double() = New Double(width - 1) {}
            Dim posterior As Double() = highDim.Posterior(query)
            Dim rawLikelihood As Double = highDim.P(query, 0)

            Call DebugAudit.Test(PosteriorIsNormalized(posterior),
                                 "the posterior stays normalized with 2000 features")
            Call DebugAudit.Test(posterior(0) > 0.99,
                                 "log-domain inference stays accurate when the raw likelihood product collapses")
            Call DebugAudit.Test(Not Double.IsInfinity(highDim.LogP(query, 0)),
                                 "LogP remains finite in the log domain")
            Call DebugAudit.Test(highDim.LogP(query, 0) < -100.0,
                                 "LogP reflects the accumulated log likelihood across 2000 features")

            Call DebugAudit.Mark("high dimensional raw P(X|Y)", rawLikelihood)
            Call DebugAudit.Mark("high dimensional log P(X|Y)", highDim.LogP(query, 0))
        End Sub

        ''' <summary>空表、维度不符、非法参数与未训练模型等边界</summary>
        Private Sub TestInputValidation()
            Call DebugAudit.Mark("input validation test")

            Dim empty As New NumericTable()

            Call DebugAudit.CheckThrows(Of ArgumentException)(
                Sub()
                    Dim ignored As Bayesian = Bayesian.Load(empty, 0)
                End Sub,
                "an empty training table is rejected")

            Call DebugAudit.CheckThrows(Of ArgumentOutOfRangeException)(
                Sub()
                    Dim ignored As Bayesian = Bayesian.Load(MakeClassTable(), 0, 0.0)
                End Sub,
                "a non-positive smoothing factor is rejected")

            Call DebugAudit.CheckThrows(Of ArgumentOutOfRangeException)(
                Sub()
                    Dim ignored As GaussianModel = GaussianModel.Load(MakeClassTable(), 0, 0.0)
                End Sub,
                "a non-positive variance floor is rejected")

            Dim classifier As Bayesian = Bayesian.Load(MakeClassTable(), 0)

            Call DebugAudit.CheckThrows(Of ArgumentException)(
                Sub()
                    Dim ignored As Integer = classifier.Classify(New Double() {0.0, 0.0, 0.0})
                End Sub,
                "a feature vector with the wrong width is rejected")

            Call DebugAudit.CheckThrows(Of ArgumentException)(
                Sub()
                    Dim ignored As Binning = Binning.Fit(New Double() {}, 5)
                End Sub,
                "an empty target vector is rejected")

            Call DebugAudit.CheckThrows(Of ArgumentOutOfRangeException)(
                Sub()
                    Dim regression As New BayesRegression()
                    Call regression.Train(MakeDiscreteRegressionTable(), 99)
                End Sub,
                "an out-of-range target column is rejected")

            Call DebugAudit.CheckThrows(Of InvalidOperationException)(
                Sub()
                    Dim regression As New BayesRegression()
                    Dim ignored As Double = regression.Predict(New Double() {0.0})
                End Sub,
                "prediction before training is rejected")
        End Sub

        ''' <summary>离散特征回归路径的精度与预测范围</summary>
        Private Sub TestDiscreteRegression()
            Call DebugAudit.Mark("discrete feature regression test")

            Dim train As NumericTable = MakeDiscreteRegressionTable()
            Dim regression As New BayesRegression()

            Call regression.Train(train, 0, BayesMode.Discrete, 5, 0.01)

            Dim testX As Double()() = {
                New Double() {0.0}, New Double() {1.0}, New Double() {2.0},
                New Double() {3.0}, New Double() {4.0}
            }
            Dim testY As Double() = {5.0, 15.0, 25.0, 35.0, 45.0}
            Dim predicted As Double() = regression.Predict(testX)
            Dim mse As Double = MeanSquaredError(testY, predicted)
            Dim mae As Double = MeanAbsoluteError(testY, predicted)
            Dim baseline As Double = MeanSquaredError(testY, Enumerable.Repeat(25.0, 5).ToArray())

            Call DebugAudit.Test(regression.IsTrained, "the discrete regression model is trained")
            Call DebugAudit.AreEqual(1, regression.FeatureWidth, "the discrete model expects one feature")
            Call DebugAudit.AreEqual(BayesMode.Discrete, regression.Mode, "the discrete path was selected")
            Call DebugAudit.Test(PosteriorIsNormalized(regression.PredictPosterior(New Double() {2.0})),
                                 "the regression posterior is normalized")
            Call DebugAudit.Test(mse < baseline, "the discrete model beats the constant-mean baseline")
            Call DebugAudit.Test(mse < 10.0, $"discrete regression MSE is small (actual {mse})")
            Call DebugAudit.Test(mae < 2.5, $"discrete regression MAE is small (actual {mae})")
            Call DebugAudit.Test(predicted.All(Function(v) v >= 4.0 AndAlso v <= 46.0),
                                 "discrete predictions stay inside the observed target range")

            Call DebugAudit.Mark("discrete regression MSE", mse)
            Call DebugAudit.Mark("discrete regression MAE", mae)
            Call DebugAudit.Mark("discrete regression baseline MSE", baseline)
            Call DebugAudit.Mark("discrete regression bin count", regression.Binning.BinCount)
        End Sub

        ''' <summary>连续特征回归路径的精度与预测范围</summary>
        Private Sub TestContinuousRegression()
            Call DebugAudit.Mark("continuous feature regression test")

            Dim train As NumericTable = MakeContinuousRegressionTable()
            Dim regression As New BayesRegression()

            Call regression.Train(train, 0, BayesMode.Continuous, 6, 0.01, GaussianModel.DefaultVarianceFloor)

            Dim testX As Double()() = {
                New Double() {0.03, -0.03}, New Double() {1.03, -1.03},
                New Double() {2.03, -2.03}, New Double() {3.03, -3.03},
                New Double() {4.03, -4.03}, New Double() {5.03, -5.03}
            }
            Dim testY As Double() = {2.0, 5.0, 8.0, 11.0, 14.0, 17.0}
            Dim predicted As Double() = regression.Predict(testX)
            Dim mse As Double = MeanSquaredError(testY, predicted)
            Dim mae As Double = MeanAbsoluteError(testY, predicted)

            Call DebugAudit.AreEqual(BayesMode.Continuous, regression.Mode, "the continuous path was selected")
            Call DebugAudit.AreEqual(2, regression.FeatureWidth, "the continuous model expects two features")
            Call DebugAudit.Test(PosteriorIsNormalized(regression.PredictPosterior(testX(3))),
                                 "the continuous regression posterior is normalized")
            Call DebugAudit.Test(mse < 20.0, $"continuous regression MSE is small (actual {mse})")
            Call DebugAudit.Test(mae < 3.0, $"continuous regression MAE is small (actual {mae})")
            Call DebugAudit.Test(predicted.All(Function(v) v >= 1.0 AndAlso v <= 18.0),
                                 "continuous predictions stay inside the observed target range")

            Call DebugAudit.Mark("continuous regression MSE", mse)
            Call DebugAudit.Mark("continuous regression MAE", mae)
            Call DebugAudit.Mark("continuous regression bin count", regression.Binning.BinCount)
        End Sub

        ''' <summary>统一 API 的批量预测、表名训练入口与 SetLabel 写回</summary>
        Private Sub TestUnifiedApiAndLabelWriteback()
            Call DebugAudit.Mark("unified api and label writeback test")

            Dim table As NumericTable = MakeDiscreteRegressionTable()
            Dim byName As New BayesRegression()
            Dim byVectors As New BayesRegression()

            Call byName.Train(table, "target", BayesMode.Discrete, 5, 0.01)
            Call byVectors.Train(table.features, table.GetLabel(0), BayesMode.Discrete, 5, 0.01)

            Dim probe As Double() = New Double() {3.0}

            Call DebugAudit.Test(
                System.Math.Abs(byName.Predict(probe) - byVectors.Predict(probe)) < 0.000000001,
                "the table-name and the matrix training entries produce identical models")
            Call DebugAudit.AreEqual(table.nsamples, byName.Predict(table).Length,
                                     "batch prediction returns one value per sample")

            Dim posterior As Double() = byName.PredictPosterior(probe)

            Call DebugAudit.AreEqual(byName.PosteriorModel.ClassCount, posterior.Length,
                                     "the posterior vector is aligned with the fitted classes")

            Call byName.Apply(table, "bayes_prediction")

            Dim written As Double() = table.GetLabel("bayes_prediction")
            Dim expected As Double() = byName.Predict(table)

            Call DebugAudit.Test(written.Length = expected.Length,
                                 "the written label column has one value per sample")
            Call DebugAudit.Test(
                Enumerable.Range(0, written.Length).All(
                    Function(i) System.Math.Abs(written(i) - expected(i)) < 0.000000001),
                "SetLabel writeback matches the batch prediction")

            Call DebugAudit.Mark("unified api sample count", table.nsamples)
        End Sub

        ''' <summary>目标恒定、超范围目标与单箱退化情形</summary>
        Private Sub TestDegenerateTargets()
            Call DebugAudit.Mark("degenerate target test")

            Dim constantBins As Binning = Binning.Fit({7.5, 7.5, 7.5, 7.5}, 5)

            Call DebugAudit.AreEqual(1, constantBins.BinCount, "a constant target collapses to a single bin")
            Call DebugAudit.Test(System.Math.Abs(constantBins.RepresentativeAt(0) - 7.5) < 0.000000001,
                                 "the single representative is the constant target value")

            Dim x As Double()() = {
                New Double() {0.0}, New Double() {1.0},
                New Double() {2.0}, New Double() {3.0}
            }
            Dim y As Double() = {7.5, 7.5, 7.5, 7.5}
            Dim regression As New BayesRegression()

            Call regression.Train(x, y, BayesMode.Discrete, 5)

            Call DebugAudit.Test(System.Math.Abs(regression.Predict(New Double() {2.0}) - 7.5) < 0.000000001,
                                 "a constant target is predicted exactly")

            Dim edges As Binning = Binning.Fit({0.0, 1.0, 2.0, 3.0}, 4)

            Call DebugAudit.AreEqual(0, edges.BinIndex(-100.0), "values below the range are clamped to bin 0")
            Call DebugAudit.AreEqual(3, edges.BinIndex(100.0), "values above the range are clamped to the last bin")
            Call DebugAudit.Test(edges.LowerEdge(2) > edges.LowerEdge(1), "bin lower edges are strictly increasing")
        End Sub

    End Module
End Namespace
