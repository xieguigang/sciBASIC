#Region "Microsoft.VisualBasic::ab5f2f11b8de0c920a77001f84571dde, Data_science\DataMining\hierarchical-clustering\hierarchical-clustering\HierarchyBuilder\HierarchyBuilder.vb"

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

    '   Total Lines: 155
    '    Code Lines: 80 (51.61%)
    ' Comment Lines: 53 (34.19%)
    '    - Xml Docs: 49.06%
    ' 
    '   Blank Lines: 22 (14.19%)
    '     File Size: 6.47 KB


    '     Class HierarchyBuilder
    ' 
    '         Properties: Clusters, Distances, First, RootCluster, TreeComplete
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: flatAgg
    ' 
    '         Sub: Agglomerate, removeCluster
    ' 
    ' 
    ' /********************************************************************************/

#End Region

'
'*****************************************************************************
' Copyright 2013 Lars Behnke
' 
' Licensed under the c, Version 2.0 (the "License");
' you may not use this file except in compliance with the License.
' You may obtain a copy of the License at
' 
'   http://www.apache.org/licenses/LICENSE-2.0
' 
' Unless required by applicable law or agreed to in writing, software
' distributed under the License is distributed on an "AS IS" BASIS,
' WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
' See the License for the specific language governing permissions and
' limitations under the License.
' *****************************************************************************
'

Imports System.Runtime.CompilerServices

