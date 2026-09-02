#Region "Microsoft.VisualBasic::632901cccbb11f1c01e7581d451ce780, Data_science\Graph\Analysis\ContractionHierarchies.vb"

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

    '   Total Lines: 335
    '    Code Lines: 273 (81.49%)
    ' Comment Lines: 5 (1.49%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 57 (17.01%)
    '     File Size: 15.24 KB


    '     Class Distance
    ' 
    ' 
    ' 
    '     Class Processed
    ' 
    ' 
    ' 
    '     Class Vertex
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '     Class PQIMPcomparator
    ' 
    '         Function: Compare
    ' 
    '     Class PriorityQueueComp
    ' 
    '         Function: Compare
    ' 
    '     Class forwComparator
    ' 
    '         Function: Compare
    ' 
    '     Class revComparator
    ' 
    '         Function: Compare
    ' 
    '     Class PreProcess
    ' 
    '         Function: checkId, preProcess, processing
    ' 
    '         Sub: calNeighbors, (+2 Overloads) computeImportance, contractNode, dijkstra, relaxEdges
    ' 
    '     Class BidirectionalDijkstra
    ' 
    '         Function: computeDist
    ' 
    '         Sub: relaxEdges
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection

Namespace Analysis.ContractionHierarchies

    ' ========== 辅助数据类 ==========
    Public Class Distance
        Public contractId As Integer = -1
        Public sourceId As Integer = -1
        Public distance As Long = Integer.MaxValue
        Public forwqueryId As Integer = -1
        Public revqueryId As Integer = -1
        Public queryDist As Long = Integer.MaxValue
        Public revDistance As Long = Integer.MaxValue
    End Class

    Public Class Processed
        Public forwProcessed As Boolean = False
        Public revProcessed As Boolean = False
        Public forwqueryId As Integer = -1
        Public revqueryId As Integer = -1
    End Class

    Public Class Vertex
        Public vertexNum As Integer
        Public inEdges As List(Of Integer) = New List(Of Integer)()
        Public inECost As List(Of Long) = New List(Of Long)()
        Public outEdges As List(Of Integer) = New List(Of Integer)()
        Public outECost As List(Of Long) = New List(Of Long)()
        Public orderPos As Integer = 0
        Public contracted As Boolean = False
        Public distance As Distance = New Distance()
        Public processed As Processed = New Processed()
        Public edgeDiff As Integer = 0
        Public delNeighbors As Long = 0
        Public shortcutCover As Integer = 0
        Public importance As Long = 0

        Public Sub New()
        End Sub

        Public Sub New(vertexNum As Integer)
            Me.vertexNum = vertexNum
        End Sub
    End Class

    ' ========== 比较器 ==========
    Public Class PQIMPcomparator
        Implements IComparer(Of Vertex)

        Public Function Compare(x As Vertex, y As Vertex) As Integer Implements IComparer(Of Vertex).Compare
            If x.importance > y.importance Then Return 1
            If x.importance < y.importance Then Return -1
            Return 0
        End Function
    End Class

    Public Class PriorityQueueComp
        Implements IComparer(Of Vertex)

        Public Function Compare(x As Vertex, y As Vertex) As Integer Implements IComparer(Of Vertex).Compare
            If x.distance.distance > y.distance.distance Then Return 1
            If x.distance.distance < y.distance.distance Then Return -1
            Return 0
        End Function
    End Class

    Public Class forwComparator
        Implements IComparer(Of Vertex)

        Public Function Compare(x As Vertex, y As Vertex) As Integer Implements IComparer(Of Vertex).Compare
            If x.distance.queryDist > y.distance.queryDist Then Return 1
            If x.distance.queryDist < y.distance.queryDist Then Return -1
            Return 0
        End Function
    End Class

    Public Class revComparator
        Implements IComparer(Of Vertex)

        Public Function Compare(x As Vertex, y As Vertex) As Integer Implements IComparer(Of Vertex).Compare
            If x.distance.revDistance > y.distance.revDistance Then Return 1
            If x.distance.revDistance < y.distance.revDistance Then Return -1
            Return 0
        End Function
    End Class

    ' ========== 预处理类 ==========
    Public Class PreProcess
        Private comp As IComparer(Of Vertex) = New PQIMPcomparator()
        Private PQImp As PriorityQueue(Of Vertex)

        Private PQcomp As IComparer(Of Vertex) = New PriorityQueueComp()
        Private queue As PriorityQueue(Of Vertex)

        Private Sub computeImportance(graph As Vertex())
            PQImp = New PriorityQueue(Of Vertex)(comp)
            For i As Integer = 0 To graph.Length - 1
                graph(i).edgeDiff = (graph(i).inEdges.Count * graph(i).outEdges.Count) - graph(i).inEdges.Count - graph(i).outEdges.Count
                graph(i).shortcutCover = graph(i).inEdges.Count + graph(i).outEdges.Count
                graph(i).importance = graph(i).edgeDiff * 14 + graph(i).shortcutCover * 25 + graph(i).delNeighbors * 10
                PQImp.push(graph(i))
            Next
        End Sub

        Private Sub computeImportance(graph As Vertex(), vertex As Vertex)
            vertex.edgeDiff = (vertex.inEdges.Count * vertex.outEdges.Count) - vertex.inEdges.Count - vertex.outEdges.Count
            vertex.shortcutCover = vertex.inEdges.Count + vertex.outEdges.Count
            vertex.importance = vertex.edgeDiff * 14 + vertex.shortcutCover * 25 + vertex.delNeighbors * 10
        End Sub

        Private Function preProcess(graph As Vertex()) As Integer()
            Dim nodeOrdering As Integer() = New Integer(graph.Length - 1) {}
            Dim extractNum As Integer = 0

            While PQImp.count() > 0
                Dim vertex As Vertex = PQImp.poll()
                computeImportance(graph, vertex)

                If PQImp.count() > 0 AndAlso vertex.importance > PQImp.Peek().importance Then
                    PQImp.push(vertex)
                    Continue While
                End If

                nodeOrdering(extractNum) = vertex.vertexNum
                vertex.orderPos = extractNum
                extractNum += 1

                contractNode(graph, vertex, extractNum - 1)
            End While

            Return nodeOrdering
        End Function

        Private Sub calNeighbors(graph As Vertex(), inEdges As List(Of Integer), outEdges As List(Of Integer))
            For Each temp As Integer In inEdges
                graph(temp).delNeighbors += 1
            Next
            For Each temp As Integer In outEdges
                graph(temp).delNeighbors += 1
            Next
        End Sub

        Private Sub contractNode(graph As Vertex(), vertex As Vertex, contractId As Integer)
            Dim inEdges As List(Of Integer) = vertex.inEdges
            Dim inECost As List(Of Long) = vertex.inECost
            Dim outEdges As List(Of Integer) = vertex.outEdges
            Dim outECost As List(Of Long) = vertex.outECost

            vertex.contracted = True

            Dim inMax As Long = 0
            Dim outMax As Long = 0

            calNeighbors(graph, vertex.inEdges, vertex.outEdges)

            For i As Integer = 0 To inECost.Count - 1
                If graph(inEdges(i)).contracted Then Continue For
                If inMax < inECost(i) Then inMax = inECost(i)
            Next
            For i As Integer = 0 To outECost.Count - 1
                If graph(outEdges(i)).contracted Then Continue For
                If outMax < outECost(i) Then outMax = outECost(i)
            Next

            Dim max As Long = inMax + outMax

            For i As Integer = 0 To inEdges.Count - 1
                Dim inVertex As Integer = inEdges(i)
                If graph(inVertex).contracted Then Continue For
                Dim incost As Long = inECost(i)

                dijkstra(graph, inVertex, max, contractId, i)

                For j As Integer = 0 To outEdges.Count - 1
                    Dim outVertex As Integer = outEdges(j)
                    Dim outcost As Long = outECost(j)
                    If graph(outVertex).contracted Then Continue For
                    If graph(outVertex).distance.contractId <> contractId OrElse graph(outVertex).distance.sourceId <> i OrElse graph(outVertex).distance.distance > incost + outcost Then
                        graph(inVertex).outEdges.Add(outVertex)
                        graph(inVertex).outECost.Add(incost + outcost)
                        graph(outVertex).inEdges.Add(inVertex)
                        graph(outVertex).inECost.Add(incost + outcost)
                    End If
                Next
            Next
        End Sub

        ' 修正后的迪杰斯特拉（移除 i>3 截断）
        Private Sub dijkstra(graph As Vertex(), source As Integer, maxcost As Long, contractId As Integer, sourceId As Integer)
            queue = New PriorityQueue(Of Vertex)(PQcomp)

            graph(source).distance.distance = 0
            graph(source).distance.contractId = contractId
            graph(source).distance.sourceId = sourceId

            queue.clear()
            queue.push(graph(source))

            While queue.count() > 0
                Dim vertex As Vertex = queue.poll()
                If vertex.distance.distance > maxcost Then
                    Return
                End If
                relaxEdges(graph, vertex.vertexNum, contractId, queue, sourceId)
            End While
        End Sub

        Private Sub relaxEdges(graph As Vertex(), vertex As Integer, contractId As Integer, queue As PriorityQueue(Of Vertex), sourceId As Integer)
            Dim vertexList As List(Of Integer) = graph(vertex).outEdges
            Dim costList As List(Of Long) = graph(vertex).outECost

            For i As Integer = 0 To vertexList.Count - 1
                Dim temp As Integer = vertexList(i)
                Dim cost As Long = costList(i)
                If graph(temp).contracted Then Continue For
                If checkId(graph, vertex, temp) OrElse graph(temp).distance.distance > graph(vertex).distance.distance + cost Then
                    graph(temp).distance.distance = graph(vertex).distance.distance + cost
                    graph(temp).distance.contractId = contractId
                    graph(temp).distance.sourceId = sourceId

                    queue.remove(graph(temp))
                    queue.push(graph(temp))
                End If
            Next
        End Sub

        Private Function checkId(graph As Vertex(), source As Integer, target As Integer) As Boolean
            Return graph(source).distance.contractId <> graph(target).distance.contractId OrElse
                   graph(source).distance.sourceId <> graph(target).distance.sourceId
        End Function

        Public Function processing(graph As Vertex()) As Integer()
            computeImportance(graph)
            Dim nodeOrdering As Integer() = preProcess(graph)
            Return nodeOrdering
        End Function
    End Class

    ' ========== 双向迪杰斯特拉查询类 ==========
    Public Class BidirectionalDijkstra
        Private forwComp As IComparer(Of Vertex) = New forwComparator()
        Private revComp As IComparer(Of Vertex) = New revComparator()
        Private forwQ As PriorityQueue(Of Vertex)
        Private revQ As PriorityQueue(Of Vertex)

        Public Function computeDist(graph As Vertex(), source As Integer, target As Integer, queryID As Integer, nodeOrdering As Integer()) As Long
            graph(source).distance.queryDist = 0
            graph(source).distance.forwqueryId = queryID
            graph(source).processed.forwqueryId = queryID

            graph(target).distance.revDistance = 0
            graph(target).distance.revqueryId = queryID
            graph(target).processed.revqueryId = queryID

            forwQ = New PriorityQueue(Of Vertex)(forwComp)
            revQ = New PriorityQueue(Of Vertex)(revComp)

            forwQ.push(graph(source))
            revQ.push(graph(target))

            Dim estimate As Long = Long.MaxValue

            While forwQ.count() > 0 OrElse revQ.count() > 0
                If forwQ.count() > 0 Then
                    Dim vertex1 As Vertex = forwQ.poll()
                    If vertex1.distance.queryDist <= estimate Then
                        relaxEdges(graph, vertex1.vertexNum, "f", nodeOrdering, queryID)
                    End If
                    If vertex1.processed.revqueryId = queryID AndAlso vertex1.processed.revProcessed Then
                        If vertex1.distance.queryDist + vertex1.distance.revDistance < estimate Then
                            estimate = vertex1.distance.queryDist + vertex1.distance.revDistance
                        End If
                    End If
                End If

                If revQ.count() > 0 Then
                    Dim vertex2 As Vertex = revQ.poll()
                    If vertex2.distance.revDistance <= estimate Then
                        relaxEdges(graph, vertex2.vertexNum, "r", nodeOrdering, queryID)
                    End If
                    If vertex2.processed.forwqueryId = queryID AndAlso vertex2.processed.forwProcessed Then
                        If vertex2.distance.revDistance + vertex2.distance.queryDist < estimate Then
                            estimate = vertex2.distance.revDistance + vertex2.distance.queryDist
                        End If
                    End If
                End If
            End While

            If estimate = Long.MaxValue Then Return -1
            Return estimate
        End Function

        Private Sub relaxEdges(graph As Vertex(), vertex As Integer, str As String, nodeOrdering As Integer(), queryId As Integer)
            If str = "f" Then
                Dim vertexList As List(Of Integer) = graph(vertex).outEdges
                Dim costList As List(Of Long) = graph(vertex).outECost
                graph(vertex).processed.forwProcessed = True
                graph(vertex).processed.forwqueryId = queryId

                For i As Integer = 0 To vertexList.Count - 1
                    Dim temp As Integer = vertexList(i)
                    Dim cost As Long = costList(i)
                    If graph(vertex).orderPos < graph(temp).orderPos Then
                        If graph(vertex).distance.forwqueryId <> graph(temp).distance.forwqueryId OrElse graph(temp).distance.queryDist > graph(vertex).distance.queryDist + cost Then
                            graph(temp).distance.forwqueryId = graph(vertex).distance.forwqueryId
                            graph(temp).distance.queryDist = graph(vertex).distance.queryDist + cost

                            forwQ.remove(graph(temp))
                            forwQ.push(graph(temp))
                        End If
                    End If
                Next
            Else
                Dim vertexList As List(Of Integer) = graph(vertex).inEdges
                Dim costList As List(Of Long) = graph(vertex).inECost
                graph(vertex).processed.revProcessed = True
                graph(vertex).processed.revqueryId = queryId

                For i As Integer = 0 To vertexList.Count - 1
                    Dim temp As Integer = vertexList(i)
                    Dim cost As Long = costList(i)
                    If graph(vertex).orderPos < graph(temp).orderPos Then
                        If graph(vertex).distance.revqueryId <> graph(temp).distance.revqueryId OrElse graph(temp).distance.revDistance > graph(vertex).distance.revDistance + cost Then
                            graph(temp).distance.revqueryId = graph(vertex).distance.revqueryId
                            graph(temp).distance.revDistance = graph(vertex).distance.revDistance + cost

                            revQ.remove(graph(temp))
                            revQ.push(graph(temp))
                        End If
                    End If
                Next
            End If
        End Sub
    End Class
End Namespace
