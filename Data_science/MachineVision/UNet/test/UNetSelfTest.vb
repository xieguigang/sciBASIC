Imports System.IO
Imports Microsoft.VisualBasic.Imaging.BitmapImage.Png
Imports Microsoft.VisualBasic.MachineLearning.CNN
Imports Microsoft.VisualBasic.MachineLearning.CNN.data
Imports Microsoft.VisualBasic.MachineLearning.CNN.layers
Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports Microsoft.VisualBasic.MachineLearning.TensorFlow.Compute
Imports Microsoft.VisualBasic.MachineVision.UNet.UNet
Imports gpu = Microsoft.VisualBasic.Computing.ILCuda.GPUTensor
Imports std = System.Math

''' <summary>
''' U-Net 的断言自检。
''' </summary>
''' <remarks>
''' 分两类：
''' <list type="number">
''' <item>不依赖外部数据的**数学断言**（结构、转置卷积、拼接、数值梯度、混合精度、存档）；</item>
''' <item>依赖数据集的**管线断言**（PNG 解码、RLE 方向交叉验证、真实样本过拟合）。</item>
''' </list>
''' 第二类在数据目录不存在时明确报告"跳过"，绝不算作通过。
''' </remarks>
Public Module UNetSelfTest

    Private passed As Integer = 0
    Private failed As Integer = 0
    Private skipped As Integer = 0

    Private Sub Check(name As String, ok As Boolean, Optional detail As String = Nothing)
        If ok Then
            passed += 1
            Console.WriteLine($"    [PASS] {name}")
        Else
            failed += 1
            Console.WriteLine($"    [FAIL] {name}" & If(detail Is Nothing, "", $"  -- {detail}"))
        End If
    End Sub

    Private Sub Section(title As String)
        Console.WriteLine()
        Console.WriteLine($">> {title}")
    End Sub

    ''' <summary>
    ''' 跑全部断言。
    ''' </summary>
    ''' <param name="cfg">运行配置。</param>
    ''' <param name="useGpu">是否额外做 GPU 对拍。</param>
    ''' <returns>0 表示全部通过。</returns>
    Public Function Run(cfg As RunConfig, useGpu As Boolean) As Integer
        passed = 0
        failed = 0
        skipped = 0

        Console.WriteLine("=== U-Net 自检 ===")
        Console.WriteLine($"    {cfg}")
        Console.WriteLine()

        ' 默认先跑 CPU；GPU 对拍放到最后（注册之后后端就变了）
        Call PrintEnvironment(cfg)

        Dim dataReady = Directory.Exists(System.IO.Path.Combine(cfg.DataRoot, DsbDataset.TrainFolder))

        Call TestStructure()
        Call TestTransposedConv()
        Call TestConcatLayer()
        Call TestNumericGradient()

        If dataReady Then
            Call TestPngRoundTrip(cfg)
            Call TestRleDirection(cfg)
            Call TestRealDataOverfit(cfg)
        Else
            Section("真实数据管线")
            Console.WriteLine($"    [跳过] 找不到数据集目录: {cfg.DataRoot}")
            skipped += 3
        End If

        Call TestMixedPrecision()
        Call TestSaveLoad()

        If useGpu Then
            Call TestGpuParity()
        End If

        Console.WriteLine()
        Console.WriteLine($"=== 自检结束: 通过 {passed}, 失败 {failed}, 跳过 {skipped} ===")

        Return If(failed = 0, 0, 1)
    End Function

    ' ---------------- 1) 结构 ----------------

    Private Sub TestStructure()
        Section("1) 网络结构与张量形状")

        Dim opt As New UNetOptions With {.InputWidth = 32, .InputHeight = 32, .InputChannels = 1,
                                         .NumClasses = 2, .Levels = 2, .BaseFilters = 4}
        Dim model As New UNetModel(opt)
        Dim block = model.CreateInput()
        Dim rng As New Random(11)
        Dim pixels(block.Weights.Length - 1) As Double

        For i As Integer = 0 To pixels.Length - 1
            pixels(i) = rng.NextDouble() - 0.5
        Next

        Call block.addImageData(pixels, 1.0)

        Dim logits = model.Forward(block, training:=False)

        Check("输出形状 = 输入空间尺寸 × 类别数",
              logits.SX = 32 AndAlso logits.SY = 32 AndAlso logits.Depth = 2,
              $"实际 {logits.SX}x{logits.SY}x{logits.Depth}")
        Check("参数块数量 > 0", model.Parameters.Length > 0, $"{model.Parameters.Length}")
        Check("参数总量 > 0", model.ParameterCount > 0, $"{model.ParameterCount}")
        Check("所有输出有限", logits.Weights.All(Function(v) Double.IsFinite(v)))

        Dim mask = model.Segment(block)

        Check("分割掩码长度 = 像素数", mask.Length = 32 * 32)
        Check("分割掩码取值合法", mask.All(Function(c) c >= 0 AndAlso c < 2))
    End Sub

    ' ---------------- 2) 转置卷积 ----------------

    Private Sub TestTransposedConv()
        Section("2) 转置卷积: 形状公式与前向数值")

        ' 尺寸公式 out = stride*(in-1) + kernel - 2*padding
        Dim def1 As New OutputDefinition(8, 8, 4)
        Dim l1 As New Conv2DTransposeLayer(def1, 2, 4, 2, 0)
        Check("kernel=2,stride=2,pad=0: 8 -> 16, depth 4",
              def1.outX = 16 AndAlso def1.outY = 16 AndAlso def1.depth = 4,
              $"{def1.outX}x{def1.outY}x{def1.depth}")

        Dim def2 As New OutputDefinition(8, 8, 4)
        Dim l2 As New Conv2DTransposeLayer(def2, 3, 6, 2, 1)
        Check("kernel=3,stride=2,pad=1: 8 -> 15, depth 6",
              def2.outX = 15 AndAlso def2.outY = 15 AndAlso def2.depth = 6,
              $"{def2.outX}x{def2.outY}x{def2.depth}")

        Dim def3 As New OutputDefinition(8, 8, 4)
        Dim l3 As New Conv2DTransposeLayer(def3, 4, 4, 2, 0)
        Check("kernel=4,stride=2,pad=0: 8 -> 18, depth 4",
              def3.outX = 18 AndAlso def3.outY = 18 AndAlso def3.depth = 4,
              $"{def3.outX}x{def3.outY}x{def3.depth}")

        ' 前向数值：拿"散射"定义做一遍暴力参考实现来对照
        Dim inSize = 4, inCh = 2, outCh = 3, kernel = 2, stride = 2, padding = 0
        Dim def As New OutputDefinition(inSize, inSize, inCh)
        Dim layer As New Conv2DTransposeLayer(def, kernel, outCh, stride, padding, useBias:=True)

        Check("转置卷积输出块尺寸", def.outX = 8 AndAlso def.outY = 8 AndAlso def.depth = outCh,
              $"{def.outX}x{def.outY}x{def.depth}")

        Dim blocks = layer.BackPropagationResult.ToArray()
        Dim rng As New Random(23)

        For Each b In blocks
            For i As Integer = 0 To b.Weights.Length - 1
                b.Weights(i) = rng.NextDouble() * 2 - 1
            Next
            Call b.NotifyModified()
        Next

        Dim inputBlock As New DataBlock(inSize, inSize, inCh, 0.0)

        For i As Integer = 0 To inputBlock.Weights.Length - 1
            Call inputBlock.setWeight(i, rng.NextDouble() * 2 - 1)
        Next

        Dim outBlock = layer.forward(inputBlock, training:=False)
        Dim outSize = def.outX

        ' 暴力参考: out[y, x, d] = Σ_{kh,kw,c} in[i, j, c] * W_d[kh, kw, c] + bias[d],
        ' 其中 y = i*stride + kh - padding
        Dim expected(outSize * outSize * outCh - 1) As Double
        Dim x = inputBlock.Weights

        For i As Integer = 0 To inSize - 1
            For j As Integer = 0 To inSize - 1
                For kh As Integer = 0 To kernel - 1
                    Dim y = i * stride + kh - padding

                    If y < 0 OrElse y >= outSize Then Continue For

                    For kw As Integer = 0 To kernel - 1
                        Dim xx = j * stride + kw - padding

                        If xx < 0 OrElse xx >= outSize Then Continue For

                        For d As Integer = 0 To outCh - 1
                            Dim filterBlock = blocks(d)

                            For c As Integer = 0 To inCh - 1
                                Dim wIndex = (kernel * kh + kw) * inCh + c
                                Dim inIndex = (inSize * i + j) * inCh + c
                                Dim outIndex = (outSize * y + xx) * outCh + d

                                expected(outIndex) += x(inIndex) * filterBlock.Weights(wIndex)
                            Next
                        Next
                    Next
                Next
            Next
        Next

        For d As Integer = 0 To outCh - 1
            Dim biasValue = blocks(outCh).Weights(d)

            For p As Integer = 0 To outSize * outSize - 1
                expected(p * outCh + d) += biasValue
            Next
        Next

        Dim maxDiff As Double = 0

        For i As Integer = 0 To expected.Length - 1
            maxDiff = std.Max(maxDiff, std.Abs(expected(i) - outBlock.Weights(i)))
        Next

        Check("转置卷积前向与暴力参考实现一致", maxDiff < 1.0E-9, $"max|diff| = {maxDiff:E3}")
    End Sub

    ' ---------------- 3) 拼接层 ----------------

    Private Sub TestConcatLayer()
        Section("3) 拼接层: 中心裁剪 + 按通道拼接 + 梯度分流")

        Dim mainWidth = 4, mainHeight = 4, mainDepth = 2
        Dim skipWidth = 8, skipHeight = 8, skipDepth = 3
        Dim def As New OutputDefinition(mainWidth, mainHeight, mainDepth)
        Dim layer As New ConcatLayer(def, skipDepth)

        Check("拼接后通道数 = 主分支 + 跳跃分支",
              def.depth = mainDepth + skipDepth, $"{def.depth}")

        Dim main As New DataBlock(mainWidth, mainHeight, mainDepth, 0.0)
        Dim skip As New DataBlock(skipWidth, skipHeight, skipDepth, 0.0)

        For i As Integer = 0 To main.Weights.Length - 1
            Call main.setWeight(i, i + 1)
        Next
        For i As Integer = 0 To skip.Weights.Length - 1
            Call skip.setWeight(i, 100 + i)
        Next

        Call layer.SetSkip(skip)

        Dim merged = layer.forward(main, training:=False)

        Check("拼接输出空间尺寸 = 主分支",
              merged.SX = mainWidth AndAlso merged.SY = mainHeight, $"{merged.SX}x{merged.SY}")
        Check("拼接输出通道数", merged.Depth = mainDepth + skipDepth, $"{merged.Depth}")

        ' 期望：前 mainDepth 个通道来自主分支，其余来自跳跃块中心 4x4
        Dim ok As Boolean = True
        Dim crop = (skipWidth - mainWidth) \ 2

        For y As Integer = 0 To mainHeight - 1
            For xx As Integer = 0 To mainWidth - 1
                Dim mBase = (y * mainWidth + xx) * (mainDepth + skipDepth)
                Dim sBase = ((y + crop) * skipWidth + (xx + crop)) * skipDepth

                For c As Integer = 0 To mainDepth - 1
                    Dim src = (y * mainWidth + xx) * mainDepth + c
                    If std.Abs(merged.Weights(mBase + c) - main.Weights(src)) > 1.0E-12 Then ok = False
                Next
                For c As Integer = 0 To skipDepth - 1
                    If std.Abs(merged.Weights(mBase + mainDepth + c) - skip.Weights(sBase + c)) > 1.0E-12 Then ok = False
                Next
            Next
        Next

        Check("拼接内容 = 主分支 + 中心裁剪后的跳跃分支", ok)

        ' 反向：把输出梯度灌成 1..N，检查两段各回各的家
        Dim grads(merged.Weights.Length - 1) As Double

        For i As Integer = 0 To grads.Length - 1
            grads(i) = i + 1
        Next

        Call merged.setGradient(grads)
        Call layer.backward()

        ' 期望的跳跃分支梯度：只有裁剪区内有值，其余为 0
        Dim expectSkip(skip.Gradients.Length - 1) As Double

        For y As Integer = 0 To mainHeight - 1
            For xx As Integer = 0 To mainWidth - 1
                Dim mBase = (y * mainWidth + xx) * (mainDepth + skipDepth)
                Dim sBase = ((y + crop) * skipWidth + (xx + crop)) * skipDepth

                For c As Integer = 0 To skipDepth - 1
                    expectSkip(sBase + c) = grads(mBase + mainDepth + c)
                Next
            Next
        Next

        Dim okMain As Boolean = True
        Dim okSkip As Boolean = True

        For y As Integer = 0 To mainHeight - 1
            For xx As Integer = 0 To mainWidth - 1
                Dim mBase = (y * mainWidth + xx) * (mainDepth + skipDepth)

                For c As Integer = 0 To mainDepth - 1
                    Dim expect = grads(mBase + c)

                    If std.Abs(main.Gradients((y * mainWidth + xx) * mainDepth + c) - expect) > 1.0E-12 Then okMain = False
                Next
            Next
        Next

        For i As Integer = 0 To expectSkip.Length - 1
            If std.Abs(skip.Gradients(i) - expectSkip(i)) > 1.0E-12 Then okSkip = False
        Next

        Check("拼接反向: 主分支拿到对应通道的梯度", okMain)
        Check("拼接反向: 跳跃分支只在裁剪区内拿到对应通道的梯度", okSkip)

        Dim outsideZero As Boolean = True

        For i As Integer = 0 To expectSkip.Length - 1
            If expectSkip(i) = 0 AndAlso skip.Gradients(i) <> 0 Then outsideZero = False
        Next

        Check("拼接反向: 裁剪区域之外不写梯度", outsideZero)
    End Sub

    ' ---------------- 4) 数值梯度 ----------------

    Private Sub TestNumericGradient()
        Section("4) 整网数值梯度校验 (中心差分 vs 反向传播)")

        Dim savedMixed = MixedPrecisionPolicy.Enabled
        Dim savedRouter = PrecisionRouter.Enabled
        Dim savedThreshold = MixedPrecisionPolicy.ConvOpsThreshold

        ' 这一项必须是纯双精度的对照；否则 fp32 的舍入会淹没差分比较
        MixedPrecisionPolicy.Enabled = False
        PrecisionRouter.Enabled = False

        Dim opt As New UNetOptions With {.InputWidth = 16, .InputHeight = 16, .InputChannels = 1,
                                         .NumClasses = 2, .Levels = 2, .BaseFilters = 2}
        Dim model As New UNetModel(opt)
        ' 只用**一个**样本：解析梯度是"累加"语义，而数值梯度用的是该样本的损失，
        ' 样本数不一致会让两边差一个倍数（这正是这个断言要盯住的那类错误）
        Dim samples = SyntheticSamples.Generate(1, 16, seed:=99)
        Dim classWeights As Double() = Nothing
        Dim blocks = model.Parameters

        ' 解析梯度
        Call ZeroGradients(model)

        For Each sample In samples
            Dim block = model.CreateInput()
            Call sample.WriteTo(block)

            Dim logits = model.Forward(block, training:=False)
            Dim grad As Double() = Nothing

            Call SegmentationLoss.CrossEntropy(logits, sample.Labels, classWeights, grad)
            Call model.Backward(grad)
        Next

        Dim analytic = blocks.Select(Function(b) CType(b.Gradients.Clone(), Double())).ToArray()

        ' 数值梯度：随机抽 14 个参数
        Dim rng As New Random(4242)
        Dim eps = 0.0005
        Dim worst As Double = 0
        Dim worstText As String = ""

        For probe As Integer = 1 To 14
            Dim b = rng.Next(blocks.Length)
            Dim j = rng.Next(blocks(b).Weights.Length)
            Dim original = blocks(b).Weights(j)

            blocks(b).Weights(j) = original + eps
            Call blocks(b).NotifyModified()
            Dim plus = TotalLoss(model, samples, classWeights)

            blocks(b).Weights(j) = original - eps
            Call blocks(b).NotifyModified()
            Dim minus = TotalLoss(model, samples, classWeights)

            blocks(b).Weights(j) = original
            Call blocks(b).NotifyModified()

            Dim numeric = (plus - minus) / (2 * eps)
            Dim scale = std.Max(1.0, std.Abs(numeric))
            Dim relative = std.Abs(numeric - analytic(b)(j)) / scale

            If relative > worst Then
                worst = relative
                worstText = $"block#{b}[{j}]: 解析 {analytic(b)(j):E5} vs 数值 {numeric:E5}"
            End If
        Next

        Check("解析梯度与中心差分一致 (相对误差 < 1e-4)", worst < 0.0001, $"最差 {worst:E3} -- {worstText}")

        ' 梯度必须真的非零，否则上面那条断言可能"两边都是 0"地假通过
        Dim gradNorm As Double = 0

        For Each g In analytic
            For Each v In g
                gradNorm += v * v
            Next
        Next

        Check("解析梯度非全零", gradNorm > 0, $"||g||^2 = {gradNorm:E3}")

        MixedPrecisionPolicy.Enabled = savedMixed
        PrecisionRouter.Enabled = savedRouter
        MixedPrecisionPolicy.ConvOpsThreshold = savedThreshold
    End Sub

    Private Sub ZeroGradients(model As UNetModel)
        For Each b In model.Parameters
            Dim g = b.Gradients

            For i As Integer = 0 To g.Length - 1
                g(i) = 0.0
            Next

            Call b.NotifyModified()
        Next
    End Sub

    Private Function TotalLoss(model As UNetModel, samples As SegmentationSample(), classWeights As Double()) As Double
        Dim total As Double = 0

        For Each sample In samples
            Dim block = model.CreateInput()
            Call sample.WriteTo(block)

            Dim logits = model.Forward(block, training:=False)
            Dim grad As Double() = Nothing

            total += SegmentationLoss.CrossEntropy(logits, sample.Labels, classWeights, grad)
        Next

        Return total / samples.Length
    End Function

    ' ---------------- 5) PNG 解码 ----------------

    Private Sub TestPngRoundTrip(cfg As RunConfig)
        Section("5) PNG 解码与回写 (自包含解码器)")

        Dim trainDir = System.IO.Path.Combine(cfg.DataRoot, DsbDataset.TrainFolder)
        Dim firstImage = Directory.GetDirectories(trainDir) _
            .Select(Function(d) System.IO.Path.Combine(d, "images")) _
            .Where(Function(d) Directory.Exists(d)) _
            .SelectMany(Function(d) Directory.GetFiles(d, "*.png")) _
            .FirstOrDefault()

        If firstImage Is Nothing Then
            Console.WriteLine("    [跳过] 训练集里找不到 PNG")
            skipped += 1
            Return
        End If

        Dim image = PngReader.Decode(firstImage, toGray:=True)

        Check("解码尺寸有效", image.Width > 0 AndAlso image.Height > 0, $"{image.Width}x{image.Height}")
        Check("解码结果不是全 0", Not image.IsBlank)
        Check("像素落在 [0,1]", image.Pixels.All(Function(v) v >= 0 AndAlso v <= 1))

        Dim temp = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "unet_selftest_roundtrip.png")

        Call PngWriter.WriteGray(temp, image.Width, image.Height, image.Pixels)
        Dim again = PngReader.Decode(temp, toGray:=True)

        Dim maxDiff As Double = 0

        For i As Integer = 0 To image.Pixels.Length - 1
            maxDiff = std.Max(maxDiff, std.Abs(image.Pixels(i) - again.Pixels(i)))
        Next

        Check("写出的 PNG 能被自己的解码器读回 (8 位量化误差内)", maxDiff <= 1.0 / 255.0 + 1.0E-9, $"max|diff| = {maxDiff:E3}")

        Dim firstMask = Directory.GetDirectories(trainDir) _
            .Select(Function(d) System.IO.Path.Combine(d, "masks")) _
            .Where(Function(d) Directory.Exists(d)) _
            .SelectMany(Function(d) Directory.GetFiles(d, "*.png")) _
            .FirstOrDefault()

        If firstMask IsNot Nothing Then
            Dim mask = PngReader.Decode(firstMask, toGray:=True)
            Dim anyFg = mask.Pixels.Any(Function(v) v > 0.5)
            Dim anyBg = mask.Pixels.Any(Function(v) v <= 0.5)

            Check("实例掩膜同时含前景与背景", anyFg AndAlso anyBg)
        End If

        Try
            System.IO.File.Delete(temp)
        Catch
        End Try
    End Sub

    ' ---------------- 6) RLE 方向 ----------------

    Private Sub TestRleDirection(cfg As RunConfig)
        Section("6) RLE 展开方向交叉验证 (用 stage1_train 的 masks PNG 做基准)")

        Dim labelFile = System.IO.Path.Combine(cfg.DataRoot, DsbDataset.TrainLabelFile)

        If Not System.IO.File.Exists(labelFile) Then
            Console.WriteLine($"    [跳过] 找不到 {DsbDataset.TrainLabelFile}")
            skipped += 1
            Return
        End If

        Dim trainDir = System.IO.Path.Combine(cfg.DataRoot, DsbDataset.TrainFolder)
        Dim labels = DsbDataset.ReadSolution(labelFile)
        Dim dirs = Directory.GetDirectories(trainDir).Where(Function(d) labels.ContainsKey(System.IO.Path.GetFileName(d))).Take(6).ToArray()

        If dirs.Length = 0 Then
            Console.WriteLine("    [跳过] 训练目录与标签文件没有交集")
            skipped += 1
            Return
        End If

        Dim colSum As Double = 0
        Dim rowSum As Double = 0
        Dim n As Integer = 0

        For Each d In dirs
            Dim id = System.IO.Path.GetFileName(d)
            Dim imageFile = Directory.GetFiles(System.IO.Path.Combine(d, "images"), "*.png").FirstOrDefault()

            If imageFile Is Nothing Then Continue For

            Dim image = PngReader.Decode(imageFile, toGray:=True)
            Dim pngMask = DsbDataset.LoadMaskFromPng(System.IO.Path.Combine(d, "masks"), image.Width, image.Height)

            If pngMask Is Nothing Then Continue For

            Dim colMask = RleCodec.Merge(labels(id), image.Height, image.Width, True)
            Dim rowMask = RleCodec.Merge(labels(id), image.Height, image.Width, False)
            Dim iouCol = RleCodec.IoU(pngMask, colMask)
            Dim iouRow = RleCodec.IoU(pngMask, rowMask)

            colSum += iouCol
            rowSum += iouRow
            n += 1

            Console.WriteLine($"      {id.Substring(0, 8)}  IoU(列主序)={iouCol:F4}  IoU(行主序)={iouRow:F4}")
        Next

        If n = 0 Then
            Console.WriteLine("    [跳过] 没有可比对的样本")
            skipped += 1
            Return
        End If

        Dim colMean = colSum / n
        Dim rowMean = rowSum / n
        Dim chosen = If(cfg.RleColumnMajor, colMean, rowMean)
        Dim other = If(cfg.RleColumnMajor, rowMean, colMean)

        Console.WriteLine($"    平均 IoU: 列主序 {colMean:F4}, 行主序 {rowMean:F4} (样本 {n} 张)")

        Check($"配置的展开方向({If(cfg.RleColumnMajor, "列主序", "行主序")})与 PNG 掩膜吻合 (IoU > 0.9)",
              chosen > 0.9, $"{chosen:F4}")
        Check("配置方向明显优于另一个方向", chosen > other + 0.2, $"{chosen:F4} vs {other:F4}")
    End Sub

    ' ---------------- 7) 真实数据过拟合 ----------------

    Private Sub TestRealDataOverfit(cfg As RunConfig)
        Section("7) 真实数据小样本过拟合")

        Dim size = 32
        ' 过拟合用的小样本：2 张图训 50 轮，量级在百来个前向/反向，既能验证"学得动"又不拖慢自检
        Dim samples = DsbDataset.LoadTrain(cfg.DataRoot, size, 2, cfg.UseCache)

        If samples.Length = 0 Then
            Console.WriteLine("    [跳过] 训练集为空")
            skipped += 1
            Return
        End If

        Dim opt As New UNetOptions With {.InputWidth = size, .InputHeight = size, .InputChannels = 1,
                                         .NumClasses = 2, .Levels = 2, .BaseFilters = 4}
        Dim model As New UNetModel(opt)
        ' 前景（细胞核）像素占比只有几个百分点，过拟合小样本时按频率取倒数加权，
        ' 否则"全预测背景"就是损失意义上的局部最优，学不出任何前景
        Dim classWeights = SegmentationLoss.ClassWeightsFromData(samples, 2, foregroundExponent:=1.0)
        Dim trainer As New UNetTrainer(0.03, 2) With {.Augment = False, .GradClip = 0}

        Dim firstLoss As Double = 0
        Dim lastLoss As Double = 0
        Dim firstIoU As Double = 0
        Dim lastIoU As Double = 0

        For epoch As Integer = 0 To 49
            lastLoss = trainer.RunEpoch(model, samples, classWeights, epoch)

            If epoch = 0 Then firstLoss = lastLoss

            Dim m = UNetDemo.Evaluate(model, samples, classWeights)

            If epoch = 0 Then firstIoU = m.MeanIoU
            lastIoU = m.MeanIoU

            Dim ratio As Double = 0

            For Each sample In samples
                Dim block = model.CreateInput()
                Call sample.WriteTo(block)
                ratio += model.Segment(block).Count(Function(c) c > 0) / CDbl(sample.PixelCount)
            Next

            If epoch Mod 10 = 0 OrElse epoch = 19 Then
                Console.WriteLine($"      epoch {epoch}: loss={lastLoss:F5} mIoU={m.MeanIoU:F4} fgIoU={m.ForegroundIoU:F4} predFg={ratio / samples.Length:P2}")
            End If
        Next

        Console.WriteLine($"    loss: {firstLoss:F5} -> {lastLoss:F5}, mIoU: {firstIoU:F4} -> {lastIoU:F4}")

        Check("50 轮之后损失显著下降 (< 初始的 60%)", lastLoss < firstLoss * 0.6, $"{firstLoss:F5} -> {lastLoss:F5}")
        Check("平均 IoU 没有退化", lastIoU >= firstIoU - 0.05, $"{firstIoU:F4} -> {lastIoU:F4}")

        ' "全预测背景"是这类前景稀疏任务最容易掉进去的退化解，必须显式挡住
        Dim predictedRatio = 0.0
        Dim foregroundIou = 0.0

        For Each sample In samples
            Dim block = model.CreateInput()
            Call sample.WriteTo(block)

            Dim predicted = model.Segment(block)

            predictedRatio += predicted.Count(Function(c) c > 0) / CDbl(sample.PixelCount)
            foregroundIou += SegmentationLoss.Evaluate(predicted, sample.Labels, 2).ForegroundIoU
        Next

        predictedRatio /= samples.Length
        foregroundIou /= samples.Length

        Check("预测不是全背景的退化解", predictedRatio > 0.01, $"预测前景占比 {predictedRatio:P2}")
        Check("前景 IoU > 0", foregroundIou > 0.01, $"fgIoU = {foregroundIou:F4}")
    End Sub

    ' ---------------- 8) 混合精度 ----------------

    Private Sub TestMixedPrecision()
        Section("8) 混合精度: 结果一致 + 真的命中 fp32 路径")

        Dim savedMixed = MixedPrecisionPolicy.Enabled
        Dim savedRouter = PrecisionRouter.Enabled
        Dim savedThreshold = MixedPrecisionPolicy.ConvOpsThreshold

        Dim opt As New UNetOptions With {.InputWidth = 32, .InputHeight = 32, .InputChannels = 1,
                                         .NumClasses = 2, .Levels = 2, .BaseFilters = 8}
        Dim samples = SyntheticSamples.Generate(3, 32, seed:=7)
        Dim classWeights As Double() = Nothing

        ' --- 双精度基准 ---
        MixedPrecisionPolicy.Enabled = False
        PrecisionRouter.Enabled = False

        Dim modelDouble As New UNetModel(opt)
        Dim lossDouble = RunSteps(modelDouble, samples, classWeights, 3)

        ' --- 强制混合精度 ---
        MixedPrecisionPolicy.Enabled = True
        PrecisionRouter.Enabled = True
        MixedPrecisionPolicy.ConvOpsThreshold = 1 ' 让所有卷积都够格降精度

        Dim modelMixed As New UNetModel(opt)

        Call CopyWeights(modelDouble, modelMixed)
        Call MixedPrecisionDiagnostics.Reset()

        Dim watch = Diagnostics.Stopwatch.StartNew()
        Dim lossMixed = RunSteps(modelMixed, samples, classWeights, 3)
        watch.Stop()

        Dim conv2d = MixedPrecisionDiagnostics.Of("Conv2D")
        Dim backInput = MixedPrecisionDiagnostics.Of("Conv2DBackwardInput")
        Dim backFilter = MixedPrecisionDiagnostics.Of("Conv2DBackwardFilter")

        Console.WriteLine($"    fp64 loss = {lossDouble:F8}, 混合精度 loss = {lossMixed:F8}")
        Console.WriteLine("    " & MixedPrecisionDiagnostics.Report().Replace(vbLf, vbLf & "    "))

        Check("卷积前向真的走了 fp32 路径", conv2d.Accelerated > 0, $"加速 {conv2d.Accelerated} / 回退 {conv2d.Fallback}")
        Check("卷积反向(对输入)真的走了 fp32 路径", backInput.Accelerated > 0, $"加速 {backInput.Accelerated} / 回退 {backInput.Fallback}")
        Check("卷积反向(对卷积核)真的走了 fp32 路径", backFilter.Accelerated > 0, $"加速 {backFilter.Accelerated} / 回退 {backFilter.Fallback}")

        Dim diff = std.Abs(lossMixed - lossDouble)
        Dim scale = std.Max(1.0, std.Abs(lossDouble))

        Check("混合精度与纯双精度的损失一致 (相对 < 1e-3)", diff / scale < 0.001, $"相对差 {diff / scale:E3}")

        MixedPrecisionPolicy.Enabled = savedMixed
        PrecisionRouter.Enabled = savedRouter
        MixedPrecisionPolicy.ConvOpsThreshold = savedThreshold
    End Sub

    Private Function RunSteps(model As UNetModel, samples As SegmentationSample(),
                              classWeights As Double(), steps As Integer) As Double
        Dim trainer As New UNetTrainer(0.01, 1) With {.Augment = False, .GradClip = 0}
        Dim block = model.CreateInput()
        Dim last As Double = 0

        For i As Integer = 1 To steps
            Dim sample = samples((i - 1) Mod samples.Length)

            Call sample.WriteTo(block)
            last = trainer.TrainStep(model, block, sample.Labels, classWeights)
        Next

        Return last
    End Function

    Private Sub CopyWeights(source As UNetModel, target As UNetModel)
        Dim from = source.Parameters
        Dim into = target.Parameters

        If from.Length <> into.Length Then
            Throw New InvalidProgramException("两个模型的结构不一致, 无法拷贝权重")
        End If

        For i As Integer = 0 To from.Length - 1
            Array.Copy(from(i).Weights, into(i).Weights, from(i).Weights.Length)
            Call into(i).NotifyModified()
        Next
    End Sub

    ' ---------------- 9) 存档 ----------------

    Private Sub TestSaveLoad()
        Section("9) 模型存档往返")

        Dim opt As New UNetOptions With {.InputWidth = 16, .InputHeight = 16, .InputChannels = 1,
                                         .NumClasses = 2, .Levels = 2, .BaseFilters = 2}
        Dim model As New UNetModel(opt)
        Dim samples = SyntheticSamples.Generate(2, 16, seed:=31)
        Dim trainer As New UNetTrainer(0.01, 1) With {.Augment = False}
        Dim block = model.CreateInput()

        For Each sample In samples
            Call sample.WriteTo(block)
            Call trainer.TrainStep(model, block, sample.Labels, Nothing)
        Next

        Dim path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "unet_selftest_model.bin")

        Call model.Save(path)

        Dim reloaded = UNetModel.Load(path)
        Dim from = model.Parameters
        Dim into = reloaded.Parameters

        Dim same As Boolean = from.Length = into.Length

        If same Then
            For i As Integer = 0 To from.Length - 1
                For j As Integer = 0 To from(i).Weights.Length - 1
                    If from(i).Weights(j) <> into(i).Weights(j) Then
                        same = False
                        Exit For
                    End If
                Next
                If Not same Then Exit For
            Next
        End If

        Check("存档往返后参数逐位相同", same)

        Dim masksSame As Boolean = True

        For Each sample In samples
            Dim a = model.Segment(sample.WriteTo(model.CreateInput()))
            Dim b = reloaded.Segment(sample.WriteTo(reloaded.CreateInput()))

            For i As Integer = 0 To a.Length - 1
                If a(i) <> b(i) Then
                    masksSame = False
                    Exit For
                End If
            Next
        Next

        Check("存档往返后预测结果相同", masksSame)

        Try
            System.IO.File.Delete(path)
        Catch
        End Try
    End Sub

    ' ---------------- 10) GPU 对拍 ----------------

    Private Sub TestGpuParity()
        Section("10) GPU(CUDA) 与 CPU 对拍")

        Dim opt As New UNetOptions With {.InputWidth = 32, .InputHeight = 32, .InputChannels = 1,
                                         .NumClasses = 2, .Levels = 2, .BaseFilters = 8}
        Dim samples = SyntheticSamples.Generate(3, 32, seed:=7)
        Dim classWeights As Double() = Nothing

        ' CPU 基准
        MixedPrecisionPolicy.Enabled = False
        PrecisionRouter.Enabled = False

        Dim cpuModel As New UNetModel(opt)
        Dim cpuBackend = Tensor.computeKernel.Name ' 必须在注册 GPU 之前取, 否则两行都会显示 CUDA
        Dim cpuWatch = Diagnostics.Stopwatch.StartNew()
        Dim cpuLoss = RunSteps(cpuModel, samples, classWeights, 3)
        cpuWatch.Stop()

        MixedPrecisionPolicy.Enabled = True
        PrecisionRouter.Enabled = True
        MixedPrecisionPolicy.ConvOpsThreshold = 1

        Dim gpuReady = TryEnableGpu()

        If Not gpuReady Then
            Console.WriteLine("    [跳过] CUDA 不可用, 未做对拍")
            skipped += 1
            Return
        End If

        Dim gpuModel As New UNetModel(opt)

        Call CopyWeights(cpuModel, gpuModel)

        Dim gpuBackend = Tensor.computeKernel.Name
        Dim gpuWatch = Diagnostics.Stopwatch.StartNew()
        Dim gpuLoss = RunSteps(gpuModel, samples, classWeights, 3)
        gpuWatch.Stop()

        Dim diff = std.Abs(cpuLoss - gpuLoss)
        Dim scale = std.Max(1.0, std.Abs(cpuLoss))

        Console.WriteLine($"    CPU({cpuBackend}) loss = {cpuLoss:F8}, 耗时 {cpuWatch.Elapsed.TotalSeconds:F3}s")
        Console.WriteLine($"    GPU({gpuBackend}) loss = {gpuLoss:F8}, 耗时 {gpuWatch.Elapsed.TotalSeconds:F3}s")

        Check("GPU 与 CPU 的 loss 一致 (相对 < 5e-3)", diff / scale < 0.005, $"相对差 {diff / scale:E3}")

        ' 还原到 CPU 后端，避免影响后续断言
        Call gpu.CudaTensor.Unregister()
    End Sub

End Module
