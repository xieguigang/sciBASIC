Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Shapes
Imports Microsoft.VisualBasic.MIME.Html.Render

Namespace PostScript.Elements

    Public Class Rectangle : Inherits PSElement(Of Box)

        Sub New()
        End Sub

        Sub New(rect As System.Drawing.Rectangle, color As Color)
            shape = New Box(rect, color)
        End Sub

        Sub New(rect As RectangleF, color As Color)
            shape = New Box(rect, color)
        End Sub

        Friend Overrides Sub WriteAscii(ps As Writer)
            Dim rect As RectangleF = shape.DrawingRegion

            Call ps.rectangle(rect, shape.fill, False)

            If shape.border IsNot Nothing Then
                Call ps.rectangle(rect, shape.border.fill, True)
            End If
        End Sub

        Friend Overrides Sub Paint(g As IGraphics)
            Call g.DrawRectangle(g.LoadEnvironment.GetPen(shape.border), shape.DrawingRegion)
        End Sub
    End Class
End Namespace