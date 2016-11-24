Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors

Module ColorTest

    Sub Main()
        Dim colors = Designer.CubicSpline({Color.SkyBlue, Color.Red, Color.Lime})
        Call Legends.ColorMapLegend(colors, "test", -100, 5000,,).SaveAs("./test.png")
    End Sub
End Module
