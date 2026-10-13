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
    End Class

    Sub Main(args As String())
        Dim opts = ParseArgs(args)

        If opts.Help Then
            PrintHelp()
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
            ' 实时仪表盘（默认模式）：完整信息 = 设备指标 + 硬件参数 + 历史曲线 + 每进程指标
            Dim hw = TryGetHardware(opts.DeviceIndex)

            If GpuProcessCounter.IsAvailable() Then
                Using procCounter As New GpuProcessCounter()
                    Dim total = device.GetMemoryInfo().TotalBytes
                    Dim procMonitor As New ProcessMonitor(procCounter, total, 20, TimeSpan.FromMinutes(1))
                    Reporter.RunLive(sampler, device, Not opts.Plain, stopFn, procMonitor, hw, opts.MaxRows)
                End Using
            Else
                If opts.Processes Then
                    Console.Error.WriteLine("警告：进程级 GPU 计数器不可用，仅显示设备仪表盘。")
                End If
                Reporter.RunLive(sampler, device, Not opts.Plain, stopFn, Nothing, hw, 0)
            End If
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
    End Sub
End Module
