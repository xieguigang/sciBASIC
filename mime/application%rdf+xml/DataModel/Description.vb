Imports System.Xml.Serialization

<XmlType("Description", [Namespace]:=RDF.Namespace)>
Public Class Description

    <XmlNamespaceDeclarations()>
    Public xmlns As XmlSerializerNamespaces

    Sub New()
        xmlns.Add("rdf", RDF.Namespace)
    End Sub

    <XmlAttribute("about", [Namespace]:=RDF.Namespace)>
    Public Property about As String
End Class
