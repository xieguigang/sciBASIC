Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging.Physics.Boids

Namespace Boids.Viewer
    Public Module SDRender
        Public Function RenderField(field As Field) As Bitmap
            Dim bmp As Bitmap = New Bitmap(CInt(field.Width), CInt(field.Height))
            Using gfx = Graphics.FromImage(bmp)
                gfx.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
                gfx.Clear(ColorTranslator.FromHtml("#003366"))
                For i = 0 To field.Boids.Count() - 1
                    If i < 3 Then
                        RenderBoid(gfx, field.Boids(i), Color.White)
                    Else
                        RenderBoid(gfx, field.Boids(i), Color.LightGreen)
                    End If
                Next
            End Using
            Return bmp
        End Function

        Private Sub RenderBoid(gfx As Graphics, boid As BOID, color As Color)
            Dim boidOutline = New Point() {New Point(0, 0), New Point(-4, -1), New Point(0, 8), New Point(4, -1), New Point(0, 0)}

            Using brush = New SolidBrush(color)
                gfx.TranslateTransform(boid.X, boid.Y)
                gfx.RotateTransform(CSng(boid.GetAngle()))
                gfx.FillClosedCurve(brush, boidOutline)
                gfx.ResetTransform()
            End Using
        End Sub
    End Module
End Namespace
