#Region "Microsoft.VisualBasic::4bfe7e54f1d234c29139b11a17abf47e, gr\network-visualization\network_layout\Orthogonal\optimization\OrthographicEmbeddingPathOptimizer.vb"

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

    '   Total Lines: 388
    '    Code Lines: 315 (81.19%)
    ' Comment Lines: 39 (10.05%)
    '    - Xml Docs: 7.69%
    ' 
    '   Blank Lines: 34 (8.76%)
    '     File Size: 18.17 KB


    '     Class OrthographicEmbeddingPathOptimizer
    ' 
    '         Function: findShortestPath, (+2 Overloads) optimize, optimizeVertex
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts.Orthogonal.util
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
    Public Class OrthographicEmbeddingPathOptimizer
        Public Shared DEBUG As Integer = 0


        Public Shared Function optimize(o As OrthographicEmbeddingResult, graph As Integer()()) As OrthographicEmbeddingResult
            Return optimize(o, graph, New SegmentLengthEmbeddingComparator())
        End Function

        Public Shared Function optimize(o As OrthographicEmbeddingResult, graph As Integer()(), comparator As EmbeddingComparator) As OrthographicEmbeddingResult
            Dim n = graph.Length
            Dim change As Boolean

            Do
                change = False
                '            System.out.println("New optimization round...");
                Dim verticesAlreadyConsidered As IList(Of Integer) = New List(Of Integer)()
                For v = 0 To n - 1
                    If verticesAlreadyConsidered.Contains(v) Then
                        Continue For
                    End If
                    o.gridAlign(1)
                    Dim o2 = optimizeVertex(v, o, graph, comparator, verticesAlreadyConsidered)
                    If o2 IsNot o Then
                        change = True
                        '                    System.out.println("Change in vertex " + v);
                    End If
                    o = o2
                Next
            Loop While change
            o.gridAlign(1)
            Return o
        End Function

        Private Shared Function optimizeVertex(v As Integer, o As OrthographicEmbeddingResult, graph As Integer()(), comparator As EmbeddingComparator, verticesAlreadyConsidered As IList(Of Integer)) As OrthographicEmbeddingResult
            ' 1) find the whole path of the vertex 
            '        System.out.println("1) o.nodeIndexes.length = " + o.nodeIndexes.length + ", o.embedding.length " + (o.embedding != null ? o.embedding.length : "-"));
            Dim pathIndexes As List(Of Integer) = New List(Of Integer)()
            Dim open As List(Of Integer) = New List(Of Integer)()
            Dim pathNodesWithMoreThan2Neighbors As IList(Of Integer) = New List(Of Integer)()
            open.Add(v)
            While open.Count > 0
                Dim current = open.PopAt(0)
                pathIndexes.Add(current)
                Dim neighbors As IList(Of Integer) = New List(Of Integer)()
                For [Next] As Integer = 0 To o.edges.Length - 1
                    If o.edges(current)([Next]) OrElse o.edges([Next])(current) Then
                        neighbors.Add([Next])
                    End If
                Next

                If neighbors.Count = 2 Then
                    For Each [Next] As Integer In neighbors
                        If Not open.Contains([Next]) AndAlso Not pathIndexes.Contains([Next]) Then
                            open.Add([Next])
                        End If
                    Next
                ElseIf neighbors.Count > 2 Then
                    pathNodesWithMoreThan2Neighbors.Add(current)
                End If
            End While
            CType(verticesAlreadyConsidered, List(Of Integer)).AddRange(pathIndexes)
            If pathIndexes.Count = 1 Then
                Return o
            End If

            ' 2) Sort the path:
            '        System.out.println("2) o.nodeIndexes.length = " + o.nodeIndexes.length + ", o.embedding.length " + (o.embedding != null ? o.embedding.length : "-"));
            If True Then
                Dim sortedPath As List(Of Integer) = New List(Of Integer)()
                sortedPath.Add(pathIndexes.PopAt(0))
                While pathIndexes.Count > 0
                    ' find for the left:
                    Dim forLeft = -1
                    For Each v2 In pathIndexes
                        If o.edges(v2)(sortedPath(0)) OrElse o.edges(sortedPath(0))(v2) Then
                            forLeft = v2
                            Exit For
                        End If
                    Next
                    If forLeft <> -1 Then
                        sortedPath.Insert(0, forLeft)
                    End If
                    pathIndexes.RemoveAt(forLeft)
                    ' find for the right:
                    Dim forRight = -1
                    For Each v2 In pathIndexes
                        If o.edges(v2)(sortedPath(sortedPath.Count - 1)) OrElse o.edges(sortedPath(sortedPath.Count - 1))(v2) Then
                            forRight = v2
                            Exit For
                        End If
                    Next
                    If forRight <> -1 Then
                        sortedPath.Add(forRight)
                    End If
                    pathIndexes.RemoveAt(forRight)
                End While
                pathIndexes = sortedPath
            End If
            If Not o.edges(pathIndexes(0))(pathIndexes(1)) Then
                If DEBUG >= 1 Then
                    Console.WriteLine("reversing path")
                End If
                pathIndexes.Reverse()
            End If

            ' check if any node within the path has more than 2 neighbors:
            For i = 1 To pathIndexes.Count - 1
                If pathNodesWithMoreThan2Neighbors.Contains(pathIndexes(i)) Then
                    ' path is no good!
                    Return o
                End If
            Next

            ' 3) Find all the elements in the path, and calculate path length:
            '        System.out.println("3) o.nodeIndexes.length = " + o.nodeIndexes.length + ", o.embedding.length " + (o.embedding != null ? o.embedding.length : "-"));
            Dim pathLength = 0
            Dim necessaryElements As IList(Of Integer) = New List(Of Integer)()
            Dim nElements = 0
            If DEBUG >= 1 Then
                Console.Write("path: ")
            End If
            For i = 0 To pathIndexes.Count - 1
                If i > 0 Then
                    pathLength += CInt(std.Abs(o.x(pathIndexes(i)) - o.x(pathIndexes(i - 1))) + std.Abs(o.y(pathIndexes(i)) - o.y(pathIndexes(i - 1))))
                End If
                Dim v2 = pathIndexes(i)
                If i <> 0 AndAlso i <> pathIndexes.Count - 1 AndAlso o.nodeIndexes(v2) <> -1 Then
                    nElements += 1
                    necessaryElements.Add(v2)
                End If
                If DEBUG >= 1 Then
                    Console.Write(o.nodeIndexes(v2).ToString() & " ")
                End If
            Next
            If DEBUG >= 1 Then
                Console.WriteLine("    ->    nElements: " & nElements.ToString())
            End If


            ' 4) Find the shortest path that is equivalent
            '        System.out.println("4) o.nodeIndexes.length = " + o.nodeIndexes.length + ", o.embedding.length " + (o.embedding != null ? o.embedding.length : "-"));
            Dim dx = 0
            Dim dy = 0
            For i = 0 To o.x.Length - 1
                If DEBUG >= 1 Then
                    Console.WriteLine("    " & o.x(i).ToString() & ", " & o.y(i).ToString() & " (" & o.nodeIndexes(i).ToString() & ")")
                End If
                If o.x(i) > dx Then
                    dx = CInt(o.x(i))
                End If
                If o.y(i) > dy Then
                    dy = CInt(o.y(i))
                End If
            Next
            dx += 3
            dy += 3

            Dim map = RectangularArray.Matrix(Of Integer)(dx, dy)
            For i = 0 To o.x.Length - 1
                If Not pathIndexes.Contains(i) Then
                    map(CInt(o.x(i)) + 1)(CInt(o.y(i)) + 1) = 1
                End If
                For j = 0 To o.x.Length - 1
                    If o.edges(i)(j) OrElse o.edges(j)(i) Then
                        If pathIndexes.Contains(i) AndAlso pathIndexes.Contains(j) AndAlso std.Abs(pathIndexes.IndexOf(i) - pathIndexes.IndexOf(j)) = 1 Then
                            Continue For
                        End If
                        ' draw path:
                        Dim x As Integer = o.x(i)
                        Dim y As Integer = o.y(i)
                        While x <> CInt(o.x(j)) OrElse y <> CInt(o.y(j))
                            map(x + 1)(y + 1) = 1
                            If x < CInt(o.x(j)) Then
                                x += 1
                            End If
                            If x > CInt(o.x(j)) Then
                                x -= 1
                            End If
                            If y < CInt(o.y(j)) Then
                                y += 1
                            End If
                            If y > CInt(o.y(j)) Then
                                y -= 1
                            End If
                        End While
                    End If
                Next
            Next
            map(CInt(o.x(pathIndexes(0))) + 1)(CInt(o.y(pathIndexes(0))) + 1) = 2
            map(CInt(o.x(pathIndexes(pathIndexes.Count - 1))) + 1)(CInt(o.y(pathIndexes(pathIndexes.Count - 1))) + 1) = 3
            If DEBUG >= 1 Then
                For i = 0 To dy - 1
                    For j = 0 To dx - 1
                        Console.Write(map(j)(i))
                    Next
                    Console.WriteLine("")
                Next
            End If

            ' find the shortest path:
            If DEBUG >= 1 Then
                Console.WriteLine("Shortest path from " & (CInt(o.x(pathIndexes(0))) + 1).ToString() & ", " & (CInt(o.y(pathIndexes(0))) + 1).ToString() & " to " & (CInt(o.x(pathIndexes(pathIndexes.Count - 1))) + 1).ToString() & ", " & (CInt(o.y(pathIndexes(pathIndexes.Count - 1))) + 1).ToString())
                Console.WriteLine("Previous path was:")
                For Each idx In pathIndexes
                    Console.WriteLine("    " & (CInt(o.x(idx)) + 1).ToString() & ", " & (CInt(o.y(idx)) + 1).ToString())
                Next
            End If
            Dim path2 = findShortestPath(map, CInt(o.x(pathIndexes(0))) + 1, CInt(o.y(pathIndexes(0))) + 1, CInt(o.x(pathIndexes(pathIndexes.Count - 1))) + 1, CInt(o.y(pathIndexes(pathIndexes.Count - 1))) + 1)
            If DEBUG >= 1 Then
                Console.WriteLine("Shortest path: " & path2.ToString())
                Console.WriteLine("Length went from " & pathLength.ToString() & " to " & (path2.Count - 1).ToString())
            End If

            ' 5) if all the components fit, replace it!
            '        System.out.println("5) o.nodeIndexes.length = " + o.nodeIndexes.length + ", o.embedding.length " + (o.embedding != null ? o.embedding.length : "-"));
            If path2 Is Nothing Then
                Return o
            End If
            If path2.Count - 2 < nElements Then
                Return o
            End If
            If pathLength <= path2.Count - 1 Then
                Return o
            End If

            ' 6) Remove the old auxiliary points, and the previous path:
            '        System.out.println("6) o.nodeIndexes.length = " + o.nodeIndexes.length + ", o.embedding.length " + (o.embedding != null ? o.embedding.length : "-"));
            If True Then
                Dim toRemove As List(Of Integer) = New List(Of Integer)()
                For i = 0 To pathIndexes.Count - 1
                    Dim v2 = pathIndexes(i)
                    If o.nodeIndexes(v2) = -1 Then
                        toRemove.Add(v2)
                    End If
                    If i > 0 Then
                        o.edges(pathIndexes(i - 1))(pathIndexes(i)) = False
                        o.edges(pathIndexes(i))(pathIndexes(i - 1)) = False
                    End If
                Next
                toRemove.Sort()
                toRemove.Reverse()

                For Each v2 In toRemove
                    If DEBUG >= 1 Then
                        Console.WriteLine("removing vertex: " & v2.ToString() & " (out of " & o.nodeIndexes.Length.ToString() & ")")
                    End If
                    o = o.removeVertex(v2)
                    For i = 0 To pathIndexes.Count - 1
                        If pathIndexes(i) >= v2 Then
                            pathIndexes(i) = pathIndexes(i) - 1
                        End If
                    Next
                Next
            End If

            ' 7) Leave only the path important and elbow points:
            '        System.out.println("7) o.nodeIndexes.length = " + o.nodeIndexes.length + ", o.embedding.length " + (o.embedding != null ? o.embedding.length : "-"));
            Dim nextAuxiliar = o.nodeIndexes.Length
            If DEBUG >= 1 Then
                Console.WriteLine("nextAuxiliar: " & nextAuxiliar.ToString())
            End If
            Dim path3Indexes As IList(Of Integer) = New List(Of Integer)()
            Dim path3 As IList(Of Pair(Of Integer, Integer)) = New List(Of Pair(Of Integer, Integer))()
            path3.Add(path2(0))
            path3Indexes.Add(pathIndexes(0))
            For i = 0 To path2.Count - 2 - 1
                Dim dx1 = path2(i).m_a - path2(i + 1).m_a
                Dim dy1 = path2(i).m_b - path2(i + 1).m_b
                Dim dx2 = path2(i + 1).m_a - path2(i + 2).m_a
                Dim dy2 = path2(i + 1).m_b - path2(i + 2).m_b
                If i < necessaryElements.Count Then
                    path3Indexes.Add(necessaryElements(i))
                    path3.Add(path2(i + 1))
                Else
                    If dx1 <> dx2 OrElse dy1 <> dy2 Then
                        ' joint!
                        path3Indexes.Add(nextAuxiliar)
                        path3.Add(path2(i + 1))
                        nextAuxiliar += 1
                    End If
                End If
            Next
            path3.Add(path2(path2.Count - 1))
            path3Indexes.Add(pathIndexes(pathIndexes.Count - 1))


            ' 8) create new arrays with less nodes, and replace the originals:
            '        System.out.println("8) o.nodeIndexes.length = " + o.nodeIndexes.length + ", o.embedding.length " + (o.embedding != null ? o.embedding.length : "-"));
            Dim nAuxiliar = path3.Count - 2 - nElements
            If nAuxiliar > 0 Then
                o = o.addVertices(nAuxiliar)
            End If
            For i = 1 To path3.Count - 1 - 1
                o.x(path3Indexes(i)) = path3(i).m_a - 1
                o.y(path3Indexes(i)) = path3(i).m_b - 1
                If DEBUG >= 1 Then
                    Console.WriteLine(path3Indexes(i).ToString() & ": " & o.x(path3Indexes(i)).ToString() & ", " & o.y(path3Indexes(i)).ToString())
                End If
                o.edges(path3Indexes(i - 1))(path3Indexes(i)) = True
                o.edges(path3Indexes(i))(path3Indexes(i + 1)) = True

            Next

            '        System.out.println("end) o.nodeIndexes.length = " + o.nodeIndexes.length + ", o.embedding.length " + (o.embedding != null ? o.embedding.length : "-"));
            Return o
        End Function

        Private Shared Function findShortestPath(map As Integer()(), sx As Integer, sy As Integer, ex As Integer, ey As Integer) As IList(Of Pair(Of Integer, Integer))
            Dim open As List(Of Integer) = New List(Of Integer)()
            Dim w = map.Length
            Dim h = map(0).Length

            Dim closed = RectangularArray.Matrix(Of Integer)(w, h)
            Dim cx, cy As Integer

            For i = 0 To w - 1
                For j = 0 To h - 1
                    closed(i)(j) = -2
                Next
            Next

            open.Add(sx + sy * w)
            closed(sx)(sy) = -1
            While open.Count > 0
                '            System.out.println("    " + open.size());
                Dim current = open.PopAt(0)
                cx = current Mod w
                cy = current / w

                If cx = ex AndAlso cy = ey Then
                    ' found the end!
                    Exit While
                End If

                Dim offsx = New Integer() {-1, 0, 1, 0}
                Dim offsy = New Integer() {0, 1, 0, -1}
                For i = 0 To 3
                    Dim nx = cx + offsx(i)
                    Dim ny = cy + offsy(i)
                    Dim [next] = nx + ny * w
                    If nx >= 0 AndAlso nx < w AndAlso ny >= 0 AndAlso ny < h AndAlso closed(nx)(ny) = -2 AndAlso Not open.Contains([next]) AndAlso map(nx)(ny) <> 1 Then
                        closed(nx)(ny) = current
                        open.Add([next])
                    End If
                Next
            End While
            '        System.out.println("    done");

            If closed(ex)(ey) = 0 Then
                Return Nothing
            End If
            Dim path As IList(Of Pair(Of Integer, Integer)) = New List(Of Pair(Of Integer, Integer))()
            cx = ex
            cy = ey
            While closed(cx)(cy) <> -1
                path.Insert(0, New Pair(Of Integer, Integer)(cx, cy))
                Dim parent = closed(cx)(cy)
                cx = parent Mod w
                cy = parent / w
                If parent = -2 Then
                    Return Nothing ' no path!
                End If
                '            System.out.println("    RP: " + parent);
            End While

            path.Insert(0, New Pair(Of Integer, Integer)(sx, sy))

            Return path
        End Function

    End Class

End Namespace
