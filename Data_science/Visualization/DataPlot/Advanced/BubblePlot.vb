Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging

''' <summary>气泡图数据系列（X/Y 定位，Sizes 映射第三维）</summary>
Public Class BubbleSeries
    Inherits Series
    ''' <summary>气泡大小（第三维数据）</summary>
    Public Property Sizes As Double() = {}
End Class

''' <summary>气泡图（三维散点，气泡半径映射第三维）</summary>
Public Class BubblePlot
    Inherits PlotEngine

    ''' <summary>最小气泡半径（像素）</summary>
    Public Property MinBubbleSize As Single = 4.0F
    ''' <summary>最大气泡半径（像素）</summary>
    Public Property MaxBubbleSize As Single = 30.0F
    ''' <summary>填充透明度（0-255）</summary>
    Public Property FillAlpha As Integer = 140
    ''' <summary>显示气泡大小图例</summary>
    Public Property ShowSizeLegend As Boolean = True
    ''' <summary>大小图例示例值个数</summary>
    Public Property SizeLegendCount As Integer = 3

    Public Sub New(width As Integer, height As Integer, Optional theme As PlotTheme = Nothing)
        MyBase.New(width, height, theme)
    End Sub

    Public Sub Plot(seriesList As IList(Of BubbleSeries))
        DrawBackground()
        ComputePlotArea()
        DrawPlotArea()
        DrawTitle()

        ' ---- 坐标范围 ----
        Dim allX = seriesList.SelectMany(Function(s) s.X).ToArray()
        Dim allY = seriesList.SelectMany(Function(s) s.Y).ToArray()
        Dim allS = seriesList.SelectMany(Function(s) s.Sizes).ToArray()

        Dim xmin = If(Me.XMin, allX.Min())
        Dim xmax = If(Me.XMax, allX.Max())
        Dim ymin = If(Me.YMin, allY.Min())
        Dim ymax = If(Me.YMax, allY.Max())
        If Me.XMin Is Nothing AndAlso Me.XMax Is Nothing Then
            Dim pad = (xmax - xmin) * 0.08
            If pad = 0 Then pad = 1
            xmin -= pad : xmax += pad
        End If
        If Me.YMin Is Nothing AndAlso Me.YMax Is Nothing Then
            Dim pad = (ymax - ymin) * 0.08
            If pad = 0 Then pad = 1
            ymin -= pad : ymax += pad
        End If

        Dim sMin = If(allS.Length > 0, allS.Min(), 0)
        Dim sMax = If(allS.Length > 0, allS.Max(), 1)
        If sMax <= sMin Then sMax = sMin + 1

        DrawAxisAndGrid(xmin, xmax, ymin, ymax)

        ' ---- 绘制气泡 ----
        For i = 0 To seriesList.Count - 1
            Dim s = seriesList(i)
            If Not s.Visible Then Continue For
            Dim color = If(s.Color, Theme.Palette(i Mod Theme.Palette.Length))
            For j = 0 To s.X.Length - 1
                Dim px = ToPixelX(s.X(j), xmin, xmax)
                Dim py = ToPixelY(s.Y(j), ymin, ymax)
                Dim radius = SizeToRadius(s.Sizes(j), sMin, sMax)
                Using br As New SolidBrush(Color.FromArgb(FillAlpha, color))
                    _g.FillEllipse(br, px - radius, py - radius, radius * 2, radius * 2)
                End Using
                Using pen As New Pen(color, Theme.LineWidth * 0.7F)
                    _g.DrawEllipse(pen, px - radius, py - radius, radius * 2, radius * 2)
                End Using
            Next
        Next

        ' ---- 颜色图例 ----
        DrawLegend(seriesList.Cast(Of Series)().ToList())

        ' ---- 大小图例 ----
        If ShowSizeLegend AndAlso allS.Length > 0 Then
            DrawSizeLegend(sMin, sMax)
        End If
    End Sub

    ''' <summary>数值映射为气泡半径</summary>
    Private Function SizeToRadius(v As Double, sMin As Double, sMax As Double) As Single
        Dim t = (v - sMin) / (sMax - sMin)
        t = Math.Max(0, Math.Min(1, t))
        ' 平方根映射，使面积（而非半径）与数值成比例
        Return CSng(MinBubbleSize + (MaxBubbleSize - MinBubbleSize) * Math.Sqrt(t))
    End Function

    ''' <summary>右下角绘制大小图例（同心圆示例）</summary>
    Private Sub DrawSizeLegend(sMin As Double, sMax As Double)
        Dim lx = _plotArea.Right - MaxBubbleSize - 10
        Dim lyBase = _plotArea.Bottom - 6
        Using br As New SolidBrush(Theme.LegendBackgroundColor),
              pen As New Pen(Theme.LegendBorderColor, 0.7F)
            Dim legendW = MaxBubbleSize * 2 + 70
            Dim legendH = MaxBubbleSize * 2 + 24
            _g.FillRectangle(br, lx - 6, lyBase - legendH + 6, legendW, legendH)
            _g.DrawRectangle(pen, lx - 6, lyBase - legendH + 6, legendW, legendH)
        End Using

        For k = 0 To SizeLegendCount - 1
            Dim t = If(SizeLegendCount = 1, 1.0, k / (SizeLegendCount - 1))
            Dim v = sMin + t * (sMax - sMin)
            Dim r = SizeToRadius(v, sMin, sMax)
            Dim cy = lyBase - r
            Using br As New SolidBrush(Color.FromArgb(FillAlpha, Theme.TextColor)),
                  pen As New Pen(Theme.TextColor, 0.7F)
                _g.FillEllipse(br, lx + MaxBubbleSize - r, cy, r * 2, r * 2)
                _g.DrawEllipse(pen, lx + MaxBubbleSize - r, cy, r * 2, r * 2)
            End Using
            Using br As New SolidBrush(Theme.TextColor),
                  sf As New StringFormat()
                sf.Alignment = StringAlignment.Near
                sf.LineAlignment = StringAlignment.Center
                _g.DrawString(FormatNumber(v), Theme.TickLabelFont, br,
                              lx + MaxBubbleSize + MaxBubbleSize + 4, cy + r)
            End Using
        Next
    End Sub
End Class
