
Imports System.Drawing

Imports Microsoft.VisualBasic.Imaging

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
