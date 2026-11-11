Imports System
Imports System.Collections.Generic
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports std = System.Math

Namespace Cox

    ''' <summary>平局（多个个体在同一时刻发生事件）时偏似然的近似方式</summary>
    Public Enum CoxTies
        ''' <summary>Breslow 近似：计算简单，把平局事件当作独立处理</summary>
        Breslow
        ''' <summary>Efron 近似：对平局更精细、偏差更小，与 ``survival::coxph`` 默认一致</summary>
        Efron
    End Enum

    ''' <summary>按观测时间降序分组之后的风险集结构</summary>
    Friend NotInheritable Class CoxGroups

        ''' <summary>``Members(group)`` 为该时刻全部个体的样本下标（含删失者）</summary>
        Public Property Members As Integer()()
        ''' <summary>每个时刻发生的事件数</summary>
        Public Property EventCounts As Integer()
        ''' <summary>每个时刻的观测时间（降序）</summary>
        Public Property Times As Double()
        ''' <summary>总事件数</summary>
        Public Property TotalEvents As Integer

        Public ReadOnly Property GroupCount As Integer
            Get
                Return If(Members Is Nothing, 0, Members.Length)
            End Get
        End Property
    End Class

    ''' <summary>Cox 拟合的预处理数据（标准化空间）</summary>
    Friend NotInheritable Class CoxData
        ''' <summary>标准化后的协变量矩阵 ``Design(sample)(feature)``</summary>
        Public Property Design As Double()()
        ''' <summary>原始协变量矩阵</summary>
        Public Property RawX As Double()()
        ''' <summary>原始生存响应 ``RawY(sample) = {time, status}``</summary>
        Public Property RawY As Double()()
        ''' <summary>观测时间</summary>
        Public Property Time As Double()
        ''' <summary>事件指示（1 = 事件、0 = 删失）</summary>
        Public Property Status As Double()
        ''' <summary>风险集分组（时间降序）</summary>
        Public Property Groups As CoxGroups
        ''' <summary>``U0(j) = sum_{events} Design(i)(j)``，偏似然梯度的常数部分</summary>
        Public Property EventSumX As Double()
        ''' <summary>特征列均值（``standardize=False`` 时为 0）</summary>
        Public Property FeatureMeans As Double()
        ''' <summary>特征列尺度（``standardize=False`` 或常数列时为 1）</summary>
        Public Property FeatureScales As Double()
        ''' <summary>重缩放后的变量惩罚因子（和为 p）</summary>
        Public Property PenaltyFactor As Double()
        ''' <summary>平局处理方式</summary>
        Public Property Ties As CoxTies
        ''' <summary>零模型（beta=0）的对数偏似然</summary>
        Public Property NullLogLikelihood As Double

        Public Property N As Integer
        Public Property P As Integer
        Public Property EventCount As Integer
        Public Property CensoredCount As Integer
    End Class

    ''' <summary>
    ''' Cox 比例风险模型的求解器：负对数偏似然 + 弹性网惩罚，循环坐标下降 + warm starts。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 优化目标：``f(beta) = -l(beta)/n + lambda * [(1-alpha)/2 * ||beta||^2 + alpha * sum_j pf_j |beta_j|]``，
    ''' 其中 ``l`` 为偏似然对数、``pf_j`` 为变量惩罚因子（内部重缩放至和为 p）。
    ''' ``alpha=1`` 为 lasso（变量选择）、``alpha=0`` 为 ridge、两者之间为弹性网。
    ''' </para>
    ''' <para>
    ''' 记风险集 ``R(t)`` 上的和为 ``S0 = sum r_j``、``S1_j = sum r_j*x_j``、
    ''' ``S2_j = sum r_j*x_j^2``，则对坐标 j（Breslow）：
    ''' ``g_j = -(U0_j - sum_t d_t*S1_j/S0)/n``、
    ''' ``h_j = sum_t d_t*(S2_j/S0 - (S1_j/S0)^2)/n``。
    ''' Efron 平局把每个时刻展开成 ``d`` 项 ``S0 - (k/d)*R1``。
    ''' 坐标更新为 ``beta_j = soft(h_j*beta_j - g_j, lambda*alpha*pf_j) / (h_j + lambda*(1-alpha))``。
    ''' </para>
    ''' <para>
    ''' lambda 路径从 KKT 条件给出的 ``lambda_max`` 按对数网格递减，上一步系数作为下一步初值。
    ''' 模型**没有截距**：截距被吸收进基线风险，在偏似然中相互抵消。
    ''' </para>
    ''' </remarks>
    Public Module CoxFitting

        ''' <summary>默认 lambda 路径步数</summary>
        Public Const DefaultNumberOfLambdas As Integer = 100
        ''' <summary>默认坐标下降最大轮数</summary>
        Public Const DefaultMaxIterations As Integer = 1000
        ''' <summary>默认收敛容差</summary>
        Public Const DefaultTolerance As Double = 0.000001
        ''' <summary>n &gt;= p 时的 lambda.min.ratio</summary>
        Public Const MinLambdaRatioLowDim As Double = 0.0001
        ''' <summary>n &lt; p 时的 lambda.min.ratio</summary>
        Public Const MinLambdaRatioHighDim As Double = 0.01
        ''' <summary>海塞矩阵的下界守卫</summary>
        Private Const MinHessian As Double = 0.0000000001
        ''' <summary>风险集和的下界守卫</summary>
        Private Const MinRiskSum As Double = 0.000000000001

        ''' <summary>输入校验，返回样本数 n 与协变量数 p</summary>
        Friend Sub ValidateData(x As Double()(),
                                y As Double()(),
                                alpha As Double,
                                numberOfLambdas As Integer,
                                maxIterations As Integer,
                                tolerance As Double,
                                lambda As Double(),
                                penaltyFactor As Double(),
                                featureNames As String(),
                                ties As CoxTies,
                                caller As String,
                                ByRef n As Integer,
                                ByRef p As Integer)

            If x Is Nothing Then
                Throw New ArgumentNullException(NameOf(x))
            End If
            If y Is Nothing Then
                Throw New ArgumentNullException(NameOf(y))
            End If
            If x.Length = 0 OrElse x(0) Is Nothing OrElse x(0).Length = 0 Then
                Throw New ArgumentException($"the '{caller}' requires a non-empty feature matrix!", NameOf(x))
            End If
            If y.Length = 0 OrElse y(0) Is Nothing OrElse y(0).Length <> 2 Then
                Throw New ArgumentException($"the '{caller}' requires an N x 2 survival response matrix with columns 'time' and 'status'!", NameOf(y))
            End If

            n = x.Length
            p = x(0).Length

            If y.Length <> n Then
                Throw New ArgumentException($"the feature matrix has {n} samples, but the survival response has {y.Length} samples!", NameOf(y))
            End If
            If n < 2 Then
                Throw New ArgumentException($"the '{caller}' requires at least 2 samples, but only {n} were given!", NameOf(x))
            End If
            If Not Double.IsFinite(alpha) OrElse alpha < 0.0 OrElse alpha > 1.0 Then
                Throw New ArgumentException($"alpha must be a finite number in [0,1], but {alpha} was given!", NameOf(alpha))
            End If
            If maxIterations <= 0 Then
                Throw New ArgumentException($"maxIterations must be a positive integer, but {maxIterations} was given!", NameOf(maxIterations))
            End If
            If Not Double.IsFinite(tolerance) OrElse tolerance <= 0.0 Then
                Throw New ArgumentException($"tolerance must be a finite positive number, but {tolerance} was given!", NameOf(tolerance))
            End If
            If lambda Is Nothing AndAlso numberOfLambdas < 2 Then
                Throw New ArgumentException($"numberOfLambdas must be at least 2 when the lambda grid is generated automatically, but {numberOfLambdas} was given!", NameOf(numberOfLambdas))
            End If
            If featureNames IsNot Nothing AndAlso featureNames.Length <> p Then
                Throw New ArgumentException($"the number of feature names ({featureNames.Length}) is not matched with the number of feature columns ({p})!", NameOf(featureNames))
            End If
            If penaltyFactor IsNot Nothing AndAlso penaltyFactor.Length <> p Then
                Throw New ArgumentException($"the size of penaltyFactor ({penaltyFactor.Length}) is not matched with the number of feature columns ({p})!", NameOf(penaltyFactor))
            End If
            If Not System.Enum.IsDefined(GetType(CoxTies), ties) Then
                Throw New ArgumentException($"unsupported cox ties handling: {ties}!", NameOf(ties))
            End If

            Dim events As Integer = 0

            For i As Integer = 0 To n - 1
                If x(i) Is Nothing OrElse x(i).Length <> p Then
                    Throw New ArgumentException($"the row {i} of the feature matrix has {If(x(i) Is Nothing, 0, x(i).Length)} features, but the first row has {p}!", NameOf(x))
                End If
                If y(i) Is Nothing OrElse y(i).Length <> 2 Then
                    Throw New ArgumentException($"the row {i} of the survival response must have exactly 2 values (time, status)!", NameOf(y))
                End If

                For j As Integer = 0 To p - 1
                    If Not Double.IsFinite(x(i)(j)) Then
                        Throw New ArgumentException($"the feature value x({i})({j}) must be a finite number!", NameOf(x))
                    End If
                Next

                Dim observedTime As Double = y(i)(0)
                Dim eventStatus As Double = y(i)(1)

                If Not Double.IsFinite(observedTime) OrElse observedTime < 0.0 Then
                    Throw New ArgumentException($"the survival time y({i})(0) = {observedTime} must be a finite non-negative number!", NameOf(y))
                End If
                If eventStatus <> 0.0 AndAlso eventStatus <> 1.0 Then
                    Throw New ArgumentException($"the status y({i})(1) = {eventStatus} must be 1 (event) or 0 (censored)!", NameOf(y))
                End If
                If eventStatus = 1.0 Then
                    events += 1
                End If
            Next

            If penaltyFactor IsNot Nothing Then
                For j As Integer = 0 To p - 1
                    If Not Double.IsFinite(penaltyFactor(j)) OrElse penaltyFactor(j) < 0.0 Then
                        Throw New ArgumentException($"penaltyFactor({j}) must be a finite non-negative number!", NameOf(penaltyFactor))
                    End If
                Next
            End If

            If events = 0 Then
                Throw New ArgumentException("the cox model requires at least one observed event (status=1)!", NameOf(y))
            End If

            If lambda IsNot Nothing Then
                If lambda.Length = 0 Then
                    Throw New ArgumentException("the custom lambda grid must not be empty!", NameOf(lambda))
                End If

                For li As Integer = 0 To lambda.Length - 1
                    If Not Double.IsFinite(lambda(li)) OrElse lambda(li) <= 0.0 Then
                        Throw New ArgumentException($"lambda({li}) = {lambda(li)} must be a finite positive number!", NameOf(lambda))
                    End If
                    If li > 0 AndAlso lambda(li) >= lambda(li - 1) Then
                        Throw New ArgumentException("the custom lambda grid must be strictly decreasing!", NameOf(lambda))
                    End If
                Next
            End If
        End Sub

        ''' <summary>
        ''' 拟合正则化 Cox 比例风险模型，返回完整的 lambda 正则化路径。
        ''' </summary>
        ''' <param name="x">协变量矩阵 ``x(sample)(feature)``，无截距列</param>
        ''' <param name="y">
        ''' ``N x 2`` 生存响应：``y(i)(0)`` 为观测时间、``y(i)(1)`` 为状态（1=事件、0=删失）。
        ''' </param>
        ''' <param name="lambda">自定义 lambda 网格（严格递减、全为正），缺省自动生成</param>
        ''' <param name="alpha">弹性网混合参数，1 为 lasso、0 为 ridge</param>
        ''' <param name="numberOfLambdas">自动网格步数</param>
        ''' <param name="standardize">是否把协变量标准化为加权单位方差（无截距，中心化不影响偏似然）</param>
        ''' <param name="ties">平局处理方式，缺省 Efron（与 ``survival::coxph`` 一致）</param>
        ''' <param name="penaltyFactor">
        ''' 变量级惩罚乘子（非负），0 表示该变量永不惩罚、永远保留；内部重缩放至和为 p。
        ''' </param>
        ''' <param name="maxIterations">每个 lambda 步骤的坐标下降最大轮数</param>
        ''' <param name="tolerance">收敛容差</param>
        ''' <param name="featureNames">协变量列名</param>
        ''' <param name="Silent">保留参数，求解器不产生控制台输出</param>
        ''' <returns>含系数路径、HR、C 指数、偏差与基线风险估计的结果对象</returns>
        Public Function Fit(x As Double()(),
                            y As Double()(),
                            Optional lambda As Double() = Nothing,
                            Optional alpha As Double = 1.0,
                            Optional numberOfLambdas As Integer = DefaultNumberOfLambdas,
                            Optional standardize As Boolean = True,
                            Optional ties As CoxTies = CoxTies.Efron,
                            Optional penaltyFactor As Double() = Nothing,
                            Optional maxIterations As Integer = DefaultMaxIterations,
                            Optional tolerance As Double = DefaultTolerance,
                            Optional featureNames As String() = Nothing,
                            Optional Silent As Boolean = False) As CoxFit

            Dim n As Integer = 0
            Dim p As Integer = 0

            Call ValidateData(x, y, alpha, numberOfLambdas, maxIterations, tolerance,
                              lambda, penaltyFactor, featureNames, ties,
                              NameOf(Fit), n, p)

            Dim data As CoxData = Prepare(x, y, standardize, penaltyFactor, ties, n, p)
            Dim grid As Double() = If(lambda, LambdaGrid(LambdaMax(data, alpha), numberOfLambdas, n, p))
            Dim names As String() = If(featureNames, DefaultNames(p))

            Return SolvePath(data, grid, alpha, maxIterations, tolerance, names, standardize)
        End Function

        ''' <summary>生成 ``X1..Xp`` 风格的默认列名</summary>
        Friend Function DefaultNames(m As Integer) As String()
            Dim names As String() = New String(m - 1) {}

            For j As Integer = 0 To m - 1
                names(j) = $"X{j + 1}"
            Next

            Return names
        End Function

        ''' <summary>
        ''' 预处理：标准化协变量、构造时间降序风险集、计算梯度常数项与惩罚因子。
        ''' </summary>
        Friend Function Prepare(x As Double()(),
                                y As Double()(),
                                standardize As Boolean,
                                penaltyFactor As Double(),
                                ties As CoxTies,
                                n As Integer, p As Integer) As CoxData

            Dim time As Double() = New Double(n - 1) {}
            Dim status As Double() = New Double(n - 1) {}
            Dim featureMeans As Double() = New Double(p - 1) {}
            Dim featureScales As Double() = New Double(p - 1) {}
            Dim events As Integer = 0

            For i As Integer = 0 To n - 1
                time(i) = y(i)(0)
                status(i) = y(i)(1)

                If status(i) = 1.0 Then
                    events += 1
                End If
            Next

            For j As Integer = 0 To p - 1
                Dim mean As Double = 0.0

                For i As Integer = 0 To n - 1
                    mean += x(i)(j)
                Next

                mean /= CDbl(n)

                Dim variance As Double = 0.0

                For i As Integer = 0 To n - 1
                    Dim d As Double = x(i)(j) - mean
                    variance += d * d
                Next

                variance /= CDbl(n)
                featureMeans(j) = If(standardize, mean, 0.0)
                featureScales(j) = If(standardize AndAlso variance > 0.0, std.Sqrt(variance), 1.0)
            Next

            Dim design As Double()() = RectangularArray.Matrix(Of Double)(n, p)
            Dim eventSumX As Double() = New Double(p - 1) {}

            For i As Integer = 0 To n - 1
                For j As Integer = 0 To p - 1
                    design(i)(j) = (x(i)(j) - featureMeans(j)) / featureScales(j)

                    If status(i) = 1.0 Then
                        eventSumX(j) += design(i)(j)
                    End If
                Next
            Next

            ' glmnet 把惩罚因子重缩放至和为 p，使 lambda 的量纲不随因子数量变化
            Dim pf As Double() = New Double(p - 1) {}
            Dim pfSum As Double = 0.0

            For j As Integer = 0 To p - 1
                pf(j) = If(penaltyFactor Is Nothing, 1.0, penaltyFactor(j))
                pfSum += pf(j)
            Next

            If pfSum <= 0.0 Then
                For j As Integer = 0 To p - 1
                    pf(j) = 1.0
                Next
            Else
                For j As Integer = 0 To p - 1
                    pf(j) = CDbl(p) * pf(j) / pfSum
                Next
            End If

            Dim groups As CoxGroups = BuildGroups(time, status)
            Dim zeroEta As Double() = New Double(n - 1) {}
            Dim nullLogPL As Double = LogPartialLikelihood(zeroEta, status, groups, ties)

            Return New CoxData With {
                .Design = design,
                .RawX = x,
                .RawY = y,
                .Time = time,
                .Status = status,
                .Groups = groups,
                .EventSumX = eventSumX,
                .FeatureMeans = featureMeans,
                .FeatureScales = featureScales,
                .PenaltyFactor = pf,
                .Ties = ties,
                .NullLogLikelihood = nullLogPL,
                .N = n,
                .P = p,
                .EventCount = events,
                .CensoredCount = n - events
            }
        End Function

        ''' <summary>
        ''' 按观测时间降序分组。同一时刻的删失者仍被视为在该时刻处于风险集中。
        ''' </summary>
        Friend Function BuildGroups(time As Double(), status As Double()) As CoxGroups
            Dim n As Integer = time.Length
            Dim order As Integer() = New Integer(n - 1) {}

            For i As Integer = 0 To n - 1
                order(i) = i
            Next

            Dim cmp As Comparison(Of Integer) =
                Function(left As Integer, right As Integer) time(right).CompareTo(time(left))

            Call Array.Sort(order, cmp)

            Dim members As New List(Of Integer())()
            Dim counts As New List(Of Integer)()
            Dim times As New List(Of Double)()
            Dim totalEvents As Integer = 0
            Dim start As Integer = 0

            While start < n
                Dim currentTime As Double = time(order(start))
                Dim [end] As Integer = start

                While [end] < n AndAlso time(order([end])) = currentTime
                    [end] += 1
                End While

                Dim group As Integer() = New Integer([end] - start - 1) {}
                Dim groupEvents As Integer = 0

                For q As Integer = start To [end] - 1
                    Dim idx As Integer = order(q)

                    group(q - start) = idx

                    If status(idx) = 1.0 Then
                        groupEvents += 1
                    End If
                Next

                Call members.Add(group)
                Call counts.Add(groupEvents)
                Call times.Add(currentTime)
                totalEvents += groupEvents
                start = [end]
            End While

            Return New CoxGroups With {
                .Members = members.ToArray(),
                .EventCounts = counts.ToArray(),
                .Times = times.ToArray(),
                .TotalEvents = totalEvents
            }
        End Function

        ''' <summary>``risk(i) = exp(eta(i) - max(eta))``，避免指数上溢且保持比值不变</summary>
        Private Sub NormalizeRisk(eta As Double(), risk As Double())
            Dim n As Integer = eta.Length
            Dim maxEta As Double = Double.NegativeInfinity

            For i As Integer = 0 To n - 1
                maxEta = std.Max(maxEta, eta(i))
            Next

            For i As Integer = 0 To n - 1
                risk(i) = std.Exp(eta(i) - maxEta)
            Next
        End Sub

        ''' <summary>
        ''' 计算坐标 j 上 ``-l(beta)/n`` 的梯度与（对角）海塞矩阵。
        ''' </summary>
        Private Sub GradientHessian(data As CoxData,
                                    risk As Double(),
                                    j As Integer,
                                    ByRef gradient As Double,
                                    ByRef hessian As Double)

            Dim groups As CoxGroups = data.Groups
            Dim s0 As Double = 0.0
            Dim s1 As Double = 0.0
            Dim s2 As Double = 0.0
            Dim riskGradient As Double = 0.0
            Dim riskHessian As Double = 0.0

            For g As Integer = 0 To groups.GroupCount - 1
                Dim group As Integer() = groups.Members(g)

                For Each idx As Integer In group
                    Dim rv As Double = risk(idx)
                    Dim xv As Double = data.Design(idx)(j)

                    s0 += rv
                    s1 += rv * xv
                    s2 += rv * xv * xv
                Next

                Dim d As Integer = groups.EventCounts(g)

                If d = 0 Then
                    Continue For
                End If

                Dim denom0 As Double = std.Max(s0, MinRiskSum)

                If data.Ties = CoxTies.Breslow Then
                    Dim mu As Double = s1 / denom0

                    riskGradient += d * mu
                    riskHessian += d * std.Max(0.0, s2 / denom0 - mu * mu)
                Else
                    Dim r1 As Double = 0.0
                    Dim r1x As Double = 0.0
                    Dim r2x As Double = 0.0

                    For Each idx As Integer In group
                        If data.Status(idx) = 1.0 Then
                            Dim rv As Double = risk(idx)
                            Dim xv As Double = data.Design(idx)(j)

                            r1 += rv
                            r1x += rv * xv
                            r2x += rv * xv * xv
                        End If
                    Next

                    For k As Integer = 0 To d - 1
                        Dim frac As Double = CDbl(k) / CDbl(d)
                        Dim denom As Double = std.Max(s0 - frac * r1, MinRiskSum)
                        Dim a1 As Double = s1 - frac * r1x
                        Dim a2 As Double = s2 - frac * r2x
                        Dim mu As Double = a1 / denom

                        riskGradient += mu
                        riskHessian += std.Max(0.0, a2 / denom - mu * mu)
                    Next
                End If
            Next

            gradient = -(data.EventSumX(j) - riskGradient) / CDbl(data.N)
            hessian = std.Max(riskHessian / CDbl(data.N), MinHessian)
        End Sub

        ''' <summary>软阈值算子 ``sign(v)*max(|v|-t, 0)``</summary>
        Private Function SoftThreshold(value As Double, threshold As Double) As Double
            If threshold <= 0.0 Then
                Return value
            End If
            If value > threshold Then
                Return value - threshold
            End If
            If value < -threshold Then
                Return value + threshold
            End If

            Return 0.0
        End Function

        ''' <summary>
        ''' KKT 条件给出的路径起点：``lambda_max = max_j |g_j(0)| / (alpha * pf_j)``。
        ''' ``alpha=0`` 与全部 ``pf_j=0`` 时退化为有限的正数。
        ''' </summary>
        Friend Function LambdaMax(data As CoxData, alpha As Double) As Double
            Dim unitRisk As Double() = New Double(data.N - 1) {}

            For i As Integer = 0 To data.N - 1
                unitRisk(i) = 1.0
            Next

            Dim maxScore As Double = 0.0
            Dim maxRaw As Double = 0.0

            For j As Integer = 0 To data.P - 1
                Dim g As Double = 0.0
                Dim h As Double = 0.0

                Call GradientHessian(data, unitRisk, j, g, h)
                maxRaw = std.Max(maxRaw, std.Abs(g))

                If data.PenaltyFactor(j) > 0.0 Then
                    maxScore = std.Max(maxScore, std.Abs(g) / data.PenaltyFactor(j))
                End If
            Next

            If maxScore <= 0.0 Then
                maxScore = std.Max(maxRaw, MinHessian)
            End If

            Return std.Max(maxScore / If(alpha > 0.0, alpha, 1.0), MinHessian)
        End Function

        ''' <summary>生成 glmnet 风格的对数间隔 lambda 网格（严格递减）</summary>
        Friend Function LambdaGrid(lambdaMax As Double, steps As Integer, n As Integer, p As Integer) As Double()
            Dim ratio As Double = If(n >= p, MinLambdaRatioLowDim, MinLambdaRatioHighDim)
            Dim grid As Double() = New Double(steps - 1) {}

            For li As Integer = 0 To steps - 1
                grid(li) = lambdaMax * std.Pow(ratio, CDbl(li) / CDbl(steps - 1))
            Next

            Return grid
        End Function

        ''' <summary>
        ''' 偏似然对数 ``l(beta)``。使用 ``r_i = exp(eta_i - max(eta))`` 做数值缩放，
        ''' 因此需要对 ``sum_events eta_i`` 补偿 ``-max(eta)*事件数``。
        ''' </summary>
        Friend Function LogPartialLikelihood(eta As Double(),
                                             status As Double(),
                                             groups As CoxGroups,
                                             ties As CoxTies) As Double

            Dim n As Integer = eta.Length
            Dim maxEta As Double = Double.NegativeInfinity

            For i As Integer = 0 To n - 1
                maxEta = std.Max(maxEta, eta(i))
            Next

            Dim risk As Double() = New Double(n - 1) {}

            For i As Integer = 0 To n - 1
                risk(i) = std.Exp(eta(i) - maxEta)
            Next

            Dim pl As Double = -maxEta * CDbl(groups.TotalEvents)

            For i As Integer = 0 To n - 1
                If status(i) = 1.0 Then
                    pl += eta(i)
                End If
            Next

            Dim s0 As Double = 0.0

            For g As Integer = 0 To groups.GroupCount - 1
                Dim group As Integer() = groups.Members(g)

                For Each idx As Integer In group
                    s0 += risk(idx)
                Next

                Dim d As Integer = groups.EventCounts(g)

                If d = 0 Then
                    Continue For
                End If

                If ties = CoxTies.Breslow Then
                    pl -= CDbl(d) * std.Log(std.Max(s0, MinRiskSum))
                Else
                    Dim r1 As Double = 0.0

                    For Each idx As Integer In group
                        If status(idx) = 1.0 Then
                            r1 += risk(idx)
                        End If
                    Next

                    For k As Integer = 0 To d - 1
                        Dim frac As Double = CDbl(k) / CDbl(d)
                        Dim denom As Double = std.Max(s0 - frac * r1, MinRiskSum)

                        pl -= std.Log(denom)
                    Next
                End If
            Next

            Return pl
        End Function

        ''' <summary>
        ''' 用原始量纲的系数在任意数据子集上计算偏似然对数（供交叉验证使用）。
        ''' </summary>
        Friend Function EvaluateLogPL(xRows As Double()(),
                                      yRows As Double()(),
                                      beta As Double(),
                                      ties As CoxTies) As Double

            Dim n As Integer = xRows.Length
            Dim p As Integer = beta.Length
            Dim eta As Double() = New Double(n - 1) {}
            Dim time As Double() = New Double(n - 1) {}
            Dim status As Double() = New Double(n - 1) {}

            For i As Integer = 0 To n - 1
                Dim score As Double = 0.0

                For j As Integer = 0 To p - 1
                    score += xRows(i)(j) * beta(j)
                Next

                eta(i) = score
                time(i) = yRows(i)(0)
                status(i) = yRows(i)(1)
            Next

            Return LogPartialLikelihood(eta, status, BuildGroups(time, status), ties)
        End Function

        ''' <summary>
        ''' Harrell 一致性指数（C 指数）：只统计「观测时间较早者确实发生事件」的可比较对，
        ''' 时间完全相同或较早者被删失的pair不计入，C 指数越大越好。
        ''' </summary>
        Friend Function ConcordanceIndex(time As Double(),
                                         status As Double(),
                                         riskScore As Double()) As Double

            Dim n As Integer = time.Length
            Dim comparable As Integer = 0
            Dim concordant As Integer = 0
            Dim tied As Integer = 0

            For i As Integer = 0 To n - 1
                For j As Integer = i + 1 To n - 1
                    If time(i) = time(j) Then
                        Continue For
                    End If

                    Dim early As Integer = If(time(i) < time(j), i, j)
                    Dim late As Integer = If(time(i) < time(j), j, i)

                    If status(early) <> 1.0 Then
                        Continue For
                    End If

                    comparable += 1

                    If riskScore(early) > riskScore(late) Then
                        concordant += 1
                    ElseIf riskScore(early) = riskScore(late) Then
                        tied += 1
                    End If
                Next
            Next

            If comparable = 0 Then
                Return 0.5
            End If

            Return (CDbl(concordant) + 0.5 * CDbl(tied)) / CDbl(comparable)
        End Function

        ''' <summary>沿 lambda 路径求解，并把系数还原到原始量纲</summary>
        Private Function SolvePath(data As CoxData,
                                   grid As Double(),
                                   alpha As Double,
                                   maxIterations As Integer,
                                   tolerance As Double,
                                   fNames As String(),
                                   standardize As Boolean) As CoxFit

            Dim n As Integer = data.N
            Dim p As Integer = data.P
            Dim steps As Integer = grid.Length
            Dim beta As Double() = New Double(p - 1) {}
            Dim eta As Double() = New Double(n - 1) {}
            Dim risk As Double() = New Double(n - 1) {}

            Dim coefficients As Double()() = New Double(steps - 1)() {}
            Dim hazardRatios As Double()() = New Double(steps - 1)() {}
            Dim activeCounts As Integer() = New Integer(steps - 1) {}
            Dim deviance As Double() = New Double(steps - 1) {}
            Dim devRatio As Double() = New Double(steps - 1) {}
            Dim cIndex As Double() = New Double(steps - 1) {}
            Dim logPL As Double() = New Double(steps - 1) {}
            Dim convergedFlags As Boolean() = New Boolean(steps - 1) {}
            Dim iterationCounts As Integer() = New Integer(steps - 1) {}

            For li As Integer = 0 To steps - 1
                Dim lam As Double = grid(li)
                Dim iterations As Integer = 0
                Dim isConverged As Boolean = False

                ' warm start：上一步的系数是这一步的初值
                For i As Integer = 0 To n - 1
                    Dim score As Double = 0.0

                    For j As Integer = 0 To p - 1
                        score += data.Design(i)(j) * beta(j)
                    Next

                    eta(i) = score
                Next

                Call NormalizeRisk(eta, risk)

                For iter As Integer = 1 To maxIterations
                    Dim maxDelta As Double = 0.0
                    Dim maxAbsBeta As Double = 1.0

                    iterations = iter

                    For j As Integer = 0 To p - 1
                        Dim g As Double = 0.0
                        Dim h As Double = 0.0

                        Call GradientHessian(data, risk, j, g, h)

                        Dim threshold As Double = lam * alpha * data.PenaltyFactor(j)
                        Dim numerator As Double = h * beta(j) - g
                        Dim shrunk As Double = SoftThreshold(numerator, threshold)
                        Dim denominator As Double = h + lam * (1.0 - alpha)
                        Dim newValue As Double = shrunk / denominator
                        Dim delta As Double = newValue - beta(j)

                        If delta = 0.0 Then
                            Continue For
                        End If

                        beta(j) = newValue
                        maxDelta = std.Max(maxDelta, std.Abs(delta))
                        maxAbsBeta = std.Max(maxAbsBeta, std.Abs(newValue))

                        For i As Integer = 0 To n - 1
                            eta(i) += delta * data.Design(i)(j)
                        Next

                        ' 风险得分只以比值形式进入偏似然，重新归一化可避免指数上溢
                        Call NormalizeRisk(eta, risk)
                    Next

                    If maxDelta <= tolerance * maxAbsBeta Then
                        isConverged = True
                        Exit For
                    End If
                Next

                Dim pl As Double = LogPartialLikelihood(eta, data.Status, data.Groups, data.Ties)
                Dim coefOrig As Double() = New Double(p - 1) {}
                Dim hr As Double() = New Double(p - 1) {}
                Dim active As Integer = 0

                For j As Integer = 0 To p - 1
                    coefOrig(j) = beta(j) / data.FeatureScales(j)
                    hr(j) = std.Exp(std.Max(-50.0, std.Min(50.0, coefOrig(j))))

                    If std.Abs(coefOrig(j)) > 0.0000000001 Then
                        active += 1
                    End If
                Next

                Dim ratio As Double

                If data.NullLogLikelihood < 0.0 Then
                    ratio = 1.0 - pl / data.NullLogLikelihood
                    ratio = std.Min(1.0, std.Max(0.0, ratio))
                Else
                    ratio = 0.0
                End If

                coefficients(li) = coefOrig
                hazardRatios(li) = hr
                activeCounts(li) = active
                logPL(li) = pl
                deviance(li) = -2.0 * pl / CDbl(n)
                devRatio(li) = ratio
                cIndex(li) = ConcordanceIndex(data.Time, data.Status, eta)
                convergedFlags(li) = isConverged
                iterationCounts(li) = iterations
            Next

            Return New CoxFit With {
                .lambdas = grid,
                .coefficients = coefficients,
                .hazardRatios = hazardRatios,
                .df = activeCounts,
                .deviance = deviance,
                .devRatio = devRatio,
                .cIndex = cIndex,
                .logPL = logPL,
                .converged = convergedFlags,
                .iterations = iterationCounts,
                .alpha = alpha,
                .standardize = standardize,
                .ties = data.Ties,
                .penaltyFactor = data.PenaltyFactor,
                .featureNames = fNames,
                .eventCount = data.EventCount,
                .censoredCount = data.CensoredCount,
                .nullLogLikelihood = data.NullLogLikelihood,
                .TrainingX = data.RawX,
                .Time = data.Time,
                .Status = data.Status,
                .RawY = data.RawY,
                .FeatureMeans = data.FeatureMeans,
                .FeatureScales = data.FeatureScales,
                .PathLength = steps
            }
        End Function
    End Module
End Namespace
