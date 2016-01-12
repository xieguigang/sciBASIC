Namespace MMFProtocol

    ''' <summary>
    ''' exec cmd /var $&lt;piplineName>, this can be using in the CLI programming for passing the variables between the program more efficient
    ''' </summary>
    Public Class Pipeline

        ReadOnly _socket As MMFProtocol.MMFSocket

        Sub New(uid As String)
            _socket = New MMFSocket(uid)
        End Sub

        Public Function GetValue(Of T As Net.Protocol.RawStream)(var As String) As T
            Dim readBuffer As Byte() = _socket.ReadData
            Dim buffer As PipeBuffer =
                PipeStream.GetValue(readBuffer, var)
            Return Net.Protocol.RawStream.GetRawStream(Of T)(buffer.byteData)
        End Function
    End Class

    Public Class PipeBuffer : Inherits Net.Protocol.RawStream

        Public Property Name As String
        Public Property byteData As Byte()

        Sub New(raw As Byte())
            Dim nameLen As Byte() = New Byte(INT32 - 1) {}
            Dim p As Long = Scan0
            Call Array.ConstrainedCopy(raw, p.Move(INT32), nameLen, Scan0, INT32)

            Dim len As Integer = BitConverter.ToInt32(nameLen, Scan0)
            Dim name As Byte() = New Byte(len - 1) {}
            Call Array.ConstrainedCopy(raw, p.Move(name.Length), name, Scan0, len)
            Me.Name = System.Text.Encoding.UTF8.GetString(name)

            byteData = New Byte(raw.Length - INT32 - len - 1) {}
            Call Array.ConstrainedCopy(raw, p, byteData, Scan0, byteData.Length)
        End Sub

        Public Overrides Function Serialize() As Byte()
            Dim nameBuf As Byte() = System.Text.Encoding.UTF8.GetBytes(Name)
            Dim buffer As Byte() = New Byte(INT32 + nameBuf.Length + byteData.Length - 1) {}
            Dim nameLen As Byte() = BitConverter.GetBytes(nameBuf.Length)
            Dim p As Long = Scan0

            Call Array.ConstrainedCopy(nameLen, Scan0, buffer, p.Move(nameLen.Length), nameLen.Length)
            Call Array.ConstrainedCopy(nameBuf, Scan0, buffer, p.Move(nameBuf.Length), nameBuf.Length)
            Call Array.ConstrainedCopy(byteData, Scan0, buffer, p.Move(byteData.Length), byteData.Length)

            Return buffer
        End Function
    End Class

    Public Class PipeStream : Inherits Net.Protocol.RawStream

        Public Property hashTable As Dictionary(Of String, PipeBuffer)

        Sub New(raw As Byte())

        End Sub

        Public Overrides Function Serialize() As Byte()
            Throw New NotImplementedException
        End Function

        Public Shared Function GetValue(raw As Byte(), name As String) As PipeBuffer
            Dim i As Long = Scan0

            Do While True
                Dim buffer As Byte() = raw
            Loop

            Return Nothing
        End Function
    End Class
End Namespace