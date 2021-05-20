Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Linq

Namespace Document

    Module BuildHtmlDocument

        <Extension>
        Public Function ToHtml(node As HtmlElement) As String
            Dim html As New StringBuilder

            If node.Attributes.IsNullOrEmpty Then
                Call html.AppendLine($"<{node.TagName}>")
            Else
                Call html.AppendLine($"<{node.TagName} {(From attr In node.Attributes Select $"{attr.Name}=""{attr.Values.JoinBy(" ")}""").JoinBy(" ")}>")
            End If

            Call html.AppendLine(node.InnerText)

            For Each child As InnerPlantText In node.HtmlElements.SafeQuery
                Call html.AppendLine(child.GetHtmlText)
            Next

            Call html.AppendLine($"</{node.TagName}>")

            Return html.ToString
        End Function

    End Module
End Namespace