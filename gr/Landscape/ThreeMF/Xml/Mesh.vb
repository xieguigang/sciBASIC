#Region "Microsoft.VisualBasic::c516e4505c5030aff47eded0a671f974, gr\Landscape\ThreeMF\Xml\Mesh.vb"

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

    '   Total Lines: 88
    '    Code Lines: 55 (62.50%)
    ' Comment Lines: 16 (18.18%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 17 (19.32%)
    '     File Size: 2.66 KB


    '     Class Mesh
    ' 
    '         Properties: triangles, vertices
    ' 
    '         Function: (+2 Overloads) GetSurfaces, ToString
    ' 
    '     Class Triangle3D
    ' 
    '         Properties: p1, pid, v1, v2, v3
    ' 
    '         Function: ToString
    ' 
    '     Class Component
    ' 
    '         Properties: objectid
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace ThreeMF.Xml

    Public Class Mesh

        Public Property vertices As Point3D()
        Public Property triangles As Triangle3D()

        Public Function GetSurfaces(material As BaseMaterial) As Surface()
            Dim out As New List(Of Surface)
            Dim color As Color = material.displaycolor.TranslateColor
            Dim b As New SolidBrush(color)

            For Each t As Triangle3D In triangles
                out += New Surface With {
                    .vertices = {
                        vertices(t.v1), vertices(t.v2), vertices(t.v3)
                    },
                    .brush = b
                }
            Next

            Return out
        End Function

    ''' <summary>
    ''' <see cref="Triangle3D.p1"/>
    ''' </summary>
        ''' <param name="materials"></param>
        ''' <returns></returns>
        Public Function GetSurfaces(materials As Brush()) As Surface()
            Dim out As New List(Of Surface)

            For Each t As Triangle3D In triangles
                Dim i As Integer = CInt(t.p1)

                out += New Surface With {
                    .vertices = {
                        vertices(t.v1), vertices(t.v2), vertices(t.v3)
                    },
                    .brush = materials(i)
                }
            Next

            Return out
        End Function

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class

    ''' <summary>
    ''' 3 vertex index to create a triangle of the surface model data
    ''' </summary>
    Public Class Triangle3D

        <XmlAttribute> Public Property v1 As Integer
        <XmlAttribute> Public Property v2 As Integer
        <XmlAttribute> Public Property v3 As Integer

        ''' <summary>
        ''' 当前的这个三角形面的所属组件的编号
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property pid As String
        ''' <summary>
        ''' 颜色索引值
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property p1 As String

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class

    Public Class Component

        <XmlAttribute>
        Public Property objectid As Integer
    End Class
End Namespace
