Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Physics

Public Class Form3

    Public gravity As Single = 9.8
    Public position As Vector2 = Vector2.zero
    Public velocity As Vector2 = Vector2.zero
    Public particleSize As Single = 5

    Public ReadOnly Property deltaTime As Single
        Get
            Return Timer1.Interval / 1000
        End Get
    End Property

    Private Sub Updates()
        velocity += Vector2.down * gravity * deltaTime
        position += velocity * deltaTime

        Call ResolveCollisions()

    End Sub


    Private Sub ResolveCollisions()

    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        Call Updates()
        PictureBox1.Image = FluidRender.Render(PictureBox1.Size, Me)
    End Sub
End Class

Public Module FluidRender

    Public Function Render(canvas As Size, container As Form3) As Bitmap
        Dim bmp As New Bitmap(canvas.Width, canvas.Height)

        Using gfx As Graphics = Graphics.FromImage(bmp)
            Call gfx.Clear(Color.Black)

            Call gfx.DrawCircle(container.position, container.particleSize, Brushes.Blue)

            Return bmp
        End Using
    End Function

End Module