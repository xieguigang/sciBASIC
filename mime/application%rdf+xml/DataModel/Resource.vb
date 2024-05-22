#Region "Microsoft.VisualBasic::13ba9d21855914366acc9bfbe37a1c7a, mime\application%rdf+xml\DataModel\Resource.vb"

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

    '   Total Lines: 11
    '    Code Lines: 5 (45.45%)
    ' Comment Lines: 3 (27.27%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 3 (27.27%)
    '     File Size: 245 B


    ' Class Resource
    ' 
    '     Properties: resource
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml.Serialization

''' <summary>
''' a data resource reference
''' </summary>
Public Class Resource

    <XmlAttribute("resource", [Namespace]:=RDFEntity.XmlnsNamespace)>
    Public Property resource As String

End Class
