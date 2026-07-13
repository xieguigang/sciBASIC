Imports System.Windows.Controls
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Models

Public Class FormAbout

    Dim cubeModel As New Cube(10)
    Dim WithEvents renderer As New SceneRenderer()

    Private Sub FormAbout_Load(sender As Object, e As EventArgs) Handles Me.Load
        renderer.Camera.Screen = Canvas.ClientSize
        renderer.BackgroundColor = Canvas.BackColor
        ' renderer.LoadModel(path)

        renderer.FitView()
        Canvas.Invalidate()
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