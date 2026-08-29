Imports Microsoft.VisualBasic.MachineLearning.TensorFlow

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