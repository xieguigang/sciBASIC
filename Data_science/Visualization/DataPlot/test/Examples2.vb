#Region "Microsoft.VisualBasic::7b3c9e1f5a2d4928b6c1e9f3a5d7b2c4, Data_science\Visualization\DataPlot\test\Examples2.vb"

    ' 
    '       sciBASIC.NET Foundation, GPL3 Licensed
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.

    ' Class MigrationExamples
    ' 
    '     Function: Gau, SampleNormal2
    ' 
    '     Sub: DemoAlignment, DemoBiHistogram, DemoBubbleHeatmap, DemoClusterHeatmap, DemoContour
    '          DemoCorrelationHeatmap, DemoCorrelationTriangle, DemoDensity, DemoForest, DemoManhattan
    '          DemoPCA, DemoPrincipalCurve, DemoPyramid, DemoQQ, DemoROC, DemoScree
    '          DemoTimeTrends, DemoVariableWidthBar, DemoVenn, DemoZScore
    '          RunAll
    ' 
#End Region

Imports System.Drawing
Imports System.IO
Imports Microsoft.VisualBasic.Data.Plots
Imports stdf = System.Math

' ============================================================================
'  Examples2.vb - 从旧 Plots / Plots-statistics 迁移过来的图表使用示例
'  调用 MigrationExamples.RunAll(outputDir) 即可在指定目录生成全套 PNG 示例图
' ============================================================================

