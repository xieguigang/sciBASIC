Public Class FormWelcome

    Sub New()

        ' 此调用是设计器所必需的。
        InitializeComponent()

        ' 在 InitializeComponent() 调用之后添加任何初始化。
        LabelTitle.Text = "Install sciBASIC#"

        Highlight(Label1)
    End Sub

    Protected Overrides Sub ButtonNext_Click(sender As Object, e As EventArgs)
        Call Microsoft.VisualBasic.Parallel.RunTask(AddressOf New FormProgress() With {.Location = Location}.ShowDialog)
        Me.Close()
    End Sub
End Class