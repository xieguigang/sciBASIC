Namespace Trainer

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

End Namespace