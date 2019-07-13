#Region "Microsoft.VisualBasic::cb1fd6143b6bb53d85570eb440ae4487, mime\text%html\test\MarkdownTranslate.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    ' Module MarkdownTranslate
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

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
