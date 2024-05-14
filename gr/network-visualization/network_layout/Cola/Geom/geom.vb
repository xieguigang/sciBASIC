#Region "Microsoft.VisualBasic::369bf73a418fe562c3ff6edb1edf132c, gr\network-visualization\network_layout\Cola\Geom\geom.vb"

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

    '   Total Lines: 415
    '    Code Lines: 273
    ' Comment Lines: 113
    '   Blank Lines: 29
    '     File Size: 17.44 KB


    '     Module Extensions
    ' 
    '         Function: above, below, isLeft, Ltangent_PointPolyC, nextPolyPoint
    '                   prevPolyPoint, Rtangent_PointPolyC, tangent_PointPolyC
    ' 
    '         Sub: clockwiseRadialSweep
    '         Delegate Function
    ' 
    ' 
    '         Delegate Function
    ' 
    '             Function: intersects, isAnyPInQ, isPointInsidePoly, LLtangent_PolyPolyC, LRtangent_PolyPolyC
    '                       polysOverlap, RLtangent_PolyPolyC, RRtangent_PolyPolyC, tangent_PolyPolyC, tangents
    ' 
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging.LayoutModel
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports Microsoft.VisualBasic.My.JavaScript
Imports stdNum = System.Math

Namespace Cola

    Partial Module Extensions

        ''' <summary>
        ''' tests if a point is Left|On|Right of an infinite line.
        ''' </summary>
        ''' <param name="P0"></param>
        ''' <param name="P1"></param>
        ''' <param name="P2"></param>
        ''' <returns>
        ''' + ``>0`` for P2 left of the line through P0 and P1
        ''' + ``=0`` for P2 on the line
        ''' + ``&lt;0`` for P2 right of the line
        ''' </returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function isLeft(P0 As Point2D, P1 As Point2D, P2 As Point2D) As Double
            Return (P1.X - P0.X) * (P2.Y - P0.Y) - (P2.X - P0.X) * (P1.Y - P0.Y)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function above(p As Point2D, vi As Point2D, vj As Point2D) As Boolean
            Return isLeft(p, vi, vj) > 0
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function below(p As Point2D, vi As Point2D, vj As Point2D) As Boolean
            Return isLeft(p, vi, vj) < 0
        End Function

        ''' <summary>
        ''' apply f to the points in P in clockwise order around the point p
        ''' </summary>
        ''' <param name="p"></param>
        ''' <param name="pList"></param>
        ''' <param name="force"></param>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Sub clockwiseRadialSweep(p As Point2D, pList As Point2D(), force As Action(Of Point2D))
            Call pList.AsEnumerable _
                .Sort(Function(a, b)
                          Return stdNum.Atan2(a.Y - p.Y, a.X - p.X) - stdNum.Atan2(b.Y - p.Y, b.X - p.X)
                      End Function) _
                .DoEach(force)
        End Sub

        Private Function nextPolyPoint(p As PolyPoint, ps As PolyPoint()) As PolyPoint
            If p.polyIndex = ps.Length - 1 Then
                Return ps(0)
            Else
                Return ps(p.polyIndex + 1)
            End If
        End Function

        Private Function prevPolyPoint(p As PolyPoint, ps As PolyPoint()) As PolyPoint
            If p.polyIndex = 0 Then
                Return ps(ps.Length - 1)
            Else
                Return ps(p.polyIndex - 1)
            End If
        End Function

        ''' <summary>
        ''' fast binary search for tangents to a convex polygon
        ''' </summary>
        ''' <param name="P">a 2D point (exterior to the polygon)</param>
        ''' <param name="V">array of vertices for a 2D convex polygon</param>
        ''' <returns></returns>
        Public Function tangent_PointPolyC(P As Point2D, V As Point2D()) As tangentPoly
            ' Rtangent_PointPolyC and Ltangent_PointPolyC require polygon to be
            ' "closed" with the first vertex duplicated at end, so V[n-1] = V[0].
            Dim Vclosed = V.ToList
            ' Copy V
            Vclosed.Add(V(0))
            ' Add V[0] at end
            Return New tangentPoly() With {
                .rtan = Rtangent_PointPolyC(P, Vclosed),
                .ltan = Ltangent_PointPolyC(P, Vclosed)
            }
        End Function

        ''' <summary>
        ''' binary search for convex polygon right tangent
        ''' </summary>
        ''' <param name="P">a 2D point (exterior to the polygon)</param>
        ''' <param name="V">array of vertices for a 2D convex polygon with first
        ''' vertex duplicated as last, so V[n-1] = V[0]</param>
        ''' <returns>index "i" of rightmost tangent point V[i]</returns>
        Public Function Rtangent_PointPolyC(P As Point2D, V As List(Of Point2D)) As Integer
            Dim n = V.Count - 1

            ' use binary search for large convex polygons
            Dim a As Integer
            Dim b As Integer
            Dim c As Integer
            ' indices for edge chain endpoints
            Dim upA As Boolean, dnC As Boolean
            ' test for up direction of edges a and c
            ' rightmost tangent = maximum for the isLeft() ordering
            ' test if V[0] is a local maximum
            If below(P, V(1), V(0)) AndAlso Not above(P, V(n - 1), V(0)) Then
                Return 0
            End If
            ' V[0] is the maximum tangent point
            a = 0
            b = n
            While True
                ' start chain = [0,n] with V[n]=V[0]
                If b - a = 1 Then
                    If above(P, V(a), V(b)) Then
                        Return a
                    Else
                        Return b
                    End If
                End If

                c = stdNum.Floor((a + b) \ 2)
                ' midpoint of [a,b], and 0<c<n
                dnC = below(P, V(c + 1), V(c))
                If dnC AndAlso Not above(P, V(c - 1), V(c)) Then
                    Return c
                End If
                ' V[c] is the maximum tangent point
                ' no max yet, so continue with the binary search
                ' pick one of the two subchains [a,c] or [c,b]
                upA = above(P, V(a + 1), V(a))
                If upA Then
                    ' edge a points up
                    If dnC Then
                        ' edge c points down
                        b = c
                    Else
                        ' select [a,c]
                        ' edge c points up
                        If above(P, V(a), V(c)) Then
                            ' V[a] above V[c]
                            b = c
                        Else
                            ' select [a,c]
                            ' V[a] below V[c]
                            a = c
                            ' select [c,b]
                        End If
                    End If
                Else
                    ' edge a points down
                    If Not dnC Then
                        ' edge c points up
                        a = c
                    Else
                        ' select [c,b]
                        ' edge c points down
                        If below(P, V(a), V(c)) Then
                            ' V[a] below V[c]
                            b = c
                        Else
                            ' select [a,c]
                            ' V[a] above V[c]
                            a = c
                            ' select [c,b]
                        End If
                    End If
                End If
            End While

            Throw New Exception("Never happends")
        End Function

        ' Ltangent_PointPolyC(): binary search for convex polygon left tangent
        '    Input:  P = a 2D point (exterior to the polygon)
        '            n = number of polygon vertices
        '            V = array of vertices for a 2D convex polygon with first
        '                vertex duplicated as last, so V[n-1] = V[0]
        '    Return: index "i" of leftmost tangent point V[i]
        Public Function Ltangent_PointPolyC(P As Point2D, V As List(Of Point2D)) As Integer
            Dim n As Integer = V.Count - 1
            ' use binary search for large convex polygons
            Dim a As Integer
            Dim b As Integer
            Dim c As Integer
            ' indices for edge chain endpoints
            Dim dnA As Boolean, dnC As Boolean
            ' test for down direction of edges a and c
            ' leftmost tangent = minimum for the isLeft() ordering
            ' test if V[0] is a local minimum
            If above(P, V(n - 1), V(0)) AndAlso Not below(P, V(1), V(0)) Then
                Return 0
            End If
            ' V[0] is the minimum tangent point
            a = 0
            b = n
            While True
                ' start chain = [0,n] with V[n] = V[0]
                If b - a = 1 Then
                    If below(P, V(a), V(b)) Then
                        Return a
                    Else
                        Return b
                    End If
                End If

                c = stdNum.Floor((a + b) \ 2)
                ' midpoint of [a,b], and 0<c<n
                dnC = below(P, V(c + 1), V(c))
                If above(P, V(c - 1), V(c)) AndAlso Not dnC Then
                    Return c
                End If
                ' V[c] is the minimum tangent point
                ' no min yet, so continue with the binary search
                ' pick one of the two subchains [a,c] or [c,b]
                dnA = below(P, V(a + 1), V(a))
                If dnA Then
                    ' edge a points down
                    If Not dnC Then
                        ' edge c points up
                        b = c
                    Else
                        ' select [a,c]
                        ' edge c points down
                        If below(P, V(a), V(c)) Then
                            ' V[a] below V[c]
                            b = c
                        Else
                            ' select [a,c]
                            ' V[a] above V[c]
                            a = c
                            ' select [c,b]
                        End If
                    End If
                Else
                    ' edge a points up
                    If dnC Then
                        ' edge c points down
                        a = c
                    Else
                        ' select [c,b]
                        ' edge c points up
                        If above(P, V(a), V(c)) Then
                            ' V[a] above V[c]
                            b = c
                        Else
                            ' select [a,c]
                            ' V[a] below V[c]
                            a = c
                            ' select [c,b]
                        End If
                    End If
                End If
            End While

            Throw New Exception("Never happends")
        End Function

        Public Delegate Function ComparePoints(a As Point2D, b As Point2D, c As Point2D) As Boolean
        Public Delegate Function PointPolyC(p As Point2D, points As List(Of Point2D)) As Integer

        ' RLtangent_PolyPolyC(): get the RL tangent between two convex polygons
        '    Input:  m = number of vertices in polygon 1
        '            V = array of vertices for convex polygon 1 with V[m]=V[0]
        '            n = number of vertices in polygon 2
        '            W = array of vertices for convex polygon 2 with W[n]=W[0]
        '    Output: *t1 = index of tangent point V[t1] for polygon 1
        '            *t2 = index of tangent point W[t2] for polygon 2
        Public Function tangent_PolyPolyC(V As List(Of Point2D), W As List(Of Point2D), t1 As PointPolyC, t2 As PointPolyC, cmp1 As ComparePoints, cmp2 As ComparePoints) As BiTangent
            Dim ix1 As Integer, ix2 As Integer
            ' search indices for polygons 1 and 2
            ' first get the initial vertex on each polygon
            ix1 = t1(W(0), V)
            ' right tangent from W[0] to V
            ix2 = t2(V(ix1), W)
            ' left tangent from V[ix1] to W
            ' ping-pong linear search until it stabilizes
            Dim done = False
            ' flag when done
            While Not done
                done = True
                ' assume done until...
                While True
                    If ix1 = V.Count - 1 Then
                        ix1 = 0
                    End If
                    If cmp1(W(ix2), V(ix1), V(ix1 + 1)) Then
                        Exit While
                    End If
                    ' get Rtangent from W[ix2] to V
                    ix1 += 1
                End While
                While True
                    If ix2 = 0 Then
                        ix2 = W.Count - 1
                    End If
                    If cmp2(V(ix1), W(ix2), W(ix2 - 1)) Then
                        Exit While
                    End If
                    ix2 -= 1
                    ' get Ltangent from V[ix1] to W
                    ' not done if had to adjust this
                    done = False
                End While
            End While
            Return New BiTangent() With {
            .t1 = ix1,
           .t2 = ix2
        }
        End Function

        Public Function LRtangent_PolyPolyC(V As List(Of Point2D), W As List(Of Point2D)) As BiTangent
            Dim rl = RLtangent_PolyPolyC(W, V)

            Return New BiTangent() With {
                .t1 = rl.t2,
                .t2 = rl.t1
            }
        End Function

        Private Function RLtangent_PolyPolyC(V As List(Of Point2D), W As List(Of Point2D)) As BiTangent
            Return tangent_PolyPolyC(V, W, AddressOf Rtangent_PointPolyC, AddressOf Ltangent_PointPolyC, AddressOf above, AddressOf below)
        End Function

        Private Function LLtangent_PolyPolyC(V As List(Of Point2D), W As List(Of Point2D)) As BiTangent
            Return tangent_PolyPolyC(V, W, AddressOf Ltangent_PointPolyC, AddressOf Ltangent_PointPolyC, AddressOf below, AddressOf below)
        End Function

        Private Function RRtangent_PolyPolyC(V As List(Of Point2D), W As List(Of Point2D)) As BiTangent
            Return tangent_PolyPolyC(V, W, AddressOf Rtangent_PointPolyC, AddressOf Rtangent_PointPolyC, AddressOf above, AddressOf above)
        End Function

        Public Function intersects(l As Line, P As Point2D()) As Point2D()
            Dim ints As New List(Of Point2D)
            Dim i As Integer = 1, n As Integer = P.Length
            While i < n
                Dim int32 = Rectangle2D.intersection(l.X1, l.Y1, l.X2, l.Y2, P(i - 1).X, P(i - 1).Y, P(i).X, P(i).Y)
                If Not int32 Is Nothing Then
                    ints.Add(int32)
                End If
                i += 1
            End While
            Return ints.ToArray
        End Function

        Public Function tangents(V As Point2D(), W As Point2D()) As BiTangents
            Dim m = V.Length - 1
            Dim n = W.Length - 1
            Dim bt = New BiTangents()

            For i As Integer = 0 To m - 1
                For j As Integer = 0 To n - 1
                    Dim v1 = V(If(i = 0, m - 1, i - 1))
                    Dim v2 = V(i)
                    Dim v3 = V(i + 1)
                    Dim w1 = W(If(j = 0, n - 1, j - 1))
                    Dim w2 = W(j)
                    Dim w3 = W(j + 1)
                    Dim v1v2w2 = isLeft(v1, v2, w2)
                    Dim v2w1w2 = isLeft(v2, w1, w2)
                    Dim v2w2w3 = isLeft(v2, w2, w3)
                    Dim w1w2v2 = isLeft(w1, w2, v2)
                    Dim w2v1v2 = isLeft(w2, v1, v2)
                    Dim w2v2v3 = isLeft(w2, v2, v3)
                    If v1v2w2 >= 0 AndAlso v2w1w2 >= 0 AndAlso v2w2w3 < 0 AndAlso w1w2v2 >= 0 AndAlso w2v1v2 >= 0 AndAlso w2v2v3 < 0 Then
                        bt.ll = New BiTangent(i, j)
                    ElseIf v1v2w2 <= 0 AndAlso v2w1w2 <= 0 AndAlso v2w2w3 > 0 AndAlso w1w2v2 <= 0 AndAlso w2v1v2 <= 0 AndAlso w2v2v3 > 0 Then
                        bt.rr = New BiTangent(i, j)
                    ElseIf v1v2w2 <= 0 AndAlso v2w1w2 > 0 AndAlso v2w2w3 <= 0 AndAlso w1w2v2 >= 0 AndAlso w2v1v2 < 0 AndAlso w2v2v3 >= 0 Then
                        bt.rl = New BiTangent(i, j)
                    ElseIf v1v2w2 >= 0 AndAlso v2w1w2 < 0 AndAlso v2w2w3 >= 0 AndAlso w1w2v2 <= 0 AndAlso w2v1v2 > 0 AndAlso w2v2v3 <= 0 Then
                        bt.lr = New BiTangent(i, j)
                    End If
                Next
            Next
            Return bt
        End Function

        Private Function isPointInsidePoly(p As Point2D, poly As Point2D()) As [Boolean]
            Dim i As Integer = 1, n As Integer = poly.Length
            While i < n
                If below(poly(i - 1), poly(i), p) Then
                    Return False
                End If
                i += 1
            End While
            Return True
        End Function

        Private Function isAnyPInQ(p As Point2D(), q As Point2D()) As Boolean
            Return Not p.All(Function(v) Not isPointInsidePoly(v, q))
        End Function

        Private Function polysOverlap(p As Point2D(), q As Point2D()) As Boolean
            If isAnyPInQ(p, q) Then
                Return True
            End If
            If isAnyPInQ(q, p) Then
                Return True
            End If
            Dim i As Integer = 1, n As Integer = p.Length
            While i < n
                Dim v = p(i)
                Dim u = p(i - 1)
                If intersects(New Line(u.X, u.Y, v.X, v.Y), q).Length > 0 Then
                    Return True
                End If
                i += 1
            End While
            Return False
        End Function
    End Module
End Namespace
