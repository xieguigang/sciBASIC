#Region "Microsoft.VisualBasic::910ee6437347dc3d1bb75b508f99c772, Microsoft.VisualBasic.Core\Net\Protocol\Streams\String.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Class [String]
    ' 
    '         Properties: Encoding, value
    ' 
    '         Constructor: (+4 Overloads) Sub New
    '         Function: Serialize, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Text
Imports BufferArray = System.Array

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
                _encoding = value.CodePage
            End Set
        End Property

        Dim _encoding As Encoding

        Sub New()
        End Sub

        Sub New(s As String, Optional encoding As Encodings = Encodings.UTF8)
            Call Me.New(s, encoding.CodePage)
        End Sub

        Sub New(s As String, Optional encoding As Encoding = Nothing)
            _value = s
            _encoding = encoding

            If _encoding Is Nothing Then
                _encoding = Encoding.UTF8
            End If
        End Sub

        Sub New(raw As Byte())
            Dim encoding As Byte = raw(Scan0)
            Dim s As Byte() = raw.Skip(1).ToArray

            _encoding = CType(encoding, Encodings).CodePage
            _value = _encoding.GetString(s)
        End Sub

        Public Overrides Function ToString() As String
            Return value
        End Function

        Public Overrides Function Serialize() As Byte()
            Dim s As Byte() = _encoding.GetBytes(value)
            Dim buffer As Byte() = New Byte(s.Length) {}

            buffer(Scan0) = CType(Encoding, Byte)
            Call BufferArray.ConstrainedCopy(s, Scan0, buffer, 1, s.Length)

            Return buffer
        End Function
    End Class
End Namespace
