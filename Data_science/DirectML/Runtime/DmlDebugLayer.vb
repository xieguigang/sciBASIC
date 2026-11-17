' ------------------------------------------------------------------------
' D3D12 调试层与 InfoQueue 的托管封装
'
' 用法：
'   DmlDebugLayer.TryEnable()            ' 必须在创建 D3D12 设备之前调用
'   DmlDevice.Create(adapter)            ' 创建后会自动 Attach 到 InfoQueue
'   Dim msgs = DmlDebugLayer.Capture()   ' 取走并清空累积的校验消息
'
' 设计取舍：调试层会显著降低执行速度并增加内存占用，因此默认关闭，
' 只在 EngineOptions.EnableDebugLayer = True 或显式调用 TryEnable 时启用。
' ------------------------------------------------------------------------

Imports System.Runtime.InteropServices
Imports Microsoft.VisualBasic.Computing.DirectML.Diagnostics
Imports Microsoft.VisualBasic.Computing.DirectML.Interop

Namespace Runtime

    ''' <summary>
    ''' D3D12 调试层的启用与调试消息捕获。
    ''' </summary>
    Public NotInheritable Class DmlDebugLayer

        Private Shared ReadOnly _sync As New Object()

        Private Shared _enabled As Boolean
        Private Shared _enableError As String

        Private Shared _queue As IntPtr
        Private Shared _getCount As D3D12InfoQueueGetNumStoredMessagesFn
        Private Shared _getMessage As D3D12InfoQueueGetMessageFn
        Private Shared _clearMessages As D3D12InfoQueueClearStoredMessagesFn

        ''' <summary>调试层是否已成功启用。</summary>
        Public Shared ReadOnly Property IsEnabled As Boolean
            Get
                SyncLock _sync
                    Return _enabled
                End SyncLock
            End Get
        End Property

        ''' <summary>启用失败的原因（成功时为 Nothing）。</summary>
        Public Shared ReadOnly Property EnableError As String
            Get
                SyncLock _sync
                    Return _enableError
                End SyncLock
            End Get
        End Property

        ''' <summary>是否已连接到某个设备的 InfoQueue。</summary>
        Public Shared ReadOnly Property IsAttached As Boolean
            Get
                SyncLock _sync
                    Return _queue <> IntPtr.Zero
                End SyncLock
            End Get
        End Property

        ''' <summary>
        ''' 启用 D3D12 调试层（必须在创建 D3D12 设备之前调用）。
        ''' </summary>
        ''' <returns>是否启用成功。</returns>
        ''' <remarks>
        ''' 需要系统安装 "图形工具" 可选功能；未安装时 <c>D3D12GetDebugInterface</c>
        ''' 会返回 <c>DXGI_ERROR_SDK_COMPONENT_MISSING</c>，此时会记录原因并返回 False，
        ''' 而不是抛异常——调试能力缺失不应该阻止框架运行。
        ''' </remarks>
        Public Shared Function TryEnable() As Boolean
            SyncLock _sync
                If _enabled Then
                    Return True
                End If

                _enableError = Nothing

                Dim d3d12Module As NativeModule = DmlRuntime.D3D12

                If d3d12Module Is Nothing Then
                    _enableError = "d3d12.dll 不可用：" & If(DmlRuntime.LastError, "未知原因")

                    Return False
                End If

                Dim getDebugInterface As D3D12GetDebugInterfaceFn =
                    d3d12Module.GetDelegate(Of D3D12GetDebugInterfaceFn)("D3D12GetDebugInterface", optional:=True)

                If getDebugInterface Is Nothing Then
                    _enableError = "d3d12.dll 未导出 D3D12GetDebugInterface"

                    Return False
                End If

                Dim iid As Guid = ComIIDs.ID3D12Debug
                Dim debug As IntPtr = IntPtr.Zero
                Dim hr As Integer = getDebugInterface(iid, debug)

                If hr < 0 OrElse debug = IntPtr.Zero Then
                    _enableError = "D3D12GetDebugInterface 失败：" & HResult.Describe(hr) &
                                   "；请安装 Windows 的『图形工具』可选功能"

                    Return False
                End If

                Try
                    Dim enable As D3D12DebugEnableDebugLayerFn =
                        ComApi.GetDelegate(Of D3D12DebugEnableDebugLayerFn)(debug, 3)

                    Call enable(debug)
                Finally
                    Dim release As ReleaseFn = ComApi.GetDelegate(Of ReleaseFn)(debug, 2)

                    Call release(debug)
                End Try

                _enabled = True
                _enableError = Nothing

                Return True
            End SyncLock
        End Function

        ''' <summary>
        ''' 连接设备的 InfoQueue，之后即可通过 <see cref="Capture"/> 读取校验消息。
        ''' </summary>
        ''' <param name="d3d12Device">ID3D12Device*。</param>
        ''' <returns>是否连接成功（未启用调试层时恒为 False）。</returns>
        Public Shared Function TryAttach(d3d12Device As IntPtr) As Boolean
            If d3d12Device = IntPtr.Zero Then
                Return False
            End If

            SyncLock _sync
                If Not _enabled Then
                    Return False
                End If

                If _queue <> IntPtr.Zero Then
                    ' 同一个进程里多个设备共用一个 InfoQueue 没有意义，替换为最新的设备。
                    Call ReleaseQueue()
                End If

                Dim queryInterface As QueryInterfaceFn = ComApi.GetDelegate(Of QueryInterfaceFn)(d3d12Device, 0)
                Dim iid As Guid = ComIIDs.ID3D12InfoQueue
                Dim queue As IntPtr = IntPtr.Zero
                Dim hr As Integer = queryInterface(d3d12Device, iid, queue)

                If hr < 0 OrElse queue = IntPtr.Zero Then
                    Return False
                End If

                _getCount = ComApi.GetDelegate(Of D3D12InfoQueueGetNumStoredMessagesFn)(queue, 8)
                _getMessage = ComApi.GetDelegate(Of D3D12InfoQueueGetMessageFn)(queue, 5)
                _clearMessages = ComApi.GetDelegate(Of D3D12InfoQueueClearStoredMessagesFn)(queue, 4)
                _queue = queue

                ' 设备创建之前积攒的消息也一并清掉，避免诊断输出里混入无关条目。
                Call Clear()

                Return True
            End SyncLock
        End Function

        ''' <summary>
        ''' 取走并清空当前累积的全部校验消息。
        ''' </summary>
        ''' <param name="operation">标注消息来源的操作名（可选，写入 <see cref="DmlValidationMessage.Operation"/>）。</param>
        Public Shared Function Capture(Optional operation As String = Nothing) As DmlValidationMessage()
            SyncLock _sync
                Dim messages As New List(Of DmlValidationMessage)()

                If _queue = IntPtr.Zero OrElse _getCount Is Nothing OrElse _getMessage Is Nothing Then
                    Return messages.ToArray()
                End If

                Dim count As ULong = _getCount(_queue)

                ' 防御性上界：消息数量异常时不要让循环/分配失控。
                Dim total As Integer = CInt(System.Math.Min(count, 512UL))

                For index As Integer = 0 To total - 1
                    Dim messageIndex As ULong = CULng(index)
                    Dim size As UIntPtr = UIntPtr.Zero

                    If _getMessage(_queue, messageIndex, IntPtr.Zero, size) < 0 Then
                        Continue For
                    End If

                    Dim bytes As ULong = size.ToUInt64()

                    If bytes = 0UL OrElse bytes > CULng(Integer.MaxValue) Then
                        Continue For
                    End If

                    Dim buffer As IntPtr = Marshal.AllocHGlobal(CInt(bytes))

                    Try
                        Dim actual As UIntPtr = size

                        If _getMessage(_queue, messageIndex, buffer, actual) < 0 Then
                            Continue For
                        End If

                        Dim [structure] As D3D12_MESSAGE = Marshal.PtrToStructure(Of D3D12_MESSAGE)(buffer)
                        Dim text As String = ReadAnsi([structure].Description, [structure].DescriptionByteLength)

                        Call messages.Add(New DmlValidationMessage(
                            [structure].Severity.ToString(),
                            [structure].Category.ToString(),
                            [structure].ID,
                            text,
                            operation))
                    Finally
                        Call Marshal.FreeHGlobal(buffer)
                    End Try
                Next

                Call Clear()

                Return messages.ToArray()
            End SyncLock
        End Function

        ''' <summary>当前排队中的校验消息条数（未挂接时为 0）。</summary>
        Public Shared ReadOnly Property PendingMessageCount As ULong
            Get
                SyncLock _sync
                    If _queue = IntPtr.Zero OrElse _getCount Is Nothing Then
                        Return 0UL
                    End If

                    Return _getCount(_queue)
                End SyncLock
            End Get
        End Property

        ''' <summary>
        ''' 自检：通过「写入消息数量上限再读回」验证 InfoQueue 的 vtable 槽位绑定是否正确。
        ''' </summary>
        ''' <returns>形如 <c>limit 64 -> 64</c> 的文本；未挂接时返回说明。</returns>
        Public Shared Function SelfTest() As String
            SyncLock _sync
                If _queue = IntPtr.Zero Then
                    Return "info queue 未挂接"
                End If

                Dim setLimit As D3D12InfoQueueSetMessageCountLimitFn =
                    ComApi.GetDelegate(Of D3D12InfoQueueSetMessageCountLimitFn)(_queue, 3)
                Dim getLimit As D3D12InfoQueueGetMessageCountLimitFn =
                    ComApi.GetDelegate(Of D3D12InfoQueueGetMessageCountLimitFn)(_queue, 11)

                Const probe As ULong = 64UL
                Dim hrSet As Integer = setLimit(_queue, probe)
                Dim limit As ULong = getLimit(_queue)

                Return $"setLimit=0x{hrSet:X8}, limit {probe} -> {limit}"
            End SyncLock
        End Function

        ''' <summary>清空累积的校验消息。</summary>
        Public Shared Sub Clear()
            SyncLock _sync
                If _queue <> IntPtr.Zero AndAlso _clearMessages IsNot Nothing Then
                    Call _clearMessages(_queue)
                End If
            End SyncLock
        End Sub

        ''' <summary>释放 InfoQueue 并恢复为未启用状态（仅用于测试/重新初始化）。</summary>
        Public Shared Sub Reset()
            SyncLock _sync
                Call ReleaseQueue()

                _enabled = False
                _enableError = Nothing
            End SyncLock
        End Sub

        ''' <summary>
        ''' 把消息列表压成一行用于异常信息（最多 <paramref name="maxItems"/> 条）。
        ''' </summary>
        Public Shared Function Summarize(messages As IEnumerable(Of DmlValidationMessage),
                                         Optional maxItems As Integer = 3) As String

            If messages Is Nothing Then
                Return Nothing
            End If

            Dim items As DmlValidationMessage() = messages.ToArray()

            If items.Length = 0 Then
                Return Nothing
            End If

            Dim text As String = String.Join(" / ", items.Take(maxItems).Select(Function(m) $"{m.Category}#{m.Id}: {m.Description}"))

            If items.Length > maxItems Then
                text &= $" …（共 {items.Length} 条）"
            End If

            Return text
        End Function

        Private Shared Sub ReleaseQueue()
            If _queue <> IntPtr.Zero Then
                Dim release As ReleaseFn = ComApi.GetDelegate(Of ReleaseFn)(_queue, 2)

                Call release(_queue)
            End If

            _queue = IntPtr.Zero
            _getCount = Nothing
            _getMessage = Nothing
            _clearMessages = Nothing
        End Sub

        Private Shared Function ReadAnsi(pointer As IntPtr, byteLength As UIntPtr) As String
            If pointer = IntPtr.Zero Then
                Return Nothing
            End If

            Dim length As ULong = byteLength.ToUInt64()

            ' DescriptionByteLength 包含结尾的 '\0'
            If length <= 1UL Then
                Return Marshal.PtrToStringAnsi(pointer)
            End If

            Dim count As Integer = CInt(System.Math.Min(length - 1UL, CULng(Integer.MaxValue)))

            Return Marshal.PtrToStringAnsi(pointer, count)
        End Function

    End Class

End Namespace

