#Region "Microsoft.VisualBasic::gtop-entry, cuda\GTop\Program.vb"

    ' Author:
    '       xieguigang (xie.guigang@live.com)
    '
    ' Copyright (c) sciBASIC.NET Foundation
    ' GNU GENERAL PUBLIC LICENSE (GPL3)

#End Region

Imports System
Imports System.Collections.Generic
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
    End Class

    Sub Main(args As String())
        Dim opts = ParseArgs(args)

        If opts.Help Then
            PrintHelp()
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
            Reporter.RunLive(sampler, device, Not opts.Plain, stopFn)
        End If

        RemoveHandler Console.CancelKeyPress, AddressOf OnCancel
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
                    ' 默认即 top 模式，显式标记无副作用
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
        Console.WriteLine()
        Console.WriteLine("选项：")
        Console.WriteLine("  --list          枚举全部 NVIDIA GPU 并打印清单表，然后退出")
        Console.WriteLine("  --once          对指定设备取一次快照并打印，然后退出")
        Console.WriteLine("  --top           实时刷新仪表盘（默认模式；按 Ctrl+C 停止）")
        Console.WriteLine("  --table         采样一段时间并打印历史二维表")
        Console.WriteLine("  --csv <文件>    采样一段时间并导出 CSV（配合 --duration 或 Ctrl+C）")
        Console.WriteLine("  --device <n>    指定 GPU 序号（默认 0）")
        Console.WriteLine("  --interval <ms> 轮询间隔毫秒（默认 1000）")
        Console.WriteLine("  --rows <n>      历史表/缓冲保留的最大行数（默认 20）")
        Console.WriteLine("  --duration <秒> 采样持续秒数（默认 0 = 直到 Ctrl+C）")
        Console.WriteLine("  --plain         关闭 ANSI 颜色，便于管道/重定向")
        Console.WriteLine("  --help          显示本帮助")
        Console.WriteLine()
        Console.WriteLine("指标：CUDA 核心(SM)利用率、显存利用率、设备数据拷贝(PCIe TX/RX 吞吐)，")
        Console.WriteLine("      以及功耗、温度、SM/显存时钟、风扇、编解码利用率。")
        Console.WriteLine("NVML 随 NVIDIA 显示驱动发布，无需 CUDA Toolkit、无需建立 CUDA 上下文即可监控整卡。")
    End Sub
End Module
