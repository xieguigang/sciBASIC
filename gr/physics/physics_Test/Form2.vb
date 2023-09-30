Imports Boids.Model.Boids.Model
Imports System
Imports System.Windows.Forms

Namespace Boids.Viewer
    Public Partial Class Form2
        Inherits Form
        Private field As Field
        Public Sub New()
            InitializeComponent()
            Reset()
        End Sub

        Private Sub pictureBox1_SizeChanged(ByVal sender As Object, ByVal e As EventArgs)
            Reset()
        End Sub
        Private Sub pictureBox1_Click(ByVal sender As Object, ByVal e As EventArgs)
            Reset()
        End Sub
        Private Sub Reset()
            field = New Field(pictureBox1.Width, pictureBox1.Height, 1000)
        End Sub

        Private Sub timer1_Tick(ByVal sender As Object, ByVal e As EventArgs)
            field.Advance()
            pictureBox1.Image?.Dispose()
            pictureBox1.Image = RenderField(field)
        End Sub

    End Class
End Namespace
