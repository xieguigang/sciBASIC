Imports std = System.Math

''' <summary>
''' <c>family = "multinomial"</c>：softmax 多分类弹性网（multnet）。
''' </summary>
''' <remarks>
''' <para>
''' 采用 glmnet 的对称参数化：每个类别有独立系数与截距，
''' <c>p_ic = exp(eta_ic) / sum_c exp(eta_ic)</c>；每次 IRLS 之后
''' 对系数做类别维中心化以消除 softmax 的平移冗余。
''' </para>
''' <para>
''' <c>type.multinomial = "ungrouped"</c>（5.1 默认）逐类别独立 lasso；
''' <c>"grouped"</c> 对变量 j 的 K 维系数向量做组-lasso 块软阈值更新。
''' </para>
''' </remarks>
Friend NotInheritable Class MultinomialFamily : Implements IGlmnetFamily

    Private Const MaxIrlsIterations As Integer = 30

    Public ReadOnly Property Kind As FamilyKind Implements IGlmnetFamily.Kind
        Get
            Return FamilyKind.Multinomial
        End Get
    End Property

    Public Function Fit(data As GlmnetData, options As GlmnetOptions) As FamilyPath Implements IGlmnetFamily.Fit
        Dim n As Integer = data.N
        Dim p As Integer = data.P
        Dim k As Integer = data.K

        Dim standardizer As New WeightedStandardizer(data.X, data.Weights, options.Standardize)
        Dim pf As New PenaltyFactors(options.PenaltyFactor, p, options.Exclude)
        Dim columns As Double()() = standardizer.Columns

        ' 零模型：加权类别频率
        Dim p0 As Double() = New Double(k - 1) {}
        For c As Integer = 0 To k - 1
            Dim sum As Double = 0.0R
            For i As Integer = 0 To n - 1
                sum += data.Weights(i) * data.Y(i)(c)
            Next
            p0(c) = std.Max(sum / CDbl(n), GlmnetControl.MinProbability)
        Next

        Dim a0 As Double() = New Double(k - 1) {}
        Dim meanLog As Double = 0.0R
        For c As Integer = 0 To k - 1
            meanLog += std.Log(p0(c))
        Next
        meanLog /= CDbl(k)
        For c As Integer = 0 To k - 1
            a0(c) = std.Log(p0(c)) - meanLog
        Next

        Dim eta0 As Double()() = New Double(n - 1)() {}
        Dim probs0 As Double()() = New Double(n - 1)() {}
        For i As Integer = 0 To n - 1
            eta0(i) = New Double(k - 1) {}
            probs0(i) = New Double(k - 1) {}
            For c As Integer = 0 To k - 1
                eta0(i)(c) = a0(c) + data.Offsets(i)(c)
                probs0(i)(c) = p0(c)
            Next
        Next
        Dim nullDev As Double = IrlsKernel.MultinomialDeviance(data.Y, probs0, data.Weights)

        ' 分组 KKT 梯度给出 lambda.max
        Dim zeroResidual As Double()() = New Double(n - 1)() {}
        For i As Integer = 0 To n - 1
            zeroResidual(i) = New Double(k - 1) {}
            For c As Integer = 0 To k - 1
                zeroResidual(i)(c) = data.Weights(i) * (data.Y(i)(c) - p0(c))
            Next
        Next
        Dim scores As Double() = LambdaGrid.GroupGradientScores(columns, zeroResidual, n, k)
        Dim grid As Double() = LambdaGrid.Build(options, scores, n, p, options.Alpha, pf)

        Dim beta As Double()() = New Double(p - 1)() {}
        For j As Integer = 0 To p - 1
            beta(j) = New Double(k - 1) {}
        Next

        Dim steps As Integer = grid.Length
        Dim allA0 As Double()() = New Double(steps - 1)() {}
        Dim allBeta As Double()()() = New Double(steps - 1)()() {}
        Dim allDf As Integer() = New Integer(steps - 1) {}
        Dim allNZero As Integer() = New Integer(steps - 1) {}
        Dim allDev As Double() = New Double(steps - 1) {}
        Dim allDevRatio As Double() = New Double(steps - 1) {}
        Dim allConverged As Boolean() = New Boolean(steps - 1) {}
        Dim allIterations As Integer() = New Integer(steps - 1) {}
        Dim executed As Integer = 0
        Dim jerr As GlmnetError = GlmnetError.None

        For s As Integer = 0 To steps - 1
            Dim lambda As Double = grid(s)
            Dim probs As Double()() = Nothing
            Dim dev As Double = 0.0R
            Dim converged As Boolean = False
            Dim iterations As Integer = 0

            For irls As Integer = 0 To MaxIrlsIterations - 1
                Dim eta As Double()() = New Double(n - 1)() {}
                For i As Integer = 0 To n - 1
                    eta(i) = New Double(k - 1) {}
                    For c As Integer = 0 To k - 1
                        Dim v As Double = a0(c) + data.Offsets(i)(c)
                        For j As Integer = 0 To p - 1
                            If beta(j)(c) <> 0.0R Then
                                v += beta(j)(c) * columns(j)(i)
                            End If
                        Next
                        eta(i)(c) = v
                    Next
                Next

                Call IrlsKernel.Softmax(eta, probs)

                Dim convergedAll As Boolean = True
                For c As Integer = 0 To k - 1
                    Dim z As Double() = New Double(n - 1) {}
                    Dim q As Double() = New Double(n - 1) {}

                    For i As Integer = 0 To n - 1
                        Dim prob As Double = probs(i)(c)
                        Dim work As Double = std.Max(prob * (1.0R - prob), GlmnetControl.MinCurvature)
                        z(i) = (eta(i)(c) - data.Offsets(i)(c)) + (data.Y(i)(c) - prob) / work
                        q(i) = data.Weights(i) * work
                    Next

                    Dim betaClass As Double() = New Double(p - 1) {}
                    For j As Integer = 0 To p - 1
                        betaClass(j) = beta(j)(c)
                    Next

                    Dim cd As CoordinateDescent.CDResult
                    If options.TypeMultinomial = MultinomialGrouping.Grouped Then
                        cd = SolveGrouped(columns, data.Weights, beta, a0, c, lambda, options.Alpha,
                                          pf, z, q, options.Thresh, options.MaxIt)
                    Else
                        cd = CoordinateDescent.SolveSingle(columns, z, q, lambda, options.Alpha,
                                                           pf, Nothing, Nothing, betaClass, a0(c),
                                                           options.Intercept, options.Thresh, options.MaxIt)
                        For j As Integer = 0 To p - 1
                            beta(j)(c) = betaClass(j)
                        Next
                        a0(c) = cd.A0
                    End If

                    iterations += cd.Iterations
                    convergedAll = convergedAll AndAlso cd.Converged
                    If Not Double.IsFinite(cd.Objective) Then
                        jerr = GlmnetError.NumericalOverflow
                    End If
                Next

                Call CenterCoefficients(beta, a0, p, k)
                dev = IrlsKernel.MultinomialDeviance(data.Y, probs, data.Weights)
                converged = convergedAll
                If irls >= 2 AndAlso convergedAll Then Exit For
            Next

            Dim betaOrig As Double()() = New Double(p - 1)() {}
            Dim a0Orig As Double() = New Double(k - 1) {}
            For c As Integer = 0 To k - 1
                a0Orig(c) = a0(c)
            Next
            For j As Integer = 0 To p - 1
                betaOrig(j) = New Double(k - 1) {}
                For c As Integer = 0 To k - 1
                    betaOrig(j)(c) = beta(j)(c) / standardizer.Scales(j)
                    a0Orig(c) -= betaOrig(j)(c) * standardizer.Means(j)
                Next
            Next

            allA0(s) = a0Orig
            allBeta(s) = betaOrig
            allDev(s) = dev
            allDevRatio(s) = If(nullDev > 0.0R, std.Max(0.0R, 1.0R - dev / nullDev), 0.0R)
            allConverged(s) = converged
            allIterations(s) = iterations

            Dim nz As Integer = 0
            For j As Integer = 0 To p - 1
                For c As Integer = 0 To k - 1
                    If beta(j)(c) <> 0.0R Then nz += 1
                Next
            Next
            Dim df As Integer = 0
            For j As Integer = 0 To p - 1
                For c As Integer = 0 To k - 1
                    If beta(j)(c) <> 0.0R Then
                        df += 1
                        Exit For
                    End If
                Next
            Next
            allDf(s) = df
            allNZero(s) = nz
            executed = s + 1

            If allDevRatio(s) >= GlmnetControl.DefaultDevMax Then Exit For
        Next

        If executed < steps Then
            Call Array.Resize(allA0, executed)
            Call Array.Resize(allBeta, executed)
            Call Array.Resize(allDf, executed)
            Call Array.Resize(allNZero, executed)
            Call Array.Resize(allDev, executed)
            Call Array.Resize(allDevRatio, executed)
            Call Array.Resize(allConverged, executed)
            Call Array.Resize(allIterations, executed)
            Call Array.Resize(grid, executed)
        End If

        Return New FamilyPath With {
            .Lambda = grid, .A0 = allA0, .Beta = allBeta, .Df = allDf,
            .NZero = allNZero, .Deviance = allDev, .DevRatio = allDevRatio,
            .NullDev = nullDev, .Converged = allConverged, .Iterations = allIterations,
            .Jerr = jerr, .Family = FamilyKind.Multinomial, .Options = options,
            .FeatureNames = data.FeatureNames, .ResponseNames = data.ResponseNames,
            .ClassLabels = data.ClassLabels, .TrainingData = data
        }
    End Function

    ''' <summary>组-lasso 的块坐标下降（变量 j 的 K 维系数一起更新）。</summary>
    Private Function SolveGrouped(columns As Double()(), weights As Double(),
                                  beta As Double()(), a0 As Double(), classIndex As Integer,
                                  lambda As Double, alpha As Double, pf As PenaltyFactors,
                                  z As Double(), q As Double(), thresh As Double, maxIt As Integer) As CoordinateDescent.CDResult

        Dim p As Integer = columns.Length
        Dim n As Integer = z.Length
        Dim k As Integer = a0.Length
        Dim curvature As Double() = New Double(p - 1) {}

        For j As Integer = 0 To p - 1
            Dim sum As Double = 0.0R
            For i As Integer = 0 To n - 1
                sum += weights(i) * columns(j)(i) * columns(j)(i)
            Next
            curvature(j) = sum / CDbl(n)
        Next

        Dim residual As Double() = New Double(n - 1) {}
        For i As Integer = 0 To n - 1
            residual(i) = z(i) - a0(classIndex)
            For j As Integer = 0 To p - 1
                residual(i) -= beta(j)(classIndex) * columns(j)(i)
            Next
        Next

        Dim l1 As Double = lambda * alpha
        Dim l2 As Double = lambda * (1.0R - alpha)
        Dim previous As Double = Double.PositiveInfinity
        Dim iterations As Integer = 0
        Dim converged As Boolean = False
        Dim objective As Double = 0.0R

        While iterations < maxIt
            iterations += 1

            For j As Integer = 0 To p - 1
                If Not pf.CanEnter(j) OrElse curvature(j) <= GlmnetControl.MinCurvature Then
                    For c As Integer = 0 To k - 1
                        beta(j)(c) = 0.0R
                    Next
                    Continue For
                End If

                Dim col As Double() = columns(j)
                Dim rho As Double = 0.0R
                For i As Integer = 0 To n - 1
                    rho += q(i) * col(i) * residual(i)
                Next
                rho = rho / CDbl(n) + curvature(j) * beta(j)(classIndex)

                Dim norm As Double = std.Abs(rho)
                For c As Integer = 0 To k - 1
                    If c <> classIndex Then norm += beta(j)(c) * beta(j)(c)
                Next
                norm = std.Sqrt(std.Max(norm, 0.0R))

                Dim shrink As Double = If(norm > 0.0R, std.Max(0.0R, 1.0R - l1 * pf.Values(j) / norm), 0.0R)
                Dim updated As Double = shrink * rho / (curvature(j) + l2)
                Dim delta As Double = updated - beta(j)(classIndex)

                beta(j)(classIndex) = updated
                If delta <> 0.0R Then
                    For i As Integer = 0 To n - 1
                        residual(i) -= delta * col(i)
                    Next
                End If
            Next

            Dim shift As Double = 0.0R
            For i As Integer = 0 To n - 1
                shift += q(i) * residual(i)
            Next
            shift /= CDbl(n)
            a0(classIndex) += shift
            For i As Integer = 0 To n - 1
                residual(i) -= shift
            Next

            Dim loss As Double = 0.0R
            For i As Integer = 0 To n - 1
                loss += q(i) * residual(i) * residual(i)
            Next
            objective = loss / (2.0R * CDbl(n)) + l1 * pf.Values(0) * std.Abs(beta(0)(classIndex))
            If Double.IsFinite(previous) AndAlso Convergence.HasConverged(previous, objective, thresh) Then
                converged = True
                Exit While
            End If
            previous = objective
        End While

        Return New CoordinateDescent.CDResult With {
            .Beta = New Double(p - 1) {}, .A0 = a0(classIndex), .Residual = residual,
            .Objective = objective, .Iterations = iterations, .Converged = converged
        }
    End Function

    ''' <summary>消除 softmax 的平移冗余：每个变量的类别维系数之和为 0。</summary>
    Private Sub CenterCoefficients(beta As Double()(), a0 As Double(), p As Integer, k As Integer)
        Dim meanIntercept As Double = 0.0R
        For c As Integer = 0 To k - 1
            meanIntercept += a0(c)
        Next
        meanIntercept /= CDbl(k)
        For c As Integer = 0 To k - 1
            a0(c) -= meanIntercept
        Next

        For j As Integer = 0 To p - 1
            Dim m As Double = 0.0R
            For c As Integer = 0 To k - 1
                m += beta(j)(c)
            Next
            m /= CDbl(k)
            For c As Integer = 0 To k - 1
                beta(j)(c) -= m
            Next
        Next
    End Sub

    Public Function Measure(linkPredictions As Double()()(), truth As GlmnetData, metric As CVMeasure) As Double() Implements IGlmnetFamily.Measure
        Dim steps As Integer = linkPredictions.Length
        Dim result As Double() = New Double(steps - 1) {}

        For s As Integer = 0 To steps - 1
            Dim probs As Double()() = Nothing
            Call IrlsKernel.Softmax(linkPredictions(s), probs)

            If metric = CVMeasure.Deviance Then
                result(s) = IrlsKernel.MultinomialDeviance(truth.Y, probs, truth.Weights) / CDbl(truth.N)
            Else
                result(s) = TypeMeasure.EvaluateMulti(New Double()()() {linkPredictions(s)}, truth, metric, FamilyKind.Multinomial)(0)
            End If
        Next

        Return result
    End Function

    Public Function LinkToResponse(link As Double()()) As Double()() Implements IGlmnetFamily.LinkToResponse
        Dim probs As Double()() = Nothing
        Call IrlsKernel.Softmax(link, probs)
        Return probs
    End Function

    Public Function IsMaximize(measure As CVMeasure) As Boolean Implements IGlmnetFamily.IsMaximize
        Return TypeMeasure.IsMaximize(measure)
    End Function

End Class
