Imports Microsoft.VisualBasic.Net.Protocols

Namespace MMFProtocol.Pipeline

    Public Class PipeBuffer : Inherits RawStream

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
End Namespace