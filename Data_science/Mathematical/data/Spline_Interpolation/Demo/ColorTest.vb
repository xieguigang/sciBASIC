Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors

Module ColorTest

    Sub Main()
        Dim colors As Color() = {
            Color.DarkRed,
            Color.SkyBlue,
            Color.Orange,
            Color.Red,
            Color.Lime,
            Color.DeepSkyBlue,
            Color.DarkViolet
        }

        colors = Designer _
            .CubicSpline(colors, 30)
        Call Legends.ColorMapLegend(
            colors, "ColorDesigner test", "minValue", "maxValue", bg:="white") _
            .SaveAs("./colordesigner.test.png")

        colors = OfficeColorThemes _
            .GetAccentColors("Slipstream") _
            .CubicSpline(30)
        Call Legends.ColorMapLegend(
            colors, "office_Slipstream_theme", "minValue", "maxValue", bg:="white") _
            .SaveAs("./officeSlipstream_theme.test.png")

        colors = OfficeColorThemes _
            .GetAccentColors("office2016") _
            .CubicSpline(30)
        Call Legends.ColorMapLegend(
            colors, "office_office2016_theme", "minValue", "maxValue", bg:="white") _
            .SaveAs("./office2016_theme.test.png")

        colors = OfficeColorThemes _
           .GetAccentColors("office2010") _
           .CubicSpline(30)
        Call Legends.ColorMapLegend(
            colors, "office_office2010_theme", "minValue", "maxValue", bg:="white") _
            .SaveAs("./office2010_theme.test.png")
    End Sub
End Module
