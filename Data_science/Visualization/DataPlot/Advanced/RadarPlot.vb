Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging

''' <summary>雷达图（蛛网图，多维度多系列叠加）</summary>
Public Class RadarPlot
    Inherits PlotEngine

    Public Property Categories As String() = {}
    ''' <summary>多系列数据 [系列, 类别]</summary>
    Public Property MultiValues As Double(,) = Nothing
    Public Property SeriesNames As String() = {}
    ''' <summary>填充透明度（0-255）</summary>
    Public Property FillAlpha As Integer = 100
    ''' <summary>同心网格层数</summary>
    Public Property GridLevels As Integer = 4
    ''' <summary>数据值归一化最大值（Nothing 表示自动）</summary>
    Public Property MaxValue As Double? = Nothing
    ''' <summary>显示数值刻度</summary>
    Public Property ShowScale As Boolean = False

    Public Sub New(width As Integer, height As Integer, Optional theme As PlotTheme = Nothing)
        MyBase.New(width, height, theme)
    End Sub

    Public Sub Plot()
        DrawBackground()
        DrawTitle()

        Dim nSer = MultiValues.GetLength(0)
        Dim nCat = MultiValues.GetLength(1)

        ' 数据最大值
        Dim vmax = 0.0
        If MaxValue.HasValue Then
            vmax = MaxValue.Value
        Else
            For i = 0 To nSer - 1
                For j = 0 To nCat - 1
                    If MultiValues(i, j) > vmax Then vmax = MultiValues(i, j)
                Next
            Next
        End If
        If vmax <= 0 Then vmax = 1

        ' 圆心与半径
        Dim cx = _width / 2.0F
        Dim cy = (_height + Theme.MarginTop) / 2.0F
        Dim R = Math.Min(_width - Theme.MarginLeft - Theme.MarginRight,
                         _height - Theme.MarginTop - Theme.MarginBottom) * 0.38F

        ' 各维度角度（从顶部 12 点方向开始顺时针）
        Dim angles(nCat - 1) As Double
        For j = 0 To nCat - 1
            angles(j) = (-90 + j * 360.0 / nCat) * Math.PI / 180
        Next

        ' ---- 同心多边形网格 ----
        If Theme.ShowGrid Then
            Using pen As New Pen(Theme.GridColor, Theme.GridLineWidth)
                For g = 1 To GridLevels
                    Dim rr = R * g / GridLevels
                    Dim pts = New PointF(nCat - 1) {}
                    For j = 0 To nCat - 1
                        pts(j) = New PointF(cx + CSng(Math.Cos(angles(j))) * rr,
                                            cy + CSng(Math.Sin(angles(j))) * rr)
                    Next
                    _g.DrawPolygon(pen, pts)
                Next
            End Using
        End If

        ' ---- 辐射轴 ----
        Using pen As New Pen(Theme.GridColor, Theme.GridLineWidth)
            For j = 0 To nCat - 1
                _g.DrawLine(pen, cx, cy,
                            cx + CSng(Math.Cos(angles(j))) * R,
                            cy + CSng(Math.Sin(angles(j))) * R)
            Next
        End Using

        ' ---- 数值刻度 ----
        If ShowScale Then
            Using br As New SolidBrush(Theme.SubTitleColor),
                  sf As New StringFormat()
                sf.Alignment = StringAlignment.Near
                sf.LineAlignment = StringAlignment.Center
                For g = 1 To GridLevels
                    Dim rr = R * g / GridLevels
                    Dim v = vmax * g / GridLevels
                    _g.DrawString(FormatNumber(v), Theme.TickLabelFont, br, cx + 2, cy - rr)
                Next
            End Using
        End If

        ' ---- 轴标签 ----
        Using br As New SolidBrush(Theme.TextColor),
              sf As New StringFormat()
            sf.Alignment = StringAlignment.Center
            sf.LineAlignment = StringAlignment.Center
            For j = 0 To nCat - 1
                Dim labelR = R * 1.15F
                Dim lx = cx + CSng(Math.Cos(angles(j))) * labelR
                Dim ly = cy + CSng(Math.Sin(angles(j))) * labelR
                _g.DrawString(If(j < Categories.Length, Categories(j), ""), Theme.TickLabelFont, br, lx, ly)
            Next
        End Using

        ' ---- 多系列数据多边形 ----
        For i = 0 To nSer - 1
            Dim color = Theme.Palette(i Mod Theme.Palette.Length)
            Dim pts(nCat - 1) As PointF
            For j = 0 To nCat - 1
                Dim rr = CSng(MultiValues(i, j) / vmax * R)
                pts(j) = New PointF(cx + CSng(Math.Cos(angles(j))) * rr,
                                    cy + CSng(Math.Sin(angles(j))) * rr)
            Next
            Using br As New SolidBrush(Color.FromArgb(FillAlpha, color))
                _g.FillPolygon(br, pts)
            End Using
            Using pen As New Pen(color, Theme.LineWidth)
                _g.DrawPolygon(pen, pts)
            End Using
            ' 顶点标记
            For j = 0 To nCat - 1
                DrawMarker(pts(j).X, pts(j).Y, MarkerShape.Circle, Theme.MarkerSize * 0.7F, color)
            Next
        Next

        ' ---- 图例 ----
        If nSer > 1 Then
            Dim seriesList As New List(Of Series)()
            For i = 0 To nSer - 1
                seriesList.Add(New Series With {
                    .Name = If(SeriesNames IsNot Nothing AndAlso i < SeriesNames.Length, SeriesNames(i), "Series " & (i + 1)),
                    .Color = Theme.Palette(i Mod Theme.Palette.Length),
                    .MarkerShape = MarkerShape.Circle
                })
            Next
            DrawLegend(seriesList)
        End If
    End Sub
End Class
