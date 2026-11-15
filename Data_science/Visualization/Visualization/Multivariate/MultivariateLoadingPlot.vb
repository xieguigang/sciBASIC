#Region "Microsoft.VisualBasic::d18f4b60a2c97e35f6b0d3a17c48e952, Data_science\Visualization\Visualization\Multivariate\MultivariateLoadingPlot.vb"

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

    ' Class MultivariateLoadingPlot
    ' 
    '     Properties: BiplotScale, HighlightDistance, LabelFont, Loadings
    '                 ShowAllLabels, ShowBiplot, ShowVariableLabels
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
''' 载荷图（Loading plot）与双标图（Biplot）。
''' 
''' <para>
''' 默认绘制两个成分的载荷散点并标注变量名，离原点较远的变量（对模型贡献大）用强调色画出；
''' 当 <see cref="ShowBiplot"/> 为 True 且 <see cref="MultivariateLoadingSet.Scores"/> 不为空时，
''' 会把得分点按一个缩放因子叠加到同一个坐标系里形成双标图。
''' </para>
''' 
''' <para>
''' 双标图不使用第二个 y 轴：而是把得分整体缩放（<c>0.85 × max|loading| / max|score|</c>），
''' 缩放系数会打印在图例脚注中。这样两种点共用同一套坐标轴，读数不会产生歧义。
''' </para>
''' </summary>
Public Class MultivariateLoadingPlot : Inherits PlotEngine

    ''' <summary>要绘制的载荷数据（可选携带得分数据用于双标图）</summary>
    Public Property Loadings As MultivariateLoadingSet = Nothing

    ''' <summary>是否把得分点叠加成双标图</summary>
    Public Property ShowBiplot As Boolean = False

    ''' <summary>手工指定双标图的得分缩放因子，Nothing 时自动计算</summary>
    Public Property BiplotScale As Double? = Nothing

    ''' <summary>是否标注所有变量的名称（False 时只标注高载荷变量）</summary>
    Public Property ShowVariableLabels As Boolean = True

    ''' <summary>
    ''' 高载荷判定阈值（到原点的距离），Nothing 时取最大距离的 55%。
    ''' </summary>
    Public Property HighlightDistance As Double? = Nothing

    ''' <summary>载荷点标记尺寸，&lt;= 0 时使用主题的 <c>MarkerSize</c></summary>
    Public Property PointSize As Single = 0

    ''' <summary>变量名标注字体</summary>
    Public Property LabelFont As Font = Nothing

    Public Sub New(width As Integer, height As Integer,
                   Optional theme As PlotTheme = Nothing,
                   Optional driver As Drivers = Drivers.Default)
        MyBase.New(width, height, theme, driver)
    End Sub

    Public Sub New(width As Integer, height As Integer, data As MultivariateLoadingSet,
                   Optional theme As PlotTheme = Nothing,
                   Optional driver As Drivers = Drivers.Default)
        MyBase.New(width, height, theme, driver)

        Me.Loadings = data
    End Sub

    ''' <summary>
    ''' 直接从多元分析结果构造载荷图。
    ''' </summary>
    Public Sub New(width As Integer, height As Integer, result As MultivariateAnalysisResult,
                   Optional xComponent As Integer = 0,
                   Optional yComponent As Integer = 1,
                   Optional variableNames As String() = Nothing,
                   Optional scores As MultivariateScoreSet = Nothing,
                   Optional theme As PlotTheme = Nothing,
                   Optional driver As Drivers = Drivers.Default)

        Call Me.New(width, height,
                    MultivariateLoadingSet.FromResult(result, xComponent, yComponent, variableNames, scores),
                    theme, driver)
    End Sub

    ''' <summary>设置数据并立即渲染（使用引擎自身的画布）。</summary>
    Public Overloads Sub Plot(data As MultivariateLoadingSet)
        Me.Loadings = data
        Call MyBase.Plot()
    End Sub

    Protected Overrides Sub PlotInternal(ByRef g As IGraphics, canvas As GraphicsRegion)
        Call DrawBackground()
        Call ComputePlotArea()
        Call DrawPlotArea()
        Call DrawTitle()

        Dim data As MultivariateLoadingSet = Me.Loadings

        If data Is Nothing OrElse data.Size = 0 Then
            Call DrawEmptyState("no loading data")

            Return
        End If

        Dim n As Integer = data.Size

        ' ---- 双标图的载荷放大系数 ----
        ' 载荷的数值通常只有得分的一个零头，直接画在同一坐标系里会全部挤在原点附近，
        ' 因此把**载荷**放大到得分的量级（而不是把得分缩小到原点）
        Dim scores As MultivariateScoreSet = Nothing
        Dim loadingScale As Double = 1

        If ShowBiplot AndAlso data.Scores IsNot Nothing AndAlso data.Scores.Size > 0 Then
            scores = data.Scores
            loadingScale = If(Me.BiplotScale.HasValue, Me.BiplotScale.Value, AutoBiplotScale(data, scores))
        End If

        ' ---- 坐标范围（缩放后的载荷 + 得分）----
        Dim xmin#, xmax#, ymin#, ymax#

        Call MultivariatePlotHelper.RangeOf(Scale(data.X, loadingScale), xmin, xmax)
        Call MultivariatePlotHelper.RangeOf(Scale(data.Y, loadingScale), ymin, ymax)

        If scores IsNot Nothing Then
            Dim sx#, sxo#, sy#, syo#

            Call MultivariatePlotHelper.RangeOf(scores.X.Take(scores.Size).ToArray, sx, sxo)
            Call MultivariatePlotHelper.RangeOf(scores.Y.Take(scores.Size).ToArray, sy, syo)

            xmin = std.Min(xmin, sx)
            xmax = std.Max(xmax, sxo)
            ymin = std.Min(ymin, sy)
            ymax = std.Max(ymax, syo)
        End If

        Call ExpandRange(xmin, xmax)
        Call ExpandRange(ymin, ymax)

        If Me.XMin.HasValue Then xmin = Me.XMin.Value
        If Me.XMax.HasValue Then xmax = Me.XMax.Value
        If Me.YMin.HasValue Then ymin = Me.YMin.Value
        If Me.YMax.HasValue Then ymax = Me.YMax.Value

        If String.IsNullOrEmpty(XLabel) Then XLabel = data.XTitle
        If String.IsNullOrEmpty(YLabel) Then YLabel = data.YTitle

        Call DrawAxisAndGrid(xmin, xmax, ymin, ymax)
        Call DrawZeroLines(xmin, xmax, ymin, ymax)

        Dim font As Font = If(Me.LabelFont, Theme.AnnotationFont)
        Dim markerSize As Single = If(PointSize > 0, PointSize, Theme.MarkerSize)

        ' ---- 双标图：先画得分点（下层）----
        If scores IsNot Nothing Then
            Dim groups As String() = scores.Groups

            If groups Is Nothing OrElse groups.Length <> scores.Size Then
                groups = Enumerable.Repeat("All", scores.Size).ToArray
            End If

            Dim colors As Dictionary(Of String, Color) = MultivariatePlotHelper.GroupColors(groups, Theme.Palette)

            For i As Integer = 0 To scores.Size - 1
                Dim px As Single = ToPixelX(scores.X(i), xmin, xmax)
                Dim py As Single = ToPixelY(scores.Y(i), ymin, ymax)

                If Not InPlotArea(px, py) Then
                    Continue For
                End If

                Call DrawMarker(px, py, MarkerShape.Circle, markerSize * 0.8F,
                                Color.FromArgb(120, colors(groups(i))))
            Next
        End If

        ' ---- 载荷：从原点射出的向量 + 点 + 标签 ----
        Dim maxDistance As Double = 0

        For i As Integer = 0 To n - 1
            maxDistance = std.Max(maxDistance, std.Sqrt(data.X(i) ^ 2 + data.Y(i) ^ 2))
        Next

        Dim threshold As Double = If(Me.HighlightDistance.HasValue, Me.HighlightDistance.Value, maxDistance * 0.55)
        Dim highlight As Color = If(Theme.Palette.Length > 3, Theme.Palette(3), Color.Firebrick)
        Dim normal As Color = If(Theme.Palette.Length > 0, Theme.Palette(0), Color.SteelBlue)
        Dim origin As New PointF(ToPixelX(0, xmin, xmax), ToPixelY(0, ymin, ymax))
        Dim occupied As New List(Of RectangleF)

        For i As Integer = 0 To n - 1
            Dim px As Single = ToPixelX(data.X(i) * loadingScale, xmin, xmax)
            Dim py As Single = ToPixelY(data.Y(i) * loadingScale, ymin, ymax)
            Dim distance As Double = std.Sqrt(data.X(i) ^ 2 + data.Y(i) ^ 2)
            Dim isHighlight As Boolean = distance >= threshold
            Dim color As Color = If(isHighlight, highlight, normal)

            If InPlotArea(px, py) Then
                Using pen As New Pen(Color.FromArgb(90, color), 0.8F)
                    g.DrawLine(pen, origin, New PointF(px, py))
                End Using

                Call DrawMarker(px, py, If(isHighlight, MarkerShape.Diamond, MarkerShape.Circle), markerSize, color)
            End If

            If ShowVariableLabels AndAlso (isHighlight OrElse maxDistance <= 0 OrElse distance >= maxDistance * 0.25) Then
                Dim label As String = If(data.VariableNames IsNot Nothing AndAlso i < data.VariableNames.Length,
                                         data.VariableNames(i), "X" & (i + 1).ToString)

                Call DrawVariableLabel(label, font, color, isHighlight, px, py, occupied)
            End If
        Next

        ' ---- 图例 ----
        Dim legends As New List(Of Series) From {
            New Series With {.Name = "Variable (high loading)", .Color = highlight, .MarkerShape = MarkerShape.Diamond},
            New Series With {.Name = "Variable", .Color = normal, .MarkerShape = MarkerShape.Circle}
        }

        If scores IsNot Nothing Then
            Call legends.Insert(0, New Series With {
                .Name = $"Score (loading ×{loadingScale.ToString("0.##")})",
                .Color = Color.Gray,
                .MarkerShape = MarkerShape.Circle
            })
        End If

        Call DrawLegend(legends)
    End Sub

    ''' <summary>
    ''' 绘制变量名，并做一次简单的竖直避让：与已经画出的标签重叠时向下平移。
    ''' </summary>
    Private Sub DrawVariableLabel(label As String, font As Font, color As Color, isHighlight As Boolean,
                                  px As Single, py As Single, occupied As List(Of RectangleF))
        Dim size As SizeF = MeasureString(label, font)
        Dim location As PointF = MultivariatePlotHelper.PlaceLabel(
            px + 4, py - size.Height / 2, size, _plotArea, occupied)

        Using brush As New SolidBrush(Color.FromArgb(If(isHighlight, 240, 170), color))
            Call DrawLabel(label, font, brush, location.X, location.Y, Nothing, Nothing)
        End Using
    End Sub

    ''' <summary>
    ''' 自动双标图缩放：把载荷放大到得分 85% 的半径范围（载荷通常比得分小一到两个数量级）。
    ''' </summary>
    Private Shared Function AutoBiplotScale(loadings As MultivariateLoadingSet, scores As MultivariateScoreSet) As Double
        Dim maxLoading As Double = 0
        Dim maxScore As Double = 0

        For i As Integer = 0 To loadings.Size - 1
            maxLoading = std.Max(maxLoading, std.Sqrt(loadings.X(i) ^ 2 + loadings.Y(i) ^ 2))
        Next

        For i As Integer = 0 To scores.Size - 1
            maxScore = std.Max(maxScore, std.Sqrt(scores.X(i) ^ 2 + scores.Y(i) ^ 2))
        Next

        If maxScore <= 0 OrElse maxLoading <= 0 Then
            Return 1
        End If

        Return 0.85 * maxScore / maxLoading
    End Function

    Private Shared Function Scale(values As Double(), factor As Double) As Double()
        Dim scaled As Double() = New Double(values.Length - 1) {}

        For i As Integer = 0 To values.Length - 1
            scaled(i) = values(i) * factor
        Next

        Return scaled
    End Function

    Private Sub DrawZeroLines(xmin As Double, xmax As Double, ymin As Double, ymax As Double)
        Using pen As New Pen(Color.FromArgb(150, Theme.AxisColor), 0.8F)
            pen.DashStyle = DashStyle.Dash

            If xmin < 0 AndAlso xmax > 0 Then
                Dim px As Single = ToPixelX(0, xmin, xmax)

                g_DrawVertical(pen, px)
            End If

            If ymin < 0 AndAlso ymax > 0 Then
                Dim py As Single = ToPixelY(0, ymin, ymax)

                _g.DrawLine(pen, _plotArea.Left, py, _plotArea.Right, py)
            End If
        End Using
    End Sub

    Private Sub g_DrawVertical(pen As Pen, x As Single)
        _g.DrawLine(pen, x, _plotArea.Top, x, _plotArea.Bottom)
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
