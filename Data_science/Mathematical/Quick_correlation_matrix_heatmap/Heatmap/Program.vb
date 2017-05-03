Imports Microsoft.VisualBasic.Data.ChartPlots
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.Mathematical.Correlations

Module Program

    Sub Main()
        Call heatmap2()
    End Sub

    Public Sub heatmap2()
        Dim data = DataSet.LoadDataSet("G:\GCModeller\src\runtime\sciBASIC#\Data_science\Mathematical\images\heatmap\Sample.heatmap.png")
        Dim spcc = data.Vectors.CorrelationMatrix(AddressOf Spearman)

        Call HeatmapTable.Plot(spcc,) _
            .Save("G:\GCModeller\src\runtime\sciBASIC#\Data_science\Mathematical\images\heatmap\Sample.SPCC.png")
    End Sub
End Module
