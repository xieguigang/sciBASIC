#Region "Microsoft.VisualBasic::2c700fc2f84936a956c26a48e5e87a8d, ..\sciBASIC#\Data_science\DataMining\hierarchical-clustering\hierarchical-clustering\PDistClusteringAlgorithm.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Collections.Generic
Imports Microsoft.VisualBasic.DataMining.HierarchicalClustering.Hierarchy

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

Public Class PDistClusteringAlgorithm
    Implements ClusteringAlgorithm

    Public Function performClustering(distances As Double()(), clusterNames As String(), linkageStrategy As LinkageStrategy) As Cluster Implements ClusteringAlgorithm.performClustering

        ' Argument checks 
        If distances Is Nothing OrElse distances.Length = 0 Then Throw New ArgumentException("Invalid distance matrix")
        If distances(0).Length <> clusterNames.Length * (clusterNames.Length - 1) \ 2 Then Throw New ArgumentException("Invalid cluster name array")
        If linkageStrategy Is Nothing Then Throw New ArgumentException("Undefined linkage strategy")

        ' Setup model 
        Dim clusters As IList(Of Cluster) = createClusters(clusterNames)
        Dim linkages As DistanceMap = createLinkages(distances, clusters)

        ' Process 
        Dim builder As New HierarchyBuilder(clusters, linkages)

        Do While Not builder.TreeComplete
            builder.Agglomerate(linkageStrategy)
        Loop

        Return builder.RootCluster
    End Function

    Public Function performFlatClustering(distances As Double()(), clusterNames As String(), linkageStrategy As LinkageStrategy, threshold As Double) As IList(Of Cluster) Implements ClusteringAlgorithm.performFlatClustering

        ' Argument checks 
        If distances Is Nothing OrElse distances.Length = 0 Then Throw New System.ArgumentException("Invalid distance matrix")
        If distances(0).Length <> clusterNames.Length * (clusterNames.Length - 1) \ 2 Then Throw New System.ArgumentException("Invalid cluster name array")
        If linkageStrategy Is Nothing Then Throw New System.ArgumentException("Undefined linkage strategy")

        ' Setup model 
        Dim clusters As IList(Of Cluster) = createClusters(clusterNames)
        Dim linkages As DistanceMap = createLinkages(distances, clusters)

        ' Process 
        Dim builder As New HierarchyBuilder(clusters, linkages)
        Return builder.flatAgg(linkageStrategy, threshold)
    End Function

    Public Function performWeightedClustering(distances As Double()(), clusterNames As String(), weights As Double(), linkageStrategy As LinkageStrategy) As Cluster Implements ClusteringAlgorithm.performWeightedClustering
        Return performClustering(distances, clusterNames, linkageStrategy)
    End Function

    Private Function createLinkages(distances As Double()(), clusters As IList(Of Cluster)) As DistanceMap
        Dim linkages As New DistanceMap

        For col As Integer = 0 To clusters.Count - 1
            Dim cluster_col As Cluster = clusters(col)
            For row As Integer = col + 1 To clusters.Count - 1
                Dim link As New HierarchyTreeNode
                Dim d As Double = distances(0)(accessFunction(row, col, clusters.Count))
                link.LinkageDistance = d
                link.Left = (cluster_col)
                link.Right = (clusters(row))
                linkages.Add(link)
            Next
        Next

        Return linkages
    End Function

    Private Function createClusters(clusterNames As String()) As IList(Of Cluster)
        Dim clusters As IList(Of Cluster) = New List(Of Cluster)

        For Each clusterName As String In clusterNames
            Dim cluster As New Cluster(clusterName)
            cluster.AddLeafName(clusterName)
            clusters.Add(cluster)
        Next

        Return clusters
    End Function

    ''' <summary>
    ''' Credit to this function goes to
    ''' http://stackoverflow.com/questions/13079563/how-does-condensed-distance-matrix-work-pdist
    ''' </summary>
    ''' <param name="i"></param>
    ''' <param name="j"></param>
    ''' <param name="n"></param>
    ''' <returns></returns>
    Private Shared Function accessFunction(i As Integer, j As Integer, n As Integer) As Integer
        Return n * j - j * (j + 1) \ 2 + i - 1 - j
    End Function

End Class
