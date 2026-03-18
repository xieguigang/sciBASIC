Imports randf = Microsoft.VisualBasic.Math.RandomExtensions
Imports std = System.Math

Namespace Analysis.Louvain

    ''' <summary>
    ''' Leiden算法 - Louvain算法的改进版，保证社区连通性
    ''' 
    ''' 核心改进：
    ''' 1. 细化阶段(Refinement)：保证每个社区内部节点是连通的
    ''' 2. 快速移动策略：使用队列进行高效的节点移动
    ''' 3. 子社区合并：在细化阶段合并连通的子社区
    ''' </summary>
    ''' <remarks>
    ''' Traag V A, Waltman L, Van Eck N J. From Louvain to Leiden: 
    ''' guaranteeing well-connected communities[J]. Scientific reports, 2019, 9(1): 1-12.
    ''' </remarks>
    Public Class LeidenCommunity : Inherits LouvainCommunity

#Region "私有字段"

        ''' <summary>
        ''' Leiden算法的细化迭代次数
        ''' </summary>
        Private ReadOnly refineIterations As Integer

        ''' <summary>
        ''' 社区到节点的映射，用于细化阶段
        ''' Key: 社区ID, Value: 该社区内的节点列表
        ''' </summary>
        Private communityNodes As Dictionary(Of Integer, List(Of Integer))

        ''' <summary>
        ''' 细化阶段的子社区分配
        ''' Key: 节点ID, Value: 子社区ID
        ''' </summary>
        Private refinedPartition As Integer()

        ''' <summary>
        ''' 用于连通性检测的访问标记数组
        ''' </summary>
        Private visited As Boolean()

        ''' <summary>
        ''' DFS/BFS使用的栈
        ''' </summary>
        Private stack As Stack(Of Integer)

        ''' <summary>
        ''' 细化阶段的参数：控制子社区合并的阈值
        ''' </summary>
        Private ReadOnly theta As Double = 1.0

#End Region

#Region "构造函数"

        ''' <summary>
        ''' 初始化Leiden算法
        ''' </summary>
        ''' <param name="maxIterations">最大迭代次数</param>
        ''' <param name="refineIterations">细化迭代次数</param>
        ''' <param name="eps">误差容限</param>
        ''' <param name="theta">细化参数，控制子社区合并的激进程度</param>
        Sub New(Optional maxIterations As Integer = 3,
                Optional refineIterations As Integer = 2,
                Optional eps As Double = 0.00000000000001,
                Optional theta As Double = 1.0)
            MyBase.New(maxIterations, eps)

            Me.refineIterations = refineIterations
            Me.theta = theta

            ' 预分配足够大的数组，避免频繁扩容
            visited = New Boolean(16383) {}
            stack = New Stack(Of Integer)(1024)
        End Sub

#End Region

#Region "主算法入口"

        ''' <summary>
        ''' Leiden算法的核心：三阶段迭代
        ''' 
        ''' 阶段1: 局部节点移动 (快速移动策略)
        ''' 阶段2: 社区细化 (保证连通性)
        ''' 阶段3: 网络聚合
        ''' </summary>
        ''' <param name="max_clusters">最大社区数量限制</param>
        ''' <returns>自身实例，支持链式调用</returns>
        Public Overloads Function SolveClusters(Optional max_clusters As Integer = Integer.MaxValue) As LeidenCommunity
            Dim count As Integer = 0           ' 迭代次数
            Dim update_flag As Boolean         ' 标记移动阶段是否发生过更新
            Dim refine_flag As Boolean         ' 标记细化阶段是否有更新

            ' 初始化：每个节点一个社区
            Call setCluster0()

            ' 初始化社区节点映射
            communityNodes = New Dictionary(Of Integer, List(Of Integer))()
            UpdateCommunityNodes()

            Do
                ' 迭代计数
                count += 1

                ' 重置社区权重
                cluster_weight = New Double(n - 1) {}
                For j As Integer = 0 To n - 1
                    cluster_weight(cluster(j)) += node_weight(j)
                Next

                ' ========== 阶段1: 快速节点移动 ==========
                update_flag = MoveNodesFast()

                ' 更新社区节点映射
                UpdateCommunityNodes()

                ' ========== 阶段2: 社区细化 ==========
                refine_flag = RefineCommunities()

                ' 如果两个阶段都没有更新，则退出
                If Not update_flag AndAlso Not refine_flag Then
                    Exit Do
                End If

                ' ========== 阶段3: 网络聚合 ==========
                rebuildGraph()
                setCluster0()

                ' 更新社区节点映射
                UpdateCommunityNodes()

                ' 检查退出条件
                If count > maxIterations Then
                    Exit Do
                ElseIf GetClusterCount() >= max_clusters Then
                    Exit Do
                End If

            Loop While True

            Return Me
        End Function

