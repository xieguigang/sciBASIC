Imports System
Imports System.Collections.Generic
Imports System.Linq


''' <summary>
''' 优化器基类
''' 定义了参数更新的接口
''' </summary>
Public MustInherit Class Optimizer
    ''' <summary>
    ''' 学习率
    ''' </summary>
    Public Property LearningRate As Single

    ''' <summary>
    ''' 需要优化的参数列表
    ''' </summary>
    Protected _parameters As List(Of Tensor)

    ''' <summary>
    ''' 参数对应的梯度列表
    ''' </summary>
    Protected _gradients As List(Of Tensor)

    Protected Sub New(parameters As List(Of Tensor), gradients As List(Of Tensor), learningRate As Single)
        If parameters.Count <> gradients.Count Then Throw New ArgumentException("参数数量和梯度数量必须相同")

        _parameters = parameters
        _gradients = gradients
        Me.LearningRate = learningRate
    End Sub

    ''' <summary>
    ''' 执行一步参数更新
    ''' </summary>
    Public MustOverride Sub [Step]()

    ''' <summary>
    ''' 清零梯度
    ''' </summary>
    Public Overridable Sub ZeroGrad()
        For Each grad In _gradients
            For i = 0 To grad.Length - 1
                grad(i) = 0
            Next
        Next
    End Sub
End Class

''' <summary>
''' 随机梯度下降优化器 (SGD)
''' 最基础的优化器：θ = θ - lr * ∇θ
''' </summary>
Public Class SGDOptimizer
    Inherits Optimizer
    ''' <summary>
    ''' 动量系数
    ''' </summary>
    Public Property Momentum As Single

    ''' <summary>
    ''' 速度（动量）缓存
    ''' </summary>
    Private _velocities As List(Of Tensor)

    ''' <summary>
    ''' 创建SGD优化器
    ''' </summary>
    ''' <param name="parameters">需要优化的参数</param>
    ''' <param name="gradients">参数对应的梯度</param>
    ''' <param name="learningRate">学习率</param>
    ''' <param name="momentum">动量系数（0表示不使用动量）</param>
    Public Sub New(parameters As List(Of Tensor), gradients As List(Of Tensor), Optional learningRate As Single = 0.01F, Optional momentum As Single = 0.0F)

        MyBase.New(parameters, gradients, learningRate)
        Me.Momentum = momentum

        ' 初始化速度缓存
        _velocities = New List(Of Tensor)()
        For Each param In parameters
            _velocities.Add(New Tensor(param.Shape))
        Next
    End Sub

    Public Overrides Sub [Step]()
        For i = 0 To _parameters.Count - 1
            Dim param = _parameters(i)
            Dim grad = _gradients(i)
            Dim velocity = _velocities(i)

            For j = 0 To param.Length - 1
                If Momentum > 0 Then
                    ' 带动量的更新
                    ' v = momentum * v - lr * grad
                    ' param = param + v
                    velocity(j) = Momentum * velocity(j) - LearningRate * grad(j)
                    param(j) += velocity(j)
                Else
                    ' 普通SGD
                    param(j) -= LearningRate * grad(j)
                End If
            Next
        Next
    End Sub
End Class

