#Region "Microsoft.VisualBasic::add7c5621c0a153add7c4259a0395529, Data_science\DataMining\DataMining\Clustering\KMeans\Models\KMeansCluster.vb"

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

    '   Total Lines: 103
    '    Code Lines: 67
    ' Comment Lines: 21
    '   Blank Lines: 15
    '     File Size: 3.81 KB


    '     Class KMeansCluster
    ' 
    '         Properties: Center, ClusterMean, ClusterSum, NumOfEntity
    ' 
    '         Function: CalculateKMeansCost, GetEnumerator, IEnumerable_GetEnumerator, ToString
    ' 
    '         Sub: Add, refresh
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.DataMining.ComponentModel
Imports Microsoft.VisualBasic.Math.Correlations
Imports Microsoft.VisualBasic.Math.SIMD
Imports std = System.Math

Namespace KMeans

    ''' <summary>
    ''' A class containing a group of data with similar characteristics (cluster), KMeans Cluster
    ''' </summary>
    <Serializable>
    Public Class KMeansCluster(Of T As EntityBase(Of Double)) : Inherits Cluster(Of T)
        Implements IEnumerable(Of T)

        ''' <summary>
        ''' The sum of all the data in the cluster
        ''' </summary>
        Public ReadOnly Property ClusterSum() As Double()

        Public ReadOnly Property NumOfEntity As Integer
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return m_innerList.Count
            End Get
        End Property

        ''' <summary>
        ''' The mean of all the data in the cluster
        ''' </summary>
        Public ReadOnly Property ClusterMean() As Double()
            Get
                Dim size As Integer = m_innerList.Count
                Dim mean As Double() = Divide.f64_op_divide_f64_scalar(_ClusterSum, size)

                Return mean
            End Get
        End Property

        Public Property Center As T

        ''' <summary>
        ''' Returns the one dimensional array data located at the index
        ''' </summary>
        Default Public Overridable ReadOnly Property Item(Index As Integer) As T
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return m_innerList(Index)
            End Get
        End Property

        Public Function CalculateKMeansCost() As Double
            Dim kMeansCost As Double = 0
            Dim distanceBetweenPoints As Double = 0
            For pointIndex As Integer = 0 To m_innerList.Count - 1
                distanceBetweenPoints = m_innerList(pointIndex).entityVector.EuclideanDistance(Center.entityVector)
                kMeansCost += std.Pow(distanceBetweenPoints, 2)
            Next
            Return kMeansCost
        End Function

        ''' <summary>
        ''' Adds a single dimension array data to the cluster.
        ''' (请注意，每当使用这个方法新添加一个对象的时候，都会导致均值被重新计算)
        ''' </summary>
        ''' <param name="data">A 1-dimensional array containing data that will be added to the cluster</param>
        Public Overrides Sub Add(data As T)
            Call m_innerList.Add(data)

            If m_innerList.Count = 1 Then
                _ClusterSum = data.entityVector.ToArray
            Else
                For offset As Integer = 0 To data.Length - 1
                    _ClusterSum(offset) = _ClusterSum(offset) + data(offset)
                Next
            End If
        End Sub

        ''' <summary>
        ''' Will keep the center member variable, but clear the list of points
        ''' within the cluster.
        ''' </summary>
        Public Sub refresh()
            If m_innerList.Count > 0 Then
                Call m_innerList.Clear()
            End If
        End Sub

        Public Overrides Function ToString() As String
            Return NumOfEntity & " data entities..."
        End Function

        Public Iterator Function GetEnumerator() As IEnumerator(Of T) Implements IEnumerable(Of T).GetEnumerator
            For Each x As T In m_innerList
                Yield x
            Next
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function
    End Class
End Namespace
