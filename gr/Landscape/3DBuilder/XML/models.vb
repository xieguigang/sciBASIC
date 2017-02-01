#Region "Microsoft.VisualBasic::bddcbcd20a59ee8532b6cd01fee9c1a3, ..\sciBASIC#\gr\Landscape\3DBuilder\XML\models.vb"

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

Imports System.Drawing
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization.JSON

Public Class mesh

    Public Property vertices As Point3D()
    Public Property triangles As triangle()

    Public Function GetSurfaces(base As base) As Drawing3D.Surface()
        Dim out As New List(Of Drawing3D.Surface)
        Dim color As Color = base.displaycolor.TranslateColor
        Dim b As New SolidBrush(color)

        For Each t As triangle In triangles
            out += New Drawing3D.Surface With {
                .vertices = {
                    vertices(t.v1), vertices(t.v2), vertices(t.v3)
                },
                .brush = b
            }
        Next

        Return out
    End Function

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function
End Class

Public Class triangle

    <XmlAttribute> Public Property v1 As Integer
    <XmlAttribute> Public Property v2 As Integer
    <XmlAttribute> Public Property v3 As Integer

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function
End Class

Public Class component

    <XmlAttribute>
    Public Property objectid As Integer
End Class
