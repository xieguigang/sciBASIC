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
            .Url = (ConsoleColor.Blue, ConsoleColor.Black)
        }

        Dim theme As MarkdownTheme
        Dim markdown As CharPtr

        Private Sub New(theme As MarkdownTheme)
            Me.theme = theme
        End Sub

        Public Sub DoPrint(markdown As String)
            Me.markdown = markdown
            Me.DoPrint()
        End Sub

        Private Sub DoPrint()
            Do While Not markdown.EndRead
                Call WalkChar(++markdown)
            Loop
        End Sub

        Dim inlineCodespan As Boolean = False
        Dim buf As New List(Of Char)

        Private Sub WalkChar(c As Char)
            If inlineCodespan Then
                Call My.Log4VB.Print(c)

            Else
                Select Case c
                    Case "`"c
                        buf += c
                    Case Else

                        Call Console.Write(c)
                End Select
            End If
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Sub Print(markdown As String, Optional theme As MarkdownTheme = Nothing)
            Call New MarkdownRender(theme Or defaultTheme).DoPrint(markdown)
        End Sub
    End Class

    Public Class MarkdownTheme

        Public Property Url As ConsoleFontStyle
        Public Property InlineCodeSpan As ConsoleFontStyle
        Public Property CodeBlock As ConsoleFontStyle
        Public Property BlockQuote As ConsoleFontStyle
        Public Property [Global] As ConsoleFontStyle

    End Class

    Public Class ConsoleFontStyle

        Public Property ForeColor As ConsoleColor = ConsoleColor.White
        Public Property BackgroundColor As ConsoleColor = ConsoleColor.Black

        Public Shared Widening Operator CType(colors As (fore As ConsoleColor, back As ConsoleColor)) As ConsoleFontStyle
            Return New ConsoleFontStyle With {
                .ForeColor = colors.fore,
                .BackgroundColor = colors.back
            }
        End Operator
    End Class
End Namespace