#End Region

#Region "阶段1: 快速节点移动"

        ''' <summary>
        ''' Leiden的快速移动策略
        ''' 
        ''' 与Louvain的区别：
        ''' 1. 使用队列存储需要检查的节点
        ''' 2. 节点移动后，其邻居自动加入队列
        ''' 3. 更高效地传播社区变化
        ''' </summary>
        ''' <returns>是否有节点发生移动</returns>
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

            ' 进度显示
            Dim maxLoop As Integer = n * 50
            Dim processed As Integer = 0
            Dim deltaP As Integer = std.Max(1, maxLoop \ 25)

            Call VBDebugger.EchoLine("")
            Call VBDebugger.Echo(" [Leiden_Move] Progress: ")

            While queue.Count > 0 AndAlso processed < maxLoop
                Dim i = queue.Dequeue()
                inQueue(i) = False
                processed += 1

                Dim oldCommunity = cluster(i)
                Dim bestCommunity = FindBestCommunityFast(i)

                If bestCommunity <> oldCommunity Then
                    ' 执行移动
                    cluster_weight(oldCommunity) -= node_weight(i)
                    cluster(i) = bestCommunity
                    cluster_weight(bestCommunity) += node_weight(i)

                    moved = True

                    ' 将邻居节点加入队列（传播变化）
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

                ' 进度显示
                If processed Mod deltaP = 0 Then
                    VBDebugger.Echo(vbTab & $"{CInt(100 * processed / maxLoop)}%")
                End If
            End While

            Call VBDebugger.EchoLine("")
            Return moved
        End Function

        ''' <summary>
        ''' 为节点寻找最佳社区 (快速移动策略)
        ''' </summary>
        ''' <param name="node">待移动的节点</param>
        ''' <returns>最佳社区ID</returns>
        Private Function FindBestCommunityFast(node As Integer) As Integer
            Dim currentCommunity = cluster(node)
            Dim bestCommunity = currentCommunity
            Dim bestDeltaQ As Double = 0.0

            ' 收集所有可能的社区（当前社区 + 邻居所在社区）
            Dim possibleCommunities = New HashSet(Of Integer)()
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
        ''' 
        ''' 模块度增量公式：
        ''' ΔQ = [k_i,in / 2m - Σ_tot * k_i / (2m)²] - [k_i,in(current) / 2m - Σ_tot(current) * k_i / (2m)²]
        ''' 简化后：ΔQ = k_i,in(target) - k_i,in(current) - k_i * (Σ_tot(target) - Σ_tot(current)) / (2m)
        ''' </summary>
        ''' <param name="node">节点ID</param>
        ''' <param name="targetCommunity">目标社区ID</param>
        ''' <returns>模块度变化值</returns>
        Private Function CalculateDeltaQ(node As Integer, targetCommunity As Integer) As Double
            Dim currentCommunity = cluster(node)
            If currentCommunity = targetCommunity Then Return 0.0

            ' 计算与目标社区连接的边权总和
            Dim kiInTarget As Double = 0.0
            Dim kiInCurrent As Double = 0.0

            Dim j = head(node)
            While j <> -1
                Dim neighborComm = cluster(edge(j).v)
                If neighborComm = targetCommunity Then
                    kiInTarget += edge(j).weight
                ElseIf neighborComm = currentCommunity Then
                    kiInCurrent += edge(j).weight
                End If
                j = edge(j).next
            End While

            Dim ki = node_weight(node)
            Dim sigmaTotTarget = cluster_weight(targetCommunity)
            Dim sigmaTotCurrent = cluster_weight(currentCommunity)

            ' 模块度增量计算
            ' 加入目标社区的增益: kiInTarget - ki * sigmaTotTarget * resolution
            ' 离开原社区的损失: -(kiInCurrent - ki * (sigmaTotCurrent - ki) * resolution)
            ' 总增量 = 增益 - 损失
            Dim deltaQ = kiInTarget - kiInCurrent -
                         ki * (sigmaTotTarget - (sigmaTotCurrent - ki)) * resolution

            Return deltaQ
        End Function

