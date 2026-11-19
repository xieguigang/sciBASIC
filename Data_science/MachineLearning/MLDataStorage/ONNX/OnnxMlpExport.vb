' /********************************************************************************/
'  Microsoft.VisualBasic.MachineLearning.DataStorage
'  Data_science\MachineLearning\MLDataStorage\ONNX\OnnxMlpExport.vb
' /********************************************************************************/

Imports System
Imports System.Linq
Imports Microsoft.VisualBasic.MachineLearning.ComponentModel.Activations
Imports Microsoft.VisualBasic.MachineLearning.DataStorage.ONNX.Proto
Imports MlpModel = Microsoft.VisualBasic.MachineLearning.MultilayerPerceptron.MultilayerPerceptron
Imports MlpDenseLayer = Microsoft.VisualBasic.MachineLearning.MultilayerPerceptron.DenseLayer
Imports MlpTask = Microsoft.VisualBasic.MachineLearning.MultilayerPerceptron.MlpTask
Imports std = System.Math

Namespace ONNX

    ''' <summary>
    ''' 把多层感知机导出为标准的 ONNX 前向推理图。
    ''' </summary>
    ''' <remarks>
    ''' 每个全连接层写出一个 <c>Gemm</c> 节点（权重初始化器形状为 [输出维度, 输入维度]，
    ''' <c>transB = 1</c>），其后跟随与隐藏层激活函数对应的标准算子。
    ''' 分类模型在末尾写出 <c>Softmax</c>，回归模型直接输出线性结果；
    ''' 最终统一通过 <c>Identity</c> 节点命名为 <c>output</c>，与 CNN 导出约定保持一致。
    ''' </remarks>
    Public Module OnnxMlpExport

        ''' <summary>构建多层感知机的 ONNX 模型对象（不写文件）。</summary>
        Public Function ToModel(mlp As MlpModel,
                                Optional options As OnnxExportOptions = Nothing) As OnnxModel
            If mlp Is Nothing Then
                Throw New ArgumentNullException(NameOf(mlp))
            End If

            Dim opts As OnnxExportOptions = options

            If opts Is Nothing Then
                opts = New OnnxExportOptions With {.ModelName = "mlp"}
            End If

            Dim graph As New GraphProto With {.Name = If(opts.ModelName, "mlp")}
            Call graph.Input.Add(ValueInfo("input", ElemType(opts), {1, mlp.InputSize}))

            Dim current As String = "input"
            Dim activations As New System.Collections.Generic.List(Of String)()

            For i As Integer = 0 To mlp.Layers.Count - 1
                Dim layer As MlpDenseLayer = mlp.Layers(i)
                Dim isOutput As Boolean = (i = mlp.Layers.Count - 1)
                Dim weightName As String = $"fc{i}.weight"
                Dim biasName As String = $"fc{i}.bias"
                Dim outputName As String = $"fc_{i}"

                Call graph.Initializer.Add(OnnxTensor _
                    .FromArray(layer.GetOnnxWeights(), layer.OutputSize, layer.InputSize) _
                    .ToTensorProto(weightName, opts.WeightPrecision))
                Call graph.Initializer.Add(OnnxTensor _
                    .FromArray(layer.Bias.Array, layer.OutputSize) _
                    .ToTensorProto(biasName, opts.WeightPrecision))

                Dim gemm As New NodeProto With {
                    .OpType = "Gemm",
                    .Name = $"Gemm_{i}"
                }
                Call gemm.Input.Add(current)
                Call gemm.Input.Add(weightName)
                Call gemm.Input.Add(biasName)
                Call gemm.Output.Add(outputName)
                Call gemm.Attribute.Add(New AttributeProto With {
                    .Name = "transB",
                    .I = 1,
                    .Type = AttributeType.Int
                })
                Call graph.Node.Add(gemm)

                current = outputName

                If isOutput Then
                    activations.Add("Identical()")
                    Continue For
                End If

                If layer.Activation Is Nothing Then
                    activations.Add("Identical()")
                    Continue For
                End If

                Dim expression As String = layer.ActivationExpression
                activations.Add(expression)
                current = WriteActivation(graph, layer.Activation, current, i, opts)
            Next

            If mlp.Task = MlpTask.Classification Then
                current = EmitNodeWithAttributes(graph, "Softmax", "Softmax", current,
                                   New AttributeProto With {
                                       .Name = "axis",
                                       .I = -1,
                                       .Type = AttributeType.Int
                                   })
            End If

            Dim finalize As New NodeProto With {
                .OpType = "Identity",
                .Name = "output"
            }
            Call finalize.Input.Add(current)
            Call finalize.Output.Add("output")
            Call graph.Node.Add(finalize)
            Call graph.Output.Add(ValueInfo("output", ElemType(opts), {1, mlp.OutputSize}))

            Dim model As New ModelProto With {
                .IrVersion = opts.IrVersion,
                .ProducerName = opts.ProducerName,
                .ProducerVersion = opts.ProducerVersion,
                .Graph = graph
            }
            Call model.OpsetImport.Add(New OperatorSetIdProto With {.Domain = "", .Version = opts.Opset})
            model.MetadataProps("task") = mlp.Task.ToString()
            model.MetadataProps("architecture") = mlp.ToString()
            model.MetadataProps("layers") = mlp.Layers.Count.ToString()
            model.MetadataProps("activations") = String.Join(";", activations)
            model.MetadataProps("exported_by") = "Microsoft.VisualBasic.MachineLearning.DataStorage.ONNX"

            Return New OnnxModel(model)
        End Function

        ''' <summary>把多层感知机保存为 ONNX 文件。</summary>
        Public Sub Save(mlp As MlpModel, path As String,
                        Optional options As OnnxExportOptions = Nothing)
            If String.IsNullOrEmpty(path) Then
                Throw New ArgumentException("ONNX 文件路径不能为空", NameOf(path))
            End If
            Call ToModel(mlp, options).Save(path)
        End Sub

        Private Function WriteActivation(graph As GraphProto, activation As IActivationFunction,
                                         input As String, counter As Integer,
                                         options As OnnxExportOptions) As String
            Dim name As String = If(activation.Store.name, "")

            Select Case name.ToLowerInvariant()
                Case "relu"
                    If activation.Store.HasKey("threshold") AndAlso activation.Store("threshold") <> 0 Then
                        Throw New NotSupportedException("带非零阈值的 ReLU 无法用标准 ONNX Relu 表示")
                    End If
                    Return EmitNode(graph, "Relu", $"Relu_{counter}", input)

                Case "elu"
                    Dim alpha As Double = If(activation.Store.HasKey("alpha"), activation.Store("alpha"), 1.0)

                    If std.Abs(alpha - 1.0) > 0.0000000001 Then
                        Return EmitNodeWithAttributes(graph, "Elu", $"Elu_{counter}", input,
                            New AttributeProto With {
                                .Name = "alpha",
                                .F = CSng(alpha),
                                .Type = AttributeType.Float
                            })
                    End If

                    Return EmitNode(graph, "Elu", $"Elu_{counter}", input)

                Case "hyperbolictangent"
                    Return EmitNode(graph, "Tanh", $"Tanh_{counter}", input)

                Case "softplus"
                    Return EmitNode(graph, "Softplus", $"Softplus_{counter}", input)

                Case "sigmoid"
                    Dim alpha As Double = If(activation.Store.HasKey("alpha"), activation.Store("alpha"), 1.0)

                    If std.Abs(alpha - 1.0) > 0.0000000001 Then
                        Dim scaleName As String = $"sigmoid_{counter}.alpha"
                        Call graph.Initializer.Add(OnnxTensor _
                            .FromArray({alpha}) _
                            .ToTensorProto(scaleName, TensorDataType.Float64))
                        input = EmitNode(graph, "Mul", $"Mul_{counter}", {input, scaleName})
                    End If

                    Return EmitNode(graph, "Sigmoid", $"Sigmoid_{counter}", input)

                Case "identical"
                    Return input

                Case Else
                    Throw New NotSupportedException($"激活函数 {name} 没有对应的标准 ONNX 算子")
            End Select
        End Function

        Private Function EmitNode(graph As GraphProto, opType As String, nodeName As String,
                                  ParamArray inputs As String()) As String
            Dim output As String = $"{nodeName}_out"
            Dim node As New NodeProto With {
                .OpType = opType,
                .Name = nodeName
            }

            For Each name As String In inputs
                Call node.Input.Add(name)
            Next

            Call node.Output.Add(output)
            Call graph.Node.Add(node)
            Return output
        End Function

        Private Function EmitNodeWithAttributes(graph As GraphProto, opType As String, nodeName As String,
                                                input As String, ParamArray attributes As AttributeProto()) As String
            Dim output As String = $"{nodeName}_out"
            Dim node As New NodeProto With {
                .OpType = opType,
                .Name = nodeName
            }
            Call node.Input.Add(input)
            Call node.Output.Add(output)

            For Each attribute As AttributeProto In attributes
                Call node.Attribute.Add(attribute)
            Next

            Call graph.Node.Add(node)
            Return output
        End Function

        Private Function ElemType(options As OnnxExportOptions) As TensorDataType
            Return If(options.WeightPrecision = TensorDataType.Float64,
                      TensorDataType.Float64, TensorDataType.Float)
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

    End Module

End Namespace
