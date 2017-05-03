Imports System.Drawing
Imports Microsoft.VisualBasic.Data.ChartPlots
Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Mathematical.Correlations

Module Program

    Sub Main()
        Call rotateImageTest()
        Call heatmap2()
    End Sub

    Public Sub rotateImageTest()
        Dim img As Image = "G:\GCModeller\src\runtime\sciBASIC#\Data_science\Mathematical\images\heatmap\Sample.heatmap.png".LoadImage
        img = img.RotateImage(-45)
        Call img.SaveAs("G:\GCModeller\src\runtime\sciBASIC#\Data_science\Mathematical\images\heatmap\r45_degree.png")
    End Sub

    Public Sub heatmap2()
        Dim data = DataSet.LoadDataSet("G:\GCModeller\src\runtime\sciBASIC#\Data_science\Mathematical\Quick_correlation_matrix_heatmap\mtcars.csv")
        Dim spcc = data.Vectors.CorrelationMatrix(AddressOf Spearman)

        Call HeatmapTable.Plot(spcc, legendTitle:="", mainTitle:="", drawGrid:=True) _
            .Save("G:\GCModeller\src\runtime\sciBASIC#\Data_science\Mathematical\images\heatmap\Sample.SPCC.png")
    End Sub
End Module
