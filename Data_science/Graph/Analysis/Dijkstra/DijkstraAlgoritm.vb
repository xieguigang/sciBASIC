Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix

Namespace Analysis.Dijkstra

    Public Class DijkstraAlgoritm

        ''' <summary>
        ''' Holds all the details of a node in the graph.
        ''' </summary>
        Public Class Node

            Public Property Index As Integer
            Public Property Parent As Integer
            Public Property TotalDistance As Double
            Public Property IsFixed As Boolean
            Public Property Path As List(Of Integer)

            Public ReadOnly Property PathToString As String
                Get
                    Return String.Join("->", Path.Select(Function(i) (i + 1).ToString()).ToArray()) & "->" & (Index + 1).ToString()
                End Get
            End Property

            Public Sub New(index As Integer, Optional isStartNode As Boolean = False)
                Me.Index = index
                IsFixed = False
                TotalDistance = If(isStartNode, 0, Integer.MaxValue)
                Parent = -1
                Path = New List(Of Integer)()
            End Sub

        End Class

        ''' <summary>
        ''' Calculates the minimum possible distance from root node to all other nodes.
        ''' </summary>
        ''' <param name="graph"></param>
        ''' <param name="startIndex"></param>
        ''' <returns></returns>
        Public Shared Function DistanceFinder(ByRef graph As SparseMatrix, vertices As Integer, startIndex As Integer) As Node()
            Return DistanceFinderInternal(graph, vertices, startIndex, endIndex:=Nothing)
        End Function

        ''' <summary>
        ''' Calculates the minimum possible distance from root node to all other nodes.
        ''' </summary>
        ''' <param name="graph"></param>
        ''' <param name="startIndex"></param>
        ''' <returns></returns>
        Private Shared Function DistanceFinderInternal(ByRef graph As SparseMatrix, vertices As Integer, startIndex As Integer, endIndex As Integer?) As Node()
            Dim nodes As Node() = New Node(vertices - 1) {}
            Dim endIndexValue As Integer = If(endIndex.HasValue, endIndex.Value, -1)

            ' 初始化节点
            For i As Integer = 0 To vertices - 1
                nodes(i) = New Node(i, i = startIndex)
            Next

            ' 使用循环代替递归，防止堆栈溢出
            While True
                ' 1. 寻找当前未确定的最短距离节点
                Dim candidates = nodes.Where(Function(n) Not n.IsFixed).ToList()

                ' 如果没有候选节点，或者所有剩余节点都是不可达的，则退出
                If candidates.Count = 0 Then
                    Exit While
                End If

                Dim currentNode As Node = Nothing
                Dim minDist As Double = Integer.MaxValue

                For Each n As Node In candidates
                    If n.TotalDistance < minDist Then
                        minDist = n.TotalDistance
                        currentNode = n
                    End If
                Next

                ' 如果剩下的最小距离是无穷大，说明剩余节点都不可达，退出
                If currentNode Is Nothing OrElse currentNode.TotalDistance = Integer.MaxValue Then
                    Exit While
                ElseIf endIndex IsNot Nothing AndAlso currentNode.Index = endIndexValue Then
                    ' 如果当前确定的节点就是目标终点，说明我们已经找到了最短路径，直接退出循环
                    ' 找到终点，先标记为 Fixed，再退出
                    currentNode.IsFixed = True
                    Exit While
                End If

                ' 2. 标记该节点为已确定
                currentNode.IsFixed = True

                ' 3. 松弛操作：更新所有邻居的距离
                For Each neighbor As Node In nodes
                    If Not neighbor.IsFixed AndAlso graph(currentNode.Index, neighbor.Index) <> 0 Then
                        ' 检查防溢出：如果当前节点距离已经是Max，则不更新
                        If currentNode.TotalDistance <> Integer.MaxValue Then
                            Dim newDist As Double = currentNode.TotalDistance + graph(currentNode.Index, neighbor.Index)

                            If newDist < neighbor.TotalDistance Then
                                neighbor.TotalDistance = newDist
                                neighbor.Parent = currentNode.Index
                                ' 更新路径
                                neighbor.Path.Clear()
                                neighbor.Path.AddRange(currentNode.Path)
                                neighbor.Path.Add(currentNode.Index)
                            End If
                        End If
                    End If
                Next
            End While

            Return nodes
        End Function

        ''' <summary>
        ''' Calculates the shortest path from startIndex to endIndex specifically.
        ''' </summary>
        ''' <param name="graph">The adjacency matrix of the graph.</param>
        ''' <param name="vertices">Total number of vertices.</param>
        ''' <param name="startIndex">The starting node index.</param>
        ''' <param name="endIndex">The target node index.</param>
        ''' <returns>Returns the target Node containing path and distance info. Returns Nothing if path not found.</returns>
        Public Shared Function FindPath(ByRef graph As SparseMatrix, vertices As Integer, startIndex As Integer, endIndex As Integer) As Node
            Dim find = DistanceFinderInternal(graph, vertices, startIndex, endIndex)
            Dim targetNode As Node = find(endIndex)
            Return targetNode
        End Function
    End Class
End Namespace