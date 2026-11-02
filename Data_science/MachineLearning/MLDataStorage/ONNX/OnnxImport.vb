' /********************************************************************************/
'  Microsoft.VisualBasic.MachineLearning.DataStorage
'  Data_science\MachineLearning\MLDataStorage\ONNX\OnnxImport.vb
' /********************************************************************************/

Imports System.IO
Imports System.Linq
Imports Microsoft.VisualBasic.MachineLearning.CNN
Imports Microsoft.VisualBasic.MachineLearning.CNN.data
Imports Microsoft.VisualBasic.MachineLearning.CNN.layers
Imports Microsoft.VisualBasic.MachineLearning.CNN.losslayers
Imports Microsoft.VisualBasic.MachineLearning.DataStorage.ONNX.Proto
Imports std = System.Math

Namespace ONNX

    ''' <summary>
    ''' ONNX 导入选项。
    ''' </summary>
    Public Class OnnxImportOptions

        ''' <summary>
        ''' 是否把输入归一化（<c>Sub</c> / <c>Div</c> 常量）折算进第一个全连接层（默认 True）。
        ''' </summary>
        ''' <remarks>
        ''' 强化学习策略模型普遍是"观测归一化 + MLP"的结构。归一化是一个仿射变换，可以完全等价地折叠进
        ''' 首层的权重与偏置：<c>y = W·((x - m)/s) + b = (W/s)·x + (b - W·m/s)</c>。
        ''' 这样还原出来的网络不需要为归一化引入新的层类型。
        ''' </remarks>
        Public Property FoldNormalizer As Boolean = True

    End Class

    ''' <summary>
    ''' 把 ONNX 计算图还原为 <see cref="ConvolutionalNN"/>。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 支持的节点：<c>Conv</c>、<c>ConvTranspose</c>、<c>MaxPool</c>、<c>Gemm</c>、<c>Softmax</c>、
    ''' <c>Relu</c>、<c>LeakyRelu</c>、<c>Elu</c>、<c>Sigmoid</c>、<c>Tanh</c>、<c>LRN</c>、
    ''' <c>Sub</c>/<c>Div</c>（输入归一化）、<c>Flatten</c>/<c>Reshape</c>/<c>Transpose</c>（形状整理）、
    ''' <c>Identity</c>/<c>Dropout</c>（恒等）。
    ''' </para>
    ''' <para>
    ''' 其余算子（例如 <c>AveragePool</c>、<c>Concat</c>、<c>BatchNormalization</c>）在本框架里没有对应的层，
    ''' 遇到时会抛出 <see cref="OnnxUnsupportedOperatorException"/>，而不是悄悄丢掉一段计算。
    ''' </para>
    ''' </remarks>
    Public Module OnnxImport

        ''' <summary>
        ''' 把 ONNX 模型还原为卷积网络。
        ''' </summary>
        ''' <param name="model">已加载的 ONNX 模型。</param>
        ''' <param name="options">导入选项；Nothing 表示使用默认值。</param>
        ''' <returns>可继续推理 / 训练的网络对象。</returns>
        Public Function ToCNN(model As OnnxModel, Optional options As OnnxImportOptions = Nothing) As ConvolutionalNN
            If model Is Nothing Then
                Throw New ArgumentNullException(NameOf(model))
            End If

            If options Is Nothing Then
                options = New OnnxImportOptions()
            End If

            Dim graph As GraphProto = model.Graph
            Dim inputs As ValueInfoProto() = model.Inputs

            If inputs.Length <> 1 Then
                Throw New NotSupportedException(
                    $"只支持单输入的计算图，当前模型有 {inputs.Length} 个输入：" &
                    String.Join(", ", inputs.Select(Function(v) v.Name)))
            End If

            Dim def As New OutputDefinition()
            Dim inputDims As Integer() = inputs(0).GetDims().Select(Function(v) CInt(v)).ToArray()
            Dim inputLayer As InputLayer = CreateInputLayer(def, inputDims, inputs(0).Name)

            Dim layers As New List(Of Layer) From {inputLayer}
            Dim consts As New Dictionary(Of String, OnnxTensor)(StringComparer.Ordinal)
            Dim current As String = inputs(0).Name
            Dim flattenShape As OutputDefinition = Nothing
            Dim channelLast As Boolean = False
            Dim mean As Double() = Nothing
            Dim scale As Double() = Nothing

            For Each node As NodeProto In graph.Node
                Dim op As String = If(node.OpType, "").ToLowerInvariant()

                Select Case op
                    Case "sub", "div"
                        Dim operand As OnnxTensor = ResolveTensor(model, consts, node.Input(1), node)

                        If Not options.FoldNormalizer Then
                            Throw New OnnxUnsupportedOperatorException(
                                node, "计算图以输入归一化开头，但导入了 FoldNormalizer = False，无法还原该层")
                        End If

                        If op = "sub" Then
                            mean = operand.Data
                        Else
                            scale = operand.Data
                        End If

                        current = node.Output(0)

                    Case "transpose"
                        Dim perm As Integer() = NodeAttributes.GetInts(node, "perm")

                        If perm.Length = 4 AndAlso perm(0) = 0 AndAlso perm(1) = 2 AndAlso perm(2) = 3 AndAlso perm(3) = 1 Then
                            ' NCHW -> NHWC：说明后续的展平顺序与框架内部一致
                            channelLast = True
                        Else
                            Throw New OnnxUnsupportedOperatorException(node, $"只支持 NCHW -> NHWC 的转置，实际为 [{String.Join(", ", perm)}]")
                        End If

                        current = node.Output(0)

                    Case "flatten"
                        flattenShape = New OutputDefinition(def.outX, def.outY, def.depth)
                        current = node.Output(0)

                    Case "reshape"
                        flattenShape = New OutputDefinition(def.outX, def.outY, def.depth)
                        current = node.Output(0)

                    Case "identity", "dropout"
                        current = node.Output(0)

                    Case "constant"
                        Dim values As OnnxTensor() = OnnxGraphEvaluator.Evaluate(node, New Dictionary(Of String, OnnxTensor))

                        If node.Output.Count > 0 Then
                            consts(node.Output(0)) = values(0)
                        End If

                    Case "gemm"
                        Dim fc As FullyConnectedLayer = ReadGemm(model, consts, node, def, flattenShape, channelLast, mean, scale)
                        Call layers.Add(fc)
                        current = node.Output(0)
                        mean = Nothing
                        scale = Nothing
                        flattenShape = Nothing

                    Case "conv"
                        Call layers.Add(ReadConvolution(model, consts, node, def))
                        current = node.Output(0)

                    Case "convtranspose"
                        Call layers.Add(ReadConvTranspose(model, consts, node, def))
                        current = node.Output(0)

                    Case "maxpool"
                        Call layers.Add(ReadMaxPool(node, def))
                        current = node.Output(0)

                    Case "relu"
                        Call layers.Add(New RectifiedLinearUnitsLayer())
                        current = node.Output(0)

                    Case "leakyrelu"
                        Call layers.Add(New LeakyReluLayer(NodeAttributes.GetFloat(node, "alpha", 0.01)))
                        current = node.Output(0)

                    Case "elu"
                        Call layers.Add(New EluLayer(NodeAttributes.GetFloat(node, "alpha", 1.0)))
                        current = node.Output(0)

                    Case "sigmoid"
                        Call layers.Add(New SigmoidLayer())
                        current = node.Output(0)

                    Case "tanh"
                        Call layers.Add(New TanhLayer())
                        current = node.Output(0)

                    Case "lrn"
                        Call layers.Add(ReadLRN(node))
                        current = node.Output(0)

                    Case "softmax"
                        Call layers.Add(New SoftMaxLayer(def))
                        current = node.Output(0)

                    Case Else
                        Throw New OnnxUnsupportedOperatorException(node)
                End Select
            Next

            ' ConvolutionalNN 约定最后一层必须是损失层
            If Not (TypeOf layers.Last() Is LossLayer) Then
                Call layers.Add(New RegressionLayer(def))
            End If

            Return New ConvolutionalNN(layers)
        End Function

        ''' <summary>
        ''' 从 ONNX 文件直接还原卷积网络。
        ''' </summary>
        ''' <param name="path">ONNX 文件路径。</param>
        ''' <param name="options">导入选项。</param>
        ''' <returns>卷积网络对象。</returns>
        Public Function Load(path As String, Optional options As OnnxImportOptions = Nothing) As ConvolutionalNN
            Return ToCNN(OnnxModel.Load(path), options)
        End Function

        ''' <summary>
        ''' 从 ONNX 数据流还原卷积网络。
        ''' </summary>
        ''' <param name="stream">ONNX 数据流。</param>
        ''' <param name="options">导入选项。</param>
        ''' <returns>卷积网络对象。</returns>
        Public Function LoadFrom(stream As Stream, Optional options As OnnxImportOptions = Nothing) As ConvolutionalNN
            Return ToCNN(OnnxModel.FromStream(stream), options)
        End Function

        Private Function CreateInputLayer(def As OutputDefinition, dims As Integer(), name As String) As InputLayer
            Select Case dims.Length
                Case 0
                    Throw New NotSupportedException($"输入 ""{name}"" 没有声明形状，无法还原输入层")

                Case 1
                    Return New InputLayer(def, dims(0), 1, 1)

                Case 2
                    Return New InputLayer(def, dims(1), 1, 1)

                Case 4
                    Return New InputLayer(def, dims(3), dims(2), dims(1))

                Case Else
                    Throw New NotSupportedException(
                        $"只支持 1/2/4 维输入，输入 ""{name}"" 的形状为 [{String.Join(", ", dims)}]")
            End Select
        End Function

        Private Function ResolveTensor(model As OnnxModel, consts As Dictionary(Of String, OnnxTensor),
                                       name As String, node As NodeProto) As OnnxTensor

            Dim tensor As OnnxTensor = Nothing

            If consts.TryGetValue(name, tensor) Then
                Return tensor
            End If

            If model.Initializers.TryGetValue(name, tensor) Then
                Return tensor
            End If

            Throw New OnnxUnsupportedOperatorException(node, $"张量 ""{name}"" 不是常量（既不是初始化器也不是 Constant 节点）")
        End Function

        Private Function HasTensor(model As OnnxModel, consts As Dictionary(Of String, OnnxTensor), name As String) As Boolean
            If String.IsNullOrEmpty(name) Then
                Return False
            End If

            Return consts.ContainsKey(name) OrElse model.Initializers.ContainsKey(name)
        End Function

        Private Function ReadGemm(model As OnnxModel, consts As Dictionary(Of String, OnnxTensor), node As NodeProto,
                                  def As OutputDefinition, flattenShape As OutputDefinition,
                                  channelLast As Boolean, mean As Double(), scale As Double()) As FullyConnectedLayer

            Dim weights As OnnxTensor = ResolveTensor(model, consts, node.Input(1), node)
            Dim transB As Boolean = NodeAttributes.GetInt(node, "transB") <> 0
            Dim alpha As Double = NodeAttributes.GetFloat(node, "alpha", 1.0)
            Dim beta As Double = NodeAttributes.GetFloat(node, "beta", 1.0)
            Dim outSize As Integer
            Dim inSize As Integer
            Dim transpose As Boolean

            If transB Then
                outSize = weights.Dims(0)
                inSize = weights.Dims(1)
                transpose = False
            Else
                inSize = weights.Dims(0)
                outSize = weights.Dims(1)
                transpose = True
            End If

            Dim expects As Integer = def.outX * def.outY * def.depth

            If expects <> inSize Then
                Throw New InvalidOperationException(
                    $"节点 ""{node.Name}"" 的输入长度为 {inSize}，而网络当前的特征图展平后是 {expects} " &
                    $"（{def.outX} x {def.outY} x {def.depth}），形状不匹配")
            End If

            Dim layer As New FullyConnectedLayer(def, outSize)
            Dim params As BackPropResult() = layer.BackPropagationResult.ToArray()
            Dim needsPermute As Boolean = Not channelLast AndAlso
                                          flattenShape IsNot Nothing AndAlso
                                          flattenShape.depth > 0 AndAlso flattenShape.outX > 0 AndAlso flattenShape.outY > 0 AndAlso
                                          flattenShape.outX * flattenShape.outY * flattenShape.depth = inSize

            For i As Integer = 0 To outSize - 1
                Dim target As Double() = params(i).Weights

                For j As Integer = 0 To inSize - 1
                    Dim source As Double

                    If transpose Then
                        source = weights.Data(j * outSize + i)
                    Else
                        source = weights.Data(i * inSize + j)
                    End If

                    If needsPermute Then
                        ' ONNX 的展平顺序是 (c, y, x)，框架要求 (y, x, c)
                        Dim channels As Integer = flattenShape.depth
                        Dim height As Integer = flattenShape.outY
                        Dim width As Integer = flattenShape.outX
                        Dim c As Integer = j \ (height * width)
                        Dim rest As Integer = j Mod (height * width)
                        Dim y As Integer = rest \ width
                        Dim x As Integer = rest Mod width

                        target((y * width + x) * channels + c) = source * alpha
                    Else
                        target(j) = source * alpha
                    End If
                Next
            Next

            Dim bias As Double() = New Double(outSize - 1) {}

            If node.Input.Count > 2 AndAlso HasTensor(model, consts, node.Input(2)) Then
                Dim c As OnnxTensor = ResolveTensor(model, consts, node.Input(2), node)

                For i As Integer = 0 To outSize - 1
                    bias(i) = c.Data(i) * beta
                Next
            End If

            If mean IsNot Nothing OrElse scale IsNot Nothing Then
                Call FoldNormalizer(params, bias, inSize, outSize, mean, scale)
            End If

            Call Array.Copy(bias, params(outSize).Weights, outSize)

            Return layer
        End Function

        ''' <summary>
        ''' 把输入归一化 <c>(x - mean) / scale</c> 折算进第一个全连接层。
        ''' </summary>
        Private Sub FoldNormalizer(params As BackPropResult(), bias As Double(), inSize As Integer, outSize As Integer,
                                   mean As Double(), scale As Double())

            For i As Integer = 0 To outSize - 1
                Dim row As Double() = params(i).Weights
                Dim shift As Double = 0.0

                For j As Integer = 0 To inSize - 1
                    Dim s As Double = If(scale Is Nothing OrElse j >= scale.Length, 1.0, scale(j))

                    If s = 0 Then
                        Throw New InvalidOperationException($"归一化的除数在第 {j} 个观测维度上为 0，无法折算")
                    End If

                    row(j) = row(j) / s

                    If mean IsNot Nothing AndAlso j < mean.Length Then
                        shift += row(j) * mean(j)
                    End If
                Next

                bias(i) -= shift
            Next
        End Sub

        Private Function ReadConvolution(model As OnnxModel, consts As Dictionary(Of String, OnnxTensor),
                                         node As NodeProto, def As OutputDefinition) As ConvolutionLayer

            Dim kernel As OnnxTensor = ResolveTensor(model, consts, node.Input(1), node)
            Dim channels As Integer = def.depth
            Dim outChannels As Integer = kernel.Dims(0)

            If kernel.Dims(1) <> channels Then
                Throw New InvalidOperationException(
                    $"Conv 的输入通道数 {channels} 与卷积核 [{String.Join(", ", kernel.Dims)}] 不匹配")
            End If

            Dim kernelH As Integer = kernel.Dims(2)
            Dim kernelW As Integer = kernel.Dims(3)

            If kernelH <> kernelW Then
                Throw New OnnxUnsupportedOperatorException(node, $"本框架只支持方形卷积核，实际为 {kernelH}x{kernelW}")
            End If

            Dim strides As Integer() = NodeAttributes.GetInts(node, "strides")
            Dim pads As Integer() = NodeAttributes.GetInts(node, "pads")
            Dim group As Integer = CInt(NodeAttributes.GetInt(node, "group", 1))

            If group <> 1 Then
                Throw New OnnxUnsupportedOperatorException(node, $"本框架的卷积层不支持分组卷积（group={group}）")
            End If

            Dim stride As Integer = If(strides.Length >= 2, strides(0), 1)

            If strides.Length >= 2 AndAlso strides(0) <> strides(1) Then
                Throw New OnnxUnsupportedOperatorException(node, "本框架只支持各向同性的步长")
            End If

            Dim padding As Integer = ValidatePadding(node, pads)

            Dim layer As New ConvolutionLayer(def, kernelH, outChannels, stride, padding)
            Dim params As BackPropResult() = layer.BackPropagationResult.ToArray()

            For m As Integer = 0 To outChannels - 1
                Dim target As Double() = params(m).Weights

                For c As Integer = 0 To channels - 1
                    For ky As Integer = 0 To kernelH - 1
                        For kx As Integer = 0 To kernelW - 1
                            target((kernelH * ky + kx) * channels + c) = kernel.Data(((m * channels + c) * kernelH + ky) * kernelW + kx)
                        Next
                    Next
                Next
            Next

            Dim bias As Double() = New Double(outChannels - 1) {}

            If node.Input.Count > 2 AndAlso HasTensor(model, consts, node.Input(2)) Then
                Dim b As OnnxTensor = ResolveTensor(model, consts, node.Input(2), node)

                Call Array.Copy(b.Data, bias, outChannels)
            End If

            Call Array.Copy(bias, params(outChannels).Weights, outChannels)

            Return layer
        End Function

        Private Function ReadConvTranspose(model As OnnxModel, consts As Dictionary(Of String, OnnxTensor),
                                           node As NodeProto, def As OutputDefinition) As Conv2DTransposeLayer

            Dim kernel As OnnxTensor = ResolveTensor(model, consts, node.Input(1), node)
            Dim inChannels As Integer = kernel.Dims(0)
            Dim outChannels As Integer = kernel.Dims(1)

            If kernel.Dims(0) <> def.depth Then
                Throw New InvalidOperationException(
                    $"ConvTranspose 的输入通道数 {def.depth} 与卷积核 [{String.Join(", ", kernel.Dims)}] 不匹配")
            End If

            Dim kernelH As Integer = kernel.Dims(2)
            Dim kernelW As Integer = kernel.Dims(3)

            If kernelH <> kernelW Then
                Throw New OnnxUnsupportedOperatorException(node, $"本框架只支持方形卷积核，实际为 {kernelH}x{kernelW}")
            End If

            Dim strides As Integer() = NodeAttributes.GetInts(node, "strides")
            Dim pads As Integer() = NodeAttributes.GetInts(node, "pads")
            Dim stride As Integer = If(strides.Length >= 2, strides(0), 1)
            Dim padding As Integer = ValidatePadding(node, pads)
            Dim useBias As Boolean = node.Input.Count > 2 AndAlso HasTensor(model, consts, node.Input(2))

            Dim layer As New Conv2DTransposeLayer(def, kernelH, outChannels, stride, padding, useBias)
            Dim params As BackPropResult() = layer.BackPropagationResult.ToArray()

            For d As Integer = 0 To outChannels - 1
                Dim target As Double() = params(d).Weights

                For c As Integer = 0 To inChannels - 1
                    For ky As Integer = 0 To kernelH - 1
                        For kx As Integer = 0 To kernelW - 1
                            target((kernelH * ky + kx) * inChannels + c) = kernel.Data(((c * outChannels + d) * kernelH + ky) * kernelW + kx)
                        Next
                    Next
                Next
            Next

            Dim bias As Double() = New Double(outChannels - 1) {}

            If useBias Then
                Dim b As OnnxTensor = ResolveTensor(model, consts, node.Input(2), node)

                Call Array.Copy(b.Data, bias, outChannels)
            End If

            Call Array.Copy(bias, params(outChannels).Weights, outChannels)

            Return layer
        End Function

        Private Function ReadMaxPool(node As NodeProto, def As OutputDefinition) As PoolingLayer
            Dim kernel As Integer() = NodeAttributes.GetInts(node, "kernel_shape")
            Dim strides As Integer() = NodeAttributes.GetInts(node, "strides")
            Dim pads As Integer() = NodeAttributes.GetInts(node, "pads")

            If kernel.Length < 2 Then
                Throw New OnnxUnsupportedOperatorException(node, "MaxPool 缺少 kernel_shape 属性")
            End If

            If kernel(0) <> kernel(1) Then
                Throw New OnnxUnsupportedOperatorException(node, $"本框架只支持方形池化窗，实际为 {kernel(0)}x{kernel(1)}")
            End If

            Dim stride As Integer = If(strides.Length >= 2, strides(0), kernel(0))

            If strides.Length >= 2 AndAlso strides(0) <> strides(1) Then
                Throw New OnnxUnsupportedOperatorException(node, "本框架只支持各向同性的步长")
            End If

            If NodeAttributes.GetInt(node, "ceil_mode", 0) <> 0 Then
                Throw New OnnxUnsupportedOperatorException(node, "不支持 ceil_mode = 1 的池化")
            End If

            Dim padding As Integer = ValidatePadding(node, pads)

            Return New PoolingLayer(def, kernel(0), stride, padding)
        End Function

        Private Function ReadLRN(node As NodeProto) As LocalResponseNormalizationLayer
            Dim alpha As Double = NodeAttributes.GetFloat(node, "alpha", 0.0001)
            Dim beta As Double = NodeAttributes.GetFloat(node, "beta", 0.75)
            Dim bias As Double = NodeAttributes.GetFloat(node, "bias", 1.0)
            Dim size As Integer = CInt(NodeAttributes.GetInt(node, "size", 5))

            ' 本框架的 LRN 层的 alpha / beta / k 是硬编码常量，超参不同就无法等价还原
            If std.Abs(alpha - 0.0001) > 1.0E-12 OrElse
               std.Abs(beta - 0.75) > 1.0E-12 OrElse
               std.Abs(bias - 2.0) > 1.0E-12 Then

                Throw New OnnxUnsupportedOperatorException(
                    node, $"本框架的 LRN 层只支持 alpha=0.0001, beta=0.75, bias=2，实际为 alpha={alpha}, beta={beta}, bias={bias}")
            End If

            Return New LocalResponseNormalizationLayer(size)
        End Function

        ''' <summary>
        ''' 校验 ONNX 的 <c>pads</c> 属性是否可以用本框架的"对称填充"表示，并返回填充宽度。
        ''' </summary>
        Private Function ValidatePadding(node As NodeProto, pads As Integer()) As Integer
            If pads Is Nothing OrElse pads.Length = 0 Then
                Return 0
            End If

            Dim top As Integer = pads(0)
            Dim left As Integer = If(pads.Length > 1, pads(1), top)
            Dim bottom As Integer = If(pads.Length > 2, pads(2), top)
            Dim right As Integer = If(pads.Length > 3, pads(3), left)

            If top <> left OrElse top <> bottom OrElse top <> right Then
                Throw New OnnxUnsupportedOperatorException(
                    node, $"本框架只支持对称填充，实际 pads = [{String.Join(", ", pads)}]")
            End If

            Return top
        End Function

    End Module

End Namespace
