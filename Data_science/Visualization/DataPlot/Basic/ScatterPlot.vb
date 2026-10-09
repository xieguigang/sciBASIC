#Region "Microsoft.VisualBasic::fd25df1f7685cc2f67ae020ddbe85a5f, Data_science\Visualization\DataPlot\Basic\ScatterPlot.vb"

' 
'       sciBASIC.NET Foundation, GPL3 Licensed
' 
' This program is free software: you can redistribute it and/or modify
' it under the terms of the GNU General Public License as published by
' the Free Software Foundation, either version 3 of the License, or
' (at your option) any later version.

' Class ScatterPlot
' 
'     Properties: BubbleMaxSize, BubbleMinSize, AbLines, ShowErrorBars
'                 HullAlpha, Jitter, JitterAmount, ShowConvexHull
'                 Smooth, SmoothSamplesPerSegment
' 
'     Constructor: (+3 Overloads) Sub New
'     Sub: Plot
' 
#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports std = System.Math

' ============================================================================
'  ScatterPlot.vb - 散点图（同时也是折线、气泡、误差棒、样条数据的统一入口）
'
'  散点家族在旧实现里有 Scatter2D / Bubble / LinePlot / PolygonPlot 等多份代码，
'  它们之间的差别其实只是「要不要连线、点要不要按第三维放大、要不要画误差棒、要不要平滑」。
'  这里把这些差别收敛为一组开关，避免同一个渲染管线被复制四份。
'
'  用法：
'   · 气泡图     —— 给 Series.Size 填值即可，半径自动映射到 [BubbleMinSize, BubbleMaxSize]
'   · 误差棒     —— 给 Series.ErrorMinus / ErrorPlus 填值（ShowErrorBars = True 时自动绘制）
'   · 平滑曲线   —— Smooth = True
'   · 凸包轮廓   —— ShowConvexHull = True
'   · 抖动       —— Jitter = True（离散取值重叠时打散）
' ============================================================================

