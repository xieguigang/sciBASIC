#Region "Microsoft.VisualBasic::50933a7419438fb766d2606d4c5872dc, Data_science\Visualization\Plots-statistics\Zscore\ZScoresPlot.vb"

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

    '   Total Lines: 163
    '    Code Lines: 135 (82.82%)
    ' Comment Lines: 6 (3.68%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 22 (13.50%)
    '     File Size: 7.12 KB


    ' Class ZScoresPlot
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Sub: PlotInternal
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Axis
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Canvas
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Legend
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.MIME.Html.Render

Public Class ZScoresPlot : Inherits Plot

    Dim ticks#()
    Dim range As DoubleRange
    Dim maxGroupLabel$
    Dim maxSerialsLabel$
    Dim groups As Dictionary(Of String, String())
    Dim colors As Dictionary(Of String, Color)
    Dim data As ZScores
    Dim displayZERO As Boolean

    Public Sub New(data As ZScores, displayZERO As Boolean, theme As Theme)
        MyBase.New(theme)

        ticks = data.Range.CreateAxisTicks
        range = ticks
        maxGroupLabel = data.groups.Keys.MaxLengthString
        maxSerialsLabel = data.serials.Keys.MaxLengthString
        groups = data.groups
        colors = data.colors

        Me.data = data
        Me.displayZERO = displayZERO
    End Sub

    Protected Overrides Sub PlotInternal(ByRef g As IGraphics, canvas As GraphicsRegion)
        Dim css As CSSEnvirnment = g.LoadEnvironment
        Dim serialLabelFont As Font = css.GetFont(CSSFont.TryParse(theme.tagCSS))
        Dim legendLabelFont As Font = css.GetFont(CSSFont.TryParse(theme.legendLabelCSS))
        Dim titleFont As Font = css.GetFont(CSSFont.TryParse(theme.mainCSS))
        Dim tickFont As Font = css.GetFont(CSSFont.TryParse(theme.axisTickCSS))
        Dim maxSerialLabelSize As SizeF = g.MeasureString(maxSerialsLabel, serialLabelFont)
        Dim maxLegendLabelSize As SizeF = g.MeasureString(maxGroupLabel, legendLabelFont)
        Dim pointSize As New SizeF(theme.pointSize, theme.pointSize)
        Dim axisStroke As Pen = css.GetPen(Stroke.TryParse(theme.axisStroke))
        Dim plotRect = canvas.PlotRegion(css)
        Dim padding As PaddingLayout = PaddingLayout.EvaluateFromCSS(css, canvas.Padding)

        ' 计算出layout信息
        Dim plotWidth% = plotRect.Width _
                                 - maxSerialLabelSize.Width _
                                 - maxLegendLabelSize.Width _
                                 - maxLegendLabelSize.Height _
                                 - 30
        Dim plotHeight = plotRect.Height - titleFont.Height - tickFont.Height - 20
        Dim plotWidthRange As DoubleRange = New Double() {0, plotWidth}
        Dim X = Function(Z#)
                    Return padding.Left _
                                   + maxSerialLabelSize.Width _
                                   + 5 _
                                   + range.ScaleMapping(Z, plotWidthRange)
                End Function
        Dim dy! = plotHeight / (data.serials.Length)
        Dim yTop! = padding.Top
        Dim left! = X(range.Min)
        Dim labelSize As SizeF
        Dim labelPosition As PointF
        Dim pt As PointF

        ' 分别绘制出X坐标轴和Y坐标轴
        g.DrawLine(axisStroke, New PointF(left, yTop), New PointF(left, yTop + plotHeight))
        g.DrawLine(axisStroke,
                           New PointF(left, yTop + plotHeight),
                           New PointF(left + plotWidth, yTop + plotHeight))

        If displayZERO Then
            Dim zeroPen As Pen = css.GetPen(Stroke.TryParse(theme.lineStroke))

            g.DrawLine(zeroPen,
                               New PointF(X(0), yTop),
                               New PointF(X(0), yTop + plotHeight))
        End If

        ' 绘制出每一个系列的点和相应的标签字符串
        For Each serial As DataSet In data.serials
            Dim labelY = yTop + (dy - serialLabelFont.Height) / 2
            Dim yPoints! = yTop + (dy - theme.pointSize) / 2

            labelSize = g.MeasureString(serial.ID, serialLabelFont)
            labelPosition = New PointF(left - labelSize.Width, labelY)
            g.DrawString(serial.ID, serialLabelFont, Brushes.Black, labelPosition)

            For Each group As KeyValuePair(Of String, String()) In groups
                Dim color As New SolidBrush(colors(group.Key))

                For Each Z As Double In serial(group.Value) _
                            .Where(Function(n)
                                       Return Not n.IsNaNImaginary
                                   End Function)

                    pt = New PointF(X(Z), yPoints)
                    g.FillEllipse(color, New RectangleF(pt, pointSize))
                Next
            Next

            yTop += dy
        Next

        ' 绘制出X轴的ticks
        For Each tick As Double In ticks
            labelSize = g.MeasureString(tick, tickFont)
            pt = New PointF(X(tick), yTop)
            labelPosition = New PointF With {
                        .X = pt.X - labelSize.Width / 2,
                        .Y = yTop + 10
                    }

            g.DrawString(tick, tickFont, Brushes.Black, labelPosition)
            g.DrawLine(Pens.Black, New PointF(pt.X, yTop), New PointF(pt.X, yTop + 8))
        Next

        ' 绘制出标题
        yTop! = padding.Top
        labelSize = g.MeasureString(main, titleFont)
        labelPosition = New PointF With {
                    .X = left + (plotWidth - labelSize.Width) / 2,
                    .Y = yTop + plotHeight + tickFont.Height + 20
                }

        g.DrawString(main, titleFont, Brushes.Black, labelPosition)

        ' 绘制legend
        Dim legendHeight! = (legendLabelFont.Height + 5) * groups.Count
        Dim maxWidth = maxLegendLabelSize.Width + legendLabelFont.Height * 3
        Dim legendLocation As New Point With {
                    .X = X(range.Max) + (padding.Right - maxWidth) / 2,
                    .Y = yTop + (plotHeight - legendHeight) / 2
                }
        Dim shapes = data.shapes
        Dim legends = groups _
                    .Keys _
                    .Select(Function(label)
                                Return New LegendObject With {
                                    .title = label,
                                    .color = colors(label).RGBExpression,
                                    .fontstyle = theme.legendLabelCSS,
                                    .style = shapes(label)
                                }
                            End Function) _
                    .ToArray
        Dim legendBoxBorder As Stroke = Stroke.TryParse(theme.legendBoxStroke)

        Call g.DrawLegends(
                    topLeft:=legendLocation,
                    legends:=legends,
                    gSize:=$"{legendLabelFont.Height * 2},{legendLabelFont.Height}",
                    regionBorder:=legendBoxBorder
                )
    End Sub
End Class
