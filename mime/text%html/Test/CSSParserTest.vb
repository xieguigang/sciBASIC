Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS.Parser

Module CSSParserTest

    Sub Main()

        Dim css$ = "D:\smartnucl_integrative\biodeepDB\smartnucl_integrative\build_tools\CVD_kb\smartnucl.CVD_kb\template\styles\hmp.css"

        Dim file = CssParser.GetTagWithCSS(css.ReadAllText)


        Pause()
    End Sub
End Module
