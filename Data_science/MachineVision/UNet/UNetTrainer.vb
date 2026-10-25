' ---------------------------------------------------------------------------
' U-Net 训练器
'
' 为什么不用 CNN 的 TrainerAlgorithm：
'   它把"前向—反向"硬编码在 ConvolutionalNN 的串行链上（``net.forward`` /
'   ``net.backward``），而 U-Net 是带跳跃连接的有向图，走不了那条链。
'   这里只复用它的**参数块抽象** BackPropResult —— 特别是其中的
'   ``NotifyModified()``：训练器是绕过 Tensor 索引器直接就地改写权重数组的，
'   CUDA 后端会把数组缓存到显存，改完必须显式声明缓存失效，
'   否则下一轮前向仍然用显存里的旧权重，表现为"loss 不动"这种极难排查的静默错误。
'
' 精度：主权重、一阶/二阶矩、更新全部在双精度下完成。
' ---------------------------------------------------------------------------

Imports Microsoft.VisualBasic.MachineLearning.CNN.data
Imports std = System.Math

Namespace UNet

    ''' <summary>
    ''' 小批量 Adam / SGD(momentum) 训练器。
    ''' </summary>
    Public Class UNetTrainer

        ''' <summary>基础学习率。</summary>
        Public Property BaseLearningRate As Double = 0.001
        ''' <summary>每多少个样本更新一次参数。</summary>
        Public Property BatchSize As Integer = 4
        ''' <summary>Adam 的一阶矩衰减率。</summary>
        Public Property Beta1 As Double = 0.9
        ''' <summary>Adam 的二阶矩衰减率。</summary>
        Public Property Beta2 As Double = 0.999
        ''' <summary>数值稳定项。</summary>
        Public Property Epsilon As Double = 0.00000001
        ''' <summary>解耦权重衰减；0 表示纯 Adam。</summary>
        Public Property WeightDecay As Double = 0.0
        ''' <summary>梯度全局范数上限；小于等于 0 表示不裁剪。</summary>
        Public Property GradClip As Double = 5.0
        ''' <summary>每轮学习率衰减系数：<c>lr = base / (1 + decay * epoch)</c>；0 表示不衰减。</summary>
        Public Property LrDecay As Double = 0.0
        ''' <summary>是否改用 SGD + momentum（默认 Adam）。</summary>
        Public Property UseSgdMomentum As Boolean = False
        ''' <summary>SGD 的动量系数。</summary>
        Public Property Momentum As Double = 0.9
        ''' <summary>训练时是否做随机翻转/旋转增广。</summary>
        Public Property Augment As Boolean = True
        ''' <summary>增广用的随机种子；固定种子可复现。</summary>
        Public Property Seed As Integer = 1234

        Private firstMoments As New List(Of Double())
        Private secondMoments As New List(Of Double())
        Private velocities As New List(Of Double())
        Private stepCount As Integer = 0
        Private updateCount As Integer = 0
        Private currentLearningRate As Double
        Private rng As Random

        ''' <summary>当前实际生效的学习率（已应用衰减）。</summary>
        Public ReadOnly Property LearningRate As Double
            Get
                Return currentLearningRate
            End Get
        End Property

        ''' <summary>已累计的训练步数（样本数）。</summary>
        Public ReadOnly Property Steps As Integer
            Get
                Return stepCount
            End Get
        End Property

        ''' <summary>已发生的参数更新次数。</summary>
        Public ReadOnly Property Updates As Integer
            Get
                Return updateCount
            End Get
        End Property

        ''' <summary>
        ''' 用给定的超参数创建训练器。
        ''' </summary>
        ''' <param name="learningRate">基础学习率。</param>
        ''' <param name="batchSize">小批量大小。</param>
        Public Sub New(Optional learningRate As Double = 0.001, Optional batchSize As Integer = 4)
            BaseLearningRate = learningRate
            BatchSize = If(batchSize < 1, 1, batchSize)
            currentLearningRate = BaseLearningRate
            rng = New Random(Seed)
        End Sub

        ''' <summary>
        ''' 重置优化器状态与计数器（学习率也回到基础值）。
        ''' </summary>
        Public Sub Reset()
            firstMoments.Clear()
            secondMoments.Clear()
            velocities.Clear()
            stepCount = 0
            updateCount = 0
            currentLearningRate = BaseLearningRate
            rng = New Random(Seed)
        End Sub

        ''' <summary>
        ''' 用一个样本训练一步（前向 + 反向 + 参数梯度累加）。
        ''' </summary>
        ''' <param name="model">待训练的模型。</param>
        ''' <param name="x">输入数据块。</param>
        ''' <param name="labels">每像素类别标签。</param>
        ''' <param name="classWeights">逐类损失权重，Nothing 表示等权。</param>
        ''' <returns>该样本的损失。</returns>
        Public Function TrainStep(model As UNetModel, x As DataBlock,
                                  labels As Integer(), classWeights As Double()) As Double
            Dim logits = model.Forward(x, training:=True)
            Dim grad As Double() = Nothing
            Dim loss = SegmentationLoss.CrossEntropy(logits, labels, classWeights, grad)

            Call model.Backward(grad)

            stepCount += 1

            If stepCount Mod BatchSize = 0 Then
                Call UpdateParameters(model)
            End If

            Return loss
        End Function

        ''' <summary>
        ''' 跑一个 epoch：打乱顺序（可选增广）后逐个样本训练。
        ''' </summary>
        ''' <param name="model">模型。</param>
        ''' <param name="samples">训练集。</param>
        ''' <param name="classWeights">逐类损失权重。</param>
        ''' <param name="epochIndex">第几轮（0 起），用于学习率衰减。</param>
        ''' <returns>本轮的平均损失。</returns>
        Public Function RunEpoch(model As UNetModel, samples As SegmentationSample(),
                                 classWeights As Double(), epochIndex As Integer) As Double
            currentLearningRate = BaseLearningRate / (1.0 + LrDecay * epochIndex)

            Dim order(samples.Length - 1) As Integer

            For i As Integer = 0 To order.Length - 1
                order(i) = i
            Next

            ' Fisher-Yates 洗牌
            For i As Integer = order.Length - 1 To 1 Step -1
                Dim j = rng.Next(i + 1)
                Dim t = order(i)
                order(i) = order(j)
                order(j) = t
            Next

            Dim block = model.CreateInput()
            Dim total As Double = 0

            For Each i In order
                Dim sample = samples(i)

                If Augment Then
                    sample = sample.Augment(rng)
                End If

                Call sample.WriteTo(block)
                total += TrainStep(model, block, sample.Labels, classWeights)
            Next

            ' 末尾不足一个 batch 的残余梯度也要应用掉
            If stepCount Mod BatchSize <> 0 Then
                Call UpdateParameters(model)
            End If

            Return total / samples.Length
        End Function

        ' ---------------- 参数更新 ----------------

        Private Sub UpdateParameters(model As UNetModel)
            Dim blocks = model.Parameters

            If firstMoments.Count = 0 Then
                For Each block In blocks
                    firstMoments.Add(New Double(block.Weights.Length - 1) {})
                    secondMoments.Add(New Double(block.Weights.Length - 1) {})
                    velocities.Add(New Double(block.Weights.Length - 1) {})
                Next
            End If

            If GradClip > 0 Then
                Call ClipGradients(blocks)
            End If

            updateCount += 1

            Dim lr = currentLearningRate
            Dim bc1 = 1.0 - std.Pow(Beta1, updateCount)
            Dim bc2 = 1.0 - std.Pow(Beta2, updateCount)

            If bc1 <= 0 Then bc1 = 1.0
            If bc2 <= 0 Then bc2 = 1.0

            For i As Integer = 0 To blocks.Length - 1
                Dim weights = blocks(i).Weights
                Dim grads = blocks(i).Gradients
                Dim m = firstMoments(i)
                Dim v = secondMoments(i)
                Dim vel = velocities(i)
                Dim n = weights.Length

                For j As Integer = 0 To n - 1
                    Dim g = grads(j) / BatchSize

                    If Not Double.IsFinite(g) Then g = 0.0

                    If UseSgdMomentum Then
                        vel(j) = Momentum * vel(j) - lr * g
                        weights(j) += vel(j)
                    Else
                        m(j) = Beta1 * m(j) + (1.0 - Beta1) * g
                        v(j) = Beta2 * v(j) + (1.0 - Beta2) * g * g

                        Dim mHat = m(j) / bc1
                        Dim vHat = v(j) / bc2

                        weights(j) += -lr * mHat / (std.Sqrt(If(vHat < 0, 0.0, vHat)) + Epsilon)

                        If WeightDecay <> 0.0 Then
                            weights(j) -= lr * WeightDecay * weights(j)
                        End If
                    End If

                    ' 清空梯度累加器，准备下一个小批量
                    grads(j) = 0.0
                Next

                ' 权重与梯度都是绕过 Tensor 索引器就地改写的，必须让宿主块声明缓存失效
                Call blocks(i).NotifyModified()
            Next
        End Sub

        ''' <summary>按全局范数裁剪全部参数梯度。</summary>
        Private Sub ClipGradients(blocks As BackPropResult())
            Dim sumSq As Double = 0

            For Each block In blocks
                For Each g In block.Gradients
                    If Double.IsFinite(g) Then
                        sumSq += g * g
                    End If
                Next
            Next

            Dim norm = std.Sqrt(sumSq)

            If norm <= GradClip OrElse norm = 0 Then Return

            Dim scale = GradClip / norm

            For Each block In blocks
                Dim grads = block.Gradients

                For j As Integer = 0 To grads.Length - 1
                    If Double.IsFinite(grads(j)) Then
                        grads(j) *= scale
                    Else
                        grads(j) = 0.0
                    End If
                Next

                Call block.NotifyModified()
            Next
        End Sub

        ''' <summary>单行超参数描述。</summary>
        Public Overrides Function ToString() As String
            Dim name = If(UseSgdMomentum, $"sgd(momentum:{Momentum})", "adam")

            Return $"{name}(lr:{currentLearningRate:G4}, batch:{BatchSize}, decay:{LrDecay}, clip:{GradClip})"
        End Function

    End Class

End Namespace
