#Region "Microsoft.VisualBasic::fef3bf50f780a9af8bddef9b3f2f1c0c, Data_science\MachineLearning\GNN\Layers\LinearLayer.vb"

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

    '   Total Lines: 154
    '    Code Lines: 68 (44.16%)
    ' Comment Lines: 60 (38.96%)
    '    - Xml Docs: 80.00%
    ' 
    '   Blank Lines: 26 (16.88%)
    '     File Size: 4.57 KB


    ' Class LinearLayer
    ' 
    '     Properties: InFeatures, OutFeatures, UseBias
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: Backward, Forward, GetBias, GetGradients, GetParameters
    '               GetWeights
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MachineLearning.TensorFlow

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
