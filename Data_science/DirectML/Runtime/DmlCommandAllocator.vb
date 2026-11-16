' ------------------------------------------------------------------------
' D3D12 命令分配器
' ------------------------------------------------------------------------

Imports Microsoft.VisualBasic.Computing.DirectML.Interop

Namespace Runtime

    ''' <summary>命令分配器：命令列表每帧/每次提交都在它之上重新记录。</summary>
    Public NotInheritable Class DmlCommandAllocator
        Inherits ComObjectBase

        Private ReadOnly _reset As D3D12CommandAllocatorResetFn

        Friend Sub New(ptr As IntPtr)
            MyBase.New(ptr)

            Me._reset = GetSlotDelegate(Of D3D12CommandAllocatorResetFn)(8)
        End Sub

        ''' <summary>重置分配器（必须在关联的命令列表执行完成之后调用）。</summary>
        Public Sub Reset()
            ThrowIfDisposed()

            HResult.ThrowIfFailed(_reset(NativePointer), "ID3D12CommandAllocator::Reset")
        End Sub

        Protected Overrides Sub Release()
            ReleaseNative()
        End Sub

    End Class

End Namespace
