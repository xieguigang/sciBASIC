Imports System.Diagnostics
Imports System.Reflection
Imports Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.IL
Imports Microsoft.VisualBasic.Computing.ILCuda.IL2Cuda
Imports Microsoft.VisualBasic.Computing.ILCuda.Runtime
Imports std = System.Math

' ---------------------------------------------------------------------------
'  IL -> CUDA 的通用脚本接口
'
'  这里**不包含任何算法**：需要 GPU 计算的函数写在脚本自己的类型里（或调用方自己的程序集里），
'  脚本把 MethodInfo 交给 cuda.compile 即可。标准库只提供与业务无关的管线：
'  编译 / 注册 / 设备 / 显存 / 启动 / 读回 / 自检。
'
'  注意：脚本可见的类型全部是本程序集自己的门面类型（CudaKernel / CudaBuffer / CudaSession），
'  这样脚本只需要 #include "stdlib.dll"，不需要再引用 ILCuda 程序集。
' ---------------------------------------------------------------------------

''' <summary>
''' IL → CUDA 的通用脚本接口：把任意 <c>Shared</c> VB.NET 方法编译成 CUDA 内核、注册、创建引擎、
''' 分配显存、启动内核、读回结果。
'''
''' <para>
''' 使用顺序（内核源码必须在创建引擎之前注册，它们会被合成一个 NVRTC 编译单元）：
''' <code>
''' dim k = cuda.compile(GetType(MyKernels).GetMethod("RowSum"))
''' call k.register()
''' dim session = cuda.createEngine()
''' dim x = cuda.buffer(values)          ' CudaBuffer（单精度）
''' dim out = cuda.bufferSize(rows)
''' call cuda.launch1D(k, session, rows, 256, out, x, cols)
''' dim result = cuda.read(out)
''' call session.dispose()
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

    ''' <summary>创建 CUDA 会话（必须先 <c>register</c> 全部内核）。</summary>
    Public Function createEngine(Optional deviceOrdinal As Integer = 0) As CudaSession
        Dim options As New EngineOptions With {.DeviceOrdinal = deviceOrdinal}
        Dim engine As CudaEngine = CudaEngine.TryCreate(options)

        If engine Is Nothing Then
            Throw New InvalidOperationException("CUDA is not available: " & options.ErrorMessage)
        End If

        Return New CudaSession(engine)
    End Function

#End Region

