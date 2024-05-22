#Region "Microsoft.VisualBasic::0f222236f8d764134d20811e5b692997, mime\text%markdown\Render\HtmlRender.vb"

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

    '   Total Lines: 130
    '    Code Lines: 90 (69.23%)
    ' Comment Lines: 13 (10.00%)
    '    - Xml Docs: 84.62%
    ' 
    '   Blank Lines: 27 (20.77%)
    '     File Size: 4.09 KB


    ' Class HtmlRender
    ' 
    '     Properties: image_class
    ' 
    '     Function: AnchorLink, BlockQuote, Bold, CodeBlock, CodeSpan
    '               Document, escapeHtml, Header, HorizontalLine, Image
    '               Italic, List, NewLine, Paragraph, Table
    '               Underline
    ' 
    ' /********************************************************************************/

#End Region

Public Class HtmlRender : Inherits Render

    Public Property image_class As String

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="text"></param>
    ''' <param name="CreateParagraphs">
    ''' add html paragraph tag automatically?
    ''' </param>
    ''' <returns></returns>
    Public Overrides Function Paragraph(text As String, CreateParagraphs As Boolean) As String
        ' replace the leading whitespace as the html tag <p>
        Dim p As String = _leadingWhitespace.Replace(text, If(CreateParagraphs, "<p>", ""))
        Dim html As String = p & (If(CreateParagraphs, "</p>", ""))

        Return html
    End Function

    Public Overrides Function Document(text As String) As String
        Return text
    End Function

    Public Overrides Function Header(text As String, level As Integer) As String
        Return String.Format("<h{1}>{0}</h{1}>" & vbLf & vbLf, text, level)
    End Function

    Public Overrides Function CodeSpan(text As String) As String
        Return {"<code>", escapeHtml(text), "</code>"}.JoinBy("")
    End Function

    Private Shared Function escapeHtml(text As String) As String
        Return If(text, "").Replace("<", "&lt;")
    End Function

    ''' <summary>
    ''' &lt;hr />
    ''' </summary>
    ''' <returns></returns>
    Public Overrides Function HorizontalLine() As String
        Return vbCrLf & vbCrLf & "<hr />" & vbCrLf & vbCrLf
    End Function

    Public Overrides Function NewLine() As String
        Return vbCrLf & "<br />" & vbCrLf
    End Function

    Public Overrides Function CodeBlock(code As String, lang As String) As String
        Return String.Concat(vbLf & vbLf & $"<pre><code class=""{lang}"">", escapeHtml(code), vbLf & "</code></pre>" & vbLf & vbLf)
    End Function

    Public Overrides Function Image(url As String, altText As String, title As String) As String
        Dim result As String

        If Not image_url_router Is Nothing Then
            url = image_url_router(url)
        End If

        result = String.Format("<img src=""{0}"" alt=""{1}""", url, altText)

        If Not image_class.StringEmpty Then
            result &= $" class=""{image_class}"""
        End If
        If Not String.IsNullOrEmpty(title) Then
            result &= $" title=""{title}"""
        End If

        result &= " />"

        Return result
    End Function

    Public Overrides Function Bold(text As String) As String
        Return $"<strong>{text}</strong>"
    End Function

    Public Overrides Function Italic(text As String) As String
        Return $"<em>{text}</em>"
    End Function

    Public Overrides Function BlockQuote(text As String) As String
        Return vbLf & vbLf & $"<blockquote>{text.LineTokens.JoinBy("<br />" & vbLf)}</blockquote>" & vbLf & vbLf
    End Function

    Public Overrides Function List(items As IEnumerable(Of String), orderList As Boolean) As String
        Dim listSet As String() = items.Select(Function(s) $"<li>{s}</li>").ToArray

        If orderList Then
            Return $"
<ol>
{listSet.JoinBy(vbLf)}
</ol>
"
        Else
            Return $"
<ul>
{listSet.JoinBy(vbLf)}
</ul>
"
        End If
    End Function

    Public Overrides Function Table(head() As String, rows As IEnumerable(Of String())) As String
        Dim bodyRows = rows _
            .Select(Function(r)
                        Return $"<tr>{r.Select(Function(d) $"<td>{d}</td>").JoinBy("")}</tr>"
                    End Function) _
            .ToArray

        Return $"<table>

<thead>
<tr>{head.Select(Function(h) $"<th>{h}</th>").JoinBy("")}</tr>
</thead>
<tbody>
{bodyRows.JoinBy(vbCrLf)}
</tbody>

</table>"
    End Function

    Public Overrides Function AnchorLink(url As String, text As String, title As String) As String
        Return $"<a href='{url}' title='{title}'>{text}</a>"
    End Function

    Public Overrides Function Underline(text As String) As String
        Return $"<u>{text}</u>"
    End Function
End Class
