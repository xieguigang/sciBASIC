#Region "Microsoft.VisualBasic::59621032113849533ede49e629ae4ea9, Data_science\DataMining\DataMining\Clustering\HDBSCAN\Hdbscanstar\HdbscanAlgorithm.vb"

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

    '   Total Lines: 546
    '    Code Lines: 345
    ' Comment Lines: 115
    '   Blank Lines: 86
    '     File Size: 30.10 KB


    '     Class HdbscanAlgorithm
    ' 
    '         Function: CalculateCoreDistances, CalculateOutlierScores, ComputeHierarchyAndClusterTree, ConstructMst, CreateNewCluster
    '                   FindProminentClusters, PropagateTree
    ' 
    '         Sub: CalculateNumConstraintsSatisfied
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel

Namespace HDBSCAN.Hdbscanstar
    Public Class HdbscanAlgorithm
        ''' <summary>
        ''' Calculates the core distances for each point in the data set, given some value for k.
        ''' </summary>
        ''' <param name="distances">The function to get the distance</param>
        ''' <param name="numPoints">The number of elements in dataset</param>
        ''' <param name="k">Each point's core distance will be it's distance to the kth nearest neighbor</param>
        ''' <returns> An array of core distances</returns>
        Public Shared Function CalculateCoreDistances(distances As Func(Of Integer, Integer, Double), numPoints As Integer, k As Integer) As Double()
            Dim numNeighbors = k - 1
            Dim coreDistances = New Double(numPoints - 1) {}

            If k = 1 Then
                For point = 0 To numPoints - 1
                    coreDistances(point) = 0
                Next
                Return coreDistances
            End If

            For point = 0 To numPoints - 1
                Dim kNNDistances = New Double(numNeighbors - 1) {}   'Sorted nearest distances found so far
                For i = 0 To numNeighbors - 1
                    kNNDistances(i) = Double.MaxValue
                Next

                For neighbor = 0 To numPoints - 1
                    If point = neighbor Then Continue For

                    Dim distance = distances(point, neighbor)

                    'Check at which position in the nearest distances the current distance would fit:
                    Dim neighborIndex = numNeighbors
                    While neighborIndex >= 1 AndAlso distance < kNNDistances(neighborIndex - 1)
                        neighborIndex -= 1
                    End While

                    'Shift elements in the array to make room for the current distance:
                    If neighborIndex < numNeighbors Then
                        For shiftIndex = numNeighbors - 1 To neighborIndex + 1 Step -1
                            kNNDistances(shiftIndex) = kNNDistances(shiftIndex - 1)
                        Next
                        kNNDistances(neighborIndex) = distance
                    End If
                Next
                coreDistances(point) = kNNDistances(numNeighbors - 1)
            Next
            Return coreDistances
        End Function

        ''' <summary>
        ''' Constructs the minimum spanning tree of mutual reachability distances for the data set, given
        ''' the core distances for each point.
        ''' </summary>
        ''' <param name="distances">The function to get the distance</param>
        ''' <param name="numPoints">The number of elements in dataset</param>
        ''' <param name="coreDistances">An array of core distances for each data point</param>
        ''' <param name="selfEdges">If each point should have an edge to itself with weight equal to core distance</param>
        ''' <returns> An MST for the data set using the mutual reachability distances</returns>
        Public Shared Function ConstructMst(distances As Func(Of Integer, Integer, Double), numPoints As Integer, coreDistances As Double(), selfEdges As Boolean) As UndirectedGraph
            Dim selfEdgeCapacity = 0
            If selfEdges Then selfEdgeCapacity = numPoints

            'One bit is set (true) for each attached point, or unset (false) for unattached points:
            Dim attachedPoints = New BitSet(capacity:=numPoints * 2)

            'Each point has a current neighbor point in the tree, and a current nearest distance:
            Dim nearestMRDNeighbors = New Integer(numPoints - 1 + selfEdgeCapacity - 1) {}
            Dim nearestMRDDistances = New Double(numPoints - 1 + selfEdgeCapacity - 1) {}

            For i = 0 To numPoints - 1 - 1
                nearestMRDDistances(i) = Double.MaxValue
            Next

            'The MST is expanded starting with the last point in the data set:
            Dim currentPoint = numPoints - 1
            Dim numAttachedPoints = 1
            attachedPoints.Set(numPoints - 1)

            'Continue attaching points to the MST until all points are attached:
            While numAttachedPoints < numPoints
                Dim nearestMRDPoint = -1
                Dim nearestMRDDistance = Double.MaxValue

                'Iterate through all unattached points, updating distances using the current point:
                For neighbor = 0 To numPoints - 1
                    If currentPoint = neighbor Then Continue For

                    If attachedPoints.Get(neighbor) Then Continue For

                    Dim distance = distances(currentPoint, neighbor)
                    Dim mutualReachabiltiyDistance = distance

                    If coreDistances(currentPoint) > mutualReachabiltiyDistance Then mutualReachabiltiyDistance = coreDistances(currentPoint)

                    If coreDistances(neighbor) > mutualReachabiltiyDistance Then mutualReachabiltiyDistance = coreDistances(neighbor)

                    If mutualReachabiltiyDistance < nearestMRDDistances(neighbor) Then
                        nearestMRDDistances(neighbor) = mutualReachabiltiyDistance
                        nearestMRDNeighbors(neighbor) = currentPoint
                    End If

                    'Check if the unattached point being updated is the closest to the tree:
                    If nearestMRDDistances(neighbor) <= nearestMRDDistance Then
                        nearestMRDDistance = nearestMRDDistances(neighbor)
                        nearestMRDPoint = neighbor
                    End If
                Next

                'Attach the closest point found in this iteration to the tree:
                attachedPoints.Set(nearestMRDPoint)
                numAttachedPoints += 1
                currentPoint = nearestMRDPoint
            End While

            'Create an array for vertices in the tree that each point attached to:
            Dim otherVertexIndices = New Integer(numPoints - 1 + selfEdgeCapacity - 1) {}
            For i = 0 To numPoints - 1 - 1
                otherVertexIndices(i) = i
            Next

            'If necessary, attach self edges:
            If selfEdges Then
                For i = numPoints - 1 To numPoints * 2 - 1 - 1
                    Dim vertex = i - (numPoints - 1)
                    nearestMRDNeighbors(i) = vertex
                    otherVertexIndices(i) = vertex
                    nearestMRDDistances(i) = coreDistances(vertex)
                Next
            End If

            Return New UndirectedGraph(numPoints, nearestMRDNeighbors, otherVertexIndices, nearestMRDDistances)
        End Function

        ''' <summary>
        ''' Computes the hierarchy and cluster tree from the minimum spanning tree, writing both to file, 
        ''' and returns the cluster tree.  Additionally, the level at which each point becomes noise is
        ''' computed.  Note that the minimum spanning tree may also have self edges (meaning it is not
        ''' a true MST).
        ''' </summary>
        ''' <param name="mst">A minimum spanning tree which has been sorted by edge weight in descending order</param>
        ''' <param name="minClusterSize">The minimum number of points which a cluster needs to be a valid cluster</param>
        ''' <param name="constraints">An optional List of Constraints to calculate cluster constraint satisfaction</param>
        ''' <param name="hierarchy">The hierarchy output</param>
        ''' <param name="pointNoiseLevels">A double[] to be filled with the levels at which each point becomes noise</param>
        ''' <param name="pointLastClusters">An int[] to be filled with the last label each point had before becoming noise</param>
        ''' <returns>The cluster tree</returns>
        Public Shared Function ComputeHierarchyAndClusterTree(mst As UndirectedGraph, minClusterSize As Integer, constraints As List(Of HdbscanConstraint), hierarchy As List(Of Integer()), pointNoiseLevels As Double(), pointLastClusters As Integer()) As List(Of Cluster)
            Dim hierarchyPosition = 0

            'The current edge being removed from the MST:
            Dim currentEdgeIndex = mst.GetNumEdges() - 1
            Dim nextClusterLabel = 2
            Dim nextLevelSignificant = True

            'The previous and current cluster numbers of each point in the data set:
            Dim previousClusterLabels = New Integer(mst.GetNumVertices() - 1) {}
            Dim currentClusterLabels = New Integer(mst.GetNumVertices() - 1) {}

            For i = 0 To currentClusterLabels.Length - 1
                currentClusterLabels(i) = 1
                previousClusterLabels(i) = 1
            Next

            'A list of clusters in the cluster tree, with the 0th cluster (noise) null:
            Dim clusters = New List(Of Cluster)()
            clusters.Add(Nothing)
            clusters.Add(New Cluster(1, Nothing, Double.NaN, mst.GetNumVertices()))

            'Calculate number of constraints satisfied for cluster 1:
            Dim clusterOne = New SortedSet(Of Integer)()
            clusterOne.Add(1)
            CalculateNumConstraintsSatisfied(clusterOne, clusters, constraints, currentClusterLabels)

            'Sets for the clusters and vertices that are affected by the edge(s) being removed:
            Dim affectedClusterLabels = New SortedSet(Of Integer)()
            Dim affectedVertices = New SortedSet(Of Integer)()

            While currentEdgeIndex >= 0
                Dim currentEdgeWeight = mst.GetEdgeWeightAtIndex(currentEdgeIndex)
                Dim newClusters = New List(Of Cluster)()

                'Remove all edges tied with the current edge weight, and store relevant clusters and vertices:
                While currentEdgeIndex >= 0 AndAlso mst.GetEdgeWeightAtIndex(currentEdgeIndex) = currentEdgeWeight
                    Dim firstVertex = mst.GetFirstVertexAtIndex(currentEdgeIndex)
                    Dim secondVertex = mst.GetSecondVertexAtIndex(currentEdgeIndex)
                    mst.GetEdgeListForVertex(firstVertex).Remove(secondVertex)
                    mst.GetEdgeListForVertex(secondVertex).Remove(firstVertex)

                    If currentClusterLabels(firstVertex) = 0 Then
                        currentEdgeIndex -= 1
                        Continue While
                    End If
                    affectedVertices.Add(firstVertex)
                    affectedVertices.Add(secondVertex)
                    affectedClusterLabels.Add(currentClusterLabels(firstVertex))
                    currentEdgeIndex -= 1
                End While

                If Not affectedClusterLabels.Any() Then Continue While

                'Check each cluster affected for a possible split:
                While affectedClusterLabels.Any()
                    Dim examinedClusterLabel = affectedClusterLabels.Last()
                    affectedClusterLabels.Remove(examinedClusterLabel)

                    Dim examinedVertices = New SortedSet(Of Integer)()

                    'Get all affected vertices that are members of the cluster currently being examined:
                    For Each vertex In affectedVertices.ToList()
                        If currentClusterLabels(vertex) = examinedClusterLabel Then
                            examinedVertices.Add(vertex)
                            affectedVertices.Remove(vertex)
                        End If
                    Next

                    Dim firstChildCluster As SortedSet(Of Integer) = Nothing
                    Dim unexploredFirstChildClusterPoints As LinkedList(Of Integer) = Nothing
                    Dim numChildClusters = 0

                    ' 
                    ' * Check if the cluster has split Or shrunk by exploring the graph from each affected
                    ' * vertex.  If there are two Or more valid child clusters (each has >= minClusterSize
                    ' * points), the cluster has split.
                    ' * Note that firstChildCluster will only be fully explored if there Is a cluster
                    ' * split, otherwise, only spurious components are fully explored, in order to label 
                    ' * them noise.

                    While examinedVertices.Any()
                        Dim constructingSubCluster = New SortedSet(Of Integer)()
                        Dim unexploredSubClusterPoints = New LinkedList(Of Integer)()
                        Dim anyEdges = False
                        Dim incrementedChildCount = False
                        Dim rootVertex = examinedVertices.Last()
                        constructingSubCluster.Add(rootVertex)
                        unexploredSubClusterPoints.AddLast(rootVertex)
                        examinedVertices.Remove(rootVertex)

                        'Explore this potential child cluster as long as there are unexplored points:
                        While unexploredSubClusterPoints.Any()
                            Dim vertexToExplore As LinkedListNode(Of Integer) = unexploredSubClusterPoints.First()
                            unexploredSubClusterPoints.RemoveFirst()

                            For Each neighbor In mst.GetEdgeListForVertex(vertexToExplore.Value)
                                anyEdges = True
                                If constructingSubCluster.Add(neighbor) Then
                                    unexploredSubClusterPoints.AddLast(neighbor)
                                    examinedVertices.Remove(neighbor)
                                End If
                            Next

                            'Check if this potential child cluster is a valid cluster:
                            If Not incrementedChildCount AndAlso constructingSubCluster.Count >= minClusterSize AndAlso anyEdges Then
                                incrementedChildCount = True
                                numChildClusters += 1

                                'If this is the first valid child cluster, stop exploring it:
                                If firstChildCluster Is Nothing Then
                                    firstChildCluster = constructingSubCluster
                                    unexploredFirstChildClusterPoints = unexploredSubClusterPoints
                                    Exit While
                                End If
                            End If
                        End While

                        'If there could be a split, and this child cluster is valid:
                        If numChildClusters >= 2 AndAlso constructingSubCluster.Count >= minClusterSize AndAlso anyEdges Then
                            'Check this child cluster is not equal to the unexplored first child cluster:
                            Dim firstChildClusterMember = firstChildCluster.Last()
                            If constructingSubCluster.Contains(firstChildClusterMember) Then
                                'Otherwise, create a new cluster:
                                numChildClusters -= 1
                            Else
                                Dim newCluster = CreateNewCluster(constructingSubCluster, currentClusterLabels, clusters(examinedClusterLabel), nextClusterLabel, currentEdgeWeight)
                                newClusters.Add(newCluster)
                                clusters.Add(newCluster)
                                nextClusterLabel += 1
                            End If
                            'If this child cluster is not valid cluster, assign it to noise:
                        ElseIf constructingSubCluster.Count < minClusterSize OrElse Not anyEdges Then
                            CreateNewCluster(constructingSubCluster, currentClusterLabels, clusters(examinedClusterLabel), 0, currentEdgeWeight)

                            For Each point In constructingSubCluster
                                pointNoiseLevels(point) = currentEdgeWeight
                                pointLastClusters(point) = examinedClusterLabel
                            Next
                        End If
                    End While

                    'Finish exploring and cluster the first child cluster if there was a split and it was not already clustered:
                    If numChildClusters >= 2 AndAlso currentClusterLabels(firstChildCluster.First()) = examinedClusterLabel Then
                        While unexploredFirstChildClusterPoints.Any()
                            Dim vertexToExplore As LinkedListNode(Of Integer) = unexploredFirstChildClusterPoints.First()
                            unexploredFirstChildClusterPoints.RemoveFirst()
                            For Each neighbor As Integer In mst.GetEdgeListForVertex(vertexToExplore.Value)
                                If firstChildCluster.Add(neighbor) Then unexploredFirstChildClusterPoints.AddLast(neighbor)
                            Next
                        End While
                        Dim newCluster = CreateNewCluster(firstChildCluster, currentClusterLabels, clusters(examinedClusterLabel), nextClusterLabel, currentEdgeWeight)
                        newClusters.Add(newCluster)
                        clusters.Add(newCluster)
                        nextClusterLabel += 1
                    End If
                End While

                'Write out the current level of the hierarchy:
                If nextLevelSignificant OrElse newClusters.Any() Then
                    Dim lineContents = New Integer(previousClusterLabels.Length - 1) {}
                    For i = 0 To previousClusterLabels.Length - 1
                        lineContents(i) = previousClusterLabels(i)
                    Next
                    hierarchy.Add(lineContents)
                    hierarchyPosition += 1
                End If

                'Assign file offsets and calculate the number of constraints satisfied:
                Dim newClusterLabels = New SortedSet(Of Integer)()
                For Each newCluster In newClusters
                    newCluster.HierarchyPosition = hierarchyPosition
                    newClusterLabels.Add(newCluster.Label)
                Next

                If newClusterLabels.Any() Then CalculateNumConstraintsSatisfied(newClusterLabels, clusters, constraints, currentClusterLabels)

                For i = 0 To previousClusterLabels.Length - 1
                    previousClusterLabels(i) = currentClusterLabels(i)
                Next

                If Not newClusters.Any() Then
                    nextLevelSignificant = False
                Else
                    nextLevelSignificant = True
                End If
            End While

            'Write out the final level of the hierarchy (all points noise):
            If True Then
                Dim lineContents = New Integer(previousClusterLabels.Length + 1 - 1) {}
                For i = 0 To previousClusterLabels.Length - 1
                    lineContents(i) = 0
                Next
                hierarchy.Add(lineContents)
            End If

            Return clusters
        End Function

        ''' <summary>
        ''' Propagates constraint satisfaction, stability, and lowest child death level from each child
        ''' cluster to each parent cluster in the tree.  This method must be called before calling
        ''' findProminentClusters() or calculateOutlierScores().
        ''' </summary>
        ''' <param name="clusters">A list of Clusters forming a cluster tree</param>
        ''' <returns>true if there are any clusters with infinite stability, false otherwise</returns>
        Public Shared Function PropagateTree(clusters As List(Of Cluster)) As Boolean
            Dim clustersToExamine = New SortedDictionary(Of Integer, Cluster)()
            Dim addedToExaminationList = New BitSet(capacity:=0, defaultValue:=False)
            Dim infiniteStability = False

            'Find all leaf clusters in the cluster tree:
            For Each cluster In clusters
                If cluster IsNot Nothing AndAlso Not cluster.HasChildren Then
                    Dim label = cluster.Label
                    clustersToExamine.Remove(label)
                    clustersToExamine.Add(label, cluster)
                    addedToExaminationList.Set(label)
                End If
            Next

            'Iterate through every cluster, propagating stability from children to parents:
            While clustersToExamine.Any()
                Dim currentKeyValue = clustersToExamine.Last()
                Dim currentCluster = currentKeyValue.Value
                clustersToExamine.Remove(currentKeyValue.Key)

                currentCluster.Propagate()

                If currentCluster.Stability = Double.PositiveInfinity Then infiniteStability = True

                If currentCluster.Parent IsNot Nothing Then
                    Dim parent = currentCluster.Parent
                    Dim label = parent.Label

                    If Not addedToExaminationList.Get(label) Then
                        clustersToExamine.Remove(label)
                        clustersToExamine.Add(label, parent)
                        addedToExaminationList.Set(label)
                    End If
                End If
            End While

            Return infiniteStability
        End Function

        ''' <summary>
        ''' Produces a flat clustering result using constraint satisfaction and cluster stability, and 
        ''' returns an array of labels.  propagateTree() must be called before calling this method.
        ''' </summary>
        ''' <param name="clusters">A list of Clusters forming a cluster tree which has already been propagated</param>
        ''' <param name="hierarchy">The hierarchy content</param>
        ''' <param name="numPoints">The number of points in the original data set</param>
        ''' <returns>An array of labels for the flat clustering result</returns>
        Public Shared Function FindProminentClusters(clusters As List(Of Cluster), hierarchy As List(Of Integer()), numPoints As Integer) As Integer()
            'Take the list of propagated clusters from the root cluster:
            Dim solution = clusters(1).PropagatedDescendants

            Dim flatPartitioning = New Integer(numPoints - 1) {}

            'Store all the hierarchy positions at which to find the birth points for the flat clustering:
            Dim significantHierarchyPositions = New SortedDictionary(Of Integer, List(Of Integer))()

            For Each cluster In solution
                Dim hierarchyPosition = cluster.HierarchyPosition
                If significantHierarchyPositions.ContainsKey(hierarchyPosition) Then
                    significantHierarchyPositions(hierarchyPosition).Add(cluster.Label)
                Else
                    significantHierarchyPositions(hierarchyPosition) = New List(Of Integer) From {
                        cluster.Label
                    }
                End If
            Next

            'Go through the hierarchy file, setting labels for the flat clustering:
            While significantHierarchyPositions.Any()
                Dim entry = significantHierarchyPositions.First()
                significantHierarchyPositions.Remove(entry.Key)

                Dim clusterList = entry.Value
                Dim hierarchyPosition = entry.Key
                Dim lineContents = hierarchy(hierarchyPosition)

                For i = 0 To lineContents.Length - 1
                    Dim label = lineContents(i)
                    If clusterList.Contains(label) Then flatPartitioning(i) = label
                Next
            End While
            Return flatPartitioning
        End Function

        ''' <summary>
        ''' Produces the outlier score for each point in the data set, and returns a sorted list of outlier
        ''' scores.  propagateTree() must be called before calling this method.
        ''' </summary>
        ''' <param name="clusters">A list of Clusters forming a cluster tree which has already been propagated</param>
        ''' <param name="pointNoiseLevels">A double[] with the levels at which each point became noise</param>
        ''' <param name="pointLastClusters">An int[] with the last label each point had before becoming noise</param>
        ''' <param name="coreDistances">An array of core distances for each data point</param>
        ''' <returns>An List of OutlierScores, sorted in descending order</returns>
        Public Shared Function CalculateOutlierScores(clusters As List(Of Cluster), pointNoiseLevels As Double(), pointLastClusters As Integer(), coreDistances As Double()) As List(Of OutlierScore)
            Dim numPoints = pointNoiseLevels.Length
            Dim outlierScores = New List(Of OutlierScore)(numPoints)

            'Iterate through each point, calculating its outlier score:
            For i = 0 To numPoints - 1
                Dim epsilonMax = clusters(pointLastClusters(i)).PropagatedLowestChildDeathLevel
                Dim epsilon = pointNoiseLevels(i)
                Dim score As Double = 0

                If epsilon <> 0 Then score = 1 - epsilonMax / epsilon

                outlierScores.Add(New OutlierScore(score, coreDistances(i), i))
            Next

            'Sort the outlier scores:
            outlierScores.Sort()

            Return outlierScores
        End Function

        ''' <summary>
        ''' Removes the set of points from their parent Cluster, and creates a new Cluster, provided the
        ''' clusterId is not 0 (noise).
        ''' </summary>
        ''' <param name="points">The set of points to be in the new Cluster</param>
        ''' <param name="clusterLabels">An array of cluster labels, which will be modified</param>
        ''' <param name="parentCluster">The parent Cluster of the new Cluster being created</param>
        ''' <param name="clusterLabel">The label of the new Cluster </param>
        ''' <param name="edgeWeight">The edge weight at which to remove the points from their previous Cluster</param>
        ''' <returns>The new Cluster, or null if the clusterId was 0</returns>
        Private Shared Function CreateNewCluster(points As SortedSet(Of Integer), clusterLabels As Integer(), parentCluster As Cluster, clusterLabel As Integer, edgeWeight As Double) As Cluster
            For Each point In points
                clusterLabels(point) = clusterLabel
            Next

            parentCluster.DetachPoints(points.Count, edgeWeight)

            If clusterLabel <> 0 Then Return New Cluster(clusterLabel, parentCluster, edgeWeight, points.Count)

            parentCluster.AddPointsToVirtualChildCluster(points)
            Return Nothing
        End Function

        ''' <summary>
        ''' Calculates the number of constraints satisfied by the new clusters and virtual children of the
        ''' parents of the new clusters.
        ''' </summary>
        ''' <param name="newClusterLabels">Labels of new clusters</param>
        ''' <param name="clusters">An List of clusters</param>
        ''' <param name="constraints">An List of constraints</param>
        ''' <param name="clusterLabels">An array of current cluster labels for points</param>
        Private Shared Sub CalculateNumConstraintsSatisfied(newClusterLabels As SortedSet(Of Integer), clusters As List(Of Cluster), constraints As List(Of HdbscanConstraint), clusterLabels As Integer())
            If constraints Is Nothing Then Return

            Dim parents = New List(Of Cluster)()

            For Each label In newClusterLabels
                Dim parent = clusters(label).Parent
                If parent IsNot Nothing AndAlso Not parents.Contains(parent) Then parents.Add(parent)
            Next

            For Each constraint In constraints
                Dim labelA = clusterLabels(constraint.GetPointA())
                Dim labelB = clusterLabels(constraint.GetPointB())

                If constraint.GetConstraintType() = HdbscanConstraintType.MustLink AndAlso labelA = labelB Then
                    If newClusterLabels.Contains(labelA) Then clusters(labelA).AddConstraintsSatisfied(2)
                ElseIf constraint.GetConstraintType() = HdbscanConstraintType.CannotLink AndAlso (labelA <> labelB OrElse labelA = 0) Then
                    If labelA <> 0 AndAlso newClusterLabels.Contains(labelA) Then clusters(labelA).AddConstraintsSatisfied(1)
                    If labelB <> 0 AndAlso newClusterLabels.Contains(labelB) Then clusters(labelB).AddConstraintsSatisfied(1)
                    If labelA = 0 Then
                        For Each parent In parents
                            If parent.VirtualChildClusterConstraintsPoint(constraint.GetPointA()) Then
                                parent.AddVirtualChildConstraintsSatisfied(1)
                                Exit For
                            End If
                        Next
                    End If
                    If labelB = 0 Then
                        For Each parent In parents
                            If parent.VirtualChildClusterConstraintsPoint(constraint.GetPointB()) Then
                                parent.AddVirtualChildConstraintsSatisfied(1)
                                Exit For
                            End If
                        Next
                    End If
                End If
            Next

            For Each parent In parents
                parent.ReleaseVirtualChildCluster()
            Next
        End Sub
    End Class
End Namespace
