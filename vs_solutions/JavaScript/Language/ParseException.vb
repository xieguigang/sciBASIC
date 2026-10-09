
''' <summary>Syntax error raised by the lexer and parser (carries source position).</summary>
Public NotInheritable Class ParseException
    Inherits Exception

    Public Sub New(message As String)
        MyBase.New(message)
    End Sub

End Class