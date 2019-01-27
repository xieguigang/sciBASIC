Imports System.Drawing
Imports Microsoft.VisualBasic.Data.ChartPlots.Statistics
Imports Microsoft.VisualBasic.Imaging

Module vennTest

    Sub Main()
        Dim a As New VennSet With {.color = Brushes.AliceBlue, .intersections = New Dictionary(Of String, Integer) From {{"b", 100}}, .Name = "a", .Size = 500}
        Dim b As New VennSet With {.color = Brushes.DarkGreen, .intersections = New Dictionary(Of String, Integer) From {{"a", 100}}, .Name = "b", .Size = 110}

        Call VennPlot.Venn2(a, b).AsGDIImage.SaveAs("./venn2.png")
    End Sub
End Module
