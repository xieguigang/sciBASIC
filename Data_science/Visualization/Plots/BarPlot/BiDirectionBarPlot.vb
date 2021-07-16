
Imports System.Drawing
Imports Microsoft.VisualBasic.Data.ChartPlots.BarPlot.Data
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Canvas
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Legend
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Axis

Namespace BarPlot

    ''' <summary>
    ''' compare two data set
    ''' </summary>
    Public Class BiDirectionBarPlot : Inherits Plot

        ReadOnly data As BiDirectionData
        ReadOnly colorFactor1 As SolidBrush
        ReadOnly colorFactor2 As SolidBrush

        Public Sub New(data As BiDirectionData, color1 As Color, color2 As Color, theme As Theme)
            MyBase.New(theme)

            Me.data = data
            Me.colorFactor1 = New SolidBrush(color1)
            Me.colorFactor2 = New SolidBrush(color2)
        End Sub

        Protected Overrides Sub PlotInternal(ByRef g As IGraphics, canvas As GraphicsRegion)
            Dim rect As Rectangle = canvas.PlotRegion
            Dim dh As Double = rect.Height / data.size
            Dim barHeight As Double = dh * 0.7
            Dim labelFont As Font = CSSFont.TryParse(theme.axisLabelCSS)
            Dim maxLen As Double = g.MeasureString(data.samples.Select(Function(d) d.tag).MaxLengthString, labelFont).Width
            Dim boxLeft As Double = rect.Left + maxLen
            Dim boxWidth As Double = rect.Right - boxLeft
            Dim center As Double = boxLeft + boxWidth / 2
            Dim dataValues = data.samples.Select(Function(d) d.data).IteratesALL.Range(scale:=1.125)
            Dim scale = d3js.scale.linear().domain({0, dataValues.Max * 1.125}).range({0.0, boxWidth / 2})

            rect = New Rectangle(boxLeft, rect.Top, rect.Width - (boxLeft - rect.Left), rect.Height)

            Call g.DrawRectangle(Stroke.TryParse(theme.axisStroke).GDIObject, rect)
            Call g.DrawLine(Stroke.TryParse(theme.gridStrokeY).GDIObject, New PointF(center, rect.Top), New PointF(center, rect.Bottom))

            Dim y As Double = rect.Top - dh + dh * 0.15
            Dim charWidth As Single = g.MeasureString("X", labelFont).Width

            ' draw main title
            Dim titleFont As Font = CSSFont.TryParse(theme.mainCSS)
            Dim labelSize As SizeF = g.MeasureString(main, titleFont)
            Dim labelPos As New Point With {
                .X = rect.Left + (rect.Width - labelSize.Width) / 2,
                .Y = (canvas.Padding.Top - labelSize.Height) / 2
            }
            Dim tickLabelFont As Font = CSSFont.TryParse(theme.axisTickCSS)

            Call g.DrawString(main, titleFont, Brushes.Black, labelPos)

            For i As Integer = 0 To data.size - 1
                Dim sample As BarDataSample = data(i)

                y += dh

                ' draw left
                Dim len1 = scale(sample.data(0))
                Dim bar As New Rectangle(center - len1, y, len1, barHeight)

                g.FillRectangle(colorFactor1, bar)
                labelSize = g.MeasureString(sample.data(0).ToString(theme.axisTickFormat), tickLabelFont)
                g.DrawString(sample.data(0).ToString(theme.axisTickFormat), tickLabelFont, Brushes.Black, New PointF(bar.Left - labelSize.Width, bar.Top + (bar.Height - labelSize.Height) / 2))

                Dim len2 = scale(sample.data(1))

                bar = New Rectangle(center, y, len2, barHeight)
                g.FillRectangle(colorFactor2, bar)
                labelSize = g.MeasureString(sample.data(1).ToString(theme.axisTickFormat), tickLabelFont)
                g.DrawString(sample.data(1).ToString(theme.axisTickFormat), tickLabelFont, Brushes.Black, New PointF(bar.Right, bar.Top + (bar.Height - labelSize.Height) / 2))

                ' draw label
                labelSize = g.MeasureString(sample.tag, labelFont)
                labelPos = New Point With {
                    .X = boxLeft - charWidth - labelSize.Width,
                    .Y = y + (dh - labelSize.Height) / 2
                }

                Call g.DrawString(sample.tag, labelFont, Brushes.Black, labelPos)
            Next

            ' draw axis
            Dim ticks As Double() = data.samples.Select(Function(d) d.data).IteratesALL.CreateAxisTicks(ticks:=3)

            y = rect.Bottom + 10
            labelFont = CSSFont.TryParse(theme.axisTickCSS)
            labelSize = g.MeasureString(0, labelFont)

            ' draw ZERO
            labelPos = New Point(center - labelSize.Width / 2, y)
            g.DrawString(0, labelFont, Brushes.Black, labelPos)

            Dim offset As Double
            Dim x As Double

            For Each tick As Double In ticks
                offset = scale(tick)
                labelSize = g.MeasureString(tick.ToString(theme.axisTickFormat), labelFont)

                ' left
                x = center + offset - labelSize.Width / 2
                labelPos = New Point(x, y)
                g.DrawString(tick.ToString(theme.axisTickFormat), labelFont, Brushes.Black, labelPos)
                x = center + offset
                g.DrawLine(Pens.Black, New PointF(x, y), New PointF(x, rect.Bottom))

                ' right
                x = center - offset - labelSize.Width / 2
                labelPos = New Point(x, y)
                g.DrawString(tick.ToString(theme.axisTickFormat), labelFont, Brushes.Black, labelPos)
                g.DrawLine(Pens.Black, New PointF(center - offset, y), New PointF(center - offset, rect.Bottom))
            Next

            labelFont = CSSFont.TryParse(theme.axisLabelCSS)
            labelSize = g.MeasureString(xlabel, labelFont)
            labelPos = New Point With {
                .X = rect.Left + (rect.Width - labelSize.Width) / 2,
                .Y = rect.Bottom + labelSize.Height
            }

            Call g.DrawString(xlabel, labelFont, Brushes.Black, labelPos)

            ' draw legends
            Dim legends As LegendObject() = {
                New LegendObject With {.color = colorFactor1.Color.ToHtmlColor, .fontstyle = theme.legendLabelCSS, .style = LegendStyles.Square, .title = data.Factor1},
                New LegendObject With {.color = colorFactor2.Color.ToHtmlColor, .fontstyle = theme.legendLabelCSS, .style = LegendStyles.Square, .title = data.Factor2}
            }

            Call DrawLegends(g, legends, showBorder:=False, canvas:=canvas)
        End Sub
    End Class
End Namespace