
''' <summary>
''' A regular-expression literal <c>/pattern/flags</c> captured by the lexer
''' (the parser turns it into a <see cref="RegexExpr"/>).
''' </summary>
Public NotInheritable Class RegexLiteral

    Public ReadOnly Property Source As String
    Public ReadOnly Property Flags As String

    Public Sub New(source As String, flags As String)
        Me.Source = source
        Me.Flags = flags
    End Sub

End Class
