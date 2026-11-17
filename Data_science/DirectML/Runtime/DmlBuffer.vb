' ------------------------------------------------------------------------
' 显存缓冲区：ID3D12Resource 的托管包装
' ------------------------------------------------------------------------

Imports System.Runtime.InteropServices
Imports Microsoft.VisualBasic.Computing.DirectML.Interop

Namespace Runtime

    ''' <summary>
    ''' 一块 ID3D12 缓冲区资源（默认堆 / 上传堆 / 回读堆）。
    ''' </summary>
    Public NotInheritable Class DmlBuffer
        Inherits ComObjectBase

        Private ReadOnly _map As D3D12ResourceMapFn
        Private ReadOnly _unmap As D3D12ResourceUnmapFn

        Private Sub New(ptr As IntPtr, sizeInBytes As ULong, name As String)
            MyBase.New(ptr)

            Me.SizeInBytes = sizeInBytes
            Me.Name = name
            Me._map = GetSlotDelegate(Of D3D12ResourceMapFn)(8)
            Me._unmap = GetSlotDelegate(Of D3D12ResourceUnmapFn)(9)
        End Sub

        ''' <summary>字节容量。</summary>
        Public ReadOnly Property SizeInBytes As ULong

        ''' <summary>调试名称。</summary>
        Public ReadOnly Property Name As String

        ''' <summary>
        ''' 创建一块缓冲区资源。
        ''' </summary>
        ''' <param name="device">设备。</param>
        ''' <param name="sizeInBytes">字节容量。</param>
        ''' <param name="heapType">堆类型。</param>
        ''' <param name="unorderedAccess">是否需要 UAV 标志（DirectML 绑定要求）。</param>
        ''' <param name="name">调试名称。</param>
        Public Shared Function Create(device As DmlDevice,
                                      sizeInBytes As ULong,
                                      heapType As D3D12_HEAP_TYPE,
                                      unorderedAccess As Boolean,
                                      Optional name As String = Nothing) As DmlBuffer

            If device Is Nothing Then
                Throw New ArgumentNullException(NameOf(device))
            End If

            If sizeInBytes = 0UL Then
                sizeInBytes = 256UL
            End If

            Dim heap As D3D12_HEAP_PROPERTIES

            Select Case heapType
                Case D3D12_HEAP_TYPE.UPLOAD_HEAP
                    heap = D3D12_HEAP_PROPERTIES.CreateUpload()
                Case D3D12_HEAP_TYPE.READBACK_HEAP
                    heap = D3D12_HEAP_PROPERTIES.CreateReadback()
                Case Else
                    heap = D3D12_HEAP_PROPERTIES.CreateDefault()
            End Select

            Dim resourceFlags As D3D12_RESOURCE_FLAGS = D3D12_RESOURCE_FLAGS.NONE

            If unorderedAccess Then
                resourceFlags = D3D12_RESOURCE_FLAGS.ALLOW_UNORDERED_ACCESS
            End If

            ' 注意：缓冲区必须声明 Layout = ROW_MAJOR。D3D12 的校验明确要求
            ' "Layout must be D3D12_TEXTURE_LAYOUT_ROW_MAJOR when Dimension is BUFFER"，
            ' 写成 UNKNOWN 会被 CreateCommittedResource 以 E_INVALIDARG 拒绝（纹理则接受 UNKNOWN）。
            Dim desc As New D3D12_RESOURCE_DESC With {
                .Dimension = D3D12_RESOURCE_DIMENSION.BUFFER,
                .Alignment = 0UL,
                .Width = sizeInBytes,
                .Height = 1UI,
                .DepthOrArraySize = 1US,
                .MipLevels = 1US,
                .Format = 0,
                .SampleDesc = New DXGI_SAMPLE_DESC With {.Count = 1UI, .Quality = 0UI},
                .Layout = D3D12_TEXTURE_LAYOUT.ROW_MAJOR,
                .Flags = resourceFlags
            }

            Dim initialState As D3D12_RESOURCE_STATES

            Select Case heapType
                Case D3D12_HEAP_TYPE.UPLOAD_HEAP
                    initialState = D3D12_RESOURCE_STATES.GENERIC_READ
                Case D3D12_HEAP_TYPE.READBACK_HEAP
                    initialState = D3D12_RESOURCE_STATES.COPY_DEST
                Case Else
                    initialState = D3D12_RESOURCE_STATES.COMMON
            End Select

            Dim iid As Guid = ComIIDs.ID3D12Resource
            Dim ptr As IntPtr = IntPtr.Zero

            Dim hr As Integer = device.CreateCommittedResource(heap, D3D12_HEAP_FLAGS.NONE, desc, initialState,
                                                               IntPtr.Zero, iid, ptr)

            If HResult.Failed(hr) Then
                ' 失败时把描述符关键字段与调试层原文一起抛出，避免只留一个 E_INVALIDARG。
                Dim validation As String = DmlDebugLayer.Summarize(DmlDebugLayer.Capture("CreateCommittedResource"))
                Dim detail As String = $"heapType={heapType}, size={sizeInBytes}, layout={desc.Layout}, " &
                                       $"flags={desc.Flags}, initialState={initialState}"

                If Not String.IsNullOrEmpty(validation) Then
                    detail &= "；调试层：" & validation
                End If

                Throw New DmlException(hr, "CreateCommittedResource", detail)
            End If

            Return New DmlBuffer(ptr, sizeInBytes, name)
        End Function

        ''' <summary>默认堆 + UAV（DirectML 计算用）。</summary>
        Public Shared Function CreateDefault(device As DmlDevice, sizeInBytes As ULong,
                                             Optional unorderedAccess As Boolean = True,
                                             Optional name As String = Nothing) As DmlBuffer
            Return Create(device, sizeInBytes, D3D12_HEAP_TYPE.DEFAULT_HEAP, unorderedAccess, name)
        End Function

        ''' <summary>上传堆（CPU 写、GPU 读）。</summary>
        Public Shared Function CreateUpload(device As DmlDevice, sizeInBytes As ULong,
                                            Optional name As String = Nothing) As DmlBuffer
            Return Create(device, sizeInBytes, D3D12_HEAP_TYPE.UPLOAD_HEAP, False, name)
        End Function

        ''' <summary>回读堆（GPU 写、CPU 读）。</summary>
        Public Shared Function CreateReadback(device As DmlDevice, sizeInBytes As ULong,
                                              Optional name As String = Nothing) As DmlBuffer
            Return Create(device, sizeInBytes, D3D12_HEAP_TYPE.READBACK_HEAP, False, name)
        End Function

        ''' <summary>映射整块内存（调用方必须成对调用 <see cref="Unmap"/>）。</summary>
        ''' <returns>CPU 可见指针。</returns>
        Public Function Map() As IntPtr
            ThrowIfDisposed()

            Dim data As IntPtr = IntPtr.Zero

            HResult.ThrowIfFailed(_map(NativePointer, 0UI, IntPtr.Zero, data), "ID3D12Resource::Map")

            Return data
        End Function

        ''' <summary>解除映射。</summary>
        Public Sub Unmap()
            If IsDisposed Then
                Return
            End If

            Call _unmap(NativePointer, 0UI, IntPtr.Zero)
        End Sub

        ''' <summary>把托管字节写入上传堆的指定偏移。</summary>
        Public Sub WriteBytes(offset As ULong, data As Byte(), Optional length As Integer = -1)
            If data Is Nothing OrElse data.Length = 0 Then
                Return
            End If

            If length < 0 Then
                length = data.Length
            End If

            If offset + CULng(length) > SizeInBytes Then
                Throw New ArgumentOutOfRangeException(NameOf(length), $"写入越界：offset={offset}, length={length}, capacity={SizeInBytes}")
            End If

            Dim ptr As IntPtr = Map()

            Try
                Call Marshal.Copy(data, 0, New IntPtr(ptr.ToInt64() + CLng(offset)), length)
            Finally
                Unmap()
            End Try
        End Sub

        ''' <summary>把显存中的字节读到托管数组（只对回读堆有效）。</summary>
        Public Sub ReadBytes(offset As ULong, data As Byte(), offsetInArray As Integer, length As Integer)
            If length <= 0 Then
                Return
            End If

            If offset + CULng(length) > SizeInBytes Then
                Throw New ArgumentOutOfRangeException(NameOf(length), $"读取越界：offset={offset}, length={length}, capacity={SizeInBytes}")
            End If

            Dim ptr As IntPtr = Map()

            Try
                Call Marshal.Copy(New IntPtr(ptr.ToInt64() + CLng(offset)), data, offsetInArray, length)
            Finally
                Unmap()
            End Try
        End Sub

        ''' <summary>写入单精度数组。</summary>
        Public Sub WriteSingles(offset As ULong, values As Single())
            If values Is Nothing OrElse values.Length = 0 Then
                Return
            End If

            Dim ptr As IntPtr = Map()

            Try
                Call Marshal.Copy(values, 0, New IntPtr(ptr.ToInt64() + CLng(offset)), values.Length)
            Finally
                Unmap()
            End Try
        End Sub

        ''' <summary>读取单精度数组。</summary>
        Public Function ReadSingles(offset As ULong, count As Integer) As Single()
            Dim values As Single() = New Single(count - 1) {}
            Dim ptr As IntPtr = Map()

            Try
                Call Marshal.Copy(New IntPtr(ptr.ToInt64() + CLng(offset)), values, 0, count)
            Finally
                Unmap()
            End Try

            Return values
        End Function

        Protected Overrides Sub Release()
            ReleaseNative()
        End Sub

        Public Overrides Function ToString() As String
            Return $"{Name} ({SizeInBytes} bytes)"
        End Function

    End Class

End Namespace
