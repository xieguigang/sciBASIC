Imports Microsoft.VisualBasic.Imaging.LayoutModel
Imports Microsoft.VisualBasic.Language
Imports number = System.Double

Namespace Layouts.Cola

    Public Module Extensions

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
    End Module
End Namespace