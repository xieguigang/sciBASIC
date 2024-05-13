Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging

Namespace Graphic.Legend

    Public Class CircleSizeLegend

        Public Property radius As Integer()
        Public Property title As String
        Public Property titleFont As Font
        Public Property radiusFont As Font
        Public Property circleStroke As Pen

        Public Sub Draw(g As IGraphics, layout As Rectangle)
            Dim titleSize As SizeF = g.MeasureString(title, titleFont)
            Dim tickSize As SizeF = g.MeasureString("0", radiusFont)
            Dim y As Single = layout.Top + titleSize.Height * 2
            Dim r As Single
            Dim max_r As Single = radius.Max
            Dim tick_left As Single = layout.Left + max_r * 3
            Dim left As Single = layout.Left
            Dim dy As Single = radius.Average

            Call g.DrawString(title, titleFont, Brushes.Black, layout.Left, layout.Top)

            For Each radius As Integer In Me.radius
                r = radius * 2
                y += r

                Call g.DrawEllipse(circleStroke, New RectangleF(left + (max_r * 2 - r) / 2, y - r, r, r))
                Call g.DrawString(radius, radiusFont, Brushes.Black, New PointF(tick_left, y - r / 2 - tickSize.Height / 2))

                y += dy
            Next
        End Sub
    End Class
End Namespace