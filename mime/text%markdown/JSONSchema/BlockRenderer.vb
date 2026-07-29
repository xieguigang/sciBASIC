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

        ''' <summary>
        ''' 返回两个整型的最小值（避免与 <see cref="Microsoft.VisualBasic.Linq"/> 中
        ''' 的同名扩展方法冲突，因此不在此模块内使用 ``Math.Min``）。
        ''' </summary>
        Private Function MinInt(a As Integer, b As Integer) As Integer
            Return If(a < b, a, b)
        End Function

        <Extension>
        Public Function ToMarkdownBlock(block As Block) As String
            Select Case Strings.LCase(block.type)
                Case "heading", "h"
                    Dim level As Integer = block.level

                    If level < 1 Then level = 1
                    If level > 6 Then level = 6

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
                Case "link", "a"
                    Return renderLinkMarkdown(block)
                Case "math", "equation", "tex", "latex"
                    Return renderMathMarkdown(block)
                Case "tasklist", "tasks", "todo"
                    Return renderTaskListMarkdown(block)
                Case "footnote", "note"
                    Return renderFootnoteMarkdown(block)
                Case "deflist", "definition", "dl"
                    Return renderDefListMarkdown(block)
                Case Else
                    Return block.content
            End Select
        End Function

        <Extension>
        Public Function ToHtmlBlock(block As Block) As String
            Select Case Strings.LCase(block.type)
                Case "heading", "h"
                    Dim level As Integer = block.level

                    If level < 1 Then level = 1
                    If level > 6 Then level = 6

                    Return "<h" & level & ">" & HtmlEncode(block.content) & "</h" & level & ">"
                Case "paragraph", "p"
                    Return "<p>" & HtmlEncode(block.content) & "</p>"
                Case "code"
                    Dim cls As String = If(String.IsNullOrEmpty(block.language), "", " class=""language-" & block.language & """")
                    Return "<pre><code" & cls & ">" & HtmlEncode(block.content) & "</code></pre>"
                Case "list", "li"
                    Return renderListHtml(block)
                Case "blockquote"
                    Return renderBlockquoteHtml(block)
                Case "table"
                    Return renderTableHtml(block)
                Case "hr", "horizontal-rule", "horizontalrule", "thematic-break"
                    Return "<hr />"
                Case "image", "img"
                    Dim titleAttr As String = If(String.IsNullOrEmpty(block.title), "", " title=""" & HtmlEncode(block.title) & """")
                    Return "<img src=""" & HtmlEncode(block.url) & """ alt=""" & HtmlEncode(block.alt) & """" & titleAttr & " />"
                Case "link", "a"
                    Return renderLinkHtml(block)
                Case "math", "equation", "tex", "latex"
                    Return renderMathHtml(block)
                Case "tasklist", "tasks", "todo"
                    Return renderTaskListHtml(block)
                Case "footnote", "note"
                    Return renderFootnoteHtml(block)
                Case "deflist", "definition", "dl"
                    Return renderDefListHtml(block)
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
            Dim ncols As Integer = If(block.headers Is Nothing, 0, block.headers.Length)

            If ncols > 0 Then
                Call sb.AppendLine("| " & block.headers.JoinBy(" | ") & " |")
                Call sb.AppendLine("| " & block.headers _
                    .Select(Function(h, i) alignMarkdownToken(block.alignments, i)) _
                    .JoinBy(" | ") & " |")
            End If

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
            Dim ncols As Integer = If(block.headers Is Nothing, 0, block.headers.Length)

            Call sb.AppendLine("<table>")

            If ncols > 0 Then
                Call sb.AppendLine("  <thead>")
                Call sb.AppendLine("    <tr>")

                For i As Integer = 0 To ncols - 1
                    Call sb.AppendLine("      <th" & AlignStyle(block.alignments, i) & ">" & HtmlEncode(block.headers(i)) & "</th>")
                Next

                Call sb.AppendLine("    </tr>")
                Call sb.AppendLine("  </thead>")
            End If

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

        Private Function renderBlockquoteHtml(block As Block) As String
            Dim lines As String() = block.content.Split(New String() {vbCrLf, vbLf, vbCr}, StringSplitOptions.None)
            Dim inner As String = lines _
                .Select(Function(l) "<p>" & HtmlEncode(l) & "</p>") _
                .JoinBy(vbCrLf)

            Return "<blockquote>" & vbCrLf & inner & vbCrLf & "</blockquote>"
        End Function

        ' -----------------------------------------------------------------
        ' math / equation
        ' -----------------------------------------------------------------
        Private Function renderMathMarkdown(block As Block) As String
            If String.IsNullOrEmpty(block.language) Then
                Return "$$" & vbCrLf & block.content & vbCrLf & "$$"
            Else
                Return "```" & block.language & vbCrLf & block.content & vbCrLf & "```"
            End If
        End Function

        Private Function renderMathHtml(block As Block) As String
            ' 保留 LaTeX 原字符，交由 KaTeX / MathJax 处理，不做 HTML 转义
            Return "<div class=""math"">" & "$$" & block.content & "$$" & "</div>"
        End Function

        ' -----------------------------------------------------------------
        ' link
        ' -----------------------------------------------------------------
        Private Function renderLinkMarkdown(block As Block) As String
            If String.IsNullOrEmpty(block.title) Then
                Return "[" & block.alt & "](" & block.url & ")"
            Else
                Return "[" & block.alt & "](" & block.url & " """ & block.title & """)"
            End If
        End Function

        Private Function renderLinkHtml(block As Block) As String
            Dim titleAttr As String = If(String.IsNullOrEmpty(block.title), "", " title=""" & HtmlEncode(block.title) & """")
            Return "<a href=""" & HtmlEncode(block.url) & """" & titleAttr & ">" & HtmlEncode(block.alt) & "</a>"
        End Function

        ' -----------------------------------------------------------------
        ' tasklist (GFM task list)
        ' -----------------------------------------------------------------
        Private Function renderTaskListMarkdown(block As Block) As String
            Dim sb As New StringBuilder
            Dim pos As Integer = 0

            For Each item As String In block.items.SafeQuery
                Dim mark As String = " "

                If Not block.checked Is Nothing AndAlso pos < block.checked.Length AndAlso block.checked(pos) Then
                    mark = "x"
                End If

                If block.ordered Then
                    Call sb.AppendLine((pos + 1) & ". [" & mark & "] " & item)
                Else
                    Call sb.AppendLine("- [" & mark & "] " & item)
                End If

                pos += 1
            Next

            Return sb.ToString.TrimEnd(vbCrLf.ToCharArray)
        End Function

        Private Function renderTaskListHtml(block As Block) As String
            Dim sb As New StringBuilder
            Dim tag As String = If(block.ordered, "ol", "ul")
            Dim pos As Integer = 0

            Call sb.AppendLine("<" & tag & " class=""task-list"">")

            For Each item As String In block.items.SafeQuery
                Dim isChecked As Boolean = Not block.checked Is Nothing AndAlso pos < block.checked.Length AndAlso block.checked(pos)
                Dim box As String = "<input type=""checkbox""" & If(isChecked, " checked", "") & " disabled />"

                Call sb.AppendLine("  <li>" & box & " " & HtmlEncode(item) & "</li>")
                pos += 1
            Next

            Call sb.AppendLine("</" & tag & ">")

            Return sb.ToString.TrimEnd(vbCrLf.ToCharArray)
        End Function

        ' -----------------------------------------------------------------
        ' footnote
        ' -----------------------------------------------------------------
        Private Function renderFootnoteMarkdown(block As Block) As String
            Return "[^" & block.id & "]: " & block.content
        End Function

        Private Function renderFootnoteHtml(block As Block) As String
            Return "<div class=""footnote"" id=""fn-" & HtmlEncode(block.id) & """>" & HtmlEncode(block.content) & "</div>"
        End Function

        ' -----------------------------------------------------------------
        ' deflist (definition list)
        ' -----------------------------------------------------------------
        Private Function renderDefListMarkdown(block As Block) As String
            Dim sb As New StringBuilder
            Dim n As Integer = MinInt(If(block.terms Is Nothing, 0, block.terms.Length), If(block.definitions Is Nothing, 0, block.definitions.Length))

            For i As Integer = 0 To n - 1
                Call sb.AppendLine(block.terms(i))
                Call sb.AppendLine(": " & block.definitions(i))
            Next

            Return sb.ToString.TrimEnd(vbCrLf.ToCharArray)
        End Function

        Private Function renderDefListHtml(block As Block) As String
            Dim sb As New StringBuilder
            Dim n As Integer = MinInt(If(block.terms Is Nothing, 0, block.terms.Length), If(block.definitions Is Nothing, 0, block.definitions.Length))

            Call sb.AppendLine("<dl>")

            For i As Integer = 0 To n - 1
                Call sb.AppendLine("  <dt>" & HtmlEncode(block.terms(i)) & "</dt>")
                Call sb.AppendLine("  <dd>" & HtmlEncode(block.definitions(i)) & "</dd>")
            Next

            Call sb.AppendLine("</dl>")

            Return sb.ToString.TrimEnd(vbCrLf.ToCharArray)
        End Function

    End Module
End Namespace