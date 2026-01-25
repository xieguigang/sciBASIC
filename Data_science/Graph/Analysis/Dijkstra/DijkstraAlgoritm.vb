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

        ReadOnly graph As SparseMatrix
        ReadOnly vertices As Integer
        ReadOnly nodes As Node()

        ''' <param name="graph">The adjacency matrix of the graph.</param>
        ''' <param name="vertices">Total number of vertices.</param>
        Sub New(graph As SparseMatrix, vertices As Integer)
            Me.graph = graph
            Me.vertices = vertices
            Me.nodes = New Node(vertices - 1) {}

            ' 初始化节点
            For i As Integer = 0 To vertices - 1
                nodes(i) = New Node(i, isStartNode:=True)
            Next
        End Sub

        ''' <summary>
        ''' Calculates the minimum possible distance from root node to all other nodes.
        ''' </summary>
        ''' <param name="startIndex"></param>
        ''' <returns></returns>
        Public Function DistanceFinder(startIndex As Integer) As Node()
            Return DistanceFinderInternal(startIndex, endIndex:=Nothing)
        End Function

        ''' <summary>
        ''' Calculates the minimum possible distance from root node to all other nodes.
        ''' </summary>
        ''' <param name="startIndex"></param>
        ''' <returns></returns>
        Private Function DistanceFinderInternal(startIndex As Integer, endIndex As Integer?) As Node()
            Dim endIndexValue As Integer = If(endIndex.HasValue, endIndex.Value, -1)
            Dim currentNode As Node

            ' 使用循环代替递归，防止堆栈溢出
            While True
                ' 1. 寻找当前未确定的最短距离节点
                Dim candidates As List(Of Node) = (From v As Node
                                                   In nodes
                                                   Where Not v.IsFixed
                                                   Order By v.TotalDistance Ascending).ToList()

                ' 如果没有候选节点，或者所有剩余节点都是不可达的，则退出
                If candidates.Count = 0 Then
                    Exit While
                Else
                    currentNode = candidates.First
                End If

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
                ' 使用 Parallel.For 循环处理邻居更新
                System.Threading.Tasks.Parallel.For(0, vertices,
                    Sub(i)
                        Dim neighbor As Node = nodes(i)

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
                    End Sub)
            End While

            Return nodes
        End Function

        ''' <summary>
        ''' Calculates the shortest path using Bi-Directional Dijkstra algorithm.
        ''' </summary>
        ''' <param name="graph">The adjacency matrix of the graph.</param>
        ''' <param name="vertices">Total number of vertices.</param>
        ''' <param name="startIndex">The starting node index.</param>
        ''' <param name="endIndex">The target node index.</param>
        ''' <returns>Returns the target Node containing path and distance info. Returns Nothing if path not found.</returns>
        Public Shared Function FindPathBiDirectional(ByRef graph As SparseMatrix, vertices As Integer, startIndex As Integer, endIndex As Integer) As Node
            ' 1. 初始化两个搜索方向的节点集合
            Dim forwardNodes() As Node = New Node(vertices - 1) {}
            Dim backwardNodes() As Node = New Node(vertices - 1) {}

            For i As Integer = 0 To vertices - 1
                ' 前向：起点为 startIndex
                forwardNodes(i) = New Node(i, i = startIndex)
                ' 后向：起点为 endIndex (即反方向搜索的起点)
                backwardNodes(i) = New Node(i, i = endIndex)
            Next

            Dim meetingNodeIndex As Integer = -1
            Dim minTotalDistance As Double = Double.MaxValue

            ' 2. 主循环：直到两个搜索都结束或找到最优解
            While True
                ' --- 步骤 A: 寻找前向搜索中未确定的最小节点 ---
                Dim fCandidates As List(Of Node) = (From n As Node In forwardNodes
                                                    Where Not n.IsFixed
                                                    Order By n.TotalDistance Ascending).ToList()

                Dim u_f As Node = If(fCandidates.Count > 0, fCandidates(0), Nothing)

                ' --- 步骤 B: 寻找后向搜索中未确定的最小节点 ---
                Dim bCandidates As List(Of Node) = (From n As Node In backwardNodes
                                                    Where Not n.IsFixed
                                                    Order By n.TotalDistance Ascending).ToList()

                Dim u_b As Node = If(bCandidates.Count > 0, bCandidates(0), Nothing)

                ' --- 步骤 C: 检查终止条件 ---
                ' 如果任意一方已经无路可走
                If (u_f Is Nothing OrElse u_f.TotalDistance = Integer.MaxValue) OrElse
                   (u_b Is Nothing OrElse u_b.TotalDistance = Integer.MaxValue) Then
                    Exit While
                End If

                ' 剪枝优化：如果当前两头的最短路径之和已经超过了目前找到的最短路径，则不可能有更优解了
                If u_f.TotalDistance + u_b.TotalDistance >= minTotalDistance Then
                    Exit While
                End If

                ' --- 步骤 D: 处理前向节点 u_f ---
                u_f.IsFixed = True

                ' 检查前向节点 u_f 是否在后向搜索中已经被处理过
                If backwardNodes(u_f.Index).IsFixed Then
                    Dim currentDist As Double = u_f.TotalDistance + backwardNodes(u_f.Index).TotalDistance
                    If currentDist < minTotalDistance Then
                        minTotalDistance = currentDist
                        meetingNodeIndex = u_f.Index
                    End If
                End If

                ' 前向松弛操作
                ' 注意：移除了 Parallel.For 以避免在双向搜索中发生复杂的线程竞争
                For i As Integer = 0 To vertices - 1
                    Dim neighbor As Node = forwardNodes(i)
                    ' 邻接矩阵 graph(u, v) 表示 u->v 的边
                    If Not neighbor.IsFixed AndAlso graph(u_f.Index, i) <> 0 Then
                        If u_f.TotalDistance <> Integer.MaxValue Then
                            Dim newDist As Double = u_f.TotalDistance + graph(u_f.Index, i)
                            If newDist < neighbor.TotalDistance Then
                                neighbor.TotalDistance = newDist
                                neighbor.Parent = u_f.Index
                                neighbor.Path.Clear()
                                neighbor.Path.AddRange(u_f.Path)
                                neighbor.Path.Add(u_f.Index)
                            End If
                        End If
                    End If
                Next

                ' --- 步骤 E: 处理后向节点 u_b ---
                u_b.IsFixed = True

                ' 检查后向节点 u_b 是否在前向搜索中已经被处理过
                If forwardNodes(u_b.Index).IsFixed Then
                    Dim currentDist As Double = u_b.TotalDistance + forwardNodes(u_b.Index).TotalDistance
                    If currentDist < minTotalDistance Then
                        minTotalDistance = currentDist
                        meetingNodeIndex = u_b.Index
                    End If
                End If

                ' 后向松弛操作
                ' 后向搜索时，我们需要寻找"指回"当前节点的节点，即查找 Graph(i, u_b) 有值的边
                For i As Integer = 0 To vertices - 1
                    Dim neighbor As Node = backwardNodes(i)
                    If Not neighbor.IsFixed AndAlso graph(i, u_b.Index) <> 0 Then
                        If u_b.TotalDistance <> Integer.MaxValue Then
                            Dim newDist As Double = u_b.TotalDistance + graph(i, u_b.Index)
                            If newDist < neighbor.TotalDistance Then
                                neighbor.TotalDistance = newDist
                                neighbor.Parent = u_b.Index
                                neighbor.Path.Clear()
                                neighbor.Path.AddRange(u_b.Path)
                                neighbor.Path.Add(u_b.Index)
                            End If
                        End If
                    End If
                Next
            End While

            ' 3. 构建结果
            If meetingNodeIndex = -1 Then Return Nothing

            ' 创建结果节点
            Dim resultNode As New Node(meetingNodeIndex)
            resultNode.TotalDistance = minTotalDistance

            ' 拼接路径：
            ' Part 1: Start -> Meeting Point (来自 forwardNodes)
            Dim pathStartToMeet As List(Of Integer) = New List(Of Integer)(forwardNodes(meetingNodeIndex).Path)
            pathStartToMeet.Add(meetingNodeIndex) ' 添加交点本身

            ' Part 2: Meeting Point -> End (来自 backwardNodes)
            ' backwardNodes 记录的是 End -> Meeting Point，需要反转
            Dim pathEndToMeet As List(Of Integer) = backwardNodes(meetingNodeIndex).Path
            pathEndToMeet.Reverse()

            ' 合并
            resultNode.Path.AddRange(pathStartToMeet)
            resultNode.Path.AddRange(pathEndToMeet)

            Return resultNode
        End Function

        ''' <summary>
        ''' Calculates the shortest path from startIndex to endIndex specifically.
        ''' </summary>
        ''' <param name="startIndex">The starting node index.</param>
        ''' <param name="endIndex">The target node index.</param>
        ''' <returns>Returns the target Node containing path and distance info. Returns Nothing if path not found.</returns>
        Public Function FindPath(startIndex As Integer, endIndex As Integer) As Node
            Dim find = DistanceFinderInternal(startIndex, endIndex)
            Dim targetNode As Node = find(endIndex)
            Return targetNode
        End Function
    End Class
End Namespace