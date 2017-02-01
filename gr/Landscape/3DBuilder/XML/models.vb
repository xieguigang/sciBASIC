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