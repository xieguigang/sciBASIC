#Region "Microsoft.VisualBasic::1b5f8c2e7a3d4913d6f2b5c8e1a4d7c9, Data_science\Visualization\DataPlot\Statistics\TimeTrendsPlot.vb"

    ' 
    '       sciBASIC.NET Foundation, GPL3 Licensed
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.

    ' Class TimeTrendsPlot
    ' 
    '     Properties: Trend, Smooth, ShowPoints, Fill, LineColor
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Sub: Plot
    ' 
#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Linq
Imports std = System.Math

' ============================================================================
'  TimeTrendsPlot.vb - 时间趋势图
'
'  迁移自 Plots-statistics 的 TimeTrends（旧项目里少数不依赖外部算法包的图表）。
'  支持多条趋势线（List(Of ODESeries)），常用于 ODE 结果与时间序列的输出。
' ============================================================================

''' <summary>时间趋势图</summary>
Public Class TimeTrendsPlot
    Inherits PlotEngine

    ''' <summary>趋势数据：每条一个系列</summary>
    Public Property Trends As List(Of ODESeries) = New List(Of ODESeries)()
    ''' <summary>是否用样条平滑</summary>
    Public Property Smooth As Boolean = False
    ''' <summary>是否在曲线上标出采样点</summary>
    Public Property ShowPoints As Boolean = False
    ''' <summary>是否填充曲线下方</summary>
    Public Property Fill As Boolean = False
    ''' <summary>线宽，小于等于 0 时用主题设定</summary>
    Public Property LineWidth As Single = 0

    Public Sub New(width As Integer, height As Integer, Optional theme As PlotTheme = Nothing)
        MyBase.New(width, height, theme)
    End Sub

    ''' <summary>兼容旧的单序列 TimePoint 输入</summary>
    Public Shared Function FromTimePoints(points As IEnumerable(Of TimePoint), Optional name As String = "trend") As ODESeries
        Dim arr = points.SafeQuery.ToArray()
        Return New ODESeries With {
            .Name = name,
            .X = arr.Select(Function(p) p.Time).ToArray(),
            .Y = arr.Select(Function(p) p.Value).ToArray()
        }
    End Function

    Public Sub Plot()
        If Trends Is Nothing OrElse Trends.Count = 0 Then
            Throw New InvalidOperationException("TimeTrendsPlot requires at least one trend series.")
        End If

        DrawBackground()
        ComputePlotArea()
        DrawPlotArea()
        DrawTitle()

        Dim allX = Trends.SelectMany(Function(t) t.X).ToArray()
        Dim allY = Trends.SelectMany(Function(t) t.Y).ToArray()

        Dim xmin = If(Me.XMin, allX.Min())
        Dim xmax = If(Me.XMax, allX.Max())
        Dim ymin = If(Me.YMin, allY.Min())
        Dim ymax = If(Me.YMax, allY.Max())

        Geometry.ExpandRange(xmin, xmax, 0)
        Geometry.ExpandRange(ymin, ymax, 0.06)

        DrawAxisAndGrid(xmin, xmax, ymin, ymax)

        For i = 0 To Trends.Count - 1
            Dim trend = Trends(i)
            Dim color = If(trend.Color, Theme.Palette(i Mod Theme.Palette.Length))
            Dim n = std.Min(trend.X.Length, trend.Y.Length)
            If n = 0 Then Continue For

            Dim pts = Enumerable.Range(0, n).Select(Function(j) New PointF(
                        ToPixelX(trend.X(j), xmin, xmax), ToPixelY(trend.Y(j), ymin, ymax))).ToArray()
            Dim w = If(LineWidth > 0, LineWidth, Theme.LineWidth)

            If Fill AndAlso n > 1 Then
                Dim area As New List(Of PointF)(pts)
                area.Add(New PointF(pts(pts.Length - 1).X, ToPixelY(ymin, ymin, ymax)))
                area.Add(New PointF(pts(0).X, ToPixelY(ymin, ymin, ymax)))
                Using br As New SolidBrush(Color.FromArgb(60, color))
                    _g.FillPolygon(br, area.ToArray())
                End Using
            End If

            Dim drawPts = If(Smooth AndAlso n >= 3, Geometry.SmoothSpline(pts), pts)

            Using pen As New Pen(color, w)
                pen.StartCap = LineCap.Round
                pen.EndCap = LineCap.Round
                If n > 1 Then _g.DrawLines(pen, drawPts)
            End Using

            If ShowPoints Then
                For Each p In pts
                    DrawMarker(p.X, p.Y, MarkerShape.Circle, Theme.MarkerSize * 0.8F, color)
                Next
            End If
        Next

        Dim legends As New List(Of Series)()
        For i = 0 To Trends.Count - 1
            legends.Add(New Series With {
                .Name = Trends(i).Name,
                .Color = If(Trends(i).Color, Theme.Palette(i Mod Theme.Palette.Length)),
                .MarkerShape = If(ShowPoints, MarkerShape.Circle, MarkerShape.None)
            })
        Next
        DrawLegend(legends)
    End Sub
End Class
