Imports System
Imports Microsoft.VisualBasic.MIME.text.markdown.Language

Module Program

    Const markdown_demo As String = "

# header1

## *header2*

this has a ``code`` span. **yes it** is!

> a **fox**
>
> a ![image](aa/bb.png) [url](aaa.txt)

"

    Sub token_test()
        Dim t As New Tokenlizer(markdown_demo)
        Dim parts = t.GetTokens.ToArray

        Pause()
    End Sub

    Sub Main(args As String())
        Console.WriteLine("Hello World!")
    End Sub
End Module
