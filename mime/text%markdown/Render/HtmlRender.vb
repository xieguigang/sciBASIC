#Region "Microsoft.VisualBasic::22e27cb32615300f47f2b8ce98e01688, mime\text%markdown\Render\HtmlRender.vb"

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

'   Total Lines: 135
'    Code Lines: 114 (84.44%)
' Comment Lines: 0 (0.00%)
'    - Xml Docs: 0.00%
' 
'   Blank Lines: 21 (15.56%)
'     File Size: 4.51 KB


' Class HtmlRender
' 
'     Function: AlignAttr, AnchorLink, BlockQuote, Bold, CodeBlock
'               CodeSpan, Document, Header, HorizontalLine, Image
'               Italic, List, NewLine, Paragraph, Strikethrough
'               Table, Underline
' 
'     Sub: SetImageUrlRouter
' 
' /********************************************************************************/

#End Region


Imports System.Text

Public Class HtmlRender : Inherits Render

    ''' <summary>
    ''' 主题样式。为 Nothing 时 <see cref="Document"/> 原样返回 html 片段（与旧行为一致）；
    ''' 非 Nothing 时会把 CSS 写进输出，生成带样式的完整 html 文档。
    ''' </summary>
    ''' <returns></returns>
    Public Property Style As MarkdownStyle

    ''' <summary>创建不带主题样式的 html 渲染器（输出 html 片段）。</summary>
    Public Sub New()
    End Sub

    ''' <summary>
    ''' 创建带主题样式的 html 渲染器：输出的 html 文档内嵌 &lt;style&gt; 样式块。
    ''' </summary>
    ''' <param name="style">主题样式，例如 <see cref="MarkdownStyle.GitHub"/>。</param>
    Public Sub New(style As MarkdownStyle)
        Me.Style = style
    End Sub

    ''' <summary>
    ''' 把文档 body 片段包装为完整的 html 文档；没有主题样式时原样返回片段。
    ''' </summary>
    ''' <param name="html">markdown 解析生成的 html 片段。</param>
    ''' <returns>html 文档或 html 片段。</returns>
    Public Overrides Function Document(html As String) As String
        If Style Is Nothing Then
            Return html
        End If

        Dim sb As New StringBuilder()
        Dim bodyClass As String = If(String.IsNullOrEmpty(Style.BodyClass), "markdown-body", Style.BodyClass)

        Call sb.AppendLine("<!DOCTYPE html>")
        Call sb.AppendLine("<html lang=""zh-CN"">")
        Call sb.AppendLine("<head>")
        Call sb.AppendLine("<meta charset=""utf-8"" />")
        Call sb.AppendLine("<meta name=""viewport"" content=""width=device-width, initial-scale=1"" />")
        Call sb.AppendLine($"<meta name=""generator"" content=""{EscapeHtml(Style.Name, forAttribute:=True)}"" />")
        Call sb.AppendLine("<style>")
        Call sb.AppendLine(If(Style.Css, ""))
        Call sb.AppendLine("</style>")
        Call sb.AppendLine("</head>")
        Call sb.AppendLine($"<body class=""{EscapeHtml(bodyClass, forAttribute:=True)}"">")
        Call sb.AppendLine(html)
        Call sb.AppendLine("</body>")
        Call sb.AppendLine("</html>")

        Return sb.ToString()
    End Function

    Public Overrides Function Paragraph(text As String, Optional createParagraphs As Boolean = True) As String
        If createParagraphs Then
            Return $"<p>{text}</p>"
        End If
        Return text
    End Function

    Public Overrides Function Header(text As String, level As Integer) As String
        Return $"<h{level}>{text}</h{level}>"
    End Function

    Public Overrides Function HorizontalLine() As String
        Return "<hr />"
    End Function

    Public Overrides Function NewLine() As String
        Return "<br />"
    End Function

    Public Overrides Function Bold(text As String) As String
        Return $"<strong>{text}</strong>"
    End Function

    Public Overrides Function Italic(text As String) As String
        Return $"<em>{text}</em>"
    End Function

    Public Overrides Function Underline(text As String) As String
        Return $"<u>{text}</u>"
    End Function

    Public Overrides Function Strikethrough(text As String) As String
        Return $"<del>{text}</del>"
    End Function

    Public Overrides Function CodeSpan(text As String) As String
        Return $"<code>{EscapeHtml(text)}</code>"
    End Function

    Public Overrides Function CodeBlock(text As String, language As String) As String
        If language.StringEmpty Then
            Return $"
<pre><code>{EscapeHtml(text)}</code></pre>
"
        Else
            Return $"
<pre><code class=""language-{language}"">{EscapeHtml(text)}</code></pre>
"
        End If
    End Function

    Public Overrides Function Image(url As String, alt As String, title As String) As String
        If title.StringEmpty Then
            Return $"<img src=""{EscapeHtml(router(url), forAttribute:=True)}"" alt=""{EscapeHtml(alt, forAttribute:=True)}"" />"
        Else
            Return $"<img src=""{EscapeHtml(router(url), forAttribute:=True)}"" alt=""{EscapeHtml(alt, forAttribute:=True)}"" title=""{EscapeHtml(title, forAttribute:=True)}"" />"
        End If
    End Function

    Public Overrides Function AnchorLink(url As String, text As String, title As String) As String
        If title.StringEmpty Then
            Return $"<a href=""{EscapeHtml(router(url), forAttribute:=True)}"">{text}</a>"
        Else
            Return $"<a href=""{EscapeHtml(router(url), forAttribute:=True)}"" title=""{EscapeHtml(title, forAttribute:=True)}"">{text}</a>"
        End If
    End Function

    Public Overrides Function BlockQuote(text As String) As String
        Return $"<blockquote>{text}</blockquote>"
    End Function

    Public Overrides Function List(items As IEnumerable(Of String), orderList As Boolean, Optional startNumber As Integer = 1) As String
        Dim listSet = items.Select(Function(s) $"<li>{s}</li>").ToArray

        If orderList Then
            Dim startAttr = If(startNumber = 1, "", $" start=""{startNumber}""")
            Return $"
<ol{startAttr}>
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

    Public Overrides Function Table(head() As String, rows As IEnumerable(Of String()), Optional align() As String = Nothing) As String
        Dim th = head.Select(Function(h, idx) $"<th{AlignAttr(align, idx)}>{h}</th>").ToArray
        Dim body = rows _
            .Select(Function(r) $"<tr>{r.Select(Function(d, idx) $"<td{AlignAttr(align, idx)}>{d}</td>").JoinBy("")}</tr>") _
            .ToArray

        Return $"
<table>
<thead>
<tr>{th.JoinBy("")}</tr>
</thead>
<tbody>
{body.JoinBy(vbLf)}
</tbody>
</table>
"
    End Function

    Private Shared Function AlignAttr(align() As String, idx As Integer) As String
        If align Is Nothing OrElse idx >= align.Length Then
            Return ""
        End If
        Dim a = align(idx)
        If a = "left" Then
            Return " style=""text-align:left;"""
        ElseIf a = "right" Then
            Return " style=""text-align:right;"""
        ElseIf a = "center" Then
            Return " style=""text-align:center;"""
        End If
        Return ""
    End Function

    Public Overrides Sub SetImageUrlRouter(router As Func(Of String, String))
        _router = router
    End Sub
End Class
