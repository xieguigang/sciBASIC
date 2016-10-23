Imports Microsoft.VisualBasic.Mathematical.Plots.Plot3D

Public Class Form1

    Dim WithEvents canvas As Canvas

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles Me.Load
        canvas = New Canvas
        canvas.Dock = System.Windows.Forms.DockStyle.Fill
        Controls.Add(canvas)
    End Sub
End Class