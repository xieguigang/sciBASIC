#Region "Microsoft.VisualBasic::919e452450838cfe27a252cd8313eb23, mime\text%html\Test\MarkdownTranslate.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 67
    '    Code Lines: 39
    ' Comment Lines: 1
    '   Blank Lines: 27
    '     File Size: 954 B


    ' Module MarkdownTranslate
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MIME.text.markdown

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
