' ------------------------------------------------------------------------
' CUDA 上下文（使用 primary context，替代已废弃的 cuCtxCreate）
' ------------------------------------------------------------------------

Namespace Runtime

    Public NotInheritable Class CudaContext
        Implements IDisposable

        Private ReadOnly _device As CudaDevice
        Private _handle As IntPtr
        Private _disposed As Boolean

        Public Sub New(device As CudaDevice)
            If device Is Nothing Then Throw New ArgumentNullException(NameOf(device))
            _device = device

            CudaDevice.EnsureInitialized()

            Dim ctx As IntPtr = IntPtr.Zero
            CudaDriverApi.Check(
                CudaDriverApi.cuDevicePrimaryCtxRetain(ctx, device.Handle), "cuDevicePrimaryCtxRetain")
            CudaDriverApi.Check(CudaDriverApi.cuCtxSetCurrent(ctx), "cuCtxSetCurrent")

            _handle = ctx
        End Sub

        ''' <summary>关联的设备</summary>
        Public ReadOnly Property Device As CudaDevice
            Get
                Return _device
            End Get
        End Property

        ''' <summary>上下文句柄</summary>
        Public ReadOnly Property Handle As IntPtr
            Get
                Return _handle
            End Get
        End Property

        ''' <summary>等待当前上下文中所有已提交的任务执行结束</summary>
        Public Sub Synchronize()
            CudaDriverApi.Check(CudaDriverApi.cuCtxSynchronize(), "cuCtxSynchronize")
        End Sub

        Private Sub Dispose(disposing As Boolean)
            If _disposed Then Return
            _disposed = True

            If _handle <> IntPtr.Zero Then
                Try
                    CudaDriverApi.Check(CudaDriverApi.cuCtxSetCurrent(IntPtr.Zero), "cuCtxSetCurrent")
                Catch
                End Try
                Try
                    CudaDriverApi.Check(CudaDriverApi.cuDevicePrimaryCtxRelease(_device.Handle), "cuDevicePrimaryCtxRelease")
                Catch
                End Try
                _handle = IntPtr.Zero
            End If
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub

        Protected Overrides Sub Finalize()
            Dispose(False)
        End Sub
    End Class
End Namespace
