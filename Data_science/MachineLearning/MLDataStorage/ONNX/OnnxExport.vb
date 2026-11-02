' /********************************************************************************/
'  Microsoft.VisualBasic.MachineLearning.DataStorage
'  Data_science\MachineLearning\MLDataStorage\ONNX\OnnxExport.vb
' /********************************************************************************/

Imports System.IO
Imports System.Linq
Imports Microsoft.VisualBasic.MachineLearning.CNN
Imports Microsoft.VisualBasic.MachineLearning.CNN.data
Imports Microsoft.VisualBasic.MachineLearning.CNN.layers
Imports Microsoft.VisualBasic.MachineLearning.CNN.losslayers
Imports Microsoft.VisualBasic.MachineLearning.DataStorage.ONNX.Proto

Namespace ONNX

    ''' <summary>
    ''' ONNX 导出选项。
    ''' </summary>
    Public Class OnnxExportOptions

        ''' <summary>目标算子集版本（默认 13：Gemm/Conv/Pool/Softmax 的语义都已稳定）。</summary>
        Public Property Opset As Long = 13

        ''' <summary>目标 IR 版本（默认 7）。</summary>
        Public Property IrVersion As Long = 7

        ''' <summary>权重落盘精度，默认单精度；可切换为 <see cref="TensorDataType.Float64"/>。</summary>
        Public Property WeightPrecision As TensorDataType = TensorDataType.Float

        ''' <summary>生成该模型的工具名称。</summary>
        Public Property ProducerName As String = "sciBASIC.DeepLearning"

        ''' <summary>生成该模型的工具版本。</summary>
        Public Property ProducerVersion As String = "1.0"

        ''' <summary>计算图名称。</summary>
        Public Property ModelName As String = "cnn"

        ''' <summary>
        ''' 全连接层之前是否插入 <c>Transpose(NCHW -&gt; NHWC)</c> 再展平（默认 True）。
        ''' </summary>
        ''' <remarks>
        ''' <para>
        ''' 框架内部的特征图是 channel-last 的（展平顺序为 <c>(y, x, c)</c>），而 ONNX 的张量是 channel-first
        ''' 的（自然展平顺序为 <c>(c, y, x)</c>）。两种做法都能得到数值等价的计算图：
        ''' </para>
        ''' <list type="bullet">
        ''' <item>True：插入一次 <c>Transpose</c> 把通道换到最后，全连接权重可以原样写出；</item>
        ''' <item>False：不插入转置，改为把全连接权重按 <c>(c, y, x)</c> 重排，图结构与 PyTorch 导出的模型一致。</item>
        ''' </list>
        ''' </remarks>
        Public Property ChannelLastFlatten As Boolean = True

    End Class

    ''' <summary>
    ''' 把 <see cref="ConvolutionalNN"/> 导出为 ONNX 计算图。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 导出的是**前向推理图**：损失层里 <see cref="SoftMaxLayer"/> 会变成标准的 <c>Softmax</c> 节点，
    ''' 而 <see cref="RegressionLayer"/>（推理时等价于恒等映射）不会产生节点。
    ''' </para>
    ''' <para>
    ''' 带有跳跃连接或本框架特有语义的层（Maxout / Concat / FourierFeature / SVM 等）没有对应的标准算子，
    ''' 遇到时会抛出 <see cref="NotSupportedException"/> 而不是导出一个错误可用的模型。
    ''' </para>
    ''' </remarks>
    Public Module OnnxExport

        ''' <summary>
        ''' 构建卷积网络的 ONNX 模型对象（不写文件）。
        ''' </summary>
        ''' <param name="cnn">待导出的网络。</param>
        ''' <param name="options">导出选项；Nothing 表示使用默认值。</param>
        ''' <returns>ONNX 模型对象。</returns>
        Public Function ToModel(cnn As ConvolutionalNN, Optional options As OnnxExportOptions = Nothing) As OnnxModel
            If cnn Is Nothing Then
                Throw New ArgumentNullException(NameOf(cnn))
            End If

            If options Is Nothing Then
                options = New OnnxExportOptions()
            End If

            Dim graph As New GraphProto With {.Name = options.ModelName}
            Dim inputLayer As InputLayer = cnn.input

            If inputLayer Is Nothing Then
                Throw New InvalidOperationException("网络没有输入层，无法导出为 ONNX")
            End If

            Dim shape As Integer() = {1, inputLayer.out_depth, inputLayer.dims.y, inputLayer.dims.x}
            Dim geo As New OutputDefinition(inputLayer.dims.x, inputLayer.dims.y, inputLayer.out_depth)

            Call graph.Input.Add(ValueInfo("input", ElemType(options), shape))

            Dim current As String = "input"
            Dim counter As Integer = 0
            Dim featureMap As Boolean = True

            For i As Integer = 1 To cnn.LayerNum - 1
                Dim layer As Layer = cnn(i)

                Select Case layer.Type
                    Case LayerTypes.Convolution
                        current = WriteConvolution(graph, DirectCast(layer, ConvolutionLayer), current, options, counter)
                        geo = DirectCast(layer, ConvolutionLayer).OutputShape
                        shape = {1, geo.depth, geo.outY, geo.outX}
                        featureMap = True

                    Case LayerTypes.Conv2DTranspose
                        current = WriteConvTranspose(graph, DirectCast(layer, Conv2DTransposeLayer), current, options, counter)
                        geo = DirectCast(layer, Conv2DTransposeLayer).OutputShape
                        shape = {1, geo.depth, geo.outY, geo.outX}
                        featureMap = True

                    Case LayerTypes.Pool
                        current = WritePool(graph, DirectCast(layer, PoolingLayer), current, counter)
                        geo = DirectCast(layer, PoolingLayer).OutputShape
                        shape = {1, geo.depth, geo.outY, geo.outX}
                        featureMap = True

                    Case LayerTypes.FullyConnected
                        current = WriteFullyConnected(graph, DirectCast(layer, FullyConnectedLayer), current, geo, featureMap, options, counter)
                        geo = DirectCast(layer, FullyConnectedLayer).OutputShape
                        shape = {1, geo.depth}
                        featureMap = False

                    Case LayerTypes.ReLU
                        Call AssertPlainRelu(DirectCast(layer, RectifiedLinearUnitsLayer))
                        current = Activation(graph, "Relu", current, counter)

                    Case LayerTypes.LeakyReLU
                        current = Activation(graph, "LeakyRelu", current, counter,
                                             New AttributeProto With {
                                                 .Name = "alpha",
                                                 .F = CSng(DirectCast(layer, LeakyReluLayer).Slope),
                                                 .Type = AttributeType.Float
                                             })

                    Case LayerTypes.Elu
                        current = Activation(graph, "Elu", current, counter,
                                             New AttributeProto With {
                                                 .Name = "alpha",
                                                 .F = CSng(DirectCast(layer, EluLayer).Alpha),
                                                 .Type = AttributeType.Float
                                             })

                    Case LayerTypes.Sigmoid
                        current = Activation(graph, "Sigmoid", current, counter)

                    Case LayerTypes.Tanh
                        current = Activation(graph, "Tanh", current, counter)

                    Case LayerTypes.LRN
                        Dim lrn As LocalResponseNormalizationLayer = DirectCast(layer, LocalResponseNormalizationLayer)

                        current = Activation(graph, "LRN", current, counter,
                                             New AttributeProto With {
                                                 .Name = "size",
                                                 .I = lrn.Size,
                                                 .Type = AttributeType.Int
                                             },
                                             New AttributeProto With {
                                                 .Name = "alpha",
                                                 .F = CSng(lrn.AlphaValue),
                                                 .Type = AttributeType.Float
                                             },
                                             New AttributeProto With {
                                                 .Name = "beta",
                                                 .F = CSng(lrn.BetaValue),
                                                 .Type = AttributeType.Float
                                             },
                                             New AttributeProto With {
                                                 .Name = "bias",
                                                 .F = CSng(lrn.BiasValue),
                                                 .Type = AttributeType.Float
                                             })

                    Case LayerTypes.Dropout
                        ' 推理阶段 dropout 是恒等映射，直接跳过（不产生节点）
                        Continue For

                    Case LayerTypes.Regression
                        ' 回归损失层在前向推理时是恒等映射，不产生节点
                        Continue For

                    Case LayerTypes.SoftMax
                        current = Activation(graph, "Softmax", current, counter,
                                             New AttributeProto With {
                                                 .Name = "axis",
                                                 .I = -1,
                                                 .Type = AttributeType.Int
                                             })

                    Case LayerTypes.Input
                        Continue For

                    Case Else
                        Throw New NotSupportedException(
                            $"无法把 {layer.Type} 层导出为 ONNX 标准算子（第 {i} 层：{layer}）")
                End Select

                counter += 1
            Next

            ' 统一用一个 Identity 节点把最终张量命名为 "output"，
            ' 这样导出的模型输出名固定，便于下游的仿真器 / 脚本直接引用
            Dim finalize As New NodeProto With {
                .OpType = "Identity",
                .Name = "output"
            }

            Call finalize.Input.Add(current)
            Call finalize.Output.Add("output")
            Call graph.Node.Add(finalize)
            Call graph.Output.Add(ValueInfo("output", ElemType(options), shape))

            Dim model As New ModelProto With {
                .IrVersion = options.IrVersion,
                .ProducerName = options.ProducerName,
                .ProducerVersion = options.ProducerVersion,
                .Graph = graph
            }

            Call model.OpsetImport.Add(New OperatorSetIdProto With {.Domain = "", .Version = options.Opset})
            model.MetadataProps("layers") = cnn.LayerNum.ToString()
            model.MetadataProps("exported_by") = "Microsoft.VisualBasic.MachineLearning.DataStorage.ONNX"

            Return New OnnxModel(model)
        End Function

        ''' <summary>
        ''' 把卷积网络导出为 ONNX 文件。
        ''' </summary>
        ''' <param name="cnn">待导出的网络。</param>
        ''' <param name="path">目标文件路径，通常以 <c>.onnx</c> 结尾。</param>
        ''' <param name="options">导出选项。</param>
        Public Sub Save(cnn As ConvolutionalNN, path As String, Optional options As OnnxExportOptions = Nothing)
            Call ToModel(cnn, options).Save(path)
        End Sub

        ''' <summary>
        ''' 把卷积网络导出到数据流。
        ''' </summary>
        ''' <param name="cnn">待导出的网络。</param>
        ''' <param name="stream">目标数据流。</param>
        ''' <param name="options">导出选项。</param>
        ''' <param name="leaveOpen">写完后是否保持数据流打开。</param>
        Public Sub Write(cnn As ConvolutionalNN, stream As Stream, Optional options As OnnxExportOptions = Nothing, Optional leaveOpen As Boolean = True)
            Call ToModel(cnn, options).Save(stream, leaveOpen)
        End Sub

        Private Sub AssertPlainRelu(layer As RectifiedLinearUnitsLayer)
            If layer.ThresholdValue <> 0 Then
                Throw New NotSupportedException(
                    $"带有非零阈值（{layer.ThresholdValue}）的 ReLU 无法映射为标准 ONNX 的 Relu 算子")
            End If
        End Sub

        ''' <summary>权重精度对应的张量元素类型。</summary>
        Private Function ElemType(options As OnnxExportOptions) As TensorDataType
            If options.WeightPrecision = TensorDataType.Float64 Then
                Return TensorDataType.Float64
            Else
                Return TensorDataType.Float
            End If
        End Function

        Private Function ValueInfo(name As String, elemType As TensorDataType, dims As Integer()) As ValueInfoProto
            Dim shape As New TensorShapeProto()

            For Each d As Integer In dims
                Call shape.Dims.Add(New TensorShapeDimension With {.DimValue = d})
            Next

            Return New ValueInfoProto With {
                .Name = name,
                .Type = New TypeProto With {
                    .TensorType = New TensorTypeInfo With {
                        .ElemType = elemType,
                        .Shape = shape
                    }
                }
            }
        End Function

        Private Function Activation(graph As GraphProto, opType As String, input As String, counter As Integer,
                                   ParamArray attributes As AttributeProto()) As String

            Dim output As String = $"{opType.ToLowerInvariant()}_{counter}"
            Dim node As New NodeProto With {
                .OpType = opType,
                .Name = $"{opType}_{counter}"
            }

            Call node.Input.Add(input)
            Call node.Output.Add(output)

            If attributes IsNot Nothing Then
                For Each attr As AttributeProto In attributes
                    Call node.Attribute.Add(attr)
                Next
            End If

            Call graph.Node.Add(node)

            Return output
        End Function

        Private Function IntAttribute(name As String, ParamArray values As Integer()) As AttributeProto
            Dim attr As New AttributeProto With {
                .Name = name,
                .Type = AttributeType.Ints
            }

            For Each v As Integer In values
                Call attr.Ints.Add(v)
            Next

            Return attr
        End Function

        Private Function WriteConvolution(graph As GraphProto, layer As ConvolutionLayer, input As String,
                                          options As OnnxExportOptions, counter As Integer) As String

            Dim outShape As OutputDefinition = layer.OutputShape
            Dim inShape As OutputDefinition = layer.InputShape
            Dim kernel As Integer = layer.KernelSize
            Dim outChannels As Integer = outShape.depth
            Dim inChannels As Integer = inShape.depth
            Dim params As BackPropResult() = layer.BackPropagationResult.ToArray()
            Dim weights As Double() = New Double(outChannels * inChannels * kernel * kernel - 1) {}
            Dim bias As Double() = New Double(outChannels - 1) {}

            ' 框架的卷积核布局是 (KH, KW, C)，ONNX 要求 (OutC, C, KH, KW)
            For m As Integer = 0 To outChannels - 1
                Dim src As Double() = params(m).Weights

                For kh As Integer = 0 To kernel - 1
                    For kw As Integer = 0 To kernel - 1
                        For c As Integer = 0 To inChannels - 1
                            weights(((m * inChannels + c) * kernel + kh) * kernel + kw) = src((kernel * kh + kw) * inChannels + c)
                        Next
                    Next
                Next
            Next

            Call Array.Copy(params(outChannels).Weights, bias, outChannels)

            Dim weightName As String = $"conv{counter}.weight"
            Dim biasName As String = $"conv{counter}.bias"

            Call graph.Initializer.Add(OnnxTensor.FromArray(weights, outChannels, inChannels, kernel, kernel) _
                .ToTensorProto(weightName, options.WeightPrecision))
            Call graph.Initializer.Add(OnnxTensor.FromArray(bias, outChannels) _
                .ToTensorProto(biasName, options.WeightPrecision))

            Dim output As String = $"conv_{counter}"
            Dim node As New NodeProto With {
                .OpType = "Conv",
                .Name = $"Conv_{counter}"
            }

            Call node.Input.Add(input)
            Call node.Input.Add(weightName)
            Call node.Input.Add(biasName)
            Call node.Output.Add(output)
            Call node.Attribute.Add(IntAttribute("strides", layer.StrideSize, layer.StrideSize))
            Call node.Attribute.Add(IntAttribute("pads", layer.PaddingSize, layer.PaddingSize, layer.PaddingSize, layer.PaddingSize))
            Call node.Attribute.Add(IntAttribute("kernel_shape", kernel, kernel))
            Call graph.Node.Add(node)

            Return output

        End Function

        Private Function WriteConvTranspose(graph As GraphProto, layer As Conv2DTransposeLayer, input As String,
                                            options As OnnxExportOptions, counter As Integer) As String

            Dim outShape As OutputDefinition = layer.OutputShape
            Dim inShape As OutputDefinition = layer.InputShape
            Dim kernel As Integer = layer.KernelSize
            Dim outChannels As Integer = outShape.depth
            Dim inChannels As Integer = inShape.depth
            Dim params As BackPropResult() = layer.BackPropagationResult.ToArray()
            Dim weights As Double() = New Double(inChannels * outChannels * kernel * kernel - 1) {}
            Dim bias As Double() = New Double(outChannels - 1) {}

            ' 框架里第 d 个 filter 对应**输出通道** d，内部布局 (KH, KW, Cin)；
            ' ONNX 的 ConvTranspose 要求 (Cin, Cout, KH, KW)
            For d As Integer = 0 To outChannels - 1
                Dim src As Double() = params(d).Weights

                For kh As Integer = 0 To kernel - 1
                    For kw As Integer = 0 To kernel - 1
                        For c As Integer = 0 To inChannels - 1
                            weights(((c * outChannels + d) * kernel + kh) * kernel + kw) = src((kernel * kh + kw) * inChannels + c)
                        Next
                    Next
                Next
            Next

            Call Array.Copy(params(outChannels).Weights, bias, outChannels)

            Dim weightName As String = $"convT{counter}.weight"
            Dim biasName As String = $"convT{counter}.bias"

            Call graph.Initializer.Add(OnnxTensor.FromArray(weights, inChannels, outChannels, kernel, kernel) _
                .ToTensorProto(weightName, options.WeightPrecision))
            Call graph.Initializer.Add(OnnxTensor.FromArray(bias, outChannels) _
                .ToTensorProto(biasName, options.WeightPrecision))

            Dim output As String = $"convT_{counter}"
            Dim node As New NodeProto With {
                .OpType = "ConvTranspose",
                .Name = $"ConvTranspose_{counter}"
            }

            Call node.Input.Add(input)
            Call node.Input.Add(weightName)

            If layer.BiasEnabled Then
                Call node.Input.Add(biasName)
            End If

            Call node.Output.Add(output)
            Call node.Attribute.Add(IntAttribute("strides", layer.StrideSize, layer.StrideSize))
            Call node.Attribute.Add(IntAttribute("pads", layer.PaddingSize, layer.PaddingSize, layer.PaddingSize, layer.PaddingSize))
            Call node.Attribute.Add(IntAttribute("kernel_shape", kernel, kernel))
            Call graph.Node.Add(node)

            Return output

        End Function

        Private Function WritePool(graph As GraphProto, layer As PoolingLayer, input As String, counter As Integer) As String
            Dim output As String = $"pool_{counter}"
            Dim node As New NodeProto With {
                .OpType = "MaxPool",
                .Name = $"MaxPool_{counter}"
            }

            Call node.Input.Add(input)
            Call node.Output.Add(output)
            Call node.Attribute.Add(IntAttribute("kernel_shape", layer.KernelSize, layer.KernelSize))
            Call node.Attribute.Add(IntAttribute("strides", layer.StrideSize, layer.StrideSize))
            Call node.Attribute.Add(IntAttribute("pads", layer.PaddingSize, layer.PaddingSize, layer.PaddingSize, layer.PaddingSize))
            Call graph.Node.Add(node)

            Return output

        End Function

        Private Function WriteFullyConnected(graph As GraphProto, layer As FullyConnectedLayer, input As String,
                                             inputShape As OutputDefinition, isFeatureMap As Boolean,
                                             options As OnnxExportOptions, counter As Integer) As String

            Dim outSize As Integer = layer.OutputSize
            Dim inSize As Integer = layer.InputLength
            Dim params As BackPropResult() = layer.BackPropagationResult.ToArray()
            Dim weights As Double() = New Double(outSize * inSize - 1) {}
            Dim bias As Double() = New Double(outSize - 1) {}
            Dim tensorName As String = input

            If isFeatureMap AndAlso inputShape IsNot Nothing Then
                Dim channelLast As Boolean = options.ChannelLastFlatten
                Dim suffix As Integer = counter

                If channelLast Then
                    Dim perm As New AttributeProto With {
                        .Name = "perm",
                        .Type = AttributeType.Ints
                    }

                    ' perm = [0, 2, 3, 1] : NCHW -> NHWC
                    Call perm.Ints.Add(0)
                    Call perm.Ints.Add(2)
                    Call perm.Ints.Add(3)
                    Call perm.Ints.Add(1)

                    tensorName = EmitNode(graph, "Transpose", $"Transpose_{suffix}", {tensorName}, {perm})
                End If

                tensorName = EmitNode(graph, "Flatten", $"Flatten_{suffix}", {tensorName},
                                      {New AttributeProto With {.Name = "axis", .I = 1, .Type = AttributeType.Int}})

                If Not channelLast Then
                    ' 特征图按 (c, y, x) 展平，而框架的权重行是 (y, x, c) 顺序，需要重排
                    Dim width As Integer = inputShape.outX
                    Dim height As Integer = inputShape.outY
                    Dim channels As Integer = inputShape.depth

                    For i As Integer = 0 To outSize - 1
                        Dim src As Double() = params(i).Weights

                        For c As Integer = 0 To channels - 1
                            For y As Integer = 0 To height - 1
                                For x As Integer = 0 To width - 1
                                    weights(i * inSize + (c * height + y) * width + x) = src((y * width + x) * channels + c)
                                Next
                            Next
                        Next
                    Next
                End If
            End If

            If options.ChannelLastFlatten OrElse Not isFeatureMap Then
                For i As Integer = 0 To outSize - 1
                    Call Array.Copy(params(i).Weights, 0, weights, i * inSize, inSize)
                Next
            End If

            Call Array.Copy(params(outSize).Weights, bias, outSize)

            Dim weightName As String = $"fc{counter}.weight"
            Dim biasName As String = $"fc{counter}.bias"

            Call graph.Initializer.Add(OnnxTensor.FromArray(weights, outSize, inSize) _
                .ToTensorProto(weightName, options.WeightPrecision))
            Call graph.Initializer.Add(OnnxTensor.FromArray(bias, outSize) _
                .ToTensorProto(biasName, options.WeightPrecision))

            Dim output As String = $"fc_{counter}"
            Dim node As New NodeProto With {
                .OpType = "Gemm",
                .Name = $"Gemm_{counter}"
            }

            Call node.Input.Add(tensorName)
            Call node.Input.Add(weightName)
            Call node.Input.Add(biasName)
            Call node.Output.Add(output)
            Call node.Attribute.Add(New AttributeProto With {
                .Name = "transB",
                .I = 1,
                .Type = AttributeType.Int
            })
            Call graph.Node.Add(node)

            Return output

        End Function

        ''' <summary>
        ''' 追加一个单输入/单输出的工具节点，并返回其输出张量名称。
        ''' </summary>
        Private Function EmitNode(graph As GraphProto, opType As String, nodeName As String,
                                  inputs As String(), ParamArray attributes As AttributeProto()) As String

            Dim output As String = $"{nodeName}_out"
            Dim node As New NodeProto With {
                .OpType = opType,
                .Name = nodeName
            }

            For Each name As String In inputs
                Call node.Input.Add(name)
            Next

            Call node.Output.Add(output)

            If attributes IsNot Nothing Then
                For Each attr As AttributeProto In attributes
                    Call node.Attribute.Add(attr)
                Next
            End If

            Call graph.Node.Add(node)

            Return output
        End Function

    End Module

End Namespace
