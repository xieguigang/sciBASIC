#Region "Microsoft.VisualBasic::86694fa434d0dab3f8aa473a526431b1, ..\sciBASIC#\gr\Landscape\3DBuilder\XML\XmlModel3D.vb"

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
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON

''' <summary>
''' ``3D/3dmodel.model`` xml file.
''' </summary>
''' 
<XmlRoot("model")> Public Class XmlModel3D

    <XmlAttribute>
    Public Property unit As String
    Public Property resources As resources
    Public Property build As build

    Public Function GetSurfaces() As IEnumerable(Of Drawing3D.Surface)
        Dim out As New List(Of Drawing3D.Surface)

        On Error Resume Next

        For Each obj As SeqValue(Of [object]) In resources _
            .objects _
            .Where(AddressOf NotNull) _
            .SeqIterator

            Dim base As base = resources _
                .basematerials _
                .basematerials(obj)

            out += (+obj) _
                .mesh _
                .GetSurfaces(base)
        Next

        Return out
    End Function
End Class

Public Class build

    <XmlElement("item")> Public Property items As item()

    Public Overrides Function ToString() As String
        Return items.GetJson
    End Function
End Class

Public Class item

    <XmlAttribute> Public Property objectid As Integer
    <XmlAttribute> Public Property transform As Double()

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function
End Class