Namespace Hierarchy

    Public Class HierarchyBuilder

        Public ReadOnly Property Distances As DistanceMap
        Public ReadOnly Property Clusters As List(Of Cluster)

        ''' <summary>
        ''' 当前仍然存活的簇数量（即 <see cref="Clusters"/> 列表中
        ''' <see cref="Cluster.removed"/> = False 的条目数）。
        ''' </summary>
        ''' <remarks>
        ''' 凝聚过程中的簇删除采用「标记 + 惰性压实」策略：
        ''' 合并时仅将两个旧簇标记为 <c>removed</c>（O(1)），失效条目在其数量
        ''' 超过存活条目时由 <see cref="CompactClusters"/> 一次性移除。
        ''' </remarks>
        Dim aliveCount As Integer

        ''' <summary>
        ''' 当<see cref="Clusters"/>的数量最终只有一个节点的时候，就认为完成了层次聚类操作了
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property TreeComplete As Boolean
            Get
                Return aliveCount = 1
            End Get
        End Property

        Const NoRoot$ = "No root available"

        ''' <summary>
        ''' Gets the root cluster of the hierarchy tree
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RootCluster As Cluster
            Get
                If Not TreeComplete Then
                    Throw New EvaluateException(NoRoot)
                Else
                    Return Me.First
                End If
            End Get
        End Property

        ''' <summary>
        ''' The first element in this <see cref="HierarchyBuilder"/>, 
        ''' if <see cref="TreeComplete"/> then this first element is the root cluster.
        ''' </summary>
        ''' <remarks>
        ''' 由于凝聚过程中的簇删除采用标记策略（<see cref="Cluster.removed"/>），
        ''' 这里返回的是第一个仍然存活的簇。
        ''' </remarks>
        ''' <returns></returns>
        Public ReadOnly Property First As Cluster
            Get
                For Each c As Cluster In Clusters
                    If Not c.removed Then
                        Return c
                    End If
                Next

                Return Nothing
            End Get
        End Property

        Public Sub New(clusters As List(Of Cluster), distances As DistanceMap)
            Me.Clusters = clusters
            Me.Distances = distances
            Me.aliveCount = clusters.Count
        End Sub

        ''' <summary>
        ''' Returns Flattened clusters, i.e. clusters that are at least apart by a given threshold </summary>
        ''' <param name="linkageStrategy"> </param>
        ''' <param name="threshold">
        ''' @return </param>
        Public Function flatAgg(linkageStrategy As LinkageStrategy, threshold As Double) As IList(Of Cluster)
            Do While ((Not TreeComplete)) AndAlso (Distances.MinimalDistance() <= threshold)
                'System.out.println("Cluster Distances: " + distances.toString());
                'System.out.println("Cluster Size: " + clusters.size());
                Call Agglomerate(linkageStrategy)
            Loop

            'System.out.println("Final MinDistance: " + distances.minDist());
            'System.out.println("Tree complete: " + isTreeComplete());

            ' Clusters 列表可能还包含已标记删除、尚未压实的失效条目，这里只返回存活簇
            Return Clusters.Where(Function(c) Not c.removed).ToList
        End Function

        ''' <summary>
        ''' 进行层次聚类的迭代计算操作，主要的限速步骤
        ''' </summary>
        ''' <param name="linkageStrategy"></param>
        Public Sub Agglomerate(linkageStrategy As LinkageStrategy)
            Dim minDistLink As HierarchyTreeNode = Distances.RemoveFirst()

            If minDistLink Is Nothing Then
                Return
            Else
                ' O(1) 标记删除：旧实现在这里对 Clusters 列表做两次 O(n) 的
                ' 线性扫描 + RemoveAt（累计 O(n^2) 的常数开销），
                ' 失效条目由 CompactClusters 惰性压实
                minDistLink.Left.removed = True
                minDistLink.Right.removed = True
                aliveCount -= 2
            End If

            Dim oldClusterL As Cluster = minDistLink.Left()
            Dim oldClusterR As Cluster = minDistLink.Right()
            Dim newCluster As Cluster = minDistLink.Agglomerate(Nothing)

            ' 顺序原地更新：对每个存活簇，移除指向 L/R 的两条旧链接，并推入一条指向新簇的新链接。
            ' 
            ' 旧实现每一轮都会开启一次 PLINQ（AsParallel + ToArray），并为每个簇分配一个
            ' List(Of HierarchyTreeNode)，在中小规模数据上并行调度与临时对象分配的开销远大于计算本身；
            ' 且每轮结束还要对整张链接表做一次全量排序。这里改为单次顺序循环，配合 DistanceMap 的
            ' 最小堆结构（插入即维护堆序），彻底移除每轮排序。
            Dim n As Integer = Clusters.Count

            For idx As Integer = 0 To n - 1
                Dim i As Cluster = Clusters(idx)

                If i.removed Then
                    Continue For
                End If

                Dim link1 As HierarchyTreeNode = Distances.FindByCodePair(i, oldClusterL)
                Dim link2 As HierarchyTreeNode = Distances.FindByCodePair(i, oldClusterR)
                Dim d1 As Distance = Nothing
                Dim d2 As Distance = Nothing

                If link1 IsNot Nothing Then
                    d1 = New Distance(link1.LinkageDistance, link1.GetOtherCluster(i).WeightValue)
                    Call Distances.Remove(link1)
                End If

                If link2 IsNot Nothing Then
                    d2 = New Distance(link2.LinkageDistance, link2.GetOtherCluster(i).WeightValue)
                    Call Distances.Remove(link2)
                End If

                Dim newLinkage As New HierarchyTreeNode With {
                    .Left = i,
                    .Right = newCluster,
                    .LinkageDistance = linkageStrategy.CalculateDistance(d1, d2)
                }

                Call Distances.Add(newLinkage, direct:=True)
            Next

            Call Clusters.Add(newCluster)
            aliveCount += 1

            ' 失效条目多于存活条目时压实一次列表，
            ' 保持 Clusters 的规模与遍历的缓存友好性（惰性压实的均摊代价为 O(1)）
            If Clusters.Count > aliveCount * 2 Then
                Call CompactClusters()
            End If
        End Sub

        ''' <summary>
        ''' 一次性移除 <see cref="Clusters"/> 列表中所有已标记删除（<see cref="Cluster.removed"/>）的失效条目。
        ''' </summary>
        Private Sub CompactClusters()
            Call Clusters.RemoveAll(Function(c) c.removed)
        End Sub
    End Class
End Namespace
