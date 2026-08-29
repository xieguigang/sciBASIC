#Region "Microsoft.VisualBasic::87ab5ced43aac5d1bb21595c4d2b4a11, Data_science\MachineLearning\GNN\Layers.vb"

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

    '   Total Lines: 687
    '    Code Lines: 381 (55.46%)
    ' Comment Lines: 195 (28.38%)
    '    - Xml Docs: 76.92%
    ' 
    '   Blank Lines: 111 (16.16%)
    '     File Size: 23.02 KB


    ' Class Layer
    ' 
    '     Properties: IsTraining, Name
    ' 
    ' Class LinearLayer
    ' 
    '     Properties: InFeatures, OutFeatures, UseBias
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: Backward, Forward, GetBias, GetGradients, GetParameters
    '               GetWeights
    ' 
    ' Class ActivationLayer
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: Backward, Forward, GetGradients, GetParameters
    ' 
    ' Class DropoutLayer
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: Backward, Forward, GetGradients, GetParameters
    ' 
    ' Class GCNConvLayer
    ' 
    '     Properties: InFeatures, OutFeatures
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: (+2 Overloads) Backward, (+3 Overloads) Forward, GetGradients, GetParameters
    ' 
    ' Class GATLayer
    ' 
    '     Properties: InFeatures, OutFeatures
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: Backward, (+2 Overloads) Forward, GetGradients, GetParameters, LeakyReLU
    ' 
    ' Class GlobalPoolingLayer
    ' 
    ' 
    '     Enum PoolingType
    ' 
    ' 
    ' 
    ' 
    '  
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: Backward, Forward, GetGradients, GetParameters, MaxPooling
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports std = System.Math

''' <summary>
''' 神经网络层基类
''' 定义了所有层必须实现的接口
''' </summary>
Public MustInherit Class Layer
    ''' <summary>
    ''' 层的名称
    ''' </summary>
    Private _Name As String

    Public Property Name As String
        Get
            Return _Name
        End Get
        Protected Set(value As String)
            _Name = value
        End Set
    End Property

    ''' <summary>
    ''' 层是否处于训练模式
    ''' </summary>
    Public Property IsTraining As Boolean = True

    ''' <summary>
    ''' 前向传播
    ''' </summary>
    Public MustOverride Function Forward(input As Tensor) As Tensor

    ''' <summary>
    ''' 反向传播
    ''' </summary>
    Public MustOverride Function Backward(gradient As Tensor) As Tensor

    ''' <summary>
    ''' 获取所有可训练参数
    ''' </summary>
    Public MustOverride Function GetParameters() As List(Of Tensor)

    ''' <summary>
    ''' 获取所有参数的梯度
    ''' </summary>
    Public MustOverride Function GetGradients() As List(Of Tensor)
End Class

