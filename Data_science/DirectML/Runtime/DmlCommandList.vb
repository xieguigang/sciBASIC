' ------------------------------------------------------------------------
' D3D12 图形命令列表（DirectML 的 RecordDispatch 也记录到这里）
' ------------------------------------------------------------------------

Imports System.Runtime.InteropServices
Imports Microsoft.VisualBasic.Computing.DirectML.Interop

Namespace Runtime

    ''' <summary>命令列表：记录拷贝、屏障与 DML dispatch。</summary>
    Public NotInheritable Class DmlCommandList
        Inherits ComObjectBase

        Private ReadOnly _close As D3D12CommandListCloseFn
        Private ReadOnly _reset As D3D12CommandListResetFn
        Private ReadOnly _copy As D3D12CopyBufferRegionFn
        Private ReadOnly _barrier As D3D12ResourceBarrierFn
        Private ReadOnly _setDescriptorHeaps As D3D12SetDescriptorHeapsFn

        Friend Sub New(ptr As IntPtr)
            MyBase.New(ptr)

            Me._close = GetSlotDelegate(Of D3D12CommandListCloseFn)(9)
            Me._reset = GetSlotDelegate(Of D3D12CommandListResetFn)(10)
            Me._copy = GetSlotDelegate(Of D3D12CopyBufferRegionFn)(15)
            Me._barrier = GetSlotDelegate(Of D3D12ResourceBarrierFn)(26)
            Me._setDescriptorHeaps = GetSlotDelegate(Of D3D12SetDescriptorHeapsFn)(28)

            ' CreateCommandList 返回的是"正在录制"状态；此时对它所属的分配器调用 Reset
            ' 会得到 E_FAIL。因此创建后立即关闭，后续统一走 Reset → 录制 → Close → 提交。
            Call Close()
        End Sub

        ''' <summary>重置命令列表以重新记录。</summary>
        Public Sub Reset(allocator As DmlCommandAllocator)
            If allocator Is Nothing Then
                Throw New ArgumentNullException(NameOf(allocator))
            End If

            ThrowIfDisposed()

            HResult.ThrowIfFailed(_reset(NativePointer, allocator.NativePointer, IntPtr.Zero), "ID3D12GraphicsCommandList::Reset")
        End Sub

        ''' <summary>关闭命令列表（关闭后才能提交）。</summary>
        Public Sub Close()
            ThrowIfDisposed()

            HResult.ThrowIfFailed(_close(NativePointer), "ID3D12GraphicsCommandList::Close")
        End Sub

        ''' <summary>缓冲区区间拷贝。</summary>
        Public Sub CopyBufferRegion(dst As DmlBuffer, dstOffset As ULong, src As DmlBuffer, srcOffset As ULong, numBytes As ULong)
            If dst Is Nothing OrElse src Is Nothing Then
                Throw New ArgumentNullException(NameOf(dst))
            End If

            If numBytes = 0UL Then
                Return
            End If

            Call _copy(NativePointer, dst.NativePointer, dstOffset, src.NativePointer, srcOffset, numBytes)
        End Sub

        ''' <summary>拷贝切片（自动加上切片偏移）。</summary>
        Public Sub CopyBufferRegion(dst As DmlAllocation, dstOffset As ULong,
                                    src As DmlAllocation, srcOffset As ULong, numBytes As ULong)
            CopyBufferRegion(dst.Buffer, dst.Offset + dstOffset, src.Buffer, src.Offset + srcOffset, numBytes)
        End Sub

        ''' <summary>资源状态转换屏障。</summary>
        Public Sub Transition(resource As DmlBuffer, before As D3D12_RESOURCE_STATES, after As D3D12_RESOURCE_STATES)
            If resource Is Nothing Then
                Throw New ArgumentNullException(NameOf(resource))
            End If

            If before = after Then
                Return
            End If

            Dim resourceBarrier As New D3D12_RESOURCE_BARRIER With {
                .Type = D3D12_RESOURCE_BARRIER_TYPE.TRANSITION,
                .Flags = D3D12_RESOURCE_BARRIER_FLAGS.NONE
            }

            resourceBarrier.Union.Transition = New D3D12_RESOURCE_TRANSITION_BARRIER With {
                .pResource = resource.NativePointer,
                .Subresource = UInteger.MaxValue,
                .StateBefore = before,
                .StateAfter = after
            }

            Call Barrier(resourceBarrier)
        End Sub

        ''' <summary>UAV 屏障：让前一次写入对后续读取可见。</summary>
        Public Sub UavBarrier(resource As DmlBuffer)
            Dim ptr As IntPtr = IntPtr.Zero

            If resource IsNot Nothing Then
                ptr = resource.NativePointer
            End If

            Call Barrier(D3D12_RESOURCE_BARRIER.UnorderedAccess(ptr))
        End Sub

        ''' <summary>
        ''' 设置当前命令列表使用的描述符堆（DML 的 dispatch 需要通过它解析描述符表）。
        ''' </summary>
        Public Sub SetDescriptorHeaps(heaps As DmlDescriptorHeap())
            If heaps Is Nothing OrElse heaps.Length = 0 Then
                Return
            End If

            ThrowIfDisposed()

            Dim pointers As IntPtr() = New IntPtr(heaps.Length - 1) {}

            For i As Integer = 0 To heaps.Length - 1
                pointers(i) = heaps(i).NativePointer
            Next

            Dim size As Integer = IntPtr.Size * pointers.Length
            Dim buffer As IntPtr = Marshal.AllocHGlobal(size)

            Try
                Call Marshal.Copy(pointers, 0, buffer, pointers.Length)
                Call _setDescriptorHeaps(NativePointer, CUInt(pointers.Length), buffer)
            Finally
                Call Marshal.FreeHGlobal(buffer)
            End Try
        End Sub

        ''' <summary>插入单个屏障。</summary>
        Public Sub Barrier(resourceBarrier As D3D12_RESOURCE_BARRIER)
            ThrowIfDisposed()

            Dim size As Integer = Marshal.SizeOf(Of D3D12_RESOURCE_BARRIER)()
            Dim ptr As IntPtr = Marshal.AllocHGlobal(size)

            Try
                Call Marshal.StructureToPtr(resourceBarrier, ptr, False)
                Call _barrier(NativePointer, 1UI, ptr)
            Finally
                Call Marshal.FreeHGlobal(ptr)
            End Try
        End Sub

        Protected Overrides Sub Release()
            ReleaseNative()
        End Sub

    End Class

End Namespace
