#Region "Microsoft.VisualBasic::1e04e39afecfcf1c6acb938e90fa302f, mime\text%html\Markups\StreamWriter\Markdown.vb"

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

    '     Module Markdown
    ' 
    '         Function: TextMarkdown
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace StreamWriter

    Public Module Markdown

        <Extension>
        Public Function TextMarkdown(link As Hyperlink) As String
            If String.IsNullOrEmpty(link.Title) Then
                Return $"[{link.Text}]({link.Links})"
            Else
                Return $"[{link.Text}]({link.Links} ""{link.Title}"")"
            End If
        End Function
    End Module
End Namespace
