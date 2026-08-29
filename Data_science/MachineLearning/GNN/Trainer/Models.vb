#Region "Microsoft.VisualBasic::1f8308f833097308994369e4184ab0f3, Data_science\MachineLearning\GNN\Models.vb"

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

    '   Total Lines: 697
    '    Code Lines: 364 (52.22%)
    ' Comment Lines: 200 (28.69%)
    '    - Xml Docs: 78.00%
    ' 
    '   Blank Lines: 133 (19.08%)
    '     File Size: 22.34 KB


    ' Class Optimizer
    ' 
    '     Properties: LearningRate
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Sub: ZeroGrad
    ' 
    ' Class SGDOptimizer
    ' 
    '     Properties: Momentum
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Sub: [Step]
    ' 
    ' Class AdamOptimizer
    ' 
    '     Properties: Beta1, Beta2, Epsilon
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Sub: [Step]
    ' 
    ' Class GNNModel
    ' 
    '     Properties: Name
    ' 
    '     Function: GetGradients, GetParameters
    ' 
    '     Sub: PrintModelInfo, SetTraining
    ' 
    ' Class GCNModel
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: Backward, Forward, ForwardLogits
    ' 
    ' Class GraphClassificationModel
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: Backward, Forward
    ' 
    ' Class Trainer
    ' 
    '     Properties: TrainLossHistory, ValAccuracyHistory
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: Evaluate, TrainEpoch
    ' 
    '     Sub: Train
    ' 
    ' Class GraphClassificationTrainer
    ' 
    '     Properties: TrainLossHistory, ValAccuracyHistory
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: Evaluate, TrainEpoch
    ' 
    '     Sub: Train
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports std = System.Math


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
