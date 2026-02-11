#Region "Microsoft.VisualBasic::3be0110a9b99d4d20282c133b0dab1e8, gr\network-visualization\network_layout\Orthogonal\optimization\OrthographicEmbeddingOptimizer.vb"

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

    '   Total Lines: 520
    '    Code Lines: 422 (81.15%)
    ' Comment Lines: 44 (8.46%)
    '    - Xml Docs: 6.82%
    ' 
    '   Blank Lines: 54 (10.38%)
    '     File Size: 22.66 KB


    '     Class OrthographicEmbeddingOptimizer
    ' 
    '         Function: findConnections, (+2 Overloads) optimize, optimizeVertex, shortestMinimumBendPath
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts.Orthogonal.util
Imports Microsoft.VisualBasic.ListExtensions
Imports std = System.Math

' 
'  To change this license header, choose License Headers in Project Properties.
'  To change this template file, choose Tools | Templates
'  and open the template in the editor.
' 

Namespace Orthogonal.optimization

    ''' 
    ''' <summary>
    ''' @author santi
    ''' </summary>
    Public Class OrthographicEmbeddingOptimizer

        Public Shared DEBUG As Integer = 0
        Public Shared maxNodes As Integer = 5

        Public Shared Function optimize(o As OrthographicEmbeddingResult, graph As Integer()()) As OrthographicEmbeddingResult
            Return optimize(o, graph, New SegmentLengthEmbeddingComparator())
        End Function

        Public Shared Function optimize(o As OrthographicEmbeddingResult, graph As Integer()(), comparator As EmbeddingComparator) As OrthographicEmbeddingResult
            Dim n = graph.Length
            Dim change As Boolean
            Do
                change = False

                For v = 0 To n - 1
                    Dim o2 = optimizeVertex(v, o, graph, comparator)
                    If o2 IsNot o Then
                        change = True
                    End If
                    o = o2
                Next
            Loop While change

            o.gridAlign(1)
            Return o
        End Function

        Public Shared Function optimizeVertex(v As Integer, o As OrthographicEmbeddingResult, graph As Integer()(), comparator As EmbeddingComparator) As OrthographicEmbeddingResult
            Dim n = graph.Length ' number of vertices
            Dim n2 = o.x.Length
            Dim maxx = 0
            Dim maxy = 0
            o.gridAlign(1)
            For w = 0 To n2 - 1
                If o.x(w) > maxx Then
                    maxx = CInt(o.x(w))
                End If
                If o.y(w) > maxy Then
                    maxy = CInt(o.y(w))
                End If
            Next
            maxx += 1
            maxy += 1

            Dim occupancyMatrix = RectangularArray.Matrix(Of Integer)(maxx, maxy)

            If DEBUG >= 1 Then
                Console.WriteLine("OrthographicEmbeddingOptimizer.optimize vertex " & v.ToString())
            End If

            ' clear the matrix:
            For i = 0 To maxy - 1
                For j = 0 To maxx - 1
                    occupancyMatrix(j)(i) = 0
                Next
            Next

            ' find all the edges that connect v to all the other graph nodes
            Dim verticesToDelete As IList(Of Integer) = New List(Of Integer)()

            Dim edgesToIgnore = RectangularArray.Matrix(Of Boolean)(n2, n2)
            Dim open As List(Of Pair(Of Integer, Integer)) = New List(Of Pair(Of Integer, Integer))()
            For i = 0 To n2 - 1
                If o.edges(v)(i) Then
                    open.Add(New Pair(Of Integer, Integer)(v, i))
                End If
                If o.edges(i)(v) Then
                    open.Add(New Pair(Of Integer, Integer)(i, v))
                End If
            Next
            While open.Count > 0
                Dim current = open.PopAt(0)
                edgesToIgnore(current.m_a)(current.m_b) = True
                edgesToIgnore(current.m_b)(current.m_a) = True


                If current.m_b >= n Then
                    verticesToDelete.Add(current.m_b)
                    For i = 0 To n2 - 1
                        If o.edges(current.m_b)(i) OrElse o.edges(i)(current.m_b) Then
                            Dim [next] As Pair(Of Integer, Integer) = New Pair(Of Integer, Integer)(current.m_b, i)
                            If Not open.Contains([next]) AndAlso Not edgesToIgnore(current.m_b)(i) Then
                                open.Add([next])
                            End If
                        End If
                    Next
                End If
            End While

            ' add all the edges (except the ones to ignore):
            For i1 = 0 To n2 - 1
                For i2 = 0 To n2 - 1
                    If o.edges(i1)(i2) AndAlso Not edgesToIgnore(i1)(i2) Then
                        Dim x1 As Integer = o.x(i1)
                        Dim y1 As Integer = o.y(i1)
                        Dim x2 As Integer = o.x(i2)
                        Dim y2 As Integer = o.y(i2)
                        occupancyMatrix(x1)(y1) = 1
                        While x1 <> x2 OrElse y1 <> y2
                            If x1 < x2 Then
                                x1 += 1
                            End If
                            If x1 > x2 Then
                                x1 -= 1
                            End If
                            If y1 < y2 Then
                                y1 += 1
                            End If
                            If y1 > y2 Then
                                y1 -= 1
                            End If
                            If (x1 <> x2 OrElse y1 <> y2) AndAlso occupancyMatrix(x1)(y1) <> 0 Then
                                Throw New Exception("Some edges overlap (" & i1.ToString() & " -> " & i2.ToString() & "): (" & o.x(i1).ToString() & "," & o.y(i1).ToString() & ")->(" & x2.ToString() & "," & y2.ToString() & "), overlap is at (" & x1.ToString() & "," & y1.ToString() & ")!!!")
                            End If
                            occupancyMatrix(x1)(y1) = 1
                        End While
                    End If
                Next
            Next
            ' make sure we have added at least all the vertices:
            For i = 0 To n - 1
                If i = v Then
                    Continue For
                End If
                occupancyMatrix(o.x(i))(o.y(i)) = 1
            Next

            ' determine the set of positions to try (all the positions along the paths):

            Dim toTry = RectangularArray.Matrix(Of Boolean)(maxx, maxy)
            For i1 = 0 To n2 - 1
                For i2 = 0 To n2 - 1
                    If o.edges(i1)(i2) AndAlso edgesToIgnore(i1)(i2) Then
                        Dim x1 As Integer = o.x(i1)
                        Dim y1 As Integer = o.y(i1)
                        Dim x2 As Integer = o.x(i2)
                        Dim y2 As Integer = o.y(i2)
                        If occupancyMatrix(x1)(y1) = 0 Then
                            toTry(x1)(y1) = True
                        End If
                        '                    if (x1>0 && om[x1-1][y1]==0) toTry[x1-1][y1] = true;
                        '                    if (y1>0 && om[x1][y1-1]==0) toTry[x1][y1-1] = true;
                        '                    if (x1<maxx-1 && om[x1+1][y1]==0) toTry[x1+1][y1] = true;
                        '                    if (y1<maxy-1 && om[x1][y1+1]==0) toTry[x1][y1+1] = true;
                        While x1 <> x2 OrElse y1 <> y2
                            If x1 < x2 Then
                                x1 += 1
                            End If
                            If x1 > x2 Then
                                x1 -= 1
                            End If
                            If y1 < y2 Then
                                y1 += 1
                            End If
                            If y1 > y2 Then
                                y1 -= 1
                            End If
                            If occupancyMatrix(x1)(y1) = 0 Then
                                toTry(x1)(y1) = True
                            End If
                            '                        if (x1>0 && om[x1-1][y1]==0) toTry[x1-1][y1] = true;
                            '                        if (y1>0 && om[x1][y1-1]==0) toTry[x1][y1-1] = true;
                            '                        if (x1<maxx-1 && om[x1+1][y1]==0) toTry[x1+1][y1] = true;
                            '                        if (y1<maxy-1 && om[x1][y1+1]==0) toTry[x1][y1+1] = true;
                        End While
                    End If
                Next
            Next

            If DEBUG >= 1 Then
                Console.WriteLine("Cells occupied (1) and cells to try (2):")
                For y = 0 To maxy - 1
                    For x = 0 To maxx - 1
                        If toTry(x)(y) Then
                            Console.Write("2")
                        Else
                            Console.Write(occupancyMatrix(x)(y))
                        End If
                    Next
                    Console.WriteLine("")
                Next
            End If


            ' try each position:
            Dim idx = 2
            Dim best_o = o
            For x = 0 To maxx - 1
                For y = 0 To maxy - 1
                    If toTry(x)(y) Then
                        If DEBUG >= 1 Then
                            Console.WriteLine("Moving vertex " & v.ToString() & " to " & x.ToString() & "," & y.ToString())
                        End If
                        Dim result = findConnections(v, x, y, o, occupancyMatrix, idx, graph, verticesToDelete, edgesToIgnore)
                        If result IsNot Nothing Then
                            If comparator.compare(best_o, result) > 0 Then
                                If DEBUG >= 1 Then
                                    Console.WriteLine("better")
                                End If
                                best_o = result
                            End If
                        End If
                        idx += 1
                    End If
                Next
            Next

            Return best_o
        End Function


        Public Shared Function findConnections(v As Integer, x As Integer, y As Integer, o As OrthographicEmbeddingResult, om As Integer()(), idx As Integer, graph As Integer()(), verticesToDelete As IList(Of Integer), edgesToIgnore As Boolean()()) As OrthographicEmbeddingResult
            Dim n = graph.Length
            Dim n_new_segments = 0
            Dim result As IList(Of Pair(Of Integer, IList(Of Pair(Of Integer, Integer)))) = New List(Of Pair(Of Integer, IList(Of Pair(Of Integer, Integer))))()

            If DEBUG >= 1 Then
                Console.WriteLine("OrthographicEmbeddingOptimizer.findConnections for " & v.ToString() & " starting at " & x.ToString() & "," & y.ToString())
            End If

            om(x)(y) = idx
            For i = 0 To n - 1
                If graph(v)(i) <> 0 Then
                    ' try to find the shortest path and with the shortest bends from v to i:
                    Dim path = shortestMinimumBendPath(x, y, o.x(i), o.y(i), om, idx)
                    If path Is Nothing Then
                        Return Nothing
                    End If

                    Dim x1 = x
                    Dim y1 = y
                    If om(x1)(y1) <> 1 Then
                        om(x1)(y1) = idx
                    End If
                    For Each [Next] As Pair(Of Integer, Integer) In path
                        While x1 <> [Next].m_a OrElse y1 <> [Next].m_b
                            If x1 < [Next].m_a Then
                                x1 += 1
                            End If
                            If x1 > [Next].m_a Then
                                x1 -= 1
                            End If
                            If y1 < [Next].m_b Then
                                y1 += 1
                            End If
                            If y1 > [Next].m_b Then
                                y1 -= 1
                            End If
                            If om(x1)(y1) <> 1 Then
                                om(x1)(y1) = idx
                            End If
                        End While
                    Next
                    Dim x2i As Integer = o.x(i)
                    Dim y2i As Integer = o.y(i)
                    If om(x1)(y1) <> 1 Then
                        om(x1)(y1) = idx
                    End If
                    While x1 <> x2i OrElse y1 <> y2i
                        If x1 < x2i Then
                            x1 += 1
                        End If
                        If x1 > x2i Then
                            x1 -= 1
                        End If
                        If y1 < y2i Then
                            y1 += 1
                        End If
                        If y1 > y2i Then
                            y1 -= 1
                        End If
                        If om(x1)(y1) <> 1 Then
                            om(x1)(y1) = idx
                        End If
                    End While

                    result.Add(New Pair(Of Integer, IList(Of Pair(Of Integer, Integer)))(i, path))
                    n_new_segments += path.Count
                End If
            Next

            ' create the new variables:
            Dim n2 = o.x.Length
            Dim newn2 = n2 - verticesToDelete.Count + n_new_segments
            Dim nodeIndexes2 = New Integer(newn2 - 1) {}
            Dim x2 = New Double(newn2 - 1) {}
            Dim y2 = New Double(newn2 - 1) {}

            Dim edges2 = RectangularArray.Matrix(Of Boolean)(newn2, newn2)

            ' copy the previous values:
            Dim i1 = 0
            For i = 0 To n2 - 1
                If Not verticesToDelete.Contains(i) Then
                    nodeIndexes2(i1) = o.nodeIndexes(i)
                    If i = v Then
                        x2(i1) = x
                        y2(i1) = y
                    Else
                        x2(i1) = o.x(i)
                        y2(i1) = o.y(i)
                    End If
                    Dim j1 = 0
                    For j = 0 To n2 - 1
                        If Not verticesToDelete.Contains(j) Then
                            If Not edgesToIgnore(i)(j) Then
                                edges2(i1)(j1) = o.edges(i)(j)
                            End If
                            j1 += 1
                        End If
                    Next
                    i1 += 1
                End If
            Next

            If DEBUG >= 1 Then
                Console.WriteLine("vertices count goes from: " & n2.ToString() & "->" & newn2.ToString() & " after copying next index is " & i1.ToString())
            End If

            ' add the new values:
            For Each path In result
                Dim last = v
                For Each point In path.m_b
                    nodeIndexes2(i1) = -1
                    x2(i1) = point.m_a
                    y2(i1) = point.m_b
                    edges2(last)(i1) = True
                    '                        edges2[i1][last] = true;
                    If DEBUG >= 1 Then
                        Console.WriteLine("connecting (a): " & last.ToString() & " -> " & i1.ToString())
                    End If
                    last = i1
                    i1 += 1
                Next
                edges2(last)(path.m_a) = True
                '                    edges2[path.m_a][last] = true;
                If DEBUG >= 1 Then
                    Console.WriteLine("connecting (b): " & last.ToString() & " -> " & path.m_a.ToString())
                End If
            Next

            Dim o2 As OrthographicEmbeddingResult = New OrthographicEmbeddingResult(newn2)
            o2.embedding = Nothing
            o2.nodeIndexes = nodeIndexes2
            o2.x = x2
            o2.y = y2
            o2.edges = edges2

            Return o2

        End Function

        ' breadth first search:
        Public Shared Function shortestMinimumBendPath(x1 As Integer, y1 As Integer, x2 As Integer, y2 As Integer, om As Integer()(), idx As Integer) As IList(Of Pair(Of Integer, Integer))
            Dim dx = om.Length
            Dim dy = om(0).Length

            Dim parents = RectangularArray.Matrix(Of List(Of Integer))(dx, dy)

            Dim cost = RectangularArray.Matrix(Of Integer)(dx, dy)

            Dim bends = RectangularArray.Matrix(Of Integer)(dx, dy)
            Dim offx = New Integer() {-1, 0, 1, 0}
            Dim offy = New Integer() {0, -1, 0, 1}

            ' initialize:
            For i = 0 To dy - 1
                For j = 0 To dx - 1
                    parents(j)(i) = New List(Of Integer)()
                    cost(j)(i) = 0
                    bends(j)(i) = 0
                Next
            Next

            Dim current = 0
            Dim open As List(Of Integer) = New List(Of Integer)()
            open.Add(x1 + y1 * dx)
            While open.Count > 0
                current = open.PopAt(0)

                Dim cx = current Mod dx ' current
                Dim cy As Integer = std.Floor(current / dx)
                Dim currentParents As IList(Of Integer) = parents(cx)(cy)
                ' 
                ' 				int parentx = cx;
                ' 				int parenty = cy;
                ' 				if (parent!=-1) {
                ' 				    parentx = parent%dx;
                ' 				    parenty = parent/dx;
                ' 				}
                ' 				

                If cx <> x2 OrElse cy <> y2 Then
                    Dim nextcost = cost(cx)(cy) + 1
                    For i = 0 To 3
                        Dim nextbends = bends(cx)(cy) + 1
                        Dim nx = cx + offx(i)
                        Dim ny = cy + offy(i)
                        For Each parent In currentParents
                            Dim parentx = parent Mod dx
                            Dim parenty As Integer = std.Floor(parent / dx)
                            ' there is one parent from where we don't have to change direction:
                            If parentx = cx - offx(i) AndAlso parenty = cy - offy(i) Then
                                nextbends = bends(cx)(cy)
                            End If
                        Next
                        If nx >= 0 AndAlso nx < dx AndAlso ny >= 0 AndAlso ny < dy AndAlso (nx = x2 AndAlso ny = y2 OrElse om(nx)(ny) <> 1 AndAlso om(nx)(ny) <> idx) Then
                            If parents(nx)(ny).Count = 0 OrElse nextcost < cost(nx)(ny) OrElse nextcost = cost(nx)(ny) AndAlso nextbends < bends(nx)(ny) Then
                                parents(nx)(ny).Clear()
                                parents(nx)(ny).Add(current)
                                cost(nx)(ny) = nextcost
                                bends(nx)(ny) = nextbends
                                open.Add(nx + ny * dx)
                            ElseIf nextcost = cost(nx)(ny) AndAlso nextbends = bends(nx)(ny) Then
                                If Not parents(nx)(ny).Contains(current) Then
                                    parents(nx)(ny).Add(current)
                                End If
                            End If
                        End If
                    Next
                End If
            End While

            ' reconstruct the path:
            Dim path As List(Of Pair(Of Integer, Integer)) = New List(Of Pair(Of Integer, Integer))()
            If parents(x2)(y2).Count = 0 Then
                Return Nothing
            End If
            current = x2 + y2 * dx
            Dim start = x1 + y1 * dx
            Dim offs = 0
            Dim index As Integer

            While current <> start
                Dim [next] = current
                index = std.Floor(current / dx)
                path.Insert(0, New Pair(Of Integer, Integer)(current Mod dx, index))
                If offs = 0 Then
                    index = std.Floor(current / dx)
                    [next] = parents(current Mod dx)(index)(0)
                Else
                    ' try to find a parent that goes in the same direction:
                    index = std.Floor(current / dx)
                    For Each p In parents(current Mod dx)(index)
                        If p - current = offs Then
                            [next] = p
                        End If
                    Next
                    If [next] = current Then
                        index = std.Floor(current / dx)
                        [next] = parents(current Mod dx)(index)(0)
                    End If
                End If
                offs = [next] - current
                current = [next]

                If path.Count > maxNodes + 1 Then
                    Dim last = path.Last

                    If path.Skip(path.Count - maxNodes).All(Function(a) a.Equals(last)) Then
                        Exit While
                    End If
                End If
            End While
            index = std.Floor(current / dx)
            path.Insert(0, New Pair(Of Integer, Integer)(current Mod dx, index))
            If DEBUG >= 1 Then
                Console.WriteLine("full path to (" & x2.ToString() & "," & y2.ToString() & ") cost " & cost(x2)(y2).ToString() & "," & bends(x2)(y2).ToString() & ": " & path.ToString())
            End If

            ' only leave the bending points:
            Dim toDelete As IList(Of Pair(Of Integer, Integer)) = New List(Of Pair(Of Integer, Integer))()
            Dim previous1 As Pair(Of Integer, Integer) = Nothing
            Dim previous2 As Pair(Of Integer, Integer) = Nothing
            For Each p In path
                If previous1 IsNot Nothing AndAlso previous2 IsNot Nothing Then
                    Dim offsx1 = p.m_a - previous1.m_a
                    Dim offsy1 = p.m_b - previous1.m_b
                    Dim offsx2 = previous1.m_a - previous2.m_a
                    Dim offsy2 = previous1.m_b - previous2.m_b

                    If offsx1 = offsx2 AndAlso offsy1 = offsy2 Then
                        toDelete.Add(previous1)
                    End If
                End If
                previous2 = previous1
                previous1 = p
            Next

            path.RemoveAll(toDelete)
            ' now remove the start and end points:
            path.RemoveAt(0)
            path.RemoveAt(path.Count - 1)
            If DEBUG >= 1 Then
                Console.WriteLine(path)
            End If

            Return path
        End Function
    End Class

End Namespace
