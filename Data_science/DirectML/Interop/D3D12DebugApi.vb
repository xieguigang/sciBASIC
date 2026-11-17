' ------------------------------------------------------------------------
' D3D12 调试层互操作（d3d12sdklayers.h）
'
' 调试层是定位 CreateCommittedResource 这类"只给一个 E_INVALIDARG"问题的唯一
' 直接手段：打开后运行时会针对每次非法调用写一条带文字说明的校验消息，
' 通过 ID3D12InfoQueue 读出来即可知道到底是哪个参数被拒绝。
'
' 槽位号取自 Windows SDK d3d12sdklayers.h 的 ID3D12InfoQueue 声明顺序：
'   3  SetMessageCountLimit      4  ClearStoredMessages      5  GetMessage
'   6  GetNumMessagesAllowedByStorageFilter
'   7  GetNumMessagesDeniedByStorageFilter
'   8  GetNumStoredMessages      ...
' ------------------------------------------------------------------------

Imports System.Runtime.InteropServices

Namespace Interop

    ''' <summary>调试消息分类。</summary>
    Public Enum D3D12_MESSAGE_CATEGORY As Integer
        APPLICATION_DEFINED = 0
        MISCELLANEOUS = 1
        INITIALIZATION = 2
        CLEANUP = 3
        RESOURCE_MANIPULATION = 4
        EXECUTION = 5
        SHADER = 6
        COMPILATION = 7
        STATE_CREATION = 8
        STATE_SETTING = 9
        STATE_GETTING = 10
        SUBRESOURCE_STATE_TRANSITION = 11
        EXECUTION_MARKER = 12
        BARRIER = 13
        SYNCHRONIZATION = 14
        VIDEO = 15
        META_COMMAND = 16
        MESH_SHADER = 17
        LIFETIMES = 18
        DXGI = 19
    End Enum

    ''' <summary>调试消息严重级别。</summary>
    Public Enum D3D12_MESSAGE_SEVERITY As Integer
        CORRUPTION = 0
        ''' <summary>VB 里 Error 是保留字，这里必须用方括号转义。</summary>
        [ERROR] = 1
        WARNING = 2
        INFO = 3
        MESSAGE = 4
    End Enum

    ''' <summary>
    ''' D3D12_MESSAGE：定长头部 + 指向同一块缓冲区内的描述文本。
    ''' </summary>
    <StructLayout(LayoutKind.Sequential)>
    Public Structure D3D12_MESSAGE
        Public Category As D3D12_MESSAGE_CATEGORY
        Public Severity As D3D12_MESSAGE_SEVERITY
        Public ID As Integer
        Public Description As IntPtr
        Public DescriptionByteLength As UIntPtr
    End Structure

    ' ---- d3d12.dll 导出 ----

    <UnmanagedFunctionPointer(CallingConvention.StdCall)>
    Public Delegate Function D3D12GetDebugInterfaceFn(ByRef riid As Guid, ByRef ppv As IntPtr) As Integer

    ' ---- ID3D12Debug ----

    ''' <summary>ID3D12Debug::EnableDebugLayer（槽位 3）。</summary>
    <UnmanagedFunctionPointer(CallingConvention.StdCall)>
    Public Delegate Sub D3D12DebugEnableDebugLayerFn(debug As IntPtr)

    ' ---- ID3D12InfoQueue ----

    ''' <summary>
    ''' ID3D12InfoQueue::GetNumStoredMessages（槽位 8）。
    ''' 注意：这是 <c>UINT64 GetNumStoredMessages(void)</c>，按值直接返回，
    ''' 不是 HRESULT + 输出参数——写成 ByRef 会永远读到 0。
    ''' </summary>
    <UnmanagedFunctionPointer(CallingConvention.StdCall)>
    Public Delegate Function D3D12InfoQueueGetNumStoredMessagesFn(queue As IntPtr) As ULong

    ''' <summary>ID3D12InfoQueue::SetMessageCountLimit（槽位 3）。</summary>
    <UnmanagedFunctionPointer(CallingConvention.StdCall)>
    Public Delegate Function D3D12InfoQueueSetMessageCountLimitFn(queue As IntPtr, limit As ULong) As Integer

    ''' <summary>ID3D12InfoQueue::GetMessageCountLimit（槽位 11，同样按值返回 UINT64）。</summary>
    <UnmanagedFunctionPointer(CallingConvention.StdCall)>
    Public Delegate Function D3D12InfoQueueGetMessageCountLimitFn(queue As IntPtr) As ULong

    ''' <summary>
    ''' ID3D12InfoQueue::GetMessage：先把 <paramref name="message"/> 传 <see cref="IntPtr.Zero"/> 取所需字节数，
    ''' 再按该长度分配缓冲区重复调用，描述文本会追加在结构体之后。
    ''' </summary>
    <UnmanagedFunctionPointer(CallingConvention.StdCall)>
    Public Delegate Function D3D12InfoQueueGetMessageFn(queue As IntPtr, messageIndex As ULong,
                                                        message As IntPtr, ByRef messageByteLength As UIntPtr) As Integer

    <UnmanagedFunctionPointer(CallingConvention.StdCall)>
    Public Delegate Function D3D12InfoQueueClearStoredMessagesFn(queue As IntPtr) As Integer

End Namespace
