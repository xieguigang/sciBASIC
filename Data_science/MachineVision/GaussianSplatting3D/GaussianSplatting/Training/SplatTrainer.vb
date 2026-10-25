' ---------------------------------------------------------------------------
' 训练循环
'
' 从测试工程的 Program.vb 迁入库内，目的有三个：
'   1. 让"训练"成为可复用的库能力（而不是复制粘贴的脚本）；
'   2. 把"每 10 步评估一次"的重复渲染收敛成一次（旧代码每个视图渲染 3 次）；
'   3. 提供分阶段计时与进度回调，便于定位瓶颈。
' ---------------------------------------------------------------------------

Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports std = System.Math

''' <summary>
''' 3D 高斯溅射训练器。
''' </summary>
Public Class SplatTrainer

        Public ReadOnly Property Model As GaussianModel
        Public ReadOnly Property Cameras As List(Of Camera)
        Public ReadOnly Property Targets As List(Of Tensor)

        Public ReadOnly Property Options As TrainingOptions
        Public ReadOnly Property Optimizer As AdamOptimizer
        Public ReadOnly Property Scheduler As LRScheduler
        Public ReadOnly Property Density As DensityController
        Public ReadOnly Property Gradients As GradientCalculator

        Private _lastLoss As Double = Double.NaN
        Private _lastPsnr As Double = Double.NaN

        Public Sub New(model As GaussianModel, cameras As List(Of Camera), targets As List(Of Tensor),
                       Optional options As TrainingOptions = Nothing)
            Me.Model = model
            Me.Cameras = cameras
            Me.Targets = targets
            Me.Options = If(options, New TrainingOptions())

            Dim opt = Me.Options

            Optimizer = New AdamOptimizer(opt.InitialLr)
            Scheduler = New LRScheduler(opt.InitialLr, opt.FinalLr, opt.Iterations)

            Density = New DensityController(opt.DensifyGradThreshold) With {
                .DensifyInterval = opt.DensifyInterval
            }

            Gradients = New GradientCalculator(epsilon:=0.01, batchSize:=0) With {
                .Mode = opt.GradientMode,
                .Options = opt.RenderOptions
            }
        End Sub

        ''' <summary>最近的评估损失。</summary>
        Public ReadOnly Property LastLoss As Double
            Get
                Return _lastLoss
            End Get
        End Property

        ''' <summary>最近的评估 PSNR。</summary>
        Public ReadOnly Property LastPsnr As Double
            Get
                Return _lastPsnr
            End Get
        End Property

        ''' <summary>
        ''' 执行完整训练。
        ''' </summary>
        Public Function Run(Optional onProgress As Action(Of TrainingProgress) = Nothing,
                            Optional cancellation As System.Threading.CancellationToken = Nothing) As TrainingResult
            Dim watch = System.Diagnostics.Stopwatch.StartNew()
            Dim result As New TrainingResult()

            If Options.EnableProfiling Then PerfTrace.Enabled = True
            Call PerfTrace.Reset()

            For iter = 0 To Options.Iterations - 1
                If cancellation.CanBeCanceled AndAlso cancellation.IsCancellationRequested Then Exit For

                Dim progress = [Step](iter)
                result.History.Add(progress)

                onProgress?.Invoke(progress)
            Next

            watch.Stop()

            result.Iterations = Options.Iterations
            result.FinalLoss = _lastLoss
            result.FinalPsnr = _lastPsnr
            result.TotalSeconds = watch.Elapsed.TotalSeconds

            If Options.EnableProfiling Then
                result.ProfileReport = PerfTrace.Report()
                PerfTrace.Enabled = False
            End If

            Return result
        End Function

        ''' <summary>执行单步（学习率调度 → 梯度 → Adam → 致密化 → 可选评估）。</summary>
        Public Function [Step](iter As Integer) As TrainingProgress
            Dim watch = System.Diagnostics.Stopwatch.StartNew()

            Optimizer.LearningRate = Scheduler.GetLr(iter)

            Dim views = SelectViews(iter)

            ' 梯度（同时得到损失）
            Dim tGrad = PerfTrace.Begin()
            Dim stepLoss = Gradients.ComputeLossAndGradients(Model, views, SelectTargets(iter))
            Call PerfTrace.Done(PerfPhase.Backward, tGrad)

            ' 参数更新
            Dim tOpt = PerfTrace.Begin()
            Optimizer.StepModel(Model)
            Call PerfTrace.Done(PerfPhase.Optimize, tOpt)

            ' 自适应密度控制
            Dim cloned = 0, split = 0, pruned = 0

            If Options.DensifyInterval > 0 AndAlso iter >= Options.DensifyFrom AndAlso iter < Options.DensifyUntil Then
                Dim tDense = PerfTrace.Begin()
                Dim stats = Density.Densify(Model, iter)
                Call PerfTrace.Done(PerfPhase.Densify, tDense)

                cloned = stats.Cloned
                split = stats.Split
                pruned = stats.Pruned
            End If

            ' 评估（同一张渲染图只算一次 MSE / PSNR）
            If Options.EvaluateEvery > 0 AndAlso (iter Mod Options.EvaluateEvery = 0 OrElse iter = Options.Iterations - 1) Then
                Dim evaluated = Evaluate()
                _lastLoss = evaluated.Loss
                _lastPsnr = evaluated.Psnr
            End If

            watch.Stop()

            Return New TrainingProgress With {
                .Iteration = iter,
                .Gaussians = Model.Count,
                .Loss = If(Options.EvaluateEvery > 0 AndAlso (iter Mod Options.EvaluateEvery = 0 OrElse iter = Options.Iterations - 1),
                           _lastLoss, Double.NaN),
                .Psnr = If(Options.EvaluateEvery > 0 AndAlso (iter Mod Options.EvaluateEvery = 0 OrElse iter = Options.Iterations - 1),
                           _lastPsnr, Double.NaN),
                .LearningRate = Optimizer.LearningRate,
                .Cloned = cloned,
                .Split = split,
                .Pruned = pruned,
                .StepMs = watch.Elapsed.TotalMilliseconds
            }
        End Function

        ''' <summary>在全量视图上评估当前模型。</summary>
        Public Function Evaluate() As (Loss As Double, Psnr As Double)
            Dim renderOptions = Options.RenderOptions
            Dim loss = 0.0
            Dim count = Cameras.Count

            If count = 0 Then Return (0.0, 0.0)

            Dim viewSums(count - 1) As Double

            System.Threading.Tasks.Parallel.For(0, count, renderOptions.ParallelOptions(),
                Sub(v)
                    Dim t0 = PerfTrace.Begin()
                    Dim rendered = SplatRenderer.Render(Model, Cameras(v), renderOptions)
                    Call PerfTrace.Done(PerfPhase.Loss, t0)

                    viewSums(v) = SplatRenderer.ComputeMSE(rendered, Targets(v))
                End Sub)

            For v = 0 To count - 1
                loss += viewSums(v)
            Next

            loss /= count

            Return (loss, SplatRenderer.ComputePSNR(loss))
        End Function

        ''' <summary>按 <see cref="TrainingOptions.ViewsPerStep"/> 选择本步视图（0 表示全部）。</summary>
        Private Function SelectViews(iter As Integer) As List(Of Camera)
            Dim limit = Options.ViewsPerStep

            If limit <= 0 OrElse limit >= Cameras.Count Then Return Cameras

            Dim selected As New List(Of Camera)(limit)

            For i = 0 To limit - 1
                selected.Add(Cameras((iter * limit + i) Mod Cameras.Count))
            Next

            Return selected
        End Function

        Private Function SelectTargets(iter As Integer) As List(Of Tensor)
            Dim limit = Options.ViewsPerStep

            If limit <= 0 OrElse limit >= Cameras.Count Then Return Targets

            Dim selected As New List(Of Tensor)(limit)

            For i = 0 To limit - 1
                selected.Add(Targets((iter * limit + i) Mod Cameras.Count))
            Next

            Return selected
        End Function

End Class
