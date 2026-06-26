Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.IO
Imports DataPlot
Imports stdf = System.Math

' ============================================================================
'  Examples.vb - 各类图表使用示例
'  调用示例方法即可在当前目录生成 PNG 论文插图
' ============================================================================

Public Class Examples

    Public Shared Sub RunAll(outputDir As String)
        Directory.CreateDirectory(outputDir)
        DemoScatter(outputDir)
        DemoLine(outputDir)
        DemoBar(outputDir)
        DemoHistogram(outputDir)
        DemoBox(outputDir)
        DemoViolin(outputDir)
        DemoPie(outputDir)
        DemoHeatmap(outputDir)
        DemoSankey(outputDir)
        Console.WriteLine("All demo charts saved to: " & outputDir)
    End Sub

    ' ---------------- 散点图 ----------------
    Public Shared Sub DemoScatter(dir As String)
        Dim rnd As New Random(42)
        Dim s1 As New Series With {
            .Name = "Group A",
            .X = Enumerable.Range(0, 50).Select(Function(i) CDbl(i)).ToArray(),
            .Y = Enumerable.Range(0, 50).Select(Function(i) i * 0.5 + rnd.NextDouble() * 5).ToArray(),
            .MarkerShape = MarkerShape.Circle
        }
        Dim s2 As New Series With {
            .Name = "Group B",
            .X = Enumerable.Range(0, 50).Select(Function(i) CDbl(i)).ToArray(),
            .Y = Enumerable.Range(0, 50).Select(Function(i) i * 0.3 + 10 + rnd.NextDouble() * 5).ToArray(),
            .MarkerShape = MarkerShape.Triangle,
            .Color = Color.FromArgb(214, 39, 40)
        }
        Using plt As New ScatterPlot(800, 600, PlotTheme.Nature())
            plt.Title = "Scatter Plot Demo"
            plt.SubTitle = "Two groups with linear trend"
            plt.XLabel = "X value"
            plt.YLabel = "Y value"
            plt.Plot({s1, s2}.ToList())
            plt.SavePng(Path.Combine(dir, "scatter.png"), 300)
        End Using
    End Sub

    ' ---------------- 折线图 ----------------
    Public Shared Sub DemoLine(dir As String)
        Dim rnd As New Random(7)
        Dim s1 As New Series With {
            .Name = "Method A",
            .X = Enumerable.Range(0, 20).Select(Function(i) CDbl(i)).ToArray(),
            .Y = Enumerable.Range(0, 20).Select(Function(i) stdf.Sin(i * 0.5) * 10 + 20 + rnd.NextDouble() * 2).ToArray(),
            .LineStyle = DashStyle.Solid
        }
        Dim s2 As New Series With {
            .Name = "Method B",
            .X = Enumerable.Range(0, 20).Select(Function(i) CDbl(i)).ToArray(),
            .Y = Enumerable.Range(0, 20).Select(Function(i) stdf.Cos(i * 0.5) * 8 + 18 + rnd.NextDouble() * 2).ToArray(),
            .LineStyle = DashStyle.Dash,
            .Color = Color.FromArgb(214, 39, 40)
        }
        Using plt As New LinePlot(800, 600, PlotTheme.Science())
            plt.Title = "Line Plot Demo"
            plt.XLabel = "Time (s)"
            plt.YLabel = "Signal amplitude"
            plt.Plot({s1, s2}.ToList())
            plt.SavePng(Path.Combine(dir, "line.png"), 300)
        End Using
    End Sub

    ' ---------------- 柱状图 ----------------
    Public Shared Sub DemoBar(dir As String)
        Dim cats = {"A", "B", "C", "D", "E"}
        Dim multi(,) As Double = {
            {12, 25, 18, 22, 30},
            {8, 20, 15, 18, 25}
        }
        Using plt As New BarPlot(800, 600, PlotTheme.Light())
            plt.Title = "Bar Chart Demo"
            plt.SubTitle = "Grouped comparison"
            plt.XLabel = "Category"
            plt.YLabel = "Value"
            plt.Categories = cats
            plt.MultiValues = multi
            plt.SeriesNames = {"2023", "2024"}
            plt.ShowValueLabels = True
            plt.Plot()
            plt.SavePng(Path.Combine(dir, "bar.png"), 300)
        End Using
    End Sub

    ' ---------------- 直方图 ----------------
    Public Shared Sub DemoHistogram(dir As String)
        Dim rnd As New Random(1)
        Dim data = Enumerable.Range(0, 1000).Select(Function(i) SampleNormal(rnd, 50, 10)).ToArray()
        Using plt As New HistogramPlot(800, 600, PlotTheme.Nature())
            plt.Title = "Histogram Demo"
            plt.SubTitle = "Normal distribution, n=1000"
            plt.XLabel = "Value"
            plt.YLabel = "Frequency"
            plt.Data = data
            plt.Bins = 30
            plt.ShowRug = True
            plt.Plot()
            plt.SavePng(Path.Combine(dir, "histogram.png"), 300)
        End Using
    End Sub

    ' ---------------- 盒须图 ----------------
    Public Shared Sub DemoBox(dir As String)
        Dim rnd As New Random(3)
        Dim groups As New List(Of BoxGroup)()
        For i = 0 To 4
            Dim data = Enumerable.Range(0, 80).Select(Function(j) SampleNormal(rnd, 20 + i * 5, 3 + i * 0.5)).ToArray()
            groups.Add(New BoxGroup With {.Name = "G" & (i + 1), .Data = data})
        Next
        Using plt As New BoxPlot(800, 600, PlotTheme.Science())
            plt.Title = "Box Plot Demo"
            plt.XLabel = "Group"
            plt.YLabel = "Measurement"
            plt.Groups = groups
            plt.Plot()
            plt.SavePng(Path.Combine(dir, "boxplot.png"), 300)
        End Using
    End Sub

    ' ---------------- 小提琴图 ----------------
    Public Shared Sub DemoViolin(dir As String)
        Dim rnd As New Random(5)
        Dim groups As New List(Of BoxGroup)()
        For i = 0 To 3
            Dim data = Enumerable.Range(0, 200).Select(Function(j) SampleNormal(rnd, 30 + i * 8, 5 + i)).ToArray()
            groups.Add(New BoxGroup With {.Name = "Cond" & (i + 1), .Data = data})
        Next
        Using plt As New ViolinPlot(800, 600, PlotTheme.Light())
            plt.Title = "Violin Plot Demo"
            plt.XLabel = "Condition"
            plt.YLabel = "Expression level"
            plt.Groups = groups
            plt.Plot()
            plt.SavePng(Path.Combine(dir, "violin.png"), 300)
        End Using
    End Sub

    ' ---------------- 饼图 ----------------
    Public Shared Sub DemoPie(dir As String)
        Using plt As New PiePlot(700, 600, PlotTheme.Light())
            plt.Title = "Pie Chart Demo"
            plt.Labels = {"Category A", "Category B", "Category C", "Category D", "Category E"}
            plt.Values = {35, 25, 20, 12, 8}
            plt.Donut = True
            plt.DonutRadius = 0.55F
            plt.ExplodeIndex = 0
            plt.Plot()
            plt.SavePng(Path.Combine(dir, "pie.png"), 300)
        End Using
    End Sub

    ' ---------------- 热图 ----------------
    Public Shared Sub DemoHeatmap(dir As String)
        Dim rnd As New Random(9)
        Dim mat(9, 11) As Double
        For i = 0 To 9
            For j = 0 To 11
                mat(i, j) = stdf.Sin(i * 0.5) * stdf.Cos(j * 0.4) + rnd.NextDouble() * 0.3
            Next
        Next
        Using plt As New heatmapPlot(800, 600, PlotTheme.Light())
            plt.Title = "Heatmap Demo"
            plt.Matrix = mat
            plt.RowLabels = Enumerable.Range(1, 10).Select(Function(i) "R" & i).ToArray()
            plt.ColLabels = Enumerable.Range(1, 12).Select(Function(i) "C" & i).ToArray()
            plt.ColorMap = heatmapPlot.ColorMapType.Viridis
            plt.ShowValues = False
            plt.Plot()
            plt.SavePng(Path.Combine(dir, "heatmap.png"), 300)
        End Using
    End Sub

    ' ---------------- 桑基图 ----------------
    Public Shared Sub DemoSankey(dir As String)
        Dim nodes As New List(Of SankeyNode)()
        nodes.Add(New SankeyNode With {.Id = "A", .Label = "Source A", .Layer = 0})
        nodes.Add(New SankeyNode With {.Id = "B", .Label = "Source B", .Layer = 0})
        nodes.Add(New SankeyNode With {.Id = "C", .Label = "Mid C", .Layer = 1})
        nodes.Add(New SankeyNode With {.Id = "D", .Label = "Mid D", .Layer = 1})
        nodes.Add(New SankeyNode With {.Id = "E", .Label = "Target E", .Layer = 2})
        nodes.Add(New SankeyNode With {.Id = "F", .Label = "Target F", .Layer = 2})

        Dim links As New List(Of SankeyLink)()
        links.Add(New SankeyLink With {.Source = "A", .Target = "C", .Value = 30})
        links.Add(New SankeyLink With {.Source = "A", .Target = "D", .Value = 20})
        links.Add(New SankeyLink With {.Source = "B", .Target = "C", .Value = 15})
        links.Add(New SankeyLink With {.Source = "B", .Target = "D", .Value = 35})
        links.Add(New SankeyLink With {.Source = "C", .Target = "E", .Value = 25})
        links.Add(New SankeyLink With {.Source = "C", .Target = "F", .Value = 20})
        links.Add(New SankeyLink With {.Source = "D", .Target = "E", .Value = 30})
        links.Add(New SankeyLink With {.Source = "D", .Target = "F", .Value = 25})

        Using plt As New SankeyPlot(900, 600, PlotTheme.Light())
            plt.Title = "Sankey Diagram Demo"
            plt.Nodes = nodes
            plt.Links = links
            plt.Plot()
            plt.SavePng(Path.Combine(dir, "sankey.png"), 300)
        End Using
    End Sub

    ' ---------------- 辅助：Box-Muller 正态采样 ----------------
    Private Shared Function SampleNormal(rnd As Random, mean As Double, std As Double) As Double
        Dim u1 = rnd.NextDouble()
        Dim u2 = rnd.NextDouble()
        Dim z = stdf.Sqrt(-2 * stdf.Log(u1)) * stdf.Cos(2 * stdf.PI * u2)
        Return mean + std * z
    End Function

End Class
