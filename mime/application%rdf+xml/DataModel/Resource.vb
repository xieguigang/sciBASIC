#Region "Microsoft.VisualBasic::3343a3813507482c7ff0576988818369, sciBASIC#\mime\application%rdf+xml\DataModel\Resource.vb"

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

    '   Total Lines: 26
    '    Code Lines: 17
    ' Comment Lines: 3
    '   Blank Lines: 6
    '     File Size: 662.00 B


    ' Class Resource
    ' 
    '     Properties: resource
    ' 
    ' Class DataValue
    ' 
    '     Properties: type, value
    ' 
    '     Function: ToString
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.Serialization
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Serialization.JSON

''' <summary>
''' a data resource reference
''' </summary>
Public Class Resource

    <XmlAttribute("resource", [Namespace]:=RDFEntity.XmlnsNamespace)>
    Public Property resource As String
End Class

Public Class DataValue

    <XmlAttribute("datatype", [Namespace]:=RDFEntity.XmlnsNamespace)>
    <DataMember(Name:="type")>
    Public Property type As String
    <XmlText>
    Public Property value As String

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function

End Class
