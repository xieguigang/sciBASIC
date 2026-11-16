Imports System.IO
Imports System.Linq
Imports Microsoft.VisualBasic.Computing.DirectML.Interop
Imports Microsoft.VisualBasic.Computing.DirectML.Operators
Imports Microsoft.VisualBasic.Computing.DirectML.Runtime
Imports Microsoft.VisualBasic.MachineLearning.DataStorage.ONNX.Proto

Namespace ONNX.DirectML

    ''' <summary>
    ''' 把 <see cref="OnnxModel"/> 的计算图编译成 DirectML 计算图。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 布局约定：DirectML 的 GEMM / 卷积都以 4 维张量（NCHW）为基本形态，
    ''' 因此这里把任意秩的 ONNX 张量**左侧补 1** 到 4 维（秩 5 的卷积保持 5 维）：
    ''' 例如 ONNX 的 <c>[M, K]</c> 在显存里就是 <c>[1, 1, M, K]</c>。
    ''' 逻辑秩单独记录在 <c>_shapes</c> 里，用于形状推导与轴换算。
    ''' </para>
    ''' <para>
    ''' 不支持的算子一律抛 <see cref="OnnxDmlUnsupportedOperatorException"/>，绝不静默降级。
    ''' </para>
    ''' </remarks>
    Public NotInheritable Class DmlOnnxGraphCompiler

        Private ReadOnly _model As OnnxModel
        Private ReadOnly _graph As DmlGraph
        Private ReadOnly _env As New Dictionary(Of String, DmlGraphTensor)(StringComparer.Ordinal)
        Private ReadOnly _shapes As New Dictionary(Of String, Integer())(StringComparer.Ordinal)
        Private ReadOnly _constants As New Dictionary(Of String, OnnxTensor)(StringComparer.Ordinal)
        Private ReadOnly _inputs As New List(Of DmlGraphTensor)()
        Private ReadOnly _outputs As New List(Of DmlGraphTensor)()
        Private _sequence As Integer

        ''' <summary>创建编译器。</summary>
        Public Sub New(model As OnnxModel, graph As DmlGraph)
            If model Is Nothing Then
                Throw New ArgumentNullException(NameOf(model))
            End If

            If graph Is Nothing Then
                Throw New ArgumentNullException(NameOf(graph))
            End If

            Me._model = model
            Me._graph = graph

            For Each item As KeyValuePair(Of String, OnnxTensor) In model.Initializers
                _constants(item.Key) = item.Value
            Next
        End Sub

        ''' <summary>图的输入张量。</summary>
        Public ReadOnly Property Inputs As IReadOnlyList(Of DmlGraphTensor)
            Get
                Return _inputs
            End Get
        End Property

        ''' <summary>图的输出张量。</summary>
        Public ReadOnly Property Outputs As IReadOnlyList(Of DmlGraphTensor)
            Get
                Return _outputs
            End Get
        End Property

        ''' <summary>取得某个张量的逻辑（ONNX）形状。</summary>
        Public Function GetLogicalShape(name As String) As Integer()
            Dim dims As Integer()

            If _shapes.TryGetValue(name, dims) Then
                Return dims
            End If

            Return Nothing
        End Function

        ''' <summary>把整个 ONNX 图翻译成 DML 图（不负责编译）。</summary>
        Public Sub Build()
            If _model.Graph Is Nothing Then
                Throw New InvalidDataException("ONNX 模型中没有计算图")
            End If

            For Each value As ValueInfoProto In _model.Inputs
                Dim dims As Integer() = StaticDims(value)

                If dims.Any(Function(d) d <= 0) Then
                    Throw New NotSupportedException(
                        $"模型输入 ""{value.Name}"" 带有动态维度 {DmlShapeInference.ShapeText(DmlShapeInference.ToUnsigned(dims))}；" &
                        "DirectML 后端要求静态形状（可以先固定 batch 再导出模型）")
                End If

                Dim tensor As DmlGraphTensor = Register(value.Name, dims, DmlTensorKind.Input, DML_TENSOR_DATA_TYPE.FLOAT32)

                Call _inputs.Add(tensor)
            Next

            For Each node As NodeProto In _model.Graph.Node
                EmitNode(node)
            Next

            For Each value As ValueInfoProto In _model.Outputs
                Dim tensor As DmlGraphTensor = Nothing

                If Not _env.TryGetValue(value.Name, tensor) Then
                    Throw New InvalidOperationException($"图输出 ""{value.Name}"" 没有被任何节点计算出来")
                End If

                tensor.Kind = DmlTensorKind.Output
                tensor.ReadbackOffset = 0UL

                Call _outputs.Add(tensor)
            Next
        End Sub

#Region "HELPERS"

        Private Shared Function StaticDims(value As ValueInfoProto) As Integer()
            Dim raw As Long() = value.GetDims()
            Dim result As Integer() = New Integer(raw.Length - 1) {}

            For i As Integer = 0 To raw.Length - 1
                result(i) = CInt(raw(i))
            Next

            Return result
        End Function

        Private Function NextName(name As String, suffix As String) As String
            _sequence += 1

            Return $"{name}::{suffix}{_sequence}"
        End Function

        ''' <summary>把逻辑形状补到 4 维（秩 >= 4 时保持不变）。</summary>
        Private Shared Function Physical(dims As Integer()) As UInteger()
            If dims Is Nothing OrElse dims.Length = 0 Then
                Return New UInteger() {1UI, 1UI, 1UI, 1UI}
            End If

            If dims.Length >= 4 Then
                Return DmlShapeInference.ToUnsigned(dims)
            End If

            Dim result As UInteger() = New UInteger(3) {}
            Dim offset As Integer = 4 - dims.Length

            For i As Integer = 0 To offset - 1
                result(i) = 1UI
            Next

            For i As Integer = 0 To dims.Length - 1
                result(offset + i) = CUInt(System.Math.Max(1, dims(i)))
            Next

            Return result
        End Function

        ''' <summary>逻辑轴换算成物理轴。</summary>
        Private Function PhysicalAxis(name As String, axis As Integer) As Integer
            Dim logicalRank As Integer = _shapes(name).Length
            Dim physRank As Integer = Physical(_shapes(name)).Length

            If axis < 0 Then
                axis += logicalRank
            End If

            Return axis + (physRank - logicalRank)
        End Function

        Private Function Register(name As String, dims As Integer(), kind As DmlTensorKind,
                                  dataType As DML_TENSOR_DATA_TYPE) As DmlGraphTensor
            Dim tensor As DmlGraphTensor = _graph.DefineTensor(name, Physical(dims), kind, dataType)

            _env(name) = tensor
            _shapes(name) = If(dims, New Integer() {})

            Return tensor
        End Function

        Private Function Create(name As String, dims As Integer(),
                                Optional dataType As DML_TENSOR_DATA_TYPE = DML_TENSOR_DATA_TYPE.FLOAT32) As DmlGraphTensor
            Return Register(name, dims, DmlTensorKind.Intermediate, dataType)
        End Function

        Private Function Create(name As String, dims As UInteger(),
                                Optional dataType As DML_TENSOR_DATA_TYPE = DML_TENSOR_DATA_TYPE.FLOAT32) As DmlGraphTensor
            Return Register(name, dims.Select(Function(v) CInt(v)).ToArray(), DmlTensorKind.Intermediate, dataType)
        End Function

        ''' <summary>按名称取得张量（常量按需上传到显存）。</summary>
        Private Function GetTensor(name As String) As DmlGraphTensor
            Dim tensor As DmlGraphTensor

            If _env.TryGetValue(name, tensor) Then
                Return tensor
            End If

            Dim constant As OnnxTensor

            If _constants.TryGetValue(name, constant) Then
                Return MaterializeConstant(name, constant)
            End If

            Throw New InvalidOperationException($"ONNX 图引用了未知张量 ""{name}""")
        End Function

        Private Function MaterializeConstant(name As String, value As OnnxTensor) As DmlGraphTensor
            Dim dims As Integer() = CType(value.Dims, Integer())

            If dims Is Nothing OrElse dims.Length = 0 Then
                dims = New Integer() {1}
            End If

            For Each d As Integer In dims
                If d <= 0 Then
                    Throw New InvalidOperationException(
                        $"常量 ""{name}"" 的形状非法：[{String.Join(", ", dims)}]（数据长度 {value.Data.Length}）")
                End If
            Next

            Dim tensor As DmlGraphTensor = Register(name, dims, DmlTensorKind.Constant, DML_TENSOR_DATA_TYPE.FLOAT32)

            tensor.SetData(value.Data.Select(Function(v) CSng(v)).ToArray())

            Return tensor
        End Function

        Private Function RequireTensor(node As NodeProto, index As Integer) As DmlGraphTensor
            If index >= node.Input.Count Then
                Return Nothing
            End If

            Dim name As String = node.Input(index)

            If String.IsNullOrEmpty(name) Then
                Return Nothing
            End If

            Return GetTensor(name)
        End Function

        Private Function ShapeOf(name As String) As Integer()
            Dim dims As Integer()

            If _shapes.TryGetValue(name, dims) Then
                Return dims
            End If

            Throw New InvalidOperationException($"缺少张量 ""{name}"" 的形状信息")
        End Function

        Private Function ShapeOf(node As NodeProto, index As Integer) As Integer()
            Return ShapeOf(node.Input(index))
        End Function

        Private Function TryConstant(node As NodeProto, index As Integer) As OnnxTensor
            If node Is Nothing OrElse index >= node.Input.Count Then
                Return Nothing
            End If

            Dim name As String = node.Input(index)

            If String.IsNullOrEmpty(name) Then
                Return Nothing
            End If

            Dim constant As OnnxTensor

            If _constants.TryGetValue(name, constant) Then
                Return constant
            End If

            Return Nothing
        End Function

        Private Function ConstantInts(node As NodeProto, index As Integer) As Integer()
            Dim constant As OnnxTensor = TryConstant(node, index)

            If constant Is Nothing Then
                Return Nothing
            End If

            Return constant.Data.Select(Function(v) CInt(v)).ToArray()
        End Function

        Private Function ConstantSingles(node As NodeProto, index As Integer) As Single()
            Dim constant As OnnxTensor = TryConstant(node, index)

            If constant Is Nothing Then
                Return Nothing
            End If

            Return constant.Data.Select(Function(v) CSng(v)).ToArray()
        End Function

        Private Shared Function BroadcastShape(a As Integer(), b As Integer()) As Integer()
            Dim rank As Integer = System.Math.Max(a.Length, b.Length)
            Dim result As Integer() = New Integer(rank - 1) {}

            For i As Integer = 0 To rank - 1
                Dim ai As Integer = If(i >= rank - a.Length, a(i - (rank - a.Length)), 1)
                Dim bi As Integer = If(i >= rank - b.Length, b(i - (rank - b.Length)), 1)

                If ai <> bi AndAlso ai <> 1 AndAlso bi <> 1 Then
                    Throw New InvalidOperationException($"无法广播形状 {DmlShapeInference.ShapeText(DmlShapeInference.ToUnsigned(a))} 与 {DmlShapeInference.ShapeText(DmlShapeInference.ToUnsigned(b))}")
                End If

                result(i) = System.Math.Max(ai, bi)
            Next

            Return result
        End Function

        ''' <summary>把张量重新描述为目标物理形状（数据不变，元素数必须一致）。</summary>
        Private Function ReshapeTensor(tensor As DmlGraphTensor, targetDims As UInteger(),
                                       Optional name As String = Nothing) As DmlGraphTensor
            Dim elements As ULong = 1UL

            For Each d As UInteger In targetDims
                elements *= d
            Next

            If elements <> tensor.Tensor.ElementCount Then
                Throw New InvalidOperationException(
                    $"无法把 {tensor.Tensor} 重排为 [{String.Join(", ", targetDims)}]：元素个数不一致")
            End If

            Dim view As DmlGraphTensor = _graph.DefineTensorView(If(name, NextName(tensor.Name, "view")),
                                                                 New DmlTensor(targetDims, tensor.Tensor.DataType),
                                                                 tensor)

            _shapes(view.Name) = targetDims.Select(Function(d) CInt(d)).ToArray()

            Return view
        End Function

        ''' <summary>保证张量是 FP32（必要时插入 Cast）。</summary>
        Private Function AsFloat(tensor As DmlGraphTensor) As DmlGraphTensor
            If tensor.Tensor.DataType = DML_TENSOR_DATA_TYPE.FLOAT32 Then
                Return tensor
            End If

            Dim converted As DmlGraphTensor = Create(NextName(tensor.Name, "cast"),
                                                     _shapes(tensor.Name),
                                                     DML_TENSOR_DATA_TYPE.FLOAT32)

            _graph.AddOperator(ShapeOperators.Cast(tensor, converted))

            Return converted
        End Function

        ''' <summary>保证两个张量的物理维度数一致（必要时插入一次拷贝）。</summary>
        Private Function AlignRank(a As DmlGraphTensor, b As DmlGraphTensor) As Tuple(Of DmlGraphTensor, DmlGraphTensor)
            If a.Tensor.Rank = b.Tensor.Rank Then
                Return New Tuple(Of DmlGraphTensor, DmlGraphTensor)(a, b)
            End If

            If a.Tensor.Rank > b.Tensor.Rank Then
                Return New Tuple(Of DmlGraphTensor, DmlGraphTensor)(a, AlignRank(b, a).Item2)
            End If

            Dim target As Integer = b.Tensor.Rank
            Dim dims As UInteger() = New UInteger(target - 1) {}
            Dim offset As Integer = target - a.Tensor.Rank

            For i As Integer = 0 To target - 1
                If i < offset Then
                    dims(i) = 1UI
                Else
                    dims(i) = a.Tensor.Dims(i - offset)
                End If
            Next

            Return New Tuple(Of DmlGraphTensor, DmlGraphTensor)(ReshapeTensor(a, dims), b)
        End Function

        Private Function NormalizeAxes(name As String, axes As Integer()) As UInteger()
            If axes Is Nothing OrElse axes.Length = 0 Then
                Return New UInteger() {}
            End If

            Dim result As UInteger() = New UInteger(axes.Length - 1) {}

            For i As Integer = 0 To axes.Length - 1
                result(i) = CUInt(PhysicalAxis(name, axes(i)))
            Next

            Return result
        End Function

#End Region

#Region "DISPATCH"

        Private Sub EmitNode(node As NodeProto)
            Dim op As String = If(node.OpType, "").ToLowerInvariant()

            If node.Output Is Nothing OrElse node.Output.Count = 0 Then
                Throw Fail(node, "节点没有任何输出")
            End If

            Select Case op
                Case "identity", "dropout"
                    EmitIdentity(node)

                Case "constant"
                    EmitConstant(node)

                Case "add", "sub", "mul", "div", "pow", "min", "max", "sum", "mean",
                     "equal", "greater", "greaterorequal", "less", "lessorequal",
                     "and", "or", "xor"
                    EmitBinary(node, op)

                Case "not"
                    EmitUnaryOp(node, DML_OPERATOR_TYPE.ELEMENT_WISE_LOGICAL_NOT)

                Case "relu", "leakyrelu", "elu", "selu", "sigmoid", "hardsigmoid", "tanh",
                     "softplus", "softsign", "exp", "log", "sqrt", "abs", "floor", "ceil",
                     "sin", "cos", "tan", "reciprocal", "sign", "neg", "erf", "round",
                     "hardswish", "shrink", "thresholdedrelu", "celu", "gelu",
                     "softmax", "logsoftmax", "hardmax", "prelu", "hardswish1"
                    EmitActivation(node, op)

                Case "clip"
                    EmitClip(node)

                Case "cast"
                    EmitCast(node)

                Case "gemm"
                    EmitGemm(node)

                Case "matmul"
                    EmitMatMul(node)

                Case "conv"
                    EmitConv(node)

                Case "convtranspose"
                    EmitConvTranspose(node)

                Case "maxpool", "averagepool", "globalaveragepool", "globalmaxpool", "lppool"
                    EmitPool(node, op)

                Case "batchnormalization"
                    EmitBatchNormalization(node)

                Case "instancenormalization"
                    EmitInstanceNormalization(node)

                Case "layernormalization"
                    EmitLayerNormalization(node)

                Case "lrn"
                    EmitLrn(node)

                Case "reducesum", "reducemean", "reducemax", "reducemin", "reduceprod",
                     "reducel1", "reducel2", "reducelogsum", "reducelogsumexp"
                    EmitReduce(node, op)

                Case "argmax", "argmin"
                    EmitArgMinMax(node, op = "argmax")

                Case "reshape", "flatten", "squeeze", "unsqueeze", "transpose"
                    EmitShape(node, op)

                Case "concat"
                    EmitConcat(node)

                Case "split"
                    EmitSplit(node)

                Case "slice"
                    EmitSlice(node)

                Case "pad"
                    EmitPad(node)

                Case "tile"
                    EmitTile(node)

                Case "expand"
                    EmitExpand(node)

                Case "gather"
                    EmitGather(node)

                Case "gatherelements"
                    EmitGatherElements(node)

                Case "gathernd"
                    EmitGatherNd(node)

                Case "scatternd"
                    EmitScatterNd(node)

                Case "topk"
                    EmitTopK(node)

                Case "where"
                    EmitWhere(node)

                Case "quantizelinear"
                    EmitQuantizeLinear(node, True)

                Case "dequantizelinear"
                    EmitQuantizeLinear(node, False)

                Case "depthtospace"
                    EmitDepthToSpace(node)

                Case "spacetodepth"
                    EmitSpaceToDepth(node)

                Case "resize", "upsample"
                    EmitResize(node)

                Case "shape"
                    EmitShapeOf(node)

                Case Else
                    Throw Fail(node, $"DirectML 后端尚未映射该算子")
            End Select
        End Sub

        Private Function Fail(node As NodeProto, reason As String) As Exception
            Dim level As String = "<unknown>"

            Try
                level = _graph.Engine.Device.FeatureLevelText
            Catch
            End Try

            Return New OnnxDmlUnsupportedOperatorException(node, level, reason)
        End Function

#End Region

#Region "EMIT"

        Private Sub EmitIdentity(node As NodeProto)
            Dim x As DmlGraphTensor = RequireTensor(node, 0)
            Dim dims As Integer() = ShapeOf(node, 0)
            Dim y As DmlGraphTensor = Create(node.Output(0), dims)

            _graph.AddOperator(ShapeOperators.Identity(AsFloat(x), y, node.Name))
        End Sub

        Private Sub EmitConstant(node As NodeProto)
            Dim attr As AttributeProto = node.GetAttribute("value")
            Dim value As OnnxTensor = Nothing

            If attr IsNot Nothing AndAlso attr.T IsNot Nothing Then
                value = OnnxTensor.FromTensorProto(attr.T)
            ElseIf node.GetAttribute("value_float") IsNot Nothing Then
                value = OnnxTensor.Scalar(node.GetAttribute("value_float").F)
            ElseIf node.GetAttribute("value_int") IsNot Nothing Then
                value = OnnxTensor.Scalar(node.GetAttribute("value_int").I)
            ElseIf node.GetAttribute("value_floats") IsNot Nothing Then
                Dim floats As AttributeProto = node.GetAttribute("value_floats")
                value = New OnnxTensor(New Integer() {floats.Floats.Count}, floats.Floats.Select(Function(v) CDbl(v)).ToArray())
            ElseIf node.GetAttribute("value_ints") IsNot Nothing Then
                Dim ints As AttributeProto = node.GetAttribute("value_ints")
                value = New OnnxTensor(New Integer() {ints.Ints.Count}, ints.Ints.Select(Function(v) CDbl(v)).ToArray())
            End If

            If value Is Nothing Then
                Throw Fail(node, "Constant 节点没有可识别的内容")
            End If

            _constants(node.Output(0)) = value
        End Sub

        Private Shared Function BinaryKind(op As String) As DML_OPERATOR_TYPE
            Select Case op
                Case "add", "sum"
                    Return DML_OPERATOR_TYPE.ELEMENT_WISE_ADD
                Case "sub"
                    Return DML_OPERATOR_TYPE.ELEMENT_WISE_SUBTRACT
                Case "mul"
                    Return DML_OPERATOR_TYPE.ELEMENT_WISE_MULTIPLY
                Case "div"
                    Return DML_OPERATOR_TYPE.ELEMENT_WISE_DIVIDE
                Case "pow"
                    Return DML_OPERATOR_TYPE.ELEMENT_WISE_POW
                Case "min"
                    Return DML_OPERATOR_TYPE.ELEMENT_WISE_MIN
                Case "max"
                    Return DML_OPERATOR_TYPE.ELEMENT_WISE_MAX
                Case "mean"
                    Return DML_OPERATOR_TYPE.ELEMENT_WISE_MEAN
                Case "equal"
                    Return DML_OPERATOR_TYPE.ELEMENT_WISE_LOGICAL_EQUALS
                Case "greater"
                    Return DML_OPERATOR_TYPE.ELEMENT_WISE_LOGICAL_GREATER_THAN
                Case "greaterorequal"
                    Return DML_OPERATOR_TYPE.ELEMENT_WISE_LOGICAL_GREATER_THAN_OR_EQUAL
                Case "less"
                    Return DML_OPERATOR_TYPE.ELEMENT_WISE_LOGICAL_LESS_THAN
                Case "lessorequal"
                    Return DML_OPERATOR_TYPE.ELEMENT_WISE_LOGICAL_LESS_THAN_OR_EQUAL
                Case "and"
                    Return DML_OPERATOR_TYPE.ELEMENT_WISE_LOGICAL_AND
                Case "or"
                    Return DML_OPERATOR_TYPE.ELEMENT_WISE_LOGICAL_OR
                Case "xor"
                    Return DML_OPERATOR_TYPE.ELEMENT_WISE_LOGICAL_XOR
                Case Else
                    Throw New NotSupportedException($"未知二元算子 {op}")
            End Select
        End Function

        Private Shared Function IsLogical(op As String) As Boolean
            Select Case op
                Case "equal", "greater", "greaterorequal", "less", "lessorequal", "and", "or", "xor"
                    Return True
                Case Else
                    Return False
            End Select
        End Function

        Private Sub EmitBinary(node As NodeProto, op As String)
            Dim a As DmlGraphTensor = RequireTensor(node, 0)
            Dim b As DmlGraphTensor = RequireTensor(node, 1)

            If a Is Nothing OrElse b Is Nothing Then
                Throw Fail(node, "缺少输入")
            End If

            If op = "sum" AndAlso node.Input.Count > 2 Then
                ' 多输入求和：两两相加
                Dim acc As DmlGraphTensor = a

                For i As Integer = 1 To node.Input.Count - 1
                    Dim nextTensor As DmlGraphTensor = RequireTensor(node, i)
                    Dim isLast As Boolean = (i = node.Input.Count - 1)
                    Dim outName As String = If(isLast, node.Output(0), NextName(node.Output(0), "sum"))
                    Dim dims As Integer() = BroadcastShape(_shapes(acc.Name), _shapes(nextTensor.Name))
                    Dim target As DmlGraphTensor = Create(outName, dims)

                    acc = EmitBinaryCore(node, DML_OPERATOR_TYPE.ELEMENT_WISE_ADD, acc, nextTensor, target)
                Next

                Return
            End If

            Dim outDims As Integer() = BroadcastShape(_shapes(a.Name), _shapes(b.Name))
            Dim output As DmlGraphTensor = Create(node.Output(0), outDims,
                                                  If(IsLogical(op), DML_TENSOR_DATA_TYPE.UINT8, DML_TENSOR_DATA_TYPE.FLOAT32))

            Dim kind As DML_OPERATOR_TYPE = BinaryKind(op)

            If op = "pow" Then
                Dim left As DmlGraphTensor = AsFloat(a)
                Dim right As DmlGraphTensor = AsFloat(b)
                Dim aligned As Tuple(Of DmlGraphTensor, DmlGraphTensor) = AlignRank(left, right)

                _graph.AddOperator(ElementWiseOperators.Binary(kind, aligned.Item1, aligned.Item2, output, node.Name))

                Return
            End If

            Call EmitBinaryCore(node, kind, a, b, output)
        End Sub

        Private Function EmitBinaryCore(node As NodeProto, kind As DML_OPERATOR_TYPE,
                                        a As DmlGraphTensor, b As DmlGraphTensor,
                                        output As DmlGraphTensor) As DmlGraphTensor
            Dim aligned As Tuple(Of DmlGraphTensor, DmlGraphTensor) = AlignRank(a, b)

            _graph.AddOperator(ElementWiseOperators.Binary(kind, aligned.Item1, aligned.Item2, output, node.Name))

            Return output
        End Function

        Private Sub EmitUnaryOp(node As NodeProto, kind As DML_OPERATOR_TYPE)
            Dim x As DmlGraphTensor = RequireTensor(node, 0)
            Dim dims As Integer() = ShapeOf(node, 0)
            Dim y As DmlGraphTensor = Create(node.Output(0), dims, DML_TENSOR_DATA_TYPE.UINT8)

            _graph.AddOperator(ElementWiseOperators.Unary(kind, x, y, Nothing, node.Name))
        End Sub

        Private Sub EmitActivation(node As NodeProto, op As String)
            Dim x As DmlGraphTensor = RequireTensor(node, 0)

            If x Is Nothing Then
                Throw Fail(node, "缺少输入")
            End If

            Dim dims As Integer() = ShapeOf(node, 0)
            Dim y As DmlGraphTensor = Create(node.Output(0), dims)
            Dim source As DmlGraphTensor = AsFloat(x)

            Select Case op
                Case "relu"
                    _graph.AddOperator(ActivationOperators.Relu(source, y, node.Name))
                Case "sigmoid"
                    _graph.AddOperator(ActivationOperators.Sigmoid(source, y, node.Name))
                Case "tanh"
                    _graph.AddOperator(ActivationOperators.Tanh(source, y, node.Name))
                Case "leakyrelu"
                    _graph.AddOperator(ActivationOperators.LeakyRelu(source, y, NodeFloat(node, "alpha", 0.01F), node.Name))
                Case "elu"
                    _graph.AddOperator(ActivationOperators.Activation(DML_OPERATOR_TYPE.ACTIVATION_ELU, source, y,
                                                                      alpha:=NodeFloat(node, "alpha", 1.0F), name:=node.Name))
                Case "selu"
                    _graph.AddOperator(ActivationOperators.Activation(DML_OPERATOR_TYPE.ACTIVATION_SCALED_ELU, source, y,
                                                                      alpha:=NodeFloat(node, "alpha", 1.6732632F),
                                                                      gamma:=NodeFloat(node, "gamma", 1.050701F),
                                                                      name:=node.Name))
                Case "hardsigmoid"
                    _graph.AddOperator(ActivationOperators.Activation(DML_OPERATOR_TYPE.ACTIVATION_HARD_SIGMOID, source, y,
                                                                      alpha:=NodeFloat(node, "alpha", 0.2F),
                                                                      beta:=NodeFloat(node, "beta", 0.5F),
                                                                      name:=node.Name))
                Case "hardswish", "hardswish1"
                    _graph.AddOperator(ActivationOperators.Activation(DML_OPERATOR_TYPE.ACTIVATION_HARD_SWISH, source, y,
                                                                      alpha:=1.0F / 6.0F, beta:=0.5F, name:=node.Name))
                Case "softplus"
                    _graph.AddOperator(ActivationOperators.Activation(DML_OPERATOR_TYPE.ACTIVATION_SOFTPLUS, source, y,
                                                                      alpha:=1.0F, name:=node.Name))
                Case "softsign"
                    _graph.AddOperator(ActivationOperators.Activation(DML_OPERATOR_TYPE.ACTIVATION_SOFTSIGN, source, y, name:=node.Name))
                Case "gelu"
                    _graph.AddOperator(ActivationOperators.Activation(DML_OPERATOR_TYPE.ACTIVATION_GELU, source, y, name:=node.Name))
                Case "celu"
                    _graph.AddOperator(ActivationOperators.Activation(DML_OPERATOR_TYPE.ACTIVATION_CELU, source, y,
                                                                      alpha:=NodeFloat(node, "alpha", 1.0F), name:=node.Name))
                Case "shrink"
                    _graph.AddOperator(ActivationOperators.Activation(DML_OPERATOR_TYPE.ACTIVATION_SHRINK, source, y,
                                                                      alpha:=NodeFloat(node, "bias", 0.0F),
                                                                      beta:=NodeFloat(node, "lambd", 0.5F), name:=node.Name))
                Case "thresholdedrelu"
                    _graph.AddOperator(ActivationOperators.Activation(DML_OPERATOR_TYPE.ACTIVATION_THRESHOLDED_RELU, source, y,
                                                                      alpha:=NodeFloat(node, "alpha", 1.0F), name:=node.Name))
                Case "prelu"
                    Dim slope As DmlGraphTensor = RequireTensor(node, 1)

                    _graph.AddOperator(ActivationOperators.Activation(DML_OPERATOR_TYPE.ACTIVATION_PARAMETERIZED_RELU, source, y,
                                                                      slope:=AsFloat(slope), name:=node.Name))
                Case "softmax", "logsoftmax", "hardmax"
                    Dim kind As DML_OPERATOR_TYPE = DML_OPERATOR_TYPE.ACTIVATION_SOFTMAX

                    If op = "logsoftmax" Then
                        kind = DML_OPERATOR_TYPE.ACTIVATION_LOG_SOFTMAX
                    ElseIf op = "hardmax" Then
                        kind = DML_OPERATOR_TYPE.ACTIVATION_HARDMAX
                    End If

                    Dim axis As Integer = NodeInt(node, "axis", -1)

                    _graph.AddOperator(ActivationOperators.Activation(kind, source, y, axis:=PhysicalAxis(x.Name, axis), name:=node.Name))
                Case Else
                    Dim kind As DML_OPERATOR_TYPE

                    Select Case op
                        Case "exp"
                            kind = DML_OPERATOR_TYPE.ELEMENT_WISE_EXP
                        Case "log"
                            kind = DML_OPERATOR_TYPE.ELEMENT_WISE_LOG
                        Case "sqrt"
                            kind = DML_OPERATOR_TYPE.ELEMENT_WISE_SQRT
                        Case "reciprocal"
                            kind = DML_OPERATOR_TYPE.ELEMENT_WISE_RECIP
                        Case "abs"
                            kind = DML_OPERATOR_TYPE.ELEMENT_WISE_ABS
                        Case "floor"
                            kind = DML_OPERATOR_TYPE.ELEMENT_WISE_FLOOR
                        Case "ceil"
                            kind = DML_OPERATOR_TYPE.ELEMENT_WISE_CEIL
                        Case "sin"
                            kind = DML_OPERATOR_TYPE.ELEMENT_WISE_SIN
                        Case "cos"
                            kind = DML_OPERATOR_TYPE.ELEMENT_WISE_COS
                        Case "tan"
                            kind = DML_OPERATOR_TYPE.ELEMENT_WISE_TAN
                        Case "sign"
                            kind = DML_OPERATOR_TYPE.ELEMENT_WISE_SIGN
                        Case "neg"
                            kind = DML_OPERATOR_TYPE.ELEMENT_WISE_NEGATE
                        Case "erf"
                            kind = DML_OPERATOR_TYPE.ELEMENT_WISE_ERF
                        Case "round"
                            kind = DML_OPERATOR_TYPE.ELEMENT_WISE_ROUND
                        Case Else
                            Throw Fail(node, "未映射的逐元素算子")
                    End Select

                    _graph.AddOperator(ElementWiseOperators.Unary(kind, source, y, Nothing, node.Name))
            End Select
        End Sub

        Private Sub EmitClip(node As NodeProto)
            Dim x As DmlGraphTensor = AsFloat(RequireTensor(node, 0))
            Dim dims As Integer() = ShapeOf(node, 0)
            Dim y As DmlGraphTensor = Create(node.Output(0), dims)
            Dim min As Single = Single.MinValue
            Dim max As Single = Single.MaxValue

            If node.Input.Count > 1 Then
                Dim values As Single() = ConstantSingles(node, 1)

                If values IsNot Nothing AndAlso values.Length > 0 Then
                    min = values(0)
                End If
            Else
                min = NodeFloat(node, "min", Single.MinValue)
            End If

            If node.Input.Count > 2 Then
                Dim values As Single() = ConstantSingles(node, 2)

                If values IsNot Nothing AndAlso values.Length > 0 Then
                    max = values(0)
                End If
            Else
                max = NodeFloat(node, "max", Single.MaxValue)
            End If

            _graph.AddOperator(ElementWiseOperators.Clip(x, y, min, max, node.Name))
        End Sub

        Private Shared Function DmlTypeOf(elementType As TensorDataType) As DML_TENSOR_DATA_TYPE
            Select Case elementType
                Case TensorDataType.Float
                    Return DML_TENSOR_DATA_TYPE.FLOAT32
                Case TensorDataType.Float16
                    Return DML_TENSOR_DATA_TYPE.FLOAT16
                Case TensorDataType.Float64
                    Return DML_TENSOR_DATA_TYPE.FLOAT32
                Case TensorDataType.UInt8, TensorDataType.Bool
                    Return DML_TENSOR_DATA_TYPE.UINT8
                Case TensorDataType.Int8
                    Return DML_TENSOR_DATA_TYPE.INT8
                Case TensorDataType.Int16
                    Return DML_TENSOR_DATA_TYPE.INT16
                Case TensorDataType.UInt16
                    Return DML_TENSOR_DATA_TYPE.UINT16
                Case TensorDataType.Int32
                    Return DML_TENSOR_DATA_TYPE.INT32
                Case TensorDataType.UInt32
                    Return DML_TENSOR_DATA_TYPE.UINT32
                Case Else
                    Return DML_TENSOR_DATA_TYPE.FLOAT32
            End Select
        End Function

        Private Sub EmitCast(node As NodeProto)
            Dim x As DmlGraphTensor = RequireTensor(node, 0)
            Dim dims As Integer() = ShapeOf(node, 0)
            Dim target As DML_TENSOR_DATA_TYPE = DML_TENSOR_DATA_TYPE.FLOAT32
            Dim toAttr As AttributeProto = node.GetAttribute("to")

            If toAttr IsNot Nothing Then
                target = DmlTypeOf(CType(CInt(toAttr.I), TensorDataType))
            End If

            ' DirectML 的 CAST 只支持数值类型之间的转换；内部统一用 FP32 表示数值
            If target <> DML_TENSOR_DATA_TYPE.FLOAT32 AndAlso target <> DML_TENSOR_DATA_TYPE.UINT8 AndAlso
               target <> DML_TENSOR_DATA_TYPE.INT32 Then
                target = DML_TENSOR_DATA_TYPE.FLOAT32
            End If

            Dim y As DmlGraphTensor = Create(node.Output(0), dims, target)

            _graph.AddOperator(ShapeOperators.Cast(x, y, node.Name))
        End Sub

        Private Shared Function NodeInt(node As NodeProto, name As String, [default] As Integer) As Integer
            Dim attr As AttributeProto = node.GetAttribute(name)

            If attr Is Nothing Then
                Return [default]
            End If

            Return CInt(attr.I)
        End Function

        Private Shared Function NodeFloat(node As NodeProto, name As String, [default] As Single) As Single
            Dim attr As AttributeProto = node.GetAttribute(name)

            If attr Is Nothing Then
                Return [default]
            End If

            Return attr.F
        End Function

        Private Shared Function NodeInts(node As NodeProto, name As String) As Integer()
            Dim attr As AttributeProto = node.GetAttribute(name)

            If attr Is Nothing OrElse attr.Ints Is Nothing OrElse attr.Ints.Count = 0 Then
                Return Nothing
            End If

            Return attr.Ints.Select(Function(v) CInt(v)).ToArray()
        End Function

#End Region

#Region "MATH"

        Private Sub EmitGemm(node As NodeProto)
            Dim a As DmlGraphTensor = AsFloat(RequireTensor(node, 0))
            Dim b As DmlGraphTensor = AsFloat(RequireTensor(node, 1))
            Dim c As DmlGraphTensor = RequireTensor(node, 2)

            If c IsNot Nothing Then
                c = AsFloat(c)
            End If

            Dim transA As Boolean = NodeInt(node, "transA", 0) <> 0
            Dim transB As Boolean = NodeInt(node, "transB", 0) <> 0
            Dim alpha As Single = NodeFloat(node, "alpha", 1.0F)
            Dim beta As Single = NodeFloat(node, "beta", 1.0F)

            Dim aDims As Integer() = _shapes(a.Name)
            Dim bDims As Integer() = _shapes(b.Name)

            Dim m As Integer = If(transA, aDims(1), aDims(0))
            Dim n As Integer = If(transB, bDims(0), bDims(1))
            Dim outDims As Integer() = {m, n}

            Dim y As DmlGraphTensor = Create(node.Output(0), outDims)

            _graph.AddOperator(MatMulOperators.Gemm(a, b, c, y, transA, transB, alpha, beta, node.Name))
        End Sub

        Private Sub EmitMatMul(node As NodeProto)
            Dim a As DmlGraphTensor = AsFloat(RequireTensor(node, 0))
            Dim b As DmlGraphTensor = AsFloat(RequireTensor(node, 1))

            Dim shapes As Tuple(Of UInteger(), UInteger(), UInteger()) =
                MatMulOperators.ResolveMatMulShapes(_shapes(a.Name), _shapes(b.Name))

            ' ONNX 输出形状（可能带批量维）
            Dim outDims As Integer() = MatMulOutputShape(_shapes(a.Name), _shapes(b.Name))

            Dim left As DmlGraphTensor = ReshapeTensor(a, shapes.Item1)
            Dim right As DmlGraphTensor = ReshapeTensor(b, shapes.Item2)
            Dim y As DmlGraphTensor = Create(node.Output(0), outDims)

            _graph.AddOperator(MatMulOperators.Gemm(left, right, Nothing, y, False, False, 1.0F, 0.0F, node.Name))
        End Sub

        Private Shared Function MatMulOutputShape(a As Integer(), b As Integer()) As Integer()
            Dim aa As Integer() = If(a.Length = 1, New Integer() {1, a(0)}, a)
            Dim bb As Integer() = If(b.Length = 1, New Integer() {b(0), 1}, b)

            Dim aBatch As Integer() = If(aa.Length > 2, aa.Take(aa.Length - 2).ToArray(), New Integer() {})
            Dim bBatch As Integer() = If(bb.Length > 2, bb.Take(bb.Length - 2).ToArray(), New Integer() {})

            Dim batch As Integer() = BroadcastShape(aBatch, bBatch)
            Dim result As New List(Of Integer)

            Call result.AddRange(batch)
            Call result.Add(aa(aa.Length - 2))
            Call result.Add(bb(bb.Length - 1))

            If a.Length = 1 Then
                Call result.RemoveAt(result.Count - 2)
            End If

            If b.Length = 1 Then
                Call result.RemoveAt(result.Count - 1)
            End If

            If result.Count = 0 Then
                Return New Integer() {1}
            End If

            Return result.ToArray()
        End Function

        Private Sub EmitConv(node As NodeProto)
            Dim x As DmlGraphTensor = AsFloat(RequireTensor(node, 0))
            Dim w As DmlGraphTensor = AsFloat(RequireTensor(node, 1))
            Dim bias As DmlGraphTensor = RequireTensor(node, 2)

            If bias IsNot Nothing Then
                bias = AsFloat(bias)
            End If

            Dim xDims As Integer() = _shapes(x.Name)
            Dim wDims As Integer() = _shapes(w.Name)
            Dim spatial As Integer = xDims.Length - 2

            Dim strides As Integer() = NodeInts(node, "strides")
            Dim dilations As Integer() = NodeInts(node, "dilations")
            Dim pads As Integer() = NodeInts(node, "pads")
            Dim autoPad As String = NodeString(node, "auto_pad")
            Dim group As Integer = NodeInt(node, "group", 1)

            If strides Is Nothing Then
                strides = Enumerable.Repeat(1, spatial).ToArray()
            End If

            If dilations Is Nothing Then
                dilations = Enumerable.Repeat(1, spatial).ToArray()
            End If

            Dim startPadding As Integer() = Nothing
            Dim endPadding As Integer() = Nothing

            If pads IsNot Nothing AndAlso pads.Length = 2 * spatial Then
                startPadding = New Integer(spatial - 1) {}
                endPadding = New Integer(spatial - 1) {}

                For i As Integer = 0 To spatial - 1
                    startPadding(i) = pads(i)
                    endPadding(i) = pads(i + spatial)
                Next
            Else
                DmlShapeInference.ResolveAutoPadding(autoPad,
                                                     xDims.Skip(2).ToArray(),
                                                     wDims.Skip(2).ToArray(),
                                                     strides, dilations,
                                                     startPadding, endPadding)
            End If

            Dim outDims As Integer() = New Integer(xDims.Length - 1) {}
            outDims(0) = xDims(0)
            outDims(1) = wDims(0)

            For i As Integer = 0 To spatial - 1
                outDims(i + 2) = DmlShapeInference.OutputSize(xDims(i + 2), wDims(i + 2), strides(i), dilations(i),
                                                              startPadding(i), endPadding(i))
            Next

            Dim y As DmlGraphTensor = Create(node.Output(0), outDims)

            _graph.AddOperator(ConvolutionOperators.Convolution(
                x, w, bias, y,
                DmlShapeInference.ToUnsigned(strides),
                DmlShapeInference.ToUnsigned(dilations),
                DmlShapeInference.ToUnsigned(startPadding),
                DmlShapeInference.ToUnsigned(endPadding),
                Nothing, CUInt(group), DML_CONVOLUTION_DIRECTION.FORWARD, node.Name))
        End Sub

        Private Shared Function NodeString(node As NodeProto, name As String) As String
            Dim attr As AttributeProto = node.GetAttribute(name)

            If attr Is Nothing Then
                Return Nothing
            End If

            Return attr.GetString()
        End Function

        Private Sub EmitConvTranspose(node As NodeProto)
            Dim x As DmlGraphTensor = AsFloat(RequireTensor(node, 0))
            Dim w As DmlGraphTensor = AsFloat(RequireTensor(node, 1))
            Dim bias As DmlGraphTensor = RequireTensor(node, 2)

            If bias IsNot Nothing Then
                bias = AsFloat(bias)
            End If

            Dim xDims As Integer() = _shapes(x.Name)
            Dim wDims As Integer() = _shapes(w.Name)
            Dim spatial As Integer = xDims.Length - 2

            Dim strides As Integer() = NodeInts(node, "strides")
            Dim dilations As Integer() = NodeInts(node, "dilations")
            Dim pads As Integer() = NodeInts(node, "pads")
            Dim outputPadding As Integer() = NodeInts(node, "output_padding")
            Dim autoPad As String = NodeString(node, "auto_pad")
            Dim group As Integer = NodeInt(node, "group", 1)

            If strides Is Nothing Then
                strides = Enumerable.Repeat(1, spatial).ToArray()
            End If

            If dilations Is Nothing Then
                dilations = Enumerable.Repeat(1, spatial).ToArray()
            End If

            Dim startPadding As Integer() = Nothing
            Dim endPadding As Integer() = Nothing

            If pads IsNot Nothing AndAlso pads.Length = 2 * spatial Then
                startPadding = New Integer(spatial - 1) {}
                endPadding = New Integer(spatial - 1) {}

                For i As Integer = 0 To spatial - 1
                    startPadding(i) = pads(i)
                    endPadding(i) = pads(i + spatial)
                Next
            Else
                DmlShapeInference.ResolveAutoPadding(autoPad, xDims.Skip(2).ToArray(), wDims.Skip(2).ToArray(),
                                                     strides, dilations, startPadding, endPadding)
            End If

            Dim outDims As Integer() = New Integer(xDims.Length - 1) {}
            outDims(0) = xDims(0)
            outDims(1) = wDims(1) * group

            For i As Integer = 0 To spatial - 1
                Dim extra As Integer = If(outputPadding Is Nothing OrElse i >= outputPadding.Length, 0, outputPadding(i))

                outDims(i + 2) = strides(i) * (xDims(i + 2) - 1) + dilations(i) * (wDims(i + 2) - 1) + 1 +
                                 extra - startPadding(i) - endPadding(i)
            Next

            Dim y As DmlGraphTensor = Create(node.Output(0), outDims)

            _graph.AddOperator(ConvolutionOperators.Convolution(
                x, w, bias, y,
                DmlShapeInference.ToUnsigned(strides),
                DmlShapeInference.ToUnsigned(dilations),
                DmlShapeInference.ToUnsigned(startPadding),
                DmlShapeInference.ToUnsigned(endPadding),
                DmlShapeInference.ToUnsigned(outputPadding),
                CUInt(group), DML_CONVOLUTION_DIRECTION.BACKWARD, node.Name))
        End Sub

        Private Sub EmitPool(node As NodeProto, op As String)
            Dim x As DmlGraphTensor = AsFloat(RequireTensor(node, 0))
            Dim xDims As Integer() = _shapes(x.Name)
            Dim spatial As Integer = xDims.Length - 2

            Dim globalPool As Boolean = op.StartsWith("global", StringComparison.Ordinal)
            Dim kernel As Integer()

            If globalPool Then
                kernel = xDims.Skip(2).ToArray()
            Else
                kernel = NodeInts(node, "kernel_shape")

                If kernel Is Nothing Then
                    Throw Fail(node, "池化缺少 kernel_shape")
                End If
            End If

            Dim strides As Integer() = NodeInts(node, "strides")

            If strides Is Nothing Then
                strides = Enumerable.Repeat(1, spatial).ToArray()
            End If

            Dim dilations As Integer() = NodeInts(node, "dilations")
            Dim pads As Integer() = NodeInts(node, "pads")
            Dim autoPad As String = NodeString(node, "auto_pad")
            Dim startPadding As Integer() = Nothing
            Dim endPadding As Integer() = Nothing

            If pads IsNot Nothing AndAlso pads.Length = 2 * spatial Then
                startPadding = New Integer(spatial - 1) {}
                endPadding = New Integer(spatial - 1) {}

                For i As Integer = 0 To spatial - 1
                    startPadding(i) = pads(i)
                    endPadding(i) = pads(i + spatial)
                Next
            Else
                DmlShapeInference.ResolveAutoPadding(autoPad, xDims.Skip(2).ToArray(), kernel, strides,
                                                     If(dilations, Enumerable.Repeat(1, spatial).ToArray()),
                                                     startPadding, endPadding)
            End If

            Dim outDims As Integer() = New Integer(xDims.Length - 1) {}
            outDims(0) = xDims(0)
            outDims(1) = xDims(1)

            For i As Integer = 0 To spatial - 1
                Dim dilation As Integer = If(dilations Is Nothing OrElse i >= dilations.Length, 1, dilations(i))

                outDims(i + 2) = DmlShapeInference.OutputSize(xDims(i + 2), kernel(i), strides(i), dilation,
                                                              startPadding(i), endPadding(i))
            Next

            Dim y As DmlGraphTensor = Create(node.Output(0), outDims)

            Select Case op
                Case "maxpool", "globalmaxpool"
                    _graph.AddOperator(ConvolutionOperators.MaxPool(
                        x, y,
                        DmlShapeInference.ToUnsigned(kernel),
                        DmlShapeInference.ToUnsigned(strides),
                        DmlShapeInference.ToUnsigned(startPadding),
                        DmlShapeInference.ToUnsigned(endPadding),
                        If(dilations Is Nothing, Nothing, DmlShapeInference.ToUnsigned(dilations)),
                        node.Name))

                Case "averagepool", "globalaveragepool"
                    Dim includePad As Boolean = NodeInt(node, "count_include_pad", 0) <> 0

                    _graph.AddOperator(ConvolutionOperators.AveragePool(
                        x, y,
                        DmlShapeInference.ToUnsigned(kernel),
                        DmlShapeInference.ToUnsigned(strides),
                        DmlShapeInference.ToUnsigned(startPadding),
                        DmlShapeInference.ToUnsigned(endPadding),
                        includePad, node.Name))

                Case Else
                    Dim p As Integer = NodeInt(node, "p", 2)

                    _graph.AddOperator(ConvolutionOperators.LpPool(
                        x, y,
                        DmlShapeInference.ToUnsigned(kernel),
                        DmlShapeInference.ToUnsigned(strides),
                        DmlShapeInference.ToUnsigned(startPadding),
                        DmlShapeInference.ToUnsigned(endPadding),
                        CUInt(p), node.Name))
            End Select
        End Sub

        Private Sub EmitBatchNormalization(node As NodeProto)
            Dim x As DmlGraphTensor = AsFloat(RequireTensor(node, 0))
            Dim scale As DmlGraphTensor = AsFloat(RequireTensor(node, 1))
            Dim bias As DmlGraphTensor = AsFloat(RequireTensor(node, 2))
            Dim mean As DmlGraphTensor = AsFloat(RequireTensor(node, 3))
            Dim variance As DmlGraphTensor = AsFloat(RequireTensor(node, 4))
            Dim dims As Integer() = ShapeOf(node, 0)
            Dim y As DmlGraphTensor = Create(node.Output(0), dims)

            _graph.AddOperator(ReduceOperators.BatchNormalization(x, mean, variance, scale, bias, y,
                                                                  NodeFloat(node, "epsilon", 0.00001F), True, node.Name))
        End Sub

        Private Sub EmitInstanceNormalization(node As NodeProto)
            Dim x As DmlGraphTensor = AsFloat(RequireTensor(node, 0))
            Dim scale As DmlGraphTensor = AsFloat(RequireTensor(node, 1))
            Dim bias As DmlGraphTensor = AsFloat(RequireTensor(node, 2))
            Dim dims As Integer() = ShapeOf(node, 0)
            Dim y As DmlGraphTensor = Create(node.Output(0), dims)

            _graph.AddOperator(ReduceOperators.InstanceNormalization(x, scale, bias, y,
                                                                     NodeFloat(node, "epsilon", 0.00001F), node.Name))
        End Sub

        Private Sub EmitLayerNormalization(node As NodeProto)
            Dim x As DmlGraphTensor = AsFloat(RequireTensor(node, 0))
            Dim scale As DmlGraphTensor = RequireTensor(node, 1)
            Dim bias As DmlGraphTensor = RequireTensor(node, 2)
            Dim dims As Integer() = ShapeOf(node, 0)
            Dim y As DmlGraphTensor = Create(node.Output(0), dims)
            Dim axis As Integer = NodeInt(node, "axis", -1)
            Dim logical As Integer() = _shapes(x.Name)

            If axis < 0 Then
                axis += logical.Length
            End If

            Dim axes As New List(Of UInteger)

            For i As Integer = axis To logical.Length - 1
                Call axes.Add(CUInt(PhysicalAxis(x.Name, i)))
            Next

            Dim scaleFloat As DmlGraphTensor = Nothing

            If scale IsNot Nothing Then
                scaleFloat = AsFloat(scale)
            End If

            Dim biasFloat As DmlGraphTensor = Nothing

            If bias IsNot Nothing Then
                biasFloat = AsFloat(bias)
            End If

            _graph.AddOperator(ReduceOperators.LayerNormalization(x, scaleFloat, biasFloat, y, axes.ToArray(),
                                                                  NodeFloat(node, "epsilon", 0.00001F), node.Name))
        End Sub

        Private Sub EmitLrn(node As NodeProto)
            Dim x As DmlGraphTensor = AsFloat(RequireTensor(node, 0))
            Dim dims As Integer() = ShapeOf(node, 0)
            Dim y As DmlGraphTensor = Create(node.Output(0), dims)

            _graph.AddOperator(ReduceOperators.LocalResponseNormalization(
                x, y,
                CUInt(NodeInt(node, "size", 5)),
                NodeFloat(node, "alpha", 0.0001F),
                NodeFloat(node, "beta", 0.75F),
                NodeFloat(node, "bias", 1.0F),
                True, node.Name))
        End Sub

        Private Shared Function ReduceFunctionOf(op As String) As DML_REDUCE_FUNCTION
            Select Case op
                Case "reducesum"
                    Return DML_REDUCE_FUNCTION.SUM
                Case "reducemean"
                    Return DML_REDUCE_FUNCTION.AVERAGE
                Case "reducemax"
                    Return DML_REDUCE_FUNCTION.MAX
                Case "reducemin"
                    Return DML_REDUCE_FUNCTION.MIN
                Case "reduceprod"
                    Return DML_REDUCE_FUNCTION.MULTIPLY
                Case "reducel1"
                    Return DML_REDUCE_FUNCTION.L1
                Case "reducel2"
                    Return DML_REDUCE_FUNCTION.L2
                Case "reducelogsum"
                    Return DML_REDUCE_FUNCTION.LOG_SUM
                Case "reducelogsumexp"
                    Return DML_REDUCE_FUNCTION.LOG_SUM_EXP
                Case Else
                    Throw New NotSupportedException($"未知归约算子 {op}")
            End Select
        End Function

        Private Sub EmitReduce(node As NodeProto, op As String)
            Dim x As DmlGraphTensor = AsFloat(RequireTensor(node, 0))
            Dim logical As Integer() = _shapes(x.Name)
            Dim axes As Integer() = NodeInts(node, "axes")

            If axes Is Nothing Then
                Dim shapeTensor As Integer() = ConstantInts(node, 1)

                If shapeTensor IsNot Nothing Then
                    axes = shapeTensor
                End If
            End If

            If axes Is Nothing Then
                axes = Enumerable.Range(0, logical.Length).ToArray()
            End If

            Dim keepDims As Boolean = NodeInt(node, "keepdims", 1) <> 0
            Dim physicalAxes As UInteger() = NormalizeAxes(x.Name, axes)
            Dim outDims As Integer() = ReduceOperators.ReduceShape(DmlShapeInference.ToUnsigned(logical),
                                                                   DmlShapeInference.ToUnsigned(axes), keepDims) _
                .Select(Function(v) CInt(v)).ToArray()
            Dim y As DmlGraphTensor = Create(node.Output(0), outDims)

            _graph.AddOperator(ReduceOperators.Reduce(ReduceFunctionOf(op), x, y, physicalAxes, node.Name))
        End Sub

        Private Sub EmitArgMinMax(node As NodeProto, isArgMax As Boolean)
            Dim x As DmlGraphTensor = AsFloat(RequireTensor(node, 0))
            Dim logical As Integer() = _shapes(x.Name)
            Dim axis As Integer = NodeInt(node, "axis", 0)
            Dim keepDims As Boolean = NodeInt(node, "keepdims", 1) <> 0
            Dim axes As Integer() = {axis}
            Dim physicalAxes As UInteger() = NormalizeAxes(x.Name, axes)
            Dim outDims As Integer() = ReduceOperators.ReduceShape(DmlShapeInference.ToUnsigned(logical),
                                                                   DmlShapeInference.ToUnsigned(axes), keepDims) _
                .Select(Function(v) CInt(v)).ToArray()
            Dim y As DmlGraphTensor = Create(node.Output(0), outDims, DML_TENSOR_DATA_TYPE.INT32)

            _graph.AddOperator(ReduceOperators.ArgMinMax(isArgMax, x, y, physicalAxes, keepDims, node.Name))
        End Sub

#End Region

#Region "SHAPE"

        Private Sub EmitShape(node As NodeProto, op As String)
            Dim x As DmlGraphTensor = RequireTensor(node, 0)
            Dim logical As Integer() = _shapes(x.Name)

            Select Case op
                Case "reshape"
                    Dim target As Integer() = ConstantInts(node, 1)

                    If target Is Nothing Then
                        Throw Fail(node, "Reshape 缺少目标形状常量")
                    End If

                    Dim resolved As Integer() = OnnxTensor.ResolveDims(logical, CInt(x.Tensor.ElementCount), target)
                    Dim y As DmlGraphTensor = Create(node.Output(0), resolved)

                    _graph.AddOperator(ShapeOperators.Identity(x, y, node.Name))

                Case "flatten"
                    Dim axis As Integer = NodeInt(node, "axis", 1)

                    If axis < 0 Then
                        axis += logical.Length
                    End If

                    Dim head As Integer = 1
                    Dim tail As Integer = 1

                    For i As Integer = 0 To axis - 1
                        head *= logical(i)
                    Next

                    For i As Integer = axis To logical.Length - 1
                        tail *= logical(i)
                    Next

                    Dim y As DmlGraphTensor = Create(node.Output(0), New Integer() {head, tail})

                    _graph.AddOperator(ShapeOperators.Identity(x, y, node.Name))

                Case "squeeze", "unsqueeze"
                    Dim axes As Integer() = NodeInts(node, "axes")

                    If axes Is Nothing Then
                        axes = ConstantInts(node, 1)
                    End If

                    Dim result As New List(Of Integer)

                    If op = "squeeze" Then
                        For i As Integer = 0 To logical.Length - 1
                            Dim drop As Boolean = False

                            If axes IsNot Nothing Then
                                For Each a As Integer In axes
                                    Dim axisIndex As Integer = If(a < 0, a + logical.Length, a)

                                    If axisIndex = i AndAlso logical(i) = 1 Then
                                        drop = True
                                    End If
                                Next
                            ElseIf logical(i) = 1 Then
                                drop = True
                            End If

                            If Not drop Then
                                Call result.Add(logical(i))
                            End If
                        Next
                    Else
                        If axes Is Nothing Then
                            Throw Fail(node, "Unsqueeze 缺少 axes")
                        End If

                        Dim inserted As New HashSet(Of Integer)

                        For Each a As Integer In axes
                            Call inserted.Add(If(a < 0, a + logical.Length + axes.Length, a))
                        Next

                        Dim source As Integer = 0

                        For i As Integer = 0 To logical.Length + axes.Length - 1
                            If inserted.Contains(i) Then
                                Call result.Add(1)
                            Else
                                Call result.Add(logical(source))
                                source += 1
                            End If
                        Next
                    End If

                    If result.Count = 0 Then
                        Call result.Add(1)
                    End If

                    Dim y As DmlGraphTensor = Create(node.Output(0), result.ToArray())

                    _graph.AddOperator(ShapeOperators.Identity(x, y, node.Name))

                Case "transpose"
                    Dim perm As Integer() = NodeInts(node, "perm")

                    If perm Is Nothing Then
                        perm = Enumerable.Range(0, logical.Length).Reverse().ToArray()
                    End If

                    Dim outLogical As Integer() = perm.Select(Function(i) logical(i)).ToArray()
                    Dim srcPhysical As UInteger() = x.Tensor.Dims
                    Dim srcStrides As UInteger() = DmlTensor.GetDefaultStrides(srcPhysical)
                    Dim pad As Integer = srcPhysical.Length - logical.Length
                    Dim physPerm As Integer() = New Integer(srcPhysical.Length - 1) {}

                    For i As Integer = 0 To pad - 1
                        physPerm(i) = i
                    Next

                    For i As Integer = 0 To perm.Length - 1
                        physPerm(pad + i) = pad + perm(i)
                    Next

                    Dim viewDims As UInteger() = physPerm.Select(Function(i) srcPhysical(i)).ToArray()
                    Dim viewStrides As UInteger() = physPerm.Select(Function(i) srcStrides(i)).ToArray()

                    Dim view As DmlGraphTensor = _graph.DefineTensorView(NextName(x.Name, "transpose"),
                                                                         New DmlTensor(viewDims, x.Tensor.DataType, viewStrides),
                                                                         x)

                    _shapes(view.Name) = outLogical

                    Dim y As DmlGraphTensor = Create(node.Output(0), outLogical)

                    _graph.AddOperator(ShapeOperators.Identity(view, y, node.Name))
            End Select
        End Sub

        Private Sub EmitShapeOf(node As NodeProto)
            Dim logical As Integer() = ShapeOf(node, 0)
            Dim values As Double() = logical.Select(Function(v) CDbl(v)).ToArray()

            _constants(node.Output(0)) = New OnnxTensor(New Integer() {logical.Length}, values)
        End Sub

        Private Sub EmitConcat(node As NodeProto)
            Dim axis As Integer = NodeInt(node, "axis", 0)
            Dim tensors As New List(Of DmlGraphTensor)
            Dim logical As Integer() = Nothing
            Dim physAxis As Integer = 0

            For Each name As String In node.Input
                If String.IsNullOrEmpty(name) Then
                    Continue For
                End If

                Dim tensor As DmlGraphTensor = AsFloat(GetTensor(name))

                If logical Is Nothing Then
                    logical = CType(_shapes(name).Clone(), Integer())
                    physAxis = PhysicalAxis(name, axis)
                End If

                Call tensors.Add(tensor)
            Next

            If tensors.Count = 0 Then
                Throw Fail(node, "Concat 没有输入")
            End If

            Dim outDims As Integer() = CType(logical.Clone(), Integer())
            outDims(If(axis < 0, axis + logical.Length, axis)) = 0

            For Each tensor As DmlGraphTensor In tensors
                outDims(If(axis < 0, axis + logical.Length, axis)) += _shapes(tensor.Name)(If(axis < 0, axis + logical.Length, axis))
            Next

            Dim y As DmlGraphTensor = Create(node.Output(0), outDims)

            Dim joined As DmlTensor() = tensors.Select(Function(t) t.Tensor).ToArray()

            _graph.AddOperator(ShapeOperators.Join(joined, y, CUInt(physAxis), node.Name))
        End Sub

        Private Sub EmitSplit(node As NodeProto)
            Dim x As DmlGraphTensor = AsFloat(RequireTensor(node, 0))
            Dim logical As Integer() = _shapes(x.Name)
            Dim axis As Integer = NodeInt(node, "axis", 0)
            Dim axisIndex As Integer = If(axis < 0, axis + logical.Length, axis)
            Dim sizes As Integer() = NodeInts(node, "split")

            If sizes Is Nothing Then
                Dim shapeTensor As Integer() = ConstantInts(node, 1)

                If shapeTensor IsNot Nothing Then
                    sizes = shapeTensor
                End If
            End If

            Dim count As Integer = node.Output.Count

            If sizes Is Nothing Then
                sizes = New Integer(count - 1) {}

                Dim perOutput As Integer = logical(axisIndex) \ count

                For i As Integer = 0 To count - 1
                    sizes(i) = perOutput
                Next
            End If

            ' 逐段 SLICE 更稳妥（DML 的 SPLIT 对不等长切分要求更严）
            Dim offset As Integer = 0

            For i As Integer = 0 To count - 1
                Dim outDims As Integer() = CType(logical.Clone(), Integer())
                outDims(axisIndex) = sizes(i)

                Dim offsets As Integer() = New Integer(logical.Length - 1) {}
                offsets(axisIndex) = offset

                Dim y As DmlGraphTensor = Create(node.Output(i), outDims)

                _graph.AddOperator(ShapeOperators.Slice(
                    x, y,
                    DmlShapeInference.ToUnsigned(offsets),
                    DmlShapeInference.ToUnsigned(outDims),
                    DmlShapeInference.ToUnsigned(Enumerable.Repeat(1, logical.Length).ToArray()),
                    If(node.Name, "") & "::split" & i))

                offset += sizes(i)
            Next
        End Sub

        Private Sub EmitSlice(node As NodeProto)
            Dim x As DmlGraphTensor = RequireTensor(node, 0)
            Dim logical As Integer() = _shapes(x.Name)
            Dim starts As Integer() = ConstantInts(node, 1)
            Dim ends As Integer() = ConstantInts(node, 2)
            Dim axes As Integer() = ConstantInts(node, 3)
            Dim steps As Integer() = ConstantInts(node, 4)

            If starts Is Nothing Then
                starts = NodeInts(node, "starts")
                ends = NodeInts(node, "ends")
                axes = NodeInts(node, "axes")
            End If

            If starts Is Nothing OrElse ends Is Nothing Then
                Throw Fail(node, "Slice 缺少 starts/ends")
            End If

            If axes Is Nothing Then
                axes = Enumerable.Range(0, starts.Length).ToArray()
            End If

            Dim offsets As Integer() = New Integer(logical.Length - 1) {}
            Dim sizes As UInteger() = DmlShapeInference.ToUnsigned(logical)
            Dim strides As Integer() = Enumerable.Repeat(1, logical.Length).ToArray()

            For i As Integer = 0 To starts.Length - 1
                Dim axis As Integer = If(axes(i) < 0, axes(i) + logical.Length, axes(i))
                Dim stepValue As Integer = If(steps Is Nothing OrElse i >= steps.Length, 1, steps(i))

                If stepValue <= 0 Then
                    Throw Fail(node, "DirectML 的 SLICE 不支持负步长")
                End If

                Dim start As Integer = If(starts(i) < 0, starts(i) + logical(axis), starts(i))
                Dim endValue As Integer = If(ends(i) < 0, ends(i) + logical(axis), ends(i))

                start = System.Math.Max(0, System.Math.Min(start, logical(axis)))
                endValue = System.Math.Max(0, System.Math.Min(endValue, logical(axis)))

                Dim length As Integer = System.Math.Max(0, (endValue - start + stepValue - 1) \ stepValue)

                offsets(axis) = start
                sizes(axis) = CUInt(length)
                strides(axis) = stepValue
            Next

            Dim outDims As Integer() = offsets.Select(Function(v, i) CInt(sizes(i))).ToArray()
            Dim y As DmlGraphTensor = Create(node.Output(0), outDims)

            _graph.AddOperator(ShapeOperators.Slice(x, y,
                                                    DmlShapeInference.ToUnsigned(offsets),
                                                    sizes,
                                                    DmlShapeInference.ToUnsigned(strides), node.Name))
        End Sub

        Private Sub EmitPad(node As NodeProto)
            Dim x As DmlGraphTensor = RequireTensor(node, 0)
            Dim logical As Integer() = _shapes(x.Name)
            Dim pads As Integer() = ConstantInts(node, 1)

            If pads Is Nothing Then
                pads = NodeInts(node, "pads")
            End If

            If pads Is Nothing Then
                Throw Fail(node, "Pad 缺少 pads")
            End If

            Dim value As Single = 0.0F
            Dim valueConstant As OnnxTensor = TryConstant(node, 2)

            If valueConstant IsNot Nothing AndAlso valueConstant.Data.Length > 0 Then
                value = CSng(valueConstant.Data(0))
            ElseIf node.GetAttribute("value") IsNot Nothing Then
                value = node.GetAttribute("value").F
            End If

            Dim mode As String = NodeString(node, "mode")
            Dim paddingMode As DML_PADDING_MODE = DML_PADDING_MODE.CONSTANT

            If String.Equals(mode, "reflect", StringComparison.OrdinalIgnoreCase) Then
                paddingMode = DML_PADDING_MODE.REFLECTION
            ElseIf String.Equals(mode, "edge", StringComparison.OrdinalIgnoreCase) Then
                paddingMode = DML_PADDING_MODE.EDGE
            End If

            Dim startPadding As Integer() = New Integer(logical.Length - 1) {}
            Dim endPadding As Integer() = New Integer(logical.Length - 1) {}
            Dim outDims As Integer() = CType(logical.Clone(), Integer())

            For i As Integer = 0 To logical.Length - 1
                startPadding(i) = pads(i)
                endPadding(i) = pads(i + logical.Length)
                outDims(i) = logical(i) + startPadding(i) + endPadding(i)
            Next

            Dim y As DmlGraphTensor = Create(node.Output(0), outDims)

            _graph.AddOperator(ShapeOperators.Pad(x, y, paddingMode,
                                                  DmlShapeInference.ToUnsigned(startPadding),
                                                  DmlShapeInference.ToUnsigned(endPadding),
                                                  value, node.Name))
        End Sub

        Private Sub EmitTile(node As NodeProto)
            Dim x As DmlGraphTensor = RequireTensor(node, 0)
            Dim logical As Integer() = _shapes(x.Name)
            Dim repeats As Integer() = ConstantInts(node, 1)

            If repeats Is Nothing Then
                Throw Fail(node, "Tile 缺少 repeats 常量")
            End If

            Dim outDims As Integer() = New Integer(logical.Length - 1) {}

            For i As Integer = 0 To logical.Length - 1
                outDims(i) = logical(i) * repeats(i)
            Next

            Dim y As DmlGraphTensor = Create(node.Output(0), outDims)

            _graph.AddOperator(ShapeOperators.Tile(x, y, DmlShapeInference.ToUnsigned(repeats), node.Name))
        End Sub

        Private Sub EmitExpand(node As NodeProto)
            Dim x As DmlGraphTensor = RequireTensor(node, 0)
            Dim logical As Integer() = _shapes(x.Name)
            Dim target As Integer() = ConstantInts(node, 1)

            If target Is Nothing Then
                Throw Fail(node, "Expand 缺少目标形状常量")
            End If

            Dim outDims As Integer() = BroadcastShape(logical, target)
            Dim repeats As Integer() = New Integer(outDims.Length - 1) {}
            Dim padded As Integer() = New Integer(outDims.Length - 1) {}

            For i As Integer = 0 To outDims.Length - 1
                Dim ai As Integer = If(i >= outDims.Length - logical.Length, logical(i - (outDims.Length - logical.Length)), 1)

                padded(i) = ai
                repeats(i) = outDims(i) \ ai
            Next

            Dim reshaped As DmlGraphTensor = ReshapeTensor(x, DmlShapeInference.ToUnsigned(padded))
            Dim y As DmlGraphTensor = Create(node.Output(0), outDims)

            _graph.AddOperator(ShapeOperators.Tile(reshaped, y, DmlShapeInference.ToUnsigned(repeats), node.Name))
        End Sub

        Private Sub EmitDepthToSpace(node As NodeProto)
            Dim x As DmlGraphTensor = RequireTensor(node, 0)
            Dim logical As Integer() = _shapes(x.Name)
            Dim block As Integer = NodeInt(node, "blocksize", 1)
            Dim mode As String = NodeString(node, "mode")
            Dim outDims As Integer() = {logical(0), logical(1) \ (block * block), logical(2) * block, logical(3) * block}
            Dim y As DmlGraphTensor = Create(node.Output(0), outDims)
            Dim order As DML_DEPTH_SPACE_ORDER = DML_DEPTH_SPACE_ORDER.DEPTH_COLUMN_ROW

            If String.Equals(mode, "CRD", StringComparison.OrdinalIgnoreCase) Then
                order = DML_DEPTH_SPACE_ORDER.COLUMN_ROW_DEPTH
            End If

            _graph.AddOperator(ShapeOperators.DepthToSpace(x, y, CUInt(block), order, node.Name))
        End Sub

        Private Sub EmitSpaceToDepth(node As NodeProto)
            Dim x As DmlGraphTensor = RequireTensor(node, 0)
            Dim logical As Integer() = _shapes(x.Name)
            Dim block As Integer = NodeInt(node, "blocksize", 1)
            Dim outDims As Integer() = {logical(0), logical(1) * block * block, logical(2) \ block, logical(3) \ block}
            Dim y As DmlGraphTensor = Create(node.Output(0), outDims)

            _graph.AddOperator(ShapeOperators.SpaceToDepth(x, y, CUInt(block), DML_DEPTH_SPACE_ORDER.DEPTH_COLUMN_ROW, node.Name))
        End Sub

        Private Sub EmitResize(node As NodeProto)
            Dim x As DmlGraphTensor = RequireTensor(node, 0)
            Dim logical As Integer() = _shapes(x.Name)
            Dim scales As Single() = ConstantSingles(node, 2)

            If node.OpType.Equals("Upsample", StringComparison.OrdinalIgnoreCase) Then
                scales = ConstantSingles(node, 1)
            End If

            Dim sizes As Integer() = ConstantInts(node, 3)

            If sizes Is Nothing Then
                sizes = NodeInts(node, "sizes")
            End If

            If scales Is Nothing Then
                If sizes Is Nothing Then
                    If node.GetAttribute("scales") IsNot Nothing Then
                        scales = node.GetAttribute("scales").Floats.ToArray()
                    Else
                        Throw Fail(node, "Resize 缺少 scales 或 sizes")
                    End If
                Else
                    scales = New Single(sizes.Length - 1) {}

                    For i As Integer = 0 To sizes.Length - 1
                        scales(i) = sizes(i) / CSng(logical(i))
                    Next
                End If
            End If

            Dim outDims As Integer() = New Integer(logical.Length - 1) {}

            For i As Integer = 0 To logical.Length - 1
                outDims(i) = CInt(System.Math.Floor(logical(i) * scales(i)))
            Next

            Dim mode As String = NodeString(node, "mode")
            Dim interpolation As DML_INTERPOLATION_MODE = DML_INTERPOLATION_MODE.NEAREST_NEIGHBOR

            If String.Equals(mode, "linear", StringComparison.OrdinalIgnoreCase) OrElse
               String.Equals(mode, "bilinear", StringComparison.OrdinalIgnoreCase) OrElse
               String.Equals(mode, "trilinear", StringComparison.OrdinalIgnoreCase) Then
                interpolation = DML_INTERPOLATION_MODE.LINEAR
            End If

            Dim y As DmlGraphTensor = Create(node.Output(0), outDims)
            Dim zeros As Single() = New Single(logical.Length - 1) {}

            _graph.AddOperator(ShapeOperators.Resample(x, y, interpolation, scales, zeros, zeros, node.Name))
        End Sub

#End Region

#Region "DATA"

        Private Sub EmitGather(node As NodeProto)
            Dim x As DmlGraphTensor = RequireTensor(node, 0)
            Dim indices As DmlGraphTensor = GetTensor(node.Input(1))
            Dim logical As Integer() = _shapes(x.Name)
            Dim indexShape As Integer() = _shapes(indices.Name)
            Dim axis As Integer = NodeInt(node, "axis", 0)
            Dim axisIndex As Integer = If(axis < 0, axis + logical.Length, axis)
            Dim outDims As New List(Of Integer)

            For i As Integer = 0 To axisIndex - 1
                Call outDims.Add(logical(i))
            Next

            Call outDims.AddRange(indexShape)

            For i As Integer = axisIndex + 1 To logical.Length - 1
                Call outDims.Add(logical(i))
            Next

            Dim y As DmlGraphTensor = Create(node.Output(0), outDims.ToArray())

            _graph.AddOperator(DataMovementOperators.Gather(x, indices, y,
                                                            CUInt(PhysicalAxis(x.Name, axisIndex)),
                                                            CUInt(System.Math.Max(1, indexShape.Length)),
                                                            node.Name))
        End Sub

        Private Sub EmitGatherElements(node As NodeProto)
            Dim x As DmlGraphTensor = RequireTensor(node, 0)
            Dim indices As DmlGraphTensor = GetTensor(node.Input(1))
            Dim axis As Integer = NodeInt(node, "axis", 0)
            Dim dims As Integer() = _shapes(indices.Name)
            Dim y As DmlGraphTensor = Create(node.Output(0), dims)

            _graph.AddOperator(DataMovementOperators.GatherElements(x, indices, y,
                                                                    CUInt(PhysicalAxis(x.Name, axis)), node.Name))
        End Sub

        Private Sub EmitGatherNd(node As NodeProto)
            Dim x As DmlGraphTensor = RequireTensor(node, 0)
            Dim indices As DmlGraphTensor = GetTensor(node.Input(1))
            Dim indexShape As Integer() = _shapes(indices.Name)
            Dim logical As Integer() = _shapes(x.Name)
            Dim k As Integer = indexShape(indexShape.Length - 1)
            Dim outDims As New List(Of Integer)

            For i As Integer = 0 To indexShape.Length - 2
                Call outDims.Add(indexShape(i))
            Next

            For i As Integer = k To logical.Length - 1
                Call outDims.Add(logical(i))
            Next

            If outDims.Count = 0 Then
                Call outDims.Add(1)
            End If

            Dim y As DmlGraphTensor = Create(node.Output(0), outDims.ToArray())

            _graph.AddOperator(DataMovementOperators.GatherNd(x, indices, y, 1UI, 1UI, node.Name))
        End Sub

        Private Sub EmitScatterNd(node As NodeProto)
            Dim x As DmlGraphTensor = RequireTensor(node, 0)
            Dim indices As DmlGraphTensor = GetTensor(node.Input(1))
            Dim updates As DmlGraphTensor = RequireTensor(node, 2)
            Dim dims As Integer() = _shapes(x.Name)
            Dim y As DmlGraphTensor = Create(node.Output(0), dims)

            _graph.AddOperator(DataMovementOperators.ScatterNd(x, indices, updates, y, 1UI, 1UI, node.Name))
        End Sub

        Private Sub EmitTopK(node As NodeProto)
            Dim x As DmlGraphTensor = RequireTensor(node, 0)
            Dim logical As Integer() = _shapes(x.Name)
            Dim axis As Integer = NodeInt(node, "axis", -1)
            Dim axisIndex As Integer = If(axis < 0, axis + logical.Length, axis)
            Dim kValues As Integer() = ConstantInts(node, 1)
            Dim k As Integer = If(kValues IsNot Nothing AndAlso kValues.Length > 0, kValues(0), 1)
            Dim outDims As Integer() = CType(logical.Clone(), Integer())

            outDims(axisIndex) = k

            Dim values As DmlGraphTensor = Create(node.Output(0), outDims)

            If node.Output.Count < 2 Then
                Throw Fail(node, "TopK 需要两个输出")
            End If

            Dim indices As DmlGraphTensor = Create(node.Output(1), outDims, DML_TENSOR_DATA_TYPE.INT32)
            Dim largest As Boolean = NodeInt(node, "largest", 1) <> 0

            _graph.AddOperator(DataMovementOperators.TopK(x, values, indices, CUInt(k),
                                                          CUInt(PhysicalAxis(x.Name, axisIndex)),
                                                          largest, node.Name))
        End Sub

        Private Sub EmitWhere(node As NodeProto)
            Dim condition As DmlGraphTensor = RequireTensor(node, 0)
            Dim a As DmlGraphTensor = AsFloat(RequireTensor(node, 1))
            Dim b As DmlGraphTensor = AsFloat(RequireTensor(node, 2))
            Dim aligned As Tuple(Of DmlGraphTensor, DmlGraphTensor) = AlignRank(a, b)
            Dim outDims As Integer() = BroadcastShape(_shapes(condition.Name), _shapes(aligned.Item1.Name))
            Dim y As DmlGraphTensor = Create(node.Output(0), outDims)

            _graph.AddOperator(ElementWiseOperators.[If](condition, aligned.Item1, aligned.Item2, y, node.Name))
        End Sub

        Private Sub EmitQuantizeLinear(node As NodeProto, quantize As Boolean)
            Dim x As DmlGraphTensor = RequireTensor(node, 0)
            Dim scale As DmlGraphTensor = GetTensor(node.Input(1))
            Dim zeroPoint As DmlGraphTensor = Nothing

            If node.Input.Count > 2 AndAlso Not String.IsNullOrEmpty(node.Input(2)) Then
                zeroPoint = GetTensor(node.Input(2))
            End If

            Dim dims As Integer() = _shapes(x.Name)

            If quantize Then
                Dim y As DmlGraphTensor = Create(node.Output(0), dims, DML_TENSOR_DATA_TYPE.UINT8)

                _graph.AddOperator(ElementWiseOperators.QuantizeLinear(x, scale, zeroPoint, y, node.Name))
            Else
                Dim y As DmlGraphTensor = Create(node.Output(0), dims)

                _graph.AddOperator(ElementWiseOperators.DequantizeLinear(x, scale, zeroPoint, y, node.Name))
            End If
        End Sub

#End Region

    End Class

End Namespace
