Imports System.Data

Namespace KMeans

    ''' <summary>
    ''' This class implement a KMeans clustering algorithm
    ''' </summary>
    Public Module KMeansAlgorithm

        ''' <summary>
        ''' Calculates the Euclidean Distance Measure between two data points
        ''' </summary>
        ''' <param name="X">An array with the values of an object or datapoint</param>
        ''' <param name="Y">An array with the values of an object or datapoint</param>
        ''' <returns>Returns the Euclidean Distance Measure Between Points X and Points Y</returns>
        Public Function EuclideanDistance(X As Double(), Y As Double()) As Double
            Dim count As Integer = 0

            Dim distance As Double = 0.0

            Dim sum As Double = 0.0


            If X.GetUpperBound(0) <> Y.GetUpperBound(0) Then
                Throw New System.ArgumentException("the number of elements in X must match the number of elements in Y")
            Else
                count = X.Length
            End If

            For i As Integer = 0 To count - 1
                sum = sum + Math.Pow(Math.Abs(X(i) - Y(i)), 2)
            Next

            distance = Math.Sqrt(sum)

            Return distance
        End Function

        ''' <summary>
        ''' Calculates the Manhattan Distance Measure between two data points
        ''' </summary>
        ''' <param name="X">An array with the values of an object or datapoint</param>
        ''' <param name="Y">An array with the values of an object or datapoint</param>
        ''' <returns>Returns the Manhattan Distance Measure Between Points X and Points Y</returns>
        Public Function ManhattanDistance(X As Double(), Y As Double()) As Double
            Dim count As Integer = 0

            Dim distance As Double = 0.0

            Dim sum As Double = 0.0


            If X.GetUpperBound(0) <> Y.GetUpperBound(0) Then
                Throw New System.ArgumentException("the number of elements in X must match the number of elements in Y")
            Else
                count = X.Length
            End If

            For i As Integer = 0 To count - 1
                sum = sum + Math.Abs(X(i) - Y(i))
            Next

            distance = sum

            Return distance
        End Function

        ''' <summary>
        ''' Calculates The Mean Of A Cluster OR The Cluster Center
        ''' </summary>
        ''' <param name="cluster">
        ''' A two-dimensional array containing a dataset of numeric values
        ''' </param>
        ''' <returns>
        ''' Returns an Array Defining A Data Point Representing The Cluster Mean or Centroid
        ''' </returns>
        Public Function ClusterMean(cluster As Double(,)) As Double()
            Dim rowCount As Integer = 0

            Dim fieldCount As Integer = 0

            Dim dataSum As Double(,)

            Dim centroid As Double()


            rowCount = cluster.GetUpperBound(0) + 1

            fieldCount = cluster.GetUpperBound(1) + 1

            dataSum = New Double(0, fieldCount - 1) {}

            centroid = New Double(fieldCount - 1) {}

            '((20+30)/2), ((170+160)/2), ((80+120)/2)
            For j As Integer = 0 To fieldCount - 1
                For i As Integer = 0 To rowCount - 1
                    dataSum(0, j) = dataSum(0, j) + cluster(i, j)
                Next

                centroid(j) = (dataSum(0, j) / rowCount)
            Next

            Return centroid
        End Function


        ''' <summary>
        ''' Seperates a dataset into clusters or groups with similar characteristics
        ''' </summary>
        ''' <param name="clusterCount">The number of clusters or groups to form</param>
        ''' <param name="data">An array containing data that will be clustered</param>
        ''' <returns>A collection of clusters of data</returns>
        Public Function ClusterDataSet(clusterCount As Integer, data As Double(,)) As ClusterCollection
            'bool stableClusterFormation = false;

            Dim clusterNumber As Integer = 0

            Dim rowCount As Integer = data.GetUpperBound(0) + 1

            Dim fieldCount As Integer = data.GetUpperBound(1) + 1

            Dim stableClustersCount As Integer = 0

            Dim iterationCount As Integer = 0

            Dim dataPoint As Double()

            Dim cluster As Cluster = Nothing

            Dim clusters As New ClusterCollection()

            Dim clusterNumbers As New System.Collections.ArrayList(clusterCount)
            Dim Random As Random = New Random

            While clusterNumbers.Count < clusterCount
                clusterNumber = random.[Next](0, rowCount - 1)

                If Not clusterNumbers.Contains(clusterNumber) Then

                    cluster = New Cluster()

                    clusterNumbers.Add(clusterNumber)

                    dataPoint = New Double(fieldCount - 1) {}


                    For field As Integer = 0 To fieldCount - 1
                        dataPoint.SetValue((data(clusterNumber, field)), field)
                    Next

                    cluster.Add(dataPoint)

                    clusters.Add(cluster)
                End If
            End While


            While stableClustersCount <> clusters.Count
                stableClustersCount = 0

                Dim newClusters As ClusterCollection = KMeans.ClusterDataSet(clusters, data)

                For clusterIndex As Integer = 0 To clusters.Count - 1
                    If (KMeans.EuclideanDistance(newClusters(clusterIndex).ClusterMean, clusters(clusterIndex).ClusterMean)) = 0 Then
                        stableClustersCount += 1
                    End If
                Next

                iterationCount += 1

                clusters = newClusters
            End While

            Return clusters
        End Function

        ''' <summary>
        ''' Seperates a dataset into clusters or groups with similar characteristics
        ''' </summary>
        ''' <param name="clusters">A collection of data clusters</param>
        ''' <param name="data">An array containing data to b eclustered</param>
        ''' <returns>A collection of clusters of data</returns>
        Public Function ClusterDataSet(clusters As ClusterCollection, data As Double(,)) As ClusterCollection

            Dim dataPoint As Double()
            Dim clusterMean As Double()
            Dim firstClusterDistance As Double = 0.0
            Dim secondClusterDistance As Double = 0.0
            Dim rowCount As Integer = data.GetUpperBound(0) + 1
            Dim fieldCount As Integer = data.GetUpperBound(1) + 1
            Dim position As Integer = 0
            ' create a new collection of clusters
            Dim newClusters As New ClusterCollection()

            For count As Integer = 0 To clusters.Count - 1
                Dim newCluster As New Cluster()

                newClusters.Add(newCluster)
            Next

            If clusters.Count <= 0 Then
                Throw New SystemException("Cluster Count Cannot Be Zero!")
            End If

            '((20+30)/2), ((170+160)/2), ((80+120)/2)
            For row As Integer = 0 To rowCount - 1
                dataPoint = New Double(fieldCount - 1) {}

                For field As Integer = 0 To fieldCount - 1
                    dataPoint.SetValue((data(row, field)), field)
                Next

                For cluster As Integer = 0 To clusters.Count - 1
                    clusterMean = clusters(cluster).ClusterMean

                    If cluster = 0 Then
                        firstClusterDistance = KMeans.EuclideanDistance(dataPoint, clusterMean)

                        position = cluster
                    Else
                        secondClusterDistance = KMeans.EuclideanDistance(dataPoint, clusterMean)

                        If firstClusterDistance > secondClusterDistance Then
                            firstClusterDistance = secondClusterDistance

                            position = cluster
                        End If
                    End If
                Next

                newClusters(position).Add(dataPoint)
            Next

            Return newClusters
        End Function

        ''' <summary>
        ''' Converts a System.Data.DataTable to a 2-dimensional array
        ''' </summary>
        ''' <param name="table">A System.Data.DataTable containing data to cluster</param>
        ''' <returns>A 2-dimensional array containing data to cluster</returns>
        Public Function ConvertDataTableToArray(table As DataTable) As Double(,)

            Dim rowCount As Integer = table.Rows.Count
            Dim fieldCount As Integer = table.Columns.Count
            Dim dataPoints As Double(,)
            Dim fieldValue As Double = 0.0
            Dim row As DataRow

            dataPoints = New Double(rowCount - 1, fieldCount - 1) {}

            For rowPosition As Integer = 0 To rowCount - 1
                row = table.Rows(rowPosition)

                For fieldPosition As Integer = 0 To fieldCount - 1
                    Try
                        fieldValue = Double.Parse(row(fieldPosition).ToString())
                    Catch ex As System.Exception
                        System.Diagnostics.Debug.WriteLine(ex.ToString())

                        Throw New InvalidCastException("Invalid row at " & rowPosition.ToString() & " and field " & fieldPosition.ToString(), ex)
                    End Try

                    dataPoints(rowPosition, fieldPosition) = fieldValue
                Next
            Next

            Return dataPoints
        End Function
    End Module

    ''' <summary>
    ''' A class containing a group of data with similar characteristics (cluster)
    ''' </summary>
    <Serializable> _
    Public Class Cluster : Inherits System.Collections.CollectionBase

        Dim _clusterSum As Double()

        ''' <summary>
        ''' The sum of all the data in the cluster
        ''' </summary>
        Public ReadOnly Property ClusterSum() As Double()
            Get
                Return Me._clusterSum
            End Get
        End Property

        Dim _clusterMean As Double()

        ''' <summary>
        ''' The mean of all the data in the cluster
        ''' </summary>
        Public ReadOnly Property ClusterMean() As Double()
            Get
                For count As Integer = 0 To Me(0).Length - 1
                    Me._clusterMean(count) = (Me._clusterSum(count) / Me.List.Count)
                Next

                Return Me._clusterMean
            End Get
        End Property

        ''' <summary>
        ''' Adds a single dimension array data to the cluster
        ''' </summary>
        ''' <param name="data">A 1-dimensional array containing data that will be added to the cluster</param>
        Public Overridable Sub Add(data As Double())
            Me.List.Add(data)

            If Me.List.Count = 1 Then
                Me._clusterSum = New Double(data.Length - 1) {}

                Me._clusterMean = New Double(data.Length - 1) {}
            End If

            For count As Integer = 0 To data.Length - 1
                Me._clusterSum(count) = Me._clusterSum(count) + data(count)
            Next
        End Sub

        ''' <summary>
        ''' Returns the one dimensional array data located at the index
        ''' </summary>
        Default Public Overridable ReadOnly Property Item(Index As Integer) As Double()
            Get
                'return the Neuron at IList[Index] 
                Return DirectCast(Me.List(Index), Double())
            End Get
        End Property
    End Class

    ''' <summary>
    ''' A collection of Cluster objects or Clusters
    ''' </summary>
    <Serializable>
    Public Class ClusterCollection : Inherits System.Collections.CollectionBase

        ''' <summary>
        ''' Adds a Cluster to the collection of Clusters
        ''' </summary>
        ''' <param name="cluster">A Cluster to be added to the collection of clusters</param>
        Public Overridable Sub Add(cluster As Cluster)
            Me.List.Add(cluster)
        End Sub

        ''' <summary>
        ''' Returns the Cluster at this index
        ''' </summary>
        Default Public Overridable ReadOnly Property Item(Index As Integer) As Cluster
            Get
                'return the Neuron at IList[Index] 
                Return DirectCast(Me.List(Index), Cluster)
            End Get
        End Property
    End Class
End Namespace
