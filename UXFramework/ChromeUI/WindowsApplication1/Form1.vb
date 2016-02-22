Public Class Form1
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ToolStripManager.Renderer = New ChromeUI.ChromeUIRender
    End Sub
End Class
