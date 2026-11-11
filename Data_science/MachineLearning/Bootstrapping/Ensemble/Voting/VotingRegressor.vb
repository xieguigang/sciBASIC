Imports System
Imports System.Collections.Generic
Imports System.Linq

Namespace Ensemble

    ''' <summary>
    ''' Voting 回归器：对各基回归器的预测值做加权平均。
    ''' </summary>
    ''' <remarks>
    ''' 回归没有「概率」概念，因此这里只有加权均值；若需要抗异常值的聚合，
    ''' 请使用 <see cref="BaggingRegressor"/> 的中位数聚合。
    ''' </remarks>
    Public NotInheritable Class VotingRegressor : Implements IRegressor

        Private ReadOnly learners As IRegressorLearner()
        Private ReadOnly modelWeights As Double()
        Private ReadOnly trained As IRegressor()

        Public Sub New(learners As IEnumerable(Of IRegressorLearner),
                       Optional weights As Double() = Nothing)
            If learners Is Nothing Then
                Throw New ArgumentNullException(NameOf(learners))
            End If

            Dim items As IRegressorLearner() = learners.ToArray()

            If items.Length = 0 Then
                Throw New ArgumentException("the voting regressor requires at least one base learner!", NameOf(learners))
            End If
            If items.Any(Function(m) m Is Nothing) Then
                Throw New ArgumentException("the base learner list contains a NULL entry!", NameOf(learners))
            End If

            Me.learners = items
            Me.modelWeights = ProbabilityMath.NormalizeWeights(weights, items.Length)
            Me.trained = New IRegressor(items.Length - 1) {}
        End Sub

        ''' <summary>基模型数量</summary>
        Public ReadOnly Property BaseModelCount As Integer
            Get
                Return trained.Length
            End Get
        End Property

        ''' <summary>训练后的基模型</summary>
        Public ReadOnly Property BaseModels As IRegressor()
            Get
                Return trained
            End Get
        End Property

        ''' <summary>归一化后的聚合权重</summary>
        Public ReadOnly Property Weights As Double()
            Get
                Return modelWeights
            End Get
        End Property

        ''' <summary>是否并行训练基模型（默认关闭，避免与算法库内部并行嵌套）</summary>
        Public Property Parallel As Boolean = False

        ''' <summary>训练全部基模型</summary>
        Public Function Train(x As Double()(), y As Double()) As VotingRegressor
            If x Is Nothing OrElse x.Length = 0 Then
                Throw New ArgumentException("the feature matrix can not be empty!", NameOf(x))
            End If
            If y Is Nothing OrElse y.Length <> x.Length Then
                Throw New ArgumentException(
                    $"the feature rows ({x.Length}) and labels ({If(y Is Nothing, 0, y.Length)}) do not agree!",
                    NameOf(y))
            End If

            If Parallel AndAlso trained.Length > 1 Then
                Call System.Threading.Tasks.Parallel.For(0, trained.Length,
                    Sub(i)
                        trained(i) = learners(i).Fit(x, y)
                    End Sub)
            Else
                For i As Integer = 0 To learners.Length - 1
                    trained(i) = learners(i).Fit(x, y)
                Next
            End If

            Return Me
        End Function

        Public Function Predict(x As Double()) As Double Implements IRegressor.Predict
            RequireTrained()

            Dim value As Double = 0.0R

            For i As Integer = 0 To trained.Length - 1
                Dim part As Double = trained(i).Predict(x)

                If Double.IsNaN(part) OrElse Double.IsInfinity(part) Then
                    Throw New InvalidOperationException(
                        $"the base regressor '{learners(i).Name}' returned an invalid prediction ({part})!")
                End If

                value += modelWeights(i) * part
            Next

            Return value
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

        Private Sub RequireTrained()
            If trained.Length = 0 OrElse trained(0) Is Nothing Then
                Throw New InvalidOperationException(
                    "the voting regressor has not been trained yet; call Train() before predicting!")
            End If
        End Sub

    End Class

End Namespace
