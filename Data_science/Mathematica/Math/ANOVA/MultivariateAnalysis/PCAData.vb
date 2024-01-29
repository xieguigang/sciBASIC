Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Math.DataFrame
Imports df = Microsoft.VisualBasic.Math.DataFrame.DataFrame

''' <summary>
''' get pca result: contribution, score, loading
''' </summary>
Public Module PCAData

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="mvar"></param>
    ''' <returns>a dataframe object that contains data field column: ``PC1, PC2, PCx``</returns>
    <Extension>
    Public Function GetPCAScore(mvar As MultivariateAnalysisResult) As df
        Dim score As New df
        Dim filesize = mvar.StatisticsObject.YIndexes.Count
        Dim compSize = mvar.Contributions.Count
        Dim labels = mvar.StatisticsObject.YLabels

        If labels Is Nothing OrElse labels.Count = 0 Then
            labels = New ObjectModel.ObservableCollection(Of String)

            For Each i As Integer In mvar.StatisticsObject.YIndexes
                labels.Add(i + 1)
            Next
        End If

        For i = 0 To filesize - 1
            Dim tList = New List(Of Double)()
            For j = 0 To compSize - 1
                tList.Add(mvar.TPreds(j)(i))
            Next
            score.add(labels(i), tList.ToArray)
        Next

        score.rownames = mvar.Contributions _
            .Select(Function(c, i) $"PC{i + 1}") _
            .ToArray

        Return score.Transpose
    End Function

    <Extension>
    Public Function GetPCALoading(mvar As MultivariateAnalysisResult) As df
        Dim loading As New df
        Dim metsize = mvar.StatisticsObject.XLabels.Count
        Dim compSize = mvar.Contributions.Count
        Dim labels = mvar.StatisticsObject.XLabels

        For i As Integer = 0 To metsize - 1
            Dim pList = New List(Of Double)()
            For j = 0 To compSize - 1
                pList.Add(mvar.PPreds(j)(i))
            Next
            loading.add(labels(i), pList.ToArray)
        Next

        loading.rownames = mvar.Contributions _
            .Select(Function(c, i) $"PC{i + 1}") _
            .ToArray

        Return loading.Transpose
    End Function
End Module