#End Region

#Region "阶段2: 社区细化"

        ''' <summary>
        ''' 社区细化阶段 - Leiden算法的核心创新
        ''' 
        ''' 目的：保证每个社区内部节点是连通的
        ''' 
        ''' 步骤：
        ''' 1. 对每个社区，检查其内部连通性
        ''' 2. 如果不连通，将不连通的部分分离为独立子社区
        ''' 3. 在子社区内进行局部优化
        ''' 4. 合并收益为正的子社区
        ''' </summary>
        ''' <returns>是否发生了细化</returns>
        Private Function RefineCommunities() As Boolean
            Dim refined As Boolean = False

            ' 初始化细化分区（每个节点初始在自己的子社区）
            refinedPartition = New Integer(n - 1) {}
            For i = 0 To n - 1
                refinedPartition(i) = i
            Next

            ' 进度显示
            Call VBDebugger.EchoLine("")
            Call VBDebugger.Echo(" [Leiden_Refine] Progress: ")

            Dim totalCommunities = communityNodes.Keys.Count
            Dim processedCommunities = 0

            ' 对每个社区进行处理
            For Each communityId In communityNodes.Keys.ToList()
                processedCommunities += 1

                Dim nodesInCommunity = communityNodes(communityId)
                If nodesInCommunity.Count <= 1 Then
                    Continue For
                End If

                ' 步骤1: 检查连通性并分离不连通分量
                Dim components = FindConnectedComponents(nodesInCommunity)

                If components.Count > 1 Then
                    ' 社区不连通，需要分离
                    refined = True

                    ' 为每个连通分量分配新的子社区ID
                    For compIdx = 0 To components.Count - 1
                        For Each node In components(compIdx)
                            refinedPartition(node) = communityId * 1000 + compIdx
                        Next
                    Next
                End If

                ' 步骤2: 在社区内进行局部移动优化
                If LocalMoveWithinCommunity(nodesInCommunity, communityId) Then
                    refined = True
                End If

                ' 进度显示
                If processedCommunities Mod std.Max(1, totalCommunities \ 10) = 0 Then
                    VBDebugger.Echo(vbTab & $"{CInt(100 * processedCommunities / totalCommunities)}%")
                End If
            Next

            ' 步骤3: 合并子社区
            If MergeSubCommunities() Then
                refined = True
            End If

            ' 步骤4: 应用细化结果到主分区
            If refined Then
                ApplyRefinedPartition()
            End If

            Call VBDebugger.EchoLine("")
            Return refined
        End Function

        ''' <summary>
        ''' 在社区内进行局部移动优化
        ''' </summary>
        ''' <param name="nodesInCommunity">社区内的节点列表</param>
        ''' <param name="communityId">社区ID</param>
        ''' <returns>是否有节点移动</returns>
        Private Function LocalMoveWithinCommunity(nodesInCommunity As List(Of Integer), communityId As Integer) As Boolean
            Dim moved As Boolean = False

            ' 子社区权重
            Dim subCommWeight = New Dictionary(Of Integer, Double)()

            ' 初始化子社区权重
            For Each node In nodesInCommunity
                Dim subComm = refinedPartition(node)
                If Not subCommWeight.ContainsKey(subComm) Then
                    subCommWeight(subComm) = 0.0
                End If
                subCommWeight(subComm) += node_weight(node)
            Next

            ' 迭代优化
            For iter = 0 To refineIterations - 1
                Dim changed = False

                ' 随机顺序遍历节点
                For Each node In nodesInCommunity.OrderBy(Function(x) randf.seeds.Next())
                    Dim bestSubComm = FindBestSubCommunity(node, subCommWeight, nodesInCommunity)

                    If bestSubComm <> refinedPartition(node) Then
                        ' 移动节点
                        Dim oldSubComm = refinedPartition(node)
                        subCommWeight(oldSubComm) -= node_weight(node)

                        refinedPartition(node) = bestSubComm

                        If Not subCommWeight.ContainsKey(bestSubComm) Then
                            subCommWeight(bestSubComm) = 0.0
                        End If
                        subCommWeight(bestSubComm) += node_weight(node)

                        changed = True
                        moved = True
                    End If
                Next

                If Not changed Then Exit For
            Next

            Return moved
        End Function

        ''' <summary>
        ''' 为节点在社区内寻找最佳子社区
        ''' </summary>
        ''' <param name="node">节点ID</param>
        ''' <param name="subCommWeight">子社区权重字典</param>
        ''' <param name="nodesInCommunity">社区内的节点列表</param>
        ''' <returns>最佳子社区ID</returns>
        Private Function FindBestSubCommunity(node As Integer,
                                              subCommWeight As Dictionary(Of Integer, Double),
                                              nodesInCommunity As List(Of Integer)) As Integer
            Dim currentSubComm = refinedPartition(node)
            Dim bestSubComm = currentSubComm
            Dim bestDeltaQ As Double = 0.0

            ' 收集邻居所在的子社区
            Dim neighborSubComms = New HashSet(Of Integer)()
            neighborSubComms.Add(currentSubComm)

            Dim j = head(node)
            While j <> -1
                Dim neighbor = edge(j).v
                ' 只考虑同一社区内的邻居
                If cluster(neighbor) = cluster(node) Then
                    neighborSubComms.Add(refinedPartition(neighbor))
                End If
                j = edge(j).next
            End While

            ' 评估每个子社区
            For Each subComm In neighborSubComms
                If subComm = currentSubComm Then Continue For

                Dim deltaQ = CalculateSubCommDeltaQ(node, subComm, subCommWeight)

                If deltaQ > bestDeltaQ Then
                    bestDeltaQ = deltaQ
                    bestSubComm = subComm
                End If
            Next

            ' 如果收益太小，保持当前子社区
            If bestDeltaQ < eps Then
                Return currentSubComm
            End If

            Return bestSubComm
        End Function

        ''' <summary>
        ''' 计算节点移动到目标子社区的模块度变化
        ''' </summary>
        ''' <param name="node">节点ID</param>
        ''' <param name="targetSubComm">目标子社区ID</param>
        ''' <param name="subCommWeight">子社区权重字典</param>
        ''' <returns>模块度变化值</returns>
        Private Function CalculateSubCommDeltaQ(node As Integer,
                                                 targetSubComm As Integer,
                                                 subCommWeight As Dictionary(Of Integer, Double)) As Double
            Dim currentSubComm = refinedPartition(node)
            If currentSubComm = targetSubComm Then Return 0.0

            ' 计算与目标子社区的连接权重
            Dim kiInTarget As Double = 0.0
            Dim kiInCurrent As Double = 0.0

            Dim j = head(node)
            While j <> -1
                Dim neighbor = edge(j).v
                If cluster(neighbor) = cluster(node) Then
                    Dim neighborSubComm = refinedPartition(neighbor)
                    If neighborSubComm = targetSubComm Then
                        kiInTarget += edge(j).weight
                    ElseIf neighborSubComm = currentSubComm Then
                        kiInCurrent += edge(j).weight
                    End If
                End If
                j = edge(j).next
            End While

            Dim ki = node_weight(node)
            Dim sigmaTarget As Double = 0.0
            Dim sigmaCurrent As Double = 0.0

            If subCommWeight.ContainsKey(targetSubComm) Then
                sigmaTarget = subCommWeight(targetSubComm)
            End If
            If subCommWeight.ContainsKey(currentSubComm) Then
                sigmaCurrent = subCommWeight(currentSubComm)
            End If

            ' 模块度增量
            Dim deltaQ = kiInTarget - kiInCurrent -
                         ki * (sigmaTarget - (sigmaCurrent - ki)) * resolution

            Return deltaQ
        End Function

        ''' <summary>
        ''' 合并子社区
        ''' 
        ''' 将收益为正的子社区合并，同时保证连通性
        ''' </summary>
        ''' <returns>是否发生了合并</returns>
        Private Function MergeSubCommunities() As Boolean
            Dim merged As Boolean = False

            ' 对每个社区处理
            For Each communityId In communityNodes.Keys.ToList()
                Dim nodesInCommunity = communityNodes(communityId)
                If nodesInCommunity.Count <= 1 Then Continue For

                ' 收集该社区内的所有子社区
                Dim subComms = New HashSet(Of Integer)()
                For Each node In nodesInCommunity
                    subComms.Add(refinedPartition(node))
                Next

                If subComms.Count <= 1 Then Continue For

                ' 构建子社区间的连接关系
                Dim subCommConnections = BuildSubCommConnections(nodesInCommunity)

                ' 贪心合并：尝试合并有连接且收益为正的子社区
                Dim subCommList = subComms.ToList()
                For i = 0 To subCommList.Count - 1
                    For j = i + 1 To subCommList.Count - 1
                        Dim subComm1 = subCommList(i)
                        Dim subComm2 = subCommList(j)

                        ' 检查是否有连接
                        If Not subCommConnections.ContainsKey(subComm1) OrElse
                           Not subCommConnections(subComm1).ContainsKey(subComm2) Then
                            Continue For
                        End If

                        ' 计算合并收益
                        Dim mergeGain = CalculateMergeGain(subComm1, subComm2, nodesInCommunity)

                        If mergeGain > eps Then
                            ' 执行合并：将subComm2合并到subComm1
                            For Each node In nodesInCommunity
                                If refinedPartition(node) = subComm2 Then
                                    refinedPartition(node) = subComm1
                                End If
                            Next

                            merged = True
                        End If
                    Next
                Next
            Next

            Return merged
        End Function

        ''' <summary>
        ''' 构建子社区间的连接关系
        ''' </summary>
        ''' <param name="nodesInCommunity">社区内的节点列表</param>
        ''' <returns>子社区连接字典</returns>
        Private Function BuildSubCommConnections(nodesInCommunity As List(Of Integer)) As Dictionary(Of Integer, Dictionary(Of Integer, Double))
            Dim connections = New Dictionary(Of Integer, Dictionary(Of Integer, Double))()

            For Each node In nodesInCommunity
                Dim subComm1 = refinedPartition(node)

                If Not connections.ContainsKey(subComm1) Then
                    connections(subComm1) = New Dictionary(Of Integer, Double)()
                End If

                Dim j = head(node)
                While j <> -1
                    Dim neighbor = edge(j).v
                    If cluster(neighbor) = cluster(node) Then
                        Dim subComm2 = refinedPartition(neighbor)
                        If subComm1 <> subComm2 Then
                            If Not connections(subComm1).ContainsKey(subComm2) Then
                                connections(subComm1)(subComm2) = 0.0
                            End If
                            connections(subComm1)(subComm2) += edge(j).weight
                        End If
                    End If
                    j = edge(j).next
                End While
            Next

            Return connections
        End Function

        ''' <summary>
        ''' 计算合并两个子社区的收益
        ''' </summary>
        ''' <param name="subComm1">子社区1</param>
        ''' <param name="subComm2">子社区2</param>
        ''' <param name="nodesInCommunity">社区内的节点列表</param>
        ''' <returns>合并收益</returns>
        Private Function CalculateMergeGain(subComm1 As Integer,
                                             subComm2 As Integer,
                                             nodesInCommunity As List(Of Integer)) As Double
            Dim weight1 As Double = 0.0
            Dim weight2 As Double = 0.0
            Dim edgeWeightBetween As Double = 0.0

            For Each node In nodesInCommunity
                Dim subComm = refinedPartition(node)
                If subComm = subComm1 Then
                    weight1 += node_weight(node)
                ElseIf subComm = subComm2 Then
                    weight2 += node_weight(node)
                End If
            Next

            ' 计算两个子社区间的边权
            For Each node In nodesInCommunity
                If refinedPartition(node) = subComm1 Then
                    Dim j = head(node)
                    While j <> -1
                        Dim neighbor = edge(j).v
                        If cluster(neighbor) = cluster(node) AndAlso
                           refinedPartition(neighbor) = subComm2 Then
                            edgeWeightBetween += edge(j).weight
                        End If
                        j = edge(j).next
                    End While
                End If
            Next

            ' 合并收益 = 边权 - θ * 权重乘积 * resolution
            Dim gain = edgeWeightBetween - theta * weight1 * weight2 * resolution

            Return gain
        End Function

        ''' <summary>
        ''' 应用细化分区结果到主分区
        ''' </summary>
        Private Sub ApplyRefinedPartition()
            ' 将细化结果映射到新的社区ID
            Dim subCommToNewComm = New Dictionary(Of Integer, Integer)()
            Dim newCommId = 0

            For i = 0 To n - 1
                Dim subComm = refinedPartition(i)
                If Not subCommToNewComm.ContainsKey(subComm) Then
                    subCommToNewComm(subComm) = newCommId
                    newCommId += 1
                End If
                cluster(i) = subCommToNewComm(subComm)
            Next
        End Sub

#End Region

#Region "连通性检测"

        ''' <summary>
        ''' 在社区内查找连通分量
        ''' 
        ''' 使用DFS算法检测社区内节点的连通性
        ''' </summary>
        ''' <param name="nodes">社区内的节点列表</param>
        ''' <returns>连通分量列表</returns>
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

            ' 对每个未访问的节点进行DFS
            For Each startNode In nodes
                ' 跳过已访问的节点
                If startNode < visited.Length AndAlso visited(startNode) Then
                    Continue For
                End If

                Dim component = New List(Of Integer)()
                stack.Push(startNode)

                If startNode < visited.Length Then
                    visited(startNode) = True
                End If

                ' DFS遍历
                While stack.Count > 0
                    Dim currentNode = stack.Pop()
                    component.Add(currentNode)

                    ' 遍历邻居
                    Dim j = head(currentNode)
                    While j <> -1
                        Dim neighbor = edge(j).v

                        ' 只考虑同一社区内且在节点集合中的邻居
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

