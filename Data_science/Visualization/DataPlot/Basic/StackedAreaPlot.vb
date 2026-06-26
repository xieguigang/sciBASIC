Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging

''' <summary>堆叠面积图</summary>
Public Class StackedAreaPlot
    Inherits PlotEngine

    Public Property Categories As String() = {}
    ''' <summary>多系列数据 [系列, 类别]</summary>
    Public Property MultiValues As Double(,) = Nothing
    Public Property SeriesNames As String() = {}
    ''' <summary>是否启用 Catmull-Rom 平滑曲线</summary>
    Public Property Smooth As Boolean = False
    Public Property SmoothSegments As Integer = 20
    Public Property FillAlpha As Integer = 140
    Public Property ShowValueLabels As Boolean = False

    Public Sub New(width As Integer, height As Integer, Optional theme As PlotTheme = Nothing)
        MyBase.New(width, height, theme)
    End Sub

    Public Sub Plot()
        DrawBackground()
        ComputePlotArea()
        DrawPlotArea()
        DrawTitle()

        Dim nSer = MultiValues.GetLength(0)
        Dim nCat = MultiValues.GetLength(1)

        ' ---- 累积值：cum[k, j] = 前 k 个系列在类别 j 的累加和（cum[0,*]=0 为基线）----
        Dim cum(nSer, nCat - 1) As Double
        For k = 0 To nSer - 1
            For j = 0 To nCat - 1
                cum(k + 1, j) = cum(k, j) + MultiValues(k, j)
            Next
        Next

        ' ---- Y 范围 ----
        Dim vmax = 0.0
        For j = 0 To nCat - 1
            If cum(nSer, j) > vmax Then vmax = cum(nSer, j)
        Next
        If vmax <= 0 Then vmax = 1
        vmax *= 1.1
        Dim vmin = 0.0

        ' ---- X 范围（类别索引 0..nCat-1，加少量边距）----
        Dim xmin = 0.0, xmax = CDbl(Math.Max(1, nCat - 1))
        Dim xpad = (xmax - xmin) * 0.02
        If xpad = 0 Then xpad = 0.5
        xmin -= xpad : xmax += xpad

        Dim xTicks = Enumerable.Range(0, nCat).Select(Function(i) CDbl(i)).ToArray()
        Dim yTicks = GenerateTicks(vmin, vmax)
        DrawAxisAndGrid(xmin, xmax, vmin, vmax, xTicks, yTicks, Categories, Nothing)

        ' ---- 逐层绘制 ----
        For k = 0 To nSer - 1
            Dim color = Theme.Palette(k Mod Theme.Palette.Length)

            ' 上界（cum[k+1]）与下界（cum[k]）像素点
            Dim upper = New List(Of PointF)()
            Dim lower = New List(Of PointF)()
            For j = 0 To nCat - 1
                Dim px = ToPixelX(j, xmin, xmax)
                upper.Add(New PointF(px, ToPixelY(cum(k + 1, j), vmin, vmax)))
                lower.Add(New PointF(px, ToPixelY(cum(k, j), vmin, vmax)))
            Next

            Dim upperDraw = If(Smooth AndAlso upper.Count >= 3,
                               CatmullRom(upper, SmoothSegments), upper)
            Dim lowerDraw = If(Smooth AndAlso lower.Count >= 3,
                               CatmullRom(lower, SmoothSegments), lower)

            ' 多边形：上界左→右 + 下界右→左
            Dim poly = New List(Of PointF)(upperDraw)
            For m = lowerDraw.Count - 1 To 0 Step -1
                poly.Add(lowerDraw(m))
            Next

            Using br As New SolidBrush(Color.FromArgb(FillAlpha, color))
                _g.FillPolygon(br, poly.ToArray())
            End Using
            Using pen As New Pen(color, Theme.LineWidth)
                _g.DrawLines(pen, upperDraw.ToArray())
            End Using

            ' 数值标签（标在累积上界）
            If ShowValueLabels Then
                Using br As New SolidBrush(Theme.TextColor),
                      sf As New StringFormat()
                    sf.Alignment = StringAlignment.Center
                    sf.LineAlignment = StringAlignment.Far
                    For j = 0 To nCat - 1
                        Dim px = ToPixelX(j, xmin, xmax)
                        Dim py = ToPixelY(cum(k + 1, j), vmin, vmax)
                        _g.DrawString(FormatNumber(cum(k + 1, j)), Theme.TickLabelFont, br, px, py - 2)
                    Next
                End Using
            End If
        Next

        ' ---- 图例 ----
        Dim seriesList As New List(Of Series)()
        For k = 0 To nSer - 1
            seriesList.Add(New Series With {
                .Name = If(SeriesNames IsNot Nothing AndAlso k < SeriesNames.Length, SeriesNames(k), "Series " & (k + 1)),
                .Color = Theme.Palette(k Mod Theme.Palette.Length),
                .MarkerShape = MarkerShape.Square
            })
        Next
        DrawLegend(seriesList)
    End Sub

    ''' <summary>Catmull-Rom 样条插值，返回密集采样点列表</summary>
    Private Function CatmullRom(pts As List(Of PointF), segments As Integer) As List(Of PointF)
        Dim result As New List(Of PointF)()
        If pts.Count < 2 Then Return pts
        For i = 0 To pts.Count - 2
            Dim p0 = If(i = 0, pts(0), pts(i - 1))
            Dim p1 = pts(i)
            Dim p2 = pts(i + 1)
            Dim p3 = If(i + 2 < pts.Count, pts(i + 2), pts(i + 1))
            For t = 0 To segments - 1
                Dim s As Double = t / segments
                Dim s2 = s * s
                Dim s3 = s2 * s
                Dim x = 0.5 * ((2 * p1.X) + (-p0.X + p2.X) * s +
                               (2 * p0.X - 5 * p1.X + 4 * p2.X - p3.X) * s2 +
                               (-p0.X + 3 * p1.X - 3 * p2.X + p3.X) * s3)
                Dim y = 0.5 * ((2 * p1.Y) + (-p0.Y + p2.Y) * s +
                               (2 * p0.Y - 5 * p1.Y + 4 * p2.Y - p3.Y) * s2 +
                               (-p0.Y + 3 * p1.Y - 3 * p2.Y + p3.Y) * s3)
                result.Add(New PointF(CSng(x), CSng(y)))
            Next
        Next
        result.Add(pts(pts.Count - 1))
        Return result
    End Function
End Class
