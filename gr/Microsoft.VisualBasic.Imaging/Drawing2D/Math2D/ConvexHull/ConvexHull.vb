Imports System.Drawing

Namespace Drawing2D.Math2D.ConvexHull

    ''' <summary>
    ''' In mathematics, the convex hull or convex envelope of a set X of points in the Euclidean plane 
    ''' or in a Euclidean space (or, more generally, in an affine space over the reals) is the smallest 
    ''' convex set that contains X. For instance, when X is a bounded subset of the plane, the convex 
    ''' hull may be visualized as the shape enclosed by a rubber band stretched around X.
    ''' </summary>
    Public Module ConvexHull

        Public Const TURN__LEFT% = 1
        Public Const TURN_RIGHT% = -1
        Public Const TURN_NONE = 0

        Friend Function turn(p As Point, q As Point, r As Point) As Integer
            Return ((q.X - p.X) * (r.Y - p.Y()) - (r.X - p.X) * (q.Y - p.Y)).CompareTo(0)
        End Function

        Public Function JarvisMatch(points As IEnumerable(Of Point)) As Point()
            Return Math2D.ConvexHull.JarvisMatch.ConvexHull(points)
        End Function

        Public Function GrahamScan(points As IEnumerable(Of Point)) As Point()
            Return Math2D.ConvexHull.GrahamScan.ConvexHull(points)
        End Function
    End Module
End Namespace