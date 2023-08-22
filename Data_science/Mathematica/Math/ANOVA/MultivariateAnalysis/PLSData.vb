Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports df = Microsoft.VisualBasic.Math.DataFrame.DataFrame

Public Module PLSData

    <Extension>
    Public Iterator Function GetComponents(mvar As MultivariateAnalysisResult) As IEnumerable(Of Component)
        For i As Integer = 0 To mvar.Presses.Count - 1
            Yield New Component With {
                .Order = (i + 1),
                .SSCV = mvar.SsCVs(i),
                .Press = mvar.Presses(i),
                .Q2 = mvar.Q2Values(i),
                .Q2cum = mvar.Q2Cums(i)
            }
        Next
    End Function

    <Extension>
    Public Function GetPLSScore(mvar As MultivariateAnalysisResult) As df
        Dim [class] As String() = mvar.StatisticsObject.YLabels.ToArray
        Dim Tscore As New List(Of Double())
        Dim fileSize = mvar.StatisticsObject.YIndexes.Count
        Dim yexp As Double() = mvar.StatisticsObject.YVariables
        Dim ypre As Double() = New Double([class].Length - 1) {}

        For i As Integer = 0 To mvar.OptimizedFactor - 1
            Tscore.Add(New Double([class].Length - 1) {})
        Next

        ' scores
        For i As Integer = 0 To fileSize - 1
            For j = 0 To mvar.TPreds.Count - 1
                Tscore(j)(i) = mvar.TPreds(j)(i)
            Next

            ypre(i) = mvar.PredictedYs(i)
        Next

        Dim df As New df With {.rownames = [class]}
        Dim index As i32 = 1

        For Each t As Double() In Tscore
            Call df.add($"T{++index}", t)
        Next

        Call df.add("Y experiment", yexp)
        Call df.add("Y predicted", ypre)

        Return df
    End Function

    <Extension>
    Public Function GetPLSLoading(mvar As MultivariateAnalysisResult) As df
        Dim features As String() = mvar.StatisticsObject.XLabels.ToArray
        Dim Ploads As New List(Of Double())
        Dim metSize = mvar.StatisticsObject.XIndexes.Count
        Dim vips As Double() = mvar.Vips.ToArray
        Dim cors As Double() = mvar.Coefficients.ToArray

        For i = 0 To mvar.OptimizedFactor - 1
            Call Ploads.Add(New Double(features.Length - 1) {})
        Next

        For i As Integer = 0 To metSize - 1
            For j As Integer = 0 To mvar.PPreds.Count - 1
                Ploads(j)(i) = mvar.PPreds(j)(i)
            Next
        Next

        Dim df As New df With {.rownames = features}
        Dim index As i32 = 1

        For Each p As Double() In Ploads
            Call df.add($"P{++index}", p)
        Next

        Call df.add("VIP", vips)
        Call df.add("Coefficients", cors)

        Return df
    End Function

End Module

Public Class Component

    Public Property Order As Integer
    Public Property SSCV As Double
    Public Property Press As Double
    Public Property Q2 As Double
    Public Property Q2cum As Double

End Class
