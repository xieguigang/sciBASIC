Imports std = System.Math

''' <summary>
''' <c>cv.glmnet</c> / <c>assess.glmnet</c> 的折外评估指标实现。
''' </summary>
Friend Module TypeMeasure

    ''' <summary>指标是否"越大越好"。</summary>
    Public Function IsMaximize(measure As CVMeasure) As Boolean
        Return measure = CVMeasure.Auc OrElse measure = CVMeasure.CIndex
    End Function

    ''' <summary>
    ''' 单响应 family 的逐 lambda 指标计算。
    ''' </summary>
    ''' <param name="linkPredictions"><c>(step)(sample)(1)</c> 的 link 预测</param>
    Public Function EvaluateSingle(linkPredictions As Double()()(), truth As GlmnetData,
                                   measure As CVMeasure, kind As FamilyKind) As Double()
        Dim steps As Integer = linkPredictions.Length
        Dim n As Integer = truth.N
        Dim result As Double() = New Double(steps - 1) {}
        Dim y As Double() = New Double(n - 1) {}
        For i As Integer = 0 To n - 1
            y(i) = truth.Y(i)(0)
        Next

        For s As Integer = 0 To steps - 1
            Dim scores As Double() = New Double(n - 1) {}
            For i As Integer = 0 To n - 1
                scores(i) = linkPredictions(s)(i)(0)
            Next

            Select Case measure
                Case CVMeasure.Mse
                    result(s) = WeightedAverage(scores, y, truth.Weights, Function(a, b) (a - b) * (a - b))
                Case CVMeasure.Mae
                    result(s) = WeightedAverage(scores, y, truth.Weights, Function(a, b) std.Abs(a - b))
                Case CVMeasure.Deviance
                    Dim eta As Double()() = Wrap(scores)
                    Select Case kind
                        Case FamilyKind.Gaussian
                            result(s) = IrlsKernel.GaussianDeviance(y, scores, truth.Weights) / CDbl(n)
                        Case FamilyKind.Binomial
                            result(s) = IrlsKernel.BinomialDeviance(y, scores, truth.Weights) / CDbl(n)
                        Case FamilyKind.Poisson
                            result(s) = IrlsKernel.PoissonDeviance(y, scores, truth.Weights) / CDbl(n)
                        Case Else
                            result(s) = WeightedAverage(scores, y, truth.Weights, Function(a, b) (a - b) * (a - b))
                    End Select
                Case CVMeasure.Class
                    Dim errors As Double = 0.0R
                    For i As Integer = 0 To n - 1
                        Dim predicted As Integer = If(IrlsKernel.Sigmoid(scores(i)) >= 0.5R, 1, 0)
                        If predicted <> CInt(std.Round(y(i))) Then
                            errors += truth.Weights(i)
                        End If
                    Next
                    result(s) = errors / CDbl(n)
                Case CVMeasure.Auc
                    result(s) = Auc(scores, y, truth.Weights)
                Case CVMeasure.CIndex
                    result(s) = 0.0R
                Case Else
                    result(s) = 0.0R
            End Select
        Next

        Return result
    End Function

    ''' <summary>多响应 / 多分类的 mse / mae / deviance / class 指标。</summary>
    Public Function EvaluateMulti(linkPredictions As Double()()(), truth As GlmnetData,
                                  measure As CVMeasure, kind As FamilyKind) As Double()
        Dim steps As Integer = linkPredictions.Length
        Dim n As Integer = truth.N
        Dim k As Integer = truth.K
        Dim result As Double() = New Double(steps - 1) {}

        For s As Integer = 0 To steps - 1
            Dim total As Double = 0.0R
            Dim count As Integer = 0
            Dim errors As Double = 0.0R

            For i As Integer = 0 To n - 1
                If measure = CVMeasure.Class Then
                    Dim best As Integer = 0
                    Dim truthClass As Integer = 0
                    For c As Integer = 0 To k - 1
                        If linkPredictions(s)(i)(c) > linkPredictions(s)(i)(best) Then best = c
                        If truth.Y(i)(c) > truth.Y(i)(truthClass) Then truthClass = c
                    Next
                    If best <> truthClass Then errors += truth.Weights(i)
                    count += 1
                Else
                    For c As Integer = 0 To k - 1
                        Dim d As Double = linkPredictions(s)(i)(c) - truth.Y(i)(c)
                        total += truth.Weights(i) * If(measure = CVMeasure.Mae, std.Abs(d), d * d)
                        count += 1
                    Next
                End If
            Next

            result(s) = If(measure = CVMeasure.Class, errors / CDbl(n), total / CDbl(count))
        Next

        Return result
    End Function

    ''' <summary>加权平均的逐点损失。</summary>
    Private Function WeightedAverage(pred As Double(), y As Double(), weights As Double(),
                                     loss As Func(Of Double, Double, Double)) As Double
        Dim n As Integer = y.Length
        Dim total As Double = 0.0R
        For i As Integer = 0 To n - 1
            total += weights(i) * loss(pred(i), y(i))
        Next
        Return total / CDbl(n)
    End Function

    Private Function Wrap(v As Double()) As Double()()
        Dim rows As Double()() = New Double(v.Length - 1)() {}
        For i As Integer = 0 To v.Length - 1
            rows(i) = New Double() {v(i)}
        Next
        Return rows
    End Function

    ''' <summary>
    ''' 加权 AUC（等价的 Mann-Whitney U 统计量，平局按 0.5 计）。
    ''' </summary>
    Public Function Auc(scores As Double(), labels As Double(), weights As Double()) As Double
        Dim n As Integer = scores.Length
        Dim positives As Double = 0.0R
        Dim total As Double = 0.0R

        For i As Integer = 0 To n - 1
            If labels(i) <= 0.0R Then Continue For
            For j As Integer = 0 To n - 1
                If labels(j) > 0.0R Then Continue For
                Dim w As Double = weights(i) * weights(j)
                If scores(i) > scores(j) Then
                    total += w
                ElseIf scores(i) = scores(j) Then
                    total += 0.5R * w
                End If
                positives += w
            Next
        Next

        Return If(positives > 0.0R, total / positives, 0.5R)
    End Function

    ''' <summary>
    ''' Harrell C 指数：按观测时间与事件状态比较可比较对。
    ''' </summary>
    Public Function CIndex(risk As Double(), time As Double(), status As Double()) As Double
        Dim n As Integer = risk.Length
        Dim concordant As Double = 0.0R
        Dim comparable As Double = 0.0R

        For i As Integer = 0 To n - 1
            If status(i) <= 0.0R Then Continue For
            For j As Integer = 0 To n - 1
                If time(j) <= time(i) Then Continue For
                comparable += 1.0R
                If risk(i) > risk(j) Then
                    concordant += 1.0R
                ElseIf risk(i) = risk(j) Then
                    concordant += 0.5R
                End If
            Next
        Next

        Return If(comparable > 0.0R, concordant / comparable, 0.5R)
    End Function

End Module
