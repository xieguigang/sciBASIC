Imports System
Imports Microsoft.VisualBasic.MachineLearning.CNN
Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports Microsoft.VisualBasic.MachineLearning.TensorFlow.Compute
Imports UNet

''' <summary>
''' 临时诊断：打印训练过程中的 logits / 梯度统计，用于定位"模型学不动"这类问题。
''' </summary>
Public Module Probe

    Public Function Run(useReal As Boolean) As Integer
        Dim size = 32
        Dim samples As SegmentationSample()

        If useReal Then
            samples = DsbDataset.LoadTrain("C:\Users\Administrator\Downloads\unet-dataset", size, 4, True)
        Else
            samples = SyntheticSamples.Generate(4, size, seed:=5)
        End If

        MixedPrecisionPolicy.Enabled = False
        PrecisionRouter.Enabled = False

        Dim opt As New UNetOptions With {.InputWidth = size, .InputHeight = size, .InputChannels = 1,
                                         .NumClasses = 2, .Levels = 2, .BaseFilters = 8}
        Dim model As New UNetModel(opt)
        Dim classWeights As Double() = Nothing
        Dim trainer As New UNetTrainer(0.02, 2) With {.Augment = False, .GradClip = 0}
        Dim block = model.CreateInput()

        Console.WriteLine($"样本 {samples.Length} 张, 前景占比 = {ForegroundRatio(samples):P2}")

        For epoch As Integer = 0 To 59
            Dim loss = trainer.RunEpoch(model, samples, classWeights, epoch)

            Call samples(0).WriteTo(block)

            Dim logits = model.Forward(block, training:=False)
            Dim values = logits.Weights
            Dim min0 As Double = Double.MaxValue, max0 As Double = Double.MinValue
            Dim min1 As Double = Double.MaxValue, max1 As Double = Double.MinValue
            Dim sum0 As Double = 0, sum1 As Double = 0

            For p As Integer = 0 To values.Length \ 2 - 1
                min0 = System.Math.Min(min0, values(p * 2))
                max0 = System.Math.Max(max0, values(p * 2))
                min1 = System.Math.Min(min1, values(p * 2 + 1))
                max1 = System.Math.Max(max1, values(p * 2 + 1))
                sum0 += values(p * 2)
                sum1 += values(p * 2 + 1)
            Next

            Dim gradNorm As Double = 0

            For Each b In model.Parameters
                For Each g In b.Gradients
                    gradNorm += g * g
                Next
            Next

            Dim predicted = model.Segment(block)
            Dim predFg = predicted.Count(Function(c) c > 0) / CDbl(predicted.Length)

            Console.WriteLine($"epoch {epoch}: loss={loss:F6} " &
                              $"logit0[{min0:F4},{max0:F4}] mean={sum0 / (values.Length \ 2):F4} " &
                              $"logit1[{min1:F4},{max1:F4}] mean={sum1 / (values.Length \ 2):F4} " &
                              $"|g|={System.Math.Sqrt(gradNorm):E3} predFg={predFg:P2}")
        Next

        Return 0
    End Function

    Private Function ForegroundRatio(samples As SegmentationSample()) As Double
        Dim fg As Long = 0, total As Long = 0

        For Each s In samples
            For Each l In s.Labels
                If l > 0 Then fg += 1
                total += 1
            Next
        Next

        Return If(total = 0, 0.0, fg / CDbl(total))
    End Function

End Module
