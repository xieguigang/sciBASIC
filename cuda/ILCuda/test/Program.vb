#Region "Microsoft.VisualBasic::75ccb44a5a0207c6e11fa938fb5098d2, cuda\ILCuda\test\Program.vb"

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

    '   Total Lines: 523
    '    Code Lines: 384 (73.42%)
    ' Comment Lines: 53 (10.13%)
    '    - Xml Docs: 22.64%
    ' 
    '   Blank Lines: 86 (16.44%)
    '     File Size: 23.23 KB


    ' Module Program
    ' 
    '     Function: Main, ParseCommandLine, RunDemo, RunEmitKernels, RunIlCompare
    '               RunIlDecompile, RunInfo, RunListKernels, RunSelfTest
    ' 
    '     Sub: PrintHelp, RunFrameworkKernelSmokeTest
    '     Class CliOptions
    ' 
    '         Properties: Cols, Command, CpuOnly, CsvPath, DeviceOrdinal
    '                     DumpIl, Elements, ForceImage, ImagePath, NvrtcPath
    '                     Preview, Rows, Seed, ShowAst, ShowHelp
    '                     ShowSource, UseAsyncPipeline
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' ------------------------------------------------------------------------
' ILCuda demo 命令行入口（皮尔逊相关系数矩阵 + 欧氏距离矩阵）
'
'   ILCuda.Demo info                    列出系统中的 NVIDIA GPU 与 NVRTC 候选
'   ILCuda.Demo demo                    基础数学加速 + 行间相似度矩阵演示
'   ILCuda.Demo selftest                用 CPU 模拟内核索引与公式，做正确性自检
'   ILCuda.Demo kernels                 列出内核注册表中的全部内核
'   ILCuda.Demo emit-kernels <dir>      导出参与编译的 CUDA 内核源码
'   ILCuda.Demo help                    显示帮助
'
' 常用参数：
'   --rows <n>      合成矩阵的行数（默认 512）
'   --cols <n>      合成矩阵的列数（默认 256）
'   --seed <n>      随机数种子（默认 42）
'   --csv <file>    从 CSV/TSV 文件载入矩阵
'   --device <n>    使用第几块 GPU（默认 0）
'   --nvrtc <dll>   显式指定 NVRTC 动态库
'   --cubin <file>  显式指定预编译的 .ptx/.cubin 镜像
'   --elements <n>  基础数学示例的向量规模（默认 1048576）
'   --preview <n>   结果矩阵预览大小（默认 6）
'   --cpu-only      只跑 CPU 参考实现
'   --async         用页锁定内存 + 独立流跑一遍异步流水线
' ------------------------------------------------------------------------

Imports System.IO
Imports ILCudaDemo.Diagnostics
Imports ILCudaDemo.IlDecompile
Imports ILCudaDemo.Metrics
Imports Microsoft.VisualBasic.Computing.ILCuda.Kernels
Imports Microsoft.VisualBasic.Computing.ILCuda.Math
Imports Microsoft.VisualBasic.Computing.ILCuda.Runtime

