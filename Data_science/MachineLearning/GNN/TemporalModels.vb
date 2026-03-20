
Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports std = System.Math

''' <summary>
''' 时序图神经网络模型
''' 结合GNN和RNN的能力，处理动态图的时间序列数据
''' </summary>
''' <summary>
''' 时序GNN模型基类
''' </summary>
Public MustInherit Class TemporalGNNModel
    Protected _layers As List(Of Layer) = New List(Of Layer)()

    Public Property Name As String

    ''' <summary>
    ''' 前向传播
    ''' </summary>
    Public MustOverride Function Forward(temporalGraph As TemporalGraph) As Tensor

    ''' <summary>
    ''' 反向传播
    ''' </summary>
    Public MustOverride Function Backward(gradient As Tensor, temporalGraph As TemporalGraph) As Tensor

    ''' <summary>
    ''' 获取所有可训练参数
    ''' </summary>
    Public Overridable Function GetParameters() As List(Of Tensor)
        Dim parameters = New List(Of Tensor)()
        For Each layer In _layers
            parameters.AddRange(layer.GetParameters())
        Next
        Return parameters
    End Function

    ''' <summary>
    ''' 获取所有参数的梯度
    ''' </summary>
    Public Overridable Function GetGradients() As List(Of Tensor)
        Dim gradients = New List(Of Tensor)()
        For Each layer In _layers
            gradients.AddRange(layer.GetGradients())
        Next
        Return gradients
    End Function

    Public Overridable Sub SetTraining(isTraining As Boolean)
        For Each layer In _layers
            layer.IsTraining = isTraining
        Next
    End Sub
End Class

''' <summary>
''' T-GCN模型 (Temporal Graph Convolutional Network)
''' 结合GCN和GRU处理时序图数据
''' 
''' 结构:
'''   1. 对每个时间步的图应用GCN提取空间特征
'''   2. 使用GRU处理时间序列特征
'''   3. 输出预测结果
''' 
''' 论文参考: T-GCN: A Temporal Graph Convolutional Network for Traffic Prediction (Zhao et al., 2019)
''' </summary>
Public Class TGCNModel
    Inherits TemporalGNNModel
    Private ReadOnly _gcnLayer As GCNConvLayer
    Private ReadOnly _gruLayer As GRULayer
    Private ReadOnly _outputLayer As LinearLayer

    ' 中间结果（用于反向传播）
    Private _gcnOutputs As List(Of Tensor)
    Private _gruAllHidden As Tensor
    Private _gruFinalHidden As Tensor
    Private _lastTemporalGraph As TemporalGraph

    ''' <summary>
    ''' 创建T-GCN模型
    ''' </summary>
    ''' <param name="inputDim">输入特征维度</param>
    ''' <param name="gcnHiddenDim">GCN隐藏层维度</param>
    ''' <param name="gruHiddenDim">GRU隐藏层维度</param>
    ''' <param name="outputDim">输出维度</param>
    Public Sub New(inputDim As Integer, gcnHiddenDim As Integer, gruHiddenDim As Integer, outputDim As Integer)
        Name = "T-GCN"

        ' GCN层：提取空间特征
        _gcnLayer = New GCNConvLayer(inputDim, gcnHiddenDim, ActivationType.ReLU)

        ' GRU层：处理时间序列
        _gruLayer = New GRULayer(gcnHiddenDim, gruHiddenDim)

        ' 输出层
        _outputLayer = New LinearLayer(gruHiddenDim, outputDim)

        _layers.Add(_gcnLayer)
        _layers.Add(_gruLayer)
        _layers.Add(_outputLayer)

        _gcnOutputs = New List(Of Tensor)()
    End Sub

    ''' <summary>
    ''' 前向传播
    ''' </summary>
    Public Overrides Function Forward(temporalGraph As TemporalGraph) As Tensor
        _lastTemporalGraph = temporalGraph
        _gcnOutputs.Clear()

        Dim numTimeSteps = temporalGraph.NumTimeSteps
        Dim numNodes = temporalGraph.NumNodes

        ' 步骤1: 对每个时间步应用GCN
        Dim gcnSequence = New Tensor(numTimeSteps, numNodes, _gcnLayer.OutFeatures)

        For t = 0 To numTimeSteps - 1
            Dim graph = temporalGraph(t)
            Dim normAdj = graph.GetNormalizedAdjacencyMatrix()
            Dim gcnOutput = _gcnLayer.Forward(graph.NodeFeatures, normAdj)
            _gcnOutputs.Add(gcnOutput)

            ' 存储到序列张量
            For i = 0 To numNodes - 1
                For j = 0 To _gcnLayer.OutFeatures - 1
                    gcnSequence(t, i, j) = gcnOutput(i, j)
                Next
            Next
        Next

        ' 步骤2: 对每个节点应用GRU处理时间序列
        ' 这里我们对所有节点的特征进行平均，然后应用GRU
        ' 也可以选择对每个节点单独应用GRU
        Dim aggregatedSequence = New Tensor(numTimeSteps, 1, _gcnLayer.OutFeatures)
        For t = 0 To numTimeSteps - 1
            For j = 0 To _gcnLayer.OutFeatures - 1
                Dim sum As Single = 0
                For i = 0 To numNodes - 1
                    sum += gcnSequence(t, i, j)
                Next
                aggregatedSequence(t, 0, j) = sum / numNodes
            Next
        Next

        ' 应用GRU
        Dim gruResult = _gruLayer.ForwardSequence(aggregatedSequence)
        _gruAllHidden = gruResult.allHidden
        _gruFinalHidden = gruResult.finalHidden

        ' 步骤3: 输出层
        Dim output = _outputLayer.Forward(_gruFinalHidden)

        Return output
    End Function

    ''' <summary>
    ''' 节点级别的预测
    ''' 对每个节点单独应用GRU，输出每个节点的预测
    ''' </summary>
    Public Function ForwardNodeLevel(temporalGraph As TemporalGraph) As Tensor
        _lastTemporalGraph = temporalGraph
        _gcnOutputs.Clear()

        Dim numTimeSteps = temporalGraph.NumTimeSteps
        Dim numNodes = temporalGraph.NumNodes

        ' 对每个时间步应用GCN
        Dim gcnSequence = New Tensor(numTimeSteps, numNodes, _gcnLayer.OutFeatures)

        For t = 0 To numTimeSteps - 1
            Dim graph = temporalGraph(t)
            Dim normAdj = graph.GetNormalizedAdjacencyMatrix()
            Dim gcnOutput = _gcnLayer.Forward(graph.NodeFeatures, normAdj)
            _gcnOutputs.Add(gcnOutput)

            For i = 0 To numNodes - 1
                For j = 0 To _gcnLayer.OutFeatures - 1
                    gcnSequence(t, i, j) = gcnOutput(i, j)
                Next
            Next
        Next

        ' 对每个节点应用GRU
        Dim nodeOutputs = New Tensor(numNodes, _outputLayer.OutFeatures)

        For nodeIdx = 0 To numNodes - 1
            ' 提取该节点的时间序列
            Dim nodeSequence = New Tensor(numTimeSteps, 1, _gcnLayer.OutFeatures)
            For t = 0 To numTimeSteps - 1
                For j = 0 To _gcnLayer.OutFeatures - 1
                    nodeSequence(t, 0, j) = gcnSequence(t, nodeIdx, j)
                Next
            Next

            ' 应用GRU
            Dim gruResult = _gruLayer.ForwardSequence(nodeSequence)

            ' 应用输出层
            Dim output = _outputLayer.Forward(gruResult.finalHidden)

            For j = 0 To _outputLayer.OutFeatures - 1
                nodeOutputs(nodeIdx, j) = output(0, j)
            Next
        Next

        Return nodeOutputs
    End Function

    Public Overrides Function Backward(gradient As Tensor, temporalGraph As TemporalGraph) As Tensor
        ' 输出层反向传播
        Dim gradGruHidden = _outputLayer.Backward(gradient)

        ' GRU反向传播（简化版本，使用最后时间步的梯度）
        Dim gruGradient = New Tensor(1, 1, _gruLayer.HiddenSize)
        For j = 0 To _gruLayer.HiddenSize - 1
            gruGradient(0, 0, j) = gradGruHidden(0, j)
        Next

        Dim gradSequence = _gruLayer.BackwardSequence(gruGradient)

        ' GCN反向传播（简化版本）
        Dim numTimeSteps = temporalGraph.NumTimeSteps
        Dim numNodes = temporalGraph.NumNodes

        For t = 0 To numTimeSteps - 1
            Dim graph = temporalGraph(t)
            Dim normAdj = graph.GetNormalizedAdjacencyMatrix()

            ' 计算GCN的梯度
            Dim gcnGrad = New Tensor(numNodes, _gcnLayer.OutFeatures)
            For i = 0 To numNodes - 1
                For j = 0 To _gcnLayer.OutFeatures - 1
                    gcnGrad(i, j) = gradSequence(t, 0, j) / numNodes
                Next
            Next

            _gcnLayer.Backward(gcnGrad, normAdj)
        Next

        Return Nothing
    End Function
