#Region "Microsoft.VisualBasic::nvml-runtime, cuda\ILCuda\Runtime\Nvml.vb"

    ' Author:
    '       xieguigang (xie.guigang@live.com)
    '
    ' Copyright (c) sciBASIC.NET Foundation
    '
    ' GNU GENERAL PUBLIC LICENSE (GPL3)

#End Region

' ------------------------------------------------------------------------
' NVML 托管封装（风格对齐 CudaDevice.vb）
'
' 职责：把 NvmlApi 的原始 P/Invoke 收敛为"只产出纯数据"的托管对象，
' 排版与输出完全交给上层（GTop）。本文件不依赖 Console，也不触碰
' 任何已有的 CUDA 计算代码（CudaDevice / CudaMemory / CudaEngine ...）。
'
' 关键设计：
'   * 每个指标 getter 内部吞掉 NVML_ERROR_NOT_SUPPORTED 等异常，
'     用哨兵值 -1（百分比 / MHz / ℃ / 瓦）或 Available=False（显存）
'     表示"该设备 / 驱动不支持此查询"，保证单点失败不中断监控循环；
'   * 显存优先用 _v2（字节级、>4GB 安全），失败回退到 v1；
'   * Nvml.IsAvailable() 用于在无 GPU / 缺 nvml.dll 时给出友好提示。
' ------------------------------------------------------------------------

Imports System.Collections.Generic
Imports System.Text

