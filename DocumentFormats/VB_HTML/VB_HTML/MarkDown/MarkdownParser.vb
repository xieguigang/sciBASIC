Imports System.Runtime.CompilerServices

Namespace MarkDown

    Public Module MarkdownParser

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="path">The file path to the markdown text document.</param>
        ''' <returns></returns>
        Public Function MarkdownParser(path As String) As Markup
            Return path.ReadAllText.SyntaxParser
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="md">The markdown file text content, not file path</param>
        ''' <returns></returns>
        ''' 
        <Extension>
        Public Function SyntaxParser(md As String) As Markup

        End Function
    End Module
End Namespace