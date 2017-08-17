Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS
Imports Microsoft.VisualBasic.Scripting.Runtime

Namespace BarPlot

    Public Module StackedBarPlot

        Public Function Plot(data As BarDataGroup,
                             Optional size$ = "3000,2700",
                             Optional padding$ = g.DefaultPadding,
                             Optional bg$ = "white",
                             Optional percentStacked! = no,
                             Optional YaxisTitle$ = "Value",
                             Optional interval! = 5,
                             Optional columnCount% = 8,
                             Optional legendLabelFontCSS$ = CSSFont.Win10NormalLarger,
                             Optional tickFontCSS$ = CSSFont.Win10Normal,
                             Optional groupLabelFontCSS$ = CSSFont.Win7Bold,
                             Optional axisLabelFontCSS$ = CSSFont.Win7LargerNormal) As GraphicsData

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
                    Dim wb = (barRegionWidth - (n - 1) * interval) / n
                    Dim groupLabelFont As Font = CSSFont.TryParse(groupLabelFontCSS)
                    Dim bottomPart = groupLabelFont.Height + 30 + (legendFont.Height + interval) * columnCount
                    Dim barRegionHeight = height - bottomPart   ' 条形图区域的总高度

                    Dim x0! = rect.Padding.Left + leftPart

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
                End Sub

            Return g.GraphicsPlots(
                size.SizeParser, padding,
                bg,
                plotInternal)
        End Function
    End Module
End Namespace