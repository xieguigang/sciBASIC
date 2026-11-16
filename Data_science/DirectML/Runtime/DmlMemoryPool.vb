' ------------------------------------------------------------------------
' 显存池：一块大的 UAV 缓冲区 + 256 字节对齐的线性切片分配
'
' DirectML 的绑定（DML_BUFFER_BINDING）由 资源 + 偏移 + 长度 组成，
' 因此多个张量可以共享同一块 ID3D12Resource，只要切片互不重叠。
' 这样整张计算图只需要 1 个大资源 + 1 个 staging + 1 个 readback，
' 提交次数与资源数量都与图的节点数无关。
' ------------------------------------------------------------------------

Imports Microsoft.VisualBasic.Computing.DirectML.Interop

Namespace Runtime

    ''' <summary>显存池里的一段切片。</summary>
    Public Structure DmlAllocation

        ''' <summary>所属缓冲区。</summary>
        Public Buffer As DmlBuffer

        ''' <summary>起始字节偏移（256 字节对齐）。</summary>
        Public Offset As ULong

        ''' <summary>字节长度。</summary>
        Public SizeInBytes As ULong

        ''' <summary>切片是否有效。</summary>
        Public ReadOnly Property IsValid As Boolean
            Get
                Return Buffer IsNot Nothing AndAlso SizeInBytes > 0UL
            End Get
        End Property

        ''' <summary>原生资源指针。</summary>
        Public ReadOnly Property NativeResource As IntPtr
            Get
                If Buffer Is Nothing Then
                    Return IntPtr.Zero
                End If

                Return Buffer.NativePointer
            End Get
        End Property

        Public Overrides Function ToString() As String
            If Buffer Is Nothing Then
                Return "<empty allocation>"
            End If

            Return $"{Buffer.Name}: offset={Offset}, size={SizeInBytes}"
        End Function

    End Structure

    ''' <summary>
    ''' 基于单块 UAV 缓冲区的线性显存池。
    ''' </summary>
    ''' <remarks>
    ''' 采用"一次性布局"策略：编译期把所有张量与临时资源一次性分配完，
    ''' 运行期不再分配/释放，因此没有碎片问题，也不需要同步。
    ''' </remarks>
    Public NotInheritable Class DmlMemoryPool
        Implements IDisposable

        ''' <summary>切片对齐粒度（DirectML 绑定要求 256 字节对齐）。</summary>
        Public Const Alignment As ULong = 256UL

        Private ReadOnly _buffer As DmlBuffer
        Private _cursor As ULong
        Private _peak As ULong

        ''' <summary>
        ''' 创建显存池。
        ''' </summary>
        ''' <param name="device">设备。</param>
        ''' <param name="sizeInBytes">池的字节容量。</param>
        ''' <param name="name">调试名称。</param>
        Public Sub New(device As DmlDevice, sizeInBytes As ULong, Optional name As String = Nothing)
            If device Is Nothing Then
                Throw New ArgumentNullException(NameOf(device))
            End If

            Dim aligned As ULong = Align(sizeInBytes)

            If aligned = 0UL Then
                aligned = Alignment
            End If

            Me._buffer = DmlBuffer.CreateDefault(device, aligned, True, If(name, "dml-arena"))
            Me.Capacity = aligned
        End Sub

        ''' <summary>池容量（字节）。</summary>
        Public ReadOnly Property Capacity As ULong

        ''' <summary>底层缓冲区。</summary>
        Public ReadOnly Property Buffer As DmlBuffer
            Get
                Return _buffer
            End Get
        End Property

        ''' <summary>已分配字节数。</summary>
        Public ReadOnly Property Used As ULong
            Get
                Return _cursor
            End Get
        End Property

        ''' <summary>峰值占用字节数。</summary>
        Public ReadOnly Property Peak As ULong
            Get
                Return _peak
            End Get
        End Property

        ''' <summary>把字节数向上对齐到 256 字节。</summary>
        Public Shared Function Align(value As ULong, Optional alignment As ULong = Alignment) As ULong
            If value = 0UL Then
                Return 0UL
            End If

            Return ((value + alignment - 1UL) \ alignment) * alignment
        End Function

        ''' <summary>
        ''' 从池中切出一段。
        ''' </summary>
        ''' <param name="sizeInBytes">需要的字节数。</param>
        ''' <returns>切片；容量不足时抛出 <see cref="InvalidOperationException"/>。</returns>
        Public Function Allocate(sizeInBytes As ULong) As DmlAllocation
            If _buffer Is Nothing Then
                Throw New ObjectDisposedException(NameOf(DmlMemoryPool))
            End If

            Dim size As ULong = Align(sizeInBytes)

            If size = 0UL Then
                size = Alignment
            End If

            If _cursor + size > Capacity Then
                Throw New InvalidOperationException(
                    $"显存池容量不足：需要 {(_cursor + size) / 1024.0 / 1024.0:F2} MB，当前容量 {Capacity / 1024.0 / 1024.0:F2} MB")
            End If

            Dim alloc As New DmlAllocation With {
                .Buffer = _buffer,
                .Offset = _cursor,
                .SizeInBytes = size
            }

            _cursor += size

            If _cursor > _peak Then
                _peak = _cursor
            End If

            Return alloc
        End Function

        ''' <summary>重置分配游标（仅用于重新布局）。</summary>
        Public Sub Reset()
            _cursor = 0UL
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            If _buffer IsNot Nothing Then
                _buffer.Dispose()
            End If
        End Sub

        Public Overrides Function ToString() As String
            Return $"arena: {Used} / {Capacity} bytes ({Peak} peak)"
        End Function

    End Class

End Namespace
