#Region "Microsoft.VisualBasic::c8b1b248179ce7ad84f4abd4712b5ec4, gr\Landscape\3DBuilder\XML\models.vb"

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
    '     File Size: 2.65 KB


    '     Class mesh
    ' 
    '         Properties: triangles, vertices
    ' 
    '         Function: (+2 Overloads) GetSurfaces, ToString
    ' 
    '     Class triangle
    ' 
    '         Properties: p1, pid, v1, v2, v3
    ' 
    '         Function: ToString
    ' 
    '     Class component
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

Namespace Vendor_3mf.XML

    Public Class mesh

        Public Property vertices As Point3D()
        Public Property triangles As triangle()

        Public Function GetSurfaces(base As base) As Surface()
            Dim out As New List(Of Surface)
            Dim color As Color = base.displaycolor.TranslateColor
            Dim b As New SolidBrush(color)

            For Each t As triangle In triangles
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
        ''' <see cref="triangle.p1"/>
        ''' </summary>
        ''' <param name="materials"></param>
        ''' <returns></returns>
        Public Function GetSurfaces(materials As Brush()) As Surface()
            Dim out As New List(Of Surface)

            For Each t As triangle In triangles
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
    Public Class triangle

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

    Public Class component

        <XmlAttribute>
        Public Property objectid As Integer
    End Class
End Namespace
