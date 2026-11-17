' ------------------------------------------------------------------------
' 原生库加载入口：DirectML.dll / d3d12.dll / dxgi.dll
'
' 三个库都使用延迟探测：只要没有真正用到 GPU，框架就不会触碰原生库，
' 因此导入本程序集的调用方在没有显卡 / 没有 DirectML 的机器上也能正常启动。
' ------------------------------------------------------------------------

Imports Microsoft.VisualBasic.Computing.DirectML.Interop

Namespace Runtime

    ''' <summary>
    ''' DirectML 运行时：原生库的探测、加载与导出缓存。
    ''' </summary>
    Public NotInheritable Class DmlRuntime

        Private Shared ReadOnly _sync As New Object()
        Private Shared ReadOnly _attempts As New List(Of NativeProbeAttempt)()
        Private Shared _directML As NativeModule
        Private Shared _d3d12 As NativeModule
        Private Shared _dxgi As NativeModule
        Private Shared _probed As Boolean
        Private Shared _lastError As String

        ''' <summary>DirectML.dll 的模块名。</summary>
        Public Const DirectMLModule As String = "DirectML.dll"
        ''' <summary>d3d12.dll 的模块名。</summary>
        Public Const D3D12Module As String = "d3d12.dll"
        ''' <summary>dxgi.dll 的模块名。</summary>
        Public Const DxgiModule As String = "dxgi.dll"

        ''' <summary>全部加载尝试的记录（成功与失败都保留，供诊断层使用）。</summary>
        Public Shared ReadOnly Property Attempts As IReadOnlyList(Of NativeProbeAttempt)
            Get
                SyncLock _sync
                    Return _attempts.ToArray()
                End SyncLock
            End Get
        End Property

        ''' <summary>最近一次探测失败的原因。</summary>
        Public Shared ReadOnly Property LastError As String
            Get
                SyncLock _sync
                    Return _lastError
                End SyncLock
            End Get
        End Property

        ''' <summary>DirectML 模块（未探测时为 Nothing）。</summary>
        Public Shared ReadOnly Property DirectML As NativeModule
            Get
                EnsureProbed()
                Return _directML
            End Get
        End Property

        ''' <summary>D3D12 模块（未探测时为 Nothing）。</summary>
        Public Shared ReadOnly Property D3D12 As NativeModule
            Get
                EnsureProbed()
                Return _d3d12
            End Get
        End Property

        ''' <summary>DXGI 模块（未探测时为 Nothing）。</summary>
        Public Shared ReadOnly Property Dxgi As NativeModule
            Get
                EnsureProbed()
                Return _dxgi
            End Get
        End Property

        ''' <summary>额外搜索目录（例如 Microsoft.AI.DirectML 分发目录）。</summary>
        Public Shared Property ExtraSearchDirectories As String()

        ''' <summary>
        ''' 探测三个原生库；全部可用时返回 True。
        ''' </summary>
        Public Shared Function Probe() As Boolean
            EnsureProbed(force:=True)

            SyncLock _sync
                Return _directML IsNot Nothing AndAlso _d3d12 IsNot Nothing AndAlso _dxgi IsNot Nothing
            End SyncLock
        End Function

        Private Shared Sub EnsureProbed(Optional force As Boolean = False)
            SyncLock _sync
                If _probed AndAlso Not force Then
                    Return
                End If

                _probed = True
                _attempts.Clear()
                _lastError = Nothing

                Dim dirs As String() = ExtraSearchDirectories

                _directML = NativeModule.TryLoad(DirectMLModule, _attempts, dirs)
                _d3d12 = NativeModule.TryLoad(D3D12Module, _attempts, dirs)
                _dxgi = NativeModule.TryLoad(DxgiModule, _attempts, dirs)

                If _directML Is Nothing Then
                    _lastError = "找不到 DirectML.dll；需要 Windows 10 1903 及以上（通过 DirectX 更新下发），" &
                                 "或把 Microsoft.AI.DirectML 分发目录中的 DirectML.dll 放到程序目录"
                ElseIf _d3d12 Is Nothing Then
                    _lastError = "找不到 d3d12.dll；系统版本过低或运行在受限环境"
                ElseIf _dxgi Is Nothing Then
                    _lastError = "找不到 dxgi.dll；系统版本过低或运行在受限环境"
                End If
            End SyncLock
        End Sub

        ''' <summary>
        ''' 创建一个 IDXGIFactory1 指针（调用方负责 Release）。
        ''' </summary>
        ''' <param name="debug">是否启用 DXGI 调试层。</param>
        Public Shared Function CreateFactory(Optional debug As Boolean = False) As IntPtr
            EnsureProbed()

            If _dxgi Is Nothing Then
                Throw New DmlException(-2147467259, "LoadLibrary(dxgi.dll)", _lastError)
            End If

            Dim create As CreateDXGIFactory2Fn = _dxgi.GetDelegate(Of CreateDXGIFactory2Fn)("CreateDXGIFactory2")
            Dim iid As Guid = ComIIDs.IDXGIFactory1
            Dim factory As IntPtr = IntPtr.Zero
            Dim flags As UInteger = If(debug, DxgiApi.DXGI_CREATE_FACTORY_DEBUG, 0UI)

            HResult.ThrowIfFailed(create(flags, iid, factory), "CreateDXGIFactory2")

            Return factory
        End Function

        ''' <summary>取得 D3D12CreateDevice 的导出委托。</summary>
        Public Shared Function GetCreateDevice() As D3D12CreateDeviceFn
            EnsureProbed()

            If _d3d12 Is Nothing Then
                Throw New DmlException(-2147467259, "LoadLibrary(d3d12.dll)", _lastError)
            End If

            Return _d3d12.GetDelegate(Of D3D12CreateDeviceFn)("D3D12CreateDevice")
        End Function

        ''' <summary>
        ''' 创建 DML 设备（优先使用 DMLCreateDevice1 以声明最低特性级别）。
        ''' </summary>
        Public Shared Function CreateDmlDevice(d3d12Device As IntPtr,
                                               minimumFeatureLevel As DML_FEATURE_LEVEL,
                                               flags As DML_CREATE_DEVICE_FLAGS) As IntPtr

            EnsureProbed()

            If _directML Is Nothing Then
                Throw New DmlException(-2147467259, "LoadLibrary(DirectML.dll)", _lastError)
            End If

            Dim iid As Guid = ComIIDs.IDMLDevice
            Dim ptr As IntPtr = IntPtr.Zero
            Dim create1 As DMLCreateDevice1Fn = _directML.GetDelegate(Of DMLCreateDevice1Fn)("DMLCreateDevice1", optional:=True)

            If create1 IsNot Nothing Then
                HResult.ThrowIfFailed(create1(d3d12Device, flags, minimumFeatureLevel, iid, ptr), "DMLCreateDevice1")

                Return ptr
            End If

            Dim create As DMLCreateDeviceFn = _directML.GetDelegate(Of DMLCreateDeviceFn)("DMLCreateDevice")

            HResult.ThrowIfFailed(create(d3d12Device, flags, iid, ptr), "DMLCreateDevice")

            Return ptr
        End Function

        ''' <summary>取得 DirectML.dll 的实际加载路径（未加载时为 Nothing）。</summary>
        Public Shared ReadOnly Property DirectMLPath As String
            Get
                If _directML Is Nothing Then
                    Return Nothing
                End If

                Return _directML.FilePath
            End Get
        End Property

        ''' <summary>重置探测状态（仅供测试使用）。</summary>
        Public Shared Sub Reset()
            SyncLock _sync
                _probed = False
                _attempts.Clear()
                _lastError = Nothing
            End SyncLock
        End Sub

    End Class

End Namespace
