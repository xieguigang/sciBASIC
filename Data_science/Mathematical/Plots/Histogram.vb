Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.ComponentModel.Ranges
Imports Microsoft.VisualBasic.Language

Public Module Histogram

    ''' <summary>
    ''' {x, y}
    ''' </summary>
    ''' <remarks>
    ''' <see cref="x1"/>到<see cref="x2"/>之间的距离是直方图的宽度
    ''' </remarks>
    Public Structure HistogramData
        Public x1#, x2#, y#

        Public ReadOnly Property width As Double
            Get
                Return x2# - x1#
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Structure

    <Extension>
    Public Function Plot(data As IEnumerable(Of HistogramData),
                         Optional color$ = "blue",
                         Optional bg$ = "white",
                         Optional size As Size = Nothing,
                         Optional margin As Size = Nothing,
                         Optional showGrid As Boolean = True) As Bitmap

        Return New HistogramGroup With {
            .Serials = {
                New NamedValue(Of Color) With {
                    .Name = NameOf(data),
                    .x = color.ToColor(Drawing.Color.Blue)
                }
            }
        }.Plot(bg, size, margin, showGrid)
    End Function

    Public Function Plot(data As IEnumerable(Of Double), xrange As DoubleRange,
                         Optional color$ = "blue",
                         Optional bg$ = "white",
                         Optional size As Size = Nothing,
                         Optional margin As Size = Nothing,
                         Optional showGrid As Boolean = True) As Bitmap
        Dim array#() = data.ToArray
        Dim delta = xrange.Length / array.Length
        Dim x As New Value(Of Double)(xrange.Min)
        Dim hist = From n As Double
                   In array
                   Let x1 As Double = x
                   Let x2 As Double = (x = x.value + delta)
                   Select New HistogramData With {
                       .x1 = x1,
                       .x2 = x2,
                       .y = n
                   }
        Return Plot(hist, color, bg, size, margin, showGrid)
    End Function

    Public Function Plot(xrange As DoubleRange, expression As Func(Of Double, Double),
                         Optional steps# = 0.01,
                         Optional color$ = "blue",
                         Optional bg$ = "white",
                         Optional size As Size = Nothing,
                         Optional margin As Size = Nothing,
                         Optional showGrid As Boolean = True) As Bitmap
        Dim data As IEnumerable(Of Double) =
            xrange _
            .seq(steps) _
            .Select(expression)
        Return Plot(data, xrange, color, bg, size, margin, showGrid)
    End Function

    Public Function Plot(xrange As NamedValue(Of DoubleRange), expression$,
                         Optional steps# = 0.01,
                         Optional color$ = "blue",
                         Optional bg$ = "white",
                         Optional size As Size = Nothing,
                         Optional margin As Size = Nothing,
                         Optional showGrid As Boolean = True) As Bitmap
        Dim data As New List(Of Double)
        Dim engine As New Expression

        For Each x# In xrange.x.seq(steps)
            Call engine.SetVariable(xrange.Name, x#)
            data += engine.Evaluation(expression$)
        Next

        Return Plot(data, xrange.x, color, bg, size, margin, showGrid)
    End Function

    <Extension>
    Public Function Plot(groups As HistogramGroup,
                         Optional bg$ = "white",
                         Optional size As Size = Nothing,
                         Optional margin As Size = Nothing,
                         Optional showGrid As Boolean = True) As Bitmap

        Return GraphicsPlots(
           size, margin,
           bg$,
           Sub(ByRef g, region)
               Dim mapper As New Scaling(groups)
               Dim annotations = groups.Serials.ToDictionary

               Call g.DrawAxis(size, margin, mapper, showGrid)

               For Each hist In mapper.ForEach_histSample(size, margin)
                   Dim ann As NamedValue(Of Color) =
                       annotations(hist.Name)
                   Dim b As New SolidBrush(ann.x)

                   For Each block As HistogramData In hist.x
                       Dim rect As New Rectangle(
                           New Point(block.x1, block.y),
                           New Size(block.width, block.y - region.GraphicsRegion.Bottom))
                       Call g.FillRectangle(b, rect)
                   Next
               Next
           End Sub)
    End Function

    Public Class HistogramGroup : Inherits ProfileGroup

        Public Property Samples As NamedValue(Of HistogramData())()
    End Class
End Module
