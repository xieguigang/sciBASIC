' ------------------------------------------------------------------------
' 描述符堆：DML 绑定表需要 CPU 与 GPU 两种句柄
' ------------------------------------------------------------------------

Imports Microsoft.VisualBasic.Computing.DirectML.Interop

Namespace Runtime

    ''' <summary>shader-visible 的 CBV/SRV/UAV 描述符堆。</summary>
    Public NotInheritable Class DmlDescriptorHeap
        Inherits ComObjectBase

        Private ReadOnly _getCpuHandle As D3D12HeapGetCPUDescriptorHandleForHeapStartFn
        Private ReadOnly _getGpuHandle As D3D12HeapGetGPUDescriptorHandleForHeapStartFn

        Friend Sub New(ptr As IntPtr, descriptorCount As UInteger, incrementSize As UInteger)
            MyBase.New(ptr)

            Me.DescriptorCount = descriptorCount
            Me.IncrementSize = incrementSize
            ' ID3D12DescriptorHeap: 8 GetDesc, 9 GetCPUDescriptorHandleForHeapStart,
            ' 10 GetGPUDescriptorHandleForHeapStart（在 RTX A4000 上实测校验）
            Me._getCpuHandle = GetSlotDelegate(Of D3D12HeapGetCPUDescriptorHandleForHeapStartFn)(9)
            Me._getGpuHandle = GetSlotDelegate(Of D3D12HeapGetGPUDescriptorHandleForHeapStartFn)(10)
        End Sub

        ''' <summary>描述符个数。</summary>
        Public ReadOnly Property DescriptorCount As UInteger

        ''' <summary>每个描述符的字节跨度。</summary>
        Public ReadOnly Property IncrementSize As UInteger

        ''' <summary>取第 index 个描述符的 CPU 句柄。</summary>
        Public Function GetCpuHandle(index As UInteger) As D3D12_CPU_DESCRIPTOR_HANDLE
            ThrowIfDisposed()

            Dim start As D3D12_CPU_DESCRIPTOR_HANDLE

            Call _getCpuHandle(NativePointer, start)

            start.ptr = New IntPtr(start.ptr.ToInt64() + CLng(index) * IncrementSize)

            Return start
        End Function

        ''' <summary>取第 index 个描述符的 GPU 句柄。</summary>
        Public Function GetGpuHandle(index As UInteger) As D3D12_GPU_DESCRIPTOR_HANDLE
            ThrowIfDisposed()

            Dim start As D3D12_GPU_DESCRIPTOR_HANDLE

            Call _getGpuHandle(NativePointer, start)

            start.ptr += CULng(index) * CULng(IncrementSize)

            Return start
        End Function

        Protected Overrides Sub Release()
            ReleaseNative()
        End Sub

    End Class

End Namespace
