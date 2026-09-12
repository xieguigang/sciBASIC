#Region "Microsoft.VisualBasic::78281255344a7345622bf8098c043eaa, cuda\ILCuda\Runtime\CudaContext.vb"

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

    '   Total Lines: 73
    '    Code Lines: 53 (72.60%)
    ' Comment Lines: 6 (8.22%)
    '    - Xml Docs: 50.00%
    ' 
    '   Blank Lines: 14 (19.18%)
    '     File Size: 2.48 KB


    '     Class CudaContext
    ' 
    '         Properties: Device, Handle
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: (+2 Overloads) Dispose, Finalize, Synchronize
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' ------------------------------------------------------------------------
' CUDA 上下文（使用 primary context，替代已废弃的 cuCtxCreate）
' ------------------------------------------------------------------------

Namespace Runtime

    Public NotInheritable Class CudaContext
        Implements IDisposable

        Private ReadOnly _device As CudaDevice
        Private _handle As IntPtr
        Private _disposed As Boolean

        Public Sub New(device As CudaDevice)
            If device Is Nothing Then Throw New ArgumentNullException(NameOf(device))
            _device = device

            CudaDevice.EnsureInitialized()

            Dim ctx As IntPtr = IntPtr.Zero
            CudaDriverApi.Check(
                CudaDriverApi.cuDevicePrimaryCtxRetain(ctx, device.Handle), "cuDevicePrimaryCtxRetain")
            CudaDriverApi.Check(CudaDriverApi.cuCtxSetCurrent(ctx), "cuCtxSetCurrent")

            _handle = ctx
        End Sub

        ''' <summary>关联的设备</summary>
        Public ReadOnly Property Device As CudaDevice
            Get
                Return _device
            End Get
        End Property

        ''' <summary>上下文句柄</summary>
        Public ReadOnly Property Handle As IntPtr
            Get
                Return _handle
            End Get
        End Property

        ''' <summary>等待当前上下文中所有已提交的任务执行结束</summary>
        Public Sub Synchronize()
            CudaDriverApi.Check(CudaDriverApi.cuCtxSynchronize(), "cuCtxSynchronize")
        End Sub

        Private Sub Dispose(disposing As Boolean)
            If _disposed Then Return
            _disposed = True

            If _handle <> IntPtr.Zero Then
                Try
                    CudaDriverApi.Check(CudaDriverApi.cuCtxSetCurrent(IntPtr.Zero), "cuCtxSetCurrent")
                Catch
                End Try
                Try
                    CudaDriverApi.Check(CudaDriverApi.cuDevicePrimaryCtxRelease(_device.Handle), "cuDevicePrimaryCtxRelease")
                Catch
                End Try
                _handle = IntPtr.Zero
            End If
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub

        Protected Overrides Sub Finalize()
            Dispose(False)
        End Sub
    End Class
End Namespace
