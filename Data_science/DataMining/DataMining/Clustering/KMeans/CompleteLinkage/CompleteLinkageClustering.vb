Namespace KMeans.CompleteLinkage
    Public Class CompleteLinkageClustering : Inherits Clustering

        Friend _completeLinkageClusters As List(Of Cluster(Of Point))

        Public Sub New(source As IEnumerable(Of Point), numClusters As Integer)
            Call MyBase.New(source, numClusters)
            _completeLinkageClusters = New List(Of Cluster(Of Point))
        End Sub

        Public Overrides Function Clustering() As List(Of Point)
            ' Start by placing each point in its own cluster
            For Each p As Point In _source
                _completeLinkageClusters.Add(New Cluster(Of Point)(p))
            Next p

            Dim minDistance As Double = Double.MaxValue
            Dim currentDistance As Double

            ' Array to hold pair of closest clusters
            Dim twoClosestClusters As Cluster(Of Point)() = New Cluster(Of Point)(1) {}

            ' while there are more than k clusters
            Do While _completeLinkageClusters.Count > mNumDesiredClusters

                ' Calculate and store the distance between each pair of clusters, finding the minimum distance between clusters
                For i As Integer = 0 To _completeLinkageClusters.Count - 1
                    For j As Integer = i + 1 To _completeLinkageClusters.Count - 1
                        Dim c1 As Cluster(Of Point) = _completeLinkageClusters(i)
                        Dim c2 As Cluster(Of Point) = _completeLinkageClusters(j)
                        currentDistance = ClusterAPI.completeLinkageDistance(c1, c2)
                        If currentDistance < minDistance Then
                            twoClosestClusters(0) = c1
                            twoClosestClusters(1) = c2
                            minDistance = currentDistance
                        End If
                    Next
                Next

                Dim mergedCluster As Cluster(Of Point) = ClusterAPI.mergeClusters(twoClosestClusters(0), twoClosestClusters(1))
                _completeLinkageClusters.Add(mergedCluster)
                _completeLinkageClusters.Remove(twoClosestClusters(0))
                _completeLinkageClusters.Remove(twoClosestClusters(1))

                ' reset min distance so Array is overwritten
                minDistance = Double.MaxValue

                ' just to be safe, nullify array as well
                twoClosestClusters(0) = Nothing
                twoClosestClusters(1) = Nothing
            Loop

            Call __writeCluster(_completeLinkageClusters)

            Return Points
        End Function
    End Class
End Namespace