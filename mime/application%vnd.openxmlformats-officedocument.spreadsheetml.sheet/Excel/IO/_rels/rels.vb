#Region "Microsoft.VisualBasic::b89a0dd70d1e163ee27738cc16734a09, ..\sciBASIC#\mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel\IO\_rels\rels.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
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

Namespace XML._rels

    <XmlRoot("Relationships", [Namespace]:="http://schemas.openxmlformats.org/package/2006/relationships")>
    Public Class rels : Inherits IXml

        <XmlElement("Relationship")>
        Public Property Relationships As Relationship()

        Protected Overrides Function filePath() As String
            Return "_rels/.rels"
        End Function

        Protected Overrides Function toXml() As String
            Return Me.GetXml
        End Function
    End Class

    Public Class Relationship
        <XmlAttribute> Public Property Id As String
        <XmlAttribute> Public Property Type As String
        <XmlAttribute> Public Property Target As String
    End Class
End Namespace
