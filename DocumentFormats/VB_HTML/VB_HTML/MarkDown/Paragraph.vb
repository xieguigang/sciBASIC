Imports Microsoft.VisualBasic.MarkupLanguage.MarkDown.Span

Namespace MarkDown

    Module Paragraph

        Public Function ParagraphParser(s As String, lines As String(), ByRef i As Integer) As PlantText
            Dim nodes As New List(Of PlantText)
            Dim links = LinksParser.InlineLinks(s).ToArray

            nodes += links.Select(Function(x) x.value)


            Dim p As New ParagraphText With {
                .Nodes = nodes,
                .Text = s
            }

            Return p
        End Function
    End Module
End Namespace