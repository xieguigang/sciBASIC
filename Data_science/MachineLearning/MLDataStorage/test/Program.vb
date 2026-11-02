Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports Microsoft.VisualBasic.ApplicationServices.Debugging
Imports Microsoft.VisualBasic.MachineLearning.CNN
Imports Microsoft.VisualBasic.MachineLearning.CNN.data
Imports Microsoft.VisualBasic.MachineLearning.DataStorage.ONNX
Imports Microsoft.VisualBasic.MachineLearning.DataStorage.ONNX.Proto

''' <summary>
''' MLDataStorage 的存储格式支持测试：
''' 1) ONNX：解析并推理 microduck-simulator 的策略模型（PyTorch 导出的 MLP）、
'''    把策略模型还原成 <c>ConvolutionalNN</c> 并逐值比对推理结果、
'''    卷积网络导出为 ONNX 之后再导入并比对预测值（单精度/双精度、两种展平方式）；
''' 2) PyTorch <c>.pt</c>：读取 KcatNet 的真实归档、写出并再读取做逐值往返比对。
''' </summary>
''' <remarks>
''' 命令行参数 <c>pt</c> 表示只运行 PyTorch <c>.pt</c> 测试（跳过 ONNX 部分）。
''' </remarks>
Module Program

    Const PoliciesDir As String = "G:\microduck-simulator\app\public\policies"

    Sub Main(args As String())
        Dim options As String() = If(args, Array.Empty(Of String)())
        Dim ptOnly As Boolean = options.Any(Function(s) String.Equals(s, "pt", StringComparison.OrdinalIgnoreCase))
        Dim failures As Integer = 0

        If Not ptOnly Then
            Call DebugAudit.Reset()

            Call TestPolicyModels()
            Call TestCnnRoundtrip()

            Dim report As String = IO.Path.Combine(AppContext.BaseDirectory, "onnx-test-report.txt")

            Call DebugAudit.SaveReport(report)

            Call Console.WriteLine()
            Call Console.WriteLine(DebugAudit.Dump())
            Call Console.WriteLine($"report saved to: {report}")
            Call Console.WriteLine($"failures: {DebugAudit.FailureCount}")
            Call Console.WriteLine()

            failures += DebugAudit.FailureCount
        End If

        ' PyTorch .pt 使用独立的测试会话与报告文件
        Call DebugAudit.Reset()
        Call PtTests.Run()

        Dim ptReport As String = IO.Path.Combine(AppContext.BaseDirectory, "pt-test-report.txt")

        Call DebugAudit.SaveReport(ptReport)

        Call Console.WriteLine()
        Call Console.WriteLine(DebugAudit.Dump())
        Call Console.WriteLine($"report saved to: {ptReport}")
        Call Console.WriteLine($"pt failures: {DebugAudit.FailureCount}")

        failures += DebugAudit.FailureCount

        Environment.ExitCode = If(failures = 0, 0, 1)
    End Sub

