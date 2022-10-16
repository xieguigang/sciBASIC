Imports System.Drawing
Imports stdNum = System.Math

Namespace Imaging.Math2D

    ''' <summary>
    ''' Ellipse shape polygon generator
    ''' </summary>
    Public Class EllipseShape

        ReadOnly radiusX As Double
        ReadOnly radiusY As Double
        ReadOnly center As PointF

        Sub New(radiusX As Double, radiusY As Double, center As PointF)
            Me.center = center
            Me.radiusX = radiusX
            Me.radiusY = radiusY
        End Sub

        Public Function GetPolygonPath() As Polygon2D
            Dim path As New List(Of PointF)

            For angle As Integer = 0 To 360
                Call path.Add(EllipseDrawing(radiusX, radiusY, center, angle))
            Next

            Return New Polygon2D(path.ToArray)
        End Function

        Private Shared Function EllipseDrawing(dHalfwidthEllipse As Double, dHalfheightEllipse As Double, origin As PointF, t As Integer) As PointF
            Return New PointF(
                origin.X + dHalfwidthEllipse * stdNum.Cos(t * stdNum.PI / 180),
                origin.Y + dHalfheightEllipse * stdNum.Sin(t * stdNum.PI / 180)
            )
        End Function
    End Class
End Namespace