End Class

''' <summary>
''' A3T-GCN模型 (Attention-based Temporal Graph Convolutional Network)
''' 在T-GCN基础上添加时间注意力机制
''' </summary>
Public Class A3TGCNModel
    Inherits TemporalGNNModel
    Private ReadOnly _gcnLayer As GCNConvLayer
    Private ReadOnly _gruLayer As GRULayer
    Private ReadOnly _attentionWeights As Tensor
    Private ReadOnly _outputLayer As LinearLayer

    Private _attentionGrad As Tensor
    Private _gcnOutputs As List(Of Tensor)
    Private _gruAllHidden As Tensor
    Private _attentionScores As Tensor
    Private _contextVector As Tensor

    Public Sub New(inputDim As Integer, gcnHiddenDim As Integer, gruHiddenDim As Integer, outputDim As Integer)
        Name = "A3T-GCN"

        _gcnLayer = New GCNConvLayer(inputDim, gcnHiddenDim, ActivationType.ReLU)
        _gruLayer = New GRULayer(gcnHiddenDim, gruHiddenDim)
        _attentionWeights = Tensor.RandomNormal(New Integer() {gruHiddenDim}, 0, 0.1F)
        _outputLayer = New LinearLayer(gruHiddenDim, outputDim)

        _layers.Add(_gcnLayer)
        _layers.Add(_gruLayer)
        _layers.Add(_outputLayer)

        _attentionGrad = New Tensor(gruHiddenDim)
        _gcnOutputs = New List(Of Tensor)()
    End Sub

    Public Overrides Function Forward(temporalGraph As TemporalGraph) As Tensor
        _gcnOutputs.Clear()

        Dim numTimeSteps = temporalGraph.NumTimeSteps
        Dim numNodes = temporalGraph.NumNodes

        ' GCN处理每个时间步
        Dim gcnSequence = New Tensor(numTimeSteps, 1, _gcnLayer.OutFeatures)

        For t = 0 To numTimeSteps - 1
            Dim graph = temporalGraph(t)
            Dim normAdj = graph.GetNormalizedAdjacencyMatrix()
            Dim gcnOutput = _gcnLayer.Forward(graph.NodeFeatures, normAdj)
            _gcnOutputs.Add(gcnOutput)

            ' 平均池化
            For j = 0 To _gcnLayer.OutFeatures - 1
                Dim sum As Single = 0
                For i = 0 To numNodes - 1
                    sum += gcnOutput(i, j)
                Next
                gcnSequence(t, 0, j) = sum / numNodes
            Next
        Next

        ' GRU处理时间序列
        Dim gruResult = _gruLayer.ForwardSequence(gcnSequence)
        _gruAllHidden = gruResult.allHidden

        ' 计算时间注意力
        _attentionScores = New Tensor(numTimeSteps)
        Dim scoreSum As Single = 0

        For t = 0 To numTimeSteps - 1
            Dim score As Single = 0
            For i = 0 To _gruLayer.HiddenSize - 1
                score += _gruAllHidden(t, 0, i) * _attentionWeights(i)
            Next
            _attentionScores(t) = CSng(std.Exp(score)) ' softmax的分子
            scoreSum += _attentionScores(t)
        Next

        ' 归一化
        For t = 0 To numTimeSteps - 1
            _attentionScores(t) /= scoreSum
        Next

        ' 计算上下文向量
        _contextVector = New Tensor(1, _gruLayer.HiddenSize)
        For t = 0 To numTimeSteps - 1
            For i = 0 To _gruLayer.HiddenSize - 1
                _contextVector(0, i) += _attentionScores(t) * _gruAllHidden(t, 0, i)
            Next
        Next

        ' 输出层
        Return _outputLayer.Forward(_contextVector)
    End Function

    Public Overrides Function Backward(gradient As Tensor, temporalGraph As TemporalGraph) As Tensor
        ' 输出层反向传播
        Dim gradContext = _outputLayer.Backward(gradient)

        ' 注意力反向传播
        Dim gruGradient = New Tensor(temporalGraph.NumTimeSteps, 1, _gruLayer.HiddenSize)

        For t = 0 To temporalGraph.NumTimeSteps - 1
            For i = 0 To _gruLayer.HiddenSize - 1
                ' 对隐藏状态的梯度
                gruGradient(t, 0, i) = _attentionScores(t) * gradContext(0, i)

                ' 对注意力权重的梯度（简化）
                _attentionGrad(i) += _attentionScores(t) * gradContext(0, i) * _gruAllHidden(t, 0, i)
            Next
        Next

        ' GRU反向传播
        _gruLayer.BackwardSequence(gruGradient)

        Return Nothing
    End Function

    Public Overrides Function GetParameters() As List(Of Tensor)
        Dim params = MyBase.GetParameters()
        params.Add(_attentionWeights)
        Return params
    End Function

    Public Overrides Function GetGradients() As List(Of Tensor)
        Dim grads = MyBase.GetGradients()
        grads.Add(_attentionGrad)
        Return grads
    End Function
