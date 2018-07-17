Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.base
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Axis
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
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
                         Optional size$ = "3300,2700",
                         Optional padding$ = g.DefaultPadding,
                         Optional bg$ = "white",
                         Optional lineWidth! = 5,
                         Optional lineColor$ = "darkblue",
                         Optional rangeColor$ = "skyblue",
                         Optional rangeOpacity! = 0.45,
                         Optional axisStrokeCSS$ = Stroke.AxisStroke,
                         Optional yTickStrokeCSS$ = Stroke.AxisGridStroke) As GraphicsData

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
            .CreateAxisTicks
        Dim rangePoly As (min As List(Of PointF), max As List(Of PointF))

        Dim lineStyle As New Pen(lineColor.TranslateColor, lineWidth)
        Dim axisPen As Pen = Stroke.TryParse(axisStrokeCSS).GDIObject
        Dim yTickPen As Pen = Stroke.TryParse(yTickStrokeCSS).GDIObject
        Dim rgColor As Color = rangeColor _
            .TranslateColor _
            .Alpha(255 * rangeOpacity)
        Dim plotInternal =
            Sub(ByRef g As IGraphics, region As GraphicsRegion)
                Dim yScaler = region.YScaler(yTicks)
                Dim xScaler = timer.Scaler(region.XRange)
                Dim rect As Rectangle = region.PlotRegion
                Dim x#, y#
                Dim p1, p2 As PointF

                For Each yVal As Double In yTicks
                    y = yScaler(yVal)
                    g.DrawLine(yTickPen, CSng(rect.Left), CSng(y), CSng(rect.Right), CSng(y))
                Next

                rangePoly = (New List(Of PointF), New List(Of PointF))
                x = xScaler(dates(Scan0).date)
                rangePoly.min.Add(New PointF(x, yScaler(dates(Scan0).range.Min)))
                rangePoly.max.Add(New PointF(x, yScaler(dates(Scan0).range.Max)))

                Dim trends As New List(Of (a As PointF, b As PointF))

                For Each win In dates.SlideWindows(winSize:=2)
                    Dim x1 = xScaler(win(0).date)
                    Dim x2 = xScaler(win(1).date)

                    p1 = New PointF(x1, yScaler(win(0).average))
                    p2 = New PointF(x2, yScaler(win(1).average))

                    Call rangePoly.min.Add(New PointF(x2, yScaler(win(0).range.Min)))
                    Call rangePoly.max.Add(New PointF(x2, yScaler(win(0).range.Max)))
                    Call trends.Add((p1, p2))
                Next

                With rangePoly
                    Dim polygon As GraphicsPath = (.max + .min.ReverseIterator).GraphicsPath
                    Dim br As New SolidBrush(rgColor)

                    Call g.FillPath(br, polygon)
                End With

                For Each line In trends
                    Call g.DrawLine(lineStyle, line.a, line.b)
                Next
            End Sub

        Return g.GraphicsPlots(
            size.SizeParser,
            padding, bg,
            plotInternal
        )
    End Function
End Module
