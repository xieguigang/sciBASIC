#Region "Microsoft.VisualBasic::a4e40e25520176971167dc3d54ceea1f, Data_science\Visualization\Plots\BoxPlot\Box.vb"

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

    '   Total Lines: 241
    '    Code Lines: 178
    ' Comment Lines: 23
    '   Blank Lines: 40
    '     File Size: 10.58 KB


    '     Class Box
    ' 
    '         Properties: dotSize, fillBox, interval, lineWidth, rangeScale
    '                     showDataPoints, showOutliers
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: PlotBox, PlotInternal
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.DataStructures
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Data.ChartPlots.BarPlot
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Axis
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Canvas
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Text
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.Quantile
Imports Microsoft.VisualBasic.MIME.Html.CSS

Namespace BoxPlot

    Public Class Box : Inherits Plot

        ReadOnly data As BoxData

        Public Property interval As Double = 100
        Public Property fillBox As Boolean = True
        Public Property rangeScale As Double = 1
        Public Property lineWidth As Double = 2
        Public Property showDataPoints As Boolean
        Public Property showOutliers As Boolean
        Public Property dotSize As Single

        Public Sub New(data As BoxData, theme As Theme)
            Call MyBase.New(theme)

            Me.data = data
        End Sub

        Protected Overrides Sub PlotInternal(ByRef g As IGraphics, canvas As GraphicsRegion)
            Dim ppi As Integer = g.Dpi
            Dim yAxisLabelFont As Font = CSSFont.TryParse(theme.axisLabelCSS).GDIObject(ppi)
            Dim groupLabelFont As Font = CSSFont.TryParse(theme.tagCSS).GDIObject(ppi)
            Dim tickLabelFont As Font = CSSFont.TryParse(theme.axisTickCSS).GDIObject(ppi)
            Dim regionStroke As String = theme.lineStroke
            Dim colors As LoopArray(Of SolidBrush) = Designer _
                .GetColors(theme.colorSet) _
                .Select(Function(color) New SolidBrush(color)) _
                .ToArray
            Dim ticks#() = data.Groups _
                .Select(Function(x) x.Value) _
                .IteratesALL _
                .Range _
                .CreateAxisTicks
            Dim ranges As DoubleRange = ticks Or BoxPlot.Zero

            ranges *= rangeScale

            Dim plotRegion = canvas.PlotRegion
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
            Dim y = d3js.scale _
                .linear _
                .domain(ranges) _
                .range(values:=New Double() {plotRegion.Top, plotRegion.Bottom})
            Dim yscale As New DataScaler() With {
                .AxisTicks = Nothing,
                .region = plotRegion,
                .X = Nothing,
                .Y = y
            }

            If Not regionStroke.StringEmpty Then
                Call g.DrawRectangle(
                        Stroke.TryParse(regionStroke).GDIObject,
                        plotRegion)
            End If

            ' x0在盒子的左边
            Dim x0! = canvas.Padding.Left + leftPart + interval
            Dim y0!
            Dim labelSize As SizeF
            Dim tickPen As Pen = Stroke.TryParse(regionStroke).GDIObject

            ' 绘制盒子
            ' 当不填充盒子的时候，使用的线条和点的颜色都是彩色的
            ' 当进行盒子的填充的时候，线条和点的颜色都是黑色的，盒子使用自定的颜色进行填充
            For Each group As NamedValue(Of Vector) In data.Groups
                Dim brush As SolidBrush = colors.Next   ' 得到了色彩画刷
                Dim x1 = x0 + boxWidth / 2  ' x1在盒子的中间

                Call PlotBox(group, x0, brush, boxWidth, fillBox, lineWidth, yscale, dotSize, showDataPoints, showOutliers, g)

                ' draw group label
                labelSize = g.MeasureString(group.Name, groupLabelFont)

                g.DrawString(group.Name, groupLabelFont, Brushes.Black, New PointF(x1 - labelSize.Width / 2, bottom + 20))
                g.DrawLine(tickPen, New Drawing.Point(x1, bottom + 20), New Drawing.Point(x1, bottom))

                x0 += boxWidth + interval
            Next

            Dim text As New GraphicsText(DirectCast(g, Graphics2D).Graphics)
            Dim label$

            x0! = canvas.Padding.Left + leftPart

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

            Dim canvasPadding As Padding = canvas.Padding
            ' 绘制y坐标轴标签
            labelSize = g.MeasureString(ylabel, yAxisLabelFont)

            Dim location As New PointF With {
                    .X = canvasPadding.Left + (leftPart - tickLabelFont.Height - labelSize.Height) / 2,
                    .Y = canvas.PlotRegion.Height / 2
                }
            text.DrawString(ylabel, yAxisLabelFont, Brushes.Black, location, angle:=-90)
        End Sub

        Public Shared Sub PlotBox(group As NamedValue(Of Vector),
                                  x0 As Double,
                                  brush As SolidBrush,
                                  boxWidth As Double,
                                  fillBox As Boolean,
                                  lineWidth As Double,
                                  y As DataScaler,
                                  dotSize As Single,
                                  showDataPoints As Boolean,
                                  showOutliers As Boolean,
                                  g As IGraphics)

            Dim quartile = group.Value.Quartile
            Dim outlier = group.Value.Outlier(quartile)
            Dim x1 = x0 + boxWidth / 2  ' x1在盒子的中间
            Dim pen As Pen
            Dim y0 As Double
            Dim deltaWidth As Double = boxWidth / 2

            If fillBox Then
                ' 使用彩色画刷填充盒子，但是线条和点都是黑色的
                pen = New Pen(Color.Black, lineWidth)
                ' 先填充盒子
                ' y 分别为q1和q3
                Dim box As New Rectangle With {
                    .Location = New Drawing.Point(x0, y.TranslateY(quartile.Q3)),
                    .Size = New Size(boxWidth, y.TranslateY(quartile.Q1) - y.TranslateY(quartile.Q3))
                }
                g.FillRectangle(brush, rect:=box)
            Else
                pen = New Pen(brush.Color, lineWidth)
            End If

            If Not outlier.outlier.IsNullOrEmpty Then
                quartile = outlier.normal.Quartile
            End If

            ' max
            y0 = y.TranslateY(quartile.range.Max)
            g.DrawLine(pen, New Drawing.Point(x0 + deltaWidth / 2, y0), New Drawing.Point(x0 + deltaWidth * 1.5, y0))

            ' min
            y0 = y.TranslateY(quartile.range.Min)
            g.DrawLine(pen, New Drawing.Point(x0 + deltaWidth / 2, y0), New Drawing.Point(x0 + deltaWidth * 1.5, y0))

            ' q1
            Dim q1Y = y.TranslateY(quartile.Q1)
            ' g.DrawLine(pen, New Drawing.Point(x0, q1Y), New Drawing.Point(x0 + boxWidth, q1Y))

            ' q2
            Dim q2Y = y.TranslateY(quartile.Q2)
            g.DrawLine(pen, New Drawing.Point(x0, q2Y), New Drawing.Point(x0 + boxWidth, q2Y))
            g.DrawLine(pen, New Drawing.Point(x0, q2Y + lineWidth), New Drawing.Point(x0 + boxWidth, q2Y + lineWidth))
            g.DrawLine(pen, New Drawing.Point(x0, q2Y + 2 * lineWidth), New Drawing.Point(x0 + boxWidth, q2Y + 2 * lineWidth))

            ' q3
            Dim q3Y = y.TranslateY(quartile.Q3)
            ' g.DrawLine(pen, New Drawing.Point(x0, q3Y), New Drawing.Point(x0 + boxWidth, q3Y))

            ' box
            ' g.DrawLine(pen, New Drawing.Point(x0, q3Y), New Drawing.Point(x0, q1Y))
            ' g.DrawLine(pen, New Drawing.Point(x0 + boxWidth, q3Y), New Drawing.Point(x0 + boxWidth, q1Y))

            ' dashline to min/max
            pen = New Pen(brush.Color, lineWidth) With {
                .DashStyle = DashStyle.Dash
            }

            g.DrawLine(pen, New Drawing.Point(x1, y.TranslateY(quartile.range.Min)), New Drawing.Point(x1, q1Y))
            g.DrawLine(pen, New Drawing.Point(x1, y.TranslateY(quartile.range.Max)), New Drawing.Point(x1, q3Y))

            If fillBox Then
                brush = Brushes.Black
            End If

            ' outliers + normal points
            If showDataPoints Then
                For Each n As Double In outlier.normal
                    Call g.FillEllipse(brush, New PointF(x1, y.TranslateY(n)).CircleRectangle(dotSize))
                Next
            End If
            If showOutliers Then
                For Each n As Double In outlier.outlier
                    Call g.FillEllipse(brush, New PointF(x1, y.TranslateY(n)).CircleRectangle(dotSize))
                Next
            End If
        End Sub
    End Class
End Namespace
