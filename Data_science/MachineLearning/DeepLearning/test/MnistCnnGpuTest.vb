#Region "Microsoft.VisualBasic::c43ce4f40fbd848fefb98b951f44e90f, Data_science\MachineLearning\DeepLearning\test\MnistCnnGpuTest.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 358
    '    Code Lines: 221 (61.73%)
    ' Comment Lines: 76 (21.23%)
    '    - Xml Docs: 59.21%
    ' 
    '   Blank Lines: 61 (17.04%)
    '     File Size: 17.84 KB


    ' Class MnistCnnGpuTest
    ' 
    '     Function: ConvOutSize, TrainAndEvaluate, Ulp
    ' 
    '     Sub: PrintDispatchTable, PrintResult, Row, Run, Section
    '     Class RunResult
    ' 
    '         Properties: Accuracy, Backend, Correct, Loss, Seconds
    '                     Total
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Diagnostics
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MachineLearning.CNN
Imports Microsoft.VisualBasic.MachineLearning.CNN.data
Imports Microsoft.VisualBasic.MachineLearning.CNN.trainers
Imports Microsoft.VisualBasic.MachineLearning.DataStorage
Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports randf2 = Microsoft.VisualBasic.Math.RandomExtensions
Imports gpu = Microsoft.VisualBasic.Computing.ILCuda.GPUTensor
Imports ILCudaRuntime = Microsoft.VisualBasic.Computing.ILCuda.Runtime

