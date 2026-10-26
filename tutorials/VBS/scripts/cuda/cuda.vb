#include "stdlib.dll"
#include "stdio.dll"

imports stdlib
imports stdio

' ============================================================================
'  IL -> CUDA tutorial
'
'  The maths lives here, in the script: PearsonMetrics is ordinary VB.NET -
'  row sums, Gram dot products, a correlation cell, a distance cell, a clamp.
'  It contains no CUDA concept at all. The standard library only offers the
'  plumbing (cuda.compile / cuda.launch* / cuda.read), so this tutorial is a
'  plain consumer of a purpose neutral api:
'
'      VB.NET function -> IL bytecode -> AST -> .cu source
'                 -> NVRTC just-in-time compile -> cuLaunchKernel -> matrix
'
'  Flow:
'      1) decompile the script's own kernels and show the generated source
'      2) self check every kernel (AST interpreter vs a direct CLR call)
'      3) run the correlation and distance matrices on the GPU
'      4) compare them with the double precision CPU reference implementation
'
'  When no CUDA environment is available the script prints the probe report and
'  falls back to the CPU results instead of failing.
'
'  NOTE: everything that takes or returns an array lives in a Class below — a
'  top level Function is rewritten into an anonymous function, which cannot have
'  array parameters (see the tutorial readme, the 编写规则与注意事项 section).
' ============================================================================

