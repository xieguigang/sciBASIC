Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Driver
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
                         Optional rangeOpacity! = 0.45) As GraphicsData

        Dim dates = data.OrderBy(Function(d) d.date).ToArray
        Dim linePen As New Pen(lineColor.TranslateColor, lineWidth)
        Dim rgColor As Color = rangeColor _
            .TranslateColor _
            .Alpha(255 * rangeOpacity)
        Dim plotInternal =
            Sub(ByRef g As IGraphics, region As GraphicsRegion)

            End Sub

        Return g.GraphicsPlots(
            size.SizeParser,
            padding, bg,
            plotInternal
        )
    End Function
End Module