''' <summary>
''' MNIST CNN 的 GPU 加速 demo。
''' </summary>
''' <remarks>
''' <para>
''' 在**完全相同的确定性配置**下（同一个随机种子、同一批样本、同一个网络结构）分别用
''' CPU(SIMD) 与 CUDA 两个后端各跑一遍训练 + 评估，然后对比：
''' </para>
''' <list type="number">
''' <item>数值一致性：两次运行的 loss 与分类正确数应当只差浮点舍入量级</item>
''' <item>真实加速比：给出两边的墙钟耗时</item>
''' <item>后端是否真的生效：打印 GPU 阈值以及各算子的实际规模，明确指出哪些算子
''' 因为规模低于阈值而回退到 CPU —— 避免"假通过"</item>
''' </list>
''' <para>
''' 之所以要固定随机种子：网络权值的初始化走 <c>Vector.rand</c>，底层是未播种的共享
''' 随机数发生器，不固定的话两次运行的初始权值不同，数值对比就没有意义了。
''' </para>
''' </remarks>
Public Class MnistCnnGpuTest

    Private Class RunResult

        Public Property Backend As String
        Public Property Loss As Double
        Public Property Correct As Integer
        Public Property Total As Integer
        Public Property Seconds As Double

        Public ReadOnly Property Accuracy As Double
            Get
                Return If(Total = 0, 0.0, CDbl(Correct) / Total)
            End Get
        End Property

    End Class

    Public Shared Sub Run(seed As Integer, passes As Integer, samples As Integer)
        Call Section("1) 环境探测")
        Call Console.WriteLine($"    默认后端  = {Tensor.computeKernel.Name}")
        Call Console.WriteLine($"    SIMD 能力 = {Microsoft.VisualBasic.Math.SIMD.SIMDEnvironment.Description}")

        ' ---------------------------------------------------------------
        ' 2) 先用 CPU(SIMD) 跑一遍作为基准
        ' ---------------------------------------------------------------
        Call Console.WriteLine()
        Call Console.WriteLine(">> 2) CPU(SIMD) 训练 + 评估")
        Dim cpu = TrainAndEvaluate(seed, passes, samples)
        Call PrintResult(cpu)

        ' 敏感性对照：后端之间的差异本身是 ULP 级的，而训练过程会把它放大。
        ' 在同一后端上只改变初始权值的 1 个 ULP，就能量出这个放大倍数，
        ' 从而判定 CPU/GPU 的差异究竟来自算子错误还是训练过程自身的敏感性。
        Call Console.WriteLine()
        Call Console.WriteLine(">> 2b) 敏感性对照: 初始权值仅扰动 1 个 ULP (同样在 SIMD 后端)")
        Dim perturbed = TrainAndEvaluate(seed, passes, samples, perturbUlp:=True)
        Call PrintResult(perturbed)

        ' ---------------------------------------------------------------
        ' 3) 注册 CUDA 后端
        ' ---------------------------------------------------------------
        Call Console.WriteLine()
        Call Console.WriteLine(">> 3) 注册 CUDA GPU 后端")

        Dim options As New ILCudaRuntime.EngineOptions()
        Dim gpuRun As RunResult = Nothing
        Dim fallbackRun As RunResult = Nothing

        If gpu.CudaTensor.Register(options) Then
            Call Console.WriteLine($"    [OK] 当前后端 = {Tensor.computeKernel.Name}")
            Call Console.WriteLine($"    阈值      : MinGpuElements={gpu.CudaTensor.MinGpuElements}, " &
                                   $"MinGemmElements={gpu.CudaTensor.MinGemmElements}")

            If gpu.CudaTensor.KernelFailures.Count = 0 Then
                Call Console.WriteLine("    IL2Cuda 双精度内核: 全部翻译并注册成功")
            Else
                For Each f In gpu.CudaTensor.KernelFailures
                    Call Console.WriteLine($"    [内核翻译失败] {f.Key} -> {f.Value}")
                Next
            End If

            Call Console.WriteLine()
            Call Console.WriteLine(">> 4) CUDA(GPU) 训练 + 评估")
            gpuRun = TrainAndEvaluate(seed, passes, samples)
            Call PrintResult(gpuRun)

            ' 4b) 决定性对照：后端仍然是 CUDA，但把阈值抬到使**所有**算子都回退 MyBase
            '     （与 SIMD 走的是同一份标量源码）。用它可以区分：
            '       结果与 SIMD 完全一致  => 内核数值实现有偏差
            '       结果与 SIMD 仍不一致  => 缓存/调度层（例如显存缓存未失效）有问题
            Call Console.WriteLine()
            Call Console.WriteLine(">> 4b) CUDA 后端但全部算子回退 MyBase（隔离缓存/调度层）")

            Dim savedMinGpu = gpu.CudaTensor.MinGpuElements
            Dim savedMinGemm = gpu.CudaTensor.MinGemmElements

            gpu.CudaTensor.MinGpuElements = Integer.MaxValue
            gpu.CudaTensor.MinGemmElements = Integer.MaxValue

            fallbackRun = TrainAndEvaluate(seed, passes, samples)

            gpu.CudaTensor.MinGpuElements = savedMinGpu
            gpu.CudaTensor.MinGemmElements = savedMinGemm

            Call PrintResult(fallbackRun)
        Else
            Call Console.WriteLine($"    [回退] 注册失败, 本次仍使用 CPU: {gpu.CudaTensor.LastError}")

            For Each line In options.Diagnostics
                Call Console.WriteLine($"      {line}")
            Next
        End If

        ' ---------------------------------------------------------------
        ' 5) 算子的实际执行后端（按规模与阈值推断，避免"假通过"）
        ' ---------------------------------------------------------------
        Call Console.WriteLine()
        Call Console.WriteLine(">> 5) 本网络各张量算子的实际执行后端")
        Call PrintDispatchTable(Tensor.computeKernel.Name, passes)

        ' ---------------------------------------------------------------
        ' 6) CPU vs GPU 对比
        ' ---------------------------------------------------------------
        Call Console.WriteLine()
        Call Console.WriteLine(">> 6) CPU(SIMD) vs CUDA(GPU)")

        If gpuRun Is Nothing Then
            Call Console.WriteLine("    GPU 未启用，跳过对比。")
            Return
        End If

        ' 注意: TensorFlow 命名空间里也有一个名为 Math 的模块, 所以这里必须显式限定
        Dim lossErr = System.Math.Abs(cpu.Loss - gpuRun.Loss)
        Dim ulpDrift = System.Math.Abs(cpu.Loss - perturbed.Loss)

        Call Console.WriteLine($"    {"后端",-16}{"loss",-24}{"accuracy",-16}{"耗时(s)",-10}")
        Call Console.WriteLine($"    {"SIMD(CPU)",-16}{cpu.Loss,-24:R}{cpu.Accuracy,-16:P2}{cpu.Seconds,-10:F3}")
        Call Console.WriteLine($"    {"SIMD+1ULP",-16}{perturbed.Loss,-24:R}{perturbed.Accuracy,-16:P2}{perturbed.Seconds,-10:F3}")
        Call Console.WriteLine($"    {"CUDA(GPU)",-16}{gpuRun.Loss,-24:R}{gpuRun.Accuracy,-16:P2}{gpuRun.Seconds,-10:F3}")
        Call Console.WriteLine()
        Call Console.WriteLine($"    CPU 与 GPU 的 loss 之差    = {lossErr:E3}")
        Call Console.WriteLine($"    1 ULP 扰动造成的 loss 漂移  = {ulpDrift:E3}")

        If fallbackRun IsNot Nothing Then
            Dim fallbackErr = System.Math.Abs(cpu.Loss - fallbackRun.Loss)

            ' 后端仍是 CUDA 但全部算子回退 MyBase 时，若结果与 SIMD 只差舍入量级，
            ' 说明后端切换与阈值调度这一层是正常的（此时不会用到显存缓存），
            ' 因而差异只可能来自"走 GPU 分支"的那部分代码。
            ' 注意：SIMDTensor 自己覆盖了 MatMul 等算子，与 TensorComputeBase 的标量实现
            '       求和顺序不同，所以这里容许 ULP 级差异，而不是要求逐位相等。
            Call Console.WriteLine($"    CUDA 后端+全回退 与 SIMD 之差 = {fallbackErr:E3}   " &
                                   If(fallbackErr < 1.0E-12,
                                      "舍入级 => 回退路径正常, 差异来自 GPU 分支",
                                      "超出舍入量级 => 回退路径本身有问题"))
        End If

        ' 判据（按优先级）：
        '   1) 差异已在舍入量级          => 后端数值实现一致
        '   2) 与"仅扰动 1 个 ULP"的漂移同量级 => 差异来自训练过程的敏感性放大，而非算子错误
        '   3) 其余                       => 需要进一步排查
        Dim verdict As String

        If lossErr < 1.0E-9 Then
            verdict = "舍入量级 => GPU 与 CPU 数值一致"
        ElseIf ulpDrift >= lossErr * 0.1 Then
            verdict = "同量级 => 差异来自训练过程的敏感性放大, 不是算子错误"
        Else
            verdict = "量级不符 => 需要进一步排查 GPU 算子"
        End If

        Call Console.WriteLine($"    => {verdict}")
        Call Console.WriteLine($"    分类正确数 = CPU {cpu.Correct}/{cpu.Total}   " &
                               $"SIMD+1ULP {perturbed.Correct}/{perturbed.Total}   " &
                               $"GPU {gpuRun.Correct}/{gpuRun.Total}")

        If gpuRun.Seconds > 0.0 Then
            Call Console.WriteLine()
            Call Console.WriteLine($"    耗时: CPU {cpu.Seconds:F3}s  GPU {gpuRun.Seconds:F3}s  " &
                                   $"比 = {cpu.Seconds / gpuRun.Seconds:F2} x")

            If gpuRun.Seconds >= cpu.Seconds Then
                Call Console.WriteLine("    说明: 单样本(N=1)下 CUDA 后端并未加速, 原因是 ILCudaTensor 采用" &
                                       "『每次算子调用都 H2D 上传 + 内核 + D2H 读回』的拷贝式执行模型,")
                Call Console.WriteLine("          而本网络池化/全连接/softmax 的规模低于 GPU 阈值、本就回退 CPU。" &
                                       "要真正体现 GPU 吞吐需要 batch 化的大张量。")
            End If
        End If
    End Sub

    ''' <summary>
    ''' 用当前后端跑一次确定性的训练 + 评估。
    ''' </summary>
    ''' <remarks>
    ''' 每次调用都重新播种：这样 CPU 与 GPU 两次运行看到的初始权值与样本顺序完全相同，
    ''' 于是两者之间的差异只可能来自后端的数值实现。
    ''' </remarks>
    Private Shared Function TrainAndEvaluate(seed As Integer, passes As Integer, samples As Integer,
                                             Optional perturbUlp As Boolean = False) As RunResult
        Call randf2.SetSeed(seed)

        Dim mr As New MNIST(MnistBaseline.MnistImages, MnistBaseline.MnistLabels)
        Dim dataset = mr.ExtractVectors.Take(samples).ToArray
        Dim net As ConvolutionalNN = MnistBaseline.BuildNetwork(mr)

        If perturbUlp Then
            ' 敏感性对照：只把第一个权值改变 1 个 ULP。若最终的 loss 漂移与 CPU/GPU 之差
            ' 处于同一量级，就说明后者是训练过程对 ULP 级差异的放大，而不是算子错误。
            Dim firstWeight = net.BackPropagationResult.First.Weights
            firstWeight(0) += Ulp(firstWeight(0))
        End If
        Dim trainer As TrainerAlgorithm = New AdaGradTrainer(20, 0.001F).SetKernel(net)
        Dim db As New DataBlock(mr.ImageSize.Width, mr.ImageSize.Height, 1, 0)

        Dim watch As Stopwatch = Stopwatch.StartNew()
        Dim lastLoss As Double = 0

        For p As Integer = 1 To passes
            Dim loss As Double = 0
            Dim check As New PerformanceCounter

            For Each img In dataset
                Call db.addImageData(img.value, img.value.Max)

                Dim tr As TrainResult = trainer.train(db, {Val(img.description)}, check.Set)
                loss += tr.Loss
            Next

            lastLoss = loss / dataset.Length
            Call Console.WriteLine($"      pass {p}/{passes}  loss={lastLoss:R}  " &
                                   $"耗时={watch.Elapsed.TotalSeconds:F2}s")
        Next

        Dim correct As Integer = 0

        For Each img In dataset
            Call db.addImageData(img.value, img.value.Max)

            If img.description = Which.Max(net.predict(db)) Then
                correct += 1
            End If
        Next

        watch.Stop()

        Return New RunResult With {
            .Backend = Tensor.computeKernel.Name,
            .Loss = lastLoss,
            .Correct = correct,
            .Total = dataset.Length,
            .Seconds = watch.Elapsed.TotalSeconds
        }
    End Function

    ''' <summary>
    ''' 打印本网络各张量算子的规模，并按后端的阈值判断它实际会走 GPU 还是回退 CPU。
    ''' </summary>
    ''' <remarks>
    ''' 这一节是刻意保留的：拷贝式执行模型下，规模低于阈值的算子会静默回退 CPU，
    ''' 如果不把规模与阈值摆出来，很容易把"部分算子走 GPU"误读成"端到端都在 GPU 上"。
    ''' </remarks>
    ''' <summary>
    ''' 打印本网络各算子被门控的那一个张量的规模，并按后端阈值判断它实际会走 GPU 还是回退 CPU。
    ''' </summary>
    ''' <remarks>
    ''' 关键是"门控对象"各不相同（均已对照 CudaTensor.vb 的实现确认）：
    '''   * 逐元素算子与 MaxPool2D —— 门控输入张量自身的元素数
    '''   * Conv2D 前向 —— 门控**输入** x 的元素数（不是输出！）
    '''   * Conv2DBackward* —— 门控 gradOutput（即本层输出）的元素数
    '''   * MatMul —— 门控 m*k*n
    ''' 若不把门控对象摆出来，很容易把"部分算子走 GPU"误读成"端到端都在 GPU 上"。
    ''' </remarks>
    Private Shared Sub PrintDispatchTable(backend As String, passes As Integer)
        Const filterSize As Integer = 5
        Const conv1Depth As Integer = 32
        Const conv2Depth As Integer = 64
        Const poolSize As Integer = 2

        Dim inSize As Integer = 28 ' MNIST 图像边长，与 MnistBaseline 的网络结构一致

        Dim c1 = ConvOutSize(inSize, filterSize, 1, 2)
        Dim p1 = ConvOutSize(c1, poolSize, poolSize, 0)
        Dim c2 = ConvOutSize(p1, filterSize, 1, 2)
        Dim p2 = ConvOutSize(c2, poolSize, poolSize, 0)

        Dim minGpu = gpu.CudaTensor.MinGpuElements
        Dim minGemm = gpu.CudaTensor.MinGemmElements

        Call Console.WriteLine($"    后端 = {backend}；单样本(N=1)；" &
                               $"阈值 MinGpuElements={minGpu}, MinGemmElements={minGemm}")

        Dim image = inSize * inSize * 1
        Dim relu1 = c1 * c1 * conv1Depth
        Dim pool1 = p1 * p1 * conv1Depth
        Dim conv2 = c2 * c2 * conv2Depth
        Dim pool2 = p2 * p2 * conv2Depth

        Call Row("conv1 前向", image, "输入 x", minGpu)
        Call Row("conv1 反向", relu1, "gradOutput", minGpu)
        Call Row("pool1 前向", relu1, "输入 x", minGpu)
        Call Row("pool1 反向", pool1, "gradOutput", minGpu)
        Call Row("relu1 前反向", relu1, "输入 x", minGpu)
        Call Row("conv2 前向", pool1, "输入 x", minGpu)
        Call Row("conv2 反向", conv2, "gradOutput", minGpu)
        Call Row("pool2 前向", conv2, "输入 x", minGpu)
        Call Row("pool2 反向", pool2, "gradOutput", minGpu)
        Call Row("relu2 前反向", conv2, "输入 x", minGpu)
        Call Row("fc  MatMul", 10 * pool2 * 1, "m*k*n", minGemm)
        Call Row("softmax", 10, "输入 x", minGpu)
    End Sub

    Private Shared Sub Row(name As String, size As Long, gate As String, threshold As Integer)
        Dim onGpu = size >= threshold

        Call Console.WriteLine($"      {name,-16}{size,12:N0}  {gate,-12}" &
                               $"阈值 {threshold,-8:N0}" &
                               If(onGpu, "GPU", "CPU 回退"))
    End Sub

    Private Shared Function ConvOutSize(inputSize As Integer, kernelSize As Integer,
                                        stride As Integer, padding As Integer) As Integer
        Return (inputSize + 2 * padding - kernelSize) \ stride + 1
    End Function

    ''' <summary>返回 <paramref name="x"/> 的一个 ULP（最小可分辨步长）</summary>
    Private Shared Function Ulp(x As Double) As Double
        If x = 0.0 Then Return Double.Epsilon

        Dim bits = BitConverter.DoubleToInt64Bits(x)
        Dim nextBits = If(x > 0.0, bits + 1L, bits - 1L)

        Return System.Math.Abs(BitConverter.Int64BitsToDouble(nextBits) - x)
    End Function

    Private Shared Sub PrintResult(result As RunResult)
        Call Console.WriteLine($"    后端={result.Backend}  loss={result.Loss:R}  " &
                               $"accuracy={result.Correct}/{result.Total} ({result.Accuracy:P2})  " &
                               $"耗时={result.Seconds:F3}s")
    End Sub

    Private Shared Sub Section(title As String)
        Call Console.WriteLine($"=== {title} ===")
    End Sub

End Class

