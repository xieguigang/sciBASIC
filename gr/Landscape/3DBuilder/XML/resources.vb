#Region "Microsoft.VisualBasic::8457df965f69a834762b1f1d895277f1, ..\sciBASIC#\gr\Landscape\3DBuilder\XML\resources.vb"

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
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Landscape
Imports Microsoft.VisualBasic.Serialization.JSON

Public Class base

    <XmlAttribute> Public Property name As String
    <XmlAttribute> Public Property displaycolor As String

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function
End Class

Public Class resources

    Public Property basematerials As basematerials
    <XmlElement("object")>
    Public Property objects As [object]()

End Class

Public Interface Iobject
    <XmlAttribute> Property id As Integer
End Interface

Public Class [object] : Implements Iobject

    <XmlAttribute("id")>
    Public Property id As Integer Implements Iobject.id
    <XmlAttribute> Public Property type As String
    <XmlAttribute> Public Property pid As String
    <XmlAttribute> Public Property pindex As Integer

    Public Property components As component()
    Public Property mesh As mesh

End Class

Public Class basematerials
    Implements Iobject

    <XmlAttribute("id")>
    Public Property id As Integer Implements Iobject.id
    <XmlElement("base")>
    Public Property basematerials As base()

    Public Overrides Function ToString() As String
        Return basematerials.GetJson
    End Function
End Class
