Imports System.Runtime.CompilerServices
Imports number = System.Double

Namespace Imaging.LayoutModel

    Public Module ColaExtensions

        <Extension>
        Public Function lineIntersections(r As Rectangle2D, x1 As number, y1 As number, x2 As number, y2 As number) As List(Of Point2D)
            Dim sides = {
                New Double() {r.X, r.Y, r.Right, r.Y},
                New Double() {r.Right, r.Y, r.Right, r.Bottom},
                New Double() {r.Right, r.Bottom, r.X, r.Bottom},
                New Double() {r.X, r.Bottom, r.X, r.Y}
            }
            Dim intersections As New List(Of Point2D)

            For i As Integer = 0 To 3
                Dim p As Point2D = Rectangle2D.intersection(x1, y1, x2, y2, sides(i)(0), sides(i)(1), sides(i)(2), sides(i)(3))

                If Not p Is Nothing Then
                    Call intersections.Add(p)
                End If
            Next

            Return intersections
        End Function

        ''' <summary>
        ''' return any intersection points between a line extending from the centre of this rectangle to the given point,
        ''' And the sides of this rectangle
        ''' </summary>
        ''' <param name="x2">second x coord of line</param>
        ''' <param name="y2">second y coord of line</param>
        ''' <returns>any intersection points found</returns>
        ''' 
        <Extension>
        Public Function rayIntersection(this As Rectangle2D, x2 As number, y2 As number) As Point2D
            Dim ints = this.lineIntersections(this.CenterX, this.CenterY, x2, y2)

            If ints.Count > 0 Then
                Return ints(Scan0)
            Else
                Return Nothing
            End If
        End Function
    End Module
End Namespace