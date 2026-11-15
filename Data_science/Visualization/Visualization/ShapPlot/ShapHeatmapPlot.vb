Imports System.Drawing
Imports Microsoft.VisualBasic.Data
Imports Microsoft.VisualBasic.Data.Plots
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Math.Statistics.ShapleyValue
Imports std = System.Math

' ---------------------------------------------------------------------------
'  ShapPlot / ShapHeatmapPlot.vb
'  Copyright (c) 2018-2026 sciBASIC.NET Foundation, GPL3 Licensed
'
'  SHAP 热力图（简化版）：以「样本 × 特征」矩阵按 SHAP 值着色。
'  与 readme 描述相比省略了顶部的模型输出曲线，只保留矩阵、色条与稀疏行列标签。
' ---------------------------------------------------------------------------

Namespace ShapPlot

    ''' <summary>
    ''' SHAP 热力图（对应 python shap 的 <c>shap.plots.heatmap</c> 的简化版）。
    ''' 
    ''' 以样本为行、特征为列的矩阵形式呈现 SHAP 值，样本默认按模型输出升序排序，
    ''' 便于观察特征贡献模式随样本的变化；本实现不含 readme 提到的输出曲线。
    ''' </summary>
    Public Class ShapHeatmapPlot
        Inherits PlotEngine

        Public ReadOnly Property Shap As ShapAnalysisResult
        Public ReadOnly Property Features As NumericTable

        ''' <summary>
        ''' 显示的特征数量（按重要性降序取前若干个）。
        ''' </summary>
        ''' <returns></returns>
        Public Property MaxDisplay As Integer = ShapPlotUtils.DefaultMaxDisplay

        ''' <summary>
        ''' 最多显示的样本数量；小于等于 0 表示显示全部样本。
        ''' </summary>
        ''' <returns></returns>
        Public Property MaxSamples As Integer = 0

        ''' <summary>
        ''' 是否按模型输出（加性值）对样本排序；关闭时保持数据原始顺序。
        ''' </summary>
        ''' <returns></returns>
        Public Property OrderByOutput As Boolean = True

        ''' <summary>
        ''' 色阶方案，默认使用以 0 为中心对称的 <see cref="ScalerPalette.Seismic"/>。
        ''' </summary>
        ''' <returns></returns>
        Public Property ColorMap As ScalerPalette = ScalerPalette.Seismic

        ''' <summary>
        ''' 是否绘制颜色条图例。
        ''' </summary>
        ''' <returns></returns>
        Public Property ShowColorLegend As Boolean = True

        ''' <summary>
        ''' 左侧留给样本 ID 标签的宽度（像素）。
        ''' </summary>
        ''' <returns></returns>
        Public Property LeftMargin As Single = 110

        ''' <summary>
        ''' 左侧最多绘制的样本 ID 标签数量（稀疏抽取，避免文字重叠）。
        ''' </summary>
        ''' <returns></returns>
        Public Property MaxRowLabels As Integer = 12

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
            Call DrawTitle()

            Dim top As Integer() = ShapPlotUtils.TopFeatures(Shap, MaxDisplay)
            Dim cols As Integer = top.Length
            Dim explanations As List(Of ShapExplanation) = Shap.Explanations

            ' 行序：默认按模型输出升序，便于观察趋势
            Dim order As Integer() = Enumerable.Range(0, explanations.Count).ToArray()

            If OrderByOutput Then
                Array.Sort(order, Function(a, b) explanations(a).AdditiveValue.CompareTo(explanations(b).AdditiveValue))
            End If
            If MaxSamples > 0 AndAlso order.Length > MaxSamples Then
                order = order.Take(MaxSamples).ToArray()
            End If

            Dim rows As Integer = order.Length
            Dim matrix As Double(,) = New Double(rows - 1, cols - 1) {}
            Dim allValues As New List(Of Double)()

            For r As Integer = 0 To rows - 1
                Dim contributions As Double() = explanations(order(r)).Contributions

                For c As Integer = 0 To cols - 1
                    Dim idx As Integer = top(c)
                    Dim v As Double = If(contributions Is Nothing OrElse idx >= contributions.Length, 0, contributions(idx))

                    matrix(r, c) = v
                    Call allValues.Add(v)
                Next
            Next

            Dim range = ShapPlotUtils.SymmetricRange(allValues)
            Dim vmin As Double = range.min
            Dim vmax As Double = range.max

            ' 自算绘图区：左侧放样本 ID、顶部放特征名、右侧放颜色条
            Dim left As Single = std.Max(Theme.MarginLeft, LeftMargin)
            Dim topPad As Single = Theme.MarginTop + 30
            Dim rightPad As Single = If(ShowColorLegend, Theme.ColorBarWidth + Theme.ColorBarGap + 60, Theme.MarginRight)

            _plotArea = New RectangleF(left, topPad,
                                       std.Max(_width - left - rightPad, 20.0F),
                                       std.Max(_height - topPad - Theme.MarginBottom, 20.0F))

            Dim cellW As Single = _plotArea.Width / cols
            Dim cellH As Single = _plotArea.Height / rows

            For r As Integer = 0 To rows - 1
                For c As Integer = 0 To cols - 1
                    Dim color As Color = ColorScale.GetColor(matrix(r, c), vmin, vmax, ColorMap)
                    Dim rect As New RectangleF(_plotArea.Left + c * cellW, _plotArea.Top + r * cellH,
                                               cellW + 0.5F, cellH + 0.5F)

                    Using br As New SolidBrush(color)
                        g.FillRectangle(br, rect)
                    End Using
                Next
            Next

            Using pen As New Pen(Theme.BorderColor, 0.6F)
                g.DrawRectangle(pen, _plotArea.X, _plotArea.Y, _plotArea.Width, _plotArea.Height)
            End Using

            ' 稀疏的样本 ID 标签
            Dim labelStep As Integer = std.Max(1, CInt(std.Ceiling(rows / std.Max(1.0, MaxRowLabels))))

            Using br As New SolidBrush(Theme.TextColor)
                For r As Integer = 0 To rows - 1 Step labelStep
                    Dim text As String = ShapPlotUtils.SampleId(Features, order(r))
                    Dim py As Single = _plotArea.Top + (r + 0.5F) * cellH
                    Dim w As Single = g.MeasureString(text, Theme.TickLabelFont).Width

                    g.DrawString(text, Theme.TickLabelFont, br, _plotArea.Left - w - 6,
                                 py - Theme.TickLabelFont.GetHeight(g) / 2)
                Next
            End Using

            ' 特征名（旋转 -45 度）
            Using br As New SolidBrush(Theme.TextColor)
                For c As Integer = 0 To cols - 1
                    Dim text As String = ShapPlotUtils.FeatureName(Shap, top(c))

                    g.TranslateTransform(_plotArea.Left + (c + 0.5F) * cellW, _plotArea.Top - 6)
                    g.RotateTransform(-45)
                    g.DrawString(text, Theme.TickLabelFont, br, 0, 0)
                    g.ResetTransform()
                Next
            End Using

            If ShowColorLegend Then
                Call DrawColorLegend(ColorMap, vmin, vmax, tickCount:=5, title:="SHAP value")
            End If
        End Sub
    End Class

End Namespace
