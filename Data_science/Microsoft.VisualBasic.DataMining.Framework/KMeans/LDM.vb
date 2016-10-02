#Region "Microsoft.VisualBasic::7a85f5c017313382551cc38023267a4e, ..\visualbasic_App\Data_science\Microsoft.VisualBasic.DataMining.Framework\KMeans\LDM.vb"

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

Imports Microsoft.VisualBasic.DataMining.KMeans.CompleteLinkage
Imports Microsoft.VisualBasic.DataMining.ComponentModel

Namespace KMeans

    ''' <summary>
    ''' A class containing a group of data with similar characteristics (cluster), KMeans Cluster
    ''' </summary>
    <Serializable> Public Class KMeansCluster(Of T As EntityBase(Of Double)) : Inherits CompleteLinkage.Cluster(Of T)
        Implements IEnumerable(Of T)

        ''' <summary>
        ''' The sum of all the data in the cluster
        ''' </summary>
        Public ReadOnly Property ClusterSum() As Double()

        Dim _clusterMean As Double()

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
                For count As Integer = 0 To _clusterMean.Length - 1
                    _clusterMean(count) = (_ClusterSum(count) / _innerList.Count)
                Next

                Return _clusterMean
            End Get
        End Property

        Public Property Center As T

        Public Function CalculateKMeansCost() As Double
            Dim kMeansCost As Double = 0
            Dim distanceBetweenPoints As Double = 0
            For pointIndex As Integer = 0 To _innerList.Count - 1
                distanceBetweenPoints = _innerList(pointIndex).DistanceBetweenPoints(Center)
                kMeansCost += Math.Pow(distanceBetweenPoints, 2)
            Next pointIndex
            Return kMeansCost
        End Function

        Public Function CalculateCenter() As T
            ' If cluster is empty, the center will remain unchanged
            If _innerList.Count = 0 Then
                Return Center
            End If

            Dim dimension As Integer = _innerList(Scan0).Length
            Dim newCenterCoordinate As Double() = New Double(dimension - 1) {}
            For i As Integer = 0 To dimension - 1
                For pointIndex As Integer = 0 To _innerList.Count - 1
                    newCenterCoordinate(i) += _innerList(pointIndex).Properties(i)
                Next pointIndex
                newCenterCoordinate(i) /= _innerList.Count
            Next i
            Dim ___center As T = Activator.CreateInstance(Of T)
            ___center.Properties = newCenterCoordinate
            Return ___center
        End Function

        ''' <summary>
        ''' Adds a single dimension array data to the cluster.
        ''' (请注意，每当使用这个方法新添加一个对象的时候，都会导致均值被重新计算)
        ''' </summary>
        ''' <param name="data">A 1-dimensional array containing data that will be added to the cluster</param>
        Public Overrides Sub Add(data As T)
            Call _innerList.Add(data)

            If _innerList.Count = 1 Then
                _ClusterSum = New Double(data.Length - 1) {}
                _clusterMean = New Double(data.Length - 1) {}
            End If

            For count As Integer = 0 To data.Length - 1
                _ClusterSum(count) = _ClusterSum(count) + data.Properties(count)
            Next
        End Sub

        ''' <summary>
        ''' Returns the one dimensional array data located at the index
        ''' </summary>
        Default Public Overridable ReadOnly Property Item(Index As Integer) As T
            Get
                Return _innerList(Index)
            End Get
        End Property

        ''' <summary>
        ''' Will keep the center member variable, but clear the list of points
        ''' within the cluster.
        ''' </summary>
        Public Sub refresh()
            If _innerList.Count > 0 Then
                Call _innerList.Clear()
            End If
        End Sub

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
