Imports System.Xml.Serialization

Namespace Net.Protocols.Streams

    ''' <summary>
    ''' 字符串序列流
    ''' </summary>
    Public Class [String] : Inherits RawStream

        <XmlAttribute> Public Property value As String

        <XmlAttribute> Public Property Encoding As Encodings
            Get
                Return TextEncodings.GetEncodings(_encoding)
            End Get
            Set(value As Encodings)
                _encoding = value.GetEncodings
            End Set
        End Property

        Dim _encoding As System.Text.Encoding

        Sub New()
        End Sub

        Sub New(s As String, Optional encoding As TextEncodings.Encodings = Encodings.UTF8)
            Call Me.New(s, encoding.GetEncodings)
        End Sub

        Sub New(s As String, Optional encoding As System.Text.Encoding = Nothing)
            _value = s
            _encoding = encoding

            If _encoding Is Nothing Then
                _encoding = System.Text.Encoding.UTF8
            End If
        End Sub

        Sub New(raw As Byte())
            Dim encoding As Byte = raw(Scan0)
            Dim s As Byte() = raw.Skip(1).ToArray

            _encoding = CType(encoding, Encodings).GetEncodings
            _value = _encoding.GetString(s)
        End Sub

        Public Overrides Function ToString() As String
            Return value
        End Function

        Public Overrides Function Serialize() As Byte()
            Dim s As Byte() = _encoding.GetBytes(value)
            Dim buffer As Byte() = New Byte(s.Length) {}

            buffer(Scan0) = CType(Encoding, Byte)
            Call System.Array.ConstrainedCopy(s, Scan0, buffer, 1, s.Length)

            Return buffer
        End Function
    End Class
End Namespace