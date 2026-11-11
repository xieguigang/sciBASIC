Imports std = System.Math

''' <summary>
''' glmnet 5.1 的全局数值控制与默认常量（对应 R 端 <c>glmnet.control()</c>）。
''' </summary>
''' <remarks>
''' 本模块集中所有"魔法数"，family 求解器不得各自硬编码默认值；
''' 这样对拍 R 时只需在一处校准。
''' </remarks>
Public Module GlmnetControl

    ''' <summary><c>alpha</c> 默认值：1 = 纯 lasso。</summary>
    Public Const DefaultAlpha As Double = 1.0R
    ''' <summary><c>nlambda</c> 默认值。</summary>
    Public Const DefaultNLambda As Integer = 100
    ''' <summary>n &gt;= p 时的 <c>lambda.min.ratio</c>。</summary>
    Public Const MinLambdaRatioLowDim As Double = 0.0001R
    ''' <summary>n &lt; p 时的 <c>lambda.min.ratio</c>。</summary>
    Public Const MinLambdaRatioHighDim As Double = 0.01R
    ''' <summary>目标函数相对改进的收敛阈值。</summary>
    Public Const DefaultThresh As Double = 0.0000001R
    ''' <summary>单个 lambda 步骤的最大坐标下降轮数。</summary>
    Public Const DefaultMaxIt As Integer = 100000
    ''' <summary>glmnet.control 的 <c>devmax</c>：最大允许 dev.ratio，超过则提前结束路径。</summary>
    Public Const DefaultDevMax As Double = 0.999R
    ''' <summary>glmnet.control 的 <c>eps</c>：判定系数为零的下界。</summary>
    Public Const DefaultEps As Double = 0.0000001R
    ''' <summary><c>cv.glmnet</c> 默认折数。</summary>
    Public Const DefaultNFolds As Integer = 10
    ''' <summary>确定性分折的默认随机种子（保证对拍可复现）。</summary>
    Public Const DefaultSeed As Integer = 1
    ''' <summary>relaxed lasso 默认的 gamma 网格。</summary>
    Public Const DefaultGammaMin As Double = 0.0R
    ''' <summary>relaxed lasso 默认 gamma 网格步数。</summary>
    Public Const DefaultGammaSteps As Integer = 5
    ''' <summary>线性预测子的钳制范围，防止 <c>exp</c> 溢出。</summary>
    Public Const MaxEta As Double = 700.0R
    ''' <summary>概率 / 期望计数的下界守卫。</summary>
    Public Const MinProbability As Double = 0.000000000001R
    ''' <summary>海塞矩阵与列曲率的下界守卫。</summary>
    Public Const MinCurvature As Double = 0.000000000001R

    ''' <summary>
    ''' 按样本量与变量数选择 <c>lambda.min.ratio</c>（glmnet 5.1 约定）。
    ''' </summary>
    Public Function ResolveLambdaMinRatio(n As Integer, p As Integer) As Double
        Return If(n < p, MinLambdaRatioHighDim, MinLambdaRatioLowDim)
    End Function

    ''' <summary>
    ''' 生成一条从 <paramref name="lambdaMax"/> 按对数等间隔下降到
    ''' <c>lambdaMax * ratio</c> 的 lambda 网格。
    ''' </summary>
    Public Function MakeLambdaGrid(lambdaMax As Double, steps As Integer, ratio As Double) As Double()
        If steps < 1 Then
            steps = 1
        End If
        If lambdaMax <= 0.0R OrElse Not Double.IsFinite(lambdaMax) Then
            Throw New ArgumentException("lambda.max must be a finite positive number!", NameOf(lambdaMax))
        End If
        If ratio <= 0.0R OrElse ratio >= 1.0R Then
            Throw New ArgumentException("lambda.min.ratio must be in (0, 1)!", NameOf(ratio))
        End If

        Dim grid As Double() = New Double(steps - 1) {}
        Dim exponent As Double = 1.0R / std.Max(1, steps - 1)

        For k As Integer = 0 To steps - 1
            grid(k) = lambdaMax * std.Pow(ratio, k * exponent)
        Next

        Return grid
    End Function

    ''' <summary>
    ''' 把 <paramref name="jerr"/> 映射为可打印的诊断文本（与 R 端 jerr 语义对齐）。
    ''' </summary>
    Public Function DescribeError(jerr As GlmnetError) As String
        Select Case jerr
            Case GlmnetError.None
                Return "no error"
            Case GlmnetError.InvalidInput
                Return "invalid input matrix or response"
            Case GlmnetError.InvalidOptions
                Return "invalid glmnet option"
            Case GlmnetError.DegenerateWeights
                Return "degenerate observation weights or response"
            Case GlmnetError.NonConvergence
                Return "coordinate descent failed to converge within maxit"
            Case GlmnetError.NumericalOverflow
                Return "numerical overflow in the IRLS/CD kernel"
            Case GlmnetError.UnsupportedOption
                Return "option is not supported by the requested family"
            Case GlmnetError.PathLimit
                Return "dfmax/pmax path limit reached"
            Case Else
                Return $"unknown error code {CInt(jerr)}"
        End Select
    End Function

End Module
