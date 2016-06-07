Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.MarkupLanguage.MarkDown.Span
Imports Microsoft.VisualBasic.Text

Namespace MarkDown

    Module Paragraph

        Public Function ParagraphParser(s As String, lines As String(), ByRef i As Integer) As PlantText
            Dim nodes As New List(Of PlantText)
            Dim links = LinksParser.InlineLinks(s).ToArray
            Dim bolds = BoldFormats(s).ToArray

            nodes += links.Select(Function(x) x.value)


            Dim p As New ParagraphText With {
                .Nodes = nodes,
                .Text = s
            }

            Return p
        End Function

        Const BoldFormat As String = "\*{2}.+?\*{2}"
        Const BoldFormat2 As String = "_{2}.+?_{2}"

        Public Iterator Function BoldFormats(s As String) As IEnumerable(Of ParserValue(Of Bold))
            Dim values =
                New List(Of String)(Regex.Matches(s, BoldFormat).ToArray) +
                                    Regex.Matches(s, BoldFormat2).ToArray
        End Function
    End Module
End Namespace