' ---------------------------------------------------------------------------
' 转置卷积（反卷积 / up-convolution）
'
' 输出尺寸：out = stride * (in - 1) + kernel - 2 * padding
'
' 实现要点（这是本层最关键的一段推导，改动前请先读懂）：
'
'   转置卷积的语义是"散射"：
'       Y[n, i*stride + kh - padding, j*stride + kw - padding, oc] += X[n, i, j, c] * W[kh, kw, c, oc]
'
'   后端只有"聚集"式的 Conv2D / Conv2DBackward*，没有转置卷积原语。
'   于是把它改写成一次 stride=1 的普通卷积：
'       1) X' = 膨胀 X（元素之间插 stride-1 个零），尺寸 (H-1)*stride + 1
'       2) X''= 四周填充 (kernel - 1 - padding)
'       3) Wf = 把 W 沿 (kh, kw) 两轴翻转
'       4) Y  = Conv2D(X'', Wf, bias, stride:=1, padding:=0)
'   四个反向量同理：
'       dX''  = Conv2DBackwardInput(dY, Wf, shape(X''), 1, 0)，再按 stride 抽回原位置
'       dWf   = Conv2DBackwardFilter(dY, X'', {K,K,Cin,Cout}, 1, 0)，翻回来才是 dW
'       dbias = Conv2DBackwardBias(dY)
'
'   为什么不用"直接把 Conv2DBackwardInput 当转置卷积用"这种写法：
'   那条路径要求 filters.Shape(2)(=Cin) 等于**输出**通道数 Cout，
'   Cin≠Cout 时 GPU 侧会静默回退、CPU 侧按错误的通道数索引（越界读）。
'   膨胀方案的三个反向全部落在现成算子上，且形状全部合法。
'
'   手算校验（H=2, X=[x0,x1], W=[w0,w1], stride=2, kernel=2, padding=0）：
'       X'=[x0,0,x1] → X''=[0,x0,0,x1,0]，Wf=[w1,w0]
'       Conv2D 得 [x0w0, x0w1, x1w0, x1w1]，与散射定义逐项一致。
' ---------------------------------------------------------------------------

Imports System.Runtime.Serialization
Imports Microsoft.VisualBasic.MachineLearning.CNN.data
Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports tpl = System.Threading.Tasks
Imports std = System.Math

