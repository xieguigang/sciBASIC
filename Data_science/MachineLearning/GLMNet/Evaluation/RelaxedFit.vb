Imports std = System.Math

''' <summary>
''' glmnet 4.1+ 的 relaxed lasso 结果：在 gamma 网格上把 lasso 解与
''' 支持集上的无惩罚重拟合解线性混合。
''' </summary>
''' <remarks>
''' <c>gamma = 1</c> 是原始 lasso 解，<c>gamma = 0</c> 是完全放松（无惩罚）解；
''' 默认网格为 <c>{0, 0.25, 0.5, 0.75, 1}</c>。
''' </remarks>
Public NotInheritable Class RelaxedFit

    ''' <summary>gamma 网格</summary>
    Public Property Gamma As Double()
    ''' <summary>lambda 网格</summary>
    Public Property Lambda As Double()
    ''' <summary><c>(gamma)(lambda)(feature+1)(response)</c> 的放松系数</summary>
    Public Property Coefficients As Double()()()()
    ''' <summary>每个 gamma 下各 lambda 的非零变量数</summary>
    Public Property NZero As Integer()()
    ''' <summary>最优 gamma 下标（CV 选出；未做 CV 时为 0）</summary>
    Public Property BestGammaIndex As Integer

End Class

''' <summary>relaxed lasso 的求解器。</summary>
Public Module RelaxedFitting

    ''' <summary>
    ''' 在给定 lasso 路径之上计算 relaxed 解。
    ''' </summary>
    ''' <remarks>
    ''' 支持集上的无惩罚解通过 <c>lambda = 0</c> 的加权 IRLS + 坐标下降获得；
    ''' cox / mgaussian 的放松拟合需要各自的偏似然 / 组内核，
    ''' 当前版本显式抛出 <see cref="NotSupportedException"/>。
    ''' </remarks>
    Public Function Fit(model As GlmnetFit, options As GlmnetOptions) As RelaxedFit
        If model.Family = FamilyKind.Cox OrElse model.Family = FamilyKind.MGaussian OrElse
           model.Family = FamilyKind.Multinomial Then
            Throw New NotSupportedException($"relax is not implemented for family='{FamilyRegistry.Name(model.Family)}' yet")
        End If

        Dim data As GlmnetData = model.Path.TrainingData
        Dim p As Integer = data.P
        Dim k As Integer = data.K
        Dim n As Integer = data.N
        Dim steps As Integer = model.Count
        Dim gamma As Double() = options.ResolveGamma()
        Dim standardizer As New WeightedStandardizer(data.X, data.Weights, options.Standardize)
        Dim pf As New PenaltyFactors(options.PenaltyFactor, p, options.Exclude)
        Dim result As Double()()()() = New Double(gamma.Length - 1)()()() {}
        Dim nzero As Integer()() = New Integer(gamma.Length - 1)() {}

        Dim y As Double() = New Double(n - 1) {}
        Dim offset As Double() = New Double(n - 1) {}
        For i As Integer = 0 To n - 1
            y(i) = data.Y(i)(0)
            offset(i) = data.Offsets(i)(0)
        Next

        Dim baseA0 As Double = SingleResponseInitialIntercept(data, y, offset, model.Family)

        For g As Integer = 0 To gamma.Length - 1
            result(g) = New Double(steps - 1)()() {}
            nzero(g) = New Integer(steps - 1) {}

            For s As Integer = 0 To steps - 1
                Dim lasso As Double()() = model.GetCoefficients(s)
                Dim support As New List(Of Integer)()
                For j As Integer = 0 To p - 1
                    If lasso(j + 1)(0) <> 0.0R Then support.Add(j)
                Next

                Dim relaxed As Double()() = New Double(p)() {}
                For r As Integer = 0 To p
                    relaxed(r) = New Double(k - 1) {}
                    For c As Integer = 0 To k - 1
                        relaxed(r)(c) = lasso(r)(c)
                    Next
                Next

                If support.Count > 0 AndAlso gamma(g) < 1.0R Then
                    Dim unpenalized As Double()() = UnpenalizedRefit(data, options, model.Family,
                                                                      standardizer, pf, y, offset,
                                                                      support, baseA0)
                    For r As Integer = 0 To p
                        relaxed(r)(0) = gamma(g) * lasso(r)(0) + (1.0R - gamma(g)) * unpenalized(r)(0)
                    Next
                End If

                result(g)(s) = relaxed
                nzero(g)(s) = support.Count
            Next
        Next

        Return New RelaxedFit With {
            .Gamma = gamma,
            .Lambda = CType(model.lambda.Clone(), Double()),
            .Coefficients = result,
            .NZero = nzero,
            .BestGammaIndex = 0
        }
    End Function

    ''' <summary>在支持集上做 <c>lambda = 0</c> 的无惩罚拟合。</summary>
    Private Function UnpenalizedRefit(data As GlmnetData, options As GlmnetOptions, family As FamilyKind,
                                      standardizer As WeightedStandardizer, pf As PenaltyFactors,
                                      y As Double(), offset As Double(), support As List(Of Integer),
                                      a0 As Double) As Double()()

        Dim n As Integer = data.N
        Dim p As Integer = data.P
        Dim columns As Double()() = New Double(support.Count - 1)() {}
        For t As Integer = 0 To support.Count - 1
            columns(t) = standardizer.Columns(support(t))
        Next

        Dim beta As Double() = New Double(support.Count - 1) {}
        Dim z As Double() = New Double(n - 1) {}
        Dim q As Double() = data.Weights

        For irls As Integer = 0 To 20
            Dim eta As Double() = New Double(n - 1) {}
            For i As Integer = 0 To n - 1
                Dim v As Double = a0 + offset(i)
                For t As Integer = 0 To support.Count - 1
                    v += beta(t) * columns(t)(i)
                Next
                eta(i) = v
            Next

            If family = FamilyKind.Gaussian Then
                For i As Integer = 0 To n - 1
                    z(i) = y(i) - offset(i)
                Next
                q = data.Weights
            ElseIf family = FamilyKind.Binomial Then
                Call IrlsKernel.BinomialWorking(eta, y, data.Weights, offset, z, q)
            Else
                Call IrlsKernel.PoissonWorking(eta, y, data.Weights, offset, z, q)
            End If

            Dim cd As CoordinateDescent.CDResult = CoordinateDescent.SolveSingle(
                columns, z, q, 0.0R, options.Alpha, pf, Nothing, Nothing,
                beta, a0, options.Intercept, options.Thresh, options.MaxIt)
            beta = cd.Beta
            a0 = cd.A0
            If family = FamilyKind.Gaussian Then Exit For
        Next

        Dim betaOrig As Double()() = New Double(p)() {}
        For r As Integer = 0 To p
            betaOrig(r) = New Double() {0.0R}
        Next
        betaOrig(0)(0) = a0
        For t As Integer = 0 To support.Count - 1
            Dim j As Integer = support(t)
            betaOrig(j + 1)(0) = beta(t) / standardizer.Scales(j)
            betaOrig(0)(0) -= betaOrig(j + 1)(0) * standardizer.Means(j)
        Next

        Return betaOrig
    End Function

    Private Function SingleResponseInitialIntercept(data As GlmnetData, y As Double(), offset As Double(), family As FamilyKind) As Double
        Dim mean As Double = IrlsKernel.WeightedMean(y, data.Weights)
        If family = FamilyKind.Binomial Then
            Dim p0 As Double = std.Min(std.Max(mean, GlmnetControl.MinProbability), 1.0R - GlmnetControl.MinProbability)
            Return std.Log(p0 / (1.0R - p0))
        ElseIf family = FamilyKind.Poisson Then
            Return std.Log(std.Max(mean, GlmnetControl.MinProbability))
        End If
        Return mean
    End Function

End Module
