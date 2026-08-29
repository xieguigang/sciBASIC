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
