Imports System.Diagnostics
Imports System.Drawing
Imports System.IO
Imports Microsoft.VisualBasic.ApplicationServices.Debugging
Imports Microsoft.VisualBasic.Data
Imports Microsoft.VisualBasic.Data.Plots
Imports Microsoft.VisualBasic.Data.visualize
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.Statistics.Hypothesis.ANOVA
Imports std = System.Math

''' <summary>
''' 多元统计分析（PCA / PLS-DA / OPLS-DA）结果可视化的渲染演示。
''' 
''' <para>
''' 演示数据全部为合成数据：
''' </para>
''' 
''' <list type="bullet">
''' <item>PCA 用 3 组 × 8 个样本 × 12 个变量（其中 9 个带组特异效应），用于展示无监督的结构；</item>
''' <item>PLS-DA / OPLS-DA 用 2 组 × 10 个样本（其中 1 个样本被人为注入离群偏差），
''' 因为 <c>PLS</c> 是单响应（PLS1）实现，两分类判别才是它的典型用法。</item>
''' </list>
''' 
''' <para>
''' 本模块同时充当自检程序：所有 PNG 的导出结果都用
''' <see cref="DebugAudit"/> 记录下来，并在结束时生成 JSON 报告。
''' </para>
''' </summary>
Module MultivariateDemo

    ''' <summary>置换检验使用的置换次数（demo 用小值以保证运行时间可控）</summary>
    Private Const Permutations As Integer = 50

    ''' <summary>本模块自己的断言失败计数（框架内部的 SafeQuery 断言不属于本演示）</summary>
    Private m_failures As Integer = 0

    ''' <summary>
    ''' 依次渲染全部多元统计图表，并把 PNG 输出到 <paramref name="outDir"/> 下的
    ''' <c>multivariate/</c> 目录。
    ''' </summary>
    ''' <param name="outDir">输出目录（一般为 <c>AppContext.BaseDirectory</c>）</param>
    Friend Sub Run(outDir As String)
        Dim dir As String = IO.Path.Combine(outDir, "multivariate")

        Call Directory.CreateDirectory(dir)
        Call Console.WriteLine($"multivariate demo -> {dir}")

        Call DebugAudit.Mark("multivariate demo output", dir)

        ' ---- 合成数据 ----
        Dim pcaData As DemoData = BuildTable({"Control", "Treat-A", "Treat-B"}, 8, -1)
        Dim plsData As DemoData = BuildTable({"Control", "Treat-A"}, 10, -1)
        ' 离群诊断单独用一份注入了异常偏差的数据：这样得分图不会被离群点撑变形
        Dim diagData As DemoData = BuildTable({"Control", "Treat-A"}, 10, outlierIndex:=9)

        Call DebugAudit.Mark("pca samples", pcaData.samples.Length)
        Call DebugAudit.Mark("pls samples", plsData.samples.Length)
        Call DebugAudit.Mark("variables", plsData.variables.Length)

        ' ---- 分析 ----
        Dim watch As Stopwatch = Stopwatch.StartNew()
        Dim pcaResult = pcaData.table.pca(maxPC:=5)
        Dim plsResult = plsData.table.plsda(component:=2)
        Dim oplsResult = plsData.table.oplsda(component:=1)
        Dim diagResult = diagData.table.plsda(component:=2)

        Call watch.Stop()
        Call Console.WriteLine($"  analysis finished in {watch.ElapsedMilliseconds} ms")
        Call DebugAudit.Mark("analysis elapsed(ms)", watch.ElapsedMilliseconds)

        Call CheckResult(pcaResult, "pca")
        Call CheckResult(plsResult, "pls-da")
        Call CheckResult(oplsResult, "opls-da")

        ' ---- 得分图 ----
        Call RenderScorePlots(dir, pcaData, pcaResult, plsData, plsResult, oplsResult)

        ' ---- 方差解释 / 载荷 ----
        Call RenderVariancePlots(dir, pcaResult)

        ' ---- OPLS-DA 的 S-plot 与 VIP ----
        Call RenderImportancePlots(dir, plsResult, oplsResult)

        ' ---- 置换检验 ----
        Call RenderPermutation(dir, plsResult, oplsResult)

        ' ---- 模型诊断 ----
        Call RenderDiagnostics(dir, plsData, plsResult, diagData, diagResult)

        ' ---- 自检报告 ----
        Dim report As String = Path.Combine(dir, "multivariate-demo-audit.json")

        Call DebugAudit.Mark("demo assertion failures", m_failures)
        Call DebugAudit.SaveReport(report)
        Call File.WriteAllText(Path.Combine(dir, "multivariate-demo-audit.txt"), DebugAudit.Dump())

        Call Console.WriteLine($"  audit report -> {report}")
        Call Console.WriteLine($"  failed checks: {m_failures}")
        Call Console.WriteLine("  note: the framework level SafeQuery assertion is inverted,")
        Call Console.WriteLine("        so DebugAudit.FailureCount also contains unrelated records.")
    End Sub

#Region "得分图"

    Private Sub RenderScorePlots(dir As String,
                                 pcaData As DemoData, pcaResult As MultivariateAnalysisResult,
                                 plsData As DemoData, plsResult As MultivariateAnalysisResult,
                                 oplsResult As MultivariateAnalysisResult)

        ' PCA：默认 95% 统计置信椭圆
        Dim pcaPlot As New MultivariateScorePlot(1200, 900, pcaResult, 0, 1, pcaData.groups, pcaData.samples, True, PlotTheme.Light())

        pcaPlot.Title = "PCA score plot"
        pcaPlot.SubTitle = "synthetic 3 groups × 8 samples, 95% confidence ellipse"
        pcaPlot.ShowPointLabels = True
        pcaPlot.Ellipses.Mode = MultivariateEllipseMode.Confidence
        pcaPlot.Ellipses.ConfidenceLevel = 0.95F

        Call Export(pcaPlot, Path.Combine(dir, "pca-score-confidence.png"))

        ' PCA：k 倍标准差椭圆
        Dim sigmaPlot As New MultivariateScorePlot(1200, 900, pcaResult, 0, 1, pcaData.groups, pcaData.samples, True, PlotTheme.Light())

        sigmaPlot.Title = "PCA score plot"
        sigmaPlot.SubTitle = "2-sigma data ellipse (Geometry.EstimateEllipse)"
        sigmaPlot.Ellipses.Mode = MultivariateEllipseMode.StandardDeviation
        sigmaPlot.Ellipses.StandardDeviations = 2.0

        Call Export(sigmaPlot, Path.Combine(dir, "pca-score-2sigma.png"))

        ' PLS-DA：T1 / T2
        Dim plsPlot As New MultivariateScorePlot(1200, 900, plsResult, 0, 1, plsData.groups, plsData.samples, True, PlotTheme.Light())

        plsPlot.Title = "PLS-DA score plot"
        plsPlot.SubTitle = $"component T1 vs T2 (optimized factor = {plsResult.OptimizedFactor})"
        plsPlot.ShowPointLabels = True
        plsPlot.Ellipses.Mode = MultivariateEllipseMode.Confidence

        Call Export(plsPlot, Path.Combine(dir, "plsda-score.png"))

        ' OPLS-DA：t[1] / t_o[1]
        Dim oplsPlot As New MultivariateScorePlot(1200, 900, oplsResult, 0, 1, plsData.groups, plsData.samples, True, PlotTheme.Light())

        oplsPlot.Title = "OPLS-DA score plot"
        oplsPlot.SubTitle = $"predictive t[1] vs orthogonal t_o[1] ({oplsResult.OptimizedOrthoFactor} orthogonal component)"
        oplsPlot.ShowPointLabels = True
        oplsPlot.Ellipses.Mode = MultivariateEllipseMode.Confidence
        oplsPlot.Ellipses.EllipseType = PCAEllipseCalculator.EllipseType.T
        oplsPlot.LegendLocation = PlotEngine.LegendPos.UpperRight

        Call Export(oplsPlot, Path.Combine(dir, "oplsda-score.png"))
    End Sub

#End Region

#Region "方差解释与载荷"

    Private Sub RenderVariancePlots(dir As String, pcaResult As MultivariateAnalysisResult)
        ' 碎石图 + 累计贡献率
        Dim scree As New ScreeVariancePlot(1200, 800, pcaResult, PlotTheme.Light())

        scree.Title = "PCA scree plot"
        scree.SubTitle = "explained variance per component with cumulative curve"
        scree.CumulativeThreshold = 80

        Call Export(scree, Path.Combine(dir, "pca-scree.png"))

        ' 载荷图 + 双标图
        Dim scoreSet As MultivariateScoreSet = pcaResult.ToScoreSet(0, 1)
        Dim loadingPlot As New MultivariateLoadingPlot(1200, 900, pcaResult, 0, 1, Nothing, scoreSet, PlotTheme.Light())

        loadingPlot.Title = "PCA loading plot (biplot)"
        loadingPlot.SubTitle = "loading vectors scaled into the score space"
        loadingPlot.ShowBiplot = True
        loadingPlot.LegendLocation = PlotEngine.LegendPos.UpperLeft

        Call Export(loadingPlot, Path.Combine(dir, "pca-loading-biplot.png"))
    End Sub

#End Region

#Region "S-plot 与 VIP"

    Private Sub RenderImportancePlots(dir As String, plsResult As MultivariateAnalysisResult, oplsResult As MultivariateAnalysisResult)
        ' OPLS-DA 的 S-plot
        Dim splot As New MultivariateSPlot(1200, 900, oplsResult, Nothing, PlotTheme.Light())

        splot.Title = "OPLS-DA S-plot"
        splot.SubTitle = "p(1) versus p(corr) of the predictive component"
        splot.HighlightCorrelation = 0.5
        splot.MaxLabels = 14
        splot.LegendLocation = PlotEngine.LegendPos.UpperLeft

        Call Export(splot, Path.Combine(dir, "oplsda-splot.png"))

        ' VIP 图
        Dim vipPlot As New MultivariateVipPlot(1100, 1000, oplsResult, False, Nothing, PlotTheme.Light())

        vipPlot.Title = "OPLS-DA VIP"
        vipPlot.SubTitle = "variables sorted by VIP, threshold = 1"
        vipPlot.MaxBars = 12
        vipPlot.LegendLocation = PlotEngine.LegendPos.LowerRight

        Call Export(vipPlot, Path.Combine(dir, "oplsda-vip.png"))

        ' 回归系数图
        Dim coefPlot As New MultivariateVipPlot(1100, 1000, plsResult, True, Nothing, PlotTheme.Light())

        coefPlot.Title = "PLS-DA regression coefficients"
        coefPlot.SubTitle = "variables sorted by coefficient"
        coefPlot.MaxBars = 12
        coefPlot.LegendLocation = PlotEngine.LegendPos.UpperLeft

        Call Export(coefPlot, Path.Combine(dir, "plsda-coefficients.png"))

        Call Assert(plsResult.Vips.Count = plsResult.Coefficients.Count,
                    $"vip count ({plsResult.Vips.Count}) equals coefficient count ({plsResult.Coefficients.Count})")
        Call Assert(plsResult.Vips.Any(Function(v) v >= 1), "at least one variable has VIP >= 1")
    End Sub

#End Region

#Region "置换检验"

    Private Sub RenderPermutation(dir As String, plsResult As MultivariateAnalysisResult, oplsResult As MultivariateAnalysisResult)
        Dim watch As Stopwatch = Stopwatch.StartNew()
        Dim progress As Action(Of Integer) =
            Sub(done As Integer)
                If done Mod 10 = 0 Then
                    Call Console.WriteLine($"    permutation {done}/{Permutations}")
                End If
            End Sub

        Dim plsPerm As PermutationTestResult = PermutationTest.Test(
            plsResult.StatisticsObject,
            numPermutations:=Permutations,
            component:=2,
            method:="pls",
            seed:=20261115,
            parallel:=True,
            progress:=progress)

        Call watch.Stop()
        Call Console.WriteLine($"  pls permutation finished in {watch.ElapsedMilliseconds} ms: {plsPerm}")
        Call DebugAudit.Mark("pls permutation elapsed(ms)", watch.ElapsedMilliseconds)

        Call Assert(plsPerm.IsValid, $"pls permutation is valid ({plsPerm.InvalidReason})")
        Call Assert(plsPerm.PValue > 0 AndAlso plsPerm.PValue <= 1,
                    $"pls permutation p-value in (0, 1]: {plsPerm.PValue}")
        Call Assert(Not Double.IsNaN(plsPerm.R2YIntercept),
                    $"pls permutation R2Y intercept = {plsPerm.R2YIntercept}")

        Dim plt As New MultivariatePermutationPlot(1300, 900, plsPerm, PlotTheme.Light())

        plt.Title = "PLS-DA permutation test"
        plt.SubTitle = $"{Permutations} permutations, R2Y / Q2 versus permuted correlation"
        plt.ShowOriginalLines = True
        plt.LegendLocation = PlotEngine.LegendPos.LowerRight

        Call Export(plt, Path.Combine(dir, "plsda-permutation.png"))

        ' OPLS 的置换检验（正交成分数固定为 1）
        watch.Restart()

        Dim oplsPerm As PermutationTestResult = PermutationTest.Test(
            oplsResult.StatisticsObject,
            numPermutations:=std.Max(10, Permutations \ 2),
            component:=1,
            method:="opls",
            seed:=20261115,
            parallel:=True)

        Call watch.Stop()
        Call Console.WriteLine($"  opls permutation finished in {watch.ElapsedMilliseconds} ms: {oplsPerm}")
        Call DebugAudit.Mark("opls permutation elapsed(ms)", watch.ElapsedMilliseconds)
        Call Assert(oplsPerm.IsValid, $"opls permutation is valid ({oplsPerm.InvalidReason})")

        Dim oplsPlt As New MultivariatePermutationPlot(1300, 900, oplsPerm, PlotTheme.Light())

        oplsPlt.Title = "OPLS-DA permutation test"
        oplsPlt.SubTitle = $"method = opls, orthogonal components = {oplsResult.OptimizedOrthoFactor}"
        oplsPlt.LegendLocation = PlotEngine.LegendPos.LowerRight

        Call Export(oplsPlt, Path.Combine(dir, "oplsda-permutation.png"))

        ' 非法方法名应当得到一个 IsValid=False 的结果而不是抛异常
        Dim rejected As PermutationTestResult = PermutationTest.Test(plsResult.StatisticsObject, method:="not-a-method")

        Call Assert(Not rejected.IsValid, "unknown method returns an invalid result instead of throwing")
        Call Assert(plsPerm.Correlation.Length = Permutations,
                    $"permutation correlation series length = {plsPerm.Correlation.Length}")
        Call Assert(plsPerm.R2Y.Length = Permutations AndAlso plsPerm.Q2.Length = Permutations,
                    $"permutation R2Y/Q2 series lengths = {plsPerm.R2Y.Length}/{plsPerm.Q2.Length}")
    End Sub

#End Region

#Region "模型诊断"

    Private Sub RenderDiagnostics(dir As String, data As DemoData, plsResult As MultivariateAnalysisResult,
                                  diagData As DemoData, diagResult As MultivariateAnalysisResult)
        ' 预测值对实测值
        Dim prediction As New MultivariatePredictionPlot(1100, 900, plsResult, data.groups, data.samples, PlotTheme.Light())

        prediction.Title = "PLS-DA prediction"
        prediction.SubTitle = $"predicted versus measured Y (RMSEE = {plsResult.Rmsee.ToString("F3")})"
        prediction.LegendLocation = PlotEngine.LegendPos.LowerRight

        Call Export(prediction, Path.Combine(dir, "plsda-prediction.png"))

        ' Hotelling T2（离群诊断使用注入了异常偏差的那份数据）
        Dim t2Plot As New MultivariateOutlierPlot(1200, 800, diagResult, MultivariateOutlierMetric.HotellingT2,
                                                  diagData.groups, diagData.samples, PlotTheme.Light())

        t2Plot.Title = "Hotelling T2 diagnostic"
        t2Plot.SubTitle = "95% control limit from the F distribution"

        Call Export(t2Plot, Path.Combine(dir, "plsda-hotelling-t2.png"))

        ' DModX
        Dim dmodxPlot As New MultivariateOutlierPlot(1200, 800, diagResult, MultivariateOutlierMetric.DModX,
                                                     diagData.groups, diagData.samples, PlotTheme.Light())

        dmodxPlot.Title = "DModX diagnostic"
        dmodxPlot.SubTitle = "residual space distance with chi-square control limit"

        Call Export(dmodxPlot, Path.Combine(dir, "plsda-dmodx.png"))

        Dim outliers As MultivariateOutlierSeries = diagResult.ToOutlierSeries(diagData.groups, diagData.samples)

        Call Assert(outliers.Size = diagData.samples.Length, $"outlier series size = {outliers.Size}")
        Call Assert(Not Double.IsNaN(outliers.T2Threshold), $"T2 threshold = {outliers.T2Threshold.ToString("F3")}")
        Call Assert(Not Double.IsNaN(outliers.DModXThreshold), $"DModX threshold = {outliers.DModXThreshold.ToString("F3")}")
        Call Assert(outliers.DModX.All(Function(v) v >= 0), "all DModX values are non-negative")
        Call Assert(outliers.Outliers.Any(Function(flag) flag), "the injected outlier sample is detected")
    End Sub

#End Region

#Region "渲染与自检"

    Private Sub Export(plot As PlotEngine, file As String)
        Try
            Call plot.Plot()
            Call plot.SavePng(file, 300)

            ' 参数名 file 会遮蔽 System.IO.File（VB 不区分大小写），因此这里写全名
            Dim length As Long = If(IO.File.Exists(file), New FileInfo(file).Length, 0)

            Call Assert(length > 1024, $"png exported: {Path.GetFileName(file)} ({length} bytes)")
            Call Console.WriteLine($"  png -> {Path.GetFileName(file)} ({length} bytes)")
        Catch ex As Exception
            Call Assert(False, $"render failed: {Path.GetFileName(file)} -> {ex.Message}")
            Call Console.WriteLine($"  !! render failed: {Path.GetFileName(file)} -> {ex.Message}")
        Finally
            Call plot.Dispose()
        End Try
    End Sub

    ''' <summary>
    ''' 记录一条本演示自己的断言（同时写入 <see cref="DebugAudit"/> 报告）。
    ''' </summary>
    Private Sub Assert(condition As Boolean, message As String)
        If Not condition Then
            m_failures += 1
        End If

        Call DebugAudit.Check(condition, message)
    End Sub

    Private Sub CheckResult(result As MultivariateAnalysisResult, name As String)
        Call Assert(result IsNot Nothing, $"{name}: result is not nothing")

        If result Is Nothing Then
            Return
        End If

        Call Assert(result.TPreds.Count > 0, $"{name}: score vectors = {result.TPreds.Count}")
        Call Console.WriteLine(
            $"  {name}: scores={result.TPreds.Count} loadings={result.PPreds.Count} " &
            $"R2Y={MultivariateMetrics.R2Y(result).ToString("F3")} Q2={MultivariateMetrics.Q2(result).ToString("F3")}")
    End Sub

