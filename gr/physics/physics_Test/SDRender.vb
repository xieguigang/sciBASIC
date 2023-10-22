Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Imaging.Physics.Boids

Namespace Boids.Viewer
    Public Module SDRender

        Dim colors As Color()
        Dim n As Integer = 30

        Sub New()
            colors = Designer.GetColors(ScalerPalette.Typhoon.Description, n)
        End Sub

        Public Function RenderField(field As Field) As Bitmap
            Dim bmp As Bitmap = New Bitmap(CInt(field.Width), CInt(field.Height))
            Dim max As Double = field.MaxSpeed

            Using gfx = Graphics.FromImage(bmp)
                gfx.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
                gfx.Clear(Color.Black) ' (ColorTranslator.FromHtml("#003366"))

                Dim len As Integer = field.Entity.Count

                For i = 0 To len - 1
                    Dim boid As Boid = field(i)

                    If i < field.PredatorCount Then
                        RenderShape.RenderBoid(gfx, boid.x, boid.y, boid.GetAngle, Color.White)
                    Else
                        Dim lv As Integer = ((boid.GetSpeed / max) * n) - 1

                        If lv < 0 Then
                            lv = 0
                        ElseIf lv >= colors.Length Then
                            lv = colors.Length - 1
                        End If

                        RenderShape.RenderBoid(gfx, boid.x, boid.y, boid.GetAngle, colors(lv))
                    End If
                Next
            End Using
            Return bmp
        End Function
    End Module
End Namespace
