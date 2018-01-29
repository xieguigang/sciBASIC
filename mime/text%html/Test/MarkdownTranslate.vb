Imports Microsoft.VisualBasic.MIME.Markup.MarkDown

Module MarkdownTranslate

    ReadOnly testMarkdown$ =
 _
        <markdown>

# header1

## header 2

### header3

```vbnet

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

        </markdown>

    Sub Main()

        Dim html = New MarkdownHTML().Transform(testMarkdown)


        Pause()
    End Sub
End Module
