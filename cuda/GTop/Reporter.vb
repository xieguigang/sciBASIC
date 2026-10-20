#Region "Microsoft.VisualBasic::463881b2cf61e61e51aaef320e34972d, cuda\GTop\Reporter.vb"

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

    '   Total Lines: 514
    '    Code Lines: 384 (74.71%)
    ' Comment Lines: 48 (9.34%)
    '    - Xml Docs: 41.67%
    ' 
    '   Blank Lines: 82 (15.95%)
    '     File Size: 25.26 KB


    ' Module Reporter
    ' 
    '     Function: Bar, BuildCurveLines, BuildHardwareLines, BuildLiveLines, BuildProcessLines
    '               BuildProcessRow, Colorize, FmtDouble, FmtMB, FmtMemMB
    '               FmtMHz, FmtPercent, FmtTemp, FmtWatts, GetConsoleWidth
    '               PadName, TruncateByWidth
    ' 
    '     Sub: AddHistory, AddRow, PrintDeviceList, PrintHistory, PrintProcesses
    '          PrintSnapshot, RunLive, RunLiveColor, RunLivePlain, WriteCsv
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Globalization
Imports System.Linq
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
                       Optional processRows As Integer = 0,
                       Optional smMonitor As CuptiMonitor = Nothing,
                       Optional smRows As Integer = 8)
        If useColor Then
            RunLiveColor(sampler, device, shouldStop, processMonitor, hardware, processRows, smMonitor, smRows)
        Else
            RunLivePlain(sampler, device, shouldStop, processMonitor, smMonitor)
        End If
    End Sub

    Private Sub RunLivePlain(sampler As MonitorSampler, device As NvmlDevice, shouldStop As Func(Of Boolean),
                             processMonitor As ProcessMonitor, Optional smMonitor As CuptiMonitor = Nothing)
        sampler.Run(shouldStop, Sub(s)
                                    Console.WriteLine($"{s.Time:HH:mm:ss} GPU={FmtPercent(s.GpuPercent)} 显存控={FmtPercent(s.MemoryControllerPercent)} 显存={FmtMB(s.UsedMB)}/{FmtMB(s.TotalMB)}MB PCIe TX={FmtDouble(s.PcieTxMBps)} RX={FmtDouble(s.PcieRxMBps)} 功耗={FmtWatts(s.PowerWatts)}W 温度={FmtTemp(s.TemperatureC)}")

                                    If smMonitor IsNot Nothing Then
                                        Console.WriteLine($"{s.Time:HH:mm:ss} {BuildSmSummaryLine(smMonitor.Refresh())}")
                                    End If

                                    If processMonitor IsNot Nothing Then
                                        processMonitor.Update()
                                        For Each p In processMonitor.Rows()
                                            Console.WriteLine($"{s.Time:HH:mm:ss} PID={p.Pid,7} CUDA={FmtPercent(p.Cuda)} Copy={FmtPercent(p.Copy)} 编码={FmtPercent(p.Enc)} 解码={FmtPercent(p.Dec)} 显存={StringFormats.Lanudry(p.MemMB * ByteSize.MB)} {Sparkline.Render(p.History, 20)} {PadName(p.Name)}")
                                        Next
                                    End If
                                End Sub)
    End Sub

    ''' <summary>无颜色模式下的单行 SM 归因摘要。</summary>
    Private Function BuildSmSummaryLine(snapshot As SmSnapshot) As String
        If Not snapshot.Capability.Available Then
            Return $"SM归因 CUPTI不可用: {snapshot.Reason}"
        End If

        Dim aggregate = snapshot.Aggregate
        Dim balance = aggregate.Balance

        If snapshot.PerSmAvailable Then
            Return $"SM归因 整卡={FmtPercentD(aggregate.UtilizationPercent)} warps={FmtPercentD(aggregate.WarpsActivePercent)} " &
                   $"切片={snapshot.SampleCount} 空转={snapshot.IdleCount}/{snapshot.SmCount} " &
                   $"最热={FormatSmList(snapshot.TopHot(3))} 最冷={FormatSmList(snapshot.TopCold(3))}"
        End If

        Dim head = $"SM归因 整卡={FmtPercentD(aggregate.UtilizationPercent)} warps={FmtPercentD(aggregate.WarpsActivePercent)} " &
                   $"发射={FmtPercentD(aggregate.ThroughputPercent)} 切片={snapshot.SampleCount}"

        If Not balance.IsEstimated Then
            Return head & " 均衡推断: 指标不足（缺少 sm__cycles_active.max/.min） [per-SM 明细不可用]"
        End If

        If balance.AveragePercent >= 0 AndAlso balance.AveragePercent < 1.0 Then
            Return head & " 均衡推断: GPU 基本空闲（平均活跃 < 1%），失衡推断无统计意义 [per-SM 明细不可用]"
        End If

        Return head &
               $" 平均活跃SM={balance.AverageActiveSm:F1}/{balance.SmCount} 推断空转={balance.IdleSmEstimate} " &
               $"失衡比={balance.ImbalanceRatio:P0} 最热/最冷={balance.HotSmPercent:F1}%/{balance.ColdSmPercent:F1}% " &
               $"[per-SM 明细不可用：CUDA 13.x 已移除 instance 接口]"
    End Function

    Private Sub RunLiveColor(sampler As MonitorSampler, device As NvmlDevice, shouldStop As Func(Of Boolean),
                             processMonitor As ProcessMonitor, hardware As CudaDevice, processRows As Integer,
                             Optional smMonitor As CuptiMonitor = Nothing, Optional smRows As Integer = 8)
        Dim frameLines As Integer = 0
        ' 进程块固定高度，避免样本数波动导致原地刷新残影
        Dim procRows = If(processRows > 0, System.Math.Min(processRows, 15), 0)
        ' SM 归因块固定高度（4 行概览 + N 行明细），避免帧高抖动
        Dim smRowCount = System.Math.Max(4, System.Math.Min(If(smRows > 0, smRows, 8), 16))

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

                                    ' SM 归因（CUPTI PM Sampling 切片 / per-SM 明细 / SM 均衡推断）
                                    If smMonitor IsNot Nothing Then
                                        lines.AddRange(BuildSmPanel(smMonitor.Refresh(), True, smRowCount))
                                    End If

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

    ' ==================================================================
    ' CUPTI per-SM 归因（--sm / --sm --top / --sm-doctor）
    ' ==================================================================

    Private Function FmtPercentD(v As Double) As String
        If v < 0 Then Return "N/A"
        Return v.ToString("F1", CultureInfo.InvariantCulture) & "%"
    End Function

    Private Function FmtMs(v As Double) As String
        If v <= 0 Then Return "N/A"
        Return v.ToString("F1", CultureInfo.InvariantCulture) & " ms"
    End Function

    ''' <summary>表格里用的迷你条形（10 格）。</summary>
    Private Function MiniBar(percent As Double, Optional width As Integer = 12) As String
        If percent < 0 Then Return New String("-"c, width)

        Dim filled = CInt(System.Math.Round(percent / 100.0 * width))
        If filled < 0 Then filled = 0
        If filled > width Then filled = width

        Return New String("#"c, filled) & New String("-"c, width - filled)
    End Function

    ''' <summary>按利用率给出状态标记：IDLE(&lt;阈值) / HOT(&gt;=80%) / BUSY。</summary>
    Private Function SmState(percent As Double, Optional idleThreshold As Double = 5.0) As String
        If percent < 0 Then Return "N/A"
        If percent < idleThreshold Then Return "IDLE"
        If percent >= 80 Then Return "HOT"
        Return "BUSY"
    End Function

    ''' <summary>
    ''' 能力与配置诊断块（--sm 一次性模式与 --sm-doctor 共用）。
    ''' </summary>
    Public Sub PrintSmCapability(snapshot As SmSnapshot)
        Dim cap = snapshot.Capability
        Dim rows As New List(Of Object())()

        rows.Add({"设备", $"{If(String.IsNullOrEmpty(snapshot.DeviceName), "N/A", snapshot.DeviceName)}  [{snapshot.DeviceIndex}]"})
        rows.Add({"SM 数量", If(snapshot.SmCount > 0, snapshot.SmCount.ToString(CultureInfo.InvariantCulture), "未知")})
        rows.Add({"CUPTI 版本", If(String.IsNullOrEmpty(cap.CuptiVersion), "N/A", cap.CuptiVersion)})
        rows.Add({"CUPTI 路径", If(String.IsNullOrEmpty(cap.CuptiPath), "N/A", cap.CuptiPath)})
        rows.Add({"芯片", If(String.IsNullOrEmpty(cap.ChipName), "N/A", cap.ChipName)})

        If Not String.IsNullOrEmpty(cap.DeviceSupported) Then
            rows.Add({"设备支持级别", cap.DeviceSupported})
        End If
        rows.Add({"采样会话", If(cap.Available, "已启用（PM Sampling）", "不可用")})
        rows.Add({"采样间隔", $"{cap.SamplingIntervalMs:F1} ms" & If(cap.UsingSysClockTrigger, "（SYSCLK 触发）", "（纳秒触发）")})
        rows.Add({"单 pass 上限", If(cap.SinglePassLimit > 0, $"{cap.SinglePassLimit} 项指标", "未知")})
        rows.Add({"采集指标", If(cap.Metrics Is Nothing OrElse cap.Metrics.Length = 0, "N/A", String.Join(", ", cap.Metrics))})

        If cap.SkippedMetrics IsNot Nothing AndAlso cap.SkippedMetrics.Length > 0 Then
            rows.Add({"已裁剪指标", String.Join(", ", cap.SkippedMetrics) & "（超出单 pass 容量）"})
        End If

        rows.Add({"per-SM 明细", If(snapshot.PerSmAvailable,
                                    "可用（旧 Event API，per-instance 计数）",
                                    "不可用" & If(String.IsNullOrEmpty(snapshot.Reason), "", "：" & snapshot.Reason))})

        ConsoleTableBuilder _
            .From(rows) _
            .WithColumn("项目", "取值") _
            .WithTitle("CUPTI 能力诊断", ConsoleColor.Cyan) _
            .ExportAndWriteLine(TableAligntment.Left)
    End Sub

    ''' <summary>
    ''' 一次性模式的完整报告：能力诊断块 → 概览表 → （per-SM 表 或 PM 切片表）→ 排名摘要。
    ''' </summary>
    Public Sub PrintSmSummary(snapshot As SmSnapshot, Optional topN As Integer = 3,
                              Optional useColor As Boolean = True, Optional maxRows As Integer = 0)
        PrintSmCapability(snapshot)

        If Not snapshot.Capability.Available Then
            Console.WriteLine()
            Console.Error.WriteLine("错误：CUPTI PM Sampling 会话不可用，无法采集 SM 指标。")
            If Not String.IsNullOrEmpty(snapshot.Reason) Then Console.Error.WriteLine($"原因：{snapshot.Reason}")
            Return
        End If

        Dim aggregate = snapshot.Aggregate
        Dim balance = aggregate.Balance
        Dim overview As New List(Of Object())()

        overview.Add({"采集时长", $"{snapshot.CaptureSeconds:F2} s"})
        overview.Add({"有效切片", snapshot.SampleCount.ToString(CultureInfo.InvariantCulture)})
        overview.Add({"平均 SM 利用率", FmtPercentD(aggregate.UtilizationPercent)})
        overview.Add({"活跃 warp 占峰值", FmtPercentD(aggregate.WarpsActivePercent)})
        overview.Add({"SM 发射利用率（吞吐代理）", FmtPercentD(aggregate.ThroughputPercent)})
        overview.Add({"最热 SM 活跃度", FmtPercentD(balance.HotSmPercent)})
        overview.Add({"最冷 SM 活跃度", FmtPercentD(balance.ColdSmPercent)})

        If snapshot.PerSmAvailable Then
            overview.Add({"空转 SM 数", $"{snapshot.IdleCount} / {snapshot.SmCount}（利用率 < 5%）"})
        ElseIf balance.IdleSmEstimate >= 0 Then
            overview.Add({"推断空转 SM 数", $"{balance.IdleSmEstimate} / {balance.SmCount}（两极估计）"})
        End If

        If balance.ImbalanceRatio >= 0 Then
            overview.Add({"SM 失衡比", balance.ImbalanceRatio.ToString("P1", CultureInfo.InvariantCulture) & "（(max-min)/max）"})
        End If

        If snapshot.Overflow Then
            overview.Add({"硬件缓冲", "发生溢出：请增大 --sm-hwbuffer 或采样间隔"})
        End If

        Console.WriteLine()
        ConsoleTableBuilder _
            .From(overview) _
            .WithColumn("项目", "取值") _
            .WithTitle("SM 归因概览", ConsoleColor.Cyan) _
            .ExportAndWriteLine(TableAligntment.Left)

        If snapshot.PerSmAvailable Then
            PrintPerSmTable(snapshot, useColor, maxRows)
            PrintPerSmRanking(snapshot, topN)
        Else
            PrintSliceTable(snapshot, useColor)
            PrintBalanceFallback(snapshot)
        End If
    End Sub

    ''' <summary>真实的逐 SM 表（旧 Event API 可用时）。</summary>
    Private Sub PrintPerSmTable(snapshot As SmSnapshot, useColor As Boolean, maxRows As Integer)
        Dim rows = snapshot.Rows
        Dim count = If(maxRows > 0, System.Math.Min(maxRows, rows.Count), rows.Count)
        Dim table As New List(Of Object())()

        For i As Integer = 0 To count - 1
            Dim r = rows(i)
            Dim bar = MiniBar(r.UtilizationPercent)
            If useColor Then bar = Colorize(bar, CInt(System.Math.Round(r.UtilizationPercent)))

            table.Add({
                r.SmIndex,
                bar,
                FmtPercentD(r.UtilizationPercent),
                FmtPercentD(r.WarpsActivePercent),
                FmtPercentD(r.ThroughputPercent),
                SmState(r.UtilizationPercent)
            })
        Next

        Console.WriteLine()
        ConsoleTableBuilder _
            .From(table) _
            .WithColumn("SM", "利用率条形", "利用率", "warps%", "吞吐%", "状态") _
            .WithTitle($"per-SM 利用率（共 {rows.Count} 个 SM，显示 {count} 个）", ConsoleColor.Cyan) _
            .ExportAndWriteLine(TableAligntment.Left)
    End Sub

    ''' <summary>per-SM 排名摘要：最热 / 最冷 / 空转统计。</summary>
    Private Sub PrintPerSmRanking(snapshot As SmSnapshot, topN As Integer)
        Console.WriteLine()
        Console.WriteLine($"  最热 {topN} 个 SM：" & FormatSmList(snapshot.TopHot(topN)))
        Console.WriteLine($"  最冷 {topN} 个 SM：" & FormatSmList(snapshot.TopCold(topN)))
        Console.WriteLine($"  空转 SM（< 5%）：{snapshot.IdleCount} / {snapshot.SmCount}")
    End Sub

    Private Function FormatSmList(rows As IReadOnlyList(Of SmRow)) As String
        If rows Is Nothing OrElse rows.Count = 0 Then Return "N/A"

        Return String.Join("   ", rows.Select(Function(r) $"SM{r.SmIndex}={r.UtilizationPercent:F1}%"))
    End Function

    ''' <summary>PM 采样切片表（降级路径：只有整卡聚合值）。</summary>
    Private Sub PrintSliceTable(snapshot As SmSnapshot, useColor As Boolean)
        Dim slices = snapshot.Slices
        If slices Is Nothing OrElse slices.Count = 0 Then
            Console.WriteLine()

            If Not String.IsNullOrEmpty(snapshot.SamplingError) Then
                Console.Error.WriteLine("  警告：CUPTI 会话已配置成功，但采样未能启动/解码。")
                Console.Error.WriteLine($"  原因：{snapshot.SamplingError}")
                Console.Error.WriteLine("  可尝试：①用 --sm-doctor 查看完整诊断；②以管理员身份运行；")
                Console.Error.WriteLine("          ③关闭 Nsight Compute / 其它 profiler（同一设备只允许一个采样会话）；")
                Console.Error.WriteLine("          ④换用 --sm-sysclk（SYSCLK 触发）；⑤更新或回退 NVIDIA 驱动。")
            Else
                Console.WriteLine("  没有采集到有效的采样切片（GPU 在采样窗口内可能完全空闲，或采样间隔过小）。")
                If Not String.IsNullOrEmpty(snapshot.Reason) Then Console.WriteLine($"  原因：{snapshot.Reason}")
            End If

            Return
        End If

        ' 只展示最近 20 条，避免刷屏
        Const showMax As Integer = 20
        Dim startIndex = System.Math.Max(0, slices.Count - showMax)
        Dim table As New List(Of Object())()

        For i As Integer = startIndex To slices.Count - 1
            Dim item = slices(i)
            Dim bar = MiniBar(item.UtilizationPercent)
            If useColor Then bar = Colorize(bar, CInt(System.Math.Round(item.UtilizationPercent)))

            table.Add({
                item.Index,
                FmtMs(item.DurationMs),
                bar,
                FmtPercentD(item.UtilizationPercent),
                FmtPercentD(item.WarpsActivePercent),
                FmtPercentD(item.ThroughputPercent)
            })
        Next

        Console.WriteLine()
        ConsoleTableBuilder _
            .From(table) _
            .WithColumn("#", "窗口", "利用率条形", "利用率", "warps%", "吞吐%") _
            .WithTitle($"PM 采样切片（最近 {table.Count} / 共 {slices.Count} 条）", ConsoleColor.Cyan) _
            .ExportAndWriteLine(TableAligntment.Left)
    End Sub

    ''' <summary>per-SM 不可用时的说明 + 整卡层面的均衡推断。</summary>
    Private Sub PrintBalanceFallback(snapshot As SmSnapshot)
        Dim balance = snapshot.Aggregate.Balance

        Console.WriteLine()
        Console.WriteLine("  说明：本机 CUPTI 无法提供逐 SM（instance）计数 —— NVIDIA 自 CUDA 12.8 弃用、")
        Console.WriteLine("        CUDA 13.0 起移除了旧 Event/Metric API（错误码 38，LEGACY_PROFILER_NOT_SUPPORTED），")
        Console.WriteLine("        新版 PM Sampling 只输出整卡聚合值。以下为整卡层面的均衡推断：")
        Console.WriteLine()

        If balance.IsEstimated Then
            Console.WriteLine($"  平均活跃 SM 数 : {balance.AverageActiveSm:F1} / {balance.SmCount}")
            Console.WriteLine($"  最热/最冷 SM   : {balance.HotSmPercent:F1}% / {balance.ColdSmPercent:F1}%")
            Console.WriteLine($"  推断空转 SM 数 : {balance.IdleSmEstimate} / {balance.SmCount}（两极模型估计）")
            Console.WriteLine($"  SM 失衡比      : {balance.ImbalanceRatio:P1}（越接近 0 越均衡）")

            If balance.AveragePercent >= 0 AndAlso balance.AveragePercent < 1.0 Then
                Console.WriteLine("  判读：采样窗口内 GPU 基本空闲（平均活跃 < 1%），SM 失衡推断无统计意义。")
            ElseIf balance.ImbalanceRatio >= 0.7 Then
                Console.WriteLine("  判读：推断负载明显集中在少数 SM 上（两极模型），可能存在并行度不足或 tail effect。")
            ElseIf balance.ImbalanceRatio >= 0.4 Then
                Console.WriteLine("  判读：推断存在中等程度失衡（两极模型），可检查网格划分与 block 调度是否均匀。")
            Else
                Console.WriteLine("  判读：推断 SM 之间基本均衡（两极模型），未观察到明显的空转 SM。")
            End If
        Else
            Console.WriteLine("  当前采集到的指标不足以推断 SM 均衡（缺少 sm__cycles_active.max/.min）。")
            Console.WriteLine("  可在具备单 pass 容量的设备上重试，或用 --sm-metrics 显式指定指标。")
        End If
    End Sub

    ''' <summary>
    ''' 实时模式：固定行数原地刷新（--sm --top）。
    ''' </summary>
    Public Sub RunSmLive(monitor As CuptiMonitor, useColor As Boolean, shouldStop As Func(Of Boolean),
                         intervalMs As Integer, Optional maxRows As Integer = 16)
        Dim frameLines As Integer = 0
        Dim rowCount = System.Math.Max(4, System.Math.Min(If(maxRows > 0, maxRows, 16), 32))
        Static gray As New ConsoleFormat(AnsiColor.FromRgbColor(Color.DimGray), AnsiColor.Black, Underline:=True)

        While Not shouldStop()
            Dim snapshot = monitor.Refresh()
            Dim lines = BuildSmLines(snapshot, useColor, rowCount)
            lines.Add("")
            lines.Add(New TextSpan("  按 Ctrl+C 停止", gray))

            Dim sb As New StringBuilder()

            If useColor Then
                ' 覆盖式刷新：光标上移 + 清行
                If frameLines > 0 Then sb.Append(AnsiEscapeCodes.GetMoveCursorUp(frameLines))

                For i As Integer = 0 To lines.Count - 1
                    sb.Append(AnsiEscapeCodes.ClearLine)
                    sb.Append(lines(i))
                    sb.Append(vbCrLf)
                Next

                frameLines = lines.Count
            Else
                ' --plain：逐帧追加，不做光标操作（便于管道/日志）
                For i As Integer = 0 To lines.Count - 1
                    sb.Append(lines(i))
                    sb.Append(vbCrLf)
                Next

                sb.Append(vbCrLf)
            End If

            Console.Out.Write(sb.ToString())
            Console.Out.Flush()

            Dim waitUntil = DateTime.Now.AddMilliseconds(System.Math.Max(200, intervalMs))
            While Not shouldStop() AndAlso DateTime.Now < waitUntil
                Threading.Thread.Sleep(50)
            End While
        End While

        Console.Out.Write(vbCrLf)
        Console.Out.Flush()
    End Sub

    ''' <summary>实时模式的一帧（固定行数，避免残影）。</summary>
    Private Function BuildSmLines(snapshot As SmSnapshot, useColor As Boolean, rowCount As Integer) As List(Of String)
        Dim L As New List(Of String)()
        L.Add($"GTop — {If(String.IsNullOrEmpty(snapshot.DeviceName), "GPU", snapshot.DeviceName)}  [{snapshot.DeviceIndex}]   SM 归因 (CUPTI)   {DateTime.Now:HH:mm:ss}")
        L.AddRange(BuildSmPanel(snapshot, useColor, rowCount))
        Return L
    End Function

    ''' <summary>
    ''' SM 归因区块（固定高度 = 4 + rowCount 行，供实时仪表盘原地刷新用）。
    '''
    ''' 行布局：
    '''   第 0 行 整卡 SM 利用率条形
    '''   第 1 行 活跃 warp / 发射槽 / 切片数 / 芯片
    '''   第 2 行 per-SM 可用性、或整卡层面的均衡推断（平均活跃 SM、推断空转、失衡比、最热/最冷）
    '''   第 3 行 per-SM 排名（最热/最冷 Top-3 + 空转计数）或判读结论
    '''   其余    逐 SM 明细（SM/条形/利用率/warps%/吞吐%/IDLE|HOT），降级时为 PM 切片
    ''' </summary>
    Public Function BuildSmPanel(snapshot As SmSnapshot, useColor As Boolean, rowCount As Integer) As List(Of String)
        Dim L As New List(Of String)()
        Dim cap = snapshot.Capability
        Dim aggregate = snapshot.Aggregate
        Dim balance = aggregate.Balance
        Dim rows = System.Math.Max(1, rowCount)

        If Not cap.Available Then
            L.Add("  整卡 SM  : CUPTI PM Sampling 不可用")
            L.Add($"  原因     : {TruncateByWidth(snapshot.Reason, System.Math.Max(30, GetConsoleWidth() - 14))}")
            L.Add("  per-SM   : 无硬件计数器数据")
            L.Add(String.Empty)

            For i As Integer = 0 To rows - 1
                L.Add(String.Empty)
            Next

            Return L
        End If

        ' 第 0 行：整卡利用率
        L.Add(Bar("整卡", CInt(System.Math.Round(System.Math.Max(0, aggregate.UtilizationPercent))), useColor))

        ' 第 1 行：活跃 warp / 发射槽 / 切片 / 芯片
        L.Add($"  活跃warp : {FmtPercentD(aggregate.WarpsActivePercent)}    发射槽 : {FmtPercentD(aggregate.ThroughputPercent)}    切片 : {snapshot.SampleCount}    芯片 : {If(String.IsNullOrEmpty(cap.ChipName), "N/A", cap.ChipName)}")

        ' 第 2 行：per-SM 可用性 / 均衡推断
        If snapshot.PerSmAvailable Then
            L.Add($"  per-SM   : [可用] 共 {snapshot.Rows.Count} 个 SM，空转(<5%) {snapshot.IdleCount} 个")
        ElseIf balance.IsEstimated Then
            L.Add($"  均衡推断 : 平均活跃SM {balance.AverageActiveSm:F1}/{balance.SmCount}   推断空转 {balance.IdleSmEstimate}   失衡比 {balance.ImbalanceRatio:P0}   最热/最冷 {balance.HotSmPercent:F1}%/{balance.ColdSmPercent:F1}%")
        ElseIf snapshot.SampleCount = 0 AndAlso Not String.IsNullOrEmpty(snapshot.SamplingError) Then
            L.Add($"  采样异常 : {TruncateByWidth(snapshot.SamplingError, System.Math.Max(30, GetConsoleWidth() - 14))}")
        Else
            L.Add("  均衡推断 : 指标不足（缺少 sm__cycles_active.max/.min）")
        End If

        ' 第 3 行：排名摘要 / 判读
        If snapshot.PerSmAvailable Then
            L.Add($"  最热/最冷 : {FormatSmList(snapshot.TopHot(3))}   |   {FormatSmList(snapshot.TopCold(3))}")
        ElseIf balance.IsEstimated Then
            If balance.AveragePercent >= 0 AndAlso balance.AveragePercent < 1.0 Then
                ' 极低活跃度下 max/min 都接近 0，失衡比会被数值噪声放大，不做结论
                L.Add("  判读     : 采样窗口内 GPU 基本空闲（平均活跃 < 1%），失衡推断无统计意义")
            ElseIf balance.ImbalanceRatio >= 0.7 Then
                L.Add("  判读     : 推断负载明显集中在少数 SM（两极模型），可能有并行度不足 / tail effect")
            ElseIf balance.ImbalanceRatio >= 0.4 Then
                L.Add("  判读     : 推断存在中等程度失衡（两极模型），可检查网格划分与 block 调度")
            Else
                L.Add("  判读     : 推断 SM 之间基本均衡（两极模型），未观察到明显空转")
            End If
        Else
            L.Add($"  per-SM   : [不可用] CUPTI 已移除 per-instance 接口（CUDA 13.x / 错误码 38）")
        End If

        ' 其余行：逐 SM 明细 / PM 切片（固定行数，避免残影）
        Dim shown As Integer = 0

        If snapshot.PerSmAvailable Then
            For i As Integer = 0 To snapshot.Rows.Count - 1
                If shown >= rows Then Exit For

                Dim r = snapshot.Rows(i)
                Dim percent = CInt(System.Math.Round(System.Math.Max(0, r.UtilizationPercent)))

                L.Add($"  SM{r.SmIndex,-3} : {BarInline(percent, useColor)} {percent,3}%   warps {FmtPercentD(r.WarpsActivePercent),7}   吞吐 {FmtPercentD(r.ThroughputPercent),7}   {SmState(r.UtilizationPercent)}")
                shown += 1
            Next
        Else
            Dim slices = snapshot.Slices

            For i As Integer = System.Math.Max(0, slices.Count - rows) To slices.Count - 1
                Dim item = slices(i)
                Dim percent = CInt(System.Math.Round(System.Math.Max(0, item.UtilizationPercent)))

                L.Add($"  #{item.Index,-4} : {BarInline(percent, useColor)} {percent,3}%   {FmtMs(item.DurationMs)}   warps {FmtPercentD(item.WarpsActivePercent)}   吞吐 {FmtPercentD(item.ThroughputPercent)}")
                shown += 1
            Next
        End If

        For i As Integer = shown To rows - 1
            L.Add(String.Empty)
        Next

        Return L
    End Function

    ''' <summary>行内条形（不含标签，供实时模式逐 SM 使用）。</summary>
    Private Function BarInline(percent As Integer, useColor As Boolean) As String
        Const width As Integer = 24
        Dim filled = CInt(System.Math.Round(System.Math.Max(0, System.Math.Min(100, percent)) / 100.0 * width))
        Dim barText = New String("#"c, filled) & New String("-"c, width - filled)

        If Not useColor Then Return $"[{barText}]"

        Dim color As AnsiColor
        If percent < 50 Then
            color = AnsiColor.BrightGreen
        ElseIf percent < 80 Then
            color = AnsiColor.BrightYellow
        Else
            color = AnsiColor.BrightRed
        End If

        Return $"[{color.GetEscapeSequence()}{barText}{AnsiEscapeCodes.Reset}]"
    End Function
End Module
