Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
Imports std = System.Math

Namespace Poisson

    ''' <summary>
    ''' 泊松回归的极大似然求解器（IRLS / Newton-Raphson）。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 模型形式：``log(mu_i) = b0 + b1*x1 + ... + bp*xp + log(exposure_i)``，
    ''' 其中 ``mu_i`` 为期望计数，``exposure_i`` 为暴露量 offset（缺省为 1）。
    ''' 对数链接保证了 ``mu_i &gt; 0``；当暴露量不同时模型拟合的是事件发生率而非原始计数。
    ''' </para>
    ''' <para>
    ''' 对数似然 ``l(beta) = sum(y_i * eta_i - exp(eta_i) - log(y_i!))`` 是凸函数但没有解析解。
    ''' 对数链接是泊松分布的规范链接，因此 Fisher scoring 与 Newton-Raphson 等价：
    ''' 记 ``W = diag(mu)``，梯度 ``U = X'(y - mu)``，海塞矩阵 ``H = X'WX``，
    ''' 每轮求解 ``H * delta = U`` 并令 ``beta := beta + step * delta``。
    ''' 步长从 1 开始，若对数似然没有改善则折半（保证迭代的数值稳定性）。
    ''' </para>
    ''' <para>
    ''' 与逻辑回归的固定学习率梯度下降不同，这里不需要调学习率，
    ''' 通常 5~10 轮即可二次收敛；正规方程与系数协方差矩阵复用
    ''' <see cref="NumericMatrix"/> 的 Cholesky 分解求解。
    ''' </para>
    ''' </remarks>
    Public Module PoissonFitting

        ''' <summary>线性预测子的钳制范围，避免 ``exp`` 上溢 / 下溢</summary>
        Friend Const MaxEta As Double = 700.0
        ''' <summary>期望计数的下限，避免工作响应与权重除零</summary>
        Friend Const MinMu As Double = 0.000000000001
        ''' <summary>步长折半的最大次数</summary>
        Private Const MaxHalfSteps As Integer = 24

        ''' <summary>
        ''' 把线性预测子钳制到 ``[-700, 700]``，使 ``exp(eta)`` 始终为有限正数。
        ''' </summary>
        Friend Function ClampEta(eta As Double) As Double
            If Double.IsNaN(eta) Then
                Return -MaxEta
            End If
            If eta > MaxEta Then
                Return MaxEta
            End If
            If eta < -MaxEta Then
                Return -MaxEta
            End If

            Return eta
        End Function

        ''' <summary>
        ''' 拟合泊松回归模型。
        ''' </summary>
        ''' <param name="x">
        ''' 特征矩阵，行主序的锯齿数组；每一行为一个样本，每一列为一个自变量。
        ''' 截距由本方法自动补上（<paramref name="addIntercept"/> 为 False 时除外），
        ''' 因此调用方**不需要**在特征矩阵里额外添加常量列。
        ''' </param>
        ''' <param name="y">非负整数计数型响应变量，长度必须等于样本数</param>
        ''' <param name="exposure">
        ''' 暴露量（人年、人口数等），长度必须等于样本数且全部为正数。
        ''' 传入的是原始暴露量 ``t``，内部自动取 ``log(t)`` 作为 offset；
        ''' 缺省（Nothing）时全部视为 1。
        ''' </param>
        ''' <param name="maxIterations">最大迭代轮数，必须为正</param>
        ''' <param name="tolerance">收敛容差（系数最大变化量或对数似然增量），必须为正</param>
        ''' <param name="featureNames">特征列名称，缺省时自动生成 ``[1]..[p]``</param>
        ''' <param name="addIntercept">是否自动拟合独立截距项</param>
        ''' <returns>含系数、标准误、IRR 与过度离散诊断的拟合结果</returns>
        Public Function Fit(x As Double()(),
                            y As Double(),
                            Optional exposure As Double() = Nothing,
                            Optional maxIterations As Integer = 100,
                            Optional tolerance As Double = 0.00000001,
                            Optional featureNames As String() = Nothing,
                            Optional addIntercept As Boolean = True) As PoissonFit

#Region "input validation"
            If x Is Nothing Then
                Throw New ArgumentNullException(NameOf(x))
            End If
            If y Is Nothing Then
                Throw New ArgumentNullException(NameOf(y))
            End If
            If x.Length = 0 OrElse x(0) Is Nothing OrElse x(0).Length = 0 Then
                Throw New ArgumentException("the poisson regression requires a non-empty feature matrix!", NameOf(x))
            End If
            If maxIterations <= 0 Then
                Throw New ArgumentException($"maxIterations must be a positive integer, but {maxIterations} was given!", NameOf(maxIterations))
            End If
            If Not Double.IsFinite(tolerance) OrElse tolerance <= 0.0 Then
                Throw New ArgumentException($"tolerance must be a finite positive number, but {tolerance} was given!", NameOf(tolerance))
            End If

            Dim n As Integer = x.Length
            Dim nFeatures As Integer = x(0).Length

            If y.Length <> n Then
                Throw New ArgumentException($"the size of the feature matrix ({n}) is not matched with the size of the observed counts ({y.Length})!", NameOf(y))
            End If
            If featureNames IsNot Nothing AndAlso featureNames.Length <> nFeatures Then
                Throw New ArgumentException($"the number of feature names ({featureNames.Length}) is not matched with the number of feature columns ({nFeatures})!", NameOf(featureNames))
            End If

            Dim p As Integer = nFeatures + If(addIntercept, 1, 0)

            If n <= p Then
                Throw New ArgumentException($"the poisson regression requires more samples than parameters to have positive residual degrees of freedom, but n={n} and p={p}!", NameOf(x))
            End If

            For i As Integer = 0 To n - 1
                If x(i) Is Nothing OrElse x(i).Length <> nFeatures Then
                    Throw New ArgumentException($"the row {i} of the feature matrix has {If(x(i) Is Nothing, 0, x(i).Length)} features, but the first row has {nFeatures}!", NameOf(x))
                End If

                For j As Integer = 0 To nFeatures - 1
                    If Not Double.IsFinite(x(i)(j)) Then
                        Throw New ArgumentException($"the feature value x({i})({j}) must be a finite number!", NameOf(x))
                    End If
                Next

                If Not Double.IsFinite(y(i)) OrElse y(i) < 0.0 OrElse y(i) <> std.Floor(y(i)) Then
                    Throw New ArgumentException($"the response value y({i}) = {y(i)} must be a finite non-negative integer count!", NameOf(y))
                End If
            Next

            Dim sumY As Double = 0.0

            For i As Integer = 0 To n - 1
                sumY += y(i)
            Next

            If sumY <= 0.0 Then
                Throw New ArgumentException("all of the response counts are zero: the poisson MLE has no finite solution!", NameOf(y))
            End If

            Dim t As Double() = New Double(n - 1) {}
            Dim logOffset As Double() = New Double(n - 1) {}
            Dim sumExposure As Double = 0.0
            Dim hasOffset As Boolean = False

            If exposure Is Nothing Then
                For i As Integer = 0 To n - 1
                    t(i) = 1.0
                Next

                sumExposure = CDbl(n)
            Else
                If exposure.Length <> n Then
                    Throw New ArgumentException($"the size of the exposure vector ({exposure.Length}) is not matched with the size of the observed counts ({n})!", NameOf(exposure))
                End If

                For i As Integer = 0 To n - 1
                    Dim ti As Double = exposure(i)

                    If Not Double.IsFinite(ti) OrElse ti <= 0.0 Then
                        Throw New ArgumentException($"the exposure({i}) = {ti} must be a finite positive number!", NameOf(exposure))
                    End If

                    t(i) = ti
                    logOffset(i) = std.Log(ti)
                    sumExposure += ti

                    If ti <> 1.0 Then
                        hasOffset = True
                    End If
                Next
            End If
#End Region

            ' 1) 设计矩阵：自动补一列常量截距（与 MLR 无截距的约定相反）
            Dim design As Double()() = RectangularArray.Matrix(Of Double)(n, p)
            Dim firstFeatureColumn As Integer = If(addIntercept, 1, 0)

            For i As Integer = 0 To n - 1
                If addIntercept Then
                    design(i)(0) = 1.0
                End If

                For j As Integer = 0 To nFeatures - 1
                    design(i)(firstFeatureColumn + j) = x(i)(j)
                Next
            Next

            Dim names As String() = New String(p - 1) {}
            Dim nameOffset As Integer = 0

            If addIntercept Then
                names(0) = "(Intercept)"
                nameOffset = 1
            End If

            For j As Integer = 0 To nFeatures - 1
                names(nameOffset + j) = If(featureNames Is Nothing, $"[{j + 1}]", featureNames(j))
            Next

            ' 2) 饱和对数似然之中的常数项 -sum(log(y!))。
            '    计数通常不大，小整数直接累加 log(k)，大计数用 Stirling 展开，
            '    避免为了一个常数项引入第三个工程引用（stats-netcore5）。
            Dim logFactorialSum As Double = 0.0

            For i As Integer = 0 To n - 1
                logFactorialSum += LogFactorial(y(i))
            Next

            ' 3) 初始值：截距取「总计数 / 总暴露量」的对数，斜率全部为 0。
            '    这是截距-only 模型的精确 MLE，作为 Newton 迭代的起点非常稳健。
            Dim beta As Double() = New Double(p - 1) {}

            If addIntercept Then
                beta(0) = std.Log(sumY / sumExposure)
            End If

            Dim ll As Double = LogLikelihood(design, y, beta, logOffset, logFactorialSum)
            Dim converged As Boolean = False
            Dim iterations As Integer = 0

            ' 4) IRLS / Newton-Raphson
            For iter As Integer = 1 To maxIterations
                Dim H As Double()() = RectangularArray.Matrix(Of Double)(p, p)
                Dim U As Double() = New Double(p - 1) {}

                For i As Integer = 0 To n - 1
                    Dim eta As Double = ClampEta(Dot(design(i), beta) + logOffset(i))
                    Dim mu As Double = std.Max(std.Exp(eta), MinMu)
                    Dim residual As Double = y(i) - mu

                    ' 只累加 H 的上三角再镜像，保证 Cholesky 的对称判据（精确比较）成立
                    For a As Integer = 0 To p - 1
                        Dim xa As Double = design(i)(a)

                        U(a) += residual * xa

                        For b As Integer = a To p - 1
                            H(a)(b) += mu * xa * design(i)(b)
                        Next
                    Next
                Next

                For a As Integer = 0 To p - 1
                    For b As Integer = a + 1 To p - 1
                        H(b)(a) = H(a)(b)
                    Next
                Next

                Dim hessian As NumericMatrix = New NumericMatrix(H)
                Dim chol As CholeskyDecomposition = hessian.chol()

                If Not chol.SPD Then
                    Throw New InvalidOperationException("the poisson regression can not be solved on the given data: X'WX is not positive definite (collinear feature columns or complete separation), please remove duplicated/constant feature columns!")
                End If

                Dim rhs As NumericMatrix = New NumericMatrix(U, p)
                Dim sol As NumericMatrix = CType(chol.Solve(rhs), NumericMatrix)
                Dim delta As Double() = New Double(p - 1) {}

                For a As Integer = 0 To p - 1
                    delta(a) = sol(a, 0)

                    If Not Double.IsFinite(delta(a)) Then
                        Throw New InvalidOperationException("the poisson regression produced a non-finite parameter update, please check the scale of the feature columns!")
                    End If
                Next

                ' 对数似然是凹函数，全步长通常即可收敛；数值异常时折半步长，
                ' 保证每一次被接受的迭代都不会让目标函数变差
                Dim stepSize As Double = 1.0
                Dim nextBeta As Double() = New Double(p - 1) {}
                Dim nextLL As Double = Double.NegativeInfinity
                Dim accepted As Boolean = False

                For half As Integer = 0 To MaxHalfSteps - 1
                    For a As Integer = 0 To p - 1
                        nextBeta(a) = beta(a) + stepSize * delta(a)
                    Next

                    nextLL = LogLikelihood(design, y, nextBeta, logOffset, logFactorialSum)

                    If Double.IsFinite(nextLL) AndAlso nextLL >= ll - 0.000000000001 Then
                        accepted = True
                        Exit For
                    End If

                    stepSize *= 0.5
                Next

                iterations = iter

                If Not accepted Then
                    ' 目标函数已无法继续改善，视为收敛到数值精度极限
                    converged = True
                    Exit For
                End If

                Dim maxDelta As Double = 0.0

                For a As Integer = 0 To p - 1
                    maxDelta = std.Max(maxDelta, std.Abs(nextBeta(a) - beta(a)))
                Next

                Dim llDelta As Double = std.Abs(nextLL - ll)

                beta = nextBeta
                ll = nextLL

                If maxDelta <= tolerance OrElse llDelta <= tolerance * (1.0 + std.Abs(ll)) Then
                    converged = True
                    Exit For
                End If
            Next

            ' 5) 收敛之后的统计量：期望计数、偏差、Pearson 卡方与最终的海塞矩阵
            Dim muFinal As Double() = New Double(n - 1) {}
            Dim HF As Double()() = RectangularArray.Matrix(Of Double)(p, p)
            Dim deviance As Double = 0.0
            Dim pearson As Double = 0.0

            For i As Integer = 0 To n - 1
                Dim eta As Double = ClampEta(Dot(design(i), beta) + logOffset(i))
                Dim mu As Double = std.Max(std.Exp(eta), MinMu)
                Dim yi As Double = y(i)
                Dim ylog As Double = If(yi > 0.0, yi * std.Log(yi / mu), 0.0)

                muFinal(i) = mu
                deviance += 2.0 * (ylog - (yi - mu))
                pearson += (yi - mu) * (yi - mu) / mu

                For a As Integer = 0 To p - 1
                    For b As Integer = a To p - 1
                        HF(a)(b) += mu * design(i)(a) * design(i)(b)
                    Next
                Next
            Next

            For a As Integer = 0 To p - 1
                For b As Integer = a + 1 To p - 1
                    HF(b)(a) = HF(a)(b)
                Next
            Next

            ' 浮点误差可能让偏差出现极小的负值
            deviance = std.Max(0.0, deviance)

            ' 6) 系数协方差 = (X'WX)^(-1)，标准误与 Wald Z 值
            Dim finalHessian As NumericMatrix = New NumericMatrix(HF)
            Dim cholF As CholeskyDecomposition = finalHessian.chol()

            If Not cholF.SPD Then
                Throw New InvalidOperationException("the poisson regression can not estimate the coefficient covariance on the given data: X'WX is not positive definite!")
            End If

            Dim inv As NumericMatrix = CType(cholF.Solve(NumericMatrix.Identity(p)), NumericMatrix)
            Dim se As Double() = New Double(p - 1) {}
            Dim zvalues As Double() = New Double(p - 1) {}
            Dim irr As Double() = New Double(p - 1) {}

            For a As Integer = 0 To p - 1
                se(a) = std.Sqrt(std.Max(inv(a, a), 0.0))
                zvalues(a) = If(se(a) > 0.0, beta(a) / se(a), 0.0)
                irr(a) = std.Exp(ClampEta(beta(a)))
            Next

            ' 7) 零模型（仅截距 + 相同的 offset）偏差：MLE 为总计数 / 总暴露量
            Dim nullRate As Double = sumY / sumExposure
            Dim nullDeviance As Double = 0.0

            For i As Integer = 0 To n - 1
                Dim mu0 As Double = std.Max(nullRate * t(i), MinMu)
                Dim yi As Double = y(i)
                Dim ylog As Double = If(yi > 0.0, yi * std.Log(yi / mu0), 0.0)

                nullDeviance += 2.0 * (ylog - (yi - mu0))
            Next

            nullDeviance = std.Max(0.0, nullDeviance)

            Dim coef As Double() = New Double(nFeatures - 1) {}
            Dim betaOffset As Integer = If(addIntercept, 1, 0)
            Dim intercept As Double = If(addIntercept, beta(0), 0.0)

            For j As Integer = 0 To nFeatures - 1
                coef(j) = beta(betaOffset + j)
            Next

            Dim tests As IFitError() = New IFitError(n - 1) {}

            For i As Integer = 0 To n - 1
                tests(i) = New TestPoint With {
                    .Y = y(i),
                    .Yfit = muFinal(i)
                }
            Next

            Return New PoissonFit With {
                .Intercept = intercept,
                .Coefficients = coef,
                .AddIntercept = addIntercept,
                .FeatureNames = names,
                .Exposures = t,
                .HasOffset = hasOffset,
                .StandardErrors = se,
                .ZValues = zvalues,
                .IncidenceRateRatios = irr,
                .FittedValues = muFinal,
                .Deviance = deviance,
                .NullDeviance = nullDeviance,
                .ResidualDf = n - p,
                .PearsonChiSquare = pearson,
                .LogLikelihood = ll,
                .AIC = 2.0 * p - 2.0 * ll,
                .Converged = converged,
                .Iterations = iterations,
                .ErrorTest = tests
            }
        End Function

        ''' <summary>行向量与系数向量的点积 ``X_i * beta``</summary>
        Private Function Dot(row As Double(), beta As Double()) As Double
            Dim sum As Double = 0.0

            For j As Integer = 0 To beta.Length - 1
                sum += row(j) * beta(j)
            Next

            Return sum
        End Function

        ''' <summary>
        ''' 泊松对数似然 ``sum(y*eta - exp(eta) - log(y!))``，
        ''' 其中 ``eta`` 已经包含 ``log(exposure)`` offset。
        ''' </summary>
        Private Function LogLikelihood(design As Double()(),
                                       y As Double(),
                                       beta As Double(),
                                       logOffset As Double(),
                                       logFactorialSum As Double) As Double

            Dim ll As Double = -logFactorialSum

            For i As Integer = 0 To y.Length - 1
                Dim eta As Double = ClampEta(Dot(design(i), beta) + logOffset(i))
                Dim mu As Double = std.Max(std.Exp(eta), MinMu)

                ll += y(i) * eta - mu
            Next

            Return ll
        End Function

        ''' <summary>
        ''' ``log(y!)``：小计数直接累加 ``log(k)``，大计数用 Stirling 展开，
        ''' 避免阶乘本身溢出，也避免为了该常数项依赖未被引用的统计工程。
        ''' </summary>
        Private Function LogFactorial(y As Double) As Double
            If y <= 1.0 Then
                Return 0.0
            End If

            If y <= 32.0 Then
                Dim sum As Double = 0.0

                For k As Integer = 2 To CInt(y)
                    sum += std.Log(k)
                Next

                Return sum
            End If

            Return y * std.Log(y) - y + 0.5 * std.Log(2.0 * std.PI * y) + 1.0 / (12.0 * y) - 1.0 / (360.0 * y * y * y)
        End Function
    End Module
End Namespace
