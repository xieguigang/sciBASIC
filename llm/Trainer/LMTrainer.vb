' ---------------------------------------------------------------------------
' LMTrainer —— 语言模型的训练循环
'
' 一个训练步的完整顺序（顺序错了会出各种"训练不收敛"的怪现象，因此不允许调整）：
'
'   1. 计算本步学习率（warmup + cosine）
'   2. 清零全部参数的梯度累加器
'   3. 前向：整段序列并行 → logits [N, Vocab]
'   4. 带掩码的交叉熵 → loss 与 d(logits) = softmax − onehot
'   5. 反向：手写 BPTT，梯度就地累加到各参数的梯度累加器
'   6. 全局梯度范数裁剪          ← 必须在更新之前
'   7. AdamW 更新参数（更新后自动清零梯度）
'   8. MoE 负载均衡偏置更新       ← 不属于梯度更新，只根据本步的负载统计调整选择偏置
'
' 损失掩码是这个训练器存在的主要理由：SFT 阶段只对 assistant 自己产出的 token 计损失，
' 用户消息与工具返回结果都不计 —— 模型要学的是"该怎么回应"，而不是"复述用户说了什么"。
' ---------------------------------------------------------------------------

Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports Diagnostics = System.Diagnostics
Imports std = System.Math

Namespace Trainer

    ''' <summary>
    ''' 语言模型训练器：把"前向 → 掩码交叉熵 → 反向 → 裁剪 → AdamW → MoE 均衡"串成一个训练步。
    ''' </summary>
    Public Class LMTrainer

        Private ReadOnly _model As LLMModel
        Private ReadOnly _history As New List(Of TrainingStepReport)
        Private _step As Integer
        Private _skippedSteps As Integer

        ''' <summary>因为梯度失控而被跳过参数更新的步数。</summary>
        ''' <remarks>
        ''' 显式暴露出来而不是静默处理：它是"训练过程真的遇到了数值问题"的证据，
        ''' 报告中必须能看到，否则会变成被掩盖的失败。
        ''' </remarks>
        Public ReadOnly Property SkippedSteps As Integer
            Get
                Return _skippedSteps
            End Get
        End Property

        ''' <summary>被打分的模型。</summary>
        Public ReadOnly Property Model As LLMModel
            Get
                Return _model
            End Get
        End Property

        ''' <summary>训练超参。</summary>
        Public Property Config As TrainingConfig

        ''' <summary>已经完成的训练步数。</summary>
        Public ReadOnly Property [Step] As Integer
            Get
                Return _step
            End Get
        End Property

        ''' <summary>逐步的损失/困惑度记录。</summary>
        Public ReadOnly Property History As IList(Of TrainingStepReport)
            Get
                Return _history
            End Get
        End Property

        ''' <summary>
        ''' Creates a language model trainer.
        ''' </summary>
        ''' <param name="model">The model whose parameters are trained.</param>
        ''' <param name="config">Optional training configuration; default values are used when omitted.</param>
        Public Sub New(model As LLMModel, Optional config As TrainingConfig = Nothing)
            If model Is Nothing Then Throw New ArgumentNullException(NameOf(model))

            _model = model
            Me.Config = If(config, New TrainingConfig())
        End Sub

        ''' <summary>
        ''' 学习率调度：线性 warmup + cosine 衰减。
        ''' </summary>
        ''' <remarks>
        ''' warmup 的意义是让 AdamW 的二阶矩估计先积累足够的样本再放大步长；
        ''' 直接用峰值学习率起步在小模型上非常容易一步发散。
        ''' </remarks>
        Public Function LearningRateAt([step] As Integer) As Double
            Dim baseLr = Config.LearningRate

            If Config.WarmupSteps > 0 AndAlso [step] <= Config.WarmupSteps Then
                Return baseLr * [step] / Config.WarmupSteps
            End If

            If Not Config.UseCosineDecay Then Return baseLr

            Dim decaySteps = std.Max(1, Config.TotalSteps - Config.WarmupSteps)
            Dim progress = ([step] - Config.WarmupSteps) / CDbl(decaySteps)
            progress = std.Min(1.0, std.Max(0.0, progress))

            Dim minLr = Config.MinLearningRate
            Return minLr + 0.5 * (baseLr - minLr) * (1.0 + std.Cos(std.PI * progress))
        End Function

        ''' <summary>
        ''' 是否记录训练步内部各阶段的耗时。
        ''' </summary>
        ''' <remarks>
        ''' 默认关闭（计时本身有少量开销，且会干扰逐 step 的 throughput）。
        ''' 打开后 <see cref="LastStageMilliseconds"/> 会给出前向 / 损失 / 反向 /
        ''' 裁剪 / 更新 / MoE 六段的实测耗时 —— 这是定位"瓶颈到底在哪"的唯一可靠手段，
        ''' 比按公式估算算力更可信。
        ''' </remarks>
        Public Property ProfileStages As Boolean = False

        ''' <summary>上一次训练步的各阶段耗时（毫秒），键为阶段名。</summary>
        Public ReadOnly Property LastStageMilliseconds As New List(Of (Stage As String, Ms As Double))

        ''' <summary>执行一个完整的训练步。</summary>
        Public Function TrainStep(batch As LMBatch) As TrainingStepReport
            If batch Is Nothing Then Throw New ArgumentNullException(NameOf(batch))

            Dim watch = Diagnostics.Stopwatch.StartNew()

            _step += 1

            Dim lr = LearningRateAt(_step)
            Dim stages = LastStageMilliseconds
            Dim mark = watch.Elapsed.TotalMilliseconds

            If ProfileStages Then stages.Clear()

            ' 1. 清零梯度（上一步的 AdamW 已经清零过，这里是显式的安全网）
            _model.Parameters.ZeroGradients()

            Dim tZero = watch.Elapsed.TotalMilliseconds

            ' 2. 前向 + 掩码交叉熵
            Dim logits = _model.Forward(batch.TokenIds, batch.BatchSize, batch.SeqLen)

            Dim tForward = watch.Elapsed.TotalMilliseconds
            Dim dLogits As Tensor = Nothing
            Dim loss = LLMTensorOps.MaskedCrossEntropy(logits, batch.Targets, batch.LossMask, dLogits)

            Dim tLoss = watch.Elapsed.TotalMilliseconds

            ' 3. 反向
            _model.Backward(dLogits)

            Dim tBackward = watch.Elapsed.TotalMilliseconds

            ' 4. 全局梯度范数裁剪（必须在更新之前）
            Dim gradNorm = _model.Parameters.ClipGradients(Config.MaxGradNorm)

            Dim tClip = watch.Elapsed.TotalMilliseconds

            ' 5. AdamW 更新（梯度失控时跳过，见 MaxTrustedGradientNorm 的说明）
            Dim trusted = Not Double.IsNaN(gradNorm) AndAlso
                          Not Double.IsInfinity(gradNorm) AndAlso
                          gradNorm <= Config.MaxTrustedGradientNorm

            If trusted Then
                _model.Parameters.ApplyUpdate(lr, _step)
            Else
                ' 丢弃这一步的梯度：前向与损失都还是有效的，坏的只是梯度方向
                _model.Parameters.ZeroGradients()
                _skippedSteps += 1
            End If

            Dim tUpdate = watch.Elapsed.TotalMilliseconds

            ' 6. MoE 负载均衡偏置（不是梯度更新）
            Dim loadRatio = _model.UpdateMoEBalancing()

            watch.Stop()

            If ProfileStages Then
                Call stages.Add(("zeroGrad", tZero - mark))
                Call stages.Add(("forward", tForward - tZero))
                Call stages.Add(("loss", tLoss - tForward))
                Call stages.Add(("backward", tBackward - tLoss))
                Call stages.Add(("clip", tClip - tBackward))
                Call stages.Add(("adamw", tUpdate - tClip))
                Call stages.Add(("moeBalance", watch.Elapsed.TotalMilliseconds - tUpdate))
            End If

            Dim report As New TrainingStepReport With {
                .[Step] = _step,
                .Loss = loss,
                .Perplexity = std.Exp(std.Min(loss, 50.0)),
                .LearningRate = lr,
                .GradientNorm = gradNorm,
                .ElapsedMilliseconds = watch.Elapsed.TotalMilliseconds,
                .MoEMaxLoadRatio = loadRatio,
                .Skipped = Not trusted
            }

            _history.Add(report)

            Return report
        End Function

        ''' <summary>
        ''' 只评估不更新（用于验证集上的损失/困惑度）。
        ''' </summary>
        ''' <remarks>
        ''' 为了不污染训练用的梯度累加器，这里在评估前清零、评估后再次清零。
        ''' </remarks>
        Public Function Evaluate(batch As LMBatch) As Double
            _model.Parameters.ZeroGradients()

            Dim logits = _model.Forward(batch.TokenIds, batch.BatchSize, batch.SeqLen)
            Dim dLogits As Tensor = Nothing
            Dim loss = LLMTensorOps.MaskedCrossEntropy(logits, batch.Targets, batch.LossMask, dLogits)

            _model.Parameters.ZeroGradients()

            Return loss
        End Function

        ''' <summary>把历史损失渲染成一行行的简易曲线（用字符表示相对高低）。</summary>
        Public Function RenderLossCurve(Optional width As Integer = 60) As String
            If _history.Count = 0 Then Return "(no training steps yet)"

            Dim losses = _history.Select(Function(h) h.Loss).ToArray()
            Dim minLoss = losses.Min()
            Dim maxLoss = losses.Max()
            Dim span = maxLoss - minLoss

            If span <= 0 Then span = 1.0

            Dim sb As New System.Text.StringBuilder()

            For Each report In _history
                Dim filled = CInt((report.Loss - minLoss) / span * width)

                Call sb.AppendLine($"step {report.Step,4} |{New String("#"c, filled).PadRight(width)}| {report.Loss:F4}")
            Next

            Return sb.ToString()
        End Function

    End Class

End Namespace
