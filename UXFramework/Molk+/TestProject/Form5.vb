Public Class Form5


    Private Sub ClosableLabel2_Load(sender As Object, e As EventArgs) Handles ClosableLabel2.Load
        ClosableLabel2.Text = "sfdjsfhsdkjfsdfs"
    End Sub

    Private Sub ClosableLabel2_CloseInvoke() Handles ClosableLabel2.CloseInvoke
        MsgBox("close")
    End Sub

    Private Sub Form5_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Call UserPicker1.Update()
    End Sub
End Class