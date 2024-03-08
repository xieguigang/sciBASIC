Imports Microsoft.VisualBasic.MIME.text.markdown

Module Program

    Const markdown_demo As String = "

# header1

## *header2*

this has a ``code`` span. **yes it** is!

```
not a **bold** word
---------------------------
``aaa``

```

-------------------------------

### head*er3*

***

this is *alt* __bold__ style
this is _alt italic_ style

_________
_________


|a|b|c|
|--|--|--|
|*1*|2|<span style='color: red;'>3</span>|
|4|*5*|6|
|7|**8**|*9*|

#### head``e``r4

###### tittle level 6

+ item1
+ item2
+ item2
+ item3: ``a+b=c``
+ item4

> a **fox**
>
> a ![image](aa/bb.png) [url](aaa.txt)

"

    Sub Main(args As String())
        Console.WriteLine(New MakrdownRender().Transform(markdown_demo))

        Call New MakrdownRender().Transform(markdown_demo).SaveTo("./test_demo.html")
    End Sub
End Module
