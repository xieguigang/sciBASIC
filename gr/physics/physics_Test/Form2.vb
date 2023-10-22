Imports System.Drawing
Imports System.Windows.Forms
Imports Microsoft.VisualBasic.Imaging.Physics.Boids
Imports Microsoft.VisualBasic.Parallel

Namespace Boids.Viewer

    ''' <summary>
    ''' Graphics Simulations
    ''' 
    ''' https://github.com/swharden/Csharp-Data-Visualization/tree/main/dev/old/drawing/boids
    ''' </summary>
    Partial Public Class Form2
        Inherits Form
        Private field As Field
        Public Sub New()
            InitializeComponent()
            Reset()

            Size = New Size(1920, 1080)
        End Sub

        Private Sub pictureBox1_SizeChanged(sender As Object, e As EventArgs) Handles pictureBox1.SizeChanged
            Reset()
        End Sub
        Private Sub pictureBox1_Click(sender As Object, e As EventArgs)
            Reset()
        End Sub
        Private Sub Reset()
            VectorTask.n_threads = 12
            field = New Field(pictureBox1.Width, pictureBox1.Height, 10000)
        End Sub

        Private Sub timer1_Tick(sender As Object, e As EventArgs) Handles timer1.Tick
            field.Advance()
            ' pictureBox1.Image?.Dispose()
            pictureBox1.Image = RenderField(field)
        End Sub

    End Class
End Namespace
