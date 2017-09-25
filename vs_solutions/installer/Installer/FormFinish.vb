Public Class FormFinish

    Protected Overrides Sub ButtonNext_Click(sender As Object, e As EventArgs)
        Close()
    End Sub

    Private Sub FormFinish_Load(sender As Object, e As EventArgs) Handles Me.Load
        ButtonNext.Text = "Close"
        LabelTitle.Text = "Success!"

        Highlight(Label3)
    End Sub

    Private Sub LinkLabel1_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles LinkLabel1.LinkClicked
        Process.Start("https://github.com/xieguigang/sciBASIC")
    End Sub
End Class