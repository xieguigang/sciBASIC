Imports std = System.Math

''' <summary>
''' gaussian / binomial / poisson 共用的单响应 IRLS + 坐标下降路径求解器。
''' </summary>
''' <remarks>
''' <para>
''' 三个 family 只在"链接函数、工作权重、偏差公式"上不同，其余
''' （加权标准化、lambda 网格、强规则、坐标下降、反变换、早停）完全共用。
''' </para>
''' <para>
''' 目标函数为 <c>1/(2n)*deviance + lambda*P(beta)</c>，
''' 与 R 5.1 的 C++ 内核保持一致；所有对外系数都还原到原始量纲。
''' </para>
''' </remarks>
Friend Module SingleResponsePath

    ''' <summary>单个 lambda 步骤允许的最大 IRLS 外循环次数。</summary>
    Private Const MaxIrlsIterations As Integer = 50

    ''' <summary>拟合完整的 lambda 路径。</summary>
    Public Function Fit(data As GlmnetData, options As GlmnetOptions, kind As FamilyKind) As FamilyPath
        Dim n As Integer = data.N
        Dim p As Integer = data.P
        Dim alpha As Double = options.Alpha

        Dim y As Double() = New Double(n - 1) {}
        Dim offset As Double() = New Double(n - 1) {}
        For i As Integer = 0 To n - 1
            y(i) = data.Y(i)(0)
            offset(i) = data.Offsets(i)(0)
        Next

        Dim standardizer As New WeightedStandardizer(data.X, data.Weights, options.Standardize)
        Dim pf As New PenaltyFactors(options.PenaltyFactor, p, options.Exclude)
        Dim lower As Double() = ScaleLimits(options.LowerLimits, standardizer.Scales)
        Dim upper As Double() = ScaleLimits(options.UpperLimits, standardizer.Scales)

        ' 零模型：仅含截距（与 offset），用于 nulldev 与 lambda.max 的梯度
        Dim a0 As Double = InitialIntercept(data, y, offset, kind)
        Dim eta0 As Double() = New Double(n - 1) {}
        For i As Integer = 0 To n - 1
            eta0(i) = a0 + offset(i)
        Next

        Dim zeroResidual As Double() = New Double(n - 1) {}
        For i As Integer = 0 To n - 1
            Dim mu0 As Double

            Select Case kind
                Case FamilyKind.Gaussian
                    mu0 = eta0(i)
                Case FamilyKind.Binomial
                    mu0 = IrlsKernel.Sigmoid(eta0(i))
                Case Else
                    mu0 = Convergence.SafeExp(eta0(i))
            End Select

            zeroResidual(i) = data.Weights(i) * (y(i) - mu0)
        Next

        Dim scores As Double() = LambdaGrid.GradientScores(standardizer.Columns, zeroResidual, n)
        Dim grid As Double() = LambdaGrid.Build(options, scores, n, p, alpha, pf)
        Dim nullDev As Double = Deviance(kind, y, eta0, data.Weights)

        Dim dfMax As Integer = If(options.DfMax > 0, options.DfMax, p + 1)
        Dim pMax As Integer = If(options.PMax > 0, options.PMax, std.Min(dfMax * 2 + 20, p))
        Dim steps As Integer = grid.Length
        Dim allA0 As Double()() = New Double(steps - 1)() {}
        Dim allBeta As Double()()() = New Double(steps - 1)()() {}
        Dim allDf As Integer() = New Integer(steps - 1) {}
        Dim allNZero As Integer() = New Integer(steps - 1) {}
        Dim allDev As Double() = New Double(steps - 1) {}
        Dim allDevRatio As Double() = New Double(steps - 1) {}
        Dim allConverged As Boolean() = New Boolean(steps - 1) {}
        Dim allIterations As Integer() = New Integer(steps - 1) {}
        Dim jerr As GlmnetError = GlmnetError.None

        Dim beta As Double() = New Double(p - 1) {}
        Dim gradients As Double() = New Double(p - 1) {}
        Dim lastWeights As Double() = data.Weights
        ' 泊松的 Fisher 权重 mu 量级远大于 1，会让坐标下降曲率放大到发散。
        ' 用固定的零模型均值同时缩放工作权重与内部 lambda，最优解不变。
        Dim poissonScale As Double = If(kind = FamilyKind.Poisson,
                                        std.Max(IrlsKernel.WeightedMean(y, data.Weights), 1.0R), 1.0R)
        Dim lambdaPrevious As Double = grid(0) / poissonScale
        Dim executed As Integer = 0
        Dim stalled As Integer = 0
        Dim previousDevRatio As Double = 0.0R

        For k As Integer = 0 To steps - 1
            Dim lambda As Double = grid(k) / poissonScale
            Dim mask As Boolean() = Nothing

            If k > 0 Then
                mask = CoordinateDescent.StrongRuleMask(gradients, beta, lambda, lambdaPrevious, alpha, pf)
            End If

            Dim cd As CoordinateDescent.CDResult = Nothing
            Dim dev As Double = 0.0R
            Dim eta As Double() = Nothing
            Dim converged As Boolean = False
            Dim iterations As Integer = 0

            ' 强规则可能漏掉变量：违规时扩大活跃集重解，最多 3 轮
            For attempt As Integer = 0 To 2
                cd = SolveAtLambda(data, options, kind, standardizer, pf, lower, upper,
                                   y, offset, beta, a0, lambda, mask, iterations, converged, eta, dev, lastWeights)

                If cd Is Nothing Then
                    jerr = GlmnetError.NumericalOverflow
                    Exit For
                End If

                Dim currentGradients As Double() = CoordinateDescent.KktGradients(standardizer.Columns, cd.Residual, lastWeights)
                Dim violations As List(Of Integer) = CoordinateDescent.KktViolations(
                    currentGradients, beta, If(mask, AllActive(p)), lambda, alpha, pf)

                If violations.Count = 0 Then
                    gradients = currentGradients
                    Exit For
                End If

                If mask Is Nothing Then
                    mask = AllActive(p)
                End If
                For Each j As Integer In violations
                    mask(j) = True
                Next
                gradients = currentGradients
            Next

            If cd Is Nothing Then
                Exit For
            End If

            Dim betaOrig As Double() = standardizer.RestoreCoefficients(beta)
            Dim a0Orig As Double = standardizer.RestoreIntercept(a0, beta)

            allA0(k) = New Double() {a0Orig}
            allBeta(k) = WrapCoefficients(betaOrig)
            allDev(k) = dev
            allDevRatio(k) = If(nullDev > 0.0R, std.Max(0.0R, 1.0R - dev / nullDev), 0.0R)
            allConverged(k) = converged
            allIterations(k) = iterations

            Dim nonzero As Integer = 0
            For j As Integer = 0 To p - 1
                If beta(j) <> 0.0R Then nonzero += 1
            Next
            allDf(k) = nonzero
            allNZero(k) = nonzero
            executed = k + 1

            If Not converged Then
                jerr = GlmnetError.NonConvergence
            End If

            lambdaPrevious = lambda

            ' glmnet.control() 的 fdev 早停：偏差解释度连续几步几乎不再改善
            Dim improvement As Double = If(previousDevRatio > 0.0000001R,
                                           (allDevRatio(k) - previousDevRatio) / previousDevRatio,
                                           1.0R)
            If k > 0 AndAlso improvement < 0.00001R Then
                stalled += 1
            Else
                stalled = 0
            End If
            previousDevRatio = allDevRatio(k)

            ' 自定义 lambda 序列表示调用方（或 cv.glmnet）已确定网格，
            ' 不应再因 fdev/devmax 截断；自动生成网格时才启用 glmnet 的早停语义。
            If nonzero >= dfMax OrElse
               CountActive(mask, p, beta) > pMax OrElse
               (options.Lambda Is Nothing AndAlso
                (allDevRatio(k) >= GlmnetControl.DefaultDevMax OrElse stalled >= 3)) Then
                Exit For
            End If
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
            .Lambda = grid,
            .A0 = allA0,
            .Beta = allBeta,
            .Df = allDf,
            .NZero = allNZero,
            .Deviance = allDev,
            .DevRatio = allDevRatio,
            .NullDev = nullDev,
            .Converged = allConverged,
            .Iterations = allIterations,
            .Jerr = jerr,
            .Family = kind,
            .Options = options,
            .FeatureNames = data.FeatureNames,
            .ResponseNames = data.ResponseNames,
            .TrainingData = data
        }
    End Function

    ''' <summary>在单个 lambda 上完成 IRLS 外循环。</summary>
    Private Function SolveAtLambda(data As GlmnetData,
                                   options As GlmnetOptions,
                                   kind As FamilyKind,
                                   standardizer As WeightedStandardizer,
                                   pf As PenaltyFactors,
                                   lower As Double(),
                                   upper As Double(),
                                   y As Double(),
                                   offset As Double(),
                                   beta As Double(),
                                   ByRef a0 As Double,
                                   lambda As Double,
                                   mask As Boolean(),
                                   ByRef iterations As Integer,
                                   ByRef converged As Boolean,
                                   ByRef eta As Double(),
                                   ByRef dev As Double,
                                   ByRef lastWeights As Double()) As CoordinateDescent.CDResult

        Dim n As Integer = data.N
        Dim p As Integer = data.P
        Dim columns As Double()() = standardizer.Columns
        Dim z As Double() = New Double(n - 1) {}
        Dim q As Double() = data.Weights
        Dim cd As CoordinateDescent.CDResult = Nothing
        Dim previousDev As Double = Double.PositiveInfinity
        iterations = 0
        converged = False

        For irls As Integer = 0 To MaxIrlsIterations - 1
            eta = BuildEta(columns, beta, a0, offset, n, p)
            Dim devBefore As Double = Deviance(kind, y, eta, data.Weights)

            If kind = FamilyKind.Gaussian Then
                For i As Integer = 0 To n - 1
                    z(i) = y(i) - offset(i)
                Next
                q = data.Weights
            ElseIf kind = FamilyKind.Binomial Then
                Call IrlsKernel.BinomialWorking(eta, y, data.Weights, offset, z, q)
            Else
                Dim weightScale As Double = std.Max(IrlsKernel.WeightedMean(y, data.Weights), 1.0R)
                Call IrlsKernel.PoissonWorking(eta, y, data.Weights, offset, weightScale, z, q)
            End If

            Dim oldBeta As Double() = CType(beta.Clone(), Double())
            Dim oldA0 As Double = a0

            cd = CoordinateDescent.SolveSingle(columns, z, q, lambda, options.Alpha, pf,
                                               lower, upper, beta, a0, options.Intercept,
                                               options.Thresh, options.MaxIt, mask)
            beta = cd.Beta
            a0 = cd.A0
            iterations += cd.Iterations
            lastWeights = q

            If Not Double.IsFinite(cd.Objective) Then
                ' 牛顿步数值失稳：回退到上一轮稳定系数，保留该 lambda 步骤并继续路径。
                ' 直接中断会让 cv.glmnet 拿到空路径。
                For j As Integer = 0 To p - 1
                    beta(j) = oldBeta(j)
                Next
                a0 = oldA0
                dev = devBefore
                Exit For
            End If

            If kind = FamilyKind.Gaussian Then
                dev = Deviance(kind, y, BuildEta(columns, beta, a0, offset, n, p), data.Weights)
                converged = cd.Converged
                Exit For
            End If

            ' glmnet 5.1 的 IRLS 步长折半（epsnr / mxitnr）：
            ' 牛顿步可能让泊松/逻辑偏差上升甚至溢出，此时把系数更新折半，
            ' 直到真实偏差不再变差；仍无法改善则回退到上一轮的稳定解。
            Dim proposedBeta As Double() = CType(beta.Clone(), Double())
            Dim proposedA0 As Double = a0
            Dim candidateEta As Double() = BuildEta(columns, beta, a0, offset, n, p)
            Dim devNew As Double = Deviance(kind, y, candidateEta, data.Weights)
            Dim stepFactor As Double = 1.0R
            Dim accepted As Boolean = False
            Dim tolerance As Double = 0.000000001R * std.Max(1.0R, std.Abs(devBefore))

            For half As Integer = 0 To 20
                If Double.IsFinite(devNew) AndAlso devNew <= devBefore + tolerance Then
                    accepted = True
                    Exit For
                End If

                stepFactor *= 0.5R
                For j As Integer = 0 To p - 1
                    beta(j) = oldBeta(j) + stepFactor * (proposedBeta(j) - oldBeta(j))
                Next
                a0 = oldA0 + stepFactor * (proposedA0 - oldA0)
                candidateEta = BuildEta(columns, beta, a0, offset, n, p)
                devNew = Deviance(kind, y, candidateEta, data.Weights)
            Next

            If accepted Then
                dev = devNew
                eta = candidateEta
            Else
                For j As Integer = 0 To p - 1
                    beta(j) = oldBeta(j)
                Next
                a0 = oldA0
                dev = devBefore
            End If

            If Double.IsFinite(previousDev) AndAlso
               std.Abs(previousDev - dev) <= options.Thresh * std.Max(std.Abs(previousDev), 1.0R) Then
                converged = cd.Converged
                Exit For
            End If
            previousDev = dev
            converged = cd.Converged
        Next

        eta = BuildEta(columns, beta, a0, offset, n, p)
        dev = Deviance(kind, y, eta, data.Weights)

        Return cd
    End Function

    ''' <summary>由标准化空间的系数重建完整线性预测子（含 offset）。</summary>
    Private Function BuildEta(columns As Double()(), beta As Double(), a0 As Double,
                              offset As Double(), n As Integer, p As Integer) As Double()
        Dim eta As Double() = New Double(n - 1) {}

        For i As Integer = 0 To n - 1
            Dim v As Double = a0 + offset(i)
            For j As Integer = 0 To p - 1
                If beta(j) <> 0.0R Then
                    v += beta(j) * columns(j)(i)
                End If
            Next
            eta(i) = v
        Next

        Return eta
    End Function

    ''' <summary>零模型截距（标准化空间）。</summary>
    Private Function InitialIntercept(data As GlmnetData, y As Double(), offset As Double(), kind As FamilyKind) As Double
        Dim n As Integer = data.N
        Dim mean As Double = IrlsKernel.WeightedMean(y, data.Weights)

        Select Case kind
            Case FamilyKind.Gaussian
                Dim adjusted As Double() = New Double(n - 1) {}
                For i As Integer = 0 To n - 1
                    adjusted(i) = y(i) - offset(i)
                Next
                Return IrlsKernel.WeightedMean(adjusted, data.Weights)
            Case FamilyKind.Binomial
                Dim p0 As Double = std.Min(std.Max(mean, GlmnetControl.MinProbability), 1.0R - GlmnetControl.MinProbability)
                Dim meanOffset As Double = IrlsKernel.WeightedMean(offset, data.Weights)
                Return std.Log(p0 / (1.0R - p0)) - meanOffset
            Case FamilyKind.Poisson
                Dim exposure As Double = 0.0R
                For i As Integer = 0 To n - 1
                    exposure += data.Weights(i) * Convergence.SafeExp(offset(i))
                Next
                Dim mu0 As Double = mean * CDbl(n) / std.Max(exposure, GlmnetControl.MinCurvature)
                Return std.Log(std.Max(mu0, GlmnetControl.MinProbability))
            Case Else
                Return 0.0R
        End Select
    End Function

    ''' <summary>零模型的均值（用于 lambda.max 的梯度）。</summary>
    Private Function ZeroModelMean(data As GlmnetData, y As Double(), eta0 As Double(), kind As FamilyKind) As Double
        If kind = FamilyKind.Gaussian Then
            Return IrlsKernel.WeightedMean(y, data.Weights)
        ElseIf kind = FamilyKind.Binomial Then
            Return IrlsKernel.WeightedMean(y, data.Weights)
        Else
            Return IrlsKernel.WeightedMean(y, data.Weights)
        End If
    End Function

    ''' <summary>按 family 计算偏差（未除以 n，与 R 的 nulldev 同尺度）。</summary>
    Private Function Deviance(kind As FamilyKind, y As Double(), eta As Double(), weights As Double()) As Double
        Select Case kind
            Case FamilyKind.Gaussian
                Return IrlsKernel.GaussianDeviance(y, eta, weights)
            Case FamilyKind.Binomial
                Return IrlsKernel.BinomialDeviance(y, eta, weights)
            Case FamilyKind.Poisson
                Return IrlsKernel.PoissonDeviance(y, eta, weights)
            Case Else
                Return 0.0R
        End Select
    End Function

    ''' <summary>把原始量纲的系数限值换算到标准化空间。</summary>
    Private Function ScaleLimits(limits As Double(), scales As Double()) As Double()
        If limits Is Nothing Then
            Return Nothing
        End If

        Dim result As Double() = New Double(limits.Length - 1) {}
        For j As Integer = 0 To limits.Length - 1
            result(j) = limits(j) * scales(j)
        Next
        Return result
    End Function

    ''' <summary>把 p 维系数包装成 <c>p x 1</c> 的响应维数组。</summary>
    Private Function WrapCoefficients(beta As Double()) As Double()()
        Dim result As Double()() = New Double(beta.Length - 1)() {}
        For j As Integer = 0 To beta.Length - 1
            result(j) = New Double() {beta(j)}
        Next
        Return result
    End Function

    Private Function AllActive(p As Integer) As Boolean()
        Dim mask As Boolean() = New Boolean(p - 1) {}
        For j As Integer = 0 To p - 1
            mask(j) = True
        Next
        Return mask
    End Function

    Private Function CountActive(mask As Boolean(), p As Integer, beta As Double()) As Integer
        Dim count As Integer = 0
        For j As Integer = 0 To p - 1
            If beta(j) <> 0.0R OrElse mask Is Nothing OrElse mask(j) Then
                count += 1
            End If
        Next
        Return count
    End Function

End Module
