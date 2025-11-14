Imports System.Xml.Serialization

Namespace Text.Xml.Models

    Public Class ints
        <XmlAttribute> Public Property ints As Integer()
    End Class

    Public Class uints
        <XmlAttribute> Public Property uints As UInteger()
    End Class

    Public Class longs
        <XmlAttribute> Public Property longs As Long()
    End Class

    Public Class ulongs
        <XmlAttribute> Public Property ulongs As ULong()
    End Class

    Public Class doubles
        <XmlAttribute> Public Property doubles As Double()
    End Class

    Public Class floats
        <XmlAttribute> Public Property floats As Single()
    End Class
End Namespace