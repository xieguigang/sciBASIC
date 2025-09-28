Imports System.Xml.Serialization

Namespace Resource

    <XmlRoot("root"), XmlType("root")>
    Public Class resx

        Public Property schema As schema
        <XmlElement("resheader")> Public Property resheader As resheader()
        Public Property assembly As assembly
        <XmlElement("data")> Public Property data As data()

    End Class
End Namespace