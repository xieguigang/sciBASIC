#Region "Microsoft.VisualBasic::b460503d3417d0ea4cfd5f23e6255dc1, mime\text%markdown\Render\Render.vb"

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

    '   Total Lines: 67
    '    Code Lines: 42 (62.69%)
    ' Comment Lines: 13 (19.40%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 12 (17.91%)
    '     File Size: 3.01 KB


    ' Class Render
    ' 
    ' 
    '     Delegate Function
    ' 
    '         Properties: CodeSyntaxHighlight, image_class
    ' 
    '         Function: EscapeHtml, router
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text.RegularExpressions

''' <summary>
''' The different markup syntax formatter for the markdown document.
''' </summary>
Public MustInherit Class Render

    Public Delegate Function CodeHtmlSyntaxHighlight(code$, lang$) As String

    Protected ReadOnly _leadingWhitespace As New Regex("^[ ]*", RegexOptions.Multiline)

    ''' <summary>
    ''' run the html text output for display
    ''' </summary>
    ''' <param name="html"></param>
    ''' <returns></returns>
    Public MustOverride Function Document(html As String) As String
    Public MustOverride Function Paragraph(text As String, Optional createParagraphs As Boolean = True) As String
    Public MustOverride Function Header(text As String, level As Integer) As String
    Public MustOverride Function HorizontalLine() As String
    Public MustOverride Function NewLine() As String

    Public MustOverride Function Bold(text As String) As String
    Public MustOverride Function Italic(text As String) As String
    Public MustOverride Function Underline(text As String) As String
    Public MustOverride Function Strikethrough(text As String) As String
    Public MustOverride Function CodeSpan(text As String) As String
    Public MustOverride Function CodeBlock(text As String, language As String) As String

    Public MustOverride Function Image(url As String, alt As String, title As String) As String
    Public MustOverride Function AnchorLink(url As String, text As String, title As String) As String

    Public MustOverride Function BlockQuote(text As String) As String
    Public MustOverride Function List(items As IEnumerable(Of String), orderList As Boolean, Optional startNumber As Integer = 1) As String
    Public MustOverride Function Table(head() As String, rows As IEnumerable(Of String()), Optional align() As String = Nothing) As String

    Public MustOverride Sub SetImageUrlRouter(router As Func(Of String, String))

    Protected _router As Func(Of String, String)

    Public Property CodeSyntaxHighlight As CodeHtmlSyntaxHighlight
    Public Property image_class As String

    Protected Function router(url As String) As String
        If _router Is Nothing Then
            Return url
        Else
            Return _router(url)
        End If
    End Function

    ''' <summary>
    ''' Escape the special html chars in a text context. When
    ''' <paramref name="forAttribute"/> is true the quote characters are also
    ''' escaped so that the value is safe to embed inside an html attribute.
    ''' </summary>
    Protected Shared Function EscapeHtml(text As String, Optional forAttribute As Boolean = False) As String
        If text Is Nothing Then
            Return ""
        End If
        Dim s = text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;")
        If forAttribute Then
            s = s.Replace("""", "&quot;").Replace("'", "&#39;")
        End If
        Return s
    End Function
End Class
