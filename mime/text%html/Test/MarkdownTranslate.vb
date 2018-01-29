Imports Microsoft.VisualBasic.MIME.Markup.MarkDown

Module MarkdownTranslate

    ReadOnly testMarkdown$ =
 _
        <markdown>

# header1

## header 2

### header3

```vbnet
' test
Call println(1,2,3)
```

```python
# code comments
print 1,2,3
```

|item|value|
|----|-----|
|a   |b    |
|c   |d    |

> quot test

-----------------------------------------------------------------------

+ ![](./test2.png)
+ This is [what???](http://scibasic.net)


> Microsoft.VisualBasic.Language.List``code``

        </markdown>


    ReadOnly codeBlockTest$ =
        <markdown>

> Microsoft.VisualBasic.Language.List``code``

```vbnet
Dim a$ = "12345"
```

> This is not a ``code`` block


        </markdown>

    Sub Main()

        Dim codeTest = New MarkdownHTML().Transform(codeBlockTest)


        Dim html = New MarkdownHTML().Transform(testMarkdown)


        Pause()
    End Sub
End Module
