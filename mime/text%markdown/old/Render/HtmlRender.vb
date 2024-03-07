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
        Return "<hr" & markdown.EmptyElementSuffix & vbLf
    End Function

    Public Overrides Function NewLine() As String
        Return String.Format("<br{0}" & vbLf, markdown.EmptyElementSuffix)
    End Function

    Public Overrides Function CodeBlock(code As String, lang As String) As String
        Return String.Concat(vbLf & vbLf & $"<pre><code class=""{lang}"">", code, vbLf & "</code></pre>" & vbLf & vbLf)
    End Function

    Public Overrides Function Image(url As String, altText As String, title As String) As String
        Dim result = String.Format("<img src=""{0}"" alt=""{1}""", AttributeSafeUrl(url), markdown.EscapeImageAltText(AttributeEncode(altText)))

        If Not String.IsNullOrEmpty(title) Then
            title = AttributeEncode(markdown.EscapeBoldItalic(title))
            result &= String.Format(" title=""{0}""", title)
        End If

        result &= markdown.EmptyElementSuffix

        Return result
    End Function
End Class
