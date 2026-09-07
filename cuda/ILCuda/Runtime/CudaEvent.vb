' ------------------------------------------------------------------------
' CUDA 事件
'
' 用途：
'   1) 记录流上的执行进度并等待（cuEventRecord / cuEventSynchronize）；
'   2) 在两个事件之间做 GPU 侧精确计时（cuEventElapsedTime）；
'   3) 通过 CudaStream.WaitEvent 建立流之间的依赖。
' ------------------------------------------------------------------------

Namespace Runtime

    ''' <summary>事件创建标志</summary>
    Public Enum CudaEventFlags
        ''' <summary>默认：可用于计时</summary>
        DefaultEvent = 0
        ''' <summary>cuEventSynchronize 使用阻塞方式等待</summary>
        BlockingSync = 1
        ''' <summary>不记录时间戳（计时精度无关时可略微降低开销）</summary>
        DisableTiming = 2
    End Enum

    ''' <summary>一个 CUDA 事件</summary>
    Public NotInheritable Class CudaEvent
        Implements IDisposable

        Private _handle As IntPtr
        Private _disposed As Boolean

        Public Sub New(Optional flags As CudaEventFlags = CudaEventFlags.DefaultEvent)
            Dim handle As IntPtr = IntPtr.Zero

            CudaDriverApi.Check(
                CudaDriverApi.cuEventCreate(handle, CUInt(flags)), "cuEventCreate")

            _handle = handle
        End Sub

        ''' <summary>事件句柄</summary>
        Public ReadOnly Property Handle As IntPtr
            Get
                Return _handle
            End Get
        End Property

        ''' <summary>把事件记录到指定流（省略时记录到 NULL 流）</summary>
        Public Sub Record(Optional stream As CudaStream = Nothing)
            ThrowIfDisposed()

            Dim streamHandle = If(stream Is Nothing, IntPtr.Zero, stream.Handle)

            CudaDriverApi.Check(
                CudaDriverApi.cuEventRecord(_handle, streamHandle), "cuEventRecord")
        End Sub

        ''' <summary>阻塞等待该事件完成</summary>
        Public Sub Synchronize()
            ThrowIfDisposed()
            CudaDriverApi.Check(CudaDriverApi.cuEventSynchronize(_handle), "cuEventSynchronize")
        End Sub

        ''' <summary>查询该事件是否已完成（未完成返回 False，其它错误照常抛出）</summary>
        Public Function Query() As Boolean
            ThrowIfDisposed()

            Dim status = CudaDriverApi.cuEventQuery(_handle)

            If status = CUresult.CUDA_SUCCESS Then Return True
            If status = CUresult.CUDA_ERROR_NOT_READY Then Return False

            CudaDriverApi.Check(status, "cuEventQuery")
            Return False
        End Function

        ''' <summary>两个事件之间的耗时（毫秒）</summary>
        Public Shared Function ElapsedMs(startEvent As CudaEvent, endEvent As CudaEvent) As Double
            If startEvent Is Nothing Then Throw New ArgumentNullException(NameOf(startEvent))
            If endEvent Is Nothing Then Throw New ArgumentNullException(NameOf(endEvent))

            Dim ms As Single = 0

            CudaDriverApi.Check(
                CudaDriverApi.cuEventElapsedTime(ms, startEvent.Handle, endEvent.Handle),
                "cuEventElapsedTime")

            Return CDbl(ms)
        End Function

        Private Sub ThrowIfDisposed()
            If _disposed Then Throw New ObjectDisposedException(NameOf(CudaEvent))
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            If _disposed Then Return
            _disposed = True

            If _handle <> IntPtr.Zero Then
                Try
                    CudaDriverApi.Check(CudaDriverApi.cuEventDestroy_v2(_handle), "cuEventDestroy_v2")
                Catch
                End Try
                _handle = IntPtr.Zero
            End If

            GC.SuppressFinalize(Me)
        End Sub

        Public Overrides Function ToString() As String
            Return If(_disposed, "CudaEvent (disposed)", $"CudaEvent 0x{_handle:X}")
        End Function
    End Class
End Namespace
