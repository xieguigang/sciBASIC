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

        Dim fontLarge As New Font(FontFace.MicrosoftYaHei, 20, FontStyle.Regular)
        Call g.DrawString(scaler.xmin, fontLarge, Brushes.Black, New PointF(o.X + 10, o.Y + 10))
        Dim fontSmall As New Font(FontFace.MicrosoftYaHei, 14)

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
            Dim label As Single = dx * (i + 1)
            Dim sz As SizeF

            If scaler.type <> GetType(Histogram) Then
                Dim x = sx(label + scaler.xmin)
                Dim axisX As New PointF(x, o.Y)

                sz = g.MeasureString(label.ToString, fontLarge)

                Call g.DrawLine(pen, axisX, New PointF(x, o.Y + margin.Height * 0.2))
                Call g.DrawString(label, fontLarge, Brushes.Black, New Point(x - sz.Width / 2, o.Y + margin.Height * 0.3))

                If showGrid Then
                    Call g.DrawLine(gridPenX, axisX, New PointF(x, margin.Height))
                End If
            End If

            label = Math.Round(dy * (i + 1), 3)

            Dim y = sy(label + scaler.ymin)
            Dim axisY As New PointF(o.X, y)

            Call g.DrawLine(pen, axisY, New PointF(o.X - margin.Width * 0.1, y))

            sz = g.MeasureString(label, fontSmall)
            g.DrawString(label, fontSmall, Brushes.Black, New Point(o.X - margin.Width * 0.1 - sz.Width, y - sz.Height / 2))

            If showGrid Then
                Call g.DrawLine(gridPenY, axisY, New PointF(size.Width - margin.Width, y))
            End If
        Next
    End Sub
End Module
