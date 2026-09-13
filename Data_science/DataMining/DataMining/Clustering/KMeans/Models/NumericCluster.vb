#Region "Microsoft.VisualBasic::NumericCluster, Data_science\DataMining\DataMining\Clustering\KMeans\Models\NumericCluster.vb"

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
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Correlations
Imports std = System.Math

Namespace KMeans

    ''' <summary>
    ''' 统一二维表聚类所使用的数值簇对象。
    ''' 
    ''' 和旧的泛型版本<see cref="Cluster(Of T)"/>不一样的地方在于：这个簇对象
    ''' 直接存放数值行（<see cref="Double"/>数组）以及该行在原始表中的下标与行名，
    ''' 因此不再依赖<see cref="ComponentModel.EntityBase(Of Double)"/>实体对象。
    ''' </summary>
    Public Class NumericCluster

        Protected Friend ReadOnly m_innerList As New List(Of Double())
        Protected Friend ReadOnly m_indices As New List(Of Integer)
        Protected Friend ReadOnly m_rowIds As New List(Of String)

        ''' <summary>
        ''' 当前簇内的样本数量
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property size As Integer
            Get
                Return m_innerList.Count
            End Get
        End Property

        Default Public Overridable ReadOnly Property Item(i As Integer) As Double()
            Get
                Return m_innerList(i)
            End Get
        End Property

        ''' <summary>
        ''' 获取簇内第<i>i</i>个样本在原始表之中的行下标
        ''' </summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function ClusterIndex(i As Integer) As Integer
            Return m_indices(i)
        End Function

        ''' <summary>
        ''' 获取簇内第<i>i</i>个样本的行名/样本 ID
        ''' </summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function RowId(i As Integer) As String
            Return m_rowIds(i)
        End Function

        Sub New()
        End Sub

        Sub New(row As Double(), Optional index As Integer = -1, Optional id As String = Nothing)
            Call Add(row, index, id)
        End Sub

        ''' <summary>
        ''' 向当前簇添加一个数值行样本
        ''' </summary>
        ''' <param name="row">样本的特征向量</param>
        ''' <param name="index">样本在原始表之中的行下标</param>
        ''' <param name="id">样本的 ID/行名</param>
        Public Overridable Sub Add(row As Double(), Optional index As Integer = -1, Optional id As String = Nothing)
            Call m_innerList.Add(row)
            Call m_indices.Add(index)
            Call m_rowIds.Add(id)
        End Sub

        ''' <summary>
        ''' 计算两个簇之间的完全连接距离
        ''' </summary>
        Public Function CompleteLinkageDistance(c2 As NumericCluster) As Double
            Dim maxDistance As Double = Double.MinValue
            Dim dist As Double

            For i1 As Integer = 0 To m_innerList.Count - 1
                For i2 As Integer = 0 To c2.m_innerList.Count - 1
                    dist = m_innerList(i1).EuclideanDistance(c2.m_innerList(i2))
                    maxDistance = std.Max(dist, maxDistance)
                Next
            Next

            Return maxDistance
        End Function

        ''' <summary>
        ''' 合并两个簇为一个新的簇
        ''' </summary>
        Public Shared Operator +(c1 As NumericCluster, c2 As NumericCluster) As NumericCluster
            Dim merged As New NumericCluster

            For i As Integer = 0 To c1.m_innerList.Count - 1
                Call merged.Add(c1.m_innerList(i), c1.m_indices(i), c1.m_rowIds(i))
            Next

            For i As Integer = 0 To c2.m_innerList.Count - 1
                Call merged.Add(c2.m_innerList(i), c2.m_indices(i), c2.m_rowIds(i))
            Next

            Return merged
        End Operator

        Public Overrides Function ToString() As String
            Return $"{size} data entities..."
        End Function
    End Class
End Namespace
