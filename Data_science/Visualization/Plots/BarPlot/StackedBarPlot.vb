#Region "Microsoft.VisualBasic::e849ed570493e9d4baf9416dbca98104, sciBASIC#\Data_science\Visualization\Plots\BarPlot\StackedBarPlot.vb"

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

    '   Total Lines: 156
    '    Code Lines: 107
    ' Comment Lines: 25
    '   Blank Lines: 24
    '     File Size: 7.59 KB


    '     Module StackedBarPlot
    ' 
    '         Function: BarWidth, Plot
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.ChartPlots.BarPlot.Data
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Text
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports stdNum = System.Math

Namespace BarPlot

    Public Module StackedBarPlot

        Public Function BarWidth(regionWidth%, n%, interval#) As Single
            Return (regionWidth - (n - 1) * interval) / n
        End Function

        ''' <summary>
        ''' 绘制百分比堆积的条形图
        ''' </summary>
        ''' <param name="data"></param>
        ''' <param name="size$"></param>
        ''' <param name="padding$"></param>
        ''' <param name="bg$"></param>
        ''' <param name="percentStacked!"></param>
        ''' <param name="YaxisTitle$"></param>
        ''' <param name="interval!"></param>
        ''' <param name="columnCount%"></param>
        ''' <param name="legendLabelFontCSS$"></param>
        ''' <param name="tickFontCSS$"></param>
        ''' <param name="groupLabelFontCSS$"></param>
        ''' <param name="axisLabelFontCSS$"></param>
        ''' <returns></returns>
        Public Function Plot(data As BarDataGroup,
                             Optional size$ = "3000,2700",
                             Optional padding$ = g.DefaultPadding,
                             Optional bg$ = "white",
                             Optional percentStacked! = no,
                             Optional YaxisTitle$ = "Value",
                             Optional interval! = 5,
                             Optional boxSeperator! = 5,
                             Optional columnCount% = 8,
                             Optional legendLabelFontCSS$ = CSSFont.Win7LittleLarge,
                             Optional tickFontCSS$ = CSSFont.Win7LittleLarge,
                             Optional groupLabelFontCSS$ = CSSFont.Win7LittleLarge,
                             Optional axisLabelFontCSS$ = CSSFont.Win7Large) As GraphicsData

            Dim serialBrushes = data.Serials _
                .Select(Function(s)
                            Return New NamedValue(Of SolidBrush) With {
                                .Name = s.Name,
                                .Value = New SolidBrush(s.Value)
                            }
                        End Function) _
                .ToArray
            Dim n = data.Samples.Length
            Dim plotInternal =
                Sub(ByRef g As IGraphics, rect As GraphicsRegion)

                    Dim width = rect.PlotRegion.Width
                    Dim height = rect.PlotRegion.Height
                    Dim tickFont As Font = CSSFont.TryParse(tickFontCSS).GDIObject(g.Dpi)
                    Dim axisFont As Font = CSSFont.TryParse(axisLabelFontCSS).GDIObject(g.Dpi)
                    Dim legendFont As Font = CSSFont.TryParse(legendLabelFontCSS).GDIObject(g.Dpi)
                    Dim tickSize = g.MeasureString("0.00", tickFont)
                    Dim leftPart = axisFont.Height + tickSize.Width + 10
                    Dim barRegionWidth = width - leftPart
                    Dim wb = BarWidth(barRegionWidth, n, interval)
                    Dim groupLabelFont As Font = CSSFont.TryParse(groupLabelFontCSS).GDIObject(g.Dpi)
                    Dim boxWidth% = legendFont.Height * 1.1
                    Dim bottomPart = groupLabelFont.Height + 30 + (boxWidth + boxSeperator * 2) * columnCount
                    ' 条形图区域的总高度
                    Dim barRegionHeight = height - bottomPart
                    Dim x0! = rect.Padding.Left + leftPart

                    Call New GraphicsText(DirectCast(g, Graphics2D).Graphics).DrawString(
                        YaxisTitle, axisFont,
                        Brushes.Black,
                        New PointF((rect.Padding.Left - axisFont.Height), height / 2),
                        angle:=-90)

                    ' 绘制y轴
                    For Each tick# In {0.00, 0.25, 0.5, 0.75, 1.0}
                        Dim y# = rect.Height - rect.Padding.Bottom - bottomPart - barRegionHeight * tick
                        Dim location As New Point(x0 - tickSize.Width - 20, y - tickSize.Height / 2)

                        g.DrawLine(Pens.Black, New Point(x0 - 10, y), New Point(x0 - 20, y))
                        g.DrawString(tick.ToString("F2"), tickFont, Brushes.Black, location)
                    Next

                    ' 遍历X轴上面的每一个分组
                    For Each group As BarDataSample In data.Samples

                        Dim y0! = rect.Padding.Top
                        Dim sum# = group.StackedSum

                        ' 慢慢的从上面累加y到下面底部
                        For Each serial As SeqValue(Of NamedValue(Of SolidBrush)) In serialBrushes.SeqIterator
                            Dim value As Double = group.data(serial) / sum  ' 百分比
                            Dim h = value * barRegionHeight
                            Dim bar As New RectangleF(New PointF(x0, y0), New SizeF(wb, h))

                            g.FillRectangle(serial.value.Value, rect:=bar)
                            y0 += h
                        Next

                        Dim x!, y!
                        Dim labelSize = g.MeasureString(group.Tag, groupLabelFont)

                        x = x0 + (wb - labelSize.Width) / 2
                        y = y0 + (30)
                        Call g.DrawString(group.Tag, groupLabelFont, Brushes.Black, New PointF(x, y))

                        x0 += wb + interval
                    Next

                    ' 绘制图例
                    Dim bottomY = rect.Padding.Top + barRegionHeight + boxWidth * 2 + groupLabelFont.Height
                    Dim ly! = bottomY

                    x0 = rect.Padding.Left + leftPart

                    For Each block In serialBrushes.Split(columnCount)

                        ' 似乎在for循环之中申明的变量必须要初始化，否则下一个循环使用的是上一个循环的结果值？？？
                        ' 这是一个bug？
                        Dim maxWidth% = 0

                        For Each legend As NamedValue(Of SolidBrush) In block
                            Dim box As New Rectangle(x0, ly, boxWidth, boxWidth)
                            ' 绘制方形色块
                            g.FillRectangle(legend.Value, box)
                            ' 绘制系列标签
                            g.DrawString(legend.Name, legendFont, Brushes.Black, New PointF(x0 + boxWidth + 5, ly))

                            maxWidth = stdNum.Max(maxWidth, g.MeasureString(legend.Name, legendFont).Width)
                            ly += boxSeperator + boxWidth
                        Next

                        ly = bottomY
                        x0 += boxSeperator * 2 + boxWidth + maxWidth
                        maxWidth = 0
                    Next
                End Sub

            Return g.GraphicsPlots(
                size.SizeParser, padding,
                bg,
                plotInternal)
        End Function
    End Module
End Namespace
