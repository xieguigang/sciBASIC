' ------------------------------------------------------------------------
' D3D12 Fence：GPU 与 CPU 之间的同步点
' ------------------------------------------------------------------------

Imports System.Runtime.InteropServices
Imports Microsoft.VisualBasic.Computing.DirectML.Interop

Namespace Runtime

    ''' <summary>D3D12 Fence 包装，支持按值等待与超时。</summary>
    Public NotInheritable Class DmlFence
        Inherits ComObjectBase

        Private Const WAIT_OBJECT_0 As UInteger = 0UI
        Private Const INFINITE As UInteger = &HFFFFFFFFUI

        Private ReadOnly _getValue As D3D12FenceGetCompletedValueFn
        Private ReadOnly _setEvent As D3D12FenceSetEventOnCompletionFn
        Private ReadOnly _signal As D3D12FenceSignalFn

        <DllImport("kernel32.dll", SetLastError:=True, CharSet:=CharSet.Unicode)>
        Private Shared Function CreateEventW(attributes As IntPtr, manualReset As Boolean,
                                             initialState As Boolean, name As String) As IntPtr
        End Function

        <DllImport("kernel32.dll", SetLastError:=True)>
        Private Shared Function WaitForSingleObject(handle As IntPtr, milliseconds As UInteger) As UInteger
        End Function

        <DllImport("kernel32.dll", SetLastError:=True)>
        Private Shared Function CloseHandle(handle As IntPtr) As Boolean
        End Function

        Friend Sub New(ptr As IntPtr)
            MyBase.New(ptr)

            Me._getValue = GetSlotDelegate(Of D3D12FenceGetCompletedValueFn)(8)
            Me._setEvent = GetSlotDelegate(Of D3D12FenceSetEventOnCompletionFn)(9)
            Me._signal = GetSlotDelegate(Of D3D12FenceSignalFn)(10)
        End Sub

        ''' <summary>当前已完成的 Fence 值。</summary>
        Public Function GetCompletedValue() As ULong
            ThrowIfDisposed()

            Return _getValue(NativePointer)
        End Function

        ''' <summary>CPU 侧发出信号。</summary>
        Public Sub Signal(value As ULong)
            ThrowIfDisposed()

            HResult.ThrowIfFailed(_signal(NativePointer, value), "ID3D12Fence::Signal")
        End Sub

        ''' <summary>
        ''' 等待 Fence 到达指定值。
        ''' </summary>
        ''' <param name="value">目标值。</param>
        ''' <param name="timeoutMs">超时毫秒数（-1 表示无限等待）。</param>
        ''' <returns>是否在超时前完成。</returns>
        Public Function WaitForValue(value As ULong, Optional timeoutMs As Integer = 60000) As Boolean
            ThrowIfDisposed()

            If GetCompletedValue() >= value Then
                Return True
            End If

            Dim handle As IntPtr = CreateEventW(IntPtr.Zero, False, False, Nothing)

            If handle = IntPtr.Zero Then
                ' 退化为轮询
                Dim deadline As DateTime = DateTime.UtcNow.AddMilliseconds(If(timeoutMs < 0, 60000, timeoutMs))

                Do
                    If GetCompletedValue() >= value Then
                        Return True
                    End If

                    Threading.Thread.Sleep(1)
                Loop While DateTime.UtcNow < deadline

                Return GetCompletedValue() >= value
            End If

            Try
                HResult.ThrowIfFailed(_setEvent(NativePointer, value, handle), "ID3D12Fence::SetEventOnCompletion")

                Dim wait As UInteger = WaitForSingleObject(handle, If(timeoutMs < 0, INFINITE, CUInt(timeoutMs)))

                Return wait = WAIT_OBJECT_0
            Finally
                Call CloseHandle(handle)
            End Try
        End Function

        Protected Overrides Sub Release()
            ReleaseNative()
        End Sub

    End Class

End Namespace
