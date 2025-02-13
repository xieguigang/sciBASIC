#Region "Microsoft.VisualBasic::1db91a0197c8dbd7fbcc3789cad71cd1, Data_science\DataMining\hierarchical-clustering\hierarchical-clustering\ClusteringAlgorithm\DefaultClusteringAlgorithm.vb"

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

    '   Total Lines: 174
    '    Code Lines: 112 (64.37%)
    ' Comment Lines: 28 (16.09%)
    '    - Xml Docs: 17.86%
    ' 
    '   Blank Lines: 34 (19.54%)
    '     File Size: 7.55 KB


    ' Class DefaultClusteringAlgorithm
    ' 
    '     Properties: debug
    ' 
    '     Function: (+2 Overloads) createClusters, createLinkages, performClustering, performFlatClustering, performWeightedClustering
    ' 
    '     Sub: checkArguments
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.ApplicationServices.Terminal.ProgressBar.Tqdm
Imports Microsoft.VisualBasic.DataMining.HierarchicalClustering.Hierarchy
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

'
'*****************************************************************************
' Copyright 2013 Lars Behnke
' <p/>
' Licensed under the Apache License, Version 2.0 (the "License");
' you may not use this file except in compliance with the License.
' You may obtain a copy of the License at
' <p/>
' http://www.apache.org/licenses/LICENSE-2.0
' <p/>
' Unless required by applicable law or agreed to in writing, software
' distributed under the License is distributed on an "AS IS" BASIS,
' WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
' See the License for the specific language governing permissions and
' limitations under the License.
' *****************************************************************************

Public Class DefaultClusteringAlgorithm : Implements ClusteringAlgorithm

    Public Property debug As Boolean = False

    Public Function performClustering(distances As Double()(), clusterNames$(), linkageStrategy As LinkageStrategy) As Cluster Implements ClusteringAlgorithm.performClustering
        Call checkArguments(distances, clusterNames, linkageStrategy)

        ' Setup model 
        Dim clusters As IList(Of Cluster) = createClusters(clusterNames)
        Dim linkages As DistanceMap = createLinkages(distances, clusters)

        ' Process 
        Dim builder As New HierarchyBuilder(clusters, linkages)
        Dim i As i32 = 1
        Dim total As Integer = clusters.Count
        Dim tqdm As ProgressBar = TqdmWrapper.Wrap(total)

        Do While Not builder.TreeComplete
            Call builder.Agglomerate(linkageStrategy)
            Call tqdm.SetLabel($"[iteration_{++i}] {builder.Clusters.Count}...")
            Call tqdm.Progress(total - builder.Clusters.Count, total)
        Loop

        Call tqdm.Finish()
        Call VBDebugger.EchoLine("")

        Return builder.RootCluster
    End Function

    Public Function performFlatClustering(distances As Double()(), clusterNames$(), linkageStrategy As LinkageStrategy, threshold As Double) As IList(Of Cluster) Implements ClusteringAlgorithm.performFlatClustering

        Call checkArguments(distances, clusterNames, linkageStrategy)

        ' Setup model 
        Dim clusters As IList(Of Cluster) = createClusters(clusterNames)
        Dim linkages As DistanceMap = createLinkages(distances, clusters)

        ' Process 
        Dim builder As New HierarchyBuilder(clusters, linkages)
        Return builder.flatAgg(linkageStrategy, threshold)
    End Function

    Private Shared Sub checkArguments(distances As Double()(), clusterNames As String(), linkageStrategy As LinkageStrategy)
        If distances Is Nothing OrElse distances.Length = 0 OrElse distances(0).Length <> distances.Length Then Throw New ArgumentException("Invalid distance matrix")
        If distances.Length <> clusterNames.Length Then Throw New ArgumentException("Invalid cluster name array")
        If linkageStrategy Is Nothing Then Throw New ArgumentException("Undefined linkage strategy")

        Dim uniqueCount As Integer = clusterNames.Distinct.Count

        If uniqueCount <> clusterNames.Length Then
            Throw New ArgumentException("Duplicate names")
        End If
    End Sub

    Public Function performWeightedClustering(distances As Double()(), clusterNames As String(), weights As Double(), linkageStrategy As LinkageStrategy) As Cluster Implements ClusteringAlgorithm.performWeightedClustering
        If weights.Length <> clusterNames.Length Then
            Throw New ArgumentException("Invalid weights array")
        Else
            Call checkArguments(distances, clusterNames, linkageStrategy)
        End If

        ' Setup model 
        Dim clusters As IList(Of Cluster) = createClusters(clusterNames, weights)
        Dim linkages As DistanceMap = Time(Function() createLinkages(distances, clusters))

        ' Process 
        Dim builder As New HierarchyBuilder(clusters, linkages)
        Do While Not builder.TreeComplete
            builder.Agglomerate(linkageStrategy)
        Loop

        Return builder.RootCluster
    End Function

    Private Function createLinkages(distances As Double()(), clusters As IList(Of Cluster)) As DistanceMap
        If clusters.Count < 100 Then
            Dim linkages As New DistanceMap

            For col As Integer = 0 To clusters.Count - 1
                For row As Integer = col + 1 To clusters.Count - 1
                    Dim link As New HierarchyTreeNode
                    Dim lCluster As Cluster = clusters(col)
                    Dim rCluster As Cluster = clusters(row)
                    link.LinkageDistance = distances(col)(row)
                    link.Left = (lCluster)
                    link.Right = (rCluster)

                    linkages.Add(link)
                Next
            Next

            Return linkages
        Else
            ' 当数量很大的时候，这里也是一个限速步骤，需要使用并行
            Dim copy As Cluster() = clusters.ToArray
            Dim LQuery = From c As SeqValue(Of Cluster)
                         In clusters.SeqIterator.AsParallel
                         Let col As Integer = c.i
                         Let lCluster As Cluster = c.value
                         Let list = alignRow(lCluster, col, distances, copy)
                         Select list.ToArray

            Dim links = LQuery _
                .IteratesALL _
                .ToArray

            Return New DistanceMap(links)
        End If
    End Function

    Private Iterator Function alignRow(lCluster As Cluster, col As Integer, distances As Double()(), clusters As Cluster()) As IEnumerable(Of HierarchyTreeNode)
        Dim n = clusters.Length

        For row As Integer = col + 1 To n - 1
            Dim rCluster As Cluster = clusters(row)
            Dim link As New HierarchyTreeNode With {
                .LinkageDistance = distances(col)(row),
                .Left = lCluster,
                .Right = rCluster
            }

            Yield link
        Next
    End Function

    ''' <summary>
    ''' 创建cluster对象
    ''' </summary>
    ''' <param name="clusterNames"></param>
    ''' <returns></returns>
    Private Shared Function createClusters(clusterNames As String()) As IList(Of Cluster)
        Return clusterNames _
            .Select(Function(clusterName) New Cluster(clusterName)) _
            .AsList
    End Function

    Private Shared Function createClusters(clusterNames As String(), weights As Double()) As IList(Of Cluster)
        Dim clusters As IList(Of Cluster) = New List(Of Cluster)

        For i As Integer = 0 To weights.Length - 1
            Dim cluster As New Cluster(clusterNames(i))
            cluster.Distance = New Distance(0.0, weights(i))
            clusters.Add(cluster)
        Next

        Return clusters
    End Function

End Class
