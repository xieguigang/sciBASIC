#Region "Microsoft.VisualBasic::dad7f7e1897e689840bcb44ede750a61, Data_science\Graph\EMD\MinCostFlow.vb"

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

    '   Total Lines: 339
    '    Code Lines: 254 (74.93%)
    ' Comment Lines: 27 (7.96%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 58 (17.11%)
    '     File Size: 11.97 KB


    '     Class MinCostFlow
    ' 
    '         Function: compute, LEFT, PARENT, RIGHT
    ' 
    '         Sub: computeShortestPath, heapDecreaseKey, heapify, heapRemoveFirst, swapHeap
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports std = System.Math

Namespace EMD

    Friend Class MinCostFlow

        Friend numNodes As Integer
        Friend nodesToQ As List(Of Integer)

        ' e - supply(positive) and demand(negative).
        ' c[i] - edges that goes from node i. first is the second nod
        ' x - the flow is returned in it
        Friend Overridable Function compute(e As List(Of Long), c As List(Of IList(Of Edge)), x As List(Of IList(Of Edge0))) As Long

            numNodes = e.Count
            nodesToQ = New List(Of Integer)()
            For i = 0 To numNodes - 1
                nodesToQ.Add(0)
            Next

            ' init flow
            Dim from = 0

            While from < numNodes
                For Each it In c(from)
                    x(from).Add(New Edge0(it._to, it._cost, 0))
                    x(it._to).Add(New Edge0(from, -it._cost, 0))
                Next

                from += 1
            End While

            ' reduced costs for forward edges (c[i,j]-pi[i]+pi[j])
            ' Note that for forward edges the residual capacity is infinity
            Dim rCostForward As List(Of IList(Of Edge1)) = New List(Of IList(Of Edge1))()
            For i = 0 To numNodes - 1
                rCostForward.Add(New List(Of Edge1)())
            Next
            from = 0

            While from < numNodes
                For Each it In c(from)
                    rCostForward(from).Add(New Edge1(it._to, it._cost))
                Next

                from += 1
            End While

            ' reduced costs and capacity for backward edges
            ' (c[j,i]-pi[j]+pi[i])
            ' Since the flow at the beginning is 0, the residual capacity is
            ' also zero
            Dim rCostCapBackward As List(Of IList(Of Edge2)) = New List(Of IList(Of Edge2))()
            For i = 0 To numNodes - 1
                rCostCapBackward.Add(New List(Of Edge2)())
            Next
            from = 0

            While from < numNodes
                For Each it In c(from)
                    rCostCapBackward(it._to).Add(New Edge2(from, -it._cost, 0))
                Next

                from += 1
            End While

            ' Max supply TODO:demand?, given U?, optimization-> min out of
            ' demand,supply
            Dim U As Long = 0
            For i = 0 To numNodes - 1
                If e(i) > U Then
                    U = e(i)
                End If
            Next
            Dim delta As Long = std.Pow(2.0, std.Ceiling(std.Log(U) / std.Log(2.0)))

            Dim d As List(Of Long) = New List(Of Long)()
            Dim prev As List(Of Integer) = New List(Of Integer)()
            For i = 0 To numNodes - 1
                d.Add(0L)
                prev.Add(0)
            Next
            delta = 1
            While True ' until we break when S or T is empty
                Dim maxSupply As Long = 0
                Dim k = 0
                For i = 0 To numNodes - 1
                    If e(i) > 0 Then
                        If maxSupply < e(i) Then
                            maxSupply = e(i)
                            k = i
                        End If
                    End If
                Next
                If maxSupply = 0 Then
                    Exit While
                End If
                delta = maxSupply

                Dim l = New Integer(0) {}
                computeShortestPath(d, prev, k, rCostForward, rCostCapBackward, e, l)

                ' find delta (minimum on the path from k to l)
                ' delta= e[k];
                ' if (-e[l]<delta) delta= e[k];
                Dim [to] = l(0)
                Do
                    from = prev([to])

                    ' residual
                    Dim itccb = 0
                    While itccb < rCostCapBackward(from).Count AndAlso rCostCapBackward(from)(itccb)._to <> [to]
                        itccb += 1
                    End While
                    If itccb < rCostCapBackward(from).Count Then
                        If rCostCapBackward(from)(itccb)._residual_capacity < delta Then
                            delta = rCostCapBackward(from)(itccb)._residual_capacity
                        End If
                    End If

                    [to] = from
                Loop While [to] <> k

                ' augment delta flow from k to l (backwards actually...)
                [to] = l(0)
                Do
                    from = prev([to])


                    ' TODO - might do here O(n) can be done in O(1)
                    Dim itx = 0
                    While x(from)(itx)._to <> [to]
                        itx += 1
                    End While
                    x(from)(itx)._flow += delta

                    ' update residual for backward edges
                    Dim itccb = 0
                    While itccb < rCostCapBackward([to]).Count AndAlso rCostCapBackward([to])(itccb)._to <> from
                        itccb += 1
                    End While
                    If itccb < rCostCapBackward([to]).Count Then
                        rCostCapBackward([to])(itccb)._residual_capacity += delta
                    End If
                    itccb = 0
                    While itccb < rCostCapBackward(from).Count AndAlso rCostCapBackward(from)(itccb)._to <> [to]
                        itccb += 1
                    End While
                    If itccb < rCostCapBackward(from).Count Then
                        rCostCapBackward(from)(itccb)._residual_capacity -= delta
                    End If

                    ' update e
                    e([to]) = e([to]) + delta
                    e(from) = e(from) - delta

                    [to] = from
                Loop While [to] <> k
            End While

            ' compute distance from x
            Dim dist As Long = 0
            For from = 0 To numNodes - 1
                For Each it In x(from)
                    dist += it._cost * it._flow
                Next
            Next
            Return dist
        End Function

        Friend Overridable Sub computeShortestPath(d As List(Of Long), prev As List(Of Integer), from As Integer, costForward As List(Of IList(Of Edge1)), costBackward As List(Of IList(Of Edge2)), e As List(Of Long), l As Integer())
            ' Making heap (all inf except 0, so we are saving comparisons...)
            Dim Q As List(Of Edge3) = New List(Of Edge3)()
            Dim i = 0
            Dim j = 1

            For i = 0 To numNodes - 1
                Q.Add(New Edge3())
            Next

            Q(0)._to = from
            nodesToQ(from) = 0
            Q(0)._dist = 0

            j = 1
            ' TODO: both of these into a function?
            i = 0

            While i < from
                Q(j)._to = i
                nodesToQ(i) = j
                Q(j)._dist = Long.MaxValue
                j += 1
                i += 1
            End While

            For i = from + 1 To numNodes - 1
                Q(j)._to = i
                nodesToQ(i) = j
                Q(j)._dist = Long.MaxValue
                j += 1
            Next

            Dim finalNodesFlg As List(Of Boolean) = New List(Of Boolean)()
            For i = 0 To numNodes - 1
                finalNodesFlg.Add(False)
            Next
            Do
                Dim u = Q(0)._to

                d(u) = Q(0)._dist ' final distance
                finalNodesFlg(u) = True
                If e(u) < 0 Then
                    l(0) = u
                    Exit Do
                End If

                heapRemoveFirst(Q, nodesToQ)

                ' neighbors of u
                For Each it In costForward(u)

                    Dim alt = d(u) + it._reduced_cost
                    Dim v = it._to
                    If nodesToQ(v) < Q.Count AndAlso alt < Q(nodesToQ(v))._dist Then
                        heapDecreaseKey(Q, nodesToQ, v, alt)
                        prev(v) = u
                    End If
                Next
                For Each it In costBackward(u)
                    If it._residual_capacity > 0 Then

                        Dim alt = d(u) + it._reduced_cost
                        Dim v = it._to
                        If nodesToQ(v) < Q.Count AndAlso alt < Q(nodesToQ(v))._dist Then
                            heapDecreaseKey(Q, nodesToQ, v, alt)
                            prev(v) = u
                        End If
                    End If
                Next

            Loop While Q.Count > 0

            Dim _from = 0

            While _from < numNodes
                For Each it In costForward(_from)
                    If finalNodesFlg(_from) Then
                        it._reduced_cost += d(_from) - d(l(0))
                    End If
                    If finalNodesFlg(it._to) Then
                        it._reduced_cost -= d(it._to) - d(l(0))
                    End If
                Next

                _from += 1
            End While

            ' reduced costs and capacity for backward edges
            ' (c[j,i]-pi[j]+pi[i])
            _from = 0

            While _from < numNodes
                For Each it In costBackward(_from)
                    If finalNodesFlg(_from) Then
                        it._reduced_cost += d(_from) - d(l(0))
                    End If
                    If finalNodesFlg(it._to) Then
                        it._reduced_cost -= d(it._to) - d(l(0))
                    End If
                Next

                _from += 1
            End While
        End Sub

        Friend Overridable Sub heapDecreaseKey(Q As List(Of Edge3), nodes_to_Q As List(Of Integer), v As Integer, alt As Long)
            Dim i = nodes_to_Q(v)
            Q(i)._dist = alt
            While i > 0 AndAlso Q(PARENT(i))._dist > Q(i)._dist
                swapHeap(Q, nodes_to_Q, i, PARENT(i))
                i = PARENT(i)
            End While
        End Sub

        Friend Overridable Sub heapRemoveFirst(Q As List(Of Edge3), nodes_to_Q As List(Of Integer))
            swapHeap(Q, nodes_to_Q, 0, Q.Count - 1)
            Q.RemoveAt(Q.Count - 1)
            heapify(Q, nodes_to_Q, 0)
        End Sub

        Friend Overridable Sub heapify(Q As List(Of Edge3), nodes_to_Q As List(Of Integer), i As Integer)
            Do
                ' TODO: change to loop
                Dim l = LEFT(i)
                Dim r = RIGHT(i)
                Dim smallest As Integer
                If l < Q.Count AndAlso Q(l)._dist < Q(i)._dist Then
                    smallest = l
                Else
                    smallest = i
                End If
                If r < Q.Count AndAlso Q(r)._dist < Q(smallest)._dist Then
                    smallest = r
                End If

                If smallest = i Then
                    Return
                End If

                swapHeap(Q, nodes_to_Q, i, smallest)
                i = smallest

            Loop While True
        End Sub

        Friend Overridable Sub swapHeap(Q As List(Of Edge3), nodesToQ As List(Of Integer), i As Integer, j As Integer)
            Dim tmp = Q(i)
            Q(i) = Q(j)
            Q(j) = tmp
            nodesToQ(Q(j)._to) = j
            nodesToQ(Q(i)._to) = i
        End Sub

        Friend Overridable Function LEFT(i As Integer) As Integer
            Return 2 * (i + 1) - 1
        End Function

        Friend Overridable Function RIGHT(i As Integer) As Integer
            Return 2 * (i + 1) ' 2 * (i + 1) + 1 - 1
        End Function

        Friend Overridable Function PARENT(i As Integer) As Integer
            Return (i - 1) / 2
        End Function
    End Class


End Namespace

