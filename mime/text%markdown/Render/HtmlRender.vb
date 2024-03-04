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
        Return _leadingWhitespace.Replace(text, If(CreateParagraphs, "<p>", "")) & (If(CreateParagraphs, "</p>", ""))
    End Function
End Class
