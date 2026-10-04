#Region "Microsoft.VisualBasic::3c8a1f6e9b2d4705a8c3e1f7d9b0a2c4, Data_science\Visualization\DataPlot\Basic\ContourPlot.vb"

    ' 
    '       sciBASIC.NET Foundation, GPL3 Licensed
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.

    ' Class ContourPlot
    ' 
    '     Properties: ColorMap, DrawContourLines, GridResolution, Levels, Matrix
    '                 Mode, Surface, ShowColorLegend, XRange, YRange
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Sub: Plot
    '     Enum ContourMode
    ' 
#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Linq
Imports std = System.Math

' ============================================================================
'  ContourPlot.vb - 等值线 / 等高填充 / 曲面热图
'
'  旧实现把这三个相近的东西拆成了 ContourPlot、ContourHeatMapPlot、
'  PlotContour、Utils 四份入口，本质上都是「把 z = f(x,y) 铺在像素网格上再上色」。
'  这里合并为一个类：
'    ContourMode.Lines   —— 只画等值线
'    ContourMode.Filled  —— 分层填充（可选叠加等值线）
'    ContourMode.Heatmap —— 连续色阶曲面
'  数据源可以是规则网格矩阵，也可以是 Func(Of Double, Double, Double) 函数。
' ============================================================================

