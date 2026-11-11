Imports Microsoft.VisualBasic.MachineLearning.GLMNet
Imports Microsoft.VisualBasic.Math.Statistics.ShapleyValue

''' <summary>
''' GLMNet（GNU R <c>glmnet</c> 的 VB.NET 复刻）模型的 SHAP 适配器。
''' 
''' 在固定 lambda 上，glmnet 的六个 family 都是 link scale 上的线性模型
''' ``eta = a0 + sum(beta_j * x_j)``，因此逐特征的精确 Shapley 值可以解析求解：
''' ``phi_j = beta_j * (x_j - E[x_j])``，基线值为 ``a0 + sum(beta_j * E[x_j])``。
''' 
''' 由于 <c>GlmnetFit</c> 的系数已经是原始量纲（与 R 的 <c>coef()</c> 直接对齐），
''' 列均值只需要在训练集的特征列上求平均即可，与 <see cref="LinearShapModel"/>
''' 使用完全相同的归因公式，因此这里直接复用 <see cref="LinearShapExplainer"/>。
''' </summary>
''' <remarks>
''' <para>
''' 解释目标所在的尺度随 family 变化：<c>gaussian</c> / <c>mgaussian</c> 为预测值，
''' <c>binomial</c> 为类别概率、<c>poisson</c> 为期望计数、<c>multinomial</c> 为
''' softmax 概率、<c>cox</c> 为风险得分。SHAP 的加性分解恒成立于 link scale，
''' 即 ``baseline + sum(phi) = eta``；响应尺度由 <see cref="Output"/> 给出。
''' </para>
''' <para>
''' 多响应模型（<c>multinomial</c> / <c>mgaussian</c>）的系数矩阵宽度为 K，
''' 需要通过 <see cref="ResponseIndex"/> 指定要解释的响应列（类别）。
''' </para>
''' </remarks>
Public Class GlmnetShapModel

    ''' <summary>
    ''' glmnet 的正则化路径拟合结果。
    ''' </summary>
    ''' <returns></returns>
    Public Property Fit As GlmnetFit

    ''' <summary>
    ''' 训练数据集（提供特征均值与全局重要性排名所需的特征名）。
    ''' </summary>
    ''' <returns></returns>
    Public Property Training As ShapDataset

    ''' <summary>
    ''' 需要解释的响应列下标。单响应 family 恒为 0；
    ''' <c>multinomial</c> / <c>mgaussian</c> 时对应系数矩阵的第 K 列，
    ''' 其顺序与 <see cref="GlmnetFit.classLabels"/> / <see cref="GlmnetFit.responseNames"/> 一致。
    ''' </summary>
    ''' <returns></returns>
    Public Property ResponseIndex As Integer = 0

    ''' <summary>
    ''' 选定的 lambda 步下标；小于 0 表示按 <see cref="Lambda"/> 在网格上插值。
    ''' </summary>
    ''' <returns></returns>
    Public Property LambdaIndex As Integer = -1

    ''' <summary>
    ''' 实际使用的 lambda 值。
    ''' </summary>
    ''' <returns></returns>
    Public Property Lambda As Double

    ''' <summary>
    ''' 模型家族。
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property Family As FamilyKind
        Get
            Return Fit.Family
        End Get
    End Property

    ''' <summary>
    ''' 是否为分类任务（<c>binomial</c> 或 <c>multinomial</c>）。
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property IsClassification As Boolean
        Get
            Return Family = FamilyKind.Binomial OrElse Family = FamilyKind.Multinomial
        End Get
    End Property

    ''' <summary>
    ''' 报告用的模型名字，例如 ``GLMNet[binomial]``。
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property Name As String
        Get
            Return $"GLMNet[{FamilyName(Family)}]"
        End Get
    End Property

    ''' <summary>
    ''' 被解释的响应列名字。多分类时即为类别标签。
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property ResponseName As String
        Get
            Dim names As String() = Fit.responseNames

            If names IsNot Nothing AndAlso ResponseIndex >= 0 AndAlso ResponseIndex < names.Length Then
                Return names(ResponseIndex)
            Else
                Return $"y{ResponseIndex + 1}"
            End If
        End Get
    End Property

    ''' <summary>
    ''' 指定 lambda 处的单响应回归系数（不包含截距），长度为 <see cref="GlmnetFit.nvars"/>。
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property Coefficients As Double()
        Get
            Dim matrix As Double()() = ResolveCoefficients()
            Dim beta As Double() = New Double(Fit.nvars - 1) {}

            For j As Integer = 0 To beta.Length - 1
                beta(j) = matrix(j + 1)(ResponseIndex)
            Next

            Return beta
        End Get
    End Property

    ''' <summary>
    ''' 指定 lambda 处的截距项。
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property Intercept As Double
        Get
            Return ResolveCoefficients()(0)(ResponseIndex)
        End Get
    End Property

    ''' <summary>
    ''' 可选的连接函数，用于把加性空间（link scale）的值变换为模型的原始输出。
    ''' 
    ''' <c>multinomial</c> 的 softmax 需要全部类别的系数，无法用一元函数表达，
    ''' 因此该 family 返回 <c>Nothing</c>，请使用 <see cref="Output"/>。
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property Link As Func(Of Double, Double)
        Get
            Select Case Family
                Case FamilyKind.Binomial
                    Return AddressOf LinearShapExplainer.Sigmoid
                Case FamilyKind.Poisson, FamilyKind.Cox
                    Return AddressOf SafeExp
                Case Else
                    ' gaussian / mgaussian：恒等连接
                    ' multinomial：softmax 依赖全部类别，无法表示为标量函数
                    Return Nothing
            End Select
        End Get
    End Property

    ''' <summary>
    ''' 在数据集之上拟合一条 glmnet 正则化路径并构建 SHAP 适配模型。
    ''' </summary>
    ''' <param name="dataset">训练数据集，标签列作为单列响应</param>
    ''' <param name="family">
    ''' R 风格的 family 名称（<c>gaussian</c> / <c>binomial</c> / <c>poisson</c> /
    ''' <c>multinomial</c> / <c>cox</c> / <c>mgaussian</c> 及其别名）。
    ''' 为空时按 <see cref="ShapDataset.IsCategorical"/> 推断 <c>binomial</c> 或 <c>gaussian</c>。
    ''' </param>
    ''' <param name="options">glmnet 参数；为空时使用缺省参数（lasso，100 步 lambda）</param>
    ''' <param name="lambdaIndex">需要解释的路径步下标；小于 0 时按 <paramref name="s"/> 或路径末端选取</param>
    ''' <param name="s">需要解释的 lambda 值（在网格上线性插值）</param>
    ''' <param name="responseIndex">多响应模型需要解释的响应列下标</param>
    ''' <returns></returns>
    ''' <remarks>
    ''' <c>cox</c> 需要 <c>(time, status)</c> 两列响应、<c>mgaussian</c> 需要 K 列响应，
    ''' 此时请使用接受响应矩阵的重载。
    ''' </remarks>
    Public Shared Function Train(dataset As ShapDataset,
                                 Optional family As String = Nothing,
                                 Optional options As GlmnetOptions = Nothing,
                                 Optional lambdaIndex As Integer = -1,
                                 Optional s As Double? = Nothing,
                                 Optional responseIndex As Integer = 0) As GlmnetShapModel

        Dim kind As FamilyKind = ResolveFamily(dataset, family)
        Dim y As Double()() = SingleColumnResponse(dataset, kind)

        Return FitPath(dataset, kind, y, options, lambdaIndex, s, responseIndex)
    End Function

    ''' <summary>
    ''' 使用显式响应矩阵拟合 glmnet 路径（供 <c>cox</c> 的 <c>(time, status)</c>
    ''' 与 <c>mgaussian</c> 的多列响应用）。
    ''' </summary>
    ''' <param name="dataset">训练数据集，仅提供特征矩阵</param>
    ''' <param name="family">R 风格的 family 名称，为空时按数据集推断</param>
    ''' <param name="y">响应矩阵，形状 <c>n x k</c></param>
    ''' <param name="options">glmnet 参数</param>
    ''' <param name="lambdaIndex">需要解释的路径步下标</param>
    ''' <param name="s">需要解释的 lambda 值</param>
    ''' <param name="responseIndex">需要解释的响应列下标</param>
    ''' <returns></returns>
    Public Shared Function Train(dataset As ShapDataset,
                                 family As String,
                                 y As Double()(),
                                 Optional options As GlmnetOptions = Nothing,
                                 Optional lambdaIndex As Integer = -1,
                                 Optional s As Double? = Nothing,
                                 Optional responseIndex As Integer = 0) As GlmnetShapModel

        Dim kind As FamilyKind = ResolveFamily(dataset, family)

        Return FitPath(dataset, kind, y, options, lambdaIndex, s, responseIndex)
    End Function

    ''' <summary>
    ''' 在数据集之上执行 <c>cv.glmnet</c> 交叉验证，并用选出的 lambda 构建 SHAP 适配模型。
    ''' </summary>
    ''' <param name="dataset">训练数据集</param>
    ''' <param name="family">R 风格的 family 名称</param>
    ''' <param name="options">glmnet 参数（<c>nfolds</c> / <c>type.measure</c> 等从此读取）</param>
    ''' <param name="s"><c>lambda.1se</c>（缺省）或者 <c>lambda.min</c></param>
    ''' <param name="responseIndex">多响应模型需要解释的响应列下标</param>
    ''' <returns></returns>
    Public Shared Function TrainCV(dataset As ShapDataset,
                                   Optional family As String = Nothing,
                                   Optional options As GlmnetOptions = Nothing,
                                   Optional s As String = "lambda.1se",
                                   Optional responseIndex As Integer = 0) As GlmnetShapModel

        Dim kind As FamilyKind = ResolveFamily(dataset, family)
        Dim y As Double()() = SingleColumnResponse(dataset, kind)

        Return FitCrossValidation(dataset, kind, y, options, s, responseIndex)
    End Function

    ''' <summary>
    ''' 使用显式响应矩阵执行 <c>cv.glmnet</c> 并构建 SHAP 适配模型。
    ''' </summary>
    ''' <param name="dataset">训练数据集，仅提供特征矩阵</param>
    ''' <param name="family">R 风格的 family 名称</param>
    ''' <param name="y">响应矩阵，形状 <c>n x k</c></param>
    ''' <param name="options">glmnet 参数</param>
    ''' <param name="s"><c>lambda.1se</c>（缺省）或者 <c>lambda.min</c></param>
    ''' <param name="responseIndex">需要解释的响应列下标</param>
    ''' <returns></returns>
    Public Shared Function TrainCV(dataset As ShapDataset,
                                   family As String,
                                   y As Double()(),
                                   Optional options As GlmnetOptions = Nothing,
                                   Optional s As String = "lambda.1se",
                                   Optional responseIndex As Integer = 0) As GlmnetShapModel

        Dim kind As FamilyKind = ResolveFamily(dataset, family)

        Return FitCrossValidation(dataset, kind, y, options, s, responseIndex)
    End Function

    ''' <summary>
    ''' 从已有的 <c>cv.glmnet</c> 结果构建 SHAP 适配模型。
    ''' </summary>
    ''' <param name="cvFit">交叉验证结果</param>
    ''' <param name="dataset">训练数据集，用于计算特征均值</param>
    ''' <param name="s"><c>lambda.1se</c>（缺省）或者 <c>lambda.min</c></param>
    ''' <param name="responseIndex">多响应模型需要解释的响应列下标</param>
    ''' <returns></returns>
    Public Shared Function FromCV(cvFit As GlmnetCVFit,
                                  dataset As ShapDataset,
                                  Optional s As String = "lambda.1se",
                                  Optional responseIndex As Integer = 0) As GlmnetShapModel

        If cvFit Is Nothing Then
            Throw New ArgumentNullException(NameOf(cvFit))
        End If

        Dim lambda As Double
        Dim index As Integer

        Select Case If(s, "lambda.1se").Trim().ToLowerInvariant()
            Case "lambda.1se"
                lambda = cvFit.lambda1se
                index = cvFit.index1se
            Case "lambda.min"
                lambda = cvFit.lambdaMin
                index = cvFit.indexMin
            Case Else
                Throw New ArgumentException($"unsupported lambda rule '{s}': only 'lambda.min' and 'lambda.1se' are supported!",
                                            NameOf(s))
        End Select

        Return Bind(cvFit.glmnetFit, dataset, index, lambda, responseIndex)
    End Function

    ''' <summary>
    ''' 对新样本进行预测，返回响应尺度的一列值（分类任务为概率）。
    ''' </summary>
    ''' <param name="features"></param>
    ''' <returns></returns>
    Public Function Predict(features As Double()()) As Double()
        Dim values As Double()() = GlmnetPredict.Predict(Fit, features, PredictionType.Response, Lambda).Values

        Return values _
            .Select(Function(row) row(If(row.Length > ResponseIndex, ResponseIndex, 0))) _
            .ToArray
    End Function

    ''' <summary>
    ''' 模型在单个样本上的原始输出（响应尺度）。
    ''' </summary>
    ''' <param name="x"></param>
    ''' <returns></returns>
    Public Function Output(x As Double()) As Double
        Return Predict(New Double()() {x})(0)
    End Function

    Public Function PredictResult() As ModelPrediction
        Return New ModelPrediction With {
            .ModelName = Name,
            .DatasetName = Training.Name,
            .IsClassification = IsClassification,
            .IDs = Training.IDs,
            .Labels = Training.Labels,
            .Predictions = Predict(Training.Features)
        }
    End Function

    ''' <summary>
    ''' 构建解析式 SHAP 解释器。
    ''' </summary>
    ''' <remarks>
    ''' 线性预测子在 link scale 上是加性的，因此 ``sum(Explain(x)) + Baseline`` 恒等于
    ''' ``a0 + sum(beta_j * x_j)``；与 <see cref="Output"/> 的差异只来自 family 的响应变换。
    ''' </remarks>
    ''' <returns></returns>
    Public Function BuildExplainer() As IShapExplainer
        Dim beta As Double() = Coefficients
        Dim means As Double() = ComputeFeatureMeans(Training.Features, beta.Length)
        Dim names As String() = Training.FeatureNames

        If names Is Nothing OrElse names.Length <> beta.Length Then
            names = Fit.featureNames
        End If

        Return New LinearShapExplainer(beta, means, names, Intercept, Link)
    End Function

    ''' <summary>
    ''' 取得当前 lambda 处的 <c>(p+1) x K</c> 系数矩阵。
    ''' </summary>
    ''' <returns></returns>
    Private Function ResolveCoefficients() As Double()()
        If LambdaIndex >= 0 Then
            Return Fit.GetCoefficients(LambdaIndex)
        Else
            Return Fit.GetCoefficients(Lambda)
        End If
    End Function

    ''' <summary>
    ''' 拟合一条正则化路径并绑定 lambda 选择。
    ''' </summary>
    Private Shared Function FitPath(dataset As ShapDataset,
                                    kind As FamilyKind,
                                    y As Double()(),
                                    options As GlmnetOptions,
                                    lambdaIndex As Integer,
                                    s As Double?,
                                    responseIndex As Integer) As GlmnetShapModel

        If dataset Is Nothing Then Throw New ArgumentNullException(NameOf(dataset))

        Dim fit As GlmnetFit = GlmnetSolver.Fit(dataset.Features, y, kind, PrepareOptions(dataset, options))

        Return Bind(fit, dataset, lambdaIndex, s, responseIndex)
    End Function

    ''' <summary>
    ''' 执行交叉验证并绑定选出的 lambda 步。
    ''' </summary>
    Private Shared Function FitCrossValidation(dataset As ShapDataset,
                                               kind As FamilyKind,
                                               y As Double()(),
                                               options As GlmnetOptions,
                                               s As String,
                                               responseIndex As Integer) As GlmnetShapModel

        If dataset Is Nothing Then Throw New ArgumentNullException(NameOf(dataset))

        ' 交叉验证至少要能分成两折，样本过少的时候直接给出明确的错误信息
        Dim opts As GlmnetOptions = PrepareOptions(dataset, options)

        If opts.NFolds > dataset.Size Then
            Throw New ArgumentException($"nfolds = {opts.NFolds} is greater than the sample size {dataset.Size}!",
                                        NameOf(options))
        End If

        Dim cvFit As GlmnetCVFit = GlmnetSolver.CrossValidate(dataset.Features, y, kind, opts)

        Return FromCV(cvFit, dataset, s, responseIndex)
    End Function

    ''' <summary>
    ''' 校验 lambda / 响应列选择并装配模型对象。
    ''' </summary>
    Private Shared Function Bind(fit As GlmnetFit,
                                 dataset As ShapDataset,
                                 lambdaIndex As Integer,
                                 s As Double?,
                                 responseIndex As Integer) As GlmnetShapModel

        If fit Is Nothing Then Throw New ArgumentNullException(NameOf(fit))
        If dataset Is Nothing Then Throw New ArgumentNullException(NameOf(dataset))

        If fit.Count = 0 Then
            Throw New InvalidOperationException("the glmnet path is empty, no lambda step can be explained!")
        End If
        If responseIndex < 0 OrElse responseIndex >= fit.nresponses Then
            Throw New ArgumentOutOfRangeException(NameOf(responseIndex),
                                                  $"responseIndex = {responseIndex} is out of range [0, {fit.nresponses - 1}] for family '{FamilyName(fit.Family)}'!")
        End If

        Dim index As Integer = lambdaIndex
        Dim lambda As Double

        If index >= 0 Then
            If index >= fit.Count Then
                Throw New ArgumentOutOfRangeException(NameOf(lambdaIndex),
                                                      $"lambdaIndex = {index} is out of range [0, {fit.Count - 1}]!")
            End If

            lambda = fit.lambda(index)
        ElseIf s.HasValue Then
            lambda = s.Value

            If Not Double.IsFinite(lambda) OrElse lambda <= 0.0R Then
                Throw New ArgumentOutOfRangeException(NameOf(s), $"lambda must be a finite positive number, but {lambda} was given!")
            End If
        Else
            ' 缺省解释路径末端（惩罚最弱）的模型
            index = fit.Count - 1
            lambda = fit.lambda(index)
        End If

        Return New GlmnetShapModel With {
            .Fit = fit,
            .Training = dataset,
            .ResponseIndex = responseIndex,
            .LambdaIndex = index,
            .Lambda = lambda
        }
    End Function

    ''' <summary>
    ''' 把数据集标签整理为 glmnet 期望的单列响应矩阵。
    ''' </summary>
    Private Shared Function SingleColumnResponse(dataset As ShapDataset, kind As FamilyKind) As Double()()
        If dataset Is Nothing Then Throw New ArgumentNullException(NameOf(dataset))

        Select Case kind
            Case FamilyKind.Cox
                Throw New ArgumentException(
                    "family 'cox' requires a two-column (time, status) response: use the Train(dataset, family, y, ...) overload instead!",
                    NameOf(kind))
            Case FamilyKind.MGaussian
                Throw New ArgumentException(
                    "family 'mgaussian' requires one column per response: use the Train(dataset, family, y, ...) overload instead!",
                    NameOf(kind))
        End Select

        Dim labels As Double() = dataset.Labels

        If labels Is Nothing OrElse labels.Length = 0 Then
            Throw New ArgumentException("the training dataset contains no sample!", NameOf(dataset))
        End If

        Dim y As Double()() = New Double(labels.Length - 1)() {}

        For i As Integer = 0 To labels.Length - 1
            y(i) = New Double() {labels(i)}
        Next

        Return y
    End Function

    ''' <summary>
    ''' 补齐参数对象的特征列名，并保持调用方持有的实例不被修改。
    ''' </summary>
    Private Shared Function PrepareOptions(dataset As ShapDataset, options As GlmnetOptions) As GlmnetOptions
        Dim opts As GlmnetOptions = If(options, New GlmnetOptions()).Clone()

        If opts.FeatureNames Is Nothing AndAlso dataset.FeatureNames IsNot Nothing Then
            opts.FeatureNames = dataset.FeatureNames
        End If

        Return opts
    End Function

    ''' <summary>
    ''' 解析 family：为空时按数据集推断，否则接受 R 风格的名称或别名。
    ''' </summary>
    Private Shared Function ResolveFamily(dataset As ShapDataset, family As String) As FamilyKind
        If String.IsNullOrEmpty(family) Then
            If dataset Is Nothing Then
                Throw New ArgumentNullException(NameOf(dataset))
            End If

            Return If(dataset.IsCategorical, FamilyKind.Binomial, FamilyKind.Gaussian)
        End If

        Select Case family.Trim().ToLowerInvariant()
            Case "gaussian", "elnet"
                Return FamilyKind.Gaussian
            Case "binomial", "lognet", "logistic"
                Return FamilyKind.Binomial
            Case "poisson", "fishnet"
                Return FamilyKind.Poisson
            Case "multinomial", "multnet"
                Return FamilyKind.Multinomial
            Case "cox", "coxnet"
                Return FamilyKind.Cox
            Case "mgaussian", "mrelnet"
                Return FamilyKind.MGaussian
            Case Else
                Throw New ArgumentException($"unsupported glmnet family: '{family}'!", NameOf(family))
        End Select
    End Function

    ''' <summary>
    ''' 返回 R 端使用的 family 名称。
    ''' </summary>
    ''' <remarks>
    ''' GLMNet 的 <c>FamilyRegistry</c> 是 <c>Friend</c> 成员，跨程序集不可见，
    ''' 因此这里保留一份只读的名称映射。
    ''' </remarks>
    Public Shared Function FamilyName(kind As FamilyKind) As String
        Select Case kind
            Case FamilyKind.Gaussian : Return "gaussian"
            Case FamilyKind.Binomial : Return "binomial"
            Case FamilyKind.Poisson : Return "poisson"
            Case FamilyKind.Multinomial : Return "multinomial"
            Case FamilyKind.Cox : Return "cox"
            Case FamilyKind.MGaussian : Return "mgaussian"
            Case Else : Return kind.ToString().ToLowerInvariant()
        End Select
    End Function

    ''' <summary>
    ''' 计算每个特征在训练集上的均值 ``E[x_j]``。
    ''' </summary>
    Private Shared Function ComputeFeatureMeans(features As Double()(), width As Integer) As Double()
        Dim means As Double() = New Double(width - 1) {}

        If features Is Nothing OrElse features.Length = 0 Then
            Return means
        End If

        For j As Integer = 0 To width - 1
            Dim sum As Double = 0

            For i As Integer = 0 To features.Length - 1
                If features(i) IsNot Nothing AndAlso j < features(i).Length Then
                    sum += features(i)(j)
                End If
            Next

            means(j) = sum / features.Length
        Next

        Return means
    End Function

    ''' <summary>
    ''' 数值安全的指数函数，避免极端 link 值导致溢出。
    ''' </summary>
    Private Shared Function SafeExp(z As Double) As Double
        If z > 700.0R Then
            Return Double.MaxValue
        ElseIf z < -700.0R Then
            Return 0.0R
        Else
            Return System.Math.Exp(z)
        End If
    End Function

End Class
