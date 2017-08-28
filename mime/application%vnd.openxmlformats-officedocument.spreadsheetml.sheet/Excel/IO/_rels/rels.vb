Imports System.Xml.Serialization

Namespace _rels

    <XmlRoot("Relationships", [Namespace]:="http://schemas.openxmlformats.org/package/2006/relationships")>
    Public Class rels : Inherits IXml

        <XmlElement("Relationship")>
        Public Property Relationships As Relationship()

        Protected Overrides Function filePath() As String
            Return "_rels/.rels"
        End Function

        Protected Overrides Function toXml() As String
            Throw New NotImplementedException()
        End Function
    End Class

    Public Class Relationship
        <XmlAttribute> Public Property Id As String
        <XmlAttribute> Public Property Type As String
        <XmlAttribute> Public Property Target As String
    End Class
End Namespace