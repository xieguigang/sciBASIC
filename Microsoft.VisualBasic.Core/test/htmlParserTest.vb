Imports Microsoft.VisualBasic.Text.Parser.HtmlParser

Module htmlParserTest

    Sub Main()
        Dim page$ = "https://www.ncbi.nlm.nih.gov/nuccore/NC_014248".GET
        Dim metaInfo = page.ParseHtmlMeta

        Pause()

        Dim testhtml = "D:\GCModeller\src\runtime\sciBASIC#\Microsoft.VisualBasic.Core\test\testhtml.txt".ReadAllText

        Dim list = testhtml.HtmlList
        Dim disable = list.Where(Function(li) li.classList.IndexOf("disabled") > -1).FirstOrDefault


        Pause()
    End Sub
End Module
