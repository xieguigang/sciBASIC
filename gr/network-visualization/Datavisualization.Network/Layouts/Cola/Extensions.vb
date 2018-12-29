Imports Microsoft.VisualBasic.Imaging.LayoutModel
Imports Microsoft.VisualBasic.Language
Imports number = System.Double

Namespace Layouts.Cola

    Public Module Extensions

        ''' <summary>
        ''' Returns the endpoints of a line that connects the centre of two rectangles.
        ''' </summary>
        ''' <param name="source">The source Rectangle.</param>
        ''' <param name="target">The target Rectangle.</param>
        ''' <param name="ah">The size of the arrow head, a distance to shorten the line by.</param>
        ''' <returns></returns>
        Public Function makeEdgeBetween(source As Rectangle2D, target As Rectangle2D, ah As number) As DirectedEdge
            Dim si = source.rayIntersection(target.CenterX, target.CenterY) Or New Point2D(source.CenterX, source.CenterY).AsDefault
            Dim ti = target.rayIntersection(source.CenterX, source.CenterY) Or New Point2D(target.CenterX, target.CenterY).AsDefault
            Dim dx = ti.X - si.X
            Dim dy = ti.Y - si.Y
            Dim l = Math.Sqrt(dx * dx + dy * dy), al = l - ah

            Return New DirectedEdge With {
                .sourceIntersection = si,
                .targetIntersection = ti,
                .arrowStart = New Point2D With {
                    .X = si.X + al * dx / l,
                    .Y = si.Y + al * dy / l
                }
            }
        End Function

        ''' <summary>
        ''' Returns the intersection of a line from the given point to the centre
        ''' of the target rectangle where it intersects the rectanngle.
        ''' </summary>
        ''' <param name="s">The source point.</param>
        ''' <param name="target">The target Rectangle.</param>
        ''' <param name="ah">The size of the arrow head, a distance to shorten the
        ''' line by.</param>
        ''' <returns>The point an arrow head of the specified size would need to start.</returns>
        Public Function makeEdgeTo(s As Point2D, target As Rectangle2D, ah As number) As Point2D
            Dim ti = target.rayIntersection(s.X, s.Y)

            If (ti Is Nothing) Then
                ti = New Point2D With {
                    .X = target.CenterX,
                    .Y = target.CenterY
                }
            End If

            Dim dx = ti.X - s.X
            Dim dy = ti.Y - s.Y
            Dim l = Math.Sqrt(dx * dx + dy * dy)

            Return New Point2D With {
                .X = ti.X - ah * dx / l,
                .Y = ti.Y - ah * dy / l
            }
        End Function

    End Module
End Namespace