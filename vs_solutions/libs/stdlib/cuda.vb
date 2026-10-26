Imports System.Diagnostics
Imports System.Reflection
Imports Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.IL
Imports Microsoft.VisualBasic.Computing.ILCuda.IL2Cuda
Imports Microsoft.VisualBasic.Computing.ILCuda.Runtime
Imports std = System.Math

''' <summary>
''' IL → CUDA 模块：把 <see cref="PearsonMetrics"/> 里的普通 VB.NET 方法编译成 CUDA 内核，
''' 在 GPU 上计算皮尔逊相关矩阵与欧氏距离矩阵，并提供 CPU 参考实现用于对拍。
''' <para>
''' 当机器上没有可用的 CUDA 环境时，<see cref="available"/> 返回 False、<see cref="metrics"/>
''' 抛出带诊断信息的异常；脚本应当先探测再决定是否走 GPU 路径（demo 里会降级到 CPU）。
''' </para>
''' </summary>
Public Module cuda

    Private ReadOnly _gate As New Object()
    Private _kernels As Dictionary(Of String, IlCudaKernel)

    ''' <summary>CUDA 环境是否可用（驱动 + 设备 + NVRTC）。</summary>
    Public Function available() As Boolean
        Dim options As New EngineOptions()

        Using engine As CudaEngine = CudaEngine.TryCreate(options)
            Return engine IsNot Nothing
        End Using
    End Function

    ''' <summary>探测当前 CUDA 环境并返回诊断信息（不会抛异常）。</summary>
    Public Function diagnostics() As String()
        Dim report = CudaEnvironment.Probe()
        Dim lines As New List(Of String)

        Call lines.Add($"driver   : {report.DriverVersionText}")
        Call lines.Add($"devices  : {report.DeviceCount}")

        If report.Devices IsNot Nothing Then
            For Each device In report.Devices
                Call lines.Add($"  - {device.Name}")
            Next
        End If

        Call lines.Add($"nvrtc    : {report.HasNvrtc}")
        Call lines.Add($"sources  : {report.HasKernelSource}")

        If report.HasError Then
            Call lines.Add($"error    : {report.ErrorMessage}")
        End If

        For Each suggestion In CudaEnvironment.Suggest(report)
            Call lines.Add($"suggestion: {suggestion.Title} — {suggestion.Detail}")
        Next

        Return lines.ToArray
    End Function

    ''' <summary>当前 CUDA 设备名称（不可用时返回空字符串）。</summary>
    Public Function deviceName() As String
        Dim options As New EngineOptions()

        Using engine As CudaEngine = CudaEngine.TryCreate(options)
            If engine Is Nothing Then
                Return ""
            End If

            Return engine.Device.Name
        End Using
    End Function

    ''' <summary>把 <see cref="PearsonMetrics"/> 的每个方法翻译成 CUDA 内核并返回内核名列表。</summary>
    Public Function kernelNames() As String()
        Return EnsureKernels().Keys.ToArray
    End Function

    ''' <summary>取回某个内核生成的 CUDA 源码。</summary>
    Public Function kernelSource(name As String) As String
        Dim kernel As IlCudaKernel = FindKernel(name)

        Return If(kernel Is Nothing, Nothing, kernel.Source)
    End Function

    ''' <summary>取回某个内核的索引模式（``1D`` / ``2D`` / ``ElementWise``）。</summary>
    Public Function kernelIndexMode(name As String) As String
        Dim kernel As IlCudaKernel = FindKernel(name)

        Return If(kernel Is Nothing, Nothing, kernel.IndexMode.ToString)
    End Function

    ''' <summary>取回某个内核反编译得到的方法伪代码。</summary>
    Public Function kernelPseudoCode(name As String) As String
        Dim kernel As IlCudaKernel = FindKernel(name)

        If kernel Is Nothing OrElse kernel.Syntax Is Nothing Then
            Return Nothing
        End If

        Return SyntaxWriter.WriteMethod(kernel.Syntax)
    End Function

    ''' <summary>
    ''' 用 AST 解释器对内核做一次自检（解释执行结果 vs 直接调用原方法），
    ''' 返回 ``"内核名: diff=... OK|FAIL"`` 形式的诊断行。
    ''' </summary>
    Public Function selfCheck(m As Single(), rows As Integer, cols As Integer) As String()
        Dim kernels As Dictionary(Of String, IlCudaKernel) = EnsureKernels()
        Dim lines As New List(Of String)

        ' 先算出 CorrelationCell / DistanceCell 需要的中间数组
        Dim rowSum(rows - 1) As Single
        Dim rowSumSq(rows - 1) As Single

        For i As Integer = 0 To rows - 1
            rowSum(i) = PearsonMetrics.RowSum(m, cols, i)
            rowSumSq(i) = PearsonMetrics.RowSumSq(m, cols, i)
        Next

        Dim dot(rows * rows - 1) As Single

        For i As Integer = 0 To rows - 1
            For j As Integer = 0 To rows - 1
                dot(i * rows + j) = PearsonMetrics.GramDot(m, cols, i, j)
            Next
        Next

        Dim cases As New List(Of (Name As String, Args As Object(), Expected As Object))
        Dim i0 As Integer = 0
        Dim j0 As Integer = If(rows > 1, 1, 0)

        Call cases.Add(("RowSum", New Object() {m, cols, i0}, PearsonMetrics.RowSum(m, cols, i0)))
        Call cases.Add(("RowSumSq", New Object() {m, cols, i0}, PearsonMetrics.RowSumSq(m, cols, i0)))
        Call cases.Add(("GramDot", New Object() {m, cols, i0, j0}, PearsonMetrics.GramDot(m, cols, i0, j0)))
        Call cases.Add(("CorrelationCell",
                        New Object() {dot, rowSum, rowSumSq, rows, cols, i0, j0},
                        PearsonMetrics.CorrelationCell(dot, rowSum, rowSumSq, rows, cols, i0, j0)))
        Call cases.Add(("DistanceCell",
                        New Object() {dot, rowSumSq, rows, i0, j0},
                        PearsonMetrics.DistanceCell(dot, rowSumSq, rows, i0, j0)))
        Call cases.Add(("PearsonClamp", New Object() {CSng(0.25), CSng(0.5)}, PearsonMetrics.PearsonClamp(CSng(0.25), CSng(0.5))))

        For Each item In cases
            Dim kernel As IlCudaKernel = Nothing

            If Not kernels.TryGetValue(item.Name, kernel) Then
                Call lines.Add($"{item.Name}: not translated")
                Continue For
            End If

            Dim actual As Object = New AstInterpreter(kernel.Syntax).Invoke(item.Args)
            Dim diff As Double = std.Abs(Convert.ToDouble(item.Expected) - Convert.ToDouble(actual))
            Dim tolerance As Double = 1.0E-6 * std.Max(1.0, std.Abs(Convert.ToDouble(item.Expected)))

            Call lines.Add($"{item.Name}: interpreter diff={diff.ToString("E2")} {If(diff <= tolerance, "OK", "FAIL")}")
        Next

        Return lines.ToArray
    End Function

    ' ======================================================== GPU 计算

    ''' <summary>
    ''' 在 GPU 上一次性算出皮尔逊相关矩阵与欧氏距离矩阵。
    ''' </summary>
    ''' <param name="m">行主序的矩阵数据，长度为 rows * cols</param>
    Public Function metrics(m As Single(), rows As Integer, cols As Integer) As CudaMetrics
        Dim kernels As Dictionary(Of String, IlCudaKernel) = EnsureKernels()
        Dim options As New EngineOptions With {.DeviceOrdinal = 0}
        Dim engine As CudaEngine = CudaEngine.TryCreate(options)

        If engine Is Nothing Then
            Throw New InvalidOperationException("CUDA is not available: " & options.ErrorMessage)
        End If

        Dim cells As Integer = rows * rows
        Dim watch As Stopwatch = Stopwatch.StartNew()

        Using engine
            Using bufX As New DeviceBuffer(Of Single)(m.Length),
                  bufSum As New DeviceBuffer(Of Single)(rows),
                  bufSumSq As New DeviceBuffer(Of Single)(rows),
                  bufDot As New DeviceBuffer(Of Single)(cells),
                  bufCorr As New DeviceBuffer(Of Single)(cells),
                  bufDist As New DeviceBuffer(Of Single)(cells)

                Call bufX.Write(m)

                Call kernels("RowSum").Launch(engine, LaunchPlanner.For1D(rows, 256), bufX, cols, bufSum, rows)
                Call kernels("RowSumSq").Launch(engine, LaunchPlanner.For1D(rows, 256), bufX, cols, bufSumSq, rows)
                Call kernels("GramDot").Launch(engine, LaunchPlanner.For2D(rows, rows, 16, 16), bufX, cols, bufDot, rows, rows)
                Call kernels("CorrelationCell").Launch(engine, LaunchPlanner.For2D(rows, rows, 16, 16),
                                                       bufDot, bufSum, bufSumSq, rows, cols, bufCorr, rows, rows)
                Call kernels("DistanceCell").Launch(engine, LaunchPlanner.For2D(rows, rows, 16, 16),
                                                    bufDot, bufSumSq, rows, bufDist, rows, rows)

                Call engine.Synchronize()

                watch.Stop()

                Return New CudaMetrics With {
                    .device = engine.Device.Name,
                    .correlation = bufCorr.Read(),
                    .distance = bufDist.Read(),
                    .rows = rows,
                    .cols = cols,
                    .elapsedMs = watch.Elapsed.TotalMilliseconds
                }
            End Using
        End Using
    End Function

    ''' <summary>只计算皮尔逊相关矩阵（GPU 不可用时抛出异常）。</summary>
    Public Function pearson(m As Single(), rows As Integer, cols As Integer) As Single()
        Return metrics(m, rows, cols).correlation
    End Function

    ''' <summary>只计算欧氏距离矩阵（GPU 不可用时抛出异常）。</summary>
    Public Function distance(m As Single(), rows As Integer, cols As Integer) As Single()
        Return metrics(m, rows, cols).distance
    End Function

    ' ======================================================== CPU 参考实现

    ''' <summary>皮尔逊相关矩阵的 CPU 双精度参考实现。</summary>
    Public Function pearsonCpu(m As Single(), rows As Integer, cols As Integer) As Double()
        Dim n As Double = cols
        Dim mean(rows - 1) As Double
        Dim sd(rows - 1) As Double

        For i As Integer = 0 To rows - 1
            Dim sum As Double = 0
            Dim sumSq As Double = 0

            For k As Integer = 0 To cols - 1
                Dim v As Double = m(i * cols + k)
                sum += v
                sumSq += v * v
            Next

            mean(i) = sum / n
            sd(i) = std.Sqrt(std.Max(sumSq - n * mean(i) * mean(i), 0.0))
        Next

        Dim out(rows * rows - 1) As Double

        For i As Integer = 0 To rows - 1
            For j As Integer = 0 To rows - 1
                Dim dot As Double = 0

                For k As Integer = 0 To cols - 1
                    dot += m(i * cols + k) * m(j * cols + k)
                Next

                Dim cov As Double = dot - n * mean(i) * mean(j)
                Dim denom As Double = sd(i) * sd(j)
                Dim c As Double = If(denom > 0, cov / denom, 0)

                out(i * rows + j) = If(i = j, 1.0, std.Min(1.0, std.Max(-1.0, c)))
            Next
        Next

        Return out
    End Function

    ''' <summary>欧氏距离矩阵的 CPU 双精度参考实现。</summary>
    Public Function distanceCpu(m As Single(), rows As Integer, cols As Integer) As Double()
        Dim sumSq(rows - 1) As Double

        For i As Integer = 0 To rows - 1
            Dim sum As Double = 0

            For k As Integer = 0 To cols - 1
                Dim v As Double = m(i * cols + k)
                sum += v * v
            Next

            sumSq(i) = sum
        Next

        Dim out(rows * rows - 1) As Double

        For i As Integer = 0 To rows - 1
            For j As Integer = 0 To rows - 1
                Dim dot As Double = 0

                For k As Integer = 0 To cols - 1
                    dot += m(i * cols + k) * m(j * cols + k)
                Next

                out(i * rows + j) = If(i = j, 0.0, std.Sqrt(std.Max(sumSq(i) + sumSq(j) - 2.0 * dot, 0.0)))
            Next
        Next

        Return out
    End Function

    ''' <summary>生成 [-1, 1] 区间内的确定性随机矩阵（行主序）。</summary>
    Public Function makeMatrix(rows As Integer, cols As Integer, Optional seed As Integer = 42) As Single()
        Dim random As New Random(seed)
        Dim data(rows * cols - 1) As Single

        For i As Integer = 0 To data.Length - 1
            data(i) = CSng(random.NextDouble() * 2.0 - 1.0)
        Next

        Return data
    End Function

    ''' <summary>把行主序的一维矩阵数据还原成 ``rows × cols`` 的交错数组。</summary>
    Public Function toMatrix(values As Double(), rows As Integer, cols As Integer) As Double()()
        Dim out(rows - 1)() As Double

        For i As Integer = 0 To rows - 1
            Dim row(cols - 1) As Double

            For j As Integer = 0 To cols - 1
                row(j) = values(i * cols + j)
            Next

            out(i) = row
        Next

        Return out
    End Function

    ''' <summary>取矩阵左上角 n×n 的单元格，格式化为文本预览。</summary>
    Public Function preview(values As Double(), rows As Integer, Optional n As Integer = 6) As String
        Dim lines As New List(Of String)

        For i As Integer = 0 To std.Min(n, rows) - 1
            Dim sb As New Text.StringBuilder()

            For j As Integer = 0 To std.Min(n, rows) - 1
                Call sb.Append(values(i * rows + j).ToString("F4").PadLeft(10))
            Next

            Call lines.Add(sb.ToString)
        Next

        Return String.Join(Environment.NewLine, lines)
    End Function

    ''' <summary>取单精度矩阵左上角 n×n 的单元格，格式化为文本预览。</summary>
    Public Function preview(values As Single(), rows As Integer, Optional n As Integer = 6) As String
        Dim lines As New List(Of String)

        For i As Integer = 0 To std.Min(n, rows) - 1
            Dim sb As New Text.StringBuilder()

            For j As Integer = 0 To std.Min(n, rows) - 1
                Call sb.Append(values(i * rows + j).ToString("F4").PadLeft(10))
            Next

            Call lines.Add(sb.ToString)
        Next

        Return String.Join(Environment.NewLine, lines)
    End Function

    ''' <summary>GPU 结果与双精度参考实现之间的最大绝对误差。</summary>
    Public Function maxError(got As Single(), expected As Double()) As Double
        Dim maxDiff As Double = 0

        For i As Integer = 0 To got.Length - 1
            Dim diff As Double = std.Abs(CDbl(got(i)) - expected(i))

            If diff > maxDiff Then
                maxDiff = diff
            End If
        Next

        Return maxDiff
    End Function

    ' ======================================================== 内部辅助

    Private Function EnsureKernels() As Dictionary(Of String, IlCudaKernel)
        SyncLock _gate
            If _kernels IsNot Nothing Then
                Return _kernels
            End If

            Dim translations As New Dictionary(Of String, IlCudaKernel)(StringComparer.Ordinal)
            Dim target As Type = GetType(PearsonMetrics)

            For Each methodName As String In {"RowSum", "RowSumSq", "GramDot", "CorrelationCell", "DistanceCell", "PearsonClamp"}
                Dim method As MethodInfo = target.GetMethod(methodName)

                Call translations.Add(methodName, IlCudaTranslator.Translate(method))
            Next

            ' 内核必须在创建 CUDA 引擎之前注册
            For Each kernel As IlCudaKernel In translations.Values
                Call kernel.Register()
            Next

            _kernels = translations
        End SyncLock

        Return _kernels
    End Function

    Private Function FindKernel(name As String) As IlCudaKernel
        Dim kernel As IlCudaKernel = Nothing

        If EnsureKernels().TryGetValue(If(name, "").Trim, kernel) Then
            Return kernel
        End If

        Return Nothing
    End Function

    ''' <summary>GPU 矩阵计算结果。</summary>
    Public Class CudaMetrics

        ''' <summary>计算所用的设备名称。</summary>
        Public Property device As String
        ''' <summary>行主序的皮尔逊相关矩阵（``rows × rows``）。</summary>
        Public Property correlation As Single()
        ''' <summary>行主序的欧氏距离矩阵（``rows × rows``）。</summary>
        Public Property distance As Single()
        ''' <summary>矩阵行数 / 样本数。</summary>
        Public Property rows As Integer
        ''' <summary>特征列数。</summary>
        Public Property cols As Integer
        ''' <summary>GPU 计算耗时（毫秒）。</summary>
        Public Property elapsedMs As Double

        ''' <summary>取相关矩阵的前 n×n 个单元格，格式化为文本预览。</summary>
        Public Function previewCorrelation(Optional n As Integer = 6) As String
            Return preview(correlation, rows, n)
        End Function

        ''' <summary>取距离矩阵的前 n×n 个单元格，格式化为文本预览。</summary>
        Public Function previewDistance(Optional n As Integer = 6) As String
            Return preview(distance, rows, n)
        End Function

        Private Function preview(values As Single(), rows As Integer, n As Integer) As String
            Dim lines As New List(Of String)

            For i As Integer = 0 To std.Min(n, rows) - 1
                Dim sb As New Text.StringBuilder()

                For j As Integer = 0 To std.Min(n, rows) - 1
                    Call sb.Append(values(i * rows + j).ToString("F4").PadLeft(10))
                Next

                Call lines.Add(sb.ToString)
            Next

            Return String.Join(Environment.NewLine, lines)
        End Function
    End Class
End Module
