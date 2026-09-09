#Region "Microsoft.VisualBasic::cbdb602f898540e406bc7467b2cb8858, cuda\ILCuda\test\IlDecompile\IlCudaComparison.vb"

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

    '   Total Lines: 540
    '    Code Lines: 374 (69.26%)
    ' Comment Lines: 48 (8.89%)
    '    - Xml Docs: 20.83%
    ' 
    '   Blank Lines: 118 (21.85%)
    '     File Size: 24.31 KB


    '     Class IlCudaTestOptions
    ' 
    '         Properties: Cols, CpuOnly, DeviceOrdinal, DumpIl, ImagePath
    '                     NvrtcPath, Rows, Seed, ShowAst, ShowSource
    ' 
    '     Module IlCudaComparison
    ' 
    '         Function: CheckAgainstInterpreter, CompareClamp, CompareCorrelation, CompareDistance, CompareGram
    '                   CompareRowStats, CountInstructions, CpuRowSum, CpuRowSumSq, Report
    '                   RunDecompileReport, RunGpuComparison, SampleArgs, TargetMethods
    ' 
    '         Sub: RunMetricsBaseline
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' ---------------------------------------------------------------------------
' IL -> CUDA 流水线的测试与对比驱动
'
' 两档：
'   1) RunDecompileReport —— 不需要 GPU。把目标方法反编译成 AST，打印类 VB 伪代码
'      与生成的 .cu，并用 AstInterpreter 在 CPU 上执行同一棵 AST，
'      与"直接调用原方法"的结果比对，验证反编译语义正确。
'   2) RunGpuComparison   —— 需要 GPU。先把 IL 生成的 .cu 注入 KernelSources，
'      创建引擎后同时跑：
'          * 手写内核 metrics.cu 的 rowStats / gram / finalize 三件套（基准）
'          * IL 反编译生成的 il_*_kernel
'      再逐元素比较两者的最大绝对误差。
'
' 中间量（rowSum / rowSumSq / dot）直接复用基准内核写回的显存缓冲，
' 因此相关/距离这两项比较的是"完全相同的输入 + 完全相同的公式"，
' 误差应当接近 0；若有量级偏差，说明反编译或发射环节有误。
' ---------------------------------------------------------------------------

Imports System.Reflection
Imports ILCudaDemo.Diagnostics
Imports ILCudaDemo.Metrics
Imports Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.IL
Imports Microsoft.VisualBasic.Computing.ILCuda.IL2Cuda
Imports Microsoft.VisualBasic.Computing.ILCuda.Runtime

