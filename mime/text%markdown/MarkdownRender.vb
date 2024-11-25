#Region "Microsoft.VisualBasic::45c57171fba87e0cdef1d0eca8d4738b, mime\text%markdown\MarkdownRender.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 410
    '    Code Lines: 297 (72.44%)
    ' Comment Lines: 25 (6.10%)
    '    - Xml Docs: 80.00%
    ' 
    '   Blank Lines: 88 (21.46%)
    '     File Size: 15.54 KB


    ' Class MarkdownRender
    ' 
    '     Constructor: (+2 Overloads) Sub New
    ' 
    '     Function: AnchorTag, AutoLink, ImageTag, Paragraph, TableBlock
    '               Transform, TrimBlockquote, TrimBold, TrimCodeSpan, TrimHeader
    '               TrimListItems, TrimOrderListItems
    ' 
    '     Sub: AutoParagraph, hideCodeBlock, hideCodeSpan, hideImage, hideUrl
    '          RunAutoLink, RunBold, RunCodeBlock, RunCodeSpan, RunHeader
    '          RunHr, RunImage, RunItalic, RunList, RunOrderList
    '          RunQuoteBlock, RunTable, RunUrl, SetImageUrlRouter
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Linq
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
        Call hideImage()
        Call hideUrl()

        Call RunAutoLink()

        ' markdown table contains a line of ------ between the thead and tbody
        ' this syntax may confused with the <hr/> syntax. so markdown table rendering
        ' should before the <hr/> line rendering.
        Call RunTable()
        Call RunHeader()
        Call RunHr()
        Call RunQuoteBlock()
        Call RunList()
        Call RunOrderList()

        Call RunBold()
        Call RunItalic()

        Call AutoParagraph()

        Call RunCodeSpan()
        Call RunCodeBlock()
        Call RunImage()
        Call RunUrl()

        Return render.Document(text)
    End Function

    Shared ReadOnly _newlinesLeadingTrailing As New Regex("^\n+|\n+\z", RegexOptions.Compiled)
    Shared ReadOnly _newlinesMultiple As New Regex("\n{2,}", RegexOptions.Compiled)

    Private Sub AutoParagraph()
        ' split on two or more newlines
        Dim grafs As String() = _newlinesMultiple.Split(_newlinesLeadingTrailing.Replace(text, ""))
        Dim graf_text As String

        Static check_tags As String() = {"p", "blockquote", "div", "ul", "ol", "h1", "h2", "h3", "h4", "h5"} _
            .Select(Iterator Function(tag) As IEnumerable(Of String)
                        Yield $"<{tag}>"
                        Yield $"</{tag}>"
                    End Function) _
            .IteratesALL _
            .ToArray

        For i As Integer = 0 To grafs.Length - 1
            graf_text = grafs(i).Trim(ASCII.LF, ASCII.CR, " "c, ASCII.TAB)

            If check_tags.Any(Function(prefix) graf_text.StartsWith(prefix)) Then
                Continue For
            End If

            grafs(i) = Paragraph(graf_text)
        Next

        text = grafs.JoinBy(vbLf & vbLf)
    End Sub

    Private Function Paragraph(text As String) As String
        Dim lines As String() = text.LineTokens

        If (lines.Length = 0) OrElse lines.All(Function(s) s.Trim.StringEmpty) Then
            Return text
        End If

        If lines.Length = 1 Then
            Static html_tag As New Regex("^[<]((h\d+)|(div)|(li)|([uo]l)|(table)|(p)|(pre)|(code))", RegexOptions.IgnoreCase Or RegexOptions.Compiled Or RegexOptions.Multiline)

            If html_tag.Match(lines(0)).Success Then
                Return text
            End If
        End If

        text = lines.JoinBy(vbLf)

        Return render.Paragraph(text, True)
    End Function

    Dim codespans As New Dictionary(Of String, String)
    Dim codeblocks As New Dictionary(Of String, String)
    Dim images As New Dictionary(Of String, String)
    Dim urls As New Dictionary(Of String, String)
    Dim autoUrls As New Dictionary(Of String, String)

    ''' <summary>
    ''' due to the reason of some image file name pattern may be mis-interpretered as
    ''' the markdown syntax, this will case the in-correct url, example as: 
    ''' ``/images/file_name_data.png`` may be interpreted as ``/images/file&lt;em>name&lt;/em>data.png``. 
    ''' so we needs hide the image span at first.
    ''' </summary>
    Private Sub hideImage()
        Dim hash As Integer = 1
        Dim key As String

        Call images.Clear()

        For Each m As Match In image.Matches(text)
            key = $";;;image;;{hash}"
            images(key) = m.Value
            hash += 1
            text = text.Replace(m.Value, key)
        Next
    End Sub

    Private Sub hideUrl()
        Dim hash As Integer = 1
        Dim key As String

        Call urls.Clear()

        For Each m As Match In url.Matches(text)
            key = $";;;url;;{hash}"
            urls(key) = m.Value
            hash += 1
            text = text.Replace(m.Value, key)
        Next
    End Sub

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

    ReadOnly table As New Regex("([|].+[|]\n)+", RegexOptions.Compiled Or RegexOptions.Multiline)

    Private Sub RunTable()
        text = table.Replace(text, Function(m) TableBlock(m.Value))
    End Sub

    Private Function TableBlock(s As String) As String
        Dim lines As String() = s.LineTokens
        Dim headers As String() = lines(0) _
            .Trim("|"c) _
            .Split("|"c) _
            .Select(AddressOf Strings.Trim) _
            .ToArray
        Dim bodyRows = lines.Skip(2) _
            .Select(Function(line)
                        Return line.Trim("|"c) _
                            .Split("|"c) _
                            .Select(AddressOf Strings.Trim) _
                            .ToArray
                    End Function)

        Return render.Table(headers, bodyRows)
    End Function

    ReadOnly url As New Regex("\[.*?\]\(.*?\)", RegexOptions.Compiled Or RegexOptions.Multiline)
    ReadOnly auto_link As New Regex("[<][^>^\s]{2,}[>]", RegexOptions.Compiled Or RegexOptions.Multiline)
    ReadOnly url_link As New Regex("[^""^']" & UrlLinkPattern & "[^""^']", RegexICMul Or RegexOptions.Compiled)

    Private Sub RunUrl()
        For Each link In urls.Reverse
            text = text.Replace(link.Key, AnchorTag(link.Value))
        Next

        ' restore auto link
        For Each link In autoUrls.Reverse
            text = text.Replace(link.Key, link.Value)
        Next
    End Sub

    ''' <summary>
    ''' Create url and hide it
    ''' </summary>
    ''' <remarks>
    ''' for avoid some un-expected markdown format bug, example as /?q=__a__ maybe transform as /?q=&lt;em>a&lt;/em>
    ''' </remarks>
    Private Sub RunAutoLink()
        Dim offset As Integer = 1

        autoUrls.Clear()
        text = url_link.Replace(text,
            evaluator:=Function(m)
                           Dim subtext = m.Value
                           Dim a$ = subtext.First
                           Dim b$ = subtext.Last
                           Dim offset1 As Integer = If(a.StringEmpty(), 1, 0)
                           Dim offset2 As Integer = If(b.StringEmpty, subtext.Length - 2, subtext.Length - offset1)
                           Dim url As String = subtext.Substring(offset1, offset2)
                           Dim link = If(a.StringEmpty, a, "") & render.AnchorLink(url, url, url) & If(b.StringEmpty, b, "")
                           Dim hash = $";;;auto_url+{offset}xxx;;;"

                           offset += 1
                           autoUrls(hash) = link

                           Return hash
                       End Function)
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
        For Each img In images.Reverse
            text = text.Replace(img.Key, ImageTag(img.Value))
        Next
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

    ReadOnly h6 As New Regex("^[#]{6}.+$", RegexOptions.Compiled Or RegexOptions.Multiline)
    ReadOnly h5 As New Regex("^[#]{5}.+$", RegexOptions.Compiled Or RegexOptions.Multiline)
    ReadOnly h4 As New Regex("^[#]{4}.+$", RegexOptions.Compiled Or RegexOptions.Multiline)
    ReadOnly h3 As New Regex("^[#]{3}.+$", RegexOptions.Compiled Or RegexOptions.Multiline)
    ReadOnly h2 As New Regex("^[#]{2}.+$", RegexOptions.Compiled Or RegexOptions.Multiline)
    ReadOnly h1 As New Regex("^[#]{1}.+$", RegexOptions.Compiled Or RegexOptions.Multiline)

    Private Sub RunHeader()
        text = h6.Replace(text, Function(m) vbLf & render.Header(TrimHeader(m.Value), 6) & vbLf)
        text = h5.Replace(text, Function(m) vbLf & render.Header(TrimHeader(m.Value), 5) & vbLf)
        text = h4.Replace(text, Function(m) vbLf & render.Header(TrimHeader(m.Value), 4) & vbLf)
        text = h3.Replace(text, Function(m) vbLf & render.Header(TrimHeader(m.Value), 3) & vbLf)
        text = h2.Replace(text, Function(m) vbLf & render.Header(TrimHeader(m.Value), 2) & vbLf)
        text = h1.Replace(text, Function(m) vbLf & render.Header(TrimHeader(m.Value), 1) & vbLf)
    End Sub

    Private Shared Function TrimHeader(s As String) As String
        Return Strings.Trim(s).Trim(ASCII.CR, ASCII.LF, "#"c, " "c, ASCII.TAB)
    End Function

    ReadOnly codespan As New Regex("``.*?``", RegexOptions.Compiled Or RegexOptions.Multiline)

    Private Sub RunCodeSpan()
        For Each hashVal In codespans.Reverse
            text = text.Replace(hashVal.Key, render.CodeSpan(TrimCodeSpan(hashVal.Value)))
        Next
    End Sub

    Private Sub RunCodeBlock()
        For Each hashVal In codeblocks.Reverse
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

    ReadOnly italic As New Regex("([*].*?[*])|([_].*?[_])", RegexOptions.Compiled Or RegexOptions.Multiline)

    Private Sub RunItalic()
        text = italic.Replace(text, Function(m)
                                        Return render.Italic(TrimBold(m.Value))
                                    End Function)
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

        Return vbLf & lines.JoinBy(vbLf) & vbLf & vbLf
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
            Yield orderPrefix.Replace(si.Trim, "").Trim
        Next
    End Function
End Class
