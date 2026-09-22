#Region "Microsoft.VisualBasic::ShapAnalyzer, MachineLearning\ML_SHAP\ShapAnalyzer.vb"

Imports System.Linq
Imports Microsoft.VisualBasic.Math.Statistics.ShapleyValue

''' <summary>
''' 一个模型在一个数据集之上的整体 SHAP 分析结果。
''' </summary>
Public Class ShapAnalysisResult

    Public Property ModelName As String
    Public Property DatasetName As String
    Public Property IsClassification As Boolean

    ''' <summary>
    ''' 特征名字。
    ''' </summary>
    ''' <returns></returns>
    Public Property FeatureNames As String()

    ''' <summary>
    ''' 基线值 / 期望值。
    ''' </summary>
    ''' <returns></returns>
    Public Property Baseline As Double

    ''' <summary>
    ''' 参与 SHAP 分析的样本的解释结果。
    ''' </summary>
    ''' <returns></returns>
    Public Property Explanations As List(Of ShapExplanation)

    ''' <summary>
    ''' 每一个特征的全局重要性：平均绝对 SHAP 值。
    ''' </summary>
    ''' <returns></returns>
    Public Property MeanAbsShap As Double()

    ''' <summary>
    ''' 每一个特征的平均有符号 SHAP 值（可用于判断影响的整体方向）。
    ''' </summary>
    ''' <returns></returns>
    Public Property MeanShap As Double()

    ''' <summary>
    ''' 返回按照全局重要性降序排列的 ``(特征名, 平均绝对 SHAP 值)`` 序列。
    ''' </summary>
    ''' <returns></returns>
    Public Function GlobalImportance() As (name As String, importance As Double)()
        If MeanAbsShap Is Nothing Then
            Return New (name As String, importance As Double)() {}
        End If

        Dim items As New List(Of (name As String, importance As Double))()

        For i As Integer = 0 To MeanAbsShap.Length - 1
            Dim featureName As String = If(FeatureNames Is Nothing OrElse i >= FeatureNames.Length, $"F{i + 1}", FeatureNames(i))
            items.Add((featureName, MeanAbsShap(i)))
        Next

        Return items.OrderByDescending(Function(t) t.importance).ToArray
    End Function

End Class

''' <summary>
''' SHAP 分析的门面：把任意 <see cref="IShapExplainer"/> 与数据集组合起来，
''' 计算逐样本的 SHAP 矩阵以及全局特征重要性。
''' </summary>
Public Module ShapAnalyzer

    ''' <summary>
    ''' 对数据集之中的样本执行 SHAP 分析。
    ''' </summary>
    ''' <param name="explainer">已经针对某个模型构建好的 SHAP 解释器</param>
    ''' <param name="dataset">用于计算 SHAP 的样本数据</param>
    ''' <param name="modelName">模型名字（用于报告）</param>
    ''' <param name="maxSamples">
    ''' 最多解释的样本数量，小于等于 0 表示解释全部样本
    ''' </param>
    ''' <param name="output">
    ''' 可选的模型输出函数，用于记录模型的原始输出
    ''' （对于逻辑回归是类别概率）。为空时使用加性值。
    ''' </param>
    ''' <returns></returns>
    Public Function Analyze(explainer As IShapExplainer,
                            dataset As ShapDataset,
                            modelName As String,
                            Optional maxSamples As Integer = 0,
                            Optional output As Func(Of Double(), Double) = Nothing) As ShapAnalysisResult

        Dim count As Integer = dataset.Size

        If maxSamples > 0 AndAlso maxSamples < count Then
            count = maxSamples
        End If

        Dim width As Integer = dataset.Width
        Dim explanations As New List(Of ShapExplanation)()
        Dim absoluteSum As Double() = New Double(width - 1) {}
        Dim signedSum As Double() = New Double(width - 1) {}

        For i As Integer = 0 To count - 1
            Dim x As Double() = dataset.Features(i)
            Dim contributions As Double() = explainer.Explain(x)
            Dim additive As Double = explainer.Baseline

            For j As Integer = 0 To width - 1
                Dim value As Double = If(j < contributions.Length, contributions(j), 0)

                absoluteSum(j) += System.Math.Abs(value)
                signedSum(j) += value
                additive += value
            Next

            Dim rawOutput As Double = additive

            If output IsNot Nothing Then
                rawOutput = output(x)
            End If

            explanations.Add(New ShapExplanation With {
                .ID = dataset.IDs(i),
                .FeatureNames = dataset.FeatureNames,
                .Contributions = contributions,
                .Baseline = explainer.Baseline,
                .AdditiveValue = additive,
                .Output = rawOutput
            })
        Next

        Dim normalizer As Double = System.Math.Max(1, explanations.Count)

        Return New ShapAnalysisResult With {
            .ModelName = modelName,
            .DatasetName = dataset.Name,
            .IsClassification = dataset.IsCategorical,
            .FeatureNames = dataset.FeatureNames,
            .Baseline = explainer.Baseline,
            .Explanations = explanations,
            .MeanAbsShap = absoluteSum.Select(Function(v) v / normalizer).ToArray,
            .MeanShap = signedSum.Select(Function(v) v / normalizer).ToArray
        }
    End Function

End Module

#End Region
