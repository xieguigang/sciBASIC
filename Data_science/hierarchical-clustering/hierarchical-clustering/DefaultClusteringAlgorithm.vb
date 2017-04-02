Imports System.Collections.Generic
Imports Microsoft.VisualBasic.DataMining.HierarchicalClustering.Hierarchy

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

Public Class DefaultClusteringAlgorithm
    Implements ClusteringAlgorithm

    Public Function performClustering(distances As Double()(), clusterNames As String(), linkageStrategy As LinkageStrategy) As Cluster Implements ClusteringAlgorithm.performClustering

        checkArguments(distances, clusterNames, linkageStrategy)
        ' Setup model 
        Dim clusters As IList(Of Cluster) = createClusters(clusterNames)
        Dim linkages As DistanceMap = createLinkages(distances, clusters)

        ' Process 
        Dim builder As New HierarchyBuilder(clusters, linkages)
        Do While Not builder.TreeComplete
            builder.agglomerate(linkageStrategy)
        Loop

        Return builder.RootCluster
    End Function

    Public Function performFlatClustering(distances As Double()(), clusterNames As String(), linkageStrategy As LinkageStrategy, threshold As Double) As IList(Of Cluster) Implements ClusteringAlgorithm.performFlatClustering

        checkArguments(distances, clusterNames, linkageStrategy)
        ' Setup model 
        Dim clusters As IList(Of Cluster) = createClusters(clusterNames)
        Dim linkages As DistanceMap = createLinkages(distances, clusters)

        ' Process 
        Dim builder As New HierarchyBuilder(clusters, linkages)
        Return builder.flatAgg(linkageStrategy, threshold)
    End Function

    Private Sub checkArguments(distances As Double()(), clusterNames As String(), linkageStrategy As LinkageStrategy)
        If distances Is Nothing OrElse distances.Length = 0 OrElse distances(0).Length <> distances.Length Then Throw New System.ArgumentException("Invalid distance matrix")
        If distances.Length <> clusterNames.Length Then Throw New System.ArgumentException("Invalid cluster name array")
        If linkageStrategy Is Nothing Then Throw New System.ArgumentException("Undefined linkage strategy")
        Dim uniqueCount As Integer = clusterNames.Distinct.Count
        If uniqueCount <> clusterNames.Length Then Throw New System.ArgumentException("Duplicate names")
    End Sub

    Public Function performWeightedClustering(distances As Double()(), clusterNames As String(), weights As Double(), linkageStrategy As LinkageStrategy) As Cluster Implements ClusteringAlgorithm.performWeightedClustering

        checkArguments(distances, clusterNames, linkageStrategy)

        If weights.Length <> clusterNames.Length Then Throw New System.ArgumentException("Invalid weights array")

        ' Setup model 
        Dim clusters As IList(Of Cluster) = createClusters(clusterNames, weights)
        Dim linkages As DistanceMap = createLinkages(distances, clusters)

        ' Process 
        Dim builder As New HierarchyBuilder(clusters, linkages)
        Do While Not builder.TreeComplete
            builder.agglomerate(linkageStrategy)
        Loop

        Return builder.RootCluster
    End Function

    Private Function createLinkages(distances As Double()(), clusters As IList(Of Cluster)) As DistanceMap
        Dim linkages As New DistanceMap
        For col As Integer = 0 To clusters.Count - 1
            For row As Integer = col + 1 To clusters.Count - 1
                Dim link As New HierarchyTreeNode
                Dim lCluster As Cluster = clusters(col)
                Dim rCluster As Cluster = clusters(row)
                link.LinkageDistance = distances(col)(row)
                link.lCluster = (lCluster)
                link.rCluster = (rCluster)
                linkages.add(link)
            Next row
        Next col
        Return linkages
    End Function

    Private Function createClusters(clusterNames As String()) As IList(Of Cluster)
        Dim clusters As IList(Of Cluster) = New List(Of Cluster)
        For Each clusterName As String In clusterNames
            Dim cluster As New Cluster(clusterName)
            clusters.Add(cluster)
        Next clusterName
        Return clusters
    End Function

    Private Function createClusters(clusterNames As String(), weights As Double()) As IList(Of Cluster)
        Dim clusters As IList(Of Cluster) = New List(Of Cluster)
        For i As Integer = 0 To weights.Length - 1
            Dim cluster As New Cluster(clusterNames(i))
            cluster.Distance = New Distance(0.0, weights(i))
            clusters.Add(cluster)
        Next i
        Return clusters
    End Function

End Class