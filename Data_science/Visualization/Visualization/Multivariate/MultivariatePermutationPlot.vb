#Region "Microsoft.VisualBasic::e0d94a2c8f17b365d5b2c9e04a7f6d18, Data_science\Visualization\Visualization\Multivariate\MultivariatePermutationPlot.vb"

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

    ' Class MultivariatePermutationSeries
    ' 
    '     Properties: Correlation, Count, OriginalQ2, OriginalR2Y, PValue, Q2
    '                 Q2Intercept, Q2PValue, Q2Slope, R2Y, R2YIntercept, R2YSlope
    ' 
    ' Class MultivariatePermutationPlot
    ' 
    '     Properties: Data, PointSize, ShowOriginalLines
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
''' 置换检验图专用的数据模型（与 <see cref="PermutationTestResult"/> 一一对应）。
''' </summary>
Public Class MultivariatePermutationSeries

    Public Property Correlation As Double() = {}
    Public Property R2Y As Double() = {}
    Public Property Q2 As Double() = {}

    Public Property R2YSlope As Double = Double.NaN
    Public Property R2YIntercept As Double = Double.NaN
    Public Property Q2Slope As Double = Double.NaN
    Public Property Q2Intercept As Double = Double.NaN

    Public Property OriginalR2Y As Double = Double.NaN
    Public Property OriginalQ2 As Double = Double.NaN

    Public Property PValue As Double = Double.NaN
    Public Property Q2PValue As Double = Double.NaN

    Public Property Count As Integer = 0

    ''' <summary>是否包含可用的 Q2 序列（PCA 结果没有交叉验证，Q2 会全部是 NaN）</summary>
    Public ReadOnly Property HasQ2 As Boolean
        Get
            Return Q2 IsNot Nothing AndAlso Q2.Any(Function(v) Not Double.IsNaN(v))
        End Get
    End Property

    ''' <summary>从置换检验结果构造绘图数据。</summary>
    Public Shared Function FromResult(result As PermutationTestResult) As MultivariatePermutationSeries
        If result Is Nothing Then
            Return New MultivariatePermutationSeries()
        End If

        Return New MultivariatePermutationSeries With {
            .Correlation = If(result.Correlation, New Double() {}),
            .R2Y = If(result.R2Y, New Double() {}),
            .Q2 = If(result.Q2, New Double() {}),
            .R2YSlope = result.R2YSlope,
            .R2YIntercept = result.R2YIntercept,
            .Q2Slope = result.Q2Slope,
            .Q2Intercept = result.Q2Intercept,
            .OriginalR2Y = result.OriginalR2Y,
            .OriginalQ2 = result.OriginalQ2,
            .PValue = result.PValue,
            .Q2PValue = result.Q2PValue,
            .Count = result.Count
        }
    End Function

End Class

