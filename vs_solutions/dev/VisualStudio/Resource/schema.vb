Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

Namespace Resource

    Public Class schema

        Public Property id As String
        Public Property import As import
        Public Property element As element

    End Class

    Public Class import

        <XmlAttribute> Public Property [namespace] As String

    End Class

    Public Class element

        <XmlAttribute> Public Property name As IStringGetter
        <XmlAttribute> Public Property IsDataSet As Boolean
        Public Property type As String

        Public Property complexType As complexType

    End Class

    Public Class sequence

        Public Property element As element

    End Class

    Public Class complexType

        Public Property choice As choice
        <XmlElement("attribute")>
        Public Property attributes As attribute()

    End Class

    Public Class choice

        <XmlAttribute> Public Property maxOccurs As String
        <XmlElement("element")>
        Public Property elements As element()

    End Class

    Public Class attribute

        <XmlAttribute> Public Property name As String
        <XmlAttribute> Public Property type As String
        <XmlAttribute> Public Property ref As String
        <XmlAttribute> Public Property minOccurs As Integer
        <XmlAttribute> Public Property Ordinal As Integer

    End Class
End Namespace