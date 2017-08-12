Imports System.Text

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

            Call sb.AppendLine("<!-- vb.net-markdown-toc -->")
            Call sb.AppendLine("<!-- vb.net-markdown-toc-config
	numbering=true
	autoSave=true
	/vb.net-markdown-toc-config -->
<!-- /vb.net-markdown-toc -->")

            Call sb.AppendLine(md)

            Return sb.ToString
        End Function

        Public Function GetHeaders(md$)

        End Function
    End Module
End Namespace