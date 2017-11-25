Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.MIME.Markup.MarkDown

Public Module Extensions

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function Markdown2HTML(markdown$, Optional opt As MarkdownOptions = Nothing) As String
        Return New MarkdownHTML(opt Or MarkdownOptions.DefaultOption).Transform(text:=markdown)
    End Function
End Module
