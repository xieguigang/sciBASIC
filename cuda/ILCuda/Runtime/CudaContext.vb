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

        ''' <summary>本上下文是否已经释放？</summary>
        Public ReadOnly Property IsDisposed As Boolean
            Get
                Return _disposed
            End Get
        End Property

        ''' <summary>
        ''' 把本上下文绑定到<b>当前线程</b>。
        ''' </summary>
        ''' <remarks>
        ''' CUDA 的"当前上下文"是<b>线程局部</b>状态：构造函数里 <c>cuCtxSetCurrent</c> 只对创建它的
        ''' 那条线程生效，其它线程调用任何需要上下文的驱动接口（分配显存、拷贝、启动内核、
        ''' 加载模块……）都会直接报 <c>CUDA_ERROR_INVALID_CONTEXT (201)</c>。
        ''' 
        ''' 因此"换个线程再跑一次"就会炸 —— 例如交互式仿真里每次点击都提交到线程池。
        ''' 常规做法是每个线程在做 CUDA 工作之前先绑定一次上下文，
        ''' 具体见 <see cref="CudaRuntime.EnsureCurrent"/>（它会自动判断是否需要绑定）。
        ''' </remarks>
        Public Sub MakeCurrent()
            If _disposed OrElse _handle = IntPtr.Zero Then
                ' 注意写 Me.GetType()：裸写 GetType() 会被解析成 VB 的 GetType 运算符（要求一个类型名）
                Throw New ObjectDisposedException(Me.GetType().Name)
            End If

            CudaDriverApi.Check(CudaDriverApi.cuCtxSetCurrent(_handle), "cuCtxSetCurrent")
        End Sub

        ''' <summary>当前线程是否已经绑定了本上下文？</summary>
        Public Function IsCurrent() As Boolean
            If _handle = IntPtr.Zero Then Return False

            Dim current As IntPtr = IntPtr.Zero

            If CudaDriverApi.cuCtxGetCurrent(current) <> CUresult.CUDA_SUCCESS Then Return False

            Return current = _handle
        End Function

        ''' <summary>等待当前上下文中所有已提交的任务执行结束</summary>
        Public Sub Synchronize()
            CudaDriverApi.Check(CudaDriverApi.cuCtxSynchronize(), "cuCtxSynchronize")
        End Sub

        Private Sub Dispose(disposing As Boolean)
            If _disposed Then Return
            _disposed = True

            ' 先从进程级登记表里摘掉：否则其它线程会继续尝试绑定一个正在被销毁的上下文
            CudaRuntime.Unregister(Me)

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
