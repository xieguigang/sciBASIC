#Region "Microsoft.VisualBasic::d018a94e25471bceb316e5f50dfc84b7, mime\text%markdown\Render\TextRender.vb"

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

    '   Total Lines: 87
    '    Code Lines: 56
    ' Comment Lines: 12
    '   Blank Lines: 19
    '     File Size: 2.74 KB


    ' Class TextRender
    ' 
    '     Function: AnchorLink, BlockQuote, Bold, CodeBlock, CodeSpan
    '               Document, Header, HorizontalLine, Image, Italic
    '               List, NewLine, Paragraph, Table, Underline
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Text.Parser.HtmlParser

''' <summary>
''' plain text document
''' </summary>
''' <remarks>
''' not only the markdown format tag will be removed from this render, and also 
''' the additional html tag inside the given markdown document will be removed.
''' </remarks>
Public Class TextRender : Inherits Render

    Public Overrides Function Paragraph(text As String, CreateParagraphs As Boolean) As String
        Return text
    End Function

    Public Overrides Function Header(text As String, level As Integer) As String
        Return text
    End Function

    Public Overrides Function CodeSpan(text As String) As String
        Return " " & text & " "
    End Function

    Public Overrides Function CodeBlock(code As String, lang As String) As String
        Return code
    End Function

    ''' <summary>
    ''' additional html tag inside the document will be removed
    ''' </summary>
    ''' <param name="text"></param>
    ''' <returns></returns>
    Public Overrides Function Document(text As String) As String
        Return text.StripHTMLTags
    End Function

    Public Overrides Function HorizontalLine() As String
        Return "-----------------------------------------"
    End Function

    Public Overrides Function NewLine() As String
        Return vbLf
    End Function

    Public Overrides Function Image(url As String, altText As String, title As String) As String
        Return altText
    End Function

    Public Overrides Function AnchorLink(url As String, text As String, title As String) As String
        Return text
    End Function

    Public Overrides Function Bold(text As String) As String
        Return text
    End Function

    Public Overrides Function Italic(text As String) As String
        Return text
    End Function

    Public Overrides Function Underline(text As String) As String
        Return text
    End Function

    Public Overrides Function BlockQuote(text As String) As String
        Return $"""{text}"""
    End Function

    Public Overrides Function List(items As IEnumerable(Of String), orderList As Boolean) As String
        Return items.JoinBy(vbLf)
    End Function

    Public Overrides Function Table(head() As String, rows As IEnumerable(Of String())) As String
        Dim sb As New StringBuilder

        Call sb.AppendLine(head.JoinBy(vbTab))
        Call sb.AppendLine("----------------------------------------------")

        For Each row As String() In rows
            Call sb.AppendLine(row.JoinBy(vbTab))
        Next

        Return sb.ToString
    End Function
End Class
