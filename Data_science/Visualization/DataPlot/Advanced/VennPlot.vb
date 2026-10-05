#Region "Microsoft.VisualBasic::5d1c8f3a7e2b4906b9f4c1a8d3e7b0c5, Data_science\Visualization\DataPlot\Advanced\VennPlot.vb"

    ' 
    '       sciBASIC.NET Foundation, GPL3 Licensed
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.

    ' Class VennPlot
    ' 
    '     Properties: FillAlpha, Sets, ShowLabels, ShowLegendImpl
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Sub: Plot
    ' 
#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports std = System.Math

' ============================================================================
'  VennPlot.vb - 文氏图（2 集合 / 3 集合）
'
'  旧实现把「交集大小」近似成圆心距的线性插值，画出来的重叠区往往和数量对不上。
'  这里改成先让圆面积正比于集合大小（r ∝ √n），再用二分法反解圆心距，
'  使两圆 lens 的面积精确等于交集对应的面积，因此视觉上的重叠比例是可信的。
'
'  用法：给 Sets 填 2 或 3 个 VennSet，两两交集写在 Intersections 里即可。
' ============================================================================

''' <summary>文氏图</summary>
Public Class VennPlot
    Inherits PlotEngine

    ''' <summary>集合数据（2 个或 3 个）</summary>
    Public Property Sets As List(Of VennSet) = New List(Of VennSet)()
    ''' <summary>填充透明度（0~1）</summary>
    Public Property FillAlpha As Double = 0.45
    ''' <summary>是否在圆上标出集合名与元素数</summary>
    Public Property ShowLabels As Boolean = True

    Public Sub New(width As Integer, height As Integer, Optional theme As PlotTheme = Nothing, Optional driver As Microsoft.VisualBasic.Imaging.Driver.Drivers = Microsoft.VisualBasic.Imaging.Driver.Drivers.Default)
        MyBase.New(width, height, theme, driver)
    End Sub

    Public Sub Plot()
        If Sets Is Nothing OrElse Sets.Count < 2 OrElse Sets.Count > 3 Then
            Throw New InvalidOperationException("VennPlot supports exactly 2 or 3 sets.")
        End If

        VennSet.FixSetCompleteness(Sets)

        DrawBackground()
        ComputePlotArea()
        DrawPlotArea()
        DrawTitle()

        Dim circles = Layout()
        Fit(circles)

        For i = 0 To circles.Length - 1
            DrawCircle(circles(i), i)
        Next

        If ShowLabels Then
            For i = 0 To circles.Length - 1
                DrawCircleLabel(circles(i))
            Next
        End If

        If ShowLegend Then DrawSetLegend()
    End Sub

    ' ---------------- 布局 ----------------

    Private Structure CircleLayout
        Public Center As PointF
        Public Radius As Single
        Public Owner As VennSet
    End Structure

    Private Function Layout() As CircleLayout()
        ' 面积 ∝ 元素数 -> r = k * sqrt(n)，k 先取 1，后面统一缩放到绘图区
        Dim radii = Sets.Select(Function(s) If(s.Size <= 0, 1.0, std.Sqrt(s.Size))).ToArray()
        Dim result(Sets.Count - 1) As CircleLayout

        For i = 0 To Sets.Count - 1
            result(i) = New CircleLayout With {
                .Radius = CSng(radii(i)),
                .Owner = Sets(i)
            }
        Next

        ' 单位面积对应的元素数系数：area = size * k^2 * pi / size = π * r²/n = π（因为 r=√n）
        ' 因此 lens 面积的目标值 = 交集数（半径按 √n 取时面积就是 π*n，比例因子统一）
        Dim unit = std.PI

        If Sets.Count = 2 Then
            result(0).Center = New PointF(0, 0)
            Dim d12 = SolveDistance(result(0).Radius, result(1).Radius, Sets(0).IntersectWith(Sets(1).Name) * unit)
            result(1).Center = New PointF(CSng(d12), 0)
        Else
            result(0).Center = New PointF(0, 0)

            Dim d12 = SolveDistance(result(0).Radius, result(1).Radius, Sets(0).IntersectWith(Sets(1).Name) * unit)
            Dim d13 = SolveDistance(result(0).Radius, result(2).Radius, Sets(0).IntersectWith(Sets(2).Name) * unit)
            Dim d23 = SolveDistance(result(1).Radius, result(2).Radius, Sets(1).IntersectWith(Sets(2).Name) * unit)

            result(1).Center = New PointF(CSng(d12), 0)

            ' 由 d13、d23 求第三个圆心（取靠下的那个交点）
            Dim x = (d13 * d13 - d23 * d23 + d12 * d12) / (2 * If(d12 = 0, 1, d12))
            Dim y2 = d13 * d13 - x * x
            Dim y = If(y2 > 0, std.Sqrt(y2), 0)

            result(2).Center = New PointF(CSng(x), CSng(y))
        End If

        Return result
    End Function

    ''' <summary>二分反解圆心距，使两圆重叠的 lens 面积等于 <paramref name="targetArea"/></summary>
    Private Function SolveDistance(r1 As Double, r2 As Double, targetArea As Double) As Double
        Dim dMin = std.Abs(r1 - r2)
        Dim dMax = r1 + r2
        Dim areaMax = Geometry.CircleOverlapArea(r1, r2, 0)

        If areaMax <= 0 Then Return dMax * 0.75
        If targetArea >= areaMax Then Return 0
        If targetArea <= 0 Then Return dMax

        Dim lo = 0.0, hi = dMax

        For i = 0 To 60
            Dim mid = (lo + hi) / 2
            Dim area = Geometry.CircleOverlapArea(r1, r2, mid)

            If area > targetArea Then
                lo = mid
            Else
                hi = mid
            End If
        Next

        Return std.Max(dMin, (lo + hi) / 2)
    End Function

    ''' <summary>把整组圆等比缩放并居中到绘图区</summary>
    Private Sub Fit(circles As CircleLayout())
        Dim xmin As Single = Single.MaxValue, xmax As Single = Single.MinValue
        Dim ymin As Single = Single.MaxValue, ymax As Single = Single.MinValue

        For Each ccc In circles
            xmin = std.Min(xmin, ccc.Center.X - ccc.Radius) : xmax = std.Max(xmax, ccc.Center.X + ccc.Radius)
            ymin = std.Min(ymin, ccc.Center.Y - ccc.Radius) : ymax = std.Max(ymax, ccc.Center.Y + ccc.Radius)
        Next

        Dim gw = xmax - xmin, gh = ymax - ymin
        If gw <= 0 Then gw = 1
        If gh <= 0 Then gh = 1

        Dim scale = std.Min(_plotArea.Width * 0.9 / gw, _plotArea.Height * 0.9 / gh)
        Dim cx = (xmin + xmax) / 2
        Dim cy = (ymin + ymax) / 2

        For i = 0 To circles.Length - 1
            circles(i).Radius = CSng(circles(i).Radius * scale)
            circles(i).Center = New PointF(
                CSng(_plotArea.Left + _plotArea.Width / 2 + (circles(i).Center.X - cx) * scale),
                CSng(_plotArea.Top + _plotArea.Height / 2 + (circles(i).Center.Y - cy) * scale))
        Next
    End Sub

    ' ---------------- 绘制 ----------------

    Private Function SetColor(i As Integer) As Color
        Return If(Sets(i).Color, Theme.Palette(i Mod Theme.Palette.Length))
    End Function

    Private Sub DrawCircle(c As CircleLayout, i As Integer)
        Dim clr = SetColor(i)
        Dim alpha = CInt(255 * FillAlpha)

        Using br As New SolidBrush(Color.FromArgb(alpha, clr)),
              pen As New Pen(clr, Theme.LineWidth * 1.2F)
            _g.FillEllipse(br, c.Center.X - c.Radius, c.Center.Y - c.Radius, c.Radius * 2, c.Radius * 2)
            _g.DrawEllipse(pen, c.Center.X - c.Radius, c.Center.Y - c.Radius, c.Radius * 2, c.Radius * 2)
        End Using
    End Sub

    Private Sub DrawCircleLabel(c As CircleLayout)
        Dim text = $"{c.Owner.Name}{vbCrLf}{c.Owner.Size}"
        Dim size = MeasureString(text, Theme.LegendFont)
        Dim x = c.Center.X - size.Width / 2
        Dim y = c.Center.Y - size.Height / 2

        Using br As New SolidBrush(Theme.TextColor)
            Using bg As New SolidBrush(Color.FromArgb(200, Theme.LegendBackgroundColor))
                _g.FillRectangle(bg, x - 3, y - 2, size.Width + 6, size.Height + 4)
            End Using
            _g.DrawString(text, Theme.LegendFont, br, New RectangleF(x, y, size.Width + 1, size.Height + 1))
        End Using
    End Sub

    Private Sub DrawSetLegend()
        Dim legends As New List(Of Series)()

        For i = 0 To Sets.Count - 1
            legends.Add(New Series With {
                .Name = Sets(i).Name,
                .Color = SetColor(i),
                .MarkerShape = MarkerShape.Circle
            })
        Next

        DrawLegend(legends)
    End Sub
End Class
