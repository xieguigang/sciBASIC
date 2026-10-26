#include "stdlib.dll"
#include "stdio.dll"

imports stdlib
imports stdio

' ============================================================================
'  IL -> CUDA tutorial (standard library edition)
'
'  The target VB.NET functions (row sums, Gram dot products, the Pearson
'  correlation cell and the Euclidean distance cell) now live in the standard
'  library assembly as stdlib.PearsonMetrics, because a script engine can only
'  hand a reflectable Shared method to the decompiler — a JS function is not a
'  CLR method. Everything else is the same pipeline as before:
'
'      VB.NET function -> IL bytecode -> AST -> .cu source
'                 -> NVRTC just-in-time compile -> cuLaunchKernel -> matrix
'
'  Flow:
'      1) decompile the standard library kernels and show the generated source
'      2) self check every kernel (AST interpreter vs a direct CLR call)
'      3) run the correlation and distance matrices on the GPU
'      4) compare them with the double precision CPU reference implementation
'
'  When no CUDA environment is available the script prints the probe report and
'  falls back to the CPU results instead of failing.
' ============================================================================

dim rows = 512
dim cols = 512
dim seed = 42
dim preview = 6

call console.WriteLine("=== 1) decompile the VB.NET functions into an AST and emit .cu source ===")

for each kernel in cuda.kernelNames()
    call console.WriteLine($"  {kernel,-16} index mode = {cuda.kernelIndexMode(kernel)}")
next

call console.WriteLine()
call console.WriteLine("  ---- CorrelationCell pseudo code ----")
call console.WriteLine(cuda.kernelPseudoCode("CorrelationCell"))
call console.WriteLine()
call console.WriteLine("  ---- DistanceCell cuda source ----")
call console.WriteLine(cuda.kernelSource("DistanceCell"))

' ---------------------------------------------------------------------------
' 2) interpreter self check: the same AST is executed on the CPU and compared
'    with a direct invocation of the original method
' ---------------------------------------------------------------------------
call console.WriteLine()
call console.WriteLine("=== 2) interpreter self check ===")

dim probe = cuda.makeMatrix(64, 64, seed)

for each check in cuda.selfCheck(probe, 64, 64)
    call console.WriteLine("  " & check)
next

' ---------------------------------------------------------------------------
' 3) GPU computation and 4) CPU reference comparison
' ---------------------------------------------------------------------------
call console.WriteLine()
call console.WriteLine($"=== 3) GPU computation ({rows} x {cols} input matrix) ===")

dim data = cuda.makeMatrix(rows, cols, seed)
dim cpu_correlation = cuda.pearsonCpu(data, rows, cols)
dim cpu_distance = cuda.distanceCpu(data, rows, cols)

if cuda.available() then
    dim result = cuda.metrics(data, rows, cols)

    call console.WriteLine($"  device           : {result.device}")
    call console.WriteLine($"  elapsed          : {result.elapsedMs} ms")
    call console.WriteLine($"  correlation error: {cuda.maxError(result.correlation, cpu_correlation):F9} (tolerance 1.0E-3)")
    call console.WriteLine($"  distance error   : {cuda.maxError(result.distance, cpu_distance):F9} (tolerance 1.0E-2)")
    call console.WriteLine()
    call console.WriteLine($"  Pearson correlation matrix (GPU, top-left {preview} x {preview}):")
    call console.WriteLine(result.previewCorrelation(preview))
    call console.WriteLine()
    call console.WriteLine($"  Euclidean distance matrix (GPU, top-left {preview} x {preview}):")
    call console.WriteLine(result.previewDistance(preview))
else
    call console.WriteLine("  GPU unavailable, the CPU reference results are shown below.")

    for each diagnostic in cuda.diagnostics()
        call console.WriteLine("  " & diagnostic)
    next

    call console.WriteLine()
    call console.WriteLine($"  Pearson correlation matrix (CPU, top-left {preview} x {preview}):")
    call console.WriteLine(cuda.preview(cpu_correlation, rows, preview))
    call console.WriteLine()
    call console.WriteLine($"  Euclidean distance matrix (CPU, top-left {preview} x {preview}):")
    call console.WriteLine(cuda.preview(cpu_distance, rows, preview))
end if

' ---------------------------------------------------------------------------
' 5) Export both matrices through the standard table writer
' ---------------------------------------------------------------------------
dim correlation_table = table.fromMatrix(cuda.toMatrix(cuda.pearsonCpu(data, 16, cols), 16, 16), table.names("row", 16), table.names("col", 16))

call console.WriteLine()
call console.WriteLine("csv : " & write.csv(correlation_table, here("cuda-correlation-16x16.csv")))
call console.WriteLine("done: cuda")
