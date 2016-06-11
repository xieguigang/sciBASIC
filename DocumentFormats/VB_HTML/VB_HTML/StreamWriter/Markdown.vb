Imports System.Runtime.CompilerServices

Namespace StreamWriter

    Public Module Markdown

        <Extension>
        Public Function TextMarkdown(link As Hyperlink) As String
            If String.IsNullOrEmpty(link.Title) Then
                Return $"[{link.Text}]({link.Links})"
            Else
                Return $"[{link.Text}]({link.Links} ""{link.Title}"")"
            End If
        End Function
    End Module
End Namespace

