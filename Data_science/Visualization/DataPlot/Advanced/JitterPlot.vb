Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging

''' <summary>Jitter 散点图（分组数据在 X 方向随机抖动以避免遮挡）</summary>
Public Class JitterPlot
    Inherits PlotEngine

    ''' <summary>分组数据（复用 BoxGroup 结构）</summary>
    Public Property Groups As New List(Of BoxGroup)()
    ''' <summary>抖动幅度占组宽比例（0-0.5）</summary>
    Public Property JitterWidth As Single = 0.3F
    ''' <summary>随机种子（确保可复现，Nothing 表示随机）</summary>
    Public Property RandomSeed As Integer? = 42
    ''' <summary>标记形状</summary>
    Public Property MarkerShape As MarkerShape = MarkerShape.Circle
    ''' <summary>标记大小</summary>
    Public Property MarkerSize As Single = 5.0F
    ''' <summary>标记透明度（0-255）</summary>
    Public Property MarkerAlpha As Integer = 180
    Public Property Horizontal As Boolean = False

    Public Sub New(width As Integer, height As Integer, Optional theme As PlotTheme = Nothing)
        MyBase.New(width, height, theme)
    End Sub

    Public Sub Plot()
        DrawBackground()
        ComputePlotArea()
        DrawPlotArea()
        DrawTitle()

        Dim nGrp = Groups.Count
        ' ---- 全局 Y 范围 ----
        Dim allData = Groups.SelectMany(Function(g) g.Data).ToArray()
        Dim ymin = If(Me.YMin, allData.Min())
        Dim ymax = If(Me.YMax, allData.Max())
        Dim pad = (ymax - ymin) * 0.08
        If pad = 0 Then pad = 1
        ymin -= pad : ymax += pad

        Dim rnd = If(RandomSeed.HasValue, New Random(RandomSeed.Value), New Random())

        ' ---- 坐标轴 ----
        Dim xTicks = Enumerable.Range(0, nGrp).Select(Function(i) CDbl(i)).ToArray()
        Dim groupNames = Groups.Select(Function(g) g.Name).ToArray()
        If Horizontal Then
            Dim yTicks = Enumerable.Range(0, nGrp).Select(Function(i) CDbl(i)).ToArray()
            DrawAxisAndGrid(ymin, ymax, -0.5, nGrp - 0.5,
                            GenerateTicks(ymin, ymax), yTicks, Nothing, groupNames)
        Else
            DrawAxisAndGrid(-0.5, nGrp - 0.5, ymin, ymax,
                            xTicks, GenerateTicks(ymin, ymax), groupNames, Nothing)
        End If

        ' ---- 绘制抖动散点 ----
        Dim groupWidth = If(Horizontal, _plotArea.Height / nGrp, _plotArea.Width / nGrp)
        Dim jitterPx = groupWidth * JitterWidth

        For i = 0 To nGrp - 1
            Dim g = Groups(i)
            Dim color = If(g.Color, Theme.Palette(i Mod Theme.Palette.Length))
            For Each v In g.Data
                Dim offset = (rnd.NextDouble() * 2 - 1) * jitterPx
                If Horizontal Then
                    ' X=数值, Y=组中心+抖动
                    Dim px = ToPixelX(v, ymin, ymax)
                    Dim cy = _plotArea.Top + (i + 0.5) * groupWidth
                    Dim py = cy + offset
                    DrawMarkerAlpha(px, py, MarkerShape, MarkerSize, color, MarkerAlpha)
                Else
                    ' X=组中心+抖动, Y=数值
                    Dim cx = _plotArea.Left + (i + 0.5) * groupWidth
                    Dim px = cx + offset
                    Dim py = ToPixelY(v, ymin, ymax)
                    DrawMarkerAlpha(px, py, MarkerShape, MarkerSize, color, MarkerAlpha)
                End If
            Next
        Next

        ' ---- 图例 ----
        If ShowLegend AndAlso nGrp > 1 Then
            Dim seriesList As New List(Of Series)()
            For i = 0 To nGrp - 1
                seriesList.Add(New Series With {
                    .Name = Groups(i).Name,
                    .Color = If(Groups(i).Color, Theme.Palette(i Mod Theme.Palette.Length)),
                    .MarkerShape = MarkerShape.Square
                })
            Next
            DrawLegend(seriesList)
        End If
    End Sub

    ''' <summary>带透明度的标记绘制（基类 DrawMarker 不支持透明度）</summary>
    Private Sub DrawMarkerAlpha(x As Single, y As Single, shape As MarkerShape, size As Single, color As Color, alpha As Integer)
        Dim c = Color.FromArgb(alpha, color)
        Using br As New SolidBrush(c)
            Select Case shape
                Case MarkerShape.Circle
                    _g.FillEllipse(br, x - size / 2, y - size / 2, size, size)
                Case MarkerShape.Square
                    _g.FillRectangle(br, x - size / 2, y - size / 2, size, size)
                Case MarkerShape.Triangle
                    Dim pts = {
                        New PointF(x, y - size / 2),
                        New PointF(x - size / 2, y + size / 2),
                        New PointF(x + size / 2, y + size / 2)
                    }
                    _g.FillPolygon(br, pts)
                Case MarkerShape.Diamond
                    Dim pts = {
                        New PointF(x, y - size / 2),
                        New PointF(x + size / 2, y),
                        New PointF(x, y + size / 2),
                        New PointF(x - size / 2, y)
                    }
                    _g.FillPolygon(br, pts)
                Case Else
                    _g.FillEllipse(br, x - size / 2, y - size / 2, size, size)
            End Select
        End Using
    End Sub
End Class
