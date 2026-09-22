#Region "Microsoft.VisualBasic::9aba2b9cfa36bc705448f5f727101971, Data_science\MachineLearning\DeepLearning\CNN\Layers\losslayers\SoftMaxLayer.vb"

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

    '   Total Lines: 108
    '    Code Lines: 69 (63.89%)
    ' Comment Lines: 17 (15.74%)
    '    - Xml Docs: 58.82%
    ' 
    '   Blank Lines: 22 (20.37%)
    '     File Size: 3.39 KB


    '     Class SoftMaxLayer
    ' 
    '         Properties: Type
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: (+2 Overloads) backward, forward, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MachineLearning.CNN.data
Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports std = System.Math

Namespace CNN.losslayers


    ''' <summary>
    ''' This layer will squash the result of the activations in the fully
    ''' connected layer and give you a value of 0 to 1 for all output activations.
    ''' 
    ''' @author Daniel Persson (mailto.woden@gmail.com)
    ''' </summary>
    Public Class SoftMaxLayer : Inherits LossLayer

        Dim es As Double()

        ''' <summary>Gets the kind of this layer, always <see cref="LayerTypes.SoftMax"/>.</summary>
        Public Overrides ReadOnly Property Type As LayerTypes
            Get
                Return LayerTypes.SoftMax
            End Get
        End Property

        ''' <summary>
        ''' Creates the softmax loss layer and flattens the shared output definition to a vector.
        ''' </summary>
        ''' <param name="def">The shared output definition that carries the last layer shape.</param>
        Public Sub New(def As OutputDefinition)
            MyBase.New(def)

            ' computed
            num_inputs = def.outY * def.outX * def.depth
            out_depth = num_inputs
            out_sx = 1
            out_sy = 1

            def.outX = out_sx
            def.outY = out_sy
            def.depth = out_depth
        End Sub

        ''' <summary>Creates an empty layer, used by the deserializer.</summary>
        Sub New()
        End Sub

        ''' <summary>
        ''' Converts the incoming scores into a probability distribution; the softmax is computed on the tensor back end with
        ''' the maximum subtracted for numerical stability.
        ''' </summary>
        ''' <param name="db">The input score vector.</param>
        ''' <param name="training">Ignored; the loss layer behaves the same in both modes.</param>
        ''' <returns>The class probabilities.</returns>
        Public Overrides Function forward(db As DataBlock, training As Boolean) As DataBlock
            Dim A As New DataBlock(1, 1, out_depth, 0.0) With {.trace = Me.ToString}

            in_act = db

            ' softmax 由张量后端完成(其内部会先减去最大值以保证数值稳定)。
            ' 本层的输入输出都是一个 (1, 1, out_depth) 的向量, 因此归约轴是最末轴。
            Dim probs = Tensor.computeKernel.Softmax(db.Value, db.Value.Rank - 1)

            Call Array.Copy(probs.Data, A.w, A.w.Length)

            ' A.w 在本层之内不会再被改写, 反向传播直接引用同一份存储即可, 无需额外拷贝
            Me.es = A.w ' save these for backprop
            out_act = A
            Return out_act
        End Function

        ''' <summary>
        ''' Computes the cross entropy loss and its gradient for a classification target.
        ''' </summary>
        ''' <param name="y">Index of the target class.</param>
        ''' <returns>The negative log likelihood of the target class.</returns>
        Public Overrides Function backward(y As Integer) As Double
            Dim x As DataBlock = in_act.clearGradient() ' zero out the gradient of input Vol

            ' 梯度 = es - onehot(y), 走后端的逐元素减法。
            ' 这里的 es 就是 out_act 的值数据（同一份存储），因此统一使用 out_act 的缓存视图，
            ' 以便它与本块其它视图共享同一套设备端缓存失效标记。
            Dim indicator As Tensor = Tensor.Zeros(New Integer() {1, 1, out_depth})
            indicator.Item(0, 0, y) = 1.0

            Dim mul = Tensor.computeKernel.Subtract(out_act.Value, indicator)

            Call x.setGradient(mul.Data)

            ' loss is the class negative log likelihood
            Return -std.Log(es(y))
        End Function

        ''' <summary>
        ''' Computes the cross entropy loss and its gradient against a soft target distribution.
        ''' </summary>
        ''' <param name="y">The target probability distribution.</param>
        ''' <returns>The per element loss vector.</returns>
        Public Overrides Function backward(y() As Double) As Double()
            Dim x As DataBlock = in_act.clearGradient
            ' -(y-es) = es - y
            Dim mul = Tensor.computeKernel.Subtract(out_act.Value, Tensor.Wrap(y, 1, 1, out_depth))

            Call x.setGradient(mul.Data)
            Return New Vector(es).Log * -1
        End Function

        ''' <summary>Returns a short description of this layer.</summary>
        ''' <returns>The constant text <c>softmax()</c>.</returns>
        Public Overrides Function ToString() As String
            Return "softmax()"
        End Function
    End Class
End Namespace
