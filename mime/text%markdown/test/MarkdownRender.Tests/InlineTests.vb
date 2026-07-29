Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports Microsoft.VisualBasic.MIME.text.markdown

<TestClass>
Public Class InlineTests

    Private Shared Function Render(md As String) As String
        Return New MarkdownRender().Transform(md)
    End Function

    <TestMethod>
    Public Sub BoldNotGreedy()
        Dim html = Render("**a** and **b**")
        Assert.IsTrue(html.Contains("<strong>a</strong>"), "first bold")
        Assert.IsTrue(html.Contains("<strong>b</strong>"), "second bold")
        Assert.IsFalse(html.Contains("<strong>a** and **b</strong>"), "must not merge separate bold spans")
    End Sub

    <TestMethod>
    Public Sub UnderscoreWordNotItalic()
        Dim html = Render("my_var_name is fine")
        Assert.IsFalse(html.Contains("<em>"), "underscore inside a word must not become italic")
        Assert.IsTrue(html.Contains("my_var_name"), "word preserved as text")
    End Sub

    <TestMethod>
    Public Sub ItalicUnderscoreAtBoundary()
        Dim html = Render("_emphasis_ works")
        Assert.IsTrue(html.Contains("<em>emphasis</em>"), "boundary underscore is italic")
    End Sub

    <TestMethod>
    Public Sub InlineCode()
        Assert.IsTrue(Render("use `code` here").Contains("<code>code</code>"), "code span")
        Assert.IsTrue(Render("`**not bold**`").Contains("<code>**not bold**</code>"), "code content is not parsed")
    End Sub

    <TestMethod>
    Public Sub LinkAndImage()
        Assert.IsTrue(Render("[text](http://x.com)").Contains("<a href=""http://x.com"">text</a>"), "link")
        Assert.IsTrue(Render("![alt](img.png)").Contains("<img src=""img.png"" alt=""alt"" />"), "image")
        Assert.IsTrue(Render("[t](http://x.com ""tip"")").Contains("title=""tip"""), "link title")
    End Sub

    <TestMethod>
    Public Sub Strikethrough()
        Assert.IsTrue(Render("~~gone~~").Contains("<del>gone</del>"), "strikethrough")
    End Sub

    <TestMethod>
    Public Sub BackslashEscape()
        Dim html = Render("a \* b")
        Assert.IsTrue(html.Contains("a * b"), "escaped star is literal")
        Assert.IsFalse(html.Contains("<em>"), "no emphasis from escaped star")
    End Sub

    <TestMethod>
    Public Sub HtmlEntityEscape()
        Assert.IsTrue(Render("a < b & c > d").Contains("a &lt; b &amp; c &gt; d"), "special chars escaped as entities")
    End Sub

    <TestMethod>
    Public Sub BoldItalicCombined()
        Assert.IsTrue(Render("***both***").Contains("<strong><em>both</em></strong>"), "*** is bold + italic")
    End Sub
End Class
