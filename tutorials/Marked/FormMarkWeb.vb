Imports Microsoft.VisualBasic.MIME.text.markdown

Public Class FormMarkWeb

    ReadOnly render As New MarkdownRender

    Private Sub ToolStripButton1_Click(sender As Object, e As EventArgs) Handles ToolStripButton1.Click
        Using file As New OpenFileDialog With {.Filter = "Markdown 文档(*.md)|*.md"}
            If file.ShowDialog = DialogResult.OK Then
                Call WebViewLoader.NavigateToLargeString(WebView21, render.Transform(file.FileName.ReadAllText))
            End If
        End Using
    End Sub

    Private Async Sub FormMarkWeb_Load(sender As Object, e As EventArgs) Handles Me.Load
        Await WebViewLoader.Init(WebView21)
    End Sub
End Class
