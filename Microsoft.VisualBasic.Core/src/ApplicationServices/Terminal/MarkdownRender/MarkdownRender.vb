#Region "Microsoft.VisualBasic::5950d7ab9de8c7aa48cc2d7899e80d56, sciBASIC#\Microsoft.VisualBasic.Core\src\ApplicationServices\Terminal\MarkdownRender\MarkdownRender.vb"

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

    '   Total Lines: 284
    '    Code Lines: 228
    ' Comment Lines: 22
    '   Blank Lines: 34
    '     File Size: 9.81 KB


    '     Class MarkdownRender
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: bufferAllIs, bufferIs, DefaultStyleRender
    ' 
    '         Sub: applyGlobal, DoParseSpans, DoPrint, EndSpan, Print
    '              PrintSpans, Reset, restoreStyle, WalkChar
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
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

#If NET_48 Or netcore5 = 1 Then

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
#End If
        Dim theme As MarkdownTheme
        Dim markdown As CharPtr
        Dim indent As Integer

        Dim initialGlobal As ConsoleFontStyle

        Private Sub New(theme As MarkdownTheme)
            Me.theme = theme
            Me.initialGlobal = New ConsoleFontStyle With {
                .BackgroundColor = Console.BackgroundColor,
                .ForeColor = Console.ForegroundColor
            }
        End Sub

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

        Friend styleStack As New Stack(Of ConsoleFontStyle)
        Friend currentStyle As ConsoleFontStyle

        Dim spans As New List(Of Span)

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

            For Each span As Span In spans
                If isNewLine Then
                    Console.CursorLeft = indent
                End If

                span.Print()
                isNewLine = span.IsEndByNewLine
            Next

            Call applyGlobal()
            Call Console.WriteLine()
        End Sub

        Private Sub EndSpan(byNewLine As Boolean)
            Dim text As String = textBuf.CharString
            Dim style As ConsoleFontStyle = currentStyle.Clone

            If text.StartsWith("((http(s)?)|(ftp))[:]//", RegexICSng) Then
                style = theme.Url
            End If

            If styleStack.Count > 0 AndAlso styleStack.Peek.Equals(theme.CodeBlock) Then
                style.BackgroundColor = theme.CodeBlock.BackgroundColor
            End If

            If text.Length > 0 Then
                spans += New Span With {
                    .style = style,
                    .text = text,
                    .IsEndByNewLine = byNewLine
                }
                textBuf *= 0
            End If
        End Sub

        Private Sub WalkChar(c As Char)
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

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Sub Print(markdown As String, Optional theme As MarkdownTheme = Nothing, Optional indent% = 0)
#If NET_48 Or netcore5 = 1 Then
            Call New MarkdownRender(theme Or defaultTheme).DoPrint(markdown, indent)
#Else
            Throw New NotImplementedException
#End If
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function DefaultStyleRender() As MarkdownRender
#If NET_48 Or netcore5 = 1 Then
            Return New MarkdownRender(defaultTheme)
#Else
            Throw New NotImplementedException
#End If
        End Function
    End Class
End Namespace
