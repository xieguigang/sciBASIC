Imports Microsoft.VisualBasic.Data.GraphTheory.Network
Imports stdNum = System.Math

Namespace Analysis.Louvain

    ''' <summary>
    ''' A fast algorithm To find communities In large network
    ''' </summary>
    ''' <remarks>
    ''' Blondel V D, Guillaume J L, Lambiotte R, et al. Fast unfolding of communities in large networks[J]. Journal of Statistical Mechanics, 2008, 2008(10)155-168.
    ''' </remarks>
    Public Class LouvainCommunity

        ''' <summary>
        ''' number of vertex
        ''' </summary>
        Friend n As Integer
        ''' <summary>
        ''' number of edges
        ''' </summary>
        Friend m As Integer
        ''' <summary>
        ''' community
        ''' </summary>
        Friend cluster As Integer()
        ''' <summary>
        ''' 邻接表
        ''' </summary>
        Friend edge As Edge()
        ''' <summary>
        ''' 头节点下标
        ''' </summary>
        Friend head As Integer()
        ''' <summary>
        ''' 已用E的个数
        ''' </summary>
        Friend top As Integer
        ''' <summary>
        ''' 1/2m 全局不变
        ''' </summary>
        Friend resolution As Double
        ''' <summary>
        ''' 节点的权重值
        ''' </summary>
        Friend node_weight As Double()
        ''' <summary>
        ''' 总边权值
        ''' </summary>
        Friend totalEdgeWeight As Double
        ''' <summary>
        ''' 簇的权值
        ''' </summary>
        Friend cluster_weight As Double()
        ''' <summary>
        ''' 误差
        ''' </summary>
        Friend eps As Double = 0.00000000000001
        ''' <summary>
        ''' 最初始的n
        ''' </summary>
        Friend global_n As Integer
        ''' <summary>
        ''' 最后的结果，i属于哪个簇
        ''' </summary>
        Friend global_cluster As Integer()
        ''' <summary>
        ''' 新的邻接表
        ''' </summary>
        Friend new_edge As Edge()
        Friend new_head As Integer()
        Friend new_top As Integer = 0
        ''' <summary>
        ''' 最大迭代次数
        ''' </summary>
        Friend ReadOnly iteration_time As Integer = 3
        ''' <summary>
        ''' 全局初始的临接表  只保存一次，永久不变，不参与后期运算
        ''' </summary>
        Friend global_edge As Edge()
        Friend global_head As Integer()
        Friend global_top As Integer = 0

        Friend Overridable Sub addEdge(u As Integer, v As Integer, weight As Double)
            If edge(top) Is Nothing Then
                edge(top) = New Edge()
            End If

            edge(top).v = v
            edge(top).weight = weight
            edge(top).next = head(u)
            head(u) = stdNum.Min(Threading.Interlocked.Increment(top), top - 1)
        End Sub

        Friend Overridable Sub addNewEdge(u As Integer, v As Integer, weight As Double)
            If new_edge(new_top) Is Nothing Then
                new_edge(new_top) = New Edge()
            End If

            new_edge(new_top).v = v
            new_edge(new_top).weight = weight
            new_edge(new_top).next = new_head(u)
            new_head(u) = stdNum.Min(Threading.Interlocked.Increment(new_top), new_top - 1)
        End Sub

        Friend Overridable Sub addGlobalEdge(u As Integer, v As Integer, weight As Double)
            If global_edge(global_top) Is Nothing Then
                global_edge(global_top) = New Edge()
            End If

            global_edge(global_top).v = v
            global_edge(global_top).weight = weight
            global_edge(global_top).next = global_head(u)
            global_head(u) = stdNum.Min(Threading.Interlocked.Increment(global_top), global_top - 1)
        End Sub

        Public Function init(Of Node As {New, Network.Node}, Edge As {New, Network.Edge(Of Node)})(g As NetworkGraph(Of Node, Edge)) As LouvainCommunity
            n = g.size.vertex
            global_n = n
            m = g.size.edges
            m *= 2
            Me.edge = New Louvain.Edge(m - 1) {}
            head = New Integer(n - 1) {}

            For i = 0 To n - 1
                head(i) = -1
            Next

            top = 0
            global_edge = New Louvain.Edge(m - 1) {}
            global_head = New Integer(n - 1) {}

            For i = 0 To n - 1
                global_head(i) = -1
            Next

            global_top = 0
            global_cluster = New Integer(n - 1) {}

            For i = 0 To global_n - 1
                global_cluster(i) = i
            Next

            node_weight = New Double(n - 1) {}
            totalEdgeWeight = 0.0

            Dim hasWeight As Boolean = g.graphEdges.Any(Function(l) l.weight <> 0.0)

            For Each link As Edge In g.graphEdges
                Dim u = link.U.ID
                Dim v = link.V.ID
                Dim curw As Double

                If hasWeight Then
                    curw = link.weight
                Else
                    curw = 1.0
                End If

                addEdge(u, v, curw)
                addEdge(v, u, curw)
                addGlobalEdge(u, v, curw)
                addGlobalEdge(v, u, curw)
                totalEdgeWeight += 2 * curw
                node_weight(u) += curw

                If u <> v Then
                    node_weight(v) += curw
                End If
            Next

            resolution = 1 / totalEdgeWeight

            Return Me
        End Function

        Friend Overridable Sub init_cluster()
            cluster = New Integer(n - 1) {}

            For i = 0 To n - 1 ' 一个结点一个簇
                cluster(i) = i
            Next
        End Sub

        Friend Overridable Function try_move_i(i As Integer) As Boolean ' 尝试将i加入某个簇
            Dim edgeWeightPerCluster = New Double(n - 1) {}
            Dim j As Integer = head(i)

            While j <> -1
                Dim l = cluster(edge(j).v) ' l是nodeid所在簇的编号
                edgeWeightPerCluster(l) += edge(j).weight
                j = edge(j).next
            End While

            Dim bestCluster = -1 ' 最优的簇号下标(先默认是自己)
            Dim maxx_deltaQ = 0.0 ' 增量的最大值
            Dim vis = New Boolean(n - 1) {}

            cluster_weight(cluster(i)) -= node_weight(i)
            j = head(i)

            While j <> -1
                Dim l = cluster(edge(j).v) ' l代表領接点的簇号

                If vis(l) Then ' 一个領接簇只判断一次
                    Continue While
                End If

                vis(l) = True
                Dim cur_deltaQ = edgeWeightPerCluster(l)
                cur_deltaQ -= node_weight(i) * cluster_weight(l) * resolution

                If cur_deltaQ > maxx_deltaQ Then
                    bestCluster = l
                    maxx_deltaQ = cur_deltaQ
                End If

                edgeWeightPerCluster(l) = 0
                j = edge(j).next
            End While

            If maxx_deltaQ < eps Then
                bestCluster = cluster(i)
            End If
            'System.out.println(maxx_deltaQ);  
            cluster_weight(bestCluster) += node_weight(i)

            If bestCluster <> cluster(i) Then ' i成功移动了
                cluster(i) = bestCluster
                Return True
            End If

            Return False
        End Function

        ''' <summary>
        ''' rebuild graph
        ''' </summary>
        Friend Overridable Sub rebuildGraph()
            ' 先对簇进行离散化  
            Dim change = New Integer(n - 1) {}
            Dim change_size = 0
            Dim vis = New Boolean(n - 1) {}
            Dim k As Integer

            For i As Integer = 0 To n - 1
                If vis(cluster(i)) Then
                    Continue For
                End If

                vis(cluster(i)) = True
                change(stdNum.Min(Threading.Interlocked.Increment(change_size), change_size - 1)) = cluster(i)
            Next

            ' index[i]代表 i号簇在新图中的结点编号
            Dim index = New Integer(n - 1) {}

            For i = 0 To change_size - 1
                index(change(i)) = i
            Next

            ' 新图的大小
            Dim new_n = change_size
            new_edge = New Edge(m - 1) {}
            new_head = New Integer(new_n - 1) {}
            new_top = 0

            ' 新点权和
            Dim new_node_weight = New Double(new_n - 1) {}

            For i = 0 To new_n - 1
                new_head(i) = -1
            Next

            ' 代表每个簇中的节点列表
            Dim nodeInCluster As List(Of Integer)() = New List(Of Integer)(new_n - 1) {}

            For i = 0 To new_n - 1
                nodeInCluster(i) = New List(Of Integer)()
            Next

            For i = 0 To n - 1
                nodeInCluster(index(cluster(i))).Add(i)
            Next

            For u = 0 To new_n - 1
                ' 将同一个簇的挨在一起分析。可以将visindex数组降到一维
                ' visindex[v]代表新图中u节点到v的边在临街表是第几个（多了1，为了初始化方便）
                Dim visindex = New Boolean(new_n - 1) {}
                ' 边权的增量
                Dim delta_w = New Double(new_n - 1) {}

                For i = 0 To nodeInCluster(u).Count - 1
                    Dim t As Integer = nodeInCluster(u)(i)
                    k = head(t)

                    While k <> -1
                        Dim j = edge(k).v
                        Dim v = index(cluster(j))

                        If u <> v Then
                            If Not visindex(v) Then
                                addNewEdge(u, v, 0)
                                visindex(v) = True
                            End If

                            delta_w(v) += edge(k).weight
                        End If

                        k = edge(k).next
                    End While

                    new_node_weight(u) += node_weight(t)
                Next

                k = new_head(u)

                While k <> -1
                    Dim v = new_edge(k).v
                    new_edge(k).weight = delta_w(v)
                    k = new_edge(k).next
                End While
            Next

            ' 更新答案  
            Dim new_global_cluster = New Integer(global_n - 1) {}

            For i = 0 To global_n - 1
                new_global_cluster(i) = index(cluster(global_cluster(i)))
            Next

            For i = 0 To global_n - 1
                global_cluster(i) = new_global_cluster(i)
            Next

            top = new_top

            For i = 0 To m - 1
                edge(i) = new_edge(i)
            Next

            For i = 0 To new_n - 1
                node_weight(i) = new_node_weight(i)
                head(i) = new_head(i)
            Next

            n = new_n
            init_cluster()
        End Sub

        Friend Overridable Sub louvain()
            init_cluster()
            Dim count = 0 ' 迭代次数
            Dim update_flag As Boolean ' 标记是否发生过更新

            Do ' 第一重循环，每次循环rebuild一次图
                '    print(); // 打印簇列表  
                count += 1
                cluster_weight = New Double(n - 1) {}

                For j = 0 To n - 1 ' 生成簇的权值
                    cluster_weight(cluster(j)) += node_weight(j)
                Next

                Dim order = New Integer(n - 1) {} ' 生成随机序列

                For i = 0 To n - 1
                    order(i) = i
                Next

                Dim random As Random = New Random()

                For i = 0 To n - 1
                    Dim j = random.Next(n)
                    Dim temp = order(i)
                    order(i) = order(j)
                    order(j) = temp
                Next

                Dim enum_time = 0 ' 枚举次数，到n时代表所有点已经遍历过且无移动的点
                Dim point = 0 ' 循环指针
                update_flag = False ' 是否发生过更新的标记

                Do
                    Dim i = order(point)
                    point = (point + 1) Mod n

                    If try_move_i(i) Then ' 对i点做尝试
                        enum_time = 0
                        update_flag = True
                    Else
                        enum_time += 1
                    End If
                Loop While enum_time < n

                If count > iteration_time OrElse Not update_flag Then ' 如果没更新过或者迭代次数超过指定值
                    Exit Do
                End If

                rebuildGraph() ' 重构图
            Loop While True
        End Sub
    End Class
End Namespace