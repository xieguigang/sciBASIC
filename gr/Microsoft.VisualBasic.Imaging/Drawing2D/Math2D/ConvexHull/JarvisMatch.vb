#Region "Microsoft.VisualBasic::09f01c570b19bc67238aa1e6124b87b6, ..\sciBASIC#\gr\Microsoft.VisualBasic.Imaging\Drawing2D\Math2D\ConvexHull\JarvisMatch.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Language

Namespace Drawing2D.Math2D.ConvexHull

    Public Module JarvisMatch

        Private Function dist(p As Point, q As Point) As Integer
            Dim dx As Integer = q.X - p.X
            Dim dy As Integer = q.Y - p.Y
            Return dx * dx + dy * dy
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

