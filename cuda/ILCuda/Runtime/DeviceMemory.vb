Namespace Runtime

    Public MustInherit Class DeviceMemory
        Implements IDisposable

        Protected _pointer As ULong
        Protected _byteSize As ULong
        Protected _disposed As Boolean

        ''' <summary>设备端指针（CUdeviceptr，64 位）</summary>
        Public ReadOnly Property Pointer As ULong
            Get
                Return _pointer
            End Get
        End Property

        ''' <summary>已分配的字节数</summary>
        Public ReadOnly Property ByteSize As ULong
            Get
                Return _byteSize
            End Get
        End Property

        Protected Sub Allocate(bytes As ULong)
            If bytes = 0 Then Throw New ArgumentOutOfRangeException(NameOf(bytes), "分配的字节数必须大于 0")
            CudaDriverApi.Check(CudaDriverApi.cuMemAlloc_v2(_pointer, bytes), "cuMemAlloc_v2")
            _byteSize = bytes
        End Sub

        Public Sub Clear()
            ThrowIfDisposed()
            CudaDriverApi.Check(CudaDriverApi.cuMemsetD8_v2(_pointer, 0, _byteSize), "cuMemsetD8_v2")
        End Sub

        Protected Sub ThrowIfDisposed()
            If _disposed Then Throw New ObjectDisposedException(Me.GetType().Name)
        End Sub

        Protected Overridable Sub Release()
            If _pointer <> 0 Then
                Try
                    CudaDriverApi.Check(CudaDriverApi.cuMemFree_v2(_pointer), "cuMemFree_v2")
                Catch
                End Try
                _pointer = 0
            End If
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            If _disposed Then Return
            _disposed = True
            Release()
            GC.SuppressFinalize(Me)
        End Sub
    End Class
End Namespace