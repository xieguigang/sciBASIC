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
' ILCudaTensor 的 demo 测试
'
' 同一段负载分别在 SIMD CPU 与 CUDA GPU（全 double）后端上执行，
' 逐项断言数值与 CPU 双精度参考一致，并输出性能参考。
'
' 关键点：下面的业务代码里**完全没有出现任何后端相关调用**——
' 只使用 Tensor / Math / nn 的公开 API，通过 CudaTensor.Register() 即可无感切换。
'
' 退出码：0 = 全部通过，1 = 有断言失败，2 = GPU 不可用。
' ---------------------------------------------------------------------------

Module Program

    Private _passed As Integer
    Private _failed As Integer

    Private Sub Check(name As String, ok As Boolean, detail As String)
        If ok Then
            _passed += 1
        Else
            _failed += 1
        End If

        Dim status As String = If(ok, "PASS", "FAIL")
        Console.WriteLine($"    [{status}] {name,-30} {detail}")
    End Sub

    Private Function MaxDiff(x As Double(), y As Double()) As Double
        Dim m As Double = 0

        For i As Integer = 0 To x.Length - 1
            m = std.Max(m, std.Abs(x(i) - y(i)))
        Next

        Return m
    End Function

    Private Function RelErr(got As Double, expected As Double) As Double
        Dim d = std.Abs(got - expected)
        Return If(std.Abs(expected) > 0.0, d / std.Abs(expected), d)
    End Function

    Sub Main(args As String())
        Console.WriteLine("=== ILCudaTensor demo test：SIMD CPU vs CUDA GPU（全 double）===")
        Console.WriteLine($"默认后端 : {tf.Tensor.computeKernel.Name}")
        Console.WriteLine($"SIMD 能力: {Microsoft.VisualBasic.Math.SIMD.SIMDEnvironment.Description}")
        Console.WriteLine()

        tfCompute.SIMDTensor.Register()

        ' ---------------- 输入 ----------------
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

        ' 大张量：用于验证两段式全局归约的并行度与精度
        Const bigN As Integer = 400000000

        Dim big = tf.Tensor.Random({bigN}, seed:=1234)

        ' ---------------- 1) SIMD CPU 参考 ----------------
        Console.WriteLine(">> 1) SIMD CPU 参考（双精度）")

        Dim ewCpu = tfMath.sqrt(tfMath.sigmoid(tfMath.tanh(tfMath.exp(x) + y)) * 2.0F)
        Dim trCpu = mat.Transpose()
        Dim matCpu = tfMath.reduce_sum(a * b)
        Dim smCpu = tfnn.softmax(logits)
        Dim lsCpu = tfnn.log_softmax(logits)
        Dim rsCpu = tfMath.reduce_sum(logits, axis:=1)
        Dim amCpu = tfMath.argmax(logits, axis:=1)

        Dim sw = Stopwatch.StartNew()
        Dim bigSumCpu = tfMath.reduce_sum(big).Data(0)
        Dim bigMaxCpu = tfMath.reduce_max(big).Data(0)
        Dim bigMinCpu = tfMath.reduce_min(big).Data(0)
        sw.Stop()
        Dim cpuBigMs = sw.Elapsed.TotalMilliseconds

        Console.WriteLine($"    大张量全局 sum/max/min {bigN:N0} 元素  用时 {cpuBigMs,8:F3} ms")
        Console.WriteLine()

        ' ---------------- 2) 注册 GPU ----------------
        Console.WriteLine(">> 2) 注册 CUDA GPU 后端")

        Dim opts As New ILCudaRuntime.EngineOptions()

        If Not gpu.CudaTensor.Register(opts) Then
            Console.WriteLine($"    注册失败: {gpu.CudaTensor.LastError}")

            For Each line In opts.Diagnostics
                Console.WriteLine($"      {line}")
            Next

            Environment.ExitCode = 2
            Return
        End If

        Console.WriteLine($"    当前后端 = {tf.Tensor.computeKernel.Name}")

        If gpu.CudaTensor.KernelFailures.Count = 0 Then
            Console.WriteLine("    IL2Cuda 双精度内核：全部翻译并注册成功")
        Else
            For Each f In gpu.CudaTensor.KernelFailures
                Console.WriteLine($"    [内核翻译失败] {f.Key} -> {f.Value}")
            Next
        End If

        Console.WriteLine()

        Dim ewGpu = tfMath.sqrt(tfMath.sigmoid(tfMath.tanh(tfMath.exp(x) + y)) * 2.0F)
        Dim trGpu = mat.Transpose()
        Dim matGpu = tfMath.reduce_sum(a * b)
        Dim smGpu = tfnn.softmax(logits)
        Dim lsGpu = tfnn.log_softmax(logits)
        Dim rsGpu = tfMath.reduce_sum(logits, axis:=1)
        Dim amGpu = tfMath.argmax(logits, axis:=1)

        sw.Restart()
        Dim bigSumGpu = tfMath.reduce_sum(big).Data(0)
        Dim bigMaxGpu = tfMath.reduce_max(big).Data(0)
        Dim bigMinGpu = tfMath.reduce_min(big).Data(0)
        sw.Stop()
        Dim gpuBigMs = sw.Elapsed.TotalMilliseconds

        ' ---------------- 3) 正确性断言 ----------------
        Console.WriteLine(">> 3) 正确性检查（以 CPU 双精度为基准）")

        Dim ewErr = MaxDiff(ewCpu.Data, ewGpu.Data)
        Dim trErr = MaxDiff(trCpu.Data, trGpu.Data)
        Dim smErr = MaxDiff(smCpu.Data, smGpu.Data)
        Dim lsErr = MaxDiff(lsCpu.Data, lsGpu.Data)
        Dim rsErr = MaxDiff(rsCpu.Data, rsGpu.Data)
        Dim amErr = MaxDiff(amCpu.Data, amGpu.Data)
        Dim gemmErr = RelErr(matGpu.Data(0), matCpu.Data(0))
        Dim sumErr = RelErr(bigSumGpu, bigSumCpu)
        Dim maxErr = RelErr(bigMaxGpu, bigMaxCpu)
        Dim minErr = RelErr(bigMinGpu, bigMinCpu)

        Check("P2 逐元素链", ewErr < 1.0E-12, $"maxdiff={ewErr:E3}")
        Check("P2 转置", trErr = 0.0, $"maxdiff={trErr:E3}")
        Check("P3 softmax", smErr < 1.0E-14, $"maxdiff={smErr:E3}")
        Check("P3 log_softmax", lsErr < 1.0E-13, $"maxdiff={lsErr:E3}")
        Check("P3 末轴求和", rsErr < 1.0E-12, $"maxdiff={rsErr:E3}")
        Check("P3 末轴 argmax", amErr = 0.0, $"maxdiff={amErr:E3}")
        Check("double GEMM", gemmErr < 1.0E-13, $"relerr={gemmErr:E3}")
        Check("两段式全局 sum", sumErr < 1.0E-13, $"relerr={sumErr:E3}")
        Check("两段式全局 max", maxErr < 1.0E-15, $"relerr={maxErr:E3}")
        Check("两段式全局 min", minErr < 1.0E-15, $"relerr={minErr:E3}")

        Dim rowSums = tfMath.reduce_sum(smGpu, axis:=1)
        Dim rowErr As Double = 0
        For i As Integer = 0 To rowSums.Length - 1
            rowErr = std.Max(rowErr, std.Abs(rowSums.Data(i) - 1.0))
        Next
        Check("softmax 行和 = 1", rowErr < 1.0E-14, $"maxerr={rowErr:E3}")

        ' ---------------- 4) 性能参考 ----------------
        Console.WriteLine()
        Console.WriteLine(">> 4) 性能参考")
        Console.WriteLine($"    大张量全局归约 {bigN:N0} 元素   SIMD {cpuBigMs,8:F3} ms   CUDA {gpuBigMs,8:F3} ms")

        If gpuBigMs > 0 Then
            Console.WriteLine($"    加速比: {cpuBigMs / gpuBigMs:F2} x")
        End If

        ' ---------------- 收尾 ----------------
        gpu.CudaTensor.Unregister()

        Console.WriteLine()
        Console.WriteLine($"== 测试结果: {_passed} 通过 / {_failed} 失败 ==")

        Environment.ExitCode = If(_failed = 0, 0, 1)
    End Sub

End Module
