#Region "Microsoft.VisualBasic::88dbbf91cec6c85dac9660d6fe150273, Data_science\Visualization\test\Examples\ChartExamples.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 303
    '    Code Lines: 234 (77.23%)
    ' Comment Lines: 47 (15.51%)
    '    - Xml Docs: 8.51%
    ' 
    '   Blank Lines: 22 (7.26%)
    '     File Size: 14.60 KB


    ' Module ChartExamples
    ' 
    '     Sub: ExampleAreaPlot, ExampleBubblePlot, ExampleChordPlot, ExampleJitterPlot, ExampleRadarPlot
    '          ExampleRosePlot, ExampleStackedAreaPlot, ExampleStackedBarPlot, ExampleTreemapPlot, RunAll
    '          RunSafe
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports DataPlot

' ============================================================================
'  ChartExamples.vb - 9 种新增图表的使用示例与测试数据
'  调用 ChartExamples.RunAll(outputDir) 即可批量生成全部示例 PNG。
' ============================================================================
'  运行方式：
'   本框架基于 GDI+ 光栅驱动，需在程序启动时注册驱动后再调用 RunAll：
'     1) 将宿主项目目标框架设为 net10.0-windows（或 net48）；
'     2) 启动时调用 Microsoft.VisualBasic.Imaging.Driver.ImageDriver.Register()；
'     3) 调用 ChartExamples.RunAll(输出目录) 批量生成 9 张 PNG。
' ============================================================================

