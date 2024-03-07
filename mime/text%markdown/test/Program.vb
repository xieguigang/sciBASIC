Imports Microsoft.VisualBasic.MIME.text.markdown

Module Program

    Const markdown_demo As String = "

# header1

## *header2*

this has a ``code`` span. **yes it** is!

> a **fox**
>
> a ![image](aa/bb.png) [url](aaa.txt)

"

    Sub Main(args As String())
        Console.WriteLine(New MakrdownRender().Render(markdown_demo))
    End Sub
End Module
