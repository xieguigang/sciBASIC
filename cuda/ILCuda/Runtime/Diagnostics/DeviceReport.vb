Namespace Runtime

    ''' <summary>
    ''' 设备属性的纯数据快照：不持有任何 CUDA 句柄，可安全地跨层传递与格式化输出。
    ''' 框架只生成它，怎么显示由调用方决定。
    ''' </summary>
    Public Class DeviceReport
        Public Property Ordinal As Integer
        Public Property Name As String
        ''' <summary>计算能力文本，例如 8.6</summary>
        Public Property ComputeCapability As String
        ''' <summary>NVRTC 虚拟架构，例如 compute_86</summary>
        Public Property Arch As String
        ''' <summary>NVRTC 真实架构（产出 cubin），例如 sm_86</summary>
        Public Property SmArch As String
        Public Property TotalMemoryMB As Double
        Public Property MultiprocessorCount As Integer
        Public Property MaxThreadsPerBlock As Integer
        Public Property MaxThreadsPerMultiprocessor As Integer
        Public Property MaxSharedMemoryPerBlockKB As Double
        Public Property WarpSize As Integer
        Public Property ClockRateMHz As Double
        Public Property MemoryClockRateMHz As Double
        Public Property GlobalMemoryBusWidth As Integer
        Public Property L2CacheSizeKB As Double

        ''' <summary>从 <see cref="CudaDevice"/> 生成快照</summary>
        Public Shared Function From(device As CudaDevice) As DeviceReport
            If device Is Nothing Then Throw New ArgumentNullException(NameOf(device))

            Return New DeviceReport With {
                .Ordinal = device.Ordinal,
                .Name = device.Name,
                .ComputeCapability = device.ComputeCapability,
                .Arch = device.Arch,
                .SmArch = device.SmArch,
                .TotalMemoryMB = device.TotalMemoryMB,
                .MultiprocessorCount = device.MultiprocessorCount,
                .MaxThreadsPerBlock = device.MaxThreadsPerBlock,
                .MaxThreadsPerMultiprocessor = device.MaxThreadsPerMultiprocessor,
                .MaxSharedMemoryPerBlockKB = device.MaxSharedMemoryPerBlock / 1024.0,
                .WarpSize = device.WarpSize,
                .ClockRateMHz = device.ClockRateKhz / 1000.0,
                .MemoryClockRateMHz = device.MemoryClockRateKhz / 1000.0,
                .GlobalMemoryBusWidth = device.GlobalMemoryBusWidth,
                .L2CacheSizeKB = device.L2CacheSize / 1024.0
            }
        End Function

        ''' <summary>把多台设备的快照一次性生成</summary>
        Public Shared Function From(devices As IEnumerable(Of CudaDevice)) As IReadOnlyList(Of DeviceReport)
            If devices Is Nothing Then Return Array.Empty(Of DeviceReport)()

            Return devices.Select(Function(d) From(d)).ToList()
        End Function

        Public Overrides Function ToString() As String
            Return $"[{Ordinal}] {Name} (CC {ComputeCapability}, {TotalMemoryMB:F0} MB)"
        End Function
    End Class
End Namespace
