#Region "Microsoft.VisualBasic::7cfb8056df36e1e8949c94633ba96202, Data_science\MachineLearning\GNN\Graph.vb"

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

    '   Total Lines: 326
    '    Code Lines: 168 (51.53%)
    ' Comment Lines: 113 (34.66%)
    '    - Xml Docs: 95.58%
    ' 
    '   Blank Lines: 45 (13.80%)
    '     File Size: 9.97 KB


    ' Class Edge
    ' 
    '     Properties: Source, Target, Weight
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    ' Class Graph
    ' 
    '     Properties: EdgeFeatures, Edges, NodeFeatures, NumEdges, NumNodes
    ' 
    '     Constructor: (+2 Overloads) Sub New
    ' 
    '     Function: GetAdjacencyMatrix, GetDegreeMatrix, GetInNeighbors, GetLaplacianMatrix, GetNeighbors
    '               GetNormalizedAdjacencyMatrix, GetNormalizedLaplacian
    ' 
    '     Sub: AddEdge, AddUndirectedEdge, PrintInfo
    ' 
    ' Class GraphDataset
    ' 
    '     Properties: Count, Graphs, Labels, NumClasses
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Sub: Add
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports std = System.Math

''' <summary>
''' 图的边结构
''' 表示图中两个节点之间的连接关系
''' </summary>
Public Class Edge
    ''' <summary>
    ''' 源节点索引
    ''' </summary>
    Public Property Source As Integer

    ''' <summary>
    ''' 目标节点索引
    ''' </summary>
    Public Property Target As Integer

    ''' <summary>
    ''' 边的权重（可选，默认为1）
    ''' </summary>
    Public Property Weight As Single = 1.0F

    Public Sub New(source As Integer, target As Integer, Optional weight As Single = 1.0F)
        Me.Source = source
        Me.Target = target
        Me.Weight = weight
    End Sub
End Class

''' <summary>
''' 图数据结构
''' 支持有向图和无向图，支持节点特征和边特征
''' 这是GNN处理的核心数据结构
''' </summary>
Public Class Graph
    ''' <summary>
    ''' 图中的节点数量
    ''' </summary>
    Dim _NumNodes As Integer
    ''' <summary>
    ''' 边列表
    ''' </summary>
    Dim _Edges As System.Collections.Generic.List(Of Edge)

    Public Property NumNodes As Integer
        Get
            Return _NumNodes
        End Get
        Private Set(value As Integer)
            _NumNodes = value
        End Set
    End Property

    ''' <summary>
    ''' 图中的边数量
    ''' </summary>
    Public ReadOnly Property NumEdges As Integer
        Get
            Return Edges.Count
        End Get
    End Property

    ''' <summary>
    ''' 节点特征矩阵 [NumNodes, FeatureDim]
    ''' 每一行代表一个节点的特征向量
    ''' </summary>
    Public Property NodeFeatures As Tensor

    Public Property Edges As List(Of Edge)
        Get
            Return _Edges
        End Get
        Private Set(value As List(Of Edge))
            _Edges = value
        End Set
    End Property

    ''' <summary>
    ''' 边特征矩阵 [NumEdges, EdgeFeatureDim]（可选）
    ''' </summary>
    Public Property EdgeFeatures As Tensor

    ''' <summary>
    ''' 邻接表：存储每个节点的邻居节点
    ''' adjList[i] 包含所有与节点i相邻的节点索引
    ''' </summary>
    Private _adjacencyList As List(Of List(Of Integer))

    ''' <summary>
    ''' 入度邻接表：存储指向每个节点的邻居
    ''' 用于消息传递时收集来自邻居的信息
    ''' </summary>
    Private _inAdjacencyList As List(Of List(Of Integer))

    ''' <summary>
    ''' 创建一个空图
    ''' </summary>
    ''' <param name="numNodes">节点数量</param>
    ''' <param name="featureDim">节点特征维度</param>
    Public Sub New(numNodes As Integer, featureDim As Integer)
        Me.NumNodes = numNodes
        NodeFeatures = New Tensor(numNodes, featureDim)
        Edges = New List(Of Edge)()
        _adjacencyList = New List(Of List(Of Integer))()
        _inAdjacencyList = New List(Of List(Of Integer))()

        For i = 0 To numNodes - 1
            _adjacencyList.Add(New List(Of Integer)())
            _inAdjacencyList.Add(New List(Of Integer)())
        Next
    End Sub

    ''' <summary>
    ''' 使用给定的节点特征创建图
    ''' </summary>
    ''' <param name="nodeFeatures">节点特征矩阵 [NumNodes, FeatureDim]</param>
    Public Sub New(nodeFeatures As Tensor)
        If nodeFeatures.Rank <> 2 Then Throw New ArgumentException("节点特征必须是二维张量")

        NumNodes = nodeFeatures.Shape(0)
        Me.NodeFeatures = nodeFeatures
        Edges = New List(Of Edge)()
        _adjacencyList = New List(Of List(Of Integer))()
        _inAdjacencyList = New List(Of List(Of Integer))()

        For i = 0 To NumNodes - 1
            _adjacencyList.Add(New List(Of Integer)())
            _inAdjacencyList.Add(New List(Of Integer)())
        Next
    End Sub

    ''' <summary>
    ''' 添加一条边
    ''' </summary>
    ''' <param name="source">源节点</param>
    ''' <param name="target">目标节点</param>
    ''' <param name="weight">边权重</param>
    Public Sub AddEdge(source As Integer, target As Integer, Optional weight As Single = 1.0F)
        If source < 0 OrElse source >= NumNodes OrElse target < 0 OrElse target >= NumNodes Then Throw New ArgumentException("节点索引超出范围")

        Edges.Add(New Edge(source, target, weight))
        _adjacencyList(source).Add(target)
        _inAdjacencyList(target).Add(source)
    End Sub

    ''' <summary>
    ''' 添加双向边（无向图）
    ''' </summary>
    Public Sub AddUndirectedEdge(node1 As Integer, node2 As Integer, Optional weight As Single = 1.0F)
        AddEdge(node1, node2, weight)
        AddEdge(node2, node1, weight)
    End Sub

    ''' <summary>
    ''' 获取节点的邻居列表（出边方向）
    ''' </summary>
    Public Function GetNeighbors(nodeIndex As Integer) As List(Of Integer)
        Return _adjacencyList(nodeIndex)
    End Function

    ''' <summary>
    ''' 获取指向该节点的邻居列表（入边方向）
    ''' 用于消息传递时聚合邻居信息
    ''' </summary>
    Public Function GetInNeighbors(nodeIndex As Integer) As List(Of Integer)
        Return _inAdjacencyList(nodeIndex)
    End Function

    ''' <summary>
    ''' 构建邻接矩阵
    ''' 邻接矩阵A[i,j] = 1 表示存在从节点i到节点j的边
    ''' </summary>
    Public Function GetAdjacencyMatrix() As Tensor
        Dim adjMatrix = New Tensor(NumNodes, NumNodes)

        For Each edge In Edges
            adjMatrix(edge.Source, edge.Target) = edge.Weight
        Next

        Return adjMatrix
    End Function

    ''' <summary>
    ''' 构建归一化的邻接矩阵（用于GCN）
    ''' A_norm = D^(-1/2) * A * D^(-1/2)
    ''' 其中D是度矩阵
    ''' 这种归一化有助于稳定训练过程
    ''' </summary>
    Public Function GetNormalizedAdjacencyMatrix() As Tensor
        Dim adjMatrix = GetAdjacencyMatrix()

        ' 添加自环（每个节点连接到自己）
        ' 这在GCN中很重要，确保节点能保留自己的信息
        For i = 0 To NumNodes - 1
            adjMatrix(i, i) += 1.0F
        Next

        ' 计算度向量
        Dim degree = New Single(NumNodes - 1) {}
        For i = 0 To NumNodes - 1
            For j = 0 To NumNodes - 1
                degree(i) += adjMatrix(i, j)
            Next
        Next

        ' 计算D^(-1/2)
        Dim dInvSqrt = New Single(NumNodes - 1) {}
        For i = 0 To NumNodes - 1
            dInvSqrt(i) = If(degree(i) > 0, CSng(1.0 / std.Sqrt(degree(i))), 0)
        Next

        ' 计算 D^(-1/2) * A * D^(-1/2)
        Dim normalizedAdj = New Tensor(NumNodes, NumNodes)
        For i = 0 To NumNodes - 1
            For j = 0 To NumNodes - 1
                normalizedAdj(i, j) = adjMatrix(i, j) * dInvSqrt(i) * dInvSqrt(j)
            Next
        Next

        Return normalizedAdj
    End Function

    ''' <summary>
    ''' 获取度矩阵（对角矩阵）
    ''' </summary>
    Public Function GetDegreeMatrix() As Tensor
        Dim degreeMatrix = New Tensor(NumNodes, NumNodes)

        For Each edge In Edges
            degreeMatrix(edge.Source, edge.Source) += edge.Weight
        Next

        Return degreeMatrix
    End Function

    ''' <summary>
    ''' 获取拉普拉斯矩阵 L = D - A
    ''' </summary>
    Public Function GetLaplacianMatrix() As Tensor
        Return GetDegreeMatrix() - GetAdjacencyMatrix()
    End Function

    ''' <summary>
    ''' 获取归一化拉普拉斯矩阵 L_norm = I - D^(-1/2) * A * D^(-1/2)
    ''' </summary>
    Public Function GetNormalizedLaplacian() As Tensor
        Dim identity = Tensor.Identity(NumNodes)
        Return identity - GetNormalizedAdjacencyMatrix()
    End Function

    ''' <summary>
    ''' 打印图的基本信息
    ''' </summary>
    Public Sub PrintInfo()
        Console.WriteLine($"图信息:")
        Console.WriteLine($"  节点数: {NumNodes}")
        Console.WriteLine($"  边数: {NumEdges}")
        Console.WriteLine($"  特征维度: {NodeFeatures.Shape(1)}")
        Console.WriteLine($"  平均度: {NumEdges / NumNodes:F2}")
    End Sub
End Class

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
    Private _Graphs As System.Collections.Generic.List(Of Graph), _Labels As System.Collections.Generic.List(Of Integer)

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