Public Class MigrationExamples

    Public Shared Sub RunAll(outputDir As String)
        Directory.CreateDirectory(outputDir)
        DemoContour(outputDir)
        DemoVenn(outputDir)
        DemoPyramid(outputDir)
        DemoDensity(outputDir)
        DemoROC(outputDir)
        DemoQQ(outputDir)
        DemoForest(outputDir)
        DemoScree(outputDir)
        DemoPCA(outputDir)
        DemoManhattan(outputDir)
        DemoBubbleHeatmap(outputDir)
        DemoCorrelationHeatmap(outputDir)
        DemoCorrelationTriangle(outputDir)
        DemoClusterHeatmap(outputDir)
        DemoTimeTrends(outputDir)
        DemoVariableWidthBar(outputDir)
        DemoZScore(outputDir)
        DemoBiHistogram(outputDir)
        DemoPrincipalCurve(outputDir)
        DemoAlignment(outputDir)
        Console.WriteLine("All migrated chart demos saved to: " & outputDir)
    End Sub

    Private Shared Function SampleNormal2(rnd As Random, mean As Double, sd As Double) As Double
        Dim u1 = 1.0 - rnd.NextDouble()
        Dim u2 = 1.0 - rnd.NextDouble()
        Return mean + sd * stdf.Sqrt(-2 * stdf.Log(u1)) * stdf.Sin(2 * stdf.PI * u2)
    End Function

    Private Shared Function Gau(x As Double, y As Double) As Double
        Return stdf.Exp(-((x - 0.5) ^ 2 + (y - 0.5) ^ 2) * 6)
    End Function

    ' ---------------- 等值线 / 曲面热图 ----------------
    Public Shared Sub DemoContour(dir As String)
        Using plt As New ContourPlot(900, 700, PlotTheme.Light())
            plt.Title = "Filled Contour Demo"
            plt.Surface = AddressOf Gau
            plt.XMin = 0 : plt.XMax = 1 : plt.YMin = 0 : plt.YMax = 1
            plt.Mode = ContourPlot.ContourMode.Filled
            plt.Levels = 10
            plt.Plot()
            plt.SavePng(Path.Combine(dir, "contour.png"), 300)
        End Using
    End Sub

    ' ---------------- 文氏图 ----------------
    Public Shared Sub DemoVenn(dir As String)
        Dim a As New VennSet With {.Name = "Set A", .Size = 520}
        Dim b As New VennSet With {.Name = "Set B", .Size = 380}
        Dim c As New VennSet With {.Name = "Set C", .Size = 260}

        a.Intersections("B") = 110
        b.Intersections("A") = 110
        a.Intersections("C") = 60
        c.Intersections("A") = 60
        b.Intersections("C") = 45
        c.Intersections("B") = 45

        Using plt As New VennPlot(800, 700, PlotTheme.Light())
            plt.Title = "Venn Diagram Demo"
            plt.Sets = New List(Of VennSet) From {a, b, c}
            plt.Plot()
            plt.SavePng(Path.Combine(dir, "venn.png"), 300)
        End Using
    End Sub

    ' ---------------- 人口金字塔 ----------------
    Public Shared Sub DemoPyramid(dir As String)
        Dim cats = {"0-9", "10-19", "20-29", "30-39", "40-49", "50-59", "60-69", "70+"}
        Dim male = {52.0, 58.0, 61.0, 55.0, 47.0, 39.0, 27.0, 14.0}
        Dim female = {50.0, 56.0, 59.0, 54.0, 49.0, 42.0, 31.0, 19.0}

        Using plt As New PyramidPlot(800, 600, PlotTheme.Light())
            plt.Title = "Population Pyramid Demo"
            plt.Categories = cats
            plt.LeftValues = male
            plt.RightValues = female
            plt.LeftName = "Male"
            plt.RightName = "Female"
            plt.Plot()
            plt.SavePng(Path.Combine(dir, "pyramid.png"), 300)
        End Using
    End Sub

    ' ---------------- 二维散点密度图 ----------------
    Public Shared Sub DemoDensity(dir As String)
        Dim rnd As New Random(11)
        Dim pts = Enumerable.Range(0, 1500) _
            .Select(Function(i) New PointF(CSng(SampleNormal2(rnd, 0, 1) * 0.3F + 0.5F),
                                           CSng(SampleNormal2(rnd, 0, 1) * 0.25F + 0.5F)))

        Using plt As New DensityPlot(800, 600, PlotTheme.Light())
            plt.Title = "2D Density Plot Demo"
            plt.Points = pts
            plt.XMin = 0 : plt.XMax = 1 : plt.YMin = 0 : plt.YMax = 1
            plt.Plot()
            plt.SavePng(Path.Combine(dir, "density.png"), 300)
        End Using
    End Sub

    ' ---------------- ROC 曲线 ----------------
    Public Shared Sub DemoROC(dir As String)
        Dim rnd As New Random(3)
        Dim pos = Enumerable.Range(0, 200).Select(Function(i) SampleNormal2(rnd, 1.2, 1)).ToArray()
        Dim neg = Enumerable.Range(0, 200).Select(Function(i) SampleNormal2(rnd, 0, 1)).ToArray()
        Dim thresholds = Enumerable.Range(0, 41).Select(Function(i) -2.0 + 0.1 * i).ToArray()

        Dim tpr = thresholds.Select(Function(t) pos.Count(Function(v) v >= t) / CDbl(pos.Length)).ToArray()
        Dim fpr = thresholds.Select(Function(t) neg.Count(Function(v) v >= t) / CDbl(neg.Length)).ToArray()
        Dim auc = Enumerable.Range(0, thresholds.Length - 1) _
            .Sum(Function(i) (fpr(i + 1) - fpr(i)) * (tpr(i) + tpr(i + 1)) / 2)

        Using plt As New ROCPlot(700, 600, PlotTheme.Light())
            plt.Title = "ROC Curve Demo"
            plt.Curves = New List(Of ROCCurve) From {
                New ROCCurve With {
                    .Name = "model A",
                    .FPR = fpr,
                    .TPR = tpr,
                    .AUC = auc
                }
            }
            plt.Plot()
            plt.SavePng(Path.Combine(dir, "roc.png"), 300)
        End Using
    End Sub

    ' ---------------- Q-Q 图 ----------------
    Public Shared Sub DemoQQ(dir As String)
        Dim rnd As New Random(7)
        Dim s1 = Enumerable.Range(0, 300).Select(Function(i) SampleNormal2(rnd, 0, 1)).ToArray()
        Dim s2 = Enumerable.Range(0, 300).Select(Function(i) SampleNormal2(rnd, 0.2, 1.1)).ToArray()

        Using plt As New QQPlot(700, 600, PlotTheme.Light())
            plt.Title = "QQ Plot Demo"
            plt.Sample1 = s1
            plt.Sample2 = s2
            plt.Sample1Name = "batch 1"
            plt.Sample2Name = "batch 2"
            plt.Plot()
            plt.SavePng(Path.Combine(dir, "qq.png"), 300)
        End Using
    End Sub

    ' ---------------- 森林图 ----------------
    Public Shared Sub DemoForest(dir As String)
        Dim entries = {
            New ForestEntry With {.Label = "Study 1", .Effect = 0.42, .Lower = 0.2, .Upper = 0.65, .Weight = 30},
            New ForestEntry With {.Label = "Study 2", .Effect = 0.31, .Lower = 0.1, .Upper = 0.52, .Weight = 45},
            New ForestEntry With {.Label = "Study 3", .Effect = 0.55, .Lower = 0.3, .Upper = 0.8, .Weight = 22},
            New ForestEntry With {.Label = "Pooled", .Effect = 0.4, .Lower = 0.28, .Upper = 0.52, .Weight = 90}
        }

        Using plt As New ForestPlot(900, 500, PlotTheme.Light())
            plt.Title = "Forest Plot Demo"
            plt.Entries = New List(Of ForestEntry)(entries)
            plt.NullLine = 0
            plt.Plot()
            plt.SavePng(Path.Combine(dir, "forest.png"), 300)
        End Using
    End Sub

    ' ---------------- 碎石图 ----------------
    Public Shared Sub DemoScree(dir As String)
        Using plt As New ScreePlot(800, 600, PlotTheme.Light())
            plt.Title = "Scree Plot Demo"
            plt.Contributions = {0.41, 0.24, 0.13, 0.09, 0.06, 0.04, 0.03}
            plt.Plot()
            plt.SavePng(Path.Combine(dir, "scree.png"), 300)
        End Using
    End Sub

    ' ---------------- PCA 得分图 ----------------
    Public Shared Sub DemoPCA(dir As String)
        Dim rnd As New Random(13)
        Dim n As Integer = 60
        Dim pc1 = Enumerable.Range(0, n).Select(Function(i) CDbl(i) / n * 4 - 2 + SampleNormal2(rnd, 0, 0.3)).ToArray()
        Dim pc2 = Enumerable.Range(0, n).Select(Function(i) stdf.Sin(i) + SampleNormal2(rnd, 0, 0.4)).ToArray()
        Dim groups = Enumerable.Range(0, n).Select(Function(i) If(i Mod 2 = 0, "control", "treated")).ToArray()

        Using plt As New PCAScorePlot(800, 600, PlotTheme.Light())
            plt.Title = "PCA Score Plot Demo"
            plt.Score = New PCAScoreSet With {
                .SampleNames = Enumerable.Range(1, n).Select(Function(i) "S" & i).ToArray(),
                .Groups = groups,
                .PC1 = pc1,
                .PC2 = pc2,
                .Contributions = {0.52, 0.27, 0.11, 0.06, 0.04}
            }
            plt.Plot()
            plt.SavePng(Path.Combine(dir, "pca.png"), 300)
        End Using
    End Sub

    ' ---------------- 曼哈顿图 ----------------
    Public Shared Sub DemoManhattan(dir As String)
        Dim rnd As New Random(17)
        Dim loci = New List(Of ManhattanPoint)()

        For chrom = 1 To 5
            For pos = 1 To 400
                Dim p As Double = stdf.Pow(10, -(1 + rnd.NextDouble() * 5))

                If chrom = 3 AndAlso pos > 210 AndAlso pos < 225 Then
                    p = stdf.Pow(10, -(7 + rnd.NextDouble() * 2))
                End If

                loci.Add(New ManhattanPoint With {
                    .Chromosome = "chr" & chrom,
                    .Position = pos,
                    .PValue = p,
                    .Label = If(p < 0.00000005, "rs" & (chrom * 1000 + pos), "")
                })
            Next
        Next

        Using plt As New ManhattanPlot(1200, 600, PlotTheme.Light())
            plt.Title = "Manhattan Plot Demo"
            plt.Points = loci
            plt.Plot()
            plt.SavePng(Path.Combine(dir, "manhattan.png"), 300)
        End Using
    End Sub

    ' ---------------- 泡泡热图 ----------------
    Public Shared Sub DemoBubbleHeatmap(dir As String)
        Dim rnd As New Random(19)
        Dim mat(5, 7) As Double

        For i = 0 To 5
            For j = 0 To 7
                mat(i, j) = stdf.Round(rnd.NextDouble(), 2)
            Next
        Next

        Using plt As New BubbleHeatmapPlot(800, 600, PlotTheme.Light())
            plt.Title = "Bubble Heatmap Demo"
            plt.Matrix = mat
            plt.RowLabels = Enumerable.Range(1, 6).Select(Function(i) "R" & i).ToArray()
            plt.ColLabels = Enumerable.Range(1, 8).Select(Function(i) "C" & i).ToArray()
            plt.Plot()
            plt.SavePng(Path.Combine(dir, "bubble_heatmap.png"), 300)
        End Using
    End Sub

    ' ---------------- 相关性矩阵热图 ----------------
    Private Shared Function DemoCorrelation() As CorrelationMatrix
        Dim names = {"A", "B", "C", "D", "E", "F"}
        Dim m(5, 5) As Double
        Dim rnd As New Random(23)

        For i = 0 To 5
            For j = 0 To 5
                If i = j Then
                    m(i, j) = 1
                ElseIf j > i Then
                    m(i, j) = stdf.Round(rnd.NextDouble() * 1.6 - 0.8, 2)
                    m(j, i) = m(i, j)
                End If
            Next
        Next

        Return New CorrelationMatrix With {.Names = names, .Matrix = m}
    End Function

    Public Shared Sub DemoCorrelationHeatmap(dir As String)
        Using plt As New CorrelationHeatmapPlot(800, 700, PlotTheme.Light())
            plt.Title = "Correlation Heatmap Demo"
            plt.Correlation = DemoCorrelation()
            plt.ShowValues = True
            plt.Plot()
            plt.SavePng(Path.Combine(dir, "correlation_heatmap.png"), 300)
        End Using
    End Sub

    Public Shared Sub DemoCorrelationTriangle(dir As String)
        Using plt As New CorrelationTrianglePlot(800, 700, PlotTheme.Light())
            plt.Title = "Correlation Triangle Demo"
            plt.Correlation = DemoCorrelation()
            plt.Plot()
            plt.SavePng(Path.Combine(dir, "correlation_triangle.png"), 300)
        End Using
    End Sub

    ' ---------------- 聚类热图 ----------------
    Public Shared Sub DemoClusterHeatmap(dir As String)
        Dim rnd As New Random(29)
        Dim rows = 10, cols = 12
        Dim mat(rows - 1, cols - 1) As Double

        For i = 0 To rows - 1
            For j = 0 To cols - 1
                mat(i, j) = stdf.Sin(i * 0.7) * stdf.Cos(j * 0.5) + rnd.NextDouble() * 0.2
            Next
        Next

        ' 行列聚类树由调用方算好；这里用一条退化树示意注入方式
        Dim rowTree = BalancedTree(Enumerable.Range(1, rows).Select(Function(i) "R" & i).ToArray())
        Dim colTree = BalancedTree(Enumerable.Range(1, cols).Select(Function(i) "C" & i).ToArray())

        Using plt As New ClusterHeatmapPlot(1000, 800, PlotTheme.Light())
            plt.Title = "Cluster Heatmap Demo"
            plt.Matrix = mat
            plt.RowLabels = rowTree.Leaves()
            plt.ColLabels = colTree.Leaves()
            plt.RowTree = rowTree
            plt.ColTree = colTree
            plt.Plot()
            plt.SavePng(Path.Combine(dir, "cluster_heatmap.png"), 300)
        End Using
    End Sub

    ''' <summary>构造一棵平衡的二叉聚类树（示例用）</summary>
    Private Shared Function BalancedTree(names As String()) As ClusterTreeNode
        If names.Length = 1 Then
            Return New ClusterTreeNode With {.Name = names(0), .Height = 0}
        End If

        Dim half = names.Length \ 2

        Return New ClusterTreeNode With {
            .Name = "(" & names.First & ".." & names.Last & ")",
            .Height = names.Length,
            .Left = BalancedTree(names.Take(half).ToArray()),
            .Right = BalancedTree(names.Skip(half).ToArray())
        }
    End Function

    ' ---------------- 时间趋势 ----------------
    Public Shared Sub DemoTimeTrends(dir As String)
        Dim series1 = Enumerable.Range(0, 50).Select(Function(i) New TimePoint(i, stdf.Sin(i * 0.2) + 2)).ToList()
        Dim series2 = Enumerable.Range(0, 50).Select(Function(i) New TimePoint(i, stdf.Cos(i * 0.2))).ToList()

        Using plt As New TimeTrendsPlot(900, 600, PlotTheme.Light())
            plt.Title = "Time Trends Demo"
            plt.Trends = New List(Of ODESeries) From {
                TimeTrendsPlot.FromTimePoints(series1, "signal A"),
                TimeTrendsPlot.FromTimePoints(series2, "signal B")
            }
            plt.Plot()
            plt.SavePng(Path.Combine(dir, "time_trends.png"), 300)
        End Using
    End Sub

    ' ---------------- 变宽条形图 ----------------
    Public Shared Sub DemoVariableWidthBar(dir As String)
        Dim bars = {
            New VariableBarData With {.Name = "A", .Value = 12, .Width = 200},
            New VariableBarData With {.Name = "B", .Value = 8, .Width = 420},
            New VariableBarData With {.Name = "C", .Value = 15, .Width = 160},
            New VariableBarData With {.Name = "D", .Value = 6, .Width = 500},
            New VariableBarData With {.Name = "E", .Value = 10, .Width = 300}
        }

        Using plt As New VariableWidthBarPlot(900, 600, PlotTheme.Light())
            plt.Title = "Variable Width Bar Demo"
            plt.Bars = New List(Of VariableBarData)(bars)
            plt.Plot()
            plt.SavePng(Path.Combine(dir, "variable_bar.png"), 300)
        End Using
    End Sub

    ' ---------------- Z-score 矩阵 ----------------
    Public Shared Sub DemoZScore(dir As String)
        Dim rnd As New Random(31)
        Dim vars = {"V1", "V2", "V3", "V4", "V5", "V6", "V7", "V8"}
        Dim entries = Enumerable.Range(1, 12) _
            .Select(Function(r) New ZScoreEntry With {
                .Name = "S" & r,
                .Group = If(r Mod 2 = 0, "case", "ctrl"),
                .Values = vars.Select(Function(v) stdf.Round(SampleNormal2(rnd, 0, 1.2), 2)).ToArray()
            }).ToList()

        Using plt As New ZScorePlot(900, 700, PlotTheme.Light())
            plt.Title = "Z-Score Plot Demo"
            plt.Entries = entries
            plt.VariableNames = vars
            plt.Plot()
            plt.SavePng(Path.Combine(dir, "zscore.png"), 300)
        End Using
    End Sub

    ' ---------------- 双直方图 ----------------
    Public Shared Sub DemoBiHistogram(dir As String)
        Dim rnd As New Random(37)
        Dim a = Enumerable.Range(0, 500).Select(Function(i) SampleNormal2(rnd, 0, 1)).ToArray()
        Dim b = Enumerable.Range(0, 500).Select(Function(i) SampleNormal2(rnd, 1.5, 1)).ToArray()

        Using plt As New BiHistogramPlot(800, 600, PlotTheme.Light())
            plt.Title = "Bi-Histogram Demo"
            plt.SampleA = New BiHistogramSample With {.Name = "control", .Data = a}
            plt.SampleB = New BiHistogramSample With {.Name = "treated", .Data = b}
            plt.PlotBihistogram()
            plt.SavePng(Path.Combine(dir, "bihistogram.png"), 300)
        End Using
    End Sub

    ' ---------------- 主曲线 ----------------
    Public Shared Sub DemoPrincipalCurve(dir As String)
        Dim rnd As New Random(41)
        Dim pts = Enumerable.Range(0, 120) _
            .Select(Function(i) New PointF(CSng(i * 0.06 + SampleNormal2(rnd, 0, 0.25)),
                                           CSng(stdf.Sin(i * 0.06) + SampleNormal2(rnd, 0, 0.25)))) _
            .ToArray()
        Dim curve = Enumerable.Range(0, 60) _
            .Select(Function(i) New PointF(CSng(i * 0.12), CSng(stdf.Sin(i * 0.12)))) _
            .ToArray()

        Using plt As New PrincipalCurvePlot(800, 600, PlotTheme.Light())
            plt.Title = "Principal Curve Demo"
            plt.Points = pts
            plt.Curve = curve
            plt.ShowProjections = True
            plt.Projections = curve
            plt.Plot()
            plt.SavePng(Path.Combine(dir, "principal_curve.png"), 300)
        End Using
    End Sub

    ' ---------------- 多序列对齐图 ----------------
    Public Shared Sub DemoAlignment(dir As String)
        Dim rnd As New Random(43)
        Dim tracks As New List(Of Series)()

        For t = 0 To 3
            tracks.Add(New Series With {
                .Name = "track " & (t + 1),
                .X = Enumerable.Range(0, 200).Select(Function(i) CDbl(i)).ToArray(),
                .Y = Enumerable.Range(0, 200) _
                    .Select(Function(i) CDbl(stdf.Abs(stdf.Sin(i * 0.05 + t))) * (10 - t * 2) + rnd.NextDouble()) _
                    .ToArray()
            })
        Next

        Using plt As New AlignmentPlot(1000, 700, PlotTheme.Light())
            plt.Title = "Alignment Tracks Demo"
            plt.Tracks = tracks
            plt.Highlights = New List(Of (start As Double, [end] As Double)) From {(50, 80), (140, 160)}
            plt.Plot()
            plt.SavePng(Path.Combine(dir, "alignment.png"), 300)
        End Using
    End Sub
End Class
