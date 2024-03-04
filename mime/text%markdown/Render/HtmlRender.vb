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
        Dim p As String = _leadingWhitespace.Replace(text, If(CreateParagraphs, "<p>", ""))
        Dim html As String = p & (If(CreateParagraphs, "</p>", ""))

        Return html
    End Function
End Class
