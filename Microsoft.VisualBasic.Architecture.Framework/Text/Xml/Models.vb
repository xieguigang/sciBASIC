Imports System.Xml.Serialization

Namespace Text.Xml.Models

    Public Class StringValue
        <XmlAttribute> Public Property value As String

        Public Overrides Function ToString() As String
            Return value
        End Function
    End Class
End Namespace