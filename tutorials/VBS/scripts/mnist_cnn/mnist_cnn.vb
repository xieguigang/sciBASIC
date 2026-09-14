#include "Microsoft.VisualBasic.DeepLearning.dll"
#include "Microsoft.VisualBasic.MachineLearning.TensorFlow.dll"
#include "Microsoft.VisualBasic.MachineLearning.DataStorage.dll"
#include "Microsoft.VisualBasic.Computing.ILCuda.dll"
#include "Microsoft.VisualBasic.Computing.ILCuda.GPUTensor.dll"

imports Microsoft.VisualBasic.ApplicationServices
imports Microsoft.VisualBasic.Language
imports Microsoft.VisualBasic.Linq
imports Microsoft.VisualBasic.MachineLearning.CNN
imports Microsoft.VisualBasic.MachineLearning.CNN.data
imports Microsoft.VisualBasic.MachineLearning.CNN.trainers
imports Microsoft.VisualBasic.MachineLearning.DataStorage
imports Microsoft.VisualBasic.MachineLearning.TensorFlow
imports Microsoft.VisualBasic.Computing.ILCuda.Runtime
imports Microsoft.VisualBasic.Computing.ILCuda.GPUTensor

' ============================================================================
'  MNIST CNN CUDA acceleration demo (script version)
'
'  The same logic as the MnistCnnGpuTest project under DeepLearning\test:
'      1) environment probe (default backend / random seed / training config)
'      2) run one pass with the CPU (SIMD) backend as the baseline
'      3) register the CUDA compute engine; on failure print diagnostics and
'         fall back to CPU (so the script always produces a result)
'      4) run the very same configuration again with the CUDA backend
'      5) print, for every tensor operator of this network, whether it is gated
'         onto the GPU or falls back to the CPU
'      6) CPU vs GPU numeric consistency, wall-clock time and speedup
'
'  Run:
'      vbs.exe tutorials\VBS\scripts\mnist_cnn\mnist_cnn.vb --mnist-data <MNIST dir>
'
'  Notes:
'    * The network weights are initialized through a shared "unseeded" random
'      generator; without fixing the seed every run starts from different
'      weights and the results cannot be compared. The seed is therefore reset
'      before each pass, so the CPU and GPU paths see exactly the same initial
'      weights and sample order -- any difference can only come from the numeric
'      implementation of the backend.
'    * The training flow lives in the Public Class below (a top-level Function
'      in a script is rewritten by the engine into an anonymous function, which
'      can neither capture outer variables nor be reflected; type blocks are
'      kept verbatim).
' ============================================================================