End Class

''' <summary>
''' 时序图预测训练器
''' 用于训练时序图神经网络模型
''' </summary>
Public Class TemporalGNNTrainer
    Private ReadOnly _model As TemporalGNNModel
    Private ReadOnly _optimizer As Optimizer

    Public ReadOnly Property TrainLossHistory As New List(Of Single)()
    Public ReadOnly Property ValLossHistory As New List(Of Single)()

    Public Sub New(model As TemporalGNNModel, optimizer As Optimizer)
        _model = model
        _optimizer = optimizer
    End Sub

    ''' <summary>
    ''' 训练一个epoch
    ''' </summary>
    Public Function TrainEpoch(dataset As TemporalGraphDataset, trainIndices As Integer()) As Single
        _model.SetTraining(True)

        Dim totalLoss As Single = 0

        For Each idx In trainIndices
            _optimizer.ZeroGrad()

            Dim graph = dataset(idx)
            Dim target = dataset.GetLabel(idx)

            ' 前向传播
            Dim prediction = _model.Forward(graph)

            ' 计算MSE损失
            Dim loss As Single = 0
            Dim gradient = New Tensor(prediction.Shape)

            For i = 0 To prediction.Length - 1
                Dim diff = prediction(i) - target(i)
                loss += diff * diff
                gradient(i) = 2 * diff / prediction.Length
            Next

            totalLoss += loss / prediction.Length

            ' 反向传播
            _model.Backward(gradient, graph)

            ' 参数更新
            _optimizer.Step()
        Next

        Dim avgLoss = totalLoss / trainIndices.Length
        TrainLossHistory.Add(avgLoss)

        Return avgLoss
    End Function

    ''' <summary>
    ''' 评估模型
    ''' </summary>
    Public Function Evaluate(dataset As TemporalGraphDataset, evalIndices As Integer()) As Single
        _model.SetTraining(False)

        Dim totalLoss As Single = 0

        For Each idx In evalIndices
            Dim graph = dataset(idx)
            Dim target = dataset.GetLabel(idx)

            Dim prediction = _model.Forward(graph)

            Dim loss As Single = 0
            For i = 0 To prediction.Length - 1
                Dim diff = prediction(i) - target(i)
                loss += diff * diff
            Next

            totalLoss += loss / prediction.Length
        Next

        Dim avgLoss = totalLoss / evalIndices.Length
        ValLossHistory.Add(avgLoss)

        Return avgLoss
    End Function

    ''' <summary>
    ''' 完整训练流程
    ''' </summary>
    Public Sub Train(dataset As TemporalGraphDataset, trainRatio As Single, epochs As Integer, Optional printEvery As Integer = 10)
        Dim totalSamples = dataset.Count
        Dim trainSize As Integer = CInt(totalSamples * trainRatio)

        ' 随机划分
        Dim indices = Enumerable.Range(0, totalSamples).ToArray()
        Dim random = New Random(42)
        For i = indices.Length - 1 To 1 Step -1
            Dim j = random.Next(i + 1)
            Dim temp = indices(i)
            indices(i) = indices(j)
            indices(j) = temp
        Next

        Dim trainIndices = indices.Take(trainSize).ToArray()
        Dim valIndices = indices.Skip(trainSize).ToArray()

        Console.WriteLine($"训练样本: {trainIndices.Length}, 验证样本: {valIndices.Length}")
        Console.WriteLine(New String("-"c, 50))

        For epoch = 0 To epochs - 1
            Dim trainLoss = TrainEpoch(dataset, trainIndices)
            Dim valLoss = Evaluate(dataset, valIndices)

            If (epoch + 1) Mod printEvery = 0 OrElse epoch = 0 Then
                Console.WriteLine($"Epoch {epoch + 1,4}/{epochs}: Train Loss = {trainLoss:F4}, Val Loss = {valLoss:F4}")
            End If
        Next

        Console.WriteLine(New String("-"c, 50))
        Console.WriteLine("训练完成！")
    End Sub
End Class

''' <summary>
''' 时间序列预测辅助类
''' 提供数据预处理和评估工具
''' </summary>
Public Class TimeSeriesUtils
    ''' <summary>
    ''' 创建滑动窗口数据集
    ''' </summary>
    ''' <param name="data">原始时间序列 [timeSteps, features]</param>
    ''' <param name="windowSize">输入窗口大小</param>
    ''' <param name="horizon">预测步长</param>
    ''' <param name="numNodes">节点数量（用于构建图）</param>
    Public Shared Function CreateSlidingWindowDataset(
        data As Tensor,
        windowSize As Integer,
        horizon As Integer,
        numNodes As Integer,
        Optional knnK As Integer = 5) As TemporalGraphDataset

        Dim timeSteps = data.Shape(0)
        Dim features = data.Shape(1)
        Dim featurePerNode = features \ numNodes

        Dim dataset = New TemporalGraphDataset()

        For startIdx = 0 To timeSteps - windowSize - horizon
            ' 创建时序图
            Dim tg = New TemporalGraph(windowSize)

            For t = 0 To windowSize - 1
                Dim actualTimeStep = startIdx + t
                Dim nodeFeatures = New Tensor(numNodes, featurePerNode)

                For n = 0 To numNodes - 1
                    For f = 0 To featurePerNode - 1
                        nodeFeatures(n, f) = data(actualTimeStep, n * featurePerNode + f)
                    Next
                Next

                Dim graph = New Graph(nodeFeatures)

                ' 构建KNN图
                For i = 0 To numNodes - 1
                    Dim distances = New List(Of (index As Integer, dist As Single))()
                    For j = 0 To numNodes - 1
                        If i <> j Then
                            Dim dist As Single = 0
                            For f = 0 To featurePerNode - 1
                                Dim diff = nodeFeatures(i, f) - nodeFeatures(j, f)
                                dist += diff * diff
                            Next
                            distances.Add((j, CSng(std.Sqrt(dist))))
                        End If
                    Next

                    distances.Sort(Function(a, b) a.dist.CompareTo(b.dist))
                    For k = 0 To std.Min(knnK - 1, distances.Count - 1)
                        Dim j = distances(k).index
                        Dim weight = CSng(std.Exp(-distances(k).dist / 2))
                        graph.AddUndirectedEdge(i, j, weight)
                    Next
                Next

                tg(t) = graph
            Next

            ' 创建目标标签（预测未来horizon步的值）
            Dim target = New Single(features - 1) {}
            For f = 0 To features - 1
                target(f) = data(startIdx + windowSize + horizon - 1, f)
            Next

            dataset.Add(tg, target)
        Next

        Return dataset
    End Function

    ''' <summary>
    ''' 计算MAE（平均绝对误差）
    ''' </summary>
    Public Shared Function MAE(predicted As Tensor, target As Tensor) As Single
        Dim sum As Single = 0
        For i = 0 To predicted.Length - 1
            sum += std.Abs(predicted(i) - target(i))
        Next
        Return sum / predicted.Length
    End Function

    ''' <summary>
    ''' 计算RMSE（均方根误差）
    ''' </summary>
    Public Shared Function RMSE(predicted As Tensor, target As Tensor) As Single
        Dim sumSquares As Single = 0
        For i = 0 To predicted.Length - 1
            Dim diff = predicted(i) - target(i)
            sumSquares += diff * diff
        Next
        Return CSng(std.Sqrt(sumSquares / predicted.Length))
    End Function

    ''' <summary>
    ''' 计算MAPE（平均绝对百分比误差）
    ''' </summary>
    Public Shared Function MAPE(predicted As Tensor, target As Tensor) As Single
        Dim sum As Single = 0
        Dim count As Integer = 0

        For i = 0 To predicted.Length - 1
            If std.Abs(target(i)) > 0.0001F Then
                sum += std.Abs((predicted(i) - target(i)) / target(i))
                count += 1
            End If
        Next

        Return If(count > 0, sum / count * 100, Single.NaN)
    End Function

    ''' <summary>
    ''' 归一化数据到[0, 1]范围
    ''' </summary>
    Public Shared Function Normalize(data As Tensor) As (normalized As Tensor, minVal As Single, maxVal As Single)
        Dim minVal As Single = Single.MaxValue
        Dim maxVal As Single = Single.MinValue

        For i = 0 To data.Length - 1
            If data(i) < minVal Then minVal = data(i)
            If data(i) > maxVal Then maxVal = data(i)
        Next

        Dim range = maxVal - minVal
        If range = 0 Then range = 1

        Dim normalized = New Tensor(data.Shape)
        For i = 0 To data.Length - 1
            normalized(i) = (data(i) - minVal) / range
        Next

        Return (normalized, minVal, maxVal)
    End Function

    ''' <summary>
    ''' 反归一化
    ''' </summary>
    Public Shared Function Denormalize(normalized As Tensor, minVal As Single, maxVal As Single) As Tensor
        Dim range = maxVal - minVal
        Dim result = New Tensor(normalized.Shape)

        For i = 0 To normalized.Length - 1
            result(i) = normalized(i) * range + minVal
        Next

        Return result
    End Function
End Class
