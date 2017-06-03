#Region "Microsoft.VisualBasic::71db8c6f183875fff6fc34ae5673e9c8, ..\sciBASIC#\mime\text%html\Test\Module1.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Text
Imports Microsoft.VisualBasic.MIME.Markup.MarkDown

Module Module1

    Sub Main()
        Dim md = "![](./images/test.png) and ![](./2. annotations/GO/plot.png)

|1|2|3|
|-|-|-|
|a|b|c|
|x|y|z and ``| test``|


```python
# this is comment, not header
code here
```

"

        ' md = "G:\temp\reports.md".ReadAllText

        Call New MarkdownHTML().Transform(md).SaveTo("x:\test.html", Encoding.UTF8)

        Pause()
    End Sub
End Module
