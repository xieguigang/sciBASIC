#Region "Microsoft.VisualBasic::3396794c52a4e0e8eb8e69381b9bcb53, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Math2D\ConvexHull\ConvexHull.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Module ConvexHull
    ' 
    '         Function: GrahamScan, JarvisMatch, turn
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices

Namespace Drawing2D.Math2D.ConvexHull

    ''' <summary>
    ''' In mathematics, the convex hull or convex envelope of a set X of points in the Euclidean plane 
    ''' or in a Euclidean space (or, more generally, in an affine space over the reals) is the smallest 
    ''' convex set that contains X. For instance, when X is a bounded subset of the plane, the convex 
    ''' hull may be visualized as the shape enclosed by a rubber band stretched around X.
    ''' (多边形的点的数量必须要至少3个点)
    ''' </summary>
    Public Module ConvexHull

        Public Const TURN__LEFT% = 1
        Public Const TURN_RIGHT% = -1
        Public Const TURN_NONE = 0

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Friend Function turn(p As PointF, q As PointF, r As PointF) As Integer
            Return ((q.X - p.X) * (r.Y - p.Y()) - (r.X - p.X) * (q.Y - p.Y)).CompareTo(0)
        End Function

        ''' <summary>
        ''' 点的数量必须要多于2个!
        ''' </summary>
        ''' <param name="points"></param>
        ''' <returns></returns>
        ''' 
        <Extension>
        Public Function JarvisMatch(points As IEnumerable(Of PointF)) As PointF()
            Return Math2D.ConvexHull.JarvisMatch.ConvexHull(points)
        End Function

        ''' <summary>
        ''' 点的数量必须要多于2个!
        ''' </summary>
        ''' <param name="points"></param>
        ''' <returns></returns>
        ''' 
        <Extension>
        Public Function GrahamScan(points As IEnumerable(Of PointF)) As PointF()
            Return Math2D.ConvexHull.GrahamScan.ConvexHull(points)
        End Function
    End Module
End Namespace
