' ------------------------------------------------------------------------
' DXGI 原生互操作：适配器枚举、显存预算查询
'
' 槽位号来自 Windows SDK dxgi.h / dxgi1_4.h 的 IDXGIxxxVtbl 结构体。
' ------------------------------------------------------------------------

Imports System.Runtime.InteropServices

Namespace Interop

    ''' <summary>DXGI 适配器标志。</summary>
    Public Enum DXGI_ADAPTER_FLAGS As UInteger
        NONE = 0
        REMOTE = 1
        SOFTWARE = 2
    End Enum

    ''' <summary>显存段类型。</summary>
    Public Enum DXGI_MEMORY_SEGMENT_GROUP As Integer
        LOCAL = 0
        NON_LOCAL = 1
    End Enum

    ''' <summary>适配器描述（DXGI_ADAPTER_DESC1）。</summary>
    <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Unicode)>
    Public Structure DXGI_ADAPTER_DESC1
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=128)>
        Public Description As String
        Public VendorId As UInteger
        Public DeviceId As UInteger
        Public SubSysId As UInteger
        Public Revision As UInteger
        Public DedicatedVideoMemory As ULong
        Public DedicatedSystemMemory As ULong
        Public SharedSystemMemory As ULong
        Public AdapterLuidLow As UInteger
        Public AdapterLuidHigh As Integer
        Public Flags As UInteger
    End Structure

    ''' <summary>显存预算信息（DXGI_QUERY_VIDEO_MEMORY_INFO）。</summary>
    <StructLayout(LayoutKind.Sequential)>
    Public Structure DXGI_QUERY_VIDEO_MEMORY_INFO
        Public Budget As ULong
        Public CurrentUsage As ULong
        Public AvailableForReservation As ULong
        Public CurrentReservation As ULong
    End Structure

    ''' <summary>DXGI 常量与 Vtbl 槽位。</summary>
    Public Module DxgiApi

        ''' <summary>CreateDXGIFactory2 的标志位。</summary>
        Public Const DXGI_CREATE_FACTORY_DEBUG As UInteger = 1

        ' ---- Vtbl 槽位（IDXGIFactory1 / IDXGIFactory4 / IDXGIAdapter1 / IDXGIAdapter3） ----

        ''' <summary>IDXGIFactory1::EnumAdapters1</summary>
        Public Const SlotEnumAdapters1 As Integer = 12
        ''' <summary>IDXGIFactory4::EnumWarpAdapter</summary>
        Public Const SlotEnumWarpAdapter As Integer = 27
        ''' <summary>IDXGIAdapter1::GetDesc1</summary>
        Public Const SlotAdapterGetDesc1 As Integer = 10
        ''' <summary>IDXGIAdapter3::QueryVideoMemoryInfo</summary>
        Public Const SlotQueryVideoMemoryInfo As Integer = 14

        ''' <summary>CreateDXGIFactory2</summary>
        <UnmanagedFunctionPointer(CallingConvention.StdCall)>
        Public Delegate Function CreateDXGIFactory2Fn(flags As UInteger, ByRef riid As Guid, ByRef ppv As IntPtr) As Integer

        ''' <summary>IDXGIFactory1::EnumAdapters1</summary>
        <UnmanagedFunctionPointer(CallingConvention.StdCall)>
        Public Delegate Function EnumAdapters1Fn(factory As IntPtr, index As UInteger, ByRef ppAdapter As IntPtr) As Integer

        ''' <summary>IDXGIFactory4::EnumWarpAdapter</summary>
        <UnmanagedFunctionPointer(CallingConvention.StdCall)>
        Public Delegate Function EnumWarpAdapterFn(factory As IntPtr, ByRef riid As Guid, ByRef ppv As IntPtr) As Integer

        ''' <summary>IDXGIAdapter1::GetDesc1（desc 指向 DXGI_ADAPTER_DESC1）</summary>
        <UnmanagedFunctionPointer(CallingConvention.StdCall)>
        Public Delegate Sub AdapterGetDesc1Fn(adapter As IntPtr, desc As IntPtr)

        ''' <summary>IDXGIAdapter3::QueryVideoMemoryInfo</summary>
        <UnmanagedFunctionPointer(CallingConvention.StdCall)>
        Public Delegate Sub QueryVideoMemoryInfoFn(adapter As IntPtr, nodeIndex As UInteger,
                                                   segmentGroup As DXGI_MEMORY_SEGMENT_GROUP,
                                                   ByRef info As DXGI_QUERY_VIDEO_MEMORY_INFO)

        ''' <summary>枚举所有硬件适配器，返回其原生指针（调用方负责 Release）。</summary>
        ''' <param name="factory">IDXGIFactory1 指针。</param>
        ''' <returns>适配器指针列表。</returns>
        Public Function EnumerateAdapterPointers(factory As IntPtr) As List(Of IntPtr)
            Dim result As New List(Of IntPtr)
            Dim enumAdapters As EnumAdapters1Fn = ComApi.GetDelegate(Of EnumAdapters1Fn)(factory, SlotEnumAdapters1)
            Dim index As UInteger = 0

            Do
                Dim adapter As IntPtr = IntPtr.Zero
                Dim hr As Integer = enumAdapters(factory, index, adapter)

                If hr <> ComApi.S_OK OrElse adapter = IntPtr.Zero Then
                    Exit Do
                End If

                Call result.Add(adapter)
                index += 1UI
            Loop

            Return result
        End Function

        ''' <summary>读取适配器描述。</summary>
        Public Function GetAdapterDesc(adapter As IntPtr) As DXGI_ADAPTER_DESC1
            Dim getDesc As AdapterGetDesc1Fn = ComApi.GetDelegate(Of AdapterGetDesc1Fn)(adapter, SlotAdapterGetDesc1)
            Dim size As Integer = Marshal.SizeOf(Of DXGI_ADAPTER_DESC1)()
            Dim buffer As IntPtr = Marshal.AllocHGlobal(size)

            Try
                Call getDesc(adapter, buffer)

                Return Marshal.PtrToStructure(Of DXGI_ADAPTER_DESC1)(buffer)
            Finally
                Call Marshal.FreeHGlobal(buffer)
            End Try
        End Function

        ''' <summary>查询本地显存预算；适配器不支持 IDXGIAdapter3 时返回 Nothing。</summary>
        Public Function TryQueryVideoMemoryInfo(adapter As IntPtr, ByRef info As DXGI_QUERY_VIDEO_MEMORY_INFO) As Boolean
            Dim adapter3 As IntPtr = ComApi.TryQueryInterface(adapter, ComIIDs.IDXGIAdapter3)

            If adapter3 = IntPtr.Zero Then
                Return False
            End If

            Try
                Dim query As QueryVideoMemoryInfoFn = ComApi.GetDelegate(Of QueryVideoMemoryInfoFn)(adapter3, SlotQueryVideoMemoryInfo)
                Dim local As DXGI_QUERY_VIDEO_MEMORY_INFO

                Call query(adapter3, 0UI, DXGI_MEMORY_SEGMENT_GROUP.LOCAL, local)
                info = local

                Return True
            Finally
                Dim release As ReleaseFn = ComApi.GetDelegate(Of ReleaseFn)(adapter3, 2)

                Call release(adapter3)
            End Try
        End Function

    End Module

End Namespace
