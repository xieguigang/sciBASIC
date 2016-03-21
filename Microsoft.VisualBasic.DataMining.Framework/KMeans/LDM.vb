Imports Microsoft.VisualBasic.DataMining.Framework.ComponentModel

Namespace KMeans

    ''' <summary>
    ''' A class containing a group of data with similar characteristics (cluster)
    ''' </summary>
    <Serializable> Public Class Cluster(Of T As EntityBase(Of Double))
        Implements IEnumerable(Of T)

        ''' <summary>
        ''' The sum of all the data in the cluster
        ''' </summary>
        Public ReadOnly Property ClusterSum() As Double()

        Dim _clusterMean As Double()
        ReadOnly _innerList As New List(Of T)

        Public ReadOnly Property NumOfEntity As Integer
            Get
                Return _innerList.Count
            End Get
        End Property

        ''' <summary>
        ''' The mean of all the data in the cluster
        ''' </summary>
        Public ReadOnly Property ClusterMean() As Double()
            Get
                For count As Integer = 0 To Me(0).Length - 1
                    Me._clusterMean(count) = (Me._ClusterSum(count) / Me._innerList.Count)
                Next

                Return Me._clusterMean
            End Get
        End Property

        ''' <summary>
        ''' Adds a single dimension array data to the cluster
        ''' </summary>
        ''' <param name="data">A 1-dimensional array containing data that will be added to the cluster</param>
        Public Overridable Sub Add(data As T)
            Me._innerList.Add(data)

            If Me._innerList.Count = 1 Then
                Me._ClusterSum = New Double(data.Length - 1) {}
                Me._clusterMean = New Double(data.Length - 1) {}
            End If

            For count As Integer = 0 To data.Length - 1
                Me._ClusterSum(count) = Me._ClusterSum(count) + data.Properties(count)
            Next
        End Sub

        ''' <summary>
        ''' Returns the one dimensional array data located at the index
        ''' </summary>
        Default Public Overridable ReadOnly Property Item(Index As Integer) As T
            Get
                Return Me._innerList(Index)
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return NumOfEntity & " data entities..."
        End Function

        Public Iterator Function GetEnumerator() As IEnumerator(Of T) Implements IEnumerable(Of T).GetEnumerator
            For Each x As T In _innerList
                Yield x
            Next
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function
    End Class

    ''' <summary>
    ''' A collection of Cluster objects or Clusters
    ''' </summary>
    <Serializable> Public Class ClusterCollection(Of T As EntityBase(Of Double))
        Implements IEnumerable(Of Cluster(Of T))

        ReadOnly _innerList As New List(Of Cluster(Of T))

        Public ReadOnly Property NumOfCluster As Integer
            Get
                Return _innerList.Count
            End Get
        End Property

        ''' <summary>
        ''' Adds a Cluster to the collection of Clusters
        ''' </summary>
        ''' <param name="cluster">A Cluster to be added to the collection of clusters</param>
        Public Overridable Sub Add(cluster As Cluster(Of T))
            Me._innerList.Add(cluster)
        End Sub

        ''' <summary>
        ''' Returns the Cluster at this index
        ''' </summary>
        Default Public Overridable ReadOnly Property Item(Index As Integer) As Cluster(Of T)
            Get
                Return Me._innerList(Index)
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return NumOfCluster & " data clusters..."
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function

        Public Iterator Function GetEnumerator() As IEnumerator(Of Cluster(Of T)) Implements IEnumerable(Of Cluster(Of T)).GetEnumerator
            For Each x As Cluster(Of T) In _innerList
                Yield x
            Next
        End Function
    End Class
End Namespace