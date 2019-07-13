#Region "Microsoft.VisualBasic::c95dadb2e8487ec67bdacb22346da3d2, mime\text%html\Extensions.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    ' Module Extensions
    ' 
    '     Function: Markdown2HTML
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.MIME.Markup.MarkDown

Public Module Extensions

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function Markdown2HTML(markdown$, Optional opt As MarkdownOptions = Nothing) As String
        Return New MarkdownHTML(opt Or MarkdownOptions.DefaultOption).Transform(text:=markdown)
    End Function
End Module
