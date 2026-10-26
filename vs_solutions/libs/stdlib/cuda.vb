Imports System.Diagnostics
Imports System.Reflection
Imports Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.IL
Imports Microsoft.VisualBasic.Computing.ILCuda.IL2Cuda
Imports Microsoft.VisualBasic.Computing.ILCuda.Runtime
Imports std = System.Math

Namespace stdlib

    ''' <summary>
    ''' IL → CUDA 的通用脚本接口：把任意 <c>Shared</c> VB.NET 方法编译成 CUDA 内核、注册、创建引擎、
    ''' 分配显存、启动内核、读回结果。
    '''
    ''' <para>
    ''' 这里**不包含任何算法**：需要 GPU 计算的函数写在脚本自己（或调用方自己）的类型里，
    ''' 脚本把 <see cref="System.Reflection.MethodInfo"/> 交给 <see cref="compile"/> 即可。
    ''' 这样标准库与具体 demo 的数学内容解耦：同一个 <c>cuda.compile</c> 既能跑皮尔逊相关，
    ''' 也能跑任意逐元素公式。
    ''' </para>
    '''
    ''' <para>
    ''' 使用顺序（内核源码必须在创建引擎之前注册，它们会被合成一个 NVRTC 编译单元）：
    ''' <code>
    ''' dim k = cuda.compile(GetType(MyKernels).GetMethod("RowSum"))
    ''' call cuda.register(k)
    ''' dim engine = cuda.createEngine()
    ''' dim x = cuda.buffer(values)        ' DeviceBuffer(Of Single)
    ''' dim out = cuda.bufferSize(rows)
    ''' call cuda.launch1D(k, engine, rows, 256, x, cols, out, rows)
    ''' dim result = cuda.read(out)
    ''' </code>
    ''' </para>
    ''' </summary>
    Public Module cuda

#Region "设备探测"

        ''' <summary>当前进程是否可以使用 CUDA 设备 + NVRTC。</summary>
        Public Function available() As Boolean
            Dim options As New EngineOptions()

            Try
                Using engine As CudaEngine = CudaEngine.TryCreate(options)
                    Return engine IsNot Nothing
                End Using
            Catch ex As Exception
                Return False
            End Try
        End Function

        ''' <summary>探测报告：可用时给出设备名，不可用时给出失败原因。</summary>
        Public Function diagnostics() As String()
            Dim lines As New List(Of String)
            Dim options As New EngineOptions()

            Try
                Using engine As CudaEngine = CudaEngine.TryCreate(options)
                    If engine IsNot Nothing Then
                        Call lines.Add("device      : " & engine.Device.Name)

                        Return lines.ToArray
                    End If
                End Using
            Catch ex As Exception
                Call lines.Add("engine error: " & ex.Message)
            End Try

            If Not String.IsNullOrEmpty(options.ErrorMessage) Then
                Call lines.Add("error       : " & options.ErrorMessage)
            End If

            If options.Diagnostics IsNot Nothing Then
                For Each item As String In options.Diagnostics
                    Call lines.Add("  " & item)
                Next
            End If

            Return lines.ToArray
        End Function

        ''' <summary>当前 CUDA 设备名称（不可用时返回空字符串）。</summary>
        Public Function deviceName() As String
            Dim options As New EngineOptions()

            Try
                Using engine As CudaEngine = CudaEngine.TryCreate(options)
                    If engine Is Nothing Then
                        Return ""
                    End If

                    Return engine.Device.Name
                End Using
            Catch ex As Exception
                Return ""
            End Try
        End Function

        ''' <summary>创建 CUDA 引擎（必须先 <see cref="register"/> 全部内核）。</summary>
        Public Function createEngine(Optional deviceOrdinal As Integer = 0) As CudaEngine
            Dim options As New EngineOptions With {.DeviceOrdinal = deviceOrdinal}
            Dim engine As CudaEngine = CudaEngine.TryCreate(options)

            If engine Is Nothing Then
                Throw New InvalidOperationException("CUDA is not available: " & options.ErrorMessage)
            End If

            Return engine
        End Function

