' ------------------------------------------------------------------------
' D3D12 设备 + DirectML 设备
'
' 一个 DmlDevice 同时持有 ID3D12Device 与 IDMLDevice，
' 并提供创建队列/分配器/命令列表/Fence/缓冲区/描述符堆/算子/绑定表的工厂方法。
' ------------------------------------------------------------------------

Imports System.Linq
Imports System.Runtime.InteropServices
Imports Microsoft.VisualBasic.Computing.DirectML.Interop

Namespace Runtime

    ''' <summary>
    ''' DirectML 计算设备（D3D12 设备 + DML 设备）。
    ''' </summary>
    Public NotInheritable Class DmlDevice
        Implements IDisposable

        Private ReadOnly _d3d12 As IntPtr
        Private ReadOnly _dml As IntPtr
        Private ReadOnly _releaseD3D12 As ReleaseFn
        Private ReadOnly _releaseDml As ReleaseFn

        Private ReadOnly _createCommandQueue As D3D12CreateCommandQueueFn
        Private ReadOnly _createCommandAllocator As D3D12CreateCommandAllocatorFn
        Private ReadOnly _createCommandList As D3D12CreateCommandListFn
        Private ReadOnly _createDescriptorHeap As D3D12CreateDescriptorHeapFn
        Private ReadOnly _getDescriptorHandleIncrementSize As D3D12GetDescriptorHandleIncrementSizeFn
        Private ReadOnly _createCommittedResource As D3D12CreateCommittedResourceFn
        Private ReadOnly _createFence As D3D12CreateFenceFn
        Private ReadOnly _getD3D12RemovedReason As D3D12GetDeviceRemovedReasonFn

        Private ReadOnly _dmlCheckFeatureSupport As DmlCheckFeatureSupportFn
        Private ReadOnly _dmlCreateOperator As DmlCreateOperatorFn
        Private ReadOnly _dmlCompileOperator As DmlCompileOperatorFn
        Private ReadOnly _dmlCreateInitializer As DmlCreateOperatorInitializerFn
        Private ReadOnly _dmlCreateCommandRecorder As DmlCreateCommandRecorderFn
        Private ReadOnly _dmlCreateBindingTable As DmlCreateBindingTableFn
        Private ReadOnly _dmlGetRemovedReason As DmlGetDeviceRemovedReasonFn

        Private _disposed As Boolean

        Private Sub New(adapter As DmlAdapter, d3d12 As IntPtr, dml As IntPtr)
            Me.Adapter = adapter
            Me._d3d12 = d3d12
            Me._dml = dml
            Me._releaseD3D12 = ComApi.GetDelegate(Of ReleaseFn)(d3d12, 2)
            Me._releaseDml = ComApi.GetDelegate(Of ReleaseFn)(dml, 2)

            Me._createCommandQueue = ComApi.GetDelegate(Of D3D12CreateCommandQueueFn)(d3d12, 8)
            Me._createCommandAllocator = ComApi.GetDelegate(Of D3D12CreateCommandAllocatorFn)(d3d12, 9)
            Me._createCommandList = ComApi.GetDelegate(Of D3D12CreateCommandListFn)(d3d12, 12)
            Me._createDescriptorHeap = ComApi.GetDelegate(Of D3D12CreateDescriptorHeapFn)(d3d12, 14)
            Me._getDescriptorHandleIncrementSize = ComApi.GetDelegate(Of D3D12GetDescriptorHandleIncrementSizeFn)(d3d12, 15)
            Me._createCommittedResource = ComApi.GetDelegate(Of D3D12CreateCommittedResourceFn)(d3d12, 27)
            Me._createFence = ComApi.GetDelegate(Of D3D12CreateFenceFn)(d3d12, 36)
            Me._getD3D12RemovedReason = ComApi.GetDelegate(Of D3D12GetDeviceRemovedReasonFn)(d3d12, 37)

            Me._dmlCheckFeatureSupport = ComApi.GetDelegate(Of DmlCheckFeatureSupportFn)(dml, DmlApi.SlotCheckFeatureSupport)
            Me._dmlCreateOperator = ComApi.GetDelegate(Of DmlCreateOperatorFn)(dml, DmlApi.SlotCreateOperator)
            Me._dmlCompileOperator = ComApi.GetDelegate(Of DmlCompileOperatorFn)(dml, DmlApi.SlotCompileOperator)
            Me._dmlCreateInitializer = ComApi.GetDelegate(Of DmlCreateOperatorInitializerFn)(dml, DmlApi.SlotCreateOperatorInitializer)
            Me._dmlCreateCommandRecorder = ComApi.GetDelegate(Of DmlCreateCommandRecorderFn)(dml, DmlApi.SlotCreateCommandRecorder)
            Me._dmlCreateBindingTable = ComApi.GetDelegate(Of DmlCreateBindingTableFn)(dml, DmlApi.SlotCreateBindingTable)
            Me._dmlGetRemovedReason = ComApi.GetDelegate(Of DmlGetDeviceRemovedReasonFn)(dml, DmlApi.SlotGetDeviceRemovedReason)

            Me.FeatureLevel = QueryFeatureLevel()
        End Sub

        ''' <summary>所属适配器。</summary>
        Public ReadOnly Property Adapter As DmlAdapter

        ''' <summary>ID3D12Device* 指针。</summary>
        Public ReadOnly Property D3D12Device As IntPtr
            Get
                Return _d3d12
            End Get
        End Property

        ''' <summary>IDMLDevice* 指针。</summary>
        Public ReadOnly Property DirectMLDevice As IntPtr
            Get
                Return _dml
            End Get
        End Property

        ''' <summary>设备支持的最高 DirectML 特性级别。</summary>
        Public ReadOnly Property FeatureLevel As DML_FEATURE_LEVEL

        ''' <summary>DirectML 特性级别文本（例如 <c>5.1</c>）。</summary>
        Public ReadOnly Property FeatureLevelText As String
            Get
                Return GetFeatureLevelText(FeatureLevel)
            End Get
        End Property

        ''' <summary>
        ''' 创建 D3D12 设备与 DML 设备。
        ''' </summary>
        ''' <param name="adapter">目标适配器。</param>
        ''' <param name="debug">是否启用调试层。</param>
        ''' <param name="minimumFeatureLevel">要求的最低 DirectML 特性级别。</param>
        Public Shared Function Create(adapter As DmlAdapter,
                                      Optional debug As Boolean = False,
                                      Optional minimumFeatureLevel As DML_FEATURE_LEVEL = DML_FEATURE_LEVEL.LEVEL_1_0) As DmlDevice

            If adapter Is Nothing Then
                Throw New ArgumentNullException(NameOf(adapter))
            End If

            Dim createDevice As D3D12CreateDeviceFn = DmlRuntime.GetCreateDevice()
            Dim iid As Guid = ComIIDs.ID3D12Device
            Dim device As IntPtr = IntPtr.Zero

            HResult.ThrowIfFailed(createDevice(adapter.NativePointer, D3D_FEATURE_LEVEL.LEVEL_11_0, iid, device), "D3D12CreateDevice")

            Dim dml As IntPtr

            Try
                Dim flags As DML_CREATE_DEVICE_FLAGS = DML_CREATE_DEVICE_FLAGS.NONE

                If debug Then
                    flags = DML_CREATE_DEVICE_FLAGS.DEBUG
                End If

                dml = DmlRuntime.CreateDmlDevice(device, minimumFeatureLevel, flags)
            Catch
                Dim release As ReleaseFn = ComApi.GetDelegate(Of ReleaseFn)(device, 2)

                Call release(device)
                Throw
            End Try

            Return New DmlDevice(adapter, device, dml)
        End Function

        Private Function QueryFeatureLevel() As DML_FEATURE_LEVEL
            Dim levels As DML_FEATURE_LEVEL() = {
                DML_FEATURE_LEVEL.LEVEL_6_1,
                DML_FEATURE_LEVEL.LEVEL_6_0,
                DML_FEATURE_LEVEL.LEVEL_5_1,
                DML_FEATURE_LEVEL.LEVEL_5_0,
                DML_FEATURE_LEVEL.LEVEL_4_1,
                DML_FEATURE_LEVEL.LEVEL_4_0,
                DML_FEATURE_LEVEL.LEVEL_3_1,
                DML_FEATURE_LEVEL.LEVEL_3_0,
                DML_FEATURE_LEVEL.LEVEL_2_4,
                DML_FEATURE_LEVEL.LEVEL_2_3,
                DML_FEATURE_LEVEL.LEVEL_2_2,
                DML_FEATURE_LEVEL.LEVEL_2_1,
                DML_FEATURE_LEVEL.LEVEL_2_0,
                DML_FEATURE_LEVEL.LEVEL_1_1,
                DML_FEATURE_LEVEL.LEVEL_1_0
            }

            Using builder As New DmlOperatorBuilder()
                Dim levelsPtr As IntPtr = builder.IntArray(levels.Select(Function(x) CInt(x)).ToArray())
                Dim queryPtr As IntPtr = builder.StructPtr(New DML_FEATURE_QUERY_FEATURE_LEVELS With {
                                                                  .RequestedFeatureLevelCount = CUInt(levels.Length),
                                                                  .RequestedFeatureLevels = levelsPtr
                                                              })
                Dim size As Integer = Marshal.SizeOf(Of DML_FEATURE_DATA_FEATURE_LEVELS)()
                Dim dataPtr As IntPtr = builder.Alloc(size)

                Dim hr As Integer = _dmlCheckFeatureSupport(_dml, DML_FEATURE.FEATURE_LEVELS,
                                                            CUInt(Marshal.SizeOf(Of DML_FEATURE_QUERY_FEATURE_LEVELS)()), queryPtr,
                                                            CUInt(size), dataPtr)

                If hr < 0 Then
                    Return DML_FEATURE_LEVEL.LEVEL_1_0
                End If

                Dim data As DML_FEATURE_DATA_FEATURE_LEVELS = DmlNativeHelper.Read(Of DML_FEATURE_DATA_FEATURE_LEVELS)(dataPtr)

                Return data.MaxSupportedFeatureLevel
            End Using
        End Function

        ''' <summary>
        ''' 查询设备是否支持某个元素类型。
        ''' </summary>
        Public Function IsDataTypeSupported(dataType As DML_TENSOR_DATA_TYPE) As Boolean
            Using builder As New DmlOperatorBuilder()
                Dim queryPtr As IntPtr = builder.StructPtr(New DML_FEATURE_QUERY_TENSOR_DATA_TYPE_SUPPORT With {.DataType = dataType})
                Dim size As Integer = Marshal.SizeOf(Of DML_FEATURE_DATA_TENSOR_DATA_TYPE_SUPPORT)()
                Dim dataPtr As IntPtr = builder.Alloc(size)

                Dim hr As Integer = _dmlCheckFeatureSupport(_dml, DML_FEATURE.TENSOR_DATA_TYPE_SUPPORT,
                                                            CUInt(Marshal.SizeOf(Of DML_FEATURE_QUERY_TENSOR_DATA_TYPE_SUPPORT)()), queryPtr,
                                                            CUInt(size), dataPtr)

                If hr < 0 Then
                    Return False
                End If

                Dim data As DML_FEATURE_DATA_TENSOR_DATA_TYPE_SUPPORT = DmlNativeHelper.Read(Of DML_FEATURE_DATA_TENSOR_DATA_TYPE_SUPPORT)(dataPtr)

                Return data.IsSupported <> 0
            End Using
        End Function

        ''' <summary>把特性级别转成 "1.7" 形式的版本文本。</summary>
        Public Shared Function GetFeatureLevelText(level As DML_FEATURE_LEVEL) As String
            Dim value As Integer = CInt(level)
            Dim major As Integer = (value >> 12) And &HF
            Dim minor As Integer = (value >> 8) And &HF

            Return $"{major}.{minor}"
        End Function

        ''' <summary>创建命令队列。</summary>
        Public Function CreateCommandQueue(Optional type As D3D12_COMMAND_LIST_TYPE = D3D12_COMMAND_LIST_TYPE.COMPUTE) As DmlCommandQueue
            Dim desc As New D3D12_COMMAND_QUEUE_DESC With {
                .Type = type,
                .Priority = 0,
                .Flags = D3D12_COMMAND_QUEUE_FLAGS.NONE,
                .NodeMask = 0UI
            }

            Dim iid As Guid = ComIIDs.ID3D12CommandQueue
            Dim ptr As IntPtr = IntPtr.Zero

            HResult.ThrowIfFailed(_createCommandQueue(_d3d12, desc, iid, ptr), "CreateCommandQueue")

            Return New DmlCommandQueue(ptr, type)
        End Function

        ''' <summary>创建命令分配器。</summary>
        Public Function CreateCommandAllocator(Optional type As D3D12_COMMAND_LIST_TYPE = D3D12_COMMAND_LIST_TYPE.COMPUTE) As DmlCommandAllocator
            Dim iid As Guid = ComIIDs.ID3D12CommandAllocator
            Dim ptr As IntPtr = IntPtr.Zero

            HResult.ThrowIfFailed(_createCommandAllocator(_d3d12, type, iid, ptr), "CreateCommandAllocator")

            Return New DmlCommandAllocator(ptr)
        End Function

        ''' <summary>创建图形命令列表（DirectML 的 RecordDispatch 需要 ID3D12CommandList）。</summary>
        Public Function CreateCommandList(allocator As DmlCommandAllocator,
                                          Optional type As D3D12_COMMAND_LIST_TYPE = D3D12_COMMAND_LIST_TYPE.COMPUTE) As DmlCommandList

            If allocator Is Nothing Then
                Throw New ArgumentNullException(NameOf(allocator))
            End If

            Dim iid As Guid = ComIIDs.ID3D12GraphicsCommandList
            Dim ptr As IntPtr = IntPtr.Zero

            HResult.ThrowIfFailed(_createCommandList(_d3d12, 0UI, type, allocator.NativePointer, IntPtr.Zero, iid, ptr),
                                  "CreateCommandList")

            Return New DmlCommandList(ptr)
        End Function

        ''' <summary>创建 Fence。</summary>
        Public Function CreateFence(Optional initialValue As ULong = 0UL) As DmlFence
            Dim iid As Guid = ComIIDs.ID3D12Fence
            Dim ptr As IntPtr = IntPtr.Zero

            HResult.ThrowIfFailed(_createFence(_d3d12, initialValue, D3D12_FENCE_FLAGS.NONE, iid, ptr), "CreateFence")

            Return New DmlFence(ptr)
        End Function

        ''' <summary>创建描述符堆（默认 shader-visible CBV/SRV/UAV）。</summary>
        Public Function CreateDescriptorHeap(descriptorCount As UInteger) As DmlDescriptorHeap
            If descriptorCount = 0UI Then
                descriptorCount = 1UI
            End If

            Dim desc As New D3D12_DESCRIPTOR_HEAP_DESC With {
                .Type = D3D12_DESCRIPTOR_HEAP_TYPE.CBV_SRV_UAV,
                .NumDescriptors = descriptorCount,
                .Flags = D3D12_DESCRIPTOR_HEAP_FLAGS.SHADER_VISIBLE,
                .NodeMask = 0UI
            }

            Dim iid As Guid = ComIIDs.ID3D12DescriptorHeap
            Dim ptr As IntPtr = IntPtr.Zero

            HResult.ThrowIfFailed(_createDescriptorHeap(_d3d12, desc, iid, ptr), "CreateDescriptorHeap")

            Dim increment As UInteger = _getDescriptorHandleIncrementSize(_d3d12, D3D12_DESCRIPTOR_HEAP_TYPE.CBV_SRV_UAV)

            Return New DmlDescriptorHeap(ptr, descriptorCount, increment)
        End Function

        ''' <summary>创建缓冲区。</summary>
        Public Function CreateBuffer(sizeInBytes As ULong,
                                     Optional heapType As D3D12_HEAP_TYPE = D3D12_HEAP_TYPE.DEFAULT_HEAP,
                                     Optional unorderedAccess As Boolean = True,
                                     Optional name As String = Nothing) As DmlBuffer
            Return DmlBuffer.Create(Me, sizeInBytes, heapType, unorderedAccess, name)
        End Function

        ''' <summary>创建提交资源（供 DmlBuffer 内部使用）。</summary>
        Friend Function CreateCommittedResource(heapProperties As D3D12_HEAP_PROPERTIES,
                                                heapFlags As D3D12_HEAP_FLAGS,
                                                resourceDesc As D3D12_RESOURCE_DESC,
                                                initialState As D3D12_RESOURCE_STATES,
                                                optimizedClearValue As IntPtr,
                                                ByRef riid As Guid,
                                                ByRef ppvResource As IntPtr) As Integer
            If _disposed Then
                Throw New ObjectDisposedException(NameOf(DmlDevice))
            End If

            Return _createCommittedResource(_d3d12, heapProperties, heapFlags, resourceDesc, initialState,
                                            optimizedClearValue, riid, ppvResource)
        End Function

        ''' <summary>创建 DML 算子（descPtr 指向 DML_OPERATOR_DESC）。</summary>
        Public Function CreateOperator(descPtr As IntPtr) As IntPtr
            If descPtr = IntPtr.Zero Then
                Throw New ArgumentNullException(NameOf(descPtr))
            End If

            Dim iid As Guid = ComIIDs.IDMLOperator
            Dim ptr As IntPtr = IntPtr.Zero

            HResult.ThrowIfFailed(_dmlCreateOperator(_dml, descPtr, iid, ptr), "IDMLDevice::CreateOperator")

            Return ptr
        End Function

        ''' <summary>编译算子。</summary>
        Public Function CompileOperator(operatorPtr As IntPtr,
                                        Optional flags As DML_EXECUTION_FLAGS = DML_EXECUTION_FLAGS.NONE) As DmlCompiledOperator

            If operatorPtr = IntPtr.Zero Then
                Throw New ArgumentNullException(NameOf(operatorPtr))
            End If

            Dim iid As Guid = ComIIDs.IDMLCompiledOperator
            Dim ptr As IntPtr = IntPtr.Zero

            Try
                HResult.ThrowIfFailed(_dmlCompileOperator(_dml, operatorPtr, flags, iid, ptr), "IDMLDevice::CompileOperator")
            Finally
                Dim release As ReleaseFn = ComApi.GetDelegate(Of ReleaseFn)(operatorPtr, 2)

                Call release(operatorPtr)
            End Try

            Return New DmlCompiledOperator(ptr)
        End Function

        ''' <summary>创建算子初始化器（编译后的算子必须先通过它初始化一次）。</summary>
        Public Function CreateOperatorInitializer(compiledOperators As IntPtr()) As DmlOperatorInitializer
            Dim count As UInteger = 0UI

            If compiledOperators IsNot Nothing Then
                count = CUInt(compiledOperators.Length)
            End If

            Using builder As New DmlOperatorBuilder()
                Dim arrayPtr As IntPtr = builder.PointerArray(compiledOperators)
                Dim iid As Guid = ComIIDs.IDMLOperatorInitializer
                Dim ptr As IntPtr = IntPtr.Zero

                HResult.ThrowIfFailed(_dmlCreateInitializer(_dml, count, arrayPtr, iid, ptr), "IDMLDevice::CreateOperatorInitializer")

                Return New DmlOperatorInitializer(ptr)
            End Using
        End Function

        ''' <summary>创建命令记录器。</summary>
        Public Function CreateCommandRecorder() As DmlCommandRecorder
            Dim iid As Guid = ComIIDs.IDMLCommandRecorder
            Dim ptr As IntPtr = IntPtr.Zero

            HResult.ThrowIfFailed(_dmlCreateCommandRecorder(_dml, iid, ptr), "IDMLDevice::CreateCommandRecorder")

            Return New DmlCommandRecorder(ptr)
        End Function

        ''' <summary>创建绑定表。</summary>
        Public Function CreateBindingTable(ByRef desc As DML_BINDING_TABLE_DESC) As DmlBindingTable
            Dim descPtr As IntPtr = Marshal.AllocHGlobal(Marshal.SizeOf(Of DML_BINDING_TABLE_DESC)())

            Try
                Marshal.StructureToPtr(desc, descPtr, False)

                Dim iid As Guid = ComIIDs.IDMLBindingTable
                Dim ptr As IntPtr = IntPtr.Zero

                HResult.ThrowIfFailed(_dmlCreateBindingTable(_dml, descPtr, iid, ptr), "IDMLDevice::CreateBindingTable")

                Return New DmlBindingTable(ptr)
            Finally
                Call Marshal.FreeHGlobal(descPtr)
            End Try
        End Function

        ''' <summary>取出设备移除原因（D3D12 侧）。</summary>
        Public Function GetDeviceRemovedReason() As Integer
            Return _getD3D12RemovedReason(_d3d12)
        End Function

        ''' <summary>取出设备移除原因（DirectML 侧）。</summary>
        Public Function GetDirectMLDeviceRemovedReason() As Integer
            Return _dmlGetRemovedReason(_dml)
        End Function

        Public Sub Dispose() Implements IDisposable.Dispose
            If _disposed Then
                Return
            End If

            _disposed = True

            If _dml <> IntPtr.Zero Then
                Call _releaseDml(_dml)
            End If

            If _d3d12 <> IntPtr.Zero Then
                Call _releaseD3D12(_d3d12)
            End If
        End Sub

        Public Overrides Function ToString() As String
            Return $"DML feature level {FeatureLevelText} @ {Adapter.Description}"
        End Function

    End Class

End Namespace
