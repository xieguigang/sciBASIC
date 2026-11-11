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
'  ShapPlot / ShapDependencePlot.vb
'  Copyright (c) 2018-2026 sciBASIC.NET Foundation, GPL3 Licensed
'
'  SHAP 依赖图（dependence / scatter）：特征取值与其 SHAP 值的关系，并按交互特征着色。
' ---------------------------------------------------------------------------

Namespace ShapPlot

    ''' <summary>
    ''' SHAP 依赖图（对应 python shap 的 <c>shap.plots.scatter</c> / <c>shap.dependence_plot</c>）。
    ''' 
    ''' 横轴是指定特征的实际取值，纵轴是该特征的 SHAP 值；点的颜色是交互特征的取值，
    ''' 在未指定交互特征时自动选择与 SHAP 值相关性最高的其他特征。
    ''' </summary>
    Public Class ShapDependencePlot
        Inherits PlotEngine

        Public ReadOnly Property Shap As ShapAnalysisResult
        Public ReadOnly Property Features As NumericTable

        ''' <summary>
        ''' 需要分析的特征名字（必填，除非指定了 <see cref="FeatureIndex"/>）。
        ''' </summary>
        ''' <returns></returns>
        Public Property FeatureName As String

        ''' <summary>
        ''' 需要分析的特征下标；小于 0 时按 <see cref="FeatureName"/> 查找。
        ''' </summary>
        ''' <returns></returns>
        Public Property FeatureIndex As Integer = -1

        ''' <summary>
        ''' 用于着色的交互特征名字；留空时自动选择相关性最高的特征。
        ''' </summary>
        ''' <returns></returns>
        Public Property InteractionFeature As String

        ''' <summary>
        ''' 是否叠加线性趋势线（最小二乘拟合）。
        ''' </summary>
        ''' <returns></returns>
        Public Property ShowTrendLine As Boolean = True

        ''' <summary>
        ''' 是否绘制 <c>y = 0</c> 的参考虚线。
        ''' </summary>
        ''' <returns></returns>
        Public Property ShowZeroLine As Boolean = True

        ''' <summary>
        ''' 标记点的直径（像素）。
        ''' </summary>
        ''' <returns></returns>
        Public Property MarkerSize As Single = 6

        ''' <summary>
        ''' 标记点的透明度（0~255）。
        ''' </summary>
        ''' <returns></returns>
        Public Property MarkerAlpha As Integer = 210

        Public Sub New(shap As ShapAnalysisResult, features As NumericTable,
                       featureName As String,
                       width As Integer, height As Integer,
                       Optional theme As PlotTheme = Nothing,
                       Optional driver As Drivers = Drivers.Default)
            MyBase.New(width, height, theme, driver)

            Me.Shap = shap
            Me.Features = features
            Me.FeatureName = featureName
        End Sub

        Public Sub New(shap As ShapAnalysisResult, features As NumericTable,
                       featureName As String,
                       g As IGraphics, Optional theme As PlotTheme = Nothing)
            MyBase.New(g, theme)

            Me.Shap = shap
            Me.Features = features
            Me.FeatureName = featureName
        End Sub

        Protected Overrides Sub PlotInternal(ByRef g As IGraphics, canvas As GraphicsRegion)
            Call ShapPlotUtils.Validate(Shap, Features)

            Dim idx As Integer = ResolveFeature()
            Dim x As Double() = ShapPlotUtils.FeatureColumn(Features, idx)
            Dim y As Double() = ShapPlotUtils.ContributionColumn(Shap, idx)
            Dim interaction As Integer = ResolveInteraction(idx, y)
            Dim interactionValues As Double() = ShapPlotUtils.FeatureColumn(Features, interaction)
            Dim shade As Double() = ShapPlotUtils.Normalize(interactionValues)

            Dim yRange = ShapPlotUtils.SymmetricRange(y)
            Dim xRange = ShapPlotUtils.PadRange(x.Min(), x.Max(), 0.06)

            If String.IsNullOrEmpty(XLabel) Then
                XLabel = ShapPlotUtils.FeatureName(Shap, idx)
            End If
            If String.IsNullOrEmpty(YLabel) Then
                YLabel = "SHAP value"
            End If
            If String.IsNullOrEmpty(Title) Then
                Title = $"SHAP dependence [{ShapPlotUtils.FeatureName(Shap, idx)}]"
            End If
            If String.IsNullOrEmpty(SubTitle) Then
                SubTitle = $"color: {ShapPlotUtils.FeatureName(Shap, interaction)}"
            End If

            Call DrawBackground()
            Call ComputePlotArea()

            ' 右侧留出交互特征色条的位置，避免色条被画布右边缘裁切
            _plotArea = New RectangleF(_plotArea.X, _plotArea.Y,
                                       std.Max(_plotArea.Width - 96, 40.0F), _plotArea.Height)

            Call DrawPlotArea()
            Call DrawTitle()

            Call DrawAxisAndGrid(xRange.min, xRange.max, yRange.min, yRange.max,
                                 GenerateTicks(xRange.min, xRange.max), GenerateTicks(yRange.min, yRange.max))

            If ShowZeroLine Then
                Dim py As Single = ToPixelY(0, yRange.min, yRange.max)

                Using pen As New Pen(Theme.AxisColor, 0.8F)
                    pen.DashStyle = DashStyle.Dash
                    g.DrawLine(pen, _plotArea.Left, py, _plotArea.Right, py)
                End Using
            End If

            If ShowTrendLine Then
                Dim fit = LinearFit(x, y)

                Using pen As New Pen(ShapPlotUtils.PositiveColor, 1.6F)
                    Call DrawAbline(fit.a, fit.b, xRange.min, xRange.max, yRange.min, yRange.max, pen)
                End Using
            End If

            Dim marker As Single = If(MarkerSize > 0, MarkerSize, Theme.MarkerSize)
            Dim alpha As Integer = std.Max(0, std.Min(255, MarkerAlpha))

            For i As Integer = 0 To y.Length - 1
                Dim px As Single = ToPixelX(x(i), xRange.min, xRange.max)
                Dim py As Single = ToPixelY(y(i), yRange.min, yRange.max)
                Dim color As Color = ShapPlotUtils.ValueColor(If(shade.Length > i, shade(i), 0.5))

                Using br As New SolidBrush(Color.FromArgb(alpha, color))
                    g.FillEllipse(br, px - marker / 2, py - marker / 2, marker, marker)
                End Using
            Next

            ' 交互特征取值的渐变图例
            Dim legend As New RectangleF(_plotArea.Right + 18, _plotArea.Top, 14, std.Max(_plotArea.Height * 0.6F, 40.0F))

            Call ShapPlotUtils.DrawValueGradient(g, Theme, legend, ShapPlotUtils.FeatureName(Shap, interaction))
        End Sub

        ''' <summary>
        ''' 解析需要分析的特征下标。
        ''' </summary>
        Private Function ResolveFeature() As Integer
            If FeatureIndex >= 0 Then
                Return FeatureIndex
            End If

            If String.IsNullOrEmpty(FeatureName) Then
                Throw New ArgumentException("ShapDependencePlot: a feature name (or feature index) is required!")
            End If

            If Shap.FeatureNames IsNot Nothing Then
                For i As Integer = 0 To Shap.FeatureNames.Length - 1
                    If String.Equals(Shap.FeatureNames(i), FeatureName, StringComparison.OrdinalIgnoreCase) Then
                        Return i
                    End If
                Next
            End If

            Throw New ArgumentException($"ShapDependencePlot: the feature '{FeatureName}' was not found in the analysis result!")
        End Function

        ''' <summary>
        ''' 解析用于着色的交互特征：显式指定时按名字查找，否则选取与 SHAP 值相关性最高的其他特征。
        ''' </summary>
        Private Function ResolveInteraction(featureIndex As Integer, shapValues As Double()) As Integer
            Dim width As Integer = ShapPlotUtils.FeatureCount(Shap)

            If Not String.IsNullOrEmpty(InteractionFeature) AndAlso Shap.FeatureNames IsNot Nothing Then
                For i As Integer = 0 To Shap.FeatureNames.Length - 1
                    If String.Equals(Shap.FeatureNames(i), InteractionFeature, StringComparison.OrdinalIgnoreCase) Then
                        Return i
                    End If
                Next
            End If

            Dim best As Integer = featureIndex
            Dim bestScore As Double = -1

            For i As Integer = 0 To width - 1
                If i = featureIndex Then
                    Continue For
                End If

                Dim score As Double = std.Abs(Correlation(ShapPlotUtils.FeatureColumn(Features, i), shapValues))

                If score > bestScore Then
                    bestScore = score
                    best = i
                End If
            Next

            Return best
        End Function

        ''' <summary>
        ''' 最小二乘线性拟合 <c>y = a + b * x</c>。
        ''' </summary>
        Private Shared Function LinearFit(x As Double(), y As Double()) As (a As Double, b As Double)
            Dim n As Integer = std.Min(x.Length, y.Length)

            If n < 2 Then
                Return (0, 0)
            End If

            Dim sx As Double = 0, sy As Double = 0, sxx As Double = 0, sxy As Double = 0

            For i As Integer = 0 To n - 1
                sx += x(i)
                sy += y(i)
                sxx += x(i) * x(i)
                sxy += x(i) * y(i)
            Next

            Dim denom As Double = n * sxx - sx * sx

            If std.Abs(denom) < 0.000000000001 Then
                Return (sy / n, 0)
            End If

            Dim b As Double = (n * sxy - sx * sy) / denom
            Dim a As Double = (sy - b * sx) / n

            Return (a, b)
        End Function

        ''' <summary>
        ''' 皮尔逊相关系数；任一序列方差为零时返回 0。
        ''' </summary>
        Private Shared Function Correlation(x As Double(), y As Double()) As Double
            Dim n As Integer = std.Min(x.Length, y.Length)

            If n < 2 Then
                Return 0
            End If

            Dim mx As Double = 0, my As Double = 0

            For i As Integer = 0 To n - 1
                mx += x(i)
                my += y(i)
            Next

            mx /= n
            my /= n

            Dim sxy As Double = 0, sxx As Double = 0, syy As Double = 0

            For i As Integer = 0 To n - 1
                Dim dx As Double = x(i) - mx
                Dim dy As Double = y(i) - my

                sxy += dx * dy
                sxx += dx * dx
                syy += dy * dy
            Next

            If sxx <= 0 OrElse syy <= 0 Then
                Return 0
            End If

            Return sxy / std.Sqrt(sxx * syy)
        End Function
    End Class

End Namespace
