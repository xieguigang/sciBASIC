Imports System.Drawing
Imports Microsoft.VisualBasic.Data
Imports Microsoft.VisualBasic.Data.Plots
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Math.Statistics.ShapleyValue
Imports std = System.Math

' ---------------------------------------------------------------------------
'  ShapPlot / ShapBarPlot.vb
'  Copyright (c) 2018-2026 sciBASIC.NET Foundation, GPL3 Licensed
'
'  SHAP 全局特征重要性柱状图（bar）：按平均绝对 SHAP 值排序的水平柱状图。
' ---------------------------------------------------------------------------

Namespace ShapPlot

    ''' <summary>
    ''' SHAP 特征重要性柱状图（对应 python shap 的 <c>shap.plots.bar</c> /
    ''' <c>shap.summary_plot(plot_type:="bar")</c>）。
    ''' 
    ''' 每个特征的柱子长度为其在所有样本上 SHAP 值的绝对平均值，丢失方向信息，
    ''' 只反映重要性大小，适合作为报告或者论文之中的简化展示。
    ''' </summary>
    Public Class ShapBarPlot
        Inherits PlotEngine

        Public ReadOnly Property Shap As ShapAnalysisResult
        Public ReadOnly Property Features As NumericTable

        ''' <summary>
        ''' 最多显示的特征数量。
        ''' </summary>
        ''' <returns></returns>
        Public Property MaxDisplay As Integer = ShapPlotUtils.DefaultMaxDisplay

        ''' <summary>
        ''' 是否在柱端标注具体的平均绝对 SHAP 值。
        ''' </summary>
        ''' <returns></returns>
        Public Property ShowValueLabel As Boolean = True

        ''' <summary>
        ''' 是否按照平均有符号 SHAP 值的正负着色（正红负蓝）；关闭时使用主题调色板。
        ''' </summary>
        ''' <returns></returns>
        Public Property SignedColor As Boolean = True

        ''' <summary>
        ''' 柱子的填充色；当 <see cref="SignedColor"/> 为 True 时该属性被忽略。
        ''' </summary>
        ''' <returns></returns>
        Public Property BarColor As Color = Color.Empty

        ''' <summary>
        ''' 柱子的透明度（0~255）。
        ''' </summary>
        ''' <returns></returns>
        Public Property BarAlpha As Integer = 220

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

            Dim importance As Double() = New Double(p - 1) {}

            For r As Integer = 0 To p - 1
                importance(r) = Shap.MeanAbsShap(top(r))
            Next

            Dim xmax As Double = importance.Max()

            If xmax <= 0 Then
                xmax = 1
            Else
                xmax *= 1.18
            End If

            If String.IsNullOrEmpty(XLabel) Then
                XLabel = "mean(|SHAP value|)"
            End If

            Dim yTicks As Double() = New Double(p - 1) {}
            Dim yLabels As String() = New String(p - 1) {}

            For r As Integer = 0 To p - 1
                yTicks(r) = CDbl(p - 1 - r)
                yLabels(r) = ShapPlotUtils.FeatureName(Shap, top(r))
            Next

            Call DrawAxisAndGrid(0, xmax, -0.5, p - 0.5,
                                 GenerateTicks(0, xmax), yTicks, Nothing, yLabels)

            Dim alpha As Integer = std.Max(0, std.Min(255, BarAlpha))
            Dim halfBand As Double = 0.38

            For r As Integer = 0 To p - 1
                Dim yc As Double = p - 1 - r
                Dim yTop As Single = ToPixelY(yc + halfBand, -0.5, p - 0.5)
                Dim yBottom As Single = ToPixelY(yc - halfBand, -0.5, p - 0.5)
                Dim right As Single = ToPixelX(importance(r), 0, xmax)
                Dim color As Color

                If SignedColor Then
                    color = If(Shap.MeanShap(top(r)) >= 0, ShapPlotUtils.PositiveColor, ShapPlotUtils.NegativeColor)
                ElseIf BarColor <> Color.Empty Then
                    color = BarColor
                Else
                    color = Theme.Palette(r Mod Theme.Palette.Length)
                End If

                Dim rect As New RectangleF(_plotArea.Left, yTop, std.Max(right - _plotArea.Left, 1.0F), yBottom - yTop)

                Using br As New SolidBrush(Color.FromArgb(alpha, color))
                    g.FillRectangle(br, rect)
                End Using

                Using pen As New Pen(color, Theme.LineWidth)
                    g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height)
                End Using

                If ShowValueLabel Then
                    Using br As New SolidBrush(Theme.TextColor)
                        Call DrawLabel(FormatNumber(importance(r)), Theme.AnnotationFont, br,
                                       rect.Right + 4, rect.Y + (rect.Height - Theme.AnnotationFont.GetHeight(g)) / 2)
                    End Using
                End If
            Next
        End Sub
    End Class

End Namespace
