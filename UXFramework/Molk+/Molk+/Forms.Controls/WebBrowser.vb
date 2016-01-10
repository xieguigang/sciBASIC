Imports System.ComponentModel

Namespace Windows.Forms.Controls

    Public Class WebBrowser

        Private Sub WebBrowser1_DocumentCompleted(sender As Object, e As WebBrowserDocumentCompletedEventArgs) Handles WebBrowser1.DocumentCompleted
            TextBox1.Text = WebBrowser1.Url.ToString
        End Sub

        Private Sub Browser_Load(sender As Object, e As EventArgs) Handles Me.Load
            Call WebBrowser1.Navigate(get_HomePage)
        End Sub

        Protected MustOverride Function get_HomePage() As String

        Private Sub TextBox1_KeyDown(sender As Object, e As KeyEventArgs) Handles TextBox1.KeyDown
            If e.KeyCode = System.Windows.Forms.Keys.Enter Then
                Call WebBrowser1.Navigate(TextBox1.Text)
            End If
        End Sub

        Public Sub GotoHomePage()
            Call WebBrowser1.Navigate(get_HomePage)
        End Sub
    End Class
End Namespace