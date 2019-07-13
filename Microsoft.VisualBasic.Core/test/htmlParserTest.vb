#Region "Microsoft.VisualBasic::07fd10d466b3e3e3a5c170394dbee05a, Microsoft.VisualBasic.Core\test\htmlParserTest.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    ' Module htmlParserTest
    ' 
    '     Sub: Main, UrlescapeTest
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Text.Parser.HtmlParser

Module htmlParserTest

    Sub Main()

        Call UrlescapeTest()

        Dim page$ = "https://www.ncbi.nlm.nih.gov/nuccore/NC_014248".GET
        Dim metaInfo = page.ParseHtmlMeta

        Pause()

        Dim testhtml = "D:\GCModeller\src\runtime\sciBASIC#\Microsoft.VisualBasic.Core\test\testhtml.txt".ReadAllText

        Dim list = testhtml.HtmlList
        Dim disable = list.Where(Function(li) li.classList.IndexOf("disabled") > -1).FirstOrDefault


        Pause()
    End Sub

    Sub UrlescapeTest()
        Dim token = "Ethinyl estradiol".UrlEncode(jswhitespace:=True)
        Dim plus = "A+B".UrlEncode


        Pause()
    End Sub
End Module
