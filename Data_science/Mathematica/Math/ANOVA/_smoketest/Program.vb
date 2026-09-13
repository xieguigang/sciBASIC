Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports Microsoft.VisualBasic.Data
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Statistics.Hypothesis.ANOVA

''' <summary>
''' ANOVA 多变量分析（PCA / PLS-DA / OPLS-DA）的 NumericTable 入口自检程序。
''' </summary>
Module Program

    Private failures As Integer = 0

    Sub Main()
        Call Console.WriteLine("== ANOVA multivariate analysis / NumericTable smoketest ==")
        Call Console.WriteLine()

        Dim n As Integer = 12
        Dim m As Integer = 4
        Dim table As NumericTable = BuildTable(n, m)
        Dim probe As Double = table.features(0)(0)

        Call Section("input table")
        Call Check(table.nsamples = n AndAlso table.nfeatures = m, $"table dimension = [{table.nsamples} x {table.nfeatures}]")
        Call Check(table.HasLabel("class"), "table has the class label column")
        Call Check(table.GetResponseVector().Length = n, "GetResponseVector() falls back to the first label column")
        Call Check(table.GetResponseVector("class").Length = n, "GetResponseVector('class') reads the named label column")

        Call Section("pca")
        Dim pcaResult = table.pca(maxPC:=3)

        Call Check(pcaResult.analysis Is GetType(PCA), "analysis type is PCA")
        Call Check(pcaResult.StatisticsObject.YVariables IsNot Nothing, "response variable was passed through")

        Dim score As NumericTable = pcaResult.ScoreTable()
        Dim loading As NumericTable = pcaResult.LoadingTable()

        Call Check(score.nsamples = n, $"score table rows = {score.nsamples}")
        Call Check(score.nfeatures = 3, $"score table components = {score.nfeatures} ({score.featureNames.JoinBy(", ")})")
        Call Check(score.featureNames(0) = "PC1", "score column prefix is 'PC'")
        Call Check(score.GetRowName(0) = "sample_1", $"score row name = {score.GetRowName(0)}")
        Call Check(loading.nsamples = m, $"loading table rows = {loading.nsamples}")
        Call Check(loading.nfeatures = 3, $"loading table components = {loading.nfeatures}")
        Call Check(loading.GetRowName(3) = "f4", $"loading row name = {loading.GetRowName(3)}")

        Call Section("pls-da")
        Dim plsResult = table.plsda(component:=2)

        Call Check(plsResult.analysis Is GetType(PLS), "analysis type is PLS")

        Dim plsScore As NumericTable = plsResult.ScoreTable()
        Dim plsLoading As NumericTable = plsResult.LoadingTable()

        Call Check(plsResult.OptimizedFactor = 2, $"optimized factor = {plsResult.OptimizedFactor}")
        Call Check(plsScore.nsamples = n, $"score table rows = {plsScore.nsamples}")
        Call Check(plsScore.nfeatures >= 2, $"score table components = {plsScore.nfeatures} ({plsScore.featureNames.JoinBy(", ")})")
        Call Check(plsScore.featureNames(0) = "T1", "score column prefix is 'T'")
        Call Check(plsScore.HasLabel("Y predicted"), "score table carries the 'Y predicted' label column")
        Call Check(plsScore.HasLabel("Y experiment"), "score table carries the 'Y experiment' label column")
        Call Check(plsScore.GetLabel("Y predicted").Length = n, "predicted Y column length matches the sample size")
        Call Check(plsLoading.nsamples = m, $"loading table rows = {plsLoading.nsamples}")
        Call Check(plsLoading.HasLabel("VIP"), "loading table carries the 'VIP' label column")
        Call Check(plsLoading.HasLabel("Coefficients"), "loading table carries the 'Coefficients' label column")

        Call Section("opls-da")
        Dim oplsResult = table.oplsda(component:=1)

        Call Check(oplsResult.analysis Is GetType(OPLS), "analysis type is OPLS")

        Dim oplsScore As NumericTable = oplsResult.ScoreTable()
        Dim oplsLoading As NumericTable = oplsResult.LoadingTable()

        Call Check(oplsScore.nsamples = n, $"score table rows = {oplsScore.nsamples}")
        Call Check(oplsScore.nfeatures >= 2, $"score table components (T + To) = {oplsScore.nfeatures} ({oplsScore.featureNames.JoinBy(", ")})")
        Call Check(oplsScore.featureNames.Contains("To1"), "OPLS score table has the orthogonal component 'To1'")
        Call Check(oplsLoading.nsamples = m, $"loading table rows = {oplsLoading.nsamples}")
        Call Check(oplsLoading.HasLabel("VIP"), "OPLS loading table carries the 'VIP' label column")

        Call Section("source table is not modified")
        Call Check(table.features(0)(0) = probe, "the source feature matrix is unchanged after the analysis")

        Call Section("error handling")
        Call Check(Throws(Sub() table.AsStatisticsObject(y:="not-exists")), "a missing response column raises an error")

        Dim noLabel As New NumericTable(table.NumericRows(deepcopy:=True), table.rowNames, table.featureNames)
        Call Check(Throws(Sub() noLabel.plsda(component:=1)), "plsda reports a clear error when there is no response variable")
        Call Check(Not Throws(Sub() noLabel.AsStatisticsObject()), "AsStatisticsObject accepts a table without a response variable (for PCA)")

        Call Console.WriteLine()
        Call Console.WriteLine($"{(If(failures = 0, "PASS", "FAIL"))}: {failures} failure(s)")

        Environment.ExitCode = If(failures = 0, 0, 1)
    End Sub

    ''' <summary>
    ''' 构造一个具有两个分离类别的测试数据集
    ''' </summary>
    Private Function BuildTable(n As Integer, m As Integer) As NumericTable
        Dim rand As New Random(12345)
        Dim features As Double()() = New Double(n - 1)() {}
        Dim classes As Double() = New Double(n - 1) {}
        Dim rows As String() = New String(n - 1) {}
        Dim names As String() = New String(m - 1) {}

        For j As Integer = 0 To m - 1
            names(j) = $"f{j + 1}"
        Next

        For i As Integer = 0 To n - 1
            Dim grp As Integer = If(i < n \ 2, 1, 2)
            Dim row As Double() = New Double(m - 1) {}

            For j As Integer = 0 To m - 1
                row(j) = If(grp = 1, 1.0 + j * 0.2, 5.0 + j * 0.2) + (rand.NextDouble() - 0.5) * 0.4
            Next

            features(i) = row
            classes(i) = grp
            rows(i) = $"sample_{i + 1}"
        Next

        Dim table As New NumericTable(features, rows, names)

        ' 第一个标签列作为缺省的响应变量
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
