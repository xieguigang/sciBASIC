Imports Microsoft.VisualBasic.Imaging.Drawing3D.Models

Public Class FormAbout

    Dim cubeModel As New Cube(10)
    Dim WithEvents renderer As New SceneRenderer()

    Private Sub FormAbout_Load(sender As Object, e As EventArgs) Handles Me.Load

    End Sub
End Class