#Region "POLICY MODELS"

    Private Sub TestPolicyModels()
        Call Console.WriteLine($"== 策略模型测试：{PoliciesDir}")

        If Not Directory.Exists(PoliciesDir) Then
            Call DebugAudit.Test(False, $"策略模型目录不存在：{PoliciesDir}")
            Return
        End If

        Dim files As String() = Directory.GetFiles(PoliciesDir, "*.onnx").OrderBy(Function(f) f).ToArray()

        Call DebugAudit.Test(files.Length = 9, $"策略模型目录中应当有 9 个 .onnx 文件，实际为 {files.Length} 个")

        For Each file As String In files
            Call TestPolicyModel(file)
        Next
    End Sub

    Private Sub TestPolicyModel(file As String)
        Dim name As String = IO.Path.GetFileName(file)

        Try
            Dim model As OnnxModel = OnnxModel.Load(file)

            Call DebugAudit.Test(model.Graph.Node.Count > 0, $"{name}: 计算图不能为空")
            Call DebugAudit.Test(model.Initializers.Count > 0, $"{name}: 应当包含权重初始化器")
            Call DebugAudit.Test(model.Inputs.Length = 1, $"{name}: 应当只有一个输入")
            Call DebugAudit.Test(model.Outputs.Length = 1, $"{name}: 应当只有一个输出")

            Dim summary As String = model.Summary()

            Call DebugAudit.Test(summary.Length > 0, $"{name}: 结构摘要不应为空")

            Dim opTypes As String() = model.GetOpTypes()

            Call DebugAudit.Test(opTypes.Contains("Gemm"), $"{name}: 策略模型应当包含 Gemm 节点")

            Dim obsDims As Long() = model.Inputs(0).GetDims()
            Dim obsLength As Integer = 1

            For Each d As Long In obsDims
                obsLength *= CInt(d)
            Next

            Call DebugAudit.Test(obsLength > 0, $"{name}: 观测向量长度必须为正数")

            Dim evaluator As New OnnxGraphEvaluator(model)
            Dim obs1 As Double() = Pattern(obsLength, 1)
            Dim obs2 As Double() = Pattern(obsLength, 7)
            Dim actions1 As Double() = evaluator.RunSingle(obs1)
            Dim actions2 As Double() = evaluator.RunSingle(obs2)

            Call DebugAudit.Test(actions1.Length > 0, $"{name}: 推理输出不能为空")
            Call DebugAudit.Test(actions1.All(AddressOf IsFiniteValue), $"{name}: 推理输出必须全部是有限值")
            Call DebugAudit.Test(Not actions1.SequenceEqual(actions2), $"{name}: 不同观测应当得到不同动作（权重确实被使用）")

            ' 还原成卷积网络并逐值比对
            Dim cnn As ConvolutionalNN = OnnxImport.ToCNN(model)

            Call DebugAudit.Test(cnn.LayerNum >= 2, $"{name}: 还原后的网络应当至少包含输入层与全连接层")

            Dim db As New DataBlock(obsLength, 1, 1, 0.0)

            For i As Integer = 0 To obsLength - 1
                db.Weights(i) = obs1(i)
            Next

            Dim predicted As Double() = cnn.predict(db)

            Call DebugAudit.AreEqual(actions1.Length, predicted.Length, $"{name}: 输出长度应当一致")

            Dim diff As Double = MaxAbsDifference(actions1, predicted)

            Call DebugAudit.Test(diff < 1.0E-09, $"{name}: 还原网络的预测应当与 ONNX 推理一致（max|Δ| = {diff:G3}）")
            Call DebugAudit.Mark($"[policy] {name}", $"{model.Graph.Node.Count} nodes, {model.Initializers.Count} tensors, max|Δ|={diff:G3}")

            Call Console.WriteLine($"  [ok] {name}: {model.Graph.Node.Count} nodes, {model.Initializers.Count} tensors, max|Δ|={diff:G3}")

        Catch ex As Exception
            Call DebugAudit.Test(False, $"{name}: 抛出异常 {ex.GetType().Name}: {ex.Message}")
            Call Console.WriteLine($"  [fail] {name}: {ex.Message}")
        End Try
    End Sub

#End Region

