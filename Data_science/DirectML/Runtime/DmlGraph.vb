' ------------------------------------------------------------------------
' 计算图：编译一次、重复执行
'
' 生命周期的三个硬性约束（DirectML 的要求）：
'   1. 编译后的算子必须先由 OperatorInitializer 初始化一次；
'   2. 每个 dispatch 都要在开始前 Reset 绑定表并重新 Bind*；
'   3. 绑定缓冲必须是 UAV 状态，偏移必须 256 字节对齐。
' ------------------------------------------------------------------------

Imports System.Runtime.InteropServices
Imports Microsoft.VisualBasic.Computing.DirectML.Interop
Imports Microsoft.VisualBasic.Computing.DirectML.Operators

Namespace Runtime

    ''' <summary>图中的一次算子调用。</summary>
    Public NotInheritable Class DmlGraphNode
        Implements IDisposable

        Friend Sub New(definition As DmlOperatorDefinition, inputs As DmlGraphTensor(), outputs As DmlGraphTensor())
            Me.Definition = definition
            Me.Inputs = inputs
            Me.Outputs = outputs
        End Sub

        ''' <summary>算子定义（编译后其描述符内存会被释放）。</summary>
        Public ReadOnly Property Definition As DmlOperatorDefinition

        ''' <summary>输入张量。</summary>
        Public ReadOnly Property Inputs As DmlGraphTensor()

        ''' <summary>输出张量。</summary>
        Public ReadOnly Property Outputs As DmlGraphTensor()

        Friend Property Compiled As DmlCompiledOperator

        Friend Property Binding As DmlBindingTable

        Friend Property DescriptorOffset As UInteger

        Friend Property DescriptorCount As UInteger

        Friend Property Temporary As DmlAllocation

        ''' <summary>调试名称。</summary>
        Public ReadOnly Property Name As String
            Get
                Return Definition.OperatorName
            End Get
        End Property

        Public Sub Dispose() Implements IDisposable.Dispose
            If Binding IsNot Nothing Then
                Binding.Dispose()
                Binding = Nothing
            End If

            If Compiled IsNot Nothing Then
                Compiled.Dispose()
                Compiled = Nothing
            End If

            If Definition IsNot Nothing Then
                Definition.Dispose()
            End If
        End Sub

        Public Overrides Function ToString() As String
            Return $"{Definition.Type}({If(Name, "")})"
        End Function

    End Class

    ''' <summary>
    ''' 一个可以编译并在 GPU 上重复执行的计算图。
    ''' </summary>
    Public NotInheritable Class DmlGraph
        Implements IDisposable

        Private ReadOnly _engine As DmlEngine
        Private ReadOnly _tensors As New List(Of DmlGraphTensor)
        Private ReadOnly _byName As New Dictionary(Of String, DmlGraphTensor)(StringComparer.Ordinal)
        Private ReadOnly _byTensor As New Dictionary(Of DmlTensor, DmlGraphTensor)()
        Private ReadOnly _nodes As New List(Of DmlGraphNode)()
        Private ReadOnly _executionFlags As DML_EXECUTION_FLAGS

        Private _pool As DmlMemoryPool
        Private _upload As DmlBuffer
        Private _readback As DmlBuffer
        Private _heap As DmlDescriptorHeap
        Private _initializer As DmlOperatorInitializer
        Private _initializerBinding As DmlBindingTable
        Private _initializerDescriptorOffset As UInteger
        Private _arenaState As D3D12_RESOURCE_STATES = D3D12_RESOURCE_STATES.COMMON
        Private _compiled As Boolean
        Private _disposed As Boolean

        Friend Sub New(engine As DmlEngine)
            If engine Is Nothing Then
                Throw New ArgumentNullException(NameOf(engine))
            End If

            Me._engine = engine

            If engine.Options.AllowHalfPrecision Then
                Me._executionFlags = DML_EXECUTION_FLAGS.ALLOW_HALF_PRECISION_COMPUTATION
            Else
                Me._executionFlags = DML_EXECUTION_FLAGS.NONE
            End If
        End Sub

        ''' <summary>所属引擎。</summary>
        Public ReadOnly Property Engine As DmlEngine
            Get
                Return _engine
            End Get
        End Property

        ''' <summary>全部张量。</summary>
        Public ReadOnly Property Tensors As IReadOnlyList(Of DmlGraphTensor)
            Get
                Return _tensors
            End Get
        End Property

        ''' <summary>全部节点。</summary>
        Public ReadOnly Property Nodes As IReadOnlyList(Of DmlGraphNode)
            Get
                Return _nodes
            End Get
        End Property

        ''' <summary>是否已编译。</summary>
        Public ReadOnly Property IsCompiled As Boolean
            Get
                Return _compiled
            End Get
        End Property

        ''' <summary>
        ''' 可选的执行阶段回调：用于在与原生驱动交互崩溃时定位到具体步骤。
        ''' </summary>
        Public Property Trace As Action(Of String)

        Private Sub TraceStep(text As String)
            Dim handler As Action(Of String) = Trace

            If handler IsNot Nothing Then
                handler(text)
            End If
        End Sub

        ''' <summary>
        ''' 注册一个张量。
        ''' </summary>
        ''' <param name="name">名称（唯一）。</param>
        ''' <param name="dims">形状。</param>
        ''' <param name="kind">图角色。</param>
        ''' <param name="dataType">数据类型，默认 FP32。</param>
        Public Function DefineTensor(name As String, dims As UInteger(),
                                     Optional kind As DmlTensorKind = DmlTensorKind.Intermediate,
                                     Optional dataType As DML_TENSOR_DATA_TYPE = DML_TENSOR_DATA_TYPE.FLOAT32) As DmlGraphTensor

            If String.IsNullOrEmpty(name) Then
                Throw New ArgumentNullException(NameOf(name))
            End If

            If _byName.ContainsKey(name) Then
                Throw New ArgumentException($"张量名称重复：{name}", NameOf(name))
            End If

            Dim tensor As New DmlTensor(dims, dataType)
            Dim node As New DmlGraphTensor(name, tensor, kind)

            Call _tensors.Add(node)
            _byName(name) = node
            _byTensor(tensor) = node

            Return node
        End Function

        ''' <summary>
        ''' 注册一个共享显存的视图张量（形状/步长不同、数据相同）。
        ''' </summary>
        ''' <param name="name">视图名称。</param>
        ''' <param name="tensor">视图的形状描述（可以带显式步长）。</param>
        ''' <param name="owner">显存宿主。</param>
        ''' <param name="kind">图角色。</param>
        Public Function DefineTensorView(name As String, tensor As DmlTensor, owner As DmlGraphTensor,
                                         Optional kind As DmlTensorKind = DmlTensorKind.Intermediate) As DmlGraphTensor

            If String.IsNullOrEmpty(name) Then
                Throw New ArgumentNullException(NameOf(name))
            End If

            If tensor Is Nothing Then
                Throw New ArgumentNullException(NameOf(tensor))
            End If

            If owner Is Nothing Then
                Throw New ArgumentNullException(NameOf(owner))
            End If

            If _byName.ContainsKey(name) Then
                Throw New ArgumentException($"张量名称重复：{name}", NameOf(name))
            End If

            Dim node As New DmlGraphTensor(name, tensor, kind) With {.AllocationOwner = owner}

            Call _tensors.Add(node)
            _byName(name) = node
            _byTensor(tensor) = node

            Return node
        End Function

        ''' <summary>注册一个常量张量并写入数据。</summary>
        Public Function DefineConstant(name As String, dims As UInteger(), data As Single()) As DmlGraphTensor
            Dim node As DmlGraphTensor = DefineTensor(name, dims, DmlTensorKind.Constant)

            node.SetData(data)

            Return node
        End Function

        ''' <summary>按名称查找张量。</summary>
        Public Function FindTensor(name As String) As DmlGraphTensor
            Dim node As DmlGraphTensor

            If _byName.TryGetValue(name, node) Then
                Return node
            End If

            Return Nothing
        End Function

        ''' <summary>
        ''' 添加一个算子。算子的输入/输出张量必须已经通过 <see cref="DefineTensor"/> 注册。
        ''' </summary>
        Public Sub AddOperator(definition As DmlOperatorDefinition)
            If definition Is Nothing Then
                Throw New ArgumentNullException(NameOf(definition))
            End If

            If _compiled Then
                Throw New InvalidOperationException("图已编译，不能再添加算子")
            End If

            Dim inputs As DmlGraphTensor() = Resolve(definition.Inputs, definition)
            Dim outputs As DmlGraphTensor() = Resolve(definition.Outputs, definition)

            Call _nodes.Add(New DmlGraphNode(definition, inputs, outputs))
        End Sub

        Private Function Resolve(tensors As DmlTensor(), definition As DmlOperatorDefinition) As DmlGraphTensor()
            If tensors Is Nothing OrElse tensors.Length = 0 Then
                Return New DmlGraphTensor() {}
            End If

            Dim result As DmlGraphTensor() = New DmlGraphTensor(tensors.Length - 1) {}

            For i As Integer = 0 To tensors.Length - 1
                Dim found As DmlGraphTensor = Nothing

                If tensors(i) Is Nothing OrElse Not _byTensor.TryGetValue(tensors(i), found) Then
                    Throw New InvalidOperationException(
                        $"算子 {definition.Type} 使用了一个未在图中注册的张量 {If(tensors(i) Is Nothing, "<Nothing>", tensors(i).ToString())}")
                End If

                result(i) = found
            Next

            Return result
        End Function

        ''' <summary>
        ''' 编译整张图：创建 DML 算子、绑定表、显存布局，并完成算子初始化。
        ''' </summary>
        Public Sub Compile()
            ThrowIfDisposed()

            If _compiled Then
                Return
            End If

            If _nodes.Count = 0 Then
                Throw New InvalidOperationException("图中没有任何算子")
            End If

            Dim device As DmlDevice = _engine.Device

            ' 1) 创建并编译算子
            Dim compiledPointers As New List(Of IntPtr)()

            For Each node As DmlGraphNode In _nodes
                TraceStep($"compile: create operator {node.Definition.Type}")

                Dim operatorPtr As IntPtr = device.CreateOperator(node.Definition.DescPtr)

                TraceStep($"compile: compile operator {node.Definition.Type}")

                node.Compiled = device.CompileOperator(operatorPtr, _executionFlags)
                node.Definition.Dispose()

                TraceStep($"compile: compiled operator at 0x{node.Compiled.NativePointer.ToInt64():X}, " &
                          $"descriptors={node.Compiled.BindingProperties.RequiredDescriptorCount}, " &
                          $"temp={node.Compiled.BindingProperties.TemporaryResourceSize}, " &
                          $"raw={BitConverter.ToString(node.Compiled.RawBindingProperties)}")

                Call compiledPointers.Add(node.Compiled.NativePointer)
            Next

            ' 2) 初始化器与描述符堆
            TraceStep("compile: create operator initializer")

            _initializer = device.CreateOperatorInitializer(compiledPointers.ToArray())

            Dim totalDescriptors As UInteger = _initializer.BindingProperties.RequiredDescriptorCount

            For Each node As DmlGraphNode In _nodes
                totalDescriptors += node.Compiled.BindingProperties.RequiredDescriptorCount
            Next

            TraceStep($"compile: create descriptor heap ({totalDescriptors} descriptors)")

            _heap = device.CreateDescriptorHeap(System.Math.Max(1UI, totalDescriptors))

            ' 3) 绑定表
            Dim offset As UInteger = 0UI

            For Each node As DmlGraphNode In _nodes
                Dim count As UInteger = node.Compiled.BindingProperties.RequiredDescriptorCount

                If count = 0UI Then
                    count = 1UI
                End If

                node.DescriptorOffset = offset
                node.DescriptorCount = count

                Dim desc As New DML_BINDING_TABLE_DESC With {
                    .Dispatchable = node.Compiled.NativePointer,
                    .CPUDescriptorHandle = _heap.GetCpuHandle(offset),
                    .GPUDescriptorHandle = _heap.GetGpuHandle(offset),
                    .SizeInDescriptors = count
                }

                TraceStep($"compile: create binding table for {node.Compiled.BindingProperties.RequiredDescriptorCount} descriptors")

                node.Binding = device.CreateBindingTable(desc)
                offset += count
            Next

            _initializerDescriptorOffset = offset

            Dim initCount As UInteger = System.Math.Max(1UI, _initializer.BindingProperties.RequiredDescriptorCount)

            Dim initDesc As New DML_BINDING_TABLE_DESC With {
                .Dispatchable = _initializer.NativePointer,
                .CPUDescriptorHandle = _heap.GetCpuHandle(_initializerDescriptorOffset),
                .GPUDescriptorHandle = _heap.GetGpuHandle(_initializerDescriptorOffset),
                .SizeInDescriptors = initCount
            }

            _initializerBinding = device.CreateBindingTable(initDesc)

            ' 4) 显存布局
            Dim totalBytes As ULong = 0UL
            Dim temporaryBytes As ULong = 0UL

            For Each tensor As DmlGraphTensor In _tensors
                If tensor.AllocationOwner Is Nothing Then
                    totalBytes += DmlMemoryPool.Align(tensor.Tensor.TotalSizeInBytes)
                End If
            Next

            For Each node As DmlGraphNode In _nodes
                temporaryBytes += DmlMemoryPool.Align(node.Compiled.BindingProperties.TemporaryResourceSize)
            Next

            TraceStep($"compile: allocate arena ({totalBytes + temporaryBytes} bytes)")

            _pool = New DmlMemoryPool(device, totalBytes + temporaryBytes, "dml-graph-arena")

            For Each tensor As DmlGraphTensor In _tensors
                If tensor.AllocationOwner Is Nothing Then
                    tensor.Allocation = _pool.Allocate(tensor.Tensor.TotalSizeInBytes)
                End If
            Next

            For Each tensor As DmlGraphTensor In _tensors
                If tensor.AllocationOwner IsNot Nothing Then
                    tensor.Allocation = tensor.AllocationOwner.Allocation
                End If
            Next

            For Each node As DmlGraphNode In _nodes
                Dim temp As ULong = node.Compiled.BindingProperties.TemporaryResourceSize

                If temp > 0UL Then
                    node.Temporary = _pool.Allocate(temp)
                End If
            Next

            ' 5) 上传 / 回读暂存区
            Dim uploadBytes As ULong = 0UL
            Dim readbackBytes As ULong = 0UL

            For Each tensor As DmlGraphTensor In _tensors
                If tensor.Kind = DmlTensorKind.Input OrElse tensor.Kind = DmlTensorKind.Constant Then
                    tensor.UploadOffset = uploadBytes
                    uploadBytes += DmlMemoryPool.Align(tensor.Tensor.TotalSizeInBytes)
                End If
            Next

            For Each tensor As DmlGraphTensor In _tensors
                If tensor.Kind = DmlTensorKind.Output Then
                    tensor.ReadbackOffset = readbackBytes
                    readbackBytes += DmlMemoryPool.Align(tensor.Tensor.TotalSizeInBytes)
                End If
            Next

            TraceStep($"compile: create staging buffers (upload={uploadBytes}, readback={readbackBytes})")

            If uploadBytes > 0UL Then
                _upload = DmlBuffer.CreateUpload(device, uploadBytes, "dml-graph-upload")
            End If

            If readbackBytes > 0UL Then
                _readback = DmlBuffer.CreateReadback(device, readbackBytes, "dml-graph-readback")
            End If

            _compiled = True

            ' 6) 算子初始化（必须在使用之前执行一次）
            TraceStep("compile: run operator initializer")

            Using context As New DmlExecutionContext(device, _engine.Context.Queue.Type)
                context.Begin()

                _initializerBinding.Reset(_initializer.NativePointer, _heap, _initializerDescriptorOffset, initCount)
                context.Recorder.RecordDispatch(context.CommandList, _initializer.NativePointer, _initializerBinding)
                context.Submit()
            End Using
        End Sub

        ''' <summary>设置输入张量的数据（下一次执行时上传）。</summary>
        Public Sub SetInput(name As String, data As Single())
            Dim tensor As DmlGraphTensor = Require(name)

            If tensor.Kind = DmlTensorKind.Constant Then
                Throw New InvalidOperationException($"张量 ""{name}"" 是常量，不能作为输入写入")
            End If

            tensor.SetData(data)
        End Sub

        ''' <summary>读取输出张量的数据（必须在执行之后）。</summary>
        Public Function GetOutput(name As String) As Single()
            Dim tensor As DmlGraphTensor = Require(name)

            Return tensor.Data
        End Function

        Private Function Require(name As String) As DmlGraphTensor
            Dim tensor As DmlGraphTensor = FindTensor(name)

            If tensor Is Nothing Then
                Throw New KeyNotFoundException($"图中不存在张量 ""{name}""")
            End If

            Return tensor
        End Function

        ''' <summary>
        ''' 执行一次：上传输入、记录全部 dispatch、回读输出。
        ''' </summary>
        Public Sub Execute()
            ThrowIfDisposed()

            If Not _compiled Then
                Compile()
            End If

            ' 1) CPU 侧把脏数据写进上传堆（此时 GPU 空闲）
            For Each tensor As DmlGraphTensor In _tensors
                If tensor.StagingDirty Then
                    If _upload Is Nothing Then
                        Throw New InvalidOperationException("图没有输入/常量，却存在待上传的数据")
                    End If

                    WriteStaging(tensor)
                End If
            Next

            Dim context As DmlExecutionContext = _engine.Context
            Dim commandList As DmlCommandList = context.CommandList

            TraceStep("execute: begin")

            context.Begin()

            ' 2) 上传
            Dim hasUpload As Boolean = False

            For Each tensor As DmlGraphTensor In _tensors
                If Not tensor.StagingDirty Then
                    Continue For
                End If

                If _arenaState <> D3D12_RESOURCE_STATES.COPY_DEST Then
                    commandList.Transition(_pool.Buffer, _arenaState, D3D12_RESOURCE_STATES.COPY_DEST)
                    _arenaState = D3D12_RESOURCE_STATES.COPY_DEST
                End If

                commandList.CopyBufferRegion(tensor.Allocation.Buffer, tensor.Allocation.Offset,
                                             _upload, tensor.UploadOffset,
                                             Align4(tensor.Tensor.TotalSizeInBytes))

                hasUpload = True
            Next

            ' 3) 切换到 UAV 并记录 dispatch
            commandList.Transition(_pool.Buffer, _arenaState, D3D12_RESOURCE_STATES.UNORDERED_ACCESS)
            _arenaState = D3D12_RESOURCE_STATES.UNORDERED_ACCESS

            For Each node As DmlGraphNode In _nodes
                TraceStep($"execute: dispatch {node.Definition.Type}")

                Dim inputs As DML_BUFFER_BINDING() = GetBindings(node.Inputs)
                Dim outputs As DML_BUFFER_BINDING() = GetBindings(node.Outputs)

                node.Binding.Reset(node.Compiled.NativePointer, _heap, node.DescriptorOffset, node.DescriptorCount)
                node.Binding.BindInputs(inputs)
                node.Binding.BindOutputs(outputs)

                If node.Temporary.IsValid Then
                    node.Binding.BindTemporaryResource(node.Temporary)
                End If

                context.Recorder.RecordDispatch(commandList, node.Compiled.NativePointer, node.Binding)
            Next

            ' 4) 回读
            Dim hasReadback As Boolean = _readback IsNot Nothing

            If hasReadback Then
                commandList.Transition(_pool.Buffer, D3D12_RESOURCE_STATES.UNORDERED_ACCESS, D3D12_RESOURCE_STATES.COPY_SOURCE)
                _arenaState = D3D12_RESOURCE_STATES.COPY_SOURCE

                For Each tensor As DmlGraphTensor In _tensors
                    If tensor.Kind <> DmlTensorKind.Output Then
                        Continue For
                    End If

                    commandList.CopyBufferRegion(_readback, tensor.ReadbackOffset,
                                                 tensor.Allocation.Buffer, tensor.Allocation.Offset,
                                                 Align4(tensor.Tensor.TotalSizeInBytes))
                Next
            End If

            TraceStep("execute: submit")

            context.Submit()

            TraceStep("execute: read back")

            ' 5) 回读结果到托管内存
            If hasReadback Then
                For Each tensor As DmlGraphTensor In _tensors
                    If tensor.Kind <> DmlTensorKind.Output Then
                        Continue For
                    End If

                    If tensor.Tensor.DataType = DML_TENSOR_DATA_TYPE.FLOAT32 Then
                        Dim count As Integer = CInt(tensor.Tensor.ElementCount)

                        tensor.Data = _readback.ReadSingles(tensor.ReadbackOffset, count)
                    Else
                        Dim size As Integer = CInt(tensor.Tensor.TotalSizeInBytes)
                        Dim bytes As Byte() = New Byte(size - 1) {}

                        _readback.ReadBytes(tensor.ReadbackOffset, bytes, 0, size)
                        tensor.RawData = bytes
                    End If
                Next
            End If

            ' 上传完成，清除脏标记
            For Each tensor As DmlGraphTensor In _tensors
                tensor.StagingDirty = False
            Next
        End Sub

        Private Sub WriteStaging(tensor As DmlGraphTensor)
            If tensor.Tensor.DataType = DML_TENSOR_DATA_TYPE.FLOAT32 Then
                If tensor.Data Is Nothing Then
                    Throw New InvalidOperationException($"张量 ""{tensor.Name}"" 没有数据")
                End If

                _upload.WriteSingles(tensor.UploadOffset, tensor.Data)
            Else
                If tensor.RawData Is Nothing Then
                    Throw New InvalidOperationException($"张量 ""{tensor.Name}"" 没有原始字节数据")
                End If

                _upload.WriteBytes(tensor.UploadOffset, tensor.RawData)
            End If
        End Sub

        ''' <summary>读取输出张量的原始字节（非 FP32 输出）。</summary>
        Public Function GetOutputBytes(name As String) As Byte()
            Dim tensor As DmlGraphTensor = Require(name)

            Return tensor.RawData
        End Function

        Private Shared Function GetBindings(tensors As DmlGraphTensor()) As DML_BUFFER_BINDING()
            If tensors Is Nothing OrElse tensors.Length = 0 Then
                Return New DML_BUFFER_BINDING() {}
            End If

            Dim result As DML_BUFFER_BINDING() = New DML_BUFFER_BINDING(tensors.Length - 1) {}

            For i As Integer = 0 To tensors.Length - 1
                Dim allocation As DmlAllocation = tensors(i).Allocation

                If Not allocation.IsValid Then
                    Throw New InvalidOperationException($"张量 ""{tensors(i).Name}"" 没有分配显存")
                End If

                result(i) = New DML_BUFFER_BINDING(allocation.NativeResource, allocation.Offset,
                                                   tensors(i).Tensor.TotalSizeInBytes)
            Next

            Return result
        End Function

        Private Shared Function Align4(value As ULong) As ULong
            Return ((value + 3UL) \ 4UL) * 4UL
        End Function

        ''' <summary>取得显存占用统计。</summary>
        Public ReadOnly Property MemoryUsage As String
            Get
                If _pool Is Nothing Then
                    Return "<not compiled>"
                End If

                Return _pool.ToString()
            End Get
        End Property

        Private Sub ThrowIfDisposed()
            If _disposed Then
                Throw New ObjectDisposedException(NameOf(DmlGraph))
            End If
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            If _disposed Then
                Return
            End If

            _disposed = True

            If _initializerBinding IsNot Nothing Then
                _initializerBinding.Dispose()
            End If

            If _initializer IsNot Nothing Then
                _initializer.Dispose()
            End If

            For Each node As DmlGraphNode In _nodes
                node.Dispose()
            Next

            If _heap IsNot Nothing Then
                _heap.Dispose()
            End If

            If _readback IsNot Nothing Then
                _readback.Dispose()
            End If

            If _upload IsNot Nothing Then
                _upload.Dispose()
            End If

            If _pool IsNot Nothing Then
                _pool.Dispose()
            End If
        End Sub

        Public Overrides Function ToString() As String
            Return $"graph: {_nodes.Count} nodes, {_tensors.Count} tensors, compiled={_compiled}"
        End Function

    End Class

End Namespace
