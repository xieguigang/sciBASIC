Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.MIME.Html.Render
Imports std = System.Math

#If NET48 Then
Imports System.Drawing
#End If

Namespace PostScript.Elements

    Public Class Arc : Inherits PSElement

        Public Property stroke As Stroke
        Public Property x As Single
        Public Property y As Single
        Public Property width As Single
        Public Property height As Single
        Public Property startAngle As Single
        Public Property sweepAngle As Single

        Friend Overrides Sub WriteAscii(ps As Writer)
            Dim startAngleRad As Double = startAngle * (std.PI / 180)
            Dim sweepAngleRad As Double = sweepAngle * (std.PI / 180)
            Dim startX As Double = x + (width / 2) + std.Cos(startAngleRad) * (width / 2)
            Dim startY As Double = y + (height / 2) + std.Sin(startAngleRad) * (height / 2)
            Dim endX As Double = x + (width / 2) + std.Cos(startAngleRad + sweepAngleRad) * (width / 2)
            Dim endY As Double = y + (height / 2) + std.Sin(startAngleRad + sweepAngleRad) * (height / 2)
            Dim pen As Pen = ps.pen(stroke)

            Call ps.linewidth(pen.Width)
            Call ps.color(pen.Color)
            Call ps.moveto(startX, startY)
            Call ps.arct(x, y, width, height, startAngle, sweepAngle)
            Call ps.stroke()
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Friend Overrides Sub Paint(g As IGraphics)
            Call g.DrawArc(g.LoadEnvironment.GetPen(stroke), x, y, width, height, startAngle, sweepAngle)
        End Sub
    End Class
End Namespace