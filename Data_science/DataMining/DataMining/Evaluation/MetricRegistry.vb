#Region "Microsoft.VisualBasic::56ceb9a4319409fe11b0cb13a4d1122f, Data_science\DataMining\DataMining\Evaluation\MetricRegistry.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xie (genetics@smrucc.org)
'       xieguigang (xie.guigang@live.com)
' 
' Copyright (c) 2018 GPL3 Licensed
' 
' 
' GNU GENERAL PUBLIC LICENSE (GPL3)
' 
' 
' This program is free software: you can redistribute it and/or modify
' it under the terms of the GNU General Public License as published by
' the Free Software Foundation, either version 3 of the License, or
' (at your option) any later version.
' 
' This program is distributed in the hope that it will be useful,
' but WITHOUT ANY WARRANTY; without even the implied warranty of
' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
' GNU General Public License for more details.
' 
' You should have received a copy of the GNU General Public License
' along with this program. If not, see <http://www.gnu.org/licenses/>.



' /********************************************************************************/

' Summaries:


' Code Statistics:

'   Total Lines: 42
'    Code Lines: 29 (69.05%)
' Comment Lines: 3 (7.14%)
'    - Xml Docs: 100.00%
' 
'   Blank Lines: 10 (23.81%)
'     File Size: 1.48 KB


'  
' 
'     Function: BinaryLabels, External, MatthewsCorrelationCoefficient, Silhouette
' 
' 
' /********************************************************************************/

#End Region

Imports std = System.Math

