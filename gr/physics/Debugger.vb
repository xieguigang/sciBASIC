Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Mathematical.LinearAlgebra

Public Module Debugger

    <Extension>
    Public Function Vector2D(v As Vector) As PointF
        Return New PointF(v(X), v(Y))
    End Function

    <Extension> Public Sub ShowForce(m As MassPoint, ByRef canvas As Graphics2D, F As Force)
        Dim v = F.Decomposition2D
        Dim pen As New Pen(Color.Red) With {
            .EndCap = LineCap.Triangle,
            .Width = 3
        }
        Dim font As New Font(FontFace.MicrosoftYaHei, 12, FontStyle.Bold)
        Dim a = m.Point.Vector2D
        Dim b = (m.Point + v).Vector2D

        With canvas
            Call .DrawCircle(a, 10, Brushes.Black)
            Call .DrawLine(pen, a, b)
            Call .DrawString(m.ToString, font, Brushes.Black, a)
            Call .DrawString(F.ToString, font, Brushes.Blue, b)
        End With
    End Sub
End Module