' ---------------------------------------------------------------------------
' The CUDA kernels: plain VB.NET mathematics.
'
' The engine's convention (stdlib.cuda / ILCuda) is:
'   * parameters named i / j are the thread indices
'   * the other parameters are either buffers (indexed with [..]) or scalars
'   * the return value is written to the output matrix
' ---------------------------------------------------------------------------
public class PearsonMetrics

    ''' <summary>Sum of one row of the input matrix.</summary>
    public shared function RowSum(x as single(), cols as integer, i as integer) as single
        dim s as single = 0

        for k = 0 to cols - 1
            s += x(i * cols + k)
        next

        return s
    end function

    ''' <summary>Sum of squares of one row of the input matrix.</summary>
    public shared function RowSumSq(x as single(), cols as integer, i as integer) as single
        dim s as single = 0

        for k = 0 to cols - 1
            dim v = x(i * cols + k)
            s += v * v
        next

        return s
    end function

    ''' <summary>Gram matrix cell: dot product of the rows i and j.</summary>
    public shared function GramDot(x as single(), cols as integer, i as integer, j as integer) as single
        dim s as single = 0

        for k = 0 to cols - 1
            s += x(i * cols + k) * x(j * cols + k)
        next

        return s
    end function

    ''' <summary>Pearson correlation between the rows i and j, from the precomputed sums.</summary>
    public shared function CorrelationCell(dot as single(), rowSum as single(), rowSumSq as single(),
                                           rows as integer, cols as integer, i as integer, j as integer) as single
        dim n = cols
        dim covariance = dot(i * rows + j) - n * (rowSum(i) / n) * (rowSum(j) / n)
        dim variance_i = rowSumSq(i) - n * (rowSum(i) / n) * (rowSum(i) / n)
        dim variance_j = rowSumSq(j) - n * (rowSum(j) / n) * (rowSum(j) / n)
        dim denominator = System.Math.Sqrt(variance_i) * System.Math.Sqrt(variance_j)

        if denominator <= 0 then
            return 0
        end if

        return covariance / denominator
    end function

    ''' <summary>Euclidean distance between the rows i and j, from the Gram matrix.</summary>
    ''' <remarks>
    ''' Device functions should avoid "if ... then local = value" statements outside a full
    ''' if/else: the IL decompiler rejects that control-flow shape (the phi node of the local
    ''' cannot be placed). Assigning the result variable in both branches keeps it decompilable.
    ''' </remarks>
    public shared function DistanceCell(dot as single(), rowSumSq as single(), rows as integer,
                                        i as integer, j as integer) as single
        dim d = rowSumSq(i) + rowSumSq(j) - 2 * dot(i * rows + j)
        dim r as single = 0

        if i = j then
            r = 0
        else
            if d < 0 then
                r = 0
            else
                r = System.Math.Sqrt(d)
            end if
        end if

        return r
    end function

    ''' <summary>A pure scalar kernel: clamps a value from below (element wise).</summary>
    public shared function PearsonClamp(value as single, minimum as single) as single
        if value < minimum then
            return minimum
        end if

        return value
    end function

end class

' ---------------------------------------------------------------------------
' The double precision CPU reference implementation (used to check the GPU).
' ---------------------------------------------------------------------------
public class CudaReference

    public shared function Pearson(m as single(), rows as integer, cols as integer) as double()
        dim n as double = cols
        dim mean(rows - 1) as double
        dim sd(rows - 1) as double

        for i = 0 to rows - 1
            dim sum as double = 0
            dim sumSq as double = 0

            for k = 0 to cols - 1
                dim v as double = m(i * cols + k)
                sum += v
                sumSq += v * v
            next

            mean(i) = sum / n
            sd(i) = System.Math.Sqrt(System.Math.Max(sumSq - n * mean(i) * mean(i), 0.0))
        next

        dim out(rows * rows - 1) as double

        for i = 0 to rows - 1
            for j = 0 to rows - 1
                dim dot as double = 0

                for k = 0 to cols - 1
                    dot += m(i * cols + k) * m(j * cols + k)
                next

                dim covariance = dot - n * mean(i) * mean(j)
                dim denominator = sd(i) * sd(j)
                dim c = 0.0

                if denominator > 0 then
                    c = covariance / denominator
                end if

                out(i * rows + j) = if(i = j, 1.0, System.Math.Min(1.0, System.Math.Max(-1.0, c)))
            next
        next

        return out
    end function

    public shared function Distance(m as single(), rows as integer, cols as integer) as double()
        dim sumSq(rows - 1) as double

        for i = 0 to rows - 1
            dim sum as double = 0

            for k = 0 to cols - 1
                dim v as double = m(i * cols + k)
                sum += v * v
            next

            sumSq(i) = sum
        next

        dim out(rows * rows - 1) as double

        for i = 0 to rows - 1
            for j = 0 to rows - 1
                dim dot as double = 0

                for k = 0 to cols - 1
                    dot += m(i * cols + k) * m(j * cols + k)
                next

                out(i * rows + j) = if(i = j, 0.0, System.Math.Sqrt(System.Math.Max(sumSq(i) + sumSq(j) - 2.0 * dot, 0.0)))
            next
        next

        return out
    end function

end class

' ===========================================================================
' 0) data + translation
' ===========================================================================
dim rows = 512
dim cols = 512
dim seed = 42
dim previewSize = 6

call console.WriteLine("=== 1) decompile the VB.NET functions of this script into CUDA kernels ===")

dim k_rowSum = cuda.compile(GetType(PearsonMetrics).GetMethod("RowSum"))
dim k_rowSumSq = cuda.compile(GetType(PearsonMetrics).GetMethod("RowSumSq"))
dim k_gramDot = cuda.compile(GetType(PearsonMetrics).GetMethod("GramDot"))
dim k_correlation = cuda.compile(GetType(PearsonMetrics).GetMethod("CorrelationCell"))
dim k_distance = cuda.compile(GetType(PearsonMetrics).GetMethod("DistanceCell"))
dim k_clamp = cuda.compile(GetType(PearsonMetrics).GetMethod("PearsonClamp"))

for each k in New Object() {k_rowSum, k_rowSumSq, k_gramDot, k_correlation, k_distance, k_clamp}
    call console.WriteLine($"  {k.Name,-22} index mode = {k.indexMode}")
next

call console.WriteLine()
call console.WriteLine("  ---- CorrelationCell pseudo code ----")
call console.WriteLine(k_correlation.pseudoCode)
call console.WriteLine()
call console.WriteLine("  ---- DistanceCell cuda source ----")
call console.WriteLine(k_distance.source)

' ===========================================================================
' 2) interpreter self check: the same AST is executed on the CPU and compared
'    with a direct invocation of the script's own method
' ===========================================================================
call console.WriteLine()
call console.WriteLine("=== 2) interpreter self check ===")

dim probe = cuda.makeMatrix(64, 64, seed)
dim probe_sum = PearsonMetrics.RowSum(probe, 64, 0)
dim probe_sumSq = PearsonMetrics.RowSumSq(probe, 64, 0)
dim probe_dot = PearsonMetrics.GramDot(probe, 64, 0, 1)
dim probe_rowSum(63) as single
dim probe_rowSumSq(63) as single

for i = 0 to 63
    probe_rowSum(i) = PearsonMetrics.RowSum(probe, 64, i)
    probe_rowSumSq(i) = PearsonMetrics.RowSumSq(probe, 64, i)
next

dim probe_gram(64 * 64 - 1) as single

for i = 0 to 63
    for j = 0 to 63
        probe_gram(i * 64 + j) = PearsonMetrics.GramDot(probe, 64, i, j)
    next
next

call console.WriteLine("  " & cuda.selfCheck(k_rowSum, New Object() {probe, 64, 0}, probe_sum))
call console.WriteLine("  " & cuda.selfCheck(k_rowSumSq, New Object() {probe, 64, 0}, probe_sumSq))
call console.WriteLine("  " & cuda.selfCheck(k_gramDot, New Object() {probe, 64, 0, 1}, probe_dot))
call console.WriteLine("  " & cuda.selfCheck(k_correlation,
                                             New Object() {probe_gram, probe_rowSum, probe_rowSumSq, 64, 64, 0, 1},
                                             PearsonMetrics.CorrelationCell(probe_gram, probe_rowSum, probe_rowSumSq, 64, 64, 0, 1)))
call console.WriteLine("  " & cuda.selfCheck(k_distance,
                                             New Object() {probe_gram, probe_rowSumSq, 64, 0, 1},
                                             PearsonMetrics.DistanceCell(probe_gram, probe_rowSumSq, 64, 0, 1)))
call console.WriteLine("  " & cuda.selfCheck(k_clamp, New Object() {CSng(0.25), CSng(0.5)}, 0.5))

' ===========================================================================
' 3) GPU computation and 4) CPU reference comparison
' ===========================================================================
call console.WriteLine()
call console.WriteLine($"=== 3) GPU computation ({rows} x {cols} input matrix) ===")

dim data = cuda.makeMatrix(rows, cols, seed)
dim cpu_correlation = CudaReference.Pearson(data, rows, cols)
dim cpu_distance = CudaReference.Distance(data, rows, cols)

if cuda.available() then
    ' kernels must be registered before the engine compiles them
    call k_rowSum.register()
    call k_rowSumSq.register()
    call k_gramDot.register()
    call k_correlation.register()
    call k_distance.register()

    dim session = cuda.createEngine()
    dim cells = rows * rows
    dim buf_x = cuda.buffer(data)
    dim buf_sum = cuda.bufferSize(rows)
    dim buf_sumSq = cuda.bufferSize(rows)
    dim buf_dot = cuda.bufferSize(cells)
    dim buf_corr = cuda.bufferSize(cells)
    dim buf_dist = cuda.bufferSize(cells)
    dim watch = Stopwatch.StartNew()

    call cuda.launch1D(k_rowSum, session, rows, 256, buf_sum, buf_x, cols)
    call cuda.launch1D(k_rowSumSq, session, rows, 256, buf_sumSq, buf_x, cols)
    call cuda.launch2D(k_gramDot, session, rows, rows, 16, buf_dot, buf_x, cols)
    call cuda.launch2D(k_correlation, session, rows, rows, 16, buf_corr, buf_dot, buf_sum, buf_sumSq, rows, cols)
    call cuda.launch2D(k_distance, session, rows, rows, 16, buf_dist, buf_dot, buf_sumSq, rows)
    call cuda.synchronize(session)

    watch.Stop()

    dim gpu_correlation = cuda.read(buf_corr)
    dim gpu_distance = cuda.read(buf_dist)

    call console.WriteLine($"  device           : {cuda.deviceName()}")
    call console.WriteLine($"  elapsed          : {watch.Elapsed.TotalMilliseconds:F4} ms")
    call console.WriteLine($"  correlation error: {cuda.maxError(gpu_correlation, cpu_correlation):F9} (tolerance 1.0E-3)")
    call console.WriteLine($"  distance error   : {cuda.maxError(gpu_distance, cpu_distance):F9} (tolerance 1.0E-2)")
    call console.WriteLine()
    call console.WriteLine($"  Pearson correlation matrix (GPU, top-left {previewSize} x {previewSize}):")
    call console.WriteLine(cuda.preview(gpu_correlation, rows, previewSize))
    call console.WriteLine()
    call console.WriteLine($"  Euclidean distance matrix (GPU, top-left {previewSize} x {previewSize}):")
    call console.WriteLine(cuda.preview(gpu_distance, rows, previewSize))

    ' release the device buffers
    call buf_x.free()
    call buf_sum.free()
    call buf_sumSq.free()
    call buf_dot.free()
    call buf_corr.free()
    call buf_dist.free()
    call session.free()

    ' 16x16 of the GPU result goes to a csv table
    dim corner(16 * 16 - 1) as double

    for i = 0 to 15
        for j = 0 to 15
            corner(i * 16 + j) = gpu_correlation(i * rows + j)
        next
    next

    dim correlation_table = table.fromMatrix(cuda.toMatrix(corner, 16, 16), table.names("row", 16), table.names("col", 16))

    call console.WriteLine()
    call console.WriteLine("csv : " & write.csv(correlation_table, here("cuda-correlation-16x16.csv")))
else
    call console.WriteLine("  GPU unavailable, the CPU reference results are shown below.")

    for each diagnostic in cuda.diagnostics()
        call console.WriteLine("  " & diagnostic)
    next

    call console.WriteLine()
    call console.WriteLine($"  Pearson correlation matrix (CPU, top-left {previewSize} x {previewSize}):")
    call console.WriteLine(cuda.preview(cpu_correlation, rows, previewSize))
    call console.WriteLine()
    call console.WriteLine($"  Euclidean distance matrix (CPU, top-left {previewSize} x {previewSize}):")
    call console.WriteLine(cuda.preview(cpu_distance, rows, previewSize))
end if

call console.WriteLine("done: cuda")
