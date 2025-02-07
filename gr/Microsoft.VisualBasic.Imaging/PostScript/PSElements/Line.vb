Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.MIME.Html.Render

#If NETCOREAPP Then
#Else
Imports System.Drawing.Drawing2D
#End If

Namespace PostScript.Elements

    Public Class Line : Inherits PSElement(Of Shapes.Line)

        Sub New()

        End Sub

        Sub New(stroke As Pen, a As PointF, b As PointF)
            shape = New Shapes.Line(stroke, a, b)
        End Sub

        Friend Overrides Sub WriteAscii(ps As Writer)
            Dim a = shape.A
            Dim b = shape.B
            Dim pen As Pen = ps.pen(shape.Stroke)
            Dim color As Color = shape.Stroke.fill.TranslateColor

            If pen.DashStyle <> DashStyle.Solid Then
                ps.dash({2, 3})
            Else
                ps.dash(Nothing)
            End If
            If color.A <> 255 Then
                ps.transparency(color.A / 255)
                ps.beginTransparent()
            End If

            Call ps.linewidth(pen.Width)
            Call ps.color(color)

            Call ps.line(a.X, a.Y, b.X, b.Y)

            If color.A <> 255 Then
                ps.endTransparent()
            End If
        End Sub

        Friend Overrides Sub Paint(g As IGraphics)
            Dim a = shape.A
            Dim b = shape.B
            Dim pen As Pen = g.LoadEnvironment.GetPen(shape.Stroke)

            Call g.DrawLine(pen, a, b)
        End Sub
    End Class
End Namespace