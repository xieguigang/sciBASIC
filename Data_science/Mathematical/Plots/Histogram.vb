Imports System.Drawing
Imports Microsoft.VisualBasic.Serialization.JSON

Public Module Histogram

    Public Structure HistogramData
        Public x#, y#

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Structure

    Public Function Plot(data As IEnumerable(Of HistogramData),
                         Optional bg$ = "white",
                         Optional size As Size = Nothing,
                         Optional margin As Size = Nothing) As Bitmap

    End Function
End Module
