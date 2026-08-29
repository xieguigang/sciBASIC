#Region "Microsoft.VisualBasic::8fce888ebf0338915421d8e777f3bcff, Microsoft.VisualBasic.Core\src\ApplicationServices\Terminal\MarkdownRender\InlineParser.vb"

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

    '   Total Lines: 417
    '    Code Lines: 241 (57.79%)
    ' Comment Lines: 95 (22.78%)
    '    - Xml Docs: 73.68%
    ' 
    '   Blank Lines: 81 (19.42%)
    '     File Size: 14.83 KB


    '     Class InlineParser
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: CountRun, FindClosing, IsUrlChar, IsWordBoundary, IsWordChar
    '                   Parse, TryBareUrl, TryEmphasis, TryEscape, TryInlineCode
    '                   TryLink, TryStrikeThrough
    ' 
    '         Sub: Emit, Flush, ParseRange
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace ApplicationServices.Terminal

    ''' <summary>
    ''' The inline level markdown parser: it walks through one single line of
    ''' text and then splits the line into a set of the styled text spans.
    ''' </summary>
    ''' <remarks>
    ''' The inline elements are matched by the priority as follows:
    ''' 
    ''' 1. ``\`` backslash escape
    ''' 2. `` ` `` inline code span, the content of the code span is never parsed again
    ''' 3. ``**``/``__`` bold, and the ``*``/``_`` italic
    ''' 4. ``~~`` strike through
    ''' 5. ``[text](url)`` link and the ``![alt](url)`` image
    ''' 6. the bare url text
    ''' 
    ''' All of the look-ahead buffers of this parser are local variables, so that
    ''' there is no global state that can be polluted by the un-matched delimiter
    ''' characters anymore.
    ''' </remarks>
    Friend Class InlineParser

        ReadOnly theme As MarkdownTheme
        ReadOnly spans As New System.Collections.Generic.List(Of TextSpan)
        ' the System.Text namespace is not imported, as the System.Text.Ascii
        ' conflicts with the Microsoft.VisualBasic.Text.Ascii
        ReadOnly buf As New System.Text.StringBuilder()

        Dim line As String
        Dim pos As Integer
        ''' <summary>
        ''' the working range of the current parse call
        ''' </summary>
        Dim rangeStart As Integer
        Dim rangeEnd As Integer
        ''' <summary>
        ''' the base style of the current working range
        ''' </summary>
        Dim style As ConsoleFormat

        Sub New(theme As MarkdownTheme)
            Me.theme = theme
        End Sub

        ''' <summary>
        ''' parse one single line of markdown text
        ''' </summary>
        ''' <param name="line">the text of the line, without the line feed char</param>
        ''' <param name="baseStyle">
        ''' the style of the block element that contains this line, e.g. the header
        ''' style or the block quote style. The inline styles are merged on top of it.
        ''' </param>
        ''' <returns></returns>
        Public Function Parse(line As String, baseStyle As ConsoleFormat) As System.Collections.Generic.List(Of TextSpan)
            Me.line = If(line, "")
            Me.spans.Clear()
            Me.buf.Clear()
            Me.style = baseStyle
            Me.pos = 0

            Call ParseRange(0, Me.line.Length, baseStyle)

            Return Me.spans
        End Function

        Private Sub ParseRange(startIdx As Integer, endIdx As Integer, rangeStyle As ConsoleFormat)
            Dim savedStart As Integer = rangeStart
            Dim savedEnd As Integer = rangeEnd
            Dim savedStyle As ConsoleFormat = style

            rangeStart = startIdx
            rangeEnd = endIdx
            style = rangeStyle
            pos = startIdx

            While pos < rangeEnd
                Dim c As Char = line(pos)

                If c = "\"c AndAlso TryEscape() Then
                    Continue While
                End If
                If c = "`"c AndAlso TryInlineCode() Then
                    Continue While
                End If
                If (c = "*"c OrElse c = "_"c) AndAlso TryEmphasis() Then
                    Continue While
                End If
                If c = "~"c AndAlso TryStrikeThrough() Then
                    Continue While
                End If
                If c = "["c AndAlso TryLink() Then
                    Continue While
                End If
                If TryBareUrl() Then
                    Continue While
                End If

                Call buf.Append(c)

                pos += 1
            End While

            Call Flush()

            pos = endIdx
            rangeStart = savedStart
            rangeEnd = savedEnd
            style = savedStyle
        End Sub

        ''' <summary>
        ''' ``\*`` -> ``*``, only the punctuation and the symbol chars can be escaped
        ''' </summary>
        ''' <returns></returns>
        Private Function TryEscape() As Boolean
            If pos + 1 >= rangeEnd Then
                Return False
            End If

            Dim nxt As Char = line(pos + 1)

            If Not (Char.IsPunctuation(nxt) OrElse Char.IsSymbol(nxt)) Then
                Return False
            End If

            Call buf.Append(nxt)

            pos += 2

            Return True
        End Function

        ''' <summary>
        ''' the ``` ``code`` ``` span, the content of the code span is emitted as
        ''' the plain text and is never parsed again.
        ''' </summary>
        ''' <returns></returns>
        Private Function TryInlineCode() As Boolean
            Dim fence As Integer = CountRun("`"c, pos)
            Dim closeAt As Integer = FindClosing("`"c, fence, pos + fence)

            If closeAt < 0 Then
                Return False
            End If

            Call Flush()
            Call Emit(line.Substring(pos + fence, closeAt - pos - fence), ConsoleFormat.Combine(style, theme.InlineCodeSpan))

            pos = closeAt + fence

            Return True
        End Function

        ''' <summary>
        ''' ``**bold**``, ``*italic*`` and the ``***both***`` spans
        ''' </summary>
        ''' <returns></returns>
        Private Function TryEmphasis() As Boolean
            Dim c As Char = line(pos)
            Dim run As Integer = CountRun(c, pos)

            ' *** and the longer delimiter runs are all treated as the bold + italic
            If run > 3 Then
                run = 3
            End If

            Dim closeAt As Integer = FindClosing(c, run, pos + run)

            If closeAt < 0 Then
                Return False
            End If
            If c = "_"c AndAlso Not IsWordBoundary(pos, closeAt + run) Then
                ' the underscore inside of a word, e.g. my_var_name, is not an
                ' emphasis delimiter at all
                Return False
            End If

            Dim emphasis As ConsoleFormat

            If run = 1 Then
                emphasis = ConsoleFormat.Combine(style, theme.Italy)
            ElseIf run = 2 Then
                emphasis = ConsoleFormat.Combine(style, theme.Bold)
            Else
                ' the ``***both***`` is the bold + italic: the colors are merged
                ' by an override rule, so that the italic color wins here and the
                ' ansi bold attribute is turned on explicitly to keep the "***"
                ' span distinguishable from the plain italic one.
                emphasis = ConsoleFormat.Combine(ConsoleFormat.Combine(style, theme.Bold), theme.Italy).Clone()
                emphasis.Bold = True
            End If

            Call Flush()
            ' the content of the emphasis span is parsed recursively, so that the
            ' nested inline elements are still working
            Call ParseRange(pos + run, closeAt, emphasis)

            pos = closeAt + run

            Return True
        End Function

        ''' <summary>
        ''' the ``~~deleted~~`` span
        ''' </summary>
        ''' <returns></returns>
        Private Function TryStrikeThrough() As Boolean
            If CountRun("~"c, pos) < 2 Then
                Return False
            End If

            Dim closeAt As Integer = FindClosing("~"c, 2, pos + 2)

            If closeAt < 0 Then
                Return False
            End If

            Call Flush()
            Call ParseRange(pos + 2, closeAt, ConsoleFormat.Combine(style, theme.StrikeThrough))

            pos = closeAt + 2

            Return True
        End Function

        ''' <summary>
        ''' the ``[text](url)`` link and the ``![alt](url)`` image
        ''' </summary>
        ''' <returns></returns>
        Private Function TryLink() As Boolean
            Dim closeBracket As Integer = line.IndexOf("]"c, pos + 1)

            If closeBracket < 0 OrElse closeBracket + 1 >= rangeEnd Then
                Return False
            End If
            If line(closeBracket + 1) <> "("c Then
                Return False
            End If

            Dim closeParen As Integer = line.IndexOf(")"c, closeBracket + 2)

            If closeParen < 0 OrElse closeParen >= rangeEnd Then
                ' the closing paren must be located inside of the current range
                Return False
            End If

            Dim text As String = line.Substring(pos + 1, closeBracket - pos - 1)
            Dim url As String = line.Substring(closeBracket + 2, closeParen - closeBracket - 2)
            ' drops the optional link title: [text](http://x "tip")
            Dim titleAt As Integer = url.IndexOf(" "c)

            If titleAt > 0 Then
                url = url.Substring(0, titleAt)
            End If

            Dim isImage As Boolean = pos > rangeStart AndAlso line(pos - 1) = "!"c

            If isImage AndAlso buf.Length > 0 AndAlso buf(buf.Length - 1) = "!"c Then
                ' the leading ``!`` of the image was already buffered as the plain text
                buf.Length -= 1
            End If

            Call Flush()

            If isImage Then
                ' there is no way to draw the image on the console, so that only
                ' the alt text is rendered here
                Call Emit(text, ConsoleFormat.Combine(style, theme.Url))
            Else
                If text.Length > 0 Then
                    Call Emit(text, ConsoleFormat.Combine(style, If(theme.LinkText, theme.Url)))
                End If

                Call Emit(" (" & url & ")", ConsoleFormat.Combine(style, theme.Url))
            End If

            pos = closeParen + 1

            Return True
        End Function

        ''' <summary>
        ''' the bare url text, e.g. the ``http://xxx``/``https://xxx``/``ftp://xxx``
        ''' </summary>
        ''' <returns></returns>
        Private Function TryBareUrl() As Boolean
            Dim scheme As String = Nothing

            For Each test As String In urlSchemes
                If pos + test.Length > rangeEnd Then
                    ' the remaining text is shorter than the scheme, the
                    ' String.Compare call below throws an out of range error
                    ' when the compared length runs over the string end.
                    Continue For
                End If
                If String.Compare(line, pos, test, 0, test.Length, StringComparison.OrdinalIgnoreCase) = 0 Then
                    scheme = test
                    Exit For
                End If
            Next

            If scheme Is Nothing Then
                Return False
            End If
            If pos > rangeStart AndAlso IsUrlChar(line(pos - 1)) Then
                ' only match at the word begin, e.g. the url text of 
                ' ``(http://xxx)`` should be matched, but the one of 
                ' ``xxxhttp://xxx`` should not.
                Return False
            End If

            Dim i As Integer = pos + scheme.Length

            While i < rangeEnd AndAlso IsUrlChar(line(i))
                i += 1
            End While
            ' the trailing punctuation is a part of the sentence, not of the url
            While i > pos + scheme.Length AndAlso urlTrimChars.IndexOf(line(i - 1)) >= 0
                i -= 1
            End While

            Call Flush()
            Call Emit(line.Substring(pos, i - pos), ConsoleFormat.Combine(style, theme.Url))

            pos = i

            Return True
        End Function

        ''' <summary>
        ''' the ``_emphasis_`` delimiter is only working at the word boundary
        ''' </summary>
        ''' <param name="openAt"></param>
        ''' <param name="closeAt"></param>
        ''' <returns></returns>
        Private Function IsWordBoundary(openAt As Integer, closeAt As Integer) As Boolean
            If openAt > rangeStart AndAlso IsWordChar(line(openAt - 1)) Then
                Return False
            End If
            If closeAt < rangeEnd AndAlso IsWordChar(line(closeAt)) Then
                Return False
            End If

            Return True
        End Function

        Private Function CountRun(c As Char, from As Integer) As Integer
            Dim n As Integer = 0

            While from + n < rangeEnd AndAlso line(from + n) = c
                n += 1
            End While

            Return n
        End Function

        ''' <summary>
        ''' finds the closing delimiter run that has exactly the same length as
        ''' the <paramref name="run"/> value.
        ''' </summary>
        ''' <param name="c"></param>
        ''' <param name="run"></param>
        ''' <param name="from"></param>
        ''' <returns>the index of the closing delimiter, or -1 if not found.</returns>
        Private Function FindClosing(c As Char, run As Integer, from As Integer) As Integer
            Dim i As Integer = from

            While i < rangeEnd
                If line(i) = c Then
                    Dim n As Integer = CountRun(c, i)

                    If n = run Then
                        Return i
                    End If

                    i += n
                Else
                    i += 1
                End If
            End While

            Return -1
        End Function

        Private Sub Flush()
            If buf.Length > 0 Then
                Call spans.Add(New TextSpan With {.text = buf.ToString(), .style = style})

                buf.Length = 0
            End If
        End Sub

        Private Sub Emit(text As String, textStyle As ConsoleFormat)
            If Not String.IsNullOrEmpty(text) Then
                Call spans.Add(New TextSpan With {.text = text, .style = textStyle})
            End If
        End Sub

        Private Shared ReadOnly urlSchemes As String() = {"https://", "http://", "ftp://"}
        Private Shared ReadOnly urlTrimChars As String = ".,;:!?)]}'""" & ChrW(&H201D) & ChrW(&HFF09)

        Private Shared Function IsWordChar(c As Char) As Boolean
            Return Char.IsLetterOrDigit(c) OrElse c = "_"c
        End Function

        Private Shared Function IsUrlChar(c As Char) As Boolean
            If Char.IsLetterOrDigit(c) Then
                Return True
            End If

            Return urlReservedChars.IndexOf(c) >= 0
        End Function

        Private Shared ReadOnly urlReservedChars As String = "-._~:/?#[]@!$&'()*+,;=%"

    End Class
End Namespace
