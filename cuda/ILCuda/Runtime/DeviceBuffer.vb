' ------------------------------------------------------------------------
' 显存缓冲区
'
' DeviceMemory 为与类型无关的基类（内核参数只需要它的 64 位设备指针），
' DeviceBuffer(Of T) 在上面提供强类型的主机 <-> 设备拷贝能力：
'   * Write / Read              —— 同步拷贝（内部按需固定托管数组）
'   * WriteAsync / ReadAsync    —— 异步拷贝，主机端必须是 PinnedHostBuffer
'   * CopyTo / CopyToAsync      —— 设备到设备拷贝
' ------------------------------------------------------------------------

Imports System.Runtime.InteropServices

Namespace Runtime

    ''' <summary>
    ''' 一段类型化的显存（仅支持可直接内存拷贝的值类型，例如 Single）
    ''' </summary>
    Public NotInheritable Class DeviceBuffer(Of T As Structure)
        Inherits DeviceMemory

        Private ReadOnly _count As Integer
        Private ReadOnly _elementSize As Integer

        Public Sub New(count As Integer)
            If count <= 0 Then Throw New ArgumentOutOfRangeException(NameOf(count))

            _count = count
            _elementSize = Marshal.SizeOf(GetType(T))
            Allocate(CULng(count) * CULng(_elementSize))
        End Sub

        ''' <summary>元素个数</summary>
        Public ReadOnly Property Count As Integer
            Get
                Return _count
            End Get
        End Property

        ''' <summary>单个元素的字节数</summary>
        Public ReadOnly Property ElementSize As Integer
            Get
                Return _elementSize
            End Get
        End Property

        ''' <summary>把主机数组拷贝到显存（H -> D，同步）</summary>
        Public Sub Write(data As T())
            ThrowIfDisposed()
            If data Is Nothing Then Throw New ArgumentNullException(NameOf(data))
            If data.Length <> _count Then
                Throw New ArgumentException($"主机数组长度 {data.Length} 与缓冲区长度 {_count} 不一致")
            End If

            Dim pin = GCHandle.Alloc(data, GCHandleType.Pinned)
            Try
                CudaDriverApi.Check(
                    CudaDriverApi.cuMemcpyHtoD_v2(_pointer, pin.AddrOfPinnedObject(), _byteSize), "cuMemcpyHtoD_v2")
            Finally
                pin.Free()
            End Try
        End Sub

        ''' <summary>从显存读回主机数组（D -> H，同步）</summary>
        Public Function Read() As T()
            ThrowIfDisposed()

            Dim result(_count - 1) As T
            Dim pin = GCHandle.Alloc(result, GCHandleType.Pinned)
            Try
                CudaDriverApi.Check(
                    CudaDriverApi.cuMemcpyDtoH_v2(pin.AddrOfPinnedObject(), _pointer, _byteSize), "cuMemcpyDtoH_v2")
            Finally
                pin.Free()
            End Try

            Return result
        End Function

        ' ------------------------------------------------------------------
        ' 异步拷贝（主机端必须是页锁定内存）
        ' ------------------------------------------------------------------

        ''' <summary>
        ''' 异步上传（H -> D）。主机端必须是 <see cref="PinnedHostBuffer(Of T)"/>；
        ''' 调用方需要在读取结果前对流做同步。
        ''' </summary>
        Public Sub WriteAsync(source As PinnedHostBuffer(Of T), Optional stream As CudaStream = Nothing)
            ThrowIfDisposed()
            If source Is Nothing Then Throw New ArgumentNullException(NameOf(source))
            If source.Count <> _count Then
                Throw New ArgumentException($"页锁定缓冲区长度 {source.Count} 与显存缓冲区长度 {_count} 不一致")
            End If

            CudaDriverApi.Check(
                CudaDriverApi.cuMemcpyHtoDAsync_v2(_pointer, source.Pointer, _byteSize, StreamHandle(stream)),
                "cuMemcpyHtoDAsync_v2")
        End Sub

        ''' <summary>
        ''' 异步回读（D -> H）。调用方需先对流做同步，再从 target 里取数据。
        ''' </summary>
        Public Sub ReadAsync(target As PinnedHostBuffer(Of T), Optional stream As CudaStream = Nothing)
            ThrowIfDisposed()
            If target Is Nothing Then Throw New ArgumentNullException(NameOf(target))
            If target.Count <> _count Then
                Throw New ArgumentException($"页锁定缓冲区长度 {target.Count} 与显存缓冲区长度 {_count} 不一致")
            End If

            CudaDriverApi.Check(
                CudaDriverApi.cuMemcpyDtoHAsync_v2(target.Pointer, _pointer, _byteSize, StreamHandle(stream)),
                "cuMemcpyDtoHAsync_v2")
        End Sub

        ' ------------------------------------------------------------------
        ' 设备到设备
        ' ------------------------------------------------------------------

        ''' <summary>同步的显存到显存拷贝（要求两段显存字节数一致）</summary>
        Public Sub CopyTo(other As DeviceBuffer(Of T))
            CopyToCore(other, IntPtr.Zero, "cuMemcpyDtoD_v2", useAsync:=False)
        End Sub

        ''' <summary>异步的显存到显存拷贝</summary>
        Public Sub CopyToAsync(other As DeviceBuffer(Of T), Optional stream As CudaStream = Nothing)
            CopyToCore(other, StreamHandle(stream), "cuMemcpyDtoDAsync_v2", useAsync:=True)
        End Sub

        Private Sub CopyToCore(other As DeviceBuffer(Of T), streamHandle As IntPtr, apiName As String, useAsync As Boolean)
            ThrowIfDisposed()

            If other Is Nothing Then Throw New ArgumentNullException(NameOf(other))
            If other.Count <> _count Then
                Throw New ArgumentException($"目标缓冲区长度 {other.Count} 与源长度 {_count} 不一致")
            End If

            If useAsync Then
                CudaDriverApi.Check(
                    CudaDriverApi.cuMemcpyDtoDAsync_v2(other.Pointer, _pointer, _byteSize, streamHandle), apiName)
            Else
                CudaDriverApi.Check(
                    CudaDriverApi.cuMemcpyDtoD_v2(other.Pointer, _pointer, _byteSize), apiName)
            End If
        End Sub

        ''' <summary>用同一个标量填满整段显存</summary>
        Public Sub Fill(value As T)
            ThrowIfDisposed()

            Dim data(_count - 1) As T

            For i As Integer = 0 To _count - 1
                data(i) = value
            Next

            Write(data)
        End Sub

        Private Shared Function StreamHandle(stream As CudaStream) As IntPtr
            Return If(stream Is Nothing, IntPtr.Zero, stream.Handle)
        End Function

        Public Overrides Function ToString() As String
            Return $"{GetType(T).Name}[{_count}] @0x{_pointer:X} ({_byteSize} bytes)"
        End Function
    End Class
End Namespace
