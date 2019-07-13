#Region "Microsoft.VisualBasic::c575a0b735567067462271099507a5ea, Data_science\Mathematica\Quick_correlation_matrix_heatmap\Heatmap\Program.vb"

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

    ' Module Program
    ' 
    '     Sub: heatmap2, Main, rotateImageTest
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Data.ChartPlots.Statistics.Heatmap
Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Math.Correlations

Module Program

    Sub Main()
        '  Call rotateImageTest()
        ' Call heatmap1()
        ' Pause()
        Call heatmap2()
    End Sub

    Public Sub rotateImageTest()
        Dim img As Image = "G:\GCModeller\src\runtime\sciBASIC#\Data_science\Mathematical\images\heatmap\Sample.heatmap.png".LoadImage
        img = img.RotateImage(-45)
        Call img.SaveAs("G:\GCModeller\src\runtime\sciBASIC#\Data_science\Mathematical\images\heatmap\r45_degree.png")
    End Sub

    Public Sub heatmap2()
        Dim data = DataSet.LoadDataSet("../../../../\Quick_correlation_matrix_heatmap\mtcars.csv")
        Dim spcc = data.Vectors.CorrelationMatrix(AddressOf Spearman).AsDataSet

        Call CorrelationHeatmap.Plot(spcc, legendTitle:="", mainTitle:="", drawGrid:=True, range:="-1,1") _
            .SaveAs("D:\Sample.SPCC.png")
    End Sub

    'Public Sub heatmap1()
    '    Dim datahm = Heatmap.LoadDataSet("../../../../../Quick_correlation_matrix_heatmap\mtcars.csv", normalization:=True)
    '    Call Heatmap.Plot(datahm, mapName:=ColorMap.PatternJet,
    '                      kmeans:=AddressOf KmeansReorder,
    '                      mapLevels:=20,
    '                      size:=New Size(2000, 2000),
    '                      padding:="padding: 300",
    '                      legendTitle:="Spearman correlations",
    '                      fontStyle:=CSSFont.GetFontStyle(FontFace.BookmanOldStyle, FontStyle.Bold, 24)).Save("X:\spcc.png")
    'End Sub
End Module
