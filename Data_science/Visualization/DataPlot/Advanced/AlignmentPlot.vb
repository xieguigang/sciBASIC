#Region "Microsoft.VisualBasic::6c3a9e1b7d2f4809a1b5c8e3f7d2a6b4, Data_science\Visualization\DataPlot\Advanced\AlignmentPlot.vb"

    ' 
    '       sciBASIC.NET Foundation, GPL3 Licensed
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.

    ' Class AlignmentPlot
    ' 
    '     Properties: Dark-like -> Highlights, TrackHeight, Tracks, ShowTrackNames
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Sub: Plot
    ' 
#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports std = System.Math

' ============================================================================
'  AlignmentPlot.vb - 多序列对齐图（tracks / multi-panel）
'
'  旧实现的 AlignmentPlot 用来把「查询序列与参考序列的对齐区间」画成一叠信号轨。
'  这里保留它的核心语义（共享同一条横轴的多条并排轨迹），并把它泛化成
'  「任意数量的信号轨 + 可标注的高亮区间」，这样它既能画序列比对结果，
'  也能画染色质信号、多通道时间序列这类 panel 图。
' ============================================================================

''' <summary>多序列对齐图</summary>
Public Class AlignmentPlot
    Inherits PlotEngine

    ''' <summary>每条轨道一个 Series（X 共享同一值域）</summary>
    Public Property Tracks As List(Of Series) = New List(Of Series)()
    ''' <summary>需要高亮的 X 区间，元素为 (起点, 终点)</summary>
    Public Property Highlights As New List(Of (start As Double, [end] As Double))()
    ''' <summary>每条轨道占的像素高度，小于等于 0 时自动均分</summary>
    Public Property TrackHeight As Single = 0
    ''' <summary>是否在每条轨道左侧写出轨道名</summary>
    Public Property ShowTrackNames As Boolean = True
    ''' <summary>是否填充曲线下方（面积模式）</summary>
    Public Property Fill As Boolean = False
    ''' <summary>高亮区间的填充色</summary>
    Public Property HighlightColor As Color = Color.FromArgb(40, 100, 180)

    Public Sub New(width As Integer, height As Integer, Optional theme As PlotTheme = Nothing, Optional driver As Microsoft.VisualBasic.Imaging.Driver.Drivers = Microsoft.VisualBasic.Imaging.Driver.Drivers.Default)
        MyBase.New(width, height, theme, driver)
    End Sub

    Public Sub Plot()
        If Tracks Is Nothing OrElse Tracks.Count = 0 Then
            Throw New InvalidOperationException("AlignmentPlot requires at least one track.")
        End If

        DrawBackground()
        ComputePlotArea()
        DrawPlotArea()
        DrawTitle()

        Dim xmin = If(Me.XMin, Tracks.SelectMany(Function(t) t.X).Min())
        Dim xmax = If(Me.XMax, Tracks.SelectMany(Function(t) t.X).Max())
        If xmax <= xmin Then xmax = xmin + 1

        Dim n = Tracks.Count
        Dim h As Single = If(TrackHeight > 0, TrackHeight, _plotArea.Height / n)
        Dim nameWidth As Single = 0

        If ShowTrackNames Then
            For Each t In Tracks
                nameWidth = std.Max(nameWidth, MeasureString(t.Name, Theme.LegendFont).Width)
            Next
            nameWidth += 10
        End If

        Dim left = _plotArea.Left + nameWidth
        Dim width = _plotArea.Right - left

        For i = 0 To n - 1
            Dim t = Tracks(i)
            Dim top = _plotArea.Top + i * h
            Dim region As New RectangleF(left, top, width, h * 0.92F)

            DrawTrackFrame(region)
            DrawHighlights(region, xmin, xmax)

            If t.X.Length = 0 OrElse t.Y.Length = 0 Then
                Continue For
            End If

            Dim vmin = t.Y.Min()
            Dim vmax = t.Y.Max()
            Geometry.ExpandRange(vmin, vmax, 0.08)

            Dim pts = New List(Of PointF)()
            For j = 0 To std.Min(t.X.Length, t.Y.Length) - 1
                pts.Add(New PointF(region.Left + CSng((t.X(j) - xmin) / (xmax - xmin) * region.Width),
                                   region.Bottom - CSng((t.Y(j) - vmin) / (vmax - vmin) * region.Height)))
            Next

            If pts.Count < 2 Then
                Continue For
            End If

            Dim color = If(t.Color, Theme.Palette(i Mod Theme.Palette.Length))

            If Fill Then
                Dim area As New List(Of PointF)(pts)
                area.Add(New PointF(pts(pts.Count - 1).X, region.Bottom))
                area.Add(New PointF(pts(0).X, region.Bottom))
                Using br As New SolidBrush(Color.FromArgb(t.FillAlpha, color))
                    _g.FillPolygon(br, area.ToArray())
                End Using
            End If

            Using pen As New Pen(color, Theme.LineWidth)
                pen.DashStyle = t.LineStyle
                _g.DrawLines(pen, pts.ToArray())
            End Using

            If ShowTrackNames Then
                Using br As New SolidBrush(Theme.TextColor)
                    Dim size = MeasureString(t.Name, Theme.LegendFont)
                    _g.DrawString(t.Name, Theme.LegendFont, br, region.Left - size.Width - 6,
                                  region.Top + region.Height / 2 - size.Height / 2)
                End Using
            End If

            ' 轨道内的量纲标注
            Using br As New SolidBrush(Theme.SubTitleColor)
                Dim lbl = Geometry.NumberLabel(vmax)
                _g.DrawString(lbl, Theme.TickLabelFont, br, region.Right + 3, region.Top)
            End Using
        Next
    End Sub

    Private Sub DrawTrackFrame(region As RectangleF)
        Using pen As New Pen(Theme.AxisColor, Theme.AxisLineWidth)
            _g.DrawLine(pen, region.Left, region.Bottom, region.Right, region.Bottom)
        End Using
    End Sub

    Private Sub DrawHighlights(region As RectangleF, xmin As Double, xmax As Double)
        If Highlights Is Nothing OrElse Highlights.Count = 0 Then Return

        Using br As New SolidBrush(HighlightColor)
            For Each hl In Highlights
                Dim px0 = region.Left + CSng((hl.start - xmin) / (xmax - xmin) * region.Width)
                Dim px1 = region.Left + CSng((hl.end - xmin) / (xmax - xmin) * region.Width)
                px0 = std.Max(region.Left, px0)
                px1 = std.Min(region.Right, px1)
                If px1 <= px0 Then Continue For
                _g.FillRectangle(br, px0, region.Top, px1 - px0, region.Height)
            Next
        End Using
    End Sub
End Class