''' <summary>
''' 线性层（全连接层）
''' 实现 y = x * W^T + b
''' 这是神经网络最基本的构建块
''' </summary>
Public Class LinearLayer
    Inherits Layer
    ''' <summary>
    ''' 权重矩阵 [outFeatures, inFeatures]
    ''' </summary>
    Private _weights As Tensor

    ''' <summary>
    ''' 偏置向量 [outFeatures]
    ''' </summary>
    Private _bias As Tensor

    ''' <summary>
    ''' 权重的梯度
    ''' </summary>
    Private _weightGradient As Tensor

    ''' <summary>
    ''' 偏置的梯度
    ''' </summary>
    Private _biasGradient As Tensor

    ''' <summary>
    ''' 保存前向传播的输入，用于反向传播
    ''' </summary>
    Private _lastInput As Tensor

    ''' <summary>
    ''' 输入特征维度
    ''' </summary>
    Public ReadOnly Property InFeatures As Integer

    ''' <summary>
    ''' 输出特征维度
    ''' </summary>
    Public ReadOnly Property OutFeatures As Integer

    ''' <summary>
    ''' 是否使用偏置
    ''' </summary>
    Public ReadOnly Property UseBias As Boolean

    ''' <summary>
    ''' 创建线性层
    ''' </summary>
    ''' <param name="inFeatures">输入特征维度</param>
    ''' <param name="outFeatures">输出特征维度</param>
    ''' <param name="useBias">是否使用偏置</param>
    ''' <param name="name">层名称</param>
    Public Sub New(inFeatures As Integer, outFeatures As Integer, Optional useBias As Boolean = True, Optional name As String = Nothing)
        Me.InFeatures = inFeatures
        Me.OutFeatures = outFeatures
        Me.UseBias = useBias
        MyBase.Name = If(name, $"Linear_{inFeatures}_{outFeatures}")

        ' 使用Xavier初始化权重
        _weights = Tensor.XavierInit(inFeatures, outFeatures)

        If useBias Then
            _bias = New Tensor(1, outFeatures)
        Else
            _bias = New Tensor(0) ' 空张量
        End If

        ' 初始化梯度存储
        _weightGradient = New Tensor(outFeatures, inFeatures)
        _biasGradient = If(useBias, New Tensor(1, outFeatures), New Tensor(0))
    End Sub

    ''' <summary>
    ''' 前向传播: y = x * W^T + b
    ''' </summary>
    Public Overrides Function Forward(input As Tensor) As Tensor
        _lastInput = input

        ' input: [batchSize, inFeatures]
        ' weights: [outFeatures, inFeatures]
        ' output: [batchSize, outFeatures]

        ' 计算 x * W^T
        Dim weightT = _weights.Transpose() ' [inFeatures, outFeatures]
        Dim output = input.MatMul(weightT)

        ' 加上偏置
        If UseBias Then
            For i = 0 To output.Shape(0) - 1
                For j = 0 To output.Shape(1) - 1
                    output(i, j) += _bias(0, j)
                Next
            Next
        End If

        Return output
    End Function

    ''' <summary>
    ''' 反向传播
    ''' </summary>
    Public Overrides Function Backward(gradient As Tensor) As Tensor
        ' gradient: [batchSize, outFeatures]
        ' _lastInput: [batchSize, inFeatures]

        ' 计算权重梯度: dW = gradient^T * input
        Dim gradientT = gradient.Transpose() ' [outFeatures, batchSize]
        _weightGradient = gradientT.MatMul(_lastInput) ' [outFeatures, inFeatures]

        ' 计算偏置梯度: db = sum(gradient, axis=0)
        If UseBias Then
            _biasGradient = gradient.Sum(0) ' [1, outFeatures]
        End If

        ' 计算输入梯度: dx = gradient * W
        Dim inputGradient = gradient.MatMul(_weights) ' [batchSize, inFeatures]

        Return inputGradient
    End Function

    Public Overrides Function GetParameters() As List(Of Tensor)
        Dim params = New List(Of Tensor) From {
                _weights
            }
        If UseBias Then params.Add(_bias)
        Return params
    End Function

    Public Overrides Function GetGradients() As List(Of Tensor)
        Dim grads = New List(Of Tensor) From {
                _weightGradient
            }
        If UseBias Then grads.Add(_biasGradient)
        Return grads
    End Function

    ''' <summary>
    ''' 获取权重矩阵
    ''' </summary>
    Public Function GetWeights() As Tensor
        Return _weights
    End Function

    ''' <summary>
    ''' 获取偏置向量
    ''' </summary>
    Public Function GetBias() As Tensor
        Return _bias
    End Function
End Class

''' <summary>
''' 激活层
''' 对输入应用非线性激活函数
''' </summary>
Public Class ActivationLayer
    Inherits Layer
    Private ReadOnly _activationType As ActivationType
    Private _lastInput As Tensor

    Public Sub New(type As ActivationType, Optional name As String = Nothing)
        _activationType = type
        MyBase.Name = If(name, $"Activation_{type}")
    End Sub

    Public Overrides Function Forward(input As Tensor) As Tensor
        _lastInput = input
        Return Apply(input, _activationType)
    End Function

    Public Overrides Function Backward(gradient As Tensor) As Tensor
        Dim activationDerivative = Derivative(_lastInput, _activationType)
        Return gradient.ElementwiseMultiply(activationDerivative)
    End Function

    Public Overrides Function GetParameters() As List(Of Tensor)
        Return New List(Of Tensor)()
    End Function
    Public Overrides Function GetGradients() As List(Of Tensor)
        Return New List(Of Tensor)()
    End Function