#End Region

#Region "合成数据"

    Private Structure DemoData
        Public table As NumericTable
        Public samples As String()
        Public groups As String()
        Public variables As String()
    End Structure

    ''' <summary>
    ''' 构造合成数据：每 3 个变量构成一组「标志物模式」，其中前 3/4 的变量带组特异效应，
    ''' 其余变量是纯噪声。
    ''' </summary>
    ''' <param name="groupNames">分组名称（2 个或者 3 个）</param>
    ''' <param name="replicates">每组的重复样本数</param>
    ''' <param name="outlierIndex">需要注入离群偏差的样本下标，&lt; 0 表示不注入</param>
    Private Function BuildTable(groupNames As String(), replicates As Integer, outlierIndex As Integer) As DemoData
        Dim nVariables As Integer = 12
        Dim effectVariables As Integer = 9
        Dim rnd As New Random(20261115)
        Dim lastGroup As Integer = groupNames.Length - 1

        ' 每个变量在每个分组上的效应量
        Dim effects(nVariables - 1, lastGroup) As Double

        For j As Integer = 0 To effectVariables - 1
            ' 每个变量的效应量再乘一个 [0.8, 1.4] 的随机因子：
            ' 否则同一模式的变量会在 S-plot / VIP 图里完全重合
            Dim scale As Double = 0.8 + 0.6 * rnd.NextDouble()

            Select Case j Mod 3
                Case 0
                    effects(j, lastGroup) = 1.45 * scale
                Case 1
                    effects(j, lastGroup) = -1.25 * scale
                Case 2
                    effects(j, std.Min(1, lastGroup)) = 1.3 * scale
            End Select
        Next

        ' 每个变量的噪声水平不同：这样标准化之后各变量的效应/噪声比才会不同，
        ' S-plot 与 VIP 图里的变量才不会全部重合成一个点
        Dim noiseScales(nVariables - 1) As Double

        For j As Integer = 0 To nVariables - 1
            noiseScales(j) = 0.5 + 1.6 * rnd.NextDouble()
        Next

        Dim nSamples As Integer = groupNames.Length * replicates
        Dim features(nSamples - 1)() As Double
        Dim samples(nSamples - 1) As String
        Dim groups(nSamples - 1) As String
        Dim classes(nSamples - 1) As Double
        Dim cursor As Integer = 0

        For g As Integer = 0 To lastGroup
            For k As Integer = 1 To replicates
                Dim values(nVariables - 1) As Double
                Dim isOutlier As Boolean = cursor = outlierIndex

                For j As Integer = 0 To nVariables - 1
                    Dim baseline As Double = 0.8 + 0.1 * (j Mod 5)
                    Dim noise As Double = (rnd.NextDouble() - 0.5) * 0.55 * noiseScales(j)
                    Dim value As Double = baseline + effects(j, g) + noise

                    If isOutlier AndAlso j >= effectVariables Then
                        ' 只在「纯噪声变量」上注入偏差：这个方向与判别方向正交，
                        ' 因此样本会在残差空间 (DModX) 里明显偏离，却几乎不影响得分图
                        value += 3.0 + 0.5 * (j Mod 3)
                    End If

                    values(j) = std.Max(0.01, value)
                Next

                features(cursor) = values
                samples(cursor) = $"{groupNames(g)}_{k}"
                groups(cursor) = groupNames(g)
                classes(cursor) = g + 1
                cursor += 1
            Next
        Next

        Dim variables(nVariables - 1) As String

        For j As Integer = 0 To nVariables - 1
            variables(j) = "F" & (j + 1).ToString("00")
        Next

        Dim table As New NumericTable(features, samples, variables)

        Call table.SetLabel("class", classes)

        Return New DemoData With {
            .table = table,
            .samples = samples,
            .groups = groups,
            .variables = variables
        }
    End Function

#End Region

End Module
