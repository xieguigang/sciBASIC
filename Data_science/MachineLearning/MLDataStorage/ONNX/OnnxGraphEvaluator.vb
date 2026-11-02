' /********************************************************************************/
'  Microsoft.VisualBasic.MachineLearning.DataStorage
'  Data_science\MachineLearning\MLDataStorage\ONNX\OnnxGraphEvaluator.vb
' /********************************************************************************/

Imports System.Linq
Imports Microsoft.VisualBasic.MachineLearning.DataStorage.ONNX.Proto

Namespace ONNX

    ''' <summary>
    ''' 一个轻量的 ONNX 计算图前向求值器：按节点顺序执行，不需要任何第三方推理引擎。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 求值环境是一个"张量名称 → 张量"的字典，初始内容由模型的初始化器（常量权重）与调用方提供的输入组成；
    ''' 之后每个节点把自己的输出写回字典，因此节点必须按拓扑顺序排列（ONNX 规范要求如此）。
    ''' </para>
    ''' <para>
    ''' 不支持的算子会抛出 <see cref="OnnxUnsupportedOperatorException"/>，其中带有算子名与节点名，
    ''' 而不是静默跳过——静默跳过会让推理结果错得难以察觉。
    ''' </para>
    ''' </remarks>
    Public Class OnnxGraphEvaluator

        ''' <summary>被求值的模型。</summary>
        Public ReadOnly Property Model As OnnxModel

        ''' <summary>
        ''' 创建一个求值器。
        ''' </summary>
        ''' <param name="model">被求值的模型。</param>
        Public Sub New(model As OnnxModel)
            If model Is Nothing Then
                Throw New ArgumentNullException(NameOf(model))
            End If

            Me.Model = model
        End Sub

        ''' <summary>
        ''' 执行一次前向计算。
        ''' </summary>
        ''' <param name="inputs">输入张量（键为图输入的名称）。</param>
        ''' <returns>图输出张量（键为图输出的名称）。</returns>
        Public Function Run(inputs As IDictionary(Of String, OnnxTensor)) As Dictionary(Of String, OnnxTensor)
            Dim env As New Dictionary(Of String, OnnxTensor)(StringComparer.Ordinal)

            For Each item As KeyValuePair(Of String, OnnxTensor) In Model.Initializers
                env(item.Key) = item.Value
            Next

            If inputs IsNot Nothing Then
                For Each item As KeyValuePair(Of String, OnnxTensor) In inputs
                    env(item.Key) = item.Value
                Next
            End If

            For Each required As ValueInfoProto In Model.Inputs
                If Not env.ContainsKey(required.Name) Then
                    Throw New ArgumentException(
                        $"缺少模型输入 ""{required.Name}""（{required}）")
                End If
            Next

            For Each node As NodeProto In Model.Graph.Node
                Dim outputs As OnnxTensor() = Evaluate(node, env)

                For i As Integer = 0 To node.Output.Count - 1
                    If i >= outputs.Length OrElse outputs(i) Is Nothing Then
                        Continue For
                    End If

                    If String.IsNullOrEmpty(node.Output(i)) Then
                        Continue For
                    End If

                    env(node.Output(i)) = outputs(i)
                Next
            Next

            Dim result As New Dictionary(Of String, OnnxTensor)(StringComparer.Ordinal)

            For Each output As ValueInfoProto In Model.Outputs
                Dim tensor As OnnxTensor = Nothing

                If Not env.TryGetValue(output.Name, tensor) Then
                    Throw New InvalidOperationException($"图输出 ""{output.Name}"" 没有被任何节点计算出来")
                End If

                result(output.Name) = tensor
            Next

            Return result
        End Function

        ''' <summary>
        ''' 执行一次前向计算，返回第一个输出的数值。
        ''' </summary>
        ''' <param name="input">单输入模型的输入数值；会被包装成 <c>[1, n]</c> 的浮点张量。</param>
        ''' <returns>第一个图输出的数值。</returns>
        Public Function RunSingle(input As Double()) As Double()
            Dim names As ValueInfoProto() = Model.Inputs

            If names.Length <> 1 Then
                Throw New InvalidOperationException(
                    $"RunSingle 只能用于单输入模型，当前模型有 {names.Length} 个输入：" &
                    String.Join(", ", names.Select(Function(v) v.Name)))
            End If

            Dim dims As Integer() = names(0).GetDims().Select(Function(v) CInt(v)).ToArray()

            If dims.Length = 0 Then
                dims = New Integer() {1, input.Length}
            Else
                Dim total As Integer = 1

                For i As Integer = 0 To dims.Length - 1
                    If dims(i) < 0 Then
                        dims(i) = If(i = 0, 1, input.Length)
                    End If

                    total *= dims(i)
                Next

                If total <> input.Length Then
                    Throw New ArgumentException(
                        $"模型输入 ""{names(0).Name}"" 的形状为 [{String.Join(", ", dims)}]，需要 {total} 个数值，但实际提供了 {input.Length} 个")
                End If
            End If

            Dim env As New Dictionary(Of String, OnnxTensor)(StringComparer.Ordinal)

            env(names(0).Name) = New OnnxTensor(dims, input)

            Dim outputs As Dictionary(Of String, OnnxTensor) = Run(env)
            Dim first As OnnxTensor = outputs.Values.First()

            Return first.Data
        End Function

        ''' <summary>
        ''' 求值单个节点。
        ''' </summary>
        ''' <param name="node">节点。</param>
        ''' <param name="env">当前的张量环境。</param>
        ''' <returns>该节点的输出张量集合。</returns>
        Public Shared Function Evaluate(node As NodeProto, env As IDictionary(Of String, OnnxTensor)) As OnnxTensor()
            Dim op As String = If(node.OpType, "").ToLowerInvariant()

            Select Case op
                Case "identity", "dropout"
                    ' Dropout 在推理阶段是恒等映射（第二个输出是掩码，这里不产出）
                    Return New OnnxTensor() {Input(node, 0, env), Nothing}

                Case "constant"
                    Return New OnnxTensor() {Constant(node)}

                Case "add"
                    Return New OnnxTensor() {OnnxOperators.Add(Input(node, 0, env), Input(node, 1, env))}

                Case "sub"
                    Return New OnnxTensor() {OnnxOperators.Subtract(Input(node, 0, env), Input(node, 1, env))}

                Case "mul"
                    Return New OnnxTensor() {OnnxOperators.Multiply(Input(node, 0, env), Input(node, 1, env))}

                Case "div"
                    Return New OnnxTensor() {OnnxOperators.Divide(Input(node, 0, env), Input(node, 1, env))}

                Case "pow"
                    Return New OnnxTensor() {OnnxOperators.Power(Input(node, 0, env), Input(node, 1, env))}

                Case "matmul"
                    Return New OnnxTensor() {OnnxOperators.MatMul(Input(node, 0, env), Input(node, 1, env))}

                Case "gemm"
                    Dim c As OnnxTensor = OptionalInput(node, 2, env)

                    Return New OnnxTensor() {
                        OnnxOperators.Gemm(Input(node, 0, env), Input(node, 1, env), c,
                                           NodeAttributes.GetFloat(node, "alpha", 1.0),
                                           NodeAttributes.GetFloat(node, "beta", 1.0),
                                           NodeAttributes.GetInt(node, "transA") <> 0,
                                           NodeAttributes.GetInt(node, "transB") <> 0)
                    }

                Case "relu"
                    Return New OnnxTensor() {OnnxOperators.Relu(Input(node, 0, env))}

                Case "leakyrelu"
                    Return New OnnxTensor() {OnnxOperators.LeakyRelu(Input(node, 0, env), NodeAttributes.GetFloat(node, "alpha", 0.01))}

                Case "elu"
                    Return New OnnxTensor() {OnnxOperators.Elu(Input(node, 0, env), NodeAttributes.GetFloat(node, "alpha", 1.0))}

                Case "selu"
                    Return New OnnxTensor() {
                        OnnxOperators.Selu(Input(node, 0, env),
                                           NodeAttributes.GetFloat(node, "alpha", 1.6732631921768188),
                                           NodeAttributes.GetFloat(node, "gamma", 1.0507010221481323))
                    }

                Case "sigmoid"
                    Return New OnnxTensor() {OnnxOperators.Sigmoid(Input(node, 0, env))}

                Case "hardsigmoid"
                    Return New OnnxTensor() {
                        OnnxOperators.HardSigmoid(Input(node, 0, env),
                                                  NodeAttributes.GetFloat(node, "alpha", 0.2),
                                                  NodeAttributes.GetFloat(node, "beta", 0.5))
                    }

                Case "tanh"
                    Return New OnnxTensor() {OnnxOperators.Tanh(Input(node, 0, env))}

                Case "softplus"
                    Return New OnnxTensor() {OnnxOperators.Softplus(Input(node, 0, env))}

                Case "softsign"
                    Return New OnnxTensor() {OnnxOperators.Softsign(Input(node, 0, env))}

                Case "exp"
                    Return New OnnxTensor() {OnnxOperators.Exp(Input(node, 0, env))}

                Case "log"
                    Return New OnnxTensor() {OnnxOperators.Log(Input(node, 0, env))}

                Case "sqrt"
                    Return New OnnxTensor() {OnnxOperators.Sqrt(Input(node, 0, env))}

                Case "neg"
                    Return New OnnxTensor() {OnnxOperators.Negate(Input(node, 0, env))}

                Case "abs"
                    Return New OnnxTensor() {OnnxOperators.Abs(Input(node, 0, env))}

                Case "floor"
                    Return New OnnxTensor() {OnnxOperators.Floor(Input(node, 0, env))}

                Case "ceil"
                    Return New OnnxTensor() {OnnxOperators.Ceiling(Input(node, 0, env))}

                Case "softmax"
                    Return New OnnxTensor() {OnnxOperators.Softmax(Input(node, 0, env), CInt(NodeAttributes.GetInt(node, "axis", -1)))}

                Case "cast"
                    ' 内部统一使用 double，数值转换在这里是恒等操作
                    Return New OnnxTensor() {Input(node, 0, env)}

                Case "conv"
                    Return New OnnxTensor() {
                        OnnxOperators.Conv(Input(node, 0, env), Input(node, 1, env), OptionalInput(node, 2, env),
                                           NodeAttributes.GetInts(node, "strides"),
                                           NodeAttributes.GetInts(node, "pads"),
                                           NodeAttributes.GetInts(node, "dilations"),
                                           CInt(NodeAttributes.GetInt(node, "group", 1)))
                    }

                Case "convtranspose"
                    Return New OnnxTensor() {
                        OnnxOperators.ConvTranspose(Input(node, 0, env), Input(node, 1, env), OptionalInput(node, 2, env),
                                                    NodeAttributes.GetInts(node, "strides"),
                                                    NodeAttributes.GetInts(node, "pads"))
                    }

                Case "maxpool"
                    Return New OnnxTensor() {
                        OnnxOperators.MaxPool(Input(node, 0, env),
                                              NodeAttributes.GetInts(node, "kernel_shape"),
                                              NodeAttributes.GetInts(node, "strides"),
                                              NodeAttributes.GetInts(node, "pads"))
                    }

                Case "averagepool"
                    Return New OnnxTensor() {
                        OnnxOperators.AveragePool(Input(node, 0, env),
                                                  NodeAttributes.GetInts(node, "kernel_shape"),
                                                  NodeAttributes.GetInts(node, "strides"),
                                                  NodeAttributes.GetInts(node, "pads"))
                    }

                Case "globalaveragepool"
                    Return New OnnxTensor() {OnnxOperators.GlobalAveragePool(Input(node, 0, env))}

                Case "lrn"
                    Return New OnnxTensor() {
                        OnnxOperators.LRN(Input(node, 0, env),
                                          CInt(NodeAttributes.GetInt(node, "size", 5)),
                                          NodeAttributes.GetFloat(node, "alpha", 0.0001),
                                          NodeAttributes.GetFloat(node, "beta", 0.75),
                                          NodeAttributes.GetFloat(node, "bias", 1.0))
                    }

                Case "flatten"
                    Return New OnnxTensor() {OnnxOperators.Flatten(Input(node, 0, env), CInt(NodeAttributes.GetInt(node, "axis", 1)))}

                Case "reshape"
                    Dim shape As OnnxTensor = Input(node, 1, env)
                    Dim target As Integer() = shape.Data.Select(Function(x) CInt(x)).ToArray()

                    Return New OnnxTensor() {OnnxOperators.Reshape(Input(node, 0, env), target)}

                Case "transpose"
                    Dim perm As Integer() = NodeAttributes.GetInts(node, "perm")
                    Dim x As OnnxTensor = Input(node, 0, env)

                    If perm Is Nothing OrElse perm.Length = 0 Then
                        perm = Enumerable.Range(0, x.Rank).Reverse().ToArray()
                    End If

                    Return New OnnxTensor() {x.Transpose(perm)}

                Case "concat"
                    Dim items As New List(Of OnnxTensor)

                    For Each name As String In node.Input
                        If Not String.IsNullOrEmpty(name) Then
                            Call items.Add(Resolve(name, env, node))
                        End If
                    Next

                    Return New OnnxTensor() {OnnxOperators.Concat(CInt(NodeAttributes.GetInt(node, "axis", 0)), items.ToArray())}

                Case "squeeze", "unsqueeze"
                    Dim axes As Integer() = NodeAttributes.GetInts(node, "axes")
                    Dim x As OnnxTensor = Input(node, 0, env)

                    If axes Is Nothing OrElse axes.Length = 0 Then
                        Dim axesTensor As OnnxTensor = OptionalInput(node, 1, env)

                        If axesTensor IsNot Nothing Then
                            axes = axesTensor.Data.Select(Function(v) CInt(v)).ToArray()
                        End If
                    End If

                    If op = "squeeze" Then
                        Return New OnnxTensor() {OnnxOperators.Squeeze(x, axes)}
                    Else
                        If axes Is Nothing OrElse axes.Length = 0 Then
                            Throw New OnnxUnsupportedOperatorException(node, "Unsqueeze 缺少 axes 参数")
                        End If

                        Return New OnnxTensor() {OnnxOperators.Unsqueeze(x, axes)}
                    End If

                Case "shape"
                    Return New OnnxTensor() {OnnxOperators.Shape(Input(node, 0, env))}

                Case "gather"
                    Return New OnnxTensor() {
                        OnnxOperators.Gather(Input(node, 0, env), Input(node, 1, env), CInt(NodeAttributes.GetInt(node, "axis", 0)))
                    }

                Case "reducesum", "reducemean"
                    Dim axes As Integer() = NodeAttributes.GetInts(node, "axes")
                    Dim axesTensor As OnnxTensor = OptionalInput(node, 1, env)

                    If (axes Is Nothing OrElse axes.Length = 0) AndAlso axesTensor IsNot Nothing Then
                        axes = axesTensor.Data.Select(Function(v) CInt(v)).ToArray()
                    End If

                    Return New OnnxTensor() {
                        OnnxOperators.Reduce(Input(node, 0, env), axes,
                                             NodeAttributes.GetInt(node, "keepdims", 1) <> 0,
                                             op = "reducemean")
                    }

                Case Else
                    Throw New OnnxUnsupportedOperatorException(node)
            End Select
        End Function

        Private Shared Function Constant(node As NodeProto) As OnnxTensor
            Dim tensor As AttributeProto = node.GetAttribute("value")

            If tensor IsNot Nothing AndAlso tensor.T IsNot Nothing Then
                Return OnnxTensor.FromTensorProto(tensor.T)
            End If

            Dim f As AttributeProto = node.GetAttribute("value_float")

            If f IsNot Nothing Then
                Return OnnxTensor.Scalar(f.F)
            End If

            Dim i As AttributeProto = node.GetAttribute("value_int")

            If i IsNot Nothing Then
                Return OnnxTensor.Scalar(i.I)
            End If

            Dim floats As AttributeProto = node.GetAttribute("value_floats")

            If floats IsNot Nothing AndAlso floats.Floats.Count > 0 Then
                Return New OnnxTensor(New Integer() {floats.Floats.Count}, floats.Floats.Select(Function(v) CDbl(v)).ToArray())
            End If

            Dim ints As AttributeProto = node.GetAttribute("value_ints")

            If ints IsNot Nothing AndAlso ints.Ints.Count > 0 Then
                Return New OnnxTensor(New Integer() {ints.Ints.Count}, ints.Ints.Select(Function(v) CDbl(v)).ToArray())
            End If

            Throw New OnnxUnsupportedOperatorException(node, "Constant 节点没有可识别的内容属性（value/value_float/value_int/...）")
        End Function

        ''' <summary>
        ''' 取得第 <paramref name="index"/> 个输入张量。
        ''' </summary>
        ''' <param name="node">当前节点（用于错误信息）。</param>
        ''' <param name="index">输入下标。</param>
        ''' <param name="env">当前环境。</param>
        ''' <returns>输入张量。</returns>
        Private Shared Function Input(node As NodeProto, index As Integer, env As IDictionary(Of String, OnnxTensor)) As OnnxTensor
            If node.Input Is Nothing OrElse index >= node.Input.Count Then
                Throw New OnnxUnsupportedOperatorException(node, $"缺少第 {index} 个输入")
            End If

            Return Resolve(node.Input(index), env, node)
        End Function

        Private Shared Function OptionalInput(node As NodeProto, index As Integer, env As IDictionary(Of String, OnnxTensor)) As OnnxTensor
            If node.Input Is Nothing OrElse index >= node.Input.Count Then
                Return Nothing
            End If

            Dim name As String = node.Input(index)

            If String.IsNullOrEmpty(name) Then
                Return Nothing
            End If

            Dim tensor As OnnxTensor = Nothing

            If env.TryGetValue(name, tensor) Then
                Return tensor
            Else
                Return Nothing
            End If
        End Function

        Private Shared Function Resolve(name As String, env As IDictionary(Of String, OnnxTensor), node As NodeProto) As OnnxTensor
            If String.IsNullOrEmpty(name) Then
                Throw New OnnxUnsupportedOperatorException(node, "输入名称为空")
            End If

            Dim tensor As OnnxTensor = Nothing

            If env.TryGetValue(name, tensor) Then
                Return tensor
            End If

            Throw New OnnxUnsupportedOperatorException(node, $"输入张量 ""{name}"" 尚未被计算（图的拓扑顺序可能不正确）")
        End Function

    End Class

    ''' <summary>
    ''' 读取节点属性的辅助函数（提供默认值并统一转换到 VB 侧的标量类型）。
    ''' </summary>
    Public Module NodeAttributes

        ''' <summary>读取整数属性。</summary>
        ''' <param name="node">节点。</param>
        ''' <param name="name">属性名。</param>
        ''' <param name="fallback">缺省值。</param>
        ''' <returns>属性值。</returns>
        Public Function GetInt(node As NodeProto, name As String, Optional fallback As Long = 0) As Long
            Dim attr As AttributeProto = node.GetAttribute(name)

            If attr Is Nothing Then
                Return fallback
            End If

            Return attr.I
        End Function

        ''' <summary>读取浮点属性。</summary>
        ''' <param name="node">节点。</param>
        ''' <param name="name">属性名。</param>
        ''' <param name="fallback">缺省值。</param>
        ''' <returns>属性值。</returns>
        Public Function GetFloat(node As NodeProto, name As String, Optional fallback As Double = 0) As Double
            Dim attr As AttributeProto = node.GetAttribute(name)

            If attr Is Nothing Then
                Return fallback
            End If

            Return attr.F
        End Function

        ''' <summary>读取整数数组属性。</summary>
        ''' <param name="node">节点。</param>
        ''' <param name="name">属性名。</param>
        ''' <returns>属性值数组；不存在时返回空数组。</returns>
        Public Function GetInts(node As NodeProto, name As String) As Integer()
            Dim attr As AttributeProto = node.GetAttribute(name)

            If attr Is Nothing OrElse attr.Ints Is Nothing Then
                Return New Integer() {}
            End If

            Return attr.Ints.Select(Function(v) CInt(v)).ToArray()
        End Function

    End Module

End Namespace
