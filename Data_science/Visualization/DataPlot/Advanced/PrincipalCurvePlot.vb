#Region "Microsoft.VisualBasic::4a7d2e8c1b3f4906d8a5c2e7f9b1d3a6, Data_science\Visualization\DataPlot\Advanced\PrincipalCurvePlot.vb"

    ' 
    '       sciBASIC.NET Foundation, GPL3 Licensed
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.

    ' Class PrincipalCurvePlot
    ' 
    '     Properties: Curve, PointColor, ShowProjections, ShowPoints
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Sub: Plot
    ' 
#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports std = System.Math

' ============================================================================
'  PrincipalCurvePlot.vb - 主曲线可视化
'
'  旧实现（PrincipalCurveVisualizer.Draw）不画坐标轴、不画投影线，且曲线的采样点
'  不参与值域计算（曲线会画到画布外）。这里按同样的语义重写，但补上：
'   · 坐标轴与刻度（走统一的 DrawAxisAndGrid）；
'   · 点到曲线的投影虚线（投影点由调用方给出，算法本身不属于绘图职责）；
'   · 曲线采样点参与值域，曲线不会被裁掉。
' ============================================================================

''' <summary>主曲线图</summary>
Public Class PrincipalCurvePlot
    Inherits PlotEngine

    ''' <summary>原始散点</summary>
    Public Property Points As PointF() = {}
    ''' <summary>拟合出的主曲线顶点（按曲线顺序）</summary>
    Public Property Curve As PointF() = {}
    ''' <summary>每个散点在曲线上的投影点（可选，用于画投影虚线）</summary>
    Public Property Projections As PointF() = Nothing
    ''' <summary>是否绘制散点</summary>
    Public Property ShowPoints As Boolean = True
    ''' <summary>是否绘制投影虚线</summary>
    Public Property ShowProjections As Boolean = False
    ''' <summary>散点颜色</summary>
    Public Property PointColor As Color = Color.SteelBlue
    ''' <summary>散点直径</summary>
    Public Property PointSize As Single = 6.0F
    ''' <summary>主曲线颜色</summary>
    Public Property CurveColor As Color = Color.Crimson
    ''' <summary>主曲线线宽</summary>
    Public Property CurveWidth As Single = 2.5F

    Public Sub New(width As Integer, height As Integer, Optional theme As PlotTheme = Nothing)
        MyBase.New(width, height, theme)
    End Sub

    Public Sub Plot()
        If (Points Is Nothing OrElse Points.Length = 0) AndAlso (Curve Is Nothing OrElse Curve.Length = 0) Then
            Throw New InvalidOperationException("PrincipalCurvePlot needs sample points or a curve.")
        End If

        DrawBackground()
        ComputePlotArea()
        DrawPlotArea()
        DrawTitle()

        Dim xmin As Double, xmax As Double, ymin As Double, ymax As Double
        ComputeRange(xmin, xmax, ymin, ymax)

        DrawAxisAndGrid(xmin, xmax, ymin, ymax)

        If ShowProjections AndAlso Projections IsNot Nothing Then
            DrawProjectionLines(xmin, xmax, ymin, ymax)
        End If

        If Curve IsNot Nothing AndAlso Curve.Length > 1 Then
            Using pen As New Pen(CurveColor, CurveWidth)
                pen.StartCap = LineCap.Round
                pen.EndCap = LineCap.Round
                _g.DrawLines(pen, Curve.Select(Function(p) New PointF(
                                   ToPixelX(p.X, xmin, xmax), ToPixelY(p.Y, ymin, ymax))).ToArray())
            End Using
        End If

        If ShowPoints Then
            Dim half = PointSize / 2
            Using br As New SolidBrush(PointColor)
                For Each p In OrEmpty(Points)
                    Dim px = ToPixelX(p.X, xmin, xmax)
                    Dim py = ToPixelY(p.Y, ymin, ymax)
                    _g.FillEllipse(br, px - half, py - half, PointSize, PointSize)
                Next
            End Using
        End If

        DrawCurveLegend()
    End Sub

    Private Function OrEmpty(pts As PointF()) As PointF()
        Return If(pts Is Nothing, New PointF() {}, pts)
    End Function

    Private Sub ComputeRange(ByRef xmin As Double, ByRef xmax As Double,
                             ByRef ymin As Double, ByRef ymax As Double)
        Dim all = OrEmpty(Points).Concat(OrEmpty(Curve)).ToArray()

        xmin = If(Me.XMin, all.Min(Function(p) p.X))
        xmax = If(Me.XMax, all.Max(Function(p) p.X))
        ymin = If(Me.YMin, all.Min(Function(p) p.Y))
        ymax = If(Me.YMax, all.Max(Function(p) p.Y))

        Geometry.ExpandRange(xmin, xmax, 0.05)
        Geometry.ExpandRange(ymin, ymax, 0.05)
    End Sub

    Private Sub DrawProjectionLines(xmin As Double, xmax As Double, ymin As Double, ymax As Double)
        Dim n = std.Min(Points.Length, Projections.Length)

        Using pen As New Pen(Color.FromArgb(140, Theme.AxisColor), Theme.AxisLineWidth)
            pen.DashStyle = DashStyle.Dot
            For i = 0 To n - 1
                _g.DrawLine(pen, ToPixelX(Points(i).X, xmin, xmax), ToPixelY(Points(i).Y, ymin, ymax),
                            ToPixelX(Projections(i).X, xmin, xmax), ToPixelY(Projections(i).Y, ymin, ymax))
            Next
        End Using
    End Sub

    Private Sub DrawCurveLegend()
        Dim legends As New List(Of Series) From {
            New Series With {.Name = "points", .Color = PointColor, .MarkerShape = MarkerShape.Circle},
            New Series With {.Name = "principal curve", .Color = CurveColor, .MarkerShape = MarkerShape.None}
        }

        DrawLegend(legends)
    End Sub
End Class
