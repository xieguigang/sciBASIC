#Region "Microsoft.VisualBasic::fba62f19e0e28d63907c76c42dacb7c7, sciBASIC#\Data_science\Visualization\Plots-statistics\BoxPlot.vb"

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

    '   Total Lines: 233
    '    Code Lines: 171
    ' Comment Lines: 27
    '   Blank Lines: 35
    '     File Size: 11.02 KB


    ' Module BoxPlot
    ' 
    '     Function: Plot
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.DataStructures
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Data.ChartPlots.BarPlot
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Axis
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Text
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.Quantile
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.Scripting.Runtime

''' <summary>
''' ```
''' min, q1, q2, q3, max
'''       _________
'''  +----|   |   |----+
'''       ---------
''' ```
''' </summary>
Public Module BoxPlot

    ReadOnly Zero As [Default](Of  Double()) = LanguageAPI.[Default]({0R}, Function(x) TryCast(x, Double()).IsNullOrEmpty)

    <Extension> Public Function Plot(data As BoxData,
                                     Optional size$ = "3000,2700",
                                     Optional padding$ = g.DefaultPadding,
                                     Optional bg$ = "white",
                                     Optional schema$ = ColorBrewer.QualitativeSchemes.Set1_9,
                                     Optional YaxisLabel$ = "value",
                                     Optional groupLabelCSSFont$ = CSSFont.Win7Large,
                                     Optional YAxisLabelFontCSS$ = CSSFont.Win7Large,
                                     Optional tickFontCSS$ = CSSFont.Win7LittleLarge,
                                     Optional regionStroke$ = Stroke.AxisStroke,
                                     Optional interval# = 100,
                                     Optional dotSize! = 10,
                                     Optional lineWidth% = 2,
                                     Optional rangeScale# = 1.25,
                                     Optional showDataPoints As Boolean = True,
                                     Optional showOutliers As Boolean = True,
                                     Optional fillBox As Boolean = True,
                                     Optional ppi As Integer = 100) As GraphicsData

        Dim yAxisLabelFont As Font = CSSFont.TryParse(YAxisLabelFontCSS).GDIObject(ppi)
        Dim groupLabelFont As Font = CSSFont.TryParse(groupLabelCSSFont).GDIObject(ppi)
        Dim tickLabelFont As Font = CSSFont.TryParse(tickFontCSS).GDIObject(ppi)
        Dim colors As LoopArray(Of SolidBrush) = Designer _
            .GetColors(schema) _
            .Select(Function(color) New SolidBrush(color)) _
            .ToArray
        Dim ticks#() = data _
            .Groups _
            .Select(Function(x) x.Value) _
            .IteratesALL _
            .Range _
            .CreateAxisTicks
        Dim ranges As DoubleRange = ticks Or Zero

        ranges *= rangeScale

        Dim plotInternal =
            Sub(ByRef g As IGraphics, rect As GraphicsRegion)

                Dim plotRegion = rect.PlotRegion
                Dim leftPart = yAxisLabelFont.Height + tickLabelFont.Height + 50
                Dim bottomPart = groupLabelFont.Height + 50

                If ranges.Length = 0 Then
                    Return  ' 没有数据的话，则直接退出绘图操作 
                End If

                With plotRegion

                    Dim topLeft = .Location.OffSet2D(leftPart, 0)
                    Dim rectSize As New Size(
                        width:= .Width - leftPart,
                        height:= .Height - bottomPart)

                    plotRegion = New Rectangle(topLeft, rectSize)
                End With

                Dim boxWidth = StackedBarPlot.BarWidth(plotRegion.Width - 2 * interval, data.Groups.Length, interval)
                Dim bottom = plotRegion.Bottom
                Dim height = plotRegion.Height
                Dim y = Function(x#) bottom - height * (x - ranges.Min) / ranges.Length

                If Not regionStroke.StringEmpty Then
                    Call g.DrawRectangle(
                        Stroke.TryParse(regionStroke).GDIObject,
                        plotRegion)
                End If

                ' x0在盒子的左边
                Dim x0! = rect.Padding.Left + leftPart + interval
                Dim y0!
                Dim labelSize As SizeF
                Dim tickPen As Pen = Stroke.TryParse(regionStroke).GDIObject

                ' 绘制盒子
                ' 当不填充盒子的时候，使用的线条和点的颜色都是彩色的
                ' 当进行盒子的填充的时候，线条和点的颜色都是黑色的，盒子使用自定的颜色进行填充
                For Each group As NamedValue(Of Vector) In data.Groups
                    Dim quartile = group.Value.Quartile
                    Dim outlier = group.Value.Outlier(quartile)
                    Dim brush As SolidBrush = colors.Next   ' 得到了色彩画刷
                    Dim x1 = x0 + boxWidth / 2  ' x1在盒子的中间
                    Dim pen As Pen

                    If fillBox Then
                        ' 使用彩色画刷填充盒子，但是线条和点都是黑色的
                        pen = New Pen(Color.Black, lineWidth)
                        ' 先填充盒子
                        ' y 分别为q1和q3
                        Dim box As New Rectangle With {
                            .Location = New Drawing.Point(x0, y(quartile.Q3)),
                            .Size = New Size(boxWidth, y(quartile.Q1) - y(quartile.Q3))
                        }
                        g.FillRectangle(brush, rect:=box)
                    Else
                        pen = New Pen(brush.Color, lineWidth)
                    End If

                    If Not outlier.outlier.IsNullOrEmpty Then
                        quartile = outlier.normal.Quartile
                    End If

                    ' max
                    y0 = y(quartile.range.Max)
                    g.DrawLine(pen, New Drawing.Point(x0, y0), New Drawing.Point(x0 + boxWidth, y0))

                    ' min
                    y0 = y(quartile.range.Min)
                    g.DrawLine(pen, New Drawing.Point(x0, y0), New Drawing.Point(x0 + boxWidth, y0))

                    ' q1
                    Dim q1Y = y(quartile.Q1)
                    g.DrawLine(pen, New Drawing.Point(x0, q1Y), New Drawing.Point(x0 + boxWidth, q1Y))

                    ' q2
                    Dim q2Y = y(quartile.Q2)
                    g.DrawLine(pen, New Drawing.Point(x0, q2Y), New Drawing.Point(x0 + boxWidth, q2Y))
                    g.DrawLine(pen, New Drawing.Point(x0, q2Y + lineWidth), New Drawing.Point(x0 + boxWidth, q2Y + lineWidth))
                    g.DrawLine(pen, New Drawing.Point(x0, q2Y + 2 * lineWidth), New Drawing.Point(x0 + boxWidth, q2Y + 2 * lineWidth))

                    ' q3
                    Dim q3Y = y(quartile.Q3)
                    g.DrawLine(pen, New Drawing.Point(x0, q3Y), New Drawing.Point(x0 + boxWidth, q3Y))

                    ' box
                    g.DrawLine(pen, New Drawing.Point(x0, q3Y), New Drawing.Point(x0, q1Y))
                    g.DrawLine(pen, New Drawing.Point(x0 + boxWidth, q3Y), New Drawing.Point(x0 + boxWidth, q1Y))

                    ' dashline to min/max
                    pen = New Pen(brush.Color, lineWidth) With {
                        .DashStyle = DashStyle.Dash
                    }

                    g.DrawLine(pen, New Drawing.Point(x1, y(quartile.range.Min)), New Drawing.Point(x1, q1Y))
                    g.DrawLine(pen, New Drawing.Point(x1, y(quartile.range.Max)), New Drawing.Point(x1, q3Y))

                    If fillBox Then
                        brush = Brushes.Black
                    End If

                    ' outliers + normal points
                    If showDataPoints Then
                        For Each n As Double In outlier.normal
                            Call g.FillEllipse(brush, New PointF(x1, y(n)).CircleRectangle(dotSize))
                        Next
                    End If
                    If showOutliers Then
                        For Each n As Double In outlier.outlier
                            Call g.FillEllipse(brush, New PointF(x1, y(n)).CircleRectangle(dotSize))
                        Next
                    End If

                    ' draw group label
                    labelSize = g.MeasureString(group.Name, groupLabelFont)
                    g.DrawString(group.Name, groupLabelFont, Brushes.Black, New PointF(x1 - labelSize.Width / 2, bottom + 20))
                    g.DrawLine(tickPen, New Drawing.Point(x1, bottom + 20), New Drawing.Point(x1, bottom))

                    x0 += boxWidth + interval
                Next

                Dim text As New GraphicsText(DirectCast(g, Graphics2D).Graphics)
                Dim label$

                x0! = rect.Padding.Left + leftPart

                ' 绘制y坐标轴
                For Each d As Double In ticks
                    y0 = y(d)
                    g.DrawLine(tickPen, New Drawing.Point(x0, y0), New Drawing.Point(x0 - 10, y0))
                    ' label = d.ToString("F2")
                    label = d
                    labelSize = g.MeasureString(label, tickLabelFont)
                    text.DrawString(label,
                                    tickLabelFont,
                                    Brushes.Black,
                                    New PointF With {
                                        .X = x0 - 10 - labelSize.Height,
                                        .Y = y0 + labelSize.Width / 2
                                    },
                                    angle:=-90)
                Next

                Dim canvasPadding = rect.Padding
                ' 绘制y坐标轴标签
                labelSize = g.MeasureString(YaxisLabel, yAxisLabelFont)

                Dim location As New PointF With {
                    .X = canvasPadding.Left + (leftPart - tickLabelFont.Height - labelSize.Height) / 2,
                    .Y = rect.PlotRegion.Height / 2
                }
                text.DrawString(YaxisLabel, yAxisLabelFont, Brushes.Black, location, angle:=-90)
            End Sub

        Return g.GraphicsPlots(
            size.SizeParser, padding,
            bg,
            plotInternal)
    End Function
End Module
