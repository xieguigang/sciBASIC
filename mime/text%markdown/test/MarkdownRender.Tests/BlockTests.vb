Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports Microsoft.VisualBasic.MIME.text.markdown

<TestClass>
Public Class BlockTests

    Private Shared Function Render(md As String) As String
        Return New MarkdownRender().Transform(md)
    End Function

    <TestMethod>
    Public Sub AtxHeaders()
        Assert.IsTrue(Render("# H1").Contains("<h1>H1</h1>"), "h1")
        Assert.IsTrue(Render("###### H6").Contains("<h6>H6</h6>"), "h6")
        Assert.IsTrue(Render("# Hello *world*").Contains("<h1>Hello <em>world</em></h1>"), "inline inside header")
    End Sub

    <TestMethod>
    Public Sub SetextHeader()
        Dim md = "Title" & vbLf & "=====" & vbLf & vbLf & "Sub" & vbLf & "-----"
        Dim html = Render(md)
        Assert.IsTrue(html.Contains("<h1>Title</h1>"), "setext h1")
        Assert.IsTrue(html.Contains("<h2>Sub</h2>"), "setext h2")
    End Sub

    <TestMethod>
    Public Sub HorizontalRule()
        Assert.IsTrue(Render("---").Contains("<hr />"), "hr dashes")
        Assert.IsTrue(Render("***").Contains("<hr />"), "hr stars")
        Assert.IsTrue(Render("___").Contains("<hr />"), "hr underscores")
    End Sub

    <TestMethod>
    Public Sub CodeBlockKeepsLiteral()
        Dim md = "```" & vbLf & "a **b** c <d>" & vbLf & "```"
        Dim html = Render(md)
        Assert.IsTrue(html.Contains("<pre><code>a **b** c &lt;d&gt;</code></pre>"), "no inline parsing and entities escaped inside code block")
    End Sub

    <TestMethod>
    Public Sub Paragraph()
        Assert.IsTrue(Render("hello world").Contains("<p>hello world</p>"), "paragraph wrap")
    End Sub
End Class
