Imports System.Collections.Generic
Imports System.Linq
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports std = System.Math

Namespace Cox

    ''' <summary>
    ''' 一条（个体化）生存曲线：各事件时刻的基线/个体累积风险与生存概率。
    ''' 与 glmnet 的 ``survfit`` 一样，本实现不提供置信带。
    ''' </summary>
    Public Class SurvivalCurve

        ''' <summary>事件时刻（升序）</summary>
        Public Property Time As Double()
        ''' <summary>``Survival(i) = exp(-CumulativeHazard(i))``</summary>
        Public Property Survival As Double()
        ''' <summary>个体的累积风险 ``H0(t)*exp(x'beta)``</summary>
        Public Property CumulativeHazard As Double()
        ''' <summary>基线生存函数 ``S0(t) = exp(-H0(t))``</summary>
        Public Property BaselineSurvival As Double()
        ''' <summary>该个体的线性预测子（风险得分对数） ``x'beta``</summary>
        Public Property RiskScore As Double
        ''' <summary>该曲线对应的新样本下标</summary>
        Public Property SampleIndex As Integer

        ''' <summary>
        ''' 阶梯函数插值：返回时间 t 处的生存概率（第一个事件时刻之前为 1）。
        ''' </summary>
        Public Function SurvivalAt(t As Double) As Double
            If Time Is Nothing OrElse Time.Length = 0 Then
                Return 1.0
            End If

            Dim hit As Integer = -1

            For i As Integer = 0 To Time.Length - 1
                If Time(i) <= t Then
                    hit = i
                Else
                    Exit For
                End If
            Next

            Return If(hit < 0, 1.0, Survival(hit))
        End Function

        Public Overrides Function ToString() As String
            Dim last As Integer = If(Survival Is Nothing, 0, Survival.Length) - 1
            Return $"SurvivalCurve[{If(Time Is Nothing, 0, Time.Length)} event time(s)] @ riskScore={RiskScore:F4}, S(last)={If(last >= 0, Survival(last), 1.0):F4}"
        End Function
    End Class

    ''' <summary>
    ''' Cox 比例风险模型的 lambda 正则化路径结果。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 模型**没有截距**：``getCoefficients`` 返回长度 p 的系数向量，
    ''' ``hazardRatios(step)(j) = exp(beta_j)`` 即风险比 HR。
    ''' ``deviance = -2*l(beta)/n``（部分似然偏差，可相差一个常数），
    ''' ``devRatio`` 是相对于零模型的伪 R²，``cIndex`` 是 Harrell 一致性指数。
    ''' </para>
    ''' <para>
    ''' 本类实现 <see cref="IFitted"/>，但语义与回归模型不同：
    ''' <see cref="GetY"/> 返回的是线性预测子（风险得分对数），
    ''' <see cref="R2"/> 是伪 R²，<see cref="ErrorTest"/> 为 Nothing（生存数据没有逐点残差）。
    ''' 默认使用路径最后一步（惩罚最弱）的系数；需要其它 lambda 步骤请用
    ''' <see cref="GetRiskModel"/>。
    ''' </para>
    ''' </remarks>
    Public Class CoxFit : Implements IFitted

        ''' <summary>每一步的 lambda（严格递减）</summary>
        Public Property lambdas As Double()
        ''' <summary>``coefficients(step)(feature)``，原始量纲，无截距</summary>
        Public Property coefficients As Double()()
        ''' <summary>``hazardRatios(step)(feature) = exp(beta)``</summary>
        Public Property hazardRatios As Double()()
        ''' <summary>每一步的非零系数个数</summary>
        Public Property df As Integer()
        ''' <summary>每一步的部分似然偏差 ``-2*l/n``</summary>
        Public Property deviance As Double()
        ''' <summary>每一步相对零模型的伪 R²</summary>
        Public Property devRatio As Double()
        ''' <summary>每一步训练集上的 Harrell C 指数</summary>
        Public Property cIndex As Double()
        ''' <summary>每一步的对数偏似然</summary>
        Public Property logPL As Double()
        ''' <summary>每一步是否收敛</summary>
        Public Property converged As Boolean()
        ''' <summary>每一步的坐标下降实际轮数</summary>
        Public Property iterations As Integer()

        ''' <summary>弹性网混合参数</summary>
        Public Property alpha As Double
        ''' <summary>是否标准化了协变量</summary>
        Public Property standardize As Boolean
        ''' <summary>平局处理方式</summary>
        Public Property ties As CoxTies
        ''' <summary>重缩放后的变量惩罚因子</summary>
        Public Property penaltyFactor As Double()
        ''' <summary>协变量列名</summary>
        Public Property featureNames As String()
        ''' <summary>事件数</summary>
        Public Property eventCount As Integer
        ''' <summary>删失数</summary>
        Public Property censoredCount As Integer
        ''' <summary>零模型对数偏似然</summary>
        Public Property nullLogLikelihood As Double

        ''' <summary>训练协变量矩阵（用于估计基线风险）</summary>
        Public Property TrainingX As Double()()
        ''' <summary>训练观测时间</summary>
        Public Property Time As Double()
        ''' <summary>训练事件指示</summary>
        Public Property Status As Double()
        ''' <summary>训练生存响应 ``RawY(sample) = {time, status}``</summary>
        Public Property RawY As Double()()
        ''' <summary>特征列均值</summary>
        Public Property FeatureMeans As Double()
        ''' <summary>特征列尺度</summary>
        Public Property FeatureScales As Double()
        ''' <summary>路径步数</summary>
        Public Property PathLength As Integer

        ''' <summary>协变量数量 p</summary>
        Public ReadOnly Property NumberOfFeatures As Integer
            Get
                Return If(coefficients Is Nothing OrElse coefficients.Length = 0, 0, coefficients(0).Length)
            End Get
        End Property

        ''' <summary>路径起点（惩罚最强）</summary>
        Public ReadOnly Property MaxLambda As Double
            Get
                Return lambdas(0)
            End Get
        End Property

        ''' <summary>路径终点（惩罚最弱）</summary>
        Public ReadOnly Property MinLambda As Double
            Get
                Return lambdas(lambdas.Length - 1)
            End Get
        End Property

        Private Function ResolveIndex(lambdaIdx As Integer) As Integer
            Dim steps As Integer = lambdas.Length
            Dim idx As Integer = If(lambdaIdx < 0, steps - 1, lambdaIdx)

            If idx < 0 OrElse idx >= steps Then
                Throw New ArgumentOutOfRangeException(NameOf(lambdaIdx),
                    $"lambda index {lambdaIdx} is out of range [0, {steps - 1}]!")
            End If

            Return idx
        End Function

        ''' <summary>取出指定 lambda 步骤的 p 维系数向量（**无截距**）</summary>
        Public Function getCoefficients(lambdaIdx As Integer) As Double()
            Return CType(coefficients(ResolveIndex(lambdaIdx)).Clone(), Double())
        End Function

        ''' <summary>取出指定 lambda 步骤各变量的风险比 HR = exp(beta)</summary>
        Public Function getHazardRatios(lambdaIdx As Integer) As Double()
            Return CType(hazardRatios(ResolveIndex(lambdaIdx)).Clone(), Double())
        End Function

        ''' <summary>
        ''' 预测线性预测子（风险得分对数）``x'beta``，返回每个样本一个值。
        ''' 得分越高代表风险越大。
        ''' </summary>
        Public Function Predict(newx As Double()(), Optional lambdaIdx As Integer = -1) As Double()
            Dim idx As Integer = ResolveIndex(lambdaIdx)
            Dim p As Integer = NumberOfFeatures

            If newx Is Nothing Then
                Throw New ArgumentNullException(NameOf(newx))
            End If
            If newx.Length = 0 Then
                Throw New ArgumentException("the prediction requires a non-empty feature matrix!", NameOf(newx))
            End If

            Dim coef As Double() = coefficients(idx)
            Dim scores As Double() = New Double(newx.Length - 1) {}

            For i As Integer = 0 To newx.Length - 1
                Dim row As Double() = newx(i)

                If row Is Nothing OrElse row.Length < p Then
                    Throw New ArgumentException($"the prediction row {i} has {If(row Is Nothing, 0, row.Length)} features, but the model requires {p}!", NameOf(newx))
                End If

                Dim score As Double = 0.0

                For j As Integer = 0 To p - 1
                    If Not Double.IsFinite(row(j)) Then
                        Throw New ArgumentException($"the prediction value newx({i})({j}) must be a finite number!", NameOf(newx))
                    End If

                    score += row(j) * coef(j)
                Next

                scores(i) = score
            Next

            Return scores
        End Function

        ''' <summary>预测单个样本的风险得分</summary>
        Public Function PredictSingle(x As Double(), Optional lambdaIdx As Integer = -1) As Double
            Dim rows As Double()() = New Double(0)() {}
            rows(0) = x
            Return Predict(rows, lambdaIdx)(0)
        End Function

        ''' <summary>
        ''' 多 lambda 预测：返回 ``result(step)(sample)``，
        ''' 对应 readme 中 ``predict(fit, newx)`` 的 N x L 矩阵（此处转置为按步骤优先）。
        ''' </summary>
        Public Function PredictPath(newx As Double()(), lambdaIdx As Integer()) As Double()()
            Dim steps As Integer

            If lambdaIdx Is Nothing Then
                steps = lambdas.Length
                lambdaIdx = New Integer(steps - 1) {}

                For li As Integer = 0 To steps - 1
                    lambdaIdx(li) = li
                Next
            Else
                steps = lambdaIdx.Length
            End If

            If steps = 0 Then
                Throw New ArgumentException("at least one lambda index is required for a path prediction!", NameOf(lambdaIdx))
            End If

            Dim result As Double()() = New Double(steps - 1)() {}

            For li As Integer = 0 To steps - 1
                result(li) = Predict(newx, lambdaIdx(li))
            Next

            Return result
        End Function

        ''' <summary>
        ''' 在给定的新数据上计算 Harrell C 指数（用于评估判别能力）。
        ''' </summary>
        Public Function CIndexFor(newx As Double()(),
                                  observedTime As Double(),
                                  eventStatus As Double(),
                                  Optional lambdaIdx As Integer = -1) As Double
            Return CoxFitting.ConcordanceIndex(observedTime, eventStatus, Predict(newx, lambdaIdx))
        End Function

        ''' <summary>
        ''' 用 Breslow 估计量计算基线累积风险 ``H0(t) = sum d_k / sum_{j in R(t_k)} exp(x_j'beta)``，
        ''' 并把协变量取 0 的「基线个体」生存曲线一并返回。
        ''' </summary>
        Public Function BaselineHazard(Optional lambdaIdx As Integer = -1) As SurvivalCurve
            Dim idx As Integer = ResolveIndex(lambdaIdx)
            Dim coef As Double() = coefficients(idx)
            Dim p As Integer = NumberOfFeatures
            Dim n As Integer = TrainingX.Length
            Dim relativeRisk As Double() = New Double(n - 1) {}

            For i As Integer = 0 To n - 1
                Dim score As Double = 0.0

                For j As Integer = 0 To p - 1
                    score += TrainingX(i)(j) * coef(j)
                Next

                relativeRisk(i) = std.Exp(std.Max(-50.0, std.Min(50.0, score)))
            Next

            Dim groups As CoxGroups = CoxFitting.BuildGroups(Time, Status)
            Dim eventTimes As New List(Of Double)()
            Dim increments As New List(Of Double)()
            Dim s0 As Double = 0.0

            ' 风险集必须按时间降序累加，但每个时刻的增量属于「该时刻」；
            ' 先记录降序增量，再反转成升序做累加，才是 H0(t) = sum_{t_k <= t} d_k/S0(t_k)
            For g As Integer = 0 To groups.GroupCount - 1
                For Each sampleIndex As Integer In groups.Members(g)
                    s0 += relativeRisk(sampleIndex)
                Next

                Dim d As Integer = groups.EventCounts(g)

                If d > 0 Then
                    Call eventTimes.Add(groups.Times(g))
                    Call increments.Add(CDbl(d) / std.Max(s0, 0.000000000001))
                End If
            Next

            Dim m As Integer = eventTimes.Count
            Dim timesAsc As Double() = New Double(m - 1) {}
            Dim hazardAsc As Double() = New Double(m - 1) {}
            Dim baselineAsc As Double() = New Double(m - 1) {}
            Dim h0 As Double = 0.0

            For q As Integer = 0 To m - 1
                Dim source As Integer = m - 1 - q

                timesAsc(q) = eventTimes(source)
                h0 += increments(source)
                hazardAsc(q) = h0
                baselineAsc(q) = std.Exp(-h0)
            Next

            Return New SurvivalCurve With {
                .Time = timesAsc,
                .CumulativeHazard = hazardAsc,
                .Survival = baselineAsc,
                .BaselineSurvival = baselineAsc,
                .RiskScore = 0.0,
                .SampleIndex = -1
            }
        End Function

        ''' <summary>
        ''' 个体化生存曲线：``S(t|x) = exp(-H0(t) * exp(x'beta))``，
        ''' 每个新样本返回一条曲线（对应 readme 中 ``survfit(fit, newx=...)``）。
        ''' </summary>
        Public Function PredictSurvival(newx As Double()(), Optional lambdaIdx As Integer = -1) As SurvivalCurve()
            Dim baseline As SurvivalCurve = BaselineHazard(lambdaIdx)
            Dim scores As Double() = Predict(newx, lambdaIdx)
            Dim curves As SurvivalCurve() = New SurvivalCurve(newx.Length - 1) {}
            Dim m As Integer = baseline.Time.Length

            For i As Integer = 0 To newx.Length - 1
                Dim relativeRisk As Double = std.Exp(std.Max(-50.0, std.Min(50.0, scores(i))))
                Dim hazard As Double() = New Double(m - 1) {}
                Dim survival As Double() = New Double(m - 1) {}

                For q As Integer = 0 To m - 1
                    hazard(q) = baseline.CumulativeHazard(q) * relativeRisk
                    survival(q) = std.Exp(-hazard(q))
                Next

                curves(i) = New SurvivalCurve With {
                    .Time = CType(baseline.Time.Clone(), Double()),
                    .CumulativeHazard = hazard,
                    .Survival = survival,
                    .BaselineSurvival = baseline.BaselineSurvival,
                    .RiskScore = scores(i),
                    .SampleIndex = i
                }
            Next

            Return curves
        End Function

        ''' <summary>取得指定 lambda 步骤的 <see cref="IFitted"/> 风险得分视图</summary>
        Public Function GetRiskModel(lambdaIdx As Integer) As IFitted
            Return New CoxRiskModel(Me, ResolveIndex(lambdaIdx))
        End Function

        ''' <summary>
        ''' 导出系数路径表：每行一个 lambda 步骤，每列一个协变量系数，
        ''' 标签列携带 ``lambdas`` / ``df`` / ``deviance`` / ``devRatio`` / ``cIndex``。
        ''' </summary>
        Public Function toNumericTable() As NumericTable
            Dim steps As Integer = lambdas.Length
            Dim p As Integer = NumberOfFeatures
            Dim names As String() = featureNames

            If names Is Nothing OrElse names.Length <> p Then
                names = CoxFitting.DefaultNames(p)
            End If

            Dim matrix As Double()() = RectangularArray.Matrix(Of Double)(steps, p)
            Dim rowNames As String() = New String(steps - 1) {}

            For li As Integer = 0 To steps - 1
                rowNames(li) = $"lambda_{li + 1}"

                For j As Integer = 0 To p - 1
                    matrix(li)(j) = coefficients(li)(j)
                Next
            Next

            Dim table As New NumericTable(matrix, rowNames, names)

            Call table.SetLabel("lambdas", CType(lambdas.Clone(), Double()))
            Call table.SetLabel("df", df.Select(Function(v) CDbl(v)).ToArray())
            Call table.SetLabel("deviance", CType(deviance.Clone(), Double()))
            Call table.SetLabel("devRatio", CType(devRatio.Clone(), Double()))
            Call table.SetLabel("cIndex", CType(cIndex.Clone(), Double()))

            Return table
        End Function

        ''' <summary><see cref="IFitted.R2"/> 的 Cox 语义：路径末端的伪 R²（基于偏似然偏差）</summary>
        Public ReadOnly Property R2 As Double Implements IFitted.R2
            Get
                Return devRatio(lambdas.Length - 1)
            End Get
        End Property

        ''' <summary>无截距的多元线性多项式，其值为风险得分 ``x'beta``</summary>
        Public ReadOnly Property Polynomial As Formula Implements IFitted.Polynomial
            Get
                Return New MultivariatePolynomial With {
                    .Factors = getCoefficients(-1)
                }
            End Get
        End Property

        ''' <summary>生存数据没有逐点残差，始终返回 Nothing</summary>
        Public Property ErrorTest As IFitError() Implements IFitted.ErrorTest
            Get
                Return Nothing
            End Get
            Set(value As IFitError())
                ' 刻意为 no-op：Cox 模型不产生可比较的 (观测值, 拟合值) 残差
            End Set
        End Property

        ''' <summary>路径末端模型的风险得分（线性预测子）</summary>
        Public Function GetY(ParamArray x() As Double) As Double Implements IFitted.GetY
            Return PredictSingle(x, -1)
        End Function

        Public Overrides Function ToString() As String
            Dim last As Integer = lambdas.Length - 1
            Return $"Cox({ties}, alpha={alpha:F2}, events={eventCount}/{eventCount + censoredCount}): {lambdas.Length} lambda steps, lambda=[{lambdas(0):G4}, {lambdas(last):G4}], df(last)={df(last)}, C(last)={cIndex(last):F4}"
        End Function
    End Class

    ''' <summary>
    ''' 指定 lambda 步骤的 <see cref="IFitted"/> 风险得分视图，
    ''' 便于把 Cox 的风险得分写回二维表或复用既有的单响应链路。
    ''' </summary>
    Public Class CoxRiskModel : Implements IFitted

        Private ReadOnly owner As CoxFit
        Private ReadOnly stepIndex As Integer
        Private ReadOnly factors As Double()

        Friend Sub New(model As CoxFit, lambdaIdx As Integer)
            owner = model
            stepIndex = lambdaIdx
            factors = model.getCoefficients(lambdaIdx)
        End Sub

        ''' <summary>该步骤的伪 R²</summary>
        Public ReadOnly Property R2 As Double Implements IFitted.R2
            Get
                Return owner.devRatio(stepIndex)
            End Get
        End Property

        ''' <summary>该步骤的 Harrell C 指数</summary>
        Public ReadOnly Property ConcordanceIndex As Double
            Get
                Return owner.cIndex(stepIndex)
            End Get
        End Property

        ''' <summary>无截距的风险得分多项式</summary>
        Public ReadOnly Property Polynomial As Formula Implements IFitted.Polynomial
            Get
                Return New MultivariatePolynomial With {
                    .Factors = CType(factors.Clone(), Double())
                }
            End Get
        End Property

        ''' <summary>生存数据没有逐点残差，始终返回 Nothing</summary>
        Public Property ErrorTest As IFitError() Implements IFitted.ErrorTest
            Get
                Return Nothing
            End Get
            Set(value As IFitError())
                ' no-op
            End Set
        End Property

        ''' <summary>风险得分 ``x'beta``</summary>
        Public Function GetY(ParamArray x() As Double) As Double Implements IFitted.GetY
            If x Is Nothing Then
                Throw New ArgumentNullException(NameOf(x))
            End If
            If x.Length < factors.Length Then
                Throw New ArgumentException($"the cox model requires {factors.Length} feature values, but only {x.Length} were given!", NameOf(x))
            End If

            Dim score As Double = 0.0

            For j As Integer = 0 To factors.Length - 1
                score += factors(j) * x(j)
            Next

            Return score
        End Function

        ''' <summary>该视图对应的 lambda 步骤下标</summary>
        Public ReadOnly Property LambdaIndex As Integer
            Get
                Return stepIndex
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return $"CoxRiskModel[lambda_{stepIndex + 1}] @ C={ConcordanceIndex:F4}, devRatio={R2:F4}"
        End Function
    End Class
End Namespace
