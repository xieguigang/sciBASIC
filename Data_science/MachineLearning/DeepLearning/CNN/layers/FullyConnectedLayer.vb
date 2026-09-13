#Region "Microsoft.VisualBasic::9dc9fd9d9cf5d36f692a3977c2a36949, Data_science\MachineLearning\DeepLearning\CNN\Layers\FullyConnectedLayer.vb"

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

    '   Total Lines: 145
    '    Code Lines: 99 (68.28%)
    ' Comment Lines: 11 (7.59%)
    '    - Xml Docs: 63.64%
    ' 
    '   Blank Lines: 35 (24.14%)
    '     File Size: 5.08 KB


    '     Class FullyConnectedLayer
    ' 
    '         Properties: Type
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: forward, ToString
    ' 
    '         Sub: backward
    '         Class ForwardTask
    ' 
    '             Constructor: (+1 Overloads) Sub New
    '             Sub: Solve
    ' 
    '         Class BackwardTask
    ' 
    '             Constructor: (+1 Overloads) Sub New
    '             Sub: Solve
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.Serialization
Imports Microsoft.VisualBasic.MachineLearning.CNN.data
Imports Microsoft.VisualBasic.MachineLearning.TensorFlow

Namespace CNN.layers

    ''' <summary>
    ''' Neurons in a fully connected layer have full connections to all
    ''' activations in the previous layer, as seen in regular Neural Networks.
    ''' Their activations can hence be computed with a matrix multiplication
    ''' followed by a bias offset.
    ''' 
    ''' @author Daniel Persson (mailto.woden@gmail.com)
    ''' </summary>
    Public Class FullyConnectedLayer : Inherits DataLink
        Implements Layer

        Private l1_decay_mul As Double = 0.0
        Private l2_decay_mul As Double = 1.0

        Private ReadOnly BIAS_PREF As Single = 0.1F

        Private out_depth, out_sx, out_sy As Integer
        Private num_inputs As Integer
        Private filters As DataBlock()
        Private biases As DataBlock

        Public Overridable ReadOnly Iterator Property BackPropagationResult As IEnumerable(Of BackPropResult) Implements Layer.BackPropagationResult
            Get
                For i As Integer = 0 To out_depth - 1
                    Dim block = filters(i)

                    Yield New BackPropResult(block.Weights, block.Gradients, l1_decay_mul, l2_decay_mul,
                                             Sub()
                                                 block.MarkValueModified()
                                                 block.MarkGradientModified()
                                             End Sub)
                Next

                Yield New BackPropResult(biases.Weights, biases.Gradients, 0.0, 0.0,
                                         Sub()
                                             biases.MarkValueModified()
                                             biases.MarkGradientModified()
                                         End Sub)
            End Get
        End Property

        Public ReadOnly Property Type As LayerTypes Implements Layer.Type
            Get
                Return LayerTypes.FullyConnected
            End Get
        End Property

        Sub New()
        End Sub

        Public Sub New(def As OutputDefinition, num_neurons As Integer)
            out_depth = num_neurons

            ' computed
            num_inputs = def.outX * def.outY * def.depth
            out_sx = 1
            out_sy = 1

            ' initializations
            filters = New DataBlock(out_depth - 1) {}

            For i As Integer = 0 To out_depth - 1
                filters(i) = New DataBlock(1, 1, num_inputs)
            Next

            biases = New DataBlock(1, 1, out_depth, BIAS_PREF)

            def.outX = out_sx
            def.outY = out_sy
            def.depth = out_depth
        End Sub

        ''' <summary>
        ''' 最近一次前向传播时按后端布局打包好的权值矩阵, 形状 (out_depth, num_inputs)。
        ''' </summary>
        ''' <remarks>
        ''' 本层的权值原本是按"每个输出神经元一个 DataBlock"分开存放的, 与后端 <c>MatMul</c> 要求的
        ''' 矩阵形状不匹配; 这里在每次前向时按行拼成一个矩阵, 反向传播也需要它(的转置),
        ''' 所以一并缓存下来。因为权值在训练过程中一直被就地更新, 每次前向都重新打包, 不存在过期问题。
        ''' </remarks>
        <IgnoreDataMember>
        Private weightsPacked As Tensor

        Public Overridable Function forward(db As DataBlock, training As Boolean) As DataBlock Implements Layer.forward
            Dim lA As New DataBlock(1, 1, out_depth, 0.0) With {.trace = Me.ToString}

            in_act = db
            out_act = lA

            Dim packed As Tensor = PackWeights()
            Dim x2 As Tensor = db.Value2D

            ' y = W·x : (out_depth x num_inputs) * (num_inputs x 1) -> (out_depth x 1)
            Dim y = Tensor.computeKernel.MatMul(packed, x2)
            ' 偏置同样当作 (out_depth x 1) 的列向量逐元素相加
            Dim withBias = Tensor.computeKernel.Add(y, biases.Value2D)

            Call lA.SetValues(withBias.Data)

            weightsPacked = packed
            Return out_act
        End Function

        ''' <summary>
        ''' 把各个输出神经元的权值按行拼成一个 (out_depth, num_inputs) 的矩阵。
        ''' </summary>
        ''' <remarks>
        ''' 每个 <see cref="filters"/>(i) 内部是连续的 num_inputs 个权值, 目标矩阵的每一行也是连续的,
        ''' 所以这里可以按行整块拷贝, 不需要逐元素重排。
        ''' </remarks>
        Private Function PackWeights() As Tensor
            Dim dst As Tensor = Tensor.Zeros(New Integer() {out_depth, num_inputs})
            Dim w = dst.Data
            Dim rowSize = num_inputs

            For i As Integer = 0 To out_depth - 1
                Call Array.Copy(filters(i).w, 0, w, i * rowSize, rowSize)
            Next

            Return dst
        End Function

        Public Overridable Sub backward() Implements Layer.backward
            Dim v As DataBlock = in_act.clearGradient()

            If weightsPacked Is Nothing Then
                ' 正常情况下 backward 总是紧跟在本层自己的 forward 之后; 这里只是兜底
                weightsPacked = PackWeights()
            End If

            Dim gradOut2 As Tensor = out_act.Grad2D
            Dim x2 As Tensor = in_act.Value2D

            ' 1) 对权值的梯度: gradW = gradOut · xᵀ -> (out_depth x num_inputs)
            Dim gradW = Tensor.computeKernel.MatMul(gradOut2, Tensor.computeKernel.Transpose(x2))
            Dim gw = gradW.Data

            ' 参数梯度是在一个 mini-batch 之内跨样本累加的(清零发生在 TrainerAlgorithm.adjustWeights),
            ' 因此这里必须累加而不是赋值
            For i As Integer = 0 To out_depth - 1
                Dim dst = filters(i).dw
                Dim ro = i * num_inputs

                For j As Integer = 0 To num_inputs - 1
                    dst(j) += gw(ro + j)
                Next

                Call filters(i).MarkGradientModified()
            Next

            ' 2) 对偏置的梯度: 每个输出神经元的偏置梯度恰好就是它的上游梯度
            For i As Integer = 0 To out_depth - 1
                biases.dw(i) += out_act.dw(i)
            Next

            Call biases.MarkGradientModified()

            ' 3) 对输入的梯度: gradX = Wᵀ · gradOut -> (num_inputs x 1)
            '    输入梯度是每个样本独立消费的(反向一开始就 clearGradient 清零), 因此直接赋值
            Dim gradX = Tensor.computeKernel.MatMul(Tensor.computeKernel.Transpose(weightsPacked), gradOut2)

            Call v.SetGradients(gradX.Data)
        End Sub

        Public Overrides Function ToString() As String
            Return $"full_connected({out_depth})"
        End Function
    End Class

End Namespace
