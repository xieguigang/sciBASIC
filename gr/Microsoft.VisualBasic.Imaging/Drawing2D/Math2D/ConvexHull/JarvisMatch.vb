#Region "Microsoft.VisualBasic::34acc545dcc96c0a699d1f62de7a8369, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Math2D\ConvexHull\JarvisMatch.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 67
    '    Code Lines: 52 (77.61%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 15 (22.39%)
    '     File Size: 1.99 KB


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
                    Call hull.Add(q)
                End If

                counter += 1

                If hull.Count / vector.Length > 100 Then
                    Exit While
                End If
            End While

            Return hull
        End Function
    End Module
End Namespace
