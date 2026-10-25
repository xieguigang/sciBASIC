' ---------------------------------------------------------------------------
' 训练配置与进度模型
' ---------------------------------------------------------------------------

''' <summary>训练配置。</summary>
Public Class TrainingOptions

        ''' <summary>迭代次数</summary>
        Public Property Iterations As Integer = 500

        ''' <summary>初始学习率</summary>
        Public Property InitialLr As Double = 0.015

        ''' <summary>最终学习率（余弦退火）</summary>
        Public Property FinalLr As Double = 0.001

        ''' <summary>从第几步开始致密化</summary>
        Public Property DensifyFrom As Integer = 50

        ''' <summary>到第几步停止致密化</summary>
        Public Property DensifyUntil As Integer = 400

        ''' <summary>致密化间隔（0 表示关闭自适应密度控制）</summary>
        Public Property DensifyInterval As Integer = 100

        ''' <summary>致密化的位置梯度阈值</summary>
        Public Property DensifyGradThreshold As Double = 0.0002

        ''' <summary>每隔多少步评估一次（0 表示不评估）</summary>
        Public Property EvaluateEvery As Integer = 10

        ''' <summary>梯度计算模式</summary>
        Public Property GradientMode As SplatGradientMode = SplatGradientMode.Analytic

        ''' <summary>渲染参数</summary>
        Public Property RenderOptions As SplatRenderOptions = SplatRenderOptions.Default

        ''' <summary>是否记录分阶段耗时</summary>
        Public Property EnableProfiling As Boolean = False

        ''' <summary>每步使用的视图数（0 表示使用全部视图）</summary>
        Public Property ViewsPerStep As Integer = 0

        Public Function Clone() As TrainingOptions
            Return New TrainingOptions With {
                .Iterations = Iterations,
                .InitialLr = InitialLr,
                .FinalLr = FinalLr,
                .DensifyFrom = DensifyFrom,
                .DensifyUntil = DensifyUntil,
                .DensifyInterval = DensifyInterval,
                .DensifyGradThreshold = DensifyGradThreshold,
                .EvaluateEvery = EvaluateEvery,
                .GradientMode = GradientMode,
                .RenderOptions = If(RenderOptions?.Clone(), Nothing),
                .EnableProfiling = EnableProfiling,
                .ViewsPerStep = ViewsPerStep
            }
        End Function

    End Class

    ''' <summary>单步训练进度快照。</summary>
    Public Structure TrainingProgress

        Public Iteration As Integer
        Public Gaussians As Integer
        ''' <summary>本步（或本次评估）的 MSE；未评估时为 NaN</summary>
        Public Loss As Double
        ''' <summary>本步（或本次评估）的 PSNR；未评估时为 NaN</summary>
        Public Psnr As Double
        Public LearningRate As Double
        Public Cloned As Integer
        Public Split As Integer
        Public Pruned As Integer
        ''' <summary>当前步耗时（毫秒）</summary>
        Public StepMs As Double

        Public Overrides Function ToString() As String
            Dim lossText = If(Double.IsNaN(Loss), "-", Loss.ToString("F6"))
            Dim psnrText = If(Double.IsNaN(Psnr), "-", Psnr.ToString("F2"))

            Return $"iter {Iteration,4:D} | gaussians {Gaussians,7:D} | loss {lossText} | psnr {psnrText} dB | " &
                   $"lr {LearningRate:F5} | +{Cloned} ~{Split} -{Pruned} | {StepMs:F0} ms"
        End Function

    End Structure

    ''' <summary>训练汇总结果。</summary>
    Public Class TrainingResult

        Public Property Iterations As Integer
        Public Property FinalLoss As Double
        Public Property FinalPsnr As Double
        Public Property TotalSeconds As Double
        Public Property History As New List(Of TrainingProgress)()

        ''' <summary>分阶段耗时报告（EnableProfiling 时非空）</summary>
        Public Property ProfileReport As String

End Class
