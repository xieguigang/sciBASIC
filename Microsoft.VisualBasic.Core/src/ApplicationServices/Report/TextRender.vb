Imports System.Text
Imports Microsoft.VisualBasic.Linq

Namespace ApplicationServices.Report

    ''' <summary>
    ''' markdown/text render: converts the markdown content model <see cref="Block"/> into
    ''' markdown text.
    ''' </summary>
    Public Class TextRender : Inherits ReportRender

        ''' <summary>
        ''' render the given <see cref="Block"/> document into markdown text, the generated
        ''' markdown text can be read from the <see cref="ReportRender.Output"/> property.
        ''' </summary>
        ''' <param name="report">the markdown content blocks of the target document</param>
        Public Overrides Sub Render(report As IEnumerable(Of Block))
            Call buffer.Clear()
            Call AppendBlocks(report.SafeQuery.Select(AddressOf RenderBlock))
        End Sub

        ''' <summary>
        ''' render a single <see cref="Block"/> into its markdown text fragment.
        ''' </summary>
        ''' <param name="block"></param>
        ''' <returns></returns>
        Public Function RenderBlock(block As Block) As String
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
                    Return renderList(block)
                Case "blockquote"
                    Return renderBlockquote(block)
                Case "table"
                    Return renderTable(block)
                Case "hr", "horizontal-rule", "horizontalrule", "thematic-break"
                    Return "---"
                Case "image", "img"
                    If String.IsNullOrEmpty(block.title) Then
                        Return "![" & block.alt & "](" & block.url & ")"
                    Else
                        Return "![" & block.alt & "](" & block.url & " """ & block.title & """)"
                    End If
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
                Case Else
                    Return block.content
            End Select
        End Function

        Private Shared Function renderList(block As Block) As String
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

        Private Shared Function renderBlockquote(block As Block) As String
            Dim lines As String() = block.content.Split(New String() {vbCrLf, vbLf, vbCr}, StringSplitOptions.None)

            Return "> " & lines.JoinBy(vbCrLf & "> ")
        End Function

        Private Shared Function renderTable(block As Block) As String
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

        Private Shared Function alignMarkdownToken(alignments As String(), i As Integer) As String
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

        ' -----------------------------------------------------------------
        ' math / equation
        ' -----------------------------------------------------------------
        Private Shared Function renderMath(block As Block) As String
            If String.IsNullOrEmpty(block.language) Then
                Return "$$" & vbCrLf & block.content & vbCrLf & "$$"
            Else
                Return "```" & block.language & vbCrLf & block.content & vbCrLf & "```"
            End If
        End Function

        ' -----------------------------------------------------------------
        ' link
        ' -----------------------------------------------------------------
        Private Shared Function renderLink(block As Block) As String
            If String.IsNullOrEmpty(block.title) Then
                Return "[" & block.alt & "](" & block.url & ")"
            Else
                Return "[" & block.alt & "](" & block.url & " """ & block.title & """)"
            End If
        End Function

        ' -----------------------------------------------------------------
        ' tasklist (GFM task list)
        ' -----------------------------------------------------------------
        Private Shared Function renderTaskList(block As Block) As String
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

        ' -----------------------------------------------------------------
        ' footnote
        ' -----------------------------------------------------------------
        Private Shared Function renderFootnote(block As Block) As String
            Return "[^" & block.id & "]: " & block.content
        End Function

        ' -----------------------------------------------------------------
        ' deflist (definition list)
        ' -----------------------------------------------------------------
        Private Shared Function renderDefList(block As Block) As String
            Dim sb As New StringBuilder
            Dim n As Integer = MinInt(If(block.terms Is Nothing, 0, block.terms.Length), If(block.definitions Is Nothing, 0, block.definitions.Length))

            For i As Integer = 0 To n - 1
                Call sb.AppendLine(block.terms(i))
                Call sb.AppendLine(": " & block.definitions(i))
            Next

            Return sb.ToString.TrimEnd(vbCrLf.ToCharArray)
        End Function
    End Class
End Namespace
