Imports System.Xml.Serialization

Namespace Text.Xml.Models

    Public MustInherit Class ListOf

        <XmlAttribute> Public Property size As Integer
            Get
                Return getSize()
            End Get
            Set(value As Integer)
                ' do nothing
            End Set
        End Property

        Protected MustOverride Function getSize() As Integer

    End Class
End Namespace