Module Program

    Private Class CliOptions
        Public Property Command As String = "demo"
        Public Property Rows As Integer = 512
        Public Property Cols As Integer = 256
        Public Property Seed As Integer = 42
        Public Property CsvPath As String
        Public Property NvrtcPath As String
        Public Property ImagePath As String
        Public Property CpuOnly As Boolean = False
        Public Property ForceImage As Boolean = False
        Public Property UseAsyncPipeline As Boolean = False
        Public Property DeviceOrdinal As Integer = 0
        Public Property Elements As Integer = 1 << 20
        Public Property Preview As Integer = 6
        Public Property ShowHelp As Boolean = False
        ''' <summary>il 命令是否打印还原出的伪代码</summary>
        Public Property ShowAst As Boolean = True
        ''' <summary>il 命令是否打印生成的 CUDA 源码</summary>
        Public Property ShowSource As Boolean = True
        ''' <summary>il 命令是否打印原始 IL 与基本块结构</summary>
        Public Property DumpIl As Boolean = False
    End Class

    Function Main(args As String()) As Integer
        ' demo 自带的内核源码（Kernels\metrics.cu）必须先注册进框架，
        ' 否则 NVRTC 编译时不会包含 rowStats / gram / finalize 三个内核。
        KernelSources.Register(GetType(Program).Assembly)
        GpuMetrics.RegisterKernels()

        Dim options As CliOptions

        Try
            options = ParseCommandLine(args)
        Catch ex As Exception
            Console.WriteLine($"参数解析失败: {ex.Message}")
            PrintHelp()
            Return 2
        End Try

        If options.ShowHelp OrElse options.Command = "help" Then
            PrintHelp()
            Return 0
        End If

        Select Case options.Command
            Case "info"
                Return RunInfo(options)
            Case "emit-kernels"
                Return RunEmitKernels(options)
            Case "selftest"
                Return RunSelfTest(options)
            Case "kernels"
                Return RunListKernels()
            Case "il"
                Return RunIlDecompile(options)
            Case "il-compare"
                Return RunIlCompare(options)
            Case "demo"
                Return RunDemo(options)
            Case Else
                Console.WriteLine($"未知命令: {options.Command}")
                PrintHelp()
                Return 2
        End Select
    End Function

    ' ------------------------------------------------------------------
    ' 命令实现
    ' ------------------------------------------------------------------

    Private Function RunInfo(options As CliOptions) As Integer
        ConsoleReporter.PrintTitle("CUDA 设备与 NVRTC 环境探测")

        Try
            Dim report = CudaEnvironment.Probe(options.NvrtcPath)

            ConsoleReporter.PrintEnvironment(report)

            If report.HasError Then
                Console.WriteLine()
                Console.WriteLine($"  探测出错: {report.ErrorMessage}")
            End If

            ' 实际编译 + 加载一次，确认当前组合可用
            If report.HasDevice Then
                Console.WriteLine()
                Console.WriteLine("  正在验证内核编译与加载 ...")

                Dim engineOptions As New EngineOptions With {
                    .DeviceOrdinal = options.DeviceOrdinal,
                    .NvrtcPath = options.NvrtcPath,
                    .ImagePath = options.ImagePath
                }

                Using engine = CudaEngine.TryCreate(engineOptions)
                    If engine Is Nothing Then
                        Console.WriteLine($"  失败: {engineOptions.ErrorMessage}")
                        ConsoleReporter.PrintDiagnostics(engineOptions.Diagnostics)
                        ConsoleReporter.PrintFixSuggestion(report)
                        Return 1
                    End If

                    Console.WriteLine($"  成功: {engine.Image}")

                    Dim memory = engine.GetMemoryInfo()
                    Console.WriteLine($"  显存      : {memory}")
                End Using
            Else
                ConsoleReporter.PrintFixSuggestion(report)
            End If

            Return 0
        Catch ex As Exception
            Console.WriteLine($"探测失败: {ex}")
            Return 1
        End Try
    End Function

    Private Function RunEmitKernels(options As CliOptions) As Integer
        Dim target As String = options.CsvPath

        If String.IsNullOrWhiteSpace(target) Then target = "kernels"

        Try
            Dim written = KernelSources.ExportTo(target)

            ' 同时输出一份合并后的源码，方便 nvcc 一次性编译成单个 cubin
            Dim combinedPath = Path.Combine(target, "all.cu")
            File.WriteAllText(combinedPath, KernelSources.CombinedSource(), New System.Text.UTF8Encoding(False))
            written = written.Concat({combinedPath}).ToList()

            Console.WriteLine($"已导出 {written.Count} 个内核源码到 {Path.GetFullPath(target)}：")

            For Each item In written
                Console.WriteLine($"  {item}")
            Next

            Console.WriteLine()
            Console.WriteLine("可以用如下命令离线编译（把 sm_86 换成你的显卡架构）：")
            Console.WriteLine("  nvcc -arch=sm_86 -cubin -o kernels\\kernels.cubin kernels\\all.cu")
            Console.WriteLine("  ILCuda.Demo demo --cubin kernels\\kernels.cubin")

            Return 0
        Catch ex As Exception
            Console.WriteLine($"导出失败: {ex.Message}")
            Return 1
        End Try
    End Function

    Private Function RunListKernels() As Integer
        ConsoleReporter.PrintTitle("内核注册表（框架内置 + demo 注册）")

        For Each info As KernelInfo In KernelCatalog.All()
            Console.WriteLine($"  {info}")
        Next

        Console.WriteLine()
        Console.WriteLine($"  共 {KernelCatalog.All().Count} 个内核。")
        Return 0
    End Function

    Private Function RunSelfTest(options As CliOptions) As Integer
        ConsoleReporter.PrintTitle("内核逻辑自检（CPU 精确模拟 GPU 内核的索引与公式）")

        Console.WriteLine()
        Console.WriteLine("  说明：该命令逐线程模拟 rowStats / gram / finalize 三个内核的分块方式，")
        Console.WriteLine("        在无法上机（例如驱动与工具包版本不匹配）时验证内核逻辑是否正确。")

        Dim ok = True

        ok = KernelEmulator.RunSelfTest(64, 32, options.Seed) AndAlso ok
        ok = KernelEmulator.RunSelfTest(200, 97, options.Seed) AndAlso ok   ' 故意不是 TILE 的整数倍
        ok = KernelEmulator.RunSelfTest(257, 133, options.Seed) AndAlso ok

        Console.WriteLine()
        Console.WriteLine(If(ok, "自检全部通过。", "自检未通过，请检查内核实现。"))

        Return If(ok, 0, 1)
    End Function

    ''' <summary>
    ''' il 命令：把 IlDecompile\MetricFunctions.vb 里的方法反编译成 AST，
    ''' 打印伪代码与生成的 .cu，并用 AST 解释器验证反编译语义（不需要 GPU）。
    ''' </summary>
    Private Function RunIlDecompile(options As CliOptions) As Integer
        Dim testOptions As New IlCudaTestOptions With {
            .Rows = options.Rows,
            .Cols = options.Cols,
            .Seed = options.Seed,
            .ShowAst = options.ShowAst,
            .ShowSource = options.ShowSource,
            .DumpIl = options.DumpIl
        }

        Return If(IlCudaComparison.RunDecompileReport(testOptions), 0, 1)
    End Function

    ''' <summary>
    ''' il-compare 命令：把 IL 生成的 .cu 注入内核注册表后创建引擎，
    ''' 在 GPU 上同时跑手写 metrics.cu 与 IL 生成内核，逐元素比较最大绝对误差。
    ''' </summary>
    Private Function RunIlCompare(options As CliOptions) As Integer
        Dim testOptions As New IlCudaTestOptions With {
            .Rows = options.Rows,
            .Cols = options.Cols,
            .Seed = options.Seed,
            .DeviceOrdinal = options.DeviceOrdinal,
            .NvrtcPath = options.NvrtcPath,
            .ImagePath = options.ImagePath,
            .CpuOnly = options.CpuOnly,
            .ShowAst = False,
            .ShowSource = False
        }

        Return If(IlCudaComparison.RunGpuComparison(testOptions), 0, 1)
    End Function

    Private Function RunDemo(options As CliOptions) As Integer
        ConsoleReporter.PrintTitle("ILCuda：纯 Interop 的 CUDA 计算框架 + 皮尔逊/欧氏 demo")

        ' ---------- 1. 准备数据 ----------
        Dim matrix As MatrixData

        Try
            If String.IsNullOrWhiteSpace(options.CsvPath) Then
                matrix = MatrixData.CreateRandom(options.Rows, options.Cols, options.Seed)
            Else
                matrix = MatrixData.FromCsv(options.CsvPath)
            End If
        Catch ex As Exception
            Console.WriteLine($"数据载入失败: {ex.Message}")
            Return 2
        End Try

        Console.WriteLine($"  输入矩阵: {matrix.Rows} 行 x {matrix.Cols} 列   ({matrix.Source})")

        ' ---------- 2. 初始化 GPU ----------
        Dim engine As CudaEngine = Nothing
        Dim engineOptions As New EngineOptions With {
            .DeviceOrdinal = options.DeviceOrdinal,
            .NvrtcPath = options.NvrtcPath,
            .ImagePath = options.ImagePath,
            .ForceImage = options.ForceImage
        }

        ConsoleReporter.PrintSection("GPU 初始化（CUDA Driver API / NVRTC）")

        If options.CpuOnly Then
            Console.WriteLine("  已指定 --cpu-only，跳过 GPU 初始化。")
        Else
            engine = CudaEngine.TryCreate(engineOptions)

            If engine Is Nothing Then
                Console.WriteLine($"  GPU 不可用: {engineOptions.ErrorMessage}")
                ConsoleReporter.PrintDiagnostics(engineOptions.Diagnostics)
                ConsoleReporter.PrintFixSuggestion(CudaEnvironment.Probe(options.NvrtcPath))
                Console.WriteLine()
                Console.WriteLine("  >> 自动回退到 CPU 实现，演示继续。")
            Else
                ConsoleReporter.PrintDevice(engine.Device)
                Console.WriteLine($"  内核镜像  : {engine.Image}")
                Console.WriteLine($"  显存      : {engine.GetMemoryInfo()}")
            End If
        End If

        Try
            ' ---------- 3. 基础数学加速（框架自带内核） ----------
            If engine IsNot Nothing Then
                ConsoleReporter.PrintSection("基础数学加速：vecAdd(c = a + b) 与 saxpy(out = 2.5x + y)")

                Dim basicMs As Double = 0
                Dim basicError As Double = 0

                GpuBasicMath.RunBasicDemo(engine, options.Elements, basicMs, basicError)

                Console.WriteLine($"  向量规模       : {options.Elements:N0} 个单精度元素")
                Console.WriteLine($"  GPU 内核耗时   : {basicMs:F3} ms")
                Console.WriteLine($"  与 CPU 的最大绝对误差: {basicError:E3}")

                RunFrameworkKernelSmokeTest(engine)
            End If

            ' ---------- 4. 行间相似度矩阵 ----------
            ConsoleReporter.PrintSection("矩阵行间皮尔逊相关系数矩阵 / 欧氏距离矩阵")

            Dim gpuResult As MatrixMetricsResult = Nothing

            If engine IsNot Nothing Then
                If options.UseAsyncPipeline Then
                    Console.WriteLine("  （--async：使用页锁定内存 + 独立流）")
                    gpuResult = GpuMetrics.ComputeAsync(engine, matrix)
                Else
                    gpuResult = GpuMetrics.Compute(engine, matrix)
                End If
            End If

            Dim cpuResult = CpuMetrics.Compute(matrix)

            If gpuResult IsNot Nothing Then
                Dim corrError = CpuMetrics.MaxAbsError(gpuResult.Correlation, cpuResult.Correlation)
                Dim distError = CpuMetrics.MaxAbsError(gpuResult.Distance, cpuResult.Distance)

                Console.WriteLine()
                Console.WriteLine($"  相关系数矩阵最大绝对误差: {corrError:E3}")
                Console.WriteLine($"  距离矩阵最大绝对误差    : {distError:E3}")
            End If

            ConsoleReporter.PrintTiming(gpuResult, cpuResult)

            ' ---------- 5. 结果预览 ----------
            Dim previewSource = If(gpuResult IsNot Nothing, gpuResult, cpuResult)

            ConsoleReporter.PrintMatrix("皮尔逊相关系数矩阵（预览）",
                                        previewSource.Correlation, options.Preview, matrix.RowNames)
            ConsoleReporter.PrintMatrix("欧氏距离矩阵（预览）",
                                        previewSource.Distance, options.Preview, matrix.RowNames)

            Console.WriteLine()
            Console.WriteLine("完成。")
            Return 0
        Finally
            If engine IsNot Nothing Then engine.Dispose()
        End Try
    End Function

    ''' <summary>顺带冒烟一下框架的通用内核（归约 / elementwise / GEMM），确认它们能被编译并算对</summary>
    Private Sub RunFrameworkKernelSmokeTest(engine As CudaEngine)
        ConsoleReporter.PrintSection("框架通用内核冒烟测试（reduce / elementwise / gemm）")

        Try
            Const n As Integer = 1 << 16

            Dim source(n - 1) As Single
            For i As Integer = 0 To n - 1
                source(i) = CSng(i Mod 7) - 3.0F
            Next

            Using device As New DeviceBuffer(Of Single)(n)
                device.Write(source)

                Dim sum = GpuReduce.Sum(engine, device)
                Dim max = GpuReduce.Max(engine, device)
                Dim min = GpuReduce.Min(engine, device)

                Dim expectedSum As Double = 0
                For Each v In source
                    expectedSum += v
                Next

                Console.WriteLine($"  归约 sum={sum:G6} (CPU {expectedSum:G6})  min={min:G6}  max={max:G6}")

                Using doubled As New DeviceBuffer(Of Single)(n)
                    GpuElementwise.Scale(engine, device, 2.0F, doubled)

                    Dim scaled = GpuReduce.Sum(engine, doubled)
                    Console.WriteLine($"  Scale x2 后的 sum={scaled:G6}（应约为 {expectedSum * 2:G6}）")
                End Using
            End Using

            ' 4x8 = (4x3) * (3x8)
            Dim a = New Single() {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12}
            Dim b = New Single() {1, 0, 0, 0, 0, 0, 0, 0,
                                  0, 1, 0, 0, 0, 0, 0, 0,
                                  0, 0, 1, 0, 0, 0, 0, 0}
            Dim c = GpuBlas.Gemm(engine, a, b, 4, 8, 3)

            Console.WriteLine($"  GEMM 结果第一行: {String.Join(", ", c.Take(8).Select(Function(v) v.ToString("G4")))}")
        Catch ex As Exception
            Console.WriteLine($"  通用内核冒烟测试失败: {ex.Message}")
        End Try
    End Sub

    ' ------------------------------------------------------------------
    ' 命令行解析
    ' ------------------------------------------------------------------

    Private Function ParseCommandLine(args As String()) As CliOptions
        Dim options As New CliOptions()
        Dim i As Integer = 0

        ' 第一个非 "--" 开头的参数视为命令
        If args.Length > 0 AndAlso Not args(0).StartsWith("--") Then
            options.Command = args(0).ToLowerInvariant()
            i = 1
        End If

        While i < args.Length
            Dim arg = args(i)

            If Not arg.StartsWith("--") Then
                i += 1
                Continue While
            End If

            Dim key = arg.Substring(2)
            Dim value As String = Nothing

            If i + 1 < args.Length AndAlso Not args(i + 1).StartsWith("--") Then
                value = args(i + 1)
                i += 1
            End If

            Select Case key.ToLowerInvariant()
                Case "rows"
                    options.Rows = Integer.Parse(value)
                Case "cols"
                    options.Cols = Integer.Parse(value)
                Case "seed"
                    options.Seed = Integer.Parse(value)
                Case "csv"
                    options.CsvPath = value
                Case "nvrtc"
                    options.NvrtcPath = value
                Case "cubin", "image", "ptx"
                    options.ImagePath = value
                Case "device"
                    options.DeviceOrdinal = Integer.Parse(value)
                Case "elements"
                    options.Elements = Integer.Parse(value)
                Case "preview"
                    options.Preview = Integer.Parse(value)
                Case "cpu-only"
                    options.CpuOnly = True
                Case "no-ast"
                    options.ShowAst = False
                Case "no-source"
                    options.ShowSource = False
                Case "dump-il"
                    options.DumpIl = True
                Case "force-image"
                    options.ForceImage = True
                Case "async"
                    options.UseAsyncPipeline = True
                Case "help", "h"
                    options.ShowHelp = True
                Case Else
                    Throw New ArgumentException($"未知参数 --{key}")
            End Select

            i += 1
        End While

        ' emit-kernels 的目标目录复用 --csv 之外的第一个裸参数
        If options.Command = "emit-kernels" Then
            Dim first = args.FirstOrDefault(Function(a) Not a.StartsWith("--") AndAlso a <> options.Command)
            If first IsNot Nothing Then options.CsvPath = first
        End If

        Return options
    End Function

    Private Sub PrintHelp()
        Console.WriteLine()
        Console.WriteLine("ILCuda.Demo —— 基于 ILCuda 框架（不依赖任何第三方 NuGet 程序包）的皮尔逊/欧氏 demo")
        Console.WriteLine()
        Console.WriteLine("用法:")
        Console.WriteLine("  ILCuda.Demo info                     枚举 GPU 设备与 NVRTC 候选，并验证内核能否加载")
        Console.WriteLine("  ILCuda.Demo demo                     基础数学加速 + 行间相似度矩阵演示（默认命令）")
        Console.WriteLine("  ILCuda.Demo selftest                 用 CPU 模拟内核索引与公式，做正确性自检")
        Console.WriteLine("  ILCuda.Demo kernels                  列出内核注册表中的全部内核")
        Console.WriteLine("  ILCuda.Demo emit-kernels <dir>       导出参与编译的 CUDA 内核源码")
        Console.WriteLine("  ILCuda.Demo il                       IL 反编译：AST 伪代码 + 生成的 .cu + 解释求值自检（不需要 GPU）")
        Console.WriteLine("  ILCuda.Demo il-compare               GPU 上对比「IL 生成内核」与手写 metrics.cu 的结果")
        Console.WriteLine("  ILCuda.Demo help                     显示本帮助")
        Console.WriteLine()
        Console.WriteLine("常用参数:")
        Console.WriteLine("  --rows <n>      合成随机矩阵行数（默认 512）")
        Console.WriteLine("  --cols <n>      合成随机矩阵列数（默认 256）")
        Console.WriteLine("  --seed <n>      随机数种子（默认 42）")
        Console.WriteLine("  --csv <file>    从 CSV/TSV 载入矩阵")
        Console.WriteLine("  --device <n>    使用第几块 GPU（默认 0）")
        Console.WriteLine("  --nvrtc <dll>   显式指定 nvrtc64_*.dll")
        Console.WriteLine("  --cubin <file>  显式指定预编译的 .ptx / .cubin")
        Console.WriteLine("  --elements <n>  基础数学示例向量规模（默认 1048576）")
        Console.WriteLine("  --preview <n>   结果矩阵预览大小（默认 6）")
        Console.WriteLine("  --cpu-only      只跑 CPU 参考实现")
        Console.WriteLine("  --no-ast        il 命令不打印还原出的伪代码")
        Console.WriteLine("  --no-source     il 命令不打印生成的 CUDA 源码")
        Console.WriteLine("  --async         用页锁定内存 + 独立流跑异步流水线")
        Console.WriteLine("  --force-image   跳过镜像/驱动版本兼容性预检（仅供调试，可能崩溃）")
        Console.WriteLine()
        Console.WriteLine("示例:")
        Console.WriteLine("  ILCuda.Demo demo --rows 1024 --cols 512")
        Console.WriteLine("  ILCuda.Demo demo --csv data.csv --preview 8")
        Console.WriteLine("  ILCuda.Demo demo --async --rows 2048 --cols 512")
        Console.WriteLine("  ILCuda.Demo demo --cubin kernels\\kernels.cubin")
        Console.WriteLine()
    End Sub
End Module

