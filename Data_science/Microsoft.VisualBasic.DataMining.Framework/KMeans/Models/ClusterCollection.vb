Imports Microsoft.VisualBasic.DataMining.KMeans.CompleteLinkage
Imports Microsoft.VisualBasic.DataMining.ComponentModel

Namespace KMeans

    ''' <summary>
    ''' A collection of Cluster objects or Clusters
    ''' </summary>
    <Serializable> Public Class ClusterCollection(Of T As EntityBase(Of Double))
        Implements IEnumerable(Of KMeansCluster(Of T))

        ReadOnly _innerList As New List(Of KMeansCluster(Of T))

        Public ReadOnly Property NumOfCluster As Integer
            Get
                Return _innerList.Count
            End Get
        End Property

        ''' <summary>
        ''' Adds a Cluster to the collection of Clusters
        ''' </summary>
        ''' <param name="cluster">A Cluster to be added to the collection of clusters</param>
        Public Overridable Sub Add(cluster As KMeansCluster(Of T))
            Call _innerList.Add(cluster)
        End Sub

        ''' <summary>
        ''' Returns the Cluster at this index
        ''' </summary>
        Default Public Overridable ReadOnly Property Item(Index As Integer) As KMeansCluster(Of T)
            Get
                Return _innerList(Index)
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return NumOfCluster & " data clusters..."
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function

        Public Iterator Function GetEnumerator() As IEnumerator(Of KMeansCluster(Of T)) Implements IEnumerable(Of KMeansCluster(Of T)).GetEnumerator
            For Each x As KMeansCluster(Of T) In _innerList
                Yield x
            Next
        End Function
    End Class
End Namespace