''' <summary>
''' One complete "build network -> train -> evaluate" pass.
''' </summary>
''' <remarks>
''' Executes on whichever backend <c>Tensor.computeKernel</c> currently points
''' to, so the same code can run on CPU (SIMD) or CUDA and the two can be
''' compared directly.
''' </remarks>
Public Class CnnRunner

    ''' <summary>
    ''' Run one deterministic training + evaluation pass.
    ''' </summary>
    ''' <returns>``{last pass loss, number of correct classifications, total samples, elapsed seconds}``</returns>
    Public Shared Function Execute(imagesPath As String, labelsPath As String,
                                  randomSeed As Integer, passes As Integer, samples As Integer) As (lastLoss as double, correct as double, size as double, cost_ms as double)
        ' The seed must be set before the network is built (i.e. before the
        ' first call to Vector.rand)
        Call Microsoft.VisualBasic.Math.RandomExtensions.SetSeed(randomSeed)

        Dim reader As New MNIST(imagesPath, labelsPath)

        ' Materialize the samples first, guaranteeing that every pass and every
        ' run sees the same batch of data in the same order
        Dim dataset = reader.ExtractVectors.Take(samples).ToArray

        ' The network structure is identical to MnistTest in the repository:
        '   input 28x28x1 -> conv5x32 -> relu -> pool2 -> conv5x64 -> relu -> pool2 -> fc10 -> softmax
        '
        ' Streamed with the LayerBuilder + operator, one-to-one with R# auto_encoder.R:
        '   let cnn = cnn() + input_layer([28,28],1) + conv_layer(5,32,1,2) + pool_layer(2,2,0) + ...
        ' The only difference is that VB implicit line continuation requires the
        ' binary operator to stay at the end of the previous line.
        Dim cnn As LayerBuilder = New LayerBuilder() +
            input_layer({reader.ImageSize.Width, reader.ImageSize.Height}, 1) +
            conv_layer(5, 32, 1, 2) +
            relu_layer() +
            pool_layer(2, 2, 0) +
            conv_layer(5, 64, 1, 2) +
            relu_layer() +
            pool_layer(2, 2, 0) +
            full_connected_layer(10) +
            softmax_layer()

        Dim net As ConvolutionalNN = New ConvolutionalNN(cnn)
        Dim trainer As TrainerAlgorithm = New AdaGradTrainer(20, 0.001F).SetKernel(net)
        Dim db As New DataBlock(reader.ImageSize.Width, reader.ImageSize.Height, 1, 0)

        Dim watch As Stopwatch = Stopwatch.StartNew()
        Dim lastLoss As Double = 0

        For p As Integer = 1 To passes
            Dim passLoss As Double = 0
            Dim check As New PerformanceCounter()

            For Each digit In dataset
                Call db.addImageData(digit.value, digit.value.Max)

                Dim result As TrainResult = trainer.train(db, {Val(digit.description)}, check.Set)

                passLoss += result.Loss
            Next

            lastLoss = passLoss / dataset.Length

            Call Console.WriteLine($"    pass {p}/{passes}  loss={lastLoss:R}")
        Next

        Dim correct As double = 0

        For Each digit In dataset
            Call db.addImageData(digit.value, digit.value.Max)

            If CInt(Val(digit.description)) = CInt(Which.Max(net.predict(db))) Then
                correct += 1
            End If
        Next

        watch.Stop()

        Return (lastLoss, correct, cdbl( dataset.Length), cdbl( watch.Elapsed.TotalSeconds))
    End Function

End Class

' ---------------------------------------------------------------------------
' 0) Command line arguments
' ---------------------------------------------------------------------------
dim mnist_repo = ?"--mnist-data"
dim images_file = $"{mnist_repo}\train-images-idx3-ubyte"
dim labels_file = $"{mnist_repo}\train-labels-idx1-ubyte"

dim random_seed = 12345
dim train_passes = 5
dim train_samples = 1000

' ---------------------------------------------------------------------------
' 1) Environment probe
' ---------------------------------------------------------------------------
call console.WriteLine("=== 1) 环境探测 ===")
call console.WriteLine($"    默认后端  = {Tensor.computeKernel.Name}")
call console.WriteLine($"    随机种子  = {random_seed}（权重初始化）")
call console.WriteLine($"    训练配置  = {train_passes} 轮 x {train_samples} 张")

' ---------------------------------------------------------------------------
' 2) CPU (SIMD) baseline
' ---------------------------------------------------------------------------
call console.WriteLine()
call console.WriteLine("=== 2) CPU(SIMD) 训练 + 评估 ===")

dim (cpu_loss, cpu_correct,cpu_total,cpu_seconds) = CnnRunner.Execute(images_file, labels_file, random_seed, train_passes, train_samples)

call console.WriteLine($"    后端={Tensor.computeKernel.Name}  loss={cpu_loss:R}  " &
                      $"正确={cpu_correct}/{cpu_total} ({cpu_correct / cpu_total:P2})  耗时={cpu_seconds:F3}s")

' ---------------------------------------------------------------------------
' 3) Register the CUDA compute engine
' ---------------------------------------------------------------------------
call console.WriteLine()
call console.WriteLine("=== 3) 注册 CUDA 计算引擎 ===")

dim gpu_enabled = false
dim gpu_options as new EngineOptions()

