#Region "Microsoft.VisualBasic::NumericClusterCollection, Data_science\DataMining\DataMining\Clustering\KMeans\Models\NumericClusterCollection.vb"

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
    ' along with this program.  If not, see <http://www.gnu.org/licenses/>.

#End Region

Imports System.Runtime.CompilerServices

Namespace KMeans

    ''' <summary>
    ''' <see cref="NumericKMeansCluster"/> 的集合，表示一次 KMeans 运算结果之中的全部簇。
    ''' </summary>
    Public Class NumericClusterCollection
        Implements IEnumerable(Of NumericKMeansCluster)

        Friend ReadOnly m_innerList As New List(Of NumericKMeansCluster)

        ''' <summary>
        ''' 簇的数量（k-centers）
        ''' </summary>
        Public ReadOnly Property NumOfCluster As Integer
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return m_innerList.Count
            End Get
        End Property

        Default Public Overridable ReadOnly Property Item(index As Integer) As NumericKMeansCluster
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return m_innerList(index)
            End Get
        End Property

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overridable Sub Add(cluster As NumericKMeansCluster)
            Call m_innerList.Add(cluster)
        End Sub

        Public Overrides Function ToString() As String
            Return NumOfCluster & " data clusters..."
        End Function

        Private Iterator Function EnumerateAll() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function

        Public Iterator Function GetEnumerator() As IEnumerator(Of NumericKMeansCluster) Implements IEnumerable(Of NumericKMeansCluster).GetEnumerator
            For Each cluster As NumericKMeansCluster In m_innerList
                Yield cluster
            Next
        End Function
    End Class
End Namespace
