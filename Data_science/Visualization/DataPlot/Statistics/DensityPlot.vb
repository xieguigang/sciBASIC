#Region "Microsoft.VisualBasic::8d3f6b1c5a7b4911b4d9a3f6e8c1b5a7, Data_science\Visualization\DataPlot\Statistics\DensityPlot.vb"

    ' 
    '       sciBASIC.NET Foundation, GPL3 Licensed
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.

    ' Class DensityPlot
    ' 
    '     Properties: Points, GridResolution, ColorMap, ShowPoints, ShowContour
    '                 ShowColorLegend, PointColor
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
'  DensityPlot.vb - 二维散点密度图
'
'  旧实现用 Graph 包的 Grid 结构做网格计数；这里改为复用 Geometry.Density2D
'  在本地完成核密度估计，既保持同样的视觉效果，又不需要为此引入新依赖。
' ============================================================================

''' <summary>二维散点密度图</summary>
Public Class DensityPlot
    Inherits PlotEngine

    ''' <summary>散点（数据坐标）</summary>
    Public Property Points As IEnumerable(Of PointF) = Nothing
    ''' <summary>网格分辨率（建议 120~240）</summary>
    Public Property GridResolution As Integer = 180
    ''' <summary>核半径（像素），小于等于 0 时按网格自动推算</summary>
    Public Property KernelRadius As Single = -1
    ''' <summary>色阶</summary>
    Public Property ColorMap As ColorScale.ColorMapType = ColorScale.ColorMapType.Inferno
    ''' <summary>是否在密度底图上叠加原始散点</summary>
    Public Property ShowPoints As Boolean = False
    ''' <summary>是否在密度底图上叠加等值线</summary>
    Public Property ShowContour As Boolean = True
    ''' <summary>是否绘制色阶图例条</summary>
    Public Property ShowColorLegend As Boolean = True
    ''' <summary>散点颜色</summary>
    Public Property PointColor As Color = Color.White

    Public Sub New(width As Integer, height As Integer, Optional theme As PlotTheme = Nothing, Optional driver As Microsoft.VisualBasic.Imaging.Driver.Drivers = Microsoft.VisualBasic.Imaging.Driver.Drivers.Default)
        MyBase.New(width, height, theme, driver)
    End Sub

    Public Sub Plot()
        If Points Is Nothing OrElse Not Points.Any() Then
            Throw New InvalidOperationException("DensityPlot requires at least one point.")
        End If

        Dim pts = Points.ToArray()

        DrawBackground()
        ComputePlotArea()
        DrawPlotArea()
        DrawTitle()

        Dim xmin = If(Me.XMin, pts.Min(Function(p) p.X))
        Dim xmax = If(Me.XMax, pts.Max(Function(p) p.X))
        Dim ymin = If(Me.YMin, pts.Min(Function(p) p.Y))
        Dim ymax = If(Me.YMax, pts.Max(Function(p) p.Y))

        Geometry.ExpandRange(xmin, xmax, 0.03)
        Geometry.ExpandRange(ymin, ymax, 0.03)

        DrawAxisAndGrid(xmin, xmax, ymin, ymax)

        ' 先把数据点换算到像素空间，再在像素网格上做密度估计
        Dim screen = pts.Select(Function(p) New PointF(ToPixelX(p.X, xmin, xmax), ToPixelY(p.Y, ymin, ymax))).ToArray()
        Dim n = If(GridResolution < 16, 16, GridResolution)
        Dim density = Geometry.Density2D(screen, _plotArea, n, n, KernelRadius)

        DrawDensityCells(density, n)

        If ShowContour Then DrawDensityContour(density, n)

        If ShowPoints Then
            Using br As New SolidBrush(PointColor)
                For Each p In screen
                    _g.FillEllipse(br, p.X - 1.5F, p.Y - 1.5F, 3, 3)
                Next
            End Using
        End If

        If ShowColorLegend Then
            DrawColorLegend(ColorMap, 0, 1, horizontal:=False, tickCount:=5, title:="density")
        End If
    End Sub

    Private Sub DrawDensityCells(density As Double(,), n As Integer)
        Dim cellW = _plotArea.Width / n
        Dim cellH = _plotArea.Height / n

        For row As Integer = 0 To n - 1
            For col As Integer = 0 To n - 1
                Dim v = density(row, col)
                If v <= 0.003 Then Continue For

                Using br As New SolidBrush(ColorScale.GetColorT(v, ColorMap))
                    _g.FillRectangle(br, _plotArea.Left + col * cellW, _plotArea.Top + row * cellH,
                                     cellW + 0.5F, cellH + 0.5F)
                End Using
            Next
        Next
    End Sub

    Private Sub DrawDensityContour(density As Double(,), n As Integer)
        Dim cellW = _plotArea.Width / n
        Dim cellH = _plotArea.Height / n

        Using pen As New Pen(Color.FromArgb(140, ColorScale.ReadableTextColor(ColorScale.GetColorT(0.9, ColorMap))),
                             Theme.GridMinorWidth)
            For level = 1 To 4
                Dim threshold = level / 5.0

                For row As Integer = 0 To n - 2
                    For col As Integer = 0 To n - 2
                        Dim v00 = density(row, col) >= threshold
                        Dim v10 = density(row, col + 1) >= threshold
                        Dim v11 = density(row + 1, col + 1) >= threshold
                        Dim v01 = density(row + 1, col) >= threshold

                        Dim x0 = _plotArea.Left + col * cellW
                        Dim y0 = _plotArea.Top + row * cellH
                        Dim x1 = x0 + cellW
                        Dim y1 = y0 + cellH

                        ' 四条边上的穿越情况各补一条短线段
                        If v00 <> v10 Then _g.DrawLine(pen, x0, y0, x1, y0)
                        If v10 <> v11 Then _g.DrawLine(pen, x1, y0, x1, y1)
                        If v11 <> v01 Then _g.DrawLine(pen, x0, y1, x1, y1)
                        If v01 <> v00 Then _g.DrawLine(pen, x0, y0, x0, y1)
                    Next
                Next
            Next
        End Using
    End Sub
End Class
