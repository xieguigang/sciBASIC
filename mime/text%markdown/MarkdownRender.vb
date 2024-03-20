Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Text

Public Class MarkdownRender

    Dim text As String
    Dim render As Render

    ''' <summary>
    ''' default rendering to html by <see cref="HtmlRender"/>
    ''' </summary>
    Sub New()
        Call Me.New(New HtmlRender)
    End Sub

    Sub New(render As Render)
        Me.render = render
    End Sub

    Public Sub SetImageUrlRouter(router As Func(Of String, String))
        render.SetImageUrlRouter(router)
    End Sub

    ''' <summary>
    ''' transform markdown document text to another document format
    ''' </summary>
    ''' <param name="markdown"></param>
    ''' <returns></returns>
    Public Function Transform(markdown As String) As String
        text = markdown.LineTokens.JoinBy(ASCII.LF)

        Call hideCodeBlock()
        Call hideCodeSpan()

        Call RunAutoLink()
        Call RunHeader()
        Call RunHr()
        Call RunQuoteBlock()
        Call RunList()
        Call RunOrderList()
        Call RunImage()
        Call RunUrl()
        Call RunTable()

        Call RunBold()
        Call RunItalic()

        Call RunCodeSpan()
        Call RunCodeBlock()

        Return render.Document(text)
    End Function

    Dim codespans As New Dictionary(Of String, String)
    Dim codeblocks As New Dictionary(Of String, String)

    Private Sub hideCodeSpan()
        Dim hash As Integer = 1
        Dim key As String

        Call codespans.Clear()

        For Each m As Match In codespan.Matches(text)
            key = $";;;codespan;;{hash}"
            codespans(key) = m.Value
            hash += 1
            text = text.Replace(m.Value, key)
        Next
    End Sub

    ReadOnly codeblock As New Regex("\n\s*[`]{3,}.*?\n\s*[`]{3,}", RegexOptions.Compiled Or RegexOptions.Singleline)

    Private Sub hideCodeBlock()
        Dim hash As Integer = 1
        Dim key As String

        Call codeblocks.Clear()

        For Each m As Match In codeblock.Matches(text)
            key = $";;;codeblock;;{hash}"
            codeblocks(key) = m.Value
            hash += 1
            text = text.Replace(m.Value, key)
        Next
    End Sub

    ReadOnly hr As New Regex("^\s*([*]{3,})|([-]{3,})|([_]{3,})\s*$", RegexOptions.Compiled Or RegexOptions.Multiline)

    Private Sub RunHr()
        text = hr.Replace(text, Function(m) render.HorizontalLine)
    End Sub

    ReadOnly table As New Regex("([|].+[|]\n)+", RegexOptions.Compiled Or RegexOptions.Singleline)

    Private Sub RunTable()
        text = table.Replace(text, Function(m) TableBlock(m.Value))
    End Sub

    Private Function TableBlock(s As String) As String
        Dim lines = s.LineTokens
        Dim headers = lines(0).Split("|"c).Select(AddressOf Strings.Trim).ToArray
        Dim bodyRows = lines.Skip(2) _
            .Select(Function(line)
                        Return line.Split("|"c).Select(AddressOf Strings.Trim).ToArray
                    End Function)

        Return render.Table(headers, bodyRows)
    End Function

    ReadOnly url As New Regex("\[.*?\]\(.*?\)", RegexOptions.Compiled Or RegexOptions.Multiline)
    ReadOnly auto_link As New Regex("[<][^>^\s]{2,}[>]", RegexOptions.Compiled Or RegexOptions.Multiline)

    Private Sub RunUrl()
        text = url.Replace(text, Function(m) AnchorTag(m.Value))
    End Sub

    Private Sub RunAutoLink()
        text = auto_link.Replace(text, Function(m) AutoLink(m.Value))
    End Sub

    Private Function AutoLink(s As String) As String
        Dim url As String = s.GetStackValue("<", ">")
        Return render.AnchorLink(url, url, url)
    End Function

    Private Function AnchorTag(s As String) As String
        Static alt_r As New Regex("\[.*?\]", RegexOptions.Compiled Or RegexOptions.Multiline)
        Static url_r As New Regex("\(.*?\)", RegexOptions.Compiled Or RegexOptions.Multiline)

        Dim alt As String = alt_r.Match(s).Value.GetStackValue("[", "]")
        Dim url As String = url_r.Match(s).Value.GetStackValue("(", ")").GetStackValue("<", ">")
        Dim title_value = url.GetTagValue

        If title_value.Name.StringEmpty Then
            Return render.AnchorLink(url, alt, alt)
        Else
            Return render.AnchorLink(title_value.Name, alt, title_value.Value.Trim("'"c, """"c, " "c))
        End If
    End Function

    ReadOnly image As New Regex("[!]\[.*?\]\(.*?\)", RegexOptions.Compiled Or RegexOptions.Multiline)

    Private Sub RunImage()
        text = image.Replace(text, Function(m) ImageTag(m.Value))
    End Sub

    Private Function ImageTag(s As String) As String
        Static alt_r As New Regex("[!]\[.*?\]", RegexOptions.Compiled Or RegexOptions.Multiline)
        Static url_r As New Regex("\(.*?\)", RegexOptions.Compiled Or RegexOptions.Multiline)

        Dim alt As String = alt_r.Match(s).Value.GetStackValue("[", "]")
        Dim url As String = url_r.Match(s).Value.GetStackValue("(", ")").GetStackValue("<", ">")
        Dim title_value = url.GetTagValue

        If title_value.Name.StringEmpty Then
            Return render.Image(url, alt, alt)
        Else
            Return render.Image(title_value.Name, alt, title_value.Value.Trim("'"c, """"c, " "c))
        End If
    End Function

    ReadOnly h6 As New Regex("[#]{6}.+", RegexOptions.Compiled Or RegexOptions.Multiline)
    ReadOnly h5 As New Regex("[#]{5}.+", RegexOptions.Compiled Or RegexOptions.Multiline)
    ReadOnly h4 As New Regex("[#]{4}.+", RegexOptions.Compiled Or RegexOptions.Multiline)
    ReadOnly h3 As New Regex("[#]{3}.+", RegexOptions.Compiled Or RegexOptions.Multiline)
    ReadOnly h2 As New Regex("[#]{2}.+", RegexOptions.Compiled Or RegexOptions.Multiline)
    ReadOnly h1 As New Regex("[#]{1}.+", RegexOptions.Compiled Or RegexOptions.Multiline)

    Private Sub RunHeader()
        text = h6.Replace(text, Function(m) render.Header(TrimHeader(m.Value), 6))
        text = h5.Replace(text, Function(m) render.Header(TrimHeader(m.Value), 5))
        text = h4.Replace(text, Function(m) render.Header(TrimHeader(m.Value), 4))
        text = h3.Replace(text, Function(m) render.Header(TrimHeader(m.Value), 3))
        text = h2.Replace(text, Function(m) render.Header(TrimHeader(m.Value), 2))
        text = h1.Replace(text, Function(m) render.Header(TrimHeader(m.Value), 1))
    End Sub

    Private Shared Function TrimHeader(s As String) As String
        Return Strings.Trim(s).Trim("#"c, " "c, ASCII.TAB)
    End Function

    ReadOnly codespan As New Regex("``.*?``", RegexOptions.Compiled Or RegexOptions.Multiline)

    Private Sub RunCodeSpan()
        For Each hashVal In codespans
            text = text.Replace(hashVal.Key, render.CodeSpan(TrimCodeSpan(hashVal.Value)))
        Next
    End Sub

    Private Sub RunCodeBlock()
        For Each hashVal In codeblocks
            Dim code_block As String() = hashVal.Value.Trim(ASCII.CR, ASCII.LF, ASCII.TAB, " "c).LineTokens
            Dim first = code_block.First
            Dim code_text As String = code_block _
                .Skip(1) _
                .Take(code_block.Length - 2) _
                .JoinBy(vbLf)
            Dim lang As String = first.Trim(" "c, "`"c)

            text = text.Replace(hashVal.Key, render.CodeBlock(code_text, lang))
        Next
    End Sub

    Private Shared Function TrimCodeSpan(s As String) As String
        Return Strings.Trim(s).Trim("`")
    End Function

    ReadOnly bold As New Regex("([*]{2}.+[*]{2})|([_]{2}.+[_]{2})", RegexOptions.Compiled Or RegexOptions.Multiline)

    Private Sub RunBold()
        text = bold.Replace(text, Function(m) render.Bold(TrimBold(m.Value)))
    End Sub

    Private Shared Function TrimBold(s As String) As String
        Return Strings.Trim(s).Trim("*"c, "_"c)
    End Function

    ReadOnly italic As New Regex("([*].+[*])|([_].+[_])", RegexOptions.Compiled Or RegexOptions.Multiline)

    Private Sub RunItalic()
        text = italic.Replace(text, Function(m) render.Italic(TrimBold(m.Value)))
    End Sub

    ReadOnly quote As New Regex("(\n[>][^\n]*)+", RegexOptions.Compiled Or RegexOptions.Singleline)

    Private Sub RunQuoteBlock()
        text = quote.Replace(text, Function(m) render.BlockQuote(TrimBlockquote(m.Value)))
    End Sub

    Private Shared Function TrimBlockquote(s As String) As String
        Dim lines = s.Trim(ASCII.LF, ASCII.CR, " "c).LineTokens
        lines = lines _
            .Select(Function(si) si.Substring(1).Trim) _
            .ToArray
        Return lines.JoinBy(vbLf)
    End Function

    ReadOnly list1 As New Regex("(\n[\+]\s([^\n])+)+", RegexOptions.Compiled Or RegexOptions.Singleline)
    ReadOnly list2 As New Regex("(\n[\-]\s([^\n])+)+", RegexOptions.Compiled Or RegexOptions.Singleline)
    ReadOnly list3 As New Regex("(\n[\*]\s([^\n])+)+", RegexOptions.Compiled Or RegexOptions.Singleline)

    Private Sub RunList()
        text = list1.Replace(text, Function(m) render.List(TrimListItems(m.Value), False))
        text = list2.Replace(text, Function(m) render.List(TrimListItems(m.Value), False))
        text = list3.Replace(text, Function(m) render.List(TrimListItems(m.Value), False))
    End Sub

    Private Shared Iterator Function TrimListItems(s As String) As IEnumerable(Of String)
        s = s.Trim(ASCII.LF, ASCII.CR, " "c)

        For Each si As String In s.LineTokens
            Yield si.Trim.Substring(1).Trim
        Next
    End Function

    ReadOnly orderList As New Regex("(\n\d+\.\s[^\n]+)+", RegexOptions.Compiled Or RegexOptions.Singleline)
    ReadOnly orderPrefix As New Regex("^\d+\.", RegexOptions.Compiled Or RegexOptions.Multiline)

    Private Sub RunOrderList()
        text = orderList.Replace(text, Function(m) render.List(TrimOrderListItems(m.Value), True))
    End Sub

    Private Iterator Function TrimOrderListItems(s As String) As IEnumerable(Of String)
        s = s.Trim(ASCII.LF, ASCII.CR, " "c)

        For Each si As String In s.LineTokens
            Yield orderPrefix.Replace(si.Trim, "")
        Next
    End Function
End Class