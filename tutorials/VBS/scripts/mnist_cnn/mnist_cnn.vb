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
'  MNIST CNN 的 CUDA 加速 demo（脚本版）
'
'  与 DeepLearning\test 工程里的 MnistCnnGpuTest 是同一份逻辑：
'      1) 环境探测（默认后端 / 随机种子 / 训练配置）
'      2) 用 CPU(SIMD) 后端跑一遍作为基准
'      3) 注册 CUDA 计算引擎；失败则打印诊断并回退 CPU（保证脚本始终能跑出结果）
'      4) 用 CUDA 后端再跑一遍同样的配置
'      5) 打印本网络各张量算子会被门控到 GPU 还是回退 CPU
'      6) CPU vs GPU 的数值一致性、耗时与加速比
'
'  运行：
'      vbs.exe tutorials\VBS\scripts\mnist_cnn\mnist_cnn.vb --mnist-data <MNIST 目录>
'
'  说明：
'    * 网络权值的初始化走共享的“未播种”随机数发生器，不固定种子的话每次运行的初始权值
'      都不同、结果无法比对。因此每跑一遍之前都重新播种，使 CPU 与 GPU 两条路径看到的
'      初始权值与样本顺序完全相同，两者之差只可能来自后端的数值实现。
'    * 训练流程放在下面的 Public Class 里（脚本顶层的 Function 会被引擎重写为匿名函数，
'      既拿不到外层变量也无法反射；类型块则会被原样保留）。
' ============================================================================

''' <summary>
''' 一次“构建网络 -> 训练 -> 评估”的完整流程。
''' </summary>
''' <remarks>
''' 用当前 <c>Tensor.computeKernel</c> 所指向的后端执行，因此同一份代码既能跑 CPU(SIMD)
''' 也能跑 CUDA，便于两者直接对比。
''' </remarks>
Public Class CnnRunner

    ''' <summary>
    ''' 跑一次确定性的训练 + 评估。
    ''' </summary>
    ''' <returns>``{最后一轮 loss, 分类正确数, 样本总数, 耗时秒}``</returns>
    Public Shared Function Execute(imagesPath As String, labelsPath As String,
                                  randomSeed As Integer, passes As Integer, samples As Integer) As Double()
        ' 必须在构建网络（即首次调用 Vector.rand）之前播种
        Call Microsoft.VisualBasic.Math.RandomExtensions.SetSeed(randomSeed)

        Dim reader As New MNIST(imagesPath, labelsPath)

        ' 先把样本物化，保证每轮、每次运行看到的都是同一批、同一顺序的数据
        Dim dataset = reader.ExtractVectors.Take(samples).ToArray

        ' 网络结构与仓库里的 MnistTest 完全一致：
        '   input 28x28x1 -> conv5x32 -> relu -> pool2 -> conv5x64 -> relu -> pool2 -> fc10 -> softmax
        '
        ' 用 LayerBuilder 的 + 运算符做流式搭建，与 R# 的 auto_encoder.R 一一对应：
        '   let cnn = cnn() + input_layer([28,28],1) + conv_layer(5,32,1,2) + pool_layer(2,2,0) + ...
        ' 差别只是 VB 的隐式换行要求二元运算符写在上一行行尾。
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

        Dim correct As Integer = 0

        For Each digit In dataset
            Call db.addImageData(digit.value, digit.value.Max)

            If CInt(Val(digit.description)) = CInt(Which.Max(net.predict(db))) Then
                correct += 1
            End If
        Next

        watch.Stop()

        Return {lastLoss, correct, dataset.Length, watch.Elapsed.TotalSeconds}
    End Function

End Class

' ---------------------------------------------------------------------------
' 0) 命令行参数
' ---------------------------------------------------------------------------
dim mnist_repo = ?"--mnist-data"
dim images_file = $"{mnist_repo}\train-images-idx3-ubyte"
dim labels_file = $"{mnist_repo}\train-labels-idx1-ubyte"

dim random_seed = 12345
dim train_passes = 5
dim train_samples = 300

' ---------------------------------------------------------------------------
' 1) 环境探测
' ---------------------------------------------------------------------------
call console.WriteLine("=== 1) 环境探测 ===")
call console.WriteLine($"    默认后端  = {Tensor.computeKernel.Name}")
call console.WriteLine($"    随机种子  = {random_seed}（权重初始化）")
call console.WriteLine($"    训练配置  = {train_passes} 轮 x {train_samples} 张")

' ---------------------------------------------------------------------------
' 2) CPU(SIMD) 基准
' ---------------------------------------------------------------------------
call console.WriteLine()
call console.WriteLine("=== 2) CPU(SIMD) 训练 + 评估 ===")

dim cpu_run = CnnRunner.Execute(images_file, labels_file, random_seed, train_passes, train_samples)
dim cpu_loss = cpu_run(0)
dim cpu_correct = CInt(cpu_run(1))
dim cpu_total = CInt(cpu_run(2))
dim cpu_seconds = cpu_run(3)

call console.WriteLine($"    后端={Tensor.computeKernel.Name}  loss={cpu_loss:R}  " &
                      $"正确={cpu_correct}/{cpu_total} ({cpu_correct / cpu_total:P2})  耗时={cpu_seconds:F3}s")

' ---------------------------------------------------------------------------
' 3) 注册 CUDA 计算引擎
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
' 4) CUDA(GPU) 训练 + 评估
' ---------------------------------------------------------------------------
if gpu_enabled then
    call console.WriteLine()
    call console.WriteLine("=== 4) CUDA(GPU) 训练 + 评估 ===")

    dim gpu_run = CnnRunner.Execute(images_file, labels_file, random_seed, train_passes, train_samples)
    dim gpu_loss = gpu_run(0)
    dim gpu_correct = CInt(gpu_run(1))
    dim gpu_total = CInt(gpu_run(2))
    dim gpu_seconds = gpu_run(3)

    call console.WriteLine($"    后端={Tensor.computeKernel.Name}  loss={gpu_loss:R}  " &
                          $"正确={gpu_correct}/{gpu_total} ({gpu_correct / gpu_total:P2})  耗时={gpu_seconds:F3}s")

    ' -----------------------------------------------------------------------
    ' 5) 本网络各张量算子的实际执行后端
    '
    '    关键：门控对象各不相同（均已对照 CudaTensor 的实现确认）
    '      * 逐元素算子 / MaxPool2D —— 门控输入张量自身的元素数
    '      * Conv2D 前向          —— 门控**输入** x 的元素数（不是输出！）
    '      * Conv2DBackward*      —— 门控 gradOutput（即本层输出）的元素数
    '      * MatMul               —— 门控 m*k*n
    '    不把门控对象摆出来，很容易把“部分算子走 GPU”误读成“端到端都在 GPU 上”。
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
    ' 6) CPU vs GPU 对比
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
