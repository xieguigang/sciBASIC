
Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging

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
        Using br As New SolidBrush(color.FromArgb(180, color)),
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

