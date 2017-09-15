Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Text
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS
Imports Microsoft.VisualBasic.Scripting.Runtime

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
                    Dim tickFont As Font = CSSFont.TryParse(tickFontCSS)
                    Dim axisFont As Font = CSSFont.TryParse(axisLabelFontCSS)
                    Dim legendFont As Font = CSSFont.TryParse(legendLabelFontCSS)
                    Dim tickSize = g.MeasureString("0.00", tickFont)
                    Dim leftPart = axisFont.Height + tickSize.Width + 10
                    Dim barRegionWidth = width - leftPart
                    Dim wb = BarWidth(barRegionWidth, n, interval)
                    Dim groupLabelFont As Font = CSSFont.TryParse(groupLabelFontCSS)
                    Dim boxWidth% = legendFont.Height * 1.1
                    Dim bottomPart = groupLabelFont.Height + 30 + (boxWidth + interval * 2) * columnCount
                    Dim barRegionHeight = height - bottomPart   ' 条形图区域的总高度
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

                        Dim maxWidth%

                        For Each legend As NamedValue(Of SolidBrush) In block
                            Dim box As New Rectangle(x0, ly, boxWidth, boxWidth)
                            ' 绘制方形色块
                            g.FillRectangle(legend.Value, box)
                            ' 绘制系列标签
                            g.DrawString(legend.Name, legendFont, Brushes.Black, New PointF(x0 + boxWidth + 5, ly))

                            maxWidth = Math.Max(maxWidth, g.MeasureString(legend.Name, legendFont).Width)
                            ly += interval + boxWidth
                        Next

                        ly = bottomY
                        x0 += interval * 2 + boxWidth + maxWidth
                    Next
                End Sub

            Return g.GraphicsPlots(
                size.SizeParser, padding,
                bg,
                plotInternal)
        End Function
    End Module
End Namespace