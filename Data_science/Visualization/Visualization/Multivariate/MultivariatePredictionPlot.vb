#Region "Microsoft.VisualBasic::f7b21c60e3a04d95b8c1e7a5d29f0436, Data_science\Visualization\Visualization\Multivariate\MultivariatePredictionPlot.vb"

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

    ' Class MultivariatePredictionPlot
    ' 
    '     Properties: Data, LabelFont, PointSize, ShowRegressionLine
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
''' 预测值对实测值散点图（PLS/OPLS 的模型拟合诊断）。
''' 
''' <para>
''' 图中的虚线是 <c>y = x</c> 理想线，点越贴近虚线说明预测越准确；
''' 右上角标注 R2 与 RMSEE。若响应变量是离散的类别编码（PLS-DA），
''' 点会自然排列成若干水平条带，此时每个条带就是一个类别。
''' </para>
''' </summary>
Public Class MultivariatePredictionPlot : Inherits PlotEngine

    ''' <summary>要绘制的预测数据</summary>
    Public Property Data As MultivariatePredictionSet = Nothing

    ''' <summary>是否绘制实测值对预测值的回归线（用于观察系统偏差）</summary>
    Public Property ShowRegressionLine As Boolean = False

    ''' <summary>散点标记尺寸，&lt;= 0 时使用主题的 <c>MarkerSize</c></summary>
    Public Property PointSize As Single = 0

    ''' <summary>标注文字字体</summary>
    Public Property LabelFont As Font = Nothing

    Public Sub New(width As Integer, height As Integer,
                   Optional theme As PlotTheme = Nothing,
                   Optional driver As Drivers = Drivers.Default)
        MyBase.New(width, height, theme, driver)
    End Sub

    Public Sub New(width As Integer, height As Integer, data As MultivariatePredictionSet,
                   Optional theme As PlotTheme = Nothing,
                   Optional driver As Drivers = Drivers.Default)
        MyBase.New(width, height, theme, driver)

        Me.Data = data
    End Sub

    ''' <summary>直接从多元分析结果构造预测-实测散点图。</summary>
    Public Sub New(width As Integer, height As Integer, result As MultivariateAnalysisResult,
                   Optional groups As String() = Nothing,
                   Optional sampleNames As String() = Nothing,
                   Optional theme As PlotTheme = Nothing,
                   Optional driver As Drivers = Drivers.Default)
        Call Me.New(width, height, MultivariatePredictionSet.FromResult(result, groups, sampleNames), theme, driver)
    End Sub

    ''' <summary>设置数据并立即渲染（使用引擎自身的画布）。</summary>
    Public Overloads Sub Plot(data As MultivariatePredictionSet)
        Me.Data = data
        Call MyBase.Plot()
    End Sub

    Protected Overrides Sub PlotInternal(ByRef g As IGraphics, canvas As GraphicsRegion)
        Call DrawBackground()
        Call ComputePlotArea()
        Call DrawPlotArea()
        Call DrawTitle()

        Dim data As MultivariatePredictionSet = Me.Data

        If data Is Nothing OrElse data.Size = 0 Then
            Call DrawEmptyState("no prediction data")

            Return
        End If

        Dim n As Integer = data.Size
        Dim xmin#, xmax#, ymin#, ymax#

        Call MultivariatePlotHelper.RangeOf(data.Measured.Take(n).ToArray, xmin, xmax)
        Call MultivariatePlotHelper.RangeOf(data.Predicted.Take(n).ToArray, ymin, ymax)

        ' 让横纵轴使用同一个范围，y = x 才会是真正的 45° 对角线
        Dim lower As Double = std.Min(xmin, ymin)
        Dim upper As Double = std.Max(xmax, ymax)

        Call ExpandRange(lower, upper)

        xmin = lower
        xmax = upper
        ymin = lower
        ymax = upper

        If Me.XMin.HasValue Then xmin = Me.XMin.Value
        If Me.XMax.HasValue Then xmax = Me.XMax.Value
        If Me.YMin.HasValue Then ymin = Me.YMin.Value
        If Me.YMax.HasValue Then ymax = Me.YMax.Value

        If String.IsNullOrEmpty(XLabel) Then XLabel = "Measured Y"
        If String.IsNullOrEmpty(YLabel) Then YLabel = "Predicted Y"

        Call DrawAxisAndGrid(xmin, xmax, ymin, ymax)

        ' ---- 理想线 y = x ----
        Using pen As New Pen(Color.FromArgb(160, Theme.AxisColor), 0.9F)
            pen.DashStyle = DashStyle.Dash

            Call DrawAbline(0, 1, xmin, xmax, ymin, ymax, pen)
        End Using

        ' ---- 回归线（可选）----
        If ShowRegressionLine AndAlso Not Double.IsNaN(data.Slope) AndAlso Not Double.IsNaN(data.Intercept) Then
            Using pen As New Pen(Color.FromArgb(200, If(Theme.Palette.Length > 3, Theme.Palette(3), Color.Firebrick)), 1.2F)
                Call DrawAbline(data.Intercept, data.Slope, xmin, xmax, ymin, ymax, pen)
            End Using
        End If

        ' ---- 分组散点 ----
        Dim groups As String() = data.Groups

        If groups Is Nothing OrElse groups.Length <> n Then
            groups = Enumerable.Repeat("All", n).ToArray
        End If

        Dim colors As Dictionary(Of String, Color) = MultivariatePlotHelper.GroupColors(groups, Theme.Palette)
        Dim names As String() = MultivariatePlotHelper.GroupNames(groups)
        Dim markerSize As Single = If(PointSize > 0, PointSize, Theme.MarkerSize)

        For i As Integer = 0 To n - 1
            Dim px As Single = ToPixelX(data.Measured(i), xmin, xmax)
            Dim py As Single = ToPixelY(data.Predicted(i), ymin, ymax)

            If Not InPlotArea(px, py) Then
                Continue For
            End If

            Call DrawMarker(px, py, MarkerShape.Circle, markerSize, colors(groups(i)))
        Next

        ' ---- 指标标注 ----
        Dim lines As New List(Of String) From {
            $"R2 = {data.R2.ToString("F3")}",
            $"RMSEE = {data.Rmsee.ToString("F3")}",
            $"slope = {data.Slope.ToString("F3")}"
        }
        Dim font As Font = If(Me.LabelFont, Theme.AnnotationFont)
        Dim lineHeight As Single = font.GetHeight(_g) + 3
        Dim width As Single = 0

        For Each line As String In lines
            width = std.Max(width, MeasureString(line, font).Width)
        Next

        ' 数据点沿 y=x 对角线分布，左上角是唯一的空白区域
        Dim x0 As Single = _plotArea.Left + 12
        Dim y0 As Single = _plotArea.Top + 8

        Using brush As New SolidBrush(Theme.TextColor)
            For i As Integer = 0 To lines.Count - 1
                Call DrawLabel(lines(i), font, brush, x0, y0 + i * lineHeight,
                               Color.FromArgb(225, Theme.LegendBackgroundColor), Theme.LegendBorderColor)
            Next
        End Using

        ' ---- 图例 ----
        Dim legends As New List(Of Series)

        For gi As Integer = 0 To names.Length - 1
            Call legends.Add(New Series With {
                .Name = names(gi),
                .Color = colors(names(gi)),
                .MarkerShape = MarkerShape.Circle
            })
        Next

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

        Dim pad As Double = (max - min) * 0.06

        min -= pad
        max += pad
    End Sub

End Class
