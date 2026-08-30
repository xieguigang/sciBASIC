#Region "Microsoft.VisualBasic::984953937373aac56f52e40045368bbb, Microsoft.VisualBasic.Core\test\test\markdownRenderVerify.vb"

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

    '   Total Lines: 392
    '    Code Lines: 243 (61.99%)
    ' Comment Lines: 57 (14.54%)
    '    - Xml Docs: 43.86%
    ' 
    '   Blank Lines: 92 (23.47%)
    '     File Size: 16.15 KB


    ' Module markdownRenderVerify
    ' 
    '     Function: ColumnsOf, R, TestTheme
    ' 
    '     Sub: Check, CheckBlock, CheckInline, CheckOriginalSample, CheckTable
    '          CheckTerminalState, ReportError, Run
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Terminal
Imports Microsoft.VisualBasic.Text

''' <summary>
''' the regression checks of the markdown console renderer
''' </summary>
''' <remarks>
''' The test project of this solution is a console application, so that the
''' verification is written as a set of the assertions instead of the MSTest
''' [TestMethod] attributes.
''' </remarks>
Module markdownRenderVerify

    ''' <summary>
    ''' the ansi escape char
    ''' </summary>
    ReadOnly ESC As String = AnsiEscapeCodes.Escape

    ' the expected escape sequences of the test theme, see the TestTheme function
    ReadOnly GLOBAL_ As String = ESC & "[0;37;40m"
    ReadOnly BOLD As String = ESC & "[0;31;40m"
    ReadOnly ITALY As String = ESC & "[0;32;40m"
    ReadOnly CODE As String = ESC & "[0;33;40m"
    ReadOnly URL As String = ESC & "[0;34;40m"
    ReadOnly STRIKE As String = ESC & "[0;35;40;9m"
    ' the ***both*** span: the italic color plus the ansi bold attribute
    ReadOnly BOLD_ITALY As String = ESC & "[0;32;40;1m"
    ReadOnly LINK As String = ESC & "[0;36;40m"
    ReadOnly HEADER As String = ESC & "[0;93;40m"
    ReadOnly CODEBLOCK As String = ESC & "[0;92;40m"
    ReadOnly MARKER As String = ESC & "[0;96;40m"
    ReadOnly HRULE As String = ESC & "[0;90;40m"
    ReadOnly QUOTE As String = ESC & "[0;37;100m"
    ' an inline code span inside of a block quote keeps the quote background
    ReadOnly CODE_IN_QUOTE As String = ESC & "[0;33;100m"
    ReadOnly RESET As String = ESC & "[0m"

    Dim passed As Integer = 0
    Dim failed As Integer = 0

    Sub Run()
        passed = 0
        failed = 0

        Console.WriteLine("==== markdown render regression checks ====")
        Console.WriteLine()

        Call CheckInline()
        Call CheckBlock()
        Call CheckTable()
        Call CheckTerminalState()
        Call CheckOriginalSample()

        Console.WriteLine()
        Console.WriteLine($"  passed: {passed}, failed: {failed}")
        Console.WriteLine()
    End Sub

    ''' <summary>
    ''' a theme with the well known colors, so that the rendered escape
    ''' sequence can be asserted exactly.
    ''' </summary>
    ''' <returns></returns>
    Private Function TestTheme() As MarkdownTheme
        Return New MarkdownTheme With {
            .[Global] = New ConsoleFormat(AnsiColor.White, AnsiColor.Black),
            .Bold = New ConsoleFormat(AnsiColor.Red, Nothing),
            .Italy = New ConsoleFormat(AnsiColor.Green, Nothing),
            .InlineCodeSpan = New ConsoleFormat(AnsiColor.Yellow, Nothing),
            .Url = New ConsoleFormat(AnsiColor.Blue, Nothing),
            .LinkText = New ConsoleFormat(AnsiColor.Cyan, Nothing),
            .StrikeThrough = New ConsoleFormat(AnsiColor.Magenta, Nothing) With {.Strikeout = True},
            .HeaderSpan = New ConsoleFormat(AnsiColor.BrightYellow, Nothing),
            .CodeBlock = New ConsoleFormat(AnsiColor.BrightGreen, Nothing),
            .ListMarker = New ConsoleFormat(AnsiColor.BrightCyan, Nothing),
            .HorizontalRule = New ConsoleFormat(AnsiColor.BrightBlack, Nothing),
            .BlockQuote = New ConsoleFormat(AnsiColor.White, AnsiColor.BrightBlack)
        }
    End Function

    Private Function R(md As String, Optional indent% = 0) As String
        Dim render As New MarkdownRender(TestTheme(), ConsoleColor.Black, ConsoleColor.White)

        ' the ansi flag is forced on, so that the checks do not depend on
        ' the environment variable or on the stdout redirection
        render.EnableAnsi = True

        Dim out As String = render.Render(md, indent)

        If render.LastError IsNot Nothing Then
            ' the renderer falls back to the plain text output on error, every
            ' style assertion below would be meaningless in that case
            Call ReportError(md, render.LastError)
        End If

        Return out
    End Function

    Private Sub ReportError(md As String, ex As Exception)
        failed += 1

        Console.WriteLine($"  [FAIL] the renderer threw on: {md.Replace(vbLf, "\n")}")
        Console.WriteLine($"         {ex.GetType().Name}: {ex.Message}")
    End Sub

    Private Sub Check(name As String, condition As Boolean, Optional detail As String = "")
        If condition Then
            passed += 1
            Console.WriteLine($"  [PASS] {name}")
        Else
            failed += 1
            Console.WriteLine($"  [FAIL] {name}")

            If Not detail.StringEmpty(whitespaceAsEmpty:=False) Then
                Console.WriteLine($"         {detail}")
            End If
        End If
    End Sub

    Private Sub CheckInline()
        Console.WriteLine("-- inline elements --")

        ' the single backtick code span was not supported at all before
        Check("single backtick inline code",
              R("use `code` here").Contains(CODE & "code"))
        Check("double backtick inline code",
              R("use ``code`` here").Contains(CODE & "code"))
        Check("code span content is not parsed",
              R("`**not bold**`").Contains(CODE & "**not bold**") AndAlso
              Not R("`**not bold**`").Contains(BOLD))

        ' the bold spans must not be greedy
        Dim boldText As String = R("**a** and **b**")

        Check("bold not greedy",
              boldText.Contains(BOLD & "a") AndAlso boldText.Contains(BOLD & "b") AndAlso
              Not boldText.Contains(BOLD & "a** and **b"))

        Check("single star italic",
              R("*emphasis* works").Contains(ITALY & "emphasis"))
        Check("underscore inside a word is not italic",
              Not R("my_var_name is fine").Contains(ITALY))
        Check("underscore at boundary is italic",
              R("_emphasis_ works").Contains(ITALY & "emphasis"))
        ' the ***both*** must be distinguishable from the plain *italic*
        Check("bold + italic",
              R("***both***").Contains(BOLD_ITALY) AndAlso
              Not R("***both***").Contains(ITALY & "both"))

        Check("strike through",
              R("~~gone~~").Contains(STRIKE & "gone"))

        ' the link and the image
        Dim linkText As String = R("[text](http://x.com)")

        Check("link text", linkText.Contains(LINK & "text"))
        Check("link url", linkText.Contains(URL & " (http://x.com)"))
        Check("image alt text", R("![alt](img.png)").Contains(URL & "alt"))

        Dim titled As String = R("[t](http://x.com ""tip"")")

        Check("link title is dropped",
              titled.Contains(" (http://x.com)") AndAlso Not titled.Contains("tip"))

        ' the backslash escape
        Dim escape As String = R("a \* b")

        Check("escaped star is literal",
              escape.Contains("a * b") AndAlso Not escape.Contains(ITALY))

        ' the most important regression: an unmatched delimiter char must stay
        ' as the plain text and must not pollute the styles of the text that
        ' follows it. The old implementation buffered the delimiter char into a
        ' global look-ahead buffer that was never cleared, so that the styles of
        ' all of the following text were broken.
        Dim stray As String = R("a * b")

        Check("an unmatched star stays as the plain text",
              stray.Contains("a * b") AndAlso Not stray.Contains(BOLD) AndAlso Not stray.Contains(ITALY))

        Dim strayCode As String = R("a ` b")

        Check("an unmatched backtick stays as the plain text",
              strayCode.Contains("a ` b") AndAlso Not strayCode.Contains(CODE))

        ' a matched ``* b *`` pair is a real italic span in the markdown syntax,
        ' but the "c" after it must not be swallowed by that span
        Dim paired As String = R("a * b * c")

        Check("a matched italic pair does not swallow the following text",
              paired.Contains(ITALY & " b ") AndAlso Not paired.Contains(BOLD) AndAlso
              paired.EndsWith(GLOBAL_ & " c" & vbLf & RESET))

        Check("bare url", R("see http://x.com/a/b.txt now").Contains(URL & "http://x.com/a/b.txt"))
        Check("trailing punctuation is not a part of the url",
              R("see http://x.com/a/b.txt.").Contains(URL & "http://x.com/a/b.txt" & GLOBAL_ & "."))
    End Sub

    Private Sub CheckBlock()
        Console.WriteLine()
        Console.WriteLine("-- block elements --")

        Check("atx header", R("# H1").Contains(HEADER & "H1"))
        Check("atx header marker is stripped", Not R("# H1").Contains("#"))
        Check("hashtag is not a header", R("#tag").Contains(GLOBAL_ & "#tag"))

        ' the fenced code block was parsed as a normal paragraph before, and
        ' the theme.CodeBlock style was never applied
        Dim codeText As String = R("```" & vbLf & "a **b** c" & vbLf & "```")

        Check("fenced code block is not parsed",
              codeText.Contains(CODEBLOCK & "a **b** c") AndAlso Not codeText.Contains(BOLD))
        Check("fence markers are removed", Not codeText.Contains("```"))

        Check("block quote", R("> quote").Contains(QUOTE & "  quote"))

        Dim quoteText As String = R("> a" & vbLf & ">" & vbLf & "> ``c`` d")

        Check("block quote with an empty quote line",
              quoteText.Contains(QUOTE & "  a") AndAlso
              quoteText.Contains(CODE_IN_QUOTE & "c") AndAlso
              quoteText.Contains(QUOTE & " d"))

        Check("dash list item", R("- item").Contains(MARKER & "- "))
        Check("plus list item", R("+ item").Contains(MARKER & "+ "))
        Check("star list item", R("* item").Contains(MARKER & "* "))
        Check("ordered list item", R("1. item").Contains(MARKER & "1. "))
        Check("horizontal rule", R("---").Contains(HRULE & "---"))
    End Sub

    Private Sub CheckTable()
        Console.WriteLine()
        Console.WriteLine("-- table --")

        Dim table As String = R("|a|b|" & vbLf & "|--|--|" & vbLf & "|1|2|")
        Dim lines As String() = table.LineTokens

        ' the |a|b| syntax used to produce two extra empty columns
        Check("no phantom empty columns", ColumnsOf(lines(0)) = 2, "columns=" & ColumnsOf(lines(0)) & " of: [" & lines(0) & "]")
        Check("table body is rendered", table.Contains("1") AndAlso table.Contains("2"))

        Dim wide As String = R("|a|b|c|" & vbLf & "|--|--|--|" & vbLf & "|1|2|3|")

        Check("a 3 columns table is wider than a 2 columns one",
              ColumnsOf(wide.LineTokens()(0)) = 3, "columns=" & ColumnsOf(wide.LineTokens()(0)))

        ' the table buffer used to be shared between the tables
        Dim two As String = R("|a|b|" & vbLf & "|--|--|" & vbLf & "|1|2|" & vbLf & vbLf & "|x|y|" & vbLf & "|--|--|" & vbLf & "|3|4|")

        Check("the second table keeps its own header",
              two.Contains("x") AndAlso two.Contains("y") AndAlso two.Contains("3") AndAlso two.Contains("4"))

        ' the table at the end of the document used to be dropped silently
        Dim tail As String = R("text" & vbLf & vbLf & "|a|b|" & vbLf & "|--|--|" & vbLf & "|1|2|")

        Check("the table at the document end is not dropped",
              tail.Contains("a") AndAlso tail.Contains("b") AndAlso tail.Contains("1"))

        ' the empty table buffer used to throw an IndexOutOfRangeException
        Dim ok As Boolean = True

        Try
            Call R("|" & vbLf & "|--|")
        Catch ex As Exception
            ok = False
        End Try

        Check("an empty table does not crash", ok)

        ' the table rows must not inherit the color of the previous span
        Dim colored As String = R("**bold**" & vbLf & vbLf & "|a|b|" & vbLf & "|--|--|" & vbLf & "|1|2|")

        Check("the table rows carry the global style",
              colored.IndexOf(GLOBAL_ & "1") > colored.IndexOf(BOLD & "bold"))
    End Sub

    Private Sub CheckTerminalState()
        Console.WriteLine()
        Console.WriteLine("-- terminal state --")

        Check("the output is terminated by the color reset", R("hello").EndsWith(RESET))
        Check("the output is terminated by a line feed",
              R("hello").Substring(0, R("hello").Length - RESET.Length).EndsWith(vbLf))

        ' the indent used to be applied by the Console.CursorLeft setter, which
        ' throws an IOException when the stdout is redirected
        Dim indented As String = R("hello", 4)

        Check("the indent is emitted as a space prefix", indented.StartsWith("    "))
        Check("the indent is applied on every line", R("a" & vbLf & "b", 2).Contains(vbLf & "  "))

        ' the ansi color can be turned off
        Dim plain As New MarkdownRender(TestTheme(), ConsoleColor.Black, ConsoleColor.White)

        plain.EnableAnsi = False

        Dim plainText As String = plain.Render("**bold** `code`")

        Check("the plain text fallback has no escape sequence",
              Not plainText.Contains(ESC) AndAlso plainText.Contains("bold") AndAlso plainText.Contains("code"))

        ' the renderer instance is reusable, the parse state must not leak
        Dim reuse As New MarkdownRender(TestTheme(), ConsoleColor.Black, ConsoleColor.White)

        reuse.EnableAnsi = True

        Dim first As String = reuse.Render("|a|b|" & vbLf & "|--|--|" & vbLf & "|1|2|")
        Dim second As String = reuse.Render("|a|b|" & vbLf & "|--|--|" & vbLf & "|1|2|")

        Check("the renderer instance is reusable", first = second)

        ' the robustness of the degenerate inputs
        Dim empty As New MarkdownRender(TestTheme(), ConsoleColor.Black, ConsoleColor.White)

        empty.EnableAnsi = True

        Check("an empty document does not crash",
              empty.Render("") = RESET, "[" & empty.Render("").Replace(ESC, "\e") & "]")
        Check("a null document does not crash", empty.Render(Nothing) = RESET)
        Check("the empty document has no error", empty.LastError Is Nothing)

        ' the document that is terminated by the windows cr-lf pair
        Dim crlf As String = R("# a" & vbCrLf & vbCrLf & "**b**")

        Check("the cr-lf line ending is handled",
              crlf.Contains(HEADER & "a") AndAlso crlf.Contains(BOLD & "b"))
    End Sub

    ''' <summary>
    ''' the original smoke test of the markdownDisplayTest.Main1, which is kept
    ''' here as the visual regression baseline.
    ''' </summary>
    Private Sub CheckOriginalSample()
        Console.WriteLine()
        Console.WriteLine("-- the original sample --")

        Dim md As String = "# title

