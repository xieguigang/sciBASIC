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


--------

Valid links:

 [this is a link]()
 [this is a link](<http://something.example.com/foo/bar>)
 [this is a link](http://something.example.com/foo/bar 'test')
 ![this is an image]()
 ![this is an image](<http://something.example.com/foo/bar>)
 ![this is an image](http://something.example.com/foo/bar 'test')
 
 [escape test](<\>\>\>\>\>\>\>\>\>\>\>\>\>\>> '\'\'\'\'\'\'\'\'\'\'\'\'\'\'')
 [escape test \]\]\]\]\]\]\]\]\]\]\]\]\]\]\]\]](\)\)\)\)\)\)\)\)\)\)\)\)\)\))



- alter list 1
- alter list2
- alter list3
- alter list4
- alter list``5``


Invalid links:

 [this is not a link

 [this is not a link](

 [this is not a link](http://something.example.com/foo/bar 'test'
 
 [this is not a link](((((((((((((((((((((((((((((((((((((((((((((((
 
 [this is not a link]((((((((((()))))))))) (((((((((()))))))))))


*this* *is* *your* *basic* *boring* *emphasis*

_this_ _is_ _your_ _basic_ _boring_ _emphasis_

**this** **is** **your** **basic** **boring** **emphasis**


- alter list 1
- alter list2
- alter list3

- alter list4
- alter list``5``

1.  a
20.  b
30. x
40. d
500. . g``gg``


```
- alter list 1
- alter list2
- alter list3

10. a
20. b
30. c
```

"

    Sub Main(args As String())
        Console.WriteLine(New MakrdownRender().Transform(markdown_demo))

        Call New MakrdownRender().Transform(markdown_demo).SaveTo("./test_demo.html")
    End Sub
End Module
