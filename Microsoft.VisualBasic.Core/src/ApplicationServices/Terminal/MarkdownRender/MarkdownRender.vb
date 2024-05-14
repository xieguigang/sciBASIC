#Region "Microsoft.VisualBasic::07492a03a92310d4696595b66ba87ecd, Microsoft.VisualBasic.Core\src\ApplicationServices\Terminal\MarkdownRender\MarkdownRender.vb"

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

    '   Total Lines: 378
    '    Code Lines: 288
    ' Comment Lines: 45
    '   Blank Lines: 45
    '     File Size: 13.93 KB


    '     Class MarkdownRender
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: bufferAllIs, bufferIs, DefaultStyleRender
    ' 
    '         Sub: applyGlobal, buildTableSimple, DoParseSpans, DoPrint, EndSpan
    '              Print, PrintSpans, Reset, restoreStyle, WalkChar
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Terminal.TablePrinter
Imports Microsoft.VisualBasic.ApplicationServices.Terminal.TablePrinter.Flags
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Text
Imports Microsoft.VisualBasic.Text.Parser

Namespace ApplicationServices.Terminal

    ''' <summary>
    ''' A simple markdown render on console
    ''' </summary>
    ''' <remarks>
    ''' 主要是渲染下面的一些元素:
    ''' 
    ''' + code: 红色
    ''' + url: 蓝色
    ''' + blockquote: 灰色背景色
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
            .HeaderSpan = (ConsoleColor.DarkGreen, ConsoleColor.Yellow)
        }

        Dim theme As MarkdownTheme
        Dim markdown As CharPtr
        Dim indent As Integer

        Dim initialGlobal As ConsoleFormat

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
            Me.markdown = markdown.LineTokens.JoinBy(ASCII.LF)
            Me.indent = indent
            Me.applyGlobal()
            Me.Reset()
            Me.DoParseSpans()
            Me.PrintSpans()
        End Sub

        Private Sub DoParseSpans()
            Do While Not markdown.EndRead
                Call WalkChar(++markdown)
            Loop

            If textBuf > 0 Then
                Call EndSpan(False)
            End If
        End Sub

        Dim boldSpan As Boolean = False
        Dim inlineCodespan As Boolean = False
        Dim blockquote As Boolean = False
        Dim italySpan As Boolean = False
        Dim headerSpan As Boolean = False
        Dim lastNewLine As Boolean = True
        Dim controlBuf As New List(Of Char)
        Dim textBuf As New List(Of Char)
        Dim tableSpan As Boolean = False

        Friend styleStack As New Stack(Of ConsoleFormat)
        Friend currentStyle As ConsoleFormat

        Dim spans As New List(Of TextSpan)

        Public Sub Reset()
            blockquote = False
            boldSpan = False
            inlineCodespan = False
            lastNewLine = True
            italySpan = False
            headerSpan = False
            controlBuf *= 0
            textBuf *= 0
            styleStack.Clear()
            spans *= 0
        End Sub

        ''' <summary>
        ''' <see cref="controlBuf"/> is in given string pattern?
        ''' </summary>
        ''' <param name="term"></param>
        ''' <returns></returns>
        Private Function bufferIs(term As String) As Boolean
            If controlBuf <> term.Length Then
                Return False
            Else
                Return controlBuf.SequenceEqual(term)
            End If
        End Function

        ''' <summary>
        ''' All of the character value in <see cref="controlBuf"/> is equals to given character <paramref name="c"/>
        ''' </summary>
        ''' <param name="c"></param>
        ''' <returns></returns>
        Private Function bufferAllIs(c As Char) As Boolean
            If controlBuf = 0 Then
                Return False
            Else
                Return controlBuf.All(Function(b) b = c)
            End If
        End Function

        Private Sub restoreStyle()
            If styleStack.Count > 0 Then
                Call styleStack.Pop()
            End If

            If styleStack.Count = 0 Then
                Call applyGlobal()
            Else
                Call styleStack.Peek.SetConfig(Me)
            End If
        End Sub

        Private Sub applyGlobal()
            If theme.Global Is Nothing Then
                Call initialGlobal.SetConfig(Me)
            Else
                Call theme.Global.SetConfig(Me)
            End If
        End Sub

        Private Sub PrintSpans()
            Dim isNewLine As Boolean = True

            For Each span As TextSpan In spans
                If isNewLine Then
                    Console.CursorLeft = indent
                End If

                Console.Write(span)
                isNewLine = span.IsEndByNewLine
            Next

            Call applyGlobal()
            Call Console.WriteLine()
        End Sub

        ''' <summary>
        ''' create new <see cref="TextSpan"/> and then clear the <see cref="textBuf"/> 
        ''' for pull next text span object.
        ''' </summary>
        ''' <param name="byNewLine"></param>
        Private Sub EndSpan(byNewLine As Boolean)
            Dim text As String = textBuf.CharString
            Dim style As ConsoleFormat = currentStyle.Clone

            If text.StartsWith("((http(s)?)|(ftp))[:]//", RegexICSng) Then
                style = theme.Url
            End If

            If styleStack.Count > 0 AndAlso styleStack.Peek.Equals(theme.CodeBlock) Then
                style.Background = theme.CodeBlock.Background
            End If

            If text.Length > 0 Then
                spans += New TextSpan With {
                    .style = style,
                    .text = text,
                    .IsEndByNewLine = byNewLine
                }
                textBuf *= 0
            End If
        End Sub

        Dim tableBuf As New List(Of String)

        Private Sub buildTableSimple()
            If tableBuf.Count = 0 Then
                spans.Add(New TextSpan With {.IsEndByNewLine = True, .text = ""})
            End If

            Dim header As String() = tableBuf(0).Split("|"c)
            Dim rows As String()() = tableBuf.Skip(2) _
                .Where(Function(r) Not Strings.Trim(r).StringEmpty) _
                .Select(Function(l) l.Split("|"c)) _
                .ToArray
            Dim tbl As New ConsoleTableBaseData(header, rows)
            Dim println As String = ConsoleTableBuilder.From(tbl) _
                .WithFormat(theme.Table) _
                .Export _
                .ToString

            For Each line As String In println.LineTokens
                Call spans.Add(New TextSpan With {.text = line & vbCrLf, .IsEndByNewLine = True})
            Next

            spans.Add(New TextSpan With {.IsEndByNewLine = True, .text = vbLf})
        End Sub

        Private Sub WalkChar(c As Char)
            If tableSpan Then
                Select Case c
                    Case ASCII.LF
                        lastNewLine = True
                        tableBuf.Add(New String(textBuf.PopAll))

                        If tableBuf.Last.StringEmpty Then
                            ' break the table context by empty line
                            tableSpan = False
                            buildTableSimple()
                        End If

                        Return
                    Case ""
                        ' is a empty line
                        ' end of table context
                        tableSpan = False
                        tableBuf.Add(New String(textBuf.PopAll))
                        buildTableSimple()
                    Case ">"c, "`"c, "#"c
                        ' end of table context
                        tableSpan = False
                        tableBuf.Add(New String(textBuf.PopAll))
                        buildTableSimple()
                    Case Else
                        textBuf += c
                        Return
                End Select
            End If

            Select Case c
                Case "`"c
                    controlBuf += c
                    lastNewLine = False
                Case "*"c
                    controlBuf += c
                    lastNewLine = False
                Case ASCII.LF
                    lastNewLine = True
                    blockquote = False
                    headerSpan = False
                    textBuf += ASCII.LF
                    EndSpan(True)
                    restoreStyle()
                Case "|"c
                    If lastNewLine AndAlso controlBuf = 0 Then
                        ' is probably a markdown table
                        tableSpan = True
                    End If

                    textBuf += c
                    lastNewLine = False
                Case ">"c
                    If lastNewLine AndAlso controlBuf = 0 Then
                        controlBuf += c
                    ElseIf lastNewLine AndAlso bufferIs(">") Then
                        ' 空白的blockquote行
                    Else
                        textBuf += c
                    End If
                    lastNewLine = False
                Case " "c
                    lastNewLine = False

                    If controlBuf = 1 AndAlso controlBuf(Scan0) = ">"c Then
                        blockquote = True
                        controlBuf *= 0
                        theme.BlockQuote.SetConfig(Me)
                        textBuf += " "c
                        textBuf += " "c
                    ElseIf headerSpan AndAlso bufferAllIs("#"c) Then
                        theme.HeaderSpan.SetConfig(Me)
                        controlBuf *= 0
                    Else
                        EndSpan(False)
                        textBuf += c
                        EndSpan(False)
                    End If
                Case "#"c
                    If lastNewLine AndAlso (controlBuf = 0 OrElse controlBuf.All(Function(x) x = "#"c)) Then
                        controlBuf += c
                        headerSpan = True
                    ElseIf headerSpan AndAlso textBuf = 0 Then
                        controlBuf += c
                    Else
                        textBuf += c
                    End If

                    lastNewLine = False
                Case Else
                    lastNewLine = False

                    If bufferIs("``") Then
                        If inlineCodespan Then
                            ' 结束栈
                            EndSpan(False)
                            inlineCodespan = False
                            restoreStyle()
                        Else
                            EndSpan(False)
                            inlineCodespan = True
                            theme.InlineCodeSpan.SetConfig(Me)
                        End If

                        controlBuf *= 0
                        textBuf += c
                    ElseIf bufferIs("**") Then
                        If boldSpan Then
                            EndSpan(False)
                            boldSpan = False
                            restoreStyle()
                        Else
                            EndSpan(False)
                            boldSpan = True
                            theme.Bold.SetConfig(Me)
                        End If

                        controlBuf *= 0
                        textBuf += c
                    Else
                        textBuf += c
                    End If
            End Select
        End Sub

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
