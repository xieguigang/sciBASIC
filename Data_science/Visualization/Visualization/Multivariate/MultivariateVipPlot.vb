#Region "Microsoft.VisualBasic::c93e0a17b5d642f8e2a7c1d08b93f54e, Data_science\Visualization\Visualization\Multivariate\MultivariateVipPlot.vb"

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

    ' Class MultivariateVipPlot
    ' 
    '     Properties: BarColor, Entries, HighlightColor, LabelFont, MaxBars
    '                 ShowValueLabels, Threshold
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
''' VIP 图 / 回归系数图（横向条形图）。
''' 
''' <para>
''' 变量指标从大到小排列，默认阈值 1（VIP &gt; 1 视为重要变量），
''' 超过阈值的条用强调色绘制。把 <see cref="Threshold"/> 设为 Nothing 并且
''' 数据换成 <c>Coefficients</c>，就得到 OPLS-DA 的系数条形图。
''' </para>
''' </summary>
Public Class MultivariateVipPlot : Inherits PlotEngine

    ''' <summary>要绘制的条目（通常已经按数值降序排序）</summary>
    Public Property Entries As MultivariateVipEntry() = {}

    ''' <summary>重要性阈值（VIP 默认 1.0），Nothing 时不绘制阈值线</summary>
    Public Property Threshold As Double? = 1.0

    ''' <summary>最多绘制多少个变量（0 或负数表示全部）</summary>
    Public Property MaxBars As Integer = 40

    ''' <summary>条形填充色，Nothing 时取主题调色板第一个颜色</summary>
    Public Property BarColor As Color? = Nothing

    ''' <summary>超过阈值的重要性条形的颜色</summary>
    Public Property HighlightColor As Color? = Nothing

    ''' <summary>是否在条形末端标注数值</summary>
    Public Property ShowValueLabels As Boolean = True

    ''' <summary>数值标注字体</summary>
    Public Property LabelFont As Font = Nothing

    Public Sub New(width As Integer, height As Integer,
                   Optional theme As PlotTheme = Nothing,
                   Optional driver As Drivers = Drivers.Default)
        MyBase.New(width, height, theme, driver)
    End Sub

    Public Sub New(width As Integer, height As Integer, entries As MultivariateVipEntry(),
                   Optional theme As PlotTheme = Nothing,
                   Optional driver As Drivers = Drivers.Default)
        MyBase.New(width, height, theme, driver)

        Me.Entries = entries
    End Sub

    ''' <summary>
    ''' 直接从多元分析结果构造 VIP（或系数）条形图。
    ''' </summary>
    Public Sub New(width As Integer, height As Integer, result As MultivariateAnalysisResult,
                   Optional useCoefficients As Boolean = False,
                   Optional variableNames As String() = Nothing,
                   Optional theme As PlotTheme = Nothing,
                   Optional driver As Drivers = Drivers.Default)

        Call Me.New(width, height, result.ToVipEntries(useCoefficients, variableNames), theme, driver)

        If useCoefficients Then
            ' 系数的「重要性」是绝对值大小，阈值线没有意义
            Me.Threshold = Nothing
        End If
    End Sub

    ''' <summary>设置数据并立即渲染（使用引擎自身的画布）。</summary>
    Public Overloads Sub Plot(entries As MultivariateVipEntry())
        Me.Entries = entries
        Call MyBase.Plot()
    End Sub

    Protected Overrides Sub PlotInternal(ByRef g As IGraphics, canvas As GraphicsRegion)
        Call DrawBackground()
        Call ComputePlotArea()
        Call DrawPlotArea()
        Call DrawTitle()

        Dim items As MultivariateVipEntry() = Me.Entries

        If items Is Nothing OrElse items.Length = 0 Then
            Call DrawEmptyState("no VIP / coefficient data")

            Return
        End If

        If MaxBars > 0 AndAlso items.Length > MaxBars Then
            items = items.Take(MaxBars).ToArray
        End If

        Dim n As Integer = items.Length

        ' ---- 坐标范围：x 是数值（系数会有负值），y 是分类轴 ----
        Dim maxValue As Double = 0
        Dim minValue As Double = 0

        For Each item As MultivariateVipEntry In items
            Dim value As Double = MultivariatePlotHelper.Finite(item.Value)

            maxValue = std.Max(maxValue, value)
            minValue = std.Min(minValue, value)
        Next

        If Me.Threshold.HasValue Then
            maxValue = std.Max(maxValue, Me.Threshold.Value)
        End If

        If maxValue <= 0 AndAlso minValue >= 0 Then
            maxValue = 1
        End If

        Dim span As Double = std.Max(0.000001, maxValue - minValue)
        Dim xmax As Double = maxValue + span * 0.12
        Dim xmin As Double = If(minValue < 0, minValue - span * 0.12, 0)
        Dim ymin As Double = -0.6
        Dim ymax As Double = n - 0.4

        If String.IsNullOrEmpty(XLabel) Then
            XLabel = If(Me.Threshold.HasValue, "VIP", "Coefficient")
        End If
        If String.IsNullOrEmpty(YLabel) Then YLabel = ""

        ' y 轴自下而上排列，最大的条目显示在最上方
        Dim ticks As Double() = Enumerable.Range(0, n).Select(Function(i) CDbl(i)).ToArray
        Dim labels As String() = Enumerable.Range(0, n).Select(Function(i) items(n - 1 - i).Variable).ToArray

        Call DrawAxisAndGrid(xmin, xmax, ymin, ymax, Nothing, ticks, Nothing, labels)

        Dim barColor As Color = If(Me.BarColor, Theme.Palette(0))
        Dim highlightColor As Color = If(Me.HighlightColor, If(Theme.Palette.Length > 3, Theme.Palette(3), Color.Firebrick))
        Dim font As Font = If(Me.LabelFont, Theme.TickLabelFont)
        Dim unit As Single = CSng(_plotArea.Height / (ymax - ymin))
        Dim barHeight As Single = std.Max(3.0F, unit * 0.62F)
        Dim zeroX As Single = ToPixelX(0, xmin, xmax)

        For r As Integer = 0 To n - 1
            Dim item As MultivariateVipEntry = items(n - 1 - r)
            Dim value As Double = MultivariatePlotHelper.Finite(item.Value)
            Dim cy As Single = ToPixelY(r, ymin, ymax)
            Dim endX As Single = ToPixelX(value, xmin, xmax)
            Dim isHighlight As Boolean = Me.Threshold.HasValue AndAlso std.Abs(value) >= Me.Threshold.Value
            Dim color As Color = If(isHighlight, highlightColor, barColor)

            ' 统一从 0 出发：正值向右、负值向左
            Dim left As Single = std.Min(zeroX, endX)
            Dim width As Single = std.Abs(endX - zeroX)

            If width > 0 Then
                Using brush As New SolidBrush(Color.FromArgb(215, color))
                    g.FillRectangle(brush, left, cy - barHeight / 2, width, barHeight)
                End Using
            End If

            If ShowValueLabels Then
                Dim text As String = value.ToString("F2")
                Dim size As SizeF = MeasureString(text, font)
                Dim labelX As Single = If(value >= 0, endX + 4, endX - size.Width - 4)

                Using brush As New SolidBrush(Theme.TextColor)
                    Call DrawLabel(text, font, brush, labelX, cy - size.Height / 2, Nothing, Nothing)
                End Using
            End If
        Next

        ' ---- 阈值线 ----
        If Me.Threshold.HasValue AndAlso Me.Threshold.Value > xmin AndAlso Me.Threshold.Value < xmax Then
            Dim px As Single = ToPixelX(Me.Threshold.Value, xmin, xmax)

            Using pen As New Pen(Color.FromArgb(180, Theme.AxisColor), 0.9F)
                pen.DashStyle = DashStyle.Dash

                g.DrawLine(pen, px, _plotArea.Top, px, _plotArea.Bottom)
            End Using

            Using brush As New SolidBrush(Theme.SubTitleColor)
                Call DrawLabel(Me.Threshold.Value.ToString("0.##"), font, brush, px + 3, _plotArea.Top + 2, Nothing, Nothing)
            End Using
        End If

        ' ---- 图例 ----
        Dim legends As New List(Of Series) From {
            New Series With {.Name = If(Me.Threshold.HasValue, "Variable", "Coefficient"), .Color = barColor, .MarkerShape = MarkerShape.Square}
        }

        If Me.Threshold.HasValue Then
            Call legends.Add(New Series With {
                .Name = $"≥ {Me.Threshold.Value.ToString("0.##")}",
                .Color = highlightColor,
                .MarkerShape = MarkerShape.Square
            })
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
