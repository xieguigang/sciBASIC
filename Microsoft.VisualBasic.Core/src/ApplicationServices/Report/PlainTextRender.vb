Imports System.Text
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Linq

Namespace ApplicationServices.Report

    ''' <summary>
    ''' plain text render: converts the markdown content model <see cref="Block"/> into
    ''' human readable plain text, that is, no markdown or html markers are kept.
    ''' (use <see cref="TextRender"/> if the markdown text is required)
    ''' </summary>
    Public Class PlainTextRender : Inherits TextReportRender

        ''' <summary>html 标签匹配（用于把 html 块降级为纯文本）。</summary>
        Private Shared ReadOnly htmlTag As New Regex("<[^>]*>", RegexOptions.Compiled)

        ''' <summary>
        ''' render the given <see cref="Block"/> document into plain text, the generated
        ''' text can be read from the <see cref="TextReportRender.Output"/> property.
        ''' </summary>
        ''' <param name="report">the markdown content blocks of the target document</param>
        Public Overrides Sub Render(report As IEnumerable(Of Block))
            Call buffer.Clear()
            Call AppendBlocks(report.SafeQuery.Select(AddressOf RenderBlock))
        End Sub

        ''' <summary>
        ''' render a single <see cref="Block"/> into its plain text fragment.
        ''' </summary>
        ''' <param name="block"></param>
        ''' <returns></returns>
        Public Function RenderBlock(block As Block) As String
            Select Case Strings.LCase(block.type)
                Case "heading", "h"
                    Return If(block.content, "")
                Case "paragraph", "p"
                    Return If(block.content, "")
                Case "code"
                    ' 代码块保留原始缩进与换行
                    Return If(block.content, "")
                Case "html", "raw"
                    Return StripTags(block.content)
                Case "list", "li"
                    Return renderList(block)
                Case "tasklist", "tasks", "todo"
                    Return renderTaskList(block)
                Case "blockquote"
                    Return renderQuote(block)
                Case "table"
                    Return renderTable(block)
                Case "hr", "horizontal-rule", "horizontalrule", "thematic-break"
                    Return "----------"
                Case "image", "img"
                    Return If(String.IsNullOrEmpty(block.alt), block.url, block.alt)
                Case "link", "a"
                    Return renderLink(block)
                Case "math", "equation", "tex", "latex"
                    Return If(block.content, "")
                Case "footnote", "note"
                    Return $"[{If(block.id, "")}] {If(block.content, "")}"
                Case "deflist", "definition", "dl"
                    Return renderDefList(block)
                Case Else
                    Return If(block.content, "")
            End Select
        End Function

        ''' <summary>去掉 html 标签，仅保留文本内容。</summary>
        ''' <param name="text"></param>
        ''' <returns></returns>
        Private Shared Function StripTags(text As String) As String
            If String.IsNullOrEmpty(text) Then
                Return ""
            End If

            Return htmlTag.Replace(text, "").Trim()
        End Function

        Private Shared Function renderList(block As Block) As String
            Dim sb As New StringBuilder()
            Dim index As Integer = 1

            For Each item As String In block.items.SafeQuery
                If block.ordered Then
                    Call sb.Append(index).Append(". ").AppendLine(item)
                    index += 1
                Else
                    Call sb.Append("• ").AppendLine(item)
                End If
            Next

            Return sb.ToString().TrimEnd()
        End Function

        Private Shared Function renderTaskList(block As Block) As String
            Dim sb As New StringBuilder()
            Dim pos As Integer = 0

            For Each item As String In block.items.SafeQuery
                Dim mark As String = " "

                If Not block.checked Is Nothing AndAlso pos < block.checked.Length AndAlso block.checked(pos) Then
                    mark = "x"
                End If

                Call sb.Append("[").Append(mark).Append("] ").AppendLine(item)
                pos += 1
            Next

            Return sb.ToString().TrimEnd()
        End Function

        Private Shared Function renderQuote(block As Block) As String
            If String.IsNullOrEmpty(block.content) Then
                Return ""
            End If

            Dim lines As String() = block.content.Replace(vbCrLf, vbLf).Replace(vbCr, vbLf).Split(ControlChars.Lf)

            Return lines.Select(Function(line) "> " & line).JoinBy(vbCrLf)
        End Function

        Private Shared Function renderTable(block As Block) As String
            Dim sb As New StringBuilder()
            Dim headers As String() = If(block.headers, New String() {})

            If headers.Length > 0 Then
                Call sb.AppendLine(headers.JoinBy(" | "))
                Call sb.AppendLine(headers.Select(Function(h) New String("-"c, 3)).JoinBy("-+-"))
            End If

            For Each row As String() In block.rows.SafeQuery
                Call sb.AppendLine(row.JoinBy(" | "))
            Next

            Return sb.ToString().TrimEnd()
        End Function

        Private Shared Function renderLink(block As Block) As String
            Dim text As String = If(block.alt, "")

            If String.IsNullOrEmpty(block.url) Then
                Return text
            End If
            If String.IsNullOrEmpty(text) OrElse text = block.url Then
                Return block.url
            End If

            Return $"{text} ({block.url})"
        End Function

        Private Shared Function renderDefList(block As Block) As String
            Dim sb As New StringBuilder()
            Dim terms As String() = If(block.terms, New String() {})
            Dim definitions As String() = If(block.definitions, New String() {})

            For i As Integer = 0 To System.Math.Min(terms.Length, definitions.Length) - 1
                Call sb.AppendLine($"{If(terms(i), "")}: {If(definitions(i), "")}")
            Next

            Return sb.ToString().TrimEnd()
        End Function
    End Class
End Namespace
