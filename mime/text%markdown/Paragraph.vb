#Region "Microsoft.VisualBasic::980a54299e667f7b99a97e7c812309c4, sciBASIC#\mime\text%markdown\Paragraph.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
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



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 34
    '    Code Lines: 25
    ' Comment Lines: 0
    '   Blank Lines: 9
    '     File Size: 1.07 KB


    ' Module Paragraph
    ' 
    '     Function: BoldFormats, ParagraphParser
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.text.markdown.Span
Imports Microsoft.VisualBasic.Text.Parser

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