''' <summary>9 种新增图表的使用示例与测试数据</summary>
Public Module ChartExamples

    ''' <summary>批量生成全部 9 种图表的示例 PNG 到指定目录</summary>
    ''' <param name="outputDir">输出目录（不存在则自动创建）</param>
    Public Sub RunAll(outputDir As String)
        If Not Directory.Exists(outputDir) Then Directory.CreateDirectory(outputDir)

        RunSafe("1/9 AreaPlot", AddressOf ExampleAreaPlot, outputDir)
        RunSafe("2/9 StackedAreaPlot", AddressOf ExampleStackedAreaPlot, outputDir)
        RunSafe("3/9 StackedBarPlot", AddressOf ExampleStackedBarPlot, outputDir)
        RunSafe("4/9 RosePlot", AddressOf ExampleRosePlot, outputDir)
        RunSafe("5/9 RadarPlot", AddressOf ExampleRadarPlot, outputDir)
        RunSafe("6/9 JitterPlot", AddressOf ExampleJitterPlot, outputDir)
        RunSafe("7/9 BubblePlot", AddressOf ExampleBubblePlot, outputDir)
        RunSafe("8/9 TreemapPlot", AddressOf ExampleTreemapPlot, outputDir)
        RunSafe("9/9 ChordPlot", AddressOf ExampleChordPlot, outputDir)

        Console.WriteLine($"全部示例已输出至: {outputDir}")
    End Sub

    ''' <summary>统一执行 + 异常捕获，避免单个图表失败中断全部</summary>
    Private Sub RunSafe(name As String, action As Action(Of String), outputDir As String)
        Try
            action(outputDir)
            Console.WriteLine($"[OK]   {name}")
        Catch ex As Exception
            Console.WriteLine($"[FAIL] {name} -> {ex.GetType().Name}: {ex.Message}")
        End Try
    End Sub

    ' ========================================================
    '  1. 基础面积图（曲线下面积 AUC）
    ' ========================================================
    Private Sub ExampleAreaPlot(outputDir As String)
        Dim months As Double() = {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12}
        Dim series As New List(Of Series) From {
            New Series With {
                .Name = "PC 端访问量",
                .X = months,
                .Y = {120, 135, 128, 142, 155, 160, 158, 165, 170, 168, 175, 180}
            },
            New Series With {
                .Name = "移动端访问量",
                .X = months,
                .Y = {80, 95, 110, 130, 150, 175, 190, 205, 220, 235, 250, 265}
            }
        }

        Using plot As New AreaPlot(1000, 700)
            plot.Title = "基础面积图 - 网站月度访问量趋势（曲线下面积）"
            plot.SubTitle = "Catmull-Rom 平滑 + 半透明填充"
            plot.XLabel = "月份"
            plot.YLabel = "访问量（万次）"
            plot.Smooth = True
            plot.FillAlpha = 120
            plot.Baseline = 0
            plot.LegendLocation = PlotEngine.LegendPos.UpperLeft
            plot.Plot(series)
            plot.SavePng(Path.Combine(outputDir, "01_AreaPlot.png"))
        End Using
    End Sub

    ' ========================================================
    '  2. 堆叠面积图
    ' ========================================================
    Private Sub ExampleStackedAreaPlot(outputDir As String)
        Using plot As New StackedAreaPlot(1000, 700)
            plot.Title = "堆叠面积图 - 各季度产品线营收"
            plot.SubTitle = "硬件 + 软件 + 服务 逐层累加"
            plot.XLabel = "季度"
            plot.YLabel = "营收（亿元）"
            plot.Categories = {"Q1", "Q2", "Q3", "Q4"}
            ' MultiValues[系列, 类别]：每行一个系列
            plot.MultiValues = {
                {12, 15, 14, 18},
                {8, 9, 11, 13},
                {5, 6, 7, 9}
            }
            plot.SeriesNames = {"硬件", "软件", "服务"}
            plot.Smooth = True
            plot.ShowValueLabels = True
            plot.Plot()
            plot.SavePng(Path.Combine(outputDir, "02_StackedAreaPlot.png"))
        End Using
    End Sub

    ' ========================================================
    '  3. 堆叠柱状图（与堆叠面积图同数据，便于对比）
    ' ========================================================
    Private Sub ExampleStackedBarPlot(outputDir As String)
        Using plot As New StackedBarPlot(1000, 700)
            plot.Title = "堆叠柱状图 - 各季度产品线营收"
            plot.SubTitle = "正负值分离堆叠，含数值标签与总计"
            plot.XLabel = "季度"
            plot.YLabel = "营收（亿元）"
            plot.Categories = {"Q1", "Q2", "Q3", "Q4"}
            plot.MultiValues = {
                {12, 15, 14, 18},
                {8, 9, 11, 13},
                {5, 6, 7, 9}
            }
            plot.SeriesNames = {"硬件", "软件", "服务"}
            plot.ShowValueLabels = True
            plot.ShowTotalLabel = True
            plot.Horizontal = False
            plot.Plot()
            plot.SavePng(Path.Combine(outputDir, "03_StackedBarPlot.png"))
        End Using
    End Sub

    ' ========================================================
    '  4. 南丁格尔玫瑰图
    ' ========================================================
    Private Sub ExampleRosePlot(outputDir As String)
        Using plot As New RosePlot(800, 800)
            plot.Title = "南丁格尔玫瑰图 - 月度降雨量"
            plot.SubTitle = "等角度扇区，半径与降雨量成比例"
            plot.Labels = {"1月", "2月", "3月", "4月", "5月", "6月",
                           "7月", "8月", "9月", "10月", "11月", "12月"}
            plot.Values = {15, 20, 35, 60, 95, 140, 180, 165, 90, 55, 30, 18}
            plot.Donut = True
            plot.DonutRadius = 0.25F
            plot.ShowPercentage = True
            plot.StartAngle = -90
            plot.ShowLegend = False
            plot.Plot()
            plot.SavePng(Path.Combine(outputDir, "04_RosePlot.png"))
        End Using
    End Sub

    ' ========================================================
    '  5. 雷达图（蛛网图）
    ' ========================================================
    Private Sub ExampleRadarPlot(outputDir As String)
        Using plot As New RadarPlot(800, 800)
            plot.Title = "雷达图 - 两款旗舰手机多维评分对比"
            plot.SubTitle = "6 维度 0-10 分，多系列半透明叠加"
            plot.Categories = {"性能", "拍照", "续航", "屏幕", "价格", "系统"}
            ' 两款手机各 6 维评分
            plot.MultiValues = {
                {9, 8, 7, 9, 6, 8},
                {7, 9, 8, 8, 8, 7}
            }
            plot.SeriesNames = {"旗舰 X", "旗舰 Y"}
            plot.FillAlpha = 100
            plot.GridLevels = 5
            plot.MaxValue = 10
            plot.ShowScale = True
            plot.Plot()
            plot.SavePng(Path.Combine(outputDir, "05_RadarPlot.png"))
        End Using
    End Sub

    ' ========================================================
    '  6. Jitter 散点图（分组随机抖动）
    ' ========================================================
    Private Sub ExampleJitterPlot(outputDir As String)
        ' 用固定种子生成 4 组血压测量值（均值随剂量递增），体现分组散点
        Dim rnd As New Random(7)
        Dim doseNames = {"安慰剂", "10mg", "20mg", "30mg"}
        Dim baseMeans = {120, 125, 132, 140}
        Dim groups As New List(Of BoxGroup)()
        For i = 0 To doseNames.Length - 1
            Dim data(29) As Double
            For k = 0 To data.Length - 1
                ' 均值 baseMeans(i)，标准差 6 的近似正态扰动
                data(k) = baseMeans(i) + (rnd.NextDouble() + rnd.NextDouble() + rnd.NextDouble() - 1.5) * 6
            Next
            groups.Add(New BoxGroup With {.Name = doseNames(i), .Data = data})
        Next

        Using plot As New JitterPlot(1000, 700)
            plot.Title = "Jitter 散点图 - 不同剂量下血压测量分布"
            plot.SubTitle = "组内 X 方向随机抖动避免遮挡（可复现种子）"
            plot.XLabel = "剂量分组"
            plot.YLabel = "收缩压 (mmHg)"
            plot.Groups = groups
            plot.JitterWidth = 0.3F
            plot.RandomSeed = 42
            plot.MarkerShape = MarkerShape.Circle
            plot.MarkerSize = 5.0F
            plot.MarkerAlpha = 180
            plot.LegendLocation = PlotEngine.LegendPos.UpperLeft
            plot.Plot()
            plot.SavePng(Path.Combine(outputDir, "06_JitterPlot.png"))
        End Using
    End Sub

    ' ========================================================
    '  7. 气泡图（三维：人口-GDP-国土面积）
    ' ========================================================
    Private Sub ExampleBubblePlot(outputDir As String)
        Dim countries = {"中国", "美国", "印度", "日本", "德国", "英国",
                         "法国", "巴西", "俄罗斯", "加拿大", "澳大利亚", "韩国"}
        ' X=人口(百万), Y=GDP(千亿美元), Sizes=国土面积(万平方公里)
        Dim pop As Double() = {1410, 333, 1408, 125, 84, 67, 65, 214, 144, 38, 26, 52}
        Dim gdp As Double() = {177, 254, 34, 42, 40, 31, 29, 19, 22, 21, 17, 17}
        Dim area As Double() = {960, 983, 329, 38, 36, 24, 55, 851, 1709, 998, 769, 10}

        Dim series As New List(Of BubbleSeries) From {
            New BubbleSeries With {
                .Name = "国家",
                .X = pop, .Y = gdp, .Sizes = area
            }
        }

        Using plot As New BubblePlot(1000, 700)
            plot.Title = "气泡图 - 人口 / GDP / 国土面积"
            plot.SubTitle = "X=人口(百万)  Y=GDP(千亿美元)  气泡面积=国土(万km²)"
            plot.XLabel = "人口（百万）"
            plot.YLabel = "GDP（千亿美元）"
            plot.MinBubbleSize = 5.0F
            plot.MaxBubbleSize = 32.0F
            plot.FillAlpha = 140
            plot.ShowSizeLegend = True
            plot.SizeLegendCount = 3
            plot.LegendLocation = PlotEngine.LegendPos.UpperLeft
            plot.Plot(series)
            plot.SavePng(Path.Combine(outputDir, "07_BubblePlot.png"))
        End Using
    End Sub

    ' ========================================================
    '  8. 矩形树图（Squarified 布局，按部门分组着色）
    ' ========================================================
    Private Sub ExampleTreemapPlot(outputDir As String)
        Dim nodes As New List(Of TreemapNode) From {
            New TreemapNode With {.Label = "手机", .Value = 320, .Group = "电子"},
            New TreemapNode With {.Label = "笔记本", .Value = 210, .Group = "电子"},
            New TreemapNode With {.Label = "平板", .Value = 95, .Group = "电子"},
            New TreemapNode With {.Label = "耳机", .Value = 70, .Group = "电子"},
            New TreemapNode With {.Label = "外套", .Value = 150, .Group = "服装"},
            New TreemapNode With {.Label = "鞋类", .Value = 130, .Group = "服装"},
            New TreemapNode With {.Label = "T恤", .Value = 88, .Group = "服装"},
            New TreemapNode With {.Label = "零食", .Value = 120, .Group = "食品"},
            New TreemapNode With {.Label = "饮料", .Value = 105, .Group = "食品"},
            New TreemapNode With {.Label = "生鲜", .Value = 75, .Group = "食品"},
            New TreemapNode With {.Label = "家具", .Value = 140, .Group = "家居"},
            New TreemapNode With {.Label = "厨具", .Value = 60, .Group = "家居"},
            New TreemapNode With {.Label = "床上用品", .Value = 48, .Group = "家居"},
            New TreemapNode With {.Label = "灯具", .Value = 35, .Group = "家居"}
        }

        Using plot As New TreemapPlot(1000, 700)
            plot.Title = "矩形树图 - 各品类销售额（按部门分组着色）"
            plot.SubTitle = "Squarified 算法优化宽高比，面积=销售额"
            plot.Nodes = nodes
            plot.ShowLabels = True
            plot.ShowValues = True
            plot.AutoFontSize = True
            plot.ColorByGroup = True
            plot.BorderWidth = 1.5F
            plot.Plot()
            plot.SavePng(Path.Combine(outputDir, "08_TreemapPlot.png"))
        End Using
    End Sub

    ' ========================================================
    '  9. 和弦图（部门间资金流转邻接矩阵）
    ' ========================================================
    Private Sub ExampleChordPlot(outputDir As String)
        Dim labels = {"研发", "市场", "生产", "销售", "财务", "行政"}
        ' 邻接矩阵 Matrix(i,j) = 从 i 流向 j 的资金（亿元），对角线为 0
        Dim matrix As Double(,) = {
            {0, 15, 35, 8, 12, 6},
            {18, 0, 10, 28, 9, 5},
            {30, 12, 0, 22, 14, 8},
            {6, 32, 18, 0, 20, 7},
            {10, 8, 16, 14, 0, 11},
            {4, 5, 7, 6, 9, 0}
        }

        Using plot As New ChordPlot(800, 800)
            plot.Title = "和弦图 - 部门间资金流转关系"
            plot.SubTitle = "圆周弧段 = 节点流量占比，贝塞尔曲线 = 流向"
            plot.NodeLabels = labels
            plot.Matrix = matrix
            plot.Symmetric = True
            plot.ChordAlpha = 100
            plot.StartAngle = -90
            plot.GapAngle = 1.5F
            plot.ShowLegend = False
            plot.Plot()
            plot.SavePng(Path.Combine(outputDir, "09_ChordPlot.png"))
        End Using
    End Sub

End Module

