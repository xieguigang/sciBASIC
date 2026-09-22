#Region "Microsoft.VisualBasic::9c6e8554472a92d234a92218c01de7c9, Data_science\MachineLearning\DeepLearning\CNN\Layers\RectifiedLinearUnitsLayer.vb"

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

    '   Total Lines: 115
    '    Code Lines: 60 (52.17%)
    ' Comment Lines: 34 (29.57%)
    '    - Xml Docs: 76.47%
    ' 
    '   Blank Lines: 21 (18.26%)
    '     File Size: 4.75 KB


    '     Class RectifiedLinearUnitsLayer
    ' 
    '         Properties: Type
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: forward, ToString
    ' 
    '         Sub: backward
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MachineLearning.CNN.data
Imports Microsoft.VisualBasic.MachineLearning.TensorFlow

Namespace CNN.layers

    ''' <summary>
    ''' This is a layer of neurons that applies the non-saturating activation
    ''' function f(x)=max(0,x). It increases the nonlinear properties of the
    ''' decision function and of the overall network without affecting the
    ''' receptive fields of the convolution layer.
    ''' 
    ''' @author Daniel Persson (mailto.woden@gmail.com)
    ''' </summary>
    ''' <remarks>
    ''' ReLU
    ''' </remarks>
    Public Class RectifiedLinearUnitsLayer : Inherits DataLink
        Implements Layer

        ''' <summary>Gets the parameter and gradient blocks of this layer; the ReLU layer has none.</summary>
        Public Overridable ReadOnly Iterator Property BackPropagationResult As IEnumerable(Of BackPropResult) Implements Layer.BackPropagationResult
            Get
                ' no data
            End Get
        End Property

        ''' <summary>Gets the kind of this layer, always <see cref="LayerTypes.ReLU"/>.</summary>
        Public Overridable ReadOnly Property Type As LayerTypes Implements Layer.Type
            Get
                Return LayerTypes.ReLU
            End Get
        End Property

        ''' <summary>Values below this threshold are clamped; zero gives the standard ReLU behaviour.</summary>
        Protected threshold As Double = 0.0

        ''' <summary>Creates a rectified linear unit layer.</summary>
        Sub New()
        End Sub

        ''' <summary>
        ''' Applies the rectifier element wise. With the default threshold of zero the whole computation is dispatched to the
        ''' tensor back end.
        ''' </summary>
        ''' <param name="db">The input data block.</param>
        ''' <param name="training">Ignored; the activation behaves the same in both modes.</param>
        ''' <returns>The rectified activations.</returns>
        Public Overridable Function forward(db As DataBlock, training As Boolean) As DataBlock Implements Layer.forward
            in_act = db

            Dim V2 As DataBlock = db.clone()

            If threshold = 0.0 Then
                ' threshold 为 0 时正是标准的 ReLU, 直接交给张量后端的统一算子完成,
                ' 从而能够被 Tensor.computeKernel 派发到 CUDA(无 GPU 时由 CPU/SIMD 后端兜底)
                Dim activated = Tensor.computeKernel.Relu(db.Value)

                Call V2.SetValues(activated.Data)

                out_act = V2
                Return out_act
            End If

            ' threshold 非 0 的截断语义无法用标准 ReLU 表达, 保留原有的标量实现
            Dim N = db.Weights.Length
            Dim V2w = V2.Weights

            For i As Integer = 0 To N - 1
                If V2w(i) < threshold Then
                    V2.setWeight(i, threshold) ' threshold at 0
                End If
            Next

            out_act = V2
            Return out_act
        End Function

        ''' <summary>
        ''' Backpropagates the gradient, keeping it only where the input was positive (a Heaviside mask on the tensor back end).
        ''' </summary>
        Public Overridable Sub backward() Implements Layer.backward
            ' zero out gradient wrt data
            Dim V = in_act.clearGradient() ' we need to set dw of this
            Dim V2 = out_act

            If threshold = 0.0 Then
                ' 反向的掩码 (x > 0) 由后端的 Heaviside 算子给出, 再与上游梯度逐元素相乘;
                ' 两个算子都定义在 computeKernel 上, 因此这一步同样可以整体下放到 GPU
                Dim mask = Tensor.computeKernel.Heaviside(V.Value)
                Dim dx = Tensor.computeKernel.Multiply(V2.Grad, mask)

                Call V.setGradient(dx.Data)
                Return
            End If

            Dim N = V.Weights.Length
            Dim Vw = V.Weights ' 获取前向传播的输入值

            For i As Integer = 0 To N - 1
                If Vw(i) <= 0 Then ' 如果原始输入 <= 0
                    V.setGradient(i, 0.0) ' 则梯度为 0
                Else
                    V.setGradient(i, V2.getGradient(i)) ' 如果原始输入 > 0，则梯度 = 上游传来的梯度
                End If
            Next
        End Sub

        ''' <summary>Returns a short description of this layer.</summary>
        ''' <returns>The constant text <c>ReLU()</c>.</returns>
        Public Overrides Function ToString() As String
            Return $"ReLU()"
        End Function
    End Class

End Namespace
