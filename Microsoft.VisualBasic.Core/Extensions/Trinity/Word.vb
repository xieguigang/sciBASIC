Imports System.Xml.Serialization

Namespace Data.Trinity.NLP

    Public Class Word

        <XmlAttribute> Public Property [Class] As WordClass
        <XmlAttribute> Public Property Text As String

        Public Overrides Function ToString() As String
            Return $"{[Class].Description} {Text}"
        End Function
    End Class
End Namespace