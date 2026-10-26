#include "stdlib.dll"
#include "stdio.dll"

imports stdlib
imports stdio

' ============================================================================
'  MNIST CNN demo (standard library edition)
'
'  The whole "build network -> train -> evaluate" pipeline now lives in
'  ml.cnnTrainTable, so this script only orchestrates the two passes:
'
'      1) CPU (SIMD) baseline
'      2) register the CUDA tensor backend and run the very same configuration
'      3) print the actual backend of every tensor operator of the network
'      4) compare loss / accuracy / elapsed time between the two backends
'
'  Run:
'      vbs.exe mnist_cnn.vb --mnist-data=<MNIST dir>
'
'  Without a real MNIST folder the demo trains on the deterministic synthetic
'  digit set that ml.syntheticDigits generates, so the tutorial always runs.
' ============================================================================

dim mnist_dir = ?"--mnist-data"
dim images_file = mnist_dir & "\train-images-idx3-ubyte"
dim labels_file = mnist_dir & "\train-labels-idx1-ubyte"
dim passes = 2
dim samples = 120
dim seed = 12345

' ---------------------------------------------------------------------------
' 0) dataset: the real MNIST files when available, the synthetic one otherwise
' ---------------------------------------------------------------------------
dim dataset

if not String.IsNullOrEmpty(mnist_dir) andalso System.IO.File.Exists(images_file) andalso System.IO.File.Exists(labels_file) then
    call console.WriteLine($"[data] mnist: {images_file}")
    dataset = read.mnist(images_file, labels_file, samples)
else
    call console.WriteLine("[data] no mnist dataset given (--mnist-data=<dir>), using the synthetic digit set")
    dataset = ml.syntheticDigits(12, seed)
end if

' ---------------------------------------------------------------------------
' 1) environment probe
' ---------------------------------------------------------------------------
call console.WriteLine()
call console.WriteLine("=== 1) environment probe ===")
call console.WriteLine($"    default backend = {ml.backendName()}")
call console.WriteLine($"    random seed     = {seed} (weight init)")
call console.WriteLine($"    dataset         = {dataset.nsamples} samples x {dataset.nfeatures} pixels")
call console.WriteLine($"    training config = {passes} passes x {samples} images")

' ---------------------------------------------------------------------------
' 2) CPU (SIMD) baseline
' ---------------------------------------------------------------------------
call console.WriteLine()
call console.WriteLine("=== 2) CPU(SIMD) training + evaluation ===")

dim cpu_result = ml.cnnTrainTable(dataset, "class", seed, passes, samples, false)

call console.WriteLine($"    backend={cpu_result.backend}  loss={cpu_result.loss}  correct={cpu_result.correct}/{cpu_result.total} ({cpu_result.accuracy:P2})  elapsed={cpu_result.seconds:F3}s")

' ---------------------------------------------------------------------------
' 3) register the CUDA compute engine
' ---------------------------------------------------------------------------
call console.WriteLine()
call console.WriteLine("=== 3) register the CUDA compute engine ===")

dim gpu_enabled = ml.useCuda()

if gpu_enabled then
    call console.WriteLine($"    [OK] current backend = {ml.backendName()}")
    call console.WriteLine($"    thresholds  : MinGpuElements={ml.cudaMinGpuElements()}, MinGemmElements={ml.cudaMinGemmElements()}")

    dim failures = ml.cudaKernelFailures()

    if failures.length = 0 then
        call console.WriteLine("    IL2Cuda double-precision kernels: all translated and registered successfully")
    else
        for each failure in failures
            call console.WriteLine($"    [kernel translation failed] {failure}")
        next
    end if
else
    call console.WriteLine($"    [fallback] registration failed, using the CPU this run: {ml.cudaLastError()}")
end if

' ---------------------------------------------------------------------------
' 4) CUDA (GPU) pass, operator backend table and CPU vs GPU comparison
' ---------------------------------------------------------------------------
if gpu_enabled then
    call console.WriteLine()
    call console.WriteLine("=== 4) CUDA(GPU) training + evaluation ===")

    dim gpu_result = ml.cnnTrainTable(dataset, "class", seed, passes, samples, false)

    call console.WriteLine($"    backend={gpu_result.backend}  loss={gpu_result.loss}  correct={gpu_result.correct}/{gpu_result.total} ({gpu_result.accuracy:P2})  elapsed={gpu_result.seconds:F3}s")

    call console.WriteLine()
    call console.WriteLine("=== 5) actual execution backend of every tensor operator ===")

    ' note: the loop variable must not be named 'line' — that identifier would collide
    ' with the stdlib plot.line function inside the script preprocessor
    for each op_line in ml.cnnOperatorTable()
        call console.WriteLine(op_line)
    next

    call console.WriteLine()
    call console.WriteLine("=== 6) CPU(SIMD) vs CUDA(GPU) ===")

    dim loss_error = System.Math.Abs(cpu_result.loss - gpu_result.loss)

    call console.WriteLine($"    {"backend",-14}{"loss",-24}{"accuracy",-18}{"elapsed(s)"}")
    call console.WriteLine($"    {"SIMD(CPU)",-14}{cpu_result.loss,-24:R}{cpu_result.accuracy,-18:P2}{cpu_result.seconds:F3}")
    call console.WriteLine($"    {"CUDA(GPU)",-14}{gpu_result.loss,-24:R}{gpu_result.accuracy,-18:P2}{gpu_result.seconds:F3}")
    call console.WriteLine()
    ' float32 kernels vs the SIMD double path: 1e-6 is the rounding level of this comparison
    call console.WriteLine($"    loss max abs error = {loss_error:F9}   {If(loss_error < 1.0E-6, "OK", "FAIL")} (tolerance 1.0E-6)")
    call console.WriteLine($"    correct count      = CPU {cpu_result.correct}/{cpu_result.total}  GPU {gpu_result.correct}/{gpu_result.total}   {If(cpu_result.correct = gpu_result.correct, "OK", "difference see above")}")
    call console.WriteLine($"    elapsed ratio      = CPU {cpu_result.seconds:F3}s  GPU {gpu_result.seconds:F3}s  ratio={cpu_result.seconds / gpu_result.seconds:F2}x")
else
    call console.WriteLine()
    call console.WriteLine("=== 4-6) skipped ===")
    call console.WriteLine($"    GPU not enabled, CPU result = loss {cpu_result.loss}, {cpu_result.correct}/{cpu_result.total} ({cpu_result.accuracy:P2})")
end if

' ---------------------------------------------------------------------------
' 7) a small markdown report about the run
' ---------------------------------------------------------------------------
dim doc = report.document("MNIST CNN backend comparison", "sciBASIC standard library")

call doc.add(report.heading("1. Configuration"))
call doc.add(report.list(New String() {
    $"dataset: {dataset.nsamples} samples x {dataset.nfeatures} pixels",
    $"{passes} training passes x {samples} images, weight seed {seed}",
    $"CUDA backend: {If(gpu_enabled, "enabled", "not available")}"
}))
call doc.add(report.heading("2. CPU baseline"))
call doc.add(report.paragraph($"loss = {cpu_result.loss}, accuracy = {cpu_result.accuracy:P2}, elapsed = {cpu_result.seconds:F3}s"))

call console.WriteLine()
call console.WriteLine("md  : " & doc.save(here("mnist-cnn.md")))
call console.WriteLine("done: mnist-cnn")
