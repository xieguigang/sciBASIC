Imports System.Runtime.CompilerServices

Namespace ApplicationServices.Terminal

    Public Class TextSpan

        Public Property text As String

        ''' <summary>
        ''' print the pnain text if the style is nothing
        ''' </summary>
        ''' <returns></returns>
        Public Property style As ConsoleFormat
        Public Property IsEndByNewLine As Boolean

        Sub New()
        End Sub

        Sub New(s As String, style As ConsoleFormat)
            Me.text = s
            Me.style = style
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            If style Is Nothing Then
                Return text
            End If

            Return AnsiEscapeCodes.ToAnsiEscapeSequenceSlow(style) & text
        End Function

        Public Shared Narrowing Operator CType(span As TextSpan) As String
            If span.style Is Nothing Then
                Return span.text
            End If

            Return AnsiEscapeCodes.ToAnsiEscapeSequenceSlow(span.style) & span.text
        End Operator
    End Class
End Namespace