#Region "Microsoft.VisualBasic::87a4d0e58d678ca46a41c0205cf00704, Data_science\Visualization\test\ChartingBase.Test\Module2.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    ' Module Module2
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Sub: HeatmapScatter, Main
    ' 
    ' /********************************************************************************/

#End Region

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
            .Save("x:\scatter.plotHeatmap.png")
    End Sub
End Module
