Imports Microsoft.VisualBasic.Data.ChartPlots
Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.Mathematical.Correlations

Module Program

    Sub Main()

    End Sub


    Public Sub heatmap2()

        Dim data = LoadData("G:\GCModeller\src\runtime\sciBASIC#\Data_science\Mathematical\images\heatmap\Sample.csv", True)
        Dim spcc = data.CorrelationMatrix(AddressOf Spearman)

        Call HeatmapTable.Plot(spcc,) _
            .Save("G:\GCModeller\src\runtime\sciBASIC#\Data_science\Mathematical\images\heatmap\Sample.SPCC.png")
        Call Microsoft.VisualBasic.Data.ChartPlots.Heatmap.Plot(spcc, mapLevels:=25) _
            .Save("G:\GCModeller\src\runtime\sciBASIC#\Data_science\Mathematical\images\heatmap\Sample.heatmap.png")

    End Sub
End Module
