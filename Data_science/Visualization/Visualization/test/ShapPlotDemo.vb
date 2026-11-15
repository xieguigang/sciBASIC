Imports System.Drawing
Imports System.IO
Imports Microsoft.VisualBasic.ApplicationServices.Debugging
Imports Microsoft.VisualBasic.Data.Plots
Imports Microsoft.VisualBasic.Data.visualize.ShapPlot
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Math.Statistics.ShapleyValue
Imports NumericTable = Microsoft.VisualBasic.Data.NumericTable
Imports std = System.Math

''' <summary>
''' SHAP 结果图（ShapPlot）的演示程序。
''' 
''' 使用内置的确定性合成数据（120 个样本 × 10 个特征，其中前 3 个为有效特征）构造一个
''' <see cref="ShapAnalysisResult"/> 与配套的 <see cref="NumericTable"/>，
''' 然后把 readme 中描述的 8 类 SHAP 图表全部渲染并导出为 PNG：
''' 
''' 1. 摘要蜂群图（beeswarm）
''' 2. 特征重要性柱状图（bar）
''' 3. 单样本瀑布图（waterfall）
''' 4. 力导向图（force）
''' 5. 依赖图（dependence）
''' 6. 决策图（decision）
''' 7. 热力图（heatmap）
''' 8. 小提琴图（violin）
''' 
''' 每张图渲染完成之后都会用 <see cref="DebugAudit"/> 断言「文件存在且内容非空」，
''' 最后输出 <c>shap-plot-report.json</c> 审计报告。
''' </summary>
Module ShapPlotDemo

    ''' <summary>样本数量。</summary>
    Private Const NSamples As Integer = 120
    ''' <summary>特征数量。</summary>
    Private Const NFeatures As Integer = 10
    ''' <summary>画布宽度。</summary>
    Private Const CanvasWidth As Integer = 1200
    ''' <summary>画布高度。</summary>
    Private Const CanvasHeight As Integer = 800
    ''' <summary>本演示应当导出的图表数量。</summary>
    Private Const ExpectedCharts As Integer = 8

    Private outputDir As String
    ''' <summary>已成功导出的图表数量。</summary>
    Private rendered As Integer = 0

    Sub Main(args As String())
        ' 光栅绘图驱动必须在任何渲染动作之前注册
        Call ImageDriver.Register()

        Try
            Console.OutputEncoding = System.Text.Encoding.UTF8
        Catch
            ' 控制台重定向的场景下设置编码可能失败，忽略即可
        End Try

        ' 只有命令行参数确实是一个已经存在的绝对路径目录时才作为输出目录使用
        ' （Visual Studio 的 launchSettings 可能会塞进与本次演示无关的参数）
        If args IsNot Nothing AndAlso args.Length > 0 AndAlso
           Path.IsPathRooted(args(0)) AndAlso Directory.Exists(args(0)) Then
            outputDir = args(0)
        Else
            outputDir = Path.Combine(AppContext.BaseDirectory, "shap_plots")
        End If

        Call Directory.CreateDirectory(outputDir)

        Console.WriteLine($"SHAP 可视化演示")
        Console.WriteLine($"输出目录: {outputDir}")
        Console.WriteLine($"合成数据: {NSamples} 个样本 × {NFeatures} 个特征")
        Console.WriteLine()

        Dim data = BuildSynthetic()
        Dim theme As PlotTheme = PlotTheme.Light()

        Console.WriteLine("正在渲染图表 ...")

        ' ---------------------------------------------------------------
        ' 1. 全局：摘要蜂群图
        ' ---------------------------------------------------------------
        Using plot As New ShapBeeswarmPlot(data.shap, data.features, CanvasWidth, CanvasHeight, theme)
            plot.Title = "SHAP summary plot (beeswarm)"
            plot.MaxDisplay = 10
            Call Render(plot, "beeswarm")
        End Using

        ' ---------------------------------------------------------------
        ' 2. 全局：特征重要性柱状图
        ' ---------------------------------------------------------------
        Using plot As New ShapBarPlot(data.shap, data.features, CanvasWidth, CanvasHeight, theme)
            plot.Title = "SHAP feature importance (mean |SHAP value|)"
            plot.MaxDisplay = 10
            Call Render(plot, "bar")
        End Using

        ' ---------------------------------------------------------------
        ' 3. 局部：单样本瀑布图
        ' ---------------------------------------------------------------
        Using plot As New ShapWaterfallPlot(data.shap, data.features, CanvasWidth, CanvasHeight, theme)
            plot.SampleId = "S0001"
            plot.MaxDisplay = 10
            Call Render(plot, "waterfall")
        End Using

        ' ---------------------------------------------------------------
        ' 4. 局部：力导向图
        ' ---------------------------------------------------------------
        Using plot As New ShapForcePlot(data.shap, data.features, CanvasWidth, CanvasHeight, theme)
            plot.SampleId = "S0001"
            plot.MaxDisplay = 10
            Call Render(plot, "force")
        End Using

        ' ---------------------------------------------------------------
        ' 5. 交互：依赖图
        ' ---------------------------------------------------------------
        Using plot As New ShapDependencePlot(data.shap, data.features, "F1", CanvasWidth, CanvasHeight, theme)
            plot.ShowTrendLine = True
            Call Render(plot, "dependence")
        End Using

        ' ---------------------------------------------------------------
        ' 6. 局部/多样本：决策图
        ' ---------------------------------------------------------------
        Using plot As New ShapDecisionPlot(data.shap, data.features, CanvasWidth, CanvasHeight, theme)
            plot.Title = "SHAP decision plot"
            plot.MaxSamples = 20
            plot.MaxDisplay = 10
            Call Render(plot, "decision")
        End Using

        ' ---------------------------------------------------------------
        ' 7. 全局：热力图（简化版）
        ' ---------------------------------------------------------------
        Using plot As New ShapHeatmapPlot(data.shap, data.features, CanvasWidth, CanvasHeight, theme)
            plot.Title = "SHAP heatmap (samples × features)"
            plot.MaxDisplay = 10
            Call Render(plot, "heatmap")
        End Using

        ' ---------------------------------------------------------------
        ' 8. 全局：小提琴图（简化版）
        ' ---------------------------------------------------------------
        Using plot As New ShapViolinPlot(data.shap, data.features, CanvasWidth, CanvasHeight, theme)
            plot.Title = "SHAP value distribution (violin)"
            plot.MaxDisplay = 10
            Call Render(plot, "violin")
        End Using

        ' ---------------------------------------------------------------
        ' 审计报告
        ' ---------------------------------------------------------------
        Dim report As String = Path.Combine(outputDir, "shap-plot-report.json")

        Call DebugAudit.SaveReport(report)

        Console.WriteLine()
        Console.WriteLine($"审计报告: {report}")
        Console.WriteLine($"导出图表数量: {rendered}/{ExpectedCharts}")
        Console.WriteLine($"断言失败次数: {DebugAudit.FailureCount}")
        Console.WriteLine("  注：框架内部的 SafeQuery 断言方向与语义相反（序列非空时反而记为失败），")
        Console.WriteLine("      因此上面的失败计数包含了若干与本次演示无关的框架级记录。")
        Console.WriteLine(DebugAudit.Dump(includeRecords:=False))

        Call DebugAudit.Check(rendered = ExpectedCharts, $"只成功导出了 {rendered}/{ExpectedCharts} 张图表!")

        Console.WriteLine("全部图表已完成。")
    End Sub

    ''' <summary>
    ''' 渲染并导出单张图表，同时用 <see cref="DebugAudit"/> 断言产物有效。
    ''' </summary>
    ''' <param name="plot">已经配置好的图表对象（必须由 <c>New(width, height)</c> 构造）</param>
    ''' <param name="name">输出文件的基名（不含扩展名）</param>
    Private Sub Render(plot As PlotEngine, name As String)
        ' 注意：局部变量不能叫 file，否则 VB 会把 File.Exists 解析成对局部变量的访问
        Dim filePath As String = Path.Combine(outputDir, $"{name}.png")

        Call plot.Plot()
        Call plot.SavePng(filePath, 150)

        Call DebugAudit.Check(File.Exists(filePath), $"{name}: 图形文件没有生成 {filePath}!")

        If File.Exists(filePath) Then
            Dim length As Long = New FileInfo(filePath).Length

            Call DebugAudit.Check(length > 0, $"{name}: 图形文件内容为空 {filePath}!")
            Console.WriteLine($"  {name,-12} -> {filePath}  ({length / 1024} KB)")

            rendered += 1
        End If
    End Sub

    ''' <summary>
    ''' 构造确定性的合成数据：120 个样本 × 10 个特征，其中 F1~F3 为有效特征。
    ''' 
    ''' 贡献值按 <c>phi_j = beta_j * (x_j - E[x_j]) + 噪声</c> 生成，
    ''' 从而保证加性分解与模型输出一致（回归任务，<c>Output = Baseline + Σ phi</c>）。
    ''' </summary>
    Private Function BuildSynthetic() As (shap As ShapAnalysisResult, features As NumericTable)
        Dim rnd As New Random(42)
        Dim betas As Double() = {1.6, -1.1, 0.7, 0.35, -0.25, 0.18, 0.12, -0.08, 0.05, 0.02}
        Dim featureNames As String() = Enumerable.Range(1, NFeatures).Select(Function(i) $"F{i}").ToArray()
        Dim rowNames As String() = Enumerable.Range(1, NSamples).Select(Function(i) $"S{i.ToString("D4")}").ToArray()
        Dim x As Double()() = New Double(NSamples - 1)() {}

        For i As Integer = 0 To NSamples - 1
            Dim row As Double() = New Double(NFeatures - 1) {}

            For j As Integer = 0 To NFeatures - 1
                ' 前三个特征保留一点非线性关系，便于观察依赖图的形态
                Dim value As Double = rnd.NextDouble() * 2 - 1

                If j = 1 Then
                    value = value * std.Abs(value)
                End If

                row(j) = value
            Next

            x(i) = row
        Next

        ' 特征均值（用于中心化贡献值）
        Dim means As Double() = New Double(NFeatures - 1) {}

        For j As Integer = 0 To NFeatures - 1
            Dim column As Double = 0

            For i As Integer = 0 To NSamples - 1
                column += x(i)(j)
            Next

            means(j) = column / NSamples
        Next

        Const baseline As Double = 0.35
        Dim explanations As New List(Of ShapExplanation)()

        For i As Integer = 0 To NSamples - 1
            Dim contributions As Double() = New Double(NFeatures - 1) {}

            For j As Integer = 0 To NFeatures - 1
                contributions(j) = betas(j) * (x(i)(j) - means(j)) + (rnd.NextDouble() - 0.5) * 0.05
            Next

            Dim additive As Double = baseline + contributions.Sum()

            Call explanations.Add(New ShapExplanation With {
                .ID = rowNames(i),
                .FeatureNames = featureNames,
                .Contributions = contributions,
                .Baseline = baseline,
                .AdditiveValue = additive,
                .Output = additive
            })
        Next

        Dim meanAbs As Double() = New Double(NFeatures - 1) {}
        Dim meanShap As Double() = New Double(NFeatures - 1) {}

        For j As Integer = 0 To NFeatures - 1
            Dim absSum As Double = 0
            Dim signedSum As Double = 0

            For Each item As ShapExplanation In explanations
                absSum += std.Abs(item.Contributions(j))
                signedSum += item.Contributions(j)
            Next

            meanAbs(j) = absSum / explanations.Count
            meanShap(j) = signedSum / explanations.Count
        Next

        Dim result As New ShapAnalysisResult With {
            .ModelName = "synthetic-linear",
            .DatasetName = "shap-plot-demo",
            .IsClassification = False,
            .FeatureNames = featureNames,
            .Baseline = baseline,
            .Explanations = explanations,
            .MeanAbsShap = meanAbs,
            .MeanShap = meanShap
        }

        Return (result, New NumericTable(x, rowNames, featureNames))
    End Function

End Module
