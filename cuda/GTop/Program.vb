#Region "Microsoft.VisualBasic::486ef1a50d07e31e76c696f1b298ceb4, cuda\GTop\Program.vb"

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

    '   Total Lines: 226
    '    Code Lines: 193 (85.40%)
    ' Comment Lines: 7 (3.10%)
    '    - Xml Docs: 57.14%
    ' 
    '   Blank Lines: 26 (11.50%)
    '     File Size: 10.28 KB


    ' Module Program
    ' 
    '     Function: ParseArgs, TryGetHardware
    ' 
    '     Sub: Main, OnCancel, PrintHelp, Run, RunProcessesOnce
    '     Class Options
    ' 
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Linq
Imports Microsoft.VisualBasic.Computing.ILCuda
Imports Microsoft.VisualBasic.Computing.ILCuda.Runtime

Module Program

    Private _cancelled As Boolean = False

    Private Class Options
        Public ListDevices As Boolean = False
        Public Once As Boolean = False
        Public Table As Boolean = False
        Public CsvPath As String = Nothing
        Public DeviceIndex As Integer = 0
        Public IntervalMs As Integer = 1000
        Public MaxRows As Integer = 20
        Public DurationSec As Integer = 0
        Public Plain As Boolean = False
        Public Help As Boolean = False
        Public Processes As Boolean = False
        Public TopExplicit As Boolean = False

        ' ---- CUPTI per-SM 归因（--sm）----
        Public Sm As Boolean = False
        Public SmDoctor As Boolean = False
        Public SmMetrics As String() = Nothing
        Public SmIntervalMs As Double = CuptiPmSamplingSession.DefaultSamplingIntervalMs
        Public SmDurationSec As Integer = 0
        Public SmRows As Integer = 0
        Public SmHwBufferMB As Integer = 0
        Public SmSysClock As Boolean = False
        ''' <summary>实时仪表盘不挂载 CUPTI SM 归因区块</summary>
        Public NoSm As Boolean = False
        Public CuptiPath As String = Nothing
    End Class

    Sub Main(args As String())
        Dim opts = ParseArgs(args)

        If opts.Help Then
            PrintHelp()
            Return
        End If

        ' CUPTI 依赖（--sm 分支）：显式路径必须在载入之前设置
        If Not String.IsNullOrWhiteSpace(opts.CuptiPath) Then
            CuptiLibrary.ExplicitPath = opts.CuptiPath
            CuptiLibrary.ResetCache()
            CuptiPerSmCounter.ResetSupportCache()
        End If

        ' SM 归因模式（CUPTI，独立于 NVML）：--sm / --sm --top / --sm-doctor
        If opts.SmDoctor Then
            Environment.Exit(RunSmDoctor(opts))
            Return
        End If

        If opts.Sm Then
            If opts.TopExplicit Then
                Environment.Exit(RunSmLive(opts))
            Else
                Environment.Exit(RunSmOnce(opts))
            End If
            Return
        End If

        ' 进程模式（PDH，独立于 NVML）：--processes 单独使用时打印一次进程表后退出
        If opts.Processes AndAlso Not opts.TopExplicit Then
            RunProcessesOnce(opts)
            Return
        End If

        If Not Nvml.IsAvailable() Then
            Console.Error.WriteLine("错误：NVML 不可用（找不到 nvml.dll 或 NVIDIA 驱动未加载）。")
            Console.Error.WriteLine("GTop 依赖随 NVIDIA 显示驱动发布的 NVML，无需 CUDA Toolkit；请确认驱动已正确安装。")
            Environment.Exit(1)
            Return
        End If

        Try
            Nvml.Init()
        Catch ex As Exception
            Console.Error.WriteLine($"无法初始化 NVML：{ex.Message}")
            Environment.Exit(1)
            Return
        End Try

        Try
            Run(opts)
        Finally
            Nvml.Shutdown()
        End Try
    End Sub

    Private Sub Run(opts As Options)
        If opts.ListDevices Then
            Dim devices = Nvml.Enumerate()
            Reporter.PrintDeviceList(devices)
            Return
        End If

        Dim device As NvmlDevice
        Try
            device = Nvml.GetDevice(opts.DeviceIndex)
        Catch ex As Exception
            Console.Error.WriteLine($"无法获取设备 #{opts.DeviceIndex}：{ex.Message}")
            Return
        End Try

        If opts.Once Then
            Dim s = device.GetSample()
            Reporter.PrintSnapshot(device, s)
            Return
        End If

        Dim sampler = New MonitorSampler(device, opts.IntervalMs, opts.MaxRows)
        AddHandler Console.CancelKeyPress, AddressOf OnCancel

        Dim start = DateTime.Now
        Dim durationSec = opts.DurationSec
        Dim stopFn As Func(Of Boolean) = Function()
                                            If _cancelled Then Return True
                                            If durationSec > 0 AndAlso (DateTime.Now - start).TotalSeconds >= durationSec Then Return True
                                            Return False
                                        End Function

        If opts.CsvPath IsNot Nothing Then
            sampler.Run(stopFn)
            Reporter.WriteCsv(sampler.Samples, opts.CsvPath)
            Console.WriteLine($"已导出 {sampler.Samples.Count} 条采样到 {opts.CsvPath}")
        ElseIf opts.Table Then
            sampler.Run(stopFn)
            Reporter.PrintHistory(device, sampler.Samples)
        Else
            ' 实时仪表盘（默认模式）：完整信息 = 设备指标 + 硬件参数 + 历史曲线 + SM 归因 + 每进程指标
            Dim hw = TryGetHardware(opts.DeviceIndex)
            Dim smMonitor = CreateSmMonitor(opts)
            Dim smRows = If(opts.SmRows > 0, opts.SmRows, 8)

            Try
                If GpuProcessCounter.IsAvailable() Then
                    Using procCounter As New GpuProcessCounter()
                        Dim total = device.GetMemoryInfo().TotalBytes
                        Dim procMonitor As New ProcessMonitor(procCounter, total, 20, TimeSpan.FromMinutes(1))
                        Reporter.RunLive(sampler, device, Not opts.Plain, stopFn, procMonitor, hw, opts.MaxRows, smMonitor, smRows)
                    End Using
                Else
                    If opts.Processes Then
                        Console.Error.WriteLine("警告：进程级 GPU 计数器不可用，仅显示设备仪表盘。")
                    End If
                    Reporter.RunLive(sampler, device, Not opts.Plain, stopFn, Nothing, hw, 0, smMonitor, smRows)
                End If
            Finally
                If smMonitor IsNot Nothing Then smMonitor.Dispose()
            End Try
        End If

        RemoveHandler Console.CancelKeyPress, AddressOf OnCancel
    End Sub

    ''' <summary>尝试获取指定设备的静态硬件参数（CUDA Driver API 设备属性）；失败返回 Nothing。</summary>
    Private Function TryGetHardware(index As Integer) As CudaDevice
        Try
            Dim devices = CudaDevice.Enumerate()
            If index >= 0 AndAlso index < devices.Count Then Return devices(index)
        Catch
        End Try
        Return Nothing
    End Function

    ''' <summary>
    ''' --processes 一次性模式：采集一次进程级 GPU 计数并打印进程表（不依赖 NVML）。
    ''' </summary>
    Private Sub RunProcessesOnce(opts As Options)
        If Not GpuProcessCounter.IsAvailable() Then
            Console.Error.WriteLine("错误：进程级 GPU 计数器不可用。")
            Console.Error.WriteLine("需要 Windows 10 1709+ / WDDM 2.0+，且系统存在 \GPU Engine 性能计数器集。")
            Environment.Exit(2)
            Return
        End If

        Using counter As New GpuProcessCounter()
            ' 速率型计数器需两次采集，预热后短暂等待以产生有效样本
            Dim waitMs = If(opts.IntervalMs > 0, System.Math.Min(opts.IntervalMs, 1000), 1000)
            Threading.Thread.Sleep(waitMs)

            Dim rows = counter.Poll()
            Reporter.PrintProcesses(rows, opts.MaxRows)
        End Using
    End Sub

    ''' <summary>构造 CUPTI 监控参数（--sm 相关选项的统一入口）。</summary>
    Private Function BuildSmOptions(opts As Options) As CuptiMonitorOptions
        Dim options As New CuptiMonitorOptions()
        options.DeviceIndex = opts.DeviceIndex
        options.SamplingIntervalMs = opts.SmIntervalMs
        If opts.SmMetrics IsNot Nothing AndAlso opts.SmMetrics.Length > 0 Then options.Metrics = opts.SmMetrics
        If opts.SmHwBufferMB > 0 Then options.HardwareBufferBytes = CLng(opts.SmHwBufferMB) << 20
        options.ForceSystemClockTrigger = opts.SmSysClock
        Return options
    End Function

    ''' <summary>
    ''' 为实时仪表盘准备 CUPTI SM 归因会话；--no-sm 或初始化失败时返回 Nothing（仪表盘自动省略该区块）。
    ''' </summary>
    Private Function CreateSmMonitor(opts As Options) As CuptiMonitor
        If opts.NoSm Then Return Nothing

        Try
            Return New CuptiMonitor(BuildSmOptions(opts))
        Catch ex As Exception
            Console.Error.WriteLine($"警告：CUPTI SM 归因会话初始化失败（{ex.Message}），仪表盘将不含 SM 归因区块。")
            Return Nothing
        End Try
    End Function

    ''' <summary>
    ''' --sm 一次性模式：采集 N 秒（默认 3 秒，可用 --duration / --sm-duration 覆盖），
    ''' 打印能力诊断 + 概览 + per-SM 表（或 PM 切片 + 均衡推断）后退出。
    ''' </summary>
    Private Function RunSmOnce(opts As Options) As Integer
        Dim seconds = If(opts.SmDurationSec > 0, opts.SmDurationSec, If(opts.DurationSec > 0, opts.DurationSec, 3))

        Using monitor As New CuptiMonitor(BuildSmOptions(opts))
            Dim snapshot = monitor.Capture(seconds * 1000)
            Reporter.PrintSmSummary(snapshot, 3, Not opts.Plain, opts.SmRows)

            If Not snapshot.Capability.Available Then Return 2
        End Using

        Return 0
    End Function

    ''' <summary>
    ''' --sm --top 实时模式：固定行数原地刷新，Ctrl+C / --duration 停止。
    ''' </summary>
    Private Function RunSmLive(opts As Options) As Integer
        AddHandler Console.CancelKeyPress, AddressOf OnCancel

        Try
            Using monitor As New CuptiMonitor(BuildSmOptions(opts))
                If Not monitor.Capability.Available Then
                    Reporter.PrintSmCapability(monitor.Refresh())
                    Console.Error.WriteLine("错误：CUPTI PM Sampling 会话不可用，无法进入实时模式。")
                    Console.Error.WriteLine($"原因：{monitor.Capability.Reason}")
                    Return 2
                End If

                Dim start = DateTime.Now
                Dim durationSec = If(opts.SmDurationSec > 0, opts.SmDurationSec, opts.DurationSec)

                Dim stopFn As Func(Of Boolean) = Function()
                                                    If _cancelled Then Return True
                                                    If durationSec > 0 AndAlso (DateTime.Now - start).TotalSeconds >= durationSec Then Return True
                                                    Return False
                                                End Function

                Reporter.RunSmLive(monitor, Not opts.Plain, stopFn, opts.IntervalMs, If(opts.SmRows > 0, opts.SmRows, 16))
            End Using
        Finally
            RemoveHandler Console.CancelKeyPress, AddressOf OnCancel
        End Try

        Return 0
    End Function

    ''' <summary>
    ''' --sm-doctor：只做能力探测并打印诊断（不进入采集循环），用于排查
    ''' cupti64 载入、芯片识别、单 pass 限制与 per-SM 可用性。
    ''' </summary>
    Private Function RunSmDoctor(opts As Options) As Integer
        Using monitor As New CuptiMonitor(BuildSmOptions(opts))
            Dim snapshot = monitor.Refresh()
            Reporter.PrintSmCapability(snapshot)

            Console.WriteLine()
            Console.WriteLine($"  per-SM 事件探测 : {If(CuptiPerSmCounter.IsSupported(opts.DeviceIndex), "支持", "不支持（CUDA 13.x 已移除旧 Event API）")}")
            Console.WriteLine($"  CUPTI 动态库    : {If(String.IsNullOrEmpty(snapshot.Capability.CuptiPath), "N/A", snapshot.Capability.CuptiPath)}")
            Console.WriteLine($"  采样指标数      : {If(snapshot.Capability.Metrics Is Nothing, 0, snapshot.Capability.Metrics.Length)}")
            Console.WriteLine($"  有效切片        : {snapshot.SampleCount}")
            Console.WriteLine($"  SetConfig 返回码: {monitor.LastSetConfigResultCode}（0 = 成功）")
            Console.WriteLine($"  Start 返回码    : {monitor.LastStartResultCode}（0 = 成功）")
            Console.WriteLine($"  解码采样条数    : {monitor.LastDecodePopulatedSamples}")
            Console.WriteLine($"  最近错误        : {If(String.IsNullOrEmpty(monitor.LastError), "无", monitor.LastError)}")

            If Not snapshot.Capability.Available Then
                Console.Error.WriteLine()
                Console.Error.WriteLine("错误：CUPTI PM Sampling 不可用。")
                If Not String.IsNullOrEmpty(snapshot.Capability.Reason) Then
                    Console.Error.WriteLine($"原因：{snapshot.Capability.Reason}")
                End If

                Return 2
            End If
        End Using

        Return 0
    End Function

    Private Sub OnCancel(sender As Object, e As ConsoleCancelEventArgs)
        e.Cancel = True
        _cancelled = True
    End Sub

    Private Function ParseArgs(args As String()) As Options
        Dim opts As New Options()
        Dim i As Integer = 0

        While i < args.Length
            Dim a = args(i)
            Select Case a
                Case "--list", "-l"
                    opts.ListDevices = True
                Case "--once", "-o"
                    opts.Once = True
                Case "--table", "-t"
                    opts.Table = True
                Case "--top"
                    opts.TopExplicit = True
                Case "--processes", "-p"
                    opts.Processes = True
                Case "--sm"
                    opts.Sm = True
                Case "--sm-doctor"
                    opts.Sm = True
                    opts.SmDoctor = True
                Case "--sm-metrics"
                    i += 1
                    If i < args.Length Then
                        opts.SmMetrics = args(i) _
                            .Split(","c) _
                            .Select(Function(s) s.Trim()) _
                            .Where(Function(s) s.Length > 0) _
                            .ToArray()
                    End If
                Case "--sm-interval"
                    i += 1
                    If i < args.Length Then Double.TryParse(args(i), opts.SmIntervalMs)
                Case "--sm-duration"
                    i += 1
                    If i < args.Length Then Integer.TryParse(args(i), opts.SmDurationSec)
                Case "--sm-rows"
                    i += 1
                    If i < args.Length Then Integer.TryParse(args(i), opts.SmRows)
                Case "--sm-hwbuffer"
                    i += 1
                    If i < args.Length Then Integer.TryParse(args(i), opts.SmHwBufferMB)
                Case "--sm-sysclk"
                    opts.SmSysClock = True
                Case "--no-sm"
                    opts.NoSm = True
                Case "--cupti"
                    i += 1
                    If i < args.Length Then opts.CuptiPath = args(i)
                Case "--plain"
                    opts.Plain = True
                Case "--help", "-h", "-?", "/?"
                    opts.Help = True
                Case "--device"
                    i += 1
                    If i < args.Length Then Integer.TryParse(args(i), opts.DeviceIndex)
                Case "--interval"
                    i += 1
                    If i < args.Length Then Integer.TryParse(args(i), opts.IntervalMs)
                Case "--rows"
                    i += 1
                    If i < args.Length Then Integer.TryParse(args(i), opts.MaxRows)
                Case "--duration"
                    i += 1
                    If i < args.Length Then Integer.TryParse(args(i), opts.DurationSec)
                Case "--csv"
                    i += 1
                    If i < args.Length Then opts.CsvPath = args(i)
            End Select
            i += 1
        End While

        Return opts
    End Function

    Private Sub PrintHelp()
        Console.WriteLine("GTop —— 基于 NVML 的 NVIDIA GPU 性能监测命令行工具")
        Console.WriteLine()
        Console.WriteLine("用法：")
        Console.WriteLine("  GTop [选项]")
        Console.WriteLine("  GTop            不带参数启动：实时仪表盘，输出完整性能信息（设备指标 + 每进程指标）")
        Console.WriteLine()
        Console.WriteLine("选项：")
        Console.WriteLine("  --list          枚举全部 NVIDIA GPU 并打印清单表，然后退出")
        Console.WriteLine("  --once          对指定设备取一次快照并打印，然后退出")
        Console.WriteLine("  --top           实时刷新仪表盘（默认模式，含设备指标与每进程指标；Ctrl+C 停止）")
        Console.WriteLine("  --table         采样一段时间并打印历史二维表")
        Console.WriteLine("  --csv <文件>    采样一段时间并导出 CSV（配合 --duration 或 Ctrl+C）")
        Console.WriteLine("  --processes, -p 仅打印一次'正在使用 GPU 计算的进程'二维表后退出（Windows PDH）")
        Console.WriteLine("  --sm            采集 CUPTI 硬件指标（PM Sampling），打印 SM 归因报告后退出（不需要 NVML）")
        Console.WriteLine("  --sm --top      实时刷新 SM 归因（per-SM 明细 / PM 切片 + 均衡推断），Ctrl+C 停止")
        Console.WriteLine("  --sm-doctor     只做 CUPTI 能力探测并打印诊断（cupti64、芯片、单 pass 上限、per-SM 可用性）")
        Console.WriteLine("  --sm-metrics <列表> 逗号分隔的指标名（默认 sm__cycles_active.avg 等 6 项；裸名会自动补全后缀）")
        Console.WriteLine("  --sm-interval <ms>   PM Sampling 的采样间隔毫秒（默认 1.0）")
        Console.WriteLine("  --sm-duration <秒>   --sm 的采集时长（默认 3 秒，优先于 --duration）")
        Console.WriteLine("  --sm-rows <n>        一次性模式最多打印多少个 SM 行（0 = 全部；实时模式为固定行数，默认 16）")
        Console.WriteLine("  --sm-hwbuffer <MB>   PM Sampling 的硬件缓冲大小（默认 64 MB；GPU 满载时过小会记录溢出）")
        Console.WriteLine("  --sm-sysclk          强制用 SYSCLK 触发（默认优先纳秒触发；Turing/GA100 需此开关）")
        Console.WriteLine("  --no-sm          实时仪表盘不加载 CUPTI SM 归因区块（默认会加载，失败时自动省略）")
        Console.WriteLine("  --cupti <dll>    显式指定 cupti64 动态库（默认自动探测 CUDA Toolkit 的 extras\CUPTI\lib64）")
        Console.WriteLine("  --device <n>    指定 GPU 序号（默认 0）")
        Console.WriteLine("  --interval <ms> 轮询间隔毫秒（默认 1000）")
        Console.WriteLine("  --rows <n>      历史表/缓冲保留的最大行数（默认 20）")
        Console.WriteLine("  --duration <秒> 采样持续秒数（默认 0 = 直到 Ctrl+C）")
        Console.WriteLine("  --plain         关闭 ANSI 颜色，便于管道/重定向")
        Console.WriteLine("  --help          显示本帮助")
        Console.WriteLine()
        Console.WriteLine("指标：CUDA 核心(SM)利用率、显存利用率、设备数据拷贝(PCIe TX/RX 吞吐)，")
        Console.WriteLine("      以及功耗、温度、SM/显存时钟、风扇、编解码利用率。")
        Console.WriteLine("进程指标(--processes)：每进程 CUDA核心%(=3D/Compute 引擎利用率最大值，Windows 下 CUDA 常计入 3D 引擎)、")
        Console.WriteLine("      拷贝引擎%(带宽代理)、编解码% 与专用显存(MB)。每进程字节吞吐 Windows 不提供，以拷贝引擎利用率代理。")
        Console.WriteLine("实时仪表盘(默认/--top)另含：显卡硬件详细参数、CUDA/显存历史曲线、每进程 CUDA 历史曲线；")
        Console.WriteLine("      进程按 (CUDA%+1)×(显存占用%+1)×(Copy%+1)×(编码%+解码%+1) 实时降序，失去活动后保留 1 分钟再移除。")
        Console.WriteLine("NVML 随 NVIDIA 显示驱动发布，无需 CUDA Toolkit、无需建立 CUDA 上下文即可监控整卡。")
        Console.WriteLine()
        Console.WriteLine("SM 归因(--sm)：通过 CUPTI 的 PM Sampling 按时间切片采集 sm__cycles_active / sm__warps_active / sm__throughput，")
        Console.WriteLine("      给出整卡 SM 利用率、活跃 warp、吞吐与 SM 失衡推断；若 CUPTI 仍支持旧 Event API（CUDA <= 12.7），")
        Console.WriteLine("      则额外给出真正的逐 SM（instance）明细表，可定位哪个 SM 空转。")
        Console.WriteLine("      注意：CUPTI 需要 CUDA Toolkit 中的 cupti64 动态库；PM Sampling 通常需要管理员权限，")
        Console.WriteLine("      且与 Nsight Compute 等 profiler 互斥（同一设备同时只允许一个 profiling 会话）。")
        Console.WriteLine("      NVIDIA 自 CUDA 13.0 起已移除 per-instance 的旧 Event/Metric API（错误码 38），此时 --sm 会自动降级。")
    End Sub
End Module
