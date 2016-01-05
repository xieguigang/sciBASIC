Public Class Form1

    Dim WithEvents ccc As New Class1

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Call Me.Controls.Add(ccc)
        ccc.Dock = Windows.Forms.DockStyle.Fill
        Call ccc.Run()
    End Sub
End Class