Imports System.Runtime.CompilerServices
Imports System.Text

Public Module DocumentWriter

    <Extension> Public Function Save(doc As DocumentFormat.HTML.HtmlDocument, SaveTo As String) As Boolean
        Return ToString(doc).SaveTo(SaveTo)
    End Function

    Public Function ToString(doc As HtmlDocument) As String
        Dim sbr As New StringBuilder(1024)

        For Each node In doc.Tags
            Call sbr.Append(node.__generateDocNode(""))
        Next

        Return sbr.ToString
    End Function

    <Extension> Private Function __generateDocNode(node As PlantText, indent As String) As String
        If node.IsPlantText Then
            Return node.InnerText
        End If

        Dim sbr As StringBuilder = New StringBuilder(1024)
        Dim nodeElement = DirectCast(node, HtmlElement)

        If nodeElement.IsBr Then
            Return "<br />"
        End If

        Dim attrs As String =
                If(nodeElement.Attributes.IsNullOrEmpty,
                "",
                " " & String.Join(" ", nodeElement.Attributes.ToArray(Function(attr) $"{attr.Name}=""{attr.Value}""")))

        If nodeElement.HtmlElements.IsNullOrEmpty Then
            Call sbr.Append($"<{nodeElement.Name}{attrs}></{nodeElement.Name}>")
        Else
            If nodeElement.OnlyInnerText Then
                Return $"<{nodeElement.Name}{attrs}>{nodeElement.HtmlElements.First.InnerText}</{nodeElement.Name}>"
            End If

            Call sbr.Append($"<{nodeElement.Name}{attrs}>")
            Dim NodeIndent = indent & "  "
            For Each node In nodeElement.HtmlElements

                If Not node.IsPlantText Then
                    Call sbr.AppendLine()
                    Call sbr.Append(NodeIndent)
                End If

                Call sbr.Append(node.__generateDocNode(NodeIndent))
            Next

            Call sbr.AppendLine()
            Call sbr.Append($"{indent}</{nodeElement.Name}>")
        End If

        Return sbr.ToString
    End Function

    <Extension> Public Function IsBr(node As HtmlElement) As Boolean
        Return String.Equals(node.Name, "br") AndAlso node.Attributes.IsNullOrEmpty AndAlso node.HtmlElements.IsNullOrEmpty
    End Function
End Module
