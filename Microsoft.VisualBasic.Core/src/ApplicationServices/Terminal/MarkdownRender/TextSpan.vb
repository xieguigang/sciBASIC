Imports System.Runtime.CompilerServices

Namespace ApplicationServices.Terminal

    Public Class TextSpan

        Public Property text As String
        Public Property style As ConsoleFormat
        Public Property IsEndByNewLine As Boolean

        Public Overrides Function ToString() As String
            Return AnsiEscapeCodes.ToAnsiEscapeSequenceSlow(style) & text
        End Function

        Public Shared Narrowing Operator CType(span As TextSpan) As String
            Return AnsiEscapeCodes.ToAnsiEscapeSequenceSlow(span.style) & span.text
        End Operator
    End Class
End Namespace