#End Region

#Region "翻译"

        ''' <summary>
        ''' 把一个已编译的 <c>Shared</c> 方法反编译并翻译成 CUDA 内核（<c>__device__</c> 标量函数 +
        ''' <c>__global__</c> 内核）。方法体里不要出现业务之外的东西——它就是设备代码本身。
        ''' </summary>
        ''' <param name="method">目标方法，必须是 <c>Shared</c></param>
        ''' <param name="name">可选的内核名；留空时用 <c>il_&lt;方法名&gt;_kernel</c></param>
        Public Function compile(method As MethodInfo, Optional name As String = Nothing) As IlCudaKernel
            If method Is Nothing Then
                Throw New ArgumentNullException(NameOf(method))
            End If

            Return IlCudaTranslator.Translate(method, name)
        End Function

        ''' <summary>把内核源码注册进框架（必须在创建引擎之前调用）。</summary>
        Public Function register(kernel As IlCudaKernel) As String
            If kernel Is Nothing Then
                Throw New ArgumentNullException(NameOf(kernel))
            End If

            Call kernel.Register()

            Return kernel.Name
        End Function

        ''' <summary>内核生成的 .cu 源码。</summary>
        Public Function kernelSource(kernel As IlCudaKernel) As String
            Return If(kernel Is Nothing, Nothing, kernel.Source)
        End Function

        ''' <summary>内核的线程索引模式（<c>Grid1D</c> / <c>Grid2D</c> / <c>None</c>）。</summary>
        Public Function kernelIndexMode(kernel As IlCudaKernel) As String
            Return If(kernel Is Nothing, Nothing, kernel.IndexMode.ToString)
        End Function

        ''' <summary>反编译得到的方法伪代码（调试用）。</summary>
        Public Function kernelPseudoCode(kernel As IlCudaKernel) As String
            If kernel Is Nothing OrElse kernel.Syntax Is Nothing Then
                Return Nothing
            End If

            Return SyntaxWriter.WriteMethod(kernel.Syntax)
        End Function

        ''' <summary>
        ''' 用 AST 解释器对内核做一次自检（解释执行结果 vs 直接调用原方法的期望值）。
        ''' </summary>
        ''' <returns>形如 ``内核名: interpreter diff=... OK|FAIL`` 的一行诊断文本</returns>
        Public Function selfCheck(kernel As IlCudaKernel, args As Object(), expected As Double) As String
            If kernel Is Nothing Then
                Return "self check: no kernel"
            End If
            If kernel.Syntax Is Nothing Then
                Return kernel.Name & ": not translated"
            End If

            Dim actual As Object

            Try
                actual = New AstInterpreter(kernel.Syntax).Invoke(args)
            Catch ex As Exception
                Return kernel.Name & ": interpreter error " & ex.Message
            End Try

            Dim diff As Double = std.Abs(expected - Convert.ToDouble(actual))
            Dim tolerance As Double = 1.0E-6 * std.Max(1.0, std.Abs(expected))

            Return $"{kernel.Name}: interpreter diff={diff.ToString("E2")} {If(diff <= tolerance, "OK", "FAIL")}"
        End Function

#End Region

