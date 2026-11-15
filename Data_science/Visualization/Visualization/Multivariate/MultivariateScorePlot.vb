#Region "Microsoft.VisualBasic::f3c81a75b6d2409e8c7f5a1d3b96e0c2, Data_science\Visualization\Visualization\Multivariate\MultivariateScorePlot.vb"

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

    ' Class MultivariateScorePlot
    ' 
    '     Properties: Ellipses, LabelFont, MarkerShapes, PointSize, Scores
    '                 ShowPointLabels, ShowZeroLines
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
''' 多元统计分析的得分图（Score plot）。
''' 
''' <para>
''' 支持三类结果：
''' </para>
''' 
''' <list type="bullet">
''' <item>PCA：默认 PC1 对 PC2；</item>
''' <item>PLS-DA：默认 T1 对 T2；</item>
''' <item>OPLS-DA：默认预测成分 t[1] 对第一个正交成分 t_o[1]。</item>
''' </list>
''' 
''' <para>
''' 同一个分组内的样本会共用一个颜色、一个标记形状以及一个置信椭圆；
''' 轴标题自动附带该成分的方差解释率。
''' </para>
''' </summary>
Public Class MultivariateScorePlot : Inherits PlotEngine

    ''' <summary>要绘制的得分数据</summary>
    Public Property Scores As MultivariateScoreSet = Nothing

    ''' <summary>分组椭圆绘制器（模式、置信水平、填充透明度都在这里配置）</summary>
    Public Property Ellipses As New MultivariateEllipseDrawer

    ''' <summary>是否在点旁边标注样本名</summary>
    Public Property ShowPointLabels As Boolean = False

    ''' <summary>点标记尺寸，&lt;= 0 时使用主题的 <c>MarkerSize</c></summary>
    Public Property PointSize As Single = 0

    ''' <summary>是否绘制 x=0 / y=0 的虚线参考轴</summary>
    Public Property ShowZeroLines As Boolean = True

    ''' <summary>各分组依次使用的标记形状（不足时循环）</summary>
    Public Property MarkerShapes As MarkerShape() = {
        MarkerShape.Circle,
        MarkerShape.Square,
        MarkerShape.Triangle,
        MarkerShape.Diamond,
        MarkerShape.Hexagon,
        MarkerShape.Pentagon
    }

    ''' <summary>样本名标注使用的字体，Nothing 时使用主题的注释字体</summary>
    Public Property LabelFont As Font = Nothing

    Public Sub New(width As Integer, height As Integer,
                   Optional theme As PlotTheme = Nothing,
                   Optional driver As Drivers = Drivers.Default)
        MyBase.New(width, height, theme, driver)
    End Sub

    Public Sub New(width As Integer, height As Integer, data As MultivariateScoreSet,
                   Optional theme As PlotTheme = Nothing,
                   Optional driver As Drivers = Drivers.Default)
        MyBase.New(width, height, theme, driver)

        Me.Scores = data
    End Sub

    ''' <summary>
    ''' 直接从多元分析结果构造得分图。
    ''' </summary>
    Public Sub New(width As Integer, height As Integer, result As MultivariateAnalysisResult,
                   Optional xComponent As Integer = 0,
                   Optional yComponent As Integer = 1,
                   Optional groups As String() = Nothing,
                   Optional sampleNames As String() = Nothing,
                   Optional orthogonal As Boolean = True,
                   Optional theme As PlotTheme = Nothing,
                   Optional driver As Drivers = Drivers.Default)

        Call Me.New(width, height,
                    MultivariateScoreSet.FromResult(result, xComponent, yComponent, groups, sampleNames, orthogonal),
                    theme, driver)
    End Sub

    ''' <summary>设置数据并立即渲染（使用引擎自身的画布）。</summary>
    Public Overloads Sub Plot(data As MultivariateScoreSet)
        Me.Scores = data
        Call MyBase.Plot()
    End Sub

    Protected Overrides Sub PlotInternal(ByRef g As IGraphics, canvas As GraphicsRegion)
        Call DrawBackground()
        Call ComputePlotArea()
        Call DrawPlotArea()
        Call DrawTitle()

        Dim data As MultivariateScoreSet = Me.Scores

        If data Is Nothing OrElse data.Size = 0 Then
            Call DrawEmptyState("no score data")

            Return
        End If

        Dim n As Integer = data.Size

        ' ---- 坐标范围 ----
        Dim xmin#, xmax#, ymin#, ymax#

        Call MultivariatePlotHelper.RangeOf(data.X.Take(n).ToArray, xmin, xmax)
        Call MultivariatePlotHelper.RangeOf(data.Y.Take(n).ToArray, ymin, ymax)
        Call Expand(xmin, xmax)
        Call Expand(ymin, ymax)

        ' 注意：局部变量 xmin/ymin 与基类的 XMin/YMin 属性同名（VB 不区分大小写），
        ' 因此必须显式写 Me. 才能访问到属性
        If Me.XMin.HasValue Then xmin = Me.XMin.Value
        If Me.XMax.HasValue Then xmax = Me.XMax.Value
        If Me.YMin.HasValue Then ymin = Me.YMin.Value
        If Me.YMax.HasValue Then ymax = Me.YMax.Value

        If String.IsNullOrEmpty(XLabel) Then XLabel = data.XTitle
        If String.IsNullOrEmpty(YLabel) Then YLabel = data.YTitle

        Call DrawAxisAndGrid(xmin, xmax, ymin, ymax)

        If ShowZeroLines Then
            Call DrawZeroLines(xmin, xmax, ymin, ymax)
        End If

        ' ---- 分组解析 ----
        Dim groups As String() = data.Groups

        If groups Is Nothing OrElse groups.Length <> n Then
            groups = Enumerable.Repeat("All", n).ToArray
        End If

        Dim colors As Dictionary(Of String, Color) = MultivariatePlotHelper.GroupColors(groups, Theme.Palette)
        Dim names As String() = MultivariatePlotHelper.GroupNames(groups)
        Dim markerSize As Single = If(PointSize > 0, PointSize, Theme.MarkerSize)
        Dim labelFont As Font = If(Me.LabelFont, Theme.AnnotationFont)

        ' ---- 逐分组绘制椭圆 + 散点 ----
        For gi As Integer = 0 To names.Length - 1
            Dim name As String = names(gi)
            Dim color As Color = colors(name)
            Dim members As New List(Of Integer)

            For i As Integer = 0 To n - 1
                If String.Equals(groups(i), name, StringComparison.Ordinal) Then
                    Call members.Add(i)
                End If
            Next

            Dim points As PointF() = ProjectPoints(data, members, xmin, xmax, ymin, ymax)

            ' 先画椭圆（位于点下层），这样数据点不会被半透明色块糊住
            Ellipses.Draw(g, Theme, points, color)

            Dim shape As MarkerShape = MarkerShapes(gi Mod MarkerShapes.Length)

            For Each p As PointF In points
                If Not InPlotArea(p) Then
                    Continue For
                End If

                Call DrawMarker(p.X, p.Y, shape, markerSize, color)
            Next

            If ShowPointLabels Then
                Call DrawPointLabels(data, members, labelFont, xmin, xmax, ymin, ymax)
            End If
        Next

        ' ---- 分组图例 ----
        Dim legends As New List(Of Series)

        For gi As Integer = 0 To names.Length - 1
            Call legends.Add(New Series With {
                .Name = names(gi),
                .Color = colors(names(gi)),
                .MarkerShape = MarkerShapes(gi Mod MarkerShapes.Length)
            })
        Next

        Call DrawLegend(legends)
    End Sub

    Private Function ProjectPoints(data As MultivariateScoreSet, members As List(Of Integer),
                                   xmin As Double, xmax As Double,
                                   ymin As Double, ymax As Double) As PointF()
        Dim points As PointF() = New PointF(members.Count - 1) {}

        For k As Integer = 0 To members.Count - 1
            Dim i As Integer = members(k)

            points(k) = New PointF(ToPixelX(data.X(i), xmin, xmax), ToPixelY(data.Y(i), ymin, ymax))
        Next

        Return points
    End Function

    Private Sub DrawPointLabels(data As MultivariateScoreSet, members As List(Of Integer), font As Font,
                                xmin As Double, xmax As Double, ymin As Double, ymax As Double)
        Dim names As String() = data.SampleNames

        If names Is Nothing Then
            Return
        End If

        ' 简单的标签避让：与已经画出的标签重叠时整体下移，避免同一簇内的样本名糊成一团
        Dim occupied As New List(Of RectangleF)

        Using brush As New SolidBrush(Color.FromArgb(235, Theme.TextColor))
            For Each i As Integer In members
                If i >= names.Length OrElse String.IsNullOrEmpty(names(i)) Then
                    Continue For
                End If

                Dim p As New PointF(ToPixelX(data.X(i), xmin, xmax), ToPixelY(data.Y(i), ymin, ymax))

                If Not InPlotArea(p) Then
                    Continue For
                End If

                Dim size As SizeF = MeasureString(names(i), font)
                Dim location As PointF = MultivariatePlotHelper.PlaceLabel(
                    p.X + 4, p.Y - size.Height, size, _plotArea, occupied)

                Call DrawLabel(names(i), font, brush, location.X, location.Y, Nothing, Nothing)
            Next
        End Using
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

    Private Function InPlotArea(point As PointF) As Boolean
        Return Not Single.IsNaN(point.X) AndAlso Not Single.IsNaN(point.Y) AndAlso
            point.X >= _plotArea.Left AndAlso point.X <= _plotArea.Right AndAlso
            point.Y >= _plotArea.Top AndAlso point.Y <= _plotArea.Bottom
    End Function

    ''' <summary>给坐标轴留出 8% 的余量，避免最外侧的点贴在边框上。</summary>
    Private Shared Sub Expand(ByRef min As Double, ByRef max As Double)
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
