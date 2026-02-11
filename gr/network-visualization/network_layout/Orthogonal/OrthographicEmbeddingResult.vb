#Region "Microsoft.VisualBasic::601a4022dc290a0713f4833c44fbcc30, gr\network-visualization\network_layout\Orthogonal\OrthographicEmbeddingResult.vb"

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

    '   Total Lines: 1317
    '    Code Lines: 1145 (86.94%)
    ' Comment Lines: 79 (6.00%)
    '    - Xml Docs: 11.39%
    ' 
    '   Blank Lines: 93 (7.06%)
    '     File Size: 62.82 KB


    '     Class OrthographicEmbeddingResult
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: addVertices, edgeFits, edgeFitsIgnoring, find, findEdgeEnd1
    '                   findEdgeEnd2, findFirstNonOrthogonalEdge, findNonOrthogonalEdges, fixNonOrthogonalEdge, indexOfClosest
    '                   nodeHorizontalWiggleRoom, nodeHorizontalWiggleRoomSingleNode, nodeVerticalWiggleRoom, nodeVerticalWiggleRoomSingleNode, removeVertex
    '                   sanityCheck, ToString
    ' 
    '         Sub: fixNonOrthogonalEdges, gridAlign
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts.Orthogonal.util
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports std = System.Math

