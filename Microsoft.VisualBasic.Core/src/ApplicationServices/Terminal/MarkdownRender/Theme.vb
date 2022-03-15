#Region "Microsoft.VisualBasic::587f9b9f54d37f97bae2d8ebfe6abdc6, sciBASIC#\Microsoft.VisualBasic.Core\src\ApplicationServices\Terminal\MarkdownRender\Theme.vb"

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

    '   Total Lines: 101
    '    Code Lines: 79
    ' Comment Lines: 0
    '   Blank Lines: 22
    '     File Size: 3.40 KB


    '     Class MarkdownTheme
    ' 
    '         Properties: [Global], BlockQuote, Bold, CodeBlock, HeaderSpan
    '                     InlineCodeSpan, Italy, Url
    ' 
    '     Class ConsoleFontStyle
    ' 
    '         Properties: BackgroundColor, ForeColor
    ' 
    '         Function: Clone, CreateSpan, Equals, HtmlColorCode
    ' 
    '         Sub: Apply, SetConfig
    ' 
    '     Class Span
    ' 
    '         Properties: IsEndByNewLine, style, text
    ' 
    '         Function: ToString
    ' 
    '         Sub: Print
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization

Namespace ApplicationServices.Terminal

    Public Class MarkdownTheme

        Public Property Url As ConsoleFontStyle
        Public Property InlineCodeSpan As ConsoleFontStyle
        Public Property CodeBlock As ConsoleFontStyle
        Public Property BlockQuote As ConsoleFontStyle
        Public Property [Global] As ConsoleFontStyle
        Public Property Bold As ConsoleFontStyle
        Public Property Italy As ConsoleFontStyle
        Public Property HeaderSpan As ConsoleFontStyle

    End Class

    Public Class ConsoleFontStyle
        Implements IEquatable(Of ConsoleFontStyle)
        Implements ICloneable(Of ConsoleFontStyle)

        Public Property ForeColor As ConsoleColor = ConsoleColor.White
        Public Property BackgroundColor As ConsoleColor = ConsoleColor.Black

        Public Sub SetConfig(render As MarkdownRender)
            Call Apply()

            render.currentStyle = Me
            render.styleStack.Push(Me)
        End Sub

        Public Sub Apply()
            Console.ForegroundColor = ForeColor
            Console.BackgroundColor = BackgroundColor
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

#If NET_48 Or netcore5 = 1 Then

        Public Shared Widening Operator CType(colors As (fore As ConsoleColor, back As ConsoleColor)) As ConsoleFontStyle
            Return New ConsoleFontStyle With {
                .ForeColor = colors.fore,
                .BackgroundColor = colors.back
            }
        End Operator

#End If

        Public Shared Function HtmlColorCode(color As ConsoleColor) As String
            Return Drawing.Color.FromName(color.ToString).ToHtmlColor
        End Function
    End Class

    Public Class Span

        Public Property text As String
        Public Property style As ConsoleFontStyle
        Public Property IsEndByNewLine As Boolean

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Print()
            Call My.Log4VB.Print(Me)
        End Sub

        Public Overrides Function ToString() As String
            Dim text$ = Me.text

            If text.StringEmpty Then
                text = "<whitespace>"
            Else
                text = $"""{text}"""
            End If

            Return style.ForeColor.DoCall(AddressOf ConsoleFontStyle.HtmlColorCode) & " " & text
        End Function
    End Class
End Namespace