#Region "翻译"

    ''' <summary>
    ''' 把一个已编译的 <c>Shared</c> 方法反编译并翻译成 CUDA 内核（<c>__device__</c> 标量函数 +
    ''' <c>__global__</c> 内核）。方法体里不要出现业务之外的东西——它就是设备代码本身。
    ''' </summary>
    ''' <param name="method">目标方法，必须是 <c>Shared</c></param>
    ''' <param name="name">可选的内核名；留空时用 <c>il_&lt;方法名&gt;_kernel</c></param>
    Public Function compile(method As MethodInfo, Optional name As String = Nothing) As CudaKernel
        If method Is Nothing Then
            Throw New ArgumentNullException(NameOf(method))
        End If

        Return New CudaKernel(IlCudaTranslator.Translate(method, name))
    End Function

    ''' <summary>把内核源码注册进框架（必须在创建引擎之前调用）。</summary>
    Public Function register(kernel As CudaKernel) As String
        If kernel Is Nothing Then
            Throw New ArgumentNullException(NameOf(kernel))
        End If

        Call kernel.register()

        Return kernel.name
    End Function

    ''' <summary>
    ''' 用 AST 解释器对内核做一次自检（解释执行结果 vs 直接调用原方法的期望值）。
    ''' </summary>
    ''' <returns>形如 ``内核名: interpreter diff=... OK|FAIL`` 的一行诊断文本</returns>
    Public Function selfCheck(kernel As CudaKernel, args As Object(), expected As Double) As String
        If kernel Is Nothing Then
            Return "self check: no kernel"
        End If

        Dim syntax As MethodSyntax = kernel.Syntax

        If syntax Is Nothing Then
            Return kernel.name & ": not translated"
        End If

        Dim actual As Object

        Try
            actual = New AstInterpreter(syntax).Invoke(args)
        Catch ex As Exception
            Return kernel.name & ": interpreter error " & ex.Message
        End Try

        Dim diff As Double = std.Abs(expected - Convert.ToDouble(actual))
        Dim tolerance As Double = 1.0E-6 * std.Max(1.0, std.Abs(expected))

        Return $"{kernel.name}: interpreter diff={diff.ToString("E2")} {If(diff <= tolerance, "OK", "FAIL")}"
    End Function

#End Region

#Region "显存"

    ''' <summary>分配单精度显存缓冲并上传数据。</summary>
    Public Function buffer(values As Single()) As CudaBuffer
        If values Is Nothing Then
            Throw New ArgumentNullException(NameOf(values))
        End If

        Dim allocated As New DeviceBuffer(Of Single)(values.Length)

        Call allocated.Write(values)

        Return New CudaBuffer(allocated, GetType(Single), values.Length)
    End Function

    ''' <summary>分配双精度显存缓冲并上传数据。</summary>
    Public Function bufferDouble(values As Double()) As CudaBuffer
        If values Is Nothing Then
            Throw New ArgumentNullException(NameOf(values))
        End If

        Dim allocated As New DeviceBuffer(Of Double)(values.Length)

        Call allocated.Write(values)

        Return New CudaBuffer(allocated, GetType(Double), values.Length)
    End Function

    ''' <summary>分配单精度显存缓冲（零填充）。</summary>
    Public Function bufferSize(count As Integer) As CudaBuffer
        If count <= 0 Then
            Throw New ArgumentOutOfRangeException(NameOf(count))
        End If

        Return New CudaBuffer(New DeviceBuffer(Of Single)(count), GetType(Single), count)
    End Function

    ''' <summary>分配双精度显存缓冲（零填充）。</summary>
    Public Function bufferSizeDouble(count As Integer) As CudaBuffer
        If count <= 0 Then
            Throw New ArgumentOutOfRangeException(NameOf(count))
        End If

        Return New CudaBuffer(New DeviceBuffer(Of Double)(count), GetType(Double), count)
    End Function

    ''' <summary>把单精度显存缓冲读回主机。</summary>
    Public Function read(buffer As CudaBuffer) As Single()
        If buffer Is Nothing Then
            Throw New ArgumentNullException(NameOf(buffer))
        End If

        Dim typed = TryCast(buffer.Memory, DeviceBuffer(Of Single))

        If typed Is Nothing Then
            Throw New ArgumentException("read() 需要 cuda.buffer/cuda.bufferSize 创建的单精度缓冲")
        End If

        Return typed.Read()
    End Function

    ''' <summary>把双精度显存缓冲读回主机。</summary>
    Public Function readDouble(buffer As CudaBuffer) As Double()
        If buffer Is Nothing Then
            Throw New ArgumentNullException(NameOf(buffer))
        End If

        Dim typed = TryCast(buffer.Memory, DeviceBuffer(Of Double))

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
    Public Sub launch1D(kernel As CudaKernel, session As CudaSession,
                        totalElements As Integer, blockSize As Integer,
                        output As CudaBuffer,
                        ParamArray args As Object())
        If kernel Is Nothing Then Throw New ArgumentNullException(NameOf(kernel))
        If session Is Nothing Then Throw New ArgumentNullException(NameOf(session))
        If output Is Nothing Then Throw New ArgumentNullException(NameOf(output))

        Dim all As List(Of Object) = unwrap(args)

        Call all.Add(output.Memory)
        Call all.Add(totalElements)
        Call kernel.Kernel.Launch(session.Engine,
                                  LaunchPlanner.For1D(totalElements, If(blockSize > 0, blockSize, 256)),
                                  all.ToArray)
    End Sub

    ''' <summary>二维启动：<paramref name="rows"/> × <paramref name="cols"/> 的输出矩阵。</summary>
    Public Sub launch2D(kernel As CudaKernel, session As CudaSession,
                        rows As Integer, cols As Integer, tile As Integer,
                        output As CudaBuffer,
                        ParamArray args As Object())
        If kernel Is Nothing Then Throw New ArgumentNullException(NameOf(kernel))
        If session Is Nothing Then Throw New ArgumentNullException(NameOf(session))
        If output Is Nothing Then Throw New ArgumentNullException(NameOf(output))

        Dim size As Integer = If(tile > 0, tile, 16)
        Dim all As List(Of Object) = unwrap(args)

        Call all.Add(output.Memory)
        Call all.Add(rows)
        Call all.Add(cols)
        Call kernel.Kernel.Launch(session.Engine, LaunchPlanner.For2D(rows, cols, size, size), all.ToArray)
    End Sub

    ''' <summary>
    ''' 把脚本侧的 <see cref="CudaBuffer"/> 门面换成内核真正需要的显存对象；
    ''' 其它参数（整数 / 单精度 / 双精度标量）原样传递。
    ''' </summary>
    Private Function unwrap(args As Object()) As List(Of Object)
        Dim all As New List(Of Object)()

        If args IsNot Nothing Then
            For Each item As Object In args
                Dim buffer = TryCast(item, CudaBuffer)

                If buffer IsNot Nothing Then
                    Call all.Add(buffer.Memory)
                Else
                    Call all.Add(item)
                End If
            Next
        End If

        Return all
    End Function

    ''' <summary>阻塞等待会话上已提交的计算全部完成（读回结果之前调用）。</summary>
    Public Sub synchronize(session As CudaSession)
        If session Is Nothing Then Return

        Call session.synchronize()
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

''' <summary>
''' 一个已翻译的 CUDA 内核（<see cref="cuda.compile"/> 的返回值）。
''' <para>
''' 门面类型：脚本可以用它的属性查看生成的设备代码，而不需要引用 ILCuda 程序集。
''' </para>
''' </summary>
Public Class CudaKernel

    Friend ReadOnly Kernel As IlCudaKernel

    Friend Sub New(kernel As IlCudaKernel)
        Me.Kernel = kernel
    End Sub

    ''' <summary>内核函数名（与 .cu 中 <c>extern "C"</c> 的名字一致）。</summary>
    Public ReadOnly Property name As String
        Get
            Return Kernel.Name
        End Get
    End Property

    ''' <summary><c>__device__</c> 标量函数名。</summary>
    Public ReadOnly Property deviceFunction As String
        Get
            Return Kernel.DeviceFunctionName
        End Get
    End Property

    ''' <summary>线程索引模式（<c>Grid1D</c> / <c>Grid2D</c> / <c>None</c>）。</summary>
    Public ReadOnly Property indexMode As String
        Get
            Return Kernel.IndexMode.ToString
        End Get
    End Property

    ''' <summary>生成的完整 .cu 源码。</summary>
    Public ReadOnly Property source As String
        Get
            Return Kernel.Source
        End Get
    End Property

    ''' <summary>反编译得到的方法伪代码（调试用）。</summary>
    Public ReadOnly Property pseudoCode As String
        Get
            If Kernel.Syntax Is Nothing Then Return ""

            Return SyntaxWriter.WriteMethod(Kernel.Syntax)
        End Get
    End Property

    ''' <summary>反编译出来的 AST（供 <see cref="cuda.selfCheck"/> 使用）。</summary>
    Public ReadOnly Property Syntax As MethodSyntax
        Get
            Return Kernel.Syntax
        End Get
    End Property

    ''' <summary>把内核源码注册进框架（必须在创建引擎之前调用）。</summary>
    Public Sub register()
        Call Kernel.Register()
    End Sub

    Public Overrides Function ToString() As String
        Return Kernel.ToString()
    End Function

