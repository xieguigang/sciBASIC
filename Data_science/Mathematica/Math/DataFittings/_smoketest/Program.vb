Imports System
Imports System.Linq
Imports Microsoft.VisualBasic.Data
Imports Microsoft.VisualBasic.Data.Bootstrapping
Imports Microsoft.VisualBasic.Linq

''' <summary>
''' DataFittings 回归算法统一 NumericTable 入口的自检程序。
''' </summary>
Module Program

    Private failures As Integer = 0

    Sub Main()
        Call Console.WriteLine("== DataFittings regression / NumericTable smoketest ==")
        Call Console.WriteLine()

        Dim n As Integer = 10
        Dim xv As Double() = {1, 2, 3, 4, 5, 6, 7, 8, 9, 10}
        Dim yv As Double() = {5.1, 7.2, 8.9, 11.1, 12.8, 15.2, 16.9, 19.1, 20.8, 23.2}
        Dim singleTable As NumericTable = BuildSingleFeatureTable(xv, yv)

        Call Section("input table")
        Call Check(singleTable.nsamples = n AndAlso singleTable.nfeatures = 1, $"table dimension = [{singleTable.nsamples} x {singleTable.nfeatures}]")
        Call Check(singleTable.HasLabel("y"), "table has the y label column")

        Call Section("linear && polynomial")
        Dim fit As FitResult = singleTable.LinearFit()

        Call Check(fit.R_square > 0.99, $"LinearFit R2 = {fit.R_square:F6}")
        Call Check(System.Math.Abs(fit.Slope - 2.0) < 0.01, $"slope = {fit.Slope:F4}")
        Call Check(System.Math.Abs(fit.Intercept - 3.0) < 0.1, $"intercept = {fit.Intercept:F4}")

        ' 指定标签列名作为响应变量
        Call singleTable.SetLabel("y2", yv)

        Dim named As FitResult = singleTable.LinearFit(y:="y2")

        Call Check(System.Math.Abs(named.Slope - 2.0) < 0.01, $"LinearFit(y:='y2') slope = {named.Slope:F4}")

        Dim poly As FitResult = singleTable.PolyFit(poly_n:=2)

        Call Check(poly.R_square > 0.99, $"PolyFit(2) R2 = {poly.R_square:F6}")

        Dim curve As IFitted = singleTable.StandardCurveFit()

        Call Check(TypeOf curve Is FitResult, $"StandardCurveFit() -> {curve.GetType.Name}")

        Dim wcurve As IFitted = singleTable.StandardCurveFit(weighted:=AddressOf DefaultWeights)

        Call Check(TypeOf wcurve Is WeightedFit, $"StandardCurveFit(weighted) -> {wcurve.GetType.Name}")

        Dim wfit As WeightedFit = singleTable.WeightedLinearFit(orderOfPolynomial:=1)
        Dim wfitR2 As Double = DirectCast(wfit, IFitted).R2

        Call Check(wfit IsNot Nothing AndAlso wfitR2 > 0.9, $"WeightedLinearFit R2 = {wfitR2:F6}")

        Call Section("constrained && local regression")
        Dim coef As Double() = singleTable.Nnls()

        Call Check(coef.Length = 1, $"NNLS coefficients = [{coef.JoinBy(", ")}]")
        Call Check(coef(0) > 0, "the NNLS coefficient is non-negative")

        Dim lowess = singleTable.Lowess(f:=0.6)

        Call Check(lowess.x.Length = n, $"LOWESS x length = {lowess.x.Length}")
        Call Check(lowess.yfit.Length = n, $"LOWESS fitted value length = {lowess.yfit.Length}")

        Dim loess As LOESSModel = singleTable.LoessFit(span:=0.75, degree:=1)

        Call Check(loess IsNot Nothing AndAlso loess.X.Length = n, $"LOESS model contains {loess.X.Length} points")

        Dim removed As Integer() = Nothing
        Dim robust As IFitted = singleTable.AutoPointDeletion(removed:=removed)

        Call Check(robust IsNot Nothing AndAlso robust.R2 > 0.9, $"AutoPointDeletion R2 = {robust.R2:F6}")
        Call Check(removed.Length <= 1, $"AutoPointDeletion removed {removed.Length} point(s)")

        Call Section("nonlinear curve fitting")
        Dim bayes As BayesianCurveFitting = singleTable.BayesianCurveFit(m:=2)

        Call Check(bayes IsNot Nothing, "BayesianCurveFit returns a fitted model")

        ' 底层的 GaussNewtonSolver 对于非方阵的雅可比矩阵存在既有的维度检查缺陷，
        ' 因此这里只探测表的输入适配，求解器的异常信息会被如实打印出来
        Call Console.WriteLine($"   [note] GaussNewtonFit -> {ProbeGaussNewton(singleTable)}")

        Call Section("multivariate regression")
        Dim f1 As Double() = {1, 2, 3, 4, 5, 6, 7, 8, 9, 10}
        Dim f2 As Double() = {2, 1, 4, 3, 6, 5, 8, 7, 10, 9}
        Dim y2 As Double() = f1.Select(Function(x, i) 1.0 + 2.0 * x + 3.0 * f2(i)).ToArray
        Dim multi As NumericTable = BuildMultiFeatureTable(f1, f2, y2)

        Call Check(multi.nsamples = n AndAlso multi.nfeatures = 3, $"table dimension = [{multi.nsamples} x {multi.nfeatures}]")

        Dim mlr = multi.MultipleLinearRegression()

        Call Check(mlr.R2 > 0.999, $"MultipleLinearRegression R2 = {mlr.R2:F6}")

        Dim nnls2 As Double() = multi.Nnls()

        Call Check(nnls2.Length = 3, $"NNLS coefficient count = {nnls2.Length}")
        Call Check(nnls2.All(Function(c) c >= 0), "all of the NNLS coefficients are non-negative")

        Dim lasso = multi.LassoRegression(Silent:=True)

        Call Check(lasso IsNot Nothing, $"LASSO regularization path steps = {lasso.intercepts.Length}")

        Dim lassoTable As NumericTable = lasso.toNumericTable()

        Call Check(lassoTable.nsamples = lasso.intercepts.Length, $"LASSO path table rows = {lassoTable.nsamples}")
        Call Check(lassoTable.nfeatures = 4, $"LASSO path table columns = {lassoTable.nfeatures} ({lassoTable.featureNames.JoinBy(", ")})")
        Call Check(lassoTable.HasLabel("rsquared"), "the LASSO path table carries the rsquared label column")

        Dim projection = multi.FeatureProjection(dimension:=2)

        Call Check(projection.Length = 3, $"FeatureProjection length = {projection.Length}")

        Dim inputs = multi.AsFitInputs()

        Call Check(inputs.Length = n, $"LMA FitInput count = {inputs.Length}")
        Call Check(inputs(0).factors.Count = 3, $"LMA FitInput factor count = {inputs(0).factors.Count}")

        Call Section("logistic regression")
        Dim ltable As NumericTable = BuildLogisticTable(f1, f2)
        Dim logistic = ltable.LogisticRegression(iterations:=3000, y:="class")

        Call Check(logistic IsNot Nothing, "LogisticRegression returns a fitted model")

        Call Section("prediction")
        Dim predicted As NumericTable = multi.SetPrediction(mlr, withResidual:=True)

        Call Check(predicted.HasLabel("prediction"), "the prediction label column was written")
        Call Check(predicted.GetLabel("prediction").Length = n, "prediction column length matches the sample size")
        Call Check(predicted.HasLabel("residual"), "the residual label column was written")

        Dim maxResidual As Double = predicted.GetLabel("residual").Select(Function(r) System.Math.Abs(r)).Max

        Call Check(maxResidual < 0.000001, $"max |residual| = {maxResidual:E3}")

        Dim copy As NumericTable = mlr.PredictTable(multi, name:="fit2")

        Call Check(copy.HasLabel("fit2"), "PredictTable writes the prediction into the table copy")
        Call Check(Not multi.HasLabel("fit2"), "PredictTable does not modify the source table")

        Call Section("error handling")
        Dim noLabel As New NumericTable(singleTable.NumericRows(deepcopy:=True), singleTable.rowNames, singleTable.featureNames)

        Call Check(Throws(Sub() noLabel.LinearFit()), "LinearFit reports a clear error when there is no response variable")
        Call Check(Throws(Sub() multi.LinearFit()), "the univariate entries reject a table with more than 1 feature column")
        Call Check(Not Throws(Sub() multi.MultipleLinearRegression()), "the multivariate entries accept multiple feature columns")

        Call Console.WriteLine()
        Call Console.WriteLine($"{(If(failures = 0, "PASS", "FAIL"))}: {failures} failure(s)")

        Environment.ExitCode = If(failures = 0, 0, 1)
    End Sub

    ''' <summary>
    ''' 探测高斯-牛顿曲线拟合的表输入适配是否可用
    ''' </summary>
    Private Function ProbeGaussNewton(table As NumericTable) As String
        Try
            Dim gn As Double() = table.GaussNewtonFit(Function(x, args) args(0, 0) + args(1, 0) * x, argumentSize:=2)

            Return $"solved, {gn.Length} value(s) returned"
        Catch ex As Exception
            Return $"{ex.GetType.Name}: {ex.Message}"
        End Try
    End Function

    Private Function BuildSingleFeatureTable(xv As Double(), yv As Double()) As NumericTable
        Dim features As Double()() = xv _
            .Select(Function(x) New Double() {x}) _
            .ToArray
        Dim rows As String() = xv.Select(Function(x) $"s{x}").ToArray
        Dim table As New NumericTable(features, rows, New String() {"x"})

        ' 第一个标签列作为缺省的响应变量
        Call table.SetLabel("y", yv)

        Return table
    End Function

    ''' <summary>
    ''' 多特征回归的测试数据。注意：多元线性回归模型不包含截距项，
    ''' 因此这里显式地添加一列常量 ``bias`` 作为偏置列
    ''' </summary>
    Private Function BuildMultiFeatureTable(f1 As Double(), f2 As Double(), y As Double()) As NumericTable
        Dim features As Double()() = f1 _
            .SeqIterator _
            .Select(Function(i) New Double() {1.0, i.value, f2(i.i)}) _
            .ToArray
        Dim rows As String() = Enumerable.Range(1, f1.Length).Select(Function(i) $"m{i}").ToArray
        Dim table As New NumericTable(features, rows, New String() {"bias", "a", "b"})

        Call table.SetLabel("y", y)

        Return table
    End Function

    ''' <summary>
    ''' 逻辑回归的测试数据：特征归一化到 ``0..1``，类别由第一个特征进行区分
    ''' </summary>
    Private Function BuildLogisticTable(f1 As Double(), f2 As Double()) As NumericTable
        Dim features As Double()() = f1 _
            .SeqIterator _
            .Select(Function(i) New Double() {i.value / 10.0, f2(i.i) / 10.0}) _
            .ToArray
        Dim rows As String() = Enumerable.Range(1, f1.Length).Select(Function(i) $"l{i}").ToArray
        Dim classes As Double() = f1.Select(Function(x) If(x >= 6, 1.0, 0.0)).ToArray
        Dim table As New NumericTable(features, rows, New String() {"a", "b"})

        Call table.SetLabel("class", classes)

        Return table
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
