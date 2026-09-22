Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports Microsoft.VisualBasic.DataMining.Evaluation

''' <summary>
''' 统一评估框架（Evaluation 模块）的数值一致性回归测试。
''' 
''' 重点验证：
''' 
''' 1. 唯一 ROC/AUC 核心（<see cref="RocBuilder"/> / <see cref="RocAuc"/>）与
'''    所有兼容薄封装（<see cref="Validation.AUC"/>、<see cref="ROC.AUC(Double(), Double())"/>、
'''    <see cref="Metric.auc"/>、<see cref="Validation.ROC(Of T)"/>）给出完全一致的结果。
''' 2. 秩和 AUC 与曲线梯形 AUC 在无并列分数的数据上互相一致。
''' 3. <see cref="Validation"/> 的比率字段归一化为 ``[0, 1]``。
''' 4. 三类结果数据（分类/回归/聚类）都能通过 <see cref="ModelEvaluation"/> 得到统一报告。
''' </summary>
Module EvaluationConsistencyTest

    Private Const EPS As Double = 0.000001

    Public Sub Run(failures As List(Of String))
        Call CheckRankAuc(failures)
        Call CheckCurveConsistency(failures)
        Call CheckValidationUnits(failures)
        Call CheckMetricCompatibility(failures)
        Call CheckClassificationReport(failures)
        Call CheckRegressionReport(failures)
        Call CheckClusteringReport(failures)
    End Sub