''' <summary>等值线图 / 曲面热图</summary>
Public Class ContourPlot
    Inherits PlotEngine

    ''' <summary>渲染方式</summary>
    Public Enum ContourMode
        ''' <summary>只绘制等值线</summary>
        Lines
        ''' <summary>按等值区间分层填充</summary>
        Filled
        ''' <summary>连续色阶的曲面热图</summary>
        Heatmap
    End Enum

    ''' <summary>z = f(x, y) 的值 Playing 网格矩阵 [row=y, col=x]</summary>
    Public Property Matrix As Double(,) = Nothing

    ''' <summary>网格 X 坐标向量（为空时用列序号）</summary>
    Public Property XRange As Double() = Nothing
    ''' <summary>网格 Y 坐标向量（为空时用行序号）</summary>
    Public Property YRange As Double() = Nothing

    ''' <summary>用函数作为数据源时的求值委托，签名 z = f(x, y)</summary>
    Public Property Surface As Func(Of Double, Double, Double) = Nothing

    ''' <summary>求值分辨率（使用函数数据源时的每边 samples，建议 120~240）</summary>
    Public Property GridResolution As Integer = 160

    ''' <summary>渲染模式</summary>
    Public Property Mode As ContourMode = ContourMode.Filled
    ''' <summary>等值层数</summary>
    Public Property Levels As Integer = 12
    ''' <summary>色阶方案</summary>
    Public Property ColorMap As ColorScale.ColorMapType = ColorScale.ColorMapType.Viridis
    ''' <summary>是否绘制色阶图例条</summary>
    Public Property ShowColorLegend As Boolean = True
    ''' <summary>填充模式下是否额外描出等值线</summary>
    Public Property DrawContourLines As Boolean = True
    ''' <summary>等值线颜色，留空时用自动挑出的可读色</summary>
    Public Property LineColor As Color? = Nothing

    ' 网格 cell Dartasample 缓存
    Private grid As Double(,) = Nothing
    Private xs As Double() = Nothing
    Private ys As Double() = Nothing

    Public Sub New(width As Integer, height As Integer, Optional theme As PlotTheme = Nothing)
        MyBase.New(width, height, theme)
    End Sub

    Public Sub Plot()
        BuildGrid()

        DrawBackground()
        ComputePlotArea()
        DrawPlotArea()
        DrawTitle()

        Dim vmin = GridValue(grid, Function(v) v.Min())
        Dim vmax = GridValue(grid, Function(v) v.Max())
        If vmax <= vmin Then vmax = vmin + 1

        Dim xmin = If(Me.XMin, xs.Min())
        Dim xmax = If(Me.XMax, xs.Max())
        Dim ymin = If(Me.YMin, ys.Min())
        Dim ymax = If(Me.YMax, ys.Max())

        DrawAxisAndGrid(xmin, xmax, ymin, ymax)

        Select Case Mode
            Case ContourMode.Heatmap
                DrawSurface(xmin, xmax, ymin, ymax, vmin, vmax)
            Case ContourMode.Lines
                DrawLines(xmin, xmax, ymin, ymax, vmin, vmax)
            Case Else
                DrawBands(xmin, xmax, ymin, ymax, vmin, vmax)
                If DrawContourLines Then DrawLines(xmin, xmax, ymin, ymax, vmin, vmax, thin:=True)
        End Select

        If ShowColorLegend AndAlso Mode <> ContourMode.Lines Then
            DrawColorLegend(ColorMap, vmin, vmax, horizontal:=False, tickCount:=5)
        End If
    End Sub

    ' ---------------- 数据源 ----------------

    Private Sub BuildGrid()
        If Matrix IsNot Nothing Then
            grid = Matrix
            Dim rows = grid.GetLength(0)
            Dim cols = grid.GetLength(1)
            xs = If(XRange IsNot Nothing AndAlso XRange.Length = cols, XRange, Sequence(cols))
            ys = If(YRange IsNot Nothing AndAlso YRange.Length = rows, YRange, Sequence(rows))
            Return
        End If

        If Surface Is Nothing Then
            Throw New InvalidOperationException(
                "ContourPlot needs a data source: set Matrix or Surface.")
        End If

        If Me.XMin Is Nothing OrElse Me.XMax Is Nothing OrElse Me.YMin Is Nothing OrElse Me.YMax Is Nothing Then
            Throw New InvalidOperationException(
                "When Surface is used, XMin/XMax/YMin/YMax must be set so the plot knows where to sample.")
        End If

        Dim n = If(GridResolution < 8, 8, GridResolution)
        Dim x0 = Me.XMin.Value, x1 = Me.XMax.Value
        Dim y0 = Me.YMin.Value, y1 = Me.YMax.Value

        grid = New Double(n - 1, n - 1) {}
        xs = New Double(n - 1) {}
        ys = New Double(n - 1) {}

        For i = 0 To n - 1
            Dim y = y0 + (y1 - y0) * i / (n - 1)
            ys(i) = y
            For j = 0 To n - 1
                Dim x = x0 + (x1 - x0) * j / (n - 1)
                If i = 0 Then xs(j) = x
                grid(i, j) = Surface(x, y)
            Next
        Next
    End Sub

    Private Function Sequence(n As Integer) As Double()
        Return Enumerable.Range(0, n).Select(Function(i) CDbl(i)).ToArray()
    End Function

    Private Function GridValue(g As Double(,), agg As Func(Of IEnumerable(Of Double), Double)) As Double
        Dim list As New List(Of Double)()
        For i = 0 To g.GetLength(0) - 1
            For j = 0 To g.GetLength(1) - 1
                list.Add(g(i, j))
            Next
        Next
        Return agg(list)
    End Function

    ' ---------------- 渲染 ----------------

    ''' <summary>把网格的每个 cell 映射到一个像素矩形后填色</summary>
    Private Sub DrawSurface(xmin As Double, xmax As Double, ymin As Double, ymax As Double,
                            vmin As Double, vmax As Double)
        Dim rows = grid.GetLength(0)
        Dim cols = grid.GetLength(1)

        For i = 0 To rows - 2
            For j = 0 To cols - 2
                Dim v = (grid(i, j) + grid(i, j + 1) + grid(i + 1, j) + grid(i + 1, j + 1)) / 4
                Dim px0 = ToPixelX(xs(j), xmin, xmax)
                Dim px1 = ToPixelX(xs(j + 1), xmin, xmax)
                Dim py1 = ToPixelY(ys(i), ymin, ymax)
                Dim py0 = ToPixelY(ys(i + 1), ymin, ymax)
                Dim rect = New RectangleF(px0, py1, px1 - px0, py0 - py1)

                Using br As New SolidBrush(ColorScale.GetColor(v, vmin, vmax, ColorMap))
                    _g.FillRectangle(br, rect)
                End Using
            Next
        Next
    End Sub

    ''' <summary>分层填充：从低阈值往上逐层覆盖，天然形成色带</summary>
    Private Sub DrawBands(xmin As Double, xmax As Double, ymin As Double, ymax As Double,
                          vmin As Double, vmax As Double)
        Dim thresholds = Geometry.NiceTicks(vmin, vmax, Levels).Where(Function(t) t > vmin AndAlso t < vmax).ToArray()
        If thresholds.Length = 0 Then Return

        For Each t In thresholds
            Dim color = ColorScale.GetColor(t, vmin, vmax, ColorMap)

            Using br As New SolidBrush(color)
                FillAbove(t, xmin, xmax, ymin, ymax, br)
            End Using
        Next
    End Sub

    Private Sub FillAbove(threshold As Double, xmin As Double, xmax As Double,
                          ymin As Double, ymax As Double, brush As Brush)
        Dim rows = grid.GetLength(0)
        Dim cols = grid.GetLength(1)

        For i = 0 To rows - 2
            For j = 0 To cols - 2
                Dim v00 = grid(i, j)       ' 左上
                Dim v10 = grid(i, j + 1)   ' 右上
                Dim v11 = grid(i + 1, j + 1) ' 右下
                Dim v01 = grid(i + 1, j)   ' 左下

                Dim above = {v00, v10, v11, v01}.Count(Function(v) v >= threshold)

                If above = 4 Then
                    ' 整格都在阈值之上，直接填矩形，避免构造多边形
                    Dim rx0 = ToPixelX(xs(j), xmin, xmax)
                    Dim rx1 = ToPixelX(xs(j + 1), xmin, xmax)
                    Dim ry1 = ToPixelY(ys(i), ymin, ymax)
                    Dim ry0 = ToPixelY(ys(i + 1), ymin, ymax)
                    _g.FillRectangle(brush, rx0, ry1, rx1 - rx0 + 1, ry0 - ry1 + 1)
                ElseIf above > 0 Then
                    Dim poly = CellPolygon(v00, v10, v11, v01, threshold, i, j, xmin, xmax, ymin, ymax)
                    If poly.Length >= 3 Then
                        _g.FillPolygon(brush, poly)
                    End If
                End If
            Next
        Next
    End Sub

    ''' <summary>用 marching squares 求「数值高于阈值」的那部分格子的轮廓多边形</summary>
    Private Function CellPolygon(v00 As Double, v10 As Double, v11 As Double, v01 As Double,
                                 threshold As Double, i As Integer, j As Integer,
                                 xmin As Double, xmax As Double, ymin As Double, ymax As Double) As PointF()
        Dim p00 = New PointF(ToPixelX(xs(j), xmin, xmax), ToPixelY(ys(i), ymin, ymax))
        Dim p10 = New PointF(ToPixelX(xs(j + 1), xmin, xmax), ToPixelY(ys(i), ymin, ymax))
        Dim p11 = New PointF(ToPixelX(xs(j + 1), xmin, xmax), ToPixelY(ys(i + 1), ymin, ymax))
        Dim p01 = New PointF(ToPixelX(xs(j), xmin, xmax), ToPixelY(ys(i + 1), ymin, ymax))

        Dim corners = {v00, v10, v11, v01}
        Dim points = {p00, p10, p11, p01}
        Dim out As New List(Of PointF)()

        For k = 0 To 3
            Dim cur = corners(k)
            Dim nxt = corners((k + 1) Mod 4)

            If cur >= threshold Then out.Add(points(k))

            If (cur >= threshold) <> (nxt >= threshold) Then
                Dim t = (threshold - cur) / (nxt - cur)
                t = std.Min(1, std.Max(0, t))
                out.Add(New PointF(points(k).X + (points((k + 1) Mod 4).X - points(k).X) * t,
                                   points(k).Y + (points((k + 1) Mod 4).Y - points(k).Y) * t))
            End If
        Next

        Return out.ToArray()
    End Function

    ''' <summary>等值线：逐格 marching squares 出线段后按 batches 绘制</summary>
    Private Sub DrawLines(xmin As Double, xmax As Double, ymin As Double, ymax As Double,
                          vmin As Double, vmax As Double, Optional thin As Boolean = False)
        Dim thresholds = Geometry.NiceTicks(vmin, vmax, Levels).Where(Function(t) t > vmin AndAlso t < vmax).ToArray()
        Dim rows = grid.GetLength(0)
        Dim cols = grid.GetLength(1)

        For Each t In thresholds
            Dim scaled As Color

            If Mode = ContourMode.Filled Then
                scaled = If(LineColor, ColorScale.ReadableTextColor(ColorScale.GetColor(t, vmin, vmax, ColorMap)))
            Else
                scaled = If(LineColor, ColorScale.GetColor(t, vmin, vmax, ColorMap))
            End If

            Using pen As New Pen(scaled, If(thin, Theme.GridMinorWidth, Theme.LineWidth))
                For i = 0 To rows - 2
                    For j = 0 To cols - 2
                        Dim v00 = grid(i, j), v10 = grid(i, j + 1), v11 = grid(i + 1, j + 1), v01 = grid(i + 1, j)
                        Dim pts = ContourSegment(v00, v10, v11, v01, t, i, j, xmin, xmax, ymin, ymax)

                        If pts IsNot Nothing Then _g.DrawLine(pen, pts.Value.Item1, pts.Value.Item2)
                    Next
                Next
            End Using
        Next
    End Sub

    ''' <summary>单格内的等值线段（只处理最常见的四种穿越情形，足够画出连续等值线）</summary>
    Private Function ContourSegment(v00 As Double, v10 As Double, v11 As Double, v01 As Double,
                                    threshold As Double, i As Integer, j As Integer,
                                    xmin As Double, xmax As Double, ymin As Double, ymax As Double) As (Item1 As PointF, Item2 As PointF)?
        Dim pTop = Interp(New PointF(ToPixelX(xs(j), xmin, xmax), ToPixelY(ys(i), ymin, ymax)),
                          New PointF(ToPixelX(xs(j + 1), xmin, xmax), ToPixelY(ys(i), ymin, ymax)),
                          v00, v10, threshold)
        Dim pRight = Interp(New PointF(ToPixelX(xs(j + 1), xmin, xmax), ToPixelY(ys(i), ymin, ymax)),
                            New PointF(ToPixelX(xs(j + 1), xmin, xmax), ToPixelY(ys(i + 1), ymin, ymax)),
                            v10, v11, threshold)
        Dim pBottom = Interp(New PointF(ToPixelX(xs(j + 1), xmin, xmax), ToPixelY(ys(i + 1), ymin, ymax)),
                             New PointF(ToPixelX(xs(j), xmin, xmax), ToPixelY(ys(i + 1), ymin, ymax)),
                             v11, v01, threshold)
        Dim pLeft = Interp(New PointF(ToPixelX(xs(j), xmin, xmax), ToPixelY(ys(i + 1), ymin, ymax)),
                           New PointF(ToPixelX(xs(j), xmin, xmax), ToPixelY(ys(i), ymin, ymax)),
                           v01, v00, threshold)

        ' 穿越组合：top-bottom / left-right / 角上的两种
        If pTop IsNot Nothing AndAlso pBottom IsNot Nothing Then Return (pTop.Value, pBottom.Value)
        If pLeft IsNot Nothing AndAlso pRight IsNot Nothing Then Return (pLeft.Value, pRight.Value)
        If pTop IsNot Nothing AndAlso pRight IsNot Nothing Then Return (pTop.Value, pRight.Value)
        If pTop IsNot Nothing AndAlso pLeft IsNot Nothing Then Return (pTop.Value, pLeft.Value)
        If pBottom IsNot Nothing AndAlso pRight IsNot Nothing Then Return (pBottom.Value, pRight.Value)
        If pBottom IsNot Nothing AndAlso pLeft IsNot Nothing Then Return (pBottom.Value, pLeft.Value)

        Return Nothing
    End Function

    Private Function Interp(a As PointF, b As PointF, va As Double, vb As Double, threshold As Double) As PointF?
        If (va >= threshold) = (vb >= threshold) Then Return Nothing
        Dim t = (threshold - va) / (vb - va)
        t = std.Min(1, std.Max(0, t))
        Return New PointF(a.X + (b.X - a.X) * t, a.Y + (b.Y - a.Y) * t)
    End Function
End Class
