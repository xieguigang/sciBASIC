' ------------------------------------------------------------------------
' 引擎：把"设备 + 上下文 + 内核镜像 + 模块"组装成一个可用的计算入口
' ------------------------------------------------------------------------

Namespace Runtime

    Public NotInheritable Class CudaEngine
        Implements IDisposable

        Private ReadOnly _device As CudaDevice
        Private ReadOnly _context As CudaContext
        Private ReadOnly _image As KernelImage
        Private ReadOnly _module As CudaModule
        Private _defaultStream As CudaStream
        Private _disposed As Boolean

        Private Sub New(device As CudaDevice, context As CudaContext, image As KernelImage, cudaModule As CudaModule)
            _device = device
            _context = context
            _image = image
            _module = cudaModule
        End Sub

        Public ReadOnly Property Device As CudaDevice
            Get
                Return _device
            End Get
        End Property

        Public ReadOnly Property Context As CudaContext
            Get
                Return _context
            End Get
        End Property

        ''' <summary>最终生效的内核镜像信息（来源、arch、路径）</summary>
        Public ReadOnly Property Image As KernelImage
            Get
                Return _image
            End Get
        End Property

        ''' <summary>
        ''' 尝试创建引擎：枚举设备 -> 建立上下文 -> 获取内核镜像。
        ''' 任何一步失败都会返回 Nothing 并把原因写入 <see cref="EngineOptions.ErrorMessage"/>。
        ''' </summary>
        Public Shared Function TryCreate(options As EngineOptions) As CudaEngine
            Dim context As CudaContext = Nothing

            Try
                Dim devices = CudaDevice.Enumerate()

                If devices.Count = 0 Then
                    options.ErrorMessage = "系统中没有检测到支持 CUDA 的 NVIDIA 设备"
                    Return Nothing
                End If

                Dim ordinal = options.DeviceOrdinal
                If ordinal < 0 OrElse ordinal >= devices.Count Then ordinal = 0

                Dim device = devices(ordinal)
                options.Diagnostics.Add($"[设备] 选择 {device}")

                context = New CudaContext(device)

                Dim image As KernelImage = Nothing
                Dim cudaModule As CudaModule = Nothing

                If Not NvrtcCompiler.TryBuild(device, options.Diagnostics,
                                              options.NvrtcPath, options.ImagePath, image, cudaModule,
                                              options.ForceImage) Then
                    options.ErrorMessage = "没有可用的内核镜像：NVRTC 编译与预编译镜像均失败"
                    context.Dispose()
                    Return Nothing
                End If

                Return New CudaEngine(device, context, image, cudaModule)
            Catch ex As Exception
                If context IsNot Nothing Then context.Dispose()
                options.ErrorMessage = ex.Message
                Return Nothing
            End Try
        End Function

        ''' <summary>
        ''' 引擎自带的一条非阻塞流。需要把拷贝与计算重叠、或让多个内核排队时用它；
        ''' 不传流的 <see cref="CudaKernel.Launch(CudaStream, Integer, Integer, Integer, Integer, Integer, Object())"/>
        ''' 走的是 NULL 流（与整个上下文隐式同步）。
        ''' </summary>
        Public ReadOnly Property DefaultStream As CudaStream
            Get
                If _defaultStream Is Nothing Then
                    _defaultStream = New CudaStream(CudaStreamFlags.NonBlocking)
                End If

                Return _defaultStream
            End Get
        End Property

        ''' <summary>当前上下文的显存使用情况</summary>
        Public Function GetMemoryInfo() As MemoryInfo
            Return CudaMemory.Query()
        End Function

        ''' <summary>取得内核；名字写错时会抛出带 CUDA 错误码的异常</summary>
        Public Function GetKernel(name As String) As CudaKernel
            Return _module.GetKernel(name)
        End Function

        ''' <summary>尝试取得内核；不存在时返回 False（而不是抛异常）</summary>
        Public Function TryGetKernel(name As String, ByRef kernel As CudaKernel) As Boolean
            kernel = Nothing

            Try
                kernel = _module.GetKernel(name)
                Return True
            Catch ex As CudaException
                Return False
            End Try
        End Function

        Public Sub Synchronize()
            _context.Synchronize()
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            If _disposed Then Return
            _disposed = True

            Try
                If _defaultStream IsNot Nothing Then _defaultStream.Dispose()
            Catch
            End Try
            Try
                _module.Dispose()
            Catch
            End Try
            Try
                _context.Dispose()
            Catch
            End Try
        End Sub
    End Class
End Namespace
