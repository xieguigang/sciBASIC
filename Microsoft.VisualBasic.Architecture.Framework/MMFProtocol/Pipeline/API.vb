Namespace MMFProtocol.Pipeline

    Public Module API

        Enum Protocols
            Allocation
        End Enum

        ''' <summary>
        ''' 生成的映射位置为:  &lt;var>:&lt;ChunkSize>
        ''' </summary>
        ''' <param name="var"></param>
        ''' <param name="value"></param>
        ''' <returns></returns>
        Public Function WriteData(var As String, value As Net.Protocol.RawStream) As Boolean
            Dim buf As Byte() = value.Serialize
            Dim chunkSize As Long = buf.Length
            Dim ref As String = $"{var}:{chunkSize}"

        End Function

        Public Function TryGetValue() As Byte()

        End Function
    End Module
End Namespace