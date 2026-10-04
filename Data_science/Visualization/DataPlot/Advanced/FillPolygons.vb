#Region "Microsoft.VisualBasic::2b7f9d4c6a1e4308b5c2d8f7a9b3e6c1, Data_science\Visualization\DataPlot\Advanced\FillPolygons.vb"

    ' 
    '       sciBASIC.NET Foundation, GPL3 Licensed
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.

    ' Class FillPolygons
    ' 
    '     Properties: Groups, ShowLegendImpl, ShowOutline, ShowPoints, PointSize
    '                 ShowAxes
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Sub: Plot
    ' 
#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports System.Runtime.CompilerServices
Imports std = System.Math

' ============================================================================
'  FillPolygons.vb - 多边形组填充图
'
'  合并旧实现的 FillPolygons 与 PolygonPlot2D：一个标签对应若干闭合子区域，
'  常见于「二维平面上的区域着色」（聚类簇的凸包、表达区域的毛刺范围等）。
'
'  相对旧实现补齐了三处缺失：
'   · PolygonGroup.Label 旧版本从未被使用，这里参与图例；
'   · 旧版本只填充不描边，这里提供 ShowOutline；
'   · 旧版本只支持「点是一个多边形」的散点叠加，这里统一为 ShowPoints 开关。
' ============================================================================

''' <summary>多边形组填充图</summary>
Public Class FillPolygons
    Inherits PlotEngine

    ''' <summary>多边形组（也可直接给散点 -> 每份散点一个多边形）</summary>
    Public Property Groups As List(Of PolygonGroup) = New List(Of PolygonGroup)()
    ''' <summary>是否描出多边形轮廓</summary>
    Public Property ShowOutline As Boolean = True
    ''' <summary>是否额外用圆点标出顶点</summary>
    Public Property ShowPoints As Boolean = False
    ''' <summary>顶点圆点直径</summary>
    Public Property PointSize As Single = 4.0F
    ''' <summary>是否绘制坐标轴与网格</summary>
    Public Property ShowAxes As Boolean = True
    ''' <summary>是否绘制图例</summary>
    Public Property ShowLegend As Boolean = True

    Public Sub New(width As Integer, height As Integer, Optional theme As PlotTheme = Nothing)
        MyBase.New(width, height, theme)
    End Sub

    Public Sub Plot()
        If Groups Is Nothing OrElse Groups.Count = 0 Then
            Throw New InvalidOperationException("FillPolygons requires at least one polygon group.")
        End If

        DrawBackground()
        ComputePlotArea()
        DrawPlotArea()
        DrawTitle()

        Dim xmin As Double, xmax As Double, ymin As Double, ymax As Double
        ComputeRange(xmin, xmax, ymin, ymax)

        If ShowAxes Then
            DrawAxisAndGrid(xmin, xmax, ymin, ymax)
        End If

        For gi = 0 To Groups.Count - 1
            Dim grp = Groups(gi)
            Dim color As Color = If(grp.Color, Theme.Palette(gi Mod Theme.Palette.Length))
            Dim alpha = CInt(std.Max(0, std.Min(255, grp.Alpha * 255)))

            Using br As New SolidBrush(Color.FromArgb(alpha, color))
                For Each poly In Regions(grp)
                    If poly Is Nothing OrElse poly.Length < 3 Then Continue For

                    Dim pts = poly.Select(Function(p) New PointF(
                                  ToPixelX(p.X, xmin, xmax), ToPixelY(p.Y, ymin, ymax))).ToArray()

                    _g.FillPolygon(br, pts)
                Next
            End Using

            If ShowOutline Then
                Using pen As New Pen(color, Theme.LineWidth)
                    For Each poly In Regions(grp)
                        If poly Is Nothing OrElse poly.Length < 3 Then Continue For

                        _g.DrawPolygon(pen, poly.Select(Function(p) New PointF(
                                       ToPixelX(p.X, xmin, xmax), ToPixelY(p.Y, ymin, ymax))).ToArray())
                    Next
                End Using
            End If

            If ShowPoints Then
                Dim half = PointSize / 2
                Using br As New SolidBrush(color)
                    For Each poly In Regions(grp)
                        For Each p In poly
                            Dim px = ToPixelX(p.X, xmin, xmax), py = ToPixelY(p.Y, ymin, ymax)
                            _g.FillEllipse(br, px - half, py - half, PointSize, PointSize)
                        Next
                    Next
                End Using
            End If
        Next

        If ShowLegend Then
            Dim legends As New List(Of Series)()
            For gi = 0 To Groups.Count - 1
                legends.Add(New Series With {
                    .Name = Groups(gi).Label,
                    .Color = If(Groups(gi).Color, Theme.Palette(gi Mod Theme.Palette.Length)),
                    .MarkerShape = MarkerShape.Square
                })
            Next
            DrawLegend(legends)
        End If
    End Sub

    ''' <summary>安全地取出一个组的子多边形（SubRegions 可能为 Nothing）</summary>
    Private Function Regions(grp As PolygonGroup) As IEnumerable(Of PointF())
        If grp Is Nothing OrElse grp.SubRegions Is Nothing Then
            Return Enumerable.Empty(Of PointF())()
        End If
        Return grp.SubRegions
    End Function

    Private Sub ComputeRange(ByRef xmin As Double, ByRef xmax As Double,
                             ByRef ymin As Double, ByRef ymax As Double)
        Dim xs As New List(Of Double)()
        Dim ys As New List(Of Double)()

        For Each grp In Groups
            For Each poly In Regions(grp)
                For Each p In poly
                    xs.Add(p.X)
                    ys.Add(p.Y)
                Next
            Next
        Next

        If xs.Count = 0 Then Throw New InvalidOperationException("No polygon vertex data to plot.")

        xmin = If(Me.XMin, xs.Min())
        xmax = If(Me.XMax, xs.Max())
        ymin = If(Me.YMin, ys.Min())
        ymax = If(Me.YMax, ys.Max())

        Geometry.ExpandRange(xmin, xmax, 0.05)
        Geometry.ExpandRange(ymin, ymax, 0.05)
    End Sub
End Class
