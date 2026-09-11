Imports System
Imports System.Diagnostics
Imports tf = Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports tfMath = Microsoft.VisualBasic.MachineLearning.TensorFlow.Math
Imports tfnn = Microsoft.VisualBasic.MachineLearning.TensorFlow.nn
Imports tfCompute = Microsoft.VisualBasic.MachineLearning.TensorFlow.Compute
Imports gpu = Microsoft.VisualBasic.Computing.ILCuda.GPUTensor
Imports ILCudaRuntime = Microsoft.VisualBasic.Computing.ILCuda.Runtime
Imports std = System.Math

' ---------------------------------------------------------------------------
' ILCudaTensor 演示：同一个负载分别在 SIMD CPU 与 CUDA GPU 后端上执行，
' 对比结果精度与耗时。
'
' 关键点：这段代码完全没有出现任何“后端”相关的调用——
' 它只使用 Tensor / Math / nn 的公开 API，通过 CudaTensor.Register() 即可无感切换。
' ---------------------------------------------------------------------------

Module Program

    ''' <summary>逐元素运算链：exp -> +y -> tanh -> sigmoid -> *2 -> sqrt</summary>
    Private Function EwChain(x As tf.Tensor, y As tf.Tensor) As tf.Tensor
        Return tfMath.sqrt(tfMath.sigmoid(tfMath.tanh(tfMath.exp(x) + y)) * 2.0F)
    End Function

    Private Function MaxDiff(x As Double(), y As Double()) As Double
        Dim m As Double = 0

        For i As Integer = 0 To x.Length - 1
            m = std.Max(m, std.Abs(x(i) - y(i)))
        Next

        Return m
    End Function

    Sub Main(args As String())
        Console.WriteLine("=== ILCudaTensor: Tensor 的 SIMD / CUDA 后端切换演示 ===")
        Console.WriteLine($"默认后端 : {tf.Tensor.computeKernel.Name}")
        Console.WriteLine($"SIMD 能力: {Microsoft.VisualBasic.Math.SIMD.SIMDEnvironment.Description}")
        Console.WriteLine()

        tfCompute.SIMDTensor.Register()

        ' ---------- 输入 ----------
        Const n As Integer = 8192
        Dim x = tf.Tensor.Random({n}, seed:=7)
        Dim y = tf.Tensor.Random({n}, seed:=11)

        Dim mat = tf.Tensor.Random({256, 128}, seed:=3)

        Const dimSize As Integer = 512
        Dim a = tf.Tensor.Random({dimSize, dimSize}, seed:=42)
        Dim b = tf.Tensor.Random({dimSize, dimSize}, seed:=2024)

        Const outer As Integer = 512
        Const classes As Integer = 64
        Dim logits = tf.Tensor.Random({outer, classes}, seed:=99)

        ' ---------- 1) SIMD CPU 参考 ----------
        Console.WriteLine(">> 1) SIMD CPU 后端")

        Dim sw = Stopwatch.StartNew()
        Dim ewCpu = EwChain(x, y)
        sw.Stop()
        Dim cpuEwMs = sw.Elapsed.TotalMilliseconds

        Dim trCpu = mat.Transpose()

        sw.Restart()
        Dim matCpu = tfMath.reduce_sum(a * b)
        sw.Stop()
        Dim cpuMatMs = sw.Elapsed.TotalMilliseconds

        sw.Restart()
        Dim smCpu = tfnn.softmax(logits)
        Dim lsCpu = tfnn.log_softmax(logits)
        Dim rsCpu = tfMath.reduce_sum(logits, axis:=1)
        Dim amCpu = tfMath.argmax(logits, axis:=1)
        sw.Stop()
        Dim cpuSoftmaxMs = sw.Elapsed.TotalMilliseconds

        Console.WriteLine($"    逐元素链 {n} 元素        用时 {cpuEwMs,8:F3} ms")
        Console.WriteLine($"    矩阵乘 + 归约 {dimSize}x{dimSize}  用时 {cpuMatMs,8:F3} ms")
        Console.WriteLine($"    softmax/log_softmax + 轴归约+argmax  用时 {cpuSoftmaxMs,8:F3} ms")
        Console.WriteLine()

        ' ---------- 2) 切换 GPU ----------
        Console.WriteLine(">> 2) 尝试注册 CUDA GPU 后端")

        Dim opts As New ILCudaRuntime.EngineOptions()

        If Not gpu.CudaTensor.Register(opts) Then
            Console.WriteLine($"  注册失败（保持 CPU/SIMD）: {gpu.CudaTensor.LastError}")

            For Each line In opts.Diagnostics
                Console.WriteLine($"    {line}")
            Next

            Return
        End If

        Console.WriteLine($"  注册成功，当前后端 = {tf.Tensor.computeKernel.Name}")

        If gpu.CudaTensor.KernelFailures.Count = 0 Then
            Console.WriteLine("  IL2Cuda 双精度内核：全部翻译并注册成功")
        Else
            For Each f In gpu.CudaTensor.KernelFailures
                Console.WriteLine($"  [内核翻译失败] {f.Key} -> {f.Value}")
            Next
        End If

        Console.WriteLine()

        sw.Restart()
        Dim ewGpu = EwChain(x, y)
        sw.Stop()
        Dim gpuEwMs = sw.Elapsed.TotalMilliseconds

        Dim trGpu = mat.Transpose()

        sw.Restart()
        Dim matGpu = tfMath.reduce_sum(a * b)
        sw.Stop()
        Dim gpuMatMs = sw.Elapsed.TotalMilliseconds

        sw.Restart()
        Dim smGpu = tfnn.softmax(logits)
        Dim lsGpu = tfnn.log_softmax(logits)
        Dim rsGpu = tfMath.reduce_sum(logits, axis:=1)
        Dim amGpu = tfMath.argmax(logits, axis:=1)
        sw.Stop()
        Dim gpuSoftmaxMs = sw.Elapsed.TotalMilliseconds

        ' ---------- 3) 精度对比 ----------
        Console.WriteLine("  精度对比（CPU 双精度为准）：")
        Console.WriteLine($"    P2 逐元素链            : {MaxDiff(ewCpu.Data, ewGpu.Data):E3}   (double 内核)")
        Console.WriteLine($"    P2 转置                : {MaxDiff(trCpu.Data, trGpu.Data):E3}   (double 内核)")
        Console.WriteLine($"    P3 softmax             : {MaxDiff(smCpu.Data, smGpu.Data):E3}   (手写 double 内核)")
        Console.WriteLine($"    P3 log_softmax         : {MaxDiff(lsCpu.Data, lsGpu.Data):E3}   (手写 double 内核)")
        Console.WriteLine($"    P3 末轴求和            : {MaxDiff(rsCpu.Data, rsGpu.Data):E3}   (手写 double 内核)")
        Console.WriteLine($"    P3 末轴 argmax         : {MaxDiff(amCpu.Data, amGpu.Data):E3}   (手写 double 内核，应为 0)")
        Console.WriteLine($"    矩阵乘 + 全局归约      : {MaxDiff(matCpu.Data, matGpu.Data):E3}   (double 内核)")

        ' softmax 行和应为 1
        Dim rowSums = tfMath.reduce_sum(smGpu, axis:=1)
        Dim maxRowErr As Double = 0
        For i As Integer = 0 To rowSums.Length - 1
            maxRowErr = std.Max(maxRowErr, std.Abs(rowSums.Data(i) - 1.0))
        Next
        Console.WriteLine($"    softmax 行和与 1 的最大偏差: {maxRowErr:E3}")

        ' ---------- 4) 性能对比 ----------
        Console.WriteLine()
        Console.WriteLine("  性能对比：")
        Console.WriteLine($"    逐元素链     SIMD {cpuEwMs,8:F3} ms   CUDA {gpuEwMs,8:F3} ms")
        Console.WriteLine($"    矩阵乘归约   SIMD {cpuMatMs,8:F3} ms   CUDA {gpuMatMs,8:F3} ms")
        Console.WriteLine($"    softmax 族   SIMD {cpuSoftmaxMs,8:F3} ms   CUDA {gpuSoftmaxMs,8:F3} ms")

        ' ---------- 5) 切回 CPU ----------
        Console.WriteLine()
        Console.WriteLine(">> 3) 切回 SIMD 后端")
        gpu.CudaTensor.Unregister()
        Console.WriteLine($"  当前后端 = {tf.Tensor.computeKernel.Name}")
    End Sub

End Module
