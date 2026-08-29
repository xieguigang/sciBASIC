
Imports Microsoft.VisualBasic.MachineLearning.TensorFlow

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
