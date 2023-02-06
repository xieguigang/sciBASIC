Friend Class StringTokenizer
    Private line As String
    Private v As String

    Public Sub New(line As String, v As String)
        Me.line = line
        Me.v = v
    End Sub

    Friend ReadOnly Property countTokens As Integer
        Get
            Throw New NotImplementedException()
        End Get
    End Property

    Friend Function nextToken() As String
        Throw New NotImplementedException()
    End Function
End Class
