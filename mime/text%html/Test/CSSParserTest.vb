Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS.Parser

Module CSSParserTest

    Sub Main()

        Dim css$ = "D:\GCModeller\src\runtime\sciBASIC#\gr\SVG\demo.css"

        Dim file = CssParser.GetTagWithCSS(css.ReadAllText)


        Pause()
    End Sub
End Module
