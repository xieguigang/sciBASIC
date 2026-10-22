#Region "Microsoft.VisualBasic::markdown-to-blocks-tests, mime\text%markdown\test\MarkdownRender.Tests\MarkdownToBlocksTests.vb"

    ' Author:
    '
    '       xieguigang (xie.guigang@live.com)
    '
    ' Copyright (c) 2018 GPL3 Licensed
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

Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports Microsoft.VisualBasic.MIME.text.markdown
Imports Microsoft.VisualBasic.MIME.text.markdown.JSONSchema
Imports System.Collections.Generic

<TestClass>
Public Class MarkdownToBlocksTests

    Private Shared Function Parse(md As String) As List(Of Block)
        Return New Microsoft.VisualBasic.MIME.text.markdown.MarkdownRender().ParseBlocks(md)
    End Function

    <TestMethod>
    Public Sub EmptyDocument()
        Assert.AreEqual(0, Parse("").Count, "empty string")
        Assert.AreEqual(0, Parse(Nothing).Count, "null")
        Assert.AreEqual(0, Parse(vbLf & vbLf & "   " & vbLf).Count, "whitespace only")
    End Sub

    <TestMethod>
    Public Sub BlockTypesAndOrder()
        Dim md = "# Title" & vbLf & vbLf &
                 "A paragraph with **bold** and `code` and [link](http://x.com)." & vbLf & vbLf &
                 "## Section" & vbLf & vbLf &
                 "- item one" & vbLf &
                 "- item two" & vbLf & vbLf &
                 "1. first" & vbLf &
                 "2. second" & vbLf & vbLf &
                 "> a quote" & vbLf &
                 "> second line" & vbLf & vbLf &
                 "| a | b |" & vbLf &
                 "| :- | -: |" & vbLf &
                 "| 1 | 2 |" & vbLf & vbLf &
                 "---" & vbLf & vbLf &
                 "```python" & vbLf &
                 "print('hi')" & vbLf &
                 "```" & vbLf & vbLf &
                 "- [x] done" & vbLf &
                 "- [ ] todo" & vbLf & vbLf &
                 "[^1]: a footnote" & vbLf & vbLf &
                 "term" & vbLf &
                 ": definition"

        Dim blocks = Parse(md)

        Assert.AreEqual(12, blocks.Count, "total block count")

        ' 1. heading
        Assert.AreEqual("heading", blocks(0).type)
        Assert.AreEqual(1, blocks(0).level)
        Assert.AreEqual("Title", blocks(0).content)

        ' 2. paragraph — content must be plain text, no html tags / markers
        Assert.AreEqual("paragraph", blocks(1).type)
        Assert.AreEqual("A paragraph with bold and code and link.", blocks(1).content)
        Assert.IsFalse(blocks(1).content.Contains("<"), "no html in content")
        Assert.IsFalse(blocks(1).content.Contains("**"), "no emphasis markers in content")

        ' 3. heading level 2
        Assert.AreEqual("heading", blocks(2).type)
        Assert.AreEqual(2, blocks(2).level)
        Assert.AreEqual("Section", blocks(2).content)

        ' 4. unordered list
        Assert.AreEqual("list", blocks(3).type)
        Assert.IsFalse(blocks(3).ordered)
        Assert.AreEqual(2, blocks(3).items.Length)
        Assert.AreEqual("item one", blocks(3).items(0))

        ' 5. ordered list
        Assert.AreEqual("list", blocks(4).type)
        Assert.IsTrue(blocks(4).ordered)
        Assert.AreEqual("second", blocks(4).items(1))

        ' 6. blockquote — content carries both lines as plain text
        Assert.AreEqual("blockquote", blocks(5).type)
        Assert.IsTrue(blocks(5).content.Contains("a quote"), "quote line 1")
        Assert.IsTrue(blocks(5).content.Contains("second line"), "quote line 2")

        ' 7. table
        Assert.AreEqual("table", blocks(6).type)
        Assert.AreEqual("a", blocks(6).headers(0))
        Assert.AreEqual("left", blocks(6).alignments(0))
        Assert.AreEqual("right", blocks(6).alignments(1))
        Assert.AreEqual(1, blocks(6).rows.Length)
        Assert.AreEqual("2", blocks(6).rows(0)(1))

        ' 8. horizontal rule
        Assert.AreEqual("hr", blocks(7).type)

        ' 9. code block keeps literal content and language
        Assert.AreEqual("code", blocks(8).type)
        Assert.AreEqual("python", blocks(8).language)
        Assert.IsTrue(blocks(8).content.Contains("print('hi')"))

        ' 10. tasklist
        Assert.AreEqual("tasklist", blocks(9).type)
        Assert.AreEqual(2, blocks(9).items.Length)
        Assert.IsTrue(blocks(9).checked(0), "first checked")
        Assert.IsFalse(blocks(9).checked(1), "second unchecked")
        Assert.AreEqual("todo", blocks(9).items(1))

        ' 11. footnote
        Assert.AreEqual("footnote", blocks(10).type)
        Assert.AreEqual("1", blocks(10).id)
        Assert.AreEqual("a footnote", blocks(10).content)

        ' 12. definition list
        Assert.AreEqual("deflist", blocks(11).type)
        Assert.AreEqual("term", blocks(11).terms(0))
        Assert.AreEqual("definition", blocks(11).definitions(0))
    End Sub

    <TestMethod>
    Public Sub SetextHeader()
        Dim md = "Title" & vbLf & "=====" & vbLf & vbLf & "Sub" & vbLf & "-----"
        Dim blocks = Parse(md)
        Assert.AreEqual(2, blocks.Count)
        Assert.AreEqual("heading", blocks(0).type)
        Assert.AreEqual(1, blocks(0).level)
        Assert.AreEqual("heading", blocks(1).type)
        Assert.AreEqual(2, blocks(1).level)
    End Sub

    <TestMethod>
    Public Sub StandaloneImageAndLink()
        Dim img = Parse("![alt text](http://x.com/a.png ""t"")")
        Assert.AreEqual(1, img.Count)
        Assert.AreEqual("image", img(0).type)
        Assert.AreEqual("http://x.com/a.png", img(0).url)
        Assert.AreEqual("alt text", img(0).alt)
        Assert.AreEqual("t", img(0).title)

        Dim link = Parse("[visit](http://x.com)")
        Assert.AreEqual(1, link.Count)
        Assert.AreEqual("link", link(0).type)
        Assert.AreEqual("http://x.com", link(0).url)
        Assert.AreEqual("visit", link(0).alt)
    End Sub

    <TestMethod>
    Public Sub MathBlock()
        Dim md = "$$" & vbLf & "\frac{a}{b}" & vbLf & "$$"
        Dim blocks = Parse(md)
        Assert.AreEqual(1, blocks.Count)
        Assert.AreEqual("math", blocks(0).type)
        Assert.IsTrue(blocks(0).content.Contains("\frac{a}{b}"))

        Dim fenced = Parse("```tex" & vbLf & "E = mc^2" & vbLf & "```")
        Assert.AreEqual(1, fenced.Count)
        Assert.AreEqual("math", fenced(0).type)
        Assert.AreEqual("tex", fenced(0).language)
    End Sub

    <TestMethod>
    Public Sub RawHtmlBlock()
        Dim md = "<div class=""note"">" & vbLf & "  hello" & vbLf & "</div>" & vbLf & vbLf & "after"
        Dim blocks = Parse(md)
        Assert.AreEqual("html", blocks(0).type)
        Assert.IsTrue(blocks(0).content.Contains("<div"))
        Assert.AreEqual("paragraph", blocks(1).type)
        Assert.AreEqual("after", blocks(1).content)
    End Sub

    <TestMethod>
    Public Sub RoundTripToMarkdown()
        Dim md = "# Title" & vbLf & vbLf &
                 "A paragraph." & vbLf & vbLf &
                 "- one" & vbLf &
                 "- two"
        Dim blocks = Parse(md)
        Dim out = blocks.ToMarkdown
        ' 重新解析往返后块类型/数量应保持一致
        Dim reparsed = Parse(out)
        Assert.AreEqual(blocks.Count, reparsed.Count, "round-trip block count")
        Assert.AreEqual("heading", reparsed(0).type)
        Assert.AreEqual("paragraph", reparsed(1).type)
        Assert.AreEqual("list", reparsed(2).type)
    End Sub

End Class
