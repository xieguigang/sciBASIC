#Region "Microsoft.VisualBasic::4781baf8d1c0e4246eb5cd33fca020fc, Data_science\MachineLearning\DeepLearning\CNN\Layers\ConvolutionLayer.vb"

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

    '   Total Lines: 217
    '    Code Lines: 156 (71.89%)
    ' Comment Lines: 20 (9.22%)
    '    - Xml Docs: 30.00%
    ' 
    '   Blank Lines: 41 (18.89%)
    '     File Size: 8.76 KB


    '     Class ConvolutionLayer
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
Imports Microsoft.VisualBasic.Parallel
Imports std = System.Math

Namespace CNN.layers

    ''' <summary>
    ''' This layer uses different filters to find attributes of the data that
    ''' affects the result. As an example there could be a filter to find
    ''' horizontal edges in an image.
    ''' 
    ''' @author Daniel Persson (mailto.woden@gmail.com)
    ''' </summary>
    Public Class ConvolutionLayer : Inherits DataLink
        Implements Layer

        Friend l1_decay_mul As Double = 0.0
        Friend l2_decay_mul As Double = 1.0
        Friend BIAS_PREF As Single = 0.1F
        Friend out_depth, out_sx, out_sy As Integer
        Friend in_depth, in_sx, in_sy As Integer
        Friend sx, sy As Integer
        Friend stride, padding As Integer
        Friend filters As DataBlock()
        Friend biases As DataBlock

        Public Overridable ReadOnly Iterator Property BackPropagationResult As IEnumerable(Of BackPropResult) Implements Layer.BackPropagationResult
            Get
                For i As Integer = 0 To out_depth - 1
                    Yield New BackPropResult(filters(i).Weights, filters(i).Gradients, l2_decay_mul, l1_decay_mul)
                Next

                Yield New BackPropResult(biases.Weights, biases.Gradients, 0.0, 0.0)
            End Get
        End Property

        Public ReadOnly Property Type As LayerTypes Implements Layer.Type
            Get
                Return LayerTypes.Convolution
            End Get
        End Property

        Sub New()
        End Sub

        Public Sub New(def As OutputDefinition, sx As Integer, filters As Integer,
                       Optional stride As Integer = 1,
                       Optional padding As Integer = 0)

            ' required
            Me.out_depth = filters
            Me.sx = sx ' filter size. Should be odd if possible, it's cleaner.
            in_depth = def.depth
            in_sx = def.outX
            in_sy = def.outY

            ' optional
            Me.sy = Me.sx
            Me.stride = stride
            Me.padding = padding

            ' computed
            ' note we are doing floor, so if the strided convolution of the filter doesnt fit into the input
            ' volume exactly, the output volume will be trimmed and not contain the (incomplete) computed
            ' final application.
            out_sx = CInt(std.Floor((in_sx + Me.padding * 2 - Me.sx) / Me.stride + 1))
            out_sy = CInt(std.Floor((in_sy + Me.padding * 2 - sy) / Me.stride + 1))

            ' initializations
            Me.filters = New DataBlock(out_depth - 1) {}

            For i As Integer = 0 To out_depth - 1
                Me.filters(i) = New DataBlock(Me.sx, sy, in_depth)
            Next

            biases = New DataBlock(1, 1, out_depth, BIAS_PREF)

            def.outX = out_sx
            def.outY = out_sy
            def.depth = out_depth
        End Sub

        ''' <summary>
        ''' 最近一次前向传播时按后端布局打包好的卷积核张量, 形状 (KH, KW, C, OutC)。
        ''' </summary>
        ''' <remarks>
        ''' 反向传播需要把它作为 <c>Conv2DBackwardInput</c> 的输入(计算对输入的梯度要用到前向的卷积核),
        ''' 所以在每次前向时缓存下来; 由于卷积核在训练过程中一直被就地更新, 这里每次都重新打包,
        ''' 因此不存在缓存过期的问题。
        ''' </remarks>
        <IgnoreDataMember>
        Private filtersPacked As Tensor

        Public Overridable Function forward(db As DataBlock, training As Boolean) As DataBlock Implements Layer.forward
            Dim lA As New DataBlock(out_sx, out_sy, out_depth, 0.0) With {.trace = Me.ToString}

            in_act = db
            out_act = lA

            ' 后端张量的布局约定是 (N, H, W, C), 与 DataBlock 的 (SY, SX, Depth) 在内存里同序,
            ' 因此输入只需要一次零拷贝的形状重解释, 算完把结果整块拷回即可
            Dim x4 As Tensor = Tensor.Wrap(db.Value, db.TensorShape4D)
            Dim packed As Tensor = PackFilters()
            Dim y = Tensor.computeKernel.Conv2D(x4, packed, Tensor.Wrap(biases.w, out_depth), stride, padding)

            Call Array.Copy(y.Data, lA.w, lA.w.Length)

            filtersPacked = packed
            Return out_act
        End Function

        ''' <summary>
        ''' 把各个输出通道的卷积核打包成一个 (KH, KW, C, OutC) 的后端张量。
        ''' </summary>
        ''' <remarks>
        ''' 每个 <see cref="filters"/>(d) 的内部布局是 (KH, KW, C), 而后端要求输出通道作为**最末轴**,
        ''' 因此需要一次带跨步的重新排布, 不能简单地整块拷贝。
        ''' </remarks>
        Private Function PackFilters() As Tensor
            Dim inDepth = in_depth
            Dim OutC = out_depth
            Dim dst As Tensor = Tensor.Zeros(New Integer() {sy, sx, inDepth, OutC})
            Dim w = dst.Data

            For d As Integer = 0 To OutC - 1
                Dim src = filters(d).w

                For kh As Integer = 0 To sy - 1
                    For kw As Integer = 0 To sx - 1
                        For c As Integer = 0 To inDepth - 1
                            w(((kh * sx + kw) * inDepth + c) * OutC + d) = src((sx * kh + kw) * inDepth + c)
                        Next
                    Next
                Next
            Next

            Return dst
        End Function

        ''' <summary>
        ''' 把后端算出的 (KH, KW, C, OutC) 卷积核梯度**累加**回各个 <see cref="filters"/>(d) 的 dw。
        ''' </summary>
        ''' <remarks>
        ''' 这里必须累加而不是直接赋值: 参数梯度是在一个 mini-batch 之内跨样本累加的
        ''' (``TrainerAlgorithm.train`` 每 ``batch_size`` 个样本才调用一次 ``adjustWeights``,
        ''' 且只有在那里才把 ``g(j)`` 清零), 如果直接赋值就会丢掉同一批内前面样本的贡献,
        ''' 等效学习率被缩小 batch_size 倍。
        ''' </remarks>
        Private Sub UnpackFilterGradients(packedGrad As Tensor)
            Dim inDepth = in_depth
            Dim OutC = out_depth
            Dim src = packedGrad.Data

            For d As Integer = 0 To OutC - 1
                Dim dst = filters(d).dw

                For kh As Integer = 0 To sy - 1
                    For kw As Integer = 0 To sx - 1
                        For c As Integer = 0 To inDepth - 1
                            dst((sx * kh + kw) * inDepth + c) += src(((kh * sx + kw) * inDepth + c) * OutC + d)
                        Next
                    Next
                Next
            Next
        End Sub

        Public Overridable Sub backward() Implements Layer.backward
            ' zero out gradient wrt bottom data, we're about to fill it
            Dim db As DataBlock = in_act.clearGradient()

            Dim x4 As Tensor = Tensor.Wrap(db.Value, db.TensorShape4D)
            Dim gradOut As Tensor = Tensor.Wrap(out_act.Grad, out_act.TensorShape4D)

            If filtersPacked Is Nothing Then
                ' 正常情况下 backward 总是紧跟在本层自己的 forward 之后; 这里只是兜底, 避免空引用
                filtersPacked = PackFilters()
            End If

            ' 1) 对卷积核的梯度: 后端按 im2col 语义直接给出 (KH, KW, C, OutC), 再拆回各个 filters(d)
            Dim gradFilters = Tensor.computeKernel.Conv2DBackwardFilter(
                gradOut, x4, New Integer() {sy, sx, in_depth, out_depth}, stride, padding)

            Call UnpackFilterGradients(gradFilters)

            ' 2) 对偏置的梯度: 沿 N/OH/OW 求和得到 (OutC)。
            '    与卷积核一样属于参数梯度, 必须跨样本累加到 mini-batch 结束
            Dim gradBias = Tensor.computeKernel.Conv2DBackwardBias(gradOut)
            Dim biasGrad = gradBias.Data

            For i As Integer = 0 To biases.dw.Length - 1
                biases.dw(i) += biasGrad(i)
            Next

            ' 3) 对输入的梯度: 用前向缓存下来的卷积核算出, 布局与 DataBlock 同序, 整块拷贝
            '
            ' 原先这一步是在按输出通道并行的循环里对本层的输入块做 addGradient, 而不同输出通道
            ' 会落在同一批滤波窗口上, 因此属于跨线程的读-改-写, 存在丢失更新(实测同一个随机种子
            ' 多次运行 loss 从 0.37 漂到 0.75)。改为走后端算子之后, 累加在算子内部按位置唯一完成,
            ' 既没有数据竞争也不再需要按线程切分。
            Dim gradInput = Tensor.computeKernel.Conv2DBackwardInput(
                gradOut, filtersPacked, db.TensorShape4D, stride, padding)

            Call Array.Copy(gradInput.Data, db.dw, db.dw.Length)
        End Sub

        Public Overrides Function ToString() As String
            Return "conv()"
        End Function
    End Class

End Namespace