#Region "CNN ROUNDTRIP"

    Private Sub TestCnnRoundtrip()
        Call Console.WriteLine("== 卷积网络导出 / 导入往返测试")

        For Each channelLast As Boolean In New Boolean() {True, False}
            For Each precision As TensorDataType In New TensorDataType() {TensorDataType.Float, TensorDataType.Float64}
                Call TestRoundtrip(channelLast, precision)
            Next
        Next

        Call TestConvTransposeRoundtrip()
        Call TestUnsupportedExport()
    End Sub

    Private Sub TestRoundtrip(channelLast As Boolean, precision As TensorDataType)
        Dim options As New OnnxExportOptions With {
            .ChannelLastFlatten = channelLast,
            .WeightPrecision = precision,
            .ModelName = "roundtrip"
        }

        Call TestRoundtripWith(BuildCNN(), $"channelLast={channelLast}, precision={precision}", options,
                               {"Conv", "MaxPool", "Relu", "Gemm", "Elu", "Softmax"})
    End Sub

    Private Sub TestConvTransposeRoundtrip()
        Dim options As New OnnxExportOptions With {
            .ChannelLastFlatten = True,
            .WeightPrecision = TensorDataType.Float64,
            .ModelName = "decoder"
        }

        Call TestRoundtripWith(BuildDecoderCNN(), "conv-transpose, precision=Float64", options,
                               {"Conv", "ConvTranspose", "Relu", "Elu", "Gemm", "Softmax"})
    End Sub

    Private Sub TestRoundtripWith(cnn As ConvolutionalNN, label As String, options As OnnxExportOptions,
                                  requiredOps As String())
        Try
            Dim width As Integer = cnn.input.dims.x
            Dim height As Integer = cnn.input.dims.y
            Dim depth As Integer = cnn.input.out_depth
            Dim input As Double() = Pattern(width * height * depth, 3)
            Dim expected As Double() = Predict(cnn, input, width, height, depth)

            Dim model As OnnxModel = cnn.ToOnnx(options)
            Dim ops As String() = model.GetOpTypes()

            For Each required As String In requiredOps
                Call DebugAudit.Test(ops.Contains(required), $"{label}: 导出的计算图应当包含 {required} 节点")
            Next

            ' 1) 用自带的解释器直接求值导出的计算图
            Dim nchw As Double() = ToNCHW(input, width, height, depth)
            Dim evaluator As New OnnxGraphEvaluator(model)
            Dim env As New Dictionary(Of String, OnnxTensor) From {
                {"input", New OnnxTensor(New Integer() {1, depth, height, width}, nchw)}
            }
            Dim outputs As Dictionary(Of String, OnnxTensor) = evaluator.Run(env)
            Dim actual As Double() = outputs.Values.First().Data
            Dim diffEvaluator As Double = MaxAbsDifference(expected, actual)
            Dim tolerance As Double = If(options.WeightPrecision = TensorDataType.Float, 1.0E-06, 1.0E-09)

            Call DebugAudit.AreEqual(expected.Length, actual.Length, $"{label}: 解释器输出的长度应当一致")
            Call DebugAudit.Test(diffEvaluator < tolerance,
                                 $"{label}: 解释器求值应当与原始网络一致（max|Δ| = {diffEvaluator:G3}）")

            ' 2) 落盘 → 重新解析 → 导入 → 比对预测
            Dim path As String = IO.Path.Combine(IO.Path.GetTempPath(),
                                                 $"scibasic_onnx_{label.GetHashCode() And &HFFFF}.onnx")

            Call model.Save(path)

            Dim reloaded As OnnxModel = OnnxModel.Load(path)

            Call DebugAudit.AreEqual(model.Graph.Node.Count, reloaded.Graph.Node.Count, $"{label}: 重新读取后节点数应当一致")
            Call DebugAudit.AreEqual(model.Initializers.Count, reloaded.Initializers.Count, $"{label}: 重新读取后初始化器数应当一致")
            Call DebugAudit.AreEqual(If(options.WeightPrecision = TensorDataType.Float64, TensorDataType.Float64, TensorDataType.Float),
                                     reloaded.Inputs(0).Type.TensorType.ElemType,
                                     $"{label}: 输入的元素类型应当与导出精度一致")

            Dim imported As ConvolutionalNN = OnnxImport.ToCNN(reloaded)
            Dim restored As Double() = Predict(imported, input, width, height, depth)
            Dim diffImport As Double = MaxAbsDifference(expected, restored)

            Call DebugAudit.AreEqual(cnn.LayerNum, imported.LayerNum, $"{label}: 还原网络的层数应当一致")
            Call DebugAudit.AreEqual(expected.Length, restored.Length, $"{label}: 还原网络的输出长度应当一致")
            Call DebugAudit.Test(diffImport < tolerance,
                                 $"{label}: 还原网络的预测应当与原始网络一致（max|Δ| = {diffImport:G3}）")

            ' GPU / 设备端缓存不受影响：再次预测应当得到同样的结果
            Dim repeated As Double() = Predict(imported, input, width, height, depth)

            Call DebugAudit.Test(MaxAbsDifference(restored, repeated) < 1.0E-12, $"{label}: 重复预测应当完全一致")

            Call DebugAudit.Mark($"[roundtrip] {label}",
                                 $"{model.Graph.Node.Count} nodes, evaluator max|Δ|={diffEvaluator:G3}, import max|Δ|={diffImport:G3}")

            Call Console.WriteLine($"  [ok] {label}: nodes={model.Graph.Node.Count}, " &
                                  $"evaluator max|Δ|={diffEvaluator:G3}, import max|Δ|={diffImport:G3}")

            Call File.Delete(path)

        Catch ex As Exception
            Call DebugAudit.Test(False, $"{label}: 抛出异常 {ex.GetType().Name}: {ex.Message}")
            Call Console.WriteLine($"  [fail] {label}: {ex.Message}")
        End Try
    End Sub

    Private Sub TestUnsupportedExport()
        ' Maxout 没有标准 ONNX 算子，导出必须显式报错而不是产出错误模型
        Dim builder As LayerBuilder = New LayerBuilder() +
            CNNLayers.input_layer({8, 8}, 1) +
            CNNLayers.conv_layer(3, 2, 1, 1) +
            CNNLayers.maxout_layer() +
            CNNLayers.full_connected_layer(4) +
            CNNLayers.softmax_layer()

        Dim cnn As New ConvolutionalNN(builder)
        Dim reported As Boolean = False

        Try
            Call cnn.ToOnnx()
        Catch ex As NotSupportedException
            reported = True
        End Try

        Call DebugAudit.Test(reported, "Maxout 层导出时应当抛出 NotSupportedException")
    End Sub

    ''' <summary>
    ''' 构建一个包含卷积 / 池化 / 激活 / 全连接 / Softmax 的小型网络。
    ''' </summary>
    Private Function BuildCNN() As ConvolutionalNN
        Dim builder As LayerBuilder = New LayerBuilder() +
            CNNLayers.input_layer({12, 10}, 1) +
            CNNLayers.conv_layer(3, 4, 1, 1) +
            CNNLayers.relu_layer() +
            CNNLayers.pool_layer(2, 2, 0) +
            CNNLayers.conv_layer(3, 6, 1, 0) +
            CNNLayers.tanh_layer() +
            CNNLayers.full_connected_layer(16) +
            CNNLayers.elu_layer() +
            CNNLayers.full_connected_layer(5) +
            CNNLayers.softmax_layer()

        Return New ConvolutionalNN(builder)
    End Function

    ''' <summary>
    ''' 构建一个包含转置卷积（解码器）的网络，用于覆盖 ConvTranspose 的导出 / 导入路径。
    ''' </summary>
    Private Function BuildDecoderCNN() As ConvolutionalNN
        Dim builder As LayerBuilder = New LayerBuilder() +
            CNNLayers.input_layer({8, 8}, 1) +
            CNNLayers.conv_layer(3, 2, 1, 1) +
            CNNLayers.relu_layer() +
            CNNLayers.conv_transpose_layer(2, 2, 2, 0) +
            CNNLayers.elu_layer() +
            CNNLayers.full_connected_layer(4) +
            CNNLayers.softmax_layer()

        Return New ConvolutionalNN(builder)
    End Function

    Private Function Predict(cnn As ConvolutionalNN, values As Double(), width As Integer, height As Integer, depth As Integer) As Double()
        Dim db As New DataBlock(width, height, depth, 0.0)

        For i As Integer = 0 To values.Length - 1
            db.Weights(i) = values(i)
        Next

        Return cnn.predict(db)
    End Function

    ''' <summary>
    ''' 把框架的 channel-last 特征图转换为 ONNX 的 NCHW 布局。
    ''' </summary>
    Private Function ToNCHW(values As Double(), width As Integer, height As Integer, depth As Integer) As Double()
        Dim result As Double() = New Double(values.Length - 1) {}

        For c As Integer = 0 To depth - 1
            For y As Integer = 0 To height - 1
                For x As Integer = 0 To width - 1
                    result((c * height + y) * width + x) = values((y * width + x) * depth + c)
                Next
            Next
        Next

        Return result
    End Function

#End Region

#Region "HELPERS"

    ''' <summary>
    ''' 生成确定性的伪随机输入（避免每次运行的随机波动）。
    ''' </summary>
    Private Function Pattern(length As Integer, seed As Integer) As Double()
        Dim result As Double() = New Double(length - 1) {}
        Dim state As Long = (seed * 1103515245L + 12345L) And &H7FFFFFFFL

        For i As Integer = 0 To length - 1
            state = (state * 1103515245L + 12345L) And &H7FFFFFFFL
            result(i) = (state Mod 20000L) / 10000.0 - 1.0
        Next

        Return result
    End Function

    Private Function MaxAbsDifference(a As Double(), b As Double()) As Double
        If a.Length <> b.Length Then
            Return Double.PositiveInfinity
        End If

        Dim max As Double = 0.0

        For i As Integer = 0 To a.Length - 1
            Dim d As Double = Math.Abs(a(i) - b(i))

            If d > max Then
                max = d
            End If
        Next

        Return max
    End Function

    Private Function IsFiniteValue(value As Double) As Boolean
        Return Not Double.IsNaN(value) AndAlso Not Double.IsInfinity(value)
    End Function

#End Region

End Module
