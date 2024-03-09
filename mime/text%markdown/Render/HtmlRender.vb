Public Class HtmlRender : Inherits Render

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
        Return {"<code>", text, "</code>"}.JoinBy("")
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
        Return String.Concat(vbLf & vbLf & $"<pre><code class=""{lang}"">", code, vbLf & "</code></pre>" & vbLf & vbLf)
    End Function

    Public Overrides Function Image(url As String, altText As String, title As String) As String
        Dim result = String.Format("<img src=""{0}"" alt=""{1}""", url, altText)

        If Not String.IsNullOrEmpty(title) Then
            result &= String.Format(" title=""{0}""", title)
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
            Return $"<ol>{listSet.JoinBy("")}</ol>"
        Else
            Return $"<ul>{listSet.JoinBy("")}</ul>"
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
End Class
