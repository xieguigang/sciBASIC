Imports std = System.Math

''' <summary>
''' 与 R <c>glmnet()</c> / <c>cv.glmnet()</c> 对齐的统一参数对象。
''' </summary>
''' <remarks>
''' <para>
''' 参数名刻意保持 R 端语义：<c>Alpha</c> 是 L1/L2 混合比例（1 = lasso），
''' <c>Lambda</c> 是正则化强度序列；这与既有 <c>ElasticNet</c> 程序集中
''' "alpha = 总强度、l1Ratio = 混合比例" 的 sklearn 约定正好相反，
''' 适配层负责换算，调用方不应混用两套语义。
''' </para>
''' <para>
''' 末尾的 <c>CV*</c> 属性只在 <c>cv.glmnet</c> 中使用；单次拟合时会被忽略。
''' </para>
''' </remarks>
Public NotInheritable Class GlmnetOptions

    ''' <summary>弹性网混合参数，取值 [0,1]；1 = 纯 lasso（默认），0 = 纯 ridge。</summary>
    Public Property Alpha As Double = GlmnetControl.DefaultAlpha

    ''' <summary>自定义 lambda 网格（严格递减、全为正）；为空时自动生成。</summary>
    Public Property Lambda As Double() = Nothing

    ''' <summary>自动 lambda 网格的步数，默认 100。</summary>
    Public Property NLambda As Integer = GlmnetControl.DefaultNLambda

    ''' <summary>
    ''' 网格终点比例；负数（默认 -1）表示按 n 与 p 自动选择 0.0001 / 0.01。
    ''' </summary>
    Public Property LambdaMinRatio As Double = -1.0R

    ''' <summary>逐样本观测权重（正数）；为空表示全部为 1。</summary>
    Public Property Weights As Double() = Nothing

    ''' <summary>
    ''' 直接加在 link scale 上的偏移项，形状为 <c>n x k</c>；
    ''' 单响应 family 只需要 1 列。为空表示无偏移。
    ''' </summary>
    Public Property Offset As Double()() = Nothing

    ''' <summary>是否把特征列标准化为加权单位方差（默认 True）。</summary>
    Public Property Standardize As Boolean = True

    ''' <summary>是否拟合不参与惩罚的独立截距（默认 True）。</summary>
    Public Property Intercept As Boolean = True

    ''' <summary>
    ''' 变量级惩罚乘子，长度 p、非负；0 表示永不惩罚（始终保留）。
    ''' 内部会重缩放至和为 p，与 glmnet 一致。
    ''' </summary>
    Public Property PenaltyFactor As Double() = Nothing

    ''' <summary>系数的逐变量下界（长度 p）。</summary>
    Public Property LowerLimits As Double() = Nothing

    ''' <summary>系数的逐变量上界（长度 p）。</summary>
    Public Property UpperLimits As Double() = Nothing

    ''' <summary>永不进入模型的变量下标（glmnet 的 <c>exclude</c>）。</summary>
    Public Property Exclude As Integer() = Nothing

    ''' <summary>路径中允许的最大非零变量数；-1 表示 p+1。</summary>
    Public Property DfMax As Integer = -1

    ''' <summary>活跃变量数上限；-1 表示 min(dfmax*2+20, p)。</summary>
    Public Property PMax As Integer = -1

    ''' <summary>目标函数相对改进的收敛阈值，默认 1e-7。</summary>
    Public Property Thresh As Double = GlmnetControl.DefaultThresh

    ''' <summary>单个 lambda 步骤的最大坐标下降轮数。</summary>
    Public Property MaxIt As Integer = GlmnetControl.DefaultMaxIt

    ''' <summary><c>type.gaussian</c>：协方差更新 / 朴素残差更新。</summary>
    Public Property TypeGaussian As GaussianAlgorithm = GaussianAlgorithm.Covariance

    ''' <summary><c>type.multinomial</c>：分组 / 非分组惩罚。glmnet 5.1 默认 ungrouped。</summary>
    Public Property TypeMultinomial As MultinomialGrouping = MultinomialGrouping.Ungrouped

    ''' <summary><c>type.logistic</c>：二项式 IRLS 的工作权重形式。</summary>
    Public Property TypeLogistic As LogisticAlgorithm = LogisticAlgorithm.Newton

    ''' <summary>仅 mgaussian 有效：是否把响应各列标准化。</summary>
    Public Property StandardizeResponse As Boolean = False

    ''' <summary>仅 cox 有效：平局处理方式，glmnet 5.1 默认 Efron。</summary>
    Public Property CoxTies As CoxTiesMode = CoxTiesMode.Efron

    ''' <summary>是否计算 relaxed lasso（gamma 网格上的无惩罚重拟合）。</summary>
    Public Property Relax As Boolean = False

    ''' <summary>relaxed lasso 的 gamma 网格；为空时使用 {0, 0.25, 0.5, 0.75, 1}。</summary>
    Public Property Gamma As Double() = Nothing

    ''' <summary>特征列名。</summary>
    Public Property FeatureNames As String() = Nothing

    ''' <summary>响应列名（mgaussian / multinomial）。</summary>
    Public Property ResponseNames As String() = Nothing

    ''' <summary><c>cv.glmnet</c> 折数，默认 10。</summary>
    Public Property NFolds As Integer = GlmnetControl.DefaultNFolds

    ''' <summary>自定义折号（长度 n、取值 [0, nfolds-1]）；为空时按种子确定性生成。</summary>
    Public Property FoldId As Integer() = Nothing

    ''' <summary>折外评估指标；为空时按 family 自动选择。</summary>
    Public Property TypeMeasure As CVMeasure? = Nothing

    ''' <summary>cox CV 是否使用"全数据减训练子集"的分组偏差算法（默认 True）。</summary>
    Public Property Grouped As Boolean = True

    ''' <summary>是否保留各折的折外预测矩阵。</summary>
    Public Property Keep As Boolean = False

    ''' <summary>确定性分折的随机种子。</summary>
    Public Property Seed As Integer = GlmnetControl.DefaultSeed

    ''' <summary>进度输出级别；库内默认静默。</summary>
    Public Property TraceIt As Integer = 0

    ''' <summary>
    ''' 创建一份浅拷贝。CV 的每一折会替换权重 / offset / lambda 等字段，
    ''' 因此必须先复制参数对象，避免污染调用方持有的实例。
    ''' </summary>
    Public Function Clone() As GlmnetOptions
        Return DirectCast(MemberwiseClone(), GlmnetOptions)
    End Function

    ''' <summary>
    ''' 解析出实际使用的 <c>lambda.min.ratio</c>。
    ''' </summary>
    Public Function ResolveLambdaMinRatio(n As Integer, p As Integer) As Double
        If LambdaMinRatio > 0.0R Then
            Return LambdaMinRatio
        End If

        Return GlmnetControl.ResolveLambdaMinRatio(n, p)
    End Function

    ''' <summary>
    ''' 解析出实际使用的评估指标（与 R 端各 family 的默认 type.measure 对齐）。
    ''' </summary>
    Public Function ResolveMeasure(family As FamilyKind) As CVMeasure
        If TypeMeasure.HasValue Then
            Return TypeMeasure.Value
        End If

        Select Case family
            Case FamilyKind.Binomial, FamilyKind.Poisson, FamilyKind.Multinomial
                Return CVMeasure.Deviance
            Case FamilyKind.Cox
                Return CVMeasure.Deviance
            Case Else
                Return CVMeasure.Mse
        End Select
    End Function

    ''' <summary>
    ''' 解析 relaxed lasso 的 gamma 网格。
    ''' </summary>
    Public Function ResolveGamma() As Double()
        If Gamma IsNot Nothing AndAlso Gamma.Length > 0 Then
            Return Gamma
        End If

        Dim steps As Integer = GlmnetControl.DefaultGammaSteps
        Dim grid As Double() = New Double(steps - 1) {}

        For i As Integer = 0 To steps - 1
            grid(i) = i / CDbl(steps - 1)
        Next

        Return grid
    End Function

    ''' <summary>
    ''' 校验全部参数；参数不合法时抛出 <see cref="ArgumentException"/>。
    ''' </summary>
    ''' <param name="n">样本数</param>
    ''' <param name="p">特征数</param>
    ''' <param name="k">响应列数</param>
    ''' <param name="family">模型家族</param>
    ''' <param name="forCrossValidation">是否同时校验 CV 专属参数</param>
    Public Sub Validate(n As Integer, p As Integer, k As Integer, family As FamilyKind, Optional forCrossValidation As Boolean = False)
        If Not Double.IsFinite(Alpha) OrElse Alpha < 0.0R OrElse Alpha > 1.0R Then
            Throw New ArgumentException($"alpha must be in [0,1], but {Alpha} was given!", NameOf(Alpha))
        End If
        If Not Double.IsFinite(Thresh) OrElse Thresh <= 0.0R Then
            Throw New ArgumentException($"thresh must be a positive number, but {Thresh} was given!", NameOf(Thresh))
        End If
        If MaxIt < 1 Then
            Throw New ArgumentException($"maxit must be a positive integer, but {MaxIt} was given!", NameOf(MaxIt))
        End If
        If LambdaMinRatio > 0.0R AndAlso (LambdaMinRatio >= 1.0R OrElse Not Double.IsFinite(LambdaMinRatio)) Then
            Throw New ArgumentException($"lambda.min.ratio must be in (0,1), but {LambdaMinRatio} was given!", NameOf(LambdaMinRatio))
        End If
        If Lambda Is Nothing AndAlso NLambda < 1 Then
            Throw New ArgumentException($"nlambda must be positive, but {NLambda} was given!", NameOf(NLambda))
        End If

        If Lambda IsNot Nothing Then
            If Lambda.Length = 0 Then
                Throw New ArgumentException("the custom lambda grid must not be empty!", NameOf(Lambda))
            End If

            For i As Integer = 0 To Lambda.Length - 1
                If Not Double.IsFinite(Lambda(i)) OrElse Lambda(i) <= 0.0R Then
                    Throw New ArgumentException($"lambda({i}) must be a finite positive number!", NameOf(Lambda))
                End If
                If i > 0 AndAlso Lambda(i) >= Lambda(i - 1) Then
                    Throw New ArgumentException("the custom lambda grid must be strictly decreasing!", NameOf(Lambda))
                End If
            Next
        End If

        Call ValidateVector(Weights, n, NameOf(Weights), requirePositive:=True)
        Call ValidateVector(PenaltyFactor, p, NameOf(PenaltyFactor), requirePositive:=False)
        Call ValidateVector(LowerLimits, p, NameOf(LowerLimits), requirePositive:=False)
        Call ValidateVector(UpperLimits, p, NameOf(UpperLimits), requirePositive:=False)

        If LowerLimits IsNot Nothing AndAlso UpperLimits IsNot Nothing Then
            For j As Integer = 0 To p - 1
                If LowerLimits(j) > UpperLimits(j) Then
                    Throw New ArgumentException($"lower.limits({j}) > upper.limits({j})!", NameOf(LowerLimits))
                End If
            Next
        End If
        If PenaltyFactor IsNot Nothing Then
            Dim anyPositive As Boolean = False

            For j As Integer = 0 To p - 1
                If PenaltyFactor(j) > 0.0R Then
                    anyPositive = True
                End If
            Next
            If Not anyPositive Then
                Throw New ArgumentException("at least one penalty.factor must be positive!", NameOf(PenaltyFactor))
            End If
        End If

        If Exclude IsNot Nothing Then
            For Each j As Integer In Exclude
                If j < 0 OrElse j >= p Then
                    Throw New ArgumentException($"exclude index {j} is out of range [0, {p - 1}]!", NameOf(Exclude))
                End If
            Next
        End If

        If Offset IsNot Nothing Then
            If Offset.Length <> n Then
                Throw New ArgumentException($"offset has {Offset.Length} rows but the design matrix has {n}!", NameOf(Offset))
            End If

            For i As Integer = 0 To n - 1
                If Offset(i) Is Nothing OrElse Offset(i).Length = 0 Then
                    Throw New ArgumentException($"offset row {i} is empty!", NameOf(Offset))
                End If
                For c As Integer = 0 To Offset(i).Length - 1
                    If Not Double.IsFinite(Offset(i)(c)) Then
                        Throw New ArgumentException($"offset({i})({c}) must be finite!", NameOf(Offset))
                    End If
                Next
            Next
        End If

        If FeatureNames IsNot Nothing AndAlso FeatureNames.Length <> p Then
            Throw New ArgumentException($"featureNames has {FeatureNames.Length} items but p = {p}!", NameOf(FeatureNames))
        End If
        If ResponseNames IsNot Nothing AndAlso ResponseNames.Length <> k Then
            Throw New ArgumentException($"responseNames has {ResponseNames.Length} items but k = {k}!", NameOf(ResponseNames))
        End If

        If StandardizeResponse AndAlso family <> FamilyKind.MGaussian Then
            Throw New ArgumentException("standardize.response is only supported for family='mgaussian'!", NameOf(StandardizeResponse))
        End If

        If forCrossValidation Then
            If NFolds < 2 OrElse NFolds > n Then
                Throw New ArgumentException($"nfolds must be in [2, {n}], but {NFolds} was given!", NameOf(NFolds))
            End If
            If FoldId IsNot Nothing Then
                If FoldId.Length <> n Then
                    Throw New ArgumentException($"foldid has {FoldId.Length} items but n = {n}!", NameOf(FoldId))
                End If
                For i As Integer = 0 To n - 1
                    If FoldId(i) < 0 OrElse FoldId(i) >= NFolds Then
                        Throw New ArgumentException($"foldid({i}) = {FoldId(i)} is out of range [0, {NFolds - 1}]!", NameOf(FoldId))
                    End If
                Next
            End If
        End If
    End Sub

    ''' <summary>校验一个长度应为 <paramref name="expected"/> 的可选向量。</summary>
    Private Sub ValidateVector(v As Double(), expected As Integer, name As String, requirePositive As Boolean)
        If v Is Nothing Then
            Return
        End If
        If v.Length <> expected Then
            Throw New ArgumentException($"{name} has {v.Length} items but {expected} were expected!", name)
        End If

        For i As Integer = 0 To v.Length - 1
            If Not Double.IsFinite(v(i)) Then
                Throw New ArgumentException($"{name}({i}) must be a finite number!", name)
            End If
            If requirePositive AndAlso v(i) <= 0.0R Then
                Throw New ArgumentException($"{name}({i}) must be strictly positive!", name)
            End If
            If Not requirePositive AndAlso name = NameOf(PenaltyFactor) AndAlso v(i) < 0.0R Then
                Throw New ArgumentException($"{name}({i}) must be non-negative!", name)
            End If
        Next
    End Sub

End Class
