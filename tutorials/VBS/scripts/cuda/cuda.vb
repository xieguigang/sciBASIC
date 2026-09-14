#include "Microsoft.VisualBasic.Computing.ILCuda.dll"
#include "Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.dll"

imports System.Reflection
imports System.Text
imports System.Text.RegularExpressions
imports Microsoft.VisualBasic.Computing.ILCuda.Runtime
imports Microsoft.VisualBasic.Computing.ILCuda.IL2Cuda
imports Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.IL

' ============================================================================
'  IL -> CUDA tutorial: define plain VB.NET math functions in this script and
'  use them to compute a pairwise Pearson correlation matrix and a Euclidean
'  distance matrix on the GPU.
'
'  Run:
'      vbs.exe tutorials\VBS\cuda.vb
'
'  Pipeline:
'      VB.NET function -> IL bytecode -> AST (MethodSyntax) -> .cu source
'                 -> NVRTC just-in-time compile -> cuLaunchKernel -> result matrix
'
'  Three kernel-mapping conventions (see cuda\ILCuda\README.md for details):
'      1) a parameter named i      -> 1D kernel, i = blockIdx.x * blockDim.x + threadIdx.x
'      2) parameters i and j       -> 2D kernel, i indexes the row (blockIdx.y) and j the column (blockIdx.x)
'      3) pure scalars, no array element access -> automatically wrapped element-wise; every scalar
'         parameter becomes an array that is read by the element index
'
'  Note: because the VBS engine rewrites top-level Function blocks into anonymous
'  functions, their Shared MethodInfo is not reachable, so every method to be
'  decompiled is placed inside the Public Class below -- the type block is moved
'  verbatim into the generated Module and can be reflected normally.
' ============================================================================

' ---------------------------------------------------------------------------
'  Section 1: target functions to decompile
'
'  Everything below is ordinary VB.NET and contains no CUDA concepts. IL2Cuda
'  decompiles these methods into expression trees at runtime and then emits .cu
'  source from them.
'
'  For row i let sum_i = Σx, sq_i = Σx², dot_ij = Σx_ik·x_jk and n = column count:
'      mean_i  = sum_i / n
'      var_i   = sq_i - n * mean_i²
'      corr_ij = (dot_ij - n * mean_i * mean_j) / sqrt(var_i * var_j)
'      dist_ij = sqrt(sq_i + sq_j - 2 * dot_ij)
'
'  On the diagonal corr is mathematically 1 and dist is 0; those cells are
'  returned directly so that subtracting two close large numbers in single
'  precision cannot amplify the rounding error (see the numeric notes in README).
' ---------------------------------------------------------------------------

Public Class PearsonMetrics

    ' Row sum; convention 1: the i parameter is the thread index -> 1D kernel
    Public Shared Function RowSum(x As Single(), cols As Integer, i As Integer) As Single
        Dim sum As Single = 0.0F

        For k As Integer = 0 To cols - 1
            sum += x(i * cols + k)
        Next

        Return sum
    End Function

    ' Row sum of squares; also a 1D kernel
    Public Shared Function RowSumSq(x As Single(), cols As Integer, i As Integer) As Single
        Dim sumSq As Single = 0.0F

        For k As Integer = 0 To cols - 1
            Dim v As Single = x(i * cols + k)
            sumSq += v * v
        Next

        Return sumSq
    End Function

    ' Dot product of two rows; convention 2: both i and j are present -> 2D kernel
    Public Shared Function GramDot(x As Single(), cols As Integer, i As Integer, j As Integer) As Single
        Dim acc As Single = 0.0F

        For k As Integer = 0 To cols - 1
            acc += x(i * cols + k) * x(j * cols + k)
        Next

        Return acc
    End Function

    ' Pearson correlation cell: guard clause + if/else diamond + MathF calls
    Public Shared Function CorrelationCell(dot As Single(), rowSum As Single(), rowSumSq As Single(),
                                           rows As Integer, cols As Integer,
                                           i As Integer, j As Integer) As Single
        If i = j Then
            Return 1.0F
        End If

        Dim n As Single = CSng(cols)
        Dim meanI As Single = rowSum(i) / n
        Dim meanJ As Single = rowSum(j) / n
        Dim varI As Single = rowSumSq(i) - n * meanI * meanI
        Dim varJ As Single = rowSumSq(j) - n * meanJ * meanJ
        Dim d As Single = dot(i * rows + j)
        Dim cov As Single = d - n * meanI * meanJ
        Dim denom As Single = MathF.Sqrt(MathF.Max(varI, 0.0F)) * MathF.Sqrt(MathF.Max(varJ, 0.0F))
        Dim c As Single

        If denom > 1.0E-12F Then
            c = cov / denom
        Else
            c = 0.0F
        End If

        Return MathF.Min(1.0F, MathF.Max(-1.0F, c))
    End Function

    ' Euclidean distance cell: guard clause + MathF.Sqrt
    Public Shared Function DistanceCell(dot As Single(), rowSumSq As Single(),
                                        rows As Integer, i As Integer, j As Integer) As Single
        If i = j Then
            Return 0.0F
        End If

        Dim d As Single = dot(i * rows + j)
        Dim d2 As Single = MathF.Max(rowSumSq(i) + rowSumSq(j) - 2.0F * d, 0.0F)

        Return MathF.Sqrt(d2)
    End Function

    ' Pure scalar clamp; convention 3: no i/j and no array element access ->
    ' automatically wrapped element-wise
    Public Shared Function PearsonClamp(cov As Single, denom As Single) As Single
        Dim c As Single

        If denom > 1.0E-12F Then
            c = cov / denom
        Else
            c = 0.0F
        End If

        Return MathF.Min(1.0F, MathF.Max(-1.0F, c))
    End Function
