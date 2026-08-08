#Region "Microsoft.VisualBasic::92c4ff28636e675ea1e2972aebf49876, mime\text%markdown\test\MarkdownRender.Tests\InlineTests.vb"

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

    '   Total Lines: 121
    '    Code Lines: 0 (0.00%)
    ' Comment Lines: 98 (80.99%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 23 (19.01%)
    '     File Size: 4.31 KB


    ' 
    ' /********************************************************************************/

#End Region

'#Region "Microsoft.VisualBasic::e8b71a8d765db2e3ab8613fcdd14ad09, mime\text%markdown\test\MarkdownRender.Tests\InlineTests.vb"

'' Author:
'' 
''       asuka (amethyst.asuka@gcmodeller.org)
''       xie (genetics@smrucc.org)
''       xieguigang (xie.guigang@live.com)
'' 
'' Copyright (c) 2018 GPL3 Licensed
'' 
'' 
'' GNU GENERAL PUBLIC LICENSE (GPL3)
'' 
'' 
'' This program is free software: you can redistribute it and/or modify
'' it under the terms of the GNU General Public License as published by
'' the Free Software Foundation, either version 3 of the License, or
'' (at your option) any later version.
'' 
'' This program is distributed in the hope that it will be useful,
'' but WITHOUT ANY WARRANTY; without even the implied warranty of
'' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'' GNU General Public License for more details.
'' 
'' You should have received a copy of the GNU General Public License
'' along with this program. If not, see <http://www.gnu.org/licenses/>.



'' /********************************************************************************/

'' Summaries:


'' Code Statistics:

''   Total Lines: 66
''    Code Lines: 55 (83.33%)
'' Comment Lines: 0 (0.00%)
''    - Xml Docs: 0.00%
'' 
''   Blank Lines: 11 (16.67%)
''     File Size: 2.61 KB


'' Class InlineTests
'' 
''     Function: Render
'' 
''     Sub: BackslashEscape, BoldItalicCombined, BoldNotGreedy, HtmlEntityEscape, InlineCode
''          ItalicUnderscoreAtBoundary, LinkAndImage, Strikethrough, UnderscoreWordNotItalic
'' 
'' /********************************************************************************/

'#End Region

'Imports Microsoft.VisualBasic.MIME.text.markdown

'<TestClass>
'Public Class InlineTests

'    Private Shared Function Render(md As String) As String
'        Return New MarkdownRender().Transform(md)
'    End Function

'    <TestMethod>
'    Public Sub BoldNotGreedy()
'        Dim html = Render("**a** and **b**")
'        Assert.IsTrue(html.Contains("<strong>a</strong>"), "first bold")
'        Assert.IsTrue(html.Contains("<strong>b</strong>"), "second bold")
'        Assert.IsFalse(html.Contains("<strong>a** and **b</strong>"), "must not merge separate bold spans")
'    End Sub

'    <TestMethod>
'    Public Sub UnderscoreWordNotItalic()
'        Dim html = Render("my_var_name is fine")
'        Assert.IsFalse(html.Contains("<em>"), "underscore inside a word must not become italic")
'        Assert.IsTrue(html.Contains("my_var_name"), "word preserved as text")
'    End Sub

'    <TestMethod>
'    Public Sub ItalicUnderscoreAtBoundary()
'        Dim html = Render("_emphasis_ works")
'        Assert.IsTrue(html.Contains("<em>emphasis</em>"), "boundary underscore is italic")
'    End Sub

'    <TestMethod>
'    Public Sub InlineCode()
'        Assert.IsTrue(Render("use `code` here").Contains("<code>code</code>"), "code span")
'        Assert.IsTrue(Render("`**not bold**`").Contains("<code>**not bold**</code>"), "code content is not parsed")
'    End Sub

'    <TestMethod>
'    Public Sub LinkAndImage()
'        Assert.IsTrue(Render("[text](http://x.com)").Contains("<a href=""http://x.com"">text</a>"), "link")
'        Assert.IsTrue(Render("![alt](img.png)").Contains("<img src=""img.png"" alt=""alt"" />"), "image")
'        Assert.IsTrue(Render("[t](http://x.com ""tip"")").Contains("title=""tip"""), "link title")
'    End Sub

'    <TestMethod>
'    Public Sub Strikethrough()
'        Assert.IsTrue(Render("~~gone~~").Contains("<del>gone</del>"), "strikethrough")
'    End Sub

'    <TestMethod>
'    Public Sub BackslashEscape()
'        Dim html = Render("a \* b")
'        Assert.IsTrue(html.Contains("a * b"), "escaped star is literal")
'        Assert.IsFalse(html.Contains("<em>"), "no emphasis from escaped star")
'    End Sub

'    <TestMethod>
'    Public Sub HtmlEntityEscape()
'        Assert.IsTrue(Render("a < b & c > d").Contains("a &lt; b &amp; c &gt; d"), "special chars escaped as entities")
'    End Sub

'    <TestMethod>
'    Public Sub BoldItalicCombined()
'        Assert.IsTrue(Render("***both***").Contains("<strong><em>both</em></strong>"), "*** is bold + italic")
'    End Sub
'End Class
