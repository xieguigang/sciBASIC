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
Imports Microsoft.VisualBasic.MachineLearning.TensorFlow.Compute

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

        ' 使用Xavier初始化权重。
        '
        ' 形状必须是 (outFeatures, inFeatures)：本层的其余代码全部按这个布局写在
        '   * Forward  : weightT = Wᵀ → [inFeatures, outFeatures]，再 x·Wᵀ
        '   * Backward : dW = gᵀ·x → [outFeatures, inFeatures]
        '                dx = g·W  → [batchSize, outFeatures]×[outFeatures, inFeatures]
        '   * _weightGradient = New Tensor(outFeatures, inFeatures)
        ' 原先写的是 XavierInit(inFeatures, outFeatures)，形状与上面三处**互为转置**。
        ' 后果分两种：
        '   * inFeatures ≠ outFeatures：Forward 的矩阵乘直接报"矩阵维度不匹配"
        '     （TGCN 时序演示就是这样挂掉的，报 1 != 16）；
        '   * inFeatures = outFeatures：形状恰好凑得上，不报错，但 W 与 dW 的布局相反，
        '     而 AdamOptimizer 是按扁平下标 param(j) -= lr·grad(j) 更新的，
        '     于是梯度被施加到了转置后的位置上 —— **静默地学错**。
        ' 之所以一直没被发现，是因为此前的测试用例都是方阵（in == out）。
        '
        ' Xavier 的标准差是 sqrt(2/(fanIn+fanOut))，对两个参数对称，
        ' 因此交换实参顺序不改变初始化分布，只是把形状摆正。
        _weights = Tensor.XavierInit(outFeatures, inFeatures)

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
        ' 这是全连接层的主力 GEMM（O(batch·in·out)），规模达标时走单精度
        Dim weightT = _weights.Transpose() ' [inFeatures, outFeatures]
        Dim output = MixedPrecisionOps.MatMulOr(input, weightT)

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
        '
        ' 注意：必须**写回构造函数里分配的持久张量**，不能把新张量赋给字段。
        ' 各训练器（GNN 的 AdamOptimizer、TemporalGNNTrainer 等）是在开始训练**之前**
        ' 用 model.GetGradients() 一次性拿到梯度张量引用的，之后每步只做
        ' param(j) -= lr * grad(j) 的原地读写。
        ' 原来的 `_weightGradient = <新张量>` 会把这份引用换掉，
        ' 优化器此后一直在更新那个再也不会被写入的旧对象 —— 该层静默地停止学习
        '（表现为损失卡住不动，且不报任何错）。
        '
        ' GRULayer / SimpleRNNLayer 用的是 `+=` 原地累加，所以没有这个问题；
        ' 这里改为"算出新值后拷进持久缓冲"，既保留引用，语义又与注释一致（dW = …，不是 dW += …）。
        Dim gradientT = gradient.Transpose() ' [outFeatures, batchSize]
        Dim computed = MixedPrecisionOps.MatMulOr(gradientT, _lastInput) ' [outFeatures, inFeatures]

        Array.Copy(computed.Data, _weightGradient.Data, _weightGradient.Length)
        Call _weightGradient.MarkHostModified()

        ' 计算偏置梯度: db = sum(gradient, axis=0)
        If UseBias Then
            Dim biasComputed = gradient.Sum(0) ' [1, outFeatures]

            Array.Copy(biasComputed.Data, _biasGradient.Data, _biasGradient.Length)
            Call _biasGradient.MarkHostModified()
        End If

        ' 计算输入梯度: dx = gradient * W
        Dim inputGradient = MixedPrecisionOps.MatMulOr(gradient, _weights) ' [batchSize, inFeatures]

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
