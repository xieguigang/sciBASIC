#Region "Microsoft.VisualBasic::ff1498b7422f97a77c059bec38f6e87e, sciBASIC#\Data_science\Mathematica\data\Spline_Interpolation\Demo\SplineTest.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
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



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 221
    '    Code Lines: 184
    ' Comment Lines: 11
    '   Blank Lines: 26
    '     File Size: 8.54 KB


    ' Module SplineTest
    ' 
    '     Sub: BezierCurveTest, CentripetalCatmullRomSplineTest, InterpolationTest, Main
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Mathematical.Interpolation
Imports Microsoft.VisualBasic.Mathematical.Plots
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS

Public Module SplineTest

    Sub Main()
        Call InterpolationTest()
        Call BezierCurveTest()
        Call CentripetalCatmullRomSplineTest()

        Pause()
    End Sub

    ''' <summary>
    ''' Spline Interpolation using 4 control points
    ''' </summary>
    Sub CentripetalCatmullRomSplineTest()
        Dim P As PointF() = {
            New PointF(1, 1),
            New PointF(10, 15),
            New PointF(90, 60),
            New PointF(150, 40)
        }
        Dim result As List(Of PointF) =
            CentripetalCatmullRomSpline.CatmulRom(P(0), P(1), P(2), P(3))

        Dim CRInterplotCenter = result.FromPoints(
            lineColor:="lime",
            ptSize:=3,
            lineType:=DashStyle.Solid,
            lineWidth:=3,
            title:="Centripetal Catmull–Rom spline")

        Dim raw As SerialData = P.FromPoints(
            lineColor:="red",
            ptSize:=30,
            lineType:=DashStyle.Dot,
            lineWidth:=5,
            title:="P1-P4 Control Points")

        raw.annotations = {
            New Annotation With {
                .Text = "P0",
                .X = P(0).X,
                .color = "skyblue",
                .Font = CSSFont.GetFontStyle(FontFace.MicrosoftYaHei, FontStyle.Regular, 24),
                .Legend = LegendStyles.Pentacle
            },
            New Annotation With {
                .Text = "P1",
                .X = P(1).X,
                .color = "skyblue",
                .Font = CSSFont.GetFontStyle(FontFace.MicrosoftYaHei, FontStyle.Regular, 24),
                .Legend = LegendStyles.Pentacle
            },
            New Annotation With {
                .Text = "P2",
                .X = P(2).X,
                .color = "skyblue",
                .Font = CSSFont.GetFontStyle(FontFace.MicrosoftYaHei, FontStyle.Regular, 24),
                .Legend = LegendStyles.Pentacle
            },
            New Annotation With {
                .Text = "P3",
                .X = P(3).X,
                .color = "skyblue",
                .Font = CSSFont.GetFontStyle(FontFace.MicrosoftYaHei, FontStyle.Regular, 24),
                .Legend = LegendStyles.Pentacle
            }
        }

        Call Scatter.Plot({raw, CRInterplotCenter}, size:=New Size(3000, 1400)) _
            .SaveAs("../../../DEMO-Centripetal-CatmullRom-Spline.png")
    End Sub

    ''' <summary>
    ''' BezierCurve using 3 Control Points
    ''' </summary>
    Sub BezierCurveTest()
        Dim P As PointF() = {
            New PointF(40, 45),
            New PointF(70, 580),
            New PointF(100, 40)
        }
        Dim result As List(Of PointF) =
            New BezierCurve(P(0), P(1), P(2), 10).BezierPoints

        Dim BezierInterplot = result.FromPoints(
            lineColor:="lime",
            ptSize:=3,
            lineType:=DashStyle.Solid,
            lineWidth:=3,
            title:="BezierCurve spline")

        Dim raw As SerialData = P.FromPoints(
            lineColor:="red",
            ptSize:=30,
            lineType:=DashStyle.Dot,
            lineWidth:=5,
            title:="BezierCurve Control Points")

        raw.annotations = {
            New Annotation With {
                .Text = "P0",
                .X = P(0).X,
                .color = "skyblue",
                .Font = CSSFont.GetFontStyle(FontFace.MicrosoftYaHei, FontStyle.Regular, 24),
                .Legend = LegendStyles.Pentacle
            },
            New Annotation With {
                .Text = "P1",
                .X = P(1).X,
                .color = "skyblue",
                .Font = CSSFont.GetFontStyle(FontFace.MicrosoftYaHei, FontStyle.Regular, 24),
                .Legend = LegendStyles.Pentacle
            },
            New Annotation With {
                .Text = "P2",
                .X = P(2).X,
                .color = "skyblue",
                .Font = CSSFont.GetFontStyle(FontFace.MicrosoftYaHei, FontStyle.Regular, 24),
                .Legend = LegendStyles.Pentacle
            }
        }

        Call Scatter.Plot({raw, BezierInterplot}, size:=New Size(3000, 1400)) _
            .SaveAs("../../../DEMO-BezierCurve-Spline.png")
    End Sub

    Sub InterpolationTest()
        Dim data#()() = "../../../duom2.txt" _
            .IterateAllLines _
            .ToArray(Function(s) Regex.Replace(s, "\s+", " ") _
                .Trim _
                .Split _
                .ToArray(AddressOf Val))
        Dim points As PointF() = data _
            .ToArray(Function(c) New PointF With {
                .X = c(Scan0),
                .Y = c(1)
            })
        Dim raw = points.FromPoints(
            lineColor:="skyblue",
            ptSize:=40,
            title:="duom2: raw")

        ' CubicSpline
        Dim result = CubicSpline.RecalcSpline(points).ToArray

        Dim interplot = result.FromPoints(
            lineColor:="red",
            ptSize:=5,
            title:="duom2: CubicSpline.RecalcSpline",
            lineType:=DashStyle.Dash,
            lineWidth:=3)

        Call Scatter.Plot({raw, interplot}, size:=New Size(3000, 1400)) _
            .SaveAs("../../../duom2-cubic-spline.png")

        ' CatmullRomSpline
        result = CatmullRomSpline.CatmullRomSpline(points)

        Dim CRInterplot = result.FromPoints(
            lineColor:="orange",
            ptSize:=3,
            lineType:=DashStyle.Dot,
            lineWidth:=3,
            title:="duom2: Catmull-Rom Spline")

        Call Scatter.Plot({raw, CRInterplot}, size:=New Size(3000, 1400)) _
            .SaveAs("../../../duom2-CatmullRomSpline.png")

        ' B-Spline
        result = B_Spline.Compute(points, 1.5, RESOLUTION:=100)

        Dim B_interplot = result.FromPoints(
            lineColor:="green",
            ptSize:=5,
            title:="duom2: B-spline",
            lineType:=DashStyle.Dot,
            lineWidth:=3)

        Call Scatter.Plot({raw, B_interplot}, size:=New Size(3000, 1400)) _
        .SaveAs("../../../duom2-B-spline.png")

        ' Method Compares
        Call Scatter.Plot({raw, CRInterplot, B_interplot, interplot}, size:=New Size(3000, 1400)) _
            .SaveAs("../../../duom2-compares.png")

        ' Compare B-spline parameters
        Dim bsplines = {
            B_Spline.Compute(points, 0.5, RESOLUTION:=100) _
                    .FromPoints(lineColor:="green", ptSize:=5, title:="duom2: B-spline, 0.5 degree", lineWidth:=3),
            B_Spline.Compute(points, 1, RESOLUTION:=100) _
                    .FromPoints(lineColor:="skyblue", ptSize:=3, title:="duom2: B-spline, 1 degree", lineWidth:=2),
            B_Spline.Compute(points, 2, RESOLUTION:=100) _
                    .FromPoints(lineColor:="yellow", ptSize:=3, title:="duom2: B-spline, 2 degree", lineWidth:=2),
            B_Spline.Compute(points, 3, RESOLUTION:=100) _
                    .FromPoints(lineColor:="lime", ptSize:=3, title:="duom2: B-spline, 3 degree", lineWidth:=2),
            B_Spline.Compute(points, 4, RESOLUTION:=100) _
                    .FromPoints(lineColor:="orange", ptSize:=3, title:="duom2: B-spline, 4 degree", lineWidth:=2),
            B_Spline.Compute(points, 5, RESOLUTION:=100) _
                    .FromPoints(lineColor:="darkred", ptSize:=3, title:="duom2: B-spline, 5 degree", lineWidth:=2),
            B_Spline.Compute(points, 10, RESOLUTION:=100) _
                    .FromPoints(lineColor:="red", ptSize:=3, title:="duom2: B-spline, 10 degree", lineWidth:=2),
            B_Spline.Compute(points, 20, RESOLUTION:=100) _
                    .FromPoints(lineColor:="gray", ptSize:=3, title:="duom2: B-spline, 20 degree", lineWidth:=2),
            B_Spline.Compute(points, 30, RESOLUTION:=100) _
                    .FromPoints(lineColor:="lightgreen", ptSize:=3, title:="duom2: B-spline, 30 degree", lineWidth:=2)
        }

        Call Scatter.Plot(raw.Join(bsplines), size:=New Size(3000, 1400)) _
            .SaveAs("../../../duom2-B-splines.png")
    End Sub
End Module
