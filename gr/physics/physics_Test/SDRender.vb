Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Physics.Boids

Namespace Boids.Viewer
    Public Module SDRender
        Public Function RenderField(field As Field) As Bitmap
            Dim bmp As Bitmap = New Bitmap(CInt(field.Width), CInt(field.Height))
            Using gfx = Graphics.FromImage(bmp)
                gfx.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
                gfx.Clear(ColorTranslator.FromHtml("#003366"))
                For i = 0 To field.Boids.Count() - 1
                    Dim boid = field.Boids(i)

                    If i < 3 Then
                        RenderShape.RenderBoid(gfx, boid.x, boid.y, boid.GetAngle, Color.White)
                    Else
                        RenderShape.RenderBoid(gfx, boid.x, boid.y, boid.GetAngle, Color.LightGreen)
                    End If
                Next
            End Using
            Return bmp
        End Function
    End Module
End Namespace
