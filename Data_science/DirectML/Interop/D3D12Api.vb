' ------------------------------------------------------------------------
' D3D12 原生互操作：结构体、枚举、导出函数与 vtable 槽位委托
'
' 槽位号来自 Windows SDK d3d12.h 的 ID3D12xxxVtbl 结构体，并在 RTX A4000 上实测校验：
'   ID3D12Device        8 CreateCommandQueue / 9 CreateCommandAllocator / 12 CreateCommandList /
'                       13 CheckFeatureSupport / 14 CreateDescriptorHeap / 15 GetDescriptorHandleIncrementSize /
'                       27 CreateCommittedResource / 36 CreateFence / 37 GetDeviceRemovedReason
' ------------------------------------------------------------------------

Imports System.Runtime.InteropServices

Namespace Interop

#Region "ENUMS"

    Public Enum D3D_FEATURE_LEVEL As Integer
        LEVEL_11_0 = &HB000
        LEVEL_11_1 = &HB100
        LEVEL_12_0 = &HC000
        LEVEL_12_1 = &HC100
        LEVEL_12_2 = &HC200
    End Enum

    Public Enum D3D12_COMMAND_LIST_TYPE As Integer
        DIRECT = 0
        BUNDLE = 1
        COMPUTE = 2
        COPY = 3
    End Enum

    Public Enum D3D12_COMMAND_QUEUE_FLAGS As Integer
        NONE = 0
        DISABLE_GPU_TIMEOUT = 1
    End Enum

    Public Enum D3D12_HEAP_TYPE As Integer
        DEFAULT_HEAP = 1
        UPLOAD_HEAP = 2
        READBACK_HEAP = 3
        CUSTOM_HEAP = 4
    End Enum

    <Flags>
    Public Enum D3D12_HEAP_FLAGS As UInteger
        NONE = 0
        SHARED_HEAP = 1
        DENY_BUFFERS = 2
        ALLOW_DISPLAY = 4
        SHARED_CROSS_ADAPTER = 8
        DENY_RT_DS_TEXTURES = &H10
        DENY_NON_RT_DS_TEXTURES = &H20
    End Enum

    Public Enum D3D12_CPU_PAGE_PROPERTY As Integer
        UNKNOWN = 0
        NOT_AVAILABLE = 1
        WRITE_COMBINE = 2
    End Enum

    Public Enum D3D12_MEMORY_POOL As Integer
        UNKNOWN = 0
        L0 = 1
        L1 = 2
    End Enum

    Public Enum D3D12_RESOURCE_DIMENSION As Integer
        UNKNOWN = 0
        BUFFER = 1
        TEXTURE1D = 2
        TEXTURE2D = 3
        TEXTURE3D = 4
    End Enum

    Public Enum D3D12_TEXTURE_LAYOUT As Integer
        UNKNOWN = 0
        ROW_MAJOR = 1
        UNDEFINED_SWIZZLE = 2
        STANDARD_SWIZZLE = 3
    End Enum

    <Flags>
    Public Enum D3D12_RESOURCE_FLAGS As UInteger
        NONE = 0
        ALLOW_RENDER_TARGET = 1
        ALLOW_DEPTH_STENCIL = 2
        ALLOW_UNORDERED_ACCESS = 4
        DENY_SHADER_RESOURCE = 8
        ALLOW_CROSS_ADAPTER = &H10
        ALLOW_SIMULTANEOUS_ACCESS = &H20
    End Enum

    <Flags>
    Public Enum D3D12_RESOURCE_STATES As UInteger
        COMMON = 0
        VERTEX_AND_CONSTANT_BUFFER = &H1
        INDEX_BUFFER = &H2
        RENDER_TARGET = &H4
        UNORDERED_ACCESS = &H8
        DEPTH_WRITE = &H10
        DEPTH_READ = &H20
        NON_PIXEL_SHADER_RESOURCE = &H40
        PIXEL_SHADER_RESOURCE = &H80
        STREAM_OUT = &H100
        INDIRECT_ARGUMENT = &H200
        COPY_DEST = &H400
        COPY_SOURCE = &H800
        RESOLVE_DEST = &H1000
        RESOLVE_SOURCE = &H2000
        GENERIC_READ = &H80000000UI
        ALL_SHADER_RESOURCE = &HC0
    End Enum

    Public Enum D3D12_DESCRIPTOR_HEAP_TYPE As Integer
        CBV_SRV_UAV = 0
        SAMPLER = 1
        RTV = 2
        DSV = 3
    End Enum

    <Flags>
    Public Enum D3D12_DESCRIPTOR_HEAP_FLAGS As UInteger
        NONE = 0
        SHADER_VISIBLE = 1
    End Enum

    <Flags>
    Public Enum D3D12_FENCE_FLAGS As UInteger
        NONE = 0
        SHARED_FENCE = 1
        SHARED_CROSS_ADAPTER = 2
        NON_MONITORED = 4
    End Enum

    Public Enum D3D12_RESOURCE_BARRIER_TYPE As Integer
        TRANSITION = 0
        ALIASING = 1
        UAV = 2
    End Enum

    <Flags>
    Public Enum D3D12_RESOURCE_BARRIER_FLAGS As UInteger
        NONE = 0
        BEGIN_ONLY = 1
        END_ONLY = 2
    End Enum

