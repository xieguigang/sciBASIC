#Region "Microsoft.VisualBasic::fab5a20f525c30ca12f8eb7f2571b553, mime\text%html\test\Module1.vb"

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

    ' Module Module1
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports Microsoft.VisualBasic.MIME.Markup.MarkDown

Module Module1

    Sub Main()
        Dim md = "

# header1

## Header2

![](./images/test.png) and ![](./2. annotations/GO/plot.png)

|1|2|3|
|-|-|-|
|a|b|c|
|x|y|z and ``| test``|


```python
# this is comment, not header
code here
```

    Dim a = a+b
    Call MsgBox(12334)

--------------------------

```vbnet
Dim DDDDDDDDDDDDDDDDDDDDDDDDDDDDD%

Public Function T() As Void
End Function
```


###### Escaping Test

```

# This is not a table

|a|b|c|
|-|-|-|
|1|2|3|

```
"

        ' md = "G:\temp\reports.md".ReadAllText

        Call New MarkdownHTML().Transform(md).SaveTo("./test.html", Encoding.UTF8)

        Pause()
    End Sub
End Module
