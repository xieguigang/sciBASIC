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
'  ShapPlot / ShapViolinPlot.vb
'  Copyright (c) 2018-2026 sciBASIC.NET Foundation, GPL3 Licensed
'
'  SHAP 小提琴图（简化版）：各特征 SHAP 值的核密度分布，可视为蜂群图的平滑替代。
'  轮廓复用 DataPlot 的 Geometry.Density1D（高斯核 + Silverman 带宽）。
' ---------------------------------------------------------------------------

Namespace ShapPlot

    ''' <summary>
    ''' SHAP 小提琴图（对应 python shap 的 <c>summary_plot(plot_type:="violin")</c> 的简化版）。
    ''' 
    ''' 每一行是一个特征的 SHAP 值分布，用高斯核密度估计得到平滑轮廓；
    ''' 按照约定本实现不内嵌箱线图，只绘制轮廓与中位数标记。
    ''' </summary>
    Public Class ShapViolinPlot
        Inherits PlotEngine

        Public ReadOnly Property Shap As ShapAnalysisResult
        Public ReadOnly Property Features As NumericTable

        ''' <summary>
        ''' 最多显示的特征数量。
        ''' </summary>
        ''' <returns></returns>
        Public Property MaxDisplay As Integer = ShapPlotUtils.DefaultMaxDisplay

        ''' <summary>
        ''' 核密度估计的带宽；小于等于 0 时按 Silverman 经验法则自动估计。
        ''' </summary>
        ''' <returns></returns>
        Public Property Bandwidth As Double = 0

        ''' <summary>
        ''' 密度轮廓的采样点数量。
        ''' </summary>
        ''' <returns></returns>
        Public Property DensityPoints As Integer = 128

        ''' <summary>
        ''' 小提琴轮廓在行内占用的高度比例（0~1）。
        ''' </summary>
        ''' <returns></returns>
        Public Property ViolinWidth As Single = 0.45F

        ''' <summary>
        ''' 是否绘制中位数竖线。
        ''' </summary>
        ''' <returns></returns>
        Public Property ShowMedian As Boolean = True

        ''' <summary>
        ''' 是否绘制 <c>x = 0</c> 的参考虚线。
        ''' </summary>
        ''' <returns></returns>
        Public Property ShowZeroLine As Boolean = True

        Public Sub New(shap As ShapAnalysisResult, features As NumericTable,
                       width As Integer, height As Integer,
                       Optional theme As PlotTheme = Nothing,
                       Optional driver As Drivers = Drivers.Default)
            MyBase.New(width, height, theme, driver)

            Me.Shap = shap
            Me.Features = features
        End Sub

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

            Dim columns As Double()() = New Double(p - 1)() {}
            Dim allValues As New List(Of Double)()

            For r As Integer = 0 To p - 1
                columns(r) = ShapPlotUtils.ContributionColumn(Shap, top(r))
                Call allValues.AddRange(columns(r))
            Next

            Dim range = ShapPlotUtils.SymmetricRange(allValues)
            Dim xmin As Double = range.min
            Dim xmax As Double = range.max

            If String.IsNullOrEmpty(XLabel) Then
                XLabel = "SHAP value"
            End If

            Dim yTicks As Double() = New Double(p - 1) {}
            Dim yLabels As String() = New String(p - 1) {}

            For r As Integer = 0 To p - 1
                yTicks(r) = CDbl(p - 1 - r)
                yLabels(r) = ShapPlotUtils.FeatureName(Shap, top(r))
            Next

            Call DrawAxisAndGrid(xmin, xmax, -0.5, p - 0.5,
                                 GenerateTicks(xmin, xmax), yTicks, Nothing, yLabels)

            If ShowZeroLine Then
                Dim pxZero As Single = ToPixelX(0, xmin, xmax)

                Using pen As New Pen(Theme.AxisColor, 0.8F)
                    pen.DashStyle = DashStyle.Dash
                    g.DrawLine(pen, pxZero, _plotArea.Top, pxZero, _plotArea.Bottom)
                End Using
            End If

            Dim halfBand As Double = std.Max(0.05, std.Min(0.5, ViolinWidth))

            For r As Integer = 0 To p - 1
                Dim sample As Double() = columns(r)

                If sample Is Nothing OrElse sample.Length = 0 Then
                    Continue For
                End If

                Dim density = Geometry.Density1D(sample, n:=std.Max(16, DensityPoints), bandwidth:=Bandwidth)

                If density.x Is Nothing OrElse density.x.Length < 2 Then
                    Continue For
                End If

                Dim peak As Double = density.y.Max()

                If peak <= 0 Then
                    Continue For
                End If

                Dim yc As Double = p - 1 - r
                Dim upper As New List(Of PointF)()
                Dim lower As New List(Of PointF)()

                For i As Integer = 0 To density.x.Length - 1
                    Dim x As Double = density.x(i)

                    If x < xmin OrElse x > xmax Then
                        Continue For
                    End If

                    Dim px As Single = ToPixelX(x, xmin, xmax)
                    Dim d As Double = density.y(i) / peak * halfBand

                    Call upper.Add(New PointF(px, ToPixelY(yc + d, -0.5, p - 0.5)))
                    Call lower.Add(New PointF(px, ToPixelY(yc - d, -0.5, p - 0.5)))
                Next

                If upper.Count < 2 Then
                    Continue For
                End If

                Call lower.Reverse()

                Dim outline As New List(Of PointF)(upper)
                Call outline.AddRange(lower)

                Dim color As Color = Theme.Palette(r Mod Theme.Palette.Length)

                Using br As New SolidBrush(Color.FromArgb(170, color)),
                      pen As New Pen(color, Theme.LineWidth)
                    g.FillPolygon(br, outline.ToArray())
                    g.DrawPolygon(pen, outline.ToArray())
                End Using

                If ShowMedian Then
                    ' 注意：局部变量不能与 Median 函数同名（VB 的大小写不敏感作用域提升会让调用变成索引访问）
                    Dim medianValue As Double = Median(sample)
                    Dim pxMedian As Single = ToPixelX(medianValue, xmin, xmax)

                    Using pen As New Pen(Color.FromArgb(120, 30, 30, 30), Theme.LineWidth)
                        g.DrawLine(pen, pxMedian, ToPixelY(yc + halfBand, -0.5, p - 0.5),
                                   pxMedian, ToPixelY(yc - halfBand, -0.5, p - 0.5))
                    End Using
                End If
            Next
        End Sub

        ''' <summary>
        ''' 计算中位数。
        ''' </summary>
        Private Shared Function Median(values As Double()) As Double
            Dim sorted As Double() = values.Where(Function(v) Not Double.IsNaN(v)).OrderBy(Function(v) v).ToArray()

            If sorted.Length = 0 Then
                Return 0
            End If

            Dim mid As Integer = sorted.Length \ 2

            If sorted.Length Mod 2 = 1 Then
                Return sorted(mid)
            Else
                Return (sorted(mid - 1) + sorted(mid)) / 2
            End If
        End Function
    End Class

End Namespace