End Class

''' <summary>
''' Dropout层
''' 在训练时随机丢弃部分神经元，防止过拟合
''' </summary>
Public Class DropoutLayer
    Inherits Layer
    Private ReadOnly _dropRate As Single
    Private _mask As Tensor
    Private _random As Random

    Public Sub New(Optional dropRate As Single = 0.5F, Optional name As String = Nothing, Optional seed As Integer? = Nothing)
        If dropRate < 0 OrElse dropRate >= 1 Then Throw New ArgumentException("Dropout率必须在[0, 1)范围内")

        _dropRate = dropRate
        _random = If(seed.HasValue, New Random(seed.Value), New Random())
        MyBase.Name = If(name, $"Dropout_{dropRate}")
    End Sub

    Public Overrides Function Forward(input As Tensor) As Tensor
        If Not IsTraining Then
            ' 测试时不进行dropout
            Return input
        End If

        ' 创建dropout掩码
        _mask = New Tensor(input.Shape)
        Dim scale = 1.0F / (1.0F - _dropRate)

        For i = 0 To input.Length - 1
            If _random.NextDouble() >= _dropRate Then
                _mask(i) = scale ' 保留并缩放
            Else
                _mask(i) = 0 ' 丢弃
            End If
        Next

        Return input.ElementwiseMultiply(_mask)
    End Function

    Public Overrides Function Backward(gradient As Tensor) As Tensor
        If Not IsTraining Then Return gradient

        Return gradient.ElementwiseMultiply(_mask)
    End Function

    Public Overrides Function GetParameters() As List(Of Tensor)
        Return New List(Of Tensor)()
    End Function
    Public Overrides Function GetGradients() As List(Of Tensor)
        Return New List(Of Tensor)()
    End Function
End Class

