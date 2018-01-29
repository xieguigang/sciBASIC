#Region "Microsoft.VisualBasic::fe30857ae50a6f726f64ecb327fa08d4, ..\sciBASIC#\mime\application%xml\xsd\XML.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Xml.Serialization

Namespace xsd

    Public Class include

        <XmlAttribute>
        Public Property schemaLocation As String

        Public Overrides Function ToString() As String
            Return schemaLocation
        End Function
    End Class

    Public Class complexType

        <XmlAttribute>
        Public Property name As String

        Public Property sequence As sequence
        Public Property attribute As attribute
    End Class

    Public Class attribute

        <XmlAttribute> Public Property name As String
        <XmlAttribute> Public Property type As String
        <XmlAttribute> Public Property use As String

        Public Property annotation As annotation

        Public Overrides Function ToString() As String
            Return $"Dim {name} As {type}"
        End Function
    End Class

    Public Class sequence

        Public Property element As element

        Public Overrides Function ToString() As String
            Return element.ToString
        End Function
    End Class

    Public Class element

        <XmlAttribute> Public Property minOccurs As String
        <XmlAttribute> Public Property maxOccurs As String
        <XmlAttribute> Public Property name As String
        <XmlAttribute> Public Property type As String

        Public Property annotation As annotation

        Public Overrides Function ToString() As String
            Return $"Dim {name} As {type}"
        End Function
    End Class

    Public Class annotation

        Public Property documentation As String

        Public Overrides Function ToString() As String
            Return documentation
        End Function
    End Class
End Namespace
