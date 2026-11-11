Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports std = System.Math

Namespace MGaussian

    ''' <summary>
    ''' 多响应高斯模型的预处理结果（全部在标准化空间之中）。
    ''' </summary>
    ''' <remarks>
    ''' 样本权重被归一化成总和为 1 的 ``Weight``，因此损失函数写作
    ''' ``0.5 * sum_i Weight(i) * ||residual_i||^2``，与 glmnet 的
    ''' ``1/(2N) * sum_i ...``（权重和为 N）等价。
    ''' </remarks>
    Friend NotInheritable Class MGaussianData

        ''' <summary>标准化后的特征矩阵 ``Design(sample)(feature)``</summary>
        Public Property Design As Double()()
        ''' <summary>标准化后的响应矩阵 ``Response(sample)(task)``</summary>
        Public Property Response As Double()()
        ''' <summary>归一化样本权重，总和为 1</summary>
        Public Property Weight As Double()
        ''' <summary>特征列均值（``intercept=False`` 时为 0）</summary>
        Public Property FeatureMeans As Double()
        ''' <summary>特征列尺度（``standardize=False`` 或常数列时为 1）</summary>
        Public Property FeatureScales As Double()
        ''' <summary>响应列均值（``intercept=False`` 时为 0）</summary>
        Public Property ResponseMeans As Double()
        ''' <summary>响应列尺度（``standardizeResponse=False`` 或常数列时为 1）</summary>
        Public Property ResponseScales As Double()
        ''' <summary>``ColumnScale(j) = sum_i Weight(i)*Design(i)(j)^2``，坐标下降的曲率</summary>
        Public Property ColumnScale As Double()
        ''' <summary>标准化空间之中的截距（不施加惩罚）</summary>
        Public Property BaseIntercept As Double()

        Public Property N As Integer
        Public Property P As Integer
        Public Property K As Integer
    End Class

    ''' <summary>
    ''' 多响应高斯模型（组-lasso）的求解器：块坐标下降 + lambda 正则化路径。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 优化目标（响应为 ``N x K`` 矩阵、系数 ``beta`` 为 ``p x K`` 矩阵）：
    ''' ``min 1/(2N) sum_i ||y_i - b0 - beta'x_i||^2
    '''      + lambda * [(1-alpha)*||beta||_F^2/2 + alpha*sum_j ||beta_j||_2]``。
    ''' ``beta_j`` 是第 j 个变量在 K 个响应上的系数向量；组-lasso 让它整体为零或整体非零，
    ''' 从而所有响应共享同一个变量支持集。``alpha=1`` 为纯组-lasso，``alpha=0`` 退化为
    ''' 对整个系数矩阵的岭型二次收缩（不会产生精确零），``0 &lt; alpha &lt; 1`` 为弹性网混合。
    ''' </para>
    ''' <para>
    ''' 记标准化空间中的残差矩阵为 ``R``，对变量 j 令
    ''' ``c_j = beta_j + (sum_i w_i z_ij R_i) / s_j``（``s_j = sum_i w_i z_ij^2``），
    ''' 则块坐标下降的闭式更新为
    ''' ``beta_j := (c_j / (1 + lambda*(1-alpha)/s_j)) * max(0, 1 - lambda*alpha/(s_j*||c_j||_2))``。
    ''' 每次只按 ``delta = beta_new - beta_old`` 增量更新残差，无需矩阵分解或逐元素乘法运算符。
    ''' </para>
    ''' <para>
    ''' lambda 路径从 ``lambda_max = max_j ||sum_i w_i z_ij y_i||_2 / alpha`` 出发按对数网格下降，
    ''' 上一步的系数作为下一步的初值（warm start）。对外输出的系数一律还原到原始量纲。
    ''' </para>
    ''' </remarks>
    Public Module MGaussianFitting

        ''' <summary>默认的 lambda 路径步数</summary>
        Public Const DefaultNumberOfLambdas As Integer = 100
        ''' <summary>默认的坐标下降最大轮数</summary>
        Public Const DefaultMaxIterations As Integer = 1000
        ''' <summary>默认的收敛容差</summary>
        Public Const DefaultTolerance As Double = 0.000001
        ''' <summary>n &gt;= p 时的 lambda.min.ratio（glmnet 约定）</summary>
        Public Const MinLambdaRatioLowDim As Double = 0.0001
        ''' <summary>n &lt; p 时的 lambda.min.ratio（glmnet 约定）</summary>
        Public Const MinLambdaRatioHighDim As Double = 0.01
        ''' <summary>系数被视为「非零变量组」的最小 2-norm</summary>
        Friend Const GroupEpsilon As Double = 0.0000000001

        ''' <summary>
        ''' 输入校验：返回样本数 n、特征数 p、响应数 k，供拟合与交叉验证共用。
        ''' </summary>
        Friend Sub ValidateData(x As Double()(),
                                y As Double()(),
                                alpha As Double,
                                numberOfLambdas As Integer,
                                maxIterations As Integer,
                                tolerance As Double,
                                lambda As Double(),
                                weights As Double(),
                                featureNames As String(),
                                responseNames As String(),
                                caller As String,
                                ByRef n As Integer,
                                ByRef p As Integer,
                                ByRef k As Integer)

            If x Is Nothing Then
                Throw New ArgumentNullException(NameOf(x))
            End If
            If y Is Nothing Then
                Throw New ArgumentNullException(NameOf(y))
            End If
            If x.Length = 0 OrElse x(0) Is Nothing OrElse x(0).Length = 0 Then
                Throw New ArgumentException($"the '{caller}' requires a non-empty feature matrix!", NameOf(x))
            End If
            If y.Length = 0 OrElse y(0) Is Nothing OrElse y(0).Length = 0 Then
                Throw New ArgumentException($"the '{caller}' requires a non-empty multi-response matrix!", NameOf(y))
            End If

            n = x.Length
            p = x(0).Length
            k = y(0).Length

            If y.Length <> n Then
                Throw New ArgumentException($"the feature matrix has {n} samples, but the response matrix has {y.Length} samples!", NameOf(y))
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
            If responseNames IsNot Nothing AndAlso responseNames.Length <> k Then
                Throw New ArgumentException($"the number of response names ({responseNames.Length}) is not matched with the number of response columns ({k})!", NameOf(responseNames))
            End If
            If weights IsNot Nothing AndAlso weights.Length <> n Then
                Throw New ArgumentException($"the size of the weight vector ({weights.Length}) is not matched with the sample size ({n})!", NameOf(weights))
            End If

            For i As Integer = 0 To n - 1
                If x(i) Is Nothing OrElse x(i).Length <> p Then
                    Throw New ArgumentException($"the row {i} of the feature matrix has {If(x(i) Is Nothing, 0, x(i).Length)} features, but the first row has {p}!", NameOf(x))
                End If
                If y(i) Is Nothing OrElse y(i).Length <> k Then
                    Throw New ArgumentException($"the row {i} of the response matrix has {If(y(i) Is Nothing, 0, y(i).Length)} responses, but the first row has {k}!", NameOf(y))
                End If

                For j As Integer = 0 To p - 1
                    If Not Double.IsFinite(x(i)(j)) Then
                        Throw New ArgumentException($"the feature value x({i})({j}) must be a finite number!", NameOf(x))
                    End If
                Next
                For c As Integer = 0 To k - 1
                    If Not Double.IsFinite(y(i)(c)) Then
                        Throw New ArgumentException($"the response value y({i})({c}) must be a finite number!", NameOf(y))
                    End If
                Next
                If weights IsNot Nothing AndAlso (Not Double.IsFinite(weights(i)) OrElse weights(i) <= 0.0) Then
                    Throw New ArgumentException($"the sample weight({i}) = {weights(i)} must be a finite positive number!", NameOf(weights))
                End If
            Next

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
        ''' 拟合多响应高斯模型，返回完整的 lambda 正则化路径。
        ''' </summary>
        ''' <param name="x">特征矩阵，``x(sample)(feature)``，共 n 行 p 列</param>
        ''' <param name="y">响应矩阵，``y(sample)(response)``，共 n 行 k 列</param>
        ''' <param name="lambda">
        ''' 自定义 lambda 网格（严格递减、全为正）。缺省时自动生成对数网格
        ''' ``lambda_max -&gt; lambda_max * ratio``。
        ''' </param>
        ''' <param name="alpha">弹性网混合参数，``1`` 为组-lasso、``0`` 为岭型二次收缩</param>
        ''' <param name="numberOfLambdas">自动网格的步数</param>
        ''' <param name="standardize">是否把特征列标准化为加权单位方差</param>
        ''' <param name="standardizeResponse">
        ''' mgaussian 特有参数：是否把响应各列标准化（均值 0、方差 1）。
        ''' 量纲差异悬殊时开启，避免某些响应主导损失函数；预测时会自动还原。
        ''' </param>
        ''' <param name="intercept">是否为每个响应拟合独立且不惩罚的截距</param>
        ''' <param name="weights">可选的逐样本权重（正数），内部归一化到总和为 1</param>
        ''' <param name="maxIterations">每个 lambda 步骤的坐标下降最大轮数</param>
        ''' <param name="tolerance">收敛容差（系数的最大相对变化量）</param>
        ''' <param name="featureNames">特征列名</param>
        ''' <param name="responseNames">响应列名</param>
        ''' <param name="Silent">保留参数，求解器不产生控制台输出</param>
        ''' <returns>含每一步系数矩阵、2-norm 路径、df 与训练误差的结果对象</returns>
        Public Function Fit(x As Double()(),
                            y As Double()(),
                            Optional lambda As Double() = Nothing,
                            Optional alpha As Double = 1.0,
                            Optional numberOfLambdas As Integer = DefaultNumberOfLambdas,
                            Optional standardize As Boolean = True,
                            Optional standardizeResponse As Boolean = False,
                            Optional intercept As Boolean = True,
                            Optional weights As Double() = Nothing,
                            Optional maxIterations As Integer = DefaultMaxIterations,
                            Optional tolerance As Double = DefaultTolerance,
                            Optional featureNames As String() = Nothing,
                            Optional responseNames As String() = Nothing,
                            Optional Silent As Boolean = False) As MGaussianFit

            Dim n As Integer = 0
            Dim p As Integer = 0
            Dim k As Integer = 0

            Call ValidateData(x, y, alpha, numberOfLambdas, maxIterations, tolerance,
                              lambda, weights, featureNames, responseNames,
                              NameOf(Fit), n, p, k)

            Dim data As MGaussianData = Prepare(x, y, weights, standardize,
                                                standardizeResponse, intercept, n, p, k)
            Dim grid As Double() = If(lambda, LambdaGrid(LambdaMax(data, alpha), numberOfLambdas, n, p))
            Dim fNames As String() = If(featureNames, DefaultNames(p, "X"))
            Dim rNames As String() = If(responseNames, DefaultNames(k, "Y"))

            Return SolvePath(data, grid, alpha, maxIterations, tolerance,
                             x, y, fNames, rNames,
                             standardize, standardizeResponse, intercept)
        End Function

        ''' <summary>生成 ``[1]..[m]`` 风格的名称（``X1..Xp`` / ``Y1..Yk``）</summary>
        Friend Function DefaultNames(m As Integer, prefix As String) As String()
            Dim names As String() = New String(m - 1) {}

            For j As Integer = 0 To m - 1
                names(j) = $"{prefix}{j + 1}"
            Next

            Return names
        End Function

        ''' <summary>
        ''' 预处理：权重归一化、特征中心化/标准化、响应中心化/标准化，并计算坐标曲率。
        ''' </summary>
        ''' <remarks>
        ''' 截距为真时同时中心化特征与响应（否则不中心化，以保证 ``intercept=False``
        ''' 的模型确实过原点）；标准化只改变尺度，常数列的尺度守卫为 1。
        ''' </remarks>
        Friend Function Prepare(x As Double()(),
                                y As Double()(),
                                weights As Double(),
                                standardize As Boolean,
                                standardizeResponse As Boolean,
                                fitIntercept As Boolean,
                                n As Integer, p As Integer, k As Integer) As MGaussianData

            Dim w As Double() = New Double(n - 1) {}
            Dim sumWeight As Double = 0.0

            If weights Is Nothing Then
                For i As Integer = 0 To n - 1
                    w(i) = 1.0
                Next
                sumWeight = CDbl(n)
            Else
                For i As Integer = 0 To n - 1
                    w(i) = weights(i)
                    sumWeight += weights(i)
                Next
            End If

            For i As Integer = 0 To n - 1
                w(i) /= sumWeight
            Next

            Dim fMeans As Double() = New Double(p - 1) {}
            Dim fScales As Double() = New Double(p - 1) {}

            For j As Integer = 0 To p - 1
                Dim mean As Double = 0.0

                If fitIntercept Then
                    For i As Integer = 0 To n - 1
                        mean += w(i) * x(i)(j)
                    Next
                End If

                Dim variance As Double = 0.0

                For i As Integer = 0 To n - 1
                    Dim d As Double = x(i)(j) - mean
                    variance += w(i) * d * d
                Next

                fMeans(j) = mean
                fScales(j) = If(standardize AndAlso variance > 0.0, std.Sqrt(variance), 1.0)
            Next

            Dim rMeans As Double() = New Double(k - 1) {}
            Dim rScales As Double() = New Double(k - 1) {}

            For c As Integer = 0 To k - 1
                Dim mean As Double = 0.0

                If fitIntercept Then
                    For i As Integer = 0 To n - 1
                        mean += w(i) * y(i)(c)
                    Next
                End If

                Dim variance As Double = 0.0

                For i As Integer = 0 To n - 1
                    Dim d As Double = y(i)(c) - mean
                    variance += w(i) * d * d
                Next

                rMeans(c) = mean
                rScales(c) = If(standardizeResponse AndAlso variance > 0.0, std.Sqrt(variance), 1.0)
            Next

            Dim design As Double()() = RectangularArray.Matrix(Of Double)(n, p)
            Dim response As Double()() = RectangularArray.Matrix(Of Double)(n, k)
            Dim columnScale As Double() = New Double(p - 1) {}

            For i As Integer = 0 To n - 1
                For j As Integer = 0 To p - 1
                    design(i)(j) = (x(i)(j) - fMeans(j)) / fScales(j)
                    columnScale(j) += w(i) * design(i)(j) * design(i)(j)
                Next
                For c As Integer = 0 To k - 1
                    response(i)(c) = (y(i)(c) - rMeans(c)) / rScales(c)
                Next
            Next

            ' 数值误差可能让常数列的曲率成为极小的正数，直接当成 0 跳过该变量
            For j As Integer = 0 To p - 1
                If columnScale(j) <= GroupEpsilon Then
                    columnScale(j) = 0.0
                End If
            Next

            Dim baseIntercept As Double() = New Double(k - 1) {}

            If fitIntercept Then
                For c As Integer = 0 To k - 1
                    For i As Integer = 0 To n - 1
                        baseIntercept(c) += w(i) * response(i)(c)
                    Next
                Next
            End If

            Return New MGaussianData With {
                .Design = design,
                .Response = response,
                .Weight = w,
                .FeatureMeans = fMeans,
                .FeatureScales = fScales,
                .ResponseMeans = rMeans,
                .ResponseScales = rScales,
                .ColumnScale = columnScale,
                .BaseIntercept = baseIntercept,
                .N = n,
                .P = p,
                .K = k
            }
        End Function

        ''' <summary>
        ''' KKT 条件给出的路径起点：``lambda_max = max_j ||sum_i w_i z_ij y_i||_2 / alpha``。
        ''' ``alpha=0`` 时无变量会被剔除，改用 ``alpha=1`` 计算一个有限的起点。
        ''' </summary>
        Friend Function LambdaMax(data As MGaussianData, alpha As Double) As Double
            Dim maxNorm As Double = 0.0
            Dim n As Integer = data.N
            Dim p As Integer = data.P
            Dim k As Integer = data.K

            For j As Integer = 0 To p - 1
                Dim gradient As Double() = New Double(k - 1) {}

                For i As Integer = 0 To n - 1
                    Dim wz As Double = data.Weight(i) * data.Design(i)(j)

                    For c As Integer = 0 To k - 1
                        gradient(c) += wz * data.Response(i)(c)
                    Next
                Next

                Dim norm As Double = 0.0

                For c As Integer = 0 To k - 1
                    norm += gradient(c) * gradient(c)
                Next

                maxNorm = std.Max(maxNorm, std.Sqrt(norm))
            Next

            If maxNorm <= 0.0 Then
                ' 全部响应都是常数：不存在有意义的收缩路径，给一个固定的有限起点
                Return 1.0
            End If

            Return maxNorm / If(alpha > 0.0, alpha, 1.0)
        End Function

        ''' <summary>生成 glmnet 风格的对数间隔 lambda 网格（严格递减）</summary>
        Friend Function LambdaGrid(lambdaMax As Double, steps As Integer, n As Integer, p As Integer) As Double()
            Dim ratio As Double = If(n >= p, MinLambdaRatioLowDim, MinLambdaRatioHighDim)
            Dim grid As Double() = New Double(steps - 1) {}

            For li As Integer = 0 To steps - 1
                grid(li) = lambdaMax * std.Pow(ratio, CDbl(li) / (steps - 1))
            Next

            Return grid
        End Function

        ''' <summary>
        ''' 沿 lambda 路径用块坐标下降求解，并把每一步的系数还原到原始量纲。
        ''' </summary>
        Private Function SolvePath(data As MGaussianData,
                                   grid As Double(),
                                   alpha As Double,
                                   maxIterations As Integer,
                                   tolerance As Double,
                                   rawX As Double()(),
                                   rawY As Double()(),
                                   fNames As String(),
                                   rNames As String(),
                                   standardize As Boolean,
                                   standardizeResponse As Boolean,
                                   fitIntercept As Boolean) As MGaussianFit

            Dim n As Integer = data.N
            Dim p As Integer = data.P
            Dim k As Integer = data.K
            Dim steps As Integer = grid.Length

            ' beta 为标准化空间中的 p x K 系数（warm start 会一直复用这块缓冲区）
            Dim beta As Double()() = RectangularArray.Matrix(Of Double)(p, k)
            Dim residual As Double()() = RectangularArray.Matrix(Of Double)(n, k)
            Dim delta As Double() = New Double(k - 1) {}
            Dim candidate As Double() = New Double(k - 1) {}

            For i As Integer = 0 To n - 1
                For c As Integer = 0 To k - 1
                    residual(i)(c) = data.Response(i)(c) - data.BaseIntercept(c)
                Next
            Next

            Dim intercepts As Double()() = New Double(steps - 1)() {}
            Dim coefficients As Double()()() = New Double(steps - 1)()() {}
            Dim groupNorms As Double()() = New Double(steps - 1)() {}
            Dim activeGroups As Integer() = New Integer(steps - 1) {}
            Dim trainMse As Double() = New Double(steps - 1) {}
            Dim convergedFlags As Boolean() = New Boolean(steps - 1) {}
            Dim iterationCounts As Integer() = New Integer(steps - 1) {}

            For li As Integer = 0 To steps - 1
                Dim lam As Double = grid(li)
                Dim iterations As Integer = 0
                Dim isConverged As Boolean = False

                For iter As Integer = 1 To maxIterations
                    Dim maxDelta As Double = 0.0
                    Dim maxAbsBeta As Double = 1.0

                    iterations = iter

                    For j As Integer = 0 To p - 1
                        Dim curvature As Double = data.ColumnScale(j)

                        If curvature <= 0.0 Then
                            For c As Integer = 0 To k - 1
                                beta(j)(c) = 0.0
                                delta(c) = 0.0
                            Next
                            Continue For
                        End If

                        ' c_j = beta_j + (sum_i w_i z_ij R_i) / s_j
                        For c As Integer = 0 To k - 1
                            candidate(c) = beta(j)(c)
                        Next

                        For i As Integer = 0 To n - 1
                            Dim wz As Double = data.Weight(i) * data.Design(i)(j)

                            If wz = 0.0 Then
                                Continue For
                            End If

                            For c As Integer = 0 To k - 1
                                candidate(c) += wz * residual(i)(c) / curvature
                            Next
                        Next

                        Dim norm As Double = 0.0

                        For c As Integer = 0 To k - 1
                            norm += candidate(c) * candidate(c)
                        Next

                        norm = std.Sqrt(norm)

                        Dim denominator As Double = 1.0 + lam * (1.0 - alpha) / curvature
                        Dim shrink As Double = If(norm > 0.0,
                                                  std.Max(0.0, 1.0 - lam * alpha / (curvature * norm)) / denominator,
                                                  0.0)

                        For c As Integer = 0 To k - 1
                            Dim newValue As Double = candidate(c) * shrink

                            delta(c) = newValue - beta(j)(c)
                            beta(j)(c) = newValue
                            maxDelta = std.Max(maxDelta, std.Abs(delta(c)))
                            maxAbsBeta = std.Max(maxAbsBeta, std.Abs(newValue))
                        Next

                        ' 增量更新残差：R_i -= z_ij * delta
                        For i As Integer = 0 To n - 1
                            Dim z As Double = data.Design(i)(j)

                            If z = 0.0 Then
                                Continue For
                            End If

                            For c As Integer = 0 To k - 1
                                residual(i)(c) -= z * delta(c)
                            Next
                        Next
                    Next

                    If maxDelta <= tolerance * maxAbsBeta Then
                        isConverged = True
                        Exit For
                    End If
                Next

                ' 训练误差（按原始量纲计算，均值覆盖全部样本与全部响应）
                Dim err As Double = 0.0

                For i As Integer = 0 To n - 1
                    For c As Integer = 0 To k - 1
                        Dim d As Double = data.ResponseScales(c) * residual(i)(c)
                        err += data.Weight(i) * d * d
                    Next
                Next

                trainMse(li) = err / CDbl(k)
                convergedFlags(li) = isConverged
                iterationCounts(li) = iterations

                ' 还原到原始量纲：
                '   coef(j,c) = responseScale(c) * beta(j,c) / featureScale(j)
                '   intercept(c) = responseMean(c) + responseScale(c)*b0(c) - sum_j featureMean(j)*coef(j,c)
                Dim coefOrig As Double()() = RectangularArray.Matrix(Of Double)(p, k)
                Dim interceptOrig As Double() = New Double(k - 1) {}
                Dim norms As Double() = New Double(p - 1) {}
                Dim active As Integer = 0

                For j As Integer = 0 To p - 1
                    For c As Integer = 0 To k - 1
                        coefOrig(j)(c) = data.ResponseScales(c) * beta(j)(c) / data.FeatureScales(j)
                        norms(j) += coefOrig(j)(c) * coefOrig(j)(c)
                    Next

                    norms(j) = std.Sqrt(norms(j))

                    If norms(j) > GroupEpsilon Then
                        active += 1
                    End If
                Next

                For c As Integer = 0 To k - 1
                    Dim b0 As Double = data.ResponseMeans(c) + data.ResponseScales(c) * data.BaseIntercept(c)

                    For j As Integer = 0 To p - 1
                        b0 -= data.FeatureMeans(j) * coefOrig(j)(c)
                    Next

                    interceptOrig(c) = If(fitIntercept, b0, 0.0)
                Next

                intercepts(li) = interceptOrig
                coefficients(li) = coefOrig
                groupNorms(li) = norms
                activeGroups(li) = active
            Next

            Return New MGaussianFit With {
                .lambdas = grid,
                .intercepts = intercepts,
                .coefficients = coefficients,
                .groupNorms = groupNorms,
                .df = activeGroups,
                .trainMse = trainMse,
                .converged = convergedFlags,
                .iterations = iterationCounts,
                .alpha = alpha,
                .standardize = standardize,
                .standardizeResponse = standardizeResponse,
                .HasIntercept = fitIntercept,
                .featureNames = fNames,
                .responseNames = rNames,
                .FeatureMeans = data.FeatureMeans,
                .FeatureScales = data.FeatureScales,
                .ResponseMeans = data.ResponseMeans,
                .ResponseScales = data.ResponseScales,
                .TrainingX = rawX,
                .TrainingY = rawY,
                .PathLength = steps
            }
        End Function
    End Module
End Namespace