''' <summary>
''' 散点图
''' </summary>
Public Class ScatterPlot : Inherits SeriesPlotEngine

    ''' <summary>是否绘制误差棒（Series 提供 ErrorMinus/ErrorPlus 时生效）</summary>
    Public Property ShowErrorBars As Boolean = True
    ''' <summary>气泡的最小像素直径</summary>
    Public Property BubbleMinSize As Single = 3.0F
    ''' <summary>气泡的最大像素直径</summary>
    Public Property BubbleMaxSize As Single = 26.0F
    ''' <summary>是否用 Catmull-Rom 样条把折线平滑成曲线</summary>
    Public Property Smooth As Boolean = False
    ''' <summary>样条每段的插值点数（越大越光滑，开销也越大）</summary>
    Public Property SmoothSamplesPerSegment As Integer = 12
    ''' <summary>是否绘制每个系列的凸包填充轮廓</summary>
    Public Property ShowConvexHull As Boolean = False
    ''' <summary>凸包填充的透明度（0~255）</summary>
    Public Property HullAlpha As Integer = 60
    ''' <summary>是否对 X/Y 施加抖动</summary>
    Public Property Jitter As Boolean = False
    ''' <summary>抖动幅度（相对值域比例），小于等于 0 时取主题设定</summary>
    Public Property JitterAmount As Double = 0
    ''' <summary>需要叠加的参考直线，元素为 (斜率 b, 截距 a)，即 y = a + b*x</summary>
    Public Property AbLines As New List(Of (b As Double, a As Double))()

    Public Sub New(width As Integer, height As Integer, Optional theme As PlotTheme = Nothing, Optional driver As Drivers = Drivers.Default)
        MyBase.New(width, height, theme, driver)
    End Sub

    ''' <summary>直接在已有的位图上绘制（用于宿主程序 PictureBox 等）。</summary>
    Public Sub New(bmp As Bitmap)
        MyBase.New(bmp)
    End Sub

    ''' <summary>直接在外部提供的绘图设备上绘制（例如 DirectX 的 GPU 画布）。</summary>
    Public Sub New(g As IGraphics, Optional theme As PlotTheme = Nothing)
        MyBase.New(g, theme)
    End Sub

    Public Overloads Shared Function Plot(seriesList As IList(Of Series), size As String) As GraphicsData
        Dim sz As Size = size.SizeParser

        Using scatter As New ScatterPlot(sz.Width, sz.Height)
            Call scatter.Plot(seriesList)
            Throw New NotImplementedException
        End Using
    End Function

    Public Overrides Sub Plot(seriesList As IList(Of Series))
        If seriesList Is Nothing OrElse seriesList.Count = 0 Then
            Throw New ArgumentException("At least one data series is required.", NameOf(seriesList))
        End If

        ' 抖动在进入坐标计算之前完成，保证后续图形Entity完全一致
        Dim visible = ApplyJitter(seriesList)

        DrawBackground()
        ComputePlotArea()
        DrawPlotArea()
        DrawTitle()

        Dim xmin As Double, xmax As Double, ymin As Double, ymax As Double
        ComputeRange(visible, xmin, xmax, ymin, ymax)

        DrawAxisAndGrid(xmin, xmax, ymin, ymax)
        DrawAbLines(xmin, xmax, ymin, ymax)

        ' 气泡尺寸需要一个全局值域，否则各系列的圆会不可比
        Dim sizeMin As Double = Double.NaN, sizeMax As Double = Double.NaN
        For Each s In visible
            If s.Size Is Nothing Then Continue For
            For Each v In s.Size
                If Double.IsNaN(sizeMin) OrElse v < sizeMin Then sizeMin = v
                If Double.IsNaN(sizeMax) OrElse v > sizeMax Then sizeMax = v
            Next
        Next

        For i = 0 To visible.Count - 1
            Dim s = visible(i)
            Dim color = If(s.Color, Theme.Palette(i Mod Theme.Palette.Length))
            Dim pts = Project(s, xmin, xmax, ymin, ymax)

            If pts.Length = 0 Then Continue For

            If ShowConvexHull AndAlso pts.Length >= 3 Then
                DrawHull(pts, color)
            End If

            Dim draw As IEnumerable(Of PointF) = pts
            If Smooth AndAlso pts.Length >= 3 Then
                draw = Geometry.SmoothSpline(pts, SmoothSamplesPerSegment)
            End If

            ' 连线
            If s.LineStyle <> DashStyle.Custom AndAlso pts.Count > 1 Then
                Using pen As New Pen(color, Theme.LineWidth)
                    pen.DashStyle = s.LineStyle
                    pen.StartCap = LineCap.Round
                    pen.EndCap = LineCap.Round
                    _g.DrawLines(pen, draw.ToArray())
                End Using
            End If

            ' 误差棒
            If ShowErrorBars AndAlso s.ErrorMinus IsNot Nothing AndAlso s.ErrorPlus IsNot Nothing Then
                DrawErrorBars(s, xmin, xmax, ymin, ymax, color)
            End If

            ' 标记 / 气泡
            If s.MarkerShape <> MarkerShape.None Then
                For j = 0 To pts.Length - 1
                    Dim size As Single = If(s.PointSize > 0, s.PointSize, Theme.MarkerSize)
                    If s.Size IsNot Nothing AndAlso j < s.Size.Length Then
                        size = Geometry.Remap(s.Size(j), sizeMin, sizeMax, BubbleMinSize, BubbleMaxSize)
                    End If
                    DrawMarker(pts(j).X, pts(j).Y, s.MarkerShape, size, color)
                Next
            End If
        Next

        DrawLegend(visible)
    End Sub

    ' ---------------- 内部实现 ----------------

    Private Function ApplyJitter(seriesList As IList(Of Series)) As List(Of Series)
        Dim result = seriesList.Where(Function(s) s.Visible).ToList()

        If Not Jitter Then Return result

        Dim amount = If(JitterAmount > 0, JitterAmount, Theme.JitterAmount)
        Dim allX = result.SelectMany(Function(s) s.X).ToArray()
        Dim allY = result.SelectMany(Function(s) s.Y).ToArray()
        Dim spread = std.Max(allX.Max() - allX.Min(), allY.Max() - allY.Min())
        If spread <= 0 Then spread = 1

        Return result.Select(Function(s) New Series With {
            .Color = s.Color,
            .ErrorMinus = s.ErrorMinus,
            .ErrorPlus = s.ErrorPlus,
            .FillAlpha = s.FillAlpha,
            .FillColor = s.FillColor,
            .LineStyle = s.LineStyle,
            .MarkerShape = s.MarkerShape,
            .Name = s.Name,
            .PointLabels = s.PointLabels,
            .PointSize = s.PointSize,
            .Size = s.Size,
            .Visible = True,
            .X = Geometry.MakeJitter(s.X, amount),
            .Y = s.Y
        }).ToList()
    End Function

    Private Sub ComputeRange(visible As List(Of Series), ByRef xmin As Double, ByRef xmax As Double,
                             ByRef ymin As Double, ByRef ymax As Double)
        Dim allX = visible.SelectMany(Function(s) s.X).ToArray()
        Dim allY = visible.SelectMany(Function(s) s.Y).ToArray()

        xmin = If(Me.XMin, allX.Min())
        xmax = If(Me.XMax, allX.Max())
        ymin = If(Me.YMin, allY.Min())
        ymax = If(Me.YMax, allY.Max())

        ' 误差棒不应被裁掉
        If ShowErrorBars Then
            For Each s In visible
                If s.ErrorMinus IsNot Nothing Then
                    For j = 0 To s.Y.Length - 1
                        If j < s.ErrorMinus.Length AndAlso s.Y(j) - s.ErrorMinus(j) < ymin Then
                            ymin = s.Y(j) - s.ErrorMinus(j)
                        End If
                    Next
                End If
                If s.ErrorPlus IsNot Nothing Then
                    For j = 0 To s.Y.Length - 1
                        If j < s.ErrorPlus.Length AndAlso s.Y(j) + s.ErrorPlus(j) > ymax Then
                            ymax = s.Y(j) + s.ErrorPlus(j)
                        End If
                    Next
                End If
            Next
        End If

        Geometry.ExpandRange(xmin, xmax, 0.05)
        Geometry.ExpandRange(ymin, ymax, 0.08)
    End Sub

    Private Function Project(s As Series, xmin As Double, xmax As Double,
                             ymin As Double, ymax As Double) As PointF()
        Dim n = std.Min(s.X.Length, s.Y.Length)
        Dim pts = New List(Of PointF)(n)

        For j = 0 To n - 1
            If Double.IsNaN(s.X(j)) OrElse Double.IsNaN(s.Y(j)) Then Continue For
            pts.Add(New PointF(ToPixelX(s.X(j), xmin, xmax), ToPixelY(s.Y(j), ymin, ymax)))
        Next

        Return pts.ToArray()
    End Function

    Private Sub DrawErrorBars(s As Series, xmin As Double, xmax As Double,
                              ymin As Double, ymax As Double, color As Color)
        Dim cap As Single = 3.0F

        Using pen As New Pen(color, Theme.LineWidth * 0.8F)
            For j = 0 To s.Y.Length - 1
                If j >= s.ErrorMinus.Length OrElse j >= s.ErrorPlus.Length Then Exit For
                If Double.IsNaN(s.Y(j)) Then Continue For

                Dim px = ToPixelX(s.X(j), xmin, xmax)
                Dim pUp = ToPixelY(s.Y(j) + s.ErrorPlus(j), ymin, ymax)
                Dim pDown = ToPixelY(s.Y(j) - s.ErrorMinus(j), ymin, ymax)

                _g.DrawLine(pen, px, pUp, px, pDown)
                _g.DrawLine(pen, px - cap, pUp, px + cap, pUp)
                _g.DrawLine(pen, px - cap, pDown, px + cap, pDown)
            Next
        End Using
    End Sub

    Private Sub DrawHull(pts As PointF(), color As Color)
        Dim hull = Geometry.ConvexHull(pts)
        If hull.Length < 3 Then Return

        Using br As New SolidBrush(Color.FromArgb(HullAlpha, color))
            _g.FillPolygon(br, hull)
        End Using
        Using pen As New Pen(color, Theme.LineWidth)
            _g.DrawPolygon(pen, hull)
        End Using
    End Sub

    Private Sub DrawAbLines(xmin As Double, xmax As Double, ymin As Double, ymax As Double)
        If AbLines Is Nothing OrElse AbLines.Count = 0 Then Return

        Using pen As New Pen(Theme.AxisColor, Theme.LineWidth)
            pen.DashStyle = DashStyle.Dash
            For Each line In AbLines
                DrawAbline(line.a, line.b, xmin, xmax, ymin, ymax, pen)
            Next
        End Using
    End Sub
End Class
