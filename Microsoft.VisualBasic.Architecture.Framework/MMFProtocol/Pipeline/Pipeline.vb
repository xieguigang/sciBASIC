Namespace MMFProtocol.Pipeline

    ''' <summary>
    ''' exec cmd /var $&lt;piplineName>, this can be using in the CLI programming for passing the variables between the program more efficient
    ''' </summary>
    Public Class Pipeline

        ReadOnly _socket As MMFSocket
        ReadOnly _netSocket As Net.TcpSynchronizationServicesSocket

        Sub New(port As Integer)
            _netSocket = New Net.TcpSynchronizationServicesSocket(port)
        End Sub

        Public Function GetValue(Of T As Net.Protocol.RawStream)(var As String) As T
            Dim readBuffer As Byte() = _socket.ReadData
            Dim buffer As PipeBuffer =
                PipeStream.GetValue(readBuffer, var)
            Return Net.Protocol.RawStream.GetRawStream(Of T)(buffer.byteData)
        End Function
    End Class
End Namespace