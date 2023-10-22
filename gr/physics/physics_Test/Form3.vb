Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Physics
Imports System.Math

Public Class Form3

    Public gravity As Single = 98
    Public position As Vector2()
    Public velocity As Vector2()
    Public particleSize As Single = 20
    Public collisionDamping As Single = 0.99
    Public smoothingRadius As Single = 100

    Public ReadOnly Property deltaTime As Single
        Get
            Return Timer1.Interval / 1000
        End Get
    End Property

    Private Sub Updates()
        For i As Integer = 0 To position.Length - 1
            velocity(i) += Vector2.down * gravity * deltaTime
            position(i) += velocity(i) * deltaTime

            Call ResolveCollisions(i)
        Next
    End Sub

    Public Function smoothingKernel(radius As Single, dst As Single) As Single
        Dim volume = PI * radius ^ 8 / 4
        Dim value = Max(0, radius ^ 2 - dst ^ 2)
        Return value ^ 3 / volume
    End Function

    Public Function CalculateDensity(samplePoint As Vector2) As Single
        Dim density As Single = 0
        Dim mass As Single = 1

        For Each position As Vector2 In Me.position
            Dim dst = (position - samplePoint).magnitude
            Dim influence = smoothingKernel(smoothingRadius, dst)

            density += mass * influence
        Next

        Return density
    End Function

    Private Sub ResolveCollisions(i As Integer)
        If position(i).x > Width - particleSize OrElse position(i).x < particleSize Then
            velocity(i).x *= -1 * collisionDamping
        End If
        If position(i).y > Height - particleSize OrElse position(i).y < particleSize Then
            velocity(i).y *= -1 * collisionDamping
        End If
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        Call Updates()
        PictureBox1.Image = FluidRender.Render(PictureBox1.Size, Me)
    End Sub

    Private Sub Form3_Load(sender As Object, e As EventArgs) Handles Me.Load
        Dim n As Integer = 10

        position = New Vector2(n - 1) {}
        velocity = New Vector2(n - 1) {}

        For i As Integer = 0 To n - 1
            position(i) = Vector2.random(Size)
            velocity(i) = Vector2.zero
        Next
    End Sub
End Class

Public Module FluidRender

    Public Function Render(canvas As Size, container As Form3) As Bitmap
        Dim bmp As New Bitmap(canvas.Width, canvas.Height)

        Using gfx As Graphics = Graphics.FromImage(bmp)
            Call gfx.Clear(Color.Black)

            For i As Integer = 0 To container.position.Length - 1
                Call gfx.DrawCircle(container.position(i), container.particleSize / 2, Brushes.Blue)
            Next

            Return bmp
        End Using
    End Function

End Module