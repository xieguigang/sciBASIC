#Region "Microsoft.VisualBasic::1594f9190feb57c3c42e9c67abd6fe25, gr\Landscape\3DBuilder\XML\XmlModel3D.vb"

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

    '   Total Lines: 74
    '    Code Lines: 54 (72.97%)
    ' Comment Lines: 6 (8.11%)
    '    - Xml Docs: 50.00%
    ' 
    '   Blank Lines: 14 (18.92%)
    '     File Size: 2.29 KB


    '     Class XmlModel3D
    ' 
    '         Properties: build, resources, unit
    ' 
    '         Function: GetSurfaces
    ' 
    '     Class build
    ' 
    '         Properties: items
    ' 
    '         Function: ToString
    ' 
    '     Class item
    ' 
    '         Properties: objectid, transform
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Vendor_3mf.XML

    ''' <summary>
    ''' ``3D/3dmodel.model`` xml file.
    ''' </summary>
    ''' 
    <XmlRoot("model")> Public Class XmlModel3D

        <XmlAttribute>
        Public Property unit As String
        Public Property resources As resources
        Public Property build As build

        Public Function GetSurfaces() As IEnumerable(Of Surface)
            Dim out As New List(Of Surface)
            Dim objects As [object]() = resources _
                .objects _
                .Where(AddressOf NotNull) _
                .ToArray
            Dim materials As Brush() = resources _
                .basematerials _
                .basematerials _
                .Select(Function(b) b.displaycolor.TranslateColor) _
                .Select(Function(c) New SolidBrush(c)) _
                .ToArray

            On Error Resume Next

            For Each obj As [object] In objects
                If obj.pindex Is Nothing Then
                    ' 使用三角面自己的资源编号
                    out += obj.mesh.GetSurfaces(materials)
                Else
                    ' 使用总编号
                    Dim base As base = resources _
                        .basematerials _
                        .basematerials(CInt(obj.pindex))

                    out += obj _
                        .mesh _
                        .GetSurfaces(base)
                End If
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
End Namespace
