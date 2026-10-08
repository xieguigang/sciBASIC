Imports Microsoft.VisualBasic.Scripting.TokenIcer

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
Public NotInheritable Class Token : Inherits CodeToken(Of TokenType)

    ''' <summary>Parsed value for Number (Double) and String (String) tokens.</summary>
    Public ReadOnly Property Value As Object
    ''' <summary>True when a newline separates this token from the previous one (for ASI).</summary>
    Public ReadOnly Property LineBreakBefore As Boolean

    Public ReadOnly Property Location As String
        Get
            Return $"{span.line}:{span.start}"
        End Get
    End Property

    Public Sub New(type As TokenType, text As String, value As Object,
                   line As Integer, col As Integer, lineBreakBefore As Boolean)
        Me.name = type
        Me.text = text
        Me.Value = value
        Me.span = New CodeSpan With {.line = line, .start = col}
        Me.LineBreakBefore = lineBreakBefore
    End Sub

    Public Overrides Function ToString() As String
        Return $"{name} '{text}' @ {span.ToString}"
    End Function

End Class
