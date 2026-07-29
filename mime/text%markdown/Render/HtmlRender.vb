Imports System.Text.RegularExpressions

Public Class HtmlRender : Inherits Render

    Public Overrides Function Document(html As String) As String
        Return html
    End Function

    Public Overrides Function Paragraph(text As String, Optional createParagraphs As Boolean = True) As String
        If createParagraphs Then
            Return _leadingWhitespace.Replace(text, "<p>")
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

    ReadOnly _leadingWhitespace As New Regex("^[ ]*", RegexOptions.Multiline)
End Class