''' <summary>
''' Adam优化器
''' 自适应学习率优化器，结合了动量和RMSprop的优点
''' 论文: Adam: A Method for Stochastic Optimization (Kingma & Ba, ICLR 2015)
''' </summary>
Public Class AdamOptimizer
    Inherits Optimizer
    ''' <summary>
    ''' 一阶矩估计的指数衰减率
    ''' </summary>
    Public Property Beta1 As Single

    ''' <summary>
    ''' 二阶矩估计的指数衰减率
    ''' </summary>
    Public Property Beta2 As Single

    ''' <summary>
    ''' 数值稳定性常数
    ''' </summary>
    Public Property Epsilon As Single

    ''' <summary>
    ''' 当前时间步
    ''' </summary>
    Private _t As Integer

    ''' <summary>
    ''' 一阶矩估计（梯度的移动平均）
    ''' </summary>
    Private _m As List(Of Tensor)

    ''' <summary>
    ''' 二阶矩估计（梯度平方的移动平均）
    ''' </summary>
    Private _v As List(Of Tensor)

    ''' <summary>
    ''' 创建Adam优化器
    ''' </summary>
    Public Sub New(parameters As List(Of Tensor), gradients As List(Of Tensor), Optional learningRate As Single = 0.001F, Optional beta1 As Single = 0.9F, Optional beta2 As Single = 0.999F, Optional epsilon As Single = 0.00000001F)

        MyBase.New(parameters, gradients, learningRate)
        Me.Beta1 = beta1
        Me.Beta2 = beta2
        Me.Epsilon = epsilon
        _t = 0

        ' 初始化矩估计
        _m = New List(Of Tensor)()
        _v = New List(Of Tensor)()
        For Each param In parameters
            _m.Add(New Tensor(param.Shape))
            _v.Add(New Tensor(param.Shape))
        Next
    End Sub

    Public Overrides Sub [Step]()
        _t += 1

        For i = 0 To _parameters.Count - 1
            Dim param = _parameters(i)
            Dim grad = _gradients(i)
            Dim m = _m(i)
            Dim v = _v(i)

            For j = 0 To param.Length - 1
                Dim g = grad(j)

                ' 更新一阶矩估计
                m(j) = Beta1 * m(j) + (1 - Beta1) * g

                ' 更新二阶矩估计
                v(j) = Beta2 * v(j) + (1 - Beta2) * g * g

                ' 偏差修正
                Dim mHat = m(j) / (1 - CSng(Math.Pow(Beta1, _t)))
                Dim vHat = v(j) / (1 - CSng(Math.Pow(Beta2, _t)))

                ' 参数更新
                param(j) -= LearningRate * mHat / (CSng(Math.Sqrt(vHat)) + Epsilon)
            Next
        Next
    End Sub
End Class

''' <summary>
''' GNN模型基类
''' 定义了图神经网络模型的基本接口
''' </summary>
Public MustInherit Class GNNModel

    ''' <summary>
    ''' 模型名称
    ''' </summary>
    Private _Name As String
    ''' <summary>
    ''' 模型中的所有层
    ''' </summary>
    Protected _layers As List(Of Layer) = New List(Of Layer)()

    Public Property Name As String
        Get
            Return _Name
        End Get
        Protected Set(value As String)
            _Name = value
        End Set
    End Property

    ''' <summary>
    ''' 前向传播
    ''' </summary>
    Public MustOverride Function Forward(nodeFeatures As Tensor, graph As Graph) As Tensor

    ''' <summary>
    ''' 反向传播
    ''' </summary>
    Public MustOverride Function Backward(gradient As Tensor, graph As Graph) As Tensor

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

    ''' <summary>
    ''' 设置训练/评估模式
    ''' </summary>
    Public Overridable Sub SetTraining(isTraining As Boolean)
        For Each layer In _layers
            layer.IsTraining = isTraining
        Next
    End Sub

    ''' <summary>
    ''' 打印模型结构
    ''' </summary>
    Public Overridable Sub PrintModelInfo()
        Console.WriteLine($"模型: {Name}")
        Console.WriteLine($"层数: {_layers.Count}")

        Dim totalParams = 0
        For Each layer In _layers
            Dim layerParams = layer.GetParameters()
            Dim layerParamCount = layerParams.Sum(Function(p) p.Length)
            totalParams += layerParamCount
            Console.WriteLine($"  - {layer.Name}: {layerParamCount} 参数")
        Next

        Console.WriteLine($"总参数量: {totalParams}")
    End Sub
End Class

