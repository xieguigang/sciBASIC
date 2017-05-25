Imports System.Drawing
Imports Microsoft.VisualBasic.Data.ChartPlots
Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Mathematical.Correlations
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS

Module Program

    Sub Main()
        '  Call rotateImageTest()
        Call heatmap1()
        Pause()
        Call heatmap2()
    End Sub

    Public Sub rotateImageTest()
        Dim img As Image = "G:\GCModeller\src\runtime\sciBASIC#\Data_science\Mathematical\images\heatmap\Sample.heatmap.png".LoadImage
        img = img.RotateImage(-45)
        Call img.SaveAs("G:\GCModeller\src\runtime\sciBASIC#\Data_science\Mathematical\images\heatmap\r45_degree.png")
    End Sub

    Public Sub heatmap2()
        Dim data = DataSet.LoadDataSet("../../../../\Quick_correlation_matrix_heatmap\mtcars.csv")
        Dim spcc = data.Vectors.CorrelationMatrix(AddressOf Spearman)

        spcc = spcc.KmeansReorder

        Call HeatmapTable.Plot(spcc, legendTitle:="", mainTitle:="", drawGrid:=True) _
            .Save("X:\Sample.SPCC.png")
    End Sub

    Public Sub heatmap1()
        Dim datahm = Heatmap.LoadDataSet("../../../../../Quick_correlation_matrix_heatmap\mtcars.csv", normalization:=True)
        Call Heatmap.Plot(datahm, mapName:=ColorMap.PatternJet,
                          kmeans:=AddressOf KmeansReorder,
                          mapLevels:=20,
                          size:=New Size(2000, 2000),
                          padding:="padding: 300",
                          legendTitle:="Spearman correlations",
                          fontStyle:=CSSFont.GetFontStyle(FontFace.BookmanOldStyle, FontStyle.Bold, 24)).Save("X:\spcc.png")
    End Sub
End Module
