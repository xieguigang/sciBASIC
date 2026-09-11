Imports System
Imports System.Diagnostics
Imports tf = Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports tfMath = Microsoft.VisualBasic.MachineLearning.TensorFlow.Math
Imports tfnn = Microsoft.VisualBasic.MachineLearning.TensorFlow.nn
Imports tfCompute = Microsoft.VisualBasic.MachineLearning.TensorFlow.Compute
Imports gpu = Microsoft.VisualBasic.Computing.ILCuda.GPUTensor
Imports std = System.Math

' ---------------------------------------------------------------------------
' ILCudaTensor 演示：同一个深度学习负载分别在 SIMD CPU 与 CUDA GPU 后端上执行，
' 对比结果精度与耗时。
'
' 关键点：这段代码完全没有出现任何“后端”相关的调用——
' 它只使用 Tensor / Math / nn 的公开 API，通过 CudaTensor.Register() 即可无感切换。
' ---------------------------------------------------------------------------

Module Program

    ''' <summary>一个典型的小型深度学习负载：逐元素 + 激活 + 矩阵乘 + 归约</summary>
    Private Function Workload(a As tf.Tensor, b As tf.Tensor) As Double()
        Dim c = tfnn.sigmoid(a + b)          ' 逐元素加法 + sigmoid
        Dim d = tfMath.tanh(c * 2.0F)        ' 标量缩放 + tanh
        Dim e = a * b                        ' 二维张量的 * 即矩阵乘
        Dim s = tfMath.reduce_sum(e)         ' 整体归约

        Return {d.TotalSum(), s.Data(0), e(0, 0), e(5, 7)}
    End Function

    Private Function Run(label As String, a As tf.Tensor, b As tf.Tensor, ByRef ms As Double) As Double()
        Dim sw = Stopwatch.StartNew()
        Dim result = Workload(a, b)
        sw.Stop()

        ms = sw.Elapsed.TotalMilliseconds
        Console.WriteLine($"  [{label}] 用时 {ms,9:F2} ms   " &
                          $"sum={result(0):G12}  matsum={result(1):G12}  " &
                          $"e00={result(2):G12}  e57={result(3):G12}")

        Return result
    End Function

    Sub Main(args As String())
        Console.WriteLine("=== TensorCuda: Tensor 计算的 SIMD / CUDA 后端切换演示 ===")
        Console.WriteLine($"默认后端 : {tf.Tensor.computeKernel.Name}")
        Console.WriteLine($"SIMD 能力: {Microsoft.VisualBasic.Math.SIMD.SIMDEnvironment.Description}")
        Console.WriteLine()

        Const n As Integer = 512
        Dim a = tf.Tensor.Random({n, n}, seed:=42)
        Dim b = tf.Tensor.Random({n, n}, seed:=2024)

        Console.WriteLine($"输入: {n} x {n} 随机矩阵（Double）")
        Console.WriteLine()

        ' ---------- 1) 默认 SIMD CPU 后端 ----------
        Console.WriteLine(">> 1) SIMD CPU 后端")
        tfCompute.SIMDTensor.Register()

        Dim cpuMs As Double
        Dim cpu = Run("SIMD", a, b, cpuMs)

        ' ---------- 2) 尝试注册 CUDA GPU 后端 ----------
        Console.WriteLine()
        Console.WriteLine(">> 2) 尝试注册 CUDA GPU 后端")

        If gpu.CudaTensor.Register() Then
            Console.WriteLine($"  注册成功，当前后端 = {tf.Tensor.computeKernel.Name}")

            Dim gpuMs As Double
            Dim gpuResult = Run("CUDA", a, b, gpuMs)

            Dim maxErr As Double = 0
            For i As Integer = 0 To cpu.Length - 1
                maxErr = std.Max(maxErr, std.Abs(cpu(i) - gpuResult(i)))
            Next

            Console.WriteLine()
            Console.WriteLine($"  CPU / GPU 最大绝对误差: {maxErr:E3}")
            If gpuMs > 0 Then
                Console.WriteLine($"  整体加速比            : {cpuMs / gpuMs:F2} x")
            End If

            ' ---------- 3) 切回 CPU ----------
            Console.WriteLine()
            Console.WriteLine(">> 3) 切回 SIMD 后端")
            gpu.CudaTensor.Unregister()
            Console.WriteLine($"  当前后端 = {tf.Tensor.computeKernel.Name}")
        Else
            Console.WriteLine($"  注册失败（保持 CPU/SIMD）: {gpu.CudaTensor.LastError}")
        End If
    End Sub

End Module
