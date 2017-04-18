Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors

Public Class Form1

    Dim WithEvents colors As New ColorPalette With {.Dock = DockStyle.Fill}

    Public setColor As Action(Of Color)

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles Me.Load
        Controls.Add(colors)
    End Sub

    Private Sub colors_SelectColor(c As Color) Handles colors.SelectColor
        Call setColor(c)
    End Sub
End Class