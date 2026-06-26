Imports System.Drawing

Imports SolidBrush = Microsoft.VisualBasic.Imaging.SolidBrush
Imports Pen = Microsoft.VisualBasic.Imaging.Pen
Imports DashStyle = Microsoft.VisualBasic.Imaging.DashStyle
Imports LineCap = Microsoft.VisualBasic.Imaging.LineCap
Imports LineJoin = Microsoft.VisualBasic.Imaging.LineJoin
Imports StringAlignment = Microsoft.VisualBasic.Imaging.StringAlignment
Imports StringFormat = Microsoft.VisualBasic.Imaging.StringFormat

' ============================================================================
'  ChartsBasic.vb - 基础图表：散点图 / 折线图 / 柱状图 / 直方图
' ============================================================================

''' <summary>散点图</summary>
Public Class ScatterPlot
    Inherits PlotEngine

    Public Sub New(width As Integer, height As Integer, Optional theme As PlotTheme = Nothing)
        MyBase.New(width, height, theme)
    End Sub

    Public Sub Plot(seriesList As IList(Of Series))
        DrawBackground()
        ComputePlotArea()
        DrawPlotArea()
        DrawTitle()

        ' 计算范围
        Dim allX = seriesList.SelectMany(Function(s) s.X).ToArray()
        Dim allY = seriesList.SelectMany(Function(s) s.Y).ToArray()
        Dim xmin = If(Me.XMin, allX.Min())
        Dim xmax = If(Me.XMax, allX.Max())
        Dim ymin = If(Me.YMin, allY.Min())
        Dim ymax = If(Me.YMax, allY.Max())
        If Me.XMin Is Nothing AndAlso Me.XMax Is Nothing Then
            Dim pad = (xmax - xmin) * 0.05
            If pad = 0 Then pad = 1
            xmin -= pad : xmax += pad
        End If
        If Me.YMin Is Nothing AndAlso Me.YMax Is Nothing Then
            Dim pad = (ymax - ymin) * 0.08
            If pad = 0 Then pad = 1
            ymin -= pad : ymax += pad
        End If

        DrawAxisAndGrid(xmin, xmax, ymin, ymax)

        ' 绘制每个系列
        For i = 0 To seriesList.Count - 1
            Dim s = seriesList(i)
            If Not s.Visible Then Continue For
            Dim color = If(s.Color, Theme.Palette(i Mod Theme.Palette.Length))
            Dim pts = New List(Of PointF)()
            For j = 0 To s.X.Length - 1
                pts.Add(New PointF(ToPixelX(s.X(j), xmin, xmax),
                                   ToPixelY(s.Y(j), ymin, ymax)))
            Next
            ' 连线
            If s.LineStyle <> DashStyle.Custom AndAlso pts.Count > 1 Then
                Using pen As New Pen(color, Theme.LineWidth)
                    pen.DashStyle = s.LineStyle
                    pen.StartCap = LineCap.Round
                    pen.EndCap = LineCap.Round
                    _g.DrawLines(pen, pts.ToArray())
                End Using
            End If
            ' 标记
            If s.MarkerShape <> MarkerShape.None Then
                For Each p In pts
                    DrawMarker(p.X, p.Y, s.MarkerShape, Theme.MarkerSize, color)
                Next
            End If
        Next

        DrawLegend(seriesList)
    End Sub
End Class

''' <summary>折线图（默认无标记，可单独配置）</summary>
Public Class LinePlot
    Inherits PlotEngine

    Public Sub New(width As Integer, height As Integer, Optional theme As PlotTheme = Nothing)
        MyBase.New(width, height, theme)
    End Sub

    Public Sub Plot(seriesList As IList(Of Series))
        ' 折线图默认不显示标记
        For Each s In seriesList
            If s.MarkerShape = MarkerShape.Circle Then s.MarkerShape = MarkerShape.None
        Next
        DrawBackground()
        ComputePlotArea()
        DrawPlotArea()
        DrawTitle()

        Dim allX = seriesList.SelectMany(Function(s) s.X).ToArray()
        Dim allY = seriesList.SelectMany(Function(s) s.Y).ToArray()
        Dim xmin = If(Me.XMin, allX.Min())
        Dim xmax = If(Me.XMax, allX.Max())
        Dim ymin = If(Me.YMin, allY.Min())
        Dim ymax = If(Me.YMax, allY.Max())
        If Me.XMin Is Nothing AndAlso Me.XMax Is Nothing Then
            Dim pad = (xmax - xmin) * 0.05
            If pad = 0 Then pad = 1
            xmin -= pad : xmax += pad
        End If
        If Me.YMin Is Nothing AndAlso Me.YMax Is Nothing Then
            Dim pad = (ymax - ymin) * 0.08
            If pad = 0 Then pad = 1
            ymin -= pad : ymax += pad
        End If

        DrawAxisAndGrid(xmin, xmax, ymin, ymax)

        For i = 0 To seriesList.Count - 1
            Dim s = seriesList(i)
            If Not s.Visible Then Continue For
            Dim color = If(s.Color, Theme.Palette(i Mod Theme.Palette.Length))
            Dim pts = New List(Of PointF)()
            For j = 0 To s.X.Length - 1
                pts.Add(New PointF(ToPixelX(s.X(j), xmin, xmax),
                                   ToPixelY(s.Y(j), ymin, ymax)))
            Next
            If pts.Count > 1 Then
                Using pen As New Pen(color, Theme.LineWidth)
                    pen.DashStyle = s.LineStyle
                    pen.StartCap = LineCap.Round
                    pen.EndCap = LineCap.Round
                    pen.LineJoin = LineJoin.Round
                    _g.DrawLines(pen, pts.ToArray())
                End Using
            End If
            If s.MarkerShape <> MarkerShape.None Then
                For Each p In pts
                    DrawMarker(p.X, p.Y, s.MarkerShape, Theme.MarkerSize, color)
                Next
            End If
        Next

        DrawLegend(seriesList)
    End Sub
