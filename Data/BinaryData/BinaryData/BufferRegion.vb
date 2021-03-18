Public Class BufferRegion

    Public Property position As Long
    Public Property size As Integer

    Public Overrides Function ToString() As String
        Return $"&{position} [{size} bytes]"
    End Function

End Class