This is a inline ``code`` span. **bold** font style test.

table test:

|id|name|value|
|--|----|-----|
|1 |aaa |bbb  |
|2 |ccc |ddd  |
|3 |eee |fff  |

list test:

+ quote
+ list
+ data

> quote
> test
> block
>
> A ``code span`` in this block quot

A new ``paragraph``.

A url test: http://test.url/a/b/c/xxxx.txt

"

        Dim out As String = R(md)
        Dim expect As String() = {"title", "code", "bold", "id", "name", "value",
                                  "aaa", "ddd", "fff", "quote", "list", "data",
                                  "paragraph", "http://test.url/a/b/c/xxxx.txt"}

        For Each term As String In expect
            Check($"sample keeps ""{term}""", out.Contains(term))
        Next

        Check("sample has no leftover marker",
              Not out.Contains("``") AndAlso Not out.Contains("|1 |"))
        Check("sample is terminated by the reset", out.EndsWith(RESET))
    End Sub

    ''' <summary>
    ''' counts the columns of one rendered table line
    ''' </summary>
    ''' <param name="line"></param>
    ''' <returns></returns>
    Private Function ColumnsOf(line As String) As Integer
        Dim cells As String() = line _
            .Split(New Char() {" "c}, StringSplitOptions.RemoveEmptyEntries)

        Return cells.Length
    End Function
End Module
