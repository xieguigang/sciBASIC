Imports System.Drawing
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Axis
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors

Module ColorLegendTest

    Sub Main()

        Dim colors As SolidBrush() = Designer.GetColors(ColorBrewer.DivergingSchemes.RdBu9, 200).GetBrushes
        Dim ticks = AxisScalling.CreateAxisTicks({-10.3301, 13.7566}, 20)
        Dim range = ticks.Range

        Using g As Graphics2D = New Size(1200, 200).CreateGDIDevice

            Call colors.ColorLegendHorizontal(ticks, g, New Rectangle(New Point, g.Size))
            Call g.Save("./test.legend.png", ImageFormats.Png)

        End Using
    End Sub
End Module