''' <summary>
''' GCN卷积层 (Graph Convolutional Layer)
''' 实现图卷积操作: H' = σ(A_norm * H * W)
''' 其中 A_norm 是归一化的邻接矩阵，H 是节点特征，W 是可学习权重
''' 
''' 这是Kipf & Welling提出的经典GCN层的实现
''' 论文: Semi-Supervised Classification with Graph Convolutional Networks (ICLR 2017)
''' </summary>
Public Class GCNConvLayer
    Inherits Layer
    ''' <summary>
    ''' 线性变换层
    ''' </summary>
    Private ReadOnly _linear As LinearLayer

    ''' <summary>
    ''' 激活函数类型
    ''' </summary>
    Private ReadOnly _activation As ActivationType

    ''' <summary>
    ''' 保存的归一化邻接矩阵
    ''' </summary>
    Private _normAdj As Tensor

    ''' <summary>
    ''' 中间结果，用于反向传播
    ''' </summary>
    Private _aggregated As Tensor
    Private _transformed As Tensor
    Private _lastInput As Tensor

    ''' <summary>
    ''' 输入特征维度
    ''' </summary>
    Public ReadOnly Property InFeatures As Integer
        Get
            Return _linear.InFeatures
        End Get
    End Property

    ''' <summary>
    ''' 输出特征维度
    ''' </summary>
    Public ReadOnly Property OutFeatures As Integer
        Get
            Return _linear.OutFeatures
        End Get
    End Property

    ''' <summary>
    ''' 创建GCN卷积层
    ''' </summary>
    ''' <param name="inFeatures">输入特征维度</param>
    ''' <param name="outFeatures">输出特征维度</param>
    ''' <param name="activation">激活函数类型</param>
    ''' <param name="useBias">是否使用偏置</param>
    ''' <param name="name">层名称</param>
    Public Sub New(inFeatures As Integer, outFeatures As Integer, Optional activation As ActivationType = ActivationType.ReLU, Optional useBias As Boolean = True, Optional name As String = Nothing)

        _linear = New LinearLayer(inFeatures, outFeatures, useBias)
        _activation = activation
        MyBase.Name = If(name, $"GCNConv_{inFeatures}_{outFeatures}")
    End Sub

    ''' <summary>
    ''' 前向传播
    ''' </summary>
    ''' <param name="input">节点特征矩阵 [numNodes, inFeatures]</param>
    ''' <param name="graph">图结构（用于获取邻接矩阵）</param>
    ''' <returns>更新后的节点特征 [numNodes, outFeatures]</returns>
    Public Overloads Function Forward(input As Tensor, graph As Graph) As Tensor
        _lastInput = input
        _normAdj = graph.GetNormalizedAdjacencyMatrix()

        ' 步骤1: 聚合邻居信息
        ' aggregated = A_norm * H
        _aggregated = _normAdj.MatMul(input)

        ' 步骤2: 特征变换
        ' transformed = aggregated * W + b
        _transformed = _linear.Forward(_aggregated)

        ' 步骤3: 应用激活函数
        Dim output = Apply(_transformed, _activation)

        Return output
    End Function

    ''' <summary>
    ''' 前向传播（仅使用预计算的邻接矩阵）
    ''' </summary>
    Public Overloads Function Forward(input As Tensor, normalizedAdjacency As Tensor) As Tensor
        _lastInput = input
        _normAdj = normalizedAdjacency

        _aggregated = _normAdj.MatMul(input)
        _transformed = _linear.Forward(_aggregated)
        Dim output = Apply(_transformed, _activation)

        Return output
    End Function

    ''' <summary>
    ''' 反向传播
    ''' </summary>
    Public Overloads Function Backward(gradient As Tensor, normalizedAdjacency As Tensor) As Tensor
        ' 激活函数的梯度
        Dim activationDerivative = Derivative(_transformed, _activation)
        Dim gradAfterActivation = gradient.ElementwiseMultiply(activationDerivative)

        ' 线性层的梯度
        Dim gradAfterLinear = _linear.Backward(gradAfterActivation)

        ' 邻接矩阵传播的梯度
        ' 因为 A_norm 是对称的，所以梯度传播使用 A_norm^T = A_norm
        Dim inputGradient = normalizedAdjacency.MatMul(gradAfterLinear)

        Return inputGradient
    End Function

    ' 实现抽象方法
    Public Overrides Function Forward(input As Tensor) As Tensor
        Throw New InvalidOperationException("GCN层需要图结构，请使用Forward(input, graph)方法")
    End Function

    Public Overrides Function Backward(gradient As Tensor) As Tensor
        Throw New InvalidOperationException("GCN层需要邻接矩阵，请使用Backward(gradient, normalizedAdjacency)方法")
    End Function

    Public Overrides Function GetParameters() As List(Of Tensor)
        Return _linear.GetParameters()
    End Function
    Public Overrides Function GetGradients() As List(Of Tensor)
        Return _linear.GetGradients()
    End Function
End Class

