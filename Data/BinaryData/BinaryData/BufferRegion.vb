Public Class BufferRegion

    Public Property position As Long
    Public Property size As Integer

    Public ReadOnly Property nextBlock As Long
        Get
            Return position + size
        End Get
    End Property

    Public Overrides Function ToString() As String
        Return $"&{position} [{size} bytes]"
    End Function

End Class
