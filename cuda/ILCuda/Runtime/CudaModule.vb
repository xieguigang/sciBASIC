' ------------------------------------------------------------------------
' 模块（PTX / cubin 镜像）加载与内核启动
' ------------------------------------------------------------------------

Imports System.Runtime.InteropServices

Namespace Runtime

    ''' <summary>
    ''' 一个已经加载到当前上下文中的 CUDA 模块
    ''' </summary>
    Public NotInheritable Class CudaModule
        Implements IDisposable

        Private _handle As IntPtr
        Private ReadOnly _kernels As New Dictionary(Of String, CudaKernel)(StringComparer.Ordinal)

        Private Sub New(handle As IntPtr)
            _handle = handle
        End Sub

        Public ReadOnly Property Handle As IntPtr
            Get
                Return _handle
            End Get
        End Property

        ''' <summary>加载 PTX 文本或 cubin/ELF 镜像</summary>
        ''' <param name="skipCompatibilityCheck">
        ''' 跳过"镜像工具包版本 vs 驱动版本"预检。预检是为了避免驱动崩溃，
        ''' 只有在明确知道自己在做什么时才用 --force-image 关掉它。
        ''' </param>
        Public Shared Function Load(image As Byte(), Optional skipCompatibilityCheck As Boolean = False) As CudaModule
            If image Is Nothing OrElse image.Length = 0 Then
                Throw New ArgumentNullException(NameOf(image))
            End If

            ' 版本不匹配的二进制镜像会让驱动直接崩溃，这里先拦一道
            Dim incompatible As String = Nothing
            If Not skipCompatibilityCheck AndAlso
               Not CubinInspector.IsCompatibleWithDriver(image, incompatible) Then
                Throw New InvalidOperationException(incompatible)
            End If

            Dim data = EnsureTerminated(image)
            Dim pin = GCHandle.Alloc(data, GCHandleType.Pinned)
            Try
                Dim handle As IntPtr = IntPtr.Zero
                CudaDriverApi.Check(
                    CudaDriverApi.cuModuleLoadData(handle, pin.AddrOfPinnedObject()), "cuModuleLoadData")
                Return New CudaModule(handle)
            Finally
                pin.Free()
            End Try
        End Function

        ''' <summary>
        ''' 尝试加载镜像；失败时通过 <paramref name="errorText"/> 返回可读的错误信息
        ''' </summary>
        Public Shared Function TryLoad(image As Byte(), ByRef loaded As CudaModule, ByRef errorText As String,
                                        Optional skipCompatibilityCheck As Boolean = False) As Boolean
            loaded = Nothing
            errorText = Nothing
            Try
                loaded = Load(image, skipCompatibilityCheck)
                Return True
            Catch ex As CudaException
                errorText = CudaDriverApi.ErrorText(ex.Status)
                Return False
            Catch ex As Exception
                errorText = ex.Message
                Return False
            End Try
        End Function

        ''' <summary>
        ''' PTX 文本需要以 \0 结尾；ELF cubin 不需要，这里只在镜像不是 ELF
        ''' （首字节不是 0x7F）时补齐一个 0 字节。
        ''' </summary>
        Private Shared Function EnsureTerminated(image As Byte()) As Byte()
            If image.Length = 0 Then Return New Byte() {0}

            If image(image.Length - 1) = 0 Then Return image
            If image(0) = &H7F Then Return image ' ELF 镜像按原样返回

            Dim copy(image.Length) As Byte
            Array.Copy(image, copy, image.Length)
            copy(image.Length) = 0
            Return copy
        End Function

        ''' <summary>取得内核函数句柄（带缓存）</summary>
        Public Function GetKernel(name As String) As CudaKernel
            Dim cached As CudaKernel = Nothing
            If _kernels.TryGetValue(name, cached) Then Return cached

            Dim hfunc As IntPtr = IntPtr.Zero
            CudaDriverApi.Check(
                CudaDriverApi.cuModuleGetFunction(hfunc, _handle, name), $"cuModuleGetFunction({name})")

            Dim kernel As New CudaKernel(name, hfunc)
            _kernels(name) = kernel
            Return kernel
        End Function

        Public Sub Dispose() Implements IDisposable.Dispose
            If _handle = IntPtr.Zero Then Return
            _kernels.Clear()
            Try
                CudaDriverApi.Check(CudaDriverApi.cuModuleUnload(_handle), "cuModuleUnload")
            Catch
            End Try
            _handle = IntPtr.Zero
        End Sub
    End Class

End Namespace
