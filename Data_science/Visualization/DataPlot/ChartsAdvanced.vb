Imports System.Drawing

Imports SolidBrush = Microsoft.VisualBasic.Imaging.SolidBrush
Imports Pen = Microsoft.VisualBasic.Imaging.Pen
Imports GraphicsPath = Microsoft.VisualBasic.Imaging.GraphicsPath
Imports DashStyle = Microsoft.VisualBasic.Imaging.DashStyle
Imports LineCap = Microsoft.VisualBasic.Imaging.LineCap
Imports LineJoin = Microsoft.VisualBasic.Imaging.LineJoin


' ============================================================================
'  ChartsAdvanced.vb - 高级图表：盒须图 / 小提琴图 / 饼图 / 热图 / 桑基图
' ============================================================================

''' <summary>盒须图（Box Plot / Box-and-Whisker）</summary>
Public Class BoxPlot
    Inherits PlotEngine

    Public Property Groups As New List(Of BoxGroup)()
    Public Property Horizontal As Boolean = False
    Public Property ShowOutliers As Boolean = True
    Public Property ShowMean As Boolean = True

    Public Sub New(width As Integer, height As Integer, Optional theme As PlotTheme = Nothing)
        MyBase.New(width, height, theme)
    End Sub

    Public Sub Plot()
        DrawBackground()
        ComputePlotArea()
        DrawPlotArea()
        DrawTitle()

        Dim nGrp = Groups.Count
        ' 计算全局 Y 范围
        Dim allData = Groups.SelectMany(Function(g) g.Data).ToList()
        Dim ymin = If(Me.YMin, allData.Min())
        Dim ymax = If(Me.YMax, allData.Max())
        Dim pad = (ymax - ymin) * 0.05
        ymin -= pad : ymax += pad

        Dim xTicks = Enumerable.Range(0, nGrp).Select(Function(i) CDbl(i)).ToArray()
        Dim yTicks = GenerateTicks(ymin, ymax)
        If Horizontal Then
            DrawAxisAndGrid(ymin, ymax, -0.5, nGrp - 0.5, yTicks, xTicks, Nothing, Groups.Select(Function(g) g.Name).ToArray())
        Else
            DrawAxisAndGrid(-0.5, nGrp - 0.5, ymin, ymax, xTicks, yTicks, Groups.Select(Function(g) g.Name).ToArray(), Nothing)
        End If

        Dim groupWidth = If(Horizontal, _plotArea.Height / nGrp, _plotArea.Width / nGrp)
        Dim boxWidth = groupWidth * 0.5

        For i = 0 To nGrp - 1
            Dim g = Groups(i)
            Dim color = If(g.Color, Theme.Palette(i Mod Theme.Palette.Length))
            Dim sorted = g.Data.OrderBy(Function(v) v).ToArray()
            Dim q1 = Quantile(sorted, 0.25)
            Dim q2 = Quantile(sorted, 0.5)
            Dim q3 = Quantile(sorted, 0.75)
            Dim iqr = q3 - q1
            Dim wLo = q1 - 1.5 * iqr
            Dim wHi = q3 + 1.5 * iqr
            Dim outliers = sorted.Where(Function(v) v < wLo OrElse v > wHi).ToList()
            Dim whiskerLo = sorted.Where(Function(v) v >= wLo).Min()
            Dim whiskerHi = sorted.Where(Function(v) v <= wHi).Max()

            Dim center As Single = If(Horizontal,
                            _plotArea.Top + (i + 0.5) * groupWidth,
                            _plotArea.Left + (i + 0.5) * groupWidth)

            If Horizontal Then
                ' 横向：X 是数值
                Dim pxQ1 = ToPixelX(q1, ymin, ymax)
                Dim pxQ2 = ToPixelX(q2, ymin, ymax)
                Dim pxQ3 = ToPixelX(q3, ymin, ymax)
                Dim pxWLo = ToPixelX(whiskerLo, ymin, ymax)
                Dim pxWHi = ToPixelX(whiskerHi, ymin, ymax)
                Dim by0 As Single = center - boxWidth / 2
                Dim by1 As Single = center + boxWidth / 2
                ' 盒子
                Using br As New SolidBrush(Color.FromArgb(180, color)),
                      pen As New Pen(color, Theme.LineWidth)
                    _g.FillRectangle(br, pxQ1, by0, pxQ3 - pxQ1, by1 - by0)
                    _g.DrawRectangle(pen, pxQ1, by0, pxQ3 - pxQ1, by1 - by0)
                    ' 中位数线
                    _g.DrawLine(pen, pxQ2, by0, pxQ2, by1)
                    ' 须
                    _g.DrawLine(pen, pxQ1, center, pxWLo, center)
                    _g.DrawLine(pen, pxQ3, center, pxWHi, center)
                    _g.DrawLine(pen, pxWLo, by0 + 4, pxWLo, by1 - 4)
                    _g.DrawLine(pen, pxWHi, by0 + 4, pxWHi, by1 - 4)
                    ' 均值
                    If ShowMean Then
                        Dim mean = g.Data.Average()
                        Dim pxM = ToPixelX(mean, ymin, ymax)
                        Using dsh As New Pen(color, Theme.LineWidth)
                            dsh.DashStyle = DashStyle.Dot
                            _g.DrawLine(dsh, pxM, CSng(by0), pxM, CSng(by1))
                        End Using
                    End If
                End Using
                ' 异常值
                If ShowOutliers Then
                    For Each o In outliers
                        Dim px = ToPixelX(o, ymin, ymax)
                        DrawMarker(px, center, MarkerShape.Circle, 4, color)
                    Next
                End If
            Else
                ' 纵向：Y 是数值
                Dim pyQ1 = ToPixelY(q1, ymin, ymax)
                Dim pyQ2 = ToPixelY(q2, ymin, ymax)
                Dim pyQ3 = ToPixelY(q3, ymin, ymax)
                Dim pyWLo = ToPixelY(whiskerLo, ymin, ymax)
                Dim pyWHi = ToPixelY(whiskerHi, ymin, ymax)
                Dim bx0 = center - boxWidth / 2
                Dim bx1 = center + boxWidth / 2
                Using br As New SolidBrush(Color.FromArgb(180, color)),
                      pen As New Pen(color, Theme.LineWidth)
                    _g.FillRectangle(br, CSng(bx0), pyQ3, CSng(bx1 - bx0), pyQ1 - pyQ3)
                    _g.DrawRectangle(pen, CSng(bx0), pyQ3, CSng(bx1 - bx0), pyQ1 - pyQ3)
                    _g.DrawLine(pen, CSng(bx0), pyQ2, CSng(bx1), pyQ2)
                    _g.DrawLine(pen, CSng(center), pyQ1, CSng(center), pyWLo)
                    _g.DrawLine(pen, CSng(center), pyQ3, CSng(center), pyWHi)
                    _g.DrawLine(pen, CSng(bx0 + 4), pyWLo, CSng(bx1 - 4), pyWLo)
                    _g.DrawLine(pen, CSng(bx0 + 4), pyWHi, CSng(bx1 - 4), pyWHi)
                    If ShowMean Then
                        Dim mean = g.Data.Average()
                        Dim pyM = ToPixelY(mean, ymin, ymax)
                        Using dsh As New Pen(color, Theme.LineWidth)
                            dsh.DashStyle = DashStyle.Dot
                            _g.DrawLine(dsh, CSng(bx0), pyM, CSng(bx1), pyM)
                        End Using
                    End If
                End Using
                If ShowOutliers Then
                    For Each o In outliers
                        Dim py = ToPixelY(o, ymin, ymax)
                        DrawMarker(center, py, MarkerShape.Circle, 4, color)
                    Next
                End If
            End If
        Next
    End Sub

    Private Shared Function Quantile(sorted As Double(), q As Double) As Double
        If sorted.Length = 0 Then Return 0
        Dim pos = (sorted.Length - 1) * q
        Dim lo = CInt(Math.Floor(pos))
        Dim hi = CInt(Math.Ceiling(pos))
        If lo = hi Then Return sorted(lo)
        Return sorted(lo) + (sorted(hi) - sorted(lo)) * (pos - lo)
    End Function
End Class

''' <summary>盒须图分组</summary>
Public Class BoxGroup
    Public Property Name As String = ""
    Public Property Data As Double() = {}
    Public Property Color As Color? = Nothing