#End Region

#Region "STRUCTS"

    <StructLayout(LayoutKind.Sequential)>
    Public Structure DXGI_SAMPLE_DESC
        Public Count As UInteger
        Public Quality As UInteger
    End Structure

    <StructLayout(LayoutKind.Sequential)>
    Public Structure D3D12_CPU_DESCRIPTOR_HANDLE
        Public ptr As IntPtr
    End Structure

    <StructLayout(LayoutKind.Sequential)>
    Public Structure D3D12_GPU_DESCRIPTOR_HANDLE
        Public ptr As ULong
    End Structure

    <StructLayout(LayoutKind.Sequential)>
    Public Structure D3D12_HEAP_PROPERTIES
        Public Type As D3D12_HEAP_TYPE
        Public CPUPageProperty As D3D12_CPU_PAGE_PROPERTY
        Public MemoryPoolPreference As D3D12_MEMORY_POOL
        Public CreationNodeMask As UInteger
        Public VisibleNodeMask As UInteger

        Public Shared Function CreateDefault() As D3D12_HEAP_PROPERTIES
            Return New D3D12_HEAP_PROPERTIES With {
                .Type = D3D12_HEAP_TYPE.DEFAULT_HEAP,
                .CPUPageProperty = D3D12_CPU_PAGE_PROPERTY.UNKNOWN,
                .MemoryPoolPreference = D3D12_MEMORY_POOL.UNKNOWN,
                .CreationNodeMask = 1,
                .VisibleNodeMask = 1
            }
        End Function

        Public Shared Function CreateUpload() As D3D12_HEAP_PROPERTIES
            Return New D3D12_HEAP_PROPERTIES With {
                .Type = D3D12_HEAP_TYPE.UPLOAD_HEAP,
                .CPUPageProperty = D3D12_CPU_PAGE_PROPERTY.UNKNOWN,
                .MemoryPoolPreference = D3D12_MEMORY_POOL.UNKNOWN,
                .CreationNodeMask = 1,
                .VisibleNodeMask = 1
            }
        End Function

        Public Shared Function CreateReadback() As D3D12_HEAP_PROPERTIES
            Return New D3D12_HEAP_PROPERTIES With {
                .Type = D3D12_HEAP_TYPE.READBACK_HEAP,
                .CPUPageProperty = D3D12_CPU_PAGE_PROPERTY.UNKNOWN,
                .MemoryPoolPreference = D3D12_MEMORY_POOL.UNKNOWN,
                .CreationNodeMask = 1,
                .VisibleNodeMask = 1
            }
        End Function
    End Structure

    <StructLayout(LayoutKind.Sequential)>
    Public Structure D3D12_RESOURCE_DESC
        Public Dimension As D3D12_RESOURCE_DIMENSION
        Public Alignment As ULong
        Public Width As ULong
        Public Height As UInteger
        Public DepthOrArraySize As UShort
        Public MipLevels As UShort
        Public Format As Integer
        Public SampleDesc As DXGI_SAMPLE_DESC
        Public Layout As D3D12_TEXTURE_LAYOUT
        Public Flags As D3D12_RESOURCE_FLAGS
    End Structure

    <StructLayout(LayoutKind.Sequential)>
    Public Structure D3D12_DESCRIPTOR_HEAP_DESC
        Public Type As D3D12_DESCRIPTOR_HEAP_TYPE
        Public NumDescriptors As UInteger
        Public Flags As D3D12_DESCRIPTOR_HEAP_FLAGS
        Public NodeMask As UInteger
    End Structure

    <StructLayout(LayoutKind.Sequential)>
    Public Structure D3D12_RANGE
        Public Begin As IntPtr
        Public [End] As IntPtr
    End Structure

    <StructLayout(LayoutKind.Sequential)>
    Public Structure D3D12_RESOURCE_TRANSITION_BARRIER
        Public pResource As IntPtr
        Public Subresource As UInteger
        Public StateBefore As D3D12_RESOURCE_STATES
        Public StateAfter As D3D12_RESOURCE_STATES
    End Structure

    <StructLayout(LayoutKind.Sequential)>
    Public Structure D3D12_RESOURCE_ALIASING_BARRIER
        Public pResourceBefore As IntPtr
        Public pResourceAfter As IntPtr
    End Structure

    <StructLayout(LayoutKind.Sequential)>
    Public Structure D3D12_RESOURCE_UAV_BARRIER
        Public pResource As IntPtr
    End Structure

    <StructLayout(LayoutKind.Explicit)>
    Public Structure D3D12_RESOURCE_BARRIER_UNION
        <FieldOffset(0)> Public Transition As D3D12_RESOURCE_TRANSITION_BARRIER
        <FieldOffset(0)> Public Aliasing As D3D12_RESOURCE_ALIASING_BARRIER
        <FieldOffset(0)> Public UAV As D3D12_RESOURCE_UAV_BARRIER
    End Structure

    <StructLayout(LayoutKind.Sequential)>
    Public Structure D3D12_RESOURCE_BARRIER
        Public Type As D3D12_RESOURCE_BARRIER_TYPE
        Public Flags As D3D12_RESOURCE_BARRIER_FLAGS
        Public Union As D3D12_RESOURCE_BARRIER_UNION

        ''' <summary>构造一个 UAV 屏障（读写同一段显存时需要显式插入）。</summary>
        Public Shared Function UnorderedAccess(resource As IntPtr) As D3D12_RESOURCE_BARRIER
            Dim barrier As New D3D12_RESOURCE_BARRIER With {
                .Type = D3D12_RESOURCE_BARRIER_TYPE.UAV,
                .Flags = D3D12_RESOURCE_BARRIER_FLAGS.NONE
            }

            barrier.Union.UAV = New D3D12_RESOURCE_UAV_BARRIER With {.pResource = resource}

            Return barrier
        End Function
    End Structure

    <StructLayout(LayoutKind.Sequential)>
    Public Structure D3D12_COMMAND_QUEUE_DESC
        Public Type As D3D12_COMMAND_LIST_TYPE
        Public Priority As Integer
        Public Flags As D3D12_COMMAND_QUEUE_FLAGS
        Public NodeMask As UInteger
    End Structure

#End Region

#Region "EXPORTS & VTABLE SLOTS"

    ''' <summary>D3D12CreateDevice</summary>
    <UnmanagedFunctionPointer(CallingConvention.StdCall)>
    Public Delegate Function D3D12CreateDeviceFn(adapter As IntPtr,
                                                  minimumFeatureLevel As D3D_FEATURE_LEVEL,
                                                  ByRef riid As Guid,
                                                  ByRef ppvDevice As IntPtr) As Integer

    ' ---- ID3D12Device ----

    <UnmanagedFunctionPointer(CallingConvention.StdCall)>
    Public Delegate Function D3D12CreateCommandQueueFn(device As IntPtr, ByRef desc As D3D12_COMMAND_QUEUE_DESC,
                                                       ByRef riid As Guid, ByRef ppv As IntPtr) As Integer

    <UnmanagedFunctionPointer(CallingConvention.StdCall)>
    Public Delegate Function D3D12CreateCommandAllocatorFn(device As IntPtr, type As D3D12_COMMAND_LIST_TYPE,
                                                           ByRef riid As Guid, ByRef ppv As IntPtr) As Integer

    <UnmanagedFunctionPointer(CallingConvention.StdCall)>
    Public Delegate Function D3D12CreateCommandListFn(device As IntPtr, nodeMask As UInteger, type As D3D12_COMMAND_LIST_TYPE,
                                                      allocator As IntPtr, initialState As IntPtr,
                                                      ByRef riid As Guid, ByRef ppv As IntPtr) As Integer

    <UnmanagedFunctionPointer(CallingConvention.StdCall)>
    Public Delegate Function D3D12CheckFeatureSupportFn(device As IntPtr, feature As Integer,
                                                        pSupportData As IntPtr, supportDataSize As UInteger) As Integer

    <UnmanagedFunctionPointer(CallingConvention.StdCall)>
    Public Delegate Function D3D12CreateDescriptorHeapFn(device As IntPtr, ByRef desc As D3D12_DESCRIPTOR_HEAP_DESC,
                                                         ByRef riid As Guid, ByRef ppv As IntPtr) As Integer

    <UnmanagedFunctionPointer(CallingConvention.StdCall)>
    Public Delegate Function D3D12GetDescriptorHandleIncrementSizeFn(device As IntPtr, heapType As D3D12_DESCRIPTOR_HEAP_TYPE) As UInteger

    <UnmanagedFunctionPointer(CallingConvention.StdCall)>
    Public Delegate Function D3D12CreateCommittedResourceFn(device As IntPtr, ByRef heapProperties As D3D12_HEAP_PROPERTIES,
                                                            heapFlags As D3D12_HEAP_FLAGS, ByRef resourceDesc As D3D12_RESOURCE_DESC,
                                                            initialState As D3D12_RESOURCE_STATES, optimizedClearValue As IntPtr,
                                                            ByRef riid As Guid, ByRef ppv As IntPtr) As Integer

    <UnmanagedFunctionPointer(CallingConvention.StdCall)>
    Public Delegate Function D3D12CreateFenceFn(device As IntPtr, initialValue As ULong, flags As D3D12_FENCE_FLAGS,
                                                ByRef riid As Guid, ByRef ppv As IntPtr) As Integer

    <UnmanagedFunctionPointer(CallingConvention.StdCall)>
    Public Delegate Function D3D12GetDeviceRemovedReasonFn(device As IntPtr) As Integer

    ' ---- ID3D12CommandQueue ----

    <UnmanagedFunctionPointer(CallingConvention.StdCall)>
    Public Delegate Sub D3D12ExecuteCommandListsFn(queue As IntPtr, numCommandLists As UInteger, ppCommandLists As IntPtr)

    <UnmanagedFunctionPointer(CallingConvention.StdCall)>
    Public Delegate Function D3D12QueueSignalFn(queue As IntPtr, fence As IntPtr, value As ULong) As Integer

    <UnmanagedFunctionPointer(CallingConvention.StdCall)>
    Public Delegate Function D3D12QueueWaitFn(queue As IntPtr, fence As IntPtr, value As ULong) As Integer

    ' ---- ID3D12CommandAllocator ----

    <UnmanagedFunctionPointer(CallingConvention.StdCall)>
    Public Delegate Function D3D12CommandAllocatorResetFn(allocator As IntPtr) As Integer

    ' ---- ID3D12GraphicsCommandList ----

    <UnmanagedFunctionPointer(CallingConvention.StdCall)>
    Public Delegate Function D3D12CommandListCloseFn(commandList As IntPtr) As Integer

    <UnmanagedFunctionPointer(CallingConvention.StdCall)>
    Public Delegate Function D3D12CommandListResetFn(commandList As IntPtr, allocator As IntPtr, initialState As IntPtr) As Integer

    <UnmanagedFunctionPointer(CallingConvention.StdCall)>
    Public Delegate Sub D3D12CopyBufferRegionFn(commandList As IntPtr, dst As IntPtr, dstOffset As ULong,
                                                src As IntPtr, srcOffset As ULong, numBytes As ULong)

    <UnmanagedFunctionPointer(CallingConvention.StdCall)>
    Public Delegate Sub D3D12ResourceBarrierFn(commandList As IntPtr, numBarriers As UInteger, barriers As IntPtr)

    <UnmanagedFunctionPointer(CallingConvention.StdCall)>
    Public Delegate Sub D3D12SetDescriptorHeapsFn(commandList As IntPtr, numHeaps As UInteger, ppHeaps As IntPtr)

    ' ---- ID3D12Resource ----

    <UnmanagedFunctionPointer(CallingConvention.StdCall)>
    Public Delegate Function D3D12ResourceMapFn(resource As IntPtr, subresource As UInteger,
                                                pReadRange As IntPtr, ByRef ppData As IntPtr) As Integer

    <UnmanagedFunctionPointer(CallingConvention.StdCall)>
    Public Delegate Sub D3D12ResourceUnmapFn(resource As IntPtr, subresource As UInteger, pWrittenRange As IntPtr)

    <UnmanagedFunctionPointer(CallingConvention.StdCall)>
    Public Delegate Function D3D12GetGPUVirtualAddressFn(resource As IntPtr) As ULong

    ' ---- ID3D12Fence ----

    <UnmanagedFunctionPointer(CallingConvention.StdCall)>
    Public Delegate Function D3D12FenceGetCompletedValueFn(fence As IntPtr) As ULong

    <UnmanagedFunctionPointer(CallingConvention.StdCall)>
    Public Delegate Function D3D12FenceSetEventOnCompletionFn(fence As IntPtr, value As ULong, hEvent As IntPtr) As Integer

    <UnmanagedFunctionPointer(CallingConvention.StdCall)>
    Public Delegate Function D3D12FenceSignalFn(fence As IntPtr, value As ULong) As Integer

    ' ---- ID3D12DescriptorHeap ----
    ' 注意：在 _WIN32 分支下，D3D12 的 COM 方法返回结构体时改为
    ' "第二个参数是 RetVal 指针、返回值也是该指针" 的约定，而不是 x64 的 sret。

    <UnmanagedFunctionPointer(CallingConvention.StdCall)>
    Public Delegate Sub D3D12HeapGetCPUDescriptorHandleForHeapStartFn(heap As IntPtr, ByRef result As D3D12_CPU_DESCRIPTOR_HANDLE)

    <UnmanagedFunctionPointer(CallingConvention.StdCall)>
    Public Delegate Sub D3D12HeapGetGPUDescriptorHandleForHeapStartFn(heap As IntPtr, ByRef result As D3D12_GPU_DESCRIPTOR_HANDLE)

#End Region

    ''' <summary>D3D12 相关常量。</summary>
    Public Module D3D12Constants

        ''' <summary>资源绑定偏移/大小要求的对齐粒度。</summary>
        Public Const BufferAlignment As ULong = 256

    End Module

End Namespace
