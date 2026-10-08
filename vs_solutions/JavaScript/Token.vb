Option Strict On
Option Explicit On

''' <summary>Token categories produced by <see cref="Lexer"/>.</summary>
Public Enum TokenType
    Number
    [String]
    Identifier
    Keyword
    Punct
    Eof
End Enum

''' <summary>A lexical token; <see cref="LineBreakBefore"/> drives the simple ASI rule.</summary>
Public NotInheritable Class Token

    Public ReadOnly Property Type As TokenType
    Public ReadOnly Property Text As String
    ''' <summary>Parsed value for Number (Double) and String (String) tokens.</summary>
    Public ReadOnly Property Value As Object
    Public ReadOnly Property Line As Integer
    Public ReadOnly Property Col As Integer
    ''' <summary>True when a newline separates this token from the previous one (for ASI).</summary>
    Public ReadOnly Property LineBreakBefore As Boolean

    Public Sub New(type As TokenType, text As String, value As Object,
                   line As Integer, col As Integer, lineBreakBefore As Boolean)
        Me.Type = type
        Me.Text = text
        Me.Value = value
        Me.Line = line
        Me.Col = col
        Me.LineBreakBefore = lineBreakBefore
    End Sub

    Public Overrides Function ToString() As String
        Return $"{Type} '{Text}' @ {Line}:{Col}"
    End Function

End Class
