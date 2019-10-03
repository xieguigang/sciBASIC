Module markdownDisplayTest
    Sub Main()
        Call ApplicationServices.Terminal.MarkdownRender.Print("# title

This is a inline ``code`` span. **bold** font style test.

> quote
> test
> block
>
> A ``code span`` in this block quot

A new ``paragraph``.

A url test: http://test.url/a/b/c/xxxx.txt


", indent:=10)

        Pause()
    End Sub
End Module