''' <summary>
''' 图注意力层 (Graph Attention Layer, GAT)
''' 使用注意力机制聚合邻居信息
''' 论文: Graph Attention Networks (Veličković et al., ICLR 2018)
''' 
''' 核心思想：不同的邻居节点对中心节点的重要性不同，
''' 通过学习注意力权重来自适应地聚合邻居信息
''' </summary>
Public Class GATLayer
    Inherits Layer
    Private ReadOnly _numHeads As Integer
    Private ReadOnly _outFeaturesPerHead As Integer
    Private ReadOnly _leakyReluSlope As Single

    ' 可学习参数
    Private _W As Tensor  ' 特征变换权重 [inFeatures, numHeads * outFeaturesPerHead]
    Private _a As Tensor  ' 注意力权重 [numHeads, 2 * outFeaturesPerHead]

    ' 梯度
    Private _wGrad As Tensor
    Private _aGrad As Tensor

    ' 中间结果
    Private _lastInput As Tensor
    Private _transformedFeatures As Tensor
    Private _attentionWeights As Tensor
    Private _lastGraph As Graph

    Public ReadOnly Property InFeatures As Integer
    Public ReadOnly Property OutFeatures As Integer
        Get
            Return _numHeads * _outFeaturesPerHead
        End Get
    End Property

    ''' <summary>
    ''' 创建图注意力层
    ''' </summary>
    ''' <param name="inFeatures">输入特征维度</param>
    ''' <param name="outFeatures">输出特征维度（每个头）</param>
    ''' <param name="numHeads">注意力头数量</param>
    ''' <param name="leakyReluSlope">LeakyReLU的负斜率</param>
    Public Sub New(inFeatures As Integer, outFeatures As Integer, Optional numHeads As Integer = 1, Optional leakyReluSlope As Single = 0.2F, Optional name As String = Nothing)
        Me.InFeatures = inFeatures
        _outFeaturesPerHead = outFeatures
        _numHeads = numHeads
        _leakyReluSlope = leakyReluSlope
        MyBase.Name = If(name, $"GAT_{inFeatures}_{outFeatures}_heads{numHeads}")

        ' 初始化权重
        _W = Tensor.XavierInit(inFeatures, numHeads * outFeatures)
        _a = Tensor.RandomNormal(New Integer() {numHeads, 2 * outFeatures}, 0, 0.1F)

        ' 初始化梯度
        _wGrad = New Tensor(inFeatures, numHeads * outFeatures)
        _aGrad = New Tensor(numHeads, 2 * outFeatures)
    End Sub

    ''' <summary>
    ''' 前向传播
    ''' </summary>
    Public Overloads Function Forward(input As Tensor, graph As Graph) As Tensor
        _lastInput = input
        _lastGraph = graph

        Dim numNodes = input.Shape(0)

        ' 步骤1: 特征变换
        ' H' = H * W, shape: [numNodes, numHeads * outFeatures]
        _transformedFeatures = input.MatMul(_W)

        ' 步骤2: 计算注意力系数
        ' 对于每条边，计算注意力权重
        _attentionWeights = New Tensor(numNodes, numNodes, _numHeads)

        For h = 0 To _numHeads - 1
            Dim headOffset = h * _outFeaturesPerHead

            For Each edge In graph.Edges
                Dim i = edge.Source
                Dim j = edge.Target

                ' 计算注意力分数 e_ij = LeakyReLU(a^T [Wh_i || Wh_j])
                Dim score As Single = 0
                For k = 0 To _outFeaturesPerHead - 1
                    score += _a(h, k) * _transformedFeatures(i, headOffset + k)
                    score += _a(h, _outFeaturesPerHead + k) * _transformedFeatures(j, headOffset + k)
                Next
                score = LeakyReLU(score, _leakyReluSlope)

                _attentionWeights(i, j, h) = score
            Next
        Next

        ' 步骤3: Softmax归一化（对每个节点的所有入边）
        ' α_ij = softmax_j(e_ij)
        Dim normalizedAttention = New Tensor(numNodes, numNodes, _numHeads)

        For h = 0 To _numHeads - 1
            For i = 0 To numNodes - 1
                ' 找到所有邻居并计算softmax
                Dim neighbors = graph.GetInNeighbors(i)
                If neighbors.Count = 0 Then Continue For

                Dim maxScore = Single.MinValue
                For Each j In neighbors
                    If _attentionWeights(j, i, h) > maxScore Then maxScore = _attentionWeights(j, i, h)
                Next

                Dim sumExp As Single = 0
                For Each j In neighbors
                    sumExp += CSng(std.Exp(_attentionWeights(j, i, h) - maxScore))
                Next

                For Each j In neighbors
                    Dim expScore As Single = std.Exp(_attentionWeights(j, i, h) - maxScore)
                    normalizedAttention(j, i, h) = expScore / sumExp
                Next
            Next
        Next

        ' 步骤4: 加权聚合
        ' h'_i = Σ_j α_ij * Wh_j
        Dim output = New Tensor(numNodes, _numHeads * _outFeaturesPerHead)

        For h = 0 To _numHeads - 1
            Dim headOffset = h * _outFeaturesPerHead

            For i = 0 To numNodes - 1
                Dim neighbors = graph.GetInNeighbors(i)
                For Each j In neighbors
                    Dim alpha = normalizedAttention(j, i, h)
                    For k = 0 To _outFeaturesPerHead - 1
                        output(i, headOffset + k) += alpha * _transformedFeatures(j, headOffset + k)
                    Next
                Next
            Next
        Next

        Return output
    End Function

    Private Function LeakyReLU(x As Single, slope As Single) As Single
        Return If(x > 0, x, slope * x)
    End Function

    Public Overrides Function Forward(input As Tensor) As Tensor
        Throw New InvalidOperationException("GAT层需要图结构，请使用Forward(input, graph)方法")
    End Function

    Public Overrides Function Backward(gradient As Tensor) As Tensor
        Throw New InvalidOperationException("GAT层需要图结构")
    End Function

    Public Overrides Function GetParameters() As List(Of Tensor)
        Return New List(Of Tensor) From {
            _W,
            _a
        }
    End Function
    Public Overrides Function GetGradients() As List(Of Tensor)
        Return New List(Of Tensor) From {
            _wGrad,
            _aGrad
        }
    End Function