' 
'  To change this license header, choose License Headers in Project Properties.
'  To change this template file, choose Tools | Templates
'  and open the template in the editor.
' 
Namespace Orthogonal

    ''' 
    ''' <summary>
    ''' @author santi
    ''' </summary>
    Public Class OrthographicEmbeddingResult

        Public embedding As OEVertex()
        Public nodeIndexes As Integer()
        Public x As Double()
        Public y As Double()
        Public edges As Boolean()()

        Public Shared separation As Double = 0.25

        Default Public ReadOnly Property GetLayout(i As Integer) As Vector2D
            Get
                Return New Vector2D(x(i), y(i))
            End Get
        End Property

        Public Sub New(n As Integer)
            embedding = New OEVertex(n - 1) {}
            nodeIndexes = New Integer(n - 1) {}
            x = New Double(n - 1) {}
            y = New Double(n - 1) {}

            edges = RectangularArray.Matrix(Of Boolean)(n, n)
        End Sub

        Public Sub New(a_embedding As OEVertex(), visibility As Visibility, fixNonOrthogonal As Boolean)
            embedding = a_embedding

            ' count the number of nodes in the embedding:
            Dim n = a_embedding.Length
            For v = 0 To embedding.Length - 1
                For Each e As OEElement In embedding(v).embedding
                    n += e.bends
                Next
            Next

            nodeIndexes = New Integer(n - 1) {}
            x = New Double(n - 1) {}
            y = New Double(n - 1) {}

            edges = RectangularArray.Matrix(Of Boolean)(n, n)

            ' populate the list of corrdinates and edges:
            ' add the basic nodes:
            Dim idx = 0
            For v = 0 To embedding.Length - 1
                nodeIndexes(idx) = embedding(v).v
                x(idx) = embedding(v).x
                y(idx) = embedding(v).y
                idx += 1
            Next

            ' add the nodes + edges resulting from the "bends"
            For v = 0 To embedding.Length - 1
                ' process all the bends:
                Dim ev As OEVertex = embedding(v)
                For Each oev As OEElement In ev.embedding
                    Dim w As Integer = oev.dest
                    If w > v Then
                        Dim ew As OEVertex = embedding(w)
                        Dim oew As OEElement = Nothing
                        For Each tmp As OEElement In ew.embedding
                            If tmp.dest = v Then
                                oew = tmp
                                Exit For
                            End If
                        Next

                        If oev.bends = 0 AndAlso oew.bends = 0 Then
                            edges(v)(w) = True
                        Else
                            ' handle the bends:
                            Dim startx = x(v)
                            Dim starty = y(v)
                            Dim endx = x(w)
                            Dim endy = y(w)

                            ' 
                            ' 							double y_direction = 0;
                            ' 							if (starty>endy) {
                            ' 							    y_direction = 1;
                            ' 							} if (starty<endy) {
                            ' 							    y_direction = -1;
                            ' 							}
                            ' 							 
                            Dim intermediate_x As Double = visibility.vertical_x(visibility.edgeIndexes(v)(w))

                            If oev.bends + oew.bends = 1 Then
                                nodeIndexes(idx) = -1
                                If oev.angle = OEElement.LEFT OrElse oev.angle = OEElement.RIGHT Then
                                    x(idx) = endx
                                    If oew.angle = OEElement.DOWN Then
                                        If starty > endy + 0.01 Then
                                            y(idx) = starty
                                        Else
                                            y(idx) = endy + separation ' needs to be fixed by fixNonOrthogonalEdges
                                        End If
                                    Else
                                        If starty < endy - 0.01 Then
                                            y(idx) = starty
                                        Else
                                            y(idx) = endy - separation ' needs to be fixed by fixNonOrthogonalEdges
                                        End If
                                    End If
                                    y(idx) = starty
                                Else
                                    x(idx) = startx
                                    If oev.angle = OEElement.DOWN Then
                                        If endy > starty + 0.01 Then
                                            y(idx) = endy
                                        Else
                                            y(idx) = starty + separation ' needs to be fixed by fixNonOrthogonalEdges
                                        End If
                                    Else
                                        If endy < starty - 0.01 Then
                                            y(idx) = endy
                                        Else
                                            y(idx) = starty - separation ' needs to be fixed by fixNonOrthogonalEdges
                                        End If
                                    End If
                                End If
                                edges(v)(idx) = True
                                edges(idx)(w) = True
                                idx += 1
                            ElseIf oev.bends + oew.bends = 2 Then
                                nodeIndexes(idx) = -1
                                nodeIndexes(idx + 1) = -1
                                If oev.angle = OEElement.LEFT Then
                                    If intermediate_x > startx Then
                                        x(idx) = startx - separation
                                        y(idx) = starty
                                        x(idx + 1) = startx - separation
                                        y(idx + 1) = endy
                                    Else
                                        If oew.angle = OEElement.LEFT AndAlso intermediate_x > endx Then
                                            x(idx) = endx - separation
                                            y(idx) = starty
                                            x(idx + 1) = endx - separation
                                            y(idx + 1) = endy
                                        Else
                                            x(idx) = intermediate_x
                                            y(idx) = starty
                                            x(idx + 1) = intermediate_x
                                            y(idx + 1) = endy
                                        End If
                                    End If
                                    edges(v)(idx) = True
                                    edges(idx)(idx + 1) = True
                                    edges(idx + 1)(w) = True
                                    idx += 2
                                ElseIf oev.angle = OEElement.RIGHT Then
                                    If intermediate_x < startx Then
                                        x(idx) = startx + separation
                                        y(idx) = starty
                                        x(idx + 1) = startx + separation
                                        y(idx + 1) = endy
                                    Else
                                        If oew.angle = OEElement.RIGHT AndAlso intermediate_x < endx Then
                                            x(idx) = endx + separation
                                            y(idx) = starty
                                            x(idx + 1) = endx + separation
                                            y(idx + 1) = endy
                                        Else
                                            x(idx) = intermediate_x
                                            y(idx) = starty
                                            x(idx + 1) = intermediate_x
                                            y(idx + 1) = endy
                                        End If
                                    End If
                                    edges(v)(idx) = True
                                    edges(idx)(idx + 1) = True
                                    edges(idx + 1)(w) = True
                                    idx += 2
                                Else
                                    If std.Abs(startx - endx) < 0.001 Then
                                        edges(v)(w) = True
                                    Else
                                        If std.Abs(intermediate_x - startx) < 0.001 Then
                                            If oew.angle = OEElement.UP Then ' up from the end
                                                x(idx) = startx
                                                y(idx) = endy - separation
                                                x(idx + 1) = endx
                                                y(idx + 1) = endy - separation ' down from the end
                                            Else
                                                x(idx) = startx
                                                y(idx) = endy + separation
                                                x(idx + 1) = endx
                                                y(idx + 1) = endy + separation
                                            End If
                                        Else
                                            If oev.angle = OEElement.UP Then ' up from the start
                                                x(idx) = startx
                                                y(idx) = starty - separation
                                                x(idx + 1) = endx
                                                y(idx + 1) = starty - separation ' down from the start
                                            Else
                                                x(idx) = startx
                                                y(idx) = starty + separation
                                                x(idx + 1) = endx
                                                y(idx + 1) = starty + separation
                                            End If
                                        End If
                                        edges(v)(idx) = True
                                        edges(idx)(idx + 1) = True
                                        edges(idx + 1)(w) = True
                                        idx += 2
                                    End If
                                End If
                            ElseIf oev.bends + oew.bends = 3 Then
                                nodeIndexes(idx) = -1
                                nodeIndexes(idx + 1) = -1
                                nodeIndexes(idx + 2) = -1
                                edges(v)(idx) = True
                                edges(idx)(idx + 1) = True
                                edges(idx + 1)(idx + 2) = True
                                edges(idx + 2)(w) = True
                                Dim nnewvertices = 3
                                If oev.angle = OEElement.LEFT Then
                                    Dim tmpx = intermediate_x
                                    If intermediate_x > startx Then
                                        tmpx = startx - separation
                                    End If
                                    x(idx) = tmpx
                                    y(idx) = starty
                                    If oew.angle = OEElement.DOWN Then
                                        x(idx + 1) = tmpx
                                        y(idx + 1) = endy + separation
                                        x(idx + 2) = endx
                                        y(idx + 2) = endy + separation
                                    Else
                                        x(idx + 1) = tmpx
                                        y(idx + 1) = endy - separation
                                        x(idx + 2) = endx
                                        y(idx + 2) = endy - separation
                                    End If
                                ElseIf oev.angle = OEElement.RIGHT Then
                                    Dim tmpx = intermediate_x
                                    If intermediate_x < startx Then
                                        tmpx = startx + separation
                                    End If
                                    x(idx) = tmpx
                                    y(idx) = starty
                                    If oew.angle = OEElement.DOWN Then
                                        x(idx + 1) = tmpx
                                        y(idx + 1) = endy + separation
                                        If std.Abs(tmpx - endx) > 0.001 Then
                                            x(idx + 2) = endx
                                            y(idx + 2) = endy + separation
                                        Else
                                            ' the thrid vertex would be identical to the second, so, do not add it!:
                                            nnewvertices = 2
                                            edges(idx + 1)(idx + 2) = False
                                            edges(idx + 2)(w) = False
                                            edges(idx + 1)(w) = True
                                        End If
                                    Else
                                        x(idx + 1) = tmpx
                                        y(idx + 1) = endy - separation
                                        If std.Abs(tmpx - endx) > 0.001 Then
                                            x(idx + 2) = endx
                                            y(idx + 2) = endy - separation
                                        Else
                                            ' the thrid vertex would be identical to the second, so, do not add it!:
                                            nnewvertices = 2
                                            edges(idx + 1)(idx + 2) = False
                                            edges(idx + 2)(w) = False
                                            edges(idx + 1)(w) = True
                                        End If
                                    End If
                                Else
                                    Dim tmpx = intermediate_x
                                    If oew.angle = OEElement.LEFT Then
                                        If intermediate_x > endx Then
                                            tmpx = endx - separation
                                        End If
                                    End If
                                    If oew.angle = OEElement.RIGHT Then
                                        If intermediate_x < endx Then
                                            tmpx = endx + separation
                                        End If
                                    End If
                                    If std.Abs(startx - tmpx) > 0.001 Then
                                        If oev.angle = OEElement.DOWN Then
                                            x(idx) = startx
                                            y(idx) = starty + separation
                                            x(idx + 1) = tmpx
                                            y(idx + 1) = starty + separation
                                        Else
                                            x(idx) = startx
                                            y(idx) = starty - separation
                                            x(idx + 1) = tmpx
                                            y(idx + 1) = starty - separation
                                        End If
                                        x(idx + 2) = tmpx
                                        y(idx + 2) = endy
                                    Else
                                        ' the thrid vertex would be identical to the second, so, do not add it!:
                                        nnewvertices = 2
                                        edges(idx + 1)(idx + 2) = False
                                        edges(idx + 2)(w) = False
                                        edges(idx + 1)(w) = True
                                        If oev.angle = OEElement.DOWN Then
                                            x(idx) = startx
                                            y(idx) = starty + separation
                                        Else
                                            x(idx) = startx
                                            y(idx) = starty - separation
                                        End If
                                        x(idx + 1) = tmpx
                                        y(idx + 1) = endy
                                    End If
                                End If
                                idx += nnewvertices
                            Else
                                'Connector with 4 bends:
                                nodeIndexes(idx) = -1
                                nodeIndexes(idx + 1) = -1
                                nodeIndexes(idx + 2) = -1
                                nodeIndexes(idx + 3) = -1
                                If oev.angle = OEElement.DOWN Then
                                    x(idx) = startx
                                    y(idx) = starty + separation
                                    x(idx + 1) = intermediate_x
                                    y(idx + 1) = starty + separation
                                Else
                                    x(idx) = startx
                                    y(idx) = starty - separation
                                    x(idx + 1) = intermediate_x
                                    y(idx + 1) = starty - separation
                                End If
                                If oew.angle = OEElement.DOWN Then
                                    x(idx + 2) = intermediate_x
                                    y(idx + 2) = endy + separation
                                    x(idx + 3) = endx
                                    y(idx + 3) = endy + separation
                                Else
                                    x(idx + 2) = intermediate_x
                                    y(idx + 2) = endy - separation
                                    x(idx + 3) = endx
                                    y(idx + 3) = endy - separation
                                End If
                                edges(v)(idx) = True
                                edges(idx)(idx + 1) = True
                                edges(idx + 1)(idx + 2) = True
                                edges(idx + 2)(idx + 3) = True
                                edges(idx + 3)(w) = True
                                idx += 4

                            End If
                        End If
                    End If
                Next
            Next

            If idx < n Then
                Dim nodeIndexes2 = New Integer(idx - 1) {}
                Dim x2 = New Double(idx - 1) {}
                Dim y2 = New Double(idx - 1) {}

                Dim edges2 = RectangularArray.Matrix(Of Boolean)(idx, idx)

                For i = 0 To idx - 1
                    nodeIndexes2(i) = nodeIndexes(i)
                    x2(i) = x(i)
                    y2(i) = y(i)
                    For j = 0 To idx - 1
                        edges2(i)(j) = edges(i)(j)
                    Next
                Next

                nodeIndexes = nodeIndexes2
                x = x2
                y = y2
                edges = edges2
            End If

            If fixNonOrthogonal Then
                fixNonOrthogonalEdges()
            End If
            gridAlign(1.0)
        End Sub

        Public Function find(x As Double, y As Double) As Boolean
            For i As Integer = 0 To nodeIndexes.Length - 1
                For j As Integer = 0 To nodeIndexes.Length - 1
                    If edges(i)(j) Then
                        If std.Abs(Me.x(i) - x) < 0.01 AndAlso std.Abs(Me.y(i) - y) < 0.01 Then
                            Return True
                        End If
                    End If
                Next
            Next

            Return False
        End Function

        Public Overridable Sub fixNonOrthogonalEdges()
            Dim repeat = True
            Dim desperate = False
            Do
                Dim edges As IList(Of Pair(Of Integer, Integer)) = findNonOrthogonalEdges()
                repeat = False
                For Each edge In edges
                    If fixNonOrthogonalEdge(edge, desperate) Then
                        repeat = True
                        Exit For
                    End If
                Next
                If Not repeat AndAlso edges.Count > 0 AndAlso Not desperate Then
                    ' one more attempt in "desperate" mode
                    repeat = True
                    desperate = True
                End If
            Loop While repeat
        End Sub

        ' if "desperate == true", when it cannot fix an edge, it will replace it by a 
        ' bent connector.

        Public Overridable Function fixNonOrthogonalEdge(edge As Pair(Of Integer, Integer), desperate As Boolean) As Boolean
            gridAlign(1.0)

            Dim v As OEVertex = If(edge.m_a < embedding.Length, embedding(edge.m_a), Nothing)
            Dim w As OEVertex = If(edge.m_b < embedding.Length, embedding(edge.m_b), Nothing)
            Dim oev As OEElement = Nothing
            Dim oew As OEElement = Nothing
            If v IsNot Nothing Then
                Dim target = -1
                If w IsNot Nothing Then
                    target = w.v
                Else
                    Dim visited As IList(Of Integer) = New List(Of Integer)()
                    visited.Add(v.v)
                    target = edge.m_b
                    While target >= embedding.Length
                        For i = 0 To edges.Length - 1
                            If (edges(target)(i) OrElse edges(i)(target)) AndAlso Not visited.Contains(i) Then
                                visited.Add(i)
                                target = i
                                Exit For
                            End If
                        Next
                    End While
                End If
                If v.v = target Then
                    Throw New Exception("fixNonOrthogonalEdges: looking for " & v.v.ToString() & " -> " & target.ToString())
                End If
                For Each tmp As OEElement In v.embedding
                    If tmp.dest = target Then
                        oev = tmp
                        Exit For
                    End If
                Next
            End If
            If w IsNot Nothing Then
                Dim target = -1
                If v IsNot Nothing Then
                    target = v.v
                Else
                    Dim visited As IList(Of Integer) = New List(Of Integer)()
                    visited.Add(w.v)
                    target = edge.m_a
                    While target >= embedding.Length
                        For i = 0 To edges.Length - 1
                            If (edges(target)(i) OrElse edges(i)(target)) AndAlso Not visited.Contains(i) Then
                                visited.Add(i)
                                target = i
                                Exit For
                            End If
                        Next
                    End While
                End If
                If w.v = target Then
                    Throw New Exception("fixNonOrthogonalEdges: looking for " & w.v.ToString() & " -> " & target.ToString())
                End If
                For Each tmp As OEElement In w.embedding
                    If tmp.dest = target Then
                        oew = tmp
                        Exit For
                    End If
                Next
            End If

            If (oev Is Nothing OrElse (oev.angle = OEElement.LEFT OrElse oev.angle = OEElement.RIGHT)) AndAlso (oew Is Nothing OrElse (oew.angle = OEElement.LEFT OrElse oew.angle = OEElement.RIGHT)) Then
                ' Find the vertical ranges of movement of each vertex:
                Dim group_v As IList(Of Integer) = New List(Of Integer)()
                Dim group_w As IList(Of Integer) = New List(Of Integer)()
                Dim range_v = nodeVerticalWiggleRoom(edge.m_a, group_v)
                Dim range_w = nodeVerticalWiggleRoom(edge.m_b, group_w)

                ' if the ranges overlap, then move them to the center of the overlap:
                If range_v.m_a <= range_w.m_b AndAlso range_w.m_a <= range_w.m_b Then
                    Dim overlap_y1 = std.Max(range_v.m_a, range_w.m_a)
                    Dim overlap_y2 = std.Min(range_v.m_b, range_w.m_b)
                    Dim new_y = (overlap_y1 + overlap_y2) / 2

                    ' check if the edge would work:
                    Dim toIgnore As IList(Of Integer) = New List(Of Integer)()
                    CType(toIgnore, List(Of Integer)).AddRange(group_v)
                    CType(toIgnore, List(Of Integer)).AddRange(group_w)
                    Dim min_x = x(edge.m_a)
                    Dim max_x = x(edge.m_a)
                    For Each tmp In group_v
                        If x(tmp) < min_x Then
                            min_x = x(tmp)
                        End If
                        If x(tmp) > max_x Then
                            max_x = x(tmp)
                        End If
                    Next
                    For Each tmp In group_w
                        If x(tmp) < min_x Then
                            min_x = x(tmp)
                        End If
                        If x(tmp) > max_x Then
                            max_x = x(tmp)
                        End If
                    Next
                    Dim fits = False
                    fits = edgeFitsIgnoring(min_x, new_y, max_x, new_y, toIgnore)
                    If Not fits Then
                        Dim tmpy = overlap_y1

                        While tmpy <= overlap_y2
                            If edgeFitsIgnoring(min_x, new_y, max_x, new_y, toIgnore) Then
                                fits = True
                                new_y = tmpy
                                Exit While
                            End If

                            tmpy += 0.25
                        End While
                    End If
                    If fits Then
                        For Each tmp In group_v
                            y(tmp) = new_y
                        Next
                        For Each tmp In group_w
                            y(tmp) = new_y
                        Next
                        Return True
                    End If
                End If
                If desperate Then
                    ' restore a bent connector:
                    edges(edge.m_a)(edge.m_b) = False
                    edges(edge.m_b)(edge.m_a) = False
                    Dim idx = x.Length
                    x = x.CopyOf(x.Length + 2)
                    y = y.CopyOf(y.Length + 2)
                    nodeIndexes = nodeIndexes.CopyOf(nodeIndexes.Length + 2)

                    Dim newedges = RectangularArray.Matrix(Of Boolean)(edges.Length + 2, edges.Length + 2)
                    For i = 0 To edges.Length - 1
                        For j = 0 To edges.Length - 1
                            newedges(i)(j) = edges(i)(j)
                        Next
                    Next
                    edges = newedges
                    Dim direction As Double = separation
                    If x(edge.m_b) < x(edge.m_a) Then
                        direction = -separation
                    End If
                    If edgeFits(x(edge.m_a), y(edge.m_a), x(edge.m_b) - direction, y(edge.m_a)) Then
                        x(idx) = x(edge.m_b) - direction
                        y(idx) = y(edge.m_a)
                        x(idx + 1) = x(edge.m_b) - direction
                        y(idx + 1) = y(edge.m_b)
                    Else
                        x(idx) = x(edge.m_a) + direction
                        y(idx) = y(edge.m_a)
                        x(idx + 1) = x(edge.m_a) + direction
                        y(idx + 1) = y(edge.m_b)
                    End If
                    nodeIndexes(idx) = -1
                    nodeIndexes(idx + 1) = -1
                    edges(edge.m_a)(idx) = True
                    edges(idx)(idx + 1) = True
                    edges(idx + 1)(edge.m_b) = True
                    Return True
                End If
            End If

            If (oev Is Nothing OrElse (oev.angle = OEElement.UP OrElse oev.angle = OEElement.DOWN)) AndAlso (oew Is Nothing OrElse (oew.angle = OEElement.UP OrElse oew.angle = OEElement.DOWN)) Then
                ' Find the horizontal ranges of movement of each vertex:
                Dim group_v As IList(Of Integer) = New List(Of Integer)()
                Dim group_w As IList(Of Integer) = New List(Of Integer)()
                Dim range_v = nodeHorizontalWiggleRoom(edge.m_a, group_v)
                Dim range_w = nodeHorizontalWiggleRoom(edge.m_b, group_w)

                ' if the ranges overlap, then move them to the center of the overlap:
                If range_v.m_a <= range_w.m_b AndAlso range_w.m_a <= range_w.m_b Then
                    Dim overlap_x1 = std.Max(range_v.m_a, range_w.m_a)
                    Dim overlap_x2 = std.Min(range_v.m_b, range_w.m_b)
                    Dim new_x = (overlap_x1 + overlap_x2) / 2

                    ' check if the edge would work:
                    '                edges[edge.m_a][edge.m_b] = false;
                    '                edges[edge.m_b][edge.m_a] = false;
                    '                if (edgeFits(new_x,y[edge.m_a], new_x, y[edge.m_b])) {
                    Dim toIgnore As IList(Of Integer) = New List(Of Integer)()
                    CType(toIgnore, List(Of Integer)).AddRange(group_v)
                    CType(toIgnore, List(Of Integer)).AddRange(group_w)
                    Dim min_y = y(edge.m_a)
                    Dim max_y = y(edge.m_a)
                    For Each tmp In group_v
                        If y(tmp) < min_y Then
                            min_y = y(tmp)
                        End If
                        If y(tmp) > max_y Then
                            max_y = y(tmp)
                        End If
                    Next
                    For Each tmp In group_w
                        If y(tmp) < min_y Then
                            min_y = y(tmp)
                        End If
                        If y(tmp) > max_y Then
                            max_y = y(tmp)
                        End If
                    Next
                    Dim fits = False
                    fits = edgeFitsIgnoring(new_x, min_y, new_x, max_y, toIgnore)
                    If Not fits Then
                        Dim tmpx = overlap_x1

                        While tmpx <= overlap_x2
                            If edgeFitsIgnoring(tmpx, min_y, tmpx, max_y, toIgnore) Then
                                fits = True
                                new_x = tmpx
                                Exit While
                            End If

                            tmpx += separation
                        End While
                    End If
                    If fits Then
                        For Each tmp In group_v
                            x(tmp) = new_x
                        Next
                        For Each tmp In group_w
                            x(tmp) = new_x
                        Next

                        Return True
                    End If
                End If
                If desperate Then
                    ' restore a bent connector:
                    edges(edge.m_a)(edge.m_b) = False
                    edges(edge.m_b)(edge.m_a) = False
                    Dim idx = x.Length
                    x = x.CopyOf(x.Length + 2)
                    y = y.CopyOf(y.Length + 2)
                    nodeIndexes = nodeIndexes.CopyOf(nodeIndexes.Length + 2)

                    Dim newedges = RectangularArray.Matrix(Of Boolean)(edges.Length + 2, edges.Length + 2)
                    For i = 0 To edges.Length - 1
                        For j = 0 To edges.Length - 1
                            newedges(i)(j) = edges(i)(j)
                        Next
                    Next
                    edges = newedges
                    Dim direction As Double = separation
                    If y(edge.m_b) < y(edge.m_a) Then
                        direction = -separation
                    End If
                    If edgeFits(x(edge.m_a), y(edge.m_a), x(edge.m_a), y(edge.m_b) - direction) Then
                        x(idx) = x(edge.m_a)
                        y(idx) = y(edge.m_b) - direction
                        x(idx + 1) = x(edge.m_b)
                        y(idx + 1) = y(edge.m_b) - direction
                    Else
                        x(idx) = x(edge.m_a)
                        y(idx) = y(edge.m_a) + direction
                        x(idx + 1) = x(edge.m_b)
                        y(idx + 1) = y(edge.m_a) + direction
                    End If
                    nodeIndexes(idx) = -1
                    nodeIndexes(idx + 1) = -1
                    edges(edge.m_a)(idx) = True
                    edges(idx)(idx + 1) = True
                    edges(idx + 1)(edge.m_b) = True
                    Return True
                End If
            End If

            Return False
        End Function


        ' 
        ' 		    This function returns the wiggle room of a node plus all the nodes (in "nodeGroup"),
        ' 		    that need to be moved together with the node (those that have vertical connections with it)
        ' 		 
        Public Overridable Function nodeHorizontalWiggleRoom(vertex As Integer, nodeGroup As IList(Of Integer)) As Pair(Of Double, Double)
            nodeGroup.Clear()
            Dim open As List(Of Integer) = New List(Of Integer)()
            open.Add(vertex)

            ' Find all the nodes that have vertical connections:
            While open.Count > 0
                Dim v = open.PopAt(0)
                nodeGroup.Add(v)

                ' find neighbors:
                For w = 0 To x.Length - 1
                    If edges(v)(w) OrElse edges(w)(v) Then
                        If std.Abs(x(w) - x(v)) < 0.01 AndAlso Not nodeGroup.Contains(w) AndAlso Not open.Contains(w) Then
                            open.Add(w)
                        End If
                    End If
                Next
            End While


            ' Find the wiggleRoom of all of them:
            Dim wiggleRooms As List(Of Pair(Of Double, Double)) = New List(Of Pair(Of Double, Double))()
            For Each v As Integer? In nodeGroup
                wiggleRooms.Add(nodeHorizontalWiggleRoomSingleNode(v.Value))
            Next

            ' Compute the intersection:
            Dim result = wiggleRooms.PopAt(0)
            While wiggleRooms.Count > 0
                Dim tmp = wiggleRooms.PopAt(0)
                result.m_a = std.Max(result.m_a, tmp.m_a)
                result.m_b = std.Min(result.m_b, tmp.m_b)
            End While
            result.m_a += separation
            result.m_b -= separation

            Return result
        End Function

        ''' <summary>
        ''' Do the same thing for vertical
        ''' </summary>
        ''' <param name="vertex"></param>
        ''' <param name="nodeGroup"></param>
        ''' <returns></returns>
        Public Overridable Function nodeVerticalWiggleRoom(vertex As Integer, nodeGroup As IList(Of Integer)) As Pair(Of Double, Double)
            nodeGroup.Clear()
            Dim open As List(Of Integer) = New List(Of Integer)()
            open.Add(vertex)

            ' Find all the nodes that have horizontal connections:
            While open.Count > 0
                Dim v = open.PopAt(0)
                nodeGroup.Add(v)

                ' find neighbors:
                For w = 0 To x.Length - 1
                    If edges(v)(w) OrElse edges(w)(v) Then
                        If std.Abs(y(w) - y(v)) < 0.01 AndAlso Not nodeGroup.Contains(w) AndAlso Not open.Contains(w) Then
                            open.Add(w)
                        End If
                    End If
                Next
            End While


            ' Find the wiggleRoom of all of them:
            Dim wiggleRooms As List(Of Pair(Of Double, Double)) = New List(Of Pair(Of Double, Double))()
            For Each v As Integer? In nodeGroup
                wiggleRooms.Add(nodeVerticalWiggleRoomSingleNode(v.Value))
            Next

            ' Compute the intersection:
            Dim result = wiggleRooms.PopAt(0)

            While wiggleRooms.Count > 0
                Dim tmp = wiggleRooms.PopAt(0)

                result.m_a = std.Max(result.m_a, tmp.m_a)
                result.m_b = std.Min(result.m_b, tmp.m_b)
            End While
            result.m_a += separation
            result.m_b -= separation

            Return result
        End Function

        Public Overridable Function nodeHorizontalWiggleRoomSingleNode(vertex As Integer) As Pair(Of Double, Double)
            Dim min As Double = 0
            Dim max As Double = 0
            For i = 0 To x.Length - 1
                If i = 0 OrElse x(i) < min Then
                    min = x(i)
                End If
                If i = 0 OrElse x(i) > max Then
                    max = x(i)
                End If
            Next
            For w = 0 To x.Length - 1
                If edges(vertex)(w) OrElse edges(w)(vertex) Then
                    If std.Abs(y(w) - y(vertex)) < 0.01 Then
                        If x(w) < x(vertex) - 0.01 Then
                            If x(w) > min Then
                                min = x(w)
                            End If
                        ElseIf x(w) > x(vertex) + 0.01 Then
                            If x(w) < max Then
                                max = x(w)
                            End If
                        Else
                            Return New Pair(Of Double, Double)(x(vertex), x(vertex))
                        End If
                    End If
                End If
            Next

            If nodeIndexes(vertex) >= 0 Then
                For Each e As OEElement In embedding(vertex).embedding
                    If e.angle = OEElement.LEFT OrElse e.angle = OEElement.RIGHT Then
                        Dim w As Integer = e.dest
                        If edges(vertex)(w) OrElse edges(w)(vertex) Then
                            If x(w) < x(vertex) - 0.01 AndAlso x(w) > min Then
                                min = x(w)
                            End If
                            If x(w) > x(vertex) + 0.01 AndAlso x(w) < max Then
                                max = x(w)
                            End If
                        End If
                    End If
                Next
            End If

            Return New Pair(Of Double, Double)(min, max)
        End Function

        Public Overridable Function nodeVerticalWiggleRoomSingleNode(vertex As Integer) As Pair(Of Double, Double)
            Dim min As Double = 0
            Dim max As Double = 0
            For i = 0 To y.Length - 1
                If i = 0 OrElse y(i) < min Then
                    min = y(i)
                End If
                If i = 0 OrElse y(i) > max Then
                    max = y(i)
                End If
            Next

            For w = 0 To y.Length - 1
                If edges(vertex)(w) OrElse edges(w)(vertex) Then
                    If std.Abs(x(w) - x(vertex)) < 0.01 Then
                        If y(w) < y(vertex) - 0.01 Then
                            If y(w) > min Then
                                min = y(w)
                            End If
                        ElseIf y(w) > y(vertex) + 0.01 Then
                            If y(w) < max Then
                                max = y(w)
                            End If
                        Else
                            Return New Pair(Of Double, Double)(y(vertex), y(vertex))
                        End If
                    End If
                End If
            Next

            If nodeIndexes(vertex) >= 0 Then
                For Each e As OEElement In embedding(vertex).embedding
                    If e.angle = OEElement.UP OrElse e.angle = OEElement.DOWN Then
                        Dim w As Integer = e.dest
                        If edges(vertex)(w) OrElse edges(w)(vertex) Then
                            If y(w) < y(vertex) - 0.01 AndAlso y(w) > min Then
                                min = y(w)
                            End If
                            If y(w) > y(vertex) + 0.01 AndAlso y(w) < max Then
                                max = y(w)
                            End If
                        End If
                    End If
                Next
            End If

            Return New Pair(Of Double, Double)(min, max)
        End Function

        Public Overridable Function findFirstNonOrthogonalEdge() As Pair(Of Integer, Integer)
            For i = 0 To edges.Length - 1
                For j = 0 To edges.Length - 1
                    If edges(i)(j) AndAlso std.Abs(x(i) - x(j)) > 0.01 AndAlso std.Abs(y(i) - y(j)) > 0.01 Then
                        Return New Pair(Of Integer, Integer)(i, j)
                    End If
                Next
            Next
            Return Nothing
        End Function

        Public Overridable Function findNonOrthogonalEdges() As IList(Of Pair(Of Integer, Integer))
            Dim l As IList(Of Pair(Of Integer, Integer)) = New List(Of Pair(Of Integer, Integer))()
            For i = 0 To edges.Length - 1
                For j = 0 To edges.Length - 1
                    If edges(i)(j) AndAlso std.Abs(x(i) - x(j)) > 0.01 AndAlso std.Abs(y(i) - y(j)) > 0.01 Then
                        l.Add(New Pair(Of Integer, Integer)(i, j))
                    End If
                Next
            Next
            Return l
        End Function

        Public Overridable Sub gridAlign([step] As Double)
            Dim xvalues As List(Of Double) = New List(Of Double)()
            Dim yvalues As List(Of Double) = New List(Of Double)()

            For Each tmp In y
                If Not yvalues.Contains(tmp) Then
                    yvalues.Add(tmp)
                End If
            Next
            For Each tmp In x
                If Not xvalues.Contains(tmp) Then
                    xvalues.Add(tmp)
                End If
            Next

            xvalues.Sort()
            yvalues.Sort()

            ' filter those that are too similar (proably the same but for precission errors):
            Dim threshold = 0.01
            Dim toDelete As IList(Of Double) = New List(Of Double)()
            For i = 0 To xvalues.Count - 1 - 1
                If std.Abs(xvalues(i) - xvalues(i + 1)) < threshold Then
                    toDelete.Add(xvalues(i + 1))
                End If
            Next

            xvalues.RemoveAll(toDelete)
            toDelete.Clear()
            For i = 0 To yvalues.Count - 1 - 1
                If std.Abs(yvalues(i) - yvalues(i + 1)) < threshold Then
                    toDelete.Add(yvalues(i + 1))
                End If
            Next

            yvalues.RemoveAll(toDelete)

            For i = 0 To y.Length - 1
                y(i) = indexOfClosest(y(i), yvalues) * [step]
            Next
            For i = 0 To x.Length - 1
                x(i) = indexOfClosest(x(i), xvalues) * [step]
            Next
        End Sub

        Friend Overridable Function indexOfClosest(v As Double, l As IList(Of Double)) As Integer
            Dim best = -1
            Dim best_diff As Double = 0
            Dim i = 0
            For Each d As Double? In l
                Dim diff = std.Abs(v - d.Value)
                If best = -1 OrElse diff < best_diff Then
                    best = i
                    best_diff = diff
                End If
                i += 1
            Next
            Return best
        End Function

        Public Overridable Function edgeFits(x1 As Double, y1 As Double, x2 As Double, y2 As Double) As Boolean
            Dim tolerance = 0.01
            Dim tolerance2 = 0.005
            Dim minx1 = std.Min(x1, x2) + tolerance
            Dim maxx1 = std.Max(x1, x2) - tolerance
            Dim miny1 = std.Min(y1, y2) + tolerance
            Dim maxy1 = std.Max(y1, y2) - tolerance
            Dim isPoint = 0
            If std.Abs(x1 - x2) < tolerance Then
                minx1 = std.Min(x1, x2) - tolerance2
                maxx1 = std.Max(x1, x2) + tolerance2
                isPoint += 1
            End If
            If std.Abs(y1 - y2) < tolerance Then
                miny1 = std.Min(y1, y2) - tolerance2
                maxy1 = std.Max(y1, y2) + tolerance2
                isPoint += 1
            End If
            If isPoint >= 2 Then
                Return True
            End If

            For i2 = 0 To edges.Length - 1
                For j2 = i2 + 1 To edges.Length - 1
                    If edges(i2)(j2) OrElse edges(j2)(i2) Then
                        ' test for intersection:
                        Dim minx2 = std.Min(x(i2), x(j2)) + tolerance
                        Dim maxx2 = std.Max(x(i2), x(j2)) - tolerance
                        Dim miny2 = std.Min(y(i2), y(j2)) + tolerance
                        Dim maxy2 = std.Max(y(i2), y(j2)) - tolerance
                        Dim isPoint2 = 0
                        If std.Abs(x(i2) - x(j2)) < tolerance Then
                            minx2 = std.Min(x(i2), x(j2)) - tolerance2
                            maxx2 = std.Max(x(i2), x(j2)) + tolerance2
                            isPoint2 += 1
                        End If
                        If std.Abs(y(i2) - y(j2)) < tolerance Then
                            miny2 = std.Min(y(i2), y(j2)) - tolerance2
                            maxy2 = std.Max(y(i2), y(j2)) + tolerance2
                            isPoint2 += 1
                        End If
                        If isPoint2 >= 2 Then
                            Continue For
                        End If

                        If minx1 <= maxx2 AndAlso minx2 <= maxx1 AndAlso miny1 <= maxy2 AndAlso miny2 <= maxy1 Then
                            ' intersection!
                            Return False
                        End If
                    End If
                Next
            Next
            Return True
        End Function

        Public Overridable Function edgeFitsIgnoring(x1 As Double, y1 As Double, x2 As Double, y2 As Double, l As IList(Of Integer)) As Boolean
            Dim tolerance = 0.01
            Dim tolerance2 = 0.005
            Dim minx1 = std.Min(x1, x2) + tolerance
            Dim maxx1 = std.Max(x1, x2) - tolerance
            Dim miny1 = std.Min(y1, y2) + tolerance
            Dim maxy1 = std.Max(y1, y2) - tolerance
            Dim isPoint = 0
            If std.Abs(x1 - x2) < tolerance Then
                minx1 = std.Min(x1, x2) - tolerance2
                maxx1 = std.Max(x1, x2) + tolerance2
                isPoint += 1
            End If
            If std.Abs(y1 - y2) < tolerance Then
                miny1 = std.Min(y1, y2) - tolerance2
                maxy1 = std.Max(y1, y2) + tolerance2
                isPoint += 1
            End If
            If isPoint >= 2 Then
                Return True
            End If
            For i2 = 0 To edges.Length - 1
                For j2 = i2 + 1 To edges.Length - 1
                    If edges(i2)(j2) OrElse edges(j2)(i2) Then
                        If Not l.Contains(i2) AndAlso Not l.Contains(j2) Then
                            ' test for intersection:
                            Dim minx2 = std.Min(x(i2), x(j2)) + tolerance
                            Dim maxx2 = std.Max(x(i2), x(j2)) - tolerance
                            Dim miny2 = std.Min(y(i2), y(j2)) + tolerance
                            Dim maxy2 = std.Max(y(i2), y(j2)) - tolerance
                            Dim isPoint2 = 0
                            If std.Abs(x(i2) - x(j2)) < tolerance Then
                                minx2 = std.Min(x(i2), x(j2)) - tolerance2
                                maxx2 = std.Max(x(i2), x(j2)) + tolerance2
                                isPoint2 += 1
                            End If
                            If std.Abs(y(i2) - y(j2)) < tolerance Then
                                miny2 = std.Min(y(i2), y(j2)) - tolerance2
                                maxy2 = std.Max(y(i2), y(j2)) + tolerance2
                                isPoint2 += 1
                            End If
                            If isPoint2 >= 2 Then
                                Continue For
                            End If

                            If minx1 <= maxx2 AndAlso minx2 <= maxx1 AndAlso miny1 <= maxy2 AndAlso miny2 <= maxy1 Then
                                ' intersection!
                                Return False
                            End If
                        End If
                    End If
                Next
            Next
            Return True
        End Function

        Public Overridable Function findEdgeEnd1(n1 As Integer, n2 As Integer) As Integer
            If n1 < embedding.Length Then
                Return n1
            End If
            For i = 0 To edges.Length - 1
                If i <> n2 AndAlso edges(i)(n1) Then
                    Return findEdgeEnd1(i, n1)
                End If
            Next
            Return -1
        End Function

        Public Overridable Function findEdgeEnd2(n1 As Integer, n2 As Integer) As Integer
            If n2 < embedding.Length Then
                Return n2
            End If
            For i = 0 To edges.Length - 1
                If i <> n1 AndAlso edges(n2)(i) Then
                    Return findEdgeEnd2(n2, i)
                End If
            Next
            Return -1
        End Function

        Public Overridable Function sanityCheck(silent As Boolean) As Boolean
            Dim toleranceNode = 0.02
            Dim toleranceEdge1 = 0.01
            Dim toleranceEdge2 = 0.005
            '        if (embedding!=null &&
            '            embedding.length != nodeIndexes.length) return false;
            ' verify that there are no intersections:
            For i1 = 0 To edges.Length - 1
                For j1 = i1 + 1 To edges.Length - 1
                    If edges(i1)(j1) OrElse edges(j1)(i1) Then
                        Dim minx1 = std.Min(x(i1), x(j1)) + toleranceEdge1
                        Dim maxx1 = std.Max(x(i1), x(j1)) - toleranceEdge1
                        Dim miny1 = std.Min(y(i1), y(j1)) + toleranceEdge1
                        Dim maxy1 = std.Max(y(i1), y(j1)) - toleranceEdge1
                        Dim isPoint = 0
                        If std.Abs(x(i1) - x(j1)) < toleranceEdge1 Then
                            minx1 = std.Min(x(i1), x(j1)) - toleranceEdge2
                            maxx1 = std.Max(x(i1), x(j1)) + toleranceEdge2
                            isPoint += 1
                        End If
                        If std.Abs(y(i1) - y(j1)) < toleranceEdge1 Then
                            miny1 = std.Min(y(i1), y(j1)) - toleranceEdge2
                            maxy1 = std.Max(y(i1), y(j1)) + toleranceEdge2
                            isPoint += 1
                        End If
                        If isPoint >= 2 Then
                            Continue For
                        End If
                        ' edge - edge intersection
                        For i2 = i1 To edges.Length - 1
                            For j2 = If(i2 = i1, j1 + 1, i2 + 1) To edges.Length - 1
                                If edges(i2)(j2) OrElse edges(j2)(i2) Then
                                    ' test for intersection:
                                    Dim minx2 = std.Min(x(i2), x(j2)) + toleranceEdge1
                                    Dim maxx2 = std.Max(x(i2), x(j2)) - toleranceEdge1
                                    Dim miny2 = std.Min(y(i2), y(j2)) + toleranceEdge1
                                    Dim maxy2 = std.Max(y(i2), y(j2)) - toleranceEdge1
                                    Dim isPoint2 = 0
                                    If std.Abs(x(i2) - x(j2)) < toleranceEdge1 Then
                                        minx2 = std.Min(x(i2), x(j2)) - toleranceEdge2
                                        maxx2 = std.Max(x(i2), x(j2)) + toleranceEdge2
                                        isPoint2 += 1
                                    End If
                                    If std.Abs(y(i2) - y(j2)) < toleranceEdge1 Then
                                        miny2 = std.Min(y(i2), y(j2)) - toleranceEdge2
                                        maxy2 = std.Max(y(i2), y(j2)) + toleranceEdge2
                                        isPoint2 += 1
                                    End If
                                    If isPoint2 >= 2 Then
                                        Continue For
                                    End If
                                    If minx1 <= maxx2 AndAlso minx2 <= maxx1 AndAlso miny1 <= maxy2 AndAlso miny2 <= maxy1 Then
                                        ' intersection!
                                        If Not silent Then
                                            Console.Error.WriteLine("edge " & i1.ToString() & "->" & j1.ToString() & " crosses with " & i2.ToString() & "->" & j2.ToString())
                                            Console.Error.WriteLine("indexes " & nodeIndexes(i1).ToString() & "->" & nodeIndexes(j1).ToString() & " crosses with " & nodeIndexes(i2).ToString() & "->" & nodeIndexes(j2).ToString())
                                            Console.Error.WriteLine("  (" & x(i1).ToString() & "," & y(i1).ToString() & ")-(" & x(j1).ToString() & "," & y(j1).ToString() & ")  crosses  (" & x(i2).ToString() & "," & y(i2).ToString() & ")-(" & x(j2).ToString() & "," & y(j2).ToString() & ")")
                                        End If
                                        Return False
                                    End If
                                End If
                            Next
                        Next

                        ' edge-node intersections:
                        For i2 = 0 To edges.Length - 1
                            If i2 = i1 OrElse i2 = j1 Then
                                Continue For
                            End If
                            Dim minx2 = x(i2) - toleranceNode
                            Dim maxx2 = x(i2) + toleranceNode
                            Dim miny2 = y(i2) - toleranceNode
                            Dim maxy2 = y(i2) + toleranceNode
                            If minx1 <= maxx2 AndAlso minx2 <= maxx1 AndAlso miny1 <= maxy2 AndAlso miny2 <= maxy1 Then
                                ' intersection!
                                If Not silent Then
                                    Console.Error.WriteLine("edge " & i1.ToString() & "->" & j1.ToString() & " crosses with node " & i2.ToString())
                                    Console.Error.WriteLine("  (" & x(i1).ToString() & "," & y(i1).ToString() & ")-(" & x(j1).ToString() & "," & y(j1).ToString() & ")  crosses  (" & x(i2).ToString() & "," & y(i2).ToString() & ")")
                                End If
                                Return False
                            End If
                        Next
                    End If
                Next
            Next

            Return True
        End Function

        Public Overridable Function removeVertex(v As Integer) As OrthographicEmbeddingResult
            Dim n = nodeIndexes.Length - 1
            Dim o As New OrthographicEmbeddingResult(n)
            If embedding Is Nothing Then
                o.embedding = Nothing
            End If

            For i = 0 To n - 1
                If i < v Then
                    If embedding IsNot Nothing Then
                        ' 
                        ' 						if (i>= o.embedding.length || i >= embedding.length) {
                        ' 						    System.err.println("i: " + i);
                        ' 						    System.err.println("embedding.length: " + embedding.length);
                        ' 						    System.err.println("o.embedding.length: " + o.embedding.length);
                        ' 						}
                        ' 						 
                        If embedding.Length <= i Then
                            o.embedding(i) = Nothing
                        Else
                            o.embedding(i) = embedding(i)
                        End If
                    End If
                    o.nodeIndexes(i) = nodeIndexes(i)
                    o.x(i) = x(i)
                    o.y(i) = y(i)
                    For j = 0 To n - 1
                        If j < v Then
                            o.edges(i)(j) = edges(i)(j)
                        Else
                            o.edges(i)(j) = edges(i)(j + 1)
                        End If
                    Next
                Else
                    If embedding IsNot Nothing Then
                        If embedding.Length <= i + 1 Then
                            o.embedding(i) = Nothing
                        Else
                            o.embedding(i) = embedding(i + 1)
                        End If
                    End If
                    o.nodeIndexes(i) = nodeIndexes(i + 1)
                    o.x(i) = x(i + 1)
                    o.y(i) = y(i + 1)
                    For j = 0 To n - 1
                        If j < v Then
                            o.edges(i)(j) = edges(i + 1)(j)
                        Else
                            o.edges(i)(j) = edges(i + 1)(j + 1)
                        End If
                    Next
                End If
            Next
            Return o
        End Function

        Public Overridable Function addVertices(nv As Integer) As OrthographicEmbeddingResult
            Dim n = nodeIndexes.Length + nv
            Dim o As New OrthographicEmbeddingResult(n)
            If embedding Is Nothing Then
                o.embedding = Nothing
            End If

            For i = 0 To n - 1
                If i < nodeIndexes.Length Then
                    If embedding IsNot Nothing Then
                        o.embedding(i) = embedding(i)
                    End If
                    o.nodeIndexes(i) = nodeIndexes(i)
                    o.x(i) = x(i)
                    o.y(i) = y(i)
                    For j = 0 To n - 1
                        If j < nodeIndexes.Length Then
                            o.edges(i)(j) = edges(i)(j)
                        Else
                            o.edges(i)(j) = False
                        End If
                    Next
                Else
                    If embedding IsNot Nothing Then
                        o.embedding(i) = Nothing
                    End If
                    o.nodeIndexes(i) = -1
                    o.x(i) = 0
                    o.y(i) = 0
                    For j = 0 To n - 1
                        o.edges(i)(j) = False
                    Next
                End If
            Next
            Return o
        End Function

        Public Overrides Function ToString() As String
            Dim tmp = ""
            For i = 0 To nodeIndexes.Length - 1
                For j = 0 To nodeIndexes.Length - 1
                    If edges(i)(j) OrElse edges(j)(i) Then
                        tmp += "1" & ", "
                    Else
                        tmp += "0" & ", "
                    End If
                Next
                tmp += vbLf
            Next
            For i = 0 To nodeIndexes.Length - 1
                tmp += i.ToString() & ", " & nodeIndexes(i).ToString() & ", " & x(i).ToString() & ", " & y(i).ToString() & vbLf
            Next

            Return tmp
        End Function
    End Class

End Namespace
