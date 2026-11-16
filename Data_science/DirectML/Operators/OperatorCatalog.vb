' ------------------------------------------------------------------------
' 算子能力表
'
' 这里只声明"本框架能把哪些 ONNX 算子映射成 DirectML 算子"，
' 以及该映射依赖的 DirectML 特性级别（用于诊断报告）。
' 真正的可用性判定交给运行时：DirectML 版本不支持时会返回明确的 HRESULT，
' 桥接层会把算子名与节点名带进异常，绝不静默降级。
' ------------------------------------------------------------------------

Imports System.Linq
Imports System.Text
Imports Microsoft.VisualBasic.Computing.DirectML.Interop
Imports Microsoft.VisualBasic.Computing.DirectML.Runtime

Namespace Operators

    ''' <summary>一个 ONNX 算子到 DML 的映射说明。</summary>
    Public NotInheritable Class DmlOperatorSupport

        Friend Sub New(opType As String, minimumFeatureLevel As DML_FEATURE_LEVEL, note As String)
            Me.OpType = opType
            Me.MinimumFeatureLevel = minimumFeatureLevel
            Me.Note = note
        End Sub

        ''' <summary>ONNX 算子名。</summary>
        Public ReadOnly Property OpType As String

        ''' <summary>依赖的最低 DirectML 特性级别（报告用）。</summary>
        Public ReadOnly Property MinimumFeatureLevel As DML_FEATURE_LEVEL

        ''' <summary>映射说明。</summary>
        Public ReadOnly Property Note As String

        Public Overrides Function ToString() As String
            Return $"{OpType} (level >= {DmlDevice.GetFeatureLevelText(MinimumFeatureLevel)})"
        End Function

    End Class

    ''' <summary>ONNX 算子到 DML 的映射目录。</summary>
    Public Module OperatorCatalog

        Private ReadOnly _table As Dictionary(Of String, DmlOperatorSupport) = BuildTable()

        Private Function BuildTable() As Dictionary(Of String, DmlOperatorSupport)
            Dim table As New Dictionary(Of String, DmlOperatorSupport)(StringComparer.OrdinalIgnoreCase)

            Const L1 As DML_FEATURE_LEVEL = DML_FEATURE_LEVEL.LEVEL_1_0
            Const L2 As DML_FEATURE_LEVEL = DML_FEATURE_LEVEL.LEVEL_2_0
            Const L4 As DML_FEATURE_LEVEL = DML_FEATURE_LEVEL.LEVEL_4_0
            Const L5 As DML_FEATURE_LEVEL = DML_FEATURE_LEVEL.LEVEL_5_0
            Const L51 As DML_FEATURE_LEVEL = DML_FEATURE_LEVEL.LEVEL_5_1
            Const L61 As DML_FEATURE_LEVEL = DML_FEATURE_LEVEL.LEVEL_6_1

            ' ---- 图结构 ----
            Add(table, "Constant", L1, "编译期常量（初始化器）")
            Add(table, "Identity", L1, "逐元素恒等复制")
            Add(table, "Dropout", L1, "推理阶段等价于 Identity")
            Add(table, "Cast", L1, "DML_OPERATOR_CAST")

            ' ---- 逐元素 ----
            For Each name As String In {"Add", "Sub", "Mul", "Div", "Pow", "Min", "Max", "Sum", "Mean"}
                Add(table, name, L1, "逐元素二元算子（支持 DML 广播）")
            Next

            For Each name As String In {"Relu", "LeakyRelu", "Elu", "Selu", "Sigmoid", "HardSigmoid", "Tanh",
                                        "Softplus", "Softsign", "Exp", "Log", "Sqrt", "Abs", "Floor", "Ceil",
                                        "Sin", "Cos", "Tan", "Reciprocal", "Sign", "HardSwish", "Shrink",
                                        "ThresholdedRelu", "Celu", "PReLU", "Softmax", "LogSoftmax", "Hardmax"}
                Add(table, name, L1, "逐元素一元 / 激活算子")
            Next

            Add(table, "Neg", L5, "DML_OPERATOR_ELEMENT_WISE_NEGATE")
            Add(table, "Round", L2, "DML_ELEMENT_WISE_ROUND（半数取偶）")
            Add(table, "Erf", L2, "DML_ELEMENT_WISE_ERF")
            Add(table, "Clip", L1, "DML_ELEMENT_WISE_CLIP")
            Add(table, "Where", L1, "DML_ELEMENT_WISE_IF")
            Add(table, "Equal", L1, "输出 UINT8")
            Add(table, "Greater", L1, "输出 UINT8")
            Add(table, "GreaterOrEqual", L1, "输出 UINT8")
            Add(table, "Less", L1, "输出 UINT8")
            Add(table, "LessOrEqual", L1, "输出 UINT8")
            Add(table, "And", L1, "逻辑与（UINT8）")
            Add(table, "Or", L1, "逻辑或（UINT8）")
            Add(table, "Xor", L1, "逻辑异或（UINT8）")
            Add(table, "Not", L1, "逻辑非（UINT8）")

            ' ---- 矩阵乘 ----
            Add(table, "Gemm", L1, "DML_OPERATOR_GEMM")
            Add(table, "MatMul", L1, "通过 GEMM + Reshape 实现")
            Add(table, "MatMulInteger", L2, "整数量化矩阵乘")

            ' ---- 卷积与池化 ----
            Add(table, "Conv", L1, "DML_OPERATOR_CONVOLUTION（交叉相关）")
            Add(table, "ConvTranspose", L1, "DML_OPERATOR_CONVOLUTION（BACKWARD）")
            Add(table, "MaxPool", L1, "DML_OPERATOR_MAX_POOLING / MAX_POOLING2")
            Add(table, "AveragePool", L1, "DML_OPERATOR_AVERAGE_POOLING")
            Add(table, "GlobalAveragePool", L1, "窗口 = 空间尺寸的平均池化")
            Add(table, "GlobalMaxPool", L1, "窗口 = 空间尺寸的最大池化")
            Add(table, "LpPool", L1, "DML_OPERATOR_LP_POOLING")
            Add(table, "MaxUnpool", L1, "需要 indices 输入")

            ' ---- 归一化 ----
            Add(table, "BatchNormalization", L1, "推理：均值/方差为常量")
            Add(table, "InstanceNormalization", L1, "DML_MEAN_VARIANCE_NORMALIZATION")
            Add(table, "LayerNormalization", L2, "DML_MEAN_VARIANCE_NORMALIZATION1")
            Add(table, "LpNormalization", L1, "DML_LP_NORMALIZATION")
            Add(table, "LRN", L1, "DML_LOCAL_RESPONSE_NORMALIZATION")

            ' ---- 归约 ----
            For Each name As String In {"ReduceSum", "ReduceMean", "ReduceMax", "ReduceMin", "ReduceProd",
                                        "ReduceL1", "ReduceL2", "ReduceLogSum", "ReduceLogSumExp"}
                Add(table, name, L1, "DML_OPERATOR_REDUCE")
            Next

            Add(table, "ArgMax", L4, "DML_OPERATOR_ARGMAX")
            Add(table, "ArgMin", L4, "DML_OPERATOR_ARGMIN")

            ' ---- 形状与布局 ----
            Add(table, "Reshape", L1, "零拷贝：只改变张量描述")
            Add(table, "Flatten", L1, "零拷贝：只改变张量描述")
            Add(table, "Squeeze", L1, "零拷贝：只改变张量描述")
            Add(table, "Unsqueeze", L1, "零拷贝：只改变张量描述")
            Add(table, "Transpose", L1, "零拷贝：通过 strides 表达")
            Add(table, "Shape", L1, "编译期常量")
            Add(table, "Concat", L1, "DML_OPERATOR_JOIN")
            Add(table, "Split", L1, "DML_OPERATOR_SPLIT 或逐段 SLICE")
            Add(table, "Slice", L1, "DML_OPERATOR_SLICE")
            Add(table, "Pad", L1, "DML_OPERATOR_PADDING")
            Add(table, "Tile", L1, "DML_OPERATOR_TILE")
            Add(table, "Expand", L1, "通过 TILE 实现")
            Add(table, "DepthToSpace", L1, "DML_OPERATOR_DEPTH_TO_SPACE1")
            Add(table, "SpaceToDepth", L1, "DML_OPERATOR_SPACE_TO_DEPTH1")
            Add(table, "Resize", L2, "DML_OPERATOR_RESAMPLE1")
            Add(table, "Upsample", L2, "DML_OPERATOR_RESAMPLE1")

            ' ---- 数据搬运 ----
            Add(table, "Gather", L1, "DML_OPERATOR_GATHER")
            Add(table, "GatherElements", L2, "DML_OPERATOR_GATHER_ELEMENTS")
            Add(table, "GatherND", L2, "DML_OPERATOR_GATHER_ND")
            Add(table, "ScatterND", L2, "DML_OPERATOR_SCATTER_ND")
            Add(table, "TopK", L2, "DML_OPERATOR_TOP_K1")
            Add(table, "OneHot", L1, "DML_OPERATOR_ONE_HOT")
            Add(table, "CumSum", L1, "DML_OPERATOR_CUMULATIVE_SUMMATION")
            Add(table, "ReverseSequence", L1, "DML_OPERATOR_REVERSE_SUBSEQUENCES")

            ' ---- 量化 ----
            Add(table, "QuantizeLinear", L1, "DML_ELEMENT_WISE_QUANTIZE_LINEAR")
            Add(table, "DequantizeLinear", L1, "DML_ELEMENT_WISE_DEQUANTIZE_LINEAR")

            ' ---- 注意力 ----
            Add(table, "MultiHeadAttention", L61, "DML_OPERATOR_MULTIHEAD_ATTENTION")

            Return table
        End Function

        Private Sub Add(table As Dictionary(Of String, DmlOperatorSupport), opType As String,
                        level As DML_FEATURE_LEVEL, note As String)
            table(opType) = New DmlOperatorSupport(opType, level, note)
        End Sub

        ''' <summary>全部已知映射。</summary>
        Public ReadOnly Property All As IReadOnlyDictionary(Of String, DmlOperatorSupport)
            Get
                Return _table
            End Get
        End Property

        ''' <summary>判断某个 ONNX 算子是否有映射。</summary>
        Public Function IsSupported(opType As String) As Boolean
            If String.IsNullOrEmpty(opType) Then
                Return False
            End If

            Return _table.ContainsKey(opType)
        End Function

        ''' <summary>查询映射说明。</summary>
        Public Function Find(opType As String) As DmlOperatorSupport
            Dim support As DmlOperatorSupport

            If _table.TryGetValue(opType, support) Then
                Return support
            End If

            Return Nothing
        End Function

        ''' <summary>
        ''' 生成能力矩阵文本（供测试程序与诊断报告使用）。
        ''' </summary>
        ''' <param name="deviceLevel">设备支持的最高特性级别（Nothing 表示不比较）。</param>
        Public Function Report(Optional deviceLevel As DML_FEATURE_LEVEL? = Nothing) As String
            Dim sb As New StringBuilder()
            Dim supported As Integer = 0
            Dim unavailable As Integer = 0

            Call sb.AppendLine("ONNX operator -> DirectML mapping")

            For Each item As DmlOperatorSupport In _table.Values.OrderBy(Function(x) x.OpType)
                Dim state As String = "OK   "

                If deviceLevel.HasValue AndAlso CInt(item.MinimumFeatureLevel) > CInt(deviceLevel.Value) Then
                    state = "N/A  "
                    unavailable += 1
                Else
                    supported += 1
                End If

                Call sb.AppendLine($"  {state} {item.OpType,-22} level >= {DmlDevice.GetFeatureLevelText(item.MinimumFeatureLevel),-4} {item.Note}")
            Next

            Call sb.AppendLine()
            Call sb.AppendLine($"total {_table.Count}: {supported} available, {unavailable} need a newer DirectML")

            Return sb.ToString()
        End Function

    End Module

End Namespace
