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

        Public Overrides ReadOnly Property Type As LayerTypes
            Get
                Return LayerTypes.SoftMax
            End Get
        End Property

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

        Sub New()
        End Sub

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
        ''' compute and accumulate gradient wrt weights and bias of this layer
        ''' </summary>
        ''' <param name="y"></param>
        ''' <returns></returns>
        Public Overrides Function backward(y As Integer) As Double
            Dim x As DataBlock = in_act.clearGradient() ' zero out the gradient of input Vol

            ' 梯度 = es - onehot(y), 走后端的逐元素减法
            Dim indicator As Tensor = Tensor.Zeros(New Integer() {out_depth})
            indicator.Item(y) = 1.0

            Dim mul = Tensor.computeKernel.Subtract(Tensor.Wrap(es, out_depth), indicator)

            Call x.setGradient(mul.Data)

            ' loss is the class negative log likelihood
            Return -std.Log(es(y))
        End Function

        Public Overrides Function backward(y() As Double) As Double()
            Dim x As DataBlock = in_act.clearGradient
            ' -(y-es) = es - y
            Dim mul = Tensor.computeKernel.Subtract(Tensor.Wrap(es, out_depth), Tensor.Wrap(y, out_depth))

            Call x.setGradient(mul.Data)
            Return New Vector(es).Log * -1
        End Function

        Public Overrides Function ToString() As String
            Return "softmax()"
        End Function
    End Class
End Namespace
