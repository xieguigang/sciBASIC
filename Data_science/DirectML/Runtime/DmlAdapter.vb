' ------------------------------------------------------------------------
' DXGI 适配器包装
' ------------------------------------------------------------------------

Imports Microsoft.VisualBasic.Computing.DirectML.Interop

Namespace Runtime

    ''' <summary>
    ''' 一个 DXGI 适配器（显卡）。枚举完成后适配器指针的所有权属于本对象。
    ''' </summary>
    Public NotInheritable Class DmlAdapter
        Inherits ComObjectBase

        Private _supportsD3D12 As Boolean?
        Private _desc As DXGI_ADAPTER_DESC1

        Private Sub New(ptr As IntPtr, index As Integer)
            MyBase.New(ptr)

            Me.Index = index
            Me._desc = DxgiApi.GetAdapterDesc(ptr)
            Me.Description = _desc.Description.TrimEnd(ChrW(0))
        End Sub

        ''' <summary>适配器序号。</summary>
        Public ReadOnly Property Index As Integer

        ''' <summary>适配器名称。</summary>
        Public ReadOnly Property Description As String

        ''' <summary>厂商 ID。</summary>
        Public ReadOnly Property VendorId As UInteger
            Get
                Return _desc.VendorId
            End Get
        End Property

        ''' <summary>设备 ID。</summary>
        Public ReadOnly Property DeviceId As UInteger
            Get
                Return _desc.DeviceId
            End Get
        End Property

        ''' <summary>专用显存（字节）。</summary>
        Public ReadOnly Property DedicatedVideoMemory As ULong
            Get
                Return _desc.DedicatedVideoMemory
            End Get
        End Property

        ''' <summary>共享系统内存（字节）。</summary>
        Public ReadOnly Property SharedSystemMemory As ULong
            Get
                Return _desc.SharedSystemMemory
            End Get
        End Property

        ''' <summary>是否为软件渲染适配器（WARP）。</summary>
        Public ReadOnly Property IsSoftware As Boolean
            Get
                Return (_desc.Flags And CUInt(DXGI_ADAPTER_FLAGS.SOFTWARE)) <> 0
            End Get
        End Property

        ''' <summary>是否为独立显卡（有专用显存且不是软件适配器）。</summary>
        Public ReadOnly Property IsDiscrete As Boolean
            Get
                Return Not IsSoftware AndAlso DedicatedVideoMemory > 0UL
            End Get
        End Property

        ''' <summary>厂商名称。</summary>
        Public ReadOnly Property VendorName As String
            Get
                Select Case VendorId
                    Case &H10DEUI
                        Return "NVIDIA"
                    Case &H1002UI
                        Return "AMD"
                    Case &H8086UI
                        Return "Intel"
                    Case &H1414UI
                        Return "Microsoft"
                    Case Else
                        Return $"0x{VendorId:X4}"
                End Select
            End Get
        End Property

        ''' <summary>
        ''' 查询当前本地显存预算（需要 IDXGIAdapter3）。
        ''' </summary>
        ''' <param name="budget">预算字节数。</param>
        ''' <returns>是否查询成功。</returns>
        Public Function TryGetBudget(ByRef budget As ULong) As Boolean
            Dim info As DXGI_QUERY_VIDEO_MEMORY_INFO

            If DxgiApi.TryQueryVideoMemoryInfo(NativePointer, info) Then
                budget = info.Budget

                Return True
            End If

            budget = DedicatedVideoMemory

            Return False
        End Function

        ''' <summary>判断该适配器能否创建 D3D12 设备。</summary>
        Public ReadOnly Property SupportsD3D12 As Boolean
            Get
                If _supportsD3D12.HasValue Then
                    Return _supportsD3D12.Value
                End If

                Dim supported As Boolean = False

                Try
                    Dim create As D3D12CreateDeviceFn = DmlRuntime.GetCreateDevice()
                    Dim iid As Guid = ComIIDs.ID3D12Device
                    Dim device As IntPtr = IntPtr.Zero
                    Dim hr As Integer = create(NativePointer, D3D_FEATURE_LEVEL.LEVEL_11_0, iid, device)

                    If hr >= 0 AndAlso device <> IntPtr.Zero Then
                        supported = True

                        Dim release As ReleaseFn = ComApi.GetDelegate(Of ReleaseFn)(device, 2)

                        Call release(device)
                    End If
                Catch
                    supported = False
                End Try

                _supportsD3D12 = supported

                Return supported
            End Get
        End Property

        ''' <summary>
        ''' 枚举系统中的显示适配器。
        ''' </summary>
        ''' <param name="includeSoftware">是否包含 WARP 软件适配器与不支持 D3D12 的适配器。</param>
        ''' <returns>适配器列表（调用方负责释放）。</returns>
        Public Shared Function Enumerate(Optional includeSoftware As Boolean = False) As List(Of DmlAdapter)
            Dim list As New List(Of DmlAdapter)
            Dim factory As IntPtr = DmlRuntime.CreateFactory(includeSoftware)

            Try
                Dim pointers As List(Of IntPtr) = DxgiApi.EnumerateAdapterPointers(factory)

                For Each ptr As IntPtr In pointers
                    Dim adapter As New DmlAdapter(ptr, list.Count)

                    If Not includeSoftware Then
                        If adapter.IsSoftware OrElse Not adapter.SupportsD3D12 Then
                            adapter.Dispose()
                            Continue For
                        End If
                    End If

                    Call list.Add(adapter)
                Next

                If list.Count = 0 AndAlso Not includeSoftware Then
                    ' 没有可用硬件适配器时回退到 WARP，至少保证框架可以跑通
                    Dim warp As IntPtr = TryCreateWarpAdapter(factory)

                    If warp <> IntPtr.Zero Then
                        Call list.Add(New DmlAdapter(warp, 0))
                    End If
                End If
            Finally
                Dim release As ReleaseFn = ComApi.GetDelegate(Of ReleaseFn)(factory, 2)

                Call release(factory)
            End Try

            Return list
        End Function

        Private Shared Function TryCreateWarpAdapter(factory As IntPtr) As IntPtr
            Dim factory4 As IntPtr = ComApi.TryQueryInterface(factory, ComIIDs.IDXGIFactory4)

            If factory4 = IntPtr.Zero Then
                Return IntPtr.Zero
            End If

            Try
                Dim enumWarp As EnumWarpAdapterFn = ComApi.GetDelegate(Of EnumWarpAdapterFn)(factory4, DxgiApi.SlotEnumWarpAdapter)
                Dim iid As Guid = ComIIDs.IDXGIAdapter1
                Dim adapter As IntPtr = IntPtr.Zero
                Dim hr As Integer = enumWarp(factory4, iid, adapter)

                If hr >= 0 Then
                    Return adapter
                End If

                Return IntPtr.Zero
            Finally
                Dim release As ReleaseFn = ComApi.GetDelegate(Of ReleaseFn)(factory4, 2)

                Call release(factory4)
            End Try
        End Function

        Protected Overrides Sub Release()
            ReleaseNative()
        End Sub

        Public Overrides Function ToString() As String
            Return $"[{Index}] {Description} ({VendorName}, {DedicatedVideoMemory / 1024.0 / 1024.0:F0} MB)"
        End Function

    End Class

End Namespace
