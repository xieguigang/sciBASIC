
Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging

Namespace Graphic.Legend

    Public Module DualColorBar

        <Extension>
        Public Sub DrawDualColorBar(canvas As IGraphics,
                                    color1 As SolidBrush(),
                                    color2 As SolidBrush(),
                                    layout As Rectangle,
                                    ticks As Double(),
                                    axisPen As Pen,
                                    tickPen As Pen,
                                    title As String,
                                    titleFont As Font,
                                    tickFont As Font,
                                    tickFormat As String)

            Dim width As Integer = layout.Width / 2.5
            Dim d As Double = layout.Width - width * 2
            Dim dy As Double = layout.Height / color1.Length
            Dim x As Double = layout.Left
            Dim y As Double = layout.Top

            For Each color As SolidBrush In color1.Reverse
                canvas.FillRectangle(color, New Rectangle(x, y, width, dy))
                y += dy
            Next

            x += d + width
            y = layout.Top

            For Each color As SolidBrush In color2.Reverse
                canvas.FillRectangle(color, New Rectangle(x, y, width, dy))
                y += dy
            Next

            Dim titleSize As SizeF = canvas.MeasureString(title, titleFont)

            x = (layout.Width - titleSize.Width) / 2 + layout.Left
            y = layout.Top - titleSize.Height - 20

            Call canvas.DrawString(title, titleFont, Brushes.Black, New PointF(x, y))
            Call canvas.DrawLine(axisPen, New PointF(layout.Top, layout.Right), New PointF(layout.Bottom, layout.Right))

            x = layout.Right + 10
            y = layout.Top
            dy = layout.Height / (ticks.Length + 1)
            d = canvas.MeasureString("0", tickFont).Height / 2

            For Each tick As Double In ticks.Reverse
                canvas.DrawString(tick.ToString(tickFormat), tickFont, Brushes.Black, New PointF(x, y - d))
                canvas.DrawLine(tickPen, New PointF(layout.Right, y), New PointF(x, y))
                y += dy
            Next
        End Sub

    End Module
End Namespace