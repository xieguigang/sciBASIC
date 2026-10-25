Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports std = System.Math

' ============================================================================
'  Histogram.vb - 直方图门面（承接旧 Histogram.HistogramPlot 的调用语义）
'
'  旧实现对「装箱步长 step」语义友好，新引擎用显式的分箱边界（BinEdges）表达，
'  这里按 step 生成边界，从而保持「每格宽度固定」的旧视觉行为。
' ============================================================================

''' <summary>直方图门面</summary>
Public Module Histogram

    ''' <summary>
    ''' 绘制直方图。
    ''' </summary>
    ''' <param name="data">原始观测值</param>
    ''' <param name="binWidth">分箱宽度（固定宽度直方图）</param>
    ''' <param name="title">图表主标题</param>
    ''' <param name="xLabel">X 轴标题</param>
    ''' <param name="yLabel">Y 轴标题</param>
    ''' <param name="color">柱子颜色表达式，留空时用主题调色板的第一个颜色</param>
    ''' <param name="size">画布尺寸</param>
    ''' <param name="dpi">出图分辨率</param>
    ''' <param name="driver">绘图驱动</param>
    ''' <param name="theme">绘图主题，留空时使用浅色主题</param>
    ''' <returns></returns>
    Public Function Plot(data As IEnumerable(Of Double),
                         Optional binWidth As Double = 1,
                         Optional title$ = Nothing,
                         Optional xLabel$ = Nothing,
                         Optional yLabel$ = Nothing,
                         Optional color$ = Nothing,
                         Optional size$ = "1600,1200",
                         Optional dpi% = 100,
                         Optional driver As Drivers = Drivers.Default,
                         Optional theme As PlotTheme = Nothing) As GraphicsData

        Dim values As Double() = data _
            .Where(Function(v) Not Double.IsNaN(v) AndAlso Not Double.IsInfinity(v)) _
            .ToArray

        If values.Length = 0 Then
            Throw New ArgumentException("The histogram input data is empty!", NameOf(data))
        End If

        Dim sz As Size = size.SizeParser
        Dim t As PlotTheme = If(theme, PlotTheme.Light())

        Using plt As New HistogramPlot(sz.Width, sz.Height, t)
            plt.Title = If(title, "")
            plt.XLabel = If(xLabel, "")
            plt.YLabel = If(yLabel, "")

            If Not color.StringEmpty Then
                plt.Color = color.TranslateColor
            End If

            If binWidth > 0 Then
                plt.BinEdges = BinEdges(values, binWidth)
            Else
                plt.Bins = 30
            End If

            Return plt.Plot(sz, dpi, driver)
        End Using
    End Function

    ''' <summary>
    ''' 按固定宽度 <paramref name="binWidth"/> 生成从数据下界到上界的装箱边界。
    ''' </summary>
    ''' <param name="values"></param>
    ''' <param name="binWidth"></param>
    ''' <returns></returns>
    Private Function BinEdges(values As Double(), binWidth As Double) As Double()
        Dim lo As Double = values.Min()
        Dim hi As Double = values.Max()
        Dim n As Integer = CInt(std.Ceiling((hi - lo) / binWidth))

        If n < 1 Then n = 1

        If n > 2048 Then
            ' 防止很小的 binWidth 导致边界数组爆炸
            n = 2048
            binWidth = If(hi > lo, (hi - lo) / n, 1)
        End If

        Dim edges As Double() = New Double(n) {}

        For i As Integer = 0 To n
            edges(i) = lo + i * binWidth
        Next

        Return edges
    End Function
End Module
