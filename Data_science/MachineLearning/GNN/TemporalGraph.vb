''' <summary>
''' 时序图数据结构
''' 支持动态图的时间序列表示，每个时间步对应一个图快照
''' 这是时序图神经网络处理的核心数据结构
''' </summary>
Public Class TemporalGraph
    ''' <summary>
    ''' 时间步数量
    ''' </summary>
    Public ReadOnly Property NumTimeSteps As Integer

    ''' <summary>
    ''' 每个时间步的图快照
    ''' </summary>
    Private ReadOnly _snapshots As List(Of Graph)

    ''' <summary>
    ''' 时间相关的边特征（可选）
    ''' 存储边随时间变化的特征
    ''' </summary>
    Private _temporalEdgeFeatures As List(Of Tensor)

    ''' <summary>
    ''' 全局时间特征（可选）
    ''' 用于编码时间本身的特征，如周期性、趋势等
    ''' </summary>
    Public Property TimeFeatures As Tensor

    ''' <summary>
    ''' 创建空的时序图
    ''' </summary>
    ''' <param name="numTimeSteps">时间步数量</param>
    Public Sub New(numTimeSteps As Integer)
        Me.NumTimeSteps = numTimeSteps
        _snapshots = New List(Of Graph)()
        _temporalEdgeFeatures = New List(Of Tensor)()

        For i = 0 To numTimeSteps - 1
            _snapshots.Add(Nothing)
            _temporalEdgeFeatures.Add(Nothing)
        Next
    End Sub

    ''' <summary>
    ''' 从图快照列表创建时序图
    ''' </summary>
    ''' <param name="snapshots">图快照列表</param>
    Public Sub New(snapshots As List(Of Graph))
        If snapshots Is Nothing OrElse snapshots.Count = 0 Then
            Throw New ArgumentException("快照列表不能为空")
        End If

        NumTimeSteps = snapshots.Count
        _snapshots = New List(Of Graph)(snapshots)
        _temporalEdgeFeatures = New List(Of Tensor)()

        For i = 0 To NumTimeSteps - 1
            _temporalEdgeFeatures.Add(Nothing)
        Next
    End Sub

    ''' <summary>
    ''' 获取或设置指定时间步的图快照
    ''' </summary>
    Default Public Property Item(timeStep As Integer) As Graph
        Get
            If timeStep < 0 OrElse timeStep >= NumTimeSteps Then
                Throw New ArgumentOutOfRangeException(NameOf(timeStep))
            End If
            Return _snapshots(timeStep)
        End Get
        Set(value As Graph)
            If timeStep < 0 OrElse timeStep >= NumTimeSteps Then
                Throw New ArgumentOutOfRangeException(NameOf(timeStep))
            End If
            _snapshots(timeStep) = value
        End Set
    End Property

    ''' <summary>
    ''' 获取所有图快照
    ''' </summary>
    Public Function GetSnapshots() As List(Of Graph)
        Return New List(Of Graph)(_snapshots)
    End Function

    ''' <summary>
    ''' 设置指定时间步的时序边特征
    ''' </summary>
    Public Sub SetTemporalEdgeFeatures(timeStep As Integer, features As Tensor)
        If timeStep < 0 OrElse timeStep >= NumTimeSteps Then
            Throw New ArgumentOutOfRangeException(NameOf(timeStep))
        End If
        _temporalEdgeFeatures(timeStep) = features
    End Sub

    ''' <summary>
    ''' 获取指定时间步的时序边特征
    ''' </summary>
    Public Function GetTemporalEdgeFeatures(timeStep As Integer) As Tensor
        If timeStep < 0 OrElse timeStep >= NumTimeSteps Then
            Throw New ArgumentOutOfRangeException(NameOf(timeStep))
        End If
        Return _temporalEdgeFeatures(timeStep)
    End Function

    ''' <summary>
    ''' 获取节点数量（假设所有时间步节点数量相同）
    ''' </summary>
    Public ReadOnly Property NumNodes As Integer
        Get
            If _snapshots(0) IsNot Nothing Then
                Return _snapshots(0).NumNodes
            End If
            Return 0
        End Get
    End Property

    ''' <summary>
    ''' 获取节点特征维度
    ''' </summary>
    Public ReadOnly Property FeatureDim As Integer
        Get
            If _snapshots(0) IsNot Nothing AndAlso _snapshots(0).NodeFeatures IsNot Nothing Then
                Return _snapshots(0).NodeFeatures.Shape(1)
            End If
            Return 0
        End Get
    End Property

    ''' <summary>
    ''' 构建时间感知的邻接张量
    ''' 返回形状为 [NumTimeSteps, NumNodes, NumNodes] 的张量
    ''' </summary>
    Public Function GetTemporalAdjacencyTensor() As Tensor
        Dim result = New Tensor(NumTimeSteps, NumNodes, NumNodes)

        For t = 0 To NumTimeSteps - 1
            If _snapshots(t) IsNot Nothing Then
                Dim adj = _snapshots(t).GetAdjacencyMatrix()
                For i = 0 To NumNodes - 1
                    For j = 0 To NumNodes - 1
                        result(t, i, j) = adj(i, j)
                    Next
                Next
            End If
        Next

        Return result
    End Function

    ''' <summary>
    ''' 构建时间感知的节点特征张量
    ''' 返回形状为 [NumTimeSteps, NumNodes, FeatureDim] 的张量
    ''' </summary>
    Public Function GetTemporalNodeFeatures() As Tensor
        Dim result = New Tensor(NumTimeSteps, NumNodes, FeatureDim)

        For t = 0 To NumTimeSteps - 1
            If _snapshots(t) IsNot Nothing AndAlso _snapshots(t).NodeFeatures IsNot Nothing Then
                Dim features = _snapshots(t).NodeFeatures
                For i = 0 To NumNodes - 1
                    For j = 0 To FeatureDim - 1
                        result(t, i, j) = features(i, j)
                    Next
                Next
            End If
        Next

        Return result
    End Function

    ''' <summary>
    ''' 获取归一化的时序邻接张量
    ''' 对每个时间步的邻接矩阵进行GCN风格的归一化
    ''' </summary>
    Public Function GetNormalizedTemporalAdjacency() As Tensor
        Dim result = New Tensor(NumTimeSteps, NumNodes, NumNodes)

        For t = 0 To NumTimeSteps - 1
            If _snapshots(t) IsNot Nothing Then
                Dim normAdj = _snapshots(t).GetNormalizedAdjacencyMatrix()
                For i = 0 To NumNodes - 1
                    For j = 0 To NumNodes - 1
                        result(t, i, j) = normAdj(i, j)
                    Next
                Next
            End If
        Next

        Return result
    End Function

    ''' <summary>
    ''' 打印时序图信息
    ''' </summary>
    Public Sub PrintInfo()
        Console.WriteLine("时序图信息:")
        Console.WriteLine($"  时间步数: {NumTimeSteps}")
        Console.WriteLine($"  节点数: {NumNodes}")
        Console.WriteLine($"  特征维度: {FeatureDim}")

        Dim totalEdges = 0
        For t = 0 To NumTimeSteps - 1
            If _snapshots(t) IsNot Nothing Then
                totalEdges += _snapshots(t).NumEdges
            End If
        Next
        Console.WriteLine($"  总边数: {totalEdges}")
        Console.WriteLine($"  平均每时间步边数: {totalEdges / NumTimeSteps:F2}")
    End Sub
