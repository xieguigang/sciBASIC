Imports System
Imports System.Collections.Generic
Imports System.Linq

Namespace Ensemble

    ''' <summary>
    ''' Stacking 回归器：K 折 OOF 预测拼成元特征，再训练元回归器。
    ''' </summary>
    ''' <remarks>
    ''' 回归没有类别，OOF 元特征宽度等于基模型数；元特征与最终预测都使用
    ''' 同一组基模型定义，训练完成后基模型会用全量数据重训一次。
    ''' </remarks>
    Public NotInheritable Class StackingRegressor : Implements IRegressor

        Private ReadOnly learners As IRegressorLearner()
        Private ReadOnly meta As IRegressorLearner
        Private ReadOnly folds As Integer
        Private ReadOnly seed As Integer
        Private ReadOnly trained As IRegressor()
        Private metaFitted As IRegressor
        Private oofRMSEValue As Double

        Public Sub New(learners As IEnumerable(Of IRegressorLearner),
                       meta As IRegressorLearner,
                       Optional folds As Integer = 5,
                       Optional seed As Integer = 42)
            If learners Is Nothing Then
                Throw New ArgumentNullException(NameOf(learners))
            End If
            If meta Is Nothing Then
                Throw New ArgumentNullException(NameOf(meta))
            End If

            Dim items As IRegressorLearner() = learners.ToArray()

            If items.Length = 0 Then
                Throw New ArgumentException("the stacking regressor requires at least one base learner!", NameOf(learners))
            End If
            If items.Any(Function(m) m Is Nothing) Then
                Throw New ArgumentException("the base learner list contains a NULL entry!", NameOf(learners))
            End If
            If folds < 2 Then
                Throw New ArgumentOutOfRangeException(NameOf(folds), $"stacking requires at least 2 folds, but got {folds}!")
            End If

            Me.learners = items
            Me.meta = meta
            Me.folds = folds
            Me.seed = seed
            Me.trained = New IRegressor(items.Length - 1) {}
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

        ''' <summary>训练后的元回归器</summary>
        Public ReadOnly Property MetaModel As IRegressor
            Get
                Return metaFitted
            End Get
        End Property

        ''' <summary>元回归器在 OOF 元特征上的均方根误差</summary>
        Public ReadOnly Property OofRMSE As Double
            Get
                Return oofRMSEValue
            End Get
        End Property

        Public ReadOnly Property FoldCount As Integer
            Get
                Return folds
            End Get
        End Property

        Public Function Train(x As Double()(), y As Double()) As StackingRegressor
            If x Is Nothing OrElse x.Length = 0 Then
                Throw New ArgumentException("the feature matrix can not be empty!", NameOf(x))
            End If
            If y Is Nothing OrElse y.Length <> x.Length Then
                Throw New ArgumentException(
                    $"the feature rows ({x.Length}) and labels ({If(y Is Nothing, 0, y.Length)}) do not agree!",
                    NameOf(y))
            End If

            Dim n As Integer = x.Length

            Call CrossValidation.Validate(n, folds)

            Dim foldIndex As Integer()() = CrossValidation.KFold(n, folds, seed)
            Dim oof As Double()() = New Double(n - 1)() {}

            For i As Integer = 0 To n - 1
                oof(i) = New Double(learners.Length - 1) {}
            Next

            For l As Integer = 0 To learners.Length - 1
                For Each testIndex As Integer() In foldIndex
                    Dim trainIndex As Integer() = CrossValidation.Complement(n, testIndex)
                    Dim xTrain As Double()() = BootstrapSampler.Resample(x, trainIndex)
                    Dim yTrain As Double() = BootstrapSampler.Resample(y, trainIndex)
                    Dim model As IRegressor = learners(l).Fit(xTrain, yTrain)

                    For Each i As Integer In testIndex
                        oof(i)(l) = model.Predict(x(i))
                    Next
                Next
            Next

            metaFitted = meta.Fit(oof, y)

            For l As Integer = 0 To learners.Length - 1
                trained(l) = learners(l).Fit(x, y)
            Next

            Dim predicted As Double() = New Double(n - 1) {}

            For i As Integer = 0 To n - 1
                predicted(i) = metaFitted.Predict(oof(i))
            Next

            oofRMSEValue = EnsembleMetrics.RMSE(predicted, y)

            Return Me
        End Function

        Public Function Predict(x As Double()) As Double Implements IRegressor.Predict
            RequireTrained()

            Dim metaX As Double() = New Double(trained.Length - 1) {}

            For l As Integer = 0 To trained.Length - 1
                Dim model As IRegressor = trained(l)

                If model Is Nothing Then
                    Throw New InvalidOperationException($"the stacking base model at index {l} is missing!")
                End If

                metaX(l) = model.Predict(x)
            Next

            Dim value As Double = metaFitted.Predict(metaX)

            If Double.IsNaN(value) OrElse Double.IsInfinity(value) Then
                Throw New InvalidOperationException($"the stacking meta regressor returned an invalid prediction ({value})!")
            End If

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
            If metaFitted Is Nothing Then
                Throw New InvalidOperationException(
                    "the stacking regressor has not been trained yet; call Train() before predicting!")
            End If
        End Sub

    End Class

End Namespace
