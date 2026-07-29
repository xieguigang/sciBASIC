Imports Microsoft.VisualBasic.MIME.text.markdown

Public Class FormMarkWeb

    ReadOnly render As New MarkdownRender

    Private Sub ToolStripButton1_Click(sender As Object, e As EventArgs) Handles ToolStripButton1.Click
        Using file As New OpenFileDialog With {.Filter = "Markdown 文档(*.md)|*.md"}
            If file.ShowDialog = DialogResult.OK Then
                Dim md As String = file.FileName.ReadAllText
                Dim html As String = render.Transform(md)

                Call TextBox1.Clear()
                Call TextBox1.AppendText(md)

                Call WebViewLoader.NavigateToLargeString(WebView21, html)
            End If
        End Using
    End Sub

    Private Async Sub FormMarkWeb_Load(sender As Object, e As EventArgs) Handles Me.Load
        Await WebViewLoader.Init(WebView21)
    End Sub

    Private Sub TabControl1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles TabControl1.SelectedIndexChanged
        If TabControl1.SelectedTab Is TabPage1 Then
            Call WebViewLoader.NavigateToLargeString(WebView21, render.Transform(TextBox1.Text))
        End If
    End Sub
End Class
