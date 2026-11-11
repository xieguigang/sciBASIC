Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports Microsoft.VisualBasic.Data
Imports Microsoft.VisualBasic.Data.Plots
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Math.Statistics.ShapleyValue
Imports std = System.Math

' ---------------------------------------------------------------------------
'  ShapPlot / ShapBeeswarmPlot.vb
'  Copyright (c) 2018-2026 sciBASIC.NET Foundation, GPL3 Licensed
'
'  SHAP 摘要蜂群图（beeswarm）：每行一个特征（按平均 |SHAP| 降序），
'  每个点一个样本，横轴为该样本的 SHAP 值，颜色为特征值高低（低蓝高红）。
' ---------------------------------------------------------------------------

Namespace ShapPlot

    ''' <summary>
    ''' SHAP 摘要蜂群图（对应 python shap 的 <c>shap.plots.beeswarm</c> / <c>shap.summary_plot</c>）。
    ''' 
    ''' 图中每一行代表一个特征、纵轴按平均 |SHAP| 值降序排列（重要性最高的特征在最上方），
    ''' 每个点代表一个样本，横轴为该样本在此特征上的 SHAP 值，颜色红蓝表示特征值的高低。
    ''' </summary>
    Public Class ShapBeeswarmPlot
        Inherits PlotEngine

        ''' <summary>
        ''' SHAP 分析结果。
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Shap As ShapAnalysisResult

        ''' <summary>
        ''' 样本特征表，提供每个样本的原始特征取值（用于着色）。
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Features As NumericTable

        ''' <summary>
        ''' 最多显示的特征数量（对应 shap 的 <c>max_display</c>）。
        ''' </summary>
        ''' <returns></returns>
        Public Property MaxDisplay As Integer = ShapPlotUtils.DefaultMaxDisplay

        ''' <summary>
        ''' 点的直径（像素）；小于等于 0 时使用主题的 <see cref="PlotTheme.MarkerSize"/>。
        ''' </summary>
        ''' <returns></returns>
        Public Property MarkerSize As Single = 4

        ''' <summary>
        ''' 点的透明度（0~255），默认半透明以便观察重叠密度。
        ''' </summary>
        ''' <returns></returns>
        Public Property MarkerAlpha As Integer = 200

        ''' <summary>
        ''' 行内纵向抖动幅度（以特征行高为单位），用于打散重叠的点。
        ''' </summary>
        ''' <returns></returns>
        Public Property JitterWidth As Single = 0.35F

        ''' <summary>
        ''' 抖动的随机种子；给定之后图形可复现。
        ''' </summary>
        ''' <returns></returns>
        Public Property RandomSeed As Integer? = 42

        ''' <summary>
        ''' 是否绘制 <c>x = 0</c> 的参考虚线。
        ''' </summary>
        ''' <returns></returns>
        Public Property ShowZeroLine As Boolean = True

        ''' <summary>
        ''' 创建蜂群图（引擎自建画布）。
        ''' </summary>
        ''' <param name="shap">SHAP 分析结果</param>
        ''' <param name="features">样本特征表</param>
        ''' <param name="width">画布宽度</param>
        ''' <param name="height">画布高度</param>
        ''' <param name="theme">绘图主题</param>
        ''' <param name="driver">绘图驱动</param>
        Public Sub New(shap As ShapAnalysisResult, features As NumericTable,
                       width As Integer, height As Integer,
                       Optional theme As PlotTheme = Nothing,
                       Optional driver As Drivers = Drivers.Default)
            MyBase.New(width, height, theme, driver)

            Me.Shap = shap
            Me.Features = features
        End Sub

        ''' <summary>
        ''' 创建蜂群图（直接绘制到外部提供的绘图设备上，用于图层叠加或宿主控件）。
        ''' </summary>
        ''' <param name="shap">SHAP 分析结果</param>
        ''' <param name="features">样本特征表</param>
        ''' <param name="g">目标绘图设备</param>
        ''' <param name="theme">绘图主题</param>
        Public Sub New(shap As ShapAnalysisResult, features As NumericTable,
                       g As IGraphics, Optional theme As PlotTheme = Nothing)
            MyBase.New(g, theme)

            Me.Shap = shap
            Me.Features = features
        End Sub

        Protected Overrides Sub PlotInternal(ByRef g As IGraphics, canvas As GraphicsRegion)
            Call ShapPlotUtils.Validate(Shap, Features)

            Call DrawBackground()
            Call ComputePlotArea()
            Call DrawPlotArea()
            Call DrawTitle()

            Dim top As Integer() = ShapPlotUtils.TopFeatures(Shap, MaxDisplay)
            Dim p As Integer = top.Length
            Dim n As Integer = ShapPlotUtils.SampleCount(Shap)

            ' 一次性把需要的列取出来，避免在逐点循环之中反复查找
            Dim shapColumns As Double()() = New Double(p - 1)() {}
            Dim valueColumns As Double()() = New Double(p - 1)() {}
            Dim allValues As New List(Of Double)()

            For r As Integer = 0 To p - 1
                shapColumns(r) = ShapPlotUtils.ContributionColumn(Shap, top(r))
                valueColumns(r) = ShapPlotUtils.FeatureColumn(Features, top(r))
                Call allValues.AddRange(shapColumns(r))
            Next

            Dim range = ShapPlotUtils.SymmetricRange(allValues)
            Dim xmin As Double = range.min
            Dim xmax As Double = range.max

            ' 重要性最高的特征放在最上方
            Dim yTicks As Double() = New Double(p - 1) {}
            Dim yLabels As String() = New String(p - 1) {}

            For r As Integer = 0 To p - 1
                yTicks(r) = CDbl(p - 1 - r)
                yLabels(r) = ShapPlotUtils.FeatureName(Shap, top(r))
            Next

            Call DrawAxisAndGrid(xmin, xmax, -0.5, p - 0.5,
                                 GenerateTicks(xmin, xmax), yTicks, Nothing, yLabels)

            If ShowZeroLine Then
                Dim px As Single = ToPixelX(0, xmin, xmax)

                Using pen As New Pen(Theme.AxisColor, 0.8F)
                    pen.DashStyle = DashStyle.Dash
                    g.DrawLine(pen, px, _plotArea.Top, px, _plotArea.Bottom)
                End Using
            End If

            Dim rnd As Random = If(RandomSeed.HasValue, New Random(RandomSeed.Value), New Random())
            Dim marker As Single = If(MarkerSize > 0, MarkerSize, Theme.MarkerSize)
            Dim alpha As Integer = std.Max(0, std.Min(255, MarkerAlpha))

            For r As Integer = 0 To p - 1
                Dim t As Double() = ShapPlotUtils.Normalize(valueColumns(r))
                Dim yc As Double = p - 1 - r
                Dim points As Integer = std.Min(n, shapColumns(r).Length)

                For i As Integer = 0 To points - 1
                    Dim px As Single = ToPixelX(shapColumns(r)(i), xmin, xmax)
                    Dim jitter As Double = (rnd.NextDouble() - 0.5) * JitterWidth
                    Dim py As Single = ToPixelY(yc + jitter, -0.5, p - 0.5)
                    Dim color As Color = ShapPlotUtils.ValueColor(If(t.Length > i, t(i), 0.5))

                    Using br As New SolidBrush(Color.FromArgb(alpha, color))
                        g.FillEllipse(br, px - marker / 2, py - marker / 2, marker, marker)
                    End Using
                Next
            Next

            ' 特征值高低的渐变图例（低蓝高红），贴在绘图区右下角内部以免超出画布
            Dim legendWidth As Single = std.Min(_plotArea.Width * 0.3F, 220.0F)
            Dim legend As New RectangleF(_plotArea.Right - legendWidth - 12, _plotArea.Bottom - 34,
                                         legendWidth, 10.0F)

            Call ShapPlotUtils.DrawValueGradient(g, Theme, legend, "Feature value")
        End Sub
    End Class

End Namespace
