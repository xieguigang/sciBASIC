#Region "Microsoft.VisualBasic::b6a05d92f7c14e83a2d9c7b5e04f1683, Data_science\Visualization\Visualization\Multivariate\MultivariateSPlot.vb"

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

    ' Class MultivariateSPlot
    ' 
    '     Properties: HighlightCorrelation, LabelFont, MaxLabels, PointSize
    '                 Points
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
''' OPLS-DA 的 S-plot：<c>p(1)</c>（协方差）对 <c>p(corr)</c>（相关性）。
''' 
''' <para>
''' 位于 S 形两端的变量即为对分组判别贡献最大、且可靠性最高的标志物：
''' x 方向代表贡献强度（协方差），y 方向代表可靠性（与得分的相关性）。
''' </para>
''' </summary>
Public Class MultivariateSPlot : Inherits PlotEngine

    ''' <summary>要绘制的 S-plot 数据点</summary>
    Public Property Points As MultivariateSPlotPoint() = {}

    ''' <summary>|p(corr)| 的显著性阈值，超过该值的点会被强调并标注</summary>
    Public Property HighlightCorrelation As Double = 0.5

    ''' <summary>最多标注多少个变量名（按 |p(corr)| 降序取）</summary>
    Public Property MaxLabels As Integer = 12

    ''' <summary>点标记尺寸，&lt;= 0 时使用主题的 <c>MarkerSize</c></summary>
    Public Property PointSize As Single = 0

    ''' <summary>变量名标注字体</summary>
    Public Property LabelFont As Font = Nothing

    Public Sub New(width As Integer, height As Integer,
                   Optional theme As PlotTheme = Nothing,
                   Optional driver As Drivers = Drivers.Default)
        MyBase.New(width, height, theme, driver)
    End Sub

    Public Sub New(width As Integer, height As Integer, points As MultivariateSPlotPoint(),
                   Optional theme As PlotTheme = Nothing,
                   Optional driver As Drivers = Drivers.Default)
        MyBase.New(width, height, theme, driver)

        Me.Points = points
    End Sub

    ''' <summary>
    ''' 直接从多元分析结果（需要 OPLS 的 p1 / p(corr)）构造 S-plot。
    ''' </summary>
    Public Sub New(width As Integer, height As Integer, result As MultivariateAnalysisResult,
                   Optional variableNames As String() = Nothing,
                   Optional theme As PlotTheme = Nothing,
                   Optional driver As Drivers = Drivers.Default)
        Call Me.New(width, height, result.ToSPlot(variableNames), theme, driver)
    End Sub

    ''' <summary>设置数据并立即渲染（使用引擎自身的画布）。</summary>
    Public Overloads Sub Plot(points As MultivariateSPlotPoint())
        Me.Points = points
        Call MyBase.Plot()
    End Sub

    Protected Overrides Sub PlotInternal(ByRef g As IGraphics, canvas As GraphicsRegion)
        Call DrawBackground()
        Call ComputePlotArea()
        Call DrawPlotArea()
        Call DrawTitle()

        Dim items As MultivariateSPlotPoint() = Me.Points

        If items Is Nothing OrElse items.Length = 0 Then
            Call DrawEmptyState("no S-plot data (p1 / p(corr) is only available for OPLS-DA)")

            Return
        End If

        Dim xs As Double() = items.Select(Function(p) MultivariatePlotHelper.Finite(p.Covariance)).ToArray
        Dim ys As Double() = items.Select(Function(p) MultivariatePlotHelper.Finite(p.Correlation)).ToArray
        Dim xmin#, xmax#, ymin#, ymax#

        Call MultivariatePlotHelper.RangeOf(xs, xmin, xmax)
        Call MultivariatePlotHelper.RangeOf(ys, ymin, ymax)
        Call ExpandRange(xmin, xmax)
        Call ExpandRange(ymin, ymax)

        ' p(corr) 天然落在 [-1, 1]，把范围撑满更符合阅读习惯
        ymin = std.Min(ymin, -1.05)
        ymax = std.Max(ymax, 1.05)

        If Me.XMin.HasValue Then xmin = Me.XMin.Value
        If Me.XMax.HasValue Then xmax = Me.XMax.Value
        If Me.YMin.HasValue Then ymin = Me.YMin.Value
        If Me.YMax.HasValue Then ymax = Me.YMax.Value

        If String.IsNullOrEmpty(XLabel) Then XLabel = "p(1)"
        If String.IsNullOrEmpty(YLabel) Then YLabel = "p(corr)"

        Call DrawAxisAndGrid(xmin, xmax, ymin, ymax)
        Call DrawZeroLines(xmin, xmax, ymin, ymax)

        Dim font As Font = If(Me.LabelFont, Theme.AnnotationFont)
        Dim markerSize As Single = If(PointSize > 0, PointSize, Theme.MarkerSize)
        Dim highlight As Color = If(Theme.Palette.Length > 3, Theme.Palette(3), Color.Firebrick)
        Dim normal As Color = If(Theme.Palette.Length > 7, Theme.Palette(7), Color.Gray)

        ' 按 |p(corr)| 挑出需要标注的变量（逐个取最大值，避免依赖匿名元组的类型推断）
        Dim labelSet As New HashSet(Of Integer)
        Dim picked As New HashSet(Of Integer)

        For k As Integer = 1 To std.Min(std.Max(0, MaxLabels), items.Length)
            Dim best As Integer = -1
            Dim bestValue As Double = -1

            For i As Integer = 0 To items.Length - 1
                If picked.Contains(i) Then
                    Continue For
                End If

                Dim value As Double = std.Abs(ys(i))

                If value > bestValue Then
                    bestValue = value
                    best = i
                End If
            Next

            If best < 0 Then
                Exit For
            End If

            Call picked.Add(best)
            Call labelSet.Add(best)
        Next

        For i As Integer = 0 To items.Length - 1
            Dim px As Single = ToPixelX(xs(i), xmin, xmax)
            Dim py As Single = ToPixelY(ys(i), ymin, ymax)

            If Not InPlotArea(px, py) Then
                Continue For
            End If

            Dim isHighlight As Boolean = std.Abs(ys(i)) >= HighlightCorrelation
            Dim color As Color = If(isHighlight, highlight, normal)

            Call DrawMarker(px, py, If(isHighlight, MarkerShape.Diamond, MarkerShape.Circle), markerSize, color)

            If labelSet.Contains(i) Then
                Dim size As SizeF = MeasureString(items(i).Variable, font)

                Using brush As New SolidBrush(Color.FromArgb(If(isHighlight, 240, 165), color))
                    Call DrawLabel(items(i).Variable, font, brush, px + 4, py - size.Height / 2, Nothing, Nothing)
                End Using
            End If
        Next

        ' ---- 显著性水平参考线 ----
        If HighlightCorrelation > 0 AndAlso HighlightCorrelation < 1 Then
            Using pen As New Pen(Color.FromArgb(140, highlight), 0.8F)
                pen.DashStyle = DashStyle.Dot

                For Each level As Double In {-HighlightCorrelation, HighlightCorrelation}
                    If level <= ymin OrElse level >= ymax Then
                        Continue For
                    End If

                    Dim py As Single = ToPixelY(level, ymin, ymax)

                    g.DrawLine(pen, _plotArea.Left, py, _plotArea.Right, py)
                Next
            End Using
        End If

        Dim legends As New List(Of Series) From {
            New Series With {.Name = $"|p(corr)| ≥ {HighlightCorrelation.ToString("0.##")}", .Color = highlight, .MarkerShape = MarkerShape.Diamond},
            New Series With {.Name = "Variable", .Color = normal, .MarkerShape = MarkerShape.Circle}
        }

        Call DrawLegend(legends)
    End Sub

    Private Sub DrawZeroLines(xmin As Double, xmax As Double, ymin As Double, ymax As Double)
        Using pen As New Pen(Color.FromArgb(150, Theme.AxisColor), 0.8F)
            pen.DashStyle = DashStyle.Dash

            If xmin < 0 AndAlso xmax > 0 Then
                Dim px As Single = ToPixelX(0, xmin, xmax)

                _g.DrawLine(pen, px, _plotArea.Top, px, _plotArea.Bottom)
            End If

            If ymin < 0 AndAlso ymax > 0 Then
                Dim py As Single = ToPixelY(0, ymin, ymax)

                _g.DrawLine(pen, _plotArea.Left, py, _plotArea.Right, py)
            End If
        End Using
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

    Private Function InPlotArea(x As Single, y As Single) As Boolean
        Return Not Single.IsNaN(x) AndAlso Not Single.IsNaN(y) AndAlso
            x >= _plotArea.Left AndAlso x <= _plotArea.Right AndAlso
            y >= _plotArea.Top AndAlso y <= _plotArea.Bottom
    End Function

    Private Shared Sub ExpandRange(ByRef min As Double, ByRef max As Double)
        If std.Abs(max - min) < 0.0000000001 Then
            min -= 1
            max += 1

            Return
        End If

        Dim pad As Double = (max - min) * 0.08

        min -= pad
        max += pad
    End Sub

End Class
