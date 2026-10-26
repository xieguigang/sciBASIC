Imports System.Diagnostics
Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.Data
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.MachineLearning.DataStorage
Imports Microsoft.VisualBasic.MachineLearning.CNN
Imports Microsoft.VisualBasic.MachineLearning.CNN.data
Imports Microsoft.VisualBasic.MachineLearning.CNN.trainers
Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports Microsoft.VisualBasic.DeepLearning.SpikingNeuralNetwork
Imports Microsoft.VisualBasic.Computing.ILCuda.GPUTensor
Imports Microsoft.VisualBasic.Computing.ILCuda.Runtime
Imports rndf = Microsoft.VisualBasic.Math.RandomExtensions
Imports std = System.Math

''' <summary>
''' 机器学习示例模块：合成数据、指标、CNN 训练（CPU / CUDA 双后端）与脉冲神经网络训练。
''' <para>
''' 所有训练流程都封装在这里（而不是写在脚本里），因此同一段脚本逻辑可以在 VBS 与 NJS
''' 中调用完全相同的 API，并得到一致的结果。
''' </para>
''' </summary>
Public Module ml

    ' ======================================================== 合成数据

    ''' <summary>
    ''' 生成 3 类高斯团簇的合成分类数据（类别标签写入 ``class`` 列，从 0 开始）。
    ''' </summary>
    Public Function gaussianClusters(Optional perClass As Integer = 120, Optional dims As Integer = 8, Optional seed As Integer = 2024) As NumericTable
        Dim rng As New Random(seed)
        Dim centers As Double()() = ClassCenters()
        Dim rows As New List(Of Double())
        Dim classes As New List(Of Double)
        Dim names As New List(Of String)

        For cls As Integer = 0 To centers.Length - 1
            For n As Integer = 1 To perClass
                Dim v(centers(cls).Length - 1) As Double

                For i As Integer = 0 To v.Length - 1
                    Dim value As Double = centers(cls)(i) + 0.09 * rng.NextGaussian(0, 1)

                    v(i) = std.Max(0.02, std.Min(0.98, value))
                Next

                rows.Add(v)
                classes.Add(cls)
                names.Add($"sample_{rows.Count}")
            Next
        Next

        Dim table As NumericTable = NumericTable.FromRows(names.ToArray, rows.ToArray, featureNames("x", dims))

        Return table.SetLabel("class", classes.ToArray)
    End Function

    ' ======================================================== 指标

    ''' <summary>分类准确率。</summary>
    Public Function accuracy(prediction As Integer(), truth As Integer()) As Double
        If prediction Is Nothing OrElse truth Is Nothing OrElse prediction.Length <> truth.Length OrElse prediction.Length = 0 Then
            Return Double.NaN
        End If

        Return SpikingNetwork.Accuracy(prediction, truth)
    End Function

    ''' <summary>混淆矩阵（行 = 真实类别，列 = 预测类别）。</summary>
    Public Function confusion(prediction As Integer(), truth As Integer()) As NumericTable
        Dim k As Integer = std.Max(prediction.Max, truth.Max) + 1
        Dim matrix(k - 1)() As Double

        For i As Integer = 0 To k - 1
            matrix(i) = New Double(k - 1) {}
        Next

        For i As Integer = 0 To truth.Length - 1
            matrix(truth(i))(prediction(i)) += 1
        Next

        Dim names As New List(Of String)

        For i As Integer = 0 To k - 1
            names.Add("class_" & i)
        Next

        Return NumericTable.FromRows(names.ToArray, matrix, names.ToArray)
    End Function

    ' ======================================================== CNN

    ''' <summary>当前张量计算后端名称（SIMD / CUDA）。</summary>
    Public Function backendName() As String
        Return Tensor.computeKernel.Name
    End Function

    ''' <summary>尝试注册 CUDA 张量后端；返回是否成功（失败时保留 CPU 后端）。</summary>
    Public Function useCuda() As Boolean
        Dim options As New EngineOptions()

        Return CudaTensor.Register(options)
    End Function

    ''' <summary>CUDA 后端的算子下放阈值：最小逐元素元素数。</summary>
    Public Function cudaMinGpuElements() As Integer
        Return CudaTensor.MinGpuElements
    End Function

    ''' <summary>CUDA 后端的算子下放阈值：最小 GEMM 规模（m*k*n）。</summary>
    Public Function cudaMinGemmElements() As Integer
        Return CudaTensor.MinGemmElements
    End Function

    ''' <summary>IL→CUDA 内核翻译失败的记录（“内核: 原因”）。</summary>
    Public Function cudaKernelFailures() As String()
        Dim out As New List(Of String)

        For Each item In CudaTensor.KernelFailures
            Call out.Add($"{item.Key}: {item.Value}")
        Next

        Return out.ToArray
    End Function

    ''' <summary>最近一次 CUDA 注册失败的原因。</summary>
    Public Function cudaLastError() As String
        Return CudaTensor.LastError
    End Function

    ''' <summary>
    ''' 用 MNIST 训练一个卷积网络（28x28 输入 → conv5x32 → pool2 → conv5x64 → pool2 → fc10 → softmax）。
    ''' </summary>
    ''' <param name="useCuda">是否在训练前注册 CUDA 张量后端</param>
    Public Function cnnTrain(imagesFile As String, labelsFile As String,
                             Optional seed As Integer = 12345,
                             Optional passes As Integer = 5,
                             Optional samples As Integer = 1000,
                             Optional useCuda As Boolean = False) As CnnReport

        Dim dataset As NumericTable

        Using reader As New MNIST(imagesFile, labelsFile)
            Dim pixels As New List(Of Double())
            Dim classes As New List(Of Double)
            Dim names As New List(Of String)

            For Each digit In reader.ExtractVectors()
                If samples > 0 AndAlso pixels.Count >= samples Then
                    Exit For
                End If

                Call pixels.Add(digit.value.Select(Function(b) CDbl(b)).ToArray)
                Call classes.Add(Val(digit.description))
                Call names.Add($"sample_{pixels.Count + 1}")
            Next

            dataset = NumericTable.FromRows(names.ToArray, pixels.ToArray, featureNames("p", pixels(0).Length))
            Call dataset.SetLabel("class", classes.ToArray)
        End Using

        Return cnnTrainTable(dataset, "class", seed, passes, samples, useCuda)
    End Function

    ''' <summary>
    ''' 用统一的二维表训练卷积网络：每行是一个 28x28 灰度图像（特征列 ``p_1 .. p_784``，
    ''' 取值 0-255 或 0-1 均可），标签列是 0-9 的数字类别。
    ''' </summary>
    Public Function cnnTrainTable(source As NumericTable,
                                  Optional labelName As String = "class",
                                  Optional seed As Integer = 12345,
                                  Optional passes As Integer = 5,
                                  Optional samples As Integer = 0,
                                  Optional useCuda As Boolean = False) As CnnReport

        If useCuda AndAlso Not ml.useCuda() Then
            ' GPU 不可用时保持 CPU 后端，脚本侧通过 report.backend 观察实际后端
        End If

        ' 种子必须在建网之前设置，否则每次运行的初始权重都不同，CPU/GPU 无法对比
        Call rndf.SetSeed(seed)

        Dim report As New CnnReport With {
            .backend = Tensor.computeKernel.Name,
            .samples = If(samples > 0, samples, source.nsamples),
            .passes = passes
        }

        Dim side As Integer = CInt(std.Sqrt(source.nfeatures))
        Dim total As Integer = If(samples > 0, std.Min(samples, source.nsamples), source.nsamples)
        Dim labels As Integer() = source.GetClassLabel(std.Max(0, Array.IndexOf(source.labelNames, labelName)))

        Dim builder As LayerBuilder = New LayerBuilder() +
            input_layer({side, side}, 1) +
            conv_layer(5, 32, 1, 2) +
            relu_layer() +
            pool_layer(2, 2, 0) +
            conv_layer(5, 64, 1, 2) +
            relu_layer() +
            pool_layer(2, 2, 0) +
            full_connected_layer(10) +
            softmax_layer()

        Dim network As New ConvolutionalNN(builder)
        Dim trainer As TrainerAlgorithm = New AdaGradTrainer(20, 0.001F).SetKernel(network)
        Dim db As New DataBlock(side, side, 1, 0)
        Dim watch As Stopwatch = Stopwatch.StartNew()
        Dim lossHistory(passes - 1) As Double
        Dim images As New List(Of Byte())

        For i As Integer = 0 To total - 1
            Call images.Add(ToImage(source.features(i)))
        Next

        For p As Integer = 1 To passes
            Dim passLoss As Double = 0
            Dim check As New PerformanceCounter()

            For i As Integer = 0 To total - 1
                Call db.addImageData(images(i), 255)

                Dim result As TrainResult = trainer.train(db, {CDbl(labels(i))}, check.Set)

                passLoss += result.Loss
            Next

            lossHistory(p - 1) = passLoss / total
        Next

        Dim correct As Double = 0

        For i As Integer = 0 To total - 1
            Call db.addImageData(images(i), 255)

            If labels(i) = CInt(Which.Max(network.predict(db))) Then
                correct += 1
            End If
        Next

        watch.Stop()

        report.loss = lossHistory(lossHistory.Length - 1)
        report.correct = correct
        report.total = total
        report.seconds = watch.Elapsed.TotalSeconds
        report.lossHistory = lossHistory
        report.backend = Tensor.computeKernel.Name

        Return report
    End Function

    ''' <summary>
    ''' 生成一份确定性的"合成手写数字"数据集（10 类，每类一条不同高度的亮带），
    ''' 让 mnist 教程在没有真实 MNIST 数据文件时仍然可以完整运行。
    ''' </summary>
    Public Function syntheticDigits(Optional perClass As Integer = 30, Optional seed As Integer = 2024) As NumericTable
        Dim random As New Random(seed)
        Dim rows As New List(Of Double())
        Dim classes As New List(Of Double)
        Dim names As New List(Of String)

        For cls As Integer = 0 To 9
            Dim bandY As Integer = 2 + cls * 2.4

            For n As Integer = 1 To perClass
                Dim pixels(783) As Double

                For y As Integer = 0 To 27
                    For x As Integer = 0 To 27
                        Dim base As Double = If(std.Abs(y - bandY) <= 1.2 AndAlso x >= 4 AndAlso x <= 23, 0.85, 0.05)
                        Dim noise As Double = random.NextDouble() * 0.1

                        pixels(y * 28 + x) = std.Min(1.0, base + noise)
                    Next
                Next

                Call rows.Add(pixels)
                Call classes.Add(cls)
                Call names.Add($"digit_{rows.Count}")
            Next
        Next

        Dim table As NumericTable = NumericTable.FromRows(names.ToArray, rows.ToArray, featureNames("p", 784))

        Return table.SetLabel("class", classes.ToArray)
    End Function

    ''' <summary>
    ''' 本网络每个张量算子的实际执行后端（按当前下放阈值推算），返回可直接打印的文本行。
    ''' </summary>
    Public Function cnnOperatorTable() As String()
        Dim minGpu As Integer = CudaTensor.MinGpuElements
        Dim minGemm As Integer = CudaTensor.MinGemmElements
        Dim imageSize As Integer = 28
        Dim relu1 As Integer = 28 * 28 * 32
        Dim pool1 As Integer = 14 * 14 * 32
        Dim conv2 As Integer = 14 * 14 * 64
        Dim pool2 As Integer = 7 * 7 * 64
        Dim fc As Integer = 10 * pool2 * 1
        Dim lines As New List(Of String)

        Call lines.Add($"    {"operator",-16}{"size",14}{"  gating operand",-20}verdict")

        Dim add As Action(Of String, Integer, String, Integer) =
            Sub(name As String, size As Integer, gating As String, threshold As Integer)
                Call lines.Add($"    {name,-16}{size,14}{"  " & gating,-20}{If(size >= threshold, "GPU", "CPU fallback")}")
            End Sub

        Call add("conv1 fwd", imageSize * imageSize, "input x", minGpu)
        Call add("conv1 bwd", relu1, "gradOutput", minGpu)
        Call add("pool1 fwd", relu1, "input x", minGpu)
        Call add("pool1 bwd", pool1, "gradOutput", minGpu)
        Call add("relu1 fwd/bwd", relu1, "input x", minGpu)
        Call add("conv2 fwd", pool1, "input x", minGpu)
        Call add("conv2 bwd", conv2, "gradOutput", minGpu)
        Call add("pool2 fwd", conv2, "input x", minGpu)
        Call add("pool2 bwd", pool2, "gradOutput", minGpu)
        Call add("relu2 fwd/bwd", conv2, "input x", minGpu)
        Call add("fc  MatMul", fc, "m*k*n", minGemm)
        Call add("softmax", 10, "input x", minGpu)

        Return lines.ToArray
    End Function

    ' ======================================================== 脉冲神经网络

    ''' <summary>
    ''' 训练一个 LIF 脉冲神经网络做 3 类高斯团簇分类，并返回训练报告。
    ''' </summary>
    Public Function snnTrain(Optional perClass As Integer = 120,
                             Optional epochs As Integer = 40,
                             Optional seed As Integer = 2024,
                             Optional hidden As Integer = 64,
                             Optional timeSteps As Integer = 30,
                             Optional batchSize As Integer = 36,
                             Optional testPerClass As Integer = 30) As SnnReport

        Dim train As NumericTable = gaussianClusters(perClass, 8, seed)
        Dim test As NumericTable = gaussianClusters(testPerClass, 8, seed + 1)
        Dim dims As Integer = train.nfeatures

        Dim network As New SpikingNetwork(dims, timeSteps, SpikeEncoding.RateCoding) With {
            .Rng = New Random(31415)
        }

        Call network.AddLayer(hidden, beta:=0.85, threshold:=1.0, seed:=11)
        Call network.AddLayer(3, beta:=0.85, threshold:=1.0, seed:=22)

        Dim optimizer As New AdamOptimizer(0.002, 1.0)
        Dim testX As Tensor = TensorHelper.AllTensor(test)
        Dim testLabels As Integer() = test.GetClassLabel(0)
        Dim lossHistory As New List(Of Double)
        Dim accuracyHistory As New List(Of Double)
        Dim order As Integer() = Enumerable.Range(0, train.nsamples).ToArray

        For epoch As Integer = 1 To epochs
            Call rndf.Shuffle(order)

            Dim lossSum As Double = 0
            Dim batches As Integer = 0
            Dim trainLabels As Integer() = train.GetClassLabel(0)

            For start As Integer = 0 To order.Length - 1 Step batchSize
                Dim count As Integer = std.Min(batchSize, order.Length - start)
                Dim bx As Tensor = TensorHelper.BatchTensor(train.features, order, start, count)
                Dim by(count - 1) As Integer

                For b As Integer = 0 To count - 1
                    by(b) = trainLabels(order(start + b))
                Next

                lossSum += network.TrainStep(bx, by, optimizer)
                batches += 1
            Next

            Call lossHistory.Add(lossSum / batches)
            Call accuracyHistory.Add(SpikingNetwork.Accuracy(network.Predict(testX), testLabels))
        Next

        ' 单样本推理轨迹：输入脉冲栅格 + 输出层脉冲计数
        Dim sample As New Tensor(test.features(0), 1, dims)
        Dim sequence As List(Of Tensor) = SpikeEncoders.RateEncode(sample, network.TimeSteps, New Random(777))

        For Each layer As LIFLayer In network.Layers
            Call layer.ResetState(1)
        Next

        Dim outSpikes As New List(Of Tensor)

        For t As Integer = 0 To network.TimeSteps - 1
            Dim signal As Tensor = sequence(t)

            For Each layer As LIFLayer In network.Layers
                signal = layer.ForwardStep(signal)
            Next

            Call outSpikes.Add(signal)
        Next

        Return New SnnReport With {
            .accuracy = accuracyHistory(accuracyHistory.Count - 1),
            .epochs = epochs,
            .timeSteps = timeSteps,
            .hidden = hidden,
            .trainSamples = train.nsamples,
            .testSamples = test.nsamples,
            .lossHistory = lossHistory.ToArray,
            .accuracyHistory = accuracyHistory.ToArray,
            .inputRaster = raster(sequence.Select(Function(t) t.Data).ToArray, dims),
            .outputRaster = outputRaster(outSpikes, 3, test.GetLabel(0)(0))
        }
    End Function

    ''' <summary>把一组数值压缩成 ``▁▂▃▄▅▆▇█`` 形式的火花线。</summary>
    Public Function sparkline(values As Double()) As String
        Const blocks As String = "▁▂▃▄▅▆▇█"

        Dim mx As Double = values.Max
        Dim mn As Double = values.Min
        Dim span As Double = std.Max(mx - mn, 0.0000001)
        Dim sb As New StringBuilder()

        For Each value As Double In values
            Dim k As Integer = CInt(std.Floor((value - mn) / span * 7.999))

            If k < 0 Then k = 0
            If k > 7 Then k = 7

            Call sb.Append(blocks(k))
        Next

        Return sb.ToString
    End Function

    ''' <summary>
    ''' 把脉冲帧序列渲染成栅格文本（每行一个神经元，█ 表示有脉冲）。
    ''' </summary>
    ''' <param name="frames">帧序列，每个元素是一帧、长度为神经元数</param>
    ''' <param name="rows">要渲染的神经元行数</param>
    Public Function raster(frames As Double()(), rows As Integer) As String
        Dim sb As New StringBuilder()

        For r As Integer = 0 To rows - 1
            Call sb.Append($"    in{r,-2}|")

            For t As Integer = 0 To frames.Length - 1
                Call sb.Append(If(frames(t)(r) > 0, "█", "·"))
            Next

            Call sb.AppendLine("|")
        Next

        Return sb.ToString.TrimEnd
    End Function

    ' ======================================================== 内部辅助

    Private Function ClassCenters() As Double()()
        Return {
            New Double() {0.2, 0.2, 0.75, 0.75, 0.5, 0.5, 0.2, 0.8},
            New Double() {0.75, 0.75, 0.2, 0.2, 0.8, 0.2, 0.5, 0.5},
            New Double() {0.5, 0.8, 0.5, 0.8, 0.2, 0.75, 0.8, 0.2}
        }
    End Function

    ''' <summary>把一行的灰度特征（0-1 或 0-255）转成 8 位像素。</summary>
    Private Function ToImage(pixels As Double()) As Byte()
        Dim out(pixels.Length - 1) As Byte
        Dim scale As Double = If(pixels.Max <= 1.0, 255.0, 1.0)

        For i As Integer = 0 To pixels.Length - 1
            out(i) = CByte(std.Round(std.Max(0.0, std.Min(255.0, pixels(i) * scale))))
        Next

        Return out
    End Function

    Private Function featureNames(prefix As String, n As Integer) As String()
        Dim out(n - 1) As String

        For i As Integer = 0 To n - 1
            out(i) = prefix & "_" & (i + 1)
        Next

        Return out
    End Function

    Private Function outputRaster(spikes As List(Of Tensor), units As Integer, truth As Double) As String
        Dim sb As New StringBuilder()
        Dim counts(units - 1) As Integer

        For j As Integer = 0 To units - 1
            Dim line As New StringBuilder()

            For t As Integer = 0 To spikes.Count - 1
                Dim v As Double = spikes(t).Data(j)

                If v > 0 Then counts(j) += 1

                Call line.Append(If(v > 0, "█", "·"))
            Next

            Dim mark As String = If(j = CInt(truth), " ← 真实类别", "")

            Call sb.AppendLine($"    out{j} |{line.ToString}| 计数={counts(j)}{mark}")
        Next

        Dim best As Integer = 0

        For j As Integer = 1 To units - 1
            If counts(j) > counts(best) Then best = j
        Next

        Call sb.AppendLine($"    → 预测类别: {best}（真实: {CInt(truth)}） {If(best = CInt(truth), "[正确]", "[错误]")}")

        Return sb.ToString.TrimEnd
    End Function

    ''' <summary>CNN 训练报告。</summary>
    Public Class CnnReport

        ''' <summary>训练结束时的张量计算后端名称。</summary>
        Public Property backend As String
        ''' <summary>最后一轮的训练损失。</summary>
        Public Property loss As Double
        ''' <summary>训练集上分类正确的样本数。</summary>
        Public Property correct As Double
        ''' <summary>训练集样本总数。</summary>
        Public Property total As Double
        ''' <summary>训练耗时（秒）。</summary>
        Public Property seconds As Double
        ''' <summary>每轮的训练损失。</summary>
        Public Property lossHistory As Double()
        ''' <summary>训练轮数。</summary>
        Public Property passes As Integer
        ''' <summary>参与训练的样本数上限。</summary>
        Public Property samples As Integer

        ''' <summary>训练集分类准确率。</summary>
        Public ReadOnly Property accuracy As Double
            Get
                Return If(total <= 0, 0, correct / total)
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return $"backend={backend} loss={loss:R} correct={correct}/{total} ({accuracy:P2}) elapsed={seconds:F3}s"
        End Function
    End Class

    ''' <summary>脉冲神经网络训练报告。</summary>
    Public Class SnnReport

        ''' <summary>测试集准确率。</summary>
        Public Property accuracy As Double
        ''' <summary>训练轮数。</summary>
        Public Property epochs As Integer
        ''' <summary>每个样本的仿真时间步数。</summary>
        Public Property timeSteps As Integer
        ''' <summary>隐藏层神经元数量。</summary>
        Public Property hidden As Integer
        ''' <summary>训练集样本数。</summary>
        Public Property trainSamples As Integer
        ''' <summary>测试集样本数。</summary>
        Public Property testSamples As Integer
        ''' <summary>每轮的平均训练损失。</summary>
        Public Property lossHistory As Double()
        ''' <summary>每轮的测试集准确率。</summary>
        Public Property accuracyHistory As Double()
        ''' <summary>输入层脉冲栅格文本。</summary>
        Public Property inputRaster As String
        ''' <summary>输出层脉冲栅格文本。</summary>
        Public Property outputRaster As String

        Public Overrides Function ToString() As String
            Return $"snn accuracy={accuracy:P2} epochs={epochs} train={trainSamples} test={testSamples}"
        End Function
    End Class
End Module
