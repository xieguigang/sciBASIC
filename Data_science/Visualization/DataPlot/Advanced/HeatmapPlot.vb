

Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports stdf = System.Math

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
                    Dim txtColor = If(Brightness(color) > 0.5, color.Black, color.White)
                    Using br As New SolidBrush(txtColor),
                          sf As New StringFormat()
                        sf.Alignment = StringAlignment.Center
                        sf.LineAlignment = StringAlignment.Center
                        _g.DrawString(FormatNumber(v), Theme.TickLabelFont, br,
                                      rect.X + rect.Width / 2, rect.Y + rect.Height / 2)
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
                                  plotX - 6, plotY + (r + 0.5) * cellH)
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
                    _g.DrawString(ColLabels(c), Theme.TickLabelFont, br, 0, 0)
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
                _g.DrawString(FormatNumber(t), Theme.TickLabelFont, br, cbX + cbW + 6, py)
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
        Dim r = stdf.Clamp(CDbl(c1.R) + (CDbl(c2.R) - CDbl(c1.R)) * f, 0, 255)
        Dim g = stdf.Clamp(CDbl(c1.G) + (CDbl(c2.G) - CDbl(c1.G)) * f, 0, 255)
        Dim b = stdf.Clamp(CDbl(c1.B) + (CDbl(c2.B) - CDbl(c1.B)) * f, 0, 255)

        Return Color.FromArgb(r, g, b)
    End Function

    Private Function Brightness(c As Color) As Double
        Return (0.299 * c.R + 0.587 * c.G + 0.114 * c.B) / 255
    End Function
End Class

