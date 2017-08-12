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
        Public Function AddToc(md$, Optional numbering As Boolean = True, Optional autoSave As Boolean = True) As String
            Dim sb As New StringBuilder
            Dim headers = GetHeaders(md)

            Call sb.AppendLine("<!-- vb.net-markdown-toc -->")
            Call sb.AppendLine("<!-- vb.net-markdown-toc-config
	numbering=true
	autoSave=true
	/vb.net-markdown-toc-config -->
<!-- /vb.net-markdown-toc -->")

            Call sb.AppendLine(md)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 按header在markdown文档之中出现的顺序进行返回
        ''' </summary>
        ''' <param name="md$"></param>
        ''' <returns></returns>
        Public Function GetHeaders(md$) As String()
            Dim headers As New List(Of String)

            headers += MarkdownHTML._headerSetext.Matches(md).ToArray
            headers += MarkdownHTML._headerAtx.Matches(md).ToArray

            Dim orders As New List(Of SeqValue(Of String))
            Dim pos%

            For Each headerGroup As IGrouping(Of String, String) In headers.GroupBy(Function(s) s)
                Do While True
                    pos = InStr(pos, md, headerGroup.Key)

                    If pos > 0 Then
                        orders += New SeqValue(Of String) With {
                            .i = pos,
                            .value = headerGroup.Key
                        }
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