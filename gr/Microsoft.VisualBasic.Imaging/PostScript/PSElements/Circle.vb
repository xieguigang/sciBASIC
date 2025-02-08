Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.MIME.Html.Render

Namespace PostScript.Elements

    Public Class Circle : Inherits PSElement(Of Shapes.Circle)

        Public Property stroke As Stroke

        Public ReadOnly Property center As PointF
            Get
                Dim r As Single = shape.Radius
                Dim x = r + shape.Location.X
                Dim y = r + shape.Location.Y

                Return New PointF(x, y)
            End Get
        End Property

        Friend Overrides Sub WriteAscii(ps As Writer)
            Throw New NotImplementedException()
        End Sub

        Friend Overrides Sub Paint(g As IGraphics)
            Call g.DrawCircle(center, shape.Radius, shape.fill.GetBrush)

            If Not stroke Is Nothing Then
                Call g.DrawCircle(center, shape.Radius, g.LoadEnvironment.GetPen(stroke), fill:=False)
            End If
        End Sub
    End Class
End Namespace