''' <summary>
''' 置换检验图（permutation test plot）。
''' 
''' <para>
''' 横轴是每次置换之后 Y 与原始 Y 的相关系数，纵轴是该次置换模型的 R2Y / Q2；
''' 两条最小二乘回归线分别标注截距，右上角给出原始模型指标与单侧 p 值。
''' OPLS-DA 模型的理想结果是：R2Y 截距接近 0、Q2 截距为负，且原始模型指标远高于置换分布。
''' </para>
''' </summary>
Public Class MultivariatePermutationPlot : Inherits PlotEngine

    ''' <summary>要绘制的置换检验数据</summary>
    Public Property Data As MultivariatePermutationSeries = Nothing

    ''' <summary>是否用虚线标出原始（未置换）模型的指标水平</summary>
    Public Property ShowOriginalLines As Boolean = True

    ''' <summary>散点标记尺寸，&lt;= 0 时使用主题的 <c>MarkerSize</c></summary>
    Public Property PointSize As Single = 0

    ''' <summary>标注文字字体</summary>
    Public Property LabelFont As Font = Nothing

    Public Sub New(width As Integer, height As Integer,
                   Optional theme As PlotTheme = Nothing,
                   Optional driver As Drivers = Drivers.Default)
        MyBase.New(width, height, theme, driver)
    End Sub

    Public Sub New(width As Integer, height As Integer, data As MultivariatePermutationSeries,
                   Optional theme As PlotTheme = Nothing,
                   Optional driver As Drivers = Drivers.Default)
        MyBase.New(width, height, theme, driver)

        Me.Data = data
    End Sub

    ''' <summary>直接从置换检验结果构造置换检验图。</summary>
    Public Sub New(width As Integer, height As Integer, result As PermutationTestResult,
                   Optional theme As PlotTheme = Nothing,
                   Optional driver As Drivers = Drivers.Default)
        Call Me.New(width, height, MultivariatePermutationSeries.FromResult(result), theme, driver)
    End Sub

    ''' <summary>设置数据并立即渲染（使用引擎自身的画布）。</summary>
    Public Overloads Sub Plot(data As MultivariatePermutationSeries)
        Me.Data = data
        Call MyBase.Plot()
    End Sub

    Protected Overrides Sub PlotInternal(ByRef g As IGraphics, canvas As GraphicsRegion)
        Call DrawBackground()
        Call ComputePlotArea()
        Call DrawPlotArea()
        Call DrawTitle()

        Dim data As MultivariatePermutationSeries = Me.Data

        If data Is Nothing OrElse data.Correlation Is Nothing OrElse data.Correlation.Length = 0 Then
            Call DrawEmptyState("no permutation test data")

            Return
        End If

        Dim n As Integer = data.Correlation.Length
        Dim hasQ2 As Boolean = data.HasQ2

        ' ---- 坐标范围 ----
        Dim xmin#, xmax#, ymin#, ymax#

        Call MultivariatePlotHelper.RangeOf(data.Correlation, xmin, xmax)
        Call MultivariatePlotHelper.RangeOf(data.R2Y, ymin, ymax)

        If hasQ2 Then
            Dim qmin#, qmax#

            Call MultivariatePlotHelper.RangeOf(data.Q2, qmin, qmax)
            ymin = std.Min(ymin, qmin)
            ymax = std.Max(ymax, qmax)
        End If

        ymin = std.Min(ymin, 0)
        Call ExpandRange(xmin, xmax)
        Call ExpandRange(ymin, ymax)

        If Me.XMin.HasValue Then xmin = Me.XMin.Value
        If Me.XMax.HasValue Then xmax = Me.XMax.Value
        If Me.YMin.HasValue Then ymin = Me.YMin.Value
        If Me.YMax.HasValue Then ymax = Me.YMax.Value

        If String.IsNullOrEmpty(XLabel) Then XLabel = "Correlation coefficient (permuted Y vs. original Y)"
        If String.IsNullOrEmpty(YLabel) Then YLabel = If(hasQ2, "R2Y / Q2", "R2Y")

        Call DrawAxisAndGrid(xmin, xmax, ymin, ymax)

        Dim r2yColor As Color = If(Theme.Palette.Length > 0, Theme.Palette(0), Color.SteelBlue)
        Dim q2Color As Color = If(Theme.Palette.Length > 3, Theme.Palette(3), Color.Firebrick)
        Dim markerSize As Single = If(PointSize > 0, PointSize, Theme.MarkerSize * 0.85F)

        ' ---- 散点 ----
        For i As Integer = 0 To n - 1
            Dim cx As Single = ToPixelX(MultivariatePlotHelper.Finite(data.Correlation(i)), xmin, xmax)
            Dim cy As Single = ToPixelY(MultivariatePlotHelper.Finite(data.R2Y(i)), ymin, ymax)

            If InPlotArea(cx, cy) Then
                Call DrawMarker(cx, cy, MarkerShape.Circle, markerSize, Color.FromArgb(190, r2yColor))
            End If

            If hasQ2 Then
                Dim qy As Single = ToPixelY(MultivariatePlotHelper.Finite(data.Q2(i)), ymin, ymax)

                If InPlotArea(cx, qy) Then
                    Call DrawMarker(cx, qy, MarkerShape.Square, markerSize, Color.FromArgb(190, q2Color))
                End If
            End If
        Next

        ' ---- 回归线 ----
        If Not Double.IsNaN(data.R2YSlope) AndAlso Not Double.IsNaN(data.R2YIntercept) Then
            Using pen As New Pen(r2yColor, Theme.LineWidth + 0.4F)
                Call DrawAbline(data.R2YIntercept, data.R2YSlope, xmin, xmax, ymin, ymax, pen)
            End Using
        End If

        If hasQ2 AndAlso Not Double.IsNaN(data.Q2Slope) AndAlso Not Double.IsNaN(data.Q2Intercept) Then
            Using pen As New Pen(q2Color, Theme.LineWidth + 0.4F)
                pen.DashStyle = DashStyle.Dash

                Call DrawAbline(data.Q2Intercept, data.Q2Slope, xmin, xmax, ymin, ymax, pen)
            End Using
        End If

        ' ---- 原始模型指标参考线 ----
        If ShowOriginalLines Then
            Using pen As New Pen(Color.FromArgb(130, Theme.AxisColor), 0.8F)
                pen.DashStyle = DashStyle.Dot

                If Not Double.IsNaN(data.OriginalR2Y) AndAlso data.OriginalR2Y > ymin AndAlso data.OriginalR2Y < ymax Then
                    Dim py As Single = ToPixelY(data.OriginalR2Y, ymin, ymax)

                    g.DrawLine(pen, _plotArea.Left, py, _plotArea.Right, py)
                End If

                If hasQ2 AndAlso Not Double.IsNaN(data.OriginalQ2) AndAlso data.OriginalQ2 > ymin AndAlso data.OriginalQ2 < ymax Then
                    Dim py As Single = ToPixelY(data.OriginalQ2, ymin, ymax)

                    g.DrawLine(pen, _plotArea.Left, py, _plotArea.Right, py)
                End If
            End Using
        End If

        ' ---- 右上角标注 ----
        Call DrawSummary(data, ymin, ymax)

        ' ---- 图例 ----
        Dim legends As New List(Of Series) From {
            New Series With {.Name = "R2Y", .Color = r2yColor, .MarkerShape = MarkerShape.Circle}
        }

        If hasQ2 Then
            Call legends.Add(New Series With {.Name = "Q2", .Color = q2Color, .MarkerShape = MarkerShape.Square})
        End If

        Call DrawLegend(legends)
    End Sub

    Private Sub DrawSummary(data As MultivariatePermutationSeries, ymin As Double, ymax As Double)
        Dim lines As New List(Of String)

        Call lines.Add($"N = {data.Count}")
        Call lines.Add($"R2Y = {data.OriginalR2Y.ToString("F3")}   intercept = {FormatIntercept(data.R2YIntercept)}   p = {FormatP(data.PValue)}")

        If data.HasQ2 Then
            Call lines.Add($"Q2  = {data.OriginalQ2.ToString("F3")}   intercept = {FormatIntercept(data.Q2Intercept)}   p = {FormatP(data.Q2PValue)}")
        End If

        Dim font As Font = If(Me.LabelFont, Theme.AnnotationFont)
        Dim height As Single = font.GetHeight(_g) + 3
        Dim width As Single = 0

        For Each line As String In lines
            width = std.Max(width, MeasureString(line, font).Width)
        Next

        Dim x As Single = _plotArea.Right - width - 12
        Dim y As Single = _plotArea.Top + 8

        Using brush As New SolidBrush(Theme.TextColor)
            For i As Integer = 0 To lines.Count - 1
                Call DrawLabel(lines(i), font, brush, x, y + i * height,
                               Color.FromArgb(225, Theme.LegendBackgroundColor), Theme.LegendBorderColor)
            Next
        End Using
    End Sub

    Private Shared Function FormatIntercept(value As Double) As String
        If Double.IsNaN(value) Then
            Return "n/a"
        Else
            Return value.ToString("F3")
        End If
    End Function

    Private Shared Function FormatP(value As Double) As String
        If Double.IsNaN(value) Then
            Return "n/a"
        ElseIf value < 0.001 Then
            Return "<0.001"
        Else
            Return value.ToString("F3")
        End If
    End Function

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
