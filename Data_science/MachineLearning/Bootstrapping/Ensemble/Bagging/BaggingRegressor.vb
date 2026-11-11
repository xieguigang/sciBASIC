Imports System

Namespace Ensemble

    ''' <summary>
    ''' Bagging 回归器：对训练集做有放回采样训练多个同质基学习器，
    ''' 再用均值或中位数聚合预测值。
    ''' </summary>
    ''' <remarks>
    ''' 中位数聚合对长尾的基模型误差更稳健，适合存在明显异常样本的场景；
    ''' 均值聚合方差更小，是默认选择。
    ''' </remarks>
    Public NotInheritable Class BaggingRegressor : Implements IRegressor

        Private ReadOnly learner As IRegressorLearner
        Private ReadOnly modelCount As Integer
        Private ReadOnly ratio As Double
        Private ReadOnly seed As Integer
        Private ReadOnly trained As IRegressor()

        Public Sub New(learner As IRegressorLearner,
                       Optional modelCount As Integer = 10,
                       Optional ratio As Double = 1.0R,
                       Optional seed As Integer = 42)
            If learner Is Nothing Then
                Throw New ArgumentNullException(NameOf(learner))
            End If
            If modelCount < 1 Then
                Throw New ArgumentOutOfRangeException(NameOf(modelCount), $"the model count must be positive, but got {modelCount}!")
            End If
            If ratio <= 0.0R OrElse ratio > 2.0R Then
                Throw New ArgumentOutOfRangeException(
                    NameOf(ratio), $"the sample ratio must be within (0, 2], but got {ratio}!")
            End If

            Me.learner = learner
            Me.modelCount = modelCount
            Me.ratio = ratio
            Me.seed = seed
            Me.trained = New IRegressor(modelCount - 1) {}
        End Sub

        Public ReadOnly Property BaseModelCount As Integer
            Get
                Return trained.Length
            End Get
        End Property

        Public ReadOnly Property BaseModels As IRegressor()
            Get
                Return trained
            End Get
        End Property

        Public ReadOnly Property SampleRatio As Double
            Get
                Return ratio
            End Get
        End Property

        ''' <summary>聚合方式</summary>
        Public Property Aggregation As BaggingAggregation = BaggingAggregation.Mean

        ''' <summary>是否并行训练基模型（默认关闭，避免与算法库内部并行嵌套）</summary>
        Public Property Parallel As Boolean = False

        Public Function Train(x As Double()(), y As Double()) As BaggingRegressor
            If x Is Nothing OrElse x.Length = 0 Then
                Throw New ArgumentException("the feature matrix can not be empty!", NameOf(x))
            End If
            If y Is Nothing OrElse y.Length <> x.Length Then
                Throw New ArgumentException(
                    $"the feature rows ({x.Length}) and labels ({If(y Is Nothing, 0, y.Length)}) do not agree!",
                    NameOf(y))
            End If

            Dim n As Integer = x.Length
            Dim size As Integer = System.Math.Max(1, CInt(System.Math.Round(n * ratio)))

            If Parallel AndAlso trained.Length > 1 Then
                Call System.Threading.Tasks.Parallel.For(0, trained.Length,
                    Sub(m)
                        trained(m) = FitOne(x, y, n, size, m)
                    End Sub)
            Else
                For m As Integer = 0 To trained.Length - 1
                    trained(m) = FitOne(x, y, n, size, m)
                Next
            End If

            Return Me
        End Function

        Private Function FitOne(x As Double()(), y As Double(),
                                n As Integer, size As Integer, modelIndex As Integer) As IRegressor
            Dim sampler As New BootstrapSampler(seed + modelIndex)
            Dim index As Integer() = sampler.Sample(size)
            Dim xSub As Double()() = BootstrapSampler.Resample(x, index)
            Dim ySub As Double() = BootstrapSampler.Resample(y, index)

            Return learner.Fit(xSub, ySub)
        End Function

        Public Function Predict(x As Double()) As Double Implements IRegressor.Predict
            RequireTrained()

            Dim values As Double() = New Double(trained.Length - 1) {}

            For m As Integer = 0 To trained.Length - 1
                Dim model As IRegressor = trained(m)

                If model Is Nothing Then
                    Throw New InvalidOperationException($"the bagging base model at index {m} is missing!")
                End If

                Dim part As Double = model.Predict(x)

                If Double.IsNaN(part) OrElse Double.IsInfinity(part) Then
                    Throw New InvalidOperationException(
                        $"the bagging base model at index {m} returned an invalid prediction ({part})!")
                End If

                values(m) = part
            Next

            If Aggregation = BaggingAggregation.Median Then
                Return Median(values)
            End If

            Dim total As Double = 0.0R

            For m As Integer = 0 To values.Length - 1
                total += values(m)
            Next

            Return total / values.Length
        End Function

        ''' <summary>批量预测</summary>
        Public Function PredictBatch(x As Double()()) As Double()
            If x Is Nothing Then
                Throw New ArgumentNullException(NameOf(x))
            End If

            Dim result As Double() = New Double(x.Length - 1) {}

            For i As Integer = 0 To x.Length - 1
                result(i) = Predict(x(i))
            Next

            Return result
        End Function

        Private Shared Function Median(values As Double()) As Double
            Dim sorted As Double() = CType(values.Clone(), Double())

            Call Array.Sort(sorted)

            Dim middle As Integer = sorted.Length \ 2

            If sorted.Length Mod 2 = 1 Then
                Return sorted(middle)
            End If

            Return (sorted(middle - 1) + sorted(middle)) / 2.0R
        End Function

        Private Sub RequireTrained()
            If trained.Length = 0 OrElse trained(0) Is Nothing Then
                Throw New InvalidOperationException(
                    "the bagging regressor has not been trained yet; call Train() before predicting!")
            End If
        End Sub

    End Class

End Namespace
