#Region "Microsoft.VisualBasic::06a8bd8d6ad9d0dd1ba940ce98d0b9ad, vs_solutions\dev\VisualStudio\Resource\xml\schema.vb"

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

    '   Total Lines: 60
    '    Code Lines: 37 (61.67%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 23 (38.33%)
    '     File Size: 1.39 KB


    '     Class schema
    ' 
    '         Properties: element, id, import
    ' 
    '     Class import
    ' 
    '         Properties: [namespace]
    ' 
    '     Class element
    ' 
    '         Properties: complexType, IsDataSet, name, type
    ' 
    '     Class sequence
    ' 
    '         Properties: element
    ' 
    '     Class complexType
    ' 
    '         Properties: attributes, choice
    ' 
    '     Class choice
    ' 
    '         Properties: elements, maxOccurs
    ' 
    '     Class attribute
    ' 
    '         Properties: minOccurs, name, Ordinal, ref, type
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml.Serialization

Namespace Resource

    Public Class schema

        Public Property id As String
        Public Property import As import
        Public Property element As element

    End Class

    Public Class import

        <XmlAttribute> Public Property [namespace] As String

    End Class

    Public Class element

        <XmlAttribute> Public Property name As String
        <XmlAttribute> Public Property IsDataSet As Boolean
        Public Property type As String

        Public Property complexType As complexType

    End Class

    Public Class sequence

        Public Property element As element

    End Class

    Public Class complexType

        Public Property choice As choice
        <XmlElement("attribute")>
        Public Property attributes As attribute()

    End Class

    Public Class choice

        <XmlAttribute> Public Property maxOccurs As String
        <XmlElement("element")>
        Public Property elements As element()

    End Class

    Public Class attribute

        <XmlAttribute> Public Property name As String
        <XmlAttribute> Public Property type As String
        <XmlAttribute> Public Property ref As String
        <XmlAttribute> Public Property minOccurs As Integer
        <XmlAttribute> Public Property Ordinal As Integer

    End Class
End Namespace
