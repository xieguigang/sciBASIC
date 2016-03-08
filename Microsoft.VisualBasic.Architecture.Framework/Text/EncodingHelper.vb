Imports System.Text

Namespace Text

    Public Class EncodingHelper

        Public ReadOnly Property TextEncoding As Encoding

        Sub New(encoding As Encodings)
            TextEncoding = encoding.GetEncodings
        End Sub

        Public Function GetBytes(s As String) As Byte()
            Return TextEncoding.GetBytes(s)
        End Function

        Public Overloads Function ToString(byts As Byte()) As String
            Return TextEncoding.GetString(byts)
        End Function

        Public Overrides Function ToString() As String
            Return TextEncoding.ToString
        End Function
    End Class
End Namespace
