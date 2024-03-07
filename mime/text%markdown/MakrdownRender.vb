Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Text

Public Class MakrdownRender

    Dim text As String

    Public Function Render(markdown As String)
        text = markdown.LineTokens.JoinBy(ASCII.LF)

        Call RunHeader()
        Call RunCodeSpan()
        Call RunBold()
        Call RunItalic()

        Return text
    End Function

    ReadOnly h5 As New Regex("[#]{5}.+", RegexOptions.Compiled Or RegexOptions.Multiline)
    ReadOnly h4 As New Regex("[#]{4}.+", RegexOptions.Compiled Or RegexOptions.Multiline)
    ReadOnly h3 As New Regex("[#]{3}.+", RegexOptions.Compiled Or RegexOptions.Multiline)
    ReadOnly h2 As New Regex("[#]{2}.+", RegexOptions.Compiled Or RegexOptions.Multiline)
    ReadOnly h1 As New Regex("[#]{1}.+", RegexOptions.Compiled Or RegexOptions.Multiline)

    Private Sub RunHeader()
        text = h5.Replace(text, Function(m) $"<h5>{TrimHeader(m.Value)}</h5>")
        text = h4.Replace(text, Function(m) $"<h4>{TrimHeader(m.Value)}</h4>")
        text = h3.Replace(text, Function(m) $"<h3>{TrimHeader(m.Value)}</h3>")
        text = h2.Replace(text, Function(m) $"<h2>{TrimHeader(m.Value)}</h2>")
        text = h1.Replace(text, Function(m) $"<h1>{TrimHeader(m.Value)}</h1>")
    End Sub

    Private Shared Function TrimHeader(s As String) As String
        Return Strings.Trim(s).Trim("#"c, " "c, ASCII.TAB)
    End Function

    ReadOnly codespan As New Regex("``.*?``", RegexOptions.Compiled Or RegexOptions.Multiline)

    Private Sub RunCodeSpan()
        text = codespan.Replace(text, Function(m) $"<code>{TrimCodeSpan(m.Value)}</code>")
    End Sub

    Private Shared Function TrimCodeSpan(s As String) As String
        Return Strings.Trim(s).Trim("`")
    End Function

    ReadOnly bold As New Regex("[*]{2}.+[*]{2}", RegexOptions.Compiled Or RegexOptions.Multiline)

    Private Sub RunBold()
        text = bold.Replace(text, Function(m) $"<strong>{TrimBold(m.Value)}</strong>")
    End Sub

    Private Shared Function TrimBold(s As String) As String
        Return Strings.Trim(s).Trim("*"c)
    End Function

    ReadOnly italic As New Regex("[*].+[*]", RegexOptions.Compiled Or RegexOptions.Multiline)

    Private Sub RunItalic()
        text = italic.Replace(text, Function(m) $"<i>{TrimBold(m.Value)}</i>")
    End Sub

End Class