Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Data.ChartPlots
Imports Microsoft.VisualBasic.Imaging

Module Module2

    Sub New()

    End Sub

    Sub Main()
        Call HeatmapScatter()

    End Sub

    Sub HeatmapScatter()
        Dim data As New List(Of DataSet)
        Dim rnd As New Random

        For i As Integer = 0 To 1000
            data += New DataSet With {
                .ID = App.NextTempName,
                .Properties = New Dictionary(Of String, Double) From {
                    {"X", rnd.NextInteger(1000)},
                    {"Y", rnd.NextInteger(1000)},
                    {"Z", rnd.NextInteger(1000)}
                }
            }
        Next

        Call Scatter _
            .PlotHeatmap(data, valueField:="Z", legendTitle:="Heatmap(Z)") _
            .SaveAs("x:\scatter.plotHeatmap.png")
    End Sub
End Module
