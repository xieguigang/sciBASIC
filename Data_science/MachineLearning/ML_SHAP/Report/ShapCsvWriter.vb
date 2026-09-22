#Region "Microsoft.VisualBasic::ShapCsvWriter, MachineLearning\ML_SHAP\Report\ShapCsvWriter.vb"

Imports System.Globalization
Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.Math.Statistics.ShapleyValue

''' <summary>
''' 把 SHAP 分析结果导出为 CSV 文件。
''' </summary>
Public Module ShapCsvWriter

    Private ReadOnly invariant As CultureInfo = CultureInfo.InvariantCulture

    ''' <summary>
    ''' 导出完整的 SHAP 矩阵。
    ''' 
    ''' 列结构为：``ID, output, baseline, additive_value, F1, F2, ..., Fn``。
    ''' </summary>
    ''' <param name="result">SHAP 分析结果</param>
    ''' <param name="path">输出文件路径</param>
    Public Sub WriteExplanations(result As ShapAnalysisResult, path As String)
        Dim folder As String = IO.Path.GetDirectoryName(path)

        If Not String.IsNullOrEmpty(folder) AndAlso Not IO.Directory.Exists(folder) Then
            Call IO.Directory.CreateDirectory(folder)
        End If

        Using writer As New StreamWriter(path, False, New UTF8Encoding(False))
            writer.WriteLine(BuildHeader(result))

            If result.Explanations IsNot Nothing Then
                For Each explanation As ShapExplanation In result.Explanations
                    writer.WriteLine(BuildRow(result, explanation))
                Next
            End If
        End Using
    End Sub

    ''' <summary>
    ''' 导出全局特征重要性（平均绝对 SHAP 值）。
    ''' </summary>
    ''' <param name="result">SHAP 分析结果</param>
    ''' <param name="path">输出文件路径</param>
    Public Sub WriteGlobalImportance(result As ShapAnalysisResult, path As String)
        Dim folder As String = IO.Path.GetDirectoryName(path)

        If Not String.IsNullOrEmpty(folder) AndAlso Not IO.Directory.Exists(folder) Then
            Call IO.Directory.CreateDirectory(folder)
        End If

        Using writer As New StreamWriter(path, False, New UTF8Encoding(False))
            writer.WriteLine("feature,mean_abs_shap,mean_shap")

            For Each item In result.GlobalImportance()
                writer.WriteLine($"{Escape(item.name)},{Format(item.importance)},{Format(MeanShapOf(result, item.name))}")
            Next
        End Using
    End Sub

    Private Function BuildHeader(result As ShapAnalysisResult) As String
        Dim columns As New List(Of String) From {"ID", "output", "baseline", "additive_value"}

        If result.FeatureNames IsNot Nothing Then
            columns.AddRange(result.FeatureNames)
        End If

        Return String.Join(",", columns)
    End Function

    Private Function BuildRow(result As ShapAnalysisResult, explanation As ShapExplanation) As String
        Dim columns As New List(Of String) From {
            Escape(explanation.ID),
            Format(explanation.Output),
            Format(explanation.Baseline),
            Format(explanation.AdditiveValue)
        }

        If result.FeatureNames IsNot Nothing Then
            For i As Integer = 0 To result.FeatureNames.Length - 1
                Dim value As Double = If(explanation.Contributions IsNot Nothing AndAlso i < explanation.Contributions.Length,
                                         explanation.Contributions(i),
                                         0)
                columns.Add(Format(value))
            Next
        End If

        Return String.Join(",", columns)
    End Function

    Private Function MeanShapOf(result As ShapAnalysisResult, name As String) As Double
        If result.FeatureNames Is Nothing OrElse result.MeanShap Is Nothing Then
            Return 0
        End If

        For i As Integer = 0 To result.FeatureNames.Length - 1
            If String.Equals(result.FeatureNames(i), name, StringComparison.OrdinalIgnoreCase) Then
                Return result.MeanShap(i)
            End If
        Next

        Return 0
    End Function

    Private Function Format(value As Double) As String
        Return value.ToString("G17", invariant)
    End Function

    Private Function Escape(value As String) As String
        If String.IsNullOrEmpty(value) Then
            Return ""
        End If

        If value.IndexOfAny(New Char() {","c, """"c, ControlChars.Cr, ControlChars.Lf}) >= 0 Then
            Return """" & value.Replace("""", """""") & """"
        Else
            Return value
        End If
    End Function

End Module

#End Region
