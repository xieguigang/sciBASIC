Imports Microsoft.VisualBasic.MIME.text.markdown

Module Program

    Const markdown_demo As String = "

# header1

## *header2*

this has a ``code`` span. **yes it** is!

```
not a **bold** word

``aaa``

```

+ item1
+ item2
+ item2
+ item3
+ item4

> a **fox**
>
> a ![image](aa/bb.png) [url](aaa.txt)

"

    Sub Main(args As String())
        Console.WriteLine(New MakrdownRender().Render(markdown_demo))

        Call New MakrdownRender().Render(markdown_demo).SaveTo("./test_demo.html")
    End Sub
End Module
