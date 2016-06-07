
Imports Microsoft.VisualBasic.MarkupLanguage
Imports Microsoft.VisualBasic.MarkupLanguage.HTML
Imports Microsoft.VisualBasic.MarkupLanguage.MarkDown

Module Program

    Sub Main()

        Dim mark As Markup = MarkdownParser.MarkdownParser("F:\VisualBasic_AppFramework\DocumentFormats\VB_HTML\syntax_test.md")

        Dim html As String = mark.ToHTML
        Call html.SaveTo("x:\test.html")

        Dim doc = HtmlDocument.Load("G:\GCModeller\GCModeller Virtual Cell System\wwwroot\gcmodeller.org\index.html")
        Call doc.Save("./trest.html")
    End Sub
End Module
