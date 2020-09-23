Imports stdNum = System.Math

Namespace KMeans.CompleteLinkage

    Public Class LloydsMethodClustering : Inherits Clustering

        Friend _lloydsPoints As List(Of Point)
        Friend mKMeansClusters As List(Of KMeansCluster(Of Point))
        Friend mMinKMeansClusters As List(Of KMeansCluster(Of Point))

        Public Overrides ReadOnly Property Points As List(Of Point)
            Get
                Return _lloydsPoints
            End Get
        End Property

        Public Sub New(source As IEnumerable(Of Point), numClusters As Integer)
            Call MyBase.New(source, numClusters)

            mKMeansClusters = New List(Of KMeansCluster(Of Point))
            _lloydsPoints = New List(Of Point)(_source)
        End Sub

        Public Overrides Function Clustering() As List(Of Point)
            Dim minKMeansCost As Double = Double.MaxValue

            For runningTimes As Integer = 0 To 99
                ' for grabbing random points from input file
                Dim random As New Random

                If mKMeansClusters.Count > 0 Then mKMeansClusters.Clear()

                ' First, create number of desired clusters
                ' Pick K (mNumDesiredRandomPoints)
                For i As Integer = 0 To mNumDesiredClusters - 1
                    mKMeansClusters.Add(New KMeansCluster(Of Point))
                    Dim randomIndex As Integer = random.Next(_source.Count)
                    mKMeansClusters(i).Center = _source(randomIndex)
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
                    For i As Integer = 0 To _lloydsPoints.Count - 1
                        Dim minDistanceToCluster As Double = Double.MaxValue
                        For j As Integer = 0 To mKMeansClusters.Count - 1

                            Dim distanceToCluster As Double = _lloydsPoints(i).distanceToOtherPoint(mKMeansClusters(j).Center)

                            If distanceToCluster < minDistanceToCluster Then
                                closestClusterToPoint = mKMeansClusters(j)
                                minDistanceToCluster = distanceToCluster
                                minClusterIndex = j
                            End If
                        Next

                        ' Indexing by one with naming, so add one
                        _lloydsPoints(i).SetKMeansCluster(minClusterIndex + 1)

                        mKMeansClusters(minClusterIndex).Add(_lloydsPoints(i))
                    Next

                    ' reset kmeans cost
                    currentKMeansCost = 0

                    ' calculate the new center
                    For i As Integer = 0 To mKMeansClusters.Count - 1
                        mKMeansClusters(i).Center = mKMeansClusters(i).CalculateCenter()
                        currentKMeansCost += mKMeansClusters(i).CalculateKMeansCost()
                    Next i
                Loop While stdNum.Abs(oldKmeansCost - currentKMeansCost) > 1

                If currentKMeansCost < minKMeansCost Then
                    minKMeansCost = currentKMeansCost
                    mMinKMeansClusters = New List(Of KMeansCluster(Of Point))(mKMeansClusters)
                End If
            Next

            Return Points
        End Function
    End Class

End Namespace