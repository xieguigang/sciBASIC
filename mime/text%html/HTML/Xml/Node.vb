Imports System.Xml.Serialization

Namespace HTML.XmlMeta

    Public MustInherit Class Node

        <XmlAttribute> Public Property id As String
        <XmlAttribute> Public Property [class] As String
        <XmlAttribute> Public Property style As String

    End Class

    Public Class GenericNode : Inherits Node

        <XmlAttribute> Public Property Tag As String

    End Class
End Namespace