#Region "Microsoft.VisualBasic::194870d454153b4e2c6e2490c39ae3b1, mime\application%rdf+xml\DataModel\Description.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    ' Class Description
    ' 
    '     Properties: about
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    ' /********************************************************************************/

#End Region

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