Namespace Evaluation

    ''' <summary>
    ''' 一个具名评估指标：声明它适用于哪一类结果数据，以及如何计算。
    ''' </summary>
    Public Class NamedMetric

        ''' <summary>
        ''' 指标名字（报告之中的键）。
        ''' </summary>
        ''' <returns></returns>
        Public Property Name As String

        ''' <summary>
        ''' 该指标所适用的结果种类。
        ''' </summary>
        ''' <returns></returns>
        Public Property Kind As ResultKinds

        ''' <summary>
        ''' 指标说明。
        ''' </summary>
        ''' <returns></returns>
        Public Property Description As String

        ''' <summary>
        ''' 指标的计算函数。
        ''' </summary>
        ''' <returns></returns>
        Public Property Evaluate As Func(Of IEvaluationResult, Double)

    End Class

    ''' <summary>
    ''' 可扩展的评估指标注册表。
    ''' 
    ''' 内置了分类/回归/聚类三类结果的标准指标，并允许
    ''' 通过 <see cref="Register(NamedMetric)"/> 注册自定义指标，
    ''' 从而把「统一评估框架」扩展到新的度量之上。
    ''' </summary>
    Public NotInheritable Class MetricRegistry

        Private Shared ReadOnly _metrics As New Dictionary(Of ResultKinds, List(Of NamedMetric))

        Shared Sub New()
            Register(ClassificationMetrics())
            Register(RegressionMetrics())
            Register(ClusteringMetrics())
        End Sub

        ''' <summary>
        ''' 注册一个自定义指标（同名的指标会被覆盖）。
        ''' </summary>
        ''' <param name="metric"></param>
        Public Shared Sub Register(metric As NamedMetric)
            If metric Is Nothing OrElse String.IsNullOrEmpty(metric.Name) Then
                Throw New ArgumentNullException("the metric name can not be empty!")
            End If

            If Not _metrics.ContainsKey(metric.Kind) Then
                _metrics(metric.Kind) = New List(Of NamedMetric)()
            End If

            Dim list As List(Of NamedMetric) = _metrics(metric.Kind)
            Dim index As Integer = list.FindIndex(Function(m) String.Equals(m.Name, metric.Name, StringComparison.OrdinalIgnoreCase))

            If index >= 0 Then
                list(index) = metric
            Else
                list.Add(metric)
            End If
        End Sub

        Public Shared Sub Register(metrics As IEnumerable(Of NamedMetric))
            For Each metric As NamedMetric In metrics
                Register(metric)
            Next
        End Sub

        ''' <summary>
        ''' 某一类结果数据所注册的全部指标。
        ''' </summary>
        ''' <param name="kind"></param>
        ''' <returns></returns>
        Public Shared Function Metrics(kind As ResultKinds) As NamedMetric()
            If _metrics.ContainsKey(kind) Then
                Return _metrics(kind).ToArray
            Else
                Return New NamedMetric() {}
            End If
        End Function

        ''' <summary>
        ''' 某一类结果数据所注册的全部指标名字。
        ''' </summary>
        ''' <param name="kind"></param>
        ''' <returns></returns>
        Public Shared Function Names(kind As ResultKinds) As String()
            Return Metrics(kind).Select(Function(m) m.Name).ToArray
        End Function

        ''' <summary>
        ''' 计算一个结果数据的完整评估报告。
        ''' </summary>
        ''' <param name="result"></param>
        ''' <returns></returns>
        Public Shared Function Compute(result As IEvaluationResult) As EvaluationReport
            If result Is Nothing Then
                Throw New ArgumentNullException(NameOf(result))
            End If

            Dim report As New EvaluationReport With {
                .Kind = result.Kind,
                .Name = result.Name,
                .Metrics = New Dictionary(Of String, Double)()
            }

            For Each metric As NamedMetric In Metrics(result.Kind)
                Try
                    report.Metrics(metric.Name) = metric.Evaluate(result)
                Catch ex As Exception
                    ' 单个指标计算失败的时候不应该让整个评估流程崩溃
                    report.Metrics(metric.Name) = Double.NaN
                    Call ex.Message.warning
                End Try
            Next

            Dim roc As IRocResult = TryCast(result, IRocResult)

            If roc IsNot Nothing Then
                Try
                    report.Curve = roc.Curve()
                Catch ex As Exception
                    Call ex.Message.warning
                End Try
            End If

            Return report
        End Function

#Region "built-in metrics"

        Private Shared Function ClassificationMetrics() As NamedMetric()
            Return {
                New NamedMetric With {
                    .Name = "acc",
                    .Kind = ResultKinds.classification,
                    .Description = "准确率",
                    .Evaluate = Function(r) Metric.accuracy(CType(r, ClassificationResult).Scores, BinaryLabels(CType(r, ClassificationResult)))
                },
                New NamedMetric With {
                    .Name = "error",
                    .Kind = ResultKinds.classification,
                    .Description = "错误率 (1 - acc)",
                    .Evaluate = Function(r) 1.0 - Metric.accuracy(CType(r, ClassificationResult).Scores, BinaryLabels(CType(r, ClassificationResult)))
                },
                New NamedMetric With {
                    .Name = "precision",
                    .Kind = ResultKinds.classification,
                    .Description = "精确率 (PPV)",
                    .Evaluate = Function(r) CType(r, ClassificationResult).Confusion().Precision
                },
                New NamedMetric With {
                    .Name = "recall",
                    .Kind = ResultKinds.classification,
                    .Description = "召回率 / 灵敏度 (TPR)",
                    .Evaluate = Function(r) CType(r, ClassificationResult).Confusion().Sensibility
                },
                New NamedMetric With {
                    .Name = "specificity",
                    .Kind = ResultKinds.classification,
                    .Description = "特异度 (TNR)",
                    .Evaluate = Function(r) CType(r, ClassificationResult).Confusion().Specificity
                },
                New NamedMetric With {
                    .Name = "f1",
                    .Kind = ResultKinds.classification,
                    .Description = "F1 分数",
                    .Evaluate = Function(r) CType(r, ClassificationResult).Confusion().F1Score
                },
                New NamedMetric With {
                    .Name = "mcc",
                    .Kind = ResultKinds.classification,
                    .Description = "马修斯相关系数",
                    .Evaluate = Function(r) MatthewsCorrelationCoefficient(CType(r, ClassificationResult).Confusion())
                },
                New NamedMetric With {
                    .Name = "auc",
                    .Kind = ResultKinds.classification,
                    .Description = "精确秩和 AUC",
                    .Evaluate = Function(r) RocAuc.RankAUC(CType(r, ClassificationResult).Scores, CType(r, ClassificationResult).PositiveLabels)
                },
                New NamedMetric With {
                    .Name = "auc_curve",
                    .Kind = ResultKinds.classification,
                    .Description = "ROC 曲线梯形面积",
                    .Evaluate = Function(r) CType(r, ClassificationResult).Curve().AUC
                },
                New NamedMetric With {
                    .Name = "mse",
                    .Kind = ResultKinds.classification,
                    .Description = "均方误差",
                    .Evaluate = Function(r) Metric.mean_square_error(CType(r, ClassificationResult).Scores, BinaryLabels(CType(r, ClassificationResult)))
                },
                New NamedMetric With {
                    .Name = "mae",
                    .Kind = ResultKinds.classification,
                    .Description = "平均绝对误差",
                    .Evaluate = Function(r) Metric.mean_absolute_error(CType(r, ClassificationResult).Scores, BinaryLabels(CType(r, ClassificationResult)))
                },
                New NamedMetric With {
                    .Name = "logloss",
                    .Kind = ResultKinds.classification,
                    .Description = "交叉熵损失",
                    .Evaluate = Function(r) Metric.crossEntropyLoss(CType(r, ClassificationResult).Scores, BinaryLabels(CType(r, ClassificationResult)))
                }
            }
        End Function

        Private Shared Function RegressionMetrics() As NamedMetric()
            Return {
                New NamedMetric With {
                    .Name = "mse",
                    .Kind = ResultKinds.regression,
                    .Description = "均方误差",
                    .Evaluate = Function(r) Metric.mean_square_error(CType(r, RegressionResult).Predictions, CType(r, RegressionResult).Actuals)
                },
                New NamedMetric With {
                    .Name = "rmse",
                    .Kind = ResultKinds.regression,
                    .Description = "均方根误差",
                    .Evaluate = Function(r) Metric.root_mean_square_error(CType(r, RegressionResult).Predictions, CType(r, RegressionResult).Actuals)
                },
                New NamedMetric With {
                    .Name = "mae",
                    .Kind = ResultKinds.regression,
                    .Description = "平均绝对误差",
                    .Evaluate = Function(r) Metric.mean_absolute_error(CType(r, RegressionResult).Predictions, CType(r, RegressionResult).Actuals)
                },
                New NamedMetric With {
                    .Name = "r2",
                    .Kind = ResultKinds.regression,
                    .Description = "决定系数 R²",
                    .Evaluate = Function(r) Metric.r2_score(CType(r, RegressionResult).Predictions, CType(r, RegressionResult).Actuals)
                },
                New NamedMetric With {
                    .Name = "auc",
                    .Kind = ResultKinds.regression,
                    .Description = "回归阈值扫描 ROC 的曲线面积",
                    .Evaluate = Function(r) CType(r, RegressionResult).Curve().AUC
                }
            }
        End Function

        Private Shared Function ClusteringMetrics() As NamedMetric()
            Return {
                New NamedMetric With {
                    .Name = "silhouette",
                    .Kind = ResultKinds.clustering,
                    .Description = "轮廓系数",
                    .Evaluate = Function(r) Silhouette(CType(r, ClusteringResult))
                },
                New NamedMetric With {
                    .Name = "dunn",
                    .Kind = ResultKinds.clustering,
                    .Description = "Dunn 指数",
                    .Evaluate = Function(r) ClusteringIndices.Dunn(CType(r, ClusteringResult).Features, CType(r, ClusteringResult).ClusterLabels)
                },
                New NamedMetric With {
                    .Name = "davies_bouldin",
                    .Kind = ResultKinds.clustering,
                    .Description = "Davies–Bouldin 指数（越小越好）",
                    .Evaluate = Function(r) ClusteringIndices.DaviesBouldin(CType(r, ClusteringResult).Features, CType(r, ClusteringResult).ClusterLabels)
                },
                New NamedMetric With {
                    .Name = "calinski_harabasz",
                    .Kind = ResultKinds.clustering,
                    .Description = "Calinski–Harabasz 指数",
                    .Evaluate = Function(r) ClusteringIndices.CalinskiHarabasz(CType(r, ClusteringResult).Features, CType(r, ClusteringResult).ClusterLabels)
                },
                New NamedMetric With {
                    .Name = "maximum_diameter",
                    .Kind = ResultKinds.clustering,
                    .Description = "最大簇内直径",
                    .Evaluate = Function(r) ClusteringIndices.MaximumDiameter(CType(r, ClusteringResult).Features, CType(r, ClusteringResult).ClusterLabels)
                },
                New NamedMetric With {
                    .Name = "purity",
                    .Kind = ResultKinds.clustering,
                    .Description = "纯度（需要真值标签）",
                    .Evaluate = Function(r) External(r, Function(c) ClusteringIndices.Purity(c.ClusterLabels, c.GroundTruth))
                },
                New NamedMetric With {
                    .Name = "ari",
                    .Kind = ResultKinds.clustering,
                    .Description = "调整兰德指数（需要真值标签）",
                    .Evaluate = Function(r) External(r, Function(c) ClusteringIndices.AdjustedRandIndex(c.ClusterLabels, c.GroundTruth))
                },
                New NamedMetric With {
                    .Name = "nmi",
                    .Kind = ResultKinds.clustering,
                    .Description = "标准化互信息（需要真值标签）",
                    .Evaluate = Function(r) External(r, Function(c) ClusteringIndices.NormalizedMutualInformation(c.ClusterLabels, c.GroundTruth))
                }
            }
        End Function

#End Region

#Region "helpers"

        Private Shared Function BinaryLabels(result As ClassificationResult) As Double()
            Return result.PositiveLabels.Select(Function(b) If(b, 1.0, 0.0)).ToArray
        End Function

        ''' <summary>
        ''' 计算外部聚类指标；当没有提供真值标签的时候返回 ``NaN``。
        ''' </summary>
        Private Shared Function External(result As ClusteringResult, evaluate As Func(Of ClusteringResult, Double)) As Double
            If result.HasGroundTruth Then
                Return evaluate(result)
            Else
                Return Double.NaN
            End If
        End Function

        Private Shared Function Silhouette(result As ClusteringResult) As Double
            Return ClusteringIndices.Silhouette(result.Features, result.ClusterLabels, result.MaxPoints)
        End Function

        Private Shared Function MatthewsCorrelationCoefficient(confusion As Validation) As Double
            Dim tp As Double = confusion.TP
            Dim tn As Double = confusion.TN
            Dim fp As Double = confusion.FP
            Dim fn As Double = confusion.FN
            Dim denominator As Double = std.Sqrt((tp + fp) * (tp + fn) * (tn + fp) * (tn + fn))

            If denominator = 0 Then
                Return 0
            End If

            Return (tp * tn - fp * fn) / denominator
        End Function

#End Region

    End Class

End Namespace

