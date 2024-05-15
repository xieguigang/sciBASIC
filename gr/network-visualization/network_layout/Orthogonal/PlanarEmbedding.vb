#Region "Microsoft.VisualBasic::0de6332e3c18da64d92aaaeb1a84cdd8, gr\network-visualization\network_layout\Orthogonal\PlanarEmbedding.vb"

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

    '   Total Lines: 435
    '    Code Lines: 332
    ' Comment Lines: 60
    '   Blank Lines: 43
    '     File Size: 21.92 KB


    '     Class PlanarEmbedding
    ' 
    '         Function: extendUpwardEmbedding, faces, (+2 Overloads) planarEmbedding2Connected, planarUpwardEmbedding, translateEmbeddingToNodeIndexes
    ' 
    '         Sub: DFS, getFace
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.GraphTheory
Imports Microsoft.VisualBasic.ListExtensions

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
    Public Class PlanarEmbedding
        Public Shared DEBUG As Integer = 0

        ''' <summary>
        ''' This function assumes that the given graph is 2-connected
        ''' </summary>
        ''' <param name="graph"></param>
        ''' <returns></returns>
        Public Shared Function planarEmbedding2Connected(graph As Integer()()) As IList(Of Integer)()
            Dim stNumbering As Integer() = Orthogonal.STNumbering.stNumbering(graph)

            Return PlanarEmbedding.planarEmbedding2Connected(graph, stNumbering)
        End Function

        Public Shared Function planarEmbedding2Connected(graph As Integer()(), stNumbering As Integer()) As IList(Of Integer)()
            Dim upwardEmbedding As IList(Of Integer)() = PlanarEmbedding.planarUpwardEmbedding(graph, stNumbering)
            Return PlanarEmbedding.translateEmbeddingToNodeIndexes(PlanarEmbedding.extendUpwardEmbedding(upwardEmbedding), stNumbering)
        End Function

        ' This function assumes that the given graph is 2-connected
        ' It returns an embedding indexed by the st-numbers
        Public Shared Function planarUpwardEmbedding(graph As Integer()(), stNumbering As Integer()) As IList(Of Integer)()
            Dim n = graph.Length
            Dim embedding As IList(Of Integer)() = New List(Of Integer)(n - 1) {}
            Dim nodeParent As Dictionary(Of PQTree, Integer) = New Dictionary(Of PQTree, Integer)()
            Dim nodeWithNumber = New Integer(n - 1) {}
            Dim s = -1
            Dim t = -1
            For i = 0 To n - 1
                If stNumbering(i) = 1 Then
                    s = i
                End If
                If stNumbering(i) = n Then
                    t = i
                End If
                nodeWithNumber(stNumbering(i) - 1) = i
            Next

            ' Create the initial PQ-tree as a P node (representing "s") with all the connections of "s" as leaves:
            Dim pqTree As PQTree = New PQTree(stNumbering(s), PQTree.P_NODE, Nothing)
            For i = 0 To graph.Length - 1
                If graph(s)(i) = 1 AndAlso stNumbering(i) > stNumbering(s) Then
                    Dim leaf As PQTree = New PQTree(stNumbering(i), PQTree.LEAF_NODE, pqTree)
                    nodeParent(leaf) = stNumbering(s)
                End If
            Next

            If PlanarEmbedding.DEBUG >= 1 Then
                Console.WriteLine("PQ-tree at the beggining:" & vbLf & pqTree.toString(2, nodeParent))
            End If

            For i = 1 To n - 1
                Dim vNumber = i + 1
                Dim vNode = nodeWithNumber(i)

                If PlanarEmbedding.DEBUG >= 1 Then
                    Console.WriteLine(vbLf & "- Next is node " & vNode.ToString() & " with st-number " & vNumber.ToString())
                End If

                ' reduction step: 
                ' - "gather" all the virtual vertices that have index vNumber+1
                '   (this means that all the vertices with index "nNumber" should 
                '    appear consecutively as leaves in the PQ-tree)
                '   (if the set of nodes with "vNumber" is "S", this operation corresponds to REDUCE(T,S))
                If Not pqTree.reduce(vNumber) Then
                    If PlanarEmbedding.DEBUG >= 1 Then
                        Console.WriteLine("Cannot reduce the PQ-tree!!!")
                    End If
                    If PlanarEmbedding.DEBUG >= 1 Then
                        Console.WriteLine(pqTree.toString(nodeParent))
                    End If
                    Return Nothing
                End If

                If PlanarEmbedding.DEBUG >= 1 Then
                    Console.WriteLine("  PQ-tree after reduction:" & vbLf & pqTree.toString(2, nodeParent))
                End If

                ' Vertex addition step:
                ' - get the pertiment root (what I used to call the "fullParent"
                ' - get all the "pertinent leaves" (full leaves) + direction indicators: l1, ..., lk
                Dim fullNodes As IList(Of PQTree) = New List(Of PQTree)()
                Dim pertinentLeaves As IList(Of PQTree) = New List(Of PQTree)()
                Dim directionIndicators As List(Of PQTree) = New List(Of PQTree)()
                Dim fullParent As PQTree = Nothing
                Dim pertinentRoot As PQTree = Nothing
                If True Then
                    Dim stack As List(Of PQTree) = New List(Of PQTree)()
                    stack.Add(pqTree)
                    While stack.Count > 0
                        Dim current As PQTree = stack.PopAt(0)
                        If current.nodeType <> PQTree.DIRECTION_INDICATOR AndAlso pertinentRoot Is Nothing AndAlso (current.label = PQTree.LABEL_FULL OrElse current.label = PQTree.LABEL_PARTIAL) Then
                            pertinentRoot = current
                        End If
                        If current.label = PQTree.LABEL_FULL Then
                            If current.nodeType <> PQTree.DIRECTION_INDICATOR AndAlso fullParent Is Nothing Then
                                fullParent = current.parent
                                While fullParent IsNot Nothing AndAlso fullParent.label = PQTree.LABEL_FULL
                                    fullParent = fullParent.parent
                                End While
                            End If
                            If current.nodeType <> PQTree.DIRECTION_INDICATOR Then
                                fullNodes.Add(current)
                            End If
                            If current.nodeType = PQTree.LEAF_NODE Then
                                pertinentLeaves.Add(current)
                            ElseIf current.nodeType = PQTree.DIRECTION_INDICATOR Then
                                directionIndicators.Add(current)
                            End If
                        End If
                        If current.children IsNot Nothing Then
                            Dim insertPoint = 0
                            For Each child As PQTree In current.children
                                stack.Insert(insertPoint, child)
                                insertPoint += 1
                            Next
                        End If
                    End While
                End If
                ' refine the pertinent root:
                '            if (pertinentRoot!=null) 
                If True Then
                    Dim repeat As Boolean
                    Do
                        repeat = False
                        Dim nonEmptyChild As PQTree = Nothing
                        If pertinentRoot.children IsNot Nothing Then
                            For Each child As PQTree In pertinentRoot.children
                                If child.nodeType = PQTree.DIRECTION_INDICATOR Then
                                    Continue For
                                End If
                                If child.label <> PQTree.LABEL_EMPTY Then
                                    If nonEmptyChild Is Nothing Then
                                        nonEmptyChild = child
                                    Else
                                        nonEmptyChild = Nothing
                                        Exit For
                                    End If
                                End If
                            Next
                            If nonEmptyChild IsNot Nothing Then
                                pertinentRoot = nonEmptyChild
                                repeat = True
                            End If
                        End If
                    Loop While repeat
                    Dim toDelete As IList(Of PQTree) = New List(Of PQTree)()
                    For Each indicator As PQTree In directionIndicators
                        If Not pertinentRoot.contains(indicator) Then
                            toDelete.Add(indicator)
                        End If
                    Next

                    directionIndicators.RemoveAll(toDelete)
                End If
                If PlanarEmbedding.DEBUG >= 1 Then
                    Console.WriteLine("  full parent: " & fullParent.ToString())
                    Console.WriteLine("  full nodes: " & fullNodes.ToString())
                    Console.WriteLine("  pertinentRoot: " & pertinentRoot.ToString())
                    Console.WriteLine("  pertinent leaves: " & pertinentLeaves.ToString())
                    Console.WriteLine("  direction indicators: " & directionIndicators.ToString())
                End If

                ' - add l1, ..., lk to the upward embedding
                embedding(vNumber - 1) = New List(Of Integer)()
                For Each leaf As PQTree In pertinentLeaves
                    If leaf.nodeIndex = vNumber Then
                        embedding(CInt(vNumber - 1)).Add(nodeParent(leaf))
                    End If
                Next


                ' - if pertinent root is full, replace it by a P-node
                '   else: - add a direction indicator with lavel 'vNumber' directed from lk to l1 to perinent root
                '         - replace all full leaves by a P-node
                ' - add all the virtual vertices to the vNumber vertices
                ' Create the new P-node:
                ' - add all the virtual vertices to the vNumber vertices
                Dim PNode As PQTree = New PQTree(vNumber, PQTree.P_NODE, Nothing)
                For j = 0 To graph.Length - 1
                    If graph(vNode)(j) = 1 AndAlso stNumbering(j) > stNumbering(vNode) Then
                        Dim leaf As PQTree = New PQTree(stNumbering(j), PQTree.LEAF_NODE, PNode)
                        nodeParent(leaf) = stNumbering(vNode)
                    End If
                Next
                If PlanarEmbedding.DEBUG >= 1 Then
                    Console.WriteLine("  new P-node will have " & PNode.children.Count.ToString() & " children.")
                End If
                If fullParent Is Nothing Then
                    pqTree = PNode
                    For Each di2 As PQTree In directionIndicators
                        If di2.direction = PQTree.DIRECTION_INDICATOR_RIGHT Then
                            '                        if (di2.direction == PQTree.DIRECTION_INDICATOR_LEFT) {
                            If PlanarEmbedding.DEBUG >= 1 Then
                                Console.WriteLine("reversing the upward embedding because of: " & di2.ToString())
                            End If
                            ' reverse the upward embedding:
                            Dim tmp As IList(Of Integer) = New List(Of Integer)()
                            CType(tmp, List(Of Integer)).AddRange(embedding(di2.nodeIndex - 1))
                            embedding(CInt(di2.nodeIndex - 1)).Clear()
                            For Each v In tmp
                                embedding(CInt(di2.nodeIndex - 1)).Insert(0, v)
                            Next
                        End If
                    Next
                Else
                    Dim insertionIndex = -1
                    ' fullParent.children.removeAll(directionIndicators);
                    Dim removedIndicators As IList(Of PQTree) = New List(Of PQTree)()
                    For Each tmp As PQTree In directionIndicators
                        If fullParent.recursivelyRemoveLeafNotThroughQNodes(tmp) Then
                            removedIndicators.Add(tmp)
                        Else
                            If fullNodes.Contains(tmp.parent) Then
                                removedIndicators.Add(tmp) ' it will be deleted, since all the fullNodes will be eliminated!
                            End If
                        End If
                    Next
                    If PlanarEmbedding.DEBUG >= 1 Then
                        Console.WriteLine("Removed direction indicators: " & removedIndicators.ToString())
                    End If
                    For Each node As PQTree In fullNodes
                        Dim idx As Integer = fullParent.children.IndexOf(node)
                        If idx <> -1 AndAlso (insertionIndex = -1 OrElse idx < insertionIndex) Then
                            insertionIndex = idx
                        End If
                    Next
                    If PlanarEmbedding.DEBUG >= 1 Then
                        Console.WriteLine("Insertion index of new P node is: " & insertionIndex.ToString())
                    End If

                    fullParent.children.RemoveAll(fullNodes)
                    If PNode.children.Count = 1 Then
                        fullParent.children.Insert(insertionIndex, PNode.children(0))
                        PNode.children(0).parent = fullParent
                    Else
                        fullParent.children.Insert(insertionIndex, PNode)
                        PNode.parent = fullParent
                    End If
                    If pertinentRoot.label <> PQTree.LABEL_FULL Then
                        If pertinentRoot IsNot fullParent Then
                            Throw New Exception("pertinentRoot is partial, but doesn't match with fullParent!!!")
                        End If

                        ' - add a direction indicator with label 'vNumber' directed from lj to l1 to perinent root
                        Dim di As PQTree = New PQTree(vNumber, PQTree.DIRECTION_INDICATOR, fullParent)
                        di.direction = PQTree.DIRECTION_INDICATOR_LEFT
                        If PlanarEmbedding.DEBUG >= 1 Then
                            Console.WriteLine("*** direction indicator added ***")
                        End If

                        ' - add the rest of direction indicators:
                        For Each di2 As PQTree In removedIndicators
                            di2.parent = fullParent
                            fullParent.children.Add(di2)
                        Next
                    Else
                        For Each di2 As PQTree In directionIndicators
                            If pertinentRoot.contains(di2) Then
                                If di2.direction = PQTree.DIRECTION_INDICATOR_RIGHT Then
                                    '                        if (di2.direction == PQTree.DIRECTION_INDICATOR_LEFT) {
                                    If PlanarEmbedding.DEBUG >= 1 Then
                                        Console.WriteLine("reversing the upward embedding because of: " & di2.ToString())
                                    End If
                                    ' reverse the upward embedding:
                                    Dim tmp As IList(Of Integer) = New List(Of Integer)()
                                    CType(tmp, List(Of Integer)).AddRange(embedding(di2.nodeIndex - 1))
                                    embedding(CInt(di2.nodeIndex - 1)).Clear()
                                    For Each v In tmp
                                        embedding(CInt(di2.nodeIndex - 1)).Insert(0, v)
                                    Next
                                End If
                            Else
                                Console.Error.WriteLine("PlanarEmbering: this should not have hapened!")
                                ' add the indicators that we should not have removed back:
                                '                            if (removedIndicators.contains(di2)) {
                                '                                fullParent.children.add(di2);
                                '                            }
                            End If
                        Next
                    End If

                End If

                If PlanarEmbedding.DEBUG >= 1 Then
                    Console.WriteLine("PQ-tree after insertion of the new P-node:" & vbLf & pqTree.toString(2, nodeParent))
                End If
            Next

            Return embedding
        End Function


        Public Shared Function translateEmbeddingToNodeIndexes(embedding As IList(Of Integer)(), stNumbering As Integer()) As IList(Of Integer)()
            If embedding Is Nothing Then
                Return Nothing
            End If
            Dim n = embedding.Length
            Dim nodeWithNumber = New Integer(n - 1) {}
            For i = 0 To n - 1
                nodeWithNumber(stNumbering(i) - 1) = i
            Next
            Dim translated As IList(Of Integer)() = New List(Of Integer)(n - 1) {}


            For i = 0 To n - 1
                Dim v = nodeWithNumber(i)
                translated(v) = New List(Of Integer)()
                If embedding(i) IsNot Nothing Then
                    For Each stNumber In embedding(i)
                        If PlanarEmbedding.DEBUG >= 1 Then
                            Console.WriteLine("translateEmbeddingToNodeIndexes: " & stNumber.ToString() & " -> " & nodeWithNumber(stNumber - 1).ToString())
                        End If
                        translated(v).Add(nodeWithNumber(stNumber - 1))
                    Next
                End If
            Next

            Return translated
        End Function


        ' This function indexes the nodes by st-numbering:
        Public Shared Function extendUpwardEmbedding(upwardEmbedding As IList(Of Integer)()) As IList(Of Integer)()
            If upwardEmbedding Is Nothing Then
                Return Nothing
            End If

            ' copy 'upwardEmbedding' onto 'embedding':
            Dim n = upwardEmbedding.Length
            Dim embedding As IList(Of Integer)() = New List(Of Integer)(n - 1) {}
            Dim newNode = New Boolean(n - 1) {}
            For i = 0 To n - 1
                newNode(i) = True
                embedding(i) = New List(Of Integer)()
                If upwardEmbedding(i) IsNot Nothing Then
                    CType(embedding(i), List(Of Integer)).AddRange(upwardEmbedding(i))
                End If
            Next

            ' since nodes are indexed by st-numbering, passing 'n' means starting with t:
            PlanarEmbedding.DFS(upwardEmbedding, embedding, newNode, n)
            '        DFS(embedding, newNode, n);

            Return embedding
        End Function


        Public Shared Sub DFS(Au As IList(Of Integer)(), A As IList(Of Integer)(), newNode As Boolean(), y As Integer)
            newNode(y - 1) = False
            If Au(y - 1) IsNot Nothing Then
                For Each v As Integer? In Au(y - 1)
                    A(v.Value - 1).Insert(0, y)
                    '                A[v-1].add(y);
                    If newNode(v.Value - 1) Then
                        PlanarEmbedding.DFS(Au, A, newNode, v.Value)
                    End If
                Next
            End If
        End Sub


        ' this method finds the faces of an embedding:
        ' It returns a list of faces, where each face is represented as the list of vertices,
        ' in a clock-wise order, and starting with the lowest indexed node
        Public Shared Function faces(embedding As IList(Of Integer)()) As IList(Of IList(Of Integer))
            Dim n = embedding.Length
            Dim lFaces As IList(Of IList(Of Integer)) = New List(Of IList(Of Integer))()

            For v1 = 0 To n - 1
                For Each v2 In embedding(v1)
                    Dim face As List(Of Integer) = New List(Of Integer)()
                    face.Add(v1)
                    face.Add(v2)
                    PlanarEmbedding.getFace(embedding, face)
                    ' rotate the face until we have the smallest element first:
                    Dim smallest = -1
                    For Each v As Integer? In face
                        If smallest = -1 OrElse v.Value < smallest Then
                            smallest = v.Value
                        End If
                    Next
                    While face(0) <> smallest
                        face.Add(face.PopAt(0))
                    End While
                    If Not lFaces.Contains(face) Then
                        lFaces.Add(face)
                    End If
                Next
            Next

            Return lFaces
        End Function


        Friend Shared Sub getFace(embedding As IList(Of Integer)(), face As IList(Of Integer)) ' , boolean clockwise) {
            Dim v_prev = face(face.Count - 2)
            Dim v = face(face.Count - 1)
            Dim idx = embedding(v).IndexOf(v_prev)
            Dim l = embedding(v).Count
            '        if (clockwise) {
            idx += 1
            If idx >= l Then
                idx = 0
            End If
            '        } else {
            '            idx--;
            '            if (idx<=0) idx=l-1;
            '        }
            Dim v_next = embedding(v)(idx)
            If face.Contains(v_next) Then
                Return
            End If
            face.Add(v_next)
            '        getFace(embedding, face, !clockwise);
            PlanarEmbedding.getFace(embedding, face)
        End Sub
    End Class

End Namespace
