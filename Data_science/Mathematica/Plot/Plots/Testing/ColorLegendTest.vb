Imports System.Drawing
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Axis
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors

Module ColorLegendTest

    Sub Main()

        Dim colors As SolidBrush() = Designer.GetColors(ColorBrewer.DivergingSchemes.RdBu9, 100).GetBrushes
        Dim ticks = AxisScalling.CreateAxisTicks({-10.3301, 13.7566}, 20)
        Dim range = ticks.Range

    End Sub
End Module
