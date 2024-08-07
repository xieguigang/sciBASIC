#Region "Microsoft.VisualBasic::1631558438f9cab4d79c8d6d45cc4563, mime\application%rdf+xml\DataModel\Resource.vb"

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

    '   Total Lines: 24
    '    Code Lines: 14 (58.33%)
    ' Comment Lines: 4 (16.67%)
    '    - Xml Docs: 75.00%
    ' 
    '   Blank Lines: 6 (25.00%)
    '     File Size: 577 B


    ' Class Resource
    ' 
    '     Properties: resource
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: ToString
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml.Serialization

''' <summary>
''' a data resource reference
''' </summary>
''' 
<XmlType("resource", [Namespace]:=RDFEntity.xmlns_nil)>
Public Class Resource

    <XmlAttribute("resource", [Namespace]:=RDFEntity.XmlnsNamespace)>
    Public Property resource As String

    <XmlNamespaceDeclarations()>
    Public xmlns As New XmlSerializerNamespaces

    Sub New()
        Call xmlns.Add("rdf", RDFEntity.XmlnsNamespace)
    End Sub

    Public Overrides Function ToString() As String
        Return resource
    End Function

End Class
