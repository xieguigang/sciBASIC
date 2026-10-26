Imports System.Text
Imports IL = Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.IL
Imports Microsoft.VisualBasic.ApplicationServices.VM.JavaScript.Runtime
Imports Microsoft.VisualBasic.Computing.ILCuda.IL2Cuda
Imports Microsoft.VisualBasic.Computing.ILCuda.Runtime
Imports Microsoft.VisualBasic.Computing.ILCuda.Kernels
Imports std = System.Math

Namespace Global.Microsoft.VisualBasic.Computing.JSCuda

    ''' <summary>
    ''' The script facing façade of the JavaScript &gt; CUDA pipeline: translate a script function,
    ''' inspect the generated device code, allocate device buffers, launch the kernel and read the
    ''' result back.
    ''' <para>
    ''' From a script: <c>import Microsoft.VisualBasic.Computing.JSCuda as jscuda from '&lt;dll&gt;'</c>
    ''' then <c>jscuda.JsCudaHost.translate(fn)</c>,
    ''' <c>jscuda.JsCudaHost.createEngine()</c>, <c>jscuda.JsCudaHost.buffer(values)</c>,
    ''' <c>jscuda.JsCudaHost.launch2D(...)</c>, <c>jscuda.JsCudaHost.read(buffer)</c>.
    ''' </para>
    ''' <para>
    ''' Everything here is CUDA plumbing: no algorithm is implemented in this class, the maths
    ''' lives in the script's own kernel function. That is what keeps the engine and the demos
    ''' independent of each other.
    ''' </para>
    ''' </summary>
    Public Class JsCudaHost

        Private Sub New()
        End Sub

        ' ======================================================== translation

        ''' <summary>
        ''' Translate a script function value into a CUDA kernel (<c>js_&lt;name&gt;_kernel</c>).
        ''' </summary>
        Public Shared Function translate(function_ As Object,
                                         Optional options As JsKernelOptions = Nothing) As IlCudaKernel
            Return JsCudaTranslator.Translate(function_, options)
        End Function

        ''' <summary>Translate JavaScript source text (one function declaration) into a CUDA kernel.</summary>
        Public Shared Function translateSource(source As String,
                                               Optional options As JsKernelOptions = Nothing) As IlCudaKernel
            Return JsCudaTranslator.TranslateSource(source, options)
        End Function

        ''' <summary>The generated <c>.cu</c> source of a kernel (header comments included).</summary>
        Public Shared Function cudaSource(kernel As IlCudaKernel) As String
            If kernel Is Nothing Then Throw New ArgumentNullException(NameOf(kernel))

            Return kernel.Source
        End Function

        ''' <summary>The recovered pseudo code of the typed AST (debugging aid).</summary>
        Public Shared Function pseudoCode(kernel As IlCudaKernel) As String
            If kernel Is Nothing OrElse kernel.Syntax Is Nothing Then Return ""

            Return IL.SyntaxWriter.WriteMethod(kernel.Syntax)
        End Function

        ''' <summary>
        ''' Register the kernel with the framework. It must be called before the engine is created
        ''' (the sources are compiled as one NVRTC unit).
        ''' </summary>
        Public Shared Function register(kernel As IlCudaKernel) As String
            If kernel Is Nothing Then Throw New ArgumentNullException(NameOf(kernel))

            Call kernel.Register()

            Return kernel.Name
        End Function

        ''' <summary>Thread index mode of the kernel (<c>Grid1D</c> / <c>Grid2D</c> / <c>None</c>).</summary>
        Public Shared Function indexMode(kernel As IlCudaKernel) As String
            If kernel Is Nothing Then Return ""

            Return kernel.IndexMode.ToString()
        End Function

        ' ======================================================== device

        ''' <summary>Is a usable CUDA device + NVRTC available in this process?</summary>
        Public Shared Function available() As Boolean
            Dim options As New EngineOptions()

            Try
                Using engine As CudaEngine = CudaEngine.TryCreate(options)
                    Return engine IsNot Nothing
                End Using
            Catch ex As Exception
                Return False
            End Try
        End Function

        ''' <summary>Name of the CUDA device used by this process, or <c>""</c> when unavailable.</summary>
        Public Shared Function deviceName() As String
            Dim options As New EngineOptions()

            Try
                Using engine As CudaEngine = CudaEngine.TryCreate(options)
                    If engine Is Nothing Then Return ""

                    Return engine.Device.Name
                End Using
            Catch ex As Exception
                Return ""
            End Try
        End Function

        ''' <summary>Human readable probe report: the device, or why no device could be created.</summary>
        Public Shared Function diagnostics() As String()
            Dim lines As New List(Of String)()
            Dim options As New EngineOptions()

            Try
                Using engine As CudaEngine = CudaEngine.TryCreate(options)
                    If engine IsNot Nothing Then
                        Call lines.Add("device      : " & engine.Device.Name)

                        Dim registered As New List(Of String)()

                        For Each file As KernelSourceFile In KernelSources.Registered()
                            Call registered.Add(file.Name)
                        Next

                        Call lines.Add("kernels     : " & String.Join(", ", registered))

                        Return lines.ToArray()
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

            Return lines.ToArray()
        End Function

        ''' <summary>Create the CUDA engine (register the kernels first).</summary>
        Public Shared Function createEngine(Optional deviceOrdinal As Integer = 0) As CudaEngine
            Dim options As New EngineOptions With {.DeviceOrdinal = deviceOrdinal}
            Dim engine As CudaEngine = CudaEngine.TryCreate(options)

            If engine Is Nothing Then
                Throw New InvalidOperationException("CUDA is not available: " & options.ErrorMessage)
            End If

            Return engine
        End Function

        ''' <summary>Block until all the work submitted to the engine finished.</summary>
        Public Shared Sub synchronize(engine As CudaEngine)
            If engine Is Nothing Then Return

            Call engine.Synchronize()
        End Sub

        ' ======================================================== buffers

        ''' <summary>Allocate a device buffer holding <paramref name="values"/> (double precision).</summary>
        Public Shared Function buffer(values As Double()) As DeviceBuffer(Of Double)
            If values Is Nothing Then Throw New ArgumentNullException(NameOf(values))

            Dim allocated As New DeviceBuffer(Of Double)(values.Length)

            Call allocated.Write(values)

            Return allocated
        End Function

        ''' <summary>Allocate a zero filled device buffer of <paramref name="count"/> doubles.</summary>
        Public Shared Function bufferSize(count As Integer) As DeviceBuffer(Of Double)
            If count <= 0 Then Throw New ArgumentOutOfRangeException(NameOf(count))

            Return New DeviceBuffer(Of Double)(count)
        End Function

        ''' <summary>Read a device buffer back to the host.</summary>
        Public Shared Function read(memory As DeviceMemory) As Double()
            If memory Is Nothing Then Throw New ArgumentNullException(NameOf(memory))

            Dim typed = TryCast(memory, DeviceBuffer(Of Double))

            If typed Is Nothing Then
                Throw New ArgumentException(
                    "read() expects a double precision device buffer created by buffer()/bufferSize()")
            End If

            Return typed.Read()
        End Function

        ' ======================================================== launch

        ''' <summary>
        ''' Launch a 1D kernel: <paramref name="totalElements"/> threads, the output buffer and the
        ''' kernel arguments given in signature order (thread index parameters excluded).
        ''' </summary>
        Public Shared Sub launch1D(kernel As IlCudaKernel, engine As CudaEngine,
                                   totalElements As Integer,
                                   blockSize As Integer,
                                   output As DeviceMemory,
                                   ParamArray args As Object())
            If kernel Is Nothing Then Throw New ArgumentNullException(NameOf(kernel))
            If engine Is Nothing Then Throw New ArgumentNullException(NameOf(engine))

            Dim marshalled = Marshal(kernel, args)
            Dim config = LaunchPlanner.For1D(totalElements, If(blockSize > 0, blockSize, 256))

            Call Launch(kernel, engine, config, marshalled, output, totalElements)
        End Sub

        ''' <summary>
        ''' Launch a 2D kernel over a <paramref name="rows"/> × <paramref name="cols"/> matrix.
        ''' </summary>
        Public Shared Sub launch2D(kernel As IlCudaKernel, engine As CudaEngine,
                                   rows As Integer, cols As Integer,
                                   tile As Integer,
                                   output As DeviceMemory,
                                   ParamArray args As Object())
            If kernel Is Nothing Then Throw New ArgumentNullException(NameOf(kernel))
            If engine Is Nothing Then Throw New ArgumentNullException(NameOf(engine))

            Dim marshalled = Marshal(kernel, args)
            Dim config = LaunchPlanner.For2D(rows, cols, If(tile > 0, tile, 16), If(tile > 0, tile, 16))

            Call Launch(kernel, engine, config, marshalled, output, rows, cols)
        End Sub

        Private Shared Sub Launch(kernel As IlCudaKernel, engine As CudaEngine, config As LaunchConfig,
                                  marshalled As List(Of Object), output As DeviceMemory,
                                  ParamArray counts As Integer())
            Dim all As New List(Of Object)(marshalled)

            Call all.Add(output)

            For Each count As Integer In counts
                Call all.Add(count)
            Next

            Call kernel.Launch(engine, config, all.ToArray)
            Call engine.Synchronize()
        End Sub

        ''' <summary>
        ''' Coerce the script arguments to the types the kernel signature expects: JavaScript
        ''' numbers are doubles, so an <c>int</c> parameter must be converted explicitly, otherwise
        ''' the launcher would write 8 bytes into a 4 byte parameter slot.
        ''' </summary>
        Private Shared Function Marshal(kernel As IlCudaKernel, args As Object()) As List(Of Object)
            Dim typed = JsCudaTranslator.FindKernel(kernel)
            Dim kinds = JsCudaTranslator.ParameterKinds(kernel)
            Dim values As Object() = If(args, New Object() {})
            Dim elementWise As Boolean = typed IsNot Nothing AndAlso typed.ElementWise

            If kinds.Length <> values.Length Then
                Throw New ArgumentException(
                    $"kernel '{kernel.Name}' expects {kinds.Length} argument(s) " &
                    $"({String.Join(", ", kinds.Select(Function(k) k.Name & ":" & k.Kind.ToString()))}) " &
                    $"but {values.Length} were given")
            End If

            Dim marshalled As New List(Of Object)()

            For i As Integer = 0 To kinds.Length - 1
                Dim kind = kinds(i).Kind
                Dim argument = values(i)

                ' an element-wise kernel reads every scalar parameter at the thread index, so the
                ' host must hand over a buffer even though the device function takes a scalar
                If kind = JsKernelParamKind.Buffer OrElse
                   (kind = JsKernelParamKind.Number AndAlso elementWise) Then
                    Dim memory = TryCast(argument, DeviceMemory)

                    If memory Is Nothing Then
                        Throw New ArgumentException(
                            $"kernel '{kernel.Name}': argument {i + 1} ('{kinds(i).Name}') must be a device buffer")
                    End If

                    Call marshalled.Add(memory)
                ElseIf kind = JsKernelParamKind.Int32 Then
                    Call marshalled.Add(CInt(JsRuntime.JsNum(argument)))
                Else
                    Call marshalled.Add(JsRuntime.JsNum(argument))
                End If
            Next

            Return marshalled
        End Function

        ' ======================================================== comparison helpers

        ''' <summary>Largest absolute difference between the GPU result and the CPU reference.</summary>
        Public Shared Function maxError(gpu As Double(), cpu As Double()) As Double
            If gpu Is Nothing OrElse cpu Is Nothing Then Return Double.NaN

            Dim n As Integer = std.Min(gpu.Length, cpu.Length)
            Dim maxDiff As Double = 0

            For i As Integer = 0 To n - 1
                Dim diff As Double = std.Abs(gpu(i) - cpu(i))

                If diff > maxDiff Then
                    maxDiff = diff
                End If
            Next

            Return maxDiff
        End Function

        ''' <summary>Format the top-left <paramref name="n"/> × <paramref name="n"/> cells of a matrix.</summary>
        Public Shared Function preview(values As Double(), rows As Integer, Optional n As Integer = 6) As String
            If values Is Nothing Then Return ""

            Dim lines As New List(Of String)()

            For i As Integer = 0 To std.Min(n, rows) - 1
                Dim line As New StringBuilder()

                For j As Integer = 0 To std.Min(n, rows) - 1
                    Call line.Append(values(i * rows + j).ToString("F4").PadLeft(10))
                Next

                Call lines.Add(line.ToString())
            Next

            Return String.Join(Environment.NewLine, lines)
        End Function

    End Class

End Namespace