if CudaTensor.Register(gpu_options) then
    gpu_enabled = true

    call console.WriteLine($"    [OK] 当前后端 = {Tensor.computeKernel.Name}")
    call console.WriteLine($"    阈值      : MinGpuElements={CudaTensor.MinGpuElements}, MinGemmElements={CudaTensor.MinGemmElements}")

    if CudaTensor.KernelFailures.Count = 0 then
        call console.WriteLine("    IL2Cuda 双精度内核: 全部翻译并注册成功")
    else
        for each failure in CudaTensor.KernelFailures
            call console.WriteLine($"    [内核翻译失败] {failure.Key} -> {failure.Value}")
        next
    end if
else
    call console.WriteLine($"    [回退] 注册失败，本次仍使用 CPU: {CudaTensor.LastError}")

    for each line in gpu_options.Diagnostics
        call console.WriteLine($"      {line}")
    next
end if

' ---------------------------------------------------------------------------
' 4) CUDA (GPU) training + evaluation
' ---------------------------------------------------------------------------
if gpu_enabled then
    call console.WriteLine()
    call console.WriteLine("=== 4) CUDA(GPU) 训练 + 评估 ===")

    dim (gpu_loss ,gpu_correct,gpu_total,gpu_seconds) = CnnRunner.Execute(images_file, labels_file, random_seed, train_passes, train_samples)

    call console.WriteLine($"    后端={Tensor.computeKernel.Name}  loss={gpu_loss:R}  " &
                          $"正确={gpu_correct}/{gpu_total} ({gpu_correct / gpu_total:P2})  耗时={gpu_seconds:F3}s")

    ' -----------------------------------------------------------------------
    ' 5) The actual execution backend of every tensor operator of this network
    '
    '    Key point: the gating operand is not the same for every operator
    '    (all verified against the CudaTensor implementation):
    '      * element-wise operators / MaxPool2D -- gated on the element count of
    '        the input tensor itself
    '      * Conv2D forward                    -- gated on the element count of
    '        the **input** x (NOT the output!)
    '      * Conv2DBackward*                   -- gated on gradOutput (i.e. the
    '        output of this layer)
    '      * MatMul                            -- gated on m*k*n
    '    Without making the gating operand explicit it is very easy to misread
    '    "some operators run on the GPU" as "everything runs on the GPU".
    ' -----------------------------------------------------------------------
    call console.WriteLine()
    call console.WriteLine("=== 5) 各张量算子的实际执行后端 ===")

    dim min_gpu = CudaTensor.MinGpuElements
    dim min_gemm = CudaTensor.MinGemmElements
    dim image_size = 28
    dim relu1_size = 28 * 28 * 32
    dim pool1_size = 14 * 14 * 32
    dim conv2_size = 14 * 14 * 64
    dim pool2_size = 7 * 7 * 64
    dim fc_gemm = 10 * pool2_size * 1

    call console.WriteLine($"    后端 = {Tensor.computeKernel.Name}；单样本(N=1)；阈值 MinGpuElements={min_gpu}, MinGemmElements={min_gemm}")

    call console.WriteLine($"    {"算子",-16}{"规模",14}{"  门控对象",-14}{"结论"}")
    call console.WriteLine($"    {"conv1 前向",-16}{image_size * image_size,14}{"  输入 x",-14}{If(image_size * image_size >= min_gpu, "GPU", "CPU 回退")}")
    call console.WriteLine($"    {"conv1 反向",-16}{relu1_size,14}{"  gradOutput",-14}{If(relu1_size >= min_gpu, "GPU", "CPU 回退")}")
    call console.WriteLine($"    {"pool1 前向",-16}{relu1_size,14}{"  输入 x",-14}{If(relu1_size >= min_gpu, "GPU", "CPU 回退")}")
    call console.WriteLine($"    {"pool1 反向",-16}{pool1_size,14}{"  gradOutput",-14}{If(pool1_size >= min_gpu, "GPU", "CPU 回退")}")
    call console.WriteLine($"    {"relu1 前反向",-16}{relu1_size,14}{"  输入 x",-14}{If(relu1_size >= min_gpu, "GPU", "CPU 回退")}")
    call console.WriteLine($"    {"conv2 前向",-16}{pool1_size,14}{"  输入 x",-14}{If(pool1_size >= min_gpu, "GPU", "CPU 回退")}")
    call console.WriteLine($"    {"conv2 反向",-16}{conv2_size,14}{"  gradOutput",-14}{If(conv2_size >= min_gpu, "GPU", "CPU 回退")}")
    call console.WriteLine($"    {"pool2 前向",-16}{conv2_size,14}{"  输入 x",-14}{If(conv2_size >= min_gpu, "GPU", "CPU 回退")}")
    call console.WriteLine($"    {"pool2 反向",-16}{pool2_size,14}{"  gradOutput",-14}{If(pool2_size >= min_gpu, "GPU", "CPU 回退")}")
    call console.WriteLine($"    {"relu2 前反向",-16}{conv2_size,14}{"  输入 x",-14}{If(conv2_size >= min_gpu, "GPU", "CPU 回退")}")
    call console.WriteLine($"    {"fc  MatMul",-16}{fc_gemm,14}{"  m*k*n",-14}{If(fc_gemm >= min_gemm, "GPU", "CPU 回退")}")
    call console.WriteLine($"    {"softmax",-16}{10,14}{"  输入 x",-14}{If(10 >= min_gpu, "GPU", "CPU 回退")}")

    ' -----------------------------------------------------------------------
    ' 6) CPU vs GPU comparison
    ' -----------------------------------------------------------------------
    call console.WriteLine()
    call console.WriteLine("=== 6) CPU(SIMD) vs CUDA(GPU) ===")

    dim loss_error = System.Math.Abs(cpu_loss - gpu_loss)

    call console.WriteLine($"    {"后端",-14}{"loss",-24}{"accuracy",-18}{"耗时(s)"}")
    call console.WriteLine($"    {"SIMD(CPU)",-14}{cpu_loss,-24:R}{cpu_correct / cpu_total,-18:P2}{cpu_seconds:F3}")
    call console.WriteLine($"    {"CUDA(GPU)",-14}{gpu_loss,-24:R}{gpu_correct / gpu_total,-18:P2}{gpu_seconds:F3}")
    call console.WriteLine()
    call console.WriteLine($"    loss 最大绝对误差 = {loss_error:E3}   " &
                          If(loss_error < 1.0E-9, "OK", "FAIL"))
    call console.WriteLine($"    分类正确数        = CPU {cpu_correct}/{cpu_total}  GPU {gpu_correct}/{gpu_total}   " &
                          If(cpu_correct = gpu_correct, "OK", "差值见上"))
    call console.WriteLine($"    耗时比            = CPU {cpu_seconds:F3}s  GPU {gpu_seconds:F3}s  " &
                          $"比={cpu_seconds / gpu_seconds:F2}x")

    call console.WriteLine()
    call console.WriteLine("    说明: loss 只差舍入量级说明 GPU 与 CPU 的数值实现一致。单样本(N=1)下端到端")
    call console.WriteLine("          并不会比 CPU 更快 —— ILCudaTensor 采用『每次算子调用都 H2D 上传 + 内核")
    call console.WriteLine("          + D2H 读回』的拷贝式执行模型，而池化/全连接/softmax 的规模低于 GPU")
    call console.WriteLine("          阈值、本就回退 CPU。要体现 GPU 吞吐需要 batch 化的大张量。")
else
    call console.WriteLine()
    call console.WriteLine("=== 4-6) 已跳过 ===")
    call console.WriteLine($"    GPU 未启用，CPU 结果 = loss {cpu_loss:R}, {cpu_correct}/{cpu_total} ({cpu_correct / cpu_total:P2})")
end if

call console.WriteLine()
call console.WriteLine("done: mnist-cnn")