Namespace Runtime

    ''' <summary>
    ''' SM 活跃占比与显存控制器带宽占比（采样周期内，单位 %）。
    ''' 哨兵 -1 表示不可用。
    ''' </summary>
    Public Structure NvmlUtilization
        Public GpuPercent As Integer
        Public MemoryControllerPercent As Integer
    End Structure

    ''' <summary>
    ''' 显存使用情况（字节级）。<see cref="Available"/> 为 False 表示查询失败。
    ''' </summary>
    Public Structure NvmlMemory
        Public UsedBytes As ULong
        Public TotalBytes As ULong
        Public Available As Boolean

        Public ReadOnly Property UsedMB As Double
            Get
                Return UsedBytes / 1024.0 / 1024.0
            End Get
        End Property

        Public ReadOnly Property TotalMB As Double
            Get
                Return TotalBytes / 1024.0 / 1024.0
            End Get
        End Property

        ''' <summary>已用比例（0~1）；总量未知时为 0</summary>
        Public ReadOnly Property UsedRatio As Double
            Get
                If TotalBytes = 0UL OrElse Not Available Then Return 0
                Return UsedBytes / CDbl(TotalBytes)
            End Get
        End Property
    End Structure

    ''' <summary>
    ''' PCIe 吞吐（MB/s）。TX = 设备→主机，RX = 主机→设备。
    ''' 哨兵 -1 表示不可用。
    ''' </summary>
    Public Structure NvmlPcie
        Public TxMBps As Double
        Public RxMBps As Double
    End Structure

    ''' <summary>
    ''' 一次采样快照：时间戳 + 全部指标。供 GTop 的 MonitorSampler 累积。
    ''' </summary>
    Public Structure NvmlSample
        Public Time As DateTime
        Public GpuPercent As Integer                ' SM 活跃占比 %
        Public MemoryControllerPercent As Integer   ' 显存控制器带宽占比 %
        Public UsedBytes As ULong
        Public TotalBytes As ULong
        Public MemoryAvailable As Boolean
        Public PcieTxMBps As Double                 ' 设备→主机
        Public PcieRxMBps As Double                 ' 主机→设备
        Public PowerWatts As Double                 ' 哨兵 -1
        Public TemperatureC As Integer              ' 哨兵 -1
        Public GraphicsClockMHz As Integer          ' 哨兵 -1
        Public SmClockMHz As Integer                ' 哨兵 -1
        Public MemClockMHz As Integer               ' 哨兵 -1
        Public FanSpeedPercent As Integer           ' 哨兵 -1
        Public EncoderPercent As Integer            ' 哨兵 -1
        Public DecoderPercent As Integer            ' 哨兵 -1

        Public ReadOnly Property UsedMB As Double
            Get
                Return UsedBytes / 1024.0 / 1024.0
            End Get
        End Property

        Public ReadOnly Property TotalMB As Double
            Get
                Return TotalBytes / 1024.0 / 1024.0
            End Get
        End Property

        Public ReadOnly Property MemoryUsedRatio As Double
            Get
                If TotalBytes = 0UL OrElse Not MemoryAvailable Then Return 0
                Return UsedBytes / CDbl(TotalBytes)
            End Get
        End Property
    End Structure

    ''' <summary>
    ''' NVML 生命周期与设备枚举（风格对齐 CudaDevice 的 EnsureInitialized / Enumerate）
    ''' </summary>
    Public Module Nvml

        Private _initialized As Boolean = False

        ''' <summary>
        ''' 探测 NVML 是否可用：能成功 nvmlInit_v2 即视为可用（随后立即 Shutdown，
        ''' 不长期占用）。缺 nvml.dll 或驱动未加载时返回 False，绝不抛异常。
        ''' </summary>
        Public Function IsAvailable() As Boolean
            Try
                Dim r = nvmlInit_v2()
                If r = nvmlReturnT.NVML_SUCCESS Then
                    Try : nvmlShutdown() : Catch : End Try
                    Return True
                End If
                Return False
            Catch ex As Exception
                Return False
            End Try
        End Function

        ''' <summary>初始化 NVML（幂等：已初始化不算错误）。</summary>
        Public Sub Init()
            Dim r = nvmlInit_v2()
            If r = nvmlReturnT.NVML_SUCCESS OrElse r = nvmlReturnT.NVML_ERROR_ALREADY_INITIALIZED Then
                _initialized = True
            Else
                Throw New NvmlException(r, "nvmlInit_v2")
            End If
        End Sub

        ''' <summary>关闭 NVML；重复调用或尚未初始化都安全。</summary>
        Public Sub Shutdown()
            If _initialized Then
                Try : nvmlShutdown() : Catch : End Try
                _initialized = False
            End If
        End Sub

        ''' <summary>系统中 NVIDIA GPU 的数量</summary>
        Public Function DeviceCount() As Integer
            Dim count As Integer = 0
            NvmlApi.Check(nvmlDeviceGetCount(count), "nvmlDeviceGetCount")
            Return count
        End Function

        ''' <summary>按序号取得设备句柄封装</summary>
        Public Function GetDevice(index As Integer) As NvmlDevice
            Dim handle As IntPtr = IntPtr.Zero
            NvmlApi.Check(nvmlDeviceGetHandleByIndex(index, handle), "nvmlDeviceGetHandleByIndex")
            Return New NvmlDevice(index, handle)
        End Function

        ''' <summary>枚举全部设备</summary>
        Public Function Enumerate() As IReadOnlyList(Of NvmlDevice)
            Dim n = DeviceCount()
            Dim list As New List(Of NvmlDevice)()
            For i As Integer = 0 To n - 1
                list.Add(GetDevice(i))
            Next
            Return list
        End Function
    End Module

    ''' <summary>
    ''' 单个 NVIDIA GPU 的 NVML 托管视图。所有 getter 对"不支持"的字段
    ''' 返回哨兵值而非抛异常，保证监控循环稳健。
    ''' </summary>
    Public NotInheritable Class NvmlDevice

        Private ReadOnly _ordinal As Integer
        Private ReadOnly _handle As IntPtr

        Public Sub New(ordinal As Integer, handle As IntPtr)
            _ordinal = ordinal
            _handle = handle
        End Sub

        Public ReadOnly Property Ordinal As Integer
            Get
                Return _ordinal
            End Get
        End Property

        Public ReadOnly Property Handle As IntPtr
            Get
                Return _handle
            End Get
        End Property

        ''' <summary>
        ''' 安全调用 NVML 入口：捕获一切异常（含 EntryPointNotFoundException，
        ''' 即该驱动版本的 nvml.dll 未导出此函数），统一返回
        ''' NVML_ERROR_FUNCTION_NOT_FOUND，使上层把缺失能力当作"不支持"而非崩溃。
        ''' </summary>
        Private Function CallStatus(action As Func(Of nvmlReturnT)) As nvmlReturnT
            Try
                Return action()
            Catch
                Return nvmlReturnT.NVML_ERROR_FUNCTION_NOT_FOUND
            End Try
        End Function

        ''' <summary>设备名称，例如 "NVIDIA RTX A4000"</summary>
        Public ReadOnly Property Name As String
            Get
                Try
                    Dim sb As New StringBuilder(256)
                    If nvmlDeviceGetName(_handle, sb, sb.Capacity) = nvmlReturnT.NVML_SUCCESS Then
                        Return sb.ToString().TrimEnd(ChrW(0)).Trim()
                    End If
                Catch
                End Try
                Return "Unknown"
            End Get
        End Property

        ''' <summary>设备 UUID</summary>
        Public ReadOnly Property Uuid As String
            Get
                Try
                    Dim sb As New StringBuilder(96)
                    If nvmlDeviceGetUUID(_handle, sb, sb.Capacity) = nvmlReturnT.NVML_SUCCESS Then
                        Return sb.ToString().TrimEnd(ChrW(0)).Trim()
                    End If
                Catch
                End Try
                Return String.Empty
            End Get
        End Property

        ''' <summary>设备序列号</summary>
        Public ReadOnly Property Serial As String
            Get
                Try
                    Dim sb As New StringBuilder(64)
                    If nvmlDeviceGetSerial(_handle, sb, sb.Capacity) = nvmlReturnT.NVML_SUCCESS Then
                        Return sb.ToString().TrimEnd(ChrW(0)).Trim()
                    End If
                Catch
                End Try
                Return String.Empty
            End Get
        End Property

        ''' <summary>计算能力 (major, minor)；不支持时返回 (-1, -1)</summary>
        Public Function GetCudaComputeCapability() As (major As Integer, minor As Integer)
            Dim major As Integer = 0
            Dim minor As Integer = 0
            If CallStatus(Function() nvmlDeviceGetCudaComputeCapability(_handle, major, minor)) = nvmlReturnT.NVML_SUCCESS Then
                Return (major, minor)
            End If
            Return (-1, -1)
        End Function

        ''' <summary>当前 PCIe 链路代际与宽度：(gen, width)；不支持时返回 (-1, -1)</summary>
        Public Function GetPcieLink() As (gen As Integer, width As Integer)
            Dim gen As UInteger = 0
            Dim width As UInteger = 0
            Dim gOk = CallStatus(Function() nvmlDeviceGetPcieLinkGen(_handle, gen))
            Dim wOk = CallStatus(Function() nvmlDeviceGetPcieLinkWidth(_handle, width))
            Return (If(gOk = nvmlReturnT.NVML_SUCCESS, CInt(gen), -1),
                    If(wOk = nvmlReturnT.NVML_SUCCESS, CInt(width), -1))
        End Function

        ''' <summary>SM 活跃占比与显存控制器带宽占比（%）；不支持时 -1</summary>
        Public Function GetUtilization() As NvmlUtilization
            Dim u As New nvmlUtilization_t()
            If CallStatus(Function() nvmlDeviceGetUtilizationRates(_handle, u)) = nvmlReturnT.NVML_SUCCESS Then
                Return New NvmlUtilization With {
                    .GpuPercent = CInt(u.gpu),
                    .MemoryControllerPercent = CInt(u.memory)
                }
            End If
            Return New NvmlUtilization With {.GpuPercent = -1, .MemoryControllerPercent = -1}
        End Function

        ''' <summary>
        ''' 显存使用（字节级）。优先 _v2，失败回退 v1（x64 下 unsigned long long
        ''' 仍为 64 位，可支持 &gt;4GB）。查询失败则 Available=False。
        ''' </summary>
        Public Function GetMemoryInfo() As NvmlMemory
            Dim m As New nvmlMemory_v2_t()
            m.version = MemoryV2Version()
            If CallStatus(Function() nvmlDeviceGetMemoryInfo_v2(_handle, m)) = nvmlReturnT.NVML_SUCCESS Then
                Return New NvmlMemory With {.UsedBytes = m.used, .TotalBytes = m.total, .Available = True}
            End If

            Dim m1 As New nvmlMemory_t()
            If CallStatus(Function() nvmlDeviceGetMemoryInfo(_handle, m1)) = nvmlReturnT.NVML_SUCCESS Then
                Return New NvmlMemory With {.UsedBytes = m1.used, .TotalBytes = m1.total, .Available = True}
            End If

            Return New NvmlMemory With {.Available = False}
        End Function

        ''' <summary>PCIe 吞吐（MB/s）：TX=设备→主机，RX=主机→设备；不支持时 -1</summary>
        Public Function GetPcieThroughput() As NvmlPcie
            Dim tx As UInteger = 0
            Dim rx As UInteger = 0
            Dim tOk = CallStatus(Function() nvmlDeviceGetPcieThroughput(_handle, nvmlPcieUtilizationCounter_t.NVML_PCIE_UTIL_TX, tx))
            Dim rOk = CallStatus(Function() nvmlDeviceGetPcieThroughput(_handle, nvmlPcieUtilizationCounter_t.NVML_PCIE_UTIL_RX, rx))
            Return New NvmlPcie With {
                .TxMBps = If(tOk = nvmlReturnT.NVML_SUCCESS, tx / 1024.0, -1),
                .RxMBps = If(rOk = nvmlReturnT.NVML_SUCCESS, rx / 1024.0, -1)
            }
        End Function

        ''' <summary>功耗（瓦）；不支持时 -1</summary>
        Public Function GetPowerWatts() As Double
            Dim mw As UInteger = 0
            If CallStatus(Function() nvmlDeviceGetPowerUsage(_handle, mw)) = nvmlReturnT.NVML_SUCCESS Then
                Return mw / 1000.0
            End If
            Return -1
        End Function

        ''' <summary>核心温度（℃）；不支持时 -1</summary>
        Public Function GetTemperatureC() As Integer
            Dim t As UInteger = 0
            If CallStatus(Function() nvmlDeviceGetTemperature(_handle, nvmlTemperatureSensors_t.NVML_TEMPERATURE_GPU, t)) = nvmlReturnT.NVML_SUCCESS Then
                Return CInt(t)
            End If
            Return -1
        End Function

        ''' <summary>时钟频率（MHz）；不支持时 -1</summary>
        Public Function GetClock(kind As nvmlClockType_t) As Integer
            Dim c As UInteger = 0
            If CallStatus(Function() nvmlDeviceGetClockInfo(_handle, kind, c)) = nvmlReturnT.NVML_SUCCESS Then
                Return CInt(c)
            End If
            Return -1
        End Function

        ''' <summary>风扇转速（%）；不支持时 -1</summary>
        Public Function GetFanSpeedPercent() As Integer
            Dim s As UInteger = 0
            If CallStatus(Function() nvmlDeviceGetFanSpeed(_handle, s)) = nvmlReturnT.NVML_SUCCESS Then
                Return CInt(s)
            End If
            Return -1
        End Function

        ''' <summary>编码器 / 解码器利用率（%）；不支持时 -1</summary>
        Public Function GetEncoderDecoderPercent() As (encoder As Integer, decoder As Integer)
            Dim enc As UInteger = 0
            Dim encPeriod As UInteger = 0
            Dim dec As UInteger = 0
            Dim decPeriod As UInteger = 0
            Dim eOk = CallStatus(Function() nvmlDeviceGetEncoderUtilization(_handle, enc, encPeriod))
            Dim dOk = CallStatus(Function() nvmlDeviceGetDecoderUtilization(_handle, dec, decPeriod))
            Return (If(eOk = nvmlReturnT.NVML_SUCCESS, CInt(enc), -1),
                    If(dOk = nvmlReturnT.NVML_SUCCESS, CInt(dec), -1))
        End Function

        ''' <summary>一次性采集全部指标，构成一份快照</summary>
        Public Function GetSample() As NvmlSample
            Dim util = GetUtilization()
            Dim mem = GetMemoryInfo()
            Dim pcie = GetPcieThroughput()
            Dim encdec = GetEncoderDecoderPercent()

            Return New NvmlSample With {
                .Time = DateTime.Now,
                .GpuPercent = util.GpuPercent,
                .MemoryControllerPercent = util.MemoryControllerPercent,
                .UsedBytes = mem.UsedBytes,
                .TotalBytes = mem.TotalBytes,
                .MemoryAvailable = mem.Available,
                .PcieTxMBps = pcie.TxMBps,
                .PcieRxMBps = pcie.RxMBps,
                .PowerWatts = GetPowerWatts(),
                .TemperatureC = GetTemperatureC(),
                .GraphicsClockMHz = GetClock(nvmlClockType_t.NVML_CLOCK_GRAPHICS),
                .SmClockMHz = GetClock(nvmlClockType_t.NVML_CLOCK_SM),
                .MemClockMHz = GetClock(nvmlClockType_t.NVML_CLOCK_MEM),
                .FanSpeedPercent = GetFanSpeedPercent(),
                .EncoderPercent = encdec.encoder,
                .DecoderPercent = encdec.decoder
            }
        End Function

        Public Overrides Function ToString() As String
            Return $"[{Ordinal}] {Name}"
        End Function
    End Class
End Namespace
