#Region "Microsoft.VisualBasic::nvml-api, cuda\ILCuda\Interop\NvmlApi.vb"

    ' Author:
    '       xieguigang (xie.guigang@live.com)
    '
    ' Copyright (c) sciBASIC.NET Foundation
    '
    ' GNU GENERAL PUBLIC LICENSE (GPL3)

#End Region

' ------------------------------------------------------------------------
' NVIDIA 管理库（NVML, nvml.dll）的 P/Invoke 声明
'
' 说明：
'   1. 这里只使用 BCL 的 System.Runtime.InteropServices.DllImport，
'      没有任何 NuGet 依赖，与 CudaDriverApi.vb 风格一致；
'   2. nvml.dll 随 NVIDIA 显示驱动发布（与 nvcuda.dll 同源），
'      运行时由系统默认 DLL 搜索路径（System32）解析，无需像
'      NVRTC 那样扫描磁盘路径；
'   3. 构建期不依赖 nvml.dll（P/Invoke 在运行期才解析），
'      缺库时 DllImport 会抛出 DllNotFoundException，由上层捕获降级；
'   4. nvmlDevice_t 是不透明句柄，统一用 IntPtr 承载。
' ------------------------------------------------------------------------

Imports System.Runtime.CompilerServices
Imports System.Runtime.InteropServices
Imports System.Text