End Class

''' <summary>
''' 全局池化层
''' 将节点特征聚合为图级别特征
''' 用于图分类任务
''' </summary>
Public Class GlobalPoolingLayer
    Inherits Layer
    Public Enum PoolingType
        Sum    ' 求和池化
        Mean   ' 平均池化
        Max     ' 最大池化
    End Enum

    Private ReadOnly _poolingType As PoolingType
    Private _lastInput As Tensor

    Public Sub New(Optional type As PoolingType = PoolingType.Mean, Optional name As String = Nothing)
        _poolingType = type
        MyBase.Name = If(name, $"GlobalPooling_{type}")
    End Sub

    ''' <summary>
    ''' 前向传播：将节点特征聚合为图级别特征
    ''' </summary>
    ''' <param name="input">节点特征 [numNodes, features]</param>
    ''' <returns>图特征 [1, features]</returns>
    Public Overrides Function Forward(input As Tensor) As Tensor
        _lastInput = input

        Select Case _poolingType
            Case PoolingType.Sum : Return input.Sum(0)
            Case PoolingType.Mean : Return input.Mean(0)
            Case PoolingType.Max : Return MaxPooling(input)
            Case Else
                Throw New ArgumentException($"未知的池化类型: {_poolingType}")
        End Select
    End Function

    Private Function MaxPooling(input As Tensor) As Tensor
        Dim result = New Tensor(1, input.Shape(1))
        For j = 0 To input.Shape(1) - 1
            Dim maxVal = Single.MinValue
            For i = 0 To input.Shape(0) - 1
                If input(i, j) > maxVal Then maxVal = input(i, j)
            Next
            result(0, j) = maxVal
        Next
        Return result
    End Function

    Public Overrides Function Backward(gradient As Tensor) As Tensor
        ' 将梯度广播回所有节点
        Dim inputGradient = New Tensor(_lastInput.Shape)

        Dim numNodes = _lastInput.Shape(0)

        If _poolingType = PoolingType.Sum Then
            ' 求和池化：每个节点获得相同的梯度
            For i = 0 To numNodes - 1
                For j = 0 To gradient.Shape(1) - 1
                    inputGradient(i, j) = gradient(0, j)
                Next
            Next
        ElseIf _poolingType = PoolingType.Mean Then
            ' 平均池化：梯度除以节点数
            For i = 0 To numNodes - 1
                For j = 0 To gradient.Shape(1) - 1
                    inputGradient(i, j) = gradient(0, j) / numNodes
                Next
            Next
        ElseIf _poolingType = PoolingType.Max Then
            ' 最大池化：只有最大值位置获得梯度
            For j = 0 To gradient.Shape(1) - 1
                Dim maxVal = Single.MinValue
                Dim maxIdx = 0
                For i = 0 To numNodes - 1
                    If _lastInput(i, j) > maxVal Then
                        maxVal = _lastInput(i, j)
                        maxIdx = i
                    End If
                Next
                inputGradient(maxIdx, j) = gradient(0, j)
            Next
        End If

        Return inputGradient
    End Function

    Public Overrides Function GetParameters() As List(Of Tensor)
        Return New List(Of Tensor)()
    End Function
    Public Overrides Function GetGradients() As List(Of Tensor)
        Return New List(Of Tensor)()
    End Function
End Class
