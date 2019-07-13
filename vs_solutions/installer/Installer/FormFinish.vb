#Region "Microsoft.VisualBasic::3417689633639e994118b879a9e629ac, vs_solutions\installer\Installer\FormFinish.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    ' Class FormFinish
    ' 
    '     Sub: ButtonNext_Click, FormFinish_Load, LinkLabel1_LinkClicked, LinkLabel2_LinkClicked
    ' 
    ' /********************************************************************************/

#End Region

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

    Private Sub LinkLabel2_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles LinkLabel2.LinkClicked
        Process.Start("http://sciBASIC.NET")
    End Sub
End Class
