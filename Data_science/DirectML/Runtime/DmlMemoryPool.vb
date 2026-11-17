' ------------------------------------------------------------------------
' 显存池：按张量切分，每段独占一块 ID3D12Resource
'
' 为什么不是"一块大缓冲 + 偏移切片"：
'   DirectML 的绑定校验以 **ID3D12Resource 指针** 判定是否 in-place 执行。
'   若输入与输出绑定同一块资源但偏移不同，DML 会报
'     "Invalid bindings for in-place execution ... must have exactly the
'      same offsets and sizes"
'   并静默丢弃该 dispatch（关闭调试层时表现为驱动内访问冲突）。
'   因此每个张量必须拥有独立的资源；绑定偏移恒为 0。
'
' 代价是资源对象数量与张量数同阶（显存总量不变），换来的是与 DML 绑定语义一致。
' ------------------------------------------------------------------------

Imports Microsoft.VisualBasic.Computing.DirectML.Interop

Namespace Runtime

    ''' <summary>显存池里的一段切片。</summary>
    Public Structure DmlAllocation

        ''' <summary>所属缓冲区（独占资源）。</summary>
        Public Buffer As DmlBuffer

        ''' <summary>起始字节偏移（独占资源时为 0）。</summary>
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
    ''' 显存池：按需为每个张量创建一块独占的 UAV 缓冲区。
    ''' </summary>
    ''' <remarks>
    ''' 采用"一次性布局"策略：编译期把所有张量与临时资源一次性分配完，
    ''' 运行期不再分配/释放，因此没有碎片问题，也不需要同步。
    ''' </remarks>
    Public NotInheritable Class DmlMemoryPool
        Implements IDisposable

        ''' <summary>切片对齐粒度（DirectML 绑定要求 256 字节对齐）。</summary>
        Public Const Alignment As ULong = 256UL

        Private ReadOnly _device As DmlDevice
        Private ReadOnly _name As String
        Private ReadOnly _buffers As New List(Of DmlBuffer)()
        Private _cursor As ULong
        Private _peak As ULong

        ''' <summary>
        ''' 创建显存池。
        ''' </summary>
        ''' <param name="device">设备。</param>
        ''' <param name="sizeInBytes">池的字节容量（作为总量上限）。</param>
        ''' <param name="name">调试名称。</param>
        Public Sub New(device As DmlDevice, sizeInBytes As ULong, Optional name As String = Nothing)
            If device Is Nothing Then
                Throw New ArgumentNullException(NameOf(device))
            End If

            Me._device = device
            Me._name = If(name, "dml-arena")
            Me.Capacity = Align(System.Math.Max(sizeInBytes, Alignment))
        End Sub

        ''' <summary>池容量（字节）。</summary>
        Public ReadOnly Property Capacity As ULong

        ''' <summary>池内全部资源。</summary>
        Public ReadOnly Property Buffers As IReadOnlyList(Of DmlBuffer)
            Get
                Return _buffers.ToArray()
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
        ''' 切出一段独占的显存。
        ''' </summary>
        ''' <param name="sizeInBytes">需要的字节数。</param>
        ''' <returns>切片；池容量不足时抛出 <see cref="InvalidOperationException"/>。</returns>
        Public Function Allocate(sizeInBytes As ULong) As DmlAllocation
            If _device Is Nothing Then
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

            Dim buffer As DmlBuffer = DmlBuffer.CreateDefault(_device, size, True, $"{_name}#{_buffers.Count}")

            Call _buffers.Add(buffer)

            Dim allocation As New DmlAllocation With {
                .Buffer = buffer,
                .Offset = 0UL,
                .SizeInBytes = size
            }

            _cursor += size

            If _cursor > _peak Then
                _peak = _cursor
            End If

            Return allocation
        End Function

        ''' <summary>重置分配游标（仅用于重新布局，会丢弃已分配的资源）。</summary>
        Public Sub Reset()
            For Each buffer As DmlBuffer In _buffers
                buffer.Dispose()
            Next

            _buffers.Clear()
            _cursor = 0UL
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            For Each buffer As DmlBuffer In _buffers
                buffer.Dispose()
            Next

            _buffers.Clear()
            _cursor = 0UL
        End Sub

        Public Overrides Function ToString() As String
            Return $"arena: {Used} / {Capacity} bytes ({_buffers.Count} resources, {Peak} peak)"
        End Function

    End Class

End Namespace
