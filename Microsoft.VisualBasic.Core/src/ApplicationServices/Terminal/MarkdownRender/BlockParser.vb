#Region "Microsoft.VisualBasic::7293fbfdd65da6d04419399918a80f49, Microsoft.VisualBasic.Core\src\ApplicationServices\Terminal\MarkdownRender\BlockParser.vb"

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

    '   Total Lines: 390
    '    Code Lines: 217 (55.64%)
    ' Comment Lines: 91 (23.33%)
    '    - Xml Docs: 81.32%
    ' 
    '   Blank Lines: 82 (21.03%)
    '     File Size: 14.05 KB


    '     Class BlockParser
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: HeaderLevel, IndentOf, IsBlank, IsBlockQuote, IsFence
    '                   IsHorizontalRule, IsTableStart, Parse, ParseBlockQuote, ParseFencedCode
    '                   StripListItem, StripQuoteMarker
    ' 
    '         Sub: AppendSpans, EmitLine, EmitListItem, FlushTable
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace ApplicationServices.Terminal

    ''' <summary>
    ''' The block level markdown parser: it splits the document into the block
    ''' elements and then delegates the text of each block to the 
    ''' <see cref="InlineParser"/> or the <see cref="TableRenderer"/>.
    ''' </summary>
    ''' <remarks>
    ''' The block elements that are supported here:
    ''' 
    ''' + the ``#`` atx header
    ''' + the ``` ``` ``` fenced code block, of which the content is never parsed
    ''' + the pipe table
    ''' + the ``&gt;`` block quote
    ''' + the ``-``/``+``/``*``/``1.`` list item
    ''' + the ``---``/``***``/``___`` horizontal rule
    ''' + the normal paragraph
    ''' 
    ''' Note that the setext header(the ``===``/``---`` underlined header) is not
    ''' supported, as the ``---`` underline is ambiguous with the horizontal rule,
    ''' and the horizontal rule always wins here.
    ''' </remarks>
    Friend Class BlockParser

        ''' <summary>
        ''' the line feed of the rendered result, the cr-lf pair is never used
        ''' in this module to keep the output consistent on all platforms.
        ''' </summary>
        Private Const LF As String = vbLf

        ReadOnly theme As MarkdownTheme
        ReadOnly inline As InlineParser
        ReadOnly globalStyle As ConsoleFormat
        ReadOnly out As New System.Collections.Generic.List(Of TextSpan)

        Sub New(theme As MarkdownTheme, globalStyle As ConsoleFormat)
            Me.theme = theme
            Me.globalStyle = globalStyle
            Me.inline = New InlineParser(theme)
        End Sub

        ''' <summary>
        ''' parse the whole markdown document
        ''' </summary>
        ''' <param name="lines">the document lines, without the line feed char</param>
        ''' <returns></returns>
        Public Function Parse(lines As String()) As System.Collections.Generic.List(Of TextSpan)
            out.Clear()

            If lines Is Nothing Then
                Return out
            End If

            ' the table buffer is a local variable instead of a field of the
            ' renderer, so that the buffer state can never be leaked into the
            ' next table or into the next DoPrint call.
            Dim table As New System.Collections.Generic.List(Of String)
            Dim i As Integer = 0

            While i < lines.Length
                Dim line As String = lines(i)

                If IsBlank(line) Then
                    Call FlushTable(table)
                    Call out.Add(New TextSpan With {.text = LF, .style = globalStyle})

                    i += 1
                    Continue While
                End If

                If IsFence(line) Then
                    Call FlushTable(table)

                    i = ParseFencedCode(lines, i)
                    Continue While
                End If

                If IsHorizontalRule(line) Then
                    Call FlushTable(table)
                    Call EmitLine(line, ConsoleFormat.Combine(globalStyle, theme.HorizontalRule))

                    i += 1
                    Continue While
                End If

                If IsTableStart(lines, i) Then
                    ' the header row, the ``--|---`` delimiter row and then all
                    ' of the following data rows are buffered as one table
                    table.Add(line)

                    i += 1
                    table.Add(lines(i))

                    i += 1

                    While i < lines.Length AndAlso
                        Not IsBlank(lines(i)) AndAlso
                        lines(i).IndexOf("|"c) >= 0

                        table.Add(lines(i))
                        i += 1
                    End While

                    Call FlushTable(table)
                    Continue While
                End If

                If IsBlockQuote(line) Then
                    i = ParseBlockQuote(lines, i)
                    Continue While
                End If

                Dim level As Integer = HeaderLevel(line)

                If level > 0 Then
                    Dim title As String = line.TrimStart().Substring(level).Trim()

                    Call EmitLine(title, ConsoleFormat.Combine(globalStyle, theme.HeaderSpan))

                    i += 1
                    Continue While
                End If

                Dim marker As String = Nothing
                Dim content As String = StripListItem(line, marker)

                If content IsNot Nothing Then
                    Call EmitListItem(content, marker, IndentOf(line))

                    i += 1
                    Continue While
                End If

                ' a normal paragraph line
                Call EmitLine(line, globalStyle)

                i += 1
            End While

            ' the table at the end of the document is not terminated by a blank
            ' line, flush it or the last table will be dropped silently.
            Call FlushTable(table)

            Return out
        End Function

        ''' <summary>
        ''' the fenced code block is emitted as the plain text, its content is
        ''' never parsed by the inline parser.
        ''' </summary>
        ''' <param name="lines"></param>
        ''' <param name="from">the index of the opening fence line</param>
        ''' <returns>the index of the next line after the closing fence</returns>
        Private Function ParseFencedCode(lines As String(), from As Integer) As Integer
            Dim i As Integer = from + 1
            Dim codeStyle As ConsoleFormat = ConsoleFormat.Combine(globalStyle, theme.CodeBlock)

            While i < lines.Length AndAlso Not IsFence(lines(i))
                Call out.Add(New TextSpan With {.text = lines(i) & LF, .style = codeStyle})

                i += 1
            End While

            If i < lines.Length Then
                ' skips the closing fence line
                i += 1
            End If

            Return i
        End Function

        Private Function ParseBlockQuote(lines As String(), from As Integer) As Integer
            Dim i As Integer = from
            Dim quoteStyle As ConsoleFormat = ConsoleFormat.Combine(globalStyle, theme.BlockQuote)

            While i < lines.Length AndAlso IsBlockQuote(lines(i))
                ' the ``&gt;`` marker is replaced by a two spaces prefix, so that
                ' the quoted text block is still visible as an indented block
                Call EmitLine("  " & StripQuoteMarker(lines(i)), quoteStyle)

                i += 1
            End While

            Return i
        End Function

        Private Sub EmitListItem(content As String, marker As String, lead As Integer)
            Dim markerStyle As ConsoleFormat = ConsoleFormat.Combine(globalStyle, theme.ListMarker)

            Call out.Add(New TextSpan With {
                .text = New String(" "c, lead) & marker & " ",
                .style = markerStyle
            })

            Call AppendSpans(inline.Parse(content, globalStyle), globalStyle)
        End Sub

        Private Sub EmitLine(line As String, style As ConsoleFormat)
            Call AppendSpans(inline.Parse(line, style), style)
        End Sub

        ''' <summary>
        ''' appends the inline spans of one line and then terminates the line
        ''' </summary>
        ''' <param name="spans"></param>
        ''' <param name="fallbackStyle">
        ''' the style of the line feed char, which is required when the line has
        ''' no content at all.
        ''' </param>
        Private Sub AppendSpans(spans As System.Collections.Generic.List(Of TextSpan), fallbackStyle As ConsoleFormat)
            If spans Is Nothing OrElse spans.Count = 0 Then
                Call out.Add(New TextSpan With {.text = LF, .style = fallbackStyle})
            Else
                spans(spans.Count - 1).text &= LF

                Call out.AddRange(spans)
            End If
        End Sub

        Private Sub FlushTable(table As System.Collections.Generic.List(Of String))
            If table.Count = 0 Then
                Return
            End If

            ' the rendered table lines always carry the global style, so that
            ' they never inherit the color of the previous text span.
            For Each line As String In TableRenderer.Render(table, theme)
                Call out.Add(New TextSpan With {.text = line & LF, .style = globalStyle})
            Next

            Call table.Clear()
        End Sub

        Private Shared Function IsBlank(line As String) As Boolean
            Return String.IsNullOrWhiteSpace(line)
        End Function

        Private Shared Function IsFence(line As String) As Boolean
            Return line.TrimStart().StartsWith("```")
        End Function

        Private Shared Function IsBlockQuote(line As String) As Boolean
            Return line.TrimStart().StartsWith(">")
        End Function

        ''' <summary>
        ''' the ``---``/``***``/``___`` horizontal rule line
        ''' </summary>
        ''' <param name="line"></param>
        ''' <returns></returns>
        Private Shared Function IsHorizontalRule(line As String) As Boolean
            Dim test As String = line.Trim()

            If test.Length < 3 Then
                Return False
            End If

            Dim c As Char = test(0)

            If c <> "-"c AndAlso c <> "*"c AndAlso c <> "_"c Then
                Return False
            End If

            For Each ch As Char In test
                If ch <> c Then
                    Return False
                End If
            Next

            Return True
        End Function

        ''' <summary>
        ''' a table is started by a pipe line that is followed by the ``--|---``
        ''' delimiter row, this rule is much more reliable than just testing the
        ''' leading pipe char of the line.
        ''' </summary>
        ''' <param name="lines"></param>
        ''' <param name="i"></param>
        ''' <returns></returns>
        Private Shared Function IsTableStart(lines As String(), i As Integer) As Boolean
            If lines(i).IndexOf("|"c) < 0 Then
                Return False
            End If
            If i + 1 >= lines.Length Then
                Return False
            End If

            Return TableRenderer.IsDelimiterRow(lines(i + 1))
        End Function

        ''' <summary>
        ''' gets the atx header level of the given line, or ZERO if it is not a header
        ''' </summary>
        ''' <param name="line"></param>
        ''' <returns>a value between 1 and 6</returns>
        Private Shared Function HeaderLevel(line As String) As Integer
            Dim test As String = line.TrimStart()
            Dim n As Integer = 0

            While n < test.Length AndAlso test(n) = "#"c
                n += 1
            End While

            If n < 1 OrElse n > 6 Then
                Return 0
            End If
            If n < test.Length AndAlso test(n) <> " "c Then
                ' the ``#hashtag`` is not a header
                Return 0
            End If

            Return n
        End Function

        Private Shared Function IndentOf(line As String) As Integer
            Return line.Length - line.TrimStart().Length
        End Function

        ''' <summary>
        ''' strips the ``-``/``+``/``*``/``1.`` list marker from the line
        ''' </summary>
        ''' <param name="line"></param>
        ''' <param name="marker">the stripped marker text</param>
        ''' <returns>
        ''' the content of the list item, or a null reference if the given line is
        ''' not a list item at all.
        ''' </returns>
        Private Shared Function StripListItem(line As String, ByRef marker As String) As String
            Dim lead As Integer = IndentOf(line)

            If lead > 3 Then
                Return Nothing
            End If

            Dim test As String = line.TrimStart()

            If test.Length < 2 Then
                Return Nothing
            End If

            If (test(0) = "-"c OrElse test(0) = "+"c OrElse test(0) = "*"c) AndAlso test(1) = " "c Then
                marker = test.Substring(0, 1)

                Return New String(" "c, lead) & test.Substring(2)
            End If

            ' the ordered list item: "1. xxx"
            Dim dot As Integer = test.IndexOf("."c)

            If dot > 0 AndAlso dot <= 3 AndAlso test.Length > dot + 1 AndAlso test(dot + 1) = " "c Then
                Dim isDigits As Boolean = True

                For k As Integer = 0 To dot - 1
                    If Not Char.IsDigit(test(k)) Then
                        isDigits = False
                        Exit For
                    End If
                Next

                If isDigits Then
                    marker = test.Substring(0, dot + 1)

                    Return New String(" "c, lead) & test.Substring(dot + 2)
                End If
            End If

            Return Nothing
        End Function

        ''' <summary>
        ''' strips the leading ``&gt;`` marker and its following space
        ''' </summary>
        ''' <param name="line"></param>
        ''' <returns></returns>
        Private Shared Function StripQuoteMarker(line As String) As String
            Dim test As String = line.TrimStart()

            If test.Length > 0 AndAlso test(0) = ">"c Then
                test = test.Substring(1)

                If test.Length > 0 AndAlso test(0) = " "c Then
                    test = test.Substring(1)
                End If
            End If

            Return test
        End Function
    End Class
End Namespace
