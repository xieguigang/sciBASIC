Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Models
Imports Microsoft.VisualBasic.Imaging.Landscape.Data

Public Class FormAbout

    Dim cubeModel As New Cube(10)
    Dim WithEvents renderer As New SceneRenderer()

    Private Sub FormAbout_Load(sender As Object, e As EventArgs) Handles Me.Load
        renderer.Camera.Screen = canvas.ClientSize
        renderer.BackgroundColor = canvas.BackColor
        renderer.LoadModel(cubeModel.faces.Select(Function(a) New Landscape.Data.Surface() With {
            .paint = DirectCast(a.brush, SolidBrush).Color.ToHtmlColor,
            .vertices = a.vertices.Select(Function(p) New Vertex(p)).ToArray
        }))

        renderer.FitView()
        canvas.Invalidate()
    End Sub

    Private Sub RenderPanel1_Paint(sender As Object, e As PaintEventArgs) Handles canvas.Paint
        If renderer Is Nothing Then Return
        If renderer.HasData Then
            renderer.Draw(e.Graphics, canvas.ClientSize)
        Else
            e.Graphics.Clear(renderer.BackgroundColor)
        End If
    End Sub
End Class