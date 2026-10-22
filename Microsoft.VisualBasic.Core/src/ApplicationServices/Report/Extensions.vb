Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq

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
    End Module
End Namespace