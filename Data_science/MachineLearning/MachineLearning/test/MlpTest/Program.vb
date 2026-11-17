' Copyright (c) sciBASIC.NET Foundation
' GNU GENERAL PUBLIC LICENSE (GPL3) or later
'
' Console verification for the multilayer perceptron implementation.
' All assertions are recorded with DebugAudit and exported as text/JSON reports.

Imports System
Imports System.IO
Imports System.Linq
Imports Microsoft.VisualBasic.ApplicationServices.Debugging
Imports Microsoft.VisualBasic.MachineLearning.ComponentModel.StoreProcedure
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
Imports ActiveFunction = Microsoft.VisualBasic.MachineLearning.ComponentModel.Activations.ActiveFunction
Imports MlpModel = Microsoft.VisualBasic.MachineLearning.MultilayerPerceptron.MultilayerPerceptron
Imports MlpTask = Microsoft.VisualBasic.MachineLearning.MultilayerPerceptron.MlpTask
Imports OptimizerKind = Microsoft.VisualBasic.MachineLearning.MultilayerPerceptron.OptimizerKind
Imports TrainingOptions = Microsoft.VisualBasic.MachineLearning.MultilayerPerceptron.TrainingOptions
Imports std = System.Math

Namespace MlpTest

    ''' <summary>
    ''' 多层感知机的分类与回归可运行验证入口。
    ''' </summary>
    Public Module Program

        Public Sub Main()
            Dim reportDirectory As String = Path.Combine(AppContext.BaseDirectory, "report")

            If Not Directory.Exists(reportDirectory) Then
                Call Directory.CreateDirectory(reportDirectory)
            End If

            Call DebugAudit.Reset()
            Call DebugAudit.Mark("multilayer perceptron verification started")

            Call TestClassification()
            Call TestRegression()
            Call TestDataSetAdapter()

            Dim reportPath As String = Path.Combine(reportDirectory, "mlp-test-report.txt")
            Dim jsonPath As String = Path.Combine(reportDirectory, "mlp-test-report.json")

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

        ''' <summary>两类线性可分数据的训练与预测验证。</summary>
        Private Sub TestClassification()
            Dim features As Double()() = {
                New Double() {0.0, 0.0},
                New Double() {0.0, 1.0},
                New Double() {1.0, 0.0},
                New Double() {1.0, 1.0}
            }
            Dim labels As Double()() = {
                New Double() {1.0, 0.0},
                New Double() {1.0, 0.0},
                New Double() {1.0, 0.0},
                New Double() {0.0, 1.0}
            }
            Dim model As New MlpModel(2, {4}, 2, MlpTask.Classification,
                                       ActiveFunction.Parse("HyperbolicTangent()"), seed:=42)
            Dim options As New TrainingOptions With {
                .Epochs = 400,
                .LearningRate = 0.05,
                .BatchSize = 4,
                .Optimizer = OptimizerKind.Adam
            }
            Dim history As Double() = model.Fit(New NumericMatrix(features), New NumericMatrix(labels), options)
            Dim predicted As Integer() = model.PredictLabels(New NumericMatrix(features))
            Dim expected As Integer() = {0, 0, 0, 1}
            Dim correct As Integer = 0

            For i As Integer = 0 To expected.Length - 1
                If predicted(i) = expected(i) Then
                    correct += 1
                End If
            Next

            Call DebugAudit.Test(history.Length > 0, "分类训练应当产生损失历史")
            Call DebugAudit.Test(history(history.Length - 1) < history(0), "分类损失应当随训练下降")
            Call DebugAudit.AreEqual(4, correct, "线性可分分类样本应当全部预测正确")
            Call DebugAudit.Mark("classification",
                                 $"loss {history(0):G4} -> {history(history.Length - 1):G4}, accuracy={correct}/4")
            Call Console.WriteLine($"  [classification] accuracy={correct}/4, loss={history(history.Length - 1):G4}")
        End Sub

        ''' <summary>一元线性回归的训练与预测验证。</summary>
        Private Sub TestRegression()
            Dim features As New System.Collections.Generic.List(Of Double())()
            Dim labels As New System.Collections.Generic.List(Of Double())()

            For i As Integer = 0 To 11
                Dim x As Double = i / 11.0 - 0.5
                features.Add(New Double() {x})
                labels.Add(New Double() {2.0 * x + 0.25})
            Next

            Dim model As New MlpModel(1, {6}, 1, MlpTask.Regression,
                                       ActiveFunction.Parse("HyperbolicTangent()"), seed:=7)
            Dim options As New TrainingOptions With {
                .Epochs = 600,
                .LearningRate = 0.03,
                .BatchSize = 6,
                .Optimizer = OptimizerKind.Adam
            }
            Dim history As Double() = model.Fit(New NumericMatrix(features.ToArray()),
                                                 New NumericMatrix(labels.ToArray()), options)
            Dim predictions As NumericMatrix = model.Predict(New NumericMatrix(features.ToArray()))
            Dim maxError As Double = 0.0

            For i As Integer = 0 To labels.Count - 1
                maxError = std.Max(maxError, std.Abs(predictions(i, 0) - labels(i)(0)))
            Next

            Call DebugAudit.Test(history(history.Length - 1) < history(0), "回归损失应当随训练下降")
            Call DebugAudit.Test(maxError < 0.15, $"线性回归最大误差应当较小（实际 {maxError:G4}）")
            Call DebugAudit.Mark("regression",
                                 $"loss {history(0):G4} -> {history(history.Length - 1):G4}, maxError={maxError:G4}")
            Call Console.WriteLine($"  [regression] maxError={maxError:G4}, loss={history(history.Length - 1):G4}")
        End Sub

        ''' <summary>DataSet / SampleData 便捷入口的验证。</summary>
        Private Sub TestDataSetAdapter()
            Dim samples As New System.Collections.Generic.List(Of SampleData)

            For i As Integer = 0 To 7
                Dim x As Double = i / 7.0

                If i < 4 Then
                    samples.Add(New SampleData($"a{i}", New Double() {x, 0.1}, 0.0))
                Else
                    samples.Add(New SampleData($"b{i}", New Double() {x, 0.9}, 1.0))
                End If
            Next

            Dim dataset As DataSet = SampleData.CreateDataSet(samples)
            Dim model As New MlpModel(2, {5}, 2, MlpTask.Classification,
                                       ActiveFunction.Parse("HyperbolicTangent()"), seed:=9)
            Dim options As New TrainingOptions With {
                .Epochs = 300,
                .LearningRate = 0.05,
                .BatchSize = 8,
                .Optimizer = OptimizerKind.Adam
            }
            Call model.Fit(dataset, options)

            Dim features As New NumericMatrix(New Double()() {
                New Double() {0.1, 0.05},
                New Double() {0.9, 0.95}
            })
            Dim predicted As Integer() = model.PredictLabels(features)

            Call DebugAudit.AreEqual(0, predicted(0), "DataSet 训练后低值样本应当预测为第一类")
            Call DebugAudit.AreEqual(1, predicted(1), "DataSet 训练后高值样本应当预测为第二类")
            Call DebugAudit.Mark("dataset-adapter", $"labels={String.Join(",", predicted)}")
            Call Console.WriteLine($"  [dataset] labels={String.Join(",", predicted)}")
        End Sub

    End Module

End Namespace
