#Region "Microsoft.VisualBasic::b7e41f68c4176adab2dab955c7ce1d5e, Microsoft.VisualBasic.Core\src\ApplicationServices\Terminal\MarkdownRender\MarkdownRender.vb"

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

    '   Total Lines: 304
    '    Code Lines: 138 (45.39%)
    ' Comment Lines: 127 (41.78%)
    '    - Xml Docs: 83.46%
    ' 
    '   Blank Lines: 39 (12.83%)
    '     File Size: 13.07 KB


    '     Class MarkdownRender
    ' 
    '         Properties: EnableAnsi, globalStyle, LastError
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: DefaultStyleRender, Render, WriteSpans
    ' 
    '         Sub: applyGlobal, DoPrint, Print, Reset
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Text

Namespace ApplicationServices.Terminal

    ''' <summary>
    ''' A simple markdown render on console
    ''' </summary>
    ''' <remarks>
    ''' 主要渲染下面的一些元素:
    ''' 
    ''' + code: 红色
    ''' + url: 蓝色
    ''' + blockquote: 灰色背景色
    ''' 
    ''' The document is rendered by a two stage pipeline: the <see cref="BlockParser"/>
    ''' splits the document into the block elements, and then the 
    ''' <see cref="InlineParser"/> splits each block into the styled text spans.
    ''' 
    ''' Supported block elements:
    ''' 
    ''' + the ``#`` atx header
    ''' + the ``` ``` ``` fenced code block
    ''' + the pipe table
    ''' + the ``&gt;`` block quote
    ''' + the ``-``/``+``/``*``/``1.`` list item
    ''' + the ``---`` horizontal rule
    ''' 
    ''' Supported inline elements:
    ''' 
    ''' + the ``\`` backslash escape
    ''' + the ``` `code` ``` inline code span
    ''' + the ``**bold**`` and the ``*italic*``
    ''' + the ``~~deleted~~`` strike through
    ''' + the ``[text](url)`` link and the ``![alt](url)`` image
    ''' + the bare url text
    ''' </remarks>
    Public Class MarkdownRender

        Shared ReadOnly defaultTheme As [Default](Of MarkdownTheme) = New MarkdownTheme With {
            .[Global] = Nothing,
            .BlockQuote = (ConsoleColor.Black, ConsoleColor.Gray),
            .CodeBlock = (ConsoleColor.Red, ConsoleColor.Yellow),
            .InlineCodeSpan = (ConsoleColor.Red, ConsoleColor.Black),
            .Url = (ConsoleColor.Blue, ConsoleColor.Black),
            .Bold = (ConsoleColor.Black, ConsoleColor.Yellow),
            .Italy = (ConsoleColor.Yellow, ConsoleColor.DarkGray),
            .HeaderSpan = (ConsoleColor.DarkGreen, ConsoleColor.Yellow),
            .StrikeThrough = New ConsoleFormat(ConsoleColor.DarkGray, ConsoleColor.Black) With {.Strikeout = True},
            .LinkText = New ConsoleFormat(ConsoleColor.Cyan, ConsoleColor.Black) With {.Underline = True},
            .ListMarker = (ConsoleColor.Green, ConsoleColor.Black),
            .HorizontalRule = (ConsoleColor.DarkGray, ConsoleColor.Black)
        }

        ReadOnly theme As MarkdownTheme
        ReadOnly initialGlobal As ConsoleFormat

        Sub New(theme As MarkdownTheme, Optional defaultBack As ConsoleColor? = Nothing, Optional defaultFore As ConsoleColor? = Nothing)
            Me.theme = theme

            If defaultBack Is Nothing OrElse defaultFore Is Nothing Then
                Me.initialGlobal = New ConsoleFormat With {
                    .Background = Console.BackgroundColor,
                    .Foreground = Console.ForegroundColor
                }
            Else
                Me.initialGlobal = New ConsoleFormat With {
                    .Background = defaultBack,
                    .Foreground = defaultFore
                }
            End If
        End Sub

        Sub New()
            Call Me.New(theme:=defaultTheme)
        End Sub

        ''' <summary>
        ''' print the given markdown text with current theme styles
        ''' </summary>
        ''' <param name="markdown$"></param>
        ''' <param name="indent%"></param>
        Public Sub DoPrint(markdown$, indent%)
            Call Console.Write(Render(markdown, indent))
        End Sub

        ''' <summary>
        ''' The error of the last <see cref="Render"/> call, which is not nothing
        ''' only when the renderer has fallen back to the plain text output.
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' The parser never throws to the caller, as an exception must never
        ''' leave the terminal in an uncontrolled color state. This property makes
        ''' the failure observable instead of silently swallowing it.
        ''' </remarks>
        Public Property LastError As Exception

        ''' <summary>
        ''' Should the ansi escape sequence be emitted into the rendered result?
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' The escape sequence is meaningless(and harmful) when the stdout is 
        ''' redirected into a file or a pipeline, or the user has turned off the 
        ''' ansi color via the ``ansi_color`` environment variable. This flag 
        ''' follows the same convention as the <see cref="Debugger"/> logging.
        ''' </remarks>
        Public Property EnableAnsi As Boolean = App.EnableAnsiColor AndAlso Not Console.IsOutputRedirected

        ''' <summary>
        ''' renders the given markdown text with the current theme styles into a
        ''' string that is decorated with the ansi escape sequence.
        ''' </summary>
        ''' <param name="markdown">the markdown text</param>
        ''' <param name="indent">the prefix space indent number.</param>
        ''' <returns>
        ''' a plain text string if the ansi color is disabled, otherwise the 
        ''' result is decorated with the ansi escape sequence and is always 
        ''' terminated by the color reset sequence.
        ''' </returns>
        ''' <remarks>
        ''' This function is a pure function: it does not touch the console, so 
        ''' that the rendered result can be verified by the unit test code.
        ''' </remarks>
        Public Function Render(markdown As String, Optional indent% = 0) As String
            Dim lines As String() = If(markdown, "").LineTokens
            Dim parser As New BlockParser(theme, globalStyle)
            Dim spans As System.Collections.Generic.List(Of TextSpan)

            Me.Reset()
            Me.applyGlobal()

            Try
                spans = parser.Parse(lines)

                LastError = Nothing
            Catch ex As Exception
                ' a broken markdown document must never leave the terminal in an
                ' uncontrolled color state, so that it falls back to the plain
                ' text rendering here. The error is kept in the 
                ' <see cref="LastError"/> property instead of being swallowed.
                Call Reset()
                Call applyGlobal()

                LastError = ex

                spans = New System.Collections.Generic.List(Of TextSpan) From {
                    New TextSpan With {.text = If(markdown, ""), .style = globalStyle}
                }
            End Try

            Return WriteSpans(spans, indent)
        End Function

        ''' <summary>
        ''' the style stack of the opened style spans
        ''' </summary>
        ''' <remarks>
        ''' This stack is only maintained for the <see cref="ConsoleFormat.PushStyle"/>
        ''' public API, the block/inline parsers of this module are stateless and do
        ''' not depend on it anymore, so that it can never grow up out of control.
        ''' </remarks>
        Friend styleStack As New Stack(Of ConsoleFormat)
        Friend currentStyle As ConsoleFormat

        ''' <summary>
        ''' resets the render state
        ''' </summary>
        Public Sub Reset()
            styleStack.Clear()
            currentStyle = Nothing
        End Sub

        ''' <summary>
        ''' the base style of the whole document
        ''' </summary>
        ''' <returns></returns>
        Private ReadOnly Property globalStyle As ConsoleFormat
            Get
                Return If(theme.Global, initialGlobal)
            End Get
        End Property

        ''' <summary>
        ''' assign the <see cref="globalStyle"/> as the current style.
        ''' </summary>
        Private Sub applyGlobal()
            Call globalStyle.Apply(Me)
        End Sub

        ''' <summary>
        ''' build the final console output text from the parsed text spans
        ''' </summary>
        ''' <param name="spans"></param>
        ''' <param name="indent%">the prefix space indent number.</param>
        ''' <returns></returns>
        ''' <remarks>
        ''' + the indent is emitted as a space prefix instead of moving the console 
        '''   cursor, as the <see cref="Console.CursorLeft"/> setter throws an 
        '''   <see cref="IO.IOException"/> when the stdout is redirected.
        ''' + the adjacent spans that share the same style are merged into one 
        '''   escape sequence, which shrinks the output size a lot.
        ''' + the result is always terminated by the color reset sequence.
        ''' </remarks>
        Private Function WriteSpans(spans As System.Collections.Generic.List(Of TextSpan), indent%) As String
            Dim ansi As Boolean = EnableAnsi
            ' the System.Text namespace is not imported here, as the Ascii class
            ' of the System.Text conflicts with the Microsoft.VisualBasic.Text.Ascii
            Dim sb As New System.Text.StringBuilder()
            Dim prefix As String = If(indent > 0, New String(" "c, indent), "")
            Dim atLineStart As Boolean = True
            Dim active As ConsoleFormat = Nothing
            Dim hasActive As Boolean = False

            For Each span As TextSpan In spans
                Dim text As String = If(span.text, "")
                Dim parts As String() = text.Split(New Char() {ASCII.LF})

                For i As Integer = 0 To parts.Length - 1
                    ' a text that is terminated by a line feed produces a trailing
                    ' empty part, which must not emit the prefix and the style again
                    If parts(i).Length > 0 OrElse i = 0 Then
                        If atLineStart Then
                            If prefix.Length > 0 Then
                                sb.Append(prefix)
                            End If

                            ' re-apply the style at the line begin, as some terminals
                            ' reset the text attributes on the line wrap
                            hasActive = False
                        End If

                        If ansi AndAlso (Not hasActive OrElse Not Equals(active, span.style)) Then
                            If span.style Is Nothing Then
                                ' a span without style must reset the terminal first,
                                ' or it will inherit the color of the previous span.
                                sb.Append(AnsiEscapeCodes.Reset)
                            Else
                                sb.Append(span.style.ToString())
                            End If

                            active = span.style
                            hasActive = True
                        End If

                        sb.Append(parts(i))
                    End If

                    If i < parts.Length - 1 Then
                        sb.Append(ASCII.LF)
                        atLineStart = True
                    End If
                Next

                atLineStart = text.Length > 0 AndAlso text(text.Length - 1) = ASCII.LF
            Next

            If Not atLineStart Then
                ' the rendered document should always be terminated by a line feed
                sb.Append(ASCII.LF)
            End If

            If ansi Then
                ' restore the terminal colors, or the console will stay in the color
                ' of the last text span and pollutes all of the following outputs.
                sb.Append(AnsiEscapeCodes.Reset)
            End If

            Return sb.ToString()
        End Function





        ''' <summary>
        ''' do console writeline with styles
        ''' </summary>
        ''' <param name="markdown">the markdown text to print on the console</param>
        ''' <param name="theme">
        ''' the theme styles for make console print
        ''' </param>
        ''' <param name="indent">the prefix space indent number.</param>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Sub Print(markdown As String, Optional theme As MarkdownTheme = Nothing, Optional indent% = 0)
            If App.Platform <> PlatformID.Win32NT Then
                Call New MarkdownRender(theme Or defaultTheme, ConsoleColor.Black, ConsoleColor.White).DoPrint(markdown, indent)
            Else
                Call New MarkdownRender(theme Or defaultTheme).DoPrint(markdown, indent)
            End If
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function DefaultStyleRender() As MarkdownRender
            If App.Platform <> PlatformID.Win32NT Then
                Return New MarkdownRender(defaultTheme, ConsoleColor.Black, ConsoleColor.White)
            Else
                Return New MarkdownRender(defaultTheme)
            End If
        End Function
    End Class
End Namespace
