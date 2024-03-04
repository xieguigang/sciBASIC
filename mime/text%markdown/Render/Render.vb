Imports System.Text.RegularExpressions

Public MustInherit Class Render

    Protected markdown As MarkdownHTML

    Protected Shared ReadOnly _leadingWhitespace As New Regex("^[ ]*", RegexOptions.Compiled)

    Public MustOverride Function Paragraph(text As String, CreateParagraphs As Boolean) As String
    Public MustOverride Function Document(text As String) As String

End Class
