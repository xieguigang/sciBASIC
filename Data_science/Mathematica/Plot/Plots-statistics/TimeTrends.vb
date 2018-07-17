Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.base
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Axis
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.d3js.Layout
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Math2D
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS
Imports Microsoft.VisualBasic.Scripting.Runtime

Public Module TimeTrends

    Public Structure TimePoint

        Dim [date] As Date
        Dim average As Double
        ''' <summary>
        ''' [min, max]
        ''' </summary>
        Dim range As DoubleRange

        Public Overrides Function ToString() As String
            Return $"<{[date].ToString}> {average} IN [{range.Min}, {range.Max}]"
        End Function
    End Structure

    <Extension>
    Public Function Plot(data As IEnumerable(Of TimePoint),
                         Optional size$ = "3600,2400",
                         Optional padding$ = g.DefaultPadding,
                         Optional bg$ = "white",
                         Optional lineWidth! = 20,
                         Optional lineColor$ = "darkblue",
                         Optional pointSize! = 30,
                         Optional pointColor$ = "blue",
                         Optional rangeColor$ = "skyblue",
                         Optional rangeOpacity! = 0.45,
                         Optional rangeStroke$ = "stroke: darkblue; stroke-width: 1px; stroke-dash: solid;",
                         Optional axisStrokeCSS$ = Stroke.AxisStroke,
                         Optional yTickStrokeCSS$ = Stroke.AxisGridStroke,
                         Optional cubicSplineExpected% = 25,
                         Optional valueLabelFormat$ = "G2",
                         Optional valueLabelFontCSS$ = CSSFont.Win7VeryVeryLarge,
                         Optional tickLabelFontCSS$ = CSSFont.Win7VeryLarge) As GraphicsData

        Dim dates = data.OrderBy(Function(d) d.date).ToArray
        Dim timer As TimeRange = dates _
            .Select(Function(d) d.date) _
            .ToArray
        Dim yTicks#() = dates _
            .Select(Iterator Function(d)
                        Yield d.average
                        Yield d.range.Min
                        Yield d.range.Max
                    End Function) _
            .IteratesALL _
            .Range _
            .CreateAxisTicks(5)
        Dim rangePoly As (min As List(Of PointF), max As List(Of PointF))

        Dim valueLabelFont As Font = CSSFont.TryParse(valueLabelFontCSS)
        Dim tickLabelFont As Font = CSSFont.TryParse(tickLabelFontCSS)
        Dim lineStyle As New Pen(lineColor.TranslateColor, lineWidth)
        Dim axisPen As Pen = Stroke.TryParse(axisStrokeCSS).GDIObject
        Dim yTickPen As Pen = Stroke.TryParse(yTickStrokeCSS).GDIObject
        Dim rgPen As Pen = Stroke.TryParse(rangeStroke).GDIObject
        Dim rgColor As Color = rangeColor _
            .TranslateColor _
            .Alpha(255 * rangeOpacity)
        Dim pointBrush As New SolidBrush(pointColor.TranslateColor)
        Dim plotInternal =
            Sub(ByRef g As IGraphics, region As GraphicsRegion)
                Dim yScaler = region.YScaler(yTicks)
                Dim xScaler = timer.Scaler(region.XRange)
                Dim rect As Rectangle = region.PlotRegion
                Dim x#, y#
                Dim ty#() = {0, 0, 0}
                Dim trends As New List(Of PointF)
                Dim labelSize As SizeF
                Dim labelText$

                For Each yVal As Double In yTicks
                    y = yScaler(yVal)
                    labelText = yVal
                    labelSize = g.MeasureString(labelText, tickLabelFont)

                    g.DrawLine(yTickPen, CSng(rect.Left), CSng(y), CSng(rect.Right), CSng(y))
                    g.DrawString(labelText, tickLabelFont, Brushes.Gray,
                         x:=rect.Left - labelSize.Width - 5,
                         y:=y - labelSize.Height / 2
                    )
                Next

                rangePoly = (New List(Of PointF), New List(Of PointF))

                For Each time As TimePoint In dates
                    x = xScaler(time.date)
                    ty = {
                        yScaler(time.range.Min),
                        yScaler(time.average),
                        yScaler(time.range.Max)
                    }
                    trends.Add(New PointF(x, ty(1)))
                    rangePoly.min.Add(New PointF(x, ty(0)))
                    rangePoly.max.Add(New PointF(x, ty(2)))
                Next

                With rangePoly
                    Dim a = .max.First +
                            .max.CubicSpline(cubicSplineExpected).AsList +
                            .max.Last
                    Dim b = .min.Last +
                            .min.ReverseIterator.CubicSpline(cubicSplineExpected).AsList +
                            .min.First
                    Dim polygon As GraphicsPath = (a.AsList + b).GraphicsPath
                    Dim br As New SolidBrush(rgColor)

                    Call g.FillPath(br, polygon)

                    If Not rgPen Is Nothing Then
                        For Each line In a.SlideWindows(winSize:=2)
                            Call g.DrawLine(rgPen, line(0), line(1))
                        Next
                        For Each line In b.SlideWindows(winSize:=2)
                            Call g.DrawLine(rgPen, line(0), line(1))
                        Next
                    End If
                End With

                trends = trends.First +
                         trends.CubicSpline(cubicSplineExpected).AsList +
                         trends.Last

                For Each line As SlideWindow(Of PointF) In trends.SlideWindows(winSize:=2)
                    Call g.DrawLine(lineStyle, line(0), line(1))
                Next

                Dim labels As New List(Of Label)
                Dim anchors As New List(Of Anchor)

                For Each time As TimePoint In dates
                    x = xScaler(time.date)
                    y = yScaler(time.average)
                    labelText = time.average.ToString(valueLabelFormat)
                    labelSize = g.MeasureString(labelText, valueLabelFont)
                    labels += New Label With {
                        .width = labelSize.Width,
                        .height = labelSize.Height,
                        .text = labelText,
                        .X = x,
                        .Y = y
                    }
                    anchors += New Anchor With {
                        .r = 5,
                        .x = x,
                        .y = y
                    }

                    Call g.DrawCircle(New PointF(x, y), pointSize, pointBrush)
                Next

                Call d3js.labeler _
                    .Size(rect.Size) _
                    .Labels(labels) _
                    .Anchors(anchors) _
                    .Start(500)

                For Each label As Label In labels
                    Call g.DrawString(label.text, valueLabelFont, Brushes.Black, label.X, label.Y)
                Next
            End Sub

        Return g.GraphicsPlots(
            size.SizeParser,
            padding, bg,
            plotInternal
        )
    End Function
End Module
