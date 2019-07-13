Public Class FrameStream

    Public ReadOnly Property length As Integer

    ReadOnly temp$

    Sub New(hashCode As Integer, buf As Byte())
        length = buf.Length
        temp = App.GetAppSysTempFile(".frame", App.PID, $"/{hashCode}/")
        buf.FlushStream(temp)
    End Sub

    Public Overrides Function ToString() As String
        Return $"{length} bytes"
    End Function

    Public Shared Narrowing Operator CType(stream As FrameStream) As Byte()
        Return stream.temp.ReadBinary
    End Operator
End Class
