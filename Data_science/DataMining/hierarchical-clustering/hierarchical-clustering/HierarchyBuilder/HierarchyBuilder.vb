﻿#Region "Microsoft.VisualBasic::db9f4cae3364b68aea3d7e44573b035f, Data_science\DataMining\hierarchical-clustering\hierarchical-clustering\HierarchyBuilder\HierarchyBuilder.vb"

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
' Licensed under the Apache License, Version 2.0 (the "License");
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
                Call Clusters.Remove(minDistLink.Right())
                Call Clusters.Remove(minDistLink.Left())
            End If

            Dim oldClusterL As Cluster = minDistLink.Left()
            Dim oldClusterR As Cluster = minDistLink.Right()
            Dim newCluster As Cluster = minDistLink.Agglomerate(Nothing)
            Dim eval = Clusters.AsParallel _
                .Select(Function(i)
                            Return evaluateDistance(newCluster, i, oldClusterL, oldClusterR, linkageStrategy)
                        End Function) _
                .ToArray

            For Each i As (removes As List(Of HierarchyTreeNode), newLinkage As HierarchyTreeNode) In eval
                Call Distances.Remove(i.removes)
                Call Distances.Add(i.newLinkage, direct:=True)
            Next

            Call Distances.Sort()
            Call Clusters.Add(newCluster)
        End Sub

        Private Function evaluateDistance(newCluster As Cluster,
                                          i As Cluster,
                                          oldClusterL As Cluster, oldClusterR As Cluster,
                                          linkageStrategy As LinkageStrategy) As (removes As List(Of HierarchyTreeNode), newLinkage As HierarchyTreeNode)

            Dim link1 As HierarchyTreeNode = Distances.FindByCodePair(i, oldClusterL)
            Dim link2 As HierarchyTreeNode = Distances.FindByCodePair(i, oldClusterR)
            Dim d1 As Distance = Nothing
            Dim d2 As Distance = Nothing
            Dim removes As New List(Of HierarchyTreeNode)

            If link1 IsNot Nothing Then
                Dim distVal As Double = link1.LinkageDistance
                Dim weightVal As Double = link1.GetOtherCluster(i).WeightValue
                d1 = New Distance(distVal, weightVal)
                removes.Add(link1)
            End If

            If link2 IsNot Nothing Then
                Dim distVal As Double = link2.LinkageDistance
                Dim weightVal As Double = link2.GetOtherCluster(i).WeightValue
                d2 = New Distance(distVal, weightVal)
                removes.Add(link2)
            End If

            Dim newLinkage As New HierarchyTreeNode With {
                .Left = i,
                .Right = newCluster,
                .LinkageDistance = linkageStrategy.CalculateDistance(d1, d2)
            }

            Return (removes, newLinkage)
        End Function
    End Class
End Namespace
