' ------------------------------------------------------------------------
' 基于 CUDA event 的 GPU 侧计时器
' ------------------------------------------------------------------------

Namespace Runtime

    ''' <summary>
    ''' 使用 cuEvent 精确测量一段 GPU 任务（内核序列）的执行耗时
    ''' </summary>
    Public NotInheritable Class CudaTimer
        Implements IDisposable

        Private ReadOnly _start As IntPtr
        Private ReadOnly _end As IntPtr
        Private _disposed As Boolean

        Public Sub New()
            Dim a As IntPtr = IntPtr.Zero
            Dim b As IntPtr = IntPtr.Zero

            CudaDriverApi.Check(CudaDriverApi.cuEventCreate(a, 0UI), "cuEventCreate")
            CudaDriverApi.Check(CudaDriverApi.cuEventCreate(b, 0UI), "cuEventCreate")

            _start = a
            _end = b
        End Sub

        Public Sub Start()
            CudaDriverApi.Check(CudaDriverApi.cuEventRecord(_start, IntPtr.Zero), "cuEventRecord")
        End Sub

        ''' <summary>记录结束事件并等待其完成，返回毫秒数</summary>
        Public Function Finish() As Double
            CudaDriverApi.Check(CudaDriverApi.cuEventRecord(_end, IntPtr.Zero), "cuEventRecord")
            CudaDriverApi.Check(CudaDriverApi.cuEventSynchronize(_end), "cuEventSynchronize")

            Dim ms As Single = 0
            CudaDriverApi.Check(CudaDriverApi.cuEventElapsedTime(ms, _start, _end), "cuEventElapsedTime")
            Return CDbl(ms)
        End Function

        Public Sub Dispose() Implements IDisposable.Dispose
            If _disposed Then Return
            _disposed = True

            Try
                CudaDriverApi.Check(CudaDriverApi.cuEventDestroy_v2(_start), "cuEventDestroy_v2")
            Catch
            End Try
            Try
                CudaDriverApi.Check(CudaDriverApi.cuEventDestroy_v2(_end), "cuEventDestroy_v2")
            Catch
            End Try
        End Sub
    End Class
End Namespace
