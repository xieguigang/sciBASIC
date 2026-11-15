#Region "Microsoft.VisualBasic::a20e7d48c5f91b36e4a8d2c70b5f8319, Data_science\Visualization\Visualization\Multivariate\MultivariateOutlierPlot.vb"

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

    ' Enum MultivariateOutlierMetric
    ' 
    '     DModX, HotellingT2
    ' 
    ' 
    ' Class MultivariateOutlierPlot
    ' 
    '     Properties: Data, LabelFont, Metric, PointSize, ShowSampleLabels
    ' 
    '     Sub: Plot
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Data.Plots
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Math.Statistics.Hypothesis.ANOVA
Imports Font = Microsoft.VisualBasic.Imaging.Font
Imports std = System.Math

''' <summary>
''' 模型诊断图所使用的统计量。
''' </summary>
Public Enum MultivariateOutlierMetric
    ''' <summary>Hotelling T2：样本在得分空间内偏离模型中心的程度</summary>
    HotellingT2
    ''' <summary>DModX：样本在残差空间内偏离模型的程度（SPE 的开方标准化）</summary>
    DModX
End Enum

''' <summary>
''' 样本离群诊断图（Hotelling T2 / DModX）。
''' 
''' <para>
''' 每个样本画成一根「棒棒糖」，超过 95% 控制限的样本用强调色标出并显示样本名。
''' 注意 T2 只反映得分空间的异常，残差空间（新出现的变量模式）要靠 DModX 才能发现，
''' 因此实务上这两张图要一起看。
''' </para>
''' </summary>
Public Class MultivariateOutlierPlot : Inherits PlotEngine

    ''' <summary>要绘制的诊断数据</summary>
    Public Property Data As MultivariateOutlierSeries = Nothing

    ''' <summary>选择绘制哪一种统计量</summary>
    Public Property Metric As MultivariateOutlierMetric = MultivariateOutlierMetric.HotellingT2

    ''' <summary>是否显示样本名字（会以 45° 旋转显示在 x 轴上）</summary>
    Public Property ShowSampleLabels As Boolean = True

    ''' <summary>标记尺寸，&lt;= 0 时使用主题的 <c>MarkerSize</c></summary>
    Public Property PointSize As Single = 0

    ''' <summary>标注文字字体</summary>
    Public Property LabelFont As Font = Nothing

    Public Sub New(width As Integer, height As Integer,
                   Optional theme As PlotTheme = Nothing,
                   Optional driver As Drivers = Drivers.Default)
        MyBase.New(width, height, theme, driver)
    End Sub

    Public Sub New(width As Integer, height As Integer, data As MultivariateOutlierSeries,
                   Optional theme As PlotTheme = Nothing,
                   Optional driver As Drivers = Drivers.Default)
        MyBase.New(width, height, theme, driver)

        Me.Data = data
    End Sub

    ''' <summary>直接从多元分析结果构造诊断图。</summary>
    Public Sub New(width As Integer, height As Integer, result As MultivariateAnalysisResult,
                   Optional metric As MultivariateOutlierMetric = MultivariateOutlierMetric.HotellingT2,
                   Optional groups As String() = Nothing,
                   Optional sampleNames As String() = Nothing,
                   Optional theme As PlotTheme = Nothing,
                   Optional driver As Drivers = Drivers.Default)
        Call Me.New(width, height, MultivariateOutlierSeries.FromResult(result, groups, sampleNames), theme, driver)

        Me.Metric = metric
    End Sub

    ''' <summary>设置数据并立即渲染（使用引擎自身的画布）。</summary>
    Public Overloads Sub Plot(data As MultivariateOutlierSeries)
        Me.Data = data
        Call MyBase.Plot()
    End Sub

    Protected Overrides Sub PlotInternal(ByRef g As IGraphics, canvas As GraphicsRegion)
        Call DrawBackground()
        Call ComputePlotArea()
        Call DrawPlotArea()
        Call DrawTitle()

        Dim data As MultivariateOutlierSeries = Me.Data

        If data Is Nothing OrElse data.Size = 0 Then
            Call DrawEmptyState("no outlier diagnostic data")

            Return
        End If

        Dim n As Integer = data.Size
        Dim values As Double() = If(Me.Metric = MultivariateOutlierMetric.DModX, data.DModX, data.HotellingT2)
        Dim threshold As Double = If(Me.Metric = MultivariateOutlierMetric.DModX, data.DModXThreshold, data.T2Threshold)
        Dim yTitle As String = If(Me.Metric = MultivariateOutlierMetric.DModX, "DModX", "Hotelling T2")

        ' ---- 坐标范围 ----
        Dim xmin As Double = -0.6
        Dim xmax As Double = n - 0.4
        Dim ymin As Double = 0
        Dim ymax As Double = 0

        For i As Integer = 0 To n - 1
            ymax = std.Max(ymax, MultivariatePlotHelper.Finite(values(i)))
        Next

        If Not Double.IsNaN(threshold) Then
            ymax = std.Max(ymax, threshold)
        End If

        If ymax <= 0 Then
            ymax = 1
        End If

        ymax *= 1.15

        If String.IsNullOrEmpty(XLabel) Then XLabel = "Sample"
        If String.IsNullOrEmpty(YLabel) Then YLabel = yTitle

        ' ---- 坐标轴 ----
        Dim ticks As Double() = Enumerable.Range(0, n).Select(Function(i) CDbl(i)).ToArray
        Dim labels As String() = Nothing

        If ShowSampleLabels AndAlso data.SampleNames IsNot Nothing AndAlso data.SampleNames.Length >= n Then
            labels = data.SampleNames.Take(n).ToArray
        End If

        Call DrawAxisAndGrid(xmin, xmax, ymin, ymax, ticks, Nothing, labels, Nothing, xLabelRotate:=labels IsNot Nothing)

        Dim normalColor As Color = If(Theme.Palette.Length > 0, Theme.Palette(0), Color.SteelBlue)
        Dim outlierColor As Color = If(Theme.Palette.Length > 3, Theme.Palette(3), Color.Firebrick)
        Dim markerSize As Single = If(PointSize > 0, PointSize, Theme.MarkerSize)
        Dim font As Font = If(Me.LabelFont, Theme.TickLabelFont)
        Dim zeroY As Single = ToPixelY(0, ymin, ymax)

        ' ---- 棒棒糖图 ----
        For i As Integer = 0 To n - 1
            Dim value As Double = MultivariatePlotHelper.Finite(values(i))
            Dim cx As Single = ToPixelX(i, xmin, xmax)
            Dim top As Single = ToPixelY(value, ymin, ymax)
            ' 只按当前绘制的这个统计量来判断是否超限：
            ' 否则一张 DModX 图上会把「仅 T2 超限」的样本也标成红色，容易误读
            Dim isOutlier As Boolean = Not Double.IsNaN(threshold) AndAlso value > threshold
            Dim color As Color = If(isOutlier, outlierColor, normalColor)

            Using pen As New Pen(Color.FromArgb(160, color), 1.0F)
                g.DrawLine(pen, cx, zeroY, cx, top)
            End Using

            Call DrawMarker(cx, top, If(isOutlier, MarkerShape.Diamond, MarkerShape.Circle), markerSize, color)

            If isOutlier AndAlso data.SampleNames IsNot Nothing AndAlso i < data.SampleNames.Length Then
                Dim size As SizeF = MeasureString(data.SampleNames(i), font)

                Using brush As New SolidBrush(Color.FromArgb(235, outlierColor))
                    Call DrawLabel(data.SampleNames(i), font, brush, cx - size.Width / 2, top - size.Height - 4, Nothing, Nothing)
                End Using
            End If
        Next

        ' ---- 控制限 ----
        If Not Double.IsNaN(threshold) AndAlso threshold < ymax Then
            Dim py As Single = ToPixelY(threshold, ymin, ymax)

            Using pen As New Pen(Color.FromArgb(190, outlierColor), 1.0F)
                pen.DashStyle = DashStyle.Dash

                g.DrawLine(pen, _plotArea.Left, py, _plotArea.Right, py)
            End Using

            Dim text As String = $"95% limit = {threshold.ToString("F2")}"
            Dim textSize As SizeF = MeasureString(text, font)

            Using brush As New SolidBrush(Theme.SubTitleColor)
                Call DrawLabel(text, font, brush,
                               _plotArea.Right - textSize.Width - 4, py - textSize.Height - 2, Nothing, Nothing)
            End Using
        End If

        ' ---- 图例 ----
        Dim legends As New List(Of Series) From {
            New Series With {.Name = "In control", .Color = normalColor, .MarkerShape = MarkerShape.Circle}
        }

        If Not Double.IsNaN(threshold) Then
            Call legends.Add(New Series With {.Name = "Outlier", .Color = outlierColor, .MarkerShape = MarkerShape.Diamond})
        End If

        Call DrawLegend(legends)
    End Sub

    Private Sub DrawEmptyState(message As String)
        If Not DrawFrame Then
            Return
        End If

        Dim font As Font = Theme.AnnotationFont
        Dim size As SizeF = MeasureString(message, font)
        Dim x As Single = _plotArea.Left + (_plotArea.Width - size.Width) / 2
        Dim y As Single = _plotArea.Top + (_plotArea.Height - size.Height) / 2

        Using brush As New SolidBrush(Theme.SubTitleColor)
            Call DrawLabel(message, font, brush, x, y, Nothing, Nothing)
        End Using
    End Sub

End Class
