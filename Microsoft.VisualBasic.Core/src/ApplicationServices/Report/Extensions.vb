Imports System.Runtime.CompilerServices

Namespace ApplicationServices.Report

    <HideModuleName> Public Module Extensions

        <Extension>
        Public Function ToMarkdown(docBlocks As IEnumerable(Of Block)) As String
            Dim render As New TextRender
            Call render.Render(docBlocks)

            Return render.Output
        End Function

        <Extension>
        Public Function ToHtml(docBlocks As IEnumerable(Of Block)) As String
            Dim render As New HtmlRender
            Call render.Render(docBlocks)

            Return render.Output
        End Function

        <Extension>
        Public Function ToMarkdownBlock(block As Block) As String
            Return {block}.ToMarkdown
        End Function
    End Module
End Namespace