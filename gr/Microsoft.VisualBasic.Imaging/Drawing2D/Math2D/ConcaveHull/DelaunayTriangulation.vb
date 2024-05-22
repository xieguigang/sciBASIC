#Region "Microsoft.VisualBasic::012bf3d247d611f2f7a4b624bb6ba40b, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Math2D\ConcaveHull\DelaunayTriangulation.vb"

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

    '   Total Lines: 267
    '    Code Lines: 229 (85.77%)
    ' Comment Lines: 14 (5.24%)
    '    - Xml Docs: 85.71%
    ' 
    '   Blank Lines: 24 (8.99%)
    '     File Size: 11.67 KB


    '     Class DelaunayTriangulation
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Diameter, InCircle, MaxEdge, Triangulate, WhichSide
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Math
Imports Vertex = Microsoft.VisualBasic.Imaging.Drawing3D.Point3D

Namespace Drawing2D.Math2D.ConcaveHull

    ''' <summary>
    ''' http://www.tuicool.com/articles/iUvMjm
    ''' </summary>
    Public Class DelaunayTriangulation

        Dim Vertex As Vertex(), Triangle As TriangleIndex()
        Dim maxTriangles%

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="vertex">a set of [x,y,z] 3d point</param>
        ''' <param name="MaxTriangles">max number of the triangles</param>
        Sub New(vertex As IEnumerable(Of Vertex), Optional MaxTriangles% = 1000)
            Me.Vertex = vertex.ToArray
            Me.Triangle = New TriangleIndex(MaxTriangles - 1) {}
            Me.maxTriangles = MaxTriangles
        End Sub

        Private Function InCircle(xp As Long,
                                  yp As Long,
                                  x1 As Long,
                                  y1 As Long,
                                  x2 As Long,
                                  y2 As Long,
                                  x3 As Long,
                                  y3 As Long,
                                  xc As Double,
                                  yc As Double,
                                  r As Double) As Boolean
            Dim eps As Double
            Dim m1 As Double
            Dim m2 As Double
            Dim mx1 As Double
            Dim mx2 As Double
            Dim my1 As Double
            Dim my2 As Double
            Dim dx As Double
            Dim dy As Double
            Dim rsqr As Double
            Dim drsqr As Double

            eps = 0.000000001

            If Abs(y1 - y2) < eps AndAlso Abs(y2 - y3) < eps Then
                Return False
            End If

            If Abs(y2 - y1) < eps Then
                m2 = (-(Convert.ToDouble(x3) - Convert.ToDouble(x2)) / (Convert.ToDouble(y3) - Convert.ToDouble(y2)))
                mx2 = Convert.ToDouble((x2 + x3) / 2.0)
                my2 = Convert.ToDouble((y2 + y3) / 2.0)
                xc = Convert.ToDouble((x2 + x1) / 2.0)
                yc = Convert.ToDouble(m2 * (xc - mx2) + my2)
            ElseIf Abs(y3 - y2) < eps Then
                m1 = (-(Convert.ToDouble(x2) - Convert.ToDouble(x1)) / (Convert.ToDouble(y2) - Convert.ToDouble(y1)))
                mx1 = Convert.ToDouble((x1 + x2) / 2.0)
                my1 = Convert.ToDouble((y1 + y2) / 2.0)
                xc = Convert.ToDouble((x3 + x2) / 2.0)
                yc = Convert.ToDouble(m1 * (xc - mx1) + my1)
            Else
                m1 = (-(Convert.ToDouble(x2) - Convert.ToDouble(x1)) / (Convert.ToDouble(y2) - Convert.ToDouble(y1)))
                m2 = (-(Convert.ToDouble(x3) - Convert.ToDouble(x2)) / (Convert.ToDouble(y3) - Convert.ToDouble(y2)))
                mx1 = Convert.ToDouble((x1 + x2) / 2.0)
                mx2 = Convert.ToDouble((x2 + x3) / 2.0)
                my1 = Convert.ToDouble((y1 + y2) / 2.0)
                my2 = Convert.ToDouble((y2 + y3) / 2.0)
                xc = Convert.ToDouble((m1 * mx1 - m2 * mx2 + my2 - my1) / (m1 - m2))
                yc = Convert.ToDouble(m1 * (xc - mx1) + my1)
            End If

            dx = (Convert.ToDouble(x2) - Convert.ToDouble(xc))
            dy = (Convert.ToDouble(y2) - Convert.ToDouble(yc))
            rsqr = Convert.ToDouble(dx * dx + dy * dy)
            r = Convert.ToDouble(Sqrt(rsqr))
            dx = Convert.ToDouble(xp - xc)
            dy = Convert.ToDouble(yp - yc)
            drsqr = Convert.ToDouble(dx * dx + dy * dy)

            If drsqr <= rsqr Then
                Return True
            End If

            Return False
        End Function

        Private Function WhichSide(xp As Long, yp As Long, x1 As Long, y1 As Long, x2 As Long, y2 As Long) As Integer
            Dim equation# = ((Convert.ToDouble(yp) - Convert.ToDouble(y1)) * (Convert.ToDouble(x2) - Convert.ToDouble(x1))) - ((Convert.ToDouble(y2) - Convert.ToDouble(y1)) * (Convert.ToDouble(xp) - Convert.ToDouble(x1)))

            If equation > 0 Then
                'WhichSide = -1;
                Return -1
            ElseIf equation = 0 Then
                Return 0
            Else
                Return 1
            End If
        End Function

        ''' <summary>
        ''' <paramref name="nvert"/>值必须要小于<see cref="Vertex"/>的数量
        ''' </summary>
        ''' <param name="nvert"></param>
        ''' <returns></returns>
        Public Function Triangulate(nvert As Integer) As Integer
            Dim Complete As Boolean() = New Boolean(maxTriangles - 1) {}
            Dim Edges As Long(,) = New Long(2, maxTriangles * 3) {}
            Dim Nedge As Long
            Dim xmin As Long
            Dim xmax As Long
            Dim ymin As Long
            Dim ymax As Long
            Dim xmid As Long
            Dim ymid As Long
            Dim dx As Double
            Dim dy As Double
            Dim dmax As Double
            Dim i As Integer
            Dim j As Integer
            Dim k As Integer
            Dim ntri As Integer
            Dim xc As Double = 0.0
            Dim yc As Double = 0.0
            Dim r As Double = 0.0
            Dim inc As Boolean

            xmin = Vertex(1).X
            ymin = Vertex(1).Y
            xmax = xmin
            ymax = ymin

            For i = 2 To nvert
                If Vertex(i).X < xmin Then
                    xmin = Vertex(i).X
                End If
                If Vertex(i).X > xmax Then
                    xmax = Vertex(i).X
                End If
                If Vertex(i).Y < ymin Then
                    ymin = Vertex(i).Y
                End If
                If Vertex(i).Y > ymax Then
                    ymax = Vertex(i).Y
                End If
            Next

            dx = Convert.ToDouble(xmax) - Convert.ToDouble(xmin)
            dy = Convert.ToDouble(ymax) - Convert.ToDouble(ymin)

            If dx > dy Then
                dmax = dx
            Else
                dmax = dy
            End If

            xmid = (xmax + xmin) \ 2
            ymid = (ymax + ymin) \ 2
            Vertex(nvert + 1).X = Convert.ToInt64(xmid - 2 * dmax)
            Vertex(nvert + 1).Y = Convert.ToInt64(ymid - dmax)
            Vertex(nvert + 2).X = xmid
            Vertex(nvert + 2).Y = Convert.ToInt64(ymid + 2 * dmax)
            Vertex(nvert + 3).X = Convert.ToInt64(xmid + 2 * dmax)
            Vertex(nvert + 3).Y = Convert.ToInt64(ymid - dmax)
            Triangle(1).vv0 = nvert + 1
            Triangle(1).vv1 = nvert + 2
            Triangle(1).vv2 = nvert + 3
            Complete(1) = False
            ntri = 1

            For i = 1 To nvert
                Nedge = 0
                j = 0
                Do
                    j = j + 1
                    If Complete(j) <> True Then
                        inc = InCircle(Vertex(i).X, Vertex(i).Y, Vertex(Triangle(j).vv0).X, Vertex(Triangle(j).vv0).Y, Vertex(Triangle(j).vv1).X, Vertex(Triangle(j).vv1).Y,
                            Vertex(Triangle(j).vv2).X, Vertex(Triangle(j).vv2).Y, xc, yc, r)
                        If inc Then
                            Edges(1, Nedge + 1) = Triangle(j).vv0
                            Edges(2, Nedge + 1) = Triangle(j).vv1
                            Edges(1, Nedge + 2) = Triangle(j).vv1
                            Edges(2, Nedge + 2) = Triangle(j).vv2
                            Edges(1, Nedge + 3) = Triangle(j).vv2
                            Edges(2, Nedge + 3) = Triangle(j).vv0
                            Nedge = Nedge + 3
                            Triangle(j).vv0 = Triangle(ntri).vv0
                            Triangle(j).vv1 = Triangle(ntri).vv1
                            Triangle(j).vv2 = Triangle(ntri).vv2
                            Complete(j) = Complete(ntri)
                            j = j - 1
                            ntri = ntri - 1
                        End If
                    End If
                Loop While j < ntri
                For j = 1 To Nedge - 1
                    If Edges(1, j) <> 0 AndAlso Edges(2, j) <> 0 Then
                        For k = j + 1 To Nedge
                            If Edges(1, k) <> 0 AndAlso Edges(2, k) <> 0 Then
                                If Edges(1, j) = Edges(2, k) Then
                                    If Edges(2, j) = Edges(1, k) Then
                                        Edges(1, j) = 0
                                        Edges(2, j) = 0
                                        Edges(1, k) = 0
                                        Edges(2, k) = 0
                                    End If
                                End If
                            End If
                        Next
                    End If
                Next
                For j = 1 To Nedge
                    If Edges(1, j) <> 0 AndAlso Edges(2, j) <> 0 Then
                        ntri = ntri + 1
                        Triangle(ntri).vv0 = Edges(1, j)
                        Triangle(ntri).vv1 = Edges(2, j)
                        Triangle(ntri).vv2 = i
                        Complete(ntri) = False
                    End If
                Next
            Next
            i = 0
            Do
                i = i + 1
                If Triangle(i).vv0 > nvert OrElse Triangle(i).vv1 > nvert OrElse Triangle(i).vv2 > nvert Then
                    Triangle(i).vv0 = Triangle(ntri).vv0
                    Triangle(i).vv1 = Triangle(ntri).vv1
                    Triangle(i).vv2 = Triangle(ntri).vv2
                    i = i - 1
                    ntri = ntri - 1
                End If
            Loop While i < ntri

            Return ntri
        End Function

        Public Shared Function Diameter(Ax As Double, Ay As Double, Bx As Double, By As Double, Cx As Double, Cy As Double) As Double
            Dim x As Double, y As Double
            Dim a__1 As Double = Ax
            Dim b__2 As Double = Bx
            Dim c As Double = Cx
            Dim m As Double = Ay
            Dim n As Double = By
            Dim k As Double = Cy
            Dim A__3 As Double = a__1 * b__2 * b__2 + a__1 * n * n + a__1 * a__1 * c - b__2 * b__2 * c + m * m * c - n * n * c - a__1 * c * c - a__1 * k * k - a__1 * a__1 * b__2 + b__2 * c * c - m * m * b__2 + b__2 * k * k
            Dim B__4 As Double = a__1 * n + m * c + k * b__2 - n * c - a__1 * k - b__2 * m
            y = A__3 / B__4 / 2
            Dim AA As Double = b__2 * b__2 * m + m * n * n + a__1 * a__1 * k - b__2 * b__2 * k + m * m * k - n * n * k - c * c * m - m * k * k - a__1 * a__1 * n + c * c * n - m * m * n + k * k * n
            Dim BB As Double = b__2 * m + a__1 * k + c * n - b__2 * k - c * m - a__1 * n
            x = AA / BB / 2
            Return Sqrt((Ax - x) * (Ax - x) + (Ay - y) * (Ay - y))
        End Function

        Public Shared Function MaxEdge(Ax As Double, Ay As Double, Bx As Double, By As Double, Cx As Double, Cy As Double) As Double
            Dim len1 As Double = Sqrt((Ax - Bx) * (Ax - Bx) + (Ay - By) * (Ay - By))
            Dim len2 As Double = Sqrt((Cx - Bx) * (Cx - Bx) + (Cy - By) * (Cy - By))
            Dim len3 As Double = Sqrt((Ax - Cx) * (Ax - Cx) + (Ay - Cy) * (Ay - Cy))
            Dim len As Double = If(len1 > len2, len1, len2)
            Return If(len > len3, len, len3)
        End Function
    End Class

End Namespace
