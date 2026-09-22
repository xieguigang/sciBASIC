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
Imports std = System.Math
Imports Diagnostics = System.Diagnostics

Namespace LLM

    ''' <summary>训练超参。</summary>
    Public Class TrainingConfig

        ''' <summary>峰值学习率。</summary>
        Public Property LearningRate As Double = 0.0003

        ''' <summary>cosine 衰减的下界。</summary>
        Public Property MinLearningRate As Double = 0.0

        ''' <summary>学习率线性 warmup 的步数。</summary>
        Public Property WarmupSteps As Integer = 10

        ''' <summary>计划的总步数（用于 cosine 衰减）。</summary>
        Public Property TotalSteps As Integer = 100

        ''' <summary>全局梯度范数上限；&lt;= 0 表示不裁剪。</summary>
        Public Property MaxGradNorm As Double = 1.0

        ''' <summary>是否启用 cosine 衰减（关闭则 warmup 之后保持恒定学习率）。</summary>
        Public Property UseCosineDecay As Boolean = True

        ''' <summary>
        ''' Upper bound for the trusted gradient norm; when it is exceeded the step is considered out of control and the
        ''' parameter update is skipped.
        ''' </summary>
        ''' <remarks>
        ''' This is a <b>safety net</b> rather than the normal path. In practice a 200 million parameter model produced a global
        ''' gradient norm spike of 7.8e36 during tool call SFT, and the gradient became NaN on the following step. The forward
        ''' pass was still finite (the loss of that step was normal), only the gradient was broken, so dropping the update of
        ''' that step lets training continue, whereas forcing the update would push the parameters to NaN in one step.
        ''' <para>
        ''' The threshold is intentionally loose (1e6 by default) so that only clearly broken steps are intercepted and normal
        ''' large gradients are left untouched. The number of skipped steps is recorded in <c>LMTrainer.SkippedSteps</c> instead
        ''' of being silently swallowed.
        ''' </para>
        ''' </remarks>
        Public Property MaxTrustedGradientNorm As Double = 1000000.0

    End Class

    ''' <summary>一个训练步的结果报告。</summary>
    Public Class TrainingStepReport

        ''' <summary>Zero based index of the training step.</summary>
        Public Property [Step] As Integer
        ''' <summary>Cross entropy loss of this step.</summary>
        Public Property Loss As Double
        ''' <summary>Perplexity of this step, i.e. <c>exp(Loss)</c>.</summary>
        Public Property Perplexity As Double
        ''' <summary>Learning rate applied in this step.</summary>
        Public Property LearningRate As Double
        ''' <summary>裁剪前的全局梯度范数。</summary>
        Public Property GradientNorm As Double
        ''' <summary>Wall clock duration of this step, in milliseconds.</summary>
        Public Property ElapsedMilliseconds As Double
        ''' <summary>本步结束时各 MoE 层中最差的最大负载比（1.0 = 完全均匀；无 MoE 时为 0）。</summary>
        Public Property MoEMaxLoadRatio As Double

        ''' <summary>本步是否因为梯度失控而跳过了参数更新。</summary>
        Public Property Skipped As Boolean

        ''' <summary>Returns a one line summary of this training step.</summary>
        ''' <returns>A text that reports the step index, loss, perplexity, learning rate and gradient norm.</returns>
        Public Overrides Function ToString() As String
            Dim moe = If(MoEMaxLoadRatio > 0, $", moe_load={MoEMaxLoadRatio:F2}x", "")
            Dim skip = If(Skipped, "  [已跳过更新：梯度失控]", "")

            Return $"step {[Step],4}  loss={Loss:F4}  ppl={Perplexity,8:F2}  lr={LearningRate:E3}  " &
                   $"gnorm={GradientNorm,7:F3}  {ElapsedMilliseconds,7:F0}ms{moe}{skip}"
        End Function

    End Class

    ''' <summary>
    ''' 一个训练批次：等长的若干条 token 序列（展平存放）+ 逐位置的损失掩码。
    ''' </summary>
    Public Class LMBatch

        ''' <summary>输入 token，展平为 <c>[BatchSize * SeqLen]</c>。</summary>
        Public Property TokenIds As Integer()

        ''' <summary>目标 token，展平后布局与 <see cref="TokenIds"/> 一致。</summary>
        Public Property Targets As Integer()

        ''' <summary>逐位置的损失掩码；<see langword="Nothing"/> 表示全部计入。</summary>
        Public Property LossMask As Boolean()

        ''' <summary>Number of sequences in this batch.</summary>
        Public Property BatchSize As Integer
        ''' <summary>Length of every sequence in this batch.</summary>
        Public Property SeqLen As Integer

        ''' <summary>Total number of tokens in this batch, i.e. <c>BatchSize * SeqLen</c>.</summary>
        Public ReadOnly Property TotalTokens As Integer
            Get
                Return BatchSize * SeqLen
            End Get
        End Property

        ''' <summary>实际计入损失的 token 个数。</summary>
        Public ReadOnly Property SupervisedTokens As Integer
            Get
                If LossMask Is Nothing Then Return TotalTokens

                Dim count As Integer = 0

                For Each flag In LossMask
                    If flag Then count += 1
                Next

                Return count
            End Get
        End Property

        ''' <summary>
        ''' 由 <c>[B][L]</c> 的 token 序列与"该 token 是否要被学习"的掩码构造批次。
        ''' </summary>
        ''' <param name="tokens">B 条等长序列，长度 L &gt;= 2</param>
        ''' <param name="learnMask">
        ''' 与 <paramref name="tokens"/> 同形；<c>learnMask(b)(t)=True</c> 表示"希望模型学会
        ''' 在位置 t 预测 token t"。user 消息与工具返回应为 False。
        ''' </param>
        ''' <param name="nextTokenId">
        ''' 每条序列末位的目标占位 token（通常传 EOS）；因为末位之后没有真实的下一个 token。
        ''' 该位置的掩码会被强制置为 False，因此占位值不会影响损失。
        ''' </param>
        ''' <remarks>
        ''' 这里完成标准的"右移一位"错位：位置 t 的输入是 token t，目标是 token t+1，
        ''' 是否计入损失取 <c>learnMask(t+1)</c>。因此输出序列长度是 L − 1。
        ''' </remarks>
        Public Shared Function Create(tokens As Integer()(), learnMask As Boolean()(),
                                      Optional nextTokenId As Integer = 0) As LMBatch

            If tokens Is Nothing OrElse tokens.Length = 0 Then
                Throw New ArgumentException("批次不能为空")
            End If

            Dim batch = tokens.Length
            Dim length = tokens(0).Length

            If length < 2 Then Throw New ArgumentException("每条序列至少需要 2 个 token 才能构造一个预测目标")

            For b As Integer = 0 To batch - 1
                If tokens(b).Length <> length Then
                    Throw New ArgumentException($"第 {b} 条序列长度 {tokens(b).Length} 与第 0 条的 {length} 不一致")
                End If
            Next

            Dim seqLen = length - 1
            Dim ids(batch * seqLen - 1) As Integer
            Dim targets(batch * seqLen - 1) As Integer
            Dim mask(batch * seqLen - 1) As Boolean

            For b As Integer = 0 To batch - 1
                For t As Integer = 0 To seqLen - 1
                    Dim idx = b * seqLen + t

                    ids(idx) = tokens(b)(t)
                    targets(idx) = If(t + 1 < length, tokens(b)(t + 1), nextTokenId)

                    Dim learn As Boolean = True

                    If learnMask IsNot Nothing AndAlso b < learnMask.Length Then
                        Dim row = learnMask(b)

                        If row IsNot Nothing AndAlso t + 1 < row.Length Then learn = row(t + 1)
                    End If

                    ' 末位的"下一个 token"是人为补的，不参与损失
                    If t + 1 >= length Then learn = False

                    mask(idx) = learn
                Next
            Next

            Return New LMBatch With {
                .TokenIds = ids,
                .Targets = targets,
                .LossMask = mask,
                .BatchSize = batch,
                .SeqLen = seqLen
            }
        End Function

    End Class

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
