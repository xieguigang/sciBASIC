Imports System
Imports System.Linq
Imports Microsoft.VisualBasic.Data
Imports Microsoft.VisualBasic.Data.Bootstrapping
Imports Microsoft.VisualBasic.Data.Bootstrapping.ElasticNet

''' <summary>
''' Elastic Net 弹性网络算法的自检程序。
''' 
''' 覆盖：正交设计之上的闭式解、共线特征的成组效应、两种退化（纯 Ridge / 无惩罚）、
''' 稀疏性、正则化路径、交叉验证选参、预测写回与参数校验。
''' </summary>
Module Program

    Private failures As Integer = 0

    Sub Main()
        Call Console.WriteLine("== Elastic Net self test ==")
        Call Console.WriteLine()

        Call CheckGroupEffect()
        Call CheckOrthogonalDesign()
        Call CheckRidgeLimit()
        Call CheckOlsLimit()
        Call CheckSparsityAndPath()
        Call CheckCrossValidation()
        Call CheckPrediction()
        Call CheckErrorHandling()

        Call Console.WriteLine()
        Call Console.WriteLine($"{(If(failures = 0, "PASS", "FAIL"))}: {failures} failure(s)")

        Environment.ExitCode = If(failures = 0, 0, 1)
    End Sub

    ''' <summary>
    ''' 成组效应：两个**完全相同**的预测变量，弹性网络会把系数平均分配；
    ''' 而纯 Lasso 的解不唯一，会任意保留其中一个
    ''' </summary>
    Private Sub CheckGroupEffect()
        Call Section("grouping effect")

        Dim n As Integer = 40
        Dim features As Double()() = New Double(n - 1)() {}
        Dim y As Double() = New Double(n - 1) {}

        For i As Integer = 0 To n - 1
            Dim a As Double = 10.0 * (Noise(i * 1000) - 0.5)

            features(i) = New Double() {a, a}
            y(i) = 2.0 * a + 0.5
        Next

        Dim table As New NumericTable(features, Nothing, New String() {"a", "b"})

        Call table.SetLabel("y", y)

        ' 成组效应的对称性只在**真正的最优点**上成立，因此这里把收敛容差收紧到 1e-12；
        ' 默认的 1e-4 会在最优解附近留下约 1e-4 量级的坐标间不平衡
        Dim model As ElasticNetFit = table.ElasticNetRegression(alpha:=0.1, l1Ratio:=0.5, tolerance:=0.000000000001)

        Call Check(model.Coefficients.Length = 2, $"the model has {model.Coefficients.Length} coefficient(s)")
        Call Check(Math.Abs(model.Coefficients(0) - model.Coefficients(1)) < 0.000001,
                   $"the duplicated features share the coefficient equally: b(a) = {model.Coefficients(0):F6}, b(b) = {model.Coefficients(1):F6}")
        Call Check(model.Coefficients(0) > 0.0, "the duplicated features are kept in the model")
        Call Check(model.R2 > 0.99, $"R2 = {model.R2:F6}")
    End Sub

    ''' <summary>
    ''' 正交设计：变量之间两两正交（Hadamard 结构），坐标下降的每一步互相独立，
    ''' 因此存在闭式解 ``b_j = soft(x_j'y/n, alpha*l1) / (s_j + alpha*(1-l1))``
    ''' </summary>
    Private Sub CheckOrthogonalDesign()
        Call Section("orthogonal design (closed form)")

        ' 4 个 ±1 的模式：各列均值 0、模长平方为 4、两两正交；重复 4 次得到 16 个样本
        Dim patterns As Double()() = {
            New Double() {1.0, 1.0, 1.0},
            New Double() {-1.0, 1.0, -1.0},
            New Double() {1.0, -1.0, -1.0},
            New Double() {-1.0, -1.0, 1.0}
        }
        Dim repeats As Integer = 4
        Dim n As Integer = patterns.Length * repeats
        Dim features As Double()() = New Double(n - 1)() {}
        Dim y As Double() = New Double(n - 1) {}

        For i As Integer = 0 To n - 1
            Dim row As Double() = patterns(i Mod patterns.Length)

            features(i) = CType(row.Clone(), Double())
            ' 真实系数为 (3, -2, 0.5)，由于列正交，x_j'y/n 恰好就是这三个数
            y(i) = 3.0 * row(0) - 2.0 * row(1) + 0.5 * row(2)
        Next

        Dim table As New NumericTable(features, Nothing, New String() {"x1", "x2", "x3"})

        Call table.SetLabel("y", y)

        Dim alpha As Double = 1.0
        Dim l1Ratio As Double = 0.5
        Dim model As ElasticNetFit = table.ElasticNetRegression(alpha, l1Ratio)
        ' 标准化之后 s_j = 1，因此闭式解是 soft(z, alpha*l1) / (1 + alpha*(1-l1))
        Dim denominator As Double = 1.0 + alpha * (1.0 - l1Ratio)
        Dim expected As Double() = {
            SoftThreshold(3.0, alpha * l1Ratio) / denominator,
            SoftThreshold(-2.0, alpha * l1Ratio) / denominator,
            SoftThreshold(0.5, alpha * l1Ratio) / denominator
        }

        For j As Integer = 0 To 2
            Call Check(Math.Abs(model.Coefficients(j) - expected(j)) < 0.000001,
                       $"b{j + 1} = {model.Coefficients(j):F8}, closed form = {expected(j):F8}")
        Next

        Call Check(Math.Abs(model.Intercept) < 0.000001, $"the intercept vanishes on centered data: {model.Intercept:E3}")
        Call Check(model.Coefficients(2) = 0.0, "the third coefficient is shrunk to exactly zero")
        Call Check(model.NonZeroCount = 2, $"the model selects {model.NonZeroCount} of 3 features")
    End Sub

    ''' <summary>
    ''' l1Ratio = 0 时弹性网络退化为 Ridge：与 DataFittings 的岭回归求解器对拍。
    ''' 
    ''' 注意两边标准差的自由度不同（Ridge 用 n-1、本模块用 n），
    ''' 这等价于 lambda 相差 n/(n-1) 倍，因此这里换算之后再比较。
    ''' </summary>
    Private Sub CheckRidgeLimit()
        Call Section("l1Ratio = 0 (ridge limit)")

        Dim table As NumericTable = BuildRegressionTable(60, 8)
        Dim alpha As Double = 0.5
        Dim n As Integer = table.nsamples
        Dim en As ElasticNetFit = table.ElasticNetRegression(alpha:=alpha, l1Ratio:=0.0)
        Dim ridge = table.RidgeRegression(lambda:=alpha * (n - 1) / n)
        Dim gap As Double = 0.0

        For j As Integer = 0 To en.Coefficients.Length - 1
            gap = Math.Max(gap, Math.Abs(en.Coefficients(j) - ridge.BestCoefficients(j)))
        Next

        Call Check(gap < 0.001, $"elastic net (l1_ratio=0) matches the ridge solver, max |diff| = {gap:E3}")
        Call Check(en.NonZeroCount = en.Coefficients.Length, "no coefficient is exactly zero when l1Ratio = 0")
    End Sub

    ''' <summary>
    ''' alpha = 0 时没有任何惩罚，弹性网络应当回到普通最小二乘
    ''' </summary>
    Private Sub CheckOlsLimit()
        Call Section("alpha = 0 (ordinary least squares)")

        Dim table As NumericTable = BuildBiasTable()
        Dim en As ElasticNetFit = table.ElasticNetRegression(alpha:=0.0, l1Ratio:=0.5, standardize:=False)
        Dim mlr = table.MultipleLinearRegression()
        Dim rows As Double()() = table.NumericRows
        Dim gap As Double = 0.0

        For i As Integer = 0 To rows.Length - 1
            gap = Math.Max(gap, Math.Abs(en.GetY(rows(i)) - mlr.GetY(rows(i))))
        Next

        Call Check(gap < 0.01, $"elastic net (alpha=0) reproduces the MLR fit, max |diff| = {gap:E3}")
        ' 表里的 bias 列是常数列：它与不受惩罚的截距完全共线，因此它的系数被收敛
        ' 到 0（其效果完全由截距承担），其余特征在没有惩罚时都不会被压成零
        Call Check(en.NonZeroCount = en.Coefficients.Length - 1,
                   $"only the constant column is zero, the other {en.NonZeroCount} feature(s) are kept")
    End Sub

    ''' <summary>
    ''' 稀疏性与正则化路径的单调性
    ''' </summary>
    Private Sub CheckSparsityAndPath()
        Call Section("sparsity && regularization path")

        Dim table As NumericTable = BuildRegressionTable(80, 12)
        Dim p As Integer = table.nfeatures
        Dim lasso As ElasticNetFit = table.ElasticNetRegression(alpha:=0.5, l1Ratio:=1.0)

        Call Check(lasso.NonZeroCount < p,
                   $"the pure lasso shrinks {p - lasso.NonZeroCount} of {p} coefficients to exactly zero")

        Dim path As ElasticNetPath = table.ElasticNetRegularizationPath(l1Ratio:=0.5, numberOfAlphas:=20)
        Dim monotone As Boolean = True

        Call Check(path.Count = 20, $"the path has {path.Count} step(s)")
        Call Check(path.NonZeroCounts(0) = 0, $"the first step (largest alpha = {path.Alphas(0):G4}) is fully sparse")

        For k As Integer = 1 To path.Count - 1
            If path.NonZeroCounts(k) < path.NonZeroCounts(k - 1) Then
                monotone = False
            End If
        Next

        Call Check(monotone, "the number of non-zero coefficients is monotone along the path")
        Call Check(path.RSquared(path.Count - 1) > path.RSquared(0),
                   $"R2 grows along the path: {path.RSquared(0):F4} -> {path.RSquared(path.Count - 1):F4}")

        Dim tbl As NumericTable = path.toNumericTable()

        Call Check(tbl.nsamples = 20 AndAlso tbl.nfeatures = p + 1,
                   $"the path table is [{tbl.nsamples} x {tbl.nfeatures}] (intercept + {p} features)")
        Call Check(tbl.HasLabel("alphas") AndAlso tbl.HasLabel("rsquared") AndAlso tbl.HasLabel("nonzero"),
                   "the path table carries the alphas / rsquared / nonzero labels")
    End Sub

    ''' <summary>
    ''' 交叉验证选参
    ''' </summary>
    Private Sub CheckCrossValidation()
        Call Section("cross validation")

        Dim table As NumericTable = BuildRegressionTable(80, 12, {0, 1, 2, 3})
        Dim cv As ElasticNetCVResult = table.ElasticNetCrossValidate(numberOfAlphas:=25, folds:=5)

        Call Check(cv.L1Ratios.Length = 7, $"the default l1Ratio grid has {cv.L1Ratios.Length} value(s)")
        Call Check(cv.Folds = 5, $"the cross validation used {cv.Folds} fold(s)")
        Call Check(cv.Alphas.Length = cv.L1Ratios.Length AndAlso cv.MSE.Length = cv.L1Ratios.Length,
                   "every l1Ratio owns its own alpha grid and error curve")
        Call Check(cv.BestAlpha > 0.0, $"the best alpha = {cv.BestAlpha:G4}")
        Call Check(cv.BestL1Ratio >= 0.1 AndAlso cv.BestL1Ratio <= 1.0, $"the best l1Ratio = {cv.BestL1Ratio:G4}")
        Call Check(cv.BestMSE < Double.PositiveInfinity, $"the best CV mse = {cv.BestMSE:G4}")
        Call Check(cv.Fit IsNot Nothing AndAlso cv.Fit.R2 > 0.9, $"the refit model R2 = {If(cv.Fit Is Nothing, 0.0, cv.Fit.R2):F6}")

        ' 真实模型只有前 4 个特征非零，其余应当是噪声
        Dim informative As Double = 0.0
        Dim noise As Double = 0.0

        For j As Integer = 0 To cv.Fit.Coefficients.Length - 1
            If j < 4 Then
                informative += Math.Abs(cv.Fit.Coefficients(j))
            Else
                noise += Math.Abs(cv.Fit.Coefficients(j))
            End If
        Next

        Call Check(informative > noise * 5.0,
                   $"the informative features dominate the noise features: {informative:F4} vs {noise:F4}")

        Dim tbl As NumericTable = cv.toNumericTable()

        Call Check(tbl.nsamples = 7 * 25, $"the CV error table has {tbl.nsamples} row(s)")
        Call Check(tbl.HasLabel("mse"), "the CV error table carries the mse label")

        ' 数据几乎没有噪声的时候 CV 会一路走到网格的最软端（正则化越少越好）；
        ' 加入明显的噪声之后，它才会在网格**内部**折中偏差与方差
        Dim noisy As NumericTable = BuildRegressionTable(60, 12, {0, 1, 2, 3}, noiseLevel:=12.0)
        Dim noisyCV As ElasticNetCVResult = noisy.ElasticNetCrossValidate(numberOfAlphas:=25, folds:=5)

        Call Check(noisyCV.BestAlphaIndex > 0,
                   $"the CV picks an interior alpha on noisy data: index {noisyCV.BestAlphaIndex} of {noisyCV.Alphas(noisyCV.BestL1RatioIndex).Length}, alpha = {noisyCV.BestAlpha:G4}")
    End Sub

    ''' <summary>
    ''' 预测、多项式模型与表导出
    ''' </summary>
    Private Sub CheckPrediction()
        Call Section("prediction && export")

        Dim table As NumericTable = BuildRegressionTable(60, 6)
        Dim p As Integer = table.nfeatures
        Dim model As ElasticNetFit = table.ElasticNetRegression(alpha:=0.05, l1Ratio:=0.5)
        Dim rows As Double()() = table.NumericRows
        Dim predicted As NumericTable = table.SetPrediction(model, name:="en_fit", withResidual:=True)

        Call Check(predicted.HasLabel("en_fit"), "SetPrediction writes the prediction column")
        Call Check(predicted.HasLabel("residual"), "SetPrediction writes the residual column")
        Call Check(table.HasLabel("en_fit"), "SetPrediction writes the prediction into the table in place")

        Dim copy As NumericTable = model.PredictTable(table, name:="en_copy")

        Call Check(copy.HasLabel("en_copy") AndAlso Not table.HasLabel("en_copy"),
                   "PredictTable writes the prediction into a copy of the table")
        Call Check(TypeOf model.Polynomial Is ElasticNetPolynomial, "the model exposes an elastic net polynomial with the intercept")
        Call Check(Math.Abs(model.Polynomial.Evaluate(rows(0)) - model.GetY(rows(0))) < 0.000001,
                   "the polynomial and GetY agree on the first sample")

        Dim tbl As NumericTable = model.toNumericTable()

        Call Check(tbl.nsamples = 1 AndAlso tbl.nfeatures = p + 1,
                   $"the single model table is [{tbl.nsamples} x {tbl.nfeatures}]")
        Call Check(tbl.HasLabel("l1_ratio") AndAlso tbl.HasLabel("nonzero"),
                   "the single model table carries the l1_ratio / nonzero labels")
    End Sub

    ''' <summary>
    ''' 参数校验
    ''' </summary>
    Private Sub CheckErrorHandling()
        Call Section("error handling")

        Dim table As NumericTable = BuildRegressionTable(30, 4)

        Call Check(Throws(Sub() table.ElasticNetRegression(alpha:=0.1, l1Ratio:=1.5)),
                   "rejects an l1Ratio greater than 1")
        Call Check(Throws(Sub() table.ElasticNetRegression(alpha:=0.1, l1Ratio:=-0.5)),
                   "rejects a negative l1Ratio")
        Call Check(Throws(Sub() table.ElasticNetRegression(alpha:=-1.0, l1Ratio:=0.5)),
                   "rejects a negative alpha")
        Call Check(Throws(Sub() table.ElasticNetCrossValidate(folds:=1)),
                   "rejects a cross validation with less than 2 folds")
        Call Check(Throws(Sub() ElasticNetFitting.Fit(New Double()() {}, New Double() {}, 0.1, 0.5)),
                   "rejects an empty feature matrix")
        Call Check(Throws(Sub() ElasticNetFitting.RegularizationPath(table.NumericRows, {1.0, 2.0, 3.0, 4.0, 5.0}.ToArray, 0.5)),
                   "rejects a feature matrix which does not match the response vector")
    End Sub

    ''' <summary>
    ''' 带一列常量 bias 的回归表：既满足本模块（截距由模型自己估计）的要求，
    ''' 也能与不带截距的多元线性回归对拍
    ''' </summary>
    Private Function BuildBiasTable() As NumericTable
        Dim patterns As Double()() = {
            New Double() {1.0, 1.0, 1.0},
            New Double() {-1.0, 1.0, -1.0},
            New Double() {1.0, -1.0, -1.0},
            New Double() {-1.0, -1.0, 1.0}
        }
        Dim n As Integer = patterns.Length * 4
        Dim features As Double()() = New Double(n - 1)() {}
        Dim y As Double() = New Double(n - 1) {}

        For i As Integer = 0 To n - 1
            Dim row As Double() = patterns(i Mod patterns.Length)

            features(i) = New Double() {1.0, row(0), row(1), row(2)}
            y(i) = 1.5 + 3.0 * row(0) - 2.0 * row(1) + 0.5 * row(2)
        Next

        Dim table As New NumericTable(features, Nothing, New String() {"bias", "x1", "x2", "x3"})

        Call table.SetLabel("y", y)

        Return table
    End Function

    ''' <summary>
    ''' 合成的回归数据：前 <paramref name="informative"/> 个特征参与生成响应变量，
    ''' 其余都是纯噪声；噪声使用确定性的散列函数，因此结果完全可复现
    ''' </summary>
    Private Function BuildRegressionTable(n As Integer,
                                          p As Integer,
                                          Optional informative As Integer() = Nothing,
                                          Optional noiseLevel As Double = 0.05) As NumericTable
        Dim active As Integer() = If(informative, New Integer() {0, 1})

        If active.Length = 0 OrElse active.Max() >= p Then
            Throw New ArgumentException("the informative feature index is out of the feature range!")
        End If

        Dim features As Double()() = New Double(n - 1)() {}
        Dim y As Double() = New Double(n - 1) {}
        Dim coef As Double() = New Double(active.Length - 1) {}

        For k As Integer = 0 To active.Length - 1
            coef(k) = 3.0 - k * 1.2
        Next

        For i As Integer = 0 To n - 1
            features(i) = New Double(p - 1) {}

            For j As Integer = 0 To p - 1
                features(i)(j) = 10.0 * (Noise(i * 1000 + j) - 0.5)
            Next

            Dim v As Double = 1.5

            For k As Integer = 0 To active.Length - 1
                v += coef(k) * features(i)(active(k))
            Next

            y(i) = v + noiseLevel * (Noise(i * 1000 + 999) - 0.5)
        Next

        Dim names As String() = Enumerable.Range(1, p).Select(Function(i) $"x{i}").ToArray
        Dim table As New NumericTable(features, Nothing, names)

        Call table.SetLabel("y", y)

        Return table
    End Function

    ''' <summary>
    ''' 确定性的伪随机数（取值 0-1），用于构造可复现的测试数据
    ''' </summary>
    Private Function Noise(seed As Integer) As Double
        Dim v As Double = Math.Sin(seed * 12.9898 + 78.233) * 43758.5453

        Return v - Math.Floor(v)
    End Function

    ''' <summary>
    ''' 与坐标下降内核一致的软阈值算子，用于计算正交设计之上的闭式解
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

    Private Sub Section(title As String)
        Call Console.WriteLine()
        Call Console.WriteLine($"-- {title} --")
    End Sub

    Private Sub Check(condition As Boolean, message As String)
        If condition Then
            Call Console.WriteLine($"   [ok]   {message}")
        Else
            failures += 1
            Call Console.WriteLine($"   [FAIL] {message}")
        End If
    End Sub

    Private Function Throws(action As Action) As Boolean
        Try
            Call action()
            Return False
        Catch ex As Exception
            Return True
        End Try
    End Function
End Module
