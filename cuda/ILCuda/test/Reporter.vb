#Region "Microsoft.VisualBasic::5087dfdb36b02609dcc18d7e5fd242e9, cuda\ILCuda\test\Reporter.vb"

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

    '   Total Lines: 166
    '    Code Lines: 115 (69.28%)
    ' Comment Lines: 15 (9.04%)
    '    - Xml Docs: 20.00%
    ' 
    '   Blank Lines: 36 (21.69%)
    '     File Size: 7.25 KB


    '     Module ConsoleReporter
    ' 
    '         Sub: PrintDevice, PrintDeviceReport, PrintDiagnostics, PrintEnvironment, PrintFixSuggestion
    '              PrintMatrix, PrintSection, PrintTiming, PrintTitle
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' ------------------------------------------------------------------------
' demo 的控制台输出层
'
' 框架（Enigma.ILCuda）只提供纯数据的诊断对象：DeviceReport、
' EnvironmentReport、FixSuggestion；怎么排版、往哪儿输出，由这里决定。
' ------------------------------------------------------------------------

Imports ILCudaDemo.Metrics
Imports Microsoft.VisualBasic.Computing.ILCuda.Runtime

Namespace Diagnostics

    Public Module ConsoleReporter

        Public Sub PrintTitle(text As String)
            Console.WriteLine()
            Console.WriteLine(New String("="c, 74))
            Console.WriteLine($"  {text}")
            Console.WriteLine(New String("="c, 74))
        End Sub

        Public Sub PrintSection(text As String)
            Console.WriteLine()
            Console.WriteLine($"---- {text} ----")
        End Sub

        ' ------------------------------------------------------------------
        ' 设备 / 环境
        ' ------------------------------------------------------------------

        Public Sub PrintDevice(device As CudaDevice)
            PrintDeviceReport(DeviceReport.From(device))
        End Sub

        Public Sub PrintDeviceReport(device As DeviceReport)
            Console.WriteLine($"  设备       : [{device.Ordinal}] {device.Name}")
            Console.WriteLine($"  计算能力   : {device.ComputeCapability}  (NVRTC -arch={device.Arch} / {device.SmArch})")
            Console.WriteLine($"  显存       : {device.TotalMemoryMB:N0} MB")
            Console.WriteLine($"  SM 数量    : {device.MultiprocessorCount}")
            Console.WriteLine($"  最大线程   : {device.MaxThreadsPerBlock} / block, {device.MaxThreadsPerMultiprocessor} / SM")
            Console.WriteLine($"  共享内存   : {device.MaxSharedMemoryPerBlockKB:N0} KB / block")
            Console.WriteLine($"  核心频率   : {device.ClockRateMHz:N0} MHz")
            Console.WriteLine($"  显存位宽   : {device.GlobalMemoryBusWidth} bit @ {device.MemoryClockRateMHz:N0} MHz")
            Console.WriteLine($"  L2 缓存    : {device.L2CacheSizeKB:N0} KB")
            Console.WriteLine($"  Warp 大小  : {device.WarpSize}")
        End Sub

        Public Sub PrintEnvironment(report As EnvironmentReport)
            Console.WriteLine($"  驱动支持的 CUDA 版本: {report.DriverVersionText}")
            Console.WriteLine($"  检测到 {report.DeviceCount} 块 CUDA 设备")

            For Each device In report.Devices
                Console.WriteLine()
                PrintDeviceReport(device)
            Next

            Console.WriteLine()
            Console.WriteLine($"  NVRTC 候选（按优先级排序，共 {report.NvrtcCandidates.Count} 个）：")

            For Each candidate In report.NvrtcCandidates
                Console.WriteLine($"    {candidate}")
            Next

            Console.WriteLine()
            Console.WriteLine($"  参与编译的内核源码（内置 + 已注册，共 {report.KernelSources.Count} 个）：")

            For Each Source As String In report.KernelSources
                Console.WriteLine($"    {Source}")
            Next
        End Sub

        Public Sub PrintDiagnostics(diagnostics As IReadOnlyList(Of String))
            If diagnostics Is Nothing OrElse diagnostics.Count = 0 Then Return

            Console.WriteLine()
            Console.WriteLine("  诊断过程：")

            For Each line In diagnostics
                Console.WriteLine($"    {line}")
            Next
        End Sub

        ''' <summary>按框架给出的结构化建议逐条打印修复办法</summary>
        Public Sub PrintFixSuggestion(report As EnvironmentReport)
            Dim suggestions = CudaEnvironment.Suggest(report)

            If suggestions.Count = 0 Then Return

            Console.WriteLine()
            Console.WriteLine("  修复建议：")

            For Each item In suggestions
                Console.WriteLine($"    - [{item.Kind}] {item.Title}")
                If Not String.IsNullOrWhiteSpace(item.Detail) Then
                    Console.WriteLine($"      {item.Detail}")
                End If

                For Each stepText In item.Commands
                    Console.WriteLine($"        > {stepText}")
                Next
            Next

            Console.WriteLine()
            Console.WriteLine("    注意：预编译镜像同样要求『工具包版本 <= 驱动支持的 CUDA 版本』，")
            Console.WriteLine("          否则驱动会在 cuModuleLoadData 内部崩溃，程序会主动跳过这类镜像。")
        End Sub

        ' ------------------------------------------------------------------
        ' 结果
        ' ------------------------------------------------------------------

        ''' <summary>打印结果矩阵的左上角预览</summary>
        Public Sub PrintMatrix(title As String, matrix As MetricResult, Optional preview As Integer = 6, Optional rowNames As String() = Nothing)
            Console.WriteLine()
            Console.WriteLine($"  {title}")

            Dim n = System.Math.Min(preview, matrix.Size)
            Dim header As String = "        "

            For j As Integer = 0 To n - 1
                header &= $"{j,10}"
            Next
            Console.WriteLine(header)

            For i As Integer = 0 To n - 1
                Dim label = If(rowNames IsNot Nothing AndAlso i < rowNames.Length, rowNames(i), $"row{i}")
                If label.Length > 8 Then label = label.Substring(0, 8)

                Dim line As String = $"  {label,-6}"

                For j As Integer = 0 To n - 1
                    line &= $"{matrix(i, j),10:F4}"
                Next

                Console.WriteLine(line)
            Next

            If matrix.Size > preview Then
                Console.WriteLine($"  ...（矩阵实际为 {matrix.Size} x {matrix.Size}，仅预览左上角 {preview} x {preview}）")
            End If
        End Sub

        ''' <summary>打印 GPU / CPU 的耗时对比</summary>
        Public Sub PrintTiming(gpu As MatrixMetricsResult, cpu As MatrixMetricsResult)
            Console.WriteLine()
            Console.WriteLine("  耗时对比：")

            If gpu IsNot Nothing Then
                Console.WriteLine($"    GPU 内核      : {gpu.KernelMs,10:F3} ms")
                Console.WriteLine($"    GPU 数据拷贝  : {gpu.CopyMs,10:F3} ms")
                Console.WriteLine($"    GPU 合计      : {gpu.TotalMs,10:F3} ms")
            End If

            Console.WriteLine($"    CPU 参考实现  : {cpu.KernelMs,10:F3} ms")

            If gpu IsNot Nothing Then
                If gpu.TotalMs > 0 Then
                    Console.WriteLine($"    加速比(合计)  : {cpu.KernelMs / gpu.TotalMs,10:F2} x")
                End If
                If gpu.KernelMs > 0 Then
                    Console.WriteLine($"    加速比(仅内核): {cpu.KernelMs / gpu.KernelMs,10:F2} x")
                End If
            End If
        End Sub
    End Module
End Namespace