#End Region

#Region "辅助方法"

        ''' <summary>
        ''' 更新社区到节点的映射
        ''' </summary>
        Private Sub UpdateCommunityNodes()
            If communityNodes Is Nothing Then
                communityNodes = New Dictionary(Of Integer, List(Of Integer))()
            Else
                communityNodes.Clear()
            End If

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
        ''' <returns>新的社区ID</returns>
        Private Function GetNextCommunityId() As Integer
            Dim maxId = -1
            For Each id In communityNodes.Keys
                If id > maxId Then
                    maxId = id
                End If
            Next
            Return maxId + 1
        End Function

        ''' <summary>
        ''' 生成随机节点顺序
        ''' </summary>
        ''' <param name="size">节点数量</param>
        ''' <returns>随机排列的节点索引数组</returns>
        Private Function GenerateRandomOrder(size As Integer) As Integer()
            Dim order = New Integer(size - 1) {}

            For i = 0 To size - 1
                order(i) = i
            Next

            ' Fisher-Yates洗牌算法
            For i = size - 1 To 1 Step -1
                Dim j = randf.seeds.Next(0, i + 1)
                Dim temp = order(i)
                order(i) = order(j)
                order(j) = temp
            Next

            Return order
        End Function

#End Region

#Region "重写父类方法"

        ''' <summary>
        ''' 重写重建图方法，添加调试信息
        ''' </summary>
        Friend Overrides Sub rebuildGraph()
            Call VBDebugger.EchoLine($"Leiden: 开始重建图，当前社区数: {communityNodes.Keys.Count}")
            MyBase.rebuildGraph()
            Call VBDebugger.EchoLine($"Leiden: 重建图完成，新图节点数: {n}")
        End Sub

#End Region

    End Class
End Namespace