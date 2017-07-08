Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Mathematical.LinearAlgebra
Imports Microsoft.VisualBasic.Mathematical.SyntaxAPI.MathExtension

Public Module Debugger

    <Extension>
    Public Function Vector2D(v As Vector) As PointF
        Return New PointF(v(X), v(Y))
    End Function

    <Extension> Public Sub ShowForce(m As MassPoint, ByRef canvas As Graphics2D, F As IEnumerable(Of Force), Optional offset As PointF = Nothing)
        Dim pen As New Pen(Color.Red) With {
            .EndCap = LineCap.ArrowAnchor,
            .Width = 5
        }
        Dim font As New Font(FontFace.MicrosoftYaHei, 12, FontStyle.Bold)
        Dim a = m.Point.Vector2D.OffSet2D(offset)

        With canvas

            Call .DrawCircle(a, 10, Brushes.Black)
            Call .DrawString(m.ToString, font, Brushes.Black, a)

            Dim draw = Sub(force As Force, color As SolidBrush)
                           Dim v = force.Decomposition2D
                           Dim b = (m.Point + v).Vector2D.OffSet2D(offset)

                           Call .DrawLine(pen, a, b)
                           Call .DrawString(F.ToString, font, color, b)
                       End Sub

            For Each force As Force In F
                Call draw(force, Brushes.SkyBlue)
            Next

            ' 绘制出合力
            Call draw(F.Sum, Brushes.Violet)
        End With
    End Sub
End Module
