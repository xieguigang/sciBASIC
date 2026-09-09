#Region "Microsoft.VisualBasic::de9a71085e07df9405cb2c5210f52f99, cuda\ILCuda\Runtime\CudaStream.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 95
    '    Code Lines: 58 (61.05%)
    ' Comment Lines: 17 (17.89%)
    '    - Xml Docs: 64.71%
    ' 
    '   Blank Lines: 20 (21.05%)
    '     File Size: 3.53 KB


    '     Enum CudaStreamFlags
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    '     Class CudaStream
    ' 
    '         Properties: Handle
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: Query, ToString
    ' 
    '         Sub: Dispose, Synchronize, ThrowIfDisposed, WaitEvent
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' ------------------------------------------------------------------------
' CUDA 流
'
' 同一条流上的任务（拷贝 / 内核）按提交顺序执行；不同流之间可以并行，
' 因此可以把"上传数据"与"计算"放到不同流上来重叠，也可以同时跑多个内核。
' ------------------------------------------------------------------------

Namespace Runtime

    ''' <summary>流创建标志</summary>
    Public Enum CudaStreamFlags
        ''' <summary>默认流：与流 0（NULL 流）有隐式同步语义</summary>
        DefaultStream = 0
        ''' <summary>非阻塞流：不与 NULL 流隐式同步</summary>
        NonBlocking = 1
    End Enum

    ''' <summary>一条 CUDA 流</summary>
    Public NotInheritable Class CudaStream
        Implements IDisposable

        Private _handle As IntPtr
        Private _disposed As Boolean

        Public Sub New(Optional flags As CudaStreamFlags = CudaStreamFlags.DefaultStream)
            Dim handle As IntPtr = IntPtr.Zero

            CudaDriverApi.Check(
                CudaDriverApi.cuStreamCreate(handle, CUInt(flags)), "cuStreamCreate")

            _handle = handle
        End Sub

        ''' <summary>流句柄；传给 cuLaunchKernel / cuMemcpyAsync</summary>
        Public ReadOnly Property Handle As IntPtr
            Get
                Return _handle
            End Get
        End Property

        ''' <summary>阻塞等待本流上已提交的全部任务完成</summary>
        Public Sub Synchronize()
            ThrowIfDisposed()
            CudaDriverApi.Check(CudaDriverApi.cuStreamSynchronize(_handle), "cuStreamSynchronize")
        End Sub

        ''' <summary>
        ''' 查询本流上已提交的任务是否全部完成。
        ''' 返回 True 表示已完成；False 表示仍在执行；其它错误照常抛出。
        ''' </summary>
        Public Function Query() As Boolean
            ThrowIfDisposed()

            Dim status = CudaDriverApi.cuStreamQuery(_handle)

            If status = CUresult.CUDA_SUCCESS Then Return True
            If status = CUresult.CUDA_ERROR_NOT_READY Then Return False

            CudaDriverApi.Check(status, "cuStreamQuery")
            Return False
        End Function

        ''' <summary>让本流等待某个事件，实现流之间的依赖关系</summary>
        Public Sub WaitEvent(evt As CudaEvent)
            ThrowIfDisposed()
            If evt Is Nothing Then Throw New ArgumentNullException(NameOf(evt))

            CudaDriverApi.Check(
                CudaDriverApi.cuStreamWaitEvent(_handle, evt.Handle, 0UI), "cuStreamWaitEvent")
        End Sub

        Private Sub ThrowIfDisposed()
            If _disposed Then Throw New ObjectDisposedException(NameOf(CudaStream))
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            If _disposed Then Return
            _disposed = True

            If _handle <> IntPtr.Zero Then
                Try
                    CudaDriverApi.Check(CudaDriverApi.cuStreamDestroy_v2(_handle), "cuStreamDestroy_v2")
                Catch
                End Try
                _handle = IntPtr.Zero
            End If

            GC.SuppressFinalize(Me)
        End Sub

        Public Overrides Function ToString() As String
            Return If(_disposed, "CudaStream (disposed)", $"CudaStream 0x{_handle:X}")
        End Function
    End Class
End Namespace

