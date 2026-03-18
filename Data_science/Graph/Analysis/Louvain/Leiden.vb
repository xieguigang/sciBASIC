Imports System.Runtime.CompilerServices
Imports System.Collections.Generic
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions

Namespace Analysis.Louvain

    ''' <summary>
    ''' Leiden算法 - Louvain算法的改进版，保证社区连通性
    ''' </summary>
    ''' <remarks>
    ''' Traag V A, Waltman L, Van Eck N J. From Louvain to Leiden: 
    ''' guaranteeing well-connected communities[J]. Scientific reports, 2019, 9(1): 1-12.
    ''' </remarks>
    Public Class LeidenCommunity : Inherits LouvainCommunity

        ''' <summary>
        ''' Leiden算法的细化迭代次数
        ''' </summary>
        Private ReadOnly refineIterations As Integer

        ''' <summary>
        ''' 社区到节点的映射，用于细化阶段
        ''' </summary>
        Private communityNodes As Dictionary(Of Integer, List(Of Integer))

        ' 类级别字段，复用visited数组
        Private visited As Boolean()
        Private stack As Stack(Of Integer)

        ''' <summary>
        ''' 初始化Leiden算法
        ''' </summary>
        ''' <param name="maxIterations">最大迭代次数</param>
        ''' <param name="refineIterations">细化迭代次数</param>
        ''' <param name="eps">误差容限</param>
        Sub New(Optional maxIterations As Integer = 3,
                Optional refineIterations As Integer = 2,
                Optional eps As Double = 0.00000000000001)
            MyBase.New(maxIterations, eps)

            Me.refineIterations = refineIterations
            visited = New Boolean(16383) {}  ' 预分配足够大的数组
            stack = New Stack(Of Integer)(1024)
        End Sub

        ''' <summary>
        ''' Leiden算法的核心：三阶段迭代
        ''' 1. 局部节点移动 (类似Louvain)
        ''' 2. 社区细化 (保证连通性)
        ''' 3. 网络聚合
        ''' </summary>
        Public Overloads Function SolveClusters(Optional max_clusters As Integer = Integer.MaxValue) As LeidenCommunity
            Dim count As Integer = 0           ' 迭代次数
            Dim update_flag As Boolean          ' 标记是否发生过更新
            Dim refine_flag As Boolean          ' 标记细化阶段是否有更新
            Dim enum_time As Integer
            Dim point As Integer

            Call setCluster0()
            communityNodes = New Dictionary(Of Integer, List(Of Integer))()

            Do
                ' 第一重循环，每次循环包含三个阶段
                count += 1

                ' 重置社区权重
                cluster_weight = New Double(n - 1) {}
                For j As Integer = 0 To n - 1
                    cluster_weight(cluster(j)) += node_weight(j)
                Next

                ' 阶段1: 局部节点移动 (快速移动)
                update_flag = MoveNodesFast()

                ' 阶段2: 社区细化 (保证连通性)
                refine_flag = RefineCommunities()

                ' 如果没有更新且没有细化，则退出
                If Not update_flag AndAlso Not refine_flag Then
                    Exit Do
                End If

                ' 阶段3: 网络聚合
                rebuildGraph()
                setCluster0()

                ' 更新社区节点映射
                UpdateCommunityNodes()

                If count > maxIterations Then
                    Exit Do
                ElseIf GetClusterCount() >= max_clusters Then
                    Exit Do
                End If
            Loop While True

            Return Me
        End Function

        ''' <summary>
        ''' Leiden的快速移动策略
        ''' 允许节点移动到任意邻居社区，即使没有直接连接
        ''' </summary>
        Private Function MoveNodesFast() As Boolean
            Dim moved As Boolean = False

            ' 使用队列存储需要检查的节点
            Dim queue = New Queue(Of Integer)()
            Dim inQueue = New Boolean(n - 1) {}

            ' 初始时所有节点入队
            For i = 0 To n - 1
                queue.Enqueue(i)
                inQueue(i) = True
            Next

            While queue.Count > 0
                Dim i = queue.Dequeue()
                inQueue(i) = False

                Dim oldCommunity = cluster(i)
                Dim bestCommunity = FindBestCommunityFast(i)

                If bestCommunity <> oldCommunity Then
                    ' 执行移动
                    cluster_weight(oldCommunity) -= node_weight(i)
                    cluster(i) = bestCommunity
                    cluster_weight(bestCommunity) += node_weight(i)

                    moved = True

                    ' 将邻居节点加入队列
                    Dim j = head(i)
                    While j <> -1
                        Dim neighbor = edge(j).v
                        If Not inQueue(neighbor) Then
                            queue.Enqueue(neighbor)
                            inQueue(neighbor) = True
                        End If
                        j = edge(j).next
                    End While
                End If
            End While

            Return moved
        End Function

        ''' <summary>
        ''' 为节点寻找最佳社区 (快速移动策略)
        ''' </summary>
        Private Function FindBestCommunityFast(node As Integer) As Integer
            Dim currentCommunity = cluster(node)
            Dim bestCommunity = currentCommunity
            Dim bestDeltaQ = 0.0

            ' 收集所有可能的社区
            Dim possibleCommunities = New HashSet(Of Integer)()

            ' 添加当前社区
            possibleCommunities.Add(currentCommunity)

            ' 添加邻居节点所在的社区
            Dim j = head(node)
            While j <> -1
                Dim neighborCommunity = cluster(edge(j).v)
                possibleCommunities.Add(neighborCommunity)
                j = edge(j).next
            End While

            ' 评估每个可能的社区
            For Each comm In possibleCommunities
                If comm = currentCommunity Then
                    Continue For
                End If

                Dim deltaQ = CalculateDeltaQ(node, comm)

                If deltaQ > bestDeltaQ Then
                    bestDeltaQ = deltaQ
                    bestCommunity = comm
                End If
            Next

            ' 如果移动收益很小，保持当前社区
            If bestDeltaQ < eps Then
                Return currentCommunity
            End If

            Return bestCommunity
        End Function

        ''' <summary>
        ''' 计算节点移动到目标社区的模块度变化
        ''' </summary>
        Private Function CalculateDeltaQ(node As Integer, targetCommunity As Integer) As Double
            Dim currentCommunity = cluster(node)
            If currentCommunity = targetCommunity Then Return 0.0

            ' 计算与目标社区连接的边权总和
            Dim kiIn = 0.0  ' 节点i与目标社区的连接权重
            Dim j = head(node)
            While j <> -1
                If cluster(edge(j).v) = targetCommunity Then
                    kiIn += edge(j).weight
                End If
                j = edge(j).next
            End While

            Dim ki = node_weight(node)
            Dim sigmaTotTarget = cluster_weight(targetCommunity)
            Dim sigmaTotCurrent = cluster_weight(currentCommunity) - ki

            ' 模块度增量 = 加入目标社区的增益 - 离开原社区的损失
            ' 加入目标社区: kiIn - ki * sigmaTotTarget / (2m)
            ' 离开原社区: -(0 - ki * sigmaTotCurrent / (2m)) = ki * sigmaTotCurrent / (2m)
            ' 总增量 = kiIn - ki * sigmaTotTarget / (2m) + ki * sigmaTotCurrent / (2m)
            ' 简化: deltaQ = kiIn - ki * (sigmaTotTarget - sigmaTotCurrent) / (2m)

            ' 你的resolution = 1 / (2m)，所以：
            Dim deltaQ = kiIn - ki * (sigmaTotTarget - sigmaTotCurrent) * resolution

            Return deltaQ
        End Function

        ''' <summary>
        ''' 社区细化阶段 - Leiden算法的核心
        ''' 保证每个社区内部节点是连通的
        ''' </summary>
        Private Function RefineCommunities() As Boolean
            Dim refined As Boolean = False

            ' 为每个社区的节点创建子社区（初始时每个节点一个子社区）
            Dim subCommunities = New Dictionary(Of Integer, Dictionary(Of Integer, Integer))()

            ' 对每个社区进行处理
            For Each communityId In communityNodes.Keys
                Dim nodesInCommunity = communityNodes(communityId)
                If nodesInCommunity.Count <= 1 Then Continue For

                ' 步骤1: 初始化子社区（每个节点一个）
                Dim subComm = New Dictionary(Of Integer, Integer)()
                Dim subCommWeight = New Dictionary(Of Integer, Double)()

                For Each node In nodesInCommunity
                    subComm(node) = node  ' 初始时节点ID即子社区ID
                    subCommWeight(node) = node_weight(node)
                Next

                ' 步骤2: 在社区内运行局部移动
                Dim subCommChanged = True
                Dim iterations = 0
                Dim maxIter = nodesInCommunity.Count * 10

                While subCommChanged AndAlso iterations < maxIter
                    subCommChanged = False
                    iterations += 1

                    ' 随机顺序遍历社区内节点
                    For Each node In nodesInCommunity.OrderBy(Function(x) randf.seeds.Next())
                        ' 找最佳子社区（只考虑邻居节点所在的子社区）
                        Dim bestSubComm = FindBestSubCommunity(node, subComm, subCommWeight, nodesInCommunity)

                        If bestSubComm <> subComm(node) Then
                            ' 移动节点到新子社区
                            subCommWeight(subComm(node)) -= node_weight(node)
                            subComm(node) = bestSubComm

                            If Not subCommWeight.ContainsKey(bestSubComm) Then
                                subCommWeight(bestSubComm) = 0.0
                            End If
                            subCommWeight(bestSubComm) += node_weight(node)

                            subCommChanged = True
                        End If
                    Next
                End While

                ' 步骤3: 合并子社区（贪心策略）
                ' 这里需要实现子社区合并逻辑，保证连通性
                ' ... 详见完整实现

                If MergeSubCommunities(subComm, subCommWeight, nodesInCommunity) Then
                    refined = True
                End If
            Next

            Return refined
        End Function

        ''' <summary>
        ''' 在社区内查找连通分量
        ''' </summary>
        Private Function FindConnectedComponents(nodes As List(Of Integer)) As List(Of List(Of Integer))
            Dim components = New List(Of List(Of Integer))()

            ' 使用HashSet提高查找效率
            Dim nodeSet = New HashSet(Of Integer)(nodes)

            ' 清空visited（只清空需要的部分）
            For Each node In nodes
                If node < visited.Length Then
                    visited(node) = False
                End If
            Next

            stack.Clear()

            For Each node In nodes
                If node < visited.Length AndAlso visited(node) Then Continue For

                Dim component = New List(Of Integer)()
                stack.Push(node)

                If node < visited.Length Then
                    visited(node) = True
                End If

                While stack.Count > 0
                    Dim currentNode = stack.Pop()
                    component.Add(currentNode)

                    Dim j = head(currentNode)
                    While j <> -1
                        Dim neighbor = edge(j).v
                        ' 使用HashSet进行O(1)查找
                        If cluster(neighbor) = cluster(currentNode) AndAlso
                   nodeSet.Contains(neighbor) AndAlso
                   (neighbor >= visited.Length OrElse Not visited(neighbor)) Then
                            If neighbor < visited.Length Then
                                visited(neighbor) = True
                            End If
                            stack.Push(neighbor)
                        End If
                        j = edge(j).next
                    End While
                End While

                components.Add(component)
            Next

            Return components
        End Function

        ''' <summary>
        ''' 更新社区到节点的映射
        ''' </summary>
        Private Sub UpdateCommunityNodes()
            communityNodes.Clear()

            For i = 0 To n - 1
                Dim commId = cluster(i)

                If Not communityNodes.ContainsKey(commId) Then
                    communityNodes(commId) = New List(Of Integer)()
                End If

                communityNodes(commId).Add(i)
            Next
        End Sub

        ''' <summary>
        ''' 获取下一个可用的社区ID
        ''' </summary>
        Private Function GetNextCommunityId() As Integer
            Dim maxId = 0
            For Each id In communityNodes.Keys
                If id > maxId Then
                    maxId = id
                End If
            Next
            Return maxId + 1
        End Function

        ''' <summary>
        ''' 构建社区间的邻接关系
        ''' </summary>
        Private Sub BuildCommunityAdjacency()
            ' 这个方法用于后续可能的优化
            ' 在Leiden的基本实现中，可以通过社区邻接关系加速计算
        End Sub

        ''' <summary>
        ''' 生成随机节点顺序
        ''' </summary>
        Private Function GenerateRandomOrder(size As Integer) As Integer()
            Dim order = New Integer(size - 1) {}

            For i = 0 To size - 1
                order(i) = i
            Next

            ' Fisher-Yates洗牌算法
            Dim rnd = New Random()
            For i = size - 1 To 1 Step -1
                Dim j = rnd.Next(0, i + 1)
                Dim temp = order(i)
                order(i) = order(j)
                order(j) = temp
            Next

            Return order
        End Function

        ' 类级别缓冲数组
        Private buffer_nodeInCluster As List(Of Integer)()
        Private buffer_edgeWeights As Double()
        Private buffer_visindex As Boolean()

        ''' <summary>
        ''' 重写重建图方法，添加调试信息
        ''' </summary>
        Friend Overrides Sub rebuildGraph()
            Call VBDebugger.EchoLine($"Leiden: 开始重建图，当前社区数: {communityNodes.Keys.Count}")
            MyBase.rebuildGraph()
            Call VBDebugger.EchoLine($"Leiden: 重建图完成，新图节点数: {n}")
        End Sub
    End Class
End Namespace