#Region "ROC / AUC"

    Private Sub CheckRankAuc(failures As List(Of String))
        ' 完全可分：正类分数全部高于负类
        Call AssertNear(failures, "rankauc.perfect",
                        RocAuc.RankAUC({0.9, 0.8, 0.7, 0.6, 0.4, 0.3, 0.2, 0.1},
                                       {1.0, 1.0, 1.0, 1.0, 0.0, 0.0, 0.0, 0.0}), 1.0)

        ' 完全反向
        Call AssertNear(failures, "rankauc.inverted",
                        RocAuc.RankAUC({0.9, 0.8, 0.7, 0.6, 0.4, 0.3, 0.2, 0.1},
                                       {0.0, 0.0, 0.0, 0.0, 1.0, 1.0, 1.0, 1.0}), 0.0)

        ' 手算校验：pos 的秩为 {1, 3}，U = 4 - 3 = 1，AUC = 1 / 4 = 0.25
        Call AssertNear(failures, "rankauc.handwritten",
                        RocAuc.RankAUC({1.0, 2.0, 3.0, 4.0}, {1.0, 0.0, 1.0, 0.0}), 0.25)

        ' 并列分数（ties）：应当取平均秩，结果为 0.5
        Call AssertNear(failures, "rankauc.ties",
                        RocAuc.RankAUC({1.0, 1.0, 2.0, 2.0}, {1.0, 0.0, 1.0, 0.0}), 0.5)

        ' 单类别时 AUC 无定义
        Dim undefined As Double = RocAuc.RankAUC({1.0, 2.0}, {1.0, 1.0})

        If Not Double.IsNaN(undefined) Then
            Call failures.Add($"rankauc.undefined: expected NaN for a single-class input, got {undefined}")
        End If
    End Sub

    Private Sub CheckCurveConsistency(failures As List(Of String))
        ' 无并列分数的时候，秩和 AUC 与曲线梯形 AUC 必须相等
        Dim scores As Double() = {1.0, 2.0, 3.0, 4.0}
        Dim labels As Double() = {1.0, 0.0, 1.0, 0.0}

        Dim rank As Double = RocAuc.RankAUC(scores, labels)
        Dim curve As RocCurve = RocBuilder.FromScores(scores, labels)

        Call AssertNear(failures, "curve.trapezoid-vs-rank", curve.AUC, rank)

        ' 曲线面积（梯形法）与秩和法在 no-ties 数据上一致
        Call AssertNear(failures, "curve.simpleauc",
                        ROC.SimpleAUC(New Microsoft.VisualBasic.Math.LinearAlgebra.Vector({0.0, 0.25, 0.5, 0.75, 1.0}),
                                      New Microsoft.VisualBasic.Math.LinearAlgebra.Vector({0.0, 0.25, 0.5, 0.75, 1.0})), 0.5)

        ' 曲线上的点必须落在 [0, 1] 区间
        For Each point As Validation In curve.Points
            If point.FPR < -EPS OrElse point.FPR > 1 + EPS OrElse point.Sensibility < -EPS OrElse point.Sensibility > 1 + EPS Then
                Call failures.Add($"curve.range: point out of [0,1]: FPR={point.FPR}, TPR={point.Sensibility}")
            End If
        Next
    End Sub

    Private Sub CheckValidationUnits(failures As List(Of String))
        ' ROC 点的比率字段必须归一化为 [0, 1] 的分数，而不是 [0, 100] 的百分数
        Dim point As Validation = Validation.FromConfusion(tp:=5, fp:=5, tn:=5, fn:=5)

        Call AssertNear(failures, "validation.sensibility", point.Sensibility, 0.5)
        Call AssertNear(failures, "validation.specificity", point.Specificity, 0.5)
        Call AssertNear(failures, "validation.accuracy", point.Accuracy, 0.5)
        Call AssertNear(failures, "validation.precision", point.Precision, 0.5)
        Call AssertNear(failures, "validation.fpr", point.FPR, 0.5)
        Call AssertNear(failures, "validation.f1", point.F1Score, 0.5)

        ' 兼容薄封装与核心实现必须完全一致
        Dim curve As RocCurve = RocBuilder.FromScores({0.9, 0.8, 0.4, 0.1}, {1.0, 1.0, 0.0, 0.0})

        Call AssertNear(failures, "validation.auc-wrapper", Validation.AUC(curve.Points), curve.AUC)
        Call AssertNear(failures, "roc.auc-wrapper", curve.Points.AUC(), curve.AUC)
    End Sub

    Private Sub CheckMetricCompatibility(failures As List(Of String))
        Dim scores As Double() = {0.9, 0.8, 0.4, 0.1}
        Dim labels As Double() = {1.0, 1.0, 0.0, 0.0}

        ' xgboost 依赖的 Metrics/IMetric/GetMetric 契约必须保持不变并委托到统一核心
        Dim aucMetric As IMetric = Metric.GetMetric(Metrics.auc)

        Call AssertNear(failures, "metric.auc", aucMetric(scores, labels), RocAuc.RankAUC(scores, labels))
        Call AssertNear(failures, "metric.auc-direct", Metric.auc(scores, labels), RocAuc.RankAUC(scores, labels))

        If Metric.Parse(NameOf(Metrics.auc)) <> Metrics.auc Then
            Call failures.Add("metric.parse: NameOf(Metrics.auc) should be parsed back to Metrics.auc")
        End If

        ' crossEntropyLoss 不允许修改调用方传入的数组
        Dim predictions As Double() = {0.0, 0.5, 1.0, 0.25}

        Call Metric.crossEntropyLoss(predictions, {0.0, 1.0, 1.0, 0.0})

        If predictions(0) <> 0.0 OrElse predictions(2) <> 1.0 Then
            Call failures.Add($"metric.crossentropy: the input predictions were modified in place: {String.Join(", ", predictions)}")
        End If
    End Sub

#End Region

