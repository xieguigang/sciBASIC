Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging

Module Axis

    <Extension>
    Public Sub DrawAxis(ByRef g As Graphics, size As Size, margin As Size, scaler As Scaling, showGrid As Boolean)
        Dim o As New Point(margin.Width, size.Height - margin.Height)
        Dim right As New Point(size.Width - margin.Width, o.Y)
        Dim top As New Point(margin.Width, margin.Height)
        Dim pen As New Pen(Color.Black, 5)

        Call g.DrawLine(pen, o, right)
        Call g.DrawLine(pen, o, top)

        Dim fontLarge As New Font(FontFace.MicrosoftYaHei, 14, FontStyle.Regular)
        Call g.DrawString(scaler.xmin, fontLarge, Brushes.Black, New PointF(o.X - 30, o.Y + 10))

        Dim dx As Single = scaler.dx / 10 '+ scaler.xmin
        Dim dy As Single = scaler.dy / 10 '+ scaler.ymin
        Dim sx = scaler.XScaler(size, margin)
        Dim sy = scaler.YScaler(size, margin)
        Dim gridPenX As New Pen(Color.LightGray, 1) With {
            .DashStyle = Drawing2D.DashStyle.Dash
        }
        Dim gridPenY As New Pen(Color.LightGray, 1) With {
            .DashStyle = Drawing2D.DashStyle.Dot
        }

        pen = New Pen(Color.Black, 3)

        For i As Integer = 0 To 9
            Dim x = sx(dx * (i + 1) + scaler.xmin)
            Dim axisX As New PointF(x, o.Y)

            Call g.DrawLine(pen, axisX, New PointF(x, o.Y + 10))

            If showGrid Then
                Call g.DrawLine(gridPenX, axisX, New Point(x, margin.Height))
            End If

            Dim y = sy(dy * (i + 1) + scaler.ymin)
            Dim axisY As New PointF(o.X, y)
            Call g.DrawLine(pen, axisY, New PointF(o.X - 10, y))

            If showGrid Then
                Call g.DrawLine(gridPenY, axisY, New Point(size.Width - margin.Width, y))
            End If
        Next
    End Sub
End Module
