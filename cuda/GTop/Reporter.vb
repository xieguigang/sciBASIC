#Region "Microsoft.VisualBasic::gtop-reporter, cuda\GTop\Reporter.vb"

' Author:
'       xieguigang (xie.guigang@live.com)
'
' Copyright (c) sciBASIC.NET Foundation
' GNU GENERAL PUBLIC LICENSE (GPL3)

#End Region

Imports System.Drawing
Imports System.Globalization
Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices.Terminal
Imports Microsoft.VisualBasic.ApplicationServices.Terminal.TablePrinter
Imports Microsoft.VisualBasic.ApplicationServices.Terminal.TablePrinter.Flags
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Unit
Imports Microsoft.VisualBasic.Computing.ILCuda.Runtime

''' <summary>
''' GTop 的渲染层：
'''   * 二维表（设备清单 / 快照 / 历史）使用 ConsoleTableBuilder；
'''   * 实时仪表盘（--top）使用 ANSI 转义序列做覆盖式刷新与阈值着色；
'''   * --plain 关闭颜色，便于管道。
''' 本模块只负责排版，不持有任何 GPU 状态。
''' </summary>
Public Module Reporter

    ' ------------------------------------------------------------------
    ' 数值 -> 显示字符串（哨兵 -1 / 不可用 -> "N/A"）
    ' ------------------------------------------------------------------
    Private Function FmtPercent(v As Integer) As String
        If v < 0 Then Return "N/A"
        Return $"{v}%"
    End Function

    Private Function FmtMHz(v As Integer) As String
        If v < 0 Then Return "N/A"
        Return v.ToString(CultureInfo.InvariantCulture)
    End Function

    Private Function FmtTemp(v As Integer) As String
        If v < 0 Then Return "N/A"
        Return $"{v}℃"
    End Function

    Private Function FmtMB(v As Double) As String
        If v <= 0 Then Return "0"
        Return v.ToString("N0", CultureInfo.InvariantCulture)
    End Function

    Private Function FmtDouble(v As Double) As String
        If v < 0 Then Return "N/A"
        Return v.ToString("F1", CultureInfo.InvariantCulture)
    End Function

    Private Function FmtWatts(v As Double) As String
        If v < 0 Then Return "N/A"
        Return v.ToString("F1", CultureInfo.InvariantCulture)
    End Function

    ' ------------------------------------------------------------------
    ' 设备清单表
    ' ------------------------------------------------------------------
    Public Sub PrintDeviceList(devices As IReadOnlyList(Of NvmlDevice))
        Dim rows As New List(Of Object())()

        For Each d In devices
            Dim cc = d.GetCudaComputeCapability()
            Dim ccStr = If(cc.major < 0, "N/A", $"{cc.major}.{cc.minor}")
            Dim link = d.GetPcieLink()
            Dim linkStr = If(link.gen < 0, "N/A", $"Gen{link.gen} x{link.width}")
            Dim uuid = If(String.IsNullOrEmpty(d.Uuid), "N/A", d.Uuid)

            rows.Add({d.Ordinal, d.Name, uuid, ccStr, linkStr})
        Next

        ConsoleTableBuilder _
            .From(rows) _
            .WithColumn("#", "名称", "UUID", "计算能力", "PCIe") _
            .WithTitle("NVIDIA GPU 设备列表", ConsoleColor.Cyan) _
            .ExportAndWriteLine(TableAligntment.Left)
    End Sub

    ' ------------------------------------------------------------------
    ' 单次快照表（指标 / 数值 两列）
    ' ------------------------------------------------------------------
    Public Sub PrintSnapshot(device As NvmlDevice, s As NvmlSample)
        Dim rows As New List(Of Object())()
        AddRow(rows, "GPU 利用率 (SM 活跃)", FmtPercent(s.GpuPercent))
        AddRow(rows, "显存控制器利用率", FmtPercent(s.MemoryControllerPercent))

        If s.MemoryAvailable Then
            AddRow(rows, "显存已用", $"{FmtMB(s.UsedMB)} MB")
            AddRow(rows, "显存总量", $"{FmtMB(s.TotalMB)} MB")
            AddRow(rows, "显存利用率", $"{s.MemoryUsedRatio:P1}")
        Else
            AddRow(rows, "显存", "N/A")
        End If

        AddRow(rows, "PCIe 发送 (设备→主机)", $"{FmtDouble(s.PcieTxMBps)} MB/s")
        AddRow(rows, "PCIe 接收 (主机→设备)", $"{FmtDouble(s.PcieRxMBps)} MB/s")
        AddRow(rows, "功耗", FmtWatts(s.PowerWatts))
        AddRow(rows, "温度", FmtTemp(s.TemperatureC))
        AddRow(rows, "SM 时钟", If(s.SmClockMHz < 0, "N/A", $"{s.SmClockMHz} MHz"))
        AddRow(rows, "显存时钟", If(s.MemClockMHz < 0, "N/A", $"{s.MemClockMHz} MHz"))
        AddRow(rows, "风扇转速", FmtPercent(s.FanSpeedPercent))
        AddRow(rows, "编码器利用率", FmtPercent(s.EncoderPercent))
        AddRow(rows, "解码器利用率", FmtPercent(s.DecoderPercent))
        AddRow(rows, "采样时间", s.Time.ToString("yyyy-MM-dd HH:mm:ss"))

        ConsoleTableBuilder _
            .From(rows) _
            .WithColumn("指标", "数值") _
            .WithTitle($"{device.Name}  [{device.Ordinal}]  实时快照", ConsoleColor.Cyan) _
            .ExportAndWriteLine(TableAligntment.Left)
    End Sub

    ' ------------------------------------------------------------------
    ' 历史采样表
    ' ------------------------------------------------------------------
    Public Sub PrintHistory(device As NvmlDevice, samples As IReadOnlyList(Of NvmlSample))
        If samples Is Nothing OrElse samples.Count = 0 Then
            Console.WriteLine("没有可用的采样数据。")
            Return
        End If

        Dim rows As New List(Of Object())()

        For Each s In samples
            rows.Add({
                s.Time.ToString("HH:mm:ss"),
                FmtPercent(s.GpuPercent),
                FmtPercent(s.MemoryControllerPercent),
                If(s.MemoryAvailable, $"{FmtMB(s.UsedMB)}/{FmtMB(s.TotalMB)}", "N/A"),
                FmtDouble(s.PcieTxMBps),
                FmtDouble(s.PcieRxMBps),
                FmtWatts(s.PowerWatts),
                FmtTemp(s.TemperatureC),
                If(s.SmClockMHz < 0, "N/A", s.SmClockMHz.ToString(CultureInfo.InvariantCulture)),
                FmtPercent(s.FanSpeedPercent),
                FmtPercent(s.EncoderPercent),
                FmtPercent(s.DecoderPercent)
            })
        Next

        ConsoleTableBuilder _
            .From(rows) _
            .WithColumn("时间", "GPU%", "显存%", "显存(MB)", "PCIeTX", "PCIeRX", "功耗W", "温度℃", "SM时钟", "风扇%", "编码%", "解码%") _
            .WithTitle($"{device.Name}  [{device.Ordinal}]  采样历史 (共 {rows.Count} 条)", ConsoleColor.Cyan) _
            .ExportAndWriteLine(TableAligntment.Left)
    End Sub

    ' ------------------------------------------------------------------
    ' 进程列表表（每进程 CUDA 计算/拷贝引擎利用率 + 专用显存）
    ' ------------------------------------------------------------------
    Public Sub PrintProcesses(rows As IReadOnlyList(Of GpuProcessSample), Optional topN As Integer = 0)
        If rows Is Nothing OrElse rows.Count = 0 Then
            Console.WriteLine("当前没有检测到正在使用 GPU 计算引擎的进程。")
            Return
        End If

        Dim count = If(topN > 0, System.Math.Min(topN, rows.Count), rows.Count)
        Dim table As New List(Of Object())()

        For i As Integer = 0 To count - 1
            Dim p = rows(i)
            table.Add({
                p.Pid,
                FmtPercent(p.CudaCorePercent),
                FmtPercent(p.CopyEnginePercent),
                FmtPercent(p.EncoderPercent),
                FmtPercent(p.DecoderPercent),
                FmtMemMB(p.DedicatedMB),
                PadName(p.Name)
            })
        Next

        Console.WriteLine()
        Console.WriteLine("说明：CUDA核心% = 3D/Compute 引擎利用率最大值（Windows 下 CUDA 计算常计入 3D 引擎）；Copy% 为数据拷贝带宽代理。")
        Console.WriteLine()

        ' 注意：表格库的 EmbedTitle 在标题长度超过表格宽度时会抛异常，故标题保持简短
        ' 进程名放最后一列：含中文（全角）时其显示宽度与字符数不一致，放末尾可避免影响前面数值列的对齐
        ConsoleTableBuilder _
            .From(table) _
            .WithColumn("PID", "CUDA核心%", "Copy%", "编码%", "解码%", "显存MB", "进程名") _
            .WithTitle($"GPU 进程列表（共 {count} 个）", ConsoleColor.Cyan) _
            .ExportAndWriteLine(TableAligntment.Left)
    End Sub

    ' ------------------------------------------------------------------
    ' CSV 导出
    ' ------------------------------------------------------------------
    Public Sub WriteCsv(samples As IReadOnlyList(Of NvmlSample), path As String)
        Using sw As New IO.StreamWriter(path, False, Encoding.UTF8)
            sw.WriteLine("time,gpu_percent,memctrl_percent,mem_used_mb,mem_total_mb,mem_used_ratio,pcie_tx_mbps,pcie_rx_mbps,power_watts,temperature_c,sm_clock_mhz,mem_clock_mhz,fan_percent,encoder_percent,decoder_percent")

            For Each s In samples
                sw.WriteLine($"{s.Time:yyyy-MM-ddTHH:mm:ss}," &
                             $"{If(s.GpuPercent < 0, "", s.GpuPercent.ToString())}," &
                             $"{If(s.MemoryControllerPercent < 0, "", s.MemoryControllerPercent.ToString())}," &
                             $"{If(s.MemoryAvailable, s.UsedMB.ToString("F1", CultureInfo.InvariantCulture), "")}," &
                             $"{If(s.MemoryAvailable, s.TotalMB.ToString("F1", CultureInfo.InvariantCulture), "")}," &
                             $"{If(s.MemoryAvailable, s.MemoryUsedRatio.ToString("F4", CultureInfo.InvariantCulture), "")}," &
                             $"{If(s.PcieTxMBps < 0, "", s.PcieTxMBps.ToString("F3", CultureInfo.InvariantCulture))}," &
                             $"{If(s.PcieRxMBps < 0, "", s.PcieRxMBps.ToString("F3", CultureInfo.InvariantCulture))}," &
                             $"{If(s.PowerWatts < 0, "", s.PowerWatts.ToString("F3", CultureInfo.InvariantCulture))}," &
                             $"{If(s.TemperatureC < 0, "", s.TemperatureC.ToString())}," &
                             $"{If(s.SmClockMHz < 0, "", s.SmClockMHz.ToString())}," &
                             $"{If(s.MemClockMHz < 0, "", s.MemClockMHz.ToString())}," &
                             $"{If(s.FanSpeedPercent < 0, "", s.FanSpeedPercent.ToString())}," &
                             $"{If(s.EncoderPercent < 0, "", s.EncoderPercent.ToString())}," &
                             $"{If(s.DecoderPercent < 0, "", s.DecoderPercent.ToString())}")
            Next
        End Using
    End Sub

    ' ------------------------------------------------------------------
    ' 实时仪表盘（--top）
    ' ------------------------------------------------------------------
    Public Sub RunLive(sampler As MonitorSampler, device As NvmlDevice, useColor As Boolean, shouldStop As Func(Of Boolean),
                       Optional processMonitor As ProcessMonitor = Nothing,
                       Optional hardware As CudaDevice = Nothing,
                       Optional processRows As Integer = 0)
        If useColor Then
            RunLiveColor(sampler, device, shouldStop, processMonitor, hardware, processRows)
        Else
            RunLivePlain(sampler, device, shouldStop, processMonitor)
        End If
    End Sub

    Private Sub RunLivePlain(sampler As MonitorSampler, device As NvmlDevice, shouldStop As Func(Of Boolean),
                             processMonitor As ProcessMonitor)
        sampler.Run(shouldStop, Sub(s)
                                    Console.WriteLine($"{s.Time:HH:mm:ss} GPU={FmtPercent(s.GpuPercent)} 显存控={FmtPercent(s.MemoryControllerPercent)} 显存={FmtMB(s.UsedMB)}/{FmtMB(s.TotalMB)}MB PCIe TX={FmtDouble(s.PcieTxMBps)} RX={FmtDouble(s.PcieRxMBps)} 功耗={FmtWatts(s.PowerWatts)}W 温度={FmtTemp(s.TemperatureC)}")
                                    If processMonitor IsNot Nothing Then
                                        processMonitor.Update()
                                        For Each p In processMonitor.Rows()
                                            Console.WriteLine($"{s.Time:HH:mm:ss} PID={p.Pid,7} CUDA={FmtPercent(p.Cuda)} Copy={FmtPercent(p.Copy)} 编码={FmtPercent(p.Enc)} 解码={FmtPercent(p.Dec)} 显存={StringFormats.Lanudry(p.MemMB * ByteSize.MB)} {Sparkline.Render(p.History, 20)} {PadName(p.Name)}")
                                        Next
                                    End If
                                End Sub)
    End Sub

    Private Sub RunLiveColor(sampler As MonitorSampler, device As NvmlDevice, shouldStop As Func(Of Boolean),
                             processMonitor As ProcessMonitor, hardware As CudaDevice, processRows As Integer)
        Dim frameLines As Integer = 0
        ' 进程块固定高度，避免样本数波动导致原地刷新残影
        Dim procRows = If(processRows > 0, System.Math.Min(processRows, 15), 0)

        Dim cudaHist As New List(Of Integer)()
        Dim memHist As New List(Of Integer)()
        Const histLen As Integer = 60

        Static gray As New ConsoleFormat(AnsiColor.FromRgbColor(Color.DimGray), AnsiColor.Black, Underline:=True)

        sampler.Run(shouldStop, Sub(s)
                                    AddHistory(cudaHist, s.GpuPercent, histLen)
                                    AddHistory(memHist, CInt(s.MemoryUsedRatio * 100), histLen)
                                    If processMonitor IsNot Nothing Then processMonitor.Update()

                                    Dim sb As New StringBuilder()
                                    If frameLines > 0 Then sb.Append(AnsiEscapeCodes.GetMoveCursorUp(frameLines))

                                    Dim lines = BuildLiveLines(device, s)
                                    If hardware IsNot Nothing Then lines.AddRange(BuildHardwareLines(hardware))
                                    lines.AddRange(BuildCurveLines(cudaHist, memHist))
                                    If processMonitor IsNot Nothing AndAlso procRows > 0 Then
                                        lines.AddRange(BuildProcessLines(processMonitor.Rows(), procRows, True))
                                    End If
                                    lines.Add("")
                                    lines.Add(New TextSpan("  按 Ctrl+C 停止", gray))
                                    lines.Add(AnsiEscapeCodes.Reset)

                                    For i As Integer = 0 To lines.Count - 1
                                        sb.Append(AnsiEscapeCodes.ClearLine)
                                        sb.Append(lines(i))
                                        sb.Append(vbCrLf)
                                    Next

                                    Console.Out.Write(sb.ToString())
                                    Console.Out.Flush()
                                    frameLines = lines.Count
                                End Sub)

        ' 结束后再下一行，留给命令行提示符
        Console.Out.Write(vbCrLf)
        Console.Out.Flush()
    End Sub

    Private Sub AddHistory(hist As List(Of Integer), value As Integer, maxLen As Integer)
        hist.Add(value)
        If hist.Count > maxLen Then hist.RemoveAt(0)
    End Sub

    ''' <summary>静态硬件详细参数（来自 CUDA Driver API 的设备属性）。</summary>
    Private Function BuildHardwareLines(d As CudaDevice) As List(Of String)
        Dim L As New List(Of String)()
        L.Add($"  硬件信息 : {d.Name}   计算能力 {d.ComputeCapability}   SM {d.MultiprocessorCount}   Warp {d.WarpSize}   线程/块 {d.MaxThreadsPerBlock}   共享内存 {d.MaxSharedMemoryPerBlock \ 1024} KB")
        L.Add($"             核心 {d.ClockRateKhz \ 1000} MHz   显存 {d.MemoryClockRateKhz \ 1000} MHz   位宽 {d.GlobalMemoryBusWidth} bit   L2 {d.L2CacheSize \ 1024} KB   显存总量 {FmtMB(d.TotalMemoryMB)} MB")
        Return L
    End Function

    ''' <summary>设备级历史曲线（CUDA 利用率 / 显存利用率，迷你曲线）。</summary>
    Private Function BuildCurveLines(cudaHist As IReadOnlyList(Of Integer), memHist As IReadOnlyList(Of Integer)) As List(Of String)
        Const w As Integer = 60
        Return New List(Of String) From {
            $"  CUDA历史 : {Sparkline.Render(cudaHist, w)}",
            $"  显存历史 : {Sparkline.Render(memHist, w)}"
        }
    End Function

    Private Function BuildLiveLines(device As NvmlDevice, s As NvmlSample) As List(Of String)
        Dim L As New List(Of String)()

        L.Add($"GTop — {device.Name}  [{device.Ordinal}]   {s.Time:HH:mm:ss}")

        L.Add(Bar("GPU", s.GpuPercent, True))
        L.Add(Bar("MEM", CInt(s.MemoryUsedRatio * 100), True))
        L.Add(Bar("FAN", s.FanSpeedPercent, True))

        If s.MemoryAvailable Then
            L.Add($"  显存     : {FmtMB(s.UsedMB)} / {FmtMB(s.TotalMB)} MB ({s.MemoryUsedRatio:P1})   显存控制器 : {FmtPercent(s.MemoryControllerPercent)}")
        Else
            L.Add($"  显存     : N/A   显存控制器 : {FmtPercent(s.MemoryControllerPercent)}")
        End If

        L.Add($"  PCIe     : TX {FmtDouble(s.PcieTxMBps)} MB/s   RX {FmtDouble(s.PcieRxMBps)} MB/s")
        L.Add($"  时钟     : SM {FmtMHz(s.SmClockMHz)} MHz   MEM {FmtMHz(s.MemClockMHz)} MHz   功耗 {FmtWatts(s.PowerWatts)} W   温度 {FmtTemp(s.TemperatureC)}")
        L.Add($"  编/解码  : 编码 {FmtPercent(s.EncoderPercent)}   解码 {FmtPercent(s.DecoderPercent)}")

        Return L
    End Function

    ''' <summary>
    ''' 构建实时仪表盘中的进程块，采用"三线表"格式（仅顶线、表头下横线、底线；无竖线与内部横线）。
    ''' 表格宽度固定为命令行窗口宽度的 85% 并水平居中；进程名过长时按列宽裁剪。
    ''' 在"进程名"之前插入一列"CUDA曲线"，显示该进程最近的 CUDA 利用率历史（迷你曲线）。
    ''' 列宽与横线长度按显示宽度（全角按 2 列）计算；固定高度 procRows 行，不足补空行以稳定原地刷新。
    ''' </summary>
    Private Function BuildProcessLines(rows As IReadOnlyList(Of ProcessMonitor.Row), procRows As Integer, useColor As Boolean) As List(Of String)
        Dim L As New List(Of String)()

        ' 目标表格宽度：窗口宽度的 85%（列间分隔符 2 空格）
        Const gap As Integer = 2
        Const curveW As Integer = 16
        Dim winW = GetConsoleWidth()
        Dim tableW = CInt(System.Math.Round(winW * 0.85))

        Dim headers = {"PID", "CUDA%", "Copy%", "编码%", "解码%", "显存", "CUDA曲线", "进程名"}
        Dim rightAlign = {True, True, True, True, True, True, False, False}
        Dim widths = {7, 6, 6, 6, 6, 9, curveW, 0}   ' 末列（进程名）占剩余宽度

        Dim fixedSum As Integer = 0
        For c As Integer = 0 To 6
            fixedSum += widths(c)
        Next

        Dim nameW = tableW - gap * (headers.Length - 1) - fixedSum
        Const minNameW As Integer = 8
        If nameW < minNameW Then
            nameW = minNameW
            tableW = fixedSum + gap * (headers.Length - 1) + nameW
        End If
        widths(7) = nameW

        ' 水平居中：左缩进 = (窗口宽度 - 表格宽度) / 2
        Dim centerPad = System.Math.Max(0, (winW - tableW) \ 2)
        Dim padStr = New String(" "c, centerPad)
        Dim rule = New String("─"c, tableW)

        L.Add("")
        L.Add("")
        L.Add(padStr & TruncateByWidth($"进程级 GPU 使用（按活动度乘积降序；无活动保留 1 分钟；共 {rows.Count} 个）", tableW))

        ' 组装数据单元格（曲线与进程名依次放最后两列）
        Dim n = System.Math.Min(rows.Count, procRows)
        Dim dataCells As New List(Of String())()
        Dim dataCuda As New List(Of Integer)()

        For i As Integer = 0 To n - 1
            Dim r = rows(i)
            dataCells.Add({
                r.Pid.ToString(),
                FmtPercent(r.Cuda),
                FmtPercent(r.Copy),
                FmtPercent(r.Enc),
                FmtPercent(r.Dec),
                StringFormats.Lanudry(r.MemMB * ByteSize.MB),
                Sparkline.Render(r.History, curveW),
                TruncateByWidth(PadName(r.Name), nameW)
            })
            dataCuda.Add(r.Cuda)
        Next

        L.Add(padStr & rule)                                                              ' 顶线
        L.Add(padStr & BuildProcessRow(headers, widths, rightAlign, -1, 0, False))        ' 表头
        L.Add(padStr & rule)                                                              ' 表头下横线

        For i As Integer = 0 To procRows - 1
            If i >= n Then
                L.Add(String.Empty)   ' 补空行，保持块高恒定，避免刷新残影
                Continue For
            End If
            L.Add(padStr & BuildProcessRow(dataCells(i), widths, rightAlign, 1, dataCuda(i), useColor))
        Next

        L.Add(padStr & rule)                                                              ' 底线

        Return L
    End Function

    ''' <summary>取命令行窗口宽度；输出被重定向或无控制台时回退到 120。</summary>
    Private Function GetConsoleWidth() As Integer
        Try
            Dim w = Console.WindowWidth
            If w > 20 Then Return w
        Catch
        End Try

        Try
            Dim w = Console.BufferWidth
            If w > 20 Then Return w
        Catch
        End Try

        Return 120
    End Function

    ''' <summary>按列宽（显示宽度）对齐地拼装一行，可对指定列按阈值着色。</summary>
    Private Function BuildProcessRow(cells As String(), widths As Integer(), rightAlign As Boolean(),
                                     colorIndex As Integer, colorPercent As Integer, useColor As Boolean) As String
        Dim parts As New List(Of String)()

        For c As Integer = 0 To cells.Length - 1
            Dim pad = System.Math.Max(0, widths(c) - UnicodeWidth.GetWidth(cells(c)))
            Dim cell = If(rightAlign(c), New String(" "c, pad) & cells(c), cells(c) & New String(" "c, pad))

            If useColor AndAlso c = colorIndex Then
                cell = Colorize(cell, colorPercent)
            End If

            parts.Add(cell)
        Next

        Return String.Join("  ", parts)
    End Function

    ''' <summary>按显示宽度截断字符串（超出时以省略号结尾）。</summary>
    Private Function TruncateByWidth(text As String, maxWidth As Integer) As String
        If String.IsNullOrEmpty(text) OrElse UnicodeWidth.GetWidth(text) <= maxWidth Then
            Return text
        End If

        Dim sb As New StringBuilder()
        Dim w As Integer = 0
        For Each ch As Char In text
            Dim cw = UnicodeWidth.GetWidth(ch)
            If w + cw > maxWidth - 1 Then Exit For
            sb.Append(ch)
            w += cw
        Next

        Return sb.ToString() & "…"
    End Function

    ''' <summary>按阈值给文本着色（&lt;50 亮绿、&lt;80 亮黄、否则亮红；不可用不着色）。</summary>
    Private Function Colorize(text As String, percent As Integer) As String
        If percent < 0 Then Return text

        Dim color As AnsiColor
        If percent < 50 Then
            color = AnsiColor.BrightGreen
        ElseIf percent < 80 Then
            color = AnsiColor.BrightYellow
        Else
            color = AnsiColor.BrightRed
        End If

        Return color.GetEscapeSequence() & text & AnsiEscapeCodes.Reset
    End Function

    Private Function FmtMemMB(v As Double) As String
        Return v.ToString("N1", CultureInfo.InvariantCulture)
    End Function

    Private Function PadName(name As String) As String
        Return If(String.IsNullOrEmpty(name), "?", name)
    End Function

    ''' <summary>阈值着色的利用率条：&lt;50 绿、&lt;80 黄、否则红。</summary>
    Private Function Bar(label As String, percent As Integer, useColor As Boolean) As String
        If percent < 0 Then
            Return $"  {label}: N/A"
        End If

        Dim width As Integer = 32
        Dim filled As Integer = CInt(Math.Round(percent / 100.0 * width))
        If filled < 0 Then filled = 0
        If filled > width Then filled = width

        Dim barText = New String("#"c, filled) & New String("-"c, width - filled)

        If Not useColor Then
            Return $"  {label}: [{barText}] {percent}%"
        End If

        Dim color As AnsiColor
        If percent < 50 Then
            color = AnsiColor.BrightGreen
        ElseIf percent < 80 Then
            color = AnsiColor.BrightYellow
        Else
            color = AnsiColor.BrightRed
        End If

        Return $"  {label}: [{color.GetEscapeSequence()}{barText}{AnsiEscapeCodes.Reset}] {percent}%"
    End Function

    Private Sub AddRow(rows As List(Of Object()), name As String, value As String)
        rows.Add({name, DirectCast(value, Object)})
    End Sub
End Module
