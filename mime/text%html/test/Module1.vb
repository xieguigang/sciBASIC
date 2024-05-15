#Region "Microsoft.VisualBasic::798d8b28423e607b4781d33ac2392b3a, mime\text%html\Test\Module1.vb"

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

    '   Total Lines: 58
    '    Code Lines: 36
    ' Comment Lines: 1
    '   Blank Lines: 21
    '     File Size: 780 B


    ' Module Module1
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports Microsoft.VisualBasic.MIME.text.markdown

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