Namespace CNN.layers

    ''' <summary>
    ''' Transposed (de)convolution layer: it upsamples a feature map back to a larger spatial size,
    ''' with a learnable kernel (U-Net decoder).
    ''' </summary>
    ''' <remarks>
    ''' 参数布局与 <see cref="ConvolutionLayer"/> 完全同构（每个输出通道一个 <see cref="DataBlock"/>
    ''' 卷积核 + 一个偏置块），因此可以直接喂给既有的训练器（<c>BackPropagationResult</c>）。
    ''' </remarks>
    Public Class Conv2DTransposeLayer : Inherits DataLink
        Implements Layer

        Friend l1_decay_mul As Double = 0.0
        Friend l2_decay_mul As Double = 1.0
        Friend BIAS_PREF As Single = 0.1F
        Friend out_depth, out_sx, out_sy As Integer
        Friend in_depth, in_sx, in_sy As Integer
        Friend sx, sy As Integer
        Friend stride, padding As Integer
        Friend useBias As Boolean
        Friend filters As DataBlock()
        Friend biases As DataBlock

        ''' <summary>最近一次前向构造的"膨胀 + 填充"输入（反向算卷积核梯度要用）。</summary>
        <IgnoreDataMember> Private xDilated As Tensor
        ''' <summary>最近一次前向打包好的翻转卷积核（反向算输入梯度要用）。</summary>
        <IgnoreDataMember> Private kernelFlipped As Tensor

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

        ''' <summary>Gets the kind of this layer, always <see cref="LayerTypes.Conv2DTranspose"/>.</summary>
        Public ReadOnly Property Type As LayerTypes Implements Layer.Type
            Get
                Return LayerTypes.Conv2DTranspose
            End Get
        End Property

        ''' <summary>Creates an empty layer, used by the deserializer.</summary>
        Sub New()
        End Sub

        ''' <summary>
        ''' Creates a transposed convolution layer and derives the (upsampled) output size from the input size.
        ''' </summary>
        ''' <param name="def">The shared output definition that carries the input size.</param>
        ''' <param name="kernel">Side length of the square filter window.</param>
        ''' <param name="outChannels">Number of output channels; 0 (default) 表示与输入通道数相同。</param>
        ''' <param name="stride">Stride; 2 表示空间尺寸翻倍。</param>
        ''' <param name="padding">Zero padding that was applied by the corresponding forward convolution.</param>
        ''' <param name="useBias">When <c>True</c> a learnable bias vector is added to the output.</param>
        ''' <exception cref="InvalidProgramException">Thrown when the derived output size is not positive.</exception>
        Public Sub New(def As OutputDefinition,
                       Optional kernel As Integer = 2,
                       Optional outChannels As Integer = 0,
                       Optional stride As Integer = 2,
                       Optional padding As Integer = 0,
                       Optional useBias As Boolean = True)

            If kernel <= 0 Then
                Throw New ArgumentOutOfRangeException(NameOf(kernel), "卷积核窗口必须为正数")
            End If
            If stride <= 0 Then
                Throw New ArgumentOutOfRangeException(NameOf(stride), "步长必须为正数")
            End If
            If padding < 0 Then
                Throw New ArgumentOutOfRangeException(NameOf(padding), "填充宽度不能为负数")
            End If
            If padding > kernel - 1 Then
                Throw New ArgumentOutOfRangeException(NameOf(padding), "填充宽度必须小于卷积核窗口")
            End If

            in_depth = def.depth
            in_sx = def.outX
            in_sy = def.outY

            Me.sx = kernel
            Me.sy = kernel
            Me.stride = stride
            Me.padding = padding
            Me.useBias = useBias
            Me.out_depth = If(outChannels > 0, outChannels, in_depth)

            ' 转置卷积的输出尺寸公式（与 PyTorch / TensorFlow 的 Conv2DTranspose 一致）
            out_sx = stride * (in_sx - 1) + sx - 2 * padding
            out_sy = stride * (in_sy - 1) + sy - 2 * padding

            If out_sx <= 0 OrElse out_sy <= 0 Then
                Throw New InvalidProgramException(
                    $"conv2d_transpose 输出尺寸非法: {out_sx}x{out_sy} (input: {in_sx}x{in_sy}, kernel: {kernel}, stride: {stride}, padding: {padding})")
            End If

            filters = New DataBlock(out_depth - 1) {}

            For i As Integer = 0 To out_depth - 1
                filters(i) = New DataBlock(sx, sy, in_depth)
            Next

            biases = New DataBlock(1, 1, out_depth, If(useBias, CDbl(BIAS_PREF), 0.0))

            def.outX = out_sx
            def.outY = out_sy
            def.depth = out_depth
        End Sub

        ' ---------------- 形状助手 ----------------

        ''' <summary>膨胀之后四周补充的零填充宽度（kernel - 1 - padding）。</summary>
        Private ReadOnly Property padWidth As Integer
            Get
                Return sx - 1 - padding
            End Get
        End Property

        ''' <summary>膨胀 + 填充之后的高度。</summary>
        Private ReadOnly Property dilateH As Integer
            Get
                Return stride * (in_sy - 1) + 1 + 2 * padWidth
            End Get
        End Property

        ''' <summary>膨胀 + 填充之后的宽度。</summary>
        Private ReadOnly Property dilateW As Integer
            Get
                Return stride * (in_sx - 1) + 1 + 2 * padWidth
            End Get
        End Property

        ' ---------------- 前向 ----------------

        ''' <summary>
        ''' 把输入膨胀补零之后做一次 stride=1 的（翻转核）卷积，实现上采样。
        ''' </summary>
        ''' <param name="db">The input data block.</param>
        ''' <param name="training">Ignored; the layer behaves the same in both modes.</param>
        ''' <returns>The upsampled feature maps.</returns>
        Public Function forward(db As DataBlock, training As Boolean) As DataBlock Implements Layer.forward
            Dim lA As New DataBlock(out_sx, out_sy, out_depth, 0.0) With {.trace = Me.ToString}

            in_act = db
            out_act = lA

            Dim xpp As Tensor = BuildDilated(db.Value4D)
            Dim kf As Tensor = PackFilters(flip:=True)
            Dim bias As Tensor = If(useBias, biases.Value, Nothing)

            ' 卷积族热点：规模达标时降到单精度算完再升回双精度
            Dim y = PrecisionRouter.Conv2DOr(xpp, kf, bias, 1, 0)

            Call lA.SetValues(y.Data)

            ' 反向传播还要用这两份中间结果（膨胀输入算核梯度、翻转核算输入梯度）
            xDilated = xpp
            kernelFlipped = kf

            Return out_act
        End Function

        ''' <summary>
        ''' 构造"膨胀 + 零填充"之后的输入，形状 (1, dilateH, dilateW, in_depth)。
        ''' </summary>
        Private Function BuildDilated(x4 As Tensor) As Tensor
            Dim depthN = in_depth
            Dim pw = padWidth
            Dim H2 = dilateH, W2 = dilateW
            Dim dst As Tensor = Tensor.Zeros(New Integer() {1, H2, W2, depthN})
            Dim d = dst.Data
            Dim s = x4.Data

            For h As Integer = 0 To in_sy - 1
                Dim dh = h * stride + pw

                For w As Integer = 0 To in_sx - 1
                    Dim dw = w * stride + pw
                    Dim si = (h * in_sx + w) * depthN
                    Dim di = (dh * W2 + dw) * depthN

                    For c As Integer = 0 To depthN - 1
                        d(di + c) = s(si + c)
                    Next
                Next
            Next

            Call dst.MarkHostModified()
            Return dst
        End Function

        ''' <summary>
        ''' 把各输出通道的卷积核打包成 (KH, KW, C, OutC)；<paramref name="flip"/> 为 True 时沿 (kh, kw) 翻转。
        ''' </summary>
        Private Function PackFilters(flip As Boolean) As Tensor
            Dim inDepth = in_depth
            Dim OutC = out_depth
            Dim dst As Tensor = Tensor.Zeros(New Integer() {sy, sx, inDepth, OutC})
            Dim buf = dst.Data

            tpl.Parallel.For(0, OutC,
                Sub(d As Integer)
                    Dim src = filters(d).w

                    For kh As Integer = 0 To sy - 1
                        Dim skh = If(flip, sy - 1 - kh, kh)

                        For kw As Integer = 0 To sx - 1
                            Dim skw = If(flip, sx - 1 - kw, kw)

                            For c As Integer = 0 To inDepth - 1
                                buf(((kh * sx + kw) * inDepth + c) * OutC + d) = src((sx * skh + skw) * inDepth + c)
                            Next
                        Next
                    Next
                End Sub)

            Call dst.MarkHostModified()
            Return dst
        End Function

        ' ---------------- 反向 ----------------

        ''' <summary>
        ''' Computes the gradients with respect to the filters, the bias and the input of this layer.
        ''' </summary>
        Public Sub backward() Implements Layer.backward
            Dim db As DataBlock = in_act

            If Not AccumulateGradient Then
                Call db.clearGradient()
            End If

            Dim gradOut As Tensor = out_act.Grad4D

            If xDilated Is Nothing Then
                xDilated = BuildDilated(db.Value4D)
            End If
            If kernelFlipped Is Nothing Then
                kernelFlipped = PackFilters(flip:=True)
            End If

            ' 1) 对卷积核的梯度：后端按 (KH, KW, C, OutC) 给出**翻转核**的梯度，拆回时翻转回来
            Dim gradFilters = PrecisionRouter.Conv2DBackwardFilterOr(
                gradOut, xDilated, New Integer() {sy, sx, in_depth, out_depth}, 1, 0)

            Call UnpackFilterGradients(gradFilters)

            ' 2) 对偏置的梯度：沿 N / OH / OW 求和；与卷积核一样属于参数梯度，跨样本累加
            If useBias Then
                Dim gradBias = PrecisionRouter.Conv2DBackwardBiasOr(gradOut)
                Dim bg = gradBias.Data

                For i As Integer = 0 To biases.dw.Length - 1
                    biases.dw(i) += bg(i)
                Next

                Call biases.MarkGradientModified()
            End If

            ' 3) 对输入的梯度：先算出膨胀域的梯度，再按 stride 抽回原始位置
            Dim gradDilated = PrecisionRouter.Conv2DBackwardInputOr(
                gradOut, kernelFlipped, New Integer() {1, dilateH, dilateW, in_depth}, 1, 0)

            Call WriteInputGradient(db, GatherInputGradient(gradDilated))
        End Sub

        ''' <summary>
        ''' 把后端算出的 (KH, KW, C, OutC) 卷积核梯度**翻转回来并累加**进各个 <see cref="filters"/>(d) 的 dw。
        ''' </summary>
        ''' <remarks>
        ''' 必须累加而不是赋值：参数梯度在一个 mini-batch 之内跨样本累加，
        ''' 只有训练器更新之后才会清零。
        ''' </remarks>
        Private Sub UnpackFilterGradients(packedGrad As Tensor)
            Dim inDepth = in_depth
            Dim OutC = out_depth
            Dim src = packedGrad.Data

            tpl.Parallel.For(0, OutC,
                Sub(d As Integer)
                    Dim dst = filters(d).dw

                    For kh As Integer = 0 To sy - 1
                        Dim skh = sy - 1 - kh

                        For kw As Integer = 0 To sx - 1
                            Dim skw = sx - 1 - kw

                            For c As Integer = 0 To inDepth - 1
                                dst((sx * kh + kw) * inDepth + c) += src(((skh * sx + skw) * inDepth + c) * OutC + d)
                            Next
                        Next
                    Next

                    Call filters(d).MarkGradientModified()
                End Sub)
        End Sub

        ''' <summary>
        ''' 从膨胀域的梯度之中按 stride 抽回输入位置的梯度（dX[h, w] = dX''[h*stride + pad]）。
        ''' </summary>
        Private Function GatherInputGradient(gradDilated As Tensor) As Double()
            Dim depthN = in_depth
            Dim W2 = dilateW
            Dim pw = padWidth
            Dim result(in_sx * in_sy * depthN - 1) As Double
            Dim src = gradDilated.Data

            For h As Integer = 0 To in_sy - 1
                Dim dh = h * stride + pw

                For w As Integer = 0 To in_sx - 1
                    Dim dw = w * stride + pw
                    Dim si = (dh * W2 + dw) * depthN
                    Dim di = (h * in_sx + w) * depthN

                    For c As Integer = 0 To depthN - 1
                        result(di + c) = src(si + c)
                    Next
                Next
            Next

            Return result
        End Function

        ''' <summary>Returns a short description of this layer.</summary>
        ''' <returns>A text of the form <c>conv2d_transpose(...)</c>.</returns>
        Public Overrides Function ToString() As String
            Return $"conv2d_transpose(k:{sx}, stride:{stride}, {in_depth}->{out_depth})"
        End Function
    End Class

End Namespace