''' <summary>
''' GCN模型（图卷积网络）
''' 用于节点分类任务
''' 结构: GCN -> ReLU -> GCN -> Softmax
''' </summary>
Public Class GCNModel
    Inherits GNNModel
    Private ReadOnly _gcn1 As GCNConvLayer
    Private ReadOnly _gcn2 As GCNConvLayer
    Private _normAdj As Tensor
    Private _hidden As Tensor
    Private _lastInput As Tensor

    ''' <summary>
    ''' 创建GCN模型
    ''' </summary>
    ''' <param name="inputDim">输入特征维度</param>
    ''' <param name="hiddenDim">隐藏层维度</param>
    ''' <param name="outputDim">输出维度（类别数）</param>
    ''' <param name="dropout">Dropout率</param>
    Public Sub New(inputDim As Integer, hiddenDim As Integer, outputDim As Integer, Optional dropout As Single = 0.5F)
        Name = "GCN"

        ' 第一层GCN: inputDim -> hiddenDim
        _gcn1 = New GCNConvLayer(inputDim, hiddenDim, ActivationType.ReLU)

        ' 第二层GCN: hiddenDim -> outputDim
        _gcn2 = New GCNConvLayer(hiddenDim, outputDim, ActivationType.None)

        _layers.Add(_gcn1)
        _layers.Add(_gcn2)
    End Sub

    Public Overrides Function Forward(nodeFeatures As Tensor, graph As Graph) As Tensor
        _lastInput = nodeFeatures
        _normAdj = graph.GetNormalizedAdjacencyMatrix()

        ' 第一层: GCN + ReLU
        _hidden = _gcn1.Forward(nodeFeatures, _normAdj)

        ' 第二层: GCN (输出logits)
        Dim logits = _gcn2.Forward(_hidden, _normAdj)

        ' 应用Softmax得到概率
        Dim probs = Apply(logits, ActivationType.Softmax)

        Return probs
    End Function

    ''' <summary>
    ''' 获取logits（未归一化的输出）
    ''' </summary>
    Public Function ForwardLogits(nodeFeatures As Tensor, graph As Graph) As Tensor
        _normAdj = graph.GetNormalizedAdjacencyMatrix()
        _hidden = _gcn1.Forward(nodeFeatures, _normAdj)
        Return _gcn2.Forward(_hidden, _normAdj)
    End Function

    Public Overrides Function Backward(gradient As Tensor, graph As Graph) As Tensor
        ' Softmax + CrossEntropy的梯度可以直接用 probs - one_hot(labels)
        ' 这里假设传入的gradient已经是正确的梯度

        ' 第二层反向传播
        Dim gradHidden = _gcn2.Backward(gradient, _normAdj)

        ' ReLU的梯度
        Dim reluDerivative = Derivative(_hidden, ActivationType.ReLU)
        gradHidden = gradHidden.ElementwiseMultiply(reluDerivative)

        ' 第一层反向传播
        Dim inputGrad = _gcn1.Backward(gradHidden, _normAdj)

        Return inputGrad
    End Function
End Class

''' <summary>
''' 图分类模型
''' 用于图级别的分类任务
''' 结构: GCN -> Pooling -> Linear -> Softmax
''' </summary>
Public Class GraphClassificationModel
    Inherits GNNModel
    Private ReadOnly _gcn1 As GCNConvLayer
    Private ReadOnly _gcn2 As GCNConvLayer
    Private ReadOnly _pooling As GlobalPoolingLayer
    Private ReadOnly _classifier As LinearLayer

    Private _normAdj As Tensor
    Private _hidden1 As Tensor
    Private _hidden2 As Tensor
    Private _pooled As Tensor

    ''' <summary>
    ''' 创建图分类模型
    ''' </summary>
    ''' <param name="inputDim">输入特征维度</param>
    ''' <param name="hiddenDim">隐藏层维度</param>
    ''' <param name="numClasses">类别数</param>
    Public Sub New(inputDim As Integer, hiddenDim As Integer, numClasses As Integer)
        Name = "GraphClassifier"

        _gcn1 = New GCNConvLayer(inputDim, hiddenDim, ActivationType.ReLU)
        _gcn2 = New GCNConvLayer(hiddenDim, hiddenDim, ActivationType.ReLU)
        _pooling = New GlobalPoolingLayer(GlobalPoolingLayer.PoolingType.Mean)
        _classifier = New LinearLayer(hiddenDim, numClasses)

        _layers.Add(_gcn1)
        _layers.Add(_gcn2)
        _layers.Add(_pooling)
        _layers.Add(_classifier)
    End Sub

    Public Overrides Function Forward(nodeFeatures As Tensor, graph As Graph) As Tensor
        _normAdj = graph.GetNormalizedAdjacencyMatrix()

        ' GCN层
        _hidden1 = _gcn1.Forward(nodeFeatures, _normAdj)
        _hidden2 = _gcn2.Forward(_hidden1, _normAdj)

        ' 全局池化
        _pooled = _pooling.Forward(_hidden2)

        ' 分类器
        Dim logits = _classifier.Forward(_pooled)

        ' Softmax
        Dim probs = Apply(logits, ActivationType.Softmax)

        Return probs
    End Function

    Public Overrides Function Backward(gradient As Tensor, graph As Graph) As Tensor
        ' 分类器反向传播
        Dim gradPooled = _classifier.Backward(gradient)

        ' 池化反向传播
        Dim gradHidden2 = _pooling.Backward(gradPooled)

        ' ReLU梯度
        Dim reluDerivative2 = Derivative(_hidden2, ActivationType.ReLU)
        gradHidden2 = gradHidden2.ElementwiseMultiply(reluDerivative2)

        ' GCN2反向传播
        Dim gradHidden1 = _gcn2.Backward(gradHidden2, _normAdj)

        ' ReLU梯度
        Dim reluDerivative1 = Derivative(_hidden1, ActivationType.ReLU)
        gradHidden1 = gradHidden1.ElementwiseMultiply(reluDerivative1)

        ' GCN1反向传播
        Dim inputGrad = _gcn1.Backward(gradHidden1, _normAdj)

        Return inputGrad
    End Function
