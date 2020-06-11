Imports System.Xml.Serialization

''' <summary>
''' a data resource reference
''' </summary>
Public Class Resource

    <XmlAttribute("resource", [Namespace]:=RDFEntity.XmlnsNamespace)>
    Public Property resource As String
End Class

Public Class DataValue

    <XmlAttribute("datatype", [Namespace]:=RDFEntity.XmlnsNamespace)>
    Public Property datatype As String
    <XmlText>
    Public Property value As String

End Class