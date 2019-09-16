#Region "Microsoft.VisualBasic::a5699a5d49ef0db4aa216c04f353be47, Microsoft.VisualBasic.Core\ApplicationServices\Terminal\MarkdownRender.vb"

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

'     Class MarkdownRender
' 
'         Constructor: (+1 Overloads) Sub New
'         Sub: (+2 Overloads) DoPrint, Print, WalkChar
' 
'     Class MarkdownTheme
' 
'         Properties: [Global], BlockQuote, CodeBlock, InlineCodeSpan, Url
' 
'     Class ConsoleFontStyle
' 
'         Properties: BackgroundColor, ForeColor
' 
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Serialization
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
            .[Global] = (ConsoleColor.White, ConsoleColor.Black),
            .BlockQuote = (ConsoleColor.Black, ConsoleColor.Gray),
            .CodeBlock = (ConsoleColor.Red, ConsoleColor.Yellow),
            .InlineCodeSpan = (ConsoleColor.Red, ConsoleColor.Black),
            .Url = (ConsoleColor.Blue, ConsoleColor.Black),
            .Bold = (ConsoleColor.Yellow, ConsoleColor.Black)
        }

        Dim theme As MarkdownTheme
        Dim markdown As CharPtr
        Dim indent As Integer

        Private Sub New(theme As MarkdownTheme)
            Me.theme = theme
        End Sub

        Public Sub DoPrint(markdown$, indent%)
            Me.markdown = markdown.LineTokens.JoinBy(ASCII.LF)
            Me.indent = indent
            Me.theme.Global.SetConfig(Me)
            Me.DoParseSpans()
            Me.PrintSpans()
        End Sub

        Private Sub DoParseSpans()
            Do While Not markdown.EndRead
                Call WalkChar(++markdown)
            Loop
        End Sub

        Dim boldSpan As Boolean = False
        Dim inlineCodespan As Boolean = False
        Dim blockquote As Boolean = False
        Dim lastNewLine As Boolean
        Dim controlBuf As New List(Of Char)
        Dim textBuf As New List(Of Char)

        Friend styleStack As New Stack(Of ConsoleFontStyle)
        Friend currentStyle As ConsoleFontStyle

        Dim spans As New List(Of Span)

        Private Function bufferIs(term As String) As Boolean
            If controlBuf <> term.Length Then
                Return False
            Else
                Return controlBuf.SequenceEqual(term)
            End If
        End Function

        Private Function bufferAllIs(c As Char) As Boolean
            If controlBuf = 0 Then
                Return False
            Else
                Return controlBuf.All(Function(b) b = c)
            End If
        End Function

        Private Sub restoreStyle()
            styleStack.Pop()

            If styleStack.Count = 0 Then
                Call theme.Global.SetConfig(Me)
            Else
                Call styleStack.Peek.SetConfig(Me)
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
        End Sub

        Private Sub EndSpan(byNewLine As Boolean)
            Dim text As String = textBuf.CharString
            Dim style As ConsoleFontStyle = currentStyle.Clone

            If text.StartsWith("((http(s)?)|(ftp))[:]//", RegexICSng) Then
                style = theme.Url
            End If

            If styleStack.Peek.Equals(theme.CodeBlock) Then
                style.BackgroundColor = theme.CodeBlock.BackgroundColor
            End If

            spans += New Span With {
                .style = style,
                .text = text,
                .IsEndByNewLine = byNewLine
            }
            textBuf *= 0
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
                    Else
                        EndSpan(False)
                        textBuf += c
                        EndSpan(False)
                    End If
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
            Call New MarkdownRender(theme Or defaultTheme).DoPrint(markdown, indent)
        End Sub
    End Class

    Public Class MarkdownTheme

        Public Property Url As ConsoleFontStyle
        Public Property InlineCodeSpan As ConsoleFontStyle
        Public Property CodeBlock As ConsoleFontStyle
        Public Property BlockQuote As ConsoleFontStyle
        Public Property [Global] As ConsoleFontStyle
        Public Property Bold As ConsoleFontStyle

    End Class

    Public Class ConsoleFontStyle
        Implements IEquatable(Of ConsoleFontStyle)
        Implements ICloneable(Of ConsoleFontStyle)

        Public Property ForeColor As ConsoleColor = ConsoleColor.White
        Public Property BackgroundColor As ConsoleColor = ConsoleColor.Black

        Public Sub SetConfig(render As MarkdownRender)
            Console.ForegroundColor = ForeColor
            Console.BackgroundColor = BackgroundColor

            render.currentStyle = Me
            render.styleStack.Push(Me)
        End Sub

        Public Function CreateSpan(text As String) As Span
            Return New Span With {
                .style = Me,
                .text = text
            }
        End Function

        Public Function Clone() As ConsoleFontStyle Implements ICloneable(Of ConsoleFontStyle).Clone
            Return New ConsoleFontStyle With {
                .BackgroundColor = BackgroundColor,
                .ForeColor = ForeColor
            }
        End Function

        Public Overloads Function Equals(other As ConsoleFontStyle) As Boolean Implements IEquatable(Of ConsoleFontStyle).Equals
            If other Is Nothing Then
                Return False
            Else
                Return BackgroundColor = other.BackgroundColor AndAlso ForeColor = other.ForeColor
            End If
        End Function

        Public Shared Widening Operator CType(colors As (fore As ConsoleColor, back As ConsoleColor)) As ConsoleFontStyle
            Return New ConsoleFontStyle With {
                .ForeColor = colors.fore,
                .BackgroundColor = colors.back
            }
        End Operator
    End Class

    Public Class Span

        Public Property text As String
        Public Property style As ConsoleFontStyle
        Public Property IsEndByNewLine As Boolean

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Print()
            Call My.Log4VB.Print(Me)
        End Sub
    End Class
End Namespace
