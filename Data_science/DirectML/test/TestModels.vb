' ------------------------------------------------------------------------
' 测试用的小型 ONNX 模型
'
' 手工构造一个 "Conv -> Relu -> GlobalAveragePool -> Flatten -> Gemm" 的图，
' 目的是让 DirectML 后端与 CPU 求值器在**完全相同的输入**上比对结果，
' 不依赖任何外部模型文件。
' ------------------------------------------------------------------------

Imports Microsoft.VisualBasic.MachineLearning.DataStorage.ONNX
Imports Microsoft.VisualBasic.MachineLearning.DataStorage.ONNX.Proto

Namespace DirectMLDemo

    Friend Module TestModels

        ''' <summary>输入张量名称。</summary>
        Public Const InputName As String = "x"
        ''' <summary>输出张量名称。</summary>
        Public Const OutputName As String = "out"

        ''' <summary>
        ''' 构造小型卷积网络模型。
        ''' </summary>
        ''' <param name="seed">权重随机种子。</param>
        Public Function CreateTinyConvNet(Optional seed As Integer = 42) As OnnxModel
            Dim random As New Random(seed)
            Dim graph As New GraphProto With {.Name = "tiny-convnet"}

            ' ---- 初始化器 ----
            Call graph.Initializer.Add(Tensor("W1", {2, 1, 3, 3}, random))
            Call graph.Initializer.Add(Tensor("B1", {2}, random))
            Call graph.Initializer.Add(Tensor("W2", {2, 2}, random))
            Call graph.Initializer.Add(Tensor("B2", {2}, random))

            ' ---- 输入 / 输出 ----
            Call graph.Input.Add(ValueInfo(InputName, {1, 1, 4, 4}))
            Call graph.Output.Add(ValueInfo(OutputName, {1, 2}))

            ' ---- 节点 ----
            Dim conv As New NodeProto With {.OpType = "Conv", .Name = "conv1"}
            Call conv.Input.AddRange({"x", "W1", "B1"})
            Call conv.Output.Add("c1")
            Call conv.Attribute.Add(Ints("pads", {1, 1, 1, 1}))
            Call conv.Attribute.Add(Ints("strides", {1, 1}))
            Call graph.Node.Add(conv)

            Call graph.Node.Add(Unary("Relu", "r1", "c1"))

            Dim gap As New NodeProto With {.OpType = "GlobalAveragePool", .Name = "gap"}
            Call gap.Input.Add("r1")
            Call gap.Output.Add("g1")
            Call graph.Node.Add(gap)

            Dim flatten As New NodeProto With {.OpType = "Flatten", .Name = "flatten"}
            Call flatten.Input.Add("g1")
            Call flatten.Output.Add("f1")
            Call flatten.Attribute.Add(Int("axis", 1))
            Call graph.Node.Add(flatten)

            Dim gemm As New NodeProto With {.OpType = "Gemm", .Name = "fc"}
            Call gemm.Input.AddRange({"f1", "W2", "B2"})
            Call gemm.Output.Add(OutputName)
            Call graph.Node.Add(gemm)

            Dim model As New ModelProto With {
                .IrVersion = 7,
                .ProducerName = "sciBASIC#",
                .ProducerVersion = "10.8",
                .Graph = graph
            }

            Call model.OpsetImport.Add(New OperatorSetIdProto With {.Domain = "", .Version = 13})

            Return New OnnxModel(model, "<generated>")
        End Function

        Private Function Unary(opType As String, output As String, input As String) As NodeProto
            Dim node As New NodeProto With {.OpType = opType, .Name = output}

            Call node.Input.Add(input)
            Call node.Output.Add(output)

            Return node
        End Function

        Private Function Int(name As String, value As Long) As AttributeProto
            Return New AttributeProto With {.Name = name, .Type = AttributeType.Int, .I = value}
        End Function

        Private Function Ints(name As String, values As Integer()) As AttributeProto
            Dim attr As New AttributeProto With {.Name = name, .Type = AttributeType.Ints}

            For Each v As Integer In values
                Call attr.Ints.Add(v)
            Next

            Return attr
        End Function

        Private Function Tensor(name As String, dims As Integer(), random As Random) As TensorProto
            Dim count As Integer = 1

            For Each d As Integer In dims
                count *= d
            Next

            Dim data As Double() = New Double(count - 1) {}

            For i As Integer = 0 To count - 1
                data(i) = (random.NextDouble() - 0.5) * 0.5
            Next

            Dim value As New OnnxTensor(dims, data)

            Return value.ToTensorProto(name, TensorDataType.Float)
        End Function

        Private Function ValueInfo(name As String, dims As Integer()) As ValueInfoProto
            Dim shape As New TensorShapeProto()

            For Each d As Integer In dims
                Call shape.Dims.Add(New TensorShapeDimension With {.DimValue = d})
            Next

            Return New ValueInfoProto With {
                .Name = name,
                .Type = New TypeProto With {
                    .TensorType = New TensorTypeInfo With {
                        .ElemType = TensorDataType.Float,
                        .Shape = shape
                    }
                }
            }
        End Function

    End Module

End Namespace
