#Region "Microsoft.VisualBasic::5388ac83a55aff66a8b4cdabbcf0c647, ..\visualbasic_App\mime\Markups\MarkDown\Paragraph.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.

#End Region

Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.MIME.Markup.MarkDown.Span
Imports Microsoft.VisualBasic.Text
Imports Microsoft.VisualBasic.Language

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
