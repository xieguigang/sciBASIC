' ---------------------------------------------------------------------------
' 分割损失与评估指标
'
' 损失：逐像素的 softmax 交叉熵（可选类别权重），全部在双精度下计算。
'   之所以不走单精度：softmax 的 exp 与归一化对数值范围敏感，
'   且损失的量级直接决定梯度尺度 —— 这里属于 MixedPrecisionPolicy 的
'   "数值敏感类"，规模再大也不降精度。
'
' 指标：像素精度 / 逐类 IoU / 平均 IoU / 前景 Dice。
'   前景（细胞核）像素占比通常只有几个百分点，只报总体像素精度会得到
'   "全预测背景也有 95% 准确率"的虚高数字，因此必须分开报前景 IoU。
' ---------------------------------------------------------------------------

Imports Microsoft.VisualBasic.MachineLearning.CNN.data
Imports std = System.Math

Namespace UNet

    ''' <summary>
    ''' 一次评估的结果。
    ''' </summary>
    Public Class SegmentationMetrics

        ''' <summary>总体像素精度。</summary>
        Public Property PixelAccuracy As Double
        ''' <summary>前景（类别 1）的 IoU。</summary>
        Public Property ForegroundIoU As Double
        ''' <summary>背景（类别 0）的 IoU。</summary>
        Public Property BackgroundIoU As Double
        ''' <summary>所有类别 IoU 的平均。</summary>
        Public Property MeanIoU As Double
        ''' <summary>前景 Dice 系数。</summary>
        Public Property Dice As Double
        ''' <summary>预测为前景的像素占比（用于发现"什么都不预测"的退化）。</summary>
        Public Property PredictedForegroundRatio As Double
        ''' <summary>真值中前景像素的占比。</summary>
        Public Property TargetForegroundRatio As Double

        ''' <summary>单行结果。</summary>
        Public Overrides Function ToString() As String
            Return $"pixel_acc={PixelAccuracy:F4} mIoU={MeanIoU:F4} fgIoU={ForegroundIoU:F4} " &
                   $"bgIoU={BackgroundIoU:F4} dice={Dice:F4} predFg={PredictedForegroundRatio:F3}/gtFg={TargetForegroundRatio:F3}"
        End Function

    End Class

    ''' <summary>
    ''' 逐像素 softmax 交叉熵及其梯度。
    ''' </summary>
    Public Module SegmentationLoss

        ''' <summary>
        ''' 计算损失与对 logits 的梯度。
        ''' </summary>
        ''' <param name="logits">模型输出块，形状 (W, H, K)。</param>
        ''' <param name="labels">每像素类别下标，长度 W*H。</param>
        ''' <param name="classWeights">逐类权重；Nothing 表示全部取 1。</param>
        ''' <param name="grad">输出：对 logits 的梯度，长度 W*H*K（已按像素数取平均）。</param>
        ''' <returns>按像素取平均的损失。</returns>
        ''' <remarks>
        ''' 单像素的梯度为 <c>w_t * (p - onehot(t))</c>，其中 t 是该像素的真值类别；
        ''' 整张图再除以像素数，使损失与梯度都不随分辨率变化。
        ''' </remarks>
        Public Function CrossEntropy(logits As DataBlock, labels As Integer(),
                                     classWeights As Double(),
                                     ByRef grad As Double()) As Double
            Dim pixels = logits.SX * logits.SY
            Dim classes = logits.Depth
            Dim z = logits.Weights
            Dim total As Double = 0

            ReDim grad(z.Length - 1)

            For p As Integer = 0 To pixels - 1
                Dim baseIndex = p * classes

                ' 数值稳定：先减最大值再取 exp
                Dim maxValue = z(baseIndex)

                For c As Integer = 1 To classes - 1
                    If z(baseIndex + c) > maxValue Then
                        maxValue = z(baseIndex + c)
                    End If
                Next

                Dim sum As Double = 0

                For c As Integer = 0 To classes - 1
                    Dim e = std.Exp(z(baseIndex + c) - maxValue)
                    grad(baseIndex + c) = e
                    sum += e
                Next

                Dim target = labels(p)

                If target < 0 Then target = 0
                If target >= classes Then target = classes - 1

                Dim weight = If(classWeights Is Nothing, 1.0, classWeights(target))
                Dim pTarget = grad(baseIndex + target) / sum

                total += -weight * std.Log(std.Max(pTarget, 1.0E-12))

                Dim inv = 1.0 / sum

                For c As Integer = 0 To classes - 1
                    Dim pc = grad(baseIndex + c) * inv
                    Dim indicator = If(c = target, 1.0, 0.0)

                    grad(baseIndex + c) = weight * (pc - indicator) / pixels
                Next
            Next

            Return total / pixels
        End Function

        ''' <summary>
        ''' 按训练集的前景占比给出类别权重：背景取 1，前景取 <c>ratio</c> 的倒数（开方缓和一点）。
        ''' </summary>
        ''' <param name="samples">训练集。</param>
        ''' <param name="numClasses">类别数。</param>
        ''' <param name="foregroundExponent">
        ''' 前景权重的指数；1 表示严格取倒数（等价于按类别频率加权），0.5 表示缓和版本。
        ''' </param>
        ''' <returns>长度 <paramref name="numClasses"/> 的权重数组。</returns>
        Public Function ClassWeightsFromData(samples As SegmentationSample(),
                                             numClasses As Integer,
                                             Optional foregroundExponent As Double = 0.5) As Double()
            Dim counts(numClasses - 1) As Long

            For Each sample In samples
                For Each label In sample.Labels
                    If label >= 0 AndAlso label < numClasses Then
                        counts(label) += 1
                    End If
                Next
            Next

            Dim total As Long = 0

            For Each c In counts
                total += c
            Next

            Dim weights(numClasses - 1) As Double

            If total = 0 Then
                For i As Integer = 0 To numClasses - 1
                    weights(i) = 1.0
                Next

                Return weights
            End If

            For i As Integer = 0 To numClasses - 1
                If counts(i) = 0 Then
                    weights(i) = 1.0
                Else
                    Dim freq = counts(i) / CDbl(total)
                    weights(i) = std.Pow(1.0 / freq, foregroundExponent)
                End If
            Next

            Return weights
        End Function

        ''' <summary>
        ''' 计算预测与真值的分割指标。
        ''' </summary>
        ''' <param name="predicted">预测类别，长度 W*H。</param>
        ''' <param name="target">真值类别，长度 W*H。</param>
        ''' <param name="numClasses">类别数。</param>
        Public Function Evaluate(predicted As Integer(), target As Integer(), numClasses As Integer) As SegmentationMetrics
            Dim inter(numClasses - 1) As Long
            Dim union(numClasses - 1) As Long
            Dim predictedCount(numClasses - 1) As Long
            Dim targetCount(numClasses - 1) As Long
            Dim correct As Long = 0
            Dim n As Long = 0

            For i As Integer = 0 To predicted.Length - 1
                Dim p = predicted(i)
                Dim t = target(i)

                If p < 0 Then p = 0
                If p >= numClasses Then p = numClasses - 1
                If t < 0 Then t = 0
                If t >= numClasses Then t = numClasses - 1

                predictedCount(p) += 1
                targetCount(t) += 1

                If p = t Then
                    correct += 1
                    inter(t) += 1
                End If

                union(t) += 1
                union(p) += 1
                n += 1
            Next

            Dim ious(numClasses - 1) As Double

            For c As Integer = 0 To numClasses - 1
                ' union 里同一个像素被计数两次（一次按真值、一次按预测），
                ' 而交集只算一次，因此 IoU = inter / (union - inter) 才是正确的 Jaccard
                Dim u = union(c) - inter(c)

                ious(c) = If(u = 0, 0.0, inter(c) / CDbl(u))
            Next

            Dim mean As Double = 0

            For c As Integer = 0 To numClasses - 1
                mean += ious(c)
            Next

            mean /= numClasses

            Dim fgIoU = If(numClasses > 1, ious(1), ious(0))
            Dim dice = If(predictedCount(1) + targetCount(1) = 0, 0.0,
                          2.0 * inter(1) / CDbl(predictedCount(1) + targetCount(1)))

            Return New SegmentationMetrics With {
                .PixelAccuracy = If(n = 0, 0.0, correct / CDbl(n)),
                .BackgroundIoU = ious(0),
                .ForegroundIoU = fgIoU,
                .MeanIoU = mean,
                .Dice = If(numClasses > 1, dice, 0.0),
                .PredictedForegroundRatio = If(n = 0, 0.0,
                    If(numClasses > 1, predictedCount(1) / CDbl(n), 0.0)),
                .TargetForegroundRatio = If(n = 0, 0.0,
                    If(numClasses > 1, targetCount(1) / CDbl(n), 0.0))
            }
        End Function

    End Module

End Namespace
