Imports Microsoft.VisualBasic.DocumentFormat.HTML

Module Program

    Sub Main()
        Dim doc = Microsoft.VisualBasic.DocumentFormat.HTML.HtmlDocument.Load("G:\GCModeller\GCModeller Virtual Cell System\wwwroot\gcmodeller.org\index.html")
        Call doc.Save("./trest.html")
    End Sub
End Module