Public Module NvmlApi

    ''' <summary>nvml.dll（Windows）。Linux 上为 libnvidia-ml.so.1，本工程仅面向 Windows。</summary>
    Friend Const NvmlLib As String = "nvml"

    ' ------------------------------------------------------------------
    ' 返回码（nvmlReturn_t）
    ' ------------------------------------------------------------------
    Public Enum nvmlReturnT
        NVML_SUCCESS = 0
        NVML_ERROR_UNINITIALIZED = 1
        NVML_ERROR_INVALID_ARGUMENT = 2
        NVML_ERROR_NOT_SUPPORTED = 3
        NVML_ERROR_NO_PERMISSION = 4
        NVML_ERROR_ALREADY_INITIALIZED = 5
        NVML_ERROR_NOT_FOUND = 6
        NVML_ERROR_INSUFFICIENT_SIZE = 7
        NVML_ERROR_INSUFFICIENT_POWER = 8
        NVML_ERROR_DRIVER_NOT_LOADED = 9
        NVML_ERROR_TIMEOUT = 10
        NVML_ERROR_IRQ_ISSUE = 11
        NVML_ERROR_LIBRARY_NOT_FOUND = 12
        NVML_ERROR_FUNCTION_NOT_FOUND = 13
        NVML_ERROR_CORRUPTED_INFOROM = 14
        NVML_ERROR_GPU_IS_LOST = 15
        NVML_ERROR_RESET_REQUIRED = 16
        NVML_ERROR_OPERATING_SYSTEM = 17
        NVML_ERROR_LIB_RM_VERSION_MISMATCH = 18
        NVML_ERROR_IN_USE = 19
        NVML_ERROR_MEMORY = 20
        NVML_ERROR_NO_DATA = 21
        NVML_ERROR_VGPU_ECC_NOT_SUPPORTED = 22
        NVML_ERROR_UNKNOWN = 999
    End Enum

    ' ------------------------------------------------------------------
    ' 枚举：PCIe 吞吐计数器、时钟类型、温度传感器
    ' ------------------------------------------------------------------
    Public Enum nvmlPcieUtilizationCounter_t
        NVML_PCIE_UTIL_TX = 0
        NVML_PCIE_UTIL_RX = 1
        NVML_PCIE_UTIL_AGGREGATE = 2
    End Enum

    Public Enum nvmlClockType_t
        NVML_CLOCK_GRAPHICS = 0
        NVML_CLOCK_SM = 1
        NVML_CLOCK_MEM = 2
        NVML_CLOCK_VIDEO = 3
    End Enum

    Public Enum nvmlTemperatureSensors_t
        NVML_TEMPERATURE_GPU = 0
    End Enum

    ' ------------------------------------------------------------------
    ' 结构体
    ' ------------------------------------------------------------------

    ''' <summary>nvmlUtilization_t：采样周期内 SM 活跃占比与显存控制器带宽占比（%）。</summary>
    <StructLayout(LayoutKind.Sequential)>
    Public Structure nvmlUtilization_t
        Public gpu As UInteger
        Public memory As UInteger
    End Structure

    ''' <summary>
    ''' nvmlMemory_v2_t：字节级显存信息（支持 &gt;4GB）。
    ''' 调用前必须把 version 字段写为 NVML_STRUCT_VERSION(nvmlMemory_v2_t, 2)，
    ''' 即 <c>Marshal.SizeOf(nvmlMemory_v2_t) Or (2UI &lt;&lt; 16)</c>。
    ''' </summary>
    <StructLayout(LayoutKind.Sequential)>
    Public Structure nvmlMemory_v2_t
        Public version As UInteger
        Public reserved As UInteger
        Public used As ULong
        Public free As ULong
        Public total As ULong
    End Structure

    ''' <summary>
    ''' nvmlMemory_t（v1）：仅用于驱动不支持 _v2 时的回退；
    ''' 进程为 x64 时 unsigned long long 也是 64 位，同样支持 &gt;4GB。
    ''' </summary>
    <StructLayout(LayoutKind.Sequential)>
    Public Structure nvmlMemory_t
        Public total As ULong
        Public free As ULong
        Public used As ULong
    End Structure

    ' ------------------------------------------------------------------
    ' 初始化 / 计数
    ' ------------------------------------------------------------------
    <DllImport(NvmlLib, EntryPoint:="nvmlInit_v2")>
    Friend Function nvmlInit_v2() As nvmlReturnT
    End Function

    <DllImport(NvmlLib, EntryPoint:="nvmlShutdown")>
    Friend Function nvmlShutdown() As nvmlReturnT
    End Function

    <DllImport(NvmlLib, EntryPoint:="nvmlDeviceGetCount")>
    Friend Function nvmlDeviceGetCount(ByRef count As Integer) As nvmlReturnT
    End Function

    <DllImport(NvmlLib, EntryPoint:="nvmlDeviceGetHandleByIndex")>
    Friend Function nvmlDeviceGetHandleByIndex(index As Integer, ByRef device As IntPtr) As nvmlReturnT
    End Function

    ' ------------------------------------------------------------------
    ' 设备标识
    ' ------------------------------------------------------------------
    <DllImport(NvmlLib, EntryPoint:="nvmlDeviceGetName", CharSet:=CharSet.Ansi)>
    Friend Function nvmlDeviceGetName(device As IntPtr, name As StringBuilder, length As Integer) As nvmlReturnT
    End Function

    <DllImport(NvmlLib, EntryPoint:="nvmlDeviceGetUUID", CharSet:=CharSet.Ansi)>
    Friend Function nvmlDeviceGetUUID(device As IntPtr, uuid As StringBuilder, length As Integer) As nvmlReturnT
    End Function

    <DllImport(NvmlLib, EntryPoint:="nvmlDeviceGetSerial", CharSet:=CharSet.Ansi)>
    Friend Function nvmlDeviceGetSerial(device As IntPtr, serial As StringBuilder, length As Integer) As nvmlReturnT
    End Function

    <DllImport(NvmlLib, EntryPoint:="nvmlDeviceGetIndex")>
    Friend Function nvmlDeviceGetIndex(device As IntPtr, ByRef index As UInteger) As nvmlReturnT
    End Function

    <DllImport(NvmlLib, EntryPoint:="nvmlDeviceGetCudaComputeCapability")>
    Friend Function nvmlDeviceGetCudaComputeCapability(device As IntPtr, ByRef major As Integer, ByRef minor As Integer) As nvmlReturnT
    End Function

    <DllImport(NvmlLib, EntryPoint:="nvmlDeviceGetPcieLinkGen")>
    Friend Function nvmlDeviceGetPcieLinkGen(device As IntPtr, ByRef linkGen As UInteger) As nvmlReturnT
    End Function

    <DllImport(NvmlLib, EntryPoint:="nvmlDeviceGetPcieLinkWidth")>
    Friend Function nvmlDeviceGetPcieLinkWidth(device As IntPtr, ByRef linkWidth As UInteger) As nvmlReturnT
    End Function

    ' ------------------------------------------------------------------
    ' 实时利用率 / 显存
    ' ------------------------------------------------------------------
    <DllImport(NvmlLib, EntryPoint:="nvmlDeviceGetUtilizationRates")>
    Friend Function nvmlDeviceGetUtilizationRates(device As IntPtr, ByRef utilization As nvmlUtilization_t) As nvmlReturnT
    End Function

    <DllImport(NvmlLib, EntryPoint:="nvmlDeviceGetMemoryInfo")>
    Friend Function nvmlDeviceGetMemoryInfo(device As IntPtr, ByRef memory As nvmlMemory_t) As nvmlReturnT
    End Function

    <DllImport(NvmlLib, EntryPoint:="nvmlDeviceGetMemoryInfo_v2")>
    Friend Function nvmlDeviceGetMemoryInfo_v2(device As IntPtr, ByRef memory As nvmlMemory_v2_t) As nvmlReturnT
    End Function

    <DllImport(NvmlLib, EntryPoint:="nvmlDeviceGetPcieThroughput")>
    Friend Function nvmlDeviceGetPcieThroughput(device As IntPtr, counter As nvmlPcieUtilizationCounter_t, ByRef value As UInteger) As nvmlReturnT
    End Function

    ' ------------------------------------------------------------------
    ' 功耗 / 温度 / 时钟 / 风扇 / 编解码
    ' ------------------------------------------------------------------
    <DllImport(NvmlLib, EntryPoint:="nvmlDeviceGetPowerUsage")>
    Friend Function nvmlDeviceGetPowerUsage(device As IntPtr, ByRef power As UInteger) As nvmlReturnT
    End Function

    <DllImport(NvmlLib, EntryPoint:="nvmlDeviceGetTemperature")>
    Friend Function nvmlDeviceGetTemperature(device As IntPtr, sensorType As nvmlTemperatureSensors_t, ByRef temp As UInteger) As nvmlReturnT
    End Function

    <DllImport(NvmlLib, EntryPoint:="nvmlDeviceGetClockInfo")>
    Friend Function nvmlDeviceGetClockInfo(device As IntPtr, clockType As nvmlClockType_t, ByRef clock As UInteger) As nvmlReturnT
    End Function

    <DllImport(NvmlLib, EntryPoint:="nvmlDeviceGetFanSpeed")>
    Friend Function nvmlDeviceGetFanSpeed(device As IntPtr, ByRef speed As UInteger) As nvmlReturnT
    End Function

    <DllImport(NvmlLib, EntryPoint:="nvmlDeviceGetEncoderUtilization")>
    Friend Function nvmlDeviceGetEncoderUtilization(device As IntPtr, ByRef utilization As UInteger, ByRef samplingPeriodUs As UInteger) As nvmlReturnT
    End Function

    <DllImport(NvmlLib, EntryPoint:="nvmlDeviceGetDecoderUtilization")>
    Friend Function nvmlDeviceGetDecoderUtilization(device As IntPtr, ByRef utilization As UInteger, ByRef samplingPeriodUs As UInteger) As nvmlReturnT
    End Function

    ' ------------------------------------------------------------------
    ' 辅助：错误检查
    ' ------------------------------------------------------------------

    ''' <summary>
    ''' 检查 NVML 调用返回值，非成功时抛出 <see cref="NvmlException"/>
    ''' </summary>
    Public Function Check(status As nvmlReturnT, <CallerMemberName> Optional apiName As String = Nothing) As nvmlReturnT
        If status = nvmlReturnT.NVML_SUCCESS Then
            Return status
        End If
        Throw New NvmlException(status, apiName)
    End Function

    ''' <summary>nvmlMemory_v2_t 的 version 字段值：NVML_STRUCT_VERSION(nvmlMemory_v2_t, 2)。</summary>
    Public Function MemoryV2Version() As UInteger
        Return CUInt(Marshal.SizeOf(GetType(nvmlMemory_v2_t))) Or (2UI << 16)
    End Function
End Module
