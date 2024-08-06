#Region "Microsoft.VisualBasic::b8afdcaf910b1cd7349433ea0f243840, mime\application%rdf+xml\DataModel\Description.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 16
    '    Code Lines: 11 (68.75%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 5 (31.25%)
    '     File Size: 409 B


    ' Class Description
    ' 
    '     Properties: about
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml.Serialization

<XmlType("abstract_rdf_description", [Namespace]:=RDFEntity.xmlns_nil)>
Public MustInherit Class Description

    <XmlNamespaceDeclarations()>
    Public xmlns As New XmlSerializerNamespaces

    <XmlAttribute("about", [Namespace]:=RDFEntity.XmlnsNamespace)>
    Public Property about As String

    <XmlElement("type", [Namespace]:=RDFEntity.XmlnsNamespace)>
    Public Property type As RDFType

    <XmlElement("label", [Namespace]:=RDFEntity.rdfs)>
    Public Property label As String

    <XmlElement("comment", [Namespace]:=RDFEntity.rdfs)>
    Public Property comment As String

    Sub New()
        xmlns.Add("rdf", RDFEntity.XmlnsNamespace)
        xmlns.Add("rdfs", RDFEntity.rdfs)
    End Sub

    Public Overrides Function ToString() As String
        Return $"{label};  # {comment}"
    End Function

End Class
