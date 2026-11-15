Namespace ShapleyValue

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
                Return New(name As String, importance As Double)() {}
            End If

            Dim items As New List(Of (name As String, importance As Double))()

            For i As Integer = 0 To MeanAbsShap.Length - 1
                Dim featureName As String = If(FeatureNames Is Nothing OrElse i >= FeatureNames.Length, $"F{i + 1}", FeatureNames(i))
                items.Add((featureName, MeanAbsShap(i)))
            Next

            Return items.OrderByDescending(Function(t) t.importance).ToArray
        End Function

    End Class
End Namespace