End Class


''' <summary>小提琴图（Violin Plot）</summary>
Public Class ViolinPlot
    Inherits PlotEngine

    Public Property Groups As New List(Of BoxGroup)()
    Public Property ShowBoxInside As Boolean = True

    Public Sub New(width As Integer, height As Integer, Optional theme As PlotTheme = Nothing)
        MyBase.New(width, height, theme)
    End Sub

    Public Sub Plot()
        DrawBackground()
        ComputePlotArea()
        DrawPlotArea()
        DrawTitle()

        Dim nGrp = Groups.Count
        Dim allData = Groups.SelectMany(Function(g) g.Data).ToList()
        Dim ymin = If(Me.YMin, allData.Min())
        Dim ymax = If(Me.YMax, allData.Max())
        Dim pad = (ymax - ymin) * 0.05
        ymin -= pad : ymax += pad

        Dim xTicks = Enumerable.Range(0, nGrp).Select(Function(i) CDbl(i)).ToArray()
        Dim yTicks = GenerateTicks(ymin, ymax)
        DrawAxisAndGrid(-0.5, nGrp - 0.5, ymin, ymax, xTicks, yTicks,
                        Groups.Select(Function(g) g.Name).ToArray(), Nothing)

        Dim groupWidth = _plotArea.Width / nGrp
        Dim maxViolinWidth = groupWidth * 0.4

        For i = 0 To nGrp - 1
            Dim g = Groups(i)
            Dim color = If(g.Color, Theme.Palette(i Mod Theme.Palette.Length))
            Dim sorted = g.Data.OrderBy(Function(v) v).ToArray()
            If sorted.Length < 2 Then Continue For

            ' KDE：用直方图密度估计
            Dim nBins = Math.Min(40, Math.Max(10, sorted.Length \ 5))
            Dim dmin = sorted.Min()
            Dim dmax = sorted.Max()
            If dmax <= dmin Then dmax = dmin + 1
            Dim binW = (dmax - dmin) / nBins
            Dim counts = New Integer(nBins - 1) {}
            For Each v In sorted
                Dim idx = CInt(Math.Floor((v - dmin) / binW))
                If idx >= nBins Then idx = nBins - 1
                If idx < 0 Then idx = 0
                counts(idx) += 1
            Next
            Dim maxCount = counts.Max()

            Dim center As Single = _plotArea.Left + (i + 0.5) * groupWidth

            ' 构造小提琴轮廓（左右对称）
            Dim leftPts As New List(Of PointF)()
            Dim rightPts As New List(Of PointF)()
            For b = 0 To nBins - 1
                Dim y0 = dmin + b * binW
                Dim y1 = y0 + binW
                Dim yMid = (y0 + y1) / 2
                Dim w = counts(b) / maxCount * maxViolinWidth
                Dim py = ToPixelY(yMid, ymin, ymax)
                leftPts.Add(New PointF(center - w, py))
                rightPts.Add(New PointF(center + w, py))
            Next
            ' 闭合路径
            Dim allPts As New List(Of PointF)()
            allPts.AddRange(leftPts)
            rightPts.Reverse()
            allPts.AddRange(rightPts)

            Using br As New SolidBrush(Color.FromArgb(160, color)),
                  pen As New Pen(color, Theme.LineWidth)
                _g.FillPolygon(br, allPts.ToArray())
                _g.DrawPolygon(pen, allPts.ToArray())
            End Using

            ' 内部盒须
            If ShowBoxInside Then
                Dim q1 As Single = Quantile(sorted, 0.25)
                Dim q2 As Single = Quantile(sorted, 0.5)
                Dim q3 As Single = Quantile(sorted, 0.75)
                Dim innerW As Single = maxViolinWidth * 0.15
                Dim pyQ1 = ToPixelY(q1, ymin, ymax)
                Dim pyQ2 = ToPixelY(q2, ymin, ymax)
                Dim pyQ3 = ToPixelY(q3, ymin, ymax)
                Using br As New SolidBrush(Color.FromArgb(220, 30, 30, 30)),
                      pen As New Pen(Color.FromArgb(50, 50, 50), Theme.LineWidth)
                    _g.FillRectangle(br, center - innerW, pyQ3, innerW * 2, pyQ1 - pyQ3)
                    _g.DrawRectangle(pen, center - innerW, pyQ3, innerW * 2, pyQ1 - pyQ3)
                    _g.FillEllipse(br, center - 2, pyQ2 - 2, 4, 4)
                End Using
            End If
        Next
    End Sub

    Private Shared Function Quantile(sorted As Double(), q As Double) As Double
        Dim pos = (sorted.Length - 1) * q
        Dim lo = CInt(Math.Floor(pos))
        Dim hi = CInt(Math.Ceiling(pos))
        If lo = hi Then Return sorted(lo)
        Return sorted(lo) + (sorted(hi) - sorted(lo)) * (pos - lo)
    End Function
End Class


''' <summary>饼图</summary>
Public Class PiePlot
    Inherits PlotEngine

    Public Property Labels As String() = {}
    Public Property Values As Double() = {}
    Public Property Colors As Color() = Nothing
    Public Property Donut As Boolean = False
    Public Property DonutRadius As Single = 0.5F
    Public Property ShowPercentage As Boolean = True
    Public Property ExplodeIndex As Integer = -1
    Public Property StartAngle As Single = -90

    Public Sub New(width As Integer, height As Integer, Optional theme As PlotTheme = Nothing)
        MyBase.New(width, height, theme)
    End Sub

    Public Sub Plot()
        DrawBackground()
        DrawTitle()

        Dim total = Values.Sum()
        Dim n = Values.Length
        Dim palette = If(Colors, Theme.Palette)

        ' 饼图区域
        Dim cx = _width / 2.0F
        Dim cy = (_height + Theme.MarginTop) / 2.0F
        Dim radius = Math.Min(_width, _height) * 0.32F

        Dim startA = StartAngle
        For i = 0 To n - 1
            Dim sweep = CSng(Values(i) / total * 360)
            Dim color = palette(i Mod palette.Length)
            Dim offsetX = 0.0F, offsetY = 0.0F
            If i = ExplodeIndex Then
                Dim midA = (startA + sweep / 2) * Math.PI / 180
                offsetX = CSng(Math.Cos(midA)) * 10
                offsetY = CSng(Math.Sin(midA)) * 10
            End If
            Using br As New SolidBrush(color),
                  pen As New Pen(Theme.BackgroundColor, 2)
                _g.FillPie(br, cx - radius + offsetX, cy - radius + offsetY,
                           radius * 2, radius * 2, startA, sweep)
                _g.DrawPie(pen, cx - radius + offsetX, cy - radius + offsetY,
                           radius * 2, radius * 2, startA, sweep)
            End Using
            startA += sweep
        Next

        ' 环形图中心挖空
        If Donut Then
            Using br As New SolidBrush(Theme.BackgroundColor)
                _g.FillEllipse(br, cx - radius * DonutRadius, cy - radius * DonutRadius,
                               radius * DonutRadius * 2, radius * DonutRadius * 2)
            End Using
        End If

        ' 标签
        startA = StartAngle
        For i = 0 To n - 1
            Dim sweep = CSng(Values(i) / total * 360)
            Dim midA = (startA + sweep / 2) * Math.PI / 180
            Dim labelR = radius * 1.15F
            Dim lx = cx + CSng(Math.Cos(midA)) * labelR
            Dim ly = cy + CSng(Math.Sin(midA)) * labelR
            Dim label = Labels(i)
            If ShowPercentage Then
                label &= String.Format(" ({0:P1})", Values(i) / total)
            End If
            Using br As New SolidBrush(Theme.TextColor),
                  sf As New StringFormat()
                sf.Alignment = StringAlignment.Center
                sf.LineAlignment = StringAlignment.Center
                _g.DrawString(label, Theme.TickLabelFont, br, lx, ly, sf)
            End Using
            startA += sweep
        Next

        ' 中心文字（环形图）
        If Donut Then
            Using br As New SolidBrush(Theme.TitleColor),
                  sf As New StringFormat()
                sf.Alignment = StringAlignment.Center
                sf.LineAlignment = StringAlignment.Center
                _g.DrawString(If(String.IsNullOrEmpty(Title), "", ""),
                              Theme.AxisLabelFont, br, cx, cy, sf)
            End Using
        End If
    End Sub