#Region "显存"

        ''' <summary>分配单精度显存缓冲并上传数据。</summary>
        Public Function buffer(values As Single()) As DeviceBuffer(Of Single)
            If values Is Nothing Then
                Throw New ArgumentNullException(NameOf(values))
            End If

            Dim allocated As New DeviceBuffer(Of Single)(values.Length)

            Call allocated.Write(values)

            Return allocated
        End Function

        ''' <summary>分配双精度显存缓冲并上传数据。</summary>
        Public Function bufferDouble(values As Double()) As DeviceBuffer(Of Double)
            If values Is Nothing Then
                Throw New ArgumentNullException(NameOf(values))
            End If

            Dim allocated As New DeviceBuffer(Of Double)(values.Length)

            Call allocated.Write(values)

            Return allocated
        End Function

        ''' <summary>分配单精度显存缓冲（零填充）。</summary>
        Public Function bufferSize(count As Integer) As DeviceBuffer(Of Single)
            If count <= 0 Then
                Throw New ArgumentOutOfRangeException(NameOf(count))
            End If

            Return New DeviceBuffer(Of Single)(count)
        End Function

        ''' <summary>分配双精度显存缓冲（零填充）。</summary>
        Public Function bufferSizeDouble(count As Integer) As DeviceBuffer(Of Double)
            If count <= 0 Then
                Throw New ArgumentOutOfRangeException(NameOf(count))
            End If

            Return New DeviceBuffer(Of Double)(count)
        End Function

        ''' <summary>把单精度显存缓冲读回主机。</summary>
        Public Function read(buffer As DeviceMemory) As Single()
            Dim typed = TryCast(buffer, DeviceBuffer(Of Single))

            If typed Is Nothing Then
                Throw New ArgumentException("read() 需要 cuda.buffer/cuda.bufferSize 创建的单精度缓冲")
            End If

            Return typed.Read()
        End Function

        ''' <summary>把双精度显存缓冲读回主机。</summary>
        Public Function readDouble(buffer As DeviceMemory) As Double()
            Dim typed = TryCast(buffer, DeviceBuffer(Of Double))

            If typed Is Nothing Then
                Throw New ArgumentException("readDouble() 需要 cuda.bufferDouble/cuda.bufferSizeDouble 创建的双精度缓冲")
            End If

            Return typed.Read()
        End Function

#End Region

#Region "启动"

        ''' <summary>
        ''' 一维启动：<paramref name="args"/> 按内核签名顺序给出（线程索引参数不在其中），
        ''' 末尾由框架补上输出数组与元素个数。
        ''' </summary>
        Public Sub launch1D(kernel As IlCudaKernel, engine As CudaEngine,
                            totalElements As Integer, blockSize As Integer,
                            output As DeviceMemory,
                            ParamArray args As Object())
            If kernel Is Nothing Then Throw New ArgumentNullException(NameOf(kernel))
            If engine Is Nothing Then Throw New ArgumentNullException(NameOf(engine))

            Dim all As New List(Of Object)(args)

            Call all.Add(output)
            Call all.Add(totalElements)
            Call kernel.Launch(engine, LaunchPlanner.For1D(totalElements, If(blockSize > 0, blockSize, 256)), all.ToArray)
        End Sub

        ''' <summary>二维启动：<paramref name="rows"/> × <paramref name="cols"/> 的输出矩阵。</summary>
        Public Sub launch2D(kernel As IlCudaKernel, engine As CudaEngine,
                            rows As Integer, cols As Integer, tile As Integer,
                            output As DeviceMemory,
                            ParamArray args As Object())
            If kernel Is Nothing Then Throw New ArgumentNullException(NameOf(kernel))
            If engine Is Nothing Then Throw New ArgumentNullException(NameOf(engine))

            Dim size As Integer = If(tile > 0, tile, 16)
            Dim all As New List(Of Object)(args)

            Call all.Add(output)
            Call all.Add(rows)
            Call all.Add(cols)
            Call kernel.Launch(engine, LaunchPlanner.For2D(rows, cols, size, size), all.ToArray)
        End Sub

        ''' <summary>阻塞等待引擎上已提交的计算全部完成（读回结果之前调用）。</summary>
        Public Sub synchronize(engine As CudaEngine)
            If engine Is Nothing Then Return

            Call engine.Synchronize()
        End Sub

#End Region

#Region "工具函数"

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

#End Region

    End Module

End Namespace
