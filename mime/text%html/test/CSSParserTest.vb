#Region "Microsoft.VisualBasic::2b8e50801156a99f5c8740ce9b79b0e9, mime\text%html\test\CSSParserTest.vb"

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

    ' Module CSSParserTest
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS.Parser

Module CSSParserTest

    Sub Main()

        Dim css$ = "D:\GCModeller\src\runtime\sciBASIC#\gr\SVG\demo.css"

        Dim file = CssParser.GetTagWithCSS(css.ReadAllText)


        Pause()
    End Sub
End Module