Namespace IlDecompile

    Public Class IlCudaTestOptions
        Public Property Rows As Integer = 64
        Public Property Cols As Integer = 32
        Public Property Seed As Integer = 42
        Public Property DeviceOrdinal As Integer = 0
        Public Property NvrtcPath As String
        Public Property ImagePath As String
        Public Property ShowAst As Boolean = True
        Public Property ShowSource As Boolean = True
        ''' <summary>打印原始 IL 指令流与基本块 / 支配 / 循环结构（定位反编译失败用）</summary>
        Public Property DumpIl As Boolean = False
        Public Property CpuOnly As Boolean = False
    End Class

    Public Module IlCudaComparison

        ''' <summary>全部待反编译的目标方法</summary>
        Public Function TargetMethods() As MethodInfo()
            Dim t = GetType(MetricFunctions)

            Return New MethodInfo() {
                t.GetMethod(NameOf(MetricFunctions.RowSum)),
                t.GetMethod(NameOf(MetricFunctions.RowSumSq)),
                t.GetMethod(NameOf(MetricFunctions.GramDot)),
                t.GetMethod(NameOf(MetricFunctions.CorrelationCell)),
                t.GetMethod(NameOf(MetricFunctions.DistanceCell)),
                t.GetMethod(NameOf(MetricFunctions.PearsonClamp))
            }
        End Function

        ' ==================================================================
        ' 第一档：反编译 + 解释求值自检（不需要 GPU）
        ' ==================================================================

        ''' <summary>反编译全部目标方法并打印诊断 / 伪代码 / 生成的 CUDA 源码，同时做解释求值自检</summary>
        Public Function RunDecompileReport(options As IlCudaTestOptions) As Boolean
            ConsoleReporter.PrintTitle("IL -> AST -> CUDA：反编译与解释求值自检")

            Dim ok As Boolean = True

            For Each method As MethodInfo In TargetMethods()
                Console.WriteLine()
                Console.WriteLine($"---- {method.Name} ----")

                If options.DumpIl Then
                    Console.WriteLine()
                    Console.WriteLine(MethodDecompiler.DumpStructure(method))
                End If

                Dim kernel As IlCudaKernel

                Try
                    kernel = IlCudaTranslator.Translate(method)
                Catch ex As Exception
                    Console.WriteLine($"  反编译失败: {ex.Message}")
                    ok = False
                    Continue For
                End Try

                Dim syntax = kernel.Syntax

                Console.WriteLine($"  签名       : {syntax}")
                Console.WriteLine($"  指令/块    : {CountInstructions(syntax)} 条 IL")
                Console.WriteLine($"  索引模式   : {kernel.IndexMode}")
                Console.WriteLine($"  设备函数   : {kernel.DeviceFunctionName}")
                Console.WriteLine($"  内核       : {kernel.Name}")

                If syntax.Diagnostics IsNot Nothing AndAlso syntax.Diagnostics.HasMessages Then
                    Console.WriteLine("  诊断       :")

                    For Each line In syntax.Diagnostics.Messages
                        Console.WriteLine($"    {line}")
                    Next
                End If

                If options.ShowAst Then
                    Console.WriteLine()
                    Console.WriteLine("  还原出的伪代码：")

                    For Each line In SyntaxWriter.WriteMethod(syntax).Split(Environment.NewLine.ToCharArray(),
                                                                            StringSplitOptions.RemoveEmptyEntries)
                        Console.WriteLine($"    {line}")
                    Next
                End If

                If options.ShowSource Then
                    Console.WriteLine()
                    Console.WriteLine("  生成的 CUDA 源码：")

                    For Each line In kernel.Source.Split(Environment.NewLine.ToCharArray(),
                                                         StringSplitOptions.RemoveEmptyEntries)
                        Console.WriteLine($"    {line}")
                    Next
                End If

                ok = CheckAgainstInterpreter(method, kernel.Syntax) AndAlso ok
            Next

            Console.WriteLine()
            Console.WriteLine(If(ok, "反编译自检全部通过。", "反编译自检存在失败项，请查看上面的输出。"))

            Return ok
        End Function

        ''' <summary>用同一组参数分别跑"原方法"与"AST 解释器"，比较返回值</summary>
        Private Function CheckAgainstInterpreter(method As MethodInfo, syntax As MethodSyntax) As Boolean
            Dim args = SampleArgs(method)
            Dim expected As Object

            Try
                expected = method.Invoke(Nothing, args)
            Catch ex As Exception
                Console.WriteLine($"  原方法调用失败: {ex.GetBaseException().Message}")
                Return False
            End Try

            Dim actual As Object

            Try
                actual = New AstInterpreter(syntax).Invoke(args)
            Catch ex As Exception
                Console.WriteLine($"  AST 解释执行失败: {ex.Message}")
                Return False
            End Try

            Dim diff = System.Math.Abs(System.Convert.ToDouble(expected) - System.Convert.ToDouble(actual))
            Dim tolerance = 1.0E-5 * System.Math.Max(1.0, System.Math.Abs(System.Convert.ToDouble(expected)))
            Dim pass = diff <= tolerance

            Console.WriteLine($"  解释求值自检: 原方法={expected}, AST={actual}, 差={diff:E3}  {(If(pass, "OK", "FAIL"))}")

            Return pass
        End Function

        ''' <summary>为目标方法造一组"下标合法、数值不退化"的样例参数</summary>
        Private Function SampleArgs(method As MethodInfo) As Object()
            Dim parameters = method.GetParameters()
            Dim args(parameters.Length - 1) As Object

            For i As Integer = 0 To parameters.Length - 1
                Dim p = parameters(i)
                Dim lower = If(p.Name, String.Empty).ToLowerInvariant()

                If p.ParameterType.IsArray Then
                    Dim n = 256
                    Dim data(n - 1) As Single

                    For k As Integer = 0 To n - 1
                        data(k) = CSng((k Mod 17) - 8) * 0.5F
                    Next

                    args(i) = data

                ElseIf p.ParameterType = GetType(Single) Then
                    args(i) = 1.5F

                ElseIf p.ParameterType = GetType(Integer) Then
                    Select Case lower
                        Case "cols" : args(i) = 8
                        Case "rows" : args(i) = 16
                        Case "i" : args(i) = 2
                        Case "j" : args(i) = 5
                        Case Else : args(i) = 3
                    End Select
                End If
            Next

            Return args
        End Function

        Private Function CountInstructions(syntax As MethodSyntax) As String
            If syntax.Method Is Nothing Then Return "?"

            Using reader As New MethodBodyReader(syntax.Method)
                Return reader.Instructions.Count.ToString()
            End Using
        End Function

        ' ==================================================================
        ' 第二档：GPU 上对比手写内核与 IL 生成内核
        ' ==================================================================

        Public Function RunGpuComparison(options As IlCudaTestOptions) As Boolean
            ConsoleReporter.PrintTitle("IL 生成内核 vs 手写 metrics.cu：GPU 结果对比")

            ' ---------- 1) 反编译并注入内核源码（必须在 TryCreate 之前） ----------
            Dim kernels As New Dictionary(Of String, IlCudaKernel)(StringComparer.Ordinal)

            For Each method As MethodInfo In TargetMethods()
                Try
                    Dim kernel = IlCudaTranslator.Translate(method)

                    kernel.Register()
                    kernels(method.Name) = kernel

                    Console.WriteLine($"  已注册内核 {kernel.Name} [{kernel.IndexMode}] <- {method.Name}")
                Catch ex As Exception
                    Console.WriteLine($"  {method.Name} 反编译/发射失败: {ex.Message}")
                    Return False
                End Try
            Next

            If options.CpuOnly Then
                Console.WriteLine()
                Console.WriteLine("  已指定 --cpu-only，跳过 GPU 对比。")
                Return True
            End If

            ' ---------- 2) 引擎 ----------
            Dim engineOptions As New EngineOptions With {
                .DeviceOrdinal = options.DeviceOrdinal,
                .NvrtcPath = options.NvrtcPath,
                .ImagePath = options.ImagePath
            }

            Dim matrix = MatrixData.CreateRandom(options.Rows, options.Cols, options.Seed)

            Console.WriteLine()
            Console.WriteLine($"  输入矩阵: {matrix.Rows} 行 x {matrix.Cols} 列")

            Using engine = CudaEngine.TryCreate(engineOptions)
                If engine Is Nothing Then
                    Console.WriteLine($"  GPU 不可用: {engineOptions.ErrorMessage}")
                    ConsoleReporter.PrintDiagnostics(engineOptions.Diagnostics)
                    Console.WriteLine()
                    Console.WriteLine("  >> 无法做 GPU 对比，请用 il 命令查看反编译与解释求值自检结果。")
                    Return False
                End If

                Console.WriteLine($"  内核镜像  : {engine.Image}")

                Dim rows = matrix.Rows
                Dim cols = matrix.Cols
                Dim cells = rows * rows

                Using bufX As New DeviceBuffer(Of Single)(matrix.Data.Length),
                      bufSum As New DeviceBuffer(Of Single)(rows),
                      bufSumSq As New DeviceBuffer(Of Single)(rows),
                      bufDot As New DeviceBuffer(Of Single)(cells),
                      bufCorr As New DeviceBuffer(Of Single)(cells),
                      bufDist As New DeviceBuffer(Of Single)(cells)

                    bufX.Write(matrix.Data)

                    ' ---------- 3) 基准：手写 metrics.cu ----------
                    RunMetricsBaseline(engine, matrix, bufX, bufSum, bufSumSq, bufDot, bufCorr, bufDist)

                    Dim baseSum = bufSum.Read()
                    Dim baseSumSq = bufSumSq.Read()
                    Dim baseDot = bufDot.Read()
                    Dim baseCorr = bufCorr.Read()
                    Dim baseDist = bufDist.Read()

                    Dim ok As Boolean = True

                    ' ---------- 4) 逐项对比 ----------
                    Console.WriteLine()
                    Console.WriteLine("  对比结果（IL 生成内核 vs 手写 metrics.cu）：")

                    ok = CompareRowStats(kernels, engine, matrix, bufX, baseSum, baseSumSq) AndAlso ok
                    ok = CompareGram(kernels, engine, matrix, bufX, baseDot) AndAlso ok
                    ok = CompareCorrelation(kernels, engine, matrix, bufDot, bufSum, bufSumSq, baseCorr) AndAlso ok
                    ok = CompareDistance(kernels, engine, matrix, bufDot, bufSumSq, baseDist) AndAlso ok
                    ok = CompareClamp(kernels, engine, matrix, baseSum, baseSumSq, baseDot) AndAlso ok

                    Console.WriteLine()
                    Console.WriteLine(If(ok, "全部对比项通过。", "存在超出容差的对比项，请查看上面的误差。"))

                    Return ok
                End Using
            End Using
        End Function

        ' ------------------------------------------------------------------
        ' 基准流水线
        ' ------------------------------------------------------------------

        Private Sub RunMetricsBaseline(engine As CudaEngine, matrix As MatrixData,
                                       bufX As DeviceBuffer(Of Single),
                                       bufSum As DeviceBuffer(Of Single),
                                       bufSumSq As DeviceBuffer(Of Single),
                                       bufDot As DeviceBuffer(Of Single),
                                       bufCorr As DeviceBuffer(Of Single),
                                       bufDist As DeviceBuffer(Of Single))
            Dim rows = matrix.Rows
            Dim cols = matrix.Cols
            Dim tile = GpuMetrics.TileSize

            engine.GetKernel(MetricsKernelNames.RowStats).Launch(
                Nothing, rows, 1, GpuMetrics.RowStatsBlockSize, 1,
                2 * GpuMetrics.RowStatsBlockSize * 4,
                bufX, rows, cols, bufSum, bufSumSq)

            engine.GetKernel(MetricsKernelNames.Gram).Launch(
                Nothing, LaunchPlanner.For2D(rows, rows, tile, tile, 2 * tile * tile * 4),
                bufX, rows, cols, bufDot)

            engine.GetKernel(MetricsKernelNames.FinalizeMetrics).Launch(
                Nothing, LaunchPlanner.For2D(rows, rows, tile, tile, 0),
                bufDot, bufSum, bufSumSq, rows, cols, bufCorr, bufDist)
        End Sub

        ' ------------------------------------------------------------------
        ' 各项对比
        ' ------------------------------------------------------------------

        Private Function CompareRowStats(kernels As Dictionary(Of String, IlCudaKernel),
                                         engine As CudaEngine, matrix As MatrixData,
                                         bufX As DeviceBuffer(Of Single),
                                         baseSum As Single(), baseSumSq As Single()) As Boolean
            Dim rows = matrix.Rows
            Dim cols = matrix.Cols
            Dim ok As Boolean = True

            Using out As New DeviceBuffer(Of Single)(rows)
                kernels(NameOf(MetricFunctions.RowSum)).Launch(
                    engine, LaunchPlanner.For1D(rows, 256), bufX, cols, out, rows)

                Dim got = out.Read()
                Dim expected = CpuRowSum(matrix)

                ok = Report("RowSum", got, expected) AndAlso ok
                ok = Report("RowSum(与 metrics.cu rowStatsKernel)", got, baseSum) AndAlso ok
            End Using

            Using out As New DeviceBuffer(Of Single)(rows)
                kernels(NameOf(MetricFunctions.RowSumSq)).Launch(
                    engine, LaunchPlanner.For1D(rows, 256), bufX, cols, out, rows)

                Dim got = out.Read()
                Dim expected = CpuRowSumSq(matrix)

                ok = Report("RowSumSq", got, expected) AndAlso ok
                ok = Report("RowSumSq(与 metrics.cu rowStatsKernel)", got, baseSumSq) AndAlso ok
            End Using

            Return ok
        End Function

        Private Function CompareGram(kernels As Dictionary(Of String, IlCudaKernel),
                                     engine As CudaEngine, matrix As MatrixData,
                                     bufX As DeviceBuffer(Of Single), baseDot As Single()) As Boolean
            Dim rows = matrix.Rows
            Dim cols = matrix.Cols

            Using out As New DeviceBuffer(Of Single)(rows * rows)
                kernels(NameOf(MetricFunctions.GramDot)).Launch(
                    engine, LaunchPlanner.For2D(rows, rows, 16, 16), bufX, cols, out, rows, rows)

                Dim got = out.Read()

                Return Report("GramDot(与 metrics.cu gramKernel)", got, baseDot)
            End Using
        End Function

        Private Function CompareCorrelation(kernels As Dictionary(Of String, IlCudaKernel),
                                            engine As CudaEngine, matrix As MatrixData,
                                            bufDot As DeviceBuffer(Of Single),
                                            bufSum As DeviceBuffer(Of Single),
                                            bufSumSq As DeviceBuffer(Of Single),
                                            baseCorr As Single()) As Boolean
            Dim rows = matrix.Rows
            Dim cols = matrix.Cols

            Using out As New DeviceBuffer(Of Single)(rows * rows)
                kernels(NameOf(MetricFunctions.CorrelationCell)).Launch(
                    engine, LaunchPlanner.For2D(rows, rows, 16, 16),
                    bufDot, bufSum, bufSumSq, rows, cols, out, rows, rows)

                Dim got = out.Read()

                Return Report("CorrelationCell(与 metrics.cu finalizeKernel)", got, baseCorr)
            End Using
        End Function

        Private Function CompareDistance(kernels As Dictionary(Of String, IlCudaKernel),
                                         engine As CudaEngine, matrix As MatrixData,
                                         bufDot As DeviceBuffer(Of Single),
                                         bufSumSq As DeviceBuffer(Of Single),
                                         baseDist As Single()) As Boolean
            Dim rows = matrix.Rows

            Using out As New DeviceBuffer(Of Single)(rows * rows)
                kernels(NameOf(MetricFunctions.DistanceCell)).Launch(
                    engine, LaunchPlanner.For2D(rows, rows, 16, 16),
                    bufDot, bufSumSq, rows, out, rows, rows)

                Dim got = out.Read()

                Return Report("DistanceCell(与 metrics.cu finalizeKernel)", got, baseDist)
            End Using
        End Function

        ''' <summary>
        ''' 纯标量函数走的是"逐元素自动包裹"约定：每个标量参数都变成按下标读取的数组。
        ''' 这里把协方差与分母先在主机上铺成两个数组，再用生成的内核算 [-1,1] 截断。
        ''' </summary>
        Private Function CompareClamp(kernels As Dictionary(Of String, IlCudaKernel),
                                      engine As CudaEngine, matrix As MatrixData,
                                      baseSum As Single(), baseSumSq As Single(),
                                      baseDot As Single()) As Boolean
            Dim rows = matrix.Rows
            Dim cols = matrix.Cols
            Dim cells = rows * rows
            Dim n = CSng(cols)

            Dim cov(cells - 1) As Single
            Dim denom(cells - 1) As Single
            Dim expected(cells - 1) As Single

            For i As Integer = 0 To rows - 1
                Dim meanI = baseSum(i) / n
                Dim varI = baseSumSq(i) - n * meanI * meanI
                Dim sdI = MathF.Sqrt(MathF.Max(varI, 0.0F))

                For j As Integer = 0 To rows - 1
                    Dim meanJ = baseSum(j) / n
                    Dim varJ = baseSumSq(j) - n * meanJ * meanJ
                    Dim index = i * rows + j

                    cov(index) = baseDot(index) - n * meanI * meanJ
                    denom(index) = sdI * MathF.Sqrt(MathF.Max(varJ, 0.0F))
                    expected(index) = MetricFunctions.PearsonClamp(cov(index), denom(index))
                Next
            Next

            Using bufCov As New DeviceBuffer(Of Single)(cells),
                  bufDenom As New DeviceBuffer(Of Single)(cells),
                  out As New DeviceBuffer(Of Single)(cells)

                bufCov.Write(cov)
                bufDenom.Write(denom)

                kernels(NameOf(MetricFunctions.PearsonClamp)).Launch(
                    engine, LaunchPlanner.For1D(cells, 256), bufCov, bufDenom, out, cells)

                Dim got = out.Read()

                Return Report("PearsonClamp(逐元素自动包裹)", got, expected)
            End Using
        End Function

        ' ------------------------------------------------------------------
        ' CPU 参考与误差统计
        ' ------------------------------------------------------------------

        Private Function CpuRowSum(matrix As MatrixData) As Single()
            Dim rows = matrix.Rows
            Dim cols = matrix.Cols
            Dim result(rows - 1) As Single

            For i As Integer = 0 To rows - 1
                Dim sum As Double = 0

                For k As Integer = 0 To cols - 1
                    sum += matrix(i, k)
                Next

                result(i) = CSng(sum)
            Next

            Return result
        End Function

        Private Function CpuRowSumSq(matrix As MatrixData) As Single()
            Dim rows = matrix.Rows
            Dim cols = matrix.Cols
            Dim result(rows - 1) As Single

            For i As Integer = 0 To rows - 1
                Dim sumSq As Double = 0

                For k As Integer = 0 To cols - 1
                    Dim v As Double = matrix(i, k)
                    sumSq += v * v
                Next

                result(i) = CSng(sumSq)
            Next

            Return result
        End Function

        ''' <summary>逐元素比较并打印最大绝对误差；超过容差返回 False</summary>
        Private Function Report(title As String, got As Single(), expected As Single()) As Boolean
            If got Is Nothing OrElse expected Is Nothing OrElse got.Length <> expected.Length Then
                Console.WriteLine($"  {title,-46}: 长度不一致")
                Return False
            End If

            Dim maxError As Double = 0
            Dim maxScale As Double = 0

            For i As Integer = 0 To got.Length - 1
                Dim diff = System.Math.Abs(CDbl(got(i)) - CDbl(expected(i)))

                If diff > maxError Then maxError = diff

                Dim scale = System.Math.Abs(CDbl(expected(i)))
                If scale > maxScale Then maxScale = scale
            Next

            ' 容差：单精度累加顺序不同会带来 ~1e-5 量级的相对误差；
            ' 完全相同公式的两项（相关 / 距离 / clamp）应当更小。
            Dim tolerance = System.Math.Max(1.0E-4, maxScale * 1.0E-5)
            Dim pass = maxError <= tolerance

            Console.WriteLine($"  {title,-46}: 最大绝对误差={maxError:E3}（参考量级 {maxScale:E3}，容差 {tolerance:E3}） {(If(pass, "OK", "FAIL"))}")

            Return pass
        End Function
    End Module
End Namespace

