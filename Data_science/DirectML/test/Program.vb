' ------------------------------------------------------------------------
' DirectML 框架的命令行测试程序
'
' 子命令：
'   info      设备与环境探测（不需要 GPU 也能跑，用于诊断）
'   ops       ONNX 算子 → DirectML 映射表
'   selftest  形状推导自检（纯 CPU，不需要 GPU）
'   onnx      小型 ONNX 模型：DirectML 推理 vs CPU 双精度参考
'
' 所有断言都经过 DebugAudit 记录，最后写出 directml-report.json。
' ------------------------------------------------------------------------

Imports System.Linq
Imports System.Runtime.InteropServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging
Imports Microsoft.VisualBasic.Computing.DirectML.Diagnostics
Imports Microsoft.VisualBasic.Computing.DirectML.Interop
Imports Microsoft.VisualBasic.Computing.DirectML.Operators
Imports Microsoft.VisualBasic.Computing.DirectML.Runtime
Imports Microsoft.VisualBasic.MachineLearning.DataStorage.ONNX
Imports Microsoft.VisualBasic.MachineLearning.DataStorage.ONNX.DirectML

Namespace DirectMLDemo

    Module Program

        Private Const ReportPath As String = "directml-report.json"

        Public Sub Main(args As String())
            Dim command As String = "info"

            If args IsNot Nothing AndAlso args.Length > 0 Then
                command = args(0).ToLowerInvariant()
            End If

            Call DebugAudit.Reset()

            Dim exitCode As Integer = 0

            Try
                Select Case command
                    Case "info"
                        RunInfo()
                        exitCode = 0
                    Case "ops"
                        RunOperators()
                        exitCode = 0
                    Case "selftest"
                        exitCode = If(RunSelfTest(), 0, 1)
                    Case "onnx"
                        exitCode = If(RunOnnx(), 0, 1)
                    Case "device"
                        RunDeviceSteps()
                        exitCode = 0
                    Case "graph"
                        exitCode = If(RunGraphSteps(args), 0, 1)
                    Case Else
                        PrintUsage()
                        exitCode = 2
                End Select
            Catch ex As Exception
                Call Reporter.Fail($"{command} 失败：{ex.GetType().Name}: {ex.Message}")
                Call DebugAudit.Mark($"unhandled exception in {command}: {ex.GetType().Name}")

                exitCode = 1
            End Try

            Call DebugAudit.SaveReport(ReportPath)
            Call Console.WriteLine()
            Call Console.WriteLine($"test report written to {IO.Path.GetFullPath(ReportPath)}")

            System.Environment.ExitCode = exitCode
        End Sub

        Private Sub Trace(text As String)
            Call Console.WriteLine("[step] " & text)
            Call Console.Out.Flush()
        End Sub

        ''' <summary>逐段创建 DirectML 对象，用于定位原生调用崩溃。</summary>
        Private Sub RunDeviceSteps()
            Call Trace("enumerate adapters")

            Dim adapters As List(Of DmlAdapter) = DmlAdapter.Enumerate()

            Call Trace("adapters = " & adapters.Count.ToString())

            For Each adapter As DmlAdapter In adapters
                Call Trace("adapter: " & adapter.ToString())
            Next

            If adapters.Count = 0 Then
                Call DebugAudit.Mark("no DirectML capable adapter found")

                Return
            End If

            Call Trace("create device")

            Dim wrapper As DmlDevice = DmlDevice.Create(adapters(0))

            Call Trace("device: " & wrapper.ToString())

            Using wrapper
                Using queue As DmlCommandQueue = wrapper.CreateCommandQueue()
                    Using allocator As DmlCommandAllocator = wrapper.CreateCommandAllocator()
                        Using list As DmlCommandList = wrapper.CreateCommandList(allocator)
                            Using fence As DmlFence = wrapper.CreateFence()
                                Call Trace("queue / allocator / list / fence created")
                                Call DebugAudit.Test(fence.GetCompletedValue() = 0UL, "fence starts at 0")
                            End Using
                        End Using
                    End Using
                End Using

                Using recorder As DmlCommandRecorder = wrapper.CreateCommandRecorder()
                    Call Trace("command recorder created")
                End Using

                Using heap As DmlDescriptorHeap = wrapper.CreateDescriptorHeap(4UI)
                    Call Trace("descriptor heap created, increment = " & heap.IncrementSize.ToString())
                    Call DebugAudit.Test(heap.IncrementSize > 0UI, "descriptor handle increment")
                End Using

                Call Trace("create buffer")

                Try
                    Using buffer As DmlBuffer = DmlBuffer.CreateDefault(wrapper, 4096UL, True, "probe")
                        Call Trace("buffer created: " & buffer.ToString())
                        Call DebugAudit.Test(buffer.SizeInBytes = 4096UL, "buffer size")
                    End Using
                Catch ex As Exception
                    Call Trace("buffer creation failed: " & ex.Message)
                    Call DebugAudit.Mark("CreateCommittedResource failed: " & ex.Message)
                End Try
            End Using

            For Each adapter As DmlAdapter In adapters
                adapter.Dispose()
            Next
        End Sub

        Private Function RunGraphSteps(args As String()) As Boolean
            Dim op As String = If(args IsNot Nothing AndAlso args.Length > 1, args(1).ToLowerInvariant(), "relu")

            Using engine As DmlEngine = DmlEngine.Create()
                Using graph As DmlGraph = engine.CreateGraph()
                    graph.Trace = Sub(text) Trace(text)

                    Dim input As DmlGraphTensor = graph.DefineTensor("input", {1UI, 1UI, 2UI, 2UI}, DmlTensorKind.Input)
                    Dim output As DmlGraphTensor = graph.DefineTensor("output", {1UI, 1UI, 2UI, 2UI}, DmlTensorKind.Output)

                    Select Case op
                        Case "conv"
                            Dim w As DmlGraphTensor = graph.DefineConstant("w", {1UI, 1UI, 2UI, 2UI},
                                                                          {1.0F, 0.0F, 0.0F, 0.0F,
                                                                           0.0F, 1.0F, 0.0F, 0.0F,
                                                                           0.0F, 0.0F, 1.0F, 0.0F,
                                                                           0.0F, 0.0F, 0.0F, 1.0F})
                            Dim conv As DmlOperatorDefinition = ConvolutionOperators.Convolution(
                                input, w, Nothing, output, {1UI, 1UI}, {1UI, 1UI}, {0UI, 0UI}, {0UI, 0UI},
                                Nothing, 1UI, DML_CONVOLUTION_DIRECTION.FORWARD, "conv")

                            graph.AddOperator(conv)
                        Case Else
                            graph.AddOperator(ActivationOperators.Relu(input, output, "relu"))
                    End Select

                    Call Trace("graph: compile")
                    graph.Compile()

                    Call Trace("graph: set input")
                    graph.SetInput("input", {-1.0F, 2.0F, -3.0F, 4.0F})

                    Call Trace("graph: execute")
                    graph.Execute()

                    Dim result As Single() = graph.GetOutput("output")

                    Call Trace("graph: output = " & String.Join(", ", result))

                    Call DebugAudit.Test(result.Length = 4, "output element count")
                End Using
            End Using

            Return True
        End Function

        Private Sub PrintUsage()
            Call Console.WriteLine("DirectML.Demo <command>")
            Call Console.WriteLine("  info       device & environment diagnostics")
            Call Console.WriteLine("  ops        ONNX operator -> DirectML mapping table")
            Call Console.WriteLine("  selftest   shape inference self test (no GPU required)")
            Call Console.WriteLine("  onnx       run a tiny ONNX model on DirectML and compare with CPU")
        End Sub

        Private Sub RunInfo()
            Dim report As EnvironmentReport = EnvironmentReport.Probe()

            Call Reporter.Environment(report)
            Call DebugAudit.Test(report IsNot Nothing, "environment report available")
            Call DebugAudit.Snapshot(("directml", report.DirectMLPath), ("adapters", report.Adapters.Count))

            If report.IsAvailable Then
                Call Reporter.Ok("DirectML runtime loaded")
            Else
                Call Reporter.Warn("DirectML runtime unavailable: " & If(report.ErrorMessage, "<unknown>"))
            End If

            Using engine As DmlEngine = DmlEngine.TryCreate()

            If engine Is Nothing Then
                Call Reporter.Warn("cannot create a DirectML engine on this machine")
                Return
            End If

            Dim device As DeviceReport = DeviceReport.FromDevice(engine.Device)

            Call Reporter.Title("Device")
            Call Console.WriteLine(device.ToText())
            Call DebugAudit.Mark($"device: {engine}")
        End Using
    End Sub

        Private Sub RunOperators()
            Dim levelText As String = "<cpu>"
            Dim level As DML_FEATURE_LEVEL? = Nothing

            Using engine As DmlEngine = DmlEngine.TryCreate()

                If engine IsNot Nothing Then
                    levelText = engine.Device.FeatureLevelText
                    level = engine.Device.FeatureLevel
                End If
            End Using

            Call Reporter.Capabilities(levelText, level)

            Dim total As Integer = OperatorCatalog.All.Count

            Call DebugAudit.Test(total > 50, $"operator catalog contains {total} entries")
        End Sub

        Private Function RunSelfTest() As Boolean
            Call Reporter.Title("Shape inference self test")

            ' 卷积输出尺寸：(4 + 1 + 1 - 3) / 1 + 1 = 4
            Call AssertEqual("conv output size", 4, DmlShapeInference.OutputSize(4, 3, 1, 1, 1, 1))
            ' 步长 2：(7 - 3) / 2 + 1 = 3
            Call AssertEqual("conv output size (stride 2)", 3, DmlShapeInference.OutputSize(7, 3, 2, 1, 0, 0))
            ' 膨胀 2：(7 - 5) / 1 + 1 = 3
            Call AssertEqual("conv output size (dilation 2)", 3, DmlShapeInference.OutputSize(7, 3, 1, 2, 0, 0))

            ' SAME_UPPER：输出 = ceil(5 / 2) = 3，总 padding = 1
            Dim startPad As Integer() = Nothing
            Dim endPad As Integer() = Nothing

            Call DmlShapeInference.ResolveAutoPadding("SAME_UPPER", {5}, {3}, {2}, {1}, startPad, endPad)

            Call AssertEqual("same upper output", 3, DmlShapeInference.OutputSize(5, 3, 2, 1, startPad(0), endPad(0)))
            ' SAME 的总 padding = (out - 1) * stride + kernel - input = 2 * 2 + 3 - 5 = 2
            Call AssertEqual("same upper padding", 2, startPad(0) + endPad(0))

            ' Reduce 形状
            Dim reduced As UInteger() = ReduceOperators.ReduceShape(New UInteger() {1UI, 3UI, 4UI, 4UI}, New UInteger() {1UI, 2UI}, False)

            Call AssertEqual("reduce rank", 2, reduced.Length)
            Call AssertEqual("reduce dim 0", 1, CInt(reduced(0)))
            ' 轴 1 与 2 都被归约掉，只剩 [1, 4]
            Call AssertEqual("reduce dim 1", 4, CInt(reduced(1)))

            Dim kept As UInteger() = ReduceOperators.ReduceShape(New UInteger() {1UI, 3UI, 4UI, 4UI}, New UInteger() {2UI, 3UI}, True)

            Call AssertEqual("reduce keepdims rank", 4, kept.Length)
            Call AssertEqual("reduce keepdims 1", 1, CInt(kept(2)))
            Call AssertEqual("reduce keepdims 2", 1, CInt(kept(3)))

            ' MatMul 批量广播： [2,3,4] x [4,5] -> [2,3,5]
            Dim matmul As Tuple(Of UInteger(), UInteger(), UInteger()) =
                MatMulOperators.ResolveMatMulShapes({2, 3, 4}, {4, 5})

            Call AssertArray("matmul A", {2UI, 1UI, 3UI, 4UI}, matmul.Item1)
            Call AssertArray("matmul B", {2UI, 1UI, 4UI, 5UI}, matmul.Item2)
            Call AssertArray("matmul C", {2UI, 1UI, 3UI, 5UI}, matmul.Item3)

            Call Reporter.Ok("shape inference checks completed")

            Return True
        End Function

        Private Function RunOnnx() As Boolean
            Call Reporter.Title("ONNX model on DirectML")

            Dim model As OnnxModel = TestModels.CreateTinyConvNet()
            Dim random As New Random(7)
            Dim input As Double() = New Double(15) {}

            For i As Integer = 0 To input.Length - 1
                input(i) = random.NextDouble() * 2.0 - 1.0
            Next

            ' CPU 参考
            Dim cpu As New OnnxGraphEvaluator(model)
            Dim expected As Double() = cpu.RunSingle(input)

            Call Reporter.Info($"{model}")
            Call DebugAudit.Mark("onnx model created")

            Dim options As New DmlOnnxOptions With {.AllowCpuFallback = False}

            If System.Environment.GetEnvironmentVariable("DIRECTML_DEBUG") = "1" Then
                options.Trace = Sub(text) Trace(text)
            End If
            Dim session As DmlOnnxSession = Nothing

            Try
                session = DmlOnnxSession.Create(model, options)
            Catch ex As Exception
                Call Reporter.Warn("DirectML backend unavailable: " & ex.Message)

                If System.Environment.GetEnvironmentVariable("DIRECTML_DEBUG") = "1" Then
                    Call Console.WriteLine(ex.ToString())
                End If

                Call DebugAudit.Mark("DirectML backend skipped: " & ex.Message)
                Call Reporter.Info("CPU reference output: " & String.Join(", ", expected.Select(Function(v) v.ToString("F6"))))

                Return True
            End Try

            Using session
                Call Reporter.Info(session.Summary())

                Dim actual As Double() = session.RunSingle(input)

                Call AssertArrayEqual("onnx output length", expected.Length, actual.Length)

                Dim maxDiff As Double = 0.0

                For i As Integer = 0 To System.Math.Min(expected.Length, actual.Length) - 1
                    maxDiff = System.Math.Max(maxDiff, System.Math.Abs(expected(i) - actual(i)))
                Next

                Call Reporter.Info($"fp32 vs fp64 max abs diff: {maxDiff:E3}")
                Call DebugAudit.Snapshot(("backend", session.Backend.ToString()), ("maxDiff", maxDiff))

                Dim passed As Boolean = DebugAudit.Test(maxDiff < 1.0E-3, $"DirectML 输出与 CPU 参考一致（最大绝对误差 {maxDiff:E3}）")

                If passed Then
                    Call Reporter.Ok("DirectML output matches the CPU reference")
                Else
                    Call Reporter.Fail($"output mismatch: {maxDiff:E3}")
                End If

                Return passed
            End Using
        End Function

        Private Sub AssertEqual(name As String, expected As Integer, actual As Integer)
            Dim passed As Boolean = DebugAudit.Test(expected = actual, $"{name}: expected {expected}, actual {actual}")

            If passed Then
                Call Reporter.Ok($"{name} = {actual}")
            Else
                Call Reporter.Fail($"{name}: expected {expected}, actual {actual}")
            End If
        End Sub

        Private Sub AssertArray(name As String, expected As UInteger(), actual As UInteger())
            Dim passed As Boolean = expected.Length = actual.Length

            If passed Then
                For i As Integer = 0 To expected.Length - 1
                    If expected(i) <> actual(i) Then
                        passed = False
                        Exit For
                    End If
                Next
            End If

            Dim text As String = $"[{String.Join(", ", expected)}] vs [{String.Join(", ", actual)}]"

            Call DebugAudit.Test(passed, $"{name}: {text}")

            If passed Then
                Call Reporter.Ok($"{name} = {text}")
            Else
                Call Reporter.Fail($"{name}: {text}")
            End If
        End Sub

        Private Sub AssertArrayEqual(name As String, expected As Integer, actual As Integer)
            Dim passed As Boolean = DebugAudit.Test(expected = actual, $"{name}: expected {expected}, actual {actual}")

            If Not passed Then
                Call Reporter.Fail($"{name}: expected {expected}, actual {actual}")
            End If
        End Sub

    End Module

End Namespace