End Class

' ---------------------------------------------------------------------------
'  Section 2: tutorial helpers (target method list / CPU reference / formatting / self-check)
' ---------------------------------------------------------------------------

Public Class TutorialKit

    ''' All target methods to be decompiled
    Public Shared Function Targets() As MethodInfo()
        Dim t As Type = GetType(PearsonMetrics)

        Return New MethodInfo() {
            t.GetMethod("RowSum"),
            t.GetMethod("RowSumSq"),
            t.GetMethod("GramDot"),
            t.GetMethod("CorrelationCell"),
            t.GetMethod("DistanceCell"),
            t.GetMethod("PearsonClamp")
        }
    End Function

    ''' Build a reproducible random matrix (row-major)
    Public Shared Function MakeMatrix(rows As Integer, cols As Integer, seed As Integer) As Single()
        Dim rnd As New Random(seed)
        Dim data(rows * cols - 1) As Single

        For i As Integer = 0 To data.Length - 1
            data(i) = CSng(rnd.NextDouble() * 2.0 - 1.0)
        Next

        Return data
    End Function

    ''' Sample arguments for the interpreter self-check: valid indices, non-degenerate values
    Public Shared Function SampleArgs(m As MethodInfo) As Object()
        Dim ps As ParameterInfo() = m.GetParameters()
        Dim args(ps.Length - 1) As Object

        For i As Integer = 0 To ps.Length - 1
            Dim p As ParameterInfo = ps(i)
            Dim lower As String = p.Name.ToLowerInvariant()

            If p.ParameterType.IsArray Then
                Dim data(255) As Single

                For k As Integer = 0 To 255
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

    ''' CPU reference implementation: Pearson correlation matrix (double-precision two-pass)
    Public Shared Function CpuCorrelation(data As Single(), rows As Integer, cols As Integer) As Double()
        Dim n As Double = cols
        Dim mean(rows - 1) As Double
        Dim sd(rows - 1) As Double

        For i As Integer = 0 To rows - 1
            Dim s As Double = 0
            Dim sq As Double = 0

            For k As Integer = 0 To cols - 1
                Dim v As Double = data(i * cols + k)
                s += v
                sq += v * v
            Next

            mean(i) = s / n
            sd(i) = System.Math.Sqrt(System.Math.Max(sq - n * mean(i) * mean(i), 0.0))
        Next

        Dim out(rows * rows - 1) As Double

        For i As Integer = 0 To rows - 1
            For j As Integer = 0 To rows - 1
                Dim d As Double = 0

                For k As Integer = 0 To cols - 1
                    d += data(i * cols + k) * data(j * cols + k)
                Next

                Dim cov As Double = d - n * mean(i) * mean(j)
                Dim den As Double = sd(i) * sd(j)
                Dim c As Double = If(den > 0.0, cov / den, 0.0)

                out(i * rows + j) = If(i = j, 1.0, System.Math.Min(1.0, System.Math.Max(-1.0, c)))
            Next
        Next

        Return out
    End Function

    ''' CPU reference implementation: Euclidean distance matrix
    Public Shared Function CpuDistance(data As Single(), rows As Integer, cols As Integer) As Double()
        Dim sq(rows - 1) As Double

        For i As Integer = 0 To rows - 1
            Dim s As Double = 0

            For k As Integer = 0 To cols - 1
                Dim v As Double = data(i * cols + k)
                s += v * v
            Next

            sq(i) = s
        Next

        Dim out(rows * rows - 1) As Double

        For i As Integer = 0 To rows - 1
            For j As Integer = 0 To rows - 1
                Dim d As Double = 0

                For k As Integer = 0 To cols - 1
                    d += data(i * cols + k) * data(j * cols + k)
                Next

                out(i * rows + j) = If(i = j, 0.0,
                    System.Math.Sqrt(System.Math.Max(sq(i) + sq(j) - 2.0 * d, 0.0)))
            Next
        Next

        Return out
    End Function

    ''' Maximum absolute error between the GPU result and the CPU reference
    Public Shared Function MaxError(got As Single(), expect As Double()) As Double
        Dim maxDiff As Double = 0

        For i As Integer = 0 To got.Length - 1
            Dim diff As Double = System.Math.Abs(CDbl(got(i)) - expect(i))

            If diff > maxDiff Then
                maxDiff = diff
            End If
        Next

        Return maxDiff
    End Function

    ''' Maximum absolute error between the GPU result and a single-precision reference
    Public Shared Function MaxErrorSingle(got As Single(), expect As Single()) As Double
        Dim maxDiff As Double = 0

        For i As Integer = 0 To got.Length - 1
            Dim diff As Double = System.Math.Abs(CDbl(got(i)) - CDbl(expect(i)))

            If diff > maxDiff Then
                maxDiff = diff
            End If
        Next

        Return maxDiff
    End Function

    ''' Convert double-precision references to single precision so the same preview layout can be reused
    Public Shared Function ToSingle(source As Double()) As Single()
        Dim out(source.Length - 1) As Single

        For i As Integer = 0 To source.Length - 1
            out(i) = CSng(source(i))
        Next

        Return out
    End Function

    ''' Section title
    Public Shared Sub PrintTitle(text As String)
        Call Console.WriteLine()
        Call Console.WriteLine(New String("="c, 74))
        Call Console.WriteLine("  " & text)
        Call Console.WriteLine(New String("="c, 74))
    End Sub

    ''' Print a multi-line text block with indentation (pseudo code / CUDA source)
    Public Shared Sub PrintBlock(text As String, indent As String)
        Dim lines As String() = Regex.Split(text, "\r\n|\r|\n")

        For Each line As String In lines
            Call Console.WriteLine(indent & line)
        Next
    End Sub

    ''' Print the top-left n x n block of a result matrix
    Public Shared Sub PrintMatrix(title As String, m As Single(), rows As Integer, n As Integer)
        Call Console.WriteLine("  " & title)

        For i As Integer = 0 To n - 1
            Dim line As New StringBuilder()

            For j As Integer = 0 To n - 1
                Call line.Append(m(i * rows + j).ToString("F4").PadLeft(10))
            Next

            Call Console.WriteLine("    " & line.ToString())
        Next
    End Sub
End Class

' ---------------------------------------------------------------------------
'  Section 3: main flow
' ---------------------------------------------------------------------------

dim rows As Integer = 1024
dim cols As Integer = 1024
dim seed As Integer = 42
dim preview As Integer = 6
dim allOk As Boolean = True

call TutorialKit.PrintTitle("IL -> CUDA tutorial: Pearson correlation matrix and Euclidean distance matrix")
call console.WriteLine("  input size: " & rows & " rows x " & cols & " cols, random seed " & seed)

' ---------- Build the input matrix ----------
dim data As Single() = TutorialKit.MakeMatrix(rows, cols, seed)

' ---------- Step 1: IL -> AST -> .cu ----------
dim kernels As New Dictionary(Of String, IlCudaKernel)(StringComparer.Ordinal)

call TutorialKit.PrintTitle("Step 1: decompile the VB.NET functions into an AST and emit .cu source")

for each m As MethodInfo In TutorialKit.Targets()
    dim errMsg As String = Nothing
    dim kernel As IlCudaKernel = Nothing

    try
        kernel = IlCudaTranslator.Translate(m)
    catch ex As Exception
        errMsg = ex.GetBaseException().Message
    end try

    if errMsg IsNot Nothing Then
        call console.WriteLine("  " & m.Name & " decompile failed: " & errMsg)
        allOk = False
    else
        call kernels.Add(m.Name, kernel)

        call console.WriteLine()
        call console.WriteLine("  ---- " & m.Name & " ----")
        call console.WriteLine("  index mode : " & kernel.IndexMode.ToString())
        call console.WriteLine("  device func: " & kernel.DeviceFunctionName)
        call console.WriteLine("  kernel     : " & kernel.Name)
        call console.WriteLine()
        call console.WriteLine("  reconstructed pseudo code:")
        call TutorialKit.PrintBlock(SyntaxWriter.WriteMethod(kernel.Syntax), "    ")
        call console.WriteLine()
        call console.WriteLine("  generated CUDA source:")
        call TutorialKit.PrintBlock(kernel.Source, "    ")

        ' Interpreter self-check: run the same AST through the CPU interpreter and
        ' compare the result with a direct invocation of the original method
        dim sampleArgs As Object() = TutorialKit.SampleArgs(m)
        dim expected As Object = m.Invoke(Nothing, sampleArgs)
        dim actual As Object = New AstInterpreter(kernel.Syntax).Invoke(sampleArgs)
        dim diff As Double = System.Math.Abs(System.Convert.ToDouble(expected) - System.Convert.ToDouble(actual))
        dim tol As Double = 1.0E-5 * System.Math.Max(1.0, System.Math.Abs(System.Convert.ToDouble(expected)))
        dim pass As Boolean = diff <= tol

        call console.WriteLine()
        call console.WriteLine("  interpreter self-check: original=" & expected & ", AST=" & actual &
                               ", diff=" & diff.ToString("E3") & "  " & (if(pass, "OK", "FAIL")))

        if Not pass Then
            allOk = False
        end if
    end if
next

' ---------- Step 2: register the generated .cu into KernelSources ----------
call TutorialKit.PrintTitle("Step 2: register the kernel sources (must be done before creating the engine)")

for each k As IlCudaKernel In kernels.Values
    call k.Register()
    call console.WriteLine("  registered " & k.ToString())
next

' ---------- Step 3: NVRTC compile + launch the kernels ----------
call TutorialKit.PrintTitle("Step 3: compute the correlation and distance matrices on the GPU")

dim opts As New EngineOptions With {.DeviceOrdinal = 0}
dim engine As CudaEngine = CudaEngine.TryCreate(opts)
dim corrGPU As Single() = Nothing
dim distGPU As Single() = Nothing
dim gpuOk As Boolean = False

if Not allOk Then
    call console.WriteLine("  Step 1 had failures, skipping the GPU computation.")
elseif engine Is Nothing Then
    call console.WriteLine("  GPU unavailable: " & opts.ErrorMessage)

    dim report = CudaEnvironment.Probe()

    for each s As FixSuggestion In CudaEnvironment.Suggest(report)
        call console.WriteLine("  suggestion: " & s.ToString())
        call console.WriteLine("        " & s.Detail)
    next

    call console.WriteLine()
    call console.WriteLine("  >> fell back to the CPU reference implementation; only CPU results are shown below.")
else
    using engine
        dim cells As Integer = rows * rows

        call console.WriteLine("  device      : " & engine.Device.Name)
        call console.WriteLine("  kernel image: " & engine.Image.ToString())

        using bufX As New DeviceBuffer(Of Single)(data.Length), _
              bufSum As New DeviceBuffer(Of Single)(rows), _
              bufSumSq As New DeviceBuffer(Of Single)(rows), _
              bufDot As New DeviceBuffer(Of Single)(cells), _
              bufCorr As New DeviceBuffer(Of Single)(cells), _
              bufDist As New DeviceBuffer(Of Single)(cells)

            call bufX.Write(data)

            ' 1) Row statistics: 1D kernel, one thread per row
            call kernels("RowSum").Launch(engine, LaunchPlanner.For1D(rows, 256), bufX, cols, bufSum, rows)
            call kernels("RowSumSq").Launch(engine, LaunchPlanner.For1D(rows, 256), bufX, cols, bufSumSq, rows)

            ' 2) Gram dot products: 2D kernel
            call kernels("GramDot").Launch(engine, LaunchPlanner.For2D(rows, rows, 16, 16), bufX, cols, bufDot, rows, rows)

            ' 3) Reconstruct the correlation and distance matrices from the dot products
            '    and the row statistics
            call kernels("CorrelationCell").Launch(engine, LaunchPlanner.For2D(rows, rows, 16, 16),
                                                   bufDot, bufSum, bufSumSq, rows, cols, bufCorr, rows, rows)
            call kernels("DistanceCell").Launch(engine, LaunchPlanner.For2D(rows, rows, 16, 16),
                                                bufDot, bufSumSq, rows, bufDist, rows, rows)

            call engine.Synchronize()

            corrGPU = bufCorr.Read()
            distGPU = bufDist.Read()
            gpuOk = True

            ' 4) Convention 3 demo: a pure scalar function is auto-wrapped into an
            '    element-wise kernel
            dim hostDot As Single() = bufDot.Read()
            dim hostSum As Single() = bufSum.Read()
            dim hostSumSq As Single() = bufSumSq.Read()
            dim n1 As Single = CSng(cols)
            dim cov(cells - 1) As Single
            dim denom(cells - 1) As Single
            dim clampRef(cells - 1) As Single

            for i As Integer = 0 To rows - 1
                dim meanI As Single = hostSum(i) / n1
                dim varI As Single = hostSumSq(i) - n1 * meanI * meanI
                dim sdI As Single = MathF.Sqrt(MathF.Max(varI, 0.0F))

                for j As Integer = 0 To rows - 1
                    dim meanJ As Single = hostSum(j) / n1
                    dim varJ As Single = hostSumSq(j) - n1 * meanJ * meanJ
                    dim idx As Integer = i * rows + j

                    cov(idx) = hostDot(idx) - n1 * meanI * meanJ
                    denom(idx) = sdI * MathF.Sqrt(MathF.Max(varJ, 0.0F))
                    clampRef(idx) = PearsonMetrics.PearsonClamp(cov(idx), denom(idx))
                next
            next

            using bufCov As New DeviceBuffer(Of Single)(cells), _
                  bufDenom As New DeviceBuffer(Of Single)(cells), _
                  bufClamp As New DeviceBuffer(Of Single)(cells)

                call bufCov.Write(cov)
                call bufDenom.Write(denom)

                call kernels("PearsonClamp").Launch(engine, LaunchPlanner.For1D(cells, 256),
                                                    bufCov, bufDenom, bufClamp, cells)
                call engine.Synchronize()

                dim clampErr As Double = TutorialKit.MaxErrorSingle(bufClamp.Read(), clampRef)
                dim clampPass As Boolean = clampErr <= 1.0E-5

                call console.WriteLine()
                call console.WriteLine("  PearsonClamp (auto-wrapped element-wise) max abs error = " &
                                       clampErr.ToString("E3") & "  " & (if(clampPass, "OK", "FAIL")))

                if Not clampPass Then
                    allOk = False
                end if
            end using
        end using
    end using
end if

' ---------- Step 4: compare against the CPU reference implementation ----------
call TutorialKit.PrintTitle("Step 4: compare with the CPU reference implementation + preview the results")

dim refCorr As Double() = TutorialKit.CpuCorrelation(data, rows, cols)
dim refDist As Double() = TutorialKit.CpuDistance(data, rows, cols)

if gpuOk Then
    dim eCorr As Double = TutorialKit.MaxError(corrGPU, refCorr)
    dim eDist As Double = TutorialKit.MaxError(distGPU, refDist)
    dim corrPass As Boolean = eCorr <= 1.0E-3
    dim distPass As Boolean = eDist <= 1.0E-2

    call console.WriteLine("  Pearson correlation matrix max abs error = " & eCorr.ToString("E3") & "  " & (if(corrPass, "OK", "FAIL")))
    call console.WriteLine("  Euclidean distance matrix  max abs error = " & eDist.ToString("E3") & "  " & (if(distPass, "OK", "FAIL")))

    if (Not corrPass) OrElse (Not distPass) Then
        allOk = False
    end if

    call console.WriteLine()
    call TutorialKit.PrintMatrix("Pearson correlation matrix (GPU, top-left " & preview & " x " & preview & "):", corrGPU, rows, preview)
    call console.WriteLine()
    call TutorialKit.PrintMatrix("Euclidean distance matrix (GPU, top-left " & preview & " x " & preview & "):", distGPU, rows, preview)
else
    call console.WriteLine("  GPU results unavailable; the CPU reference results are shown below.")
    call console.WriteLine()
    call TutorialKit.PrintMatrix("Pearson correlation matrix (CPU, top-left " & preview & " x " & preview & "):", TutorialKit.ToSingle(refCorr), rows, preview)
    call console.WriteLine()
    call TutorialKit.PrintMatrix("Euclidean distance matrix (CPU, top-left " & preview & " x " & preview & "):", TutorialKit.ToSingle(refDist), rows, preview)
end if

call console.WriteLine()

if allOk Then
    call console.WriteLine("All tutorial steps passed.")
else
    call console.WriteLine("Some steps failed, please check the output above.")
end if