End Class

''' <summary>一段显存（<see cref="cuda.buffer"/> / <see cref="cuda.bufferSize"/> 的返回值）。</summary>
Public Class CudaBuffer
    Implements IDisposable

    Friend ReadOnly Memory As DeviceMemory

    ''' <summary>元素个数。</summary>
    Public ReadOnly Property count As Integer
    ''' <summary>元素类型（<c>Single</c> 或 <c>Double</c>）。</summary>
    Public ReadOnly Property elementType As Type

    Friend Sub New(memory As DeviceMemory, elementType As Type, count As Integer)
        Me.Memory = memory
        Me.elementType = elementType
        Me.count = count
    End Sub

    ''' <summary>释放显存（与 Dispose 等价，脚本里名字更短）。</summary>
    Public Sub free()
        Call Memory.Dispose()
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        Call Memory.Dispose()
    End Sub

    Public Overrides Function ToString() As String
        Return $"CudaBuffer[{count}] ({If(elementType Is Nothing, "?", elementType.Name)})"
    End Function

End Class

''' <summary>一个 CUDA 会话（<see cref="cuda.createEngine"/> 的返回值）。</summary>
Public Class CudaSession
    Implements IDisposable

    Friend ReadOnly Engine As CudaEngine

    Friend Sub New(engine As CudaEngine)
        Me.Engine = engine
    End Sub

    ''' <summary>设备名称。</summary>
    Public ReadOnly Property device As String
        Get
            Return Engine.Device.Name
        End Get
    End Property

    ''' <summary>阻塞等待已提交的计算完成。</summary>
    Public Sub synchronize()
        Call Engine.Synchronize()
    End Sub

    ''' <summary>释放设备上下文（与 Dispose 等价，脚本里名字更短）。</summary>
    Public Sub free()
        Call Engine.Dispose()
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        Call Engine.Dispose()
    End Sub

    Public Overrides Function ToString() As String
        Return "CUDA(" & device & ")"
    End Function

End Class
