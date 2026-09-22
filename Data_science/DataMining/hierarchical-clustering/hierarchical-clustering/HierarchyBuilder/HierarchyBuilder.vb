#Region "Microsoft.VisualBasic::db9f4cae3364b68aea3d7e44573b035f, Data_science\DataMining\hierarchical-clustering\hierarchical-clustering\HierarchyBuilder\HierarchyBuilder.vb"

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

    '   Total Lines: 153
    '    Code Lines: 88 (57.52%)
    ' Comment Lines: 43 (28.10%)
    '    - Xml Docs: 51.16%
    ' 
    '   Blank Lines: 22 (14.38%)
    '     File Size: 6.24 KB


    '     Class HierarchyBuilder
    ' 
    '         Properties: Clusters, Distances, First, RootCluster, TreeComplete
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: evaluateDistance, flatAgg
    ' 
    '         Sub: Agglomerate
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
        ''' 当<see cref="Clusters"/>的数量最终只有一个节点的时候，就认为完成了层次聚类操作了
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property TreeComplete As Boolean
            Get
                Return Clusters.Count = 1
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
        ''' <returns></returns>
        Public ReadOnly Property First As Cluster
            Get
                Return Clusters(Scan0)
            End Get
        End Property

        Public Sub New(clusters As List(Of Cluster), distances As DistanceMap)
            Me.Clusters = clusters
            Me.Distances = distances
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
            Return Clusters
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
                Call removeCluster(minDistLink.Right())
                Call removeCluster(minDistLink.Left())
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
        End Sub

        ''' <summary>
        ''' 按引用（而非 <see cref="Cluster.Equals(Object)"/> 基于名称的比较）从 <see cref="Clusters"/> 中移除指定簇，
        ''' 避免每次合并都进行 O(n) 次字符串比较。
        ''' </summary>
        Private Sub removeCluster(cluster As Cluster)
            For i As Integer = 0 To Clusters.Count - 1
                If Clusters(i) Is cluster Then
                    Call Clusters.RemoveAt(i)
                    Return
                End If
            Next
        End Sub
    End Class
End Namespace
