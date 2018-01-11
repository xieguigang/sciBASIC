Imports System.Xml.Serialization

Namespace XML.xl

    <XmlType("calcChain", [Namespace]:="http://schemas.openxmlformats.org/spreadsheetml/2006/main")>
    Public Class calcChain
        <XmlElement("c")>
        Public Property c As c()
    End Class

    Public Class c
        <XmlAttribute> Public Property r As String
        <XmlAttribute> Public Property i As String
        <XmlAttribute> Public Property l As String
    End Class
End Namespace