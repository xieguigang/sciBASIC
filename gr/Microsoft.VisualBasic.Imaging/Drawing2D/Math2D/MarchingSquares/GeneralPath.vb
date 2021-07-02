Imports System.Drawing
Imports Microsoft.VisualBasic.Language

Namespace Drawing2D.Math2D.MarchingSquares

    Public Class GeneralPath

        Dim polygons As New List(Of PointF())
        Dim temp As New List(Of PointF)

        Public ReadOnly Property level As Double
        Public Property dimension As Size

        Sub New(level As Double)
            Me.level = level
        End Sub

        Sub New(contour As ContourLayer)
            level = contour.threshold
            polygons = contour.shapes _
                .Select(Function(p) p.ToArray) _
                .AsList
        End Sub

        Public Function GetContour() As ContourLayer
            Return New ContourLayer With {
                .threshold = level,
                .shapes = polygons _
                    .Select(Function(list)
                                Return New Polygon2D With {
                                    .x = list.Select(Function(p) CInt(p.X)).ToArray,
                                    .y = list.Select(Function(p) CInt(p.Y)).ToArray
                                }
                            End Function) _
                    .ToArray
            }
        End Function

        Public Overrides Function ToString() As String
            Return $"{polygons.Count} polygons under threshold {level}"
        End Function

        Friend Sub MoveTo(x As Double, y As Double)
            temp.Add(New PointF(x, y))
        End Sub

        Friend Sub LineTo(x As Double, y As Double)
            temp.Add(New PointF(x, y))
        End Sub

        Public Iterator Function GetPolygons(Optional scaleX As d3js.scale.LinearScale = Nothing, Optional scaleY As d3js.scale.LinearScale = Nothing) As IEnumerable(Of PointF())
            If scaleX Is Nothing OrElse scaleY Is Nothing Then
                For Each raw In polygons
                    Yield raw
                Next
            Else
                For Each raw In polygons
                    Yield raw.Select(Function(p) New PointF(scaleX(p.X), scaleY(p.Y))).ToArray
                Next
            End If
        End Function

        Public Sub Fill(canvas As IGraphics, color As Brush, scaleX As d3js.scale.LinearScale, scaleY As d3js.scale.LinearScale)
            For Each polygon In GetPolygons(scaleX, scaleY)
                Call canvas.FillPolygon(color, polygon)
            Next
        End Sub

        Public Sub Draw(canvas As IGraphics, border As Pen, scaleX As d3js.scale.LinearScale, scaleY As d3js.scale.LinearScale)
            For Each polygon In GetPolygons(scaleX, scaleY)
                Call canvas.DrawPolygon(border, polygon)
            Next
        End Sub

        Friend Sub ClosePath()
            polygons.Add(temp.PopAll)
        End Sub
    End Class
End Namespace