Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Namespace MarkDown

    ''' <summary>
    ''' TOC generator for markdown
    ''' </summary>
    Public Module TOC

        ''' <summary>
        ''' 为makrdown文本添加目录
        ''' </summary>
        ''' <param name="md$"></param>
        ''' <returns></returns>
        Public Function AddToc(md As StringBuilder, Optional numbering As Boolean = True, Optional autoSave As Boolean = True) As String
            Dim sb As New StringBuilder

            Call sb.AppendLine("<!-- vb.net-markdown-toc -->")
            Call sb.AppendLine()
            Call sb.AppendLine(md.ReplaceHeaders)
            Call sb.AppendLine()
            Call sb.AppendLine("<!-- vb.net-markdown-toc-config
	numbering=true
	autoSave=true
	/vb.net-markdown-toc-config -->
<!-- /vb.net-markdown-toc -->")

            Call sb.AppendLine(md.ToString)

            Return sb.ToString
        End Function

        <Extension> Private Function ReplaceHeaders(ByRef md As StringBuilder) As String
            Dim headers = GetHeaders(md.ToString)
            Dim i%() = {1, 1, 1, 1, 1, 1}
            Dim TOC As New List(Of String)

            For Each head In headers
                Dim parts = head.GetTagValue(" ", trim:=True)
                Dim level$ = parts.Name
                Dim indent$ = "   ".Repeats(level.Length - 1).JoinBy("")

                If level.Length > 4 Then
                    Continue For
                End If

                If level.Length = 1 Then
                    TOC += $"{i}. " & parts.Value
                End If

                level = $"<h{level.Length}>"

            Next
        End Function

        ''' <summary>
        ''' 按header在markdown文档之中出现的顺序进行返回
        ''' </summary>
        ''' <param name="md$"></param>
        ''' <returns></returns>
        Public Function GetHeaders(md$) As String()
            Dim headers As New List(Of String)

            headers += MarkdownHTML._headerSetext.Matches(md).ToArray
            headers += MarkdownHTML._headerAtx _
                .Matches(md) _
                .ToArray(Function(s) s.TrimNewLine.Trim)

            Dim orders As New List(Of SeqValue(Of String))
            Dim pos%

            For Each headerGroup As IGrouping(Of String, String) In headers.GroupBy(Function(s) s)
                pos = 1  ' start 参数必须要大于零

                Do While True
                    pos = InStr(pos, md, headerGroup.Key)

                    If pos > 0 Then
                        orders += New SeqValue(Of String) With {
                            .i = pos,
                            .value = headerGroup.Key
                        }
                        pos += 1  ' 必须要往前位移一个字符，否则会出现死循环
                    Else
                        Exit Do
                    End If
                Loop
            Next

            Return orders _
                .OrderBy(Function(i) i.i) _
                .Select(Function(s) s.value) _
                .ToArray
        End Function
    End Module
End Namespace