End Class


''' <summary>热图</summary>
Public Class HeatmapPlot
    Inherits PlotEngine

    Public Property Matrix As Double(,) = Nothing
    Public Property RowLabels As String() = Nothing
    Public Property ColLabels As String() = Nothing
    Public Property ColorMap As ColorMapType = ColorMapType.Viridis
    Public Property ShowValues As Boolean = False
    Public Property MinValue As Double? = Nothing
    Public Property MaxValue As Double? = Nothing

    Public Sub New(width As Integer, height As Integer, Optional theme As PlotTheme = Nothing)
        MyBase.New(width, height, theme)
    End Sub

    Public Sub Plot()
        DrawBackground()
        DrawTitle()

        Dim nRow = Matrix.GetLength(0)
        Dim nCol = Matrix.GetLength(1)

        Dim vmin = If(MinValue, Matrix.Cast(Of Double).Min())
        Dim vmax = If(MaxValue, Matrix.Cast(Of Double).Max())
        If vmax <= vmin Then vmax = vmin + 1

        ' 留出标签空间
        Dim leftPad = If(RowLabels IsNot Nothing, 100, Theme.MarginLeft)
        Dim topPad = If(ColLabels IsNot Nothing, 100, Theme.MarginTop)
        Dim rightPad = 80 ' colorbar
        Dim bottomPad = 50

        Dim plotX = leftPad
        Dim plotY = topPad
        Dim plotW = _width - leftPad - rightPad
        Dim plotH = _height - topPad - bottomPad
        _plotArea = New RectangleF(plotX, plotY, plotW, plotH)

        Dim cellW = plotW / nCol
        Dim cellH = plotH / nRow

        ' 绘制单元格
        For r = 0 To nRow - 1
            For c = 0 To nCol - 1
                Dim v = Matrix(r, c)
                Dim color = GetColor(v, vmin, vmax, ColorMap)
                Dim rect = New RectangleF(plotX + c * cellW, plotY + r * cellH, cellW, cellH)
                Using br As New SolidBrush(color)
                    _g.FillRectangle(br, rect)
                End Using
                If ShowValues Then
                    Dim txtColor = If(Brightness(color) > 0.5, Color.Black, Color.White)
                    Using br As New SolidBrush(txtColor),
                          sf As New StringFormat()
                        sf.Alignment = StringAlignment.Center
                        sf.LineAlignment = StringAlignment.Center
                        _g.DrawString(FormatNumber(v), Theme.TickLabelFont, br,
                                      rect.X + rect.Width / 2, rect.Y + rect.Height / 2, sf)
                    End Using
                End If
            Next
        Next

        ' 边框
        Using pen As New Pen(Theme.BorderColor, 0.5F)
            _g.DrawRectangle(pen, plotX, plotY, plotW, plotH)
        End Using

        ' 行标签
        If RowLabels IsNot Nothing Then
            Using br As New SolidBrush(Theme.TextColor),
                  sf As New StringFormat()
                sf.Alignment = StringAlignment.Far
                sf.LineAlignment = StringAlignment.Center
                For r = 0 To nRow - 1
                    _g.DrawString(RowLabels(r), Theme.TickLabelFont, br,
                                  plotX - 6, plotY + (r + 0.5) * cellH, sf)
                Next
            End Using
        End If

        ' 列标签（旋转 45°）
        If ColLabels IsNot Nothing Then
            Using br As New SolidBrush(Theme.TextColor),
                  sf As New StringFormat()
                sf.Alignment = StringAlignment.Far
                sf.LineAlignment = StringAlignment.Center
                For c = 0 To nCol - 1
                    _g.TranslateTransform(plotX + (c + 0.5) * cellW, plotY - 6)
                    _g.RotateTransform(-45)
                    _g.DrawString(ColLabels(c), Theme.TickLabelFont, br, 0, 0, sf)
                    _g.ResetTransform()
                Next
            End Using
        End If

        ' Colorbar
        Dim cbX = plotX + plotW + 20
        Dim cbY = plotY
        Dim cbW = 16
        Dim cbH = plotH
        For i = 0 To CInt(cbH) - 1
            Dim t = 1 - i / cbH
            Dim v = vmin + t * (vmax - vmin)
            Dim color = GetColor(v, vmin, vmax, ColorMap)
            Using br As New SolidBrush(color)
                _g.FillRectangle(br, cbX, cbY + i, cbW, 1)
            End Using
        Next
        Using pen As New Pen(Theme.BorderColor, 0.7F)
            _g.DrawRectangle(pen, cbX, cbY, cbW, cbH)
        End Using
        ' colorbar 刻度
        Dim ticks = GenerateTicks(vmin, vmax, 5)
        Using br As New SolidBrush(Theme.TextColor),
              pen As New Pen(Theme.AxisColor, 0.7F),
              sf As New StringFormat()
            sf.Alignment = StringAlignment.Near
            sf.LineAlignment = StringAlignment.Center
            For Each t In ticks
                If t < vmin OrElse t > vmax Then Continue For
                Dim py As Single = cbY + (1 - (t - vmin) / (vmax - vmin)) * cbH
                _g.DrawLine(pen, cbX + cbW, py, cbX + cbW + 4, py)
                _g.DrawString(FormatNumber(t), Theme.TickLabelFont, br, cbX + cbW + 6, py, sf)
            Next
        End Using
    End Sub

    Public Enum ColorMapType
        Viridis
        Plasma
        Inferno
        CoolWarm
        Grayscale
        Jet
    End Enum

    Private Function GetColor(v As Double, vmin As Double, vmax As Double, cmap As ColorMapType) As Color
        Dim t = (v - vmin) / (vmax - vmin)
        t = Math.Max(0, Math.Min(1, t))
        Select Case cmap
            Case ColorMapType.Viridis
                Return LerpPalette(t, {
                    Color.FromArgb(68, 1, 84), Color.FromArgb(59, 82, 139),
                    Color.FromArgb(33, 145, 140), Color.FromArgb(94, 201, 98),
                    Color.FromArgb(253, 231, 37)})
            Case ColorMapType.Plasma
                Return LerpPalette(t, {
                    Color.FromArgb(13, 8, 135), Color.FromArgb(126, 3, 168),
                    Color.FromArgb(204, 71, 120), Color.FromArgb(248, 149, 64),
                    Color.FromArgb(240, 249, 33)})
            Case ColorMapType.Inferno
                Return LerpPalette(t, {
                    Color.FromArgb(0, 0, 4), Color.FromArgb(87, 16, 110),
                    Color.FromArgb(187, 55, 84), Color.FromArgb(249, 142, 9),
                    Color.FromArgb(252, 255, 164)})
            Case ColorMapType.CoolWarm
                Return LerpPalette(t, {
                    Color.FromArgb(59, 76, 192), Color.FromArgb(221, 221, 221),
                    Color.FromArgb(180, 4, 38)})
            Case ColorMapType.Grayscale
                Dim g = CInt(t * 255)
                Return Color.FromArgb(g, g, g)
            Case ColorMapType.Jet
                Return LerpPalette(t, {
                    Color.FromArgb(0, 0, 131), Color.FromArgb(0, 60, 170),
                    Color.FromArgb(5, 255, 255), Color.FromArgb(255, 255, 0),
                    Color.FromArgb(250, 0, 0), Color.FromArgb(128, 0, 0)})
        End Select
        Return Color.White
    End Function

    Private Function LerpPalette(t As Double, colors As Color()) As Color
        Dim n = colors.Length - 1
        Dim pos = t * n
        Dim i = CInt(Math.Floor(pos))
        If i < 0 Then i = 0
        If i >= n Then i = n - 1
        Dim f = pos - i
        Dim c1 = colors(i), c2 = colors(i + 1)
        Return Color.FromArgb(
            CInt(c1.R + (c2.R - c1.R) * f),
            CInt(c1.G + (c2.G - c1.G) * f),
            CInt(c1.B + (c2.B - c1.B) * f))
    End Function

    Private Function Brightness(c As Color) As Double
        Return (0.299 * c.R + 0.587 * c.G + 0.114 * c.B) / 255
    End Function
End Class


''' <summary>桑基图（Sankey Diagram）</summary>
Public Class SankeyPlot
    Inherits PlotEngine

    Public Property Nodes As New List(Of SankeyNode)()
    Public Property Links As New List(Of SankeyLink)()

    Public Sub New(width As Integer, height As Integer, Optional theme As PlotTheme = Nothing)
        MyBase.New(width, height, theme)
    End Sub

    Public Sub Plot()
        DrawBackground()
        DrawTitle()

        ' 计算每个节点的总入流 / 出流
        For Each n In Nodes
            n.InFlow = 0 : n.OutFlow = 0
        Next
        For Each l In Links
            Dim src = Nodes.FirstOrDefault(Function(n) n.Id = l.Source)
            Dim dst = Nodes.FirstOrDefault(Function(n) n.Id = l.Target)
            If src IsNot Nothing Then src.OutFlow += l.Value
            If dst IsNot Nothing Then dst.InFlow += l.Value
        Next
        For Each n In Nodes
            n.Total = Math.Max(n.InFlow, n.OutFlow)
        Next

        ' 简单布局：按 Layer 分列
        Dim maxLayer = If(Nodes.Count > 0, Nodes.Max(Function(n) n.Layer), 0)
        Dim nLayer = maxLayer + 1
        Dim colW = (_width - Theme.MarginLeft - Theme.MarginRight) / Math.Max(1, nLayer - 1)
        Dim nodeW = 14.0F
        Dim totalValue = Nodes.Sum(Function(n) n.Total)
        Dim availH = _height - Theme.MarginTop - Theme.MarginBottom - 20

        ' 按层分组并垂直布局
        Dim layerNodes As New Dictionary(Of Integer, List(Of SankeyNode))()
        For Each n In Nodes
            If Not layerNodes.ContainsKey(n.Layer) Then layerNodes(n.Layer) = New List(Of SankeyNode)()
            layerNodes(n.Layer).Add(n)
        Next

        For Each kv In layerNodes
            Dim layerTotal = kv.Value.Sum(Function(n) n.Total)
            Dim y = Theme.MarginTop + 10.0F
            For Each n In kv.Value
                n.X = Theme.MarginLeft + kv.Key * colW
                n.Y = y
                n.Height = CSng(n.Total / layerTotal * availH)
                y += n.Height + 6
            Next
        Next

        ' 绘制连接（贝塞尔曲线）
        For Each l In Links
            Dim src = Nodes.FirstOrDefault(Function(n) n.Id = l.Source)
            Dim dst = Nodes.FirstOrDefault(Function(n) n.Id = l.Target)
            If src Is Nothing OrElse dst Is Nothing Then Continue For

            ' 计算源节点和目标节点上的偏移（按连接顺序累加）
            If Not src.OutOffsets.ContainsKey(l.Target) Then src.OutOffsets(l.Target) = 0
            If Not dst.InOffsets.ContainsKey(l.Source) Then dst.InOffsets(l.Source) = 0
            Dim srcY = src.Y + src.OutOffsets(l.Target)
            Dim dstY = dst.Y + dst.InOffsets(l.Source)
            Dim linkH = CSng(l.Value / Math.Max(src.Total, 1) * src.Height)
            Dim linkH2 = CSng(l.Value / Math.Max(dst.Total, 1) * dst.Height)
            src.OutOffsets(l.Target) += linkH
            dst.InOffsets(l.Source) += linkH2

            Dim x1 = src.X + nodeW
            Dim x2 = dst.X
            Dim cx = (x1 + x2) / 2
            Dim path As New GraphicsPath()
            path.AddBezier(
                New PointF(x1, srcY + linkH / 2),
                New PointF(cx, srcY + linkH / 2),
                New PointF(cx, dstY + linkH2 / 2),
                New PointF(x2, dstY + linkH2 / 2))
            path.AddBezier(
                New PointF(x2, dstY + linkH2 / 2),
                New PointF(cx, dstY + linkH2 / 2),
                New PointF(cx, srcY + linkH / 2),
                New PointF(x1, srcY + linkH / 2))
            path.CloseFigure()

            Dim color As Color = If(l.Color, Color.FromArgb(80, Theme.Palette(0)))
            Using br As New SolidBrush(color)
                _g.FillPath(br, path)
            End Using
        Next

        ' 绘制节点
        For Each n In Nodes
            Dim color = If(n.Color, Theme.Palette(n.Layer Mod Theme.Palette.Length))
            Using br As New SolidBrush(color),
                  pen As New Pen(Theme.BorderColor, 0.5F)
                _g.FillRectangle(br, n.X, n.Y, nodeW, n.Height)
                _g.DrawRectangle(pen, n.X, n.Y, nodeW, n.Height)
            End Using
            ' 标签
            Using br As New SolidBrush(Theme.TextColor),
                  sf As New StringFormat()
                sf.LineAlignment = StringAlignment.Center
                If n.Layer = 0 Then
                    sf.Alignment = StringAlignment.Far
                    _g.DrawString(n.Label, Theme.TickLabelFont, br, n.X - 6, n.Y + n.Height / 2, sf)
                Else
                    sf.Alignment = StringAlignment.Near
                    _g.DrawString(n.Label, Theme.TickLabelFont, br, n.X + nodeW + 6, n.Y + n.Height / 2, sf)
                End If
            End Using
        Next
    End Sub
End Class

Public Class SankeyNode
    Public Property Id As String
    Public Property Label As String
    Public Property Layer As Integer
    Public Property Color As Color? = Nothing
    ' 运行时
    Public Property InFlow As Double
    Public Property OutFlow As Double
    Public Property Total As Double
    Public Property X As Single
    Public Property Y As Single
    Public Property Height As Single
    Public Property OutOffsets As New Dictionary(Of String, Double)()
    Public Property InOffsets As New Dictionary(Of String, Double)()
End Class

Public Class SankeyLink
    Public Property Source As String
    Public Property Target As String
    Public Property Value As Double
    Public Property Color As Color? = Nothing
End Class


