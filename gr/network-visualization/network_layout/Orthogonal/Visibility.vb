#Region "Microsoft.VisualBasic::15212ac95fdcdbed47ab25bdb12bebf7, gr\network-visualization\network_layout\Orthogonal\Visibility.vb"

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

    '   Total Lines: 1019
    '    Code Lines: 823 (80.77%)
    ' Comment Lines: 96 (9.42%)
    '    - Xml Docs: 8.33%
    ' 
    '   Blank Lines: 100 (9.81%)
    '     File Size: 49.46 KB


    '     Class Visibility
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: adjacentFaces, allPossibleWVisibility2Connected, blockSubgraph, criticalPathCosts, indexOfClosest
    '                   moveSegment, reorganizeAttempt, sanityCheck, WVisibility, (+2 Overloads) WVisibility2Connected
    ' 
    '         Sub: copy, gridAlign, printResult, reorganize
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' 
'  To change this license header, choose License Headers in Project Properties.
'  To change this template file, choose Tools | Templates
'  and open the template in the editor.
' 

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts.Orthogonal.util
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Orthogonal

    ''' 
    ''' <summary>
    ''' @author santi
    ''' </summary>
    Public Class Visibility
        Public Shared DEBUG As Integer = 0

        Public graph As Integer()()
        Public nEdges As Integer
        Public edgeIndexes As Integer()() ' the index of each of the edges
        Public edge_n1 As Integer() ' node 1 of each of the edges
        Public edge_n2 As Integer() ' node 2 of each of the edges

        ' visibility results:
        Public horizontal_y As Double() ' graph nodes
        Public horizontal_x1 As Double()
        Public horizontal_x2 As Double()
        Public vertical_x As Double() ' graph edges
        Public vertical_y1 As Double()
        Public vertical_y2 As Double()

        Public Sub New(a_graph As Integer()())
            Dim n = a_graph.Length
            graph = a_graph
            nEdges = 0
            edgeIndexes = RectangularArray.Matrix(Of Integer)(n, n)

            For i = 0 To n - 1
                For j = i + 1 To n - 1
                    If graph(i)(j) = 0 Then
                        edgeIndexes(i)(j) = -1
                        edgeIndexes(j)(i) = -1
                    Else
                        edgeIndexes(i)(j) = nEdges
                        edgeIndexes(j)(i) = nEdges
                        nEdges += 1
                    End If
                Next
            Next
            edge_n1 = New Integer(nEdges - 1) {}
            edge_n2 = New Integer(nEdges - 1) {}
            For i = 0 To n - 1
                For j = i + 1 To n - 1
                    If edgeIndexes(i)(j) <> -1 Then
                        edge_n1(edgeIndexes(i)(j)) = i
                        edge_n2(edgeIndexes(i)(j)) = j
                    End If
                Next
            Next
            If Visibility.DEBUG >= 1 Then
                Console.WriteLine("Visibility calculator created for graph with adjacency matrix (n_edges = " & nEdges.ToString() & "):")
                For i = 0 To graph.Length - 1
                    For j = 0 To graph.Length - 1
                        Console.Write(graph(i)(j).ToString() & " ")
                    Next
                    Console.WriteLine("")
                Next
            End If
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub New(v As Visibility)
            Call copy(v)
        End Sub


        Public Overridable Sub copy(v As Visibility)
            graph = v.graph
            '        graph = new int[v.graph.length][v.graph[0].length];
            '        for(int i = 0;i<v.graph.length;i++) {
            '            for(int j = 0;j<v.graph[0].length;j++) {
            '                graph[i][j] = v.graph[i][j];
            '            }
            '        }      
            nEdges = v.nEdges
            edgeIndexes = v.edgeIndexes
            edge_n1 = v.edge_n1
            edge_n2 = v.edge_n2

            If v.horizontal_y IsNot Nothing Then
                horizontal_y = New Double(v.horizontal_y.Length - 1) {}
                horizontal_x1 = New Double(v.horizontal_x1.Length - 1) {}
                horizontal_x2 = New Double(v.horizontal_x2.Length - 1) {}
                For i As Integer = 0 To v.horizontal_y.Length - 1
                    horizontal_y(i) = v.horizontal_y(i)
                    horizontal_x1(i) = v.horizontal_x1(i)
                    horizontal_x2(i) = v.horizontal_x2(i)
                Next
            End If
            If v.vertical_x IsNot Nothing Then
                vertical_x = New Double(v.vertical_x.Length - 1) {}
                vertical_y1 = New Double(v.vertical_y1.Length - 1) {}
                vertical_y2 = New Double(v.vertical_y2.Length - 1) {}
                For i As Integer = 0 To v.vertical_x.Length - 1
                    vertical_x(i) = v.vertical_x(i)
                    vertical_y1(i) = v.vertical_y1(i)
                    vertical_y2(i) = v.vertical_y2(i)
                Next
            End If
        End Sub

        ' 
        ' 		This method stretches a visibility representatino to make each point fall into a dot of a grid, 
        ' 		the grid step size is given in "step"
        ' 		
        Public Overridable Sub gridAlign([step] As Double)
            Dim xvalues As List(Of Double) = New List(Of Double)()
            Dim yvalues As List(Of Double) = New List(Of Double)()

            For Each Y As Double In horizontal_y
                If Not yvalues.Contains(Y) Then
                    yvalues.Add(Y)
                End If
            Next
            For Each Y As Double In vertical_y1
                If Not yvalues.Contains(Y) Then
                    yvalues.Add(Y)
                End If
            Next
            For Each Y As Double In vertical_y2
                If Not yvalues.Contains(Y) Then
                    yvalues.Add(Y)
                End If
            Next
            For Each X As Double In vertical_x
                If Not xvalues.Contains(X) Then
                    xvalues.Add(X)
                End If
            Next
            For Each X As Double In horizontal_x1
                If Not xvalues.Contains(X) Then
                    xvalues.Add(X)
                End If
            Next
            For Each X As Double In horizontal_x2
                If Not xvalues.Contains(X) Then
                    xvalues.Add(X)
                End If
            Next

            xvalues.Sort()
            yvalues.Sort()

            ' filter those that are too similar (proably the same but for precission errors):
            Dim threshold = 0.00001
            Dim toDelete As IList(Of Double) = New List(Of Double)()
            For i = 0 To xvalues.Count - 1 - 1
                If System.Math.Abs(xvalues(i) - xvalues(i + 1)) < threshold Then
                    toDelete.Add(xvalues(i + 1))
                End If
            Next

            xvalues.RemoveAll(toDelete)
            toDelete.Clear()
            For i = 0 To yvalues.Count - 1 - 1
                If System.Math.Abs(yvalues(i) - yvalues(i + 1)) < threshold Then
                    toDelete.Add(yvalues(i + 1))
                End If
            Next

            yvalues.RemoveAll(toDelete)

            For i = 0 To horizontal_y.Length - 1
                horizontal_y(i) = indexOfClosest(horizontal_y(i), yvalues) * [step]
                horizontal_x1(i) = indexOfClosest(horizontal_x1(i), xvalues) * [step]
                horizontal_x2(i) = indexOfClosest(horizontal_x2(i), xvalues) * [step]
            Next
            For i = 0 To vertical_x.Length - 1
                vertical_x(i) = indexOfClosest(vertical_x(i), xvalues) * [step]
                vertical_y1(i) = indexOfClosest(vertical_y1(i), yvalues) * [step]
                vertical_y2(i) = indexOfClosest(vertical_y2(i), yvalues) * [step]
            Next
        End Sub

        Friend Overridable Function indexOfClosest(v As Double, l As IList(Of Double)) As Integer
            Dim best = -1
            Dim best_diff As Double = 0
            Dim i = 0
            For Each d As Double? In l
                Dim diff = System.Math.Abs(v - d.Value)
                If best = -1 OrElse diff < best_diff Then
                    best = i
                    best_diff = diff
                End If
                i += 1
            Next
            Return best
        End Function


        ' W-Visibility algorithm (reference)
        Public Overridable Function WVisibility() As Boolean
            If Visibility.DEBUG >= 1 Then
                Console.WriteLine("Blocks and cutnodes (for a graph with " & graph.Length.ToString() & " nodes)")
            End If
            Dim tmp As Pair(Of Dictionary(Of Integer, IList(Of Integer)), Dictionary(Of Integer, IList(Of Integer))) = Orthogonal.Blocks.blocks(graph)
            Dim blocks = tmp.m_a
            Dim cutNodes = tmp.m_b
            If Visibility.DEBUG >= 1 Then
                For Each blockID In blocks.Keys
                    Console.WriteLine("block " & blockID.ToString() & ": " & blocks(blockID).ToString())
                Next
                For Each cutNode In cutNodes.Keys
                    Console.WriteLine("cutnode " & cutNode.ToString() & ": " & cutNodes(cutNode).ToString())
                Next
            End If

            If blocks.Count = 1 Then
                Return WVisibility2Connected()
            Else
                ' W-VISIBILITY2 algorithm
                ' from: "A Unified Approach to Visibility Representations of Planar Graphs"
                ' 1 find the blocks:
                Dim T As List(Of Integer) = New List(Of Integer)()
                Dim S As List(Of Integer) = New List(Of Integer)()
                CType(T, List(Of Integer)).AddRange(blocks.Keys)

                ' we randomize it (to get differnt outputs every time):
                T.Shuffle()

                ' 2 construct the visibility representation for B_1:
                Dim blockID1 = T.PopAt(0)
                Dim block1 = blocks(blockID1)
                Dim blockgraph1 = blockSubgraph(block1)

                Dim block1Visibility As New Visibility(blockgraph1)
                If Not block1Visibility.WVisibility2Connected() Then
                    Return False
                End If
                S.Add(blockID1)

                ' initialize the visibility representation for the whole graph:
                Dim n = graph.Length
                horizontal_y = New Double(n - 1) {}
                horizontal_x1 = New Double(n - 1) {}
                horizontal_x2 = New Double(n - 1) {}
                vertical_x = New Double(nEdges - 1) {}
                vertical_y1 = New Double(nEdges - 1) {}
                vertical_y2 = New Double(nEdges - 1) {}
                For i = 0 To block1.Count - 1
                    horizontal_y(block1(i)) = block1Visibility.horizontal_y(i)
                    horizontal_x1(block1(i)) = block1Visibility.horizontal_x1(i)
                    horizontal_x2(block1(i)) = block1Visibility.horizontal_x2(i)
                Next
                For i As Integer = 0 To block1Visibility.nEdges - 1
                    Dim v1 As Integer = block1Visibility.edge_n1(i)
                    Dim v2 As Integer = block1Visibility.edge_n2(i)
                    Dim idx = edgeIndexes(block1(v1))(block1(v2))
                    vertical_x(idx) = block1Visibility.vertical_x(i)
                    vertical_y1(idx) = block1Visibility.vertical_y1(i)
                    vertical_y2(idx) = block1Visibility.vertical_y2(i)
                Next


                ' 3:
                While T.Count > 0
                    ' Find all the blocks in T that have a cutpoint c in common with some blocks in S:

                    For Each c In cutNodes.Keys
                        Dim blocksWithC = cutNodes(c)
                        Dim newBlocks As IList(Of Integer) = New List(Of Integer)()
                        Dim intersectionWithS = False
                        For Each bc In blocksWithC
                            If S.Contains(bc) Then
                                intersectionWithS = True
                            End If
                            If T.Contains(bc) Then
                                newBlocks.Add(bc)
                            End If
                        Next
                        If intersectionWithS AndAlso newBlocks.Count > 0 Then
                            ' Find min and max y values of the current visibility representation (to later reason
                            ' on whether to add new blocks below or no top):
                            Dim currentMinY As Double = 0
                            Dim currentMaxY As Double = 0
                            For i = 0 To n - 1
                                If i = 0 OrElse horizontal_y(i) < currentMinY Then
                                    currentMinY = horizontal_y(i)
                                End If
                                If i = 0 OrElse horizontal_y(i) > currentMaxY Then
                                    currentMaxY = horizontal_y(i)
                                End If
                            Next


                            ' ind a w-visibility representation for each Be, using algorithm W-VISIBILITY, 
                            ' where in step 1, c is chosen to be the source vertex s:


                            Dim leftx = horizontal_x1(c) + 0.1
                            Dim rightx = horizontal_x1(c) + 0.9
                            Dim topy = horizontal_y(c) - 0.9
                            Dim bottomy = horizontal_y(c)
                            Dim offset_step = (rightx - leftx) / newBlocks.Count
                            Dim offset As Double = 0
                            Dim flipY = False

                            If bottomy = currentMaxY Then
                                flipY = True
                            End If

                            For Each blockID In newBlocks

                                Dim block = blocks(blockID)
                                Dim blockgraph = blockSubgraph(block)
                                Dim bv As New Visibility(blockgraph)
                                ' get an st-numbering where s is the cutnode:
                                Dim blockSTNumbering As Integer() = STNumbering.stNumbering(blockgraph, blocks(blockID).IndexOf(c))
                                ' compute the visibility:
                                If Not bv.WVisibility2Connected(blockSTNumbering) Then
                                    Return False
                                End If

                                ' scale down the above representations in such a way that they all fit 
                                ' on the top of the vertex-segment corresponding to c in the w-visibility 
                                ' representation already constructed for S;
                                Dim block_leftx = leftx + offset_step * offset
                                Dim block_rightx = leftx + offset_step * (offset + 0.9)

                                If Visibility.DEBUG >= 1 Then
                                    Console.WriteLine("W-VISIBILITY2: adding new block at  " & block_leftx.ToString() & " - " & block_rightx.ToString() & " , " & topy.ToString() & " - " & bottomy.ToString())
                                End If

                                offset += 1
                                Dim minx As Double = 0, maxx As Double = 0
                                Dim miny As Double = 0, maxy As Double = 0
                                For j As Integer = 0 To bv.horizontal_y.Length - 1
                                    If j = 0 Then
                                        maxy = bv.horizontal_y(j)
                                        miny = bv.horizontal_y(j)
                                        minx = bv.horizontal_x1(j)
                                        maxx = bv.horizontal_x2(j)
                                    Else
                                        If bv.horizontal_y(j) < miny Then
                                            miny = bv.horizontal_y(j)
                                        End If
                                        If bv.horizontal_y(j) > maxy Then
                                            maxy = bv.horizontal_y(j)
                                        End If
                                        If bv.horizontal_x1(j) < minx Then
                                            minx = bv.horizontal_x1(j)
                                        End If
                                        If bv.horizontal_x2(j) > maxx Then
                                            maxx = bv.horizontal_x2(j)
                                        End If
                                    End If
                                Next
                                For i = 0 To block.Count - 1
                                    If c <> block(i) Then
                                        If flipY Then
                                            horizontal_y(block(i)) = bottomy + (bv.horizontal_y(i) - miny) / (maxy - miny) * (bottomy - topy)
                                        Else
                                            horizontal_y(block(i)) = bottomy - (bv.horizontal_y(i) - miny) / (maxy - miny) * (bottomy - topy)
                                        End If
                                        horizontal_x1(block(i)) = (bv.horizontal_x1(i) - minx) / (maxx - minx) * (block_rightx - block_leftx) + block_leftx
                                        horizontal_x2(block(i)) = (bv.horizontal_x2(i) - minx) / (maxx - minx) * (block_rightx - block_leftx) + block_leftx
                                        If Visibility.DEBUG >= 1 Then
                                            Console.WriteLine("W-VISIBILITY2: adding new node " & block(i).ToString())
                                            Console.WriteLine("  " & horizontal_x1(block(i)).ToString() & " - " & horizontal_x2(block(i)).ToString() & "," & horizontal_y(block(i)).ToString())
                                        End If
                                    End If
                                Next
                                For i As Integer = 0 To bv.nEdges - 1
                                    Dim v1 As Integer = bv.edge_n1(i)
                                    Dim v2 As Integer = bv.edge_n2(i)
                                    Dim idx = edgeIndexes(block(v1))(block(v2))
                                    vertical_x(idx) = (bv.vertical_x(i) - minx) / (maxx - minx) * (block_rightx - block_leftx) + block_leftx
                                    If flipY Then
                                        vertical_y1(idx) = bottomy + (bv.vertical_y1(i) - miny) / (maxy - miny) * (bottomy - topy)
                                        vertical_y2(idx) = bottomy + (bv.vertical_y2(i) - miny) / (maxy - miny) * (bottomy - topy)
                                    Else
                                        vertical_y1(idx) = bottomy - (bv.vertical_y1(i) - miny) / (maxy - miny) * (bottomy - topy)
                                        vertical_y2(idx) = bottomy - (bv.vertical_y2(i) - miny) / (maxy - miny) * (bottomy - topy)
                                    End If
                                    If Visibility.DEBUG >= 1 Then
                                        Console.WriteLine("W-VISIBILITY2: adding new edge " & block(v1).ToString() & " - " & block(v2).ToString())
                                        Console.WriteLine("  " & vertical_x(idx).ToString() & "," & vertical_y1(idx).ToString() & " - " & vertical_y2(idx).ToString())
                                    End If
                                Next
                            Next


                            T.RemoveAll(newBlocks)
                            CType(S, List(Of Integer)).AddRange(newBlocks)
                            gridAlign(1.0)
                            '                        if (S.size()>3) return true;
                            Exit For
                        End If
                    Next
                End While

                Call gridAlign(1.0)

                If Not sanityCheck() Then
                    For i = 0 To n - 1
                        Console.Error.Write("{")
                        For j = 0 To n - 1
                            Console.Error.Write(graph(i)(j).ToString() & ",")
                        Next
                        Console.Error.WriteLine("},")
                    Next
                    Throw New Exception("WVisibility: Visibility representation is not consistent after merging!")
                End If
                Return True
            End If
        End Function

        Friend Overridable Function blockSubgraph(block As IList(Of Integer)) As Integer()()
            Dim bn = block.Count
            Dim subgraph = RectangularArray.Matrix(Of Integer)(bn, bn)
            For i = 0 To bn - 1
                For j = 0 To bn - 1
                    subgraph(i)(j) = graph(block(i))(block(j))
                Next
            Next
            Return subgraph
        End Function

        ''' <summary>
        ''' 1,2: select (s,t) and generate an st-order. 
        '''   Generate the graph D induced by the st-ordering
        ''' </summary>
        ''' <returns></returns>
        Public Overridable Function WVisibility2Connected() As Boolean
            Return WVisibility2Connected(Orthogonal.STNumbering.stNumbering(graph))
        End Function

        Public Overridable Function allPossibleWVisibility2Connected() As IList(Of Visibility)
            Dim l As New List(Of Visibility)()
            Dim stNumberings As IList(Of Integer()) = Orthogonal.STNumbering.allSTNumberings(graph)
            For Each stNumbering In stNumberings
                If Not Orthogonal.STNumbering.verifySTNumbering(graph, stNumbering) Then
                    Throw New Exception($"Wrong STNumbering! {stNumbering.GetJson()}")
                End If
                Dim v As New Visibility(Me)
                v.WVisibility2Connected(stNumbering)
                l.Add(v)
            Next
            Return l
        End Function

        Public Overridable Function WVisibility2Connected(stNumbering As Integer()) As Boolean
            Dim n = graph.Length
            Dim s = -1
            Dim t = -1

            If stNumbering.IsNullOrEmpty Then
                Return False
            End If

            For i = 0 To n - 1
                If stNumbering(i) = 1 Then
                    s = i
                End If
                If stNumbering(i) = n Then
                    t = i
                End If
            Next

            If Visibility.DEBUG >= 1 Then
                Console.WriteLine("WVisibility2Connected: (s,t) = (" & s.ToString() & "," & t.ToString() & ")")
            End If

            If Visibility.DEBUG >= 1 Then
                Console.WriteLine("Graph indexed by stNumbers:")
                For i = 0 To n - 1
                    Dim v = -1
                    For tmp = 0 To n - 1
                        If stNumbering(tmp) = i + 1 Then
                            v = tmp
                        End If
                    Next
                    For j = 0 To n - 1
                        Dim w = -1
                        For tmp = 0 To n - 1
                            If stNumbering(tmp) = j + 1 Then
                                w = tmp
                            End If
                        Next
                        Console.Write(graph(v)(w).ToString() & " ")
                    Next
                    Console.WriteLine("")
                Next
            End If


            ' 3: Find a planar representation of D such that the arc [s,t] is on the external face
            '    Use the planar representation to construct a new digraph DStar:
            Dim D_star As Integer()()
            Dim embedding As IList(Of Integer)() = PlanarEmbedding.planarEmbedding2Connected(graph, stNumbering)
            If embedding Is Nothing Then
                Return False
            End If

            Dim faces As IList(Of IList(Of Integer)) = PlanarEmbedding.faces(embedding)
            Dim nFaces = faces.Count

            If faces.Count = 1 AndAlso n = 2 Then
                ' this can only happen if the graph has only 2 nodes (special case)
                horizontal_y = New Double(n - 1) {}
                horizontal_x1 = New Double(n - 1) {}
                horizontal_x2 = New Double(n - 1) {}
                For i = 0 To n - 1
                    horizontal_y(i) = stNumbering(i)
                    horizontal_x1(i) = -1
                    horizontal_x2(i) = 0
                Next

                ' edge segments
                vertical_x = New Double(nEdges - 1) {}
                vertical_y1 = New Double(nEdges - 1) {}
                vertical_y2 = New Double(nEdges - 1) {}
                For i = 0 To nEdges - 1
                    vertical_x(i) = 0
                    vertical_y1(i) = stNumbering(s)
                    vertical_y2(i) = stNumbering(t)
                Next
                Return True
            End If

            D_star = RectangularArray.Matrix(Of Integer)(nFaces, nFaces)
            Dim s_star = -1
            Dim t_star = -1
            For face1 = 0 To nFaces - 1
                Dim idxs = faces(face1).IndexOf(s)
                Dim idxt = faces(face1).IndexOf(t)
                Dim fn = faces(face1).Count
                If idxs >= 0 AndAlso idxt >= 0 Then
                    ' we want to make sure we get the two faces that are separated
                    ' by the s -> t edge
                    If (idxs + 1) Mod fn = idxt OrElse (idxt + 1) Mod fn = idxs Then
                        If t_star = -1 Then
                            t_star = face1
                        Else
                            s_star = face1
                        End If
                    End If
                End If
                For face2 = face1 + 1 To nFaces - 1
                    Dim edge As Pair(Of Integer, Integer) = Visibility.adjacentFaces(faces(face1), faces(face2))
                    If edge IsNot Nothing Then

                        If stNumbering(edge.m_a) > stNumbering(edge.m_b) Then
                            D_star(face1)(face2) = 2
                        Else
                            D_star(face2)(face1) = 2
                        End If
                    End If
                Next
            Next
            D_star(s_star)(t_star) = 0
            D_star(t_star)(s_star) = 0

            If Visibility.DEBUG >= 1 Then
                Console.WriteLine("D*: (s*,t*) = (" & s_star.ToString() & "," & t_star.ToString() & ")")
                For i = 0 To D_star.Length - 1
                    For j = 0 To D_star.Length - 1
                        Console.Write(D_star(i)(j).ToString() & " ")
                    Next
                    Console.WriteLine("")
                Next
            End If
            ' 4: Apply the critical path medhod to D* with all arc-lengths equal to 2. 
            '    This gives the function alpha(f) for each vertex f of D*.
            Dim alpha As Integer() = Visibility.criticalPathCosts(D_star)


            If True Then
                ' 5: Construct the w-visibility representation:
                ' vertex segments:
                horizontal_x1 = New Double(n - 1) {}
                horizontal_x2 = New Double(n - 1) {}
                Dim vertexEdges As IList(Of Integer)() = New List(Of Integer)(n - 1) {} ' to easily keep track of vertices in each node

                ' edge segments
                vertical_x = New Double(nEdges - 1) {}
                vertical_y1 = New Double(nEdges - 1) {}
                vertical_y2 = New Double(nEdges - 1) {}

                ' 5.1:  Use the st-numbering computed in step 2 to assign y-coordinates to horizontal vertex-segments.
                horizontal_y = New Double(n - 1) {}
                For i = 0 To n - 1
                    horizontal_y(i) = stNumbering(i)
                Next

                ' 5.2: Set the x-coordinate of arc [s, t] equal to -1.
                Dim st_index = edgeIndexes(s)(t)

                If st_index > -1 Then
                    vertical_x(st_index) = -1
                End If

                For i = 0 To n - 1
                    For j = 0 To n - 1
                        Dim idx = edgeIndexes(i)(j)
                        If idx <> -1 AndAlso stNumbering(i) < stNumbering(j) Then
                            If idx <> st_index Then
                                Dim face1 = -1
                                Dim face2 = -1
                                For Each face In faces
                                    Dim idxi = face.IndexOf(i)
                                    Dim idxj = face.IndexOf(j)
                                    Dim fn = face.Count
                                    If idxi >= 0 AndAlso idxj >= 0 AndAlso ((idxi + 1) Mod fn = idxj OrElse (idxj + 1) Mod fn = idxi) Then
                                        If face1 = -1 Then
                                            face1 = faces.IndexOf(face)
                                        Else
                                            face2 = faces.IndexOf(face)
                                            Exit For
                                        End If
                                    End If
                                Next
                                ' 5.3: ...
                                vertical_x(idx) = (alpha(face1) + alpha(face2)) / 2
                                If Visibility.DEBUG >= 1 Then
                                    Console.WriteLine("Visibility, faces for v segment (" & idx.ToString() & ": " & i.ToString() & "->" & j.ToString() & "): " & face1.ToString() & " , " & face2.ToString() & " -> " & vertical_x(idx).ToString())
                                End If
                            End If
                            ' 5.4: 
                            vertical_y1(idx) = System.Math.Min(horizontal_y(i), horizontal_y(j))
                            vertical_y2(idx) = System.Math.Max(horizontal_y(i), horizontal_y(j))

                            If vertexEdges(i) Is Nothing Then
                                vertexEdges(i) = New List(Of Integer)()
                            End If
                            vertexEdges(i).Add(idx)
                            If vertexEdges(j) Is Nothing Then
                                vertexEdges(j) = New List(Of Integer)()
                            End If
                            vertexEdges(j).Add(idx)
                        End If
                    Next
                Next

                ' 5.5: 
                For v As Integer = 0 To n - 1
                    Dim first As Boolean = True

                    horizontal_x1(v) = 0
                    horizontal_x2(v) = 0

                    If vertexEdges(v) Is Nothing Then
                        Continue For
                    End If

                    For Each edge In vertexEdges(v)

                        If first Then
                            first = False
                            horizontal_x2(v) = vertical_x(edge)
                            horizontal_x1(v) = vertical_x(edge)
                        Else
                            If vertical_x(edge) < horizontal_x1(v) Then
                                horizontal_x1(v) = vertical_x(edge)
                            End If
                            If vertical_x(edge) > horizontal_x2(v) Then
                                horizontal_x2(v) = vertical_x(edge)
                            End If
                        End If
                    Next
                    If horizontal_x1(v) = horizontal_x2(v) Then
                        horizontal_x1(v) -= 0.5
                    End If
                Next
            End If
            gridAlign(1.0)

            If Not sanityCheck() Then
                Console.Error.WriteLine("WVisibility2Connected: Visibility representation is not consistent!")
                For i = 0 To n - 1
                    Console.Error.Write("{")
                    For j = 0 To n - 1
                        Console.Error.Write(graph(i)(j).ToString() & ",")
                    Next
                    Console.Error.WriteLine("},")
                Next
                Return False
            End If

            Return True
        End Function


        Friend Shared Function criticalPathCosts(graph As Integer()()) As Integer()
            Dim n = graph.Length
            Dim distance = New Integer(n - 1) {}
            Dim indegree = New Integer(n - 1) {}

            For i = 0 To n - 1
                distance(i) = 0
                indegree(i) = 0
                For j = 0 To n - 1
                    If graph(j)(i) <> 0 Then
                        indegree(i) += 1
                    End If
                Next
            Next

            Dim Q As List(Of Integer) = New List(Of Integer)()
            For v = 0 To n - 1
                If indegree(v) = 0 Then
                    Q.Add(v)
                End If
            Next

            While Q.Count > 0
                Dim v = Q.PopAt(0)
                If Visibility.DEBUG >= 2 Then
                    Console.WriteLine("criticalPathCosts: " & v.ToString())
                End If
                For u = 0 To n - 1
                    If graph(v)(u) <> 0 Then
                        distance(u) = System.Math.Max(distance(u), distance(v) + graph(v)(u))
                        indegree(u) -= 1
                        If indegree(u) = 0 Then
                            Q.Add(u)
                        End If
                    End If
                Next
            End While

            Return distance
        End Function


        ' looks if two faces are adjacent (they are if the share any edge)
        Friend Shared Function adjacentFaces(face1 As IList(Of Integer), face2 As IList(Of Integer)) As Pair(Of Integer, Integer)
            For idx1 = 0 To face1.Count - 1
                Dim idx1b = idx1 + 1
                If idx1b >= face1.Count Then
                    idx1b = 0
                End If
                For idx2 = 0 To face2.Count - 1
                    If face1(idx1).Equals(face2(idx2)) Then
                        Dim idx2b = idx2 - 1
                        If idx2b < 0 Then
                            idx2b = face2.Count - 1
                        End If
                        If face1(idx1b).Equals(face2(idx2b)) Then
                            Return New Pair(Of Integer, Integer)(face1(idx1), face1(idx1b))
                        End If
                    End If
                Next
            Next
            Return Nothing
        End Function


        ' 
        ' 		    This method tries to realign the vertical segments, in order to minimize the 
        ' 		    number of contact points of each horitonzal edges with vertical edges
        ' 		
        Public Overridable Sub reorganize()
            gridAlign(1.0)

            For i = 0 To horizontal_y.Length - 1
                If horizontal_x1(i) > horizontal_x2(i) Then
                    Dim tmp = horizontal_x1(i)
                    horizontal_x1(i) = horizontal_x2(i)
                    horizontal_x2(i) = tmp
                End If
            Next
            For i = 0 To vertical_x.Length - 1
                If vertical_y1(i) > vertical_y2(i) Then
                    Dim tmp = vertical_y1(i)
                    vertical_y1(i) = vertical_y2(i)
                    vertical_y2(i) = tmp
                End If
            Next

            ' 
            ' 			boolean anotherRound;
            ' 			do {
            ' 			    anotherRound = false;
            ' 			    Visibility v = new Visibility(this);
            ' 			    if (v.reorganizeAttempt() && v.sanityCheck()) {
            ' 			        copy(v);
            ' 			        anotherRound = true;
            ' 			    }
            ' 			}while(anotherRound);        
            ' 			
            Dim alreadyMoved = New Boolean(vertical_x.Length - 1) {}
            For i = 0 To alreadyMoved.Length - 1
                alreadyMoved(i) = False
            Next
            While reorganizeAttempt(alreadyMoved)
            End While
            gridAlign(1.0)
        End Sub

        Public Overridable Function reorganizeAttempt(alreadyMoved As Boolean()) As Boolean
            For i = 0 To graph.Length - 1
                ' find the number of different contact points:
                Dim upContacts As IList(Of Integer) = New List(Of Integer)()
                Dim downContacts As IList(Of Integer) = New List(Of Integer)()
                Dim upContactsIdx As IList(Of Integer) = New List(Of Integer)()
                Dim downContactsIdx As IList(Of Integer) = New List(Of Integer)()
                Dim allContacts As IList(Of Integer) = New List(Of Integer)()
                Dim leftMostUp = -1
                Dim rightMostUp = -1
                Dim leftMostDown = -1
                Dim rightMostDown = -1
                For j = 0 To graph.Length - 1
                    If graph(i)(j) <> 0 Then
                        Dim idx = edgeIndexes(i)(j)
                        Dim x As Integer = vertical_x(idx)
                        If Not allContacts.Contains(x) Then
                            allContacts.Add(x)
                        End If
                        If horizontal_y(j) > horizontal_y(i) Then
                            downContacts.Add(x)
                            downContactsIdx.Add(idx)
                            If leftMostDown = -1 OrElse x < vertical_x(leftMostDown) Then
                                leftMostDown = idx
                            End If
                            If rightMostDown = -1 OrElse x > vertical_x(rightMostDown) Then
                                rightMostDown = idx
                            End If
                        Else
                            upContacts.Add(x)
                            upContactsIdx.Add(idx)
                            If leftMostUp = -1 OrElse x < vertical_x(leftMostUp) Then
                                leftMostUp = idx
                            End If
                            If rightMostUp = -1 OrElse x > vertical_x(rightMostUp) Then
                                rightMostUp = idx
                            End If
                        End If
                    End If
                Next


                If allContacts.Count > System.Math.Max(upContacts.Count, downContacts.Count) Then
                    If Visibility.DEBUG >= 1 Then
                        Console.WriteLine("Vertex " & i.ToString() & " might require reorganizing.")
                    End If

                    ' try to move the leftMostUp over the leftMostDown:
                    If Not alreadyMoved(leftMostUp) AndAlso CInt(vertical_x(leftMostUp)) > CInt(vertical_x(leftMostDown)) Then
                        Dim upx As Integer = vertical_x(leftMostUp)
                        Dim downx As Integer = vertical_x(leftMostDown)
                        If Visibility.DEBUG >= 1 Then
                            Console.WriteLine("  Trying to fix it by moving the leftMostUp over the leftMostDown edge " & leftMostUp.ToString() & " (x = " & upx.ToString() & ")")
                        End If
                        If moveSegment(leftMostUp, upx, downx) Then
                            alreadyMoved(leftMostUp) = True
                            Return True
                        End If
                    End If
                    ' try to move the leftMostDown over the leftMostUp:
                    If Not alreadyMoved(leftMostDown) AndAlso CInt(vertical_x(leftMostUp)) < CInt(vertical_x(leftMostDown)) Then
                        Dim upx As Integer = vertical_x(leftMostUp)
                        Dim downx As Integer = vertical_x(leftMostDown)
                        If Visibility.DEBUG >= 1 Then
                            Console.WriteLine("  Trying to fix it by moving the leftMostDown over the leftMostUp edge " & leftMostDown.ToString() & " (x = " & downx.ToString() & ")")
                        End If
                        If moveSegment(leftMostDown, downx, upx) Then
                            alreadyMoved(leftMostDown) = True
                            Return True
                        End If
                    End If

                    ' try to move the rightMostUp over the rightMostDown:
                    If Not alreadyMoved(rightMostUp) AndAlso CInt(vertical_x(rightMostUp)) < CInt(vertical_x(rightMostDown)) Then
                        Dim upx As Integer = vertical_x(rightMostUp)
                        Dim downx As Integer = vertical_x(rightMostDown)
                        If Visibility.DEBUG >= 1 Then
                            Console.WriteLine("  Trying to fix it by moving the rightMostUp over the rightMostDown edge " & rightMostUp.ToString() & " (x = " & upx.ToString() & ")")
                        End If
                        If moveSegment(rightMostUp, upx, downx) Then
                            alreadyMoved(rightMostUp) = True
                            Return True
                        End If
                    End If
                    ' try to move the rightMostDown over the rightMostUp:
                    If Not alreadyMoved(rightMostDown) AndAlso CInt(vertical_x(rightMostUp)) > CInt(vertical_x(rightMostDown)) Then
                        Dim upx As Integer = vertical_x(rightMostUp)
                        Dim downx As Integer = vertical_x(rightMostDown)
                        If Visibility.DEBUG >= 1 Then
                            Console.WriteLine("  Trying to fix it by moving the rightMostDown over the rightMostUp edge " & rightMostDown.ToString() & " (x = " & downx.ToString() & ")")
                        End If
                        If moveSegment(rightMostDown, downx, upx) Then
                            alreadyMoved(rightMostDown) = True
                            Return True
                        End If
                    End If

                End If
            Next
            Return False
        End Function

        Public Overridable Function moveSegment(segIdx As Integer, oldx As Double, newx As Double) As Boolean
            Dim tolerance = 0.1

            ' see if we can move upx to downx:
            ' check for collision with the vertical segments:
            For j = 0 To vertical_x.Length - 1
                If j <> segIdx Then
                    If vertical_x(j) >= newx - tolerance AndAlso vertical_x(j) <= oldx + tolerance OrElse vertical_x(j) >= oldx - tolerance AndAlso vertical_x(j) <= newx + tolerance Then
                        If vertical_y1(j) + tolerance < vertical_y2(segIdx) AndAlso vertical_y2(j) - tolerance > vertical_y1(segIdx) Then
                            If Visibility.DEBUG >= 1 Then
                                Console.WriteLine("Can't because of vertical segment " & j.ToString())
                            End If
                            Return False
                        End If
                    End If
                End If
            Next
            ' check for collision with the horizontal segments:
            For i = 0 To horizontal_y.Length - 1
                If i = edge_n1(segIdx) OrElse i = edge_n2(segIdx) Then
                    Continue For
                End If
                If System.Math.Abs(horizontal_y(i) - horizontal_y(edge_n1(segIdx))) < tolerance Then
                    If horizontal_x1(i) + tolerance < horizontal_x2(edge_n1(segIdx)) AndAlso horizontal_x2(i) - tolerance > horizontal_x1(edge_n1(segIdx)) Then
                        If Visibility.DEBUG >= 1 Then
                            Console.WriteLine("Can't because of horizontal segment " & i.ToString())
                        End If
                        Return False
                    End If
                End If
                If System.Math.Abs(horizontal_y(i) - horizontal_y(edge_n2(segIdx))) < tolerance Then
                    If horizontal_x1(i) + tolerance < horizontal_x2(edge_n2(segIdx)) AndAlso horizontal_x2(i) - tolerance > horizontal_x1(edge_n2(segIdx)) Then
                        If Visibility.DEBUG >= 1 Then
                            Console.WriteLine("Can't because of horizontal segment " & i.ToString())
                        End If
                        Return False
                    End If
                End If
            Next

            vertical_x(segIdx) = newx
            horizontal_x1(edge_n1(segIdx)) = System.Math.Min(horizontal_x1(edge_n1(segIdx)), newx)
            horizontal_x2(edge_n1(segIdx)) = System.Math.Max(horizontal_x2(edge_n1(segIdx)), newx)
            horizontal_x1(edge_n2(segIdx)) = System.Math.Min(horizontal_x1(edge_n2(segIdx)), newx)
            horizontal_x2(edge_n2(segIdx)) = System.Math.Max(horizontal_x2(edge_n2(segIdx)), newx)
            If Visibility.DEBUG >= 1 Then
                Console.WriteLine("  Edge " & segIdx.ToString() & " moved from " & oldx.ToString() & " to " & newx.ToString())
            End If
            Return True
        End Function


        Public Overridable Sub printResult()
            For i = 0 To horizontal_y.Length - 1
                Console.WriteLine("Hsegment: (" & horizontal_x1(i).ToString() & "-" & horizontal_x2(i).ToString() & ", " & horizontal_y(i).ToString() & ")")
            Next
            For i = 0 To vertical_x.Length - 1
                Console.WriteLine("Vsegment: (" & vertical_x(i).ToString() & ", " & vertical_y1(i).ToString() & "-" & vertical_y2(i).ToString() & ")")
            Next
        End Sub

        ' 
        ' 		// visibility results:
        ' 		public double horizontal_y[];   // graph nodes
        ' 		public double horizontal_x1[];
        ' 		public double horizontal_x2[];
        ' 		public double vertical_x[];     // graph edges
        ' 		public double vertical_y1[];
        ' 		public double vertical_y2[];    
        ' 		
        ' 		

        Public Overridable Function sanityCheck() As Boolean
            For i = 0 To vertical_x.Length - 1
                If vertical_y2(i) = vertical_y1(i) Then
                    Console.Error.WriteLine("vertical edge with negative or zero length: " & i.ToString())
                    Console.Error.WriteLine(i.ToString() & ": " & vertical_x(i).ToString() & ", [" & vertical_y1(i).ToString() & "," & vertical_y2(i).ToString() & "]")
                    Return False
                End If
                For j = i + 1 To vertical_x.Length - 1
                    If System.Math.Abs(vertical_x(i) - vertical_x(j)) < 0.01 Then
                        If vertical_y1(i) < vertical_y2(j) AndAlso vertical_y1(j) < vertical_y2(i) Then
                            Console.Error.WriteLine("vertical edges cross: " & i.ToString() & ", " & j.ToString())
                            Console.Error.WriteLine(i.ToString() & ": " & vertical_x(i).ToString() & ", [" & vertical_y1(i).ToString() & "," & vertical_y2(i).ToString() & "]")
                            Console.Error.WriteLine(j.ToString() & ": " & vertical_x(j).ToString() & ", [" & vertical_y1(j).ToString() & "," & vertical_y2(i).ToString() & "]")
                            Return False
                        End If
                    End If
                Next
            Next
            For i = 0 To horizontal_y.Length - 1
                If horizontal_x2(i) = horizontal_x1(i) Then
                    Console.Error.WriteLine("horizontal vertex with negative or zero length: " & i.ToString())
                    Console.Error.WriteLine(i.ToString() & ": " & horizontal_y(i).ToString() & ", [" & horizontal_x1(i).ToString() & "," & horizontal_x2(i).ToString() & "]")
                    Return False
                End If
                For j = i + 1 To horizontal_y.Length - 1
                    If System.Math.Abs(horizontal_y(i) - horizontal_y(j)) < 0.01 Then
                        If horizontal_x1(i) < horizontal_x2(j) AndAlso horizontal_x1(j) < horizontal_x2(i) Then
                            Console.Error.WriteLine("horizontal vertices cross: " & i.ToString() & ", " & j.ToString())
                            Console.Error.WriteLine(i.ToString() & ": " & horizontal_y(i).ToString() & ", [" & horizontal_x1(i).ToString() & "," & horizontal_x2(i).ToString() & "]")
                            Console.Error.WriteLine(j.ToString() & ": " & horizontal_y(j).ToString() & ", [" & horizontal_x1(j).ToString() & "," & horizontal_x2(i).ToString() & "]")
                            Return False
                        End If
                    End If
                Next
            Next

            Return True
        End Function
    End Class

End Namespace
