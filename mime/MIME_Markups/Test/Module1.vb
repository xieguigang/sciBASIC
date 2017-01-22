Imports Microsoft.VisualBasic.MIME.Markup.MarkDown

Module Module1

    Sub Main()
        Dim md = "![](./images/test.png) and ![](./2. annotations/GO/plot.png)"

        md = "G:\temp\reports.md".ReadAllText

        Call New MarkdownHTML().Transform(md).__DEBUG_ECHO

        Pause()
    End Sub
End Module