End Class

''' <summary>
''' 训练器
''' 封装了训练循环、验证和测试逻辑
''' </summary>
Public Class Trainer
    Private ReadOnly _model As GNNModel
    Private ReadOnly _optimizer As Optimizer
    Private ReadOnly _lossType As LossType

    ''' <summary>
    ''' 训练损失历史
    ''' </summary>
    Public ReadOnly Property TrainLossHistory As List(Of Single) = New List(Of Single)()

    ''' <summary>
    ''' 验证准确率历史
    ''' </summary>
    Public ReadOnly Property ValAccuracyHistory As List(Of Single) = New List(Of Single)()

    Public Sub New(model As GNNModel, optimizer As Optimizer, Optional lossType As LossType = LossType.CrossEntropy)
        _model = model
        _optimizer = optimizer
        _lossType = lossType
    End Sub

    ''' <summary>
    ''' 训练一个epoch
    ''' </summary>
    ''' <param name="graph">图数据</param>
    ''' <param name="labels">节点标签</param>
    ''' <param name="trainMask">训练集掩码（标记哪些节点用于训练）</param>
    ''' <returns>平均训练损失</returns>
    Public Function TrainEpoch(graph As Graph, labels As Integer(), trainMask As Boolean()) As Single
        _model.SetTraining(True)
        _optimizer.ZeroGrad()

        ' 前向传播
        Dim probs = _model.Forward(graph.NodeFeatures, graph)

        ' 计算损失（只在训练节点上）
        Dim totalLoss As Single = 0
        Dim trainCount = 0

        ' 创建梯度张量
        Dim gradient = New Tensor(probs.Shape)

        For i = 0 To labels.Length - 1
            If Not trainMask(i) Then Continue For

            trainCount += 1

            ' 计算交叉熵损失及其梯度
            ' 对于Softmax + CrossEntropy，梯度 = probs - one_hot(label)
            For j = 0 To probs.Shape(1) - 1
                If j = labels(i) Then
                    totalLoss -= CSng(Math.Log(Math.Max(probs(i, j), 0.0000001F)))
                    gradient(i, j) = probs(i, j) - 1
                Else
                    gradient(i, j) = probs(i, j)
                End If
            Next
        Next

        Dim avgLoss = totalLoss / trainCount
        TrainLossHistory.Add(avgLoss)

        ' 反向传播
        _model.Backward(gradient, graph)

        ' 参数更新
        _optimizer.Step()

        Return avgLoss
    End Function

    ''' <summary>
    ''' 评估模型
    ''' </summary>
    ''' <param name="graph">图数据</param>
    ''' <param name="labels">节点标签</param>
    ''' <param name="evalMask">评估集掩码</param>
    ''' <returns>准确率</returns>
    Public Function Evaluate(graph As Graph, labels As Integer(), evalMask As Boolean()) As Single
        _model.SetTraining(False)

        Dim probs = _model.Forward(graph.NodeFeatures, graph)

        Dim correct = 0
        Dim total = 0

        For i = 0 To labels.Length - 1
            If Not evalMask(i) Then Continue For

            total += 1

            ' 找到预测的类别
            Dim predictedClass = 0
            Dim maxProb = probs(i, 0)
            For j = 1 To probs.Shape(1) - 1
                If probs(i, j) > maxProb Then
                    maxProb = probs(i, j)
                    predictedClass = j
                End If
            Next

            If predictedClass = labels(i) Then correct += 1
        Next

        Return If(total > 0, CSng(correct) / total, 0)
    End Function

    ''' <summary>
    ''' 完整训练流程
    ''' </summary>
    Public Sub Train(graph As Graph, labels As Integer(), trainMask As Boolean(), valMask As Boolean(), epochs As Integer, Optional printEvery As Integer = 10)
        Console.WriteLine($"开始训练，共 {epochs} 个epoch")
        Console.WriteLine(New String("-"c, 50))

        For epoch = 0 To epochs - 1
            Dim loss = TrainEpoch(graph, labels, trainMask)
            Dim valAcc = Evaluate(graph, labels, valMask)
            ValAccuracyHistory.Add(valAcc)

            If (epoch + 1) Mod printEvery = 0 OrElse epoch = 0 Then
                Dim trainAcc = Evaluate(graph, labels, trainMask)
                Console.WriteLine($"Epoch {epoch + 1,4}/{epochs}: " & $"Loss = {loss:F4}, Train Acc = {trainAcc:P2}, Val Acc = {valAcc:P2}")
            End If
        Next

        Console.WriteLine(New String("-"c, 50))
        Console.WriteLine("训练完成！")
    End Sub
End Class

''' <summary>
''' 图分类训练器
''' 用于图级别分类任务的训练
''' </summary>
Public Class GraphClassificationTrainer
    Private ReadOnly _model As GraphClassificationModel
    Private ReadOnly _optimizer As Optimizer

    Public ReadOnly Property TrainLossHistory As List(Of Single) = New List(Of Single)()
    Public ReadOnly Property ValAccuracyHistory As List(Of Single) = New List(Of Single)()

    Public Sub New(model As GraphClassificationModel, optimizer As Optimizer)
        _model = model
        _optimizer = optimizer
    End Sub

    ''' <summary>
    ''' 训练一个epoch
    ''' </summary>
    Public Function TrainEpoch(dataset As GraphDataset, trainIndices As Integer()) As Single
        _model.SetTraining(True)

        Dim totalLoss As Single = 0

        For Each idx In trainIndices
            _optimizer.ZeroGrad()

            Dim graph = dataset.Graphs(idx)
            Dim label = dataset.Labels(idx)

            ' 前向传播
            Dim probs = _model.Forward(graph.NodeFeatures, graph)

            ' 计算损失和梯度
            Dim gradient = New Tensor(probs.Shape)
            Dim loss As Single = 0

            For j = 0 To probs.Shape(1) - 1
                If j = label Then
                    loss = -CSng(Math.Log(Math.Max(probs(0, j), 0.0000001F)))
                    gradient(0, j) = probs(0, j) - 1
                Else
                    gradient(0, j) = probs(0, j)
                End If
            Next

            totalLoss += loss

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
    Public Function Evaluate(dataset As GraphDataset, evalIndices As Integer()) As Single
        _model.SetTraining(False)

        Dim correct = 0

        For Each idx In evalIndices
            Dim graph = dataset.Graphs(idx)
            Dim label = dataset.Labels(idx)

            Dim probs = _model.Forward(graph.NodeFeatures, graph)

            ' 找到预测类别
            Dim predictedClass = 0
            Dim maxProb = probs(0, 0)
            For j = 1 To probs.Shape(1) - 1
                If probs(0, j) > maxProb Then
                    maxProb = probs(0, j)
                    predictedClass = j
                End If
            Next

            If predictedClass = label Then correct += 1
        Next

        Return CSng(correct) / evalIndices.Length
    End Function

    ''' <summary>
    ''' 完整训练流程
    ''' </summary>
    Public Sub Train(dataset As GraphDataset, trainRatio As Single, epochs As Integer, Optional printEvery As Integer = 10)
        Dim totalSamples = dataset.Count
        Dim trainSize As Integer = totalSamples * trainRatio

        ' 随机划分训练集和验证集
        Dim indices = Enumerable.Range(0, totalSamples).ToArray()
        Dim random = New Random(42)
        For i = indices.Length - 1 To 1 Step -1
            Dim j = random.Next(i + 1)
            Dim ij = (indices(j), indices(i))
            indices(i) = ij.Item1
            indices(j) = ij.Item2
        Next

        Dim trainIndices = indices.Take(trainSize).ToArray()
        Dim valIndices = indices.Skip(trainSize).ToArray()

        Console.WriteLine($"训练样本: {trainIndices.Length}, 验证样本: {valIndices.Length}")
        Console.WriteLine(New String("-"c, 50))

        For epoch = 0 To epochs - 1
            Dim loss = TrainEpoch(dataset, trainIndices)
            Dim valAcc = Evaluate(dataset, valIndices)
            Dim trainAcc = Evaluate(dataset, trainIndices)

            ValAccuracyHistory.Add(valAcc)

            If (epoch + 1) Mod printEvery = 0 OrElse epoch = 0 Then
                Console.WriteLine($"Epoch {epoch + 1,4}/{epochs}: " & $"Loss = {loss:F4}, Train Acc = {trainAcc:P2}, Val Acc = {valAcc:P2}")
            End If
        Next

        Console.WriteLine(New String("-"c, 50))
        Console.WriteLine("训练完成！")
    End Sub
End Class

