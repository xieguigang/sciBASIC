Public Class Score

    Public ReadOnly Property pwm As String
        Get
            Return Mid(seq, start + 1, len)
        End Get
    End Property

    Public Property seq As String
    Public Property start As Integer
    Public Property len As Integer

    Public Overrides Function ToString() As String
        Return pwm
    End Function

End Class