End Class

''' <summary>柱状图（分类数据）</summary>
Public Class BarPlot
    Inherits PlotEngine

    Public Property Categories As String() = {}
    Public Property Values As Double() = {}
    Public Property SeriesNames As String() = {}
    ''' <summary>多系列时使用 [系列, 类别] 二维数组</summary>
    Public Property MultiValues As Double(,) = Nothing
    Public Property Horizontal As Boolean = False
    Public Property ShowValueLabels As Boolean = False

    Public Sub New(width As Integer, height As Integer, Optional theme As PlotTheme = Nothing)
        MyBase.New(width, height, theme)
    End Sub

    Public Sub Plot()
        DrawBackground()
        ComputePlotArea()
        DrawPlotArea()
        DrawTitle()

        Dim isMulti = (MultiValues IsNot Nothing)
        Dim nCat = Categories.Length
        Dim nSer = If(isMulti, MultiValues.GetLength(0), 1)

        ' 计算数值范围
        Dim vmin = 0.0, vmax = 1.0
        If isMulti Then
            vmin = 0 : vmax = Double.MinValue
            For i = 0 To nSer - 1
                For j = 0 To nCat - 1
                    If MultiValues(i, j) > vmax Then vmax = MultiValues(i, j)
                Next
            Next
        Else
            vmax = If(Values.Length > 0, Values.Max(), 1)
        End If
        If vmax <= 0 Then vmax = 1
        vmax *= 1.1

        ' 绘制坐标轴（类别轴范围设为 -0.5 ~ nCat-0.5，使刻度对齐柱子中心）
        If Horizontal Then
            ' 横向：X 是数值，Y 是分类
            Dim xTicks = GenerateTicks(vmin, vmax)
            Dim yTicks = Enumerable.Range(0, nCat).Select(Function(i) CDbl(i)).ToArray()
            DrawAxisAndGrid(vmin, vmax, -0.5, nCat - 0.5, xTicks, yTicks, Nothing, Categories)
        Else
            Dim xTicks = Enumerable.Range(0, nCat).Select(Function(i) CDbl(i)).ToArray()
            Dim yTicks = GenerateTicks(vmin, vmax)
            DrawAxisAndGrid(-0.5, nCat - 0.5, vmin, vmax, xTicks, yTicks, Categories, Nothing)
        End If

        ' 绘制柱子
        Dim groupWidth = If(Horizontal, _plotArea.Height / nCat, _plotArea.Width / nCat)
        Dim barWidth = groupWidth * (1 - Theme.BarPadding * 2) / nSer
        For i = 0 To nSer - 1
            Dim color = Theme.Palette(i Mod Theme.Palette.Length)
            For j = 0 To nCat - 1
                Dim val = If(isMulti, MultiValues(i, j), Values(j))
                If Horizontal Then
                    Dim cy = _plotArea.Top + (j + 0.5) * groupWidth
                    Dim bx0 = ToPixelX(0, vmin, vmax)
                    Dim bx1 = ToPixelX(val, vmin, vmax)
                    Dim by = cy - (nSer * barWidth) / 2 + i * barWidth + barWidth * 0.1
                    Dim rect = New RectangleF(Math.Min(bx0, bx1), by - barWidth * 0.45,
                                              Math.Abs(bx1 - bx0), barWidth * 0.9)
                    Using br As New SolidBrush(color)
                        _g.FillRectangle(br, rect)
                    End Using
                    Using pen As New Pen(Theme.BorderColor, 0.5F)
                        _g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height)
                    End Using
                    If ShowValueLabels Then
                        Using br As New SolidBrush(Theme.TextColor),
                              sf As New StringFormat()
                            sf.Alignment = StringAlignment.Near
                            sf.LineAlignment = StringAlignment.Center
                            _g.DrawString(FormatNumber(val), Theme.TickLabelFont, br,
                                          rect.Right + 4, rect.Y + rect.Height / 2)
                        End Using
                    End If
                Else
                    Dim cx = _plotArea.Left + (j + 0.5) * groupWidth
                    Dim by0 = ToPixelY(0, vmin, vmax)
                    Dim by1 = ToPixelY(val, vmin, vmax)
                    Dim bx = cx - (nSer * barWidth) / 2 + i * barWidth + barWidth * 0.1
                    Dim rect = New RectangleF(bx, Math.Min(by0, by1),
                                              barWidth * 0.9, Math.Abs(by1 - by0))
                    Using br As New SolidBrush(color)
                        _g.FillRectangle(br, rect)
                    End Using
                    Using pen As New Pen(Theme.BorderColor, 0.5F)
                        _g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height)
                    End Using
                    If ShowValueLabels Then
                        Using br As New SolidBrush(Theme.TextColor),
                              sf As New StringFormat()
                            sf.Alignment = StringAlignment.Center
                            sf.LineAlignment = StringAlignment.Far
                            _g.DrawString(FormatNumber(val), Theme.TickLabelFont, br,
                                          rect.X + rect.Width / 2, rect.Y - 2)
                        End Using
                    End If
                End If
            Next
        Next

        ' 图例（多系列时）
        If isMulti AndAlso nSer > 1 Then
            Dim seriesList As New List(Of Series)()
            For i = 0 To nSer - 1
                seriesList.Add(New Series With {
                    .Name = If(SeriesNames IsNot Nothing AndAlso i < SeriesNames.Length, SeriesNames(i), "Series " & (i + 1)),
                    .Color = Theme.Palette(i Mod Theme.Palette.Length),
                    .MarkerShape = MarkerShape.Square
                })
            Next
            DrawLegend(seriesList)
        End If
    End Sub
