Imports System

Namespace Ensemble

    ''' <summary>
    ''' 用委托实现的分类器，便于把任意「输入特征、输出概率」的函数接入集成框架。
    ''' </summary>
    Public NotInheritable Class DelegateClassifier : Implements IClassifier

        Private ReadOnly classesArray As Double()
        Private ReadOnly predictor As Func(Of Double(), Double())

        Public Sub New(classes As Double(), predictor As Func(Of Double(), Double()))
            If classes Is Nothing OrElse classes.Length = 0 Then
                Throw New ArgumentException("the classes array can not be empty!", NameOf(classes))
            End If
            If predictor Is Nothing Then
                Throw New ArgumentNullException(NameOf(predictor))
            End If

            Me.classesArray = CType(classes.Clone(), Double())
            Me.predictor = predictor
        End Sub

        Public ReadOnly Property Classes As Double() Implements IClassifier.Classes
            Get
                Return classesArray
            End Get
        End Property

        Public Function PredictProbability(x As Double()) As Double() Implements IClassifier.PredictProbability
            Return predictor(x)
        End Function

    End Class

    ''' <summary>用委托实现的回归器</summary>
    Public NotInheritable Class DelegateRegressor : Implements IRegressor

        Private ReadOnly predictor As Func(Of Double(), Double)

        Public Sub New(predictor As Func(Of Double(), Double))
            If predictor Is Nothing Then
                Throw New ArgumentNullException(NameOf(predictor))
            End If

            Me.predictor = predictor
        End Sub

        Public Function Predict(x As Double()) As Double Implements IRegressor.Predict
            Return predictor(x)
        End Function

    End Class

    ''' <summary>用委托实现的分类基学习器</summary>
    Public NotInheritable Class DelegateClassifierLearner : Implements IClassifierLearner

        Private ReadOnly modelName As String
        Private ReadOnly fitFunction As Func(Of Double()(), Integer(), Double(), IClassifier)

        Public Sub New(name As String, fit As Func(Of Double()(), Integer(), Double(), IClassifier))
            If fit Is Nothing Then
                Throw New ArgumentNullException(NameOf(fit))
            End If

            Me.modelName = If(name, "delegate-classifier")
            Me.fitFunction = fit
        End Sub

        Public ReadOnly Property Name As String Implements IClassifierLearner.Name
            Get
                Return modelName
            End Get
        End Property

        Public Function Fit(x As Double()(), yIndex As Integer(), classes As Double()) As IClassifier Implements IClassifierLearner.Fit
            Return fitFunction(x, yIndex, classes)
        End Function

    End Class

    ''' <summary>用委托实现的回归基学习器</summary>
    Public NotInheritable Class DelegateRegressorLearner : Implements IRegressorLearner

        Private ReadOnly modelName As String
        Private ReadOnly fitFunction As Func(Of Double()(), Double(), IRegressor)

        Public Sub New(name As String, fit As Func(Of Double()(), Double(), IRegressor))
            If fit Is Nothing Then
                Throw New ArgumentNullException(NameOf(fit))
            End If

            Me.modelName = If(name, "delegate-regressor")
            Me.fitFunction = fit
        End Sub

        Public ReadOnly Property Name As String Implements IRegressorLearner.Name
            Get
                Return modelName
            End Get
        End Property

        Public Function Fit(x As Double()(), y As Double()) As IRegressor Implements IRegressorLearner.Fit
            Return fitFunction(x, y)
        End Function

    End Class

End Namespace
