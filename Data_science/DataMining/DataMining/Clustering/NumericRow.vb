#Region "Microsoft.VisualBasic::NumericRow, Data_science\DataMining\DataMining\Clustering\NumericRow.vb"

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

Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Math

Namespace Clustering

    ''' <summary>
    ''' 数值行对象：包装统一二维表<see cref="Data.NumericTable"/>之中的一行数值数据，
    ''' 同时携带样本 ID 与该行在原始表之中的下标。
    ''' 
    ''' 该对象用于在不修改既有泛型聚类算法内部实现的前提之下，让这些算法直接消费
    ''' 二维表的数值行。
    ''' </summary>
    Public Class NumericRow
        Implements IReadOnlyId
        Implements INamedValue
        Implements IVector

        ''' <summary>
        ''' 该行在原始二维表之中的行下标
        ''' </summary>
        ''' <returns></returns>
        Public Property index As Integer

        ''' <summary>
        ''' 样本 ID/行名
        ''' </summary>
        ''' <returns></returns>
        Public Property Identity As String Implements IReadOnlyId.Identity, INamedValue.Key

        ''' <summary>
        ''' 样本的特征向量
        ''' </summary>
        ''' <returns></returns>
        Public Property data As Double() Implements IVector.Data

        Sub New()
        End Sub

        Sub New(id As String, values As Double(), Optional index As Integer = -1)
            Me.Identity = id
            Me.data = values
            Me.index = index
        End Sub

        Public Overrides Function ToString() As String
            Return $"[{index}] {Identity}"
        End Function
    End Class
End Namespace
