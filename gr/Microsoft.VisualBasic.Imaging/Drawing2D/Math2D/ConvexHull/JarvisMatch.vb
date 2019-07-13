#Region "Microsoft.VisualBasic::e1cd62e2e1647bae0a275a0f4b2034eb, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Math2D\ConvexHull\JarvisMatch.vb"

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

    '     Module JarvisMatch
    ' 
    '         Function: ConvexHull, dist, nextHullPoint
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Point = System.Drawing.PointF

Namespace Drawing2D.Math2D.ConvexHull

    Public Module JarvisMatch

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function dist(p As Point, q As Point) As Integer
            Return (q.X - p.X) ^ 2 + (q.Y - p.Y) ^ 2
        End Function

        Private Function nextHullPoint(points As Point(), p As Point) As Point
            Dim q As Point = p
            Dim t As Integer

            For Each r As Point In points
                t = turn(p, q, r)

                If t = TURN_RIGHT OrElse t = TURN_NONE AndAlso dist(p, r) > dist(p, q) Then
                    q = r
                End If
            Next

            Return q
        End Function

        Public Function ConvexHull(points As IEnumerable(Of Point)) As Point()
            Dim vector = points.ToArray
            Dim hull As New List(Of Point)()

            For Each p As Point In vector
                If hull.Count = 0 Then
                    hull.Add(p)
                Else
                    If hull(0).X > p.X Then
                        hull(0) = p
                    ElseIf hull(0).X = p.X Then
                        If hull(0).Y > p.Y Then
                            hull(0) = p
                        End If
                    End If
                End If
            Next

            Dim q As Point
            Dim counter As Integer = 0

            While counter < hull.Count
                q = nextHullPoint(vector, hull(counter))

                If Not q = hull(0) Then
                    hull.Add(q)
                End If

                counter += 1
            End While

            Return hull
        End Function
    End Module
End Namespace
