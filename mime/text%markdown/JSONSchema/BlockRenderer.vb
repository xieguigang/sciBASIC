Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Linq

Namespace JSONSchema

    ''' <summary>
    ''' markdown block rendering extensions, converts a single <see cref="Block"/>
    ''' object into its markdown or html text fragment.
    ''' </summary>
    Public Module BlockRenderer

        ''' <summary>
        ''' escape the special html characters in the given text fragment
        ''' </summary>
        ''' <param name="text"></param>
        ''' <returns></returns>
        Private Function HtmlEncode(text As String) As String
            If text Is Nothing Then
                Return ""
            End If

            Return text _
                .Replace("&", "&amp;") _
                .Replace("<", "&lt;") _
                .Replace(">", "&gt;") _
                .Replace("""", "&quot;")
        End Function

        ''' <summary>
        ''' get the html ``text-align`` style value from the table alignment token.
        ''' </summary>
        ''' <param name="alignments"></param>
        ''' <param name="i"></param>
        ''' <returns></returns>
        Private Function AlignStyle(alignments As String(), i As Integer) As String
            If alignments Is Nothing OrElse i >= alignments.Length Then
                Return ""
            End If

            Select Case Strings.LCase(alignments(i))
                Case "left" : Return " style=""text-align:left;"""
                Case "center" : Return " style=""text-align:center;"""
                Case "right" : Return " style=""text-align:right;"""
                Case Else : Return ""
            End Select
        End Function

        <Extension>
        Public Function ToMarkdownBlock(block As Block) As String
            Select Case Strings.LCase(block.type)
                Case "heading", "h"
                    Dim level As Integer = If(block.level <= 0, 1, block.level)
                    Return New String("#"c, level) & " " & block.content
                Case "paragraph", "p", "html", "raw"
                    Return block.content
                Case "code"
                    If String.IsNullOrEmpty(block.language) Then
                        Return "```" & vbCrLf & block.content & vbCrLf & "```"
                    Else
                        Return "```" & block.language & vbCrLf & block.content & vbCrLf & "```"
                    End If
                Case "list", "li"
                    Return renderListMarkdown(block)
                Case "blockquote"
                    Return renderBlockquoteMarkdown(block)
                Case "table"
                    Return renderTableMarkdown(block)
                Case "hr", "horizontal-rule", "horizontalrule", "thematic-break"
                    Return "---"
                Case "image", "img"
                    If String.IsNullOrEmpty(block.title) Then
                        Return "![" & block.alt & "](" & block.url & ")"
                    Else
                        Return "![" & block.alt & "](" & block.url & " """ & block.title & """)"
                    End If
                Case Else
                    Return block.content
            End Select
        End Function

        <Extension>
        Public Function ToHtmlBlock(block As Block) As String
            Select Case Strings.LCase(block.type)
                Case "heading", "h"
                    Dim level As Integer = If(block.level <= 0, 1, block.level)
                    Return "<h" & level & ">" & HtmlEncode(block.content) & "</h" & level & ">"
                Case "paragraph", "p"
                    Return "<p>" & HtmlEncode(block.content) & "</p>"
                Case "code"
                    Dim cls As String = If(String.IsNullOrEmpty(block.language), "", " class=""language-" & block.language & """")
                    Return "<pre><code" & cls & ">" & HtmlEncode(block.content) & "</code></pre>"
                Case "list", "li"
                    Return renderListHtml(block)
                Case "blockquote"
                    Return "<blockquote>" & HtmlEncode(block.content) & "</blockquote>"
                Case "table"
                    Return renderTableHtml(block)
                Case "hr", "horizontal-rule", "horizontalrule", "thematic-break"
                    Return "<hr />"
                Case "image", "img"
                    Dim titleAttr As String = If(String.IsNullOrEmpty(block.title), "", " title=""" & HtmlEncode(block.title) & """")
                    Return "<img src=""" & HtmlEncode(block.url) & """ alt=""" & HtmlEncode(block.alt) & """" & titleAttr & " />"
                Case "html", "raw"
                    ' raw html block, output as-is without encoding
                    Return block.content
                Case Else
                    Return "<p>" & HtmlEncode(block.content) & "</p>"
            End Select
        End Function

        Private Function renderListMarkdown(block As Block) As String
            Dim sb As New StringBuilder

            If block.ordered Then
                Dim i As Integer = 1

                For Each item As String In block.items.SafeQuery
                    Call sb.AppendLine(i & ". " & item)
                    i += 1
                Next
            Else
                For Each item As String In block.items.SafeQuery
                    Call sb.AppendLine("- " & item)
                Next
            End If

            Return sb.ToString.TrimEnd(vbCrLf.ToCharArray)
        End Function

        Private Function renderBlockquoteMarkdown(block As Block) As String
            Dim lines As String() = block.content.Split(New String() {vbCrLf, vbLf, vbCr}, StringSplitOptions.None)

            Return "> " & lines.JoinBy(vbCrLf & "> ")
        End Function

        Private Function renderTableMarkdown(block As Block) As String
            Dim sb As New StringBuilder

            Call sb.AppendLine("| " & block.headers.JoinBy(" | ") & " |")
            Call sb.AppendLine("| " & block.headers _
                .Select(Function(h, i) alignMarkdownToken(block.alignments, i)) _
                .JoinBy(" | ") & " |")

            For Each row As String() In block.rows.SafeQuery
                Call sb.AppendLine("| " & row.JoinBy(" | ") & " |")
            Next

            Return sb.ToString.TrimEnd(vbCrLf.ToCharArray)
        End Function

        Private Function alignMarkdownToken(alignments As String(), i As Integer) As String
            If alignments Is Nothing OrElse i >= alignments.Length Then
                Return "---"
            End If

            Select Case Strings.LCase(alignments(i))
                Case "left" : Return ":---"
                Case "center" : Return ":---:"
                Case "right" : Return "---:"
                Case Else : Return "---"
            End Select
        End Function

        Private Function renderListHtml(block As Block) As String
            Dim sb As New StringBuilder
            Dim tag As String = If(block.ordered, "ol", "ul")

            Call sb.AppendLine("<" & tag & ">")

            For Each item As String In block.items.SafeQuery
                Call sb.AppendLine("  <li>" & HtmlEncode(item) & "</li>")
            Next

            Call sb.AppendLine("</" & tag & ">")

            Return sb.ToString.TrimEnd(vbCrLf.ToCharArray)
        End Function

        Private Function renderTableHtml(block As Block) As String
            Dim sb As New StringBuilder

            Call sb.AppendLine("<table>")
            Call sb.AppendLine("  <thead>")
            Call sb.AppendLine("    <tr>")

            For i As Integer = 0 To block.headers.Length - 1
                Call sb.AppendLine("      <th" & AlignStyle(block.alignments, i) & ">" & HtmlEncode(block.headers(i)) & "</th>")
            Next

            Call sb.AppendLine("    </tr>")
            Call sb.AppendLine("  </thead>")
            Call sb.AppendLine("  <tbody>")

            For Each row As String() In block.rows.SafeQuery
                Call sb.AppendLine("    <tr>")

                For i As Integer = 0 To row.Length - 1
                    Call sb.AppendLine("      <td" & AlignStyle(block.alignments, i) & ">" & HtmlEncode(row(i)) & "</td>")
                Next

                Call sb.AppendLine("    </tr>")
            Next

            Call sb.AppendLine("  </tbody>")
            Call sb.AppendLine("</table>")

            Return sb.ToString.TrimEnd(vbCrLf.ToCharArray)
        End Function

    End Module
End Namespace