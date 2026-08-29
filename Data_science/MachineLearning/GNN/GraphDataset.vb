#Region "Microsoft.VisualBasic::dc25b94535c355aba1ad36e45d0d9f4a, Data_science\MachineLearning\GNN\GraphDataset.vb"

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

    '   Total Lines: 64
    '    Code Lines: 37 (57.81%)
    ' Comment Lines: 19 (29.69%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 8 (12.50%)
    '     File Size: 1.52 KB


    ' Class GraphDataset
    ' 
    '     Properties: Count, Graphs, Labels, NumClasses
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Sub: Add
    ' 
    ' /********************************************************************************/

#End Region


''' <summary>
''' 图数据集
''' 用于存储多个图样本，常用于图分类任务
''' </summary>
Public Class GraphDataset
    ''' <summary>
    ''' 图样本列表
    ''' </summary>

    ''' <summary>
    ''' 图标签（用于图分类任务）
    ''' </summary>
    Private _Graphs As List(Of Graph), _Labels As List(Of Integer)

    Public Property Graphs As List(Of Graph)
        Get
            Return _Graphs
        End Get
        Private Set(value As List(Of Graph))
            _Graphs = value
        End Set
    End Property

    Public Property Labels As List(Of Integer)
        Get
            Return _Labels
        End Get
        Private Set(value As List(Of Integer))
            _Labels = value
        End Set
    End Property

    Public Sub New()
        Graphs = New List(Of Graph)()
        Labels = New List(Of Integer)()
    End Sub

    ''' <summary>
    ''' 添加图样本
    ''' </summary>
    Public Sub Add(graph As Graph, label As Integer)
        Graphs.Add(graph)
        Labels.Add(label)
    End Sub

    ''' <summary>
    ''' 获取数据集大小
    ''' </summary>
    Public ReadOnly Property Count As Integer
        Get
            Return Graphs.Count
        End Get
    End Property

    ''' <summary>
    ''' 获取类别数量
    ''' </summary>
    Public ReadOnly Property NumClasses As Integer
        Get
            Return Labels.Distinct().Count()
        End Get
    End Property
End Class