End Class

''' <summary>
''' 时序图数据集
''' 用于存储多个时序图样本，常用于时序图分类/预测任务
''' </summary>
Public Class TemporalGraphDataset
    Private _graphs As List(Of TemporalGraph)
    Private _labels As List(Of Single())

    Public Sub New()
        _graphs = New List(Of TemporalGraph)()
        _labels = New List(Of Single())()
    End Sub

    ''' <summary>
    ''' 添加时序图样本
    ''' </summary>
    ''' <param name="graph">时序图</param>
    ''' <param name="label">标签（可以是分类标签或回归目标）</param>
    Public Sub Add(graph As TemporalGraph, label As Single())
        _graphs.Add(graph)
        _labels.Add(label)
    End Sub

    ''' <summary>
    ''' 添加时序图样本（分类任务）
    ''' </summary>
    Public Sub Add(graph As TemporalGraph, label As Integer)
        _graphs.Add(graph)
        _labels.Add(New Single() {CSng(label)})
    End Sub

    Public ReadOnly Property Count As Integer
        Get
            Return _graphs.Count
        End Get
    End Property

    Default Public Property Item(index As Integer) As TemporalGraph
        Get
            Return _graphs(index)
        End Get
        Set(value As TemporalGraph)
            _graphs(index) = value
        End Set
    End Property

    Public Function GetLabel(index As Integer) As Single()
        Return _labels(index)
    End Function

    Public Function GetGraphs() As List(Of TemporalGraph)
        Return _graphs
    End Function

    Public Function GetLabels() As List(Of Single())
        Return _labels
    End Function
End Class

