Imports System.Text
Imports Microsoft.VisualBasic.Linq

Namespace ApplicationServices.Report

    ''' <summary>
    ''' html render: converts the markdown content model <see cref="Block"/> into html text.
    ''' </summary>
    Public Class HtmlRender : Inherits ReportRender

        ''' <summary>
        ''' render the given <see cref="Block"/> document into html text, the generated html
        ''' text can be read from the <see cref="ReportRender.Output"/> property.
        ''' </summary>
        ''' <param name="report">the markdown content blocks of the target document</param>
        Public Overrides Sub Render(report As IEnumerable(Of Block))
            Call buffer.Clear()
            Call AppendBlocks(report.SafeQuery.Select(AddressOf RenderBlock))
        End Sub

        ''' <summary>
        ''' render a single <see cref="Block"/> into its html text fragment.
        ''' </summary>
        ''' <param name="block"></param>
        ''' <returns></returns>
        Public Function RenderBlock(block As Block) As String
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
                    Return renderList(block)
                Case "blockquote"
                    Return renderBlockquote(block)
                Case "table"
                    Return renderTable(block)
                Case "hr", "horizontal-rule", "horizontalrule", "thematic-break"
                    Return "<hr />"
                Case "image", "img"
                    Dim titleAttr As String = If(String.IsNullOrEmpty(block.title), "", " title=""" & HtmlEncode(block.title) & """")
                    Return "<img src=""" & HtmlEncode(block.url) & """ alt=""" & HtmlEncode(block.alt) & """" & titleAttr & " />"
                Case "link", "a"
                    Return renderLink(block)
                Case "math", "equation", "tex", "latex"
                    Return renderMath(block)
                Case "tasklist", "tasks", "todo"
                    Return renderTaskList(block)
                Case "footnote", "note"
                    Return renderFootnote(block)
                Case "deflist", "definition", "dl"
                    Return renderDefList(block)
                Case "html", "raw"
                    ' raw html block, output as-is without encoding
                    Return block.content
                Case Else
                    Return "<p>" & HtmlEncode(block.content) & "</p>"
            End Select
        End Function

        ''' <summary>
        ''' escape the special html characters in the given text fragment
        ''' </summary>
        ''' <param name="text"></param>
        ''' <returns></returns>
        Private Shared Function HtmlEncode(text As String) As String
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
        Private Shared Function AlignStyle(alignments As String(), i As Integer) As String
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

        Private Shared Function renderList(block As Block) As String
            Dim sb As New StringBuilder
            Dim tag As String = If(block.ordered, "ol", "ul")

            Call sb.AppendLine("<" & tag & ">")

            For Each item As String In block.items.SafeQuery
                Call sb.AppendLine("  <li>" & HtmlEncode(item) & "</li>")
            Next

            Call sb.AppendLine("</" & tag & ">")

            Return sb.ToString.TrimEnd(vbCrLf.ToCharArray)
        End Function

        Private Shared Function renderTable(block As Block) As String
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

        Private Shared Function renderBlockquote(block As Block) As String
            Dim lines As String() = block.content.Split(New String() {vbCrLf, vbLf, vbCr}, StringSplitOptions.None)
            Dim inner As String = lines _
                .Select(Function(line) "<p>" & HtmlEncode(line) & "</p>") _
                .JoinBy(vbCrLf)

            Return "<blockquote>" & vbCrLf & inner & vbCrLf & "</blockquote>"
        End Function

        ' -----------------------------------------------------------------
        ' math / equation
        ' -----------------------------------------------------------------
        Private Shared Function renderMath(block As Block) As String
            ' 保留 LaTeX 原字符，交由 KaTeX / MathJax 处理，不做 HTML 转义
            Return "<div class=""math"">" & "$$" & block.content & "$$" & "</div>"
        End Function

        ' -----------------------------------------------------------------
        ' link
        ' -----------------------------------------------------------------
        Private Shared Function renderLink(block As Block) As String
            Dim titleAttr As String = If(String.IsNullOrEmpty(block.title), "", " title=""" & HtmlEncode(block.title) & """")
            Return "<a href=""" & HtmlEncode(block.url) & """" & titleAttr & ">" & HtmlEncode(block.alt) & "</a>"
        End Function

        ' -----------------------------------------------------------------
        ' tasklist (GFM task list)
        ' -----------------------------------------------------------------
        Private Shared Function renderTaskList(block As Block) As String
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
        Private Shared Function renderFootnote(block As Block) As String
            Return "<div class=""footnote"" id=""fn-" & HtmlEncode(block.id) & """>" & HtmlEncode(block.content) & "</div>"
        End Function

        ' -----------------------------------------------------------------
        ' deflist (definition list)
        ' -----------------------------------------------------------------
        Private Shared Function renderDefList(block As Block) As String
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
    End Class
End Namespace
