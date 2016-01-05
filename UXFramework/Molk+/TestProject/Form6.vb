Public Class Form6
    Private Sub Form6_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Controls.Remove(Me.CheckedListBox1)
        XpPanel2.Control = Me.CheckedListBox1
    End Sub

    Private Sub JumpNavigator1_Load(sender As Object, e As EventArgs) Handles JumpNavigator1.Load
        JumpNavigator1.IndexList = (From i As Integer In 20.Sequence Select CStr(i)).ToArray


    End Sub

    Private Sub JumpNavigator1_StartNavigation(Index As String) Handles JumpNavigator1.StartNavigation
        MsgBox(Index)
    End Sub
End Class