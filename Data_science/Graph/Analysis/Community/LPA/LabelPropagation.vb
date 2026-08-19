#Region "Microsoft.VisualBasic::4f1b9d7c2e8a63504f1b9d7c2e8a6350, Data_science\Graph\Analysis\Community\LPA\LabelPropagation.vb"

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

    '   Total Lines: 250
    '   Code Lines: 160 (64.00%)
    ' Comment Lines: 52 (20.80%)
    '    - Xml Docs: 92.31%
    ' 
    '   Blank Lines: 38 (15.20%)
    '     File Size: 8.90 KB


    '     Class LabelPropagation
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: GetClusterCount, GetClusters, GetCommunity, propagateLabel, SolveClusters
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions
Imports std = System.Math

Namespace Analysis.LPA

    ''' <summary>
    ''' Label Propagation Algorithm, LPA community detection
    ''' </summary>
    ''' <remarks>
    ''' Raghavan U N, Albert R, Kumara S. Near linear time algorithm to 
    ''' detect community structures In large-scale networks[J]. Physical 
    ''' Review E, 2007, 76(3): 036106.
    ''' 
    ''' 算法的迭代逻辑：每个节点把自己的标签改成邻居之中出现次数（加权）最多的标签，
    ''' 收敛之后节点的标签值即为其所归属的社区编号。
    ''' </remarks>
    Public Class LabelPropagation

        ''' <summary>
        ''' number of vertex
        ''' </summary>
        Friend n As Integer
        ''' <summary>
        ''' number of edges(有向边的数目，即无向边数目的两倍)
        ''' </summary>
        Friend m As Integer
        ''' <summary>
        ''' 每个节点当前所拥有的标签值，标签值即社区编号
        ''' </summary>
        Friend label As Integer()
        ''' <summary>
        ''' 邻接表
        ''' </summary>
        Friend edge As Edge()
        ''' <summary>
        ''' 头节点下标
        ''' </summary>
        Friend head As Integer()
        ''' <summary>
        ''' 已用边的个数
        ''' </summary>
        Friend top As Integer
        ''' <summary>
        ''' 节点ID下标所对应的节点label名字，用于社区划分结果的输出
        ''' </summary>
        Friend nodeLabels As String()

        ''' <summary>
        ''' 最大迭代次数上限，LPA一般经过少数几轮迭代之后就会收敛
        ''' </summary>
        Protected ReadOnly maxIterations As Integer = 100

        Sub New(Optional maxIterations As Integer = 100)
            Me.maxIterations = maxIterations
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetCommunity() As String()
            Return label.Select(Function(l) l.ToString).ToArray
        End Function

        ''' <summary>
        ''' get the number of the community class the graph it has currently.
        ''' (获取当前图结构之中的社区的数量)
        ''' </summary>
        ''' <returns></returns>
        Public Function GetClusterCount() As Integer
            Return label.Distinct.Count
        End Function

        ''' <summary>
        ''' 获取社区划分的结果
        ''' </summary>
        ''' <returns>
        ''' 返回一个字典：字典的key为社区标签编号（字符串），字典的value为
        ''' 该社区之中的所有成员节点的<see cref="Vertex.label"/>名字列表
        ''' </returns>
        Public Function GetClusters() As Dictionary(Of String, String())
            Dim clusters As New Dictionary(Of String, List(Of String))

            For i As Integer = 0 To n - 1
                Dim community As String = label(i).ToString

                If Not clusters.ContainsKey(community) Then
                    clusters.Add(community, New List(Of String))
                End If

                clusters(community).Add(nodeLabels(i))
            Next

            Return clusters.ToDictionary(Function(cl) cl.Key,
                                         Function(cl) cl.Value.ToArray)
        End Function

        ''' <summary>
        ''' 执行标签传播迭代：按照随机节点序列进行异步更新，每一轮迭代都将节点的标签
        ''' 修改为其邻居之中加权票数最多的标签，直到某一轮迭代之中所有的节点的标签
        ''' 都不再发生变化（收敛）或者达到最大迭代次数上限为止。
        ''' </summary>
        ''' <returns>返回当前对象自身，以便于进行链式调用</returns>
        Public Function SolveClusters() As LabelPropagation
            If n <= 0 Then
                Return Me
            End If

            ' 所有节点所共享的标签票数计数器，配合touched列表进行局部重置，
            ' 避免每一个节点更新的时候都分配一个O(n)的数组
            Dim labelCounter As Double() = New Double(n - 1) {}
            Dim touched As New List(Of Integer)
            ' 随机节点遍历序列
            Dim order As Integer() = New Integer(n - 1) {}
            Dim count As Integer = 0

            For i As Integer = 0 To n - 1
                order(i) = i
            Next

            Call VBDebugger.EchoLine("")

            Do
                count += 1

                ' 生成随机序列（Fisher-Yates洗牌）
                For i As Integer = 0 To n - 1
                    Dim j = randf.seeds.Next(n)
                    Dim temp = order(i)
                    order(i) = order(j)
                    order(j) = temp
                Next

                ' 异步更新：按随机序列遍历所有的节点，进行一轮标签传播
                Dim updated As Boolean = False

                For p As Integer = 0 To n - 1
                    If propagateLabel(order(p), labelCounter, touched) Then
                        updated = True
                    End If
                Next

                Call VBDebugger.EchoLine($" [LPA loop_{count}] community: {GetClusterCount()}")

                ' 一轮迭代下来所有的节点的标签都没有发生变化，则算法已经收敛
                If Not updated OrElse count >= maxIterations Then
                    Exit Do
                End If
            Loop While True

            Call VBDebugger.EchoLine("")

            Return Me
        End Function

        ''' <summary>
        ''' 标签传播的单节点更新：将节点i的标签修改为其邻居之中加权票数最多的标签
        ''' </summary>
        ''' <param name="i">目标节点下标</param>
        ''' <param name="labelCounter">所有节点所共享的标签票数计数器</param>
        ''' <param name="touched">当前节点所统计到的标签列表，用于计数器的重置</param>
        ''' <returns>节点的标签是否发生了变化</returns>
        Private Function propagateLabel(i As Integer,
                                        labelCounter As Double(),
                                        touched As List(Of Integer)) As Boolean
            Const eps As Double = 0.00000000000001

            Dim j As Integer = head(i)
            Dim maxVotes As Double = 0.0

            ' 1. 沿邻接表累加统计邻居标签的加权票数
            While j <> -1
                If edge(j).v <> i AndAlso edge(j).weight <> 0.0 Then
                    ' 跳过自环和零权重边
                    Dim l As Integer = label(edge(j).v)

                    If labelCounter(l) = 0.0 Then
                        touched.Add(l)
                    End If

                    labelCounter(l) += edge(j).weight

                    If labelCounter(l) > maxVotes Then
                        maxVotes = labelCounter(l)
                    End If
                End If

                j = edge(j).next
            End While

            If touched.Count = 0 Then
                ' 孤立节点，没有产生任何的邻居投票，保持自己的标签不变
                Return False
            End If

            ' 2. 找出最大票数的标签（可能存在多个平局的标签），
            '    同时重置共享计数器以供下一个节点的更新所使用
            Dim bestLabels As New List(Of Integer)

            For Each l As Integer In touched
                If std.Abs(labelCounter(l) - maxVotes) <= eps Then
                    bestLabels.Add(l)
                End If

                labelCounter(l) = 0.0
            Next

            Call touched.Clear()

            ' 3. 平局规则：假若当前标签也是最大票数的标签之一，则优先保持当前
            '    标签不变（防止标签振荡）；否则从平局集合之中随机挑选一个标签
            Dim bestLabel As Integer

            If bestLabels.Contains(label(i)) Then
                bestLabel = label(i)
            ElseIf bestLabels.Count = 1 Then
                bestLabel = bestLabels(0)
            Else
                bestLabel = bestLabels(randf.seeds.Next(bestLabels.Count))
            End If

            If bestLabel <> label(i) Then
                ' 节点的标签发生了变化
                label(i) = bestLabel
                Return True
            Else
                Return False
            End If
        End Function
    End Class
End Namespace
