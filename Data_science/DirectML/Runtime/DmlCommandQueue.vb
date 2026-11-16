' ------------------------------------------------------------------------
' D3D12 命令队列
' ------------------------------------------------------------------------

Imports Microsoft.VisualBasic.Computing.DirectML.Interop

Namespace Runtime

    ''' <summary>D3D12 命令队列（COMPUTE 或 DIRECT）。</summary>
    Public NotInheritable Class DmlCommandQueue
        Inherits ComObjectBase

        Private ReadOnly _execute As D3D12ExecuteCommandListsFn
        Private ReadOnly _signal As D3D12QueueSignalFn
        Private ReadOnly _wait As D3D12QueueWaitFn

        Friend Sub New(ptr As IntPtr, type As D3D12_COMMAND_LIST_TYPE)
            MyBase.New(ptr)

            Me.Type = type
            Me._execute = GetSlotDelegate(Of D3D12ExecuteCommandListsFn)(10)
            Me._signal = GetSlotDelegate(Of D3D12QueueSignalFn)(14)
            Me._wait = GetSlotDelegate(Of D3D12QueueWaitFn)(15)
        End Sub

        ''' <summary>队列类型。</summary>
        Public ReadOnly Property Type As D3D12_COMMAND_LIST_TYPE

        ''' <summary>提交一个已关闭的命令列表。</summary>
        Public Sub Execute(commandList As DmlCommandList)
            If commandList Is Nothing Then
                Throw New ArgumentNullException(NameOf(commandList))
            End If

            Execute(New DmlCommandList() {commandList})
        End Sub

        ''' <summary>提交一批已关闭的命令列表。</summary>
        Public Sub Execute(commandLists As DmlCommandList())
            If commandLists Is Nothing OrElse commandLists.Length = 0 Then
                Return
            End If

            ThrowIfDisposed()

            Dim pointers As IntPtr() = New IntPtr(commandLists.Length - 1) {}

            For i As Integer = 0 To commandLists.Length - 1
                pointers(i) = commandLists(i).NativePointer
            Next

            Dim handle As System.Runtime.InteropServices.GCHandle =
                System.Runtime.InteropServices.GCHandle.Alloc(pointers, System.Runtime.InteropServices.GCHandleType.Pinned)

            Try
                Call _execute(NativePointer, CUInt(pointers.Length), handle.AddrOfPinnedObject())
            Finally
                Call handle.Free()
            End Try
        End Sub

        ''' <summary>在队列上发出 Fence 信号。</summary>
        Public Sub Signal(fence As DmlFence, value As ULong)
            If fence Is Nothing Then
                Throw New ArgumentNullException(NameOf(fence))
            End If

            HResult.ThrowIfFailed(_signal(NativePointer, fence.NativePointer, value), "ID3D12CommandQueue::Signal")
        End Sub

        ''' <summary>让队列等待某个 Fence 值。</summary>
        Public Sub Wait(fence As DmlFence, value As ULong)
            If fence Is Nothing Then
                Throw New ArgumentNullException(NameOf(fence))
            End If

            HResult.ThrowIfFailed(_wait(NativePointer, fence.NativePointer, value), "ID3D12CommandQueue::Wait")
        End Sub

        Protected Overrides Sub Release()
            ReleaseNative()
        End Sub

    End Class

End Namespace
