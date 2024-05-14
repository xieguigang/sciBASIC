#Region "Microsoft.VisualBasic::82e53da2e78cf1acbe319eac5b9d3c61, Data_science\DataMining\DataMining\Clustering\KMeans\Models\ClusterCollection.vb"

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

    '   Total Lines: 59
    '    Code Lines: 35
    ' Comment Lines: 15
    '   Blank Lines: 9
    '     File Size: 2.17 KB


    '     Class ClusterCollection
    ' 
    '         Properties: NumOfCluster
    ' 
    '         Function: GetEnumerator, IEnumerable_GetEnumerator, ToString
    ' 
    '         Sub: Add
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.DataMining.ComponentModel

Namespace KMeans

    ''' <summary>
    ''' A collection of Cluster objects or Clusters
    ''' </summary>
    Public Class ClusterCollection(Of T As EntityBase(Of Double))
        Implements IEnumerable(Of KMeansCluster(Of T))

        Friend ReadOnly m_innerList As New List(Of KMeansCluster(Of T))

        ''' <summary>
        ''' the numbers of the kmeans clusters
        ''' </summary>
        ''' <returns>k-centers</returns>
        Public ReadOnly Property NumOfCluster As Integer
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return m_innerList.Count
            End Get
        End Property

        ''' <summary>
        ''' Returns the Cluster at this index
        ''' </summary>
        Default Public Overridable ReadOnly Property Item(Index As Integer) As KMeansCluster(Of T)
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return m_innerList(Index)
            End Get
        End Property

        ''' <summary>
        ''' Adds a Cluster to the collection of Clusters
        ''' </summary>
        ''' <param name="cluster">A Cluster to be added to the collection of clusters</param>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overridable Sub Add(cluster As KMeansCluster(Of T))
            Call m_innerList.Add(cluster)
        End Sub

        Public Overrides Function ToString() As String
            Return NumOfCluster & " data clusters..."
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function

        Public Iterator Function GetEnumerator() As IEnumerator(Of KMeansCluster(Of T)) Implements IEnumerable(Of KMeansCluster(Of T)).GetEnumerator
            For Each cluster As KMeansCluster(Of T) In m_innerList
                Yield cluster
            Next
        End Function
    End Class
End Namespace
