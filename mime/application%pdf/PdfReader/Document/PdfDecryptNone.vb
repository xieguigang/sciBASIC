Imports System.Text

Namespace PdfReader
    Public Class PdfDecryptNone
        Inherits PdfDecrypt

        Public Sub New(ByVal parent As PdfObject)
            MyBase.New(parent)
        End Sub

        Public Overrides Function DecodeString(ByVal obj As PdfString) As String
            Return obj.ParseString.Value
        End Function

        Public Overrides Function DecodeStringAsBytes(ByVal obj As PdfString) As Byte()
            Return obj.ParseString.ValueAsBytes
        End Function

        Public Overrides Function DecodeStream(ByVal stream As PdfStream) As String
            Return Encoding.ASCII.GetString(stream.ParseStream.DecodeBytes(stream.ParseStream.StreamBytes))
        End Function

        Public Overrides Function DecodeStreamAsBytes(ByVal stream As PdfStream) As Byte()
            Return stream.ParseStream.DecodeBytes(stream.ParseStream.StreamBytes)
        End Function
    End Class
End Namespace
