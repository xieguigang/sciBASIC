#Region "Microsoft.VisualBasic::7a3e9c1b5d2f4086ae1b7c9d0e3f5a2b, Data_science\Visualization\DataPlot\Basic\HistogramPlot.vb"

    ' 
    '       sciBASIC.NET Foundation, GPL3 Licensed
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.

    ' Class HistogramPlot
    ' 
    '     Properties: Bins, BinEdges, Color, Data, Density, Groups
    '                 Mirrored, ShowDensityCurve, ShowLegendImpl, ShowRug
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Sub: Plot
    ' 
#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports std = System.Math

' ============================================================================
'  HistogramPlot.vb - 直方图
'
'  除了单样本直方图，还提供：
'   · 多组叠加 / 并列（Groups）
'   · 双向镜像直方图（Mirrored = True，用于双直方图对比两个分布）
'   · 显式分箱边界（BinEdges，对应旧实现的 DataBinBox 输入）
'   · 核密度曲线叠加（ShowDensityCurve）
' ============================================================================

''' <summary>直方图</summary>
Public Class HistogramPlot
    Inherits PlotEngine

    ''' <summary>单样本数据（Groups 为空时使用）</summary>
    Public Property Data As Double() = {}
    ''' <summary>多组数据；给了值时按组分别统计</summary>
    Public Property Groups As List(Of CategoryGroup) = Nothing
    ''' <summary>分箱数量（未提供 BinEdges 时生效）</summary>
    Public Property Bins As Integer = 30
    ''' <summary>显式分箱边界；给了值时不自动推算装箱区间</summary>
    Public Property BinEdges As Double() = Nothing
    ''' <summary>纵轴转换为密度（面积积分为 1）</summary>
    Public Property Density As Boolean = False
    ''' <summary>单样本时的柱子颜色</summary>
    Public Property Color As Color? = Nothing
    ''' <summary>在底边画出每个样本的短须</summary>
    Public Property ShowRug As Boolean = False
    ''' <summary>叠加核密度估计曲线</summary>
    Public Property ShowDensityCurve As Boolean = False
    ''' <summary>多组时是否两两镜像（双向直方图）</summary>
    Public Property Mirrored As Boolean = False
    ''' <summary>柱子透明度（多组并列时便于观察重叠）</summary>
    Public Property FillAlpha As Integer = 180

    Public Sub New(width As Integer, height As Integer, Optional theme As PlotTheme = Nothing)
        MyBase.New(width, height, theme)
    End Sub

    Public Sub Plot()
        Dim sources = ResolveSources()

        If sources.Count = 0 OrElse sources.All(Function(g) g.Data Is Nothing OrElse g.Data.Length = 0) Then
            Throw New InvalidOperationException("HistogramPlot requires at least one non-empty sample.")
        End If

        Dim edges = ResolveEdges(sources)
        Dim counts = sources.Select(Function(g) CountBins(g.Data, edges)).ToArray()

        DrawBackground()
        ComputePlotArea()
        DrawPlotArea()
        DrawTitle()

        Dim xmin = If(Me.XMin, edges(0))
        Dim xmax = If(Me.XMax, edges(edges.Length - 1))
        Dim binW = (edges(edges.Length - 1) - edges(0)) / (edges.Length - 1)

        Dim maxCount As Double = 0
        For Each c In counts
            Dim maxHere = DensityOr(c, sources(0).Data.Length, binW).Max()
            If maxHere > maxCount Then maxCount = maxHere
        Next

        Dim ymin As Double = 0
        Dim ymax As Double = maxCount * 1.1
        If Mirrored AndAlso counts.Length >= 2 Then
            Dim maxMirror = std.Max(DensityOr(counts(0), sources(0).Data.Length, binW).Max(),
                                    DensityOr(counts(1), sources(1).Data.Length, binW).Max())
            ymin = -maxMirror * 1.1
            ymax = maxMirror * 1.1
            If Me.YMin IsNot Nothing Then ymin = Me.YMin.Value
            If Me.YMax IsNot Nothing Then ymax = Me.YMax.Value
        End If

        If Me.YMin IsNot Nothing AndAlso Not Mirrored Then ymin = Me.YMin.Value
        If Me.YMax IsNot Nothing AndAlso Not Mirrored Then ymax = Me.YMax.Value

        DrawAxisAndGrid(xmin, xmax, ymin, ymax)

        Dim nGroup = counts.Length

        For gi = 0 To nGroup - 1
            Dim heights = DensityOr(counts(gi), sources(gi).Data.Length, binW)
            Dim color As Color = If(sources(gi).Color, If(Me.Color, Theme.Palette(gi Mod Theme.Palette.Length)))
            Dim mirroredDown = Mirrored AndAlso (gi Mod 2 = 1)

            Using br As New SolidBrush(Color.FromArgb(FillAlpha, color)),
                  pen As New Pen(color, Theme.LineWidth)
                For i = 0 To edges.Length - 2
                    Dim h As Double = heights(i)
                    If mirroredDown Then h = -h

                    Dim px0 = ToPixelX(edges(i), xmin, xmax)
                    Dim px1 = ToPixelX(edges(i + 1), xmin, xmax)
                    Dim py0 = ToPixelY(0, ymin, ymax)
                    Dim py1 = ToPixelY(h, ymin, ymax)
                    Dim rect = New RectangleF(px0, std.Min(py0, py1), px1 - px0, std.Abs(py1 - py0))

                    _g.FillRectangle(br, rect)
                    _g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height)
                Next
            End Using

            If ShowDensityCurve Then
                DrawDensityCurve(sources(gi).Data, xmin, xmax, ymin, ymax, sources(gi).Data.Length, binW, color)
            End If

            If ShowRug Then
                Using pen As New Pen(color, 1.0F)
                    For Each v In sources(gi).Data
                        Dim px = ToPixelX(v, xmin, xmax)
                        _g.DrawLine(pen, px, _plotArea.Bottom, px, _plotArea.Bottom + 6)
                    Next
                End Using
            End If
        Next

        If sources.Count > 1 Then
            Dim legends As New List(Of Series)()
            For i = 0 To sources.Count - 1
                legends.Add(New Series With {
                    .Name = sources(i).Name,
                    .Color = If(sources(i).Color, Theme.Palette(i Mod Theme.Palette.Length)),
                    .MarkerShape = MarkerShape.Square
                })
            Next
            DrawLegend(legends)
        End If
    End Sub

    ' ---------------- 内部实现 ----------------

    Private Function ResolveSources() As List(Of CategoryGroup)
        If Groups IsNot Nothing AndAlso Groups.Count > 0 Then
            Return Groups.Where(Function(g) g.Data IsNot Nothing AndAlso g.Data.Length > 0).ToList()
        End If
        Return New List(Of CategoryGroup) From {
            New CategoryGroup With {.Name = "data", .Data = Data, .Color = Me.Color}
        }
    End Function

    Private Function ResolveEdges(sources As List(Of CategoryGroup)) As Double()
        If BinEdges IsNot Nothing AndAlso BinEdges.Length >= 2 Then Return BinEdges

        Dim all = sources.SelectMany(Function(g) g.Data).ToArray()
        Dim dmin = all.Min(), dmax = all.Max()
        If dmax <= dmin Then dmax = dmin + 1

        Geometry.ExpandRange(dmin, dmax, 0)

        Dim edges(Bins) As Double
        For i = 0 To Bins
            edges(i) = dmin + (dmax - dmin) * i / Bins
        Next

        Return edges
    End Function

    Private Function CountBins(data As Double(), edges As Double()) As Double()
        Dim n = edges.Length - 1
        Dim counts = New Double(n - 1) {}

        For Each v In data
            Dim lo = 0, hi = n - 1, hit = -1

            ' 二分定位所属箱，避免逐箱线性扫描
            While lo <= hi
                Dim mid = (lo + hi) \ 2
                If v < edges(mid) Then
                    hi = mid - 1
                ElseIf v >= edges(mid + 1) Then
                    lo = mid + 1
                Else
                    hit = mid
                    Exit While
                End If
            End While

            If hit < 0 Then
                If std.Abs(v - edges(n)) < 0.000000001 Then hit = n - 1
            End If
            If hit >= 0 Then counts(hit) += 1
        Next

        Return counts
    End Function

    Private Function DensityOr(counts As Double(), n As Integer, binW As Double) As Double()
        If Not Density Then Return counts

        Dim scale = If(n > 0, n * binW, 1)
        If scale = 0 Then scale = 1
        Return counts.Select(Function(c) c / scale).ToArray()
    End Function

    Private Sub DrawDensityCurve(data As Double(), xmin As Double, xmax As Double,
                                 ymin As Double, ymax As Double, n As Integer, binW As Double, color As Color)
        Dim d = Geometry.Density1D(data, 256)
        If d.x Is Nothing OrElse d.x.Length = 0 Then Return

        Dim scale = If(Density, 1.0, If(n > 0, n * binW, 1.0))
        Dim pts As New List(Of PointF)()

        For i = 0 To d.x.Length - 1
            If d.x(i) < xmin OrElse d.x(i) > xmax Then Continue For
            pts.Add(New PointF(ToPixelX(d.x(i), xmin, xmax), ToPixelY(d.y(i) * scale, ymin, ymax)))
        Next

        If pts.Count > 1 Then
            Using pen As New Pen(color, Theme.LineWidth * 1.4F)
                _g.DrawLines(pen, pts.ToArray())
            End Using
        End If
    End Sub
End Class
