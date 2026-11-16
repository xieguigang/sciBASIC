' ------------------------------------------------------------------------
' 执行上下文：队列 + 分配器 + 命令列表 + Fence 的提交/等待编排
'
' 整张图的所有 dispatch 记录进同一条命令列表，一次 ExecuteCommandLists
' 加一次 Fence 等待完成，把驱动往返次数压到最低。
' ------------------------------------------------------------------------

Imports Microsoft.VisualBasic.Computing.DirectML.Interop

Namespace Runtime

    ''' <summary>一次 GPU 提交的执行上下文。</summary>
    Public NotInheritable Class DmlExecutionContext
        Implements IDisposable

        Private _fenceValue As ULong
        Private _disposed As Boolean

        ''' <summary>创建执行上下文。</summary>
        ''' <param name="device">设备。</param>
        ''' <param name="queueType">队列类型（COMPUTE 或 DIRECT）。</param>
        Public Sub New(device As DmlDevice, Optional queueType As D3D12_COMMAND_LIST_TYPE = D3D12_COMMAND_LIST_TYPE.COMPUTE)
            If device Is Nothing Then
                Throw New ArgumentNullException(NameOf(device))
            End If

            Me.Device = device
            Me.Queue = device.CreateCommandQueue(queueType)
            Me.Allocator = device.CreateCommandAllocator(queueType)
            Me.CommandList = device.CreateCommandList(Allocator, queueType)
            Me.Fence = device.CreateFence(0UL)
            Me.Recorder = device.CreateCommandRecorder()
        End Sub

        ''' <summary>设备。</summary>
        Public ReadOnly Property Device As DmlDevice

        ''' <summary>命令队列。</summary>
        Public ReadOnly Property Queue As DmlCommandQueue

        ''' <summary>命令分配器。</summary>
        Public ReadOnly Property Allocator As DmlCommandAllocator

        ''' <summary>命令列表。</summary>
        Public ReadOnly Property CommandList As DmlCommandList

        ''' <summary>同步 Fence。</summary>
        Public ReadOnly Property Fence As DmlFence

        ''' <summary>DML 命令记录器。</summary>
        Public ReadOnly Property Recorder As DmlCommandRecorder

        ''' <summary>开始一段新的记录（重置分配器与命令列表）。</summary>
        Public Sub Begin()
            ThrowIfDisposed()

            Allocator.Reset()
            CommandList.Reset(Allocator)
        End Sub

        ''' <summary>关闭命令列表、提交并等待 GPU 完成。</summary>
        ''' <param name="timeoutMs">等待超时（毫秒）。</param>
        Public Sub Submit(Optional timeoutMs As Integer = 60000)
            ThrowIfDisposed()

            CommandList.Close()
            Queue.Execute(CommandList)

            _fenceValue += 1UL
            Queue.Signal(Fence, _fenceValue)

            If Not Fence.WaitForValue(_fenceValue, timeoutMs) Then
                Throw New TimeoutException($"等待 GPU 完成超时（{timeoutMs} ms）；设备移除原因：0x{Device.GetDeviceRemovedReason():X8}")
            End If
        End Sub

        ''' <summary>一次提交完成整段记录。</summary>
        Public Sub SubmitAndWait(Optional timeoutMs As Integer = 60000)
            Begin()
            Submit(timeoutMs)
        End Sub

        Private Sub ThrowIfDisposed()
            If _disposed Then
                Throw New ObjectDisposedException(NameOf(DmlExecutionContext))
            End If
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            If _disposed Then
                Return
            End If

            _disposed = True

            If Recorder IsNot Nothing Then
                Recorder.Dispose()
            End If

            If Fence IsNot Nothing Then
                Fence.Dispose()
            End If

            If CommandList IsNot Nothing Then
                CommandList.Dispose()
            End If

            If Allocator IsNot Nothing Then
                Allocator.Dispose()
            End If

            If Queue IsNot Nothing Then
                Queue.Dispose()
            End If
        End Sub

    End Class

End Namespace
