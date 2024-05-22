#Region "Microsoft.VisualBasic::fe3404f0fef8770b9ba55355675b190d, mime\text%markdown\Render\Render.vb"

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

    '   Total Lines: 29
    '    Code Lines: 24 (82.76%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 5 (17.24%)
    '     File Size: 1.61 KB


    ' Class Render
    ' 
    '     Function: SetImageUrlRouter
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text.RegularExpressions

Public MustInherit Class Render

    Protected Shared ReadOnly _leadingWhitespace As New Regex("^[ ]*", RegexOptions.Compiled)
    Protected image_url_router As Func(Of String, String)

    Public Function SetImageUrlRouter(router As Func(Of String, String)) As Render
        image_url_router = router
        Return Me
    End Function

    Public MustOverride Function Paragraph(text As String, CreateParagraphs As Boolean) As String
    Public MustOverride Function Header(text As String, level As Integer) As String
    Public MustOverride Function CodeSpan(text As String) As String
    Public MustOverride Function CodeBlock(code As String, lang As String) As String
    Public MustOverride Function Document(text As String) As String
    Public MustOverride Function HorizontalLine() As String
    Public MustOverride Function NewLine() As String
    Public MustOverride Function Image(url As String, altText As String, title As String) As String
    Public MustOverride Function AnchorLink(url As String, text As String, title As String) As String
    Public MustOverride Function Bold(text As String) As String
    Public MustOverride Function Italic(text As String) As String
    Public MustOverride Function Underline(text As String) As String
    Public MustOverride Function BlockQuote(text As String) As String
    Public MustOverride Function List(items As IEnumerable(Of String), orderList As Boolean) As String
    Public MustOverride Function Table(head As String(), rows As IEnumerable(Of String())) As String

End Class
