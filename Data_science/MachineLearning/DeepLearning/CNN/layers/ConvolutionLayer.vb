#Region "Microsoft.VisualBasic::dcebd11c5b16796e57c65b8aafdc535c, Data_science\MachineLearning\DeepLearning\CNN\Layers\ConvolutionLayer.vb"

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

    '   Total Lines: 249
    '    Code Lines: 134 (53.82%)
    ' Comment Lines: 73 (29.32%)
    '    - Xml Docs: 71.23%
    ' 
    '   Blank Lines: 42 (16.87%)
    '     File Size: 11.42 KB


    '     Class ConvolutionLayer
    ' 
    '         Properties: Type
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: forward, PackFilters, ToString
    ' 
    '         Sub: backward, UnpackFilterGradients
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.Serialization
Imports Microsoft.VisualBasic.MachineLearning.CNN.data
Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports Microsoft.VisualBasic.MachineLearning.TensorFlow.Compute
Imports Microsoft.VisualBasic.Parallel
Imports tpl = System.Threading.Tasks
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

        ''' <summary>Gets the filter and bias parameter blocks of this layer.</summary>
        Public Overridable ReadOnly Iterator Property BackPropagationResult As IEnumerable(Of BackPropResult) Implements Layer.BackPropagationResult
            Get
                For i As Integer = 0 To out_depth - 1
                    Dim block = filters(i)

                    Yield New BackPropResult(block.Weights, block.Gradients, l2_decay_mul, l1_decay_mul,
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

        ''' <summary>Gets the kind of this layer, always <see cref="LayerTypes.Convolution"/>.</summary>
        Public ReadOnly Property Type As LayerTypes Implements Layer.Type
            Get
                Return LayerTypes.Convolution
            End Get
        End Property

        ''' <summary>Creates an empty layer, used by the deserializer.</summary>
        Sub New()
        End Sub

        ''' <summary>
        ''' Creates a convolution layer and derives the output size from the input size, the filter size and the padding.
        ''' </summary>
        ''' <param name="def">The shared output definition that carries the input size.</param>
        ''' <param name="sx">Side length of the square filter window.</param>
        ''' <param name="filters">Number of filters, i.e. the number of output channels.</param>
        ''' <param name="stride">Sliding stride of the filter window.</param>
        ''' <param name="padding">Zero padding applied around the input.</param>
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

        ''' <summary>
        ''' Runs the convolution of the input with every filter and adds the per filter bias.
        ''' </summary>
        ''' <param name="db">The input data block.</param>
        ''' <param name="training">Ignored; the convolution layer behaves the same in both modes.</param>
        ''' <returns>The output feature maps.</returns>
        Public Overridable Function forward(db As DataBlock, training As Boolean) As DataBlock Implements Layer.forward
            Dim lA As New DataBlock(out_sx, out_sy, out_depth, 0.0) With {.trace = Me.ToString}

            in_act = db
            out_act = lA

            ' 后端张量的布局约定是 (N, H, W, C), 与 DataBlock 的 (SY, SX, Depth) 在内存里同序,
            ' 因此输入只需要一次零拷贝的形状重解释, 算完把结果整块拷回即可
            Dim x4 As Tensor = db.Value4D
            Dim packed As Tensor = PackFilters()
            Dim bias As Tensor = biases.Value

            ' 卷积是典型的算力密集算子：规模达标时降到单精度计算再升回双精度。
            ' MixedPrecisionOps 返回 Nothing 表示判据不成立（或数据里有非有限值），
            ' 此时保持原来的双精度路径。
            Dim y = MixedPrecisionOps.Conv2D(x4, packed, bias, stride, padding)

            If y Is Nothing Then
                y = Tensor.computeKernel.Conv2D(x4, packed, bias, stride, padding)
            End If

            Call lA.SetValues(y.Data)

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

            ' 按输出通道并行：第 d 个通道只写 w 里"步长 OutC、偏移 d"的那一列，
            ' 各通道写入位置互不相交，因此不需要锁。
            ' 这是纯数据重排（带宽受限），搬 GPU 不会有收益，
            ' 并行是这个四重循环唯一真实的加速来源。
            tpl.Parallel.For(0, OutC,
                Sub(d As Integer)
                    Dim src = filters(d).w

                    For kh As Integer = 0 To sy - 1
                        For kw As Integer = 0 To sx - 1
                            For c As Integer = 0 To inDepth - 1
                                w(((kh * sx + kw) * inDepth + c) * OutC + d) = src((sx * kh + kw) * inDepth + c)
                            Next
                        Next
                    Next
                End Sub)

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

            ' 同样按输出通道并行：每个通道只写自己的 filters(d).dw，互不重叠。
            ' 循环体是**累加**而不是赋值（见上文说明），并行化不改变这一语义。
            tpl.Parallel.For(0, OutC,
                Sub(d As Integer)
                    Dim dst = filters(d).dw

                    For kh As Integer = 0 To sy - 1
                        For kw As Integer = 0 To sx - 1
                            For c As Integer = 0 To inDepth - 1
                                dst((sx * kh + kw) * inDepth + c) += src(((kh * sx + kw) * inDepth + c) * OutC + d)
                            Next
                        Next
                    Next

                    Call filters(d).MarkGradientModified()
                End Sub)
        End Sub

        ''' <summary>
        ''' Computes the gradients with respect to the filters, the biases and the input of this layer.
        ''' </summary>
        Public Overridable Sub backward() Implements Layer.backward
            ' zero out gradient wrt bottom data, we're about to fill it
            ' (图的执行器会置 AccumulateGradient=True 并事先统一清零, 此时不能清零, 否则会丢掉
            '  其它消费者(例如跳跃连接)已经写入本块的梯度)
            Dim db As DataBlock = in_act

            If Not AccumulateGradient Then
                Call db.clearGradient()
            End If

            Dim x4 As Tensor = db.Value4D
            Dim gradOut As Tensor = out_act.Grad4D

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

            Call biases.MarkGradientModified()

            ' 3) 对输入的梯度: 用前向缓存下来的卷积核算出, 布局与 DataBlock 同序, 整块拷贝
            '
            ' 原先这一步是在按输出通道并行的循环里对本层的输入块做 addGradient, 而不同输出通道
            ' 会落在同一批滤波窗口上, 因此属于跨线程的读-改-写, 存在丢失更新(实测同一个随机种子
            ' 多次运行 loss 从 0.37 漂到 0.75)。改为走后端算子之后, 累加在算子内部按位置唯一完成,
            ' 既没有数据竞争也不再需要按线程切分。
            Dim gradInput = Tensor.computeKernel.Conv2DBackwardInput(
                gradOut, filtersPacked, db.TensorShape4D, stride, padding)

            Call WriteInputGradient(db, gradInput.Data)
        End Sub

        ''' <summary>Returns a short description of this layer.</summary>
        ''' <returns>The constant text <c>conv()</c>.</returns>
        Public Overrides Function ToString() As String
            Return "conv()"
        End Function
    End Class

End Namespace
