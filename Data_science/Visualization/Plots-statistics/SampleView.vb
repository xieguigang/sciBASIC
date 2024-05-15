#Region "Microsoft.VisualBasic::118656dd9f4314b6a622b9715be1037c, Data_science\Visualization\Plots-statistics\SampleView.vb"

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

    '   Total Lines: 159
    '    Code Lines: 134
    ' Comment Lines: 5
    '   Blank Lines: 20
    '     File Size: 6.73 KB


    ' Module SampleView
    ' 
    '     Function: NormalDistributionPlot, SDY
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.base
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Axis
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.Distributions
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.Statistics.MomentFunctions
Imports Microsoft.VisualBasic.MIME.HTML.CSS
Imports Microsoft.VisualBasic.Scripting.Runtime

''' <summary>
''' 通过正态分布曲线和散点图来可视化用户的样品数据
''' </summary>
Public Module SampleView

    Const defaultMeanLineStyle$ = "stroke: green; stroke-width: 2px; stroke-dash: dash;"
    Const defaultNormalDistLineStyle$ = "stroke: " & NameOf(Color.Purple) & "; stroke-width: 2px; stroke-dash: dash;"
    Const outlierLineStyle$ = "stroke: red; stroke-width: 2px; stroke-dash: solid;"
    Const normalErrorLineStyle$ = "stroke: green; stroke-width: 2px; stroke-dash: solid;"

    <Extension>
    Public Function SDY(sample As BasicProductMoments) As Double()
        Dim sd As New List(Of Double)
        Dim calc As New BasicProductMoments

        For Each x As Double In sample
            Call calc.AddObservation(observation:=x)
            Call sd.Add(calc.StDev)
        Next

        Return sd
    End Function

    <Extension>
    Public Function NormalDistributionPlot(sample As IEnumerable(Of Double),
                                           Optional size$ = "2000,1800",
                                           Optional bg$ = "white",
                                           Optional margin$ = g.DefaultPadding,
                                           Optional dotRadius! = 2.5,
                                           Optional dotColorStyle$ = NameOf(Color.Blue),
                                           Optional normaldistLineColor$ = defaultNormalDistLineStyle,
                                           Optional outlierColor$ = outlierLineStyle,
                                           Optional normalErrorColor$ = normalErrorLineStyle,
                                           Optional meanLineCSS$ = defaultMeanLineStyle,
                                           Optional xlabel$ = "X") As GraphicsData

        Dim data As New BasicProductMoments(sample)
        Dim means = data.Mean
        Dim meanLine As Pen = Stroke.TryParse(meanLineCSS).GDIObject
        Dim normalErrorLine As Pen = Stroke.TryParse(normalErrorColor).GDIObject
        Dim outlierLine As Pen = Stroke.TryParse(outlierColor).GDIObject
        Dim normaldistLine As Pen = Stroke.TryParse(normaldistLineColor).GDIObject
        Dim dotBrush As Brush = dotColorStyle.GetBrush
        Dim dotSize As New Size(dotRadius * 2, dotRadius * 2)
        Dim d1# = data.StDev
        Dim d2# = 2 * d1
        Dim d3# = 3 * d1
        Dim d4# = 4 * d1
        Dim d5# = 5 * d1
        Dim d6# = 6 * d1
        Dim xrange As Vector = {data.Mean - d6, data.Mean + d6} _
            .Range _
            .Enumerate(n:=200) _
            .AsVector
        Dim points As PointF() = xrange _
            .AsVector _
            .ProbabilityDensity(data.Mean, d1) _
            .Select(Function(y, i)
                        Return New PointF With {
                            .X = xrange(i),
                            .Y = y
                        }
                    End Function) _
            .ToArray
        Dim XTicks = xrange.Range().CreateAxisTicks
        Dim YTicks = points.Y.Range.CreateAxisTicks
        Dim ptX#() = data.ToArray
        Dim ptY#() = data.SDY

        Dim plotInternal =
            Sub(ByRef g As IGraphics, region As GraphicsRegion)
                Dim X, Y As d3js.scale.LinearScale
                Dim rect = region.PlotRegion
                Dim up As New Rectangle(rect.Location, New Size(rect.Width, rect.Height / 2))

                X = d3js.scale.linear.domain(values:=XTicks).range(integers:={up.Left, up.Right})
                Y = d3js.scale.linear.domain(values:=YTicks).range(integers:={up.Top, up.Bottom})

                Dim scaler As New DataScaler With {
                    .X = X,
                    .Y = Y,
                    .region = up,
                    .AxisTicks = (XTicks, YTicks)
                }

                ' 绘制出坐标轴
                Call g.DrawAxis(
                    region, scaler, True,
                    xlabel:=xlabel, ylabel:="Offset",
                    htmlLabel:=False
                )

                For Each pair As SlideWindow(Of PointF) In points.SlideWindows(2, offset:=1)
                    Dim p1 As PointF = pair(0), p2 As PointF = pair(1)
                    p1 = scaler.Translate(p1.X, p1.Y)
                    p2 = scaler.Translate(p2.X, p2.Y)

                    Call g.DrawLine(normaldistLine, p1, p2)
                Next

                ' 绘制下半部分的散点图
                Dim down As New Rectangle With {
                    .X = up.Left,
                    .Y = up.Bottom,
                    .Width = up.Width,
                    .Height = up.Height
                }

                XTicks = ptX.Range.CreateAxisTicks
                YTicks = ptY.Range.CreateAxisTicks
                X = d3js.scale.linear.domain(values:=XTicks).range(integers:={down.Left, down.Right})
                Y = d3js.scale.linear.domain(values:=YTicks).range(integers:={down.Top, down.Bottom})

                scaler = New DataScaler(rev:=True) With {
                    .X = X,
                    .Y = Y,
                    .region = down,
                    .AxisTicks = (XTicks, YTicks)
                }

                Call g.DrawAxis(
                    region, scaler, True,
                    xlabel:=xlabel, ylabel:="Offset",
                    htmlLabel:=False,
                    xlayout:=XAxisLayoutStyles.Top
                )

                For i As Integer = 0 To data.SampleSize - 1
                    Dim point As New PointF(ptX(i), ptY(i))
                    point = scaler.Translate(point)

                    Call g.DrawCircle(centra:=point, r:=dotRadius, color:=dotBrush)
                Next
            End Sub

        Return g.GraphicsPlots(
            size.SizeParser,
            margin,
            bg,
            plotAPI:=plotInternal
        )
    End Function
End Module
