Imports System
Imports System.Collections.Generic
Imports System.Text
Imports Microsoft.VisualBasic.DataMining.Framework.KMeans

Namespace KMeans.CompleteLinkage

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <remarks>
    ''' https://github.com/halfjew22/Clustering/blob/master/src/com/lustig/model/Clustering.java
    ''' </remarks>
    Public Class Clustering

        Friend mCompleteLinkageClusters As List(Of Cluster(Of Point))
        Friend mKMeansClusters As List(Of KMeansCluster(Of Point))
        Friend mMinKMeansClusters As List(Of KMeansCluster(Of Point))
        Friend mOriginalCoordinates As List(Of Point)
        ' I'm embarrassed about how bad this code is, sorry
        Friend mOriginalCoordinatesCopy As List(Of Point)
        Friend mNumDesiredClusters As Integer

        Public Const COMPLETE_LINKAGE As Integer = 0
        Public Const LLOYS_METHOD As Integer = 1

        Public Sub New(originalCoordinates As List(Of Point), clusteringMethodFlag As Integer, numClusters As Integer)

            mOriginalCoordinates = originalCoordinates
            mCompleteLinkageClusters = New List(Of Cluster(Of Point))
            mNumDesiredClusters = numClusters

            Select Case clusteringMethodFlag

                Case COMPLETE_LINKAGE
                    performCompleteLinkageClustering()
                Case LLOYS_METHOD
                    mKMeansClusters = New List(Of KMeansCluster(Of Point))
                    mOriginalCoordinatesCopy = New List(Of Point)(mOriginalCoordinates)
                    performLloydsMethodClustering()
                Case Else
                    Console.WriteLine("Unimplemented clustering method.")
            End Select
        End Sub

        Private Sub performCompleteLinkageClustering()

            ' Start by placing each point in its own cluster
            For Each p As Point In mOriginalCoordinates
                mCompleteLinkageClusters.Add(New Cluster(Of Point)(p))
            Next p

            Dim minDistance As Double = Double.MaxValue
            Dim currentDistance As Double

            ' Array to hold pair of closest clusters
            Dim twoClosestClusters As Cluster(Of Point)() = New Cluster(Of Point)(1) {}

            ' while there are more than k clusters
            Do While mCompleteLinkageClusters.Count > mNumDesiredClusters

                ' Calculate and store the distance between each pair of clusters, finding the minimum distance between clusters
                For i As Integer = 0 To mCompleteLinkageClusters.Count - 1
                    For j As Integer = i + 1 To mCompleteLinkageClusters.Count - 1
                        Dim c1 As Cluster(Of Point) = mCompleteLinkageClusters(i)
                        Dim c2 As Cluster(Of Point) = mCompleteLinkageClusters(j)
                        currentDistance = ClusterAPI.completeLinkageDistance(c1, c2)
                        If currentDistance < minDistance Then
                            twoClosestClusters(0) = c1
                            twoClosestClusters(1) = c2
                            minDistance = currentDistance
                        End If
                    Next
                Next

                Dim mergedCluster As Cluster(Of Point) = ClusterAPI.mergeClusters(twoClosestClusters(0), twoClosestClusters(1))
                mCompleteLinkageClusters.Add(mergedCluster)
                mCompleteLinkageClusters.Remove(twoClosestClusters(0))
                mCompleteLinkageClusters.Remove(twoClosestClusters(1))

                ' reset min distance so Array is overwritten
                minDistance = Double.MaxValue

                ' just to be safe, nullify array as well
                twoClosestClusters(0) = Nothing
                twoClosestClusters(1) = Nothing
            Loop
        End Sub

        Private Sub performLloydsMethodClustering()
            Dim minKMeansCost As Double = Double.MaxValue

            For runningTimes As Integer = 0 To 99
                ' for grabbing random points from input file
                Dim random As New Random

                If mKMeansClusters.Count > 0 Then mKMeansClusters.Clear()

                ' First, create number of desired clusters
                ' Pick K (mNumDesiredRandomPoints)
                For i As Integer = 0 To mNumDesiredClusters - 1
                    mKMeansClusters.Add(New KMeansCluster(Of Point))
                    Dim randomIndex As Integer = random.Next(mOriginalCoordinates.Count)
                    mKMeansClusters(i).Center = mOriginalCoordinates(randomIndex)
                Next

                Dim oldKmeansCost As Double = 0
                Dim currentKMeansCost As Double = 0

                ' Until convergence:
                Do
                    For i As Integer = 0 To mKMeansClusters.Count - 1
                        mKMeansClusters(i).refresh()
                    Next
                    oldKmeansCost = currentKMeansCost

                    Dim closestClusterToPoint As KMeansCluster(Of Point) = Nothing

                    Dim minClusterIndex As Integer = Integer.MaxValue

                    ' find the closest cluster to each point
                    For i As Integer = 0 To mOriginalCoordinatesCopy.Count - 1
                        Dim minDistanceToCluster As Double = Double.MaxValue
                        For j As Integer = 0 To mKMeansClusters.Count - 1

                            Dim distanceToCluster As Double = mOriginalCoordinatesCopy(i).distanceToOtherPoint(mKMeansClusters(j).Center)

                            If distanceToCluster < minDistanceToCluster Then
                                closestClusterToPoint = mKMeansClusters(j)
                                minDistanceToCluster = distanceToCluster
                                minClusterIndex = j
                            End If
                        Next

                        ' Indexing by one with naming, so add one
                        mOriginalCoordinatesCopy(i).SetKMeansCluster(minClusterIndex + 1)

                        mKMeansClusters(minClusterIndex).Add(mOriginalCoordinatesCopy(i))
                    Next

                    ' reset kmeans cost
                    currentKMeansCost = 0

                    ' calculate the new center
                    For i As Integer = 0 To mKMeansClusters.Count - 1
                        mKMeansClusters(i).Center = mKMeansClusters(i).CalculateCenter()
                        currentKMeansCost += mKMeansClusters(i).CalculateKMeansCost()
                    Next i
                Loop While Math.Abs(oldKmeansCost - currentKMeansCost) > 1

                If currentKMeansCost < minKMeansCost Then
                    minKMeansCost = currentKMeansCost
                    mMinKMeansClusters = New List(Of KMeansCluster(Of Point))(mKMeansClusters)
                End If
            Next
        End Sub

        Public ReadOnly Property LloydsPoints As List(Of Point)
            Get
                Return mOriginalCoordinatesCopy
            End Get
        End Property

        Public ReadOnly Property CompleteLinkagePoints As List(Of Point)
            Get
                Return mOriginalCoordinates
            End Get
        End Property
    End Class
End Namespace