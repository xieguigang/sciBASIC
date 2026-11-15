#Region "Microsoft.VisualBasic::a5d3c9e17b0f42d6e8c1b7a45f923d08, Data_science\Visualization\Visualization\Multivariate\ScreeVariancePlot.vb"

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

    ' Class ScreeVariancePlot
    ' 
    '     Properties: BarColor, Components, CumulativeThreshold, LineColor
    '                 ShowCumulative, ShowValueLabels
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
''' 碎石图（Scree plot）/ 方差解释图：每个成分的贡献率柱状图 + 累计贡献率折线。
''' 
''' <para>
''' 直接喂 PCA 的分析结果即可（<c>Contributions</c> 本身就是百分数），
''' 也可以手工给出 <see cref="MultivariateVarianceComponent"/> 数组。
''' PLS/OPLS 的每个成分方差解释率由 <c>t p'</c> 从 X 中逐个扣除后估算得到。
''' </para>
''' </summary>
Public Class ScreeVariancePlot : Inherits PlotEngine

    ''' <summary>要绘制的成分方差解释率列表</summary>
    Public Property Components As MultivariateVarianceComponent() = {}

    ''' <summary>是否叠加累计贡献率折线</summary>
    Public Property ShowCumulative As Boolean = True

    ''' <summary>可选的阈值参考线（例如 80%），Nothing 时不绘制</summary>
    Public Property CumulativeThreshold As Double? = Nothing

    ''' <summary>是否在柱子顶部标注数值</summary>
    Public Property ShowValueLabels As Boolean = True

    ''' <summary>柱子的填充色，Nothing 时取主题调色板的第一个颜色</summary>
    Public Property BarColor As Color? = Nothing

    ''' <summary>累计折线的颜色，Nothing 时取主题调色板的第四个颜色</summary>
    Public Property LineColor As Color? = Nothing

    ''' <summary>数值标注使用的字体，Nothing 时使用主题的刻度字体</summary>
    Public Property LabelFont As Font = Nothing

    Public Sub New(width As Integer, height As Integer,
                   Optional theme As PlotTheme = Nothing,
                   Optional driver As Drivers = Drivers.Default)
        MyBase.New(width, height, theme, driver)
    End Sub

    Public Sub New(width As Integer, height As Integer, components As MultivariateVarianceComponent(),
                   Optional theme As PlotTheme = Nothing,
                   Optional driver As Drivers = Drivers.Default)
        MyBase.New(width, height, theme, driver)

        Me.Components = components
    End Sub

    ''' <summary>
    ''' 直接从多元分析结果构造碎石图。
    ''' </summary>
    Public Sub New(width As Integer, height As Integer, result As MultivariateAnalysisResult,
                   Optional theme As PlotTheme = Nothing,
                   Optional driver As Drivers = Drivers.Default)
        Call Me.New(width, height, result.ToVarianceComponents(), theme, driver)
    End Sub

    ''' <summary>设置数据并立即渲染（使用引擎自身的画布）。</summary>
    Public Overloads Sub Plot(components As MultivariateVarianceComponent())
        Me.Components = components
        Call MyBase.Plot()
    End Sub

    Protected Overrides Sub PlotInternal(ByRef g As IGraphics, canvas As GraphicsRegion)
        Call DrawBackground()
        Call ComputePlotArea()
        Call DrawPlotArea()
        Call DrawTitle()

        Dim items As MultivariateVarianceComponent() = Me.Components

        If items Is Nothing OrElse items.Length = 0 Then
            Call DrawEmptyState("no variance component data")

            Return
        End If

        Dim n As Integer = items.Length

        ' ---- 坐标范围：x 轴是分类轴，预留半个柱宽 ----
        Dim xmin As Double = -0.6
        Dim xmax As Double = n - 0.4

        Dim ymax As Double = 105

        For Each item As MultivariateVarianceComponent In items
            ymax = std.Max(ymax, std.Max(item.Value, item.Cumulative) * 1.08)
        Next

        If CumulativeThreshold.HasValue Then
            ymax = std.Max(ymax, CumulativeThreshold.Value * 1.05)
        End If

        Dim ymin As Double = 0

        If String.IsNullOrEmpty(XLabel) Then XLabel = "Component"
        If String.IsNullOrEmpty(YLabel) Then YLabel = "Explained variance (%)"

        Dim ticks As Double() = Enumerable.Range(0, n).Select(Function(i) CDbl(i)).ToArray
        Dim labels As String() = items.Select(Function(item) item.Name).ToArray

        Call DrawAxisAndGrid(xmin, xmax, ymin, ymax, ticks, Nothing, labels, Nothing)

        ' 局部变量与同名属性（VB 不区分大小写）必须用 Me. 区分
        Dim barColor As Color = If(Me.BarColor, Theme.Palette(0))
        Dim lineColor As Color = If(Me.LineColor, Theme.Palette(If(Theme.Palette.Length > 3, 3, 0)))
        Dim font As Font = If(Me.LabelFont, Theme.TickLabelFont)
        Dim unit As Single = CSng(_plotArea.Width / (xmax - xmin))
        Dim barWidth As Single = std.Max(4.0F, unit * 0.55F)
        Dim zeroY As Single = ToPixelY(0, ymin, ymax)

        ' ---- 柱状图 ----
        Using brush As New SolidBrush(Color.FromArgb(210, barColor))
            For i As Integer = 0 To n - 1
                Dim cx As Single = ToPixelX(i, xmin, xmax)
                Dim top As Single = ToPixelY(MultivariatePlotHelper.Finite(items(i).Value), ymin, ymax)
                Dim height As Single = zeroY - top

                If height > 0 Then
                    g.FillRectangle(brush, cx - barWidth / 2, top, barWidth, height)
                End If
            Next
        End Using

        ' ---- 累计贡献率折线 ----
        If ShowCumulative Then
            Dim points As PointF() = New PointF(n - 1) {}

            For i As Integer = 0 To n - 1
                points(i) = New PointF(ToPixelX(i, xmin, xmax),
                                       ToPixelY(MultivariatePlotHelper.Finite(items(i).Cumulative), ymin, ymax))
            Next

            Using pen As New Pen(lineColor, Theme.LineWidth + 0.5F)
                Call g.DrawLines(pen, points)
            End Using

            For Each p As PointF In points
                Call DrawMarker(p.X, p.Y, MarkerShape.Circle, Theme.MarkerSize * 0.85F, lineColor)
            Next
        End If

        ' ---- 数值标注 ----
        If ShowValueLabels Then
            Using brush As New SolidBrush(Theme.TextColor)
                For i As Integer = 0 To n - 1
                    Dim cx As Single = ToPixelX(i, xmin, xmax)
                    Dim top As Single = ToPixelY(MultivariatePlotHelper.Finite(items(i).Value), ymin, ymax)
                    Dim text As String = items(i).Value.ToString("F1")
                    Dim size As SizeF = MeasureString(text, font)

                    Call DrawLabel(text, font, brush, cx - size.Width / 2, top - size.Height - 2, Nothing, Nothing)
                Next
            End Using
        End If

        ' ---- 阈值参考线 ----
        If CumulativeThreshold.HasValue Then
            Dim y As Single = ToPixelY(CumulativeThreshold.Value, ymin, ymax)

            Using pen As New Pen(Color.FromArgb(170, Theme.AxisColor), 0.9F)
                pen.DashStyle = DashStyle.Dash

                g.DrawLine(pen, _plotArea.Left, y, _plotArea.Right, y)
            End Using

            Using brush As New SolidBrush(Theme.SubTitleColor)
                Call DrawLabel($"{CumulativeThreshold.Value.ToString("F0")}%", font, brush,
                               _plotArea.Right - 34, y - font.GetHeight(g) - 2, Nothing, Nothing)
            End Using
        End If

        ' ---- 图例 ----
        Dim legends As New List(Of Series) From {
            New Series With {.Name = "Component", .Color = barColor, .MarkerShape = MarkerShape.Square}
        }

        If ShowCumulative Then
            Call legends.Add(New Series With {.Name = "Cumulative", .Color = lineColor, .MarkerShape = MarkerShape.Circle})
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
