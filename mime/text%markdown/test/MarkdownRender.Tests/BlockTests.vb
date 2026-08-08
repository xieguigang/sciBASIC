#Region "Microsoft.VisualBasic::6c15268466941135bb9accaafc9fef1c, mime\text%markdown\test\MarkdownRender.Tests\BlockTests.vb"

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

    '   Total Lines: 99
    '    Code Lines: 0 (0.00%)
    ' Comment Lines: 80 (80.81%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 19 (19.19%)
    '     File Size: 3.47 KB


    ' 
    ' /********************************************************************************/

#End Region

'#Region "Microsoft.VisualBasic::4088daa443b782f34a311013eadba72d, mime\text%markdown\test\MarkdownRender.Tests\BlockTests.vb"

'    ' Author:
'    ' 
'    '       asuka (amethyst.asuka@gcmodeller.org)
'    '       xie (genetics@smrucc.org)
'    '       xieguigang (xie.guigang@live.com)
'    ' 
'    ' Copyright (c) 2018 GPL3 Licensed
'    ' 
'    ' 
'    ' GNU GENERAL PUBLIC LICENSE (GPL3)
'    ' 
'    ' 
'    ' This program is free software: you can redistribute it and/or modify
'    ' it under the terms of the GNU General Public License as published by
'    ' the Free Software Foundation, either version 3 of the License, or
'    ' (at your option) any later version.
'    ' 
'    ' This program is distributed in the hope that it will be useful,
'    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
'    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'    ' GNU General Public License for more details.
'    ' 
'    ' You should have received a copy of the GNU General Public License
'    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



'    ' /********************************************************************************/

'    ' Summaries:


'    ' Code Statistics:

'    '   Total Lines: 44
'    '    Code Lines: 37 (84.09%)
'    ' Comment Lines: 0 (0.00%)
'    '    - Xml Docs: 0.00%
'    ' 
'    '   Blank Lines: 7 (15.91%)
'    '     File Size: 1.67 KB


'    ' Class BlockTests
'    ' 
'    '     Function: Render
'    ' 
'    '     Sub: AtxHeaders, CodeBlockKeepsLiteral, HorizontalRule, Paragraph, SetextHeader
'    ' 
'    ' /********************************************************************************/

'#End Region

'Imports Microsoft.VisualStudio.TestTools.UnitTesting
'Imports Microsoft.VisualBasic.MIME.text.markdown

'<TestClass>
'Public Class BlockTests

'    Private Shared Function Render(md As String) As String
'        Return New MarkdownRender().Transform(md)
'    End Function

'    <TestMethod>
'    Public Sub AtxHeaders()
'        Assert.IsTrue(Render("# H1").Contains("<h1>H1</h1>"), "h1")
'        Assert.IsTrue(Render("###### H6").Contains("<h6>H6</h6>"), "h6")
'        Assert.IsTrue(Render("# Hello *world*").Contains("<h1>Hello <em>world</em></h1>"), "inline inside header")
'    End Sub

'    <TestMethod>
'    Public Sub SetextHeader()
'        Dim md = "Title" & vbLf & "=====" & vbLf & vbLf & "Sub" & vbLf & "-----"
'        Dim html = Render(md)
'        Assert.IsTrue(html.Contains("<h1>Title</h1>"), "setext h1")
'        Assert.IsTrue(html.Contains("<h2>Sub</h2>"), "setext h2")
'    End Sub

'    <TestMethod>
'    Public Sub HorizontalRule()
'        Assert.IsTrue(Render("---").Contains("<hr />"), "hr dashes")
'        Assert.IsTrue(Render("***").Contains("<hr />"), "hr stars")
'        Assert.IsTrue(Render("___").Contains("<hr />"), "hr underscores")
'    End Sub

'    <TestMethod>
'    Public Sub CodeBlockKeepsLiteral()
'        Dim md = "```" & vbLf & "a **b** c <d>" & vbLf & "```"
'        Dim html = Render(md)
'        Assert.IsTrue(html.Contains("<pre><code>a **b** c &lt;d&gt;</code></pre>"), "no inline parsing and entities escaped inside code block")
'    End Sub

'    <TestMethod>
'    Public Sub Paragraph()
'        Assert.IsTrue(Render("hello world").Contains("<p>hello world</p>"), "paragraph wrap")
'    End Sub
'End Class
