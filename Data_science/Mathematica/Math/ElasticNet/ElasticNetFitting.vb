Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports std = System.Math

''' <summary>
''' 弹性网络（Elastic Net）的求解器
''' 
''' 弹性网络最小化下面这个带联合惩罚的目标函数（sklearn 的参数化）：
''' 
''' ```
''' J(b) = 1/(2n) * ||y - X*b||^2
'''      + alpha * ( l1Ratio * ||b||_1 + (1 - l1Ratio)/2 * ||b||_2^2 )
''' ```
''' 
''' 其中 ``alpha``（sklearn 的 ``alpha``、glmnet 的 ``lambda``）控制总的正则化强度，
''' ``l1Ratio``（sklearn 的 ``l1_ratio``、glmnet 的 ``alpha``）控制 L1 与 L2 的
''' 混合比例：0 就是纯 Ridge、1 就是纯 Lasso。
''' 
''' 求解使用**坐标下降**（与 sklearn / glmnet 同源）：每次只更新一个系数，
''' 用软阈值算子实现 L1 的稀疏性，同时维护残差向量做增量更新，
''' 因此一次坐标扫描的复杂度只有 O(n)。
''' 
''' 特征会先中心化并且（默认）标准化，截距单独估计且**不施加惩罚**；
''' 路径求解时以上一步的解作为下一步的初值（warm start）。
''' </summary>
''' <remarks>
''' 注意：``l1Ratio`` 非常小（例如小于 0.01）的时候数值上并不可靠，
''' 这与 sklearn 文档的提示一致；需要极小值时建议直接使用 Ridge 回归。
''' </remarks>
Public Module ElasticNetFitting

    ''' <summary>
    ''' 预处理之后的数据视图：中心化并且（可选）标准化
    ''' </summary>
    Friend Class ElasticNetData
        ''' <summary>样本数量</summary>
        Friend N As Integer
        ''' <summary>特征数量</summary>
        Friend P As Integer
        ''' <summary>标准化之后的特征矩阵</summary>
        Friend X As Double()()
        ''' <summary>中心化之后的响应向量</summary>
        Friend Y As Double()
        ''' <summary>特征列的均值</summary>
        Friend Means As Double()
        ''' <summary>特征列的标准差（不标准化时为 1）</summary>
        Friend Scales As Double()
        ''' <summary>每一列的 ||x||^2/n；标准化列为 1、常数列为 0</summary>
        Friend ColumnScale As Double()
        ''' <summary>响应变量的均值</summary>
        Friend YMean As Double
    End Class

    ''' <summary>
    ''' 一条正则化路径上的解
    ''' </summary>
    Friend Class PathSolution
        Friend Coefficients As Double()()
        Friend Intercepts As Double()
        Friend RSquared As Double()
        Friend NonZero As Integer()
    End Class

    ''' <summary>
    ''' 默认的 L1 混合比例网格，与 sklearn 的 ``ElasticNetCV`` 默认值一致
    ''' </summary>
    Public ReadOnly Property DefaultL1Ratios As Double()
        Get
            Return New Double() {0.1, 0.5, 0.7, 0.9, 0.95, 0.99, 1.0}
        End Get
    End Property

    ''' <summary>
    ''' 在给定的 ``(alpha, l1Ratio)`` 之上拟合一个弹性网络模型
    ''' </summary>
    ''' <param name="x">自变量矩阵，每一行是一个样本、每一列是一个特征</param>
    ''' <param name="y">与 <paramref name="x"/> 各行对应的观测值向量</param>
    ''' <param name="alpha">总的正则化强度，必须是大于等于 0 的数值</param>
    ''' <param name="l1Ratio">
    ''' L1 惩罚所占的混合比例，取值 [0, 1]；0 表示纯 Ridge、1 表示纯 Lasso
    ''' </param>
    ''' <param name="standardize">
    ''' 是否先对特征做标准化；中心化总是会做。置为 False 的时候惩罚就直接作用在
    ''' 原始量纲的系数之上，此时 ``alpha`` 的实际力度会随各特征的单位而改变，
    ''' 一般只在所有特征本身同量纲（或者已经手工标准化）的时候才这样用
    ''' </param>
    ''' <param name="tolerance">坐标下降的收敛容差（系数更新的最大变化量）</param>
    ''' <param name="maxIterations">坐标下降的最大迭代轮数</param>
    ''' <param name="featureNames">特征列的名称，缺省时自动生成 ``[1]..[p]``</param>
    ''' <returns>拟合结果</returns>
    Public Function Fit(x As Double()(),
                        y As Double(),
                        alpha As Double,
                        l1Ratio As Double,
                        Optional standardize As Boolean = True,
                        Optional tolerance As Double = 0.0001,
                        Optional maxIterations As Integer = 10000,
                        Optional featureNames As String() = Nothing) As ElasticNetFit

        Dim n As Integer = 0
        Dim p As Integer = 0

        Call CheckMatrix(x, y, NameOf(Fit), n, p)
        Call CheckAlpha(alpha, l1Ratio, NameOf(Fit))
        Call CheckSolver(tolerance, maxIterations, NameOf(Fit))

        Dim data As ElasticNetData = Prepare(x, y, standardize, Nothing)
        Dim beta As Double() = New Double(p - 1) {}
        Dim iterations As Integer = 0
        Dim converged As Boolean = CoordinateDescent(data, alpha, l1Ratio, beta, tolerance, maxIterations, iterations)

        Return CreateFit(x, y, data, alpha, l1Ratio, beta, iterations, converged, featureNames)
    End Function

    ''' <summary>
    ''' 沿一条对数间隔的 alpha 路径求解整条正则化路径
    ''' 
    ''' alpha 由大到小排列：最大的 alpha 之下所有系数都为零（完全稀疏），
    ''' 越往后越接近无惩罚的最小二乘解。每一步都以上一步的解作为初值。
    ''' </summary>
    ''' <param name="x">自变量矩阵</param>
    ''' <param name="y">观测值向量</param>
    ''' <param name="l1Ratio">L1 惩罚所占的混合比例</param>
    ''' <param name="numberOfAlphas">路径的步数</param>
    ''' <param name="alphaRatio">路径的终止比例，即最小 alpha = 最大 alpha × 这个比例</param>
    ''' <param name="alphaMax">手写指定路径的起点 alpha，缺省时自动推导</param>
    ''' <param name="standardize">是否先对特征做标准化</param>
    ''' <param name="tolerance">坐标下降的收敛容差</param>
    ''' <param name="maxIterations">每一步的最大迭代轮数</param>
    ''' <param name="featureNames">特征列的名称</param>
    ''' <returns>正则化路径</returns>
    Public Function RegularizationPath(x As Double()(),
                                       y As Double(),
                                       l1Ratio As Double,
                                       Optional numberOfAlphas As Integer = 100,
                                       Optional alphaRatio As Double = 0.001,
                                       Optional alphaMax As Double? = Nothing,
                                       Optional standardize As Boolean = True,
                                       Optional tolerance As Double = 0.0001,
                                       Optional maxIterations As Integer = 10000,
                                       Optional featureNames As String() = Nothing) As ElasticNetPath

        Dim n As Integer = 0
        Dim p As Integer = 0

        Call CheckMatrix(x, y, NameOf(RegularizationPath), n, p)
        Call CheckAlpha(0.0, l1Ratio, NameOf(RegularizationPath))
        Call CheckSolver(tolerance, maxIterations, NameOf(RegularizationPath))

        If numberOfAlphas < 1 Then
            Throw New ArgumentException("the number of alphas must be a positive integer!", NameOf(numberOfAlphas))
        End If
        If alphaRatio <= 0.0 OrElse alphaRatio >= 1.0 Then
            Throw New ArgumentException("the alpha ratio must be in the range (0, 1)!", NameOf(alphaRatio))
        End If

        Dim data As ElasticNetData = Prepare(x, y, standardize, Nothing)
        Dim grid As Double() = AlphaGrid(data, l1Ratio, numberOfAlphas, alphaRatio, alphaMax)
        Dim solution As PathSolution = RunPath(data, l1Ratio, grid, tolerance, maxIterations)

        Return New ElasticNetPath With {
            .L1Ratio = l1Ratio,
            .Alphas = grid,
            .Intercepts = solution.Intercepts,
            .Coefficients = solution.Coefficients,
            .RSquared = solution.RSquared,
            .NonZeroCounts = solution.NonZero,
            .FeatureNames = NormalizeNames(featureNames, p)
        }
    End Function

    ''' <summary>
    ''' 在 ``l1Ratio × alpha`` 的二维网格之上做 K 折交叉验证，自动选择最优超参数
    ''' 
    ''' 折划分使用确定性的交错方式（``fold(i) = i Mod k``，等价于 sklearn 默认的
    ''' 不洗牌 KFold），因此结果是完全可复现的、不依赖随机数。
    ''' 每一个 ``l1Ratio`` 都会在全量数据上面先求出一条 alpha 路径，
    ''' 该路径的 alpha 网格随后被各折复用；最终以折外均方误差最小的组合
    ''' 在全量数据上面重新拟合。
    ''' </summary>
    ''' <param name="x">自变量矩阵</param>
    ''' <param name="y">观测值向量</param>
    ''' <param name="l1Ratios">待搜索的 L1 混合比例网格，缺省为 sklearn 的默认值</param>
    ''' <param name="numberOfAlphas">每一比例下的 alpha 路径步数</param>
    ''' <param name="folds">交叉验证的折数，必须大于等于 2</param>
    ''' <param name="standardize">是否先对特征做标准化（每一折只用自己的训练集统计量）</param>
    ''' <param name="tolerance">坐标下降的收敛容差</param>
    ''' <param name="maxIterations">每一步的最大迭代轮数</param>
    ''' <param name="featureNames">特征列的名称</param>
    ''' <returns>交叉验证结果（含最优超参数与重新拟合的最终模型）</returns>
    Public Function CrossValidate(x As Double()(),
                                  y As Double(),
                                  Optional l1Ratios As Double() = Nothing,
                                  Optional numberOfAlphas As Integer = 50,
                                  Optional folds As Integer = 5,
                                  Optional standardize As Boolean = True,
                                  Optional tolerance As Double = 0.0001,
                                  Optional maxIterations As Integer = 10000,
                                  Optional featureNames As String() = Nothing) As ElasticNetCVResult

        Dim n As Integer = 0
        Dim p As Integer = 0

        Call CheckMatrix(x, y, NameOf(CrossValidate), n, p)
        Call CheckSolver(tolerance, maxIterations, NameOf(CrossValidate))

        If numberOfAlphas < 1 Then
            Throw New ArgumentException("the number of alphas must be a positive integer!", NameOf(numberOfAlphas))
        End If
        If folds < 2 Then
            Throw New ArgumentException("the cross validation requires at least 2 folds!", NameOf(folds))
        End If
        If folds > n Then
            Throw New ArgumentException($"the fold number ({folds}) can not be larger than the sample size ({n})!", NameOf(folds))
        End If

        Dim ratios As Double() = If(l1Ratios, DefaultL1Ratios)

        If ratios.Length = 0 Then
            Throw New ArgumentException("at least one l1Ratio value is required!", NameOf(l1Ratios))
        End If

        For i As Integer = 0 To ratios.Length - 1
            Call CheckAlpha(0.0, ratios(i), NameOf(CrossValidate))
        Next

        Dim fullData As ElasticNetData = Prepare(x, y, standardize, Nothing)
        Dim ratioSize As Integer = ratios.Length
        Dim alphas As Double()() = New Double(ratioSize - 1)() {}
        Dim errs As Double()() = New Double(ratioSize - 1)() {}

        ' 交错划分，保证可复现
        Dim foldId As Integer() = New Integer(n - 1) {}

        For i As Integer = 0 To n - 1
            foldId(i) = i Mod folds
        Next

        For ri As Integer = 0 To ratioSize - 1
            Dim ratio As Double = ratios(ri)
            Dim grid As Double() = AlphaGrid(fullData, ratio, numberOfAlphas, 0.001, Nothing)
            Dim accum As Double() = New Double(grid.Length - 1) {}

            For f As Integer = 0 To folds - 1
                Dim train As New List(Of Integer)
                Dim test As New List(Of Integer)

                For i As Integer = 0 To n - 1
                    If foldId(i) = f Then
                        Call test.Add(i)
                    Else
                        Call train.Add(i)
                    End If
                Next

                If train.Count = 0 OrElse test.Count = 0 Then
                    Continue For
                End If

                ' 每一折的标准化统计量只能来自它自己的训练集，否则会把测试集信息泄漏进模型
                Dim foldData As ElasticNetData = Prepare(x, y, standardize, train.ToArray)
                Dim solution As PathSolution = RunPath(foldData, ratio, grid, tolerance, maxIterations)

                For k As Integer = 0 To grid.Length - 1
                    Dim coef As Double() = solution.Coefficients(k)
                    Dim intercept As Double = solution.Intercepts(k)
                    Dim sse As Double = 0.0

                    For Each t As Integer In test
                        Dim yhat As Double = intercept

                        For j As Integer = 0 To p - 1
                            yhat += coef(j) * x(t)(j)
                        Next

                        Dim d As Double = y(t) - yhat

                        sse += d * d
                    Next

                    accum(k) += sse
                Next
            Next

            ' 累计的折外平方和平均成均方误差
            For k As Integer = 0 To accum.Length - 1
                accum(k) = accum(k) / n
            Next

            alphas(ri) = grid
            errs(ri) = accum
        Next

        ' 挑选折外均方误差最小的组合
        Dim bestRatioIdx As Integer = 0
        Dim bestAlphaIdx As Integer = 0
        Dim bestErr As Double = Double.PositiveInfinity

        For ri As Integer = 0 To ratioSize - 1
            Dim grid As Double() = errs(ri)

            If grid Is Nothing Then
                Continue For
            End If

            For k As Integer = 0 To grid.Length - 1
                If grid(k) < bestErr Then
                    bestErr = grid(k)
                    bestRatioIdx = ri
                    bestAlphaIdx = k
                End If
            Next
        Next

        ' 用最优超参数在全量数据上面重新拟合
        Dim bestL1 As Double = ratios(bestRatioIdx)
        Dim bestAlpha As Double = alphas(bestRatioIdx)(bestAlphaIdx)
        Dim beta As Double() = New Double(p - 1) {}
        Dim iterations As Integer = 0
        Dim converged As Boolean = CoordinateDescent(fullData, bestAlpha, bestL1, beta, tolerance, maxIterations, iterations)

        Return New ElasticNetCVResult With {
            .L1Ratios = ratios,
            .Alphas = alphas,
            .MSE = errs,
            .Folds = folds,
            .BestL1RatioIndex = bestRatioIdx,
            .BestAlphaIndex = bestAlphaIdx,
            .Fit = CreateFit(x, y, fullData, bestAlpha, bestL1, beta, iterations, converged, featureNames)
        }
    End Function

    ''' <summary>
    ''' 组装最终的拟合结果对象
    ''' </summary>
    Private Function CreateFit(x As Double()(),
                               y As Double(),
                               data As ElasticNetData,
                               alpha As Double,
                               l1Ratio As Double,
                               beta As Double(),
                               iterations As Integer,
                               converged As Boolean,
                               featureNames As String()) As ElasticNetFit

        Dim intercept As Double = 0.0
        Dim coefficients As Double() = Nothing

        Call BackTransform(data, beta, intercept, coefficients)

        Dim n As Integer = data.N
        Dim p As Integer = data.P
        Dim sse As Double = 0.0
        Dim sst As Double = 0.0

        For i As Integer = 0 To n - 1
            Dim yhat As Double = intercept

            For j As Integer = 0 To p - 1
                yhat += coefficients(j) * x(i)(j)
            Next

            Dim d As Double = y(i) - yhat

            sse += d * d

            Dim dm As Double = y(i) - data.YMean

            sst += dm * dm
        Next

        Dim model As New ElasticNetFit With {
            .Alpha = alpha,
            .L1Ratio = l1Ratio,
            .Intercept = intercept,
            .Coefficients = coefficients,
            .FeatureNames = NormalizeNames(featureNames, p),
            .SSE = sse,
            .SST = sst,
            .Iterations = iterations,
            .Converged = converged
        }

        Dim tests As IFitError() = New IFitError(n - 1) {}

        For i As Integer = 0 To n - 1
            Dim yhat As Double = intercept

            For j As Integer = 0 To p - 1
                yhat += coefficients(j) * x(i)(j)
            Next

            tests(i) = New TestPoint With {
                .Y = y(i),
                .Yfit = yhat
            }
        Next

        model.ErrorTest = tests

        Return model
    End Function

    ''' <summary>
    ''' 中心化（并且可选地标准化）给定的数据；<paramref name="rows"/> 非空的时候
    ''' 只使用其中的行（交叉验证的每一折都只用自己的训练集统计量）
    ''' </summary>
    Private Function Prepare(x As Double()(), y As Double(), standardize As Boolean, rows As Integer()) As ElasticNetData
        Dim n As Integer = If(rows Is Nothing, y.Length, rows.Length)
        Dim p As Integer = x(0).Length
        Dim data As New ElasticNetData With {
            .N = n,
            .P = p,
            .Means = New Double(p - 1) {},
            .Scales = New Double(p - 1) {},
            .ColumnScale = New Double(p - 1) {},
            .X = RectangularArray.Matrix(Of Double)(n, p),
            .Y = New Double(n - 1) {}
        }
        Dim i As Integer
        Dim j As Integer

        ' 均值
        For j = 0 To p - 1
            Dim sum As Double = 0.0

            For i = 0 To n - 1
                sum += x(RowAt(rows, i))(j)
            Next

            data.Means(j) = sum / n
        Next

        Dim ySum As Double = 0.0

        For i = 0 To n - 1
            ySum += y(RowAt(rows, i))
        Next

        data.YMean = ySum / n

        ' 标准差（总体标准差，使得标准化之后每一列的 ||x||^2/n 恰好为 1）
        For j = 0 To p - 1
            If standardize Then
                Dim var As Double = 0.0

                For i = 0 To n - 1
                    Dim d As Double = x(RowAt(rows, i))(j) - data.Means(j)

                    var += d * d
                Next

                var = var / n
                data.Scales(j) = If(var > 0.0, std.Sqrt(var), 1.0)
            Else
                data.Scales(j) = 1.0
            End If
        Next

        ' 填充
        For i = 0 To n - 1
            Dim src As Integer = RowAt(rows, i)

            data.Y(i) = y(src) - data.YMean

            For j = 0 To p - 1
                data.X(i)(j) = (x(src)(j) - data.Means(j)) / data.Scales(j)
            Next
        Next

        ' 每一列的 ||x||^2/n
        For j = 0 To p - 1
            Dim sum As Double = 0.0

            For i = 0 To n - 1
                sum += data.X(i)(j) * data.X(i)(j)
            Next

            data.ColumnScale(j) = sum / n
        Next

        Return data
    End Function

    ''' <summary>
    ''' 在一条给定的 alpha 网格上面求解正则化路径（warm start）
    ''' </summary>
    Private Function RunPath(data As ElasticNetData,
                             l1Ratio As Double,
                             grid As Double(),
                             tolerance As Double,
                             maxIterations As Integer) As PathSolution

        Dim steps As Integer = grid.Length
        Dim p As Integer = data.P
        Dim solution As New PathSolution With {
            .Coefficients = New Double(steps - 1)() {},
            .Intercepts = New Double(steps - 1) {},
            .RSquared = New Double(steps - 1) {},
            .NonZero = New Integer(steps - 1) {}
        }
        Dim beta As Double() = New Double(p - 1) {}
        Dim sst As Double = 0.0

        For i As Integer = 0 To data.N - 1
            sst += data.Y(i) * data.Y(i)
        Next

        For k As Integer = 0 To steps - 1
            Dim iterations As Integer = 0

            ' warm start：beta 直接沿用上一步（更大 alpha）的结果
            Call CoordinateDescent(data, grid(k), l1Ratio, beta, tolerance, maxIterations, iterations)

            Dim intercept As Double = 0.0
            Dim coefficients As Double() = Nothing

            Call BackTransform(data, beta, intercept, coefficients)

            solution.Coefficients(k) = coefficients
            solution.Intercepts(k) = intercept

            ' 在中心化之后的尺度上计算残差与 R2（与原始尺度等价，但少一次还原）
            Dim sse As Double = 0.0
            Dim nonzero As Integer = 0

            For i As Integer = 0 To data.N - 1
                Dim yhat As Double = 0.0

                For j As Integer = 0 To p - 1
                    yhat += data.X(i)(j) * beta(j)
                Next

                Dim d As Double = data.Y(i) - yhat

                sse += d * d
            Next

            For j As Integer = 0 To p - 1
                If beta(j) <> 0.0 Then
                    nonzero += 1
                End If
            Next

            solution.RSquared(k) = If(sst > 0.0, 1.0 - sse / sst, If(sse <= 0.0, 1.0, 0.0))
            solution.NonZero(k) = nonzero
        Next

        Return solution
    End Function

    ''' <summary>
    ''' 坐标下降内核：在已经预处理过的数据上面求解
    ''' 
    ''' 每一次坐标更新都只需要扫描一列（O(n)），并且用增量方式维护残差：
    ''' 
    ''' ```
    ''' rho_j = (1/n) * x_j'r + s_j * b_j
    ''' b_j   = soft(rho_j, alpha*l1Ratio) / (s_j + alpha*(1 - l1Ratio))
    ''' r    -= x_j * (b_j_new - b_j_old)
    ''' ```
    ''' 
    ''' 其中 ``s_j = ||x_j||^2/n``。常数列（``s_j = 0``）没有可辨识的系数，直接跳过。
    ''' </summary>
    ''' <param name="beta">输入输出参数：传入的是 warm start 初值，返回的是解</param>
    ''' <param name="iterations">返回实际消耗的迭代轮数</param>
    ''' <returns>是否在迭代上限之内收敛</returns>
    Private Function CoordinateDescent(data As ElasticNetData,
                                       alpha As Double,
                                       l1Ratio As Double,
                                       beta As Double(),
                                       tolerance As Double,
                                       maxIterations As Integer,
                                       ByRef iterations As Integer) As Boolean

        Dim n As Integer = data.N
        Dim p As Integer = data.P
        Dim l1 As Double = alpha * l1Ratio
        Dim l2 As Double = alpha * (1.0 - l1Ratio)
        Dim residual As Double() = New Double(n - 1) {}
        Dim i As Integer
        Dim j As Integer

        ' 初始化残差：r = y - X*beta
        For i = 0 To n - 1
            residual(i) = data.Y(i)
        Next

        For j = 0 To p - 1
            If beta(j) = 0.0 Then
                Continue For
            End If

            For i = 0 To n - 1
                residual(i) -= data.X(i)(j) * beta(j)
            Next
        Next

        iterations = 0

        While iterations < maxIterations
            iterations += 1

            Dim maxDelta As Double = 0.0

            For j = 0 To p - 1
                Dim scale As Double = data.ColumnScale(j)

                If scale <= 0.0 Then
                    ' 常数列：标准化之后整列都是 0，系数无法辨识
                    beta(j) = 0.0
                    Continue For
                End If

                Dim rho As Double = 0.0

                For i = 0 To n - 1
                    rho += data.X(i)(j) * residual(i)
                Next

                rho = rho / n + scale * beta(j)

                Dim updated As Double = SoftThreshold(rho, l1) / (scale + l2)

                If Double.IsNaN(updated) OrElse Double.IsInfinity(updated) Then
                    Return False
                End If

                Dim delta As Double = updated - beta(j)

                If delta <> 0.0 Then
                    beta(j) = updated

                    For i = 0 To n - 1
                        residual(i) -= data.X(i)(j) * delta
                    Next

                    Dim moved As Double = std.Abs(delta)

                    If moved > maxDelta Then
                        maxDelta = moved
                    End If
                End If
            Next

            If maxDelta < tolerance Then
                Return True
            End If
        End While

        Return False
    End Function

    ''' <summary>
    ''' 软阈值算子：``soft(z, g) = sign(z) * max(|z| - g, 0)``，它是 L1 惩罚在
    ''' 坐标下降中的近端算子，正是它把系数压到精确的零
    ''' </summary>
    Private Function SoftThreshold(z As Double, gamma As Double) As Double
        If gamma <= 0.0 Then
            Return z
        End If

        If z > gamma Then
            Return z - gamma
        ElseIf z < -gamma Then
            Return z + gamma
        End If

        Return 0.0
    End Function

    ''' <summary>
    ''' 把标准化尺度下的系数还原到原始量纲
    ''' </summary>
    Private Sub BackTransform(data As ElasticNetData,
                              beta As Double(),
                              ByRef intercept As Double,
                              ByRef coefficients As Double())

        Dim p As Integer = data.P

        intercept = data.YMean
        coefficients = New Double(p - 1) {}

        For j As Integer = 0 To p - 1
            Dim c As Double = beta(j) / data.Scales(j)

            coefficients(j) = c
            intercept -= c * data.Means(j)
        Next
    End Sub

    ''' <summary>
    ''' 生成 alpha 网格：最大的 alpha 使得所有系数恰好为零，随后按比例递减
    ''' </summary>
    Private Function AlphaGrid(data As ElasticNetData,
                               l1Ratio As Double,
                               steps As Integer,
                               ratio As Double,
                               alphaMax As Double?) As Double()

        If steps < 1 Then
            steps = 1
        End If

        Dim top As Double

        If alphaMax.HasValue Then
            top = alphaMax.Value
        Else
            Dim score As Double = 0.0

            For j As Integer = 0 To data.P - 1
                Dim s As Double = 0.0

                For i As Integer = 0 To data.N - 1
                    s += data.X(i)(j) * data.Y(i)
                Next

                s = std.Abs(s) / data.N

                If s > score Then
                    score = s
                End If
            Next

            ' 最大 alpha 与 L1 惩罚强度成正比；l1Ratio 极小的时候这个上界会发散，
            ' 因此在这种情况下退化为不带 L1 归一化的上界
            top = If(l1Ratio > 0.0001, score / l1Ratio, score)
        End If

        If top <= 0.0 Then
            top = 1.0
        End If

        Dim grid As Double() = New Double(steps - 1) {}

        For k As Integer = 0 To steps - 1
            grid(k) = top * std.Pow(ratio, k)
        Next

        Return grid
    End Function

    ''' <summary>
    ''' 校验特征矩阵与观测向量
    ''' </summary>
    Private Sub CheckMatrix(x As Double()(), y As Double(), caller As String, ByRef n As Integer, ByRef p As Integer)
        If x Is Nothing Then
            Throw New ArgumentNullException(NameOf(x))
        End If
        If y Is Nothing Then
            Throw New ArgumentNullException(NameOf(y))
        End If
        If x.Length = 0 OrElse x(0) Is Nothing OrElse x(0).Length = 0 Then
            Throw New ArgumentException($"the '{caller}' requires a non-empty feature matrix!", NameOf(x))
        End If
        If x.Length <> y.Length Then
            Throw New ArgumentException($"the size of the feature matrix ({x.Length}) is not matched with the size of the observed values ({y.Length})!", NameOf(y))
        End If

        n = y.Length
        p = x(0).Length

        For i As Integer = 0 To n - 1
            If x(i) Is Nothing OrElse x(i).Length <> p Then
                Throw New ArgumentException($"the row {i} of the feature matrix has a different number of features than the first row ({p})!", NameOf(x))
            End If
        Next
    End Sub

    ''' <summary>
    ''' 校验正则化参数
    ''' </summary>
    Private Sub CheckAlpha(alpha As Double, l1Ratio As Double, caller As String)
        If Double.IsNaN(alpha) OrElse Double.IsInfinity(alpha) OrElse alpha < 0.0 Then
            Throw New ArgumentException($"the '{caller}' requires a finite and non-negative alpha, but {alpha} was given!", NameOf(alpha))
        End If
        If Double.IsNaN(l1Ratio) OrElse l1Ratio < 0.0 OrElse l1Ratio > 1.0 Then
            Throw New ArgumentException($"the l1 ratio of the '{caller}' must be in the range [0, 1], but {l1Ratio} was given!", NameOf(l1Ratio))
        End If
    End Sub

    ''' <summary>
    ''' 校验求解器的数值参数
    ''' </summary>
    Private Sub CheckSolver(tolerance As Double, maxIterations As Integer, caller As String)
        If Double.IsNaN(tolerance) OrElse tolerance <= 0.0 Then
            Throw New ArgumentException($"the tolerance of the '{caller}' must be a positive number!", NameOf(tolerance))
        End If
        If maxIterations < 1 Then
            Throw New ArgumentException($"the max iteration number of the '{caller}' must be a positive integer!", NameOf(maxIterations))
        End If
    End Sub

    ''' <summary>
    ''' 规范化特征名：数量不匹配的时候自动生成 ``[1]..[p]``
    ''' </summary>
    Private Function NormalizeNames(names As String(), p As Integer) As String()
        If names Is Nothing OrElse names.Length <> p Then
            Return Enumerable.Range(1, p).Select(Function(i) $"[{i}]").ToArray
        End If

        Return names
    End Function

    ''' <summary>
    ''' 把子集下标映射回原始行号
    ''' </summary>
    Private Function RowAt(rows As Integer(), i As Integer) As Integer
        If rows Is Nothing Then
            Return i
        End If

        Return rows(i)
    End Function
End Module
