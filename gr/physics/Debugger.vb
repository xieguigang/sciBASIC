Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Mathematical.LinearAlgebra

Public Module Debugger

    <Extension>
    Public Function Vector2D(v As Vector) As PointF
        Return New PointF(v(X), v(Y))
    End Function

    <Extension> Public Sub ShowForce(m As MassPoint, canvas As Graphics2D, F As Force)
        Dim v = F.Decomposition2D
        Dim pen As New Pen(Color.Red) With {
            .EndCap = LineCap.Triangle,
            .Width = 3
        }

        Call canvas.DrawCircle(m.Point.Vector2D, 10, Brushes.Black)
        Call canvas.DrawLine(pen, m.Point.Vector2D, (m.Point + v).Vector2D)
    End Sub
End Module
