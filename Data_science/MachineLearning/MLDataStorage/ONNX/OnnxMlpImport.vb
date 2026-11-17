' /********************************************************************************/
'  Microsoft.VisualBasic.MachineLearning.DataStorage
'  Data_science\MachineLearning\MLDataStorage\ONNX\OnnxMlpImport.vb
' /********************************************************************************/

Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports Microsoft.VisualBasic.MachineLearning.ComponentModel.Activations
Imports Microsoft.VisualBasic.MachineLearning.DataStorage.ONNX.Proto
Imports MlpModel = Microsoft.VisualBasic.MachineLearning.MultilayerPerceptron.MultilayerPerceptron
Imports MlpDenseLayer = Microsoft.VisualBasic.MachineLearning.MultilayerPerceptron.DenseLayer
Imports MlpTask = Microsoft.VisualBasic.MachineLearning.MultilayerPerceptron.MlpTask

Namespace ONNX

    ''' <summary>
    ''' 从 ONNX 前向图还原多层感知机。支持 Gemm 全连接层以及
    ''' Relu / Sigmoid / Tanh / Softplus / Softmax / Identity 等标准激活算子。
    ''' </summary>
    Public Module OnnxMlpImport

        ''' <summary>从 ONNX 模型对象还原多层感知机。</summary>
        Public Function ToMLP(model As OnnxModel,
                              Optional options As OnnxImportOptions = Nothing) As MlpModel
            If model Is Nothing Then
                Throw New ArgumentNullException(NameOf(model))
            End If
            If model.Graph Is Nothing Then
                Throw New ArgumentException("ONNX 模型缺少计算图", NameOf(model))
            End If

            Dim inputs As ValueInfoProto() = model.Inputs

            If inputs.Length <> 1 Then
                Throw New NotSupportedException(
                    $"只支持单输入的计算图，当前模型有 {inputs.Length} 个输入：" &
                    String.Join(", ", inputs.Select(Function(v) v.Name)))
            End If

            Dim dims As Long() = inputs(0).GetDims()

            If dims.Length < 2 Then
                Throw New NotSupportedException("多层感知机要求输入张量至少具有 batch 与 feature 两个维度")
            End If

            Dim inputSize As Integer = 1

            For i As Integer = 1 To dims.Length - 1
                If dims(i) <= 0 Then
                    Throw New NotSupportedException("无法从动态输入形状推断特征维度")
                End If
                inputSize *= CInt(dims(i))
            Next

            Dim specs As New List(Of LayerSpec)()
            Dim task As MlpTask = MlpTask.Regression
            Dim pendingAlpha As Double = 0.0
            Dim mean As Double() = Nothing
            Dim scale As Double() = Nothing
            Dim opts As OnnxImportOptions = If(options, New OnnxImportOptions())

            Dim metadata As Dictionary(Of String, String) = model.Metadata

            If metadata.ContainsKey("task") AndAlso
               String.Equals(metadata("task"), MlpTask.Classification.ToString(),
                             StringComparison.OrdinalIgnoreCase) Then
                task = MlpTask.Classification
            End If

            For Each node As NodeProto In model.Graph.Node
                Dim op As String = If(node.OpType, "").ToLowerInvariant()

                Select Case op
                    Case "gemm"
                        specs.Add(ReadGemm(model, node, inputSize, specs, mean, scale))
                        pendingAlpha = 0.0
                        mean = Nothing
                        scale = Nothing

                    Case "sub", "div"
                        If Not opts.FoldNormalizer Then
                            Throw New OnnxUnsupportedOperatorException(
                                node, "计算图带有输入归一化，但 FoldNormalizer = False，无法还原 MLP")
                        End If

                        Dim operand As OnnxTensor = ResolveOperand(model, node)

                        If op = "sub" Then
                            mean = operand.Data
                        Else
                            scale = operand.Data
                        End If

                    Case "relu"
                        RequireLayer(specs, node)
                        specs(specs.Count - 1).Activation = "ReLU()"

                    Case "elu"
                        RequireLayer(specs, node)
                        Dim alpha As Double = NodeAttributes.GetFloat(node, "alpha", 1.0)
                        specs(specs.Count - 1).Activation =
                            $"Elu(alpha:={alpha.ToString("R", System.Globalization.CultureInfo.InvariantCulture)})"

                    Case "tanh"
                        RequireLayer(specs, node)
                        specs(specs.Count - 1).Activation = "HyperbolicTangent()"

                    Case "softplus"
                        RequireLayer(specs, node)
                        specs(specs.Count - 1).Activation = "Softplus()"

                    Case "sigmoid"
                        RequireLayer(specs, node)
                        Dim expression As String = "Sigmoid(alpha:=1.0)"

                        If pendingAlpha <> 0.0 Then
                            expression = $"Sigmoid(alpha:={pendingAlpha.ToString("R", System.Globalization.CultureInfo.InvariantCulture)})"
                            pendingAlpha = 0.0
                        End If

                        specs(specs.Count - 1).Activation = expression

                    Case "mul"
                        pendingAlpha = ReadScalar(model, node)

                    Case "softmax"
                        task = MlpTask.Classification

                    Case "identity", "dropout", "flatten", "reshape"
                        ' 推理时均为恒等映射，不需要在 MLP 中表达

                    Case Else
                        Throw New OnnxUnsupportedOperatorException(node)
                End Select
            Next

            If specs.Count = 0 then
                Throw New NotSupportedException("计算图中没有 Gemm 层，无法还原多层感知机")
            End If

            Dim outputSize As Integer = specs(specs.Count - 1).OutputSize
            Dim hiddenSizes As Integer() = specs _
                .Take(specs.Count - 1) _
                .Select(Function(s) s.OutputSize) _
                .ToArray()
            Dim firstActivation As String = If(specs.Count > 1, specs(0).Activation, "ReLU()")
            Dim mlp As New MlpModel(inputSize, hiddenSizes, outputSize, task,
                                                ActiveFunction.Parse(firstActivation))

            For i As Integer = 0 To specs.Count - 1
                Dim layer As MlpDenseLayer = mlp.Layers(i)
                Call layer.SetOnnxParameters(specs(i).Weights, specs(i).Bias)

                If i < specs.Count - 1 Then
                    layer.Activation = ActiveFunction.Parse(specs(i).Activation).CreateFunction()
                Else
                    layer.Activation = Nothing
                End If
            Next

            Return mlp
        End Function

        ''' <summary>从 ONNX 文件还原多层感知机。</summary>
        Public Function Load(path As String,
                             Optional options As OnnxImportOptions = Nothing) As MlpModel
            Return ToMLP(OnnxModel.Load(path), options)
        End Function

        ''' <summary>从数据流还原多层感知机。</summary>
        Public Function LoadFrom(stream As Stream,
                                 Optional leaveOpen As Boolean = True,
                                 Optional options As OnnxImportOptions = Nothing) As MlpModel
            Return ToMLP(OnnxModel.FromStream(stream, leaveOpen), options)
        End Function

        Private Function ReadGemm(model As OnnxModel, node As NodeProto,
                                  expectedInput As Integer, specs As List(Of LayerSpec),
                                  mean As Double(), scale As Double()) As LayerSpec
            If node.Input.Count < 2 Then
                Throw New OnnxUnsupportedOperatorException(node, "Gemm 节点缺少权重输入")
            End If

            Dim weight As OnnxTensor = Nothing

            If Not model.Initializers.TryGetValue(node.Input(1), weight) Then
                Throw New OnnxUnsupportedOperatorException(node, $"Gemm 权重 {node.Input(1)} 不是初始化器")
            End If

            If weight.Rank <> 2 Then
                Throw New OnnxUnsupportedOperatorException(node, "仅支持二维的 Gemm 权重张量")
            End If

            Dim transB As Integer = CInt(NodeAttributes.GetInt(node, "transB", 1))
            Dim inSize As Integer = If(transB <> 0, weight.Dims(1), weight.Dims(0))
            Dim outSize As Integer = If(transB <> 0, weight.Dims(0), weight.Dims(1))
            Dim values As Double() = New Double(outSize * inSize - 1) {}

            For i As Integer = 0 To outSize - 1
                For j As Integer = 0 To inSize - 1
                    Dim source As Integer = If(transB <> 0, i * inSize + j, j * outSize + i)
                    values(i * inSize + j) = weight.Data(source)
                Next
            Next

            Dim bias As Double() = New Double(outSize - 1) {}

            If node.Input.Count > 2 AndAlso model.Initializers.ContainsKey(node.Input(2)) Then
                Dim biasTensor As OnnxTensor = model.Initializers(node.Input(2))

                If biasTensor.Data.Length <> outSize Then
                    Throw New OnnxUnsupportedOperatorException(node, "Gemm 偏置长度与权重输出维度不一致")
                End If

                Call Array.Copy(biasTensor.Data, bias, outSize)
            End If

            If specs.Count = 0 AndAlso (mean IsNot Nothing OrElse scale IsNot Nothing) Then
                If mean IsNot Nothing AndAlso mean.Length <> inSize Then
                    Throw New OnnxUnsupportedOperatorException(node, "归一化均值长度与输入维度不一致")
                End If
                If scale IsNot Nothing AndAlso scale.Length <> inSize Then
                    Throw New OnnxUnsupportedOperatorException(node, "归一化尺度长度与输入维度不一致")
                End If

                For j As Integer = 0 To outSize - 1
                    Dim correction As Double = 0.0

                    For i As Integer = 0 To inSize - 1
                        Dim s As Double = If(scale Is Nothing, 1.0, scale(i))
                        Dim m As Double = If(mean Is Nothing, 0.0, mean(i))

                        If s = 0.0 Then
                            Throw New OnnxUnsupportedOperatorException(node, "归一化尺度不能为零")
                        End If

                        Dim w As Double = values(j * inSize + i) / s
                        values(j * inSize + i) = w
                        correction += w * m
                    Next

                    bias(j) -= correction
                Next
            End If

            Dim expected As Integer = If(specs.Count = 0, expectedInput, specs(specs.Count - 1).OutputSize)

            If inSize <> expected Then
                Throw New OnnxUnsupportedOperatorException(
                    node, $"Gemm 输入维度为 {inSize}，但上一层输出为 {expected}")
            End If

            Return New LayerSpec With {
                .InputSize = inSize,
                .OutputSize = outSize,
                .Weights = values,
                .Bias = bias
            }
        End Function

        ''' <summary>从归一化算子的输入中找出常量张量（均值或尺度）。</summary>
        Private Function ResolveOperand(model As OnnxModel, node As NodeProto) As OnnxTensor
            For i As Integer = 1 To node.Input.Count - 1
                Dim tensor As OnnxTensor = Nothing

                If model.Initializers.TryGetValue(node.Input(i), tensor) Then
                    Return tensor
                End If
            Next

            If node.Input.Count > 0 Then
                Dim tensor As OnnxTensor = Nothing

                If model.Initializers.TryGetValue(node.Input(0), tensor) Then
                    Return tensor
                End If
            End If

            Throw New OnnxUnsupportedOperatorException(node, "归一化算子缺少常量输入")
        End Function

        Private Function ReadScalar(model As OnnxModel, node As NodeProto) As Double
            For Each name As String In node.Input
                Dim tensor As OnnxTensor = Nothing

                If model.Initializers.TryGetValue(name, tensor) AndAlso tensor.Data.Length = 1 Then
                    Return tensor.Data(0)
                End If
            Next

            Throw New OnnxUnsupportedOperatorException(node, "Mul 节点应当包含一个标量初始化器")
        End Function

        Private Sub RequireLayer(specs As List(Of LayerSpec), node As NodeProto)
            If specs.Count = 0 Then
                Throw New OnnxUnsupportedOperatorException(node, "激活算子之前必须存在 Gemm 层")
            End If
        End Sub

        Private Class LayerSpec
            Public Property InputSize As Integer
            Public Property OutputSize As Integer
            Public Property Weights As Double()
            Public Property Bias As Double()
            Public Property Activation As String = "Identical()"
        End Class

    End Module

End Namespace
