Imports System.Xml.Serialization

Namespace Resource

    Public Class resheader

        <XmlAttribute>
        Public Property name As String
        Public Property value As String

    End Class

    Public Class assembly

        <XmlAttribute> Public Property [alias] As String
        <XmlAttribute> Public Property name As String

    End Class

    Public Class data

        <XmlAttribute> Public Property name As String
        <XmlAttribute> Public Property type As String
        Public Property value As String

    End Class


End Namespace