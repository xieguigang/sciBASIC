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
            Dim order = GenerateRandomOrder(n)
            Dim iterations = 0
            Dim maxIter = n * 10  ' 最大内部迭代次数

            ' 构建社区邻接关系
            BuildCommunityAdjacency()

            Do
                Dim anyMove As Boolean = False

                For Each i In order
                    Dim bestCommunity = FindBestCommunityFast(i)

                    If bestCommunity <> cluster(i) Then
                        ' 执行移动
                        cluster_weight(cluster(i)) -= node_weight(i)
                        cluster(i) = bestCommunity
                        cluster_weight(bestCommunity) += node_weight(i)
                        anyMove = True
                        moved = True
                    End If
                Next

                iterations += 1
                If iterations >= maxIter OrElse Not anyMove Then
                    Exit Do
                End If
            Loop

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
            If currentCommunity = targetCommunity Then
                Return 0.0
            End If

            ' 计算与目标社区连接的边权总和
            Dim sumWeightsToCommunity = 0.0
            Dim j = head(node)
            While j <> -1
                If cluster(edge(j).v) = targetCommunity Then
                    sumWeightsToCommunity += edge(j).weight
                End If
                j = edge(j).next
            End While

            ' 计算模块度变化
            Dim ki = node_weight(node)
            Dim sigma_tot = cluster_weight(targetCommunity)

            ' ΔQ = [Σ_in + k_i,in]/2m - [Σ_tot + k_i]²/4m² - [Σ_in/2m - (Σ_tot)²/4m² - (k_i)²/4m²]
            ' 简化为: ΔQ = (k_i,in - k_i * Σ_tot * resolution)
            Dim deltaQ = sumWeightsToCommunity - ki * sigma_tot * resolution

            Return deltaQ
        End Function

        ''' <summary>
        ''' 社区细化阶段 - Leiden算法的核心
        ''' 保证每个社区内部节点是连通的
        ''' </summary>
        Private Function RefineCommunities() As Boolean
            Dim refined As Boolean = False
            UpdateCommunityNodes()

            ' 对每个社区进行细化
            For Each communityId In communityNodes.Keys
                Dim nodesInCommunity = communityNodes(communityId)

                If nodesInCommunity.Count <= 1 Then
                    Continue For
                End If

                ' 检查社区连通性
                Dim connectedComponents = FindConnectedComponents(nodesInCommunity)

                ' 如果社区不连通，需要细化
                If connectedComponents.Count > 1 Then
                    ' 重新分配不连通的组件到新社区
                    Dim newCommunityId = GetNextCommunityId()

                    ' 第一个组件保持原社区ID，其他组件分配新ID
                    For compIdx = 1 To connectedComponents.Count - 1
                        For Each nodeId In connectedComponents(compIdx)
                            cluster(nodeId) = newCommunityId
                            cluster_weight(newCommunityId) += node_weight(nodeId)
                            cluster_weight(communityId) -= node_weight(nodeId)
                        Next
                        newCommunityId = GetNextCommunityId()
                        refined = True
                    Next
                End If
            Next

            Return refined
        End Function

        ''' <summary>
        ''' 在社区内查找连通分量
        ''' </summary>
        Private Function FindConnectedComponents(nodes As List(Of Integer)) As List(Of List(Of Integer))
            Dim components = New List(Of List(Of Integer))()
            Dim visited = New Boolean(n) {}

            For Each node In nodes
                If Not visited(node) Then
                    Dim component = New List(Of Integer)()
                    Dim stack = New Stack(Of Integer)()

                    stack.Push(node)
                    visited(node) = True

                    While stack.Count > 0
                        Dim currentNode = stack.Pop()
                        component.Add(currentNode)

                        ' 遍历当前节点的邻居
                        Dim j = head(currentNode)
                        While j <> -1
                            Dim neighbor = edge(j).v

                            ' 只考虑在同一社区内且未被访问的邻居
                            If cluster(neighbor) = cluster(node) AndAlso
                               nodes.Contains(neighbor) AndAlso
                               Not visited(neighbor) Then
                                visited(neighbor) = True
                                stack.Push(neighbor)
                            End If
                            j = edge(j).next
                        End While
                    End While

                    components.Add(component)
                End If
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