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
'  ShapPlot / ShapWaterfallPlot.vb
'  Copyright (c) 2018-2026 sciBASIC.NET Foundation, GPL3 Licensed
'
'  SHAP 单样本瀑布图（waterfall）：从基准值出发，按贡献绝对值降序逐级叠加到模型输出。
' ---------------------------------------------------------------------------

Namespace ShapPlot

    ''' <summary>
    ''' SHAP 单样本瀑布图（对应 python shap 的 <c>shap.plots.waterfall</c>）。
    ''' 
    ''' 从基准值 <c>E[f(X)]</c> 出发，按特征贡献的绝对值从大到小逐级叠加，
    ''' 红色条表示正向推力、蓝色条表示负向推力，最终到达模型的输出值 <c>f(X)</c>。
    ''' 超出 <see cref="MaxDisplay"/> 的特征会被合并为「其余 N 个特征」。
    ''' </summary>
    Public Class ShapWaterfallPlot
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
        ''' 最多显示的特征数量，其余特征合并为一条「其余 N 个特征」。
        ''' </summary>
        ''' <returns></returns>
        Public Property MaxDisplay As Integer = 10

        ''' <summary>
        ''' 是否在特征名后面附加该样本的原始特征取值。
        ''' </summary>
        ''' <returns></returns>
        Public Property ShowFeatureValues As Boolean = True

        ''' <summary>
        ''' 左侧留给特征名标签的宽度（像素）。
        ''' </summary>
        ''' <returns></returns>
        Public Property LeftMargin As Single = 280

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
            Call ApplyLeftMargin()
            Call DrawPlotArea()
            Call DrawTitle()

            Dim index As Integer = If(String.IsNullOrEmpty(SampleId), SampleIndex, ShapPlotUtils.FindSample(Shap, SampleId))

            If index < 0 OrElse index >= ShapPlotUtils.SampleCount(Shap) Then
                index = 0
            End If

            Dim group As ShapContributionGroup = ShapPlotUtils.AggregateTop(Shap, Features, index, MaxDisplay)

            ' 行布局：0 = 基准值，1..m = 各个贡献，m + 1 = 模型输出
            Dim rows As New List(Of (label As String, value As Double, startValue As Double, endValue As Double))()
            Dim cum As Double = group.Baseline

            For Each item As (name As String, value As Double) In group.Items
                Dim label As String = item.name

                If ShowFeatureValues Then
                    Dim raw As Double? = FeatureValue(index, item.name)

                    If raw.HasValue Then
                        label = $"{label} = {FormatNumber(raw.Value)}"
                    End If
                End If

                Call rows.Add((label, item.value, cum, cum + item.value))
                cum += item.value
            Next

            If group.OthersCount > 0 Then
                Call rows.Add(($"其余 {group.OthersCount} 个特征", group.Others, cum, cum + group.Others))
                cum += group.Others
            End If

            Dim finalValue As Double = cum
            Dim m As Integer = rows.Count

            Dim all As New List(Of Double) From {group.Baseline, finalValue}

            For Each r In rows
                Call all.Add(r.startValue)
                Call all.Add(r.endValue)
            Next

            Dim range = ShapPlotUtils.PadRange(all.Min(), all.Max(), 0.08)
            Dim xmin As Double = range.min
            Dim xmax As Double = range.max

            If String.IsNullOrEmpty(XLabel) Then
                XLabel = "model output"
            End If

            Dim yLabels As String() = New String(m + 1) {}

            yLabels(0) = $"E[f(X)] = {FormatNumber(group.Baseline)}"

            For i As Integer = 0 To m - 1
                yLabels(i + 1) = rows(i).label
            Next

            yLabels(m + 1) = $"f(X) = {FormatNumber(finalValue)}"

            ' 纵轴刻度由本方法手工绘制（引擎的刻度标签做了右对齐假设，长标签会被裁掉）
            Call DrawAxisAndGrid(xmin, xmax, -0.5, m + 1.5,
                                 GenerateTicks(xmin, xmax), New Double() {}, Nothing, Nothing)

            DrawRowLabels(yLabels, m)

            ' 基准值与输出位置的参考虚线
            Using pen As New Pen(Theme.AxisColor, 0.8F)
                pen.DashStyle = DashStyle.Dash
                g.DrawLine(pen, ToPixelX(group.Baseline, xmin, xmax), _plotArea.Top,
                           ToPixelX(group.Baseline, xmin, xmax), _plotArea.Bottom)
                g.DrawLine(pen, ToPixelX(finalValue, xmin, xmax), _plotArea.Top,
                           ToPixelX(finalValue, xmin, xmax), _plotArea.Bottom)
            End Using

            For i As Integer = 0 To m - 1
                Dim row = rows(i)
                Dim yc As Double = i + 1
                Dim yTop As Single = ToPixelY(yc + 0.32, -0.5, m + 1.5)
                Dim yBottom As Single = ToPixelY(yc - 0.32, -0.5, m + 1.5)
                Dim px0 As Single = ToPixelX(row.startValue, xmin, xmax)
                Dim px1 As Single = ToPixelX(row.endValue, xmin, xmax)
                Dim left As Single = std.Min(px0, px1)
                Dim width As Single = std.Max(std.Abs(px1 - px0), 1.0F)
                Dim color As Color = If(row.value >= 0, ShapPlotUtils.PositiveColor, ShapPlotUtils.NegativeColor)

                Using br As New SolidBrush(Color.FromArgb(220, color))
                    g.FillRectangle(br, left, yTop, width, yBottom - yTop)
                End Using

                Using br As New SolidBrush(Theme.TextColor)
                    Dim text As String = If(row.value >= 0, "+", "") & FormatNumber(row.value)
                    Dim x As Single = std.Max(px0, px1) + 5

                    Call DrawLabel(text, Theme.AnnotationFont, br, x,
                                   yTop + (yBottom - yTop - Theme.AnnotationFont.GetHeight(g)) / 2)
                End Using

                ' 与下一根柱子之间的阶梯连接线
                If i < m - 1 Then
                    Using pen As New Pen(Color.FromArgb(160, Theme.AxisColor), 0.8F)
                        pen.DashStyle = DashStyle.Dash
                        g.DrawLine(pen, px1, yBottom, px1, ToPixelY(yc + 1 + 0.32, -0.5, m + 1.5))
                    End Using
                End If
            Next
        End Sub

        ''' <summary>
        ''' 逐行绘制右对齐的标签（基准值 / 特征名 = 取值 / 模型输出）。
        ''' </summary>
        ''' <param name="labels">标签文本</param>
        ''' <param name="m">最后一根柱子的行下标</param>
        Private Sub DrawRowLabels(labels As String(), m As Integer)
            Using br As New SolidBrush(Theme.TextColor)
                For i As Integer = 0 To labels.Length - 1
                    Dim text As String = labels(i)

                    If String.IsNullOrEmpty(text) Then
                        Continue For
                    End If

                    Dim py As Single = ToPixelY(i, -0.5, m + 1.5)
                    Dim w As Single = _g.MeasureString(text, Theme.AnnotationFont).Width
                    Dim h As Single = Theme.AnnotationFont.GetHeight(_g)

                    _g.DrawString(text, Theme.AnnotationFont, br,
                                  _plotArea.Left - w - 10, py - h / 2)
                Next
            End Using
        End Sub

        ''' <summary>
        ''' 按特征名查找该样本的原始特征取值；找不到的时候返回 Nothing。
        ''' </summary>
        Private Function FeatureValue(sampleIndex As Integer, featureName As String) As Double?
            If Shap.FeatureNames Is Nothing OrElse Features Is Nothing Then
                Return Nothing
            End If

            For i As Integer = 0 To Shap.FeatureNames.Length - 1
                If String.Equals(Shap.FeatureNames(i), featureName, StringComparison.OrdinalIgnoreCase) Then
                    Dim row As Double() = Features.features(sampleIndex)

                    If row IsNot Nothing AndAlso i < row.Length Then
                        Return row(i)
                    End If

                    Return Nothing
                End If
            Next

            Return Nothing
        End Function

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
