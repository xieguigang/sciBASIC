#Region "Microsoft.VisualBasic::e2446cf9274b188b39e302338285633f, Data_science\DataMining\hierarchical-clustering\hierarchical-clustering\ClusteringAlgorithm\DefaultClusteringAlgorithm.vb"
' Author:
' 
' asuka (amethyst.asuka@gcmodeller.org)
' xie (genetics@smrucc.org)
' xieguigang (xie.guigang@live.com)
' 
' Copyright (c)2018 GPL3 Licensed
' 
' 
' GNU GENERAL PUBLIC LICENSE (GPL3)
' 
' 
' This program is free software: you can redistribute it and/or modify
' it under the terms of the GNU General Public License as published by
' the Free Software Foundation, either version3 of the License, or
' (at your option) any later version.
' 
' This program is distributed in the hope that it will be useful,
' but WITHOUT ANY WARRANTY; without even the implied warranty of
' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
' GNU General Public License for more details.
' 
' You should have received a copy of the GNU General Public License
' along with this program. If not, see <http://www.gnu.org/licenses/>.
' /********************************************************************************/
' Summaries:
' Code Statistics:
' Total Lines:172
' Code Lines:110 (63.95%)
' Comment Lines:28 (16.28%)
' - Xml Docs:17.86%
' 
' Blank Lines:34 (19.77%)
' File Size:7.30 KB
' Class DefaultClusteringAlgorithm
' 
' Properties: debug
' 
' Function: alignRow, (+2 Overloads) createClusters, createLinkages, performClustering, performFlatClustering
' performWeightedClustering
' 
' Sub: checkArguments
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
' Copyright2013 Lars Behnke
' <p/>
' Licensed under the Apache License, Version2.0 (the "License");
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
''' <summary>
''' Default implementation of the <see cref="ClusteringAlgorithm"/>  interface that provides
''' hierarchical agglomerative clustering functionality. Supports standard clustering,
''' flat (threshold-based) clustering, and weighted clustering using configurable linkage strategies.
''' </summary>
Public Class DefaultClusteringAlgorithm : Implements ClusteringAlgorithm

    ''' <summary>
    ''' Gets or sets a value indicating whether debug progress information should be
    ''' printed to the console during clustering execution.
    ''' </summary>
    ''' <returns><c>True</c> if debug output is enabled; otherwise, <c>False</c>.
    ''' Default value is <c>False</c>.</returns>
    Public Property debug As Boolean = False

    ''' <summary>
    ''' Performs full hierarchical agglomerative clustering on the given distance matrix,
    ''' using the specified linkage strategy. Progress is reported via a console progress bar.
    ''' </summary>
    ''' <param name="distances">A symmetric square distance matrix where <c>distances(i)(j)</c>
    ''' represents the distance between the i-th and j-th data points.</param>
    ''' <param name="clusterNames">An array of names for each data point/cluster.</param>
    ''' <param name="linkageStrategy">The linkage strategy (e.g., single, complete, average, weighted)
    ''' that defines how inter-cluster distances are computed.</param>
    ''' <returns>A <see cref="Cluster"/>  object representing the root of the complete hierarchical
    ''' clustering tree (dendrogram).</returns>
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

    ''' <summary>
    ''' Performs flat (non-hierarchical) clustering by cutting the dendrogram at a specified
    ''' distance threshold. Returns a flat list of clusters where the inter-cluster distance
    ''' exceeds the given threshold.
    ''' </summary>
    ''' <param name="distances">A symmetric square distance matrix where <c>distances(i)(j)</c>
    ''' represents the distance between the i-th and j-th data points.</param>
    ''' <param name="clusterNames">An array of names for each data point/cluster.</param>
    ''' <param name="linkageStrategy">The linkage strategy used to compute inter-cluster distances.</param>
    ''' <param name="threshold">The distance threshold at which to cut the dendrogram. Clusters
    ''' merged below this threshold will be grouped together.</param>
    ''' <returns>A list of <see cref="Cluster"/>  objects representing the flat clustering result.</returns>
    Public Function performFlatClustering(distances As Double()(), clusterNames$(), linkageStrategy As LinkageStrategy, threshold As Double) As IList(Of Cluster) Implements ClusteringAlgorithm.performFlatClustering
        Call checkArguments(distances, clusterNames, linkageStrategy)
        ' Setup model 
        Dim clusters As IList(Of Cluster) = createClusters(clusterNames)
        Dim linkages As DistanceMap = createLinkages(distances, clusters)
        ' Process 
        Dim builder As New HierarchyBuilder(clusters, linkages)
        Return builder.flatAgg(linkageStrategy, threshold)
    End Function

    ''' <summary>
    ''' Validates the input arguments for consistency and correctness before clustering begins.
    ''' Checks that the distance matrix is valid, the cluster names array matches the matrix size,
    ''' the linkage strategy is provided, and that there are no duplicate cluster names.
    ''' </summary>
    ''' <param name="distances">The distance matrix to validate.</param>
    ''' <param name="clusterNames">The array of cluster names to validate.</param>
    ''' <param name="linkageStrategy">The linkage strategy to validate (must not be <c>Nothing</c>).</param>
    ''' <exception cref="ArgumentException">Thrown when any of the validation checks fail:
    ''' invalid distance matrix, mismatched array lengths, undefined linkage strategy,
    ''' or duplicate cluster names.</exception>
    Private Shared Sub checkArguments(distances As Double()(), clusterNames As String(), linkageStrategy As LinkageStrategy)
        If distances Is Nothing OrElse distances.Length = 0 OrElse distances(0).Length <> distances.Length Then Throw New ArgumentException("Invalid distance matrix")
        If distances.Length <> clusterNames.Length Then Throw New ArgumentException("Invalid cluster name array")
        If linkageStrategy Is Nothing Then Throw New ArgumentException("Undefined linkage strategy")
        Dim uniqueCount As Integer = clusterNames.Distinct.Count
        If uniqueCount <> clusterNames.Length Then
            Throw New ArgumentException("Duplicate names")
        End If
    End Sub

    ''' <summary>
    ''' Performs hierarchical clustering with weighted data points. Each cluster is initialized
    ''' with a weight that influences the subsequent agglomerative process via the linkage strategy.
    ''' </summary>
    ''' <param name="distances">A symmetric square distance matrix where <c>distances(i)(j)</c>
    ''' represents the distance between the i-th and j-th data points.</param>
    ''' <param name="clusterNames">An array of names for each data point/cluster.</param>
    ''' <param name="weights">An array of weights for each data point. The length must match
    ''' the number of cluster names.</param>
    ''' <param name="linkageStrategy">The linkage strategy (e.g., weighted linkage) that uses
    ''' the cluster weights during distance computation.</param>
    ''' <returns>A <see cref="Cluster"/>  object representing the root of the weighted hierarchical
    ''' clustering tree (dendrogram).</returns>
    ''' <exception cref="ArgumentException">Thrown when the weights array length does not match
    ''' the cluster names array, or when other input validation fails.</exception>
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

    ''' <summary>
    ''' Builds the initial distance map (pairwise linkages) between all clusters from the
    ''' distance matrix. For small numbers of clusters (&lt; 100), a simple nested-loop approach
    ''' is used. For larger datasets, parallel LINQ is employed to improve performance.
    ''' </summary>
    ''' <param name="distances">The symmetric square distance matrix.</param>
    ''' <param name="clusters">The list of cluster objects to build linkages for.</param>
    ''' <returns>A <see cref="DistanceMap"/>  containing all pairwise <see cref="HierarchyTreeNode"/> 
    ''' linkages between clusters.</returns>
    Private Function createLinkages(distances As Double()(), clusters As IList(Of Cluster)) As DistanceMap
        If clusters.Count < 100 Then
            Dim linkages As New DistanceMap
            For col As Integer = 0 To clusters.Count - 1
                For row As Integer = col + 1 To clusters.Count - 1
                    Dim link As New HierarchyTreeNode
                    Dim lCluster As Cluster = clusters(col)
                    Dim rCluster As Cluster = clusters(row)
                    link.LinkageDistance = distances(col)(row)
                    link.Left =(lCluster)
                    link.Right =(rCluster)
                    linkages.Add(link)
                Next
            Next

            Return linkages
        Else
            '当数量很大的时候，这里也是一个限速步骤，需要使用并行
            Dim copy As Cluster() = clusters.ToArray
            Dim LQuery = From c As SeqValue(Of Cluster) In clusters.SeqIterator.AsParallel Let col As Integer = c.i Let lCluster As Cluster = c.value Let list = alignRow(lCluster, col, distances, copy) Select list.ToArray
            Dim links = LQuery.IteratesALL.ToArray
            Return New DistanceMap(links)
        End If
    End Function

    ''' <summary>
    ''' Iterator that generates a sequence of <see cref="HierarchyTreeNode"/>  linkages for a single
    ''' row (cluster) against all subsequent clusters in the distance matrix. Used as a helper
    ''' by <see cref="createLinkages"/>  during parallel linkage construction.
    ''' </summary>
    ''' <param name="lCluster">The left (source) cluster for the linkage.</param>
    ''' <param name="col">The column/index of the left cluster in the distance matrix.</param>
    ''' <param name="distances">The symmetric square distance matrix.</param>
    ''' <param name="clusters">The array of all cluster objects.</param>
    ''' <returns>An enumerable sequence of <see cref="HierarchyTreeNode"/>  linkages connecting
    ''' the left cluster to each subsequent right cluster.</returns>
    Private Iterator Function alignRow(lCluster As Cluster, col As Integer, distances As Double()(), clusters As Cluster()) As IEnumerable(Of HierarchyTreeNode)
        Dim n = clusters.Length
        For row As Integer = col + 1 To n - 1
            Dim rCluster As Cluster = clusters(row)
            Dim link As New HierarchyTreeNode With {.LinkageDistance = distances(col)(row), .Left = lCluster, .Right = rCluster}
            Yield link
        Next
    End Function

    ''' <summary>
    ''' Creates a list of <see cref="Cluster"/>  objects from the given array of cluster names.
    ''' Each cluster is initialized with its name and a default distance of zero.
    ''' </summary>
    ''' <param name="clusterNames">An array of strings representing the names of the clusters
    ''' to create.</param>
    ''' <returns>A list of <see cref="Cluster"/>  objects, one for each provided name.</returns>
    Private Shared Function createClusters(clusterNames As String()) As IList(Of Cluster)
        Return clusterNames.Select(Function(clusterName) New Cluster(clusterName)).AsList
    End Function

    ''' <summary>
    ''' Creates a list of <see cref="Cluster"/>  objects with associated weights. Each cluster
    ''' is initialized with its name and a <see cref="Distance"/>  object that carries the
    ''' corresponding weight value. This is used for weighted hierarchical clustering.
    ''' </summary>
    ''' <param name="clusterNames">An array of strings representing the names of the clusters
    ''' to create.</param>
    ''' <param name="weights">An array of weights, one per cluster. The length must match
    ''' <paramref name="clusterNames"/> .</param>
    ''' <returns>A list of <see cref="Cluster"/>  objects, each initialized with its name
    ''' and associated weight.</returns>
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