End Class

''' <summary>直方图</summary>
Public Class HistogramPlot
    Inherits PlotEngine

    Public Property Data As Double() = {}
    Public Property Bins As Integer = 30
    Public Property Density As Boolean = False
    Public Property Color As Color? = Nothing
    Public Property ShowRug As Boolean = False

    Public Sub New(width As Integer, height As Integer, Optional theme As PlotTheme = Nothing)
        MyBase.New(width, height, theme)
    End Sub

    Public Sub Plot()
        DrawBackground()
        ComputePlotArea()
        DrawPlotArea()
        DrawTitle()

        Dim dmin = Data.Min()
        Dim dmax = Data.Max()
        If dmax <= dmin Then dmax = dmin + 1
        Dim binW = (dmax - dmin) / Bins
        Dim counts = New Integer(Bins - 1) {}
        For Each v In Data
            Dim idx = CInt(Math.Floor((v - dmin) / binW))
            If idx >= Bins Then idx = Bins - 1
            If idx < 0 Then idx = 0
            counts(idx) += 1
        Next

        Dim xmin = If(Me.XMin, dmin)
        Dim xmax = If(Me.XMax, dmax)
        Dim ymax = If(Density,
                      counts.Max() / (Data.Length * binW) * 1.1,
                      counts.Max() * 1.1)
        Dim ymin = If(Me.YMin, 0)

        DrawAxisAndGrid(xmin, xmax, ymin, ymax)

        Dim color = If(Me.Color, Theme.Palette(0))
        Using br As New SolidBrush(Color.FromArgb(180, color)),
              pen As New Pen(color, Theme.LineWidth)
            For i = 0 To Bins - 1
                Dim x0 = dmin + i * binW
                Dim x1 = x0 + binW
                Dim h = If(Density, counts(i) / (Data.Length * binW), CDbl(counts(i)))
                Dim px0 = ToPixelX(x0, xmin, xmax)
                Dim px1 = ToPixelX(x1, xmin, xmax)
                Dim py0 = ToPixelY(0, ymin, ymax)
                Dim py1 = ToPixelY(h, ymin, ymax)
                Dim rect = New RectangleF(px0, py1, px1 - px0, py0 - py1)
                _g.FillRectangle(br, rect)
                _g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height)
            Next
        End Using

        ' Rug plot
        If ShowRug Then
            Using pen As New Pen(color, 1.0F)
                For Each v In Data
                    Dim px = ToPixelX(v, xmin, xmax)
                    _g.DrawLine(pen, px, _plotArea.Bottom, px, _plotArea.Bottom + 6)
                Next
            End Using
        End If
    End Sub
End Class

