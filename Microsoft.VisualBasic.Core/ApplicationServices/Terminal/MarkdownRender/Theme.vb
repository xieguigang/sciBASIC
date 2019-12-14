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

        Public Shared Widening Operator CType(colors As (fore As ConsoleColor, back As ConsoleColor)) As ConsoleFontStyle
            Return New ConsoleFontStyle With {
                .ForeColor = colors.fore,
                .BackgroundColor = colors.back
            }
        End Operator

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