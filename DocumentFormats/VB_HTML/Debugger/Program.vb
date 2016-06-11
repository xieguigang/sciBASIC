
Imports Microsoft.VisualBasic.MarkupLanguage
Imports Microsoft.VisualBasic.MarkupLanguage.HTML
Imports Microsoft.VisualBasic.MarkupLanguage.MarkDown
Imports Microsoft.VisualBasic.MarkupLanguage.StreamWriter

Module Program

    Sub Main()

        Dim mmmmm As New MarkupLanguage.MarkDown.Markdown(New MarkdownOptions)

        Dim hhhh = mmmmm.Transform("F:\VisualBasic_AppFramework\DocumentFormats\VB_HTML\syntax_test.md".GET)

        Dim mark As Markup = MarkdownParser.MarkdownParser("F:\VisualBasic_AppFramework\DocumentFormats\VB_HTML\syntax_test.md")

        Dim html As String = mark.ToHTML
        Call html.SaveTo("x:\test.html")

        Dim doc = HtmlDocument.Load("G:\GCModeller\GCModeller Virtual Cell System\wwwroot\gcmodeller.org\index.html")
        Call doc.Save("./trest.html")
    End Sub
End Module
