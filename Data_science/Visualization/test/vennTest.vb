#Region "Microsoft.VisualBasic::274e1f1d04f72de31859ade394bd3753, Data_science\Visualization\test\vennTest.vb"

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

    ' Module vennTest
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Data.ChartPlots
Imports Microsoft.VisualBasic.Imaging

Module vennTest

    Sub Main()
        Dim a As New VennSet With {.color = Color.AliceBlue, .intersections = New Dictionary(Of String, Integer) From {{"b", 100}}, .Name = "a", .Size = 500}
        Dim b As New VennSet With {.color = Color.DarkGreen, .intersections = New Dictionary(Of String, Integer) From {{"a", 100}}, .Name = "b", .Size = 110}
        Dim c As New VennSet With {.color = Color.DeepSkyBlue, .intersections = New Dictionary(Of String, Integer) From {{"a", 300}, {"b", 30}}, .Name = "c", .Size = 500}

        Call VennPlot.Venn2(a, b).AsGDIImage.SaveAs("./venn2.png")
        Call VennPlot.Venn3(a, b, c).AsGDIImage.SaveAs("./venn3.png")
    End Sub
End Module