''' <summary>
''' 时序图构建器
''' 提供便捷的方法构建时序图
''' </summary>
Public Class TemporalGraphBuilder
    ''' <summary>
    ''' 从滑动窗口构建时序图
    ''' 适用于时间序列预测任务
    ''' </summary>
    ''' <param name="timeSeriesData">时间序列数据 [numTimeSteps, numNodes, featureDim]</param>
    ''' <param name="windowSize">滑动窗口大小</param>
    ''' <param name="stride">滑动步长</param>
    ''' <param name="adjacencyMatrix">静态邻接矩阵（可选，如果为Nothing则自动构建）</param>
    ''' <param name="knnK">KNN图的k值（当adjacencyMatrix为Nothing时使用）</param>
    Public Shared Function BuildFromSlidingWindow(
        timeSeriesData As Tensor,
        windowSize As Integer,
        stride As Integer,
        Optional adjacencyMatrix As Tensor = Nothing,
        Optional knnK As Integer = 5) As List(Of TemporalGraph)

        If timeSeriesData.Rank <> 3 Then
            Throw New ArgumentException("时间序列数据必须是三维张量 [numTimeSteps, numNodes, featureDim]")
        End If

        Dim numTimeSteps = timeSeriesData.Shape(0)
        Dim numNodes = timeSeriesData.Shape(1)
        Dim featureDim = timeSeriesData.Shape(2)

        Dim result = New List(Of TemporalGraph)()

        ' 如果没有提供邻接矩阵，使用KNN构建
        Dim adj = adjacencyMatrix
        If adj Is Nothing Then
            adj = BuildKNNGraph(timeSeriesData, knnK)
        End If

        ' 滑动窗口构建时序图
        For startIdx = 0 To numTimeSteps - windowSize Step stride
            Dim tg = New TemporalGraph(windowSize)

            For t = 0 To windowSize - 1
                Dim actualTimeStep = startIdx + t
                Dim nodeFeatures = New Tensor(numNodes, featureDim)

                ' 提取当前时间步的节点特征
                For i = 0 To numNodes - 1
                    For j = 0 To featureDim - 1
                        nodeFeatures(i, j) = timeSeriesData(actualTimeStep, i, j)
                    Next
                Next

                ' 创建图快照
                Dim graph = New Graph(nodeFeatures)

                ' 添加边
                For i = 0 To numNodes - 1
                    For j = 0 To numNodes - 1
                        If adj(i, j) > 0 Then
                            graph.AddEdge(i, j, adj(i, j))
                        End If
                    Next
                Next

                tg(t) = graph
            Next

            result.Add(tg)
        Next

        Return result
    End Function

    ''' <summary>
    ''' 使用KNN方法构建图邻接矩阵
    ''' </summary>
    Private Shared Function BuildKNNGraph(data As Tensor, k As Integer) As Tensor
        Dim numTimeSteps = data.Shape(0)
        Dim numNodes = data.Shape(1)
        Dim featureDim = data.Shape(2)

        ' 计算平均特征用于构建静态图
        Dim avgFeatures = New Tensor(numNodes, featureDim)
        For i = 0 To numNodes - 1
            For j = 0 To featureDim - 1
                Dim sum As Single = 0
                For t = 0 To numTimeSteps - 1
                    sum += data(t, i, j)
                Next
                avgFeatures(i, j) = sum / numTimeSteps
            Next
        Next

        ' 计算节点间距离并构建KNN图
        Dim adj = New Tensor(numNodes, numNodes)
        For i = 0 To numNodes - 1
            Dim distances = New List(Of (index As Integer, dist As Single))()

            For j = 0 To numNodes - 1
                If i <> j Then
                    Dim dist As Single = 0
                    For d = 0 To featureDim - 1
                        Dim diff = avgFeatures(i, d) - avgFeatures(j, d)
                        dist += diff * diff
                    Next
                    distances.Add((j, CSng(Math.Sqrt(dist))))
                End If
            Next

            ' 选择k个最近邻
            distances.Sort(Function(a, b) a.dist.CompareTo(b.dist))
            For idx = 0 To Math.Min(k - 1, distances.Count - 1)
                Dim j = distances(idx).index
                ' 使用高斯核函数计算边权重
                Dim weight = CSng(Math.Exp(-distances(idx).dist * distances(idx).dist / 2))
                adj(i, j) = weight
                adj(j, i) = weight ' 无向图
            Next
        Next

        Return adj
    End Function

    ''' <summary>
    ''' 从动态边列表构建时序图
    ''' </summary>
    ''' <param name="numNodes">节点数量</param>
    ''' <param name="featureDim">特征维度</param>
    ''' <param name="nodeFeaturesPerStep">每个时间步的节点特征列表</param>
    ''' <param name="edgesPerStep">每个时间步的边列表</param>
    Public Shared Function BuildFromDynamicEdges(
        numNodes As Integer,
        featureDim As Integer,
        nodeFeaturesPerStep As List(Of Tensor),
        edgesPerStep As List(Of List(Of (source As Integer, target As Integer, weight As Single)))) As TemporalGraph

        If nodeFeaturesPerStep.Count <> edgesPerStep.Count Then
            Throw New ArgumentException("节点特征列表和边列表长度必须相同")
        End If

        Dim numTimeSteps = nodeFeaturesPerStep.Count
        Dim tg = New TemporalGraph(numTimeSteps)

        For t = 0 To numTimeSteps - 1
            Dim graph = New Graph(nodeFeaturesPerStep(t))

            For Each edge In edgesPerStep(t)
                graph.AddEdge(edge.source, edge.target, edge.weight)
            Next

            tg(t) = graph
        Next

        Return tg
    End Function
End Class
