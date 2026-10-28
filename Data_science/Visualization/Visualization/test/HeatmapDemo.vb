Imports System.Drawing
Imports System.Text
Imports Microsoft.VisualBasic.Data
Imports Microsoft.VisualBasic.Data.Plots
Imports Microsoft.VisualBasic.Data.visualize
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Font = Microsoft.VisualBasic.Imaging.Font

''' <summary>
''' 复合热图（<see cref="ComplexHeatMap"/>）的渲染演示。
'''
''' <para>
''' 版式参照典型的 R pheatmap 复合热图（行 z-score 后截断到 ±2、
''' 蓝-白-红发散色阶、顶部列聚类树、左侧行聚类树、列注释条、
''' 右侧行标签、竖直 colorbar 与分类图例、底部竖排列标签）。
''' </para>
'''
''' <para>
''' 演示数据全部为合成数据：30 个行特征 × 15 个样本（5 组 × 3 重复），
''' 组别用占位的 Group A ~ Group E 表示。
''' </para>
''' </summary>
Module HeatmapDemo

    ''' <summary>
    ''' 注册底层图形驱动（必须早于任何渲染动作）。
    ''' </summary>
    Sub New()
        Call ImageDriver.Register()
    End Sub

    ''' <summary>
    ''' 依次渲染复合热图演示与三维图表演示。
    '''
    ''' <para>
    ''' 可以用命令行参数选择只跑其中一部分：
    ''' <c>--heatmap</c> 只跑热图，<c>--3d</c> 只跑三维图表，缺省两者都跑。
    ''' </para>
    ''' </summary>
    Sub Main()
        Dim outDir As String = AppContext.BaseDirectory
        Dim args As String() = Environment.GetCommandLineArgs()
        Dim only3D As Boolean = args.Any(Function(a) a = "--3d")
        Dim onlyHeatmap As Boolean = args.Any(Function(a) a = "--heatmap")

        Call Console.WriteLine($"output directory: {outDir}")

        If Not only3D Then
            Call RenderMatrixDemo(outDir)
            Call RenderTableDemo(outDir)
            Call RenderCsvDemo(outDir)
            Call RenderEdgeCases(outDir)
        End If

        If Not onlyHeatmap Then
            Call Plot3DDemo.Run(outDir)
        End If

        Call Console.WriteLine("done.")
    End Sub

#Region "演示 1：直接给矩阵（带列注释的复合热图）"

    Private Sub RenderMatrixDemo(outDir As String)
        Dim sample As SampleData = CreateSample()

        Using plt As New ComplexHeatMap(1074, 955, PlotTheme.Light())
            Call plt.ColAnnotations.Add(
                HeatMapAnnotation.Discrete("Group", sample.groups, GroupColors()))

            plt.Matrix = sample.matrix
            plt.RowLabels = sample.compounds
            plt.ColLabels = sample.samples
            plt.ScaleMode = HeatMapScale.Row
            plt.MinValue = -2
            plt.MaxValue = 2
            plt.RowLinkage = LinkageMethod.Ward
            plt.ColLinkage = LinkageMethod.Ward
            plt.RowLabelFont = New Font(PlotTheme.DefaultFontFamily, 9.5F)
            plt.ColLabelFont = New Font(PlotTheme.DefaultFontFamily, 10.0F)
            plt.Plot()

            Dim file As String = IO.Path.Combine(outDir, "complex-heatmap.png")

            Call plt.SavePng(file)

            Call Console.WriteLine($"matrix demo  -> {file}")
            Call Console.WriteLine($"  row order  : {String.Join(" | ", plt.RowOrder.Take(4))} ...")
            Call Console.WriteLine($"  col order  : {String.Join(" | ", plt.ColOrder)}")
            Call Console.WriteLine($"  body       : {plt.HeatmapBody}")
        End Using
    End Sub

#End Region

#Region "演示 2：从 NumericTable 构建"

    Private Sub RenderTableDemo(outDir As String)
        Dim sample As SampleData = CreateSample()
        ' 行 = 化合物、列 = 样本（与参考图的朝向一致）
        Dim table As New NumericTable(
            ToJagged(sample.matrix),
            sample.compounds,
            sample.samples)

        ' 数值标签列 → 连续行注释（示例：每个化合物在全部样本上的平均强度）
        Dim totals(sample.compounds.Length - 1) As Double

        For c As Integer = 0 To sample.compounds.Length - 1
            For s As Integer = 0 To sample.samples.Length - 1
                totals(c) += sample.matrix(c, s)
            Next

            totals(c) /= sample.samples.Length
        Next

        Call table.SetLabel("mean.intensity", totals)

        Using plt As ComplexHeatMap = ComplexHeatMap.FromTable(table, width:=1074, height:=900)
            plt.ScaleMode = HeatMapScale.Row
            plt.MinValue = -2
            plt.MaxValue = 2
            plt.Plot()

            Dim file As String = IO.Path.Combine(outDir, "complex-heatmap-table.png")

            Call plt.SavePng(file)

            Call Console.WriteLine($"table demo   -> {file}")
            Call Console.WriteLine($"  row annotations: {String.Join(", ", plt.RowAnnotations.Select(Function(a) a.Name))}")
        End Using
    End Sub

#End Region

#Region "演示 3：从 csv 构建（文本列自动成为离散注释）"

    Private Sub RenderCsvDemo(outDir As String)
        Dim sample As SampleData = CreateSample()
        Dim file As String = IO.Path.Combine(outDir, "complex-heatmap-table.csv")
        Dim sb As New StringBuilder

        Call sb.Append("sample,Group")

        For c As Integer = 0 To sample.compounds.Length - 1
            Call sb.Append("," & sample.compounds(c))
        Next

        Call sb.AppendLine()

        For s As Integer = 0 To sample.samples.Length - 1
            Call sb.Append($"{sample.samples(s)},{sample.groups(s)}")

            For c As Integer = 0 To sample.compounds.Length - 1
                Call sb.Append("," & sample.matrix(c, s).ToString("F4"))
            Next

            Call sb.AppendLine()
        Next

        Call IO.File.WriteAllText(file, sb.ToString(), Encoding.UTF8)

        ' csv 的布局是「行=样本、列=化合物」，因此这里把行列互换后再画
        Using plt As ComplexHeatMap = ComplexHeatMap.FromCsv(file, width:=1074, height:=900)
            plt.Matrix = Transpose(plt.Matrix)
            plt.RowLabels = sample.compounds
            plt.ColLabels = sample.samples

            ' csv 中的文本列（Group）是对样本的注释，行列互换之后应作为列注释
            Dim group As HeatMapAnnotation = plt.RowAnnotations(0)

            plt.RowAnnotations.Clear()
            Call plt.ColAnnotations.Add(HeatMapAnnotation.Discrete("Group", group.Values, GroupColors()))

            plt.ScaleMode = HeatMapScale.Row
            plt.MinValue = -2
            plt.MaxValue = 2
            plt.Plot()

            Dim png As String = IO.Path.Combine(outDir, "complex-heatmap-csv.png")

            Call plt.SavePng(png)

            Call Console.WriteLine($"csv demo     -> {png}")
            Call Console.WriteLine($"  csv file   : {file}")
            Call Console.WriteLine($"  col annotations: {String.Join(", ", plt.ColAnnotations.Select(Function(a) a.Name))}")
        End Using
    End Sub

#End Region

#Region "演示 4：边界场景（不崩即通过）"

    Private Sub RenderEdgeCases(outDir As String)
        Call EdgeCase(outDir, "edge-nan", CreateNaNMatrix(), {"r1", "r2", "r3", "r4"}, {"c1", "c2", "c3"},
                       Sub(p)
                       End Sub)

        Call EdgeCase(outDir, "edge-equal", CreateEqualMatrix(), {"r1", "r2", "r3"}, {"c1", "c2", "c3", "c4"},
                       Sub(p)
                           p.ShowColDendrogram = True
                       End Sub)

        Call EdgeCase(outDir, "edge-single-row", CreateSingleRow(), {"only"}, {"c1", "c2", "c3", "c4"},
                       Sub(p)
                       End Sub)

        ' 只画行树、不做列聚类，并且把行标签放到左侧
        Call EdgeCase(outDir, "edge-row-tree-only", CreateEqualMatrix(), {"r1", "r2", "r3"}, {"c1", "c2", "c3", "c4"},
                       Sub(p)
                           p.AutoClusterColumns = False
                           p.ShowColDendrogram = False
                           p.ColLabelAngle = 0
                           p.RowLabelsOnRight = False
                       End Sub)

        ' 外部传入一棵叶节点名称不匹配的树：应当回退成原始顺序而不是崩溃
        Call EdgeCase(outDir, "edge-foreign-tree", CreateEqualMatrix(), {"r1", "r2", "r3"}, {"c1", "c2", "c3", "c4"},
                       Sub(p)
                           p.RowTree = ComplexHeatMap.BuildTree(
                               New Double()() {New Double() {1.0, 2.0}, New Double() {3.0, 4.0}},
                               New String() {"x", "y"},
                               LinkageMethod.AverageLinkage)
                       End Sub)
    End Sub

    Private Sub EdgeCase(outDir As String, name As String, matrix As Double(,), rowNames As String(), colNames As String(),
                         configure As Action(Of ComplexHeatMap))
        Using plt As New ComplexHeatMap(600, 460, PlotTheme.Light())
            plt.Matrix = matrix
            plt.RowLabels = rowNames
            plt.ColLabels = colNames
            plt.MinValue = -2
            plt.MaxValue = 2

            Call configure(plt)
            plt.Plot()
            Call plt.SavePng(IO.Path.Combine(outDir, name & ".png"))

            Call Console.WriteLine($"edge case    -> {name}.png (rows={plt.RowOrder.Length}, cols={plt.ColOrder.Length})")
            Call Console.WriteLine($"  rows: {String.Join(",", plt.RowOrder)} / cols: {String.Join(",", plt.ColOrder)} / body: {plt.HeatmapBody}")
        End Using
    End Sub

    Private Function CreateNaNMatrix() As Double(,)
        Dim m(3, 2) As Double

        For i As Integer = 0 To 3
            For j As Integer = 0 To 2
                m(i, j) = i * 3 + j
            Next
        Next

        m(0, 0) = Double.NaN
        m(1, 2) = Double.NaN

        Return m
    End Function

    Private Function CreateEqualMatrix() As Double(,)
        Dim m(2, 3) As Double

        For i As Integer = 0 To 2
            For j As Integer = 0 To 3
                m(i, j) = 5
            Next
        Next

        Return m
    End Function

    Private Function CreateSingleRow() As Double(,)
        Return New Double(,) {{1.0, 2.0, 3.0, 4.0}}
    End Function

#End Region

#Region "示例数据"

    Private Structure SampleData
        Public matrix As Double(,)
        Public compounds As String()
        Public samples As String()
        Public groups As String()
    End Structure

    ''' <summary>
    ''' 演示用的 5 组分组配色（Group A 红 / Group B 橙 / Group C 蓝 / Group D 绿 / Group E 灰）。
    ''' </summary>
    Private Function GroupColors() As Dictionary(Of String, Color)
        Return New Dictionary(Of String, Color) From {
            {"Group A", ColorTranslator.FromHtml("#C0392B")},
            {"Group B", ColorTranslator.FromHtml("#E67E22")},
            {"Group C", ColorTranslator.FromHtml("#2471A3")},
            {"Group D", ColorTranslator.FromHtml("#1E8449")},
            {"Group E", ColorTranslator.FromHtml("#7F8C8D")}
        }
    End Function

    ''' <summary>
    ''' 构造 30 个行特征 × 15 个样本（5 组 × 3 重复）的示例矩阵：
    ''' 每 5 个特征共享一种「组特异高/低丰度」模式，因此行聚类应当稳定地还原出这些分组。
    ''' </summary>
    Private Function CreateSample() As SampleData
        Dim compounds As String() = {
            "Feature01", "Feature02", "Feature03", "Feature04", "Feature05",
            "Feature06", "Feature07", "Feature08", "Feature09", "Feature10",
            "Feature11", "Feature12", "Feature13", "Feature14", "Feature15",
            "Feature16", "Feature17", "Feature18", "Feature19", "Feature20",
            "Feature21", "Feature22", "Feature23", "Feature24", "Feature25",
            "Feature26", "Feature27", "Feature28", "Feature29", "Feature30"
        }
        Dim samples As String() = {
            "GroupA_1", "GroupA_2", "GroupA_3",
            "GroupB_1", "GroupB_2", "GroupB_3",
            "GroupC_1", "GroupC_2", "GroupC_3",
            "GroupD_1", "GroupD_2", "GroupD_3",
            "GroupE_1", "GroupE_2", "GroupE_3"
        }
        Dim groups As String() = {
            "Group A", "Group A", "Group A",
            "Group B", "Group B", "Group B",
            "Group C", "Group C", "Group C",
            "Group D", "Group D", "Group D",
            "Group E", "Group E", "Group E"
        }

        ' 每个样本所属的分组下标（0 = Group A ... 4 = Group E）
        Dim group As Integer() = {0, 0, 0, 1, 1, 1, 2, 2, 2, 3, 3, 3, 4, 4, 4}

        ' 6 种丰度模式 × 5 个分组的效应量
        Dim patterns As Double()() = {
            New Double() {0.0, 1.7, 0.0, 0.0, 0.0},
            New Double() {1.7, 0.0, 0.0, 0.0, 0.0},
            New Double() {0.0, 0.0, 1.6, 0.0, 0.0},
            New Double() {0.0, 0.0, 0.0, 1.6, 0.0},
            New Double() {0.0, 0.0, 0.0, 0.0, 1.7},
            New Double() {0.0, 0.0, -1.4, 0.0, 0.0}
        }

        Dim rnd As New Random(20261026)
        Dim nCompound As Integer = compounds.Length
        Dim nSample As Integer = samples.Length
        Dim matrix(nCompound - 1, nSample - 1) As Double

        For c As Integer = 0 To nCompound - 1
            Dim pattern As Double() = patterns(c Mod patterns.Length)
            Dim level As Double = 1.0 + rnd.NextDouble() * 0.4

            For s As Integer = 0 To nSample - 1
                ' 同组的重复样本之间再叠加一点随机偏差
                matrix(c, s) = level + pattern(group(s)) + (rnd.NextDouble() - 0.5) * 0.45
            Next
        Next

        Return New SampleData With {
            .matrix = matrix,
            .compounds = compounds,
            .samples = samples,
            .groups = groups
        }
    End Function

    Private Function ToJagged(matrix As Double(,)) As Double()()
        Dim nRow As Integer = matrix.GetLength(0)
        Dim nCol As Integer = matrix.GetLength(1)
        Dim rows(nRow - 1)() As Double

        For i As Integer = 0 To nRow - 1
            Dim v(nCol - 1) As Double

            For j As Integer = 0 To nCol - 1
                v(j) = matrix(i, j)
            Next

            rows(i) = v
        Next

        Return rows
    End Function

    Private Function Transpose(matrix As Double(,)) As Double(,)
        Dim nRow As Integer = matrix.GetLength(0)
        Dim nCol As Integer = matrix.GetLength(1)
        Dim t(nCol - 1, nRow - 1) As Double

        For i As Integer = 0 To nRow - 1
            For j As Integer = 0 To nCol - 1
                t(j, i) = matrix(i, j)
            Next
        Next

        Return t
    End Function

#End Region
End Module
