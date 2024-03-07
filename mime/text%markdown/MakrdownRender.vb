Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Text

Public Class MakrdownRender

    Dim text As String

    Public Function Render(markdown As String) As String
        text = markdown.LineTokens.JoinBy(ASCII.LF)

        Call hideCodeBlock()
        Call hideCodeSpan()

        Call RunHeader()
        Call RunBold()
        Call RunItalic()
        Call RunQuoteBlock()

        Call RunCodeSpan()
        Call RunCodeBlock()

        Return text
    End Function

    Dim codespans As New Dictionary(Of String, String)
    Dim codeblocks As New Dictionary(Of String, String)

    Private Sub hideCodeSpan()
        Dim hash As Integer = 1
        Dim key As String

        Call codespans.Clear()

        For Each m As Match In codespan.Matches(text)
            key = $";__codespan_{hash}"
            codespans(key) = m.Value
            hash += 1
            text = text.Replace(m.Value, key)
        Next
    End Sub

    ReadOnly codeblock As New Regex("```.+```", RegexOptions.Compiled Or RegexOptions.Singleline)

    Private Sub hideCodeBlock()
        Dim hash As Integer = 1
        Dim key As String

        Call codeblocks.Clear()

        For Each m As Match In codeblock.Matches(text)
            key = $";__codeblock_{hash}"
            codeblocks(key) = m.Value
            hash += 1
            text = text.Replace(m.Value, key)
        Next
    End Sub

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
        For Each hashVal In codespans
            text = text.Replace(hashVal.Key, $"<code>{TrimCodeSpan(hashVal.Value)}</code>")
        Next
    End Sub

    Private Sub RunCodeBlock()
        For Each hashVal In codeblocks
            text = text.Replace(hashVal.Key, $"<pre><code>{TrimCodeSpan(hashVal.Value)}</code></pre>")
        Next
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

    ReadOnly quote As New Regex("\n([>].+)+\n", RegexOptions.Compiled Or RegexOptions.Singleline)

    Private Sub RunQuoteBlock()
        text = quote.Replace(text, Function(m) $"<blockquote>{TrimBlockquote(m.Value)}</blockquote>")
    End Sub

    Private Shared Function TrimBlockquote(s As String) As String
        Dim lines = s.LineTokens
        lines = lines.Select(Function(si) If(si = "", "", si.Substring(1).Trim)).ToArray
        Return lines.JoinBy("<br />")
    End Function

End Class