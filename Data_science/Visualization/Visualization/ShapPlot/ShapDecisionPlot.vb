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
'  ShapPlot / ShapDecisionPlot.vb
'  Copyright (c) 2018-2026 sciBASIC.NET Foundation, GPL3 Licensed
'
'  SHAP 决策图（decision）：展示累积 SHAP 值如何引导模型做出决策路径。
' ---------------------------------------------------------------------------

Namespace ShapPlot

    ''' <summary>
    ''' SHAP 决策图（对应 python shap 的 <c>shap.decision_plot</c>）。
    ''' 
    ''' 横轴为累积的 SHAP 值（从基准值到模型输出），纵轴为按重要性排列的特征；
    ''' 每条折线解释一个样本的决策路径，便于比较多条样本或者识别异常路径。
    ''' </summary>
    Public Class ShapDecisionPlot
        Inherits PlotEngine

        Public ReadOnly Property Shap As ShapAnalysisResult
        Public ReadOnly Property Features As NumericTable

        ''' <summary>
        ''' 最多绘制的折线数量（样本数量）；小于等于 0 表示绘制全部样本。
        ''' </summary>
        ''' <returns></returns>
        Public Property MaxSamples As Integer = 20

        ''' <summary>
        ''' 纵轴显示的特征数量。
        ''' </summary>
        ''' <returns></returns>
        Public Property MaxDisplay As Integer = 10

        ''' <summary>
        ''' 是否在折线末端标注模型的输出值（样本较多时只标注首批）。
        ''' </summary>
        ''' <returns></returns>
        Public Property ShowFinalValue As Boolean = True

        ''' <summary>
        ''' 标注输出值的最大样本数量，避免文字互相覆盖。
        ''' </summary>
        ''' <returns></returns>
        Public Property MaxLabels As Integer = 3

        ''' <summary>
        ''' 左侧留给特征名标签的宽度（像素）。
        ''' </summary>
        ''' <returns></returns>
        Public Property LeftMargin As Single = 200

        ''' <summary>
        ''' 折线的透明度（0~255）。
        ''' </summary>
        ''' <returns></returns>
        Public Property LineAlpha As Integer = 170

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

            Dim top As Integer() = ShapPlotUtils.TopFeatures(Shap, MaxDisplay)
            Dim p As Integer = top.Length

            ' 纵轴自下而上按重要性升序：最重要的特征在最上方
            Dim featureRows As Integer() = New Integer(p - 1) {}

            For r As Integer = 0 To p - 1
                featureRows(r) = top(p - 1 - r)
            Next

            Dim samples As Integer = ShapPlotUtils.SampleCount(Shap)

            If MaxSamples > 0 AndAlso MaxSamples < samples Then
                samples = MaxSamples
            End If

            ' 一次性把每条决策路径算出来（后面还要用它们求值域）
            Dim paths As New List(Of (x As Double(), y As Double(), output As Double, sample As String))()
            Dim allX As New List(Of Double)()
            Dim baseline As Double = Shap.Baseline

            For i As Integer = 0 To samples - 1
                Dim explanation As ShapExplanation = Shap.Explanations(i)
                Dim startValue As Double = If(explanation.Baseline <> 0, explanation.Baseline, baseline)
                Dim xs As Double() = New Double(p) {}
                Dim ys As Double() = New Double(p) {}
                Dim cum As Double = startValue

                xs(0) = cum
                ys(0) = 0

                For r As Integer = 0 To p - 1
                    Dim contributions As Double() = explanation.Contributions
                    Dim idx As Integer = featureRows(r)

                    If contributions IsNot Nothing AndAlso idx < contributions.Length Then
                        cum += contributions(idx)
                    End If

                    xs(r + 1) = cum
                    ys(r + 1) = r
                Next

                Call allX.AddRange(xs)
                Call paths.Add((xs, ys, cum, ShapPlotUtils.SampleId(Features, i)))
                Call allX.Add(startValue)
            Next

            Dim range = ShapPlotUtils.PadRange(allX.Min(), allX.Max(), 0.06)
            Dim xmin As Double = range.min
            Dim xmax As Double = range.max

            If String.IsNullOrEmpty(XLabel) Then
                XLabel = "model output"
            End If

            Dim yTicks As Double() = New Double(p - 1) {}
            Dim yLabels As String() = New String(p - 1) {}

            For r As Integer = 0 To p - 1
                yTicks(r) = r
                yLabels(r) = ShapPlotUtils.FeatureName(Shap, featureRows(r))
            Next

            Call DrawBackground()
            Call ApplyLeftMargin()
            Call DrawPlotArea()
            Call DrawTitle()

            Call DrawAxisAndGrid(xmin, xmax, -0.5, p - 0.5,
                                 GenerateTicks(xmin, xmax), yTicks, Nothing, yLabels)

            ' 基准值参考线
            Using pen As New Pen(Theme.AxisColor, 0.8F)
                pen.DashStyle = DashStyle.Dash
                g.DrawLine(pen, ToPixelX(baseline, xmin, xmax), _plotArea.Top,
                           ToPixelX(baseline, xmin, xmax), _plotArea.Bottom)
            End Using

            Dim alpha As Integer = std.Max(30, std.Min(255, LineAlpha))

            For i As Integer = 0 To paths.Count - 1
                Dim path = paths(i)
                Dim color As Color = Theme.Palette(i Mod Theme.Palette.Length)

                Using pen As New Pen(Color.FromArgb(alpha, color), 1.6F)
                    For k As Integer = 1 To path.x.Length - 1
                        g.DrawLine(pen,
                                   ToPixelX(path.x(k - 1), xmin, xmax), ToPixelY(path.y(k - 1), -0.5, p - 0.5),
                                   ToPixelX(path.x(k), xmin, xmax), ToPixelY(path.y(k), -0.5, p - 0.5))
                    Next
                End Using

                ' 末端标记
                Dim px As Single = ToPixelX(path.x(path.x.Length - 1), xmin, xmax)
                Dim py As Single = ToPixelY(path.y(path.y.Length - 1), -0.5, p - 0.5)

                Using br As New SolidBrush(color)
                    g.FillEllipse(br, px - 3, py - 3, 6, 6)
                End Using

                If ShowFinalValue AndAlso i < MaxLabels Then
                    ' 交替上下错开，减少末端标注之间的重叠
                    Dim offsetY As Single = If(i Mod 2 = 0, -7, 9)

                    Using br As New SolidBrush(Theme.TextColor)
                        Call DrawLabel(FormatNumber(path.output), Theme.AnnotationFont, br, px + 6, py + offsetY)
                    End Using
                End If
            Next
        End Sub

        ''' <summary>
        ''' 按 <see cref="LeftMargin"/> 重新计算绘图区，给特征名标签留出足够的空间。
        ''' </summary>
        Private Sub ApplyLeftMargin()
            Dim left As Single = std.Max(Theme.MarginLeft, LeftMargin)

            _plotArea = New RectangleF(left, Theme.MarginTop,
                                       std.Max(_width - left - Theme.MarginRight, 10.0F),
                                       std.Max(_height - Theme.MarginTop - Theme.MarginBottom, 10.0F))
        End Sub
    End Class

End Namespace
