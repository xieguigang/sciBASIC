Public Class XmlElement

    Public Property name As String
    Public Property [namespace] As String
    Public Property attributes As Dictionary(Of String, String)
    Public Property elements As XmlElement()
    Public Property text As String

    Public ReadOnly Property id As String
        Get
            Return attributes _
                .Where(Function(a) a.Key = "id") _
                .FirstOrDefault _
                .Value
        End Get
    End Property

    Public Function getElementById(id As String) As XmlElement
        Return elements.Where(Function(a) a.id = id).FirstOrDefault
    End Function

    Public Iterator Function getElementsByTagName(name As String) As IEnumerable(Of XmlElement)
        For Each element As XmlElement In elements
            If element.name = name Then
                Yield element
            End If
        Next
    End Function

    Public Overrides Function ToString() As String
        Return $"{[namespace]}::{name}"
    End Function

    Public Shared Function ParseXmlText(xmlText As String) As XmlElement
        Return XmlParser.ParseXml(xmlText)
    End Function

End Class
