#Region "Microsoft.VisualBasic::NumericKMeansCluster, Data_science\DataMining\DataMining\Clustering\KMeans\Models\NumericKMeansCluster.vb"

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

Imports System.Collections
Imports System.Collections.Generic
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math.Correlations
Imports Microsoft.VisualBasic.Math.SIMD
Imports std = System.Math

Namespace KMeans

    ''' <summary>
    ''' 统一二维表聚类所使用的 KMeans 簇对象，直接基于数值行进行均值与代价计算。
    ''' </summary>
    <Serializable>
    Public Class NumericKMeansCluster : Inherits NumericCluster
        Implements IEnumerable(Of Double())

        Private _sum As Double()

        ''' <summary>
        ''' 簇内所有样本的数值向量之和
        ''' </summary>
        Public ReadOnly Property ClusterSum As Double()
            Get
                Return _sum
            End Get
        End Property

        Public ReadOnly Property NumOfEntity As Integer
            Get
                Return m_innerList.Count
            End Get
        End Property

        ''' <summary>
        ''' 簇内所有样本的均值中心点
        ''' </summary>
        Public ReadOnly Property ClusterMean As Double()
            Get
                Dim n As Integer = m_innerList.Count

                If n = 0 OrElse _sum Is Nothing Then
                    Return New Double() {}
                End If

                Return Divide.f64_op_divide_f64_scalar(_sum, n)
            End Get
        End Property

        ''' <summary>
        ''' 簇的中心点数值向量
        ''' </summary>
        Public Property Center As Double()

        Default Public Overrides ReadOnly Property Item(index As Integer) As Double()
            Get
                Return m_innerList(index)
            End Get
        End Property

        ''' <summary>
        ''' 计算当前簇的 KMeans 代价（簇内样本到中心的平方距离之和）
        ''' </summary>
        Public Function CalculateKMeansCost() As Double
            Dim cost As Double = 0
            Dim distance As Double

            For i As Integer = 0 To m_innerList.Count - 1
                distance = m_innerList(i).EuclideanDistance(Center)
                cost += std.Pow(distance, 2)
            Next

            Return cost
        End Function

        Public Overrides Sub Add(row As Double(), Optional index As Integer = -1, Optional id As String = Nothing)
            Call MyBase.Add(row, index, id)

            If m_innerList.Count = 1 Then
                _sum = row.ToArray
            ElseIf _sum IsNot Nothing Then
                Dim w As Integer = std.Min(_sum.Length, row.Length)

                For offset As Integer = 0 To w - 1
                    _sum(offset) = _sum(offset) + row(offset)
                Next
            End If
        End Sub

        ''' <summary>
        ''' 清空簇内样本列表（保留中心点信息）
        ''' </summary>
        Public Sub refresh()
            If m_innerList.Count > 0 Then
                Call m_innerList.Clear()
                Call m_indices.Clear()
                Call m_rowIds.Clear()
                _sum = Nothing
            End If
        End Sub

        Public Iterator Function GetEnumerator() As IEnumerator(Of Double()) Implements IEnumerable(Of Double()).GetEnumerator
            For Each row As Double() In m_innerList
                Yield row
            Next
        End Function

        Private Function EnumerateAll() As IEnumerator Implements System.Collections.IEnumerable.GetEnumerator
            Return GetEnumerator()
        End Function
    End Class
End Namespace
