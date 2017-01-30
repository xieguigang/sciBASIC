Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text
Imports Microsoft.VisualBasic.Text.Xml

Namespace Net.Protocols.ContentTypes

    Public Class Type

        <XmlAttribute> Public Property Extension As String
        <XmlAttribute> Public Property ContentType As String

        Public Overrides Function ToString() As String
            Return $"<Default Extension=""{Extension}"" ContentType=""{ContentType}"" />"
        End Function

        Public Class Types

            Const xmlns$ = "http://schemas.openxmlformats.org/package/2006/content-types"

            <XmlElement("Default")> Public Property Defaults As Type()

            Public Overrides Function ToString() As String
                Return Me.GetJson
            End Function

            Public Function SaveAsXml(path$) As Boolean
                Dim xml As New XmlDoc(Me.GetXml)
                xml.xmlns.xsd = Nothing
                xml.xmlns.xsi = Nothing
                xml.xmlns.xmlns = xmlns
                xml.encoding = XmlEncodings.UTF8

                Return xml.ToString.SaveTo(path, Encodings.UTF8.GetEncodings)
            End Function
        End Class
    End Class
End Namespace