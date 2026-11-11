Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports std = System.Math

Namespace MGaussian

    ''' <summary>
    ''' 某一个响应在某一个 lambda 步骤上的 <see cref="IFitted"/> 轻量视图。
    ''' 
    ''' 多响应模型本身不是单响应契约，但把 ``(截距, 该响应的系数向量)`` 包装成
    ''' 一个普通多元线性模型之后，就可以复用
    ''' ``NumericTablePrediction.SetPrediction`` 等既有的单响应链路。
    ''' </summary>
    Public Class MGaussianResponseModel : Implements IFitted

        Private ReadOnly owner As MGaussianFit
        Private ReadOnly taskIndex As Integer
        Private ReadOnly stepIndex As Integer
        Private ReadOnly interceptValue As Double
        Private ReadOnly factors As Double()
        Private testCache As IFitError() = Nothing

        ''' <summary>由结果模型构造逐响应视图</summary>
        Friend Sub New(model As MGaussianFit, k As Integer, lambdaIdx As Integer)
            Dim coef As Double()() = model.getCoefficients(lambdaIdx)
            Dim p As Integer = coef.Length - 1

            owner = model
            taskIndex = k
            stepIndex = lambdaIdx
            interceptValue = coef(0)(k)
            factors = New Double(p - 1) {}

            For j As Integer = 0 To p - 1
                factors(j) = coef(j + 1)(k)
            Next
        End Sub

        ''' <summary>
        ''' 该响应在训练数据上的决定系数（由逐点误差真实计算，裁剪到 ``[0,1]``）。
        ''' </summary>
        Public ReadOnly Property R2 As Double Implements IFitted.R2
            Get
                Dim tests As IFitError() = ErrorTest
                Dim n As Integer = tests.Length

                If n = 0 Then
                    Return 0.0
                End If

                Dim mean As Double = 0.0

                For i As Integer = 0 To n - 1
                    mean += tests(i).Y
                Next

                mean /= CDbl(n)

                Dim sse As Double = 0.0
                Dim sst As Double = 0.0

                For i As Integer = 0 To n - 1
                    Dim d As Double = tests(i).Y - tests(i).Yfit
                    Dim t As Double = tests(i).Y - mean

                    sse += d * d
                    sst += t * t
                Next

                Dim r As Double = If(sst > 0.0, 1.0 - sse / sst, If(sse <= 0.0, 1.0, 0.0))

                Return std.Min(1.0, std.Max(0.0, r))
            End Get
        End Property

        ''' <summary>该响应的多元线性多项式（含独立截距）</summary>
        Public ReadOnly Property Polynomial As Formula Implements IFitted.Polynomial
            Get
                Return New Ridge.RidgePolynomial With {
                    .Intercept = interceptValue,
                    .Factors = CType(factors.Clone(), Double())
                }
            End Get
        End Property

        ''' <summary>逐点误差测试结果（惰性构造并缓存）</summary>
        Public Property ErrorTest As IFitError() Implements IFitted.ErrorTest
            Get
                If testCache Is Nothing Then
                    testCache = BuildErrorTest()
                End If

                Return testCache
            End Get
            Set(value As IFitError())
                testCache = value
            End Set
        End Property

        Private Function BuildErrorTest() As IFitError()
            Dim trainingX As Double()() = owner.TrainingX
            Dim trainingY As Double()() = owner.TrainingY
            Dim p As Integer = factors.Length
            Dim n As Integer = If(trainingX Is Nothing, 0, trainingX.Length)
            Dim tests As IFitError() = New IFitError(n - 1) {}

            For i As Integer = 0 To n - 1
                Dim yfit As Double = interceptValue

                For j As Integer = 0 To p - 1
                    yfit += factors(j) * trainingX(i)(j)
                Next

                tests(i) = New TestPoint With {
                    .Y = trainingY(i)(taskIndex),
                    .Yfit = yfit
                }
            Next

            Return tests
        End Function

        ''' <summary>预测该响应的值</summary>
        Public Function GetY(ParamArray x() As Double) As Double Implements IFitted.GetY
            If x Is Nothing Then
                Throw New ArgumentNullException(NameOf(x))
            End If
            If x.Length < factors.Length Then
                Throw New ArgumentException($"the response model requires {factors.Length} feature values, but only {x.Length} were given!", NameOf(x))
            End If

            Dim yfit As Double = interceptValue

            For j As Integer = 0 To factors.Length - 1
                yfit += factors(j) * x(j)
            Next

            Return yfit
        End Function

        ''' <summary>该视图对应的 lambda 步骤下标</summary>
        Public ReadOnly Property LambdaIndex As Integer
            Get
                Return stepIndex
            End Get
        End Property

        ''' <summary>该视图对应的响应列下标</summary>
        Public ReadOnly Property ResponseIndex As Integer
            Get
                Return taskIndex
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return $"MGaussian[response {owner.responseNames(taskIndex)}, lambda_{stepIndex + 1}] @ R2={R2:F4}"
        End Function
    End Class
End Namespace
