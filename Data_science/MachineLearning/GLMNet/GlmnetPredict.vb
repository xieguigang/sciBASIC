Imports std = System.Math

''' <summary>
''' 统一的 <c>predict.glmnet</c> / <c>coef.glmnet</c> / <c>deviance.glmnet</c> 入口。
''' </summary>
Public Module GlmnetPredict

    ''' <summary>
    ''' 在给定 lambda 处预测。
    ''' </summary>
    ''' <param name="fit">拟合结果</param>
    ''' <param name="newx">待预测特征矩阵，每行 p 列</param>
    ''' <param name="type">link / response / coefficients / nonzero / class</param>
    ''' <param name="s">lambda 值；为空表示路径最后一步</param>
    ''' <param name="exact">是否在该 lambda 上重新拟合（而不是插值）</param>
    ''' <param name="newoffset">预测期 offset，形状 <c>n x K</c> 或 <c>n x 1</c></param>
    Public Function Predict(fit As GlmnetFit,
                            newx As Double()(),
                            Optional type As PredictionType = PredictionType.Link,
                            Optional s As Double? = Nothing,
                            Optional exact As Boolean = False,
                            Optional newoffset As Double()() = Nothing) As PredictionBundle

        If fit Is Nothing Then Throw New ArgumentNullException(NameOf(fit))
        If newx Is Nothing Then Throw New ArgumentNullException(NameOf(newx))

        Dim lambda As Double = If(s.HasValue, s.Value, fit.lambda(fit.Count - 1))
        Dim coefficients As Double()() = fit.GetCoefficients(lambda, exact)

        If type = PredictionType.Coefficients Then
            Return New PredictionBundle With {
                .Type = type, .Coefficients = coefficients, .Lambda = lambda,
                .ClassLabels = fit.classLabels
            }
        End If

        If type = PredictionType.NonZero Then
            Dim p As Integer = fit.nvars
            Dim k As Integer = fit.nresponses
            Dim indices As New List(Of Integer)()

            For j As Integer = 0 To p - 1
                For c As Integer = 0 To k - 1
                    If coefficients(j + 1)(c) <> 0.0R Then
                        indices.Add(j)
                        Exit For
                    End If
                Next
            Next

            Return New PredictionBundle With {
                .Type = type, .Indices = indices.ToArray(), .Lambda = lambda
            }
        End If

        Dim link As Double()() = fit.LinearPredictor(newx, coefficients, newoffset)

        If type = PredictionType.Link Then
            Return PredictionBundle.FromValues(type, link, lambda)
        End If

        If type = PredictionType.Response Then
            Dim response As Double()() = FamilyRegistry.Create(fit.Family).LinkToResponse(link)
            Return PredictionBundle.FromValues(type, response, lambda)
        End If

        If type = PredictionType.Class Then
            Dim n As Integer = link.Length
            Dim k As Integer = link(0).Length
            Dim classIndex As Integer() = New Integer(n - 1) {}

            For i As Integer = 0 To n - 1
                If fit.Family = FamilyKind.Binomial Then
                    classIndex(i) = If(IrlsKernel.Sigmoid(link(i)(0)) >= 0.5R, 1, 0)
                Else
                    Dim best As Integer = 0
                    For c As Integer = 1 To k - 1
                        If link(i)(c) > link(i)(best) Then best = c
                    Next
                    classIndex(i) = best
                End If
            Next

            Return New PredictionBundle With {
                .Type = type, .ClassIndex = classIndex,
                .ClassLabels = fit.classLabels, .Lambda = lambda
            }
        End If

        Throw New ArgumentOutOfRangeException(NameOf(type))
    End Function

    ''' <summary>取指定 lambda 处的 <c>(p+1) x K</c> 系数矩阵。</summary>
    Public Function Coef(fit As GlmnetFit, Optional s As Double? = Nothing, Optional exact As Boolean = False) As Double()()
        Return fit.GetCoefficients(s, exact)
    End Function

    ''' <summary>
    ''' 在给定数据与 lambda 处重新计算模型偏差（与 <c>deviance.glmnet</c> 对应）。
    ''' </summary>
    Public Function Deviance(fit As GlmnetFit, newx As Double()(), newy As Double()(),
                             Optional s As Double? = Nothing) As Double
        Dim link As Double()() = Predict(fit, newx, PredictionType.Link, s).Values
        Dim n As Integer = link.Length
        Dim weights As Double() = New Double(n - 1) {}
        For i As Integer = 0 To n - 1
            weights(i) = 1.0R
        Next

        Select Case fit.Family
            Case FamilyKind.Gaussian
                Dim y As Double() = newy.Select(Function(r) r(0)).ToArray()
                Dim eta As Double() = link.Select(Function(r) r(0)).ToArray()
                Return IrlsKernel.GaussianDeviance(y, eta, weights)
            Case FamilyKind.Binomial
                Dim y As Double() = newy.Select(Function(r) r(0)).ToArray()
                Dim eta As Double() = link.Select(Function(r) r(0)).ToArray()
                Return IrlsKernel.BinomialDeviance(y, eta, weights)
            Case FamilyKind.Poisson
                Dim y As Double() = newy.Select(Function(r) r(0)).ToArray()
                Dim eta As Double() = link.Select(Function(r) r(0)).ToArray()
                Return IrlsKernel.PoissonDeviance(y, eta, weights)
            Case FamilyKind.Multinomial
                Dim probs As Double()() = Nothing
                Call IrlsKernel.Softmax(link, probs)
                Return IrlsKernel.MultinomialDeviance(newy, probs, weights)
            Case Else
                Dim total As Double = 0.0R
                For i As Integer = 0 To n - 1
                    For c As Integer = 0 To link(i).Length - 1
                        Dim d As Double = link(i)(c) - newy(i)(c)
                        total += d * d
                    Next
                Next
                Return total
        End Select
    End Function

End Module