#Region "unified evaluation framework"

    Private Sub CheckClassificationReport(failures As List(Of String))
        Dim result As ClassificationResult = ClassificationResult.Create(
            scores:={0.9, 0.8, 0.7, 0.6, 0.4, 0.3, 0.2, 0.1},
            labels:={1.0, 1.0, 1.0, 1.0, 0.0, 0.0, 0.0, 0.0},
            name:="demo")

        Dim report As EvaluationReport = ModelEvaluation.Evaluate(result)

        If report.Kind <> ResultKinds.classification Then
            Call failures.Add($"classification.kind: expected classification but got {report.Kind}")
        End If

        Call AssertNear(failures, "classification.auc", report.Metric("auc"), 1.0)
        Call AssertNear(failures, "classification.acc", report.Metric("acc"), 1.0)
        Call AssertNear(failures, "classification.auc_curve", report.Metric("auc_curve"), report.Curve.AUC)

        For Each name As String In {"acc", "error", "precision", "recall", "specificity", "f1", "mcc", "auc", "auc_curve", "mse", "mae", "logloss"}
            If Not report.HasMetric(name) Then
                Call failures.Add($"classification.metrics: missing metric '{name}'")
            End If
        Next

        If report.Curve Is Nothing Then
            Call failures.Add("classification.curve: the ROC curve is missing")
        End If
    End Sub

    Private Sub CheckRegressionReport(failures As List(Of String))
        Dim actuals As Double() = {1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0}
        Dim predicts As Double() = {1.1, 1.9, 3.2, 3.8, 5.1, 5.9, 7.2, 7.8}
        Dim result As RegressionResult = RegressionResult.Create(predicts, actuals, "reg")

        Dim report As EvaluationReport = ModelEvaluation.Evaluate(result)

        If report.Kind <> ResultKinds.regression Then
            Call failures.Add($"regression.kind: expected regression but got {report.Kind}")
        End If

        For Each name As String In {"mse", "rmse", "mae", "r2", "auc"}
            If Not report.HasMetric(name) Then
                Call failures.Add($"regression.metrics: missing metric '{name}'")
            End If
        Next

        If report.Metric("r2") < 0.99 Then
            Call failures.Add($"regression.r2: expected a near-perfect fit, got {report.Metric("r2")}")
        End If

        If report.Curve Is Nothing OrElse report.Curve.Points.Length = 0 Then
            Call failures.Add("regression.curve: the regression ROC curve is empty")
        End If

        ' 与旧的 RegressionROC 入口保持一致
        Call AssertNear(failures, "regression.curve-compat",
                        RocCurve.Create(RegressionROC.ROC(predicts, actuals, n:=25)).AUC,
                        report.Curve.AUC)
    End Sub

    Private Sub CheckClusteringReport(failures As List(Of String))
        ' 两个分离良好的二维簇，每个簇 3 个样本
        Dim features As Double()() = {
            New Double() {0.0, 0.0},
            New Double() {0.1, 0.2},
            New Double() {0.2, 0.1},
            New Double() {5.0, 5.0},
            New Double() {5.1, 4.9},
            New Double() {4.9, 5.2}
        }
        Dim labels As Integer() = {0, 0, 0, 1, 1, 1}
        Dim truth As Integer() = {0, 0, 0, 1, 1, 1}

        Dim result As ClusteringResult = ClusteringResult.Create(features, labels, "clusters", truth)
        Dim report As EvaluationReport = ModelEvaluation.Evaluate(result)

        If report.Kind <> ResultKinds.clustering Then
            Call failures.Add($"clustering.kind: expected clustering but got {report.Kind}")
        End If

        For Each name As String In {"silhouette", "dunn", "davies_bouldin", "calinski_harabasz", "maximum_diameter", "purity", "ari", "nmi"}
            If Not report.HasMetric(name) Then
                Call failures.Add($"clustering.metrics: missing metric '{name}'")
            End If
        Next

        If report.Metric("silhouette") <= 0.5 Then
            Call failures.Add($"clustering.silhouette: expected a high score for well separated clusters, got {report.Metric("silhouette")}")
        End If

        Call AssertNear(failures, "clustering.purity", report.Metric("purity"), 1.0)
        Call AssertNear(failures, "clustering.ari", report.Metric("ari"), 1.0)
        Call AssertNear(failures, "clustering.nmi", report.Metric("nmi"), 1.0)

        ' 没有真值标签的时候，外部指标应当是 NaN
        Dim unsupervised As EvaluationReport = ModelEvaluation.Evaluate(ClusteringResult.Create(features, labels, "unsupervised"))

        If Not Double.IsNaN(unsupervised.Metric("purity")) Then
            Call failures.Add("clustering.external: purity should be NaN without a ground truth")
        End If

        ' 聚类结果不产生 ROC 曲线
        If unsupervised.Curve IsNot Nothing Then
            Call failures.Add("clustering.curve: clustering results should not produce a ROC curve")
        End If
    End Sub

#End Region

    Private Sub AssertNear(failures As List(Of String), name As String, actual As Double, expected As Double)
        If Double.IsNaN(actual) AndAlso Double.IsNaN(expected) Then
            Return
        End If

        If Math.Abs(actual - expected) > EPS Then
            Call failures.Add($"{name}: expected {expected}, but got {actual}")
        End If
    End Sub

End Module
