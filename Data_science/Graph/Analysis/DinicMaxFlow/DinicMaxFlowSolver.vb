#Region "Microsoft.VisualBasic::21d5bf3f06ddc76b0d94552984ba51f7, Data_science\Graph\Analysis\DinicMaxFlow\DinicMaxFlowSolver.vb"

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

    '   Total Lines: 284
    '    Code Lines: 161 (56.69%)
    ' Comment Lines: 80 (28.17%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 43 (15.14%)
    '     File Size: 12.70 KB


    '     Class DinicMaxFlowSolver
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: BFS, DFS, GetEdgeFlowInfo, GetFlowDistribution, GetNodeDetail
    '                   MaxFlow
    ' 
    '         Sub: AddEdge
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports std = System.Math

' =============================================================================
' 模块名称: DinicMaxFlowModule
' 功能说明: 基于 Dinic 算法的最大网络流计算模块
'          支持计算指定源点的最大流量以及全网络节点的流量分布
' 作者: Auto-Generated
' 日期: 2026-05-09
' =============================================================================

Namespace DinicMaxFlow

    ' ========================================================================
    ' Dinic 最大流算法核心类
    ' 使用邻接表存储网络图, 支持增广路径上的反向边
    ' ========================================================================
    Public Class DinicMaxFlowSolver

        ' 邻接表: graph(i) 表示节点 i 的所有出边
        Private graph As List(Of List(Of FlowEdge))
        ' 节点数量
        Private nodeCount As Integer
        ' BFS 分层数组: level(i) 表示节点 i 的层级
        Private level As Integer()
        ' DFS 当前弧优化指针: iter(i) 表示节点 i 已访问到的边索引
        Private iter As Integer()

        ' ====================================================================
        ' 构造函数
        ' n: 网络图中的节点总数 (节点编号从 0 到 n-1)
        ' ====================================================================
        Public Sub New(n As Integer)
            nodeCount = n
            graph = New List(Of List(Of FlowEdge))(n)
            For i As Integer = 0 To n - 1
                graph.Add(New List(Of FlowEdge)())
            Next
            level = New Integer(n - 1) {}
            iter = New Integer(n - 1) {}
        End Sub

        ' ====================================================================
        ' 添加一条有向边
        ' fromNode: 起始节点, toNode: 目标节点, cap: 边容量
        ' 同时自动添加反向边 (容量为0), 用于算法中的残余网络更新
        ' ====================================================================
        Public Sub AddEdge(fromNode As Integer, toNode As Integer, cap As Integer)
            Dim forwardEdge As New FlowEdge(toNode, cap, graph(toNode).Count)
            Dim backwardEdge As New FlowEdge(fromNode, 0, graph(fromNode).Count)
            graph(fromNode).Add(forwardEdge)
            graph(toNode).Add(backwardEdge)
        End Sub

        ' ====================================================================
        ' BFS 分层: 构建层级图
        ' 从源点 source 出发, 对所有节点进行分层
        ' 只有当边的残余容量 > 0 且目标节点未被访问时才进行扩展
        ' 返回 True 表示汇点可达, False 表示无增广路径
        ' ====================================================================
        Private Function BFS(source As Integer, sink As Integer) As Boolean
            ' 初始化所有节点层级为 -1 (未访问)
            For i As Integer = 0 To nodeCount - 1
                level(i) = -1
            Next

            Dim queue As New Queue(Of Integer)()
            level(source) = 0
            queue.Enqueue(source)

            Do While queue.Count > 0
                Dim currentNode As Integer = queue.Dequeue()

                ' 遍历当前节点的所有出边
                For Each edge As FlowEdge In graph(currentNode)
                    ' 如果目标节点未被分层且边有残余容量
                    If level(edge.To) < 0 AndAlso edge.Capacity - edge.Flow > 0 Then
                        level(edge.To) = level(currentNode) + 1
                        queue.Enqueue(edge.To)
                    End If
                Next
            Loop

            ' 如果汇点层级不为 -1, 则可达
            Return level(sink) >= 0
        End Function

        ' ====================================================================
        ' DFS 增广: 在分层图中寻找阻塞流
        ' currentNode: 当前节点, sink: 汇点, maxFlow: 当前可推送的最大流量
        ' 使用 iter 数组实现当前弧优化, 避免重复访问已探索的边
        ' 返回实际推送的流量
        ' ====================================================================
        Private Function DFS(currentNode As Integer, sink As Integer, maxFlow As Integer) As Integer
            ' 到达汇点, 返回当前流量
            If currentNode = sink Then
                Return maxFlow
            End If

            ' 从 iter 指向的边开始遍历 (当前弧优化)
            Dim pushed As Integer = 0
            Do While iter(currentNode) < graph(currentNode).Count
                Dim edgeIdx As Integer = iter(currentNode)
                Dim edge As FlowEdge = graph(currentNode)(edgeIdx)

                ' 检查: 目标节点层级必须比当前节点大 1, 且有残余容量
                If level(edge.To) = level(currentNode) + 1 AndAlso
                   edge.Capacity - edge.Flow > 0 Then

                    ' 递归推送流量, 取残余容量和剩余可推流量的最小值
                    Dim remainFlow As Integer = std.Min(maxFlow - pushed, edge.Capacity - edge.Flow)
                    Dim flow As Integer = DFS(edge.To, sink, remainFlow)

                    If flow > 0 Then
                        ' 更新正向边的流量
                        Dim updatedEdge As FlowEdge = graph(currentNode)(edgeIdx)
                        updatedEdge.Flow += flow
                        graph(currentNode)(edgeIdx) = updatedEdge

                        ' 更新反向边的流量 (减少反向边的流量相当于增加残余容量)
                        Dim revEdge As FlowEdge = graph(edge.To)(updatedEdge.Rev)
                        revEdge.Flow -= flow
                        graph(edge.To)(updatedEdge.Rev) = revEdge

                        pushed += flow

                        ' 如果已推送完所有可用流量, 返回
                        If pushed = maxFlow Then
                            Return pushed
                        End If
                    End If
                End If

                iter(currentNode) += 1
            Loop

            Return pushed
        End Function

        ' ====================================================================
        ' 计算最大流量 (主入口)
        ' source: 源点编号, sink: 汇点编号
        ' 返回从源点到汇点的最大流量
        ' ====================================================================
        Public Function MaxFlow(source As Integer, sink As Integer) As Integer
            Dim totalFlow As Integer = 0

            ' 循环执行 BFS + DFS, 直到无法再找到增广路径
            Do While BFS(source, sink)
                ' 每次新的 BFS 分层后, 重置 iter 指针
                For i As Integer = 0 To nodeCount - 1
                    iter(i) = 0
                Next

                ' 在当前分层图中反复进行 DFS 增广
                Dim f As Integer
                Do
                    f = DFS(source, sink, Integer.MaxValue)
                    totalFlow += f
                Loop While f > 0
            Loop

            Return totalFlow
        End Function

        ' ====================================================================
        ' 获取所有节点的流量分布
        ' source: 源点编号, sink: 汇点编号
        ' 返回每个节点的流入、流出、净流量信息
        ' ====================================================================
        Public Function GetFlowDistribution(source As Integer, sink As Integer) As List(Of NodeFlowInfo)
            Dim result As New List(Of NodeFlowInfo)()

            For i As Integer = 0 To nodeCount - 1
                Dim info As New NodeFlowInfo()
                info.NodeIndex = i
                info.TotalInFlow = 0
                info.TotalOutFlow = 0
                info.IsSource = (i = source)
                info.IsSink = (i = sink)

                ' 遍历所有节点的出边, 统计流量
                For Each edge As FlowEdge In graph(i)
                    ' 只统计正向边的实际流量 (反向边为算法内部使用)
                    ' 反向边的容量为 0, 其流量为负值表示回退流
                    If edge.Capacity > 0 Then
                        If edge.Flow > 0 Then
                            ' 节点 i 的流出
                            info.TotalOutFlow += edge.Flow
                            ' 目标节点的流入需要在其自己的迭代中统计
                        End If
                    End If
                Next

                result.Add(info)
            Next

            ' 第二遍遍历: 统计每个节点的流入量
            For i As Integer = 0 To nodeCount - 1
                For Each edge As FlowEdge In graph(i)
                    If edge.Capacity > 0 AndAlso edge.Flow > 0 Then
                        ' 边 i -> edge.To 的实际流量贡献给 edge.To 的流入
                        result(edge.To).TotalInFlow += edge.Flow
                    End If
                Next
            Next

            ' 计算净流量
            For Each info As NodeFlowInfo In result
                info.NetFlow = info.TotalInFlow - info.TotalOutFlow
            Next

            Return result
        End Function

        ' ====================================================================
        ' 获取所有边的实际流量信息
        ' 返回字符串列表, 每行格式: "节点A -> 节点B: 流量/容量"
        ' ====================================================================
        Public Function GetEdgeFlowInfo() As List(Of String)
            Dim result As New List(Of String)()

            For i As Integer = 0 To nodeCount - 1
                For Each edge As FlowEdge In graph(i)
                    ' 只输出正向边 (容量 > 0) 的流量信息
                    If edge.Capacity > 0 Then
                        result.Add(String.Format(
                            "节点{0} -> 节点{1}: 流量={2}/{3}",
                            i, edge.To, edge.Flow, edge.Capacity))
                    End If
                Next
            Next

            Return result
        End Function

        ' ====================================================================
        ' 获取指定节点的流量分布信息
        ' nodeIndex: 目标节点编号
        ' 返回该节点的详细流量信息字符串
        ' ====================================================================
        Public Function GetNodeDetail(nodeIndex As Integer) As String
            If nodeIndex < 0 OrElse nodeIndex >= nodeCount Then
                Return "无效的节点编号"
            End If

            Dim sb As New System.Text.StringBuilder()
            sb.AppendLine(String.Format("=== 节点 {0} 流量详情 ===", nodeIndex))

            ' 流入边
            sb.AppendLine("--- 流入边 ---")
            Dim totalIn As Integer = 0
            For i As Integer = 0 To nodeCount - 1
                For Each edge As FlowEdge In graph(i)
                    If edge.Capacity > 0 AndAlso edge.To = nodeIndex AndAlso edge.Flow > 0 Then
                        sb.AppendLine(String.Format(
                            "  节点{0} -> 节点{1}: 流量 {2}/{3}",
                            i, nodeIndex, edge.Flow, edge.Capacity))
                        totalIn += edge.Flow
                    End If
                Next
            Next
            sb.AppendLine(String.Format("  总流入: {0}", totalIn))

            ' 流出边
            sb.AppendLine("--- 流出边 ---")
            Dim totalOut As Integer = 0
            For Each edge As FlowEdge In graph(nodeIndex)
                If edge.Capacity > 0 AndAlso edge.Flow > 0 Then
                    sb.AppendLine(String.Format(
                        "  节点{0} -> 节点{1}: 流量 {2}/{3}",
                        nodeIndex, edge.To, edge.Flow, edge.Capacity))
                    totalOut += edge.Flow
                End If
            Next
            sb.AppendLine(String.Format("  总流出: {0}", totalOut))

            sb.AppendLine(String.Format("--- 净流量: {0} ---", totalIn - totalOut))

            Return sb.ToString()
        End Function

    End Class

End Namespace

