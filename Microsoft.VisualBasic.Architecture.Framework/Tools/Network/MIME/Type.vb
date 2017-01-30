Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Net.Protocols.ContentTypes

    Public Class Type

        <XmlAttribute> Public Property Extension As String
        <XmlAttribute> Public Property ContentType As String

        Public Overrides Function ToString() As String
            Return $"<Default Extension=""{Extension}"" ContentType=""{ContentType}"" />"
        End Function

        <XmlType("Types", [Namespace]:="http://schemas.openxmlformats.org/package/2006/content-types")>
        Public Class Types

            <XmlElement("Default")> Public Property Defaults As Type()

            Public Overrides Function ToString() As String
                Return Me.GetJson
            End Function
        End Class
    End Class
End Namespace