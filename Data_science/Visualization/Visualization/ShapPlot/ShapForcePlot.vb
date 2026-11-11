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
'  ShapPlot / ShapForcePlot.vb
'  Copyright (c) 2018-2026 sciBASIC.NET Foundation, GPL3 Licensed
'
'  SHAP 力导向图（force）：与瀑布图信息等价，但以紧凑的水平布局呈现单样本归因。
' ---------------------------------------------------------------------------

Namespace ShapPlot

    ''' <summary>
    ''' SHAP 单样本力导向图（对应 python shap 的 <c>shap.plots.force</c>）。
    ''' 
    ''' 把预测推高的特征显示为红色、推向更低的特征显示为蓝色，
    ''' 各段从基准值出发向两侧伸展，段长与 SHAP 值的绝对值成正比。
    ''' </summary>
    Public Class ShapForcePlot
        Inherits PlotEngine

        Public ReadOnly Property Shap As ShapAnalysisResult
        Public ReadOnly Property Features As NumericTable

        ''' <summary>
        ''' 需要解释的样本下标（<see cref="SampleId"/> 非空时被忽略）。
        ''' </summary>
        ''' <returns></returns>
        Public Property SampleIndex As Integer = 0

        ''' <summary>
        ''' 需要解释的样本 ID；非空时优先按 ID 定位样本。
        ''' </summary>
        ''' <returns></returns>
        Public Property SampleId As String

        ''' <summary>
        ''' 最多显示的特征数量，其余特征合并为一段。
        ''' </summary>
        ''' <returns></returns>
        Public Property MaxDisplay As Integer = 10

        ''' <summary>
        ''' 色带的高度（像素）。
        ''' </summary>
        ''' <returns></returns>
        Public Property BandHeight As Single = 46

        ''' <summary>
        ''' 段间箭头的高度（像素）。
        ''' </summary>
        ''' <returns></returns>
        Public Property ArrowHeight As Single = 11

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

            Dim index As Integer = If(String.IsNullOrEmpty(SampleId), SampleIndex, ShapPlotUtils.FindSample(Shap, SampleId))

            If index < 0 OrElse index >= ShapPlotUtils.SampleCount(Shap) Then
                index = 0
            End If

            Dim group As ShapContributionGroup = ShapPlotUtils.AggregateTop(Shap, Features, index, MaxDisplay)
            Dim segments As New List(Of (name As String, value As Double, x0 As Double, x1 As Double, color As Color))()

            ' 正向段：从基准值向右依次累加（小贡献靠近基准值）
            Dim running As Double = 0

            For Each item In group.Items.Where(Function(i) i.value > 0).OrderBy(Function(i) i.value)
                Call segments.Add((item.name, item.value, running, running + item.value, ShapPlotUtils.PositiveColor))
                running += item.value
            Next

            If group.Others > 0 Then
                Call segments.Add(($"其余 {group.OthersCount} 个特征", group.Others, running, running + group.Others,
                                   ShapPlotUtils.PositiveColor))
                running += group.Others
            End If

            Dim positiveEnd As Double = running

            ' 负向段：从基准值向左依次累加
            running = 0

            For Each item In group.Items.Where(Function(i) i.value < 0).OrderBy(Function(i) std.Abs(i.value))
                Call segments.Add((item.name, item.value, running + item.value, running, ShapPlotUtils.NegativeColor))
                running += item.value
            Next

            If group.Others < 0 Then
                Call segments.Add(($"其余 {group.OthersCount} 个特征", group.Others, running + group.Others, running,
                                   ShapPlotUtils.NegativeColor))
                running += group.Others
            End If

            Dim negativeEnd As Double = running

            ' 值域覆盖实际的负向 / 正向段，同时保证基准值 0 一定落在轴内
            Dim xmin As Double = std.Min(negativeEnd, 0) * 1.12
            Dim xmax As Double = std.Max(positiveEnd, 0) * 1.12

            If xmax - xmin <= 0 Then
                xmin = -1
                xmax = 1
            End If

            If String.IsNullOrEmpty(Title) Then
                Title = If(String.IsNullOrEmpty(group.SampleId), "SHAP force plot", $"SHAP force plot [{group.SampleId}]")
            End If
            If String.IsNullOrEmpty(SubTitle) Then
                SubTitle = $"base = {FormatNumber(group.Baseline)}  →  f(X) = {FormatNumber(group.Output)}"
            End If

            Call DrawBackground()
            Call ComputePlotArea()
            Call DrawPlotArea()
            Call DrawTitle()

            ' ---- 版面布局（以像素为单位，色带位于绘图区中部偏上，坐标轴紧贴其下）----
            Dim bandHeightPx As Single = std.Max(12.0F, BandHeight)
            Dim bandTop As Single = _plotArea.Top + (_plotArea.Height - bandHeightPx) * 0.5F
            Dim bandBottom As Single = bandTop + bandHeightPx
            Dim bandMid As Single = (bandTop + bandBottom) / 2
            Dim axisY As Single = bandBottom + 56
            Dim ticks As Double() = GenerateTicks(xmin, xmax, 6)

            ' ---- 坐标轴线、网格与刻度 ----
            Using gridPen As New Pen(Theme.GridColor, Theme.GridLineWidth),
                  axisPen As New Pen(Theme.AxisColor, Theme.AxisLineWidth),
                  br As New SolidBrush(Theme.TextColor)

                g.DrawLine(axisPen, _plotArea.Left, axisY, _plotArea.Right, axisY)

                For Each t As Double In ticks
                    If t < xmin OrElse t > xmax Then
                        Continue For
                    End If

                    Dim px As Single = ToPixelX(t, xmin, xmax)
                    g.DrawLine(gridPen, px, _plotArea.Top, px, axisY)
                    g.DrawLine(axisPen, px, axisY, px, axisY + 5)

                    Dim label As String = FormatNumber(t)
                    Dim w As Single = g.MeasureString(label, Theme.TickLabelFont).Width
                    g.DrawString(label, Theme.TickLabelFont, br, px - w / 2, axisY + 7)
                Next
            End Using

            For i As Integer = 0 To segments.Count - 1
                Dim seg = segments(i)
                Dim px0 As Single = ToPixelX(seg.x0, xmin, xmax)
                Dim px1 As Single = ToPixelX(seg.x1, xmin, xmax)
                Dim left As Single = std.Min(px0, px1)
                Dim width As Single = std.Max(std.Abs(px1 - px0), 1.0F)
                Dim rect As New RectangleF(left, bandTop, width, bandBottom - bandTop)
                Dim tip As Single = If(seg.value >= 0, rect.Right, rect.Left)
                Dim dir As Single = If(seg.value >= 0, 1.0F, -1.0F)

                Using br As New SolidBrush(Color.FromArgb(230, seg.color))
                    g.FillRectangle(br, rect)

                    ' 段间箭头
                    Dim pts As PointF() = {
                        New PointF(tip, bandMid),
                        New PointF(tip - dir * ArrowHeight, bandMid - ArrowHeight / 2),
                        New PointF(tip - dir * ArrowHeight, bandMid + ArrowHeight / 2)
                    }
                    g.FillPolygon(br, pts)
                End Using

                Using pen As New Pen(Color.White, 1.2F)
                    g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height)
                End Using

                ' 特征名（宽度足够时才标注，交替上下两行以减少重叠）
                If rect.Width >= 26 Then
                    Dim labelY As Single = If(i Mod 2 = 0, bandTop - 20, bandTop - 38)

                    Using br As New SolidBrush(Theme.TextColor)
                        Dim w As Single = g.MeasureString(seg.name, Theme.AnnotationFont).Width
                        g.DrawString(seg.name, Theme.AnnotationFont, br, rect.X + rect.Width / 2 - w / 2, labelY)
                    End Using
                End If
            Next

            ' ---- 基准值参考线 ----
            Dim pxBase As Single = ToPixelX(0, xmin, xmax)

            Using pen As New Pen(Theme.AxisColor, 1.2F)
                pen.DashStyle = DashStyle.Dash
                g.DrawLine(pen, pxBase, bandTop - 6, pxBase, bandBottom + 6)
            End Using
        End Sub
    End Class

End Namespace
