Imports System.Xml.Serialization

Namespace xsd

    Public Class include

        <XmlAttribute>
        Public Property schemaLocation As String

        Public Overrides Function ToString() As String
            Return schemaLocation
        End Function
    End Class

    Public Class complexType

        <XmlAttribute>
        Public Property name As String

        Public Property sequence As sequence
        Public Property attribute As attribute
    End Class

    Public Class attribute

        <XmlAttribute> Public Property name As String
        <XmlAttribute> Public Property type As String
        <XmlAttribute> Public Property use As String

        Public Property annotation As annotation

        Public Overrides Function ToString() As String
            Return $"Dim {name} As {type}"
        End Function
    End Class

    Public Class sequence

        Public Property element As element

        Public Overrides Function ToString() As String
            Return element.ToString
        End Function
    End Class

    Public Class element

        <XmlAttribute> Public Property minOccurs As String
        <XmlAttribute> Public Property maxOccurs As String
        <XmlAttribute> Public Property name As String
        <XmlAttribute> Public Property type As String

        Public Property annotation As annotation

        Public Overrides Function ToString() As String
            Return $"Dim {name} As {type}"
        End Function
    End Class

    Public Class annotation

        Public Property documentation As String

        Public Overrides Function ToString() As String
            Return documentation
        End Function
    End Class
End Namespace