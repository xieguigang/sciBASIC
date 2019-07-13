#Region "Microsoft.VisualBasic::8b53f7fd97cf9b50a4718a3d47f77b45, mime\text%html\MarkDown\Paragraph.vb"

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

    '     Module Paragraph
    ' 
    '         Function: BoldFormats, ParagraphParser
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.Markup.MarkDown.Span
Imports Microsoft.VisualBasic.Text.Parser

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
            Dim text = New List(Of String)(Regex.Matches(s, BoldFormat).ToArray) + Regex.Matches(s, BoldFormat2).ToArray

            For Each s$ In text

            Next
        End Function
    End Module
End Namespace
