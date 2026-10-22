Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Math.SIMD
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
        ''' <summary>
        ''' 标准化之后的特征矩阵，<b>列主序</b>：<c>Xc(j)(i)</c> 表示第 j 个特征
        ''' 在第 i 个样本之上的取值。
        ''' 
        ''' 坐标下降的内积与残差更新都是"固定列、扫描样本"，列主序让每一列成为
        ''' 一段连续内存，可以直接交给 <see cref="SIMDIntrinsics.DotFma(Double(), Double())"/>
        ''' 与 <see cref="SIMDIntrinsics.AxpyInplace"/>，避免跨行访问带来的缓存缺失。
        ''' </summary>
        Friend Xc As Double()()
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
        ''' <summary>实际执行的路径步数(早停之后可能小于网格长度)</summary>
        Friend Steps As Integer
        ''' <summary>是否因为非零系数超出上限而提前结束</summary>
        Friend StoppedEarly As Boolean
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

        Return CreateFit(y, data, alpha, l1Ratio, beta, iterations, converged, featureNames)
    End Function

    ''' <summary>
    ''' 在给定的 ``(alpha, l1Ratio)`` 之上拟合弹性网络，入参为<b>列主序</b>的特征矩阵。
    ''' 
    ''' 与 <see cref="Fit(Double()(), Double(), Double, Double, Boolean, Double, Integer, String())"/>
    ''' 完全等价，只是省掉了一次行主序到列主序的转置拷贝：调用方（例如基因调控网络
    ''' 拟合）手里本来就有"每个基因一条表达向量"的列式数据，直接传进来即可。
    ''' </summary>
    ''' <param name="columns">特征列，<c>columns(j)</c> 是第 j 个特征在所有样本之上的取值</param>
    ''' <param name="y">与列向量对齐的观测值</param>
    ''' <param name="alpha">总的正则化强度</param>
    ''' <param name="l1Ratio">L1 惩罚所占的混合比例</param>
    ''' <param name="standardize">是否先对特征做标准化</param>
    ''' <param name="tolerance">坐标下降的收敛容差</param>
    ''' <param name="maxIterations">坐标下降的最大迭代轮数</param>
    ''' <param name="featureNames">特征列的名称</param>
    ''' <returns>拟合结果</returns>
    Public Function FitColumns(columns As Double()(),
                               y As Double(),
                               alpha As Double,
                               l1Ratio As Double,
                               Optional standardize As Boolean = True,
                               Optional tolerance As Double = 0.0001,
                               Optional maxIterations As Integer = 10000,
                               Optional featureNames As String() = Nothing) As ElasticNetFit

        Dim n As Integer = 0
        Dim p As Integer = 0

        Call CheckColumns(columns, y, NameOf(FitColumns), n, p)
        Call CheckAlpha(alpha, l1Ratio, NameOf(FitColumns))
        Call CheckSolver(tolerance, maxIterations, NameOf(FitColumns))

        Dim data As ElasticNetData = PrepareColumns(columns, y, standardize, Nothing)
        Dim beta As Double() = New Double(p - 1) {}
        Dim iterations As Integer = 0
        Dim converged As Boolean = CoordinateDescent(data, alpha, l1Ratio, beta, tolerance, maxIterations, iterations)

        Return CreateFit(y, data, alpha, l1Ratio, beta, iterations, converged, featureNames)
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

        Return NewPath(data, l1Ratio, grid, solution, featureNames)
    End Function

    ''' <summary>
    ''' 沿一条对数间隔的 alpha 路径求解，入参为<b>列主序</b>的特征矩阵。
    ''' 
    ''' 与 <see cref="RegularizationPath(Double()(), Double(), Double, Integer, Double, Double?, Boolean, Double, Integer, String())"/>
    ''' 完全等价，只是省掉了一次转置拷贝。
    ''' </summary>
    ''' <param name="columns">特征列，<c>columns(j)</c> 是第 j 个特征在所有样本之上的取值</param>
    ''' <param name="y">与列向量对齐的观测值</param>
    ''' <param name="l1Ratio">L1 惩罚所占的混合比例</param>
    ''' <param name="numberOfAlphas">路径的步数</param>
    ''' <param name="alphaRatio">路径的终止比例</param>
    ''' <param name="alphaMax">手写指定路径的起点 alpha</param>
    ''' <param name="standardize">是否先对特征做标准化</param>
    ''' <param name="tolerance">坐标下降的收敛容差</param>
    ''' <param name="maxIterations">每一步的最大迭代轮数</param>
    ''' <param name="featureNames">特征列的名称</param>
    ''' <param name="maxFeatures">
    ''' 每个目标基因最多保留的非零系数个数；大于零的时候，路径会在非零个数
    ''' 超过它的两倍时提前结束（更靠后的 alpha 只会更稠密）。
    ''' </param>
    ''' <returns>正则化路径</returns>
    Public Function RegularizationPathColumns(columns As Double()(),
                                              y As Double(),
                                              l1Ratio As Double,
                                              Optional numberOfAlphas As Integer = 20,
                                              Optional alphaRatio As Double = 0.001,
                                              Optional alphaMax As Double? = Nothing,
                                              Optional standardize As Boolean = True,
                                              Optional tolerance As Double = 0.0001,
                                              Optional maxIterations As Integer = 10000,
                                              Optional featureNames As String() = Nothing,
                                              Optional maxFeatures As Integer = 0,
                                              Optional penalty As Double() = Nothing) As ElasticNetPath

        Dim n As Integer = 0
        Dim p As Integer = 0

        Call CheckColumns(columns, y, NameOf(RegularizationPathColumns), n, p)
        Call CheckAlpha(0.0, l1Ratio, NameOf(RegularizationPathColumns))
        Call CheckSolver(tolerance, maxIterations, NameOf(RegularizationPathColumns))

        If numberOfAlphas < 1 Then
            Throw New ArgumentException("the number of alphas must be a positive integer!", NameOf(numberOfAlphas))
        End If
        If alphaRatio <= 0.0 OrElse alphaRatio >= 1.0 Then
            Throw New ArgumentException("the alpha ratio must be in the range (0, 1)!", NameOf(alphaRatio))
        End If
        If penalty IsNot Nothing AndAlso penalty.Length <> p Then
            Throw New ArgumentException(
                $"the length of the penalty vector ({penalty.Length}) must equal the number of the feature columns ({p})!",
                NameOf(penalty))
        End If

        Dim data As ElasticNetData = PrepareColumns(columns, y, standardize, Nothing)
        Dim grid As Double() = AlphaGrid(data, l1Ratio, numberOfAlphas, alphaRatio, alphaMax)
        Dim solution As PathSolution = RunPath(data, l1Ratio, grid, tolerance, maxIterations, maxFeatures, penalty)

        Return NewPath(data, l1Ratio, grid, solution, featureNames)
    End Function

    ''' <summary>
    ''' 把路径求解的结果组装成公开的 <see cref="ElasticNetPath"/>；
    ''' 早停的时候 alpha 网格也要同步截断，否则会与系数数组长度不一致。
    ''' </summary>
    Private Function NewPath(data As ElasticNetData,
                             l1Ratio As Double,
                             grid As Double(),
                             solution As PathSolution,
                             featureNames As String()) As ElasticNetPath

        Dim alphas As Double() = grid

        If solution.Steps > 0 AndAlso solution.Steps < grid.Length Then
            alphas = New Double(solution.Steps - 1) {}
            Call Array.Copy(grid, alphas, solution.Steps)
        End If

        Return New ElasticNetPath With {
            .L1Ratio = l1Ratio,
            .Alphas = alphas,
            .Intercepts = solution.Intercepts,
            .Coefficients = solution.Coefficients,
            .RSquared = solution.RSquared,
            .NonZeroCounts = solution.NonZero,
            .FeatureNames = NormalizeNames(featureNames, data.P),
            .StoppedEarly = solution.StoppedEarly
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
            .Fit = CreateFit(y, fullData, bestAlpha, bestL1, beta, iterations, converged, featureNames)
        }
    End Function

    ''' <summary>
    ''' 组装最终的拟合结果对象
    ''' </summary>
    Private Function CreateFit(y As Double(),
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
        Dim sum As Double = 0.0
        Dim sse As Double = 0.0
        Dim sst As Double = 0.0

        ' yhat 用列主序累加：yhat = Σ_j c_j * (mean_j + scale_j * Xc(j))
        ' 一次 AXPY 处理一整列，避免按样本逐行拼内积
        Dim yhat As Double() = New Double(n - 1) {}

        For j As Integer = 0 To p - 1
            If coefficients(j) = 0.0 Then
                Continue For
            End If

            sum += coefficients(j) * data.Means(j)
            Call SIMDIntrinsics.AxpyInplace(coefficients(j) * data.Scales(j), data.Xc(j), yhat)
        Next

        sum += intercept

        For i As Integer = 0 To n - 1
            Dim d As Double = y(i) - (sum + yhat(i))

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
            tests(i) = New TestPoint With {
                .Y = y(i),
                .Yfit = sum + yhat(i)
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
        Dim p As Integer = x(0).Length
        Dim columns As Double()() = New Double(p - 1)() {}

        ' 行主序 -> 列主序：只做一次转置，换来坐标下降整个热循环的连续访存
        For j As Integer = 0 To p - 1
            Dim col As Double() = New Double(y.Length - 1) {}

            For i As Integer = 0 To y.Length - 1
                col(i) = x(i)(j)
            Next

            columns(j) = col
        Next

        Return PrepareColumns(columns, y, standardize, rows)
    End Function

    ''' <summary>
    ''' 中心化（并且可选地标准化）给定的数据，并按<b>列主序</b>组织成
    ''' <see cref="ElasticNetData.Xc"/>。
    ''' 
    ''' 与 <see cref="Prepare(Double()(), Double(), Boolean, Integer())"/> 的区别是
    ''' 这里的入参已经是"每个特征一列"的形式（<c>columns(j)(i)</c>），
    ''' 因此调用方可以直接把表达矩阵的列引用传进来，省掉一次 n x p 的转置拷贝。
    ''' <paramref name="rows"/> 非空的时候只使用其中的样本（交叉验证的每一折
    ''' 都只用自己的训练集统计量）。
    ''' </summary>
    ''' <param name="columns">特征列，<c>columns(j)</c> 的长度必须等于 <paramref name="y"/> 的长度</param>
    ''' <param name="y">与列向量对齐的观测值</param>
    ''' <param name="standardize">是否按总体标准差（除以 n）标准化</param>
    ''' <param name="rows">参与计算的样本行号，为空表示全部样本</param>
    ''' <returns></returns>
    Private Function PrepareColumns(columns As Double()(), y As Double(), standardize As Boolean, rows As Integer()) As ElasticNetData
        Dim n As Integer = If(rows Is Nothing, y.Length, rows.Length)
        Dim p As Integer = columns.Length
        Dim data As New ElasticNetData With {
            .N = n,
            .P = p,
            .Means = New Double(p - 1) {},
            .Scales = New Double(p - 1) {},
            .ColumnScale = New Double(p - 1) {},
            .Xc = New Double(p - 1)() {},
            .Y = New Double(n - 1) {}
        }
        Dim i As Integer
        Dim j As Integer

        Dim ySum As Double = 0.0

        For i = 0 To n - 1
            ySum += y(RowAt(rows, i))
        Next

        data.YMean = ySum / n

        For i = 0 To n - 1
            data.Y(i) = y(RowAt(rows, i)) - data.YMean
        Next

        For j = 0 To p - 1
            Dim src As Double() = columns(j)
            Dim col As Double() = New Double(n - 1) {}
            Dim sum As Double = 0.0

            For i = 0 To n - 1
                Dim v As Double = src(RowAt(rows, i))

                col(i) = v
                sum += v
            Next

            Dim mean As Double = sum / n
            Dim scale As Double = 1.0

            ' 标准差（总体标准差，使得标准化之后每一列的 ||x||^2/n 恰好为 1）
            If standardize Then
                Dim variance As Double = 0.0

                For i = 0 To n - 1
                    Dim d As Double = col(i) - mean

                    variance += d * d
                Next

                variance = variance / n
                scale = If(variance > 0.0, std.Sqrt(variance), 1.0)
            End If

            Dim inv As Double = 1.0 / scale

            For i = 0 To n - 1
                col(i) = (col(i) - mean) * inv
            Next

            data.Means(j) = mean
            data.Scales(j) = scale
            ' 每一列的 ||x||^2/n，用 SIMD 归约；标准化列恒为 1、常数列为 0
            data.ColumnScale(j) = SIMDIntrinsics.SumSquaresFma(col) / n
            data.Xc(j) = col
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
                             maxIterations As Integer,
                             Optional maxFeatures As Integer = 0,
                             Optional penalty As Double() = Nothing) As PathSolution

        Dim steps As Integer = grid.Length
        Dim p As Integer = data.P
        Dim solution As New PathSolution With {
            .Coefficients = New Double(steps - 1)() {},
            .Intercepts = New Double(steps - 1) {},
            .RSquared = New Double(steps - 1) {},
            .NonZero = New Integer(steps - 1) {}
        }
        Dim beta As Double() = New Double(p - 1) {}
        Dim residual As Double() = New Double(data.N - 1) {}
        Dim sst As Double = SIMDIntrinsics.SumSquaresFma(data.Y)
        Dim executed As Integer = 0
        Dim stoppedEarly As Boolean = False
        Dim initResidual As Boolean = True
        ' 是否已经出现过满足 maxFeatures 约束的步；没有出现过就不允许早停，
        ' 否则在半数数据上（例如未取对数、被共同的测序深度因子主导的矩阵）
        ' 会停在 alpha 最大处的平凡解上，得到 r2 ~ 0 的常数模型。
        Dim hasValidStep As Boolean = False

        For k As Integer = 0 To steps - 1
            Dim iterations As Integer = 0

            ' warm start：beta 沿用上一步的结果；residual 也一并沿用
            ' （上一步收敛时 r 恰好等于 y - X*beta），省掉每个 alpha 一次 O(nnz*n) 的重算
            Call CoordinateDescent(data, grid(k), l1Ratio, beta, tolerance, maxIterations, iterations, residual, initResidual, penalty)
            initResidual = False

            executed = k + 1

            ' 收敛之后 SSE = ||r||^2，避免每个 alpha 再做一次 O(n*p) 的 X*beta
            Dim sse As Double = SIMDIntrinsics.SumSquaresFma(residual)
            Dim nonzero As Integer = 0

            For j As Integer = 0 To p - 1
                If beta(j) <> 0.0 Then
                    nonzero += 1
                End If
            Next

            Dim intercept As Double = 0.0
            Dim coefficients As Double() = Nothing

            Call BackTransform(data, beta, intercept, coefficients)

            solution.Coefficients(k) = coefficients
            solution.Intercepts(k) = intercept
            solution.RSquared(k) = If(sst > 0.0, 1.0 - sse / sst, If(sse <= 0.0, 1.0, 0.0))
            solution.NonZero(k) = nonzero

            If nonzero > 0 AndAlso (maxFeatures <= 0 OrElse nonzero <= maxFeatures) Then
                hasValidStep = True
            End If

            ' 非零系数个数随 alpha 减小单调不减：一旦超过两倍上限，
            ' 之后的每一步都不可能再满足 maxFeatures 约束，路径可以就此结束。
            ' 但前提是此前确实已经出现过满足约束的步，否则必须把路径跑完，
            ' 由调用方退化为"解释力最强的一步 + 裁剪"。
            If maxFeatures > 0 AndAlso hasValidStep AndAlso k > 0 AndAlso nonzero > maxFeatures * 2 Then
                stoppedEarly = True
                Exit For
            End If
        Next

        If executed > 0 AndAlso executed < steps Then
            Call Array.Resize(solution.Coefficients, executed)
            Call Array.Resize(solution.Intercepts, executed)
            Call Array.Resize(solution.RSquared, executed)
            Call Array.Resize(solution.NonZero, executed)
        End If

        solution.Steps = executed
        solution.StoppedEarly = stoppedEarly

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
                                       ByRef iterations As Integer,
                                       Optional residual As Double() = Nothing,
                                       Optional initResidual As Boolean = True,
                                       Optional penalty As Double() = Nothing) As Boolean

        Dim n As Integer = data.N
        Dim p As Integer = data.P
        Dim l1 As Double = alpha * l1Ratio
        Dim l2 As Double = alpha * (1.0 - l1Ratio)
        Dim j As Integer

        If residual Is Nothing OrElse residual.Length <> n Then
            residual = New Double(n - 1) {}
            initResidual = True
        End If

        If initResidual Then
            ' 初始化残差：r = y - X*beta
            Call Array.Copy(data.Y, residual, n)

            For j = 0 To p - 1
                If beta(j) = 0.0 Then
                    Continue For
                End If

                Call SIMDIntrinsics.AxpyInplace(-beta(j), data.Xc(j), residual)
            Next
        End If

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

                ' rho = x_j'r，列主序连续访存 + FMA 归约
                Dim rho As Double = SIMDIntrinsics.DotFma(data.Xc(j), residual)

                rho = rho / n + scale * beta(j)

                ' 逐特征惩罚：先验知识支持的边用更小的 L1 权重，因而更容易存活。
                ' penalty 为空时走原来的标量分支，热循环里不会多一次数组访问。
                Dim l1j As Double = If(penalty Is Nothing, l1, l1 * penalty(j))
                Dim updated As Double = SoftThreshold(rho, l1j) / (scale + l2)

                If Double.IsNaN(updated) OrElse Double.IsInfinity(updated) Then
                    Return False
                End If

                Dim delta As Double = updated - beta(j)

                If delta <> 0.0 Then
                    beta(j) = updated

                    ' r -= delta * x_j，就地更新，无临时数组
                    Call SIMDIntrinsics.AxpyInplace(-delta, data.Xc(j), residual)

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
                ' 列主序 + FMA 归约取 x_j'y
                Dim s As Double = std.Abs(SIMDIntrinsics.DotFma(data.Xc(j), data.Y)) / data.N

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

        ' 按对数等间隔从 alpha_max 衰减到 alpha_max * ratio。
        ' 注意：这里必须是 ratio^(k / (steps - 1))，而不是 ratio^k —— 后者会让
        ' 第二步就掉到 alpha_max * 0.001，整条路径实际上只剩两个有效解，
        ' 既浪费了后续所有 alpha 的求解时间，也让路径选步失去意义。
        Dim exponent As Double = 1.0R / std.Max(1, steps - 1)

        For k As Integer = 0 To steps - 1
            grid(k) = top * std.Pow(ratio, k * exponent)
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
    ''' 校验列主序的特征矩阵与观测向量
    ''' </summary>
    Private Sub CheckColumns(columns As Double()(), y As Double(), caller As String, ByRef n As Integer, ByRef p As Integer)
        If columns Is Nothing Then
            Throw New ArgumentNullException(NameOf(columns))
        End If
        If y Is Nothing Then
            Throw New ArgumentNullException(NameOf(y))
        End If
        If columns.Length = 0 OrElse columns(0) Is Nothing OrElse columns(0).Length = 0 Then
            Throw New ArgumentException($"the '{caller}' requires a non-empty feature matrix!", NameOf(columns))
        End If

        n = y.Length
        p = columns.Length

        If columns(0).Length <> n Then
            Throw New ArgumentException($"the size of the feature columns ({columns(0).Length}) is not matched with the size of the observed values ({n})!", NameOf(y))
        End If

        For j As Integer = 1 To p - 1
            If columns(j) Is Nothing OrElse columns(j).Length <> n Then
                Throw New ArgumentException($"the column {j} of the feature matrix has a different size ({If(columns(j) Is Nothing, 0, columns(j).Length)}) than the first column ({n})!", NameOf(columns))
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
