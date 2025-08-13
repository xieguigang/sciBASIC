Imports std = System.Math

Namespace Drawing2D.Math2D.DelaunayVoronoi
    Public Class Polygon

        Private vertices As List(Of Vector2)

        Public Sub New(vertices As List(Of Vector2))
            Me.vertices = vertices
        End Sub

        Public Function Area() As Single
            Return std.Abs(SignedDoubleArea() * 0.5F)
        End Function

        Public Function PolyWinding() As Winding
            Dim signedDoubleArea As Single = Me.SignedDoubleArea()
            If signedDoubleArea < 0 Then
                Return Winding.CLOCKWISE
            End If
            If signedDoubleArea > 0 Then
                Return Winding.COUNTERCLOCKWISE
            End If
            Return Winding.NONE
        End Function

        Private Function SignedDoubleArea() As Single
            Dim index, nextIndex As Integer
            Dim n = vertices.Count
            Dim point, [next] As Vector2
            Dim lSignedDoubleArea As Single = 0

            For index = 0 To n - 1
                nextIndex = (index + 1) Mod n
                point = vertices(index)
                [next] = vertices(nextIndex)
                lSignedDoubleArea += point.X * [next].Y - [next].X * point.Y
            Next

            Return lSignedDoubleArea
        End Function
    End Class
End Namespace
