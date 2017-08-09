#Region "Microsoft.VisualBasic::90322157d701e46fed00efb2bad271d4, ..\sciBASIC#\Data_science\Mathematica\data\Spline_Interpolation\Demo\ColorTest.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Linq

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

        'Accent
        colors = Designer _
            .CubicSpline(Designer.ColorBrewer("Accent").c8.ToArray(AddressOf ToColor), 30)
        Call Legends.ColorMapLegend(
            colors, "ColorDesigner test", "minValue", "maxValue", bg:="white") _
            .SaveAs("./colordesigner.ColorBrewer.Accent.c8.png")

        colors = Designer _
            .CubicSpline(Designer.Rainbow, 30)
        Call Legends.ColorMapLegend(
            colors, "ColorDesigner Rainbow", "minValue", "maxValue", bg:="white") _
            .SaveAs("./colordesigner.Rainbow.png")

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

        Call ColorBrewer()
    End Sub

    Sub ColorBrewer()

        For Each key$ In Designer.ColorBrewer.Keys
            On Error Resume Next

            Dim s = Designer.ColorBrewer(key)
            Dim colors = Designer _
                .CubicSpline(
                If(s.c10.IsNullOrEmpty,
                If(s.c9.IsNullOrEmpty,
                If(s.c8.IsNullOrEmpty,
                If(s.c7.IsNullOrEmpty,
                If(s.c6.IsNullOrEmpty, s.c5, s.c6), s.c7), s.c8), s.c9), s.c10) _
                .ToArray(AddressOf ToColor), 30)
            Call Legends.ColorMapLegend(
                colors, $"ColorDesigner ColorBrewer:{key}", "minValue", "maxValue", bg:="white") _
                .SaveAs($"./colordesigner.ColorBrewer.{key}.png")
        Next
    End Sub
End Module
