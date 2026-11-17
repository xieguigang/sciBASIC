Imports System.Linq
Imports Microsoft.VisualBasic.Computing.DirectML.Runtime

Namespace ONNX.DirectML

    ''' <summary>
    ''' 一个 DirectML 推理会话：编译一次、重复执行。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' API 形状与 <see cref="OnnxGraphEvaluator"/> 对齐（<see cref="Run"/> / <see cref="RunSingle"/>），
    ''' 因此同一份调用代码可以在 CPU 后端与 DirectML 后端之间切换，并直接比较两者的输出。
    ''' </para>
    ''' <para>
    ''' 数值差异只来自 FP32 与 FP64 的精度差：输入在边界处由 <see cref="Double"/> 转成
    ''' <see cref="Single"/>，中间结果全程驻留显存，输出再转回 <see cref="Double"/>。
    ''' </para>
    ''' </remarks>
    Public NotInheritable Class DmlOnnxSession
        Implements IDisposable

        ''' <summary>实际使用的后端。</summary>
        Public Enum BackendKind As Integer

            ''' <summary>DirectML（GPU）。</summary>
            DirectML = 0

            ''' <summary>CPU 双精度求值器（回退）。</summary>
            Cpu = 1

        End Enum

        Private ReadOnly _model As OnnxModel
        Private ReadOnly _options As DmlOnnxOptions
        Private ReadOnly _engine As DmlEngine
        Private ReadOnly _graph As DmlGraph
        Private ReadOnly _compiler As DmlOnnxGraphCompiler
        Private ReadOnly _cpu As OnnxGraphEvaluator
        Private ReadOnly _backend As BackendKind
        Private _disposed As Boolean

        Private Sub New(model As OnnxModel, options As DmlOnnxOptions, backend As BackendKind,
                        engine As DmlEngine, graph As DmlGraph, compiler As DmlOnnxGraphCompiler)
            Me._model = model
            Me._options = options
            Me._backend = backend
            Me._engine = engine
            Me._graph = graph
            Me._compiler = compiler

            If backend = BackendKind.Cpu Then
                Me._cpu = New OnnxGraphEvaluator(model)
                Me.Capabilities = Nothing
            Else
                Me.Capabilities = New DmlOnnxCapabilities(engine.Device)
            End If
        End Sub

        ''' <summary>被执行的模型。</summary>
        Public ReadOnly Property Model As OnnxModel
            Get
                Return _model
            End Get
        End Property

        ''' <summary>实际使用的后端。</summary>
        Public ReadOnly Property Backend As BackendKind
            Get
                Return _backend
            End Get
        End Property

        ''' <summary>DirectML 能力矩阵（CPU 回退时为 Nothing）。</summary>
        Public ReadOnly Property Capabilities As DmlOnnxCapabilities

        ''' <summary>底层引擎（CPU 回退时为 Nothing）。</summary>
        Public ReadOnly Property Engine As DmlEngine
            Get
                Return _engine
            End Get
        End Property

        ''' <summary>底层 DML 计算图（CPU 回退时为 Nothing）。</summary>
        Public ReadOnly Property Graph As DmlGraph
            Get
                Return _graph
            End Get
        End Property

        ''' <summary>图输入的名称。</summary>
        Public ReadOnly Property InputNames As String()
            Get
                Return _model.Inputs.Select(Function(v) v.Name).ToArray()
            End Get
        End Property

        ''' <summary>图输出的名称。</summary>
        Public ReadOnly Property OutputNames As String()
            Get
                Return _model.Outputs.Select(Function(v) v.Name).ToArray()
            End Get
        End Property

        ''' <summary>
        ''' 创建会话；失败时抛出异常。
        ''' </summary>
        Public Shared Function Create(model As OnnxModel, Optional options As DmlOnnxOptions = Nothing) As DmlOnnxSession
            If model Is Nothing Then
                Throw New ArgumentNullException(NameOf(model))
            End If

            If options Is Nothing Then
                options = New DmlOnnxOptions()
            End If

            Dim engineOptions As New EngineOptions With {
                .AdapterIndex = options.AdapterIndex,
                .DirectMLDirectory = options.DirectMLDirectory,
                .EnableDebugLayer = options.EnableDebugLayer,
                .AllowHalfPrecision = options.AllowHalfPrecision
            }

            Dim engine As DmlEngine

            Try
                engine = DmlEngine.Create(engineOptions)
            Catch ex As Exception
                If options.AllowCpuFallback Then
                    options.ErrorMessage = ex.Message

                    Return New DmlOnnxSession(model, options, BackendKind.Cpu, Nothing, Nothing, Nothing)
                End If

                Throw
            End Try

            Try
                Dim graph As DmlGraph = engine.CreateGraph()

                graph.Trace = options.Trace

                Dim compiler As New DmlOnnxGraphCompiler(model, graph)

                compiler.Build()
                graph.Compile()

                Return New DmlOnnxSession(model, options, BackendKind.DirectML, engine, graph, compiler)
            Catch ex As Exception
                engine.Dispose()

                If options.AllowCpuFallback AndAlso TypeOf ex IsNot OnnxDmlUnsupportedOperatorException Then
                    options.ErrorMessage = ex.Message

                    Return New DmlOnnxSession(model, options, BackendKind.Cpu, Nothing, Nothing, Nothing)
                End If

                Throw
            End Try
        End Function

        ''' <summary>
        ''' 尝试创建会话；失败时返回 Nothing 并写入 <paramref name="options"/>.ErrorMessage。
        ''' </summary>
        Public Shared Function TryCreate(model As OnnxModel, Optional options As DmlOnnxOptions = Nothing) As DmlOnnxSession
            If options Is Nothing Then
                options = New DmlOnnxOptions()
            End If

            Try
                Return Create(model, options)
            Catch ex As Exception
                options.ErrorMessage = ex.Message

                Return Nothing
            End Try
        End Function

        ''' <summary>
        ''' 执行一次前向计算。
        ''' </summary>
        ''' <param name="inputs">输入张量（键为图输入名称）。</param>
        ''' <returns>输出张量（键为图输出名称）。</returns>
        Public Function Run(inputs As IDictionary(Of String, OnnxTensor)) As Dictionary(Of String, OnnxTensor)
            If _backend = BackendKind.Cpu Then
                Return _cpu.Run(inputs)
            End If

            If inputs IsNot Nothing Then
                For Each item As KeyValuePair(Of String, OnnxTensor) In inputs
                    _graph.SetInput(item.Key, item.Value.Data.Select(Function(v) CSng(v)).ToArray())
                Next
            End If

            _graph.Execute()

            Dim result As New Dictionary(Of String, OnnxTensor)(StringComparer.Ordinal)

            For Each tensor As DmlGraphTensor In _compiler.Outputs
                Dim logical As Integer() = _compiler.GetLogicalShape(tensor.Name)

                If logical Is Nothing OrElse logical.Length = 0 Then
                    logical = New Integer() {1}
                End If

                Dim values As Double() = tensor.Data.Select(Function(v) CDbl(v)).ToArray()

                result(tensor.Name) = New OnnxTensor(logical, values)
            Next

            Return result
        End Function

        ''' <summary>
        ''' 单输入模型的便捷执行入口。
        ''' </summary>
        ''' <param name="input">输入数值（按模型输入形状的解释）。</param>
        ''' <returns>第一个输出的数值。</returns>
        Public Function RunSingle(input As Double()) As Double()
            If _backend = BackendKind.Cpu Then
                Return _cpu.RunSingle(input)
            End If

            Dim names As String() = InputNames

            If names.Length <> 1 Then
                Throw New InvalidOperationException($"RunSingle 只能用于单输入模型，当前模型有 {names.Length} 个输入")
            End If

            Dim env As New Dictionary(Of String, OnnxTensor)(StringComparer.Ordinal)
            Dim dims As Integer() = _compiler.GetLogicalShape(names(0))

            If dims Is Nothing OrElse dims.Length = 0 Then
                dims = New Integer() {input.Length}
            End If

            env(names(0)) = New OnnxTensor(dims, input)

            Dim outputs As Dictionary(Of String, OnnxTensor) = Run(env)

            Return outputs.Values.First().Data
        End Function

        ''' <summary>生成会话摘要。</summary>
        Public Function Summary() As String
            If _backend = BackendKind.Cpu Then
                Return $"ONNX session (CPU fallback): {_model}"
            End If

            Return $"ONNX session ({_engine.Device.Adapter.Description}, DirectML level {_engine.Device.FeatureLevelText}): " &
                   $"{_graph.Nodes.Count} operators, {_graph.Tensors.Count} tensors, {_graph.MemoryUsage}"
        End Function

        Public Sub Dispose() Implements IDisposable.Dispose
            If _disposed Then
                Return
            End If

            _disposed = True

            If _graph IsNot Nothing Then
                _graph.Dispose()
            End If

            If _engine IsNot Nothing Then
                _engine.Dispose()
            End If
        End Sub

        Public Overrides Function ToString() As String
            Return Summary()
        End Function

    End Class

End Namespace
