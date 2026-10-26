Imports System.IO
Imports Microsoft.VisualBasic.Imaging.BitmapImage.Png
Imports Microsoft.VisualBasic.MachineVision.UNet.UNet

''' <summary>
''' 真实数据上的完整训练 + 评估 + 预测可视化流程。
''' </summary>
Public Module UNetDemo

    ''' <summary>
    ''' 跑一遍完整 demo。
    ''' </summary>
    ''' <param name="cfg">运行配置。</param>
    ''' <param name="useGpu">是否先注册 CUDA 后端。</param>
    ''' <returns>0 表示成功。</returns>
    Public Function Run(cfg As RunConfig, useGpu As Boolean) As Integer
        Console.WriteLine("=== U-Net 混合精度图像分割 demo (DSB2018 stage1) ===")
        Console.WriteLine($"    {cfg}")
        Console.WriteLine()

        Call cfg.EnsureOutputDir()

        If useGpu Then
            Console.WriteLine(">> 0) 注册 CUDA 后端")
            Call TryEnableGpu()
            Console.WriteLine()
        End If

        Call PrintEnvironment(cfg)

        ' ---------------- 1) 数据 ----------------
        Console.WriteLine()
        Console.WriteLine(">> 1) 加载数据集")

        Dim watch = Stopwatch.StartNew()
        Dim train = DsbDataset.LoadTrain(cfg.DataRoot, cfg.ImageSize, cfg.Limit, cfg.UseCache)
        Dim trainLoad = watch.Elapsed.TotalSeconds

        watch.Restart()
        Dim test = DsbDataset.LoadTest(cfg.DataRoot, cfg.ImageSize, cfg.TestLimit, cfg.RleColumnMajor, cfg.UseCache)
        Dim testLoad = watch.Elapsed.TotalSeconds

        If train.Length = 0 OrElse test.Length = 0 Then
            Console.WriteLine("    [失败] 训练集或测试集为空")
            Return 1
        End If

        Console.WriteLine($"    训练集 {train.Length} 张 (耗时 {trainLoad:F1}s), 测试集 {test.Length} 张 (耗时 {testLoad:F1}s)")
        Console.WriteLine($"    训练样本示例: {train(0)}, 测试样本示例: {test(0)}")
        Console.WriteLine($"    训练前景占比 = {ForegroundRatio(train):P2}, 测试前景占比 = {ForegroundRatio(test):P2}")

        ' ---------------- 2) 模型 ----------------
        Console.WriteLine()
        Console.WriteLine(">> 2) 构建 U-Net")

        Dim opt As New UNetOptions With {
            .InputWidth = cfg.ImageSize,
            .InputHeight = cfg.ImageSize,
            .InputChannels = 1,
            .NumClasses = 2,
            .Levels = cfg.Levels,
            .BaseFilters = cfg.BaseFilters
        }

        Dim model As New UNetModel(opt)

        Console.WriteLine(model.Describe())

        ' ---------------- 3) 训练 ----------------
        Console.WriteLine()
        Console.WriteLine(">> 3) 训练")

        ' 细胞核像素占比只有几个百分点，按类别频率取倒数加权，
        ' 否则"全预测背景"在损失上就已经是局部最优，模型根本不会去学前景观
        Dim classWeights = SegmentationLoss.ClassWeightsFromData(train, opt.NumClasses, foregroundExponent:=1.0)
        Console.WriteLine($"    类别权重 = [{classWeights(0):F3}, {classWeights(1):F3}]")

        Dim trainer As New UNetTrainer(cfg.LearningRate, cfg.BatchSize) With {
            .LrDecay = cfg.LrDecay,
            .Augment = True,
            .GradClip = 5.0
        }

        For epoch As Integer = 0 To cfg.Epochs - 1
            watch.Restart()

            Dim loss = trainer.RunEpoch(model, train, classWeights, epoch)
            Dim evalWatch = Stopwatch.StartNew()
            Dim metrics = Evaluate(model, test, classWeights)

            Console.WriteLine($"    epoch {epoch + 1}/{cfg.Epochs}  loss={loss:F5}  {metrics}  " &
                              $"train={watch.Elapsed.TotalSeconds:F1}s eval={evalWatch.Elapsed.TotalSeconds:F1}s")
        Next

        ' ---------------- 4) 预测与可视化 ----------------
        Console.WriteLine()
        Console.WriteLine(">> 4) 在 stage1_test 上预测并落盘")

        Dim preview = System.Math.Min(cfg.PreviewCount, test.Length)

        For i As Integer = 0 To preview - 1
            Dim sample = test(i)
            Dim block = model.CreateInput()

            Call sample.WriteTo(block)

            Dim predicted = model.Segment(block)

            Call PngWriter.WriteGray(Path.Combine(cfg.OutputDir, $"{sample.Id}_image.png"), sample.Width, sample.Height, sample.Pixels)
            Call PngWriter.WriteGray(Path.Combine(cfg.OutputDir, $"{sample.Id}_gt.png"), sample.Width, sample.Height, ToDouble(sample.Labels))
            Call PngWriter.WriteGray(Path.Combine(cfg.OutputDir, $"{sample.Id}_pred.png"), sample.Width, sample.Height, ToDouble(predicted))

            Console.WriteLine($"    {sample.Id}: 已写出 image / gt / pred")
            Call PrintPreview("      真值", sample.Labels, sample.Width, sample.Height)
            Call PrintPreview("      预测", predicted, sample.Width, sample.Height)
        Next

        ' ---------------- 5) 存档 ----------------
        Console.WriteLine()
        Console.WriteLine(">> 5) 保存模型")

        Dim modelPath = Path.Combine(cfg.OutputDir, "unet_model.bin")

        Call model.Save(modelPath)

        Console.WriteLine($"    模型已写出: {modelPath} ({New FileInfo(modelPath).Length / 1024.0:F1} KB)")

        Dim reloaded = UNetModel.Load(modelPath)
        Dim check = Evaluate(reloaded, test, classWeights)

        Console.WriteLine($"    重新加载后评估: {check}")

        Console.WriteLine()
        Console.WriteLine("=== demo 完成 ===")

        Return 0
    End Function

    ''' <summary>在测试集上评估（逐样本平均）。</summary>
    Public Function Evaluate(model As UNetModel, samples As SegmentationSample(), classWeights As Double()) As SegmentationMetrics
        Dim block = model.CreateInput()
        Dim acc As Double = 0, iou As Double = 0, dice As Double = 0, fgIoU As Double = 0
        Dim bgIoU As Double = 0, predFg As Double = 0, gtFg As Double = 0

        For Each sample In samples
            Call sample.WriteTo(block)

            Dim predicted = model.Segment(block)
            Dim m = SegmentationLoss.Evaluate(predicted, sample.Labels, 2)

            acc += m.PixelAccuracy
            iou += m.MeanIoU
            dice += m.Dice
            fgIoU += m.ForegroundIoU
            bgIoU += m.BackgroundIoU
            predFg += m.PredictedForegroundRatio
            gtFg += m.TargetForegroundRatio
        Next

        Dim n = samples.Length

        Return New SegmentationMetrics With {
            .PixelAccuracy = acc / n,
            .MeanIoU = iou / n,
            .Dice = dice / n,
            .ForegroundIoU = fgIoU / n,
            .BackgroundIoU = bgIoU / n,
            .PredictedForegroundRatio = predFg / n,
            .TargetForegroundRatio = gtFg / n
        }
    End Function

    Private Function ForegroundRatio(samples As SegmentationSample()) As Double
        Dim fg As Long = 0
        Dim total As Long = 0

        For Each sample In samples
            For Each label In sample.Labels
                If label > 0 Then fg += 1
                total += 1
            Next
        Next

        Return If(total = 0, 0.0, fg / CDbl(total))
    End Function

    Private Function ToDouble(labels As Integer()) As Double()
        Dim values(labels.Length - 1) As Double

        For i As Integer = 0 To labels.Length - 1
            values(i) = labels(i)
        Next

        Return values
    End Function

    ''' <summary>把掩码降采样成 ASCII 图，便于在控制台直接看效果。</summary>
    Private Sub PrintPreview(title As String, labels As Integer(), width As Integer, height As Integer)
        Dim stepSize = System.Math.Max(1, width \ 32)
        Dim lines As New Text.StringBuilder()

        For y As Integer = 0 To height - 1 Step stepSize * 2
            Dim sb As New Text.StringBuilder()

            For x As Integer = 0 To width - 1 Step stepSize
                sb.Append(If(labels(y * width + x) > 0, "#"c, "."c))
            Next

            Call lines.AppendLine(sb.ToString())
        Next

        Console.WriteLine($"{title}:")
        Console.WriteLine(lines.ToString())
    End Sub

End Module
