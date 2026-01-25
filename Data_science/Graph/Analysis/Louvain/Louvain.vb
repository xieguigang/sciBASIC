#Region "Microsoft.VisualBasic::0492e6ad4f8a21703b3c07201657441b, Data_science\Graph\Analysis\Louvain\Louvain.vb"

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

    '   Total Lines: 383
    '    Code Lines: 225 (58.75%)
    ' Comment Lines: 89 (23.24%)
    '    - Xml Docs: 79.78%
    ' 
    '   Blank Lines: 69 (18.02%)
    '     File Size: 12.41 KB


    '     Class LouvainCommunity
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: GetClusterCount, GetCommunity, SolveClusters, TryMoveNode
    ' 
    '         Sub: addNewEdge, rebuildGraph, setCluster0
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions
Imports std = System.Math

Namespace Analysis.Louvain

    ''' <summary>
    ''' A fast algorithm To find communities In large network
    ''' </summary>
    ''' <remarks>
    ''' Blondel V D, Guillaume J L, Lambiotte R, et al. Fast 
    ''' unfolding of communities in large networks[J]. Journal 
    ''' of Statistical Mechanics, 2008, 2008(10)155-168.
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
        ReadOnly maxIterations As Integer = 3

        Sub New(Optional maxIterations As Integer = 3, Optional eps As Double = 0.00000000000001)
            Me.eps = eps
            Me.maxIterations = maxIterations
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetCommunity() As String()
            Return global_cluster.Select(Function(cl) cl.ToString).ToArray
        End Function

        ''' <summary>
        ''' get the number of the cluster class the graph it has currently.
        ''' </summary>
        ''' <returns></returns>
        Public Function GetClusterCount() As Integer
            Return global_cluster.Distinct.Count
        End Function

        Friend Overridable Sub addNewEdge(u As Integer, v As Integer, weight As Double)
            If new_edge(new_top) Is Nothing Then
                new_edge(new_top) = New Edge()
            End If

            new_edge(new_top).v = v
            new_edge(new_top).weight = weight
            new_edge(new_top).next = new_head(u)
            new_head(u) = new_top
            new_top += 1
        End Sub

        Friend Overridable Sub setCluster0()
            cluster = New Integer(n - 1) {}

            For i As Integer = 0 To n - 1
                ' 一个节点一个簇
                cluster(i) = i
            Next
        End Sub

        ''' <summary>
        ''' 尝试将i加入某个簇
        ''' </summary>
        ''' <param name="i"></param>
        ''' <returns></returns>
        Friend Overridable Function TryMoveNode(i As Integer) As Boolean
            Dim edgeWeightPerCluster = New Double(n - 1) {}
            Dim j As Integer = head(i)

            While j <> -1
                ' l是nodeid所在簇的编号
                Dim l = cluster(edge(j).v)
                edgeWeightPerCluster(l) += edge(j).weight
                j = edge(j).next
            End While

            Dim bestCluster = -1  ' 最优的簇号下标(先默认是自己)
            Dim maxx_deltaQ = 0.0 ' 增量的最大值
            Dim vis = New Boolean(n - 1) {}
            Dim cur_deltaQ As Double

            cluster_weight(cluster(i)) -= node_weight(i)
            j = head(i)

            While j <> -1
                ' l代表領接点的簇号
                Dim l = cluster(edge(j).v)

                If vis(l) Then
                    ' 一个領接簇只判断一次
                    Exit While
                Else
                    vis(l) = True
                End If

                cur_deltaQ = edgeWeightPerCluster(l)
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

            cluster_weight(bestCluster) += node_weight(i)

            If bestCluster <> cluster(i) Then
                ' i成功移动了
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
                change(change_size) = cluster(i)
                change_size += 1
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

            global_cluster = new_global_cluster
            top = new_top

            For i = 0 To m - 1
                edge(i) = new_edge(i)
            Next

            For i = 0 To new_n - 1
                node_weight(i) = new_node_weight(i)
                head(i) = new_head(i)
            Next

            n = new_n
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="max_clusters">
        ''' set the limits of the max number of the node class we finally have.
        ''' </param>
        ''' <returns></returns>
        Public Function SolveClusters(Optional max_clusters As Integer = Integer.MaxValue) As LouvainCommunity
            Dim count As Integer = 0   ' 迭代次数
            Dim update_flag As Boolean ' 标记是否发生过更新
            Dim enum_time As Integer
            Dim point As Integer

            Call setCluster0()

            Do
                ' 第一重循环，每次循环rebuild一次图
                count += 1
                cluster_weight = New Double(n - 1) {}

                For j As Integer = 0 To n - 1
                    ' 生成簇的权值
                    cluster_weight(cluster(j)) += node_weight(j)
                Next

                ' 生成随机序列
                Dim order = New Integer(n - 1) {}
                Dim maxLoop As Integer = 0

                For i As Integer = 0 To n - 1
                    order(i) = i
                Next

                For i As Integer = 0 To n - 1
                    Dim j = randf.seeds.Next(n)
                    Dim temp = order(i)
                    order(i) = order(j)
                    order(j) = temp
                Next

                enum_time = 0       ' 枚举次数，到n时代表所有点已经遍历过且无移动的点
                point = 0           ' 循环指针
                update_flag = False ' 是否发生过更新的标记
                maxLoop = node_weight.Length * 50

                Dim max As Integer = maxLoop
                Dim deltaP As Integer = maxLoop / 25
                Dim p As Integer = Scan0

                Call VBDebugger.EchoLine("")
                Call VBDebugger.Echo($" [loop_{count}] Progress: ")

                Do
                    Dim i As Integer = order(point)

                    point = (point + 1) Mod n
                    maxLoop -= 1

                    If TryMoveNode(i) Then
                        enum_time = 0
                        update_flag = True
                    Else
                        enum_time += 1
                    End If

                    If maxLoop < 0 Then
                        Exit Do
                    Else
                        p += 1

                        If p = deltaP Then
                            p = 0

                            VBDebugger.Echo(vbTab & $"{CInt(100 * (max - maxLoop) / max)}%")
                        End If
                    End If
                Loop While enum_time < n

                If count > maxIterations OrElse Not update_flag Then
                    Exit Do
                ElseIf GetClusterCount >= max_clusters Then
                    Exit Do
                End If

                rebuildGraph()
                setCluster0()
            Loop While True

            Call VBDebugger.EchoLine("")

            Return Me
        End Function
    End Class
End Namespace
